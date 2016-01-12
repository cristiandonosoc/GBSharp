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

  public class DisplayViewModel : ViewModelBase, IDisposable
  {
    public event Action UpdateDisplay;

    private readonly IDispatcher _dispatcher;
    private readonly IDisplay _display;
    private readonly IMemory _memory;
    private readonly IGameBoy _gameBoy;

    private readonly BackgroundViewModel _background;
    private readonly TileMapViewModel _tileMap;
    private readonly WindowViewModel _window;
    private readonly SpritesViewModel _sprites;
    private readonly SpriteLayerViewModel _spriteLayer;
    private readonly DisplayTimingViewModel _displayTiming;


    public DisplayViewModel(IGameBoy gameboy, IDisplay display, IMemory memory, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      _gameBoy.FrameCompleted += OnFrameCompleted;
      _memory = memory;
      _dispatcher = dispatcher;

      _background = new BackgroundViewModel(gameboy, display, dispatcher);
      _tileMap = new TileMapViewModel(gameboy, display, dispatcher);
      _window = new WindowViewModel(gameboy, display, dispatcher);
      _sprites = new SpritesViewModel(gameboy, display, memory, dispatcher);
      _spriteLayer = new SpriteLayerViewModel(gameboy, display, dispatcher);
      _displayTiming = new DisplayTimingViewModel(gameboy, display, dispatcher);

    }

    private void OnFrameCompleted()
    {
      _dispatcher.BeginInvoke(new Action(CopyFromDomain), null);
    }

    public BackgroundViewModel Background { get { return _background; } }
    public TileMapViewModel TileMap { get { return _tileMap; } }
    public WindowViewModel Window { get { return _window; } }
    public SpritesViewModel Sprites { get { return _sprites; } }
    public SpriteLayerViewModel SpriteLayer { get { return _spriteLayer; } }
    public DisplayTimingViewModel DisplayTiming { get { return _displayTiming; } }

    public void CopyFromDomain()
    {
      _background.CopyFromDomain();
      _tileMap.CopyFromDomain();
      _window.CopyFromDomain();
      _sprites.CopyFromDomain();
      _spriteLayer.CopyFromDomain();
      _displayTiming.CopyFromDomain();
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
      _background.Dispose();
      _tileMap.Dispose();
      _window.Dispose();
      _sprites.Dispose();
      _spriteLayer.Dispose();
      _displayTiming.Dispose();
    }
  }
}