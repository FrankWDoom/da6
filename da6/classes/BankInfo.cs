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
    }
}
