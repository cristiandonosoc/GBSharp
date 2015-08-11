using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

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
      this.memory = memory;
      screen = new Bitmap(screenWidth, screenHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      background = new Bitmap(backgroundWidth, backgroundHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      UpdateScreen();
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

      // We update the whole screen
      BitmapData bmpData = background.LockBits(new Rectangle(0, 0, background.Width, background.Height), 
                                               ImageLockMode.WriteOnly, 
                                               PixelFormat.Format32bppRgb);
      unsafe
      {
        int pixelSize = 4;
        for (int tileY = 0; tileY < 32; tileY++)
        {
          for (int tileX = 0; tileX < 32; tileX++)
          {
            int t = 256 * tileY + tileX;
            sbyte tileIndex = (sbyte)memory.Read((ushort)(0x9800 + t));

            byte[] tile = new byte[16];
            for (uint i = 0; i < 16; i++)
            {
              tile[i] = memory.Read((ushort)(0x8800 + tileIndex));
            }

            int a = 2;
            // We iterate for the actual bytes
            for (int j = 0; j < 16; j += 2)
            {
              byte* row = (byte*)bmpData.Scan0 + ((8*tileY + (j / 2)) * bmpData.Stride);
              for (int i = 0; i < 8; i++)
              {
                int up = (tile[j] >> i) & 1;
                int down = (tile[j + 1] >> i) & 1;

                int index = 2 * up + down;
                byte color = (byte)(84 * index);

                row[(8 * tileX + i) * pixelSize] = color ;
                row[(8 * tileX + i) * pixelSize + 1] = color;
                row[(8 * tileX + i) * pixelSize + 2] = color;
                row[(8 * tileX + i) * pixelSize + 3] = 0;
              }
            }
          }
        }
      }

      background.UnlockBits(bmpData);
      background.Save("test.bmp");


      // We interpret the pixels
      // NOTE(Cristian): We use uint for the value, when byte would have
      // sufficed (and even make the code a little slimmer). But this permits
      // arbitrary sized screens which might prove useful while debugging.

      // TODO(Cristian): initialX and initialY should be defined by the
      // SCX and SCY registers respectively
      ushort initialX = 30;
      ushort initialY = 30;
      ushort screenWidth = 32;
      ushort screenHeight = 32;
      ushort displayWidth = 20;
      ushort displayHeight = 18;
      ushort currentX = initialX;   // Helper X variable to track overflow
      ushort currentY = initialY;   // Helper Y variable to track overflow

      ushort displayIndex = 0;      // The "pointer" to the actual pixel
      ushort firstOfRow = 0;        // To help the pointer go back to the start at overflow
      for (ushort y = 0; y < displayHeight; y++)
      {
        // We move the index to the start of the display row
        firstOfRow = (ushort)(currentY * screenHeight);
        currentX = initialX;
        displayIndex = (ushort)((currentY * screenHeight) + currentX);

        for (ushort x = 0; x < displayWidth; x++)
        {
          // We read the memory
          // TODO(Cristian): Actually read the memory
          //Console.Out.WriteLine("{0}: Accessed: {1} (X: {2}, Y: {3})",
          //  displayIndex, currentX, currentY);

          // We load the memory
          byte tileIndex = memory.Read(displayIndex);
          for (uint i = 0; i < 8; i++)
          {
            for(uint j = 0; j < 8; j++)
            {

            }
          }

          displayIndex++;
          currentX++;
          if(currentX >= screenWidth)
          {
            currentX = 0;
            displayIndex = firstOfRow;
          }
        }

        //Console.Out.WriteLine("ROW");

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
