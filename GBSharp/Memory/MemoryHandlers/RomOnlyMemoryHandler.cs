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
    public class RomOnlyMemoryHandler : MemoryHandler
    {
        public RomOnlyMemoryHandler(GBSharp.Catridge.Cartridge cartridge)
            : base(cartridge)
        {
        }

        public override void LoadInternalMemory(byte[] data)
        {
            base.LoadInternalMemory(data);
            // We copy the ROM areas to the internal memory of the GB
            ushort min = 0x0000;
            ushort max = 0x7FFF;

            // TODO(Cristián): Perform fast memory copy
            for (ushort i = min;
                i < max;
                i++)
            {
                internalMemory[i] = this.cartridge.Data[i];
            }
        }

        protected override void InternalWrite(ushort address, byte value)
        {
            internalMemory[address] = value;
        }

        protected override void InternalWrite(ushort address, ushort value)
        {
            internalMemory[address] = (byte)(value & 0x00FF);
            internalMemory[address + 1] = (byte)(value >> 8);
        }

        protected override byte InternalRead(ushort address)
        {
            return internalMemory[address];
        }
    }
}
