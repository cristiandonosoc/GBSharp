﻿using System;
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

        public byte FZ
        {
            get
            {
                return (byte)((F & 0x80) == 0x80 ? 1 : 0);
            }

            set
            {
                if (value == 0)
                {
                    F = (byte)(F & (~0x80));
                }
                else
                {
                    F = (byte)(F | 0x80);
                }
            }
        }

        public byte FN
        {
            get
            {
                return (byte)((F & 0x40) == 0x40 ? 1 : 0);
            }

            set
            {
                if (value == 0)
                {
                    F = (byte)(F & (~0x40));
                }
                else
                {
                    F = (byte)(F | 0x40);
                }
            }
        }

        public byte FH
        {
            get
            {
                return (byte)((F & 0x20) == 0x20 ? 1 : 0);
            }

            set
            {
                if (value == 0)
                {
                    F = (byte)(F & (~0x20));
                }
                else
                {
                    F = (byte)(F | 0x20);
                }
            }
        }

        public byte FC
        {
            get
            {
                return (byte)((F & 0x10) == 0x10 ? 1 : 0);
            }

            set
            {
                if (value == 0)
                {
                    F = (byte)(F & (~0x10));
                }
                else
                {
                    F = (byte)(F | 0x10);
                }
            }
        }
    }
}