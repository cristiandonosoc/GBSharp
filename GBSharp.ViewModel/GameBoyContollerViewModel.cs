using System.IO;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class GameBoyContollerViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;

    private string _filePath;
    private string _cartridgeTitle;

    public string FilePath
    {
      get { return _filePath; }
      set
      {
        if (_filePath != value)
        {
          _filePath = value;
          NotifyCartridgeFileLoaded();
        }
      }
    }

    public string CartridgeTitle
    {
      get { return _cartridgeTitle; }
      set
      {
        if (_cartridgeTitle != value)
        {
          _cartridgeTitle = value;
          OnPropertyChanged(() => CartridgeTitle);
        }
      }
    }

    public ICommand RunCommand
    {
      get { return new DelegateCommand(Run); }
    }

    public ICommand StepCommand
    {
      get { return new DelegateCommand(Step); }
    }


    public ICommand PauseCommand
    {
      get { return new DelegateCommand(Pause); }
    }

    public ICommand StopCommand
    {
      get { return new DelegateCommand(Stop); }
    }

    public GameBoyContollerViewModel(IGameBoy gameBoy)
    {
      _gameBoy = gameBoy;
      //_gameBoy.StepFinished += OnStepFinished;
    }

    private void NotifyCartridgeFileLoaded()
    {
      if (File.Exists(_filePath))
        OnCartridgeFileLoaded(File.ReadAllBytes(_filePath));
    }

    private void Run()
    {
      _gameBoy.Run();
    }

    private void Step()
    {
      _gameBoy.Step();
    }

    private void Pause()
    {
      _gameBoy.Pause();
    }

    private void Stop()
    {
      _gameBoy.Stop();
    }

    private void OnStepFinished()
    {
      //_dispatcher.Invoke(PrintCPU);
    }
    
    private void OnCartridgeFileLoaded(byte[] data)
    {
      _gameBoy.LoadCartridge(data);
      CartridgeTitle = _gameBoy.Cartridge.Title;
    }

   
    
  }
}