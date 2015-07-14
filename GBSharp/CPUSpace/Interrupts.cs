using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace
{
  [Flags]
  public enum Interrupts : byte
  {
    VerticalBlanking = 0x01,
    LCDCStatus = 0x02,
    TimerOverflow = 0x04,
    SerialIOTransferCompleted = 0x08,
    P10to13TerminalNegativeEdge = 0x10
  }
}
