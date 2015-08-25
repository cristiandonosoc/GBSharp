using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace
{
  class DMA
  {
    byte[] memory;
    bool copying = false;
    ushort startAddress;
    ushort progress;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="memory">A reference to the virtual memory array.</param>
    internal DMA(byte[] memory)
    {
      this.memory = memory;
    }

    /// <summary>
    /// Updates the copy process according to the ellapsed amount of time since the last update.
    /// A full copy takes ~ 160 microseconds.
    /// </summary>
    /// <param name="ticks">Clock ticks since last update.</param>
    internal void Step(byte ticks)
    {
      if (!copying)
      {
        return;
      }
    }

  }
}
