using GBSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUCBInstructions
  {
    /// <summary>
    /// Runs an CB opcode instruction
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    internal static void RunCBInstruction(CPU cpu, byte opcode, ushort n)
    {
      switch (opcode)
      {
        // RLC B: Rotate B left with carry
        case 0x00:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.B);
            cpu.registers.B = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RLC C: Rotate C left with carry
        case 0x01:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.C);
            cpu.registers.C = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RLC D: Rotate D left with carry
        case 0x02:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.D);
            cpu.registers.D = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RLC E: Rotate E left with carry
        case 0x03:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.E);
            cpu.registers.E = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RLC H: Rotate H left with carry
        case 0x04:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.H);
            cpu.registers.H = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RLC L: Rotate L left with carry
        case 0x05:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.L);
            cpu.registers.L = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RLC (HL): Rotate value pointed by HL left with carry
        case 0x06:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RLC A: Rotate A left with carry
        case 0x07:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.registers.A);
            cpu.registers.A = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC B: Rotate B right with carry
        case 0x08:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.B);
            cpu.registers.B = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC C: Rotate C right with carry
        case 0x09:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.C);
            cpu.registers.C = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC D: Rotate D right with carry
        case 0x0A:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.D);
            cpu.registers.D = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC E: Rotate E right with carry
        case 0x0B:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.E);
            cpu.registers.E = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC H: Rotate H right with carry
        case 0x0C:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.H);
            cpu.registers.H = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC L: Rotate L right with carry
        case 0x0D:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.L);
            cpu.registers.L = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RRC (HL): Rotate value pointed by HL right with carry
        case 0x0E:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RRC A: Rotate A right with carry
        case 0x0F:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.registers.A);
            cpu.registers.A = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL B: Rotate B left
        case 0x10:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.B, 1, cpu.registers.FC);
            cpu.registers.B = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL C: Rotate C left
        case 0x11:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.C, 1, cpu.registers.FC);
            cpu.registers.C = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL D: Rotate D left
        case 0x12:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.D, 1, cpu.registers.FC);
            cpu.registers.D = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL E: Rotate E left
        case 0x13:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.E, 1, cpu.registers.FC);
            cpu.registers.E = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL H: Rotate H left
        case 0x14:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.H, 1, cpu.registers.FC);
            cpu.registers.H = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL L: Rotate L left
        case 0x15:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.L, 1, cpu.registers.FC);
            cpu.registers.L = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RL (HL): Rotate value pointed by HL left
        case 0x16:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RL A: Rotate A left
        case 0x17:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.registers.A, 1, cpu.registers.FC);
            cpu.registers.A = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR B: Rotate B right
        case 0x18:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.B, 1, cpu.registers.FC);
            cpu.registers.B = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR C: Rotate C right
        case 0x19:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.C, 1, cpu.registers.FC);
            cpu.registers.C = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR D: Rotate D right
        case 0x1A:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.D, 1, cpu.registers.FC);
            cpu.registers.D = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR E: Rotate E right
        case 0x1B:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.E, 1, cpu.registers.FC);
            cpu.registers.E = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR H: Rotate H right
        case 0x1C:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.H, 1, cpu.registers.FC);
            cpu.registers.H = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR L: Rotate L right
        case 0x1D:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.L, 1, cpu.registers.FC);
            cpu.registers.L = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // RR (HL): Rotate value pointed by HL right
        case 0x1E:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RR A: Rotate A right
        case 0x1F:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.registers.A, 1, cpu.registers.FC);
            cpu.registers.A = rotateCarry.Item1;

            cpu.registers.FC = rotateCarry.Item2;
            cpu.registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA B: Shift B left preserving sign
        case 0x20:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.B);
            cpu.registers.B = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA C: Shift C left preserving sign
        case 0x21:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.C);
            cpu.registers.C = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA D: Shift D left preserving sign
        case 0x22:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.D);
            cpu.registers.D = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA E: Shift E left preserving sign
        case 0x23:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.E);
            cpu.registers.E = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA H: Shift H left preserving sign
        case 0x24:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.H);
            cpu.registers.H = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA L: Shift L left preserving sign
        case 0x25:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.L);
            cpu.registers.L = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SLA (HL): Shift value pointed by HL left preserving sign
        case 0x26:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SLA A: Shift A left preserving sign
        case 0x27:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.registers.A);
            cpu.registers.A = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
          }
          break;
        // SRA B: Shift B right preserving sign
        case 0x28:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.B);
            cpu.registers.B = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRA C: Shift C right preserving sign
        case 0x29:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.C);
            cpu.registers.C = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRA D: Shift D right preserving sign
        case 0x2A:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.D);
            cpu.registers.D = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRA E: Shift E right preserving sign
        case 0x2B:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.E);
            cpu.registers.E = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRA H: Shift H right preserving sign
        case 0x2C:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.H);
            cpu.registers.H = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRA L: Shift L right preserving sign
        case 0x2D:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.L);
            cpu.registers.L = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRA (HL): Shift value pointed by HL right preserving sign
        case 0x2E:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SRA A: Shift A right preserving sign
        case 0x2F:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.registers.A);
            cpu.registers.A = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SWAP B: Swap nybbles in B
        case 0x30:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.B);
            cpu.registers.B = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SWAP C: Swap nybbles in C
        case 0x31:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.C);
            cpu.registers.C = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SWAP D: Swap nybbles in D
        case 0x32:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.D);
            cpu.registers.D = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SWAP E: Swap nybbles in E
        case 0x33:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.E);
            cpu.registers.E = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SWAP H: Swap nybbles in H
        case 0x34:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.H);
            cpu.registers.H = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SWAP L: Swap nybbles in L
        case 0x35:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.L);
            cpu.registers.L = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SWAP (HL): Swap nybbles in value pointed by HL
        case 0x36:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SWAP A: Swap nybbles in A
        case 0x37:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.registers.A);
            cpu.registers.A = result;

            cpu.registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            cpu.registers.FC = 0;
            break;
          }

        // SRL B: Shift B right
        case 0x38:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.B);
            cpu.registers.B = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRL C: Shift C right
        case 0x39:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.C);
            cpu.registers.C = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRL D: Shift D right
        case 0x3A:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.D);
            cpu.registers.D = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRL E: Shift E right
        case 0x3B:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.E);
            cpu.registers.E = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRL H: Shift H right
        case 0x3C:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.H);
            cpu.registers.H = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRL L: Shift L right
        case 0x3D:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.L);
            cpu.registers.L = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // SRL (HL): Shift value pointed by HL right
        case 0x3E:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SRL A: Shift A right
        case 0x3F:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.registers.A);
            cpu.registers.A = shiftCarry.Item1;

            cpu.registers.FC = shiftCarry.Item2;
            cpu.registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 0;
            break;
          }

        // BIT 0,B: Test bit 0 of B
        case 0x40:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,C: Test bit 0 of C
        case 0x41:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,D: Test bit 0 of D
        case 0x42:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,E: Test bit 0 of E
        case 0x43:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,H: Test bit 0 of H
        case 0x44:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,L: Test bit 0 of L
        case 0x45:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,(HL): Test bit 0 of value pointed by HL
        case 0x46:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 0,A: Test bit 0 of A
        case 0x47:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 0) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,B: Test bit 1 of B
        case 0x48:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,C: Test bit 1 of C
        case 0x49:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,D: Test bit 1 of D
        case 0x4A:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,E: Test bit 1 of E
        case 0x4B:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,H: Test bit 1 of H
        case 0x4C:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,L: Test bit 1 of L
        case 0x4D:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,(HL): Test bit 1 of value pointed by HL
        case 0x4E:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 1,A: Test bit 1 of A
        case 0x4F:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 1) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,B: Test bit 2 of B
        case 0x50:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,C: Test bit 2 of C
        case 0x51:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,D: Test bit 2 of D
        case 0x52:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,E: Test bit 2 of E
        case 0x53:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,H: Test bit 2 of H
        case 0x54:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,L: Test bit 2 of L
        case 0x55:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,(HL): Test bit 2 of value pointed by HL
        case 0x56:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 2,A: Test bit 2 of A
        case 0x57:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 2) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,B: Test bit 3 of B
        case 0x58:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,C: Test bit 3 of C
        case 0x59:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,D: Test bit 3 of D
        case 0x5A:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,E: Test bit 3 of E
        case 0x5B:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,H: Test bit 3 of H
        case 0x5C:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,L: Test bit 3 of L
        case 0x5D:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,(HL): Test bit 3 of value pointed by HL
        case 0x5E:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 3,A: Test bit 3 of A
        case 0x5F:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 3) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,B: Test bit 4 of B
        case 0x60:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,C: Test bit 4 of C
        case 0x61:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,D: Test bit 4 of D
        case 0x62:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,E: Test bit 4 of E
        case 0x63:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,H: Test bit 4 of H
        case 0x64:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,L: Test bit 4 of L
        case 0x65:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,(HL): Test bit 4 of value pointed by HL
        case 0x66:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 4,A: Test bit 4 of A
        case 0x67:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 4) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,B: Test bit 5 of B
        case 0x68:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,C: Test bit 5 of C
        case 0x69:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,D: Test bit 5 of D
        case 0x6A:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,E: Test bit 5 of E
        case 0x6B:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,H: Test bit 5 of H
        case 0x6C:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,L: Test bit 5 of L
        case 0x6D:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,(HL): Test bit 5 of value pointed by HL
        case 0x6E:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 5,A: Test bit 5 of A
        case 0x6F:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 5) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,B: Test bit 6 of B
        case 0x70:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,C: Test bit 6 of C
        case 0x71:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,D: Test bit 6 of D
        case 0x72:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,E: Test bit 6 of E
        case 0x73:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,H: Test bit 6 of H
        case 0x74:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,L: Test bit 6 of L
        case 0x75:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,(HL): Test bit 6 of value pointed by HL
        case 0x76:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 6,A: Test bit 6 of A
        case 0x77:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 6) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,B: Test bit 7 of B
        case 0x78:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.B, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,C: Test bit 7 of C
        case 0x79:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.C, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,D: Test bit 7 of D
        case 0x7A:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.D, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,E: Test bit 7 of E
        case 0x7B:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.E, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,H: Test bit 7 of H
        case 0x7C:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.H, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,L: Test bit 7 of L
        case 0x7D:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.L, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,(HL): Test bit 7 of value pointed by HL
        case 0x7E:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.registers.HL), 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // BIT 7,A: Test bit 7 of A
        case 0x7F:
          {
            cpu.registers.FZ = (byte)(UtilFuncs.TestBit(cpu.registers.A, 7) == 0 ? 1 : 0);
            cpu.registers.FN = 0;
            cpu.registers.FH = 1;
            break;
          }

        // RES 0,B: Clear (reset) bit 0 of B
        case 0x80:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 0);
            break;
          }

        // RES 0,C: Clear (reset) bit 0 of C
        case 0x81:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 0);
            break;
          }

        // RES 0,D: Clear (reset) bit 0 of D
        case 0x82:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 0);
            break;
          }

        // RES 0,E: Clear (reset) bit 0 of E
        case 0x83:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 0);
            break;
          }

        // RES 0,H: Clear (reset) bit 0 of H
        case 0x84:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 0);
            break;
          }

        // RES 0,L: Clear (reset) bit 0 of L
        case 0x85:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 0);
            break;
          }

        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        case 0x86:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 0,A: Clear (reset) bit 0 of A
        case 0x87:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 0);
            break;
          }

        // RES 1,B: Clear (reset) bit 1 of B
        case 0x88:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 1);
            break;
          }

        // RES 1,C: Clear (reset) bit 1 of C
        case 0x89:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 1);
            break;
          }

        // RES 1,D: Clear (reset) bit 1 of D
        case 0x8A:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 1);
            break;
          }

        // RES 1,E: Clear (reset) bit 1 of E
        case 0x8B:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 1);
            break;
          }

        // RES 1,H: Clear (reset) bit 1 of H
        case 0x8C:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 1);
            break;
          }

        // RES 1,L: Clear (reset) bit 1 of L
        case 0x8D:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 1);
            break;
          }

        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        case 0x8E:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 1,A: Clear (reset) bit 1 of A
        case 0x8F:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 1);
            break;
          }

        // RES 2,B: Clear (reset) bit 2 of B
        case 0x90:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 2);
            break;
          }

        // RES 2,C: Clear (reset) bit 2 of C
        case 0x91:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 2);
            break;
          }

        // RES 2,D: Clear (reset) bit 2 of D
        case 0x92:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 2);
            break;
          }

        // RES 2,E: Clear (reset) bit 2 of E
        case 0x93:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 2);
            break;
          }

        // RES 2,H: Clear (reset) bit 2 of H
        case 0x94:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 2);
            break;
          }

        // RES 2,L: Clear (reset) bit 2 of L
        case 0x95:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 2);
            break;
          }

        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        case 0x96:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 2,A: Clear (reset) bit 2 of A
        case 0x97:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 2);
            break;
          }

        // RES 3,B: Clear (reset) bit 3 of B
        case 0x98:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 3);
            break;
          }

        // RES 3,C: Clear (reset) bit 3 of C
        case 0x99:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 3);
            break;
          }

        // RES 3,D: Clear (reset) bit 3 of D
        case 0x9A:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 3);
            break;
          }

        // RES 3,E: Clear (reset) bit 3 of E
        case 0x9B:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 3);
            break;
          }

        // RES 3,H: Clear (reset) bit 3 of H
        case 0x9C:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 3);
            break;
          }

        // RES 3,L: Clear (reset) bit 3 of L
        case 0x9D:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 3);
            break;
          }

        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        case 0x9E:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 3,A: Clear (reset) bit 3 of A
        case 0x9F:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 3);
            break;
          }

        // RES 4,B: Clear (reset) bit 4 of B
        case 0xA0:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 4);
            break;
          }

        // RES 4,C: Clear (reset) bit 4 of C
        case 0xA1:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 4);
            break;
          }

        // RES 4,D: Clear (reset) bit 4 of D
        case 0xA2:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 4);
            break;
          }

        // RES 4,E: Clear (reset) bit 4 of E
        case 0xA3:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 4);
            break;
          }

        // RES 4,H: Clear (reset) bit 4 of H
        case 0xA4:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 4);
            break;
          }

        // RES 4,L: Clear (reset) bit 4 of L
        case 0xA5:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 4);
            break;
          }

        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        case 0xA6:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 4,A: Clear (reset) bit 4 of A
        case 0xA7:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 4);
            break;
          }

        // RES 5,B: Clear (reset) bit 5 of B
        case 0xA8:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 5);
            break;
          }

        // RES 5,C: Clear (reset) bit 5 of C
        case 0xA9:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 5);
            break;
          }

        // RES 5,D: Clear (reset) bit 5 of D
        case 0xAA:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 5);
            break;
          }

        // RES 5,E: Clear (reset) bit 5 of E
        case 0xAB:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 5);
            break;
          }

        // RES 5,H: Clear (reset) bit 5 of H
        case 0xAC:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 5);
            break;
          }

        // RES 5,L: Clear (reset) bit 5 of L
        case 0xAD:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 5);
            break;
          }

        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        case 0xAE:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 5,A: Clear (reset) bit 5 of A
        case 0xAF:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 5);
            break;
          }

        // RES 6,B: Clear (reset) bit 6 of B
        case 0xB0:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 6);
            break;
          }

        // RES 6,C: Clear (reset) bit 6 of C
        case 0xB1:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 6);
            break;
          }

        // RES 6,D: Clear (reset) bit 6 of D
        case 0xB2:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 6);
            break;
          }

        // RES 6,E: Clear (reset) bit 6 of E
        case 0xB3:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 6);
            break;
          }

        // RES 6,H: Clear (reset) bit 6 of H
        case 0xB4:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 6);
            break;
          }

        // RES 6,L: Clear (reset) bit 6 of L
        case 0xB5:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 6);
            break;
          }

        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        case 0xB6:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 6,A: Clear (reset) bit 6 of A
        case 0xB7:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 6);
            break;
          }

        // RES 7,B: Clear (reset) bit 7 of B
        case 0xB8:
          {
            cpu.registers.B = UtilFuncs.ClearBit(cpu.registers.B, 7);
            break;
          }

        // RES 7,C: Clear (reset) bit 7 of C
        case 0xB9:
          {
            cpu.registers.C = UtilFuncs.ClearBit(cpu.registers.C, 7);
            break;
          }

        // RES 7,D: Clear (reset) bit 7 of D
        case 0xBA:
          {
            cpu.registers.D = UtilFuncs.ClearBit(cpu.registers.D, 7);
            break;
          }

        // RES 7,E: Clear (reset) bit 7 of E
        case 0xBB:
          {
            cpu.registers.E = UtilFuncs.ClearBit(cpu.registers.E, 7);
            break;
          }

        // RES 7,H: Clear (reset) bit 7 of H
        case 0xBC:
          {
            cpu.registers.H = UtilFuncs.ClearBit(cpu.registers.H, 7);
            break;
          }

        // RES 7,L: Clear (reset) bit 7 of L
        case 0xBD:
          {
            cpu.registers.L = UtilFuncs.ClearBit(cpu.registers.L, 7);
            break;
          }

        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        case 0xBE:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // RES 7,A: Clear (reset) bit 7 of A
        case 0xBF:
          {
            cpu.registers.A = UtilFuncs.ClearBit(cpu.registers.A, 7);
            break;
          }

        // SET 0,B: Set bit 0 of B
        case 0xC0:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 0);
            break;
          }

        // SET 0,C: Set bit 0 of C
        case 0xC1:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 0);
            break;
          }

        // SET 0,D: Set bit 0 of D
        case 0xC2:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 0);
            break;
          }

        // SET 0,E: Set bit 0 of E
        case 0xC3:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 0);
            break;
          }

        // SET 0,H: Set bit 0 of H
        case 0xC4:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 0);
            break;
          }

        // SET 0,L: Set bit 0 of L
        case 0xC5:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 0);
            break;
          }

        // SET 0,(HL): Set bit 0 of value pointed by HL
        case 0xC6:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 0,A: Set bit 0 of A
        case 0xC7:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 0);
            break;
          }

        // SET 1,B: Set bit 1 of B
        case 0xC8:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 1);
            break;
          }

        // SET 1,C: Set bit 1 of C
        case 0xC9:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 1);
            break;
          }

        // SET 1,D: Set bit 1 of D
        case 0xCA:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 1);
            break;
          }

        // SET 1,E: Set bit 1 of E
        case 0xCB:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 1);
            break;
          }

        // SET 1,H: Set bit 1 of H
        case 0xCC:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 1);
            break;
          }

        // SET 1,L: Set bit 1 of L
        case 0xCD:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 1);
            break;
          }

        // SET 1,(HL): Set bit 1 of value pointed by HL
        case 0xCE:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 1,A: Set bit 1 of A
        case 0xCF:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 1);
            break;
          }

        // SET 2,B: Set bit 2 of B
        case 0xD0:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 2);
            break;
          }

        // SET 2,C: Set bit 2 of C
        case 0xD1:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 2);
            break;
          }

        // SET 2,D: Set bit 2 of D
        case 0xD2:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 2);
            break;
          }

        // SET 2,E: Set bit 2 of E
        case 0xD3:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 2);
            break;
          }

        // SET 2,H: Set bit 2 of H
        case 0xD4:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 2);
            break;
          }

        // SET 2,L: Set bit 2 of L
        case 0xD5:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 2);
            break;
          }

        // SET 2,(HL): Set bit 2 of value pointed by HL
        case 0xD6:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 2,A: Set bit 2 of A
        case 0xD7:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 2);
            break;
          }

        // SET 3,B: Set bit 3 of B
        case 0xD8:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 3);
            break;
          }

        // SET 3,C: Set bit 3 of C
        case 0xD9:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 3);
            break;
          }

        // SET 3,D: Set bit 3 of D
        case 0xDA:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 3);
            break;
          }

        // SET 3,E: Set bit 3 of E
        case 0xDB:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 3);
            break;
          }

        // SET 3,H: Set bit 3 of H
        case 0xDC:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 3);
            break;
          }

        // SET 3,L: Set bit 3 of L
        case 0xDD:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 3);
            break;
          }

        // SET 3,(HL): Set bit 3 of value pointed by HL
        case 0xDE:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 3,A: Set bit 3 of A
        case 0xDF:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 3);
            break;
          }

        // SET 4,B: Set bit 4 of B
        case 0xE0:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 4);
            break;
          }

        // SET 4,C: Set bit 4 of C
        case 0xE1:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 4);
            break;
          }

        // SET 4,D: Set bit 4 of D
        case 0xE2:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 4);
            break;
          }

        // SET 4,E: Set bit 4 of E
        case 0xE3:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 4);
            break;
          }

        // SET 4,H: Set bit 4 of H
        case 0xE4:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 4);
            break;
          }

        // SET 4,L: Set bit 4 of L
        case 0xE5:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 4);
            break;
          }

        // SET 4,(HL): Set bit 4 of value pointed by HL
        case 0xE6:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 4,A: Set bit 4 of A
        case 0xE7:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 4);
            break;
          }

        // SET 5,B: Set bit 5 of B
        case 0xE8:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 5);
            break;
          }

        // SET 5,C: Set bit 5 of C
        case 0xE9:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 5);
            break;
          }

        // SET 5,D: Set bit 5 of D
        case 0xEA:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 5);
            break;
          }

        // SET 5,E: Set bit 5 of E
        case 0xEB:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 5);
            break;
          }

        // SET 5,H: Set bit 5 of H
        case 0xEC:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 5);
            break;
          }

        // SET 5,L: Set bit 5 of L
        case 0xED:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 5);
            break;
          }

        // SET 5,(HL): Set bit 5 of value pointed by HL
        case 0xEE:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 5,A: Set bit 5 of A
        case 0xEF:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 5);
            break;
          }

        // SET 6,B: Set bit 6 of B
        case 0xF0:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 6);
            break;
          }

        // SET 6,C: Set bit 6 of C
        case 0xF1:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 6);
            break;
          }

        // SET 6,D: Set bit 6 of D
        case 0xF2:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 6);
            break;
          }

        // SET 6,E: Set bit 6 of E
        case 0xF3:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 6);
            break;
          }

        // SET 6,H: Set bit 6 of H
        case 0xF4:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 6);
            break;
          }

        // SET 6,L: Set bit 6 of L
        case 0xF5:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 6);
            break;
          }

        // SET 6,(HL): Set bit 6 of value pointed by HL
        case 0xF6:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 6,A: Set bit 6 of A
        case 0xF7:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 6);
            break;
          }

        // SET 7,B: Set bit 7 of B
        case 0xF8:
          {
            cpu.registers.B = UtilFuncs.SetBit(cpu.registers.B, 7);
            break;
          }

        // SET 7,C: Set bit 7 of C
        case 0xF9:
          {
            cpu.registers.C = UtilFuncs.SetBit(cpu.registers.C, 7);
            break;
          }

        // SET 7,D: Set bit 7 of D
        case 0xFA:
          {
            cpu.registers.D = UtilFuncs.SetBit(cpu.registers.D, 7);
            break;
          }

        // SET 7,E: Set bit 7 of E
        case 0xFB:
          {
            cpu.registers.E = UtilFuncs.SetBit(cpu.registers.E, 7);
            break;
          }

        // SET 7,H: Set bit 7 of H
        case 0xFC:
          {
            cpu.registers.H = UtilFuncs.SetBit(cpu.registers.H, 7);
            break;
          }

        // SET 7,L: Set bit 7 of L
        case 0xFD:
          {
            cpu.registers.L = UtilFuncs.SetBit(cpu.registers.L, 7);
            break;
          }

        // SET 7,(HL): Set bit 7 of value pointed by HL
        case 0xFE:
          {
            cpu.registers.TEMP = cpu.memory.Read(cpu.registers.HL);
            break;
          }

        // SET 7,A: Set bit 7 of A
        case 0xFF:
          {
            cpu.registers.A = UtilFuncs.SetBit(cpu.registers.A, 7);
            break;
          }
      }
    }
  }
}
