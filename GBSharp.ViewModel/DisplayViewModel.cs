using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GBSharp.MemorySpace;
using GBSharp.VideoSpace;
using System.Windows.Media;

namespace GBSharp.ViewModel
{
  public enum TileMapOptions
  {
    None,
    x9800,
    x9C00
  };


  public class DisplayViewModel : ViewModelBase, IDisposable
  {
    public event Action UpdateDisplay;

    private IDispatcher _dispatcher;

    private readonly IDisplay _display;
    private readonly IMemory _memory;

    #region BACKGROUND

    private bool _updateBackground;
    public bool UpdateBackground
    {
      get { return _updateBackground; }
      set
      {
        if(_updateBackground == value) { return; }
        _updateBackground = value;
        _display.SetUpdateDebugTarget(DebugTargets.Background, value);
      }
    }
    private WriteableBitmap _background;
    public WriteableBitmap Background
    {
      get { return _background; }
      set
      {
        _background = value;
        OnPropertyChanged(() => Background);
      }
    }

    #endregion

    #region TILES

    private bool _updateTiles;
    public bool UpdateTiles
    {
      get { return _updateTiles; }
      set
      {
        if(_updateTiles == value) { return; }
        _updateTiles = value;
        _display.SetUpdateDebugTarget(DebugTargets.Tiles, value);
      }
    }
    private WriteableBitmap _tiles;
    public WriteableBitmap Tiles
    {
      get { return _tiles; }
      set
      {
        _tiles = value;
        OnPropertyChanged(() => Tiles);
      }
    }

    private bool _tileBase;
    public bool TileBase {
      get { return _tileBase; }
      set
      {
        if(_tileBase == value) { return; }
        _tileBase = value;
        _display.TileBase = value;
        _display.DrawTiles();
        CopyFromDomain();
        OnPropertyChanged(() => TileBase);
      }
    }

    private void SetTileMapOptionFromBool(bool noTileMap, bool tileMap)
    {
      if (noTileMap)
      {
        TileMap = TileMapOptions.None;
      }
      else if(tileMap)
      {
        TileMap = TileMapOptions.x9C00;
      }
      else
      {
        TileMap = TileMapOptions.x9800;
      }
    }

    private TileMapOptions _tileMapOption;
    public TileMapOptions TileMap {
      get
      {
        return _tileMapOption;
      }
      set
      {
        if(_tileMapOption == value) { return; }
        _tileMapOption = value;

        switch(_tileMapOption)
        {
          case TileMapOptions.None:
            _display.NoTileMap = true;
            break;
          case TileMapOptions.x9800:
            _display.NoTileMap = false;
            _display.TileMap = false;
            break;
          case TileMapOptions.x9C00:
            _display.NoTileMap = false;
            _display.TileMap = true;
            break;
        }
        CopyFromDomain();
        _display.DrawTiles();
        OnPropertyChanged(() => TileMap);
      }
    }

    #endregion

    #region WINDOW

    private bool _updateWindow;
    public bool UpdateWindow
    {
      get { return _updateWindow; }
      set
      {
        if(_updateWindow == value) { return; }
        _updateWindow = value;
        _display.SetUpdateDebugTarget(DebugTargets.Window, value);
      }
    }
    private WriteableBitmap _window;
    public WriteableBitmap Window
    {
      get { return _window; }
      set
      {
        _window = value;
        OnPropertyChanged(() => Window);
      }
    }

    #endregion

    #region SPRITES

    public bool UpdateSprites { get; set; }
    private SpriteViewModel _selectedSprite;
    public SpriteViewModel SelectedSprite 
    {
      get { return _selectedSprite; }
      set
      {
        if (_selectedSprite != value)
        {
          _selectedSprite = value;
          OnPropertyChanged(() => SelectedSprite);
        }
      }
    }

    private readonly ObservableCollection<SpriteViewModel> _sprites = new ObservableCollection<SpriteViewModel>();
    public ObservableCollection<SpriteViewModel> Sprites
    {
      get { return _sprites; }
    }

    #endregion

    #region SPRITE LAYER

    private bool _updateSpriteLayer;
    public bool UpdateSpriteLayer
    {
      get { return _updateSpriteLayer; }
      set
      {
        if(_updateSpriteLayer == value) { return; }
        _updateSpriteLayer = value;
        _display.SetUpdateDebugTarget(DebugTargets.SpriteLayer, value);
      }
    }
    private WriteableBitmap _spriteLayer;
    public WriteableBitmap SpriteLayer
    {
      get { return _spriteLayer; }
      set
      {
        _spriteLayer = value;
        OnPropertyChanged(() => SpriteLayer);
      }
    }

    #endregion

    #region DISPLAY TIMING

    private bool _enabled;
    public bool Enabled
    {
      get { return _enabled; }
      set
      {
        _enabled = value;
        OnPropertyChanged(() => Enabled);
      }
    }

    private int _displayCurrentLine;
    public int CurrentLine
    {
      get { return _displayCurrentLine; }
      set
      {
        _displayCurrentLine = value;
        OnPropertyChanged(() => CurrentLine);
      }
    }

    private DisplayModes _displayMode;
    public DisplayModes DisplayMode
    {
      get { return _displayMode; }
      set
      {
        _displayMode = value;
        OnPropertyChanged(() => DisplayMode);
      }
    }

    private int _prevTickCount;
    public int PrevTickCount
    {
      get { return _prevTickCount; }
      set
      {
        _prevTickCount = value;
        OnPropertyChanged(() => PrevTickCount);
      }
    }

    private int _currentTickCount;
    public int CurrentTickCount
    {
      get { return _currentTickCount; }
      set
      {
        _currentTickCount = value;
        OnPropertyChanged(() => CurrentTickCount);
      }
    }

    private bool _updateDisplayTiming;
    public bool UpdateDisplayTiming
    {
      get { return _updateDisplayTiming; }
      set
      {
        if(_updateDisplayTiming == value) { return; }
        _updateDisplayTiming = value;
        _display.SetUpdateDebugTarget(DebugTargets.DisplayTiming, value);
      }
    }
    private WriteableBitmap _displayTiming;
    public WriteableBitmap DisplayTiming
    {
      get { return _displayTiming; }
      set
      {
        _displayTiming = value;
        OnPropertyChanged(() => DisplayTiming);
      }
    }

    #endregion

    #region COMMANDS

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    public ICommand ScrollXDecreaseCommand
    {
      get { return new DelegateCommand(ScrollXDecrease); }
    }

    public ICommand ScrollXIncreaseCommand
    {
      get { return new DelegateCommand(ScrollXIncrease); }
    }

    public ICommand ScrollYDecreaseCommand
    {
      get { return new DelegateCommand(ScrollYDecrease); }
    }

    public ICommand ScrollYIncreaseCommand
    {
      get { return new DelegateCommand(ScrollYIncrease); }
    }

    #endregion

    IGameBoy _gameBoy;

    public DisplayViewModel(IGameBoy gameboy, IDisplay display, IMemory memory, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      _gameBoy.RefreshScreen += OnRefreshScreen;
      _memory = memory;
      _dispatcher = dispatcher;

      var disDef = _display.GetDisplayDefinition();

      _background = new WriteableBitmap(disDef.framePixelCountX, disDef.framePixelCountY,
                                        96, 96, PixelFormats.Bgra32, null);
      _window = new WriteableBitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, 
                                    96, 96, PixelFormats.Bgra32, null);
      _spriteLayer = new WriteableBitmap(disDef.screenPixelCountX, disDef.screenPixelCountY, 
                                         96, 96, PixelFormats.Bgra32, null);
      _displayTiming = new WriteableBitmap(disDef.timingPixelCountX, disDef.timingPixelCountY, 
                                           96, 96, PixelFormats.Bgra32, null);

      _tiles = new WriteableBitmap(disDef.screenPixelCountX, disDef.screenPixelCountY,
                                   96, 96, PixelFormats.Bgra32, null);

      for (int i = 0; i < 40; i++)
      {
        Sprites.Add(new SpriteViewModel());
      }
      SelectedSprite = Sprites.First();
    }

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      UpdateBackground = _display.GetUpdateDebugTarget(DebugTargets.Background);
      if(UpdateBackground)
      {
        Utils.TransferBytesToWriteableBitmap(_background,
                                             _display.GetDebugTarget(DebugTargets.Background));
      }

      UpdateTiles = _display.GetUpdateDebugTarget(DebugTargets.Tiles);
      if (UpdateTiles)
      {
        TileBase = _display.TileBase;
        SetTileMapOptionFromBool(_display.NoTileMap, _display.TileMap);
        Utils.TransferBytesToWriteableBitmap(_tiles,
                                             _display.GetDebugTarget(DebugTargets.Tiles));
        OnPropertyChanged(() => Tiles);
      }

      UpdateWindow = _display.GetUpdateDebugTarget(DebugTargets.Window);
      if (UpdateWindow)
      {
        Utils.TransferBytesToWriteableBitmap(_window,
                                             _display.GetDebugTarget(DebugTargets.Window));
        OnPropertyChanged(() => Window);
      }

      if (UpdateSprites)
      {
        for (int i = 0; i < 40; i++)
        {
          Sprites[i].RefreshSprite(_display.GetSprite(i), _display.GetOAM(i));
        }
      }

      UpdateSpriteLayer = _display.GetUpdateDebugTarget(DebugTargets.SpriteLayer);
      if (UpdateSpriteLayer)
      {
        Utils.TransferBytesToWriteableBitmap(_spriteLayer,
                                             _display.GetDebugTarget(DebugTargets.SpriteLayer));
        OnPropertyChanged(() => SpriteLayer);
      }

      UpdateDisplayTiming = _display.GetUpdateDebugTarget(DebugTargets.DisplayTiming);
      if (UpdateDisplayTiming)
      {
        DisplayStatus disStat = _display.GetDisplayStatus();
        Enabled = disStat.enabled;
        CurrentLine = disStat.currentLine;
        DisplayMode = disStat.displayMode;
        PrevTickCount = disStat.prevTickCount;
        CurrentTickCount = disStat.currentLineTickCount;
        Utils.TransferBytesToWriteableBitmap(_displayTiming,
                                             _display.GetDebugTarget(DebugTargets.DisplayTiming));
        OnPropertyChanged(() => DisplayTiming);
      }

      NotifyUpdateDisplay();
    }

    private void NotifyUpdateDisplay()
    {
      if (UpdateDisplay != null)
        UpdateDisplay();
    }

    private void CopyToDomain()
    {

    }

    private void ScrollXDecrease()
    {
      ushort access = (ushort)MemoryMappedRegisters.SCX;
      byte[] mem = _memory.Data;
      mem[access] = --mem[access];
    }

    private void ScrollXIncrease()
    {
      ushort access = (ushort)MemoryMappedRegisters.SCX;
      byte[] mem = _memory.Data;
      mem[access] = ++mem[access];
    }

    private void ScrollYDecrease()
    {
      ushort access = (ushort)MemoryMappedRegisters.SCY;
      byte[] mem = _memory.Data;
      mem[access] = ++mem[access];
    }

    private void ScrollYIncrease()
    {
      ushort access = (ushort)MemoryMappedRegisters.SCY;
      byte[] mem = _memory.Data;
      mem[access] = --mem[access];
    }

    public void Dispose()
    {
      _gameBoy.RefreshScreen -= OnRefreshScreen;
    }
  }
}