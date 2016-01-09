using CSCore;
using CSCore.Streams;
using System;
using CSCore.SoundOut;
using System.Diagnostics;

namespace GBSharp.Audio
{
  public class AudioManager : IDisposable
  {
    private IAPU _apu;

    private WaveFormat _waveFormat;
    //private CircularWriteableBufferingSource _source;
    private DirectStreamingSource _source;

    private ISoundOut _soundOut;


    public AudioManager(IGameBoy gameBoy)
    {
      _apu = gameBoy.APU;
      _waveFormat = new WaveFormat(_apu.SampleRate,
                                   _apu.SampleSize * 8,
                                   _apu.NumChannels);
      // NOTE(Cristian): At 4400 bytes means 50 ms worth of audio.
      //                 This low is for having low latency
      //_source = new CircularWriteableBufferingSource(_waveFormat, 88 * 5000, 120);
      _source = new DirectStreamingSource(_waveFormat, _apu);

      gameBoy.FrameCompleted += gameBoy_FrameCompleted;

      _soundOut = GetSoundOut();
      _soundOut.Initialize(_source);
      _soundOut.Volume = 0.2f;

      sw = new Stopwatch();

      gameBoy.PauseRequested += GameBoy_PauseRequested;
      gameBoy.StopRequested += GameBoy_StopRequested;
    }

    private void GameBoy_PauseRequested()
    {
      if(_soundOut.PlaybackState == PlaybackState.Playing)
      {
        _soundOut.Pause();
      }
    }

    private void GameBoy_StopRequested()
    {
      if(_soundOut.PlaybackState == PlaybackState.Playing)
      {
        _soundOut.Stop();
      }
    }

    ~AudioManager()
    {

    }

    Stopwatch sw;

    private bool _firstRun = true;
    int _prevRC = 0;


    ulong _RCSum = 0;
    ulong _RCSamples = 0;


    int _prevWC = 0;
    ulong _WCSum = 0;
    ulong _WCSamples = 0;


    void gameBoy_FrameCompleted()
    {
      #region REPORTING
#if false

      ulong avgRC = 0;
      int RCDiff = _source.ReadCursor - _prevRC;
      if (Math.Abs(RCDiff) < 5000)
      {
        _RCSum += (ulong)RCDiff;
        ++_RCSamples;
        if (_RCSamples != 0)
        {
          avgRC = _RCSum / _RCSamples;
        }
      }

      ulong avgWC = 0;
      int WCDiff = _source.WriteCursor - _prevWC;
      if (Math.Abs(WCDiff) < 10000)
      {
        _WCSum += (ulong)WCDiff;
        ++_WCSamples;
        if (_WCSamples != 0)
        {
          avgWC = _WCSum / _WCSamples;
        }
      }

      sw.Stop();

      System.Console.Out.WriteLineAsync("MS: " + sw.ElapsedMilliseconds.ToString());

      //System.Console.Out.WriteLine("MS {7} | RC: {0}, WC: {1} | RCDIFF: {2}, RCDIFFAVG: {3} | WCDIFF: {4}, WCDIFFAVG: {5} | TOTALDIFF: {6}",
      //                             _source.ReadCursor, _source.WriteCursor,
      //                             RCDiff, avgRC,
      //                             WCDiff, avgWC,
      //                             _source.WriteCursor - _source.ReadCursor,
      //                             sw.ElapsedMilliseconds);

      _prevRC = _source.ReadCursor;
      _prevWC = _source.WriteCursor;

      sw.Reset();
      sw.Start();

#endif
      #endregion

      if(_soundOut.PlaybackState != PlaybackState.Playing)
      {
        _soundOut.Play();
      }
    }

    private static ISoundOut GetSoundOut()
    {
      if (WasapiOut.IsSupportedOnCurrentPlatform)
        return new WasapiOut();
      else
        return new DirectSoundOut();
    }

    private bool _startPlayback = false;

    // TODO(Cristian): Improve this management!!!!!
    public void Play()
    {
      _startPlayback = true;
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
