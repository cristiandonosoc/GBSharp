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
    LSFR_CHANGE
  }

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

        AddSoundEvent(NoiseChannelEvents.ENABLED_CHANGE, Enabled ? 1 : 0);
      }
    }

    private const int _volumeConstant = 511;
    public int Volume {
      get
      {
        return EnvelopeCurrentValue * _volumeConstant;
      }
    }

    private int _channelIndex;

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
    private bool _envelopeCurrentUp;
    private int _envelopeCurrentValue;
    private int EnvelopeCurrentValue
    {
      get { return _envelopeCurrentValue; }
      set
      {
        _envelopeCurrentValue = value;
        AddSoundEvent(NoiseChannelEvents.VOLUME_CHANGE, Volume);
      }
    }

    private Memory _memory;
    private FrameSequencer _frameSequencer;

    private long _tickDiff;

    private SoundEventQueue _eventQueue;

    private void AddSoundEvent(NoiseChannelEvents soundEvent, int value, int channelIndex = 0xFF)
    {
      if (channelIndex == 0xFF) { channelIndex = _channelIndex; }
      _eventQueue.AddSoundEvent(_tickDiff, (int)soundEvent, value, channelIndex);
      _tickDiff = 0;
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

      _eventQueue = new SoundEventQueue(1000);
    }

    private int _soundLengthCounter;
    private bool _continuousOutput;

    public void HandleMemoryChange(MMR register, byte value)
    {
      switch (register)
      {
        case MMR.NR41:  // Sound Length
          _soundLengthCounter = 0x3F - (value & 0x3F);
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR42:
          _envelopeTicks = value & 0x07;
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
          AddSoundEvent(NoiseChannelEvents.NR43_WRITE, value);
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
            if ((_frameSequencer.Value & 0x01) == 0)
            {
              ClockLengthCounter();
            }
          }

          bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          if (init)
          {
            AddSoundEvent(NoiseChannelEvents.INIT, 0);
            if(_envelopeDACOn)
            {
              Enabled = true;
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
                  ((_frameSequencer.Value & 0x01) == 0))
              {
                ClockLengthCounter();
              }
            }

            // Envelope is reloaded
            _envelopeTickCounter = _envelopeTicks;
            _envelopeCurrentUp = _envelopeUp;
            EnvelopeCurrentValue = _envelopeDefaultValue;
          }

          _memory.LowLevelWrite((ushort)register, value);
          break;
        default:
          throw new InvalidProgramException("Invalid register received.");
      }
    }

    public void PowerOff()
    {
      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)MMR.NR41, 0);

      // Volume Envelope 
      _envelopeTicks = 0;
      _envelopeTickCounter = 0;
      _envelopeUp = false;
      _envelopeDefaultValue = 0;
      _memory.LowLevelWrite((ushort)MMR.NR42, 0);
    }

    public void ChangeLength(byte value)
    {
      _soundLengthCounter = 0x3F - (value & 0x3F);
      // Only the length part changes
      byte prevValue = _memory.LowLevelRead((ushort)MMR.NR41);
      _memory.LowLevelWrite((ushort)MMR.NR41, (byte)((prevValue & 0xC0) | (value & 0x3F)));
    }

    internal void Step(int ticks)
    {
      _tickDiff += ticks;

      if (_frameSequencer.Clocked)
      {
        // SOUND LENGTH COUNTER
        if ((_frameSequencer.Value & 0x01) == 0)
        {
          // NOTE(Cristian): The length counter runs even when the channel is disabled
          if (!_continuousOutput)
          {
            // We have an internal period
            ClockLengthCounter();
          }
        }

        #region VOLUME ENVELOPE

        if ((_frameSequencer.Value & 0x07) == 0x07)
        {
          if (_envelopeTicks > 0)
          {
            --_envelopeTickCounter;
            if (_envelopeTickCounter <= 0)
            {
              _envelopeTickCounter = _envelopeTicks;
              if (_envelopeCurrentUp)
              {
                ++EnvelopeCurrentValue;
                if (EnvelopeCurrentValue > 15)
                {
                  EnvelopeCurrentValue = 15;
                  _envelopeTicks = 0;
                }
              }
              else
              {
                --EnvelopeCurrentValue;
                if (EnvelopeCurrentValue < 0)
                {
                  EnvelopeCurrentValue = 0;
                  _envelopeTicks = 0;
                }
              }
            }
          }
        }

        #endregion
      }
    }

    long _eventTickCounter;
    long _eventOnHoldCounter;
    bool _eventAlreadyRun = true;
    SoundEvent _currentEvent = new SoundEvent();

    private int _sampleLsfrRegister = 0x7FFF;
    private bool _sampleLsfrBigSteps;
    private int _sampleClockDividerThreshold = CalculateSampleClockDividerThreshold(0);
    private int _sampleClockDividerCounter = 4;
    private int _sampleClockPreScalerThreshold = 0x01;
    private int _sampleClockPreScalerCounter = 1;

    private bool _sampleUp;
    private int _sampleVolume;

    private short _sampleValue;
    private bool _sampleEnabled;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateSampleClockDividerThreshold(int dividerKey)
    {
      int result;
      if (dividerKey == 0) { result = 4; }
      else { result = 8 * dividerKey; }
      return result;
    }

    public void GenerateSamples(int sampleCount)
    {
      while(sampleCount > 0)
      {
        --sampleCount;
        // We how many ticks will pass this sample
        long eventTicks = APU.MinimumTickThreshold;
        eventTicks += _eventOnHoldCounter;
        _eventOnHoldCounter = 0;

        while (eventTicks > 0)
        {
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
              _eventOnHoldCounter += eventTicks;
              eventTicks = 0;
            }
          }
          else
          {
            // We need to substract the on hold time
            if (_eventTickCounter > eventTicks)
            {
              _eventTickCounter -= eventTicks;
              eventTicks = 0;
            }
            else
            {
              eventTicks -= _eventTickCounter;
              _eventTickCounter = 0;
            }

            if (_eventTickCounter <= 0)
            {
              switch ((NoiseChannelEvents)_currentEvent.Kind)
              {
                case NoiseChannelEvents.VOLUME_CHANGE:
                  _sampleVolume = _currentEvent.Value;
                  _sampleValue = (short)(_sampleUp ? _sampleVolume : -_sampleVolume);
                  break;
                case NoiseChannelEvents.NR43_WRITE:
                  int dividerKey = _currentEvent.Value & 0x07;
                  _sampleClockDividerThreshold = CalculateSampleClockDividerThreshold(dividerKey);

                  _sampleLsfrBigSteps = ((_currentEvent.Value & 0x08) == 0);
                  if (_sampleLsfrBigSteps) { _sampleLsfrRegister = 0x7FFF; }
                  else { _sampleLsfrRegister = 0x7F; }

                  int preScalerKey = _currentEvent.Value >> 4;
                  if (preScalerKey < 0xDF) // Last two values are not used
                  {
                    _sampleClockPreScalerThreshold = (0x01 << (_currentEvent.Value >> 4));
                  }
                  break;
                case NoiseChannelEvents.ENABLED_CHANGE:
                  _sampleEnabled = (_currentEvent.Value == 1);
                  break;
                case NoiseChannelEvents.INIT:
                  if (_sampleLsfrBigSteps) { _sampleLsfrRegister = 0x7FFF; }
                  else { _sampleLsfrRegister = 0x7F; }
                  break;
              }
              _eventAlreadyRun = true;
            }
          }
        }

        // We simulate
        int ticks = APU.MinimumTickThreshold;
        _sampleClockDividerCounter -= ticks;
        while (_sampleClockDividerCounter <= 0)
        {
          _sampleClockDividerCounter += _sampleClockDividerThreshold;

          --_sampleClockPreScalerCounter;
          if (_sampleClockPreScalerCounter <= 0)
          {
            _sampleClockPreScalerCounter += _sampleClockPreScalerThreshold;

            // We remember the last value
            int firstBit = _sampleLsfrRegister & 0x01;
            bool prevSampleUp = _sampleUp;
            _sampleUp = (firstBit == 0); // The last bit is inverted
            _sampleValue = (short)(_sampleUp ? _sampleVolume : -_sampleVolume);

            _sampleLsfrRegister >>= 1;
            // We are only interested in XOR'ing the last two bits
            int newDigit = (firstBit ^ _sampleLsfrRegister) & 0x01;

            // We insert the digit in its place
            if (_sampleLsfrBigSteps)
            {
              // If bit steps, as we did a shift, the 15 place is 0
              _sampleLsfrRegister |= (newDigit << 14);
            }
            else
            {
              // In this case we have to clear the bit 7 first
              _sampleLsfrRegister = (_sampleLsfrRegister & ~0x40) | (newDigit << 6);
            }
          }

        }

        // We output
        for(int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = _sampleValue;
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
