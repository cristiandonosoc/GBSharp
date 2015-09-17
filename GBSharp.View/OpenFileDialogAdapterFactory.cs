using GBSharp.ViewModel;

namespace GBSharp.View
{
  public class OpenFileDialogAdapterFactory : IOpenFileDialogFactory
  {
    public IOpenFileDialog Create()
    {
      return new OpenFileDialogAdapter();
    }
  }
}