using GBSharp.MemorySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  internal class SquareChannel
  {
    #region BUFFER DEFINITION

    private int _sampleRate;
    private int _msSampleRate;
    public int SampleRate { get { return _sampleRate; } }

    private int _numChannels;
    public int NumChannels { get { return _numChannels; } }

    private int _sampleSize;
    public int SampleSize { get { return _sampleSize; } }

    private int _milliseconds = 1000; // ms of sample

    short[] _buffer;
    public short[] Buffer { get { return _buffer; } }

    private int _sampleIndex;
    public int SampleCount { get { return _sampleIndex; } }

    internal bool Enabled { get; private set; }

    #endregion

    internal byte LowFreqByte { get; private set; }
    internal byte HighFreqByte { get; private set; }

    private ushort _frequencyFactor;
    internal ushort FrequencyFactor
    {
      get { return _frequencyFactor; }
      private set
      {
        _frequencyFactor = value;
        Frequency = (double)0x20000 / (double)(0x800 - _frequencyFactor);
        _tickThreshold = (int)(GameBoy.ticksPerMillisecond * (1000.0 / (2 * Frequency)));
        _tickCounter = _tickThreshold;
      }
    }

    private int _tickThreshold;

    internal double Frequency { get; private set; }

    private int _tickCounter = 0;

    private bool _up = false;
    private short _outputValue = 0;

    // Registers
    private MMR _sweepRegister;
    private MMR _wavePatternDutyRegister;
    private MMR _volumeEnvelopeRegister;
    private MMR _freqLowRegister;
    private MMR _freqHighRegister;

    private int _channelIndex;

    private int _soundLengthTicks;
    private int _soundLengthTickCounter;
    private bool _continuousOutput;

    internal SquareChannel(int sampleRate, int numChannels, int sampleSize, int channelIndex,
                           MMR sweepRegister, MMR wavePatternDutyRegister, MMR volumeEnvelopeRegister, 
                           MMR freqLowRegister, MMR freqHighRegister)
    {
      _sampleRate = sampleRate;
      _msSampleRate = _sampleRate / 1000;
      _numChannels = numChannels;
      _sampleSize = sampleSize;
      _buffer = new short[_sampleRate * _numChannels * _sampleSize * _milliseconds / 1000];

      _channelIndex = channelIndex;

      // Register setup
      _sweepRegister = sweepRegister;
      _wavePatternDutyRegister = wavePatternDutyRegister;
      _volumeEnvelopeRegister = volumeEnvelopeRegister;
      _freqLowRegister = freqLowRegister;
      _freqHighRegister = freqHighRegister;
    }

    public void GenerateSamples(int sampleCount)
    {
      while(sampleCount > 0)
      {
        --sampleCount;

        for(int c = 0; c < _numChannels; ++c)
        {
          _buffer[_sampleIndex++] = _outputValue;
        }

        // The amount of ticks in a sample
        _tickCounter -= APU.MinimumTickThreshold;
        if(_tickCounter < 0)
        {
          _tickCounter = _tickThreshold;
          _up = !_up;
          _outputValue = (short)(_up ? 8191 : -8192);
        }

        if(!_continuousOutput)
        {
          _soundLengthTickCounter += APU.MinimumTickThreshold;
          if(_soundLengthTickCounter >= _soundLengthTicks)
          {
            Enabled = false;
            // TODO(Cristian): Trigger a change to ouput the correct enabled bit
            //                 NR52
          }
        }
      }
    }

    public void ClearBuffer()
    {
      _sampleIndex = 0;
    }
    
    public void HandleMemoryChange(MMR register, byte value)
    {
      if(register == _sweepRegister)
      {
        // TODO(Cristian): Implement sweep registers
      }
      else if(register == _wavePatternDutyRegister)
      {
        // TODO(Cristian): Wave Pattern Duty
        int soundLenghtFactor = value & 0x3F;
        double soundLengthMs = (double)(0x40 - soundLenghtFactor) / (double)0x100;
        _soundLengthTicks = (int)(GameBoy.ticksPerMillisecond * soundLengthMs);
      } 
      else if(register == _volumeEnvelopeRegister)
      {
        // TODO(Cristian): Implement volume envelope
      }
      else if(register == _freqLowRegister)
      {
        LowFreqByte = value;
        FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | LowFreqByte);
      }
      else if(register == _freqHighRegister)
      {

        HighFreqByte = value;
        FrequencyFactor = (ushort)(((HighFreqByte & 0x7) << 8) | LowFreqByte);

        _continuousOutput = (Utils.UtilFuncs.TestBit(value, 6) == 0);

        Enabled = (Utils.UtilFuncs.TestBit(value, 7) != 0);
        if(Enabled)
        {
          _soundLengthTickCounter = 0;
        }
      }
      else if(register == MMR.NR52)
      {
        Enabled = (Utils.UtilFuncs.TestBit(value, _channelIndex) != 0);
      }
      else
      {
        throw new InvalidProgramException("Invalid register received");
      }
    }
  }
}
