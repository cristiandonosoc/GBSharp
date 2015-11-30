using GBSharp.MemorySpace;
using GBSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUInstructions
  {
    internal static void Run(CPU cpu, ref CPURegisters registers, ref Memory memory,
                             byte opcode, ushort n
                             //ref Dictionary<byte, Action<ushort>> instructionLambdas
      )
    {
      switch (opcode)
      {
        // NOP: No Operation
        case 0x00:
          {
            break;
          }

        // LD BC,nn: Load 16-bit immediate into BC
        case 0x01:
          {
            registers.BC = n;
            break;
          }

        // LD (BC),A: Save A to address pointed by BC
        case 0x02:
          {
            memory.Write(registers.BC, registers.A);
            break;
          }

        // INC BC: Increment 16-bit BC
        case 0x03:
          {
            registers.BC++;
            break;
          }

        // INC B: Increment B
        case 0x04:
          {
            registers.B++;
            registers.FZ = (byte)(registers.B == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.B & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC B: Decrement B
        case 0x05:
          {
            registers.B--;
            registers.FZ = (byte)(registers.B == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.B & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD B,n: Load 8-bit immediate into B
        case 0x06:
          {
            registers.B = (byte)n;
            break;
          }

        // RLC A: Rotate A left with carry
        case 0x07:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(registers.A);
            registers.A = rotateCarry.Item1;
            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // LD (nn),SP: Save SP to given address
        case 0x08:
          {
            memory.Write(n, registers.SP);
            break;
          }

        // ADD HL,BC: Add 16-bit BC to HL
        case 0x09:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.BC;
            registers.HL += registers.BC;
            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ registers.B ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LD A,(BC): Load A from address pointed to by BC
        case 0x0A:
          {
            registers.A = memory.Read(registers.BC);
            break;
          }

        // DEC BC: Decrement 16-bit BC
        case 0x0B:
          {
            registers.BC--;
            break;
          }

        // INC C: Increment C
        case 0x0C:
          {
            registers.C++;
            registers.FZ = (byte)(registers.C == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.C & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC C: Decrement C
        case 0x0D:
          {
            registers.C--;
            registers.FZ = (byte)(registers.C == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.C & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD C,n: Load 8-bit immediate into C
        case 0x0E:
          {
            registers.C = (byte)n;
            break;
          }

        // RRC A: Rotate A right with carry
        case 0x0F:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(registers.A);
            registers.A = rotateCarry.Item1;
            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // STOP: Stop processor
        case 0x10:
          {
            cpu.stopped = true;
            break;
          }

        // LD DE,nn: Load 16-bit immediate into DE
        case 0x11:
          {
            registers.DE = n;
            break;
          }

        // LD (DE),A: Save A to address pointed by DE
        case 0x12:
          {
            memory.Write(registers.DE, registers.A);
            break;
          }

        // INC DE: Increment 16-bit DE
        case 0x13:
          {
            registers.DE++;
            break;
          }

        // INC D: Increment D
        case 0x14:
          {
            registers.D++;
            registers.FZ = (byte)(registers.D == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.D & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC D: Decrement D
        case 0x15:
          {
            registers.D--;
            registers.FZ = (byte)(registers.D == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.D & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD D,n: Load 8-bit immediate into D
        case 0x16:
          {
            registers.D = (byte)n;
            break;
          }

        // RL A: Rotate A left
        case 0x17:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(registers.A, 1, registers.FC);
            registers.A = rotateCarry.Item1;
            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // JR n: Relative jump by signed immediate
        case 0x18:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            cpu.nextPC = (ushort)(cpu.nextPC + sn);
            break;
          }

        // ADD HL,DE: Add 16-bit DE to HL
        case 0x19:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.DE;
            registers.HL += registers.DE;
            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ registers.D ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LD A,(DE): Load A from address pointed to by DE
        case 0x1A:
          {
            registers.A = memory.Read(registers.DE);
            break;
          }

        // DEC DE: Decrement 16-bit DE
        case 0x1B:
          {
            registers.DE--;
            break;
          }

        // INC E: Increment E
        case 0x1C:
          {
            registers.E++;
            registers.FZ = (byte)(registers.E == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.E & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC E: Decrement E
        case 0x1D:
          {
            registers.E--;
            registers.FZ = (byte)(registers.E == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.E & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD E,n: Load 8-bit immediate into E
        case 0x1E:
          {
            registers.E = (byte)n;
            break;
          }

        // RR A: Rotate A right
        case 0x1F:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(registers.A, 1, registers.FC);
            registers.A = rotateCarry.Item1;
            registers.FC = rotateCarry.Item2;
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // JR NZ,n: Relative jump by signed immediate if last result was not zero
        case 0x20:
          {
            if (registers.FZ != 0) { return; }
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            cpu.nextPC = (ushort)(cpu.nextPC + sn);
            cpu._currentInstruction.Ticks = 12;
            break;
          }

        // LD HL,nn: Load 16-bit immediate into HL
        case 0x21:
          {
            registers.HL = n;
            break;
          }

        // LDI (HL),A: Save A to address pointed by HL, and increment HL
        case 0x22:
          {
            memory.Write(registers.HL++, registers.A);
            break;
          }

        // INC HL: Increment 16-bit HL
        case 0x23:
          {
            registers.HL++;
            break;
          }

        // INC H: Increment H
        case 0x24:
          {
            registers.H++;
            registers.FZ = (byte)(registers.H == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.H & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC H: Decrement H
        case 0x25:
          {
            registers.H--;
            registers.FZ = (byte)(registers.H == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.H & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD H,n: Load 8-bit immediate into H
        case 0x26:
          {
            registers.H = (byte)n;
            break;
          }

        // DAA: Adjust A for BCD addition
        case 0x27:
          {
            int value = registers.A;
            if (registers.FN != 0) // ADD, ADC, INC
            {
              if (registers.FH != 0) { value = (value - 0x06) & 0xFF; }
              if (registers.FC != 0) { value -= 0x60; }
            }
            else // SUB, SBC, DEC, NEG
            {
              if ((registers.FH != 0) || ((value & 0x0F) > 0x09)) { value += 0x06; }
              if ((registers.FC != 0) || (value > 0x9F)) { value += 0x60; }
            }
            registers.FH = 0;
            //registers.FC = 0;
            if ((value & 0x100) == 0x100) { registers.FC = 1; }
            value &= 0xFF;
            registers.FZ = (byte)(value == 0 ? 1 : 0);
            registers.A = (byte)value;
            break;
          }

        // JR Z,n: Relative jump by signed immediate if last result was zero
        case 0x28:
          {
            if (registers.FZ == 0) { return; }
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            cpu.nextPC = (ushort)(cpu.nextPC + sn);
            cpu._currentInstruction.Ticks = 12;
            break;
          }

        // ADD HL,HL: Add 16-bit HL to HL
        case 0x29:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.HL;
            registers.HL += registers.HL;
            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ initialH ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LDI A,(HL): Load A from address pointed to by HL, and increment HL
        case 0x2A:
          {
            registers.A = memory.Read(registers.HL++);
            break;
          }

        // DEC HL: Decrement 16-bit HL
        case 0x2B:
          {
            registers.HL--;
            break;
          }

        // INC L: Increment L
        case 0x2C:
          {
            registers.L++;
            registers.FZ = (byte)(registers.L == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.L & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC L: Decrement L
        case 0x2D:
          {
            registers.L--;
            registers.FZ = (byte)(registers.L == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.L & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD L,n: Load 8-bit immediate into L
        case 0x2E:
          {
            registers.L = (byte)n;
            break;
          }

        // CPL: Complement (logical NOT) on A
        case 0x2F:
          {
            registers.A = (byte)~registers.A;
            registers.FN = 1;
            registers.FH = 1;
            break;
          }

        // JR NC,n: Relative jump by signed immediate if last result caused no carry
        case 0x30:
          {
            if (registers.FC != 0) { return; }
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            cpu.nextPC = (ushort)(cpu.nextPC + sn);
            cpu._currentInstruction.Ticks = 12;
            break;
          }

        // LD SP,nn: Load 16-bit immediate into SP
        case 0x31:
          {
            registers.SP = n;
            break;
          }

        // LDD (HL),A: Save A to address pointed by HL, and decrement HL
        case 0x32:
          {
            memory.Write(registers.HL--, registers.A);
            break;
          }

        // INC SP: Increment 16-bit HL
        case 0x33:
          {
            registers.SP++;
            break;
          }

        // INC (HL): Increment value pointed by HL
        case 0x34:
          {
            byte value = memory.Read(registers.HL);
            ++value;
            memory.Write(registers.HL, value);
            registers.FZ = (byte)(value == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((value & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC (HL): Decrement value pointed by HL
        case 0x35:
          {
            byte value = memory.Read(registers.HL);
            --value;
            memory.Write(registers.HL, value);
            registers.FZ = (byte)(value == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((value & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD (HL),n: Load 8-bit immediate into address pointed by HL
        case 0x36:
          {
            memory.Write(registers.HL, (byte)n);
            break;
          }

        // SCF: Set carry flag
        case 0x37:
          {
            registers.FC = 1;
            registers.FN = 0;
            registers.FH = 0;
            break;
          }

        // JR C,n: Relative jump by signed immediate if last result caused carry
        case 0x38:
          {
            if (registers.FC == 0) { return; }
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            cpu.nextPC = (ushort)(cpu.nextPC + sn);
            cpu._currentInstruction.Ticks = 12;
            break;
          }

        // ADD HL,SP: Add 16-bit SP to HL
        case 0x39:
          {
            var initialH = registers.H;
            int res = registers.HL + registers.SP;
            registers.HL += registers.SP;
            registers.FN = 0;
            registers.FH = (byte)(((registers.H ^ (registers.SP >> 8) ^ initialH) & 0x10) == 0 ? 0 : 1);
            registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LDD A,(HL): Load A from address pointed to by HL, and decrement HL
        case 0x3A:
          {
            registers.A = memory.Read(registers.HL--);
            break;
          }

        // DEC SP: Decrement 16-bit SP
        case 0x3B:
          {
            registers.SP--;
            break;
          }

        // INC A: Increment A
        case 0x3C:
          {
            registers.A++;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FN = 0;
            registers.FH = (byte)((registers.A & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC A: Decrement A
        case 0x3D:
          {
            registers.A--;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FN = 1;
            registers.FH = (byte)((registers.A & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD A,n: Load 8-bit immediate into A
        case 0x3E:
          {
            registers.A = (byte)n;
            break;
          }

        // CCF: Complement Carry Flag
        case 0x3F:
          {
            registers.FN = 0;
            registers.FH = 0;
            registers.FC = (byte)(~registers.FC & 1);
            break;
          }

        // LD B,B: Copy B to B
        case 0x40:
          {
#pragma warning disable
            registers.B = registers.B;
#pragma warning restore
            break;
          }

        // LD B,C: Copy C to B
        case 0x41:
          {
            registers.B = registers.C;
            break;
          }

        // LD B,D: Copy D to B
        case 0x42:
          {
            registers.B = registers.D;
            break;
          }

        // LD B,E: Copy E to B
        case 0x43:
          {
            registers.B = registers.E;
            break;
          }

        // LD B,H: Copy H to B
        case 0x44:
          {
            registers.B = registers.H;
            break;
          }

        // LD B,L: Copy L to B
        case 0x45:
          {
            registers.B = registers.L;
            break;
          }

        // LD B,(HL): Copy value pointed by HL to B
        case 0x46:
          {
            registers.B = memory.Read(registers.HL);
            break;
          }

        // LD B,A: Copy A to B
        case 0x47:
          {
            registers.B = registers.A;
            break;
          }

        // LD C,B: Copy B to C
        case 0x48:
          {
            registers.C = registers.B;
            break;
          }

        // LD C,C: Copy C to C
        case 0x49:
          {
#pragma warning disable
            registers.C = registers.C;
#pragma warning restore
            break;
          }

        // LD C,D: Copy D to C
        case 0x4A:
          {
            registers.C = registers.D;
            break;
          }

        // LD C,E: Copy E to C
        case 0x4B:
          {
            registers.C = registers.E;
            break;
          }

        // LD C,H: Copy H to C
        case 0x4C:
          {
            registers.C = registers.H;
            break;
          }

        // LD C,L: Copy L to C
        case 0x4D:
          {
            registers.C = registers.L;
            break;
          }

        // LD C,(HL): Copy value pointed by HL to C
        case 0x4E:
          {
            registers.C = memory.Read(registers.HL);
            break;
          }

        // LD C,A: Copy A to C
        case 0x4F:
          {
            registers.C = registers.A;
            break;
          }

        // LD D,B: Copy B to D
        case 0x50:
          {
            registers.D = registers.B;
            break;
          }

        // LD D,C: Copy C to D
        case 0x51:
          {
            registers.D = registers.C;
            break;
          }

        // LD D,D: Copy D to D
        case 0x52:
          {
#pragma warning disable
            registers.D = registers.D;
#pragma warning restore
            break;
          }

        // LD D,E: Copy E to D
        case 0x53:
          {
            registers.D = registers.E;
            break;
          }

        // LD D,H: Copy H to D
        case 0x54:
          {
            registers.D = registers.H;
            break;
          }

        // LD D,L: Copy L to D
        case 0x55:
          {
            registers.D = registers.L;
            break;
          }

        // LD D,(HL): Copy value pointed by HL to D
        case 0x56:
          {
            registers.D = memory.Read(registers.HL);
            break;
          }

        // LD D,A: Copy A to D
        case 0x57:
          {
            registers.D = registers.A;
            break;
          }

        // LD E,B: Copy B to E
        case 0x58:
          {
            registers.E = registers.B;
            break;
          }

        // LD E,C: Copy C to E
        case 0x59:
          {
            registers.E = registers.C;
            break;
          }

        // LD E,D: Copy D to E
        case 0x5A:
          {
            registers.E = registers.D;
            break;
          }

        // LD E,E: Copy E to E
        case 0x5B:
          {
#pragma warning disable
            registers.E = registers.E;
#pragma warning restore
            break;
          }

        // LD E,H: Copy H to E
        case 0x5C:
          {
            registers.E = registers.H;
            break;
          }

        // LD E,L: Copy L to E
        case 0x5D:
          {
            registers.E = registers.L;
            break;
          }

        // LD E,(HL): Copy value pointed by HL to E
        case 0x5E:
          {
            registers.E = memory.Read(registers.HL);
            break;
          }

        // LD E,A: Copy A to E
        case 0x5F:
          {
            registers.E = registers.A;
            break;
          }

        // LD H,B: Copy B to H
        case 0x60:
          {
            registers.H = registers.B;
            break;
          }

        // LD H,C: Copy C to H
        case 0x61:
          {
            registers.H = registers.C;
            break;
          }

        // LD H,D: Copy D to H
        case 0x62:
          {
            registers.H = registers.D;
            break;
          }

        // LD H,E: Copy E to H
        case 0x63:
          {
            registers.H = registers.E;
            break;
          }

        // LD H,H: Copy H to H
        case 0x64:
          {
#pragma warning disable
            registers.H = registers.H;
#pragma warning restore
            break;
          }

        // LD H,L: Copy L to H
        case 0x65:
          {
            registers.H = registers.L;
            break;
          }

        // LD H,(HL): Copy value pointed by HL to H
        case 0x66:
          {
            registers.H = memory.Read(registers.HL);
            break;
          }

        // LD H,A: Copy A to H
        case 0x67:
          {
            registers.H = registers.A;
            break;
          }

        // LD L,B: Copy B to L
        case 0x68:
          {
            registers.L = registers.B;
            break;
          }

        // LD L,C: Copy C to L
        case 0x69:
          {
            registers.L = registers.C;
            break;
          }

        // LD L,D: Copy D to L
        case 0x6A:
          {
            registers.L = registers.D;
            break;
          }

        // LD L,E: Copy E to L
        case 0x6B:
          {
            registers.L = registers.E;
            break;
          }

        // LD L,H: Copy H to L
        case 0x6C:
          {
            registers.L = registers.H;
            break;
          }

        // LD L,L: Copy L to L
        case 0x6D:
          {
#pragma warning disable
            registers.L = registers.L;
#pragma warning restore
            break;
          }

        // LD L,(HL): Copy value pointed by HL to L
        case 0x6E:
          {
            registers.L = memory.Read(registers.HL);
            break;
          }

        // LD L,A: Copy A to L
        case 0x6F:
          {
            registers.L = registers.A;
            break;
          }

        // LD (HL),B: Copy B to address pointed by HL
        case 0x70:
          {
            memory.Write(registers.HL, registers.B);
            break;
          }

        // LD (HL),C: Copy C to address pointed by HL
        case 0x71:
          {
            memory.Write(registers.HL, registers.C);
            break;
          }

        // LD (HL),D: Copy D to address pointed by HL
        case 0x72:
          {
            memory.Write(registers.HL, registers.D);
            break;
          }

        // LD (HL),E: Copy E to address pointed by HL
        case 0x73:
          {
            memory.Write(registers.HL, registers.E);
            break;
          }

        // LD (HL),H: Copy H to address pointed by HL
        case 0x74:
          {
            memory.Write(registers.HL, registers.H);
            break;
          }

        // LD (HL),L: Copy L to address pointed by HL
        case 0x75:
          {
            memory.Write(registers.HL, registers.L);
            break;
          }

        // HALT: Halt processor
        case 0x76:
          {
            if (cpu.interruptController.InterruptMasterEnable)
            {
              cpu.halted = true;
            }
            else
            {
              // TODO(Cristian): See the double halt load
              cpu.halted = true;
              cpu.haltLoad = true;
            }
            break;
          }

        // LD (HL),A: Copy A to address pointed by HL
        case 0x77:
          {
            memory.Write(registers.HL, registers.A);
            break;
          }

        // LD A,B: Copy B to A
        case 0x78:
          {
            registers.A = registers.B;
            break;
          }

        // LD A,C: Copy C to A
        case 0x79:
          {
            registers.A = registers.C;
            break;
          }

        // LD A,D: Copy D to A
        case 0x7A:
          {
            registers.A = registers.D;
            break;
          }

        // LD A,E: Copy E to A
        case 0x7B:
          {
            registers.A = registers.E;
            break;
          }

        // LD A,H: Copy H to A
        case 0x7C:
          {
            registers.A = registers.H;
            break;
          }

        // LD A,L: Copy L to A
        case 0x7D:
          {
            registers.A = registers.L;
            break;
          }

        // LD A,(HL): Copy value pointed by HL to A
        case 0x7E:
          {
            registers.A = memory.Read(registers.HL);
            break;
          }

        // LD A,A: Copy A to A
        case 0x7F:
          {
#pragma warning disable
            registers.A = registers.A;
#pragma warning restore
            break;
          }

        // ADD A,B: Add B to A
        case 0x80:
          {
            byte initial = registers.A;
            byte toSum = registers.B;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,C: Add C to A
        case 0x81:
          {
            byte initial = registers.A;
            byte toSum = registers.C;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,D: Add D to A
        case 0x82:
          {
            byte initial = registers.A;
            byte toSum = registers.D;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,E: Add E to A
        case 0x83:
          {
            byte initial = registers.A;
            byte toSum = registers.E;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,H: Add H to A
        case 0x84:
          {
            byte initial = registers.A;
            byte toSum = registers.H;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,L: Add L to A
        case 0x85:
          {
            byte initial = registers.A;
            byte toSum = registers.L;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,(HL): Add value pointed by HL to A
        case 0x86:
          {
            byte initial = registers.A;
            byte toSum = memory.Read(registers.HL);
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADD A,A: Add A to A
        case 0x87:
          {
            byte initial = registers.A;
            byte toSum = registers.A;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,B: Add B and carry flag to A
        case 0x88:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.B;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.B ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,C: Add C and carry flag to A
        case 0x89:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.C;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.C ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,D: Add D and carry flag to A
        case 0x8A:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.D;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.D ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,E: Add E and carry flag to A
        case 0x8B:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.E;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.E ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,H: Add H and carry flag to A
        case 0x8C:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.H;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.H ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,L: Add and carry flag L to A
        case 0x8D:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.L;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ registers.L ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // ADC A,(HL): Add value pointed by HL and carry flag to A
        case 0x8E:
          {
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
            registers.FN = 0;
            break;
          }

        // ADC A,A: Add A and carry flag to A
        case 0x8F:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += registers.A;
            A += registers.FC;
            registers.A = (byte)(A & 0xFF);
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ initial ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // SUB A,B: Subtract B from A
        case 0x90:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.B,
                          0);
            break;
          }

        // SUB A,C: Subtract C from A
        case 0x91:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.C,
                          0);
            break;
          }

        // SUB A,D: Subtract D from A
        case 0x92:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.D,
                          0);
            break;
          }

        // SUB A,E: Subtract E from A
        case 0x93:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.E,
                          0);
            break;
          }

        // SUB A,H: Subtract H from A
        case 0x94:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.H,
                          0);
            break;
          }

        // SUB A,L: Subtract L from A
        case 0x95:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.L,
                          0);
            break;
          }

        // SUB A,(HL): Subtract value pointed by HL from A
        case 0x96:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          memory.Read(registers.HL),
                          0);
            break;
          }

        // SUB A,A: Subtract A from A
        case 0x97:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.A,
                          0);
            break;
          }

        // SBC A,B: Subtract B and carry flag from A
        case 0x98:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.B,
                          registers.FC);
            break;
          }

        // SBC A,C: Subtract C and carry flag from A
        case 0x99:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.C,
                          registers.FC);
            break;
          }

        // SBC A,D: Subtract D and carry flag from A
        case 0x9A:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.D,
                          registers.FC);
            break;
          }

        // SBC A,E: Subtract E and carry flag from A
        case 0x9B:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.E,
                          registers.FC);
            break;
          }

        // SBC A,H: Subtract H and carry flag from A
        case 0x9C:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.H,
                          registers.FC);
            break;
          }

        // SBC A,L: Subtract and carry flag L from A
        case 0x9D:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.L,
                          registers.FC);
            break;
          }

        // SBC A,(HL): Subtract value pointed by HL and carry flag from A
        case 0x9E:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          memory.Read(registers.HL),
                          registers.FC);
            break;
          }

        // SBC A,A: Subtract A and carry flag from A
        case 0x9F:
          {
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          registers.A,
                          registers.FC);
            break;
          }

        // AND B: Logical AND B against A
        case 0xA0:
          {
            registers.A &= registers.B;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND C: Logical AND C against A
        case 0xA1:
          {
            registers.A &= registers.C;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND D: Logical AND D against A
        case 0xA2:
          {
            registers.A &= registers.D;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND E: Logical AND E against A
        case 0xA3:
          {
            registers.A &= registers.E;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND H: Logical AND H against A
        case 0xA4:
          {
            registers.A &= registers.H;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND L: Logical AND L against A
        case 0xA5:
          {
            registers.A &= registers.L;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND (HL): Logical AND value pointed by HL against A
        case 0xA6:
          {
            registers.A &= memory.Read(registers.HL);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // AND A: Logical AND A against A
        case 0xA7:
          {
            registers.A &= registers.A;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR B: Logical XOR B against A
        case 0xA8:
          {
            registers.A ^= registers.B;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR C: Logical XOR C against A
        case 0xA9:
          {
            registers.A ^= registers.C;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR D: Logical XOR D against A
        case 0xAA:
          {
            registers.A ^= registers.D;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR E: Logical XOR E against A
        case 0xAB:
          {
            registers.A ^= registers.E;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR H: Logical XOR H against A
        case 0xAC:
          {
            registers.A ^= registers.H;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR L: Logical XOR L against A
        case 0xAD:
          {
            registers.A ^= registers.L;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR (HL): Logical XOR value pointed by HL against A
        case 0xAE:
          {
            registers.A ^= memory.Read(registers.HL);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // XOR A: Logical XOR A against A
        case 0xAF:
          {
            registers.A ^= registers.A;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR B: Logical OR B against A
        case 0xB0:
          {
            registers.A |= registers.B;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR C: Logical OR C against A
        case 0xB1:
          {
            registers.A |= registers.C;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR D: Logical OR D against A
        case 0xB2:
          {
            registers.A |= registers.D;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR E: Logical OR E against A
        case 0xB3:
          {
            registers.A |= registers.E;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR H: Logical OR H against A
        case 0xB4:
          {
            registers.A |= registers.H;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR L: Logical OR L against A
        case 0xB5:
          {
            registers.A |= registers.L;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR (HL): Logical OR value pointed by HL against A
        case 0xB6:
          {
            registers.A |= memory.Read(registers.HL);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // OR A: Logical OR A against A
        case 0xB7:
          {
            registers.A |= registers.A;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // CP B: Compare B against A
        case 0xB8:
          {
            byte operand = registers.B;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP C: Compare C against A
        case 0xB9:
          {
            byte operand = registers.C;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP D: Compare D against A
        case 0xBA:
          {
            byte operand = registers.D;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP E: Compare E against A
        case 0xBB:
          {
            byte operand = registers.E;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP H: Compare H against A
        case 0xBC:
          {
            byte operand = registers.H;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP L: Compare L against A
        case 0xBD:
          {
            byte operand = registers.L;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP (HL): Compare value pointed by HL against A
        case 0xBE:
          {
            byte operand = memory.Read(registers.HL);
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // CP A: Compare A against A
        case 0xBF:
          {
            byte operand = registers.A;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // RET NZ: Return if last result was not zero
        case 0xC0:
          {
            if (registers.FZ != 0) { return; }
            // We load the program counter (high byte is in higher address)
            cpu.nextPC = memory.Read(registers.SP++);
            cpu.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            cpu._currentInstruction.Ticks = 20;
            break;
          }

        // POP BC: Pop 16-bit value from stack into BC
        case 0xC1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.BC = res;
            break;
          }

        // JP NZ,nn: Absolute jump to 16-bit location if last result was not zero
        case 0xC2:
          {
            if (registers.FZ != 0) { return; }
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 16;
            break;
          }

        // JP nn: Absolute jump to 16-bit location
        case 0xC3:
          {
            cpu.nextPC = n;
            break;
          }

        // CALL NZ,nn: Call routine at 16-bit location if last result was not zero
        case 0xC4:
          {
            if (registers.FZ != 0) { return; }
            registers.SP -= 2;
            memory.Write(registers.SP, cpu.nextPC);
            // We jump
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 24;
            break;
          }

        // PUSH BC: Push 16-bit BC onto stack
        case 0xC5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.BC);
            break;
          }

        // ADD A,n: Add 8-bit immediate to A
        case 0xC6:
          {
            byte initial = registers.A;
            byte toSum = (byte)n;
            int sum = initial + toSum;
            registers.A += toSum;
            // Update flags
            registers.FC = (byte)((sum > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // RST 0: Call routine at address 0000h
        case 0xC7:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0);
            //instructionLambdas[0xCD](0);
            break;
          }

        // RET Z: Return if last result was zero
        case 0xC8:
          {
            if (registers.FZ == 0) { return; }
            // We load the program counter (high byte is in higher address)
            cpu.nextPC = memory.Read(registers.SP++);
            cpu.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            cpu._currentInstruction.Ticks = 20;
            break;
          }

        // RET: Return to calling routine
        case 0xC9:
          {
            // We load the program counter (high byte is in higher address)
            cpu.nextPC = memory.Read(registers.SP++);
            cpu.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            break;
          }

        // JP Z,nn: Absolute jump to 16-bit location if last result was zero
        case 0xCA:
          {
            if (registers.FZ == 0) { return; }
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 16;
            break;
          }

        // Ext ops: Extended operations (two-byte instruction code)
        case 0xCB:
          {
            throw new InvalidInstructionException("Ext ops (0xCB)");
          }

        // CALL Z,nn: Call routine at 16-bit location if last result was zero
        case 0xCC:
          {
            if (registers.FZ == 0) { return; }
            registers.SP -= 2;
            memory.Write(registers.SP, cpu.nextPC);
            // We jump
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 24;
            break;
          }

        // CALL nn: Call routine at 16-bit location
        case 0xCD:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, cpu.nextPC);
            // We jump
            cpu.nextPC = n;
            break;
          }

        // ADC A,n: Add 8-bit immediate and carry to A
        case 0xCE:
          {
            ushort A = registers.A;
            byte initial = registers.A;
            A += n;
            A += registers.FC;
            registers.A = (byte)A;
            // Update flags
            registers.FC = (byte)((A > 255) ? 1 : 0);
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)(((registers.A ^ n ^ initial) & 0x10) == 0 ? 0 : 1);
            registers.FN = 0;
            break;
          }

        // RST 8: Call routine at address 0008h
        case 0xCF:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x08);
            //instructionLambdas[0xCD](0x08);
            break;
          }

        // RET NC: Return if last result caused no carry
        case 0xD0:
          {
            if (registers.FC != 0) { return; }
            // We load the program counter (high byte is in higher address)
            cpu.nextPC = memory.Read(registers.SP++);
            cpu.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            cpu._currentInstruction.Ticks = 20;
            break;
          }

        // POP DE: Pop 16-bit value from stack into DE
        case 0xD1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.DE = res;
            break;
          }

        // JP NC,nn: Absolute jump to 16-bit location if last result caused no carry
        case 0xD2:
          {
            if (registers.FC != 0) { return; }
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 16;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xD3:
          {
            throw new InvalidInstructionException("XX (0xD3)");
          }

        // CALL NC,nn: Call routine at 16-bit location if last result caused no carry
        case 0xD4:
          {
            if (registers.FC != 0) { return; }
            registers.SP -= 2;
            memory.Write(registers.SP, cpu.nextPC);
            // We jump
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 24;
            break;
          }

        // PUSH DE: Push 16-bit DE onto stack
        case 0xD5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.DE);
            break;
          }

        // SUB A,n: Subtract 8-bit immediate from A
        case 0xD6:
          {
            byte subtractor = (byte)n;
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          subtractor,
                          0);
            break;
          }

        // RST 10: Call routine at address 0010h
        case 0xD7:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x10);
            //instructionLambdas[0xCD](0x10);
            break;

          }
        // RET C: Return if last result caused carry

        case 0xD8:
          {
            if (registers.FC == 0) { return; }
            // We load the program counter (high byte is in higher address)
            cpu.nextPC = memory.Read(registers.SP++);
            cpu.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            cpu._currentInstruction.Ticks = 20;
            break;
          }

        // RETI: Enable interrupts and return to calling routine
        case 0xD9:
          {
            cpu.interruptController.InterruptMasterEnable = true;
            // We load the program counter (high byte is in higher address)
            cpu.nextPC = memory.Read(registers.SP++);
            cpu.nextPC += (ushort)(memory.Read(registers.SP++) << 8);
            break;
          }

        // JP C,nn: Absolute jump to 16-bit location if last result caused carry
        case 0xDA:
          {
            if (registers.FC == 0) { return; }
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 16;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xDB:
          {
            throw new InvalidInstructionException("XX (0xDB)");
          }

        // CALL C,nn: Call routine at 16-bit location if last result caused carry
        case 0xDC:
          {
            if (registers.FC == 0) { return; }
            registers.SP -= 2;
            memory.Write(registers.SP, cpu.nextPC);
            // We jump
            cpu.nextPC = n;
            cpu._currentInstruction.Ticks = 24;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xDD:
          {
            throw new InvalidInstructionException("XX (0xDD)");
          }

        // SBC A,n: Subtract 8-bit immediate and carry from A
        case 0xDE:
          {
            byte substractor = 0;
            unchecked { substractor = (byte)n; }
            UtilFuncs.SBC(ref registers,
                          ref registers.A,
                          substractor,
                          registers.FC);
            break;
          }

        // RST 18: Call routine at address 0018h
        case 0xDF:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x18);
            //instructionLambdas[0xCD](0x18);
            break;
          }

        // LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)
        case 0xE0:
          {
            memory.Write((ushort)(0xFF00 | (byte)n), registers.A);
            break;
          }

        // POP HL: Pop 16-bit value from stack into HL
        case 0xE1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.HL = res;
            break;
          }

        // LDH (C),A: Save A at address pointed to by (FF00h + C)
        case 0xE2:
          {
            memory.Write((ushort)(0xFF00 | registers.C), registers.A);
            break;
          }

        // XX: Operation removed in this CPU
        case 0xE3:
          {
            throw new InvalidInstructionException("XX (0xE3)");
          }

        // XX: Operation removed in this CPU
        case 0xE4:
          {
            throw new InvalidInstructionException("XX (0xE4)");
          }

        // PUSH HL: Push 16-bit HL onto stack
        case 0xE5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.HL);
            break;
          }

        // AND n: Logical AND 8-bit immediate against A
        case 0xE6:
          {
            registers.A &= (byte)n;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)1;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // RST 20: Call routine at address 0020h
        case 0xE7:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x20);
            //instructionLambdas[0xCD](0x20);
            break;
          }

        // ADD SP,d: Add signed 8-bit immediate to SP
        case 0xE8:
          {
            // We determine the short offset
            sbyte sn = 0;
            unchecked { sn = (sbyte)n; }
            // We set the registers
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = (byte)
              (((registers.SP & 0x0F) + (sn & 0x0F) > 0x0F) ? 1 : 0);
            registers.FC = (byte)
              (((registers.SP & 0xFF) + (sn & 0xFF) > 0xFF) ? 1 : 0);
            // We make the sum
            registers.SP = (ushort)(registers.SP + sn);
            break;
          }

        // JP (HL): Jump to 16-bit value pointed by HL
        case 0xE9:
          {
            cpu.nextPC = registers.HL;
            break;
          }

        // LD (nn),A: Save A at given 16-bit address
        case 0xEA:
          {
            memory.Write(n, registers.A);
            break;
          }

        // XX: Operation removed in this CPU
        case 0xEB:
          {
            throw new InvalidInstructionException("XX (0xEB)");
          }

        // XX: Operation removed in this CPU
        case 0xEC:
          {
            throw new InvalidInstructionException("XX (0xEC)");
          }

        // XX: Operation removed in this CPU
        case 0xED:
          {
            throw new InvalidInstructionException("XX (0xED)");
          }

        // XOR n: Logical XOR 8-bit immediate against A
        case 0xEE:
          {
            registers.A ^= (byte)n;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // RST 28: Call routine at address 0028h
        case 0xEF:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x28);
            //instructionLambdas[0xCD](0x28);
            break;
          }

        // LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)
        case 0xF0:
          {
            registers.A = memory.Read((ushort)(0xFF00 | (byte)n));
            break;
          }

        // POP AF: Pop 16-bit value from stack into AF
        case 0xF1:
          {
            ushort res = memory.Read(registers.SP++);
            res += (ushort)(memory.Read(registers.SP++) << 8);
            registers.AF = res;
            break;
          }

        // LDH A, (C): Operation removed in this CPU? (Or Load into A memory from FF00 + C?)
        case 0xF2:
          {
            registers.A = memory.Read((ushort)(0xFF00 | registers.C));
            break;
          }

        // DI: DIsable interrupts
        case 0xF3:
          {
            cpu.interruptController.InterruptMasterEnable = false;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xF4:
          {
            throw new InvalidInstructionException("XX (0xF4)");
          }

        // PUSH AF: Push 16-bit AF onto stack
        case 0xF5:
          {
            registers.SP -= 2;
            memory.Write(registers.SP, registers.AF);
            break;
          }

        // OR n: Logical OR 8-bit immediate against A
        case 0xF6:
          {
            registers.A |= (byte)n;
            registers.FZ = (byte)(registers.A == 0 ? 1 : 0);
            registers.FH = (byte)0;
            registers.FN = (byte)0;
            registers.FC = (byte)0;
            break;
          }

        // RST 30: Call routine at address 0030h
        case 0xF7:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x30);
            //instructionLambdas[0xCD](0x30);
            break;
          }

        // LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL
        case 0xF8:
          {
            // We determine the short offset
            sbyte sn = 0;
            unchecked { sn = (sbyte)n; }
            // We set the registers
            registers.FZ = 0;
            registers.FN = 0;
            registers.FH = (byte)
              (((registers.SP & 0x0F) + (sn & 0x0F) > 0x0F) ? 1 : 0);
            registers.FC = (byte)
              (((registers.SP & 0xFF) + (sn & 0xFF) > 0xFF) ? 1 : 0);
            // We make the sum
            registers.HL = (ushort)(registers.SP + sn);
            break;
          }

        // LD SP,HL: Copy HL to SP
        case 0xF9:
          {
            registers.SP = registers.HL;
            break;
          }

        // LD A,(nn): Load A from given 16-bit address
        case 0xFA:
          {
            registers.A = memory.Read(n);
            break;
          }

        // EI: Enable interrupts
        case 0xFB:
          {
            cpu.interruptController.InterruptMasterEnable = true;
            break;
          }

        // XX: Operation removed in this CPU
        case 0xFC:
          {
            throw new InvalidInstructionException("XX (0xFC)");
          }

        // XX: Operation removed in this CPU
        case 0xFD:
          {
            throw new InvalidInstructionException("XX (0xFD)");
          }

        // CP n: Compare 8-bit immediate against A
        case 0xFE:
          {
            byte operand = (byte)n;
            registers.FN = 1;
            registers.FC = 0; // This flag might get changed
            registers.FH = (byte)(((registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);
            if (registers.A == operand) {
              registers.FZ = 1;
            }
            else {
              registers.FZ = 0;
              if (registers.A < operand) {
                registers.FC = 1;
              }
            }
            break;
          }

        // RST 38: Call routine at address 0038h
        case 0xFF:
          {
            CPUInstructions.Run(cpu, ref registers, ref memory, 0xCD, 0x38);
            //instructionLambdas[0xCD](0x38);
            break;
          }
      }
    }
  }
}
