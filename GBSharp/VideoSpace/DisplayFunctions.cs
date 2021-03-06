﻿using GBSharp.MemorySpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GBSharp.VideoSpace
{
  internal static class DisFuncs
  {

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
                int tileX, int tileY,
                bool LCDCBit2, bool LCDBit3, bool LCDBit4,
                bool wrap)
    {

      if (wrap)
      {
        tileX %= disDef.FrameTileCountX;
        tileY %= disDef.ScreenTileCountY;
      }
      else
      {
        if (tileX >= disDef.FrameTileCountX) { tileX = disDef.FrameTileCountX - 1; }
        if (tileY >= disDef.ScreenTileCountY) { tileY = disDef.ScreenTileCountY - 1; }
      }

      ushort tileMapBaseAddress = GetTileMapBaseAddress(LCDBit3);
      ushort tileBaseAddress = GetTileBaseAddress(LCDBit4);
      int tileOffset = GetTileOffset(disDef, memory, tileMapBaseAddress, LCDBit4, tileX, tileY);

      // We obtain the correct tile index
      byte[] result = GetTileData(disDef, memory, tileBaseAddress, tileOffset, LCDCBit2);
      return result;
    }

    static byte[] flipLookup = new byte[16]
    {
      0x0, 0x8, 0x4, 0xC, 0x2, 0xA, 0x6, 0xE,
      0x1, 0x9, 0x5, 0xD, 0x3, 0xB, 0x7, 0xF
    };

    internal static byte[]
    GetTileData(DisplayDefinition disDef, Memory memory,
                int tileBaseAddress, int tileOffset,
                bool LCDCBit2,
                bool flipX = false, bool flipY = false)
    {
      int tileLength = 16;
      int spriteLength = disDef.BytesPerTileShort;
      if (LCDCBit2) { spriteLength = disDef.BytesPerTileLong; }

      // We obtain the tile memory
      byte[] data = memory.LowLevelArrayRead(
        (ushort)(tileBaseAddress + (tileLength * tileOffset)),
        spriteLength);

      if (flipX)
      {
        for (int i = 0; i < spriteLength; ++i)
        {
          byte d = data[i];
          byte r = (byte)((flipLookup[d & 0x0F] << 4) | flipLookup[d >> 4]);
          data[i] = r;
        }
      }

      if (flipY)
      {
        // NOTE(Cristian): We have to flip them in pairs, because
        //                 otherwise the colors change!
        for (int i = 0; i < spriteLength / 4; ++i)
        {
          byte d0 = data[2 * i];
          byte d1 = data[2 * i + 1];

          int index = (spriteLength - 1) - 2 * i;
          data[2 * i] = data[index - 1];
          data[2 * i + 1] = data[index];

          data[index - 1] = d0;
          data[index] = d1;
        }
      }

      return data;
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
    internal static void
    GetRowPixels(ref uint[] frameLineBuffer,
                 short[] pixelLookupTable,
                 DisplayDefinition disDef, Memory memory,
                 uint[] pixelBuffer, 
                 int row, bool LCDBit3, bool LCDBit4)
    {
      // We determine the y tile
      int tileY = row / disDef.PixelPerTileY;
      int tileRemainder = row % disDef.PixelPerTileY;

      ushort tileMapBaseAddress = GetTileMapBaseAddress(LCDBit3);
      ushort tileBaseAddress = GetTileBaseAddress(LCDBit4);

      for (int tileX = 0; tileX < disDef.FrameTileCountX; tileX++)
      {
        // We obtain the correct tile index
        int tileOffset = GetTileOffset(disDef, memory, tileMapBaseAddress, LCDBit4, tileX, tileY);

        // We obtain both pixels
        int currentTileBaseAddress = tileBaseAddress + disDef.BytesPerTileShort * tileOffset;
        byte top = memory.LowLevelRead((ushort)(currentTileBaseAddress + 2 * tileRemainder));
        byte bottom = memory.LowLevelRead((ushort)(currentTileBaseAddress + 2 * tileRemainder + 1));

        GetPixelsFromTileBytes(pixelLookupTable,
                               ref pixelBuffer,
                               disDef.TilePallete,
                               disDef.PixelPerTileX,
                               top, bottom);
        int currentTileIndex = tileX * disDef.PixelPerTileX;
        for (int i = 0; i < disDef.PixelPerTileX; i++)
        {
          frameLineBuffer[currentTileIndex + i] = pixelBuffer[i];
        }
      }
    }

    internal static void
    GetPixelRowFromBitmap(ref uint[] frameLineBuffer, 
                          DisplayDefinition disDef, uint[] bitmapData, int row, int stride)
    {
      int index = row * stride;
      for (int x = 0; x < stride; ++x)
      {
        frameLineBuffer[x] = bitmapData[index++];
      }
    }

    /// <summary>
    /// Gets the OAMs for the a certain row
    /// </summary>
    /// <param name="spriteOAMs"></param>
    /// <param name="row"></param>
    /// <param name="LCDCBit2"></param>
    /// <returns></returns>
    internal static int
    GetScanLineOAMs(DisplayDefinition disDef, OAM[] spriteOAMs, int row, bool LCDCBit2)
    {
      int spriteSize = disDef.BytesPerTileShort / 2;
      if (LCDCBit2) { spriteSize = disDef.BytesPerTileLong / 2; }

      // Then we select the 10 that correspond
      int scanLineSize = 0;
      int maxScanLineSize = 10;
      foreach (OAM oam in spriteOAMs)
      {
        int y = oam.Y - 16;
        if ((y <= row) && (row < (y + spriteSize)))
        {
          scanLineOAMs[scanLineSize++] = oam;
          if (scanLineSize == maxScanLineSize) { break; }
        }
      }

      return scanLineSize;
    }


    static OAM[] scanLineOAMs = new OAM[10];

    internal static void
    GetSpriteRowPixels(short[] pixelLookupTable,
                       DisplayDefinition disDef, Memory memory, OAM[] spriteOAMs,
                       uint[] targetPixels, uint[] pixelBuffer,
                       int row, bool LCDCBit2,
                       bool ignoreBackgroundPriority = false)
    {
      int scanLineSize = GetScanLineOAMs(disDef, spriteOAMs, row, LCDCBit2);

      // We obtain the pixels we want from it
      for (int oamIndex = scanLineSize - 1; oamIndex >= 0; --oamIndex)
      {
        OAM oam = scanLineOAMs[oamIndex];

        bool flipX = Utils.UtilFuncs.TestBit(oam.Flags, 5) != 0;
        bool flipY = Utils.UtilFuncs.TestBit(oam.Flags, 6) != 0;
        byte[] tilePixels = GetTileData(disDef, memory, 0x8000, oam.SpriteCode,
                                        LCDCBit2, flipX, flipY);

        int x = oam.X - 8;
        int y = row - (oam.Y - 16);

        uint[] spritePallete = (Utils.UtilFuncs.TestBit(oam.Flags, 4) == 0) ?
                                  disDef.SpritePallete0 : disDef.SpritePallete1;

        GetPixelsFromTileBytes(pixelLookupTable,
                               ref pixelBuffer,
                               spritePallete,
                               disDef.PixelPerTileX,
                               tilePixels[2 * y], tilePixels[2 * y + 1]);
        bool backgroundPriority = (Utils.UtilFuncs.TestBit(oam.Flags, 7) != 0);
        if (ignoreBackgroundPriority)
        {
          backgroundPriority = false;
        }
        for (int i = 0; i < 8; ++i)
        {
          int pX = x + i;
          if (pX < 0) { continue; }
          if (pX >= disDef.ScreenPixelCountX) { break; }
          uint color = pixelBuffer[i];
          if (color == 0) { continue; } // transparent pixel

          // NOTE(Cristian): If the BG priority bit is set, the sprite is hidden
          //                 on every color except tile color 0
          if (backgroundPriority)
          {
            if (targetPixels[pX] != disDef.TileColors[0]) { continue; }
          }
          targetPixels[pX] = color;
        }
      }
    }

    internal static int
    GetTileOffset(DisplayDefinition disDef, Memory memory,
                  ushort tileMapBaseAddress, bool LCDBit4,
                  int tileX, int tileY)
    {
      int tileOffset;
      if (LCDBit4)
      {
        tileOffset = memory.LowLevelRead((ushort)(tileMapBaseAddress +
                                                 (disDef.FrameTileCountX * tileY) +
                                                 tileX));
      }
      else
      {
        unchecked
        {
          byte t = memory.LowLevelRead((ushort)(tileMapBaseAddress +
                                                (disDef.FrameTileCountX * tileY) +
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

    internal static void
    SetupTilePallete(DisplayDefinition disDef, Memory memory)
    {
      // We extract the BGP (BG Pallete Data)
      byte bgp = memory.LowLevelRead((ushort)MMR.BGP);

      for (int color = 0; color < 4; ++color)
      {
        int down = (bgp >> (2 * color)) & 1;
        int up = (bgp >> (2 * color + 1)) & 1;
        int index = (up << 1) | down;
        disDef.TilePallete[color] = disDef.TileColors[index];
      }
    }

    internal static void
    SetupSpritePalletes(DisplayDefinition disDef, Memory memory, MMR pallete)
    {
      if (pallete == MMR.OBP0)
      {

        byte obp0 = memory.LowLevelRead((ushort)MMR.OBP0);
        disDef.SpritePallete0[0] = 0x00000000; // Sprite colors are trasparent
        for (int color = 1; color < 4; ++color)
        {
          int down = (obp0 >> (2 * color)) & 1;
          int up = (obp0 >> (2 * color + 1)) & 1;
          int index = (up << 1) | down;
          disDef.SpritePallete0[color] = disDef.SpriteColors[index];
        }
      }
      else if (pallete == MMR.OBP1)
      {
        byte obp1 = memory.LowLevelRead((ushort)MMR.OBP1);
        disDef.SpritePallete1[1] = 0x00000000; // Sprite colors are trasparent
        for (int color = 1; color < 4; ++color)
        {
          int down = (obp1 >> (2 * color)) & 1;
          int up = (obp1 >> (2 * color + 1)) & 1;
          int index = (up << 1) | down;
          disDef.SpritePallete1[color] = disDef.SpriteColors[index];
        }
      }
      else
      {
        throw new InvalidProgramException("Invalid pallete register given");
      }
    }


    /// <summary>
    /// Gets the color of a pixel from two bytes. Each index within the two bytes
    /// determine a 2-bit color pixel
    /// </summary>
    /// <param name="pixelBuffer">
    /// The output buffer. We pass it as ref because out demands an assignment 
    /// and we pass a already allocated to avoid allocating several times a frame</param>
    /// <param name="pallete">The current color pallete</param>
    /// <param name="pixelPerTileX">Amount of pixel per tiles (somewhat const actually)</param>
    /// <param name="top">The top row of bits</param>
    /// <param name="bottom">Bottom row of bits</param>
    internal static void
    GetPixelsFromTileBytes(short[] pixelLookupTable, 
                           ref uint[] pixelBuffer, uint[] pallete,
                           int pixelPerTileX, byte top, byte bottom)
    {
      short lookup = pixelLookupTable[(top << 8) | bottom];
      for (int i = 0; i < pixelPerTileX; i++)
      {
        int index = (lookup >> 2 * (7 - i)) & 0x3;
        uint color = pallete[index];
        pixelBuffer[i] = color;
      }
    }
  }
}
