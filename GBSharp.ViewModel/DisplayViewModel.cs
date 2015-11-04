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

    private IDispatcher _dispatcher;

    private readonly IDisplay _display;
    private readonly IMemory _memory;

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

    private readonly ObservableCollection<SpriteViewModel> _sprites = new ObservableCollection<SpriteViewModel>();

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

    public ObservableCollection<SpriteViewModel> Sprites
    {
      get { return _sprites; }
    }

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

   

    public DisplayViewModel(IDisplay display, IMemory memory, IDispatcher dispatcher)
    {
      _display = display;
      _display.RefreshScreen += OnRefreshScreen;
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
      Utils.TransferBytesToWriteableBitmap(_background, _display.Tiles);
      OnPropertyChanged(() => Background);

      Utils.TransferBytesToWriteableBitmap(_window, _display.Window);
      OnPropertyChanged(() => Window);

      Utils.TransferBytesToWriteableBitmap(_spriteLayer, _display.SpriteLayer);
      OnPropertyChanged(() => SpriteLayer);
 
      Utils.TransferBytesToWriteableBitmap(_displayTiming, _display.DisplayTiming);
      OnPropertyChanged(() => DisplayTiming);

      for (int i = 0; i < 40; i++)
      {
        Sprites[i].RefreshSprite(_display.GetSprite(i), _display.GetOAM(i));
      }

      DisplayStatus disStat = _display.GetDisplayStatus();
      Enabled = disStat.enabled;
      CurrentLine = disStat.currentLine;
      DisplayMode = disStat.displayMode;
      PrevTickCount = disStat.prevTickCount;
      CurrentTickCount = disStat.currentLineTickCount;

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
      _display.RefreshScreen -= OnRefreshScreen;
    }
  }
}