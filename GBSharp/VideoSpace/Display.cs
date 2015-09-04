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
    public event Action RefreshScreen;

    private Bitmap screen;
    private Memory memory;

    private int backgroundPixelCountX = 256;
    private int backgroundPixelCountY = 256;
    private int screenPixelCountX = 160;
    private int screenPixelCountY = 144;
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
      screen = new Bitmap(screenPixelCountX, screenPixelCountY, 
                          System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      background = new Bitmap(backgroundPixelCountX, backgroundPixelCountY, 
                              System.Drawing.Imaging.PixelFormat.Format32bppRgb);

      // TODO(Cristian): Remove this call eventually, when testing is not needed!
#if DEBUG
      UpdateScreen();
#endif
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
      ushort tileBaseAddress = (ushort)(LCDBit4 ? 0x8000 : 0x9000);
      ushort tileMapBaseAddress = (ushort)(!LCDBit3 ? 0x9C00 : 0x9800);

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
        tileIndex = memory.LowLevelRead((ushort)(tileMapBaseAddress + totalTileCountX * tileY + tileX));
      }
      else
      {
        unchecked
        {
          byte t = memory.LowLevelRead((ushort)(tileMapBaseAddress + totalTileCountX * tileY + tileX));
          sbyte tR = (sbyte)t;
          tileIndex = tR;
        }
      }
      
      // We obtain the tile memory
      byte[] result = new byte[bytesPerTile];
      for(int i = 0; i < bytesPerTile; i++)
      {
        result[i] = memory.LowLevelRead((ushort)(tileBaseAddress + bytesPerTile * tileIndex + i));
      }

      return result;
    }

    internal uint[] GetRowPixels(int row, bool LCDBit3, bool LCDBit4)
    {
      ushort tileBaseAddress = (ushort)(LCDBit4 ? 0x8000 : 0x9000);
      ushort tileMapBaseAddress = (ushort)(!LCDBit3 ? 0x9C00 : 0x9800);

      // We determine the y tile
      int tileY = row / pixelPerTileY;
      int tileRemainder = row % pixelPerTileY;

      uint[] pixels = new uint[backgroundPixelCountX];
      for(int tileX = 0; tileX < totalTileCountX; tileX++)
      {
        // We obtain the correct tile index
        int tileIndex;
        if (LCDBit4)
        {
          tileIndex = memory.LowLevelRead((ushort)(tileMapBaseAddress + totalTileCountX * tileY + tileX));
        }
        else
        {
          unchecked
          {
            byte t = memory.LowLevelRead((ushort)(tileMapBaseAddress + totalTileCountX * tileY + tileX));
            sbyte tR = (sbyte)t;
            tileIndex = tR;
          }
        }

        // We obtain both pixels
        int currentTileBaseAddress = tileBaseAddress + bytesPerTile * tileIndex;
        byte top = memory.LowLevelRead((ushort)(currentTileBaseAddress + 2 * tileRemainder));
        byte bottom = memory.LowLevelRead((ushort)(currentTileBaseAddress + 2 * tileRemainder + 1));

        uint[] tilePixels = GetPixelsFromTileBytes(top, bottom);
        int currentTileIndex = tileX * pixelPerTileX;
        for (int i = 0; i < pixelPerTileX; i++)
        {
          pixels[currentTileIndex + i] = tilePixels[i];
        }
      }

      return pixels;
    }


    internal uint[] GetPixelsFromTileBytes(byte top, byte bottom)
    {
      uint[] pixels = new uint[pixelPerTileX];
      for(int i = 0; i < pixelPerTileX; i++)
      {
        int up = (top >> (7 - i)) & 1;
        int down = (bottom >> (7 - i)) & 1;

        int index = 2 * up + down;
        uint color = 0x00FFFFFF;
        if (index == 1) { color = 0x00BBBBBB; }
        if (index == 2) { color = 0x00666666; }
        if (index == 3) { color = 0x00000000; }
        pixels[i] = color;
      }

      return pixels;
    }

    /// <summary>
    /// Draw a tile into a bitmap. 
    /// IMPORTANT: presently it requires that the bitmap be the whole background (256x256 pixels)
    /// </summary>
    /// <param name="bmd">The bitmap data where to output the pixels</param>
    /// <param name="tileData">The 16 bytes that conform the 8x8 pixels</param>
    /// <param name="pX">x coord of the pixel where to start drawing the tile</param>
    /// <param name="pY">y coord of the pixel where to start drawing the tile</param>
    internal void DrawTile(BitmapData bmd, byte[] tileData, int pX, int pY,
      int maxX = 256, int maxY = 256)
    {
      // TODO(Cristian): Remove this assertions
      if ((pY + 7) >= 256)
      {
        throw new ArgumentOutOfRangeException("Y pixel too high");
      }
      else if ((pY == 255) && ((pX + 7) >= 256))
      {
        throw new ArgumentOutOfRangeException("X pixel too high in last line");
      }
      unsafe
      {
        int uintStride = bmd.Stride / bytesPerPixel;
        uint* start = (uint*)bmd.Scan0;

        // We iterate for the actual bytes
        for (int j = 0; j < 16; j += 2)
        {
          int pixelY = pY + (j / 2);
          if(pixelY >= maxY) { break; }

          uint* row = start + pixelY * uintStride; // Only add every 2 bytes
          for (int i = 0; i < 8; i++)
          {

            int pixelX = pX + i;
            if(pixelX >= maxX) { break; }
            uint* cPtr = row + pixelX;

            int up = (tileData[j] >> (7 - i)) & 1;
            int down = (tileData[j + 1] >> (7 - i)) & 1;

            int index = 2 * up + down;

            uint color = 0x00FFFFFF;
            if(index == 1) { color = 0x00BBBBBB; }
            if(index == 2) { color = 0x00666666; }
            if(index == 3) { color = 0x00000000; }

            cPtr[0] = color;
          }
        }
      }
    }
    
    internal void DrawRectangle(BitmapData bmd, int rX, int rY, int rWidth, int rHeight, uint color)
    {
      unsafe
      {
        int uintStride = bmd.Stride / bytesPerPixel;
        for(int y = 0; y < rHeight; y++)
        {
          for (int x = 0; x < rWidth; x++)
          {
            int pX = (rX + x) % 256;
            int pY = (rY + y) % 256;
            if(x == 0 || x == (rWidth - 1) ||
               y == 0 || y == (rHeight - 1))
            {
              uint* p = (uint*)bmd.Scan0 + pY * uintStride + pX;
              p[0] = color;
            }
          }
        }
      }
    }

    internal void DrawLine(BitmapData bmd, uint[] rowPixels,
                           int targetX, int targetY,
                           int rowStart, int rowSpan)
    {
      // We obtain the data 
      unsafe
      {
        uint* bmdPtr = (uint*)bmd.Scan0 + (targetY * bmd.Stride / bytesPerPixel) + targetX;
        // NOTE(Cristian): rowMax is included
        for(int i = 0; i < rowSpan; i++)
        {
          bmdPtr[i] = rowPixels[rowStart + i];
        }
      }
    }

    internal void UpdateScreen()
    {
      // TODO(Cristian): Line-based drawing!!!
      byte lcdRegister = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.LCDC);

      bool LCDBit3 = Utils.UtilFuncs.TestBit(lcdRegister, 3) != 0;
      bool LCDBit4 = Utils.UtilFuncs.TestBit(lcdRegister, 4) != 0;

      int SCX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCX);
      int SCY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCY);

      // We update the whole screen
      BitmapData backgroundBmpData = background.LockBits(
        new Rectangle(0, 0, background.Width, background.Height),
        ImageLockMode.WriteOnly,
        PixelFormat.Format32bppRgb);

      for(int row = 0; row < backgroundPixelCountY; row++)
      {

        uint[] rowPixels = GetRowPixels(row, LCDBit3, LCDBit4);
        DrawLine(backgroundBmpData, rowPixels, 0, row, 0, backgroundPixelCountX);
      }

      // We draw the background
      // TODO(Cristian): Probably join the loops
      int WDX = 0;
      int WDY = 0;
      for (int row = WDY; row < screenPixelCountY; row++)
      {
        // TODO(Cristian): Remove this negation to LCDBit4!!!!
        //                 It's a bug I'm using for dev only!
        uint[] rowPixels = GetRowPixels(row, LCDBit3, !LCDBit4);
        DrawLine(backgroundBmpData, rowPixels, WDX, row, 0, screenPixelCountX - WDX);
      }

      // We draw the SCREEN
      BitmapData bmpData = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height),
                                           ImageLockMode.WriteOnly,
                                           PixelFormat.Format32bppRgb);

      // We copy the information from the background tile to the effective screen
      // NOTE(Cristian): The screen is 160x144
      // NOTE(Cristian): We precalculate the stride into the uint boundary to make easier
      //                 copying the pixels.
      int backgroundUintStride = backgroundBmpData.Stride / bytesPerPixel;
      int bmpUintStride = bmpData.Stride / bytesPerPixel;
      for (int y = 0; y < screenPixelCountY; y++)
      {
        for (int x = 0; x < screenPixelCountX; x++)
        {
          int pX = (x + SCX) % backgroundPixelCountX;
          int pY = (y + SCY) % backgroundPixelCountY;

          unsafe
          {
            uint* bP = (uint*)backgroundBmpData.Scan0 + (pY * backgroundUintStride) + pX;
            uint* sP = (uint*)bmpData.Scan0 + (y * bmpUintStride) + x;
            sP[0] = bP[0];
          }
        }
      }

      screen.UnlockBits(bmpData);

      // We draw the screen boundaries
      DrawRectangle(backgroundBmpData, SCX, SCY, 160, 144, 0x00FF00FF);
      background.UnlockBits(backgroundBmpData);
    }


    private const int screenStep = 96905; // Aprox. ~16.6687 ms
    private int screenSum = 0;
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

      screenSum += ticks;
      if(screenSum > screenStep)
      {
        screenSum %= screenStep;
        UpdateScreen();
        if (RefreshScreen != null)
        {
          RefreshScreen();
        }
      } 
    }
  }
}
