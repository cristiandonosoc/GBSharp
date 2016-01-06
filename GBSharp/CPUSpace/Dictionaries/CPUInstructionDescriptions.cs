using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  public class CPUInstructionDescriptions
  {
    public static string Get(byte opcode)
    {
      string result;
      switch(opcode)
      {
        case 0x00:  
          result = "NOP: No Operation";
          break;
        case 0x01:  
          result = "LD BC,nn: Load 16-bit immediate into BC";
          break;
        case 0x02:  
          result = "LD (BC),A: Save A to address pointed by BC";
          break;
        case 0x03:  
          result = "INC BC: Increment 16-bit BC";
          break;
        case 0x04:  
          result = "INC B: Increment B";
          break;
        case 0x05:  
          result = "DEC B: Decrement B";
          break;
        case 0x06:  
          result = "LD B,n: Load 8-bit immediate into B";
          break;
        case 0x07:  
          result = "RLC A: Rotate A left with carry";
          break;
        case 0x08:  
          result = "LD (nn),SP: Save SP to given address";
          break;
        case 0x09:  
          result = "ADD HL,BC: Add 16-bit BC to HL";
          break;
        case 0x0A:  
          result = "LD A,(BC): Load A from address pointed to by BC";
          break;
        case 0x0B:  
          result = "DEC BC: Decrement 16-bit BC";
          break;
        case 0x0C:  
          result = "INC C: Increment C";
          break;
        case 0x0D:  
          result = "DEC C: Decrement C";
          break;
        case 0x0E:  
          result = "LD C,n: Load 8-bit immediate into C";
          break;
        case 0x0F:  
          result = "RRC A: Rotate A right with carry";
          break;
        case 0x10:  
          result = "STOP: Stop processor";
          break;
        case 0x11:  
          result = "LD DE,nn: Load 16-bit immediate into DE";
          break;
        case 0x12:  
          result = "LD (DE),A: Save A to address pointed by DE";
          break;
        case 0x13:  
          result = "INC DE: Increment 16-bit DE";
          break;
        case 0x14:  
          result = "INC D: Increment D";
          break;
        case 0x15:  
          result = "DEC D: Decrement D";
          break;
        case 0x16:  
          result = "LD D,n: Load 8-bit immediate into D";
          break;
        case 0x17:  
          result = "RL A: Rotate A left";
          break;
        case 0x18:  
          result = "JR n: Relative jump by signed immediate";
          break;
        case 0x19:  
          result = "ADD HL,DE: Add 16-bit DE to HL";
          break;
        case 0x1A:  
          result = "LD A,(DE): Load A from address pointed to by DE";
          break;
        case 0x1B:  
          result = "DEC DE: Decrement 16-bit DE";
          break;
        case 0x1C:  
          result = "INC E: Increment E";
          break;
        case 0x1D:  
          result = "DEC E: Decrement E";
          break;
        case 0x1E:  
          result = "LD E,n: Load 8-bit immediate into E";
          break;
        case 0x1F:  
          result = "RR A: Rotate A right";
          break;
        case 0x20:  
          result = "JR NZ,n: Relative jump by signed immediate if last result was not zero";
          break;
        case 0x21:  
          result = "LD HL,nn: Load 16-bit immediate into HL";
          break;
        case 0x22:  
          result = "LDI (HL),A: Save A to address pointed by HL, and increment HL";
          break;
        case 0x23:  
          result = "INC HL: Increment 16-bit HL";
          break;
        case 0x24:  
          result = "INC H: Increment H";
          break;
        case 0x25:  
          result = "DEC H: Decrement H";
          break;
        case 0x26:  
          result = "LD H,n: Load 8-bit immediate into H";
          break;
        case 0x27:  
          result = "DAA: Adjust A for BCD addition";
          break;
        case 0x28:  
          result = "JR Z,n: Relative jump by signed immediate if last result was zero";
          break;
        case 0x29:  
          result = "ADD HL,HL: Add 16-bit HL to HL";
          break;
        case 0x2A:  
          result = "LDI A,(HL): Load A from address pointed to by HL, and increment HL";
          break;
        case 0x2B:  
          result = "DEC HL: Decrement 16-bit HL";
          break;
        case 0x2C:  
          result = "INC L: Increment L";
          break;
        case 0x2D:  
          result = "DEC L: Decrement L";
          break;
        case 0x2E:  
          result = "LD L,n: Load 8-bit immediate into L";
          break;
        case 0x2F:  
          result = "CPL: Complement (logical NOT) on A";
          break;
        case 0x30:  
          result = "JR NC,n: Relative jump by signed immediate if last result caused no carry";
          break;
        case 0x31:  
          result = "LD SP,nn: Load 16-bit immediate into SP";
          break;
        case 0x32:  
          result = "LDD (HL),A: Save A to address pointed by HL, and decrement HL";
          break;
        case 0x33:  
          result = "INC SP: Increment 16-bit HL";
          break;
        case 0x34:  
          result = "INC (HL): Increment value pointed by HL";
          break;
        case 0x35:  
          result = "DEC (HL): Decrement value pointed by HL";
          break;
        case 0x36:  
          result = "LD (HL),n: Load 8-bit immediate into address pointed by HL";
          break;
        case 0x37:  
          result = "SCF: Set carry flag";
          break;
        case 0x38:  
          result = "JR C,n: Relative jump by signed immediate if last result caused carry";
          break;
        case 0x39:  
          result = "ADD HL,SP: Add 16-bit SP to HL";
          break;
        case 0x3A:  
          result = "LDD A,(HL): Load A from address pointed to by HL, and decrement HL";
          break;
        case 0x3B:  
          result = "DEC SP: Decrement 16-bit SP";
          break;
        case 0x3C:  
          result = "INC A: Increment A";
          break;
        case 0x3D:  
          result = "DEC A: Decrement A";
          break;
        case 0x3E:  
          result = "LD A,n: Load 8-bit immediate into A";
          break;
        case 0x3F:  
          result = "CCF: Complement Carry Flag";
          break;
        case 0x40:  
          result = "LD B,B: Copy B to B";
          break;
        case 0x41:  
          result = "LD B,C: Copy C to B";
          break;
        case 0x42:  
          result = "LD B,D: Copy D to B";
          break;
        case 0x43:  
          result = "LD B,E: Copy E to B";
          break;
        case 0x44:  
          result = "LD B,H: Copy H to B";
          break;
        case 0x45:  
          result = "LD B,L: Copy L to B";
          break;
        case 0x46:  
          result = "LD B,(HL): Copy value pointed by HL to B";
          break;
        case 0x47:  
          result = "LD B,A: Copy A to B";
          break;
        case 0x48:  
          result = "LD C,B: Copy B to C";
          break;
        case 0x49:  
          result = "LD C,C: Copy C to C";
          break;
        case 0x4A:  
          result = "LD C,D: Copy D to C";
          break;
        case 0x4B:  
          result = "LD C,E: Copy E to C";
          break;
        case 0x4C:  
          result = "LD C,H: Copy H to C";
          break;
        case 0x4D:  
          result = "LD C,L: Copy L to C";
          break;
        case 0x4E:  
          result = "LD C,(HL): Copy value pointed by HL to C";
          break;
        case 0x4F:  
          result = "LD C,A: Copy A to C";
          break;
        case 0x50:  
          result = "LD D,B: Copy B to D";
          break;
        case 0x51:  
          result = "LD D,C: Copy C to D";
          break;
        case 0x52:  
          result = "LD D,D: Copy D to D";
          break;
        case 0x53:  
          result = "LD D,E: Copy E to D";
          break;
        case 0x54:  
          result = "LD D,H: Copy H to D";
          break;
        case 0x55:  
          result = "LD D,L: Copy L to D";
          break;
        case 0x56:  
          result = "LD D,(HL): Copy value pointed by HL to D";
          break;
        case 0x57:  
          result = "LD D,A: Copy A to D";
          break;
        case 0x58:  
          result = "LD E,B: Copy B to E";
          break;
        case 0x59:  
          result = "LD E,C: Copy C to E";
          break;
        case 0x5A:  
          result = "LD E,D: Copy D to E";
          break;
        case 0x5B:  
          result = "LD E,E: Copy E to E";
          break;
        case 0x5C:  
          result = "LD E,H: Copy H to E";
          break;
        case 0x5D:  
          result = "LD E,L: Copy L to E";
          break;
        case 0x5E:  
          result = "LD E,(HL): Copy value pointed by HL to E";
          break;
        case 0x5F:  
          result = "LD E,A: Copy A to E";
          break;
        case 0x60:  
          result = "LD H,B: Copy B to H";
          break;
        case 0x61:  
          result = "LD H,C: Copy C to H";
          break;
        case 0x62:  
          result = "LD H,D: Copy D to H";
          break;
        case 0x63:  
          result = "LD H,E: Copy E to H";
          break;
        case 0x64:  
          result = "LD H,H: Copy H to H";
          break;
        case 0x65:  
          result = "LD H,L: Copy L to H";
          break;
        case 0x66:  
          result = "LD H,(HL): Copy value pointed by HL to H";
          break;
        case 0x67:  
          result = "LD H,A: Copy A to H";
          break;
        case 0x68:  
          result = "LD L,B: Copy B to L";
          break;
        case 0x69:  
          result = "LD L,C: Copy C to L";
          break;
        case 0x6A:  
          result = "LD L,D: Copy D to L";
          break;
        case 0x6B:  
          result = "LD L,E: Copy E to L";
          break;
        case 0x6C:  
          result = "LD L,H: Copy H to L";
          break;
        case 0x6D:  
          result = "LD L,L: Copy L to L";
          break;
        case 0x6E:  
          result = "LD L,(HL): Copy value pointed by HL to L";
          break;
        case 0x6F:  
          result = "LD L,A: Copy A to L";
          break;
        case 0x70:  
          result = "LD (HL),B: Copy B to address pointed by HL";
          break;
        case 0x71:  
          result = "LD (HL),C: Copy C to address pointed by HL";
          break;
        case 0x72:  
          result = "LD (HL),D: Copy D to address pointed by HL";
          break;
        case 0x73:  
          result = "LD (HL),E: Copy E to address pointed by HL";
          break;
        case 0x74:  
          result = "LD (HL),H: Copy H to address pointed by HL";
          break;
        case 0x75:  
          result = "LD (HL),L: Copy L to address pointed by HL";
          break;
        case 0x76:  
          result = "HALT: Halt processor";
          break;
        case 0x77:  
          result = "LD (HL),A: Copy A to address pointed by HL";
          break;
        case 0x78:  
          result = "LD A,B: Copy B to A";
          break;
        case 0x79:  
          result = "LD A,C: Copy C to A";
          break;
        case 0x7A:  
          result = "LD A,D: Copy D to A";
          break;
        case 0x7B:  
          result = "LD A,E: Copy E to A";
          break;
        case 0x7C:  
          result = "LD A,H: Copy H to A";
          break;
        case 0x7D:  
          result = "LD A,L: Copy L to A";
          break;
        case 0x7E:  
          result = "LD A,(HL): Copy value pointed by HL to A";
          break;
        case 0x7F:  
          result = "LD A,A: Copy A to A";
          break;
        case 0x80:  
          result = "ADD A,B: Add B to A";
          break;
        case 0x81:  
          result = "ADD A,C: Add C to A";
          break;
        case 0x82:  
          result = "ADD A,D: Add D to A";
          break;
        case 0x83:  
          result = "ADD A,E: Add E to A";
          break;
        case 0x84:  
          result = "ADD A,H: Add H to A";
          break;
        case 0x85:  
          result = "ADD A,L: Add L to A";
          break;
        case 0x86:  
          result = "ADD A,(HL): Add value pointed by HL to A";
          break;
        case 0x87:  
          result = "ADD A,A: Add A to A";
          break;
        case 0x88:  
          result = "ADC A,B: Add B and carry flag to A";
          break;
        case 0x89:  
          result = "ADC A,C: Add C and carry flag to A";
          break;
        case 0x8A:  
          result = "ADC A,D: Add D and carry flag to A";
          break;
        case 0x8B:  
          result = "ADC A,E: Add E and carry flag to A";
          break;
        case 0x8C:  
          result = "ADC A,H: Add H and carry flag to A";
          break;
        case 0x8D:  
          result = "ADC A,L: Add and carry flag L to A";
          break;
        case 0x8E:  
          result = "ADC A,(HL): Add value pointed by HL and carry flag to A";
          break;
        case 0x8F:  
          result = "ADC A,A: Add A and carry flag to A";
          break;
        case 0x90:  
          result = "SUB A,B: Subtract B from A";
          break;
        case 0x91:  
          result = "SUB A,C: Subtract C from A";
          break;
        case 0x92:  
          result = "SUB A,D: Subtract D from A";
          break;
        case 0x93:  
          result = "SUB A,E: Subtract E from A";
          break;
        case 0x94:  
          result = "SUB A,H: Subtract H from A";
          break;
        case 0x95:  
          result = "SUB A,L: Subtract L from A";
          break;
        case 0x96:  
          result = "SUB A,(HL): Subtract value pointed by HL from A";
          break;
        case 0x97:  
          result = "SUB A,A: Subtract A from A";
          break;
        case 0x98:  
          result = "SBC A,B: Subtract B and carry flag from A";
          break;
        case 0x99:  
          result = "SBC A,C: Subtract C and carry flag from A";
          break;
        case 0x9A:  
          result = "SBC A,D: Subtract D and carry flag from A";
          break;
        case 0x9B:  
          result = "SBC A,E: Subtract E and carry flag from A";
          break;
        case 0x9C:  
          result = "SBC A,H: Subtract H and carry flag from A";
          break;
        case 0x9D:  
          result = "SBC A,L: Subtract and carry flag L from A";
          break;
        case 0x9E:  
          result = "SBC A,(HL): Subtract value pointed by HL and carry flag from A";
          break;
        case 0x9F:  
          result = "SBC A,A: Subtract A and carry flag from A";
          break;
        case 0xA0:  
          result = "AND B: Logical AND B against A";
          break;
        case 0xA1:  
          result = "AND C: Logical AND C against A";
          break;
        case 0xA2:  
          result = "AND D: Logical AND D against A";
          break;
        case 0xA3:  
          result = "AND E: Logical AND E against A";
          break;
        case 0xA4:  
          result = "AND H: Logical AND H against A";
          break;
        case 0xA5:  
          result = "AND L: Logical AND L against A";
          break;
        case 0xA6:  
          result = "AND (HL): Logical AND value pointed by HL against A";
          break;
        case 0xA7:  
          result = "AND A: Logical AND A against A";
          break;
        case 0xA8:  
          result = "XOR B: Logical XOR B against A";
          break;
        case 0xA9:  
          result = "XOR C: Logical XOR C against A";
          break;
        case 0xAA:  
          result = "XOR D: Logical XOR D against A";
          break;
        case 0xAB:  
          result = "XOR E: Logical XOR E against A";
          break;
        case 0xAC:  
          result = "XOR H: Logical XOR H against A";
          break;
        case 0xAD:  
          result = "XOR L: Logical XOR L against A";
          break;
        case 0xAE:  
          result = "XOR (HL): Logical XOR value pointed by HL against A";
          break;
        case 0xAF:  
          result = "XOR A: Logical XOR A against A";
          break;
        case 0xB0:  
          result = "OR B: Logical OR B against A";
          break;
        case 0xB1:  
          result = "OR C: Logical OR C against A";
          break;
        case 0xB2:  
          result = "OR D: Logical OR D against A";
          break;
        case 0xB3:  
          result = "OR E: Logical OR E against A";
          break;
        case 0xB4:  
          result = "OR H: Logical OR H against A";
          break;
        case 0xB5:  
          result = "OR L: Logical OR L against A";
          break;
        case 0xB6:  
          result = "OR (HL): Logical OR value pointed by HL against A";
          break;
        case 0xB7:  
          result = "OR A: Logical OR A against A";
          break;
        case 0xB8:  
          result = "CP B: Compare B against A";
          break;
        case 0xB9:  
          result = "CP C: Compare C against A";
          break;
        case 0xBA:  
          result = "CP D: Compare D against A";
          break;
        case 0xBB:  
          result = "CP E: Compare E against A";
          break;
        case 0xBC:  
          result = "CP H: Compare H against A";
          break;
        case 0xBD:  
          result = "CP L: Compare L against A";
          break;
        case 0xBE:  
          result = "CP (HL): Compare value pointed by HL against A";
          break;
        case 0xBF:  
          result = "CP A: Compare A against A";
          break;
        case 0xC0:  
          result = "RET NZ: Return if last result was not zero";
          break;
        case 0xC1:  
          result = "POP BC: Pop 16-bit value from stack into BC";
          break;
        case 0xC2:  
          result = "JP NZ,nn: Absolute jump to 16-bit location if last result was not zero";
          break;
        case 0xC3:  
          result = "JP nn: Absolute jump to 16-bit location";
          break;
        case 0xC4:  
          result = "CALL NZ,nn: Call routine at 16-bit location if last result was not zero";
          break;
        case 0xC5:  
          result = "PUSH BC: Push 16-bit BC onto stack";
          break;
        case 0xC6:  
          result = "ADD A,n: Add 8-bit immediate to A";
          break;
        case 0xC7:  
          result = "RST 0: Call routine at address 0000h";
          break;
        case 0xC8:  
          result = "RET Z: Return if last result was zero";
          break;
        case 0xC9:  
          result = "RET: Return to calling routine";
          break;
        case 0xCA:  
          result = "JP Z,nn: Absolute jump to 16-bit location if last result was zero";
          break;
        case 0xCB:  
          result = "Ext ops: Extended operations (two-byte instruction code)";
          break;
        case 0xCC:  
          result = "CALL Z,nn: Call routine at 16-bit location if last result was zero";
          break;
        case 0xCD:  
          result = "CALL nn: Call routine at 16-bit location";
          break;
        case 0xCE:  
          result = "ADC A,n: Add 8-bit immediate and carry to A";
          break;
        case 0xCF:  
          result = "RST 8: Call routine at address 0008h";
          break;
        case 0xD0:  
          result = "RET NC: Return if last result caused no carry";
          break;
        case 0xD1:  
          result = "POP DE: Pop 16-bit value from stack into DE";
          break;
        case 0xD2:  
          result = "JP NC,nn: Absolute jump to 16-bit location if last result caused no carry";
          break;
        case 0xD3:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xD4:  
          result = "CALL NC,nn: Call routine at 16-bit location if last result caused no carry";
          break;
        case 0xD5:  
          result = "PUSH DE: Push 16-bit DE onto stack";
          break;
        case 0xD6:  
          result = "SUB A,n: Subtract 8-bit immediate from A";
          break;
        case 0xD7:  
          result = "RST 10: Call routine at address 0010h";
          break;
        case 0xD8:  
          result = "RET C: Return if last result caused carry";
          break;
        case 0xD9:  
          result = "RETI: Enable interrupts and return to calling routine";
          break;
        case 0xDA:  
          result = "JP C,nn: Absolute jump to 16-bit location if last result caused carry";
          break;
        case 0xDB:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xDC:  
          result = "CALL C,nn: Call routine at 16-bit location if last result caused carry";
          break;
        case 0xDD:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xDE:  
          result = "SBC A,n: Subtract 8-bit immediate and carry from A";
          break;
        case 0xDF:  
          result = "RST 18: Call routine at address 0018h";
          break;
        case 0xE0:  
          result = "LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)";
          break;
        case 0xE1:  
          result = "POP HL: Pop 16-bit value from stack into HL";
          break;
        case 0xE2:  
          result = "LDH (C),A: Save A at address pointed to by (FF00h + C)";
          break;
        case 0xE3:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xE4:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xE5:  
          result = "PUSH HL: Push 16-bit HL onto stack";
          break;
        case 0xE6:  
          result = "AND n: Logical AND 8-bit immediate against A";
          break;
        case 0xE7:  
          result = "RST 20: Call routine at address 0020h";
          break;
        case 0xE8:  
          result = "ADD SP,d: Add signed 8-bit immediate to SP";
          break;
        case 0xE9:  
          result = "JP (HL): Jump to 16-bit value pointed by HL";
          break;
        case 0xEA:  
          result = "LD (nn),A: Save A at given 16-bit address";
          break;
        case 0xEB:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xEC:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xED:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xEE:  
          result = "XOR n: Logical XOR 8-bit immediate against A";
          break;
        case 0xEF:  
          result = "RST 28: Call routine at address 0028h";
          break;
        case 0xF0:  
          result = "LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)";
          break;
        case 0xF1:  
          result = "POP AF: Pop 16-bit value from stack into AF";
          break;
        case 0xF2:  
          result = "LDH A, (C): Operation removed in this CPU? (Or Load into A memory from FF00 + C?)";
          break;
        case 0xF3:  
          result = "DI: DIsable interrupts";
          break;
        case 0xF4:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xF5:  
          result = "PUSH AF: Push 16-bit AF onto stack";
          break;
        case 0xF6:  
          result = "OR n: Logical OR 8-bit immediate against A";
          break;
        case 0xF7:  
          result = "RST 30: Call routine at address 0030h";
          break;
        case 0xF8:  
          result = "LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL";
          break;
        case 0xF9:  
          result = "LD SP,HL: Copy HL to SP";
          break;
        case 0xFA:  
          result = "LD A,(nn): Load A from given 16-bit address";
          break;
        case 0xFB:  
          result = "EI: Enable interrupts";
          break;
        case 0xFC:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xFD:  
          result = "XX: Operation removed in this CPU";
          break;
        case 0xFE:  
          result = "CP n: Compare 8-bit immediate against A";
          break;
        case 0xFF:  
          result = "RST 38: Call routine at address 0038h";
          break;
        default:
          result = "";
          break;
      }

      return result;
    }
  }
}
