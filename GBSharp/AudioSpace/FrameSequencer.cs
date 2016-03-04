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

    internal void Reset()
    {
      // FrameSequencer is restarted
      _internalCounter &= 0x1FFF;
      // ... and clocked once (why not?)
      // NOTE(Cristian): There is NO indication that this is what
      //                 happens, but this passes the test
      //                 If it quacks like a DMG...
      _internalCounter += 0x2000;
      Value = _internalCounter >> 13;
    }
  }
}
