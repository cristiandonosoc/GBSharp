using GBSharp.AudioSpace;

namespace GBSharp
{
  public interface IAPU
  {
    AudioBuffer AudioStream { get; set; }
    uint[] AudioVisualization { get; }
    bool Running { get; set; }
    int VisHeight { get; }
    int VisWidth { get; }

  }
}