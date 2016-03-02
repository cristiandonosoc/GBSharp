using System.Reflection.Emit;

namespace GBSharp
{
  public interface IInstruction
  {
    string Name { get; }
    string Description { get; }
    ushort Address { get; }
    ushort OpCode { get; }
    ushort Literal { get; }
    byte?[] Operands { get; }
    bool CB { get; }
  }
}