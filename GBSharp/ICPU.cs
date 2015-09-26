using System;

namespace GBSharp
{
  public interface ICPU
  {
    CPURegisters Registers { get; }
    byte Step();
    string GetCurrentInstructionName();
    string GetCurrentInstructionDescription();

    bool InterruptMasterEnable { get; }
  }
}