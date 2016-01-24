using GBSharp.CPUSpace;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class InstructionViewModel : ViewModelBase
  {
    public event Action BreakpointChanged;

    private readonly ICPU _cpu;
    private readonly IInstruction _instruction;

    internal ushort originalOpcode { get; set; }
    internal ushort originalAddress { get; set; }
    public string Address { get; set; }
    public string Opcode { get; set; }
    public string Name { get; set; }
    public string Literal { get; set; }
    public string Description { get; set; }

    private bool _hasBreakpoint;
    public bool HasBreakpoint
    {
      get { return _hasBreakpoint; }
      set
      {
        if(_hasBreakpoint == value) { return; }
        _hasBreakpoint = value;
        OnPropertyChanged(() => HasBreakpoint);
      }
    }

    private bool _isCurrent;
    public bool IsCurrent
    {
      get { return _isCurrent; }
      set
      {
        if (_isCurrent == value) { return; }
        _isCurrent = value;
        OnPropertyChanged(() => IsCurrent);
      }
    }

    public InstructionViewModel(ICPU cpu, IInstruction instruction)
    {
      _cpu = cpu;

      _instruction = instruction;
      originalAddress = instruction.Address;
      Address = "0x" + instruction.Address.ToString("x2");
      Opcode = "0x" + instruction.OpCode.ToString("x2");
      Name = instruction.Name;
      Literal = "0x" + instruction.Literal.ToString("x2");
      Description = instruction.Description;
    }

    public InstructionViewModel(ICPU cpu)
    {
      _cpu = cpu;
    }

    public ICommand ToggleBreakpointCommand
    {
      get { return new DelegateCommand(ToggleBreakpoint); }
    }

    public void ToggleBreakpoint()
    {
      List<ushort> execBreakpoints = _cpu.GetBreakpoints(BreakpointKinds.EXECUTION);
      if(execBreakpoints.Contains(originalAddress))
      {
        _cpu.RemoveBreakpoint(BreakpointKinds.EXECUTION, originalAddress);
        HasBreakpoint = false;
      }
      else
      {
        _cpu.AddBreakpoint(BreakpointKinds.EXECUTION, originalAddress);
        HasBreakpoint = true;
      }

      if(BreakpointChanged != null) { BreakpointChanged(); }
    }
  }
}