using System;
using System.IO;
using GBSharp.Catridge;

namespace GBSharp.ViewModel
{
  public class CartridgeViewModel : ViewModelBase
  {
    public event Action<byte[]> CartridgeFileLoaded;

    private readonly ICartridge _cartridge;
    private readonly MemoryViewModel _memory;
    private string _filePath;

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

    private void NotifyCartridgeFileLoaded()
    {
      if (CartridgeFileLoaded != null)
        CartridgeFileLoaded(File.ReadAllBytes(_filePath));
    }

    public CartridgeViewModel(ICartridge cartridge)
    {
      _cartridge = cartridge;
      _memory = new MemoryViewModel(_cartridge);

    }

    public void Update()
    {
      _memory.Update();
    }
   
  }
}