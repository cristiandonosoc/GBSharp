using System;

namespace GBSharp.ViewModel
{
  public interface IOpenFileDialog
  {
    event Action<string, int> OnFileOpened;
    void Open(string filter);
  }
}