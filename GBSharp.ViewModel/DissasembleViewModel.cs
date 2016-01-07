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


    public void Dissasemble(ushort currentAddress)
    {
      _disassembler.PoorManDisassemble();
      _instructions.Clear();
      byte[][] matrix = _disassembler.DisassembledMatrix;
      int instCount = 0xFFFF;
      for (int address = 0; address < instCount; ++address)
      {
        byte[] entry = matrix[address];
        int intLength = entry[0];
        if(intLength == 0) { continue; }
        var vm = new InstructionViewModel();
        // We fill it up
        // We check the length
        if (intLength == 1)
        {
          vm.Address = "0x" + address.ToString("x2");
          vm.Opcode = "0x" + entry[1].ToString("x2");
          vm.Name = CPUOpcodeNames.Get(entry[1]);
          vm.Description = CPUInstructionDescriptions.Get(entry[1]);
        }
        else if (intLength == 2)
        {
          if (entry[1] != 0xCB)
          {
            vm.Address = "0x" + address.ToString("x2");
            vm.Opcode = "0x" + entry[1].ToString("x2");
            vm.Name = CPUOpcodeNames.Get(entry[1]);
            vm.Literal = "0x" + entry[2].ToString("x2");
            vm.Description = CPUInstructionDescriptions.Get(entry[1]);
          }
          else
          {
            vm.Address = "0x" + address.ToString("x2");
            int instOpcode = ((entry[1] << 8) | entry[2]);
            vm.Opcode = "0x" + instOpcode.ToString("x2");
            vm.Name = CPUCBOpcodeNames.Get(entry[2]);
            vm.Literal = "0x" + entry[2].ToString("x2");
            vm.Description = CPUCBInstructionDescriptions.Get(entry[2]);
          }
        }
        else
        {
          vm.Address = "0x" + address.ToString("x2");
          vm.Opcode = "0x" + entry[1].ToString("x2");
          vm.Name = CPUOpcodeNames.Get(entry[1]);
          int literal = ((entry[2] << 8) | entry[3]);
          vm.Literal = "0x" + literal.ToString("x2");
          vm.Description = CPUInstructionDescriptions.Get(entry[1]);
        }

        _instructions.Add(vm);
        _addressToInstruction[(ushort)address] = vm;

        if(address == currentAddress)
        {
          SelectedInstruction = vm;
        }
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