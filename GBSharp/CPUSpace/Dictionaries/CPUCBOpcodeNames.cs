using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUCBOpcodeNames
  {
    internal static Dictionary<byte, string> Setup()
    {
      return new Dictionary<byte, string>(){
            {0x00, "RLC B"}, // RLC B
            {0x01, "RLC C"}, // RLC C
            {0x02, "RLC D"}, // RLC D
            {0x03, "RLC E"}, // RLC E
            {0x04, "RLC H"}, // RLC H
            {0x05, "RLC L"}, // RLC L
            {0x06, "RLC (HL)"}, // RLC (HL)
            {0x07, "RLC A"}, // RLC A
            {0x08, "RRC B"}, // RRC B
            {0x09, "RRC C"}, // RRC C
            {0x0A, "RRC D"}, // RRC D
            {0x0B, "RRC E"}, // RRC E
            {0x0C, "RRC H"}, // RRC H
            {0x0D, "RRC L"}, // RRC L
            {0x0E, "RRC (HL)"}, // RRC (HL)
            {0x0F, "RRC A"}, // RRC A
            {0x10, "RL B"}, // RL B
            {0x11, "RL C"}, // RL C
            {0x12, "RL D"}, // RL D
            {0x13, "RL E"}, // RL E
            {0x14, "RL H"}, // RL H
            {0x15, "RL L"}, // RL L
            {0x16, "RL (HL)"}, // RL (HL)
            {0x17, "RL A"}, // RL A
            {0x18, "RR B"}, // RR B
            {0x19, "RR C"}, // RR C
            {0x1A, "RR D"}, // RR D
            {0x1B, "RR E"}, // RR E
            {0x1C, "RR H"}, // RR H
            {0x1D, "RR L"}, // RR L
            {0x1E, "RR (HL)"}, // RR (HL)
            {0x1F, "RR A"}, // RR A
            {0x20, "SLA B"}, // SLA B
            {0x21, "SLA C"}, // SLA C
            {0x22, "SLA D"}, // SLA D
            {0x23, "SLA E"}, // SLA E
            {0x24, "SLA H"}, // SLA H
            {0x25, "SLA L"}, // SLA L
            {0x26, "SLA (HL)"}, // SLA (HL)
            {0x27, "SLA A"}, // SLA A
            {0x28, "SRA B"}, // SRA B
            {0x29, "SRA C"}, // SRA C
            {0x2A, "SRA D"}, // SRA D
            {0x2B, "SRA E"}, // SRA E
            {0x2C, "SRA H"}, // SRA H
            {0x2D, "SRA L"}, // SRA L
            {0x2E, "SRA (HL)"}, // SRA (HL)
            {0x2F, "SRA A"}, // SRA A
            {0x30, "SWAP B"}, // SWAP B
            {0x31, "SWAP C"}, // SWAP C
            {0x32, "SWAP D"}, // SWAP D
            {0x33, "SWAP E"}, // SWAP E
            {0x34, "SWAP H"}, // SWAP H
            {0x35, "SWAP L"}, // SWAP L
            {0x36, "SWAP (HL)"}, // SWAP (HL)
            {0x37, "SWAP A"}, // SWAP A
            {0x38, "SRL B"}, // SRL B
            {0x39, "SRL C"}, // SRL C
            {0x3A, "SRL D"}, // SRL D
            {0x3B, "SRL E"}, // SRL E
            {0x3C, "SRL H"}, // SRL H
            {0x3D, "SRL L"}, // SRL L
            {0x3E, "SRL (HL)"}, // SRL (HL)
            {0x3F, "SRL A"}, // SRL A
            {0x40, "BIT 0,B"}, // BIT 0,B
            {0x41, "BIT 0,C"}, // BIT 0,C
            {0x42, "BIT 0,D"}, // BIT 0,D
            {0x43, "BIT 0,E"}, // BIT 0,E
            {0x44, "BIT 0,H"}, // BIT 0,H
            {0x45, "BIT 0,L"}, // BIT 0,L
            {0x46, "BIT 0,(HL)"}, // BIT 0,(HL)
            {0x47, "BIT 0,A"}, // BIT 0,A
            {0x48, "BIT 1,B"}, // BIT 1,B
            {0x49, "BIT 1,C"}, // BIT 1,C
            {0x4A, "BIT 1,D"}, // BIT 1,D
            {0x4B, "BIT 1,E"}, // BIT 1,E
            {0x4C, "BIT 1,H"}, // BIT 1,H
            {0x4D, "BIT 1,L"}, // BIT 1,L
            {0x4E, "BIT 1,(HL)"}, // BIT 1,(HL)
            {0x4F, "BIT 1,A"}, // BIT 1,A
            {0x50, "BIT 2,B"}, // BIT 2,B
            {0x51, "BIT 2,C"}, // BIT 2,C
            {0x52, "BIT 2,D"}, // BIT 2,D
            {0x53, "BIT 2,E"}, // BIT 2,E
            {0x54, "BIT 2,H"}, // BIT 2,H
            {0x55, "BIT 2,L"}, // BIT 2,L
            {0x56, "BIT 2,(HL)"}, // BIT 2,(HL)
            {0x57, "BIT 2,A"}, // BIT 2,A
            {0x58, "BIT 3,B"}, // BIT 3,B
            {0x59, "BIT 3,C"}, // BIT 3,C
            {0x5A, "BIT 3,D"}, // BIT 3,D
            {0x5B, "BIT 3,E"}, // BIT 3,E
            {0x5C, "BIT 3,H"}, // BIT 3,H
            {0x5D, "BIT 3,L"}, // BIT 3,L
            {0x5E, "BIT 3,(HL)"}, // BIT 3,(HL)
            {0x5F, "BIT 3,A"}, // BIT 3,A
            {0x60, "BIT 4,B"}, // BIT 4,B
            {0x61, "BIT 4,C"}, // BIT 4,C
            {0x62, "BIT 4,D"}, // BIT 4,D
            {0x63, "BIT 4,E"}, // BIT 4,E
            {0x64, "BIT 4,H"}, // BIT 4,H
            {0x65, "BIT 4,L"}, // BIT 4,L
            {0x66, "BIT 4,(HL)"}, // BIT 4,(HL)
            {0x67, "BIT 4,A"}, // BIT 4,A
            {0x68, "BIT 5,B"}, // BIT 5,B
            {0x69, "BIT 5,C"}, // BIT 5,C
            {0x6A, "BIT 5,D"}, // BIT 5,D
            {0x6B, "BIT 5,E"}, // BIT 5,E
            {0x6C, "BIT 5,H"}, // BIT 5,H
            {0x6D, "BIT 5,L"}, // BIT 5,L
            {0x6E, "BIT 5,(HL)"}, // BIT 5,(HL)
            {0x6F, "BIT 5,A"}, // BIT 5,A
            {0x70, "BIT 6,B"}, // BIT 6,B
            {0x71, "BIT 6,C"}, // BIT 6,C
            {0x72, "BIT 6,D"}, // BIT 6,D
            {0x73, "BIT 6,E"}, // BIT 6,E
            {0x74, "BIT 6,H"}, // BIT 6,H
            {0x75, "BIT 6,L"}, // BIT 6,L
            {0x76, "BIT 6,(HL)"}, // BIT 6,(HL)
            {0x77, "BIT 6,A"}, // BIT 6,A
            {0x78, "BIT 7,B"}, // BIT 7,B
            {0x79, "BIT 7,C"}, // BIT 7,C
            {0x7A, "BIT 7,D"}, // BIT 7,D
            {0x7B, "BIT 7,E"}, // BIT 7,E
            {0x7C, "BIT 7,H"}, // BIT 7,H
            {0x7D, "BIT 7,L"}, // BIT 7,L
            {0x7E, "BIT 7,(HL)"}, // BIT 7,(HL)
            {0x7F, "BIT 7,A"}, // BIT 7,A
            {0x80, "RES 0,B"}, // RES 0,B
            {0x81, "RES 0,C"}, // RES 0,C
            {0x82, "RES 0,D"}, // RES 0,D
            {0x83, "RES 0,E"}, // RES 0,E
            {0x84, "RES 0,H"}, // RES 0,H
            {0x85, "RES 0,L"}, // RES 0,L
            {0x86, "RES 0,(HL)"}, // RES 0,(HL)
            {0x87, "RES 0,A"}, // RES 0,A
            {0x88, "RES 1,B"}, // RES 1,B
            {0x89, "RES 1,C"}, // RES 1,C
            {0x8A, "RES 1,D"}, // RES 1,D
            {0x8B, "RES 1,E"}, // RES 1,E
            {0x8C, "RES 1,H"}, // RES 1,H
            {0x8D, "RES 1,L"}, // RES 1,L
            {0x8E, "RES 1,(HL)"}, // RES 1,(HL)
            {0x8F, "RES 1,A"}, // RES 1,A
            {0x90, "RES 2,B"}, // RES 2,B
            {0x91, "RES 2,C"}, // RES 2,C
            {0x92, "RES 2,D"}, // RES 2,D
            {0x93, "RES 2,E"}, // RES 2,E
            {0x94, "RES 2,H"}, // RES 2,H
            {0x95, "RES 2,L"}, // RES 2,L
            {0x96, "RES 2,(HL)"}, // RES 2,(HL)
            {0x97, "RES 2,A"}, // RES 2,A
            {0x98, "RES 3,B"}, // RES 3,B
            {0x99, "RES 3,C"}, // RES 3,C
            {0x9A, "RES 3,D"}, // RES 3,D
            {0x9B, "RES 3,E"}, // RES 3,E
            {0x9C, "RES 3,H"}, // RES 3,H
            {0x9D, "RES 3,L"}, // RES 3,L
            {0x9E, "RES 3,(HL)"}, // RES 3,(HL)
            {0x9F, "RES 3,A"}, // RES 3,A
            {0xA0, "RES 4,B"}, // RES 4,B
            {0xA1, "RES 4,C"}, // RES 4,C
            {0xA2, "RES 4,D"}, // RES 4,D
            {0xA3, "RES 4,E"}, // RES 4,E
            {0xA4, "RES 4,H"}, // RES 4,H
            {0xA5, "RES 4,L"}, // RES 4,L
            {0xA6, "RES 4,(HL)"}, // RES 4,(HL)
            {0xA7, "RES 4,A"}, // RES 4,A
            {0xA8, "RES 5,B"}, // RES 5,B
            {0xA9, "RES 5,C"}, // RES 5,C
            {0xAA, "RES 5,D"}, // RES 5,D
            {0xAB, "RES 5,E"}, // RES 5,E
            {0xAC, "RES 5,H"}, // RES 5,H
            {0xAD, "RES 5,L"}, // RES 5,L
            {0xAE, "RES 5,(HL)"}, // RES 5,(HL)
            {0xAF, "RES 5,A"}, // RES 5,A
            {0xB0, "RES 6,B"}, // RES 6,B
            {0xB1, "RES 6,C"}, // RES 6,C
            {0xB2, "RES 6,D"}, // RES 6,D
            {0xB3, "RES 6,E"}, // RES 6,E
            {0xB4, "RES 6,H"}, // RES 6,H
            {0xB5, "RES 6,L"}, // RES 6,L
            {0xB6, "RES 6,(HL)"}, // RES 6,(HL)
            {0xB7, "RES 6,A"}, // RES 6,A
            {0xB8, "RES 7,B"}, // RES 7,B
            {0xB9, "RES 7,C"}, // RES 7,C
            {0xBA, "RES 7,D"}, // RES 7,D
            {0xBB, "RES 7,E"}, // RES 7,E
            {0xBC, "RES 7,H"}, // RES 7,H
            {0xBD, "RES 7,L"}, // RES 7,L
            {0xBE, "RES 7,(HL)"}, // RES 7,(HL)
            {0xBF, "RES 7,A"}, // RES 7,A
            {0xC0, "SET 0,B"}, // SET 0,B
            {0xC1, "SET 0,C"}, // SET 0,C
            {0xC2, "SET 0,D"}, // SET 0,D
            {0xC3, "SET 0,E"}, // SET 0,E
            {0xC4, "SET 0,H"}, // SET 0,H
            {0xC5, "SET 0,L"}, // SET 0,L
            {0xC6, "SET 0,(HL)"}, // SET 0,(HL)
            {0xC7, "SET 0,A"}, // SET 0,A
            {0xC8, "SET 1,B"}, // SET 1,B
            {0xC9, "SET 1,C"}, // SET 1,C
            {0xCA, "SET 1,D"}, // SET 1,D
            {0xCB, "SET 1,E"}, // SET 1,E
            {0xCC, "SET 1,H"}, // SET 1,H
            {0xCD, "SET 1,L"}, // SET 1,L
            {0xCE, "SET 1,(HL)"}, // SET 1,(HL)
            {0xCF, "SET 1,A"}, // SET 1,A
            {0xD0, "SET 2,B"}, // SET 2,B
            {0xD1, "SET 2,C"}, // SET 2,C
            {0xD2, "SET 2,D"}, // SET 2,D
            {0xD3, "SET 2,E"}, // SET 2,E
            {0xD4, "SET 2,H"}, // SET 2,H
            {0xD5, "SET 2,L"}, // SET 2,L
            {0xD6, "SET 2,(HL)"}, // SET 2,(HL)
            {0xD7, "SET 2,A"}, // SET 2,A
            {0xD8, "SET 3,B"}, // SET 3,B
            {0xD9, "SET 3,C"}, // SET 3,C
            {0xDA, "SET 3,D"}, // SET 3,D
            {0xDB, "SET 3,E"}, // SET 3,E
            {0xDC, "SET 3,H"}, // SET 3,H
            {0xDD, "SET 3,L"}, // SET 3,L
            {0xDE, "SET 3,(HL)"}, // SET 3,(HL)
            {0xDF, "SET 3,A"}, // SET 3,A
            {0xE0, "SET 4,B"}, // SET 4,B
            {0xE1, "SET 4,C"}, // SET 4,C
            {0xE2, "SET 4,D"}, // SET 4,D
            {0xE3, "SET 4,E"}, // SET 4,E
            {0xE4, "SET 4,H"}, // SET 4,H
            {0xE5, "SET 4,L"}, // SET 4,L
            {0xE6, "SET 4,(HL)"}, // SET 4,(HL)
            {0xE7, "SET 4,A"}, // SET 4,A
            {0xE8, "SET 5,B"}, // SET 5,B
            {0xE9, "SET 5,C"}, // SET 5,C
            {0xEA, "SET 5,D"}, // SET 5,D
            {0xEB, "SET 5,E"}, // SET 5,E
            {0xEC, "SET 5,H"}, // SET 5,H
            {0xED, "SET 5,L"}, // SET 5,L
            {0xEE, "SET 5,(HL)"}, // SET 5,(HL)
            {0xEF, "SET 5,A"}, // SET 5,A
            {0xF0, "SET 6,B"}, // SET 6,B
            {0xF1, "SET 6,C"}, // SET 6,C
            {0xF2, "SET 6,D"}, // SET 6,D
            {0xF3, "SET 6,E"}, // SET 6,E
            {0xF4, "SET 6,H"}, // SET 6,H
            {0xF5, "SET 6,L"}, // SET 6,L
            {0xF6, "SET 6,(HL)"}, // SET 6,(HL)
            {0xF7, "SET 6,A"}, // SET 6,A
            {0xF8, "SET 7,B"}, // SET 7,B
            {0xF9, "SET 7,C"}, // SET 7,C
            {0xFA, "SET 7,D"}, // SET 7,D
            {0xFB, "SET 7,E"}, // SET 7,E
            {0xFC, "SET 7,H"}, // SET 7,H
            {0xFD, "SET 7,L"}, // SET 7,L
            {0xFE, "SET 7,(HL)"}, // SET 7,(HL)
            {0xFF, "SET 7,A"}, // SET 7,A
        };
    }
  }
}