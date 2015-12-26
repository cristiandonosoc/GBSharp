﻿using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
#if SoundTiming
  internal enum TimelineEvents
  {
    FREQUENCY_CHANGE,
    SOUND_LENGTH_SET,
    SOUND_LENGTH_END
  }
#endif

  internal class SquareChannel
  {
    #region BUFFER DEFINITION

#if SoundTiming
    static long[] Timeline = new long[10000 * 2];
    static uint TimelineCount = 0;
#endif

    private int _msSampleRate;
    public int SampleRate { get; private set; }
    public int NumChannels { get; private set; }
    public int SampleSize { get; private set; }
    private int _milliseconds = 1000; // ms of sample

    short[] _buffer;
    public short[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    internal bool Enabled { get; private set; }

    private int _envelopeVolumeMultiplier = 0;
    private const int _volumeConstant = 511;
    public int Volume {
      get
      {
        return _envelopeVolumeMultiplier * _volumeConstant;
      }
    }

    #endregion

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

#if SoundTiming
        Timeline[TimelineCount++] = APU.swAPU.ElapsedTicks;
        Timeline[TimelineCount++] = (long)TimelineEvents.FREQUENCY_CHANGE;
#endif
      }
    }

    internal double Frequency { get; set; }

    private int _tickCounter = 0;

    private bool _up = false;
    private short _outputValue = 0;

    // Registers
    private MMR _sweepRegister;
    private MMR _wavePatternDutyRegister;
    private MMR _volumeEnvelopeRegister;
    private MMR _freqLowRegister;
    private MMR _freqHighRegister;

    private int _channelIndex;

    private int _soundLengthTicks;
    private int _soundLengthTickCounter;
    private bool _continuousOutput;

    private int _envelopeTicks = 0;
    private int _envelopeTickCounter = 0;
    private bool _envelopeUp;

    private byte _envelopeRegister;

    internal SquareChannel(int sampleRate, int numChannels, int sampleSize, int channelIndex,
                           MMR sweepRegister, MMR wavePatternDutyRegister, MMR volumeEnvelopeRegister, 
                           MMR freqLowRegister, MMR freqHighRegister)
    {
      SampleRate = sampleRate;
      _msSampleRate = SampleRate / 1000;
      NumChannels = numChannels;
      SampleSize = sampleSize;
      _buffer = new short[SampleRate * NumChannels * SampleSize * _milliseconds / 1000];

      _channelIndex = channelIndex;

      // Register setup
      _sweepRegister = sweepRegister;
      _wavePatternDutyRegister = wavePatternDutyRegister;
      _volumeEnvelopeRegister = volumeEnvelopeRegister;
      _freqLowRegister = freqLowRegister;
      _freqHighRegister = freqHighRegister;
    }

    private int _sweepShiftFactor;
    private bool _sweepUp;
    private int _sweepTicks;
    private int _sweepTicksCounter;

    public void HandleMemoryChange(MMR register, byte value)
    {
      if (register == _sweepRegister)
      {
        // Sweep Shift Number (Bits 0-2)
        int sweepShiftNumber = value & 0x7;
        _sweepShiftFactor = 1 << sweepShiftNumber; // 2^sweepShiftNumber

        // Sweep Direction (Bit 3)
        _sweepUp = (value & 0x8) == 0;

        // Sweep Time (Bits 4-6)
        int sweepTime = ((value >> 4) & 0x7);
        double sweepTimeMs = 1000 * ((double)sweepTime / (double)0x200);
        _sweepTicks = (int)(GameBoy.ticksPerMillisecond * sweepTimeMs);
        _sweepTicksCounter = 0;
      }       
      else if (register == _wavePatternDutyRegister)
      {
        // TODO(Cristian): Wave Pattern Duty
        int soundLenghtFactor = value & 0x3F;
        double soundLengthMs = (double)(0x40 - soundLenghtFactor) / (double)0x100;
        _soundLengthTicks = (int)(GameBoy.ticksPerMillisecond * soundLengthMs);

#if SoundTiming
        Timeline[TimelineCount++] = APU.swAPU.ElapsedTicks;
        Timeline[TimelineCount++] = (long)TimelineEvents.SOUND_LENGTH_SET;
#endif
      }
      else if (register == _volumeEnvelopeRegister)
      {
        _envelopeRegister = value;

      }
      else if (register == _freqLowRegister)
      {
        FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | value);
      }
      else if (register == _freqHighRegister)
      {
        FrequencyFactor = (ushort)(((value & 0x7) << 8) | LowFreqByte);

        _continuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);

        Enabled = (Utils.UtilFuncs.TestBit(value, 7) != 0);

        /**
         * Bit 7 is called a channel INIT. On this event the following occurs:
         * - The Volume Envelope values value are changed
         */

        if (Enabled)
        {
          _soundLengthTickCounter = 0;

          double envelopeMsLength = 1000 * ((double)(_envelopeRegister & 0x7) / (double)64);
          _envelopeTicks = (int)(GameBoy.ticksPerMillisecond * envelopeMsLength);
          _envelopeUp = (_envelopeRegister & 0x8) != 0;
          _envelopeVolumeMultiplier = _envelopeRegister >> 4;
        }
      }
      else if (register == MMR.NR52)
      {
        Enabled = (Utils.UtilFuncs.TestBit(value, _channelIndex) != 0);
      }
      else
      {
        throw new InvalidProgramException("Invalid register received");
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

        // The amount of ticks in a sample
        _tickCounter -= APU.MinimumTickThreshold;
        if(_tickCounter < 0)
        {
          _tickCounter = _tickThreshold + _tickCounter;
          _up = !_up;

          _outputValue = (short)(_up ? Volume : -Volume);
        }

        /* FREQUENCY SWEEP */
        if(_sweepTicks > 0)
        {
          _sweepTicksCounter += APU.MinimumTickThreshold;
          if(_sweepTicksCounter > _sweepTicks)
          {
            _sweepTicksCounter -= _sweepTicks;

            if(_sweepUp)
            {
              ushort newFreqFactor = (ushort)(FrequencyFactor + FrequencyFactor / _sweepShiftFactor);
              if(newFreqFactor < 0x80) // Higher than an 11-bit number
              {
                FrequencyFactor = newFreqFactor;
              }
              else
              {
                Enabled = false;
                // TODO(Cristian): Update NR52 bits
              }
            }
            else
            {
              ushort newFreqFactor = (ushort)(FrequencyFactor - FrequencyFactor / _sweepShiftFactor);
              if (newFreqFactor > 0)
              {
                FrequencyFactor = newFreqFactor;
              }
              else
              {
                Enabled = false;
                // TODO(Cristian): Update NR52 bits
              }
            }
          }
        }
        // TODO(Cristian): Implement frequency sweep

        /* SOUND LENGTH DURATION */

        //        if(!_continuousOutput)
        //        {
        //          _soundLengthTickCounter += APU.MinimumTickThreshold;
        //          if(_soundLengthTickCounter >= _soundLengthTicks)
        //          {
        //            Enabled = false;
        //            // TODO(Cristian): Trigger a change to ouput the correct enabled bit
        //            //                 NR52
        //#if SoundTiming
        //            Timeline[TimelineCount++] = APU.swAPU.ElapsedTicks;
        //        		Timeline[TimelineCount++] = (long)TimelineEvents.SOUND_LENGTH_END;
        //#endif
        //          }
        //        }

        /* VOLUME ENVELOPE */
        if (_envelopeTicks > 0)
        {
          _envelopeTickCounter += APU.MinimumTickThreshold;
          if (_envelopeTickCounter > _envelopeTicks)
          {
            _envelopeTickCounter -= _envelopeTicks;
            if (_envelopeUp)
            {
              ++_envelopeVolumeMultiplier;
              if (_envelopeVolumeMultiplier > 15) { _envelopeVolumeMultiplier = 15; }
            }
            else
            {
              --_envelopeVolumeMultiplier;
              if (_envelopeVolumeMultiplier < 0) { _envelopeVolumeMultiplier = 0; }
            }

            _outputValue = (short)(_up ? Volume : -Volume);
          }
        }
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }
    
#if SoundTiming
    public void WriteOutput()
    {
      using (var file = new System.IO.StreamWriter("sound_events.csv", false))
      {
        for (uint i = 0; i < TimelineCount; i += 2)
        {
          file.WriteLine("{0}, {1}",
                         Timeline[i],
                         ((TimelineEvents)Timeline[i + 1]).ToString());
        }
      }
    }
#endif

  }
}
