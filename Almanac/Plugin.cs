using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Almanac.Almanac;
using Almanac.Managers;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using UnityEngine.Rendering;

namespace Almanac
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInIncompatibility("randyknapp.mods.auga")]
    [BepInDependency("Krumpac.Krumpac_Reforge_Core", BepInDependency.DependencyFlags.SoftDependency)]
    public class AlmanacPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Almanac";
        internal const string ModVersion = "2.2.9";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource AlmanacLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

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

        public static bool KrumpacLoaded = false;

        public static WorkingAs WorkingAsType;

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "0 - Lock Configuration", Toggle.On,
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

            // List<string> IgnoredList = new()
            // {
            //     "StaminaUpgrade_Greydwarf",
            //     "StaminaUpgrade_Troll",
            //     "StaminaUpgrade_Wraith",
            //     "IronOre",
            //     "DvergerArbalest_shoot",
            //     "DvergerArbalest",
            //     "CapeTest",
            //     "SledgeCheat",
            //     "SwordCheat",
            //     "HealthUpgrade_Bonemass",
            //     "HealthUpgrade_GDKing",
            //     "guard_stone_test",
            //     "Trailership",
            //     "Player",
            //     "TorchMist",
            //     "NPC_HelmetIron_Worn0",
            //     "NPC_HelmetBronze_Worn0",
            //     "NPC_ArmorIronChest_Worn",
            //     "NPC_ArmorIronLegs_Worn",
            //     "TrainingDummy",
            //     "VegvisirShard_Bonemass"
            // };
            // string ignoredPrefabs = string.Join(",", IgnoredList);

            // _IgnoredPrefabs = config("3 - Utilities", "Ignored Prefabs", ignoredPrefabs,
            //     "List of prefabs ignored by almanac upon launch");
            
            _AchievementPowers = config("4 - Achievements", "Powers Enabled", Toggle.On,
                "If on, achievements reward players with powers");

            _VisualEffectThreshold = config("4 - Achievements", "Visual Effects Threshold", 15f,
                "Determines the value threshold for the visual effects to be activated");

            _VisualEffects = config("4 - Achievements", "Visual Effects Enabled", Toggle.On,
                "If on, when player passes threshold, visual effects are applied to character", false);

            _AchievementPanelSize = config("4 - Achievements", "Panel Size", new Vector2(1f, 1f),
                "Set the size of the achievement panel", false);

            _LeaderboardRefreshRate = config("5 - Leaderboard", "Refresh rate", 5f,
                "Leaderboard refresh rate in minutes", true);
            
            List<string> meadowCreatures = new()
            {
                "defeated_greyling",
                "defeated_neck",
                "defeated_boar",
                "defeated_deer",
                "defeated_eikthyr"
            };

            List<string> blackForestCreatures = new()
            {
                "defeated_skeleton",
                "KilledTroll",
                "defeated_ghost",
                "defeated_greydwarf",
                "defeated_greydwarf_elite",
                "defeated_greydwarf_shaman",
                "defeated_gdking"
            };

            List<string> swampCreatures = new()
            {
                "defeated_blob",
                "defeated_blobelite",
                "defeated_draugr",
                "defeated_draugr_elite",
                "defeated_skeleton_poison",
                "killed_surtling",
                "defeated_wraith",
                "defeated_leech",
                "defeated_bonemass"
            };

            List<string> mountainCreatures = new()
            {
                "defeated_wolf",
                "defeated_fenring",
                "defeated_hatchling",
                "KilledBat",
                "defeated_fenring_cultist",
                "defeated_stonegolem",
                "defeated_ulv",
                "defeated_dragon"
            };

            List<string> plainsCreatures = new()
            {
                "defeated_deathsquito",
                "defeated_goblin",
                "defeated_goblinbrute",
                "defeated_goblinshaman",
                "defeated_lox",
                "defeated_blobtar",
                "defeated_goblinking"
            };

            List<string> mistLandCreatures = new()
            {
                "defeated_dverger",
                "defeated_dvergermagefire",
                "defeated_dvergermagesupport",
                "defeated_dvergermageice",
                "defeated_gjall",
                "defeated_tick",
                "defeated_hare",
                "defeated_seeker",
                "defeated_seekerbrood",
                "defeated_seekerbrute",
                "defeated_queen"
            };

            List<string> deepNorthCreatures = new();
            List<string> ashlandsCreatures = new();

            _MeadowCreatures = config("4 - Achievements", "01 - Meadow List", ConvertListToString(meadowCreatures),
                "List of meadow creature defeat keys to track biome kills");

            _BlackForestCreatures = config("4 - Achievements", "02 - Black Forest List",
                ConvertListToString(blackForestCreatures), "List of black forest defeat keys to track biome kills");

            _SwampCreatures = config("4 - Achievements", "03 - Swamp List", ConvertListToString(swampCreatures),
                "List of swamp defeat keys to track biome kills");

            _MountainCreatures = config("4 - Achievements", "04 - Mountain List", ConvertListToString(mountainCreatures),
                "List of mountain defeat keys to track biome kills");

            _PlainsCreatures = config("4 - Achievements", "05 - Plains List", ConvertListToString(plainsCreatures),
                "List of plains defeat keys to track biome kills");

            _MistLandCreatures = config("4 - Achievements", "06 - Mistland List", ConvertListToString(mistLandCreatures),
                "List of mistland defeat keys to track biome kills");

            _DeepNorthCreatures = config("4 - Achievements", "07 - Deep North List", "",
                "List of deep north creatures to track biome kills");

            _AshLandsCreatures = config("4 - Achievements", "08 - AshLands List", "",
                "List of ashland defeat keys to track for biome kills");

            _TransparentPanel = config("1 - General", "Transparent Background", Toggle.Off,
                "If on, almanac panels become transparent", false);

            _UsePrivateKeys = config("1 - General", "Use Private Keys", Toggle.Off,
                "If on, creature data is unlocked using private keys, to be used with world advancement progression",
                false);

            _EnableHudIcons = config("1 - General", "HUD Icons", Toggle.Off,
                "If on, HUD displays almanac status effects");
            WorkingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server : WorkingAs.Client;
            
            Localizer.Load();

            if (Chainloader.PluginInfos.ContainsKey("Krumpac.Krumpac_Reforge_Core"))
            {
                AlmanacLogger.LogWarning("Krumpac is loaded by chainloader");
                
                KrumpacLoaded = true;
            }
            
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            IgnoreList.InitIgnoreList();
            BlackList.InitBlackList();
            FileSystem.InitializeFileSystemWatch();
        }

        private static string ConvertListToString(List<string> input)
        {
            string output = input[0];
            for (int i = 1; i < input.Count; ++i)
            {
                output += ",";
                output += input[i];
            }

            return output;
        }

        public static List<string> StringToListDefeatKeys(string input)
        {
            List<string> output = new();
            string[] inputList = input.Split(',');
            foreach (string item in inputList)
            {
                AlmanacLogger.LogDebug($"Failed to find key: {item}");
                if (!CreatureDataCollector.tempCreatureData.Exists(x => x.defeatedKey == item)) continue;
                output.Add(item);
            }

            return output;
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
        // public static ConfigEntry<string> _IgnoredPrefabs = null!;

        public static ConfigEntry<Toggle> _AchievementPowers = null!;
        public static ConfigEntry<float> _VisualEffectThreshold = null!;
        public static ConfigEntry<Toggle> _VisualEffects = null!;
        public static ConfigEntry<Vector2> _AchievementPanelSize = null!;
        public static ConfigEntry<float> _LeaderboardRefreshRate = null!;

        public static ConfigEntry<string> _MeadowCreatures = null!;
        public static ConfigEntry<string> _BlackForestCreatures = null!;
        public static ConfigEntry<string> _SwampCreatures = null!;
        public static ConfigEntry<string> _MountainCreatures = null!;
        public static ConfigEntry<string> _PlainsCreatures = null!;
        public static ConfigEntry<string> _MistLandCreatures = null!;
        public static ConfigEntry<string> _DeepNorthCreatures = null!;
        public static ConfigEntry<string> _AshLandsCreatures = null!;

        public static ConfigEntry<Toggle> _TransparentPanel = null!;
        public static ConfigEntry<Toggle> _UsePrivateKeys = null!;
        public static ConfigEntry<Toggle> _EnableHudIcons = null!;

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