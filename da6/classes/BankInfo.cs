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

        public static void SetBankOrigins(List<BankInfo> bankList, byte[] prg, byte[] cdl, int cdlOffset)
        {
            if (cdl != null && cdlOffset != 0)
            {
                var cdlTmp = Util.CopyBytes(cdl, cdlOffset, prg.Length);
                cdl = cdlTmp;
            }

            int romBanks = bankList.Count;
            var romBankSize = bankList[0].BankSize;

            if (romBankSize == da6Umbrella.LEN_32K || romBankSize == da6Umbrella.LEN_16K)
            {
                // 32K: everything starts at $8000
                // 16K: same, except last bank at $C000 (set below)
                bankList.ForEach(n => n.Origin = da6Umbrella.CPU_ADDR_BASE);
            }
            else if (romBankSize == da6Umbrella.LEN_8K && cdl != null)
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
                        address = 0x8000 + (cpuBank * da6Umbrella.CPU_BANK_LEN);
                    }

                    //int subAddr = address % 0x2000;
                    //address += subAddr; // for rom banks less than 8K // todo this probably doesn't happen

                    if (address > 0)
                        info.Origin = address;

                    //Console.WriteLine($"rom bank: {j,3} cpu bank: {cpuBank,3} ${address:X4}");
                }
            }

            var last = bankList[bankList.Count - 1];
            last.Origin = (da6Umbrella.CPU_ADDR_BASE + da6Umbrella.LEN_32K) - romBankSize; // set last bank origin so vectors come out right
        }

        /// <summary>
        /// searches cdl record for the memory address of any code executed within the rom bank
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

                int cdlBank = (byteCDL & da6Umbrella.CDL_BANK_MASK) >> 2; // bank mask = b1100

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
