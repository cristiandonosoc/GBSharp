namespace GBSharp.ViewModel
{
  public enum MemoryWordFormat
  {
    Decimal,
    Binary,
    Hexa
  }

  public class MemoryFormatViewModel : ViewModelBase
  {
    private MemoryWordFormat _memoryWordFormat;
    private string _name;

    public MemoryFormatViewModel(string name, MemoryWordFormat memoryWordFormat)
    {
      _name = name;
      _memoryWordFormat = memoryWordFormat;
    }

    public string Name
    {
      get { return _name; }
    }

    public MemoryWordFormat WordFormat
    {
      get { return _memoryWordFormat; }
    }
  }
}