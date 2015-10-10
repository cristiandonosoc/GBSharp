using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  /// <summary>
  /// The ROM Only Catdridge is (shock!) a ROM only catdridge.
  /// It is 32 kB of size and the mapping spans from the addresses
  /// 0x0000 to 0x7FFF.
  /// </summary>
  class RomOnlyMemoryHandler : MemoryHandler
  {
    private const ushort romOnlyStart = 0x0000;
    private const ushort romOnlyLength = 0x8000;

    /// <summary>
    /// Class constructor. Performs the loading of the current cartridge into memory.
    /// </summary>
    /// <param name="gameboy"></param>
    internal RomOnlyMemoryHandler(GameBoy gameboy)
      : base(gameboy)
    {
      Buffer.BlockCopy(this.cartridge.Data, romOnlyStart, 
                       this.memoryData, romOnlyStart, 
                       Math.Min(this.cartridge.Data.Length, romOnlyLength));
    }
  }
}
