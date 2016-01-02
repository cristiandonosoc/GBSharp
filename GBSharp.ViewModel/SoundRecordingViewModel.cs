using System.Windows.Input;

namespace GBSharp.ViewModel
{
  /// <summary>
  /// This class contains properties that a View can data bind to.
  /// <para>
  /// See http://www.galasoft.ch/mvvm
  /// </para>
  /// </summary>
  public class SoundRecordingViewModel : ViewModelBase
  {

    private bool _playChannel1 = true;
    public bool PlayChannel1
    {
      get { return _playChannel1; }
      set
      {
        if(_playChannel1 == value) { return; }
        _playChannel1 = value;
        _apu.Channel1Run = value;
        OnPropertyChanged(() => PlayChannel1);
      }
    }

    private bool _playChannel2 = true;
    public bool PlayChannel2
    {
      get { return _playChannel2; }
      set
      {
        if(_playChannel2 == value) { return; }
        _playChannel2 = value;
        _apu.Channel2Run = value;
        OnPropertyChanged(() => PlayChannel2);
      }
    }

    private bool _playChannel3 = true;
    public bool PlayChannel3
    {
      get { return _playChannel3; }
      set
      {
        if(_playChannel3 == value) { return; }
        _playChannel3 = value;
        _apu.Channel3Run = value;
        OnPropertyChanged(() => PlayChannel3);
      }
    }

    private readonly IAPU _apu;

    private bool _recordSeparateChannels;
    public bool RecordSeparateChannels
    {
      get { return _recordSeparateChannels; }
      set
      {
        if(_recordSeparateChannels == value) { return; }
        _recordSeparateChannels = value;
        _apu.RecordSeparateChannels = value;
        OnPropertyChanged(() => RecordSeparateChannels);
      }
    }

    public bool CartridgeLoaded { get; set; }

    public SoundRecordingViewModel(IAPU apu)
    {
      _apu = apu;
    }

    public ICommand StartRecordingCommand
    {
      get { return new DelegateCommand(StartRecordingWrapper); }
    }

    public void StartRecordingWrapper()
    {
      if(!CartridgeLoaded) { return; }
      _apu.StartRecording();
    }

    public ICommand StopRecordingCommand
    {
      get { return new DelegateCommand(StopRecordingWrapper); }
    }

    public void StopRecordingWrapper()
    {
      _apu.StopRecording();
    }
  }
}