using CSCore;
using CSCore.Streams;
using CSCore.Codecs.RAW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.SoundOut;
using System.Threading;

namespace GBSharp.Audio
{
  public class Test
  {
    public static void Main(string[] args)
    {
      byte[] wave = new byte[44000 * 2 * 1000];

      for(int i = 0; i < wave.Length; i = i + 2)
      {
        wave[i] = (byte)(Math.Sin(i/2) * 128);
        wave[i + 1] = wave[i];
      }

      LoopStream stream = new LoopStream(wave);

      var format = new WaveFormat(44000, 8, 2);
      RawDataReader reader = new RawDataReader(stream, format);

      using (IWaveSource soundSource = reader)
      {
        //SoundOut implementation which plays the sound
        using (ISoundOut soundOut = GetSoundOut())
        {
          //Tell the SoundOut which sound it has to play
          soundOut.Initialize(soundSource);
          //Play the sound
          soundOut.Play();

          while (true) ;
        }
      }
    }

    private static ISoundOut GetSoundOut()
    {
      if (WasapiOut.IsSupportedOnCurrentPlatform)
        return new WasapiOut();
      else
        return new DirectSoundOut();
    }
  }


  public class LoopStream : Stream
  {
    private byte[] _buffer;
    private long position = 0;

    public LoopStream(byte[] buffer)
    {
      _buffer = buffer;
    }

    public override bool CanRead
    {
      get { return true; }
    }

    public override bool CanSeek
    {
      get { return true; }
    }

    public override bool CanWrite
    {
      get { return true; }
    }

    public override long Length
    {
      get { return _buffer.Length; }
    }

    public override long Position
    {
      get { return position; }
      set
      {
        position = value;
      }
    }

    public override void Flush()
    {
      for (int i = 0; i < _buffer.Length; ++i)
      {
        _buffer[i] = 0;
      }
      position = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      for (int i = 0; i < count; ++i)
      {
        buffer[offset + i] = _buffer[position++];
        
        // We loop
        if(position == _buffer.Length)
        {
          position = 0;
        }
      }

      return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      long resOffset;
      if(origin == SeekOrigin.Begin)
      {
        resOffset = offset;

      }
      else if(origin == SeekOrigin.Current)
      {
        resOffset = position + offset;
      }
      else
      {
        resOffset = _buffer.Length - 1 + offset;
      }

      while (resOffset >= _buffer.Length)
      {
        resOffset -= _buffer.Length;
      }
      position = resOffset;

      return position;
    }

    public override void SetLength(long value)
    {
      _buffer = new byte[value];
      position = 0;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      for(int i = 0; i < count; ++i)
      {
        _buffer[position++] = buffer[offset + i];
        if(position == _buffer.Length)
        {
          position = 0;
        }
      }
    }
  }
}
