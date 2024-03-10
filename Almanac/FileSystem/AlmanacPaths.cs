using System.IO;
using BepInEx;

namespace Almanac.FileSystem;

public static class AlmanacPaths
{
    private static readonly string LocalPath = Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + Path.DirectorySeparatorChar + "AlmanacData";
    private static readonly string FolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";
    public static readonly string CustomDataFileName = "_Data.yml";
    public static readonly string AchievementFolderPath = FolderPath + Path.DirectorySeparatorChar + "AchievementData";
    public static readonly string PlayerDataFolderPath = (AlmanacPlugin._RootPath.Value is AlmanacPlugin.DataPath.LocalLow ? LocalPath : FolderPath) + Path.DirectorySeparatorChar + "PlayerData";
    public static readonly string FilterFolderPath = FolderPath + Path.DirectorySeparatorChar + "Filters";
    public static readonly string IgnoreListFileName = "IgnoreList.yml";
    public static readonly string IgnorePath = FilterFolderPath + Path.DirectorySeparatorChar + IgnoreListFileName;
    public static readonly string CreatureFolderPath = FolderPath + Path.DirectorySeparatorChar + "Creatures";
    public static readonly string ServerPlayerDataFolderPath = (AlmanacPlugin._RootPath.Value is AlmanacPlugin.DataPath.LocalLow ? LocalPath : FolderPath) + Path.DirectorySeparatorChar + "Players";
    public static readonly string ServerPlayerDataFilePath = ServerPlayerDataFolderPath + Path.DirectorySeparatorChar + "PlayerListData.yml";
    public static readonly string CustomCreatureGroupFolder = CreatureFolderPath + Path.DirectorySeparatorChar + "Custom";
    public static readonly string BountyFolderPath = FolderPath + Path.DirectorySeparatorChar + "Bounties";
    public static readonly string TreasureHuntFolderPath = FolderPath + Path.DirectorySeparatorChar + "Treasures";
    public static void CreateFolderDirectories()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        if (!Directory.Exists(AchievementFolderPath)) Directory.CreateDirectory(AchievementFolderPath);
        if (!Directory.Exists(FilterFolderPath)) Directory.CreateDirectory(FilterFolderPath);
        if (!Directory.Exists(CreatureFolderPath)) Directory.CreateDirectory(CreatureFolderPath);
        if (!Directory.Exists(PlayerDataFolderPath)) Directory.CreateDirectory(PlayerDataFolderPath);
        if (!Directory.Exists(ServerPlayerDataFolderPath)) Directory.CreateDirectory(ServerPlayerDataFolderPath);
        if (!Directory.Exists(CustomCreatureGroupFolder)) Directory.CreateDirectory(CustomCreatureGroupFolder);
        if (!Directory.Exists(BountyFolderPath)) Directory.CreateDirectory(BountyFolderPath);
        if (!Directory.Exists(TreasureHuntFolderPath)) Directory.CreateDirectory(TreasureHuntFolderPath);
    }
}