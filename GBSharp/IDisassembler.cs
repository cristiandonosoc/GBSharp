using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
  public interface IDisassembler
  {
    IEnumerable<IInstruction> Disassamble(ushort startAddress, 
                                          bool permissive = true);
    void PoorManDisassemble();

    int DisassembledCount { get; }
    byte[][] DisassembledMatrix { get; }

  }
}
