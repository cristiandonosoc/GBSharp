﻿using GBSharp.CPUSpace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class BreakpointsViewModel : ViewModelBase
  {
    public event Action BreakpointChanged;

    private readonly IGameBoy _gameboy;

    private readonly ObservableCollection<BreakpointViewModel> _breakpoints 
      = new ObservableCollection<BreakpointViewModel>();
    public ObservableCollection<BreakpointViewModel> Breakpoints
    {
      get { return _breakpoints; }
    }
    private BreakpointViewModel _activeBreakpoint;

    private string _addressField;
    public string AddressField
    {
      get { return _addressField; }
      set
      {
        if(_addressField == value) { return; }
        _addressField = value;
        OnPropertyChanged(() => AddressField);
      }
    }

    public BreakpointsViewModel(IGameBoy gameboy)
    {
      _gameboy = gameboy;
      _gameboy.CPU.BreakpointFound += CPU_BreakpointFound;
    }

    private void CPU_BreakpointFound()
    {
      Breakpoint breakpoint = _gameboy.CPU.CurrentBreakpoint;
      foreach(BreakpointViewModel bpvm in _breakpoints)
      {
        if(bpvm.OriginalAddress == breakpoint.Target)
        {
          // We clear the past breakpoint
          if(_activeBreakpoint != null)
          {
            _activeBreakpoint.IsExecuteActive = false;
            _activeBreakpoint.IsReadActive = false;
            _activeBreakpoint.IsWriteActive = false;
            _activeBreakpoint.IsJumpActive = false;
          }

          _activeBreakpoint = bpvm;

          switch(breakpoint.Kind)
          {
            case BreakpointKinds.EXECUTION:
              _activeBreakpoint.IsExecuteActive = true;
              break;
            case BreakpointKinds.READ:
              _activeBreakpoint.IsReadActive = true;
              break;
            case BreakpointKinds.WRITE:
              _activeBreakpoint.IsWriteActive = true;
              break;
            case BreakpointKinds.JUMP:
              _activeBreakpoint.IsJumpActive = true;
              break;
          }

          break;
        }
      }
    }

    private void ClearActiveBreakpoint()
    {
      if (_activeBreakpoint != null)
      {
        _activeBreakpoint.IsExecuteActive = false;
        _activeBreakpoint.IsReadActive = false;
        _activeBreakpoint.IsWriteActive = false;
        _activeBreakpoint.IsJumpActive = false;
      }
    }

    public void StepHandler()
    {
      // We clear the active breakpoint because step doesn't trigger
      // breakpoints
      ClearActiveBreakpoint();
    }

    public void RunHandler()
    {
      // Running clears the active breakpoint
      ClearActiveBreakpoint();
    }

    public void RecreateBreakpoints()
    {
      Dictionary<ushort, int> breakpointKindMap = new Dictionary<ushort, int>();
      
      // We create the breakpoint mask
      List<ushort> execBreakpoints = _gameboy.CPU.GetBreakpoints(BreakpointKinds.EXECUTION);
      foreach (ushort execBreakpoint in execBreakpoints)
      {
        int mask = 0;
        if (breakpointKindMap.ContainsKey(execBreakpoint))
        {
          mask = breakpointKindMap[execBreakpoint];
        }
        // We add the breakpoint mask
        breakpointKindMap[execBreakpoint] = mask | 1;
      }

      List<ushort> readBreakpoints = _gameboy.CPU.GetBreakpoints(BreakpointKinds.READ);
      foreach (ushort readBreakpoint in readBreakpoints)
      {
        int mask = 0;
        if (breakpointKindMap.ContainsKey(readBreakpoint))
        {
          mask = breakpointKindMap[readBreakpoint];
        }
        // We add the breakpoint mask
        breakpointKindMap[readBreakpoint] = mask | 2;
      }

      List<ushort> writeBreakpoints = _gameboy.CPU.GetBreakpoints(BreakpointKinds.WRITE);
      foreach (ushort writeBreakpoint in writeBreakpoints)
      {
        int mask = 0;
        if (breakpointKindMap.ContainsKey(writeBreakpoint))
        {
          mask = breakpointKindMap[writeBreakpoint];
        }
        // We add the breakpoint mask
        breakpointKindMap[writeBreakpoint] = mask | 4;
      }

      List<ushort> jumpBreakpoints = _gameboy.CPU.GetBreakpoints(BreakpointKinds.JUMP);
      foreach (ushort jumpBreakpoint in jumpBreakpoints)
      {
        int mask = 0;
        if (breakpointKindMap.ContainsKey(jumpBreakpoint))
        {
          mask = breakpointKindMap[jumpBreakpoint];
        }
        // We add the breakpoint mask
        breakpointKindMap[jumpBreakpoint] = mask | 8;
      }

      // We search for enabled
      foreach (BreakpointViewModel enabledBreakpoint in _breakpoints)
      {
        int mask = 0;
        if (breakpointKindMap.ContainsKey(enabledBreakpoint.OriginalAddress))
        {
          mask = breakpointKindMap[enabledBreakpoint.OriginalAddress];
        }
        // We add the breakpoint mask
        if(enabledBreakpoint.Enabled)
        {
          breakpointKindMap[enabledBreakpoint.OriginalAddress] = mask | 16;
        }
        else
        {
          if(enabledBreakpoint.ToBeDeleted) { continue; }
          breakpointKindMap[enabledBreakpoint.OriginalAddress] = mask | 32;
        }
      }


      ushort[] keys = new ushort[breakpointKindMap.Count];
      breakpointKindMap.Keys.CopyTo(keys, 0);
      Array.Sort(keys);
      
      _breakpoints.Clear();

      foreach(ushort address in keys)
      {
        IInstruction inst = _gameboy.Disassembler.FetchAndDecode(address);
        BreakpointViewModel vm = new BreakpointViewModel(_gameboy, inst);

        int mask = breakpointKindMap[address];

        vm.BreakpointChanged += UpdateBreakpoints;
        vm.BreakpointDeleted += Vm_BreakpointDeleted;
        vm.Enabled = true;
        if((mask & 1)   != 0) { vm.DirectOnExecute = true; }
        if((mask & 2) 	!= 0) { vm.DirectOnRead = true; }
        if((mask & 4) 	!= 0) { vm.DirectOnWrite = true; }
        if((mask & 8) 	!= 0) { vm.DirectOnJump = true; }
        if((mask & 16)  != 0) { vm.Enabled = true; }
        if((mask & 32)  != 0) { vm.Enabled = false; }

        // This is *very* ugly, but we use it to search for a highlighted breakpoint.
        // TODO(Cristian): Do this without erasing all records everytime
        CPU_BreakpointFound();

        _breakpoints.Add(vm);
      }
    }

    private void Vm_BreakpointDeleted()
    {
      RecreateBreakpoints();
      BreakpointChanged();
    }

    /// <summary>
    /// This event is triggered when the user unchecked all the
    /// breakpoints kind, so the breakpoint is deleted
    /// </summary>
    private void UpdateBreakpoints()
    {
      // We notify other views about this
      BreakpointChanged();
    }

    public ICommand AddBreakpointCommand { get { return new DelegateCommand(AddBreakpoint); } }
    public void AddBreakpoint()
    {
      ushort address = 0;
      try
      {
        address = Convert.ToUInt16(AddressField, 16);
      }
      catch(FormatException) { return; }

      _gameboy.CPU.AddBreakpoint(BreakpointKinds.EXECUTION, address);
      RecreateBreakpoints();
      UpdateBreakpoints();
    }


  }
}