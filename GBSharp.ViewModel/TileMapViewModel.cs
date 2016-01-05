using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class TileMapViewModel : ViewModelBase, IDisposable
  {
    private readonly IDispatcher _dispatcher;
    private readonly IDisplay _display;
    private readonly IGameBoy _gameBoy;

    private bool _updateTiles;

    public bool UpdateTiles
    {
      get { return _updateTiles; }
      set
      {
        if (_updateTiles == value)
        {
          return;
        }
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

    public bool TileBase
    {
      get { return _tileBase; }
      set
      {
        if (_tileBase == value)
        {
          return;
        }
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
      else if (tileMap)
      {
        TileMap = TileMapOptions.x9C00;
      }
      else
      {
        TileMap = TileMapOptions.x9800;
      }
    }

    private TileMapOptions _tileMapOption;

    public TileMapOptions TileMap
    {
      get { return _tileMapOption; }
      set
      {
        if (_tileMapOption == value)
        {
          return;
        }
        _tileMapOption = value;

        switch (_tileMapOption)
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

    public TileMapViewModel(IGameBoy gameboy, IDisplay display, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      //_gameBoy.FrameCompleted += OnFrameCompleted;
      _dispatcher = dispatcher;

      var disDef = _display.GetDisplayDefinition();

      _tiles = new WriteableBitmap(disDef.ScreenPixelCountX, disDef.ScreenPixelCountY,
        96, 96, PixelFormats.Bgra32, null);


    }

    private void OnFrameCompleted()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      UpdateTiles = _display.GetUpdateDebugTarget(DebugTargets.Tiles);
      if (UpdateTiles)
      {
        TileBase = _display.TileBase;
        SetTileMapOptionFromBool(_display.NoTileMap, _display.TileMap);
        Utils.TransferBytesToWriteableBitmap(_tiles,
          _display.GetDebugTarget(DebugTargets.Tiles));
        OnPropertyChanged(() => Tiles);
      }
    }

    public void Dispose()
    {
      //_gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}