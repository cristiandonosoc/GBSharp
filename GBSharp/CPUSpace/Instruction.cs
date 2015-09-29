using System;

namespace GBSharp.CPUSpace
{
  internal class Instruction : IInstruction
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public ushort Address { get; set; }
    public ushort OpCode { get; set; }
    public ushort Literal { get; set; }
    public byte?[] Operands { get; set; }
    internal Action<ushort> Lambda { get; set; }
    internal byte Length { get; set; }
    internal byte Ticks { get; set; }

    public Instruction()
    {
      Operands = new byte?[2];
    }
  }
}