using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  class WavExporter : IDisposable
  {
    private BinaryWriter _wavWritter;

    private int _currentWavSamples = 0;
    private const int _wavBufferLength = 44000 * 2 / 10;
    private short[] _wavBuffer1 = new short[_wavBufferLength];
    private short[] _wavBuffer2 = new short[_wavBufferLength];
    private bool _wavBuffer1Active = true;
    internal short[] ActiveWavBuffer
    {
      get { return _wavBuffer1Active ? _wavBuffer1 : _wavBuffer2; }
    }

    internal short[] BackWavBuffer
    {
      get { return _wavBuffer1Active ? _wavBuffer2 : _wavBuffer1; }
    }

    private int _wavBuffersWritten;
    private bool _wavBufferReady;

    internal bool Recording { get; private set; }

    internal WavExporter()
    {
    }

    private void WriteWavHeader()
    {
      _wavWritter.Write(new char[] { 'R', 'I', 'F', 'F' });
      _wavWritter.Write(0);                                   // File size (added later)
      _wavWritter.Write(new char[] { 'W', 'A', 'V', 'E' });

      _wavWritter.Write(new char[] { 'f', 'm', 't', ' ' });
      _wavWritter.Write(16);                                  // Pre header size
      _wavWritter.Write((short)1);                            // Type: PCM
      _wavWritter.Write((short)2);                            // Num Channels
      _wavWritter.Write(44000);                               // Sample Rate
      _wavWritter.Write(44000 * 16 * 2 / 8);                  // SampleRate*BitsPerSample*NumChannels/8
      _wavWritter.Write((short)4);                            // 16-bit stereo
      _wavWritter.Write((short)16);                           // Bits per sample

      _wavWritter.Write(new char[] { 'd', 'a', 't', 'a' });
      _wavWritter.Write(0);                                   // Data size (added later)
    }

    private void WriteBuffer()
    {
      int byteSample = 0;
      byte[] byteBuffer = new byte[BackWavBuffer.Length * 2];
      int i = 0;
      while (i < BackWavBuffer.Length)
      {
        short sample = BackWavBuffer[i++];
        byteBuffer[byteSample++] = (byte)sample;
        byteBuffer[byteSample++] = (byte)(sample >> 8);
      }

      _wavWritter.Write(byteBuffer);
      ++_wavBuffersWritten;
    }

    // Hopefully this bullshit gets inlined
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteSamples(short leftSample, short rightSample)
    {
      // We output the wav
      ActiveWavBuffer[_currentWavSamples++] = leftSample;
      ActiveWavBuffer[_currentWavSamples++] = rightSample;

      // We check if we have to change the buffers
      if (_currentWavSamples >= _wavBufferLength)
      {
        _wavBuffer1Active = !_wavBuffer1Active;
        _currentWavSamples = 0;
        _wavBufferReady = true;
      }
    }

    internal void UpdateExporter()
    {
      if (!Recording) { return; }

      if(_wavBufferReady)
      {
        WriteBuffer();
        _wavBufferReady = false;
      }
    }

    internal string StartRecording(string filename)
    {
      if (Recording) { return ""; }
      Recording = true;

      int fileCounter = 0;
      string newFilenameWithoutExtension = filename;
      string newFilename = String.Format("{0}.wav", newFilenameWithoutExtension);
      while(File.Exists(newFilename))
      {
        newFilenameWithoutExtension = String.Format("{0}_{1}", filename, fileCounter);
        newFilename = String.Format("{0}.wav", newFilenameWithoutExtension);
        ++fileCounter;
      }

      _wavWritter = new BinaryWriter(new FileStream(newFilename, FileMode.Create));
      WriteWavHeader();

      return newFilenameWithoutExtension;
    }

    internal string StartRecording(string filename, int channelNum)
    {
      return StartRecording(String.Format("{0}_channel{1}", filename, channelNum));
    }

    internal void EndRecording()
    {
      if (!Recording) { return; }
      Recording = false;

      // We close the wav buffer
      // File Size 
      _wavWritter.Seek(4, SeekOrigin.Begin);
      int dataLenght = _wavBuffersWritten * BackWavBuffer.Length * 2;
      _wavWritter.Write(dataLenght + 44 - 8);
      // Data Size 
      _wavWritter.Seek(40, SeekOrigin.Begin);
      _wavWritter.Write(dataLenght);
      _wavWritter.Close();
    }

    public void Dispose()
    {
      if(Recording)
      {
        EndRecording();
      }
    }
  }
}
