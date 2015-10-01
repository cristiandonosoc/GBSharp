using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private BitmapImage _window;
    private SpriteViewModel _selectedSprite;
    private readonly ObservableCollection<SpriteViewModel> _sprites = new ObservableCollection<SpriteViewModel>();
    private BitmapImage _spriteLayer;
    private BitmapImage _displayTiming;
    
    public BitmapImage Background
    {
      get { return _background; }
      set
      {
        _background = value;
        OnPropertyChanged(() => Background);
      }
    }

    public BitmapImage Window
    {
      get { return _window; }
      set
      {
        _window = value;
        OnPropertyChanged(() => Window);
      }
    }

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
    
    public BitmapImage SpriteLayer
    {
      get { return _spriteLayer; }
      set
      {
        _spriteLayer = value;
        OnPropertyChanged(() => SpriteLayer);
      }
    }

    public BitmapImage DisplayTiming
    {
      get { return _displayTiming; }
      set
      {
        _displayTiming = value;
        OnPropertyChanged(() => DisplayTiming);
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
    }

    public void CopyFromDomain()
    {
      Background = Utils.BitmapToImageSource(_display.Background);
      Window = Utils.BitmapToImageSource(_display.Window);
      SpriteLayer = Utils.BitmapToImageSource(_display.SpriteLayer);
      DisplayTiming = Utils.BitmapToImageSource(_display.DisplayTiming);

      Sprites.Clear();
      for (int i = 0; i < 40; i++)
      {
        Sprites.Add(new SpriteViewModel(Utils.BitmapToImageSource(_display.GetSprite(i)), _display.GetOAM(i)));
      }
      SelectedSprite = Sprites.First();

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