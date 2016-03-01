using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.AudioSpace;

namespace GBSharp
{
  public interface ISquareChannel
  {
    // Sound Length
    int SoundLengthCounter { get; }
    bool ContinuousOutput { get; }

    // Sweep
    int SweepFrequencyRegister { get; }
    int SweepShifts { get; }
    int SweepLength { get; }
    int SweepCounter { get; }
    bool SweepUp { get; }
  }
}
