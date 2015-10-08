using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;
using GBSharp.MemorySpace.MemoryHandlers;

namespace GBSharp.MemorySpace
{
  class Memory : IMemory
  {

    public ushort LastChangedStart { get; private set; }
    public ushort LastChangedEnd { get; private set; }

    /// <summary>
    /// This is what can be addressed.
    /// </summary>
    private byte[] data;
    private DMA dma;

    /// <summary>
    /// The class that is going to handle the memory writes depending on the cartridge type.
    /// </summary>
    private MemoryHandler memoryHandler;

    /// <summary>
    /// Class constructor, does nothing.
    /// </summary>
    internal Memory()
    {
      LastChangedStart = 0;
      LastChangedEnd = 0;
      data = new byte[65536];

      this.dma = new DMA(this.data);
    }

    internal void Step(byte ticks)
    {
      this.dma.Step(ticks);
    }

    /// <summary>
    /// Clears the internal memory.
    /// </summary>
    private void ClearMemory()
    {
      for (int i = 0; i < this.data.Length; ++i)
      {
        data[i] = 0;
      }
    }

    /// <summary>
    /// Sets the memory handler that is going to be used by this memory.
    /// </summary>
    /// <param name="memoryHandler">A reference to an initialized memory handler.</param>
    internal void SetMemoryHandler(MemoryHandler memoryHandler)
    {
      this.memoryHandler = memoryHandler;
      this.memoryHandler.UpdateMemoryReference(this.data, this.dma);
    }

    /// <summary>
    /// Writes 8 bits value to memory.
    /// </summary>
    /// <param name="address">16 bits address.</param>
    /// <param name="value">8 bits value.</param>
    internal void Write(ushort address, byte value)
    {
      this.memoryHandler.Write(address, value);
    }

    /// <summary>
    /// Writes 16 bit value to memory.
    /// </summary>
    /// <param name="address">16 bit address.</param>
    /// <param name="value">16 bit value.</param>
    internal void Write(ushort address, ushort value)
    {
      this.memoryHandler.Write(address, value);
    }

    /// <summary>
    /// Writes 8 bits value to memory without performing a call to the memory manager.
    /// Use this method to simulate changes in the underlying hardware that is being mapped to an address.
    /// For example, an input port is not writable by the cpu core instructions, but an external hardware
    /// change like a button press may change this value.
    /// </summary>
    /// <param name="address">16 bits address.</param>
    /// <param name="value">8 bits value.</param>
    internal void LowLevelWrite(ushort address, byte value)
    {
      this.data[address] = value;
    }

    /// <summary>
    /// Reads 8 bits value from memory.
    /// </summary>
    /// <param name="address">16 bit address.</param>
    /// <returns>8 bit value located at Mem[address]</returns>
    internal byte Read(ushort address)
    {
      return this.memoryHandler.Read(address);
    }

    internal byte LowLevelRead(ushort address)
    {
      byte result = this.data[address];
      return result;
    }

    internal byte[] LowLevelArrayRead(ushort address, int length)
    {
      byte[] result = new byte[length];
      for (int i = 0; i < length; i++)
      {
        result[i] = this.data[address++];
      }
      return result;
    }

    #region External memory interface

    byte[] IMemory.Data
    {
      get { return this.data; }
    }

    public void Load(byte[] data)
    {
      this.data = data;
    }

    #endregion
  }
}
