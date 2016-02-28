using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  class NoiseChannel : IChannel
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

    private bool _enabled;
    public bool Enabled
    {
      get { return _enabled; }
      internal set
      {
        _enabled = value;
        // We update the NR52 byte
        byte nr52 = _memory.LowLevelRead((ushort)MMR.NR52);
        if(_enabled)
        {
          byte mask = (byte)(1 << _channelIndex);
          nr52 |= mask;
        }
        else
        {
          byte mask = (byte)(~(1 << _channelIndex));
          nr52 &= mask;
        }
        _memory.LowLevelWrite((ushort)MMR.NR52, nr52);
      }
    }



    private short _outputValue = 0;

    // TODO(Cristian): Implement
    private const int _volumeConstant = 0;
    public int Volume
    {
      get
      {
        return 0;
      }
    }

    private int _channelIndex;

    internal byte LowFreqByte { get; private set; }
    internal byte HighFreqByte { get; private set; }

    private int _tickThreshold;

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

    internal NoiseChannel(Memory  memory,
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

      _frameSequencerTicks = (int)(GameBoy.ticksPerMillisecond * (double)1000 / (double)512);
      _frameSequencerTickCounter = _frameSequencerTicks;
    }

    private int _frameSequencerTicks;
    private int _frameSequencerTickCounter;

    private int _soundLengthCounter;
    private int _soundLengthPeriod;
    private bool _continuousOutput;

    private int _envelopeTicks;
    private int _envelopeTickCounter;
    private bool _envelopeUp;
    private int _envelopeDefaultValue;
    private bool _envelopeDACOn;

    public void HandleMemoryChange(MMR register, byte value)
    {
      switch (register)
      {
        case MMR.NR41:  // Sound Length
          _soundLengthCounter = 0x3F - (value & 0x3F);

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR42:
          double envelopeMsLength = 1000 * ((double)(value & 0x7) / (double)64);
          _envelopeTicks = (int)(GameBoy.ticksPerMillisecond * envelopeMsLength);
          _envelopeUp = (value & 0x8) != 0;
          _envelopeDefaultValue = value >> 4;

          // Putting volume 0 disables the channel
          if ((_envelopeDefaultValue == 0) && !_envelopeUp)
          {
            _envelopeDACOn = false;
            Enabled = false;
          }
          else
          {
            _envelopeDACOn = true;
          }

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR43:
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR44:

          bool prevContinuousOutput = _continuousOutput;
          _continuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);
          // Only enabling sound length (disabled -> enabled) could trigger a clock
          if ((!_continuousOutput) &&
              (prevContinuousOutput != _continuousOutput))
          {
            // If the next frameSequencer WON'T trigger the length period,
            // the counter is somehow decremented...
            if ((_soundLengthPeriod & 0x01) == 0)
            {
              ClockLengthCounter();
            }
          }

          bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          if (init)
          {
            // NOTE(Cristian): If the length counter is empty at INIT,
            //                 it's reloaded with full length
            if(_soundLengthCounter < 0)
            {
              _soundLengthCounter = 0x3F;

              // If INIT on an zerioed empty enabled length channel
              // AND the next frameSequencer tick WON'T tick the length period
              // The lenght counter is somehow decremented
              if (!_continuousOutput &&
                  ((_soundLengthPeriod & 0x01) == 0))
              {
                ClockLengthCounter();
              }
            }

            if(_envelopeDACOn)
            {
              Enabled = true;
            }
          }

          _memory.LowLevelWrite((ushort)register, value);
          break;
        default:
          throw new InvalidProgramException("Invalid register received.");
      }
    }

    internal void Step(int ticks)
    {
      _frameSequencerTickCounter -= ticks;
      if (_frameSequencerTickCounter <= 0)
      {
        _frameSequencerTickCounter += _frameSequencerTicks;

        // SOUND LENGTH COUNTER
        ++_soundLengthPeriod;
        if ((_soundLengthPeriod & 0x01) == 0)
        {
          // NOTE(Cristian): The length counter runs even when the channel is disabled
          if (!_continuousOutput)
          {
            // We have an internal period
            ClockLengthCounter();
          }
        }
      }
    }

    public void GenerateSamples(int sampleCount)
    {
      while(sampleCount > 0)
      {
        --sampleCount;

        for(int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = _outputValue;
        }
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClockLengthCounter()
    {
      if (_soundLengthCounter >= 0)
      {
        --_soundLengthCounter;
        if (_soundLengthCounter < 0)
        {
          Enabled = false;
        }
      }
    }
  }
}
