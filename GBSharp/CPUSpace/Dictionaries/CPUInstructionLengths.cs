using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUInstructionLengths
  {
    internal static Dictionary<byte, byte> Setup() {
      return new Dictionary<byte, byte>() {
            {0x00, 1}, // NOP
            {0x01, 3}, // LD BC,nn
            {0x02, 1}, // LD (BC),A
            {0x03, 1}, // INC BC
            {0x04, 1}, // INC B
            {0x05, 1}, // DEC B
            {0x06, 2}, // LD B,n
            {0x07, 1}, // RLC A
            {0x08, 3}, // LD (nn),SP
            {0x09, 1}, // ADD HL,BC
            {0x0A, 1}, // LD A,(BC)
            {0x0B, 1}, // DEC BC
            {0x0C, 1}, // INC C
            {0x0D, 1}, // DEC C
            {0x0E, 2}, // LD C,n
            {0x0F, 1}, // RRC A
            {0x10, 2}, // STOP
            {0x11, 3}, // LD DE,nn
            {0x12, 1}, // LD (DE),A
            {0x13, 1}, // INC DE
            {0x14, 1}, // INC D
            {0x15, 1}, // DEC D
            {0x16, 2}, // LD D,n
            {0x17, 1}, // RL A
            {0x18, 2}, // JR n
            {0x19, 1}, // ADD HL,DE
            {0x1A, 1}, // LD A,(DE)
            {0x1B, 1}, // DEC DE
            {0x1C, 1}, // INC E
            {0x1D, 1}, // DEC E
            {0x1E, 2}, // LD E,n
            {0x1F, 1}, // RR A
            {0x20, 2}, // JR NZ,n
            {0x21, 3}, // LD HL,nn
            {0x22, 1}, // LDI (HL),A
            {0x23, 1}, // INC HL
            {0x24, 1}, // INC H
            {0x25, 1}, // DEC H
            {0x26, 2}, // LD H,n
            {0x27, 1}, // DAA
            {0x28, 2}, // JR Z,n
            {0x29, 1}, // ADD HL,HL
            {0x2A, 1}, // LDI A,(HL)
            {0x2B, 1}, // DEC HL
            {0x2C, 1}, // INC L
            {0x2D, 1}, // DEC L
            {0x2E, 2}, // LD L,n
            {0x2F, 1}, // CPL
            {0x30, 2}, // JR NC,n
            {0x31, 3}, // LD SP,nn
            {0x32, 1}, // LDD (HL),A
            {0x33, 1}, // INC SP
            {0x34, 1}, // INC (HL)
            {0x35, 1}, // DEC (HL)
            {0x36, 2}, // LD (HL),n
            {0x37, 1}, // SCF
            {0x38, 2}, // JR C,n
            {0x39, 1}, // ADD HL,SP
            {0x3A, 1}, // LDD A,(HL)
            {0x3B, 1}, // DEC SP
            {0x3C, 1}, // INC A
            {0x3D, 1}, // DEC A
            {0x3E, 2}, // LD A,n
            {0x3F, 1}, // CCF
            {0x40, 1}, // LD B,B
            {0x41, 1}, // LD B,C
            {0x42, 1}, // LD B,D
            {0x43, 1}, // LD B,E
            {0x44, 1}, // LD B,H
            {0x45, 1}, // LD B,L
            {0x46, 1}, // LD B,(HL)
            {0x47, 1}, // LD B,A
            {0x48, 1}, // LD C,B
            {0x49, 1}, // LD C,C
            {0x4A, 1}, // LD C,D
            {0x4B, 1}, // LD C,E
            {0x4C, 1}, // LD C,H
            {0x4D, 1}, // LD C,L
            {0x4E, 1}, // LD C,(HL)
            {0x4F, 1}, // LD C,A
            {0x50, 1}, // LD D,B
            {0x51, 1}, // LD D,C
            {0x52, 1}, // LD D,D
            {0x53, 1}, // LD D,E
            {0x54, 1}, // LD D,H
            {0x55, 1}, // LD D,L
            {0x56, 1}, // LD D,(HL)
            {0x57, 1}, // LD D,A
            {0x58, 1}, // LD E,B
            {0x59, 1}, // LD E,C
            {0x5A, 1}, // LD E,D
            {0x5B, 1}, // LD E,E
            {0x5C, 1}, // LD E,H
            {0x5D, 1}, // LD E,L
            {0x5E, 1}, // LD E,(HL)
            {0x5F, 1}, // LD E,A
            {0x60, 1}, // LD H,B
            {0x61, 1}, // LD H,C
            {0x62, 1}, // LD H,D
            {0x63, 1}, // LD H,E
            {0x64, 1}, // LD H,H
            {0x65, 1}, // LD H,L
            {0x66, 1}, // LD H,(HL)
            {0x67, 1}, // LD H,A
            {0x68, 1}, // LD L,B
            {0x69, 1}, // LD L,C
            {0x6A, 1}, // LD L,D
            {0x6B, 1}, // LD L,E
            {0x6C, 1}, // LD L,H
            {0x6D, 1}, // LD L,L
            {0x6E, 1}, // LD L,(HL)
            {0x6F, 1}, // LD L,A
            {0x70, 1}, // LD (HL),B
            {0x71, 1}, // LD (HL),C
            {0x72, 1}, // LD (HL),D
            {0x73, 1}, // LD (HL),E
            {0x74, 1}, // LD (HL),H
            {0x75, 1}, // LD (HL),L
            {0x76, 1}, // HALT
            {0x77, 1}, // LD (HL),A
            {0x78, 1}, // LD A,B
            {0x79, 1}, // LD A,C
            {0x7A, 1}, // LD A,D
            {0x7B, 1}, // LD A,E
            {0x7C, 1}, // LD A,H
            {0x7D, 1}, // LD A,L
            {0x7E, 1}, // LD A,(HL)
            {0x7F, 1}, // LD A,A
            {0x80, 1}, // ADD A,B
            {0x81, 1}, // ADD A,C
            {0x82, 1}, // ADD A,D
            {0x83, 1}, // ADD A,E
            {0x84, 1}, // ADD A,H
            {0x85, 1}, // ADD A,L
            {0x86, 1}, // ADD A,(HL)
            {0x87, 1}, // ADD A,A
            {0x88, 1}, // ADC A,B
            {0x89, 1}, // ADC A,C
            {0x8A, 1}, // ADC A,D
            {0x8B, 1}, // ADC A,E
            {0x8C, 1}, // ADC A,H
            {0x8D, 1}, // ADC A,L
            {0x8E, 1}, // ADC A,(HL)
            {0x8F, 1}, // ADC A,A
            {0x90, 1}, // SUB A,B
            {0x91, 1}, // SUB A,C
            {0x92, 1}, // SUB A,D
            {0x93, 1}, // SUB A,E
            {0x94, 1}, // SUB A,H
            {0x95, 1}, // SUB A,L
            {0x96, 1}, // SUB A,(HL)
            {0x97, 1}, // SUB A,A
            {0x98, 1}, // SBC A,B
            {0x99, 1}, // SBC A,C
            {0x9A, 1}, // SBC A,D
            {0x9B, 1}, // SBC A,E
            {0x9C, 1}, // SBC A,H
            {0x9D, 1}, // SBC A,L
            {0x9E, 1}, // SBC A,(HL)
            {0x9F, 1}, // SBC A,A
            {0xA0, 1}, // AND B
            {0xA1, 1}, // AND C
            {0xA2, 1}, // AND D
            {0xA3, 1}, // AND E
            {0xA4, 1}, // AND H
            {0xA5, 1}, // AND L
            {0xA6, 1}, // AND (HL)
            {0xA7, 1}, // AND A
            {0xA8, 1}, // XOR B
            {0xA9, 1}, // XOR C
            {0xAA, 1}, // XOR D
            {0xAB, 1}, // XOR E
            {0xAC, 1}, // XOR H
            {0xAD, 1}, // XOR L
            {0xAE, 1}, // XOR (HL)
            {0xAF, 1}, // XOR A
            {0xB0, 1}, // OR B
            {0xB1, 1}, // OR C
            {0xB2, 1}, // OR D
            {0xB3, 1}, // OR E
            {0xB4, 1}, // OR H
            {0xB5, 1}, // OR L
            {0xB6, 1}, // OR (HL)
            {0xB7, 1}, // OR A
            {0xB8, 1}, // CP B
            {0xB9, 1}, // CP C
            {0xBA, 1}, // CP D
            {0xBB, 1}, // CP E
            {0xBC, 1}, // CP H
            {0xBD, 1}, // CP L
            {0xBE, 1}, // CP (HL)
            {0xBF, 1}, // CP A
            {0xC0, 1}, // RET NZ
            {0xC1, 1}, // POP BC
            {0xC2, 3}, // JP NZ,nn
            {0xC3, 3}, // JP nn
            {0xC4, 3}, // CALL NZ,nn
            {0xC5, 1}, // PUSH BC
            {0xC6, 2}, // ADD A,n
            {0xC7, 1}, // RST 0
            {0xC8, 1}, // RET Z
            {0xC9, 1}, // RET
            {0xCA, 3}, // JP Z,nn
            {0xCB, 2}, // Ext ops
            {0xCC, 3}, // CALL Z,nn
            {0xCD, 3}, // CALL nn
            {0xCE, 2}, // ADC A,n
            {0xCF, 1}, // RST 8
            {0xD0, 1}, // RET NC
            {0xD1, 1}, // POP DE
            {0xD2, 3}, // JP NC,nn
            // {0xD3, }, // XX
            {0xD4, 3}, // CALL NC,nn
            {0xD5, 1}, // PUSH DE
            {0xD6, 2}, // SUB A,n
            {0xD7, 1}, // RST 10
            {0xD8, 1}, // RET C
            {0xD9, 1}, // RETI
            {0xDA, 3}, // JP C,nn
            // {0xDB, }, // XX
            {0xDC, 3}, // CALL C,nn
            // {0xDD, }, // XX
            {0xDE, 2}, // SBC A,n
            {0xDF, 1}, // RST 18
            {0xE0, 2}, // LDH (n),A
            {0xE1, 1}, // POP HL
            {0xE2, 2}, // LDH (C),A
            // {0xE3, }, // XX
            // {0xE4, }, // XX
            {0xE5, 1}, // PUSH HL
            {0xE6, 2}, // AND n
            {0xE7, 1}, // RST 20
            {0xE8, 2}, // ADD SP,d
            {0xE9, 1}, // JP (HL)
            {0xEA, 3}, // LD (nn),A
            // {0xEB, }, // XX
            // {0xEC, }, // XX
            // {0xED, }, // XX
            {0xEE, 2}, // XOR n
            {0xEF, 1}, // RST 28
            {0xF0, 2}, // LDH A,(n)
            {0xF1, 1}, // POP AF
            {0xF2, 2}, // LDH A, (C)
            {0xF3, 1}, // DI
            // {0xF4, }, // XX
            {0xF5, 1}, // PUSH AF
            {0xF6, 2}, // OR n
            {0xF7, 1}, // RST 30
            {0xF8, 2}, // LDHL SP,d
            {0xF9, 1}, // LD SP,HL
            {0xFA, 3}, // LD A,(nn)
            {0xFB, 1}, // EI
            // {0xFC, }, // XX
            // {0xFD, }, // XX
            {0xFE, 2}, // CP n
            {0xFF, 1} // RST 38
        };
    }
  }
}
