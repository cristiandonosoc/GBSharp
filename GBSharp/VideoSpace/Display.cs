using GBSharp.CPUSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GBSharp.VideoSpace
{
  class Display : IDisplay
  {
    private int screenWidth = 160;
    private int screenHeight = 144;
    private Bitmap screen;

    private int backgroundWidth = 256;
    private int backgroundHeight = 256;
    private Bitmap background;

    public Bitmap Screen
    {
      get { return screen; }
    }

    public Bitmap Background
    {
      get { return background; }
    }

    /// <summary>
    /// Display constructor.
    /// </summary>
    /// <param name="interruptController">A reference to the interrupt controller.</param>
    public Display(InterruptController interruptController)
    {
      screen = new Bitmap(screenWidth, screenHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
      background = new Bitmap(backgroundWidth, backgroundHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
    }

    /// <summary>
    /// Simulates the update of the display for a period of time of a given number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks ellapsed since the last call.
    /// A tick is a complete source clock oscillation, ~238.4 ns (2^-22 seconds).</param>
    internal void Step(byte ticks)
    {
      // Count ticks and then..
      // OAM Access?
      // Do Line Magics?
      // H-Blank?
      // V-Blank?
    }
  }
}
