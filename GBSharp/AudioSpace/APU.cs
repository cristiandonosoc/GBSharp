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

    #region BUFFER DEFINITION

    private AudioBuffer _audioStream;
    public AudioBuffer AudioStream
    {
      get { return _audioStream; }
      set
      {
        // TODO(Cristian): Restart the audio playback if currently running
        _audioStream = value;
      }
    }


   #endregion

    public APU()
    {
    }
  }
}
