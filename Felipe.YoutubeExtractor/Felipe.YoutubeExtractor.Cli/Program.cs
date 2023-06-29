using CommandLine;
using Felipe.YoutubeExtractor.Cli;


var arguments = Parser.Default.ParseArguments<ArgsOptions>(args).Value;

Console.WriteLine("video url: " + arguments.VideoUrl);