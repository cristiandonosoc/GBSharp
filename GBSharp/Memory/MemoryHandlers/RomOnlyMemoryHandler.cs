using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Memory.MemoryHandlers
{
  /// <summary>
  /// The ROM Only Catdridge is (shock!) a ROM only catdridge.
  /// It is 32 kB of size and the mapping spans from the addresses
  /// 0x0000 to 0x7FFF.
  /// </summary>
  class RomOnlyMemoryHandler : MemoryHandler
  {
    private ushort romOnlyStart = 0x0000;
    private ushort romOnlyEnd = 0x7FFF;

    internal RomOnlyMemoryHandler(GBSharp.Catridge.Cartridge cartridge)
      : base(cartridge)
    {
    }

    internal override void LoadInternalMemory(byte[] data)
    {
      base.LoadInternalMemory(data);

      // We copy the ROM areas to the internal memory of the GB
      // TODO(Cristián): Perform fast memory copy
      for (ushort i = romOnlyStart;
          i < romOnlyEnd;
          i++)
      {
          internalMemory[i] = this.cartridge.Data[i];
      }
    }

    internal override void Write(ushort address, byte value)
    {
      // If the address is within the rom only address space,
      // we do nothing at all.
      if (romOnlyStart <= address && address <= romOnlyEnd)
      {
        return;
      }

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
