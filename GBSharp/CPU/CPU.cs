using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPU
{
    class CPU : ICPU
    {
        public CPURegisters registers;

        # region CPU Interface methods
        IEnumerable<IRegister> ICPU.Registers
        {
            get { throw new NotImplementedException(); }
        }

        IRegister ICPU.ProgramCounter
        {
            get { throw new NotImplementedException(); }
        }

        IRegister ICPU.StackPointer
        {
            get { throw new NotImplementedException(); }
        }

        IRegister ICPU.Flags
        {
            get { throw new NotImplementedException(); }
        }
        #endregion


        public CPU()
        {
            // Magic CPU initial values (after bios execution).
            this.registers.BC = 0x0013;
            this.registers.DE = 0x00D8;
            this.registers.HL = 0x014D;
            this.registers.PC = 0x0100;
            this.registers.SP = 0xFFFE;
        }

    }
}
