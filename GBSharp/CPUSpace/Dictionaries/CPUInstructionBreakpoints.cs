using GBSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUInstructionBreakpoints
  {
    /// <summary>
    /// Runs an normal opcode instruction
    /// </summary>
    /// <param name="opcode">The opcode to run</param>
    /// <param name="n">The argument (if any) of the opcode</param>
    /// <returns>Whether a breakpoint was found</returns>
    internal static BreakpointKinds Check(CPU cpu, byte opcode, ushort n, bool ignoreBreakpoints)
    {
      switch (opcode)
      {
        // NOP: No Operation
        case 0x00: { break; }
        // LD BC,nn: Load 16-bit immediate into BC
        case 0x01: { break; }
        // LD (BC),A: Save A to address pointed by BC
        case 0x02:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.BC))
            {
              return BreakpointKinds.WRITE;
            }
 
            break;
          }
        // INC BC: Increment 16-bit BC
        case 0x03: { break; }
        // INC B: Increment B
        case 0x04: { break; }
        // DEC B: Decrement B
        case 0x05: { break; }
        // LD B,n: Load 8-bit immediate into B
        case 0x06: { break; }
        // RLC A: Rotate A left with carry
        case 0x07: { break; }
        // LD (nn),SP: Save SP to given address
        case 0x08:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(n))
            {
              return BreakpointKinds.WRITE;
            }
 
            break;
          }
        // ADD HL,BC: Add 16-bit BC to HL
        case 0x09: { break; }
        // LD A,(BC): Load A from address pointed to by BC
        case 0x0A:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.BC))
            {
              return BreakpointKinds.READ;
            }
            
            break;
          }
        // DEC BC: Decrement 16-bit BC
        case 0x0B: { break; }
        // INC C: Increment C
        case 0x0C: { break; }
        // DEC C: Decrement C
        case 0x0D: { break; }
        // LD C,n: Load 8-bit immediate into C
        case 0x0E: { break; }
        // RRC A: Rotate A right with carry
        case 0x0F: { break; }
        // STOP: Stop processor
        case 0x10: { break; }
        // LD DE,nn: Load 16-bit immediate into DE
        case 0x11: { break; }
        // LD (DE),A: Save A to address pointed by DE
        case 0x12:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.DE))
            {
              return BreakpointKinds.WRITE;
            }
 
            break;
          }
        // INC DE: Increment 16-bit DE
        case 0x13: { break; }
        // INC D: Increment D
        case 0x14: { break; }
        // DEC D: Decrement D
        case 0x15: { break; }
        // LD D,n: Load 8-bit immediate into D
        case 0x16: { break; }
        // RL A: Rotate A left
        case 0x17: { break; }
        // JR n: Relative jump by signed immediate
        case 0x18:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.nextPC + sn);
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
 
            break;
          }
        // ADD HL,DE: Add 16-bit DE to HL
        case 0x19: { break; }
        // LD A,(DE): Load A from address pointed to by DE
        case 0x1A:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.DE))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // DEC DE: Decrement 16-bit DE
        case 0x1B: { break; }
        // INC E: Increment E
        case 0x1C: { break; }
        // DEC E: Decrement E
        case 0x1D: { break; }
        // LD E,n: Load 8-bit immediate into E
        case 0x1E: { break; }
        // RR A: Rotate A right
        case 0x1F: { break; }
        // JR NZ,n: Relative jump by signed immediate if last result was not zero
        case 0x20:
          {
            if (cpu.registers.FZ != 0) { return BreakpointKinds.NONE; }

            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.nextPC + sn);
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }

            break;
          }
        // LD HL,nn: Load 16-bit immediate into HL
        case 0x21: { break; }
        // LDI (HL),A: Save A to address pointed by HL, and increment HL
        case 0x22:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // INC HL: Increment 16-bit HL
        case 0x23: { break; }
        // INC H: Increment H
        case 0x24: { break; }
        // DEC H: Decrement H
        case 0x25: { break; }
        // LD H,n: Load 8-bit immediate into H
        case 0x26: { break; }
        // DAA: Adjust A for BCD addition
        case 0x27: { break; }
        // JR Z,n: Relative jump by signed immediate if last result was zero
        case 0x28:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.nextPC + sn);
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // ADD HL,HL: Add 16-bit HL to HL
        case 0x29: { break; }
        // LDI A,(HL): Load A from address pointed to by HL, and increment HL
        case 0x2A:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // DEC HL: Decrement 16-bit HL
        case 0x2B: { break; }
        // INC L: Increment L
        case 0x2C: { break; }
        // DEC L: Decrement L
        case 0x2D: { break; }
        // LD L,n: Load 8-bit immediate into L
        case 0x2E: { break; }
        // CPL: Complement (logical NOT) on A
        case 0x2F: { break; }
        // JR NC,n: Relative jump by signed immediate if last result caused no carry
        case 0x30:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.nextPC + sn);
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // LD SP,nn: Load 16-bit immediate into SP
        case 0x31: { break; }
        // LDD (HL),A: Save A to address pointed by HL, and decrement HL
        case 0x32:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // INC SP: Increment 16-bit HL
        case 0x33: { break; }
        // INC (HL): Increment value pointed by HL
        case 0x34:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // DEC (HL): Decrement value pointed by HL
        case 0x35:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD (HL),n: Load 8-bit immediate into address pointed by HL
        case 0x36:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // SCF: Set carry flag
        case 0x37: { break; }
        // JR C,n: Relative jump by signed immediate if last result caused carry
        case 0x38:
          {
            // We cast down the input, ignoring the overflows
            short sn = 0;
            unchecked { sn = (sbyte)n; }
            ushort target = (ushort)(cpu.nextPC + sn);
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // ADD HL,SP: Add 16-bit SP to HL
        case 0x39: { break; }
        // LDD A,(HL): Load A from address pointed to by HL, and decrement HL
        case 0x3A:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // DEC SP: Decrement 16-bit SP
        case 0x3B: { break; }
        // INC A: Increment A
        case 0x3C: { break; }
        // DEC A: Decrement A
        case 0x3D: { break; }
        // LD A,n: Load 8-bit immediate into A
        case 0x3E: { break; }
        // CCF: Complement Carry Flag
        case 0x3F: { break; }
        // LD B,B: Copy B to B
        case 0x40: { break; }
        // LD B,C: Copy C to B
        case 0x41: { break; }
        // LD B,D: Copy D to B
        case 0x42: { break; }
        // LD B,E: Copy E to B
        case 0x43: { break; }
        // LD B,H: Copy H to B
        case 0x44: { break; }
        // LD B,L: Copy L to B
        case 0x45: { break; }
        // LD B,(HL): Copy value pointed by HL to B
        case 0x46:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD B,A: Copy A to B
        case 0x47: { break; }
        // LD C,B: Copy B to C
        case 0x48: { break; }
        // LD C,C: Copy C to C
        case 0x49: { break; }
        // LD C,D: Copy D to C
        case 0x4A: { break; }
        // LD C,E: Copy E to C
        case 0x4B: { break; }
        // LD C,H: Copy H to C
        case 0x4C: { break; }
        // LD C,L: Copy L to C
        case 0x4D: { break; }
        // LD C,(HL): Copy value pointed by HL to C
        case 0x4E:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD C,A: Copy A to C
        case 0x4F: { break; }
        // LD D,B: Copy B to D
        case 0x50: { break; }
        // LD D,C: Copy C to D
        case 0x51: { break; }
        // LD D,D: Copy D to D
        case 0x52: { break; }
        // LD D,E: Copy E to D
        case 0x53: { break; }
        // LD D,H: Copy H to D
        case 0x54: { break; }
        // LD D,L: Copy L to D
        case 0x55: { break; }
        // LD D,(HL): Copy value pointed by HL to D
        case 0x56:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD D,A: Copy A to D
        case 0x57: { break; }
        // LD E,B: Copy B to E
        case 0x58: { break; }
        // LD E,C: Copy C to E
        case 0x59: { break; }
        // LD E,D: Copy D to E
        case 0x5A: { break; }
        // LD E,E: Copy E to E
        case 0x5B: { break; }
        // LD E,H: Copy H to E
        case 0x5C: { break; }
        // LD E,L: Copy L to E
        case 0x5D: { break; }
        // LD E,(HL): Copy value pointed by HL to E
        case 0x5E:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD E,A: Copy A to E
        case 0x5F: { break; }
        // LD H,B: Copy B to H
        case 0x60: { break; }
        // LD H,C: Copy C to H
        case 0x61: { break; }
        // LD H,D: Copy D to H
        case 0x62: { break; }
        // LD H,E: Copy E to H
        case 0x63: { break; }
        // LD H,H: Copy H to H
        case 0x64: { break; }
        // LD H,L: Copy L to H
        case 0x65: { break; }
        // LD H,(HL): Copy value pointed by HL to H
        case 0x66:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD H,A: Copy A to H
        case 0x67: { break; }
        // LD L,B: Copy B to L
        case 0x68: { break; }
        // LD L,C: Copy C to L
        case 0x69: { break; }
        // LD L,D: Copy D to L
        case 0x6A: { break; }
        // LD L,E: Copy E to L
        case 0x6B: { break; }
        // LD L,H: Copy H to L
        case 0x6C: { break; }
        // LD L,L: Copy L to L
        case 0x6D: { break; }
        // LD L,(HL): Copy value pointed by HL to L
        case 0x6E:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD L,A: Copy A to L
        case 0x6F: { break; }
        // LD (HL),B: Copy B to address pointed by HL
        case 0x70:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD (HL),C: Copy C to address pointed by HL
        case 0x71:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD (HL),D: Copy D to address pointed by HL
        case 0x72:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD (HL),E: Copy E to address pointed by HL
        case 0x73:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD (HL),H: Copy H to address pointed by HL
        case 0x74:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD (HL),L: Copy L to address pointed by HL
        case 0x75:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // HALT: Halt processor
        case 0x76: { break; }
        // LD (HL),A: Copy A to address pointed by HL
        case 0x77:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // LD A,B: Copy B to A
        case 0x78: { break; }
        // LD A,C: Copy C to A
        case 0x79: { break; }
        // LD A,D: Copy D to A
        case 0x7A: { break; }
        // LD A,E: Copy E to A
        case 0x7B: { break; }
        // LD A,H: Copy H to A
        case 0x7C: { break; }
        // LD A,L: Copy L to A
        case 0x7D: { break; }
        // LD A,(HL): Copy value pointed by HL to A
        case 0x7E:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // LD A,A: Copy A to A
        case 0x7F: { break; }
        // ADD A,B: Add B to A
        case 0x80: { break; }
        // ADD A,C: Add C to A
        case 0x81: { break; }
        // ADD A,D: Add D to A
        case 0x82: { break; }
        // ADD A,E: Add E to A
        case 0x83: { break; }
        // ADD A,H: Add H to A
        case 0x84: { break; }
        // ADD A,L: Add L to A
        case 0x85: { break; }
        // ADD A,(HL): Add value pointed by HL to A
        case 0x86: { break; }
        // ADD A,A: Add A to A
        case 0x87: { break; }
        // ADC A,B: Add B and carry flag to A
        case 0x88: { break; }
        // ADC A,C: Add C and carry flag to A
        case 0x89: { break; }
        // ADC A,D: Add D and carry flag to A
        case 0x8A: { break; }
        // ADC A,E: Add E and carry flag to A
        case 0x8B: { break; }
        // ADC A,H: Add H and carry flag to A
        case 0x8C: { break; }
        // ADC A,L: Add and carry flag L to A
        case 0x8D: { break; }
        // ADC A,(HL): Add value pointed by HL and carry flag to A
        case 0x8E: { break; }
        // ADC A,A: Add A and carry flag to A
        case 0x8F: { break; }
        // SUB A,B: Subtract B from A
        case 0x90: { break; }
        // SUB A,C: Subtract C from A
        case 0x91: { break; }
        // SUB A,D: Subtract D from A
        case 0x92: { break; }
        // SUB A,E: Subtract E from A
        case 0x93: { break; }
        // SUB A,H: Subtract H from A
        case 0x94: { break; }
        // SUB A,L: Subtract L from A
        case 0x95: { break; }
        // SUB A,(HL): Subtract value pointed by HL from A
        case 0x96: { break; }
        // SUB A,A: Subtract A from A
        case 0x97: { break; }
        // SBC A,B: Subtract B and carry flag from A
        case 0x98: { break; }
        // SBC A,C: Subtract C and carry flag from A
        case 0x99: { break; }
        // SBC A,D: Subtract D and carry flag from A
        case 0x9A: { break; }
        // SBC A,E: Subtract E and carry flag from A
        case 0x9B: { break; }
        // SBC A,H: Subtract H and carry flag from A
        case 0x9C: { break; }
        // SBC A,L: Subtract and carry flag L from A
        case 0x9D: { break; }
        // SBC A,(HL): Subtract value pointed by HL and carry flag from A
        case 0x9E: { break; }
        // SBC A,A: Subtract A and carry flag from A
        case 0x9F: { break; }
        // AND B: Logical AND B against A
        case 0xA0: { break; }
        // AND C: Logical AND C against A
        case 0xA1: { break; }
        // AND D: Logical AND D against A
        case 0xA2: { break; }
        // AND E: Logical AND E against A
        case 0xA3: { break; }
        // AND H: Logical AND H against A
        case 0xA4: { break; }
        // AND L: Logical AND L against A
        case 0xA5: { break; }
        // AND (HL): Logical AND value pointed by HL against A
        case 0xA6: { break; }
        // AND A: Logical AND A against A
        case 0xA7: { break; }
        // XOR B: Logical XOR B against A
        case 0xA8: { break; }
        // XOR C: Logical XOR C against A
        case 0xA9: { break; }
        // XOR D: Logical XOR D against A
        case 0xAA: { break; }
        // XOR E: Logical XOR E against A
        case 0xAB: { break; }
        // XOR H: Logical XOR H against A
        case 0xAC: { break; }
        // XOR L: Logical XOR L against A
        case 0xAD: { break; }
        // XOR (HL): Logical XOR value pointed by HL against A
        case 0xAE: { break; }
        // XOR A: Logical XOR A against A
        case 0xAF: { break; }
        // OR B: Logical OR B against A
        case 0xB0: { break; }
        // OR C: Logical OR C against A
        case 0xB1: { break; }
        // OR D: Logical OR D against A
        case 0xB2: { break; }
        // OR E: Logical OR E against A
        case 0xB3: { break; }
        // OR H: Logical OR H against A
        case 0xB4: { break; }
        // OR L: Logical OR L against A
        case 0xB5: { break; }
        // OR (HL): Logical OR value pointed by HL against A
        case 0xB6: { break; }
        // OR A: Logical OR A against A
        case 0xB7: { break; }
        // CP B: Compare B against A
        case 0xB8: { break; }
        // CP C: Compare C against A
        case 0xB9: { break; }
        // CP D: Compare D against A
        case 0xBA: { break; }
        // CP E: Compare E against A
        case 0xBB: { break; }
        // CP H: Compare H against A
        case 0xBC: { break; }
        // CP L: Compare L against A
        case 0xBD: { break; }
        // CP (HL): Compare value pointed by HL against A
        case 0xBE:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(cpu.registers.HL))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // CP A: Compare A against A
        case 0xBF: { break; }
        // RET NZ: Return if last result was not zero
        case 0xC0: { break; }
        // POP BC: Pop 16-bit value from stack into BC
        case 0xC1: { break; }
        // JP NZ,nn: Absolute jump to 16-bit location if last result was not zero
        case 0xC2:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // JP nn: Absolute jump to 16-bit location
        case 0xC3:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // CALL NZ,nn: Call routine at 16-bit location if last result was not zero
        case 0xC4:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // PUSH BC: Push 16-bit BC onto stack
        case 0xC5: { break; }
        // ADD A,n: Add 8-bit immediate to A
        case 0xC6: { break; }
        // RST 0: Call routine at address 0000h
        case 0xC7:
          {
            return Check(cpu, 0xCD, 0, ignoreBreakpoints);
          }
        // RET Z: Return if last result was zero
        case 0xC8: { break; }
        // RET: Return to calling routine
        case 0xC9: { break; }
        // JP Z,nn: Absolute jump to 16-bit location if last result was zero
        case 0xCA:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
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
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // CALL nn: Call routine at 16-bit location
        case 0xCD:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // ADC A,n: Add 8-bit immediate and carry to A
        case 0xCE: { break; }
        // RST 8: Call routine at address 0008h
        case 0xCF:
          {
            return Check(cpu, 0xCD, 0x08, ignoreBreakpoints);
          }
        // RET NC: Return if last result caused no carry
        case 0xD0: { break; }
        // POP DE: Pop 16-bit value from stack into DE
        case 0xD1: { break; }
        // JP NC,nn: Absolute jump to 16-bit location if last result caused no carry
        case 0xD2:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
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
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // PUSH DE: Push 16-bit DE onto stack
        case 0xD5: { break; }
        // SUB A,n: Subtract 8-bit immediate from A
        case 0xD6: { break; }
        // RST 10: Call routine at address 0010h
        case 0xD7:
          {
            return Check(cpu, 0xCD, 0x10, ignoreBreakpoints);
          }
        // RET C: Return if last result caused carry
        case 0xD8: { break; }
        // RETI: Enable interrupts and return to calling routine
        case 0xD9: { break; }
        // JP C,nn: Absolute jump to 16-bit location if last result caused carry
        case 0xDA:
          {
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
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
            ushort target = n;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // XX: Operation removed in this CPU
        case 0xDD:
          {
            throw new InvalidInstructionException("XX (0xDD)");
          }
        // SBC A,n: Subtract 8-bit immediate and carry from A
        case 0xDE: { break; }
        // RST 18: Call routine at address 0018h
        case 0xDF:
          {
            return Check(cpu, 0xCD, 0x18, ignoreBreakpoints);
          }
        // LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)
        case 0xE0:
          {
            ushort address = (ushort)(0xFF00 | (byte)n);
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(address))
            {
              return BreakpointKinds.WRITE;
            }
            break;
          }
        // POP HL: Pop 16-bit value from stack into HL
        case 0xE1: { break; }
        // LDH (C),A: Save A at address pointed to by (FF00h + C)
        case 0xE2:
          {
            ushort address = (ushort)(0xFF00 | cpu.registers.C);
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(address))
            {
              return BreakpointKinds.WRITE;
            }
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
        case 0xE5: { break; }
        // AND n: Logical AND 8-bit immediate against A
        case 0xE6: { break; }
        // RST 20: Call routine at address 0020h
        case 0xE7:
          {
            return Check(cpu, 0xCD, 0x20, ignoreBreakpoints);
          }
        // ADD SP,d: Add signed 8-bit immediate to SP
        case 0xE8: { break; }
        // JP (HL): Jump to 16-bit value pointed by HL
        case 0xE9:
          {
            ushort target = cpu.registers.HL;
            if (!ignoreBreakpoints && cpu.JumpBreakpoints.Contains(target))
            {
              return BreakpointKinds.JUMP;
            }
            break;
          }
        // LD (nn),A: Save A at given 16-bit address
        case 0xEA:
          {
            if (!ignoreBreakpoints && cpu.WriteBreakpoints.Contains(n))
            {
              return BreakpointKinds.WRITE;
            }
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
        case 0xEE: { break; }
        // RST 28: Call routine at address 0028h
        case 0xEF:
          {
            return Check(cpu, 0xCD, 0x28, ignoreBreakpoints);
          }
        // LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)
        case 0xF0:
          {
            ushort address = (ushort)(0xFF00 | (byte)n);
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(address))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // POP AF: Pop 16-bit value from stack into AF
        case 0xF1: { break; }
        // LDH A, (C): Operation removed in this CPU? (Or Load into A memory from FF00 + C?)
        case 0xF2:
          {
            ushort address = (ushort)(0xFF00 | cpu.registers.C);
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(address))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // DI: DIsable interrupts
        case 0xF3: { break; }
        // XX: Operation removed in this CPU
        case 0xF4:
          {
            throw new InvalidInstructionException("XX (0xF4)");
          }
        // PUSH AF: Push 16-bit AF onto stack
        case 0xF5: { break; }
        // OR n: Logical OR 8-bit immediate against A
        case 0xF6: { break; }
        // RST 30: Call routine at address 0030h
        case 0xF7:
          {
            return Check(cpu, 0xCD, 0x30, ignoreBreakpoints);
          }
        // LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL
        case 0xF8: { break; }
        // LD SP,HL: Copy HL to SP
        case 0xF9: { break; }
        // LD A,(nn): Load A from given 16-bit address
        case 0xFA:
          {
            if (!ignoreBreakpoints && cpu.ReadBreakpoints.Contains(n))
            {
              return BreakpointKinds.READ;
            }
            break;
          }
        // EI: Enable interrupts
        case 0xFB: { break; }
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
        case 0xFE: { break; }
        // RST 38: Call routine at address 0038h
        case 0xFF:
          {
            return Check(cpu, 0xCD, 0x38, ignoreBreakpoints);
          }
      }

      // No breakpoints found
      return BreakpointKinds.NONE;
    }
  }
}
