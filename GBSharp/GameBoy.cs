using GBSharp.Catridge;
using GBSharp.CPU;
using GBSharp.Memory;

namespace GBSharp
{
  public class GameBoy : IGameBoy
  {
    private CPU.CPU cpu;
    private Memory.Memory memory;
    private Cartridge cartridge;

    /// <summary>
    /// Class constructor.
    /// Initializes cpu and memory.
    /// </summary>
    public GameBoy()
    {
      this.memory = new Memory.Memory();
      this.cpu = new CPU.CPU(this.memory);
      this.cartridge = new Cartridge();
    }

    public ICPU CPU
    {
      get { return cpu; }
    }

    public IMemory Memory
    {
      get { return memory; }
    }

    public ICartridge Cartridge
    {
      get { return cartridge; }
    }

    public void LoadCartridge(byte[] cartridgeData)
    {
      this.cartridge.Load(cartridgeData);
    }
  }
}
