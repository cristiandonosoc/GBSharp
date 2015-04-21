using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Catridge
{
    interface ICatridge
    {
        /// <summary>
        /// Loads the contents of the catridge rom from a byte array.
        /// </summary>
        void Load(byte[] rom);

        /// <summary>
        /// See: CatridgeType enum.
        /// </summary>
        CatridgeType Type { get; }

        /// <summary>
        /// Contents of the rom.
        /// </summary>
        byte[] Rom { get; }

        /// <summary>
        /// Game title, max 16 characters.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// ROM size in bytes.
        /// </summary>
        int RomSize { get; }

        /// <summary>
        /// RAM size in bytes.
        /// </summary>
        int RamSize { get; }
    }
}
