using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Utils
{
  class InvalidInstructionException : Exception
  {
    public InvalidInstructionException() { }
    public InvalidInstructionException(string message) : base(message) { }
    public InvalidInstructionException(string message, Exception inner)
      : base(message, inner)
    {

    }
  }
}
