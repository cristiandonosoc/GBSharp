using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using GBSharp.CPUSpace.Dictionaries;

namespace GBSharp.ViewModel
{
  public class DissasembleViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;
    private readonly ICPU _cpu;
    private readonly IDisassembler _disassembler;
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
      get { return new DelegateCommand(DissasembleCommandWrapper); }
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
      _disassembler = gameBoy.Disassembler;
    }

    public void SetCurrentSelectedInstruction()
    {
      try
      {
        SelectedInstruction = _addressToInstruction[_cpu.Registers.PC];
      }
      catch(KeyNotFoundException e)
      {
        Dissasemble(_cpu.Registers.PC);
      }
    }

    public void DissasembleCommandWrapper()
    {
      Dissasemble(0x100);
    }


    public void Dissasemble(ushort address)
    {
      //var dissasembledInstructions = _gameBoy.Disassamble(address);
      //_instructions.Clear();
      //foreach (var dissasembledInstruction in dissasembledInstructions)
      //{
      //  InstructionViewModel inst = new InstructionViewModel(dissasembledInstruction);
      //  _instructions.Add(inst);
      //  _addressToInstruction[dissasembledInstruction.Address] = inst;
      //}
      //SelectedInstruction = _addressToInstruction[address];
      _disassembler.PoorManDisassemble();
      _instructions.Clear();
      byte[][] matrix = _disassembler.DisassembledMatrix;
      int instCount = _disassembler.DisassembledCount;
      for (int i = 0; i < instCount; ++i)
      {
        var vm = new InstructionViewModel();
        // We fill it up
        byte[] entry = matrix[i];
        // We check the length
        if (entry[0] == 1)
        {
          int instAddress = ((entry[4] << 8) | entry[5]);
          vm.Address = "0x" + instAddress.ToString("x2");
          vm.Opcode = "0x" + entry[1].ToString("x2");
          vm.Name = CPUOpcodeNames.Get(entry[1]);
          vm.Description = CPUInstructionDescriptions.Get(entry[1]);
        }
        else if (entry[0] == 2)
        {
          if (entry[1] != 0xCB)
          {
            int instAddress = ((entry[4] << 8) | entry[5]);
            vm.Address = "0x" + instAddress.ToString("x2");
            vm.Opcode = "0x" + entry[1].ToString("x2");
            vm.Name = CPUOpcodeNames.Get(entry[1]);
            vm.Literal = "0x" + entry[2].ToString("x2");
            vm.Description = CPUInstructionDescriptions.Get(entry[1]);
          }
          else
          {
            int instAddress = ((entry[4] << 8) | entry[5]);
            vm.Address = "0x" + instAddress.ToString("x2");
            int instOpcode = ((entry[1] << 8) | entry[2]);
            vm.Opcode = "0x" + instOpcode.ToString("x2");
            vm.Name = CPUCBOpcodeNames.Get(entry[2]);
            vm.Literal = "0x" + entry[2].ToString("x2");
            vm.Description = CPUCBInstructionDescriptions.Get(entry[2]);
          }
        }
        else
        {
          int instAddress = ((entry[4] << 8) | entry[5]);
          vm.Address = "0x" + instAddress.ToString("x2");
          vm.Opcode = "0x" + entry[1].ToString("x2");
          vm.Name = CPUOpcodeNames.Get(entry[1]);
          int literal = ((entry[2] << 8) | entry[3]);
          vm.Literal = "0x" + literal.ToString("x2");
          vm.Description = CPUInstructionDescriptions.Get(entry[1]);
        }

        _instructions.Add(vm);
      }
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