namespace GBSharp.ViewModel
{
  public class InstructionViewModel : ViewModelBase
  {
    private readonly IInstruction _instruction;

    //public string Address { get { return "0x" + _instruction.Address.ToString("x2"); } }
    //public string Opcode { get { return "0x" + _instruction.OpCode.ToString("x2"); } }
    //public string Name { get { return _instruction.Name; } }
    //public string Literal { get { return "0x" + _instruction.Literal.ToString("x2"); } }
    //public string Description { get { return _instruction.Description; } }

    internal ushort originalOpcode { get; set; }
    public string Address { get; set; }
    public string Opcode { get; set; }
    public string Name { get; set; }
    public string Literal { get; set; }
    public string Description { get; set; }

    public InstructionViewModel(IInstruction instruction)
    {
      _instruction = instruction;
      Address = "0x" + instruction.Address.ToString("x2");
      Opcode = "0x" + instruction.OpCode.ToString("x2");
      Name = instruction.Name;
      Literal = "0x" + instruction.Literal.ToString("x2");
      Description = instruction.Description;
    }

    public InstructionViewModel() { }
  }
}