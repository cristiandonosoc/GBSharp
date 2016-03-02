using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  public class CPUInstructionClocks
  {
    public static string Get(byte opcode)
    {
      string result;
      switch(opcode)
      {
        case 0x00:  // NOP
          result = "4";
          break;
        case 0x01:  // LD BC,nn
          result = "12";
          break;
        case 0x02:  // LD (BC),A
          result = "8";
          break;
        case 0x03:  // INC BC
          result = "8";
          break;
        case 0x04:  // INC B
          result = "4";
          break;
        case 0x05:  // DEC B
          result = "4";
          break;
        case 0x06:  // LD B,n
          result = "8";
          break;
        case 0x07:  // RLC A
          result = "4";
          break;
        case 0x08:  // LD (nn),SP
          result = "20";
          break;
        case 0x09:  // ADD HL,BC
          result = "8";
          break;
        case 0x0A:  // LD A,(BC)
          result = "8";
          break;
        case 0x0B:  // DEC BC
          result = "8";
          break;
        case 0x0C:  // INC C
          result = "4";
          break;
        case 0x0D:  // DEC C
          result = "4";
          break;
        case 0x0E:  // LD C,n
          result = "8";
          break;
        case 0x0F:  // RRC A
          result = "4";
          break;
        case 0x10:  // STOP
          result = "4";
          break;
        case 0x11:  // LD DE,nn
          result = "12";
          break;
        case 0x12:  // LD (DE),A
          result = "8";
          break;
        case 0x13:  // INC DE
          result = "8";
          break;
        case 0x14:  // INC D
          result = "4";
          break;
        case 0x15:  // DEC D
          result = "4";
          break;
        case 0x16:  // LD D,n
          result = "8";
          break;
        case 0x17:  // RL A
          result = "4";
          break;
        case 0x18:  // JR n
          result = "12";
          break;
        case 0x19:  // ADD HL,DE
          result = "8";
          break;
        case 0x1A:  // LD A,(DE)
          result = "8";
          break;
        case 0x1B:  // DEC DE
          result = "8";
          break;
        case 0x1C:  // INC E
          result = "4";
          break;
        case 0x1D:  // DEC E
          result = "4";
          break;
        case 0x1E:  // LD E,n
          result = "8";
          break;
        case 0x1F:  // RR A
          result = "4";
          break;
        case 0x20:  // JR NZ,n
          result = "12/8";
          break;
        case 0x21:  // LD HL,nn
          result = "12";
          break;
        case 0x22:  // LDI (HL),A
          result = "8";
          break;
        case 0x23:  // INC HL
          result = "8";
          break;
        case 0x24:  // INC H
          result = "4";
          break;
        case 0x25:  // DEC H
          result = "4";
          break;
        case 0x26:  // LD H,n
          result = "8";
          break;
        case 0x27:  // DAA
          result = "4";
          break;
        case 0x28:  // JR Z,n
          result = "12/8";
          break;
        case 0x29:  // ADD HL,HL
          result = "8";
          break;
        case 0x2A:  // LDI A,(HL)
          result = "8";
          break;
        case 0x2B:  // DEC HL
          result = "8";
          break;
        case 0x2C:  // INC L
          result = "4";
          break;
        case 0x2D:  // DEC L
          result = "4";
          break;
        case 0x2E:  // LD L,n
          result = "8";
          break;
        case 0x2F:  // CPL
          result = "4";
          break;
        case 0x30:  // JR NC,n
          result = "12/8";
          break;
        case 0x31:  // LD SP,nn
          result = "12";
          break;
        case 0x32:  // LDD (HL),A
          result = "8";
          break;
        case 0x33:  // INC SP
          result = "8";
          break;
        case 0x34:  // INC (HL)
          result = "12";
          break;
        case 0x35:  // DEC (HL)
          result = "12";
          break;
        case 0x36:  // LD (HL),n
          result = "12";
          break;
        case 0x37:  // SCF
          result = "4";
          break;
        case 0x38:  // JR C,n
          result = "12/8";
          break;
        case 0x39:  // ADD HL,SP
          result = "8";
          break;
        case 0x3A:  // LDD A,(HL)
          result = "8";
          break;
        case 0x3B:  // DEC SP
          result = "8";
          break;
        case 0x3C:  // INC A
          result = "4";
          break;
        case 0x3D:  // DEC A
          result = "4";
          break;
        case 0x3E:  // LD A,n
          result = "8";
          break;
        case 0x3F:  // CCF
          result = "4";
          break;
        case 0x40:  // LD B,B
          result = "4";
          break;
        case 0x41:  // LD B,C
          result = "4";
          break;
        case 0x42:  // LD B,D
          result = "4";
          break;
        case 0x43:  // LD B,E
          result = "4";
          break;
        case 0x44:  // LD B,H
          result = "4";
          break;
        case 0x45:  // LD B,L
          result = "4";
          break;
        case 0x46:  // LD B,(HL)
          result = "8";
          break;
        case 0x47:  // LD B,A
          result = "4";
          break;
        case 0x48:  // LD C,B
          result = "4";
          break;
        case 0x49:  // LD C,C
          result = "4";
          break;
        case 0x4A:  // LD C,D
          result = "4";
          break;
        case 0x4B:  // LD C,E
          result = "4";
          break;
        case 0x4C:  // LD C,H
          result = "4";
          break;
        case 0x4D:  // LD C,L
          result = "4";
          break;
        case 0x4E:  // LD C,(HL)
          result = "8";
          break;
        case 0x4F:  // LD C,A
          result = "4";
          break;
        case 0x50:  // LD D,B
          result = "4";
          break;
        case 0x51:  // LD D,C
          result = "4";
          break;
        case 0x52:  // LD D,D
          result = "4";
          break;
        case 0x53:  // LD D,E
          result = "4";
          break;
        case 0x54:  // LD D,H
          result = "4";
          break;
        case 0x55:  // LD D,L
          result = "4";
          break;
        case 0x56:  // LD D,(HL)
          result = "8";
          break;
        case 0x57:  // LD D,A
          result = "4";
          break;
        case 0x58:  // LD E,B
          result = "4";
          break;
        case 0x59:  // LD E,C
          result = "4";
          break;
        case 0x5A:  // LD E,D
          result = "4";
          break;
        case 0x5B:  // LD E,E
          result = "4";
          break;
        case 0x5C:  // LD E,H
          result = "4";
          break;
        case 0x5D:  // LD E,L
          result = "4";
          break;
        case 0x5E:  // LD E,(HL)
          result = "8";
          break;
        case 0x5F:  // LD E,A
          result = "4";
          break;
        case 0x60:  // LD H,B
          result = "4";
          break;
        case 0x61:  // LD H,C
          result = "4";
          break;
        case 0x62:  // LD H,D
          result = "4";
          break;
        case 0x63:  // LD H,E
          result = "4";
          break;
        case 0x64:  // LD H,H
          result = "4";
          break;
        case 0x65:  // LD H,L
          result = "4";
          break;
        case 0x66:  // LD H,(HL)
          result = "8";
          break;
        case 0x67:  // LD H,A
          result = "4";
          break;
        case 0x68:  // LD L,B
          result = "4";
          break;
        case 0x69:  // LD L,C
          result = "4";
          break;
        case 0x6A:  // LD L,D
          result = "4";
          break;
        case 0x6B:  // LD L,E
          result = "4";
          break;
        case 0x6C:  // LD L,H
          result = "4";
          break;
        case 0x6D:  // LD L,L
          result = "4";
          break;
        case 0x6E:  // LD L,(HL)
          result = "8";
          break;
        case 0x6F:  // LD L,A
          result = "4";
          break;
        case 0x70:  // LD (HL),B
          result = "8";
          break;
        case 0x71:  // LD (HL),C
          result = "8";
          break;
        case 0x72:  // LD (HL),D
          result = "8";
          break;
        case 0x73:  // LD (HL),E
          result = "8";
          break;
        case 0x74:  // LD (HL),H
          result = "8";
          break;
        case 0x75:  // LD (HL),L
          result = "8";
          break;
        case 0x76:  // HALT
          result = "4";
          break;
        case 0x77:  // LD (HL),A
          result = "8";
          break;
        case 0x78:  // LD A,B
          result = "4";
          break;
        case 0x79:  // LD A,C
          result = "4";
          break;
        case 0x7A:  // LD A,D
          result = "4";
          break;
        case 0x7B:  // LD A,E
          result = "4";
          break;
        case 0x7C:  // LD A,H
          result = "4";
          break;
        case 0x7D:  // LD A,L
          result = "4";
          break;
        case 0x7E:  // LD A,(HL)
          result = "8";
          break;
        case 0x7F:  // LD A,A
          result = "4";
          break;
        case 0x80:  // ADD A,B
          result = "4";
          break;
        case 0x81:  // ADD A,C
          result = "4";
          break;
        case 0x82:  // ADD A,D
          result = "4";
          break;
        case 0x83:  // ADD A,E
          result = "4";
          break;
        case 0x84:  // ADD A,H
          result = "4";
          break;
        case 0x85:  // ADD A,L
          result = "4";
          break;
        case 0x86:  // ADD A,(HL)
          result = "8";
          break;
        case 0x87:  // ADD A,A
          result = "4";
          break;
        case 0x88:  // ADC A,B
          result = "4";
          break;
        case 0x89:  // ADC A,C
          result = "4";
          break;
        case 0x8A:  // ADC A,D
          result = "4";
          break;
        case 0x8B:  // ADC A,E
          result = "4";
          break;
        case 0x8C:  // ADC A,H
          result = "4";
          break;
        case 0x8D:  // ADC A,L
          result = "4";
          break;
        case 0x8E:  // ADC A,(HL)
          result = "8";
          break;
        case 0x8F:  // ADC A,A
          result = "4";
          break;
        case 0x90:  // SUB A,B
          result = "4";
          break;
        case 0x91:  // SUB A,C
          result = "4";
          break;
        case 0x92:  // SUB A,D
          result = "4";
          break;
        case 0x93:  // SUB A,E
          result = "4";
          break;
        case 0x94:  // SUB A,H
          result = "4";
          break;
        case 0x95:  // SUB A,L
          result = "4";
          break;
        case 0x96:  // SUB A,(HL)
          result = "8";
          break;
        case 0x97:  // SUB A,A
          result = "4";
          break;
        case 0x98:  // SBC A,B
          result = "4";
          break;
        case 0x99:  // SBC A,C
          result = "4";
          break;
        case 0x9A:  // SBC A,D
          result = "4";
          break;
        case 0x9B:  // SBC A,E
          result = "4";
          break;
        case 0x9C:  // SBC A,H
          result = "4";
          break;
        case 0x9D:  // SBC A,L
          result = "4";
          break;
        case 0x9E:  // SBC A,(HL)
          result = "8";
          break;
        case 0x9F:  // SBC A,A
          result = "4";
          break;
        case 0xA0:  // AND B
          result = "4";
          break;
        case 0xA1:  // AND C
          result = "4";
          break;
        case 0xA2:  // AND D
          result = "4";
          break;
        case 0xA3:  // AND E
          result = "4";
          break;
        case 0xA4:  // AND H
          result = "4";
          break;
        case 0xA5:  // AND L
          result = "4";
          break;
        case 0xA6:  // AND (HL)
          result = "8";
          break;
        case 0xA7:  // AND A
          result = "4";
          break;
        case 0xA8:  // XOR B
          result = "4";
          break;
        case 0xA9:  // XOR C
          result = "4";
          break;
        case 0xAA:  // XOR D
          result = "4";
          break;
        case 0xAB:  // XOR E
          result = "4";
          break;
        case 0xAC:  // XOR H
          result = "4";
          break;
        case 0xAD:  // XOR L
          result = "4";
          break;
        case 0xAE:  // XOR (HL)
          result = "8";
          break;
        case 0xAF:  // XOR A
          result = "4";
          break;
        case 0xB0:  // OR B
          result = "4";
          break;
        case 0xB1:  // OR C
          result = "4";
          break;
        case 0xB2:  // OR D
          result = "4";
          break;
        case 0xB3:  // OR E
          result = "4";
          break;
        case 0xB4:  // OR H
          result = "4";
          break;
        case 0xB5:  // OR L
          result = "4";
          break;
        case 0xB6:  // OR (HL)
          result = "8";
          break;
        case 0xB7:  // OR A
          result = "4";
          break;
        case 0xB8:  // CP B
          result = "4";
          break;
        case 0xB9:  // CP C
          result = "4";
          break;
        case 0xBA:  // CP D
          result = "4";
          break;
        case 0xBB:  // CP E
          result = "4";
          break;
        case 0xBC:  // CP H
          result = "4";
          break;
        case 0xBD:  // CP L
          result = "4";
          break;
        case 0xBE:  // CP (HL)
          result = "8";
          break;
        case 0xBF:  // CP A
          result = "4";
          break;
        case 0xC0:  // RET NZ
          result = "20/8";
          break;
        case 0xC1:  // POP BC
          result = "12";
          break;
        case 0xC2:  // JP NZ,nn
          result = "16/12";
          break;
        case 0xC3:  // JP nn
          result = "16";
          break;
        case 0xC4:  // CALL NZ,nn
          result = "24/12";
          break;
        case 0xC5:  // PUSH BC
          result = "16";
          break;
        case 0xC6:  // ADD A,n
          result = "8";
          break;
        case 0xC7:  // RST 0
          result = "16";
          break;
        case 0xC8:  // RET Z
          result = "20/8";
          break;
        case 0xC9:  // RET
          result = "16";
          break;
        case 0xCA:  // JP Z,nn
          result = "16/12";
          break;
        case 0xCB:  // Ext ops
          result = "4";
          break;
        case 0xCC:  // CALL Z,nn
          result = "24/12";
          break;
        case 0xCD:  // CALL nn
          result = "24";
          break;
        case 0xCE:  // ADC A,n
          result = "8";
          break;
        case 0xCF:  // RST 8
          result = "16";
          break;
        case 0xD0:  // RET NC
          result = "20/8";
          break;
        case 0xD1:  // POP DE
          result = "12";
          break;
        case 0xD2:  // JP NC,nn
          result = "16/12";
          break;
        // {0xD3, }, // XX
        case 0xD4:  // CALL NC,nn
          result = "24/12";
          break;
        case 0xD5:  // PUSH DE
          result = "16";
          break;
        case 0xD6:  // SUB A,n
          result = "8";
          break;
        case 0xD7:  // RST 10
          result = "16";
          break;
        case 0xD8:  // RET C
          result = "20/8";
          break;
        case 0xD9:  // RETI
          result = "16";
          break;
        case 0xDA:  // JP C,nn
          result = "16/12";
          break;
        // {0xDB, }, // XX
        case 0xDC:  // CALL C,nn
          result = "24/12";
          break;
        // {0xDD, }, // XX
        case 0xDE:  // SBC A,n
          result = "8";
          break;
        case 0xDF:  // RST 18
          result = "16";
          break;
        case 0xE0:  // LDH (n),A
          result = "12";
          break;
        case 0xE1:  // POP HL
          result = "12";
          break;
        case 0xE2:  // LDH (C),A
          result = "8";
          break;
        // {0xE3, }, // XX
        //{0xE4, 4}, // XX
        case 0xE5:  // PUSH HL
          result = "16";
          break;
        case 0xE6:  // AND n
          result = "8";
          break;
        case 0xE7:  // RST 20
          result = "16";
          break;
        case 0xE8:  // ADD SP,d
          result = "16";
          break;
        case 0xE9:  // JP (HL)
          result = "4";
          break;
        case 0xEA:  // LD (nn),A
          result = "16";
          break;
        // {0xEB, }, // XX
        // {0xEC, }, // XX
        // {0xED, }, // XX
        case 0xEE:  // XOR n
          result = "8";
          break;
        case 0xEF:  // RST 28
          result = "16";
          break;
        case 0xF0:  // LDH A,(n)
          result = "12";
          break;
        case 0xF1:  // POP AF
          result = "12";
          break;
        case 0xF2:  // LDH A, (C)
          result = "8";
          break;
        case 0xF3:  // DI
          result = "4";
          break;
        // {0xF4, }, // XX
        case 0xF5:  // PUSH AF
          result = "16";
          break;
        case 0xF6:  // OR n
          result = "8";
          break;
        case 0xF7:  // RST 30
          result = "16";
          break;
        case 0xF8:  // LDHL SP,d
          result = "12";
          break;
        case 0xF9:  // LD SP,HL
          result = "8";
          break;
        case 0xFA:  // LD A,(nn)
          result = "16";
          break;
        case 0xFB:  // EI
          result = "4";
          break;
        // {0xFC, }, // XX
        // {0xFD, }, // XX
        case 0xFE:  // CP n
          result = "8";
          break;
        case 0xFF: // RST 38
          result = "16";
          break;
        default:
          result = "0";
          break;
      }

      return result;
    }
  }
}
