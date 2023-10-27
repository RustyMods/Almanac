using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;

namespace Almanac.Almanac;

public static class BlackList
{
    public static readonly CustomSyncedValue<List<string>> ItemBlackList =
        new(AlmanacPlugin.ConfigSync, "ItemBlackList", new());
    
    public static readonly CustomSyncedValue<List<string>> CreatureBlackList =
        new(AlmanacPlugin.ConfigSync, "CreatureBlackList", new());
    
    public static readonly CustomSyncedValue<List<string>> PieceBlackList =
        new(AlmanacPlugin.ConfigSync, "PieceBlackList", new());

    private static readonly string folderName = "Almanac";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);

    public static void InitBlackList()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Server)
        {
            if (!Directory.Exists(Path.Combine(Paths.ConfigPath, folderName)))
            {
                Directory.CreateDirectory(folderPath);
            }

            CreateDefaultFile("ItemBlackList.yml", "#List out item prefabs to blacklist:\n#SwordCheat");
            CreateDefaultFile("CreatureBlackList.yml", "#List out creature prefabs to blacklist:\n#Player");
            CreateDefaultFile("PieceBlackList.yml", "#List out pieces prefabs to blacklist:\n#piece_workbench");
            
            
            FileSystemWatcher blacklistWatcher = new FileSystemWatcher(folderPath)
            {
                Filter = "*.yml",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                SynchronizingObject = ThreadingHelper.SynchronizingObject,
                NotifyFilter = NotifyFilters.LastWrite
            };
            blacklistWatcher.Changed += BlackListChanged;
            
            SetInitialBlackList("ItemBlackList.yml");
            SetInitialBlackList("CreatureBlackList.yml");
            SetInitialBlackList("PieceBlackList.yml");
        }
    }

    private static void SetInitialBlackList(string fileName)
    {
        List<string> blacklist = new List<string>();
        foreach (string line in File.ReadLines(Path.Combine(folderPath, fileName)))
        {
            if (line.StartsWith("#")) continue;
            blacklist.Add(line);
        }

        switch (fileName)
        {
            case "ItemBlackList.yml":
                ItemBlackList.Value = blacklist;
                break;

            case "CreatureBlackList.yml":
                CreatureBlackList.Value = blacklist;
                break;
            
            case "PieceBlackList.yml":
                PieceBlackList.Value = blacklist;
                break;
        }
    }

    private static void BlackListChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType is not (WatcherChangeTypes.Changed or WatcherChangeTypes.Deleted)) return;
        
        string fName = Path.GetFileName(e.Name);
        List<string> blacklist = new List<string>();
        foreach (string line in File.ReadLines(Path.Combine(folderPath, fName)))
        {
            if (line.StartsWith("#")) continue;
            blacklist.Add(line);
        }

        switch (fName)
        {
            case "ItemBlackList.yml":
                ItemBlackList.Value = blacklist;
                break;

            case "CreatureBlackList.yml":
                CreatureBlackList.Value = blacklist;
                break;
            
            case "PieceBlackList.yml":
                PieceBlackList.Value = blacklist;
                break;
        }
    }

    private static void CreateDefaultFile(string fileName, string contents)
    {
        string filePath = Path.Combine(folderPath, fileName);
        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, contents);
        }
    }
}