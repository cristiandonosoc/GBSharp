using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;

namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private IDispatcher _dispatcher;
    private readonly IGameBoy _gameBoy;
    private readonly CartridgeViewModel _cartridge;
    private readonly MemoryViewModel _memory;

    private string _registerPC;
    private string _registerSP;
    private string _registerA;
    private string _registerB;
    private string _registerC;
    private string _registerD;
    private string _registerE;
    private string _registerH;
    private string _registerL;

    private bool _flagZero;
    private bool _flagCarry;
    private bool _flagHalfCarry;
    private bool _flagNegative;

    private bool _interruptMasterEnabled;
    private bool _verticalBlankInterruptEnabled;
    private bool _verticalBlankInterruptRequested;
    private bool _lcdStatusInterruptEnabled;
    private bool _lcdStatusInterruptRequested;
    private bool _timerOverflowInterruptEnabled;
    private bool _timerOverflowInterruptRequested;
    private bool _serialTransferCompletedInterruptEnabled;
    private bool _serialTransferCompletedInterruptRequested;
    private bool _keyPadPressedInterruptEnabled;
    private bool _keyPadPressedInterruptRequested;

    private string _filePath;
    private string _cartridgeTitle;

    private BitmapImage _screen;
    private BitmapImage _background;

 
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

    public BitmapImage Background
    {
      get { return _background; }
      set
      {
        _background = value;
        OnPropertyChanged(() => Background);
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

    public ICommand UpdateDisplayCommand
    {
      get { return new DelegateCommand(UpdateDisplay); }
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

    public string RegisterPC
    {
      get { return _registerPC; }
      set
      {
        if (_registerPC != value)
        {
          _registerPC = value;
          OnPropertyChanged(() => RegisterPC);
        }
      }
    }

    public string RegisterSP
    {
      get { return _registerSP; }
      set
      {
        if (_registerSP != value)
        {
          _registerSP = value;
          OnPropertyChanged(() => RegisterSP);
        }
      }
    }

    public bool FlagZero
    {
      get { return _flagZero; }
      set
      {
        if (_flagZero != value)
        {
          _flagZero = value;
          OnPropertyChanged(() => FlagZero);
        }
      }
    }

    public bool FlagCarry
    {
      get { return _flagCarry; }
      set
      {
        if (_flagCarry != value)
        {
          _flagCarry = value;
          OnPropertyChanged(() => FlagCarry);
        }
      }
    }

    public bool FlagHalfCarry
    {
      get { return _flagHalfCarry; }
      set
      {
        if (_flagHalfCarry != value)
        {
          _flagHalfCarry = value;
          OnPropertyChanged(() => FlagHalfCarry);
        }
      }
    }

    public bool FlagNegative
    {
      get { return _flagNegative; }
      set
      {
        if (_flagNegative != value)
        {
          _flagNegative = value;
          OnPropertyChanged(() => FlagNegative);
        }
      }
    }

    public bool InterruptMasterEnabled
    {
      get { return _interruptMasterEnabled; }
      set
      {
        if (_interruptMasterEnabled != value)
        {
          _interruptMasterEnabled = value;
          OnPropertyChanged(() => InterruptMasterEnabled);
        }
      }
    }

    public bool VerticalBlankInterruptEnabled
    {
      get { return _verticalBlankInterruptEnabled; }
      set
      {
        if (_verticalBlankInterruptEnabled != value)
        {
          _verticalBlankInterruptEnabled = value;
          OnPropertyChanged(() => VerticalBlankInterruptEnabled);
        }
      }
    }

    public bool VerticalBlankInterruptRequested
    {
      get { return _verticalBlankInterruptRequested; }
      set
      {
        if (_verticalBlankInterruptRequested != value)
        {
          _verticalBlankInterruptRequested = value;
          OnPropertyChanged(() => VerticalBlankInterruptRequested);
        }
      }
    }

    public bool LcdStatusInterruptEnabled
    {
      get { return _lcdStatusInterruptEnabled; }
      set
      {
        if (_lcdStatusInterruptEnabled != value)
        {
          _lcdStatusInterruptEnabled = value;
          OnPropertyChanged(() => LcdStatusInterruptEnabled);
        }
      }
    }

    public bool LcdStatusInterruptRequested
    {
      get { return _lcdStatusInterruptRequested; }
      set
      {
        if (_lcdStatusInterruptRequested != value)
        {
          _lcdStatusInterruptRequested = value;
          OnPropertyChanged(() => LcdStatusInterruptRequested);
        }
      }
    }

    public bool TimerOverflowInterruptEnabled
    {
      get { return _timerOverflowInterruptEnabled; }
      set
      {
        if (_timerOverflowInterruptEnabled != value)
        {
          _timerOverflowInterruptEnabled = value;
          OnPropertyChanged(() => TimerOverflowInterruptEnabled);
        }
      }
    }

    public bool TimerOverflowInterruptRequested
    {
      get { return _timerOverflowInterruptRequested; }
      set
      {
        if (_timerOverflowInterruptRequested != value)
        {
          _timerOverflowInterruptRequested = value;
          OnPropertyChanged(() => TimerOverflowInterruptRequested);
        }
      }
    }

    public bool SerialTransferCompletedInterruptEnabled
    {
      get { return _serialTransferCompletedInterruptEnabled; }
      set
      {
        if (_serialTransferCompletedInterruptEnabled != value)
        {
          _serialTransferCompletedInterruptEnabled = value;
          OnPropertyChanged(() => SerialTransferCompletedInterruptEnabled);
        }
      }
    }

    public bool SerialTransferCompletedInterruptRequested
    {
      get { return _serialTransferCompletedInterruptRequested; }
      set
      {
        if (_serialTransferCompletedInterruptRequested != value)
        {
          _serialTransferCompletedInterruptRequested = value;
          OnPropertyChanged(() => SerialTransferCompletedInterruptRequested);
        }
      }
    }

    public bool KeyPadPressedInterruptEnabled
    {
      get { return _keyPadPressedInterruptEnabled; }
      set
      {
        if (_keyPadPressedInterruptEnabled != value)
        {
          _keyPadPressedInterruptEnabled = value;
          OnPropertyChanged(() => KeyPadPressedInterruptEnabled);
        }
      }
    }

    public bool KeyPadPressedInterruptRequested
    {
      get { return _keyPadPressedInterruptRequested; }
      set
      {
        if (_keyPadPressedInterruptRequested != value)
        {
          _keyPadPressedInterruptRequested = value;
          OnPropertyChanged(() => KeyPadPressedInterruptRequested);
        }
      }
    }

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


    public GameBoyViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _memory = new MemoryViewModel(_gameBoy.Cartridge, "Memory View");//, 0xC000, 0xE000);
      _gameBoy.CPU.StepFinished += OnStepFinished;
    }

    private void OnStepFinished()
    {
      //_dispatcher.Invoke(PrintCPU);
    }

    private void OnCartridgeFileLoaded(byte[] data)
    {
      _gameBoy.LoadCartridge(data);
      CartridgeTitle = _gameBoy.Cartridge.Title;
      _memory.Update();
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
      RegisterPC = "0x" + _gameBoy.CPU.Registers.PC.ToString("x2");
      RegisterSP = "0x" + _gameBoy.CPU.Registers.SP.ToString("x2");

      RegisterA = "0x" + _gameBoy.CPU.Registers.A.ToString("x2");
      RegisterB = "0x" + _gameBoy.CPU.Registers.B.ToString("x2");
      RegisterC = "0x" + _gameBoy.CPU.Registers.C.ToString("x2");
      RegisterD = "0x" + _gameBoy.CPU.Registers.D.ToString("x2");
      RegisterE = "0x" + _gameBoy.CPU.Registers.E.ToString("x2");
      RegisterH = "0x" + _gameBoy.CPU.Registers.H.ToString("x2");
      RegisterL = "0x" + _gameBoy.CPU.Registers.L.ToString("x2");

      FlagZero = _gameBoy.CPU.Registers.FZ == 1;
      FlagCarry = _gameBoy.CPU.Registers.FC == 1;
      FlagHalfCarry = _gameBoy.CPU.Registers.FH == 1;
      FlagNegative = _gameBoy.CPU.Registers.FN == 1;

      InterruptMasterEnabled = _gameBoy.CPU.InterruptMasterEnable;

      var interruptEnabledWord = _gameBoy.Memory.Data[(int) MemoryMappedRegisters.IE];
      var interruptRequestedWord = _gameBoy.Memory.Data[(int)MemoryMappedRegisters.IF];

      VerticalBlankInterruptEnabled = (interruptEnabledWord & (byte)Interrupts.VerticalBlanking) > 0;
      VerticalBlankInterruptRequested = (interruptRequestedWord & (byte)Interrupts.VerticalBlanking) > 0;
      LcdStatusInterruptEnabled = (interruptEnabledWord & (byte)Interrupts.LCDCStatus) > 0;
      LcdStatusInterruptRequested = (interruptRequestedWord & (byte)Interrupts.LCDCStatus) > 0;
      TimerOverflowInterruptEnabled = (interruptEnabledWord & (byte)Interrupts.TimerOverflow) > 0;
      TimerOverflowInterruptRequested = (interruptRequestedWord & (byte)Interrupts.TimerOverflow) > 0;
      SerialTransferCompletedInterruptEnabled = (interruptEnabledWord & (byte)Interrupts.SerialIOTransferCompleted) > 0;
      SerialTransferCompletedInterruptRequested = (interruptRequestedWord & (byte)Interrupts.SerialIOTransferCompleted) > 0;
      KeyPadPressedInterruptEnabled = (interruptEnabledWord & (byte)Interrupts.P10to13TerminalNegativeEdge) > 0;
      KeyPadPressedInterruptRequested = (interruptRequestedWord & (byte)Interrupts.P10to13TerminalNegativeEdge) > 0;

     
    }

    private void UpdateDisplay()
    {
      Screen = BitmapToImageSource(_gameBoy.Display.Screen);
      Background = BitmapToImageSource(_gameBoy.Display.Screen);
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

   
    private void NotifyCartridgeFileLoaded()
    {
      if (File.Exists(_filePath))
        OnCartridgeFileLoaded(File.ReadAllBytes(_filePath));
    }
  }
}