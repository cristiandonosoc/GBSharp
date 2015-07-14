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
    /// <param name="cartridge">The cartridge to be loaded into the GB</param>
    /// <returns>A MemoryHandler instance</returns>
    internal static MemoryHandler CreateMemoryHandler(Cartridge.Cartridge cartridge)
    {
      if (cartridge.Type == CartridgeType.ROM_ONLY)
      {
        return new RomOnlyMemoryHandler(cartridge);
      }

      // TODO(Cristián): Implement logic for different CartridgeType
      throw new NotImplementedException();
    }
  }
}
