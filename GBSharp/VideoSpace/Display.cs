using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GBSharp.VideoSpace
{
  class Display : IDisplay
  {
    public event Action RefreshScreen;

    private Memory memory;

    /// <summary>
    /// Pixel numbers of the display.
    /// With this it should be THEORETICALLY have displays
    /// of different sizes without too much hazzle.
    /// </summary>
    private int framePixelCountX = 256;
    private int framePixelCountY = 256;
    private int screenPixelCountX = 160;
    private int screenPixelCountY = 144;
    private int windowTileCountX = 32;
    private int windowTileCountY = 32;
    private int screenTileCountX = 20;
    private int screenTileCountY = 18;
    private int bytesPerTile = 16;
    private int pixelPerTileX = 8;
    private int pixelPerTileY = 8;
    private int bytesPerPixel = 4;

    PixelFormat pixelFormat;

    private Bitmap background;
    public Bitmap Background { get { return background; } }

    private Bitmap window;
    public Bitmap Window { get { return window; } }

    /// <summary>
    /// The final composed frame from with the screen is calculated.
    /// It's the conbination of the background, window and sprites.
    /// </summary>
    private Bitmap frame;
    public Bitmap Frame { get { return frame; } }

    /// <summary>
    /// The bitmap that represents the actual screen.
    /// It's a portial of the frame specified by the SCX and SCY registers.
    /// </summary>
    private Bitmap screen;
    public Bitmap Screen { get { return screen; } }


    /// <summary>
    /// Display constructor.
    /// </summary>
    /// <param name="interruptController">A reference to the interrupt controller.</param>
    /// <param name="Memory">A reference to the memory.</param>
    public Display(InterruptController interruptController, Memory memory)
    {

      this.memory = memory;
      pixelFormat = PixelFormat.Format32bppArgb;
      background = new Bitmap(framePixelCountX, framePixelCountY, pixelFormat);
      window = new Bitmap(screenPixelCountX, screenPixelCountY, pixelFormat);
      screen = new Bitmap(screenPixelCountX, screenPixelCountY, pixelFormat);
      frame = new Bitmap(framePixelCountX, framePixelCountY, pixelFormat);

      // TODO(Cristian): Remove this call eventually, when testing is not needed!
#if DEBUG
      UpdateScreen();
#endif
    }

    BitmapData LockBitmap(Bitmap bmp, ImageLockMode lockMode, PixelFormat pixelFormat)
    {
      BitmapData result = bmp.LockBits(
        new Rectangle(0, 0, bmp.Width, bmp.Height),
        lockMode, pixelFormat);
      return result;
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
        tileX %= windowTileCountX;
        tileY %= windowTileCountY;
      }
      else
      {
        // TODO(Cristian): See if clipping is what we want
        if(tileX >= windowTileCountX) { tileX = windowTileCountX - 1; }
        if(tileY >= windowTileCountY) { tileY = windowTileCountY - 1; }
      }

      // We obtain the correct tile index
      int tileIndex;
      if(LCDBit4)
      {
        tileIndex = memory.LowLevelRead((ushort)(tileMapBaseAddress + windowTileCountX * tileY + tileX));
      }
      else
      {
        unchecked
        {
          byte t = memory.LowLevelRead((ushort)(tileMapBaseAddress + windowTileCountX * tileY + tileX));
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

    /// <summary>
    /// Gets a row of pixels from the tilemap
    /// </summary>
    /// <param name="row">The row number to retreive [0, frameHeight)</param>
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
    /// <returns>An array with the pixels to show for that row (color already calculated)</returns>
    internal uint[] GetRowPixels(int row, bool LCDBit3, bool LCDBit4)
    {
      ushort tileBaseAddress = (ushort)(LCDBit4 ? 0x8000 : 0x9000);
      ushort tileMapBaseAddress = (ushort)(LCDBit3 ? 0x9C00 : 0x9800);

      // We determine the y tile
      int tileY = row / pixelPerTileY;
      int tileRemainder = row % pixelPerTileY;

      uint[] pixels = new uint[framePixelCountX];
      for(int tileX = 0; tileX < windowTileCountX; tileX++)
      {
        // We obtain the correct tile index
        int tileIndex;
        if (LCDBit4)
        {
          tileIndex = memory.LowLevelRead((ushort)(tileMapBaseAddress + windowTileCountX * tileY + tileX));
        }
        else
        {
          unchecked
          {
            byte t = memory.LowLevelRead((ushort)(tileMapBaseAddress + windowTileCountX * tileY + tileX));
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
        // TODO(Cristian): Obtain color from a palette!
        uint color = 0xFFFFFFFF;
        if (index == 1) { color = 0xFFBBBBBB; }
        if (index == 2) { color = 0xFF666666; }
        if (index == 3) { color = 0xFF000000; }
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
          uint[] pixels = GetPixelsFromTileBytes(tileData[j], tileData[j + 1]);
          for (int i = 0; i < 8; i++)
          {
            int pixelX = pX + i;
            if(pixelX >= maxX) { break; }
            uint* cPtr = row + pixelX;
            cPtr[i] = pixels[7 - i];
          }
        }
      }
    }

    internal void DrawTransparency(BitmapData bmd, int minX, int minY, int maxX, int maxY)
    {
      uint[] colors = { 0xF0FCFCFC, 0xF0CDCDCD };
      int squareSize = 7;
      unsafe
      {
        int uintStride = bmd.Stride / bytesPerPixel;
        uint* start = (uint*)bmd.Scan0;

        for(int y = minY; y < maxY; y++)
        {
          uint* rowStart = start + y * uintStride;
          for(int x = minX; x < maxX; x++)
          {
            int sX = x / squareSize;
            int sY = y / squareSize;
            int index = (sX + (sY % 2)) % 2;
            uint* pixel = rowStart + x;
            pixel[0] = colors[index];
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
            int pX = (rX + x) % framePixelCountX;
            int pY = (rY + y) % framePixelCountY;
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


      // *** BACKGROUND ***
      BitmapData backgroundBmpData = LockBitmap(background, ImageLockMode.ReadOnly, pixelFormat);
      for (int row = 0; row < framePixelCountY; row++)
      {
        uint[] rowPixels = GetRowPixels(row, LCDBit3, LCDBit4);
        DrawLine(backgroundBmpData, rowPixels, 0, row, 0, framePixelCountY);
      }
      background.UnlockBits(backgroundBmpData);

      // *** WINDOW ***
      int WX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.WX) + 30;
      int rWX = WX - 7; // The window pos is (WX - 7, WY)
      int WY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.WY) + 30;

      BitmapData windowBmpData = LockBitmap(window, ImageLockMode.WriteOnly, pixelFormat);
      DrawTransparency(windowBmpData, 0, 0, screenPixelCountX, WY);
      DrawTransparency(windowBmpData, 0, WY, rWX, screenPixelCountY);

      for (int row = 0; row < screenPixelCountY; row++)
      {
        if(row >= WY)
        {
          // The offset indexes represent that the window is drawn from it's beggining
          // at (WX, WY)
          uint[] rowPixels = GetRowPixels(row - WY, LCDBit3, LCDBit4);
          DrawLine(windowBmpData, rowPixels, rWX, row, 0, screenPixelCountX - rWX);
        }
      }
      window.UnlockBits(windowBmpData);

      // *** SPRITES ***
      // TODO(Cristian): Sprites!


      // *** SCREEN ***

      // We draw the SCREEN
      int SCX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCX);
      int SCY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCY);
      BitmapData screenBmpData = screen.LockBits(
        new Rectangle(0, 0, screen.Width, screen.Height),
        ImageLockMode.WriteOnly, pixelFormat);
      // Background Pass
      bool drawBackground = Utils.UtilFuncs.TestBit(lcdRegister, 0) != 0;
      if(drawBackground)
      {
        backgroundBmpData = LockBitmap(background, ImageLockMode.ReadOnly, pixelFormat);

        // We copy the information from the background tile to the effective screen
        int backgroundUintStride = backgroundBmpData.Stride / bytesPerPixel;
        int screenUintStride = screenBmpData.Stride / bytesPerPixel;
        for (int y = 0; y < screenPixelCountY; y++)
        {
          for (int x = 0; x < screenPixelCountX; x++)
          {
            int pX = (x + SCX) % framePixelCountX;
            int pY = (y + SCY) % framePixelCountY;

            unsafe
            {
              uint* bP = (uint*)backgroundBmpData.Scan0 + (pY * backgroundUintStride) + pX;
              uint* sP = (uint*)screenBmpData.Scan0 + (y * screenUintStride) + x;
              sP[0] = bP[0];
            }
          }
        }

        background.UnlockBits(backgroundBmpData);
      }

      bool drawWindow = Utils.UtilFuncs.TestBit(lcdRegister, 5) != 0;
      if(drawWindow)
      {
        windowBmpData = LockBitmap(window, ImageLockMode.ReadOnly, pixelFormat);

        int windowUintStrided = windowBmpData.Stride / bytesPerPixel;
        int screenUintStride = screenBmpData.Stride / bytesPerPixel;

        for (int y = 0; y < screenPixelCountY; y++)
        {
          for (int x = 0; x < screenPixelCountX; x++)
          {
            unsafe
            {
              uint* bP = (uint*)windowBmpData.Scan0 + (y * windowUintStrided) + x;
              if (((*bP) & 0xFF000000)  != 0) // We check if the pixel wasn't disabled
              {
                uint* sP = (uint*)screenBmpData.Scan0 + (y * screenUintStride) + x;
                sP[0] = bP[0];
              }
            }
          }
        }

        window.UnlockBits(windowBmpData);
      }

      // *** SCREEN RECTANGLE ***

      bool drawRectangle = true;
      if(drawRectangle)
      {
        backgroundBmpData = LockBitmap(background, ImageLockMode.WriteOnly, pixelFormat);
        DrawRectangle(backgroundBmpData, SCX, SCY, screenPixelCountX, screenPixelCountY, 0xFFFF8822);
        background.UnlockBits(backgroundBmpData);
      }

      screen.UnlockBits(screenBmpData);
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
