using System.Linq;
using System.Text.RegularExpressions;

namespace Felipe.YoutubeExtractor.Extensions
{
    public static class StringExtensions
    {
        public static string CapitalizedToSpaceJoined(this string text)
        {
            var splitted = Regex.Split(text, @"(?<!\s)(?=[A-Z])");

            return string.Join(" ", splitted.Where(x => !string.IsNullOrEmpty(x)));
        }

        public static string RemoveAllWhitespaces(this string text)
        {
            return Regex.Replace(text, @"\s+", "").Trim();
        }
    }
}
