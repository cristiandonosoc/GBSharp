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
    ushort Breakpoint { get; set; }

    byte Step(bool ignoreBreakpoints);
  }
}