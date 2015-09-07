using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GBSharp.MemorySpace;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class DisplayViewModel : ViewModelBase, IDisposable
  {
    public event Action UpdateDisplay;

    private IDispatcher _dispatcher;

    private readonly IDisplay _display;
    private readonly IMemory _memory;


    private bool _blockSelectionFlag;
    private bool _codeAreaSelectionFlag;
    private bool _characterDataSelectionFlag;

    private byte _scrollX;
    private byte _scrollY;

    private BitmapImage _background;
    public BitmapImage Background
    {
      get { return _background; }
      set
      {
        _background = value;
        OnPropertyChanged(() => Background);
      }
    }

    private BitmapImage _window;
    public BitmapImage Window
    {
      get { return _window; }
      set
      {
        _window = value;
        OnPropertyChanged(() => Window);
      }
    }

    private BitmapImage _sprite;
    public BitmapImage Sprite 
    {
      get { return _sprite; }
      set
      {
        _sprite = value;
        OnPropertyChanged(() => Sprite);
      }
    }

    private int _currentSprite;
    public int CurrentSprite
    {
      get { return _currentSprite; }
      set
      {
        _currentSprite = value;
        // TODO(Cristian): Use the correct event bs
        Sprite = Utils.BitmapToImageSource(_display.GetSprite(_currentSprite));
      }
    }

    public bool BlockSelectionFlag
    {
      get { return _blockSelectionFlag; }
      set
      {
        if (_blockSelectionFlag != value)
        {
          _blockSelectionFlag = value;
          OnPropertyChanged(() => BlockSelectionFlag);
        }
      }
    }

    public bool CodeAreaSelectionFlag
    {
      get { return _codeAreaSelectionFlag; }
      set
      {
        if (_codeAreaSelectionFlag != value)
        {
          _codeAreaSelectionFlag = value;
          OnPropertyChanged(() => CodeAreaSelectionFlag);
        }
      }
    }

    public bool CharacterDataSelectionFlag
    {
      get { return _characterDataSelectionFlag; }
      set
      {
        if (_characterDataSelectionFlag != value)
        {
          _characterDataSelectionFlag = value;
          OnPropertyChanged(() => CharacterDataSelectionFlag);
        }
      }
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
    }

    public void CopyFromDomain()
    {
      Background = Utils.BitmapToImageSource(_display.Background);
      Window = Utils.BitmapToImageSource(_display.Window);
      Sprite = Utils.BitmapToImageSource(_display.GetSprite(_currentSprite));

      var lcdControl = _memory.Data[(int)MemoryMappedRegisters.LCDC];
      BlockSelectionFlag = (lcdControl & (byte)LCDControlFlags.OBJBlockCompositionSelection) > 0;
      CodeAreaSelectionFlag = (lcdControl & (byte)LCDControlFlags.BGCodeAreaSelection) > 0;
      CharacterDataSelectionFlag = (lcdControl & (byte)LCDControlFlags.BGCharacterDataSelection) > 0;

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

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
    }

    public void Dispose()
    {
      _display.RefreshScreen -= OnRefreshScreen;
    }
  }
}