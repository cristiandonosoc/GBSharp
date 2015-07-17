using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace
{
  class InterruptController
  {
    MemorySpace.Memory memory;
    private bool IME; // Interrupt Master Enable
    private const ushort IFAddress = 0xFF0F; // Interrupt Request Address, see Interrupts.cs
    private const ushort IEAddress = 0xFFFF; // Interrupt Enable Address, see Interrupts.cs
    private byte pressedButtons = 0x00; // 8 bits: Start, Select, B, A, Down, Up, Left, Right
    
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
      this.memory = memory;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttons"></param>
    public void UpdateKeypadState(Keypad buttons)
    {
      this.pressedButtons = (byte)buttons; // This only works because the magic values we chose for the Keypad enum
      byte currentP1 = this.memory.Read((ushort)MemoryMappedRegisters.P1);
      byte newP1 = SolveP1(currentP1);

      // Update memory mapped keypad state. In the real gameboy there is a propagation delay of 2^4 clock oscillations before
      // this value is updated. We are not going to simulate that since the code in the game is supposed to handle this delay by
      // waiting a few instructions between writing [p14, p15] and reading [p10, p11, p12, p13]. If a game is using some shady
      // hack to read (like burst-writing and reading at the exact time the propagation occurs), then is going to die *here*.
      this.memory.LowLevelWrite((ushort)MemoryMappedRegisters.P1, newP1);

      // If in current and not in new, NEGATIVE EDGE INTERRUPT!
      if(((0x0F & currentP1) & (0x0F & ~newP1)) != 0x00){
        this.SetInterrupt(Interrupts.P10to13TerminalNegativeEdge);
      }
    }

    /// <summary>
    /// The P1 port is mapped to the 0xFF00 address. In the real gameboy only the bits 4 and 5 (P14 and P15)
    /// are writable. The bits 0 to 3 are the output of a 8 button matrix with the bits 4 and 5 as inputs for one
    /// side and a pulled high (logical 1) on the other side; thus, the values for the bits 0 to 3 can be calculated
    /// from the values of the bit 4 and 5 and the pressed buttons.
    /// </summary>
    /// <param name="p14p15"></param>
    /// <returns>The value for P1.</returns>
    private byte SolveP1(byte p14p15)
    {
      byte p1 = 0x00;

      // Check if P14 is 0
      if ((p14p15 & 0x10) == 0x00)
      {
        p1 = (byte)(0x0F & this.pressedButtons); // 0x0F is the mask for Down, Up, Left, Right (P14)
      }

      // Check if P15 is 0
      if ((p14p15 & 0x20) == 0x00)
      {
        p1 |= (byte)(this.pressedButtons >> 4); // 0xF0 is the mask for A, B, Select, Start (P15)
      }

      // At this point we have the combination of pressed buttons for P14 OR P15 in the lower 4 bits of the P1
      // variable only if that column in the matrix is being sampled, but when buttons are pressed, the signal
      // goes LOW, so we need to negate button values before returning the P1 value.
      return (byte)((p14p15 & 0x30) | (0x0F & ~p1)); // Keep writable bits (0x30 mask) and add the inputs (0x0F mask).
    }

    /// <summary>
    /// Set the interrupt flags for a given kind of interrupt.
    /// </summary>
    /// <param name="kind">An interrupt (or a combination of).</param>
    internal void SetInterrupt(Interrupts kind)
    {
      byte IF = this.memory.Read((ushort)MemoryMappedRegisters.IF);
      this.memory.LowLevelWrite((ushort)MemoryMappedRegisters.IF, (byte)(IF | (byte)kind));
    }

    /// <summary>
    /// Resets the interrupt flags for a given kind of interrupt.
    /// </summary>
    /// <param name="kind">An interrupt (or a combination of).</param>
    internal void ResetInterrupt(Interrupts kind)
    {
      byte IF = this.memory.Read((ushort)MemoryMappedRegisters.IF);
      this.memory.LowLevelWrite((ushort)MemoryMappedRegisters.IF, (byte)(IF & ~(byte)kind));
    }
  }
}
