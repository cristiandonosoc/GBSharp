﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.Cartridge;

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
      protected byte[] memory;
      protected Cartridge.Cartridge cartridge;
      #endregion

      #region CONSTRUCTORS
      internal MemoryHandler(GameBoy gameboy)
      {
        this.gameboy = gameboy;
        this.cartridge = (Cartridge.Cartridge)gameboy.Cartridge;
        this.memory = gameboy.Memory.Data;
      }
      #endregion

      #region PUBLIC METHODS

      internal virtual void UpdateMemoryReference(byte[] memory)
      {
        this.memory = memory;
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
        if (address < 0x8000) {
          // Do nothing, not writeable!
        }

        /* [0x8000 - 0xBFFF]: VRAM, Cartridge RAM */
        else if (address < 0xC000)
        {
          this.memory[address] = value;
        }

        /* [0xC000 - 0xDFFF]: Internal RAM */
        else if (address < 0xE000) {
          this.memory[address] = value;

          // Internal RAM is 8kb, but RAM Echo is only 7.5kb
          if (address < 0xDE00)
          {
            // Copy to RAM Echo, add 8kb offset
            this.memory[address + 0x2000] = value;
          }
        }

        /* [0xE000 - 0xFDFF]: RAM Echo */
        else if (address < 0xFE00)
        {
          this.memory[address] = value;
          this.memory[address - 0x2000] = value; // 8kb offset
        }

        /* [0xFE00 - 0xFE9F]: Sprite Attributes Memory (OAM) */
        else if (address < 0xFEA0)
        {
          this.memory[address] = value;
        }

        /* [0xFEA0 - 0xFEFF]: Empty but unusable for I/O */
        else if (address < 0xFF00)
        {
          this.memory[address] = value;
        }

        /* [0xFF00 - 0xFF4B]: I/O Ports */
        else if (address < 0xFF4C)
        {
          this.memory[address] = value;
        }

        /* [0xFF4C - 0xFF7F]: Empty but unusable for I/O */
        else if (address < 0xFF80)
        {
          this.memory[address] = value;
        }

        /* [0xFF80 - 0xFFFE]: Internal RAM */
        else if (address < 0xFFFE)
        {
          this.memory[address] = value;
        }

        /* [0xFFFF]: INTERRUPT ENABLE */
        else
        {
          this.memory[address] = value;
        }
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
        Write(address, (byte)((value >> 8) & 0x00FF));
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
        // I don't care
        return this.memory[address];
      }

      #endregion
    }
}
