using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GBSharp.ViewModel
{
  public class BreakpointsViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameboy;

    private readonly ObservableCollection<BreakpointViewModel> _breakpoints 
      = new ObservableCollection<BreakpointViewModel>();
    public ObservableCollection<BreakpointViewModel> Breakpoints
    {
      get { return _breakpoints; }
    }

    public BreakpointsViewModel(IGameBoy gameboy)
    {
      _gameboy = gameboy;
    }

    public void RecreateBreakpoints()
    {
      List<ushort> addresses = _gameboy.CPU.Breakpoints;
      addresses.Sort();
      _breakpoints.Clear();

      foreach(ushort address in addresses)
      {
        IInstruction inst = _gameboy.Disassembler.FetchAndDecode(address);
        BreakpointViewModel vm = new BreakpointViewModel(_gameboy, inst);
        vm.OnExecute = true;
        _breakpoints.Add(vm);
      }
    }
  }
}