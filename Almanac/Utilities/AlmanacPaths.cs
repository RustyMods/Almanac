using System.IO;
using BepInEx;

namespace Almanac.Utilities;

public static class AlmanacPaths
{
    public static readonly string FolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";
    public static readonly string AchievementFolderPath = FolderPath + Path.DirectorySeparatorChar + "Achievements";
    private static readonly string FilterFolderPath = FolderPath + Path.DirectorySeparatorChar + "Filters";
    private const string IgnoreListFileName = "IgnoreList.yml";
    public static readonly string IgnorePath = FilterFolderPath + Path.DirectorySeparatorChar + IgnoreListFileName;
    public static readonly string CreatureFolderPath = FolderPath + Path.DirectorySeparatorChar + "CreatureGroups";
    public static readonly string BountyFolderPath = FolderPath + Path.DirectorySeparatorChar + "Bounties";
    public static readonly string TreasureHuntFolderPath = FolderPath + Path.DirectorySeparatorChar + "Treasures";
    public static readonly string StoreFolderPath = FolderPath + Path.DirectorySeparatorChar + "Store";
    public static readonly string CustomEffectPath = FolderPath + Path.DirectorySeparatorChar + "CustomEffects";
    public static readonly string LeaderboardFolderPath = FolderPath + Path.DirectorySeparatorChar + "Leaderboards";
    public static readonly string LotteryFolderPath = FolderPath + Path.DirectorySeparatorChar + "Lotteries";
    public static readonly string MarketplaceFolderPath = FolderPath + Path.DirectorySeparatorChar + "Marketplace";
    public static void CreateFolderDirectories()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        if (!Directory.Exists(AchievementFolderPath)) Directory.CreateDirectory(AchievementFolderPath);
        if (!Directory.Exists(FilterFolderPath)) Directory.CreateDirectory(FilterFolderPath);
        if (!Directory.Exists(CreatureFolderPath)) Directory.CreateDirectory(CreatureFolderPath);
        if (!Directory.Exists(BountyFolderPath)) Directory.CreateDirectory(BountyFolderPath);
        if (!Directory.Exists(TreasureHuntFolderPath)) Directory.CreateDirectory(TreasureHuntFolderPath);
        if (!Directory.Exists(StoreFolderPath)) Directory.CreateDirectory(StoreFolderPath);
        if (!Directory.Exists(CustomEffectPath)) Directory.CreateDirectory(CustomEffectPath);
        if (!Directory.Exists(LeaderboardFolderPath)) Directory.CreateDirectory(LeaderboardFolderPath);
        if (!Directory.Exists(LotteryFolderPath)) Directory.CreateDirectory(LotteryFolderPath);
        if (!Directory.Exists(MarketplaceFolderPath)) Directory.CreateDirectory(MarketplaceFolderPath);
    }
}