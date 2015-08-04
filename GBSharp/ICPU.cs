using System;

namespace GBSharp
{
  public interface ICPU
  {
    event Action StepFinished;
    CPURegisters Registers { get; }
    byte Step();
    string GetCurrentInstructionName();

    bool InterruptMasterEnable { get; }
    
  }
}