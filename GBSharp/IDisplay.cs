using System;
using System.Drawing;

namespace GBSharp
{
  public interface IDisplay
  {
    event Action RefreshScreen;
    Bitmap Screen { get; }
    Bitmap Frame { get; }
  }
}
