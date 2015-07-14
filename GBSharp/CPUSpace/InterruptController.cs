using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace
{
  class InterruptController
  {
    private bool IME; // Interrupt Master Enable
    private const ushort IFAddress = 0xFF0F; // Interrupt Request Address, see Interrupts.cs
    private const ushort IEAddress = 0xFFFF; // Interrupt Enable Address, see Interrupts.cs
    
    // Interrupt starting addresses
    Dictionary<Interrupts, ushort> InterruptHandlers = new Dictionary<Interrupts, ushort>()
    {
      {Interrupts.VerticalBlanking, 0x0040},
      {Interrupts.LCDCStatus, 0x0048},
      {Interrupts.TimerOverflow, 0x0050},
      {Interrupts.SerialIOTransferCompleted, 0x0058},
      {Interrupts.P10to13TerminalNegativeEdge, 0x0060}
    };

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="memory">The memory referenced by the CPU core.</param>
    internal InterruptController(MemorySpace.Memory memory)
    {

    }

    /// <summary>
    /// If set to false, prohibits all interrupts.
    /// This should be set to false by DI instruction and true by EI instruction.
    /// </summary>
    internal bool InterruptMasterEnable
    {
      get
      {
        return this.IME;
      }

      set
      {
        this.IME = value;
      }
    }
  }
}
