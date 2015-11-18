using GBSharp.MemorySpace;
using System;

namespace GBSharp.VideoSpace
{
  internal static class DrawFuncs
  {
    /// <summary>
    /// Draw a tile into a bitmap. 
    /// IMPORTANT: presently it requires that the bitmap be the whole background (256x256 pixels)
    /// </summary>
    /// <param name="bmd">The bitmap data where to output the pixels</param>
    /// <param name="tileData">The 16 bytes that conform the 8x8 pixels</param>
    /// <param name="pX">x coord of the pixel where to start drawing the tile</param>
    /// <param name="pY">y coord of the pixel where to start drawing the tile</param>
    internal static void
    DrawTile(DisplayDefinition disDef, uint[] bitmapData, uint[] pixelBuffer,
             int stride, byte[] tileData,
             int pX, int pY, int maxPx, int maxPy)
    {
      // We iterate for the actual bytes
      for (int j = 0; j < tileData.Length; j += 2)
      {
        int pixelY = pY + (j / 2);
        if (pixelY < 0) { continue; }
        if (pixelY >= maxPy) { break; } // We can continue no further

        int index = pixelY * stride; // Only add every 2 bytes
        DisFuncs.GetPixelsFromTileBytes(ref pixelBuffer,
                                        disDef.tilePallete,
                                        disDef.pixelPerTileX,
                                        tileData[j], tileData[j + 1]);
        for (int i = 0; i < 8; i++)
        {
          int pixelX = pX + i;
          if (pixelX < 0) { continue; }
          if (pixelX >= maxPx) { break; }
          int pIndex = index + pixelX;
          bitmapData[pIndex] = pixelBuffer[i];
        }
      }
    }

    internal static void
    DrawTransparency(DisplayDefinition disDef, uint[] bitmapData, int stride,
                    int minX, int minY, int maxX, int maxY)
    {
      uint[] colors = { 0xF0F0F0F0, 0xF0CDCDCD };
      int squareSize = 5;
      for (int y = minY; y < maxY; y++)
      {
        int rowIndex = y * stride;
        for (int x = minX; x < maxX; x++)
        {
          int sX = x / squareSize;
          int sY = y / squareSize;
          int colorIndex = (sX + (sY % 2)) % 2;
          int pIndex = rowIndex + x;
          bitmapData[pIndex] = colors[colorIndex];
        }
      }
    }

    internal static void
    DrawRectangle(DisplayDefinition disDef, uint[] bitmapData, int stride,
                  int rX, int rY, int rWidth, int rHeight,
                  uint color, bool fill = false)
    {
      for (int y = 0; y < rHeight; y++)
      {
        for (int x = 0; x < rWidth; x++)
        {
          int pX = (rX + x) % disDef.framePixelCountX;
          int pY = (rY + y) % disDef.framePixelCountY;
          if (fill ||
             x == 0 || x == (rWidth - 1) ||
             y == 0 || y == (rHeight - 1))
          {
            int index = pY * stride + pX;
            bitmapData[index] = color;
          }
        }
      }
    }

    internal static void
    DrawLine(DisplayDefinition disDef, uint[] bitmapData, int stride,
             uint[] rowPixels,
             int targetX, int targetY, 
             int rowStart, int rowSpan,
             bool CopyZeroPixels = false,
             bool wrap = false)
    {
      // We obtain the data 
      int baseIndex = targetY * stride + targetX;
      // NOTE(Cristian): rowMax is included
      for (int i = 0; i < stride; i++)
      {
        // NOTE(Cristian): Sometimes the window is 7 pixels to the left (in the case of the windows)
        //                 We must draw on those cases
        if (targetX < 0)
        {
          targetX++;
          continue;
        }

        int rowIndex = rowStart + i;
        if (rowIndex >= rowSpan)
        {
          if(wrap)
          {
            rowIndex %= rowSpan; 
          }
          else
          {
            break;
          }
        }
        uint pixel = rowPixels[rowIndex];
        if (!CopyZeroPixels && pixel == 0) { continue; }
        bitmapData[baseIndex + i] = rowPixels[rowIndex];
      }
    }
  }
}
