using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUOpcodeNames
  {
    internal static string Get(byte opcode)
    {
      string result;
      switch(opcode)
      {
        case 0x00:  // NOP
          result = "NOP";
          break;
        case 0x01:  // LD BC,nn
          result = "LD BC,nn";
          break;
        case 0x02:  // LD (BC),A
          result = "LD (BC),A";
          break;
        case 0x03:  // INC BC
          result = "INC BC";
          break;
        case 0x04:  // INC B
          result = "INC B";
          break;
        case 0x05:  // DEC B
          result = "DEC B";
          break;
        case 0x06:  // LD B,n
          result = "LD B,n";
          break;
        case 0x07:  // RLC A
          result = "RLC A";
          break;
        case 0x08:  // LD (nn),SP
          result = "LD (nn),SP";
          break;
        case 0x09:  // ADD HL,BC
          result = "ADD HL,BC";
          break;
        case 0x0A:  // LD A,(BC)
          result = "LD A,(BC)";
          break;
        case 0x0B:  // DEC BC
          result = "DEC BC";
          break;
        case 0x0C:  // INC C
          result = "INC C";
          break;
        case 0x0D:  // DEC C
          result = "DEC C";
          break;
        case 0x0E:  // LD C,n
          result = "LD C,n";
          break;
        case 0x0F:  // RRC A
          result = "RRC A";
          break;
        case 0x10:  // STOP
          result = "STOP";
          break;
        case 0x11:  // LD DE,nn
          result = "LD DE,nn";
          break;
        case 0x12:  // LD (DE),A
          result = "LD (DE),A";
          break;
        case 0x13:  // INC DE
          result = "INC DE";
          break;
        case 0x14:  // INC D
          result = "INC D";
          break;
        case 0x15:  // DEC D
          result = "DEC D";
          break;
        case 0x16:  // LD D,n
          result = "LD D,n";
          break;
        case 0x17:  // RL A
          result = "RL A";
          break;
        case 0x18:  // JR n
          result = "JR n";
          break;
        case 0x19:  // ADD HL,DE
          result = "ADD HL,DE";
          break;
        case 0x1A:  // LD A,(DE)
          result = "LD A,(DE)";
          break;
        case 0x1B:  // DEC DE
          result = "DEC DE";
          break;
        case 0x1C:  // INC E
          result = "INC E";
          break;
        case 0x1D:  // DEC E
          result = "DEC E";
          break;
        case 0x1E:  // LD E,n
          result = "LD E,n";
          break;
        case 0x1F:  // RR A
          result = "RR A";
          break;
        case 0x20:  // JR NZ,n
          result = "JR NZ,n";
          break;
        case 0x21:  // LD HL,nn
          result = "LD HL,nn";
          break;
        case 0x22:  // LDI (HL),A
          result = "LDI (HL),A";
          break;
        case 0x23:  // INC HL
          result = "INC HL";
          break;
        case 0x24:  // INC H
          result = "INC H";
          break;
        case 0x25:  // DEC H
          result = "DEC H";
          break;
        case 0x26:  // LD H,n
          result = "LD H,n";
          break;
        case 0x27:  // DAA
          result = "DAA";
          break;
        case 0x28:  // JR Z,n
          result = "JR Z,n";
          break;
        case 0x29:  // ADD HL,HL
          result = "ADD HL,HL";
          break;
        case 0x2A:  // LDI A,(HL)
          result = "LDI A,(HL)";
          break;
        case 0x2B:  // DEC HL
          result = "DEC HL";
          break;
        case 0x2C:  // INC L
          result = "INC L";
          break;
        case 0x2D:  // DEC L
          result = "DEC L";
          break;
        case 0x2E:  // LD L,n
          result = "LD L,n";
          break;
        case 0x2F:  // CPL
          result = "CPL";
          break;
        case 0x30:  // JR NC,n
          result = "JR NC,n";
          break;
        case 0x31:  // LD SP,nn
          result = "LD SP,nn";
          break;
        case 0x32:  // LDD (HL),A
          result = "LDD (HL),A";
          break;
        case 0x33:  // INC SP
          result = "INC SP";
          break;
        case 0x34:  // INC (HL)
          result = "INC (HL)";
          break;
        case 0x35:  // DEC (HL)
          result = "DEC (HL)";
          break;
        case 0x36:  // LD (HL),n
          result = "LD (HL),n";
          break;
        case 0x37:  // SCF
          result = "SCF";
          break;
        case 0x38:  // JR C,n
          result = "JR C,n";
          break;
        case 0x39:  // ADD HL,SP
          result = "ADD HL,SP";
          break;
        case 0x3A:  // LDD A,(HL)
          result = "LDD A,(HL)";
          break;
        case 0x3B:  // DEC SP
          result = "DEC SP";
          break;
        case 0x3C:  // INC A
          result = "INC A";
          break;
        case 0x3D:  // DEC A
          result = "DEC A";
          break;
        case 0x3E:  // LD A,n
          result = "LD A,n";
          break;
        case 0x3F:  // CCF
          result = "CCF";
          break;
        case 0x40:  // LD B,B
          result = "LD B,B";
          break;
        case 0x41:  // LD B,C
          result = "LD B,C";
          break;
        case 0x42:  // LD B,D
          result = "LD B,D";
          break;
        case 0x43:  // LD B,E
          result = "LD B,E";
          break;
        case 0x44:  // LD B,H
          result = "LD B,H";
          break;
        case 0x45:  // LD B,L
          result = "LD B,L";
          break;
        case 0x46:  // LD B,(HL)
          result = "LD B,(HL)";
          break;
        case 0x47:  // LD B,A
          result = "LD B,A";
          break;
        case 0x48:  // LD C,B
          result = "LD C,B";
          break;
        case 0x49:  // LD C,C
          result = "LD C,C";
          break;
        case 0x4A:  // LD C,D
          result = "LD C,D";
          break;
        case 0x4B:  // LD C,E
          result = "LD C,E";
          break;
        case 0x4C:  // LD C,H
          result = "LD C,H";
          break;
        case 0x4D:  // LD C,L
          result = "LD C,L";
          break;
        case 0x4E:  // LD C,(HL)
          result = "LD C,(HL)";
          break;
        case 0x4F:  // LD C,A
          result = "LD C,A";
          break;
        case 0x50:  // LD D,B
          result = "LD D,B";
          break;
        case 0x51:  // LD D,C
          result = "LD D,C";
          break;
        case 0x52:  // LD D,D
          result = "LD D,D";
          break;
        case 0x53:  // LD D,E
          result = "LD D,E";
          break;
        case 0x54:  // LD D,H
          result = "LD D,H";
          break;
        case 0x55:  // LD D,L
          result = "LD D,L";
          break;
        case 0x56:  // LD D,(HL)
          result = "LD D,(HL)";
          break;
        case 0x57:  // LD D,A
          result = "LD D,A";
          break;
        case 0x58:  // LD E,B
          result = "LD E,B";
          break;
        case 0x59:  // LD E,C
          result = "LD E,C";
          break;
        case 0x5A:  // LD E,D
          result = "LD E,D";
          break;
        case 0x5B:  // LD E,E
          result = "LD E,E";
          break;
        case 0x5C:  // LD E,H
          result = "LD E,H";
          break;
        case 0x5D:  // LD E,L
          result = "LD E,L";
          break;
        case 0x5E:  // LD E,(HL)
          result = "LD E,(HL)";
          break;
        case 0x5F:  // LD E,A
          result = "LD E,A";
          break;
        case 0x60:  // LD H,B
          result = "LD H,B";
          break;
        case 0x61:  // LD H,C
          result = "LD H,C";
          break;
        case 0x62:  // LD H,D
          result = "LD H,D";
          break;
        case 0x63:  // LD H,E
          result = "LD H,E";
          break;
        case 0x64:  // LD H,H
          result = "LD H,H";
          break;
        case 0x65:  // LD H,L
          result = "LD H,L";
          break;
        case 0x66:  // LD H,(HL)
          result = "LD H,(HL)";
          break;
        case 0x67:  // LD H,A
          result = "LD H,A";
          break;
        case 0x68:  // LD L,B
          result = "LD L,B";
          break;
        case 0x69:  // LD L,C
          result = "LD L,C";
          break;
        case 0x6A:  // LD L,D
          result = "LD L,D";
          break;
        case 0x6B:  // LD L,E
          result = "LD L,E";
          break;
        case 0x6C:  // LD L,H
          result = "LD L,H";
          break;
        case 0x6D:  // LD L,L
          result = "LD L,L";
          break;
        case 0x6E:  // LD L,(HL)
          result = "LD L,(HL)";
          break;
        case 0x6F:  // LD L,A
          result = "LD L,A";
          break;
        case 0x70:  // LD (HL),B
          result = "LD (HL),B";
          break;
        case 0x71:  // LD (HL),C
          result = "LD (HL),C";
          break;
        case 0x72:  // LD (HL),D
          result = "LD (HL),D";
          break;
        case 0x73:  // LD (HL),E
          result = "LD (HL),E";
          break;
        case 0x74:  // LD (HL),H
          result = "LD (HL),H";
          break;
        case 0x75:  // LD (HL),L
          result = "LD (HL),L";
          break;
        case 0x76:  // HALT
          result = "HALT";
          break;
        case 0x77:  // LD (HL),A
          result = "LD (HL),A";
          break;
        case 0x78:  // LD A,B
          result = "LD A,B";
          break;
        case 0x79:  // LD A,C
          result = "LD A,C";
          break;
        case 0x7A:  // LD A,D
          result = "LD A,D";
          break;
        case 0x7B:  // LD A,E
          result = "LD A,E";
          break;
        case 0x7C:  // LD A,H
          result = "LD A,H";
          break;
        case 0x7D:  // LD A,L
          result = "LD A,L";
          break;
        case 0x7E:  // LD A,(HL)
          result = "LD A,(HL)";
          break;
        case 0x7F:  // LD A,A
          result = "LD A,A";
          break;
        case 0x80:  // ADD A,B
          result = "ADD A,B";
          break;
        case 0x81:  // ADD A,C
          result = "ADD A,C";
          break;
        case 0x82:  // ADD A,D
          result = "ADD A,D";
          break;
        case 0x83:  // ADD A,E
          result = "ADD A,E";
          break;
        case 0x84:  // ADD A,H
          result = "ADD A,H";
          break;
        case 0x85:  // ADD A,L
          result = "ADD A,L";
          break;
        case 0x86:  // ADD A,(HL)
          result = "ADD A,(HL)";
          break;
        case 0x87:  // ADD A,A
          result = "ADD A,A";
          break;
        case 0x88:  // ADC A,B
          result = "ADC A,B";
          break;
        case 0x89:  // ADC A,C
          result = "ADC A,C";
          break;
        case 0x8A:  // ADC A,D
          result = "ADC A,D";
          break;
        case 0x8B:  // ADC A,E
          result = "ADC A,E";
          break;
        case 0x8C:  // ADC A,H
          result = "ADC A,H";
          break;
        case 0x8D:  // ADC A,L
          result = "ADC A,L";
          break;
        case 0x8E:  // ADC A,(HL)
          result = "ADC A,(HL)";
          break;
        case 0x8F:  // ADC A,A
          result = "ADC A,A";
          break;
        case 0x90:  // SUB A,B
          result = "SUB A,B";
          break;
        case 0x91:  // SUB A,C
          result = "SUB A,C";
          break;
        case 0x92:  // SUB A,D
          result = "SUB A,D";
          break;
        case 0x93:  // SUB A,E
          result = "SUB A,E";
          break;
        case 0x94:  // SUB A,H
          result = "SUB A,H";
          break;
        case 0x95:  // SUB A,L
          result = "SUB A,L";
          break;
        case 0x96:  // SUB A,(HL)
          result = "SUB A,(HL)";
          break;
        case 0x97:  // SUB A,A
          result = "SUB A,A";
          break;
        case 0x98:  // SBC A,B
          result = "SBC A,B";
          break;
        case 0x99:  // SBC A,C
          result = "SBC A,C";
          break;
        case 0x9A:  // SBC A,D
          result = "SBC A,D";
          break;
        case 0x9B:  // SBC A,E
          result = "SBC A,E";
          break;
        case 0x9C:  // SBC A,H
          result = "SBC A,H";
          break;
        case 0x9D:  // SBC A,L
          result = "SBC A,L";
          break;
        case 0x9E:  // SBC A,(HL)
          result = "SBC A,(HL)";
          break;
        case 0x9F:  // SBC A,A
          result = "SBC A,A";
          break;
        case 0xA0:  // AND B
          result = "AND B";
          break;
        case 0xA1:  // AND C
          result = "AND C";
          break;
        case 0xA2:  // AND D
          result = "AND D";
          break;
        case 0xA3:  // AND E
          result = "AND E";
          break;
        case 0xA4:  // AND H
          result = "AND H";
          break;
        case 0xA5:  // AND L
          result = "AND L";
          break;
        case 0xA6:  // AND (HL)
          result = "AND (HL)";
          break;
        case 0xA7:  // AND A
          result = "AND A";
          break;
        case 0xA8:  // XOR B
          result = "XOR B";
          break;
        case 0xA9:  // XOR C
          result = "XOR C";
          break;
        case 0xAA:  // XOR D
          result = "XOR D";
          break;
        case 0xAB:  // XOR E
          result = "XOR E";
          break;
        case 0xAC:  // XOR H
          result = "XOR H";
          break;
        case 0xAD:  // XOR L
          result = "XOR L";
          break;
        case 0xAE:  // XOR (HL)
          result = "XOR (HL)";
          break;
        case 0xAF:  // XOR A
          result = "XOR A";
          break;
        case 0xB0:  // OR B
          result = "OR B";
          break;
        case 0xB1:  // OR C
          result = "OR C";
          break;
        case 0xB2:  // OR D
          result = "OR D";
          break;
        case 0xB3:  // OR E
          result = "OR E";
          break;
        case 0xB4:  // OR H
          result = "OR H";
          break;
        case 0xB5:  // OR L
          result = "OR L";
          break;
        case 0xB6:  // OR (HL)
          result = "OR (HL)";
          break;
        case 0xB7:  // OR A
          result = "OR A";
          break;
        case 0xB8:  // CP B
          result = "CP B";
          break;
        case 0xB9:  // CP C
          result = "CP C";
          break;
        case 0xBA:  // CP D
          result = "CP D";
          break;
        case 0xBB:  // CP E
          result = "CP E";
          break;
        case 0xBC:  // CP H
          result = "CP H";
          break;
        case 0xBD:  // CP L
          result = "CP L";
          break;
        case 0xBE:  // CP (HL)
          result = "CP (HL)";
          break;
        case 0xBF:  // CP A
          result = "CP A";
          break;
        case 0xC0:  // RET NZ
          result = "RET NZ";
          break;
        case 0xC1:  // POP BC
          result = "POP BC";
          break;
        case 0xC2:  // JP NZ,nn
          result = "JP NZ,nn";
          break;
        case 0xC3:  // JP nn
          result = "JP nn";
          break;
        case 0xC4:  // CALL NZ,nn
          result = "CALL NZ,nn";
          break;
        case 0xC5:  // PUSH BC
          result = "PUSH BC";
          break;
        case 0xC6:  // ADD A,n
          result = "ADD A,n";
          break;
        case 0xC7:  // RST 0
          result = "RST 0";
          break;
        case 0xC8:  // RET Z
          result = "RET Z";
          break;
        case 0xC9:  // RET
          result = "RET";
          break;
        case 0xCA:  // JP Z,nn
          result = "JP Z,nn";
          break;
        case 0xCB:  // Ext ops
          result = "Ext ops";
          break;
        case 0xCC:  // CALL Z,nn
          result = "CALL Z,nn";
          break;
        case 0xCD:  // CALL nn
          result = "CALL nn";
          break;
        case 0xCE:  // ADC A,n
          result = "ADC A,n";
          break;
        case 0xCF:  // RST 8
          result = "RST 8";
          break;
        case 0xD0:  // RET NC
          result = "RET NC";
          break;
        case 0xD1:  // POP DE
          result = "POP DE";
          break;
        case 0xD2:  // JP NC,nn
          result = "JP NC,nn";
          break;
        // {0xD3, }, // XX
        case 0xD4:  // CALL NC,nn
          result = "CALL NC,nn";
          break;
        case 0xD5:  // PUSH DE
          result = "PUSH DE";
          break;
        case 0xD6:  // SUB A,n
          result = "SUB A,n";
          break;
        case 0xD7:  // RST 10
          result = "RST 10";
          break;
        case 0xD8:  // RET C
          result = "RET C";
          break;
        case 0xD9:  // RETI
          result = "RETI";
          break;
        case 0xDA:  // JP C,nn
          result = "JP C,nn";
          break;
        // {0xDB, }, // XX
        case 0xDC:  // CALL C,nn
          result = "CALL C,nn";
          break;
        // {0xDD, }, // XX
        case 0xDE:  // SBC A,n
          result = "SBC A,n";
          break;
        case 0xDF:  // RST 18
          result = "RST 18";
          break;
        case 0xE0:  // LDH (n),A
          result = "LDH (n),A";
          break;
        case 0xE1:  // POP HL
          result = "POP HL";
          break;
        case 0xE2:  // LDH (C),A
          result = "LDH (C),A";
          break;
        // {0xE3, }, // XX
        case 0xE4:  // XX
          result = "E4 is a (mayor) douchebag";
          break;
        case 0xE5:  // PUSH HL
          result = "PUSH HL";
          break;
        case 0xE6:  // AND n
          result = "AND n";
          break;
        case 0xE7:  // RST 20
          result = "RST 20";
          break;
        case 0xE8:  // ADD SP,d
          result = "ADD SP,d";
          break;
        case 0xE9:  // JP (HL)
          result = "JP (HL)";
          break;
        case 0xEA:  // LD (nn),A
          result = "LD (nn),A";
          break;
        // {0xEB, }, // XX
        // {0xEC, }, // XX
        // {0xED, }, // XX
        case 0xEE:  // XOR n
          result = "XOR n";
          break;
        case 0xEF:  // RST 28
          result = "RST 28";
          break;
        case 0xF0:  // LDH A,(n)
          result = "LDH A,(n)";
          break;
        case 0xF1:  // POP AF
          result = "POP AF";
          break;
        case 0xF2:  // LDH A, (C)
          result = "LDH A, (C)";
          break;
        case 0xF3:  // DI
          result = "DI";
          break;
        // {0xF4, }, // XX
        case 0xF5:  // PUSH AF
          result = "PUSH AF";
          break;
        case 0xF6:  // OR n
          result = "OR n";
          break;
        case 0xF7:  // RST 30
          result = "RST 30";
          break;
        case 0xF8:  // LDHL SP,d
          result = "LDHL SP,d";
          break;
        case 0xF9:  // LD SP,HL
          result = "LD SP,HL";
          break;
        case 0xFA:  // LD A,(nn)
          result = "LD A,(nn)";
          break;
        case 0xFB:  // EI
          result = "EI";
          break;
        // {0xFC, }, // XX
        // {0xFD, }, // XX
        case 0xFE:  // CP n
          result = "CP n";
          break;
        case 0xFF: // RST 38
          result = "RST 38";
          break;
        default:
          result = "";
          break;
      }

      return result;
    }
  }
}
