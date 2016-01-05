using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
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

    public static string DisplayBytesToString(uint[] pixels, char[] characters, int width)
    {
      var dictionary = new Dictionary<uint, int>();
      var index = 0;
      foreach (uint pixel in pixels)
      {
        if (!dictionary.ContainsKey(pixel))
        {
          dictionary.Add(pixel, index);
          index++;
        }
      }
      var s = new StringBuilder();
      var widthIndex = 0;
      foreach (uint pixel in pixels)
      {
        s.Append(characters[dictionary[pixel]]);
        widthIndex++;
        if (widthIndex >= width)
        {
          s.Append('\n');
          widthIndex = 0;
        }
      }
      return s.ToString();
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
