using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.AudioSpace;

namespace GBSharp
{
  public interface IWaveChannel 
  {
    // Sound Length
    int SoundLengthCounter { get; }
    bool ContinuousOutput { get; }
    
    byte CurrentSampleIndex { get; }
    byte CurrentSample { get; }
  }
}
