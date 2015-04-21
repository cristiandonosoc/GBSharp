using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Memory
{
    class Memory : IMemory
    {
        /// <summary>
        /// This is what can be addressed.
        /// </summary>
        byte[] data = new byte[65536];

        /// <summary>
        /// Class constructor, initializes everything to 0.
        /// </summary>
        internal Memory()
        {
            for (int i = 0; i < this.data.Length; ++i)
            {
                this.data[i] = 0;
            }
        }

        /// <summary>
        /// Writes 8 bits value to memory.
        /// </summary>
        /// <param name="address">16 bits address.</param>
        /// <param name="value">8 bits value.</param>
        internal void Write(ushort address, byte value)
        {
            // TODO: Perform I/O and block magics?
            // TODO: Notify writes, maybe this is the same.
            this.data[address] = value;
        }

        /// <summary>
        /// Writes 16 bit value to memory.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <param name="value">16 bit value.</param>
        internal void Write(ushort address, ushort value)
        {
            this.data[address] = (byte)(value & 0x00FF);
            this.data[address + 1] = (byte)(value >> 8);
        }

        /// <summary>
        /// Reads 8 bits value from memory.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <returns>8 bit value located at Mem[address]</returns>
        internal byte Read(ushort address)
        {
            return this.data[address];
        }

        #region External memory interface
        event Action IMemory.ValuesChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        byte[] IMemory.Values
        {
            get { return this.data; }
        }
        #endregion
    }
}
