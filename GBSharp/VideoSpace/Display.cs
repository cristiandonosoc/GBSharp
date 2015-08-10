using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GBSharp.VideoSpace
{
  class Display : IDisplay
  {
    private int screenWidth = 160;
    private int screenHeight = 144;
    private Bitmap screen;
    private Memory memory;

    private int backgroundWidth = 256;
    private int backgroundHeight = 256;
    private Bitmap background;

    public Bitmap Screen
    {
      get { return screen; }
    }

    public Bitmap Background
    {
      get { return background; }
    }

    /// <summary>
    /// Display constructor.
    /// </summary>
    /// <param name="interruptController">A reference to the interrupt controller.</param>
    /// <param name="Memory">A reference to the memory.</param>
    public Display(InterruptController interruptController, Memory memory)
    {
      screen = new Bitmap(screenWidth, screenHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
      background = new Bitmap(backgroundWidth, backgroundHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
    }

    internal void UpdateScreen()
    {
      byte lcdRegister = this.memory.Read((ushort)MemoryMappedRegisters.LCDC);

      // Tile memory address comes defined in LCD Control Register bit 3
      // 0: 0x9800 - 0x9BFF
      // 1: 0x9C00 - 0x9FFF
      ushort displayBaseAddress = 0x9800;
      if ((Utils.UtilFuncs.TestBit(lcdRegister, 3)) == 1)
      {
        displayBaseAddress = 0x9C00;
      }

      // Tile data base address is defined in LCD Control Register bit 4
      // 0: 0x8800 - 0x97FF (signed access)
      // 1: 0x8000 - 0x8FFF (unsigned access)
      bool signedAccess = true;
      ushort tileDataBaseAddress = 0x8800;
      if ((Utils.UtilFuncs.TestBit(lcdRegister, 4)) == 1)
      {
        signedAccess = false;
        tileDataBaseAddress = 0x8000;
      }

      // We interpret the pixels
      // NOTE(Cristian): We use uint for the value, when byte would have
      // sufficed (and even make the code a little slimmer). But this permits
      // arbitrary sized screens which might prove useful while debugging.

      // TODO(Cristian): initialX and initialY should be defined by the
      // SCX and SCY registers respectively
      uint initialX = 30;
      uint initialY = 30;
      uint screenWidth = 32;
      uint screenHeight = 32;
      uint displayWidth = 20;
      uint displayHeight = 18;
      uint currentX = initialX;   // Helper X variable to track overflow
      uint currentY = initialY;   // Helper Y variable to track overflow

      uint displayIndex = 0;      // The "pointer" to the actual pixel
      uint firstOfRow = 0;        // To help the pointer go back to the start at overflow
      for (int y = 0; y < displayHeight; y++)
      {
        // We move the index to the start of the display row
        firstOfRow = currentY * screenHeight;
        currentX = initialX;
        displayIndex = (currentY * screenHeight) + currentX;

        for (int x = 0; x < displayWidth; x++)
        {
          // We read the memory
          // TODO(Cristian): Actually read the memory
          Console.Out.WriteLine("{0}: Accessed: {1} (X: {2}, Y: {3})",
            displayIndex, currentX, currentY);

          displayIndex++;
          currentX++;
          if(currentX >= screenWidth)
          {
            currentX = 0;
            displayIndex = firstOfRow;
          }
        }

        Console.Out.WriteLine("ROW");

        // We update the Y variable
        currentY++;
        if(currentY >= screenHeight)
        {
          currentY = 0;
        }
      }
    }


    /// <summary>
    /// Simulates the update of the display for a period of time of a given number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks ellapsed since the last call.
    /// A tick is a complete source clock oscillation, ~238.4 ns (2^-22 seconds).</param>
    internal void Step(byte ticks)
    {
      // Count ticks and then..
      // OAM Access?
      // Do Line Magics?
      // H-Blank?
      // V-Blank?
    }
  }
}
