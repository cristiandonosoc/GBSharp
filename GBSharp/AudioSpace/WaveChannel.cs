using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.MemorySpace;
using System.Runtime.CompilerServices;

namespace GBSharp.AudioSpace
{
  internal enum WaveChannelEvents
  {
    THRESHOLD_CHANGE,
    VOLUME_CHANGE,
    MEMORY_CHANGE,
    ENABLED_CHANGE,
    INIT 
  }

  internal class WaveChannel : IChannel, IWaveChannel
  {
    [Serializable]
    internal class State
    {
      internal bool Enabled;

      internal int VolumeRightShift;

      internal byte LowFreqByte;
      internal byte HighFreqByte;

      internal long TickDiff;

      internal int TickThreshold;
      internal double TickCounter;
      internal int TimerDivider;

      internal ushort FrequencyFactor;

      internal bool ChannelDACOn;
      internal int SoundLengthCounter;
      internal bool ContinuousOutput;

      internal byte CurrentWaveByte;
      internal byte CurrentSample;
      internal byte CurrentSampleIndex;
    }
    State _state = new State();
    internal State GetState() { return _state; }
    internal void SetState(State state) { _state = state; }

    #region STATE INTERNALS GETTERS/SETTERS

    public bool Enabled { get { return _state.Enabled; } }
    public int SoundLengthCounter { get { return _state.SoundLengthCounter; } }
    public bool ContinuousOutput { get { return _state.ContinuousOutput; } }
    public byte CurrentSampleIndex { get { return _state.CurrentSampleIndex; } }
    public byte CurrentSample { get { return _state.CurrentSample; } }

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

    internal void SetEnabled(bool enabled)
    {
        _state.Enabled = enabled;
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

        AddSoundEvent(WaveChannelEvents.ENABLED_CHANGE, _state.Enabled ? 1 : 0);
    }

    private void SetVolumeRightShift(int value)
    {
      _state.VolumeRightShift = value;
      AddSoundEvent(WaveChannelEvents.VOLUME_CHANGE, _state.VolumeRightShift);
    }

    void SetFrequencyFactor(ushort value)
    {
        _state.FrequencyFactor = value;
        _state.LowFreqByte = (byte)_state.FrequencyFactor;
        _state.HighFreqByte = (byte)((_state.FrequencyFactor >> 8) & 0x7);
        // This is the counter used to output sound
        _state.TickThreshold = (0x800 - _state.FrequencyFactor) / 2;
        AddSoundEvent(WaveChannelEvents.THRESHOLD_CHANGE, _state.TickThreshold);
    }

    private Memory _memory;
    private FrameSequencer _frameSequencer;
    private SoundEventQueue _soundEventQueue;
    private int _channelIndex;

    internal WaveChannel(Memory memory, FrameSequencer frameSequencer,
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

    private void AddSoundEvent(WaveChannelEvents soundEvent, int value)
    {
        _soundEventQueue.AddSoundEvent(_state.TickDiff, (int)soundEvent, value, _channelIndex);
        _state.TickDiff = 0;
    }

    public void HandleMemoryChange(MMR register, byte value)
    {
      switch(register)
      {
        case MMR.NR30:  // Sound on/off
          // Last bit determines sound on/off
          _state.ChannelDACOn = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          // When DAC re-enabled, the channel doesn't enables itself
          if (!_state.ChannelDACOn)
          {
            SetEnabled(false);
          }

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR31:  // Sound Length
          _state.SoundLengthCounter = 0xFF - value;
          //_memory.LowLevelWrite((ushort)register, 0xFF);
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR32:  // Output Level (volume)
          // Basically, we shift by this amount.
          // If the amount is 0, it means we mute
          SetVolumeRightShift(((value >> 5) & 0x3) - 1);
          // We reload the sample
          //_outputValue = (short)Volume;

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR33:  // FrequencyFactor lower
          {
            ushort newFreqFactor = (ushort)(((_state.HighFreqByte & 0x7) << 8) | value);
            SetFrequencyFactor(newFreqFactor);

            _memory.LowLevelWrite((ushort)register, value);
            break;
          }
        case MMR.NR34:  // FrequencyFactor higher
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

            bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
            if (init)
            {
              AddSoundEvent(WaveChannelEvents.INIT, value);
              _state.TickCounter = _state.TickThreshold;
              _state.CurrentSampleIndex = 0;

              // NOTE(Cristian): If the length counter is empty at INIT,
              //                 it's reloaded with full length
              if (_state.SoundLengthCounter < 0)
              {
                _state.SoundLengthCounter = 0xFF;

                // If INIT on an zerioed empty enabled length channel
                // AND the next frameSequencer tick WON'T tick the length period
                // The lenght counter is somehow decremented
                if (!_state.ContinuousOutput &&
                    ((_frameSequencer.Value & 0x01) == 0))
                {
                  ClockLengthCounter();
                }
              }

              if (_state.ChannelDACOn)
              {
                SetEnabled(true);
              }
            }

            _memory.LowLevelWrite((ushort)register, value);
            break;
          }
        default:
          throw new InvalidProgramException("Invalid register received.");
      }
    }

    internal byte HandleWaveRead()
    {
      return _state.CurrentWaveByte;
    }

    internal void HandleWaveWrite(ushort address, byte value)
    {
      int encodedValue = ((address << 8) | value);
      AddSoundEvent(WaveChannelEvents.MEMORY_CHANGE, encodedValue);
    }

    public void PowerOff()
    {
      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)MMR.NR30, 0);
      _memory.LowLevelWrite((ushort)MMR.NR32, 0);

      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)MMR.NR31, 0x00);


      // Frequency-Low
      SetFrequencyFactor(0x00);
      _state.ContinuousOutput = true;
      _memory.LowLevelWrite((ushort)MMR.NR33, 0);
      _memory.LowLevelWrite((ushort)MMR.NR34, 0);
    }

    public void ChangeLength(byte value)
    {
      _state.SoundLengthCounter = 0xFF - value;
      _memory.LowLevelWrite((ushort)MMR.NR31, value);
    }

    internal void Step(int ticks)
    {
      _state.TickDiff += ticks;

      // FrameSequencer ticks at 512 Hz
      if (_frameSequencer.Clocked)
      {

        // Length counter ticks at 256 Hz (every two frameSequencer ticks)
        if ((_frameSequencer.Value & 0x01) == 0)
        {
          if (!_state.ContinuousOutput)
          {
            ClockLengthCounter();
          }
        }
      }

      if (!_state.Enabled) { return; }

      _state.TimerDivider -= ticks;
      if (_state.TimerDivider <= 0)
      {
        _state.TimerDivider += 32;

        --_state.TickCounter;
        if (_state.TickCounter <= 0)
        {
          _state.TickCounter += _state.TickThreshold;

          ++_state.CurrentSampleIndex;
          if (_state.CurrentSampleIndex >= 32)
          {
            _state.CurrentSampleIndex = 0;
          }

          // We get the memory value
          ushort waveRAMAddress = (ushort)(0xFF30 + _state.CurrentSampleIndex / 2);

          _state.CurrentWaveByte = _memory.LowLevelRead(waveRAMAddress);
          // Pair means the first 4 bits,
          // Odd means the last 4 bits
          if ((_state.CurrentSampleIndex & 1) == 0)
          {
            _state.CurrentSample = (byte)(_state.CurrentWaveByte >> 4);
          }
          else
          {
            _state.CurrentSample = (byte)(_state.CurrentWaveByte & 0xF);
          }
        }
      }
    }

    class OutputState
    {
      internal long EventTickCounter;
      internal long EventOnHoldCounter;
      internal bool EventAlreadyRun = true;
      internal SoundEvent CurrentEvent = new SoundEvent();

      internal int SampleTickCounter;
      internal int SampleTickThreshold;
      internal int SampleVolume;

      internal long SampleTimerDivider;

      internal byte[] SampleArray = new byte[16];
      internal int SampleWaveIndex;
      internal byte NewCurrentSample;
      internal int NewVolumeShift;
      internal bool SampleEnabled = true;

      internal int SampleIndex;
    }
    OutputState _outputState = new OutputState();

    private const int _volumeConstant = 1023;
    int CalculateWaveVolume(int volumeRightShift, byte currentSample)
    {
        if(volumeRightShift < 0) { return 0; }
        int index = ((currentSample - 0x8) >> volumeRightShift);
        int volume = index * _volumeConstant;
        return volume;
    }

    public void GenerateSamples(int fullSamples)
    {
      int fullSampleCount = fullSamples;
      while (fullSampleCount > 0)
      {
        // We how many ticks will pass this sample
        long eventTicks = APU.MinimumTickThreshold;
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
              switch ((WaveChannelEvents)_outputState.CurrentEvent.Kind)
              {
                case WaveChannelEvents.THRESHOLD_CHANGE:
                  _outputState.SampleTickThreshold = _outputState.CurrentEvent.Value;
                  break;
                case WaveChannelEvents.VOLUME_CHANGE:
                  _outputState.NewVolumeShift = _outputState.CurrentEvent.Value;
                  _outputState.SampleVolume = CalculateWaveVolume(_outputState.NewVolumeShift, 
                                                                  _outputState.NewCurrentSample);
                  break;
                case WaveChannelEvents.MEMORY_CHANGE:
                  // The key is the actual address in memory
                  int index = (_outputState.CurrentEvent.Value >> 8) - 0xFF30;
                  byte value = (byte)(_outputState.CurrentEvent.Value & 0xFF);
                  _outputState.SampleArray[index] = value;
                  break;
                case WaveChannelEvents.ENABLED_CHANGE:
                  _outputState.SampleEnabled = (_outputState.CurrentEvent.Value == 1);
                  break;
                case WaveChannelEvents.INIT:
                  _outputState.SampleTickCounter = _outputState.SampleTickThreshold;
                  _outputState.SampleWaveIndex = 0;
                  break;
              }
              _outputState.EventAlreadyRun = true;
            }
          }
        }

        long ticks = APU.MinimumTickThreshold;

        // We simulate to output the output
        if (_outputState.SampleEnabled)
        {
          _outputState.SampleTimerDivider -= ticks;
          while (_outputState.SampleTimerDivider <= 0)
          {
            _outputState.SampleTimerDivider += 4;

            --_outputState.SampleTickCounter;
            if (_outputState.SampleTickCounter <= 0)
            {
              _outputState.SampleTickCounter += _outputState.SampleTickThreshold;

              ++_outputState.SampleWaveIndex;
              if (_outputState.SampleWaveIndex >= 32)
              {
                _outputState.SampleWaveIndex = 0;
              }

              // We get the memory value
              byte currentByte = _outputState.SampleArray[_outputState.SampleWaveIndex >> 1];
              if ((_outputState.SampleWaveIndex & 1) == 0)
              {
                _outputState.NewCurrentSample = (byte)(currentByte >> 4);
              }
              else
              {
                _outputState.NewCurrentSample = (byte)(currentByte & 0x0F);
              }

              _outputState.SampleVolume = CalculateWaveVolume(_outputState.NewVolumeShift, 
                                                              _outputState.NewCurrentSample);
            }
          }
        }

        int volume = 0;
        if (_outputState.SampleEnabled) { volume = _outputState.SampleVolume; }

        // We generate the sample
        for (int c = 0; c < NumChannels; ++c)
        {
          _buffer[_outputState.SampleIndex++] = (short)(volume);
        }

        --fullSampleCount;
      }
    }

    public void ClearBuffer()
    {
      _outputState.SampleIndex = 0;
    }


  }
}
