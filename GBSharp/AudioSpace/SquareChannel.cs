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
  internal enum SquareChannelEvents
  {
    THRESHOLD_CHANGE,
    VOLUME_CHANGE
  }


  internal class SquareChannel : IChannel, ISquareChannel
  {
    
#if SoundTiming
    internal static long[] TimelineLocal = new long[10000 * 4];
    internal static uint TimelineLocalCount = 0;
    internal static long[] TimelineSoundThread = new long[10000 * 3];
    internal static uint TimelineSoundThreadCount = 0;
    internal static Stopwatch sw = new Stopwatch();
#endif

    private Memory _memory;
    #region BUFFER DEFINITION

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
        // This is the counter used to output sound
        _tickThreshold = (0x800 - _frequencyFactor) / 2;
        AddSoundEvent(SquareChannelEvents.THRESHOLD_CHANGE, _tickThreshold);
      }
    }

    internal double Frequency { get; set; }

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
    private int CurrentEnvelopeValue
    {
      get { return _currentEnvelopeValue; }
      set
      {
        _currentEnvelopeValue = value;
        AddSoundEvent(SquareChannelEvents.VOLUME_CHANGE, Volume);
      }
    }

    private const int _volumeConstant = 511;
    public int Volume {
      get
      {
        return CurrentEnvelopeValue * _volumeConstant;
      }
    }

    #endregion

    private FrameSequencer _frameSequencer;

    private SoundEventQueue _eventQueue;
    private long _tickDiff;

    internal SquareChannel(Memory memory, FrameSequencer frameSequencer,
                           int sampleRate, int numChannels, int sampleSize, int channelIndex,
                           MMR sweepRegister, MMR wavePatternDutyRegister, MMR volumeEnvelopeRegister,
                           MMR freqLowRegister, MMR freqHighRegister)
    {
      _memory = memory;
      _frameSequencer = frameSequencer;

      SampleRate = sampleRate;
      _msSampleRate = SampleRate / 1000;
      NumChannels = numChannels;
      SampleSize = sampleSize;
      _buffer = new short[SampleRate * NumChannels * SampleSize * _milliseconds / 1000];

      _channelIndex = channelIndex;
      _eventQueue = new SoundEventQueue(1000);

      // Register setup
      _sweepRegister = sweepRegister;
      _wavePatternDutyRegister = wavePatternDutyRegister;
      _volumeEnvelopeRegister = volumeEnvelopeRegister;
      _freqLowRegister = freqLowRegister;
      _freqHighRegister = freqHighRegister;
#if SoundTiming
      if (_channelIndex == 0)
      {
        sw.Start();
      }
#endif

    }

    private bool _sweepEnabled;
    private bool _sweepCalcOcurred;
    public int SweepFrequencyRegister { get; private set; }
    public int SweepLength { get; private set; }
    public int SweepCounter { get; private set; }
    public int SweepShifts { get; private set; }

    public bool SweepUp { get; private set; }

    // DEBUG FLAGS
    private bool _runSoundLength = true;
    private bool _runVolumeEnvelope = true;

    public int SoundLengthCounter { get; private set; }
    public bool ContinuousOutput { get; private set; }

    private void AddSoundEvent(SquareChannelEvents soundEvent, int value)
    {
        _eventQueue.AddSoundEvent(_tickDiff, (int)soundEvent, value, _channelIndex);
        _tickDiff = 0;
    }

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
        SweepShifts = value & 0x07;

        // Sweep Direction (Bit 3)
        bool prevSweepUp = SweepUp;
        SweepUp = ((value & 0x08) == 0);
        // Going from neg->pos after a calc has occurred disabled the channel
        if (_sweepCalcOcurred && !prevSweepUp && SweepUp)
        {
          Enabled = false;
        }
        _sweepCalcOcurred = false;

        // Sweep Time (Bits 4-6)
        SweepLength = ((value >> 4) & 0x07);

        // Bit 7 is always 1
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x80));
      }
      else if (register == _wavePatternDutyRegister)
      {
        // TODO(Cristian): Wave Pattern Duty
        SoundLengthCounter = 0x3F - (value & 0x3F);

        _memory.LowLevelWrite((ushort)register, value);
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

        _memory.LowLevelWrite((ushort)register, value);
      }
      else if (register == _freqLowRegister)
      {
        FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | value);
        _memory.LowLevelWrite((ushort)register, value);
      }
      else if (register == _freqHighRegister)
      {
        FrequencyFactor = (ushort)(((value & 0x7) << 8) | LowFreqByte);

        bool prevContinuousOutput = ContinuousOutput;
        ContinuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);
        // Only enabling sound length (disabled -> enabled) could trigger a clock
        if ((!ContinuousOutput) &&
            (prevContinuousOutput != ContinuousOutput))
        {
          // If the next frameSequencer WON'T trigger the length period,
          // the counter is somehow decremented...
          if ((_frameSequencer.Value & 0x01) == 0)
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

          // FREQUENCY SWEEP
          SweepFrequencyRegister = FrequencyFactor;
          SweepCounter = SweepLength;

          if (SweepLength == 0) { SweepCounter = 8; }
          _sweepEnabled = ((SweepLength > 0) || (SweepShifts > 0));
          // We create immediate frequency calculation
          if (SweepShifts > 0)
          {
            CalculateSweepChange(updateValue: false, redoCalculation: false);
            _sweepCalcOcurred = true;
          }

          // NOTE(Cristian): If the length counter is empty at INIT,
          //                 it's reloaded with full length
          if(SoundLengthCounter < 0)
          {
            SoundLengthCounter = 0x3F;

            // If INIT on an zerioed empty enabled length channel
            // AND the next frameSequencer tick WON'T tick the length period
            // The lenght counter is somehow decremented
            if (!ContinuousOutput &&
                ((_frameSequencer.Value & 0x01) == 0))
            {
              ClockLengthCounter();
            }
          }

          // Envelope is reloaded
          _currentEnvelopeTicks = _envelopeTicks;
          _currentEnvelopeUp = _envelopeUp;
          CurrentEnvelopeValue = _envelopeDefaultValue;
          _envelopeTickCounter = 0;
        }

        // Bits 3-5 are always 1
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x38));
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

    public void PowerOff()
    {
      // Sweep
      if (_sweepRegister != 0)
      {
        SweepShifts = 0;
        SweepUp = true;
        _sweepCalcOcurred = false;
        SweepLength = 0;
        _memory.LowLevelWrite((ushort)_sweepRegister, 0);
      }

      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)_wavePatternDutyRegister, 0x00);


      // Volume Envelope
      _envelopeTicks = 0;
      _envelopeTickCounter = 0;
      _envelopeUp = false;
      _envelopeDefaultValue = 0;
      _memory.LowLevelWrite((ushort)_volumeEnvelopeRegister, 0);

      // Frequency-Low
      FrequencyFactor = 0x00;
      ContinuousOutput = true;
      _memory.LowLevelWrite((ushort)_freqLowRegister, 0);
      _memory.LowLevelWrite((ushort)_freqHighRegister, 0);
    }

    public void ChangeLength(byte value)
    {
      SoundLengthCounter = 0x3F - (value & 0x3F);
      // Only length part changes
      byte prevValue = _memory.LowLevelRead((ushort)_wavePatternDutyRegister);
      _memory.LowLevelWrite((ushort)_wavePatternDutyRegister, (byte)((prevValue & 0xC0) | (value & 0x3F)));
    }


    int _tickCount = 0;
    int _tickDivider;

    internal void Step(int ticks)
    {
      _tickDiff += ticks;

      if (_frameSequencer.Clocked)
      { 
        // We check which internal element ticket

        // SOUND LENGTH
        // Clocks at 256 Hz (every two frame sequencer ticks)
        if ((_frameSequencer.Value & 0x01) == 0)
        {
          // NOTE(Cristian): The length counter runs even when the channel is disabled
          if (_runSoundLength && !ContinuousOutput)
          {
            // We have an internal period
            ClockLengthCounter();
          }
        }

        #region FREQUENCY SWEEP

        // FREQUENCY SWEEP
        // Clocks at 128 Hz (every four frame sequencer ticks)
        if ((_frameSequencer.Value & 0x03) == 0x02)
        {
          if (_sweepEnabled)
          {
            --SweepCounter;
            if (SweepCounter == 0)
            {
              if (SweepLength > 0)
              {
                CalculateSweepChange(updateValue: true, redoCalculation: true);
                _sweepCalcOcurred = true;
              }

              // We restart the sweep counter
              SweepCounter = SweepLength;
              if (SweepLength == 0) { SweepCounter = 8; }
            }
          }
        }

        #endregion

      }

      if (!Enabled) { return; }

      _tickDivider -= ticks;
      if (_tickDivider < 0)
      {
        _tickDivider += 32;

        --_tickCount;
        if (_tickCount <= 0)
        {
          _tickCount += _tickThreshold;
        }
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
            ++CurrentEnvelopeValue;
            if (CurrentEnvelopeValue > 15) { CurrentEnvelopeValue = 15; }
          }
          else
          {
            --CurrentEnvelopeValue;
            if (CurrentEnvelopeValue < 0) { CurrentEnvelopeValue = 0; }
          }
        }
      }

      #endregion

    }

    // We copy the state into the channel when we start generating input
    // This means that the channel can continue to emulate while the generation
    // thread outputs a snapshot of the frequecy and volume.
    // This approach works only when the GenerateSample is called frequently
    // and asks for relatively few samples each time
    private int _sampleTickCount = 0;
#pragma warning disable 414 // Disabling never used warning
    private int _sampleTickThreshold = 0;
    private int _sampleVolume = 0;
#pragma warning restore 414
    private bool _sampleUp = false;
    private int _sampleTimerDivider = 0;

    public void GenerateSamples(int fullSamples)
    {
      // We obtain the status of the APU
      int _sampleTickThreshold = _tickThreshold;
      int _sampleVolume = Volume;

      int fullSamplesCount = fullSamples;
      while(fullSamplesCount > 0)
      {
        if (Enabled)
        {
          // We simulate the hardware for that amount of ticks
          _sampleTimerDivider -= APU.MinimumTickThreshold;
          while (_sampleTimerDivider < 0)
          {
            // Timer divider is a 5 bit counter that clocks the 11-bit frequency counter
            _sampleTimerDivider += 32; 

            --_sampleTickCount;
            if (_sampleTickCount <= 0)
            {
              _sampleTickCount += _sampleTickThreshold;
              _sampleUp = !_sampleUp;
            }
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

    long _eventTickCounter;
    long _eventOnHoldCounter;
    bool _eventAlreadyRun = true;
    SoundEvent _currentEvent = new SoundEvent();

    int _newSampleTickCounter;
    int _newSampleTickThreshold;
    int _newSampleVolume;
    bool _newSampleUp;

    long _newSampleTimerDivider;

    public void GenerateSamples2(int fullSamples)
    {
      int fullSampleCount = fullSamples;
      while (fullSampleCount > 0)
      {
        // We how many ticks will pass this sample
        long ticks = APU.MinimumTickThreshold;

        // If the event already run, we try to see if there is a new one
        if (_eventAlreadyRun)
        {
          if (_eventQueue.GetNextEvent(ref _currentEvent))
          {
            _eventTickCounter = _currentEvent.TickDiff;
            _eventAlreadyRun = false;
          }
          else
          {
            _eventOnHoldCounter += ticks;
          }
        }
        else
        {
          // We need to substract the on hold time
          _eventOnHoldCounter += ticks;
          if (_eventTickCounter > _eventOnHoldCounter)
          {
            _eventTickCounter -= _eventOnHoldCounter;
          }
          else
          {
            _eventTickCounter = 0;
          }
          _eventOnHoldCounter = 0;

          if (_eventTickCounter <= 0)
          {
            switch((SquareChannelEvents)_currentEvent.Kind)
            {
              case SquareChannelEvents.THRESHOLD_CHANGE:
                _newSampleTickThreshold = _currentEvent.Value;
                break;
              case SquareChannelEvents.VOLUME_CHANGE:
                _newSampleVolume = _currentEvent.Value;
                break;
            }
            _eventAlreadyRun = true;
          }
        }

        // We simulate to output the output
        int volume = 0;
        if (Enabled)
        {
          _newSampleTimerDivider -= ticks;
          while (_newSampleTimerDivider <= 0)
          {
            _newSampleTimerDivider += 32;

            --_newSampleTickCounter;
            if (_newSampleTickCounter <= 0)
            {
              _newSampleTickCounter += _newSampleTickThreshold;
              _newSampleUp = !_newSampleUp;
            }
          }

          volume = _newSampleVolume;
        }

        // We generate the sample
        for (int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = (short)(_newSampleUp ? volume : -volume);
        }

        --fullSampleCount;
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClockLengthCounter()
    {
      if (SoundLengthCounter >= 0)
      {
        --SoundLengthCounter;
        if (SoundLengthCounter < 0)
        {
          Enabled = false;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CalculateSweepChange(bool updateValue, bool redoCalculation)
    {
      int freqChange = SweepFrequencyRegister;
      freqChange >>= SweepShifts;
      if (!SweepUp) { freqChange *= -1; }
      int newFreq = SweepFrequencyRegister + freqChange;

      // Overflows turns off the channel
      if (newFreq >= 0x800)
      {
        Enabled = false;
      }
      else
      {
        if ((SweepShifts > 0) && (SweepLength > 0))
        {
          if (!updateValue) { return; }

          SweepFrequencyRegister = newFreq;
          FrequencyFactor = (ushort)SweepFrequencyRegister;

          if (redoCalculation)
          {
            // We need to perform another check
            freqChange = SweepFrequencyRegister;
            freqChange >>= SweepShifts;
            if (!SweepUp) { freqChange *= -1; }
            newFreq = SweepFrequencyRegister + freqChange;

            if (newFreq > 0x800)
            {
              Enabled = false;
            }
          }
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
          file.WriteLine("{0},{1},{2},{3}", "Ms", "TickDiff","Threshold","Volume");
          for (uint i = 0; i < TimelineLocalCount; i += 4)
          {
            //file.WriteLine("{0},{1},{2}",
            //               Timeline[i],
            //               //"0x" + Timeline[i + 1].ToString("x2").ToUpper());
            //               Timeline[i + 1],
            //               Timeline[i + 2]);
            file.WriteLine("{0},{1},{2},{3}", TimelineLocal[i],
                                              TimelineLocal[i + 1],
                                              TimelineLocal[i + 2],
                                              TimelineLocal[i + 3]);
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
