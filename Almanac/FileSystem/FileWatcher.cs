using System.Collections.Generic;
using System.IO;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using BepInEx;
using YamlDotNet.Serialization;

namespace Almanac.FileSystem;

public static class FileWatcher
{
    public static void InitFileSystemWatch()
    {
        FileSystemWatcher AchievementWatcher = new FileSystemWatcher(AlmanacPaths.AchievementFolderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        AchievementWatcher.Created += OnAchievementChange;
        AchievementWatcher.Deleted += OnAchievementChange;
        AchievementWatcher.Changed += OnAchievementChange;

        FileSystemWatcher FilterWatcher = new FileSystemWatcher(AlmanacPaths.FilterFolderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        FilterWatcher.Changed += OnFilterChange;
        FilterWatcher.Created += OnFilterChange;
        FilterWatcher.Deleted += OnFilterDelete;

        FileSystemWatcher CreatureWatcher = new FileSystemWatcher(AlmanacPaths.CreatureFolderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };

        CreatureWatcher.Changed += OnCreatureChange;
        CreatureWatcher.Deleted += OnCreatureChange;
        CreatureWatcher.Created += OnCreatureChange;
        
        FileSystemWatcher ServerPlayerDataListWatcher =
            new FileSystemWatcher(AlmanacPaths.ServerPlayerDataFolderPath)
            {
                Filter = "*.yml",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                SynchronizingObject = ThreadingHelper.SynchronizingObject,
                NotifyFilter = NotifyFilters.LastWrite
            };

        ServerPlayerDataListWatcher.Changed += OnServerPlayerDataListChange;
        ServerPlayerDataListWatcher.Created += OnServerPlayerDataListChange;

        FileSystemWatcher ServerBountyListWatcher = new FileSystemWatcher(AlmanacPaths.BountyFolderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        ServerBountyListWatcher.Changed += OnBountyChange;
        ServerBountyListWatcher.Created += OnBountyChange;
        ServerBountyListWatcher.Deleted += OnBountyChange;

        FileSystemWatcher TreasureListWatcher = new FileSystemWatcher(AlmanacPaths.TreasureHuntFolderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        TreasureListWatcher.Changed += OnTreasureChange;
        TreasureListWatcher.Created += OnTreasureChange;
        TreasureListWatcher.Deleted += OnTreasureChange;
    }

    private static void OnTreasureChange(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " changed, reloading treasures");
                break;
            case WatcherChangeTypes.Created:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " created, reloading treasures");
                break;
            case WatcherChangeTypes.Deleted:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " deleted, reloading treasures");
                break;
        }
        
        if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
        {
            TreasureHunt.TreasureManager.InitTreasureManager();
            ServerSyncedData.UpdateServerTreasureList();
        }
        else
        {
            TreasureHunt.TreasureManager.InitTreasureManager(true);
        }
    }

    private static void OnServerPlayerDataListChange(object sender, FileSystemEventArgs e)
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Leaderboard file changed");
        string data = File.ReadAllText(AlmanacPaths.ServerPlayerDataFilePath);
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, PlayerData> LeaderboardData = deserializer.Deserialize<Dictionary<string, PlayerData>>(data);
        Leaderboard.LeaderboardData = LeaderboardData;
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Sending updated leaderboard to clients");
        Leaderboard.SendToClients(data);
    }

    private static void OnAchievementChange(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " changed, reloading achievements");
                break;
            case WatcherChangeTypes.Created:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " created, reloading achievements");
                break;
            case WatcherChangeTypes.Deleted:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " deleted, reloading achievements");
                break;
        }
        
        AchievementManager.AchievementsRan = false;
        AchievementManager.ReadAchievements();
        AchievementManager.InitAchievements(AchievementManager.LoadAchievementData(AchievementManager.AchievementData));

        if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
        {
            ServerSyncedData.UpdateServerAchievements();
        }
    }

    private static void OnFilterChange(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        if (fileName != AlmanacPaths.IgnoreListFileName) return;
        AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " changed, reloading filters");
        
        Filters.InitFilters();

        if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
        {
            ServerSyncedData.UpdateServerIgnoreList();
        }
    }

    private static void OnFilterDelete(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        if (fileName != AlmanacPaths.IgnoreListFileName) return;
        AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " deleted, removing filters");
        
        Filters.FilterList.Clear();
    }

    private static void OnCreatureChange(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " changed, reloading creature list");
                break;
            case WatcherChangeTypes.Created:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " created, reloading creature list");
                break;
            case WatcherChangeTypes.Deleted:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " deleted, reloading creature list");
                break;
        }
        
        CreatureLists.InitCreatureLists();
        if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
        {
            ServerSyncedData.UpdateServerCreatureList();
        }
    }

    private static void OnBountyChange(object sender, FileSystemEventArgs e)
    {

        string fileName = Path.GetFileName(e.Name);
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " changed, reloading bounty list");
                break;
            case WatcherChangeTypes.Created:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " created, reloading bounty list");
                break;
            case WatcherChangeTypes.Deleted:
                AlmanacPlugin.AlmanacLogger.LogDebug(fileName + " deleted, reloading bounty list");
                break;
        }

        if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
        {
            BountyManager.InitBounties(false);
            ServerSyncedData.UpdateServerBountyList();
        }
        else
        {
            BountyManager.InitBounties(false);
        }
        
    }
}