using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using UnityEngine;


namespace Almanac
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class AlmanacPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Almanac";
        internal const string ModVersion = "1.0.5";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource AlmanacLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        // To set requirements of install
        
        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, ModRequired = false};
        
        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            _normalColorConfig = config("2 - Resistance Colors", "Normal", Color.white, "Color code for normal damages", false);
            _weakColorConfig = config("2 - Resistance Colors", "Weak", new Color(1f, 0.7f, 0f, 1f), "Color code for weak damages", false);
            _veryWeakColorConfig = config("2 - Resistance Colors", "Very Weak", new Color(1f, 0.8f, 0f, 1f), "Color code for very weak damages", false);
            _resistantColorConfig = config("2 - Resistance Colors", "Resistant", new Color(0.5f, 0.5f, 0.5f, 1f), "Color code for resistant damages", false);
            _veryResistantColorConfig = config("2 - Resistance Colors", "Very Resistant", new Color(0.5f, 0.5f, 0.5f, 1f), "Color code for very resistant damages", false);
            _immuneColorConfig = config("2 - Resistance Colors", "Immune", new Color(0.5f, 0.5f, 1f, 1f), "Color code for immune damages", false);
            _ignoreColorConfig = config("2 - Resistance Colors", "Ignore", new Color(0.5f, 0.5f, 1f, 1f), "Color code for ignore damages", false);
            
            Localizer.Load();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
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
        }


        #region ConfigOptions
        
        public static ConfigEntry<Color> _normalColorConfig = null!;
        public static ConfigEntry<Color> _weakColorConfig = null!;
        public static ConfigEntry<Color> _veryWeakColorConfig = null!;
        public static ConfigEntry<Color> _resistantColorConfig = null!;
        public static ConfigEntry<Color> _veryResistantColorConfig = null!;
        public static ConfigEntry<Color> _ignoreColorConfig = null!;
        public static ConfigEntry<Color> _immuneColorConfig = null!;


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

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        }

        #endregion
    }
}