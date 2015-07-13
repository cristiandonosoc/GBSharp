﻿using System;
using System.Collections.Generic;
using GBSharp.Utils;
using GBSharp.CPUSpace.Dictionaries;

namespace GBSharp.CPUSpace
{
  class CPU : ICPU
  {
    internal CPURegisters registers;
    internal MemorySpace.Memory memory;
    internal InterruptController interruptController;
    internal ushort nextPC;
    internal ushort clock; // 16 bit oscillation counter at 4.1943 MHz
    internal bool halted;
    internal bool stopped;

    #region Lengths and clocks

    Dictionary<byte, byte> instructionLengths = CPUInstructionLengths.Setup();
    Dictionary<byte, byte> instructionClocks = CPUInstructionClocks.Setup();
    Dictionary<byte, byte> CBInstructionLengths = CPUCBInstructionLengths.Setup();
    Dictionary<byte, byte> CBInstructionClocks = CPUCBIntructionClocks.Setup();

    #endregion

    #region Instruction Lambdas

    private Dictionary<byte, Action<ushort>> instructionLambdas;
    private Dictionary<byte, Action<ushort>> CBInstructionLambdas;

    private Dictionary<byte, string> instructionNames;
    private Dictionary<byte, string> CBinstructionNames;

    private void CreateInstructionLambdas()
    {
      instructionLambdas = new Dictionary<byte, Action<ushort>>() {
            // NOP: No Operation
            {0x00, (n)=>{ }},

            // LD BC,nn: Load 16-bit immediate into BC
            {0x01, (n)=>{ registers.BC = n; }},

            // LD (BC),A: Save A to address pointed by BC
            {0x02, (n)=>{ memory.Write(registers.BC, registers.A); }},

            // INC BC: Increment 16-bit BC
            {0x03, (n)=>{ registers.BC++; }},

            // INC B: Increment B
            {0x04, (n)=>{
              registers.B++;

              registers.FZ = (byte)(registers.B == 0 ? 1 : 0);
              registers.FN = 0;
              registers.FH = (byte)((registers.B & 0x0F) == 0x00 ? 1 : 0);
            }},

            // DEC B: Decrement B
            {0x05, (n)=>{
              registers.B--;

              registers.FZ = (byte)(registers.B == 0 ? 1 : 0);
              registers.FN = 1;
              registers.FH = (byte)((registers.B & 0x0F) == 0x0F ? 1 : 0);
            }},

            // LD B,n: Load 8-bit immediate into B
            {0x06, (n)=>{ registers.B = (byte)n; }},

            // RLC A: Rotate A left with carry
            {0x07, (n)=>{
              byte bit7 = (byte)(registers.A >> 7);
              registers.A <<= 1;
              registers.A += bit7;

              registers.FZ = 0; // TODO: CHECK THIS, 2 sources say it's 0, 1 n/a, 1 conditional.
              registers.FN = 0;
              registers.FH = 0;
              registers.FC = bit7;
            }},

            // LD (nn),SP: Save SP to given address
            {0x08, (n)=>memory.Write(n, registers.SP)},

            // ADD HL,BC: Add 16-bit BC to HL
            {0x09, (n)=>{
              var initialH = registers.H;
              registers.HL += registers.BC;

              registers.FN = 0;
              registers.FH = (byte)(((registers.H ^ registers.B ^ initialH) & 0x10) == 0 ? 0 : 1);
              registers.FC = (byte)(initialH > registers.H ? 1 : 0);
            }},

            // LD A,(BC): Load A from address pointed to by BC
            {0x0A, (n) => registers.A = memory.Read(registers.BC)},

            // DEC BC: Decrement 16-bit BC
            {0x0B, (n) => { registers.BC--; }},

            // INC C: Increment C
            {0x0C, (n) => {
              registers.C++;

              registers.FZ = (byte)(registers.C == 0 ? 1 : 0);
              registers.FN = 0;
              registers.FH = (byte)((registers.C & 0x0F) == 0x00 ? 1 : 0);
            }},

            // DEC C: Decrement C
            {0x0D, (n) => {
              registers.C--;

              registers.FZ = (byte)(registers.C == 0 ? 1 : 0);
              registers.FN = 1;
              registers.FH = (byte)((registers.C & 0x0F) == 0x0F ? 1 : 0);
            }},

            // LD C,n: Load 8-bit immediate into C
            {0x0E, (n)=>{ registers.C = (byte)n;}},

            // RRC A: Rotate A right with carry
            {0x0F, (n)=>{
              byte bit0 = (byte)(registers.A & 0x01);
              registers.A >>= 1;
              registers.A += (byte)(bit0 << 7);

              registers.FZ = (byte)(registers.A == 0 ? 1 : 0); // TODO: CHECK THIS, 2 sources say it's 0, 1 n/a, 1 conditional.
              registers.FN = 0;
              registers.FH = 0;
              registers.FC = bit0;
            }},

            // STOP: Stop processor
            {0x10, (n)=>{throw new NotImplementedException("STOP (0x10)");}},

            // LD DE,nn: Load 16-bit immediate into DE
            {0x11, (n)=>{registers.DE = n;}},

            // LD (DE),A: Save A to address pointed by DE
            {0x12, (n)=>{memory.Write(registers.DE, registers.A);}},

            // INC DE: Increment 16-bit DE
            {0x13, (n)=>{registers.DE++;}},

            // INC D: Increment D
            {0x14, (n)=>{
              registers.D++;

              registers.FZ = (byte)(registers.D == 0 ? 1 : 0);
              registers.FN = 0;
              registers.FH = (byte)((registers.D & 0x0F) == 0x00 ? 1 : 0);
            }},

            // DEC D: Decrement D
            {0x15, (n)=>{
              registers.D--;

              registers.FZ = (byte)(registers.D == 0 ? 1 : 0);
              registers.FN = 1;
              registers.FH = (byte)((registers.D & 0x0F) == 0x0F ? 1 : 0);
            }},

            // LD D,n: Load 8-bit immediate into D
            {0x16, (n)=>{registers.D = (byte)n;}},

            // RL A: Rotate A left
            {0x17, (n)=>{
              byte carryOut = (byte)(registers.A >> 7);
              registers.A = (byte)((registers.A << 1) | registers.FC);

              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FN = 0;
              registers.FH = 0;
              registers.FC = carryOut;
            }},

            // JR n: Relative jump by signed immediate
            {0x18, (n)=>{
              // We cast down the input, ignoring the overflows
              sbyte sn = 0;
              unchecked{
                sn = (sbyte)n;
              }
              // We prepare the nextPC variable
              this.nextPC = registers.PC;
              Utils.UtilFuncs.SignedAdd(ref this.nextPC, sn);
            }},

            // ADD HL,DE: Add 16-bit DE to HL
            {0x19, (n)=>{
              var initialH = registers.H;
              registers.HL += registers.DE;

              registers.FN = 0;
              registers.FH = (byte)(((registers.H ^ registers.D ^ initialH) & 0x10) == 0 ? 0 : 1);
              registers.FC = (byte)(initialH > registers.H ? 1 : 0);
            }},

            // LD A,(DE): Load A from address pointed to by DE
            {0x1A, (n)=>{registers.A = memory.Read(registers.DE);}},

            // DEC DE: Decrement 16-bit DE
            {0x1B, (n)=>{registers.DE--;}},

            // INC E: Increment E
            {0x1C, (n)=>{
              registers.E++;

              registers.FZ = (byte)(registers.E == 0 ? 1 : 0);
              registers.FN = 0;
              registers.FH = (byte)((registers.E & 0x0F) == 0x00 ? 1 : 0);
            }},

            // DEC E: Decrement E
            {0x1D, (n)=>{
              registers.E--;

              registers.FZ = (byte)(registers.E == 0 ? 1 : 0);
              registers.FN = 1;
              registers.FH = (byte)((registers.E & 0x0F) == 0x0F ? 1 : 0);
            }},

            // LD E,n: Load 8-bit immediate into E
            {0x1E, (n)=>{registers.E = (byte)n;}},

            // RR A: Rotate A right
            {0x1F, (n)=>{
              byte carryOut = (byte)(registers.A & 0x01);
              registers.A = (byte)((registers.A >> 1) | (registers.FC << 7));

              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FN = 0;
              registers.FH = 0;
              registers.FC = carryOut;
            }},

            // JR NZ,n: Relative jump by signed immediate if last result was not zero
            {0x20, (n)=>{
              if (registers.FZ != 0) { return; }
              // We cast down the input, ignoring the overflows
              sbyte sn = 0;
              unchecked{
                sn = (sbyte)n;
              }
              // We prepare the nextPC variable
              this.nextPC = registers.PC;
              Utils.UtilFuncs.SignedAdd(ref this.nextPC, sn);
            }},

            // LD HL,nn: Load 16-bit immediate into HL
            {0x21, (n)=>{registers.HL = n;}},

            // LDI (HL),A: Save A to address pointed by HL, and increment HL
            {0x22, (n) =>
            {
              memory.Write(registers.HL++, registers.A);
            }},

            // INC HL: Increment 16-bit HL
            {0x23, (n)=>{registers.HL++;}},

            // INC H: Increment H
            {0x24, (n)=>{registers.H++;}},

            // DEC H: Decrement H
            {0x25, (n)=>{registers.H--;}},

            // LD H,n: Load 8-bit immediate into H
            {0x26, (n)=>{registers.H = (byte)n;}},

            // DAA: Adjust A for BCD addition
            {0x27, (n)=>{
              // Based on this table http://www.z80.info/z80syntx.htm#DAA
              ushort value = registers.A;

              if (registers.FN == 0) // ADD, ADC, INC
              {
                // Check first digit
                if ((registers.FH != 0) || (value & 0x0F) > 0x09)
                {
                  registers.A += 0x06;
                }

                // Check second digit
                if ((registers.FC != 0) || ((value & 0xF0) > 0x90) || (((value & 0xF0) > 0x80) && ((value & 0x0F) > 0x09)))
                {
                  registers.A += 0x60;
                  registers.FC = 1;
                }
                else
                {
                  registers.FC = 0;
                }
              }
              else // SUB, SBC, DEC, NEG
              {
                if((registers.FC == 0) && (registers.FH != 0) && ((value & 0xF0) < 0x90) && ((value & 0x0F) > 0x05)) {
                  registers.A += 0xFA;
                  registers.FC = 0;
                } else if ((registers.FC != 0) && (registers.FH == 0) && ((value & 0xF0) > 0x60) && ((value & 0x0F) < 0x0A)) {
                  registers.A += 0xA0;
                  registers.FC = 1;
                } else if ((registers.FC != 0) && (registers.FH != 0) && ((value & 0xF0) > 0x50) && ((value & 0x0F) > 0x05)) {
                  registers.A += 0x9A;
                  registers.FC = 1;
                } else {
                  registers.FC = 0;
                }
              }

              registers.FH = 0;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            }},

            // JR Z,n: Relative jump by signed immediate if last result was zero
            {0x28, (n)=>{
              if (registers.FZ == 0) { return; }
              // We cast down the input, ignoring the overflows
              sbyte sn = 0;
              unchecked{
                sn = (sbyte)n;
              }
              // We prepare the nextPC variable
              this.nextPC = registers.PC;
              Utils.UtilFuncs.SignedAdd(ref this.nextPC, sn);
            }},

            // ADD HL,HL: Add 16-bit HL to HL
            {0x29, (n)=>{
              var initialH = registers.H;
              registers.HL += registers.HL;

              registers.FN = 0;
              registers.FH = (byte)(((registers.H ^ registers.H ^ initialH) & 0x10) == 0 ? 0 : 1);
              registers.FC = (byte)(initialH > registers.H ? 1 : 0);
            }},

            // LDI A,(HL): Load A from address pointed to by HL, and increment HL
            {0x2A, (n)=>{registers.A = memory.Read(registers.HL++);}},

            // DEC HL: Decrement 16-bit HL
            {0x2B, (n)=>{registers.HL--;}},

            // INC L: Increment L
            {0x2C, (n)=>{registers.L++;}},

            // DEC L: Decrement L
            {0x2D, (n)=>{registers.L--;}},

            // LD L,n: Load 8-bit immediate into L
            {0x2E, (n)=>{registers.L = (byte)n;}},

            // CPL: Complement (logical NOT) on A
            {0x2F, (n)=>{registers.A = (byte)~registers.A;}},

            // JR NC,n: Relative jump by signed immediate if last result caused no carry
            {0x30, (n)=>{
              if (registers.FC != 0) { return; }
              // We cast down the input, ignoring the overflows
              sbyte sn = 0;
              unchecked{
                sn = (sbyte)n;
              }
              // We prepare the nextPC variable
              this.nextPC = registers.PC;
              Utils.UtilFuncs.SignedAdd(ref this.nextPC, sn);
            }},

            // LD SP,nn: Load 16-bit immediate into SP
            {0x31, (n)=>{registers.SP = n;}},

            // LDD (HL),A: Save A to address pointed by HL, and decrement HL
            {0x32, (n)=>{memory.Write(registers.HL, registers.A);}},

            // INC SP: Increment 16-bit HL
            {0x33, (n)=>{registers.SP++;}},

            // INC (HL): Increment value pointed by HL
            {0x34, (n)=>{memory.Write(registers.HL, (byte)(memory.Read(registers.HL) + 1));}},

            // DEC (HL): Decrement value pointed by HL
            {0x35, (n)=>{memory.Write(registers.HL, (byte)(memory.Read(registers.HL) - 1));}},

            // LD (HL),n: Load 8-bit immediate into address pointed by HL
            {0x36, (n) => { memory.Write(registers.HL, n);}},

            // SCF: Set carry flag
            {0x37, (n)=>{registers.F = UtilFuncs.SetBit(registers.F, (int)Flags.C);}},

            // JR C,n: Relative jump by signed immediate if last result caused carry
            {0x38, (n)=>{
              if (registers.FC == 0) { return; }
              // We cast down the input, ignoring the overflows
              sbyte sn = 0;
              unchecked{
                sn = (sbyte)n;
              }
              // We prepare the nextPC variable
              this.nextPC = registers.PC;
              Utils.UtilFuncs.SignedAdd(ref this.nextPC, sn);
            }},

            // ADD HL,SP: Add 16-bit SP to HL
            {0x39, (n)=>{
              var initialH = registers.H;
              registers.HL += registers.SP;

              registers.FN = 0;
              registers.FH = (byte)(((registers.H ^ (registers.SP >> 8) ^ initialH) & 0x10) == 0 ? 0 : 1);
              registers.FC = (byte)(initialH > registers.H ? 1 : 0);
            }},

            // LDD A,(HL): Load A from address pointed to by HL, and decrement HL
            {0x3A, (n) => { registers.A = memory.Read(registers.HL--);}},

            // DEC SP: Decrement 16-bit SP
            {0x3B, (n)=>{registers.SP--;}},

            // INC A: Increment A
            {0x3C, (n)=>{registers.A++;}},

            // DEC A: Decrement A
            {0x3D, (n)=>{registers.A--;}},

            // LD A,n: Load 8-bit immediate into A
            {0x3E, (n)=>{registers.A = (byte)n;}},

            // CCF: Complement Carry Flag
            {0x3F, (n)=>{
              registers.FN = 0;
              registers.FH = 0;
              registers.FC = (byte)(~registers.FC & 1);
            }},

            // LD B,B: Copy B to B
            {0x40, (n)=>{registers.B = registers.B;}}, //love this instruction

            // LD B,C: Copy C to B
            {0x41, (n)=>{registers.B = registers.C;}},

            // LD B,D: Copy D to B
            {0x42, (n)=>{registers.B = registers.D;}},

            // LD B,E: Copy E to B
            {0x43, (n)=>{registers.B = registers.E;}},

            // LD B,H: Copy H to B
            {0x44, (n)=>{registers.B = registers.H;}},

            // LD B,L: Copy L to B
            {0x45, (n)=>{registers.B = registers.L;}},

            // LD B,(HL): Copy value pointed by HL to B
            {0x46, (n)=>{registers.B = memory.Read(registers.HL);}},

            // LD B,A: Copy A to B
            {0x47, (n)=>{registers.B = registers.A;}},

            // LD C,B: Copy B to C
            {0x48, (n)=>{registers.C = registers.B;}},

            // LD C,C: Copy C to C
            {0x49, (n)=>{registers.C = registers.C;}},

            // LD C,D: Copy D to C
            {0x4A, (n)=>{registers.C = registers.D;}},

            // LD C,E: Copy E to C
            {0x4B, (n)=>{registers.C = registers.E;}},

            // LD C,H: Copy H to C
            {0x4C, (n)=>{registers.C = registers.H;}},

            // LD C,L: Copy L to C
            {0x4D, (n)=>{registers.C = registers.L;}},

            // LD C,(HL): Copy value pointed by HL to C
            {0x4E, (n)=>{registers.C = memory.Read(registers.HL);}},

            // LD C,A: Copy A to C
            {0x4F, (n)=>{registers.C = registers.A;}},

            // LD D,B: Copy B to D
            {0x50, (n)=>{registers.D = registers.B;}},

            // LD D,C: Copy C to D
            {0x51, (n)=>{registers.D = registers.C;}},

            // LD D,D: Copy D to D
            {0x52, (n)=>{registers.D = registers.D;}},

            // LD D,E: Copy E to D
            {0x53, (n)=>{registers.D = registers.E;}},

            // LD D,H: Copy H to D
            {0x54, (n)=>{registers.D = registers.H;}},

            // LD D,L: Copy L to D
            {0x55, (n)=>{registers.D = registers.L;}},

            // LD D,(HL): Copy value pointed by HL to D
            {0x56, (n)=>{registers.D = memory.Read(registers.HL);}},

            // LD D,A: Copy A to D
            {0x57, (n)=>{registers.D = registers.A;}},

            // LD E,B: Copy B to E
            {0x58, (n)=>{registers.E = registers.B;}},

            // LD E,C: Copy C to E
            {0x59, (n)=>{registers.E = registers.C;}},

            // LD E,D: Copy D to E
            {0x5A, (n)=>{registers.E = registers.D;}},

            // LD E,E: Copy E to E
            {0x5B, (n)=>{registers.E = registers.E;}},

            // LD E,H: Copy H to E
            {0x5C, (n)=>{registers.E = registers.H;}},

            // LD E,L: Copy L to E
            {0x5D, (n)=>{registers.E = registers.L;}},

            // LD E,(HL): Copy value pointed by HL to E
            {0x5E, (n)=>{registers.E = memory.Read(registers.HL);}},

            // LD E,A: Copy A to E
            {0x5F, (n)=>{registers.E = registers.A;}},

            // LD H,B: Copy B to H
            {0x60, (n)=>{registers.H = registers.B;}},

            // LD H,C: Copy C to H
            {0x61, (n)=>{registers.H = registers.C;}},

            // LD H,D: Copy D to H
            {0x62, (n)=>{registers.H = registers.D;}},

            // LD H,E: Copy E to H
            {0x63, (n)=>{registers.H = registers.E;}},

            // LD H,H: Copy H to H
            {0x64, (n)=>{registers.H = registers.H;}},

            // LD H,L: Copy L to H
            {0x65, (n)=>{registers.H = registers.L;}},

            // LD H,(HL): Copy value pointed by HL to H
            {0x66, (n)=>{registers.H = memory.Read(registers.HL);}},

            // LD H,A: Copy A to H
            {0x67, (n)=>{registers.H = registers.A;}},

            // LD L,B: Copy B to L
            {0x68, (n)=>{registers.L = registers.B;}},

            // LD L,C: Copy C to L
            {0x69, (n)=>{registers.L = registers.C;}},

            // LD L,D: Copy D to L
            {0x6A, (n)=>{registers.L = registers.D;}},

            // LD L,E: Copy E to L
            {0x6B, (n)=>{registers.L = registers.E;}},

            // LD L,H: Copy H to L
            {0x6C, (n)=>{registers.L = registers.H;}},

            // LD L,L: Copy L to L
            {0x6D, (n)=>{registers.L = registers.L;}},

            // LD L,(HL): Copy value pointed by HL to L
            {0x6E, (n)=>{registers.L = memory.Read(registers.HL);}},

            // LD L,A: Copy A to L
            {0x6F, (n)=>{registers.L = registers.A;}},

            // LD (HL),B: Copy B to address pointed by HL
            {0x70, (n)=>{memory.Write(registers.HL, registers.B);}},

            // LD (HL),C: Copy C to address pointed by HL
            {0x71, (n)=>{memory.Write(registers.HL, registers.C);}},

            // LD (HL),D: Copy D to address pointed by HL
            {0x72, (n)=>{memory.Write(registers.HL, registers.D);}},

            // LD (HL),E: Copy E to address pointed by HL
            {0x73, (n)=>{memory.Write(registers.HL, registers.E);}},

            // LD (HL),H: Copy H to address pointed by HL
            {0x74, (n)=>{memory.Write(registers.HL, registers.H);}},

            // LD (HL),L: Copy L to address pointed by HL
            {0x75, (n)=>{memory.Write(registers.HL, registers.L);}},

            // HALT: Halt processor
            {0x76, (n)=>{throw new NotImplementedException("HALT (0x76)");}},

            // LD (HL),A: Copy A to address pointed by HL
            {0x77, (n)=>{memory.Write(registers.HL, registers.A);}},

            // LD A,B: Copy B to A
            {0x78, (n)=>{registers.A = registers.B;}},

            // LD A,C: Copy C to A
            {0x79, (n)=>{registers.A = registers.C;}},

            // LD A,D: Copy D to A
            {0x7A, (n)=>{registers.A = registers.D;}},

            // LD A,E: Copy E to A
            {0x7B, (n)=>{registers.A = registers.E;}},

            // LD A,H: Copy H to A
            {0x7C, (n)=>{registers.A = registers.H;}},

            // LD A,L: Copy L to A
            {0x7D, (n)=>{registers.A = registers.L;}},

            // LD A,(HL): Copy value pointed by HL to A
            {0x7E, (n)=>{registers.A = memory.Read(registers.HL);}},

            // LD A,A: Copy A to A
            {0x7F, (n)=>{registers.A = registers.A;}},

            // ADD A,B: Add B to A
            {0x80, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.B;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.B ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,C: Add C to A
            {0x81, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.C;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.C ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,D: Add D to A
            {0x82, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.D;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.D ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,E: Add E to A
            {0x83, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.E;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.E ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,H: Add H to A
            {0x84, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.H;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.H ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,L: Add L to A
            {0x85, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.L;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.L ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,(HL): Add value pointed by HL to A
            {0x86, (n) =>
            {
              byte initial = registers.A;
              byte mem = memory.Read(registers.HL);
              registers.A += mem;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ mem ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADD A,A: Add A to A
            {0x87, (n) =>
            {
              byte initial = registers.A;
              registers.A += registers.A;
              // Update flags
              registers.FC = (byte)((registers.A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.A ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,B: Add B and carry flag to A
            {0x88, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.B;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.B ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,C: Add C and carry flag to A
            {0x89, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.C;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.C ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,D: Add D and carry flag to A
            {0x8A, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.D;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.D ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,E: Add E and carry flag to A
            {0x8B, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.E;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.E ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,H: Add H and carry flag to A
            {0x8C, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.H;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.H ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,L: Add and carry flag L to A
            {0x8D, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.L;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.L ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,(HL): Add value pointed by HL and carry flag to A
            {0x8E, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              byte m = memory.Read(registers.HL);
              A += m;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ m ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // ADC A,A: Add A and carry flag to A
            {0x8F, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += registers.A;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ registers.A ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // SUB A,B: Subtract B from A
            {0x90, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.B,
                            0);
            }},

            // SUB A,C: Subtract C from A
            {0x91, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.C,
                            0);
            }},

            // SUB A,D: Subtract D from A
            {0x92, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.D,
                            0);
            }},

            // SUB A,E: Subtract E from A
            {0x93, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.E,
                            0);
            }},

            // SUB A,H: Subtract H from A
            {0x94, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.H,
                            0);
            }},

            // SUB A,L: Subtract L from A
            {0x95, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.L,
                            0);
            }},

            // SUB A,(HL): Subtract value pointed by HL from A
            {0x96, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            memory.Read(registers.HL),
                            0);
            }},

            // SUB A,A: Subtract A from A
            {0x97, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.A,
                            0);
            }},

            // SBC A,B: Subtract B and carry flag from A
            {0x98, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.B,
                            registers.FC);
            }},

            // SBC A,C: Subtract C and carry flag from A
            {0x99, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.C,
                            registers.FC);
            }},

            // SBC A,D: Subtract D and carry flag from A
            {0x9A, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.D,
                            registers.FC);
            }},

            // SBC A,E: Subtract E and carry flag from A
            {0x9B, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.E,
                            registers.FC);
            }},

            // SBC A,H: Subtract H and carry flag from A
            {0x9C, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.H,
                            registers.FC);
            }},

            // SBC A,L: Subtract and carry flag L from A
            {0x9D, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.L,
                            registers.FC);
            }},

            // SBC A,(HL): Subtract value pointed by HL and carry flag from A
            {0x9E, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            memory.Read(registers.HL),
                            registers.FC);
            }},

            // SBC A,A: Subtract A and carry flag from A
            {0x9F, (n)=>{
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            registers.A,
                            registers.FC);
            }},

            // AND B: Logical AND B against A
            {0xA0, (n) =>
            {
              registers.A &= registers.B;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND C: Logical AND C against A
            {0xA1, (n) =>
            {
              registers.A &= registers.C;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND D: Logical AND D against A
            {0xA2, (n) =>
            {
              registers.A &= registers.D;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND E: Logical AND E against A
            {0xA3, (n) =>
            {
              registers.A &= registers.E;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND H: Logical AND H against A
            {0xA4, (n) =>
            {
              registers.A &= registers.H;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND L: Logical AND L against A
            {0xA5, (n) =>
            {
              registers.A &= registers.L;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND (HL): Logical AND value pointed by HL against A
            {0xA6, (n) =>
            {
              registers.A &= memory.Read(registers.HL);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // AND A: Logical AND A against A
            {0xA7, (n) =>
            {
              registers.A &= registers.A;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)1;
            }},

            // XOR B: Logical XOR B against A
            {0xA8, (n) =>
            {
              registers.A ^= registers.B;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR C: Logical XOR C against A
            {0xA9, (n) =>
            {
              registers.A ^= registers.C;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR D: Logical XOR D against A
            {0xAA, (n) =>
            {
              registers.A ^= registers.D;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR E: Logical XOR E against A
            {0xAB, (n) =>
            {
              registers.A ^= registers.E;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR H: Logical XOR H against A
            {0xAC, (n) =>
            {
              registers.A ^= registers.H;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR L: Logical XOR L against A
            {0xAD, (n) =>
            {
              registers.A ^= registers.L;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR (HL): Logical XOR value pointed by HL against A
            {0xAE, (n) =>
            {
              registers.A ^= memory.Read(registers.HL);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // XOR A: Logical XOR A against A
            {0xAF, (n) =>
            {
              registers.A ^= registers.A;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR B: Logical OR B against A
            {0xB0, (n) =>
            {
              registers.A |= registers.B;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR C: Logical OR C against A
            {0xB1, (n) =>
            {
              registers.A |= registers.C;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR D: Logical OR D against A
            {0xB2, (n) =>
            {
              registers.A |= registers.D;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR E: Logical OR E against A
            {0xB3, (n) =>
            {
              registers.A |= registers.E;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR H: Logical OR H against A
            {0xB4, (n) =>
            {
              registers.A |= registers.H;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR L: Logical OR L against A
            {0xB5, (n) =>
            {
              registers.A |= registers.L;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR (HL): Logical OR value pointed by HL against A
            {0xB6, (n) =>
            {
              registers.A |= memory.Read(registers.HL);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // OR A: Logical OR A against A
            {0xB7, (n) =>
            {
              registers.A |= registers.A;
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.H = (byte)0;
            }},

            // CP B: Compare B against A
            {0xB8, (n)=>{
              byte operand = registers.B;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP C: Compare C against A
            {0xB9, (n)=>{
              byte operand = registers.C;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP D: Compare D against A
            {0xBA, (n)=>{
              byte operand = registers.D;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP E: Compare E against A
            {0xBB, (n)=>{
              byte operand = registers.E;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP H: Compare H against A
            {0xBC, (n)=>{
              byte operand = registers.H;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP L: Compare L against A
            {0xBD, (n)=>{
              byte operand = registers.L;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP (HL): Compare value pointed by HL against A
            {0xBE, (n)=>{
              byte operand = memory.Read(registers.HL);
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // CP A: Compare A against A
            {0xBF, (n)=>{
              byte operand = registers.A;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // RET NZ: Return if last result was not zero
            {0xC0, (n)=>{
              if (registers.FZ != 0) { return; }
              // We load the program counter (high byte is in higher address)
              this.nextPC = memory.Read(registers.SP++);
              this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            }},

            // POP BC: Pop 16-bit value from stack into BC
            {0xC1, (n)=>{
              ushort res = memory.Read(registers.SP++);
              res += (ushort)(memory.Read(registers.SP++) << 8);
              registers.BC = res;
            }},

            // JP NZ,nn: Absolute jump to 16-bit location if last result was not zero
            {0xC2, (n)=>{
              if (registers.FZ != 0) { return; }
              this.nextPC = n;
            }},

            // JP nn: Absolute jump to 16-bit location
            {0xC3, (n)=>{
              this.nextPC = n;
            }},

            // CALL NZ,nn: Call routine at 16-bit location if last result was not zero
            {0xC4, (n)=>{
              if (registers.FZ != 0) { return; }
              // We decrease the SP by 2
              registers.SP -= 2;
              // We but the nextPC in the stack (high byte first get the higher address)
              memory.Write(registers.SP, this.nextPC);
              // We jump
              this.nextPC = n;
            }},

            // PUSH BC: Push 16-bit BC onto stack
            {0xC5, (n)=>{
              registers.SP -= 2;
              // We push the value in the stack (high byte gets the higher address)
              memory.Write(registers.SP, registers.BC);
            }},

            // ADD A,n: Add 8-bit immediate to A
            {0xC6, (n)=>{registers.A += (byte)n;}},

            // RST 0: Call routine at address 0000h
            {0xC7, (n)=>{instructionLambdas[0xCD](0);}},

            // RET Z: Return if last result was zero
            {0xC8, (n)=>{
              if (registers.FZ == 0) { return; }
              // We load the program counter (high byte is in higher address)
              this.nextPC = memory.Read(registers.SP++);
              this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            }},

            // RET: Return to calling routine
            {0xC9, (n)=>{
              // We load the program counter (high byte is in higher address)
              this.nextPC = memory.Read(registers.SP++);
              this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            }},

            // JP Z,nn: Absolute jump to 16-bit location if last result was zero
            {0xCA, (n)=>{
              if (registers.FZ == 0) { return; }
              this.nextPC = n;
            }},

            // Ext ops: Extended operations (two-byte instruction code)
            {0xCB, (n)=>{throw new NotImplementedException("Ext ops (0xCB)");}},

            // CALL Z,nn: Call routine at 16-bit location if last result was zero
            {0xCC, (n)=>{
              if (registers.FZ == 0) { return; }
              // We decrease the SP by 2
              registers.SP -= 2;
              // We but the nextPC in the stack (high byte first get the higher address)
              memory.Write(registers.SP, this.nextPC);
              // We jump
              this.nextPC = n;
            }},

            // CALL nn: Call routine at 16-bit location
            {0xCD, (n)=>{
              // We decrease the SP by 2
              registers.SP -= 2;
              // We but the nextPC in the stack (high byte first get the higher address)
              memory.Write(registers.SP, this.nextPC);
              // We jump
              this.nextPC = n;
            }},

            // ADC A,n: Add 8-bit immediate and carry to A
            {0xCE, (n)=>{
              ushort A = registers.A;
              byte initial = registers.A;
              A += n;
              A += registers.FC;
              registers.A = (byte)A;

              // Update flags
              registers.FC = (byte)((A > 255) ? 1 : 0);
              registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
              registers.FH = (byte)(((registers.A ^ n ^ initial) & 0x10) == 0 ? 0 : 1);
            }},

            // RST 8: Call routine at address 0008h
            {0xCF, (n)=>{instructionLambdas[0xCD](0x08);}},

            // RET NC: Return if last result caused no carry
            {0xD0, (n)=>{
              if (registers.FC != 0) { return; }
              // We load the program counter (high byte is in higher address)
              this.nextPC = memory.Read(registers.SP++);
              this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            }},

            // POP DE: Pop 16-bit value from stack into DE
            {0xD1, (n)=>{
              ushort res = memory.Read(registers.SP++);
              res += (ushort)(memory.Read(registers.SP++) << 8);
              registers.DE = res;
            }},

            // JP NC,nn: Absolute jump to 16-bit location if last result caused no carry
            {0xD2, (n)=>{
              if (registers.FC != 0) { return; }
              this.nextPC = n;
            }},

            // XX: Operation removed in this CPU
            {0xD3, (n)=>{throw new NotImplementedException("XX (0xD3)");}},

            // CALL NC,nn: Call routine at 16-bit location if last result caused no carry
            {0xD4, (n)=>{
              if (registers.FC != 0) { return; }
              // We decrease the SP by 2
              registers.SP -= 2;
              // We but the nextPC in the stack (high byte first get the higher address)
              memory.Write(registers.SP, this.nextPC);
              // We jump
              this.nextPC = n;
            }},

            // PUSH DE: Push 16-bit DE onto stack
            {0xD5, (n)=>{
              registers.SP -= 2;
              // We push the value in the stack (high byte gets the higher address)
              memory.Write(registers.SP, registers.DE);
            }},

            // SUB A,n: Subtract 8-bit immediate from A
            {0xD6, (n)=>{registers.A -= (byte)n;}},

            // RST 10: Call routine at address 0010h
            {0xD7, (n)=>{instructionLambdas[0xCD](0x10);}},

            // RET C: Return if last result caused carry
            {0xD8, (n)=>{
              if (registers.FC == 0) { return; }
              // We load the program counter (high byte is in higher address)
              this.nextPC = memory.Read(registers.SP++);
              this.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            }},

            // RETI: Enable interrupts and return to calling routine
            {0xD9, (n)=>{throw new NotImplementedException("RETI (0xD9)");}},

            // JP C,nn: Absolute jump to 16-bit location if last result caused carry
            {0xDA, (n)=>{
              if (registers.FC == 0) { return; }
              this.nextPC = n;
            }},

            // XX: Operation removed in this CPU
            {0xDB, (n)=>{throw new NotImplementedException("XX (0xDB)");}},

            // CALL C,nn: Call routine at 16-bit location if last result caused carry
            {0xDC, (n)=>{
              if (registers.FC == 0) { return; }
              // We decrease the SP by 2
              registers.SP -= 2;
              // We but the nextPC in the stack (high byte first get the higher address)
              memory.Write(registers.SP, this.nextPC);
              // We jump
              this.nextPC = n;
            }},

            // XX: Operation removed in this CPU
            {0xDD, (n)=>{throw new NotImplementedException("XX (0xDD)");}},

            // SBC A,n: Subtract 8-bit immediate and carry from A
            {0xDE, (n)=>{
              byte substractor = 0;
              unchecked { substractor = (byte)n; }
              UtilFuncs.SBC(ref registers,
                            ref registers.A,
                            substractor,
                            0);
            }},

            // RST 18: Call routine at address 0018h
            {0xDF, (n)=>{instructionLambdas[0xCD](0x18);}},

            // LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)
            {0xE0, (n)=>{memory.Write((ushort)(0xFF00 & n), registers.A);}},

            // POP HL: Pop 16-bit value from stack into HL
            {0xE1, (n)=>{
              ushort res = memory.Read(registers.SP++);
              res += (ushort)(memory.Read(registers.SP++) << 8);
              registers.HL = res;
            }},

            // LDH (C),A: Save A at address pointed to by (FF00h + C)
            {0xE2, (n)=>{memory.Write((ushort)(0xFF00 & registers.C), registers.A);}},

            // XX: Operation removed in this CPU
            {0xE3, (n)=>{throw new NotImplementedException("XX (0xE3)");}},

            // XX: Operation removed in this CPU
            {0xE4, (n)=>{throw new NotImplementedException("XX (0xE4)");}},

            // PUSH HL: Push 16-bit HL onto stack
            {0xE5, (n)=>{
              registers.SP -= 2;
              // We push the value in the stack (high byte gets the higher address)
              memory.Write(registers.SP, registers.HL);
            }},

            // AND n: Logical AND 8-bit immediate against A
            {0xE6, (n)=>{registers.A &= (byte)n;}},

            // RST 20: Call routine at address 0020h
            {0xE7, (n)=>{instructionLambdas[0xCD](0x20);}},

            // ADD SP,d: Add signed 8-bit immediate to SP
            {0xE8, (n)=>{throw new NotImplementedException("ADD SP,d (0xE8)");}},

            // JP (HL): Jump to 16-bit value pointed by HL
            {0xE9, (n)=>{
              this.nextPC = memory.Read(n);
            }},

            // LD (nn),A: Save A at given 16-bit address
            {0xEA, (n)=>{memory.Write(n, registers.A);}},

            // XX: Operation removed in this CPU
            {0xEB, (n)=>{throw new NotImplementedException("XX (0xEB)");}},

            // XX: Operation removed in this CPU
            {0xEC, (n)=>{throw new NotImplementedException("XX (0xEC)");}},

            // XX: Operation removed in this CPU
            {0xED, (n)=>{throw new NotImplementedException("XX (0xED)");}},

            // XOR n: Logical XOR 8-bit immediate against A
            {0xEE, (n)=>{registers.A ^= (byte)n;}},

            // RST 28: Call routine at address 0028h
            {0xEF, (n)=>{instructionLambdas[0xCD](0x28);}},

            // LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)
            {0xF0, (n)=>{registers.A = memory.Read((ushort)(0xFF00 & n));}},

            // POP AF: Pop 16-bit value from stack into AF
            {0xF1, (n)=>{
              ushort res = memory.Read(registers.SP++);
              res += (ushort)(memory.Read(registers.SP++) << 8);
              registers.AF = res;
            }},

            // LDH A, (C): Operation removed in this CPU? (Or Load into A memory from FF00 + C?)
            {0xF2, (n)=>{registers.A = memory.Read((ushort)(0xFF00 & registers.C));}},

            // DI: DIsable interrupts
            {0xF3, (n)=>{Console.WriteLine("NOT IMP DI (0xF3)");}},

            // XX: Operation removed in this CPU
            {0xF4, (n)=>{throw new NotImplementedException("XX (0xF4)");}},

            // PUSH AF: Push 16-bit AF onto stack
            {0xF5, (n)=>{
              registers.SP -= 2;
              // We push the value in the stack (high byte gets the higher address)
              memory.Write(registers.SP, registers.AF);
            }},

            // OR n: Logical OR 8-bit immediate against A
            {0xF6, (n)=>{registers.A |= (byte)n;}},

            // RST 30: Call routine at address 0030h
            {0xF7, (n)=>{instructionLambdas[0xCD](0x30);}},

            // LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL
            {0xF8, (n)=>{throw new NotImplementedException("LDHL SP,d (0xF8)");}},

            // LD SP,HL: Copy HL to SP
            {0xF9, (n)=>{registers.SP = registers.HL;}},

            // LD A,(nn): Load A from given 16-bit address
            {0xFA, (n)=>{registers.A = memory.Read(n);}},

            // EI: Enable interrupts
            {0xFB, (n)=>{throw new NotImplementedException("EI (0xFB)");}},

            // XX: Operation removed in this CPU
            {0xFC, (n)=>{throw new NotImplementedException("XX (0xFC)");}},

            // XX: Operation removed in this CPU
            {0xFD, (n)=>{throw new NotImplementedException("XX (0xFD)");}},

            // CP n: Compare 8-bit immediate against A
            {0xFE, (n)=>{
              byte operand = (byte)n;
              registers.FN = 1;
              registers.FC = 0; // This flag might get changed
              registers.FH = (byte)
                (((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

              if(registers.A == operand) {
                registers.FZ = 1;
              }
              else {
                registers.FZ = 0;
                if(registers.A < operand) {
                  registers.FC = 1;
                }
              }
            }},

            // RST 38: Call routine at address 0038h
            {0xFF, (n)=>{instructionLambdas[0xCD](0x38);}}
        };
    }

    private void CreateCBInstructionLambdas()
    {
      CBInstructionLambdas = new Dictionary<byte, Action<ushort>>()
      {
        // RLC B: Rotate B left with carry
        {0x00, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.B);
                        registers.B = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC C: Rotate C left with carry
        {0x01, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.C);
                        registers.C = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC D: Rotate D left with carry
        {0x02, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.D);
                        registers.D = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC E: Rotate E left with carry
        {0x03, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.E);
                        registers.E = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC H: Rotate H left with carry
        {0x04, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.H);
                        registers.H = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC L: Rotate L left with carry
        {0x05, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.L);
                        registers.L = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC (HL): Rotate value pointed by HL left with carry
        {0x06, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(memory.Read(registers.HL));
                        memory.Write(registers.HL, rotateCarry.Item1);
                        registers.FC = rotateCarry.Item2;
        }},

        // RLC A: Rotate A left with carry
        {0x07, (n) => { var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.A);
                        registers.A = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RRC B: Rotate B right with carry
        {0x08, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.B);
                        registers.B = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RRC C: Rotate C right with carry
        {0x09, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.C);
                        registers.C = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RRC D: Rotate D right with carry
        {0x0A, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.D);
                        registers.D = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RRC E: Rotate E right with carry
        {0x0B, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.E);
                        registers.E = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},


        // RRC H: Rotate H right with carry
        {0x0C, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.H);
                        registers.H = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},


        // RRC L: Rotate L right with carry
        {0x0D, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.L);
                        registers.L = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},


        // RRC (HL): Rotate value pointed by HL right with carry
        {0x0E, (n) => { var rotateCarry = UtilFuncs.RotateRightAndCarry(memory.Read(registers.HL));
                        memory.Write(registers.HL, rotateCarry.Item1);
                        registers.FC = rotateCarry.Item2;
        }},


        // RRC A: Rotate A right with carry
        {0x0F, (n) =>{ var rotateCarry = UtilFuncs.RotateRightAndCarry
          (registers.A);
                        registers.A = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL B: Rotate B left
        {0x10, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.B,1,registers.FC);
                        registers.B = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL C: Rotate C left
        {0x11, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.C,1,registers.FC);
                        registers.C = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL D: Rotate D left
        {0x12, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.D,1,registers.FC);
                        registers.D = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL E: Rotate E left
        {0x13, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.E,1,registers.FC);
                        registers.E = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL H: Rotate H left
        {0x14, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.H,1,registers.FC);
                        registers.H = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL L: Rotate L left
        {0x15, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.L,1,registers.FC);
                        registers.L = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RL (HL): Rotate value pointed by HL left
        {0x16, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(memory.Read(registers.HL),1,registers.FC);
                        memory.Write(registers.HL, rotateCarry.Item1);
                        registers.FC = rotateCarry.Item2;
        }},

        // RL A: Rotate A left
        {0x17, (n) => { var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.A,1,registers.FC);
                        registers.A = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR B: Rotate B right
        {0x18, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.B,1,registers.FC);
                        registers.B = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR C: Rotate C right
        {0x19, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.C,1,registers.FC);
                        registers.C = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR D: Rotate D right
        {0x1A, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.D,1,registers.FC);
                        registers.D = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR E: Rotate E right
        {0x1B, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.E,1,registers.FC);
                        registers.E = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR H: Rotate H right
        {0x1C, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.H,1,registers.FC);
                        registers.H = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR L: Rotate L right
        {0x1D, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.L,1,registers.FC);
                        registers.L = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // RR (HL): Rotate value pointed by HL right
        {0x1E, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(memory.Read(registers.HL),1,registers.FC);
                        memory.Write(registers.HL, rotateCarry.Item1);
                        registers.FC = rotateCarry.Item2;
        }},

        // RR A: Rotate A right
        {0x1F, (n) => { var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.A,1,registers.FC);
                        registers.A = rotateCarry.Item1;
                        registers.FC = rotateCarry.Item2;
        }},

        // SLA B: Shift B left preserving sign
        {0x20, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.B);
                        registers.B = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA C: Shift C left preserving sign
        {0x21, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.C);
                        registers.C = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA D: Shift D left preserving sign
        {0x22, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.D);
                        registers.D = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA E: Shift E left preserving sign
        {0x23, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.E);
                        registers.E = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA H: Shift H left preserving sign
        {0x24, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.H);
                        registers.H = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA L: Shift L left preserving sign
        {0x25, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.L);
                        registers.L = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA (HL): Shift value pointed by HL left preserving sign
        {0x26, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(memory.Read(registers.HL));
                        memory.Write(registers.HL, shiftCarry.Item1);
                        registers.FC = shiftCarry.Item2;
        }},

        // SLA A: Shift A left preserving sign
        {0x27, (n) => { var shiftCarry = UtilFuncs.ShiftLeft(registers.A);
                        registers.A = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},
        // SRA B: Shift B right preserving sign
        {0x28, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.B);
                        registers.B = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA C: Shift C right preserving sign
        {0x29, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.C);
                        registers.C = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA D: Shift D right preserving sign
        {0x2A, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.D);
                        registers.D = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA E: Shift E right preserving sign
        {0x2B, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.E);
                        registers.E = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA H: Shift H right preserving sign
        {0x2C, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.H);
                        registers.H = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA L: Shift L right preserving sign
        {0x2D, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.L);
                        registers.L = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA (HL): Shift value pointed by HL right preserving sign
        {0x2E, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(memory.Read(registers.HL));
                        memory.Write(registers.HL, shiftCarry.Item1);
                        registers.FC = shiftCarry.Item2;
        }},

        // SRA A: Shift A right preserving sign
        {0x2F, (n) => { var shiftCarry = UtilFuncs.ShiftRightArithmetic(registers.A);
                        registers.A = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SWAP B: Swap nybbles in B
        {0x30, (n) => { registers.B = UtilFuncs.SwapNibbles(registers.B); }},

        // SWAP C: Swap nybbles in C
        {0x31, (n) => { registers.C = UtilFuncs.SwapNibbles(registers.C); }},

        // SWAP D: Swap nybbles in D
        {0x32, (n) => { registers.D = UtilFuncs.SwapNibbles(registers.D); }},

        // SWAP E: Swap nybbles in E
        {0x33, (n) => { registers.E = UtilFuncs.SwapNibbles(registers.E); }},

        // SWAP H: Swap nybbles in H
        {0x34, (n) => { registers.H = UtilFuncs.SwapNibbles(registers.H); }},

        // SWAP L: Swap nybbles in L
        {0x35, (n) => {registers.L = UtilFuncs.SwapNibbles(registers.L); }},

        // SWAP (HL): Swap nybbles in value pointed by HL
        {0x36, (n) => { memory.Write(registers.HL, UtilFuncs.SwapNibbles(memory.Read(registers.HL))); }},

        // SWAP A: Swap nybbles in A
        {0x37, (n) => { registers.A = UtilFuncs.SwapNibbles(registers.A); }},

        // SRL B: Shift B right
        {0x38, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.B);
                        registers.B = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL C: Shift C right
        {0x39, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.C);
                        registers.C = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL D: Shift D right
        {0x3A, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.D);
                        registers.D = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL E: Shift E right
        {0x3B, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.E);
                        registers.E = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL H: Shift H right
        {0x3C, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.H);
                        registers.H = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL L: Shift L right
        {0x3D, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.L);
                        registers.L = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL (HL): Shift value pointed by HL right
        {0x3E, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(memory.Read(registers.HL));
                        memory.Write(registers.HL, shiftCarry.Item1);
                        registers.FC = shiftCarry.Item2;
        }},

        // SRL A: Shift A right
        {0x3F, (n) => { var shiftCarry = UtilFuncs.ShiftRightLogic(registers.A);
                        registers.A = shiftCarry.Item1;
                        registers.FC = shiftCarry.Item2;
        }},

        // BIT 0,B: Test bit 0 of B
        {0x40, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,C: Test bit 0 of C
        {0x41, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,D: Test bit 0 of D
        {0x42, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,E: Test bit 0 of E
        {0x43, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,H: Test bit 0 of H
        {0x44, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,L: Test bit 0 of L
        {0x45, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,(HL): Test bit 0 of value pointed by HL
        {0x46, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 0,A: Test bit 0 of A
        {0x47, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 0);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,B: Test bit 1 of B
        {0x48, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,C: Test bit 1 of C
        {0x49, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,D: Test bit 1 of D
        {0x4A, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,E: Test bit 1 of E
        {0x4B, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,H: Test bit 1 of H
        {0x4C, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,L: Test bit 1 of L
        {0x4D, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,(HL): Test bit 1 of value pointed by HL
        {0x4E, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 1,A: Test bit 1 of A
        {0x4F, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 1);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,B: Test bit 2 of B
        {0x50, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,C: Test bit 2 of C
        {0x51, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,D: Test bit 2 of D
        {0x52, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,E: Test bit 2 of E
        {0x53, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,H: Test bit 2 of H
        {0x54, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,L: Test bit 2 of L
        {0x55, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,(HL): Test bit 2 of value pointed by HL
        {0x56, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 2,A: Test bit 2 of A
        {0x57, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 2);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,B: Test bit 3 of B
        {0x58, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,C: Test bit 3 of C
        {0x59, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,D: Test bit 3 of D
        {0x5A, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,E: Test bit 3 of E
        {0x5B, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,H: Test bit 3 of H
        {0x5C, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,L: Test bit 3 of L
        {0x5D, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,(HL): Test bit 3 of value pointed by HL
        {0x5E, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 3,A: Test bit 3 of A
        {0x5F, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 3);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,B: Test bit 4 of B
        {0x60, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,C: Test bit 4 of C
        {0x61, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,D: Test bit 4 of D
        {0x62, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,E: Test bit 4 of E
        {0x63, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,H: Test bit 4 of H
        {0x64, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,L: Test bit 4 of L
        {0x65, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,(HL): Test bit 4 of value pointed by HL
        {0x66, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 4,A: Test bit 4 of A
        {0x67, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 4);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,B: Test bit 5 of B
        {0x68, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,C: Test bit 5 of C
        {0x69, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,D: Test bit 5 of D
        {0x6A, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,E: Test bit 5 of E
        {0x6B, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,H: Test bit 5 of H
        {0x6C, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,L: Test bit 5 of L
        {0x6D, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,(HL): Test bit 5 of value pointed by HL
        {0x6E, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 5,A: Test bit 5 of A
        {0x6F, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 5);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,B: Test bit 6 of B
        {0x70, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,C: Test bit 6 of C
        {0x71, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,D: Test bit 6 of D
        {0x72, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,E: Test bit 6 of E
        {0x73, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,H: Test bit 6 of H
        {0x74, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,L: Test bit 6 of L
        {0x75, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,(HL): Test bit 6 of value pointed by HL
        {0x76, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 6,A: Test bit 6 of A
        {0x77, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 6);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,B: Test bit 7 of B
        {0x78, (n) => { registers.FZ = UtilFuncs.TestBit(registers.B, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,C: Test bit 7 of C
        {0x79, (n) => { registers.FZ = UtilFuncs.TestBit(registers.C, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,D: Test bit 7 of D
        {0x7A, (n) => { registers.FZ = UtilFuncs.TestBit(registers.D, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,E: Test bit 7 of E
        {0x7B, (n) => { registers.FZ = UtilFuncs.TestBit(registers.E, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,H: Test bit 7 of H
        {0x7C, (n) => { registers.FZ = UtilFuncs.TestBit(registers.H, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,L: Test bit 7 of L
        {0x7D, (n) => { registers.FZ = UtilFuncs.TestBit(registers.L, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,(HL): Test bit 7 of value pointed by HL
        {0x7E, (n) => { registers.FZ = UtilFuncs.TestBit(memory.Read(registers.HL), 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // BIT 7,A: Test bit 7 of A
        {0x7F, (n) => { registers.FZ = UtilFuncs.TestBit(registers.A, 7);
                        registers.FN = 0;
                        registers.FH = 1;
        }},

        // RES 0,B: Clear (reset) bit 0 of B
        {0x80, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 0); }},

        // RES 0,C: Clear (reset) bit 0 of C
        {0x81, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 0); }},

        // RES 0,D: Clear (reset) bit 0 of D
        {0x82, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 0); }},

        // RES 0,E: Clear (reset) bit 0 of E
        {0x83, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 0); }},

        // RES 0,H: Clear (reset) bit 0 of H
        {0x84, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 0); }},

        // RES 0,L: Clear (reset) bit 0 of L
        {0x85, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 0); }},

        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        {0x86, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 0)); }},

        // RES 0,A: Clear (reset) bit 0 of A
        {0x87, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 0); }},

        // RES 1,B: Clear (reset) bit 1 of B
        {0x88, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 1); }},

        // RES 1,C: Clear (reset) bit 1 of C
        {0x89, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 1); }},

        // RES 1,D: Clear (reset) bit 1 of D
        {0x8A, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 1); }},

        // RES 1,E: Clear (reset) bit 1 of E
        {0x8B, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 1); }},

        // RES 1,H: Clear (reset) bit 1 of H
        {0x8C, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 1); }},

        // RES 1,L: Clear (reset) bit 1 of L
        {0x8D, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 1); }},

        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        {0x8E, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 1)); }},

        // RES 1,A: Clear (reset) bit 1 of A
        {0x8F, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 1); }},

        // RES 2,B: Clear (reset) bit 2 of B
        {0x90, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 2); }},

        // RES 2,C: Clear (reset) bit 2 of C
        {0x91, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 2); }},

        // RES 2,D: Clear (reset) bit 2 of D
        {0x92, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 2); }},

        // RES 2,E: Clear (reset) bit 2 of E
        {0x93, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 2); }},

        // RES 2,H: Clear (reset) bit 2 of H
        {0x94, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 2); }},

        // RES 2,L: Clear (reset) bit 2 of L
        {0x95, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 2); }},

        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        {0x96, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 2)); }},

        // RES 2,A: Clear (reset) bit 2 of A
        {0x97, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 2); }},

        // RES 3,B: Clear (reset) bit 3 of B
        {0x98, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 3); }},

        // RES 3,C: Clear (reset) bit 3 of C
        {0x99, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 3); }},

        // RES 3,D: Clear (reset) bit 3 of D
        {0x9A, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 3); }},

        // RES 3,E: Clear (reset) bit 3 of E
        {0x9B, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 3); }},

        // RES 3,H: Clear (reset) bit 3 of H
        {0x9C, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 3); }},

        // RES 3,L: Clear (reset) bit 3 of L
        {0x9D, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 3); }},

        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        {0x9E, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 3)); }},

        // RES 3,A: Clear (reset) bit 3 of A
        {0x9F, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 3); }},

        // RES 4,B: Clear (reset) bit 4 of B
        {0xA0, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 4); }},

        // RES 4,C: Clear (reset) bit 4 of C
        {0xA1, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 4); }},

        // RES 4,D: Clear (reset) bit 4 of D
        {0xA2, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 4); }},

        // RES 4,E: Clear (reset) bit 4 of E
        {0xA3, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 4); }},

        // RES 4,H: Clear (reset) bit 4 of H
        {0xA4, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 4); }},

        // RES 4,L: Clear (reset) bit 4 of L
        {0xA5, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 4); }},

        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        {0xA6, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 4)); }},

        // RES 4,A: Clear (reset) bit 4 of A
        {0xA7, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 4); }},

        // RES 5,B: Clear (reset) bit 5 of B
        {0xA8, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 5); }},

        // RES 5,C: Clear (reset) bit 5 of C
        {0xA9, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 5); }},

        // RES 5,D: Clear (reset) bit 5 of D
        {0xAA, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 5); }},

        // RES 5,E: Clear (reset) bit 5 of E
        {0xAB, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 5); }},

        // RES 5,H: Clear (reset) bit 5 of H
        {0xAC, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 5); }},

        // RES 5,L: Clear (reset) bit 5 of L
        {0xAD, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 5); }},

        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        {0xAE, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 5)); }},

        // RES 5,A: Clear (reset) bit 5 of A
        {0xAF, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 5); }},

        // RES 6,B: Clear (reset) bit 6 of B
        {0xB0, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 6); }},

        // RES 6,C: Clear (reset) bit 6 of C
        {0xB1, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 6); }},

        // RES 6,D: Clear (reset) bit 6 of D
        {0xB2, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 6); }},

        // RES 6,E: Clear (reset) bit 6 of E
        {0xB3, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 6); }},

        // RES 6,H: Clear (reset) bit 6 of H
        {0xB4, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 6); }},

        // RES 6,L: Clear (reset) bit 6 of L
        {0xB5, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 6); }},

        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        {0xB6, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 6)); }},

        // RES 6,A: Clear (reset) bit 6 of A
        {0xB7, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 6); }},

        // RES 7,B: Clear (reset) bit 7 of B
        {0xB8, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 7); }},

        // RES 7,C: Clear (reset) bit 7 of C
        {0xB9, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 7); }},

        // RES 7,D: Clear (reset) bit 7 of D
        {0xBA, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 7); }},

        // RES 7,E: Clear (reset) bit 7 of E
        {0xBB, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 7); }},

        // RES 7,H: Clear (reset) bit 7 of H
        {0xBC, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 7); }},

        // RES 7,L: Clear (reset) bit 7 of L
        {0xBD, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 7); }},

        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        {0xBE, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 7)); }},

        // RES 7,A: Clear (reset) bit 7 of A
        {0xBF, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 7); }},

        // SET 0,B: Set bit 0 of B
        {0xC0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 0); }},

        // SET 0,C: Set bit 0 of C
        {0xC1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 0); }},

        // SET 0,D: Set bit 0 of D
        {0xC2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 0); }},

        // SET 0,E: Set bit 0 of E
        {0xC3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 0); }},

        // SET 0,H: Set bit 0 of H
        {0xC4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 0); }},

        // SET 0,L: Set bit 0 of L
        {0xC5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 0); }},

        // SET 0,(HL): Set bit 0 of value pointed by HL
        {0xC6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 0)); }},

        // SET 0,A: Set bit 0 of A
        {0xC7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 0); }},

        // SET 1,B: Set bit 1 of B
        {0xC8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 1); }},

        // SET 1,C: Set bit 1 of C
        {0xC9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 1); }},

        // SET 1,D: Set bit 1 of D
        {0xCA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 1); }},

        // SET 1,E: Set bit 1 of E
        {0xCB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 1); }},

        // SET 1,H: Set bit 1 of H
        {0xCC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 1); }},

        // SET 1,L: Set bit 1 of L
        {0xCD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 1); }},

        // SET 1,(HL): Set bit 1 of value pointed by HL
        {0xCE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 1)); }},

        // SET 1,A: Set bit 1 of A
        {0xCF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 1); }},

        // SET 2,B: Set bit 2 of B
        {0xD0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 2); }},

        // SET 2,C: Set bit 2 of C
        {0xD1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 2); }},

        // SET 2,D: Set bit 2 of D
        {0xD2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 2); }},

        // SET 2,E: Set bit 2 of E
        {0xD3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 2); }},

        // SET 2,H: Set bit 2 of H
        {0xD4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 2); }},

        // SET 2,L: Set bit 2 of L
        {0xD5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 2); }},

        // SET 2,(HL): Set bit 2 of value pointed by HL
        {0xD6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 2)); }},

        // SET 2,A: Set bit 2 of A
        {0xD7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 2); }},

        // SET 3,B: Set bit 3 of B
        {0xD8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 3); }},

        // SET 3,C: Set bit 3 of C
        {0xD9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 3); }},

        // SET 3,D: Set bit 3 of D
        {0xDA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 3); }},

        // SET 3,E: Set bit 3 of E
        {0xDB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 3); }},

        // SET 3,H: Set bit 3 of H
        {0xDC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 3); }},

        // SET 3,L: Set bit 3 of L
        {0xDD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 3); }},

        // SET 3,(HL): Set bit 3 of value pointed by HL
        {0xDE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 3)); }},

        // SET 3,A: Set bit 3 of A
        {0xDF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 3); }},

        // SET 4,B: Set bit 4 of B
        {0xE0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 4); }},

        // SET 4,C: Set bit 4 of C
        {0xE1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 4); }},

        // SET 4,D: Set bit 4 of D
        {0xE2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 4); }},

        // SET 4,E: Set bit 4 of E
        {0xE3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 4); }},

        // SET 4,H: Set bit 4 of H
        {0xE4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 4); }},

        // SET 4,L: Set bit 4 of L
        {0xE5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 4); }},

        // SET 4,(HL): Set bit 4 of value pointed by HL
        {0xE6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 4)); }},

        // SET 4,A: Set bit 4 of A
        {0xE7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 4); }},

        // SET 5,B: Set bit 5 of B
        {0xE8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 5); }},

        // SET 5,C: Set bit 5 of C
        {0xE9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 5); }},

        // SET 5,D: Set bit 5 of D
        {0xEA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 5); }},

        // SET 5,E: Set bit 5 of E
        {0xEB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 5); }},

        // SET 5,H: Set bit 5 of H
        {0xEC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 5); }},

        // SET 5,L: Set bit 5 of L
        {0xED, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 5); }},

        // SET 5,(HL): Set bit 5 of value pointed by HL
        {0xEE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 5)); }},

        // SET 5,A: Set bit 5 of A
        {0xEF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 5); }},

        // SET 6,B: Set bit 6 of B
        {0xF0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 6); }},

        // SET 6,C: Set bit 6 of C
        {0xF1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 6); }},

        // SET 6,D: Set bit 6 of D
        {0xF2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 6); }},

        // SET 6,E: Set bit 6 of E
        {0xF3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 6); }},

        // SET 6,H: Set bit 6 of H
        {0xF4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 6); }},

        // SET 6,L: Set bit 6 of L
        {0xF5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 6); }},

        // SET 6,(HL): Set bit 6 of value pointed by HL
        {0xF6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 6)); }},

        // SET 6,A: Set bit 6 of A
        {0xF7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 6); }},

        // SET 7,B: Set bit 7 of B
        {0xF8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 7); }},

        // SET 7,C: Set bit 7 of C
        {0xF9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 7); }},

        // SET 7,D: Set bit 7 of D
        {0xFA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 7); }},

        // SET 7,E: Set bit 7 of E
        {0xFB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 7); }},

        // SET 7,H: Set bit 7 of H
        {0xFC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 7); }},

        // SET 7,L: Set bit 7 of L
        {0xFD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 7); }},

        // SET 7,(HL): Set bit 7 of value pointed by HL
        {0xFE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 7)); }},

        // SET 7,A: Set bit 7 of A
        {0xFF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 7); }},
      };
    }

    #endregion

    public CPU(MemorySpace.Memory memory)
    {
      //Create Instruction Lambdas
      CreateInstructionLambdas();
      CreateCBInstructionLambdas();
      instructionNames = CPUOpcodeNames.Setup();
      CBinstructionNames = CPUCBOpcodeNames.Setup();

      this.memory = memory;


      this.Initialize();

    }

    /// <summary>
    /// Sets the initial values for the cpu registers and memory mapped registers.
    /// </summary>
    private void Initialize()
    {
      // Reset the clock state
      this.clock = 0;

      this.halted = false;
      this.stopped = false;

      // Magic CPU initial values (after bios execution).
      this.registers.BC = 0x0013;
      this.registers.DE = 0x00D8;
      this.registers.HL = 0x014D;
      this.registers.PC = 0x0100;
      this.registers.SP = 0xFFFE;

      // Initialize memory mapped registers
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

    /// <summary>
    /// Executes one instruction, requiring arbitrary machine and clock cycles.
    /// </summary>
    public void Step()
    {
      // Instruction fetch and decode
      string instructionName;
      byte instructionLength;
      byte clocks;
      Action<ushort> instruction;
      ushort initialClock = this.clock;
      ushort literal = 0;
      ushort opcode = this.memory.Read(this.registers.PC);
      if (opcode != 0xCB)
      {
        // Normal instructions
        instructionLength = this.instructionLengths[(byte)opcode];

        // Extract literal
        if (instructionLength == 2)
        {
          // 8 bit literal
          literal = this.memory.Read((ushort)(this.registers.PC + 1));
        }
        else if (instructionLength == 3)
        {
          // 16 bit literal, little endian
          literal = this.memory.Read((ushort)(this.registers.PC + 1));
          literal += (ushort)(this.memory.Read((ushort)(this.registers.PC + 2)) << 8);
        }

        instruction = this.instructionLambdas[(byte)opcode];
        clocks = this.instructionClocks[(byte)opcode];
        instructionName = instructionNames[(byte)opcode];

      }
      else
      {
        // CB instructions block
        opcode <<= 8;
        opcode += this.memory.Read((ushort)(this.registers.PC + 1));
        instructionLength = this.CBInstructionLengths[(byte)opcode];
        // There is no literal in CB instructions!

        instruction = this.CBInstructionLambdas[(byte)opcode];
        clocks = this.CBInstructionClocks[(byte)opcode];
        instructionName = CBinstructionNames[(byte)opcode];
      }

      // Prepare for program counter movement, but wait for instruction execution.
      // Overwrite nextPC in the instruction lambdas if you want to implement jumps.
      this.nextPC = (ushort)(this.registers.PC + instructionLength);


      Console.WriteLine("Instruction:" + instructionName + " , OpCode: " + opcode.ToString("x") + " , Literal: " + literal.ToString("x"));
      Console.WriteLine(registers.ToString());
      // Execute instruction
      instruction(literal);

      // Push the next program counter value into the real program counter!
      this.registers.PC = this.nextPC;

      // Update clock
      this.clock += clocks;
    }


    CPURegisters ICPU.Registers
    {
      get
      {
        return this.registers;
      }
    }
  }
}
