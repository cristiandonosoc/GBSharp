using System;

namespace GBSharp.ViewModel
{
  public interface IOpenFileDialog
  {
    event Action<string> OnFileOpened;
    void Open();
  }
}