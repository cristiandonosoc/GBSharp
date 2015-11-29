using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUCBInstructionDescriptions
  {
    internal static string Get(byte opcode)
    {
      string result;
      switch(opcode)
      {
        case 0x00:  
          result = "RLC B: Rotate B left with carry";
          break;
        case 0x01:  
          result = "RLC C: Rotate C left with carry";
          break;
        case 0x02:  
          result = "RLC D: Rotate D left with carry";
          break;
        case 0x03:  
          result = "RLC E: Rotate E left with carry";
          break;
        case 0x04:  
          result = "RLC H: Rotate H left with carry";
          break;
        case 0x05:  
          result = "RLC L: Rotate L left with carry";
          break;
        case 0x06:  
          result = "RLC (HL): Rotate value pointed by HL left with carry";
          break;
        case 0x07:  
          result = "RLC A: Rotate A left with carry";
          break;
        case 0x08:  
          result = "RRC B: Rotate B right with carry";
          break;
        case 0x09:  
          result = "RRC C: Rotate C right with carry";
          break;
        case 0x0A:  
          result = "RRC D: Rotate D right with carry";
          break;
        case 0x0B:  
          result = "RRC E: Rotate E right with carry";
          break;
        case 0x0C:  
          result = "RRC H: Rotate H right with carry";
          break;
        case 0x0D:  
          result = "RRC L: Rotate L right with carry";
          break;
        case 0x0E:  
          result = "RRC (HL): Rotate value pointed by HL right with carry";
          break;
        case 0x0F:  
          result = "RRC A: Rotate A right with carry";
          break;
        case 0x10:  
          result = "RL B: Rotate B left";
          break;
        case 0x11:  
          result = "RL C: Rotate C left";
          break;
        case 0x12:  
          result = "RL D: Rotate D left";
          break;
        case 0x13:  
          result = "RL E: Rotate E left";
          break;
        case 0x14:  
          result = "RL H: Rotate H left";
          break;
        case 0x15:  
          result = "RL L: Rotate L left";
          break;
        case 0x16:  
          result = "RL (HL): Rotate value pointed by HL left";
          break;
        case 0x17:  
          result = "RL A: Rotate A left";
          break;
        case 0x18:  
          result = "RR B: Rotate B right";
          break;
        case 0x19:  
          result = "RR C: Rotate C right";
          break;
        case 0x1A:  
          result = "RR D: Rotate D right";
          break;
        case 0x1B:  
          result = "RR E: Rotate E right";
          break;
        case 0x1C:  
          result = "RR H: Rotate H right";
          break;
        case 0x1D:  
          result = "RR L: Rotate L right";
          break;
        case 0x1E:  
          result = "RR (HL): Rotate value pointed by HL right";
          break;
        case 0x1F:  
          result = "RR A: Rotate A right";
          break;
        case 0x20:  
          result = "SLA B: Shift B left preserving sign";
          break;
        case 0x21:  
          result = "SLA C: Shift C left preserving sign";
          break;
        case 0x22:  
          result = "SLA D: Shift D left preserving sign";
          break;
        case 0x23:  
          result = "SLA E: Shift E left preserving sign";
          break;
        case 0x24:  
          result = "SLA H: Shift H left preserving sign";
          break;
        case 0x25:  
          result = "SLA L: Shift L left preserving sign";
          break;
        case 0x26:  
          result = "SLA (HL): Shift value pointed by HL left preserving sign";
          break;
        case 0x27:  
          result = "SLA A: Shift A left preserving sign";
          break;
        case 0x28:  
          result = "SRA B: Shift B right preserving sign";
          break;
        case 0x29:  
          result = "SRA C: Shift C right preserving sign";
          break;
        case 0x2A:  
          result = "SRA D: Shift D right preserving sign";
          break;
        case 0x2B:  
          result = "SRA E: Shift E right preserving sign";
          break;
        case 0x2C:  
          result = "SRA H: Shift H right preserving sign";
          break;
        case 0x2D:  
          result = "SRA L: Shift L right preserving sign";
          break;
        case 0x2E:  
          result = "SRA (HL): Shift value pointed by HL right preserving sign";
          break;
        case 0x2F:  
          result = "SRA A: Shift A right preserving sign";
          break;
        case 0x30:  
          result = "SWAP B: Swap nybbles in B";
          break;
        case 0x31:  
          result = "SWAP C: Swap nybbles in C";
          break;
        case 0x32:  
          result = "SWAP D: Swap nybbles in D";
          break;
        case 0x33:  
          result = "SWAP E: Swap nybbles in E";
          break;
        case 0x34:  
          result = "SWAP H: Swap nybbles in H";
          break;
        case 0x35:  
          result = "SWAP L: Swap nybbles in L";
          break;
        case 0x36:  
          result = "SWAP (HL): Swap nybbles in value pointed by HL";
          break;
        case 0x37:  
          result = "SWAP A: Swap nybbles in A";
          break;
        case 0x38:  
          result = "SRL B: Shift B right";
          break;
        case 0x39:  
          result = "SRL C: Shift C right";
          break;
        case 0x3A:  
          result = "SRL D: Shift D right";
          break;
        case 0x3B:  
          result = "SRL E: Shift E right";
          break;
        case 0x3C:  
          result = "SRL H: Shift H right";
          break;
        case 0x3D:  
          result = "SRL L: Shift L right";
          break;
        case 0x3E:  
          result = "SRL (HL): Shift value pointed by HL right";
          break;
        case 0x3F:  
          result = "SRL A: Shift A right";
          break;
        case 0x40:  
          result = "BIT 0,B: Test bit 0 of B";
          break;
        case 0x41:  
          result = "BIT 0,C: Test bit 0 of C";
          break;
        case 0x42:  
          result = "BIT 0,D: Test bit 0 of D";
          break;
        case 0x43:  
          result = "BIT 0,E: Test bit 0 of E";
          break;
        case 0x44:  
          result = "BIT 0,H: Test bit 0 of H";
          break;
        case 0x45:  
          result = "BIT 0,L: Test bit 0 of L";
          break;
        case 0x46:  
          result = "BIT 0,(HL): Test bit 0 of value pointed by HL";
          break;
        case 0x47:  
          result = "BIT 0,A: Test bit 0 of A";
          break;
        case 0x48:  
          result = "BIT 1,B: Test bit 1 of B";
          break;
        case 0x49:  
          result = "BIT 1,C: Test bit 1 of C";
          break;
        case 0x4A:  
          result = "BIT 1,D: Test bit 1 of D";
          break;
        case 0x4B:  
          result = "BIT 1,E: Test bit 1 of E";
          break;
        case 0x4C:  
          result = "BIT 1,H: Test bit 1 of H";
          break;
        case 0x4D:  
          result = "BIT 1,L: Test bit 1 of L";
          break;
        case 0x4E:  
          result = "BIT 1,(HL): Test bit 1 of value pointed by HL";
          break;
        case 0x4F:  
          result = "BIT 1,A: Test bit 1 of A";
          break;
        case 0x50:  
          result = "BIT 2,B: Test bit 2 of B";
          break;
        case 0x51:  
          result = "BIT 2,C: Test bit 2 of C";
          break;
        case 0x52:  
          result = "BIT 2,D: Test bit 2 of D";
          break;
        case 0x53:  
          result = "BIT 2,E: Test bit 2 of E";
          break;
        case 0x54:  
          result = "BIT 2,H: Test bit 2 of H";
          break;
        case 0x55:  
          result = "BIT 2,L: Test bit 2 of L";
          break;
        case 0x56:  
          result = "BIT 2,(HL): Test bit 2 of value pointed by HL";
          break;
        case 0x57:  
          result = "BIT 2,A: Test bit 2 of A";
          break;
        case 0x58:  
          result = "BIT 3,B: Test bit 3 of B";
          break;
        case 0x59:  
          result = "BIT 3,C: Test bit 3 of C";
          break;
        case 0x5A:  
          result = "BIT 3,D: Test bit 3 of D";
          break;
        case 0x5B:  
          result = "BIT 3,E: Test bit 3 of E";
          break;
        case 0x5C:  
          result = "BIT 3,H: Test bit 3 of H";
          break;
        case 0x5D:  
          result = "BIT 3,L: Test bit 3 of L";
          break;
        case 0x5E:  
          result = "BIT 3,(HL): Test bit 3 of value pointed by HL";
          break;
        case 0x5F:  
          result = "BIT 3,A: Test bit 3 of A";
          break;
        case 0x60:  
          result = "BIT 4,B: Test bit 4 of B";
          break;
        case 0x61:  
          result = "BIT 4,C: Test bit 4 of C";
          break;
        case 0x62:  
          result = "BIT 4,D: Test bit 4 of D";
          break;
        case 0x63:  
          result = "BIT 4,E: Test bit 4 of E";
          break;
        case 0x64:  
          result = "BIT 4,H: Test bit 4 of H";
          break;
        case 0x65:  
          result = "BIT 4,L: Test bit 4 of L";
          break;
        case 0x66:  
          result = "BIT 4,(HL): Test bit 4 of value pointed by HL";
          break;
        case 0x67:  
          result = "BIT 4,A: Test bit 4 of A";
          break;
        case 0x68:  
          result = "BIT 5,B: Test bit 5 of B";
          break;
        case 0x69:  
          result = "BIT 5,C: Test bit 5 of C";
          break;
        case 0x6A:  
          result = "BIT 5,D: Test bit 5 of D";
          break;
        case 0x6B:  
          result = "BIT 5,E: Test bit 5 of E";
          break;
        case 0x6C:  
          result = "BIT 5,H: Test bit 5 of H";
          break;
        case 0x6D:  
          result = "BIT 5,L: Test bit 5 of L";
          break;
        case 0x6E:  
          result = "BIT 5,(HL): Test bit 5 of value pointed by HL";
          break;
        case 0x6F:  
          result = "BIT 5,A: Test bit 5 of A";
          break;
        case 0x70:  
          result = "BIT 6,B: Test bit 6 of B";
          break;
        case 0x71:  
          result = "BIT 6,C: Test bit 6 of C";
          break;
        case 0x72:  
          result = "BIT 6,D: Test bit 6 of D";
          break;
        case 0x73:  
          result = "BIT 6,E: Test bit 6 of E";
          break;
        case 0x74:  
          result = "BIT 6,H: Test bit 6 of H";
          break;
        case 0x75:  
          result = "BIT 6,L: Test bit 6 of L";
          break;
        case 0x76:  
          result = "BIT 6,(HL): Test bit 6 of value pointed by HL";
          break;
        case 0x77:  
          result = "BIT 6,A: Test bit 6 of A";
          break;
        case 0x78:  
          result = "BIT 7,B: Test bit 7 of B";
          break;
        case 0x79:  
          result = "BIT 7,C: Test bit 7 of C";
          break;
        case 0x7A:  
          result = "BIT 7,D: Test bit 7 of D";
          break;
        case 0x7B:  
          result = "BIT 7,E: Test bit 7 of E";
          break;
        case 0x7C:  
          result = "BIT 7,H: Test bit 7 of H";
          break;
        case 0x7D:  
          result = "BIT 7,L: Test bit 7 of L";
          break;
        case 0x7E:  
          result = "BIT 7,(HL): Test bit 7 of value pointed by HL";
          break;
        case 0x7F:  
          result = "BIT 7,A: Test bit 7 of A";
          break;
        case 0x80:  
          result = "RES 0,B: Clear (reset) bit 0 of B";
          break;
        case 0x81:  
          result = "RES 0,C: Clear (reset) bit 0 of C";
          break;
        case 0x82:  
          result = "RES 0,D: Clear (reset) bit 0 of D";
          break;
        case 0x83:  
          result = "RES 0,E: Clear (reset) bit 0 of E";
          break;
        case 0x84:  
          result = "RES 0,H: Clear (reset) bit 0 of H";
          break;
        case 0x85:  
          result = "RES 0,L: Clear (reset) bit 0 of L";
          break;
        case 0x86:  
          result = "RES 0,(HL): Clear (reset) bit 0 of value pointed by HL";
          break;
        case 0x87:  
          result = "RES 0,A: Clear (reset) bit 0 of A";
          break;
        case 0x88:  
          result = "RES 1,B: Clear (reset) bit 1 of B";
          break;
        case 0x89:  
          result = "RES 1,C: Clear (reset) bit 1 of C";
          break;
        case 0x8A:  
          result = "RES 1,D: Clear (reset) bit 1 of D";
          break;
        case 0x8B:  
          result = "RES 1,E: Clear (reset) bit 1 of E";
          break;
        case 0x8C:  
          result = "RES 1,H: Clear (reset) bit 1 of H";
          break;
        case 0x8D:  
          result = "RES 1,L: Clear (reset) bit 1 of L";
          break;
        case 0x8E:  
          result = "RES 1,(HL): Clear (reset) bit 1 of value pointed by HL";
          break;
        case 0x8F:  
          result = "RES 1,A: Clear (reset) bit 1 of A";
          break;
        case 0x90:  
          result = "RES 2,B: Clear (reset) bit 2 of B";
          break;
        case 0x91:  
          result = "RES 2,C: Clear (reset) bit 2 of C";
          break;
        case 0x92:  
          result = "RES 2,D: Clear (reset) bit 2 of D";
          break;
        case 0x93:  
          result = "RES 2,E: Clear (reset) bit 2 of E";
          break;
        case 0x94:  
          result = "RES 2,H: Clear (reset) bit 2 of H";
          break;
        case 0x95:  
          result = "RES 2,L: Clear (reset) bit 2 of L";
          break;
        case 0x96:  
          result = "RES 2,(HL): Clear (reset) bit 2 of value pointed by HL";
          break;
        case 0x97:  
          result = "RES 2,A: Clear (reset) bit 2 of A";
          break;
        case 0x98:  
          result = "RES 3,B: Clear (reset) bit 3 of B";
          break;
        case 0x99:  
          result = "RES 3,C: Clear (reset) bit 3 of C";
          break;
        case 0x9A:  
          result = "RES 3,D: Clear (reset) bit 3 of D";
          break;
        case 0x9B:  
          result = "RES 3,E: Clear (reset) bit 3 of E";
          break;
        case 0x9C:  
          result = "RES 3,H: Clear (reset) bit 3 of H";
          break;
        case 0x9D:  
          result = "RES 3,L: Clear (reset) bit 3 of L";
          break;
        case 0x9E:  
          result = "RES 3,(HL): Clear (reset) bit 3 of value pointed by HL";
          break;
        case 0x9F:  
          result = "RES 3,A: Clear (reset) bit 3 of A";
          break;
        case 0xA0:  
          result = "RES 4,B: Clear (reset) bit 4 of B";
          break;
        case 0xA1:  
          result = "RES 4,C: Clear (reset) bit 4 of C";
          break;
        case 0xA2:  
          result = "RES 4,D: Clear (reset) bit 4 of D";
          break;
        case 0xA3:  
          result = "RES 4,E: Clear (reset) bit 4 of E";
          break;
        case 0xA4:  
          result = "RES 4,H: Clear (reset) bit 4 of H";
          break;
        case 0xA5:  
          result = "RES 4,L: Clear (reset) bit 4 of L";
          break;
        case 0xA6:  
          result = "RES 4,(HL): Clear (reset) bit 4 of value pointed by HL";
          break;
        case 0xA7:  
          result = "RES 4,A: Clear (reset) bit 4 of A";
          break;
        case 0xA8:  
          result = "RES 5,B: Clear (reset) bit 5 of B";
          break;
        case 0xA9:  
          result = "RES 5,C: Clear (reset) bit 5 of C";
          break;
        case 0xAA:  
          result = "RES 5,D: Clear (reset) bit 5 of D";
          break;
        case 0xAB:  
          result = "RES 5,E: Clear (reset) bit 5 of E";
          break;
        case 0xAC:  
          result = "RES 5,H: Clear (reset) bit 5 of H";
          break;
        case 0xAD:  
          result = "RES 5,L: Clear (reset) bit 5 of L";
          break;
        case 0xAE:  
          result = "RES 5,(HL): Clear (reset) bit 5 of value pointed by HL";
          break;
        case 0xAF:  
          result = "RES 5,A: Clear (reset) bit 5 of A";
          break;
        case 0xB0:  
          result = "RES 6,B: Clear (reset) bit 6 of B";
          break;
        case 0xB1:  
          result = "RES 6,C: Clear (reset) bit 6 of C";
          break;
        case 0xB2:  
          result = "RES 6,D: Clear (reset) bit 6 of D";
          break;
        case 0xB3:  
          result = "RES 6,E: Clear (reset) bit 6 of E";
          break;
        case 0xB4:  
          result = "RES 6,H: Clear (reset) bit 6 of H";
          break;
        case 0xB5:  
          result = "RES 6,L: Clear (reset) bit 6 of L";
          break;
        case 0xB6:  
          result = "RES 6,(HL): Clear (reset) bit 6 of value pointed by HL";
          break;
        case 0xB7:  
          result = "RES 6,A: Clear (reset) bit 6 of A";
          break;
        case 0xB8:  
          result = "RES 7,B: Clear (reset) bit 7 of B";
          break;
        case 0xB9:  
          result = "RES 7,C: Clear (reset) bit 7 of C";
          break;
        case 0xBA:  
          result = "RES 7,D: Clear (reset) bit 7 of D";
          break;
        case 0xBB:  
          result = "RES 7,E: Clear (reset) bit 7 of E";
          break;
        case 0xBC:  
          result = "RES 7,H: Clear (reset) bit 7 of H";
          break;
        case 0xBD:  
          result = "RES 7,L: Clear (reset) bit 7 of L";
          break;
        case 0xBE:  
          result = "RES 7,(HL): Clear (reset) bit 7 of value pointed by HL";
          break;
        case 0xBF:  
          result = "RES 7,A: Clear (reset) bit 7 of A";
          break;
        case 0xC0:  
          result = "SET 0,B: Set bit 0 of B";
          break;
        case 0xC1:  
          result = "SET 0,C: Set bit 0 of C";
          break;
        case 0xC2:  
          result = "SET 0,D: Set bit 0 of D";
          break;
        case 0xC3:  
          result = "SET 0,E: Set bit 0 of E";
          break;
        case 0xC4:  
          result = "SET 0,H: Set bit 0 of H";
          break;
        case 0xC5:  
          result = "SET 0,L: Set bit 0 of L";
          break;
        case 0xC6:  
          result = "SET 0,(HL): Set bit 0 of value pointed by HL";
          break;
        case 0xC7:  
          result = "SET 0,A: Set bit 0 of A";
          break;
        case 0xC8:  
          result = "SET 1,B: Set bit 1 of B";
          break;
        case 0xC9:  
          result = "SET 1,C: Set bit 1 of C";
          break;
        case 0xCA:  
          result = "SET 1,D: Set bit 1 of D";
          break;
        case 0xCB:  
          result = "SET 1,E: Set bit 1 of E";
          break;
        case 0xCC:  
          result = "SET 1,H: Set bit 1 of H";
          break;
        case 0xCD:  
          result = "SET 1,L: Set bit 1 of L";
          break;
        case 0xCE:  
          result = "SET 1,(HL): Set bit 1 of value pointed by HL";
          break;
        case 0xCF:  
          result = "SET 1,A: Set bit 1 of A";
          break;
        case 0xD0:  
          result = "SET 2,B: Set bit 2 of B";
          break;
        case 0xD1:  
          result = "SET 2,C: Set bit 2 of C";
          break;
        case 0xD2:  
          result = "SET 2,D: Set bit 2 of D";
          break;
        case 0xD3:  
          result = "SET 2,E: Set bit 2 of E";
          break;
        case 0xD4:  
          result = "SET 2,H: Set bit 2 of H";
          break;
        case 0xD5:  
          result = "SET 2,L: Set bit 2 of L";
          break;
        case 0xD6:  
          result = "SET 2,(HL): Set bit 2 of value pointed by HL";
          break;
        case 0xD7:  
          result = "SET 2,A: Set bit 2 of A";
          break;
        case 0xD8:  
          result = "SET 3,B: Set bit 3 of B";
          break;
        case 0xD9:  
          result = "SET 3,C: Set bit 3 of C";
          break;
        case 0xDA:  
          result = "SET 3,D: Set bit 3 of D";
          break;
        case 0xDB:  
          result = "SET 3,E: Set bit 3 of E";
          break;
        case 0xDC:  
          result = "SET 3,H: Set bit 3 of H";
          break;
        case 0xDD:  
          result = "SET 3,L: Set bit 3 of L";
          break;
        case 0xDE:  
          result = "SET 3,(HL): Set bit 3 of value pointed by HL";
          break;
        case 0xDF:  
          result = "SET 3,A: Set bit 3 of A";
          break;
        case 0xE0:  
          result = "SET 4,B: Set bit 4 of B";
          break;
        case 0xE1:  
          result = "SET 4,C: Set bit 4 of C";
          break;
        case 0xE2:  
          result = "SET 4,D: Set bit 4 of D";
          break;
        case 0xE3:  
          result = "SET 4,E: Set bit 4 of E";
          break;
        case 0xE4:  
          result = "SET 4,H: Set bit 4 of H";
          break;
        case 0xE5:  
          result = "SET 4,L: Set bit 4 of L";
          break;
        case 0xE6:  
          result = "SET 4,(HL): Set bit 4 of value pointed by HL";
          break;
        case 0xE7:  
          result = "SET 4,A: Set bit 4 of A";
          break;
        case 0xE8:  
          result = "SET 5,B: Set bit 5 of B";
          break;
        case 0xE9:  
          result = "SET 5,C: Set bit 5 of C";
          break;
        case 0xEA:  
          result = "SET 5,D: Set bit 5 of D";
          break;
        case 0xEB:  
          result = "SET 5,E: Set bit 5 of E";
          break;
        case 0xEC:  
          result = "SET 5,H: Set bit 5 of H";
          break;
        case 0xED:  
          result = "SET 5,L: Set bit 5 of L";
          break;
        case 0xEE:  
          result = "SET 5,(HL): Set bit 5 of value pointed by HL";
          break;
        case 0xEF:  
          result = "SET 5,A: Set bit 5 of A";
          break;
        case 0xF0:  
          result = "SET 6,B: Set bit 6 of B";
          break;
        case 0xF1:  
          result = "SET 6,C: Set bit 6 of C";
          break;
        case 0xF2:  
          result = "SET 6,D: Set bit 6 of D";
          break;
        case 0xF3:  
          result = "SET 6,E: Set bit 6 of E";
          break;
        case 0xF4:  
          result = "SET 6,H: Set bit 6 of H";
          break;
        case 0xF5:  
          result = "SET 6,L: Set bit 6 of L";
          break;
        case 0xF6:  
          result = "SET 6,(HL): Set bit 6 of value pointed by HL";
          break;
        case 0xF7:  
          result = "SET 6,A: Set bit 6 of A";
          break;
        case 0xF8:  
          result = "SET 7,B: Set bit 7 of B";
          break;
        case 0xF9:  
          result = "SET 7,C: Set bit 7 of C";
          break;
        case 0xFA:  
          result = "SET 7,D: Set bit 7 of D";
          break;
        case 0xFB:  
          result = "SET 7,E: Set bit 7 of E";
          break;
        case 0xFC:  
          result = "SET 7,H: Set bit 7 of H";
          break;
        case 0xFD:  
          result = "SET 7,L: Set bit 7 of L";
          break;
        case 0xFE:  
          result = "SET 7,(HL): Set bit 7 of value pointed by HL";
          break;
        case 0xFF:  
          result = "SET 7,A: Set bit 7 of A";
          break;
        default:
          throw new InvalidProgramException("Invalid Opcode");
      }

      return result;
    }
  }
}
