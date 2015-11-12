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
using System.Diagnostics;

namespace GBSharp.Audio
{
  public class Test
  {
    public static void Main(string[] args)
    {
      int sampleRate = 44000;
      int freq = 440;

      double ratio = (double)sampleRate / (double)freq;
      byte[] wave440 = new byte[sampleRate * 2 * 10];
      byte[] wave880 = new byte[sampleRate * 2 * 10];
      for(int i = 0; i < wave440.Length; i = i + 2)
      {
        double index440 = (double)i / ratio;
        double index880 = 2 * (double)i / ratio;

        wave440[i] = (byte)(Math.Sin(index440 * Math.PI) * 26);
        wave440[i + 1] = wave440[i];

        wave880[i] = (byte)(Math.Sin(index880 * Math.PI) * 128);
        wave880[i + 1] = wave880[i];
      }

      LoopStream stream = new LoopStream(wave440);

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

          int count = sampleRate * 2;

          Stopwatch sw = new Stopwatch();
          long before, after;
          int diff;
          int samplesToCommit;
          bool switchWave = true;
          bool firstPass = true;
          before = sw.ElapsedMilliseconds;
          sw.Start();

          for (int i = 0; i < 30; ++i)
          {

            Thread.Sleep(1000);

            sw.Stop();
            after = sw.ElapsedMilliseconds;


            if (firstPass)
            {
              stream.CreateWriteCursor();
              firstPass = false;
            }

            diff = (int)(after - before);

            before = sw.ElapsedMilliseconds;
            sw.Start();

            samplesToCommit = 88 * diff;

            stream.Write(switchWave ? wave440 : wave880,
                         0, samplesToCommit);
            switchWave = !switchWave;

            System.Console.Out.WriteLine("PC: {0}, WC: {1}",
                                         (int)stream.Position,
                                         (int)stream.WriteCursor);
          }

          soundOut.Stop();



          #region TIMING TEST

#if false
          long min = 10000000000000;
          long max = 0;
          long total = 0;
          long before, after, diff;
          long avg = 0;
          Stopwatch sw = new Stopwatch();
          double tickRatio = (double)1000000000 / (double)Stopwatch.Frequency;


          long samples = 1000;
          for (int i = 1; i < samples; ++i)
          {
            before = sw.ElapsedTicks;
            sw.Start();

            Thread.Sleep(5);

            sw.Stop();
            after = sw.ElapsedTicks;

            diff = after - before;
            total += diff;
            avg = total / i;

            if (diff < min) { min = diff; }
            if (diff > max) { max = diff; }

            //System.Console.Out.WriteLine("Before: {0} ns, After: {1} ns, Diff: {2} ns",
            //                             tickRatio * before,
            //                             tickRatio * after,
            //                             tickRatio * (after - before));
          }

          System.Console.Out.WriteLine("Samples: {0}", samples);

          System.Console.Out.WriteLine("Min: {0} ns, Max: {1} ns, Avg: {2} ns",
                                       tickRatio * min,
                                       tickRatio * max,
                                       tickRatio * avg);

#endif

          #endregion

          System.Console.In.ReadLine();

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

    // Play Cursor
    private long _playCursor = 0;
    /// <summary>
    /// Identical to Position
    /// </summary>
    public long PlayCursor { get { return _playCursor; } }

    // Write Cursor
    private long _writeCursor = 0;
    public long WriteCursor { get { return _writeCursor; } }
    private bool _writeCursorCreated = false;
    public bool WriteCursorCreated { get { return _writeCursorCreated; } }

    // TODO(Cristian): unhadcode this
    private long _delay = 44 * 2 * 30; // 30 ms delay

    public LoopStream(byte[] buffer)
    {
      _buffer = buffer;
    }

    public override bool CanRead { get { return true; } }

    public override bool CanSeek { get { return true; } }

    public override bool CanWrite { get { return true; } }

    public override long Length { get { return _buffer.Length; } }

    public override long Position
    {
      get { return _playCursor; }
      set
      {
        _playCursor = value;
      }
    }

    public override void Flush()
    {
      for (int i = 0; i < _buffer.Length; ++i)
      {
        _buffer[i] = 0;
      }
      _playCursor = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      for (int i = 0; i < count; ++i)
      {
        buffer[offset + i] = _buffer[_playCursor++];
        
        // We loop
        if(_playCursor == _buffer.Length)
        {
          _playCursor = 0;
        }
      }

      //System.Console.Out.WriteLineAsync(_playCursor.ToString());

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
        resOffset = _playCursor + offset;
      }
      else
      {
        resOffset = _buffer.Length - 1 + offset;
      }

      while (resOffset >= _buffer.Length)
      {
        resOffset -= _buffer.Length;
      }
      _playCursor = resOffset;

      return _playCursor;
    }

    public override void SetLength(long value)
    {
      _buffer = new byte[value];
      _playCursor = 0;
      _writeCursorCreated = false;
    }

    public void CreateWriteCursor()
    {
      _writeCursor = _playCursor + _delay;
      while(_writeCursor >= _buffer.Length)
      {
        _writeCursor -= _buffer.Length;
      }

      _writeCursorCreated = true;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      for(int i = 0; i < count; ++i)
      {
        _buffer[_writeCursor++] = buffer[offset + i];

        if(_writeCursor == _playCursor)
        {
          throw new InvalidDataException("writeCursor cannot pass the playCursor");
        }

        if(_writeCursor == _buffer.Length)
        {
          _writeCursor = 0;
        }
      }
    }
  }
}
