using System;
using System.IO;
using GBSharp.Cartridge;

namespace GBSharp.ViewModel
{
  public class CartridgeViewModel : ViewModelBase
  {
    public event Action<byte[]> CartridgeFileLoaded;

    private readonly ICartridge _cartridge;
    private readonly MemoryViewModel _memory;
    private string _filePath;

    private string _cartridgeTitle;


    public MemoryViewModel Memory
    {
      get { return _memory; }
    }

    public string FilePath
    {
      get { return _filePath; }
      set
      {
        if (_filePath != value)
        {
          _filePath = value;
          NotifyCartridgeFileLoaded();
        }
      }
    }

    public string CartridgeTitle
    {
      get { return _cartridgeTitle; }
      set
      {
        if (_cartridgeTitle != value)
        {
          _cartridgeTitle = value;
          OnPropertyChanged(() => CartridgeTitle);
        }
      }
    }

    private void NotifyCartridgeFileLoaded()
    {
      if (CartridgeFileLoaded != null && _filePath != null)
        if(File.Exists(_filePath))
          CartridgeFileLoaded(File.ReadAllBytes(_filePath));
    }

    public CartridgeViewModel(ICartridge cartridge)
    {
      _cartridge = cartridge;
      _memory = new MemoryViewModel(_cartridge);

    }

    public void Update()
    {
      CartridgeTitle = _cartridge.Title;
      _memory.Update();
    }
   
  }
}