using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class FrameSequencer
  {
    private uint _internalCounter;

    internal uint Value { get; private set; }
    internal bool Clocked { get; private set; }

    internal void Step(uint ticks)
    {
      uint pre = _internalCounter & 0x1FFF;
      _internalCounter += ticks;
      uint post = _internalCounter & 0x1FFF;
      // If the bit 13 changed, the frameSequencer clocked
      Clocked = post < pre;

      Value = _internalCounter >> 13;
    }
  }
}
