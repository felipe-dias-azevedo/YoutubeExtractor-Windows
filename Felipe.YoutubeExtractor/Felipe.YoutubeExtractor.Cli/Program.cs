using System.Text.Json;
using CommandLine;
using Felipe.YoutubeExtractor.Cli;
using Felipe.YoutubeExtractor.Core.Helpers;
using Felipe.YoutubeExtractor.Core.Models;
using Felipe.YoutubeExtractor.Core.Services;


var argsParsed = Parser.Default.ParseArguments<ArgsOptions>(args);

var arguments = argsParsed.Value;

if (arguments is null)
{
    return;
}

if (arguments.Verbose)
{
    Console.WriteLine($"URL: {arguments.VideoUrl}; Normalize: {arguments.NormalizeAudio}");
}

if (!YoutubeHelper.IsValidUrl(arguments.VideoUrl))
{
    Console.WriteLine("Video URL is not valid.");
    return;
}

var videoOptions = new VideoOptionsModel
{
    YoutubeUrl = arguments.VideoUrl
};

if (arguments.Verbose)
{
    var options = JsonSerializer.Serialize(videoOptions);
    Console.WriteLine($"Options: {options}");
}

var youtubeService = new YoutubeService(videoOptions);

var downloadFilePath = await youtubeService.Download();

Console.WriteLine("Download Done.");

if (arguments.NormalizeAudio)
{
    await youtubeService.Normalize(downloadFilePath);
    
    Console.WriteLine("Normalize Done.");
}

