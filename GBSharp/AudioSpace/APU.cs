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

        // We resize the audioVisualization
        _visWidth = (_audioStream.Milliseconds / 1000) * 50;
        _audioVisualization = new uint[_visWidth * _visHeight];
      }
    }

    #endregion

    private int _visHeight = 10;
    private int _visWidth;

    private uint[] _audioVisualization;
    public uint[] AudioVisualization { get { return _audioVisualization; } }

    public bool Running { get; set; }

    public APU()
    {
      Running = false;
    }

    public void RefreshVisualization()
    {
      if (!Running) { return; }
      // We clear the buffer
      for (int i = 0; i < _audioVisualization.Length; ++i)
      {
        _audioVisualization[i] = 0;
      }

      // We show the PlayCursor and the WriteCursor
      uint playColor = 0xFFFFFF00;
      uint writeColor = 0xFFFF0000;

      long playCursor = _audioStream.PlayCursor;
      long writeCursor = _audioStream.WriteCursor;

      System.Console.Out.WriteLine("PC: {0}, WC: {0}", playCursor, writeCursor);

      //for(int i = 0; i < _visHeight; ++i)
      //{
      //  _audioVisualization[i * _visWidth + playCursor] = playColor;
      //  _audioVisualization[i * _visWidth + writeCursor] = writeColor;
      //}
    }
  }
}
