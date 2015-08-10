using System.Drawing;

namespace GBSharp
{
  public interface IDisplay
  {
    Bitmap Screen { get; }
    Bitmap Background { get; }
  }
}
