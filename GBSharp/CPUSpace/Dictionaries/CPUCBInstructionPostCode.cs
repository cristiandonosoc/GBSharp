using GBSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUCBInstructionPostCode
  {
    /// <summary>
    /// Runs an CB opcode post code. This is for instruction that read an write
    /// on different clock cycles
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    internal static void Run(CPU cpu, byte opcode, ushort n)
    {
      switch (opcode)
      {
        // RLC B: Rotate B left with carry
        case 0x00: { break; }
        // RLC C: Rotate C left with carry
        case 0x01: { break; }
        // RLC D: Rotate D left with carry
        case 0x02: { break; }
        // RLC E: Rotate E left with carry
        case 0x03: { break; }
        // RLC H: Rotate H left with carry
        case 0x04: { break; }
        // RLC L: Rotate L left with carry
        case 0x05: { break; }
        // RLC (HL): Rotate value pointed by HL left with carry
        // NOTE: two-stage opcode
        case 0x06:
          {
            var rotateCarry = UtilFuncs.RotateLeftAndCarry(cpu.Registers.TEMP);
            cpu._memory.Write(cpu.Registers.HL, rotateCarry.Item1);

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // RLC A: Rotate A left with carry
        case 0x07: { break; }
        // RRC B: Rotate B right with carry
        case 0x08: { break; }
        // RRC C: Rotate C right with carry
        case 0x09: { break; }
        // RRC D: Rotate D right with carry
        case 0x0A: { break; }
        // RRC E: Rotate E right with carry
        case 0x0B: { break; }
        // RRC H: Rotate H right with carry
        case 0x0C: { break; }
        // RRC L: Rotate L right with carry
        case 0x0D: { break; }
        // RRC (HL): Rotate value pointed by HL right with carry
        // NOTE: two-stage opcode
        case 0x0E:
          {
            var rotateCarry = UtilFuncs.RotateRightAndCarry(cpu.Registers.TEMP);
            cpu._memory.Write(cpu.Registers.HL, rotateCarry.Item1);

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // RRC A: Rotate A right with carry
        case 0x0F: { break; }
        // RL B: Rotate B left
        case 0x10: { break; }
        // RL C: Rotate C left
        case 0x11: { break; }
        // RL D: Rotate D left
        case 0x12: { break; }
        // RL E: Rotate E left
        case 0x13: { break; }
        // RL H: Rotate H left
        case 0x14: { break; }
        // RL L: Rotate L left
        case 0x15: { break; }
        // RL (HL): Rotate value pointed by HL left
        // NOTE: two-stage opcode
        case 0x16:
          {
            var rotateCarry = UtilFuncs.RotateLeftThroughCarry(cpu.Registers.TEMP, 1, cpu.Registers.FC);
            cpu._memory.Write(cpu.Registers.HL, rotateCarry.Item1);

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // RL A: Rotate A left
        case 0x17: { break; }
        // RR B: Rotate B right
        case 0x18: { break; }
        // RR C: Rotate C right
        case 0x19: { break; }
        // RR D: Rotate D right
        case 0x1A: { break; }
        // RR E: Rotate E right
        case 0x1B: { break; }
        // RR H: Rotate H right
        case 0x1C: { break; }
        // RR L: Rotate L right
        case 0x1D: { break; }
        // RR (HL): Rotate value pointed by HL right
        // NOTE: two-stage opcode
        case 0x1E:
          {
            var rotateCarry = UtilFuncs.RotateRightThroughCarry(cpu.Registers.TEMP, 1, cpu.Registers.FC);
            cpu._memory.Write(cpu.Registers.HL, rotateCarry.Item1);

            cpu.Registers.FC = rotateCarry.Item2;
            cpu.Registers.FZ = (byte)((rotateCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // RR A: Rotate A right
        case 0x1F: { break; }
        // SLA B: Shift B left preserving sign
        case 0x20: { break; }
        // SLA C: Shift C left preserving sign
        case 0x21: { break; }
        // SLA D: Shift D left preserving sign
        case 0x22: { break; }
        // SLA E: Shift E left preserving sign
        case 0x23: { break; }
        // SLA H: Shift H left preserving sign
        case 0x24: { break; }
        // SLA L: Shift L left preserving sign
        case 0x25: { break; }
        // SLA (HL): Shift value pointed by HL left preserving sign
        // NOTE: two-stage opcode
        case 0x26:
          {
            var shiftCarry = UtilFuncs.ShiftLeft(cpu.Registers.TEMP);
            cpu._memory.Write(cpu.Registers.HL, shiftCarry.Item1);

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // SLA A: Shift A left preserving sign
        case 0x27: { break; }
        case 0x28: { break; }
        // SRA C: Shift C right preserving sign
        case 0x29: { break; }
        // SRA D: Shift D right preserving sign
        case 0x2A: { break; }
        // SRA E: Shift E right preserving sign
        case 0x2B: { break; }
        // SRA H: Shift H right preserving sign
        case 0x2C: { break; }
        // SRA L: Shift L right preserving sign
        case 0x2D: { break; }
        // SRA (HL): Shift value pointed by HL right preserving sign
        // NOTE: two-stage opcode
        case 0x2E:
          {
            var shiftCarry = UtilFuncs.ShiftRightArithmetic(cpu.Registers.TEMP);
            cpu._memory.Write(cpu.Registers.HL, shiftCarry.Item1);

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // SRA A: Shift A right preserving sign
        case 0x2F: { break; }
        // SWAP B: Swap nybbles in B
        case 0x30: { break; }
        // SWAP C: Swap nybbles in C
        case 0x31: { break; }
        // SWAP D: Swap nybbles in D
        case 0x32: { break; }
        // SWAP E: Swap nybbles in E
        case 0x33: { break; }
        // SWAP H: Swap nybbles in H
        case 0x34: { break; }
        // SWAP L: Swap nybbles in L
        case 0x35: { break; }
        // SWAP (HL): Swap nybbles in value pointed by HL
        // NOTE: two-stage opcode
        case 0x36:
          {
            byte result = UtilFuncs.SwapNibbles(cpu.Registers.TEMP);
            cpu._memory.Write(cpu.Registers.HL, result);

            cpu.Registers.FZ = (byte)(result == 0 ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            cpu.Registers.FC = 0;
            break;
          }
        // SWAP A: Swap nybbles in A
        case 0x37: { break; }
        // SRL B: Shift B right
        case 0x38: { break; }
        // SRL C: Shift C right
        case 0x39: { break; }
        // SRL D: Shift D right
        case 0x3A: { break; }
        // SRL E: Shift E right
        case 0x3B: { break; }
        // SRL H: Shift H right
        case 0x3C: { break; }
        // SRL L: Shift L right
        case 0x3D: { break; }
        // SRL (HL): Shift value pointed by HL right
        // NOTE: two-stage opcode
        case 0x3E:
          {
            var shiftCarry = UtilFuncs.ShiftRightLogic(cpu.Registers.TEMP);
            cpu._memory.Write(cpu.Registers.HL, shiftCarry.Item1);

            cpu.Registers.FC = shiftCarry.Item2;
            cpu.Registers.FZ = (byte)((shiftCarry.Item1 == 0) ? 1 : 0);
            cpu.Registers.FN = 0;
            cpu.Registers.FH = 0;
            break;
          }
        // SRL A: Shift A right
        case 0x3F: { break; }
        // BIT 0,B: Test bit 0 of B
        case 0x40: { break; }
        // BIT 0,C: Test bit 0 of C
        case 0x41: { break; }
        // BIT 0,D: Test bit 0 of D
        case 0x42: { break; }
        // BIT 0,E: Test bit 0 of E
        case 0x43: { break; }
        // BIT 0,H: Test bit 0 of H
        case 0x44: { break; }
        // BIT 0,L: Test bit 0 of L
        case 0x45: { break; }
        // BIT 0,(HL): Test bit 0 of value pointed by HL
        case 0x46: { break; }
        // BIT 0,A: Test bit 0 of A
        case 0x47: { break; }
        // BIT 1,B: Test bit 1 of B
        case 0x48: { break; }
        // BIT 1,C: Test bit 1 of C
        case 0x49: { break; }
        // BIT 1,D: Test bit 1 of D
        case 0x4A: { break; }
        // BIT 1,E: Test bit 1 of E
        case 0x4B: { break; }
        // BIT 1,H: Test bit 1 of H
        case 0x4C: { break; }
        // BIT 1,L: Test bit 1 of L
        case 0x4D: { break; }
        // BIT 1,(HL): Test bit 1 of value pointed by HL
        case 0x4E: { break; }
        // BIT 1,A: Test bit 1 of A
        case 0x4F: { break; }
        // BIT 2,B: Test bit 2 of B
        case 0x50: { break; }
        // BIT 2,C: Test bit 2 of C
        case 0x51: { break; }
        // BIT 2,D: Test bit 2 of D
        case 0x52: { break; }
        // BIT 2,E: Test bit 2 of E
        case 0x53: { break; }
        // BIT 2,H: Test bit 2 of H
        case 0x54: { break; }
        // BIT 2,L: Test bit 2 of L
        case 0x55: { break; }
        // BIT 2,(HL): Test bit 2 of value pointed by HL
        case 0x56: { break; }
        // BIT 2,A: Test bit 2 of A
        case 0x57: { break; }
        // BIT 3,B: Test bit 3 of B
        case 0x58: { break; }
        // BIT 3,C: Test bit 3 of C
        case 0x59: { break; }
        // BIT 3,D: Test bit 3 of D
        case 0x5A: { break; }
        // BIT 3,E: Test bit 3 of E
        case 0x5B: { break; }
        // BIT 3,H: Test bit 3 of H
        case 0x5C: { break; }
        // BIT 3,L: Test bit 3 of L
        case 0x5D: { break; }
        // BIT 3,(HL): Test bit 3 of value pointed by HL
        case 0x5E: { break; }
        // BIT 3,A: Test bit 3 of A
        case 0x5F: { break; }
        // BIT 4,B: Test bit 4 of B
        case 0x60: { break; }
        // BIT 4,C: Test bit 4 of C
        case 0x61: { break; }
        // BIT 4,D: Test bit 4 of D
        case 0x62: { break; }
        // BIT 4,E: Test bit 4 of E
        case 0x63: { break; }
        // BIT 4,H: Test bit 4 of H
        case 0x64: { break; }
        // BIT 4,L: Test bit 4 of L
        case 0x65: { break; }
        // BIT 4,(HL): Test bit 4 of value pointed by HL
        case 0x66: { break; }
        // BIT 4,A: Test bit 4 of A
        case 0x67: { break; }
        // BIT 5,B: Test bit 5 of B
        case 0x68: { break; }
        // BIT 5,C: Test bit 5 of C
        case 0x69: { break; }
        // BIT 5,D: Test bit 5 of D
        case 0x6A: { break; }
        // BIT 5,E: Test bit 5 of E
        case 0x6B: { break; }
        // BIT 5,H: Test bit 5 of H
        case 0x6C: { break; }
        // BIT 5,L: Test bit 5 of L
        case 0x6D: { break; }
        // BIT 5,(HL): Test bit 5 of value pointed by HL
        case 0x6E: { break; }
        // BIT 5,A: Test bit 5 of A
        case 0x6F: { break; }
        // BIT 6,B: Test bit 6 of B
        case 0x70: { break; }
        // BIT 6,C: Test bit 6 of C
        case 0x71: { break; }
        // BIT 6,D: Test bit 6 of D
        case 0x72: { break; }
        // BIT 6,E: Test bit 6 of E
        case 0x73: { break; }
        // BIT 6,H: Test bit 6 of H
        case 0x74: { break; }
        // BIT 6,L: Test bit 6 of L
        case 0x75: { break; }
        // BIT 6,(HL): Test bit 6 of value pointed by HL
        case 0x76: { break; }
        // BIT 6,A: Test bit 6 of A
        case 0x77: { break; }
        // BIT 7,B: Test bit 7 of B
        case 0x78: { break; }
        // BIT 7,C: Test bit 7 of C
        case 0x79: { break; }
        // BIT 7,D: Test bit 7 of D
        case 0x7A: { break; }
        // BIT 7,E: Test bit 7 of E
        case 0x7B: { break; }
        // BIT 7,H: Test bit 7 of H
        case 0x7C: { break; }
        // BIT 7,L: Test bit 7 of L
        case 0x7D: { break; }
        // BIT 7,(HL): Test bit 7 of value pointed by HL
        case 0x7E: { break; }
        // BIT 7,A: Test bit 7 of A
        case 0x7F: { break; }
        // RES 0,B: Clear (reset) bit 0 of B
        case 0x80: { break; }
        // RES 0,C: Clear (reset) bit 0 of C
        case 0x81: { break; }
        // RES 0,D: Clear (reset) bit 0 of D
        case 0x82: { break; }
        // RES 0,E: Clear (reset) bit 0 of E
        case 0x83: { break; }
        // RES 0,H: Clear (reset) bit 0 of H
        case 0x84: { break; }
        // RES 0,L: Clear (reset) bit 0 of L
        case 0x85: { break; }
        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x86:
          {
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 0));
            break;
          }
        // RES 0,A: Clear (reset) bit 0 of A
        case 0x87: { break; }
        // RES 1,B: Clear (reset) bit 1 of B
        case 0x88: { break; }
        // RES 1,C: Clear (reset) bit 1 of C
        case 0x89: { break; }
        // RES 1,D: Clear (reset) bit 1 of D
        case 0x8A: { break; }
        // RES 1,E: Clear (reset) bit 1 of E
        case 0x8B: { break; }
        // RES 1,H: Clear (reset) bit 1 of H
        case 0x8C: { break; }
        // RES 1,L: Clear (reset) bit 1 of L
        case 0x8D: { break; }
        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x8E:
          {
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 1));
            break;
          }
        // RES 1,A: Clear (reset) bit 1 of A
        case 0x8F: { break; }
        // RES 2,B: Clear (reset) bit 2 of B
        case 0x90: { break; }
        // RES 2,C: Clear (reset) bit 2 of C
        case 0x91: { break; }
        // RES 2,D: Clear (reset) bit 2 of D
        case 0x92: { break; }
        // RES 2,E: Clear (reset) bit 2 of E
        case 0x93: { break; }
        // RES 2,H: Clear (reset) bit 2 of H
        case 0x94: { break; }
        // RES 2,L: Clear (reset) bit 2 of L
        case 0x95: { break; }
        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x96:
          {
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 2));
            break;
          }
        // RES 2,A: Clear (reset) bit 2 of A
        case 0x97: { break; }
        // RES 3,B: Clear (reset) bit 3 of B
        case 0x98: { break; }
        // RES 3,C: Clear (reset) bit 3 of C
        case 0x99: { break; }
        // RES 3,D: Clear (reset) bit 3 of D
        case 0x9A: { break; }
        // RES 3,E: Clear (reset) bit 3 of E
        case 0x9B: { break; }
        // RES 3,H: Clear (reset) bit 3 of H
        case 0x9C: { break; }
        // RES 3,L: Clear (reset) bit 3 of L
        case 0x9D: { break; }
        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        // NOTE: two-stage opcode
        case 0x9E: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 3));
            break; 
          }
        // RES 3,A: Clear (reset) bit 3 of A
        case 0x9F: { break; }
        // RES 4,B: Clear (reset) bit 4 of B
        case 0xA0: { break; }
        // RES 4,C: Clear (reset) bit 4 of C
        case 0xA1: { break; }
        // RES 4,D: Clear (reset) bit 4 of D
        case 0xA2: { break; }
        // RES 4,E: Clear (reset) bit 4 of E
        case 0xA3: { break; }
        // RES 4,H: Clear (reset) bit 4 of H
        case 0xA4: { break; }
        // RES 4,L: Clear (reset) bit 4 of L
        case 0xA5: { break; }
        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xA6: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 4));
            break; 
          }
        // RES 4,A: Clear (reset) bit 4 of A
        case 0xA7: { break; }
        // RES 5,B: Clear (reset) bit 5 of B
        case 0xA8: { break; }
        // RES 5,C: Clear (reset) bit 5 of C
        case 0xA9: { break; }
        // RES 5,D: Clear (reset) bit 5 of D
        case 0xAA: { break; }
        // RES 5,E: Clear (reset) bit 5 of E
        case 0xAB: { break; }
        // RES 5,H: Clear (reset) bit 5 of H
        case 0xAC: { break; }
        // RES 5,L: Clear (reset) bit 5 of L
        case 0xAD: { break; }
        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xAE: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 5));
            break; 
          }
        // RES 5,A: Clear (reset) bit 5 of A
        case 0xAF: { break; }
        // RES 6,B: Clear (reset) bit 6 of B
        case 0xB0: { break; }
        // RES 6,C: Clear (reset) bit 6 of C
        case 0xB1: { break; }
        // RES 6,D: Clear (reset) bit 6 of D
        case 0xB2: { break; }
        // RES 6,E: Clear (reset) bit 6 of E
        case 0xB3: { break; }
        // RES 6,H: Clear (reset) bit 6 of H
        case 0xB4: { break; }
        // RES 6,L: Clear (reset) bit 6 of L
        case 0xB5: { break; }
        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xB6: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 6));
            break; 
          }
        // RES 6,A: Clear (reset) bit 6 of A
        case 0xB7: { break; }
        // RES 7,B: Clear (reset) bit 7 of B
        case 0xB8: { break; }
        // RES 7,C: Clear (reset) bit 7 of C
        case 0xB9: { break; }
        // RES 7,D: Clear (reset) bit 7 of D
        case 0xBA: { break; }
        // RES 7,E: Clear (reset) bit 7 of E
        case 0xBB: { break; }
        // RES 7,H: Clear (reset) bit 7 of H
        case 0xBC: { break; }
        // RES 7,L: Clear (reset) bit 7 of L
        case 0xBD: { break; }
        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xBE: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.ClearBit(cpu.Registers.TEMP, 7));
            break; 
          }
        // RES 7,A: Clear (reset) bit 7 of A
        case 0xBF: { break; }
        // SET 0,B: Set bit 0 of B
        case 0xC0: { break; }
        // SET 0,C: Set bit 0 of C
        case 0xC1: { break; }
        // SET 0,D: Set bit 0 of D
        case 0xC2: { break; }
        // SET 0,E: Set bit 0 of E
        case 0xC3: { break; }
        // SET 0,H: Set bit 0 of H
        case 0xC4: { break; }
        // SET 0,L: Set bit 0 of L
        case 0xC5: { break; }
        // SET 0,(HL): Set bit 0 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xC6: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 0));
            break; 
          }
        // SET 0,A: Set bit 0 of A
        case 0xC7: { break; }
        // SET 1,B: Set bit 1 of B
        case 0xC8: { break; }
        // SET 1,C: Set bit 1 of C
        case 0xC9: { break; }
        // SET 1,D: Set bit 1 of D
        case 0xCA: { break; }
        // SET 1,E: Set bit 1 of E
        case 0xCB: { break; }
        // SET 1,H: Set bit 1 of H
        case 0xCC: { break; }
        // SET 1,L: Set bit 1 of L
        case 0xCD: { break; }
        // SET 1,(HL): Set bit 1 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xCE: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 1));
            break; 
          }
        // SET 1,A: Set bit 1 of A
        case 0xCF: { break; }
        // SET 2,B: Set bit 2 of B
        case 0xD0: { break; }
        // SET 2,C: Set bit 2 of C
        case 0xD1: { break; }
        // SET 2,D: Set bit 2 of D
        case 0xD2: { break; }
        // SET 2,E: Set bit 2 of E
        case 0xD3: { break; }
        // SET 2,H: Set bit 2 of H
        case 0xD4: { break; }
        // SET 2,L: Set bit 2 of L
        case 0xD5: { break; }
        // SET 2,(HL): Set bit 2 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xD6: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 2));
            break; 
          }
        // SET 2,A: Set bit 2 of A
        case 0xD7: { break; }
        // SET 3,B: Set bit 3 of B
        case 0xD8: { break; }
        // SET 3,C: Set bit 3 of C
        case 0xD9: { break; }
        // SET 3,D: Set bit 3 of D
        case 0xDA: { break; }
        // SET 3,E: Set bit 3 of E
        case 0xDB: { break; }
        // SET 3,H: Set bit 3 of H
        case 0xDC: { break; }
        // SET 3,L: Set bit 3 of L
        case 0xDD: { break; }
        // SET 3,(HL): Set bit 3 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xDE: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 3));
            break; 
          }
        // SET 3,A: Set bit 3 of A
        case 0xDF: { break; }
        // SET 4,B: Set bit 4 of B
        case 0xE0: { break; }
        // SET 4,C: Set bit 4 of C
        case 0xE1: { break; }
        // SET 4,D: Set bit 4 of D
        case 0xE2: { break; }
        // SET 4,E: Set bit 4 of E
        case 0xE3: { break; }
        // SET 4,H: Set bit 4 of H
        case 0xE4: { break; }
        // SET 4,L: Set bit 4 of L
        case 0xE5: { break; }
        // SET 4,(HL): Set bit 4 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xE6: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 4));
            break; 
          }
        // SET 4,A: Set bit 4 of A
        case 0xE7: { break; }
        // SET 5,B: Set bit 5 of B
        case 0xE8: { break; }
        // SET 5,C: Set bit 5 of C
        case 0xE9: { break; }
        // SET 5,D: Set bit 5 of D
        case 0xEA: { break; }
        // SET 5,E: Set bit 5 of E
        case 0xEB: { break; }
        // SET 5,H: Set bit 5 of H
        case 0xEC: { break; }
        // SET 5,L: Set bit 5 of L
        case 0xED: { break; }
        // SET 5,(HL): Set bit 5 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xEE: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 5));
            break; 
          }
        // SET 5,A: Set bit 5 of A
        case 0xEF: { break; }
        // SET 6,B: Set bit 6 of B
        case 0xF0: { break; }
        // SET 6,C: Set bit 6 of C
        case 0xF1: { break; }
        // SET 6,D: Set bit 6 of D
        case 0xF2: { break; }
        // SET 6,E: Set bit 6 of E
        case 0xF3: { break; }
        // SET 6,H: Set bit 6 of H
        case 0xF4: { break; }
        // SET 6,L: Set bit 6 of L
        case 0xF5: { break; }
        // SET 6,(HL): Set bit 6 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xF6: 
          { 
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 6));
            break; 
          }
        // SET 6,A: Set bit 6 of A
        case 0xF7: { break; }
        // SET 7,B: Set bit 7 of B
        case 0xF8: { break; }
        // SET 7,C: Set bit 7 of C
        case 0xF9: { break; }
        // SET 7,D: Set bit 7 of D
        case 0xFA: { break; }
        // SET 7,E: Set bit 7 of E
        case 0xFB: { break; }
        // SET 7,H: Set bit 7 of H
        case 0xFC: { break; }
        // SET 7,L: Set bit 7 of L
        case 0xFD: { break; }
        // SET 7,(HL): Set bit 7 of value pointed by HL
        // NOTE: two-stage opcode
        case 0xFE:
          {
            cpu._memory.Write(cpu.Registers.HL, UtilFuncs.SetBit(cpu.Registers.TEMP, 7));
            break;
          }
        // SET 7,A: Set bit 7 of A
        case 0xFF: { break; }
      }
    }
  }
}
