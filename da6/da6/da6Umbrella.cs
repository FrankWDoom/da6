using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace da6
{
    /// <summary>
    /// this is the parent class to all the disassembly stuff. it constains contants and other shared methods created for the .net codebase. 
    /// the actual disassembler code, both baseline and updateabl versions, are nested within da6Umbrella 
    /// in order to have access to these shared resources without having qualifying classnames everywhere.
    /// </summary>
    public partial class da6Umbrella
    {
        // ines header length

        public const byte HDR_LEN = 0x10;

        // 6502 vector addresses

        const int V_NMI = 0xFFFA; // start of vectors
        const int V_RESET = 0xFFFC;
        const int V_IRQ_BRK = 0xFFFE;

        // addressing modes (arbitrary? from the original php)

        const int DAT = -1; // .hex ?
        const int IMM = 0;
        const int ABS = 1;
        const int ABS_X = 2;
        const int ABS_Y = 3;
        const int ZP = 4;
        const int ZP_X = 5;
        const int ZP_Y = 6;
        const int IND_X = 7;
        const int IND_Y = 8;
        const int IND_JP = 9;
        const int JP = 10;
        const int TBL_RTS = 11;
        const int TBL_JP = 12;
        const int TBL_LO = 13;
        const int TBL_HI = 14;



        // cdl bit masks
        // bytes replace repeated calls to bindec() throughout code
        // http://www.fceux.com/web/help/fceux.html?CodeDataLogger.html

        const byte CDL_CODE = 0x01; // bindec('00000001')
        const byte CDL_DATA = 0x02; // bindec('00000010')
        const byte CDL_BANK_MASK = 0x0C; // 1100
        const byte CDL_IND_DATA = 0x10;
        const byte CDL_IND_CODE = 0x20;
        const byte CDL_PCM_DATA = 0x40;

        static readonly byte CDL_ANY_CODE = (CDL_CODE | CDL_IND_CODE);
        static readonly byte CDL_ANY_DATA = (CDL_DATA | CDL_IND_DATA | CDL_PCM_DATA);

        const int CPU_BANK_LEN = LEN_8K; // size of cpu address space used by cdl bank mask

        // common bank sizes for nes games

        internal const int LEN_8K = 0x2000;
        internal const int LEN_16K = 0x4000;
        internal const int LEN_32K = 0x8000;

        internal const int HEX_ALIGN = 2; // 2 digit places per byte
        internal const int BIN_ALIGN = 8; // 8 digit places per byte

        internal const int CPU_ADDR_BASE = 0x8000; // starting address for prg rom memory space

        internal static string _targetPath = null; // port addition // todo
                                                   //private static string _workingPath = null; // port addition // todo


        /// <summary>
        /// pad a number string to byte width
        /// </summary>
        /// <param name="numberStr">printed number</param>
        /// <param name="byteAlign">number of digits representing one byte</param>
        /// <param name="padChar">padding character (default '0')</param>
        /// <returns></returns>
        internal static string PadToByteWidth(string numberStr, int byteAlign, char padChar = '0')
        {
            var ctBytes = numberStr.Length / byteAlign; // number of bytes currently represented
            if (numberStr.Length % byteAlign != 0)
            {
                ctBytes++; // align to next full byte width
            }

            var padded = numberStr.PadLeft(ctBytes * byteAlign, padChar);
            return padded;
        }

        public static string ToLabel(int address, int bank = -1)
        {
            var bankSub = string.Empty;
            if (bank > -1)
            {
                var bankHex = bank.ToString("x");
                bankSub = $"_{PadToByteWidth(bankHex, HEX_ALIGN)}";
            }

            var addressHex = address.ToString("x");
            return $"_{bankSub}_{PadToByteWidth(addressHex, HEX_ALIGN)}";
        }

        /// <summary>
        /// check for address label string or string array and return the first valid label.
        /// </summary>
        /// <param name="labelsEntry">mixed use object</param>
        /// <returns></returns>
        internal static string FirstLabel(object labelsEntry)
        {
            // this method exists to manage output issues that come from php's type flexibility
            // anywhere this method is called is a change from the original php

            var arr = labelsEntry as string[];
            if (arr != null)
            {
                if (arr.Length > 0)
                    return arr[0];

                return null;
            }

            // null is valid for a string but we only want to use s if it's an actual string
            var s = labelsEntry as string;
            if (s != null)
            {
                return s;
            }

            // value could be bool but that it usually checked for before this method is called
            // unlikely to get to this point but it's probably an actual null string instance
            // if not, it'll be a surprise for everyone
            return Convert.ToString(labelsEntry);
        }

        internal static string FirstLabel(List<string> labelsEntry, int counter = -1)
        {
            if (labelsEntry != null && labelsEntry.Count > 0)
            {
                return labelsEntry[0];
            }

            if (counter > -1)
            {
                return ToLabel(counter); // default if no explicit label created
            }

            return null;
        }

        /// <summary>
        /// get all labels as an array from the single value/list provided
        /// </summary>
        /// <param name="labelsEntry">mixed use object</param>
        /// <returns>string array of labels (may be empty)</returns>
        internal static string[] AllLabels(object labelsEntry)
        {
            // this method exists to manage output issues that come from php's type flexibility
            // anywhere this method is called is a change from the original php

            var arr = PushArray(labelsEntry); // shortcut to create/ensure array 
            return arr;
        }

        /// <summary>
        /// adds a label entry to the collection. if the existing collection is a single label,
        /// an array will be created with the existing label and new label.
        /// null/empty labels will not be added, and the result will be an array with the existing labels (if any).
        /// </summary>
        /// <param name="labelsEntry">label collection</param>
        /// <param name="newVal">label to add</param>
        /// <returns></returns>
        internal static string[] PushArray(object labelsEntry, object newVal = null)
        {
            var items = new List<string>();

            var a_labels_vector = labelsEntry as string[];
            if (a_labels_vector != null)
            {
                items.AddRange(a_labels_vector);
            }

            var s_labels_vector = labelsEntry as string;
            if (s_labels_vector != null)
            {
                // in case it's just one label and the array part hasn't been created yet
                items.Add(s_labels_vector);
            }

            var s_val = newVal as string;
            if (s_val != null)
            {
                items.Add(s_val);
            }

            return items.ToArray();
        }

        public static byte byt(int val)
        {
            // this exists to cut down on some repetitive casting
            return (byte)val;
        }

    }
}
