using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GBSharp.ViewModel
{
  public class DisplayViewModel : ViewModelBase
  {
    private readonly IDisplay _display;
    
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

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    public DisplayViewModel(IDisplay display)
    {
      _display = display;
    }

    public void CopyFromDomain()
    {
      Background = Utils.BitmapToImageSource(_display.Background);
    }

    private void CopyToDomain()
    {

    }
  }
}