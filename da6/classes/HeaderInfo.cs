using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace da6
{
    public class stdClass : HeaderInfo
    {
        // stdClass is only used once in the original code, for the header object
        // implementing it here specifically as a header obj for code matching
    }

    public class HeaderInfo
    {
        public byte[] head { get; set; }
        public byte prg { get; set; }
        public byte chr { get; set; }
        public byte ctrl_1 { get; set; }
        public byte ctrl_2 { get; set; }
        public byte[] tail { get; set; }

        public byte mirroring { get; set; }
        public byte sram { get; set; }
        public byte trainer { get; set; }
        public byte fourscreen { get; set; }

        public int romtype { get; set; }
        public int mapper { get; set; }

        /// <summary>
        /// break up total prg into chunks sized to match mapper bank size
        /// </summary>
        /// <param name="prg"></param>
        /// <param name="size">0 for auto</param>
        /// <returns></returns>
        public List<byte[]> SlicePrg(byte[] prg, int size = 0)
        {
            if (size == 0)
            {
                size = GetBankSize(this.mapper);
            }

            var list = new List<byte[]>();

            if (prg.Length < size)
            {
                list.Add(prg.ToArray());
                return list;
            }

            for (int j = 0; j < prg.Length; j += size)
            {
                var b = new byte[size];
                Array.Copy(prg, j, b, 0, size);
                list.Add(b);
            }

            return list;
        }

        internal List<byte[]> SliceCdl(byte[] rawCdl, List<byte[]> prgSlices)
        {
            if (rawCdl == null)
                return null;

            var list = new List<byte[]>();
            int j = 0;

            foreach (var slice in prgSlices)
            {
                var cdl = new byte[slice.Length];
                Array.Copy(rawCdl, j, cdl, 0, cdl.Length);
                j += cdl.Length;

                list.Add(cdl);
            }

            // should be chr data left over in rawCdl

            return list;
        }


        public static int GetBankSize(int mapper)
        {
            int size = 0;

            switch (mapper)
            {
                default: // todo
                case 0: // nrom
                case 3: // cnrom
                    size = da6Umbrella.LEN_32K; // 0x8000; // 32K unbanked. actual prg length might be less 
                    break;

                case 1: // mmc1 todo variable
                    size = da6Umbrella.LEN_32K; // 0x8000;
                    break;

                case 2: // unrom
                    size = da6Umbrella.LEN_16K; // 0x4000;
                    break;

                case 4: // mmc3
                    size = da6Umbrella.LEN_8K; // 0x2000; 
                    break;
            }

            return size;
        }

        internal int GetBankSize()
        {
            var romBankSize = HeaderInfo.GetBankSize(this.mapper);
            return romBankSize;
        }
    }
}
