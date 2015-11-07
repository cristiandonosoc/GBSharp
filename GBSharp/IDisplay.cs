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
    uint[] Tiles { get; }

    bool TileBase { get; set; }
    bool TileMap { get; set; }
    
    // Composed targets
    uint[] Frame { get; }
    uint[] Screen { get; }
    uint[] GetSprite(int index);

    DisplayDefinition GetDisplayDefinition();
    DisplayStatus GetDisplayStatus();

    void DrawFrame(int rowStart = 0, int rowEnd = 144);
    void DrawTiles();
  }
}
