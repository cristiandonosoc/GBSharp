using GBSharp.Catridge;

namespace GBSharp
{
  public interface IGameBoy
  {
    ICPU CPU { get; }
    IMemory Memory { get; }
    ICartridge Cartridge { get; }
    void LoadCartridge(byte[] cartridgeData);
  }
}