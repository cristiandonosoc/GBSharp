using System;
using GBSharp.VideoSpace;

namespace GBSharp
{
  public interface IDisplay
  {
    OAM GetOAM(int index);

    // Intermediate targets
    uint[] GetDebugTarget(DebugTargets debugTarget);
    bool GetUpdateDebugTarget(DebugTargets debugTarget);
    void SetUpdateDebugTarget(DebugTargets debugTarget, bool value);

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
