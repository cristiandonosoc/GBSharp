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
    private bool _filter;
    private ushort _maxHistogramValue;

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

    public bool Filter
    {
      get { return _filter; }
      set
      {
        if (_filter != value)
        {
          _filter = value;
          CopyFromDomain();
        }
        
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

    public ushort MaxHistogramValue
    {
      get { return _maxHistogramValue; }
      set
      {
        if (_maxHistogramValue != value)
        {
          _maxHistogramValue = value;
          CopyFromDomain();
        }

      }
    }

    public InstructionHistogramViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _gameBoy.FrameCompleted += OnFrameCompleted;
      _cpu = _gameBoy.CPU;
      _histogram = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Gray16, null);
      _cbHistogram = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Gray16, null);
      _maxHistogramValue = 65535;
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
      ushort max = 65535;
      if (_filter)
        max = _maxHistogramValue;

      return instructionHistogram.ToList().Select(h => ((ushort)(Math.Min(max, h) * (ulong)(65535 / max)))).ToArray();
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}