﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.MemorySpace;
using System.Diagnostics;

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
    WaveChannel _channel3;

    public bool Enabled { get; private set; }
    public bool LeftChannelEnabled { get; private set; }
    public bool RightChannelEnabled { get; private set; }

    private bool _channel1Run = true;
    private bool _channel2Run = true;
    private bool _channel3Run = false;

    private int _currentWavSamples = 0;
    private const int _wavBufferLength = 44000 * 2 / 1;
    private short[] _wavBuffer1 = new short[_wavBufferLength];
    private short[] _wavBuffer2 = new short[_wavBufferLength];
    private bool _wavBuffer1Active = true;
    private short[] ActiveWavBuffer
    {
      get { return _wavBuffer1Active ? _wavBuffer1 : _wavBuffer2; }
    }

    internal bool WavBufferReady { get; set; }
    internal short[] BackWavBuffer
    {
      get { return _wavBuffer1Active ? _wavBuffer2 : _wavBuffer1; }
    }

#if SoundTiming
    public static Stopwatch swAPU = new Stopwatch();
#endif

    internal APU(Memory memory, int sampleRate, int numChannels, int sampleSize)
    {
      _memory = memory;

      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];
      _tempBuffer = new short[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];

      // We setup the channels
      _channel1 = new SquareChannel(sampleRate, numChannels, sampleSize, 0,
                                    MMR.NR10, MMR.NR11, MMR.NR12, MMR.NR13, MMR.NR14);
      // NOTE(Cristian): Channel 2 doesn't have frequency sweep
      _channel2 = new SquareChannel(sampleRate, numChannels, sampleSize, 1,
                                    0, MMR.NR21, MMR.NR22, MMR.NR23, MMR.NR24);
      _channel3 = new WaveChannel(_memory, sampleRate, numChannels, sampleSize, 2);

      LeftChannelEnabled = true;
      RightChannelEnabled = true;

#if SoundTiming
      swAPU.Start();
#endif
    }

    internal void HandleMemoryChange(MMR register, byte value)
    {
      // We store previous channel status
      bool channel1Enabled = _channel1.Enabled;
      bool channel2Enabled = _channel2.Enabled;
      bool channel3Enabled = _channel3.Enabled;
      bool prevEnabled = Enabled;

      switch (register)
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
        case MMR.NR30:
        case MMR.NR31:
        case MMR.NR32:
        case MMR.NR33:
        case MMR.NR34:
          _channel3.HandleMemoryChange(register, value);
          break;
        // TODO(Cristian): Handle channel 4 memory change
        case MMR.NR50:
          break;
        case MMR.NR51:
          break;
        case MMR.NR52:
          Enabled = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          break;
      }

      // We compare to see if we have to change the NR52 byte
      if ((channel1Enabled != _channel1.Enabled) ||
          (channel2Enabled != _channel2.Enabled) ||
          (channel3Enabled != _channel3.Enabled) ||
          (prevEnabled != Enabled))
      {
        byte nr52 = (byte)((_channel1.Enabled ? 0x1 : 0) |  // bit 0
                           (_channel2.Enabled ? 0x2 : 0) |  // bit 1
                           (_channel3.Enabled ? 0x4 : 0) |  // bit 2
                           (Enabled ? 0x80 : 0));           // bit 7
        _memory.LowLevelWrite((ushort)MMR.NR52, nr52);
      }
    }

    public void GenerateSamples(int sampleCount)
    {
      ClearBuffer();

      int sc = sampleCount / _sampleSize;

      if (Enabled)
      {
        if (_channel1Run && _channel1.Enabled)
        {
          _channel1.GenerateSamples(sc);
        }
        if (_channel2Run && _channel2.Enabled)
        {
          _channel2.GenerateSamples(sc);
        }
        if (_channel3Run && _channel3.Enabled)
        {
          _channel3.GenerateSamples(sc);
        }
      }

      // We transformate the samples
      int _channelSampleIndex = 0;
      for (int i = 0; i < sc; ++i)
      {

        // LEFT CHANNEL
        short leftSample = 0;
        short c1Sample;
        short c2Sample;
        short c3Sample;
        if (Enabled && LeftChannelEnabled)
        {
          // We add the correspondant samples
          if (_channel1Run && _channel1.Enabled)
          {
            c1Sample = _channel1.Buffer[_channelSampleIndex];
            leftSample += c1Sample;
          }
          if (_channel2Run && _channel2.Enabled)
          {
            c2Sample = _channel2.Buffer[_channelSampleIndex];
            leftSample += c2Sample;
          }
          if (_channel3Run && _channel3.Enabled)
          {
            c3Sample = _channel3.Buffer[_channelSampleIndex];
            leftSample += c3Sample;
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
          if (_channel1Run && _channel1.Enabled)
          {
            rightSample += _channel1.Buffer[_channelSampleIndex];
          }
          if (_channel2Run && _channel2.Enabled)
          {
            rightSample += _channel2.Buffer[_channelSampleIndex];
          }
          if (_channel3Run && _channel3.Enabled)
          {
            rightSample += _channel3.Buffer[_channelSampleIndex];
          }
        }
        ++_channelSampleIndex;
        //  TODO(Cristian): post-process mixed sample?
        _buffer[_sampleIndex++] = (byte)rightSample;
        _buffer[_sampleIndex++] = (byte)(rightSample >> 8);

        // We output the wav
        ActiveWavBuffer[_currentWavSamples++] = leftSample;
        ActiveWavBuffer[_currentWavSamples++] = rightSample;

        // We check if we have to change the buffers
        if (_currentWavSamples >= _wavBufferLength)
        {
          _wavBuffer1Active = !_wavBuffer1Active;
          _currentWavSamples = 0;
          WavBufferReady = true;
        }

      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
      _channel1.ClearBuffer();
      _channel2.ClearBuffer();
      _channel3.ClearBuffer();
    }

#if SoundTiming
    ~APU()
    {
      _channel1.WriteOutput();
    }
#endif
  }

}
