using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace GBSharp.SerialSpace
{
  class LANSerialAdapter : SerialAdapter
  {
    TcpClient tcpClient;
    TcpListener tcpListener;

    /// <summary>
    /// Returns the available connection names.
    /// </summary
    public override string[] Discover()
    {
      return new string[0];
    }

    /// <summary>
    /// Binds the gameboy serial port to the given IP and port address.
    /// </summary>
    /// <param name="connection">An IP:port address usually obtained from Discover().</param>
    public override void Connect(string connection)
    {
      throw new NotImplementedException();
    }

    public override void Step(byte ticks)
    {
      // throw new NotImplementedException();
    }
  }
}
