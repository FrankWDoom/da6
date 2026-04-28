using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace da6
{

    public class Inputs
    {
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
        public int cdlOffset { get; set; } = 0;
        public bool ignoreWrites { get; set; } = false;
        public bool useLowerCase { get; set; } = true;
        public bool usingMapper2 { get; set; } = false;
        public bool trace { get; set; } = false;
        public string traceFilename { get; set; } = null;

        public int lastPass { get; set; } = 9;
    }
}
