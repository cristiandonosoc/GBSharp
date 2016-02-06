using System;
using System.Collections.Generic;
using System.Linq;
using GBSharp.Utils;
using GBSharp.CPUSpace.Dictionaries;
using GBSharp.MemorySpace;

namespace GBSharp.CPUSpace
{
  public enum BreakpointKinds
  {
    NONE,
    EXECUTION,
    READ,
    WRITE,
    JUMP
  }

  public class Breakpoint
  {
    public bool Valid { get; internal set; }
    public ushort Address { get; internal set; }
    public BreakpointKinds Kind { get; internal set; }
  }

  class CPU : ICPU
  {

    internal CPURegisters registers;
    internal MemorySpace.Memory memory;
    internal InterruptController interruptController;
    internal ushort nextPC;
    internal ushort clock; // 16 bit oscillation counter at 4194304 Hz (2^22).
    internal bool halted;
    internal bool haltLoad;
    internal bool stopped;
    internal bool interruptRequired;
    internal bool interruptInProgress;
    // Whether the next step should trigger an interrupt event
    internal Interrupts interruptToTrigger;
    internal Interrupts interruptTriggered;
    internal Dictionary<Interrupts, bool> _breakableInterrupts = new Dictionary<Interrupts, bool>()
    {
      {Interrupts.VerticalBlanking, false},
      {Interrupts.LCDCStatus, false},
      {Interrupts.TimerOverflow, false},
      {Interrupts.SerialIOTransferCompleted, false},
      {Interrupts.P10to13TerminalNegativeEdge, false}
    };

    // Timing

    private ushort _divCounter;
    private byte _timaCounter;
    private int _tacCounter;
    private int _tacMask;
    private byte _tmaValue;

    #region BREAKPOINTS

    public event Action BreakpointFound;
    public event Action<Interrupts> InterruptHappened;

    private List<ushort> _executionBreakpoints;
    internal List<ushort> ExecutionBreakpoints { get { return _executionBreakpoints; } }
    private List<ushort> _readBreakpoints;
    internal List<ushort> ReadBreakpoints { get { return _readBreakpoints; } }
    private List<ushort> _writeBreakpoints;
    internal List<ushort> WriteBreakpoints { get { return _writeBreakpoints; } }
    private List<ushort> _jumpBreakpoints;
    internal List<ushort> JumpBreakpoints { get { return _jumpBreakpoints; } }

    public List<ushort> GetBreakpoints(BreakpointKinds kind)
    {
      switch(kind)
      {
        case BreakpointKinds.EXECUTION:
          return _executionBreakpoints;
        case BreakpointKinds.READ:
          return _readBreakpoints;
        case BreakpointKinds.WRITE:
          return _writeBreakpoints;
        case BreakpointKinds.JUMP:
          return _jumpBreakpoints;
      }

      throw new InvalidProgramException("Wrong Breakpoint kind");
    }

    public void AddBreakpoint(BreakpointKinds kind, ushort address)
    {
      switch(kind)
      {
        case BreakpointKinds.EXECUTION:
          if (_executionBreakpoints.Contains(address)) { return; }
          _executionBreakpoints.Add(address);
          break;
        case BreakpointKinds.READ:
          if (_readBreakpoints.Contains(address)) { return; }
          _readBreakpoints.Add(address);
          break;
        case BreakpointKinds.WRITE:
          if (_writeBreakpoints.Contains(address)) { return; }
          _writeBreakpoints.Add(address);
          break;
        case BreakpointKinds.JUMP:
          if (_jumpBreakpoints.Contains(address)) { return; }
          _jumpBreakpoints.Add(address);
          break;
      }
    }

    public void RemoveBreakpoint(BreakpointKinds kind, ushort address)
    {
      switch(kind)
      {
        case BreakpointKinds.EXECUTION:
          _executionBreakpoints.Remove(address);
          break;
        case BreakpointKinds.READ:
          _readBreakpoints.Remove(address);
          break;
        case BreakpointKinds.WRITE:
          _writeBreakpoints.Remove(address);
          break;
        case BreakpointKinds.JUMP:
          _jumpBreakpoints.Remove(address);
          break;
      }
    }

    public Breakpoint CurrentBreakpoint { get; private set; }

    public void ResetBreakpoints()
    {
      CurrentBreakpoint = new Breakpoint();
      _executionBreakpoints = new List<ushort>();
      _readBreakpoints = new List<ushort>();
      _writeBreakpoints = new List<ushort>();
      _jumpBreakpoints = new List<ushort>();
    }

    #endregion

    // NOTE(Cristian): This Instruction instance is no longer used to communicate
    //                 the current state to the CPUView. This is because it's always
    //                 one instruction behing because the PC advances after the Step.
    //                 What it's done is that the current PC is decoded on-demand by the view.
    private Instruction _currentInstruction = null;
    internal Instruction InternalCurrentInstruction
    {
      get { return _currentInstruction; }
    }
    private Instruction _exportInstruction = null;
    /// <summary>
    /// The current operands (extra bytes) used by the current instruction
    /// running in the CPU. This is (for now) mainly used to display this
    /// information in the CPU view
    /// NOTE is nullable so we can signify less operands
    /// </summary>
    public IInstruction CurrentInstruction
    {
      get
      {
        FetchAndDecode(ref _exportInstruction, registers.PC);
        return _exportInstruction;
      }
    }

    // Interrupt starting addresses
    Dictionary<Interrupts, ushort> interruptHandlers = new Dictionary<Interrupts, ushort>()
    {
      {Interrupts.VerticalBlanking, 0x0040},
      {Interrupts.LCDCStatus, 0x0048},
      {Interrupts.TimerOverflow, 0x0050},
      {Interrupts.SerialIOTransferCompleted, 0x0058},
      {Interrupts.P10to13TerminalNegativeEdge, 0x0060}
    };

    CPURegisters ICPU.Registers
    {
      get
      {
        return this.registers;
      }
    }

    public bool InterruptMasterEnable
    {
      get { return interruptController.InterruptMasterEnable; }
    }

    public ushort[] InstructionHistogram
    {
      get { return instructionHistogram; }
    }

    public ushort[] CbInstructionHistogram
    {
      get { return cbInstructionHistogram; }
    }

    internal ushort[] instructionHistogram = new ushort[256];
    internal ushort[] cbInstructionHistogram = new ushort[256];

    public CPU(MemorySpace.Memory memory)
    {
      this.memory = memory;
      this.interruptController = new InterruptController(this.memory);

      // Initialize registers 
      Reset();
      ResetBreakpoints();
    }

    /// <summary>
    /// Sets the initial values for the cpu registers and memory mapped registers.
    /// This is the equivalent to run the BIOS rom.
    /// </summary>
    internal void Reset()
    {
      // Reset the clock state
      this.clock = 0;


      this.halted = false;
      this.haltLoad = false;
      this.stopped = false;

      // Magic CPU initial values (after bios execution).
      this.registers.A = 1;
      this.registers.BC = 0x0013;
      this.registers.DE = 0x00D8;
      this.registers.HL = 0x014D;
      this.registers.PC = 0x0100;
      this.registers.SP = 0xFFFE;

      // We restart the timer
      _divCounter = 0xABFF;
      _timaCounter = 0;
      _tacCounter = 0;
      _tacMask = 0;
      _tmaValue = 0;

      // Initialize memory mapped registers
      this.memory.LowLevelWrite(0xFF04, 0xAB); // DIV
      this.memory.LowLevelWrite(0xFF05, 0x00); // TIMA
      this.memory.LowLevelWrite(0xFF06, 0x00); // TMA
      this.memory.LowLevelWrite(0xFF07, 0x00); // TAC
      this.memory.LowLevelWrite(0xFF10, 0x80); // NR10
      this.memory.LowLevelWrite(0xFF11, 0xBF); // NR11
      this.memory.LowLevelWrite(0xFF12, 0xF3); // NR12
      this.memory.LowLevelWrite(0xFF14, 0xBF); // NR14
      this.memory.LowLevelWrite(0xFF16, 0x3F); // NR21
      this.memory.LowLevelWrite(0xFF17, 0x00); // NR22
      this.memory.LowLevelWrite(0xFF19, 0xBF); // NR24
      this.memory.LowLevelWrite(0xFF1A, 0x7F); // NR30
      this.memory.LowLevelWrite(0xFF1B, 0xFF); // NR31
      this.memory.LowLevelWrite(0xFF1C, 0x9F); // NR32
      this.memory.LowLevelWrite(0xFF1E, 0xBF); // NR33
      this.memory.LowLevelWrite(0xFF20, 0xFF); // NR41
      this.memory.LowLevelWrite(0xFF21, 0x00); // NR42
      this.memory.LowLevelWrite(0xFF22, 0x00); // NR43
      this.memory.LowLevelWrite(0xFF23, 0xBF); // NR30
      this.memory.LowLevelWrite(0xFF24, 0x77); // NR50
      this.memory.LowLevelWrite(0xFF25, 0xF3); // NR51
      this.memory.LowLevelWrite(0xFF26, 0xF1); // NR52 GB: 0xF1, SGB: 0xF0
      this.memory.LowLevelWrite(0xFF40, 0x91); // LCDC
      this.memory.LowLevelWrite(0xFF42, 0x00); // SCY
      this.memory.LowLevelWrite(0xFF43, 0x00); // SCX
      this.memory.LowLevelWrite(0xFF45, 0x00); // LYC
      this.memory.LowLevelWrite(0xFF47, 0xFC); // BGP
      this.memory.LowLevelWrite(0xFF48, 0xFF); // OBP0
      this.memory.LowLevelWrite(0xFF49, 0xFF); // OBP1
      this.memory.LowLevelWrite(0xFF4A, 0x00); // WY
      this.memory.LowLevelWrite(0xFF4B, 0x00); // WX
      this.memory.LowLevelWrite(0xFFFF, 0x00); // IE

      _currentInstruction = new Instruction();
      _exportInstruction = new Instruction();
    }

    public void ResetInstructionHistograms()
    {
      instructionHistogram = new ushort[256];
      cbInstructionHistogram = new ushort[256];
    }

    public void SetInterruptBreakable(Interrupts interrupt, bool isBreakable)
    {
      _breakableInterrupts[interrupt] = isBreakable;
    }

    /// <summary>
    /// Executes one instruction, requiring arbitrary machine and clock cycles.
    /// </summary>
    /// <param name="ignoreBreakpoints">If this step should check for breakpoints</param>
    /// <returns>The number of ticks that were required at the base clock frequency (2^22hz, ~4Mhz).
    /// This can be 0 in STOP mode or even 24 for CALL Z, nn and other long CALL instructions.</returns>
    public byte Step(bool ignoreBreakpoints)
    {
      if (stopped) { return 0; }
      if (halted) { return 0; }

      if (interruptRequired)
      {
        // NOTE(Cristian): We store the interrupt so we break on the next
        //                 step. This will enable that we're breaking on the
        //                 first instruction of the interrupt handler, vs
        //                 an invented CALL
        interruptInProgress = true;
        interruptTriggered = interruptToTrigger;
        _currentInstruction = InterruptHandler(interruptToTrigger);

        // We need to check if there is another interrupt waiting
        CheckForInterruptRequired();
      }
      else
      {
        // If we have set an interupt to trigger a breakpoint, we break
        if (interruptInProgress)
        {
          // TODO(Cristian): Change this to an array!
          if (_breakableInterrupts[interruptTriggered])
          {
            InterruptHappened(interruptTriggered);
            return 0; // We don't advance the state because we're breaking
          }

          interruptInProgress = false;
        }

        // Otherwise we fetch as usual
        FetchAndDecode(ref _currentInstruction, this.registers.PC, haltLoad);
        haltLoad = false;
      }

      // We see if there is an breakpoint to this address
      if(!ignoreBreakpoints && _executionBreakpoints.Contains(_currentInstruction.Address))
      {
        CurrentBreakpoint.Address = _currentInstruction.Address;
        CurrentBreakpoint.Kind = BreakpointKinds.EXECUTION;
        BreakpointFound();
        return 0;
      }

      // Prepare for program counter movement, but wait for instruction execution.
      // Overwrite nextPC in the instruction lambdas if you want to implement jumps.
      // NOTE(Cristian): If we don't differentiate this case, the CALL instruction of the
      //                 interrupt will be added to nextPC, which will in turn be written
      //                 into the stack. This means that when we RET, we would have jumped
      //                 into the address we *should* have jumped plus some meaningless offset!
      if (!interruptInProgress)
      {
        this.nextPC = (ushort)(this.registers.PC + _currentInstruction.Length);
      }

      // Execute instruction
      // NOTE(Cristian): This lambda could modify some fields of _currentInstruction
      //                 Most notably, change the ticks in the case of conditional jumps

      BreakpointKinds breakpointKind = BreakpointKinds.NONE;
      if(!_currentInstruction.CB)
      {
        breakpointKind = CPUInstructionsBreakpoints.Check(this,
                                                          (byte)_currentInstruction.OpCode, 
                                                          _currentInstruction.Literal,
                                                          ignoreBreakpoints);
        CPUInstructions.RunInstruction(this, 
                                       (byte)_currentInstruction.OpCode, 
                                       _currentInstruction.Literal);
      }
      else
      {
        breakpointKind = CPUCBInstructionsBreakpoints.Check(this,
                                                            (byte)_currentInstruction.OpCode,  
                                                            _currentInstruction.Literal, 
                                                            ignoreBreakpoints);
        RunCBInstruction((byte)_currentInstruction.OpCode,
                         _currentInstruction.Literal);
      }

      // We see if there is an breakpoint to this address
      // NOTE(Cristian): ignoreBreakpoints is implicit in the RunInstruction
      if(breakpointKind != BreakpointKinds.NONE)
      {
        CurrentBreakpoint.Address = _currentInstruction.Address;
        CurrentBreakpoint.Kind = breakpointKind;
        BreakpointFound();
        return 0;
      }



      // We check if the instruction triggered a breakpoint

      // Push the next program counter value into the real program counter!
      this.registers.PC = this.nextPC;

      // We return the ticks that the instruction took
      return _currentInstruction.Ticks;
    }

    private Instruction InterruptHandler(Interrupts interrupt)
    {
      // Handle interrupt with a CALL instruction to the interrupt handler
      Instruction instruction = new Instruction();
      instruction.OpCode = 0xCD; // CALL!
      byte lowOpcode = (byte)instruction.OpCode;
      instruction.Length = CPUInstructionLengths.Get(lowOpcode);
      instruction.Literal = this.interruptHandlers[(Interrupts)interrupt];
      instruction.Ticks = CPUInstructionClocks.Get(lowOpcode);
      instruction.Name = CPUOpcodeNames.Get(lowOpcode);
      instruction.Description = CPUInstructionDescriptions.Get(lowOpcode);

      // Disable interrupts during interrupt handling and clear the current one
      this.interruptController.InterruptMasterEnable = false;
      byte IF = this.memory.LowLevelRead((ushort)MMR.IF);
      IF &= (byte)~(byte)interrupt;
      this.memory.LowLevelWrite((ushort)MMR.IF, IF);
      return instruction;
    }

    /// <summary>
    /// Fetches and Decodes an instruction
    /// </summary>
    /// <param name="instruction">
    /// As FetchAndDecode gets called *several* times a frame (and many times during
    /// disassembly), it is better to have a pre-allocated instruction and to replace
    /// the values, instead of getting the overhead of allocating a new Instrucion
    /// everytime.
    /// </param>
    /// <param name="instructionAddress"></param>
    /// <param name="haltLoad"></param>
    /// <returns></returns>
    internal Instruction FetchAndDecode(ref Instruction instruction, 
                                        ushort instructionAddress, bool haltLoad = false)
    {
      instruction.Address = instructionAddress;
      byte opcode = this.memory.LowLevelRead(instructionAddress);
      instruction.OpCode = opcode;

      if (instruction.OpCode != 0xCB)
      {
        instruction.CB = false;
        byte lowOpcode = (byte)instruction.OpCode;
        if(instructionHistogram[lowOpcode] < ushort.MaxValue)
          instructionHistogram[lowOpcode]++;
        // Normal instructions
        instruction.Length = CPUInstructionLengths.Get(lowOpcode);

        // Extract literal
        if (instruction.Length == 2)
        {
          // 8 bit literal
          instruction.Operands[0] = this.memory.LowLevelRead((ushort)(instructionAddress + 1));
          if(haltLoad)
          {
            instruction.Operands[0] = opcode;
          }
          instruction.Literal = (byte)instruction.Operands[0];
        }
        else if (instruction.Length == 3)
        {
          // 16 bit literal, little endian
          instruction.Operands[0] = this.memory.LowLevelRead((ushort)(instructionAddress + 2));
          instruction.Operands[1] = this.memory.LowLevelRead((ushort)(instructionAddress + 1));

          if(haltLoad)
          {
            instruction.Operands[1] = instruction.Operands[0];
            instruction.Operands[0] = opcode;
          }

          instruction.Literal = (byte)instruction.Operands[1];
          instruction.Literal += (ushort)(instruction.Operands[0] << 8);
        }

        instruction.Ticks = CPUInstructionClocks.Get(lowOpcode);
        instruction.Name = CPUOpcodeNames.Get(lowOpcode);
        instruction.Description = CPUInstructionDescriptions.Get(lowOpcode);
      }
      else
      {
        instruction.CB = true;
        // CB instructions block
        instruction.OpCode <<= 8;
        if (!haltLoad)
        {
          instruction.OpCode += this.memory.LowLevelRead((ushort)(instructionAddress + 1));
        }
        else
        {
          instruction.OpCode += 0xCB; // The first byte is duplicated
        }

        byte lowOpcode = (byte)instruction.OpCode;

        if (cbInstructionHistogram[lowOpcode] < ushort.MaxValue)
        {
          cbInstructionHistogram[lowOpcode]++;
        }
        instruction.Length = CPUCBInstructionLengths.Get(lowOpcode);
        // There is no literal in CB instructions!

        //instruction.Lambda = this.CBInstructionLambdas[lowOpcode];
        instruction.Ticks = CPUCBInstructionClocks.Get(lowOpcode);
        instruction.Name = CPUCBOpcodeNames.Get(lowOpcode);
        instruction.Description = CPUCBInstructionDescriptions.Get(lowOpcode);
      }

      // NOTE(Cristian): On haltLoad (HALT with IME disabled), the next byte after the HALT opcode
      //                 is "duplicated". This is a hardware bug.
      if(haltLoad)
      {
        instruction.Length--;
      }

      return instruction;
    }

    /// <summary>
    /// Returns the address of the interrupt handler 
    /// </summary>
    /// <returns></returns>
    internal void CheckForInterruptRequired()
    {
      interruptRequired = false;

      // Read interrupt flags
      int interruptRequest = this.memory.LowLevelRead((ushort)MMR.IF);
      // Mask enabled interrupts
      int interruptEnable = this.memory.LowLevelRead((ushort)MMR.IE);

      int interrupt = interruptEnable & interruptRequest;
      interrupt &= 0x1F; // 0x1F masks the useful bits of IE and IF, there is only 5 interrupts.

      // We check if no interrupt have happeded, or are disabled, who cares
      if (interrupt == 0x00)  { return; }

      // Interrupts unhaltd the CPU *even* if the IME is disabled
      this.halted = false;

      if (this.interruptController.InterruptMasterEnable)
      {
        // There is an interrupt waiting
        interruptRequired = true;

        // Ok, find the interrupt with the highest priority, check the first bit set
        interrupt &= -interrupt; // Magics ;)

        // Return the first interrupt
        interruptToTrigger = (Interrupts)(interrupt & 0x1F);
      }
    }

    /// <summary>
    /// Adjusts the this.clock counter to the right value after an instruction execution and updates the status of
    /// the code-controlled timer system TIMA/TMA/TAC, setting the right flags if an overflow interrupt is required.
    /// Updates the interrupt controller and the memory mapped registers related to timing.
    /// </summary>
    /// <param name="ticks">Base number of clock cycles required by the executed instruction.
    /// This number is obtained from the instruction clocks dictionaries and does not include additional time
    /// required for conditional CALL or JUMP instructions that is directly added into this.clock counter.</param>
    /// <returns>The total real number of clock oscillations at 4194304 Hz that occurred during a Step execution.
    /// This value includes the changes in the clock made directly by some instructions (conditionals) and the base
    /// execution times obtained from CPUInstructionClocks and CPUCBInstructionClocks dictionaries.</returns>
    internal void UpdateClockAndTimers(byte ticks)
    {
      // Update clock adding only base ticks. 
      // NOTE(Cristian): Conditional instructions times are already added at this point.
      //                 The instruction modifies the currentInstruction ticks
      this.clock += ticks;

      // Upper 8 bits of the clock should be accessible through DIV register.
      _divCounter += ticks;
      this.memory.LowLevelWrite((ushort)MMR.DIV, (byte)(_divCounter >> 8));

      // Configurable timer TIMA/TMA/TAC system:
      byte TAC = this.memory.LowLevelRead((ushort)MMR.TAC);
      bool runTimer = (TAC & 0x04) == 0x04; // Run timer is the bit 2 of the TAC register
      if (runTimer)
      {
        // Simulate every tick that occurred during the execution of the instruction
        for (int i = 1; i <= ticks; ++i)
        {
          ++_tacCounter;
          // Maybe there is a faster way to do this without checking every clock value (??)
          if ((_tacCounter & _tacMask) == 0x0000)
          {
            ++_timaCounter;

            // If overflow, we trigger the event
            if (_timaCounter == 0x0000)
            {
              _timaCounter = _tmaValue;
              this.interruptController.SetInterrupt(Interrupts.TimerOverflow);
            }

            // Update memory mapped timer
            this.memory.LowLevelWrite((ushort)MemorySpace.MMR.TIMA, _timaCounter);
          }
        }
      }
    }

    internal void HandleMemoryChange(MMR register, byte value)
    {
      switch (register)
      {
        case MMR.TIMA:
          _timaCounter = value;
          this.memory.LowLevelWrite((ushort)MMR.TIMA, value);
          break;
        case MMR.TMA:
          _tmaValue = value;
          this.memory.LowLevelWrite((ushort)MMR.TMA, value);
          break;
        case MMR.TAC:
          // Clock select is the bits 0 and 1 of the TAC register
          byte clockSelect = (byte)(value & 0x03); 
          switch (clockSelect)
          {
            case 1:
              _tacMask = 0x000F; // f/2^4  0000 0000 0000 1111, (262144 Hz)
              break;
            case 2:
              _tacMask = 0x003F; // f/2^6  0000 0000 0011 1111, (65536 Hz)
              break;
            case 3:
              _tacMask = 0x00FF; // f/2^8  0000 0000 1111 1111, (16384 Hz)
              break;
            default:
              _tacMask = 0x03FF; // f/2^10 0000 0011 1111 1111, (4096 Hz)
              break;
          }
          // We restart the counter
          _tacCounter = 0;
          // TAC has a 0xF8 mask (only lower 3 bits are useful)
          this.memory.LowLevelWrite((ushort)MMR.TAC, (byte)(0xF8 | value));
          break;
      }
    }

    public override string ToString()
    {
      return registers.ToString();
    }

    #region Instruction Lambdas

    /// <summary>
    /// Runs an CB opcode instruction
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    private void RunCBInstruction(byte opcode, ushort n)
    {
      switch (opcode)
      {
        // RLC B: Rotate B left with carry
        case 0x00:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.B);
            registers.B = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC C: Rotate C left with carry
        case 0x01:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.C);
            registers.C = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC D: Rotate D left with carry
        case 0x02:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.D);
            registers.D = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC E: Rotate E left with carry
        case 0x03:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.E);
            registers.E = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC H: Rotate H left with carry
        case 0x04:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.H);
            registers.H = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC L: Rotate L left with carry
        case 0x05:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.L);
            registers.L = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC (HL): Rotate value pointed by HL left with carry
        case 0x06:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(memory.Read(registers.HL));
            memory.Write(registers.HL, rotateCarry.Item1);

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RLC A: Rotate A left with carry
        case 0x07:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.A);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC B: Rotate B right with carry
        case 0x08:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.B);
            registers.B = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC C: Rotate C right with carry
        case 0x09:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.C);
            registers.C = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC D: Rotate D right with carry
        case 0x0A:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.D);
            registers.D = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC E: Rotate E right with carry
        case 0x0B:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.E);
            registers.E = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC H: Rotate H right with carry
        case 0x0C:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.H);
            registers.H = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC L: Rotate L right with carry
        case 0x0D:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.L);
            registers.L = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC (HL): Rotate value pointed by HL right with carry
        case 0x0E:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(memory.Read(registers.HL));
            memory.Write(registers.HL, rotateCarry.Item1);

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RRC A: Rotate A right with carry
        case 0x0F:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.A);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL B: Rotate B left
        case 0x10:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.B, 1, registers.FC);
            registers.B = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL C: Rotate C left
        case 0x11:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.C, 1, registers.FC);
            registers.C = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL D: Rotate D left
        case 0x12:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.D, 1, registers.FC);
            registers.D = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL E: Rotate E left
        case 0x13:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.E, 1, registers.FC);
            registers.E = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL H: Rotate H left
        case 0x14:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.H, 1, registers.FC);
            registers.H = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL L: Rotate L left
        case 0x15:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.L, 1, registers.FC);
            registers.L = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL (HL): Rotate value pointed by HL left
        case 0x16:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(memory.Read(registers.HL), 1, registers.FC);
            memory.Write(registers.HL, rotateCarry.Item1);

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RL A: Rotate A left
        case 0x17:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.A, 1, registers.FC);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR B: Rotate B right
        case 0x18:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.B, 1, registers.FC);
            registers.B = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR C: Rotate C right
        case 0x19:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.C, 1, registers.FC);
            registers.C = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR D: Rotate D right
        case 0x1A:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.D, 1, registers.FC);
            registers.D = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR E: Rotate E right
        case 0x1B:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.E, 1, registers.FC);
            registers.E = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR H: Rotate H right
        case 0x1C:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.H, 1, registers.FC);
            registers.H = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR L: Rotate L right
        case 0x1D:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.L, 1, registers.FC);
            registers.L = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR (HL): Rotate value pointed by HL right
        case 0x1E:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(memory.Read(registers.HL), 1, registers.FC);
            memory.Write(registers.HL, rotateCarry.Item1);

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // RR A: Rotate A right
        case 0x1F:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.A, 1, registers.FC);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA B: Shift B left preserving sign
        case 0x20:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.B);
            registers.B = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA C: Shift C left preserving sign
        case 0x21:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.C);
            registers.C = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA D: Shift D left preserving sign
        case 0x22:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.D);
            registers.D = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA E: Shift E left preserving sign
        case 0x23:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.E);
            registers.E = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA H: Shift H left preserving sign
        case 0x24:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.H);
            registers.H = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA L: Shift L left preserving sign
        case 0x25:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.L);
            registers.L = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA (HL): Shift value pointed by HL left preserving sign
        case 0x26:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(memory.Read(registers.HL));
            memory.Write(registers.HL, shiftCarry.Item1);

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SLA A: Shift A left preserving sign
        case 0x27:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(registers.A);
            registers.A = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
          }
          break;
        // SRA B: Shift B right preserving sign
        case 0x28:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.B);
            registers.B = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA C: Shift C right preserving sign
        case 0x29:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.C);
            registers.C = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA D: Shift D right preserving sign
        case 0x2A:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.D);
            registers.D = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA E: Shift E right preserving sign
        case 0x2B:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.E);
            registers.E = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA H: Shift H right preserving sign
        case 0x2C:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.H);
            registers.H = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA L: Shift L right preserving sign
        case 0x2D:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.L);
            registers.L = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA (HL): Shift value pointed by HL right preserving sign
        case 0x2E:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(memory.Read(registers.HL));
            memory.Write(registers.HL, shiftCarry.Item1);

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRA A: Shift A right preserving sign
        case 0x2F:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.A);
            registers.A = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SWAP B: Swap nybbles in B
        case 0x30:
          {
            byte result = UtilFuncs.SwapNibbles(registers.B);
            registers.B = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP C: Swap nybbles in C
        case 0x31:
          {
            byte result = UtilFuncs.SwapNibbles(registers.C);
            registers.C = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP D: Swap nybbles in D
        case 0x32:
          {
            byte result = UtilFuncs.SwapNibbles(registers.D);
            registers.D = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP E: Swap nybbles in E
        case 0x33:
          {
            byte result = UtilFuncs.SwapNibbles(registers.E);
            registers.E = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP H: Swap nybbles in H
        case 0x34:
          {
            byte result = UtilFuncs.SwapNibbles(registers.H);
            registers.H = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP L: Swap nybbles in L
        case 0x35:
          {
            byte result = UtilFuncs.SwapNibbles(registers.L);
            registers.L = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP (HL): Swap nybbles in value pointed by HL
        case 0x36:
          {
            byte result = UtilFuncs.SwapNibbles(memory.Read(registers.HL));
            memory.Write(registers.HL, result);

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SWAP A: Swap nybbles in A
        case 0x37:
          {
            byte result = UtilFuncs.SwapNibbles(registers.A);
            registers.A = result;

            registers.FZ = (byte)(result == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = 0;
            break;
          }

        // SRL B: Shift B right
        case 0x38:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.B);
            registers.B = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL C: Shift C right
        case 0x39:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.C);
            registers.C = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL D: Shift D right
        case 0x3A:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.D);
            registers.D = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL E: Shift E right
        case 0x3B:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.E);
            registers.E = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL H: Shift H right
        case 0x3C:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.H);
            registers.H = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL L: Shift L right
        case 0x3D:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.L);
            registers.L = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL (HL): Shift value pointed by HL right
        case 0x3E:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(memory.Read(registers.HL));
            memory.Write(registers.HL, shiftCarry.Item1);

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // SRL A: Shift A right
        case 0x3F:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(registers.A);
            registers.A = shiftCarry.Item1;

            registers.FC = shiftCarry.Item2;
            registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // BIT 0,B: Test bit 0 of B
        case 0x40:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,C: Test bit 0 of C
        case 0x41:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,D: Test bit 0 of D
        case 0x42:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,E: Test bit 0 of E
        case 0x43:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,H: Test bit 0 of H
        case 0x44:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,L: Test bit 0 of L
        case 0x45:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,(HL): Test bit 0 of value pointed by HL
        case 0x46:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 0,A: Test bit 0 of A
        case 0x47:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 0) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,B: Test bit 1 of B
        case 0x48:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,C: Test bit 1 of C
        case 0x49:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,D: Test bit 1 of D
        case 0x4A:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,E: Test bit 1 of E
        case 0x4B:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,H: Test bit 1 of H
        case 0x4C:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,L: Test bit 1 of L
        case 0x4D:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,(HL): Test bit 1 of value pointed by HL
        case 0x4E:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 1,A: Test bit 1 of A
        case 0x4F:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 1) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,B: Test bit 2 of B
        case 0x50:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,C: Test bit 2 of C
        case 0x51:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,D: Test bit 2 of D
        case 0x52:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,E: Test bit 2 of E
        case 0x53:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,H: Test bit 2 of H
        case 0x54:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,L: Test bit 2 of L
        case 0x55:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,(HL): Test bit 2 of value pointed by HL
        case 0x56:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 2,A: Test bit 2 of A
        case 0x57:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 2) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,B: Test bit 3 of B
        case 0x58:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,C: Test bit 3 of C
        case 0x59:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,D: Test bit 3 of D
        case 0x5A:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,E: Test bit 3 of E
        case 0x5B:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,H: Test bit 3 of H
        case 0x5C:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,L: Test bit 3 of L
        case 0x5D:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,(HL): Test bit 3 of value pointed by HL
        case 0x5E:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 3,A: Test bit 3 of A
        case 0x5F:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 3) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,B: Test bit 4 of B
        case 0x60:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,C: Test bit 4 of C
        case 0x61:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,D: Test bit 4 of D
        case 0x62:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,E: Test bit 4 of E
        case 0x63:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,H: Test bit 4 of H
        case 0x64:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,L: Test bit 4 of L
        case 0x65:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,(HL): Test bit 4 of value pointed by HL
        case 0x66:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 4,A: Test bit 4 of A
        case 0x67:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 4) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,B: Test bit 5 of B
        case 0x68:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,C: Test bit 5 of C
        case 0x69:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,D: Test bit 5 of D
        case 0x6A:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,E: Test bit 5 of E
        case 0x6B:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,H: Test bit 5 of H
        case 0x6C:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,L: Test bit 5 of L
        case 0x6D:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,(HL): Test bit 5 of value pointed by HL
        case 0x6E:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 5,A: Test bit 5 of A
        case 0x6F:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 5) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,B: Test bit 6 of B
        case 0x70:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,C: Test bit 6 of C
        case 0x71:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,D: Test bit 6 of D
        case 0x72:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,E: Test bit 6 of E
        case 0x73:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,H: Test bit 6 of H
        case 0x74:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,L: Test bit 6 of L
        case 0x75:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,(HL): Test bit 6 of value pointed by HL
        case 0x76:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 6,A: Test bit 6 of A
        case 0x77:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 6) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,B: Test bit 7 of B
        case 0x78:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,C: Test bit 7 of C
        case 0x79:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,D: Test bit 7 of D
        case 0x7A:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,E: Test bit 7 of E
        case 0x7B:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,H: Test bit 7 of H
        case 0x7C:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,L: Test bit 7 of L
        case 0x7D:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,(HL): Test bit 7 of value pointed by HL
        case 0x7E:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // BIT 7,A: Test bit 7 of A
        case 0x7F:
          {
            registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 7) == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = 1;
            break;
          }

        // RES 0,B: Clear (reset) bit 0 of B
        case 0x80:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 0);
            break;
          }

        // RES 0,C: Clear (reset) bit 0 of C
        case 0x81:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 0);
            break;
          }

        // RES 0,D: Clear (reset) bit 0 of D
        case 0x82:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 0);
            break;
          }

        // RES 0,E: Clear (reset) bit 0 of E
        case 0x83:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 0);
            break;
          }

        // RES 0,H: Clear (reset) bit 0 of H
        case 0x84:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 0);
            break;
          }

        // RES 0,L: Clear (reset) bit 0 of L
        case 0x85:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 0);
            break;
          }

        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        case 0x86:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 0));
            break;
          }

        // RES 0,A: Clear (reset) bit 0 of A
        case 0x87:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 0);
            break;
          }

        // RES 1,B: Clear (reset) bit 1 of B
        case 0x88:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 1);
            break;
          }

        // RES 1,C: Clear (reset) bit 1 of C
        case 0x89:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 1);
            break;
          }

        // RES 1,D: Clear (reset) bit 1 of D
        case 0x8A:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 1);
            break;
          }

        // RES 1,E: Clear (reset) bit 1 of E
        case 0x8B:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 1);
            break;
          }

        // RES 1,H: Clear (reset) bit 1 of H
        case 0x8C:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 1);
            break;
          }

        // RES 1,L: Clear (reset) bit 1 of L
        case 0x8D:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 1);
            break;
          }

        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        case 0x8E:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 1));
            break;
          }

        // RES 1,A: Clear (reset) bit 1 of A
        case 0x8F:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 1);
            break;
          }

        // RES 2,B: Clear (reset) bit 2 of B
        case 0x90:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 2);
            break;
          }

        // RES 2,C: Clear (reset) bit 2 of C
        case 0x91:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 2);
            break;
          }

        // RES 2,D: Clear (reset) bit 2 of D
        case 0x92:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 2);
            break;
          }

        // RES 2,E: Clear (reset) bit 2 of E
        case 0x93:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 2);
            break;
          }

        // RES 2,H: Clear (reset) bit 2 of H
        case 0x94:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 2);
            break;
          }

        // RES 2,L: Clear (reset) bit 2 of L
        case 0x95:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 2);
            break;
          }

        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        case 0x96:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 2));
            break;
          }

        // RES 2,A: Clear (reset) bit 2 of A
        case 0x97:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 2);
            break;
          }

        // RES 3,B: Clear (reset) bit 3 of B
        case 0x98:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 3);
            break;
          }

        // RES 3,C: Clear (reset) bit 3 of C
        case 0x99:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 3);
            break;
          }

        // RES 3,D: Clear (reset) bit 3 of D
        case 0x9A:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 3);
            break;
          }

        // RES 3,E: Clear (reset) bit 3 of E
        case 0x9B:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 3);
            break;
          }

        // RES 3,H: Clear (reset) bit 3 of H
        case 0x9C:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 3);
            break;
          }

        // RES 3,L: Clear (reset) bit 3 of L
        case 0x9D:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 3);
            break;
          }

        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        case 0x9E:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 3));
            break;
          }

        // RES 3,A: Clear (reset) bit 3 of A
        case 0x9F:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 3);
            break;
          }

        // RES 4,B: Clear (reset) bit 4 of B
        case 0xA0:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 4);
            break;
          }

        // RES 4,C: Clear (reset) bit 4 of C
        case 0xA1:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 4);
            break;
          }

        // RES 4,D: Clear (reset) bit 4 of D
        case 0xA2:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 4);
            break;
          }

        // RES 4,E: Clear (reset) bit 4 of E
        case 0xA3:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 4);
            break;
          }

        // RES 4,H: Clear (reset) bit 4 of H
        case 0xA4:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 4);
            break;
          }

        // RES 4,L: Clear (reset) bit 4 of L
        case 0xA5:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 4);
            break;
          }

        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        case 0xA6:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 4));
            break;
          }

        // RES 4,A: Clear (reset) bit 4 of A
        case 0xA7:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 4);
            break;
          }

        // RES 5,B: Clear (reset) bit 5 of B
        case 0xA8:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 5);
            break;
          }

        // RES 5,C: Clear (reset) bit 5 of C
        case 0xA9:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 5);
            break;
          }

        // RES 5,D: Clear (reset) bit 5 of D
        case 0xAA:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 5);
            break;
          }

        // RES 5,E: Clear (reset) bit 5 of E
        case 0xAB:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 5);
            break;
          }

        // RES 5,H: Clear (reset) bit 5 of H
        case 0xAC:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 5);
            break;
          }

        // RES 5,L: Clear (reset) bit 5 of L
        case 0xAD:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 5);
            break;
          }

        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        case 0xAE:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 5));
            break;
          }

        // RES 5,A: Clear (reset) bit 5 of A
        case 0xAF:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 5);
            break;
          }

        // RES 6,B: Clear (reset) bit 6 of B
        case 0xB0:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 6);
            break;
          }

        // RES 6,C: Clear (reset) bit 6 of C
        case 0xB1:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 6);
            break;
          }

        // RES 6,D: Clear (reset) bit 6 of D
        case 0xB2:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 6);
            break;
          }

        // RES 6,E: Clear (reset) bit 6 of E
        case 0xB3:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 6);
            break;
          }

        // RES 6,H: Clear (reset) bit 6 of H
        case 0xB4:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 6);
            break;
          }

        // RES 6,L: Clear (reset) bit 6 of L
        case 0xB5:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 6);
            break;
          }

        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        case 0xB6:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 6));
            break;
          }

        // RES 6,A: Clear (reset) bit 6 of A
        case 0xB7:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 6);
            break;
          }

        // RES 7,B: Clear (reset) bit 7 of B
        case 0xB8:
          {
            registers.B = UtilFuncs.ClearBit(registers.B, 7);
            break;
          }

        // RES 7,C: Clear (reset) bit 7 of C
        case 0xB9:
          {
            registers.C = UtilFuncs.ClearBit(registers.C, 7);
            break;
          }

        // RES 7,D: Clear (reset) bit 7 of D
        case 0xBA:
          {
            registers.D = UtilFuncs.ClearBit(registers.D, 7);
            break;
          }

        // RES 7,E: Clear (reset) bit 7 of E
        case 0xBB:
          {
            registers.E = UtilFuncs.ClearBit(registers.E, 7);
            break;
          }

        // RES 7,H: Clear (reset) bit 7 of H
        case 0xBC:
          {
            registers.H = UtilFuncs.ClearBit(registers.H, 7);
            break;
          }

        // RES 7,L: Clear (reset) bit 7 of L
        case 0xBD:
          {
            registers.L = UtilFuncs.ClearBit(registers.L, 7);
            break;
          }

        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        case 0xBE:
          {
            memory.Write(registers.HL, UtilFuncs.ClearBit(memory.Read(registers.HL), 7));
            break;
          }

        // RES 7,A: Clear (reset) bit 7 of A
        case 0xBF:
          {
            registers.A = UtilFuncs.ClearBit(registers.A, 7);
            break;
          }

        // SET 0,B: Set bit 0 of B
        case 0xC0:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 0);
            break;
          }

        // SET 0,C: Set bit 0 of C
        case 0xC1:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 0);
            break;
          }

        // SET 0,D: Set bit 0 of D
        case 0xC2:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 0);
            break;
          }

        // SET 0,E: Set bit 0 of E
        case 0xC3:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 0);
            break;
          }

        // SET 0,H: Set bit 0 of H
        case 0xC4:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 0);
            break;
          }

        // SET 0,L: Set bit 0 of L
        case 0xC5:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 0);
            break;
          }

        // SET 0,(HL): Set bit 0 of value pointed by HL
        case 0xC6:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 0));
            break;
          }

        // SET 0,A: Set bit 0 of A
        case 0xC7:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 0);
            break;
          }

        // SET 1,B: Set bit 1 of B
        case 0xC8:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 1);
            break;
          }

        // SET 1,C: Set bit 1 of C
        case 0xC9:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 1);
            break;
          }

        // SET 1,D: Set bit 1 of D
        case 0xCA:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 1);
            break;
          }

        // SET 1,E: Set bit 1 of E
        case 0xCB:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 1);
            break;
          }

        // SET 1,H: Set bit 1 of H
        case 0xCC:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 1);
            break;
          }

        // SET 1,L: Set bit 1 of L
        case 0xCD:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 1);
            break;
          }

        // SET 1,(HL): Set bit 1 of value pointed by HL
        case 0xCE:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 1));
            break;
          }

        // SET 1,A: Set bit 1 of A
        case 0xCF:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 1);
            break;
          }

        // SET 2,B: Set bit 2 of B
        case 0xD0:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 2);
            break;
          }

        // SET 2,C: Set bit 2 of C
        case 0xD1:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 2);
            break;
          }

        // SET 2,D: Set bit 2 of D
        case 0xD2:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 2);
            break;
          }

        // SET 2,E: Set bit 2 of E
        case 0xD3:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 2);
            break;
          }

        // SET 2,H: Set bit 2 of H
        case 0xD4:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 2);
            break;
          }

        // SET 2,L: Set bit 2 of L
        case 0xD5:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 2);
            break;
          }

        // SET 2,(HL): Set bit 2 of value pointed by HL
        case 0xD6:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 2));
            break;
          }

        // SET 2,A: Set bit 2 of A
        case 0xD7:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 2);
            break;
          }

        // SET 3,B: Set bit 3 of B
        case 0xD8:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 3);
            break;
          }

        // SET 3,C: Set bit 3 of C
        case 0xD9:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 3);
            break;
          }

        // SET 3,D: Set bit 3 of D
        case 0xDA:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 3);
            break;
          }

        // SET 3,E: Set bit 3 of E
        case 0xDB:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 3);
            break;
          }

        // SET 3,H: Set bit 3 of H
        case 0xDC:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 3);
            break;
          }

        // SET 3,L: Set bit 3 of L
        case 0xDD:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 3);
            break;
          }

        // SET 3,(HL): Set bit 3 of value pointed by HL
        case 0xDE:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 3));
            break;
          }

        // SET 3,A: Set bit 3 of A
        case 0xDF:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 3);
            break;
          }

        // SET 4,B: Set bit 4 of B
        case 0xE0:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 4);
            break;
          }

        // SET 4,C: Set bit 4 of C
        case 0xE1:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 4);
            break;
          }

        // SET 4,D: Set bit 4 of D
        case 0xE2:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 4);
            break;
          }

        // SET 4,E: Set bit 4 of E
        case 0xE3:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 4);
            break;
          }

        // SET 4,H: Set bit 4 of H
        case 0xE4:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 4);
            break;
          }

        // SET 4,L: Set bit 4 of L
        case 0xE5:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 4);
            break;
          }

        // SET 4,(HL): Set bit 4 of value pointed by HL
        case 0xE6:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 4));
            break;
          }

        // SET 4,A: Set bit 4 of A
        case 0xE7:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 4);
            break;
          }

        // SET 5,B: Set bit 5 of B
        case 0xE8:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 5);
            break;
          }

        // SET 5,C: Set bit 5 of C
        case 0xE9:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 5);
            break;
          }

        // SET 5,D: Set bit 5 of D
        case 0xEA:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 5);
            break;
          }

        // SET 5,E: Set bit 5 of E
        case 0xEB:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 5);
            break;
          }

        // SET 5,H: Set bit 5 of H
        case 0xEC:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 5);
            break;
          }

        // SET 5,L: Set bit 5 of L
        case 0xED:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 5);
            break;
          }

        // SET 5,(HL): Set bit 5 of value pointed by HL
        case 0xEE:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 5));
            break;
          }

        // SET 5,A: Set bit 5 of A
        case 0xEF:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 5);
            break;
          }

        // SET 6,B: Set bit 6 of B
        case 0xF0:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 6);
            break;
          }

        // SET 6,C: Set bit 6 of C
        case 0xF1:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 6);
            break;
          }

        // SET 6,D: Set bit 6 of D
        case 0xF2:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 6);
            break;
          }

        // SET 6,E: Set bit 6 of E
        case 0xF3:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 6);
            break;
          }

        // SET 6,H: Set bit 6 of H
        case 0xF4:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 6);
            break;
          }

        // SET 6,L: Set bit 6 of L
        case 0xF5:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 6);
            break;
          }

        // SET 6,(HL): Set bit 6 of value pointed by HL
        case 0xF6:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 6));
            break;
          }

        // SET 6,A: Set bit 6 of A
        case 0xF7:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 6);
            break;
          }

        // SET 7,B: Set bit 7 of B
        case 0xF8:
          {
            registers.B = UtilFuncs.SetBit(registers.B, 7);
            break;
          }

        // SET 7,C: Set bit 7 of C
        case 0xF9:
          {
            registers.C = UtilFuncs.SetBit(registers.C, 7);
            break;
          }

        // SET 7,D: Set bit 7 of D
        case 0xFA:
          {
            registers.D = UtilFuncs.SetBit(registers.D, 7);
            break;
          }

        // SET 7,E: Set bit 7 of E
        case 0xFB:
          {
            registers.E = UtilFuncs.SetBit(registers.E, 7);
            break;
          }

        // SET 7,H: Set bit 7 of H
        case 0xFC:
          {
            registers.H = UtilFuncs.SetBit(registers.H, 7);
            break;
          }

        // SET 7,L: Set bit 7 of L
        case 0xFD:
          {
            registers.L = UtilFuncs.SetBit(registers.L, 7);
            break;
          }

        // SET 7,(HL): Set bit 7 of value pointed by HL
        case 0xFE:
          {
            memory.Write(registers.HL, UtilFuncs.SetBit(memory.Read(registers.HL), 7));
            break;
          }

        // SET 7,A: Set bit 7 of A
        case 0xFF:
          {
            registers.A = UtilFuncs.SetBit(registers.A, 7);
            break;
          }
      }
    }

    #endregion

  }
}
