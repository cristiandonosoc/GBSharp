using CSCore;
using CSCore.Utils.Buffer;
using System;

namespace GBSharp.Audio
{
  /// <summary>
  /// Buffered WaveSource which overrides the allocated memory after the internal buffer got full. 
  /// This buffer wraps around itself so that it doesn't cut when it's full.
  /// </summary>
  public class CircularWriteableBufferingSource : IWaveSource
  {
    private readonly WaveFormat _waveFormat;

    public int ReadCursor { get; private set; }
    public int WriteCursor { get; private set; }

    private byte[] _buffer;
    private int _bufferedElements;

    public int Latency { get; private set; }
    public int LatencyInBytes { get; private set; }

    //private CircularBuffer<byte> _buffer;
    private volatile object _lockObj = new object();

    /// Gets the maximum size of the buffer in bytes.
    /// </summary>
    /// <value>
    /// The maximum size of the buffer in bytes.
    /// </value>
    public int MaxBufferSize { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularWriteableBufferingSource"/> class.
    /// </summary>
    /// <param name="waveFormat">The WaveFormat of the source.</param>
    /// <param name="bufferSize">Buffersize in bytes.</param>
    public CircularWriteableBufferingSource(WaveFormat waveFormat, int bufferSize, int latency)
    {
      if (waveFormat == null)
        throw new ArgumentNullException("waveFormat");
      if (bufferSize <= 0 || (bufferSize % waveFormat.BlockAlign) != 0)
        throw new ArgumentException("Invalid bufferSize.");

      MaxBufferSize = bufferSize;

      _waveFormat = waveFormat;
      _buffer = new byte[bufferSize];

      Latency = latency;
      LatencyInBytes = (int)_waveFormat.MillisecondsToBytes(latency);
    }

    public void SetWriteCursor()
    {
      lock(_lockObj)
      {
        WriteCursor = ReadCursor + LatencyInBytes;
        if (WriteCursor >= _buffer.Length)
        {
          WriteCursor -= _buffer.Length;
        }
      }
    }

    /// <summary>
    /// Adds new data to the internal buffer.
    /// </summary>
    /// <param name="buffer">A byte-array which contains the data.</param>
    /// <param name="offset">Zero-based offset in the <paramref name="buffer"/> (specified in bytes).</param>
    /// <param name="count">Number of bytes to add to the internal buffer.</param>
    /// <returns>Number of added bytes.</returns>
    public int Write(byte[] buffer, int offset, int count)
    {
      int counter = count;
      lock (_lockObj)
      {
        // Circular implementation
        // TODO(Cristian): Do this implementation with Array.Copy
        while(counter > 0)
        {
          _buffer[WriteCursor++] = buffer[offset++];
          if(WriteCursor == _buffer.Length)
          {
            WriteCursor = 0;
          }

          --counter;
        }

        _bufferedElements += count;
      }

      return count;
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
      int read = 0;
      int counter = count;
      lock (_lockObj)
      {
        // TODO(Cristian): Do this with Array.Copy
        while (counter > 0)
        {
          buffer[offset++] = _buffer[ReadCursor++];
          if(ReadCursor == _buffer.Length)
          {
            ReadCursor = 0;
          }

          --counter;
          --_bufferedElements;
        }

        read = count - counter;
      }

      return read;
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
      get { return _bufferedElements; }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="IAudioSource"/> supports seeking.
    /// </summary>
    public bool CanSeek
    {
      get { return false; }
    }

    private bool _disposed;

    /// <summary>
    /// Disposes the <see cref="CircularWriteableBufferingSource"/> and its internal buffer.
    /// </summary>
    public void Dispose()
    {
      if (!_disposed)
      {
        _disposed = true;

        Dispose(true);
        GC.SuppressFinalize(this);
      }
    }

    /// <summary>
    /// Disposes the <see cref="CircularWriteableBufferingSource"/> and its internal buffer.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        //dispose managed
        _buffer = null;
      }
    }

    /// <summary>
    /// Default destructor which calls <see cref="Dispose(bool)"/>.
    /// </summary>
    ~CircularWriteableBufferingSource()
    {
      Dispose(false);
    }
  }
}