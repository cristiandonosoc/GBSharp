using System;
using System.Windows.Input;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System.Collections.Generic;
using System.Linq;

namespace GBSharp.ViewModel
{
  public class InterruptViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDisplay _display;
    private readonly IDispatcher _dispatcher;

    #region INTERRUPTS
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

    #endregion

    #region REGISTERS

    private string PrintRegister(MemoryMappedRegisters reg)
    {
      string regString = "0x" + _regsDic[reg].ToString("x2");
      return regString;
    }

    private Dictionary<MemoryMappedRegisters, ushort> _regsDic;
    public string regLCDC { get { return PrintRegister(MemoryMappedRegisters.LCDC); } }
    public string regSTAT { get { return PrintRegister(MemoryMappedRegisters.STAT); } }
    public string regSCY { get { return PrintRegister(MemoryMappedRegisters.SCY); } }
    public string regSCX { get { return PrintRegister(MemoryMappedRegisters.SCX); } }
    public string regLY { get { return PrintRegister(MemoryMappedRegisters.LY); } }
    public string regLYC { get { return PrintRegister(MemoryMappedRegisters.LYC); } }
    public string regBGP { get { return PrintRegister(MemoryMappedRegisters.BGP); } }
    public string regOBP0 { get { return PrintRegister(MemoryMappedRegisters.OBP0); } }
    public string regOBP1 { get { return PrintRegister(MemoryMappedRegisters.OBP1); } }
    public string regWY { get { return PrintRegister(MemoryMappedRegisters.WY); } }
    public string regWX { get { return PrintRegister(MemoryMappedRegisters.WX); } }

    #endregion

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    public InterruptViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _gameBoy = gameBoy;
      _display = gameBoy.Display;
      //_display.RefreshScreen += OnRefreshScreen;
      //_gameBoy.StepFinished += OnRefreshScreen;
      _regsDic = _gameBoy.GetRegisterDic();
    }

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      #region INTERRUPTS
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
      #endregion

      _regsDic = _gameBoy.GetRegisterDic();

      // TODO(Cristian, aaecheve): See if we can simply notify the view 
      //                           that all the values changed
      OnPropertyChanged(() => regLCDC);
      OnPropertyChanged(() => regSTAT);
      OnPropertyChanged(() => regSCY);
      OnPropertyChanged(() => regSCX);
      OnPropertyChanged(() => regLY);
      OnPropertyChanged(() => regLYC);
      OnPropertyChanged(() => regBGP);
      OnPropertyChanged(() => regOBP0);
      OnPropertyChanged(() => regOBP1);
      OnPropertyChanged(() => regWY);
      OnPropertyChanged(() => regWX);


      // TODO(Cristian, aaecheve): I tried using reflection but failed
      //                           C# masters halp
      // Doesn't compile (some recursive compilation)
      //Type type = typeof(InterruptViewModel);
      //var properties = from property in type.GetProperties()
      //                 where property.Name.StartsWith("reg")
      //                 select property;
      //foreach (var property in properties)
      //{
      //  OnPropertyChanged(() => property);
      //}
    }

    private void CopyToDomain()
    {
      
    }

    public void Dispose()
    {
      //_display.RefreshScreen -= OnRefreshScreen;
      //_gameBoy.StepFinished -= OnRefreshScreen;
    }
  }
}