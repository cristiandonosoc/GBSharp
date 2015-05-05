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
  }
}
