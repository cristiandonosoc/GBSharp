using System;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class CPUViewModel : ViewModelBase, IDisposable
  {
    private ICPU _cpu;
    private readonly IDisplay _display;
    private readonly IDispatcher _dispatcher;

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

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    public CPUViewModel(ICPU cpu, IDisplay display, IDispatcher dispatcher)
    {
      _cpu = cpu;
      _display = display;
      _dispatcher = dispatcher;
      _display.RefreshScreen += OnRefreshScreen;
    }

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    private void CopyFromDomain()
    {
      RegisterPC = "0x" + _cpu.Registers.PC.ToString("x2");
      RegisterSP = "0x" + _cpu.Registers.SP.ToString("x2");

      RegisterA = "0x" + _cpu.Registers.A.ToString("x2");
      RegisterB = "0x" + _cpu.Registers.B.ToString("x2");
      RegisterC = "0x" + _cpu.Registers.C.ToString("x2");
      RegisterD = "0x" + _cpu.Registers.D.ToString("x2");
      RegisterE = "0x" + _cpu.Registers.E.ToString("x2");
      RegisterH = "0x" + _cpu.Registers.H.ToString("x2");
      RegisterL = "0x" + _cpu.Registers.L.ToString("x2");

      FlagZero = _cpu.Registers.FZ == 1;
      FlagCarry = _cpu.Registers.FC == 1;
      FlagHalfCarry = _cpu.Registers.FH == 1;
      FlagNegative = _cpu.Registers.FN == 1;
    }

    private void CopyToDomain()
    {
      
    }

    public void Dispose()
    {
      _display.RefreshScreen -= OnRefreshScreen;
    }
  }
}
