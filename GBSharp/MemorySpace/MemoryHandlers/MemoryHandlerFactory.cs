using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  class MemoryHandlerFactory
  {
    /// <summary>
    /// Creates a MemoryHandler the matches the CartridgeType.
    /// </summary>
    /// <param name="gameboy">A reference to a gameboy with a loaded catridge.</param>
    /// <returns>A MemoryHandler instance</returns>
    internal static MemoryHandler CreateMemoryHandler(GameBoy gameboy)
    {
      switch (gameboy.Cartridge.Type)
      {
        case CartridgeType.ROM_ONLY:
        case CartridgeType.ROM_RAM:
          return new RomOnlyMemoryHandler(gameboy);
        case CartridgeType.ROM_MBC1:
        case CartridgeType.ROM_MBC1_RAM:
          return new MBC1MemoryHandler(gameboy);
        case CartridgeType.ROM_MBC1_RAM_BATT:
          return new MBC1MemoryHandler(gameboy, true);
        case CartridgeType.ROM_MBC3:
        case CartridgeType.ROM_MBC3_RAM:
        case CartridgeType.ROM_MBC3_RAM_BATT:
        case CartridgeType.ROM_MBC3_TIMER_BATT:
        case CartridgeType.ROM_MBC3_TIMER_RAM_BATT:
          return new MBC3MemoryHandler(gameboy);
      }

      // TODO(Cristián): Implement logic for different CartridgeType
      throw new NotImplementedException();
    }
  }
}
