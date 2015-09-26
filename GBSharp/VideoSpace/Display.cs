using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace GBSharp.VideoSpace
{

  internal enum DisplayModes : byte
  {
    /// <summary>
    /// H-Blank. CPU can access all VRAM
    /// </summary>
    Mode00,
    /// <summary>
    /// V-Blank. CPU can access all VRAM
    /// </summary>
    Mode01,
    /// <summary>
    /// OAM Search. CPU cannot access OAM Memory (0xFE00-0xFE9F)
    /// </summary>
    Mode10,
    /// <summary>
    /// Data Transfer. CPU cannot access any VRAM (0x8000-0x9FFF)
    /// or OAM (0xFE00-0xFE9F)
    /// </summary>
    Mode11
  }

  public class OAM
  {
    internal int index;
    internal byte y;
    internal byte x;
    internal byte spriteCode;
    internal byte flags;

    public byte X { get { return x; } }
    public byte Y { get { return y; } }
    public byte SpriteCode { get { return spriteCode; } }
    public byte Flags { get { return flags; } }

  }

  internal class DisplayDefinition
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
    internal uint[] tileColors;
    internal uint[] spriteColors;
    internal uint[] tilePallete;
    internal uint[] spritePallete0;
    internal uint[] spritePallete1;
  }

  class Display : IDisplay
  {
    public event Action RefreshScreen;

    private InterruptController interruptController;
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
      spriteOAMs[index].index = index;
      spriteOAMs[index].x = x;
      spriteOAMs[index].y = y;
      spriteOAMs[index].spriteCode = spriteCode;
      spriteOAMs[index].flags = flags;
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
      this.interruptController = interruptController;
      this.memory = memory;

      this.disDef = new DisplayDefinition();
      this.disDef.framePixelCountX = 256;
      this.disDef.framePixelCountY = 256;
      this.disDef.screenPixelCountX = 160;
      this.disDef.screenPixelCountY = 144;
      this.disDef.frameTileCountX = 32;
      this.disDef.frameTileCountY = 32;
      this.disDef.screenTileCountX = 20;
      this.disDef.screenTileCountY = 18;
      this.disDef.bytesPerTile = 16;
      this.disDef.pixelPerTileX = 8;
      this.disDef.pixelPerTileY = 8;
      this.disDef.bytesPerPixel = 4;
      this.disDef.pixelFormat = PixelFormat.Format32bppArgb;
      // TODO(Cristian): Output the color to the view for custom setting
      this.disDef.tileColors = new uint[4]
      {
        0xFFFFFFFF,
        0xFFBBBBBB,
        0xFF666666,
        0xFF000000
      };
      this.disDef.tilePallete = new uint[4];

      // TODO(Cristian): Output the color to the view for custom setting
      this.disDef.spriteColors = new uint[4]
      {
        0xFFFFFFFF,
        0xFFBBBBBB,
        0xFF666666,
        0xFF000000
      };
      this.disDef.spritePallete0 = new uint[4];
      this.disDef.spritePallete1 = new uint[4];


      // We create the target bitmaps
      this.background = new Bitmap(disDef.framePixelCountX, disDef.framePixelCountY, 
                                   disDef.pixelFormat);
      this.window = new Bitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, 
                               disDef.pixelFormat);
      this.sprite = new Bitmap(8, 8, disDef.pixelFormat);
      this.spriteLayer = new Bitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, 
                                    disDef.pixelFormat);

      this.screen = new Bitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, 
                               disDef.pixelFormat);
      this.frame = new Bitmap(disDef.framePixelCountX, disDef.framePixelCountY, 
                              disDef.pixelFormat);

      this.spriteOAMs = new OAM[spriteCount];
      for(int i = 0; i < spriteOAMs.Length; ++i)
      {
        this.spriteOAMs[i] = new OAM(); 
      }

      // TODO(Cristian): Remove this call eventually, when testing is not needed!
#if DEBUG
      //UpdateScreen();
#endif
    }

    internal void LoadSprites()
    {
      // We load the OAMs
      ushort spriteOAMAddress = 0xFE00;
      for (int i = 0; i < spriteCount; i++)
      {
        SetOAM(i, memory.LowLevelArrayRead(spriteOAMAddress, 4));
        spriteOAMAddress += 4;
      }
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

    public void UpdateScreen()
    {
      LoadSprites();

      // TODO(Cristian): Do the image composition line-based instead of image-based
      byte lcdRegister = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.LCDC);
      bool LCDBit3 = Utils.UtilFuncs.TestBit(lcdRegister, 3) != 0;
      bool LCDBit4 = Utils.UtilFuncs.TestBit(lcdRegister, 4) != 0;

      DisplayFunctions.SetupTilePallete(disDef, memory);
      DisplayFunctions.SetupSpritePalletes(disDef, memory);

      BitmapData screenBmpData = DisplayFunctions.LockBitmap(screen,
                                                             ImageLockMode.ReadWrite,
                                                             disDef.pixelFormat);

      #region BACKGROUND


      int SCX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCX);
      int SCY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.SCY);
      BitmapData backgroundBmpData = DisplayFunctions.LockBitmap(background, 
                                                                 ImageLockMode.ReadWrite, 
                                                                 disDef.pixelFormat);
      for (int row = 0; row < disDef.framePixelCountY; row++)
      {
        uint[] rowPixels = DisplayFunctions.GetRowPixels(disDef, memory, row, LCDBit3, LCDBit4);
        DisplayFunctions.DrawLine(disDef, backgroundBmpData, rowPixels, 
                                  0, row, 
                                  0, disDef.framePixelCountY);

        // TODO(Cristian): Move the background render to a DrawLine call instead of copying
        //                 from one bitmap to another
      }

      bool drawBackground = Utils.UtilFuncs.TestBit(lcdRegister, 0) != 0;
      if(drawBackground)
      {
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
      }

      bool drawRectangle = true;
      if(drawRectangle)
      {
        uint rectangleColor = 0xFFFF8822;
        DisplayFunctions.DrawRectangle(disDef, backgroundBmpData, 
                                       SCX, SCY, 
                                       disDef.screenPixelCountX, disDef.screenPixelCountY, 
                                       rectangleColor);
      }

      background.UnlockBits(backgroundBmpData);

      #endregion

      #region WINDOW

      int WX = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.WX);
      int rWX = WX - 7; // The window pos is (WX - 7, WY)
      int WY = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.WY);

      BitmapData windowBmpData = DisplayFunctions.LockBitmap(window,
                                                             ImageLockMode.ReadWrite,
                                                             disDef.pixelFormat);
      DisplayFunctions.DrawTransparency(disDef, windowBmpData, 
                                        0, 0, 
                                        disDef.screenPixelCountX, WY);
      DisplayFunctions.DrawTransparency(disDef, windowBmpData, 
                                        0, WY, 
                                        rWX, disDef.screenPixelCountY);

      bool drawWindow = Utils.UtilFuncs.TestBit(lcdRegister, 5) != 0;
      for (int row = 0; row < disDef.screenPixelCountY; row++)
      {
        if (row >= WY)
        {
          // The offset indexes represent that the window is drawn from it's beggining
          // at (WX, WY)
          uint[] rowPixels = DisplayFunctions.GetRowPixels(disDef, memory, row - WY, 
                                                           LCDBit3, LCDBit4);
          
          // Independent target
          DisplayFunctions.DrawLine(disDef, windowBmpData, rowPixels, 
                                    rWX, row, 
                                    0, disDef.screenPixelCountX - rWX);

          // Screen target
          if (drawWindow)
          {
            DisplayFunctions.DrawLine(disDef, screenBmpData, rowPixels,
                                      rWX, row,
                                      0, disDef.screenPixelCountX - rWX);
          }
        }
      }

      window.UnlockBits(windowBmpData);

      #endregion

      #region SPRITES

      // *** SPRITES ***
      BitmapData spriteLayerBmp = DisplayFunctions.LockBitmap(spriteLayer,
                                                              ImageLockMode.ReadWrite,
                                                              disDef.pixelFormat);
      DisplayFunctions.DrawTransparency(disDef, spriteLayerBmp, 
                                        0, 0, 
                                        spriteLayerBmp.Width, spriteLayerBmp.Height);

      bool drawSprites = Utils.UtilFuncs.TestBit(lcdRegister, 1) != 0;
      for (int row = 0; row < disDef.screenPixelCountY; row++)
      {
        // Independent target
        uint[] pixels = new uint[disDef.screenPixelCountX];
        DisplayFunctions.GetSpriteRowPixels(disDef, memory, spriteOAMs, pixels, row);
        DisplayFunctions.DrawLine(disDef, spriteLayerBmp, pixels, 
                                  0, row, 
                                  0, disDef.screenPixelCountX);

        // Screen Target
        if(drawSprites)
        {
          uint[] linePixels = DisplayFunctions.GetPixelRowFromBitmap(disDef, screenBmpData, row);
          DisplayFunctions.GetSpriteRowPixels(disDef, memory, spriteOAMs, linePixels, row);
          DisplayFunctions.DrawLine(disDef, screenBmpData, linePixels, 
                                    0, row, 
                                    0, disDef.screenPixelCountX);
        }
      }

      spriteLayer.UnlockBits(spriteLayerBmp);

      #endregion

      // *** SCREEN RECTANGLE ***

      screen.UnlockBits(screenBmpData);
    }

    private const int screenStep = 96905; // Aprox. ~16.6687 ms
    private int screenSum = 0;

    private int prevTickCount = 0;
    private int currentLineTickCount = 0; // We trigger OAM search at the start

    private int OAMSearchTickCount = 83;
    private int DataTransferTickCount = 83+175;
    private const int totalLineTickCount = 456;

    private byte currentLine = 0; // The first run will fix this numberooh
    private DisplayModes displayMode;

    private double pixelsPerTick = (double)256 / (double)456;

    private bool debugFirstRun = true;

    /// <summary>
    /// Simulates the update of the display for a period of time of a given number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks ellapsed since the last call.
    /// A tick is a complete source clock oscillation, ~238.4 ns (2^-22 seconds).</param>
    internal void Step(int ticks)
    {
      // TODO(Cristian): Remove this code!!
      if(debugFirstRun)
      {
        DrawTransparency();
        debugFirstRun = false;
      }

      // TODO(Cristian): Check that the LY=LYC is correct when the display starts

      /**
       * We want to advance the display according to the tick count
       * So the simulation is to decrease the tick count and simulating
       * the display accordingly
       **/
      prevTickCount = currentLineTickCount;
      while(ticks > 0)
      {
        // We try to advance to the next state
        // The display behaves differently if it's on V-BLANK or not
        if(displayMode != DisplayModes.Mode01)
        {
          if(displayMode == DisplayModes.Mode10)
          {
            if(CalculateTickChange(OAMSearchTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode11);
            }
          }
          else if(displayMode == DisplayModes.Mode11)
          {
            if(CalculateTickChange(DataTransferTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode00);
            }
          }
          else if(displayMode == DisplayModes.Mode00)
          {
            if(CalculateTickChange(totalLineTickCount, ref ticks))
            {
              // We start a new line
              ChangeDisplayLine();
              if(currentLine < 144) // We continue on normal mode
              {
                ChangeDisplayMode(DisplayModes.Mode10);
              }
              else // V-BLANK
              {
                ChangeDisplayMode(DisplayModes.Mode01);
              }

              RefreshScreen();
            }
          }
        }
        else // V-BLANK
        {
          // TODO(Cristian): Find out if the display triggers H-BLANK
          //                 events during V-BLANK
          if (CalculateTickChange(totalLineTickCount, ref ticks))
          {
            ChangeDisplayLine();
            if (currentLine >= 154)
            {
              ChangeDisplayMode(DisplayModes.Mode10);
              currentLine = 0;
              DrawTransparency();
            }

            RefreshScreen();
          }
        }
      }

      // TODO(Cristian): Copying the bitmap to the View is EXTREMELY slow. 
      //                 We (will probably) need some kind of direct access
      //                 if we want to achieve 60 FPS
      DrawPixels();
    }

    internal void DrawPixels()
    {
      //  TODO(Cristian): Remember that the WY state changes over frame (after V-BLANK)
      //                  and not between lines (as changes with WX)
      BitmapData backgroundBmpData = DisplayFunctions.LockBitmap(background,
                                                             ImageLockMode.WriteOnly,
                                                             disDef.pixelFormat);
      int uintStride = backgroundBmpData.Stride / disDef.bytesPerPixel;
      unsafe
      {
        uint* row = (uint*)backgroundBmpData.Scan0 + currentLine * uintStride;

        int beginX = (int)(pixelsPerTick * prevTickCount);
        int endX = (int)(pixelsPerTick * currentLineTickCount);
        if(beginX >= 256 || endX >= 256)
        {
          background.UnlockBits(backgroundBmpData);
          return;
        }

        byte mode = (byte)displayMode;
        uint color = 0xFFFFFFFF;
        if(mode == 1) { color = 0xFFFF0000; }
        if(mode == 2) { color = 0xFF00FF00; }
        if(mode == 3) { color = 0xFF0000FF; }

        for(int i = beginX; i < endX; ++i)
        {
          row[i] = color;
        }
      }
      background.UnlockBits(backgroundBmpData);
    }

    private void DrawTransparency()
    {
      BitmapData bmp = DisplayFunctions.LockBitmap(background,
                                                   ImageLockMode.WriteOnly,
                                                   disDef.pixelFormat);
      DisplayFunctions.DrawTransparency(disDef, bmp,
                                        0, 0,
                                        bmp.Width, bmp.Height);
      background.UnlockBits(bmp);

    }

    // Returns whether the tick count is enough to get to the target
    private bool CalculateTickChange(int target, ref int ticks)
    {
      if (currentLineTickCount > target)
      {
        throw new ArgumentOutOfRangeException("currentLineTickCount in invalid state");
      }

      int remainder = target - currentLineTickCount;
      if(ticks >= remainder)
      {
        // We got to the target
        currentLineTickCount += remainder;
        ticks -= remainder;
        return true;
      }
      else
      {
        currentLineTickCount += ticks;
        ticks = 0;
        return false;
      }
    }

    private void ChangeDisplayMode(DisplayModes newDisplayMode)
    {
      this.displayMode = newDisplayMode;
      byte byteDisplayMode = (byte)this.displayMode;

      byte STAT = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.STAT);
      // We strip the last 2 bits of STAT and replace them with the mode
      STAT = (byte)((STAT & 0xFC) | byteDisplayMode);
      this.memory.LowLevelWrite((ushort)MemoryMappedRegisters.STAT, STAT);

      // NOTE(Cristian): The bits that determine which interrupt is enabled
      //                 are ordered in the same numbers that the mode numbering
      //                 So basically we can shift by the mode numbering to get
      //                 the corresponding bit for the current mode being changed.
      //                 Bit 5: Mode 10
      // 								 Bit 4: Mode 01
      // 								 Bit 3: Mode 00
      if (((STAT >> (byteDisplayMode + 3)) & 1) == 1)
      {
        interruptController.SetInterrupt(Interrupts.LCDCStatus);
      }
    }

    private void ChangeDisplayLine()
    {
      currentLineTickCount = 0;
      ++currentLine;

      this.memory.LowLevelWrite((ushort)MemoryMappedRegisters.LY, currentLine);
      byte LYC = this.memory.Read((ushort)MemoryMappedRegisters.LYC);

      byte STAT = this.memory.LowLevelRead((ushort)MemoryMappedRegisters.STAT);
      // We update the STAT corresponding to the LY=LYC coincidence
      if (LYC == currentLine)
      {
        byte STATMask = 0x04; // Bit 2 is set 1
        STAT = (byte)(STAT | STATMask);

        if((STAT & 0x40) == 1) // We check if Bit 6 is enabled
        {
          interruptController.SetInterrupt(Interrupts.LCDCStatus);
        }
      }
      else
      {
        byte STATMask = 0xFB; // Bit 2 is set to 0 
        STAT = (byte)(STAT & STATMask);
      }

      this.memory.LowLevelWrite((ushort)MemoryMappedRegisters.STAT, STAT);
    }
  }
}
