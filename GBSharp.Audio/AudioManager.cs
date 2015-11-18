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
using GBSharp.AudioSpace;

namespace GBSharp.Audio
{
  public class AudioManager : IDisposable
  {
    private IAPU _apu;

    private WaveFormat _waveFormat;
    private WriteableBufferingSource _source;

    private ISoundOut _soundOut;


    public AudioManager(IGameBoy gameBoy)
    {
      _apu = gameBoy.APU;
      _waveFormat = new WaveFormat(_apu.SampleRate,
                                   _apu.SampleSize * 8,
                                   _apu.NumChannels);
      _source = new WriteableBufferingSource(_waveFormat);

      gameBoy.FrameCompleted += gameBoy_FrameCompleted;

      _soundOut = GetSoundOut();
      _soundOut.Initialize(_source);
      _soundOut.Volume = 0.7f;

      #region OLD TEST
#if false

      int sampleRate = 44000;
      int freq = 440;
      int milliseconds = 4000;
      int numChannels = 2;

      int bufferSize = sampleRate * numChannels * milliseconds / 1000;

      double ratio = (double)sampleRate / (double)freq;
      byte[] wave440 = new byte[bufferSize];
      byte[] wave880 = new byte[bufferSize];
      for(int i = 0; i < wave440.Length; i = i + numChannels)
      {
        double index440 = (double)i / ratio;
        double index880 = 2 * (double)i / ratio;

        wave440[i] = (byte)(Math.Sin(index440 * Math.PI) * 128);
        wave880[i] = (byte)(Math.Sin(index880 * Math.PI) * 128);

        for(int c = 1; c < numChannels; ++c)
        {
          wave440[i + c] = wave440[i];
          wave880[i + c] = wave880[i];
        }
      }

      WaveFormat format = new WaveFormat(44000, 8, numChannels);
      WriteableBufferingSource source = new WriteableBufferingSource(format, bufferSize);
      source.Write(wave440, 0, sampleRate * numChannels * 1);

      using (IWaveSource soundSource = source)
      {
        //SoundOut implementation which plays the sound
        using (ISoundOut soundOut = GetSoundOut())
        {
          //Tell the SoundOut which sound it has to play
          soundOut.Initialize(soundSource);

          soundOut.Volume = 0.7f;

          //Play the sound
          soundOut.Play();

          int count = sampleRate * 2;

          Stopwatch sw = new Stopwatch();
          long before, after;
          int diff;
          int samplesToCommit;
          bool switchWave = true;
          before = sw.ElapsedMilliseconds;
          sw.Start();

          for (int i = 0; i < 500; ++i)
          {

            Thread.Sleep(500);

            sw.Stop();
            after = sw.ElapsedMilliseconds;

            diff = (int)(after - before);
            before = sw.ElapsedMilliseconds;

            samplesToCommit = (sampleRate / 1000) * numChannels * diff;

            if(switchWave)
            {
              source.Write(wave440, 0, samplesToCommit);
            }
            else
            {
              source.Write(wave880, 0, samplesToCommit);
            }
            switchWave = !switchWave;

            sw.Start();
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
#endif

      #endregion
    }

    ~AudioManager()
    {

    }

    void gameBoy_FrameCompleted()
    {
      _source.Write(_apu.Buffer, 0, _apu.SampleCount);
    }

    private static ISoundOut GetSoundOut()
    {
      if (WasapiOut.IsSupportedOnCurrentPlatform)
        return new WasapiOut();
      else
        return new DirectSoundOut();
    }

    // TODO(Cristian): Improve this management!!!!!
    public void Play()
    {
      _soundOut.Play();
    }

    public void Stop()
    {
      _soundOut.Stop();
    }

    public void Dispose()
    {
      if(_soundOut.PlaybackState == PlaybackState.Playing)
      {
        _soundOut.Stop();
      }
      _soundOut.Dispose();
    }
  }


}
