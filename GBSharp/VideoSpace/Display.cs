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
    private int totalTileCountX = 32;
    private int totalTileCountY = 32;
    private int screenTileCountX = 20;
    private int screenTileCountY = 18;
    private int bytesPerTile = 16;
    private int pixelPerTileX = 8;
    private int pixelPerTileY = 8;
    private int bytesPerPixel = 4;
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

    /// <summary>
    /// Retreives a the contents on a tile depending on the coordinates and the accessing methods.
    /// </summary>
    /// <param name="tileX">The x coord for the tile</param>
    /// <param name="tileY">The y coord for the tile</param>
    /// <param name="LCDBit3">
    /// Whether the LCDC Register (0xFF40) Bit 3 is enabled.
    /// Determines what tilemap (where the tile indexes are) is used:
    /// 0: 0x9800 - 0x9BFF
    /// 1: 0x9C00 - 0x9FFF
    /// </param>
    /// <param name="LCDBit4">
    /// Whether the LCDC Register (0xFF40) Bit 3 is enabled.
    /// Determines the base address for the actual tiles and the
    /// accessing method (interpretation of the byte tile index retreived from the tilemap).
    /// 0: 0x8800 - 0x97FF | signed access
    /// 1: 0x8000 - 0x8FFF | unsigned access
    /// </param>
    /// <param name="wrap">Whether the x, y tile coordinates should wrap or be clipped</param>
    /// <returns>A byte[] with the 16 bytes that create a tile</returns>
    internal byte[] GetTileData(int tileX, int tileY, bool LCDBit3, bool LCDBit4, bool wrap)
    {
      ushort tileBaseAddress = (ushort)(LCDBit4 ? 0x8000 : 0x8800);
      ushort tileMapBaseAddress = (ushort)(LCDBit3 ? 0x9C00 : 0x9800);

      if(wrap)
      {
        tileX %= totalTileCountX;
        tileY %= totalTileCountY;
      }
      else
      {
        // TODO(Cristian): See if clipping is what we want
        if(tileX >= totalTileCountX) { tileX = totalTileCountX - 1; }
        if(tileY >= totalTileCountY) { tileY = totalTileCountY - 1; }
      }

      // We obtain the correct tile index
      int tileIndex;
      if(LCDBit4)
      {
        tileIndex = memory.Read((ushort)(tileMapBaseAddress + totalTileCountX * tileY + tileX));
      }
      else
      {
        unchecked
        {
          tileIndex = (sbyte)memory.Read((ushort)(tileMapBaseAddress + totalTileCountX * tileY + tileX));
        }
      }
      
      // We obtain the tile memory
      byte[] result = new byte[bytesPerTile];
      for(int i = 0; i < bytesPerTile; i++)
      {
        result[i] = memory.Read((ushort)(tileBaseAddress + bytesPerTile * tileIndex + i));
      }

      return result;
    }

    internal void DrawTile(BitmapData bmd, byte[] tileData, int tileX, int tileY)
    {
      unsafe
      {
        // We iterate for the actual bytes
        for (int j = 0; j < 16; j += 2)
        {
          byte* row = (byte*)bmd.Scan0;
          row += ((pixelPerTileY * tileY + (j / 2)) * bmd.Stride);
          for (int i = 0; i < 8; i++)
          {
            int up = (tileData[j] >> i) & 1;
            int down = (tileData[j + 1] >> i) & 1;

            int index = 2 * up + down;
            byte color = (byte)(84 * index);

            byte* offset = row + (pixelPerTileX * tileX + i) * bytesPerPixel;

            offset[0] = color;
            offset[1] = color;
            offset[2] = color;
            offset[3] = 0;
          }
        }
      }
    }
    
    internal void DrawRectangle(BitmapData bmd, int initialTileX, int initialTileY, int tileWidth, int tileHeight, uint color)
    {
      int pixelsPerTileX = 8;
      int pixelsPerTileY = 8;

      unsafe
      {
        for(int iterTileY = 0; iterTileY < tileHeight; iterTileY++)
        {
          int tileY = (iterTileY + initialTileY) % totalTileCountY;
          for(int iterTileX = 0; iterTileX < tileWidth; iterTileX++)
          {
            int tileX = (iterTileX + initialTileX) % totalTileCountX;

            byte* begin = (byte*)bmd.Scan0 +
                            tileY * pixelsPerTileY * bmd.Stride +
                            tileX * pixelsPerTileX * bytesPerPixel;

            // We render the square
            if(iterTileX == 0)
            {
              byte* cursor = begin;
              for(int y = 0; y < pixelsPerTileY; y++)
              {
                ((uint*)cursor)[0] = color;
                cursor += bmd.Stride;
              }
            }
            else if(iterTileX == screenTileCountX - 1)
            {
              byte* cursor = begin + pixelsPerTileX * bytesPerPixel;
              for (int y = 0; y < pixelsPerTileY; y++)
              {
                ((uint*)cursor)[0] = color;
                cursor += bmd.Stride;
              }
            }

            if(iterTileY == 0)
            {
              byte* cursor = begin;
              for(int x = 0; x < pixelsPerTileX; x++)
              {
                ((uint*)cursor)[0] = color;
                cursor += bytesPerPixel;
              }
            }
            else if(iterTileY == screenTileCountY - 1)
            {
              byte* cursor = begin + pixelsPerTileY * bmd.Stride;
              for(int x = 0; x < pixelsPerTileX; x++)
              {
                ((uint*)cursor)[0] = color;
                cursor += bytesPerPixel;
              }
            }
          }
        }
      }
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

      int OX = 22; int OY = 23;

      // We update the whole screen
      BitmapData backgroundBmpData = background.LockBits(new Rectangle(0, 0, background.Width, background.Height), 
                                                ImageLockMode.WriteOnly, 
                                                PixelFormat.Format32bppRgb);

      // We render the complete tile map
      for (int tileY = 0; tileY < 32; tileY++)
      {
        for (int tileX = 0; tileX < 32; tileX++)
        {
          byte[] tileData = GetTileData(tileX, tileY, false, true, false);
          DrawTile(backgroundBmpData, tileData, tileX, tileY);
        }
      }
      DrawRectangle(backgroundBmpData, OX, OY, screenTileCountX, screenTileCountY, 0x00FF00FF);
      background.UnlockBits(backgroundBmpData);


      BitmapData bmpData = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height), 
                                           ImageLockMode.WriteOnly, 
                                           PixelFormat.Format32bppRgb);

      // We render the complete tile map
      for (int tileY = 0; tileY < 18; tileY++)
      {
        for (int tileX = 0; tileX < 20; tileX++)
        {
          byte[] tileData = GetTileData(tileX + OX, tileY + OY, false, true, true);
          DrawTile(bmpData, tileData, tileX, tileY);
        }
      }
      screen.UnlockBits(bmpData);
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
