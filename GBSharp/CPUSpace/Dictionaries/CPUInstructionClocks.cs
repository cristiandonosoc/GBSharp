using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUInstructionClocks
  {
    internal static Dictionary<byte, byte> Setup()
    {
      return new Dictionary<byte, byte>{
            {0x00, 4}, // NOP
            {0x01, 12}, // LD BC,nn
            {0x02, 8}, // LD (BC),A
            {0x03, 8}, // INC BC
            {0x04, 4}, // INC B
            {0x05, 4}, // DEC B
            {0x06, 8}, // LD B,n
            {0x07, 4}, // RLC A
            {0x08, 20}, // LD (nn),SP
            {0x09, 8}, // ADD HL,BC
            {0x0A, 8}, // LD A,(BC)
            {0x0B, 8}, // DEC BC
            {0x0C, 4}, // INC C
            {0x0D, 4}, // DEC C
            {0x0E, 8}, // LD C,n
            {0x0F, 4}, // RRC A
            {0x10, 4}, // STOP
            {0x11, 12}, // LD DE,nn
            {0x12, 8}, // LD (DE),A
            {0x13, 8}, // INC DE
            {0x14, 4}, // INC D
            {0x15, 4}, // DEC D
            {0x16, 8}, // LD D,n
            {0x17, 4}, // RL A
            {0x18, 12}, // JR n
            {0x19, 8}, // ADD HL,DE
            {0x1A, 8}, // LD A,(DE)
            {0x1B, 8}, // DEC DE
            {0x1C, 4}, // INC E
            {0x1D, 4}, // DEC E
            {0x1E, 8}, // LD E,n
            {0x1F, 4}, // RR A
            {0x20, 8}, // JR NZ,n
            {0x21, 12}, // LD HL,nn
            {0x22, 8}, // LDI (HL),A
            {0x23, 8}, // INC HL
            {0x24, 4}, // INC H
            {0x25, 4}, // DEC H
            {0x26, 8}, // LD H,n
            {0x27, 4}, // DAA
            {0x28, 8}, // JR Z,n
            {0x29, 8}, // ADD HL,HL
            {0x2A, 8}, // LDI A,(HL)
            {0x2B, 8}, // DEC HL
            {0x2C, 4}, // INC L
            {0x2D, 4}, // DEC L
            {0x2E, 8}, // LD L,n
            {0x2F, 4}, // CPL
            {0x30, 8}, // JR NC,n
            {0x31, 12}, // LD SP,nn
            {0x32, 8}, // LDD (HL),A
            {0x33, 8}, // INC SP
            {0x34, 12}, // INC (HL)
            {0x35, 12}, // DEC (HL)
            {0x36, 12}, // LD (HL),n
            {0x37, 4}, // SCF
            {0x38, 8}, // JR C,n
            {0x39, 8}, // ADD HL,SP
            {0x3A, 8}, // LDD A,(HL)
            {0x3B, 8}, // DEC SP
            {0x3C, 4}, // INC A
            {0x3D, 4}, // DEC A
            {0x3E, 8}, // LD A,n
            {0x3F, 4}, // CCF
            {0x40, 4}, // LD B,B
            {0x41, 4}, // LD B,C
            {0x42, 4}, // LD B,D
            {0x43, 4}, // LD B,E
            {0x44, 4}, // LD B,H
            {0x45, 4}, // LD B,L
            {0x46, 8}, // LD B,(HL)
            {0x47, 4}, // LD B,A
            {0x48, 4}, // LD C,B
            {0x49, 4}, // LD C,C
            {0x4A, 4}, // LD C,D
            {0x4B, 4}, // LD C,E
            {0x4C, 4}, // LD C,H
            {0x4D, 4}, // LD C,L
            {0x4E, 8}, // LD C,(HL)
            {0x4F, 4}, // LD C,A
            {0x50, 4}, // LD D,B
            {0x51, 4}, // LD D,C
            {0x52, 4}, // LD D,D
            {0x53, 4}, // LD D,E
            {0x54, 4}, // LD D,H
            {0x55, 4}, // LD D,L
            {0x56, 8}, // LD D,(HL)
            {0x57, 4}, // LD D,A
            {0x58, 4}, // LD E,B
            {0x59, 4}, // LD E,C
            {0x5A, 4}, // LD E,D
            {0x5B, 4}, // LD E,E
            {0x5C, 4}, // LD E,H
            {0x5D, 4}, // LD E,L
            {0x5E, 8}, // LD E,(HL)
            {0x5F, 4}, // LD E,A
            {0x60, 4}, // LD H,B
            {0x61, 4}, // LD H,C
            {0x62, 4}, // LD H,D
            {0x63, 4}, // LD H,E
            {0x64, 4}, // LD H,H
            {0x65, 4}, // LD H,L
            {0x66, 8}, // LD H,(HL)
            {0x67, 4}, // LD H,A
            {0x68, 4}, // LD L,B
            {0x69, 4}, // LD L,C
            {0x6A, 4}, // LD L,D
            {0x6B, 4}, // LD L,E
            {0x6C, 4}, // LD L,H
            {0x6D, 4}, // LD L,L
            {0x6E, 8}, // LD L,(HL)
            {0x6F, 4}, // LD L,A
            {0x70, 8}, // LD (HL),B
            {0x71, 8}, // LD (HL),C
            {0x72, 8}, // LD (HL),D
            {0x73, 8}, // LD (HL),E
            {0x74, 8}, // LD (HL),H
            {0x75, 8}, // LD (HL),L
            {0x76, 4}, // HALT
            {0x77, 8}, // LD (HL),A
            {0x78, 4}, // LD A,B
            {0x79, 4}, // LD A,C
            {0x7A, 4}, // LD A,D
            {0x7B, 4}, // LD A,E
            {0x7C, 4}, // LD A,H
            {0x7D, 4}, // LD A,L
            {0x7E, 8}, // LD A,(HL)
            {0x7F, 4}, // LD A,A
            {0x80, 4}, // ADD A,B
            {0x81, 4}, // ADD A,C
            {0x82, 4}, // ADD A,D
            {0x83, 4}, // ADD A,E
            {0x84, 4}, // ADD A,H
            {0x85, 4}, // ADD A,L
            {0x86, 8}, // ADD A,(HL)
            {0x87, 4}, // ADD A,A
            {0x88, 4}, // ADC A,B
            {0x89, 4}, // ADC A,C
            {0x8A, 4}, // ADC A,D
            {0x8B, 4}, // ADC A,E
            {0x8C, 4}, // ADC A,H
            {0x8D, 4}, // ADC A,L
            {0x8E, 8}, // ADC A,(HL)
            {0x8F, 4}, // ADC A,A
            {0x90, 4}, // SUB A,B
            {0x91, 4}, // SUB A,C
            {0x92, 4}, // SUB A,D
            {0x93, 4}, // SUB A,E
            {0x94, 4}, // SUB A,H
            {0x95, 4}, // SUB A,L
            {0x96, 8}, // SUB A,(HL)
            {0x97, 4}, // SUB A,A
            {0x98, 4}, // SBC A,B
            {0x99, 4}, // SBC A,C
            {0x9A, 4}, // SBC A,D
            {0x9B, 4}, // SBC A,E
            {0x9C, 4}, // SBC A,H
            {0x9D, 4}, // SBC A,L
            {0x9E, 8}, // SBC A,(HL)
            {0x9F, 4}, // SBC A,A
            {0xA0, 4}, // AND B
            {0xA1, 4}, // AND C
            {0xA2, 4}, // AND D
            {0xA3, 4}, // AND E
            {0xA4, 4}, // AND H
            {0xA5, 4}, // AND L
            {0xA6, 8}, // AND (HL)
            {0xA7, 4}, // AND A
            {0xA8, 4}, // XOR B
            {0xA9, 4}, // XOR C
            {0xAA, 4}, // XOR D
            {0xAB, 4}, // XOR E
            {0xAC, 4}, // XOR H
            {0xAD, 4}, // XOR L
            {0xAE, 8}, // XOR (HL)
            {0xAF, 4}, // XOR A
            {0xB0, 4}, // OR B
            {0xB1, 4}, // OR C
            {0xB2, 4}, // OR D
            {0xB3, 4}, // OR E
            {0xB4, 4}, // OR H
            {0xB5, 4}, // OR L
            {0xB6, 8}, // OR (HL)
            {0xB7, 4}, // OR A
            {0xB8, 4}, // CP B
            {0xB9, 4}, // CP C
            {0xBA, 4}, // CP D
            {0xBB, 4}, // CP E
            {0xBC, 4}, // CP H
            {0xBD, 4}, // CP L
            {0xBE, 8}, // CP (HL)
            {0xBF, 4}, // CP A
            {0xC0, 8}, // RET NZ
            {0xC1, 12}, // POP BC
            {0xC2, 12}, // JP NZ,nn
            {0xC3, 16}, // JP nn
            {0xC4, 12}, // CALL NZ,nn
            {0xC5, 16}, // PUSH BC
            {0xC6, 8}, // ADD A,n
            {0xC7, 16}, // RST 0
            {0xC8, 8}, // RET Z
            {0xC9, 16}, // RET
            {0xCA, 12}, // JP Z,nn
            {0xCB, 4}, // Ext ops
            {0xCC, 12}, // CALL Z,nn
            {0xCD, 24}, // CALL nn
            {0xCE, 8}, // ADC A,n
            {0xCF, 16}, // RST 8
            {0xD0, 8}, // RET NC
            {0xD1, 12}, // POP DE
            {0xD2, 12}, // JP NC,nn
            // {0xD3, }, // XX
            {0xD4, 12}, // CALL NC,nn
            {0xD5, 16}, // PUSH DE
            {0xD6, 8}, // SUB A,n
            {0xD7, 16}, // RST 10
            {0xD8, 8}, // RET C
            {0xD9, 16}, // RETI
            {0xDA, 12}, // JP C,nn
            // {0xDB, }, // XX
            {0xDC, 12}, // CALL C,nn
            // {0xDD, }, // XX
            {0xDE, 8}, // SBC A,n
            {0xDF, 16}, // RST 18
            {0xE0, 12}, // LDH (n),A
            {0xE1, 12}, // POP HL
            {0xE2, 8}, // LDH (C),A
            // {0xE3, }, // XX
            {0xE4, 4}, // XX
            {0xE5, 16}, // PUSH HL
            {0xE6, 8}, // AND n
            {0xE7, 16}, // RST 20
            {0xE8, 16}, // ADD SP,d
            {0xE9, 4}, // JP (HL)
            {0xEA, 16}, // LD (nn),A
            // {0xEB, }, // XX
            // {0xEC, }, // XX
            // {0xED, }, // XX
            {0xEE, 8}, // XOR n
            {0xEF, 16}, // RST 28
            {0xF0, 12}, // LDH A,(n)
            {0xF1, 12}, // POP AF
            {0xF2, 8}, // LDH A, (C)
            {0xF3, 4}, // DI
            // {0xF4, }, // XX
            {0xF5, 16}, // PUSH AF
            {0xF6, 8}, // OR n
            {0xF7, 16}, // RST 30
            {0xF8, 12}, // LDHL SP,d
            {0xF9, 8}, // LD SP,HL
            {0xFA, 16}, // LD A,(nn)
            {0xFB, 4}, // EI
            // {0xFC, }, // XX
            // {0xFD, }, // XX
            {0xFE, 8}, // CP n
            {0xFF, 16} // RST 38
        };
    }
  }
}
