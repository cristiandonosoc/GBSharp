﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.MemorySpace;
using System.Diagnostics;
using System.IO;

namespace GBSharp.AudioSpace
{
  /// <summary>
  /// Audio Processing Unit
  /// </summary>
  class APU : IAPU, IDisposable
  {
#if SoundTiming
    internal static long[] TimelineLocal = new long[10000 * 4];
    internal static uint TimelineLocalCount = 0;
    internal static long[] TimelineSoundThread = new long[10000 * 3];
    internal static uint TimelineSoundThreadCount = 0;
    internal static Stopwatch sw = new Stopwatch();
#endif

    [Serializable]
    internal class State
    {
      internal bool Enabled;
      internal bool LeftChannelEnabled;
      internal bool RightChannelEnabled;

      internal bool OutputChannel1Left;
      internal bool OutputChannel1Right;
      internal bool OutputChannel2Left;
      internal bool OutputChannel2Right;
      internal bool OutputChannel3Left;
      internal bool OutputChannel3Right;
      internal bool OutputChannel4Left;
      internal bool OutputChannel4Right;

      internal FrameSequencer.State FrameSequencerState;
      internal SquareChannel.State Channel1State;
      internal SquareChannel.State Channel2State;
      internal WaveChannel.State Channel3State;
      internal NoiseChannel.State Channel4State;
    }
    State _state = new State();

    internal State GetState()
    {
      // We collect the frameSequencer State
      _state.FrameSequencerState = _frameSequencer.GetState();
      // We collect the channel states
      _state.Channel1State = _channel1.GetState();
      _state.Channel2State = _channel2.GetState();
      _state.Channel3State = _channel3.GetState();
      _state.Channel4State = _channel4.GetState();

      return _state;
    }
    internal void SetState(State state)
    {
      // We restore the state
      _state = state;
      // We restore the frameSequencer state
      _frameSequencer.SetState(_state.FrameSequencerState);
      // We restore the channel states
      _channel1.SetState(_state.Channel1State);
      _channel2.SetState(_state.Channel2State);
      _channel3.SetState(_state.Channel3State);
      _channel4.SetState(_state.Channel4State);
    }

    private FrameSequencer _frameSequencer;
    public FrameSequencer FrameSequencerTimer { get { return _frameSequencer; } }


    /// <summary>
    /// This is the amount of ticks needed to output a single sample
    /// ~ 22 kHz max frequency
    /// </summary>
    internal static int MinimumTickThreshold = 96;

    #region OUTPUT INFORMATION

    public bool Channel1Run { get; set; }
    public bool Channel2Run { get; set; }
    public bool Channel3Run { get; set; }
    public bool Channel4Run { get; set; }

    internal string CartridgeFilename { get; set; }
    private int _sampleRate;
    private int _msSampleRate;
    public int SampleRate { get { return _sampleRate; } }

    private int _numChannels;
    public int NumChannels { get { return _numChannels; } }

    private int _sampleSize;
    public int SampleSize { get { return _sampleSize; } }

    private int _milliseconds = 1000; // ms of sample

    byte[] _buffer;
    public byte[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    #endregion

    private GameBoy _gameboy;
    private Memory _memory;

    SquareChannel _channel1;
    SquareChannel _channel2;
    WaveChannel _channel3;
    NoiseChannel _channel4;
    public ISquareChannel Channel1 { get { return _channel1; } }
    public IWaveChannel Channel3 { get { return _channel3; } }

    private bool _recordSeparateChannels;
    public bool RecordSeparateChannels
    {
      get
      {
        return _recordSeparateChannels;
      }
      set
      {
        // NOTE(Cristian): If we're recording, we don't want to change the channel exporting
        //                 It's more complicated than it's worth to support
        if (Recording) { return; }
        _recordSeparateChannels = value;
      }
    }

    public bool Recording
    {
      get { return _wavExporter.Recording; }
    }

    private WavExporter _wavExporter;
    private WavExporter _channel1WavExporter;
    private WavExporter _channel2WavExporter;
    private WavExporter _channel3WavExporter;
    private WavExporter _channel4WavExporter;


    internal APU(GameBoy gameboy, Memory memory, int sampleRate, int numChannels, int sampleSize)
    {
      _gameboy = gameboy;
      _memory = memory;

      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;

      Reset();
    }

    internal void Reset()
    {
      // Wav exporter
      if ((_wavExporter != null))
      {
        // We stop recording just in case
        StopRecording();
      }
      else
      {
        _wavExporter = new WavExporter();
        _channel1WavExporter = new WavExporter();
        _channel2WavExporter = new WavExporter();
        _channel3WavExporter = new WavExporter();
        _channel4WavExporter = new WavExporter();
      }

      _frameSequencer = new FrameSequencer();

      // We setup the channels
      _channel1 = new SquareChannel(_memory, _frameSequencer,
                                    SampleRate, NumChannels, SampleSize, 0,
                                    MMR.NR10, MMR.NR11, MMR.NR12, MMR.NR13, MMR.NR14);
      // NOTE(Cristian): Channel 2 doesn't have frequency sweep
      _channel2 = new SquareChannel(_memory, _frameSequencer,
                                    SampleRate, NumChannels, SampleSize, 1,
                                    0, MMR.NR21, MMR.NR22, MMR.NR23, MMR.NR24);

      _channel3 = new WaveChannel(_memory, _frameSequencer,
                                  SampleRate, NumChannels, SampleSize, 2);
      _channel4 = new NoiseChannel(_memory, _frameSequencer,
                                   SampleRate, NumChannels, SampleSize, 3);

      _state.LeftChannelEnabled = true;
      _state.RightChannelEnabled = true;

      Channel1Run = true;
      Channel2Run = true;
      Channel3Run = true;
      Channel4Run = true;

      _memory.LowLevelWrite((ushort)MMR.NR10, 0x80);
      _memory.LowLevelWrite((ushort)MMR.NR11, 0xBF);
      _memory.LowLevelWrite((ushort)MMR.NR12, 0xF3);
      _memory.LowLevelWrite((ushort)MMR.NR13, 0xFF);
      _memory.LowLevelWrite((ushort)MMR.NR14, 0xBF);

      _memory.LowLevelWrite(0xFF15, 0xFF);

      _memory.LowLevelWrite((ushort)MMR.NR21, 0x3F);
      _memory.LowLevelWrite((ushort)MMR.NR22, 0x00);
      _memory.LowLevelWrite((ushort)MMR.NR23, 0xFF);
      _memory.LowLevelWrite((ushort)MMR.NR24, 0xBF);

      _memory.LowLevelWrite((ushort)MMR.NR30, 0x7F);
      _memory.LowLevelWrite((ushort)MMR.NR31, 0xFF);
      _memory.LowLevelWrite((ushort)MMR.NR32, 0x9F);
      _memory.LowLevelWrite((ushort)MMR.NR33, 0xBF);
      _memory.LowLevelWrite((ushort)MMR.NR34, 0xBF); // No info

      _memory.LowLevelWrite(0xFF1F, 0xFF);

      _memory.LowLevelWrite((ushort)MMR.NR41, 0xFF);
      _memory.LowLevelWrite((ushort)MMR.NR42, 0x00);
      _memory.LowLevelWrite((ushort)MMR.NR43, 0x00);
      _memory.LowLevelWrite((ushort)MMR.NR44, 0xBF); // No info

      _memory.LowLevelWrite((ushort)MMR.NR50, 0x77);
      _memory.LowLevelWrite((ushort)MMR.NR51, 0xF3);
      _memory.LowLevelWrite((ushort)MMR.NR52, 0xF1);

      // We fill 0xFF27-0xFF2F with 0xFF
      for (ushort r = 0xFF27; r <= 0xFF2F; ++r)
      {
        _memory.LowLevelWrite(r, 0xFF);
      }

      _buffer = new byte[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];
    }

    internal void HandleMemoryChange(MMR register, byte value, bool updatedEnabledFlag = true)
    {
      if (!_state.Enabled)
      {
        // When powered off, the internal length can be changed (DMG only)
        switch (register)
        {
          case MMR.NR11:
            _channel1.ChangeLength(value);
            break;
          case MMR.NR21:
            _channel2.ChangeLength(value);
            break;
          case MMR.NR31:
            _channel3.ChangeLength(value);
            break;
          case MMR.NR41:
            _channel4.ChangeLength(value);
            break;
        }

        // Other than turning on, all other writes are ignored
        if (register != MMR.NR52) { return; }
      }

      // We store previous channel status
      bool channel1Enabled = _channel1.Enabled;
      bool channel2Enabled = _channel2.Enabled;
      bool channel3Enabled = _channel3.Enabled;
      bool channel4Enabled = _channel4.Enabled;
      bool prevEnabled = _state.Enabled;

      switch (register)
      {
        case MMR.NR10:
        case MMR.NR11:
        case MMR.NR12:
        case MMR.NR13:
        case MMR.NR14:
          _channel1.HandleMemoryChange(register, value);
          break;
        case MMR.NR21:
        case MMR.NR22:
        case MMR.NR23:
        case MMR.NR24:
          _channel2.HandleMemoryChange(register, value);
          break;
        case MMR.NR30:
        case MMR.NR31:
        case MMR.NR32:
        case MMR.NR33:
        case MMR.NR34:
          _channel3.HandleMemoryChange(register, value);
          break;
        case MMR.NR41:
        case MMR.NR42:
        case MMR.NR43:
        case MMR.NR44:
          _channel4.HandleMemoryChange(register, value);
          break;
        case MMR.NR50:
          // NOTE(Cristian): No Vin support
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR51:
          // TODO(Cristian): Implement this logic
          _state.OutputChannel1Left = ((value & 0x01) != 0);
          _state.OutputChannel2Left = ((value & 0x02) != 0);
          _state.OutputChannel3Left = ((value & 0x04) != 0);
          _state.OutputChannel4Left = ((value & 0x08) != 0);
          _state.OutputChannel1Right = ((value & 0x10) != 0);
          _state.OutputChannel2Right = ((value & 0x20) != 0);
          _state.OutputChannel3Right = ((value & 0x40) != 0);
          _state.OutputChannel4Right = ((value & 0x80) != 0);
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR52:
          bool apuEnabled = (Utils.UtilFuncs.TestBit(value, 7) != 0);
          if (!apuEnabled)
          {
            // Powering down the APU should power down all the registers
            //for (ushort r = (ushort)MMR.NR10; r < (ushort)MMR.NR52; ++r)
            //{
            //  HandleMemoryChange((MMR)r, 0, false);
            //}
            _channel1.PowerOff();
            _channel1.SetEnabled(false);
            _channel2.PowerOff();
            _channel2.SetEnabled(false);
            _channel3.PowerOff();
            _channel3.SetEnabled(false);
            _channel4.PowerOff();
            _channel4.SetEnabled(false);

            _memory.LowLevelWrite((ushort)MMR.NR50, 0);
            _memory.LowLevelWrite((ushort)MMR.NR51, 0);
          }
          else if (!_state.Enabled)
          {
            _frameSequencer.Reset();
          }
          // We update at the end because otherwise the recursive calls would
          // be rejected by the guard
          _state.Enabled = apuEnabled;
          break;
      }

      // NOTE(Cristian): This is an "optimization" for when NR52 is disabled,
      //                 All the registers are set to 0. A normal recursive call
      //                 would write the NR52 memory several times unnecessarily
      if (!updatedEnabledFlag) { return; }

      // We compare to see if we have to change the NR52 byte
      if ((channel1Enabled != _channel1.Enabled) ||
          (channel2Enabled != _channel2.Enabled) ||
          (channel3Enabled != _channel3.Enabled) ||
          (channel4Enabled != _channel4.Enabled) ||
          (prevEnabled != _state.Enabled))
      {
        byte nr52 = 0x70;
        if (_state.Enabled)
        {
          nr52 = (byte)((_channel1.Enabled ? 0x1 : 0) |  // bit 0
                        (_channel2.Enabled ? 0x2 : 0) |  // bit 1
                        (_channel3.Enabled ? 0x4 : 0) |  // bit 2
                        (_channel4.Enabled ? 0x8 : 0) |  // bit 3
                        0xF0);                           // bit 4-7 are 1
        }

        // We know bit 7 is 1 because otherwise the whole register is 0x70
        _memory.LowLevelWrite((ushort)MMR.NR52, nr52);
      }
    }

    internal byte HandleWaveRead(ushort address)
    {
      if (_channel3.Enabled)
      {
        byte wave = _channel3.HandleWaveRead();
        return wave;
      }

      // If the channel is off, we return the actual data
      return _memory.LowLevelRead(address);
    }

    internal void HandleWaveWrite(ushort address, byte value)
    {
      _channel3.HandleWaveWrite(address, value);
    }

    internal void Step(int ticks)
    {
#if SoundTiming
      if (firstStep)
      {
        firstStep = false;
        sw.Start();
        RecordSeparateChannels = true;
        StartRecording();
      }
#endif

      _frameSequencer.Step((uint)ticks);
      _channel1.Step(ticks);
      _channel2.Step(ticks);
      _channel3.Step(ticks);
      _channel4.Step(ticks);
    }

    public void GenerateSamples(int fullSampleCount)
    {
      ClearBuffer();

      int ticksPerSample = APU.MinimumTickThreshold;
      if (_gameboy.FastEmulation)
      {
        DepleteSoundEventQueues();
      }

      // If the channels are disabled, all the channels will output 0
      _channel1.GenerateSamples(fullSampleCount, ticksPerSample);
      _channel2.GenerateSamples(fullSampleCount, ticksPerSample);
      _channel3.GenerateSamples(fullSampleCount, ticksPerSample);
      _channel4.GenerateSamples(fullSampleCount, ticksPerSample);

      // We transformate the samples
      int _channelSampleIndex = 0;
      for (int i = 0; i < fullSampleCount; ++i)
      {
        // LEFT CHANNEL
        short leftSample = 0;
        short c1LeftSample = 0;
        short c2LeftSample = 0;
        short c3LeftSample = 0;
        short c4LeftSample = 0;
        if (_state.Enabled && _state.LeftChannelEnabled)
        {
          // We add the correspondant samples
          if (_channel1.Enabled)
          {
            c1LeftSample = _channel1.Buffer[_channelSampleIndex];
            if (Channel1Run && _state.OutputChannel1Left) { leftSample += c1LeftSample; }
          }
          if (_channel2.Enabled)
          {
            c2LeftSample = _channel2.Buffer[_channelSampleIndex];
            if (Channel2Run && _state.OutputChannel2Left) { leftSample += c2LeftSample; }
          }
          if (_channel3.Enabled)
          {
            c3LeftSample = _channel3.Buffer[_channelSampleIndex];
            if (Channel3Run && _state.OutputChannel3Left) { leftSample += c3LeftSample; }
          }
          if (_channel4.Enabled)
          {
            c4LeftSample = _channel4.Buffer[_channelSampleIndex];
            if (Channel4Run && _state.OutputChannel4Left) { leftSample += c4LeftSample; }
          }
        }
        ++_channelSampleIndex;
        //  TODO(Cristian): post-process mixed sample?
        _buffer[_sampleIndex++] = (byte)leftSample;
        _buffer[_sampleIndex++] = (byte)(leftSample >> 8);

        // RIGHT CHANNEL
        short rightSample = 0;
        short c1RightSample = 0;
        short c2RightSample = 0;
        short c3RightSample = 0;
        short c4RightSample = 0;
        if (_state.Enabled && _state.RightChannelEnabled)
        {
          // We add the correspondant samples
          if (_channel1.Enabled)
          {
            c1RightSample = _channel1.Buffer[_channelSampleIndex];
            if (Channel1Run && _state.OutputChannel1Right) { rightSample += c1RightSample; }
          }
          if (_channel2.Enabled)
          {
            c2RightSample = _channel2.Buffer[_channelSampleIndex];
            if (Channel2Run && _state.OutputChannel2Right) { rightSample += c2RightSample; }
          }
          if (_channel3.Enabled)
          {
            c3RightSample = _channel3.Buffer[_channelSampleIndex];
            if (Channel3Run && _state.OutputChannel3Right) { rightSample += c3RightSample; }
          }
          if (_channel4.Enabled)
          {
            c4RightSample = _channel4.Buffer[_channelSampleIndex];
            if (Channel4Run && _state.OutputChannel4Right) { rightSample += c4RightSample; }
          }
        }
        ++_channelSampleIndex;
        //  TODO(Cristian): post-process mixed sample?
        _buffer[_sampleIndex++] = (byte)rightSample;
        _buffer[_sampleIndex++] = (byte)(rightSample >> 8);

        if (_wavExporter.Recording)
        {
          _wavExporter.WriteSamples(leftSample, rightSample);

          if (RecordSeparateChannels)
          {
            _channel1WavExporter.WriteSamples(c1LeftSample, c1RightSample);
            _channel2WavExporter.WriteSamples(c2LeftSample, c2RightSample);
            _channel3WavExporter.WriteSamples(c3LeftSample, c3RightSample);
            _channel4WavExporter.WriteSamples(c4LeftSample, c4RightSample);
          }
        }
      }
    }

    /// <summary>
    /// This method is intended for when the fast emulation mode ends
    /// so the sound channels synchronize (deplete) their events queues
    /// </summary>
    internal void DepleteSoundEventQueues()
    {
      _channel1.DepleteSoundEventQueue();
      _channel2.DepleteSoundEventQueue();
      _channel3.DepleteSoundEventQueue();
      _channel4.DepleteSoundEventQueue();
    }

    internal void EndFrame()
    {
      _wavExporter.UpdateExporter();
      if (RecordSeparateChannels)
      {
        _channel1WavExporter.UpdateExporter();
        _channel2WavExporter.UpdateExporter();
        _channel3WavExporter.UpdateExporter();
        _channel4WavExporter.UpdateExporter();
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
      _channel1.ClearBuffer();
      _channel2.ClearBuffer();
      _channel3.ClearBuffer();
      _channel4.ClearBuffer();
    }

#if SoundTiming
    ~APU()
    {
      WriteOutput();
    }
#endif

    public void StartRecording(string filename = null)
    {
      if (Recording) { return; }
      if (filename == null) { filename = CartridgeFilename; }
      string finalFilename = _wavExporter.StartRecording(filename);

      if (RecordSeparateChannels)
      {
        _channel1WavExporter.StartRecording(finalFilename, 1);
        _channel2WavExporter.StartRecording(finalFilename, 2);
        _channel3WavExporter.StartRecording(finalFilename, 3);
        _channel4WavExporter.StartRecording(finalFilename, 4);
      }
    }

    public void StopRecording()
    {
      _wavExporter.StopRecording();
      _channel1WavExporter.StopRecording();
      _channel2WavExporter.StopRecording();
      _channel3WavExporter.StopRecording();
      _channel4WavExporter.StopRecording();
    }

    public void Dispose()
    {
      _wavExporter.Dispose();
      _channel1WavExporter.Dispose();
      _channel2WavExporter.Dispose();
      _channel3WavExporter.Dispose();
      _channel4WavExporter.Dispose();
    }

#if SoundTiming
    public void WriteOutput()
    {
      try
      {
        using (var file = new StreamWriter("sound_events.csv", false))
        {
          file.WriteLine("{0},{1},{2}", "Ms", "Event", "Value");
          for (uint i = 0; i < TimelineLocalCount; i += 3)
          {
            //file.WriteLine("{0},{1},{2}",
            //               Timeline[i],
            //               //"0x" + Timeline[i + 1].ToString("x2").ToUpper());
            //               Timeline[i + 1],
            //               Timeline[i + 2]);
            NoiseChannelEvents soundEvent = (NoiseChannelEvents)TimelineLocal[i + 1];
            long soundEventValue = TimelineLocal[i + 2];
            string value;
            if (soundEvent == NoiseChannelEvents.VOLUME_CHANGE)
            {
              value = soundEventValue.ToString();
            }
            else if (soundEvent == NoiseChannelEvents.ENABLED_CHANGE)
            {
              if (soundEventValue == 1)
              {
                value = "ON";
              }
              else
              {
                value = String.Format("OFF");
              }
            }
            else if ((soundEvent == NoiseChannelEvents.LSFR_CHANGE) ||
                     (soundEvent == NoiseChannelEvents.LSFR_CHANGE2) ||
                     (soundEvent == NoiseChannelEvents.LSFR_CHANGE3))
            {
              value = Convert.ToString(soundEventValue, 2);
            }
            else
            {
              value = "0x" + soundEventValue.ToString("x2");
            }

            file.WriteLine("{0},{1},{2}", TimelineLocal[i],
                                          soundEvent,
                                          value);
          }
        }
      }
      catch (IOException)
      {
        // Probably because the csv is opened by a program. Whatever...
      }
    }
#endif
  }

}
