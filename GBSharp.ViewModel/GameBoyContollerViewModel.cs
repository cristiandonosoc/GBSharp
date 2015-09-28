using System;
using System.IO;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class GameBoyContollerViewModel : ViewModelBase, IDisposable
  {
    public event Action OnFileLoaded;
    public event Action OnStep;

    private readonly IGameBoy _gameBoy;
    private readonly IOpenFileDialog _fileDialog;

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
          OnPropertyChanged(() => FilePath);
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

    public ICommand LoadCommand
    {
      get { return new DelegateCommand(Load); }
    }

    public GameBoyContollerViewModel(IGameBoy gameBoy, IOpenFileDialogFactory fileDialogFactory)
    {
      _gameBoy = gameBoy;
      _fileDialog = fileDialogFactory.Create();
      _fileDialog.OnFileOpened += OnFileOpened;
    }

    public void Dispose()
    {
      _fileDialog.OnFileOpened -= OnFileOpened;
    }

    private void Load()
    {
      _fileDialog.Open("ROM Files (*.gb)|*.gb|Dump Files (*.dmp)|*.dmp");
    }

    private void Run()
    {
      _gameBoy.Run();
    }

    private void Step()
    {
      _gameBoy.Step();
      NotifyStep();
    }

    private void Pause()
    {
      _gameBoy.Pause();
    }

    private void Stop()
    {
      _gameBoy.Stop();
    }

    private void OnFileOpened(string filePath, int filterIndex)
    {
      if(filterIndex == 1)
        LoadROM(filePath);
      else if (filterIndex == 2)
        LoadMemoryDump(filePath);
      NotifyFileLoaded();
    }

    private void LoadROM(string filePath)
    {
      FilePath = filePath;
      var data = File.ReadAllBytes(filePath);
      _gameBoy.LoadCartridge(data);
      CartridgeTitle = _gameBoy.Cartridge.Title;
    }

    private void LoadMemoryDump(string filePath)
    {
      FilePath = filePath;
      var data = File.ReadAllBytes(filePath);
      _gameBoy.Memory.Load(data);
      _gameBoy.Display.UpdateScreen();
     
    }

    private void NotifyFileLoaded()
    {
      if (OnFileLoaded != null)
        OnFileLoaded();
    }

    private void NotifyStep()
    {
      if (OnStep != null)
        OnStep();
    }
    
  }
}