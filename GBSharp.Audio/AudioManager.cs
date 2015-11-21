﻿using CSCore;
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
      _soundOut.Volume = 0.05f;
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
