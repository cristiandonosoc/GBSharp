using System.Windows.Media.Imaging;
using GBSharp.VideoSpace;
using System.Windows.Media;

namespace GBSharp.ViewModel
{
  public class SpriteViewModel : ViewModelBase
  {
    private WriteableBitmap _spriteImage;
    public WriteableBitmap SpriteImage
    {
      get { return _spriteImage; }
    }

    private OAM _spriteData;
    public OAM SpriteData
    {
      get { return _spriteData; }
    }

    internal void RefreshSprite(uint[] pixels, OAM oam)
    {
      Utils.TransferBytesToWriteableBitmap(_spriteImage, pixels);
      OnPropertyChanged(() => SpriteImage);

      _spriteData = oam;
      OnPropertyChanged(() => SpriteData);
    }

    public SpriteViewModel()
    {
      _spriteImage = new WriteableBitmap(8, 16, 96, 96, PixelFormats.Bgra32, null);
    }
  }
}