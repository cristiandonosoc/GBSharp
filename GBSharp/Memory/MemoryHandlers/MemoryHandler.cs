using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Catridge;

namespace GBSharp.Memory.MemoryHandlers
{
    /// <summary>
    /// A memory handler contains the logic for the management
    /// of the memory for a certain catdridge type. Different
    /// catdridges handle memory mapping and storing in
    /// different ways, but should be transparent to the CPU.
    /// </summary>
    public abstract class MemoryHandler
    {
        #region ATTRIBUTES

        protected byte[] internalMemory;
        protected Cartridge cartridge;

        #endregion

        #region CONSTRUCTORS

        public MemoryHandler(Cartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        #endregion

        #region PUBLIC METHODS

        public virtual void LoadInternalMemory(byte[] data)
        {
          this.internalMemory = data;
        }

        /// <summary>
        /// Writes 8 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bits address.</param>
        /// <param name="value">8 bits value.</param>
        abstract public void Write(ushort address, byte value);

        /// <summary>
        /// Writes 16 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <param name="value">16 bit value.</param>
        abstract public void Write(ushort address, ushort value);

        /// <summary>
        /// Reads 8 bit value from memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <returns>
        /// 8 bit value located at the address
        /// correspondant to the memory management scheme.
        /// </returns>
        abstract public byte Read(ushort address);

        #endregion
    }
}
