using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain.WallChanger;

public class BannerChangerDto
{
    public string Context { get; set; }
    [JsonIgnore] public List<string> ImagesList { get; set; } = new();

    public BannerChangerDto ParseImages()
    {
        var split = Context.Split('\n');
        ImagesList = split.Where(s => s.StartsWith("      -")).ToList(); //Indented 4 levels
        //random an item from imageslist
        return this;
    }

    public static string ParseSingleImageLine(string imageLine)
    {
        const string pattern = @"\!\[.*?\]\((.*?)\)";

        var match = Regex.Match(imageLine, pattern, RegexOptions.Compiled);
        if (!match.Success) return string.Empty;
        
        var imageUrl = match.Groups[1].Value;
        return imageUrl;
    }
}