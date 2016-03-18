using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class SoundEvent
  {
    internal long TickDiff { get; set; }
    internal int Kind { get; set; }
    internal int Value { get; set; }
  }

  internal class SoundEventQueue
  {
    int _size;
    SoundEvent[] _events;

    int _writeBuffer;
    int _readBuffer;

    internal SoundEventQueue(int size)
    {
      _size = size;
      _events = new SoundEvent[_size];
      // We initialize all the fuckers
      for (int i = 0; i < _size; ++i)
      {
        _events[i] = new SoundEvent();
      }
    }

    internal void AddSoundEvent(long ticks, int kind, int value, int channelIndex)
    {
      _events[_writeBuffer].TickDiff = ticks;
      _events[_writeBuffer].Kind = kind;
      _events[_writeBuffer].Value = value;
#if SoundTiming
        if (channelIndex == 3)
        {
          APU.TimelineLocal[APU.TimelineLocalCount++] = APU.sw.ElapsedMilliseconds;
          APU.TimelineLocal[APU.TimelineLocalCount++] = kind;
          APU.TimelineLocal[APU.TimelineLocalCount++] = value;
        }

#endif
      if (_writeBuffer + 1 == _size)
      {
        _writeBuffer = 0;
      }
      else
      {
        ++_writeBuffer;
      }
    }

    internal bool GetNextEvent(ref SoundEvent soundEvent)
    {
      if (_readBuffer == _writeBuffer) { return false; }

      soundEvent.TickDiff = _events[_readBuffer].TickDiff;
      soundEvent.Kind = _events[_readBuffer].Kind;
      soundEvent.Value = _events[_readBuffer].Value;
      ++_readBuffer;
      if (_readBuffer == _size)
      {
        _readBuffer = 0;
      }

      return true;
    }
  }
}
