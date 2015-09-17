using System;
using System.IO;
using GBSharp.ViewModel;
using Microsoft.Win32;

namespace GBSharp.View
{
  public class OpenFileDialogAdapter : IOpenFileDialog
  {
    public event Action<string> OnFileOpened;

    public void Open()
    {
      var openFileDialog = new OpenFileDialog();
      openFileDialog.ShowDialog();

      if (openFileDialog.FileName != null)
      {
        if (File.Exists(openFileDialog.FileName))
          NotifyFileOpened(openFileDialog.FileName);
      }
    }

    private void NotifyFileOpened(string fileName)
    {
      if (OnFileOpened != null)
        OnFileOpened(fileName);
    }

  }
}
