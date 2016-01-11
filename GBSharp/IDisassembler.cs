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
    void PoorManDisassemble(ushort PC);

    int DisassembledCount { get; }
    byte[][] DisassembledMatrix { get; }

  }
}
