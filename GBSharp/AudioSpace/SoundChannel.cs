using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  class SoundChannel
  {
    #region BUFFER DEFINITION

    private int _sampleRate;
    private int _msSampleRate;
    public int SampleRate { get { return _sampleRate; } }

    private int _numChannels;
    public int NumChannels { get { return _numChannels; } }

    private int _sampleSize;
    public int SampleSize { get { return _sampleSize; } }

    private int _milliseconds = 1000; // ms of sample

    byte[] _buffer;
    public byte[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    #endregion

    private int _frequencyFactor;
    internal int FrequencyFactor
    {
      get { return _frequencyFactor; }
      private set
      {
        _frequencyFactor = value;
        _frequency = (double)0x20000 / (double)(0x800 - _frequencyFactor);
        _tickThreshold = (int)(GameBoy.ticksPerMillisecond * (1000.0 / _frequency));
        _tickCounter = _tickThreshold;
      }
    }

    private int _tickThreshold;

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
    private int _outputTickCounter = 0;

    private bool _up = false;
    private byte _outputValue = 0;

    internal SoundChannel(int sampleRate, int numChannels, int sampleSize)
    {
      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];
    }

    internal void Step(int ticks)
    {
      _outputTickCounter += ticks;

      while (_outputTickCounter >= APU.MinimumTickThreshold)
      {
        _outputTickCounter -= APU.MinimumTickThreshold;
        _tickCounter -= APU.MinimumTickThreshold;

        // We output a sample
        for (int c = 0; c < _numChannels; ++c)
        {
          _buffer[_sampleIndex++] = _outputValue;
        }

        // We check if we need to change the value
        if (_tickCounter < 0)
        {
          _tickCounter = _tickThreshold;
          _up = !_up;
          _outputValue = (byte)(_up ? 255 : 0);
        }
      }
    }

    internal void ClearBuffer()
    {
      _sampleIndex = 0;
    }
  }
}
