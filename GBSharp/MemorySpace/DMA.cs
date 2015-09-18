using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace
{
  class DMA
  {
    byte[] memoryData;
    bool active = false;
    ushort startAddress;
    ushort currentTickCount;
    // TODO(Cristian): This GB timing, not GBC
    // TODO(Cristian): Unify this value throughout the program!!!
    static double usPerTick = 0.2384;
    ushort targetTickCount = (ushort)(Math.Ceiling(160 / usPerTick));

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="memory">A reference to the virtual memory array.</param>
    internal DMA(byte[] memory)
    {
      this.memoryData = memory;
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
      if(active) { return; } // Defensive programming :]
      startAddress = (ushort)(source << 8);
      currentTickCount = 0;
      active = true;
    }

    /// <summary>
    /// Updates the copy process according to the ellapsed amount of time since the last update.
    /// A full copy takes ~ 160 microseconds.
    /// </summary>
    /// <param name="ticks">Clock ticks since last update.</param>
    internal void Step(byte ticks)
    {
      if (!active) { return; }
      currentTickCount += ticks;
      if(currentTickCount >= targetTickCount)
      {
        throw new NotImplementedException();
      }
    }
  }
} 