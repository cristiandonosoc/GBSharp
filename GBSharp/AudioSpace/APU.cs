using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.MemorySpace;

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
    public byte[] Buffer { get { return _buffer; } }

    private uint[] _tempBuffer;

    // TODO(Cristian): Join channels to make an unified sound channel
    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    private Memory _memory;

    SquareChannel _channel1;
    SquareChannel _channel2;

    internal APU(Memory memory, int sampleRate, int numChannels, int sampleSize)
    {
      _memory = memory;

      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];
      _tempBuffer = new uint[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];

      _channel1 = new SquareChannel(sampleRate, numChannels, sampleSize);
      _channel2 = new SquareChannel(sampleRate, numChannels, sampleSize);
    }

    // TODO(Cristian): Do this on memory change
    internal void UpdateChannels()
    {
      // We check if any of the channels changed
      _channel1.LoadFrequencyFactor(_memory.LowLevelRead((ushort)MMR.NR13),
                                    _memory.LowLevelRead((ushort)MMR.NR14));

      _channel2.LoadFrequencyFactor(_memory.LowLevelRead((ushort)MMR.NR23),
                                    _memory.LowLevelRead((ushort)MMR.NR24));
    }

    public void GenerateSamples(int sampleCount)
    {
      ClearBuffer();

      int sc = sampleCount / 2;

      _channel1.GenerateSamples(sc);
      _channel2.GenerateSamples(sc);

      // We transformate the samples
      int _channelSampleIndex = 0;
      for(int i = 0; i < sc; ++i)
      {
        for(int c = 0; c < _numChannels; ++c)
        {
          short sample1 = _channel1.Buffer[_channelSampleIndex];
          short sample2 = _channel2.Buffer[_channelSampleIndex];
          ++_channelSampleIndex;

          //  TODO(Cristian): post-process mixed sample?

          short finalSample = (short)(sample1 + sample2);
          _buffer[_sampleIndex++] = (byte)finalSample;
          _buffer[_sampleIndex++] = (byte)(finalSample >> 8);
        }
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
      _channel1.ClearBuffer();
      _channel2.ClearBuffer();
    }

  }

}
