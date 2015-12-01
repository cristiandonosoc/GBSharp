using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal interface ISoundChannel
  {
    float[] Buffer { get; }
    int SampleCount { get; }

    void HandleMemoryChange(MMR register, byte value);
    void GenerateSamples(int sampleCount);
    void ClearBuffer();
  }
}
