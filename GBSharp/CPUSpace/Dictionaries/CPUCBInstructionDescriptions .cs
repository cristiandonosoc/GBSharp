using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUCBInstructionDescriptions
  {
    internal static Dictionary<byte, string> Setup()
    {
      return new Dictionary<byte, string>()
      {
        {0x00, "RLC B: Rotate B left with carry" },
        {0x01, "RLC C: Rotate C left with carry" },
        {0x02, "RLC D: Rotate D left with carry" },
        {0x03, "RLC E: Rotate E left with carry" },
        {0x04, "RLC H: Rotate H left with carry" },
        {0x05, "RLC L: Rotate L left with carry" },
        {0x06, "RLC (HL): Rotate value pointed by HL left with carry" },
        {0x07, "RLC A: Rotate A left with carry" },
        {0x08, "RRC B: Rotate B right with carry" },
        {0x09, "RRC C: Rotate C right with carry" },
        {0x0A, "RRC D: Rotate D right with carry" },
        {0x0B, "RRC E: Rotate E right with carry" },
        {0x0C, "RRC H: Rotate H right with carry" },
        {0x0D, "RRC L: Rotate L right with carry" },
        {0x0E, "RRC (HL): Rotate value pointed by HL right with carry" },
        {0x0F, "RRC A: Rotate A right with carry" },
        {0x10, "RL B: Rotate B left" },
        {0x11, "RL C: Rotate C left" },
        {0x12, "RL D: Rotate D left" },
        {0x13, "RL E: Rotate E left" },
        {0x14, "RL H: Rotate H left" },
        {0x15, "RL L: Rotate L left" },
        {0x16, "RL (HL): Rotate value pointed by HL left" },
        {0x17, "RL A: Rotate A left" },
        {0x18, "RR B: Rotate B right" },
        {0x19, "RR C: Rotate C right" },
        {0x1A, "RR D: Rotate D right" },
        {0x1B, "RR E: Rotate E right" },
        {0x1C, "RR H: Rotate H right" },
        {0x1D, "RR L: Rotate L right" },
        {0x1E, "RR (HL): Rotate value pointed by HL right" },
        {0x1F, "RR A: Rotate A right" },
        {0x20, "SLA B: Shift B left preserving sign" },
        {0x21, "SLA C: Shift C left preserving sign" },
        {0x22, "SLA D: Shift D left preserving sign" },
        {0x23, "SLA E: Shift E left preserving sign" },
        {0x24, "SLA H: Shift H left preserving sign" },
        {0x25, "SLA L: Shift L left preserving sign" },
        {0x26, "SLA (HL): Shift value pointed by HL left preserving sign" },
        {0x28, "SRA B: Shift B right preserving sign" },
        {0x29, "SRA C: Shift C right preserving sign" },
        {0x2A, "SRA D: Shift D right preserving sign" },
        {0x2B, "SRA E: Shift E right preserving sign" },
        {0x2C, "SRA H: Shift H right preserving sign" },
        {0x2D, "SRA L: Shift L right preserving sign" },
        {0x2E, "SRA (HL): Shift value pointed by HL right preserving sign" },
        {0x2F, "SRA A: Shift A right preserving sign" },
        {0x30, "SWAP B: Swap nybbles in B" },
        {0x31, "SWAP C: Swap nybbles in C" },
        {0x32, "SWAP D: Swap nybbles in D" },
        {0x33, "SWAP E: Swap nybbles in E" },
        {0x34, "SWAP H: Swap nybbles in H" },
        {0x35, "SWAP L: Swap nybbles in L" },
        {0x36, "SWAP (HL): Swap nybbles in value pointed by HL" },
        {0x37, "SWAP A: Swap nybbles in A" },
        {0x38, "SRL B: Shift B right" },
        {0x39, "SRL C: Shift C right" },
        {0x3A, "SRL D: Shift D right" },
        {0x3B, "SRL E: Shift E right" },
        {0x3C, "SRL H: Shift H right" },
        {0x3D, "SRL L: Shift L right" },
        {0x3E, "SRL (HL): Shift value pointed by HL right" },
        {0x3F, "SRL A: Shift A right" },
        {0x40, "BIT 0,B: Test bit 0 of B" },
        {0x41, "BIT 0,C: Test bit 0 of C" },
        {0x42, "BIT 0,D: Test bit 0 of D" },
        {0x43, "BIT 0,E: Test bit 0 of E" },
        {0x44, "BIT 0,H: Test bit 0 of H" },
        {0x45, "BIT 0,L: Test bit 0 of L" },
        {0x46, "BIT 0,(HL): Test bit 0 of value pointed by HL" },
        {0x47, "BIT 0,A: Test bit 0 of A" },
        {0x48, "BIT 1,B: Test bit 1 of B" },
        {0x49, "BIT 1,C: Test bit 1 of C" },
        {0x4A, "BIT 1,D: Test bit 1 of D" },
        {0x4B, "BIT 1,E: Test bit 1 of E" },
        {0x4C, "BIT 1,H: Test bit 1 of H" },
        {0x4D, "BIT 1,L: Test bit 1 of L" },
        {0x4E, "BIT 1,(HL): Test bit 1 of value pointed by HL" },
        {0x4F, "BIT 1,A: Test bit 1 of A" },
        {0x50, "BIT 2,B: Test bit 2 of B" },
        {0x51, "BIT 2,C: Test bit 2 of C" },
        {0x52, "BIT 2,D: Test bit 2 of D" },
        {0x53, "BIT 2,E: Test bit 2 of E" },
        {0x54, "BIT 2,H: Test bit 2 of H" },
        {0x55, "BIT 2,L: Test bit 2 of L" },
        {0x56, "BIT 2,(HL): Test bit 2 of value pointed by HL" },
        {0x57, "BIT 2,A: Test bit 2 of A" },
        {0x58, "BIT 3,B: Test bit 3 of B" },
        {0x59, "BIT 3,C: Test bit 3 of C" },
        {0x5A, "BIT 3,D: Test bit 3 of D" },
        {0x5B, "BIT 3,E: Test bit 3 of E" },
        {0x5C, "BIT 3,H: Test bit 3 of H" },
        {0x5D, "BIT 3,L: Test bit 3 of L" },
        {0x5E, "BIT 3,(HL): Test bit 3 of value pointed by HL" },
        {0x5F, "BIT 3,A: Test bit 3 of A" },
        {0x60, "BIT 4,B: Test bit 4 of B" },
        {0x61, "BIT 4,C: Test bit 4 of C" },
        {0x62, "BIT 4,D: Test bit 4 of D" },
        {0x63, "BIT 4,E: Test bit 4 of E" },
        {0x64, "BIT 4,H: Test bit 4 of H" },
        {0x65, "BIT 4,L: Test bit 4 of L" },
        {0x66, "BIT 4,(HL): Test bit 4 of value pointed by HL" },
        {0x67, "BIT 4,A: Test bit 4 of A" },
        {0x68, "BIT 5,B: Test bit 5 of B" },
        {0x69, "BIT 5,C: Test bit 5 of C" },
        {0x6A, "BIT 5,D: Test bit 5 of D" },
        {0x6B, "BIT 5,E: Test bit 5 of E" },
        {0x6C, "BIT 5,H: Test bit 5 of H" },
        {0x6D, "BIT 5,L: Test bit 5 of L" },
        {0x6E, "BIT 5,(HL): Test bit 5 of value pointed by HL" },
        {0x6F, "BIT 5,A: Test bit 5 of A" },
        {0x70, "BIT 6,B: Test bit 6 of B" },
        {0x71, "BIT 6,C: Test bit 6 of C" },
        {0x72, "BIT 6,D: Test bit 6 of D" },
        {0x73, "BIT 6,E: Test bit 6 of E" },
        {0x74, "BIT 6,H: Test bit 6 of H" },
        {0x75, "BIT 6,L: Test bit 6 of L" },
        {0x76, "BIT 6,(HL): Test bit 6 of value pointed by HL" },
        {0x77, "BIT 6,A: Test bit 6 of A" },
        {0x78, "BIT 7,B: Test bit 7 of B" },
        {0x79, "BIT 7,C: Test bit 7 of C" },
        {0x7A, "BIT 7,D: Test bit 7 of D" },
        {0x7B, "BIT 7,E: Test bit 7 of E" },
        {0x7C, "BIT 7,H: Test bit 7 of H" },
        {0x7D, "BIT 7,L: Test bit 7 of L" },
        {0x7E, "BIT 7,(HL): Test bit 7 of value pointed by HL" },
        {0x7F, "BIT 7,A: Test bit 7 of A" },
        {0x80, "RES 0,B: Clear (reset) bit 0 of B" },
        {0x81, "RES 0,C: Clear (reset) bit 0 of C" },
        {0x82, "RES 0,D: Clear (reset) bit 0 of D" },
        {0x83, "RES 0,E: Clear (reset) bit 0 of E" },
        {0x84, "RES 0,H: Clear (reset) bit 0 of H" },
        {0x85, "RES 0,L: Clear (reset) bit 0 of L" },
        {0x86, "RES 0,(HL): Clear (reset) bit 0 of value pointed by HL" },
        {0x87, "RES 0,A: Clear (reset) bit 0 of A" },
        {0x88, "RES 1,B: Clear (reset) bit 1 of B" },
        {0x89, "RES 1,C: Clear (reset) bit 1 of C" },
        {0x8A, "RES 1,D: Clear (reset) bit 1 of D" },
        {0x8B, "RES 1,E: Clear (reset) bit 1 of E" },
        {0x8C, "RES 1,H: Clear (reset) bit 1 of H" },
        {0x8D, "RES 1,L: Clear (reset) bit 1 of L" },
        {0x8E, "RES 1,(HL): Clear (reset) bit 1 of value pointed by HL" },
        {0x8F, "RES 1,A: Clear (reset) bit 1 of A" },
        {0x90, "RES 2,B: Clear (reset) bit 2 of B" },
        {0x91, "RES 2,C: Clear (reset) bit 2 of C" },
        {0x92, "RES 2,D: Clear (reset) bit 2 of D" },
        {0x93, "RES 2,E: Clear (reset) bit 2 of E" },
        {0x94, "RES 2,H: Clear (reset) bit 2 of H" },
        {0x95, "RES 2,L: Clear (reset) bit 2 of L" },
        {0x96, "RES 2,(HL): Clear (reset) bit 2 of value pointed by HL" },
        {0x97, "RES 2,A: Clear (reset) bit 2 of A" },
        {0x98, "RES 3,B: Clear (reset) bit 3 of B" },
        {0x99, "RES 3,C: Clear (reset) bit 3 of C" },
        {0x9A, "RES 3,D: Clear (reset) bit 3 of D" },
        {0x9B, "RES 3,E: Clear (reset) bit 3 of E" },
        {0x9C, "RES 3,H: Clear (reset) bit 3 of H" },
        {0x9D, "RES 3,L: Clear (reset) bit 3 of L" },
        {0x9E, "RES 3,(HL): Clear (reset) bit 3 of value pointed by HL" },
        {0x9F, "RES 3,A: Clear (reset) bit 3 of A" },
        {0xA0, "RES 4,B: Clear (reset) bit 4 of B" },
        {0xA1, "RES 4,C: Clear (reset) bit 4 of C" },
        {0xA2, "RES 4,D: Clear (reset) bit 4 of D" },
        {0xA3, "RES 4,E: Clear (reset) bit 4 of E" },
        {0xA4, "RES 4,H: Clear (reset) bit 4 of H" },
        {0xA5, "RES 4,L: Clear (reset) bit 4 of L" },
        {0xA6, "RES 4,(HL): Clear (reset) bit 4 of value pointed by HL" },
        {0xA7, "RES 4,A: Clear (reset) bit 4 of A" },
        {0xA8, "RES 5,B: Clear (reset) bit 5 of B" },
        {0xA9, "RES 5,C: Clear (reset) bit 5 of C" },
        {0xAA, "RES 5,D: Clear (reset) bit 5 of D" },
        {0xAB, "RES 5,E: Clear (reset) bit 5 of E" },
        {0xAC, "RES 5,H: Clear (reset) bit 5 of H" },
        {0xAD, "RES 5,L: Clear (reset) bit 5 of L" },
        {0xAE, "RES 5,(HL): Clear (reset) bit 5 of value pointed by HL" },
        {0xAF, "RES 5,A: Clear (reset) bit 5 of A" },
        {0xB0, "RES 6,B: Clear (reset) bit 6 of B" },
        {0xB1, "RES 6,C: Clear (reset) bit 6 of C" },
        {0xB2, "RES 6,D: Clear (reset) bit 6 of D" },
        {0xB3, "RES 6,E: Clear (reset) bit 6 of E" },
        {0xB4, "RES 6,H: Clear (reset) bit 6 of H" },
        {0xB5, "RES 6,L: Clear (reset) bit 6 of L" },
        {0xB6, "RES 6,(HL): Clear (reset) bit 6 of value pointed by HL" },
        {0xB7, "RES 6,A: Clear (reset) bit 6 of A" },
        {0xB8, "RES 7,B: Clear (reset) bit 7 of B" },
        {0xB9, "RES 7,C: Clear (reset) bit 7 of C" },
        {0xBA, "RES 7,D: Clear (reset) bit 7 of D" },
        {0xBB, "RES 7,E: Clear (reset) bit 7 of E" },
        {0xBC, "RES 7,H: Clear (reset) bit 7 of H" },
        {0xBD, "RES 7,L: Clear (reset) bit 7 of L" },
        {0xBE, "RES 7,(HL): Clear (reset) bit 7 of value pointed by HL" },
        {0xBF, "RES 7,A: Clear (reset) bit 7 of A" },
        {0xC0, "SET 0,B: Set bit 0 of B" },
        {0xC1, "SET 0,C: Set bit 0 of C" },
        {0xC2, "SET 0,D: Set bit 0 of D" },
        {0xC3, "SET 0,E: Set bit 0 of E" },
        {0xC4, "SET 0,H: Set bit 0 of H" },
        {0xC5, "SET 0,L: Set bit 0 of L" },
        {0xC6, "SET 0,(HL): Set bit 0 of value pointed by HL" },
        {0xC7, "SET 0,A: Set bit 0 of A" },
        {0xC8, "SET 1,B: Set bit 1 of B" },
        {0xC9, "SET 1,C: Set bit 1 of C" },
        {0xCA, "SET 1,D: Set bit 1 of D" },
        {0xCB, "SET 1,E: Set bit 1 of E" },
        {0xCC, "SET 1,H: Set bit 1 of H" },
        {0xCD, "SET 1,L: Set bit 1 of L" },
        {0xCE, "SET 1,(HL): Set bit 1 of value pointed by HL" },
        {0xCF, "SET 1,A: Set bit 1 of A" },
        {0xD0, "SET 2,B: Set bit 2 of B" },
        {0xD1, "SET 2,C: Set bit 2 of C" },
        {0xD2, "SET 2,D: Set bit 2 of D" },
        {0xD3, "SET 2,E: Set bit 2 of E" },
        {0xD4, "SET 2,H: Set bit 2 of H" },
        {0xD5, "SET 2,L: Set bit 2 of L" },
        {0xD6, "SET 2,(HL): Set bit 2 of value pointed by HL" },
        {0xD7, "SET 2,A: Set bit 2 of A" },
        {0xD8, "SET 3,B: Set bit 3 of B" },
        {0xD9, "SET 3,C: Set bit 3 of C" },
        {0xDA, "SET 3,D: Set bit 3 of D" },
        {0xDB, "SET 3,E: Set bit 3 of E" },
        {0xDC, "SET 3,H: Set bit 3 of H" },
        {0xDD, "SET 3,L: Set bit 3 of L" },
        {0xDE, "SET 3,(HL): Set bit 3 of value pointed by HL" },
        {0xDF, "SET 3,A: Set bit 3 of A" },
        {0xE0, "SET 4,B: Set bit 4 of B" },
        {0xE1, "SET 4,C: Set bit 4 of C" },
        {0xE2, "SET 4,D: Set bit 4 of D" },
        {0xE3, "SET 4,E: Set bit 4 of E" },
        {0xE4, "SET 4,H: Set bit 4 of H" },
        {0xE5, "SET 4,L: Set bit 4 of L" },
        {0xE6, "SET 4,(HL): Set bit 4 of value pointed by HL" },
        {0xE7, "SET 4,A: Set bit 4 of A" },
        {0xE8, "SET 5,B: Set bit 5 of B" },
        {0xE9, "SET 5,C: Set bit 5 of C" },
        {0xEA, "SET 5,D: Set bit 5 of D" },
        {0xEB, "SET 5,E: Set bit 5 of E" },
        {0xEC, "SET 5,H: Set bit 5 of H" },
        {0xED, "SET 5,L: Set bit 5 of L" },
        {0xEE, "SET 5,(HL): Set bit 5 of value pointed by HL" },
        {0xEF, "SET 5,A: Set bit 5 of A" },
        {0xF0, "SET 6,B: Set bit 6 of B" },
        {0xF1, "SET 6,C: Set bit 6 of C" },
        {0xF2, "SET 6,D: Set bit 6 of D" },
        {0xF3, "SET 6,E: Set bit 6 of E" },
        {0xF4, "SET 6,H: Set bit 6 of H" },
        {0xF5, "SET 6,L: Set bit 6 of L" },
        {0xF6, "SET 6,(HL): Set bit 6 of value pointed by HL" },
        {0xF7, "SET 6,A: Set bit 6 of A" },
        {0xF8, "SET 7,B: Set bit 7 of B" },
        {0xF9, "SET 7,C: Set bit 7 of C" },
        {0xFA, "SET 7,D: Set bit 7 of D" },
        {0xFB, "SET 7,E: Set bit 7 of E" },
        {0xFC, "SET 7,H: Set bit 7 of H" },
        {0xFD, "SET 7,L: Set bit 7 of L" },
        {0xFE, "SET 7,(HL): Set bit 7 of value pointed by HL" },
        {0xFF, "SET 7,A: Set bit 7 of A" },
      };
    }
  }
}
