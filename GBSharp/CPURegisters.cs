﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
  [Serializable]
  [StructLayout(LayoutKind.Explicit)]
  public class CPURegisters
  {
    // 1 byte registers
    [FieldOffset(0)]
    private byte _F;
    [FieldOffset(1)]
    public byte A;
    [FieldOffset(2)]
    public byte C;
    [FieldOffset(3)]
    public byte B;
    [FieldOffset(4)]
    public byte E;
    [FieldOffset(5)]
    public byte D;
    [FieldOffset(6)]
    public byte L;
    [FieldOffset(7)]
    public byte H;

    // 2 byte registers
    [FieldOffset(8)]
    public ushort SP;

    /// <summary>
    /// Program Counter. Do not write directly to this register to implement jumps.
    /// Write to CPU.nextPC intead, that's the value that is going to be pushed to the PC at the end of the step.
    /// </summary>
    [FieldOffset(10)]
    public ushort PC;

    /// <summary>
    /// Internal register used for two-stages instructions
    /// (read on one clock and write on another). Some of these instruction
    /// don't read into any register, so we need a temporary one)
    /// </summary>
    [FieldOffset(12)]
    internal byte TEMP;

    // 2 byte "union" registers
    [FieldOffset(0)]
    private ushort _AF;
    [FieldOffset(2)]
    public ushort BC;
    [FieldOffset(4)]
    public ushort DE;
    [FieldOffset(6)]
    public ushort HL;

    public byte F
    {
      get { return _F; }
      set
      {
        // NOTE(Cristian): The lowest nibble of F is always 0
        _F = (byte)(value & 0xF0);
      }
    }

    public ushort AF
    {
      get { return _AF; }
      set
      {
        _AF = value;
        // NOTE(Cristian): The lowest nibble of F is always 0
        _F = (byte)(_F & 0xF0);
      }
    }

    public byte FZ
    {
      get
      {
        return (byte)((F & 0x80) == 0x80 ? 1 : 0);
      }

      set
      {
        if (value == 0)
        {
          F = (byte)(F & (~0x80));
        }
        else
        {
          F = (byte)(F | 0x80);
        }
      }
    }

    public byte FN
    {
      get
      {
        return (byte)((F & 0x40) == 0x40 ? 1 : 0);
      }

      set
      {
        if (value == 0)
        {
          F = (byte)(F & (~0x40));
        }
        else
        {
          F = (byte)(F | 0x40);
        }
      }
    }

    public byte FH
    {
      get
      {
        return (byte)((F & 0x20) == 0x20 ? 1 : 0);
      }

      set
      {
        if (value == 0)
        {
          F = (byte)(F & (~0x20));
        }
        else
        {
          F = (byte)(F | 0x20);
        }
      }
    }

    public byte FC
    {
      get
      {
        return (byte)((F & 0x10) == 0x10 ? 1 : 0);
      }

      set
      {
        if (value == 0)
        {
          F = (byte)(F & (~0x10));
        }
        else
        {
          F = (byte)(F | 0x10); 
        }
      }
    }


    public override string ToString()
    {
      string word = "";
      word += "PC:" + PC.ToString("x") + "\n";
      word += "SP:" + SP.ToString("x") + "\n";
      word += "A:" + A.ToString("x") + "\n";
      word += "B:" + B.ToString("x") + "\n";
      word += "C:" + C.ToString("x") + "\n";
      word += "D:" + D.ToString("x") + "\n";
      word += "E:" + E.ToString("x") + "\n";
      word += "F:" + F.ToString("x") + "\n";
      word += "H:" + H.ToString("x") + "\n";
      word += "L:" + L.ToString("x") + "\n";
      return word;
    }
  }
}
