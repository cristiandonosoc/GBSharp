using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.ViewModel
{
  public class SoundChannelInternalsViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameboy;

    private string _frameSequencer;
    public string FrameSequencer
    {
      get { return _frameSequencer; }
      set
      {
        _frameSequencer = value;
        OnPropertyChanged(() => FrameSequencer);
      }
    }

    private string _soundLengthCounter;
    public string SoundLengthCounter
    {
      get { return _soundLengthCounter; }
      set
      {
        _soundLengthCounter = value;
        OnPropertyChanged(() => SoundLengthCounter);
      }
    }

    private bool _continuousOutput;
    public bool ContinuousOutput
    {
      get { return _continuousOutput; }
      set
      {
        _continuousOutput = value;
        OnPropertyChanged(() => ContinuousOutput);
      }
    }

    private string _sweepCounter;
    public string SweepCounter
    {
      get { return _sweepCounter; }
      set
      {
        _sweepCounter = value;
        OnPropertyChanged(() => SweepCounter);
      }
    }

    private string _sweepFrequency;
    public string SweepFrequency
    {
      get { return _sweepFrequency; }
      set
      {
        _sweepFrequency = value;
        OnPropertyChanged(() => SweepFrequency);
      }
    }

    private string _sweepLength;
    public string SweepLength
    {
      get { return _sweepLength; }
      set
      {
        _sweepLength = value;
        OnPropertyChanged(() => SweepLength);
      }
    }

    private string _sweepShifts;
    public string SweepShifts
    {
      get { return _sweepShifts; }
      set
      {
        _sweepShifts = value;
        OnPropertyChanged(() => SweepShifts);
      }
    }

    private bool _sweepUp;
    public bool SweepUp
    {
      get { return _sweepUp; }
      set
      {
        _sweepUp = value;
        OnPropertyChanged(() => SweepUp);
      }
    }

    internal SoundChannelInternalsViewModel(IGameBoy gameboy)
    {
      _gameboy = gameboy;
    }

    internal void Reload()
    {
      FrameSequencer = "0x" + _gameboy.APU.Channel1.FrameSequencerCounter.ToString("x2");

      SoundLengthCounter = "0x" + _gameboy.APU.Channel1.SoundLengthCounter.ToString("x2");
      ContinuousOutput = _gameboy.APU.Channel1.ContinuousOutput;

      SweepCounter = "0x" + _gameboy.APU.Channel1.SweepCounter.ToString("x2");
      SweepFrequency = "0x" + _gameboy.APU.Channel1.SweepFrequencyRegister.ToString("x2");
      SweepLength = "0x" + _gameboy.APU.Channel1.SweepLength.ToString("x2");
      SweepShifts = _gameboy.APU.Channel1.SweepShifts.ToString();
      SweepUp = _gameboy.APU.Channel1.SweepUp;
    }

    internal void Clear()
    {
      FrameSequencer = "";

      SoundLengthCounter = "";
      ContinuousOutput = false;

      SweepCounter = "";
      SweepFrequency = "";
      SweepLength = "";
      SweepShifts = "";
      SweepUp = false;
    }
  }
}
