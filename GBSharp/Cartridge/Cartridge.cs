using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridge
{
  public class Cartridge : ICartridge
  {
    public ushort MemoryChangedLow { get; private set; }
    public ushort MemoryChangedHigh { get; private set; }

    [Serializable]
    internal class State
    {
      internal byte[] Rom;
      internal int RomSize;
      internal int RamSize;
      internal CartridgeType Type;
      internal string Title;
    }
    State _state = new State();
    internal State GetState() { return _state; }
    internal void SetState(State state) { _state = state; }

    /// <summary>
    /// Loads the contents of the catridge rom from a byte array.
    /// </summary>
    public void Load(byte[] rom)
    {
      // Magic addresses!
      ushort titleAddress = 0x0134;
      ushort catridgeTypeAddress = 0x0147;
      ushort ramSizeAddress = 0x0149;

      // Save rom reference and parse headers
      _state.Rom = rom;
      _state.RomSize = _state.Rom.Length;
      _state.RamSize = 0;

      if (_state.Rom.Length > ramSizeAddress)
      {
        switch (_state.Rom[ramSizeAddress])
        {
          case 0:
            _state.RamSize = 0;
            break;
          case 1:
            _state.RamSize = 1 * 1024;
            break;
          case 2:
            _state.RamSize = 8 * 1024;
            break;
          case 3:
            _state.RamSize = 32 * 1024;
            break;
          case 4:
            _state.RamSize = 128 * 1024;
            break;
        }
      }

      _state.Type = _state.Rom.Length > catridgeTypeAddress ? (CartridgeType)_state.Rom[catridgeTypeAddress] : CartridgeType.ROM_ONLY;
      _state.Title = "";

      if (rom.Length > titleAddress + 16)
      {
        for (var i = 0; i < 16; ++i)
        {
          if (_state.Rom[titleAddress + i] == 0)
          {
            break;
          }
          _state.Title += (char)_state.Rom[titleAddress + i];
        }
      }
    }

    /// <summary>
    /// See: CatridgeType enum.
    /// </summary>this
    public CartridgeType Type
    {
      get { return _state.Type; }
    }

    /// <summary>
    /// Contents of the rom.
    /// </summary>
    public byte[] Data
    {
      get { return _state.Rom; }
    }

    /// <summary>
    /// Game title, max 16 characters.
    /// </summary>
    public string Title
    {
      get { return _state.Title; }
    }

    /// <summary>
    /// ROM size in bytes.
    /// </summary>
    public int RomSize
    {
      get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// RAM size in bytes.
    /// </summary>
    public int RamSize
    {
      get { throw new NotImplementedException(); }
    }
  }
}
