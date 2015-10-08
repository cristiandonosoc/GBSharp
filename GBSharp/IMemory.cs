using System;

namespace GBSharp
{
  public interface IMemory
  {
    byte[] Data { get; }
    void Load(byte[] data);

    ushort LastChangedStart { get; }
    ushort LastChangedEnd { get; }
  }
}