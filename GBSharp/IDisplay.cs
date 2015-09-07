using System;
using System.Drawing;

namespace GBSharp
{
  public interface IDisplay
  {
    event Action RefreshScreen;
    // Intermediate targets
    Bitmap Background { get; }
    Bitmap Window { get; }
    
    // Composed targets
    Bitmap Frame { get; }
    Bitmap Screen { get; }
    Bitmap GetSprite(int index);
  }
}
