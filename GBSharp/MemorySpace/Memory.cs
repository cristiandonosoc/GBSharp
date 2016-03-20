using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;
using GBSharp.MemorySpace.MemoryHandlers;

namespace GBSharp.MemorySpace
{
  class Memory : IMemory, IDisposable
  {
    // TODO(Cristian): This GB timing, not GBC
    // TODO(Cristian): Unify this value throughout the program!!!
    static double usPerTick = 0.2384185791015625; // 2^-22
    static ushort targetTickCount = (ushort)(Math.Ceiling(160 / usPerTick));

    public ushort MemoryChangedLow { get; private set; }
    public ushort MemoryChangedHigh { get; private set; }

    class State
    {
      /// <summary>
      /// This is what can be addressed.
      /// </summary>
      internal byte[] Data;
      
      // DMA
      internal ushort DmaStartAddress;
      internal ushort DmaCurrentTickCount;
      internal bool DmaActive;
    }
    State _state = new State();

    // This is an pointer needed by the DMA
    internal byte[] MemoryData
    {
      get { return _state.Data; }
    }

    /// <summary>
    /// The class that is going to handle the memory writes depending on the cartridge type.
    /// </summary>
    private MemoryHandler memoryHandler;

    /// <summary>
    /// Class constructor, does nothing.
    /// </summary>
    internal Memory()
    {
      Reset();
    }

    internal void Reset()
    {
      _state.Data = new byte[65536];
      _state.DmaActive = false;
    }

    /// <summary>
    /// The DMA transfers 160 bytes from the address/0x100
    /// into the addresses 0xFF00-0xFF9F (the OAM table).
    /// This process takes ~160us and in that time CPU can
    /// only access the "High" RAM (0xFF80-FFFE)
    /// </summary>
    /// <param name="source">
    /// The source address/0x100
    /// (i.e if the source address is to be 0x8800, 
    /// then this input is 88)
    /// </param>
    internal void StartDma(byte source)
    {
      if(_state.DmaActive) {
        throw new InvalidOperationException(
          "There should not be a DMA start during DMA transfer");
      }
      _state.DmaStartAddress = (ushort)(source << 8);
      _state.DmaCurrentTickCount = 0;
      _state.DmaActive = true;
    }

    /// <summary>
    /// Updates the copy process according to the ellapsed amount of time since the last update.
    /// A full copy takes ~ 160 microseconds.
    /// </summary>
    /// <param name="ticks">Clock ticks since last update.</param>
    internal void Step(byte ticks)
    {
      if (!_state.DmaActive) { return; }

      // NOTE(Cristian): Right now, the ticks of the write to the DMA address
      //                 are being updated to this count.
      //                 This is because this moves the timing closer to what
      //                 the expected ticks are
      _state.DmaCurrentTickCount += ticks;
      if(_state.DmaCurrentTickCount >= targetTickCount)
      {
        // We copy the result of the DMA
        Buffer.BlockCopy(_state.Data, _state.DmaStartAddress,
                         _state.Data, 0xFE00,
                         0xA0);
        _state.DmaCurrentTickCount = 0;
        _state.DmaActive = false;

        // We notify the display that the sprites must be sorted
        // We use the memoryHandler as it has the display reference
        memoryHandler.DmaReady();
      }
    }

    internal void SaveState()
    {

    }

    internal void LoadState()
    {

    }

    /// <summary>
    /// Clears the internal memory.
    /// </summary>
    private void ClearMemory()
    {
      for (int i = 0; i < _state.Data.Length; ++i)
      {
        _state.Data[i] = 0;
      }
    }

    /// <summary>
    /// Sets the memory handler that is going to be used by this memory.
    /// </summary>
    /// <param name="memoryHandler">A reference to an initialized memory handler.</param>
    internal void SetMemoryHandler(MemoryHandler memoryHandler)
    {
      this.memoryHandler = memoryHandler;
    }

    /// <summary>
    /// Writes 8 bits value to memory.
    /// </summary>
    /// <param name="address">16 bits address.</param>
    /// <param name="value">8 bits value.</param>
    internal void Write(ushort address, byte value)
    {
      this.memoryHandler.Write(address, value);
      MemoryChangedLow = address;
      MemoryChangedLow = address;
    }

    /// <summary>
    /// Writes 16 bit value to memory.
    /// </summary>
    /// <param name="address">16 bit address.</param>
    /// <param name="value">16 bit value.</param>
    internal void Write(ushort address, ushort value)
    {
      this.memoryHandler.Write(address, value);
      MemoryChangedLow = address;
      MemoryChangedLow = ++address;
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
      _state.Data[address] = value;
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
      byte result = _state.Data[address];
      return result;
    }

    internal byte[] LowLevelArrayRead(ushort address, int length)
    {
      byte[] result = new byte[length];
      for (int i = 0; i < length; i++)
      {
        result[i] = _state.Data[address++];
      }
      return result;
    }

    #region External memory interface

    byte[] IMemory.Data
    {
      get { return _state.Data; }
    }

    public void Load(byte[] data)
    {
      _state.Data = data;
    }

    public void Dispose()
    {
      if (memoryHandler != null)
      {
        memoryHandler.Dispose();
      }
    }

    #endregion
  }
}
