using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using Almanac.Almanac;
using Almanac.Managers;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using UnityEngine.Rendering;
using YamlDotNet.Serialization;
using CompressionLevel = UnityEngine.CompressionLevel;
using Patches = Almanac.Almanac.Patches;


namespace Almanac
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class AlmanacPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Almanac";
        internal const string ModVersion = "2.2.0";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource AlmanacLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static readonly ConfigSync ConfigSync = new(ModGUID)
        { 
            DisplayName = ModName, 
            ModRequired = false,
            MinimumRequiredVersion = "0.0.0"
            // CurrentVersion = ModVersion
        };
        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public static readonly Sprite? questionMarkIcon = SpriteManager.RegisterSprite("AlmanacUnknownIcon.png");
        public static readonly Sprite? AlmanacIconButton = SpriteManager.RegisterSprite("AlmanacIconButton.png");
        public static readonly Sprite? boneSkullIcon = SpriteManager.RegisterSprite("bone_skull.png");
        public static readonly Sprite? swordBasicBlueIcon =SpriteManager.RegisterSprite("sword_basic_blue.png");
        public static readonly Sprite? swordBasicBrownIcon = SpriteManager.RegisterSprite("sword_basic4_blue.png");
        public static readonly Sprite? arrowBasicIcon = SpriteManager.RegisterSprite("arrow_basic.png");
        public static readonly Sprite? capeHoodIcon = SpriteManager.RegisterSprite("cape_hood_darkyellow.png");
        public static readonly Sprite? bottleStandardEmptyIcon = SpriteManager.RegisterSprite("bottle_standard_empty.png");
        public static readonly Sprite? bottleStandardBlueIcon = SpriteManager.RegisterSprite("bottle_standard_blue.png");
        public static readonly Sprite? fishGreenIcon = SpriteManager.RegisterSprite("fish_green.png");
        public static readonly Sprite? bowWoodIcon = SpriteManager.RegisterSprite("bow_wood1.png");
        public static readonly Sprite? necklaceSilverRed = SpriteManager.RegisterSprite("necklace_silver_red.png");
        public static readonly Sprite? mushroomBigRedIcon = SpriteManager.RegisterSprite("mushroom_big_red.png");
        public static readonly Sprite? goldCoinsPileIcon = SpriteManager.RegisterSprite("gold_coins_many.png");
        public static readonly Sprite? keySilverIcon = SpriteManager.RegisterSprite("key_silver.png");
        public static readonly Sprite? boneWhiteIcon = SpriteManager.RegisterSprite("bone_white.png");
        public static readonly Sprite? bookClosedRedIcon = SpriteManager.RegisterSprite("book_closed_red.png");
        public static readonly Sprite? bottleStandardGreenIcon = SpriteManager.RegisterSprite("bottle_standard_green.png");
        public static readonly Sprite? crownGoldIcon = SpriteManager.RegisterSprite("crown_gold.png");
        public static readonly Sprite? gemDiamondRedIcon = SpriteManager.RegisterSprite("gem_diamond_red.png");
        public static readonly Sprite? goldBarsIcon = SpriteManager.RegisterSprite("gold_bars_three.png");
        public static readonly Sprite? scrollMapIcon = SpriteManager.RegisterSprite("scroll_map2.png");
        public static readonly Sprite? shieldBasicIcon = SpriteManager.RegisterSprite("shield_basic_metal.png");
        public static readonly Sprite? silverBarsIcon = SpriteManager.RegisterSprite("silver_bars.png");
        public static readonly Sprite? silverCoinsIcon = SpriteManager.RegisterSprite("silver_coins_many.png");
        public static readonly Sprite? woodLogIcon = SpriteManager.RegisterSprite("wood_log.png");
        public static readonly Sprite? woodLogsIcon = SpriteManager.RegisterSprite("wood_logs_three.png");

        public enum WorkingAs
        {
            Client,
            Server,
            Both
        }

        public static WorkingAs WorkingAsType;

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            _normalColorConfig = config("2 - Resistance Colors", "Normal", Color.white, "Color code for normal damages", false);
            _weakColorConfig = config("2 - Resistance Colors", "Weak", new Color(1f, 0.7f, 0f, 1f), "Color code for weak damages", false);
            _veryWeakColorConfig = config("2 - Resistance Colors", "Very Weak", new Color(1f, 0.8f, 0f, 1f), "Color code for very weak damages", false);
            _resistantColorConfig = config("2 - Resistance Colors", "Resistant", new Color(0.5f, 0.5f, 0.5f, 1f), "Color code for resistant damages", false);
            _veryResistantColorConfig = config("2 - Resistance Colors", "Very Resistant", new Color(0.5f, 0.5f, 0.5f, 1f), "Color code for very resistant damages", false);
            _immuneColorConfig = config("2 - Resistance Colors", "Immune", new Color(0.5f, 0.5f, 1f, 1f), "Color code for immune damages", false);
            _ignoreColorConfig = config("2 - Resistance Colors", "Ignore", new Color(0.5f, 0.5f, 1f, 1f), "Color code for ignore damages", false);

            _KnowledgeLock = config("3 - Utilities", "Knowledge Wall", Toggle.On,
                "If on, data is locked behind knowledge of item", true);

            List<string> IgnoredList = new()
            {
                "StaminaUpgrade_Greydwarf",
                "StaminaUpgrade_Troll",
                "StaminaUpgrade_Wraith",
                "IronOre",
                "DvergerArbalest_shoot",
                "DvergerArbalest",
                "CapeTest",
                "SledgeCheat",
                "SwordCheat",
                "HealthUpgrade_Bonemass",
                "HealthUpgrade_GDKing",
                "guard_stone_test",
                "Trailership",
                "Player",
                "TorchMist",
                "NPC_HelmetIron_Worn0",
                "NPC_HelmetBronze_Worn0",
                "NPC_ArmorIronChest_Worn",
                "NPC_ArmorIronLegs_Worn",
                "TrainingDummy",
                "VegvisirShard_Bonemass"
            };
            string ignoredPrefabs = string.Join(",", IgnoredList);

            _IgnoredPrefabs = config("3 - Utilities", "Ignored Prefabs", ignoredPrefabs,
                "List of prefabs ignored by almanac upon launch");
            
            _AchievementPowers = config("4 - Achievements", "Powers Enabled", Toggle.On,
                "If on, achievements reward players with powers");

            _VisualEffectThreshold = config("4 - Achievements", "Visual Effects Threshold", 15f,
                "Determines the value threshold for the visual effects to be activated");

            _VisualEffects = config("4 - Achievements", "Visual Effects Enabled", Toggle.On,
                "If on, when player passes threshold, visual effects are applied to character", false);

            _AchievementPanelSize = config("4 - Achievements", "Panel Size", new Vector2(1f, 1f),
                "Set the size of the achievement panel", false);

            _LeaderboardRefreshRate = config("5 - Leaderboard", "Refresh rate", 5f,
                "Leaderboard refresh rate in minutes", false);

            WorkingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server : WorkingAs.Client;
            
            Localizer.Load();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            BlackList.InitBlackList();
            FileSystem.InitializeFileSystemWatch();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                AlmanacLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                AlmanacLogger.LogError($"There was an issue loading your {ConfigFileName}");
                AlmanacLogger.LogError("Please check your config entries for spelling and format!");
            }

            if (_AchievementPowers.Value is Toggle.On) return;
            if (!Player.m_localPlayer) return;
            List<StatusEffect> activeEffects = Player.m_localPlayer.m_seman.GetStatusEffects();
            List<StatusEffect> effectsToRemove = new List<StatusEffect>();

            foreach (StatusEffect effect in activeEffects)
            {
                if (RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name))
                {
                    effectsToRemove.Add(effect);
                }
            }
            foreach (StatusEffect effect in effectsToRemove)
            {
                Player.m_localPlayer.m_seman.RemoveStatusEffect(effect);
            }
            

        }

        #region ConfigOptions
        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        public static ConfigEntry<Color> _normalColorConfig = null!;
        public static ConfigEntry<Color> _weakColorConfig = null!;
        public static ConfigEntry<Color> _veryWeakColorConfig = null!;
        public static ConfigEntry<Color> _resistantColorConfig = null!;
        public static ConfigEntry<Color> _veryResistantColorConfig = null!;
        public static ConfigEntry<Color> _ignoreColorConfig = null!;
        public static ConfigEntry<Color> _immuneColorConfig = null!;
        public static ConfigEntry<Toggle> _KnowledgeLock = null!;
        public static ConfigEntry<string> _IgnoredPrefabs = null!;

        public static ConfigEntry<Toggle> _AchievementPowers = null!;
        public static ConfigEntry<float> _VisualEffectThreshold = null!;
        public static ConfigEntry<Toggle> _VisualEffects = null!;
        public static ConfigEntry<Vector2> _AchievementPanelSize = null!;
        public static ConfigEntry<float> _LeaderboardRefreshRate = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        // private class ConfigurationManagerAttributes
        // {
        //     [UsedImplicitly] public int? Order;
        //     [UsedImplicitly] public bool? Browsable;
        //     [UsedImplicitly] public string? Category;
        //     [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        // }

        // class AcceptableShortcuts : AcceptableValueBase
        // {
        //     public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
        //     {
        //     }
        //
        //     public override object Clamp(object value) => value;
        //     public override bool IsValid(object value) => true;
        //
        //     public override string ToDescriptionString() =>
        //         "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        // }

        #endregion
    }
}