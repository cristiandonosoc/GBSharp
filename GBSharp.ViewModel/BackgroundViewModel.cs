using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class BackgroundViewModel : ViewModelBase, IDisposable
  {
    private readonly IDispatcher _dispatcher;

    private readonly IDisplay _display;
    private readonly IGameBoy _gameBoy;

    private bool _updateBackground;

    public bool UpdateBackground
    {
      get { return _updateBackground; }
      set
      {
        if (_updateBackground == value)
        {
          return;
        }
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

    public BackgroundViewModel(IGameBoy gameboy, IDisplay display, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      _gameBoy.RefreshScreen += OnRefreshScreen;
      _dispatcher = dispatcher;

      var disDef = _display.GetDisplayDefinition();

      _background = new WriteableBitmap(disDef.framePixelCountX, disDef.framePixelCountY,
          96, 96, PixelFormats.Bgra32, null);

    }

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      UpdateBackground = _display.GetUpdateDebugTarget(DebugTargets.Background);
      if (UpdateBackground)
      {
        Utils.TransferBytesToWriteableBitmap(_background,
            _display.GetDebugTarget(DebugTargets.Background));
      }
    }

    public void Dispose()
    {
      _gameBoy.RefreshScreen -= OnRefreshScreen;
    }
  }
}