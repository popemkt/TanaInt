using System.Text.RegularExpressions;

namespace TanaInt.Domain;

public static partial class Utils
{
    public static string FormatTanaDate(DateTime date) => $"[[date:{date:yyyy-MM-dd}]]";
    public static string FormatTanaDateTime(DateTime date) => $"[[date:{date:yyyy-MM-ddTHH:mm}]]";

    public static string[] ExtractDateStrings(string line)
    {
        var match = TanaDateRegex().Match(line);

        return match.Groups[1].Value[(match.Groups[1].Value.IndexOf(":") + 1)..].Split("/");
    }
    
    public static DateTime WithKind(this DateTime dateTime, DateTimeKind kind) => DateTime.SpecifyKind(dateTime, kind);
    
    [GeneratedRegex(@"\[\[(.*?)\]\]", RegexOptions.Compiled)]
    private static partial Regex TanaDateRegex();
}