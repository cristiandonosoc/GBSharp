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

        protected override void InternalWrite(ushort address, byte value)
        {
            throw new NotImplementedException();
        }

        protected override void InternalWrite(ushort address, ushort value)
        {
            throw new NotImplementedException();
        }

        protected override byte InternalRead(ushort address)
        {
            throw new NotImplementedException();
        }
    }
}
