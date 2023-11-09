using System.Collections.Generic;
using System.IO;
using BepInEx;
using static Almanac.Almanac.AchievementManager;
using static Almanac.Almanac.BlackList;
using static Almanac.AlmanacPlugin;

namespace Almanac.Almanac;

public static class FileSystem
{
    private static readonly string folderName = "Almanac";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    private static readonly string achievementPath = folderPath + Path.DirectorySeparatorChar + "AchievementData";
    
    public static void InitializeFileSystemWatch()
    {
        if (WorkingAsType is not WorkingAs.Server) return;
        
        FileSystemWatcher fileWatcher = new FileSystemWatcher(folderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        fileWatcher.Created += OnChanged;
        fileWatcher.Deleted += OnChanged;
        fileWatcher.Changed += OnChanged;
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType is not (WatcherChangeTypes.Changed or WatcherChangeTypes.Deleted)) return;
        string fName = Path.GetFileName(e.Name);
        
        if (e.FullPath.StartsWith(achievementPath))
        {
            AlmanacLogger.LogInfo($"Server achievement file changed: {fName}");
            InitAchievements();
            return;
        }

        List<string> blacklist = new List<string>();
        foreach (string line in File.ReadLines(Path.Combine(folderPath, fName)))
        {
            if (line.StartsWith("#")) continue;
            blacklist.Add(line);
        }

        switch (fName)
        {
            case "ItemBlackList.yml": ItemBlackList.Value = blacklist; break;
            case "CreatureBlackList.yml": CreatureBlackList.Value = blacklist; break;
            case "PieceBlackList.yml": PieceBlackList.Value = blacklist; break;
        }
    }
}