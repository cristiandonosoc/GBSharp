using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;

namespace GBSharp.ViewModel
{
  public class SpriteViewModel : ViewModelBase
  {
    private readonly BitmapImage _spriteImage;
    private readonly OAM _spriteData;


    public BitmapImage SpriteImage
    {
      get { return _spriteImage; }
    }

    public OAM SpriteData
    {
      get { return _spriteData; }
    }

    public SpriteViewModel(BitmapImage spriteImage, OAM spriteData)
    {
      _spriteImage = spriteImage;
      _spriteData = spriteData;
    }
  }
}