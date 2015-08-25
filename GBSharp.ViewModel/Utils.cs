using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace GBSharp.ViewModel
{
  public static class Utils
  {
    public static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
      using (var memory = new MemoryStream())
      {
        bitmap.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;
        BitmapImage bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
      }
    }

  }
}
