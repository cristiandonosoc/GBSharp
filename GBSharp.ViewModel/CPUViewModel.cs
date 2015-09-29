using System;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class CPUViewModel : ViewModelBase, IDisposable
  {
    private ICPU _cpu;
    private IGameBoy _gameBoy;
    private readonly IDispatcher _dispatcher;

    private string _registerPC;
    private string _registerPCOpcode;
    private string _registerPCDescription;
    private string _registerPCOperand1;
    private string _registerPCOperand2;
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

    public string RegisterPCOpcode
    {
      get { return _registerPCOpcode; }
      set
      {
        if(_registerPCOpcode != value )
        {
          _registerPCOpcode = value;
          OnPropertyChanged(() => RegisterPCOpcode);
        }
      }
    }

    public string RegisterPCDescription
    {
      get { return _registerPCDescription; }
      set
      {
        if(_registerPCDescription != value )
        {
          _registerPCDescription= value;
          OnPropertyChanged(() => RegisterPCDescription);
        }
      }
    }

    public string RegisterPCOperand1
    {
      get { return _registerPCOperand1; }
      set
      {
        if(_registerPCOperand1 != value)
        {
          _registerPCOperand1 = value;
          OnPropertyChanged(() => RegisterPCOperand1);
        }
      }
    }

    public string RegisterPCOperand2
    {
      get { return _registerPCOperand2; }
      set
      {
        if(_registerPCOperand2 != value)
        {
          _registerPCOperand2 = value;
          OnPropertyChanged(() => RegisterPCOperand2);
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

    public CPUViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _cpu = _gameBoy.CPU;
      _dispatcher = dispatcher;

      _gameBoy.Display.RefreshScreen += OnRefreshScreen;
      //_gameBoy.StepFinished += OnRefreshScreen;
    }

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      // TODO(Cristian): Apparently, the PC being displayed is the one
      //                 that was already updated by the Step cycle,
      //                 this showing the *NEXT* instruction instead
      //                 of the one currently displayed.
      //                 Verify if this is the case, and fix it.
      RegisterPC = "0x" + _cpu.Registers.PC.ToString("x2");
      RegisterPCOpcode = _cpu.CurrentInstruction.Name;
      RegisterPCDescription = _cpu.CurrentInstruction.Description;

      byte?[] currentOperands = _cpu.CurrentInstruction.Operands;
      string op1String = "";
      if(currentOperands[0] != null)
      {
        op1String = "0x" + ((byte)currentOperands[0]).ToString("x2");
      }
      RegisterPCOperand1 = op1String;

      string op2String = "";
      if (currentOperands[1] != null)
      {
        op2String = "0x" + ((byte)currentOperands[1]).ToString("x2");
      }
      RegisterPCOperand2 = op2String;

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
      _gameBoy.Display.RefreshScreen -= OnRefreshScreen;
      //_gameBoy.StepFinished -= OnRefreshScreen;
    }
  }
}
