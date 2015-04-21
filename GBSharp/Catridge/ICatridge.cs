using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Catridge
{
    interface ICatridge
    {
        void Load(Stream rom);

        CatridgeType Type { get; }

        byte[] Rom { get; }
    }
}
