using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace da6
{
    public class BankInfo
    {

        /// <summary>
        /// position within prg data
        /// </summary>
        public int DataOffset
        {
            get { return this.Index * this.BankSize; }
            //set;
        }

        /// <summary>
        /// number of bytes comprising a bank size within the rom
        /// </summary>
        public int BankSize { get; set; }

        /// <summary>
        /// rom bank number according to rom bank size
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// start address of prg space data is mapped into
        /// </summary>
        public int Origin { get; set; }

        /// <summary>
        /// true when last 6 bytes are vector values
        /// </summary>
        public bool HasVectors { get; set; }

        public List<int> EntryPoints { get; set; } = new List<int>();


        public int EndOfBank { get; set; }

        public bool IsFixed { get; set; }


        public bool AddressInRange(int addr)
        {
            return this.Origin <= addr && addr <= this.EndOfBank;

            //if (addr < this.Origin)
            //    return false;

            //if (!HasVectors)
            //    return addr <= this.EndofBank;

            //// has vectors
            //return addr <= this.EndofBank - 6;
        }


        public override string ToString()
        {
            return $"#:{Index,2} loc:0x{DataOffset,5:x4} org:${Origin,4:x4} v:{HasVectors}";
        }

        /// <summary>
        /// mark banks as having vectors based on mapper in use
        /// </summary>
        /// <param name="banksList"></param>
        /// <param name="mapperNumber"></param>
        public static void SetBankVectorsFlag(List<BankInfo> banksList, int mapperNumber)
        {
            if (mapperNumber == 1)
            {
                // for mmc1 no bank is guaranteed to be in place at power on
                // all banks should have vectors (with identical code in identical locations?)
                banksList.ForEach(n => n.HasVectors = true);
            }
            else if (mapperNumber == 5)
            {
                // mmc5 $E000 is always bankable, not sure what initial state is TODO
                // going to assume last bank has vectors and not assume anything else 

                var last = banksList[banksList.Count - 1];
                last.HasVectors = true;
            }
            else if (mapperNumber == 180)
            {
                // crazy climber, $E000 is bankable, not sure what initial state is TODO
                // $8000 fixed to first bank
                // going to assume last bank has vectors, first bank does not, and not assume anything else 

                var first = banksList[0];
                first.HasVectors = false;

                // todo might be all banks after first have a copy of vectors

                var last = banksList[banksList.Count - 1];
                last.HasVectors = true;
            }
            else
            {
                // pretty much everything else will be last bank only
                var last = banksList[banksList.Count - 1];
                last.HasVectors = true; // almost always
            }
        }

    }
}
