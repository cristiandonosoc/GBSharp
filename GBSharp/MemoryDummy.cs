using System;
using System.Collections.Generic;
using System.IO;

namespace GBSharp
{
  public class MemoryDummy : IMemory
  {
    public event Action ValuesChanged;

    private readonly byte[] _values;

    public byte[] Values
    {
      get { return _values; }
    }

    public MemoryDummy()
    {
      _values = File.ReadAllBytes("rom.gb");

    }
  }
}