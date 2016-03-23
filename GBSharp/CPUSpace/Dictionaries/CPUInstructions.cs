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
    /// <summary>
    /// Runs an normal opcode instruction. Notice some instructions have two-stage approach:
    /// They read in the normal execution (and store the value in a temporary Registers)
    /// and actually write in a post-execution step. This is because some instruction
    /// read and write on different clock ticks
    /// The code for the post is in CPUInstructionPostCode
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    internal static void RunInstruction(CPU cpu, byte opcode, ushort n)
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
            cpu.Registers.BC = n;
            break;
          }

        // LD (BC),A: Save A to address pointed by BC
        case 0x02:
          {
            cpu.memory.Write(cpu.Registers.BC, cpu.Registers.A);
            break;
          }

        // INC BC: Increment 16-bit BC
        case 0x03:
          {
            cpu.Registers.BC++;
            break;
          }

        // INC B: Increment B
        case 0x04:
          {
            cpu.Registers.B++;

            cpu.Registers.FZ = (byte)(cpu.Registers.B == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.B & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC B: Decrement B
        case 0x05:
          {
            cpu.Registers.B--;

            cpu.Registers.FZ = (byte)(cpu.Registers.B == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.B & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD B,n: Load 8-bit immediate into B
        case 0x06:
          {
            cpu.Registers.B = (byte)n;
            break;
          }

        // RLC A: Rotate A left with carry
        case 0x07:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.A);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = 0;
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // LD (nn),SP: Save SP to given address
        case 0x08:
          {
            cpu.memory.Write(n, cpu.Registers.SP);
            break;
          }

        // ADD HL,BC: Add 16-bit BC to HL
        case 0x09:
          {
            var initialH = cpu.Registers.H;
            int res = cpu.Registers.HL + cpu.Registers.BC;

            cpu.Registers.HL += cpu.Registers.BC;

            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)(((cpu.Registers.H ^ cpu.Registers.B ^ initialH) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LD A,(BC): Load A from address pointed to by BC
        case 0x0A:
          {
            cpu.Registers.A = cpu.memory.Read(cpu.Registers.BC);
            break;
          }

        // DEC BC: Decrement 16-bit BC
        case 0x0B:
          {
            cpu.Registers.BC--;
            break;
          }

        // INC C: Increment C
        case 0x0C:
          {
            cpu.Registers.C++;

            cpu.Registers.FZ = (byte)(cpu.Registers.C == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.C & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC C: Decrement C
        case 0x0D:
          {
            cpu.Registers.C--;

            cpu.Registers.FZ = (byte)(cpu.Registers.C == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.C & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD C,n: Load 8-bit immediate into C
        case 0x0E:
          {
            cpu.Registers.C = (byte)n;
            break;
          }

        // RRC A: Rotate A right with carry
        case 0x0F:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.A);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = 0;
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // STOP: Stop processor
        case 0x10:
          {
            cpu.Stopped = true;
            break;
          }

        // LD DE,nn: Load 16-bit immediate into DE
        case 0x11:
          {
            cpu.Registers.DE = n;
            break;
          }

        // LD (DE),A: Save A to address pointed by DE
        case 0x12:
          {
            cpu.memory.Write(cpu.Registers.DE, cpu.Registers.A);
            break;
          }

        // INC DE: Increment 16-bit DE
        case 0x13:
          {
            cpu.Registers.DE++;
            break;
          }

        // INC D: Increment D
        case 0x14:
          {
            cpu.Registers.D++;

            cpu.Registers.FZ = (byte)(cpu.Registers.D == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.D & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC D: Decrement D
        case 0x15:
          {
            cpu.Registers.D--;

            cpu.Registers.FZ = (byte)(cpu.Registers.D == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.D & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD D,n: Load 8-bit immediate into D
        case 0x16:
          {
            cpu.Registers.D = (byte)n;
            break;
          }

        // RL A: Rotate A left
        case 0x17:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.A, 1, cpu.Registers.FC);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = 0;
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // JR n: Relative jump by signed immediate
        case 0x18:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.NextPC + sn);
            cpu.NextPC = target;
            break;
          }

        // ADD HL,DE: Add 16-bit DE to HL
        case 0x19:
          {
            var initialH = cpu.Registers.H;
            int res = cpu.Registers.HL + cpu.Registers.DE;
            cpu.Registers.HL += cpu.Registers.DE;

            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)(((cpu.Registers.H ^ cpu.Registers.D ^ initialH) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LD A,(DE): Load A from address pointed to by DE
        case 0x1A:
          {
            cpu.Registers.A = cpu.memory.Read(cpu.Registers.DE);
            break;
          }

        // DEC DE: Decrement 16-bit DE
        case 0x1B:
          {
            cpu.Registers.DE--;
            break;
          }

        // INC E: Increment E
        case 0x1C:
          {
            cpu.Registers.E++;

            cpu.Registers.FZ = (byte)(cpu.Registers.E == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.E & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC E: Decrement E
        case 0x1D:
          {
            cpu.Registers.E--;

            cpu.Registers.FZ = (byte)(cpu.Registers.E == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.E & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD E,n: Load 8-bit immediate into E
        case 0x1E:
          {
            cpu.Registers.E = (byte)n;
            break;
          }

        // RR A: Rotate A right
        case 0x1F:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.A, 1, cpu.Registers.FC);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = 0;
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // JR NZ,n: Relative jump by signed immediate if last result was not zero
        case 0x20:
          {
            if (cpu.Registers.FZ != 0) { break; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.NextPC + sn);
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 12;
            break;
          }

        // LD HL,nn: Load 16-bit immediate into HL
        case 0x21:
          {
            cpu.Registers.HL = n;
            break;
          }

        // LDI (HL),A: Save A to address pointed by HL, and increment HL
        case 0x22:
          {
            cpu.memory.Write(cpu.Registers.HL++, cpu.Registers.A);
            break;
          }

        // INC HL: Increment 16-bit HL
        case 0x23:
          {
            cpu.Registers.HL++;
            break;
          }

        // INC H: Increment H
        case 0x24:
          {
            cpu.Registers.H++;

            cpu.Registers.FZ = (byte)(cpu.Registers.H == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.H & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC H: Decrement H
        case 0x25:
          {
            cpu.Registers.H--;

            cpu.Registers.FZ = (byte)(cpu.Registers.H == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.H & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD H,n: Load 8-bit immediate into H
        case 0x26:
          {
            cpu.Registers.H = (byte)n;
            break;
          }

        // DAA: Adjust A for BCD addition
        case 0x27:
          {
            int value = cpu.Registers.A;

            if (cpu.Registers.FN != 0) // ADD, ADC, INC
            {
              if (cpu.Registers.FH != 0) { value = (value - 0x06) & 0xFF; }
              if (cpu.Registers.FC != 0) { value -= 0x60; }
            }
            else // SUB, SBC, DEC, NEG
            {
              if ((cpu.Registers.FH != 0) || ((value & 0x0F) > 0x09)) { value += 0x06; }
              if ((cpu.Registers.FC != 0) || (value > 0x9F)) { value += 0x60; }
            }

            cpu.Registers.FH = 0;

            //cpu.Registers.FC = 0;
            if ((value & 0x100) == 0x100) { cpu.Registers.FC = 1; }

            value &= 0xFF;

            cpu.Registers.FZ = (byte)(value == 0 ? 1 : 0);

            cpu.Registers.A = (byte)value;
            break;
          }

        // JR Z,n: Relative jump by signed immediate if last result was zero
        case 0x28:
          {
            if (cpu.Registers.FZ == 0) { break; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.NextPC + sn);
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 12;
            break;
          }

        // ADD HL,HL: Add 16-bit HL to HL
        case 0x29:
          {
            var initialH = cpu.Registers.H;
            int res = cpu.Registers.HL + cpu.Registers.HL;

            cpu.Registers.HL += cpu.Registers.HL;

            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)(((cpu.Registers.H ^ initialH ^ initialH) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LDI A,(HL): Load A from address pointed to by HL, and increment HL
        case 0x2A:
          {
            cpu.Registers.A = cpu.memory.Read(cpu.Registers.HL++);
            break;
          }

        // DEC HL: Decrement 16-bit HL
        case 0x2B:
          {
            cpu.Registers.HL--;
            break;
          }

        // INC L: Increment L
        case 0x2C:
          {
            cpu.Registers.L++;

            cpu.Registers.FZ = (byte)(cpu.Registers.L == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.L & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC L: Decrement L
        case 0x2D:
          {
            cpu.Registers.L--;

            cpu.Registers.FZ = (byte)(cpu.Registers.L == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.L & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD L,n: Load 8-bit immediate into L
        case 0x2E:
          {
            cpu.Registers.L = (byte)n;
            break;
          }

        // CPL: Complement (logical NOT) on A
        case 0x2F:
          {
            cpu.Registers.A = (byte)~cpu.Registers.A;

            cpu.Registers.FN = 1;
            cpu.Registers.FH = 1;
            break;
          }

        // JR NC,n: Relative jump by signed immediate if last result caused no carry
        case 0x30:
          {
            if (cpu.Registers.FC != 0) { break; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.NextPC + sn);
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 12;
            break;
          }

        // LD SP,nn: Load 16-bit immediate into SP
        case 0x31:
          {
            cpu.Registers.SP = n;
            break;
          }

        // LDD (HL),A: Save A to address pointed by HL, and decrement HL
        case 0x32:
          {
            cpu.memory.Write(cpu.Registers.HL--, cpu.Registers.A);
            break;
          }

        // INC SP: Increment 16-bit HL
        case 0x33:
          {
            cpu.Registers.SP++;
            break;
          }

        // INC (HL): Increment value pointed by HL
        // NOTE: Two-stage opcode
        case 0x34:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // DEC (HL): Decrement value pointed by HL
        // NOTE: Two-stage opcode
        case 0x35:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD (HL),n: Load 8-bit immediate into address pointed by HL
        case 0x36:
          {
            cpu.memory.Write(cpu.Registers.HL, (byte)n);
            break;
          }

        // SCF: Set carry flag
        case 0x37:
          {
            cpu.Registers.FC = 1;
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // JR C,n: Relative jump by signed immediate if last result caused carry
        case 0x38:
          {
            if (cpu.Registers.FC == 0) { break; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.NextPC + sn);
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 12;
            break;
          }

        // ADD HL,SP: Add 16-bit SP to HL
        case 0x39:
          {
            var initialH = cpu.Registers.H;
            int res = cpu.Registers.HL + cpu.Registers.SP;

            cpu.Registers.HL += cpu.Registers.SP;

            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)(((cpu.Registers.H ^ (cpu.Registers.SP >> 8) ^ initialH) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FC = (byte)((res > 0xFFFF) ? 1 : 0);
            break;
          }

        // LDD A,(HL): Load A from address pointed to by HL, and decrement HL
        case 0x3A:
          {
            cpu.Registers.A = cpu.memory.Read(cpu.Registers.HL--);
            break;
          }

        // DEC SP: Decrement 16-bit SP
        case 0x3B:
          {
            cpu.Registers.SP--;
            break;
          }

        // INC A: Increment A
        case 0x3C:
          {
            cpu.Registers.A++;

            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)((cpu.Registers.A & 0x0F) == 0x00 ? 1 : 0);
            break;
          }

        // DEC A: Decrement A
        case 0x3D:
          {
            cpu.Registers.A--;

            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FN = 1;
            cpu.Registers.FH = (byte)((cpu.Registers.A & 0x0F) == 0x0F ? 1 : 0);
            break;
          }

        // LD A,n: Load 8-bit immediate into A
        case 0x3E:
          {
            cpu.Registers.A = (byte)n;
            break;
          }

        // CCF: Complement Carry Flag
        case 0x3F:
          {
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = (byte)(~cpu.Registers.FC & 1);
            break;
          }

        // LD B,B: Copy B to B
        case 0x40:
          {
#pragma warning disable
            cpu.Registers.B = cpu.Registers.B;
#pragma warning restore
            break;
          }

        // LD B,C: Copy C to B
        case 0x41:
          {
            cpu.Registers.B = cpu.Registers.C;
            break;
          }

        // LD B,D: Copy D to B
        case 0x42:
          {
            cpu.Registers.B = cpu.Registers.D;
            break;
          }

        // LD B,E: Copy E to B
        case 0x43:
          {
            cpu.Registers.B = cpu.Registers.E;
            break;
          }

        // LD B,H: Copy H to B
        case 0x44:
          {
            cpu.Registers.B = cpu.Registers.H;
            break;
          }

        // LD B,L: Copy L to B
        case 0x45:
          {
            cpu.Registers.B = cpu.Registers.L;
            break;
          }

        // LD B,(HL): Copy value pointed by HL to B
        case 0x46:
          {
            cpu.Registers.B = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD B,A: Copy A to B
        case 0x47:
          {
            cpu.Registers.B = cpu.Registers.A;
            break;
          }

        // LD C,B: Copy B to C
        case 0x48:
          {
            cpu.Registers.C = cpu.Registers.B;
            break;
          }

        // LD C,C: Copy C to C
        case 0x49:
          {
#pragma warning disable
            cpu.Registers.C = cpu.Registers.C;
#pragma warning restore
            break;
          }

        // LD C,D: Copy D to C
        case 0x4A:
          {
            cpu.Registers.C = cpu.Registers.D;
            break;
          }

        // LD C,E: Copy E to C
        case 0x4B:
          {
            cpu.Registers.C = cpu.Registers.E;
            break;
          }

        // LD C,H: Copy H to C
        case 0x4C:
          {
            cpu.Registers.C = cpu.Registers.H;
            break;
          }

        // LD C,L: Copy L to C
        case 0x4D:
          {
            cpu.Registers.C = cpu.Registers.L;
            break;
          }

        // LD C,(HL): Copy value pointed by HL to C
        case 0x4E:
          {
            cpu.Registers.C = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD C,A: Copy A to C
        case 0x4F:
          {
            cpu.Registers.C = cpu.Registers.A;
            break;
          }

        // LD D,B: Copy B to D
        case 0x50:
          {
            cpu.Registers.D = cpu.Registers.B;
            break;
          }

        // LD D,C: Copy C to D
        case 0x51:
          {
            cpu.Registers.D = cpu.Registers.C;
            break;
          }

        // LD D,D: Copy D to D
        case 0x52:
          {
#pragma warning disable
            cpu.Registers.D = cpu.Registers.D;
#pragma warning restore
            break;
          }

        // LD D,E: Copy E to D
        case 0x53:
          {
            cpu.Registers.D = cpu.Registers.E;
            break;
          }

        // LD D,H: Copy H to D
        case 0x54:
          {
            cpu.Registers.D = cpu.Registers.H;
            break;
          }

        // LD D,L: Copy L to D
        case 0x55:
          {
            cpu.Registers.D = cpu.Registers.L;
            break;
          }

        // LD D,(HL): Copy value pointed by HL to D
        case 0x56:
          {
            cpu.Registers.D = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD D,A: Copy A to D
        case 0x57:
          {
            cpu.Registers.D = cpu.Registers.A;
            break;
          }

        // LD E,B: Copy B to E
        case 0x58:
          {
            cpu.Registers.E = cpu.Registers.B;
            break;
          }

        // LD E,C: Copy C to E
        case 0x59:
          {
            cpu.Registers.E = cpu.Registers.C;
            break;
          }

        // LD E,D: Copy D to E
        case 0x5A:
          {
            cpu.Registers.E = cpu.Registers.D;
            break;
          }

        // LD E,E: Copy E to E
        case 0x5B:
          {
#pragma warning disable
            cpu.Registers.E = cpu.Registers.E;
#pragma warning restore
            break;
          }

        // LD E,H: Copy H to E
        case 0x5C:
          {
            cpu.Registers.E = cpu.Registers.H;
            break;
          }

        // LD E,L: Copy L to E
        case 0x5D:
          {
            cpu.Registers.E = cpu.Registers.L;
            break;
          }

        // LD E,(HL): Copy value pointed by HL to E
        case 0x5E:
          {
            cpu.Registers.E = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD E,A: Copy A to E
        case 0x5F:
          {
            cpu.Registers.E = cpu.Registers.A;
            break;
          }

        // LD H,B: Copy B to H
        case 0x60:
          {
            cpu.Registers.H = cpu.Registers.B;
            break;
          }

        // LD H,C: Copy C to H
        case 0x61:
          {
            cpu.Registers.H = cpu.Registers.C;
            break;
          }

        // LD H,D: Copy D to H
        case 0x62:
          {
            cpu.Registers.H = cpu.Registers.D;
            break;
          }

        // LD H,E: Copy E to H
        case 0x63:
          {
            cpu.Registers.H = cpu.Registers.E;
            break;
          }

        // LD H,H: Copy H to H
        case 0x64:
          {
#pragma warning disable
            cpu.Registers.H = cpu.Registers.H;
#pragma warning restore
            break;
          }

        // LD H,L: Copy L to H
        case 0x65:
          {
            cpu.Registers.H = cpu.Registers.L;
            break;
          }

        // LD H,(HL): Copy value pointed by HL to H
        case 0x66:
          {
            cpu.Registers.H = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD H,A: Copy A to H
        case 0x67:
          {
            cpu.Registers.H = cpu.Registers.A;
            break;
          }

        // LD L,B: Copy B to L
        case 0x68:
          {
            cpu.Registers.L = cpu.Registers.B;
            break;
          }

        // LD L,C: Copy C to L
        case 0x69:
          {
            cpu.Registers.L = cpu.Registers.C;
            break;
          }

        // LD L,D: Copy D to L
        case 0x6A:
          {
            cpu.Registers.L = cpu.Registers.D;
            break;
          }

        // LD L,E: Copy E to L
        case 0x6B:
          {
            cpu.Registers.L = cpu.Registers.E;
            break;
          }

        // LD L,H: Copy H to L
        case 0x6C:
          {
            cpu.Registers.L = cpu.Registers.H;
            break;
          }

        // LD L,L: Copy L to L
        case 0x6D:
          {
#pragma warning disable
            cpu.Registers.L = cpu.Registers.L;
#pragma warning restore
            break;
          }

        // LD L,(HL): Copy value pointed by HL to L
        case 0x6E:
          {
            cpu.Registers.L = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD L,A: Copy A to L
        case 0x6F:
          {
            cpu.Registers.L = cpu.Registers.A;
            break;
          }

        // LD (HL),B: Copy B to address pointed by HL
        case 0x70:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.B);
            break;
          }

        // LD (HL),C: Copy C to address pointed by HL
        case 0x71:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.C);
            break;
          }

        // LD (HL),D: Copy D to address pointed by HL
        case 0x72:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.D);
            break;
          }

        // LD (HL),E: Copy E to address pointed by HL
        case 0x73:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.E);
            break;
          }

        // LD (HL),H: Copy H to address pointed by HL
        case 0x74:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.H);
            break;
          }

        // LD (HL),L: Copy L to address pointed by HL
        case 0x75:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.L);
            break;
          }

        // HALT: Halt processor
        case 0x76:
          {
            if (cpu.interruptController.InterruptMasterEnable)
            {
              cpu.Halted = true;
            }
            else
            {
              cpu.Halted = true;
              cpu.HaltLoad = true;
            }
            break;
          }

        // LD (HL),A: Copy A to address pointed by HL
        case 0x77:
          {
            cpu.memory.Write(cpu.Registers.HL, cpu.Registers.A);
            break;
          }

        // LD A,B: Copy B to A
        case 0x78:
          {
            cpu.Registers.A = cpu.Registers.B;
            break;
          }

        // LD A,C: Copy C to A
        case 0x79:
          {
            cpu.Registers.A = cpu.Registers.C;
            break;
          }

        // LD A,D: Copy D to A
        case 0x7A:
          {
            cpu.Registers.A = cpu.Registers.D;
            break;
          }

        // LD A,E: Copy E to A
        case 0x7B:
          {
            cpu.Registers.A = cpu.Registers.E;
            break;
          }

        // LD A,H: Copy H to A
        case 0x7C:
          {
            cpu.Registers.A = cpu.Registers.H;
            break;
          }

        // LD A,L: Copy L to A
        case 0x7D:
          {
            cpu.Registers.A = cpu.Registers.L;
            break;
          }

        // LD A,(HL): Copy value pointed by HL to A
        case 0x7E:
          {
            cpu.Registers.A = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // LD A,A: Copy A to A
        case 0x7F:
          {
#pragma warning disable
            cpu.Registers.A = cpu.Registers.A;
#pragma warning restore
            break;
          }

        // ADD A,B: Add B to A
        case 0x80:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.B;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,C: Add C to A
        case 0x81:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.C;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,D: Add D to A
        case 0x82:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.D;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,E: Add E to A
        case 0x83:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.E;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,H: Add H to A
        case 0x84:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.H;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,L: Add L to A
        case 0x85:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.L;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,(HL): Add value pointed by HL to A
        case 0x86:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.memory.Read(cpu.Registers.HL);
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADD A,A: Add A to A
        case 0x87:
          {
            byte initial = cpu.Registers.A;
            byte toSum = cpu.Registers.A;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,B: Add B and carry flag to A
        case 0x88:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.B;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ cpu.Registers.B ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,C: Add C and carry flag to A
        case 0x89:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.C;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ cpu.Registers.C ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,D: Add D and carry flag to A
        case 0x8A:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.D;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ cpu.Registers.D ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,E: Add E and carry flag to A
        case 0x8B:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.E;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ cpu.Registers.E ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,H: Add H and carry flag to A
        case 0x8C:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.H;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ cpu.Registers.H ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,L: Add and carry flag L to A
        case 0x8D:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.L;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ cpu.Registers.L ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,(HL): Add value pointed by HL and carry flag to A
        case 0x8E:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            byte m = cpu.memory.Read(cpu.Registers.HL);
            A += m;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ m ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // ADC A,A: Add A and carry flag to A
        case 0x8F:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += cpu.Registers.A;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)(A & 0xFF);

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ initial ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // SUB A,B: Subtract B from A
        case 0x90:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.B,
                          0);
            break;
          }

        // SUB A,C: Subtract C from A
        case 0x91:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.C,
                          0);
            break;
          }

        // SUB A,D: Subtract D from A
        case 0x92:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.D,
                          0);
            break;
          }

        // SUB A,E: Subtract E from A
        case 0x93:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.E,
                          0);
            break;
          }

        // SUB A,H: Subtract H from A
        case 0x94:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.H,
                          0);
            break;
          }

        // SUB A,L: Subtract L from A
        case 0x95:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.L,
                          0);
            break;
          }

        // SUB A,(HL): Subtract value pointed by HL from A
        case 0x96:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.memory.Read(cpu.Registers.HL),
                          0);
            break;
          }

        // SUB A,A: Subtract A from A
        case 0x97:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.A,
                          0);
            break;
          }

        // SBC A,B: Subtract B and carry flag from A
        case 0x98:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.B,
                          cpu.Registers.FC);
            break;
          }

        // SBC A,C: Subtract C and carry flag from A
        case 0x99:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.C,
                          cpu.Registers.FC);
            break;
          }

        // SBC A,D: Subtract D and carry flag from A
        case 0x9A:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.D,
                          cpu.Registers.FC);
            break;
          }

        // SBC A,E: Subtract E and carry flag from A
        case 0x9B:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.E,
                          cpu.Registers.FC);
            break;
          }

        // SBC A,H: Subtract H and carry flag from A
        case 0x9C:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.H,
                          cpu.Registers.FC);
            break;
          }

        // SBC A,L: Subtract and carry flag L from A
        case 0x9D:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.L,
                          cpu.Registers.FC);
            break;
          }

        // SBC A,(HL): Subtract value pointed by HL and carry flag from A
        case 0x9E:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.memory.Read(cpu.Registers.HL),
                          cpu.Registers.FC);
            break;
          }

        // SBC A,A: Subtract A and carry flag from A
        case 0x9F:
          {
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          cpu.Registers.A,
                          cpu.Registers.FC);
            break;
          }

        // AND B: Logical AND B against A
        case 0xA0:
          {
            cpu.Registers.A &= cpu.Registers.B;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND C: Logical AND C against A
        case 0xA1:
          {
            cpu.Registers.A &= cpu.Registers.C;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND D: Logical AND D against A
        case 0xA2:
          {
            cpu.Registers.A &= cpu.Registers.D;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND E: Logical AND E against A
        case 0xA3:
          {
            cpu.Registers.A &= cpu.Registers.E;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND H: Logical AND H against A
        case 0xA4:
          {
            cpu.Registers.A &= cpu.Registers.H;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND L: Logical AND L against A
        case 0xA5:
          {
            cpu.Registers.A &= cpu.Registers.L;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND (HL): Logical AND value pointed by HL against A
        case 0xA6:
          {
            cpu.Registers.A &= cpu.memory.Read(cpu.Registers.HL);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // AND A: Logical AND A against A
        case 0xA7:
          {
            cpu.Registers.A &= cpu.Registers.A;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR B: Logical XOR B against A
        case 0xA8:
          {
            cpu.Registers.A ^= cpu.Registers.B;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR C: Logical XOR C against A
        case 0xA9:
          {
            cpu.Registers.A ^= cpu.Registers.C;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR D: Logical XOR D against A
        case 0xAA:
          {
            cpu.Registers.A ^= cpu.Registers.D;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR E: Logical XOR E against A
        case 0xAB:
          {
            cpu.Registers.A ^= cpu.Registers.E;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR H: Logical XOR H against A
        case 0xAC:
          {
            cpu.Registers.A ^= cpu.Registers.H;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR L: Logical XOR L against A
        case 0xAD:
          {
            cpu.Registers.A ^= cpu.Registers.L;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR (HL): Logical XOR value pointed by HL against A
        case 0xAE:
          {
            cpu.Registers.A ^= cpu.memory.Read(cpu.Registers.HL);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // XOR A: Logical XOR A against A
        case 0xAF:
          {
            cpu.Registers.A ^= cpu.Registers.A;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR B: Logical OR B against A
        case 0xB0:
          {
            cpu.Registers.A |= cpu.Registers.B;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR C: Logical OR C against A
        case 0xB1:
          {
            cpu.Registers.A |= cpu.Registers.C;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR D: Logical OR D against A
        case 0xB2:
          {
            cpu.Registers.A |= cpu.Registers.D;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR E: Logical OR E against A
        case 0xB3:
          {
            cpu.Registers.A |= cpu.Registers.E;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR H: Logical OR H against A
        case 0xB4:
          {
            cpu.Registers.A |= cpu.Registers.H;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR L: Logical OR L against A
        case 0xB5:
          {
            cpu.Registers.A |= cpu.Registers.L;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR (HL): Logical OR value pointed by HL against A
        case 0xB6:
          {
            cpu.Registers.A |= cpu.memory.Read(cpu.Registers.HL);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // OR A: Logical OR A against A
        case 0xB7:
          {
            cpu.Registers.A |= cpu.Registers.A;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // CP B: Compare B against A
        case 0xB8:
          {
            byte operand = cpu.Registers.B;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP C: Compare C against A
        case 0xB9:
          {
            byte operand = cpu.Registers.C;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP D: Compare D against A
        case 0xBA:
          {
            byte operand = cpu.Registers.D;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP E: Compare E against A
        case 0xBB:
          {
            byte operand = cpu.Registers.E;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP H: Compare H against A
        case 0xBC:
          {
            byte operand = cpu.Registers.H;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP L: Compare L against A
        case 0xBD:
          {
            byte operand = cpu.Registers.L;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP (HL): Compare value pointed by HL against A
        case 0xBE:
          {
            byte operand = cpu.memory.Read(cpu.Registers.HL);
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // CP A: Compare A against A
        case 0xBF:
          {
            byte operand = cpu.Registers.A;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // RET NZ: Return if last result was not zero
        case 0xC0:
          {
            if (cpu.Registers.FZ != 0) { break; }
            // We load the program counter (high byte is in higher address)
            cpu.NextPC = cpu.memory.Read(cpu.Registers.SP++);
            cpu.NextPC += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.InternalCurrentInstruction.Ticks = 20;
            break;
          }

        // POP BC: Pop 16-bit value from stack into BC
        case 0xC1:
          {
            ushort res = cpu.memory.Read(cpu.Registers.SP++);
            res += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.Registers.BC = res;
            break;
          }

        // JP NZ,nn: Absolute jump to 16-bit location if last result was not zero
        case 0xC2:
          {
            if (cpu.Registers.FZ != 0) { break; }

            ushort target = n;
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 16;
            break;
          }

        // JP nn: Absolute jump to 16-bit location
        case 0xC3:
          {
            ushort target = n;
            cpu.NextPC = target;
            break;
          }

        // CALL NZ,nn: Call routine at 16-bit location if last result was not zero
        case 0xC4:
          {
            if (cpu.Registers.FZ != 0) { break; }

            ushort target = n;
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.NextPC);

            // We jump
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 24;
            break;
          }

        // PUSH BC: Push 16-bit BC onto stack
        case 0xC5:
          {
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.Registers.BC);
            break;
          }

        // ADD A,n: Add 8-bit immediate to A
        case 0xC6:
          {
            byte initial = cpu.Registers.A;
            byte toSum = (byte)n;
            int sum = initial + toSum;
            cpu.Registers.A += toSum;
            // Update flags
            cpu.Registers.FC = (byte)((sum > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ toSum ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // RST 0: Call routine at address 0000h
        case 0xC7:
          {
            RunInstruction(cpu, 0xCD, 0);
            break;
          }

        // RET Z: Return if last result was zero
        case 0xC8:
          {
            if (cpu.Registers.FZ == 0) { break; }
            // We load the program counter (high byte is in higher address)
            cpu.NextPC = cpu.memory.Read(cpu.Registers.SP++);
            cpu.NextPC += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.InternalCurrentInstruction.Ticks = 20;
            break;
          }

        // RET: Return to calling routine
        case 0xC9:
          {
            // We load the program counter (high byte is in higher address)
            cpu.NextPC = cpu.memory.Read(cpu.Registers.SP++);
            cpu.NextPC += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            break;
          }

        // JP Z,nn: Absolute jump to 16-bit location if last result was zero
        case 0xCA:
          {
            if (cpu.Registers.FZ == 0) { break; }

            ushort target = n;
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 16;
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
            if (cpu.Registers.FZ == 0) { break; }

            ushort target = n;
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.NextPC);

            // We jump
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 24;
            break;
          }

        // CALL nn: Call routine at 16-bit location
        case 0xCD:
          {
            ushort target = n;
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.NextPC);

            // We jump
            cpu.NextPC = target;
            break;
          }

        // ADC A,n: Add 8-bit immediate and carry to A
        case 0xCE:
          {
            ushort A = cpu.Registers.A;
            byte initial = cpu.Registers.A;
            A += n;
            A += cpu.Registers.FC;
            cpu.Registers.A = (byte)A;

            // Update flags
            cpu.Registers.FC = (byte)((A > 255) ? 1 : 0);
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)(((cpu.Registers.A ^ n ^ initial) & 0x10) == 0 ? 0 : 1);
            cpu.Registers.FN = 0;
            break;
          }

        // RST 8: Call routine at address 0008h
        case 0xCF:
          {
            RunInstruction(cpu, 0xCD, 0x08);
            break;
          }

        // RET NC: Return if last result caused no carry
        case 0xD0:
          {
            if (cpu.Registers.FC != 0) { break; }
            // We load the program counter (high byte is in higher address)
            cpu.NextPC = cpu.memory.Read(cpu.Registers.SP++);
            cpu.NextPC += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.InternalCurrentInstruction.Ticks = 20;
            break;
          }

        // POP DE: Pop 16-bit value from stack into DE
        case 0xD1:
          {
            ushort res = cpu.memory.Read(cpu.Registers.SP++);
            res += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.Registers.DE = res;
            break;
          }

        // JP NC,nn: Absolute jump to 16-bit location if last result caused no carry
        case 0xD2:
          {
            if (cpu.Registers.FC != 0) { break; }

            ushort target = n;
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 16;
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xD3:
          {
            throw new InvalidInstructionException("XX (0xD3)");
          }

        // CALL NC,nn: Call routine at 16-bit location if last result caused no carry
        case 0xD4:
          {
            if (cpu.Registers.FC != 0) { break; }

            ushort target = n;
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.NextPC);

            // We jump
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 24;
            break;
          }

        // PUSH DE: Push 16-bit DE onto stack
        case 0xD5:
          {
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.Registers.DE);
            break;
          }

        // SUB A,n: Subtract 8-bit immediate from A
        case 0xD6:
          {
            byte subtractor = (byte)n;
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          subtractor,
                          0);
            break;
          }

        // RST 10: Call routine at address 0010h
        case 0xD7:
          {
            RunInstruction(cpu, 0xCD, 0x10);
            break;
          }

        // RET C: Return if last result caused carry
        case 0xD8:
          {
            if (cpu.Registers.FC == 0) { break; }
            // We load the program counter (high byte is in higher address)
            cpu.NextPC = cpu.memory.Read(cpu.Registers.SP++);
            cpu.NextPC += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.InternalCurrentInstruction.Ticks = 20;
            break;
          }

        // RETI: Enable interrupts and return to calling routine
        case 0xD9:
          {
            cpu.interruptController.InterruptMasterEnable = true;

            // We load the program counter (high byte is in higher address)
            cpu.NextPC = cpu.memory.Read(cpu.Registers.SP++);
            cpu.NextPC += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            break;
          }

        // JP C,nn: Absolute jump to 16-bit location if last result caused carry
        case 0xDA:
          {
            if (cpu.Registers.FC == 0) { break; }

            ushort target = n;
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 16;
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xDB:
          {
            throw new InvalidInstructionException("XX (0xDB)");
          }

        // CALL C,nn: Call routine at 16-bit location if last result caused carry
        case 0xDC:
          {
            if (cpu.Registers.FC == 0) { break; }

            ushort target = n;
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.NextPC);

            // We jump
            cpu.NextPC = target;
            cpu.InternalCurrentInstruction.Ticks = 24;
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xDD:
          {
            throw new InvalidInstructionException("XX (0xDD)");
          }

        // SBC A,n: Subtract 8-bit immediate and carry from A
        case 0xDE:
          {
            byte substractor = 0;
            unchecked { substractor = (byte)n; }
            UtilFuncs.SBC(cpu.Registers,
                          ref cpu.Registers.A,
                          substractor,
                          cpu.Registers.FC);
            break;
          }

        // RST 18: Call routine at address 0018h
        case 0xDF:
          {
            RunInstruction(cpu, 0xCD, 0x18);
            break;
          }

        // LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)
        case 0xE0:
          {
            ushort address = (ushort)(0xFF00 | (byte)n);
            cpu.memory.Write(address, cpu.Registers.A);
            break;
          }

        // POP HL: Pop 16-bit value from stack into HL
        case 0xE1:
          {
            ushort res = cpu.memory.Read(cpu.Registers.SP++);
            res += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.Registers.HL = res;
            break;
          }

        // LDH (C),A: Save A at address pointed to by (FF00h + C)
        case 0xE2:
          {
            ushort address = (ushort)(0xFF00 | cpu.Registers.C);
            cpu.memory.Write(address, cpu.Registers.A);
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xE3:
          {
            throw new InvalidInstructionException("XX (0xE3)");
          }

        // XX: Operation removed in cpu CPU
        case 0xE4:
          {
            throw new InvalidInstructionException("XX (0xE4)");
          }

        // PUSH HL: Push 16-bit HL onto stack
        case 0xE5:
          {
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.Registers.HL);
            break;
          }

        // AND n: Logical AND 8-bit immediate against A
        case 0xE6:
          {
            cpu.Registers.A &= (byte)n;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)1;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // RST 20: Call routine at address 0020h
        case 0xE7:
          {
            RunInstruction(cpu, 0xCD, 0x20);
            break;
          }

        // ADD SP,d: Add signed 8-bit immediate to SP
        case 0xE8:
          {
            // We determine the short offset
            sbyte sn = 0;
            unchecked { sn = (sbyte)n; }

            // We set the cpu.Registers
            cpu.Registers.FZ = 0;
            cpu.Registers.FN = 0;

            cpu.Registers.FH = (byte)
              (((cpu.Registers.SP & 0x0F) + (sn & 0x0F) > 0x0F) ? 1 : 0);
            cpu.Registers.FC = (byte)
              (((cpu.Registers.SP & 0xFF) + (sn & 0xFF) > 0xFF) ? 1 : 0);

            // We make the sum
            cpu.Registers.SP = (ushort)(cpu.Registers.SP + sn);
            break;
          }

        // JP (HL): Jump to 16-bit value pointed by HL
        case 0xE9:
          {
            ushort target = cpu.Registers.HL;
            cpu.NextPC = target;
            break;
          }

        // LD (nn),A: Save A at given 16-bit address
        case 0xEA:
          {
            cpu.memory.Write(n, cpu.Registers.A);
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xEB:
          {
            throw new InvalidInstructionException("XX (0xEB)");
          }

        // XX: Operation removed in cpu CPU
        case 0xEC:
          {
            throw new InvalidInstructionException("XX (0xEC)");
          }

        // XX: Operation removed in cpu CPU
        case 0xED:
          {
            throw new InvalidInstructionException("XX (0xED)");
          }

        // XOR n: Logical XOR 8-bit immediate against A
        case 0xEE:
          {
            cpu.Registers.A ^= (byte)n;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // RST 28: Call routine at address 0028h
        case 0xEF:
          {
            RunInstruction(cpu, 0xCD, 0x28);
            break;
          }

        // LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)
        case 0xF0:
          {
            ushort address = (ushort)(0xFF00 | (byte)n);
            cpu.Registers.A = cpu.memory.Read(address);
            break;
          }

        // POP AF: Pop 16-bit value from stack into AF
        case 0xF1:
          {
            ushort res = cpu.memory.Read(cpu.Registers.SP++);
            res += (ushort)(cpu.memory.Read(cpu.Registers.SP++) << 8);
            cpu.Registers.AF = res;
            break;
          }

        // LDH A, (C): Operation removed in cpu CPU? (Or Load into A cpu.memory from FF00 + C?)
        case 0xF2:
          {
            ushort address = (ushort)(0xFF00 | cpu.Registers.C);
            cpu.Registers.A = cpu.memory.Read(address);
            break;
          }

        // DI: DIsable interrupts
        case 0xF3:
          {
            cpu.interruptController.InterruptMasterEnable = false;
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xF4:
          {
            throw new InvalidInstructionException("XX (0xF4)");
          }

        // PUSH AF: Push 16-bit AF onto stack
        case 0xF5:
          {
            cpu.Registers.SP -= 2;
            cpu.memory.Write(cpu.Registers.SP, cpu.Registers.AF);
            break;
          }

        // OR n: Logical OR 8-bit immediate against A
        case 0xF6:
          {
            cpu.Registers.A |= (byte)n;
            cpu.Registers.FZ = (byte)(cpu.Registers.A == 0 ? 1 : 0);
            cpu.Registers.FH = (byte)0;
            cpu.Registers.FN = (byte)0;
            cpu.Registers.FC = (byte)0;
            break;
          }

        // RST 30: Call routine at address 0030h
        case 0xF7:
          {
            RunInstruction(cpu, 0xCD, 0x30);
            break;
          }

        // LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL
        case 0xF8:
          {
            // We determine the short offset
            sbyte sn = 0;
            unchecked { sn = (sbyte)n; }

            // We set the cpu.Registers
            cpu.Registers.FZ = 0;
            cpu.Registers.FN = 0;
            cpu.Registers.FH = (byte)
              (((cpu.Registers.SP & 0x0F) + (sn & 0x0F) > 0x0F) ? 1 : 0);
            cpu.Registers.FC = (byte)
              (((cpu.Registers.SP & 0xFF) + (sn & 0xFF) > 0xFF) ? 1 : 0);

            // We make the sum
            cpu.Registers.HL = (ushort)(cpu.Registers.SP + sn);
            break;
          }

        // LD SP,HL: Copy HL to SP
        case 0xF9:
          {
            cpu.Registers.SP = cpu.Registers.HL;
            break;
          }

        // LD A,(nn): Load A from given 16-bit address
        case 0xFA:
          {
            cpu.Registers.A = cpu.memory.Read(n);
            break;
          }

        // EI: Enable interrupts
        case 0xFB:
          {
            cpu.interruptController.InterruptMasterEnable = true;
            break;
          }

        // XX: Operation removed in cpu CPU
        case 0xFC:
          {
            throw new InvalidInstructionException("XX (0xFC)");
          }

        // XX: Operation removed in cpu CPU
        case 0xFD:
          {
            throw new InvalidInstructionException("XX (0xFD)");
          }

        // CP n: Compare 8-bit immediate against A
        case 0xFE:
          {
            byte operand = (byte)n;
            cpu.Registers.FN = 1;
            cpu.Registers.FC = 0; // cpu flag might get changed
            cpu.Registers.FH = (byte)
              (((cpu.Registers.A & 0x0F) < (operand & 0x0F)) ? 1 : 0);

            if (cpu.Registers.A == operand)
            {
              cpu.Registers.FZ = 1;
            }
            else
            {
              cpu.Registers.FZ = 0;
              if (cpu.Registers.A < operand)
              {
                cpu.Registers.FC = 1;
              }
            }
            break;
          }

        // RST 38: Call routine at address 0038h
        case 0xFF:
          {
            RunInstruction(cpu, 0xCD, 0x38);
            break;
          }
      }
    }
  }
}
