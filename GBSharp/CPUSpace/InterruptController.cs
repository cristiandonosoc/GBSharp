using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace
{
  class InterruptController
  {
    internal bool IME; // Interrupt Master Enable
    ushort IFAddress = 0xFF0F; // Interrupt Request
    ushort IEAddress = 0xFFFF; // Interrupt Enable
    // Interrupt starting address
    Dictionary<Interrupts, ushort> InterruptHandlers = new Dictionary<Interrupts, ushort>()
    {
      {Interrupts.VerticalBlanking, 0x0040},
      {Interrupts.LCDCStatus, 0x0048},
      {Interrupts.TimerOverflow, 0x0050},
      {Interrupts.SerialIOTransferCompleted, 0x0058},
      {Interrupts.P10to13TerminalNegativeEdge, 0x0060}
    };

  }
}
