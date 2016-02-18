using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class SquareChannel : IChannel
  {
    private Memory _memory;
    #region BUFFER DEFINITION

#if SoundTiming
    static long[] Timeline = new long[10000 * 2];
    static uint TimelineCount = 0;
    static Stopwatch sw = new Stopwatch();
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
      _latencySimulated = true;
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

    #region VOLUME / VOLUME ENVELOPE

    /**
     * These are the values that are currently set by the volume envelope registers.
     * These are the values that will become 'live' on the next channel INIT
     */
    private int _envelopeTicks;
    private int _envelopeTickCounter;
    private bool _envelopeUp;
    private int _envelopeDefaultValue;
    private bool _envelopeDACOn;

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

      _frameSequencerTicks = (int)(GameBoy.ticksPerMillisecond * (double)1000 / (double)512);
      _frameSequencerTickCounter = _frameSequencerTicks;
    }

    private bool _sweepEnabled;
    private int _sweepPeriod;
    private int _sweepFrequencyRegister;
    private int _sweepTicks;
    private int _sweepTickCounter;
    private int _sweepShifts;
    private bool _sweepUp;

    // DEBUG FLAGS
    private bool _runSweep = true;
    private bool _runSoundLength = true;
    private bool _runVolumeEnvelope = true;


    private int _frameSequencerTicks;
    private int _frameSequencerTickCounter;

    private int _soundLengthCounter;
    private byte _soundLengthPeriod;
    private bool _continuousOutput;

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
        _sweepShifts = value & 0x07;

        // Sweep Direction (Bit 3)
        _sweepUp = ((value & 0x08) == 0);

        // Sweep Time (Bits 4-6)
        _sweepTicks = ((value >> 4) & 0x07);

        // NR10 is read as ORed with 0x80
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x80));
      }
      else if (register == _wavePatternDutyRegister)
      {
        // TODO(Cristian): Wave Pattern Duty
        _soundLengthCounter = 0x3F - (value & 0x3F);

        // NR(1,2)1 values are read ORed with 0x3F
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x3F));
      }
      else if (register == _volumeEnvelopeRegister)
      {
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

        // Bit 7 is called a channel INIT. On this event the following occurs:
        // * The Volume Envelope values value are changed
        bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
        // INIT triggers only if the DAC (volume) is on
        if (init)
        {
          if(_envelopeDACOn)
          {
            Enabled = true;
          }

          // Tick Counter is restarted
          _tickCounter = 0;

          // FREQUENCY SWEEP
          _sweepFrequencyRegister = FrequencyFactor;
          _sweepTickCounter = _sweepTicks;
          _sweepEnabled = ((_sweepTicks > 0) || (_sweepShifts > 0));
          // We create immediate frequency calculation
          if (_sweepShifts > 0)
          {
            int freqChange = _sweepFrequencyRegister;
            freqChange >>= _sweepShifts;
            if (!_sweepUp) { freqChange *= -1; }
            int newFreq = _sweepFrequencyRegister + freqChange;
            if (newFreq >= 0x800)
            {
              Enabled = false;
            }
          }

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

          // Envelope is reloaded
          _currentEnvelopeTicks = _envelopeTicks;
          _currentEnvelopeUp = _envelopeUp;
          _currentEnvelopeValue = _envelopeDefaultValue;
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

    internal void Step(int ticks)
    {
      _frameSequencerTickCounter -= ticks;
      if (_frameSequencerTickCounter <= 0)
      {
        _frameSequencerTickCounter += _frameSequencerTicks;

        // We check which internal element ticket

        // SOUND LENGTH
        // Ticks every two frame sequencer ticks
        ++_soundLengthPeriod;
        if ((_soundLengthPeriod & 0x01) == 0)
        {
          // NOTE(Cristian): The length counter runs even when the channel is disabled
          if (_runSoundLength && !_continuousOutput)
          {
            // We have an internal period
            ClockLengthCounter();
          }
        }

        #region FREQUENCY SWEEP

        // FREQUENCY SWEEP
        // Ticks every 8 frame sequencer ticks
        ++_sweepPeriod;
        if ((_sweepPeriod & 0x07) == 0x07)
        {
          if (_sweepEnabled)
          {
            --_sweepTickCounter;
            if ((_sweepTickCounter == 0) && _sweepEnabled)
            {
              // We restart the sweep counter
              _sweepTickCounter = _sweepTicks;

              if (_sweepShifts > 0)
              {
                int freqChange = _sweepFrequencyRegister;
                freqChange >>= _sweepShifts;
                if (!_sweepUp) { freqChange *= -1; }
                _sweepFrequencyRegister += freqChange;

                // Overflows turns off the channel
                if (_sweepFrequencyRegister >= 0x800)
                {
                  Enabled = false;
                }
                else
                {
                  FrequencyFactor = (ushort)_sweepFrequencyRegister;
                }
              }
            }
          }
        }

        #endregion

      }

      if (!Enabled) { return; }

      // The amount of ticks in a sample
      _tickCounter += ticks;
      if (_tickCounter >= _tickThreshold)
      {
        _up = !_up;
        _outputValue = (short)(_up ? Volume : -Volume);
        _tickCounter -= _tickThreshold;
      }


      #region VOLUME ENVELOPE

      if (_runVolumeEnvelope && _envelopeTicks > 0)
      {
        _envelopeTickCounter += ticks;
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

      #endregion

    }

    private int _sampleTickCount = 0;
    private int _sampleTickThreshold = 0;
    private bool _sampleUp = false;
    private int _sampleVolume = 0;

    public void GenerateSamples(int fullSamples)
    {
      // We obtain the status of the APU
      int newTicksThreshold = _tickThreshold;
      int newVolume = Volume;

      int fullSamplesCount = fullSamples;
      while(fullSamplesCount > 0)
      {
        if (Enabled)
        {
          _sampleTickCount += APU.MinimumTickThreshold;
          if (_sampleTickCount >= _sampleTickThreshold)
          {
            _sampleTickThreshold = newTicksThreshold;
            _sampleTickCount -= _sampleTickThreshold;
            _sampleVolume = newVolume;
            _sampleUp = !_sampleUp;
          }

          for(int c = 0; c < NumChannels; ++c)
          {
            _buffer[_sampleIndex++] = (short)(_sampleUp ? _sampleVolume : -_sampleVolume);
          }
        }
        else
        {
          _buffer[_sampleIndex++] = 0;
        }

        --fullSamplesCount;
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
    
#if SoundTiming
    public void WriteOutput()
    {
      try
      {
        using (var file = new StreamWriter("sound_events.csv", false))
        {
          //file.WriteLine("{0},{1}", "Ticks", "Value");
          for (uint i = 0; i < TimelineCount; i += 3)
          {
            //file.WriteLine("{0},{1},{2}",
            //               Timeline[i],
            //               //"0x" + Timeline[i + 1].ToString("x2").ToUpper());
            //               Timeline[i + 1],
            //               Timeline[i + 2]);
            file.WriteLine("{0},{1},{2}", Timeline[i]*0.0002384,
                                          Timeline[i + 1]*0.0002384,
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
