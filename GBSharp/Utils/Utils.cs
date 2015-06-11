using System;
using Microsoft.Win32;

namespace GBSharp.Utils
{
  enum Flags
  {
    Z = 7,
    N = 6,
    H = 5,
    C = 4
  }

  static class UtilFuncs
  {
    public static byte TestBit(byte word, int bit)
    {
      byte mask = (byte)(1 << bit);
      return (byte)(word & mask);
    }

    public static byte SetBit(byte word, int bit)
    {
      byte mask = (byte)(1 << bit);
      return (byte)(word | mask);
    }

    public static byte ClearBit(byte word, int bit)
    {
      byte mask = (byte)~(1 << bit);
      return (byte)(word & mask);
    }

    public static byte RotateLeft(byte value, int count = 1)
    {
      return (byte)((value << count) | (value >> (32 - count)));
    }

    public static byte RotateRight(byte value, int count = 1)
    {
      return (byte)((value >> count) | (value << (32 - count)));
    }

    public static Tuple<byte, byte> RotateLeftThroughCarry(byte value, int count = 1, int carry = 0)
    {
      var carryOut = (byte)((value >> 7) & 1);
      var valueShifted = value << count;
      var valueRotated = (byte)(valueShifted | carry);
      return new Tuple<byte, byte>(valueRotated, carryOut);
    }

    public static Tuple<byte, byte> RotateLeftAndCarry(byte value, int count = 1)
    {
      var carryOut = (byte)((value >> 7) & 1);
      var valueShifted = value << count;
      var valueRotated = (byte)(valueShifted | carryOut);
      return new Tuple<byte, byte>(valueRotated, carryOut);
    }

    public static Tuple<byte, byte> RotateRightThroughCarry(byte value, int count = 1, int carry = 0)
    {
      var carryOut = (byte)(value & 1);
      var valueShifted = value >> count;
      var valueRotated = (byte)(valueShifted | (carry << 7));
      return new Tuple<byte, byte>(valueRotated, carryOut);
    }

    public static Tuple<byte, byte> RotateRightAndCarry(ushort value, int count = 1)
    {
      var carryOut = (byte)(value & 1);
      var valueShifted = value >> count;
      var valueRotated = (byte)(valueShifted | (carryOut << 7));
      return new Tuple<byte, byte>(valueRotated, carryOut);
    }

    public static Tuple<byte, byte> ShiftLeft(byte value, int count = 1)
    {
      var carryOut = (byte)((value >> 7) & 1);
      var valueShifted = (byte)(value << count);
      return new Tuple<byte, byte>(valueShifted, carryOut);
    }

    public static Tuple<byte, byte> ShiftRightLogic(byte value, int count = 1)
    {
      var carryOut = (byte)(value & 1);
      var valueShifted = value >> count;
      return new Tuple<byte, byte>((byte)valueShifted, carryOut);
    }

    public static Tuple<byte, byte> ShiftRightArithmetic(ushort value, int count = 1)
    {
      var carryOut = (byte)(value & 1);
      var msb = (byte)(value & 0x80);
      var valueShifted = (byte) ((value >> count) | msb);
      return new Tuple<byte, byte>(valueShifted, carryOut);
    }

    public static byte SwapNibbles(byte value)
    {
      var highNibble = (byte)(value >> 4);
      var lowNibble = (byte)(value << 4);
      return (byte)(highNibble | lowNibble);
    }

    public static void SignedAdd(ref ushort target, sbyte operand)
    {
      short offset = operand;
      if (offset >= 0)
      {
        target += (ushort)offset;
      }
      else
      {
        offset *= -1;  // We invert the value
        target -= (ushort)offset;
      }
    }
  }
}
