using System.Text.RegularExpressions;

namespace E.Data
{
    public partial class DataProcessor
    {
        private class Utility
        {
            private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]+([^/\\]+)[/\\]*)$");

            public static string GetDirectoryName(string filePath)
            { return fileNameRegex.Replace(filePath, string.Empty); }

            public static string GetFileName(string filePath)
            { return fileNameRegex.Match(filePath).Groups[1].Value; }

            private static readonly Regex SlashRegex = new Regex(@"[/\\]");

            public static string ConvertURLSlash(string input)
            {
                return SlashRegex.Replace(input, "/");
            }

        }
    }
}
