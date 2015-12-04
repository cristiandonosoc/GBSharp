﻿using System;
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

    private short[] _tempBuffer;
    byte[] _buffer;
    public byte[] Buffer { get { return _buffer; } }

    // TODO(Cristian): Join channels to make an unified sound channel
    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    private Memory _memory;

    SquareChannel _channel1;
    SquareChannel _channel2;

    public bool Enabled { get; private set; }
    public bool LeftChannelEnabled { get; private set; }
    public bool RightChannelEnabled { get; private set; }

    internal APU(Memory memory, int sampleRate, int numChannels, int sampleSize)
    {
      _memory = memory;

      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];
      _tempBuffer = new short[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];

      _channel1 = new SquareChannel(sampleRate, numChannels, sampleSize, 0,
                                    MMR.NR10, MMR.NR11, MMR.NR12, MMR.NR13, MMR.NR14);
      // NOTE(Cristian): Channel 2 doesn't have frequency sweep
      _channel2 = new SquareChannel(sampleRate, numChannels, sampleSize, 1,
                                    0, MMR.NR21, MMR.NR22, MMR.NR23, MMR.NR24);
      LeftChannelEnabled = true;
      RightChannelEnabled = true;
    }

    internal void HandleMemoryChange(MMR register, byte value)
    {
      switch(register)
      {
        case MMR.NR10:
        case MMR.NR11:
        case MMR.NR12:
        case MMR.NR13:
        case MMR.NR14:
          _channel1.HandleMemoryChange(register, value);
          break;
        case MMR.NR21:
        case MMR.NR22:
        case MMR.NR23:
        case MMR.NR24:
          _channel2.HandleMemoryChange(register, value);
          break;
        // TODO(Cristian): Handle Channel 3/4 changes
        case MMR.NR50:
          break;
        case MMR.NR51:
          break;
        case MMR.NR52:
          Enabled = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          break;
      }
    }

    public void GenerateSamples(int sampleCount)
    {
      ClearBuffer();

      int sc = sampleCount / _sampleSize;

      if(Enabled)
      {
        _channel1.GenerateSamples(sc);
        _channel2.GenerateSamples(sc);
      }

      // We transformate the samples
      int _channelSampleIndex = 0;
      for (int i = 0; i < sc; ++i)
      {

        // LEFT CHANNEL
        short leftSample = 0;
        if (Enabled && LeftChannelEnabled)
        {
          // We add the correspondant samples
          if (_channel1.Enabled)
          {
            leftSample += _channel1.Buffer[_channelSampleIndex];
          }
          if (_channel1.Enabled)
          {
            leftSample += _channel2.Buffer[_channelSampleIndex];
          }
        }
        ++_channelSampleIndex;
        //  TODO(Cristian): post-process mixed sample?
        _buffer[_sampleIndex++] = (byte)leftSample;
        _buffer[_sampleIndex++] = (byte)(leftSample >> 8);

        // RIGHT CHANNEL
        short rightSample = 0;
        if (Enabled && RightChannelEnabled)
        {
          // We add the correspondant samples
          if (_channel1.Enabled)
          {
            rightSample += _channel1.Buffer[_channelSampleIndex];
          }
          if (_channel1.Enabled)
          {
            rightSample += _channel2.Buffer[_channelSampleIndex];
          }
        }
        ++_channelSampleIndex;
        //  TODO(Cristian): post-process mixed sample?
        _buffer[_sampleIndex++] = (byte)rightSample;
        _buffer[_sampleIndex++] = (byte)(rightSample >> 8);
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
