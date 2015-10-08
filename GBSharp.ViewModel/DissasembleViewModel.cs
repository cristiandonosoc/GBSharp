using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class DissasembleViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;
    private readonly ICPU _cpu;
    private readonly ObservableCollection<InstructionViewModel> _instructions = new ObservableCollection<InstructionViewModel>();
    private readonly Dictionary<ushort, InstructionViewModel> _addressToInstruction = new Dictionary<ushort, InstructionViewModel>();
    private InstructionViewModel _selectedInstruction;

    public string BreakPoint
    {
      get { return "0x" + _cpu.Breakpoint.ToString("x2"); }
    }

    public ObservableCollection<InstructionViewModel> Instructions
    {
      get { return _instructions; }
    }

    public ICommand DissasembleCommand
    {
      get { return new DelegateCommand(Dissasemble); }
    }

    public ICommand SetBreakPointCommand
    {
      get { return new DelegateCommand(SetBreakpoint); }
    }

    public InstructionViewModel SelectedInstruction
    {
      get { return _selectedInstruction; }
      set
      {
        if (_selectedInstruction != value)
        {
          _selectedInstruction = value;
          OnPropertyChanged(() => SelectedInstruction);
        }
      }
    }

    public DissasembleViewModel(IGameBoy gameBoy)
    {
      _gameBoy = gameBoy;
      _cpu = gameBoy.CPU;
    }

    public void SetCurrentSelectedInstruction()
    {
      SelectedInstruction = _addressToInstruction[_cpu.Registers.PC];
    }

    public void Dissasemble()
    {
      var dissasembledInstructions = _cpu.Dissamble();
      _instructions.Clear();
      foreach (var dissasembledInstruction in dissasembledInstructions)
      {
        InstructionViewModel inst = new InstructionViewModel(dissasembledInstruction);
        _instructions.Add(inst);
        _addressToInstruction[dissasembledInstruction.Address] = inst;
      }
      SelectedInstruction = _instructions.First();
    }

    private void SetBreakpoint()
    {
      if (SelectedInstruction != null)
      {
        _gameBoy.CPU.Breakpoint = ushort.Parse(SelectedInstruction.Address.Remove(0, 2), NumberStyles.HexNumber);
        OnPropertyChanged(() => BreakPoint);
      }
    }
  
  }
}