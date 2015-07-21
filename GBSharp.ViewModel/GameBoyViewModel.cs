using System;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;
    private readonly CartridgeViewModel _cartridge;
    private readonly MemoryViewModel _memory;

    private string _cpuState = "";

    public CartridgeViewModel Cartridge
    {
      get { return _cartridge; }
    }

    public MemoryViewModel Memory
    {
      get { return _memory; }
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

    public ICommand PrintCPUCommand
    {
      get { return new DelegateCommand(PrintCPU); }
    }

    public ICommand PrintMemoryCommand
    {
      get { return new DelegateCommand(PrintMemory); }
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
      _memory = new MemoryViewModel(_gameBoy.Memory, "RAM");
      _cartridge = new CartridgeViewModel(_gameBoy.Cartridge);
      _cartridge.CartridgeFileLoaded += OnCartridgeFileLoaded;
    }

    private void OnCartridgeFileLoaded(byte[] data)
    {
      _gameBoy.LoadCartridge(data);
      _cartridge.Update();
      //_gameBoy.Run();
    }

    private void Run()
    {
      _gameBoy.Run();
    }

    private void Step()
    {
      _gameBoy.Step();
      PrintCPU();
    }

    private void Pause()
    {
      _gameBoy.Pause();
    }

    private void Stop()
    {
      _gameBoy.Stop();
    }

    private void PrintCPU()
    {
      var instructionName = _gameBoy.CPU.GetCurrentInstructionName();
      var registersStateString = _gameBoy.CPU.ToString();
      CPUState = "Instruction: " + instructionName + "\n" + "Registers:" + "\n" + registersStateString;
      //_memory.Update();
      //_cartridge.Memory.SelectedAddress = (int)_gameBoy.CPU.Registers.PC;
    }

    private void PrintMemory()
    {
      _memory.Update();
    }


    private void ButtonA()
    {
      _gameBoy.PressButton(Keypad.A);
      PrintCPU();
    }

    private void ButtonB()
    {
      _gameBoy.PressButton(Keypad.B);
      PrintCPU();
    }

    private void ButtonLeft()
    {
      _gameBoy.PressButton(Keypad.Left);
      PrintCPU();
    }

    private void ButtonUp()
    {
      _gameBoy.PressButton(Keypad.Up);
      PrintCPU();
    }

    private void ButtonRight()
    {
      _gameBoy.PressButton(Keypad.Right);
      PrintCPU();
    }

    private void ButtonDown()
    {
      _gameBoy.PressButton(Keypad.Down);
      PrintCPU();
    }

    private void ButtonStart()
    {
      _gameBoy.PressButton(Keypad.Start);
      PrintCPU();
    }


    private void ButtonSelect()
    {
      _gameBoy.PressButton(Keypad.Select);
      PrintCPU();
    }
  }
}