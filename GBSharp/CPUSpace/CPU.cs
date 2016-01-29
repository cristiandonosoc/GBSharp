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
    private bool _timaChangedThisInstruction;
    private int _tacCounter;
    private int _tacMask;
    private byte _tmaValue;

    #region BREAKPOINTS

    public event Action BreakpointFound;
    public event Action<Interrupts> InterruptHappened;

    private List<ushort> _executionBreakpoints;
    private List<ushort> _readBreakpoints;
    private List<ushort> _writeBreakpoints;
    private List<ushort> _jumpBreakpoints;

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
      _timaChangedThisInstruction = false;
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
      if(stopped) { return 0; }

      // Instruction fetch and decode
      //bool interruptExists = false;
      //Interrupts? interruptRequested = InterruptRequired(ref interruptExists);
      //bool interruptInProgress = interruptRequested != null;

      if (halted)
      {
        if (interruptRequired)
        {
          // An interrupt unhalts the CPU
          halted = false;
        }
        else
        {
          // The CPU is halted, so it shouldn't decode
          return 0;
        }
      }

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
        breakpointKind = this.RunInstruction((byte)_currentInstruction.OpCode, 
                                                  _currentInstruction.Literal,
                                                  ignoreBreakpoints);
      }
      else
      {
        breakpointKind = this.RunCBInstruction((byte)_currentInstruction.OpCode, 
                                                    _currentInstruction.Literal,
                                                    ignoreBreakpoints);
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
      var initialClock = this.clock;
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
      if (runTimer && !_tacChangedThisInstruction)
      {
        // Simulate every tick that occurred during the execution of the instruction
        for (int i = 1; i <= ticks; ++i)
        {
          ++_tacCounter;
          // Maybe there is a faster way to do this without checking every clock value (??)
          if ((_tacCounter & _tacMask) == 0x0000)
          {
            if(!_timaChangedThisInstruction)
            {
              ++_timaCounter;
            }

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

      _timaChangedThisInstruction = false;
      _tacChangedThisInstruction = false;
    }

    private bool _tacChangedThisInstruction = false;

    internal void HandleMemoryChange(MMR register, byte value)
    {
      switch (register)
      {
        case MMR.TIMA:
          _timaCounter = value;
          _timaChangedThisInstruction = true;
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
          _tacChangedThisInstruction = true;
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
    /// Runs an normal opcode instruction
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    /// <returns>Whether a breakpoint was found</returns>
    private BreakpointKinds RunInstruction(byte opcode, ushort n, bool ignoreBreakpoints)
    {
      switch (opcode)
      {
        // NOP: No Operation
        case 0x00:
          {
            break;
          }

        // LD BC,nn: Load 16-bit immediate into BC
        case 0x01:
          {
            registers.BC = n;
            break;
          }

        // LD (BC),A: Save A to address pointed by BC
        case 0x02:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.BC))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.BC, registers.A);
            break;
          }

        // INC BC: Increment 16-bit BC
        case 0x03:
          {
            registers.BC++;
            break;
          }

        // INC B: Increment B
        case 0x04:
          {
            registers.B++;

            registers.FZ = (byte)(registers.B == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.B & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC B: Decrement B
        case 0x05:
          {
            registers.B--;

            registers.FZ = (byte)(registers.B == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.B & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD B,n: Load 8-bit immediate into B
        case 0x06:
          {
            registers.B = (byte)n;
            break;
          }

        // RLC A: Rotate A left with carry
        case 0x07:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.A);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // LD (nn),SP: Save SP to given address
        case 0x08:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(n))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(n, registers.SP);
            break;
          }

        // ADD HL,BC: Add 16-bit BC to HL
        case 0x09:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.BC;

            registers.HL += registers.BC;

            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ registers.B ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LD A,(BC): Load A from address pointed to by BC
        case 0x0A:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.BC))
            {
              return BreakpointKinds.READ;
            }
            
            registers.A = memory.Read(registers.BC);
            break;
          }

        // DEC BC: Decrement 16-bit BC
        case 0x0B:
          {
            registers.BC--;
            break;
          }

        // INC C: Increment C
        case 0x0C:
          {
            registers.C++;

            registers.FZ = (byte)(registers.C == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.C & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC C: Decrement C
        case 0x0D:
          {
            registers.C--;

            registers.FZ = (byte)(registers.C == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.C & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD C,n: Load 8-bit immediate into C
        case 0x0E:
          {
            registers.C = (byte)n;
            break;
          }

        // RRC A: Rotate A right with carry
        case 0x0F:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.A);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // STOP: Stop processor
        case 0x10:
          {
            this.stopped = true;
            break;
          }

        // LD DE,nn: Load 16-bit immediate into DE
        case 0x11:
          {
            registers.DE = n;
            break;
          }

        // LD (DE),A: Save A to address pointed by DE
        case 0x12:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.DE))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.DE, registers.A);
            break;
          }

        // INC DE: Increment 16-bit DE
        case 0x13:
          {
            registers.DE++;
            break;
          }

        // INC D: Increment D
        case 0x14:
          {
            registers.D++;

            registers.FZ = (byte)(registers.D == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.D & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC D: Decrement D
        case 0x15:
          {
            registers.D--;

            registers.FZ = (byte)(registers.D == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.D & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD D,n: Load 8-bit immediate into D
        case 0x16:
          {
            registers.D = (byte)n;
            break;
          }

        // RL A: Rotate A left
        case 0x17:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.A, 1, registers.FC);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // JR n: Relative jump by signed immediate
        case 0x18:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(this.nextPC + sn);
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
 
            this.nextPC = target;
            break;
          }

        // ADD HL,DE: Add 16-bit DE to HL
        case 0x19:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.DE;
            registers.HL += registers.DE;

            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ registers.D ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LD A,(DE): Load A from address pointed to by DE
        case 0x1A:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.DE))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(registers.DE);
            break;
          }

        // DEC DE: Decrement 16-bit DE
        case 0x1B:
          {
            registers.DE--;
            break;
          }

        // INC E: Increment E
        case 0x1C:
          {
            registers.E++;

            registers.FZ = (byte)(registers.E == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.E & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC E: Decrement E
        case 0x1D:
          {
            registers.E--;

            registers.FZ = (byte)(registers.E == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.E & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD E,n: Load 8-bit immediate into E
        case 0x1E:
          {
            registers.E = (byte)n;
            break;
          }

        // RR A: Rotate A right
        case 0x1F:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.A, 1, registers.FC);
            registers.A = rotateCarry.Item1;

            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // JR NZ,n: Relative jump by signed immediate if last result was not zero
        case 0x20:
          {
            if (registers.FZ != 0) { return BreakpointKinds.NONE; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(this.nextPC + sn);
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 12;
            break;
          }

        // LD HL,nn: Load 16-bit immediate into HL
        case 0x21:
          {
            registers.HL = n;
            break;
          }

        // LDI (HL),A: Save A to address pointed by HL, and increment HL
        case 0x22:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL++, registers.A);
            break;
          }

        // INC HL: Increment 16-bit HL
        case 0x23:
          {
            registers.HL++;
            break;
          }

        // INC H: Increment H
        case 0x24:
          {
            registers.H++;

            registers.FZ = (byte)(registers.H == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.H & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC H: Decrement H
        case 0x25:
          {
            registers.H--;

            registers.FZ = (byte)(registers.H == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.H & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD H,n: Load 8-bit immediate into H
        case 0x26:
          {
            registers.H = (byte)n;
            break;
          }

        // DAA: Adjust A for BCD addition
        case 0x27:
          {
            int value = registers.A;

            if (registers.FN != 0) // ADD, ADC, INC
            {
              if (registers.FH != 0) { value = (value - 0x06) & 0xFF; }
              if (registers.FC != 0) { value -= 0x60; }
            }
            else // SUB, SBC, DEC, NEG
            {
              if ((registers.FH != 0) || ((value & 0x0F) > 0x09)) { value += 0x06; }
              if ((registers.FC != 0) || (value > 0x9F)) { value += 0x60; }
            }

            registers.FH = 0;

            //registers.FC = 0;
            if ((value & 0x100) == 0x100) { registers.FC = 1; }

            value &= 0xFF;

            registers.FZ = (byte)(value == 0 ? 1 : 0);

            registers.A = (byte)value;
            break;
          }

        // JR Z,n: Relative jump by signed immediate if last result was zero
        case 0x28:
          {
            if (registers.FZ == 0) { return BreakpointKinds.NONE; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(this.nextPC + sn);
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 12;
            break;
          }

        // ADD HL,HL: Add 16-bit HL to HL
        case 0x29:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.HL;

            registers.HL += registers.HL;

            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ initialH ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LDI A,(HL): Load A from address pointed to by HL, and increment HL
        case 0x2A:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(registers.HL++);
            break;
          }

        // DEC HL: Decrement 16-bit HL
        case 0x2B:
          {
            registers.HL--;
            break;
          }

        // INC L: Increment L
        case 0x2C:
          {
            registers.L++;

            registers.FZ = (byte)(registers.L == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.L & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC L: Decrement L
        case 0x2D:
          {
            registers.L--;

            registers.FZ = (byte)(registers.L == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.L & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD L,n: Load 8-bit immediate into L
        case 0x2E:
          {
            registers.L = (byte)n;
            break;
          }

        // CPL: Complement (logical NOT) on A
        case 0x2F:
          {
            registers.A = (byte)~registers.A;

            registers.FN = 1;
            registers.FH = 1;
            break;
          }

        // JR NC,n: Relative jump by signed immediate if last result caused no carry
        case 0x30:
          {
            if (registers.FC != 0) { return BreakpointKinds.NONE; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(this.nextPC + sn);
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 12;
            break;
          }

        // LD SP,nn: Load 16-bit immediate into SP
        case 0x31:
          {
            registers.SP = n;
            break;
          }

        // LDD (HL),A: Save A to address pointed by HL, and decrement HL
        case 0x32:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL--, registers.A);
            break;
          }

        // INC SP: Increment 16-bit HL
        case 0x33:
          {
            registers.SP++;
            break;
          }

        // INC (HL): Increment value pointed by HL
        case 0x34:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            byte value = memory.Read(registers.HL);
            ++value;
            memory.Write(registers.HL, value);

            registers.FZ = (byte)(value == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((value & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC (HL): Decrement value pointed by HL
        case 0x35:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            byte value = memory.Read(registers.HL);
            --value;
            memory.Write(registers.HL, value);

            registers.FZ = (byte)(value == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((value & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD (HL),n: Load 8-bit immediate into address pointed by HL
        case 0x36:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, (byte)n);
            break;
          }

        // SCF: Set carry flag
        case 0x37:
          {
            registers.FC = 1;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // JR C,n: Relative jump by signed immediate if last result caused carry
        case 0x38:
          {
            if (registers.FC == 0) { return BreakpointKinds.NONE; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(this.nextPC + sn);
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 12;
            break;
          }

        // ADD HL,SP: Add 16-bit SP to HL
        case 0x39:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.SP;

            registers.HL += registers.SP;

            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ (registers.SP >> 8) ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LDD A,(HL): Load A from address pointed to by HL, and decrement HL
        case 0x3A:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(registers.HL--);
            break;
          }

        // DEC SP: Decrement 16-bit SP
        case 0x3B:
          {
            registers.SP--;
            break;
          }

        // INC A: Increment A
        case 0x3C:
          {
            registers.A++;

            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.A & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC A: Decrement A
        case 0x3D:
          {
            registers.A--;

            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.A & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD A,n: Load 8-bit immediate into A
        case 0x3E:
          {
            registers.A = (byte)n;
            break;
          }

        // CCF: Complement Carry Flag
        case 0x3F:
          {
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = (byte)(~registers.FC & 1);
            break;
          }

        // LD B,B: Copy B to B
        case 0x40:
          {
#pragma warning disable
            registers.B = registers.B;
#pragma warning restore
            break;
          }

        // LD B,C: Copy C to B
        case 0x41:
          {
            registers.B = registers.C;
            break;
          }

        // LD B,D: Copy D to B
        case 0x42:
          {
            registers.B = registers.D;
            break;
          }

        // LD B,E: Copy E to B
        case 0x43:
          {
            registers.B = registers.E;
            break;
          }

        // LD B,H: Copy H to B
        case 0x44:
          {
            registers.B = registers.H;
            break;
          }

        // LD B,L: Copy L to B
        case 0x45:
          {
            registers.B = registers.L;
            break;
          }

        // LD B,(HL): Copy value pointed by HL to B
        case 0x46:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.B = memory.Read(registers.HL);
            break;
          }

        // LD B,A: Copy A to B
        case 0x47:
          {
            registers.B = registers.A;
            break;
          }

        // LD C,B: Copy B to C
        case 0x48:
          {
            registers.C = registers.B;
            break;
          }

        // LD C,C: Copy C to C
        case 0x49:
          {
#pragma warning disable
            registers.C = registers.C;
#pragma warning restore
            break;
          }

        // LD C,D: Copy D to C
        case 0x4A:
          {
            registers.C = registers.D;
            break;
          }

        // LD C,E: Copy E to C
        case 0x4B:
          {
            registers.C = registers.E;
            break;
          }

        // LD C,H: Copy H to C
        case 0x4C:
          {
            registers.C = registers.H;
            break;
          }

        // LD C,L: Copy L to C
        case 0x4D:
          {
            registers.C = registers.L;
            break;
          }

        // LD C,(HL): Copy value pointed by HL to C
        case 0x4E:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.C = memory.Read(registers.HL);
            break;
          }

        // LD C,A: Copy A to C
        case 0x4F:
          {
            registers.C = registers.A;
            break;
          }

        // LD D,B: Copy B to D
        case 0x50:
          {
            registers.D = registers.B;
            break;
          }

        // LD D,C: Copy C to D
        case 0x51:
          {
            registers.D = registers.C;
            break;
          }

        // LD D,D: Copy D to D
        case 0x52:
          {
#pragma warning disable
            registers.D = registers.D;
#pragma warning restore
            break;
          }

        // LD D,E: Copy E to D
        case 0x53:
          {
            registers.D = registers.E;
            break;
          }

        // LD D,H: Copy H to D
        case 0x54:
          {
            registers.D = registers.H;
            break;
          }

        // LD D,L: Copy L to D
        case 0x55:
          {
            registers.D = registers.L;
            break;
          }

        // LD D,(HL): Copy value pointed by HL to D
        case 0x56:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.D = memory.Read(registers.HL);
            break;
          }

        // LD D,A: Copy A to D
        case 0x57:
          {
            registers.D = registers.A;
            break;
          }

        // LD E,B: Copy B to E
        case 0x58:
          {
            registers.E = registers.B;
            break;
          }

        // LD E,C: Copy C to E
        case 0x59:
          {
            registers.E = registers.C;
            break;
          }

        // LD E,D: Copy D to E
        case 0x5A:
          {
            registers.E = registers.D;
            break;
          }

        // LD E,E: Copy E to E
        case 0x5B:
          {
#pragma warning disable
            registers.E = registers.E;
#pragma warning restore
            break;
          }

        // LD E,H: Copy H to E
        case 0x5C:
          {
            registers.E = registers.H;
            break;
          }

        // LD E,L: Copy L to E
        case 0x5D:
          {
            registers.E = registers.L;
            break;
          }

        // LD E,(HL): Copy value pointed by HL to E
        case 0x5E:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.E = memory.Read(registers.HL);
            break;
          }

        // LD E,A: Copy A to E
        case 0x5F:
          {
            registers.E = registers.A;
            break;
          }

        // LD H,B: Copy B to H
        case 0x60:
          {
            registers.H = registers.B;
            break;
          }

        // LD H,C: Copy C to H
        case 0x61:
          {
            registers.H = registers.C;
            break;
          }

        // LD H,D: Copy D to H
        case 0x62:
          {
            registers.H = registers.D;
            break;
          }

        // LD H,E: Copy E to H
        case 0x63:
          {
            registers.H = registers.E;
            break;
          }

        // LD H,H: Copy H to H
        case 0x64:
          {
#pragma warning disable
            registers.H = registers.H;
#pragma warning restore
            break;
          }

        // LD H,L: Copy L to H
        case 0x65:
          {
            registers.H = registers.L;
            break;
          }

        // LD H,(HL): Copy value pointed by HL to H
        case 0x66:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.H = memory.Read(registers.HL);
            break;
          }

        // LD H,A: Copy A to H
        case 0x67:
          {
            registers.H = registers.A;
            break;
          }

        // LD L,B: Copy B to L
        case 0x68:
          {
            registers.L = registers.B;
            break;
          }

        // LD L,C: Copy C to L
        case 0x69:
          {
            registers.L = registers.C;
            break;
          }

        // LD L,D: Copy D to L
        case 0x6A:
          {
            registers.L = registers.D;
            break;
          }

        // LD L,E: Copy E to L
        case 0x6B:
          {
            registers.L = registers.E;
            break;
          }

        // LD L,H: Copy H to L
        case 0x6C:
          {
            registers.L = registers.H;
            break;
          }

        // LD L,L: Copy L to L
        case 0x6D:
          {
#pragma warning disable
            registers.L = registers.L;
#pragma warning restore
            break;
          }

        // LD L,(HL): Copy value pointed by HL to L
        case 0x6E:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.L = memory.Read(registers.HL);
            break;
          }

        // LD L,A: Copy A to L
        case 0x6F:
          {
            registers.L = registers.A;
            break;
          }

        // LD (HL),B: Copy B to address pointed by HL
        case 0x70:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.B);
            break;
          }

        // LD (HL),C: Copy C to address pointed by HL
        case 0x71:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.C);
            break;
          }

        // LD (HL),D: Copy D to address pointed by HL
        case 0x72:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.D);
            break;
          }

        // LD (HL),E: Copy E to address pointed by HL
        case 0x73:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.E);
            break;
          }

        // LD (HL),H: Copy H to address pointed by HL
        case 0x74:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.H);
            break;
          }

        // LD (HL),L: Copy L to address pointed by HL
        case 0x75:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.L);
            break;
          }

        // HALT: Halt processor
        case 0x76:
          {
            if (interruptController.InterruptMasterEnable)
            {
              halted = true;
            }
            else
            {
              halted = true;
              haltLoad = true;
            }
            break;
          }

        // LD (HL),A: Copy A to address pointed by HL
        case 0x77:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(registers.HL, registers.A);
            break;
          }

        // LD A,B: Copy B to A
        case 0x78:
          {
            registers.A = registers.B;
            break;
          }

        // LD A,C: Copy C to A
        case 0x79:
          {
            registers.A = registers.C;
            break;
          }

        // LD A,D: Copy D to A
        case 0x7A:
          {
            registers.A = registers.D;
            break;
          }

        // LD A,E: Copy E to A
        case 0x7B:
          {
            registers.A = registers.E;
            break;
          }

        // LD A,H: Copy H to A
        case 0x7C:
          {
            registers.A = registers.H;
            break;
          }

        // LD A,L: Copy L to A
        case 0x7D:
          {
            registers.A = registers.L;
            break;
          }

        // LD A,(HL): Copy value pointed by HL to A
        case 0x7E:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(registers.HL);
            break;
          }

        // LD A,A: Copy A to A
        case 0x7F:
          {
#pragma warning disable
            registers.A = registers.A;
#pragma warning restore
            break;
          }

        // ADD A,B: Add B to A
        case 0x80:
          {
            byte initial = registers.A;
            byte toSum = registers.B;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,C: Add C to A
        case 0x81:
          {
            byte initial = registers.A;
            byte toSum = registers.C;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,D: Add D to A
        case 0x82:
          {
            byte initial = registers.A;
            byte toSum = registers.D;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,E: Add E to A
        case 0x83:
          {
            byte initial = registers.A;
            byte toSum = registers.E;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,H: Add H to A
        case 0x84:
          {
            byte initial = registers.A;
            byte toSum = registers.H;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,L: Add L to A
        case 0x85:
          {
            byte initial = registers.A;
            byte toSum = registers.L;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,(HL): Add value pointed by HL to A
        case 0x86:
          {
            byte initial = registers.A;
            byte toSum = memory.Read(registers.HL);
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,A: Add A to A
        case 0x87:
          {
            byte initial = registers.A;
            byte toSum = registers.A;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,B: Add B and carry flag to A
        case 0x88:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.B;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.B ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,C: Add C and carry flag to A
        case 0x89:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.C;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.C ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,D: Add D and carry flag to A
        case 0x8A:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.D;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.D ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,E: Add E and carry flag to A
        case 0x8B:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.E;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.E ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,H: Add H and carry flag to A
        case 0x8C:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.H;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.H ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,L: Add and carry flag L to A
        case 0x8D:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.L;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.L ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,(HL): Add value pointed by HL and carry flag to A
        case 0x8E:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            byte m = memory.Read(registers.HL);
            A += m;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ m ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,A: Add A and carry flag to A
        case 0x8F:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.A;
            A += registers.FC;
            registers.A = (byte)(A & 0xFF);

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ initial ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // SUB A,B: Subtract B from A
        case 0x90:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.B,
                          0);
            break;
          }

        // SUB A,C: Subtract C from A
        case 0x91:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.C,
                          0);
            break;
          }

        // SUB A,D: Subtract D from A
        case 0x92:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.D,
                          0);
            break;
          }

        // SUB A,E: Subtract E from A
        case 0x93:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.E,
                          0);
            break;
          }

        // SUB A,H: Subtract H from A
        case 0x94:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.H,
                          0);
            break;
          }

        // SUB A,L: Subtract L from A
        case 0x95:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.L,
                          0);
            break;
          }

        // SUB A,(HL): Subtract value pointed by HL from A
        case 0x96:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          memory.Read(registers.HL),
                          0);
            break;
          }

        // SUB A,A: Subtract A from A
        case 0x97:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.A,
                          0);
            break;
          }

        // SBC A,B: Subtract B and carry flag from A
        case 0x98:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.B,
                          registers.FC);
            break;
          }

        // SBC A,C: Subtract C and carry flag from A
        case 0x99:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.C,
                          registers.FC);
            break;
          }

        // SBC A,D: Subtract D and carry flag from A
        case 0x9A:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.D,
                          registers.FC);
            break;
          }

        // SBC A,E: Subtract E and carry flag from A
        case 0x9B:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.E,
                          registers.FC);
            break;
          }

        // SBC A,H: Subtract H and carry flag from A
        case 0x9C:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.H,
                          registers.FC);
            break;
          }

        // SBC A,L: Subtract and carry flag L from A
        case 0x9D:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.L,
                          registers.FC);
            break;
          }

        // SBC A,(HL): Subtract value pointed by HL and carry flag from A
        case 0x9E:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          memory.Read(registers.HL),
                          registers.FC);
            break;
          }

        // SBC A,A: Subtract A and carry flag from A
        case 0x9F:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.A,
                          registers.FC);
            break;
          }

        // AND B: Logical AND B against A
        case 0xA0:
          {
            registers.A &= registers.B;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND C: Logical AND C against A
        case 0xA1:
          {
            registers.A &= registers.C;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND D: Logical AND D against A
        case 0xA2:
          {
            registers.A &= registers.D;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND E: Logical AND E against A
        case 0xA3:
          {
            registers.A &= registers.E;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND H: Logical AND H against A
        case 0xA4:
          {
            registers.A &= registers.H;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND L: Logical AND L against A
        case 0xA5:
          {
            registers.A &= registers.L;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND (HL): Logical AND value pointed by HL against A
        case 0xA6:
          {
            registers.A &= memory.Read(registers.HL);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND A: Logical AND A against A
        case 0xA7:
          {
            registers.A &= registers.A;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR B: Logical XOR B against A
        case 0xA8:
          {
            registers.A ^= registers.B;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR C: Logical XOR C against A
        case 0xA9:
          {
            registers.A ^= registers.C;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR D: Logical XOR D against A
        case 0xAA:
          {
            registers.A ^= registers.D;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR E: Logical XOR E against A
        case 0xAB:
          {
            registers.A ^= registers.E;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR H: Logical XOR H against A
        case 0xAC:
          {
            registers.A ^= registers.H;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR L: Logical XOR L against A
        case 0xAD:
          {
            registers.A ^= registers.L;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR (HL): Logical XOR value pointed by HL against A
        case 0xAE:
          {
            registers.A ^= memory.Read(registers.HL);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR A: Logical XOR A against A
        case 0xAF:
          {
            registers.A ^= registers.A;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR B: Logical OR B against A
        case 0xB0:
          {
            registers.A |= registers.B;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR C: Logical OR C against A
        case 0xB1:
          {
            registers.A |= registers.C;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR D: Logical OR D against A
        case 0xB2:
          {
            registers.A |= registers.D;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR E: Logical OR E against A
        case 0xB3:
          {
            registers.A |= registers.E;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR H: Logical OR H against A
        case 0xB4:
          {
            registers.A |= registers.H;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR L: Logical OR L against A
        case 0xB5:
          {
            registers.A |= registers.L;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR (HL): Logical OR value pointed by HL against A
        case 0xB6:
          {
            registers.A |= memory.Read(registers.HL);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR A: Logical OR A against A
        case 0xB7:
          {
            registers.A |= registers.A;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // CP B: Compare B against A
        case 0xB8:
          {
            byte operand = registers.B;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP C: Compare C against A
        case 0xB9:
          {
            byte operand = registers.C;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP D: Compare D against A
        case 0xBA:
          {
            byte operand = registers.D;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP E: Compare E against A
        case 0xBB:
          {
            byte operand = registers.E;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP H: Compare H against A
        case 0xBC:
          {
            byte operand = registers.H;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP L: Compare L against A
        case 0xBD:
          {
            byte operand = registers.L;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP (HL): Compare value pointed by HL against A
        case 0xBE:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

            byte operand = memory.Read(registers.HL);
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP A: Compare A against A
        case 0xBF:
          {
            byte operand = registers.A;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // RET NZ: Return if last result was not zero
        case 0xC0:
          {
            if (registers.FZ != 0) { return BreakpointKinds.NONE; }
            // We load the program counter (high byte is in higher address)
            this.nextPC = memory.Read(registers.SP++);
            this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            _currentInstruction.Ticks = 20;
            break;
          }

        // POP BC: Pop 16-bit value from stack into BC
        case 0xC1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.BC = res;
            break;
          }

        // JP NZ,nn: Absolute jump to 16-bit location if last result was not zero
        case 0xC2:
          {
            if (registers.FZ != 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 16;
            break;
          }

        // JP nn: Absolute jump to 16-bit location
        case 0xC3:
          {
            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            break;
          }

        // CALL NZ,nn: Call routine at 16-bit location if last result was not zero
        case 0xC4:
          {
            if (registers.FZ != 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            registers.SP -= 2;
            memory.Write(registers.SP, this.nextPC);

            // We jump
            this.nextPC = target;
            _currentInstruction.Ticks = 24;
            break;
          }

        // PUSH BC: Push 16-bit BC onto stack
        case 0xC5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.BC);
            break;
          }

        // ADD A,n: Add 8-bit immediate to A
        case 0xC6:
          {
            byte initial = registers.A;
            byte toSum = (byte)n;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // RST 0: Call routine at address 0000h
        case 0xC7:
          {
            this.RunInstruction(0xCD, 0, ignoreBreakpoints);

            break;
          }

        // RET Z: Return if last result was zero
        case 0xC8:
          {
            if (registers.FZ == 0) { return BreakpointKinds.NONE; }
            // We load the program counter (high byte is in higher address)
            this.nextPC = memory.Read(registers.SP++);
            this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            _currentInstruction.Ticks = 20;
            break;
          }

        // RET: Return to calling routine
        case 0xC9:
          {
            // We load the program counter (high byte is in higher address)
            this.nextPC = memory.Read(registers.SP++);
            this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            break;
          }

        // JP Z,nn: Absolute jump to 16-bit location if last result was zero
        case 0xCA:
          {
            if (registers.FZ == 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 16;
            break;
          }

        // Ext ops: Extended operations (two-byte instruction code)
        case 0xCB:
          {
            throw new InvalidInstructionException("Ext ops (0xCB)");
          }

        // CALL Z,nn: Call routine at 16-bit location if last result was zero
        case 0xCC:
          {
            if (registers.FZ == 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            registers.SP -= 2;
            memory.Write(registers.SP, this.nextPC);

            // We jump
            this.nextPC = target;
            _currentInstruction.Ticks = 24;
            break;
          }

        // CALL nn: Call routine at 16-bit location
        case 0xCD:
          {
            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            registers.SP -= 2;
            memory.Write(registers.SP, this.nextPC);

            // We jump
            this.nextPC = target;
            break;
          }

        // ADC A,n: Add 8-bit immediate and carry to A
        case 0xCE:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += n;
            A += registers.FC;
            registers.A = (byte)A;

            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ n ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // RST 8: Call routine at address 0008h
        case 0xCF:
          {
            this.RunInstruction(0xCD, 0x08, ignoreBreakpoints);
            break;
          }

        // RET NC: Return if last result caused no carry
        case 0xD0:
          {
            if (registers.FC != 0) { return BreakpointKinds.NONE; }
            // We load the program counter (high byte is in higher address)
            this.nextPC = memory.Read(registers.SP++);
            this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            _currentInstruction.Ticks = 20;
            break;
          }

        // POP DE: Pop 16-bit value from stack into DE
        case 0xD1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.DE = res;
            break;
          }

        // JP NC,nn: Absolute jump to 16-bit location if last result caused no carry
        case 0xD2:
          {
            if (registers.FC != 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 16;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xD3:
          {
            throw new InvalidInstructionException("XX (0xD3)");
          }

        // CALL NC,nn: Call routine at 16-bit location if last result caused no carry
        case 0xD4:
          {
            if (registers.FC != 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            registers.SP -= 2;
            memory.Write(registers.SP, this.nextPC);

            // We jump
            this.nextPC = target;
            _currentInstruction.Ticks = 24;
            break;
          }

        // PUSH DE: Push 16-bit DE onto stack
        case 0xD5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.DE);
            break;
          }

        // SUB A,n: Subtract 8-bit immediate from A
        case 0xD6:
          {
            byte subtractor = (byte)n;
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          subtractor,
                          0);
            break;
          }

        // RST 10: Call routine at address 0010h
        case 0xD7:
          {
            this.RunInstruction(0xCD, 0x10, ignoreBreakpoints);
            break;

          }

        // RET C: Return if last result caused carry
        case 0xD8:
          {
            if (registers.FC == 0) { return BreakpointKinds.NONE; }
            // We load the program counter (high byte is in higher address)
            this.nextPC = memory.Read(registers.SP++);
            this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            _currentInstruction.Ticks = 20;
            break;
          }

        // RETI: Enable interrupts and return to calling routine
        case 0xD9:
          {
            this.interruptController.InterruptMasterEnable = true;

            // We load the program counter (high byte is in higher address)
            this.nextPC = memory.Read(registers.SP++);
            this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            break;
          }

        // JP C,nn: Absolute jump to 16-bit location if last result caused carry
        case 0xDA:
          {
            if (registers.FC == 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            _currentInstruction.Ticks = 16;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xDB:
          {
            throw new InvalidInstructionException("XX (0xDB)");
          }

        // CALL C,nn: Call routine at 16-bit location if last result caused carry
        case 0xDC:
          {
            if (registers.FC == 0) { return BreakpointKinds.NONE; }

            ushort target = n;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            registers.SP -= 2;
            memory.Write(registers.SP, this.nextPC);

            // We jump
            this.nextPC = target;
            _currentInstruction.Ticks = 24;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xDD:
          {
            throw new InvalidInstructionException("XX (0xDD)");
          }

        // SBC A,n: Subtract 8-bit immediate and carry from A
        case 0xDE:
          {
            byte substractor = 0;
            unchecked { substractor = (byte)n; }
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          substractor,
                          registers.FC);
            break;
          }

        // RST 18: Call routine at address 0018h
        case 0xDF:
          {
            this.RunInstruction(0xCD, 0x18, ignoreBreakpoints);
            break;
          }

        // LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)
        case 0xE0:
          {
            ushort address = (ushort)(0xFF00 | (byte)n);
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(address))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(address, registers.A);
            break;
          }

        // POP HL: Pop 16-bit value from stack into HL
        case 0xE1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.HL = res;
            break;
          }

        // LDH (C),A: Save A at address pointed to by (FF00h + C)
        case 0xE2:
          {
            ushort address = (ushort)(0xFF00 | registers.C);
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(address))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(address, registers.A);
            break;
          }

        // XX: Operation removed in this CPU
        case 0xE3:
          {
            throw new InvalidInstructionException("XX (0xE3)");
          }

        // XX: Operation removed in this CPU
        case 0xE4:
          {
            throw new InvalidInstructionException("XX (0xE4)");
          }

        // PUSH HL: Push 16-bit HL onto stack
        case 0xE5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.HL);
            break;
          }

        // AND n: Logical AND 8-bit immediate against A
        case 0xE6:
          {
            registers.A &= (byte)n;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // RST 20: Call routine at address 0020h
        case 0xE7:
          {
            this.RunInstruction(0xCD, 0x20, ignoreBreakpoints);
            break;
          }

        // ADD SP,d: Add signed 8-bit immediate to SP
        case 0xE8:
          {
            // We determine the short offset
            sbyte sn = 0;
            unchecked { sn = (sbyte)n; }

            // We set the registers
            registers.FZ = 0;
            registers.FN = 0;

            registers.FH = (byte)
              (((registers.SP & 0x0F) + (sn & 0x0F) > 0x0F) ? 1 : 0);
            registers.FC = (byte)
              (((registers.SP & 0xFF) + (sn & 0xFF) > 0xFF) ? 1 : 0);

            // We make the sum
            registers.SP = (ushort)(registers.SP + sn);
            break;
          }

        // JP (HL): Jump to 16-bit value pointed by HL
        case 0xE9:
          {
            ushort target = registers.HL;
            if (!ignoreBreakpoints && _jumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            this.nextPC = target;
            break;
          }

        // LD (nn),A: Save A at given 16-bit address
        case 0xEA:
          {
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(n))
            {
              return BreakpointKinds.WRITE;
            }
 
            memory.Write(n, registers.A);
            break;
          }

        // XX: Operation removed in this CPU
        case 0xEB:
          {
            throw new InvalidInstructionException("XX (0xEB)");
          }

        // XX: Operation removed in this CPU
        case 0xEC:
          {
            throw new InvalidInstructionException("XX (0xEC)");
          }

        // XX: Operation removed in this CPU
        case 0xED:
          {
            throw new InvalidInstructionException("XX (0xED)");
          }

        // XOR n: Logical XOR 8-bit immediate against A
        case 0xEE:
          {
            registers.A ^= (byte)n;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // RST 28: Call routine at address 0028h
        case 0xEF:
          {
            this.RunInstruction(0xCD, 0x28, ignoreBreakpoints);
            break;
          }

        // LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)
        case 0xF0:
          {
            ushort address = (ushort)(0xFF00 | (byte)n);
            if (!ignoreBreakpoints && _readBreakpoints.Contains(address))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(address);
            break;
          }

        // POP AF: Pop 16-bit value from stack into AF
        case 0xF1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.AF = res;
            break;
          }

        // LDH A, (C): Operation removed in this CPU? (Or Load into A memory from FF00 + C?)
        case 0xF2:
          {
            ushort address = (ushort)(0xFF00 | registers.C);
            if (!ignoreBreakpoints && _readBreakpoints.Contains(address))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(address);
            break;
          }

        // DI: DIsable interrupts
        case 0xF3:
          {
            this.interruptController.InterruptMasterEnable = false;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xF4:
          {
            throw new InvalidInstructionException("XX (0xF4)");
          }

        // PUSH AF: Push 16-bit AF onto stack
        case 0xF5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.AF);
            break;
          }

        // OR n: Logical OR 8-bit immediate against A
        case 0xF6:
          {
            registers.A |= (byte)n;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // RST 30: Call routine at address 0030h
        case 0xF7:
          {
            this.RunInstruction(0xCD, 0x30, ignoreBreakpoints);
            break;
          }

        // LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL
        case 0xF8:
          {
            // We determine the short offset
            sbyte sn = 0;
            unchecked { sn = (sbyte)n; }

            // We set the registers
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = (byte)
              (((registers.SP & 0x0F) + (sn & 0x0F) > 0x0F) ? 1 : 0);
            registers.FC = (byte)
              (((registers.SP & 0xFF) + (sn & 0xFF) > 0xFF) ? 1 : 0);

            // We make the sum
            registers.HL = (ushort)(registers.SP + sn);
            break;
          }

        // LD SP,HL: Copy HL to SP
        case 0xF9:
          {
            registers.SP = registers.HL;
            break;
          }

        // LD A,(nn): Load A from given 16-bit address
        case 0xFA:
          {
            if (!ignoreBreakpoints && _readBreakpoints.Contains(n))
            {
              return BreakpointKinds.READ;
            }

            registers.A = memory.Read(n);
            break;
          }

        // EI: Enable interrupts
        case 0xFB:
          {
            this.interruptController.InterruptMasterEnable = true;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xFC:
          {
            throw new InvalidInstructionException("XX (0xFC)");
          }

        // XX: Operation removed in this CPU
        case 0xFD:
          {
            throw new InvalidInstructionException("XX (0xFD)");
          }

        // CP n: Compare 8-bit immediate against A
        case 0xFE:
          {
            byte operand = (byte)n;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)
              (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (registers.A == operand)
            {
              registers.FZ = 1;
            }
            else
            {
              registers.FZ = 0;
              if (registers.A < operand)
              {
                registers.FC = 1;
              }
            }
            break;
          }

        // RST 38: Call routine at address 0038h
        case 0xFF:
          {
            this.RunInstruction(0xCD, 0x38, ignoreBreakpoints);
            break;
          }
      }

      // No breakpoints found
      return BreakpointKinds.NONE;
    }

    /// <summary>
    /// Runs an CB opcode instruction
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    /// <returns>Whether a breakpoint was found</returns>
    private BreakpointKinds RunCBInstruction(byte opcode, ushort n, bool ignoreBreakpoints)
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _readBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.READ;
            }

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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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
            if (!ignoreBreakpoints && _writeBreakpoints.Contains(registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
 
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

      // No breakpoints found
      return BreakpointKinds.NONE;
    }

    #endregion

  }
}
