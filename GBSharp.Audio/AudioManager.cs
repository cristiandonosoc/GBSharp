using CSCore;
using CSCore.Streams;
using System;
using CSCore.SoundOut;

namespace GBSharp.Audio
{
  public class AudioManager : IDisposable
  {
    private IAPU _apu;

    private WaveFormat _waveFormat;
    private CircularWriteableBufferingSource _source;

    private ISoundOut _soundOut;


    public AudioManager(IGameBoy gameBoy)
    {
      _apu = gameBoy.APU;
      _waveFormat = new WaveFormat(_apu.SampleRate,
                                   _apu.SampleSize * 8,
                                   _apu.NumChannels);
      // NOTE(Cristian): At 4400 bytes means 50 ms worth of audio.
      //                 This low is for having low latency
      _source = new CircularWriteableBufferingSource(_waveFormat, 88 * 5000, 50);

      gameBoy.FrameCompleted += gameBoy_FrameCompleted;

      _soundOut = GetSoundOut();
      _soundOut.Initialize(_source);
      _soundOut.Volume = 0.05f;
    }

    ~AudioManager()
    {

    }

    private bool _firstRun = true;

    void gameBoy_FrameCompleted()
    {
      if(_firstRun)
      {
        _source.SetWriteCursor();
        _firstRun = false;
      }
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
