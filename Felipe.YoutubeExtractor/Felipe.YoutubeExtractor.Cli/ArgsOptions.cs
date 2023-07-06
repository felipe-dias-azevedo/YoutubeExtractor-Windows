using CommandLine;

namespace Felipe.YoutubeExtractor.Cli
{
    public class ArgsOptions
    {
        [Option('u', "url", Required = true, HelpText = "Youtube video URL")]
        public string VideoUrl { get; set; }
        
        [Option('n', "normalize", Default = false, HelpText = "Normalize audio on post-processing")]
        public bool NormalizeAudio { get; set; }
        
        [Option('v', "verbose", Default = false, HelpText = "Output more information")]
        public bool Verbose { get; set; }
    }
}
