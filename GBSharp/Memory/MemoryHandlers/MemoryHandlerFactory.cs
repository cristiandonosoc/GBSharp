using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Catridge;

namespace GBSharp.Memory.MemoryHandlers
{
    class MemoryHandlerFactory
    {
        /// <summary>
        /// Creates a MemoryHandler the matches the CartridgeType.
        /// </summary>
        /// <param name="cartridge">The cartridge to be loaded into the GB</param>
        /// <returns>A MemoryHandler instance</returns>
        public static MemoryHandler CreateMemoryHandler(Cartridge cartridge)
        {
            // TODO(Cristián): Implement logic for different CartridgeType
            return new RomOnlyMemoryHandler(cartridge);
        }
    }
}
