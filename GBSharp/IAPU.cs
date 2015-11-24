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
  }
}