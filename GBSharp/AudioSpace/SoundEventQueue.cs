using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class SoundEvent
  {
    internal long TickDiff;
    internal int Threshold;
    internal int Volume;
  }

  internal class SoundEventQueue
  {
    int _size;
    SoundEvent[] _events;

    int _writeBuffer;
    int _readBuffer;

    internal SoundEventQueue(int size)
    {
      _events = new SoundEvent[size];
      // We initialize all the fuckers
      for (int i = 0; i < _size; ++i)
      {
        _events[i] = new SoundEvent();
      }
    }

    internal void AddSoundEvent(long ticks, int threshold, int volume, int channelIndex)
    {
      _events[_writeBuffer].TickDiff = ticks;
      _events[_writeBuffer].Threshold = threshold;
      _events[_writeBuffer].Volume = volume;
#if SoundTiming
        if (channelIndex == 0)
        {
          SquareChannel.TimelineLocal[SquareChannel.TimelineLocalCount++] = SquareChannel.sw.ElapsedMilliseconds;
          SquareChannel.TimelineLocal[SquareChannel.TimelineLocalCount++] = ticks;
          SquareChannel.TimelineLocal[SquareChannel.TimelineLocalCount++] = threshold;
          SquareChannel.TimelineLocal[SquareChannel.TimelineLocalCount++] = volume;
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
      soundEvent.Threshold = _events[_readBuffer].Threshold;
      soundEvent.Volume = _events[_readBuffer].Volume;
      ++_readBuffer;
      if (_readBuffer == _size)
      {
        _readBuffer = 0;
      }

      return true;
    }
  }
}
