using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace da6
{

    public class Inputs
    {
        const int LEN_32K = da6Umbrella.LEN_32K;
        const int CPU_ADDR_BASE = da6Umbrella.CPU_ADDR_BASE;
        const int CPU_ADDR_END = CPU_ADDR_BASE + LEN_32K;


        public string filename { get; set; }

        public bool showHeader { get; set; } = true;
        public bool includeChr { get; set; } = false;
        public bool includeReg { get; set; } = false;
        public bool originOverride { get; set; } = false;
        public bool noDetect { get; set; } = false;
        public string targetPath { get; set; } = string.Empty;
        public string shortname { get; set; } // = pathinfo(filename, PhpPathInfo.PATHINFO_FILENAME);
        public string labelFile { get; set; } = null;
        public string cdlFilename { get; set; } = null;
        public bool ignoreHeader { get; set; } = false;
        public int fileStart { get; set; } = 0;
        public bool fileStartOverride { get; set; } = false;
        public int fileLength { get; set; } = 0x10000;
        public bool lengthOverride { get; set; } = false;
        public int fileEnd { get; set; } = 0;
        public bool fileEndOverride { get; set; } = false;
        public int codeStart { get; set; } = 0;
        public bool codeStartOverride { get; set; } = false;
        public int codeEnd { get; set; } = 0;
        public bool codeEndOverride { get; set; } = false;
        public int bankNumber { get; set; } = -1;
        public int cdlOffset { get; set; } = 0;
        public bool ignoreWrites { get; set; } = false;
        public bool useLowerCase { get; set; } = true;
        public bool usingMapper2 { get { return this.mapperOverride && this.mapperNumber == 2; } }
        public int mapperNumber { get; set; } = 0;
        public bool mapperOverride { get; set; } = false;
        public bool trace { get; set; } = false;
        public string traceFilename { get; set; } = null;

        public int lastPass { get; set; } = 9;

        /// <summary>
        /// determine file range values based on user inputs
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fstartOver"></param>
        /// <param name="fstart"></param>
        /// <param name="fendOver"></param>
        /// <param name="fend"></param>
        /// <param name="flenOver"></param>
        /// <param name="flen"></param>
        internal void SetFileRange(
            byte[] buffer, bool fstartOver, int fstart, bool fendOver, int fend, bool flenOver, int flen)
        {
            int bufferLen = buffer.Length;

            int dataStart = 0; // port: dataStart is an absolute file position
            int dataEnd = 0;
            int dataLen = bufferLen;

            if (fstartOver || fendOver || flenOver)
            {
                // manually take a slice of the input file to operate on 
                // how the contents are divided depend on header, mapper, chr, etc. all can still be present within data slice

                if (fstartOver)
                    dataStart = fstart;

                dataLen = bufferLen - dataStart;
                if (fendOver) // file end has priority over length if both are specified
                {
                    dataLen = fend - dataStart;
                }
                else if (flenOver)
                {
                    dataLen = flen;
                }

                flenOver = true; // this is the flag to check later on if file range is a consideration

                dataEnd = dataStart + dataLen;
                fendOver = true;

                if (dataEnd > bufferLen)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("calculated file data range exceeds actual file:");
                    msg.AppendLine($" fileStart: 0x {dataStart,6:X4}");
                    msg.AppendLine($"fileLength: 0x {  dataLen,6:X4}");
                    msg.AppendLine($"   fileEnd: 0x {  dataEnd,6:X4}");
                    msg.AppendLine($"actual end: 0x {bufferLen,6:X4}");
                    throw new Exception(msg.ToString());
                }
            }

            dataEnd = dataStart + dataLen; // dataEnd might be 0 here, make sure we have a proper value going forward

            this.fileStartOverride = fstartOver;
            this.fileStart = dataStart;

            this.fileEndOverride = fendOver;
            this.fileEnd = dataEnd;

            this.lengthOverride = flenOver;
            this.fileLength = dataLen;
        }

        // port: start offset for prg within file depending on header presence, 0 or 0x10 (HDR_LEN)
        public int prgOffset { get; set; } = 0;
        public int prgLen { get; set; }
        public int romBankSize { get; set; } = da6Umbrella.LEN_32K; // default size

        /// <summary>
        /// determine prg offset and length within file
        /// </summary>
        /// <param name="fileBuffer"></param>
        /// <param name="userNoDetect"></param>
        /// <param name="ignoreHdr"></param>
        /// <param name="info"></param>
        /// <param name="mapOver"></param>
        /// <param name="mapNum"></param>
        internal void SetPRGRange(byte[] fileBuffer, bool userNoDetect, bool ignoreHdr, HeaderInfo info, bool mapOver, int mapNum)
        {
            int bufferLen = fileBuffer.Length;

            this.prgOffset = 0; // port: start offset for prg within file depending on header presence, 0 or 0x10 (HDR_LEN)
            this.prgLen = bufferLen; // until determined otherwise

            this.mapperNumber = 0;
            if (this.mapperOverride = mapOver)
                this.mapperNumber = mapNum;

            this.ignoreHeader = ignoreHdr;

            if (!this.ignoreHeader && info != null)
            {
                if (!this.mapperOverride)
                    this.mapperNumber = info.mapper;

                this.prgOffset = da6Umbrella.HDR_LEN; // prg data starts after the header
                this.prgLen = info.prg * da6Umbrella.LEN_16K; // header prg values is # of 16K banks regardless of mapper
            }

            // validate ranges/bounds TODO
            if (bufferLen < this.prgOffset + this.prgLen)
            {
                // TODO? this would only happen if the header is present but the file has been cut down into the prg rom
                var msg = new StringBuilder();
                msg.AppendLine($"PRG data length overruns file data available");
                msg.AppendLine($"Use -i to use specified file data range as PRG, ");
                msg.AppendLine("or specify range after header and within PRG ROM.");
                throw new Exception(msg.ToString());
            }

            // todo any mapper that supports prg banking
            if (this.prgLen > da6Umbrella.LEN_32K && !this.mapperOverride) // prg length too long to fit everything in address space
            {
                // port: TODO support other mappers
                throw new Exception("PRG length requires a mapper, use mapper2 option");
            }

            // check for 16k roms // the original code explicitly checked for two equal 0x4000 blocks
            if (!(this.noDetect = userNoDetect) && this.prgLen == LEN_32K)
            {
                if (da6Umbrella.IsDataRepeated(fileBuffer, this.prgOffset, this.prgLen)) // check for mirrored data in prg
                {
                    // remap to last half
                    var half = this.prgLen / 2;

                    this.prgOffset += half;
                    this.prgLen = half;

                    da6Umbrella.echo("PRG ROM is mirrored, overdump suspected, use -d to disable check\n");
                    da6Umbrella.echo_line($"treating as 16K ROM");
                }
            }

            this.romBankSize = HeaderInfo.GetBankSize(this.mapperNumber);

            // for sub 32K games
            if (this.prgLen < this.romBankSize) // rom bank size bigger thank actual rom size
            {
                this.romBankSize = this.prgLen;
            }
        }


        public int origin { get; set; } = 0x8000;
        public int prgStartIndex { get; set; } = 0;

        /// <summary>
        /// determine codes start/stop and origin
        /// </summary>
        /// <param name="prgBuffer"></param>
        /// <param name="userOverOrg"></param>
        /// <param name="userOrg"></param>
        /// <param name="userOverCS"></param>
        /// <param name="userCS"></param>
        /// <param name="userOverCE"></param>
        /// <param name="userCE"></param>
        /// <param name="userBankNum"></param>
        internal void SetCodeRange(byte[] prgBuffer, bool userOverOrg, int userOrg, bool userOverCS, int userCS, bool userOverCE, int userCE, int userBankNum)
        {
            // TODO finish all this

            int bufferOffset = this.prgStartIndex; // 0;
            int bufferLength = prgBuffer.Length;

            // establish origin by buffer size

            this.originOverride = false;
            this.origin = CPU_ADDR_BASE;


            if (userOverOrg)
            {
                this.originOverride = true;
                this.origin = userOrg;
            }
            else if (bufferLength < LEN_32K)
            {
                // input file data could be undersized for whatever reason
                // if user hasn't specified the origin, assume prg will be 
                // loaded to end of cpu space and set origin accordingly

                if (!userOverOrg)
                    this.origin = CPU_ADDR_END - bufferLength;
            }


            // determine where to start in prg based on codestart/end

            int cpuStart = this.origin;
            int cpuEnd = -1; // cpuStart + bufferLength;
            int cpuLen = -1;

            if (userOverCS)
            {
                cpuStart = userCS;

                if (userOverCE)
                {
                    cpuEnd = userCE;
                }
                else
                {
                    //cpuEnd = cpuStart + bufferLength;
                }
            }
            else if (userOverCE)
            {
                cpuEnd = userCE;
            }

            if (cpuEnd > -1)
                cpuLen = cpuEnd - cpuStart;


            if (userOverCS || userOverCE)
            {
                // without a defined prg structure, assume prg byte 0 is origin and content is linear to the end

                int cpuOffset = cpuStart - this.origin;
                bufferOffset += cpuOffset;

                if (bufferLength > LEN_32K)
                {
                    // could have multiple banks with code that runs at codeStart, need bank information 
                    if (userBankNum < 0)
                        throw new NotImplementedException("bank # required");
                }
                else if (userBankNum > -1)
                {
                    // 32K or less TODO ignore user bank selection?
                }

                int bankOffset = 0;
                if (userBankNum > -1)
                {
                    // determine closest base address to codeStart and assume the bank # starts at that base
                    // eg if codeStart is $f000, 
                    // codeOffset could be 0x7000, 0x3000, 0x1000 for 32/16/8K banks
                    // then base would be $8000, $C000, or $E000

                    bankOffset = userBankNum * this.romBankSize;
                }

                bufferOffset += bankOffset;

                if (cpuLen > -1 && bufferLength < bufferOffset + cpuLen)
                    da6Umbrella.echo_line("code end overruns available PRG ROM");
            }

            //if (!userOverCE && cpuEnd > CPU_ADDR_END)
            //    cpuEnd = CPU_ADDR_END;

            if (this.codeStartOverride = userOverCS)
            {
                this.codeStart = cpuStart;

                this.originOverride = true;
                this.origin = cpuStart; // codeStart supercedes calculated or provided origin
            }

            if (this.codeEndOverride = userOverCE)
                this.codeEnd = cpuEnd;


            this.bankNumber = userBankNum;
            this.prgStartIndex = bufferOffset;

            if (this.prgStartIndex < 0)
                throw new Exception($"invalid prgStartIndex value: 0x{this.prgStartIndex:X5}");
        }
    }
}
