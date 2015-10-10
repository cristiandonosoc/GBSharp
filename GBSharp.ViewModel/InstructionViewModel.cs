namespace GBSharp.ViewModel
{
  public class InstructionViewModel : ViewModelBase
  {
    private readonly IInstruction _instruction;

    public string Address { get { return "0x" + _instruction.Address.ToString("x2"); } }
    public string Opcode { get { return "0x" + _instruction.OpCode.ToString("x2"); } }
    public string Name { get { return _instruction.Name; } }
    public string Literal { get { return "0x" + _instruction.Literal.ToString("x2"); } }
    public string Description { get { return _instruction.Description; } }

    public InstructionViewModel(IInstruction instruction)
    {
      _instruction = instruction;
    }

  }
}