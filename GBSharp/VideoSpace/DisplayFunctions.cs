using GBSharp.MemorySpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GBSharp.VideoSpace
{
  internal static class DisplayFunctions
  {

    internal static BitmapData 
    LockBitmap(Bitmap bmp, ImageLockMode lockMode, PixelFormat pixelFormat)
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
    internal static byte[] 
    GetTileData(DisplayDefinition disDef, Memory memory, 
                int tileX, int tileY, bool LCDBit3, bool LCDBit4, bool wrap)
    {

      if(wrap)
      {
        tileX %= disDef.frameTileCountX;
        tileY %= disDef.screenTileCountY;
      }
      else
      {
        // TODO(Cristian): See if clipping is what we want
        if(tileX >= disDef.frameTileCountX) { tileX = disDef.frameTileCountX - 1; }
        if(tileY >= disDef.screenTileCountY) { tileY = disDef.screenTileCountY - 1; }
      }

      ushort tileMapBaseAddress = GetTileMapBaseAddress(LCDBit3);
      ushort tileBaseAddress = GetTileBaseAddress(LCDBit4);
      int tileOffset = GetTileOffset(disDef, memory, tileMapBaseAddress, LCDBit4, tileX, tileY);

      // We obtain the correct tile index
      byte[] result = GetTileData(disDef, memory, tileBaseAddress, tileOffset);
      return result;
    }

    internal static byte[]
    GetTileData(DisplayDefinition disDef, Memory memory, int tileBaseAddress, int tileOffset)
    {
      // We obtain the tile memory
      byte[] result = new byte[disDef.bytesPerTile];
      result = memory.LowLevelArrayRead(
        (ushort)(tileBaseAddress + (disDef.bytesPerTile * tileOffset)),
        disDef.bytesPerTile);
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
    internal static uint[] 
    GetRowPixels(DisplayDefinition disDef, Memory memory,
                 int row, bool LCDBit3, bool LCDBit4)
    {
      // We determine the y tile
      int tileY = row / disDef.pixelPerTileY;
      int tileRemainder = row % disDef.pixelPerTileY;

      ushort tileMapBaseAddress = GetTileMapBaseAddress(LCDBit3);
      ushort tileBaseAddress = GetTileBaseAddress(LCDBit4);

      uint[] pixels = new uint[disDef.framePixelCountX];
      for(int tileX = 0; tileX < disDef.frameTileCountX; tileX++)
      {
        // We obtain the correct tile index
        int tileOffset = GetTileOffset(disDef, memory, tileMapBaseAddress, LCDBit4, tileX, tileY);

        // We obtain both pixels
        int currentTileBaseAddress = tileBaseAddress + disDef.bytesPerTile * tileOffset;
        byte top = memory.LowLevelRead((ushort)(currentTileBaseAddress + 2 * tileRemainder));
        byte bottom = memory.LowLevelRead((ushort)(currentTileBaseAddress + 2 * tileRemainder + 1));

        uint[] tilePixels = GetPixelsFromTileBytes(disDef, top, bottom);
        int currentTileIndex = tileX * disDef.pixelPerTileX;
        for (int i = 0; i < disDef.pixelPerTileX; i++)
        {
          pixels[currentTileIndex + i] = tilePixels[i];
        }
      }

      return pixels;
    }

    internal static int
    GetTileOffset(DisplayDefinition disDef, Memory memory, 
                  ushort tileMapBaseAddress, bool LCDBit4,
                  int tileX, int tileY)
    {
      int tileOffset;
      if(LCDBit4)
      {
        tileOffset = memory.LowLevelRead((ushort)(tileMapBaseAddress + 
                                                 (disDef.frameTileCountX * tileY) + 
                                                 tileX));
      }
      else
      {
        unchecked
        {
          byte t = memory.LowLevelRead((ushort)(tileMapBaseAddress + 
                                                (disDef.frameTileCountX * tileY) + 
                                                tileX));
          sbyte tR = (sbyte)t;
          tileOffset = tR;
        }
      }

      return tileOffset;
    }


    internal static ushort
    GetTileBaseAddress(bool LCDBit4)
    {
      ushort result = (ushort)(LCDBit4 ? 0x8000 : 0x9000);
      return result;
    }
    
    internal static ushort
    GetTileMapBaseAddress(bool LCDBit3)
    {
      ushort result = (ushort)(LCDBit3 ? 0x9C00 : 0x9800);
      return result;
    }


    internal static uint[] 
    GetPixelsFromTileBytes(DisplayDefinition disDef, byte top, byte bottom)
    {
      uint[] pixels = new uint[disDef.pixelPerTileX];
      for(int i = 0; i < disDef.pixelPerTileX; i++)
      {
        int up = (top >> (7 - i)) & 1;
        int down = (bottom >> (7 - i)) & 1;

        int index = 2 * up + down;
        // TODO(Cristian): Obtain color from a palette!
        uint color = 0xFFFFFFFF;
        if (index == 1) { color = 0xFF666666; }
        if (index == 2) { color = 0xFFBBBBBB; }
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
    internal static void
    DrawTile(DisplayDefinition disDef, BitmapData bmd, byte[] tileData,
             int pX, int pY, int maxPx, int maxPy)
    {
      unsafe
      {
        int uintStride = bmd.Stride / disDef.bytesPerPixel;
        uint* start = (uint*)bmd.Scan0;

        // We iterate for the actual bytes
        for (int j = 0; j < 16; j += 2)
        {
          int pixelY = pY + (j / 2);
          if(pixelY < 0) { continue; }
          if(pixelY >= maxPy) { break; } // We can continue no further

          uint* row = start + pixelY * uintStride; // Only add every 2 bytes
          uint[] pixels = GetPixelsFromTileBytes(disDef, tileData[j], tileData[j + 1]);
          for (int i = 0; i < 8; i++)
          {
            int pixelX = pX + i;
            if(pixelX < 0) { continue; }
            if(pixelX >= maxPx) { break; }
            uint* cPtr = row + pixelX;
            cPtr[0] = pixels[i];
          }
        }
      }
    }

    internal static void 
    DrawTransparency(DisplayDefinition disDef, BitmapData bmd, int minX, int minY, int maxX, int maxY)
    {
      uint[] colors = { 0xF0F0F0F0, 0xF0CDCDCD };
      int squareSize = 7;
      unsafe
      {
        int uintStride = bmd.Stride / disDef.bytesPerPixel;
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
    
    internal static void 
    DrawRectangle(DisplayDefinition disDef, BitmapData bmd, 
                  int rX, int rY, int rWidth, int rHeight, uint color)
    {
      unsafe
      {
        int uintStride = bmd.Stride / disDef.bytesPerPixel;
        for(int y = 0; y < rHeight; y++)
        {
          for (int x = 0; x < rWidth; x++)
          {
            int pX = (rX + x) % disDef.framePixelCountX;
            int pY = (rY + y) % disDef.framePixelCountY;
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

    internal static void 
    DrawLine(DisplayDefinition disDef, BitmapData bmd, uint[] rowPixels,
             int targetX, int targetY, int rowStart, int rowSpan)
    {
      // We obtain the data 
      unsafe
      {
        uint* bmdPtr = (uint*)bmd.Scan0 + (targetY * bmd.Stride / disDef.bytesPerPixel) + targetX;
        // NOTE(Cristian): rowMax is included
        for(int i = 0; i < rowSpan; i++)
        {
          bmdPtr[i] = rowPixels[rowStart + i];
        }
      }
    }
  }
}
