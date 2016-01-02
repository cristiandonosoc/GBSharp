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