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

  [Serializable]
  public class State
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

    private State _state;
    public State GetState()
    {
      return _state;
    }

    public void SetState(State state)
    {
      _state = state;
    }

    private bool[] _updateDebugTargets;
    private uint[][] _debugInternalTargets;
    private uint[][] _debugExternalTargets;

    public uint[] GetDebugTarget(DebugTargets debugTarget)
    {
      return _debugExternalTargets[(int)debugTarget];
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
    private uint[] _screenExternalBuffer;
    public uint[] Screen { get { return _screenExternalBuffer; } }

    public bool TileBase
    {
      get { return _state.TileBase; }
      set { _state.TileBase = value; }
    }
    public bool NoTileMap
    {
      get { return _state.NoTileMap; }
      set { _state.NoTileMap = value; }
    }
    public bool TileMap
    {
      get { return _state.TileMap; }
      set { _state.TileMap = value; }
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
      _disDef = new DisplayDefinition();
      _state = new State();
      GeneratePixelLookupTable();

      Reset();
    }

    internal void Reset()
    {
      /*** DISPLAY DEFINITION ***/

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

      _state.PrevTickCount = 0;
      _state.CurrentLineTickCount = 0;
      _state.CurrentLine = 0;
      // NOTE(Cristian): This are default values when there are no sprites
      //                 They should change on runtime
      _state.OAMSearchTickCount = 83;
      _state.DataTransferTickCount = 83 + 175;
      _state.TotalLineTickCount = 456;
      _state.Enabled = true;
      // TODO(Cristian): Find out at what state the display starts!
      _state.DisplayMode = DisplayModes.Mode10;

      _state.TileBase = true;
      _state.NoTileMap = false;
      _state.TileMap = false;

      _state.LCDCBits = new bool[8];

      // We start the registers correctly
      HandleMemoryChange(MMR.LCDC, _memory.LowLevelRead((ushort)MMR.LCDC));
      HandleMemoryChange(MMR.LCDC, _memory.LowLevelRead((ushort)MMR.LCDC));
      HandleMemoryChange(MMR.SCY, _memory.LowLevelRead((ushort)MMR.SCY));
      HandleMemoryChange(MMR.SCX, _memory.LowLevelRead((ushort)MMR.SCX));
      HandleMemoryChange(MMR.LYC, _memory.LowLevelRead((ushort)MMR.LYC));
      HandleMemoryChange(MMR.DMA, _memory.LowLevelRead((ushort)MMR.DMA));
      HandleMemoryChange(MMR.BGP, _memory.LowLevelRead((ushort)MMR.BGP));
      HandleMemoryChange(MMR.OBP0, _memory.LowLevelRead((ushort)MMR.OBP0));
      HandleMemoryChange(MMR.OBP1, _memory.LowLevelRead((ushort)MMR.OBP1));
      HandleMemoryChange(MMR.WY, _memory.LowLevelRead((ushort)MMR.WY));
      HandleMemoryChange(MMR.WX, _memory.LowLevelRead((ushort)MMR.WX));

      /*** DRAW TARGETS ***/

      // We create the target bitmaps
      _debugInternalTargets = new uint[Enum.GetNames(typeof(DebugTargets)).Length][];
      _debugExternalTargets = new uint[Enum.GetNames(typeof(DebugTargets)).Length][];
      _updateDebugTargets = new bool[Enum.GetNames(typeof(DebugTargets)).Length];

      _debugInternalTargets[(int)DebugTargets.Background] = new uint[_disDef.FramePixelCountX * _disDef.FramePixelCountY];
      _debugExternalTargets[(int)DebugTargets.Background] = new uint[_disDef.FramePixelCountX * _disDef.FramePixelCountY];
      _updateDebugTargets[(int)DebugTargets.Background] = false;

      _debugInternalTargets[(int)DebugTargets.Tiles] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _debugExternalTargets[(int)DebugTargets.Tiles] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _updateDebugTargets[(int)DebugTargets.Tiles] = false;

      _debugInternalTargets[(int)DebugTargets.Window] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _debugExternalTargets[(int)DebugTargets.Window] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _updateDebugTargets[(int)DebugTargets.Window] = false;

      _debugInternalTargets[(int)DebugTargets.SpriteLayer] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _debugExternalTargets[(int)DebugTargets.SpriteLayer] = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _updateDebugTargets[(int)DebugTargets.SpriteLayer] = false;

      _debugInternalTargets[(int)DebugTargets.DisplayTiming] = new uint[_disDef.TimingPixelCountX * _disDef.TimingPixelCountY];
      _debugExternalTargets[(int)DebugTargets.DisplayTiming] = new uint[_disDef.TimingPixelCountX * _disDef.TimingPixelCountY];
      _updateDebugTargets[(int)DebugTargets.DisplayTiming] = false;

      _screenInternalBuffer = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _screenExternalBuffer = new uint[_disDef.ScreenPixelCountX * _disDef.ScreenPixelCountY];
      _sprite = new uint[8 * 16];

      _tempPixelBuffer = new uint[_disDef.PixelPerTileX];
      _tempFrameLineBuffer = new uint[_disDef.FramePixelCountX];

      // We update the display status info
      UpdateDisplayLineInfo(false);
    }

    private short[] _pixelLookupTable;
    private void GeneratePixelLookupTable()
    {
      _pixelLookupTable = new short[0x100 * 0x100];
      for (int top = 0; top < 0x100; ++top)
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
      switch (mappedRegister)
      {
        case MMR.LCDC:
          // We set all the LCDC bits
          for (int i = 0; i < 8; ++i)
          {
            _state.LCDCBits[i] = (value & (1 << i)) != 0;
          }
          break;
        case MMR.STAT:
          _state.STAT = value;
          break;
        case MMR.SCY:
          _state.SCY = value;
          break;
        case MMR.SCX:
          _state.SCX = value;
          break;
        case MMR.LY:
          _state.CurrentLine = 0;
          break;
        case MMR.LYC:
          _state.LYC = value;
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
          _state.WY = value;
          break;
        case MMR.WX:
          _state.WX = value;
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

      if (_state.LCDCBits[2])
      {
        spriteCode = spriteCode & 0xFE; // We remove the last bit
      }

      // We draw the top part
      byte[] pixels = DisFuncs.GetTileData(_disDef, _memory, 0x8000, spriteCode, _state.LCDCBits[2]);
      DrawFuncs.DrawTile(_pixelLookupTable,
                         _disDef, spriteData, _tempPixelBuffer,
                         8, pixels, pX, pY,
                         _disDef.ScreenPixelCountX, _disDef.ScreenPixelCountY);
    }

    internal void StartFrame()
    {
      _state.CurrentLine = 0;
      _state.CurrentWY = _state.WY;

      if (!_state.Enabled) { return; }

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
      if (!_state.Enabled) { return; }
      if (rowBegin > 143) { return; }

      #region BACKGROUND

      bool drawBackground = _state.LCDCBits[0];
      // We copy the information from the background tile to the effective screen
      for (int y = rowBegin; y < rowEnd; y++)
      {

        // We obtain the correct row
        int bY = (y + _state.SCY) % _disDef.FramePixelCountY;
        DisFuncs.GetRowPixels(ref _tempFrameLineBuffer,
                              _pixelLookupTable,
                              _disDef, _memory,
                              _tempPixelBuffer,
                              bY, _state.LCDCBits[3], _state.LCDCBits[4]);

        if (_updateDebugTargets[(int)DebugTargets.Background])
        {
          // TODO(Cristian): Draw the whole debug target
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
                             _state.SCX, _disDef.FramePixelCountX,
                             false, true);
        }
      }

      #endregion

      #region WINDOW

      int rWX = _state.WX - 7; // The window pos is (WX - 7, WY)

      // TODO(Cristian): If BG display is off, it actually prints white
      bool drawWindow = _state.LCDCBits[5];
      for (int row = rowBegin; row < rowEnd; row++)
      {
        if ((row >= _state.CurrentWY) && (row < 144))
        {
          // The offset indexes represent that the window is drawn from it's beggining
          // at (WX, WY)
          DisFuncs.GetRowPixels(ref _tempFrameLineBuffer,
                                _pixelLookupTable,
                                _disDef, _memory, _tempPixelBuffer,
                                row - _state.CurrentWY,
                                _state.LCDCBits[6], _state.LCDCBits[4]);

          // Independent target
          if (_updateDebugTargets[(int)DebugTargets.Window])
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
            DrawFuncs.DrawLine(_disDef, _screenInternalBuffer, 
                               _disDef.ScreenPixelCountX,
                               _tempFrameLineBuffer,
                               rWX, row,
                               0, _disDef.ScreenPixelCountX - rWX);
          }
        }
      }

      #endregion

      #region SPRITES

      bool drawSprites = _state.LCDCBits[1];
      for (int row = rowBegin; row < rowEnd; row++)
      {
        if (_updateDebugTargets[(int)DebugTargets.SpriteLayer])
        {
          // Independent target
          uint[] pixels = new uint[_disDef.ScreenPixelCountX];
          DisFuncs.GetSpriteRowPixels(_pixelLookupTable,
                                      _disDef, _memory, _spriteOAMs, pixels,
                                      _tempPixelBuffer,
                                      row, _state.LCDCBits[2],
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
                                      row, _state.LCDCBits[2]);
          DrawFuncs.DrawLine(_disDef, _screenInternalBuffer, _disDef.ScreenPixelCountX,
                            _tempFrameLineBuffer,
                            0, row,
                            0, _disDef.ScreenPixelCountX);
        }
      }

      #endregion
    }

    internal void EndFrame()
    {
      if (_state.Enabled)
      {
        if (_updateDebugTargets[(int)DebugTargets.Background])
        {
          DrawFuncs.DrawRectangle(_disDef, _debugInternalTargets[(int)DebugTargets.Background],
                                 _disDef.FramePixelCountX,
                                 _state.SCX, _state.SCY,
                                 _disDef.ScreenPixelCountX, _disDef.ScreenPixelCountY,
                                 _disDef.RectangleColor);
        }

        if (_updateDebugTargets[(int)DebugTargets.Tiles])
        {
          DrawTiles();
        }
      }

    }

    public void DrawTiles()
    {
      ushort tileBaseAddress = DisFuncs.GetTileBaseAddress(_state.TileBase);
      ushort tileMapBaseAddress = DisFuncs.GetTileMapBaseAddress(_state.TileMap);
      for (int tileY = 0; tileY < 18; ++tileY)
      {
        for (int tileX = 0; tileX < 20; ++tileX)
        {
          int tileOffset;
          if (_state.NoTileMap)
          {
            tileOffset = 16 * tileY + tileX;
          }
          else
          {
            tileOffset = DisFuncs.GetTileOffset(_disDef, _memory, tileMapBaseAddress,
                                                  _state.TileBase, tileX, tileY);
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

    private bool firstRun = true;

    /// <summary>
    /// Simulates the update of the display for a period of time of a given number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks ellapsed since the last call.
    /// A tick is a complete source clock oscillation, ~238.4 ns (2^-22 seconds).</param>
    internal void Step(int ticks)
    {
      // We check if the display is supposed to run
      bool activate = _state.LCDCBits[7];

      if (activate && !_state.Enabled) // We need to turn on the LCD
      {
        _state.Enabled = true;
      }
      if (!activate && _state.Enabled) // We need to turn off the LCD
      {
        if (_state.CurrentLine < 144)
        {
          // NOTE(Cristian): Turning off the gameboy *should* be made only during V-BLANK.
          //                 Apparently it damages the hardware otherwise.
          throw new InvalidOperationException("Stopping LCD should be made during V-BLANK");
        }
        _state.Enabled = false;
      }

      /**
       * We want to advance the display according to the tick count
       * So the simulation is to decrease the tick count and simulating
       * the display accordingly
       **/
      _state.PrevTickCount = _state.CurrentLineTickCount;
      while (ticks > 0)
      {
        // We try to advance to the next state
        // The display behaves differently if it's on V-BLANK or not
        if (_state.DisplayMode != DisplayModes.Mode01)
        {
          if (_state.DisplayMode == DisplayModes.Mode10)
          {
            if (CalculateTickChange(_state.OAMSearchTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode11);
            }
          }
          else if (_state.DisplayMode == DisplayModes.Mode11)
          {
            if (CalculateTickChange(_state.DataTransferTickCount, ref ticks))
            {
              ChangeDisplayMode(DisplayModes.Mode00);
              DrawFrame(_state.CurrentLine, _state.CurrentLine + 1);
            }
          }
          else if (_state.DisplayMode == DisplayModes.Mode00)
          {
            if (CalculateTickChange(_state.TotalLineTickCount, ref ticks))
            {
              // We start a new line
              UpdateDisplayLineInfo();
              if (_state.CurrentLine < 144) // We continue on normal mode
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
          if (CalculateTickChange(_state.TotalLineTickCount, ref ticks))
          {
            UpdateDisplayLineInfo();
            if (_state.CurrentLine >= 154)
            {
              ChangeDisplayMode(DisplayModes.Mode10);
              FrameReady();
              //EndFrame();
              //StartFrame();
            }
          }
        }
      }

      if (_updateDebugTargets[(int)DebugTargets.DisplayTiming])
      {
        DrawTiming();
      }

      if (firstRun)
      {
        firstRun = false;
        _state.CurrentWY = 0;
      }
    }

    private double pixelsPerTick = (double)256 / (double)456;
    internal void DrawTiming()
    {
      // This probably means an empty step (breakpoint)
      if (_state.PrevTickCount == _state.CurrentLineTickCount) { return; }

      int beginX = (int)(pixelsPerTick * _state.PrevTickCount);
      int endX = (int)(pixelsPerTick * _state.CurrentLineTickCount);
      if (beginX >= 256 || endX >= 256)
      {
        return;
      }

      if ((_state.CurrentLine == 0) &&
         ((endX <= beginX) || firstRun))
      {
        //DisplayFunctions.DrawTransparency(disDef, displayTimingBmpData, 0, 0, 256, 154);
        DrawFuncs.DrawRectangle(_disDef, _debugInternalTargets[(int)DebugTargets.DisplayTiming],
                               _disDef.TimingPixelCountX,
                               0, 0,
                               _disDef.TimingPixelCountX, _disDef.TimingPixelCountY,
                               0xFF000000, true);
      }

      byte mode = (byte)_state.DisplayMode;
      uint color = 0xFFFFFF00;
      if (mode == 1) { color = 0xFFFF0000; }
      if (mode == 2) { color = 0xFF00FF00; }
      if (mode == 3) { color = 0xFF0000FF; }

      int rowIndex = _state.CurrentLine * _disDef.TimingPixelCountX;

      for (int i = beginX; i < endX; ++i)
      {
        _debugInternalTargets[(int)DebugTargets.DisplayTiming][rowIndex + i] = color;
      }
    }

    // Returns whether the tick count is enough to get to the target
    private bool CalculateTickChange(int target, ref int ticks)
    {
      if (_state.CurrentLineTickCount > target)
      {
        throw new ArgumentOutOfRangeException("currentLineTickCount in invalid state");
      }

      int remainder = target - _state.CurrentLineTickCount;
      if (ticks >= remainder)
      {
        // We got to the target
        _state.CurrentLineTickCount += remainder;
        ticks -= remainder;
        return true;
      }
      else
      {
        _state.CurrentLineTickCount += ticks;
        ticks = 0;
        return false;
      }
    }

    private void ChangeDisplayMode(DisplayModes newDisplayMode)
    {
      _state.DisplayMode = newDisplayMode;
      byte byteDisplayMode = (byte)_state.DisplayMode;

      if (!_state.Enabled) { return; }

      // We strip the last 2 bits of STAT and replace them with the mode
      _state.STAT = (byte)((_state.STAT & 0xFC) | byteDisplayMode);
      this._memory.LowLevelWrite((ushort)MMR.STAT, _state.STAT);

      // We check if we have to trigger vertical blanking
      if (_state.DisplayMode == DisplayModes.Mode01) // We just change to V-BLANK Mode
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
          ((_state.STAT >> (byteDisplayMode + 3)) & 1) == 1)
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
        _state.CurrentLineTickCount = 0;
        ++_state.CurrentLine;
      }

      if (!_state.Enabled) { return; }

      this._memory.LowLevelWrite((ushort)MMR.LY, _state.CurrentLine);

      // We update the STAT corresponding to the LY=LYC coincidence
      if (_state.LYC == _state.CurrentLine)
      {
        byte STATMask = 0x04; // Bit 2 is set 1
        _state.STAT = (byte)(_state.STAT | STATMask);

        if (Utils.UtilFuncs.TestBit(_state.STAT, 6) != 0)
        {
          _interruptController.SetInterrupt(Interrupts.LCDCStatus);
        }
      }
      else
      {
        byte STATMask = 0xFB; // Bit 2 is set to 0 
        _state.STAT = (byte)(_state.STAT & STATMask);
      }

      this._memory.LowLevelWrite((ushort)MMR.STAT, _state.STAT);
    }

    internal void CopyTargets()
    {
      Array.Copy(_screenInternalBuffer,
                 _screenExternalBuffer,
                 _screenInternalBuffer.Length);

      // We copy the debug targets
      for (int i = 0; i < 4; ++i)
      {
        if (_updateDebugTargets[i])
        {
          Array.Copy(_debugInternalTargets[i],
                     _debugExternalTargets[i],
                     _debugInternalTargets[i].Length);
        }
      }
    }
  }

}
