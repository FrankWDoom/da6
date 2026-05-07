using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace da6
{

    partial class Program
    {

        private static string _workingPath = null; // port addition // todo

        static void Main(string[] argv)
        {
            // port: argv as passed in omits the executing file name 
            // so all the indexes are off by 1 as the original code would expect them
            // this puts the exe name at the front of the array
            argv = Environment.GetCommandLineArgs();
            int argc = argv.Length;

            for (int j = 0; j < argv.Length; j++)
            {
                if (string.Equals(argv[j], "-dir", StringComparison.OrdinalIgnoreCase))
                {
                    int k = j + 1;
                    if (k < argv.Length)
                    {
                        _workingPath = argv[k];
                        Directory.SetCurrentDirectory(_workingPath);
                    }

                    break;
                }
            }

            // c# direct port
            try
            {
                //Console.WriteLine("executing c# direct conversion".PadRight(80, '_'));
                //da6Umbrella.disasm6net.Run(argc, argv);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                // c# revised code TODO
                Console.WriteLine("executing c# revised program".PadRight(80, '_'));
                var dis = new da6Umbrella.Disassembler();
                dis.Run(argc, argv);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (argv.Any(n => string.Equals(n.ToLowerInvariant(), "-php")))
            {
                // php exe version
                try
                {
                    Console.WriteLine("executing original php version".PadRight(80, '_'));
                    RunOriginal(argv);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
             
            Console.WriteLine("finished".PadRight(80, '_'));

            if (argv.Any(n => string.Equals(n.ToLowerInvariant(), "-wfe")))
            {
                Console.WriteLine("press any key to exit");
                Console.ReadLine();
            }
        }

        static void RunOriginal(string[] argv)
        {
            var args = new StringBuilder();

            for (int j = 1; j < argv.Length; j++)
            {
                var item = argv[j];

                if (item.Contains(" "))
                {
                    args.Append($"\"{item}\" ");
                }
                else
                {
                    args.Append(item);
                    args.Append(" ");
                }
            }

            var romName = argv[1];
            var asmOutName = $"{Path.GetFileNameWithoutExtension(romName).Trim()}.php.asm";

            // Use ProcessStartInfo class
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WorkingDirectory = _workingPath;

            //startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "disasm6.exe";
            startInfo.Arguments = args + $"-t \"{asmOutName}\" ";
            startInfo.RedirectStandardOutput = true;

            var rxAsm = new Regex(@"Disassembly (.+) generated", RegexOptions.IgnoreCase);
            var rxChr = new Regex(@"exported as (.+)$", RegexOptions.IgnoreCase);

            var files = new List<string>();

            using (var exeProcess = System.Diagnostics.Process.Start(startInfo))
            {
                string output;
                while ((output = exeProcess.StandardOutput.ReadLine()) != null)
                {
                    var match = rxAsm.Match(output);
                    if (match.Success)
                    {
                        files.Add(match.Groups[1].Value);
                    }

                    match = rxChr.Match(output);
                    if (match.Success)
                    {
                        files.Add(match.Groups[1].Value);
                    }

                    Console.WriteLine(output);
                }
            }

            //if (!string.IsNullOrWhiteSpace(_workingPath))
            //{
            //    foreach (var filename in files)
            //    {
            //        var fi = new FileInfo(filename);
            //        var dest = Path.Combine(_workingPath, filename);

            //        if (!string.Equals(dest, fi.FullName, StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            if (File.Exists(dest))
            //                File.Delete(dest);

            //            File.Move(fi.FullName, dest);
            //        }
            //    }
            //}
        }

    }
}
