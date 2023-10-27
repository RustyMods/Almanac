using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
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


namespace Almanac
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class AlmanacPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Almanac";
        internal const string ModVersion = "2.1.3";
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
                CurrentVersion = ModVersion
            };
        
        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public static Sprite? questionMarkIcon;
        public static Sprite? AlmanacIconButton;

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
            
            WorkingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server : WorkingAs.Client;
            
            Localizer.Load();

            questionMarkIcon = SpriteManager.RegisterSprite("QuestionMark.png");
            AlmanacIconButton = SpriteManager.RegisterSprite("AlmanacIconButton.png");
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            BlackList.InitBlackList();
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
        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        public static ConfigEntry<Color> _normalColorConfig = null!;
        public static ConfigEntry<Color> _weakColorConfig = null!;
        public static ConfigEntry<Color> _veryWeakColorConfig = null!;
        public static ConfigEntry<Color> _resistantColorConfig = null!;
        public static ConfigEntry<Color> _veryResistantColorConfig = null!;
        public static ConfigEntry<Color> _ignoreColorConfig = null!;
        public static ConfigEntry<Color> _immuneColorConfig = null!;
        public static ConfigEntry<Toggle> _KnowledgeLock = null!;

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