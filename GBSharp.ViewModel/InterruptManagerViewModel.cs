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

    public ObservableCollection<InterruptViewModel> InterruptList
    {
      get { return _interrupts; }
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
      _gameBoy.FrameCompleted += OnFrameCompleted;

      InterruptList.Add(new InterruptViewModel("Vertical Blank", Interrupts.VerticalBlanking, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Timer Overflow", Interrupts.TimerOverflow, _gameBoy));
      InterruptList.Add(new InterruptViewModel("LCD Status", Interrupts.LCDCStatus, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Button Pressed", Interrupts.P10to13TerminalNegativeEdge, _gameBoy));
      InterruptList.Add(new InterruptViewModel("Serial Transfer Completed", Interrupts.SerialIOTransferCompleted, _gameBoy));
    }

    private void OnFrameCompleted()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      InterruptMasterEnabled = _gameBoy.CPU.InterruptMasterEnable;
      foreach (var interrupt in _interrupts)
      {
        interrupt.CopyFromDomain();
      }

    }

    private void CopyToDomain()
    {
      
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
      //_gameBoy.StepCompleted -= OnFrameCompleted;
    }
  }
}