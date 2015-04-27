namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private readonly IGameBoy _gameBoy;
    private readonly CartridgeViewModel _cartridge;

    public CartridgeViewModel Cartridge
    {
      get { return _cartridge; }
    }

    public GameBoyViewModel(IGameBoy gameBoy)
    {
      _gameBoy = gameBoy;
      _cartridge = new CartridgeViewModel(_gameBoy.Cartridge);
      _cartridge.CartridgeFileLoaded += OnCartridgeFileLoaded;
    }

    private void OnCartridgeFileLoaded(byte[] data)
    {
      _gameBoy.LoadCartridge(data);
      _cartridge.Update();
    }
  }
}