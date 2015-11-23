using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Math;

namespace GBSharp.ViewModel
{
  public class APUViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDispatcher _dispatcher;

    private bool _update;
    private WriteableBitmap _spectrogram;

    private int _currentFrame = 0;
    private const int _fftSize = 512;
    private const int _numberOfFramesPerImage = 600;
    private ushort[] _spectrogramData = new ushort[_numberOfFramesPerImage * _fftSize];

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

    public WriteableBitmap Spectrogram
    {
      get { return _spectrogram; }
      set
      {
        _spectrogram = value;
        OnPropertyChanged(() => Spectrogram);
      }
    }

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public APUViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _gameBoy.FrameCompleted += OnFrameCompleted;
      _spectrogram = new WriteableBitmap(_numberOfFramesPerImage, _fftSize, 96, 96, PixelFormats.Gray16, null);
    }

    private void OnFrameCompleted()
    {
      if (Update)
      {
        _dispatcher.Invoke(CopyFromDomain);
        _currentFrame = (_currentFrame + 1) % _numberOfFramesPerImage;
      }
    }

    private void CopyFromDomain()
    {
      var audioBuffer = _gameBoy.APU.Buffer;
      var complexBuffer = new Complex[1024];
      var index = 0;
      var originalIndex = 0;
      foreach (var value in audioBuffer)
      {
        if (originalIndex % 2 == 0)
        {
          complexBuffer[index] = new Complex(value, 0);
          index++;
          if (index == 1024)
            break;
        }
        originalIndex++;
      }
      FourierTransform.FFT(complexBuffer, FourierTransform.Direction.Forward);
      CopyFFTToFrameColumn(complexBuffer);
      Utils.TransferBytesToWriteableBitmap(_spectrogram, _spectrogramData);
    }

    private void CopyFFTToFrameColumn(Complex[] fftResults)
    {
      var maxReal = fftResults.Select(c => 20 * Math.Log10(Math.Abs(c.Re))).Max();
      var minReal = fftResults.Select(c => 20 * Math.Log10(Math.Abs(c.Re))).Min();
      for (int i = 0; i < _fftSize; i++)
      {
        var value = fftResults[i].Re;
        var logValue = 20 * Math.Log10(Math.Abs(value));
        var ushortedValue = 65536 * (logValue - minReal) / (maxReal - minReal);
        _spectrogramData[i * _numberOfFramesPerImage +  _currentFrame] = (ushort)(ushortedValue);

      }
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}