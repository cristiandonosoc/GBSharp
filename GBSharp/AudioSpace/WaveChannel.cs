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
    ENABLED_CHANGE
  }

  internal class WaveChannel : IChannel, IWaveChannel
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

    private const int _volumeConstant = 1023;

    int _volumeRightShift;
    int VolumeRightShift
    {
      get { return _volumeRightShift; }
      set
      {
        _volumeRightShift = value;
        AddSoundEvent(WaveChannelEvents.VOLUME_CHANGE, _volumeRightShift);
      }
    }

    public int Volume
    {
      get
      {
        if(_volumeRightShift < 0) { return 0; }
        int index = ((CurrentSample >> _volumeRightShift) - 7);
        int volume = index * _volumeConstant;
        return volume;
      }
    }

    private int _channelIndex;

    internal byte LowFreqByte { get; private set; }
    internal byte HighFreqByte { get; private set; }

    long _tickDiff;

    private int _tickThreshold;
    private double _tickCounter;
    private int _timerDivider;

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

        AddSoundEvent(WaveChannelEvents.THRESHOLD_CHANGE, _tickThreshold);
      }
    }

    internal double Frequency { get; set; }

    private Memory _memory;
    private FrameSequencer _frameSequencer;

    private SoundEventQueue _eventQueue;

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

      _eventQueue = new SoundEventQueue(1000);
    }

    private bool _channelDACOn;

    public int SoundLengthCounter { get; private set; }

    public bool ContinuousOutput { get; private set; }
    private short _outputValue;

    private void AddSoundEvent(WaveChannelEvents soundEvent, int value)
    {
        _eventQueue.AddSoundEvent(_tickDiff, (int)soundEvent, value, _channelIndex);
        _tickDiff = 0;
    }

    public void HandleMemoryChange(MMR register, byte value)
    {
      switch(register)
      {
        case MMR.NR30:  // Sound on/off
          // Last bit determines sound on/off
          _channelDACOn = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          // When DAC re-enabled, the channel doesn't enables itself
          if (!_channelDACOn)
          {
            Enabled = false;
          }

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR31:  // Sound Length
          SoundLengthCounter = 0xFF - value;
          //_memory.LowLevelWrite((ushort)register, 0xFF);
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR32:  // Output Level (volume)
          // Basically, we shift by this amount.
          // If the amount is 0, it means we mute
          _volumeRightShift = ((value >> 5) & 0x3) - 1;
          // We reload the sample
          //_outputValue = (short)Volume;

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR33:  // FrequencyFactor lower
          FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | value);

          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR34:  // FrequencyFactor higher
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

          bool init = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          if(init)
          {
            _tickCounter = _tickThreshold;
            CurrentSampleIndex = 0;

            // NOTE(Cristian): If the length counter is empty at INIT,
            //                 it's reloaded with full length
            if (SoundLengthCounter < 0)
            {
              SoundLengthCounter = 0xFF;

              // If INIT on an zerioed empty enabled length channel
              // AND the next frameSequencer tick WON'T tick the length period
              // The lenght counter is somehow decremented
              if (!ContinuousOutput &&
                  ((_frameSequencer.Value & 0x01) == 0))
              {
                ClockLengthCounter();
              }
            }

            if(_channelDACOn)
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

    internal byte HandleWaveRead()
    {
      return _currentWaveByte;
    }

    internal void HandleWaveWrite(ushort address, byte value)
    {
      int encodedValue = ((address << 8) | value);
      AddSoundEvent(WaveChannelEvents.MEMORY_CHANGE, encodedValue);
    }

    public void PowerOff()
    {
      // Length Register is unaffected by write

      // Volume Envelope 
      //_envelopeTicks = 0;
      //_envelopeTickCounter = 0;
      //_envelopeUp = false;
      //_envelopeDefaultValue = 0;
      _memory.LowLevelWrite((ushort)MMR.NR30, 0);
      _memory.LowLevelWrite((ushort)MMR.NR32, 0);

      // Length Register is unaffected by write
      _memory.LowLevelWrite((ushort)MMR.NR31, 0x00);


      // Frequency-Low
      FrequencyFactor = 0x00;
      ContinuousOutput = true;
      _memory.LowLevelWrite((ushort)MMR.NR33, 0);
      _memory.LowLevelWrite((ushort)MMR.NR34, 0);
    }

    public void ChangeLength(byte value)
    {
      SoundLengthCounter = 0xFF - value;
      _memory.LowLevelWrite((ushort)MMR.NR31, value);
    }

    internal void Step(int ticks)
    {
      _tickDiff += ticks;

      // FrameSequencer ticks at 512 Hz
      if (_frameSequencer.Clocked)
      {

        // Length counter ticks at 256 Hz (every two frameSequencer ticks)
        if ((_frameSequencer.Value & 0x01) == 0)
        {
          if (!ContinuousOutput)
          {
            ClockLengthCounter();
          }
        }
      }

      if (!Enabled) { return; }

      _timerDivider -= ticks;
      if (_timerDivider <= 0)
      {
        _timerDivider += 32;

        --_tickCounter;
        if (_tickCounter <= 0)
        {
          _tickCounter += _tickThreshold;

          ++CurrentSampleIndex;
          if (CurrentSampleIndex >= 32)
          {
            CurrentSampleIndex = 0;
          }

          // We get the memory value
          ushort waveRAMAddress = (ushort)(0xFF30 + CurrentSampleIndex / 2);

          _currentWaveByte = _memory.LowLevelRead(waveRAMAddress);
          // Pair means the first 4 bits,
          // Odd means the last 4 bits
          if ((CurrentSampleIndex & 1) == 0)
          {
            CurrentSample = (byte)(_currentWaveByte >> 4);
          }
          else
          {
            CurrentSample = (byte)(_currentWaveByte & 0xF);
          }

          _outputValue = (short)Volume;
        }
      }
    }

    private byte _currentWaveByte;
    public byte CurrentSample { get; private set; }
    public byte CurrentSampleIndex { get; private set; }

    public void GenerateSamples(int sampleCount)
    {
      while (sampleCount > 0)
      {
        --sampleCount;

        for (int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = _outputValue;
        }
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

    byte[] _newSampleArray = new byte[16];
    int _newSampleIndex;
    byte _newCurrentSample;
    int _newVolumeShift;

    int CalculateWaveVolume(int volumeRightShift, byte currentSample)
    {
        if(volumeRightShift < 0) { return 0; }
        int index = ((currentSample >> volumeRightShift) - 7);
        int volume = index * _volumeConstant;
        return volume;
    }

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
            switch((WaveChannelEvents)_currentEvent.Kind)
            {
              case WaveChannelEvents.THRESHOLD_CHANGE:
                _newSampleTickThreshold = _currentEvent.Value;
                break;
              case WaveChannelEvents.VOLUME_CHANGE:
                _newVolumeShift = _currentEvent.Value;
                _newSampleVolume = CalculateWaveVolume(_newVolumeShift, _newCurrentSample);
                break;
              case WaveChannelEvents.MEMORY_CHANGE:
                // The key is the actual address in memory
                int index = (_currentEvent.Value >> 8) - 0xFF30;
                byte value = (byte)(_currentEvent.Value & 0xFF);
                _newSampleArray[index] = value;
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
              ++_newSampleIndex;
              if (_newSampleIndex >= 32)
              {
                _newSampleIndex = 0;
              }

              // We get the memory value
              byte currentByte = _newSampleArray[_newSampleIndex >> 1];
              if ((_newSampleIndex & 1) == 0)
              {
                _newCurrentSample = (byte)(currentByte >> 4);
              }
              else
              {
                _newCurrentSample = (byte)(currentByte & 0x0F);
              }

              _newSampleVolume = CalculateWaveVolume(_newVolumeShift, _newCurrentSample);
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
 
  }
}
