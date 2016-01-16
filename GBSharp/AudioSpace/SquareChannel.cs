﻿using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class SquareChannel : IChannel
  {
    private Memory _memory;
    private SoundEventQueue _eventsQueue;

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

    public bool Enabled { get; internal set; }

    #endregion

    internal byte LowFreqByte { get; private set; }
    internal byte HighFreqByte { get; private set; }

    private int _latencyTicks;
    private int _latencyTicksLeft;
    private bool _latencySimulated;
    internal void SetLatencyTicks(int latencyTicks)
    {
      _latencyTicks = latencyTicks;
      _latencyTicksLeft = _latencyTicks;
      _latencySimulated = false;
    }


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


    #region VOLUME / VOLUME ENVELOPE

    /**
     * These are the values that are currently set by the volume envelope registers.
     * These are the values that will become 'live' on the next channel INIT
     */
    private int _envelopeTicks;
    private int _envelopeTickCounter;
    private bool _envelopeUp;
    private int _defaultEnvelopeValue;

    /**
     * These are the values that are currently 'live' in the channel.
     * These could be (or not) the same values that are loaded in the envelope register.
     */
    private int _currentEnvelopeTicks;
    private bool _currentEnvelopeUp;
    private int _currentEnvelopeValue;

    private const int _volumeConstant = 511;
    public int Volume {
      get
      {
        return _currentEnvelopeValue * _volumeConstant;
      }
    }

    #endregion

    internal SquareChannel(Memory memory,
                           int sampleRate, int numChannels, int sampleSize, int channelIndex,
                           MMR sweepRegister, MMR wavePatternDutyRegister, MMR volumeEnvelopeRegister,
                           MMR freqLowRegister, MMR freqHighRegister)
    {
      _memory = memory;
      _eventsQueue = new SoundEventQueue();

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

    private bool _sweepEnabled = false;
    private ushort _sweepFrequencyFactor;
    private int _sweepShiftNumber;
    private bool _sweepUp;
    private int _sweepTicks;
    private int _sweepTicksCounter;

    // DEBUG FLAGS
    private bool _runSweep = true;
    private bool _runSoundLength = true;
    private bool _runVolumeEnvelope = true;

    public void HandleMemoryChange(MMR register, byte value)
    {
      byte before = _memory.LowLevelRead((ushort)register);
#if SoundTiming
      //Timeline[TimelineCount++] = (long)register;
      //Timeline[TimelineCount++] = value;
#endif

      if (register == _sweepRegister)
      {
        // Sweep Shift Number (Bits 0-2)
        _sweepShiftNumber = value & 0x7;

        // Sweep Direction (Bit 3)
        _sweepUp = (value & 0x8) == 0;

        // Sweep Time (Bits 4-6)
        int sweepTime = ((value >> 4) & 0x7);
        double sweepTimeMs = 1000 * ((double)sweepTime / (double)0x200);
        _sweepTicks = (int)(GameBoy.ticksPerMillisecond * sweepTimeMs);
        _sweepTicksCounter = 0;

        // NR10 is read as ORed with 0x80
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x80));
      }
      else if (register == _wavePatternDutyRegister)
      {
        // TODO(Cristian): Wave Pattern Duty
        int soundLenghtFactor = value & 0x3F;
        double soundLengthMs = (double)(0x40 - soundLenghtFactor) / (double)0x100;
        _soundLengthTicks = (int)(GameBoy.ticksPerMillisecond * soundLengthMs);

        // NR(1,2)1 values are read ORed with 0x3F
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x3F));
      }
      else if (register == _volumeEnvelopeRegister)
      {

        double envelopeMsLength = 1000 * ((double)(value & 0x7) / (double)64);
        _envelopeTicks = (int)(GameBoy.ticksPerMillisecond * envelopeMsLength);
        _envelopeUp = (value & 0x8) != 0;
        _defaultEnvelopeValue = value >> 4;

        // NR(1,2)2 values are read ORed with 0x00
        _memory.LowLevelWrite((ushort)register, value);
      }
      else if (register == _freqLowRegister)
      {
        FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | value);

        _memory.LowLevelWrite((ushort)register, value);

        // NR(1,2)3 values are read ORed with 0xFF
        _memory.LowLevelWrite((ushort)register, 0xFF);
      }
      else if (register == _freqHighRegister)
      {
        FrequencyFactor = (ushort)(((value & 0x7) << 8) | LowFreqByte);

        _continuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);

        bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);

        /**
         * Bit 7 is called a channel INIT. On this event the following occurs:
         * - The Volume Envelope values value are changed
         */

        if (init)
        {
          Enabled = true;

          // Tick Counter is restarted
          _tickCounter = 0;

          // Frequency Sweep
          _sweepFrequencyFactor = FrequencyFactor;
          _sweepTicksCounter = 0;
          _sweepEnabled = ((_sweepShiftNumber != 0) && (_sweepTicks != 0));
          // Apparently frequency change is runned immediately if there is a
          // shift number
          if (_sweepShiftNumber > 0)
          {
            _sweepTicksCounter = _sweepTicks;
          }

          // Sound Length timer is reloaded
          _soundLengthTickCounter = 0;

          // Envelope is reloaded
          _currentEnvelopeTicks = _envelopeTicks;
          _currentEnvelopeUp = _envelopeUp;
          _currentEnvelopeValue = _defaultEnvelopeValue;
          _envelopeTickCounter = 0;
        }

        // NRx4 values are read ORed with 0xBF
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0xBF));
      }
      else if (register == MMR.NR52)
      {
        // NOTE(Cristian): This register is written at the APU level
        Enabled = (Utils.UtilFuncs.TestBit(value, _channelIndex) != 0);
      }
      else
      {
        throw new InvalidProgramException("Invalid register received");
      }

#if SoundTiming
      //Timeline[TimelineCount++] = before;
      //Timeline[TimelineCount++] = _memory.LowLevelRead((ushort)register);
#endif
    }

    long _stepCounter = 0;
    long _totalStepCounter = 0;
    internal void Step(int ticks)
    {
      _stepCounter += ticks;
      _totalStepCounter += ticks;

      // We check how much latency we need to simulate
      if(_latencyTicksLeft > 0)
      {
        _latencyTicksLeft -= ticks;
        if(_latencyTicksLeft <= 0)
        {
          _latencyTicksLeft = 0;
          _latencySimulated = true;
        }
      }

      // The amount of ticks in a sample
      _tickCounter += ticks;
      if (_tickCounter >= _tickThreshold)
      {
        _up = !_up;
        short outputValue = (short)(_up ? Volume : -Volume);
        _eventsQueue.Queue(_stepCounter, outputValue);
#if SoundTiming
        //Timeline[TimelineCount++] = _stepCounter;
        //Timeline[TimelineCount++] = outputValue;
#endif

        _tickCounter -= _tickThreshold;
        _stepCounter = _tickCounter;
      }

      

#if false
      /* FREQUENCY SWEEP */
      if (_runSweep && _sweepTicks > 0)
      {
        _sweepTicksCounter += APU.MinimumTickThreshold;
        if (_sweepTicksCounter > _sweepTicks)
        {
          _sweepTicksCounter -= _sweepTicks;

          if (_sweepShiftNumber == 0)
          {
            // If the shift number is 0, the channel stops when it's time to
            // shift
            Enabled = false;
          }
          else
          {
            if (_sweepUp)
            {
              int newFreqFactor = FrequencyFactor +
                                  (_sweepFrequencyFactor >> _sweepShiftNumber);
              if (newFreqFactor < 0x800) // Higher than an 11-bit number
              {
                _sweepFrequencyFactor = (ushort)newFreqFactor;
                FrequencyFactor = _sweepFrequencyFactor;
              }
              else
              {
                // Overflow stops the channel
                Enabled = false;
              }
            }
            else
            {
              int newFreqFactor = FrequencyFactor -
                                     (_sweepFrequencyFactor >> _sweepShiftNumber);
              if (newFreqFactor > 0)
              {
                _sweepFrequencyFactor = (ushort)newFreqFactor;
                FrequencyFactor = _sweepFrequencyFactor;
              }
            }
          }
        }
      }

      /* SOUND LENGTH DURATION */

      if (_runSoundLength && !_continuousOutput)
      {
        _soundLengthTickCounter += APU.MinimumTickThreshold;
        if (_soundLengthTickCounter >= _soundLengthTicks)
        {
          Enabled = false;
        }
      }

      /* VOLUME ENVELOPE */
      if (_runVolumeEnvelope && _envelopeTicks > 0)
      {
        _envelopeTickCounter += APU.MinimumTickThreshold;
        if (_envelopeTickCounter > _envelopeTicks)
        {
          _envelopeTickCounter -= _envelopeTicks;
          if (_envelopeUp)
          {
            ++_currentEnvelopeValue;
            if (_currentEnvelopeValue > 15) { _currentEnvelopeValue = 15; }
          }
          else
          {
            --_currentEnvelopeValue;
            if (_currentEnvelopeValue < 0) { _currentEnvelopeValue = 0; }
          }

          _outputValue = (short)(_up ? Volume : -Volume);
        }
      }
#endif
    }

    long _samplesStepCounter = 0;
    long _totalSamplesStepCounter = 0;

    int _currentEventId = 999999999;
    long _currentStepThreshold = 0;
    short _currentOutputValue = 0;

    int _nextEventId = 999999999;
    long _nextStepThreshold = 0;
    short _nextOutputValue = 0;

    bool _waitingForNextEvent = true;

    public void GenerateSamples(int sampleCount)
    {
      while (sampleCount > 0)
      {
        // We see if the next event was met
        --sampleCount;
        if(!_waitingForNextEvent)
        {
          _totalSamplesStepCounter += APU.MinimumTickThreshold;
        }

        for (int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = _currentOutputValue;
        }

        if(!_latencySimulated) { continue; }

        // We see if we need to change the output value according to the event queue
        if (_waitingForNextEvent)
        {
          // GetNextEvent is true is there is one event in the loop
          if (_eventsQueue.GetNextEvent(ref _nextEventId,
                                        ref _nextStepThreshold,
                                        ref _nextOutputValue))

          {
#if SoundTiming
            //Timeline[TimelineCount++] = _samplesStepCounter;
            //Timeline[TimelineCount++] = _nextStepThreshold;
            //Timeline[TimelineCount++] = _eventsQueue.ElementCount;
            Timeline[TimelineCount++] = _totalStepCounter;
            Timeline[TimelineCount++] = _totalSamplesStepCounter;
            Timeline[TimelineCount++] = _totalStepCounter - _totalSamplesStepCounter;
#endif
            _waitingForNextEvent = false;
          }
        }

        if (!_waitingForNextEvent)
        {

          _samplesStepCounter += APU.MinimumTickThreshold;

          // If we haven't passed the next event waiting, 
          // we check if the threshold was met
          if (_samplesStepCounter >= _nextStepThreshold)
          {
            _samplesStepCounter %= _nextStepThreshold;
            _waitingForNextEvent = true;

            // We copy the event
            _currentEventId = _nextEventId;
            _currentStepThreshold = _nextStepThreshold;
            _currentOutputValue = _nextOutputValue;
          }
        }

#if false
        /* FREQUENCY SWEEP */
        if(_runSweep && _sweepTicks > 0)
        {
          _sweepTicksCounter += APU.MinimumTickThreshold;
          if(_sweepTicksCounter > _sweepTicks)
          {
            _sweepTicksCounter -= _sweepTicks;

            if (_sweepShiftNumber == 0)
            {
              // If the shift number is 0, the channel stops when it's time to
              // shift
              Enabled = false;
            }
            else
            {
              if (_sweepUp) 
              {
                int newFreqFactor = FrequencyFactor +
                                    (_sweepFrequencyFactor >> _sweepShiftNumber);
                if (newFreqFactor < 0x800) // Higher than an 11-bit number
                {
                  _sweepFrequencyFactor = (ushort)newFreqFactor;
                  FrequencyFactor = _sweepFrequencyFactor;
                }
                else
                {
                  // Overflow stops the channel
                  Enabled = false;
                }
              }
              else
              {
                int newFreqFactor = FrequencyFactor -
                                       (_sweepFrequencyFactor >> _sweepShiftNumber);
                if (newFreqFactor > 0)
                {
                  _sweepFrequencyFactor = (ushort)newFreqFactor;
                  FrequencyFactor = _sweepFrequencyFactor;
                }
              }
            }
          }
        }

        /* SOUND LENGTH DURATION */

        if (_runSoundLength && !_continuousOutput)
        {
          _soundLengthTickCounter += APU.MinimumTickThreshold;
          if (_soundLengthTickCounter >= _soundLengthTicks)
          {
            Enabled = false;
          }
        }

        /* VOLUME ENVELOPE */
        if (_runVolumeEnvelope && _envelopeTicks > 0)
        {
          _envelopeTickCounter += APU.MinimumTickThreshold;
          if (_envelopeTickCounter > _envelopeTicks)
          {
            _envelopeTickCounter -= _envelopeTicks;
            if (_envelopeUp)
            {
              ++_currentEnvelopeValue;
              if (_currentEnvelopeValue > 15) { _currentEnvelopeValue = 15; }
            }
            else
            {
              --_currentEnvelopeValue;
              if (_currentEnvelopeValue < 0) { _currentEnvelopeValue = 0; }
            }

            _outputValue = (short)(_up ? Volume : -Volume);
          }
        }
#endif
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }
    
#if SoundTiming
    public void WriteOutput()
    {
      try
      {
        using (var file = new StreamWriter("sound_events.csv", false))
        {
          file.WriteLine("{0},{1}", "Ticks", "Value");
          for (uint i = 0; i < TimelineCount; i += 3)
          {
            file.WriteLine("{0},{1},{2}",
                           Timeline[i],
                           //"0x" + Timeline[i + 1].ToString("x2").ToUpper());
                           Timeline[i + 1],
                           Timeline[i + 2]);
          }
        }
      }
      catch(IOException)
      {
        // Probably because the csv is opened by a program. Whatever...
      }
    }
#endif

  }
}
