using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.SerialSpace
{
  internal abstract class SerialAdapter
  {
    /// <summary>
    /// Use the internal clock, provided by Step(), to transfer data.
    /// Use an external clock if false.
    /// </summary>
    public bool UseOwnClock { get; set; }

    /// <summary>
    /// Returns the available connection names.
    /// </summary
    public abstract string[] Discover();

    /// <summary>
    /// Binds the gameboy serial port to the serial adapter using a connection name.
    /// </summary>
    /// <param name="connection">A connection name usually obtained from Discover().</param>
    public abstract void Connect(string connection);

    public abstract void Step(byte ticks);
  }
}
