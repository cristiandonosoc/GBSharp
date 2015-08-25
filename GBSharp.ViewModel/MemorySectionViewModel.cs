namespace GBSharp.ViewModel
{

  public class MemorySectionViewModel : ViewModelBase
  {
    private readonly string _name;
    private readonly int _initialAddress;
    private readonly int _finalAddress;

    public MemorySectionViewModel(string name, int initialAddress, int finalAddress)
    {
      _name = name;
      _initialAddress = initialAddress;
      _finalAddress = finalAddress;
    }

    public string Name
    {
      get { return _name; }
    }

    
    public int InitialAddress
    {
      get { return _initialAddress; }
    }

    public int FinalAddress
    {
      get { return _finalAddress; }
    }
  }
}