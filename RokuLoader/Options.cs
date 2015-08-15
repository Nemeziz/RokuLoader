using CommandLine;
using CommandLine.Text;

namespace RokuLoader
{

    public class Options
    {
        [Option('h', "hostname", Required = true, HelpText = "The hostname or IP address of the Roku Streaming Player.")]
        public string Hostname { get; set; }

        [Option('u', "username", DefaultValue = "rokudev", HelpText = "Do not set. This is always set to the 'rokudev' local developer account of the Roku Streaming Player's Developer Application Installer. Very old Roku firmware may not require this; use  '-u none'   in this case, and send a bogus passord.")]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "The password for the 'rokudev' local developer account to access the Roku Streaming Player's Developer Application Installer")]
        public string Password { get; set; }

        [Option('z', "zip-file-path", HelpText = "The local path to the source code for a Roku application packaged as a zip file", MutuallyExclusiveSet = "By Package")]
        public string ZipFilePath { get; set; }

        [Option('c', "code-directory-path", HelpText = "NOT IMPLEMENTED - The local path to the root folder of the source code for a Roku application", MutuallyExclusiveSet = "By Directory")]
        public string CodeDirectoryPath { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
