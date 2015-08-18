// ------------------------------------------------------------------------------
// RokuLoader 1.0
// Copyright (C) 2015 Patrick Fournier
// http://github.com/patrick0xf/RokuLoader
// Under MIT License
// ------------------------------------------------------------------------------

using CommandLine;
using CommandLine.Text;

namespace RokuLoader
{
    /// <summary>
    /// Defines options for the command line parameters
    /// </summary>
    public class Options
    {
        [Option('h', "hostname", Required = true, HelpText = "The hostname or IP address of the Roku Streaming Player.")]
        public string Hostname { get; set; }

        [Option('u', "username", DefaultValue = "rokudev", HelpText = "Do not set unless you are on very old Roku firware that doesn't require local developer authentication, in which case, use '-u none'.")]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "The password for the 'rokudev' local developer account to access the Roku Streaming Player's Developer Application Installer")]
        public string Password { get; set; }

        [Option('z', "zipfile", Required = true, HelpText = "The local path to the Roku application packaged as a zip file", MutuallyExclusiveSet = "By Package")]
        public string ZipFilePath { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText()
            {
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("For details regarding Roku development, visit http://sdkdocs.roku.com/display/sdkdoc/Developer+Guide");
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("Usage:");
            help.AddPostOptionsLine("Example:   rokuloader -h 192.168.1.10 -p password -z c:\\rokudev\\package.zip");
            help.AddPostOptionsLine("");
            help.AddOptions(this);
            return help;
        }
    }
}
