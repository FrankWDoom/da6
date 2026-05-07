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
        /**
        *    DISASM6 - A NES-oriented 6502 disassembler which produces asm6 code
        *    Created by Frantik 2011-2015
        *    ported by FrankWDoom
        */

        /// <summary>
        /// this is the .net version of the app as it will be going forward
        /// </summary>
        public class Disassembler
        {
            const string VERSION = "1.5.2";

            // port: this list is used when checking for code or data
            static string[] _branches = new[] {
                "BCC",
                "BCS",
                "BEQ",
                "BMI",
                "BNE",
                "BPL",
                "BVC",
                "BVS",
            };

            // port: 'global' variables from original php
            static int origin = CPU_ADDR_BASE; // port: start of prg address space, assuming all 32K prg
            static int labelLen = 0;

            #region disasm6 methods
            // used for branch opcodes
            int addressOffset(int value, string offset2)
            {
                // port fix: original version was adding command length to offset
                // triggering + to - correction when the +2 put the offset over 0x80

                var offset = hexdec(offset2);
                var cmdEnd = value + 2; // length of branch command

                if (offset >= 0x80)
                {
                    offset = offset - 0x100;
                }

                return (cmdEnd + offset); //.ToString("x4");
                //return str_pad(dechex(value + offset), 4, '0', STR_PAD_LEFT);
            }

            bool isValidLabel(int addr)
            {
                return (addr >= origin && addr < V_NMI);
            }

            bool addValidLabel(int addr, AsmLabels labels)
            {
                if (isValidLabel(addr) && !labels.ContainsKey(addr))
                {
                    labels.Add(addr);
                    return true;
                }

                return false;
            }

            void addVector(int vector, string str, AsmLabels labels)
            {
                // todo this could be method of Labels class
                if (!labels.ContainsKey(vector))
                    labels.Add(vector);

                if (!string.IsNullOrWhiteSpace(str))
                {
                    var list = labels[vector];
                    if (!list.Contains(str))
                        list.Add(str);
                }
            }

            /// <summary>
            /// reads two values in lsb order and combines them into a msb order string
            /// </summary>
            /// <param name="str">list of printed hex values</param>
            /// <param name="offset">index to low byte</param>
            /// <returns></returns>
            string wordStr(List<string> str, int offset = 0) // port: added optional offset value
            {
                int h1 = offset + 1;
                int L0 = offset + 0;
                var wordHi = (str.Count > h1) ? str[h1] : string.Empty;
                var wordLo = str[L0];

                var byteText = wordHi + wordLo;
                return byteText;
            }

            /// <summary>
            /// interpret 2 little endian bytes as an integer
            /// </summary>
            /// <param name="str"></param>
            /// <param name="offset"></param>
            /// <returns></returns>
            int wordAddr(byte[] str, int offset = 0) // port: added optional offset value
            {
                int h1 = offset + 1;
                int L0 = offset + 0;

                int addr = (str[h1] << 8) + str[L0];
                return addr;
            }

            /// <summary>
            /// make sure hex values have leading zeros
            /// </summary>
            /// <param name="dec"></param>
            /// <param name="len">not implemented</param>
            /// <returns></returns>
            string hex_pad(int dec, int len = 2)
            {
                var hexStr = dechex(dec); //.PadLeft(len, '0');
                var padded = PadToByteWidth(hexStr, HEX_ALIGN);
                return padded;
            }

            /// <summary>
            /// make sure binary values have leading zeros
            /// </summary>
            /// <param name="dec"></param>
            /// <param name="len">not implemented</param>
            /// <returns></returns>
            string bin_pad(int dec, int len = 8)
            {
                var binStr = decbin(dec); //.PadLeft(len, '0');
                var padded = PadToByteWidth(binStr, BIN_ALIGN);
                return padded;
            }

            ///// <summary>
            ///// pad a number string to byte width
            ///// </summary>
            ///// <param name="numberStr">printed number</param>
            ///// <param name="byteAlign">number of digits representing one byte</param>
            ///// <param name="padChar">padding character (default '0')</param>
            ///// <returns></returns>
            //string PadToByteWidth(string numberStr, int byteAlign, char padChar = '0')
            //{
            //    var ctBytes = numberStr.Length / byteAlign; // number of bytes currently represented
            //    if (numberStr.Length % byteAlign != 0)
            //    {
            //        ctBytes++; // align to next full byte width
            //    }

            //    var padded = numberStr.PadLeft(ctBytes * byteAlign, padChar);
            //    return padded;
            //}

            /// <summary>
            /// generate a commented line of dashes for asm output to use as a separator
            /// </summary>
            /// <param name="len"></param>
            /// <returns></returns>
            string commentLine(int len = 80)
            {
                return ";".PadRight(len, '-') + Environment.NewLine;
            }

            /// <summary>
            /// generate a comment block for asm output containing the given text
            /// </summary>
            /// <param name="text"></param>
            /// <param name="initialNL"></param>
            /// <param name="initialLine"></param>
            /// <param name="closingLine"></param>
            /// <returns></returns>
            string commentHeader(string text, bool initialNL = true, bool initialLine = true, bool closingLine = true)
            {
                var ret = new StringBuilder();

                if (initialNL)
                    ret.AppendLine();
                if (initialLine)
                    ret.Append(commentLine());

                ret.AppendLine($"; {text}");

                if (closingLine) // port: this is a change to correct some issues where multiple comments run together
                    ret.Append(commentLine());

                return ret.ToString();
            }

            /// <summary>
            /// print a series of bytes as hex values
            /// </summary>
            /// <param name="str"></param>
            /// <param name="startIndex"></param>
            /// <param name="length"></param>
            /// <param name="fancy">never used, omitted to simplify</param>
            /// <returns></returns>
            string strToHex(byte[] str, int startIndex = 0, int length = -1)
            {
                // port: function name doesn't make any sense now but left as is for consistency
                var words = new List<string>();

                if (startIndex < 0)
                    startIndex = 0;

                if (length == -1 || length > str.Length)
                    length = str.Length;

                for (int j = startIndex; j < length; j++)
                {
                    var b = str[j];
                    words.Add(hex_pad(b));
                }

                var ret = string.Join(" ", words);
                return ret;
            }

            HeaderInfo getHeaderInfo(FileStream file) // port: this is the equivalanet to the original method
            {
                var oldloc = file.Position;

                var head = fread(file, HDR_LEN);
                var info = getHeaderInfo(head);
                if (info == null)
                    fseek(file, oldloc);
                return info;
            }

            HeaderInfo getHeaderInfo(byte[] file, int loc = 0) // port: this version works on data already loaded
            {
                var head = fread(file, 0x4, ref loc);

                if (Encoding.ASCII.GetString(head) == "NES" + chr(0x1A))
                {
                    var info = new stdClass();

                    info.head = head;
                    info.prg = ord(fread(file, 1, ref loc));
                    info.chr = ord(fread(file, 1, ref loc));
                    info.ctrl_1 = ord(fread(file, 1, ref loc));
                    info.ctrl_2 = ord(fread(file, 1, ref loc));
                    info.tail = fread(file, 8, ref loc);

                    info.mirroring = byt(info.ctrl_1 & bindec("00000001"));
                    info.sram = byt((info.ctrl_1 & bindec("0000010")) >> 1);
                    info.trainer = byt((info.ctrl_1 & bindec("00000100")) >> 2);
                    info.fourscreen = byt((info.ctrl_1 & bindec("00001000")) >> 3);

                    info.romtype = info.ctrl_2 & bindec("00000011");

                    info.mapper = ((info.ctrl_1 & bindec("11110000")) >> 4)
                       + info.ctrl_2 & bindec("00001111"); // port: todo make sure math comes out right

                    return info;

                }
                else
                {
                    echo("Invalid header, use -ignoreheader switch for headerless files");
                    return null;
                }
            }

            string processHeaderInfo(HeaderInfo info)
            {
                var pad = 30 + labelLen;
                var ret = "";

                if (!is_object(info))
                    return null;

                var flagsIndent = str_repeat(" ", pad);

                ret += commentHeader("iNES Header");
                ret += str_pad(LEFT_MARGIN + ".db \"NES\", $1A", pad) + " ; Header\n";
                ret += str_pad(LEFT_MARGIN + $".db {info.prg}", pad) + $" ; {info.prg} x 16k PRG banks\n";
                ret += str_pad(LEFT_MARGIN + $".db {info.chr}", pad) + $" ; {info.chr} x 8k CHR banks\n";
                ret += str_pad(LEFT_MARGIN + ".db %" + bin_pad(info.ctrl_1), pad) + " ; Mirroring: " + (info.mirroring == 1 ? "Vertical" : "Horizontal") + "\n";
                ret += flagsIndent + " ; SRAM: " + (info.sram == 1 ? "Enabled" : "Not used") + "\n";
                ret += flagsIndent + " ; 512k Trainer: " + (info.trainer == 1 ? "Enabled" : "Not used") + "\n";
                ret += flagsIndent + " ; 4 Screen VRAM: " + (info.fourscreen == 1 ? "Enabled" : "Not used") + "\n";
                ret += flagsIndent + " ; Mapper: " + info.mapper + "\n";

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
                ret += str_pad(LEFT_MARGIN + ".db %" + bin_pad(info.ctrl_2), pad) + " ; RomType: " + romtype + "\n";

                ret += str_pad(LEFT_MARGIN + ".hex " + strToHex(info.tail, 0, 4), pad) + " ; iNES Tail \n";
                ret += str_pad(LEFT_MARGIN + ".hex " + strToHex(info.tail, 4), pad) + "  \n";

                return ret;
            }

            string toLittleEndianStr(string str)
            {
                if (str == null)
                    return null;

                var s = $"{str} {str}".Substring(2, 5);
                return s;
                //return new string(new char[] { str[2], str[3], ' ', str[0], str[1] });
            }

            string processVectors(string nmi, string reset, string irq_break)
            {
                var marginLen = strlen(LEFT_MARGIN);
                var pad = 30 + marginLen;

                var ret = commentHeader("Vector Table");
                var line1 = str_pad("vectors:", marginLen);
                line1 += ".dw nmi";
                ret = ret + str_pad(line1, pad) + " ; $fffa: " + toLittleEndianStr(nmi) + "     Vector table\n";
                ret += str_pad(LEFT_MARGIN + ".dw reset", pad) + " ; $fffc: " + toLittleEndianStr(reset) + "     Vector table\n";
                ret += str_pad(LEFT_MARGIN + ".dw irq", pad) + " ; $fffe: " + toLittleEndianStr(irq_break) + "     Vector table\n";

                return ret;
            }

            int baseToDec(string str)
            {
                switch (str[0])
                {
                    case '0':
                        if (str[1] != 'x')
                            break; //port: this will kick down to the int.Parse() call which .net will try to read as an octal
                                   // user has to be careful about defining addresses to avoid this. or todo throw up a warning or something

                        return hexdec(substr(str, 2));

                    case '$':
                        return hexdec(substr(str, 1));

                    case '%': //port: apparent typo '$' in original, corrected to binary indicator
                        return bindec(substr(str, 1));
                }

                // port: php would happily return the input as is and from there idk
                // .net won't allow that so it's either a number or an exception
                return int.Parse(str);
            }

            AsmLabels readLabels2(string filename, out int maxLength)
            {
                var contents = file_get_contents(filename);
                var arr = readLabelText2(contents, out maxLength);
                return arr;
            }

            AsmLabels readLabelText2(string str, out int maxLength)
            {
                var arr = new AsmLabels();
                var len = 0;
                str = preg_replace("(?m);.*$", "", str); // port: removes semicolon plus anything to end of the line

                // todo extend labels to specify Read/Write/eXecute and choose the appropriate one for the op

                var matches = new List<Match>();
                if (preg_match_all(@"(?m)^\s*([a-zA-Z0-9_\-\+\@]*)\s*\=\s*([\$\%]*)([xXa-fA-F0-9]+)", str, out matches) != 0)
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
                                matches_3_n = hex_pad(hexdec(matches_3_n)); // port: todo verify conversion
                            }

                            if (matches_2_n == "%")
                            {
                                matches_3_n = hex_pad(bindec(matches_3_n));
                            }

                            // port note: anything with a '$' in group 2 is taken as is, which means it might be
                            // without a leading zero (or extra zeroes) the way addresses are elsewhere in the code 
                            // todo maybe parse and pad
                            int addr = hexdec(matches_3_n);
                            arr.Add(addr, matches_1_n);
                        }
                    }
                }

                maxLength = len;

                return arr;
                //var matches = Regex.Matches(str, @"^\s*([a-zA-Z0-9_\-\+\@]*)\s*=\s*([\$\%]*)([a-fA-F0-9]*)", RegexOptions.Multiline);
            }

            /// <summary>
            /// print a list of non-prg label definitions (eg nes registers)
            /// </summary>
            /// <param name="arr"></param>
            /// <param name="text"></param>
            /// <returns></returns>
            string outputLabels(AsmLabels arr, string text)
            {
                var ret = commentHeader(text);

                foreach (var n_v in arr)
                {
                    var n = n_v.Key;
                    if (n < origin) // port: this is probably a nes register, or maybe something in prg ram?
                    {
                        var addr = hex_pad(n);
                        foreach (var lbl in n_v.Value)
                        {
                            var v = str_pad(lbl, 20);
                            ret += v + " = $" + addr + "\n";
                        }
                    }
                }

                return ret;
            }

            string outputLabels(array arr, string text)
            {
                var ret = commentHeader(text);

                foreach (var n_v in arr)
                {
                    var n = (string)n_v.Key;
                    if (n == "maxLength")
                    {
                        continue;
                    }

                    if (hexdec(n) < origin) // port: this is probably a nes register, or maybe something in prg ram?
                    {
                        var v = Convert.ToString(n_v.Value); // port: todo this could be an array, check output
                        ret += str_pad(v, 20) + " = $" + n + "\n";
                    }
                }

                return ret;

            }

            void outputHelp(string text = null)
            {
                //global argv;
                //var dasm = pathinfo(argv[0], PATHINFO_BASENAME); // port: this is never used, commented so argv isn't an issue

                Console.Write(
@"Usage:

disasm6 <file> [-t <file>] [-o #] [-l <file>] [-cdl <file>] [-cdlo #] [-d] [-i]
         [-h] [-c] [-p #] [-r] [-lc] [-uc] [-fs #] [-cs #] [-fe #] [-ce <#>] [-b <#>]
         [-len #] [-iw] [-m2] [-v #] [-xt [<file>]]

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
  b     bank #          ROM bank number containing codestart/codeend (if prg banking is involved)
  len   length          Number of bytes to read
  iw    ignorewrites    Ignore writes to $8000 - $FFFF
  m2    mapper2         Enable mapper 2 (UxROM) support
  v     vectors         Read 6502 vectors from specific file location
  nv    novectors       Do not look for 6502 vectors
  xt    trace           Trace code from known execution start points 
");

                echo("\n" + (text != null ? $"\nERROR: {text}\n" : ""));
            }

            /// <summary>
            /// checks if a counter address exists in the labels collection
            /// </summary>
            /// <param name="addr">counter address</param>
            /// <param name="labels"></param>
            /// <returns></returns>
            bool isCounterLabel(int addr, AsmLabels labels)
            {
                var success = false;

                if (labels.ContainsKey(addr)) // port: address does exist but...
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

            /// <summary>
            /// generate a pad instruction for output
            /// </summary>
            /// <param name="endAddr">pad up to address (exclusive)</param>
            /// <param name="value">fill byte</param>
            /// <returns></returns>
            private string WritePad(int endAddr, byte value = 0)
            {
                var line = $".pad ${hex_pad(endAddr)}";
                if (value != 0)
                    line += $",${hex_pad(value)}";
                // todo generate trailing comment
                return line;
            }

            public void Run(int argc, string[] argv)
            {
                // port: this is the revised version of the c# reference conversion, focusing on native .net methods
                // fixes and feature updates will happen here going forward

                var time_start = microtime(true);
                // Program start

                var head = "DISASM6 v1.5 - A NES-oriented 6502 disassembler - Created by Frantik 2015\n";
                head += $".NET port v{VERSION} by FrankWDoom 2026";

                echo($"\n{head}\n" + str_repeat('-', 79) + "\n");

                if (!isset(argv, 1)) // port: no input file specified
                {
                    outputHelp(); return; // port: outputHelp had an exit at the end of the method
                }
                else if (!File.Exists(argv[1]))
                {
                    outputHelp("File not found\n"); return; // port: first arg not a filename
                }

                var filename = argv[1];

                origin = CPU_ADDR_BASE;
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
                int codeEnd = -1;
                bool codeEndOverride = false;
                int bankNumber = -1;
                int cdlOffset = 0;
                bool ignoreWrites = false;
                bool useLowerCase = true;
                bool usingMapper2 = false;
                int mapperNumber = 0;
                bool mapperOverride = false;
                // port: new options
                int vectorsFilePos = 0;
                bool vectorsOverride = false;
                bool noVectors = false;
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
                                outputHelp("Must specify a valid origin");
                                return;
                            }

                            origin = baseToDec(argv[++i]);
                            originOverride = true;
                            break;

                        case "-cs":
                        case "-codestart":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid code start location ");
                                return;
                            }

                            codeStart = baseToDec(argv[++i]);
                            codeStartOverride = true;

                            break;

                        case "-fs":
                        case "-filestart":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid file start location ");
                                return;
                            }

                            fileStart = baseToDec(argv[++i]);
                            fileStartOverride = true;
                            break;

                        case "-len":
                        case "-length":
                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid length to read");
                                return;
                            }

                            fileLength = baseToDec(argv[++i]);  // this will be tweaked later
                            lengthOverride = true;
                            break;

                        case "-fe":
                        case "-fileend":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid file end location ");
                                return;
                            }

                            fileEnd = baseToDec(argv[++i]);
                            fileEndOverride = true;
                            break;

                        case "-ce":
                        case "-codeend":

                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid code end location ");
                                return;
                            }

                            //fileLength = baseToDec(argv[++i]); // will NOT be tweaked since lengthOverride isn't enable // do not understand what this line is intended for
                            codeEndOverride = true; // port: wasn't used?
                            codeEnd = baseToDec(argv[++i]); // port: doing this instead
                            break;

                        case "-b":
                        case "-bank":
                            if (!int.TryParse(nextParam, out bankNumber))
                            {
                                outputHelp("Must specify a valid bank number");
                                return;
                            }

                            ++i;
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
                                outputHelp("You must specify a target file");
                                return;
                            }

                            shortname = pathinfo(preg_replace(@"%[^a-zA-Z0-9_\-\. ]%", "", argv[++i]), PATHINFO_FILENAME);

                            //var target = argv[i];
                            //_targetPath = Path.GetDirectoryName(target);
                            //shortname = string.Join("_", Path.GetFileNameWithoutExtension(target).Split(Path.GetInvalidFileNameChars()));
                            break;

                        case "-p":
                        case "-passes":
                            if (!int.TryParse(nextParam, out lastPass))
                            {
                                lastPass = 9;
                                outputHelp($"You must specify a number of passes, using default {lastPass}");
                            }

                            ++i;
                            break;

                        case "-nodetect":
                        case "-d":
                            noDetect = true;
                            break;

                        case "-l":
                        case "-labels":
                            if (nextParam == null || !File.Exists(nextParam))
                            {
                                outputHelp("You must specify a valid file");
                                return;
                            }

                            labelFile = argv[++i];
                            break;


                        case "-cdl":
                            if (nextParam == null || !File.Exists(nextParam))
                            {
                                outputHelp("You must specify a valid file");
                                return;
                            }

                            cdlFilename = argv[++i];
                            break;

                        case "-cdlo":
                        case "-cdloffset":
                            if (nextParam == null)
                            {
                                outputHelp("You must specify a valid offset for the CDL");
                                return;
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
                            mapperNumber = 2;
                            mapperOverride = true;
                            break;

                        case "-m":
                        case "-mapper":

                            mapperOverride = int.TryParse(nextParam, out mapperNumber);
                            if (!mapperOverride)
                            {
                                outputHelp($"Must specify a valid mapper number, defaulting to {mapperNumber}");
                            }

                            usingMapper2 = (mapperNumber == 2);
                            ++i;
                            break;

                        case "-v":
                        case "-vectors":
                            if (nextParam == null)
                            {
                                outputHelp("Must specify a valid vector location ");
                                return;
                            }

                            vectorsFilePos = baseToDec(argv[++i]);
                            vectorsOverride = true;

                            break;

                        case "-xt":
                        case "-trace":

                            trace = true;
                            if (File.Exists(nextParam))
                            {
                                traceFilename = argv[++i];
                            }
                            break;
                    }

                }
                #endregion

                // port: variables that need to be declared outside their scope of first use
                HeaderInfo headerInfo = null;
                var labels = new AsmLabels();
                var fileLabels = new AsmLabels(); // port: labeled addresses read in from file
                var oldPrgLabels = new AsmLabels(); // port: previous-pass prg labels, empty for first pass
                byte newPrg = 0;
                byte oldPrg = 0;
                byte cdlByte = 0;
                bool oldDidDrawLine = false;
                int invalidCounter = 0;
                var theText = new StringBuilder();

                /* port: don't know what this is doing but not using it now
                if (fileEndOverride)
                {
                    fileLength = fileStart + fileEnd;
                    lengthOverride = true;
                }
                */


                var fileSource = File.ReadAllBytes(filename);

                var pass = 1;

                AsmLabels oldLabels = null;

                var initLabels = new AsmLabels();
                if (!noVectors)
                {
                    initLabels.Add(V_NMI, "vectors");
                    initLabels.Add(V_RESET);
                    initLabels.Add(V_IRQ_BRK);
                }

                if (includeReg) // port note: include the NES defined function addresses
                {
                    var registerLabels = new AsmLabels(registers);
                    initLabels.AddRange(registerLabels);
                }

                var labelLen = 0;
                if (labelFile != null) // port note: read in user defined named addresses from file
                {
                    int maxLength;
                    fileLabels = readLabels2(labelFile, out maxLength);

                    labelLen = maxLength - 10;

                    if (labelLen < 0)
                        labelLen = 0;

                    initLabels.AddRange(fileLabels);

                }

                byte[] cdlFile = null;
                if (cdlFilename != null)
                {
                    cdlFile = File.ReadAllBytes(cdlFilename);
                    cdlByte = 0;
                }


                string theOldLabel = "";

                theText.Append(commentHeader(pathinfo(filename, PATHINFO_BASENAME) + " disassembled by DISASM6 v" + VERSION + ", .NET port", false));

                var prgBank = 0;
                var theLabel = "";


                #region pre-loop setup

                // todo userInputs object
                var inputs = new Inputs(); // this will be a mirror of user inputs, resolved to authoritative values

                // establish content bounds within source data   
                inputs.SetFileRange(
                    fileSource, fileStartOverride, fileStart, fileEndOverride, fileEnd, lengthOverride, fileLength);
                // file bounds ready (usually whole .nes or .bin file)
                var file = Util.CopyBytes(fileSource, inputs.fileStart, inputs.fileLength);

                // establish prg location within file data, total size, and bank size
                // port: without a header, prg data will be the specified range of file data;
                if (!ignoreHeader)
                    headerInfo = getHeaderInfo(file);

                inputs.SetPRGRange(file, noDetect, ignoreHeader, headerInfo, mapperOverride, mapperNumber);
                if (headerInfo != null)
                {
                    oldPrg = headerInfo.prg;

                    if (headerInfo.prg * LEN_16K != inputs.prgLen)
                    {
                        newPrg = (byte)(inputs.prgLen / LEN_16K); // TODO if prgLen isn't length 2^x ?
                        if (newPrg == 0)
                            newPrg = 1; // 1 bank minimum
                    }
                }

                // prg bounds and tentative start index are ready
                var prg = Util.CopyBytes(file, inputs.prgOffset, inputs.prgLen);

                // determine where to start in prg based on codestart/end
                inputs.SetCodeRange(prg, originOverride, origin, codeStartOverride, codeStart, codeEndOverride, codeEnd, bankNumber);
                originOverride = inputs.originOverride;
                origin = inputs.origin; // copying back since origin is used outside this method // todo replace references

                //  cdl offset
                if (cdlFile != null && cdlOffset == 0 && inputs.prgStartIndex != 0)
                {
                    if (cdlFile.Length == file.Length || cdlFile.Length == file.Length + HDR_LEN)
                    {
                        // assume file prg and cdl prg data are aligned from byte 0 and use prg start index
                        cdlOffset = inputs.prgStartIndex;
                    }
                    else
                    {
                        echo_line($"Can't determine starting point in CDL file for prg start index: 0x{inputs.prgStartIndex:X5}");
                        echo_line($"Specify offset manually with -cdloffset switch");
                    }
                }

                // should be set for prg loop now
                // origin should be a prg address space value
                // prgStartIndex is a prg address where disassembly starts

                #endregion


                var romBanksInfo = MapBanks(prg, inputs.romBankSize);
                SetBankOrigins(romBanksInfo, prg, cdlFile, cdlOffset - inputs.prgStartIndex); // cdlOffset should align to prgStartIndex. this method starts at prg 0, so backtrack cdlOffset to match
                BankInfo.SetBankVectorsFlag(romBanksInfo, inputs.mapperNumber);

                DisplayRomBanks(romBanksInfo);

                var padBlocks = FindPadBlocks(prg, 0xA0); // todo

                // by default assume vectors always at the end of the last bank (but could be in other banks as well)
                // todo identify any mappers where this isn't true

                var nmi = wordAddr(prg, prg.Length - 6);
                var reset = wordAddr(prg, prg.Length - 4);
                var irq_break = wordAddr(prg, prg.Length - 2);

                // user can identify vectors location manually if needed 
                // todo might cause problems with output
                if (vectorsOverride)
                {
                    nmi = wordAddr(file, vectorsFilePos);
                    reset = wordAddr(file, vectorsFilePos + 2);
                    irq_break = wordAddr(file, vectorsFilePos + 4);
                }


                var prgChunks = new List<byte[]>();
                var cdlChunks = new List<byte[]>();

                if (headerInfo != null && prg.Length > 0x8000) // over 32K, requires banking
                {
                    prgChunks = headerInfo.SlicePrg(prg); // break prg down to bank-sized chunks
                    if (cdlFile != null)
                        cdlChunks = headerInfo.SliceCdl(cdlFile, prgChunks); // break cdl down to match prg slices;
                }
                else
                {
                    prgChunks.Add(prg); // just the 1
                    if (cdlFile != null)
                        cdlChunks.Add(cdlFile);
                }

                // todo list of bank info
                /*

                // for vectors this should be safe for just about everything. 
                // don't know of any roms that would not have vectors in last prg bank regardless of banking scheme. 
                // multi carts or other pirates might swap the entire prg out and reset or something but 
                // those would be tough to process as a whole anyway
                byte[] prgChunk = prgChunks.Last();
                byte[] cdlChunk = null;


                // port: todo loop chunks
                prgChunk = prgChunks[0];
                if (cdlFile != null)
                    cdlChunk = cdlChunks[0];

                // todo this is temp values
                var bankInfo = new BankInfo() { Origin = origin, EndOfBank = V_NMI, };

                List<bool[]> codeMasks = null; //  new List<byte[]>();
                bool[] codeMask = null;

                if (trace)
                {
                    // run through prg slices from known execution points and keep track of what should be considered code

                    var entryPoints = new List<int>();

                    entryPoints.Add(reset);
                    entryPoints.Add(nmi);
                    entryPoints.Add(irq_break);

                    codeMasks = new List<bool[]>();

                    if (!string.IsNullOrWhiteSpace(traceFilename))
                    {
                        // todo // todo figure out what the intent is then implement
                        //var dict = readLabels(inputs.traceFilename);
                    }

                    //  codeMask = TracePrg(slice, bankInfo, entryPoints, sliceCdl);
                    TracePrg(bankInfo, prgChunks, cdlChunks, entryPoints, codeMasks, filename);
                }

                //PrintInputs(inputs, origin, headerInfo);
                codeMask = codeMasks?.FirstOrDefault(); // todo iterate with prg slice
                */

                // ---------------------------------------------------------
                // all settings and input should be processed and ready to use
                // proceed with disassembly

                /*
                // check for 16k roms
                if (!noDetect && headerInfo != null)
                {
                    newPrg = 0;
                    if (headerInfo.prg == 2) // port: 2 x 16K
                    {
                        int pos = fileStart + HDR_LEN; // port: temp file position variable
                        var prg0 = fread(file, 0x4000, ref pos);
                        var prg1 = fread(file, 0x4000, ref pos);

                        if (php_bytes_equal(prg0, prg1) && headerInfo.mapper == 0)
                        {
                            echo("PRG Banks 0 and 1 are identical, overdumped 16K PRG suspected, use -d to disable check\n");
                            // port: this is an overdump and a mess to sort out with the byte arrays. todo
                            return;
                            newPrg = 1;

                            origin = originOverride ? origin : 0xc000;

                            if (cdlFilename != null)
                            {
                                cdlOffset += 0x4000;
                            }
                        }
                    }
                    else if (headerInfo.prg == 1) // port: this would be standard 16K rom
                    {
                        origin = originOverride ? origin : 0xc000;
                    }
                }
                */

                echo("Using Origin: 0x" + hex_pad(origin) + "\n\n");

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

                if (inputs.mapperOverride)
                {
                    echo_line($"Mapper {inputs.mapperNumber} selected");
                }

                if (inputs.usingMapper2 != false)
                {
                    echo("Mapper 2 (UxROM) support enabled\n");
                }

                if (inputs.fileStartOverride)
                {
                    echo("Starting at file location 0x" + hex_pad(inputs.fileStart) + "\n");
                }

                if (inputs.codeStartOverride)
                {

                    echo("Starting at code location $" + hex_pad(inputs.codeStart) + "\n");
                }

                if (inputs.lengthOverride)
                {
                    echo("Reading 0x" + hex_pad(inputs.fileLength) + " bytes\n");

                }

                if (includeChr && headerInfo != null)
                {
                    //echo "Using CHR-ROM\n"; // port: disabled in original
                }

                echo("\n");


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

                    /*
                    counter = origin;
                    // port: counter is location within executing address space. filePos represents the location within the prg data.

                    if (codeStartOverride)
                    {
                        prgStartIndex = fileStart - prgOffset;
                        // port: might be wrong initially, fileStart gets recalculated in the pass 1 only code
                    }
                    else if (fileStartOverride)
                    {
                        // port: calculate the equivalent starting point within the prg block
                        prgStartIndex = fileStart - prgOffset;
                    }


                    if (headerInfo != null)
                    {
                        oldPrg = headerInfo.prg;
                    }

                    */

                    #region pass 1 only
                    // do this stuff only on the first pass
                    if (pass == 1)
                    {
                        oldDidDrawLine = false;
                        oldLabels = labels.AsCopy(); // port: previous-pass general labels
                    }

                    #endregion


                    int filePos = inputs.prgStartIndex; // port: filePos is the working index within the prg block
                    int cdlPos = cdlOffset;

                    // this assumes rom bank size of 16K and uses it in relation to rom header prg count and unrom bank size
                    // TODO needs to be mapper aware, or separate value to track rom bank sized index or something
                    prgBank = 0;
                    // port: prgBank was never reset in original code, prg bank comments didn't get printed

                    // if 16k rom, update prg info
                    if (newPrg != 0)
                    {
                        headerInfo.prg = newPrg; // todo should we be changing the header?
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

                        string headerText = null;
                        if (showHeader && (headerText = processHeaderInfo(headerInfo)) != null)
                        {
                            theText.Append(headerText);
                        }

                        theText.Append(commentHeader("Program Origin"));
                        theText.Append(str_pad(LEFT_MARGIN + ".org $" + hex_pad(origin), 30 + labelLen) + " ; Set program counter\n");
                        theText.Append(commentHeader("ROM Start"));

                    }
                    #endregion

                    // read the file
                    // each pass of this loop completes one line of output

                    var counter = origin; // port: counter is location within executing address space. filePos represents the location within the prg data.
                    echo($"Starting pass {pass} " + (pass == lastPass ? "(final) " : "") + "... ");

                    #region byte loop
                    var currentRomBank = romBanksInfo[0];

                    while (filePos < prg.Length)
                    {
                        if (inputs.codeEndOverride && counter >= inputs.codeEnd)
                            break; // replaces counter < fileLength condition

                        var invalidText = "Invalid Opcode";
                        var didDrawLine = false;

                        if (filePos >= currentRomBank.DataOffset + currentRomBank.BankSize)
                            currentRomBank = SelectRomBank(romBanksInfo, filePos);

                        // handle mapper 2
                        if (NewRomBank(romBanksInfo, counter, inputs.usingMapper2, headerInfo))
                        {
                            prgBank++; // port change: update prgBank value before checking for last bank

                            if (prgBank < (headerInfo.prg - 1)) // port: header.prg-1 is fixed last bank
                                counter = 0x8000; // port: counter changes, so this block doesn't happen when loop restarts

                            if (pass == lastPass)
                            {
                                theText.Append(commentHeader($"PRG Bank {prgBank}"));
                                theText.Append(LEFT_MARGIN + $".base 0x{hex_pad(counter)}\n");
                                theText.Append(commentLine());
                            }
                            if (counter == 0x8000) // port: make sure counter has moved before restarting
                                continue;
                        }

                        // handle vectors

                        if (pass < lastPass && counter == V_NMI && !noVectors /*&& currentRomBank.HasVectors*/)
                        {
                            // todo if remaining bytes match vectors, then add labels
                            // eg for mmc1 where all banks should have the vectors in the same place

                            /* port: this assumes the prg bank being processed is the last bank. 
                             * use predetermined values from above instead
                            nmi = wordStr(fread(prgChunk, 2, ref filePos));
                            reset = wordStr(fread(prgChunk, 2, ref filePos));
                            irq_break = wordStr(fread(prgChunk, 2, ref filePos)); 
                            */
                            filePos += 6;
                            cdlPos += 6;

                            addVector(nmi, "nmi", labels);
                            addVector(reset, "reset", labels);
                            addVector(irq_break, "irq", labels);

                            prgLabels.Add(nmi);
                            prgLabels.Add(reset);
                            prgLabels.Add(irq_break);

                            counter += 6;
                            continue;
                        }
                        else if (pass == lastPass && counter == V_NMI && !noVectors /*&& currentRomBank.HasVectors*/)
                        {
                            theText.Append(processVectors(hex_pad(nmi), hex_pad(reset), hex_pad(irq_break)));
                            filePos += 6;
                            cdlPos += 6;

                            counter += 6;

                            continue;
                        }

                        //read opcode
                        var opcode = prg[filePos];
                        var opinfo = opcodes.First(n => n.Code == opcode);

                        var isInvalid = opinfo.Legal; // [0];
                        var mnemonic = opinfo.Text; // [1];
                        var byteLen = opinfo.Bytes; // [2];
                        var addressingType = opinfo.AddrMode; //[4];

                        var isDataByte = false;
                        var dataStr = "Suspected data";

                        if (cdlFilename == null)
                        {
                            theOldLabel = "";  // Reset 'theOldLabel' when we are no longer in a known data byte
                        }

                        // check code/data log - if data, don't process as an opcode
                        if (cdlFilename != null)
                        {
                            var newCdlByte = cdlFile[cdlPos];

                            // draw line between data and code
                            if (pass == lastPass
                                && DrawDataCodeSeparator(oldDidDrawLine, counter, newCdlByte, cdlByte)
                                )
                            {
                                theText.Append("\n" + commentLine());

                                didDrawLine = true;
                            }

                            /* port: todo what was i doing here? manually identifying code bytes?
                            if (codeMask != null)
                            {
                                if (codeMask[filePos])
                                {
                                    newCdlByte |= CDL_CODE;

                                    //newCdlByte |= CDL_DATA;
                                    //newCdlByte = (byte)( newCdlByte ^ CDL_DATA );
                                }
                                //else if (newCdlByte == 0) // todo
                                //{
                                //    newCdlByte |= CDL_DATA;
                                //}
                            }
                            */

                            // check if the CDL byte is known, if known, copy, otherwise do some checks
                            var dechex_pad_counter = hex_pad(counter);
                            if (newCdlByte != 0)
                            {
                                cdlByte = newCdlByte;
                            }
                            // if byte is zero and we're at a program label, assume code
                            //else if (isset(oldPrgLabels, dechex_pad_counter))
                            else if (oldPrgLabels.ContainsKey(counter))
                            {
                                cdlByte = CDL_CODE; // port: bindec('00000001')
                            }
                            // if byte is zero and we're at a label, but not program, assume data (only on 2nd pass)
                            else if (oldLabels.ContainsKey(counter) && pass > 1)
                            {
                                cdlByte = CDL_DATA; // port: bindec('00000010')
                            }
                            // else assume program code


                            // data byte
                            if ((cdlByte & CDL_DATA) != 0 && (cdlByte & CDL_CODE) == 0)
                            {
                                if (isCounterLabel(counter, oldLabels))
                                {
                                    //theOldLabel = ToLabel(counter);

                                    //if (oldLabels[counter].Count > 0)
                                    //{
                                    //    theOldLabel = oldLabels[counter][0]; // todo should be fine?
                                    //}

                                    theOldLabel = FirstLabel(oldLabels[counter], (counter));
                                }

                                // port: JumpTable/RTSTable don't appear anywhere else in the original php
                                // these would have to come from the labels file?
                                if (theOldLabel.EndsWith("JumpTable"))
                                {
                                    byteLen = 2;
                                    mnemonic = ".word";
                                    addressingType = 11;
                                    isInvalid = 0;
                                }
                                else if (theOldLabel.EndsWith("RTSTable"))
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
                                    // generic .hex ff ... statement 
                                    byteLen = 4;
                                    mnemonic = "";
                                    addressingType = -1;
                                    isInvalid = 1;
                                }
                                isDataByte = true;
                                dataStr = "Data";
                            }
                        }
                        //else
                        //{
                        //    theOldLabel = "";  // Reset 'theOldLabel' when we are no longer in a known data byte
                        //}


                        var readBytes = byteLen - 1;
                        //var bytes = ""; // port: now declared where it's used below
                        var byteStr = "";
                        var trailer = "";
                        var hextext = hex_pad(opcode);

                        var byteArr = new[] { hextext }.ToList();


                        /* better handled when branch command is checked, probably won't need this
                        if (readBytes > 0 && filePos + 1 < prgChunk.Length)
                        {
                            // port: added an operand check for branch into self scenarios
                            // original code could generate a branch instruction with a non-existent label
                            byte operand = 0; // port: 1 byte of operand, for use with branch checks
                                operand = prgChunk[filePos + 1];
                                if (operand == 0xff && _branches.Contains(mnemonic)) // port: branch into self check
                                {
                                    invalidCounter = 0; // port note: set but never used
                                    readBytes = 0;
                                    isInvalid = 1;
                                    byteLen = 1;
                                    addressingType = -1;
                                }
                        }
                        */

                        // read 1 or 2 byte parameters for the opcode
                        if (readBytes > 0)
                        {
                            // check to see if a label exists in this opcode.. if so then usually it's data
                            for (var i = 1; i <= readBytes; i++)
                            {
                                bool lastPrgBank = headerInfo != null && prgBank == headerInfo.prg - 1;
                                if (HasConflict(counter, filePos, i, inputs.prgLen, inputs.mapperNumber, lastPrgBank, noVectors, oldLabels))
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
                                    var newCdlByte = cdlFile[cdlPos + i];
                                    //var didMoveCdlPtr = true; // port: no longer relevant
                                    if ((newCdlByte & CDL_CODE) != 0)
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


                            if (readBytes > 0) // if readbytes is still > 0 after above
                            {
                                var tmp = filePos + 1; // port: todo sort out file position 
                                var bytes = fread(prg, readBytes, ref tmp);

                                for (var j = 0; j < readBytes; j++)
                                {
                                    byteArr.Add(hex_pad(bytes[j]));
                                    //hextext += ' ' + byteArr[j + 1]; // port: this happens all at once after the loop now
                                }

                                int offset = 1;
                                if (addressingType == TBL_JP || addressingType == TBL_RTS)
                                {
                                    offset = 0;
                                }

                                byteStr = wordStr(byteArr, offset);

                                if (addressingType == TBL_JP)
                                {
                                    byteStr = hex_pad(hexdec(byteStr) + 1);
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

                        int byteStrNum = hexdec(byteStr);

                        if (addressingType > 0
                            && isValidLabel(byteStrNum)
                            && !(ignoreWrites && mnemonic.StartsWith("ST") && byteStrNum < 0x8000)) // do not add labels when writing to PRG 
                        {
                            lbl = "__";

                            if (pass < lastPass && isInvalid != 1)
                            {
                                addValidLabel(byteStrNum, labels);
                            }
                        }

                        var newByteStr = lbl + byteStr;

                        if (oldLabels.ContainsKey(byteStrNum))
                        {
                            var list = oldLabels[byteStrNum];

                            if (list.Count > 0)
                                newByteStr = list[0];
                        }

                        // lets check for various addressing types to figure out how to format the text
                        switch (addressingType)
                        {

                            case 0: // Implicit/Accumulator/Immediate
                                if (readBytes > 0)
                                    byteStr = "#$" + byteStr;
                                else
                                    byteStr = string.Empty;

                                break;

                            case TBL_JP: // port: jump table
                            case TBL_RTS: // port: rts table
                            case JP: // port: jsr, jmp
                                if (isInvalid != 1)
                                {
                                    addValidLabel(hexdec(byteStr), prgLabels);
                                }

                                byteStr = newByteStr;

                                if (addressingType == 12)
                                {
                                    byteStr += "-1";
                                }

                                break; // port note: original code had fall through to 1,4 case. properly separated now.
                            case ABS: // Absolute
                            case ZP: // Zero Page
                                byteStr = newByteStr; // port: shared line in original code
                                break;

                            case ABS_X: // Absolute X
                            case ZP_X: // Zero Page X
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
                                      // don't do anything
                                break;

                            default: // port: added to help with troubleshooting
                                Console.WriteLine(addressingType);
                                break;
                        }

                        // now lets cover specific mnemonics
                        bool drawTrailer = false;

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

                                // port: making use of isInvalidBranch for branch into self
                                // possibly legal but creates label conflicts
                                if (filePos + 1 < prg.Length && prg[filePos + 1] == 0xff)
                                    isInvalidBranch = true;

                                // todo check for label at destination addr? at least from user defined labels
                                // todo need to know how many bytes at label that would be cut into

                                if (pass < lastPass && isInvalid != 1 && !isInvalidBranch)
                                {
                                    addValidLabel(addr, labels);
                                    addValidLabel(addr, prgLabels);
                                }

                                if (!isInvalidBranch && isValidLabel(addr))
                                {
                                    byteStr = ToLabel(addr);

                                    if (labels.ContainsKey(addr))
                                    {
                                        var list = new List<string>(labels[addr]);

                                        if (list.Count == 0)
                                            list.Add(ToLabel(addr));

                                        byteStr = list[0]; // this should be fine? only need one reference?
                                    }
                                }
                                else
                                {
                                    isInvalid = 1;
                                    invalidText = "Illegal Branch";

                                    if (isInvalidBranch)
                                        invalidText = "Branch into self";
                                }

                                break;

                            // add some space after RTS/JMP
                            case "RTS":
                            case "RTI":
                            case "JMP":
                                if (isInvalid != 1)
                                {
                                    trailer = "\n" + commentLine();
                                    drawTrailer = true;
                                    didDrawLine = true;
                                }
                                break;

                        }

                        #region output
                        // only deal with output on last pass
                        if (pass == lastPass)
                        {
                            hextext = string.Join(" ", byteArr); // port: this is where the disabled line above is reimplemented

                            string oldMnemonicStr = dataStr;
                            if (isInvalid == 1)
                            {
                                if (addressingType != -1)
                                    oldMnemonicStr = (invalidText + " - " + mnemonic + " " + byteStr);

                                mnemonic = ".hex";
                                byteStr = hextext;
                            }

                            if (!oldLabels.ContainsKey(counter))
                            {
                                theText.Append(LEFT_MARGIN);
                            }

                            if (oldLabels.ContainsKey(counter))
                            {
                                var tmpList = new List<string>(oldLabels[counter]);
                                if (tmpList.Count == 0)
                                {
                                    // generate generic label so there's at least 1 in the list
                                    tmpList.Add(ToLabel(counter));
                                }

                                var labelsSub = new StringBuilder(); // port: for gathering address labels together before outputting, to prevent interruptions
                                for (var i = 0; i < tmpList.Count; i++)
                                {
                                    theLabel = tmpList[i];

                                    // if label has a + or - in it but doesn't start with one
                                    // then don't show it
                                    // if not 0 or false
                                    if (strpos(theLabel, '+') > 0 || strpos(theLabel, '-') > 0)
                                    {
                                        theText.Append(LEFT_MARGIN);
                                        continue;
                                    }

                                    bool drawn = oldDidDrawLine || didDrawLine; // port: refactoring to simplify
                                    bool lastComment = i == tmpList.Count - 1;

                                    switch (theLabel)
                                    {
                                        case "irq":
                                            theText.Append(commentHeader("irq/brk vector", !drawn, !drawn, lastComment));
                                            //didDrawLine = true; // port: todo?
                                            break;

                                        case "nmi":
                                        case "reset":
                                            theText.Append(commentHeader($"{theLabel} vector", !drawn, !drawn, lastComment));
                                            //didDrawLine = true; // port: todo?
                                            break;

                                    }

                                    if (strlen(theLabel) >= marginLen - 1)
                                    {
                                        //if (!drawn && counter != origin) // is this equivalent?
                                        if (!(drawn || counter == origin))
                                            labelsSub.AppendLine();

                                        labelsSub.AppendLine($"{theLabel}:");

                                        // port note if label is too long then put content on next line // todo actually doing this somewhere?

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
                            /* todo remove
                            else
                            {
                                theText.Append(LEFT_MARGIN);
                            }
                            */


                            var mnem = useLowerCase ? strtolower(mnemonic) : mnemonic;

                            var width = 30 - marginLen + labelLen;
                            var line = mnem + " " + byteStr;
                            line = str_pad(line, width);

                            var line2 = new StringBuilder();
                            line2.Append($"{mnem} {byteStr}".PadRight(width));

                            width = (isDataByte ? 54 : 50) - marginLen + labelLen;
                            line += " ; $" + hex_pad(counter) + ": " + hextext; // port: this is the post-instruction comment
                            line = str_pad(line, width);

                            line2.Append($" ; ${hex_pad(counter)}: {hextext}".PadRight(width - line2.Length)); // fixed too much padding

                            if (isInvalid == 1)
                            {
                                line += oldMnemonicStr;
                                line2.Append(oldMnemonicStr);
                            }

                            //if (drawTrailer) // port todo?
                            line += "\n" + trailer;
                            line2.AppendLine();
                            line2.Append(trailer);

                            //theText.Append(line);
                            theText.Append(line2);

                        }
                        #endregion

                        filePos += byteLen; // port: move index to next opcode
                        cdlPos += byteLen;
                        counter += byteLen;
                        oldDidDrawLine = didDrawLine;
                    }  // end line by line loop

                    #endregion

                    // port note: if no change in labels this pass, lastPass truncated to skip any redundant passes
                    if (pass < lastPass && oldLabels != null && php_dictionaries_equal(labels, oldLabels))
                    {
                        lastPass = pass + 1;
                    }

                    if (pass < lastPass)
                    {
                        oldLabels = labels.AsCopy();
                        oldPrgLabels = prgLabels.AsCopy();
                    }

                    echo("complete\n");
                    pass++;
                }
                #endregion


                if (includeChr && headerInfo != null)
                {
                    byte[] chr = null;

                    if (headerInfo.chr > 0)
                    {
                        chr = new byte[headerInfo.chr * 0x2000]; // banks x 8K  
                        Array.Copy(file, inputs.prgOffset + prg.Length, chr, 0, chr.Length);
                    }

                    // port: chr already loaded in entirety above

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
                    if (inputs.ignoreHeader)
                    {
                        echo("\nTry disabling -ignoreheader if you wish to export CHR-ROM data"); // port: original did not explicitly echo, assuming it was intended to do so
                    }
                }

                //file_put_contents(shortname + ".asm", theText);
                file_put_contents($"{shortname}.da6.asm", theText);

                var time_end = microtime(true);
                var time = round(time_end - time_start, 3);

                echo($"\nDisassembly {shortname}.da6.asm generated in {time} seconds\n\n");
            }




            //public string ToLabel(int address, int bank = -1)
            //{
            //    var b = string.Empty;
            //    if (bank > -1)
            //        b = $"_{hex_pad(bank)}";

            //    return $"_{b}_{hex_pad(address)}";
            //}


            /// <summary>
            /// choose the rom bank info containing the file position
            /// </summary>
            /// <param name="romBanksInfo"></param>
            /// <param name="filePos"></param>
            /// <returns></returns>
            private BankInfo SelectRomBank(List<BankInfo> romBanksInfo, int filePos)
            {
                var bank = romBanksInfo.FirstOrDefault(
                    n => n.DataOffset <= filePos && filePos < n.DataOffset + n.BankSize);
                return bank;
            }

            /// <summary>
            /// print info to screen
            /// </summary>
            /// <param name="romBanksInfo"></param>
            private void DisplayRomBanks(List<BankInfo> romBanksInfo)
            {
                echo_line("banks:");
                foreach (var rbi in romBanksInfo)
                    echo_line(rbi.ToString());
                echo_line();
            }

            /// <summary>
            /// look for runs of bytes that can be replaced with a .pad instruction
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="minLength"></param>
            /// <returns></returns>
            private List<int[]> FindPadBlocks(byte[] buffer, int minLength = 0x20)
            {
                var padValues = new[] { 0x00, 0xff }; // byte values that can be used to create pads (of the same byte)
                byte dummy = 0xa9; // dummy value to prevent false matches to start new sequence

                var found = new List<int[]>(); // start (incl), end (excl), pad byte
                int currentStart = -1;
                byte currentVal = dummy;

                for (int j = 0; j < buffer.Length; j++)
                {
                    byte b = buffer[j];

                    if (currentStart != -1)
                    {
                        // in sequence

                        if (b == currentVal)
                        {
                            // sequence extends
                            continue;
                        }

                        // sequence ended
                        int len = j - currentStart;
                        if (len >= minLength)
                        {
                            // valid for pad command
                            found.Add(new[] { origin + currentStart, origin + j, currentVal });
                        }

                        currentStart = -1;
                        currentVal = dummy;
                    }

                    if (padValues.Contains(b))
                    {
                        // new sequence
                        currentStart = j;
                        currentVal = b;
                    }
                }

                return found;
            }

            /// <summary>
            /// check for op's data bytes crossing any kind of barrier that would suggest an invalid op
            /// </summary>
            /// <param name="counter">counter postion of op</param>
            /// <param name="filePos">index location of op within file</param>
            /// <param name="offset">number of bytes post-counter/filePos to op parameter byte</param>
            /// <param name="prgLen">total prg length</param>
            /// <param name="mapper">mapper number</param>
            /// <param name="lastPrgBank">true if currently in the last rom bank</param>
            /// <param name="oldLabels"></param>
            /// <returns></returns>
            private bool HasConflict(int counter, int filePos, int offset, int prgLen, int mapper, bool lastPrgBank, bool noVectors, AsmLabels oldLabels)
            {
                int counter_i = counter + offset;
                int filePos_i = filePos + offset;

                var counterMax = V_NMI; // counter runs into vectors
                if (noVectors)
                {
                    counterMax = 0x10000; // counter runs out of range
                }

                return (
                    filePos_i >= prgLen // port: added prg overrun check, original code dipped into chr space
                    || counter_i >= counterMax
                    //|| (counter_i >= fileLength) // counter exceeds something? // TODO fileLength is wrong
                    || isCounterLabel(counter_i, oldLabels) // existing label at counter
                    || (mapper != 0 && mapper != 3 && counter_i > 0xBFFF && !lastPrgBank) // TODO this is checking for counter running into last bank, assuming it's fixed, modify it for general banking/variable bank size
                    );
            }

            /// <summary>
            /// determine if a separation line needs to printed to the output asm
            /// </summary>
            /// <param name="oldDidDrawLine"></param>
            /// <param name="counter"></param>
            /// <param name="newCdlByte"></param>
            /// <param name="cdlByte"></param>
            /// <returns></returns>
            private bool DrawDataCodeSeparator(bool oldDidDrawLine, int counter, byte newCdlByte, byte cdlByte)
            {
                return (
                    !oldDidDrawLine
                    && counter != origin
                    && newCdlByte != 0
                    && ((newCdlByte & CDL_CODE) != (cdlByte & CDL_CODE)) // port: change from data to code or vice versa //todo indirect?
                    );
            }

            /// <summary>
            /// determine if counter has incremented into a different rom bank
            /// </summary>
            /// <param name="romBanksInfo"></param>
            /// <param name="counter"></param>
            /// <param name="usingMapper2"></param>
            /// <param name="headerInfo"></param>
            /// <returns></returns>
            private bool NewRomBank(List<BankInfo> romBanksInfo, int counter, bool usingMapper2, HeaderInfo headerInfo)
            {
                return (
                    usingMapper2
                    && headerInfo != null
                    && headerInfo.mapper == 2
                    && counter == 0xC000
                    //&& prgBank < (headerInfo.prg - 1) // port: relocated below
                    );
            }

            private static List<BankInfo> MapBanks(byte[] prg, int romBankSize)
            {
                var bankList = new List<BankInfo>();
                int romBanks = prg.Length / romBankSize; // chunks of data as managed by mapper

                for (int j = 0; j < romBanks; j++)
                {
                    var bi = new BankInfo();
                    bi.Index = j;
                    bi.BankSize = romBankSize;
                    bankList.Add(bi);
                }

                return bankList;
            }

            public static void SetBankOrigins(List<BankInfo> bankList, byte[] prg, byte[] cdl, int cdlOffset)
            {
                if (cdl != null && cdlOffset != 0)
                {
                    var cdlTmp = Util.CopyBytes(cdl, cdlOffset, prg.Length);
                    cdl = cdlTmp;
                }

                int romBanks = bankList.Count;
                var romBankSize = bankList[0].BankSize;

                if (romBankSize == LEN_32K || romBankSize == LEN_16K)
                {
                    // 32K: everything starts at $8000
                    // 16K: same, except last bank at $C000 (set below)
                    bankList.ForEach(n => n.Origin = CPU_ADDR_BASE);
                }
                else if (romBankSize == LEN_8K && cdl != null)
                {
                    // $8000
                    // $A000
                    // $C000
                    // $E000

                    // assume last bank at $E000, everything else tbd
                    for (int j = 0; j < bankList.Count; j++)
                    {
                        var info = bankList[j];

                        var cpuBank = IdentifyCPUBank(cdl, info.DataOffset, info.BankSize);

                        int address = 0x0000; // zero indicates no bank information available
                        if (cpuBank > -1) // one of the 4 prg banks in the $8000-FFFF space
                        {
                            address = 0x8000 + (cpuBank * CPU_BANK_LEN);
                        }

                        //int subAddr = address % 0x2000;
                        //address += subAddr; // for rom banks less than 8K // todo this probably doesn't happen

                        if (address > 0)
                            info.Origin = address;

                        //Console.WriteLine($"rom bank: {j,3} cpu bank: {cpuBank,3} ${address:X4}");
                    }
                }

                var last = bankList[bankList.Count - 1];
                last.Origin = (CPU_ADDR_BASE + LEN_32K) - romBankSize; // set last bank origin so vectors come out right
            }

            /*
            public static List<BankInfo> GetBankOrigins(byte[] prg, byte[] cdl, int romBankSize = LEN_8K)
            {
                // rom bank size is 8K default
                // actual banks can be more (or less?) depending on mapper

                var bankList = new List<BankInfo>();
                int romBanks = prg.Length / romBankSize; // chunks of data as managed by mapper
                //int slices = prg.Length / CPU_BANK_LEN; // chunks of data executing from (ideally) the same 8K address space

                int cpuBank = -1;

                for (int j = 0; j < romBanks; j++)
                {
                    var bi = new BankInfo();
                    bi.Index = j;
                    bi.BankSize = romBankSize;
                    bankList.Add(bi);

                    int nextOffset = (j + 1) * romBankSize;
                    int chunkSize = Math.Min(CPU_BANK_LEN, romBankSize);
                }

                if (romBankSize == LEN_32K || romBankSize == LEN_16K)
                {
                    // 32K: everything starts at $8000
                    // 16K: same, except last bank at $C000 (set below)
                    bankList.ForEach(n => n.Origin = 0x8000);
                }
                else if (romBankSize == LEN_8K && cdl != null)
                {
                    // assume last bank at $E000, everything else tbd
                    for (int j = 0; j < bankList.Count; j++)
                    {

                        cpuBank = IdentifyCPUBank(cdl, bankList[j].DataOffset, romBankSize);

                        int address = 0x0000; // zero indicates no bank information available
                        if (cpuBank > -1) // one of the 4 prg banks in the $8000-FFFF space
                        {
                            address = 0x8000 + (cpuBank * CPU_BANK_LEN);
                        }

                        //int subAddr = address % 0x2000;
                        //address += subAddr; // for rom banks less than 8K // todo this probably doesn't happen

                        bankList[j].Origin = address;

                        //Console.WriteLine($"rom bank: {j,3} cpu bank: {cpuBank,3} ${address:X4}");
                    }
                }

                bankList[bankList.Count - 1].Origin = 0x10000 - romBankSize; // set last bank origin so vectors come out right

                for (int j = 0; j < bankList.Count; j++)
                {
                    Console.WriteLine($"rom bank: 0x{bankList[j].DataOffset:X5} ${bankList[j].Origin:x4}");
                }

                return bankList;
            }
            */

            /// <summary>
            /// searches cdl data for the memory address of any code executed within the rom bank
            /// </summary>
            /// <param name="cdl"></param>
            /// <param name="bankSize"></param>
            /// <param name="offset"></param>
            /// <returns></returns>
            private static int IdentifyCPUBank(byte[] cdl, int offset, int romBankSize)
            {
                int cpuBank = -1;

                for (int j = 0; j < romBankSize; j++)
                {
                    if (j >= cdl.Length)
                        break;

                    var byteCDL = cdl[offset + j];
                    if (byteCDL == 0)
                        continue;

                    int cdlBank = (byteCDL & CDL_BANK_MASK) >> 2; // bank mask = b1100

                    if (cpuBank != -1 && cpuBank != cdlBank)
                    {
                        Console.Write("cpu bank mismatch");
                    }
                    else if (cpuBank == -1)
                    {
                        cpuBank = cdlBank;
                        break; // disable to allow mismatch checks
                    }
                }

                return cpuBank;
            }
        }
    }
}
