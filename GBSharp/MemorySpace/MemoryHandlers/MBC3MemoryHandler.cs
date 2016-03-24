using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

    // ROM data doesn't need to be backuped
    internal byte[] _romBanksData;

    [Serializable]
    class State
    {
      internal byte CurrentRomBank;
      internal byte CurrentRamBank;
      internal byte[] RamBanksData;
    }
    State _state = new State();

    /// <summary>
    /// Class constructor. Performs the loading of the current cartridge into memory.
    /// </summary>
    /// <param name="gameboy"></param>
    internal MBC3MemoryHandler(GameBoy gameboy)
      : base(gameboy)
    {
      _state.CurrentRamBank = 0;
      _state.CurrentRomBank = 0;
      _state.RamBanksData = new byte[0x8000]; // 32768 bytes
      _romBanksData = new byte[0x200000]; // 2097152 bytes

      // We get the memory pointer
      byte[] memoryData = memory.MemoryData;

      // Copy ROM banks
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       _romBanksData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, _romBanksData.Length));

      // Copy first and second ROM banks
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       memoryData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, romBankLength * 2));
    }

    internal override byte[] GetStateData()
    {
      byte[] result;
      BinaryFormatter formatter = new BinaryFormatter();
      using (MemoryStream memoryStream = new MemoryStream())
      {
        formatter.Serialize(memoryStream, _state);
        result = memoryStream.ToArray();
      }
      return result;
    }

    internal override void SetStateData(byte[] data)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        BinaryFormatter formatter = new BinaryFormatter();
        memoryStream.Write(data, 0, data.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        _state = formatter.Deserialize(memoryStream) as State;
      }
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

          if (bank == _state.CurrentRomBank) { return; } // Avoid unnecessary switching

          _state.CurrentRomBank = bank;

          Buffer.BlockCopy(this.cartridge.Data, _state.CurrentRomBank * romBankLength,
                           memoryData, romBankLength, romBankLength);
          
        }
        /* [0x4000 - 0x5FFF]: RAM Bank select or real time clock access */
        else if (address < 0x6000)
        {
          // RAM bank select
          if (value <= 0x03)
          {
            byte newRamBank = (byte)(value & 0x03);

            if (newRamBank == _state.CurrentRamBank) { return; } // Avoid unnecessary switching

            // Backup current bank
            Buffer.BlockCopy(memoryData, ramBank0Start, _state.RamBanksData,
                             _state.CurrentRamBank * ramBankLength, ramBankLength);

            // Copy the new one
            _state.CurrentRamBank = newRamBank;
            Buffer.BlockCopy(_state.RamBanksData, _state.CurrentRamBank * ramBankLength,
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
