using CommandLine;

namespace Felipe.YoutubeExtractor.Cli
{
    public class ArgsOptions
    {
        [Option('u', "url", Required = true, HelpText = "Youtube Video URL")]
        public string VideoUrl { get; set; }
    }
}
