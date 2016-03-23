using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class DisplayTimingViewModel : ViewModelBase, IDisposable
  {
    private readonly IDispatcher _dispatcher;
    private readonly IDisplay _display;
    private readonly IGameBoy _gameBoy;

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
        if (_updateDisplayTiming == value) { return; }
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

    public DisplayTimingViewModel(IGameBoy gameboy, IDisplay display, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      //_gameBoy.FrameCompleted += OnFrameCompleted;
      _dispatcher = dispatcher;

      var disDef = _display.GetDisplayDefinition();
      _displayTiming = new WriteableBitmap(disDef.TimingPixelCountX, disDef.TimingPixelCountY,
                                           96, 96, PixelFormats.Bgra32, null);
    }

    private void OnFrameCompleted()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void CopyFromDomain()
    {
      UpdateDisplayTiming = _display.GetUpdateDebugTarget(DebugTargets.DisplayTiming);
      if (UpdateDisplayTiming)
      {
        State disStat = _display.GetState();
        Enabled = disStat.Enabled;
        CurrentLine = disStat.CurrentLine;
        DisplayMode = disStat.DisplayMode;
        PrevTickCount = disStat.PrevTickCount;
        CurrentTickCount = disStat.CurrentLineTickCount;
        Utils.TransferBytesToWriteableBitmap(_displayTiming,
                                             _display.GetDebugTarget(DebugTargets.DisplayTiming));
        OnPropertyChanged(() => DisplayTiming);
      }
    }

    public void Dispose()
    {
      //_gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}