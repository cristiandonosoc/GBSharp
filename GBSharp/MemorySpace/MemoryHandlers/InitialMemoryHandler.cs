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

    internal override void LoadInternalMemory(byte[] data)
    {
      base.LoadInternalMemory(data);

      // NOTE(Cristian): We create a sample gb memory
      for (uint i = 0; i < 100; i++)
        internalMemory[i] = 0xFF;


      // We write the sample tiles
      for (uint i = 0x8000;
          i < 0x9800;
          i++)
      {
        if (i < 0x8800)
        {
          internalMemory[i] = (byte)0x00;
        }
        else if (i < 0x9000)
        {
          internalMemory[i] = (byte)0xFF;
        }
        else
        {
          internalMemory[i] = (byte)0xCC;
        }
      }

      // We write the sample display memory
      for (uint i = 0;
           i < 1024;
           i++)
      {
        internalMemory[i+0x9800] = (byte)0xFF;
      }
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
