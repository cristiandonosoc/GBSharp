using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal enum NoiseChannelEvents
  {
    VOLUME_CHANGE,
    NR43_WRITE,
    ENABLED_CHANGE,
    INIT,
    LSFR_CHANGE,
    LSFR_CHANGE2,
    LSFR_CHANGE3,
  }

  class NoiseChannel : IChannel
  {
    [Serializable]
    internal class State
    {
      internal long TickDiff;

      internal bool Enabled;

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

    #endregion

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

      AddSoundEvent(NoiseChannelEvents.ENABLED_CHANGE, _state.Enabled ? 1 : 0);
    }

    private const int _volumeConstant = 511;
    public int Volume {
      get
      {
        return _state.EnvelopeCurrentValue * _volumeConstant;
      }
    }

    private void SetEnvelopeCurrentValue(int value)
    {
      _state.EnvelopeCurrentValue = value;
      AddSoundEvent(NoiseChannelEvents.VOLUME_CHANGE, Volume);
    }

    private Memory _memory;
    private FrameSequencer _frameSequencer;
    private SoundEventQueue _soundEventQueue;
    private int _channelIndex;

    private void AddSoundEvent(NoiseChannelEvents soundEvent, int value, int channelIndex = 0xFF)
    {
      if (channelIndex == 0xFF) { channelIndex = _channelIndex; }
      _soundEventQueue.AddSoundEvent(_state.TickDiff, (int)soundEvent, value, channelIndex);
      _state.TickDiff = 0;
    }

    internal NoiseChannel(Memory  memory, FrameSequencer frameSequencer,
                          int sampleRate, int numChannels, 
                          int sampleSize, int channelIndex)
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
    }


    public void HandleMemoryChange(MMR register, byte value)
    {
      switch (register)
      {
        case MMR.NR41:  // Sound Length
          _state.SoundLengthCounter = 0x3F - (value & 0x3F);
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR42:
          _state.EnvelopeTicks = value & 0x07;
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
          break;
        case MMR.NR43:
          AddSoundEvent(NoiseChannelEvents.NR43_WRITE, value);
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR44:

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

          bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          if (init)
          {
            AddSoundEvent(NoiseChannelEvents.INIT, 0);
            if(_state.EnvelopeDACOn)
            {
              SetEnabled(true);
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

          _memory.LowLevelWrite((ushort)register, value);
          break;
      }
    }

    public void PowerOff()
    {
      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)MMR.NR41, 0);

      // Volume Envelope 
      _state.EnvelopeTicks = 0;
      _state.EnvelopeTickCounter = 0;
      _state.EnvelopeUp = false;
      _state.EnvelopeDefaultValue = 0;
      _memory.LowLevelWrite((ushort)MMR.NR42, 0);
    }

    public void ChangeLength(byte value)
    {
      _state.SoundLengthCounter = 0x3F - (value & 0x3F);
      // Only the length part changes
      byte prevValue = _memory.LowLevelRead((ushort)MMR.NR41);
      _memory.LowLevelWrite((ushort)MMR.NR41, (byte)((prevValue & 0xC0) | (value & 0x3F)));
    }

    internal void Step(int ticks)
    {
      _state.TickDiff += ticks;

      if (_frameSequencer.Clocked)
      {
        // SOUND LENGTH COUNTER
        if ((_frameSequencer.Value & 0x01) == 0)
        {
          // NOTE(Cristian): The length counter runs even when the channel is disabled
          if (!_state.ContinuousOutput)
          {
            // We have an internal period
            ClockLengthCounter();
          }
        }

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

    class OutputState
    {
      internal long EventTickCounter;
      internal long EventOnHoldCounter;
      internal bool EventAlreadyRun = true;
      internal SoundEvent CurrentEvent = new SoundEvent();

      internal int SampleLsfrRegister = 0x7FFF;
      internal bool SampleLsfrBigSteps;
      internal int SampleClockDividerThreshold = CalculateSampleClockDividerThreshold(0);
      internal int SampleClockDividerCounter = 4;
      internal int SampleClockPreScalerThreshold = 0x01;
      internal int SampleClockPreScalerCounter = 1;

      internal bool SampleUp;
      internal int SampleVolume;

      internal short SampleValue;
      internal bool SampleEnabled;

      internal int SampleIndex;
    }
    OutputState _outputState = new OutputState();

    public void GenerateSamples(int sampleCount, int ticksPerSample)
    {
      while(sampleCount > 0)
      {
        --sampleCount;
        // We how many ticks will pass this sample
        long eventTicks = ticksPerSample;
        eventTicks += _outputState.EventOnHoldCounter;
        _outputState.EventOnHoldCounter = 0;

        while (eventTicks > 0)
        {
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
              _outputState.EventOnHoldCounter += eventTicks;
              eventTicks = 0;
            }
          }
          else
          {
            // We need to substract the on hold time
            if (_outputState.EventTickCounter > eventTicks)
            {
              _outputState.EventTickCounter -= eventTicks;
              eventTicks = 0;
            }
            else
            {
              eventTicks -= _outputState.EventTickCounter;
              _outputState.EventTickCounter = 0;
            }

            if (_outputState.EventTickCounter <= 0)
            {
              HandleSoundEvent();
            }
          }
        }

        // We simulate
        int ticks = ticksPerSample;
        _outputState.SampleClockDividerCounter -= ticks;
        while (_outputState.SampleClockDividerCounter <= 0)
        {
          _outputState.SampleClockDividerCounter += _outputState.SampleClockDividerThreshold;

          --_outputState.SampleClockPreScalerCounter;
          if (_outputState.SampleClockPreScalerCounter <= 0)
          {
            _outputState.SampleClockPreScalerCounter += _outputState.SampleClockPreScalerThreshold;

            // We remember the last value
            int firstBit = _outputState.SampleLsfrRegister & 0x01;
            bool prevSampleUp = _outputState.SampleUp;
            _outputState.SampleUp = (firstBit == 0); // The last bit is inverted
            _outputState.SampleValue = (short)(_outputState.SampleUp ? _outputState.SampleVolume : -_outputState.SampleVolume);

            _outputState.SampleLsfrRegister >>= 1;
            // We are only interested in XOR'ing the last two bits
            int newDigit = (firstBit ^ _outputState.SampleLsfrRegister) & 0x01;

            // We insert the digit in its place
            if (_outputState.SampleLsfrBigSteps)
            {
              // If bit steps, as we did a shift, the 15 place is 0
              _outputState.SampleLsfrRegister |= (newDigit << 14);
            }
            else
            {
              // In this case we have to clear the bit 7 first
              _outputState.SampleLsfrRegister = (_outputState.SampleLsfrRegister & ~0x40) | (newDigit << 6);
            }
          }

        }

        // We output
        for(int c = 0; c < NumChannels; ++c)
        {
          _buffer[_outputState.SampleIndex++] = _outputState.SampleValue;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateSampleClockDividerThreshold(int dividerKey)
    {
      int result;
      if (dividerKey == 0) { result = 4; }
      else { result = 8 * dividerKey; }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleSoundEvent()
    {
      switch ((NoiseChannelEvents)_outputState.CurrentEvent.Kind)
      {
        case NoiseChannelEvents.VOLUME_CHANGE:
          _outputState.SampleVolume = _outputState.CurrentEvent.Value;
          _outputState.SampleValue = (short)(_outputState.SampleUp ? _outputState.SampleVolume :
                                                                    -_outputState.SampleVolume);
          break;
        case NoiseChannelEvents.NR43_WRITE:
          int dividerKey = _outputState.CurrentEvent.Value & 0x07;
          _outputState.SampleClockDividerThreshold = CalculateSampleClockDividerThreshold(dividerKey);

          _outputState.SampleLsfrBigSteps = ((_outputState.CurrentEvent.Value & 0x08) == 0);
          if (_outputState.SampleLsfrBigSteps) { _outputState.SampleLsfrRegister = 0x7FFF; }
          else { _outputState.SampleLsfrRegister = 0x7F; }

          int preScalerKey = _outputState.CurrentEvent.Value >> 4;
          if (preScalerKey < 0xDF) // Last two values are not used
          {
            _outputState.SampleClockPreScalerThreshold = (0x01 << (_outputState.CurrentEvent.Value >> 4));
          }
          break;
        case NoiseChannelEvents.ENABLED_CHANGE:
          _outputState.SampleEnabled = (_outputState.CurrentEvent.Value == 1);
          break;
        case NoiseChannelEvents.INIT:
          if (_outputState.SampleLsfrBigSteps) { _outputState.SampleLsfrRegister = 0x7FFF; }
          else { _outputState.SampleLsfrRegister = 0x7F; }
          break;
      }
      _outputState.EventAlreadyRun = true;
    }

    internal void DepleteSoundEventQueue()
    {
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
