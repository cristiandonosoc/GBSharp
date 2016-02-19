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

    public SoundChannelInternalsViewModel(IGameBoy gameboy)
    {
      _gameboy = gameboy;
    }

    public void Reload()
    {
      FrameSequencer = "0x" + _gameboy.APU.Channel1.FrameSequencerCounter.ToString("x2");
    }
  }
}
