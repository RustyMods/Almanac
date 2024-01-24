using System.IO;
using Almanac.Data;
using BepInEx;

namespace Almanac.FileSystem;

public static class AlmanacPaths
{
    private static readonly string FolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";
    public static readonly string CustomDataFileName = "_Data.yml";
    public static readonly string AchievementFolderPath = FolderPath + Path.DirectorySeparatorChar + "AchievementData";
    public static readonly string PlayerDataFolderPath = FolderPath + Path.DirectorySeparatorChar + "PlayerData";
    public static readonly string FilterFolderPath = FolderPath + Path.DirectorySeparatorChar + "Filters";
    public static readonly string IgnoreListFileName = "IgnoreList.yml";
    public static readonly string IgnorePath = FilterFolderPath + Path.DirectorySeparatorChar + IgnoreListFileName;
    public static readonly string AchievementTutorialPath = FolderPath + Path.DirectorySeparatorChar + "Achievements_README.md";
    public static readonly string CreatureFolderPath = FolderPath + Path.DirectorySeparatorChar + "Creatures";
    public static readonly string ServerPlayerDataFolderPath = FolderPath + Path.DirectorySeparatorChar + "Players";
    public static readonly string ServerPlayerDataFilePath = ServerPlayerDataFolderPath + Path.DirectorySeparatorChar + "PlayerListData.yml";

    public static void CreateFolderDirectories()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        if (!Directory.Exists(AchievementFolderPath)) Directory.CreateDirectory(AchievementFolderPath);
        if (!Directory.Exists(FilterFolderPath)) Directory.CreateDirectory(FilterFolderPath);
        if (!Directory.Exists(CreatureFolderPath)) Directory.CreateDirectory(CreatureFolderPath);

        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
        {
            if (!Directory.Exists(PlayerDataFolderPath)) Directory.CreateDirectory(PlayerDataFolderPath);
        }
        else
        {
            if (!Directory.Exists(ServerPlayerDataFolderPath)) Directory.CreateDirectory(ServerPlayerDataFolderPath);
        }
    }
}