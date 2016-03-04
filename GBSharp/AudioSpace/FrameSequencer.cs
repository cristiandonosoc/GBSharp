using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  public class FrameSequencer
  {
    public uint InternalCounter { get; private set; }

    public uint Value { get; private set; }
    internal bool Clocked { get; private set; }

    internal void Step(uint ticks)
    {
      uint pre = InternalCounter & 0x1FFF;
      InternalCounter += ticks;
      uint post = InternalCounter & 0x1FFF;
      // If the bit 13 changed, the frameSequencer clocked
      Clocked = post < pre;

      Value = InternalCounter >> 13;
    }

    internal void Reset()
    {
      // Next frame sequencer is 0, but the 512 Hz timer remains intact
      InternalCounter |= ~(uint)0x1FFF;
      Value = InternalCounter >> 13;
    }

    internal void AddTicks(uint ticks)
    {
      InternalCounter += ticks;
      Value = InternalCounter >> 13;
    }
  }
}
