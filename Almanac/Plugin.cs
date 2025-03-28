﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Almanac.Achievements;
using Almanac.API;
using Almanac.Bounties;
using Almanac.FileSystem;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using UnityEngine.Rendering;
using YamlDotNet.Serialization;

namespace Almanac
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInIncompatibility("randyknapp.mods.auga")]
    [BepInDependency("Krumpac.Krumpac_Reforge_Core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("kg.ValheimEnchantmentSystem", BepInDependency.DependencyFlags.SoftDependency)]
    public class AlmanacPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Almanac";
        internal const string ModVersion = "3.4.0";
        internal const string Author = "RustyMods";
        public const string ModGUID = Author + "." + ModName;
        private static readonly string ConfigFileName = ModGUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AlmanacLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static AlmanacPlugin _plugin = null!;
        public static GameObject _root = null!;
        public static readonly AssetBundle _UIAssets = GetAssetBundle("almanacui");
        public void SpawnBounty()
        {
            if (Bounty.ActiveBountyLocation is not { } bountyData) return;
            GameObject go = Instantiate(bountyData.m_critter, bountyData.m_position, Quaternion.identity);

            Bounty bounty = go.AddComponent<Bounty>();
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(bountyData.m_data);
            bounty._znv.GetZDO().Set(Bounty.bountyHash, data);
            CachedEffects.m_spawnEffects.Create(go.transform.position, Quaternion.identity);
        }
        
        public void Awake()
        {
            Localizer.Load();
            
            _plugin = this;
            _root = new GameObject("root");
            DontDestroyOnLoad(_root);
            _root.SetActive(false);
            
            InitConfigs();
            CheckChainLoader();
            AlmanacPaths.CreateFolderDirectories();
            AchievementYML.InitDefaultAchievements();
            Filters.InitFilters();
            FileWatcher.InitFileSystemWatch();
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            // TrackMinimalUI.SetupWatcher();
        }

        public void Update()
        {
            if (!Player.m_localPlayer || !AlmanacUI.m_instance) return;
            if (!Input.GetKeyDown(_AlmanacHotKey.Value)) return;
            AlmanacUI.m_instance.Toggle();
        }

        private void OnDestroy() => Config.Save();
        
        #region Chainloader
        public static bool KrumpacLoaded = false;
        public static bool JewelCraftLoaded = false;
        public static bool KGEnchantmentLoaded = false;
        private static void CheckChainLoader()
        {
            if (Chainloader.PluginInfos.ContainsKey("kg.ValheimEnchantmentSystem"))
            {
                AlmanacLogger.LogInfo("KG Enchantment System Loaded");
                KGEnchantmentLoaded = true;
            }
            
            if (Chainloader.PluginInfos.ContainsKey("Krumpac.Krumpac_Reforge_Core"))
            {
                AlmanacLogger.LogInfo("Krumpac Loaded");
                KrumpacLoaded = true;
            }

            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.jewelcrafting"))
            {
                AlmanacLogger.LogInfo("Jewel Crafting Loaded");
                JewelCraftLoaded = true;
            }
        }
        #endregion
        
        #region Utililies
        public enum Toggle { On = 1, Off = 0 }
        private static AssetBundle GetAssetBundle(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
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
        }
        #endregion
        
        # region Configurations
        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        public static ConfigEntry<Toggle> _KnowledgeWall = null!;
        public static ConfigEntry<Toggle> _UseIgnoreList = null!;
        public static ConfigEntry<Toggle> _AchievementIcons = null!;
        public static ConfigEntry<Toggle> _AchievementPowers = null!;
        public static ConfigEntry<int> _AchievementThreshold = null!;
        public static ConfigEntry<Toggle> _ShowAllData = null!;
        public static ConfigEntry<Color> _OutlineColor = null!;
        public static ConfigEntry<KeyCode> _AlmanacHotKey = null!;
        public static ConfigEntry<Toggle> _LoadDefaultAchievements = null!;
        public static ConfigEntry<int> _TreasureCooldown = null!;
        public static ConfigEntry<int> _BountyCooldown = null!;
        public static ConfigEntry<Toggle> _TreasureEnabled = null!;
        public static ConfigEntry<Toggle> _BountyEnabled = null!;
        public static ConfigEntry<Toggle> _AchievementsEnabled = null!;
        public static ConfigEntry<Toggle> _showLore = null!;
        private void InitConfigs()
        {
            _serverConfigLocked = config("1 - General", "0 - Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            _KnowledgeWall = config("1 - General", "1 - Knowledge Wall", Toggle.On, "If on, the plugin blacks out unknown items from the almanac");
            _UseIgnoreList = config("1 - General", "2 - Use Ignore List", Toggle.On, "If on, the plugin uses the IgnoreList.yml to filter almanac");
            _ShowAllData = config("1 - General", "3 - Show All Data", Toggle.Off, "If on, Almanac does not filter extra data, like prefab name and material name");
            _AchievementIcons = config("Achievements", "HUD Icons", Toggle.Off, "If on, achievements icons appear alongside status effects on HUD");
            _AchievementIcons.SettingChanged += AchievementManager.OnAchievementConfigChanged;
            _AchievementPowers = config("Achievements", "Bonuses Enabled", Toggle.On, "If on, achievements are interactable and reward players with bonuses");
            _AchievementPowers.SettingChanged += AchievementManager.OnAchievementConfigChanged;
            _AchievementThreshold = config("Achievements", "Threshold", 3, "Total amount of achievement effects allowed at the same time");
            _OutlineColor = config("1 - General", "6 - Outline Color", Color.yellow, "Set the color of the outline for selected items");
            _AlmanacHotKey = config("1 - General", "7 - Almanac HotKey", KeyCode.F6, "Set the hotkey to open almanac", false);
            _LoadDefaultAchievements = config("1 - General", "8 - Load Default Achievements", Toggle.Off,
                "If on, Almanac will write any missing default achievements to file", false);
            _TreasureCooldown = config("Cooldown", "Treasure Hunt", 30,
                "Set cooldown between treasure hunts, in minutes");
            _BountyCooldown = config("Cooldown", "Bounties", 30, "Set cooldown between bounty hunts, in minutes");
            _BountyEnabled = config("2 - Settings", "Bounties", Toggle.On, "If on, bounty feature is enabled");
            _TreasureEnabled = config("2 - Settings", "Treasures", Toggle.On, "If on, treasure feature is enabled");
            _AchievementsEnabled = config("3 - Achievements", "Enabled", Toggle.On, "If on, achievements is enabled");
            _showLore = config("3 - Achievements", "Always Show Lore", Toggle.On, "If on, achievements lore is always displayed");
        }
        #endregion

        #region ConfigOptions
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
        #endregion
    }
}