using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.Lottery;
using Almanac.Managers;
using Almanac.Marketplace;
using Almanac.Store;
using Almanac.TreasureHunt;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace Almanac
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInIncompatibility("randyknapp.mods.auga")]
    public class AlmanacPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Almanac";
        internal const string ModVersion = "3.5.13";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        public const string ConfigFileName = ModGUID + ".cfg";
        public static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AlmanacLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static readonly AlmanacDir AlmanacDir = new(Paths.ConfigPath, ModName);
        public static readonly AlmanacDir AchievementDir = new(AlmanacDir.Path, "Achievements");
        public static readonly AlmanacDir FilterDir = new (AlmanacDir.Path, "Filters");
        public static readonly AlmanacDir CreatureDir = new (AlmanacDir.Path, "CreatureGroups");
        public static readonly AlmanacDir BountyDir = new (AlmanacDir.Path, "Bounties");
        public static readonly AlmanacDir TreasureDir = new (AlmanacDir.Path, "Treasures");
        public static readonly AlmanacDir StoreDir = new (AlmanacDir.Path, "Store");
        public static readonly AlmanacDir CustomEffectDir = new (AlmanacDir.Path, "CustomEffects");
        public static readonly AlmanacDir LeaderboardDir = new (AlmanacDir.Path, "Leaderboards");
        public static readonly AlmanacDir LotteryDir = new (AlmanacDir.Path, "Lotteries");
        public static readonly AlmanacDir MarketplaceDir = new (AlmanacDir.Path, "Marketplace");
        public static readonly AlmanacDir IconsDir = new (AlmanacDir.Path, "Icons");
        public static AlmanacPlugin instance = null!;
        public static event Action? OnZNetAwake;
        public static event Action? OnZNetSceneAwake;
        public static event Action? OnZNetSave;
        public static event Action<Player>? OnPlayerProfileLoadPlayerData;
        public static event Action? OnPlayerProfileSavePlayerDataPostfix;
        public static event Action<Player>? OnPlayerProfileSavePlayerDataPrefix;
        public static event Action? OnObjectDBAwake;
        public static event Action<GameObject>? OnZNetScenePrefabs;
        public static event Action<GameObject>? OnObjectDBPrefabs;
        public void Awake()
        {
            instance = this;
            
            // AlmanacPaths.CreateFolderDirectories();
            Keys.Write();
            Localizer.Load();
            Configs.Load();
            
            StoreManager.Setup();
            BountyManager.Setup();
            TreasureManager.Setup();
            AchievementManager.Setup();
            CreatureGroup.Setup();
            CustomEffectManager.Setup();
            Filters.Setup();
            LotteryManager.Setup();
            MarketManager.Setup();
            
            Leaderboard.Setup();
            CritterHelper.Setup();
            ItemHelper.Setup();
            PieceHelper.Setup();
            PlayerInfo.Setup();
            SpriteManager.RegisterCustomIcons();
            SetupCommands();
            
            // Use to rebuild readmes 
            
            // AchievementReadMeBuilder.Write();
            // StoreReadMeBuilder.Write();
            // TreasureReadMeBuilder.Write();
            // BountyReadMeBuilder.Write();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }
        private void OnDestroy() => Config.Save();
        private void SetupCommands()
        {
            Terminal.ConsoleCommand main = new(CommandData.m_startCommand, "Use help to find commands", args =>
            {
                if (args.Length < 2) return false;
                if (!CommandData.m_commands.TryGetValue(args[1], out CommandData data)) return false;
                return data.Run(args);
            }, optionsFetcher: CommandData.m_commands.Where(x => !x.Value.IsSecret()).Select(x => x.Key).ToList);

            CommandData reset = new CommandData("reset", "clears all almanac data off player file", _ =>
            {
                if (!Player.m_localPlayer) return false;
                Player.m_localPlayer.ClearRecords();
                Player.m_localPlayer.ClearTokens();
                return true;
            });

            CommandData size = new CommandData("size", "prints the kilobyte size of almanac data on player file",
                _ =>
                {
                    if (!Player.m_localPlayer) return false;
                    int total = Player.m_localPlayer.GetTokensByteCount() + Player.m_localPlayer.GetRecordByteCount();
                    double kilobytes = total / 1024.0;
                    Logger.LogInfo($"Almanac Data Size: {kilobytes} kilobytes");
                    return true;
                });

            CommandData fullReset = new CommandData("full_reset", "clears all almanac data as well as valheim data",
                _ =>
                {
                    if (!Player.m_localPlayer) return false;
                    Player.m_localPlayer.ClearRecords();
                    Player.m_localPlayer.ClearTokens();
                    Player.m_localPlayer.m_knownBiome.Clear();
                    Player.m_localPlayer.m_knownMaterial.Clear();
                    Player.m_localPlayer.m_knownRecipes.Clear();
                    Player.m_localPlayer.m_knownStations.Clear();
                    Player.m_localPlayer.m_knownTexts.Clear();
                    Player.m_localPlayer.m_trophies.Clear();
                    return true;
                });

            CommandData give = new CommandData("tokens", "give local player almanac tokens", args =>
            {
                if (!Player.m_localPlayer) return false;
                if (args.Length < 3) return false;
                if (!int.TryParse(args[2], out int amount)) return false;
                Player.m_localPlayer.AddTokens(amount);
                return true;
            }, adminOnly: true);
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        private static class PlayerProfile_SavePlayerData_Patch
        {
            [UsedImplicitly]
            private static void Prefix(Player player) => OnPlayerProfileSavePlayerDataPrefix?.Invoke(player);
            
            [UsedImplicitly]
            private static void Postfix() => OnPlayerProfileSavePlayerDataPostfix?.Invoke();
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.LoadPlayerData))]
        private static class PlayerProfile_LoadPlayerData_Patch
        {
            [UsedImplicitly]
            private static void Postfix(Player player) => OnPlayerProfileLoadPlayerData?.Invoke(player);
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))]
        private static class ZNet_Save_Patch
        {
            [UsedImplicitly]
            private static void Postfix() => OnZNetSave?.Invoke();
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
        private static class ZNet_Awake_Patch
        {
            [UsedImplicitly]
            private static void Postfix() => OnZNetAwake?.Invoke();
        }
        
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        public class ZNetScene_Awake_Patch
        {
            [UsedImplicitly]
            public static void Postfix(ZNetScene __instance)
            {
                foreach (GameObject? prefab in __instance.m_prefabs) OnZNetScenePrefabs?.Invoke(prefab);
                OnZNetSceneAwake?.Invoke();
            }
        }

        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        private static class ObjectDB_Awake_Patch
        {
            [UsedImplicitly]
            private static void Postfix()
            {
                if (ZNetScene.instance == null) return;
                foreach (GameObject? prefab in ObjectDB.instance.m_items)
                {
                    OnObjectDBPrefabs?.Invoke(prefab);
                }
                foreach (StatusEffect se in ObjectDB.instance.m_StatusEffects)
                {
                    SpriteManager.OnObjectDBStatusEffects(se);
                }
                OnObjectDBAwake?.Invoke();
            }
        }
    }
}