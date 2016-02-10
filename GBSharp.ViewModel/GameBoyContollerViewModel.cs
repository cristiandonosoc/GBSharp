using System;
using System.IO;
using System.Threading;
using System.Windows.Input;
using GBSharp.Audio;

namespace GBSharp.ViewModel
{
  public class GameBoyContollerViewModel : ViewModelBase, IDisposable
  {
    public event Action OnFileLoaded;
    public event Action OnStep;
    public event Action OnRun;

    public AudioManager audioManager;

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

    public ICommand RunCommand { get { return new DelegateCommand(Run); } }
    public ICommand StepCommand { get { return new DelegateCommand(Step); } }
    public ICommand PauseCommand { get { return new DelegateCommand(Pause); } }
    public ICommand StopCommand { get { return new DelegateCommand(Stop); } }
    public ICommand ResetCommand { get { return new DelegateCommand(Reset); } }
    public ICommand LoadCommand { get { return new DelegateCommand(Load); } }

    public GameBoyContollerViewModel(IGameBoy gameBoy, IOpenFileDialogFactory fileDialogFactory)
    {
      _gameBoy = gameBoy;
      _fileDialog = fileDialogFactory.Create();
      _fileDialog.OnFileOpened += OnFileOpened;

      audioManager = new AudioManager(gameBoy);
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
      audioManager.Play();
      NotifyRun();
    }

    public void OnClosed()
    {
      audioManager.Dispose();
    }

    public void Step()
    {
      _gameBoy.Step(true);
      NotifyStep();
    }

    private void Pause()
    {
      _gameBoy.Pause();
    }

    private void Stop()
    {
      _gameBoy.Reset();
    }

    private void Reset()
    {
      _gameBoy.Reset();
      _gameBoy.Run();
    }

    private void OnFileOpened(string filePath, int filterIndex)
    {
      if(filterIndex == 1)
      {
        LoadROM(filePath);
        NotifyFileLoaded();
      }
      else if (filterIndex == 2)
      {
        LoadMemoryDump(filePath);
      }
    }

    private void LoadROM(string filePath)
    {
      FilePath = filePath;
      var data = File.ReadAllBytes(filePath);
      _gameBoy.LoadCartridge(FilePath, data);
      CartridgeTitle = _gameBoy.Cartridge.Title;
    }

    private void LoadMemoryDump(string filePath)
    {
      FilePath = filePath;
      var data = File.ReadAllBytes(filePath);
      _gameBoy.Memory.Load(data);
      _gameBoy.Display.DrawFrame();
     
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

    private void NotifyRun()
    {
      if (OnRun != null)
      {
        OnRun();
      }
    }
  }
}