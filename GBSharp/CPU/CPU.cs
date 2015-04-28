using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPU
{
  class CPU : ICPU
  {
    internal CPURegisters registers;
    internal Memory.Memory memory;

    #region Lengths and clocks
    Dictionary<byte, byte> instructionLengths = new Dictionary<byte, byte>() {
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

    Dictionary<byte, byte> instructionClocks = new Dictionary<byte, byte>{
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
            // {0xE4, }, // XX
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
            // {0xFE, 8}, // CP n
            {0xFF, 16} // RST 38
        };

    Dictionary<byte, byte> CBInstructionLengths = new Dictionary<byte, byte>() {
            {0x00, 2}, // RLC B
            {0x01, 2}, // RLC C
            {0x02, 2}, // RLC D
            {0x03, 2}, // RLC E
            {0x04, 2}, // RLC H
            {0x05, 2}, // RLC L
            {0x06, 2}, // RLC (HL)
            {0x07, 2}, // RLC A
            {0x08, 2}, // RRC B
            {0x09, 2}, // RRC C
            {0x0A, 2}, // RRC D
            {0x0B, 2}, // RRC E
            {0x0C, 2}, // RRC H
            {0x0D, 2}, // RRC L
            {0x0E, 2}, // RRC (HL)
            {0x0F, 2}, // RRC A
            {0x10, 2}, // RL B
            {0x11, 2}, // RL C
            {0x12, 2}, // RL D
            {0x13, 2}, // RL E
            {0x14, 2}, // RL H
            {0x15, 2}, // RL L
            {0x16, 2}, // RL (HL)
            {0x17, 2}, // RL A
            {0x18, 2}, // RR B
            {0x19, 2}, // RR C
            {0x1A, 2}, // RR D
            {0x1B, 2}, // RR E
            {0x1C, 2}, // RR H
            {0x1D, 2}, // RR L
            {0x1E, 2}, // RR (HL)
            {0x1F, 2}, // RR A
            {0x20, 2}, // SLA B
            {0x21, 2}, // SLA C
            {0x22, 2}, // SLA D
            {0x23, 2}, // SLA E
            {0x24, 2}, // SLA H
            {0x25, 2}, // SLA L
            {0x26, 2}, // SLA (HL)
            {0x27, 2}, // SLA A
            {0x28, 2}, // SRA B
            {0x29, 2}, // SRA C
            {0x2A, 2}, // SRA D
            {0x2B, 2}, // SRA E
            {0x2C, 2}, // SRA H
            {0x2D, 2}, // SRA L
            {0x2E, 2}, // SRA (HL)
            {0x2F, 2}, // SRA A
            {0x30, 2}, // SWAP B
            {0x31, 2}, // SWAP C
            {0x32, 2}, // SWAP D
            {0x33, 2}, // SWAP E
            {0x34, 2}, // SWAP H
            {0x35, 2}, // SWAP L
            {0x36, 2}, // SWAP (HL)
            {0x37, 2}, // SWAP A
            {0x38, 2}, // SRL B
            {0x39, 2}, // SRL C
            {0x3A, 2}, // SRL D
            {0x3B, 2}, // SRL E
            {0x3C, 2}, // SRL H
            {0x3D, 2}, // SRL L
            {0x3E, 2}, // SRL (HL)
            {0x3F, 2}, // SRL A
            {0x40, 2}, // BIT 0,B
            {0x41, 2}, // BIT 0,C
            {0x42, 2}, // BIT 0,D
            {0x43, 2}, // BIT 0,E
            {0x44, 2}, // BIT 0,H
            {0x45, 2}, // BIT 0,L
            {0x46, 2}, // BIT 0,(HL)
            {0x47, 2}, // BIT 0,A
            {0x48, 2}, // BIT 1,B
            {0x49, 2}, // BIT 1,C
            {0x4A, 2}, // BIT 1,D
            {0x4B, 2}, // BIT 1,E
            {0x4C, 2}, // BIT 1,H
            {0x4D, 2}, // BIT 1,L
            {0x4E, 2}, // BIT 1,(HL)
            {0x4F, 2}, // BIT 1,A
            {0x50, 2}, // BIT 2,B
            {0x51, 2}, // BIT 2,C
            {0x52, 2}, // BIT 2,D
            {0x53, 2}, // BIT 2,E
            {0x54, 2}, // BIT 2,H
            {0x55, 2}, // BIT 2,L
            {0x56, 2}, // BIT 2,(HL)
            {0x57, 2}, // BIT 2,A
            {0x58, 2}, // BIT 3,B
            {0x59, 2}, // BIT 3,C
            {0x5A, 2}, // BIT 3,D
            {0x5B, 2}, // BIT 3,E
            {0x5C, 2}, // BIT 3,H
            {0x5D, 2}, // BIT 3,L
            {0x5E, 2}, // BIT 3,(HL)
            {0x5F, 2}, // BIT 3,A
            {0x60, 2}, // BIT 4,B
            {0x61, 2}, // BIT 4,C
            {0x62, 2}, // BIT 4,D
            {0x63, 2}, // BIT 4,E
            {0x64, 2}, // BIT 4,H
            {0x65, 2}, // BIT 4,L
            {0x66, 2}, // BIT 4,(HL)
            {0x67, 2}, // BIT 4,A
            {0x68, 2}, // BIT 5,B
            {0x69, 2}, // BIT 5,C
            {0x6A, 2}, // BIT 5,D
            {0x6B, 2}, // BIT 5,E
            {0x6C, 2}, // BIT 5,H
            {0x6D, 2}, // BIT 5,L
            {0x6E, 2}, // BIT 5,(HL)
            {0x6F, 2}, // BIT 5,A
            {0x70, 2}, // BIT 6,B
            {0x71, 2}, // BIT 6,C
            {0x72, 2}, // BIT 6,D
            {0x73, 2}, // BIT 6,E
            {0x74, 2}, // BIT 6,H
            {0x75, 2}, // BIT 6,L
            {0x76, 2}, // BIT 6,(HL)
            {0x77, 2}, // BIT 6,A
            {0x78, 2}, // BIT 7,B
            {0x79, 2}, // BIT 7,C
            {0x7A, 2}, // BIT 7,D
            {0x7B, 2}, // BIT 7,E
            {0x7C, 2}, // BIT 7,H
            {0x7D, 2}, // BIT 7,L
            {0x7E, 2}, // BIT 7,(HL)
            {0x7F, 2}, // BIT 7,A
            {0x80, 2}, // RES 0,B
            {0x81, 2}, // RES 0,C
            {0x82, 2}, // RES 0,D
            {0x83, 2}, // RES 0,E
            {0x84, 2}, // RES 0,H
            {0x85, 2}, // RES 0,L
            {0x86, 2}, // RES 0,(HL)
            {0x87, 2}, // RES 0,A
            {0x88, 2}, // RES 1,B
            {0x89, 2}, // RES 1,C
            {0x8A, 2}, // RES 1,D
            {0x8B, 2}, // RES 1,E
            {0x8C, 2}, // RES 1,H
            {0x8D, 2}, // RES 1,L
            {0x8E, 2}, // RES 1,(HL)
            {0x8F, 2}, // RES 1,A
            {0x90, 2}, // RES 2,B
            {0x91, 2}, // RES 2,C
            {0x92, 2}, // RES 2,D
            {0x93, 2}, // RES 2,E
            {0x94, 2}, // RES 2,H
            {0x95, 2}, // RES 2,L
            {0x96, 2}, // RES 2,(HL)
            {0x97, 2}, // RES 2,A
            {0x98, 2}, // RES 3,B
            {0x99, 2}, // RES 3,C
            {0x9A, 2}, // RES 3,D
            {0x9B, 2}, // RES 3,E
            {0x9C, 2}, // RES 3,H
            {0x9D, 2}, // RES 3,L
            {0x9E, 2}, // RES 3,(HL)
            {0x9F, 2}, // RES 3,A
            {0xA0, 2}, // RES 4,B
            {0xA1, 2}, // RES 4,C
            {0xA2, 2}, // RES 4,D
            {0xA3, 2}, // RES 4,E
            {0xA4, 2}, // RES 4,H
            {0xA5, 2}, // RES 4,L
            {0xA6, 2}, // RES 4,(HL)
            {0xA7, 2}, // RES 4,A
            {0xA8, 2}, // RES 5,B
            {0xA9, 2}, // RES 5,C
            {0xAA, 2}, // RES 5,D
            {0xAB, 2}, // RES 5,E
            {0xAC, 2}, // RES 5,H
            {0xAD, 2}, // RES 5,L
            {0xAE, 2}, // RES 5,(HL)
            {0xAF, 2}, // RES 5,A
            {0xB0, 2}, // RES 6,B
            {0xB1, 2}, // RES 6,C
            {0xB2, 2}, // RES 6,D
            {0xB3, 2}, // RES 6,E
            {0xB4, 2}, // RES 6,H
            {0xB5, 2}, // RES 6,L
            {0xB6, 2}, // RES 6,(HL)
            {0xB7, 2}, // RES 6,A
            {0xB8, 2}, // RES 7,B
            {0xB9, 2}, // RES 7,C
            {0xBA, 2}, // RES 7,D
            {0xBB, 2}, // RES 7,E
            {0xBC, 2}, // RES 7,H
            {0xBD, 2}, // RES 7,L
            {0xBE, 2}, // RES 7,(HL)
            {0xBF, 2}, // RES 7,A
            {0xC0, 2}, // SET 0,B
            {0xC1, 2}, // SET 0,C
            {0xC2, 2}, // SET 0,D
            {0xC3, 2}, // SET 0,E
            {0xC4, 2}, // SET 0,H
            {0xC5, 2}, // SET 0,L
            {0xC6, 2}, // SET 0,(HL)
            {0xC7, 2}, // SET 0,A
            {0xC8, 2}, // SET 1,B
            {0xC9, 2}, // SET 1,C
            {0xCA, 2}, // SET 1,D
            {0xCB, 2}, // SET 1,E
            {0xCC, 2}, // SET 1,H
            {0xCD, 2}, // SET 1,L
            {0xCE, 2}, // SET 1,(HL)
            {0xCF, 2}, // SET 1,A
            {0xD0, 2}, // SET 2,B
            {0xD1, 2}, // SET 2,C
            {0xD2, 2}, // SET 2,D
            {0xD3, 2}, // SET 2,E
            {0xD4, 2}, // SET 2,H
            {0xD5, 2}, // SET 2,L
            {0xD6, 2}, // SET 2,(HL)
            {0xD7, 2}, // SET 2,A
            {0xD8, 2}, // SET 3,B
            {0xD9, 2}, // SET 3,C
            {0xDA, 2}, // SET 3,D
            {0xDB, 2}, // SET 3,E
            {0xDC, 2}, // SET 3,H
            {0xDD, 2}, // SET 3,L
            {0xDE, 2}, // SET 3,(HL)
            {0xDF, 2}, // SET 3,A
            {0xE0, 2}, // SET 4,B
            {0xE1, 2}, // SET 4,C
            {0xE2, 2}, // SET 4,D
            {0xE3, 2}, // SET 4,E
            {0xE4, 2}, // SET 4,H
            {0xE5, 2}, // SET 4,L
            {0xE6, 2}, // SET 4,(HL)
            {0xE7, 2}, // SET 4,A
            {0xE8, 2}, // SET 5,B
            {0xE9, 2}, // SET 5,C
            {0xEA, 2}, // SET 5,D
            {0xEB, 2}, // SET 5,E
            {0xEC, 2}, // SET 5,H
            {0xED, 2}, // SET 5,L
            {0xEE, 2}, // SET 5,(HL)
            {0xEF, 2}, // SET 5,A
            {0xF0, 2}, // SET 6,B
            {0xF1, 2}, // SET 6,C
            {0xF2, 2}, // SET 6,D
            {0xF3, 2}, // SET 6,E
            {0xF4, 2}, // SET 6,H
            {0xF5, 2}, // SET 6,L
            {0xF6, 2}, // SET 6,(HL)
            {0xF7, 2}, // SET 6,A
            {0xF8, 2}, // SET 7,B
            {0xF9, 2}, // SET 7,C
            {0xFA, 2}, // SET 7,D
            {0xFB, 2}, // SET 7,E
            {0xFC, 2}, // SET 7,H
            {0xFD, 2}, // SET 7,L
            {0xFE, 2}, // SET 7,(HL)
            {0xFF, 2} // SET 7,A
        };

    Dictionary<byte, byte> CBInstructionClocks = new Dictionary<byte, byte>(){
            {0x00, 8}, // RLC B
            {0x01, 8}, // RLC C
            {0x02, 8}, // RLC D
            {0x03, 8}, // RLC E
            {0x04, 8}, // RLC H
            {0x05, 8}, // RLC L
            {0x06, 16}, // RLC (HL)
            {0x07, 8}, // RLC A
            {0x08, 8}, // RRC B
            {0x09, 8}, // RRC C
            {0x0A, 8}, // RRC D
            {0x0B, 8}, // RRC E
            {0x0C, 8}, // RRC H
            {0x0D, 8}, // RRC L
            {0x0E, 16}, // RRC (HL)
            {0x0F, 8}, // RRC A
            {0x10, 8}, // RL B
            {0x11, 8}, // RL C
            {0x12, 8}, // RL D
            {0x13, 8}, // RL E
            {0x14, 8}, // RL H
            {0x15, 8}, // RL L
            {0x16, 16}, // RL (HL)
            {0x17, 8}, // RL A
            {0x18, 8}, // RR B
            {0x19, 8}, // RR C
            {0x1A, 8}, // RR D
            {0x1B, 8}, // RR E
            {0x1C, 8}, // RR H
            {0x1D, 8}, // RR L
            {0x1E, 16}, // RR (HL)
            {0x1F, 8}, // RR A
            {0x20, 8}, // SLA B
            {0x21, 8}, // SLA C
            {0x22, 8}, // SLA D
            {0x23, 8}, // SLA E
            {0x24, 8}, // SLA H
            {0x25, 8}, // SLA L
            {0x26, 16}, // SLA (HL)
            {0x27, 8}, // SLA A
            {0x28, 8}, // SRA B
            {0x29, 8}, // SRA C
            {0x2A, 8}, // SRA D
            {0x2B, 8}, // SRA E
            {0x2C, 8}, // SRA H
            {0x2D, 8}, // SRA L
            {0x2E, 16}, // SRA (HL)
            {0x2F, 8}, // SRA A
            {0x30, 8}, // SWAP B
            {0x31, 8}, // SWAP C
            {0x32, 8}, // SWAP D
            {0x33, 8}, // SWAP E
            {0x34, 8}, // SWAP H
            {0x35, 8}, // SWAP L
            {0x36, 16}, // SWAP (HL)
            {0x37, 8}, // SWAP A
            {0x38, 8}, // SRL B
            {0x39, 8}, // SRL C
            {0x3A, 8}, // SRL D
            {0x3B, 8}, // SRL E
            {0x3C, 8}, // SRL H
            {0x3D, 8}, // SRL L
            {0x3E, 16}, // SRL (HL)
            {0x3F, 8}, // SRL A
            {0x40, 8}, // BIT 0,B
            {0x41, 8}, // BIT 0,C
            {0x42, 8}, // BIT 0,D
            {0x43, 8}, // BIT 0,E
            {0x44, 8}, // BIT 0,H
            {0x45, 8}, // BIT 0,L
            {0x46, 16}, // BIT 0,(HL)
            {0x47, 8}, // BIT 0,A
            {0x48, 8}, // BIT 1,B
            {0x49, 8}, // BIT 1,C
            {0x4A, 8}, // BIT 1,D
            {0x4B, 8}, // BIT 1,E
            {0x4C, 8}, // BIT 1,H
            {0x4D, 8}, // BIT 1,L
            {0x4E, 16}, // BIT 1,(HL)
            {0x4F, 8}, // BIT 1,A
            {0x50, 8}, // BIT 2,B
            {0x51, 8}, // BIT 2,C
            {0x52, 8}, // BIT 2,D
            {0x53, 8}, // BIT 2,E
            {0x54, 8}, // BIT 2,H
            {0x55, 8}, // BIT 2,L
            {0x56, 16}, // BIT 2,(HL)
            {0x57, 8}, // BIT 2,A
            {0x58, 8}, // BIT 3,B
            {0x59, 8}, // BIT 3,C
            {0x5A, 8}, // BIT 3,D
            {0x5B, 8}, // BIT 3,E
            {0x5C, 8}, // BIT 3,H
            {0x5D, 8}, // BIT 3,L
            {0x5E, 16}, // BIT 3,(HL)
            {0x5F, 8}, // BIT 3,A
            {0x60, 8}, // BIT 4,B
            {0x61, 8}, // BIT 4,C
            {0x62, 8}, // BIT 4,D
            {0x63, 8}, // BIT 4,E
            {0x64, 8}, // BIT 4,H
            {0x65, 8}, // BIT 4,L
            {0x66, 16}, // BIT 4,(HL)
            {0x67, 8}, // BIT 4,A
            {0x68, 8}, // BIT 5,B
            {0x69, 8}, // BIT 5,C
            {0x6A, 8}, // BIT 5,D
            {0x6B, 8}, // BIT 5,E
            {0x6C, 8}, // BIT 5,H
            {0x6D, 8}, // BIT 5,L
            {0x6E, 16}, // BIT 5,(HL)
            {0x6F, 8}, // BIT 5,A
            {0x70, 8}, // BIT 6,B
            {0x71, 8}, // BIT 6,C
            {0x72, 8}, // BIT 6,D
            {0x73, 8}, // BIT 6,E
            {0x74, 8}, // BIT 6,H
            {0x75, 8}, // BIT 6,L
            {0x76, 16}, // BIT 6,(HL)
            {0x77, 8}, // BIT 6,A
            {0x78, 8}, // BIT 7,B
            {0x79, 8}, // BIT 7,C
            {0x7A, 8}, // BIT 7,D
            {0x7B, 8}, // BIT 7,E
            {0x7C, 8}, // BIT 7,H
            {0x7D, 8}, // BIT 7,L
            {0x7E, 16}, // BIT 7,(HL)
            {0x7F, 8}, // BIT 7,A
            {0x80, 8}, // RES 0,B
            {0x81, 8}, // RES 0,C
            {0x82, 8}, // RES 0,D
            {0x83, 8}, // RES 0,E
            {0x84, 8}, // RES 0,H
            {0x85, 8}, // RES 0,L
            {0x86, 16}, // RES 0,(HL)
            {0x87, 8}, // RES 0,A
            {0x88, 8}, // RES 1,B
            {0x89, 8}, // RES 1,C
            {0x8A, 8}, // RES 1,D
            {0x8B, 8}, // RES 1,E
            {0x8C, 8}, // RES 1,H
            {0x8D, 8}, // RES 1,L
            {0x8E, 16}, // RES 1,(HL)
            {0x8F, 8}, // RES 1,A
            {0x90, 8}, // RES 2,B
            {0x91, 8}, // RES 2,C
            {0x92, 8}, // RES 2,D
            {0x93, 8}, // RES 2,E
            {0x94, 8}, // RES 2,H
            {0x95, 8}, // RES 2,L
            {0x96, 16}, // RES 2,(HL)
            {0x97, 8}, // RES 2,A
            {0x98, 8}, // RES 3,B
            {0x99, 8}, // RES 3,C
            {0x9A, 8}, // RES 3,D
            {0x9B, 8}, // RES 3,E
            {0x9C, 8}, // RES 3,H
            {0x9D, 8}, // RES 3,L
            {0x9E, 16}, // RES 3,(HL)
            {0x9F, 8}, // RES 3,A
            {0xA0, 8}, // RES 4,B
            {0xA1, 8}, // RES 4,C
            {0xA2, 8}, // RES 4,D
            {0xA3, 8}, // RES 4,E
            {0xA4, 8}, // RES 4,H
            {0xA5, 8}, // RES 4,L
            {0xA6, 16}, // RES 4,(HL)
            {0xA7, 8}, // RES 4,A
            {0xA8, 8}, // RES 5,B
            {0xA9, 8}, // RES 5,C
            {0xAA, 8}, // RES 5,D
            {0xAB, 8}, // RES 5,E
            {0xAC, 8}, // RES 5,H
            {0xAD, 8}, // RES 5,L
            {0xAE, 16}, // RES 5,(HL)
            {0xAF, 8}, // RES 5,A
            {0xB0, 8}, // RES 6,B
            {0xB1, 8}, // RES 6,C
            {0xB2, 8}, // RES 6,D
            {0xB3, 8}, // RES 6,E
            {0xB4, 8}, // RES 6,H
            {0xB5, 8}, // RES 6,L
            {0xB6, 16}, // RES 6,(HL)
            {0xB7, 8}, // RES 6,A
            {0xB8, 8}, // RES 7,B
            {0xB9, 8}, // RES 7,C
            {0xBA, 8}, // RES 7,D
            {0xBB, 8}, // RES 7,E
            {0xBC, 8}, // RES 7,H
            {0xBD, 8}, // RES 7,L
            {0xBE, 16}, // RES 7,(HL)
            {0xBF, 8}, // RES 7,A
            {0xC0, 8}, // SET 0,B
            {0xC1, 8}, // SET 0,C
            {0xC2, 8}, // SET 0,D
            {0xC3, 8}, // SET 0,E
            {0xC4, 8}, // SET 0,H
            {0xC5, 8}, // SET 0,L
            {0xC6, 16}, // SET 0,(HL)
            {0xC7, 8}, // SET 0,A
            {0xC8, 8}, // SET 1,B
            {0xC9, 8}, // SET 1,C
            {0xCA, 8}, // SET 1,D
            {0xCB, 8}, // SET 1,E
            {0xCC, 8}, // SET 1,H
            {0xCD, 8}, // SET 1,L
            {0xCE, 16}, // SET 1,(HL)
            {0xCF, 8}, // SET 1,A
            {0xD0, 8}, // SET 2,B
            {0xD1, 8}, // SET 2,C
            {0xD2, 8}, // SET 2,D
            {0xD3, 8}, // SET 2,E
            {0xD4, 8}, // SET 2,H
            {0xD5, 8}, // SET 2,L
            {0xD6, 16}, // SET 2,(HL)
            {0xD7, 8}, // SET 2,A
            {0xD8, 8}, // SET 3,B
            {0xD9, 8}, // SET 3,C
            {0xDA, 8}, // SET 3,D
            {0xDB, 8}, // SET 3,E
            {0xDC, 8}, // SET 3,H
            {0xDD, 8}, // SET 3,L
            {0xDE, 16}, // SET 3,(HL)
            {0xDF, 8}, // SET 3,A
            {0xE0, 8}, // SET 4,B
            {0xE1, 8}, // SET 4,C
            {0xE2, 8}, // SET 4,D
            {0xE3, 8}, // SET 4,E
            {0xE4, 8}, // SET 4,H
            {0xE5, 8}, // SET 4,L
            {0xE6, 16}, // SET 4,(HL)
            {0xE7, 8}, // SET 4,A
            {0xE8, 8}, // SET 5,B
            {0xE9, 8}, // SET 5,C
            {0xEA, 8}, // SET 5,D
            {0xEB, 8}, // SET 5,E
            {0xEC, 8}, // SET 5,H
            {0xED, 8}, // SET 5,L
            {0xEE, 16}, // SET 5,(HL)
            {0xEF, 8}, // SET 5,A
            {0xF0, 8}, // SET 6,B
            {0xF1, 8}, // SET 6,C
            {0xF2, 8}, // SET 6,D
            {0xF3, 8}, // SET 6,E
            {0xF4, 8}, // SET 6,H
            {0xF5, 8}, // SET 6,L
            {0xF6, 16}, // SET 6,(HL)
            {0xF7, 8}, // SET 6,A
            {0xF8, 8}, // SET 7,B
            {0xF9, 8}, // SET 7,C
            {0xFA, 8}, // SET 7,D
            {0xFB, 8}, // SET 7,E
            {0xFC, 8}, // SET 7,H
            {0xFD, 8}, // SET 7,L
            {0xFE, 16}, // SET 7,(HL)
            {0xFF, 8}, // SET 7,A
        };
    #endregion

    #region Instruction Lambdas

    private Dictionary<byte, Action<ushort>> instructionLambdas;

    private void CreateInstructionLambdas()
    {
      instructionLambdas = new Dictionary<byte, Action<ushort>>() {
            {0x00, (n) => { }}, // NOP
            {0x01, (n) => { registers.BC = n; }}, // LD BC,nn
            {0x02, (n) => { memory.Write(registers.BC, registers.A); }}, // LD (BC),A
            {0x03, (n) => { registers.BC++; }}, // INC BC
            {0x04, (n) => { registers.B++; }}, // INC B
            {0x05, (n) => {registers.B--;}}, // DEC B
            {0x06, (n) => { registers.B = (byte)n; }}, // LD B,n
            {0x07, (n) => { throw new NotImplementedException(); }}, // RLC A
            {0x08, (n) => memory.Write(n, registers.SP)}, // LD (nn),SP
            {0x09, (n) => { registers.HL += registers.BC; }}, // ADD HL,BC
            {0x0A, (n) => registers.A = memory.Read(registers.BC)}, // LD A,(BC)
            {0x0B, (n) => { registers.BC--; }}, // DEC BC
            {0x0C, (n) => { registers.C++; }}, // INC C
            {0x0D, (n) => { registers.C--; }}, // DEC C
            {0x0E, (n)=>{throw new NotImplementedException();}}, // LD C,n
            {0x0F, (n)=>{throw new NotImplementedException();}}, // RRC A
            {0x10, (n)=>{throw new NotImplementedException();}}, // STOP
            {0x11, (n)=>{throw new NotImplementedException();}}, // LD DE,nn
            {0x12, (n)=>{throw new NotImplementedException();}}, // LD (DE),A
            {0x13, (n)=>{throw new NotImplementedException();}}, // INC DE
            {0x14, (n)=>{throw new NotImplementedException();}}, // INC D
            {0x15, (n)=>{throw new NotImplementedException();}}, // DEC D
            {0x16, (n)=>{throw new NotImplementedException();}}, // LD D,n
            {0x17, (n)=>{throw new NotImplementedException();}}, // RL A
            {0x18, (n)=>{throw new NotImplementedException();}}, // JR n
            {0x19, (n)=>{throw new NotImplementedException();}}, // ADD HL,DE
            {0x1A, (n)=>{throw new NotImplementedException();}}, // LD A,(DE)
            {0x1B, (n)=>{throw new NotImplementedException();}}, // DEC DE
            {0x1C, (n)=>{throw new NotImplementedException();}}, // INC E
            {0x1D, (n)=>{throw new NotImplementedException();}}, // DEC E
            {0x1E, (n)=>{throw new NotImplementedException();}}, // LD E,n
            {0x1F, (n)=>{throw new NotImplementedException();}}, // RR A
            {0x20, (n)=>{throw new NotImplementedException();}}, // JR NZ,n
            {0x21, (n)=>{throw new NotImplementedException();}}, // LD HL,nn
            {0x22, (n)=>{throw new NotImplementedException();}}, // LDI (HL),A
            {0x23, (n)=>{throw new NotImplementedException();}}, // INC HL
            {0x24, (n)=>{throw new NotImplementedException();}}, // INC H
            {0x25, (n)=>{throw new NotImplementedException();}}, // DEC H
            {0x26, (n)=>{throw new NotImplementedException();}}, // LD H,n
            {0x27, (n)=>{throw new NotImplementedException();}}, // DAA
            {0x28, (n)=>{throw new NotImplementedException();}}, // JR Z,n
            {0x29, (n)=>{throw new NotImplementedException();}}, // ADD HL,HL
            {0x2A, (n)=>{throw new NotImplementedException();}}, // LDI A,(HL)
            {0x2B, (n)=>{throw new NotImplementedException();}}, // DEC HL
            {0x2C, (n)=>{throw new NotImplementedException();}}, // INC L
            {0x2D, (n)=>{throw new NotImplementedException();}}, // DEC L
            {0x2E, (n)=>{throw new NotImplementedException();}}, // LD L,n
            {0x2F, (n)=>{throw new NotImplementedException();}}, // CPL
            {0x30, (n)=>{throw new NotImplementedException();}}, // JR NC,n
            {0x31, (n)=>{throw new NotImplementedException();}}, // LD SP,nn
            {0x32, (n)=>{throw new NotImplementedException();}}, // LDD (HL),A
            {0x33, (n)=>{throw new NotImplementedException();}}, // INC SP
            {0x34, (n)=>{throw new NotImplementedException();}}, // INC (HL)
            {0x35, (n)=>{throw new NotImplementedException();}}, // DEC (HL)
            {0x36, (n)=>{throw new NotImplementedException();}}, // LD (HL),n
            {0x37, (n)=>{throw new NotImplementedException();}}, // SCF
            {0x38, (n)=>{throw new NotImplementedException();}}, // JR C,n
            {0x39, (n)=>{throw new NotImplementedException();}}, // ADD HL,SP
            {0x3A, (n)=>{throw new NotImplementedException();}}, // LDD A,(HL)
            {0x3B, (n)=>{throw new NotImplementedException();}}, // DEC SP
            {0x3C, (n)=>{throw new NotImplementedException();}}, // INC A
            {0x3D, (n)=>{throw new NotImplementedException();}}, // DEC A
            {0x3E, (n)=>{throw new NotImplementedException();}}, // LD A,n
            {0x3F, (n)=>{throw new NotImplementedException();}}, // CCF
            {0x40, (n)=>{throw new NotImplementedException();}}, // LD B,B
            {0x41, (n)=>{throw new NotImplementedException();}}, // LD B,C
            {0x42, (n)=>{throw new NotImplementedException();}}, // LD B,D
            {0x43, (n)=>{throw new NotImplementedException();}}, // LD B,E
            {0x44, (n)=>{throw new NotImplementedException();}}, // LD B,H
            {0x45, (n)=>{throw new NotImplementedException();}}, // LD B,L
            {0x46, (n)=>{throw new NotImplementedException();}}, // LD B,(HL)
            {0x47, (n)=>{throw new NotImplementedException();}}, // LD B,A
            {0x48, (n)=>{throw new NotImplementedException();}}, // LD C,B
            {0x49, (n)=>{throw new NotImplementedException();}}, // LD C,C
            {0x4A, (n)=>{throw new NotImplementedException();}}, // LD C,D
            {0x4B, (n)=>{throw new NotImplementedException();}}, // LD C,E
            {0x4C, (n)=>{throw new NotImplementedException();}}, // LD C,H
            {0x4D, (n)=>{throw new NotImplementedException();}}, // LD C,L
            {0x4E, (n)=>{throw new NotImplementedException();}}, // LD C,(HL)
            {0x4F, (n)=>{throw new NotImplementedException();}}, // LD C,A
            {0x50, (n)=>{throw new NotImplementedException();}}, // LD D,B
            {0x51, (n)=>{throw new NotImplementedException();}}, // LD D,C
            {0x52, (n)=>{throw new NotImplementedException();}}, // LD D,D
            {0x53, (n)=>{throw new NotImplementedException();}}, // LD D,E
            {0x54, (n)=>{throw new NotImplementedException();}}, // LD D,H
            {0x55, (n)=>{throw new NotImplementedException();}}, // LD D,L
            {0x56, (n)=>{throw new NotImplementedException();}}, // LD D,(HL)
            {0x57, (n)=>{throw new NotImplementedException();}}, // LD D,A
            {0x58, (n)=>{throw new NotImplementedException();}}, // LD E,B
            {0x59, (n)=>{throw new NotImplementedException();}}, // LD E,C
            {0x5A, (n)=>{throw new NotImplementedException();}}, // LD E,D
            {0x5B, (n)=>{throw new NotImplementedException();}}, // LD E,E
            {0x5C, (n)=>{throw new NotImplementedException();}}, // LD E,H
            {0x5D, (n)=>{throw new NotImplementedException();}}, // LD E,L
            {0x5E, (n)=>{throw new NotImplementedException();}}, // LD E,(HL)
            {0x5F, (n)=>{throw new NotImplementedException();}}, // LD E,A
            {0x60, (n)=>{throw new NotImplementedException();}}, // LD H,B
            {0x61, (n)=>{throw new NotImplementedException();}}, // LD H,C
            {0x62, (n)=>{throw new NotImplementedException();}}, // LD H,D
            {0x63, (n)=>{throw new NotImplementedException();}}, // LD H,E
            {0x64, (n)=>{throw new NotImplementedException();}}, // LD H,H
            {0x65, (n)=>{throw new NotImplementedException();}}, // LD H,L
            {0x66, (n)=>{throw new NotImplementedException();}}, // LD H,(HL)
            {0x67, (n)=>{throw new NotImplementedException();}}, // LD H,A
            {0x68, (n)=>{throw new NotImplementedException();}}, // LD L,B
            {0x69, (n)=>{throw new NotImplementedException();}}, // LD L,C
            {0x6A, (n)=>{throw new NotImplementedException();}}, // LD L,D
            {0x6B, (n)=>{throw new NotImplementedException();}}, // LD L,E
            {0x6C, (n)=>{throw new NotImplementedException();}}, // LD L,H
            {0x6D, (n)=>{throw new NotImplementedException();}}, // LD L,L
            {0x6E, (n)=>{throw new NotImplementedException();}}, // LD L,(HL)
            {0x6F, (n)=>{throw new NotImplementedException();}}, // LD L,A
            {0x70, (n)=>{throw new NotImplementedException();}}, // LD (HL),B
            {0x71, (n)=>{throw new NotImplementedException();}}, // LD (HL),C
            {0x72, (n)=>{throw new NotImplementedException();}}, // LD (HL),D
            {0x73, (n)=>{throw new NotImplementedException();}}, // LD (HL),E
            {0x74, (n)=>{throw new NotImplementedException();}}, // LD (HL),H
            {0x75, (n)=>{throw new NotImplementedException();}}, // LD (HL),L
            {0x76, (n)=>{throw new NotImplementedException();}}, // HALT
            {0x77, (n)=>{throw new NotImplementedException();}}, // LD (HL),A
            {0x78, (n)=>{throw new NotImplementedException();}}, // LD A,B
            {0x79, (n)=>{throw new NotImplementedException();}}, // LD A,C
            {0x7A, (n)=>{throw new NotImplementedException();}}, // LD A,D
            {0x7B, (n)=>{throw new NotImplementedException();}}, // LD A,E
            {0x7C, (n)=>{throw new NotImplementedException();}}, // LD A,H
            {0x7D, (n)=>{throw new NotImplementedException();}}, // LD A,L
            {0x7E, (n)=>{throw new NotImplementedException();}}, // LD A,(HL)
            {0x7F, (n)=>{throw new NotImplementedException();}}, // LD A,A
            {0x80, (n)=>{throw new NotImplementedException();}}, // ADD A,B
            {0x81, (n)=>{throw new NotImplementedException();}}, // ADD A,C
            {0x82, (n)=>{throw new NotImplementedException();}}, // ADD A,D
            {0x83, (n)=>{throw new NotImplementedException();}}, // ADD A,E
            {0x84, (n)=>{throw new NotImplementedException();}}, // ADD A,H
            {0x85, (n)=>{throw new NotImplementedException();}}, // ADD A,L
            {0x86, (n)=>{throw new NotImplementedException();}}, // ADD A,(HL)
            {0x87, (n)=>{throw new NotImplementedException();}}, // ADD A,A
            {0x88, (n)=>{throw new NotImplementedException();}}, // ADC A,B
            {0x89, (n)=>{throw new NotImplementedException();}}, // ADC A,C
            {0x8A, (n)=>{throw new NotImplementedException();}}, // ADC A,D
            {0x8B, (n)=>{throw new NotImplementedException();}}, // ADC A,E
            {0x8C, (n)=>{throw new NotImplementedException();}}, // ADC A,H
            {0x8D, (n)=>{throw new NotImplementedException();}}, // ADC A,L
            {0x8E, (n)=>{throw new NotImplementedException();}}, // ADC A,(HL)
            {0x8F, (n)=>{throw new NotImplementedException();}}, // ADC A,A
            {0x90, (n)=>{throw new NotImplementedException();}}, // SUB A,B
            {0x91, (n)=>{throw new NotImplementedException();}}, // SUB A,C
            {0x92, (n)=>{throw new NotImplementedException();}}, // SUB A,D
            {0x93, (n)=>{throw new NotImplementedException();}}, // SUB A,E
            {0x94, (n)=>{throw new NotImplementedException();}}, // SUB A,H
            {0x95, (n)=>{throw new NotImplementedException();}}, // SUB A,L
            {0x96, (n)=>{throw new NotImplementedException();}}, // SUB A,(HL)
            {0x97, (n)=>{throw new NotImplementedException();}}, // SUB A,A
            {0x98, (n)=>{throw new NotImplementedException();}}, // SBC A,B
            {0x99, (n)=>{throw new NotImplementedException();}}, // SBC A,C
            {0x9A, (n)=>{throw new NotImplementedException();}}, // SBC A,D
            {0x9B, (n)=>{throw new NotImplementedException();}}, // SBC A,E
            {0x9C, (n)=>{throw new NotImplementedException();}}, // SBC A,H
            {0x9D, (n)=>{throw new NotImplementedException();}}, // SBC A,L
            {0x9E, (n)=>{throw new NotImplementedException();}}, // SBC A,(HL)
            {0x9F, (n)=>{throw new NotImplementedException();}}, // SBC A,A
            {0xA0, (n)=>{throw new NotImplementedException();}}, // AND B
            {0xA1, (n)=>{throw new NotImplementedException();}}, // AND C
            {0xA2, (n)=>{throw new NotImplementedException();}}, // AND D
            {0xA3, (n)=>{throw new NotImplementedException();}}, // AND E
            {0xA4, (n)=>{throw new NotImplementedException();}}, // AND H
            {0xA5, (n)=>{throw new NotImplementedException();}}, // AND L
            {0xA6, (n)=>{throw new NotImplementedException();}}, // AND (HL)
            {0xA7, (n)=>{throw new NotImplementedException();}}, // AND A
            {0xA8, (n)=>{throw new NotImplementedException();}}, // XOR B
            {0xA9, (n)=>{throw new NotImplementedException();}}, // XOR C
            {0xAA, (n)=>{throw new NotImplementedException();}}, // XOR D
            {0xAB, (n)=>{throw new NotImplementedException();}}, // XOR E
            {0xAC, (n)=>{throw new NotImplementedException();}}, // XOR H
            {0xAD, (n)=>{throw new NotImplementedException();}}, // XOR L
            {0xAE, (n)=>{throw new NotImplementedException();}}, // XOR (HL)
            {0xAF, (n)=>{throw new NotImplementedException();}}, // XOR A
            {0xB0, (n)=>{throw new NotImplementedException();}}, // OR B
            {0xB1, (n)=>{throw new NotImplementedException();}}, // OR C
            {0xB2, (n)=>{throw new NotImplementedException();}}, // OR D
            {0xB3, (n)=>{throw new NotImplementedException();}}, // OR E
            {0xB4, (n)=>{throw new NotImplementedException();}}, // OR H
            {0xB5, (n)=>{throw new NotImplementedException();}}, // OR L
            {0xB6, (n)=>{throw new NotImplementedException();}}, // OR (HL)
            {0xB7, (n)=>{throw new NotImplementedException();}}, // OR A
            {0xB8, (n)=>{throw new NotImplementedException();}}, // CP B
            {0xB9, (n)=>{throw new NotImplementedException();}}, // CP C
            {0xBA, (n)=>{throw new NotImplementedException();}}, // CP D
            {0xBB, (n)=>{throw new NotImplementedException();}}, // CP E
            {0xBC, (n)=>{throw new NotImplementedException();}}, // CP H
            {0xBD, (n)=>{throw new NotImplementedException();}}, // CP L
            {0xBE, (n)=>{throw new NotImplementedException();}}, // CP (HL)
            {0xBF, (n)=>{throw new NotImplementedException();}}, // CP A
            {0xC0, (n)=>{throw new NotImplementedException();}}, // RET NZ
            {0xC1, (n)=>{throw new NotImplementedException();}}, // POP BC
            {0xC2, (n)=>{throw new NotImplementedException();}}, // JP NZ,nn
            {0xC3, (n)=>{throw new NotImplementedException();}}, // JP nn
            {0xC4, (n)=>{throw new NotImplementedException();}}, // CALL NZ,nn
            {0xC5, (n)=>{throw new NotImplementedException();}}, // PUSH BC
            {0xC6, (n)=>{throw new NotImplementedException();}}, // ADD A,n
            {0xC7, (n)=>{throw new NotImplementedException();}}, // RST 0
            {0xC8, (n)=>{throw new NotImplementedException();}}, // RET Z
            {0xC9, (n)=>{throw new NotImplementedException();}}, // RET
            {0xCA, (n)=>{throw new NotImplementedException();}}, // JP Z,nn
            {0xCB, (n)=>{throw new NotImplementedException();}}, // Ext ops
            {0xCC, (n)=>{throw new NotImplementedException();}}, // CALL Z,nn
            {0xCD, (n)=>{throw new NotImplementedException();}}, // CALL nn
            {0xCE, (n)=>{throw new NotImplementedException();}}, // ADC A,n
            {0xCF, (n)=>{throw new NotImplementedException();}}, // RST 8
            {0xD0, (n)=>{throw new NotImplementedException();}}, // RET NC
            {0xD1, (n)=>{throw new NotImplementedException();}}, // POP DE
            {0xD2, (n)=>{throw new NotImplementedException();}}, // JP NC,nn
            // {0xD3, }, // XX
            {0xD4, (n)=>{throw new NotImplementedException();}}, // CALL NC,nn
            {0xD5, (n)=>{throw new NotImplementedException();}}, // PUSH DE
            {0xD6, (n)=>{throw new NotImplementedException();}}, // SUB A,n
            {0xD7, (n)=>{throw new NotImplementedException();}}, // RST 10
            {0xD8, (n)=>{throw new NotImplementedException();}}, // RET C
            {0xD9, (n)=>{throw new NotImplementedException();}}, // RETI
            {0xDA, (n)=>{throw new NotImplementedException();}}, // JP C,nn
            // {0xDB, }, // XX
            {0xDC, (n)=>{throw new NotImplementedException();}}, // CALL C,nn
            // {0xDD, }, // XX
            {0xDE, (n)=>{throw new NotImplementedException();}}, // SBC A,n
            {0xDF, (n)=>{throw new NotImplementedException();}}, // RST 18
            {0xE0, (n)=>{throw new NotImplementedException();}}, // LDH (n),A
            {0xE1, (n)=>{throw new NotImplementedException();}}, // POP HL
            {0xE2, (n)=>{throw new NotImplementedException();}}, // LDH (C),A
            // {0xE3, }, // XX
            // {0xE4, }, // XX
            {0xE5, (n)=>{throw new NotImplementedException();}}, // PUSH HL
            {0xE6, (n)=>{throw new NotImplementedException();}}, // AND n
            {0xE7, (n)=>{throw new NotImplementedException();}}, // RST 20
            {0xE8, (n)=>{throw new NotImplementedException();}}, // ADD SP,d
            {0xE9, (n)=>{throw new NotImplementedException();}}, // JP (HL)
            {0xEA, (n)=>{throw new NotImplementedException();}}, // LD (nn),A
            // {0xEB, }, // XX
            // {0xEC, }, // XX
            // {0xED, }, // XX
            {0xEE, (n)=>{throw new NotImplementedException();}}, // XOR n
            {0xEF, (n)=>{throw new NotImplementedException();}}, // RST 28
            {0xF0, (n)=>{throw new NotImplementedException();}}, // LDH A,(n)
            {0xF1, (n)=>{throw new NotImplementedException();}}, // POP AF
            {0xF2, (n)=>{throw new NotImplementedException();}}, // LDH A, (C)
            {0xF3, (n)=>{throw new NotImplementedException();}}, // DI
            // {0xF4, }, // XX
            {0xF5, (n)=>{throw new NotImplementedException();}}, // PUSH AF
            {0xF6, (n)=>{throw new NotImplementedException();}}, // OR n
            {0xF7, (n)=>{throw new NotImplementedException();}}, // RST 30
            {0xF8, (n)=>{throw new NotImplementedException();}}, // LDHL SP,d
            {0xF9, (n)=>{throw new NotImplementedException();}}, // LD SP,HL
            {0xFA, (n)=>{throw new NotImplementedException();}}, // LD A,(nn)
            {0xFB, (n)=>{throw new NotImplementedException();}}, // EI
            // {0xFC, }, // XX
            // {0xFD, }, // XX
            {0xFE, (n)=>{throw new NotImplementedException();}}, // CP n
            {0xFF, (n)=>{throw new NotImplementedException();}} // RST 38
        };
    }

    Dictionary<byte, Action<ushort>> CBInstructionLambdas = new Dictionary<byte, Action<ushort>>() {
            {0x00, (n)=>{throw new NotImplementedException();}}, // RLC B
            {0x01, (n)=>{throw new NotImplementedException();}}, // RLC C
            {0x02, (n)=>{throw new NotImplementedException();}}, // RLC D
            {0x03, (n)=>{throw new NotImplementedException();}}, // RLC E
            {0x04, (n)=>{throw new NotImplementedException();}}, // RLC H
            {0x05, (n)=>{throw new NotImplementedException();}}, // RLC L
            {0x06, (n)=>{throw new NotImplementedException();}}, // RLC (HL)
            {0x07, (n)=>{throw new NotImplementedException();}}, // RLC A
            {0x08, (n)=>{throw new NotImplementedException();}}, // RRC B
            {0x09, (n)=>{throw new NotImplementedException();}}, // RRC C
            {0x0A, (n)=>{throw new NotImplementedException();}}, // RRC D
            {0x0B, (n)=>{throw new NotImplementedException();}}, // RRC E
            {0x0C, (n)=>{throw new NotImplementedException();}}, // RRC H
            {0x0D, (n)=>{throw new NotImplementedException();}}, // RRC L
            {0x0E, (n)=>{throw new NotImplementedException();}}, // RRC (HL)
            {0x0F, (n)=>{throw new NotImplementedException();}}, // RRC A
            {0x10, (n)=>{throw new NotImplementedException();}}, // RL B
            {0x11, (n)=>{throw new NotImplementedException();}}, // RL C
            {0x12, (n)=>{throw new NotImplementedException();}}, // RL D
            {0x13, (n)=>{throw new NotImplementedException();}}, // RL E
            {0x14, (n)=>{throw new NotImplementedException();}}, // RL H
            {0x15, (n)=>{throw new NotImplementedException();}}, // RL L
            {0x16, (n)=>{throw new NotImplementedException();}}, // RL (HL)
            {0x17, (n)=>{throw new NotImplementedException();}}, // RL A
            {0x18, (n)=>{throw new NotImplementedException();}}, // RR B
            {0x19, (n)=>{throw new NotImplementedException();}}, // RR C
            {0x1A, (n)=>{throw new NotImplementedException();}}, // RR D
            {0x1B, (n)=>{throw new NotImplementedException();}}, // RR E
            {0x1C, (n)=>{throw new NotImplementedException();}}, // RR H
            {0x1D, (n)=>{throw new NotImplementedException();}}, // RR L
            {0x1E, (n)=>{throw new NotImplementedException();}}, // RR (HL)
            {0x1F, (n)=>{throw new NotImplementedException();}}, // RR A
            {0x20, (n)=>{throw new NotImplementedException();}}, // SLA B
            {0x21, (n)=>{throw new NotImplementedException();}}, // SLA C
            {0x22, (n)=>{throw new NotImplementedException();}}, // SLA D
            {0x23, (n)=>{throw new NotImplementedException();}}, // SLA E
            {0x24, (n)=>{throw new NotImplementedException();}}, // SLA H
            {0x25, (n)=>{throw new NotImplementedException();}}, // SLA L
            {0x26, (n)=>{throw new NotImplementedException();}}, // SLA (HL)
            {0x27, (n)=>{throw new NotImplementedException();}}, // SLA A
            {0x28, (n)=>{throw new NotImplementedException();}}, // SRA B
            {0x29, (n)=>{throw new NotImplementedException();}}, // SRA C
            {0x2A, (n)=>{throw new NotImplementedException();}}, // SRA D
            {0x2B, (n)=>{throw new NotImplementedException();}}, // SRA E
            {0x2C, (n)=>{throw new NotImplementedException();}}, // SRA H
            {0x2D, (n)=>{throw new NotImplementedException();}}, // SRA L
            {0x2E, (n)=>{throw new NotImplementedException();}}, // SRA (HL)
            {0x2F, (n)=>{throw new NotImplementedException();}}, // SRA A
            {0x30, (n)=>{throw new NotImplementedException();}}, // SWAP B
            {0x31, (n)=>{throw new NotImplementedException();}}, // SWAP C
            {0x32, (n)=>{throw new NotImplementedException();}}, // SWAP D
            {0x33, (n)=>{throw new NotImplementedException();}}, // SWAP E
            {0x34, (n)=>{throw new NotImplementedException();}}, // SWAP H
            {0x35, (n)=>{throw new NotImplementedException();}}, // SWAP L
            {0x36, (n)=>{throw new NotImplementedException();}}, // SWAP (HL)
            {0x37, (n)=>{throw new NotImplementedException();}}, // SWAP A
            {0x38, (n)=>{throw new NotImplementedException();}}, // SRL B
            {0x39, (n)=>{throw new NotImplementedException();}}, // SRL C
            {0x3A, (n)=>{throw new NotImplementedException();}}, // SRL D
            {0x3B, (n)=>{throw new NotImplementedException();}}, // SRL E
            {0x3C, (n)=>{throw new NotImplementedException();}}, // SRL H
            {0x3D, (n)=>{throw new NotImplementedException();}}, // SRL L
            {0x3E, (n)=>{throw new NotImplementedException();}}, // SRL (HL)
            {0x3F, (n)=>{throw new NotImplementedException();}}, // SRL A
            {0x40, (n)=>{throw new NotImplementedException();}}, // BIT 0,B
            {0x41, (n)=>{throw new NotImplementedException();}}, // BIT 0,C
            {0x42, (n)=>{throw new NotImplementedException();}}, // BIT 0,D
            {0x43, (n)=>{throw new NotImplementedException();}}, // BIT 0,E
            {0x44, (n)=>{throw new NotImplementedException();}}, // BIT 0,H
            {0x45, (n)=>{throw new NotImplementedException();}}, // BIT 0,L
            {0x46, (n)=>{throw new NotImplementedException();}}, // BIT 0,(HL)
            {0x47, (n)=>{throw new NotImplementedException();}}, // BIT 0,A
            {0x48, (n)=>{throw new NotImplementedException();}}, // BIT 1,B
            {0x49, (n)=>{throw new NotImplementedException();}}, // BIT 1,C
            {0x4A, (n)=>{throw new NotImplementedException();}}, // BIT 1,D
            {0x4B, (n)=>{throw new NotImplementedException();}}, // BIT 1,E
            {0x4C, (n)=>{throw new NotImplementedException();}}, // BIT 1,H
            {0x4D, (n)=>{throw new NotImplementedException();}}, // BIT 1,L
            {0x4E, (n)=>{throw new NotImplementedException();}}, // BIT 1,(HL)
            {0x4F, (n)=>{throw new NotImplementedException();}}, // BIT 1,A
            {0x50, (n)=>{throw new NotImplementedException();}}, // BIT 2,B
            {0x51, (n)=>{throw new NotImplementedException();}}, // BIT 2,C
            {0x52, (n)=>{throw new NotImplementedException();}}, // BIT 2,D
            {0x53, (n)=>{throw new NotImplementedException();}}, // BIT 2,E
            {0x54, (n)=>{throw new NotImplementedException();}}, // BIT 2,H
            {0x55, (n)=>{throw new NotImplementedException();}}, // BIT 2,L
            {0x56, (n)=>{throw new NotImplementedException();}}, // BIT 2,(HL)
            {0x57, (n)=>{throw new NotImplementedException();}}, // BIT 2,A
            {0x58, (n)=>{throw new NotImplementedException();}}, // BIT 3,B
            {0x59, (n)=>{throw new NotImplementedException();}}, // BIT 3,C
            {0x5A, (n)=>{throw new NotImplementedException();}}, // BIT 3,D
            {0x5B, (n)=>{throw new NotImplementedException();}}, // BIT 3,E
            {0x5C, (n)=>{throw new NotImplementedException();}}, // BIT 3,H
            {0x5D, (n)=>{throw new NotImplementedException();}}, // BIT 3,L
            {0x5E, (n)=>{throw new NotImplementedException();}}, // BIT 3,(HL)
            {0x5F, (n)=>{throw new NotImplementedException();}}, // BIT 3,A
            {0x60, (n)=>{throw new NotImplementedException();}}, // BIT 4,B
            {0x61, (n)=>{throw new NotImplementedException();}}, // BIT 4,C
            {0x62, (n)=>{throw new NotImplementedException();}}, // BIT 4,D
            {0x63, (n)=>{throw new NotImplementedException();}}, // BIT 4,E
            {0x64, (n)=>{throw new NotImplementedException();}}, // BIT 4,H
            {0x65, (n)=>{throw new NotImplementedException();}}, // BIT 4,L
            {0x66, (n)=>{throw new NotImplementedException();}}, // BIT 4,(HL)
            {0x67, (n)=>{throw new NotImplementedException();}}, // BIT 4,A
            {0x68, (n)=>{throw new NotImplementedException();}}, // BIT 5,B
            {0x69, (n)=>{throw new NotImplementedException();}}, // BIT 5,C
            {0x6A, (n)=>{throw new NotImplementedException();}}, // BIT 5,D
            {0x6B, (n)=>{throw new NotImplementedException();}}, // BIT 5,E
            {0x6C, (n)=>{throw new NotImplementedException();}}, // BIT 5,H
            {0x6D, (n)=>{throw new NotImplementedException();}}, // BIT 5,L
            {0x6E, (n)=>{throw new NotImplementedException();}}, // BIT 5,(HL)
            {0x6F, (n)=>{throw new NotImplementedException();}}, // BIT 5,A
            {0x70, (n)=>{throw new NotImplementedException();}}, // BIT 6,B
            {0x71, (n)=>{throw new NotImplementedException();}}, // BIT 6,C
            {0x72, (n)=>{throw new NotImplementedException();}}, // BIT 6,D
            {0x73, (n)=>{throw new NotImplementedException();}}, // BIT 6,E
            {0x74, (n)=>{throw new NotImplementedException();}}, // BIT 6,H
            {0x75, (n)=>{throw new NotImplementedException();}}, // BIT 6,L
            {0x76, (n)=>{throw new NotImplementedException();}}, // BIT 6,(HL)
            {0x77, (n)=>{throw new NotImplementedException();}}, // BIT 6,A
            {0x78, (n)=>{throw new NotImplementedException();}}, // BIT 7,B
            {0x79, (n)=>{throw new NotImplementedException();}}, // BIT 7,C
            {0x7A, (n)=>{throw new NotImplementedException();}}, // BIT 7,D
            {0x7B, (n)=>{throw new NotImplementedException();}}, // BIT 7,E
            {0x7C, (n)=>{throw new NotImplementedException();}}, // BIT 7,H
            {0x7D, (n)=>{throw new NotImplementedException();}}, // BIT 7,L
            {0x7E, (n)=>{throw new NotImplementedException();}}, // BIT 7,(HL)
            {0x7F, (n)=>{throw new NotImplementedException();}}, // BIT 7,A
            {0x80, (n)=>{throw new NotImplementedException();}}, // RES 0,B
            {0x81, (n)=>{throw new NotImplementedException();}}, // RES 0,C
            {0x82, (n)=>{throw new NotImplementedException();}}, // RES 0,D
            {0x83, (n)=>{throw new NotImplementedException();}}, // RES 0,E
            {0x84, (n)=>{throw new NotImplementedException();}}, // RES 0,H
            {0x85, (n)=>{throw new NotImplementedException();}}, // RES 0,L
            {0x86, (n)=>{throw new NotImplementedException();}}, // RES 0,(HL)
            {0x87, (n)=>{throw new NotImplementedException();}}, // RES 0,A
            {0x88, (n)=>{throw new NotImplementedException();}}, // RES 1,B
            {0x89, (n)=>{throw new NotImplementedException();}}, // RES 1,C
            {0x8A, (n)=>{throw new NotImplementedException();}}, // RES 1,D
            {0x8B, (n)=>{throw new NotImplementedException();}}, // RES 1,E
            {0x8C, (n)=>{throw new NotImplementedException();}}, // RES 1,H
            {0x8D, (n)=>{throw new NotImplementedException();}}, // RES 1,L
            {0x8E, (n)=>{throw new NotImplementedException();}}, // RES 1,(HL)
            {0x8F, (n)=>{throw new NotImplementedException();}}, // RES 1,A
            {0x90, (n)=>{throw new NotImplementedException();}}, // RES 2,B
            {0x91, (n)=>{throw new NotImplementedException();}}, // RES 2,C
            {0x92, (n)=>{throw new NotImplementedException();}}, // RES 2,D
            {0x93, (n)=>{throw new NotImplementedException();}}, // RES 2,E
            {0x94, (n)=>{throw new NotImplementedException();}}, // RES 2,H
            {0x95, (n)=>{throw new NotImplementedException();}}, // RES 2,L
            {0x96, (n)=>{throw new NotImplementedException();}}, // RES 2,(HL)
            {0x97, (n)=>{throw new NotImplementedException();}}, // RES 2,A
            {0x98, (n)=>{throw new NotImplementedException();}}, // RES 3,B
            {0x99, (n)=>{throw new NotImplementedException();}}, // RES 3,C
            {0x9A, (n)=>{throw new NotImplementedException();}}, // RES 3,D
            {0x9B, (n)=>{throw new NotImplementedException();}}, // RES 3,E
            {0x9C, (n)=>{throw new NotImplementedException();}}, // RES 3,H
            {0x9D, (n)=>{throw new NotImplementedException();}}, // RES 3,L
            {0x9E, (n)=>{throw new NotImplementedException();}}, // RES 3,(HL)
            {0x9F, (n)=>{throw new NotImplementedException();}}, // RES 3,A
            {0xA0, (n)=>{throw new NotImplementedException();}}, // RES 4,B
            {0xA1, (n)=>{throw new NotImplementedException();}}, // RES 4,C
            {0xA2, (n)=>{throw new NotImplementedException();}}, // RES 4,D
            {0xA3, (n)=>{throw new NotImplementedException();}}, // RES 4,E
            {0xA4, (n)=>{throw new NotImplementedException();}}, // RES 4,H
            {0xA5, (n)=>{throw new NotImplementedException();}}, // RES 4,L
            {0xA6, (n)=>{throw new NotImplementedException();}}, // RES 4,(HL)
            {0xA7, (n)=>{throw new NotImplementedException();}}, // RES 4,A
            {0xA8, (n)=>{throw new NotImplementedException();}}, // RES 5,B
            {0xA9, (n)=>{throw new NotImplementedException();}}, // RES 5,C
            {0xAA, (n)=>{throw new NotImplementedException();}}, // RES 5,D
            {0xAB, (n)=>{throw new NotImplementedException();}}, // RES 5,E
            {0xAC, (n)=>{throw new NotImplementedException();}}, // RES 5,H
            {0xAD, (n)=>{throw new NotImplementedException();}}, // RES 5,L
            {0xAE, (n)=>{throw new NotImplementedException();}}, // RES 5,(HL)
            {0xAF, (n)=>{throw new NotImplementedException();}}, // RES 5,A
            {0xB0, (n)=>{throw new NotImplementedException();}}, // RES 6,B
            {0xB1, (n)=>{throw new NotImplementedException();}}, // RES 6,C
            {0xB2, (n)=>{throw new NotImplementedException();}}, // RES 6,D
            {0xB3, (n)=>{throw new NotImplementedException();}}, // RES 6,E
            {0xB4, (n)=>{throw new NotImplementedException();}}, // RES 6,H
            {0xB5, (n)=>{throw new NotImplementedException();}}, // RES 6,L
            {0xB6, (n)=>{throw new NotImplementedException();}}, // RES 6,(HL)
            {0xB7, (n)=>{throw new NotImplementedException();}}, // RES 6,A
            {0xB8, (n)=>{throw new NotImplementedException();}}, // RES 7,B
            {0xB9, (n)=>{throw new NotImplementedException();}}, // RES 7,C
            {0xBA, (n)=>{throw new NotImplementedException();}}, // RES 7,D
            {0xBB, (n)=>{throw new NotImplementedException();}}, // RES 7,E
            {0xBC, (n)=>{throw new NotImplementedException();}}, // RES 7,H
            {0xBD, (n)=>{throw new NotImplementedException();}}, // RES 7,L
            {0xBE, (n)=>{throw new NotImplementedException();}}, // RES 7,(HL)
            {0xBF, (n)=>{throw new NotImplementedException();}}, // RES 7,A
            {0xC0, (n)=>{throw new NotImplementedException();}}, // SET 0,B
            {0xC1, (n)=>{throw new NotImplementedException();}}, // SET 0,C
            {0xC2, (n)=>{throw new NotImplementedException();}}, // SET 0,D
            {0xC3, (n)=>{throw new NotImplementedException();}}, // SET 0,E
            {0xC4, (n)=>{throw new NotImplementedException();}}, // SET 0,H
            {0xC5, (n)=>{throw new NotImplementedException();}}, // SET 0,L
            {0xC6, (n)=>{throw new NotImplementedException();}}, // SET 0,(HL)
            {0xC7, (n)=>{throw new NotImplementedException();}}, // SET 0,A
            {0xC8, (n)=>{throw new NotImplementedException();}}, // SET 1,B
            {0xC9, (n)=>{throw new NotImplementedException();}}, // SET 1,C
            {0xCA, (n)=>{throw new NotImplementedException();}}, // SET 1,D
            {0xCB, (n)=>{throw new NotImplementedException();}}, // SET 1,E
            {0xCC, (n)=>{throw new NotImplementedException();}}, // SET 1,H
            {0xCD, (n)=>{throw new NotImplementedException();}}, // SET 1,L
            {0xCE, (n)=>{throw new NotImplementedException();}}, // SET 1,(HL)
            {0xCF, (n)=>{throw new NotImplementedException();}}, // SET 1,A
            {0xD0, (n)=>{throw new NotImplementedException();}}, // SET 2,B
            {0xD1, (n)=>{throw new NotImplementedException();}}, // SET 2,C
            {0xD2, (n)=>{throw new NotImplementedException();}}, // SET 2,D
            {0xD3, (n)=>{throw new NotImplementedException();}}, // SET 2,E
            {0xD4, (n)=>{throw new NotImplementedException();}}, // SET 2,H
            {0xD5, (n)=>{throw new NotImplementedException();}}, // SET 2,L
            {0xD6, (n)=>{throw new NotImplementedException();}}, // SET 2,(HL)
            {0xD7, (n)=>{throw new NotImplementedException();}}, // SET 2,A
            {0xD8, (n)=>{throw new NotImplementedException();}}, // SET 3,B
            {0xD9, (n)=>{throw new NotImplementedException();}}, // SET 3,C
            {0xDA, (n)=>{throw new NotImplementedException();}}, // SET 3,D
            {0xDB, (n)=>{throw new NotImplementedException();}}, // SET 3,E
            {0xDC, (n)=>{throw new NotImplementedException();}}, // SET 3,H
            {0xDD, (n)=>{throw new NotImplementedException();}}, // SET 3,L
            {0xDE, (n)=>{throw new NotImplementedException();}}, // SET 3,(HL)
            {0xDF, (n)=>{throw new NotImplementedException();}}, // SET 3,A
            {0xE0, (n)=>{throw new NotImplementedException();}}, // SET 4,B
            {0xE1, (n)=>{throw new NotImplementedException();}}, // SET 4,C
            {0xE2, (n)=>{throw new NotImplementedException();}}, // SET 4,D
            {0xE3, (n)=>{throw new NotImplementedException();}}, // SET 4,E
            {0xE4, (n)=>{throw new NotImplementedException();}}, // SET 4,H
            {0xE5, (n)=>{throw new NotImplementedException();}}, // SET 4,L
            {0xE6, (n)=>{throw new NotImplementedException();}}, // SET 4,(HL)
            {0xE7, (n)=>{throw new NotImplementedException();}}, // SET 4,A
            {0xE8, (n)=>{throw new NotImplementedException();}}, // SET 5,B
            {0xE9, (n)=>{throw new NotImplementedException();}}, // SET 5,C
            {0xEA, (n)=>{throw new NotImplementedException();}}, // SET 5,D
            {0xEB, (n)=>{throw new NotImplementedException();}}, // SET 5,E
            {0xEC, (n)=>{throw new NotImplementedException();}}, // SET 5,H
            {0xED, (n)=>{throw new NotImplementedException();}}, // SET 5,L
            {0xEE, (n)=>{throw new NotImplementedException();}}, // SET 5,(HL)
            {0xEF, (n)=>{throw new NotImplementedException();}}, // SET 5,A
            {0xF0, (n)=>{throw new NotImplementedException();}}, // SET 6,B
            {0xF1, (n)=>{throw new NotImplementedException();}}, // SET 6,C
            {0xF2, (n)=>{throw new NotImplementedException();}}, // SET 6,D
            {0xF3, (n)=>{throw new NotImplementedException();}}, // SET 6,E
            {0xF4, (n)=>{throw new NotImplementedException();}}, // SET 6,H
            {0xF5, (n)=>{throw new NotImplementedException();}}, // SET 6,L
            {0xF6, (n)=>{throw new NotImplementedException();}}, // SET 6,(HL)
            {0xF7, (n)=>{throw new NotImplementedException();}}, // SET 6,A
            {0xF8, (n)=>{throw new NotImplementedException();}}, // SET 7,B
            {0xF9, (n)=>{throw new NotImplementedException();}}, // SET 7,C
            {0xFA, (n)=>{throw new NotImplementedException();}}, // SET 7,D
            {0xFB, (n)=>{throw new NotImplementedException();}}, // SET 7,E
            {0xFC, (n)=>{throw new NotImplementedException();}}, // SET 7,H
            {0xFD, (n)=>{throw new NotImplementedException();}}, // SET 7,L
            {0xFE, (n)=>{throw new NotImplementedException();}}, // SET 7,(HL)
            {0xFF, (n)=>{throw new NotImplementedException();}} // SET 7,A
        };

    #endregion

    public CPU(Memory.Memory memory)
    {
      // Magic CPU initial values (after bios execution).
      this.registers.BC = 0x0013;
      this.registers.DE = 0x00D8;
      this.registers.HL = 0x014D;
      this.registers.PC = 0x0100;
      this.registers.SP = 0xFFFE;

      // Initialize the memory
      this.memory = memory;
      this.memory.Write(0xFF05, 0x00); // TIMA
      this.memory.Write(0xFF06, 0x00); // TMA
      this.memory.Write(0xFF07, 0x00); // TAC
      this.memory.Write(0xFF10, 0x80); // NR10
      this.memory.Write(0xFF11, 0xBF); // NR11
      this.memory.Write(0xFF12, 0xF3); // NR12
      this.memory.Write(0xFF14, 0xBF); // NR14
      this.memory.Write(0xFF16, 0x3F); // NR21
      this.memory.Write(0xFF17, 0x00); // NR22
      this.memory.Write(0xFF19, 0xBF); // NR24
      this.memory.Write(0xFF1A, 0x7F); // NR30
      this.memory.Write(0xFF1B, 0xFF); // NR31
      this.memory.Write(0xFF1C, 0x9F); // NR32
      this.memory.Write(0xFF1E, 0xBF); // NR33
      this.memory.Write(0xFF20, 0xFF); // NR41
      this.memory.Write(0xFF21, 0x00); // NR42
      this.memory.Write(0xFF22, 0x00); // NR43
      this.memory.Write(0xFF23, 0xBF); // NR30
      this.memory.Write(0xFF24, 0x77); // NR50
      this.memory.Write(0xFF25, 0xF3); // NR51
      this.memory.Write(0xFF26, 0xF1); // NR52 GB: 0xF1, SGB: 0xF0
      this.memory.Write(0xFF40, 0x91); // LCDC
      this.memory.Write(0xFF42, 0x00); // SCY
      this.memory.Write(0xFF43, 0x00); // SCX
      this.memory.Write(0xFF45, 0x00); // LYC
      this.memory.Write(0xFF47, 0xFC); // BGP
      this.memory.Write(0xFF48, 0xFF); // OBP0
      this.memory.Write(0xFF49, 0xFF); // OBP1
      this.memory.Write(0xFF4A, 0x00); // WY
      this.memory.Write(0xFF4B, 0x00); // WX
      this.memory.Write(0xFFFF, 0x00); // IE

    }


    CPURegisters ICPU.Registers
    {
      get
      {
        return this.registers;
      }
    }
  }
}
