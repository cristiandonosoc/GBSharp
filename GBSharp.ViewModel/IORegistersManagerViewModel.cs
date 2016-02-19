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

    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _miscRegisters 
      = new ObservableCollection<MemoryMappedRegisterViewModel>();
    public ObservableCollection<MemoryMappedRegisterViewModel> MiscRegisters
    {
      get { return _miscRegisters; }
    }
    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _displayRegisters 
      = new ObservableCollection<MemoryMappedRegisterViewModel>();
    public ObservableCollection<MemoryMappedRegisterViewModel> DisplayRegisters
    {
      get { return _displayRegisters; }
    }
    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _soundRegistersSet1
      = new ObservableCollection<MemoryMappedRegisterViewModel>();
    public ObservableCollection<MemoryMappedRegisterViewModel> SoundRegistersSet1
    {
      get { return _soundRegistersSet1; }
    }
    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _soundRegistersSet2
      = new ObservableCollection<MemoryMappedRegisterViewModel>();
    public ObservableCollection<MemoryMappedRegisterViewModel> SoundRegistersSet2
    {
      get { return _soundRegistersSet2; }
    }
    private readonly ObservableCollection<MemoryMappedRegisterViewModel> _soundRegistersSet3
      = new ObservableCollection<MemoryMappedRegisterViewModel>();
    public ObservableCollection<MemoryMappedRegisterViewModel> SoundRegistersSet3
    {
      get { return _soundRegistersSet3; }
    }

    public MemoryMappedRegisterViewModel SS
    {
      get { return _soundRegistersSet3[0]; }

    }

    public ICommand ReadCommand { get { return new DelegateCommand(CopyFromDomain); }}
    public ICommand WriteCommand { get { return new DelegateCommand(CopyToDomain); } }

    public IORegistersManagerViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _gameBoy = gameBoy;
      _gameBoy.StepCompleted += _gameBoy_StepCompleted;
      _gameBoy.PauseRequested += _gameBoy_PauseRequested;

      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF00: P1", MMR.P1, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF01: SB", MMR.SB, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF02: SC", MMR.SC, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF04: DIV", MMR.DIV, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF05: TIMA", MMR.TIMA, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF06: TMA", MMR.TMA, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF07: TAC", MMR.TAC, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FF0F: IF", MMR.IF, _gameBoy));
      MiscRegisters.Add(new MemoryMappedRegisterViewModel("FFFF: IE", MMR.IE, _gameBoy));

      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF40: LCDC", MMR.LCDC, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF41: STAT", MMR.STAT, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF42: SCY", MMR.SCY, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF43: SCX", MMR.SCX, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF44: LY", MMR.LY, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF45: LYC", MMR.LYC, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF47: BGP", MMR.BGP, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF48: OBP0", MMR.OBP0, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF49: OBP1", MMR.OBP1, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF4A: WY", MMR.WY, _gameBoy));
      DisplayRegisters.Add(new MemoryMappedRegisterViewModel("FF4B: WX", MMR.WX, _gameBoy));

      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF10: NR10", MMR.NR10, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF11: NR11", MMR.NR11, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF12: NR12", MMR.NR12, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF13: NR13", MMR.NR13, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF14: NR14", MMR.NR14, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF16: NR21", MMR.NR21, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF17: NR22", MMR.NR22, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF18: NR23", MMR.NR23, _gameBoy));
      SoundRegistersSet1.Add(new MemoryMappedRegisterViewModel("FF19: NR24", MMR.NR24, _gameBoy));

      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF1A: NR30", MMR.NR30, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF1B: NR31", MMR.NR31, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF1C: NR32", MMR.NR32, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF1D: NR33", MMR.NR33, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF1E: NR34", MMR.NR34, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF20: NR41", MMR.NR41, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF21: NR42", MMR.NR42, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF22: NR43", MMR.NR43, _gameBoy));
      SoundRegistersSet2.Add(new MemoryMappedRegisterViewModel("FF23: NR44", MMR.NR44, _gameBoy));

      SoundRegistersSet3.Add(new MemoryMappedRegisterViewModel("FF24: NR50", MMR.NR50, _gameBoy));
      SoundRegistersSet3.Add(new MemoryMappedRegisterViewModel("FF25: NR51", MMR.NR51, _gameBoy));
      SoundRegistersSet3.Add(new MemoryMappedRegisterViewModel("FF26: NR52", MMR.NR52, _gameBoy));
    }

    private void _gameBoy_PauseRequested()
    {
      _dispatcher.BeginInvoke(new Action(CopyFromDomain));
    }

    private void _gameBoy_StepCompleted()
    {
      _dispatcher.BeginInvoke(new Action(CopyFromDomain));
    }

    public void CopyFromDomain()
    {
      foreach (var memoryMappedRegister in _miscRegisters)
      {
        memoryMappedRegister.CopyFromDomain();
      }
      foreach (var memoryMappedRegister in _displayRegisters)
      {
        memoryMappedRegister.CopyFromDomain();
      }
      foreach (var memoryMappedRegister in _soundRegistersSet1)
      {
        memoryMappedRegister.CopyFromDomain();
      }
      foreach (var memoryMappedRegister in _soundRegistersSet2)
      {
        memoryMappedRegister.CopyFromDomain();
      }
      foreach (var memoryMappedRegister in _soundRegistersSet3)
      {
        memoryMappedRegister.CopyFromDomain();
      }
    }

    private void CopyToDomain()
    {

    }

    public void Dispose()
    {
      _gameBoy.StepCompleted -= _gameBoy_StepCompleted;
    }
  }
}