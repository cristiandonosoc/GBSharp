using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class WindowViewModel : ViewModelBase, IDisposable
  {
    private readonly IDispatcher _dispatcher;
    private readonly IDisplay _display;
    private readonly IGameBoy _gameBoy;

    private bool _updateWindow;
    public bool UpdateWindow
    {
      get { return _updateWindow; }
      set
      {
        if (_updateWindow == value) { return; }
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

    public WindowViewModel(IGameBoy gameboy, IDisplay display, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      _gameBoy.FrameCompleted += OnFrameCompleted;
      _dispatcher = dispatcher;

      var disDef = _display.GetDisplayDefinition();

      _window = new WriteableBitmap(disDef.screenPixelCountX, disDef.screenPixelCountY,
        96, 96, PixelFormats.Bgra32, null);
    }

    private void OnFrameCompleted()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      UpdateWindow = _display.GetUpdateDebugTarget(DebugTargets.Window);
      if (UpdateWindow)
      {
        Utils.TransferBytesToWriteableBitmap(_window,
          _display.GetDebugTarget(DebugTargets.Window));
        OnPropertyChanged(() => Window);
      }
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}