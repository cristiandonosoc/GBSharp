using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPU
{
    class CPU : ICPU
    {
        internal CPURegisters registers;
        internal Memory.Memory memory;


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


        public CPU(Memory.Memory memory)
        {
            // Magic CPU initial values (after bios execution).
            this.registers.BC = 0x0013;
            this.registers.DE = 0x00D8;
            this.registers.HL = 0x014D;
            this.registers.PC = 0x0100;
            this.registers.SP = 0xFFFE;

            // Initialize the memory
            this.memory = memory;
            this.memory.Write(0xFF05, 0x00); // TIMA
            this.memory.Write(0xFF06, 0x00); // TMA
            this.memory.Write(0xFF07, 0x00); // TAC
            this.memory.Write(0xFF10, 0x80); // NR10
            this.memory.Write(0xFF11, 0xBF); // NR11
            this.memory.Write(0xFF12, 0xF3); // NR12
            this.memory.Write(0xFF14, 0xBF); // NR14
            this.memory.Write(0xFF16, 0x3F); // NR21
            this.memory.Write(0xFF17, 0x00); // NR22
            this.memory.Write(0xFF19, 0xBF); // NR24
            this.memory.Write(0xFF1A, 0x7F); // NR30
            this.memory.Write(0xFF1B, 0xFF); // NR31
            this.memory.Write(0xFF1C, 0x9F); // NR32
            this.memory.Write(0xFF1E, 0xBF); // NR33
            this.memory.Write(0xFF20, 0xFF); // NR41
            this.memory.Write(0xFF21, 0x00); // NR42
            this.memory.Write(0xFF22, 0x00); // NR43
            this.memory.Write(0xFF23, 0xBF); // NR30
            this.memory.Write(0xFF24, 0x77); // NR50
            this.memory.Write(0xFF25, 0xF3); // NR51
            this.memory.Write(0xFF26, 0xF1); // NR52 GB: 0xF1, SGB: 0xF0
            this.memory.Write(0xFF40, 0x91); // LCDC
            this.memory.Write(0xFF42, 0x00); // SCY
            this.memory.Write(0xFF43, 0x00); // SCX
            this.memory.Write(0xFF45, 0x00); // LYC
            this.memory.Write(0xFF47, 0xFC); // BGP
            this.memory.Write(0xFF48, 0xFF); // OBP0
            this.memory.Write(0xFF49, 0xFF); // OBP1
            this.memory.Write(0xFF4A, 0x00); // WY
            this.memory.Write(0xFF4B, 0x00); // WX
            this.memory.Write(0xFFFF, 0x00); // IE

        }

    }
}
