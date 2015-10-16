using System;
using System.Collections;
using System.Collections.Generic;

namespace GBSharp
{
  public interface ICPU
  {
    event Action BreakpointFound;
    CPURegisters Registers { get; }
    bool InterruptMasterEnable { get; }
    IInstruction CurrentInstruction { get; }
    ushort Breakpoint { get; set; }

    byte Step(bool ignoreBreakpoints);
    IEnumerable<IInstruction> Dissamble(ushort startAddress, 
                                        bool permisive = true);
  }
}