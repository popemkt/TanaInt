using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain.WallChanger;

public partial class BannerChangerDto
{
    public BannerChangerDto(string imagesString)
    {
        ImageText = imagesString;
    }
    public string ImageText { get; set; }
    [JsonIgnore] public List<string> ImagesList { get; set; } = new();

    public BannerChangerDto ParseImages()
    {
        var split = ImageText.Split("\\n");
        ImagesList = split.Where(s => s.StartsWith("  -")).ToList(); //Indented 4 levels
        //random an item from imageslist
        return this;
    }

    public static string ParseSingleImageLine(string imageLine)
    {
        var match = TanaImageRegex().Match(imageLine);
        if (!match.Success) return string.Empty;
        
        var imageUrl = match.Groups[1].Value;
        return imageUrl;
    }

    [GeneratedRegex(@"\!\[.*?\]\((.*?)\)", RegexOptions.Compiled)]
    private static partial Regex TanaImageRegex();
}