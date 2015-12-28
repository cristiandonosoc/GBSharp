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
    public int Index { get; internal set; }
    public byte Y { get; internal set; }
    public byte X { get; internal set; }
    public byte SpriteCode { get; internal set; }
    public byte Flags { get; internal set; }
  }

  public class DisplayDefinition
  {
    public int FramePixelCountX { get; internal set; }
    public int FramePixelCountY { get; internal set; }
    public int ScreenPixelCountX { get; internal set; }
    public int ScreenPixelCountY { get; internal set; }
    public int TimingPixelCountX { get; internal set; }
    public int TimingPixelCountY { get; internal set; }
    public int FrameTileCountX { get; internal set; }
    public int FrameTileCountY { get; internal set; }
    public int ScreenTileCountX { get; internal set; }
    public int ScreenTileCountY { get; internal set; }
    public int BytesPerTileShort { get; internal set; }
    public int BytesPerTileLong { get; internal set; }
    public int PixelPerTileX { get; internal set; }
    public int PixelPerTileY { get; internal set; }
    public int BytesPerPixel { get; internal set; }
    public PixelFormat PixelFormat { get; internal set; }
    public uint[] TileColors { get; internal set; }
    public uint[] SpriteColors { get; internal set; }
    public uint[] TilePallete { get; internal set; }
    public uint[] SpritePallete0 { get; internal set; }
    public uint[] SpritePallete1 { get; internal set; }
    public uint RectangleColor { get; set; }
  }

  public class DisplayStatus
  {
    public int PrevTickCount { get; internal set; }
    public int CurrentLineTickCount { get; internal set; }
    public byte CurrentLine { get; internal set; }
    public int OAMSearchTickCount { get; internal set; }
    public int DataTransferTickCount { get; internal set; }
    public int TotalLineTickCount { get; internal set; }
    public bool Enabled { get; internal set; }
    public DisplayModes DisplayMode { get; internal set; }

    // Debug targets
    public bool TileBase { get; internal set; }
    public bool NoTileMap { get; internal set; }
    public bool TileMap { get; internal set; }

    // Registers
    public bool[] LCDCBits { get; internal set; }
    public byte STAT { get; internal set; }
    public byte SCX { get; internal set; }
    public byte SCY { get; internal set; }
    public byte LY { get; internal set; }
    public byte LYC { get; internal set; }
    public byte WX { get; internal set; }
    public byte WY { get; internal set; }
    // WY actually changes at the beginning of the frame
    public byte CurrentWY { get; internal set; }
  }

  class Display : IDisplay
  {

    public event Action FrameReady;

    private InterruptController _interruptController;
    private Memory _memory;

    private int _spriteCount = 40;
    private OAM[] _spriteOAMs;
    public OAM GetOAM(int index)
    {
      return _spriteOAMs[index];
    }

    private DisplayDefinition _disDef;
    public DisplayDefinition GetDisplayDefinition()
    {
      return _disDef;
    }

    private DisplayStatus _disStat;
    public DisplayStatus GetDisplayStatus()
    {
      return _disStat;
    }

    private bool[] _updateDebugTargets;
    private uint[][] _debugInternalTargets;
    public uint[] GetDebugTarget(DebugTargets debugTarget)
    {
      return _debugInternalTargets[(int)debugTarget];
    }

    public bool GetUpdateDebugTarget(DebugTargets debugTarget)
    {
      return _updateDebugTargets[(int)debugTarget];
    }
    public void SetUpdateDebugTarget(DebugTargets debugTarget, bool value)
    {
      _updateDebugTargets[(int)debugTarget] = value;
    }

    private uint[] _sprite;

    /// <summary>
    /// The bitmap that represents the actual screen.
    /// It's a portial of the frame specified by the SCX and SCY registers.
    /// </summary>
    private uint[] _screenInternalBuffer;
    public uint[] Screen { get { return _screenInternalBuffer; } }

    public bool TileBase
    {
      get { return _disStat.TileBase; }
      set { _disStat.TileBase = value; }
    }
    public bool NoTileMap
    {
      get { return _disStat.NoTileMap; }
      set { _disStat.NoTileMap = value; }
    }
    public bool TileMap
    {
      get { return _disStat.TileMap; }
      set { _disStat.TileMap = value; }
    }

    // Temporary buffer used or not allocating a local one on each iteration
    private uint[] _tempPixelBuffer;
    private uint[] _tempFrameLineBuffer;

    /// <summary>
    /// Display constructor.
    /// </summary>
    /// <param name="interruptController">A reference to the interrupt controller.</param>
    /// <param name="Memory">A reference to the memory.</param>
    public Display(InterruptController interruptController, Memory memory)
    {
      _interruptController = interruptController;
      _memory = memory;

      /*** DISPLAY DEFINITION ***/

      _disDef = new DisplayDefinition();
      _disDef.FramePixelCountX = 256;
      _disDef.FramePixelCountY = 256;
      _disDef.ScreenPixelCountX = 160;
      _disDef.ScreenPixelCountY = 144;
      _disDef.TimingPixelCountX = 256;
      _disDef.TimingPixelCountY = 154;
      _disDef.FrameTileCountX = 32;
      _disDef.FrameTileCountY = 32;
      _disDef.ScreenTileCountX = 20;
      _disDef.ScreenTileCountY = 18;
      _disDef.BytesPerTileShort = 16;
      _disDef.BytesPerTileLong = 32;
      _disDef.PixelPerTileX = 8;
      _disDef.PixelPerTileY = 8;
      _disDef.BytesPerPixel = 4;
      _disDef.PixelFormat = PixelFormat.Format32bppArgb;
      // TODO(Cristian): Output the color to the view for custom setting
      _disDef.TileColors = new uint[4]
      {
        0xFFFFFFFF,
        0xFFBBBBBB,
        0xFF666666,
        0xFF000000
      };
      _disDef.TilePallete = new uint[4];
      // TODO(Cristian): Output the color to the view for custom setting
      _disDef.SpriteColors = new uint[4]
      {
        0xFFFFFFFF,
        0xFFBBBBBB,
        0xFF666666,
        0xFF000000
      };
      _disDef.SpritePallete0 = new uint[4];
      _disDef.SpritePallete1 = new uint[4];

      // Tile stargets
      _spriteOAMs = new OAM[_spriteCount];
      for (int i = 0; i < _spriteOAMs.Length; ++i)
      {
        _spriteOAMs[i] = new OAM();
      }

      /*** DISPLAY STATUS ***/

      _disStat = new DisplayStatus();
      _disStat.PrevTickCount = 0;
      _disStat.CurrentLineTickCount = 0;
      _disStat.CurrentLine = 0;
      // NOTE(Cristian): This are default values when there are no sprites
      //                 They should change on runtime
      _disStat.OAMSearchTickCount = 83;
      _disStat.DataTransferTickCount = 83 + 175;
      _disStat.TotalLineTickCount = 456;
      _disStat.Enabled = true;
      // TODO(Cristian): Find out at what state the display starts!
      _disStat.DisplayMode = DisplayModes.Mode10;

      _disStat.TileBase = true;
      _disStat.NoTileMap = false;
      _disStat.TileMap = false;

      _disStat.LCDCBits = new bool[8];

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
      _debugInternalTargets = new uint[Enum.GetNames(typeof(DebugTargets)).Length][];
      _updateDebugTargets = new bool[Enum.GetNames(typeof(DebugTargets)).Length];

      _debugInternalTargets[(int)DebugTargets.Background] = new uint[_disDef.FramePixelCountX * _disDef.FramePixelCountY];
      _updateDebugTargets[(int)DebugTargets.Background] = false;

      _debugInternalTargets[(int)DebugTargets.Tiles] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _updateDebugTargets[(int)DebugTargets.Tiles] = false;

      _debugInternalTargets[(int)DebugTargets.Window] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _updateDebugTargets[(int)DebugTargets.Window] = false;

      _debugInternalTargets[(int)DebugTargets.SpriteLayer] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _updateDebugTargets[(int)DebugTargets.SpriteLayer] = false;

      _debugInternalTargets[(int)DebugTargets.DisplayTiming] = new uint[_disDef.TimingPixelCountX * _disDef.TimingPixelCountY];
      _updateDebugTargets[(int)DebugTargets.DisplayTiming] = false;

      _screenInternalBuffer = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _sprite = new uint[8 * 16];

      _tempPixelBuffer = new uint[_disDef.PixelPerTileX];
      _tempFrameLineBuffer = new uint[_disDef.FramePixelCountX];

      GeneratePixelLookupTable();

      // We update the display status info
      UpdateDisplayLineInfo(false);
    }

    private short[] _pixelLookupTable;
    private void GeneratePixelLookupTable()
    {
      _pixelLookupTable = new short[0x100 * 0x100];
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

          _pixelLookupTable[(top << 8) | bottom] = (short)lookup;
        }
      }

    }

    internal void LoadSprites()
    {
      // We load the OAMs
      ushort spriteOAMAddress = 0xFE00;
      byte[] tempArray;
      for (int i = 0; i < _spriteCount; i++)
      {
        tempArray = _memory.LowLevelArrayRead(spriteOAMAddress, 4);
        _spriteOAMs[i].Index = _spriteCount;
        _spriteOAMs[i].X = tempArray[1];
        _spriteOAMs[i].Y = tempArray[0];
        _spriteOAMs[i].SpriteCode = tempArray[2];
        _spriteOAMs[i].Flags = tempArray[3];

        spriteOAMAddress += 4;
      }

      // Now we sort them according to sprite priority rules
      Array.Sort<OAM>(_spriteOAMs, 
                      (a, b) => (a.X == b.X) ? (a.Index - b.Index) : (a.X - b.X));
    }

    internal void HandleMemoryChange(MMR mappedRegister, byte value)
    {
      switch(mappedRegister)
      {
        case MMR.LCDC:
          // We set all the LCDC bits
          for (int i = 0; i < 8; ++i)
          {
            _disStat.LCDCBits[i] = (value & (1 << i)) != 0;
          }
          break;
        case MMR.STAT:
          _disStat.STAT = value;
          break;
        case MMR.SCY:
          _disStat.SCY = value;
          break;
        case MMR.SCX:
          _disStat.SCX = value;
          break;
        case MMR.LY:
          _disStat.CurrentLine = 0;
          break;
        case MMR.LYC:
          _disStat.LYC = value;
          break;
        case MMR.DMA:
          LoadSprites(); 
          break;
        case MMR.BGP:
          DisFuncs.SetupTilePallete(_disDef, _memory);
          break;
        case MMR.OBP0:
          DisFuncs.SetupSpritePalletes(_disDef, _memory, MMR.OBP0);
          break;
        case MMR.OBP1:
          DisFuncs.SetupSpritePalletes(_disDef, _memory, MMR.OBP1);
          break;
        case MMR.WY:
          _disStat.WY = value;
          break;
        case MMR.WX:
          _disStat.WX = value;
          break;
        default:
          throw new InvalidProgramException("All cases should be handled...");
      }
    }

    public uint[] GetSprite(int index)
    {
      OAM oam = GetOAM(index);
      DrawSprite(_sprite, oam.SpriteCode, 0, 0);
      return _sprite;
    }

    internal void DrawSprite(uint[] spriteData, int spriteCode, int pX, int pY)
    {
      DrawFuncs.DrawTransparency(_disDef, spriteData, 8, 0, 0, 8, 16);

      if(_disStat.LCDCBits[2])
      {
        spriteCode = spriteCode & 0xFE; // We remove the last bit
      }

      // We draw the top part
      byte[] pixels = DisFuncs.GetTileData(_disDef, _memory, 0x8000, spriteCode, _disStat.LCDCBits[2]);
      DrawFuncs.DrawTile(_pixelLookupTable, 
                         _disDef, spriteData, _tempPixelBuffer, 
                         8, pixels, pX, pY,
                         _disDef.ScreenPixelCountX, _disDef.ScreenPixelCountY);
    }

    private void StartFrame()
    {
      _disStat.CurrentLine = 0;
      _disStat.CurrentWY = _disStat.WY;

      if(!_disStat.Enabled) { return; }

      // WINDOW TRANSPARENCY
      if (_updateDebugTargets[(int)DebugTargets.Window])
      {
        DrawFuncs.DrawTransparency(_disDef, _debugInternalTargets[(int)DebugTargets.Window],
                                  _disDef.ScreenPixelCountX,
                                  0, 0,
                                  _disDef.ScreenPixelCountX, _disDef.ScreenPixelCountY);
      }

      // SPRITES TRANSPARENCY
      if (_updateDebugTargets[(int)DebugTargets.SpriteLayer])
      {
        DrawFuncs.DrawTransparency(_disDef, _debugInternalTargets[(int)DebugTargets.SpriteLayer],
                                  _disDef.ScreenPixelCountX,
                                  0, 0,
                                  _disDef.ScreenPixelCountX, _disDef.ScreenPixelCountY);
      }
    }

    public void DrawFrame(int rowBegin, int rowEnd)
    {
      if(!_disStat.Enabled) { return; }
      if (rowBegin > 143) { return; }

      #region BACKGROUND

      bool drawBackground = _disStat.LCDCBits[0];
      // We copy the information from the background tile to the effective screen
      for (int y = rowBegin; y < rowEnd; y++)
      {

        // We obtain the correct row
        int bY = (y + _disStat.SCY) % _disDef.FramePixelCountY;
        DisFuncs.GetRowPixels(ref _tempFrameLineBuffer,
                              _pixelLookupTable,
                              _disDef, _memory, 
                              _tempPixelBuffer, 
                              bY, _disStat.LCDCBits[3], _disStat.LCDCBits[4]);

        if(_updateDebugTargets[(int)DebugTargets.Background])
        {
          DrawFuncs.DrawLine(_disDef, _debugInternalTargets[(int)DebugTargets.Background],
                             _disDef.FramePixelCountX,
                             _tempFrameLineBuffer,
                             0, bY,
                             0, _disDef.FramePixelCountX);
        }

        if (drawBackground)
        {
          DrawFuncs.DrawLine(_disDef, _screenInternalBuffer, 
                             _disDef.ScreenPixelCountX, 
                             _tempFrameLineBuffer,
                             0, y,
                             _disStat.SCX, _disDef.FramePixelCountX,
                             false, true);
        }
      }

      #endregion

      #region WINDOW

      int rWX = _disStat.WX - 7; // The window pos is (WX - 7, WY)

      // TODO(Cristian): If BG display is off, it actually prints white
      bool drawWindow = _disStat.LCDCBits[5];
      for (int row = rowBegin; row < rowEnd; row++)
      {
        if ((row >= _disStat.CurrentWY) && (row < 144))
        {
          // The offset indexes represent that the window is drawn from it's beggining
          // at (WX, WY)
          DisFuncs.GetRowPixels(ref _tempFrameLineBuffer,
                                _pixelLookupTable,
                                _disDef, _memory, _tempPixelBuffer,
                                row - _disStat.CurrentWY, 
                                _disStat.LCDCBits[6], _disStat.LCDCBits[4]);

          // Independent target
          if(_updateDebugTargets[(int)DebugTargets.Window])
          {
            DrawFuncs.DrawLine(_disDef, _debugInternalTargets[(int)DebugTargets.Window],
                               _disDef.ScreenPixelCountX,
                               _tempFrameLineBuffer,
                               rWX, row,
                               0, _disDef.ScreenPixelCountX - rWX);
          }

          // Screen target
          if (drawWindow)
          {
            DrawFuncs.DrawLine(_disDef, _screenInternalBuffer, _disDef.ScreenPixelCountX,
                               _tempFrameLineBuffer,
                               rWX, row,
                               0, _disDef.ScreenPixelCountX - rWX);
          }
        }
      }

      #endregion

      #region SPRITES

      bool drawSprites = _disStat.LCDCBits[1];
      for (int row = rowBegin; row < rowEnd; row++)
      {
        if(_updateDebugTargets[(int)DebugTargets.SpriteLayer])
        {
          // Independent target
          uint[] pixels = new uint[_disDef.ScreenPixelCountX];
          DisFuncs.GetSpriteRowPixels(_pixelLookupTable,
                                      _disDef, _memory, _spriteOAMs, pixels,
                                      _tempPixelBuffer,
                                      row, _disStat.LCDCBits[2],
                                      true);
          DrawFuncs.DrawLine(_disDef, _debugInternalTargets[(int)DebugTargets.SpriteLayer],
                            _disDef.ScreenPixelCountX,
                            pixels,
                            0, row,
                            0, _disDef.ScreenPixelCountX);
        }

        // Screen Target
        if (drawSprites)
        {
          DisFuncs.GetPixelRowFromBitmap(ref _tempFrameLineBuffer, 
                                         _disDef, _screenInternalBuffer, 
                                         row, _disDef.ScreenPixelCountX);
          DisFuncs.GetSpriteRowPixels(_pixelLookupTable,
                                      _disDef, _memory, _spriteOAMs, 
                                      _tempFrameLineBuffer, _tempPixelBuffer,
                                      row, _disStat.LCDCBits[2]);
          DrawFuncs.DrawLine(_disDef, _screenInternalBuffer, _disDef.ScreenPixelCountX,
                            _tempFrameLineBuffer,
                            0, row,
                            0, _disDef.ScreenPixelCountX);
        }
      }

      #endregion
    }

    private void EndFrame()
    {
      if (_disStat.Enabled)
      {
        if (_updateDebugTargets[(int)DebugTargets.Background])
        {
          DrawFuncs.DrawRectangle(_disDef, _debugInternalTargets[(int)DebugTargets.Background],
                                 _disDef.FramePixelCountX,
                                 _disStat.SCX, _disStat.SCY,
                                 _disDef.ScreenPixelCountX, _disDef.ScreenPixelCountY,
                                 _disDef.RectangleColor);
        }

        if (_updateDebugTargets[(int)DebugTargets.Tiles])
        {
          DrawTiles();
        }
      }

      FrameReady();
    }

    public void DrawTiles()
    {
      ushort tileBaseAddress = DisFuncs.GetTileBaseAddress(_disStat.TileBase);
      ushort tileMapBaseAddress = DisFuncs.GetTileMapBaseAddress(_disStat.TileMap);
      for (int tileY = 0; tileY < 18; ++tileY)
      {
        for (int tileX = 0; tileX < 20; ++tileX)
        {
          int tileOffset;
          if(_disStat.NoTileMap)
          {
            tileOffset = 16 * tileY + tileX;
          }
          else
          {
            tileOffset = DisFuncs.GetTileOffset(_disDef, _memory, tileMapBaseAddress,
                                                  _disStat.TileBase, tileX, tileY);
          }
          byte[] tileData = DisFuncs.GetTileData(_disDef, _memory, tileBaseAddress, tileOffset, false);

          DrawFuncs.DrawTile(_pixelLookupTable,
                             _disDef, _debugInternalTargets[(int)DebugTargets.Tiles],
                             _tempPixelBuffer,
                             _disDef.ScreenPixelCountX, tileData,
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
      bool activate = _disStat.LCDCBits[7];

      if(activate && !_disStat.Enabled) // We need to turn on the LCD
      {
        _disStat.Enabled = true;
      }
      if(!activate && _disStat.Enabled) // We need to turn off the LCD
      {
        if(_disStat.CurrentLine < 144)
        {
          // NOTE(Cristian): Turning off the gameboy *should* be made only during V-BLANK.
          //                 Apparently it damages the hardware otherwise.
          throw new InvalidOperationException("Stopping LCD should be made during V-BLANK");
        }
        _disStat.Enabled = false;
      }

      /**
       * We want to advance the display according to the tick count
       * So the simulation is to decrease the tick count and simulating
       * the display accordingly
       **/
      _disStat.PrevTickCount = _disStat.CurrentLineTickCount;
      while(ticks > 0)
      {
        // We try to advance to the next state
        // The display behaves differently if it's on V-BLANK or not
        if(_disStat.DisplayMode != DisplayModes.Mode01)
        {
          if(_disStat.DisplayMode == DisplayModes.Mode10)
          {
            if(CalculateTickChange(_disStat.OAMSearchTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode11);
            }
          }
          else if(_disStat.DisplayMode == DisplayModes.Mode11)
          {
            if(CalculateTickChange(_disStat.DataTransferTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode00);
              DrawFrame(_disStat.CurrentLine, _disStat.CurrentLine + 1);
            }
          }
          else if(_disStat.DisplayMode == DisplayModes.Mode00)
          {
            if(CalculateTickChange(_disStat.TotalLineTickCount, ref ticks))
            {
              // We start a new line
              UpdateDisplayLineInfo();
              if(_disStat.CurrentLine < 144) // We continue on normal mode
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
          if (CalculateTickChange(_disStat.TotalLineTickCount, ref ticks))
          {
            UpdateDisplayLineInfo();
            if (_disStat.CurrentLine >= 154)
            {
              ChangeDisplayMode(DisplayModes.Mode10);
              EndFrame();
              StartFrame();
            }
          }
        }
      }

      if(_updateDebugTargets[(int)DebugTargets.DisplayTiming])
      {
        DrawTiming();
      }

      if(firstRun)
      {
        firstRun = false;
        _disStat.CurrentWY = 0;
      }
    }

    internal void DrawTiming()
    {
      // This probably means an empty step (breakpoint)
      if (_disStat.PrevTickCount == _disStat.CurrentLineTickCount) { return; }

      int beginX = (int)(pixelsPerTick * _disStat.PrevTickCount);
      int endX = (int)(pixelsPerTick * _disStat.CurrentLineTickCount);
      if (beginX >= 256 || endX >= 256)
      {
        return;
      }

      if ((_disStat.CurrentLine == 0) &&
         ((endX <= beginX) || firstRun))
      {
        //DisplayFunctions.DrawTransparency(disDef, displayTimingBmpData, 0, 0, 256, 154);
        DrawFuncs.DrawRectangle(_disDef, _debugInternalTargets[(int)DebugTargets.DisplayTiming],
                               _disDef.TimingPixelCountX,
                               0, 0,
                               _disDef.TimingPixelCountX, _disDef.TimingPixelCountY,
                               0xFF000000, true);
      }

      byte mode = (byte)_disStat.DisplayMode;
      uint color = 0xFFFFFF00;
      if (mode == 1) { color = 0xFFFF0000; }
      if (mode == 2) { color = 0xFF00FF00; }
      if (mode == 3) { color = 0xFF0000FF; }

      int rowIndex = _disStat.CurrentLine * _disDef.TimingPixelCountX;

      for (int i = beginX; i < endX; ++i)
      {
        _debugInternalTargets[(int)DebugTargets.DisplayTiming][rowIndex + i] = color;
      }
    }

    // Returns whether the tick count is enough to get to the target
    private bool CalculateTickChange(int target, ref int ticks)
    {
      if (_disStat.CurrentLineTickCount > target)
      {
        throw new ArgumentOutOfRangeException("currentLineTickCount in invalid state");
      }

      int remainder = target - _disStat.CurrentLineTickCount;
      if(ticks >= remainder)
      {
        // We got to the target
        _disStat.CurrentLineTickCount += remainder;
        ticks -= remainder;
        return true;
      }
      else
      {
        _disStat.CurrentLineTickCount += ticks;
        ticks = 0;
        return false;
      }
    }

    private void ChangeDisplayMode(DisplayModes newDisplayMode)
    {
      _disStat.DisplayMode = newDisplayMode;
      byte byteDisplayMode = (byte)_disStat.DisplayMode;

      if(!_disStat.Enabled) { return; }

      // We strip the last 2 bits of STAT and replace them with the mode
      _disStat.STAT = (byte)((_disStat.STAT & 0xFC) | byteDisplayMode);
      this._memory.LowLevelWrite((ushort)MMR.STAT, _disStat.STAT);

      // We check if we have to trigger vertical blanking
      if(_disStat.DisplayMode == DisplayModes.Mode01) // We just change to V-BLANK Mode
      {
        _interruptController.SetInterrupt(Interrupts.VerticalBlanking);
      }

      // NOTE(Cristian): The bits that determine which interrupt is enabled
      //                 are ordered in the same numbers that the mode numbering
      //                 So basically we can shift by the mode numbering to get
      //                 the corresponding bit for the current mode being changed.
      //                 Bit 5: Mode 10
      // 								 Bit 4: Mode 01
      // 								 Bit 3: Mode 00
      if ((newDisplayMode != DisplayModes.Mode11) &&
          ((_disStat.STAT >> (byteDisplayMode + 3)) & 1) == 1)
      {
        _interruptController.SetInterrupt(Interrupts.LCDCStatus);
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
        _disStat.CurrentLineTickCount = 0;
        ++_disStat.CurrentLine;
      }

      if(!_disStat.Enabled) { return; }

      this._memory.LowLevelWrite((ushort)MMR.LY, _disStat.CurrentLine);

      // We update the STAT corresponding to the LY=LYC coincidence
      if (_disStat.LYC == _disStat.CurrentLine)
      {
        byte STATMask = 0x04; // Bit 2 is set 1
        _disStat.STAT = (byte)(_disStat.STAT | STATMask);

        if(Utils.UtilFuncs.TestBit(_disStat.STAT, 6) != 0)
        {
          _interruptController.SetInterrupt(Interrupts.LCDCStatus);
        }
      }
      else
      {
        byte STATMask = 0xFB; // Bit 2 is set to 0 
        _disStat.STAT = (byte)(_disStat.STAT & STATMask);
      }

      this._memory.LowLevelWrite((ushort)MMR.STAT, _disStat.STAT);
    }
  }
}
