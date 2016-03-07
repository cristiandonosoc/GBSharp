using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal interface IChannel
  {
    int SampleRate { get; }
    int NumChannels { get; }
    int SampleSize { get; }

    short[] Buffer { get; }
    int SampleCount { get; }

    bool Enabled { get; }
    int Volume { get; }

    void HandleMemoryChange(MemorySpace.MMR register, byte value);
    void GenerateSamples(int sampleCount);
    void ClearBuffer();

    void PowerOff();
    void ChangeLength(byte value);

  }
}
