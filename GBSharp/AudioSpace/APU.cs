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

    private long _tickCounter = 0;
    private int _msCounter = 0;
    private bool _up = true;

    internal void Step(int ticks)
    {
      _tickCounter += ticks;

      //TODO(Cristian): Update the sound channels

      // We tick until the ms threshold
      while(_tickCounter > GameBoy.ticksPerMillisecond)
      {
        _tickCounter -= GameBoy.ticksPerMillisecond;
        ++_msCounter;

        if(_msCounter >= 500)
        {
          _up = !_up;
        }

        // We should output a ms of samples
        byte value = 0;
        if (_up) { value = 255; }

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

  class SoundChannel
  {
    private int _frequencyFactor;
    internal int FrequencyFactor
    {
      get { return _frequencyFactor; }
      private set
      {
        _frequencyFactor = value;
        _frequency = (double)0x20000 / (double)(0x800 - _frequencyFactor);
      }
    }

    private double _frequency;
    internal double Frequency
    {
      get { return _frequency; }
    }

    internal void LoadFrequencyFactor(byte low, byte high)
    {
      FrequencyFactor = ((high & 0x7) << 8) | low;
    }

    private int _tickCounter = 0;

    internal SoundChannel()
    {

    }

    private int tickThreshold = 200; // ~ 47.6 us (~ 20.97 kHz max)

    internal void Step(int ticks)
    {
      _tickCounter += ticks;
      if(_tickCounter >= tickThreshold)
      {
        _tickCounter -= tickThreshold;
        
        //TODO(Cristian): Update according to frequency
      }
    }
  }
}
