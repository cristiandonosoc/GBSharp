using System;
using System.IO;

namespace GBSharp.Audio
{
  /// <summary>
  /// Represents a read- and writeable buffer which can hold a specified number of elements. 
  /// </summary>
  /// <typeparam name="T">Specifies the type of the elements to store.</typeparam>
  public class CircularBuffer<T> : IDisposable
  {
    private T[] _buffer;
    private int _bufferedElements;

    private int _writeOffset;
    private int _readOffset;

    private readonly object _lockObj = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
    /// </summary>
    /// <param name="bufferSize">Size of the buffer.</param>
    public CircularBuffer(int bufferSize)
    {
      _buffer = new T[bufferSize];
      _writeOffset = 44000;
    }

    /// <summary>
    /// Adds new data to the internal buffer.
    /// </summary>
    /// <param name="buffer">Array which contains the data.</param>
    /// <param name="offset">Zero-based offset in the <paramref name="buffer"/> (specified in "elements").</param>
    /// <param name="count">Number of elements to add to the internal buffer.</param>
    /// <returns>Number of added elements.</returns>
    public int Write(T[] buffer, int offset, int count)
    {
      int counter = count;
      lock (_lockObj)
      {
        #region ORIGINAL (NON CIRCULAR) IMPLEMENTATION
        //if (count > _buffer.Length - _bufferedElements)
        //{
        //  count = _buffer.Length - _bufferedElements;
        //}

        //int length = Math.Min(count, _buffer.Length - _writeOffset);
        //Array.Copy(buffer, offset, _buffer, _writeOffset, length); //copy to buffer
        //_writeOffset += length;
        //written += length;
        //_writeOffset = _writeOffset % _buffer.Length;

        //if (written < count)
        //{
        //  Array.Copy(buffer, offset + written, _buffer, _writeOffset, count - written);
        //  _writeOffset += (count - written);
        //  written += (count - written);
        //}

        //_bufferedElements += written;
        #endregion

        // Circular implementation
        // TODO(Cristian): Do this implementation with Array.Copy
        while(counter > 0)
        {
          _buffer[_writeOffset++] = buffer[offset++];
          if(_writeOffset == _buffer.Length)
          {
            _writeOffset = 0;
          }

          --counter;
        }

        _bufferedElements += count;
      }

      return count;
    }

    /// <summary>
    ///     Reads a sequence of elements from the internal buffer of the <see cref="CircularBuffer{T}" />.
    /// </summary>
    /// <param name="buffer">
    ///     An array of elements. When this method returns, the <paramref name="buffer" /> contains the specified
    ///     array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
    ///     <paramref name="count" /> - 1) replaced by the elements read from the internal buffer.
    /// </param>
    /// <param name="offset">
    ///     The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
    ///     read from the internal buffer.
    /// </param>
    /// <param name="count">The maximum number of elements to read from the internal buffer.</param>
    /// <returns>The total number of elements read into the <paramref name="buffer"/>.</returns>
    public int Read(T[] buffer, int offset, int count)
    {
      int read = 0;
      int counter = count;
      lock (_lockObj)
      {
        #region ORIGINAL (NON CIRCULAR) IMPLEMENTATION
        //count = Math.Min(count, _bufferedElements);
        //int length = Math.Min(count, _buffer.Length - _readOffset);
        //Array.Copy(_buffer, _readOffset, buffer, offset, length); //copy to buffer
        //read += length;
        //_readOffset += read;
        //_readOffset = _readOffset % _buffer.Length;
         
        //if (read < count)
        //{
        //  Array.Copy(_buffer, _readOffset, buffer, offset + read, count - read);
        //  _readOffset += (count - read);
        //  read += (count - read);
        //}

        //_bufferedElements -= read;
        #endregion

        // TODO(Cristian): Do this with Array.Copy
        while ((_bufferedElements > 0) && (counter > 0))
        {
          buffer[offset++] = _buffer[_readOffset++];
          if(_readOffset == _buffer.Length)
          {
            _readOffset = 0;
          }

          --counter;
          --_bufferedElements;
        }

        read = count - counter;
      }

      return read;
    }

    /// <summary>
    /// Gets the size of the internal buffer.
    /// </summary>
    public int Length { get { return _buffer.Length; } }

    /// <summary>
    /// Gets the number of buffered elements.
    /// </summary>
    public int Buffered { get { return _bufferedElements; } }

    /// <summary>
    /// Clears the internal buffer.
    /// </summary>
    public void Clear()
    {
      Array.Clear(_buffer, 0, _buffer.Length);
      //reset all offsets
      _bufferedElements = 0;
      _writeOffset = 0;
      _readOffset = 0;
    }

    private bool _disposed;

    /// <summary>
    /// Disposes the <see cref="CircularBuffer{T}"/> and releases the internal used buffer.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the <see cref="CircularBuffer{T}"/> and releases the internal used buffer.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        _buffer = null;
      }
      _disposed = true;
    }

    /// <summary>
    /// Default destructor which calls the <see cref="Dispose(bool)"/> method.
    /// </summary>
    ~CircularBuffer()
    {
      Dispose(false);
    }

    internal Stream ToStream()
    {
      if (typeof(T) != typeof(byte))
        throw new NotSupportedException("Only byte buffers are supported.");
      return new CircularStream(this as CircularBuffer<byte>);
    }

    private class CircularStream : Stream
    {
      private readonly CircularBuffer<byte> _buffer;

      public CircularStream(CircularBuffer<byte> buffer)
      {
        if (buffer == null)
          throw new ArgumentNullException("buffer");
        _buffer = buffer;
      }


      public override void Flush()
      {
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        throw new NotSupportedException();
      }

      public override void SetLength(long value)
      {
        throw new NotSupportedException();
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        return _buffer.Read(buffer, offset, count);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        _buffer.Write(buffer, offset, count);
      }

      public override bool CanRead
      {
        get { return true; }
      }

      public override bool CanSeek
      {
        get { return false; }
      }

      public override bool CanWrite
      {
        get { return true; }
      }

      public override long Length
      {
        get { return _buffer.Buffered; }
      }

      public override long Position
      {
        get { return 0; }
        set { throw new NotSupportedException(); }
      }
    }
  }
}