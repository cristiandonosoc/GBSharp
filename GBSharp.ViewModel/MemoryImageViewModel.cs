using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBSharp.ViewModel
{
  public class MemoryImageViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDispatcher _dispatcher;

    private bool _update;
    private readonly WriteableBitmap _memoryImage;

    public MemoryImageViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _gameBoy.FrameCompleted += OnFrameCompleted;
      _memoryImage = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Gray8, null);
    }

    public WriteableBitmap MemoryImage
    {
      get { return _memoryImage; }
    }

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public bool Update
    {
      get { return _update; }
      set
      {
        if (_update != value)
        {
          _update = value;
        }

      }
    }

    private void OnFrameCompleted()
    {
      if (Update)
      {
        _dispatcher.BeginInvoke(new Action(CopyFromDomain), null);
      }
    }

    private void CopyFromDomain()
    {
      Utils.TransferBytesToWriteableBitmap(_memoryImage, _gameBoy.Memory.Data);
    }

    public void Dispose()
    {
      //_gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}