using System.Collections.Generic;

namespace GBSharp
{
  public interface ICPU
  {
    CPURegisters Registers { get; }
    void Step();
  }
}