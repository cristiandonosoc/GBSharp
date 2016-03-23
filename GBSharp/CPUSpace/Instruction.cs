using System;

namespace GBSharp.CPUSpace
{
  internal class Instruction : IInstruction
  {
    public string Name { get; internal set; }
    public string Description { get; internal set; }
    public ushort Address { get; internal set; }
    public ushort OpCode { get; internal set; }
    public ushort Literal { get; internal set; }
    public byte?[] Operands { get; internal set; }
    internal byte Length { get; set; }
    internal byte Ticks { get; set; }
    public bool CB { get; internal set; }

    public Instruction()
    {
      Operands = new byte?[2];
    }
  }
}