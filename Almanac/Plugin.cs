using System.IO;
using System.Linq;
using System.Reflection;
using Almanac.Achievements;
using Almanac.Data;
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
        internal const string ModVersion = "3.0.0";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AlmanacLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        
        public enum Toggle { On = 1, Off = 0 }
        public enum WorkingAs { Client, Server, Both }
        
        public static WorkingAs WorkingAsType;
        public static AlmanacPlugin _plugin = null!;
        public static AssetBundle _assets = null!;
        
        public static bool KrumpacLoaded = false;
        public static bool JewelCraftLoaded = false;
        public static bool KGEnchantmentLoaded = false;
        public void Awake()
        {
            Localizer.Load();
            
            _plugin = this;
            WorkingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null ? WorkingAs.Server : WorkingAs.Client;
            _assets = GetAssetBundle("almanacbundle");
            
            CheckChainLoader();
            InitConfigs();
            AlmanacPaths.CreateFolderDirectories();
            CacheAssets.LoadAssets();
            AchievementYML.InitDefaultAchievements();
            Filters.InitFilters();
            FileWatcher.InitFileSystemWatch();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

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

        public void Update()
        {
            UpdateAlmanac.UpdateGUI();
        }
        private void OnDestroy()
        {
            Config.Save();
        }
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

        #region ConfigOptions
        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        public static ConfigEntry<Toggle> _KnowledgeWall = null!;
        public static ConfigEntry<Toggle> _UseIgnoreList = null!;
        public static ConfigEntry<Toggle> _AchievementIcons = null!;
        public static ConfigEntry<Toggle> _AchievementPowers = null!;
        public static ConfigEntry<int> _AchievementThreshold = null!;
        public static ConfigEntry<Toggle> _ShowAllData = null!;
        private void InitConfigs()
        {
            _serverConfigLocked = config("1 - General", "0 - Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            _KnowledgeWall = config("1 - General", "1 - Knowledge Wall", Toggle.On,
                "If on, the plugin blacks out unknown items from the almanac");

            _UseIgnoreList = config("1 - General", "2 - Use Ignore List", Toggle.On,
                "If on, the plugin uses the IgnoreList.yml to filter almanac");

            _ShowAllData = config("1 - General", "3 - Show All Data", Toggle.Off,
                "If on, Almanac does not filter extra data, like prefab name and material name");
            
            _AchievementIcons = config("Achievements", "HUD Icons", Toggle.Off,
                "If on, achievements icons appear alongside status effects on HUD");

            _AchievementIcons.SettingChanged += AchievementManager.OnAchievementConfigChanged;
            
            _AchievementPowers = config("Achievements", "Bonuses Enabled", Toggle.On,
                "If on, achievements are interactable and reward players with bonuses");

            _AchievementPowers.SettingChanged += AchievementManager.OnAchievementConfigChanged;

            _AchievementThreshold = config("Achievements", "Threshold", 3,
                new ConfigDescription("Total amount of achievement effects allowed at the same time",
                    new AcceptableValueRange<int>(1, 5)));
        }
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