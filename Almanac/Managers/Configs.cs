using System.IO;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using BepInEx.Configuration;
using ServerSync;
using UnityEngine;

namespace Almanac.Managers;

public enum Toggle { On = 1, Off = 0 }

public static class Configs
{
    private static ConfigEntry<Toggle> _serverConfigLocked = null!;
    private static ConfigEntry<Toggle> _KnowledgeWall = null!;
    private static ConfigEntry<Toggle> _UseIgnoreList = null!;
    private static ConfigEntry<Toggle> _ShowAllData = null!;
    private static ConfigEntry<Color> _OutlineColor = null!;
    private static ConfigEntry<Toggle> _Transparent = null!;
    private static ConfigEntry<Toggle> _HideUnknownEntries = null!;
    
    private static ConfigEntry<int> _TreasureCooldown = null!;
    private static ConfigEntry<int> _BountyCooldown = null!;
    private static ConfigEntry<int> _lotteryCost = null!;
    private static ConfigEntry<int> _minFullHouse = null!;
    
    private static ConfigEntry<Toggle> _TreasureEnabled = null!;
    private static ConfigEntry<Toggle> _BountyEnabled = null!;
    private static ConfigEntry<Toggle> _AchievementsEnabled = null!;
    private static ConfigEntry<Toggle> _lotteryEnabled = null!;
    private static ConfigEntry<Toggle> _statusEffectsEnabled = null!;
    private static ConfigEntry<Toggle> _storeEnabled = null!;
    private static ConfigEntry<Toggle> _metricsEnabled = null!;
    private static ConfigEntry<Toggle> _leaderboardEnabled = null!;
    private static ConfigEntry<Toggle> _marketplaceEnabled = null!;
    private static ConfigEntry<Toggle> _allowConversion = null!;
    private static ConfigEntry<int> _conversionRate = null!;
    private static ConfigEntry<string> _conversionItem = null!;
    
    public static bool UseIgnoreList => _UseIgnoreList.Value is Toggle.On;
    public static bool ShowBounties => _BountyEnabled.Value is Toggle.On;
    public static bool ShowAchievements => _AchievementsEnabled.Value is Toggle.On;
    public static bool ShowTreasures => _TreasureEnabled.Value is Toggle.On;
    public static bool ShowAllData => _ShowAllData.Value is Toggle.On;
    public static bool UseKnowledgeWall => _KnowledgeWall.Value is Toggle.On;
    public static bool Transparent => _Transparent.Value is Toggle.On;
    public static Color OutlineColor => _OutlineColor.Value;
    public static int BountyCooldown => _BountyCooldown.Value;
    public static int TreasureCooldown => _TreasureCooldown.Value;
    public static bool ShowLottery => _lotteryEnabled.Value is Toggle.On;
    public static bool ShowStatusEffects => _statusEffectsEnabled.Value is Toggle.On;
    public static bool ShowStore => _storeEnabled.Value is Toggle.On;
    public static bool ShowMetrics =>  _metricsEnabled.Value is Toggle.On;
    public static bool ShowLeaderboard => _leaderboardEnabled.Value is Toggle.On;
    public static bool HideUnknownEntries => _HideUnknownEntries.Value is Toggle.On;
    public static int LotteryCost =>  _lotteryCost.Value;
    public static bool ShowMarketplace => _marketplaceEnabled.Value is Toggle.On;
    public static int MinFullHouse => _minFullHouse.Value;
    public static bool AllowConversion => _allowConversion.Value is Toggle.On;
    public static int ConversionRate => _conversionRate.Value;
    public static string ConversionItem => _conversionItem.Value;

    public static AlmanacPanel.Background.BackgroundOption bkgOption => Transparent
        ? AlmanacPanel.Background.BackgroundOption.Transparent
        : AlmanacPanel.Background.BackgroundOption.Opaque;
    
    public static void Load()
    {
        _serverConfigLocked = config("1 - General", "0 - Lock Configuration", Toggle.On,
            "If on, the configuration is locked and can be changed by server admins only.");
        _ = AlmanacPlugin.ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

        _KnowledgeWall = config("1 - General", "Knowledge Wall", Toggle.On, "If on, the plugin blacks out unknown items from the almanac");
        _UseIgnoreList = config("1 - General", "Use Ignore List", Toggle.On, "If on, the plugin uses the IgnoreList.yml to filter almanac");
        _ShowAllData = config("1 - General", "Show All Data", Toggle.Off, "If on, Almanac does not filter extra data, like prefab name and material name");
        _allowConversion = config("1 - General", "Enable Conversion", Toggle.On, "If on, players can convert chosen item into almanac tokens");
        _conversionRate = config("1 - General", "Conversion Rate", 10, "Set conversion ratio, ie. 10 coins for 1 token");
        _conversionItem = config("1 - General", "Conversion Item", "Coins",
            "Set item to use to convert into almanac tokens");
        _lotteryEnabled = config("2 - Tabs", "Lottery", Toggle.On, "If on, lottery feature is enabled");
        _lotteryEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Lottery].SetActive(ShowLottery);
        _statusEffectsEnabled = config("2 - Tabs", "Status Effects", Toggle.On, "If on, status effects is enabled");
        _statusEffectsEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.StatusEffects].SetActive(ShowStatusEffects);
        _AchievementsEnabled = config("2 - Tabs", "Achievements", Toggle.On, "If on, achievements is enabled");
        _AchievementsEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Achievements].SetActive(ShowAchievements);
        _storeEnabled =config("2 - Tabs", "Store", Toggle.On, "If on, store is enabled");
        _storeEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Store].SetActive(ShowStore);
        _metricsEnabled = config("2 - Tabs", "Metrics",  Toggle.On, "If on, metrics is enabled");
        _metricsEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Metrics].SetActive(ShowMetrics);
        _leaderboardEnabled = config("2 - Tabs", "Leaderboard",  Toggle.On, "If on, leaderboard is enabled");
        _leaderboardEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Leaderboard].SetActive(ShowLeaderboard);
        _marketplaceEnabled = config("2 - Tabs", "Marketplace", Toggle.On, "If on, marketplace extends store tab");
        
        _TreasureCooldown = config("4 - Treasures", "Cooldown", 30, "Set cooldown between treasure hunts, in minutes");
        _TreasureEnabled = config("4 - Treasures", "Enabled", Toggle.On, "If on, treasure feature is enabled");
        _TreasureEnabled.SettingChanged += (_, _) =>
            AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Treasures].SetActive(ShowTreasures);
        
        _BountyCooldown = config("3 - Bounties", "Cooldown", 30, "Set cooldown between bounty hunts, in minutes");
        _BountyEnabled = config("3 - Bounties", "Enabled", Toggle.On, "If on, bounty feature is enabled");
        _BountyEnabled.SettingChanged += (_, _) =>
            AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Bounties].SetActive(ShowBounties);
        
        _lotteryCost = config("4 - Lottery", "Cost to Roll", 1, "Set cost to roll");
        _minFullHouse = config("4 - Lottery", "Min Full House", 10, "Set minimum full house reward");

        _HideUnknownEntries = config("5 - User Interface", "Hide Unknown", Toggle.Off, "If on, elements are not displayed instead of blacked out");
        AlmanacPanel.panelPos = config("5 - User Interface", "Panel Position", new Vector3(410f, 200f, 0f), "Set position of panel");
        AlmanacPanel.panelPos.SettingChanged += (_, _) => AlmanacPanel.instance?.SetPanelPosition(AlmanacPanel.panelPos.Value);
        _OutlineColor = config("5 - User Interface", "Outline Color", Helpers._OrangeColor, "Set the color of the outline for selected items");
        _OutlineColor.SettingChanged += AlmanacPanel.OnSelectedColorChange;
        _Transparent = config("5 - User Interface", "Transparent Panels", Toggle.Off, "If on, panels are transparent");
        _Transparent.SettingChanged += (_, _) =>
        {
            if (AlmanacPanel.instance == null || Modal.instance == null) return;
            AlmanacPanel.instance.background.SetBackground(bkgOption);
            Modal.instance.background.SetBackground(bkgOption);
        };
        
        SetupWatcher();
    }
    
    private static void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, AlmanacPlugin.ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }
    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(AlmanacPlugin.ConfigFileFullPath)) return;
        try
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("ReadConfigValues called");
            AlmanacPlugin.instance.Config.Reload();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogError($"There was an issue loading your {AlmanacPlugin.ConfigFileName}");
            AlmanacPlugin.AlmanacLogger.LogError("Please check your config entries for spelling and format!");
        }
    }
    private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        ConfigEntry<T> configEntry = AlmanacPlugin.instance.Config.Bind(group, name, value, extendedDescription);

        SyncedConfigEntry<T> syncedConfigEntry = AlmanacPlugin.ConfigSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }
    private static ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }
}