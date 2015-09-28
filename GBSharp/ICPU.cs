using System.Collections;
using System.Collections.Generic;

namespace GBSharp
{
  public interface ICPU
  {
    CPURegisters Registers { get; }
    bool InterruptMasterEnable { get; }
    IInstruction CurrentInstruction { get; }

    byte Step();
    IEnumerable<IInstruction> Dissamble();

  
    
  }
}