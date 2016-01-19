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
    private bool transfering;

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
        /* if LD_LOW
            rxData = (rxData & 0xF0) | (value & 0x0F);
        if LD_HIGH
            rxData = (value & 0xF0) | (rxData & 0x0F);

        */
      }
    }

    /// <summary>
    /// Stores the data that will be transfered when the StartTransferMethod is called.
    /// </summary>
    public void WriteSerialBuffer(byte data)
    {
      txData = data;
      // Arduino magics here
    }

    /// <summary>
    /// Starts a data transfer of the data that was stored using WriteSerialBuffer(data).
    /// </summary>
    /// <param name="internalClock">If true, the emulator clock will be used and the data
    /// transfer will be mediated by Step() function. If false, the complete event will be
    /// triggered when the serial data arrives, this could be a lot faster than 8192bps.</param>
    public void StartTransfer(bool internalClock)
    {
      transfering = true;
      // Arduino magics
    }

    public override void Step(byte ticks)
    {
      // throw new NotImplementedException();
    }
  }
}
