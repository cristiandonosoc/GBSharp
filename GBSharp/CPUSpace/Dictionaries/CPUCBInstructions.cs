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
    /// Runs an CB opcode instruction. Notice some instructions have two-stage approach:
    /// They read in the normal execution (and store the value in a temporary Registers)
    /// and actually write in a post-execution step. This is because some instruction
    /// read and write on different clock ticks
    /// The code for the post is in CPUCBInstructionPostCode
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
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.B);
            cpu.Registers.B = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RLC C: Rotate C left with carry
        case 0x01:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.C);
            cpu.Registers.C = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RLC D: Rotate D left with carry
        case 0x02:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.D);
            cpu.Registers.D = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RLC E: Rotate E left with carry
        case 0x03:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.E);
            cpu.Registers.E = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RLC H: Rotate H left with carry
        case 0x04:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.H);
            cpu.Registers.H = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RLC L: Rotate L left with carry
        case 0x05:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.L);
            cpu.Registers.L = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RLC (HL): Rotate value pointed by HL left with carry
        // NOTE: two-stage opcode
        case 0x06:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RLC A: Rotate A left with carry
        case 0x07:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.A);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC B: Rotate B right with carry
        case 0x08:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.B);
            cpu.Registers.B = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC C: Rotate C right with carry
        case 0x09:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.C);
            cpu.Registers.C = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC D: Rotate D right with carry
        case 0x0A:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.D);
            cpu.Registers.D = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC E: Rotate E right with carry
        case 0x0B:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.E);
            cpu.Registers.E = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC H: Rotate H right with carry
        case 0x0C:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.H);
            cpu.Registers.H = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC L: Rotate L right with carry
        case 0x0D:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.L);
            cpu.Registers.L = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RRC (HL): Rotate value pointed by HL right with carry
        // NOTE: two-stage opcode
        case 0x0E:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RRC A: Rotate A right with carry
        case 0x0F:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.A);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL B: Rotate B left
        case 0x10:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.B, 1, cpu.Registers.FC);
            cpu.Registers.B = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL C: Rotate C left
        case 0x11:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.C, 1, cpu.Registers.FC);
            cpu.Registers.C = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL D: Rotate D left
        case 0x12:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.D, 1, cpu.Registers.FC);
            cpu.Registers.D = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL E: Rotate E left
        case 0x13:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.E, 1, cpu.Registers.FC);
            cpu.Registers.E = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL H: Rotate H left
        case 0x14:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.H, 1, cpu.Registers.FC);
            cpu.Registers.H = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL L: Rotate L left
        case 0x15:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.L, 1, cpu.Registers.FC);
            cpu.Registers.L = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RL (HL): Rotate value pointed by HL left
        // NOTE: two-stage opcode
        case 0x16:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RL A: Rotate A left
        case 0x17:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.A, 1, cpu.Registers.FC);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR B: Rotate B right
        case 0x18:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.B, 1, cpu.Registers.FC);
            cpu.Registers.B = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR C: Rotate C right
        case 0x19:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.C, 1, cpu.Registers.FC);
            cpu.Registers.C = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR D: Rotate D right
        case 0x1A:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.D, 1, cpu.Registers.FC);
            cpu.Registers.D = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR E: Rotate E right
        case 0x1B:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.E, 1, cpu.Registers.FC);
            cpu.Registers.E = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR H: Rotate H right
        case 0x1C:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.H, 1, cpu.Registers.FC);
            cpu.Registers.H = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR L: Rotate L right
        case 0x1D:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.L, 1, cpu.Registers.FC);
            cpu.Registers.L = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // RR (HL): Rotate value pointed by HL right
        // NOTE: two-stage opcode
        case 0x1E:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RR A: Rotate A right
        case 0x1F:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.A, 1, cpu.Registers.FC);
            cpu.Registers.A = rotateCarry.Item1;

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA B: Shift B left preserving sign
        case 0x20:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.B);
            cpu.Registers.B = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA C: Shift C left preserving sign
        case 0x21:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.C);
            cpu.Registers.C = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA D: Shift D left preserving sign
        case 0x22:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.D);
            cpu.Registers.D = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA E: Shift E left preserving sign
        case 0x23:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.E);
            cpu.Registers.E = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA H: Shift H left preserving sign
        case 0x24:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.H);
            cpu.Registers.H = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA L: Shift L left preserving sign
        case 0x25:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.L);
            cpu.Registers.L = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SLA (HL): Shift value pointed by HL left preserving sign
        // NOTE: two-stage opcode
        case 0x26:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SLA A: Shift A left preserving sign
        case 0x27:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.A);
            cpu.Registers.A = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
          }
          break;
        // SRA B: Shift B right preserving sign
        case 0x28:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.B);
            cpu.Registers.B = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRA C: Shift C right preserving sign
        case 0x29:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.C);
            cpu.Registers.C = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRA D: Shift D right preserving sign
        case 0x2A:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.D);
            cpu.Registers.D = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRA E: Shift E right preserving sign
        case 0x2B:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.E);
            cpu.Registers.E = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRA H: Shift H right preserving sign
        case 0x2C:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.H);
            cpu.Registers.H = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRA L: Shift L right preserving sign
        case 0x2D:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.L);
            cpu.Registers.L = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRA (HL): Shift value pointed by HL right preserving sign
        // NOTE: two-stage opcode
        case 0x2E:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SRA A: Shift A right preserving sign
        case 0x2F:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.A);
            cpu.Registers.A = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SWAP B: Swap nybbles in B
        case 0x30:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.B);
            cpu.Registers.B = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SWAP C: Swap nybbles in C
        case 0x31:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.C);
            cpu.Registers.C = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SWAP D: Swap nybbles in D
        case 0x32:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.D);
            cpu.Registers.D = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SWAP E: Swap nybbles in E
        case 0x33:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.E);
            cpu.Registers.E = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SWAP H: Swap nybbles in H
        case 0x34:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.H);
            cpu.Registers.H = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SWAP L: Swap nybbles in L
        case 0x35:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.L);
            cpu.Registers.L = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SWAP (HL): Swap nybbles in value pointed by HL
        // NOTE: two-stage opcode
        case 0x36:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SWAP A: Swap nybbles in A
        case 0x37:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.A);
            cpu.Registers.A = result;

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }

        // SRL B: Shift B right
        case 0x38:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.B);
            cpu.Registers.B = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRL C: Shift C right
        case 0x39:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.C);
            cpu.Registers.C = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRL D: Shift D right
        case 0x3A:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.D);
            cpu.Registers.D = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRL E: Shift E right
        case 0x3B:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.E);
            cpu.Registers.E = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRL H: Shift H right
        case 0x3C:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.H);
            cpu.Registers.H = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRL L: Shift L right
        case 0x3D:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.L);
            cpu.Registers.L = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // SRL (HL): Shift value pointed by HL right
        // NOTE: two-stage opcode
        case 0x3E:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SRL A: Shift A right
        case 0x3F:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.A);
            cpu.Registers.A = shiftCarry.Item1;

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }

        // BIT 0,B: Test bit 0 of B
        case 0x40:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,C: Test bit 0 of C
        case 0x41:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,D: Test bit 0 of D
        case 0x42:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,E: Test bit 0 of E
        case 0x43:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,H: Test bit 0 of H
        case 0x44:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,L: Test bit 0 of L
        case 0x45:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,(HL): Test bit 0 of value pointed by HL
        case 0x46:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 0,A: Test bit 0 of A
        case 0x47:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 0) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,B: Test bit 1 of B
        case 0x48:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,C: Test bit 1 of C
        case 0x49:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,D: Test bit 1 of D
        case 0x4A:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,E: Test bit 1 of E
        case 0x4B:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,H: Test bit 1 of H
        case 0x4C:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,L: Test bit 1 of L
        case 0x4D:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,(HL): Test bit 1 of value pointed by HL
        case 0x4E:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 1,A: Test bit 1 of A
        case 0x4F:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 1) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,B: Test bit 2 of B
        case 0x50:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,C: Test bit 2 of C
        case 0x51:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,D: Test bit 2 of D
        case 0x52:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,E: Test bit 2 of E
        case 0x53:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,H: Test bit 2 of H
        case 0x54:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,L: Test bit 2 of L
        case 0x55:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,(HL): Test bit 2 of value pointed by HL
        case 0x56:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 2,A: Test bit 2 of A
        case 0x57:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 2) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,B: Test bit 3 of B
        case 0x58:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,C: Test bit 3 of C
        case 0x59:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,D: Test bit 3 of D
        case 0x5A:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,E: Test bit 3 of E
        case 0x5B:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,H: Test bit 3 of H
        case 0x5C:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,L: Test bit 3 of L
        case 0x5D:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,(HL): Test bit 3 of value pointed by HL
        case 0x5E:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 3,A: Test bit 3 of A
        case 0x5F:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 3) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,B: Test bit 4 of B
        case 0x60:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,C: Test bit 4 of C
        case 0x61:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,D: Test bit 4 of D
        case 0x62:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,E: Test bit 4 of E
        case 0x63:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,H: Test bit 4 of H
        case 0x64:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,L: Test bit 4 of L
        case 0x65:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,(HL): Test bit 4 of value pointed by HL
        case 0x66:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 4,A: Test bit 4 of A
        case 0x67:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 4) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,B: Test bit 5 of B
        case 0x68:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,C: Test bit 5 of C
        case 0x69:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,D: Test bit 5 of D
        case 0x6A:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,E: Test bit 5 of E
        case 0x6B:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,H: Test bit 5 of H
        case 0x6C:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,L: Test bit 5 of L
        case 0x6D:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,(HL): Test bit 5 of value pointed by HL
        case 0x6E:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 5,A: Test bit 5 of A
        case 0x6F:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 5) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,B: Test bit 6 of B
        case 0x70:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,C: Test bit 6 of C
        case 0x71:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,D: Test bit 6 of D
        case 0x72:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,E: Test bit 6 of E
        case 0x73:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,H: Test bit 6 of H
        case 0x74:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,L: Test bit 6 of L
        case 0x75:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,(HL): Test bit 6 of value pointed by HL
        case 0x76:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 6,A: Test bit 6 of A
        case 0x77:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 6) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,B: Test bit 7 of B
        case 0x78:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.B, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,C: Test bit 7 of C
        case 0x79:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.C, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,D: Test bit 7 of D
        case 0x7A:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.D, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,E: Test bit 7 of E
        case 0x7B:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.E, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,H: Test bit 7 of H
        case 0x7C:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.H, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,L: Test bit 7 of L
        case 0x7D:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.L, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,(HL): Test bit 7 of value pointed by HL
        case 0x7E:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.memory.Read(cpu.Registers.HL), 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // BIT 7,A: Test bit 7 of A
        case 0x7F:
          {
            cpu.Registers.FZ = (byte)(UtilFuncs.TestBit(cpu.Registers.A, 7) == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 1;
            break;
          }

        // RES 0,B: Clear (reset) bit 0 of B
        case 0x80:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 0);
            break;
          }

        // RES 0,C: Clear (reset) bit 0 of C
        case 0x81:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 0);
            break;
          }

        // RES 0,D: Clear (reset) bit 0 of D
        case 0x82:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 0);
            break;
          }

        // RES 0,E: Clear (reset) bit 0 of E
        case 0x83:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 0);
            break;
          }

        // RES 0,H: Clear (reset) bit 0 of H
        case 0x84:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 0);
            break;
          }

        // RES 0,L: Clear (reset) bit 0 of L
        case 0x85:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 0);
            break;
          }

        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x86:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 0,A: Clear (reset) bit 0 of A
        case 0x87:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 0);
            break;
          }

        // RES 1,B: Clear (reset) bit 1 of B
        case 0x88:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 1);
            break;
          }

        // RES 1,C: Clear (reset) bit 1 of C
        case 0x89:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 1);
            break;
          }

        // RES 1,D: Clear (reset) bit 1 of D
        case 0x8A:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 1);
            break;
          }

        // RES 1,E: Clear (reset) bit 1 of E
        case 0x8B:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 1);
            break;
          }

        // RES 1,H: Clear (reset) bit 1 of H
        case 0x8C:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 1);
            break;
          }

        // RES 1,L: Clear (reset) bit 1 of L
        case 0x8D:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 1);
            break;
          }

        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x8E:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 1,A: Clear (reset) bit 1 of A
        case 0x8F:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 1);
            break;
          }

        // RES 2,B: Clear (reset) bit 2 of B
        case 0x90:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 2);
            break;
          }

        // RES 2,C: Clear (reset) bit 2 of C
        case 0x91:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 2);
            break;
          }

        // RES 2,D: Clear (reset) bit 2 of D
        case 0x92:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 2);
            break;
          }

        // RES 2,E: Clear (reset) bit 2 of E
        case 0x93:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 2);
            break;
          }

        // RES 2,H: Clear (reset) bit 2 of H
        case 0x94:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 2);
            break;
          }

        // RES 2,L: Clear (reset) bit 2 of L
        case 0x95:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 2);
            break;
          }

        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x96:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 2,A: Clear (reset) bit 2 of A
        case 0x97:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 2);
            break;
          }

        // RES 3,B: Clear (reset) bit 3 of B
        case 0x98:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 3);
            break;
          }

        // RES 3,C: Clear (reset) bit 3 of C
        case 0x99:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 3);
            break;
          }

        // RES 3,D: Clear (reset) bit 3 of D
        case 0x9A:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 3);
            break;
          }

        // RES 3,E: Clear (reset) bit 3 of E
        case 0x9B:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 3);
            break;
          }

        // RES 3,H: Clear (reset) bit 3 of H
        case 0x9C:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 3);
            break;
          }

        // RES 3,L: Clear (reset) bit 3 of L
        case 0x9D:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 3);
            break;
          }

        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x9E:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 3,A: Clear (reset) bit 3 of A
        case 0x9F:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 3);
            break;
          }

        // RES 4,B: Clear (reset) bit 4 of B
        case 0xA0:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 4);
            break;
          }

        // RES 4,C: Clear (reset) bit 4 of C
        case 0xA1:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 4);
            break;
          }

        // RES 4,D: Clear (reset) bit 4 of D
        case 0xA2:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 4);
            break;
          }

        // RES 4,E: Clear (reset) bit 4 of E
        case 0xA3:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 4);
            break;
          }

        // RES 4,H: Clear (reset) bit 4 of H
        case 0xA4:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 4);
            break;
          }

        // RES 4,L: Clear (reset) bit 4 of L
        case 0xA5:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 4);
            break;
          }

        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xA6:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 4,A: Clear (reset) bit 4 of A
        case 0xA7:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 4);
            break;
          }

        // RES 5,B: Clear (reset) bit 5 of B
        case 0xA8:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 5);
            break;
          }

        // RES 5,C: Clear (reset) bit 5 of C
        case 0xA9:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 5);
            break;
          }

        // RES 5,D: Clear (reset) bit 5 of D
        case 0xAA:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 5);
            break;
          }

        // RES 5,E: Clear (reset) bit 5 of E
        case 0xAB:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 5);
            break;
          }

        // RES 5,H: Clear (reset) bit 5 of H
        case 0xAC:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 5);
            break;
          }

        // RES 5,L: Clear (reset) bit 5 of L
        case 0xAD:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 5);
            break;
          }

        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xAE:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 5,A: Clear (reset) bit 5 of A
        case 0xAF:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 5);
            break;
          }

        // RES 6,B: Clear (reset) bit 6 of B
        case 0xB0:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 6);
            break;
          }

        // RES 6,C: Clear (reset) bit 6 of C
        case 0xB1:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 6);
            break;
          }

        // RES 6,D: Clear (reset) bit 6 of D
        case 0xB2:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 6);
            break;
          }

        // RES 6,E: Clear (reset) bit 6 of E
        case 0xB3:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 6);
            break;
          }

        // RES 6,H: Clear (reset) bit 6 of H
        case 0xB4:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 6);
            break;
          }

        // RES 6,L: Clear (reset) bit 6 of L
        case 0xB5:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 6);
            break;
          }

        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xB6:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 6,A: Clear (reset) bit 6 of A
        case 0xB7:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 6);
            break;
          }

        // RES 7,B: Clear (reset) bit 7 of B
        case 0xB8:
          {
            cpu.Registers.B = UtilFuncs.ClearBit(cpu.Registers.B, 7);
            break;
          }

        // RES 7,C: Clear (reset) bit 7 of C
        case 0xB9:
          {
            cpu.Registers.C = UtilFuncs.ClearBit(cpu.Registers.C, 7);
            break;
          }

        // RES 7,D: Clear (reset) bit 7 of D
        case 0xBA:
          {
            cpu.Registers.D = UtilFuncs.ClearBit(cpu.Registers.D, 7);
            break;
          }

        // RES 7,E: Clear (reset) bit 7 of E
        case 0xBB:
          {
            cpu.Registers.E = UtilFuncs.ClearBit(cpu.Registers.E, 7);
            break;
          }

        // RES 7,H: Clear (reset) bit 7 of H
        case 0xBC:
          {
            cpu.Registers.H = UtilFuncs.ClearBit(cpu.Registers.H, 7);
            break;
          }

        // RES 7,L: Clear (reset) bit 7 of L
        case 0xBD:
          {
            cpu.Registers.L = UtilFuncs.ClearBit(cpu.Registers.L, 7);
            break;
          }

        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xBE:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // RES 7,A: Clear (reset) bit 7 of A
        case 0xBF:
          {
            cpu.Registers.A = UtilFuncs.ClearBit(cpu.Registers.A, 7);
            break;
          }

        // SET 0,B: Set bit 0 of B
        case 0xC0:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 0);
            break;
          }

        // SET 0,C: Set bit 0 of C
        case 0xC1:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 0);
            break;
          }

        // SET 0,D: Set bit 0 of D
        case 0xC2:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 0);
            break;
          }

        // SET 0,E: Set bit 0 of E
        case 0xC3:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 0);
            break;
          }

        // SET 0,H: Set bit 0 of H
        case 0xC4:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 0);
            break;
          }

        // SET 0,L: Set bit 0 of L
        case 0xC5:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 0);
            break;
          }

        // SET 0,(HL): Set bit 0 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xC6:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 0,A: Set bit 0 of A
        case 0xC7:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 0);
            break;
          }

        // SET 1,B: Set bit 1 of B
        case 0xC8:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 1);
            break;
          }

        // SET 1,C: Set bit 1 of C
        case 0xC9:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 1);
            break;
          }

        // SET 1,D: Set bit 1 of D
        case 0xCA:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 1);
            break;
          }

        // SET 1,E: Set bit 1 of E
        case 0xCB:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 1);
            break;
          }

        // SET 1,H: Set bit 1 of H
        case 0xCC:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 1);
            break;
          }

        // SET 1,L: Set bit 1 of L
        case 0xCD:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 1);
            break;
          }

        // SET 1,(HL): Set bit 1 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xCE:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 1,A: Set bit 1 of A
        case 0xCF:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 1);
            break;
          }

        // SET 2,B: Set bit 2 of B
        case 0xD0:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 2);
            break;
          }

        // SET 2,C: Set bit 2 of C
        case 0xD1:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 2);
            break;
          }

        // SET 2,D: Set bit 2 of D
        case 0xD2:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 2);
            break;
          }

        // SET 2,E: Set bit 2 of E
        case 0xD3:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 2);
            break;
          }

        // SET 2,H: Set bit 2 of H
        case 0xD4:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 2);
            break;
          }

        // SET 2,L: Set bit 2 of L
        case 0xD5:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 2);
            break;
          }

        // SET 2,(HL): Set bit 2 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xD6:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 2,A: Set bit 2 of A
        case 0xD7:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 2);
            break;
          }

        // SET 3,B: Set bit 3 of B
        case 0xD8:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 3);
            break;
          }

        // SET 3,C: Set bit 3 of C
        case 0xD9:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 3);
            break;
          }

        // SET 3,D: Set bit 3 of D
        case 0xDA:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 3);
            break;
          }

        // SET 3,E: Set bit 3 of E
        case 0xDB:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 3);
            break;
          }

        // SET 3,H: Set bit 3 of H
        case 0xDC:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 3);
            break;
          }

        // SET 3,L: Set bit 3 of L
        case 0xDD:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 3);
            break;
          }

        // SET 3,(HL): Set bit 3 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xDE:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 3,A: Set bit 3 of A
        case 0xDF:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 3);
            break;
          }

        // SET 4,B: Set bit 4 of B
        case 0xE0:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 4);
            break;
          }

        // SET 4,C: Set bit 4 of C
        case 0xE1:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 4);
            break;
          }

        // SET 4,D: Set bit 4 of D
        case 0xE2:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 4);
            break;
          }

        // SET 4,E: Set bit 4 of E
        case 0xE3:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 4);
            break;
          }

        // SET 4,H: Set bit 4 of H
        case 0xE4:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 4);
            break;
          }

        // SET 4,L: Set bit 4 of L
        case 0xE5:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 4);
            break;
          }

        // SET 4,(HL): Set bit 4 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xE6:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 4,A: Set bit 4 of A
        case 0xE7:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 4);
            break;
          }

        // SET 5,B: Set bit 5 of B
        case 0xE8:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 5);
            break;
          }

        // SET 5,C: Set bit 5 of C
        case 0xE9:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 5);
            break;
          }

        // SET 5,D: Set bit 5 of D
        case 0xEA:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 5);
            break;
          }

        // SET 5,E: Set bit 5 of E
        case 0xEB:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 5);
            break;
          }

        // SET 5,H: Set bit 5 of H
        case 0xEC:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 5);
            break;
          }

        // SET 5,L: Set bit 5 of L
        case 0xED:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 5);
            break;
          }

        // SET 5,(HL): Set bit 5 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xEE:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 5,A: Set bit 5 of A
        case 0xEF:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 5);
            break;
          }

        // SET 6,B: Set bit 6 of B
        case 0xF0:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 6);
            break;
          }

        // SET 6,C: Set bit 6 of C
        case 0xF1:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 6);
            break;
          }

        // SET 6,D: Set bit 6 of D
        case 0xF2:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 6);
            break;
          }

        // SET 6,E: Set bit 6 of E
        case 0xF3:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 6);
            break;
          }

        // SET 6,H: Set bit 6 of H
        case 0xF4:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 6);
            break;
          }

        // SET 6,L: Set bit 6 of L
        case 0xF5:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 6);
            break;
          }

        // SET 6,(HL): Set bit 6 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xF6:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 6,A: Set bit 6 of A
        case 0xF7:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 6);
            break;
          }

        // SET 7,B: Set bit 7 of B
        case 0xF8:
          {
            cpu.Registers.B = UtilFuncs.SetBit(cpu.Registers.B, 7);
            break;
          }

        // SET 7,C: Set bit 7 of C
        case 0xF9:
          {
            cpu.Registers.C = UtilFuncs.SetBit(cpu.Registers.C, 7);
            break;
          }

        // SET 7,D: Set bit 7 of D
        case 0xFA:
          {
            cpu.Registers.D = UtilFuncs.SetBit(cpu.Registers.D, 7);
            break;
          }

        // SET 7,E: Set bit 7 of E
        case 0xFB:
          {
            cpu.Registers.E = UtilFuncs.SetBit(cpu.Registers.E, 7);
            break;
          }

        // SET 7,H: Set bit 7 of H
        case 0xFC:
          {
            cpu.Registers.H = UtilFuncs.SetBit(cpu.Registers.H, 7);
            break;
          }

        // SET 7,L: Set bit 7 of L
        case 0xFD:
          {
            cpu.Registers.L = UtilFuncs.SetBit(cpu.Registers.L, 7);
            break;
          }

        // SET 7,(HL): Set bit 7 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xFE:
          {
            cpu.Registers.TEMP = cpu.memory.Read(cpu.Registers.HL);
            break;
          }

        // SET 7,A: Set bit 7 of A
        case 0xFF:
          {
            cpu.Registers.A = UtilFuncs.SetBit(cpu.Registers.A, 7);
            break;
          }
      }
    }
  }
}
