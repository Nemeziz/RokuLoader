using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace RokuLoader
{
    public class Options
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('h', "hostname", HelpText = "Prints all messages to standard output.")]
        public string Hostname { get; set; }

        [Option('u', "username", DefaultValue = "rokudev", HelpText = "Prints all messages to standard output.")]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "Prints all messages to standard output.")]
        public string Password { get; set; }

        [Option('z', "zip-file-path", HelpText = "Prints all messages to standard output.")]
        public string ZipFilePath { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
