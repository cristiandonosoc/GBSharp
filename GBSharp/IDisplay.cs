using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GBSharp
{
  public interface IDisplay
  {
    Bitmap Screen { get; }
  }
}
