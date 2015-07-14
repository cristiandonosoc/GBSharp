using System;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;
    private readonly CartridgeViewModel _cartridge;

    private string _cpuState = "";

    public CartridgeViewModel Cartridge
    {
      get { return _cartridge; }
    }

    public ICommand RunCommand
    {
      get { return new DelegateCommand(Run); }
    }

    public ICommand PauseCommand
    {
      get { return new DelegateCommand(Pause); }
    }

    public ICommand StopCommand
    {
      get { return new DelegateCommand(Stop); }
    }

    public ICommand PrintCommand
    {
      get { return new DelegateCommand(Print); }
    }

    public ICommand ButtonACommand
    {
      get { return new DelegateCommand(ButtonA); }
    }


    public ICommand ButtonBCommand
    {
      get { return new DelegateCommand(ButtonB); }
    }



    public ICommand ButtonLeftCommand
    {
      get { return new DelegateCommand(ButtonLeft); }
    }


    public ICommand ButtonUpCommand
    {
      get { return new DelegateCommand(ButtonUp); }
    }



    public ICommand ButtonRightCommand
    {
      get { return new DelegateCommand(ButtonRight); }
    }



    public ICommand ButtonDownCommand
    {
      get { return new DelegateCommand(ButtonDown); }
    }


    public ICommand ButtonStartCommand
    {
      get { return new DelegateCommand(ButtonStart); }
    }


    public ICommand ButtonSelectCommand
    {
      get { return new DelegateCommand(ButtonSelect); }
    }

    public string CPUState
    {
      get { return _cpuState; }
      set
      {
        if (_cpuState != value)
        {
          _cpuState = value;
          OnPropertyChanged(() => CPUState);
        }
      }
    }

    public GameBoyViewModel(IGameBoy gameBoy)
    {
      _gameBoy = gameBoy;
      _cartridge = new CartridgeViewModel(_gameBoy.Cartridge);
      _cartridge.CartridgeFileLoaded += OnCartridgeFileLoaded;
    }

    private void OnCartridgeFileLoaded(byte[] data)
    {
      _gameBoy.LoadCartridge(data);
      _cartridge.Update();
      _gameBoy.Run();
    }

    private void Run()
    {
      _gameBoy.Run();
    }

    private void Pause()
    {
      _gameBoy.Pause();
    }

    private void Stop()
    {
      _gameBoy.Stop();
    }

    private void Print()
    {
      CPUState = _gameBoy.CPU.ToString();
    }


    private void ButtonA()
    {
      Console.WriteLine("A");
    }

    private void ButtonB()
    {
      Console.WriteLine("B");
    }

    private void ButtonLeft()
    {
      Console.WriteLine("Left");
    }

    private void ButtonUp()
    {
      Console.WriteLine("Up");
    }

    private void ButtonRight()
    {
      Console.WriteLine("Right");
    }

    private void ButtonDown()
    {
      Console.WriteLine("Down");
    }

    private void ButtonStart()
    {
      Console.WriteLine("Start");
    }


    private void ButtonSelect()
    {
      Console.WriteLine("Select");
    }
  }
}