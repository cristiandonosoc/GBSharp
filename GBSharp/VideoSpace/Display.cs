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
            int up = (tileData[j] >> (7 - i)) & 1;
            int down = (tileData[j + 1] >> (7 - i)) & 1;

            int index = 2 * up + down;

            uint color = 0x00FFFFFF;
            if(index == 1) { color = 0x00BBBBBB; }
            if(index == 2) { color = 0x00666666; }
            if(index == 3) { color = 0x00000000; }

            byte* offset = row + (pixelPerTileX * tileX + i) * bytesPerPixel;
            uint* cPtr = (uint*)offset;
            cPtr[0] = color;
          }
        }
      }
    }
    
    internal void DrawRectangle(BitmapData bmd, int rX, int rY, int rWidth, int rHeight, uint color)
    {
      unsafe
      {
        for(int y = 0; y < rHeight; y++)
        {
          for (int x = 0; x < rWidth; x++)
          {
            int pX = (rX + x) % 256;
            int pY = (rY + y) % 256;
            if(x == 0 || x == (rWidth - 1) ||
               y == 0 || y == (rHeight - 1))
            {
              byte* p = (byte*)bmd.Scan0 + pY * bmd.Stride + 4*pX;
              ((uint*)p)[0] = color;
            }
          }
        }
      }
    }

    internal void UpdateScreen()
    {
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

      // We render the complete tile map
      for (int tileY = 0; tileY < 32; tileY++)
      {
        for (int tileX = 0; tileX < 32; tileX++)
        {
          byte[] tileData = GetTileData(tileX, tileY, LCDBit3, LCDBit4, true);
          DrawTile(backgroundBmpData, tileData, tileX, tileY);
        }
      }
      DrawRectangle(backgroundBmpData, SCX, SCY, 160, 144, 0x00FF00FF);


      BitmapData bmpData = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height),
                                           ImageLockMode.WriteOnly,
                                           PixelFormat.Format32bppRgb);

      // We copy the information from the background tile to the effective screen
      // NOTE(Cristian): The screen is 160x144
      int screenCountX = 160;
      int screenCountY = 144;
      for (int y = 0; y < screenCountY; y++)
      {
        for (int x = 0; x < screenCountX; x++)
        {
          int pX = (x + SCX) % backgroundWidth;
          int pY = (y + SCY) % backgroundHeight;

          unsafe
          {
            byte* bP = (byte*)backgroundBmpData.Scan0 + pY * backgroundBmpData.Stride + pX;
            byte* sP = (byte*)bmpData.Scan0 + y * bmpData.Stride + x;
            sP[0] = bP[0];
          }
        }
      }
      background.UnlockBits(backgroundBmpData);
      screen.UnlockBits(bmpData);
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
