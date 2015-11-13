using System.IO;

namespace GBSharp.AudioSpace
{
  public class AudioBuffer : Stream
  {
    #region BUFFER DEFINITION

    private int _sampleRate;
    public int SampleRate { get { return _sampleRate; } }

    private int _channelCount;
    public int ChannelCount { get { return _channelCount; } }

    /// <summary>
    /// This represents the size in bytes of a sample WITHIN a channel.
    /// This means that if we have 2 channels, then the total
    /// sample size would be 2*_sampleSize
    /// </summary>
    private int _sampleSize;
    public int SampleSize { get { return _sampleSize; } }

    private int _milliseconds;
    public int Milliseconds { get { return _milliseconds; } }

    #endregion

    private byte[] _buffer;
    internal byte[] Buffer { get { return _buffer; } }

    // Play Cursor
    private long _playCursor = 0;
    /// <summary>
    /// Identical to Position
    /// </summary>
    public long PlayCursor { get { return _playCursor; } }

    // Write Cursor
    private long _writeCursor = 0;
    public long WriteCursor { get { return _writeCursor; } }
    private bool _writeCursorCreated = false;
    public bool WriteCursorCreated { get { return _writeCursorCreated; } }

    // TODO(Cristian): unhadcode this
    private long _delay = 44 * 2 * 30; // 30 ms delay

    public AudioBuffer(byte[] buffer, int sampleRate, int channelCount, 
                                      int sampleSize, int milliseconds)
    {
      if((sampleRate * channelCount * sampleSize * milliseconds / 1000) != buffer.Length)
      {
        throw new InvalidDataException("Specified Buffer Length is different that actual buffer length");
      }

      _sampleRate = sampleRate;
      _channelCount = channelCount;
      _sampleSize = sampleSize;
      _milliseconds = milliseconds;
      _buffer = buffer;
    }

    public AudioBuffer(int sampleRate, int channelCount, int sampleSize, int milliseconds)
    {
      _sampleRate = sampleRate;
      _channelCount = channelCount;
      _sampleSize = sampleSize;
      _milliseconds = milliseconds;

      _buffer = new byte[sampleRate * channelCount * sampleSize * milliseconds / 1000];
    }

    public override bool CanRead { get { return true; } }

    public override bool CanSeek { get { return true; } }

    public override bool CanWrite { get { return true; } }

    public override long Length { get { return _buffer.Length; } }

    public override long Position
    {
      get { return _playCursor; }
      set
      {
        _playCursor = value;
      }
    }

    public override void Flush()
    {
      for (int i = 0; i < _buffer.Length; ++i)
      {
        _buffer[i] = 0;
      }
      _playCursor = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      for (int i = 0; i < count; ++i)
      {
        buffer[offset + i] = _buffer[_playCursor++];
        
        // We loop
        if(_playCursor == _buffer.Length)
        {
          _playCursor = 0;
        }
      }

      //System.Console.Out.WriteLineAsync(_playCursor.ToString());

      return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      long resOffset;
      if(origin == SeekOrigin.Begin)
      {
        resOffset = offset;

      }
      else if(origin == SeekOrigin.Current)
      {
        resOffset = _playCursor + offset;
      }
      else
      {
        resOffset = _buffer.Length - 1 + offset;
      }

      while (resOffset >= _buffer.Length)
      {
        resOffset -= _buffer.Length;
      }
      _playCursor = resOffset;

      return _playCursor;
    }

    public override void SetLength(long value)
    {
      _buffer = new byte[value];
      _playCursor = 0;
      _writeCursorCreated = false;
    }

    public void CreateWriteCursor()
    {
      _writeCursor = _playCursor + _delay;
      while(_writeCursor >= _buffer.Length)
      {
        _writeCursor -= _buffer.Length;
      }

      _writeCursorCreated = true;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      for(int i = 0; i < count; ++i)
      {
        _buffer[_writeCursor++] = buffer[offset + i];

        if(_writeCursor == _playCursor)
        {
          throw new InvalidDataException("writeCursor cannot pass the playCursor");
        }

        if(_writeCursor == _buffer.Length)
        {
          _writeCursor = 0;
        }
      }
    }
  }
}
