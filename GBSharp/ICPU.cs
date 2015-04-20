using System.Collections.Generic;

namespace GBSharp
{
  public interface ICPU
  {
    IEnumerable<IRegister> Registers { get; }
    IRegister ProgramCounter { get; }
    IRegister StackPointer { get; }
    IRegister Flags { get; }
  }
}