using System;

namespace GBSharp
{
  public interface ICPU
  {
    CPURegisters Registers { get; }
    byte Step();
    string GetCurrentInstructionName();

    bool InterruptMasterEnable { get; }
  }
}