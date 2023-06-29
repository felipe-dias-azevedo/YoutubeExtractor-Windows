namespace Felipe.YoutubeExtractor.Core.Helpers
{
    public static class FileHelper
    {
        public static bool ExistsFile(string path)
        {
            var pathString = path;

            if (path.Contains("\'"))
            {
                pathString = path.Replace("\'", "");
            }
            else if (path.Contains("\""))
            {
                pathString = path.Replace("\"", "");
            }

            return File.Exists(pathString);
        }

        public static bool ExistsFolder(string path)
        {
            var pathString = path;

            if (path.Contains("\'"))
            {
                pathString = path.Replace("\'", "");
            }
            else if (path.Contains("\""))
            {
                pathString = path.Replace("\"", "");
            }

            return Directory.Exists(pathString);
        }

        public static string ConvertExecutable(string path)
        {
            if (path.Contains(" "))
            {
                return $"\"{path}\"";
            }
            return path;
        }

        public static string? GetFolderPathFromFilePath(string path)
        {
            string? folderPath = null;

            if (!string.IsNullOrEmpty(path))
            {
                var directoryWithoutFile = path.Split(Path.DirectorySeparatorChar).SkipLast(1);

                folderPath = string.Join(Path.DirectorySeparatorChar, directoryWithoutFile);
            }

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            return folderPath;
        }
    }
}
