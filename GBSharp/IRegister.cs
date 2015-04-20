using System;

namespace GBSharp
{
  public interface IRegister
  {
    event Action ValueChanged;
    string Name { get; }
    int Value { get; }
    int Size { get; }
  }
}
