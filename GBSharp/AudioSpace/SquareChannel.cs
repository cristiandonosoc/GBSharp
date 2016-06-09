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
    [Serializable]
    internal class State
    {
      // Time passed since the last event was triggered (in ticks)
      internal long TickDiff;

      internal bool Enabled;

      internal byte LowFreqByte;
      internal byte HighFreqByte;
      internal int TickThreshold;
      internal ushort FrequencyFactor;

      /**
       * Thse are the swepp variables
       */
      internal bool SweepEnabled;
      internal bool SweepCalcOcurred;
      internal int SweepFrequencyRegister;
      internal int SweepLength;
      internal int SweepCounter;
      internal int SweepShifts;
      internal bool SweepUp;

      /**
       * These are the values that are currently set by the volume envelope registers.
       * These are the values that will become 'live' on the next channel INIT
       */
      internal int EnvelopeTicks;
      internal int EnvelopeTickCounter;
      internal bool EnvelopeUp;
      internal int EnvelopeDefaultValue;
      internal bool EnvelopeDACOn;

      /**
       * These are the values that are currently 'live' in the channel.
       * These could be (or not) the same values that are loaded in the envelope register.
       */
      internal bool EnvelopeCurrentUp;
      internal int EnvelopeCurrentValue;

      internal int SoundLengthCounter;
      internal bool ContinuousOutput;
    }
    State _state = new State();
    internal State GetState() { return _state; }
    internal void SetState(State state) { _state = state; }

    #region STATE INTERNALS GETTERS/SETTERS

    public bool Enabled { get { return _state.Enabled; } }
    public int SoundLengthCounter { get { return _state.SoundLengthCounter; } }
    public bool ContinuousOutput { get { return _state.ContinuousOutput; } }

    public int SweepFrequencyRegister { get { return _state.SweepFrequencyRegister; } }
    public int SweepShifts { get { return _state.SweepShifts; } }
    public int SweepLength { get { return _state.SweepLength; } }
    public int SweepCounter { get { return _state.SweepCounter; } }
    public bool SweepUp { get { return _state.SweepUp; } }

    #endregion

    private Memory _memory;

    #region BUFFER DEFINITION

    private int _msSampleRate;
    public int SampleRate { get; private set; }
    public int NumChannels { get; private set; }
    public int SampleSize { get; private set; }
    private int _milliseconds = 1000; // ms of sample

    short[] _buffer;
    public short[] Buffer { get { return _buffer; } }

    public int SampleCount { get { return _outputState.SampleIndex; } }

    #endregion

    internal void SetEnabled(bool value)
    {
      _state.Enabled = value;
      // We update the NR52 byte
      byte nr52 = _memory.LowLevelRead((ushort)MMR.NR52);
      if(_state.Enabled)
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

    private void SetFrequencyFactor(ushort value)
    {
      _state.FrequencyFactor = value;
      _state.LowFreqByte = (byte)_state.FrequencyFactor;
      _state.HighFreqByte = (byte)((_state.FrequencyFactor >> 8) & 0x7);
      // This is the counter used to output sound
      _state.TickThreshold = (0x800 - _state.FrequencyFactor) / 2;
      AddSoundEvent(SquareChannelEvents.THRESHOLD_CHANGE, _state.TickThreshold);
    }

    private void SetEnvelopeCurrentValue(int value)
    {
      _state.EnvelopeCurrentValue = value;
      AddSoundEvent(SquareChannelEvents.VOLUME_CHANGE, Volume);
    }

    #region VOLUME / VOLUME ENVELOPE

    private const int _volumeConstant = 511;
    public int Volume {
      get
      {
        return _state.EnvelopeCurrentValue * _volumeConstant;
      }
    }

    #endregion

    private FrameSequencer _frameSequencer;
    private SoundEventQueue _soundEventQueue;
    // Registers
    private MMR _sweepRegister;
    private MMR _wavePatternDutyRegister;
    private MMR _volumeEnvelopeRegister;
    private MMR _freqLowRegister;
    private MMR _freqHighRegister;
    private int _channelIndex;

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
      _soundEventQueue = new SoundEventQueue(1000);

      // Register setup
      _sweepRegister = sweepRegister;
      _wavePatternDutyRegister = wavePatternDutyRegister;
      _volumeEnvelopeRegister = volumeEnvelopeRegister;
      _freqLowRegister = freqLowRegister;
      _freqHighRegister = freqHighRegister;
    }

    private void AddSoundEvent(SquareChannelEvents soundEvent, int value)
    {
        _soundEventQueue.AddSoundEvent(_state.TickDiff, (int)soundEvent, value, _channelIndex);
        _state.TickDiff = 0;
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
        _state.SweepShifts = value & 0x07;

        // Sweep Direction (Bit 3)
        bool prevSweepUp = _state.SweepUp;
        _state.SweepUp = ((value & 0x08) == 0);
        // Going from neg->pos after a calc has occurred disabled the channel
        if (_state.SweepCalcOcurred && !prevSweepUp && _state.SweepUp)
        {
          SetEnabled(false);
        }
        _state.SweepCalcOcurred = false;

        // Sweep Time (Bits 4-6)
        _state.SweepLength = ((value >> 4) & 0x07);

        // Bit 7 is always 1
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x80));
      }
      else if (register == _wavePatternDutyRegister)
      {
        // TODO(Cristian): Wave Pattern Duty
        _state.SoundLengthCounter = 0x3F - (value & 0x3F);

        _memory.LowLevelWrite((ushort)register, value);
      }
      else if (register == _volumeEnvelopeRegister)
      {
        _state.EnvelopeTicks = value & 0x7;
        _state.EnvelopeUp = (value & 0x8) != 0;
        _state.EnvelopeDefaultValue = value >> 4;

        // Putting volume 0 disables the channel
        if ((_state.EnvelopeDefaultValue == 0) && !_state.EnvelopeUp)
        {
          _state.EnvelopeDACOn = false;
          SetEnabled(false);
        }
        else
        {
          _state.EnvelopeDACOn = true;
        }

        _memory.LowLevelWrite((ushort)register, value);
      }
      else if (register == _freqLowRegister)
      {
        ushort newFreqFactor = (ushort)(((_state.HighFreqByte & 0x7) << 8) | value);
        SetFrequencyFactor(newFreqFactor);
        _memory.LowLevelWrite((ushort)register, value);
      }
      else if (register == _freqHighRegister)
      {
        ushort newFreqFactor = (ushort)(((value & 0x7) << 8) | _state.LowFreqByte); 
        SetFrequencyFactor(newFreqFactor);

        bool prevContinuousOutput = _state.ContinuousOutput;
        _state.ContinuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);
        // Only enabling sound length (disabled -> enabled) could trigger a clock
        if ((!_state.ContinuousOutput) &&
            (prevContinuousOutput != _state.ContinuousOutput))
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
          if(_state.EnvelopeDACOn)
          {
            SetEnabled(true);
          }

          // FREQUENCY SWEEP
          _state.SweepFrequencyRegister = _state.FrequencyFactor;
          _state.SweepCounter = _state.SweepLength;

          if (_state.SweepLength == 0) { _state.SweepCounter = 8; }
          _state.SweepEnabled = ((_state.SweepLength > 0) || (_state.SweepShifts > 0));
          // We create immediate frequency calculation
          if (_state.SweepShifts > 0)
          {
            CalculateSweepChange(updateValue: false, redoCalculation: false);
            _state.SweepCalcOcurred = true;
          }

          // NOTE(Cristian): If the length counter is empty at INIT,
          //                 it's reloaded with full length
          if(_state.SoundLengthCounter < 0)
          {
            _state.SoundLengthCounter = 0x3F;

            // If INIT on an zerioed empty enabled length channel
            // AND the next frameSequencer tick WON'T tick the length period
            // The lenght counter is somehow decremented
            if (!_state.ContinuousOutput &&
                ((_frameSequencer.Value & 0x01) == 0))
            {
              ClockLengthCounter();
            }
          }

          // Envelope is reloaded
          _state.EnvelopeTickCounter = _state.EnvelopeTicks;
          _state.EnvelopeCurrentUp = _state.EnvelopeUp;
          SetEnvelopeCurrentValue(_state.EnvelopeDefaultValue);
        }

        // Bits 3-5 are always 1
        _memory.LowLevelWrite((ushort)register, (byte)(value | 0x38));
      }
      else if (register == MMR.NR52)
      {
        SetEnabled((Utils.UtilFuncs.TestBit(value, _channelIndex) != 0));
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
        _state.SweepShifts = 0;
        _state.SweepUp = true;
        _state.SweepCalcOcurred = false;
        _state.SweepLength = 0;
        _memory.LowLevelWrite((ushort)_sweepRegister, 0);
      }

      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)_wavePatternDutyRegister, 0x00);

      // Volume Envelope
      _state.EnvelopeTicks = 0;
      _state.EnvelopeTickCounter = 0;
      _state.EnvelopeUp = false;
      _state.EnvelopeDefaultValue = 0;
      _memory.LowLevelWrite((ushort)_volumeEnvelopeRegister, 0);

      // Frequency-Low
      SetFrequencyFactor(0x00);
      _state.ContinuousOutput = true;
      _memory.LowLevelWrite((ushort)_freqLowRegister, 0);
      _memory.LowLevelWrite((ushort)_freqHighRegister, 0);
    }

    public void ChangeLength(byte value)
    {
      _state.SoundLengthCounter = 0x3F - (value & 0x3F);
      // Only length part changes
      byte prevValue = _memory.LowLevelRead((ushort)_wavePatternDutyRegister);
      _memory.LowLevelWrite((ushort)_wavePatternDutyRegister, (byte)((prevValue & 0xC0) | (value & 0x3F)));
    }

    internal void Step(int ticks)
    {
      _state.TickDiff += ticks;

      if (_frameSequencer.Clocked)
      { 
        // We check which internal element ticket

        // SOUND LENGTH
        // Clocks at 256 Hz (every two frame sequencer ticks)
        if ((_frameSequencer.Value & 0x01) == 0)
        {
          // NOTE(Cristian): The length counter runs even when the channel is disabled
          if (!_state.ContinuousOutput)
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
          if (_state.SweepEnabled)
          {
            --_state.SweepCounter;
            if (_state.SweepCounter == 0)
            {
              if (_state.SweepLength > 0)
              {
                CalculateSweepChange(updateValue: true, redoCalculation: true);
                _state.SweepCalcOcurred = true;
              }

              // We restart the sweep counter
              _state.SweepCounter = _state.SweepLength;
              if (_state.SweepLength == 0) { _state.SweepCounter = 8; }
            }
          }
        }

        #endregion

        #region VOLUME ENVELOPE

        if ((_frameSequencer.Value & 0x07) == 0x07)
        {
          if (_state.EnvelopeTicks > 0)
          {
            --_state.EnvelopeTickCounter;
            if (_state.EnvelopeTickCounter <= 0)
            {
              _state.EnvelopeTickCounter = _state.EnvelopeTicks;
              if (_state.EnvelopeCurrentUp)
              {
                SetEnvelopeCurrentValue(_state.EnvelopeCurrentValue + 1);
                if (_state.EnvelopeCurrentValue > 15)
                {
                  SetEnvelopeCurrentValue(15);
                  _state.EnvelopeTicks = 0;
                }
              }
              else
              {
                SetEnvelopeCurrentValue(_state.EnvelopeCurrentValue - 1);
                if (_state.EnvelopeCurrentValue < 0)
                {
                  SetEnvelopeCurrentValue(0);
                  _state.EnvelopeTicks = 0;
                }
              }
            }
          }
        }

        #endregion

      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CalculateSweepChange(bool updateValue, bool redoCalculation)
    {
      int freqChange = _state.SweepFrequencyRegister;
      freqChange >>= _state.SweepShifts;
      if (!_state.SweepUp) { freqChange *= -1; }
      int newFreq = _state.SweepFrequencyRegister + freqChange;

      // Overflows turns off the channel
      if (newFreq >= 0x800)
      {
        SetEnabled(false);
      }
      else
      {
        if ((_state.SweepShifts > 0) && (_state.SweepLength > 0))
        {
          if (!updateValue) { return; }

          _state.SweepFrequencyRegister = newFreq;
          SetFrequencyFactor((ushort)_state.SweepFrequencyRegister);

          if (redoCalculation)
          {
            // We need to perform another check
            freqChange = _state.SweepFrequencyRegister;
            freqChange >>= _state.SweepShifts;
            if (!_state.SweepUp) { freqChange *= -1; }
            newFreq = _state.SweepFrequencyRegister + freqChange;

            if (newFreq > 0x800)
            {
              SetEnabled(false);
            }
          }
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClockLengthCounter()
    {
      if (_state.SoundLengthCounter >= 0)
      {
        --_state.SoundLengthCounter;
        if (_state.SoundLengthCounter < 0)
        {
          SetEnabled(false);
        }
      }
    }
 

    // These are output variables
    class OutputState
    {
      internal long EventTickCounter;
      internal long EventOnHoldCounter;
      internal bool EventAlreadyRun = true;
      internal SoundEvent CurrentEvent = new SoundEvent();

      internal int SampleTickCounter;
      internal int SampleTickThreshold;
      internal int SampleVolume;
      internal bool SampleUp;
      internal long SampleTimerDivider;

      internal int SampleIndex;
    }
    OutputState _outputState = new OutputState();

    public void GenerateSamples(int fullSamples, int ticksPerSample)
    {
      int fullSampleCount = fullSamples;
      while (fullSampleCount > 0)
      {
        // We how many ticks will pass this sample
        long ticks = ticksPerSample;

        // If the event already run, we try to see if there is a new one
        if (_outputState.EventAlreadyRun)
        {
          if (_soundEventQueue.GetNextEvent(ref _outputState.CurrentEvent))
          {
            _outputState.EventTickCounter = _outputState.CurrentEvent.TickDiff;
            _outputState.EventAlreadyRun = false;
          }
          else
          {
            _outputState.EventOnHoldCounter += ticks;
          }
        }
        else
        {
          // We need to substract the on hold time
          _outputState.EventOnHoldCounter += ticks;
          if (_outputState.EventTickCounter > _outputState.EventOnHoldCounter)
          {
            _outputState.EventTickCounter -= _outputState.EventOnHoldCounter;
          }
          else
          {
            _outputState.EventTickCounter = 0;
          }
          _outputState.EventOnHoldCounter = 0;

          if (_outputState.EventTickCounter <= 0)
          {
            HandleSoundEvent();
          }
        }

        // We simulate to output the output
        int volume = 0;
        if (_state.Enabled)
        {
          _outputState.SampleTimerDivider -= ticks;
          while (_outputState.SampleTimerDivider <= 0)
          {
            _outputState.SampleTimerDivider += 32;

            --_outputState.SampleTickCounter;
            if (_outputState.SampleTickCounter <= 0)
            {
              _outputState.SampleTickCounter += _outputState.SampleTickThreshold;
              _outputState.SampleUp = !_outputState.SampleUp;
            }
          }

          volume = _outputState.SampleVolume;
        }

        // We generate the sample
        for (int c = 0; c < NumChannels; ++c)
        {
          _buffer[_outputState.SampleIndex++] = (short)(_outputState.SampleUp ? volume : -volume);
        }

        --fullSampleCount;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleSoundEvent()
    {
      switch ((SquareChannelEvents)_outputState.CurrentEvent.Kind)
      {
        case SquareChannelEvents.THRESHOLD_CHANGE:
          _outputState.SampleTickThreshold = _outputState.CurrentEvent.Value;
          break;
        case SquareChannelEvents.VOLUME_CHANGE:
          _outputState.SampleVolume = _outputState.CurrentEvent.Value;
          break;
      }
      _outputState.EventAlreadyRun = true;
    }

    internal void DepleteSoundEventQueue()
    {
      // We pop all the events
      while (_soundEventQueue.GetNextEvent(ref _outputState.CurrentEvent))
      {
        HandleSoundEvent();
      }
    }

    public void ClearBuffer()
    {
      _outputState.SampleIndex = 0;
    }
  }
}
