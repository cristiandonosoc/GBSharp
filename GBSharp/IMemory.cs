using System;

namespace GBSharp
{
  public interface IMemory
  {
    byte[] Data { get; }
    void Load(byte[] data);

    ushort MemoryChangedLow { get; }
    ushort MemoryChangedHigh { get; }
  }
}