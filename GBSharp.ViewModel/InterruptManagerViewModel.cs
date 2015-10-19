using System;
using System.Windows.Input;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GBSharp.ViewModel
{
  public class InterruptManagerViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDisplay _display;
    private readonly IDispatcher _dispatcher;

    #region INTERRUPTS
    
    private bool _interruptMasterEnabled;

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

    private ObservableCollection<InterruptViewModel> _interrupts = new ObservableCollection<InterruptViewModel>(); 

   
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

    public ObservableCollection<InterruptViewModel> InterruptList
    {
      get { return _interrupts; }
    }


    public InterruptManagerViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _gameBoy = gameBoy;
      _display = gameBoy.Display;
      //_display.RefreshScreen += OnRefreshScreen;
      //_gameBoy.StepFinished += OnRefreshScreen;
      _regsDic = _gameBoy.GetRegisterDic();
      InterruptList.Add(new InterruptViewModel("Vertical Blank", Interrupts.VerticalBlanking, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Timer Overflow", Interrupts.TimerOverflow, _gameBoy));
      InterruptList.Add(new InterruptViewModel("LCD Status", Interrupts.LCDCStatus, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Button Pressed", Interrupts.P10to13TerminalNegativeEdge, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Serial Transfer Completed", Interrupts.SerialIOTransferCompleted, _gameBoy));
    }

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      #region INTERRUPTS
      InterruptMasterEnabled = _gameBoy.CPU.InterruptMasterEnable;
      foreach (var interrupt in _interrupts)
      {
        interrupt.CopyFromDomain();
      }

     
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