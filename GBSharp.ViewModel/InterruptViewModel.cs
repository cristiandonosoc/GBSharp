using System.Windows.Input;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;

namespace GBSharp.ViewModel
{
  public class InterruptViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;

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

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    public InterruptViewModel(IGameBoy gameBoy)
    {
      _gameBoy = gameBoy;
    }

    private void CopyFromDomain()
    {
      InterruptMasterEnabled = _gameBoy.CPU.InterruptMasterEnable;

      var interruptEnabledWord = _gameBoy.Memory.Data[(int)MemoryMappedRegisters.IE];
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

    private void CopyToDomain()
    {
      
    }
  }
}