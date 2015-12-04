using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.MemorySpace
{
  /// <summary>
  /// Memory Mapped Registers
  /// </summary>
  public enum MMR : ushort
  {
    // Keypad IO port register:
    P1 = 0xFF00,   // P1 Port: P10 ~ P15 bit mapped
    
    // Serial communication registers:
    SB = 0xFF01,   // Serial Transfer Data (8 bit shift register)
    SC = 0xFF02,   // Serial Transfer Control
    
    // Timer registers
    DIV = 0xFF04,  // Frequency divider; upper 8 bits of the 16 bit clock counter
    TIMA = 0xFF05, // Timer counter
    TMA = 0xFF06,  // Timer modulo
    TAC = 0xFF07,  // Timer controller

    // Interrupt flags:
    IF = 0xFF0F, // Interrupt request
    IE = 0xFFFF, // Interrupt enabled mask

    // LCD registers:
    LCDC = 0xFF40,
    STAT = 0xFF41,
    SCY = 0xFF42,  // Scroll Y
    SCX = 0xFF43,  // Scroll X
    LY = 0xFF44,
    LYC = 0xFF45,
    DMA = 0xFF46,
    BGP = 0xFF47,
    OBP0 = 0xFF48,
    OBP1 = 0xFF49,
    WY = 0xFF4A,
    WX = 0xFF4B,
    OAM0 = 0xFE00, 

    // Sound registers:
    NR10 = 0xFF10, 
    NR11 = 0xFF11, 
    NR12 = 0xFF12, 
    NR13 = 0xFF13, 
    NR14 = 0xFF14, 

    NR21 = 0xFF16, 
    NR22 = 0xFF17, 
    NR23 = 0xFF18, 
    NR24 = 0xFF19, 

    NR30 = 0xFF1A, 
    NR31 = 0xFF1B, 
    NR32 = 0xFF1C, 
    NR33 = 0xFF1D, 
    NR34 = 0xFF1E, 

    NR41 = 0xFF20, 
    NR42 = 0xFF21, 
    NR43 = 0xFF22, 
    NR44 = 0xFF23, 

    NR50 = 0xFF24,
    NR51 = 0xFF25,
    NR52 = 0xFF26,

    WFRAM = 0xFF30 // Waveform RAM 0xFF30 ~ 0xFF3F
  }
}
