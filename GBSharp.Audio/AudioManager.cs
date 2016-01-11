using CSCore;
using CSCore.Streams;
using System;
using CSCore.SoundOut;
using System.Diagnostics;

namespace GBSharp.Audio
{
  public class AudioManager : IDisposable
  {
    private IGameBoy _gameboy;
    private IAPU _apu;
    private WaveFormat _waveFormat;
    private DirectStreamingSource _source;
    private ISoundOut _soundOut;

    public AudioManager(IGameBoy gameboy)
    {
      _gameboy = gameboy;
      gameboy.FrameCompleted += gameBoy_FrameCompleted;
      gameboy.PauseRequested += GameBoy_PauseRequested;
      gameboy.StopRequested += GameBoy_StopRequested;
    }

    private void ReloadAPU()
    {
      _apu = _gameboy.APU;
      _waveFormat = new WaveFormat(_apu.SampleRate,
                                   _apu.SampleSize * 8,
                                   _apu.NumChannels);
      _source = new DirectStreamingSource(_waveFormat, _apu);
      _soundOut = GetSoundOut();
      _soundOut.Initialize(_source);
      _soundOut.Volume = 0.2f;
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
        // The soundout will be reloaded after the first frame
        // of gameplay passes
        _soundOut.Stop();
        _soundOut.Dispose();
        _soundOut = null;
      }
    }

    ~AudioManager()
    {

    }

    void gameBoy_FrameCompleted()
    {
      if (_soundOut == null)
      {
        ReloadAPU();
      }

      if ((_soundOut.PlaybackState != PlaybackState.Playing)) { 
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
