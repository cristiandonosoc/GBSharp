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
    private double _instructionSetCoverage;
    private double _regularInstructionSetCoverage;
    private double _cbInstructionSetCoverage;

    public string InstructionSetCoverage
    {
      get { return _instructionSetCoverage.ToString("0.00"); }
    }

    public string RegularInstructionSetCoverage
    {
      get { return _regularInstructionSetCoverage.ToString("0.00"); }
    }

    public string CBInstructionSetCoverage
    {
      get { return _cbInstructionSetCoverage.ToString("0.00"); }
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

    public ICommand ResetCommand
    {
      get { return new DelegateCommand(Reset); }
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
        _dispatcher.BeginInvoke(new Action(CopyFromDomain), null);
      }
    }

    public void CopyFromDomain()
    {
      var normalInstructionsHistogram = ReMapHistogram(_cpu.InstructionHistogram);
      var cbInstructionsHistogram = ReMapHistogram(_cpu.CbInstructionHistogram);
      Utils.TransferBytesToWriteableBitmap(_histogram, normalInstructionsHistogram);
      Utils.TransferBytesToWriteableBitmap(_cbHistogram, cbInstructionsHistogram);
      UpdateInstructionSetCoverage();
    }

    private void UpdateInstructionSetCoverage()
    {
      _instructionSetCoverage = 0;
      _regularInstructionSetCoverage = 0;
      _cbInstructionSetCoverage = 0;
      foreach (var instructionUsage in _cpu.InstructionHistogram)
      {
        if (instructionUsage > 0)
        {
          _instructionSetCoverage++;
          _regularInstructionSetCoverage++;
        }
      }
      foreach (var instructionUsage in _cpu.CbInstructionHistogram)
      {
        if (instructionUsage > 0)
        {
          _instructionSetCoverage++;
          _cbInstructionSetCoverage++;
        }
      }
      _instructionSetCoverage /= (_cpu.InstructionHistogram.Length + _cpu.CbInstructionHistogram.Length);
      _instructionSetCoverage *= 100;
      _regularInstructionSetCoverage /= (_cpu.InstructionHistogram.Length);
      _regularInstructionSetCoverage *= 100;
      _cbInstructionSetCoverage /= (_cpu.CbInstructionHistogram.Length);
      _cbInstructionSetCoverage *= 100;
      OnPropertyChanged(() => InstructionSetCoverage);
      OnPropertyChanged(() => RegularInstructionSetCoverage);
      OnPropertyChanged(() => CBInstructionSetCoverage);
    }

    private void Reset()
    {
      _gameBoy.CPU.ResetInstructionHistograms();
      CopyFromDomain();
    }

    private ushort[] ReMapHistogram(ushort[] instructionHistogram)
    {
      ushort max = 65535;
      if (_filter)
        max = _maxHistogramValue;

      return instructionHistogram.ToList().Select(h => ((ushort)(Math.Min(max, h) * (65535 / max)))).ToArray();
    }

    public void Dispose()
    {
      //_gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}