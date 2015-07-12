using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;

namespace GBSharp.MemorySpace.MemoryHandlers
{
    /// <summary>
    /// A memory handler contains the logic for the management
    /// of the memory for a certain catdridge type. Different
    /// catdridges handle memory mapping and storing in
    /// different ways, but should be transparent to the CPU.
    /// </summary>
    abstract class MemoryHandler
    {
        #region ATTRIBUTES

        protected byte[] internalMemory;
        protected Cartridge.Cartridge cartridge;

        #endregion

        #region CONSTRUCTORS

        internal MemoryHandler(Cartridge.Cartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        #endregion

        #region PUBLIC METHODS

        internal virtual void LoadInternalMemory(byte[] data)
        {
          this.internalMemory = data;
        }

        /// <summary>
        /// Writes 8 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bits address.</param>
        /// <param name="value">8 bits value.</param>
        abstract internal void Write(ushort address, byte value);

        /// <summary>
        /// Writes 16 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <param name="value">16 bit value.</param>
        abstract internal void Write(ushort address, ushort value);

        /// <summary>
        /// Reads 8 bit value from memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <returns>
        /// 8 bit value located at the address
        /// correspondant to the memory management scheme.
        /// </returns>
        abstract internal byte Read(ushort address);

        #endregion
    }
}
