﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.MemorySpace;

namespace GBSharp.AudioSpace
{
  internal class WaveChannel : IChannel
  {
    private int _msSampleRate;
    public int SampleRate { get; private set; }
    public int NumChannels { get; private set; }
    public int SampleSize { get; private set; }
    private int _milliseconds = 1000; // ms of sample

    short[] _buffer;
    public short[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    public bool Enabled { get; private set; }

    private const int _volumeConstant = 1023;
    public int Volume
    {
      get
      {
        if(_volumeRightShift < 0) { return 0; }
        int index = ((_currentSample >> _volumeRightShift) - 7);
        int volume = index * _volumeConstant;
        return volume;
      }
    }

    private int _channelIndex;

    internal byte LowFreqByte { get; private set; }
    internal byte HighFreqByte { get; private set; }

    private int _tickThreshold;
    private int _tickCounter;

    private ushort _frequencyFactor;
    internal ushort FrequencyFactor
    {
      get { return _frequencyFactor; }
      private set
      {
        _frequencyFactor = value;
        LowFreqByte = (byte)_frequencyFactor;
        HighFreqByte = (byte)((_frequencyFactor >> 8) & 0x7);
        Frequency = (double)0x20000 / (double)(0x800 - _frequencyFactor);
        _tickThreshold = (int)(GameBoy.ticksPerMillisecond * (1000.0 / (2 * Frequency)));
      }
    }

    internal double Frequency { get; set; }

    private Memory _memory;

    internal WaveChannel(Memory memory, 
                         int sampleRate, int numChannels, 
                         int sampleSize, int channelIndex)
    {
      _memory = memory;

      SampleRate = sampleRate;
      _msSampleRate = SampleRate / 1000;
      NumChannels = numChannels;
      SampleSize = sampleSize;
      _buffer = new short[SampleRate * NumChannels * SampleSize * _milliseconds / 1000];

      _channelIndex = channelIndex;
    }

    int _soundLengthTicks;
    int _soundLengthTickCounter;

    int _volumeRightShift;

    bool _continuousOutput;
    private short _outputValue;

    public void HandleMemoryChange(MMR register, byte value)
    {
      switch(register)
      {
        case MMR.NR30:  // Sound on/off
          // Last bit determines sound on/off
          Enabled = (Utils.UtilFuncs.TestBit(value, 7) != 0);

          // NR30 is ORed with 0x7F
          _memory.LowLevelWrite((ushort)register, (byte)(value | 0x7f));
          break;
        case MMR.NR31:  // Sound Length
          double soundLengthMs = (double)(0x100 - value) / (double)0x100;
          _soundLengthTicks = (int)(GameBoy.ticksPerMillisecond * soundLengthMs);
          _soundLengthTickCounter = 0;

          // NR31 is ORed with 0xFF
          _memory.LowLevelWrite((ushort)register, 0xFF);
          break;
        case MMR.NR32:  // Output Level (volume)
          // Basically, we shift by this amount.
          // If the amount is 0, it means we mute
          _volumeRightShift = ((value >> 5) & 0x3) - 1;
          // We reload the sample
          //_outputValue = (short)Volume;

          // NR32 is ORed with 0x9F
          _memory.LowLevelWrite((ushort)register, (byte)(value | 0x9F));
          break;
        case MMR.NR33:  // FrequencyFactor lower
          FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | value);

          // NR33 is ORed with 0xFF
          _memory.LowLevelWrite((ushort)register, 0xFF);
          break;
        case MMR.NR34:  // FrequencyFactor higher
          FrequencyFactor = (ushort)(((value & 0x7) << 8) | LowFreqByte);
          _continuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);

          bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          if(init)
          {
            Enabled = true;
          }

          // NR34 is ORed with 0xBF
          _memory.LowLevelWrite((ushort)register, (byte)(value | 0xBF));
          break;
        default:
          throw new InvalidProgramException("Invalid register received.");
      }
    }

    private byte _currentSampleIndex;
    private byte _currentSample;

    public void GenerateSamples(int sampleCount)
    {
      while (sampleCount > 0)
      {
        --sampleCount;

        for (int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = _outputValue;
        }

        _tickCounter -= APU.MinimumTickThreshold;
        if (_tickCounter < 0)
        {
          _tickCounter = _tickThreshold + _tickCounter;
          ++_currentSampleIndex;
          if (_currentSampleIndex >= 32)
          {
            _currentSampleIndex = 0;
          }

          // We get the memory value
          ushort waveRAM = (ushort)(0xFF30 + _currentSampleIndex / 2);
          byte value = _memory.Read(waveRAM);
          // Pair means the first 4 bits,
          // Odd means the last 4 bits
          if ((_currentSampleIndex & 1) == 0)
          {
            _currentSample = (byte)(value >> 4);
          }
          else
          {
            _currentSample = (byte)(value & 0xF);
          }

          _outputValue = (short)Volume;
        }

        /* SOUND LENGTH */
        if (!_continuousOutput)
        {
          _soundLengthTickCounter += APU.MinimumTickThreshold;
          if (_soundLengthTickCounter >= _soundLengthTicks)
          {
            Enabled = false;
          }
        }
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }

  }
}