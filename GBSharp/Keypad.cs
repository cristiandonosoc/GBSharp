using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
  [Flags]
  public enum Keypad : byte
  {
    None   = 0x00, // Special value, nothing
    Right  = 0x01, // P10 P14
    Left   = 0x02, // P11 P14
    Up     = 0x04, // P12 P14
    Down   = 0x08, // P13 P14
    A      = 0x10, // P10 P15
    B      = 0x20, // P11 P15
    Select = 0x40, // P12 P15
    Start  = 0x80, // P13 P15
  }
}
