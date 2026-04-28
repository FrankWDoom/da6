using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace da6
{
    partial class da6Umbrella
    {
        // https://forums.nesdev.org/viewtopic.php?t=7466

        /**
        *    DISASM6 - A NES-oriented 6502 disassembler which produces asm6 code
        *    Created by Frantik 2011-2015
        *
        */

        const string VERSION = "1.5.1";
        const string LEFT_MARGIN = "            ";


        #region opcodes and nes registers
        static Op[] opcodes = new Op[]
        {
            // byte => legal, text, bytes, cycles, addressing mode
            new Op(0x00 , 0, "BRK", 1, 0, 0), // port: code, legal (really 1=illegal), op name, byte len, cycles, adr mode // todo consts for adr modes
            new Op(0x01 , 0, "ORA", 2, 6, 7),
            new Op(0x02 , 1, "KIL", 1, 0, 0),
            new Op(0x03 , 1, "SLO", 2, 8, 7),
            new Op(0x04 , 1, "NOP", 2, 3, 4),
            new Op(0x05 , 0, "ORA", 2, 3, 4),
            new Op(0x06 , 0, "ASL", 2, 5, 4),
            new Op(0x07 , 1, "SLO", 2, 5, 4),
            new Op(0x08 , 0, "PHP", 1, 3, 0),
            new Op(0x09 , 0, "ORA", 2, 2, 0),
            new Op(0x0A , 0, "ASL", 1, 2, 0),
            new Op(0x0B , 1, "ANC", 2, 2, 0),
            new Op(0x0C , 1, "NOP", 3, 4, 1),
            new Op(0x0D , 0, "ORA", 3, 4, 1),
            new Op(0x0E , 0, "ASL", 3, 6, 1),
            new Op(0x0F , 1, "SLO", 3, 6, 1),
            new Op(0x10 , 0, "BPL", 2, 3, 0),
            new Op(0x11 , 0, "ORA", 2, 5, 8),
            new Op(0x12 , 1, "KIL", 1, 0, 0),
            new Op(0x13 , 1, "SLO", 2, 8, 8),
            new Op(0x14 , 1, "NOP", 2, 4, 5),
            new Op(0x15 , 0, "ORA", 2, 4, 5),
            new Op(0x16 , 0, "ASL", 2, 6, 5),
            new Op(0x17 , 1, "SLO", 2, 6, 5),
            new Op(0x18 , 0, "CLC", 1, 2, 0),
            new Op(0x19 , 0, "ORA", 3, 4, 3),
            new Op(0x1A , 1, "NOP", 1, 2, 0),
            new Op(0x1B , 1, "SLO", 3, 7, 3),
            new Op(0x1C , 1, "NOP", 3, 4, 2),
            new Op(0x1D , 0, "ORA", 3, 4, 2),
            new Op(0x1E , 0, "ASL", 3, 7, 2),
            new Op(0x1F , 1, "SLO", 3, 7, 2),
            new Op(0x20 , 0, "JSR", 3, 6, 10),
            new Op(0x21 , 0, "AND", 2, 6, 7),
            new Op(0x22 , 1, "KIL", 1, 0, 0),
            new Op(0x23 , 1, "RLA", 2, 8, 7),
            new Op(0x24 , 0, "BIT", 2, 3, 4),
            new Op(0x25 , 0, "AND", 2, 3, 4),
            new Op(0x26 , 0, "ROL", 2, 5, 4),
            new Op(0x27 , 1, "RLA", 2, 5, 4),
            new Op(0x28 , 0, "PLP", 1, 4, 0),
            new Op(0x29 , 0, "AND", 2, 2, 0),
            new Op(0x2A , 0, "ROL", 1, 2, 0),
            new Op(0x2B , 1, "ANC", 2, 2, 0),
            new Op(0x2C , 0, "BIT", 3, 4, 1),
            new Op(0x2D , 0, "AND", 3, 4, 1),
            new Op(0x2E , 0, "ROL", 3, 6, 1),
            new Op(0x2F , 1, "RLA", 3, 6, 1),
            new Op(0x30 , 0, "BMI", 2, 2, 0),
            new Op(0x31 , 0, "AND", 2, 5, 8),
            new Op(0x32 , 1, "KIL", 1, 0, 0),
            new Op(0x33 , 1, "RLA", 2, 8, 8),
            new Op(0x34 , 1, "NOP", 2, 4, 5),
            new Op(0x35 , 0, "AND", 2, 4, 5),
            new Op(0x36 , 0, "ROL", 2, 6, 5),
            new Op(0x37 , 1, "RLA", 2, 6, 5),
            new Op(0x38 , 0, "SEC", 1, 2, 0),
            new Op(0x39 , 0, "AND", 3, 4, 3),
            new Op(0x3A , 1, "NOP", 1, 2, 0),
            new Op(0x3B , 1, "RLA", 3, 7, 3),
            new Op(0x3C , 1, "NOP", 3, 4, 2),
            new Op(0x3D , 0, "AND", 3, 4, 2),
            new Op(0x3E , 0, "ROL", 3, 7, 2),
            new Op(0x3F , 1, "RLA", 3, 7, 2),
            new Op(0x40 , 0, "RTI", 1, 6, 0),
            new Op(0x41 , 0, "EOR", 2, 6, 7),
            new Op(0x42 , 1, "KIL", 1, 0, 0),
            new Op(0x43 , 1, "SRE", 2, 8, 7),
            new Op(0x44 , 1, "NOP", 2, 3, 4),
            new Op(0x45 , 0, "EOR", 2, 3, 4),
            new Op(0x46 , 0, "LSR", 2, 5, 4),
            new Op(0x47 , 1, "SRE", 2, 5, 4),
            new Op(0x48 , 0, "PHA", 1, 3, 0),
            new Op(0x49 , 0, "EOR", 2, 2, 0),
            new Op(0x4A , 0, "LSR", 1, 2, 0),
            new Op(0x4B , 1, "ALR", 2, 2, 0),
            new Op(0x4C , 0, "JMP", 3, 3, 10),
            new Op(0x4D , 0, "EOR", 3, 4, 1),
            new Op(0x4E , 0, "LSR", 3, 6, 1),
            new Op(0x4F , 1, "SRE", 3, 6, 1),
            new Op(0x50 , 0, "BVC", 2, 3, 0),
            new Op(0x51 , 0, "EOR", 2, 5, 8),
            new Op(0x52 , 1, "KIL", 1, 0, 0),
            new Op(0x53 , 1, "SRE", 2, 8, 8),
            new Op(0x54 , 1, "NOP", 2, 4, 5),
            new Op(0x55 , 0, "EOR", 2, 4, 5),
            new Op(0x56 , 0, "LSR", 2, 6, 5),
            new Op(0x57 , 1, "SRE", 2, 6, 5),
            new Op(0x58 , 0, "CLI", 1, 2, 0),
            new Op(0x59 , 0, "EOR", 3, 4, 3),
            new Op(0x5A , 1, "NOP", 1, 2, 0),
            new Op(0x5B , 1, "SRE", 3, 7, 3),
            new Op(0x5C , 1, "NOP", 3, 4, 2),
            new Op(0x5D , 0, "EOR", 3, 4, 2),
            new Op(0x5E , 0, "LSR", 3, 7, 2),
            new Op(0x5F , 1, "SRE", 3, 7, 2),
            new Op(0x60 , 0, "RTS", 1, 6, 0),
            new Op(0x61 , 0, "ADC", 2, 6, 7),
            new Op(0x62 , 1, "KIL", 1, 0, 0),
            new Op(0x63 , 1, "RRA", 2, 8, 7),
            new Op(0x64 , 1, "NOP", 2, 3, 4),
            new Op(0x65 , 0, "ADC", 2, 3, 4),
            new Op(0x66 , 0, "ROR", 2, 5, 4),
            new Op(0x67 , 1, "RRA", 2, 5, 4),
            new Op(0x68 , 0, "PLA", 1, 4, 0),
            new Op(0x69 , 0, "ADC", 2, 2, 0),
            new Op(0x6A , 0, "ROR", 1, 2, 0),
            new Op(0x6B , 1, "ARR", 2, 2, 0),
            new Op(0x6C , 0, "JMP", 3, 5, 9),
            new Op(0x6D , 0, "ADC", 3, 4, 1),
            new Op(0x6E , 0, "ROR", 3, 6, 1),
            new Op(0x6F , 1, "RRA", 3, 6, 1),
            new Op(0x70 , 0, "BVS", 2, 2, 0),
            new Op(0x71 , 0, "ADC", 2, 5, 8),
            new Op(0x72 , 1, "KIL", 1, 0, 0),
            new Op(0x73 , 1, "RRA", 2, 8, 8),
            new Op(0x74 , 1, "NOP", 2, 4, 5),
            new Op(0x75 , 0, "ADC", 2, 4, 5),
            new Op(0x76 , 0, "ROR", 2, 6, 5),
            new Op(0x77 , 1, "RRA", 2, 6, 5),
            new Op(0x78 , 0, "SEI", 1, 2, 0),
            new Op(0x79 , 0, "ADC", 3, 4, 3),
            new Op(0x7A , 1, "NOP", 1, 2, 0),
            new Op(0x7B , 1, "RRA", 3, 7, 3),
            new Op(0x7C , 1, "NOP", 3, 4, 2),
            new Op(0x7D , 0, "ADC", 3, 4, 2),
            new Op(0x7E , 0, "ROR", 3, 7, 2),
            new Op(0x7F , 1, "RRA", 3, 7, 2),
            new Op(0x80 , 1, "NOP", 2, 2, 0),
            new Op(0x81 , 0, "STA", 2, 6, 7),
            new Op(0x82 , 1, "NOP", 2, 2, 0),
            new Op(0x83 , 1, "SAX", 2, 6, 7),
            new Op(0x84 , 0, "STY", 2, 3, 4),
            new Op(0x85 , 0, "STA", 2, 3, 4),
            new Op(0x86 , 0, "STX", 2, 3, 4),
            new Op(0x87 , 1, "SAX", 2, 3, 4),
            new Op(0x88 , 0, "DEY", 1, 2, 0),
            new Op(0x89 , 1, "NOP", 2, 2, 0),
            new Op(0x8A , 0, "TXA", 1, 2, 0),
            new Op(0x8B , 1, "XAA", 2, 2, 0),
            new Op(0x8C , 0, "STY", 3, 4, 1),
            new Op(0x8D , 0, "STA", 3, 4, 1),
            new Op(0x8E , 0, "STX", 3, 4, 1),
            new Op(0x8F , 1, "SAX", 3, 4, 1),
            new Op(0x90 , 0, "BCC", 2, 3, 0),
            new Op(0x91 , 0, "STA", 2, 6, 8),
            new Op(0x92 , 1, "KIL", 1, 0, 0),
            new Op(0x93 , 1, "AHX", 2, 6, 8),
            new Op(0x94 , 0, "STY", 2, 4, 5),
            new Op(0x95 , 0, "STA", 2, 4, 5),
            new Op(0x96 , 0, "STX", 2, 4, 6),
            new Op(0x97 , 1, "SAX", 2, 4, 6),
            new Op(0x98 , 0, "TYA", 1, 2, 0),
            new Op(0x99 , 0, "STA", 3, 5, 3),
            new Op(0x9A , 0, "TXS", 1, 2, 0),
            new Op(0x9B , 1, "TAS", 1, 5, 0),
            new Op(0x9C , 1, "SHY", 3, 5, 2),
            new Op(0x9D , 0, "STA", 3, 5, 2),
            new Op(0x9E , 1, "SHX", 3, 5, 3),
            new Op(0x9F , 1, "AHX", 3, 5, 3),
            new Op(0xA0 , 0, "LDY", 2, 2, 0),
            new Op(0xA1 , 0, "LDA", 2, 6, 7),
            new Op(0xA2 , 0, "LDX", 2, 2, 0),
            new Op(0xA3 , 1, "LAX", 2, 6, 7),
            new Op(0xA4 , 0, "LDY", 2, 3, 4),
            new Op(0xA5 , 0, "LDA", 2, 3, 4),
            new Op(0xA6 , 0, "LDX", 2, 3, 4),
            new Op(0xA7 , 1, "LAX", 2, 3, 4),
            new Op(0xA8 , 0, "TAY", 1, 2, 0),
            new Op(0xA9 , 0, "LDA", 2, 2, 0),
            new Op(0xAA , 0, "TAX", 1, 2, 0),
            new Op(0xAB , 1, "LAX", 2, 2, 0),
            new Op(0xAC , 0, "LDY", 3, 4, 1),
            new Op(0xAD , 0, "LDA", 3, 4, 1),
            new Op(0xAE , 0, "LDX", 3, 4, 1),
            new Op(0xAF , 1, "LAX", 3, 4, 1),
            new Op(0xB0 , 0, "BCS", 2, 2, 0),
            new Op(0xB1 , 0, "LDA", 2, 5, 8),
            new Op(0xB2 , 1, "KIL", 1, 0, 0),
            new Op(0xB3 , 1, "LAX", 2, 5, 8),
            new Op(0xB4 , 0, "LDY", 2, 4, 5),
            new Op(0xB5 , 0, "LDA", 2, 4, 5),
            new Op(0xB6 , 0, "LDX", 2, 4, 6),
            new Op(0xB7 , 1, "LAX", 2, 4, 6),
            new Op(0xB8 , 0, "CLV", 1, 2, 0),
            new Op(0xB9 , 0, "LDA", 3, 4, 3),
            new Op(0xBA , 0, "TSX", 1, 2, 0),
            new Op(0xBB , 1, "LAS", 3, 4, 3),
            new Op(0xBC , 0, "LDY", 3, 4, 2),
            new Op(0xBD , 0, "LDA", 3, 4, 2),
            new Op(0xBE , 0, "LDX", 3, 4, 3),
            new Op(0xBF , 1, "LAX", 3, 4, 3),
            new Op(0xC0 , 0, "CPY", 2, 2, 0),
            new Op(0xC1 , 0, "CMP", 2, 6, 7),
            new Op(0xC2 , 1, "NOP", 2, 2, 0),
            new Op(0xC3 , 1, "DCP", 2, 8, 7),
            new Op(0xC4 , 0, "CPY", 2, 3, 4),
            new Op(0xC5 , 0, "CMP", 2, 3, 4),
            new Op(0xC6 , 0, "DEC", 2, 5, 4),
            new Op(0xC7 , 1, "DCP", 2, 5, 4),
            new Op(0xC8 , 0, "INY", 1, 2, 0),
            new Op(0xC9 , 0, "CMP", 2, 2, 0),
            new Op(0xCA , 0, "DEX", 1, 2, 0),
            new Op(0xCB , 1, "AXS", 2, 2, 0),
            new Op(0xCC , 0, "CPY", 3, 4, 1),
            new Op(0xCD , 0, "CMP", 3, 4, 1),
            new Op(0xCE , 0, "DEC", 3, 6, 1),
            new Op(0xCF , 1, "DCP", 3, 6, 1),
            new Op(0xD0 , 0, "BNE", 2, 3, 0),
            new Op(0xD1 , 0, "CMP", 2, 5, 8),
            new Op(0xD2 , 1, "KIL", 1, 0, 0),
            new Op(0xD3 , 1, "DCP", 2, 8, 8),
            new Op(0xD4 , 1, "NOP", 2, 4, 5),
            new Op(0xD5 , 0, "CMP", 2, 4, 5),
            new Op(0xD6 , 0, "DEC", 2, 6, 5),
            new Op(0xD7 , 1, "DCP", 2, 6, 5),
            new Op(0xD8 , 0, "CLD", 1, 2, 0),
            new Op(0xD9 , 0, "CMP", 3, 4, 3),
            new Op(0xDA , 1, "NOP", 1, 2, 0),
            new Op(0xDB , 1, "DCP", 3, 7, 3),
            new Op(0xDC , 1, "NOP", 3, 4, 2),
            new Op(0xDD , 0, "CMP", 3, 4, 2),
            new Op(0xDE , 0, "DEC", 3, 7, 2),
            new Op(0xDF , 1, "DCP", 3, 7, 2),
            new Op(0xE0 , 0, "CPX", 2, 2, 0),
            new Op(0xE1 , 0, "SBC", 2, 6, 7),
            new Op(0xE2 , 1, "NOP", 2, 2, 0),
            new Op(0xE3 , 1, "ISC", 2, 8, 7),
            new Op(0xE4 , 0, "CPX", 2, 3, 4),
            new Op(0xE5 , 0, "SBC", 2, 3, 4),
            new Op(0xE6 , 0, "INC", 2, 5, 4),
            new Op(0xE7 , 1, "ISC", 2, 5, 4),
            new Op(0xE8 , 0, "INX", 1, 2, 0),
            new Op(0xE9 , 0, "SBC", 2, 2, 0),
            new Op(0xEA , 0, "NOP", 1, 2, 0),
            new Op(0xEB , 1, "SBC", 2, 2, 0),
            new Op(0xEC , 0, "CPX", 3, 4, 1),
            new Op(0xED , 0, "SBC", 3, 4, 1),
            new Op(0xEE , 0, "INC", 3, 6, 1),
            new Op(0xEF , 1, "ISC", 3, 6, 1),
            new Op(0xF0 , 0, "BEQ", 2, 2, 0),
            new Op(0xF1 , 0, "SBC", 2, 5, 8),
            new Op(0xF2 , 1, "KIL", 1, 0, 0),
            new Op(0xF3 , 1, "ISC", 2, 8, 8),
            new Op(0xF4 , 1, "NOP", 2, 4, 5),
            new Op(0xF5 , 0, "SBC", 2, 4, 5),
            new Op(0xF6 , 0, "INC", 2, 6, 5),
            new Op(0xF7 , 1, "ISC", 2, 6, 5),
            new Op(0xF8 , 0, "SED", 1, 2, 0),
            new Op(0xF9 , 0, "SBC", 3, 4, 3),
            new Op(0xFA , 1, "NOP", 1, 2, 0),
            new Op(0xFB , 1, "ISC", 3, 7, 3),
            new Op(0xFC , 1, "NOP", 3, 4, 2),
            new Op(0xFD , 0, "SBC", 3, 4, 2),
            new Op(0xFE , 0, "INC", 3, 7, 2),
            new Op(0xFF , 1, "ISC", 3, 7, 2),
        };

        static array
            registers = new array() { // port: address, label
            { "2000" , "PPUCTRL" },
            { "2001" , "PPUMASK" },
            { "2002" , "PPUSTATUS" },
            { "2003" , "OAMADDR" },
            { "2004" , "OAMDATA" },
            { "2005" , "PPUSCROLL" },
            { "2006" , "PPUADDR" },
            { "2007" , "PPUDATA" },

            { "4000" , "SQ1_VOL" },
            { "4001" , "SQ1_SWEEP" },
            { "4002" , "SQ1_LO" },
            { "4003" , "SQ1_HI" },
            { "4004" , "SQ2_VOL" },
            { "4005" , "SQ2_SWEEP" },
            { "4006" , "SQ2_LO" },
            { "4007" , "SQ2_HI" },
            { "4008" , "TRI_LINEAR" },
            { "400A" , "TRI_LO" },
            { "400B" , "TRI_HI" },
            { "400C" , "NOISE_VOL" },
            { "400E" , "NOISE_LO" },
            { "400F" , "NOISE_HI" },
            { "4010" , "DMC_FREQ" },
            { "4011" , "DMC_RAW" },
            { "4012" , "DMC_START" },
            { "4013" , "DMC_LEN" },
            { "4014" , "OAM_DMA" },
            { "4015" , "SND_CHN" },
            { "4016" , "JOY1" },
            { "4017" , "JOY2" },

        };
        #endregion

        /// <summary>
        /// this is the direct(ish) converson of the original php code. 
        /// it exists as a baseline and will generallly not be updated.
        /// </summary>
        public class disasm6net
        {
            // port: 'global' variables from original php
            static int origin = 0x8000; // port: start of prg address space, assuming all 32K prg used
            static int labelLen = 0;

            #region disasm6 methods
            // used for branch opcodes
            static string addressOffset(int value, string offset2)
            {
                var offset = hexdec(offset2);
                offset += 2; // length of branch command
                if (offset > 0x80)
                {
                    offset = offset - 0x100;
                }
                else
                {
                    //$offset += 2; // port: disabled in original
                }
                return str_pad(dechex(value + offset), 4, '0', STR_PAD_LEFT);
            }

            static bool isValidLabel(string addr)
            {
                //global $origin; // port: declared static above

                var newaddr = hexdec(addr);

                return (newaddr >= origin && newaddr < 0xFFFA);
            }

            static bool addValidLabel(string addr, array labels)
            {
                if (isValidLabel(addr) && !isset(labels, addr))
                {
                    labels[addr] = true;
                    return true;
                }

                return false;
            }


            static void addVector(string vector, string str, array labels)
            {
                if (isset(labels, vector))
                {
                    var s_labels_vector = labels[vector] as string;
                    if ((labels[vector] is bool))
                    {
                        labels[vector] = str; // port note: address in collection but unlabeled
                    }
                    else if (is_array(labels[vector]))
                    {
                        var a_labels_vector = (string[])labels[vector];
                        if (!in_array(str, a_labels_vector))
                        {
                            labels[vector] = PushArray(a_labels_vector, str);
                        }
                    }
                    else if (s_labels_vector != str)
                    {
                        labels[vector] = PushArray(labels[vector], str); // port note: existing label but different from str, add to collection
                    }
                }
                else
                {
                    labels[vector] = str; // port: vector not added yet, create entry with label 
                }
            }

            static string wordStr(byte[] str, int offset = 0) // port: offset is a new addition from porting
            {
                var h1 = offset + 1;
                var L0 = offset + 0;
                return dechex_pad(ord(str[h1])) + dechex_pad(ord(str[L0]));
            }

            // make sure hex values have leading zeros
            static string dechex_pad(int dec, int len = 2)
            {

                if (dec > 0xFF)
                {
                    len = 4;
                }
                else if (dec > 0xFFFF)
                {
                    len = 6;
                }

                return str_pad(dechex(dec), len, '0', STR_PAD_LEFT);
            }

            // make sure binary values have leading zeros
            static string decbin_pad(int dec, int len = 8)
            {

                if (dec > 0xFF)
                {
                    len = 16;
                }
                else if (dec > 0xFFFF)
                {
                    len = 32;
                }

                return str_pad(decbin(dec), len, '0', STR_PAD_LEFT);
            }

            static string commentLine(int len = 80)
            {
                return ";" + str_repeat('-', len - 1) + "\n";
            }

            static string commentHeader(string text, bool initialNL = true, bool initialLine = true, bool closingLine = true)
            {
                var ret = (initialNL ? "\n" : "") + (initialLine ? commentLine() : "");
                ret += $"; {text}\n";
                if (closingLine) // port: this is a change to help where multiple comments run together
                    ret += commentLine();

                return ret;
            }

            static string strToHex(string str, bool fancy = false) // port note: fancy option not used in original code
            {
                var len = strlen(str);

                var ret = "";

                for (int i = 0; i < len; i++)
                {
                    ret += (fancy ? "$" : "") + dechex_pad(ord(str[i]));

                    if (i < len - 1)
                    {
                        ret += (fancy ? "," : "") + " ";
                    }
                }

                return ret;
            }

            static stdClass getHeaderInfo(FileStream file)
            {
                var oldloc = ftell(file);

                var head = fread(file, 0x4);
                var nes = Encoding.ASCII.GetString(head);

                if (nes == "NES" + chr(0x1A))
                {
                    var info = new stdClass();

                    info.head = head;
                    info.prg = ord(fread(file, 1));
                    info.chr = ord(fread(file, 1));
                    info.ctrl_1 = ord(fread(file, 1));
                    info.ctrl_2 = ord(fread(file, 1));
                    info.tail = fread(file, 8);

                    info.mirroring = byt(info.ctrl_1 & bindec("00000001"));
                    info.sram = byt((info.ctrl_1 & bindec("0000010")) >> 1);
                    info.trainer = byt((info.ctrl_1 & bindec("00000100")) >> 2);
                    info.fourscreen = byt((info.ctrl_1 & bindec("00001000")) >> 3);

                    info.romtype = info.ctrl_2 & bindec("00000011");

                    info.mapper = ((info.ctrl_1 & bindec("11110000")) >> 4)
                       + info.ctrl_2 & bindec("00001111");

                    return info;

                }
                else
                {
                    fseek(file, oldloc);
                    return null;
                }
            }

            static string processHeaderInfo(HeaderInfo info)
            {
                //global $labelLen; // port: declared static above

                var pad = 30 + labelLen;
                var ret = "";
                if (is_object(info))
                {
                    //$ret .= commentLine(); // disabled in original
                    ret += commentHeader("iNES Header");
                    ret += str_pad(LEFT_MARGIN + ".db \"NES\", $1A", pad) + " ; Header\n";
                    ret += str_pad(LEFT_MARGIN + $".db {info.prg}", pad) + $" ; {info.prg} x 16k PRG banks\n";
                    ret += str_pad(LEFT_MARGIN + $".db {info.chr}", pad) + $" ; {info.chr} x 8k CHR banks\n";
                    ret += str_pad(LEFT_MARGIN + ".db %" + decbin_pad(info.ctrl_1), pad) + " ; Mirroring: " + (info.mirroring == 1 ? "Vertical" : "Horizontal") + "\n";
                    ret += str_repeat(" ", pad) + " ; SRAM: " + (info.sram == 1 ? "Enabled" : "Not used") + "\n";
                    ret += str_repeat(" ", pad) + " ; 512k Trainer: " + (info.trainer == 1 ? "Enabled" : "Not used") + "\n";
                    ret += str_repeat(" ", pad) + " ; 4 Screen VRAM: " + (info.fourscreen == 1 ? "Enabled" : "Not used") + "\n";
                    ret += str_repeat(" ", pad) + " ; Mapper: " + info.mapper + "\n";

                    var romtype = string.Empty;
                    switch (info.romtype)
                    {
                        case 0:
                            romtype = "NES";
                            break;
                        case 1:
                            romtype = "VS Unisystem";
                            break;
                        case 2:
                            romtype = "Playchoice 10";
                            break;
                    }
                    ret += str_pad(LEFT_MARGIN + ".db %" + decbin_pad(info.ctrl_2), pad) + " ; RomType: " + romtype + "\n";

                    ret += str_pad(LEFT_MARGIN + ".hex " + strToHex(substr(info.tail, 0, 4)), pad) + " ; iNES Tail \n";
                    ret += str_pad(LEFT_MARGIN + ".hex " + strToHex(substr(info.tail, 4)), pad) + "  \n";

                    return ret;
                }

                return null;
            }

            static string toLittleEndianStr(string str)
            {
                return new string(new[] { str[2], str[3], ' ', str[0], str[1] });
                // port: c# will treat the chars as byte values if using the plus operator (+) to join them
            }

            static string processVectors(string nmi, string reset, string irq_break) // port: just 'break' in the original
            {
                //global $labelLen; // port: declared static above

                var marginLen = strlen(LEFT_MARGIN);
                var pad = 30 + marginLen;

                var ret = commentHeader("Vector Table");
                var line1 = str_pad("vectors:", marginLen);
                line1 += ".dw nmi";
                ret = ret + str_pad(line1, pad) + " ; $fffa: " + toLittleEndianStr(nmi) + "     Vector table\n";
                ret += str_pad(LEFT_MARGIN + ".dw reset", pad) + " ; $fffc: " + toLittleEndianStr(reset) + "     Vector table\n";
                ret += str_pad(LEFT_MARGIN + ".dw irq  ", pad) + " ; $fffe: " + toLittleEndianStr(irq_break) + "     Vector table\n";

                return ret;
            }

            static int baseToDec(string str)
            {
                switch (str[0])
                {
                    case '0':
                        if (str[1] != 'x')
                        {
                            break; //port: this will kick down to the int.Parse() call which .net will try to read as an octal
                            // user has to be careful about defining addresses to avoid this. or todo throw up a warning or something
                        }
                        else
                        {
                            return hexdec(substr(str, 2));
                        }
                        break;

                    case '$':
                        return hexdec(substr(str, 1));
                        break;

                    case '%': //port: apparent typo '$' in original, corrected to binary indicator
                        return bindec(substr(str, 1));
                        break;
                }
                // port: php would happily return the input as is and from there idk
                // .net won't allow that so it's either a number or an exception
                return int.Parse(str);
            }

            static array readLabels(string filename)
            {

                var arr = readLabelText(file_get_contents(filename));

                return arr;


            }
            static array readLabelText(string str)
            {
                var arr = new array();
                var len = 0;
                str = preg_replace("(?m);.*$", "", str); // port: removes semicolon plus anything to end of the line

                List<Match> matches;
                if (preg_match_all(@"(?m)^\s*([a-zA-Z0-9_\-\+\@]*)\s*\=\s*([\$\%]*)([a-fA-F0-9]*)", str, out matches) != 0)
                {
                    foreach (var match in matches)
                    {
                        string
                            matches_1_n = trim(match.Groups[1].Value), // port: label name
                            matches_2_n = match.Groups[2].Value, // port: numeric format token ($,%,empty)
                            matches_3_n = match.Groups[3].Value; // port: address value, digits 0..F

                        // port: the regex pattern allows for an empty string in the address group
                        // which would not make sense to use as a key
                        if (string.IsNullOrWhiteSpace(matches_3_n))
                            throw new Exception($"label '{matches_1_n}' does not have an address specified");

                        int thislen = strlen(matches_1_n);

                        if (thislen > len)
                        {
                            len = thislen;
                        }

                        if (strlen(matches_1_n) > 0)
                        {
                            if (matches_2_n == "") // port note: anything without a group 2 value is parsed as a hex value
                            {
                                matches_3_n = dechex_pad(hexdec(matches_3_n));
                            }

                            if (matches_2_n == "%")
                            {
                                matches_3_n = dechex_pad(bindec(matches_3_n));
                            }

                            // port note: anything with a '$' in group 2 is taken as is, which means it might be
                            // without a leading zero (or extra zeroes) the way addresses are elsewhere in the code 
                            // todo maybe parse and pad
                            arr[strtolower(matches_3_n)] = matches_1_n;
                        }
                    }
                }

                arr["maxLength"] = len;

                return arr;
            }

            static string outputLabels(array arr, string text)
            {
                //global $origin; // port: declared static above

                var ret = commentHeader(text);

                foreach (var n_v in arr)
                {
                    var n = (string)n_v.Key;
                    if (n == "maxLength")
                    {
                        continue;
                    }

                    if (hexdec(n) < origin)
                    {
                        var v = FirstLabel(n_v.Value); // port: this could be a bad conversion, check output
                        ret += str_pad(v, 20) + " = $" + n + "\n";
                    }
                }

                return ret;

            }

            static void outputHelp(string text = null)
            {
                //global $argv;
                //var $dasm = pathinfo(argv[0], PATHINFO_BASENAME); // port: this is never used, commented out so argv isn't an issue

                Console.Write(
@"Usage:

disasm6 <file> [-t <file>] [-o #] [-l <file>] [-cdl <file>] [-cdlo #] [-d] [-i]
         [-h] [-c] [-p #] [-r] [-lc] [-uc] [-fs #] [-cs #] [-fe #] [-ce <#>] 
         [-len #] [-iw] [-m2]

  <file>                The file to disassemble
  t     target <file>   Target output filename (default is input filename.asm)
  o     origin #        Set the program origin.
                           (default: 0x8000 for 32k roms, 0xC000 for 16k roms)
  l     labels <file>   Load user defined labels from file
  cdl   cdl <file>      Use a code/data log generated by FCEUX
  cdlo  cdloffset #     Set the offset of the cdl file
  d     nodetect        Disable 16kb prg size detection
  i     ignoreheader    Do not look for iNES header
  h     noheader        Do not include iNES header (if found) in disassembly
  c     chr             Export CHR-ROM as file and include in disassembly
  p     passes #        Maximum number of passes (default: 9)
  r     registers       Use default NES registers
  lc    lowercase       Use lowercase mnemonics [default]
  uc    uppercase       Use uppercase mnemonics
  fs    filestart       Start reading at a specific file location
  cs    codestart       Start reading at a specific code location
  fe    fileend         Stop reading at a specific file location
  ce    codeend         Stop reading at a specific code location
  len   length          Number of bytes to read
  iw    ignorewrites    Ignore writes to $8000 - $FFFF
  m2    mapper2         Enable mapper 2 (UxROM) support
");

                echo("\n" + (text != null ? $"\nERROR: {text}\n" : ""));
            }

            static bool isCounterLabel(int addr2, array labels) // port note: checks if a counter address exists in the labels collection
            {
                var addr = dechex_pad(addr2);

                var success = false;
                if (isset(labels, addr)) // port: address does exist but...
                {
                    success = true;

                    var labels_addr = AllLabels(labels[addr]);
                    // port: original code does not consider the collection value could be an array
                    // this was restructed to handle arrays as well

                    if (labels_addr.Length == 0)
                        return success; // port: no labels to disqualify the result 

                    bool anyValid = false; // port: any label that doesn't match the regex is valid so the counter is valid

                    foreach (var label in labels_addr)
                    {
                        // port note: something[+-]digits , considered not a valid counter label
                        if (!preg_match(@"^([^\+\-]+)[\+\-][0-9]+", label))
                        {
                            anyValid = true;
                            break;
                        }
                    }

                    success = anyValid;
                }
                return success;
                // port: if not set or nothing in the list was valid (all labels matched pattern) then false

            }
            #endregion

            public static void Run(int argc, string[] argv)
            {
                // port: this is the direct translation of the inline code from the original php, as close as was reasonable

                // Program start
                var time_start = microtime(true);

                var head = "DISASM6 v" + VERSION + " - A NES-oriented 6502 disassembler - Created by Frantik 2015";
                echo($"\n{head}\n" + str_repeat('-', 79) + "\n");


                string filename = null;

                if (!isset(argv, 1)) // port: no input file specified
                {
                    outputHelp(); return; // port: outputHelp had an exit at the end of the method
                }
                else if (!file_exists(argv[1]))
                {
                    outputHelp("File not found\n"); return; // port: first arg not a filename
                }
                else
                {
                    filename = argv[1];
                }

                origin = 0x8000;
                bool showHeader = true;
                bool includeChr = false;
                bool includeReg = false;
                bool originOverride = false;
                bool noDetect = false;
                string shortname = pathinfo(filename, PATHINFO_FILENAME);
                string labelFile = null;
                string cdlFilename = null;
                bool ignoreHeader = false;
                int fileStart = 0;
                bool fileStartOverride = false;
                int fileLength = 0x10000; // port: absolute maximum prg address + 1, counter can not overrun this. TODO needs some work i think
                bool lengthOverride = false;
                int fileEnd = 0;
                bool fileEndOverride = false;
                int codeStart = 0;
                bool codeStartOverride = false;
                int codeEnd = 0;
                bool codeEndOverride = false;
                int cdlOffset = 0;
                bool ignoreWrites = false;
                bool useLowerCase = true;
                bool usingMapper2 = false;
                // port: new options
                bool trace = false;
                string traceFilename = null;

                int lastPass = 9;

                int marginLen = strlen(LEFT_MARGIN);

                #region run arguments
                // check command line params
                for (var i = 2; i < argc; i++) // port: expecting 0=exe, 1=rom, 2...=options
                {
                    string nextParam = null;

                    if (isset(argv, i + 1) && substr(argv[i + 1], 0, 1) != "-")
                    {
                        nextParam = argv[i + 1];
                    }

                    switch (strtolower(argv[i]))
                    {
                        case "-o":
                        case "-origin":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid origin"); return;
                            }

                            origin = baseToDec(argv[++i]);
                            originOverride = true;
                            break;

                        case "-cs":
                        case "-codestart":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid code start location "); return;
                            }

                            codeStart = baseToDec(argv[++i]);
                            codeStartOverride = true;

                            break;

                        case "-fs":
                        case "-filestart":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid file start location "); return;
                            }

                            fileStart = baseToDec(argv[++i]);
                            fileStartOverride = true;
                            break;

                        case "-len":
                        case "-length":
                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid length to read"); return;
                            }

                            fileLength = baseToDec(argv[++i]);  // this will be tweaked later
                            lengthOverride = true;
                            break;

                        case "-fe":
                        case "-fileend":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid file end location "); return;
                            }

                            fileEnd = baseToDec(argv[++i]);
                            fileEndOverride = true;
                            break;

                        case "-ce":
                        case "-codeend":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid code end location "); return;
                            }

                            fileLength = baseToDec(argv[++i]); // will NOT be tweaked since lengthOverride isn't enable
                            codeEndOverride = true; // port: never used?
                            break;


                        case "-h":
                        case "-noheader":
                            showHeader = false;
                            break;

                        case "-i":
                        case "-ignoreheader":
                            ignoreHeader = true;
                            break;

                        case "-c":
                        case "-chr":
                            includeChr = true;
                            break;

                        case "-r":
                        case "-registers":
                            includeReg = true;
                            break;

                        case "-t":
                        case "-target":
                            if (nextParam == null)
                            {
                                outputHelp("You must specify a target file"); return;
                            }

                            var target = argv[++i];
                            shortname = pathinfo(preg_replace(@"%[^a-zA-Z0-9_\-\. ]%", "", target), PATHINFO_FILENAME);
                            _targetPath = Path.GetDirectoryName(target); // port: original code did not save path?

                            break;

                        case "-p":
                        case "-passes":
                            int num;
                            if (!is_numeric(nextParam, out num))
                            {
                                outputHelp("You must specify a number of passes"); return;
                            }

                            lastPass = (int)num; ++i;
                            break;

                        case "-nodetect":
                        case "-d":
                            noDetect = true;
                            break;

                        case "-l":
                        case "-labels":
                            if (nextParam == null || !file_exists(nextParam))
                            {
                                outputHelp("You must specify a valid file"); return;
                            }

                            labelFile = argv[++i];
                            break;


                        case "-cdl":
                            if (nextParam == null || !file_exists(nextParam))
                            {
                                outputHelp("You must specify a valid file"); return;
                            }

                            cdlFilename = argv[++i];
                            break;

                        case "-cdlo":
                        case "-cdloffset":
                            if (nextParam == null)
                            {
                                outputHelp("You must specify a valid offset for the CDL"); return;
                            }

                            cdlOffset = baseToDec(argv[++i]);
                            break;


                        case "-lc":
                        case "-lowercase":
                            useLowerCase = true;

                            break;

                        case "-cc":
                        case "-uc": // port: php source has '-cc', assuming intended to be '-uc'
                        case "-uppercase":
                            useLowerCase = false;

                            break;

                        case "-iw":
                        case "-ignorewrites":

                            ignoreWrites = true;
                            break;

                        case "-m2":
                        case "-mapper2":

                            usingMapper2 = true;
                            break;
                    }

                }
                #endregion

                // port: variables that need to be declared outside their scope of first use
                HeaderInfo headerInfo = null;
                var labels = new array();
                var fileLabels = new array(); // port: labeled addresses read in from file
                var oldPrgLabels = new array(); // port: previous-pass prg labels, empty for first pass
                byte newPrg = 0;
                byte oldPrg = 0;
                byte cdlByte = 0;
                bool oldDidDrawLine = false;
                int invalidCounter = 0;
                var theText = new StringBuilder();
                string nmi = null;
                string reset = null;
                string irq_break = null;

                if (fileEndOverride)
                {
                    fileLength = fileStart + fileEnd;
                    lengthOverride = true;
                }


                var file = fopen(filename, 'r');

                var pass = 1;

                array oldLabels = null;

                var initLabels = new array() {
                    { "fffa" , "vectors" },
                    { "fffc" , true },
                    { "fffe" , true },
                };

                if (includeReg) // port note: include the NES defined function addresses
                {
                    initLabels.AddRange(registers);
                }

                labelLen = 0;
                if (labelFile != null) // port note: read in user defined named addresses from file
                {
                    fileLabels = readLabels(labelFile);

                    //$mapperArr = $fileLabels['mapperArr'];
                    //unset($fileLabels['mapperArr']);
                    // port: disabled in original

                    labelLen = (int)fileLabels["maxLength"] - 10;
                    labelLen = labelLen < 0 ? 0 : labelLen;
                    unset(fileLabels, "maxLength");

                    initLabels.AddRange(fileLabels);

                }

                FileStream cdlFile = null;
                if (cdlFilename != null)
                {
                    cdlFile = fopen(cdlFilename, 'r');
                    cdlByte = 0;
                }


                string header = null;
                string theOldLabel = "";

                theText.Append(commentHeader(pathinfo(filename, PATHINFO_BASENAME) + " disasembled by DISASM6 v" + VERSION, false));
                //$invalidCounter = 0; // port: disabled in original

                var prgBank = 0;
                var theLabel = "";

                int prgOffset = 0; // port: start index for prg within file, 0 or 0x10 (HDR_LEN)
                if (!ignoreHeader)
                {
                    prgOffset = HDR_LEN;
                }

                #region pass loop
                //  This loop is done x times
                //  The first pass we just collect addesses
                //  The next passes we look for new addresses
                //
                //  The last pass we build the actual output
                while (pass <= lastPass)
                {
                    if (pass < 3) // port: why only passes 1 and 2? why at all- would be throwing away labels?
                    {
                        labels = initLabels.AsCopy();
                    }
                    var prgLabels = initLabels.AsCopy();

                    var counter = origin;
                    // port: counter is location within executing address space. 

                    if (fileStartOverride && !codeStartOverride)
                    {
                        fseek(file, fileStart);
                        // port: calculate the equivalent starting point within the prg block
                    }

                    if (!ignoreHeader)
                    {
                        headerInfo = getHeaderInfo(file);
                    }
                    else
                    {
                        headerInfo = null;
                    }

                    if (codeStartOverride)
                    {
                        fseek(file, fileStart);
                        // port: might be wrong initially, fileStart gets recalculated in the pass 1
                    }

                    if (headerInfo != null)
                    {
                        oldPrg = headerInfo.prg;
                    }
                    else
                    {

                    }

                    #region pass 1 only
                    // do this stuff only on the first pass
                    if (pass == 1)
                    {
                        oldDidDrawLine = false;
                        oldLabels = labels.AsCopy(); // port note: previous-pass general labels

                        // check for 16k roms
                        if (!noDetect)
                        {
                            newPrg = 0;
                            if (headerInfo != null && headerInfo.prg == 2) // port: 2 x 16K
                            {
                                var prg0 = fread(file, 0x4000);
                                var prg1 = fread(file, 0x4000);
                                fseek(file, fileStart + HDR_LEN);

                                if (php_bytes_equal(prg0, prg1) && headerInfo.mapper == 0)
                                {
                                    echo("PRG Banks 0 and 1 are identical, overdumped 16k PRG suspected, use -d to disable check\n");
                                    newPrg = 1;

                                    origin = originOverride ? origin : 0xc000;

                                    if (cdlFilename != null)
                                    {
                                        cdlOffset += 0x4000;
                                    }
                                }
                            }
                            else if (headerInfo != null && headerInfo.prg == 1) // port: this is 16K of prg total
                            {
                                origin = originOverride ? origin : 0xc000;
                            }
                        }


                        echo("Using Origin: 0x" + dechex_pad(origin) + "\n\n");


                        if (headerInfo != null)
                        {
                            echo("NES Header Found - " + (showHeader ? "included in disassembly" : "not included") + "\n");
                        }

                        if (labelFile != null)
                        {
                            echo("Using user defined labels\n");
                        }

                        if (includeReg)
                        {
                            echo("Using NES registers\n");
                        }

                        if (cdlFilename != null)
                        {
                            echo("Using code/data log\n");
                        }

                        if (ignoreWrites != false)
                        {
                            echo("Writes to PRG will not create labels\n");
                        }

                        if (usingMapper2 != false)
                        {
                            echo("Mapper 2 (UxROM) support enabled\n");
                        }


                        if (fileStartOverride && !codeStartOverride)
                        {
                            echo("Starting at file location 0x" + dechex_pad(fileStart) + "\n");
                        }

                        if (codeStartOverride)
                        {
                            fileStart = codeStart - origin + (headerInfo != null ? HDR_LEN : 0); // port: was 10 instead of 0x10 in original, assuming bug
                            origin = codeStart;
                            originOverride = true;
                            // port notes: fileStart is an absolute file position, and should be in the prg bytes
                            // origin should be a prg address space value. prg byte 0 maps to this address (maybe?)
                            // codeStart is a prg address where disassembly starts

                            fileStartOverride = true;
                            fseek(file, fileStart);

                            if (fileStart < prgOffset)
                                throw new Exception("invalid file start");

                            cdlOffset += fileStart - (headerInfo != null ? HDR_LEN : 0); // port: was 10 instead of 0x10 in original, assuming bug


                            echo("Starting at code location $" + dechex_pad(fileStart) + "\n");
                        }

                        if (lengthOverride)
                        {
                            echo("Reading 0x" + dechex_pad(fileLength) + " bytes\n");

                            fileLength += origin - (headerInfo != null ? HDR_LEN : 0); // port note correcting for header present/missing
                        }


                        if (includeChr && headerInfo != null)
                        {
                            //echo "Using CHR-ROM\n"; // port: disabled in original
                        }

                        echo("\n");
                    }
                    #endregion

                    if (cdlFilename != null)
                    {
                        fseek(cdlFile, cdlOffset);
                    }

                    //prgBank = 0; // port: TODO uncomment in revised c# version
                    // port: prgBank was never reset in original code, throwing off the output

                    // if 16k rom, update prg info
                    if (newPrg != 0)
                    {
                        headerInfo.prg = newPrg;
                    }

                    #region last pass only
                    // do this stuff only on the lass pass
                    if (pass == lastPass)
                    {

                        if (labelFile != null)
                        {
                            theText.Append(outputLabels(fileLabels, "User Defined Labels"));
                        }

                        if (includeReg)
                        {
                            theText.Append(outputLabels(registers, "Registers"));
                        }

                        header = processHeaderInfo(headerInfo);

                        if (header != null && showHeader)
                        {
                            theText.Append(header);
                        }

                        theText.Append(commentHeader("Program Origin"));
                        theText.Append(str_pad(LEFT_MARGIN + ".org $" + dechex_pad(counter), 30 + labelLen) + " ; Set program counter\n");
                        theText.Append(commentHeader("ROM Start"));

                    }
                    #endregion

                    // read the file
                    // each pass of this loop completes one line of output

                    counter = origin;
                    echo($"Starting pass {pass} " + (pass == lastPass ? "(final) " : "") + "... ");

                    #region byte loop
                    while (!feof(file) && counter < fileLength)
                    {
                        // add = false; // port: not used anywhere
                        var invalidText = "Invalid Opcode";
                        var didDrawLine = false;

                        // handle mapper 2
                        if (usingMapper2
                            && headerInfo != null
                            && headerInfo.mapper == 2
                            && counter == 0xC000
                            && prgBank < (headerInfo.prg - 1) // port: relocated below // todo disable for revised version
                            )
                        {
                            prgBank++; // port change: update prgBank value before checking for last bank

                            //if (prgBank < (headerInfo.prg - 1)) // port: header.prg-1 is fixed last bank // todo enable for revised
                            counter = 0x8000; // port: counter changes, so this block doesn't happen when loop restarts

                            if (pass == lastPass)
                            {
                                theText.Append(commentHeader($"PRG Bank {prgBank}"));
                                theText.Append(LEFT_MARGIN + $".base 0x{dechex_pad(counter)}\n");
                                theText.Append(commentLine());
                            }
                            if (counter == 0x8000) // port: make sure counter has moved before restarting
                                continue;

                        }

                        // handle vectors

                        if (pass < lastPass && counter == 0xFFFA) // port: todo when vectors in every bank (mmc1)
                        {
                            nmi = wordStr(fread(file, 2));
                            reset = wordStr(fread(file, 2));
                            irq_break = wordStr(fread(file, 2));

                            addVector(nmi, "nmi", labels);
                            addVector(reset, "reset", labels);
                            addVector(irq_break, "irq", labels);

                            prgLabels[nmi] = true;
                            prgLabels[reset] = true;
                            prgLabels[irq_break] = true;

                            counter += 6;
                            continue;
                        }
                        else if (pass == lastPass && counter == 0xFFFA)
                        {
                            theText.Append(processVectors(nmi, reset, irq_break));
                            fread(file, 6);

                            counter += 6;

                            continue;
                        }

                        //read opcode
                        var opcode = ord(fread(file, 1));
                        var opinfo = opcodes.FirstOrDefault(n => n.Code == opcode);
                        if (opinfo == null)
                            throw new Exception($"opcode ${opcodes:X2} not in resource list");

                        var isInvalid = opinfo.Legal; // [0];
                        var mnemonic = opinfo.Text; // [1];
                        var byteLen = opinfo.Bytes; // [2];
                        var addressingType = opinfo.AddrMode; //[4];

                        var isDataByte = false;
                        var dataStr = "Suspected data";

                        // check code/data log - if data, don't process as an opcode
                        if (cdlFilename != null)
                        {
                            var newCdlByte = ord(fread(cdlFile, 1));


                            // draw line between data and code
                            if (pass == lastPass
                                && !oldDidDrawLine
                                && counter != origin
                                && newCdlByte != 0
                                && ((newCdlByte & CDL_CODE) != (cdlByte & CDL_CODE)) // port: change from data to code or vice versa
                            )
                            {
                                theText.Append("\n" + commentLine());

                                didDrawLine = true;
                            }

                            // check if the CDL byte is known, if known, copy, otherwise do some checks
                            var dechex_pad_counter = dechex_pad(counter);
                            if (newCdlByte != 0)
                            {
                                cdlByte = newCdlByte;
                            }
                            // if byte is zero and we're at a program label, assume code
                            else if (isset(oldPrgLabels, dechex_pad_counter))
                            {
                                cdlByte = CDL_CODE; // port: bindec('00000001')
                            }
                            // if byte is zero and we're at a label, but not program, assume data (only on 2nd pass)
                            else if (isset(oldLabels, dechex_pad_counter) && pass > 1)
                            {
                                cdlByte = CDL_DATA; // port: bindec('00000010')
                            }
                            // else assume program code


                            // data byte
                            if ((cdlByte & CDL_DATA) != 0 && (cdlByte & CDL_CODE) == 0) // port: originally bindec('00000010') bindec('00000001')
                            {

                                var counter_pad = dechex_pad(counter);

                                if (isCounterLabel(counter, oldLabels))
                                {
                                    theOldLabel = (oldLabels[counter_pad] is bool)
                                        ? "__" + counter_pad
                                        : FirstLabel(oldLabels[counter_pad]);

                                    //$theOldLabel = preg_replace('%^([^\+\-]+)[\+\-][0-9]+%', '$1', $theOldLabel); // port: disabled in original
                                }

                                // port: JumpTable/RTSTable don't appear anywhere else in the original php
                                if (substr(theOldLabel, -9) == "JumpTable")
                                {

                                    byteLen = 2;
                                    mnemonic = ".word";
                                    addressingType = 11;
                                    isInvalid = 0;
                                    //fseek($file, ftell($file) - 1); // port: disabled in original



                                }
                                else if (substr(theOldLabel, -8) == "RTSTable")
                                {

                                    byteLen = 2;
                                    mnemonic = ".word";
                                    addressingType = 12;
                                    isInvalid = 0;

                                }/*
                                elseif (substr($theOldLabel, -8) == 'TableLow')
                                {
                                   $byteLen = 1;
                                   $mnemonic = '.byte';
                                   $addressingType = 13;
                                   $isInvalid = 0;
                                }
                                elseif (substr($theOldLabel, -9) == 'TableHigh')
                                {
                                   $byteLen = 1;
                                   $mnemonic = '.byte';
                                   $addressingType = 14;
                                   $isInvalid = 0;
                                }  */ // port: disabled in original
                                else
                                {
                                    byteLen = 4;
                                    //echo substr($theLabel, -11); // port: disabled in original
                                    mnemonic = "";
                                    addressingType = -1;
                                    isInvalid = 1;
                                }
                                isDataByte = true;
                                dataStr = "Data";
                            }
                        }
                        else
                        {
                            theOldLabel = "";  // Reset 'theOldLabel' when we are no longer in a known data byte
                        }


                        var readBytes = byteLen - 1;
                        //var bytes = ""; // port: now declared where it's used below
                        var byteStr = "";
                        var trailer = "";
                        var hextext = dechex_pad(opcode);

                        var byteArr = new[] { hextext }.ToList();


                        // read 1 or 2 byte paramters for the opcode
                        if (readBytes > 0)
                        {
                            var cdlPos = 0;
                            var didMoveCdlPtr = false;

                            if (pass >= 1) // port note: this is never not true, passes start at 1
                            {
                                if (cdlFilename != null)
                                {
                                    cdlPos = ftell(cdlFile);
                                    didMoveCdlPtr = false;
                                }
                                // check to see if a label exists in this opcode.. if so then usually it's data
                                for (var i = 1; i <= readBytes; i++)
                                {
                                    var counter_i = counter + i;
                                    if (isCounterLabel(counter_i, oldLabels)
                                       //if (isset($oldLabels[dechex_pad($counter + $i)]) // port: disabled in original, in favor of condition above apparently
                                       || counter_i >= 0xFFFA
                                       || (counter_i >= fileLength)
                                       //|| (operand == 0xff && _branches.Contains(mnemonic)) // port: branch into self check // todo enable for revised version
                                       || (usingMapper2 && headerInfo != null && headerInfo.mapper == 2 && counter_i > 0xBFFF && prgBank < headerInfo.prg - 1)
                                     ) // if counter in the vectors

                                    {
                                        invalidCounter = 0; // port note: set but never used
                                        readBytes = i - 1;
                                        isInvalid = 1;
                                        byteLen = i;
                                        addressingType = -1;
                                        continue;
                                    }

                                    // if this byte marked as data in cdl; check if next bytes are code
                                    if (cdlFilename != null && isDataByte)
                                    {
                                        var newCdlByte = ord(fread(cdlFile, 1));
                                        didMoveCdlPtr = true;
                                        if ((newCdlByte & CDL_CODE) != 0) // port: originally bindec('00000001'))
                                        {
                                            invalidCounter = 0; // port note: set but never used
                                            readBytes = i - 1;
                                            isInvalid = 1;
                                            byteLen = i;
                                            addressingType = -1;
                                            continue;
                                        }
                                    }
                                }

                                if (didMoveCdlPtr && cdlFilename != null)
                                {
                                    fseek(cdlFile, cdlPos);
                                }

                            }

                            if (readBytes > 0) // if readbytes is still > 0 after above
                            {
                                var bytes = fread(file, readBytes);

                                if (cdlFilename != null)
                                {
                                    var cdlBytes = fread(cdlFile, readBytes); // port note: just advancing cdlFile pointer?
                                }

                                for (var j = 0; j < readBytes; j++)
                                {
                                    byteArr.Add(dechex_pad(bytes[j]));
                                    //hextext += ' ' + byteArr[j + 1]; // port: this happens all at once after the loop now
                                }

                                if (addressingType == 12) // port: jump table
                                {
                                    byteStr = (isset(byteArr, 1) ? byteArr[1] : "") + byteArr[0];
                                    byteStr = dechex_pad(hexdec(byteStr) + 1);
                                }
                                else if (addressingType == 11) // port: rts table
                                {
                                    byteStr = (isset(byteArr, 1) ? byteArr[1] : "") + byteArr[0];
                                }
                                else
                                {
                                    byteStr = (isset(byteArr, 2) ? byteArr[2] : "") + byteArr[1];
                                }
                            }
                        }

                        // ASM6 seems to do some optimization and won't allow absolute addr mode
                        // when using $00xx.. it turns it into $xx
                        // so instead we'll use .hex
                        if (readBytes == 2
                           && substr(byteStr, 0, 2) == "00"
                           && addressingType > 0
                           && addressingType < 9
                           && addressingType != 3)
                        {
                            isInvalid = 1;
                            invalidText = "Bad Addr Mode";
                        }

                        // add label to list
                        string oldByteStr = byteStr;
                        string lbl = "$";

                        if (addressingType > 0
                            && isValidLabel(byteStr)
                            && !(ignoreWrites && substr(mnemonic, 0, 2) == "ST" && (hexdec(byteStr) < 0x8000))) // do not add labels when writing to PRG 
                        {

                            lbl = "__";

                            if (pass < lastPass && isInvalid != 1)
                            {

                                addValidLabel(byteStr, labels);
                            }

                        }

                        oldByteStr = byteStr;

                        //    byteStrDec = (dechex_pad(byteStr); // port: disabled in original
                        var newByteStr = lbl + byteStr;

                        if (isset(oldLabels, byteStr) && lbl != "")
                        {
                            var oldLabel = oldLabels[byteStr];

                            newByteStr = (oldLabel is bool)
                               ? newByteStr
                               : FirstLabel(oldLabel);
                        }

                        // lets check for various addressing types to figure out how to format the text
                        switch (addressingType)
                        {

                            case 0: // Implicit/Accumulator/Immediate
                                byteStr = (readBytes > 0 ? "#$" + byteStr : "");
                                break;

                            case 12: // port: jump table
                            case 11: // port: rts table
                            case 10: // port: jsr, jmp
                                if (isInvalid != 1)
                                {
                                    addValidLabel(byteStr, prgLabels);
                                }
                                byteStr = newByteStr;

                                if (addressingType == 12)
                                {
                                    byteStr += "-1";
                                }
                                break; // port note: original code had fall through to 1,4 case. properly separated now.

                            case 1: // Absolute
                            case 4: // Zero Page
                                byteStr = newByteStr; // port: shared line in original code
                                break;

                            case 2: // Absolute X
                            case 5: // Zero Page X
                                byteStr = newByteStr + ",x";
                                break;

                            case 3: // Absolute Y
                            case 6: // Zero Page Y
                                byteStr = newByteStr + ",y";
                                break;

                            case 7: // Indrect X
                                byteStr = "(" + newByteStr + ",x)";
                                break;

                            case 8: // Indirect Y
                                byteStr = "(" + newByteStr + "),y";
                                break;

                            case 9: // Indirect Jump
                                byteStr = "(" + newByteStr + ")";
                                break;

                            case DAT: // port: added to help with troubleshooting
                                      // this is expected, don't do anything
                                break;

                            default: // port: added to help with troubleshooting
                                Console.WriteLine($"unexcpected address type: {addressingType}");
                                break;
                        }

                        // now lets cover specific mnemonics

                        switch (mnemonic)
                        {
                            // handle branches
                            case "BCC":
                            case "BCS":
                            case "BEQ":
                            case "BMI":
                            case "BNE":
                            case "BPL":
                            case "BVC":
                            case "BVS":

                                var addr = addressOffset(counter, oldByteStr);

                                var isInvalidBranch = false; // port note: never set to anything else in original code

                                if (pass < lastPass && isInvalid != 1 && !isInvalidBranch)
                                {
                                    addValidLabel(addr, labels);
                                    addValidLabel(addr, prgLabels);
                                }

                                if (!isInvalidBranch && isValidLabel(addr))
                                {
                                    // port: 'true' is used as a placeholder when there's no explicit label for an address
                                    // label placeholder won't ever be literally false, so this is converted as a test for bool type
                                    if (isset(labels, addr) && !(labels[addr] is bool))
                                    {
                                        byteStr = FirstLabel(labels[addr]);
                                    }
                                    else
                                    {

                                        byteStr = "__" + addr;
                                    }
                                }
                                else
                                {
                                    isInvalid = 1;
                                    invalidText = "Illegal Branch";
                                }

                                break;

                            // add some space after RTS/JMP
                            case "RTS":
                            case "RTI":
                            case "JMP":
                                if (isInvalid != 1)
                                {
                                    trailer = "\n" + commentLine();
                                    didDrawLine = true;
                                }
                                break;

                        }

                        #region output
                        // only deal with output on last pass
                        if (pass == lastPass)
                        {
                            hextext = string.Join(" ", byteArr); // port: this is where the disabled line above is reimplemented
                            string oldMnemonicStr = string.Empty;
                            if (isInvalid == 1)
                            {
                                oldMnemonicStr = addressingType == -1 ? dataStr : (invalidText + " - " + mnemonic + " " + byteStr);
                                mnemonic = ".hex";
                                byteStr = hextext;

                            }
                            var counter_pad = dechex_pad(counter);
                            if (array_key_exists(counter_pad, oldLabels))
                            {
                                var leng = 1; // port note: number of labels for address
                                var a_oldLabels_counter_pad = oldLabels[counter_pad] as string[];
                                if (is_array(a_oldLabels_counter_pad))
                                {
                                    leng = a_oldLabels_counter_pad.Length;
                                }

                                var labelsSub = new StringBuilder(); // port: for gathering address labels together before outputting, to prevent interruptions
                                for (var i = 0; i < leng; i++)
                                {
                                    if (is_array(a_oldLabels_counter_pad))
                                    {
                                        theLabel = a_oldLabels_counter_pad[i];
                                    }
                                    else
                                    {
                                        theLabel = (oldLabels[counter_pad] is bool)
                                           ? "__" + counter_pad
                                           : FirstLabel(oldLabels[counter_pad]);

                                    }

                                    // if label has a + or - in it but doesn't start with one
                                    // then don't show it
                                    // if not 0 or false
                                    if (strpos(theLabel, '+') > 0 || strpos(theLabel, '-') > 0)
                                    {
                                        theText.Append(LEFT_MARGIN);
                                        continue;
                                    }

                                    bool drawn = oldDidDrawLine || didDrawLine; // port: refactoring to simplify
                                    bool lastComment = i == leng - 1;
                                    switch (theLabel)
                                    {
                                        case "irq":
                                            theText.Append(commentHeader("irq/brk vector", !drawn, !drawn, lastComment));
                                            break;

                                        case "nmi":
                                        case "reset":
                                            theText.Append(commentHeader($"{theLabel} vector", !drawn, !drawn, lastComment));
                                            break;

                                    }

                                    if (strlen(theLabel) >= marginLen - 1)
                                    {
                                        labelsSub.Append((drawn || (counter == origin) ? "" : "\n")
                                            + $"{theLabel}:\n");
                                        // port note if label is too long then put content on next line
                                        if (lastComment) // port: only on the last label
                                            labelsSub.Append(LEFT_MARGIN); // port: separated from prev statement
                                    }
                                    else
                                    {
                                        labelsSub.Append(str_pad(theLabel + ":", marginLen));
                                        if (!lastComment) // port: any but the last label
                                            labelsSub.AppendLine(); // port: original did not have a newline in this else block
                                    }

                                }

                                theText.Append(labelsSub); // port: print the labels after the vector comment block
                            }
                            else
                            {
                                theText.Append(LEFT_MARGIN);
                            }

                            //$line = array_key_exists(dechex_pad($counter), $oldLabels) ? '__' . dechex_pad($counter) .':' : '       '; // port: disabled in original
                            var line = "";
                            line += (useLowerCase ? strtolower(mnemonic) : mnemonic) + " " + byteStr;

                            //$labelLen
                            line = str_pad(line, 30 - marginLen + labelLen);

                            line += " ; $" + dechex_pad(counter) + ": " + hextext; // port: this is the post-instruction comment
                            line = str_pad(line, (isDataByte ? 54 : 50) - marginLen + labelLen);
                            line += (isInvalid == 1 ? oldMnemonicStr : "");
                            line += "\n" + trailer;

                            theText.Append(line);

                        }
                        #endregion

                        counter += byteLen;
                        oldDidDrawLine = didDrawLine;
                    }  // end line by line loop
                    #endregion

                    // port note: if no change in labels this pass, lastPass truncated to skip any redundant passes
                    if (pass < lastPass && oldLabels != null && php_dictionaries_equal(labels, oldLabels))
                    {
                        lastPass = pass + 1;
                    }
                    /*elseif($pass < $lastPass)
                    {
                        echo "$pass < $lastPass && $oldLabels !== false && $labels == $oldLabels (".print_r($labels == $oldLabels, true);
                        file_put_contents('out', print_r($labels, true).print_r($oldLabels, true));
                        file_put_contents('out2', print_r(array_diff_assoc($labels, $oldLabels), true));
                    }*/ // port: disabled in original

                    if (pass < lastPass)
                    {

                        oldLabels = labels.AsCopy();
                        oldPrgLabels = prgLabels.AsCopy();

                        rewind(file);
                    }

                    echo("complete\n");
                    pass++;
                }
                #endregion

                if (includeChr && headerInfo != null)
                {

                    fseek(file, oldPrg * 0x4000 + HDR_LEN);

                    byte[] chr = null;


                    while (!feof(file))
                    {
                        chr = fread(file, headerInfo.chr * LEN_8K); // port: condensing this to one read, chr banks x 8K
                    }

                    if (chr == null || chr.Length == 0)
                    {
                        echo($"\nNo CHR-ROM data available");
                    }
                    else
                    {

                        theText.Append("\n" + commentLine());
                        theText.Append("; CHR-ROM");
                        theText.Append("\n" + commentLine());

                        var incLine = LEFT_MARGIN + ".incbin " + shortname + ".chr";
                        theText.Append(str_pad(incLine, 30 + labelLen) + " ; Include CHR-ROM\n");

                        file_put_contents(shortname + ".chr", chr);
                        echo($"\nCHR-ROM exported as {shortname}.chr");
                    }

                }
                else if (includeChr)
                {
                    echo("\nCHR-ROM cannot be exported without iNES header data");
                    if (ignoreHeader)
                    {
                        echo("\nTry disabling -ignoreheader if you wish to export CHR-ROM data"); // port: original did not explicitly echo, assuming it was intended to do so
                    }
                }

                file_put_contents(shortname + ".cs.asm", theText);


                var time_end = microtime(true);
                var time = round(time_end - time_start, 3);

                echo($"\nDisassembly {shortname}.cs.asm generated in {time} seconds\n\n");
            }
        }

    }
}
