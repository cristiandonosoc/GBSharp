using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  class NoiseChannel : IChannel
  {
    private int _msSampleRate;
    public int SampleRate { get; private set; }
    public int NumChannels { get; private set; }
    public int SampleSize { get; private set; }
    private int _milliseconds = 1000; // ms of sample

    short[] _buffer;
    public short[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    public bool Enabled { get; internal set; }

    private short _outputValue = 0;

    // TODO(Cristian): Implement
    private const int _volumeConstant = 0;
    public int Volume
    {
      get
      {
        return 0;
      }
    }

    private int _channelIndex;

    internal byte LowFreqByte { get; private set; }
    internal byte HighFreqByte { get; private set; }

    private int _tickThreshold;
    private int _tickCounter;

    private ushort _frequencyFactor;
    internal ushort FrequencyFactor
    {
      get { return _frequencyFactor; }
      private set
      {
        _frequencyFactor = value;
        LowFreqByte = (byte)_frequencyFactor;
        HighFreqByte = (byte)((_frequencyFactor >> 8) & 0x7);
        Frequency = (double)0x20000 / (double)(0x800 - _frequencyFactor);
        _tickThreshold = (int)(GameBoy.ticksPerMillisecond * (1000.0 / (2 * Frequency)));
      }
    }

    internal double Frequency { get; set; }

    private Memory _memory;

    internal NoiseChannel(Memory  memory,
                          int sampleRate, int numChannels, 
                          int sampleSize, int channelIndex)
    {
      _memory = memory;

      SampleRate = sampleRate;
      _msSampleRate = SampleRate / 1000;
      NumChannels = numChannels;
      SampleSize = sampleSize;
      _buffer = new short[SampleRate * NumChannels * SampleSize * _milliseconds / 1000];

      _channelIndex = channelIndex;
    }

    public void HandleMemoryChange(MMR register, byte value)
    {
      switch (register)
      {
        case MMR.NR41:  // Sound Length
          // NR41 is read as ORed with 0xFF
          _memory.LowLevelWrite((ushort)register, 0xFF);
          break;
        case MMR.NR42:
          // NR42 is read as ORed with 0x00
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR43:
          // NR42 is read as ORed with 0x00
          _memory.LowLevelWrite((ushort)register, value);
          break;
        case MMR.NR44:
          // NR42 is read as ORed with 0xBF
          _memory.LowLevelWrite((ushort)register, (byte)(value | 0xBF));
          break;
        default:
          throw new InvalidProgramException("Invalid register received.");
      }
    }

    public void GenerateSamples(int sampleCount)
    {
      while(sampleCount > 0)
      {
        --sampleCount;

        for(int c = 0; c < NumChannels; ++c)
        {
          _buffer[_sampleIndex++] = _outputValue;
        }
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }

    







  }
}
