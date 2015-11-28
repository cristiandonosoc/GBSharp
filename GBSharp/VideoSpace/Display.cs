using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace GBSharp.VideoSpace
{

  public enum DisplayModes : byte
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

  public enum DebugTargets 
  {
    Background = 0,
    Tiles = 1,
    Window = 2,
    SpriteLayer = 3,
    DisplayTiming = 4
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

  public class DisplayDefinition
  {
    public int framePixelCountX;
    public int framePixelCountY;
    public int screenPixelCountX;
    public int screenPixelCountY;
    public int timingPixelCountX;
    public int timingPixelCountY;
    public int frameTileCountX;
    public int frameTileCountY;
    public int screenTileCountX;
    public int screenTileCountY;
    public int bytesPerTileShort;
    public int bytesPerTileLong;
    public int pixelPerTileX;
    public int pixelPerTileY;
    public int bytesPerPixel;
    public PixelFormat pixelFormat;
    public uint[] tileColors;
    public uint[] spriteColors;
    public uint[] tilePallete;
    public uint[] spritePallete0;
    public uint[] spritePallete1;
  }

  public class DisplayStatus
  {
    public int prevTickCount;
    public int currentLineTickCount; // We trigger OAM search at the start
    public byte currentLine;
    public int OAMSearchTickCount;
    public int dataTransferTickCount;
    public int totalLineTickCount;
    public bool enabled;
    public DisplayModes displayMode;

    // Debug targets
    public bool tileBase;
    public bool noTileMap;
    public bool tileMap;

    // Registers
    public bool[] LCDCBits;
    public byte STAT;
    public byte SCX;
    public byte SCY;
    public byte LY;
    public byte LYC;
    public byte WX;
    public byte WY;
    // WY actually changes at the beginning of the frame
    public byte currentWY;
  }

  class Display : IDisplay
  {

    public event Action FrameReady;

    private InterruptController interruptController;
    private Memory memory;

    private int spriteCount = 40;
    private OAM[] spriteOAMs;
    public OAM GetOAM(int index)
    {
      return spriteOAMs[index];
    }

    private DisplayDefinition disDef;
    public DisplayDefinition GetDisplayDefinition()
    {
      return disDef;
    }

    private DisplayStatus disStat;
    public DisplayStatus GetDisplayStatus()
    {
      return disStat;
    }

    private bool[] updateDebugTargets;
    private uint[][] debugTargets;
    public uint[] GetDebugTarget(DebugTargets debugTarget)
    {
      return debugTargets[(int)debugTarget];
    }

    public bool GetUpdateDebugTarget(DebugTargets debugTarget)
    {
      return updateDebugTargets[(int)debugTarget];
    }
    public void SetUpdateDebugTarget(DebugTargets debugTarget, bool value)
    {
      updateDebugTargets[(int)debugTarget] = value;
    }

    private uint[] sprite;

    /// <summary>
    /// The bitmap that represents the actual screen.
    /// It's a portial of the frame specified by the SCX and SCY registers.
    /// </summary>
    private uint[] screen;
    public uint[] Screen { get { return screen; } }

    public bool TileBase
    {
      get { return disStat.tileBase; }
      set { disStat.tileBase = value; }
    }
    public bool NoTileMap
    {
      get { return disStat.noTileMap; }
      set { disStat.noTileMap = value; }
    }
    public bool TileMap
    {
      get { return disStat.tileMap; }
      set { disStat.tileMap = value; }
    }

    // Temporary buffer used or not allocating a local one on each iteration
    private uint[] pixelBuffer;

    /// <summary>
    /// Display constructor.
    /// </summary>
    /// <param name="interruptController">A reference to the interrupt controller.</param>
    /// <param name="Memory">A reference to the memory.</param>
    public Display(InterruptController interruptController, Memory memory)
    {
      this.interruptController = interruptController;
      this.memory = memory;

      /*** DISPLAY DEFINITION ***/

      this.disDef = new DisplayDefinition();
      this.disDef.framePixelCountX = 256;
      this.disDef.framePixelCountY = 256;
      this.disDef.screenPixelCountX = 160;
      this.disDef.screenPixelCountY = 144;
      this.disDef.timingPixelCountX = 256;
      this.disDef.timingPixelCountY = 154;
      this.disDef.frameTileCountX = 32;
      this.disDef.frameTileCountY = 32;
      this.disDef.screenTileCountX = 20;
      this.disDef.screenTileCountY = 18;
      this.disDef.bytesPerTileShort = 16;
      this.disDef.bytesPerTileLong = 32;
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

      // Tile stargets
      this.spriteOAMs = new OAM[spriteCount];
      for (int i = 0; i < spriteOAMs.Length; ++i)
      {
        this.spriteOAMs[i] = new OAM();
      }

      /*** DISPLAY STATUS ***/

      this.disStat = new DisplayStatus();
      this.disStat.prevTickCount = 0;
      this.disStat.currentLineTickCount = 0;
      this.disStat.currentLine = 0;
      // NOTE(Cristian): This are default values when there are no sprites
      //                 They should change on runtime
      this.disStat.OAMSearchTickCount = 83;
      this.disStat.dataTransferTickCount = 83 + 175;
      this.disStat.totalLineTickCount = 456;
      this.disStat.enabled = true;
      // TODO(Cristian): Find out at what state the display starts!
      this.disStat.displayMode = DisplayModes.Mode10;

      this.disStat.tileBase = true;
      this.disStat.noTileMap = false;
      this.disStat.tileMap = false;

      this.disStat.LCDCBits = new bool[8];

      // We start the registers correctly
      HandleMemoryChange(MMR.LCDC, memory.LowLevelRead((ushort)MMR.LCDC));
      HandleMemoryChange(MMR.LCDC, memory.LowLevelRead((ushort)MMR.LCDC));
      HandleMemoryChange(MMR.SCY, memory.LowLevelRead((ushort)MMR.SCY));
      HandleMemoryChange(MMR.SCX, memory.LowLevelRead((ushort)MMR.SCX));
      HandleMemoryChange(MMR.LYC, memory.LowLevelRead((ushort)MMR.LYC));
      HandleMemoryChange(MMR.DMA, memory.LowLevelRead((ushort)MMR.DMA));
      HandleMemoryChange(MMR.BGP, memory.LowLevelRead((ushort)MMR.BGP));
      HandleMemoryChange(MMR.OBP0, memory.LowLevelRead((ushort)MMR.OBP0));
      HandleMemoryChange(MMR.OBP1, memory.LowLevelRead((ushort)MMR.OBP1));
      HandleMemoryChange(MMR.WY, memory.LowLevelRead((ushort)MMR.WY));
      HandleMemoryChange(MMR.WX, memory.LowLevelRead((ushort)MMR.WX));

      /*** DRAW TARGETS ***/

      // We create the target bitmaps
      debugTargets = new uint[Enum.GetNames(typeof(DebugTargets)).Length][];
      updateDebugTargets = new bool[Enum.GetNames(typeof(DebugTargets)).Length];

      debugTargets[(int)DebugTargets.Background] = new uint[disDef.framePixelCountX * disDef.framePixelCountY];
      updateDebugTargets[(int)DebugTargets.Background] = false;

      debugTargets[(int)DebugTargets.Tiles] = new uint[disDef.screenPixelCountX * disDef.screenPixelCountY];
      updateDebugTargets[(int)DebugTargets.Tiles] = false;

      debugTargets[(int)DebugTargets.Window] = new uint[disDef.screenPixelCountX * disDef.screenPixelCountY];
      updateDebugTargets[(int)DebugTargets.Window] = false;

      debugTargets[(int)DebugTargets.SpriteLayer] = new uint[disDef.screenPixelCountX * disDef.screenPixelCountY];
      updateDebugTargets[(int)DebugTargets.SpriteLayer] = false;

      debugTargets[(int)DebugTargets.DisplayTiming] = new uint[disDef.timingPixelCountX * disDef.timingPixelCountY];
      updateDebugTargets[(int)DebugTargets.DisplayTiming] = false;

      this.screen = new uint[disDef.screenPixelCountX * disDef.screenPixelCountY];
      this.sprite = new uint[8 * 16];

      this.pixelBuffer = new uint[disDef.pixelPerTileX];

      GeneratePixelLookupTable();

      // We update the display status info
      UpdateDisplayLineInfo(false);
    }

    private short[] pixelLookupTable;
    private void GeneratePixelLookupTable()
    {
      pixelLookupTable = new short[0x100 * 0x100];
      for(int top = 0; top < 0x100; ++top)
      {
        for (int bottom = 0; bottom < 0x100; ++bottom)
        {
          int lookup = 0;
          for (int i = 0; i < 8; ++i)
          {
            int up = (bottom >> (7 - i)) & 1;
            int down = (top >> (7 - i)) & 1;
            lookup = lookup | (((up << 1) | down) << 2 * (7 - i));
          }

          pixelLookupTable[(top << 8) | bottom] = (short)lookup;
        }
      }

    }

    internal void LoadSprites()
    {
      // We load the OAMs
      ushort spriteOAMAddress = 0xFE00;
      byte[] tempArray;
      for (int i = 0; i < spriteCount; i++)
      {
        tempArray = memory.LowLevelArrayRead(spriteOAMAddress, 4);
        spriteOAMs[i].index = spriteCount;
        spriteOAMs[i].x = tempArray[1];
        spriteOAMs[i].y = tempArray[0];
        spriteOAMs[i].spriteCode = tempArray[2];
        spriteOAMs[i].flags = tempArray[3];

        spriteOAMAddress += 4;
      }

      // Now we sort them according to sprite priority rules
      Array.Sort<OAM>(spriteOAMs, (a, b) => (a.x == b.x) ?
                                            (a.index - b.index) : (a.x - b.x));
    }

    internal void HandleMemoryChange(MMR mappedRegister, byte value)
    {

      switch(mappedRegister)
      {
        case MMR.LCDC:
          // We set all the LCDC bits
          for (int i = 0; i < 8; ++i)
          {
            disStat.LCDCBits[i] = (value & (1 << i)) != 0;
          }
          break;
        case MMR.STAT:
          disStat.STAT = value;
          break;
        case MMR.SCY:
          disStat.SCY = value;
          break;
        case MMR.SCX:
          disStat.SCX = value;
          break;
        case MMR.LY:
          // TODO(Cristian): Perhaps this should be handled by memory?
          throw new InvalidProgramException("There shouldn't be any writes to LY");
        case MMR.LYC:
          disStat.LYC = value;
          break;
        case MMR.DMA:
          LoadSprites(); 
          break;
        case MMR.BGP:
          DisFuncs.SetupTilePallete(disDef, memory);
          break;
        case MMR.OBP0:
          DisFuncs.SetupSpritePalletes(disDef, memory, MMR.OBP0);
          break;
        case MMR.OBP1:
          DisFuncs.SetupSpritePalletes(disDef, memory, MMR.OBP1);
          break;
        case MMR.WY:
          disStat.WY = value;
          break;
        case MMR.WX:
          disStat.WX = value;
          break;
        default:
          throw new InvalidProgramException("All cases should be handled...");
      }
    }


    public uint[] GetSprite(int index)
    {
      OAM oam = GetOAM(index);
      DrawSprite(sprite, oam.spriteCode, 0, 0);
      return sprite;
    }

    internal void DrawSprite(uint[] spriteData, int spriteCode, int pX, int pY)
    {
      DrawFuncs.DrawTransparency(disDef, spriteData, 8, 0, 0, 8, 16);

      if(disStat.LCDCBits[2])
      {
        spriteCode = spriteCode & 0xFE; // We remove the last bit
      }

      // We draw the top part
      byte[] pixels = DisFuncs.GetTileData(disDef, memory, 0x8000, spriteCode, disStat.LCDCBits[2]);
      DrawFuncs.DrawTile(pixelLookupTable, 
                         disDef, spriteData, pixelBuffer, 
                         8, pixels, pX, pY,
                         disDef.screenPixelCountX, disDef.screenPixelCountY);
    }

    private void StartFrame()
    {
      disStat.currentLine = 0;
      disStat.currentWY = disStat.WY;

      if(!disStat.enabled) { return; }

      // WINDOW TRANSPARENCY
      if (updateDebugTargets[(int)DebugTargets.Window])
      {
        DrawFuncs.DrawTransparency(disDef, debugTargets[(int)DebugTargets.Window],
                                  disDef.screenPixelCountX,
                                  0, 0,
                                  disDef.screenPixelCountX, disDef.screenPixelCountY);
      }

      // SPRITES TRANSPARENCY
      if (updateDebugTargets[(int)DebugTargets.SpriteLayer])
      {
        DrawFuncs.DrawTransparency(disDef, debugTargets[(int)DebugTargets.SpriteLayer],
                                  disDef.screenPixelCountX,
                                  0, 0,
                                  disDef.screenPixelCountX, disDef.screenPixelCountY);
      }
    }

    public void DrawFrame(int rowBegin, int rowEnd)
    {
      if(!disStat.enabled) { return; }
      if (rowBegin > 143) { return; }

      #region BACKGROUND

      // TODO(Cristian): Move this to disStat

      bool drawBackground = disStat.LCDCBits[0];
      // We copy the information from the background tile to the effective screen
      for (int y = rowBegin; y < rowEnd; y++)
      {

        // We obtain the correct row
        int bY = (y + disStat.SCY) % disDef.framePixelCountY;
        uint[] rowPixels = DisFuncs.GetRowPixels(pixelLookupTable,
                                                 disDef, memory, pixelBuffer,
                                                 bY, disStat.LCDCBits[3], disStat.LCDCBits[4]);

        if(updateDebugTargets[(int)DebugTargets.Background])
        {
          DrawFuncs.DrawLine(disDef, debugTargets[(int)DebugTargets.Background],
                            disDef.framePixelCountX,
                            rowPixels,
                            0, bY,
                            0, disDef.framePixelCountX);
        }

        if (drawBackground)
        {
          DrawFuncs.DrawLine(disDef, screen, disDef.screenPixelCountX, rowPixels,
                            0, y,
                            disStat.SCX, disDef.framePixelCountX,
                            false, true);


        }
      }

      #endregion

      #region WINDOW

      int rWX = disStat.WX - 7; // The window pos is (WX - 7, WY)

      // TODO(Cristian): If BG display is off, it actually prints white
      bool drawWindow = disStat.LCDCBits[5];
      for (int row = rowBegin; row < rowEnd; row++)
      {
        if ((row >= disStat.currentWY) && (row < 144))
        {
          // The offset indexes represent that the window is drawn from it's beggining
          // at (WX, WY)
          uint[] rowPixels = DisFuncs.GetRowPixels(pixelLookupTable,
                                                   disDef, memory, pixelBuffer,
                                                   row - disStat.currentWY, 
                                                   disStat.LCDCBits[6], disStat.LCDCBits[4]);

          // Independent target
          if(updateDebugTargets[(int)DebugTargets.Window])
          {
            DrawFuncs.DrawLine(disDef, debugTargets[(int)DebugTargets.Window],
                              disDef.screenPixelCountX,
                              rowPixels,
                              rWX, row,
                              0, disDef.screenPixelCountX - rWX);
          }

          // Screen target
          if (drawWindow)
          {
            DrawFuncs.DrawLine(disDef, screen, disDef.screenPixelCountX,
                              rowPixels,
                              rWX, row,
                              0, disDef.screenPixelCountX - rWX);
          }
        }
      }

      #endregion

      #region SPRITES

      bool drawSprites = disStat.LCDCBits[1];
      for (int row = rowBegin; row < rowEnd; row++)
      {
        if(updateDebugTargets[(int)DebugTargets.SpriteLayer])
        {
          // Independent target
          uint[] pixels = new uint[disDef.screenPixelCountX];
          DisFuncs.GetSpriteRowPixels(pixelLookupTable,
                                      disDef, memory, spriteOAMs, pixels,
                                      pixelBuffer,
                                      row, disStat.LCDCBits[2],
                                      true);
          DrawFuncs.DrawLine(disDef, debugTargets[(int)DebugTargets.SpriteLayer],
                            disDef.screenPixelCountX,
                            pixels,
                            0, row,
                            0, disDef.screenPixelCountX);
        }

        // Screen Target
        if (drawSprites)
        {
          uint[] linePixels = DisFuncs.GetPixelRowFromBitmap(disDef, screen, 
                                                             row, disDef.screenPixelCountX);
          DisFuncs.GetSpriteRowPixels(pixelLookupTable,
                                      disDef, memory, spriteOAMs, 
                                      linePixels, pixelBuffer,
                                      row, disStat.LCDCBits[2]);
          DrawFuncs.DrawLine(disDef, screen, disDef.screenPixelCountX,
                            linePixels,
                            0, row,
                            0, disDef.screenPixelCountX);
        }
      }

      #endregion
    }

    private void EndFrame()
    {
      if (disStat.enabled)
      {
        if (updateDebugTargets[(int)DebugTargets.Background])
        {
          // TODO(Cristian): Move this to disStat
          uint rectangleColor = 0xFFFF8822;
          DrawFuncs.DrawRectangle(disDef, debugTargets[(int)DebugTargets.Background],
                                 disDef.framePixelCountX,
                                 disStat.SCX, disStat.SCY,
                                 disDef.screenPixelCountX, disDef.screenPixelCountY,
                                 rectangleColor);
        }

        if (updateDebugTargets[(int)DebugTargets.Tiles])
        {
          DrawTiles();
        }
      }

      FrameReady();
    }

    public void DrawTiles()
    {
      ushort tileBaseAddress = DisFuncs.GetTileBaseAddress(disStat.tileBase);
      ushort tileMapBaseAddress = DisFuncs.GetTileMapBaseAddress(disStat.tileMap);
      for (int tileY = 0; tileY < 18; ++tileY)
      {
        for (int tileX = 0; tileX < 20; ++tileX)
        {
          int tileOffset;
          if(disStat.noTileMap)
          {
            tileOffset = 16 * tileY + tileX;
          }
          else
          {
            tileOffset = DisFuncs.GetTileOffset(disDef, memory, tileMapBaseAddress,
                                                  disStat.tileBase, tileX, tileY);
          }
          byte[] tileData = DisFuncs.GetTileData(disDef, memory, tileBaseAddress, tileOffset, false);

          DrawFuncs.DrawTile(pixelLookupTable,
                             disDef, debugTargets[(int)DebugTargets.Tiles],
                             pixelBuffer,
                             disDef.screenPixelCountX, tileData,
                             8 * tileX, 8 * tileY, 
                             256, 256);
        }
      }
    }

    private double pixelsPerTick = (double)256 / (double)456;
    private bool firstRun = true;

    /// <summary>
    /// Simulates the update of the display for a period of time of a given number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks ellapsed since the last call.
    /// A tick is a complete source clock oscillation, ~238.4 ns (2^-22 seconds).</param>
    internal void Step(int ticks)
    {
      // We check if the display is supposed to run
      bool activate = disStat.LCDCBits[7];

      if(activate && !disStat.enabled) // We need to turn on the LCD
      {
        disStat.enabled = true;
      }
      if(!activate && disStat.enabled) // We need to turn off the LCD
      {
        if(disStat.currentLine < 144)
        {
          // NOTE(Cristian): Turning off the gameboy *should* be made only during V-BLANK.
          //                 Apparently it damages the hardware otherwise.
          // TODO(Cristian): See if this should be an assertion
          throw new InvalidOperationException("Stopping LCD should be made during V-BLANK");
        }
        disStat.enabled = false;
      }

      /**
       * We want to advance the display according to the tick count
       * So the simulation is to decrease the tick count and simulating
       * the display accordingly
       **/
      disStat.prevTickCount = disStat.currentLineTickCount;
      while(ticks > 0)
      {
        // We try to advance to the next state
        // The display behaves differently if it's on V-BLANK or not
        if(disStat.displayMode != DisplayModes.Mode01)
        {
          if(disStat.displayMode == DisplayModes.Mode10)
          {
            if(CalculateTickChange(disStat.OAMSearchTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode11);
            }
          }
          else if(disStat.displayMode == DisplayModes.Mode11)
          {
            if(CalculateTickChange(disStat.dataTransferTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode00);
              DrawFrame(disStat.currentLine, disStat.currentLine + 1);
            }
          }
          else if(disStat.displayMode == DisplayModes.Mode00)
          {
            if(CalculateTickChange(disStat.totalLineTickCount, ref ticks))
            {
              // We start a new line
              UpdateDisplayLineInfo();
              if(disStat.currentLine < 144) // We continue on normal mode
              {
                ChangeDisplayMode(DisplayModes.Mode10);
              }
              else // V-BLANK
              {
                ChangeDisplayMode(DisplayModes.Mode01);
              }
            }
          }
        }
        else // V-BLANK
        {
          // TODO(Cristian): Find out if the display triggers H-BLANK
          //                 events during V-BLANK
          if (CalculateTickChange(disStat.totalLineTickCount, ref ticks))
          {
            UpdateDisplayLineInfo();
            if (disStat.currentLine >= 154)
            {
              ChangeDisplayMode(DisplayModes.Mode10);
              EndFrame();
              StartFrame();
            }
          }
        }
      }

      if(updateDebugTargets[(int)DebugTargets.DisplayTiming])
      {
        DrawTiming();
      }

      if(firstRun)
      {
        firstRun = false;
        disStat.currentWY = 0;
      }
    }

    internal void DrawTiming()
    {
      // This probably means an empty step (breakpoint)
      if (disStat.prevTickCount == disStat.currentLineTickCount) { return; }
      //  TODO(Cristian): Remember that the WY state changes over frame (after V-BLANK)
      //                  and not between lines (as changes with WX)


      int beginX = (int)(pixelsPerTick * disStat.prevTickCount);
      int endX = (int)(pixelsPerTick * disStat.currentLineTickCount);
      if (beginX >= 256 || endX >= 256)
      {
        return;
      }

      if ((disStat.currentLine == 0) &&
         ((endX <= beginX) || firstRun))
      {
        //DisplayFunctions.DrawTransparency(disDef, displayTimingBmpData, 0, 0, 256, 154);
        DrawFuncs.DrawRectangle(disDef, debugTargets[(int)DebugTargets.DisplayTiming],
                               disDef.timingPixelCountX,
                               0, 0,
                               disDef.timingPixelCountX, disDef.timingPixelCountY,
                               0xFF000000, true);
      }



      byte mode = (byte)disStat.displayMode;
      uint color = 0xFFFFFF00;
      if (mode == 1) { color = 0xFFFF0000; }
      if (mode == 2) { color = 0xFF00FF00; }
      if (mode == 3) { color = 0xFF0000FF; }

      int rowIndex = disStat.currentLine * disDef.timingPixelCountX;

      // TODO(Cristian): Check if this dictionary access is happening every time!
      for (int i = beginX; i < endX; ++i)
      {
        debugTargets[(int)DebugTargets.DisplayTiming][rowIndex + i] = color;
      }
    }

    // Returns whether the tick count is enough to get to the target
    private bool CalculateTickChange(int target, ref int ticks)
    {
      if (disStat.currentLineTickCount > target)
      {
        throw new ArgumentOutOfRangeException("currentLineTickCount in invalid state");
      }

      int remainder = target - disStat.currentLineTickCount;
      if(ticks >= remainder)
      {
        // We got to the target
        disStat.currentLineTickCount += remainder;
        ticks -= remainder;
        return true;
      }
      else
      {
        disStat.currentLineTickCount += ticks;
        ticks = 0;
        return false;
      }
    }

    private void ChangeDisplayMode(DisplayModes newDisplayMode)
    {
      disStat.displayMode = newDisplayMode;
      byte byteDisplayMode = (byte)disStat.displayMode;

      if(!disStat.enabled) { return; }

      // We strip the last 2 bits of STAT and replace them with the mode
      disStat.STAT = (byte)((disStat.STAT & 0xFC) | byteDisplayMode);
      this.memory.LowLevelWrite((ushort)MMR.STAT, disStat.STAT);

      // We check if we have to trigger vertical blanking
      if(disStat.displayMode == DisplayModes.Mode01) // We just change to V-BLANK Mode
      {
        interruptController.SetInterrupt(Interrupts.VerticalBlanking);
      }

      // NOTE(Cristian): The bits that determine which interrupt is enabled
      //                 are ordered in the same numbers that the mode numbering
      //                 So basically we can shift by the mode numbering to get
      //                 the corresponding bit for the current mode being changed.
      //                 Bit 5: Mode 10
      // 								 Bit 4: Mode 01
      // 								 Bit 3: Mode 00
      if ((newDisplayMode != DisplayModes.Mode11) &&
          ((disStat.STAT >> (byteDisplayMode + 3)) & 1) == 1)
      {
        interruptController.SetInterrupt(Interrupts.LCDCStatus);
      }
    }

    /// <summary>
    /// Updates the line variables of the Display and gameboy's registers.
    /// Triggers the LYC=LY interrupt when in corresponds.
    /// </summary>
    /// <param name="updateLineCount">
    /// Whether update the variables.
    /// This is used when we want only to update the gameboy's registers
    /// without changing the line count (for example, when the display starts).
    /// </param>
    private void UpdateDisplayLineInfo(bool updateLineCount = true)
    {
      if (updateLineCount)
      {
        disStat.currentLineTickCount = 0;
        ++disStat.currentLine;
      }

      if(!disStat.enabled) { return; }

      this.memory.LowLevelWrite((ushort)MMR.LY, disStat.currentLine);

      // We update the STAT corresponding to the LY=LYC coincidence
      if (disStat.LYC == disStat.currentLine)
      {
        byte STATMask = 0x04; // Bit 2 is set 1
        disStat.STAT = (byte)(disStat.STAT | STATMask);

        if(Utils.UtilFuncs.TestBit(disStat.STAT, 6) != 0)
        {
          interruptController.SetInterrupt(Interrupts.LCDCStatus);
        }
      }
      else
      {
        byte STATMask = 0xFB; // Bit 2 is set to 0 
        disStat.STAT = (byte)(disStat.STAT & STATMask);
      }

      this.memory.LowLevelWrite((ushort)MMR.STAT, disStat.STAT);
    }
  }
}
