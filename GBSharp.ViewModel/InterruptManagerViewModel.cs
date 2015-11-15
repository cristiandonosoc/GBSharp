using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;

namespace GBSharp.ViewModel
{
  public class InterruptManagerViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDisplay _display;
    private readonly IDispatcher _dispatcher;
    
    
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

    private readonly ObservableCollection<InterruptViewModel> _interrupts = new ObservableCollection<InterruptViewModel>();
    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _memoryMappedRegisters = new ObservableCollection<MemoryMappedRegisterViewModel>();

    public ObservableCollection<InterruptViewModel> InterruptList
    {
      get { return _interrupts; }
    }

    public ObservableCollection<MemoryMappedRegisterViewModel> MemoryMappedRegisterList
    {
      get { return _memoryMappedRegisters; }
    }
   

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }


    public InterruptManagerViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _gameBoy = gameBoy;
      _display = gameBoy.Display;
      _gameBoy.RefreshScreen += OnRefreshScreen;
      //_gameBoy.StepFinished += OnRefreshScreen;

      InterruptList.Add(new InterruptViewModel("Vertical Blank", Interrupts.VerticalBlanking, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Timer Overflow", Interrupts.TimerOverflow, _gameBoy));
      InterruptList.Add(new InterruptViewModel("LCD Status", Interrupts.LCDCStatus, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Button Pressed", Interrupts.P10to13TerminalNegativeEdge, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Serial Transfer Completed", Interrupts.SerialIOTransferCompleted, _gameBoy));

      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF40: LCDC", MemoryMappedRegisters.LCDC, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF41: STAT", MemoryMappedRegisters.STAT, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF42: SCY", MemoryMappedRegisters.SCY, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF43: SCX", MemoryMappedRegisters.SCX, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF44: LY", MemoryMappedRegisters.LY, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF45: LYC", MemoryMappedRegisters.LYC, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF47: BGP", MemoryMappedRegisters.BGP, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF48: OBP0", MemoryMappedRegisters.OBP0, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF49: OBP1", MemoryMappedRegisters.OBP1, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF4A: WY", MemoryMappedRegisters.WY, _gameBoy));
      MemoryMappedRegisterList.Add(new MemoryMappedRegisterViewModel("FF4B: WX", MemoryMappedRegisters.WX, _gameBoy));
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

      foreach (var memoryMappedRegister in _memoryMappedRegisters)
      {
        memoryMappedRegister.CopyFromDomain();
      }

     
      #endregion

    }

    private void CopyToDomain()
    {
      
    }

    public void Dispose()
    {
      _gameBoy.RefreshScreen -= OnRefreshScreen;
      //_gameBoy.StepFinished -= OnRefreshScreen;
    }
  }
}