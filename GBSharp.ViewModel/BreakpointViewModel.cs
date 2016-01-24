using GBSharp.CPUSpace;

namespace GBSharp.ViewModel
{
  public class BreakpointViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameboy;
    private readonly IInstruction _instruction;

    public string Address { get; private set; }
    public string Name { get; private set; }

    private bool _firstOnExecute = true;
    private bool _onExecute;
    public bool OnExecute
    {
      get { return _onExecute; }
      set
      {
        if(_onExecute == value) { return; }
        _onExecute = value;

        if(_firstOnExecute)
        {
          _firstOnExecute = false;
          return;
        }

        if(_onExecute)
        {
          _gameboy.CPU.AddBreakpoint(BreakpointKinds.EXECUTION, _instruction.Address);
        }
        else
        {
          _gameboy.CPU.RemoveBreakpoint(BreakpointKinds.EXECUTION, _instruction.Address);
        }
        OnPropertyChanged(() => OnExecute);
      }
    }

    public BreakpointViewModel(IGameBoy gameboy, IInstruction inst)
    {
      _gameboy = gameboy;
      _instruction = inst;

      Address = "0x" + inst.Address.ToString("x2");
      Name = inst.Name;
    }
  }
}