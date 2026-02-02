using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Almanac.Utilities;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace Almanac;

public static class Localizer
{
    private const string FolderName = "Localizations";
    private static readonly string FolderPath;
    private static readonly Dictionary<string, string[]> localizations;
    
    static Localizer()
    {
        FolderPath = Path.Combine(AlmanacPlugin.AlmanacDir.Path, FolderName);
        localizations = new Dictionary<string, string[]>();

        Harmony harmony = AlmanacPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(Localization), nameof(Localization.LoadCSV)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(Localizer),
                nameof(Patch_Localization_LoadCSV))));
    }

    private static List<string> ReadAssemblyFile(string resourceName, string folder)
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

    public static void Start()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        string[] files = Directory.GetFiles(FolderPath, "*.yml", SearchOption.AllDirectories);
        if (files.Length <= 0)
        {
            string filePath = Path.Combine(FolderPath, "English.yml");
            List<string> defaultLines = Keys.Write();
            File.WriteAllLines(filePath, defaultLines);
            localizations["English"] = defaultLines.ToArray();
        }
        else
        {
            for (int i = 0; i < files.Length; ++i)
            {
                string filePath = files[i];
                string? fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] extraLines = File.ReadAllLines(filePath);
                List<string> lines = new();
                if (localizations.TryGetValue(fileName, out string[] translations))
                {
                    lines.AddRange(translations);
                    lines.AddRange(extraLines);
                }
                else
                {
                    lines.AddRange(extraLines);
                }
                localizations[fileName] = lines.ToArray();
            }
        }
    }

    private static void Patch_Localization_LoadCSV(Localization __instance, string language)
    {
        if (!localizations.TryGetValue(language, out string[] lines)) return;
        ParseLines(__instance, lines);
    }

    private static void ParseLines(Localization instance, string[] lines)
    {
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;
            string[] parts = line.Split(':');
            if (parts.Length < 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            value = instance.StripCitations(value);

            instance.AddWord(key, value);
        }
    }

    private static void Update()
    {
        if (Localization.instance == null) return;
        string lang = Localization.instance.GetSelectedLanguage();
        if (!localizations.TryGetValue(lang, out string[] translations)) return;
        ParseLines(Localization.instance, translations);
        Localization.instance.m_cache.EvictAll();
    }
}