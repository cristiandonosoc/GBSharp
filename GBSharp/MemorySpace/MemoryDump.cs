namespace GBSharp.MemorySpace
{
  public class MemoryDump : IMemory
  {
    private readonly byte[] _data;

    public byte[] Data
    {
      get { return _data; }
    }

    public MemoryDump(byte[] data)
    {
      _data = data;
    }
  }
}