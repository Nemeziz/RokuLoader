using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RokuLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options)) return;

            // Values are available here
            if (options.Verbose) Console.WriteLine("Filename: {0}", options.ZipFilePath);
        }
    }
}
