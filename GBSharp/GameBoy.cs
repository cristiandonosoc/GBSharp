using GBSharp.CPU;
using GBSharp.Memory;

namespace GBSharp
{
    public class GameBoy
    {
        CPU.CPU cpu;

        /// <summary>
        /// Class constructor.
        /// Initializes cpu and memory.
        /// </summary>
        public GameBoy()
        {
            this.cpu = new CPU.CPU();
        }
    }
}
