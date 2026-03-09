using System;
using System.Collections.Generic;
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
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using UnityEngine;

namespace Almanac;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInIncompatibility("randyknapp.mods.auga")]
[BepInDependency("RustyMods.DiscordBot", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("RustyMods.RustyBags", BepInDependency.DependencyFlags.SoftDependency)]
public class AlmanacPlugin : BaseUnityPlugin
{
    internal const string ModName = "Almanac";
    internal const string ModVersion = "3.7.8";
    internal const string Author = "RustyMods";
    private const string ModGUID = Author + "." + ModName;
    public const string ConfigFileName = ModGUID + ".cfg";
    public static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
    internal static string ConnectionError = "";

    public readonly Harmony _harmony = new(ModGUID);

    public static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

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
        Configs.Load();
        Clone._root = new GameObject("Almanac.PrefabManager.Clones");
        DontDestroyOnLoad(Clone._root);
        Clone._root.SetActive(false);
        Keys.Write();
        Localizer.Load();

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
        SetupDiscordCommands();
        var assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
    }

    private void OnDestroy()
    {
        Config.Save();
    }

    private void Update()
    {
        if (QuestPanel.instance is not { } panel) return;
        if (Time.time - panel.lastInputTime < QuestPanel.Input_Cooldown) return;
        if (!ZInput.GetKeyDown(Configs.QuestKey)) return;
        panel.lastInputTime = Time.time;
        panel.Toggle();
    }

    public static void LogInfo(string msg) => instance.Logger.LogInfo(msg);
    public static void LogWarning(string msg) => instance.Logger.LogWarning(msg);
    public static void LogError(string msg) => instance.Logger.LogError(msg);
    public static void LogDebug(string msg) => instance.Logger.LogDebug(msg);

    private static void SetupDiscordCommands()
    {
        if (!DiscordBot_API.IsLoaded()) return;
        DiscordBot_API.RegisterCommand("!almanactokens", "give player tokens, `player name` `amount`", args =>
            {
                if (args.Length < 2) return;
                var playerName = args[1].Trim();
                if (!int.TryParse(args[2].Trim(), out var amount)) return;
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
                var amount = pkg.ReadInt();
                Player.m_localPlayer.AddTokens(amount);
            },
            true,
            false,
            "fries");

        DiscordBot_API.RegisterCommand("!almanacleaderboard", "show almanac leaderboard", _ =>
        {
            var leaderboard = Leaderboard.GetLeaderboard();
            if (leaderboard.Count == 0)
            {
                DiscordBot_API.SendWebhookMessage(DiscordBot_API.Channel.Commands, "Leaderboard is empty");
                return;
            }

            var table = new Dictionary<string, string>();
            foreach (var info in leaderboard)
                table[info.PlayerName] = "```" + string.Join("\n",
                    new List<string>
                    {
                        $"Achievements: {info.CollectedAchievements}",
                        $"Kills: {info.Kills}",
                        $"Deaths: {info.Deaths}",
                        $"Rank: {info.GetRank()}"
                    }) + "```";
            DiscordBot_API.SendWebhookTable(DiscordBot_API.Channel.Commands, "Leaderboard", table);
        }, emoji: "brokenheart");

        DiscordBot_API.RegisterCommand("!almanacstore", "displays player items on sale", _ =>
        {
            var list = MarketManager.GetMarketItems();
            if (list.Count <= 0)
            {
                DiscordBot_API.SendWebhookMessage(DiscordBot_API.Channel.Commands, "No player items on marketplace");
            }
            else
            {
                Dictionary<string, string> details = new();
                foreach (var item in list)
                    details[item.itemData.m_shared.m_name] =
                        $"```Cost: {item.GetCostPerUnit()} {Keys.AlmanacToken}\nPosted by: {item.PostedBy}\nDate posted: {item.DatePosted:yyyy-M-d dddd}```";
                DiscordBot_API.SendWebhookTable(DiscordBot_API.Channel.Commands, "Marketplace", details);
            }
        });

        DiscordBot_API.RegisterCommand("!leaderboardremove", "removes player entry from leaderboard, `player name`",
            args =>
            {
                if (args.Length < 2) return;
                var playerName = args[1].Trim();
                if (!Leaderboard.players.Remove(playerName))
                    DiscordBot_API.SendWebhookMessage(DiscordBot_API.Channel.Commands,
                        "Failed to find leaderboard player");
            }, adminOnly: true);
    }

    private void SetupCommands()
    {
        Terminal.ConsoleCommand main = new(CommandData.m_startCommand, "Use help to find commands", args =>
        {
            if (args.Length < 2) return false;
            if (!CommandData.m_commands.TryGetValue(args[1], out var data)) return false;
            return data.Run(args);
        }, optionsFetcher: CommandData.m_commands.Where(x => !x.Value.IsSecret()).Select(x => x.Key).ToList);

        var reset = new CommandData("reset", "clears all almanac data off player file", _ =>
        {
            if (!Player.m_localPlayer) return false;
            Player.m_localPlayer.ClearRecords();
            Player.m_localPlayer.ClearTokens();
            Player.m_localPlayer.ClearSavedQuests();
            Player.m_localPlayer.ClearSavedDialogues();
            Player.m_localPlayer.ClearNotices();
            return true;
        });

        var reset_quests = new CommandData("reset_quests", "clears all quests from player file", _ =>
        {
            if (!Player.m_localPlayer) return false;
            Player.m_localPlayer.ClearSavedQuests();
            return true;
        });

        var reset_dialogues = new CommandData("reset_dialogues", "clears all dialogues from player file",
            _ =>
            {
                if (!Player.m_localPlayer) return false;
                Player.m_localPlayer.ClearSavedDialogues();
                return true;
            });

        var size = new CommandData("size", "prints the kilobyte size of almanac data on player file",
            _ =>
            {
                if (!Player.m_localPlayer) return false;
                var total = Player.m_localPlayer.GetTokensByteCount()
                            + Player.m_localPlayer.GetRecordByteCount()
                            + Player.m_localPlayer.GetQuestByteCount()
                            + Player.m_localPlayer.GetDialogueByteCount();
                var kilobytes = total / 1024.0;
                Logger.LogInfo($"Almanac Data Size: {kilobytes} kilobytes");
                return true;
            });

        var fullReset = new CommandData("full_reset", "clears all almanac data as well as valheim player data",
            _ =>
            {
                if (!Player.m_localPlayer) return false;
                Player.m_localPlayer.ClearRecords();
                Player.m_localPlayer.ClearTokens();
                Player.m_localPlayer.ClearSavedQuests();
                Player.m_localPlayer.ClearSavedDialogues();
                Player.m_localPlayer.ClearNotices();
                Player.m_localPlayer.m_knownBiome.Clear();
                Player.m_localPlayer.m_knownMaterial.Clear();
                Player.m_localPlayer.m_knownRecipes.Clear();
                Player.m_localPlayer.m_knownStations.Clear();
                Player.m_localPlayer.m_knownTexts.Clear();
                Player.m_localPlayer.m_trophies.Clear();
                return true;
            });

        var give = new CommandData("tokens", "give local player almanac tokens", args =>
        {
            if (!Player.m_localPlayer) return false;
            if (args.Length < 3) return false;
            if (!int.TryParse(args[2], out var amount)) return false;
            Player.m_localPlayer.AddTokens(amount);
            return true;
        }, adminOnly: true);

        var setFullhouse = new CommandData("fullhouse", "[amount] Set current full house lottery reward",
            args =>
            {
                if (args.Length < 2) return false;
                if (!int.TryParse(args[1], out var amount)) return false;
                LotteryManager.LotteryTotal = amount;
                return true;
            }, adminOnly: true);
    }

    [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
    private static class PlayerProfile_SavePlayerData_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Player player)
        {
            OnPlayerProfileSavePlayerDataPrefix?.Invoke(player);
        }

        [UsedImplicitly]
        private static void Postfix()
        {
            OnPlayerProfileSavePlayerDataPostfix?.Invoke();
        }
    }

    [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.LoadPlayerData))]
    private static class PlayerProfile_LoadPlayerData_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player player)
        {
            OnPlayerProfileLoadPlayerDataPostfix?.Invoke(player);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))]
    private static class ZNet_Save_Patch
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            OnZNetSave?.Invoke();
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            OnZNetAwake?.Invoke();
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    public class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ZNetScene __instance)
        {
            foreach (var prefab in __instance.m_prefabs)
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
            foreach (var prefab in ObjectDB.instance.m_items)
            {
                if (prefab == null) continue;
                OnObjectDBPrefabs?.Invoke(prefab);
            }

            foreach (var se in ObjectDB.instance.m_StatusEffects) SpriteManager.OnObjectDBStatusEffects(se);
            OnObjectDBAwake?.Invoke();
        }
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnNewCharacterDone))]
    private static class FejdStartup_OnNewCharacterDone_Patch
    {
        [UsedImplicitly]
        private static void Prefix()
        {
            OnNewCharacterDone?.Invoke();
        }
    }
}