using CSCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
  class DirectStreamingSource : IWaveSource
  {
    private readonly WaveFormat _waveFormat;

    private volatile object _lockObj = new object();

    /// Gets the maximum size of the buffer in bytes.
    /// </summary>
    /// <value>
    /// The maximum size of the buffer in bytes.
    /// </value>
    public int MaxBufferSize { get; private set; }

    private IAPU _streamingSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularWriteableBufferingSource"/> class.
    /// </summary>
    /// <param name="waveFormat">The WaveFormat of the source.</param>
    /// <param name="bufferSize">Buffersize in bytes.</param>
    public DirectStreamingSource(WaveFormat waveFormat, IAPU apu)
    {
      if (waveFormat == null) { throw new ArgumentNullException("waveFormat"); }
      _waveFormat = waveFormat;
      _streamingSource = apu;
      MaxBufferSize = 88000;
    }

    /// <summary>
    ///     Reads a sequence of bytes from the internal buffer of the <see cref="CircularWriteableBufferingSource" /> and advances the position within the internal buffer by the
    ///     number of bytes read.
    /// </summary>
    /// <param name="buffer">
    ///     An array of bytes. When this method returns, the <paramref name="buffer" /> contains the specified
    ///     byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
    ///     <paramref name="count" /> - 1) replaced by the bytes read from the internal buffer.
    /// </param>
    /// <param name="offset">
    ///     The zero-based byte offset in the <paramref name="buffer" /> at which to begin storing the data
    ///     read from the internal buffer.
    /// </param>
    /// <param name="count">The maximum number of bytes to read from the internal buffer.</param>
    /// <returns>The total number of bytes read into the <paramref name="buffer"/>.</returns>
    public int Read(byte[] buffer, int offset, int count)
    {
      // We generate the needed samples
      _streamingSource.GenerateSamples(count / _streamingSource.NumChannels);
      if(_streamingSource.SampleCount != count)
      {
        throw new InvalidProgramException("Samples should be the same");
      }

      Array.Copy(_streamingSource.Buffer, buffer, count);
      _streamingSource.ClearBuffer();

      return count;
    }

    public void Dispose()
    {
      // Nothing to dispose
    }

    /// <summary>
    ///     Gets the <see cref="IAudioSource.WaveFormat" /> of the waveform-audio data.
    /// </summary>
    public WaveFormat WaveFormat
    {
      get { return _waveFormat; }
    }

    /// <summary>
    ///     Not supported.
    /// </summary>
    public long Position
    {
      get
      {
        return 0;
      }
      set
      {
        throw new InvalidOperationException();
      }
    }

    /// <summary>
    ///     Gets the number of stored bytes inside of the internal buffer.
    /// </summary>
    public long Length
    {
      get { return 0; }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="IAudioSource"/> supports seeking.
    /// </summary>
    public bool CanSeek
    {
      get { return false; }
    }
  }
}
