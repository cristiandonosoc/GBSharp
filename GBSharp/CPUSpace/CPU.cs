using System;
using System.Collections.Generic;
using System.Linq;
using GBSharp.Utils;
using GBSharp.CPUSpace.Dictionaries;
using GBSharp.MemorySpace;

namespace GBSharp.CPUSpace
{
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
    // Whether the next step should trigger an interrupt event
    internal Interrupts? interruptToTrigger;
    internal Dictionary<Interrupts, bool> _breakableInterrupts = new Dictionary<Interrupts, bool>()
    {
      {Interrupts.VerticalBlanking, false},
      {Interrupts.LCDCStatus, false},
      {Interrupts.TimerOverflow, false},
      {Interrupts.SerialIOTransferCompleted, false},
      {Interrupts.P10to13TerminalNegativeEdge, false}
    };

    public event Action BreakpointFound;
    public event Action<Interrupts> InterruptHappened;

    public ushort Breakpoint
    {
      get;
      set;
    }


    // NOTE(Cristian): This Instruction instance is no longer used to communicate
    //                 the current state to the CPUView. This is because it's always
    //                 one instruction behing because the PC advances after the Step.
    //                 What it's done is that the current PC is decoded on-demand by the view.
    internal Instruction _currentInstruction = null;

    /// <summary>
    /// The current operands (extra bytes) used by the current instruction
    /// running in the CPU. This is (for now) mainly used to display this
    /// information in the CPU view
    /// NOTE is nullable so we can signify less operands
    /// </summary>
    public IInstruction CurrentInstruction
    {
      get { return FetchAndDecode(registers.PC); }
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
      //Create Instruction Lambdas
      CreateCBInstructionLambdas();

      _currentInstruction = new Instruction();

      this.memory = memory;
      this.interruptController = new InterruptController(this.memory);

      // Initialize registers 
      this.Initialize();
    }

    /// <summary>
    /// Sets the initial values for the cpu registers and memory mapped registers.
    /// This is the equivalent to run the BIOS rom.
    /// </summary>
    private void Initialize()
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

      // Initialize memory mapped registers
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

      // NOTE(Cristian): This address is not writable as a program,
      //                 so this breakpoint *should* be inactive
      this.Breakpoint = 0xFFFF;

      this.interruptToTrigger = null;
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
      bool interruptExists = false;
      Interrupts? interruptRequested = InterruptRequired(ref interruptExists);
      bool interruptInProgress = interruptRequested != null;

      if (halted)
      {
        if (interruptInProgress || interruptExists)
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

      if (interruptInProgress)
      {
        // NOTE(Cristian): We store the interrupt so we break on the next
        //                 step. This will enable that we're breaking on the
        //                 first instruction of the interrupt handler, vs
        //                 an invented CALL
        interruptToTrigger = interruptRequested.Value;
        _currentInstruction = InterruptHandler(interruptRequested.Value);
      }
      else
      {
        // If we have set an interupt to trigger a breakpoint, we break
        if (interruptToTrigger != null)
        {
          if (_breakableInterrupts[interruptToTrigger.Value])
          {
            InterruptHappened(interruptToTrigger.Value);
            return 0; // We don't advance the state because we're breaking
          }

          interruptToTrigger = null;
        }

        // Otherwise we fetch as usual
        _currentInstruction = FetchAndDecode(this.registers.PC, haltLoad);
        haltLoad = false;
      }

      // We see if there is an breakpoint to this address
      if (!ignoreBreakpoints && (_currentInstruction.Address == Breakpoint))
      {
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
      try
      {
        _currentInstruction.Lambda(_currentInstruction.Literal);
      }
      catch(NullReferenceException)
      {
        CPUInstructions.Run(this, ref registers, ref memory, 
                            (byte)_currentInstruction.OpCode, _currentInstruction.Literal);
      }

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
      //instruction.Lambda = this.instructionLambdas[(byte)instruction.OpCode];
      instruction.Ticks = CPUInstructionClocks.Get(lowOpcode);
      instruction.Name = CPUOpcodeNames.Get(lowOpcode);
      instruction.Description = CPUInstructionDescriptions.Get(lowOpcode);

      // Disable interrupts during interrupt handling and clear the current one
      this.interruptController.InterruptMasterEnable = false;
      byte IF = this.memory.Read((ushort)MMR.IF);
      IF &= (byte)~(byte)interrupt;
      this.memory.LowLevelWrite((ushort)MMR.IF, IF);
      return instruction;
    }

    /// <summary>
    /// Fetches and Decodes an instruction
    /// </summary>
    /// <param name="instructionAddress"></param>
    /// <param name="haltLoad"></param>
    /// <returns></returns>
    internal Instruction FetchAndDecode(ushort instructionAddress, bool haltLoad = false)
    {
      Instruction instruction = new Instruction();
      instruction.Address = instructionAddress;
      byte opcode = this.memory.Read(instructionAddress);
      instruction.OpCode = opcode;

      if (instruction.OpCode != 0xCB)
      {
        byte lowOpcode = (byte)instruction.OpCode;
        if(instructionHistogram[lowOpcode] < ushort.MaxValue)
          instructionHistogram[lowOpcode]++;
        // Normal instructions
        instruction.Length = CPUInstructionLengths.Get(lowOpcode);

        // Extract literal
        if (instruction.Length == 2)
        {
          // 8 bit literal
          instruction.Operands[0] = this.memory.Read((ushort)(instructionAddress + 1));
          if(haltLoad)
          {
            instruction.Operands[0] = opcode;
          }
          instruction.Literal = (byte)instruction.Operands[0];
        }
        else if (instruction.Length == 3)
        {
          // 16 bit literal, little endian
          instruction.Operands[0] = this.memory.Read((ushort)(instructionAddress + 2));
          instruction.Operands[1] = this.memory.Read((ushort)(instructionAddress + 1));

          if(haltLoad)
          {
            instruction.Operands[1] = instruction.Operands[0];
            instruction.Operands[0] = opcode;
          }

          instruction.Literal = (byte)instruction.Operands[1];
          instruction.Literal += (ushort)(instruction.Operands[0] << 8);
        }

        //instruction.Lambda = this.instructionLambdas[(byte)instruction.OpCode];
        instruction.Ticks = CPUInstructionClocks.Get(lowOpcode);
        instruction.Name = CPUOpcodeNames.Get(lowOpcode);
        instruction.Description = CPUInstructionDescriptions.Get(lowOpcode);
      }
      else
      {
        // CB instructions block
        instruction.OpCode <<= 8;
        if (!haltLoad)
        {
          instruction.OpCode += this.memory.Read((ushort)(instructionAddress + 1));
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

        instruction.Lambda = this.CBInstructionLambdas[lowOpcode];
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
    private Interrupts? InterruptRequired(ref bool interruptRequired)
    {
      // Read interrupt flags
      int interruptRequest = this.memory.Read((ushort)MMR.IF);
      // Mask enabled interrupts
      int interruptEnable = this.memory.Read((ushort)MMR.IE);

      int interrupt = interruptEnable & interruptRequest;

      if ((interrupt & 0x1F) == 0x00) // 0x1F masks the useful bits of IE and IF, there is only 5 interrupts.
      {
        // Nothing, or disabled, who cares
        return null;
      }

      // There is an interrupt waiting
      interruptRequired = true;

      if (this.interruptController.InterruptMasterEnable)
      {
        // Ok, find the interrupt with the highest priority, check the first bit set
        interrupt &= -interrupt; // Magics ;)

        // Return the first interrupt
        return (Interrupts)(interrupt & 0x1F);
      }
      else
      {
        return null;
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
      this.memory.LowLevelWrite((ushort)MMR.DIV, (byte)(this.clock >> 8));

      // Configurable timer TIMA/TMA/TAC system:
      byte TAC = this.memory.LowLevelRead((ushort)MMR.TAC);

      byte clockSelect = (byte)(TAC & 0x03); // Clock select is the bits 0 and 1 of the TAC register
      bool runTimer = (TAC & 0x04) == 0x04; // Run timer is the bit 2 of the TAC register

      ushort timerMask;
      switch (clockSelect)
      {
        case 1:
          timerMask = 0x000F; // f/2^4 0000 0000 0000 1111, (262144 Hz)
          break;
        case 2:
          timerMask = 0x003F; // f/2^6 0000 0000 0011 1111, (65536 Hz)
          break;
        case 3:
          timerMask = 0x00FF; // f/2^8 0000 0000 1111 1111, (16384 Hz)
          break;
        default:
          timerMask = 0x03FF; // f/2^10 0000 0011 1111 1111, (4096 Hz)
          break;
      }

      if (runTimer)
      {
        // Simulate every tick that occurred during the execution of the instruction
        for (int i = 1; i <= ticks; ++i)
        {
          // Maybe there is a faster way to do this without checking every clock value (??)
          if (((initialClock + i) & timerMask) == 0x0000)
          {
            // We have a perfect match! The number of oscilations is now a multiple of clock selected by TAC

            // Fetch current TIMA value
            byte TIMA = this.memory.Read((ushort)MMR.TIMA);

            // TIMA-tick
            TIMA += 1;

            if (TIMA == 0x0000)
            {
              // TIMA overflow, load TMA into TIMA
              TIMA = this.memory.Read((ushort)MMR.TMA);

              // Set the interrupt request flag
              this.interruptController.SetInterrupt(Interrupts.TimerOverflow);
            }

            // Update memory mapped timer
            this.memory.LowLevelWrite((ushort)MemorySpace.MMR.TIMA, TIMA);
          }
        }
      }
    }

    public override string ToString()
    {
      return registers.ToString();
    }


    #region Instruction Lambdas

    private Dictionary<byte, Action<ushort>> CBInstructionLambdas;

    private void CreateCBInstructionLambdas()
    {
      CBInstructionLambdas = new Dictionary<byte, Action<ushort>>()
      {
        // RLC B: Rotate B left with carry
        {0x00, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.B);
          registers.B = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC C: Rotate C left with carry
        {0x01, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.C);
          registers.C = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC D: Rotate D left with carry
        {0x02, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.D);
          registers.D = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC E: Rotate E left with carry
        {0x03, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.E);
          registers.E = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC H: Rotate H left with carry
        {0x04, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.H);
          registers.H = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC L: Rotate L left with carry
        {0x05, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.L);
          registers.L = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC (HL): Rotate value pointed by HL left with carry
        {0x06, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(memory.Read(registers.HL));
          memory.Write(registers.HL, rotateCarry.Item1);

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RLC A: Rotate A left with carry
        {0x07, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.A);
          registers.A = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RRC B: Rotate B right with carry
        {0x08, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.B);
          registers.B = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RRC C: Rotate C right with carry
        {0x09, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.C);
          registers.C = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RRC D: Rotate D right with carry
        {0x0A, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.D);
          registers.D = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RRC E: Rotate E right with carry
        {0x0B, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.E);
          registers.E = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},


        // RRC H: Rotate H right with carry
        {0x0C, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.H);
          registers.H = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},


        // RRC L: Rotate L right with carry
        {0x0D, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.L);
          registers.L = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},


        // RRC (HL): Rotate value pointed by HL right with carry
        {0x0E, (n) => {
          var rotateCarry = UtilFuncs.RotateRightAndCarry(memory.Read(registers.HL));
          memory.Write(registers.HL, rotateCarry.Item1);

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},


        // RRC A: Rotate A right with carry
        {0x0F, (n) =>{
          var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.A);
          registers.A = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL B: Rotate B left
        {0x10, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.B,1,registers.FC);
          registers.B = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL C: Rotate C left
        {0x11, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.C,1,registers.FC);
          registers.C = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL D: Rotate D left
        {0x12, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.D,1,registers.FC);
          registers.D = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL E: Rotate E left
        {0x13, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.E,1,registers.FC);
          registers.E = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL H: Rotate H left
        {0x14, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.H,1,registers.FC);
          registers.H = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL L: Rotate L left
        {0x15, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.L,1,registers.FC);
          registers.L = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL (HL): Rotate value pointed by HL left
        {0x16, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(memory.Read(registers.HL),1,registers.FC);
          memory.Write(registers.HL, rotateCarry.Item1);

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RL A: Rotate A left
        {0x17, (n) => {
          var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.A,1,registers.FC);
          registers.A = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR B: Rotate B right
        {0x18, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.B,1,registers.FC);
          registers.B = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR C: Rotate C right
        {0x19, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.C,1,registers.FC);
          registers.C = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR D: Rotate D right
        {0x1A, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.D,1,registers.FC);
          registers.D = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR E: Rotate E right
        {0x1B, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.E,1,registers.FC);
          registers.E = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR H: Rotate H right
        {0x1C, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.H,1,registers.FC);
          registers.H = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR L: Rotate L right
        {0x1D, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.L,1,registers.FC);
          registers.L = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR (HL): Rotate value pointed by HL right
        {0x1E, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(memory.Read(registers.HL),1,registers.FC);
          memory.Write(registers.HL, rotateCarry.Item1);

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // RR A: Rotate A right
        {0x1F, (n) => {
          var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.A,1,registers.FC);
          registers.A = rotateCarry.Item1;

          registers.FC = rotateCarry.Item2;
          registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA B: Shift B left preserving sign
        {0x20, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.B);
          registers.B = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA C: Shift C left preserving sign
        {0x21, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.C);
          registers.C = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA D: Shift D left preserving sign
        {0x22, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.D);
          registers.D = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA E: Shift E left preserving sign
        {0x23, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.E);
          registers.E = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA H: Shift H left preserving sign
        {0x24, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.H);
          registers.H = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA L: Shift L left preserving sign
        {0x25, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.L);
          registers.L = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA (HL): Shift value pointed by HL left preserving sign
        {0x26, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(memory.Read(registers.HL));
          memory.Write(registers.HL, shiftCarry.Item1);

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SLA A: Shift A left preserving sign
        {0x27, (n) => {
          var shiftCarry = UtilFuncs.ShiftLeft(registers.A);
          registers.A = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},
        // SRA B: Shift B right preserving sign
        {0x28, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.B);
          registers.B = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA C: Shift C right preserving sign
        {0x29, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.C);
          registers.C = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA D: Shift D right preserving sign
        {0x2A, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.D);
          registers.D = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA E: Shift E right preserving sign
        {0x2B, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.E);
          registers.E = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA H: Shift H right preserving sign
        {0x2C, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.H);
          registers.H = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA L: Shift L right preserving sign
        {0x2D, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.L);
          registers.L = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA (HL): Shift value pointed by HL right preserving sign
        {0x2E, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(memory.Read(registers.HL));
          memory.Write(registers.HL, shiftCarry.Item1);

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRA A: Shift A right preserving sign
        {0x2F, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.A);
          registers.A = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SWAP B: Swap nybbles in B
        {0x30, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.B);
          registers.B = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP C: Swap nybbles in C
        {0x31, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.C);
          registers.C = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP D: Swap nybbles in D
        {0x32, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.D);
          registers.D = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP E: Swap nybbles in E
        {0x33, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.E);
          registers.E = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP H: Swap nybbles in H
        {0x34, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.H);
          registers.H = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP L: Swap nybbles in L
        {0x35, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.L);
          registers.L = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP (HL): Swap nybbles in value pointed by HL
        {0x36, (n) => {
          byte result = UtilFuncs.SwapNibbles(memory.Read(registers.HL));
          memory.Write(registers.HL, result);

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SWAP A: Swap nybbles in A
        {0x37, (n) => {
          byte result = UtilFuncs.SwapNibbles(registers.A);
          registers.A = result;

          registers.FZ = (byte)(result == 0 ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
          registers.FC = 0;
        }},

        // SRL B: Shift B right
        {0x38, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.B);
          registers.B = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL C: Shift C right
        {0x39, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.C);
          registers.C = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL D: Shift D right
        {0x3A, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.D);
          registers.D = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL E: Shift E right
        {0x3B, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.E);
          registers.E = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL H: Shift H right
        {0x3C, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.H);
          registers.H = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL L: Shift L right
        {0x3D, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.L);
          registers.L = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL (HL): Shift value pointed by HL right
        {0x3E, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(memory.Read(registers.HL));
          memory.Write(registers.HL, shiftCarry.Item1);

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // SRL A: Shift A right
        {0x3F, (n) => {
          var shiftCarry = UtilFuncs.ShiftRightLogic(registers.A);
          registers.A = shiftCarry.Item1;

          registers.FC = shiftCarry.Item2;
          registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
          registers.FN = 0;
          registers.FH = 0;
        }},

        // BIT 0,B: Test bit 0 of B
        {0x40, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,C: Test bit 0 of C
        {0x41, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,D: Test bit 0 of D
        {0x42, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,E: Test bit 0 of E
        {0x43, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,H: Test bit 0 of H
        {0x44, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,L: Test bit 0 of L
        {0x45, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,(HL): Test bit 0 of value pointed by HL
        {0x46, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,A: Test bit 0 of A
        {0x47, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 0) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,B: Test bit 1 of B
        {0x48, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,C: Test bit 1 of C
        {0x49, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,D: Test bit 1 of D
        {0x4A, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,E: Test bit 1 of E
        {0x4B, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,H: Test bit 1 of H
        {0x4C, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,L: Test bit 1 of L
        {0x4D, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,(HL): Test bit 1 of value pointed by HL
        {0x4E, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,A: Test bit 1 of A
        {0x4F, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 1) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,B: Test bit 2 of B
        {0x50, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,C: Test bit 2 of C
        {0x51, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,D: Test bit 2 of D
        {0x52, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,E: Test bit 2 of E
        {0x53, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,H: Test bit 2 of H
        {0x54, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,L: Test bit 2 of L
        {0x55, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,(HL): Test bit 2 of value pointed by HL
        {0x56, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,A: Test bit 2 of A
        {0x57, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 2) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,B: Test bit 3 of B
        {0x58, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,C: Test bit 3 of C
        {0x59, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,D: Test bit 3 of D
        {0x5A, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,E: Test bit 3 of E
        {0x5B, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,H: Test bit 3 of H
        {0x5C, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,L: Test bit 3 of L
        {0x5D, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,(HL): Test bit 3 of value pointed by HL
        {0x5E, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,A: Test bit 3 of A
        {0x5F, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 3) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,B: Test bit 4 of B
        {0x60, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,C: Test bit 4 of C
        {0x61, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,D: Test bit 4 of D
        {0x62, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,E: Test bit 4 of E
        {0x63, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,H: Test bit 4 of H
        {0x64, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,L: Test bit 4 of L
        {0x65, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,(HL): Test bit 4 of value pointed by HL
        {0x66, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,A: Test bit 4 of A
        {0x67, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 4) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,B: Test bit 5 of B
        {0x68, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,C: Test bit 5 of C
        {0x69, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,D: Test bit 5 of D
        {0x6A, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,E: Test bit 5 of E
        {0x6B, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,H: Test bit 5 of H
        {0x6C, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,L: Test bit 5 of L
        {0x6D, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,(HL): Test bit 5 of value pointed by HL
        {0x6E, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,A: Test bit 5 of A
        {0x6F, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 5) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,B: Test bit 6 of B
        {0x70, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,C: Test bit 6 of C
        {0x71, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,D: Test bit 6 of D
        {0x72, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,E: Test bit 6 of E
        {0x73, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,H: Test bit 6 of H
        {0x74, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,L: Test bit 6 of L
        {0x75, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,(HL): Test bit 6 of value pointed by HL
        {0x76, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,A: Test bit 6 of A
        {0x77, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 6) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,B: Test bit 7 of B
        {0x78, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.B, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,C: Test bit 7 of C
        {0x79, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.C, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,D: Test bit 7 of D
        {0x7A, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.D, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,E: Test bit 7 of E
        {0x7B, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.E, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,H: Test bit 7 of H
        {0x7C, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.H, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,L: Test bit 7 of L
        {0x7D, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.L, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,(HL): Test bit 7 of value pointed by HL
        {0x7E, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(memory.Read(registers.HL), 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,A: Test bit 7 of A
        {0x7F, (n) => { registers.FZ = (byte)(UtilFuncs.TestBit(registers.A, 7) == 0 ? 1 : 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // RES 0,B: Clear (reset) bit 0 of B
        {0x80, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 0); }},

        // RES 0,C: Clear (reset) bit 0 of C
        {0x81, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 0); }},

        // RES 0,D: Clear (reset) bit 0 of D
        {0x82, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 0); }},

        // RES 0,E: Clear (reset) bit 0 of E
        {0x83, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 0); }},

        // RES 0,H: Clear (reset) bit 0 of H
        {0x84, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 0); }},

        // RES 0,L: Clear (reset) bit 0 of L
        {0x85, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 0); }},

        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        {0x86, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 0)); }},

        // RES 0,A: Clear (reset) bit 0 of A
        {0x87, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 0); }},

        // RES 1,B: Clear (reset) bit 1 of B
        {0x88, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 1); }},

        // RES 1,C: Clear (reset) bit 1 of C
        {0x89, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 1); }},

        // RES 1,D: Clear (reset) bit 1 of D
        {0x8A, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 1); }},

        // RES 1,E: Clear (reset) bit 1 of E
        {0x8B, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 1); }},

        // RES 1,H: Clear (reset) bit 1 of H
        {0x8C, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 1); }},

        // RES 1,L: Clear (reset) bit 1 of L
        {0x8D, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 1); }},

        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        {0x8E, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 1)); }},

        // RES 1,A: Clear (reset) bit 1 of A
        {0x8F, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 1); }},

        // RES 2,B: Clear (reset) bit 2 of B
        {0x90, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 2); }},

        // RES 2,C: Clear (reset) bit 2 of C
        {0x91, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 2); }},

        // RES 2,D: Clear (reset) bit 2 of D
        {0x92, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 2); }},

        // RES 2,E: Clear (reset) bit 2 of E
        {0x93, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 2); }},

        // RES 2,H: Clear (reset) bit 2 of H
        {0x94, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 2); }},

        // RES 2,L: Clear (reset) bit 2 of L
        {0x95, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 2); }},

        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        {0x96, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 2)); }},

        // RES 2,A: Clear (reset) bit 2 of A
        {0x97, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 2); }},

        // RES 3,B: Clear (reset) bit 3 of B
        {0x98, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 3); }},

        // RES 3,C: Clear (reset) bit 3 of C
        {0x99, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 3); }},

        // RES 3,D: Clear (reset) bit 3 of D
        {0x9A, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 3); }},

        // RES 3,E: Clear (reset) bit 3 of E
        {0x9B, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 3); }},

        // RES 3,H: Clear (reset) bit 3 of H
        {0x9C, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 3); }},

        // RES 3,L: Clear (reset) bit 3 of L
        {0x9D, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 3); }},

        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        {0x9E, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 3)); }},

        // RES 3,A: Clear (reset) bit 3 of A
        {0x9F, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 3); }},

        // RES 4,B: Clear (reset) bit 4 of B
        {0xA0, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 4); }},

        // RES 4,C: Clear (reset) bit 4 of C
        {0xA1, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 4); }},

        // RES 4,D: Clear (reset) bit 4 of D
        {0xA2, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 4); }},

        // RES 4,E: Clear (reset) bit 4 of E
        {0xA3, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 4); }},

        // RES 4,H: Clear (reset) bit 4 of H
        {0xA4, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 4); }},

        // RES 4,L: Clear (reset) bit 4 of L
        {0xA5, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 4); }},

        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        {0xA6, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 4)); }},

        // RES 4,A: Clear (reset) bit 4 of A
        {0xA7, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 4); }},

        // RES 5,B: Clear (reset) bit 5 of B
        {0xA8, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 5); }},

        // RES 5,C: Clear (reset) bit 5 of C
        {0xA9, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 5); }},

        // RES 5,D: Clear (reset) bit 5 of D
        {0xAA, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 5); }},

        // RES 5,E: Clear (reset) bit 5 of E
        {0xAB, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 5); }},

        // RES 5,H: Clear (reset) bit 5 of H
        {0xAC, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 5); }},

        // RES 5,L: Clear (reset) bit 5 of L
        {0xAD, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 5); }},

        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        {0xAE, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 5)); }},

        // RES 5,A: Clear (reset) bit 5 of A
        {0xAF, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 5); }},

        // RES 6,B: Clear (reset) bit 6 of B
        {0xB0, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 6); }},

        // RES 6,C: Clear (reset) bit 6 of C
        {0xB1, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 6); }},

        // RES 6,D: Clear (reset) bit 6 of D
        {0xB2, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 6); }},

        // RES 6,E: Clear (reset) bit 6 of E
        {0xB3, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 6); }},

        // RES 6,H: Clear (reset) bit 6 of H
        {0xB4, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 6); }},

        // RES 6,L: Clear (reset) bit 6 of L
        {0xB5, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 6); }},

        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        {0xB6, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 6)); }},

        // RES 6,A: Clear (reset) bit 6 of A
        {0xB7, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 6); }},

        // RES 7,B: Clear (reset) bit 7 of B
        {0xB8, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 7); }},

        // RES 7,C: Clear (reset) bit 7 of C
        {0xB9, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 7); }},

        // RES 7,D: Clear (reset) bit 7 of D
        {0xBA, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 7); }},

        // RES 7,E: Clear (reset) bit 7 of E
        {0xBB, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 7); }},

        // RES 7,H: Clear (reset) bit 7 of H
        {0xBC, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 7); }},

        // RES 7,L: Clear (reset) bit 7 of L
        {0xBD, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 7); }},

        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        {0xBE, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 7)); }},

        // RES 7,A: Clear (reset) bit 7 of A
        {0xBF, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 7); }},

        // SET 0,B: Set bit 0 of B
        {0xC0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 0); }},

        // SET 0,C: Set bit 0 of C
        {0xC1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 0); }},

        // SET 0,D: Set bit 0 of D
        {0xC2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 0); }},

        // SET 0,E: Set bit 0 of E
        {0xC3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 0); }},

        // SET 0,H: Set bit 0 of H
        {0xC4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 0); }},

        // SET 0,L: Set bit 0 of L
        {0xC5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 0); }},

        // SET 0,(HL): Set bit 0 of value pointed by HL
        {0xC6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 0)); }},

        // SET 0,A: Set bit 0 of A
        {0xC7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 0); }},

        // SET 1,B: Set bit 1 of B
        {0xC8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 1); }},

        // SET 1,C: Set bit 1 of C
        {0xC9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 1); }},

        // SET 1,D: Set bit 1 of D
        {0xCA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 1); }},

        // SET 1,E: Set bit 1 of E
        {0xCB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 1); }},

        // SET 1,H: Set bit 1 of H
        {0xCC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 1); }},

        // SET 1,L: Set bit 1 of L
        {0xCD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 1); }},

        // SET 1,(HL): Set bit 1 of value pointed by HL
        {0xCE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 1)); }},

        // SET 1,A: Set bit 1 of A
        {0xCF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 1); }},

        // SET 2,B: Set bit 2 of B
        {0xD0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 2); }},

        // SET 2,C: Set bit 2 of C
        {0xD1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 2); }},

        // SET 2,D: Set bit 2 of D
        {0xD2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 2); }},

        // SET 2,E: Set bit 2 of E
        {0xD3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 2); }},

        // SET 2,H: Set bit 2 of H
        {0xD4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 2); }},

        // SET 2,L: Set bit 2 of L
        {0xD5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 2); }},

        // SET 2,(HL): Set bit 2 of value pointed by HL
        {0xD6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 2)); }},

        // SET 2,A: Set bit 2 of A
        {0xD7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 2); }},

        // SET 3,B: Set bit 3 of B
        {0xD8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 3); }},

        // SET 3,C: Set bit 3 of C
        {0xD9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 3); }},

        // SET 3,D: Set bit 3 of D
        {0xDA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 3); }},

        // SET 3,E: Set bit 3 of E
        {0xDB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 3); }},

        // SET 3,H: Set bit 3 of H
        {0xDC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 3); }},

        // SET 3,L: Set bit 3 of L
        {0xDD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 3); }},

        // SET 3,(HL): Set bit 3 of value pointed by HL
        {0xDE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 3)); }},

        // SET 3,A: Set bit 3 of A
        {0xDF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 3); }},

        // SET 4,B: Set bit 4 of B
        {0xE0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 4); }},

        // SET 4,C: Set bit 4 of C
        {0xE1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 4); }},

        // SET 4,D: Set bit 4 of D
        {0xE2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 4); }},

        // SET 4,E: Set bit 4 of E
        {0xE3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 4); }},

        // SET 4,H: Set bit 4 of H
        {0xE4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 4); }},

        // SET 4,L: Set bit 4 of L
        {0xE5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 4); }},

        // SET 4,(HL): Set bit 4 of value pointed by HL
        {0xE6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 4)); }},

        // SET 4,A: Set bit 4 of A
        {0xE7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 4); }},

        // SET 5,B: Set bit 5 of B
        {0xE8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 5); }},

        // SET 5,C: Set bit 5 of C
        {0xE9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 5); }},

        // SET 5,D: Set bit 5 of D
        {0xEA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 5); }},

        // SET 5,E: Set bit 5 of E
        {0xEB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 5); }},

        // SET 5,H: Set bit 5 of H
        {0xEC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 5); }},

        // SET 5,L: Set bit 5 of L
        {0xED, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 5); }},

        // SET 5,(HL): Set bit 5 of value pointed by HL
        {0xEE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 5)); }},

        // SET 5,A: Set bit 5 of A
        {0xEF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 5); }},

        // SET 6,B: Set bit 6 of B
        {0xF0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 6); }},

        // SET 6,C: Set bit 6 of C
        {0xF1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 6); }},

        // SET 6,D: Set bit 6 of D
        {0xF2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 6); }},

        // SET 6,E: Set bit 6 of E
        {0xF3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 6); }},

        // SET 6,H: Set bit 6 of H
        {0xF4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 6); }},

        // SET 6,L: Set bit 6 of L
        {0xF5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 6); }},

        // SET 6,(HL): Set bit 6 of value pointed by HL
        {0xF6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 6)); }},

        // SET 6,A: Set bit 6 of A
        {0xF7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 6); }},

        // SET 7,B: Set bit 7 of B
        {0xF8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 7); }},

        // SET 7,C: Set bit 7 of C
        {0xF9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 7); }},

        // SET 7,D: Set bit 7 of D
        {0xFA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 7); }},

        // SET 7,E: Set bit 7 of E
        {0xFB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 7); }},

        // SET 7,H: Set bit 7 of H
        {0xFC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 7); }},

        // SET 7,L: Set bit 7 of L
        {0xFD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 7); }},

        // SET 7,(HL): Set bit 7 of value pointed by HL
        {0xFE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 7)); }},

        // SET 7,A: Set bit 7 of A
        {0xFF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 7); }},
      };
    }

    #endregion

  }
}
