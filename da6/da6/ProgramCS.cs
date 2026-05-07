using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace da6
{
    partial class da6Umbrella
    {

        /*
        static Inputs GetArgs(string[] argv)
        {
            var inputs = new Inputs() { };

            int lastPass = 9;

            inputs.filename = argv[0];
            inputs.shortname = pathinfo(inputs.filename, PhpPathInfo.PATHINFO_FILENAME);

            // check command line params
            for (var i = 1; i < argv.Length; i++) // filename is arg 0, anything else starts at 1
            {
                int lookAhead = i + 1;
                string nextParam = null;

                if (lookAhead < argv.Length && substr(argv[lookAhead], 0, 1) != "-")
                {
                    nextParam = argv[lookAhead]?.Trim();
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

                        // note codestart takes priority over filestart

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

                        inputs.fileLength = baseToDec(argv[++i]); // will NOT be tweaked since lengthOverride isn't enable // todo what?
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

                        var target = argv[++i];

                        //var path = Regex.Replace(target, @"[^a-zA-Z0-9_\-\. ]", "");
                        //inputs.shortname = pathinfo(path, PhpPathInfo.PATHINFO_FILENAME);

                        inputs.targetPath = Path.GetDirectoryName(target);
                        inputs.shortname = string.Join("_", Path.GetFileNameWithoutExtension(target).Split(Path.GetInvalidFileNameChars()));

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

                        inputs.labelFile = nextParam;
                        ++i;

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

                    case "-uc": // port: php source has '-cc', assuming intended to be '-uc'
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

                    case "-xt":
                    case "-trace":

                        inputs.trace = true;

                        if (File.Exists(nextParam))
                        {
                            inputs.traceFilename = nextParam;
                        }

                        break;
                }

            }

            return inputs;
        }
        */

        /*
        static void PrintInputs(Inputs inputs, int origin, HeaderInfo headerInfo)
        {
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

        }
        */

        /*
        static array LoadFileLabels(string labelFile)
        {
            array fileLabels = null;

            if (labelFile != null)
            {
                fileLabels = readLabels(labelFile);

                //$mapperArr = $fileLabels['mapperArr'];
                //unset($fileLabels['mapperArr']);

                labelLen = (int)fileLabels["maxLength"] - 10;
                labelLen = labelLen < 0 ? 0 : labelLen;
                //unset(fileLabels["maxLength"]);
                fileLabels["maxLength"] = null;

                //foreach (var item in fileLabels)
                //{
                //    initLabels.Add(item.Key, item.Value);
                //}

            }

            return fileLabels;
        }
        */

        /*
        static bool[] TracePrg(byte[] slice, BankInfo bankInfo, List<int> entryPoints, byte[] cdl, string _filename)
        {
            var sb = new StringBuilder();

            var jumps = new[]
            {
                "JSR", "JMP",
                "RTS", "RTI",
                "BRK", // todo?
            };

            var branches = new[]
            {
                "BCC", "BCS",
                "BEQ", "BMI",
                "BNE", "BPL",
                "BVC", "BVS",
            };

            int ep = 0;

            var codeMask = new bool[slice.Length];
            var dataPoints = new List<int>();


            if (cdl != null)
            {
                bool code = false;
                int j = 0;

                // run through cdl and add entry points where a new run of code bytes start

                while (j < cdl.Length)
                {
                    if ((cdl[j] & CDL_CODE) != 0)
                    {
                        if (!code)
                        {
                            // previously data, new code sequence
                            entryPoints.Add(bankInfo.Origin + j);
                        }
                        code = true;
                    }
                    else
                    {
                        code = false;
                    }

                    j++;
                }
            }

            while (ep < entryPoints.Count)
            {
                int entry = entryPoints[ep];
                ep++;


                if (entry == 0xccd8 || entry == 0xccc9)
                {

                }

                sb.AppendLine();
                sb.Append($"tracing at address ${entry:X4}...");

                //if (addr < bankInfo.Origin || addr > bankInfo.EndOfBank)
                if (!bankInfo.AddressInRange(entry))
                {
                    sb.Append(" out of bank range"); sb.Append(Environment.NewLine);
                    continue; // todo targets outside bank
                }

                int addr = entry;
                int offset = addr - bankInfo.Origin;

                // gradius 8bf2 data

                byte c = 0;

                if (cdl != null)
                {
                    c = cdl[offset];

                    if ((c & (CDL_CODE | CDL_IND_CODE)) == 0)
                    {
                        if ((c & CDL_IND_DATA) != 0)
                        {
                            //sb.Append($"  ${addr:X4} is marked as indirect data in cdl, skipping"); sb.Append(Environment.NewLine);
                            //continue;
                        }

                        if ((c & CDL_PCM_DATA) != 0)
                        {
                            //sb.Append($"  ${addr:X4} is marked as pcm data in cdl, skipping"); sb.Append(Environment.NewLine);
                            //continue;
                        }

                        if ((c & CDL_ANY_DATA) != 0)
                        {
                            sb.Append($"  ${addr:X4} is marked as data in cdl, skipping"); sb.Append(Environment.NewLine);
                            continue;
                        }
                    }
                }

                if (dataPoints.Contains(addr))
                {
                    // todo
                    sb.Append($"  ${addr:X4} exists in data points list, skipping"); sb.Append(Environment.NewLine);
                    continue;
                }

                sb.Append(Environment.NewLine);


                bool cleanCode = true;

                int branchAddr = -1;
                int continueAddr = -1;
                var foundRW = new List<int>(); // todo
                int dataCount = 0;

                int next = 0;

                while (true)
                {
                    // process bytes as code until jump/branch or illegal op hit

                    var cease = false; // stop processing after this op

                    offset = addr - bankInfo.Origin;
                    var opcode = slice[offset];
                    var cmd = opcodes[opcode];

                    c = cdl != null ? cdl[offset] : (byte)0;

                    if ((c & CDL_ANY_CODE) == 0)
                    {
                        // not flagged as code in cdl

                        if ((c & CDL_ANY_DATA) != 0)
                        {
                            // but is flagged as data
                            dataCount++;
                            cleanCode = false;
                        }
                    }

                    next = addr + cmd.Bytes;

                    if (cmd.Legal == 1) // port note: 1=illegal
                    {
                        cleanCode = false;

                        sb.Append($"  ${addr:X4} illegal opcode {cmd.Text}(0x{opcode:X2}), ");

                        //if (addr == entryPoints[ep - 1]) {
                        // unlikely a code block start with an illegal op, stop processing
                        //sb.Append($"  breaking off processing of this code block"); sb.Append(Environment.NewLine);
                        //break;
                        //}

                        if ((c * CDL_ANY_CODE) == 0)
                        {
                            // not flagged as code in cdl
                            sb.AppendLine("cdl does not flag as code, stopping");
                            break;
                        }

                        sb.AppendLine("cdl flagged as code, not really stopping");
                    }

                    if (cmd.Bytes == 3 && !jumps.Contains(cmd.Text))
                    {
                        //   check for load/store to addresses in entry points list for suspected data
                        // todo check for direct vs indexed

                        var target = hexdec(wordStr(slice, offset + 1));
                        if (target >= 0x6000 && target <= 0xffff) // todo range?
                        {
                            foundRW.Add(target);
                        }
                    }

                    //// mark bytes as code
                    //for (int rb = 0; rb < cmd.Bytes; rb++)
                    //{
                    //    //if (codeMask[offset + rb])
                    //    //{
                    //    //    cease = true; // this code has already been looked at, don't continue past this op
                    //    //}

                    //    codeMask[offset + rb] = true;
                    //}


                    if (jumps.Contains(cmd.Text)) // op = jump/return
                    {
                        sb.Append($"  ${addr:X4} {cmd.Text} (0x{opcode:X2})  "); sb.Append(Environment.NewLine);

                        // end current trace after this op
                        // jsr follow up will be new entry point

                        if (cmd.Bytes > 1)
                        {
                            // jmp or jsr with 2 byte target address

                            if (opcode != 0x6c) // indirect jsr, usually from ram, unlikely to know destination
                            {
                                var target = hexdec(wordStr(slice, offset + 1));
                                branchAddr = target;

                                //// jsr or direct jmp
                                //if (!entryPoints.Contains(target))
                                //{
                                //    sb.Append($"    add jump to entry point ${target:X4} "); sb.Append(Environment.NewLine);
                                //    if (ep == entryPoints.Count)
                                //        entryPoints.Add(target); // at end of list, add to end
                                //    else
                                //        entryPoints.Insert(ep, target);
                                //}
                            }
                        }

                        if (opcode == 0x20)
                        {
                            // jsr, which should continue here after executing
                            // add new entry point for continuation

                            var target = next;
                            continueAddr = target;

                            //if (!entryPoints.Contains(target))
                            //{
                            //    sb.Append($"    add continuation entry point ${target:X4} "); sb.Append(Environment.NewLine);
                            //    entryPoints.Add(target);
                            //}
                        }
                        else
                        {
                            if (cdl != null && (cdl[next - bankInfo.Origin] | CDL_ANY_CODE) != 0)
                            {
                                // this op ends execution here but next byte is flagged as code, add as entry point
                                continueAddr = next;
                            }
                        }

                        // other commands end current code sequence with this op

                        break; // stop executing, continuation will process as its own code block
                    }
                    else if (branches.Contains(cmd.Text)) // op = conditional branch
                    {
                        // all should be 2 bytes

                        var delta = slice[offset + 1];
                        sb.Append($"  ${addr:X4} {cmd.Text} 0x{delta:X2}  "); sb.Append(Environment.NewLine);

                        if (delta == 0xff)
                        {
                            // this would be branch into self, which gets ugly 
                            sb.Append($"encountered branch into self at ${addr:X4}, stopping" + Environment.NewLine);
                            cleanCode = false;
                            break;
                        }

                        var target = next;
                        if (!entryPoints.Contains(target))
                        {
                            //entryPoints.Add(target);
                            //sb.Append($"    add continuation entry point ${target:X4} " + Environment.NewLine);

                            continueAddr = target;
                        }

                        if (delta == 0xfe)
                        {
                            sb.Append($"encountered branch to same inst at ${addr:X4}, could be infinite loop" + Environment.NewLine);
                            // do not add an entry point, already marked as code
                        }

                        target += delta; // go forward
                        if (delta >= 0x80)
                        {
                            target -= 0x100; // go back
                        }

                        branchAddr = target;
                        //if (!entryPoints.Contains(target))
                        //{
                        //    sb.Append($"    add branch to entry point ${target:X4} " + Environment.NewLine);
                        //    if (ep == entryPoints.Count)
                        //        entryPoints.Add(target); // at end of list, add to end
                        //    else
                        //        entryPoints.Insert(ep, target);
                        //}


                        break; // stop executing, let next inst pick up from added entry point
                    }

                    if (cease || !bankInfo.AddressInRange(next))
                    {
                        //   op runs into known code = stop
                        // out of range // todo new entry point for other bank?
                        break;
                    }

                    addr = next; // advance addr to next op 
                }


                if (cleanCode)
                {

                    // mark block as code

                    int seqLen = next - entry;

                    offset = entry - bankInfo.Origin;
                    for (int a = 0; a < seqLen; a++)
                    {
                        codeMask[offset + a] = true;
                    }

                    // add new data points

                    dataPoints.AddRange(foundRW);

                    // add new entry points

                    if (branchAddr > -1 && !entryPoints.Contains(branchAddr))
                    {
                        sb.Append($"    add external entry point ${branchAddr:X4} " + Environment.NewLine);
                        if (ep == entryPoints.Count)
                            entryPoints.Add(branchAddr); // at end of list, add to end
                        else
                            entryPoints.Insert(ep, branchAddr);
                    }

                    if (continueAddr > -1 && !entryPoints.Contains(continueAddr))
                    {
                        sb.Append($"    add continuation entry point ${continueAddr:X4} " + Environment.NewLine);
                        entryPoints.Add(continueAddr); // at end of list, add to end
                    }

                }
                else
                {

                    sb.AppendLine($"  potential issues, not marking as code");

                    if (dataCount > 0)
                    {
                        sb.AppendLine($"  {dataCount} data bytes in sequence");
                    }

                    // todo?
                }

            }

            File.WriteAllText(Path.GetFileNameWithoutExtension(_filename) + "-trace.txt", sb.ToString());

            return codeMask;
        }
*/

        internal static void TracePrg(BankInfo bankInfo, List<byte[]> prgSlices, List<byte[]> cdlSlices, List<int> entryPoints, List<bool[]> codeMasks, string _filename)
        {
            var sb = new StringBuilder();

            var jumps = new[]
            {
                "JSR", "JMP",
                "RTS", "RTI",
                "BRK", // todo?
            };

            var branches = new[]
            {
                "BCC", "BCS",
                "BEQ", "BMI",
                "BNE", "BPL",
                "BVC", "BVS",
            };

            int ep = 0;


            foreach (var item in prgSlices)
            {
                codeMasks.Add(new bool[item.Length]);
            }


            byte[] slice = prgSlices[0];
            byte[] cdl = cdlSlices?.First();

            var codeMask = codeMasks[0];
            var dataPoints = new List<int>();


            if (cdl != null)
            {
                bool code = false;

                // run through cdl and add entry points
                // assume anywhere a new run of code bytes start is the jump-in point

                for (int j = 0; j < cdl.Length; j++)
                {
                    if ((cdl[j] & CDL_CODE) != 0)
                    {
                        if (!code)
                        {
                            // previously data, new code sequence
                            entryPoints.Add(bankInfo.Origin + j);
                        }
                        code = true; // sequence continues
                    }
                    else
                    {
                        code = false; // sequence terminated
                    }
                }

            }

            while (ep < entryPoints.Count)
            {
                int entry = entryPoints[ep];
                ep++;


                if (entry == 0xccd8 || entry == 0xccc9)
                {

                }

                sb.AppendLine();
                sb.Append($"tracing at address ${entry:X4}...");

                //if (addr < bankInfo.Origin || addr > bankInfo.EndOfBank)
                if (!bankInfo.AddressInRange(entry))
                {
                    sb.Append(" out of bank range"); sb.Append(Environment.NewLine);
                    continue; // todo targets outside bank
                }

                int addr = entry;
                int offset = addr - bankInfo.Origin;

                // gradius 8bf2 data

                byte c = 0;

                if (cdl != null)
                {
                    c = cdl[offset];

                    if ((c & (CDL_CODE | CDL_IND_CODE)) == 0)
                    {
                        if ((c & CDL_IND_DATA) != 0)
                        {
                            //sb.Append($"  ${addr:X4} is marked as indirect data in cdl, skipping"); sb.Append(Environment.NewLine);
                            //continue;
                        }

                        if ((c & CDL_PCM_DATA) != 0)
                        {
                            //sb.Append($"  ${addr:X4} is marked as pcm data in cdl, skipping"); sb.Append(Environment.NewLine);
                            //continue;
                        }

                        if ((c & CDL_ANY_DATA) != 0)
                        {
                            sb.Append($"  ${addr:X4} is marked as data in cdl, skipping"); sb.Append(Environment.NewLine);
                            continue;
                        }
                    }
                }

                if (dataPoints.Contains(addr))
                {
                    // todo
                    sb.Append($"  ${addr:X4} exists in data points list, skipping"); sb.Append(Environment.NewLine);
                    continue;
                }

                sb.Append(Environment.NewLine);


                bool cleanCode = true;

                int branchAddr = -1;
                int continueAddr = -1;
                var foundRW = new List<int>(); // todo
                int dataCount = 0;

                int next = 0;

                while (true)
                {
                    // process bytes as code until jump/branch or illegal op hit

                    var cease = false; // stop processing after this op

                    offset = addr - bankInfo.Origin;
                    var opcode = slice[offset];
                    var cmd = opcodes[opcode];

                    c = cdl != null ? cdl[offset] : (byte)0;

                    if ((c & CDL_ANY_CODE) == 0)
                    {
                        // not flagged as code in cdl

                        if ((c & CDL_ANY_DATA) != 0)
                        {
                            // but is flagged as data
                            dataCount++;
                            cleanCode = false;
                        }
                    }

                    next = addr + cmd.Bytes;

                    if (cmd.Legal == 1) // legal = illegal? todo
                    {
                        cleanCode = false;

                        sb.Append($"  ${addr:X4} illegal opcode {cmd.Text}(0x{opcode:X2}), ");

                        //if (addr == entryPoints[ep - 1]) {
                        // unlikely a code block start with an illegal op, stop processing
                        //sb.Append($"  breaking off processing of this code block"); sb.Append(Environment.NewLine);
                        //break;
                        //}

                        if ((c & CDL_ANY_CODE) == 0)
                        {
                            // not flagged as code in cdl
                            sb.AppendLine("cdl does not flag as code, stopping");
                            break;
                        }

                        sb.AppendLine("cdl flagged as code, not really stopping");
                    }

                    if (cmd.Bytes == 3 && !jumps.Contains(cmd.Text))
                    {
                        //   check for load/store to addresses in entry points list for suspected data
                        // todo check for direct vs indexed

                        //var target = hexdec(wordStr(slice, offset + 1));
                        var target = hexdec(slice[offset + 2].ToString("x2") + slice[offset + 1].ToString("x2"));
                        if (target >= 0x6000 && target <= 0xffff) // todo range?
                        {
                            foundRW.Add(target);
                        }
                    }

                    //// mark bytes as code
                    //for (int rb = 0; rb < cmd.Bytes; rb++)
                    //{
                    //    //if (codeMask[offset + rb])
                    //    //{
                    //    //    cease = true; // this code has already been looked at, don't continue past this op
                    //    //}

                    //    codeMask[offset + rb] = true;
                    //}


                    if (jumps.Contains(cmd.Text)) // op = jump/return
                    {
                        sb.Append($"  ${addr:X4} {cmd.Text} (0x{opcode:X2})  "); sb.AppendLine();

                        // end current trace after this op
                        // jsr follow up will be new entry point

                        if (cmd.Bytes > 1)
                        {
                            // jmp or jsr with 2 byte target address

                            if (opcode != 0x6c) // indirect jsr, usually from ram, unlikely to know destination
                            {
                                //var target = hexdec(wordStr(slice, offset + 1));
                                var target = hexdec(slice[offset + 2].ToString("x2") + slice[offset + 1].ToString("x2"));
                                branchAddr = target;

                                //// jsr or direct jmp
                                //if (!entryPoints.Contains(target))
                                //{
                                //    sb.Append($"    add jump to entry point ${target:X4} "); sb.Append(Environment.NewLine);
                                //    if (ep == entryPoints.Count)
                                //        entryPoints.Add(target); // at end of list, add to end
                                //    else
                                //        entryPoints.Insert(ep, target);
                                //}
                            }
                        }

                        if (opcode == 0x20)
                        {
                            // jsr, which should continue here after executing
                            // add new entry point for continuation

                            var target = next;
                            continueAddr = target;

                            //if (!entryPoints.Contains(target))
                            //{
                            //    sb.Append($"    add continuation entry point ${target:X4} "); sb.Append(Environment.NewLine);
                            //    entryPoints.Add(target);
                            //}
                        }
                        else
                        {
                            if (cdl != null && (cdl[next - bankInfo.Origin] | CDL_ANY_CODE) != 0)
                            {
                                // this op ends execution here but next byte is flagged as code, add as entry point
                                continueAddr = next;
                            }
                        }

                        // other commands end current code sequence with this op

                        break; // stop executing, continuation will process as its own code block
                    }
                    else if (branches.Contains(cmd.Text)) // op = conditional branch
                    {
                        // all should be 2 bytes

                        var delta = slice[offset + 1];
                        sb.Append($"  ${addr:X4} {cmd.Text} 0x{delta:X2}  "); sb.Append(Environment.NewLine);

                        if (delta == 0xff)
                        {
                            // this would be branch into self, which gets ugly 
                            sb.Append($"encountered branch into self at ${addr:X4}, stopping" + Environment.NewLine);
                            cleanCode = false;
                            break;
                        }

                        var target = next;
                        if (!entryPoints.Contains(target))
                        {
                            //entryPoints.Add(target);
                            //sb.Append($"    add continuation entry point ${target:X4} " + Environment.NewLine);

                            continueAddr = target;
                        }

                        if (delta == 0xfe)
                        {
                            sb.Append($"encountered branch to same inst at ${addr:X4}, could be infinite loop" + Environment.NewLine);
                            // do not add an entry point, already marked as code
                        }

                        target += delta; // go forward
                        if (delta >= 0x80)
                        {
                            target -= 0x100; // go back
                        }

                        branchAddr = target;
                        //if (!entryPoints.Contains(target))
                        //{
                        //    sb.Append($"    add branch to entry point ${target:X4} " + Environment.NewLine);
                        //    if (ep == entryPoints.Count)
                        //        entryPoints.Add(target); // at end of list, add to end
                        //    else
                        //        entryPoints.Insert(ep, target);
                        //}


                        break; // stop executing, let next inst pick up from added entry point
                    }

                    if (cease || !bankInfo.AddressInRange(next))
                    {
                        //   op runs into known code = stop
                        // out of range // todo new entry point for other bank?
                        break;
                    }

                    addr = next; // advance addr to next op 
                }


                if (cleanCode)
                {

                    // mark block as code

                    int seqLen = next - entry;

                    offset = entry - bankInfo.Origin;
                    for (int a = 0; a < seqLen; a++)
                    {
                        codeMask[offset + a] = true;
                    }

                    // add new data points

                    dataPoints.AddRange(foundRW);

                    // add new entry points

                    if (branchAddr > -1 && !entryPoints.Contains(branchAddr))
                    {
                        sb.Append($"    add external entry point ${branchAddr:X4} " + Environment.NewLine);
                        if (ep == entryPoints.Count)
                            entryPoints.Add(branchAddr); // at end of list, add to end
                        else
                            entryPoints.Insert(ep, branchAddr);
                    }

                    if (continueAddr > -1 && !entryPoints.Contains(continueAddr))
                    {
                        sb.Append($"    add continuation entry point ${continueAddr:X4} " + Environment.NewLine);
                        entryPoints.Add(continueAddr); // at end of list, add to end
                    }

                }
                else
                {

                    sb.AppendLine($"  potential issues, not marking as code");

                    if (dataCount > 0)
                    {
                        sb.AppendLine($"  {dataCount} data bytes in sequence");
                    }

                    // todo?
                }

            }

            File.WriteAllText(Path.GetFileNameWithoutExtension(_filename) + "-trace.txt", sb.ToString());

            //   return codeMask;
        }

        internal static bool IsDataRepeated(byte[] slice)
        {
            return IsDataRepeated(slice, 0, slice.Length);

            if (slice.Length % 2 != 0)
                throw new Exception("odd length byte array");

            int half = slice.Length / 2;

            for (int j = 0; j < half; j++)
            {
                if (slice[j] != slice[half + j])
                    return false;
            }

            return true;
        }

        internal static bool IsDataRepeated(byte[] slice, int start, int totalLength)
        {
            if (totalLength % 2 != 0)
                throw new Exception("odd length byte array");

            int half = totalLength / 2;

            for (int j = 0; j < half; j++)
            {
                if (slice[start + j] != slice[start + half + j])
                    return false;
            }

            return true;
        }

        /*
        static string FormatOp(
            string byteStr2, string newByteStr, int addressingType, bool isInvalid, int readBytes, array prgLabels)
        {

            var byteStr = byteStr2;

            // lets check for various addressing types to figure out how to format the text
            switch (addressingType)
            {

                case 0: // Implicit/Accumulator/Immediate

                    //byteStr = (readBytes > 0 ? "#$" + byteStr : "");

                    byteStr = string.Empty;
                    if (readBytes > 0)
                    {
                        byteStr = $"#${byteStr2}";
                    }

                    break;

                case 12: // jump table?
                case 11: // rts table?
                case 10: // jsr, jmp
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

                    //if (addressingType == 12)
                    //{
                    //    byteStr += "-1";
                    //}

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

                case -1:
                    // don't do anything
                    break;

                default:
                    Console.WriteLine(addressingType);
                    break;
            }

            return byteStr;
        }
        */

    }

}
