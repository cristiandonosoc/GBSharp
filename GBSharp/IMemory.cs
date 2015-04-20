using System;

namespace GBSharp
{
  public interface IMemory
  {
    event Action ValuesChanged;
    byte[] Values { get; }
  }
}