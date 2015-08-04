using System;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;
    private readonly CartridgeViewModel _cartridge;
    private readonly MemoryViewModel _memory;

    private string _registerA;
    private string _registerB;
    private string _registerC;
    private string _registerD;
    private string _registerE;
    private string _registerH;
    private string _registerL;

    private BitmapImage _screen;

    
    public CartridgeViewModel Cartridge
    {
      get { return _cartridge; }
    }

    public MemoryViewModel Memory
    {
      get { return _memory; }
    }

    public BitmapImage Screen
    {
      get { return _screen; }
      set
      {
        _screen = value;
        OnPropertyChanged(() => Screen);
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

    public string RegisterA
    {
      get { return _registerA; }
      set
      {
        if (_registerA != value)
        {
          _registerA = value;
          OnPropertyChanged(() => RegisterA);
        }
      }
    }

    public string RegisterB
    {
      get { return _registerB; }
      set
      {
        if (_registerB != value)
        {
          _registerB = value;
          OnPropertyChanged(() => RegisterB);
        }
      }
    }

    public string RegisterC
    {
      get { return _registerC; }
      set
      {
        if (_registerC != value)
        {
          _registerC = value;
          OnPropertyChanged(() => RegisterC);
        }
      }
    }

    public string RegisterD
    {
      get { return _registerD; }
      set
      {
        if (_registerD != value)
        {
          _registerD = value;
          OnPropertyChanged(() => RegisterD);
        }
      }
    }

    public string RegisterE
    {
      get { return _registerE; }
      set
      {
        if (_registerE != value)
        {
          _registerE = value;
          OnPropertyChanged(() => RegisterE);
        }
      }
    }

    public string RegisterH
    {
      get { return _registerH; }
      set
      {
        if (_registerH != value)
        {
          _registerH = value;
          OnPropertyChanged(() => RegisterH);
        }
      }
    }

    public string RegisterL
    {
      get { return _registerL; }
      set
      {
        if (_registerL != value)
        {
          _registerL = value;
          OnPropertyChanged(() => RegisterL);
        }
      }
    }

    public GameBoyViewModel(IGameBoy gameBoy)
    {
      _gameBoy = gameBoy;
      _memory = new MemoryViewModel(_gameBoy.Memory, "RAM");//, 0xC000, 0xE000);
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
      RegisterA = "0x" + _gameBoy.CPU.Registers.A.ToString("x2");
      RegisterB = "0x" + _gameBoy.CPU.Registers.B.ToString("x2");
      RegisterC = "0x" + _gameBoy.CPU.Registers.C.ToString("x2");
      RegisterD = "0x" + _gameBoy.CPU.Registers.D.ToString("x2");
      RegisterE = "0x" + _gameBoy.CPU.Registers.E.ToString("x2");
      RegisterH = "0x" + _gameBoy.CPU.Registers.H.ToString("x2");
      RegisterL = "0x" + _gameBoy.CPU.Registers.L.ToString("x2");

      Screen = BitmapToImageSource(_gameBoy.Display.Screen);
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

    private BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
      using (MemoryStream memory = new MemoryStream())
      {
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;
        BitmapImage bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
      }
    }
  }
}