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
    internal Cartridge.Cartridge.State CartridgeState { get; set; }
    internal VideoSpace.State DisplayState { get; set; }
  }
}
