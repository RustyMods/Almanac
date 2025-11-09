using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.UI;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace Almanac.Utilities;

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
    public static ConfigEntry<Vector3> _customizationPos = null!;
    public static ConfigEntry<Vector3> _dialoguePos = null!;
    public static ConfigEntry<Vector3> _panelPos = null!;

    private static ConfigEntry<int> _TreasureCooldown = null!;
    private static ConfigEntry<int> _BountyCooldown = null!;
    private static ConfigEntry<int> _lotteryCost = null!;
    private static ConfigEntry<int> _minFullHouse = null!;
    private static ConfigEntry<Toggle> _generateBounties = null!;
    
    private static ConfigEntry<Toggle> _TreasureEnabled = null!;
    private static ConfigEntry<Toggle> _BountyEnabled = null!;
    private static ConfigEntry<Toggle> _AchievementsEnabled = null!;
    private static ConfigEntry<Toggle> _lotteryEnabled = null!;
    private static ConfigEntry<Toggle> _statusEffectsEnabled = null!;
    private static ConfigEntry<Toggle> _storeEnabled = null!;
    private static ConfigEntry<Toggle> _metricsEnabled = null!;
    private static ConfigEntry<Toggle> _leaderboardEnabled = null!;
    private static ConfigEntry<Toggle> _marketplaceEnabled = null!;
    private static ConfigEntry<Toggle> _trophiesEnabled = null!;
    private static ConfigEntry<Toggle> _itemsEnabled = null!;
    private static ConfigEntry<Toggle> _piecesEnabled = null!;
    private static ConfigEntry<Toggle> _creaturesEnabled = null!;
    private static ConfigEntry<Toggle> _almanacEnabled = null!;
    private static ConfigEntry<Vector2> _elementViewGridSize = null!;
    public static ConfigEntry<Vector3> _questPanelPos = null!;
    private static ConfigEntry<KeyCode> _questKey = null!;
    
    private static ConfigEntry<Toggle> _allowConversion = null!;
    private static ConfigEntry<int> _conversionRate = null!;
    private static ConfigEntry<string> _conversionItem = null!;
    private static ConfigEntry<float> _scrollbarSensitivity = null!;
    private static ConfigEntry<Toggle> _returnBountyCostWhenCancel = null!;
    private static ConfigEntry<Toggle> _returnTreasureCostWhenCancel = null!;

    private static ConfigEntry<Toggle> _checkNoCost = null!;
    private static ConfigEntry<string> _adminList = null!;
    private static ConfigEntry<Toggle> _enableAchievementNotices = null!;

    private static ConfigEntry<Toggle> _enableAchievementEffects = null!;
    private static ConfigEntry<int> _maxAchievementEffects = null!;
    
    public static bool UseIgnoreList => _UseIgnoreList.Value is Toggle.On;
    public static bool ShowBounties => _BountyEnabled.Value is Toggle.On;
    public static bool ShowAchievements => _AchievementsEnabled.Value is Toggle.On;
    public static bool ShowTreasures => _TreasureEnabled.Value is Toggle.On;
    public static bool ShowAllData => _ShowAllData.Value is Toggle.On;
    public static bool UseKnowledgeWall => _KnowledgeWall.Value is Toggle.On;
    
    public static bool AchievementEffectsEnabled => _enableAchievementEffects.Value is Toggle.On;
    public static int MaxAchievementEffects => _maxAchievementEffects.Value;
    
    public static bool ShowAchievementNotices => _enableAchievementNotices.Value is Toggle.On;
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
    public static float ScrollbarSensitivity => _scrollbarSensitivity.Value;
    public static bool ReturnBountyCostWhenCancel => _returnBountyCostWhenCancel.Value is Toggle.On;
    public static bool ReturnTreasureCostWhenCancel => _returnTreasureCostWhenCancel.Value is Toggle.On;
    public static bool ShowTrophies => _trophiesEnabled.Value is Toggle.On;
    public static bool ShowItems => _itemsEnabled.Value is Toggle.On;
    public static bool ShowPieces => _piecesEnabled.Value is Toggle.On;
    public static bool ShowCreatures => _creaturesEnabled.Value is Toggle.On;
    public static bool ShowAlmanac => _almanacEnabled.Value is Toggle.On;
    public static Vector3 QuestPanelPos => _questPanelPos.Value;
    public static Vector2 ElementViewGridSize => _elementViewGridSize.Value;
    public static Vector3 CustomizationPos => _customizationPos.Value;
    public static Vector3 DialoguePos => _dialoguePos.Value;
    public static KeyCode QuestKey => _questKey.Value;
    public static Vector3 PanelPos => _panelPos.Value;
    public static bool GenerateBounties => _generateBounties.Value is Toggle.On;
    private static string AdminList => _adminList.Value;
    public static bool IsAdmin(string name) => new StringListConfig(AdminList).list.Select(x => x.ToLower()).Contains(name.ToLower());
    public static bool CheckNoCost => _checkNoCost.Value is Toggle.On;
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
        _conversionRate = config("1 - General", "Conversion Rate", 50, "Set conversion ratio, ie. 50 coins for 1 token");
        _conversionItem = config("1 - General", "Conversion Item", "Coins", "Set item to use to convert into almanac tokens");
        _almanacEnabled = config("1 - General", "Almanac Button", Toggle.On, "If on, almanac button replaces trophies button");
        _checkNoCost = config("1 - General", "Admin Tools Check No Cost", Toggle.Off, "If on, no cost must be enabled to use admin tools");
        _enableAchievementNotices = config("1 - General", "Achievement Notices", Toggle.Off,
            "If on, will show notifications of achievement completion");
        _adminList = config("1 - General", "Admin List", "",
            new ConfigDescription("Almanac admin list, names seperated by a comma", null,
                new ConfigurationManagerAttributes() { CustomDrawer = StringListConfig.Draw }));
        _almanacEnabled.SettingChanged += (_, _) => AlmanacPanel.inventoryButton?.Show(ShowAlmanac);
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
        _TreasureEnabled = config("2 - Tabs", "Treasures", Toggle.On, "If on, treasures is enabled");
        _TreasureEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Treasures].SetActive(ShowTreasures);
        _BountyEnabled = config("2 - Tabs", "Bounties", Toggle.On, "If on, bounties is enabled");
        _BountyEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Bounties].SetActive(ShowBounties);
        _trophiesEnabled = config("2 - Tabs", "Trophies", Toggle.On, "If on, trophies is enabled");
        _trophiesEnabled.SettingChanged += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Trophies].SetActive(ShowTrophies);
        _itemsEnabled = config("2 - Tabs", "Items", Toggle.On, "If on, items are enabled");
        _itemsEnabled.SettingChanged  += (_, _) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Items].SetActive(ShowItems);
        _piecesEnabled = config("2 - Tabs", "Pieces", Toggle.On, "If on, pieces are enabled");
        _piecesEnabled.SettingChanged += (_,_) => AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Pieces].SetActive(ShowPieces);
        _creaturesEnabled = config("2 - Tabs", "Creatures", Toggle.On, "If on, creatures are enabled");
        _creaturesEnabled.SettingChanged += (_,_)=>AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Creatures].SetActive(ShowCreatures);
        
        _TreasureCooldown = config("4 - Treasures", "Cooldown", 30, "Set cooldown between treasure hunts, in minutes");
        _returnTreasureCostWhenCancel = config("4 - Treasures", "Cancel Return Cost", Toggle.Off,
            "If on, cost will be reimbursed when canceled");
        
        _BountyCooldown = config("3 - Bounties", "Cooldown", 30, "Set cooldown between bounty hunts, in minutes");
        _returnBountyCostWhenCancel = config("3 - Bounties", "Cancel Return Cost", Toggle.Off, "If on, cost will be reimbursed when canceled");
        _generateBounties = config("3 - Bounties", "Generate", Toggle.Off, "If on, will generate random bounties");
        _lotteryCost = config("4 - Lottery", "Cost to Roll", 1, "Set cost to roll");
        _minFullHouse = config("4 - Lottery", "Min Full House", 10, "Set minimum full house reward");

        _HideUnknownEntries = config("5 - User Interface", "Hide Unknown", Toggle.Off, "If on, elements are not displayed instead of blacked out");
        _panelPos = config("5 - User Interface", "Panel Position", new Vector3(410f, 200f, 0f), "Set position of panel", false);
        _panelPos.SettingChanged += (_, _) => AlmanacPanel.instance?.SetPanelPosition(PanelPos);
        _OutlineColor = config("5 - User Interface", "Outline Color", Helpers._OrangeColor, "Set the color of the outline for selected items", false);
        _OutlineColor.SettingChanged += AlmanacPanel.OnSelectedColorChange;
        _Transparent = config("5 - User Interface", "Transparent Panels", Toggle.Off, "If on, panels are transparent", false);
        _Transparent.SettingChanged += (_, _) =>
        {
            if (AlmanacPanel.instance == null || FormPanel.instance == null || NPCCustomization.instance == null) return;
            AlmanacPanel.instance.background.SetBackground(bkgOption);
            FormPanel.instance.background.SetBackground(bkgOption);
            NPCCustomization.instance.background.SetBackground(bkgOption);
        };
        _scrollbarSensitivity = config("5 - User Interface", "Scroll Sensitivity", 700f, "High is more sensitive", false);
        _scrollbarSensitivity.SettingChanged += (_, _) =>
        {
            AlmanacPanel.OnScrollbarSensitivityChanged(ScrollbarSensitivity);
            NPCCustomization.View.OnScrollbarSensitivityChanged(ScrollbarSensitivity);
        };
        _customizationPos = config("5 - User Interface", "NPC Customization", new Vector3(490f, 170f, 0f), "Set position of customization", false);
        _dialoguePos = config("5 - User Interface", "NPC Dialogue", new Vector3(980f, 365f, 0f), "Set position of dialogue", false);
        _dialoguePos.SettingChanged += DialoguePanel.OnPosChange;
        _elementViewGridSize = config("5 - User Interface", "Grid Element Size", new Vector2(180f, 180f), "");
        _elementViewGridSize.SettingChanged += (_, _) => AlmanacPanel.instance?.elementView.SetGridElementSize(ElementViewGridSize);
        _questPanelPos = config("5 - User Interface", "Quest Panel", new Vector3(45f, 870f, 0f), "Set position of quest tooltips", false);
        _questPanelPos.SettingChanged += QuestPanel.OnPosChange;
        _questKey = config("5 - User Interface", "Quest Key", KeyCode.F6, "Set key to show / hide quest tooltips", false);

        _enableAchievementEffects = config("6 - Achievements", "Status Effects", Toggle.Off,
            "If on, will allow legacy achievement feature: status effects");
        _maxAchievementEffects = config("6 - Achievements", "Max Effects", 8,
            "Set maximum amount of concurrent achievement effects");
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
    
    public class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order = null!;
        [UsedImplicitly] public bool? Browsable = null!;
        [UsedImplicitly] public string? Category = null!;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
    }
    public class StringListConfig
    {
        public readonly List<string> list;
        public StringListConfig(List<string> items) => list = items;
        public StringListConfig(string items) => list = items.Split(',').ToList();
        public static void Draw(ConfigEntryBase cfg)
        {
            bool locked = cfg.Description.Tags
                .Select(a =>
                    a.GetType().Name == "ConfigurationManagerAttributes"
                        ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                        : null).FirstOrDefault(v => v != null) ?? false;
            bool wasUpdated = false;
            List<string> strings = new();
            GUILayout.BeginVertical();
            foreach (var prefab in new StringListConfig((string)cfg.BoxedValue).list)
            {
                GUILayout.BeginHorizontal();
                var prefabName = prefab;
                var nameField = GUILayout.TextField(prefab);
                if (nameField != prefab && !locked)
                {
                    wasUpdated = true;
                    prefabName = nameField;
                }

                if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
                {
                    wasUpdated = true;
                }
                else
                {
                    strings.Add(prefabName);
                }

                if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
                {
                    strings.Add("");
                    wasUpdated = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            if (wasUpdated)
            {
                cfg.BoxedValue = new StringListConfig(strings).ToString();
            }
        }

        public override string ToString() => string.Join(",", list);
    }
}