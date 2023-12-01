using System.Text.RegularExpressions;

namespace TanaInt.Domain;

public static class Utils
{
    public static string FormatDate(DateTime date) => $"[[date:{date:yyyy-MM-dd}]]";

    public static string[] ExtractDateStrings(string line)
    {
        const string pattern = @"\[\[(.*?)\]\]";

        // Create a Regex object and match the pattern
        var match = Regex.Match(line, pattern, RegexOptions.Compiled);

        return match.Groups[1].Value[(match.Groups[1].Value.IndexOf(":") + 1)..].Split("/");
    }
}