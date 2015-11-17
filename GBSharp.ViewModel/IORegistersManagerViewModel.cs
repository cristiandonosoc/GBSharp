using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GBSharp.MemorySpace;

namespace GBSharp.ViewModel
{
  public class IORegistersManagerViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDispatcher _dispatcher;

    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _memoryMappedRegisters = new ObservableCollection<MemoryMappedRegisterViewModel>();

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

    public IORegistersManagerViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _gameBoy = gameBoy;
      _gameBoy.FrameCompleted += OnFrameCompleted;
  
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

    private void OnFrameCompleted()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      foreach (var memoryMappedRegister in _memoryMappedRegisters)
      {
        memoryMappedRegister.CopyFromDomain();
      }
      
    }

    private void CopyToDomain()
    {

    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}