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
    private uint[] _spectrogramData = new uint[_numberOfFramesPerImage * _fftSize];
    private byte maxValue = 255;

    private byte[] _maxColor = new byte[] { 255, 170, 0 };
    private byte[] _minColor = new byte[] { 0, 0, 170 };

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
      _spectrogram = new WriteableBitmap(_numberOfFramesPerImage, _fftSize, 96, 96, PixelFormats.Bgr32, null);
    }

    private void OnFrameCompleted()
    {
      if (Update)
      {
        _dispatcher.Invoke(CopyFromDomain);
        _currentFrame = (_currentFrame + 1) % _numberOfFramesPerImage;
      }
    }

    private void CopyFakeFromDomain()
    {
      var sampleRate = 44000;
      var frequency = 400;
      var sampleCount = 1024;
      var buffer = new Complex[sampleCount];
      var t = 0.0;
      for (int i = 0; i < sampleCount; i++)
      {
        var real = Math.Sin(2 * Math.PI * frequency * t);
        buffer[i] = new Complex(real, 0);
        t += 1.0 / sampleRate;
      }
      FourierTransform.FFT(buffer, FourierTransform.Direction.Forward);
      CopyFFTToFrameColumn(buffer);
      Utils.TransferBytesToWriteableBitmap(_spectrogram, _spectrogramData);
    }

    private void CopyFromDomain()
    {
      var audioBuffer = _gameBoy.APU.Buffer;
      var sampleCount = _gameBoy.APU.SampleCount;
      var complexBuffer = new Complex[_fftSize * 2];
      var index = 0;
      var originalIndex = 0;
      foreach (var value in audioBuffer)
      {
        if (originalIndex % 2 == 0)
        {
          if (index >= sampleCount)
            complexBuffer[index] = new Complex(0, 0);
          else
            complexBuffer[index] = new Complex(value, 0);

          index++;
          if (index == _fftSize * 2)
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
      var maxReal = fftResults.Select(c => 20 * Math.Log10(Math.Abs(c.Magnitude))).Max();
      var minReal = fftResults.Select(c => 20 * Math.Log10(Math.Abs(c.Magnitude))).Min();
      for (int i = 0; i < _fftSize; i++)
      {
        var value = fftResults[i].Magnitude;
        var logValue = 20 * Math.Log10(Math.Abs(value));
        uint ushortedValue = (uint)(maxValue * (logValue - minReal) / (maxReal - minReal));
        uint colorB = ushortedValue * _maxColor[2] + (1-ushortedValue)* _minColor[2];
        uint colorG = ushortedValue * _maxColor[1] + (1-ushortedValue)* _minColor[1];
        uint colorR = ushortedValue * _maxColor[0] + (1-ushortedValue)* _minColor[0];
        uint uintValue = 0x000000FF;
        uintValue = uintValue | (colorR << 8);
        uintValue = uintValue | (colorG << 16);
        uintValue = uintValue | (colorB << 24);

        _spectrogramData[(_fftSize - i - 1) * _numberOfFramesPerImage + _currentFrame] = (uintValue);

      }
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
    }
  }
}