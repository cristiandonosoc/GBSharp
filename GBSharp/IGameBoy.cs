using GBSharp.Cartridge;

namespace GBSharp
{
  public interface IGameBoy
  {
    ICPU CPU { get; }
    IMemory Memory { get; }
    ICartridge Cartridge { get; }
    void LoadCartridge(byte[] cartridgeData);
    void Run();
    void Pause();
    void Stop();
    void Step();
    void PressButton(Keypad button);
    void ReleaseButton(Keypad button);
  }
}