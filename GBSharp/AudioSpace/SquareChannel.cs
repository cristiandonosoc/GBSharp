using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    private int _soundLengthFactor;
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
        _soundLengthFactor = value & 0x3F;
        _soundLengthTickCounter = CalculateSoundLengthTicks(_soundLengthFactor);

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

          if(_soundLengthTickCounter == 0)
          {
            _soundLengthFactor = 0;
            _soundLengthTickCounter = CalculateSoundLengthTicks(_soundLengthFactor);
          }

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

    internal void Step(int ticks)
    {
      if (!Enabled) { return; }

      // The amount of ticks in a sample
      _tickCounter += ticks;
      if (_tickCounter >= _tickThreshold)
      {
        _up = !_up;
        _outputValue = (short)(_up ? Volume : -Volume);
        _tickCounter -= _tickThreshold;
      }

      #region FREQUENCY SWEEP

      _runSweep = false;
      if (_runSweep && _sweepTicks > 0)
      {
        _sweepTicksCounter += ticks;
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

      #endregion

      #region SOUND LENGTH DURATION

      if (_runSoundLength && !_continuousOutput)
      {
        _soundLengthTickCounter -= ticks;
        if (_soundLengthTickCounter <= 0)
        {
          _soundLengthTickCounter = 0;
          Enabled = false;
        }
      }

      #endregion

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

    private int CalculateSoundLengthTicks(int soundLengthFactor)
    {
      double soundLengthMs = (double)(1000 * ((0x40 - soundLengthFactor) / (double)0x100));
      return (int)(GameBoy.ticksPerMillisecond * soundLengthMs);
    }


  }
}
