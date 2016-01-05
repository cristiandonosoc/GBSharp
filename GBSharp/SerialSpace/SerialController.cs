using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;

namespace GBSharp.SerialSpace
{
  class SerialController
  {
    private SerialAdapter adapter;
    private InterruptController interruptController;
    private Memory memory;

    /// <summary>
    /// Class constructor.
    /// </summary>
    public SerialController(InterruptController interruptController, Memory memory)
    {
      this.interruptController = interruptController;
      this.memory = memory;
      
      // Always try to serial connect for now
      SerialAdapter uart = new UARTSerialAdapter();
      var connections = uart.Discover();
      if(connections.Length > 0)
      {
        // Connect to the first one, this is awesome!
        uart.Connect(connections[0]);
        this.adapter = uart;
        return;
      }
    }

    /// <summary>
    /// Steps the serial data in/out.
    /// </summary>
    /// <param name="ticks">Number of clock ticks ellapsed since last call.</param>
    internal void Step(byte ticks)
    {
      if(adapter == null)
      {
        return;
      }

      adapter.Step(ticks);
    }
  }
}
