using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.AudioSpace
{
  public class FrameSequencer
  {
    [Serializable]
    internal class State
    {
      internal uint InternalCounter;
      internal uint Value;
      internal bool Clocked;
    }
    State _state = new State();
    internal State GetState() { return _state; }
    internal void SetState(State state) { _state = state; }

    #region STATE INTERNALS GETTERS/SETTERS

    internal bool Clocked { get { return _state.Clocked; } }
    public uint Value { get { return _state.Value; } }
    public uint InternalCounter { get { return _state.InternalCounter; } }

    #endregion

    internal void Step(uint ticks)
    {
      uint pre = _state.InternalCounter & 0x1FFF;
      _state.InternalCounter += ticks;
      uint post = _state.InternalCounter & 0x1FFF;
      // If the bit 13 changed, the frameSequencer clocked
      _state.Clocked = post < pre;

      _state.Value = _state.InternalCounter >> 13;
    }

    internal void Reset()
    {
      // Next frame sequencer is 0, but the 512 Hz timer remains intact
      _state.InternalCounter |= ~(uint)0x1FFF;
      _state.Value = _state.InternalCounter >> 13;
    }

    internal void AddTicks(uint ticks)
    {
      _state.InternalCounter += ticks;
      _state.Value = _state.InternalCounter >> 13;
    }
  }
}
