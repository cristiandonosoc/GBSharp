using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUCBInstructionLengths
  {
    internal static byte Get(byte opcode)
    {
      byte result;
      switch (opcode)
      {
        case 0x00:  // RLC B
          result = 2;
          break;
        case 0x01:  // RLC C
          result = 2;
          break;
        case 0x02:  // RLC D
          result = 2;
          break;
        case 0x03:  // RLC E
          result = 2;
          break;
        case 0x04:  // RLC H
          result = 2;
          break;
        case 0x05:  // RLC L
          result = 2;
          break;
        case 0x06:  // RLC (HL)
          result = 2;
          break;
        case 0x07:  // RLC A
          result = 2;
          break;
        case 0x08:  // RRC B
          result = 2;
          break;
        case 0x09:  // RRC C
          result = 2;
          break;
        case 0x0A:  // RRC D
          result = 2;
          break;
        case 0x0B:  // RRC E
          result = 2;
          break;
        case 0x0C:  // RRC H
          result = 2;
          break;
        case 0x0D:  // RRC L
          result = 2;
          break;
        case 0x0E:  // RRC (HL)
          result = 2;
          break;
        case 0x0F:  // RRC A
          result = 2;
          break;
        case 0x10:  // RL B
          result = 2;
          break;
        case 0x11:  // RL C
          result = 2;
          break;
        case 0x12:  // RL D
          result = 2;
          break;
        case 0x13:  // RL E
          result = 2;
          break;
        case 0x14:  // RL H
          result = 2;
          break;
        case 0x15:  // RL L
          result = 2;
          break;
        case 0x16:  // RL (HL)
          result = 2;
          break;
        case 0x17:  // RL A
          result = 2;
          break;
        case 0x18:  // RR B
          result = 2;
          break;
        case 0x19:  // RR C
          result = 2;
          break;
        case 0x1A:  // RR D
          result = 2;
          break;
        case 0x1B:  // RR E
          result = 2;
          break;
        case 0x1C:  // RR H
          result = 2;
          break;
        case 0x1D:  // RR L
          result = 2;
          break;
        case 0x1E:  // RR (HL)
          result = 2;
          break;
        case 0x1F:  // RR A
          result = 2;
          break;
        case 0x20:  // SLA B
          result = 2;
          break;
        case 0x21:  // SLA C
          result = 2;
          break;
        case 0x22:  // SLA D
          result = 2;
          break;
        case 0x23:  // SLA E
          result = 2;
          break;
        case 0x24:  // SLA H
          result = 2;
          break;
        case 0x25:  // SLA L
          result = 2;
          break;
        case 0x26:  // SLA (HL)
          result = 2;
          break;
        case 0x27:  // SLA A
          result = 2;
          break;
        case 0x28:  // SRA B
          result = 2;
          break;
        case 0x29:  // SRA C
          result = 2;
          break;
        case 0x2A:  // SRA D
          result = 2;
          break;
        case 0x2B:  // SRA E
          result = 2;
          break;
        case 0x2C:  // SRA H
          result = 2;
          break;
        case 0x2D:  // SRA L
          result = 2;
          break;
        case 0x2E:  // SRA (HL)
          result = 2;
          break;
        case 0x2F:  // SRA A
          result = 2;
          break;
        case 0x30:  // SWAP B
          result = 2;
          break;
        case 0x31:  // SWAP C
          result = 2;
          break;
        case 0x32:  // SWAP D
          result = 2;
          break;
        case 0x33:  // SWAP E
          result = 2;
          break;
        case 0x34:  // SWAP H
          result = 2;
          break;
        case 0x35:  // SWAP L
          result = 2;
          break;
        case 0x36:  // SWAP (HL)
          result = 2;
          break;
        case 0x37:  // SWAP A
          result = 2;
          break;
        case 0x38:  // SRL B
          result = 2;
          break;
        case 0x39:  // SRL C
          result = 2;
          break;
        case 0x3A:  // SRL D
          result = 2;
          break;
        case 0x3B:  // SRL E
          result = 2;
          break;
        case 0x3C:  // SRL H
          result = 2;
          break;
        case 0x3D:  // SRL L
          result = 2;
          break;
        case 0x3E:  // SRL (HL)
          result = 2;
          break;
        case 0x3F:  // SRL A
          result = 2;
          break;
        case 0x40:  // BIT 0,B
          result = 2;
          break;
        case 0x41:  // BIT 0,C
          result = 2;
          break;
        case 0x42:  // BIT 0,D
          result = 2;
          break;
        case 0x43:  // BIT 0,E
          result = 2;
          break;
        case 0x44:  // BIT 0,H
          result = 2;
          break;
        case 0x45:  // BIT 0,L
          result = 2;
          break;
        case 0x46:  // BIT 0,(HL)
          result = 2;
          break;
        case 0x47:  // BIT 0,A
          result = 2;
          break;
        case 0x48:  // BIT 1,B
          result = 2;
          break;
        case 0x49:  // BIT 1,C
          result = 2;
          break;
        case 0x4A:  // BIT 1,D
          result = 2;
          break;
        case 0x4B:  // BIT 1,E
          result = 2;
          break;
        case 0x4C:  // BIT 1,H
          result = 2;
          break;
        case 0x4D:  // BIT 1,L
          result = 2;
          break;
        case 0x4E:  // BIT 1,(HL)
          result = 2;
          break;
        case 0x4F:  // BIT 1,A
          result = 2;
          break;
        case 0x50:  // BIT 2,B
          result = 2;
          break;
        case 0x51:  // BIT 2,C
          result = 2;
          break;
        case 0x52:  // BIT 2,D
          result = 2;
          break;
        case 0x53:  // BIT 2,E
          result = 2;
          break;
        case 0x54:  // BIT 2,H
          result = 2;
          break;
        case 0x55:  // BIT 2,L
          result = 2;
          break;
        case 0x56:  // BIT 2,(HL)
          result = 2;
          break;
        case 0x57:  // BIT 2,A
          result = 2;
          break;
        case 0x58:  // BIT 3,B
          result = 2;
          break;
        case 0x59:  // BIT 3,C
          result = 2;
          break;
        case 0x5A:  // BIT 3,D
          result = 2;
          break;
        case 0x5B:  // BIT 3,E
          result = 2;
          break;
        case 0x5C:  // BIT 3,H
          result = 2;
          break;
        case 0x5D:  // BIT 3,L
          result = 2;
          break;
        case 0x5E:  // BIT 3,(HL)
          result = 2;
          break;
        case 0x5F:  // BIT 3,A
          result = 2;
          break;
        case 0x60:  // BIT 4,B
          result = 2;
          break;
        case 0x61:  // BIT 4,C
          result = 2;
          break;
        case 0x62:  // BIT 4,D
          result = 2;
          break;
        case 0x63:  // BIT 4,E
          result = 2;
          break;
        case 0x64:  // BIT 4,H
          result = 2;
          break;
        case 0x65:  // BIT 4,L
          result = 2;
          break;
        case 0x66:  // BIT 4,(HL)
          result = 2;
          break;
        case 0x67:  // BIT 4,A
          result = 2;
          break;
        case 0x68:  // BIT 5,B
          result = 2;
          break;
        case 0x69:  // BIT 5,C
          result = 2;
          break;
        case 0x6A:  // BIT 5,D
          result = 2;
          break;
        case 0x6B:  // BIT 5,E
          result = 2;
          break;
        case 0x6C:  // BIT 5,H
          result = 2;
          break;
        case 0x6D:  // BIT 5,L
          result = 2;
          break;
        case 0x6E:  // BIT 5,(HL)
          result = 2;
          break;
        case 0x6F:  // BIT 5,A
          result = 2;
          break;
        case 0x70:  // BIT 6,B
          result = 2;
          break;
        case 0x71:  // BIT 6,C
          result = 2;
          break;
        case 0x72:  // BIT 6,D
          result = 2;
          break;
        case 0x73:  // BIT 6,E
          result = 2;
          break;
        case 0x74:  // BIT 6,H
          result = 2;
          break;
        case 0x75:  // BIT 6,L
          result = 2;
          break;
        case 0x76:  // BIT 6,(HL)
          result = 2;
          break;
        case 0x77:  // BIT 6,A
          result = 2;
          break;
        case 0x78:  // BIT 7,B
          result = 2;
          break;
        case 0x79:  // BIT 7,C
          result = 2;
          break;
        case 0x7A:  // BIT 7,D
          result = 2;
          break;
        case 0x7B:  // BIT 7,E
          result = 2;
          break;
        case 0x7C:  // BIT 7,H
          result = 2;
          break;
        case 0x7D:  // BIT 7,L
          result = 2;
          break;
        case 0x7E:  // BIT 7,(HL)
          result = 2;
          break;
        case 0x7F:  // BIT 7,A
          result = 2;
          break;
        case 0x80:  // RES 0,B
          result = 2;
          break;
        case 0x81:  // RES 0,C
          result = 2;
          break;
        case 0x82:  // RES 0,D
          result = 2;
          break;
        case 0x83:  // RES 0,E
          result = 2;
          break;
        case 0x84:  // RES 0,H
          result = 2;
          break;
        case 0x85:  // RES 0,L
          result = 2;
          break;
        case 0x86:  // RES 0,(HL)
          result = 2;
          break;
        case 0x87:  // RES 0,A
          result = 2;
          break;
        case 0x88:  // RES 1,B
          result = 2;
          break;
        case 0x89:  // RES 1,C
          result = 2;
          break;
        case 0x8A:  // RES 1,D
          result = 2;
          break;
        case 0x8B:  // RES 1,E
          result = 2;
          break;
        case 0x8C:  // RES 1,H
          result = 2;
          break;
        case 0x8D:  // RES 1,L
          result = 2;
          break;
        case 0x8E:  // RES 1,(HL)
          result = 2;
          break;
        case 0x8F:  // RES 1,A
          result = 2;
          break;
        case 0x90:  // RES 2,B
          result = 2;
          break;
        case 0x91:  // RES 2,C
          result = 2;
          break;
        case 0x92:  // RES 2,D
          result = 2;
          break;
        case 0x93:  // RES 2,E
          result = 2;
          break;
        case 0x94:  // RES 2,H
          result = 2;
          break;
        case 0x95:  // RES 2,L
          result = 2;
          break;
        case 0x96:  // RES 2,(HL)
          result = 2;
          break;
        case 0x97:  // RES 2,A
          result = 2;
          break;
        case 0x98:  // RES 3,B
          result = 2;
          break;
        case 0x99:  // RES 3,C
          result = 2;
          break;
        case 0x9A:  // RES 3,D
          result = 2;
          break;
        case 0x9B:  // RES 3,E
          result = 2;
          break;
        case 0x9C:  // RES 3,H
          result = 2;
          break;
        case 0x9D:  // RES 3,L
          result = 2;
          break;
        case 0x9E:  // RES 3,(HL)
          result = 2;
          break;
        case 0x9F:  // RES 3,A
          result = 2;
          break;
        case 0xA0:  // RES 4,B
          result = 2;
          break;
        case 0xA1:  // RES 4,C
          result = 2;
          break;
        case 0xA2:  // RES 4,D
          result = 2;
          break;
        case 0xA3:  // RES 4,E
          result = 2;
          break;
        case 0xA4:  // RES 4,H
          result = 2;
          break;
        case 0xA5:  // RES 4,L
          result = 2;
          break;
        case 0xA6:  // RES 4,(HL)
          result = 2;
          break;
        case 0xA7:  // RES 4,A
          result = 2;
          break;
        case 0xA8:  // RES 5,B
          result = 2;
          break;
        case 0xA9:  // RES 5,C
          result = 2;
          break;
        case 0xAA:  // RES 5,D
          result = 2;
          break;
        case 0xAB:  // RES 5,E
          result = 2;
          break;
        case 0xAC:  // RES 5,H
          result = 2;
          break;
        case 0xAD:  // RES 5,L
          result = 2;
          break;
        case 0xAE:  // RES 5,(HL)
          result = 2;
          break;
        case 0xAF:  // RES 5,A
          result = 2;
          break;
        case 0xB0:  // RES 6,B
          result = 2;
          break;
        case 0xB1:  // RES 6,C
          result = 2;
          break;
        case 0xB2:  // RES 6,D
          result = 2;
          break;
        case 0xB3:  // RES 6,E
          result = 2;
          break;
        case 0xB4:  // RES 6,H
          result = 2;
          break;
        case 0xB5:  // RES 6,L
          result = 2;
          break;
        case 0xB6:  // RES 6,(HL)
          result = 2;
          break;
        case 0xB7:  // RES 6,A
          result = 2;
          break;
        case 0xB8:  // RES 7,B
          result = 2;
          break;
        case 0xB9:  // RES 7,C
          result = 2;
          break;
        case 0xBA:  // RES 7,D
          result = 2;
          break;
        case 0xBB:  // RES 7,E
          result = 2;
          break;
        case 0xBC:  // RES 7,H
          result = 2;
          break;
        case 0xBD:  // RES 7,L
          result = 2;
          break;
        case 0xBE:  // RES 7,(HL)
          result = 2;
          break;
        case 0xBF:  // RES 7,A
          result = 2;
          break;
        case 0xC0:  // SET 0,B
          result = 2;
          break;
        case 0xC1:  // SET 0,C
          result = 2;
          break;
        case 0xC2:  // SET 0,D
          result = 2;
          break;
        case 0xC3:  // SET 0,E
          result = 2;
          break;
        case 0xC4:  // SET 0,H
          result = 2;
          break;
        case 0xC5:  // SET 0,L
          result = 2;
          break;
        case 0xC6:  // SET 0,(HL)
          result = 2;
          break;
        case 0xC7:  // SET 0,A
          result = 2;
          break;
        case 0xC8:  // SET 1,B
          result = 2;
          break;
        case 0xC9:  // SET 1,C
          result = 2;
          break;
        case 0xCA:  // SET 1,D
          result = 2;
          break;
        case 0xCB:  // SET 1,E
          result = 2;
          break;
        case 0xCC:  // SET 1,H
          result = 2;
          break;
        case 0xCD:  // SET 1,L
          result = 2;
          break;
        case 0xCE:  // SET 1,(HL)
          result = 2;
          break;
        case 0xCF:  // SET 1,A
          result = 2;
          break;
        case 0xD0:  // SET 2,B
          result = 2;
          break;
        case 0xD1:  // SET 2,C
          result = 2;
          break;
        case 0xD2:  // SET 2,D
          result = 2;
          break;
        case 0xD3:  // SET 2,E
          result = 2;
          break;
        case 0xD4:  // SET 2,H
          result = 2;
          break;
        case 0xD5:  // SET 2,L
          result = 2;
          break;
        case 0xD6:  // SET 2,(HL)
          result = 2;
          break;
        case 0xD7:  // SET 2,A
          result = 2;
          break;
        case 0xD8:  // SET 3,B
          result = 2;
          break;
        case 0xD9:  // SET 3,C
          result = 2;
          break;
        case 0xDA:  // SET 3,D
          result = 2;
          break;
        case 0xDB:  // SET 3,E
          result = 2;
          break;
        case 0xDC:  // SET 3,H
          result = 2;
          break;
        case 0xDD:  // SET 3,L
          result = 2;
          break;
        case 0xDE:  // SET 3,(HL)
          result = 2;
          break;
        case 0xDF:  // SET 3,A
          result = 2;
          break;
        case 0xE0:  // SET 4,B
          result = 2;
          break;
        case 0xE1:  // SET 4,C
          result = 2;
          break;
        case 0xE2:  // SET 4,D
          result = 2;
          break;
        case 0xE3:  // SET 4,E
          result = 2;
          break;
        case 0xE4:  // SET 4,H
          result = 2;
          break;
        case 0xE5:  // SET 4,L
          result = 2;
          break;
        case 0xE6:  // SET 4,(HL)
          result = 2;
          break;
        case 0xE7:  // SET 4,A
          result = 2;
          break;
        case 0xE8:  // SET 5,B
          result = 2;
          break;
        case 0xE9:  // SET 5,C
          result = 2;
          break;
        case 0xEA:  // SET 5,D
          result = 2;
          break;
        case 0xEB:  // SET 5,E
          result = 2;
          break;
        case 0xEC:  // SET 5,H
          result = 2;
          break;
        case 0xED:  // SET 5,L
          result = 2;
          break;
        case 0xEE:  // SET 5,(HL)
          result = 2;
          break;
        case 0xEF:  // SET 5,A
          result = 2;
          break;
        case 0xF0:  // SET 6,B
          result = 2;
          break;
        case 0xF1:  // SET 6,C
          result = 2;
          break;
        case 0xF2:  // SET 6,D
          result = 2;
          break;
        case 0xF3:  // SET 6,E
          result = 2;
          break;
        case 0xF4:  // SET 6,H
          result = 2;
          break;
        case 0xF5:  // SET 6,L
          result = 2;
          break;
        case 0xF6:  // SET 6,(HL)
          result = 2;
          break;
        case 0xF7:  // SET 6,A
          result = 2;
          break;
        case 0xF8:  // SET 7,B
          result = 2;
          break;
        case 0xF9:  // SET 7,C
          result = 2;
          break;
        case 0xFA:  // SET 7,D
          result = 2;
          break;
        case 0xFB:  // SET 7,E
          result = 2;
          break;
        case 0xFC:  // SET 7,H
          result = 2;
          break;
        case 0xFD:  // SET 7,L
          result = 2;
          break;
        case 0xFE:  // SET 7,(HL)
          result = 2;
          break;
        case 0xFF: // SET 7,A
          result = 2;
          break;
        default:
          throw new InvalidProgramException("Invalid Opcode");
      }

      return result;
    }
  }
}
