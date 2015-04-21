namespace GBSharp.ViewModel
{
  public class MainWindowViewModel : ViewModelBase
  {
    private readonly MemoryViewModel _memory;

    public MemoryViewModel Memory
    {
      get { return _memory; }
    }

    public MainWindowViewModel()
    {
      var memory = new MemoryDummy();
      _memory = new MemoryViewModel(memory);
    }

    
  }
}