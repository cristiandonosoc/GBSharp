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

        /// <summary>
        /// The minimum address that is addressable for
        /// this memory manager.
        /// </summary>
        protected ushort minAddress = 0x0000;
        /// <summary>
        /// The maximum address that is addressable for
        /// this memory manager.
        /// </summary>
        protected ushort maxAddress = 0xFFFF;

        #endregion

        #region CONSTRUCTORS

        public MemoryHandler(Cartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        #endregion

        #region PROTECTED METHODS

        /// <summary>
        /// Validates that the address given is within the 
        /// valid range for the memory manager. The range
        /// is defined with the minAddress and maxAddress
        /// variables
        /// </summary>
        /// <param name="address">A 16 bit address</param>
        protected void ValidateAddress(ushort address)
        {
          if (address < minAddress || maxAddress < address)
          {
            throw new ArgumentOutOfRangeException();
          }
        }

        #endregion


        #region PUBLIC METHODS

        /// <summary>
        /// Writes 8 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bits address.</param>
        /// <param name="value">8 bits value.</param>
        public void Write(ushort address, byte value)
        {
            ValidateAddress(address);
            InternalWrite(address, value);
        }

        /// <summary>
        /// Writes 16 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <param name="value">16 bit value.</param>
        public void Write(ushort address, ushort value)
        {
            ValidateAddress(address);
            InternalWrite(address, value);
        }

        /// <summary>
        /// Reads 8 bit value from memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <returns>
        /// 8 bit value located at the address
        /// correspondant to the memory management scheme.
        /// </returns>
        public byte Read(ushort address)
        {
            ValidateAddress(address);
            return InternalRead(address);
        }

        #endregion

        #region ABSTRACT METHODS

        public virtual void LoadInternalMemory(byte[] data)
        {
            this.internalMemory = data;
        }

        /// <summary>
        /// Writes 8 bits value to memory according to
        /// the correspondant memory management scheme.
        /// Only called within a public Write method call.
        /// </summary>
        /// <param name="address">16 bits address.</param>
        /// <param name="value">8 bits value.</param>
        abstract protected void InternalWrite(ushort address, byte value);

        /// <summary>
        /// Writes 16 bits value to memory according to
        /// the correspondant memory management scheme.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <param name="value">16 bit value.</param>
        abstract protected void InternalWrite(ushort address, ushort value);

        /// <summary>
        /// Reads 8 bit value from memory according to
        /// the correspondant memory management scheme.
        /// Only called within a public Read method call.
        /// </summary>
        /// <param name="address">16 bit address.</param>
        /// <returns>
        /// 8 bit value located at the address
        /// correspondant to the memory management scheme.
        /// </returns>
        abstract protected byte InternalRead(ushort address);

        #endregion
    }
}
