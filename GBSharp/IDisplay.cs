using System;
using System.Drawing;
using GBSharp.VideoSpace;

namespace GBSharp
{
  public interface IDisplay
  {
    event Action RefreshScreen;

    OAM GetOAM(int index);

    // Intermediate targets
    uint[] Background { get; }
    uint[] Window { get; }
    uint[] SpriteLayer { get; }
    uint[] DisplayTiming { get; }
    
    // Composed targets
    uint[] Frame { get; }
    uint[] Screen { get; }
    uint[] GetSprite(int index);

    uint[] UintSpriteLayer { get; }

    DisplayDefinition GetDisplayDefinition();

    void DrawDisplay(int rowStart = 0, int rowEnd = 144);
  }
}
