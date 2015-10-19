using GBSharp.CPUSpace;
using GBSharp.MemorySpace;

namespace GBSharp.ViewModel
{
  public class InterruptViewModel : ViewModelBase
  {
    private Interrupts _interrupt;
    private readonly IGameBoy _gameBoy;
    private bool _enabled;
    private bool _requested;
    private bool _breakable;
    private string _name;

    public bool Enabled
    {
      get { return _enabled; }
      set
      {
        if (_enabled != value)
        {
          _enabled = value;
          OnPropertyChanged(() => _enabled);
        }
      }
    }

    public bool Requested
    {
      get { return _requested; }
      set
      {
        if (_requested != value)
        {
          _requested = value;
          OnPropertyChanged(() => _requested);
        }
      }
    }

    public bool Breakable
    {
      get { return _breakable; }
      set
      {
        if (_breakable != value)
        {
          _breakable = value;
          _gameBoy.CPU.SetInterruptBreakable(_interrupt, _breakable);
          OnPropertyChanged(() => _breakable);
        }
      }
    }

    public string Name
    {
      get { return _name; }
    }

    public InterruptViewModel(string name, Interrupts interrupt, IGameBoy gameBoy)
    {
      _name = name;
      _interrupt = interrupt;
      _gameBoy = gameBoy;
    }

    public void CopyFromDomain()
    {
      var interruptEnabledWord = _gameBoy.Memory.Data[(int)MemoryMappedRegisters.IE];
      var interruptRequestedWord = _gameBoy.Memory.Data[(int)MemoryMappedRegisters.IF];

      Enabled = (interruptEnabledWord & (byte)_interrupt) > 0;
      Requested = (interruptRequestedWord & (byte)_interrupt) > 0;
    }    
    
  }
}
