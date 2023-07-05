# Youtube Extractor

<p align="center">
  <img width="128" height="128" src=".github/logo.png">
</p>

Desktop Tool for downloading videos from youtube with yt-dlp and converting with ffmpeg, built for Windows using .NET 6

## Install

- [Build Releases Available](https://github.com/felipe-dias-azevedo/YoutubeExtractor-Windows/releases/)

## Build

#### Dependencies

- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

### Instructions

#### GUI

- Windows

**GUI is only available on Windows.**

```sh
cd Felipe.YoutubeExtractor

dotnet publish -p:PublishProfile=.\Felipe.YoutubeExtractor\Properties\PublishProfiles\WindowsGui.pubxml
```

#### CLI

- Windows

```sh
cd Felipe.YoutubeExtractor

dotnet publish -p:PublishProfile=.\Felipe.YoutubeExtractor.Cli\Properties\PublishProfiles\WindowsCli.pubxml
```

- Linux

```sh
cd Felipe.YoutubeExtractor

dotnet publish -p:PublishProfile=./Felipe.YoutubeExtractor.Cli/Properties/PublishProfiles/LinuxCli.pubxml
```

- MacOS

```sh
cd Felipe.YoutubeExtractor

dotnet publish -p:PublishProfile=./Felipe.YoutubeExtractor.Cli/Properties/PublishProfiles/MacIntelCli.pubxml
```

#### All Projects

```sh
dotnet publish -c Release -r win-x64 --output ./out ./Felipe.YoutubeExtractor.sln
```
