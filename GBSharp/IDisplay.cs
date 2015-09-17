﻿using System;
using System.Drawing;
using GBSharp.VideoSpace;

namespace GBSharp
{
  public interface IDisplay
  {
    event Action RefreshScreen;

    OAM GetOAM(int index);

    // Intermediate targets
    Bitmap Background { get; }
    Bitmap Window { get; }
    Bitmap SpriteLayer { get; }
    
    // Composed targets
    Bitmap Frame { get; }
    Bitmap Screen { get; }
    Bitmap GetSprite(int index);

    void UpdateScreen();
  }
}
