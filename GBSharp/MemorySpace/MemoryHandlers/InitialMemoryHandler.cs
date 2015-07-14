using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  class InitialMemoryHandler : MemoryHandler
  {
    internal InitialMemoryHandler() : base(null)
    {

    }

    internal override void Write(ushort address, byte value)
    {
      internalMemory[address] = value; 
    }

    internal override void Write(ushort address, ushort value)
    {
      internalMemory[address] = (byte)(value & 0x00FF);
      internalMemory[address + 1] = (byte)(value >> 8);
    }

    internal override byte Read(ushort address)
    {
      return internalMemory[address];
    }
  }
}
