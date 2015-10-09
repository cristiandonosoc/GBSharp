using System;

namespace GBSharp
{
  public interface IMemory
  {
    byte[] Data { get; }
    void Load(byte[] data);

    event Action<ushort, ushort> MemoryWritten;
  }
}