using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class SoundEventQueue
  {
    private int _readCursor = 0;
    private int _writeCursor = 0;
    private int _elementCount = 0;
    internal int ElementCount { get { return _elementCount; } }
    private object _lockObj = new object();

    private long[] _ticksBuffer;
    private short[] _outputValuesBuffer;

    private int _bufferSize = 10000;

    internal SoundEventQueue()
    {
      _ticksBuffer = new long[_bufferSize];
      _outputValuesBuffer = new short[_bufferSize];
    }

    internal void Queue(long ticks, short outputValue)
    {
      lock(_lockObj)
      {
        if (_elementCount == _bufferSize) { throw new Exception("Event Queue overflow"); }
        _ticksBuffer[_writeCursor] = ticks;
        _outputValuesBuffer[_writeCursor] = outputValue;
        ++_writeCursor;
        if(_writeCursor == _bufferSize) { _writeCursor = 0; }
        ++_elementCount;
      }
    }

    internal bool GetNextEvent(ref int eventId, ref long ticks, ref short outputValue)
    {
      lock(_lockObj)
      {
        if (_elementCount == 0) { return false; }
        // If we have an element waiting, we unqueue
        --_elementCount;
      }
      eventId = _readCursor;
      ticks = _ticksBuffer[_readCursor];
      outputValue = _outputValuesBuffer[_readCursor];
      ++_readCursor;
      if(_readCursor == _bufferSize) { _readCursor = 0; }
      return true;
    }

    private long _currentTickCounter = 0;


    internal short GetNextOutputValue(int advancedTicks)
    {
      if (_elementCount == 0)
      {
        return _outputValuesBuffer[_readCursor];
      }
      else
      {
        if (_currentTickCounter < _ticksBuffer[_readCursor])
        {
          return _outputValuesBuffer[_readCursor];
        }
        else
        {
          lock(_lockObj)
          {
            --_elementCount;
          }
          _currentTickCounter -= _ticksBuffer[_readCursor];

        }

      }

      return 0;
    }
  }


}
