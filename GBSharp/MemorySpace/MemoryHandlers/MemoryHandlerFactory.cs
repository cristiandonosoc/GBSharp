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
      if (gameboy.Cartridge.Type == CartridgeType.ROM_ONLY)
      {
        return new RomOnlyMemoryHandler(gameboy);
      }

      // TODO(Cristián): Implement logic for different CartridgeType
      throw new NotImplementedException();
    }
  }
}
