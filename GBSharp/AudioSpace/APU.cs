using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  /// <summary>
  /// Audio Processing Unit
  /// </summary>
  class APU : IAPU
  {
    private int _sampleRate;
    private int _msSampleRate;
    public int SampleRate { get { return _sampleRate; } }

    private int _numChannels;
    public int NumChannels { get { return _numChannels; } }

    private int _sampleSize;
    public int SampleSize { get { return _sampleSize; } }

    private int _milliseconds = 200; // ms of sample

    byte[] _buffer;
    public byte[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    internal APU(int sampleRate, int numChannels, int sampleSize)
    {
      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];
    }

    private const double targetMillisecondsPerTick = 0.0002384185791015625; // It is know that this is 2^-22.
    private const int ticksPerMillisecond = 4194; // Actually it's 4194,304

    private int _tickCounter = 0;
    private int _msCounter = 0;
    private bool _positive = true;

    internal void Step(int ticks)
    {
      _tickCounter += ticks;
      if(_tickCounter > ticksPerMillisecond)
      {
        _tickCounter -= ticksPerMillisecond;
        ++_msCounter;
        if(_msCounter >= 500)
        {
          _positive = !_positive;
        }

        // We should output a ms of samples
        byte value = 0;
        if (_positive) { value = 255; }

        for(int i = 0; i < _msSampleRate; ++i)
        {
          for (int c = 0; c < _numChannels; ++c)
          {
            _buffer[_sampleIndex++] = value;
          }
        }
      }
    }

    internal void ClearBuffer()
    {
      _sampleIndex = 0;
    }

  }

}
