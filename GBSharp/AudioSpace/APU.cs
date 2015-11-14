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
        _audioVisualization = new uint[VisWidth * VisHeight];

      }
    }

    #endregion

    public int VisWidth { get; private set; }
    public int VisHeight { get; private set; }

    private uint[] _audioVisualization;
    public uint[] AudioVisualization { get { return _audioVisualization; } }

    public bool Running { get; set; }

    public APU()
    {
      VisWidth = 1000;
      VisHeight = 50;
      Running = false;
    }

    public void RefreshVisualization()
    {
      return;
      if(!Running) { return; }

      int length = _audioStream.SampleRate * _audioStream.ChannelCount * _audioStream.Milliseconds / 1000;
      int jump = length / VisWidth;

      int playCursor, writeCursor, scan;

      lock (_audioVisualization)
      {
        scan = 0;
        lock(_audioStream)
        {
          playCursor = (int)(((double)_audioStream.PlayCursor / (double)length) * VisWidth);
          writeCursor = (int)(((double)_audioStream.WriteCursor / (double)length) * VisWidth);
        }
        // We clear the buffer
        for (int x = 0; x < VisWidth; ++x)
        {
          if (playCursor == x)
          {
            for (int y = 0; y < VisHeight; ++y)
            {
              _audioVisualization[y * VisWidth + playCursor] = 0xFFFFFF00;
            }

            continue;
          }

          if (writeCursor == x)
          {
            for (int y = 0; y < VisHeight; ++y)
            {
              _audioVisualization[y * VisWidth + writeCursor] = 0xFFFF0000;
            }

            continue;
          }

          // We clear
          for (int y = 0; y < VisHeight; ++y)
          {
            _audioVisualization[y * VisWidth + x] = 0;
          }

          // We draw
          int height = (int)(((double)_audioStream.Buffer[scan] / (double)256) * VisHeight);
          _audioVisualization[height * VisWidth + x] = 0xFFFFFF00;
          scan += jump;
        }



      }

      // We show the PlayCursor and the WriteCursor
      //uint playColor = 0xFFFFFF00;
      //uint writeColor = 0xFFFF0000;

      //long playCursor = _audioStream.PlayCursor;
      //long writeCursor = _audioStream.WriteCursor;

      //System.Console.Out.WriteLine("PC: {0}, WC: {0}", playCursor, writeCursor);

      //for(int i = 0; i < _visHeight; ++i)
      //{
      //  _audioVisualization[i * _visWidth + playCursor] = playColor;
      //  _audioVisualization[i * _visWidth + writeCursor] = writeColor;
      //}
    }
  }
}
