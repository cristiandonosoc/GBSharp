using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class SpriteLayerViewModel : ViewModelBase, IDisposable
  {
    private readonly IDispatcher _dispatcher;
    private readonly IDisplay _display;
    private readonly IGameBoy _gameBoy;

    private bool _updateSpriteLayer;
    public bool UpdateSpriteLayer
    {
      get { return _updateSpriteLayer; }
      set
      {
        if (_updateSpriteLayer == value) { return; }
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

    public SpriteLayerViewModel(IGameBoy gameboy, IDisplay display, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      //_gameBoy.FrameCompleted += OnFrameCompleted;
      _dispatcher = dispatcher;

      var disDef = _display.GetDisplayDefinition();

      _spriteLayer = new WriteableBitmap(disDef.ScreenPixelCountX, disDef.ScreenPixelCountY,
                                         96, 96, PixelFormats.Bgra32, null);
    }

    private void OnFrameCompleted()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {

      UpdateSpriteLayer = _display.GetUpdateDebugTarget(DebugTargets.SpriteLayer);
      if (UpdateSpriteLayer)
      {
        Utils.TransferBytesToWriteableBitmap(_spriteLayer,
                                             _display.GetDebugTarget(DebugTargets.SpriteLayer));
        OnPropertyChanged(() => SpriteLayer);
      }

    }

    public void Dispose()
    {
      //_gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}