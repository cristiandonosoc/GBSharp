using GBSharp.VideoSpace;

namespace GBSharp.MemorySpace
{
  internal static class MemoryFunctions
  {
    internal static byte
    CalculateSTATModeChange(byte STAT, DisplayModes mode)
    {
      // The mode is in the first 2 bytes, we want them in position 
      byte bMode = (byte)mode;
      // TODO(Cristian): See if we can specify binary masks directy in binary, not hex
      byte result = (byte)((STAT | 0x03) & (bMode | 0xFC));
      return result;
    }
  }
}
