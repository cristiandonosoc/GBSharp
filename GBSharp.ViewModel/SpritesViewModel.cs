using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GBSharp.ViewModel
{
  public class SpritesViewModel : ViewModelBase, IDisposable
  {
    private readonly IDispatcher _dispatcher;
    private readonly IDisplay _display;
    private readonly IMemory _memory;
    private readonly IGameBoy _gameBoy;

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

    public SpritesViewModel(IGameBoy gameboy, IDisplay display, IMemory memory, IDispatcher dispatcher)
    {
      _gameBoy = gameboy;
      _display = display;
      _gameBoy.RefreshScreen += OnRefreshScreen;
      _memory = memory;
      _dispatcher = dispatcher;

     
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
      if (UpdateSprites)
      {
        for (int i = 0; i < 40; i++)
        {
          Sprites[i].RefreshSprite(_display.GetSprite(i), _display.GetOAM(i));
        }
      }
    }

    public void Dispose()
    {
      _gameBoy.RefreshScreen -= OnRefreshScreen;
    }
  }
}