using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace da6
{
    /// <summary>
    /// this file contains equivalent implementations of php core functions. the functionality 
    /// is mostly limited to what is needed per the original php disassembly code.
    /// these are defined in the umbrella class so the .net ports can access them without qualifiers.
    /// </summary>
    partial class da6Umbrella
    {

        #region php shims

        const int FILE_APPEND = 0x08;

        #region str_pad() types

        const int STR_PAD_LEFT = 0;
        const int STR_PAD_RIGHT = 1;
        const int STR_PAD_BOTH = 2;

        #endregion

        #region pathinfo() flags


        /// <summary>
        /// path of the directory or file
        /// </summary>
        public const int PATHINFO_DIRNAME = 1;

        /// <summary>
        /// name of the directory or the name and extension of the file
        /// </summary>
        public const int PATHINFO_BASENAME = 2;

        /// <summary>
        /// extension of the file
        /// </summary>
        public const int PATHINFO_EXTENSION = 4;

        /// <summary>
        /// name of the file (without the extension) or directory
        /// </summary>
        public const int PATHINFO_FILENAME = 8;

        /// <summary>
        /// All parts of the pathinfo returned as an associative array
        /// </summary>
        public const int PATHINFO_ALL = 15; // (a bitmask of all the above) 


        #endregion


        // php shim functions -------------------

        // bindec ( string $binary_string ) : number
        public static int bindec(string binary_string)
        {
            var s = binary_string.TrimStart('%');
            int i = Convert.ToInt32(s, 2);

            //if (i > 255)
            //    throw new OverflowException("bindec");

            return (byte)i;
        }

        /* https://www.php.net/manual/en/function.decbin.php
            decbin(int $num): string
        */
        /// <summary>
        /// Returns a string containing a binary representation of the given num argument
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string decbin(int num)
        {
            return Convert.ToString(num, 2);
        }

        /* https://www.php.net/manual/en/function.dechex.php
            dechex(int $num): string
        */
        /// <summary>
        /// Returns a string containing a hexadecimal representation of the given unsigned num argument.
        /// No leading hex identifier, value characters only
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string dechex(int num)
        {
            return num.ToString("x");
        }

        public static void echo(string s)
        {
            Console.Write(s);
        }

        //// feof ( resource $handle ) : bool
        public static bool feof(FileStream handle)
        {
            return handle.Position >= handle.Length;
        }

        public static bool file_exists(string filename)
        {
            return File.Exists(filename);
        }

        /*
        file_get_contents(
            string $filename,
            bool $use_include_path = false,
            ?resource $context = null,
            int $offset = 0,
            ?int $length = null
        ): string|false
        */
        /// <summary>
        /// Reads entire file into a string, starting at the specified offset up to length bytes.
        /// port: this is only called with filename as a parameter, other parameters not implemented.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>string or null (in place of false)</returns>
        public static string file_get_contents(string filename)
        {
            try
            {
                return File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"file_get_contents failed: {ex.Message}");
                return null;
            }
        }

        /* https://www.php.net/manual/en/function.file-put-contents.php
            file_put_contents(
                string $filename,
                mixed $data,
                int $flags = 0,
                ?resource $context = null
            ): int|false
        */
        /// <summary>
        /// Write data to a file
        /// </summary>
        /// <param name="filename">Path to the file where to write the data</param>
        /// <param name="data">The data to write. Can be either a string, an array or a stream resource</param>
        /// <param name="flags">not implemented</param>
        /// <param name="context">not implemented</param>
        /// <returns>the number of bytes that were written to the file</returns>
        public static int file_put_contents(string filename, string data, int flags = 0, object context = null)
        {
            Util.EnsureDirectory(_targetPath);

            var destFile = filename;
            if (!string.IsNullOrWhiteSpace(_targetPath))
                destFile = Path.Combine(_targetPath, filename);

            // todo FILE_APPEND flag could be implemented but it's not used in original php 
            try
            {
                File.WriteAllText(destFile, data);
                return data.Length;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //return -1;

                throw new Exception("file write error", ex);
            }
        }

        public static int file_put_contents(string filename, StringBuilder data, int flags = 0, object context = null)
        {
            return file_put_contents(filename, data.ToString());
        }

        public static int file_put_contents(string filename, byte[] data /* , int flags = 0 , resource context */ )
        {
            var destFile = filename;
            if (!string.IsNullOrWhiteSpace(_targetPath))
                destFile = Path.Combine(_targetPath, filename);

            try
            {
                File.WriteAllBytes(destFile, data);
                return data.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        /* https://www.php.net/manual/en/function.fopen.php
            fopen(
                string $filename,
                string $mode,
                bool $use_include_path = false,
                ?resource $context = null
            ): resource|false
        */
        /// <summary>
        /// binds a named resource, specified by filename, to a stream
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mode">not implemented</param>
        /// <param name="use_include_path">not implemented</param>
        /// <param name="context">not implemented</param>
        /// <returns>a file pointer resource</returns>
        public static FileStream fopen(string filename, string mode, bool use_include_path = false, object context = null)
        {
            return File.OpenRead(filename);
        }

        public static FileStream fopen(string filename, char mode, bool use_include_path = false, object context = null)
        {
            return fopen(filename, mode.ToString());
        }

        // this isn't an equivalent, just a matching-friendly way to get the data
        public static byte[] fopen_bytes(string filename, char mode)
        {
            if (!File.Exists(filename))
                return null;

            return File.ReadAllBytes(filename);
        }

        // fread ( resource $handle , int $length ) : string
        public static byte[] fread(FileStream handle, int length)
        {
            var pos = handle.Position;

            var b = new byte[length];
            handle.Read(b, 0, length);
            fseek(handle, pos + length); // todo don't need to do this manually?
            return b;
        }

        public static byte[] fread(byte[] source, int length, ref int srcIndex)
        {
            if (srcIndex + length > source.Length)
                return new byte[0];

            var dest = new byte[length];
            Array.Copy(source, srcIndex, dest, 0, dest.Length);
            srcIndex += length;
            return dest;
        }

        private static byte[] fread(byte[] source, int length, byte[] buf, ref int srcIndex)
        {
            var read = fread(source, length, ref srcIndex);

            int bufLen = 0;
            if (buf != null)
                bufLen = buf.Length;

            var dest = new byte[bufLen + read.Length];
            int destIndex = 0;
            if (buf != null)
            {
                Array.Copy(buf, 0, dest, destIndex, bufLen);
                destIndex += bufLen;
            }
            Array.Copy(read, 0, dest, destIndex, read.Length);
            return read;
        }

        /* https://www.php.net/manual/en/function.ftell.php
            ftell(resource $stream): int|false
        */
        /// <summary>
        /// Returns the position of the file pointer referenced by stream
        /// </summary>
        /// <param name="stream">The file pointer must be valid, and must point to a file successfully opened</param>
        /// <returns>Returns the position of the file pointer referenced by stream as an integer; i.e., its offset into the file stream</returns>
        public static int ftell(FileStream stream)
        {
            // php docs read like the return value is signed 32 bit and anything over 2 gb is undefined
            // shouldn't be an issue for this application, if this happens something has gone off the rails anyway

            if (stream.Position > int.MaxValue)
                throw new Exception("stream position overruns int");

            // idk what goes wrong to produce return false in php but here it's gonna throw an exception
            // original php script doesn't check for false so non-issue

            return (int)stream.Position;
        }

        // fseek ( resource $handle , int $offset [, int $whence = SEEK_SET ] ) : int
        public static int fseek(FileStream handle, long offset /* , int whence = SEEK_SET */ )
        {
            handle.Position = offset;
            return 0; // original code never checks return value, this is a dummy value
        }

        /* https://www.php.net/manual/en/function.rewind.php
            rewind(resource $stream): bool
        */
        /// <summary>
        /// Sets the file position indicator for stream to the beginning of the file stream
        /// </summary>
        /// <param name="handle"></param>
        public static bool rewind(FileStream handle)
        {
            handle.Position = 0;
            return true;
        }

        /* https://www.php.net/manual/en/function.hexdec.php
            hexdec(string $hex_string): int|float
        */
        /// <summary>
        /// Returns the decimal equivalent of the hexadecimal number represented by the hex_string argument.
        /// </summary>
        /// <param name="hex_string">The hexadecimal string to convert</param>
        /// <returns>The decimal representation of hex_string</returns>
        public static int hexdec(string hex_string)
        {
            if (string.IsNullOrWhiteSpace(hex_string))
                return 0;

            // php ignores anything non digit characters in the string
            hex_string = Regex.Replace(hex_string, @"[^a-fA-F0-9]", string.Empty);

            int i = 0;
            try
            {
                i = int.Parse(hex_string, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"can't parse '{hex_string}' as a numeric value: {ex.Message}");
            }

            // the php version returns 0 if the string isn't valid
            return i;
        }

        /* https://www.php.net/manual/en/function.array-key-exists.php
            array_key_exists(string|int|float|bool|resource|null $key, array $array): bool
        */
        /// <summary>
        /// returns true if the given key is set in the array. key can be any value possible for an array index. 
        /// </summary>
        /// <returns></returns>
        public static bool array_key_exists(string key, array array)
        {
            return array.ContainsKey(key);
        }

        /* https://www.php.net/manual/en/function.in-array.php
            in_array(mixed $needle, array $haystack, bool $strict = false): bool
        */
        /// <summary>
        /// Searches for needle in haystack using loose comparison unless strict is set
        /// </summary>
        /// <param name="needle"></param>
        /// <param name="haystack"></param>
        /// <param name="strict">not implemented</param>
        /// <returns></returns>
        public static bool in_array<T>(T needle, T[] haystack, bool strict = false)
        {
            if (haystack == null)
                return false;

            foreach (var item in haystack)
            {
                if (Equals(item, needle)) // hoping this is sufficient to identify matches without explicit types
                    return true;
            }

            return false;
        }

        // is_array ( mixed $var ) : bool
        public static bool is_array(object va)
        {
            return va is Array;
        }

        /* https://www.php.net/manual/en/function.is-object.php
            is_object(mixed $value): bool
        */
        /// <summary>
        /// Finds whether the given variable is an object
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool is_object(object value)
        {
            // not sure what php does or doesn't consider an object
            // this is the most sensible thing i can think of
            return value != null;
        }

        // isset ( mixed $var [, mixed $... ] ) : bool
        public static bool isset(Dictionary<object, object> d, object key)
        {
            // todo generic dictionary

            if (d == null)
                return false;

            return d.ContainsKey(key) && d[key] != null;
        }

        public static bool isset(string[] mixed, int index)
        {
            if (mixed != null && index < mixed.Length)
            {
                var obj = mixed[index];
                return !string.IsNullOrWhiteSpace(obj);
            }
            return false;
        }

        public static bool isset(List<string> mixed, int index)
        {
            if (mixed == null)
                return false;

            return isset(mixed.ToArray(), index);
        }

        public static bool is_numeric(object val, out int nxt)
        {
            var str = val as string;
            return int.TryParse(str, out nxt);
        }

        // microtime ([ bool $get_as_float = FALSE ] ) : mixed
        public static DateTime microtime(bool get_as_float = false)
        {
            return DateTime.Now;
        }

        /* https://www.php.net/manual/en/function.ord.php
            ord(string $character): int
        */
        /// <summary>
        /// Convert the first byte of a string to a value between 0 and 255
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static byte ord(char character)
        {
            // this method is never called with a proper string as input
            // made input type a char to simplify
            return (byte)character;
        }

        public static byte ord(params byte[] character)
        {
            return character[0];
        }


        /* https://www.php.net/manual/en/function.pathinfo.php
            pathinfo(string $path, int $flags = PATHINFO_ALL): array|string
        */
        /// <summary>
        /// Returns information about a file path
        /// </summary>
        /// <param name="path">The path to be parsed</param>
        /// <param name="flags">If present, specifies a specific element to be returned. 
        /// If not specified, returns all available elements</param>
        /// <returns></returns>
        public static string pathinfo(string path, int flags = PATHINFO_ALL)
        {
            //$path_parts = pathinfo('/www/htdocs/inc/lib.inc.php');

            //echo $path_parts['dirname'], "\n";    /www/htdocs/inc
            //echo $path_parts['basename'], "\n";   lib.inc.php
            //echo $path_parts['extension'], "\n";  php
            //echo $path_parts['filename'], "\n";   lib.inc

            switch (flags)
            {
                case PATHINFO_DIRNAME:
                    return Path.GetDirectoryName(path);

                case PATHINFO_BASENAME: // filename w/ ext
                    return Path.GetFileName(path);

                case PATHINFO_EXTENSION: // last extension
                    return Path.GetExtension(path);

                case PATHINFO_FILENAME: // filename w/o last ext
                    return Path.GetFileNameWithoutExtension(path);

                case PATHINFO_ALL:
                default: // any other combination of flags, or zero which is same as PATHINFO_ALL
                         // note: php docs don't specify what happens on OR'd flags but the original php doesn't try that
                    throw new Exception("can only do one pathinfo flag at a time");
            }
        }

        /// <summary>
        /// shorthand to test of val is a string and if so does it match the regex pattern
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool preg_match(string pattern, object val)
        {
            var s = val as string;

            if (s != null && Regex.IsMatch(s, pattern)) // TODO case insensitive? 
                return true;

            return false;
        }

        /* https://www.php.net/manual/en/function.preg-match-all.php
            preg_match_all(
                string $pattern,
                string $subject,
                array &$matches = null,
                int $flags = 0,
                int $offset = 0
            ): int|false
        */
        /// <summary>
        /// Searches subject for all matches to the regular expression given in pattern and puts them in matches in the order specified by flags.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="subject"></param>
        /// <returns>Returns the number of full pattern matches (which might be zero), or false on failure.</returns>
        public static int preg_match_all(string pattern, string subject, out List<Match> matches)
        {
            var rx = Util.ToDotNetRegex(pattern);
            var matches2 = rx.Matches(subject);

            matches = new List<Match>();

            foreach (Match m in matches2)
                matches.Add(m);

            return matches.Count;
        }

        /*
            preg_replace(
                string|array $pattern,
                string|array $replacement,
                string|array $subject,
                int $limit = -1,
                int &$count = null
            ): string|array|null
        */
        /// <summary>
        /// Searches subject for matches to pattern and replaces them with replacement
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static string preg_replace(string pattern, string replacement, string subject)
        {
            var rx = Util.ToDotNetRegex(pattern);
            var result = rx.Replace(subject, replacement);
            return result;
        }

        // strlen ( string $string ) : int
        public static int strlen(string str)
        {
            return str.Length;
        }

        /* https://www.php.net/manual/en/function.chr.php
            chr(int $codepoint): string
        */
        /// <summary>
        /// Generate a single-byte string from a number.
        /// </summary>
        /// <param name="codepoint"></param>
        /// <returns></returns>
        public static string chr(byte codepoint)
        {
            var c = (char)codepoint;
            return c.ToString();
        }

        /* https://www.php.net/manual/en/function.str-pad.php
            str_pad(
                string $string,
                int $length,
                string $pad_string = " ",
                int $pad_type = STR_PAD_RIGHT
            ): string
        */
        /// <summary>
        /// This function returns the string string padded on the left, the right, or both sides to the 
        /// specified padding length. If the optional argument pad_string is not supplied, the string 
        /// is padded with spaces, otherwise it is padded with characters from pad_string up to the limit. 
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="length">The desired length of the final padded string. 
        /// If the value of length is negative, less than, or equal to the length
        /// of the input string, no padding takes place, and string will be returned</param>
        /// <param name="pad_string">the character to use for padding</param>
        /// <param name="pad_type">which side the padding is added to</param>
        /// <returns></returns>
        public static string str_pad(string input, int length, char pad_string = ' ', int pad_type = STR_PAD_RIGHT)
        {
            // PadLeft/PadRight expect a single character for the pad value. 
            // no calls from the original php specify more than one character
            // this is the default overload to simplify

            if (length < 0 || length <= input.Length)
                return input;

            switch (pad_type)
            {
                case STR_PAD_LEFT:
                    return input.PadLeft(length, pad_string);

                case STR_PAD_BOTH:

                    // unequal padding favors right 

                    int leftpad = (length - input.Length) / 2;

                    // ? maybe reverse left/right order if not coming out right
                    //return str_pad(
                    //    str_pad(input, input.Length + (pad / 2), pad_string, PadType.STR_PAD_LEFT),
                    //    pad_length, pad_string, PadType.STR_PAD_RIGHT);

                    return input.PadLeft(input.Length + leftpad, pad_string).PadRight(length, pad_string);

                case STR_PAD_RIGHT:
                default: // php defaults to right, we'll just treat any out of range pad_type as right
                    return input.PadRight(length, pad_string);
            }
        }

        public static string str_pad(string input, int length, string pad_string, int pad_type = STR_PAD_RIGHT)
        {
            if (pad_string.Length > 1)
                throw new ArgumentException("using more than one character as the pad not implemented", "pad_string");

            return str_pad(input, length, pad_string[0], pad_type);
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
            if (input.Length == 1)
                return str_repeat(input[0], multiplier);

            var sb = new StringBuilder();
            for (int j = 0; j < multiplier; j++)
            {
                sb.Append(input);
            }

            return sb.ToString();
        }

        public static string str_repeat(char input, int multiplier)
        {
            return string.Empty.PadRight(multiplier, input);
            //return str_repeat(input.ToString(), multiplier);
        }

        // strtolower ( string $string ) : string
        public static string strtolower(string str)
        {
            return str.ToLower();
        }

        /* https://www.php.net/manual/en/function.substr.php
            substr(string $string, int $offset, ?int $length = null): string
        */
        /// <summary>
        /// Returns the portion of string specified by the offset and length parameters
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="offset">If non-negative, the returned string will start at the offset'th position in string, counting from zero.
        /// If offset is negative, the returned string will start at the offset'th character from the end of string.</param>
        /// <param name="length"> If positive, the string returned will contain at most length characters beginning from offset (depending on the length of string). 
        /// If negative, then that many characters will be omitted from the end of string. If offset denotes the position of this truncation or beyond, an empty string will be returned. 
        /// If 0, an empty string will be returned. 
        /// If omitted or null, the substring starting from offset until the end of the string will be returned. </param>
        /// <returns></returns>
        public static string substr(string input, int offset, int? length = null)
        {
            //echo substr("abcdef", -1), PHP_EOL;    // returns "f"
            //echo substr("abcdef", -2), PHP_EOL;    // returns "ef"
            //echo substr("abcdef", -3, 1), PHP_EOL; // returns "d"

            //echo substr("abcdef", 0, -1), PHP_EOL;  // returns "abcde"
            //echo substr("abcdef", 2, -1), PHP_EOL;  // returns "cde"
            //echo substr("abcdef", 4, -4), PHP_EOL;  // returns ""; prior to PHP 8.0.0, false was returned
            //echo substr("abcdef", -3, -1), PHP_EOL; // returns "de"

            if (length.HasValue && length.Value == 0)
                return string.Empty;

            int start = offset < 0
                ? input.Length + offset // 'adding' because offset is already negative
                : offset;

            if (start < 0)
                return string.Empty;

            var working = input.Substring(start);

            if (!length.HasValue)
                return working; // return entire value

            var len = length < 0
                ? working.Length + length.Value // 'adding' because length is already negative
                : length.Value;

            if (len > working.Length)
                return working; // return entire value

            if (len <= 0)
                return string.Empty; // all characters would be removed, nothing left to return

            var final = working.Substring(0, len);
            return final;
        }

        public static string substr(byte[] source, int offset, int length = 0)
        {
            // todo implement this properly

            if (length <= 0)
                length = source.Length - offset;

            var dest = new byte[length];
            Array.Copy(source, offset, dest, 0, length);

            return Encoding.ASCII.GetString(dest);
            //return dest;
        }

        public static string trim(string str)
        {
            return str?.Trim();
        }

        public static double round(TimeSpan span, int places)
        {
            var time = Math.Round((span).TotalMilliseconds / 1000, 3);
            return time;
        }

        /// <summary>
        /// destroys the specified variables.
        /// port: in this case, removing key and value from the dictionary entirely
        /// </summary>
        /// <param name="mixed"></param>
        /// <param name="key"></param>
        public static void unset(Dictionary<object, object> mixed, object key)
        {
            mixed.Remove(key);

            // port: in the php docs there are comments about indexing issues
            // no idea how to handle those, just gonna hope the code calling this doesn't have that problem
        }

        // these handle some assignments/comparisons that php does automatically

        static bool php_bytes_equal(byte[] prg0, byte[] prg1)
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

        static bool php_dictionaries_equal(Dictionary<object, object> labels, Dictionary<object, object> oldLabels)
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

        static bool php_dictionaries_equal(AsmLabels labels, AsmLabels oldLabels)
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

        #endregion

    }

    /// <summary>
    /// to approximate php's associative arrays, without having dictionary declarations everywhere
    /// </summary>
    public class array : Dictionary<object, object>
    {
        // TODO: this is generally hex string for the key, label string for the value. value can also be string[], or false
        // TODO should probably replace any false sets or checks with null

        public array()
        {

        }

        public array(Dictionary<object, object> dictionary) : base(dictionary)
        {

        }

        internal void AddRange(array labels)
        {
            var initLabels = this;

            foreach (var item in labels)
            {
                if (initLabels.ContainsKey(item.Key))
                {
                    // TODO?
                }

                if (!initLabels.ContainsKey(item.Key))
                    initLabels.Add(item.Key, item.Value);

                initLabels[item.Key] = item.Value;
            }
        }

        internal array AsCopy()
        {
            return new array(this);
        }
    }

    public class AsmLabels : Dictionary<int, List<string>>
    {
        // TODO: use this for optimized c# version 

        public AsmLabels()
        {

        }

        public AsmLabels(Dictionary<int, List<string>> dictionary) : base(dictionary)
        {

        }

        public AsmLabels(array registers)
        {
            // registers = hex string (no token), label 

            foreach (var item in registers)
            {
                int addr = da6Umbrella.hexdec((string)item.Key);
                var lbl = (string)item.Value;
                this.Add(addr, lbl);
            }
        }

        internal void AddRange(AsmLabels fileLabels)
        {
            var initLabels = this;

            foreach (var item in fileLabels)
            {
                if (initLabels.ContainsKey(item.Key))
                {
                    // TODO?
                }

                if (!initLabels.ContainsKey(item.Key))
                    initLabels.Add(item.Key, item.Value);

                initLabels[item.Key] = item.Value;
            }
        }

        internal void Add(int address, string lbl = null)
        {
            if (!this.ContainsKey(address))
                this.Add(address, new List<string>());

            if (!string.IsNullOrWhiteSpace(lbl))
            {
                var list = this[address];
                if (!list.Contains(lbl))
                {
                    list.Add(lbl);
                }
            }
        }

        internal AsmLabels AsCopy()
        {
            return new AsmLabels(this);
        }
    }
}
