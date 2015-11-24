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

    public static void TransferBytesToWriteableBitmap(WriteableBitmap bitmap, uint[] pixels)
    {
      bitmap.Lock();
      unsafe
      {
        uint* bPtr = (uint*)bitmap.BackBuffer;
        foreach (uint pixel in pixels)
        {
          *bPtr++ = pixel;
        }
      }
      bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
      bitmap.Unlock();
    }

    public static void TransferBytesToWriteableBitmap(WriteableBitmap bitmap, ushort[] pixels)
    {
      bitmap.Lock();
      unsafe
      {
        ushort* bPtr = (ushort*)bitmap.BackBuffer;
        foreach (ushort pixel in pixels)
        {
          *bPtr++ = pixel;
        }
      }
      bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
      bitmap.Unlock();
    }

    public static void TransferBytesToWriteableBitmap(WriteableBitmap bitmap, byte[] pixels)
    {
      bitmap.Lock();
      unsafe
      {
        byte* bPtr = (byte*)bitmap.BackBuffer;
        foreach (byte pixel in pixels)
        {
          *bPtr++ = pixel;
        }
      }
      bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
      bitmap.Unlock();
    }
  }
}
