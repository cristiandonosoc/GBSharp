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
    uint[] GetDebugTarget(DebugTargets debugTarget);

    bool TileBase { get; set; }
    bool TileMap { get; set; }
    bool NoTileMap { get; set; }
    
    // Composed targets
    uint[] Screen { get; }
    uint[] GetSprite(int index);

    DisplayDefinition GetDisplayDefinition();
    DisplayStatus GetDisplayStatus();

    void DrawFrame(int rowStart = 0, int rowEnd = 144);
    void DrawTiles();
  }
}
