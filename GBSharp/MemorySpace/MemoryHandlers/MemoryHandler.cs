using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;
using GBSharp.VideoSpace;

namespace GBSharp.MemorySpace.MemoryHandlers
{
  /// <summary>
  /// A memory handler contains the logic for the management
  /// of the memory for a certain catdridge type. Different
  /// catdridges handle memory mapping and storing in
  /// different ways, but should be transparent to the CPU.
  /// </summary>
  class MemoryHandler
  {

    #region ATTRIBUTES
    protected GameBoy gameboy;
    protected byte[] memoryData;
    protected Cartridge.Cartridge cartridge;
    protected DMA dma;
    protected Display display;
    #endregion

    #region CONSTRUCTORS
    internal MemoryHandler(GameBoy gameboy)
    {
      this.gameboy = gameboy;
      this.cartridge = (Cartridge.Cartridge)gameboy.Cartridge;
      this.memoryData = gameboy.Memory.Data;
      this.display = (Display)gameboy.Display;
    }
    #endregion

    #region PUBLIC METHODS

    internal virtual void UpdateMemoryReference(byte[] memory, DMA dma)
    {
      this.memoryData = memory;
      this.dma = dma;
    }

    /// <summary>
    /// Writes 8 bits value to memory according to
    /// the correspondant memory management scheme.
    /// </summary>
    /// <param name="address">16 bits address.</param>
    /// <param name="value">8 bits value.</param>
    internal virtual void Write(ushort address, byte value)
    {

      /* [0x0000 - 0x7FFF]: Memory Bank 0, Memory Bank 1 */
      if (address < 0x8000)
      {
        // Do nothing, not writeable!
      } // < 0x8000

      /* [0x8000 - 0xBFFF]: VRAM, Cartridge RAM */
      else if (address < 0xC000)
      {
        this.memoryData[address] = value;
      } // < 0xC000

      /* [0xC000 - 0xDFFF]: Internal RAM */
      else if (address < 0xE000)
      {
        this.memoryData[address] = value;

        // Internal RAM is 8kb, but RAM Echo is only 7.5kb
        if (address < 0xDE00)
        {
          // Copy to RAM Echo, add 8kb offset
          this.memoryData[address + 0x2000] = value;
        }
      } // < 0xE000

      /* [0xE000 - 0xFDFF]: RAM Echo */
      else if (address < 0xFE00)
      {
        this.memoryData[address] = value;
        this.memoryData[address - 0x2000] = value; // 8kb offset
      } // < 0xFE00

      /* [0xFE00 - 0xFE9F]: Sprite Attributes Memory (OAM) */
      else if (address < 0xFEA0)
      {
        this.memoryData[address] = value;
      } // < 0xFEA0

      /* [0xFEA0 - 0xFF7F]: */
      else if (address < 0xFF80)
      {
        // During DMA transfer, the CPU can only access the High-RAM block
        // HRAM: 0xFF80-0xFE9F
        if (this.dma.Active)
        {
          // TODO(Cristian): Investigate what the gameboy actually does on this event.
          //                 Does it just return and the instruction took as much time?
          //                 Does it crash and it's simply not done?
          //                 Who knows...
          //throw new InvalidOperationException("Can only access HRAM during DMA transfer");
        }

        /* [0xFEA0 - 0xFEFF]: Empty but unusable for I/O */
        if (address < 0xFF00)
        {
          this.memoryData[address] = value;
        }

        /* [0xFF00 - 0xFF4B]: I/O Ports */
        else if (address < 0xFF4C)
        {
          if (address == (ushort)MMR.P1)
          {
            byte p1 = this.memoryData[address];
            // Only the bits 4 and 5 are writable in P1
            p1 &= 0xCF; // &= 11001111;
            p1 |= (byte)(value & 0x30); // |= (value & 00110000); writable mask
            this.memoryData[address] = p1;


            // Request an interrupt if necessary
            this.gameboy.InterruptController.UpdateKeypadState();
          }

          // NOTE(Cristian): We start a DMA process.
          else if (address == (ushort)MMR.DMA)
          {
            this.dma.Start(value);
          }
          else if (address >= 0xFF40)
          {
            this.memoryData[address] = value;
            // We handle display memory changes
            this.display.HandleMemoryChange((MMR)address, value);
          }
          else
          {
            this.memoryData[address] = value;
          }

        }

        /* [0xFF4C - 0xFF7F]: Empty but unusable for I/O */
        else
        {
          this.memoryData[address] = value;
        }
      } // < 0xFF80

      /* [0xFF80 - 0xFFFE]: Internal RAM */
      else if (address < 0xFFFF)
      {
        this.memoryData[address] = value;
      } // < 0xFFFF

      /* [0xFFFF]: INTERRUPT ENABLE */
      else
      {
        this.memoryData[address] = value;
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
      // During DMA transfer, the CPU can only access the High-RAM block
      // HRAM: 0xFF80-0xFE9F
      if (this.dma.Active)
      {
        if (address < 0xFF80 || 0xFE9F < address)
        {
          // TODO(Cristian): Investigate what the gameboy actually does on this event.
          //                 Does it just return and the instruction took as much time?
          //                 Does it crash and it's simply not done?
          //                 Who knows...
          //throw new InvalidOperationException("Can only access HRAM during DMA transfer");
        }
      }

      // I don't care
      return this.memoryData[address];
    }

    #endregion
  }
}
