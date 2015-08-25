namespace GBSharp.ViewModel
{

  public class MemorySectionViewModel : ViewModelBase
  {
    private readonly string _name;
    private readonly uint _initialAddress;
    private readonly uint _finalAddress;

    public MemorySectionViewModel(string name, uint initialAddress, uint finalAddress)
    {
      _name = name;
      _initialAddress = initialAddress;
      _finalAddress = finalAddress;
    }

    public string Name
    {
      get { return _name; }
    }

    
    public uint InitialAddress
    {
      get { return _initialAddress; }
    }

    public uint FinalAddress
    {
      get { return _finalAddress; }
    }
  }
}