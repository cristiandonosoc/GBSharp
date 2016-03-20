using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace
{
  class DMA
  {
    internal event Action DMAReady;

    Memory _memory;
    class State
    {
      internal ushort StartAddress;
      internal ushort CurrentTickCount;
      internal bool Active;
    }
    State _state = new State();

    // TODO(Cristian): This GB timing, not GBC
    // TODO(Cristian): Unify this value throughout the program!!!
    static double usPerTick = 0.2384185791015625; // 2^-22
    static ushort targetTickCount = (ushort)(Math.Ceiling(160 / usPerTick));

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="memory">
    /// A reference to the memory. 
    /// This is so we always access the updated memory pointer
    /// (it could change, say, because of state loading)
    /// </param>
    internal DMA(Memory memory)
    {
      _state.Active = false;
      _memory = memory;
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
    internal void Start(byte source)
    {
      if(_state.Active) {
        throw new InvalidOperationException(
          "There should not be a DMA start during DMA transfer");
      }
      _state.StartAddress = (ushort)(source << 8);
      _state.CurrentTickCount = 0;
      _state.Active = true;
    }

    /// <summary>
    /// Updates the copy process according to the ellapsed amount of time since the last update.
    /// A full copy takes ~ 160 microseconds.
    /// </summary>
    /// <param name="ticks">Clock ticks since last update.</param>
    internal void Step(byte ticks)
    {
      if (!_state.Active) { return; }

      // NOTE(Cristian): Right now, the ticks of the write to the DMA address
      //                 are being updated to this count.
      //                 This is because this moves the timing closer to what
      //                 the expected ticks are
      _state.CurrentTickCount += ticks;
      if(_state.CurrentTickCount >= targetTickCount)
      {
        // We copy the result of the DMA
        byte[] memoryData = _memory.MemoryData;
        Buffer.BlockCopy(memoryData, _state.StartAddress,
                         memoryData, 0xFE00,
                         0xA0);
        _state.CurrentTickCount = 0;
        _state.Active = false;

        // We notify the display that the sprites must be sorted
        if(DMAReady != null)
        {
          DMAReady();
        }
      }
    }
  }
} 