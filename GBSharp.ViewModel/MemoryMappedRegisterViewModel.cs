using GBSharp.MemorySpace;

namespace GBSharp.ViewModel
{
  public class MemoryMappedRegisterViewModel : ViewModelBase
  {
    private string _name;
    private string _value;
    private MemoryMappedRegisters _register;
    private IGameBoy _gameBoy;

    public string Name
    {
      get { return _name; }
    }

    public string Value
    {
      get { return _value; }
      set
      {
        if (_value != value)
        {
          _value = value;
          OnPropertyChanged(() => Value);
        }
      }
    }

    
    public MemoryMappedRegisterViewModel(string name, MemoryMappedRegisters register, IGameBoy gameBoy)
    {
      _name = name;
      _register = register;
      _gameBoy = gameBoy;
    }

    public void CopyFromDomain()
    {
      Value = "0x" +_gameBoy.GetRegisterDic()[_register].ToString("x2");
    }

   
  }
}