using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
  [Serializable]
  internal class SaveStateFileFormat
  {
    internal MemorySpace.Memory.State MemoryState { get; set; }
    internal VideoSpace.State DisplayState { get; set; }
    internal CPUSpace.InterruptController.State InterruptState { get; set; }
    internal CPUSpace.CPU.State CPUState { get; set; }
    internal AudioSpace.APU.State APUState { get; set; }
  }
}
