using System;
using System.Collections.Generic;
using GBSharp.Utils;

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
    private Dictionary<byte, Action<ushort>> CBInstructionLambdas;

    private void CreateInstructionLambdas()
    {
      instructionLambdas = new Dictionary<byte, Action<ushort>>() {
            // NOP: No Operation
            {0x00, (n)=>{ }},

            // LD BC,nn: Load 16-bit immediate into BC
            {0x01, (n)=>{ registers.BC = n; }},

            // LD (BC),A: Save A to address pointed by BC
            {0x02, (n)=>{ memory.Write(registers.BC, registers.A); }},

            // INC BC: Increment 16-bit BC
            {0x03, (n)=>{ registers.BC++; }},

            // INC B: Increment B
            {0x04, (n)=>{ registers.B++; }},

            // DEC B: Decrement B
            {0x05, (n)=>{ registers.B--; }},

            // LD B,n: Load 8-bit immediate into B
            {0x06, (n)=>{ registers.B = (byte)n; }},

            // RLC A: Rotate A left with carry
            {0x07, (n)=>{throw new NotImplementedException();}},

            // LD (nn),SP: Save SP to given address
            {0x08, (n)=>memory.Write(n, registers.SP)},

            // ADD HL,BC: Add 16-bit BC to HL
            {0x09, (n)=>{ registers.HL += registers.BC; }},

            // LD A,(BC): Load A from address pointed to by BC
            {0x0A, (n) => registers.A = memory.Read(registers.BC)},

            // DEC BC: Decrement 16-bit BC
            {0x0B, (n) => { registers.BC--; }},

            // INC C: Increment C
            {0x0C, (n) => { registers.C++; }},

            // DEC C: Decrement C
            {0x0D, (n) => { registers.C--; }},

            // LD C,n: Load 8-bit immediate into C
            {0x0E, (n)=>{ registers.C = (byte)n;}},

            // RRC A: Rotate A right with carry
            {0x0F, (n)=>{throw new NotImplementedException();}},

            // STOP: Stop processor
            {0x10, (n)=>{throw new NotImplementedException();}},

            // LD DE,nn: Load 16-bit immediate into DE
            {0x11, (n)=>{registers.DE = n;}},

            // LD (DE),A: Save A to address pointed by DE
            {0x12, (n)=>{memory.Write(registers.DE, registers.A);}},

            // INC DE: Increment 16-bit DE
            {0x13, (n)=>{registers.DE++;}},

            // INC D: Increment D
            {0x14, (n)=>{registers.D++;}},

            // DEC D: Decrement D
            {0x15, (n)=>{registers.D--;}},

            // LD D,n: Load 8-bit immediate into D
            {0x16, (n)=>{registers.D = (byte)n;}},

            // RL A: Rotate A left
            {0x17, (n)=>{throw new NotImplementedException();}},

            // JR n: Relative jump by signed immediate
            {0x18, (n)=>{throw new NotImplementedException();}},

            // ADD HL,DE: Add 16-bit DE to HL
            {0x19, (n)=>{registers.HL += registers.DE;}},

            // LD A,(DE): Load A from address pointed to by DE
            {0x1A, (n)=>{registers.A = memory.Read(registers.DE);}},

            // DEC DE: Decrement 16-bit DE
            {0x1B, (n)=>{registers.DE--;}},

            // INC E: Increment E
            {0x1C, (n)=>{registers.E++;}},

            // DEC E: Decrement E
            {0x1D, (n)=>{registers.E--;}},

            // LD E,n: Load 8-bit immediate into E
            {0x1E, (n)=>{registers.E = (byte)n;}},

            // RR A: Rotate A right
            {0x1F, (n)=>{throw new NotImplementedException();}},

            // JR NZ,n: Relative jump by signed immediate if last result was not zero
            {0x20, (n)=>{throw new NotImplementedException();}},

            // LD HL,nn: Load 16-bit immediate into HL
            {0x21, (n)=>{registers.HL = n;}},

            // LDI (HL),A: Save A to address pointed by HL, and increment HL
            {0x22, (n)=>{memory.Write(registers.HL, registers.A);}},

            // INC HL: Increment 16-bit HL
            {0x23, (n)=>{registers.HL++;}},

            // INC H: Increment H
            {0x24, (n)=>{registers.H++;}},

            // DEC H: Decrement H
            {0x25, (n)=>{registers.H--;}},

            // LD H,n: Load 8-bit immediate into H
            {0x26, (n)=>{registers.H = (byte)n;}},

            // DAA: Adjust A for BCD addition
            {0x27, (n)=>{throw new NotImplementedException();}},

            // JR Z,n: Relative jump by signed immediate if last result was zero
            {0x28, (n)=>{throw new NotImplementedException();}},

            // ADD HL,HL: Add 16-bit HL to HL
            {0x29, (n)=>{registers.HL+=registers.HL;}},

            // LDI A,(HL): Load A from address pointed to by HL, and increment HL
            {0x2A, (n)=>{registers.A = memory.Read(registers.HL++);}},

            // DEC HL: Decrement 16-bit HL
            {0x2B, (n)=>{registers.HL--;}},

            // INC L: Increment L
            {0x2C, (n)=>{registers.L++;}},

            // DEC L: Decrement L
            {0x2D, (n)=>{registers.L--;}},

            // LD L,n: Load 8-bit immediate into L
            {0x2E, (n)=>{registers.L = (byte)n;}},

            // CPL: Complement (logical NOT) on A
            {0x2F, (n)=>{registers.A = (byte)~registers.A;}},

            // JR NC,n: Relative jump by signed immediate if last result caused no carry
            {0x30, (n)=>{throw new NotImplementedException();}},

            // LD SP,nn: Load 16-bit immediate into SP
            {0x31, (n)=>{registers.SP = n;}},

            // LDD (HL),A: Save A to address pointed by HL, and decrement HL
            {0x32, (n)=>{memory.Write(registers.HL, registers.A);}},

            // INC SP: Increment 16-bit HL
            {0x33, (n)=>{registers.SP++;}},

            // INC (HL): Increment value pointed by HL
            {0x34, (n)=>{memory.Write(registers.HL, (byte)(memory.Read(registers.HL) + 1));}},

            // DEC (HL): Decrement value pointed by HL
            {0x35, (n)=>{memory.Write(registers.HL, (byte)(memory.Read(registers.HL) - 1));}},

            // LD (HL),n: Load 8-bit immediate into address pointed by HL
            {0x36, (n) => { memory.Write(registers.HL, n);}},

            // SCF: Set carry flag
            {0x37, (n)=>{registers.F = UtilFuncs.SetBit(registers.F, (int)Flags.C);}},

            // JR C,n: Relative jump by signed immediate if last result caused carry
            {0x38, (n)=>{throw new NotImplementedException();}},

            // ADD HL,SP: Add 16-bit SP to HL
            {0x39, (n)=>{registers.HL += registers.SP;}},

            // LDD A,(HL): Load A from address pointed to by HL, and decrement HL
            {0x3A, (n) => { registers.A = memory.Read(registers.HL--);}},

            // DEC SP: Decrement 16-bit SP
            {0x3B, (n)=>{registers.SP--;}},

            // INC A: Increment A
            {0x3C, (n)=>{registers.A++;}},

            // DEC A: Decrement A
            {0x3D, (n)=>{registers.A--;}},

            // LD A,n: Load 8-bit immediate into A
            {0x3E, (n)=>{registers.A = (byte)n;}},

            // CCF: Clear carry flag
            {0x3F, (n)=>{registers.F = UtilFuncs.ClearBit(registers.F, (int)Flags.C);}},

            // LD B,B: Copy B to B
            {0x40, (n)=>{registers.B = registers.B;}}, //love this instruction

            // LD B,C: Copy C to B
            {0x41, (n)=>{registers.B = registers.C;}},

            // LD B,D: Copy D to B
            {0x42, (n)=>{registers.B = registers.D;}},

            // LD B,E: Copy E to B
            {0x43, (n)=>{registers.B = registers.E;}},

            // LD B,H: Copy H to B
            {0x44, (n)=>{registers.B = registers.H;}},

            // LD B,L: Copy L to B
            {0x45, (n)=>{registers.B = registers.L;}},

            // LD B,(HL): Copy value pointed by HL to B
            {0x46, (n)=>{registers.B = memory.Read(registers.HL);}},

            // LD B,A: Copy A to B
            {0x47, (n)=>{registers.B = registers.A;}},

            // LD C,B: Copy B to C
            {0x48, (n)=>{registers.C = registers.B;}},

            // LD C,C: Copy C to C
            {0x49, (n)=>{registers.C = registers.C;}},

            // LD C,D: Copy D to C
            {0x4A, (n)=>{registers.C = registers.D;}},

            // LD C,E: Copy E to C
            {0x4B, (n)=>{registers.C = registers.E;}},

            // LD C,H: Copy H to C
            {0x4C, (n)=>{registers.C = registers.H;}},

            // LD C,L: Copy L to C
            {0x4D, (n)=>{registers.C = registers.L;}},

            // LD C,(HL): Copy value pointed by HL to C
            {0x4E, (n)=>{registers.C = memory.Read(registers.HL);}},

            // LD C,A: Copy A to C
            {0x4F, (n)=>{registers.C = registers.A;}},

            // LD D,B: Copy B to D
            {0x50, (n)=>{registers.D = registers.B;}},

            // LD D,C: Copy C to D
            {0x51, (n)=>{registers.D = registers.C;}},

            // LD D,D: Copy D to D
            {0x52, (n)=>{registers.D = registers.D;}},

            // LD D,E: Copy E to D
            {0x53, (n)=>{registers.D = registers.E;}},

            // LD D,H: Copy H to D
            {0x54, (n)=>{registers.D = registers.H;}},

            // LD D,L: Copy L to D
            {0x55, (n)=>{registers.D = registers.L;}},

            // LD D,(HL): Copy value pointed by HL to D
            {0x56, (n)=>{registers.D = memory.Read(registers.HL);}},

            // LD D,A: Copy A to D
            {0x57, (n)=>{registers.D = registers.A;}},

            // LD E,B: Copy B to E
            {0x58, (n)=>{registers.E = registers.B;}},

            // LD E,C: Copy C to E
            {0x59, (n)=>{registers.E = registers.C;}},

            // LD E,D: Copy D to E
            {0x5A, (n)=>{registers.E = registers.D;}},

            // LD E,E: Copy E to E
            {0x5B, (n)=>{registers.E = registers.E;}},

            // LD E,H: Copy H to E
            {0x5C, (n)=>{registers.E = registers.H;}},

            // LD E,L: Copy L to E
            {0x5D, (n)=>{registers.E = registers.L;}},

            // LD E,(HL): Copy value pointed by HL to E
            {0x5E, (n)=>{registers.E = memory.Read(registers.HL);}},

            // LD E,A: Copy A to E
            {0x5F, (n)=>{registers.E = registers.A;}},

            // LD H,B: Copy B to H
            {0x60, (n)=>{registers.H = registers.B;}},

            // LD H,C: Copy C to H
            {0x61, (n)=>{registers.H = registers.C;}},

            // LD H,D: Copy D to H
            {0x62, (n)=>{registers.H = registers.D;}},

            // LD H,E: Copy E to H
            {0x63, (n)=>{registers.H = registers.E;}},

            // LD H,H: Copy H to H
            {0x64, (n)=>{registers.H = registers.H;}},

            // LD H,L: Copy L to H
            {0x65, (n)=>{registers.H = registers.L;}},

            // LD H,(HL): Copy value pointed by HL to H
            {0x66, (n)=>{registers.H = memory.Read(registers.HL);}},

            // LD H,A: Copy A to H
            {0x67, (n)=>{registers.H = registers.A;}},

            // LD L,B: Copy B to L
            {0x68, (n)=>{registers.L = registers.B;}},

            // LD L,C: Copy C to L
            {0x69, (n)=>{registers.L = registers.C;}},

            // LD L,D: Copy D to L
            {0x6A, (n)=>{registers.L = registers.D;}},

            // LD L,E: Copy E to L
            {0x6B, (n)=>{registers.L = registers.E;}},

            // LD L,H: Copy H to L
            {0x6C, (n)=>{registers.L = registers.H;}},

            // LD L,L: Copy L to L
            {0x6D, (n)=>{registers.L = registers.L;}},

            // LD L,(HL): Copy value pointed by HL to L
            {0x6E, (n)=>{registers.L = memory.Read(registers.HL);}},

            // LD L,A: Copy A to L
            {0x6F, (n)=>{registers.L = registers.A;}},

            // LD (HL),B: Copy B to address pointed by HL
            {0x70, (n)=>{memory.Write(registers.HL, registers.B);}},

            // LD (HL),C: Copy C to address pointed by HL
            {0x71, (n)=>{memory.Write(registers.HL, registers.C);}},

            // LD (HL),D: Copy D to address pointed by HL
            {0x72, (n)=>{memory.Write(registers.HL, registers.D);}},

            // LD (HL),E: Copy E to address pointed by HL
            {0x73, (n)=>{memory.Write(registers.HL, registers.E);}},

            // LD (HL),H: Copy H to address pointed by HL
            {0x74, (n)=>{memory.Write(registers.HL, registers.H);}},

            // LD (HL),L: Copy L to address pointed by HL
            {0x75, (n)=>{memory.Write(registers.HL, registers.L);}},

            // HALT: Halt processor
            {0x76, (n)=>{throw new NotImplementedException();}},

            // LD (HL),A: Copy A to address pointed by HL
            {0x77, (n)=>{memory.Write(registers.HL, registers.A);}},

            // LD A,B: Copy B to A
            {0x78, (n)=>{registers.A = registers.B;}},

            // LD A,C: Copy C to A
            {0x79, (n)=>{registers.A = registers.C;}},

            // LD A,D: Copy D to A
            {0x7A, (n)=>{registers.A = registers.D;}},

            // LD A,E: Copy E to A
            {0x7B, (n)=>{registers.A = registers.E;}},

            // LD A,H: Copy H to A
            {0x7C, (n)=>{registers.A = registers.H;}},

            // LD A,L: Copy L to A
            {0x7D, (n)=>{registers.A = registers.L;}},

            // LD A,(HL): Copy value pointed by HL to A
            {0x7E, (n)=>{registers.A = memory.Read(registers.HL);}},

            // LD A,A: Copy A to A
            {0x7F, (n)=>{registers.A = registers.A;}},

            // ADD A,B: Add B to A
            {0x80, (n)=>{registers.A += registers.B;}},

            // ADD A,C: Add C to A
            {0x81, (n)=>{registers.A += registers.C;}},

            // ADD A,D: Add D to A
            {0x82, (n)=>{registers.A += registers.D;}},

            // ADD A,E: Add E to A
            {0x83, (n)=>{registers.A += registers.E;}},

            // ADD A,H: Add H to A
            {0x84, (n)=>{registers.A += registers.H;}},

            // ADD A,L: Add L to A
            {0x85, (n)=>{registers.A += registers.L;}},

            // ADD A,(HL): Add value pointed by HL to A
            {0x86, (n)=>{registers.A += memory.Read(registers.HL);}},

            // ADD A,A: Add A to A
            {0x87, (n)=>{registers.A += registers.A;}},

            // ADC A,B: Add B and carry flag to A
            {0x88, (n)=>{throw new NotImplementedException();}},

            // ADC A,C: Add C and carry flag to A
            {0x89, (n)=>{throw new NotImplementedException();}},

            // ADC A,D: Add D and carry flag to A
            {0x8A, (n)=>{throw new NotImplementedException();}},

            // ADC A,E: Add E and carry flag to A
            {0x8B, (n)=>{throw new NotImplementedException();}},

            // ADC A,H: Add H and carry flag to A
            {0x8C, (n)=>{throw new NotImplementedException();}},

            // ADC A,L: Add and carry flag L to A
            {0x8D, (n)=>{throw new NotImplementedException();}},

            // ADC A,(HL): Add value pointed by HL and carry flag to A
            {0x8E, (n)=>{throw new NotImplementedException();}},

            // ADC A,A: Add A and carry flag to A
            {0x8F, (n)=>{throw new NotImplementedException();}},

            // SUB A,B: Subtract B from A
            {0x90, (n)=>{registers.A -= registers.B;}},

            // SUB A,C: Subtract C from A
            {0x91, (n)=>{registers.A -= registers.C;}},

            // SUB A,D: Subtract D from A
            {0x92, (n)=>{registers.A -= registers.D;}},

            // SUB A,E: Subtract E from A
            {0x93, (n)=>{registers.A -= registers.E;}},

            // SUB A,H: Subtract H from A
            {0x94, (n)=>{registers.A -= registers.H;}},

            // SUB A,L: Subtract L from A
            {0x95, (n)=>{registers.A -= registers.L;}},

            // SUB A,(HL): Subtract value pointed by HL from A
            {0x96, (n)=>{registers.A -= memory.Read(registers.HL);}},

            // SUB A,A: Subtract A from A
            {0x97, (n)=>{registers.A -= registers.A;}},

            // SBC A,B: Subtract B and carry flag from A
            {0x98, (n)=>{throw new NotImplementedException();}},

            // SBC A,C: Subtract C and carry flag from A
            {0x99, (n)=>{throw new NotImplementedException();}},

            // SBC A,D: Subtract D and carry flag from A
            {0x9A, (n)=>{throw new NotImplementedException();}},

            // SBC A,E: Subtract E and carry flag from A
            {0x9B, (n)=>{throw new NotImplementedException();}},

            // SBC A,H: Subtract H and carry flag from A
            {0x9C, (n)=>{throw new NotImplementedException();}},

            // SBC A,L: Subtract and carry flag L from A
            {0x9D, (n)=>{throw new NotImplementedException();}},

            // SBC A,(HL): Subtract value pointed by HL and carry flag from A
            {0x9E, (n)=>{throw new NotImplementedException();}},

            // SBC A,A: Subtract A and carry flag from A
            {0x9F, (n)=>{throw new NotImplementedException();}},

            // AND B: Logical AND B against A
            {0xA0, (n)=>{registers.A &= registers.B;}},

            // AND C: Logical AND C against A
            {0xA1, (n)=>{registers.A &= registers.C;}},

            // AND D: Logical AND D against A
            {0xA2, (n)=>{registers.A &= registers.D;}},

            // AND E: Logical AND E against A
            {0xA3, (n)=>{registers.A &= registers.E;}},

            // AND H: Logical AND H against A
            {0xA4, (n)=>{registers.A &= registers.H;}},

            // AND L: Logical AND L against A
            {0xA5, (n)=>{registers.A &= registers.L;}},

            // AND (HL): Logical AND value pointed by HL against A
            {0xA6, (n)=>{registers.A &= memory.Read(registers.HL);}},

            // AND A: Logical AND A against A
            {0xA7, (n)=>{registers.A &= registers.A;}},

            // XOR B: Logical XOR B against A
            {0xA8, (n)=>{registers.A ^= registers.B;}},

            // XOR C: Logical XOR C against A
            {0xA9, (n)=>{registers.A ^= registers.C;}},

            // XOR D: Logical XOR D against A
            {0xAA, (n)=>{registers.A ^= registers.D;}},

            // XOR E: Logical XOR E against A
            {0xAB, (n)=>{registers.A ^= registers.E;}},

            // XOR H: Logical XOR H against A
            {0xAC, (n)=>{registers.A ^= registers.H;}},

            // XOR L: Logical XOR L against A
            {0xAD, (n)=>{registers.A ^= registers.L;}},

            // XOR (HL): Logical XOR value pointed by HL against A
            {0xAE, (n)=>{registers.A ^= memory.Read(registers.HL);}},

            // XOR A: Logical XOR A against A
            {0xAF, (n)=>{registers.A ^= registers.A;}},

            // OR B: Logical OR B against A
            {0xB0, (n)=>{registers.A |= registers.B;}},

            // OR C: Logical OR C against A
            {0xB1, (n)=>{registers.A |= registers.C;}},

            // OR D: Logical OR D against A
            {0xB2, (n)=>{registers.A |= registers.D;}},

            // OR E: Logical OR E against A
            {0xB3, (n)=>{registers.A |= registers.E;}},

            // OR H: Logical OR H against A
            {0xB4, (n)=>{registers.A |= registers.H;}},

            // OR L: Logical OR L against A
            {0xB5, (n)=>{registers.A |= registers.L;}},

            // OR (HL): Logical OR value pointed by HL against A
            {0xB6, (n)=>{registers.A |= memory.Read(registers.HL);}},

            // OR A: Logical OR A against A
            {0xB7, (n)=>{registers.A |= registers.A;}},

            // CP B: Compare B against A
            {0xB8, (n)=>{throw new NotImplementedException();}},

            // CP C: Compare C against A
            {0xB9, (n)=>{throw new NotImplementedException();}},

            // CP D: Compare D against A
            {0xBA, (n)=>{throw new NotImplementedException();}},

            // CP E: Compare E against A
            {0xBB, (n)=>{throw new NotImplementedException();}},

            // CP H: Compare H against A
            {0xBC, (n)=>{throw new NotImplementedException();}},

            // CP L: Compare L against A
            {0xBD, (n)=>{throw new NotImplementedException();}},

            // CP (HL): Compare value pointed by HL against A
            {0xBE, (n)=>{throw new NotImplementedException();}},

            // CP A: Compare A against A
            {0xBF, (n)=>{throw new NotImplementedException();}},

            // RET NZ: Return if last result was not zero
            {0xC0, (n)=>{throw new NotImplementedException();}},

            // POP BC: Pop 16-bit value from stack into BC
            {0xC1, (n)=>{throw new NotImplementedException();}},

            // JP NZ,nn: Absolute jump to 16-bit location if last result was not zero
            {0xC2, (n)=>{throw new NotImplementedException();}},

            // JP nn: Absolute jump to 16-bit location
            {0xC3, (n)=>{throw new NotImplementedException();}},

            // CALL NZ,nn: Call routine at 16-bit location if last result was not zero
            {0xC4, (n)=>{throw new NotImplementedException();}},

            // PUSH BC: Push 16-bit BC onto stack
            {0xC5, (n)=>{throw new NotImplementedException();}},

            // ADD A,n: Add 8-bit immediate to A
            {0xC6, (n)=>{registers.A += (byte)n;}},

            // RST 0: Call routine at address 0000h
            {0xC7, (n)=>{throw new NotImplementedException();}},

            // RET Z: Return if last result was zero
            {0xC8, (n)=>{throw new NotImplementedException();}},

            // RET: Return to calling routine
            {0xC9, (n)=>{throw new NotImplementedException();}},

            // JP Z,nn: Absolute jump to 16-bit location if last result was zero
            {0xCA, (n)=>{throw new NotImplementedException();}},

            // Ext ops: Extended operations (two-byte instruction code)
            {0xCB, (n)=>{throw new NotImplementedException();}},

            // CALL Z,nn: Call routine at 16-bit location if last result was zero
            {0xCC, (n)=>{throw new NotImplementedException();}},

            // CALL nn: Call routine at 16-bit location
            {0xCD, (n)=>{throw new NotImplementedException();}},

            // ADC A,n: Add 8-bit immediate and carry to A
            {0xCE, (n)=>{throw new NotImplementedException();}},

            // RST 8: Call routine at address 0008h
            {0xCF, (n)=>{throw new NotImplementedException();}},

            // RET NC: Return if last result caused no carry
            {0xD0, (n)=>{throw new NotImplementedException();}},

            // POP DE: Pop 16-bit value from stack into DE
            {0xD1, (n)=>{throw new NotImplementedException();}},

            // JP NC,nn: Absolute jump to 16-bit location if last result caused no carry
            {0xD2, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xD3, (n)=>{throw new NotImplementedException();}},

            // CALL NC,nn: Call routine at 16-bit location if last result caused no carry
            {0xD4, (n)=>{throw new NotImplementedException();}},

            // PUSH DE: Push 16-bit DE onto stack
            {0xD5, (n)=>{throw new NotImplementedException();}},

            // SUB A,n: Subtract 8-bit immediate from A
            {0xD6, (n)=>{registers.A -= (byte)n;}},

            // RST 10: Call routine at address 0010h
            {0xD7, (n)=>{throw new NotImplementedException();}},

            // RET C: Return if last result caused carry
            {0xD8, (n)=>{throw new NotImplementedException();}},

            // RETI: Enable interrupts and return to calling routine
            {0xD9, (n)=>{throw new NotImplementedException();}},

            // JP C,nn: Absolute jump to 16-bit location if last result caused carry
            {0xDA, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xDB, (n)=>{throw new NotImplementedException();}},

            // CALL C,nn: Call routine at 16-bit location if last result caused carry
            {0xDC, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xDD, (n)=>{throw new NotImplementedException();}},

            // SBC A,n: Subtract 8-bit immediate and carry from A
            {0xDE, (n)=>{throw new NotImplementedException();}},

            // RST 18: Call routine at address 0018h
            {0xDF, (n)=>{throw new NotImplementedException();}},

            // LDH (n),A: Save A at address pointed to by (FF00h + 8-bit immediate)
            {0xE0, (n)=>{memory.Write((ushort)(0xFF00 & n), registers.A);}},

            // POP HL: Pop 16-bit value from stack into HL
            {0xE1, (n)=>{throw new NotImplementedException();}},

            // LDH (C),A: Save A at address pointed to by (FF00h + C)
            {0xE2, (n)=>{memory.Write((ushort)(0xFF00 & registers.C), registers.A);}},

            // XX: Operation removed in this CPU
            {0xE3, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xE4, (n)=>{throw new NotImplementedException();}},

            // PUSH HL: Push 16-bit HL onto stack
            {0xE5, (n)=>{throw new NotImplementedException();}},

            // AND n: Logical AND 8-bit immediate against A
            {0xE6, (n)=>{registers.A &= (byte)n;}},

            // RST 20: Call routine at address 0020h
            {0xE7, (n)=>{throw new NotImplementedException();}},

            // ADD SP,d: Add signed 8-bit immediate to SP
            {0xE8, (n)=>{throw new NotImplementedException();}},

            // JP (HL): Jump to 16-bit value pointed by HL
            {0xE9, (n)=>{throw new NotImplementedException();}},

            // LD (nn),A: Save A at given 16-bit address
            {0xEA, (n)=>{memory.Write(n, registers.A);}},

            // XX: Operation removed in this CPU
            {0xEB, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xEC, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xED, (n)=>{throw new NotImplementedException();}},

            // XOR n: Logical XOR 8-bit immediate against A
            {0xEE, (n)=>{registers.A ^= (byte)n;}},

            // RST 28: Call routine at address 0028h
            {0xEF, (n)=>{throw new NotImplementedException();}},

            // LDH A,(n): Load A from address pointed to by (FF00h + 8-bit immediate)
            {0xF0, (n)=>{registers.A = memory.Read((ushort)(0xFF00 & n));}},

            // POP AF: Pop 16-bit value from stack into AF
            {0xF1, (n)=>{throw new NotImplementedException();}},

            // LDH A, (C): Operation removed in this CPU? (Or Load into A memory from FF00 + C?)
            {0xF2, (n)=>{registers.A = memory.Read((ushort)(0xFF00 & registers.C));}},

            // DI: DIsable interrupts
            {0xF3, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xF4, (n)=>{throw new NotImplementedException();}},

            // PUSH AF: Push 16-bit AF onto stack
            {0xF5, (n)=>{throw new NotImplementedException();}},

            // OR n: Logical OR 8-bit immediate against A
            {0xF6, (n)=>{registers.A |= (byte)n;}},

            // RST 30: Call routine at address 0030h
            {0xF7, (n)=>{throw new NotImplementedException();}},

            // LDHL SP,d: Add signed 8-bit immediate to SP and save result in HL
            {0xF8, (n)=>{throw new NotImplementedException();}},

            // LD SP,HL: Copy HL to SP
            {0xF9, (n)=>{registers.SP = registers.HL;}},

            // LD A,(nn): Load A from given 16-bit address
            {0xFA, (n)=>{registers.A = memory.Read(n);}},

            // EI: Enable interrupts
            {0xFB, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xFC, (n)=>{throw new NotImplementedException();}},

            // XX: Operation removed in this CPU
            {0xFD, (n)=>{throw new NotImplementedException();}},

            // CP n: Compare 8-bit immediate against A
            {0xFE, (n)=>{throw new NotImplementedException();}},

            // RST 38: Call routine at address 0038h
            {0xFF, (n)=>{throw new NotImplementedException();}}
        };
    }

    private void CreateCBInstructionLambdas()
    {
      CBInstructionLambdas = new Dictionary<byte, Action<ushort>>()
      {
        // RLC B: Rotate B left with carry
        {0x00, (n) => { throw new NotImplementedException(); }},

        // RLC C: Rotate C left with carry
        {0x01, (n) => { throw new NotImplementedException(); }},

        // RLC D: Rotate D left with carry
        {0x02, (n) => { throw new NotImplementedException(); }},

        // RLC E: Rotate E left with carry
        {0x03, (n) => { throw new NotImplementedException(); }},

        // RLC H: Rotate H left with carry
        {0x04, (n) => { throw new NotImplementedException(); }},

        // RLC L: Rotate L left with carry
        {0x05, (n) => { throw new NotImplementedException(); }},

        // RLC (HL): Rotate value pointed by HL left with carry
        {0x06, (n) => { throw new NotImplementedException(); }},

        // RLC A: Rotate A left with carry
        {0x07, (n) => { throw new NotImplementedException(); }},

        // RRC B: Rotate B right with carry
        {0x08, (n) => { throw new NotImplementedException(); }},

        // RRC C: Rotate C right with carry
        {0x09, (n) => { throw new NotImplementedException(); }},

        // RRC D: Rotate D right with carry
        {0x0A, (n) => { throw new NotImplementedException(); }},

        // RRC E: Rotate E right with carry
        {0x0B, (n) => { throw new NotImplementedException(); }},

        // RRC H: Rotate H right with carry
        {0x0C, (n) => { throw new NotImplementedException(); }},

        // RRC L: Rotate L right with carry
        {0x0D, (n) => { throw new NotImplementedException(); }},

        // RRC (HL): Rotate value pointed by HL right with carry
        {0x0E, (n) => { throw new NotImplementedException(); }},

        // RRC A: Rotate A right with carry
        {0x0F, (n) => { throw new NotImplementedException(); }},

        // RL B: Rotate B left
        {0x10, (n) => { throw new NotImplementedException(); }},

        // RL C: Rotate C left
        {0x11, (n) => { throw new NotImplementedException(); }},

        // RL D: Rotate D left
        {0x12, (n) => { throw new NotImplementedException(); }},

        // RL E: Rotate E left
        {0x13, (n) => { throw new NotImplementedException(); }},

        // RL H: Rotate H left
        {0x14, (n) => { throw new NotImplementedException(); }},

        // RL L: Rotate L left
        {0x15, (n) => { throw new NotImplementedException(); }},

        // RL (HL): Rotate value pointed by HL left
        {0x16, (n) => { throw new NotImplementedException(); }},

        // RL A: Rotate A left
        {0x17, (n) => { throw new NotImplementedException(); }},

        // RR B: Rotate B right
        {0x18, (n) => { throw new NotImplementedException(); }},

        // RR C: Rotate C right
        {0x19, (n) => { throw new NotImplementedException(); }},

        // RR D: Rotate D right
        {0x1A, (n) => { throw new NotImplementedException(); }},

        // RR E: Rotate E right
        {0x1B, (n) => { throw new NotImplementedException(); }},

        // RR H: Rotate H right
        {0x1C, (n) => { throw new NotImplementedException(); }},

        // RR L: Rotate L right
        {0x1D, (n) => { throw new NotImplementedException(); }},

        // RR (HL): Rotate value pointed by HL right
        {0x1E, (n) => { throw new NotImplementedException(); }},

        // RR A: Rotate A right
        {0x1F, (n) => { throw new NotImplementedException(); }},

        // SLA B: Shift B left preserving sign
        {0x20, (n) => { throw new NotImplementedException(); }},

        // SLA C: Shift C left preserving sign
        {0x21, (n) => { throw new NotImplementedException(); }},

        // SLA D: Shift D left preserving sign
        {0x22, (n) => { throw new NotImplementedException(); }},

        // SLA E: Shift E left preserving sign
        {0x23, (n) => { throw new NotImplementedException(); }},

        // SLA H: Shift H left preserving sign
        {0x24, (n) => { throw new NotImplementedException(); }},

        // SLA L: Shift L left preserving sign
        {0x25, (n) => { throw new NotImplementedException(); }},

        // SLA (HL): Shift value pointed by HL left preserving sign
        {0x26, (n) => { throw new NotImplementedException(); }},

        // SLA A: Shift A left preserving sign
        {0x27, (n) => { throw new NotImplementedException(); }},

        // SRA B: Shift B right preserving sign
        {0x28, (n) => { throw new NotImplementedException(); }},

        // SRA C: Shift C right preserving sign
        {0x29, (n) => { throw new NotImplementedException(); }},

        // SRA D: Shift D right preserving sign
        {0x2A, (n) => { throw new NotImplementedException(); }},

        // SRA E: Shift E right preserving sign
        {0x2B, (n) => { throw new NotImplementedException(); }},

        // SRA H: Shift H right preserving sign
        {0x2C, (n) => { throw new NotImplementedException(); }},

        // SRA L: Shift L right preserving sign
        {0x2D, (n) => { throw new NotImplementedException(); }},

        // SRA (HL): Shift value pointed by HL right preserving sign
        {0x2E, (n) => { throw new NotImplementedException(); }},

        // SRA A: Shift A right preserving sign
        {0x2F, (n) => { throw new NotImplementedException(); }},

        // SWAP B: Swap nybbles in B
        {0x30, (n) => { throw new NotImplementedException(); }},

        // SWAP C: Swap nybbles in C
        {0x31, (n) => { throw new NotImplementedException(); }},

        // SWAP D: Swap nybbles in D
        {0x32, (n) => { throw new NotImplementedException(); }},

        // SWAP E: Swap nybbles in E
        {0x33, (n) => { throw new NotImplementedException(); }},

        // SWAP H: Swap nybbles in H
        {0x34, (n) => { throw new NotImplementedException(); }},

        // SWAP L: Swap nybbles in L
        {0x35, (n) => { throw new NotImplementedException(); }},

        // SWAP (HL): Swap nybbles in value pointed by HL
        {0x36, (n) => { throw new NotImplementedException(); }},

        // SWAP A: Swap nybbles in A
        {0x37, (n) => { throw new NotImplementedException(); }},

        // SRL B: Shift B right
        {0x38, (n) => { registers.B >>= 1; }},

        // SRL C: Shift C right
        {0x39, (n) => { registers.C >>= 1; }},

        // SRL D: Shift D right
        {0x3A, (n) => { registers.D >>= 1; }},

        // SRL E: Shift E right
        {0x3B, (n) => { registers.E >>= 1; }},

        // SRL H: Shift H right
        {0x3C, (n) => { registers.H >>= 1; }},

        // SRL L: Shift L right
        {0x3D, (n) => { registers.L >>= 1; }},

        // SRL (HL): Shift value pointed by HL right
        {0x3E, (n) => { memory.Write(registers.HL, (byte)(memory.Read(registers.HL) >> 1)); }},

        // SRL A: Shift A right
        {0x3F, (n) => { registers.A >>= 1; }},

        // BIT 0,B: Test bit 0 of B
        {0x40, (n) => { throw new NotImplementedException(); }},

        // BIT 0,C: Test bit 0 of C
        {0x41, (n) => { throw new NotImplementedException(); }},

        // BIT 0,D: Test bit 0 of D
        {0x42, (n) => { throw new NotImplementedException(); }},

        // BIT 0,E: Test bit 0 of E
        {0x43, (n) => { throw new NotImplementedException(); }},

        // BIT 0,H: Test bit 0 of H
        {0x44, (n) => { throw new NotImplementedException(); }},

        // BIT 0,L: Test bit 0 of L
        {0x45, (n) => { throw new NotImplementedException(); }},

        // BIT 0,(HL): Test bit 0 of value pointed by HL
        {0x46, (n) => { throw new NotImplementedException(); }},

        // BIT 0,A: Test bit 0 of A
        {0x47, (n) => { throw new NotImplementedException(); }},

        // BIT 1,B: Test bit 1 of B
        {0x48, (n) => { throw new NotImplementedException(); }},

        // BIT 1,C: Test bit 1 of C
        {0x49, (n) => { throw new NotImplementedException(); }},

        // BIT 1,D: Test bit 1 of D
        {0x4A, (n) => { throw new NotImplementedException(); }},

        // BIT 1,E: Test bit 1 of E
        {0x4B, (n) => { throw new NotImplementedException(); }},

        // BIT 1,H: Test bit 1 of H
        {0x4C, (n) => { throw new NotImplementedException(); }},

        // BIT 1,L: Test bit 1 of L
        {0x4D, (n) => { throw new NotImplementedException(); }},

        // BIT 1,(HL): Test bit 1 of value pointed by HL
        {0x4E, (n) => { throw new NotImplementedException(); }},

        // BIT 1,A: Test bit 1 of A
        {0x4F, (n) => { throw new NotImplementedException(); }},

        // BIT 2,B: Test bit 2 of B
        {0x50, (n) => { throw new NotImplementedException(); }},

        // BIT 2,C: Test bit 2 of C
        {0x51, (n) => { throw new NotImplementedException(); }},

        // BIT 2,D: Test bit 2 of D
        {0x52, (n) => { throw new NotImplementedException(); }},

        // BIT 2,E: Test bit 2 of E
        {0x53, (n) => { throw new NotImplementedException(); }},

        // BIT 2,H: Test bit 2 of H
        {0x54, (n) => { throw new NotImplementedException(); }},

        // BIT 2,L: Test bit 2 of L
        {0x55, (n) => { throw new NotImplementedException(); }},

        // BIT 2,(HL): Test bit 2 of value pointed by HL
        {0x56, (n) => { throw new NotImplementedException(); }},

        // BIT 2,A: Test bit 2 of A
        {0x57, (n) => { throw new NotImplementedException(); }},

        // BIT 3,B: Test bit 3 of B
        {0x58, (n) => { throw new NotImplementedException(); }},

        // BIT 3,C: Test bit 3 of C
        {0x59, (n) => { throw new NotImplementedException(); }},

        // BIT 3,D: Test bit 3 of D
        {0x5A, (n) => { throw new NotImplementedException(); }},

        // BIT 3,E: Test bit 3 of E
        {0x5B, (n) => { throw new NotImplementedException(); }},

        // BIT 3,H: Test bit 3 of H
        {0x5C, (n) => { throw new NotImplementedException(); }},

        // BIT 3,L: Test bit 3 of L
        {0x5D, (n) => { throw new NotImplementedException(); }},

        // BIT 3,(HL): Test bit 3 of value pointed by HL
        {0x5E, (n) => { throw new NotImplementedException(); }},

        // BIT 3,A: Test bit 3 of A
        {0x5F, (n) => { throw new NotImplementedException(); }},

        // BIT 4,B: Test bit 4 of B
        {0x60, (n) => { throw new NotImplementedException(); }},

        // BIT 4,C: Test bit 4 of C
        {0x61, (n) => { throw new NotImplementedException(); }},

        // BIT 4,D: Test bit 4 of D
        {0x62, (n) => { throw new NotImplementedException(); }},

        // BIT 4,E: Test bit 4 of E
        {0x63, (n) => { throw new NotImplementedException(); }},

        // BIT 4,H: Test bit 4 of H
        {0x64, (n) => { throw new NotImplementedException(); }},

        // BIT 4,L: Test bit 4 of L
        {0x65, (n) => { throw new NotImplementedException(); }},

        // BIT 4,(HL): Test bit 4 of value pointed by HL
        {0x66, (n) => { throw new NotImplementedException(); }},

        // BIT 4,A: Test bit 4 of A
        {0x67, (n) => { throw new NotImplementedException(); }},

        // BIT 5,B: Test bit 5 of B
        {0x68, (n) => { throw new NotImplementedException(); }},

        // BIT 5,C: Test bit 5 of C
        {0x69, (n) => { throw new NotImplementedException(); }},

        // BIT 5,D: Test bit 5 of D
        {0x6A, (n) => { throw new NotImplementedException(); }},

        // BIT 5,E: Test bit 5 of E
        {0x6B, (n) => { throw new NotImplementedException(); }},

        // BIT 5,H: Test bit 5 of H
        {0x6C, (n) => { throw new NotImplementedException(); }},

        // BIT 5,L: Test bit 5 of L
        {0x6D, (n) => { throw new NotImplementedException(); }},

        // BIT 5,(HL): Test bit 5 of value pointed by HL
        {0x6E, (n) => { throw new NotImplementedException(); }},

        // BIT 5,A: Test bit 5 of A
        {0x6F, (n) => { throw new NotImplementedException(); }},

        // BIT 6,B: Test bit 6 of B
        {0x70, (n) => { throw new NotImplementedException(); }},

        // BIT 6,C: Test bit 6 of C
        {0x71, (n) => { throw new NotImplementedException(); }},

        // BIT 6,D: Test bit 6 of D
        {0x72, (n) => { throw new NotImplementedException(); }},

        // BIT 6,E: Test bit 6 of E
        {0x73, (n) => { throw new NotImplementedException(); }},

        // BIT 6,H: Test bit 6 of H
        {0x74, (n) => { throw new NotImplementedException(); }},

        // BIT 6,L: Test bit 6 of L
        {0x75, (n) => { throw new NotImplementedException(); }},

        // BIT 6,(HL): Test bit 6 of value pointed by HL
        {0x76, (n) => { throw new NotImplementedException(); }},

        // BIT 6,A: Test bit 6 of A
        {0x77, (n) => { throw new NotImplementedException(); }},

        // BIT 7,B: Test bit 7 of B
        {0x78, (n) => { throw new NotImplementedException(); }},

        // BIT 7,C: Test bit 7 of C
        {0x79, (n) => { throw new NotImplementedException(); }},

        // BIT 7,D: Test bit 7 of D
        {0x7A, (n) => { throw new NotImplementedException(); }},

        // BIT 7,E: Test bit 7 of E
        {0x7B, (n) => { throw new NotImplementedException(); }},

        // BIT 7,H: Test bit 7 of H
        {0x7C, (n) => { throw new NotImplementedException(); }},

        // BIT 7,L: Test bit 7 of L
        {0x7D, (n) => { throw new NotImplementedException(); }},

        // BIT 7,(HL): Test bit 7 of value pointed by HL
        {0x7E, (n) => { throw new NotImplementedException(); }},

        // BIT 7,A: Test bit 7 of A
        {0x7F, (n) => { throw new NotImplementedException(); }},

        // RES 0,B: Clear (reset) bit 0 of B
        {0x80, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 0); }},

        // RES 0,C: Clear (reset) bit 0 of C
        {0x81, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 0); }},

        // RES 0,D: Clear (reset) bit 0 of D
        {0x82, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 0); }},

        // RES 0,E: Clear (reset) bit 0 of E
        {0x83, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 0); }},

        // RES 0,H: Clear (reset) bit 0 of H
        {0x84, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 0); }},

        // RES 0,L: Clear (reset) bit 0 of L
        {0x85, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 0); }},

        // RES 0,(HL): Clear (reset) bit 0 of value pointed by HL
        {0x86, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 0)); }},

        // RES 0,A: Clear (reset) bit 0 of A
        {0x87, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 0); }},

        // RES 1,B: Clear (reset) bit 1 of B
        {0x88, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 1); }},

        // RES 1,C: Clear (reset) bit 1 of C
        {0x89, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 1); }},

        // RES 1,D: Clear (reset) bit 1 of D
        {0x8A, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 1); }},

        // RES 1,E: Clear (reset) bit 1 of E
        {0x8B, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 1); }},

        // RES 1,H: Clear (reset) bit 1 of H
        {0x8C, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 1); }},

        // RES 1,L: Clear (reset) bit 1 of L
        {0x8D, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 1); }},

        // RES 1,(HL): Clear (reset) bit 1 of value pointed by HL
        {0x8E, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 1)); }},

        // RES 1,A: Clear (reset) bit 1 of A
        {0x8F, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 1); }},

        // RES 2,B: Clear (reset) bit 2 of B
        {0x90, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 2); }},

        // RES 2,C: Clear (reset) bit 2 of C
        {0x91, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 2); }},

        // RES 2,D: Clear (reset) bit 2 of D
        {0x92, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 2); }},

        // RES 2,E: Clear (reset) bit 2 of E
        {0x93, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 2); }},

        // RES 2,H: Clear (reset) bit 2 of H
        {0x94, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 2); }},

        // RES 2,L: Clear (reset) bit 2 of L
        {0x95, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 2); }},

        // RES 2,(HL): Clear (reset) bit 2 of value pointed by HL
        {0x96, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 2)); }},

        // RES 2,A: Clear (reset) bit 2 of A
        {0x97, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 2); }},

        // RES 3,B: Clear (reset) bit 3 of B
        {0x98, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 3); }},

        // RES 3,C: Clear (reset) bit 3 of C
        {0x99, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 3); }},

        // RES 3,D: Clear (reset) bit 3 of D
        {0x9A, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 3); }},

        // RES 3,E: Clear (reset) bit 3 of E
        {0x9B, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 3); }},

        // RES 3,H: Clear (reset) bit 3 of H
        {0x9C, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 3); }},

        // RES 3,L: Clear (reset) bit 3 of L
        {0x9D, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 3); }},

        // RES 3,(HL): Clear (reset) bit 3 of value pointed by HL
        {0x9E, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 3)); }},

        // RES 3,A: Clear (reset) bit 3 of A
        {0x9F, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 3); }},

        // RES 4,B: Clear (reset) bit 4 of B
        {0xA0, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 4); }},

        // RES 4,C: Clear (reset) bit 4 of C
        {0xA1, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 4); }},

        // RES 4,D: Clear (reset) bit 4 of D
        {0xA4, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 4); }},

        // RES 4,E: Clear (reset) bit 4 of E
        {0xA5, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 4); }},

        // RES 4,H: Clear (reset) bit 4 of H
        {0xA4, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 4); }},

        // RES 4,L: Clear (reset) bit 4 of L
        {0xA5, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 4); }},

        // RES 4,(HL): Clear (reset) bit 4 of value pointed by HL
        {0xA6, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 4)); }},

        // RES 4,A: Clear (reset) bit 4 of A
        {0xA7, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 4); }},

        // RES 5,B: Clear (reset) bit 5 of B
        {0xA8, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 5); }},

        // RES 5,C: Clear (reset) bit 5 of C
        {0xA9, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 5); }},

        // RES 5,D: Clear (reset) bit 5 of D
        {0xAA, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 5); }},

        // RES 5,E: Clear (reset) bit 5 of E
        {0xAB, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 5); }},

        // RES 5,H: Clear (reset) bit 5 of H
        {0xAC, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 5); }},

        // RES 5,L: Clear (reset) bit 5 of L
        {0xAD, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 5); }},

        // RES 5,(HL): Clear (reset) bit 5 of value pointed by HL
        {0xAE, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 5)); }},

        // RES 5,A: Clear (reset) bit 5 of A
        {0xAF, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 5); }},

        // RES 6,B: Clear (reset) bit 6 of B
        {0xB0, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 6); }},

        // RES 6,C: Clear (reset) bit 6 of C
        {0xB1, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 6); }},

        // RES 6,D: Clear (reset) bit 6 of D
        {0xB6, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 6); }},

        // RES 6,E: Clear (reset) bit 6 of E
        {0xB7, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 6); }},

        // RES 6,H: Clear (reset) bit 6 of H
        {0xB6, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 6); }},

        // RES 6,L: Clear (reset) bit 6 of L
        {0xB7, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 6); }},

        // RES 6,(HL): Clear (reset) bit 6 of value pointed by HL
        {0xB6, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 6)); }},

        // RES 6,A: Clear (reset) bit 6 of A
        {0xB7, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 6); }},

        // RES 7,B: Clear (reset) bit 7 of B
        {0xB8, (n) => { registers.B = UtilFuncs.ClearBit(registers.B, 7); }},

        // RES 7,C: Clear (reset) bit 7 of C
        {0xB9, (n) => { registers.C = UtilFuncs.ClearBit(registers.C, 7); }},

        // RES 7,D: Clear (reset) bit 7 of D
        {0xBA, (n) => { registers.D = UtilFuncs.ClearBit(registers.D, 7); }},

        // RES 7,E: Clear (reset) bit 7 of E
        {0xBB, (n) => { registers.E = UtilFuncs.ClearBit(registers.E, 7); }},

        // RES 7,H: Clear (reset) bit 7 of H
        {0xBC, (n) => { registers.H = UtilFuncs.ClearBit(registers.H, 7); }},

        // RES 7,L: Clear (reset) bit 7 of L
        {0xBD, (n) => { registers.L = UtilFuncs.ClearBit(registers.L, 7); }},

        // RES 7,(HL): Clear (reset) bit 7 of value pointed by HL
        {0xBE, (n) => { memory.Write(registers.HL,  UtilFuncs.ClearBit(memory.Read(registers.HL), 7)); }},

        // RES 7,A: Clear (reset) bit 7 of A
        {0xBF, (n) => { registers.A = UtilFuncs.ClearBit(registers.A, 7); }},

        // SET 0,B: Set bit 0 of B
        {0xC0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 0); }},

        // SET 0,C: Set bit 0 of C
        {0xC1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 0); }},

        // SET 0,D: Set bit 0 of D
        {0xC2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 0); }},

        // SET 0,E: Set bit 0 of E
        {0xC3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 0); }},

        // SET 0,H: Set bit 0 of H
        {0xC4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 0); }},

        // SET 0,L: Set bit 0 of L
        {0xC5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 0); }},

        // SET 0,(HL): Set bit 0 of value pointed by HL
        {0xC6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 0)); }},

        // SET 0,A: Set bit 0 of A
        {0xC7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 0); }},

        // SET 1,B: Set bit 1 of B
        {0xC8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 1); }},

        // SET 1,C: Set bit 1 of C
        {0xC9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 1); }},

        // SET 1,D: Set bit 1 of D
        {0xCA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 1); }},

        // SET 1,E: Set bit 1 of E
        {0xCB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 1); }},

        // SET 1,H: Set bit 1 of H
        {0xCC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 1); }},

        // SET 1,L: Set bit 1 of L
        {0xCD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 1); }},

        // SET 1,(HL): Set bit 1 of value pointed by HL
        {0xCE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 1)); }},

        // SET 1,A: Set bit 1 of A
        {0xCF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 1); }},

        // SET 2,B: Set bit 2 of B
        {0xD0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 2); }},

        // SET 2,C: Set bit 2 of C
        {0xD1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 2); }},

        // SET 2,D: Set bit 2 of D
        {0xD2, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 2); }},

        // SET 2,E: Set bit 2 of E
        {0xD3, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 2); }},

        // SET 2,H: Set bit 2 of H
        {0xD4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 2); }},

        // SET 2,L: Set bit 2 of L
        {0xD5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 2); }},

        // SET 2,(HL): Set bit 2 of value pointed by HL
        {0xD6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 2)); }},

        // SET 2,A: Set bit 2 of A
        {0xD7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 2); }},

        // SET 3,B: Set bit 3 of B
        {0xD8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 3); }},

        // SET 3,C: Set bit 3 of C
        {0xD9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 3); }},

        // SET 3,D: Set bit 3 of D
        {0xDA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 3); }},

        // SET 3,E: Set bit 3 of E
        {0xDB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 3); }},

        // SET 3,H: Set bit 3 of H
        {0xDC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 3); }},

        // SET 3,L: Set bit 3 of L
        {0xDD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 3); }},

        // SET 3,(HL): Set bit 3 of value pointed by HL
        {0xDE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 3)); }},

        // SET 3,A: Set bit 3 of A
        {0xDF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 3); }},

        // SET 4,B: Set bit 4 of B
        {0xE0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 4); }},

        // SET 4,C: Set bit 4 of C
        {0xE1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 4); }},

        // SET 4,D: Set bit 4 of D
        {0xE4, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 4); }},

        // SET 4,E: Set bit 4 of E
        {0xE5, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 4); }},

        // SET 4,H: Set bit 4 of H
        {0xE4, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 4); }},

        // SET 4,L: Set bit 4 of L
        {0xE5, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 4); }},

        // SET 4,(HL): Set bit 4 of value pointed by HL
        {0xE6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 4)); }},

        // SET 4,A: Set bit 4 of A
        {0xE7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 4); }},

        // SET 5,B: Set bit 5 of B
        {0xE8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 5); }},

        // SET 5,C: Set bit 5 of C
        {0xE9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 5); }},

        // SET 5,D: Set bit 5 of D
        {0xEA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 5); }},

        // SET 5,E: Set bit 5 of E
        {0xEB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 5); }},

        // SET 5,H: Set bit 5 of H
        {0xEC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 5); }},

        // SET 5,L: Set bit 5 of L
        {0xED, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 5); }},

        // SET 5,(HL): Set bit 5 of value pointed by HL
        {0xEE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 5)); }},

        // SET 5,A: Set bit 5 of A
        {0xEF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 5); }},

        // SET 6,B: Set bit 6 of B
        {0xF0, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 6); }},

        // SET 6,C: Set bit 6 of C
        {0xF1, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 6); }},

        // SET 6,D: Set bit 6 of D
        {0xF6, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 6); }},

        // SET 6,E: Set bit 6 of E
        {0xF7, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 6); }},

        // SET 6,H: Set bit 6 of H
        {0xF6, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 6); }},

        // SET 6,L: Set bit 6 of L
        {0xF7, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 6); }},

        // SET 6,(HL): Set bit 6 of value pointed by HL
        {0xF6, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 6)); }},

        // SET 6,A: Set bit 6 of A
        {0xF7, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 6); }},

        // SET 7,B: Set bit 7 of B
        {0xF8, (n) => { registers.B = UtilFuncs.SetBit(registers.B, 7); }},

        // SET 7,C: Set bit 7 of C
        {0xF9, (n) => { registers.C = UtilFuncs.SetBit(registers.C, 7); }},

        // SET 7,D: Set bit 7 of D
        {0xFA, (n) => { registers.D = UtilFuncs.SetBit(registers.D, 7); }},

        // SET 7,E: Set bit 7 of E
        {0xFB, (n) => { registers.E = UtilFuncs.SetBit(registers.E, 7); }},

        // SET 7,H: Set bit 7 of H
        {0xFC, (n) => { registers.H = UtilFuncs.SetBit(registers.H, 7); }},

        // SET 7,L: Set bit 7 of L
        {0xFD, (n) => { registers.L = UtilFuncs.SetBit(registers.L, 7); }},

        // SET 7,(HL): Set bit 7 of value pointed by HL
        {0xFE, (n) => { memory.Write(registers.HL,  UtilFuncs.SetBit(memory.Read(registers.HL), 7)); }},

        // SET 7,A: Set bit 7 of A
        {0xFF, (n) => { registers.A = UtilFuncs.SetBit(registers.A, 7); }},
      };
    }

    #endregion

    public CPU(Memory.Memory memory)
    {
      //Create Instruction Lambdas
      CreateInstructionLambdas();
      CreateCBInstructionLambdas();

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
