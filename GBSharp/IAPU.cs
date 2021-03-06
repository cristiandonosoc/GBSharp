﻿using GBSharp.AudioSpace;

namespace GBSharp
{
  public interface IAPU
  {

    int SampleRate { get; }
    int NumChannels { get; }
    int SampleSize { get; }

    byte[] Buffer { get; }
    int SampleCount { get; }

    void GenerateSamples(int sampleCount);
    void ClearBuffer();

    bool RecordSeparateChannels { get; set; }
    void StartRecording(string filename = null);
    void StopRecording();

    bool Channel1Run { get; set; }
    bool Channel2Run { get; set; }
    bool Channel3Run { get; set; }

    ISquareChannel Channel1 { get; }
    IWaveChannel Channel3 { get; }

    FrameSequencer FrameSequencerTimer { get; }
  }
}