﻿using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace GBSharp.VideoSpace
{
  public struct OAM
  {
    internal byte y;
    internal byte x;
    internal byte spriteCode;
    internal byte flags;

    public byte X { get { return x; } }
    public byte Y { get { return y; } }
    public byte SpriteCode { get { return spriteCode; } }
    public byte Flags { get { return flags; } }

  }

  internal struct DisplayDefinition
  {
    internal int framePixelCountX;
    internal int framePixelCountY;
    internal int screenPixelCountX;
    internal int screenPixelCountY;
    internal int frameTileCountX;
    internal int frameTileCountY;
    internal int screenTileCountX;
    internal int screenTileCountY;
    internal int bytesPerTile;
    internal int pixelPerTileX;
    internal int pixelPerTileY;
    internal int bytesPerPixel;
    internal PixelFormat pixelFormat;
  }

  class Display : IDisplay
  {
    public event Action RefreshScreen;

    private Memory memory;

    private DisplayDefinition disDef;

    private int spriteCount = 40;
    private OAM[] spriteOAMs;
    public OAM GetOAM(int index)
    {
      return spriteOAMs[index];
    }
    internal void SetOAM(int index, byte x, byte y, byte spriteCode, byte flags)
    {
        spriteOAMs[index].x           = x;
        spriteOAMs[index].y           = y;
        spriteOAMs[index].spriteCode  = spriteCode;
        spriteOAMs[index].flags       = flags;
    }

    /// <summary>
    /// Load an OAM from direct byte data. NOTE THE ARRAY FORMAT
    /// </summary>
    /// <param name="index"></param>
    /// <param name="data">
    /// Data layout assumes
    /// data[0]: y 
    /// data[0]: x
    /// data[0]: spriteCode 
    /// data[0]: flags 
    /// This is because this is the way the OAM are in memory.
    /// </param>
    internal void SetOAM(int index, byte[] data)
    {
      SetOAM(index, data[1], data[0], data[2], data[3]);
    }



    /// <summary>
    /// Pixel numbers of the display.
    /// With this it should be THEORETICALLY have displays
    /// of different sizes without too much hazzle.
    /// </summary>
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


    private Bitmap sprite;

    private Bitmap spriteLayer;
    public Bitmap SpriteLayer { get { return spriteLayer; } }


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
      disDef = new DisplayDefinition();
      disDef.framePixelCountX = 256;
      disDef.framePixelCountY = 256;
      disDef.screenPixelCountX = 160;
      disDef.screenPixelCountY = 144;
      disDef.frameTileCountX = 32;
      disDef.frameTileCountY = 32;
      disDef.screenTileCountX = 20;
      disDef.screenTileCountY = 18;
      disDef.bytesPerTile = 16;
      disDef.pixelPerTileX = 8;
      disDef.pixelPerTileY = 8;
      disDef.bytesPerPixel = 4;
      disDef.pixelFormat = PixelFormat.Format32bppArgb;

      this.memory = memory;

      // We create the target bitmaps
      background = new Bitmap(disDef.framePixelCountX, disDef.framePixelCountY, disDef.pixelFormat);
      window = new Bitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, disDef.pixelFormat);
      sprite = new Bitmap(8, 8, disDef.pixelFormat);
      spriteLayer = new Bitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, disDef.pixelFormat);

      screen = new Bitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, disDef.pixelFormat);
      frame = new Bitmap(disDef.framePixelCountX, disDef.framePixelCountY, disDef.pixelFormat);

      // We load the OAMs
      ushort spriteOAMAddress = 0xFE00;
      spriteOAMs = new OAM[spriteCount];
      for (int i = 0; i < spriteCount; i++)
      {
        SetOAM(i, memory.LowLevelArrayRead(spriteOAMAddress, 4));
        spriteOAMAddress += 4;
      }

      // TODO(Cristian): Remove this call eventually, when testing is not needed!
#if DEBUG
      UpdateScreen();
#endif
    }


    public Bitmap GetSprite(int index)
    {
      BitmapData spriteBmd = DisplayFunctions.LockBitmap(sprite,
                                                         ImageLockMode.WriteOnly,
                                                         disDef.pixelFormat);
      OAM oam = GetOAM(index);
      DrawSprite(spriteBmd, oam.spriteCode, 0, 0);
      sprite.UnlockBits(spriteBmd);
      return sprite;
    }

    internal void DrawSprite(BitmapData spriteBmd, int spriteCode, int pX, int pY)
    {
      byte[] pixels = DisplayFunctions.GetTileData(disDef, memory, 0x8000, spriteCode);
      DisplayFunctions.DrawTile(disDef, spriteBmd, pixels, pX, pY,
                                disDef.screenPixelCountX, disDef.screenPixelCountY);
    }

    internal void UpdateScreen()
    {
      // TODO(Cristian): Line-based drawing!!!
      byte lcdRegister = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.LCDC);
      bool LCDBit3 = Utils.UtilFuncs.TestBit(lcdRegister, 3) != 0;
      bool LCDBit4 = Utils.UtilFuncs.TestBit(lcdRegister, 4) != 0;


      // *** BACKGROUND ***
      BitmapData backgroundBmpData = DisplayFunctions.LockBitmap(background, 
                                                                 ImageLockMode.ReadOnly, 
                                                                 disDef.pixelFormat);
      for (int row = 0; row < disDef.framePixelCountY; row++)
      {
        uint[] rowPixels = DisplayFunctions.GetRowPixels(disDef, memory, row, LCDBit3, LCDBit4);
        DisplayFunctions.DrawLine(disDef, backgroundBmpData, 
                                  rowPixels, 0, row, 0, disDef.framePixelCountY);
      }
      background.UnlockBits(backgroundBmpData);

      // *** WINDOW ***
      int WX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.WX);
      int rWX = WX - 7; // The window pos is (WX - 7, WY)
      int WY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.WY);

      BitmapData windowBmpData = DisplayFunctions.LockBitmap(window, 
                                                             ImageLockMode.WriteOnly, 
                                                             disDef.pixelFormat);
      DisplayFunctions.DrawTransparency(disDef, windowBmpData, 0, 0, disDef.screenPixelCountX, WY);
      DisplayFunctions.DrawTransparency(disDef, windowBmpData, 0, WY, rWX, disDef.screenPixelCountY);

      for (int row = 0; row < disDef.screenPixelCountY; row++)
      {
        if(row >= WY)
        {
          // The offset indexes represent that the window is drawn from it's beggining
          // at (WX, WY)
          uint[] rowPixels = DisplayFunctions.GetRowPixels(disDef, memory, row - WY, LCDBit3, LCDBit4);
          DisplayFunctions.DrawLine(disDef, windowBmpData, 
                                    rowPixels, rWX, row, 0, disDef.screenPixelCountX - rWX);
        }
      }
      window.UnlockBits(windowBmpData);

      // *** SPRITES ***
      // TODO(Cristian): Find a more efficient way to keep this list sorted by priority
      OAM[] oams = (OAM[])spriteOAMs.Clone();
      Array.Sort<OAM>(oams, (a, b) => (a.x == b.x) ?
                                      (a.y - b.y) : (a.x - b.x));

#if DEBUG
      for (int i = 0; i < spriteOAMs.Length; ++i)
      {
        Console.Out.WriteLine("OLD: ({0}, {1}) \t, NEW: ({2}, {3})",
                              spriteOAMs[i].x, spriteOAMs[i].y,
                              oams[i].x, oams[i].y);
      }

      Console.Out.WriteLine("FILTERED SPRITES");
      for (int i = 0; i < spriteOAMs.Length; ++i)
      {
        int x = oams[i].x - 8;
        int y = oams[i].y - 16;
        if (x < 0 || x >= 160 ||
            y < 0 || y >= 144)
          continue;
        Console.Out.WriteLine("X: {0} \t, Y: {1}", x, y);
      }

      //// Sprite Checks
      //DrawSprite(spriteLayerBmp, 0, -5, -5);
      //DrawSprite(spriteLayerBmp, 0, 155, -5);
      //DrawSprite(spriteLayerBmp, 0, -5, 80);
      //DrawSprite(spriteLayerBmp, 0, -5, 140);
      //DrawSprite(spriteLayerBmp, 0, -50, -50);
      //DrawSprite(spriteLayerBmp, 0, 155, 80);
      //DrawSprite(spriteLayerBmp, 0, 10, 140);
      //DrawSprite(spriteLayerBmp, 0, 155, 140);
      //DrawSprite(spriteLayerBmp, 0, 200, 200);
#endif

      BitmapData spriteLayerBmp = DisplayFunctions.LockBitmap(spriteLayer,
                                                              ImageLockMode.WriteOnly,
                                                              disDef.pixelFormat);
      DisplayFunctions.DrawTransparency(disDef, spriteLayerBmp, 0, 0, spriteLayerBmp.Width, spriteLayerBmp.Height);

      //int maxScanLineSize = 10;
      //OAM[] scanLineOAMs = new OAM[maxScanLineSize];
      //for (int row = 0; row < disDef.screenPixelCountY; row++)
      //{
      //  // We select which sprites enter the scan
      //  int scanLineSize = 0;
      //  for (int i = 0; i < spriteCount; ++i)
      //  {
      //    // We load the OAMs to be displayed
      //    OAM oam = oams[i];
      //    int y = oam.y - 16;
      //    if ((y <= row) && (row <= (y + 8)))
      //    {
      //      scanLineOAMs[scanLineSize++] = oam;
      //      if (scanLineSize == maxScanLineSize) { break; }
      //    }
      //  }

      //  int a = 10;

      //  for (int i = (scanLineSize - 1); i >= 0; --i)
      //  {
      //    OAM oam = scanLineOAMs[i];
      //    int x = oam.x - 8;
      //    int y = oam.y - 16;
      //    DrawSprite(spriteLayerBmp, oam.spriteCode, x, y);
      //  }
      //}


      for (int row = 0; row < disDef.screenPixelCountY; row++)
      {
        uint[] pixels = DisplayFunctions.GetSpriteRowPixels(disDef, memory, spriteOAMs, row);
        DisplayFunctions.DrawLine(disDef, spriteLayerBmp, pixels, 0, row, 0, disDef.screenPixelCountX);
      }




      spriteLayer.UnlockBits(spriteLayerBmp);



      // *** SCREEN ***

      // We draw the SCREEN
      int SCX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCX);
      int SCY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCY);
      BitmapData screenBmpData = screen.LockBits(
        new Rectangle(0, 0, screen.Width, screen.Height),
        ImageLockMode.WriteOnly, disDef.pixelFormat);
      // Background Pass
      bool drawBackground = Utils.UtilFuncs.TestBit(lcdRegister, 0) != 0;
      if(drawBackground)
      {
        backgroundBmpData = DisplayFunctions.LockBitmap(background, 
                                                        ImageLockMode.ReadOnly, 
                                                        disDef.pixelFormat);

        // We copy the information from the background tile to the effective screen
        int backgroundUintStride = backgroundBmpData.Stride / disDef.bytesPerPixel;
        int screenUintStride = screenBmpData.Stride / disDef.bytesPerPixel;
        for (int y = 0; y < disDef.screenPixelCountY; y++)
        {
          for (int x = 0; x < disDef.screenPixelCountX; x++)
          {
            int pX = (x + SCX) % disDef.framePixelCountX;
            int pY = (y + SCY) % disDef.framePixelCountY;

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
        windowBmpData = DisplayFunctions.LockBitmap(window, ImageLockMode.ReadOnly, disDef.pixelFormat);

        int windowUintStrided = windowBmpData.Stride / disDef.bytesPerPixel;
        int screenUintStride = screenBmpData.Stride / disDef.bytesPerPixel;

        for (int y = 0; y < disDef.screenPixelCountY; y++)
        {
          for (int x = 0; x < disDef.screenPixelCountX; x++)
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
        backgroundBmpData = DisplayFunctions.LockBitmap(background, 
                                                        ImageLockMode.WriteOnly, 
                                                        disDef.pixelFormat);
        DisplayFunctions.DrawRectangle(disDef, backgroundBmpData, 
                                       SCX, SCY, 
                                       disDef.screenPixelCountX, disDef.screenPixelCountY, 
                                       0xFFFF8822);
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
