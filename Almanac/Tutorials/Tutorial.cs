using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Almanac.Tutorials;

public class Tutorial
{
    private static readonly StringBuilder sb = new();
    private readonly List<string> lines;
    public readonly string label;
    public readonly string tooltip;
    public Tutorial(string label, string resourceFileName, string folder = "Tutorials") : this(label, LoadMarkdownFromAssembly(resourceFileName, folder)){}
    private Tutorial(string label, List<string> lines)
    {
        this.label = label;
        this.lines = lines;
        tooltip = GetInfo();
    }
    public static List<string> LoadMarkdownFromAssembly(string resourceName, string folder = "Tutorials")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string path = $"{AlmanacPlugin.ModName}.{folder}.{resourceName}";
        using Stream? stream = assembly.GetManifestResourceStream(path);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found in assembly '{assembly.FullName}'.");

        using StreamReader reader = new StreamReader(stream);
        List<string> lines = new List<string>();
        while (!reader.EndOfStream)
        {
            lines.Add(reader.ReadLine() ?? string.Empty);
        }
        return lines;
    }
    private static string MarkdownToRichText(string line)
    {
        // Bold: **text**
        line = Regex.Replace(line, @"\*\*(.+?)\*\*", "<b>$1</b>");
        // Italic: *text* or _text_
        line = Regex.Replace(line, @"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", "<i>$1</i>");
        line = Regex.Replace(line, "_(.+?)_", "<i>$1</i>");
        // Inline code: `text`
        line = Regex.Replace(line, "`(.+?)`", "<color=#d19a66><i>$1</i></color>");
        // Headers: #, ##, ###
        line = Regex.Replace(line, "^### (.+)$", "<size=16%><b>$1</b></size>\n");
        line = Regex.Replace(line, "^## (.+)$", "<size=17%><b>$1</b></size>\n");
        line = Regex.Replace(line, "^# (.+)$", "<size=18%><b>$1</b></size>\n");
        // Links: [text](url) → just display text in blue
        line = Regex.Replace(line, @"\[(.+?)\]\((.+?)\)", "<color=#61afef><u>$1</u></color>");
        // Unordered list: "- " → "• "
        line = Regex.Replace(line, @"^\-\s+", "• ");
        return line;
    }
    private string GetInfo()
    {
        sb.Clear();
        foreach (string? line in lines)
        {
            sb.Append(MarkdownToRichText(line));
            sb.Append('\n');
        }
        return sb.ToString();
    }
}