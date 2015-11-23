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

    /// <summary>
    /// This is the amount of ticks needed to output a single sample
    /// ~ 22 kHz max frequency
    /// </summary>
    internal static int MinimumTickThreshold = 96; 

    private int _sampleRate;
    private int _msSampleRate;
    public int SampleRate { get { return _sampleRate; } }

    private int _numChannels;
    public int NumChannels { get { return _numChannels; } }

    private int _sampleSize;
    public int SampleSize { get { return _sampleSize; } }

    private int _milliseconds = 1000; // ms of sample

    byte[] _buffer;
    public byte[] Buffer { get { return _channel.Buffer; } }

    // TODO(Cristian): Join channels to make an unified sound channel
    private int _sampleIndex;
    public int SampleCount { get { return _channel.SampleCount; } }

    SoundChannel _channel;

    internal APU(int sampleRate, int numChannels, int sampleSize)
    {
      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];

      _channel = new SoundChannel(sampleRate, numChannels, sampleSize);
      _channel.LoadFrequencyFactor(0x4F, 0x7);
    }

    internal void Step(int ticks)
    {
      _channel.Step(ticks);
    }

    internal void ClearBuffer()
    {
      _sampleIndex = 0;
      _channel.ClearBuffer();
    }

  }

}
