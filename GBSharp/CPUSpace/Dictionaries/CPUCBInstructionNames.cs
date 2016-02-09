using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  public class CPUCBInstructionNames
  {
    public static string Get(byte opcode)
    {
      string result;
      switch(opcode)
      {
        case 0x00: // RLC B
          result = "RLC B"; 
          break;
        case 0x01:  // RLC C
          result = "RLC C";
          break;
        case 0x02:  // RLC D
          result = "RLC D";
          break;
        case 0x03:  // RLC E
          result = "RLC E";
          break;
        case 0x04:  // RLC H
          result = "RLC H";
          break;
        case 0x05:  // RLC L
          result = "RLC L";
          break;
        case 0x06:  // RLC (HL)
          result = "RLC (HL)";
          break;
        case 0x07:  // RLC A
          result = "RLC A";
          break;
        case 0x08:  // RRC B
          result = "RRC B";
          break;
        case 0x09:  // RRC C
          result = "RRC C";
          break;
        case 0x0A:  // RRC D
          result = "RRC D";
          break;
        case 0x0B:  // RRC E
          result = "RRC E";
          break;
        case 0x0C:  // RRC H
          result = "RRC H";
          break;
        case 0x0D:  // RRC L
          result = "RRC L";
          break;
        case 0x0E:  // RRC (HL)
          result = "RRC (HL)";
          break;
        case 0x0F:  // RRC A
          result = "RRC A";
          break;
        case 0x10:  // RL B
          result = "RL B";
          break;
        case 0x11:  // RL C
          result = "RL C";
          break;
        case 0x12:  // RL D
          result = "RL D";
          break;
        case 0x13:  // RL E
          result = "RL E";
          break;
        case 0x14:  // RL H
          result = "RL H";
          break;
        case 0x15:  // RL L
          result = "RL L";
          break;
        case 0x16:  // RL (HL)
          result = "RL (HL)";
          break;
        case 0x17:  // RL A
          result = "RL A";
          break;
        case 0x18:  // RR B
          result = "RR B";
          break;
        case 0x19:  // RR C
          result = "RR C";
          break;
        case 0x1A:  // RR D
          result = "RR D";
          break;
        case 0x1B:  // RR E
          result = "RR E";
          break;
        case 0x1C:  // RR H
          result = "RR H";
          break;
        case 0x1D:  // RR L
          result = "RR L";
          break;
        case 0x1E:  // RR (HL)
          result = "RR (HL)";
          break;
        case 0x1F:  // RR A
          result = "RR A";
          break;
        case 0x20:  // SLA B
          result = "SLA B";
          break;
        case 0x21:  // SLA C
          result = "SLA C";
          break;
        case 0x22:  // SLA D
          result = "SLA D";
          break;
        case 0x23:  // SLA E
          result = "SLA E";
          break;
        case 0x24:  // SLA H
          result = "SLA H";
          break;
        case 0x25:  // SLA L
          result = "SLA L";
          break;
        case 0x26:  // SLA (HL)
          result = "SLA (HL)";
          break;
        case 0x27:  // SLA A
          result = "SLA A";
          break;
        case 0x28:  // SRA B
          result = "SRA B";
          break;
        case 0x29:  // SRA C
          result = "SRA C";
          break;
        case 0x2A:  // SRA D
          result = "SRA D";
          break;
        case 0x2B:  // SRA E
          result = "SRA E";
          break;
        case 0x2C:  // SRA H
          result = "SRA H";
          break;
        case 0x2D:  // SRA L
          result = "SRA L";
          break;
        case 0x2E:  // SRA (HL)
          result = "SRA (HL)";
          break;
        case 0x2F:  // SRA A
          result = "SRA A";
          break;
        case 0x30:  // SWAP B
          result = "SWAP B";
          break;
        case 0x31:  // SWAP C
          result = "SWAP C";
          break;
        case 0x32:  // SWAP D
          result = "SWAP D";
          break;
        case 0x33:  // SWAP E
          result = "SWAP E";
          break;
        case 0x34:  // SWAP H
          result = "SWAP H";
          break;
        case 0x35:  // SWAP L
          result = "SWAP L";
          break;
        case 0x36:  // SWAP (HL)
          result = "SWAP (HL)";
          break;
        case 0x37:  // SWAP A
          result = "SWAP A";
          break;
        case 0x38:  // SRL B
          result = "SRL B";
          break;
        case 0x39:  // SRL C
          result = "SRL C";
          break;
        case 0x3A:  // SRL D
          result = "SRL D";
          break;
        case 0x3B:  // SRL E
          result = "SRL E";
          break;
        case 0x3C:  // SRL H
          result = "SRL H";
          break;
        case 0x3D:  // SRL L
          result = "SRL L";
          break;
        case 0x3E:  // SRL (HL)
          result = "SRL (HL)";
          break;
        case 0x3F:  // SRL A
          result = "SRL A";
          break;
        case 0x40:  // BIT 0,B
          result = "BIT 0,B";
          break;
        case 0x41:  // BIT 0,C
          result = "BIT 0,C";
          break;
        case 0x42:  // BIT 0,D
          result = "BIT 0,D";
          break;
        case 0x43:  // BIT 0,E
          result = "BIT 0,E";
          break;
        case 0x44:  // BIT 0,H
          result = "BIT 0,H";
          break;
        case 0x45:  // BIT 0,L
          result = "BIT 0,L";
          break;
        case 0x46:  // BIT 0,(HL)
          result = "BIT 0,(HL)";
          break;
        case 0x47:  // BIT 0,A
          result = "BIT 0,A";
          break;
        case 0x48:  // BIT 1,B
          result = "BIT 1,B";
          break;
        case 0x49:  // BIT 1,C
          result = "BIT 1,C";
          break;
        case 0x4A:  // BIT 1,D
          result = "BIT 1,D";
          break;
        case 0x4B:  // BIT 1,E
          result = "BIT 1,E";
          break;
        case 0x4C:  // BIT 1,H
          result = "BIT 1,H";
          break;
        case 0x4D:  // BIT 1,L
          result = "BIT 1,L";
          break;
        case 0x4E:  // BIT 1,(HL)
          result = "BIT 1,(HL)";
          break;
        case 0x4F:  // BIT 1,A
          result = "BIT 1,A";
          break;
        case 0x50:  // BIT 2,B
          result = "BIT 2,B";
          break;
        case 0x51:  // BIT 2,C
          result = "BIT 2,C";
          break;
        case 0x52:  // BIT 2,D
          result = "BIT 2,D";
          break;
        case 0x53:  // BIT 2,E
          result = "BIT 2,E";
          break;
        case 0x54:  // BIT 2,H
          result = "BIT 2,H";
          break;
        case 0x55:  // BIT 2,L
          result = "BIT 2,L";
          break;
        case 0x56:  // BIT 2,(HL)
          result = "BIT 2,(HL)";
          break;
        case 0x57:  // BIT 2,A
          result = "BIT 2,A";
          break;
        case 0x58:  // BIT 3,B
          result = "BIT 3,B";
          break;
        case 0x59:  // BIT 3,C
          result = "BIT 3,C";
          break;
        case 0x5A:  // BIT 3,D
          result = "BIT 3,D";
          break;
        case 0x5B:  // BIT 3,E
          result = "BIT 3,E";
          break;
        case 0x5C:  // BIT 3,H
          result = "BIT 3,H";
          break;
        case 0x5D:  // BIT 3,L
          result = "BIT 3,L";
          break;
        case 0x5E:  // BIT 3,(HL)
          result = "BIT 3,(HL)";
          break;
        case 0x5F:  // BIT 3,A
          result = "BIT 3,A";
          break;
        case 0x60:  // BIT 4,B
          result = "BIT 4,B";
          break;
        case 0x61:  // BIT 4,C
          result = "BIT 4,C";
          break;
        case 0x62:  // BIT 4,D
          result = "BIT 4,D";
          break;
        case 0x63:  // BIT 4,E
          result = "BIT 4,E";
          break;
        case 0x64:  // BIT 4,H
          result = "BIT 4,H";
          break;
        case 0x65:  // BIT 4,L
          result = "BIT 4,L";
          break;
        case 0x66:  // BIT 4,(HL)
          result = "BIT 4,(HL)";
          break;
        case 0x67:  // BIT 4,A
          result = "BIT 4,A";
          break;
        case 0x68:  // BIT 5,B
          result = "BIT 5,B";
          break;
        case 0x69:  // BIT 5,C
          result = "BIT 5,C";
          break;
        case 0x6A:  // BIT 5,D
          result = "BIT 5,D";
          break;
        case 0x6B:  // BIT 5,E
          result = "BIT 5,E";
          break;
        case 0x6C:  // BIT 5,H
          result = "BIT 5,H";
          break;
        case 0x6D:  // BIT 5,L
          result = "BIT 5,L";
          break;
        case 0x6E:  // BIT 5,(HL)
          result = "BIT 5,(HL)";
          break;
        case 0x6F:  // BIT 5,A
          result = "BIT 5,A";
          break;
        case 0x70:  // BIT 6,B
          result = "BIT 6,B";
          break;
        case 0x71:  // BIT 6,C
          result = "BIT 6,C";
          break;
        case 0x72:  // BIT 6,D
          result = "BIT 6,D";
          break;
        case 0x73:  // BIT 6,E
          result = "BIT 6,E";
          break;
        case 0x74:  // BIT 6,H
          result = "BIT 6,H";
          break;
        case 0x75:  // BIT 6,L
          result = "BIT 6,L";
          break;
        case 0x76:  // BIT 6,(HL)
          result = "BIT 6,(HL)";
          break;
        case 0x77:  // BIT 6,A
          result = "BIT 6,A";
          break;
        case 0x78:  // BIT 7,B
          result = "BIT 7,B";
          break;
        case 0x79:  // BIT 7,C
          result = "BIT 7,C";
          break;
        case 0x7A:  // BIT 7,D
          result = "BIT 7,D";
          break;
        case 0x7B:  // BIT 7,E
          result = "BIT 7,E";
          break;
        case 0x7C:  // BIT 7,H
          result = "BIT 7,H";
          break;
        case 0x7D:  // BIT 7,L
          result = "BIT 7,L";
          break;
        case 0x7E:  // BIT 7,(HL)
          result = "BIT 7,(HL)";
          break;
        case 0x7F:  // BIT 7,A
          result = "BIT 7,A";
          break;
        case 0x80:  // RES 0,B
          result = "RES 0,B";
          break;
        case 0x81:  // RES 0,C
          result = "RES 0,C";
          break;
        case 0x82:  // RES 0,D
          result = "RES 0,D";
          break;
        case 0x83:  // RES 0,E
          result = "RES 0,E";
          break;
        case 0x84:  // RES 0,H
          result = "RES 0,H";
          break;
        case 0x85:  // RES 0,L
          result = "RES 0,L";
          break;
        case 0x86:  // RES 0,(HL)
          result = "RES 0,(HL)";
          break;
        case 0x87:  // RES 0,A
          result = "RES 0,A";
          break;
        case 0x88:  // RES 1,B
          result = "RES 1,B";
          break;
        case 0x89:  // RES 1,C
          result = "RES 1,C";
          break;
        case 0x8A:  // RES 1,D
          result = "RES 1,D";
          break;
        case 0x8B:  // RES 1,E
          result = "RES 1,E";
          break;
        case 0x8C:  // RES 1,H
          result = "RES 1,H";
          break;
        case 0x8D:  // RES 1,L
          result = "RES 1,L";
          break;
        case 0x8E:  // RES 1,(HL)
          result = "RES 1,(HL)";
          break;
        case 0x8F:  // RES 1,A
          result = "RES 1,A";
          break;
        case 0x90:  // RES 2,B
          result = "RES 2,B";
          break;
        case 0x91:  // RES 2,C
          result = "RES 2,C";
          break;
        case 0x92:  // RES 2,D
          result = "RES 2,D";
          break;
        case 0x93:  // RES 2,E
          result = "RES 2,E";
          break;
        case 0x94:  // RES 2,H
          result = "RES 2,H";
          break;
        case 0x95:  // RES 2,L
          result = "RES 2,L";
          break;
        case 0x96:  // RES 2,(HL)
          result = "RES 2,(HL)";
          break;
        case 0x97:  // RES 2,A
          result = "RES 2,A";
          break;
        case 0x98:  // RES 3,B
          result = "RES 3,B";
          break;
        case 0x99:  // RES 3,C
          result = "RES 3,C";
          break;
        case 0x9A:  // RES 3,D
          result = "RES 3,D";
          break;
        case 0x9B:  // RES 3,E
          result = "RES 3,E";
          break;
        case 0x9C:  // RES 3,H
          result = "RES 3,H";
          break;
        case 0x9D:  // RES 3,L
          result = "RES 3,L";
          break;
        case 0x9E:  // RES 3,(HL)
          result = "RES 3,(HL)";
          break;
        case 0x9F:  // RES 3,A
          result = "RES 3,A";
          break;
        case 0xA0:  // RES 4,B
          result = "RES 4,B";
          break;
        case 0xA1:  // RES 4,C
          result = "RES 4,C";
          break;
        case 0xA2:  // RES 4,D
          result = "RES 4,D";
          break;
        case 0xA3:  // RES 4,E
          result = "RES 4,E";
          break;
        case 0xA4:  // RES 4,H
          result = "RES 4,H";
          break;
        case 0xA5:  // RES 4,L
          result = "RES 4,L";
          break;
        case 0xA6:  // RES 4,(HL)
          result = "RES 4,(HL)";
          break;
        case 0xA7:  // RES 4,A
          result = "RES 4,A";
          break;
        case 0xA8:  // RES 5,B
          result = "RES 5,B";
          break;
        case 0xA9:  // RES 5,C
          result = "RES 5,C";
          break;
        case 0xAA:  // RES 5,D
          result = "RES 5,D";
          break;
        case 0xAB:  // RES 5,E
          result = "RES 5,E";
          break;
        case 0xAC:  // RES 5,H
          result = "RES 5,H";
          break;
        case 0xAD:  // RES 5,L
          result = "RES 5,L";
          break;
        case 0xAE:  // RES 5,(HL)
          result = "RES 5,(HL)";
          break;
        case 0xAF:  // RES 5,A
          result = "RES 5,A";
          break;
        case 0xB0:  // RES 6,B
          result = "RES 6,B";
          break;
        case 0xB1:  // RES 6,C
          result = "RES 6,C";
          break;
        case 0xB2:  // RES 6,D
          result = "RES 6,D";
          break;
        case 0xB3:  // RES 6,E
          result = "RES 6,E";
          break;
        case 0xB4:  // RES 6,H
          result = "RES 6,H";
          break;
        case 0xB5:  // RES 6,L
          result = "RES 6,L";
          break;
        case 0xB6:  // RES 6,(HL)
          result = "RES 6,(HL)";
          break;
        case 0xB7:  // RES 6,A
          result = "RES 6,A";
          break;
        case 0xB8:  // RES 7,B
          result = "RES 7,B";
          break;
        case 0xB9:  // RES 7,C
          result = "RES 7,C";
          break;
        case 0xBA:  // RES 7,D
          result = "RES 7,D";
          break;
        case 0xBB:  // RES 7,E
          result = "RES 7,E";
          break;
        case 0xBC:  // RES 7,H
          result = "RES 7,H";
          break;
        case 0xBD:  // RES 7,L
          result = "RES 7,L";
          break;
        case 0xBE:  // RES 7,(HL)
          result = "RES 7,(HL)";
          break;
        case 0xBF:  // RES 7,A
          result = "RES 7,A";
          break;
        case 0xC0:  // SET 0,B
          result = "SET 0,B";
          break;
        case 0xC1:  // SET 0,C
          result = "SET 0,C";
          break;
        case 0xC2:  // SET 0,D
          result = "SET 0,D";
          break;
        case 0xC3:  // SET 0,E
          result = "SET 0,E";
          break;
        case 0xC4:  // SET 0,H
          result = "SET 0,H";
          break;
        case 0xC5:  // SET 0,L
          result = "SET 0,L";
          break;
        case 0xC6:  // SET 0,(HL)
          result = "SET 0,(HL)";
          break;
        case 0xC7:  // SET 0,A
          result = "SET 0,A";
          break;
        case 0xC8:  // SET 1,B
          result = "SET 1,B";
          break;
        case 0xC9:  // SET 1,C
          result = "SET 1,C";
          break;
        case 0xCA:  // SET 1,D
          result = "SET 1,D";
          break;
        case 0xCB:  // SET 1,E
          result = "SET 1,E";
          break;
        case 0xCC:  // SET 1,H
          result = "SET 1,H";
          break;
        case 0xCD:  // SET 1,L
          result = "SET 1,L";
          break;
        case 0xCE:  // SET 1,(HL)
          result = "SET 1,(HL)";
          break;
        case 0xCF:  // SET 1,A
          result = "SET 1,A";
          break;
        case 0xD0:  // SET 2,B
          result = "SET 2,B";
          break;
        case 0xD1:  // SET 2,C
          result = "SET 2,C";
          break;
        case 0xD2:  // SET 2,D
          result = "SET 2,D";
          break;
        case 0xD3:  // SET 2,E
          result = "SET 2,E";
          break;
        case 0xD4:  // SET 2,H
          result = "SET 2,H";
          break;
        case 0xD5:  // SET 2,L
          result = "SET 2,L";
          break;
        case 0xD6:  // SET 2,(HL)
          result = "SET 2,(HL)";
          break;
        case 0xD7:  // SET 2,A
          result = "SET 2,A";
          break;
        case 0xD8:  // SET 3,B
          result = "SET 3,B";
          break;
        case 0xD9:  // SET 3,C
          result = "SET 3,C";
          break;
        case 0xDA:  // SET 3,D
          result = "SET 3,D";
          break;
        case 0xDB:  // SET 3,E
          result = "SET 3,E";
          break;
        case 0xDC:  // SET 3,H
          result = "SET 3,H";
          break;
        case 0xDD:  // SET 3,L
          result = "SET 3,L";
          break;
        case 0xDE:  // SET 3,(HL)
          result = "SET 3,(HL)";
          break;
        case 0xDF:  // SET 3,A
          result = "SET 3,A";
          break;
        case 0xE0:  // SET 4,B
          result = "SET 4,B";
          break;
        case 0xE1:  // SET 4,C
          result = "SET 4,C";
          break;
        case 0xE2:  // SET 4,D
          result = "SET 4,D";
          break;
        case 0xE3:  // SET 4,E
          result = "SET 4,E";
          break;
        case 0xE4:  // SET 4,H
          result = "SET 4,H";
          break;
        case 0xE5:  // SET 4,L
          result = "SET 4,L";
          break;
        case 0xE6:  // SET 4,(HL)
          result = "SET 4,(HL)";
          break;
        case 0xE7:  // SET 4,A
          result = "SET 4,A";
          break;
        case 0xE8:  // SET 5,B
          result = "SET 5,B";
          break;
        case 0xE9:  // SET 5,C
          result = "SET 5,C";
          break;
        case 0xEA:  // SET 5,D
          result = "SET 5,D";
          break;
        case 0xEB:  // SET 5,E
          result = "SET 5,E";
          break;
        case 0xEC:  // SET 5,H
          result = "SET 5,H";
          break;
        case 0xED:  // SET 5,L
          result = "SET 5,L";
          break;
        case 0xEE:  // SET 5,(HL)
          result = "SET 5,(HL)";
          break;
        case 0xEF:  // SET 5,A
          result = "SET 5,A";
          break;
        case 0xF0:  // SET 6,B
          result = "SET 6,B";
          break;
        case 0xF1:  // SET 6,C
          result = "SET 6,C";
          break;
        case 0xF2:  // SET 6,D
          result = "SET 6,D";
          break;
        case 0xF3:  // SET 6,E
          result = "SET 6,E";
          break;
        case 0xF4:  // SET 6,H
          result = "SET 6,H";
          break;
        case 0xF5:  // SET 6,L
          result = "SET 6,L";
          break;
        case 0xF6:  // SET 6,(HL)
          result = "SET 6,(HL)";
          break;
        case 0xF7:  // SET 6,A
          result = "SET 6,A";
          break;
        case 0xF8:  // SET 7,B
          result = "SET 7,B";
          break;
        case 0xF9:  // SET 7,C
          result = "SET 7,C";
          break;
        case 0xFA:  // SET 7,D
          result = "SET 7,D";
          break;
        case 0xFB:  // SET 7,E
          result = "SET 7,E";
          break;
        case 0xFC:  // SET 7,H
          result = "SET 7,H";
          break;
        case 0xFD:  // SET 7,L
          result = "SET 7,L";
          break;
        case 0xFE:  // SET 7,(HL)
          result = "SET 7,(HL)";
          break;
        case 0xFF:  // SET 7,A
          result = "SET 7,A";
          break;
        default:
          throw new InvalidProgramException("Invalid Opcode");
      }

      return result;
    }
  }
}
