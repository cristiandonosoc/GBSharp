using System;
using System.Collections.Generic;
using System.IO;

namespace GBSharp
{
  public class MemoryDummy : IMemory
  {
    private readonly byte[] _values;

    public byte[] Data
    {
      get { return _values; }
    }

    public MemoryDummy()
    {
      _values = File.ReadAllBytes("rom.gb");

    }
  }
}