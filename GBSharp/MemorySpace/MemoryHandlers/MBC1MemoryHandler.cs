using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  /// <summary>
  /// Memory Bank Controller 1 has two different maximum memory modes:
  /// 16Mbit ROM/8KByte RAM or 4Mbit ROM/32KByte RAM.
  /// 
  /// Writing a value (XXXXXXXS - X = Don't care, S = Memory model select)
  /// into 6000-7FFF area will select the memory model to use.
  /// S = 0 selects 16/8 mode. S = 1 selects 4/32 mode.
  /// 
  /// Writing a value (XXXBBBBB - X = Don't cares, B = bank select bits) into
  /// 2000-3FFF area will select an appropriate ROM bank at 4000-7FFF.
  /// Values of 0 and 1 do the same thing and point to ROM bank 1.
  /// Rom bank 0 is not accessible from 4000-7FFF and can only be read from
  /// 0000-3FFF.
  /// 
  /// If memory model is set to 4/32: Writing a value (XXXXXXBB - X = Don't
  /// care, B = bank select bits) into 4000-5FFF area will select an
  /// appropriate RAM bank at A000-C000. Before you can read or write to a RAM
  /// bank you have to enable it by writing a XXXX1010 into 0000-1FFF area*.
  /// To disable RAM bank operations write any value but XXXX1010 into
  /// 0000-1FFF area. Disabling a RAM bank probably protects that bank from
  /// false writes during power down of the GameBoy.
  /// (NOTE: Nintendo suggests values 0A to enable and 00 to disable RAM bank!)
  /// 
  /// If memory model is set to 16/8 mode: Writing a value (XXXXXXBB - X =
  /// Don't care, B = bank select bits) into 4000-5FFF area will set the two
  /// most significant ROM address lines.
  /// </summary>
  class MBC1MemoryHandler : MemoryHandler
  {
    private const ushort romBank0Start = 0x0000;
    private const ushort romBankLength = 0x4000;
    private const ushort ramBank0Start = 0xA000;
    private const ushort ramBankLength = 0x2000;
    private enum MBC1Modes : byte { Rom16MbRam8KB = 0, Rom4MbRam32KB = 1 }
    /// The MBC1 defaults to 16Mbit ROM/8KByte RAM mode on power up.
    private MBC1Modes mode = MBC1Modes.Rom16MbRam8KB;

    /// <summary>
    /// Class constructor. Performs the loading of the current cartridge into memory.
    /// </summary>
    /// <param name="gameboy"></param>
    internal MBC1MemoryHandler(GameBoy gameboy)
      : base(gameboy)
    {
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       this.memoryData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, romBankLength));
    }

    internal override void Write(ushort address, byte value)
    {
      /* [0x0000 - 0x7FFF]: Memory Bank 0, Memory Bank 1 */
      if (address < 0x8000)
      {
        /* [0x0000 - 0x1FFF]: RAM bank R/W enable: */
        if (address < 0x2000)
        {
          // TODO: For completeness the ram bank enable/disable mechanism should be
          // implemented here, but it doesn't seem to be necessary.
        }
        /* [0x2000 - 0x3FFF]: ROM Bank select */
        else if (address < 0x4000)
        {
          // ROM bank select
          byte bank = (byte)(value & 0x1F);
          if (bank == 0) { value = 1; } // 0 and 1 point to bank 1.
          Buffer.BlockCopy(this.cartridge.Data, bank * romBankLength,
                           this.memoryData, romBankLength, romBankLength);
        }
        /* [0x4000 - 0x5FFF]: RAM Bank select */
        else if (address < 0x6000)
        {
          // RAM bank select

        }
        /* [0x6000 - 0x7FFF]: Mode select */
        else
        {
          this.mode = (MBC1Modes)(value & 0x01);
        }
      } // < 0x8000
      else
      {
        base.Write(address, value);
      }
    }
  }
}
