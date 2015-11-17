using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBSharp.ViewModel
{
  public class InstructionHistogramViewModel : ViewModelBase, IDisposable
  {
    private IGameBoy _gameBoy;
    private readonly IDispatcher _dispatcher;
    private ICPU _cpu;

    private bool _update;

    public bool Update
    {
      get { return _update; }
      set
      {
        if (_update == value)
        {
          return;
        }
        _update = value;
      }
    }

    private WriteableBitmap _histogram;
    private WriteableBitmap _cbHistogram;

    public WriteableBitmap Histogram
    {
      get { return _histogram; }
      set
      {
        _histogram = value;
        OnPropertyChanged(() => Histogram);
      }
    }

    public WriteableBitmap CBHistogram
    {
      get { return _cbHistogram; }
      set
      {
        _cbHistogram = value;
        OnPropertyChanged(() => CBHistogram);
      }
    }

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public InstructionHistogramViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _gameBoy.FrameCompleted += OnFrameCompleted;
      _cpu = _gameBoy.CPU;
      _histogram = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Gray16, null);
      _cbHistogram = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Gray16, null);
    }

    private void OnFrameCompleted()
    {
      if (Update)
      {
        _dispatcher.Invoke(CopyFromDomain);
      }
    }

    public void CopyFromDomain()
    {
      var normalInstructionsHistogram = ReMapHistogram(_cpu.InstructionHistogram);
      var cbInstructionsHistogram = ReMapHistogram(_cpu.CbInstructionHistogram);
      Utils.TransferBytesToWriteableBitmap(_histogram, normalInstructionsHistogram);
      Utils.TransferBytesToWriteableBitmap(_cbHistogram, cbInstructionsHistogram);

    }

    private ushort[] ReMapHistogram(ulong[] instructionHistogram)
    {
      var max = instructionHistogram.Max();
      ulong maxShort = 65535;
      
      ulong ratio = max / maxShort;
      if (ratio == 0)
        ratio = 1;
      return instructionHistogram.ToList().Select(h => (ushort)(65535 - (h / ratio))).ToArray();
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}