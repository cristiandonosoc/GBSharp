using System;
using System.IO;
using GBSharp.ViewModel;
using Microsoft.Win32;

namespace GBSharp.View
{
  public class OpenFileDialogAdapter : IOpenFileDialog
  {
    public event Action<string, int> OnFileOpened;

    public void Open(string filter)
    {
      var openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = filter;
      openFileDialog.ShowDialog();

      if (openFileDialog.FileName != null)
      {
        if (File.Exists(openFileDialog.FileName))
          NotifyFileOpened(openFileDialog.FileName, openFileDialog.FilterIndex);
      }
    }

    private void NotifyFileOpened(string fileName, int filerIndex)
    {
      if (OnFileOpened != null)
        OnFileOpened(fileName, filerIndex);
    }

  }
}
