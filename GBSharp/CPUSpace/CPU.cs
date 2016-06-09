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
        public ushort Target { get; internal set; }
        public BreakpointKinds Kind { get; internal set; }
    }

    class CPU : ICPU
    {
        [Serializable]
        internal class State
        {
            internal ushort NextPC;
            internal ushort Clock; // 16 bit oscillation counter at 4194304 Hz (2^22).
            internal bool Halted;
            internal bool HaltLoad;
            internal bool Stopped;
            internal bool InterruptRequired;
            internal bool InterruptInProgress;
            // Whether the next step should trigger an interrupt event
            internal Interrupts InterruptToTrigger;
            internal Interrupts InterruptTriggered;

            // Timing
            internal ushort DivCounter;
            internal byte TimaCounter;
            internal int TacCounter;
            internal int TacMask;
            internal byte TmaValue;

            internal CPURegisters Registers = new CPURegisters();

            internal Instruction CurrentInstruction = null;
        }
        State _state = new State();

        internal State GetState() { return _state; }
        internal void SetState(State state) { _state = state; }

        #region INTERNAL STATE GETTERS/SETTERS

        internal ushort NextPC
        {
            get { return _state.NextPC; }
            set { _state.NextPC = value; }
        }

        internal bool Halted
        {
            get { return _state.Halted; }
            set { _state.Halted = value; }
        }

        internal bool HaltLoad
        {
            get { return _state.HaltLoad; }
            set { _state.HaltLoad = value; }
        }

        internal bool Stopped
        {
            get { return _state.Stopped; }
            set { _state.Stopped = value; }
        }

        // NOTE(Cristian): This Instruction instance is no longer used to communicate
        //                 the current state to the CPUView. This is because it's always
        //                 one instruction behing because the PC advances after the Step.
        //                 What it's done is that the current PC is decoded on-demand by the view.
        internal Instruction InternalCurrentInstruction
        {
            get { return _state.CurrentInstruction; }
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
                FetchAndDecode(ref _exportInstruction, _state.Registers.PC);
                return _exportInstruction;
            }
        }



        #endregion

        internal MemorySpace.Memory _memory;
        internal InterruptController _interruptController;

        #region BREAKPOINTS

        internal Dictionary<Interrupts, bool> _breakableInterrupts = new Dictionary<Interrupts, bool>()
    {
      {Interrupts.VerticalBlanking, false},
      {Interrupts.LCDCStatus, false},
      {Interrupts.TimerOverflow, false},
      {Interrupts.SerialIOTransferCompleted, false},
      {Interrupts.P10to13TerminalNegativeEdge, false}
    };

        public void SetInterruptBreakable(Interrupts interrupt, bool isBreakable)
        {
            _breakableInterrupts[interrupt] = isBreakable;
        }

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
            switch (kind)
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
            switch (kind)
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
            switch (kind)
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

        // Interrupt starting addresses
        Dictionary<Interrupts, ushort> interruptHandlers = new Dictionary<Interrupts, ushort>()
    {
      {Interrupts.VerticalBlanking, 0x0040},
      {Interrupts.LCDCStatus, 0x0048},
      {Interrupts.TimerOverflow, 0x0050},
      {Interrupts.SerialIOTransferCompleted, 0x0058},
      {Interrupts.P10to13TerminalNegativeEdge, 0x0060}
    };

        public CPURegisters Registers
        {
            get { return _state.Registers; }
        }

        public bool InterruptMasterEnable
        {
            get { return _interruptController.InterruptMasterEnable; }
        }

        #region HISTOGRAM

        internal ushort[] _instructionHistogram = new ushort[256];
        internal ushort[] _cbInstructionHistogram = new ushort[256];

        public ushort[] InstructionHistogram
        {
            get { return _instructionHistogram; }
        }

        public ushort[] CbInstructionHistogram
        {
            get { return _cbInstructionHistogram; }
        }

        public void ResetInstructionHistograms()
        {
            _instructionHistogram = new ushort[256];
            _cbInstructionHistogram = new ushort[256];
        }

        #endregion

        public CPU(MemorySpace.Memory memory)
        {
            this._memory = memory;
            this._interruptController = new InterruptController(this._memory);

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
            _state.Clock = 0;

            _state.Halted = false;
            _state.HaltLoad = false;
            _state.Stopped = false;

            // Magic CPU initial values (after bios execution).
            _state.Registers.A = 1;
            _state.Registers.BC = 0x0013;
            _state.Registers.DE = 0x00D8;
            _state.Registers.HL = 0x014D;
            _state.Registers.PC = 0x0100;
            _state.Registers.SP = 0xFFFE;

            // We restart the timer
            _state.DivCounter = 0xABFF;
            _state.TimaCounter = 0;
            _state.TacCounter = 0;
            _state.TacMask = 0;
            _state.TmaValue = 0;

            // Initialize memory mapped registers
            this._memory.LowLevelWrite(0xFF04, 0xAB); // DIV
            this._memory.LowLevelWrite(0xFF05, 0x00); // TIMA
            this._memory.LowLevelWrite(0xFF06, 0x00); // TMA
            this._memory.LowLevelWrite(0xFF07, 0x00); // TAC
            this._memory.LowLevelWrite(0xFF10, 0x80); // NR10
            this._memory.LowLevelWrite(0xFF11, 0xBF); // NR11
            this._memory.LowLevelWrite(0xFF12, 0xF3); // NR12
            this._memory.LowLevelWrite(0xFF14, 0xBF); // NR14
            this._memory.LowLevelWrite(0xFF16, 0x3F); // NR21
            this._memory.LowLevelWrite(0xFF17, 0x00); // NR22
            this._memory.LowLevelWrite(0xFF19, 0xBF); // NR24
            this._memory.LowLevelWrite(0xFF1A, 0x7F); // NR30
            this._memory.LowLevelWrite(0xFF1B, 0xFF); // NR31
            this._memory.LowLevelWrite(0xFF1C, 0x9F); // NR32
            this._memory.LowLevelWrite(0xFF1E, 0xBF); // NR33
            this._memory.LowLevelWrite(0xFF20, 0xFF); // NR41
            this._memory.LowLevelWrite(0xFF21, 0x00); // NR42
            this._memory.LowLevelWrite(0xFF22, 0x00); // NR43
            this._memory.LowLevelWrite(0xFF23, 0xBF); // NR30
            this._memory.LowLevelWrite(0xFF24, 0x77); // NR50
            this._memory.LowLevelWrite(0xFF25, 0xF3); // NR51
            this._memory.LowLevelWrite(0xFF26, 0xF1); // NR52 GB: 0xF1, SGB: 0xF0
            this._memory.LowLevelWrite(0xFF40, 0x91); // LCDC
            this._memory.LowLevelWrite(0xFF42, 0x00); // SCY
            this._memory.LowLevelWrite(0xFF43, 0x00); // SCX
            this._memory.LowLevelWrite(0xFF45, 0x00); // LYC
            this._memory.LowLevelWrite(0xFF47, 0xFC); // BGP
            this._memory.LowLevelWrite(0xFF48, 0xFF); // OBP0
            this._memory.LowLevelWrite(0xFF49, 0xFF); // OBP1
            this._memory.LowLevelWrite(0xFF4A, 0x00); // WY
            this._memory.LowLevelWrite(0xFF4B, 0x00); // WX
            this._memory.LowLevelWrite(0xFFFF, 0x00); // IE

            _state.CurrentInstruction = new Instruction();
            _exportInstruction = new Instruction();
        }

        /// <summary>
        /// Get the amount of ticks to be run before the opcode execution
        /// </summary>
        /// <returns>cpu ticks</returns>
        private byte GetPreClocks()
        {
            byte ticks = 0;
            // We return the ticks that the instruction took
            if (!_state.CurrentInstruction.CB)
            {
                ticks = CPUInstructionPreClocks.Get((byte)_state.CurrentInstruction.OpCode);
            }
            else
            {
                ticks = CPUCBInstructionPreClocks.Get((byte)_state.CurrentInstruction.OpCode);
            }
            return ticks;
        }

        /// <summary>
        /// Get the amount of ticks to be run after the opcode execution
        /// (and before any potention post-execution code)
        /// </summary>
        /// <returns>cpu ticks</returns>
        private byte GetPostTicks()
        {
            // We calculate how many more ticks have to run
            byte ticks = 0;
            if (!_state.CurrentInstruction.CB)
            {
                ticks = CPUInstructionPostClocks.Get(this,
                                                     (byte)_state.CurrentInstruction.OpCode,
                                                     _state.CurrentInstruction.Literal);
            }
            else
            {
                ticks = CPUCBInstructionPostClocks.Get(this,
                                                       (byte)_state.CurrentInstruction.OpCode,
                                                       _state.CurrentInstruction.Literal);
            }
            return ticks;
        }

        /// <summary>
        /// Sets the current instruction to be executed. Note that this DOES NOT execute the opcode,
        /// but sets the CPU internal state so it can be executed when the correct clock ticks happen
        /// before the execution has to runb.
        /// </summary>
        /// <param name="ignoreBreakpoints">If this step should check for breakpoints</param>
        /// <returns>
        /// The number of ticks that have to run before the opcode is executed. 
        /// The base clock frequency (2^22hz, ~4Mhz).
        /// </returns>
        internal byte DetermineStep(bool ignoreBreakpoints)
        {
            if (_state.Stopped) { return 0; }
            if (_state.Halted) { return 0; }

            if (_state.InterruptRequired)
            {
                // NOTE(Cristian): We store the interrupt so we break on the next
                //                 step. This will enable that we're breaking on the
                //                 first instruction of the interrupt handler, vs
                //                 an invented CALL
                _state.InterruptInProgress = true;
                _state.InterruptTriggered = _state.InterruptToTrigger;
                _state.CurrentInstruction = InterruptHandler(_state.InterruptToTrigger);

                // We need to check if there is another interrupt waiting
                CheckForInterruptRequired();
            }
            else
            {
                // If we have set an interupt to trigger a breakpoint, we break
                if (_state.InterruptInProgress)
                {
                    // TODO(Cristian): Change this to an array!
                    if (_breakableInterrupts[_state.InterruptTriggered])
                    {
                        InterruptHappened(_state.InterruptTriggered);
                        return 0; // We don't advance the state because we're breaking
                    }

                    _state.InterruptInProgress = false;
                }

                // Otherwise we fetch as usual
                FetchAndDecode(ref _state.CurrentInstruction, _state.Registers.PC, _state.HaltLoad);
                _state.HaltLoad = false;
            }

            // We see if there is an breakpoint to this address
            if (!ignoreBreakpoints && _executionBreakpoints.Contains(_state.CurrentInstruction.Address))
            {
                CurrentBreakpoint.Address = _state.CurrentInstruction.Address;
                CurrentBreakpoint.Target = _state.CurrentInstruction.Address;
                CurrentBreakpoint.Kind = BreakpointKinds.EXECUTION;
                BreakpointFound();
                return 0;
            }

            // We check to see if there is breakpoint to be triggered
            BreakpointKinds breakpointKind = BreakpointKinds.NONE;
            if (!_state.CurrentInstruction.CB)
            {
                breakpointKind = CPUInstructionBreakpoints.Check(this,
                                                                 (byte)_state.CurrentInstruction.OpCode,
                                                                 _state.CurrentInstruction.Literal,
                                                                 ignoreBreakpoints);
            }
            else
            {
                breakpointKind = CPUCBInstructionBreakpoints.Check(this,
                                                                   (byte)_state.CurrentInstruction.OpCode,
                                                                   _state.CurrentInstruction.Literal,
                                                                   ignoreBreakpoints);
            }

            // We see if there is an breakpoint to this address
            // NOTE(Cristian): CurrentBreakpoint.Target was calculated by Check
            if (breakpointKind != BreakpointKinds.NONE)
            {
                CurrentBreakpoint.Address = _state.CurrentInstruction.Address;
                CurrentBreakpoint.Kind = breakpointKind;
                BreakpointFound();
                return 0;
            }

            byte prevTicksRequired = GetPreClocks();
            return prevTicksRequired;
        }

        /// <summary>
        /// Actually executes the instruction that was determine by the Step phase.
        /// In most cases, the whole of the instrucftion is run, but for some opcodes, 
        /// some additional code has to be run.
        /// It is possible for those cases (and conditional jumps) that some extra clock
        /// ticks have to run after execution.
        /// </summary>
        /// <returns>How many clock ticks have to run after the opcode execution</returns>
        internal byte ExecuteInstruction()
        {
            // Prepare for program counter movement, but wait for instruction execution.
            // Overwrite _state.NextPC in the instruction lambdas if you want to implement jumps.
            // NOTE(Cristian): If we don't differentiate this case, the CALL instruction of the
            //                 interrupt will be added to _state.NextPC, which will in turn be written
            //                 into the stack. This means that when we RET, we would have jumped
            //                 into the address we *should* have jumped plus some meaningless offset!
            if (!_state.InterruptInProgress)
            {
                this._state.NextPC = (ushort)(_state.Registers.PC + _state.CurrentInstruction.Length);
            }

            if (!_state.CurrentInstruction.CB)
            {
                CPUInstructions.RunInstruction(this,
                                               (byte)_state.CurrentInstruction.OpCode,
                                               _state.CurrentInstruction.Literal);
            }
            else
            {
                CPUCBInstructions.RunCBInstruction(this,
                                                   (byte)_state.CurrentInstruction.OpCode,
                                                   _state.CurrentInstruction.Literal);
            }

            // Push the next program counter value into the real program counter!
            _state.Registers.PC = this._state.NextPC;

            // We calculate how many more ticks have to run
            byte remainingSteps = GetPostTicks();
            return remainingSteps;
        }

        /// <summary>
        /// Some instructions have a two-stage approach: the read at one clock cycle
        /// and write at the next. For this we need some post-execution code to be run
        /// in the step process.
        /// </summary>
        internal void PostExecuteInstruction()
        {
            if (!_state.CurrentInstruction.CB)
            {
                CPUInstructionPostCode.Run(this,
                                            (byte)_state.CurrentInstruction.OpCode,
                                            _state.CurrentInstruction.Literal);
            }
            else
            {
                CPUCBInstructionPostCode.Run(this,
                                             (byte)_state.CurrentInstruction.OpCode,
                                             _state.CurrentInstruction.Literal);
            }
        }

        private Instruction InterruptHandler(Interrupts interrupt)
        {
            // Handle interrupt with a CALL instruction to the interrupt handler
            Instruction instruction = new Instruction();
            instruction.OpCode = 0xCD; // CALL!
            byte lowOpcode = (byte)instruction.OpCode;
            instruction.Length = CPUInstructionLengths.Get(lowOpcode);
            instruction.Literal = this.interruptHandlers[(Interrupts)interrupt];
            instruction.Ticks = CPUInstructionPreClocks.Get(lowOpcode);
            instruction.Name = CPUInstructionNames.Get(lowOpcode);
            instruction.Description = CPUInstructionDescriptions.Get(lowOpcode);

            // Disable interrupts during interrupt handling and clear the current one
            this._interruptController.InterruptMasterEnable = false;
            byte IF = this._memory.LowLevelRead((ushort)MMR.IF);
            IF &= (byte)~(byte)interrupt;
            this._memory.LowLevelWrite((ushort)MMR.IF, IF);
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
            byte opcode = this._memory.LowLevelRead(instructionAddress);
            instruction.OpCode = opcode;

            if (instruction.OpCode != 0xCB)
            {
                instruction.CB = false;
                byte lowOpcode = (byte)instruction.OpCode;
                if (_instructionHistogram[lowOpcode] < ushort.MaxValue)
                    _instructionHistogram[lowOpcode]++;
                // Normal instructions
                instruction.Length = CPUInstructionLengths.Get(lowOpcode);

                // Extract literal
                if (instruction.Length == 2)
                {
                    // 8 bit literal
                    instruction.Operands[0] = this._memory.LowLevelRead((ushort)(instructionAddress + 1));
                    if (haltLoad)
                    {
                        instruction.Operands[0] = opcode;
                    }
                    instruction.Literal = (byte)instruction.Operands[0];
                }
                else if (instruction.Length == 3)
                {
                    // 16 bit literal, little endian
                    instruction.Operands[0] = this._memory.LowLevelRead((ushort)(instructionAddress + 2));
                    instruction.Operands[1] = this._memory.LowLevelRead((ushort)(instructionAddress + 1));

                    if (haltLoad)
                    {
                        instruction.Operands[1] = instruction.Operands[0];
                        instruction.Operands[0] = opcode;
                    }

                    instruction.Literal = (byte)instruction.Operands[1];
                    instruction.Literal += (ushort)(instruction.Operands[0] << 8);
                }

                instruction.Ticks = CPUInstructionPreClocks.Get(lowOpcode);
                instruction.Name = CPUInstructionNames.Get(lowOpcode);
                instruction.Description = CPUInstructionDescriptions.Get(lowOpcode);
            }
            else
            {
                instruction.CB = true;
                // CB instructions block
                instruction.OpCode <<= 8;
                if (!haltLoad)
                {
                    instruction.OpCode += this._memory.LowLevelRead((ushort)(instructionAddress + 1));
                }
                else
                {
                    instruction.OpCode += 0xCB; // The first byte is duplicated
                }

                byte lowOpcode = (byte)instruction.OpCode;

                if (_cbInstructionHistogram[lowOpcode] < ushort.MaxValue)
                {
                    _cbInstructionHistogram[lowOpcode]++;
                }
                instruction.Length = CPUCBInstructionLengths.Get(lowOpcode);
                // There is no literal in CB instructions!

                //instruction.Lambda = this.CBInstructionLambdas[lowOpcode];
                instruction.Ticks = CPUCBInstructionPreClocks.Get(lowOpcode);
                instruction.Name = CPUCBInstructionNames.Get(lowOpcode);
                instruction.Description = CPUCBInstructionDescriptions.Get(lowOpcode);
            }

            // NOTE(Cristian): On haltLoad (HALT with IME disabled), the next byte after the HALT opcode
            //                 is "duplicated". This is a hardware bug.
            if (haltLoad)
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
            _state.InterruptRequired = false;

            // Read interrupt flags
            int interruptRequest = this._memory.LowLevelRead((ushort)MMR.IF);
            // Mask enabled interrupts
            int interruptEnable = this._memory.LowLevelRead((ushort)MMR.IE);

            int interrupt = interruptEnable & interruptRequest;
            interrupt &= 0x1F; // 0x1F masks the useful bits of IE and IF, there is only 5 interrupts.

            // We check if no interrupt have happeded, or are disabled, who cares
            if (interrupt == 0x00) { return; }

            // Interrupts unhaltd the CPU *even* if the IME is disabled
            _state.Halted = false;

            if (this._interruptController.InterruptMasterEnable)
            {
                // There is an interrupt waiting
                _state.InterruptRequired = true;

                // Ok, find the interrupt with the highest priority, check the first bit set
                interrupt &= -interrupt; // Magics ;)

                // Return the first interrupt
                _state.InterruptToTrigger = (Interrupts)(interrupt & 0x1F);
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
            _state.Clock += ticks;

            // Upper 8 bits of the clock should be accessible through DIV register.
            _state.DivCounter += ticks;
            this._memory.LowLevelWrite((ushort)MMR.DIV, (byte)(_state.DivCounter >> 8));

            // Configurable timer TIMA/TMA/TAC system:
            byte TAC = this._memory.LowLevelRead((ushort)MMR.TAC);
            bool runTimer = (TAC & 0x04) == 0x04; // Run timer is the bit 2 of the TAC register
            if (runTimer)
            {
                // Simulate every tick that occurred during the execution of the instruction
                for (int i = 1; i <= ticks; ++i)
                {
                    ++_state.TacCounter;
                    // Maybe there is a faster way to do this without checking every clock value (??)
                    if ((_state.TacCounter & _state.TacMask) == 0x0000)
                    {
                        ++_state.TimaCounter;

                        // If overflow, we trigger the event
                        if (_state.TimaCounter == 0x0000)
                        {
                            _state.TimaCounter = _state.TmaValue;
                            this._interruptController.SetInterrupt(Interrupts.TimerOverflow);
                        }

                        // Update memory mapped timer
                        this._memory.LowLevelWrite((ushort)MemorySpace.MMR.TIMA, _state.TimaCounter);
                    }
                }
            }
        }

        internal void HandleMemoryChange(MMR register, byte value)
        {
            switch (register)
            {
                case MMR.TIMA:
                    _state.TimaCounter = value;
                    this._memory.LowLevelWrite((ushort)MMR.TIMA, value);
                    break;
                case MMR.TMA:
                    _state.TmaValue = value;
                    this._memory.LowLevelWrite((ushort)MMR.TMA, value);
                    break;
                case MMR.TAC:
                    // Clock select is the bits 0 and 1 of the TAC register
                    byte clockSelect = (byte)(value & 0x03);
                    switch (clockSelect)
                    {
                        case 1:
                            _state.TacMask = 0x000F; // f/2^4  0000 0000 0000 1111, (262144 Hz)
                            break;
                        case 2:
                            _state.TacMask = 0x003F; // f/2^6  0000 0000 0011 1111, (65536 Hz)
                            break;
                        case 3:
                            _state.TacMask = 0x00FF; // f/2^8  0000 0000 1111 1111, (16384 Hz)
                            break;
                        default:
                            _state.TacMask = 0x03FF; // f/2^10 0000 0011 1111 1111, (4096 Hz)
                            break;
                    }
                    // We restart the counter
                    _state.TacCounter = 0;
                    // TAC has a 0xF8 mask (only lower 3 bits are useful)
                    this._memory.LowLevelWrite((ushort)MMR.TAC, (byte)(0xF8 | value));
                    break;
            }
        }

        public override string ToString()
        {
            return _state.Registers.ToString();
        }
    }
}
