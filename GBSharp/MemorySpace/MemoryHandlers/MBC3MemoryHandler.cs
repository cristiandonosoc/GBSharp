using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  /// <summary>
  /// Memory Bank Controller 3
  /// This controller is similar to MBC1 except it accesses all 16Mbits
  /// of ROM without requiring any writes to the 4000-5FFF area.
  /// Writing a value (XBBBBBBB, X = Don't care, B = bank select bits)
  /// into 2000-3FFF area will select an appropriate ROM bank at 4000-7FFF.
  /// Also, this MBC has a built-in battery-backed Real Time Clock (RTC)
  /// not found in any other MBC. Some MBC3 carts do not support it
  /// (WarioLand II non color version) but some do (Harvest Moon/Japanese
  /// version.)
  /// </summary>
  class MBC3MemoryHandler : MemoryHandler
  {
    private const ushort romBank0Start = 0x0000;
    private const ushort romBankLength = 0x4000;
    private const ushort ramBank0Start = 0xA000;
    private const ushort ramBankLength = 0x2000;
    private byte currentRomBank;
    private byte currentRamBank;
    private byte[] ramBanksData;
    private byte[] romBanksData;

    /// <summary>
    /// Class constructor. Performs the loading of the current cartridge into memory.
    /// </summary>
    /// <param name="gameboy"></param>
    internal MBC3MemoryHandler(GameBoy gameboy)
      : base(gameboy)
    {
      this.currentRamBank = 0;
      this.currentRomBank = 0;
      this.ramBanksData = new byte[0x8000]; // 32768 bytes
      this.romBanksData = new byte[0x200000]; // 2097152 bytes

      // We get the memory pointer
      byte[] memoryData = memory.MemoryData;

      // Copy ROM banks
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       this.romBanksData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, this.romBanksData.Length));

      // Copy first and second ROM banks
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       memoryData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, romBankLength * 2));
    }

    internal override void Write(ushort address, byte value)
    {
      // We ge the memory pointer
      byte[] memoryData = memory.MemoryData;

      /* [0x0000 - 0x7FFF]: Memory Bank 0, Memory Bank 1 */
      if (address < 0x8000)
      {
        /* [0x0000 - 0x1FFF]: RAM bank R/W enable: */
        if (address < 0x2000)
        {
          // TODO: For completeness the ram bank enable/disable mechanism should be
          // implemented here, but it doesn't seem to be necessary.
        }
        /* [0x2000 - 0x3FFF]: ROM Bank select (7 bits) */
        else if (address < 0x4000)
        {
          // ROM bank select
          byte bank = (byte)(value & 0x7F);

          if (bank == currentRomBank) { return; } // Avoid unnecessary switching

          currentRomBank = bank;

          Buffer.BlockCopy(this.cartridge.Data, currentRomBank * romBankLength,
                           memoryData, romBankLength, romBankLength);
          
        }
        /* [0x4000 - 0x5FFF]: RAM Bank select or real time clock access */
        else if (address < 0x6000)
        {
          // RAM bank select
          if (value <= 0x03)
          {
            byte newRamBank = (byte)(value & 0x03);

            if (newRamBank == currentRamBank) { return; } // Avoid unnecessary switching

            // Backup current bank
            Buffer.BlockCopy(memoryData, ramBank0Start, this.ramBanksData,
                             currentRamBank * ramBankLength, ramBankLength);

            // Copy the new one
            currentRamBank = newRamBank;
            Buffer.BlockCopy(this.ramBanksData, currentRamBank * ramBankLength,
                             memoryData, ramBank0Start, ramBankLength);
          }
          else
          // RTC access
          {
            //throw new NotImplementedException("MBC3+RTC Real time clock access not implemented.");
          }
        }
        /* [0x6000 - 0x7FFF]: RTC registers latch */
        else
        {
          // throw new NotImplementedException("MBC3+RTC Real time clock latch not implemented.");
        }
      } // < 0x8000
      else
      {
        base.Write(address, value);
      }
    }
  }
}
