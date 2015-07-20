using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.CPUSpace.Dictionaries
{
  class CPUOpcodeNames
  {
    internal static Dictionary<byte, string> Setup()
    {
      return new Dictionary<byte, string>{
            {0x00, "NOP"}, // NOP
            {0x01, "LD BC,nn"}, // LD BC,nn
            {0x02, "LD (BC),A"}, // LD (BC),A
            {0x03, "INC BC"}, // INC BC
            {0x04, "INC B"}, // INC B
            {0x05, "DEC B"}, // DEC B
            {0x06, "LD B,n"}, // LD B,n
            {0x07, "RLC A"}, // RLC A
            {0x08, "LD (nn),SP"}, // LD (nn),SP
            {0x09, "ADD HL,BC"}, // ADD HL,BC
            {0x0A, "LD A,(BC)"}, // LD A,(BC)
            {0x0B, "DEC BC"}, // DEC BC
            {0x0C, "INC C"}, // INC C
            {0x0D, "DEC C"}, // DEC C
            {0x0E, "LD C,n"}, // LD C,n
            {0x0F, "RRC A"}, // RRC A
            {0x10, "STOP"}, // STOP
            {0x11, "LD DE,nn"}, // LD DE,nn
            {0x12, "LD (DE),A"}, // LD (DE),A
            {0x13, "INC DE"}, // INC DE
            {0x14, "INC D"}, // INC D
            {0x15, "DEC D"}, // DEC D
            {0x16, "LD D,n"}, // LD D,n
            {0x17, "RL A"}, // RL A
            {0x18, "JR n"}, // JR n
            {0x19, "ADD HL,DE"}, // ADD HL,DE
            {0x1A, "LD A,(DE)"}, // LD A,(DE)
            {0x1B, "DEC DE"}, // DEC DE
            {0x1C, "INC E"}, // INC E
            {0x1D, "DEC E"}, // DEC E
            {0x1E, "LD E,n"}, // LD E,n
            {0x1F, "RR A"}, // RR A
            {0x20, "JR NZ,n"}, // JR NZ,n
            {0x21, "LD HL,nn"}, // LD HL,nn
            {0x22, "LDI (HL),A"}, // LDI (HL),A
            {0x23, "INC HL"}, // INC HL
            {0x24, "INC H"}, // INC H
            {0x25, "DEC H"}, // DEC H
            {0x26, "LD H,n"}, // LD H,n
            {0x27, "DAA"}, // DAA
            {0x28, "JR Z,n"}, // JR Z,n
            {0x29, "ADD HL,HL"}, // ADD HL,HL
            {0x2A, "LDI A,(HL)"}, // LDI A,(HL)
            {0x2B, "DEC HL"}, // DEC HL
            {0x2C, "INC L"}, // INC L
            {0x2D, "DEC L"}, // DEC L
            {0x2E, "LD L,n"}, // LD L,n
            {0x2F, "CPL"}, // CPL
            {0x30, "JR NC,n"}, // JR NC,n
            {0x31, "LD SP,nn"}, // LD SP,nn
            {0x32, "LDD (HL),A"}, // LDD (HL),A
            {0x33, "INC SP"}, // INC SP
            {0x34, "INC (HL)"}, // INC (HL)
            {0x35, "DEC (HL)"}, // DEC (HL)
            {0x36, "LD (HL),n"}, // LD (HL),n
            {0x37, "SCF"}, // SCF
            {0x38, "JR C,n"}, // JR C,n
            {0x39, "ADD HL,SP"}, // ADD HL,SP
            {0x3A, "LDD A,(HL)"}, // LDD A,(HL)
            {0x3B, "DEC SP"}, // DEC SP
            {0x3C, "INC A"}, // INC A
            {0x3D, "DEC A"}, // DEC A
            {0x3E, "LD A,n"}, // LD A,n
            {0x3F, "CCF"}, // CCF
            {0x40, "LD B,B"}, // LD B,B
            {0x41, "LD B,C"}, // LD B,C
            {0x42, "LD B,D"}, // LD B,D
            {0x43, "LD B,E"}, // LD B,E
            {0x44, "LD B,H"}, // LD B,H
            {0x45, "LD B,L"}, // LD B,L
            {0x46, "LD B,(HL)"}, // LD B,(HL)
            {0x47, "LD B,A"}, // LD B,A
            {0x48, "LD C,B"}, // LD C,B
            {0x49, "LD C,C"}, // LD C,C
            {0x4A, "LD C,D"}, // LD C,D
            {0x4B, "LD C,E"}, // LD C,E
            {0x4C, "LD C,H"}, // LD C,H
            {0x4D, "LD C,L"}, // LD C,L
            {0x4E, "LD C,(HL)"}, // LD C,(HL)
            {0x4F, "LD C,A"}, // LD C,A
            {0x50, "LD D,B"}, // LD D,B
            {0x51, "LD D,C"}, // LD D,C
            {0x52, "LD D,D"}, // LD D,D
            {0x53, "LD D,E"}, // LD D,E
            {0x54, "LD D,H"}, // LD D,H
            {0x55, "LD D,L"}, // LD D,L
            {0x56, "LD D,(HL)"}, // LD D,(HL)
            {0x57, "LD D,A"}, // LD D,A
            {0x58, "LD E,B"}, // LD E,B
            {0x59, "LD E,C"}, // LD E,C
            {0x5A, "LD E,D"}, // LD E,D
            {0x5B, "LD E,E"}, // LD E,E
            {0x5C, "LD E,H"}, // LD E,H
            {0x5D, "LD E,L"}, // LD E,L
            {0x5E, "LD E,(HL)"}, // LD E,(HL)
            {0x5F, "LD E,A"}, // LD E,A
            {0x60, "LD H,B"}, // LD H,B
            {0x61, "LD H,C"}, // LD H,C
            {0x62, "LD H,D"}, // LD H,D
            {0x63, "LD H,E"}, // LD H,E
            {0x64, "LD H,H"}, // LD H,H
            {0x65, "LD H,L"}, // LD H,L
            {0x66, "LD H,(HL)"}, // LD H,(HL)
            {0x67, "LD H,A"}, // LD H,A
            {0x68, "LD L,B"}, // LD L,B
            {0x69, "LD L,C"}, // LD L,C
            {0x6A, "LD L,D"}, // LD L,D
            {0x6B, "LD L,E"}, // LD L,E
            {0x6C, "LD L,H"}, // LD L,H
            {0x6D, "LD L,L"}, // LD L,L
            {0x6E, "LD L,(HL)"}, // LD L,(HL)
            {0x6F, "LD L,A"}, // LD L,A
            {0x70, "LD (HL),B"}, // LD (HL),B
            {0x71, "LD (HL),C"}, // LD (HL),C
            {0x72, "LD (HL),D"}, // LD (HL),D
            {0x73, "LD (HL),E"}, // LD (HL),E
            {0x74, "LD (HL),H"}, // LD (HL),H
            {0x75, "LD (HL),L"}, // LD (HL),L
            {0x76, "HALT"}, // HALT
            {0x77, "LD (HL),A"}, // LD (HL),A
            {0x78, "LD A,B"}, // LD A,B
            {0x79, "LD A,C"}, // LD A,C
            {0x7A, "LD A,D"}, // LD A,D
            {0x7B, "LD A,E"}, // LD A,E
            {0x7C, "LD A,H"}, // LD A,H
            {0x7D, "LD A,L"}, // LD A,L
            {0x7E, "LD A,(HL)"}, // LD A,(HL)
            {0x7F, "LD A,A"}, // LD A,A
            {0x80, "ADD A,B"}, // ADD A,B
            {0x81, "ADD A,C"}, // ADD A,C
            {0x82, "ADD A,D"}, // ADD A,D
            {0x83, "ADD A,E"}, // ADD A,E
            {0x84, "ADD A,H"}, // ADD A,H
            {0x85, "ADD A,L"}, // ADD A,L
            {0x86, "ADD A,(HL)"}, // ADD A,(HL)
            {0x87, "ADD A,A"}, // ADD A,A
            {0x88, "ADC A,B"}, // ADC A,B
            {0x89, "ADC A,C"}, // ADC A,C
            {0x8A, "ADC A,D"}, // ADC A,D
            {0x8B, "ADC A,E"}, // ADC A,E
            {0x8C, "ADC A,H"}, // ADC A,H
            {0x8D, "ADC A,L"}, // ADC A,L
            {0x8E, "ADC A,(HL)"}, // ADC A,(HL)
            {0x8F, "ADC A,A"}, // ADC A,A
            {0x90, "SUB A,B"}, // SUB A,B
            {0x91, "SUB A,C"}, // SUB A,C
            {0x92, "SUB A,D"}, // SUB A,D
            {0x93, "SUB A,E"}, // SUB A,E
            {0x94, "SUB A,H"}, // SUB A,H
            {0x95, "SUB A,L"}, // SUB A,L
            {0x96, "SUB A,(HL)"}, // SUB A,(HL)
            {0x97, "SUB A,A"}, // SUB A,A
            {0x98, "SBC A,B"}, // SBC A,B
            {0x99, "SBC A,C"}, // SBC A,C
            {0x9A, "SBC A,D"}, // SBC A,D
            {0x9B, "SBC A,E"}, // SBC A,E
            {0x9C, "SBC A,H"}, // SBC A,H
            {0x9D, "SBC A,L"}, // SBC A,L
            {0x9E, "SBC A,(HL)"}, // SBC A,(HL)
            {0x9F, "SBC A,A"}, // SBC A,A
            {0xA0, "AND B"}, // AND B
            {0xA1, "AND C"}, // AND C
            {0xA2, "AND D"}, // AND D
            {0xA3, "AND E"}, // AND E
            {0xA4, "AND H"}, // AND H
            {0xA5, "AND L"}, // AND L
            {0xA6, "AND (HL)"}, // AND (HL)
            {0xA7, "AND A"}, // AND A
            {0xA8, "XOR B"}, // XOR B
            {0xA9, "XOR C"}, // XOR C
            {0xAA, "XOR D"}, // XOR D
            {0xAB, "XOR E"}, // XOR E
            {0xAC, "XOR H"}, // XOR H
            {0xAD, "XOR L"}, // XOR L
            {0xAE, "XOR (HL)"}, // XOR (HL)
            {0xAF, "XOR A"}, // XOR A
            {0xB0, "OR B"}, // OR B
            {0xB1, "OR C"}, // OR C
            {0xB2, "OR D"}, // OR D
            {0xB3, "OR E"}, // OR E
            {0xB4, "OR H"}, // OR H
            {0xB5, "OR L"}, // OR L
            {0xB6, "OR (HL)"}, // OR (HL)
            {0xB7, "OR A"}, // OR A
            {0xB8, "CP B"}, // CP B
            {0xB9, "CP C"}, // CP C
            {0xBA, "CP D"}, // CP D
            {0xBB, "CP E"}, // CP E
            {0xBC, "CP H"}, // CP H
            {0xBD, "CP L"}, // CP L
            {0xBE, "CP (HL)"}, // CP (HL)
            {0xBF, "CP A"}, // CP A
            {0xC0, "RET NZ"}, // RET NZ
            {0xC1, "POP BC"}, // POP BC
            {0xC2, "JP NZ,nn"}, // JP NZ,nn
            {0xC3, "JP nn"}, // JP nn
            {0xC4, "CALL NZ,nn"}, // CALL NZ,nn
            {0xC5, "PUSH BC"}, // PUSH BC
            {0xC6, "ADD A,n"}, // ADD A,n
            {0xC7, "RST 0"}, // RST 0
            {0xC8, "RET Z"}, // RET Z
            {0xC9, "RET"}, // RET
            {0xCA, "JP Z,nn"}, // JP Z,nn
            {0xCB, "Ext ops"}, // Ext ops
            {0xCC, "CALL Z,nn"}, // CALL Z,nn
            {0xCD, "CALL nn"}, // CALL nn
            {0xCE, "ADC A,n"}, // ADC A,n
            {0xCF, "RST 8"}, // RST 8
            {0xD0, "RET NC"}, // RET NC
            {0xD1, "POP DE"}, // POP DE
            {0xD2, "JP NC,nn"}, // JP NC,nn
            // {0xD3, }, // XX
            {0xD4, "CALL NC,nn"}, // CALL NC,nn
            {0xD5, "PUSH DE"}, // PUSH DE
            {0xD6, "SUB A,n"}, // SUB A,n
            {0xD7, "RST 10"}, // RST 10
            {0xD8, "RET C"}, // RET C
            {0xD9, "RETI"}, // RETI
            {0xDA, "JP C,nn"}, // JP C,nn
            // {0xDB, }, // XX
            {0xDC, "CALL C,nn"}, // CALL C,nn
            // {0xDD, }, // XX
            {0xDE, "SBC A,n"}, // SBC A,n
            {0xDF, "RST 18"}, // RST 18
            {0xE0, "LDH (n),A"}, // LDH (n),A
            {0xE1, "POP HL"}, // POP HL
            {0xE2, "LDH (C),A"}, // LDH (C),A
            // {0xE3, }, // XX
            // {0xE4, }, // XX
            {0xE5, "PUSH HL"}, // PUSH HL
            {0xE6, "AND n"}, // AND n
            {0xE7, "RST 20"}, // RST 20
            {0xE8, "ADD SP,d"}, // ADD SP,d
            {0xE9, "JP (HL)"}, // JP (HL)
            {0xEA, "LD (nn),A"}, // LD (nn),A
            // {0xEB, }, // XX
            // {0xEC, }, // XX
            // {0xED, }, // XX
            {0xEE, "XOR n"}, // XOR n
            {0xEF, "RST 28"}, // RST 28
            {0xF0, "LDH A,(n)"}, // LDH A,(n)
            {0xF1, "POP AF"}, // POP AF
            {0xF2, "LDH A, (C)"}, // LDH A, (C)
            {0xF3, "DI"}, // DI
            // {0xF4, }, // XX
            {0xF5, "PUSH AF"}, // PUSH AF
            {0xF6, "OR n"}, // OR n
            {0xF7, "RST 30"}, // RST 30
            {0xF8, "LDHL SP,d"}, // LDHL SP,d
            {0xF9, "LD SP,HL"}, // LD SP,HL
            {0xFA, "LD A,(nn)"}, // LD A,(nn)
            {0xFB, "EI"}, // EI
            // {0xFC, }, // XX
            // {0xFD, }, // XX
            {0xFE, "CP n"}, // CP n
            {0xFF, "RST 38"} // RST 38
        };
    }
  }
}
