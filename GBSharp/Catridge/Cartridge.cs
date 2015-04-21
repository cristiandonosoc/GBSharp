using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Catridge
{
    class Cartridge : ICatridge
    {
        byte[] rom;
        int romSize;
        int ramSize;
        CatridgeType type;
        string title;

        /// <summary>
        /// Loads the contents of the catridge rom from a byte array.
        /// </summary>
        public void Load(byte[] rom)
        {
            // Magic addresses!
            ushort titleAddress = 0x0134;
            ushort catridgeTypeAddress = 0x0147;
            ushort ramSizeAddress = 0x0149;

            // Save rom reference and parse headers
            this.rom = rom;
            this.romSize = this.rom.Length;
            this.ramSize = 0;
            switch (this.rom[ramSizeAddress])
            {
                case 0:
                    ramSize = 0;
                    break;
                case 1:
                    ramSize = 1 * 1024;
                    break;
                case 2:
                    ramSize = 8 * 1024;
                    break;
                case 3:
                    ramSize = 32 * 1024;
                    break;
                case 4:
                    ramSize = 128 * 1024;
                    break;
            }
            this.type = (CatridgeType)this.rom[catridgeTypeAddress];
            this.title = "";
            for (var i = 0; i < 16; ++i)
            {
                if (this.rom[titleAddress + i] == 0)
                {
                    break;
                }
                this.title += (char)this.rom[titleAddress + i];
            }
        }

        /// <summary>
        /// See: CatridgeType enum.
        /// </summary>
        public CatridgeType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Contents of the rom.
        /// </summary>
        public byte[] Rom
        {
            get { return this.rom; }
        }

        /// <summary>
        /// Game title, max 16 characters.
        /// </summary>
        public string Title
        {
            get { return this.title; }
        }

        /// <summary>
        /// ROM size in bytes.
        /// </summary>
        public int RomSize
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// RAM size in bytes.
        /// </summary>
        public int RamSize
        {
            get { throw new NotImplementedException(); }
        }
    }
}
