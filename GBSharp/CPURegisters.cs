using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    [StructLayout(LayoutKind.Explicit)]
    public struct CPURegisters
    {
        // 1 byte registers
        [FieldOffset(0)]
        public byte F;
        [FieldOffset(1)]
        public byte A;
        [FieldOffset(2)]
        public byte C;
        [FieldOffset(3)]
        public byte B;
        [FieldOffset(4)]
        public byte E;
        [FieldOffset(5)]
        public byte D;
        [FieldOffset(6)]
        public byte L;
        [FieldOffset(7)]
        public byte H;

        // 2 byte registers
        [FieldOffset(8)]
        public ushort SP;
        [FieldOffset(10)]
        public ushort PC;

        // 2 byte "union" registers
        [FieldOffset(0)]
        public ushort AF;
        [FieldOffset(2)]
        public ushort BC;
        [FieldOffset(4)]
        public ushort DE;
        [FieldOffset(6)]
        public ushort HL;

        /// <summary>
        /// Exports the registers values to the external interface.
        /// </summary>
        /// <returns>A List of IRegisters with the current values of the registers.</returns>
        public IEnumerable<IRegister> Export()
        {
            var registers = new List<IRegister>();
            
            // TODO: Replace this with typeof(Register).getFields(...)
            var register = new Register();
            register.Value = this.A;
            register.Size = 1;
            register.Name = "A";
            registers.Add(register);

            register = new Register();
            register.Value = this.B;
            register.Size = 1;
            register.Name = "B";
            registers.Add(register);

            register = new Register();
            register.Value = this.PC;
            register.Size = 2;
            register.Name = "PC";
            registers.Add(register);

            register = new Register();
            register.Value = this.PC;
            register.Size = 2;
            register.Name = "SP";
            registers.Add(register);

            return registers;
        }
    }

    /// <summary>
    /// Struct used to export a register value outside the CPU.
    /// </summary>
    struct Register : IRegister
    {
        public event Action ValueChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public string Name
        {
            get;
            internal set;
        }

        public int Value
        {
            get;
            internal set;
        }

        public int Size
        {
            get;
            internal set;
        }
    }
}
