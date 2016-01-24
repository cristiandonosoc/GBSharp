using GBSharp.CPUSpace;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GBSharp
{
  public interface ICPU
  {
    event Action BreakpointFound;
    event Action<Interrupts> InterruptHappened;

    CPURegisters Registers { get; }
    bool InterruptMasterEnable { get; }
    IInstruction CurrentInstruction { get; }
    
    ushort[] InstructionHistogram { get; }
    ushort[] CbInstructionHistogram { get; }
    void ResetInstructionHistograms();

    void SetInterruptBreakable(Interrupts interrupt, bool isBreakable);

    byte Step(bool ignoreBreakpoints);


    List<ushort> Breakpoints { get; }
    List<ushort> GetBreakpoints(BreakpointKinds kind);
    void AddBreakpoint(BreakpointKinds kind, ushort address);
    void RemoveBreakpoint(BreakpointKinds kind, ushort address);
    void ResetBreakpoints();
  }
}