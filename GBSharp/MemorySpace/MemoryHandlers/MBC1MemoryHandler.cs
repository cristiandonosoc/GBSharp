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
  /// Memory Bank Controller 1 has two different maximum memory modes:
  /// 2048KB ROM/8KB RAM or 512KB ROM/32KB RAM.
  /// 
  /// Writing a value (XXXXXXXS, X = Don't care, S = Memory model select)
  /// into 6000-7FFF area will select the memory model to use.
  /// S = 0 selects 2048/8 mode. S = 1 selects 512/32 mode.
  /// 
  /// Writing a value (XXXBBBBB, X = Don't cares, B = bank select bits) into
  /// 2000-3FFF area will select an appropriate ROM bank at 4000-7FFF.
  /// Values of 0 and 1 do the same thing and point to ROM bank 1.
  /// Rom bank 0 is not accessible from 4000-7FFF and can only be read from
  /// 0000-3FFF.
  /// 
  /// If memory model is set to 512/32: Writing a value (XXXXXXBB, X = Don't
  /// care, B = bank select bits) into 4000-5FFF area will select an
  /// appropriate RAM bank at A000-C000. Before you can read or write to a RAM
  /// bank you have to enable it by writing a XXXX1010 into 0000-1FFF area*.
  /// To disable RAM bank operations write any value but XXXX1010 into
  /// 0000-1FFF area. Disabling a RAM bank probably protects that bank from
  /// false writes during power down of the GameBoy.
  /// (NOTE: Nintendo suggests values 0A to enable and 00 to disable RAM bank!)
  /// 
  /// If memory model is set to 2048/8 mode: Writing a value (XXXXXXBB, X =
  /// Don't care, B = bank select bits) into 4000-5FFF area will set the two
  /// most significant ROM address lines (bit 6 and 5).
  /// </summary>
  class MBC1MemoryHandler : MemoryHandler
  {
    private const ushort romBank0Start = 0x0000;
    private const ushort romBankLength = 0x4000;
    private const ushort ramBank0Start = 0xA000;
    private const ushort ramBankLength = 0x2000;

    private bool hasBattery;
    private string saveFilePath = "";

    // ROM data doesn't need to be backuped
    internal byte[] _romBanksData;

    private enum MBC1Modes : byte { Rom2048KBRam8KB = 0, Rom512KBRam32KB = 1 }
    [Serializable]
    class State
    {
      /// The MBC1 defaults to 2048KB ROM/8KB RAM mode on power up.
      internal MBC1Modes Mode = MBC1Modes.Rom2048KBRam8KB;
      internal byte CurrentRomBank;
      internal byte CurrentRamBank;
      internal byte[] RamBanksData;
    }
    private State _state = new State();

    /// <summary>
    /// Class constructor. Performs the loading of the current cartridge into memory.
    /// </summary>
    /// <param name="gameboy"></param>
    internal MBC1MemoryHandler(GameBoy gameboy, bool hasBattery = false)
      : base(gameboy)
    {
      _state.CurrentRamBank = 0;
      _state.CurrentRomBank = 1;
      _state.RamBanksData = new byte[0x8000]; // 32768 bytes
      _romBanksData = new byte[0x200000]; // 2097152 bytes

      // We obtain the memory pointer
      byte[] memoryData = memory.MemoryData;

      // Copy ROM banks
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       _romBanksData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, _romBanksData.Length));

      // Copy first and second ROM banks
      Buffer.BlockCopy(this.cartridge.Data, romBank0Start,
                       memoryData, romBank0Start,
                       Math.Min(this.cartridge.Data.Length, romBankLength * 2));

      this.hasBattery = hasBattery;
      if(hasBattery)
      {
        string saveFile = String.Format("{0}.save", gameboy.CartridgeFilename);
        saveFilePath = Path.Combine(gameboy.CartridgeDirectory, saveFile);
        if(File.Exists(saveFilePath))
        {
          byte[] savedRAM = File.ReadAllBytes(saveFilePath);
          Array.Copy(savedRAM, _state.RamBanksData, savedRAM.Length);

          // We send it to main memory
          Buffer.BlockCopy(_state.RamBanksData, _state.CurrentRamBank * ramBankLength,
                           memoryData, ramBank0Start, ramBankLength);
        }
      }
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
      // We get the pointer
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
        /* [0x2000 - 0x3FFF]: ROM Bank select (lower 5 bits) */
        else if (address < 0x4000)
        {
          // ROM bank select
          byte bank = (byte)(value & 0x1F);
          if (bank == 0) { value = 1; } // 0 and 1 point to bank 1.

          // Replace only the lower 5 bits
          _state.CurrentRomBank = (byte)((_state.CurrentRomBank & 0xE0) | (bank & 0x1F));

          Buffer.BlockCopy(this.cartridge.Data, _state.CurrentRomBank * romBankLength,
                           memoryData, romBankLength, romBankLength);
        }
        /* [0x4000 - 0x5FFF]: RAM Bank select or Upper ROM bank bits */
        else if (address < 0x6000)
        {
          // ROM bank select
          if (_state.Mode == MBC1Modes.Rom2048KBRam8KB)
          {
            byte highBank = (byte)((value & 0x03) << 5);

            // Replace only the bit 5 and 6
            _state.CurrentRomBank = (byte)((highBank) | (_state.CurrentRamBank & 0x1F));

            Buffer.BlockCopy(this.cartridge.Data, _state.CurrentRomBank * romBankLength,
                             memoryData, romBankLength, romBankLength);
          }
          // RAM bank select
          else
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
        }
        /* [0x6000 - 0x7FFF]: Mode select */
        else
        {
          _state.Mode = (MBC1Modes)(value & 0x01);
        }
      } // < 0x8000
      else
      {
        base.Write(address, value);
      }
    }

    internal override void Save()
    {
      if (!hasBattery) { return; }
      using (var file = new System.IO.BinaryWriter(new FileStream(saveFilePath, FileMode.Create)))
      {
        // We get the pointer
        byte[] memoryData = memory.MemoryData;

        // We need to save the current data into the rombanks
        Buffer.BlockCopy(memoryData, ramBank0Start, _state.RamBanksData,
                         _state.CurrentRamBank * ramBankLength, ramBankLength);
        file.Write(_state.RamBanksData);
      }
    }

    public override void Dispose()
    {
      base.Dispose();
      Save();

    }
  }
}
