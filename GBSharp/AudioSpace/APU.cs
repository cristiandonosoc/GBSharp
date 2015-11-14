using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  /// <summary>
  /// Audio Processing Unit
  /// </summary>
  class APU : IAPU
  {
    int _sampleRate = 44000;
    int _numChannels = 2;
    int _sampleSize = 1;

    public APU()
    {
    }
  }
}
