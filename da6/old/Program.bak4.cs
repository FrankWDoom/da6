using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace da6
{

    // https://forums.nesdev.com/viewtopic.php?t=7466

    // label = $0000 
    
    class Program
    {

        static void Main(string[] argv)
        {


            int fnIndex = 0;

            if (argv.Length < fnIndex + 1)
            {
                outputHelp();
                return;
            }

            var filename = argv[fnIndex];
            if (!File.Exists(filename))
            {
                outputHelp("File not found\n");
                return;
            }

            var inputs = new Inputs() { filename = filename };
            inputs.shortname = pathinfo(filename, PhpPathInfo.PATHINFO_FILENAME);


            ////$origin = 0x8000; // global
            //bool showHeader = true;
            //bool includeChr = false;
            //bool includeReg = false;
            //bool originOverride = false;
            //bool noDetect = false;
            //string shortname = pathinfo(filename, PhpPathInfo.PATHINFO_FILENAME);
            //string labelFile = null;
            //string cdlFilename = null;
            //bool ignoreHeader = false;
            //int fileStart = 0;
            //bool fileStartOverride = false;
            //int fileLength = 0x10000;
            //bool lengthOverride = false;
            //int fileEnd = 0;
            //bool fileEndOverride = false;
            //int codeStart = 0;
            //bool codeStartOverride = false;
            //int codeEnd = 0;
            //bool codeEndOverride = false;
            //int cdlOffset = 0;
            //bool ignoreWrites = false;
            //bool useLowerCase = true;
            //bool usingMapper2 = false;

            int lastPass = 9;

            // check command line params
            for (var i = fnIndex + 1; i < argv.Length; i++)
            {
                string nextParam = null;

                if ((i + 1) < argv.Length && substr(argv[i + 1], 0, 1) != "-")
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
                        }

                        origin = baseToDec(argv[++i]);
                        inputs.originOverride = true;
                        break;

                    case "-cs":
                    case "-codestart":

                        if (nextParam == null)
                        {
                            outputHelp("Must specify a valid code start location ");
                        }

                        inputs.codeStart = baseToDec(argv[++i]);
                        inputs.codeStartOverride = true;

                        break;

                    case "-fs":
                    case "-filestart":

                        if (nextParam == null)
                        {
                            outputHelp("Must specify a valid file start location ");
                        }

                        inputs.fileStart = baseToDec(argv[++i]);
                        inputs.fileStartOverride = true;
                        break;

                    case "-len":
                    case "-length":
                        if (nextParam == null)
                        {
                            outputHelp("Must specify a valid length to read");
                        }

                        inputs.fileLength = baseToDec(argv[++i]);  // this will be tweaked later
                        inputs.lengthOverride = true;
                        break;

                    case "-fe":
                    case "-fileend":

                        if (nextParam == null)
                        {
                            outputHelp("Must specify a valid file end location ");
                        }

                        inputs.fileEnd = baseToDec(argv[++i]);
                        inputs.fileEndOverride = true;
                        break;

                    case "-ce":
                    case "-codeend":

                        if (nextParam == null)
                        {
                            outputHelp("Must specify a valid code end location ");
                        }

                        inputs.fileLength = baseToDec(argv[++i]); // will NOT be tweaked since lengthOverride isn't enable
                        inputs.codeEndOverride = true; // todo never used
                        break;


                    case "-h":
                    case "-noheader":
                        inputs.showHeader = false;
                        break;

                    case "-i":
                    case "-ignoreheader":
                        inputs.ignoreHeader = true;
                        break;

                    case "-c":
                    case "-chr":
                        inputs.includeChr = true;
                        break;

                    case "-r":
                    case "-registers":
                        inputs.includeReg = true;
                        break;

                    case "-t":
                    case "-target":
                        if (nextParam == null)
                        {
                            outputHelp("You must specify a target file");
                        }

                        var path = Regex.Replace(argv[++i], @"[^a-zA-Z0-9_\-\. ]", "");
                        inputs.shortname = pathinfo(path, PhpPathInfo.PATHINFO_FILENAME);

                        break;

                    case "-p":
                    case "-passes":
                        if (nextParam == null || !int.TryParse(nextParam, out lastPass))
                        {
                            outputHelp("You must specify a number of passes");
                        }

                        //lastPass = (int)argv[++i];
                        inputs.lastPass = lastPass;
                        break;

                    case "-nodetect":
                    case "-d":
                        inputs.noDetect = true;
                        break;

                    case "-l":
                    case "-labels":
                        if (nextParam == null || !File.Exists(nextParam))
                        {
                            outputHelp("You must specify a valid file");
                        }

                        inputs.labelFile = argv[++i];
                        break;


                    case "-cdl":
                        if (nextParam == null || !File.Exists(nextParam))
                        {
                            outputHelp("You must specify a valid file");
                        }

                        inputs.cdlFilename = argv[++i];
                        break;

                    case "-cdlo":
                    case "-cdloffset":
                        if (nextParam == null)
                        {
                            outputHelp("You must specify a valid offset for the CDL");
                        }

                        inputs.cdlOffset = baseToDec(argv[++i]);
                        break;


                    case "-lc":
                    case "-lowercase":
                        inputs.useLowerCase = true;

                        break;

                    case "-cc":
                    case "-uppercase":
                        inputs.useLowerCase = false;

                        break;

                    case "-iw":
                    case "-ignorewrites":

                        inputs.ignoreWrites = true;
                        break;

                    case "-m2":
                    case "-mapper2":

                        inputs.usingMapper2 = true;
                        break;
                }

            }

            Run(inputs);
        }


        #region php shims

        /// php shim functions -------------------


        // bindec ( string $binary_string ) : number
        public static int bindec(string binary_string)
        {
            var s = binary_string.TrimStart('%');
            int i = Convert.ToInt32(s, 2);
            return i;
        }


        // decbin ( int $number ) : string
        public static string decbin(int number)
        {
            return Convert.ToString(number, 2);
        }

        // dechex ( int $number ) : string
        public static string dechex(int number)
        {
            return number.ToString("x");
        }

        public static void echo(string s)
        {
            Console.Write(s);
        }

        // feof ( resource $handle ) : bool
        public static bool feof(FileStream handle)
        {
            return handle.Position >= handle.Length;
        }

        // file_get_contents ( string $filename [, bool $use_include_path = FALSE [, resource $context [, int $offset = 0 [, int $maxlen ]]]] ) : string
        public static string file_get_contents(string filename /* [, bool $use_include_path = FALSE[, resource $context[, int $offset = 0[, int $maxlen]]]] */ )
        {
            return System.IO.File.ReadAllText(filename);
        }

        // file_put_contents ( string $filename , mixed $data [, int $flags = 0 [, resource $context ]] ) : int
        public static int file_put_contents(string filename, string data /* , int flags = 0 , resource context */ )
        {
            try
            {
                System.IO.File.WriteAllText(filename, data);
                return data.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        // fread ( resource $handle , int $length ) : string
        public static byte[] fread(System.IO.FileStream handle, int length)
        {
            var pos = handle.Position;

            var b = new byte[length];
            handle.Read(b, 0, length);
            fseek(handle, pos + length);
            return b;
        }

        // fseek ( resource $handle , int $offset [, int $whence = SEEK_SET ] ) : int
        public static int fseek(System.IO.FileStream handle, long offset /* , int whence = SEEK_SET */ )
        {
            handle.Position = offset;
            return 0;
        }

        // hexdec ( string $hex_string ) : number
        public static int hexdec(string hex_string)
        {
            if (string.IsNullOrWhiteSpace(hex_string))
                return 0;

            //if (string.IsNullOrWhiteSpace(hex_string))
            //    throw new ArgumentException("hex_string");

            int i = 0;

            try
            {
                i = int.Parse(hex_string.Replace("$", ""), System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
            }

            return i;
        }

        // in_array ( mixed $needle , array $haystack [, bool $strict = FALSE ] ) : bool
        public static bool in_array(object needle, Array haystack, bool strict = false)
        {

            foreach (var item in haystack)
            {
                if (Equals(item, needle))
                {
                    return true;
                }
            }

            return false;
        }

        // is_array ( mixed $var ) : bool
        public static bool is_array(object va)
        {
            return va is Array;
        }

        // isset ( mixed $var [, mixed $... ] ) : bool
        public static bool isset(Dictionary<object, object> d, object key)
        {
            // todo generic dictionary

            return d.ContainsKey(key) && d[key] != null;
        }

        // microtime ([ bool $get_as_float = FALSE ] ) : mixed
        public static DateTime microtime(bool get_as_float = false)
        {
            return DateTime.Now;
        }

        // ord ( string $string ) : int
        public static byte ord(string str)
        {
            return (byte)str[0];
        }

        public static byte ord(char str)
        {
            return (byte)str;
        }


        [Flags]
        public enum PhpPathInfo
        {
            PATHINFO_DIRNAME = 1,
            PATHINFO_BASENAME,
            PATHINFO_EXTENSION,
            PATHINFO_FILENAME,
        }

        // pathinfo ( string $path [, int $options = PATHINFO_DIRNAME | PATHINFO_BASENAME | PATHINFO_EXTENSION | PATHINFO_FILENAME ] ) : mixed

        public static string pathinfo(string path, PhpPathInfo options = PhpPathInfo.PATHINFO_DIRNAME | PhpPathInfo.PATHINFO_BASENAME | PhpPathInfo.PATHINFO_EXTENSION | PhpPathInfo.PATHINFO_FILENAME)
        {

            switch (options)
            {
                case PhpPathInfo.PATHINFO_DIRNAME:
                    return System.IO.Path.GetDirectoryName(path);

                case PhpPathInfo.PATHINFO_BASENAME: // filename w/ ext
                    return System.IO.Path.GetFileName(path);

                case PhpPathInfo.PATHINFO_EXTENSION:
                    return System.IO.Path.GetExtension(path);

                case PhpPathInfo.PATHINFO_FILENAME: // filename no ext
                    return System.IO.Path.GetFileNameWithoutExtension(path);

            }

            throw new ArgumentException("optoins");
        }


        // strlen ( string $string ) : int

        public static int strlen(string str)
        {
            return str.Length;
        }

        public enum PhpStrPadType
        {
            STR_PAD_RIGHT,
            STR_PAD_LEFT,
            STR_PAD_BOTH,
        }

        // str_pad ( string $input , int $pad_length [, string $pad_string = " " [, int $pad_type = STR_PAD_RIGHT ]] ) : string
        public static string str_pad(string input, int pad_length, char pad_string = ' ', PhpStrPadType pad_type = PhpStrPadType.STR_PAD_RIGHT)
        {
            if (pad_length <= input.Length)
                return input;

            switch (pad_type)
            {
                case PhpStrPadType.STR_PAD_RIGHT:
                    return input.PadRight(pad_length, pad_string);

                case PhpStrPadType.STR_PAD_LEFT:
                    return input.PadLeft(pad_length, pad_string);

                case PhpStrPadType.STR_PAD_BOTH:

                    // unequal padding favors right 

                    int pad = pad_length - input.Length;

                    // ? maybe reverse left/right order if not coming out right
                    //return str_pad(
                    //    str_pad(input, input.Length + (pad / 2), pad_string, PadType.STR_PAD_LEFT),
                    //    pad_length, pad_string, PadType.STR_PAD_RIGHT);

                    return input.PadLeft(input.Length + (pad / 2), pad_string).PadRight(pad_length, pad_string);
            }

            throw new ArgumentException("pad_type");
        }

        public static string str_pad(string input, int pad_length, string pad_string, PhpStrPadType pad_type = PhpStrPadType.STR_PAD_RIGHT)
        {
            if (pad_string.Length > 1)
                throw new ArgumentException("pad value has to be single character", "pad_string");

            return str_pad(input, pad_length, pad_string[0], pad_type);
        }

        // strpos ( string $haystack , mixed $needle [, int $offset = 0 ] ) : int
        public static int strpos(string haystack, string needle, int offset = 0)
        {
            return haystack.IndexOf(needle, offset);
        }

        public static int strpos(string haystack, char needle, int offset = 0)
        {
            return strpos(haystack, needle.ToString(), offset);
        }

        // str_repeat ( string $input , int $multiplier ) : string
        public static string str_repeat(string input, int multiplier)
        {
            var sb = new StringBuilder();
            for (int j = 0; j < multiplier; j++)
            {
                sb.Append(input);
            }

            return sb.ToString();
        }

        public static string str_repeat(char input, int multiplier)
        {
            return str_repeat(input.ToString(), multiplier);
        }

        // strtolower ( string $string ) : string
        public static string strtolower(string str)
        {
            return str.ToLower();
        }

        // substr ( string $string , int $start [, int $length ] ) : string
        public static string substr(string str, int start, int length = 0)
        {
            if (length > 0)
            {
                return str.Substring(start, length);
            }

            return str.Substring(start);
        }


        // these handle some assignments/comparisons that php does automatically

        private static bool BytesEqual(byte[] prg0, byte[] prg1)
        {
            if (prg0.Length != prg1.Length)
                return false;

            for (int j = 0; j < prg0.Length; j++)
            {
                if (prg0[j] != prg1[j])
                    return false;
            }

            return true;
        }

        private static bool DictionariesEqual(Dictionary<object, object> labels, Dictionary<object, object> oldLabels)
        {
            if (labels.Keys.Count != oldLabels.Keys.Count)
                return false;

            foreach (var item in labels)
            {
                if (!oldLabels.ContainsKey(item.Key))
                    return false;

                if (!Equals(oldLabels[item.Key], item.Value))
                    return false;
            }

            foreach (var item in oldLabels)
            {
                if (!labels.ContainsKey(item.Key))
                    return false;
            }

            return true;
        }

        private static Dictionary<object, object> DictionaryCopy(Dictionary<object, object> labels)
        {
            //var d = new Dictionary<object, object>();

            //foreach (var item in labels)
            //{
            //    d.Add(item.Key, item.Value);
            //}

            var d = new Dictionary<object, object>(labels);


            return d;
        }

        private static byte ReadAhead(FileStream file)
        {
            var pos = file.Position;
            byte b = (byte)file.ReadByte();
            file.Position = pos;
            return b;
        }

        #endregion


        const byte CDL_CODE = 0x01;
        const byte CDL_DATA = 0x02;
        const byte CDL_BANK_MASK = 0x0C;
        const byte CDL_IND_DATA = 0x10;
        const byte CDL_IND_CODE = 0x20;
        const byte CDL_PCM_DATA = 0x40;


        /**
         *    DISASM6 - A NES-oriented 6502 disassembler which produces asm6 code
         *    Created by Frantik 2011-2015
         *
         */

        const string VERSION = "1.5";
        const string LEFT_MARGIN = "            ";

        static DateTime time_start = microtime(true);

        static Op[] opcodes = new Op[]
        {
            // byte => legal, text, bytes, cycles, addressing mode
            new Op(0x00 , 0, "BRK", 1, 0, 0),
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

        static Dictionary<object, object> registers = new Dictionary<object, object>()
        {
            { "2000", "PPUCTRL" },
            { "2001", "PPUMASK" },
            { "2002", "PPUSTATUS" },
            { "2003", "OAMADDR" },
            { "2004", "OAMDATA" },
            { "2005", "PPUSCROLL" },
            { "2006", "PPUADDR" },
            { "2007", "PPUDATA" },

            { "4000", "SQ1_VOL" },
            { "4001", "SQ1_SWEEP" },
            { "4002", "SQ1_LO" },
            { "4003", "SQ1_HI" },
            { "4004", "SQ2_VOL" },
            { "4005", "SQ2_SWEEP" },
            { "4006", "SQ2_LO" },
            { "4007", "SQ2_HI" },
            { "4008", "TRI_LINEAR" },
            { "400A", "TRI_LO" },
            { "400B", "TRI_HI" },
            { "400C", "NOISE_VOL" },
            { "400E", "NOISE_LO" },
            { "400F", "NOISE_HI" },
            { "4010", "DMC_FREQ" },
            { "4011", "DMC_RAW" },
            { "4012", "DMC_START" },
            { "4013", "DMC_LEN" },
            { "4014", "OAM_DMA" },
            { "4015", "SND_CHN" },
            { "4016", "JOY1" },
            { "4017", "JOY2" },
        };


        static int origin = 0x8000;
        static int labelLen = 0;


        // used for branch opcodes
        private static string addressOffset(int value, string offset2)
        {
            var offset = hexdec(offset2);
            offset += 2; // length of brance command
            if (offset > 0x80)
            {
                offset = offset - 0x100;
            }
            else
            {
                //offset += 2;
            }
            return str_pad(dechex(value + offset), 4, '0', PhpStrPadType.STR_PAD_LEFT);
        }

        private static bool isValidLabel(string addr)
        {
            //global $origin;

            var newaddr = hexdec(addr);

            return (newaddr >= origin && newaddr < 0xFFFA);
        }

        private static bool addValidLabel(string addr, Dictionary<object, object> labels)
        {
            if (isValidLabel(addr) && !isset(labels, addr))
            {
                labels[addr] = true;
                return true;
            }

            return false;
        }


        private static void addVector(string vector, string str, Dictionary<object, object> labels)
        {
            if (isset(labels, vector))
            {
                if (labels[vector] is bool && (bool)labels[vector] == true)
                {
                    labels[vector] = str;
                }
                else if (is_array(labels[vector]))
                {
                    var arr = (string[])labels[vector];
                    if (!in_array(str, arr))
                    {
                        var tmp = arr.ToList();
                        tmp.Add(str);
                        labels[vector] = tmp.ToArray();
                    }
                }
                else if (labels[vector] is string)
                {
                    var vecstr = (string)labels[vector];
                    if (!string.Equals(vecstr, str))
                    {
                        // labels[vector] is a standalone string but is not the string given
                        labels[vector] = new string[] { vecstr, str };
                    }
                }
            }
            else
            {
                labels[vector] = str; // vector not added/used yet, assign name 
            }
        }

        private static string wordStr(byte[] str)
        {
            return dechex_pad(str[1]) + dechex_pad(str[0]);
        }

        // make sure hex values have leading zeros
        private static string dechex_pad(int dec, int len = 2)
        {

            if (dec > 0xFF)
            {
                len = 4;
            }
            else if (dec > 0xFFFF)
            {
                len = 6;
            }

            return str_pad(dechex(dec), len, '0', PhpStrPadType.STR_PAD_LEFT);
        }

        // make sure binary values have leading zeros
        private static string decbin_pad(int dec, int len = 8)
        {

            if (dec > 0xFF)
            {
                len = 16;
            }
            else if (dec > 0xFFFF)
            {
                len = 32;
            }

            return str_pad(decbin(dec), len, '0', PhpStrPadType.STR_PAD_LEFT);
        }

        private static string commentLine(int len = 80)
        {
            return ";" + str_repeat('-', len - 1) + "\n";
        }

        private static string commentHeader(string text, bool initialNL = true, bool initialLine = true)
        {
            string ret = (initialNL ? "\n" : "") + (initialLine ? commentLine() : "");
            ret += $"; {text}";
            ret += "\n" + commentLine();

            return ret;
        }

        private string strToHex(string str, bool fancy = false)
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

        private static HeaderInfo getHeaderInfo(System.IO.FileStream file)
        {
            var oldloc = file.Position;

            var head = fread(file, 0x4);
            var nes = Encoding.ASCII.GetString(head);

            if (nes == ("NES" + ((char)(0x1A)).ToString()))
            {
                var info = new HeaderInfo();

                info.head = head;
                info.prg = (fread(file, 1))[0];
                info.chr = (fread(file, 1))[0];
                info.ctrl_1 = (fread(file, 1))[0];
                info.ctrl_2 = (fread(file, 1))[0];
                info.tail = fread(file, 8);

                info.mirroring = (byte)(info.ctrl_1 & bindec("00000001"));
                info.sram = (byte)((info.ctrl_1 & bindec("0000010")) >> 1);
                info.trainer = (byte)((info.ctrl_1 & bindec("00000100")) >> 2);
                info.fourscreen = (byte)((info.ctrl_1 & bindec("00001000")) >> 3);

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

        private static string processHeaderInfo(HeaderInfo info)
        {
            //global $labelLen;

            var pad = 30 + labelLen;
            var ret = "";
            if (info != null)
            {
                //$ret .= commentLine();
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

                //$ret.= str_pad(LEFT_MARGIN. ".hex ".strToHex(substr($info->tail, 0, 4)), $pad). " ; iNES Tail \n";
                //$ret.= str_pad(LEFT_MARGIN. ".hex ".strToHex(substr($info->tail, 4)), $pad). "  \n";

                var sb = new StringBuilder();
                sb.Append(LEFT_MARGIN);
                sb.Append(" .hex ");
                sb.Append(string.Join(" ", info.tail.Take(4).Select(b => b.ToString("x2"))).PadRight(pad));
                sb.AppendLine(" ; iNES Tail ");
                sb.Append(LEFT_MARGIN);
                sb.Append(" .hex ");
                sb.Append(string.Join(" ", info.tail.Skip(4).Select(b => b.ToString("x2"))).PadRight(pad));
                sb.AppendLine("  ");

                ret += sb.ToString();

                return ret;
            }

            return null;
        }

        private static string toLittleEndianStr(string str)
        {
            if (str == null)
                return null;

            //return str[2] + str[3] + " " + str[0] + str[1];
            return $"{str.Substring(2)} {str.Substring(0, 2)}";
        }

        private static string processVectors(string nmi, string reset, string brk)
        {
            //global $labelLen;

            var marginLen = strlen(LEFT_MARGIN);
            var pad = 30 + marginLen;

            var ret = commentHeader("Vector Table");
            var line1 = str_pad("vectors:", marginLen);
            line1 += ".dw nmi";
            ret = ret + str_pad(line1, pad) + " ; $fffa: " + toLittleEndianStr(nmi) + "     Vector table\n";
            ret += str_pad(LEFT_MARGIN + ".dw reset", pad) + " ; $fffc: " + toLittleEndianStr(reset) + "     Vector table\n";
            ret += str_pad(LEFT_MARGIN + ".dw irq", pad) + " ; $fffe: " + toLittleEndianStr(brk) + "     Vector table\n";

            return ret;
        }

        private static int baseToDec(string str)
        {
            switch (str[0])
            {
                case '0':
                    if (str[1] != 'x')
                    {
                        break;
                    }
                    else
                    {
                        return hexdec(substr(str, 2));
                    }
                    break;

                case '$':
                    return hexdec(substr(str, 1));
                    break;

                case '%':
                    return bindec(substr(str, 1));
                    break;
            }
            return int.Parse(str); // todo? TrimStart('0') 
        }

        private static Dictionary<object, object> readLabels(string filename)
        {

            //var arr = readLabelText(file_get_contents(filename));

            var lines = File.ReadAllLines(filename);
            var arr = readLabelText(lines);

            return arr;


        }

        /*      

        function readLabelText($str)
        {
           $arr = array();
           $len = 0;
           $str  = preg_replace('%;.*$%m', '', $str);

           if (preg_match_all('%^\s*([a-zA-Z0-9_\-\+\@]*)\s*\=\s*([\$\%]*)([a-fA-F0-9]*)%m', $str, $matches))
           {
              foreach($matches[0] as $n => $v)
              {  $matches[1][$n] = trim($matches[1][$n]);

                 $thislen = strlen($matches[1][$n]);

                 if ($thislen > $len)
                 {
                    $len = $thislen;
                 }

                 if (strlen($matches[1][$n]) > 0)
                 {
                    if ($matches[2][$n] == '')
                    {  $matches[3][$n] = dechex_pad($matches[3][$n]);
                    }

                    if ($matches[2][$n] == '%')
                    {
                       $matches[3][$n] = dechex_pad(bindec($matches[3][$n]));
                    }

                    $arr[strtolower($matches[3][$n])] =  $matches[1][$n];
                 }
              }
           }

           $arr['maxLength'] = $len;

           return $arr;
        }
        */

        private static Dictionary<object, object> readLabelText(string[] lines)
        {
            var arr = new Dictionary<object, object>();
            var len = 0;

            var rxLabel = new Regex(@"[a-zA-Z0-9_\-\+\@]+");
            var rxAddr = new Regex(@"[\$\%]*[a-fA-F0-9]+");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var tmp = line.Replace(".", "").Replace(";", "").Replace("*", "");

                var parts = tmp.Split('=');
                if (parts.Length != 2)
                    continue;

                if (!rxLabel.IsMatch(parts[0]) || !rxAddr.IsMatch(parts[1]))
                    continue;

                var label = parts[0].Trim();
                var addr = parts[1].Trim();

                int addrVal = addr.StartsWith("%")
                    ? bindec(addr)
                    : hexdec(addr);

                arr.Add(dechex_pad(addrVal), label.ToLower());

                if (label.Length > len)
                {
                    len = label.Length;
                }
            }

            arr["maxLength"] = len;

            return arr;
        }


        private static string outputLabels(Dictionary<object, object> arr, string text)
        {
            //global $origin;

            var ret = commentHeader(text);

            //foreach ($arr as $n => $v)
            foreach (var item in arr)
            {
                var n = (string)item.Key;
                var v = (string)item.Value;

                if (n == "maxLength")
                {
                    continue;
                }

                if (hexdec(v) < origin)
                {
                    ret += str_pad(v, 20) + " = $" + n + "\n";
                }
            }

            return ret;

        }

        private static void outputHelp(string text = null)
        {
            //global $argv;
            //$dasm =  pathinfo($argv[0], PATHINFO_BASENAME);

            Console.Write(@"
Usage:

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
  iw    ignorewrites    Ignore writes to \$8000 - \$FFFF
  m2    mapper2         Enable mapper 2 (UxROM) support
");

            Console.Write("\n" + (text != null ? $"\nERROR: {text}\n" : ""));
        }

        private static bool isCounterLabel(int addr2, Dictionary<object, object> labels)
        {
            var addr = dechex_pad(addr2);

            if (isset(labels, addr))
            {
                if (labels[addr] is string[])
                {

                }

                //if (preg_match('%^([^\+\-]+)[\+\-][0-9]+%', $labels[$addr]))
                if (labels[addr] is string && Regex.IsMatch((string)labels[addr], @"^([^\+\-]+)[\+\-][0-9]+"))
                {
                    return false;
                }

                return true;
            }
            return false;

        }



        public static void Run(Inputs inputs)
        {
            // Program start

            var head = "DISASM6 v" + VERSION + " - A NES-oriented 6502 disassembler - Created by Frantik 2015";
            Console.Write($"\n{head}\n" + str_repeat('-', 79) + "\n");

            //int fnIndex = 0;

            ////if (!isset(argv[1]))
            //if (argv.Length < fnIndex + 1)
            //{
            //    outputHelp();
            //    return;
            //}

            //var filename = argv[fnIndex];

            //if (!File.Exists(filename))
            //{
            //    outputHelp("File not found\n");
            //    return;
            //}

            ////$origin = 0x8000; // global
            //bool showHeader = true;
            //bool includeChr = false;
            //bool includeReg = false;
            //bool originOverride = false;
            //bool noDetect = false;
            //string shortname = pathinfo(filename, PhpPathInfo.PATHINFO_FILENAME);
            //string labelFile = null;
            //string cdlFilename = null;
            //bool ignoreHeader = false;
            //int fileStart = 0;
            //bool fileStartOverride = false;
            //int fileLength = 0x10000;
            //bool lengthOverride = false;
            //int fileEnd = 0;
            //bool fileEndOverride = false;
            //int codeStart = 0;
            //bool codeStartOverride = false;
            //int codeEnd = 0;
            //bool codeEndOverride = false;
            //int cdlOffset = 0;
            //bool ignoreWrites = false;
            //bool useLowerCase = true;
            //bool usingMapper2 = false;

            //int lastPass = 9;


            bool showHeader = inputs.showHeader;
            bool includeChr = inputs.includeChr;
            bool includeReg = inputs.includeReg;
            bool originOverride = inputs.originOverride;
            bool noDetect = inputs.noDetect;
            string shortname = inputs.shortname; // pathinfo(filename, PhpPathInfo.PATHINFO_FILENAME);
            string labelFile = inputs.labelFile;
            string cdlFilename = inputs.cdlFilename;
            bool ignoreHeader = inputs.ignoreHeader;
            int fileStart = inputs.fileStart;
            bool fileStartOverride = inputs.fileStartOverride;
            int fileLength = inputs.fileLength;
            bool lengthOverride = inputs.lengthOverride;
            int fileEnd = inputs.fileEnd;
            bool fileEndOverride = inputs.fileEndOverride;
            int codeStart = inputs.codeStart;
            bool codeStartOverride = inputs.codeStartOverride;
            int codeEnd = inputs.codeEnd;
            bool codeEndOverride = inputs.codeEndOverride;
            int cdlOffset = inputs.cdlOffset;
            bool ignoreWrites = inputs.ignoreWrites;
            bool useLowerCase = inputs.useLowerCase;
            bool usingMapper2 = inputs.usingMapper2;

            int lastPass = inputs.lastPass;

            int marginLen = strlen(LEFT_MARGIN);



            //// check command line params
            //for (var i = fnIndex + 1; i < argv.Length; i++)
            //{
            //    string nextParam = null;

            //    if ((i + 1) < argv.Length && substr(argv[i + 1], 0, 1) != "-")
            //    {
            //        nextParam = argv[i + 1];
            //    }

            //    switch (strtolower(argv[i]))
            //    {
            //        case "-o":
            //        case "-origin":

            //            if (nextParam == null)
            //            {
            //                outputHelp("Must specify a valid origin");
            //            }

            //            origin = baseToDec(argv[++i]);
            //            originOverride = true;
            //            break;

            //        case "-cs":
            //        case "-codestart":

            //            if (nextParam == null)
            //            {
            //                outputHelp("Must specify a valid code start location ");
            //            }

            //            codeStart = baseToDec(argv[++i]);
            //            codeStartOverride = true;

            //            break;

            //        case "-fs":
            //        case "-filestart":

            //            if (nextParam == null)
            //            {
            //                outputHelp("Must specify a valid file start location ");
            //            }

            //            fileStart = baseToDec(argv[++i]);
            //            fileStartOverride = true;
            //            break;

            //        case "-len":
            //        case "-length":
            //            if (nextParam == null)
            //            {
            //                outputHelp("Must specify a valid length to read");
            //            }

            //            fileLength = baseToDec(argv[++i]);  // this will be tweaked later
            //            lengthOverride = true;
            //            break;

            //        case "-fe":
            //        case "-fileend":

            //            if (nextParam == null)
            //            {
            //                outputHelp("Must specify a valid file end location ");
            //            }

            //            fileEnd = baseToDec(argv[++i]);
            //            fileEndOverride = true;
            //            break;

            //        case "-ce":
            //        case "-codeend":

            //            if (nextParam == null)
            //            {
            //                outputHelp("Must specify a valid code end location ");
            //            }

            //            fileLength = baseToDec(argv[++i]); // will NOT be tweaked since lengthOverride isn't enable
            //            codeEndOverride = true; // todo never used
            //            break;


            //        case "-h":
            //        case "-noheader":
            //            showHeader = false;
            //            break;

            //        case "-i":
            //        case "-ignoreheader":
            //            ignoreHeader = true;
            //            break;

            //        case "-c":
            //        case "-chr":
            //            includeChr = true;
            //            break;

            //        case "-r":
            //        case "-registers":
            //            includeReg = true;
            //            break;

            //        case "-t":
            //        case "-target":
            //            if (nextParam == null)
            //            {
            //                outputHelp("You must specify a target file");
            //            }

            //            var path = Regex.Replace(argv[++i], @"[^a-zA-Z0-9_\-\. ]", "");
            //            shortname = pathinfo(path, PhpPathInfo.PATHINFO_FILENAME);

            //            break;

            //        case "-p":
            //        case "-passes":
            //            if (nextParam == null || !int.TryParse(nextParam, out lastPass))
            //            {
            //                outputHelp("You must specify a number of passes");
            //            }

            //            //lastPass = (int)argv[++i];
            //            break;

            //        case "-nodetect":
            //        case "-d":
            //            noDetect = true;
            //            break;

            //        case "-l":
            //        case "-labels":
            //            if (nextParam == null || !File.Exists(nextParam))
            //            {
            //                outputHelp("You must specify a valid file");
            //            }

            //            labelFile = argv[++i];
            //            break;


            //        case "-cdl":
            //            if (nextParam == null || !System.IO.File.Exists(nextParam))
            //            {
            //                outputHelp("You must specify a valid file");
            //            }

            //            cdlFilename = argv[++i];
            //            break;

            //        case "-cdlo":
            //        case "-cdloffset":
            //            if (nextParam == null)
            //            {
            //                outputHelp("You must specify a valid offset for the CDL");
            //            }

            //            cdlOffset = baseToDec(argv[++i]);
            //            break;


            //        case "-lc":
            //        case "-lowercase":
            //            useLowerCase = true;

            //            break;

            //        case "-cc":
            //        case "-uppercase":
            //            useLowerCase = false;

            //            break;

            //        case "-iw":
            //        case "-ignorewrites":

            //            ignoreWrites = true;
            //            break;

            //        case "-m2":
            //        case "-mapper2":

            //            usingMapper2 = true;
            //            break;
            //    }

            //}


            if (fileEndOverride)
            {
                fileLength = fileStart + fileEnd;
                lengthOverride = true;
            }


            var file = File.OpenRead(inputs.filename);

            var pass = 1;

            Dictionary<object, object> oldLabels = null;

            var initLabels = new Dictionary<object, object>()
            {
                { "fffa", "vectors" },
                { "fffc", true },
                { "fffe", true },
            };

            if (includeReg)
            {
                //$initLabels += $registers;
                foreach (var item in registers)
                {
                    initLabels.Add(item.Key, item.Value);
                }
            }

            Dictionary<object, object> fileLabels = null;
            if (labelFile != null)
            {
                fileLabels = readLabels(labelFile);

                //$mapperArr = $fileLabels['mapperArr'];
                //unset($fileLabels['mapperArr']);

                labelLen = (int)fileLabels["maxLength"] - 10;
                labelLen = labelLen < 0 ? 0 : labelLen;
                //unset(fileLabels["maxLength"]);
                fileLabels["maxLength"] = null;

                //initLabels += fileLabels;
                foreach (var item in fileLabels)
                {
                    initLabels.Add(item.Key, item.Value);
                }

            }

            System.IO.FileStream cdlFile = null;
            var cdlByte = 0;

            if (cdlFilename != null)
            {
                cdlFile = File.OpenRead(cdlFilename); //cdlFile = fopen(cdlFilename, 'r');
                cdlByte = 0;
            }


            string header = null;
            var theOldLabel = "";

            var theText = new StringBuilder();
            theText.Append(commentHeader(pathinfo(inputs.filename, PhpPathInfo.PATHINFO_BASENAME) + " disasembled by DISASM6(dotnet) v" + VERSION, false));
            //invalidCounter = 0;

            var prgBank = 0;
            var theLabel = "";

            Dictionary<object, object> oldPrgLabels = null;

            HeaderInfo headerInfo = null;
            byte oldPrg = 0;
            int prgStart = 0;

            if (!ignoreHeader)
            {
                headerInfo = getHeaderInfo(file);
                oldPrg = headerInfo.prg;
                prgStart = 0x10;
            }

            string nmi = null;
            string reset = null;
            string brk = null;

            var oldDidDrawLine = false;

            //  This loop is done x times
            //  The first pass we just collect addesses
            //  The next passes we look for new addresses
            //
            //  The last pass we build the actual output
            while (pass <= lastPass)
            {
                var labels = new Dictionary<object, object>();

                if (true || pass < 3) // todo this excludes nes register labels starting for pass 4+
                {
                    labels = DictionaryCopy(initLabels);
                }
                var prgLabels = DictionaryCopy(initLabels);

                var counter = origin;

                if (fileStartOverride && !codeStartOverride)
                {
                    fseek(file, fileStart);
                }


                if (!ignoreHeader)
                {
                    fseek(file, prgStart);
                }

                if (codeStartOverride)
                {
                    fseek(file, fileStart);
                }


                byte newPrg = 0;


                // do this stuff only on the first pass
                if (pass == 1)
                {
                    oldDidDrawLine = false;
                    oldLabels = DictionaryCopy(labels);

                    // check for 16k roms
                    if (!noDetect)
                    {
                        newPrg = 0;
                        if (headerInfo?.prg == 2) // 2 x 16K
                        {
                            var prg0 = fread(file, 0x4000);
                            var prg1 = fread(file, 0x4000);
                            fseek(file, fileStart + 0x10);

                            if (headerInfo.mapper == 0 && BytesEqual(prg0, prg1)) // todo cnrom could also be this
                            {
                                echo("PRG Banks 0 and 1 are identical, 16k PRG suspected, use -d to disable check\n");
                                newPrg = 1;

                                origin = originOverride ? origin : 0xc000;

                                if (cdlFilename != null)
                                {
                                    cdlOffset += 0x4000;
                                }
                            }
                        }
                        else if (headerInfo?.prg == 1)
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



                    if (codeStartOverride)
                    {
                        fileStart = codeStart - origin + (headerInfo != null ? 10 : 0);  // todo 0x10 ?
                        origin = codeStart;
                        originOverride = true;

                        fileStartOverride = true;
                        fseek(file, fileStart);

                        cdlOffset += fileStart - (headerInfo != null ? 10 : 0); // todo 0x10 ?


                        echo("Starting at code location $" + dechex_pad(fileStart) + "\n");
                    }
                    else if (fileStartOverride /* && !codeStartOverride */)
                    {
                        echo("Starting at file location 0x" + dechex_pad(fileStart) + "\n");
                    }

                    if (lengthOverride)
                    {
                        echo("Reading 0x" + dechex_pad(fileLength) + " bytes\n");

                        fileLength += origin - (headerInfo != null ? 0x10 : 0);
                    }


                    if (includeChr && headerInfo != null)
                    {
                        //echo "Using CHR-ROM\n";
                    }

                    echo("\n");
                }

                if (cdlFilename != null)
                {
                    fseek(cdlFile, cdlOffset);
                }

                // if 16k rom, update prg info
                if (newPrg > 0)
                {
                    headerInfo.prg = newPrg;
                }

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

                // read the file
                // each pass of this loop completes one line of output

                counter = origin;
                echo($"Starting pass {pass} " + (pass == lastPass ? "(final) " : "") + "... ");

                while (!feof(file) && counter < fileLength)
                {
                    //var add = false; // unused
                    var invalidText = "Invalid Opcode";
                    var didDrawLine = false;

                    // handle mapper 2 // todo appears not to work
                    if (usingMapper2
                        && headerInfo?.mapper == 2
                        && counter == 0xC000
                        && prgBank < (headerInfo.prg - 1)
                    )
                    {
                        counter = 0x8000;
                        prgBank++;

                        if (pass == lastPass)
                        {
                            theText.Append(commentHeader($"PRG Bank {prgBank}"));
                            theText.Append(LEFT_MARGIN + ".base 0x8000\n");
                            theText.Append(commentLine());
                        }
                        continue;

                    }

                    // handle vectors

                    if (pass < lastPass && counter == 0xFFFA)
                    {
                        nmi = wordStr(fread(file, 2));
                        reset = wordStr(fread(file, 2));
                        brk = wordStr(fread(file, 2));

                        addVector(nmi, "nmi", labels);
                        addVector(reset, "reset", labels);
                        addVector(brk, "irq", labels);

                        prgLabels[nmi] = true;
                        prgLabels[reset] = true;
                        prgLabels[brk] = true;

                        counter += 6;
                        continue;
                    }
                    else if (pass == lastPass && counter == 0xFFFA)
                    {
                        theText.Append(processVectors(nmi, reset, brk));
                        fread(file, 6);

                        counter += 6;

                        continue;
                    }

                    if (counter == 0xd616 || counter == 0xd617)
                    {
                        if (pass >= 4)
                        {

                        }
                    }

                    //read opcode
                    var opcode = (fread(file, 1)[0]);


                    var isInvalid = opcodes[opcode].Legal; // [0]; // todo backwards?
                    var mnemonic = opcodes[opcode].Text; // [1];
                    var byteLen = opcodes[opcode].Bytes; // [2];
                    var addressingType = opcodes[opcode].AddrMode; //[4];

                    var isDataByte = false;
                    var dataStr = "Suspected data";

                    // check code/data log - if data, don' process as an opcode
                    if (cdlFilename != null)
                    {
                        var newCdlByte = (fread(cdlFile, 1))[0];
                        var counter_pad = dechex_pad(counter);


                        // draw line between data and code
                        if (pass == lastPass
                           && !oldDidDrawLine
                           && counter != origin
                           && newCdlByte != 0 // 0 would indicate not logged?
                                              //&& ((newCdlByte & bindec("00000001")) != (cdlByte & bindec("00000001")))
                           && ((newCdlByte & (CDL_CODE)) != (cdlByte & (CDL_CODE))) // change from data to code or vice versa todo indirect todo? indirect code
                        )
                        {
                            theText.Append("\n" + commentLine());

                            didDrawLine = true;
                        }

                        // check if the CDL byte is known, if known, copy, otherwise do some checks
                        if (newCdlByte != 0)
                        {
                            cdlByte = newCdlByte;
                        }
                        // if byte is zero and we're at a program label, assume code
                        else if (oldPrgLabels != null && isset(oldPrgLabels, counter_pad))
                        {
                            //cdlByte = bindec("00000001");
                            cdlByte = CDL_CODE;
                        }
                        // if byte is zero and we're at a label, but not program, assume data (only on 2nd pass)
                        else if (isset(oldLabels, counter_pad) && pass > 1)
                        {
                            //cdlByte = bindec("00000010");
                            cdlByte = CDL_DATA;
                        }
                        // else assume program code
                        // todo?


                        // data byte
                        //if ((($cdlByte & bindec('00000010')) >> 1) && !($cdlByte & bindec('00000001')))
                        if ((cdlByte & CDL_DATA) != 0 && (cdlByte & CDL_CODE) == 0)
                        {

                            //var counter_pad = dechex_pad(counter);

                            if (isCounterLabel(counter, oldLabels))
                            {
                                theOldLabel = (((bool)oldLabels[counter_pad]) == true)
                                   ? "__" + counter_pad
                                   : (string)oldLabels[counter_pad];

                                //$theOldLabel = preg_replace("%^([^\+\-]+)[\+\-][0-9]+%", "$1", $theOldLabel);
                            }

                            if (theOldLabel.EndsWith("JumpTable"))
                            {

                                byteLen = 2;
                                mnemonic = ".word";
                                addressingType = 11;
                                isInvalid = false;  // 0;
                                //fseek($file, ftell($file) - 1);



                            }
                            else if (theOldLabel.EndsWith("RTSTable"))
                            {

                                byteLen = 2;
                                mnemonic = ".word";
                                addressingType = 12;
                                isInvalid = false;  // 0;

                            }
                            //else if (substr(theOldLabel, -8) == "TableLow")
                            //{
                            //   byteLen = 1;
                            //   mnemonic = ".byte";
                            //   addressingType = 13;
                            //   isInvalid = 0;
                            //}
                            //else if (substr(theOldLabel, -9) == "TableHigh")
                            //{
                            //   byteLen = 1;
                            //   mnemonic = ".byte";
                            //   addressingType = 14;
                            //   isInvalid = 0;
                            //}  
                            else
                            {
                                byteLen = 4;
                                //echo substr($theLabel, -11);
                                mnemonic = "";
                                addressingType = -1;
                                isInvalid = true; //  1;
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
                    var bytes = new byte[0];
                    var byteStr = "";
                    var trailer = "";
                    var hextext = dechex_pad(opcode);

                    var byteArr = new List<string>() { hextext };
                    long cdlPos = 0;

                    // read 1 or 2 byte paramters for the opcode
                    if (readBytes > 0)
                    {
                        var didMoveCdlPtr = false;

                        var operand = ReadAhead(file);
                        var brks = new[] { "BCC", "BCS", "BEQ", "BMI", "BNE", "BPL", "BVC", "BVS", };

                        if (pass >= 1)
                        {
                            if (cdlFilename != null)
                            {
                                //cdlPos = ftell(cdlFile);
                                cdlPos = cdlFile.Position;
                                didMoveCdlPtr = false;
                            }
                            // check to see if a label exists in this opcode.. if so then usually it's data
                            for (var i = 1; i <= readBytes; i++)
                            {
                                var counterNext = counter + i;

                                if (isCounterLabel(counterNext, oldLabels)
                                   //if (isset($oldLabels[dechex_pad($counter + $i)])
                                   || counterNext >= 0xFFFA
                                   || (counterNext >= fileLength)
                                   || (operand == 0xff && brks.Contains(mnemonic)) // break into self
                                   || (usingMapper2 && headerInfo?.mapper == 2 && counterNext > 0xBFFF && prgBank < headerInfo.prg - 1)
                                 ) // if counter in the vectors

                                {
                                    //var invalidCounter = 0;
                                    readBytes = i - 1;
                                    isInvalid = true; // 1;
                                    byteLen = i;
                                    addressingType = -1;
                                    continue;
                                }

                                // if this byte marked as data in cdl; check if next bytes are code
                                if (cdlFilename != null && isDataByte)
                                {
                                    var newCdlByte = (fread(cdlFile, 1))[0];
                                    didMoveCdlPtr = true;
                                    //if ((newCdlByte & bindec("00000001")) != 0)
                                    if ((newCdlByte & CDL_CODE) != 0)
                                    {
                                        //var invalidCounter = 0;
                                        readBytes = i - 1;
                                        isInvalid = true; // 1;
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
                            bytes = fread(file, readBytes);

                            if (cdlFilename != null)
                            {
                                var cdlBytes = fread(cdlFile, readBytes);
                            }

                            for (var j = 0; j < readBytes; j++)
                            {
                                byteArr.Add(dechex_pad((bytes[j])));
                                hextext += " " + byteArr[j + 1];
                            }

                            if (addressingType == 12)
                            {
                                byteStr = (byteArr.Count > 1 ? byteArr[1] : "") + byteArr[0];
                                byteStr = dechex_pad(hexdec(byteStr) + 1);
                                //echo " $counter $addressingType $byteStr ";
                                //print_r($byteArr);
                            }
                            else if (addressingType == 11)
                            {
                                byteStr = (byteArr.Count > 1 ? byteArr[1] : "") + byteArr[0];
                            }
                            else
                            {
                                byteStr = (byteArr.Count > 2 ? byteArr[2] : "") + byteArr[1];
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
                       // && addressingType != 9
                       && addressingType != 3)
                    {
                        isInvalid = true; // 1;
                        invalidText = "Bad Addr Mode";
                    }

                    // add label to list
                    var oldByteStr = byteStr;
                    var lbl = "$";



                    if (addressingType > 0
                      && isValidLabel(byteStr)
                      && !(ignoreWrites && substr(mnemonic, 0, 2) == "ST" && (hexdec(byteStr) < 0x8000))) // do not add labels when writing to PRG 
                    {

                        lbl = "__";

                        if (pass < lastPass && isInvalid != true)
                        {

                            addValidLabel(byteStr, labels);
                        }

                    }

                    oldByteStr = byteStr;

                    //    byteStrDec = (dechex_pad(byteStr);
                    var newByteStr = lbl + byteStr;

                    //if (isset($oldLabels[$byteStr]) && $lbl !== '')
                    //{
                    //   $oldLabel = $oldLabels[$byteStr];

                    //   $newByteStr = $oldLabel === true
                    //      ?  $newByteStr
                    //      :  $oldLabel;
                    //}

                    if (isset(oldLabels, byteStr) && lbl != "")
                    {
                        var oldLabel = oldLabels[byteStr];

                        var b = oldLabel as bool?;
                        var s = oldLabel as string;

                        newByteStr = b != null && b.Value
                           ? newByteStr
                           : s ?? "Array"; // Convert.ToString( oldLabel ); // todo 
                    }

                    // lets check for various addressing types to figure out how to format the text
                    switch (addressingType)
                    {

                        case 0: // Implicit/Accumulator/Immediate
                            byteStr = (readBytes > 0 ? "#$" + byteStr : "");
                            break;

                        case 12:
                        case 11:
                        case 10:
                            if (!isInvalid)
                            {
                                addValidLabel(byteStr, prgLabels);
                            }

                            byteStr = newByteStr;
                            if (addressingType == 12)
                            {
                                byteStr += "-1";
                            }

                            break;
                        case 1: // Absolute
                        case 4: // Zero Page

                            byteStr = newByteStr;

                            if (addressingType == 12)
                            {
                                byteStr += "-1";
                            }

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

                            var isInvalidBranch = false;

                            var operand = ReadAhead(file);
                            if (operand == 0xff)
                            {
                                isInvalidBranch = true;
                            }

                            if (pass < lastPass && !isInvalid && !isInvalidBranch)
                            {
                                addValidLabel(addr, labels);
                                addValidLabel(addr, prgLabels);
                            }

                            if (!isInvalidBranch && isValidLabel(addr))
                            {
                                //if (isset($labels[$addr]) && $labels[$addr] !== true)

                                if (isset(labels, addr) && !(labels[addr] is bool && (bool)labels[addr] == true))
                                {
                                    byteStr = (string)labels[addr];
                                }
                                else
                                {

                                    byteStr = "__" + addr;
                                }
                            }
                            else
                            {
                                isInvalid = true;
                                invalidText = "Illegal Branch";
                            }

                            break;

                        // add some space after RTS/JMP
                        case "RTS":
                        case "RTI":
                        case "JMP":
                            if (!isInvalid)
                            {
                                trailer = "\n" + commentLine();
                                didDrawLine = true;
                            }
                            break;

                    }

                    string oldMnemonicStr = string.Empty;

                    // only deal with output on last pass
                    if (pass == lastPass)
                    {
                        if (isInvalid)
                        {
                            oldMnemonicStr = addressingType == -1 ? dataStr : (invalidText + " - " + mnemonic + " " + byteStr);
                            mnemonic = ".hex";
                            byteStr = hextext;

                        }
                        var counter_pad = dechex_pad(counter);
                        if (oldLabels.ContainsKey(counter_pad))
                        {
                            var arr = oldLabels[counter_pad] as string[];
                            var leng = 1;

                            if (arr != null)
                            {
                                leng = arr.Length;
                            }

                            for (var i = 0; i < leng; i++)
                            {
                                if (arr != null)
                                {
                                    theLabel = arr[i];
                                }
                                else
                                {
                                    //theLabel = oldLabels[counter_pad] is bool && ((bool)oldLabels[counter_pad]) == true
                                    //   ? "__" + counter_pad
                                    //   : (string)oldLabels[counter_pad];

                                    var s = oldLabels[counter_pad] as string;
                                    var b = oldLabels[counter_pad] as bool?;

                                    theLabel = b != null && b.Value ? "__" + counter_pad : s;
                                }

                                // if label has a + or - in it but doesn't start with one
                                // then don't show it
                                // if not 0 or false
                                if (strpos(theLabel, '+') != -1 || strpos(theLabel, '-') != -1)
                                {
                                    theText.Append(LEFT_MARGIN);
                                    continue;
                                }

                                bool didNotDraw = !(oldDidDrawLine || didDrawLine);

                                switch (theLabel)
                                {
                                    case "irq":
                                        theText.Append(commentHeader("irq/brk vector", didNotDraw, didNotDraw));
                                        break;

                                    case "nmi":
                                    case "reset":
                                        theText.Append(commentHeader($"{theLabel} vector", didNotDraw, didNotDraw));
                                        break;

                                }

                                if (strlen(theLabel) >= marginLen - 1)
                                {
                                    theText.Append((oldDidDrawLine || didDrawLine || (counter == origin) ? "" : "\n")
                                       + $"{theLabel}:\n" + LEFT_MARGIN);
                                }
                                else
                                {
                                    theText.Append(str_pad(theLabel + ":", marginLen));
                                }

                            }

                        }
                        else
                        {
                            theText.Append(LEFT_MARGIN);
                        }

                        //$line = array_key_exists(dechex_pad($counter), $oldLabels) ? '__' . dechex_pad($counter) .':' : '       ';
                        var line = (useLowerCase ? strtolower(mnemonic) : mnemonic) + " " + byteStr;

                        //$labelLen
                        line = str_pad(line, 30 - marginLen + labelLen);

                        line += " ; $" + dechex_pad(counter) + ": " + hextext;
                        line = str_pad(line, (isDataByte ? 54 : 50) - marginLen + labelLen);
                        line += (isInvalid ? oldMnemonicStr : "");
                        line += "\n" + trailer;

                        theText.Append(line);

                    }
                    counter += byteLen;
                    oldDidDrawLine = didDrawLine;
                }  // end line by line loop



                if (pass < lastPass && oldLabels != null && DictionariesEqual(labels, oldLabels))
                {
                    lastPass = pass + 1;
                }
                //elseif($pass < $lastPass)
                //{
                //    echo "$pass < $lastPass && $oldLabels !== false && $labels == $oldLabels (" . print_r($labels == $oldLabels,true);
                //    file_put_contents('out', print_r($labels,true) . print_r($oldLabels, true));
                //    file_put_contents('out2', print_r(array_diff_assoc($labels, $oldLabels),true));
                //}

                if (pass < lastPass)
                {
                    // don't have to deep copy, labels/prgLabels are created new at the top of the loop
                    oldLabels = (labels);
                    oldPrgLabels = (prgLabels);

                    file.Position = 0;
                }

                echo("complete\n");
                pass++;
            }

            if (includeChr && headerInfo != null)
            {

                fseek(file, oldPrg * 0x4000 + 0x10);

                var chr = "";


                while (!feof(file))
                {
                    chr += fread(file, 1024);

                }

                theText.Append("\n" + commentLine());
                theText.Append("; CHR-ROM");
                theText.Append("\n" + commentLine());

                var incLine = LEFT_MARGIN + ".incbin " + shortname + ".chr";
                theText.Append(str_pad(incLine, 30 + labelLen) + " ; Include CHR-ROM\n");

                file_put_contents(shortname + ".chr", chr);
                Console.Write($"\nCHR-ROM exported as {shortname}.chr");


            }
            else if (includeChr)
            {
                Console.Write("\nCHR-ROM cannot be exported without iNES header data");
                if (ignoreHeader)
                {
                    Console.Write("\nTry disabling -ignoreheader if you wish to export CHR-ROM data");
                }
            }

            file_put_contents(shortname + ".asm", theText.ToString());


            var time_end = microtime(true);
            var time = Math.Round((time_end - time_start).TotalMilliseconds / 1000, 3);

            echo($"\nDisassembly {shortname}.asm generated in {time} seconds\n\n");
        }

    }

}
