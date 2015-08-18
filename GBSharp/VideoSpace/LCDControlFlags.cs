using System;

namespace GBSharp.VideoSpace
{
  [Flags]
  public enum LCDControlFlags : byte
  {
    OBJOn = 0x01,
    OBJBlockCompositionSelection = 0x02,
    BGCodeAreaSelection = 0x04,
    BGCharacterDataSelection = 0x08,
    WindowingOn = 0x10,
    WindowingCodeAreaSelection = 0x20,
    LCDControllerOperationStop = 0x40,
  }
}
