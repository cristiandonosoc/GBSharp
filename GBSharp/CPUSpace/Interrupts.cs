using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace
{
  /// <summary>
  /// The bit masks used by the Interrupt Request (IF, 0xFF0F) and Interrupt Enable (IE, 0xFFFF) registers.
  /// The upper 3 bits of each register are not used (0x20, 0x40, 0x80).
  /// Lower masks have the highest priority. When multiple interrupts occur simultaneously and the IE flag
  /// of each is set, only that with the highest priority is started.
  /// </summary>
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
