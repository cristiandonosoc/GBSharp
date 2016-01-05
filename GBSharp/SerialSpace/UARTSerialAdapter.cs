using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.SerialSpace
{
  internal class UARTSerialAdapter : SerialAdapter
  {
    private SerialPort port;
    private byte rxData;
    private byte txData;

    /// <summary>
    /// Returns the available connection names.
    /// </summary
    public override string[] Discover()
    {
      string[] portNames = SerialPort.GetPortNames();
      return portNames;
    }

    /// <summary>
    /// Binds the gameboy serial port to the serial adapter using a connection name.
    /// </summary>
    /// <param name="connection">A serial port name usually obtained from Discover().</param>
    public override void Connect(string connection)
    {
      int baudRate = 115200;
      port = new SerialPort(connection, baudRate);
      port.Open();
      port.DataReceived += DataReceived;
    }

    /// <summary>
    /// Listens for a serial data received event on the serial port.
    /// </summary>
    private void DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      int incoming;
      while ((incoming = port.ReadByte()) != -1)
      {
        throw new NotImplementedException();
      }
    }

    public override void Step(byte ticks)
    {
      // throw new NotImplementedException();
    }
  }
}
