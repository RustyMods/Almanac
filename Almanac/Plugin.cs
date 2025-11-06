using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.ExternalAPIs;
using Almanac.Lottery;
using Almanac.Managers;
using Almanac.Marketplace;
using Almanac.NPC;
using Almanac.Quests;
using Almanac.Store;
using Almanac.TreasureHunt;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
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
        internal const string ModVersion = "3.6.1";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        public const string ConfigFileName = ModGUID + ".cfg";
        public static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AlmanacLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static readonly AlmanacDir AlmanacDir = new(Paths.ConfigPath, ModName);
        public static AlmanacPlugin instance = null!;
        public static event Action? OnZNetAwake;
        public static event Action? OnZNetSceneAwake;
        public static event Action? OnZNetSave;
        public static event Action<Player>? OnPlayerProfileLoadPlayerDataPostfix;
        public static event Action? OnPlayerProfileSavePlayerDataPostfix;
        public static event Action<Player>? OnPlayerProfileSavePlayerDataPrefix;
        public static event Action? OnObjectDBAwake;
        public static event Action<GameObject>? OnZNetScenePrefabs;
        public static event Action<GameObject>? OnObjectDBPrefabs;
        public static event Action? OnNewCharacterDone;
        public void Awake()
        {
            instance = this;
            Clone._root = new GameObject("Almanac.PrefabManager.Clones");
            DontDestroyOnLoad(Clone._root);
            Clone._root.SetActive(false);
            Keys.Write();
            Localizer.Load();
            Configs.Load();
            
            SpriteManager.RegisterCustomIcons();
            StoreManager.Setup();
            BountyManager.Setup();
            TreasureManager.Setup();
            AchievementManager.Setup();
            CreatureGroup.Setup();
            CustomEffectManager.Setup();
            Filters.Setup();
            LotteryManager.Setup();
            MarketManager.Setup();
            QuestManager.Setup();
            
            Leaderboard.Setup();
            CritterHelper.Setup();
            ItemHelper.Setup();
            PieceHelper.Setup();
            PlayerInfo.Setup();
            DialogueManager.Setup();
            RandomTalkManager.Setup();
            SetupCommands();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }
        private void OnDestroy() => Config.Save();

        private void Update()
        {
            if (QuestPanel.instance is not { } panel) return;
            if (Time.time - panel.lastInputTime < QuestPanel.Input_Cooldown) return;
            if (!ZInput.GetKeyDown(Configs.QuestKey)) return;
            panel.lastInputTime = Time.time;
            panel.Toggle();
        }

        private void SetupDiscordCommands()
        {
            if (!DiscordBot_API.IsLoaded()) return;
            DiscordBot_API.RegisterCommand("!almanactokens", "give player tokens, `player name` `amount`", args => 
            {
                if (args.Length < 2) return;
                string playerName = args[1].Trim();
                if (!int.TryParse(args[2].Trim(), out int amount)) return;
                if (Player.m_localPlayer && Player.m_localPlayer.GetPlayerName() == playerName)
                {
                    Player.m_localPlayer.AddTokens(amount);
                }
                else if (ZNet.instance.GetPeerByPlayerName(playerName) is { } peer)
                {
                    var pkg = new ZPackage();
                    pkg.Write("!almanactokens");
                    pkg.Write(amount);
                    peer.m_rpc.Invoke("RPC_BotToClient", pkg);
                }
            }, pkg =>
            {
                int amount = pkg.ReadInt();
                Player.m_localPlayer.AddTokens(amount);
            }, 
            adminOnly:true, 
            isSecret:false,
            emoji:"fries");

        }
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
                Player.m_localPlayer.ClearSavedQuests();
                Player.m_localPlayer.ClearSavedDialogues();
                return true;
            });

            CommandData reset_quests = new CommandData("reset_quests", "clears all quests from player file", _ =>
            {
                if (!Player.m_localPlayer) return false;
                Player.m_localPlayer.ClearSavedQuests();
                return true;
            });

            CommandData reset_dialogues = new CommandData("reset_dialogues", "clears all dialogues from player file",
                _ =>
                {
                    if (!Player.m_localPlayer) return false;
                    Player.m_localPlayer.ClearSavedDialogues();
                    return true;
                });

            CommandData size = new CommandData("size", "prints the kilobyte size of almanac data on player file",
                _ =>
                {
                    if (!Player.m_localPlayer) return false;
                    int total = Player.m_localPlayer.GetTokensByteCount()
                                + Player.m_localPlayer.GetRecordByteCount()
                                + Player.m_localPlayer.GetQuestByteCount()
                                + Player.m_localPlayer.GetDialogueByteCount();
                    double kilobytes = total / 1024.0;
                    Logger.LogInfo($"Almanac Data Size: {kilobytes} kilobytes");
                    return true;
                });

            CommandData fullReset = new CommandData("full_reset", "clears all almanac data as well as valheim player data",
                _ =>
                {
                    if (!Player.m_localPlayer) return false;
                    Player.m_localPlayer.ClearRecords();
                    Player.m_localPlayer.ClearTokens();
                    Player.m_localPlayer.ClearSavedQuests();
                    Player.m_localPlayer.ClearSavedDialogues();
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
            private static void Postfix(Player player) => OnPlayerProfileLoadPlayerDataPostfix?.Invoke(player);
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
                // ModHelper.MapAssets();
                foreach (GameObject? prefab in __instance.m_prefabs)
                {
                    if (prefab == null) continue;
                    OnZNetScenePrefabs?.Invoke(prefab);
                }
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
                    if (prefab == null) continue;
                    OnObjectDBPrefabs?.Invoke(prefab);
                }
                foreach (StatusEffect se in ObjectDB.instance.m_StatusEffects)
                {
                    SpriteManager.OnObjectDBStatusEffects(se);
                }
                OnObjectDBAwake?.Invoke();
            }
        }
        
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnNewCharacterDone))]
        private static class FejdStartup_OnNewCharacterDone_Patch
        {
            [UsedImplicitly]
            private static void Prefix() => OnNewCharacterDone?.Invoke();
        }
    }
}