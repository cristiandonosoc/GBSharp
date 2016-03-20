using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;
using GBSharp.VideoSpace;
using GBSharp.AudioSpace;
using GBSharp.CPUSpace;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  /// <summary>
  /// A memory handler contains the logic for the management
  /// of the memory for a certain catdridge type. Different
  /// catdridges handle memory mapping and storing in
  /// different ways, but should be transparent to the CPU.
  /// </summary>
  class MemoryHandler : IDisposable
  {

    #region ATTRIBUTES
    protected GameBoy gameboy;
    protected Memory memory;
    protected Cartridge.Cartridge cartridge;
    protected Display display;
    protected APU apu;
    protected CPU cpu;
    #endregion

    #region CONSTRUCTORS
    internal MemoryHandler(GameBoy gameboy)
    {
      this.gameboy = gameboy;
      this.memory = (Memory)gameboy.Memory;
      this.cartridge = (Cartridge.Cartridge)gameboy.Cartridge;
      this.cpu = (CPU)gameboy.CPU;
      this.display = (Display)gameboy.Display;
      this.apu = (APU)gameboy.APU;
    }
    #endregion

    #region PUBLIC METHODS

    // TODO(Cristian): Do we need this method here? Could be in memory...
    internal void DmaReady()
    {
      // NOTE(Cristian): The value is not important, this is a signal
      this.display.HandleMemoryChange(MMR.DMA, 0);
    }

    /// <summary>
    /// Writes 8 bits value to memory according to
    /// the correspondant memory management scheme.
    /// </summary>
    /// <param name="address">16 bits address.</param>
    /// <param name="value">8 bits value.</param>
    internal virtual void Write(ushort address, byte value)
    {
      // We get the pointer
      byte[] memoryData = memory.MemoryData;

      /* [0x0000 - 0x7FFF]: Memory Bank 0, Memory Bank 1 */
      if (address < 0x8000)
      {
        // Do nothing, not writeable!
      } // < 0x8000

      /* [0x8000 - 0xBFFF]: VRAM, Cartridge RAM */
      else if (address < 0xC000)
      {
        memoryData[address] = value;
      } // < 0xC000

      /* [0xC000 - 0xDFFF]: Internal RAM */
      else if (address < 0xE000)
      {
        memoryData[address] = value;

        // Internal RAM is 8kb, but RAM Echo is only 7.5kb
        if (address < 0xDE00)
        {
          // Copy to RAM Echo, add 8kb offset
          memoryData[address + 0x2000] = value;
        }
      } // < 0xE000

      /* [0xE000 - 0xFDFF]: RAM Echo */
      else if (address < 0xFE00)
      {
        memoryData[address] = value;
        memoryData[address - 0x2000] = value; // 8kb offset
      } // < 0xFE00

      /* [0xFE00 - 0xFE9F]: Sprite Attributes Memory (OAM) */
      else if (address < 0xFEA0)
      {
        memoryData[address] = value;
        // TODO(Cristian): This is to see if OAM table is changed without DMA.
        //                 Perhaps the only way to access it is through DMA.
        //                 If that is the case, sprite sorting is greatly simplified
        //throw new InvalidProgramException("OAM should be accessed through DMA");
      } // < 0xFEA0

      /* [0xFEA0 - 0xFF7F]: */
      else if (address < 0xFF80)
      {
        // During DMA transfer, the CPU can only access the High-RAM block
        // HRAM: 0xFF80-0xFE9F
        /* [0xFEA0 - 0xFEFF]: Empty but unusable for I/O */
        if (address < 0xFF00)
        {
          memoryData[address] = value;
        }
        
        /* [0xFF00 - 0xFF4B]: I/O Ports */
        else if (address < 0xFF4C)
        {
          if (address == (ushort)MMR.P1)
          {
            byte p1 = memoryData[address];
            // Only the bits 4 and 5 are writable in P1
            p1 &= 0xCF; // &= 11001111;
            p1 |= (byte)(value & 0x30); // |= (value & 00110000); writable mask
            memoryData[address] = p1;


            // Request an interrupt if necessary
            this.gameboy.InterruptController.UpdateKeypadState();
          }

          // Any write to DIV restarts the timer
          else if (address == (ushort)MMR.DIV)
          {
            memoryData[address] = 0;
          }

          else if ((address == (ushort)MMR.TIMA) ||
                   (address == (ushort)MMR.TMA) ||
                   (address == (ushort)MMR.TAC))
          {
            // NOTE(Cristian): Changes to memory value are handled by the cpu handler
            this.cpu.HandleMemoryChange((MMR)address, value);
          }

          else if (address == (ushort)MMR.IF)
          {
            // IF has a 0xE0 mask
            memoryData[address] = (byte)(0xE0 | value);
            // Trigger interrupt check event
            this.cpu.CheckForInterruptRequired();
          }

          /* [0xFF10 - 0xFF26]: Sound registers */
          else if ((0xFF10 <= address) && (address <= 0xFF26))
          {
            // NOTE(Cristian): Memory change in the sound register area
            //                 is very specific, so all the changes to the
            //                 actual values are made by the APU and sound channels
            //                 themselves
            this.apu.HandleMemoryChange((MMR)address, value);
          }

          else if ((0xFF27 <= address) && (address <= 0xFF2F))
          {
            // This addresses cannot be written
          }
          else if ((0xFF30 <= address) && (address < 0xFF40))
          {
            this.apu.HandleWaveWrite(address, value);
            memoryData[address] = value;
          }
          // NOTE(Cristian): We start a DMA process.
          else if (address == (ushort)MMR.DMA)
          {
            this.memory.StartDma(value);
          }
          else if (0xFF40 <= address)
          {
            memoryData[address] = value;
            // We handle display memory changes
            this.display.HandleMemoryChange((MMR)address, value);
          }
          else
          {
            memoryData[address] = value;
          }
        }

        /* [0xFF4C - 0xFF7F]: Empty but unusable for I/O */
        else
        {
          memoryData[address] = value;
        }
      } // < 0xFF80

      /* [0xFF80 - 0xFFFE]: Internal RAM */
      else if (address < 0xFFFF)
      {
        memoryData[address] = value;
      } // < 0xFFFF

      /* [0xFFFF]: INTERRUPT ENABLE */
      else
      {
        memoryData[address] = value;
        // Trigger memory event check
        this.cpu.CheckForInterruptRequired();
      } // < 0x10000
    }

    /// <summary>
    /// Writes 16 bits value to memory according to
    /// the correspondant memory management scheme.
    /// </summary>
    /// <param name="address">16 bit address.</param>
    /// <param name="value">16 bit value.</param>
    internal virtual void Write(ushort address, ushort value)
    {
      Write(address, (byte)(value & 0x00FF));
      Write(++address, (byte)((value >> 8) & 0x00FF));
    }

    /// <summary>
    /// Reads 8 bit value from memory according to
    /// the correspondant memory management scheme.
    /// </summary>
    /// <param name="address">16 bit address.</param>
    /// <returns>
    /// 8 bit value located at the address
    /// correspondant to the memory management scheme.
    /// </returns>
    virtual internal byte Read(ushort address)
    {
      // We get the pointer
      byte[] memoryData = memory.MemoryData;


      // We see if it's wave data
      if ((0xFF30 <= address) && (address < 0xFF40))
      {
        byte wave = this.apu.HandleWaveRead(address);
        return wave;
      }

      // The sound values come or'ed
      byte value = memoryData[address];
      switch((MMR)address)
      {
        case MMR.NR10: value |= 0x80; break;
        case MMR.NR11: value |= 0x3F; break;
        case MMR.NR12: value |= 0x00; break;
        case MMR.NR13: value |= 0xFF; break;
        case MMR.NR14: value |= 0xBF; break;

        case MMR.NR21: value |= 0x3F; break;
        case MMR.NR22: value |= 0x00; break;
        case MMR.NR23: value |= 0xFF; break;
        case MMR.NR24: value |= 0xBF; break;

        case MMR.NR30: value |= 0x7F; break;
        case MMR.NR31: value |= 0xFF; break;
        case MMR.NR32: value |= 0x9F; break;
        case MMR.NR33: value |= 0xFF; break;
        case MMR.NR34: value |= 0xBF; break;

        case MMR.NR41: value |= 0xFF; break;
        case MMR.NR42: value |= 0x00; break;
        case MMR.NR43: value |= 0x00; break;
        case MMR.NR44: value |= 0xBF; break;

        case MMR.NR50: value |= 0x00; break;
        case MMR.NR51: value |= 0x00; break;
        case MMR.NR52: value |= 0x70; break;
      }

      return value;
    }

    public virtual void Dispose()
    {

    }

    #endregion
  }
}
