using System;
using System.Collections.Generic;
using System.IO;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using YamlDotNet.Serialization;
using Patches = Almanac.FileSystem.Patches;

namespace Almanac.Data;

public static class PlayerStats
{
    public static readonly string AlmanacStatsKey = "AlmanacStats";
    private static Dictionary<PlayerStatType, float> PlayerProfileStats = new();
    public static CustomData LocalPlayerData = new();

    public static float GetPlayerStat(PlayerStatType type)
    {
        if (!PlayerProfileStats.TryGetValue(type, out float value)) return 0;
        return value;
    }
    private static string GetCustomDataFilePath()
    {
        string PrefixPath = AlmanacPaths.PlayerDataFolderPath + Path.DirectorySeparatorChar;
        string FormattedPlayerName = Player.m_localPlayer.GetHoverName().Replace(" ", "_");
        return PrefixPath + FormattedPlayerName + AlmanacPaths.CustomDataFileName;
    }

    private static void InitPlayerTracker()
    {
        if (!Player.m_localPlayer) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Initializing Player Kill Death Tracker");

        if (!File.Exists(GetCustomDataFilePath()))
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Player Kill Death File not found");
            if (Player.m_localPlayer.m_customData.TryGetValue(AlmanacStatsKey, out string CurrentData))
            {
                ReadCustomTrackerData(CurrentData, false);
            }
            else
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Client: Generating kill death custom data");
                foreach (string key in CreatureDataCollector.TempDefeatKeys)
                {
                    LocalPlayerData.Player_Kill_Deaths[key] = new KillDeaths();
                }
            }
            WriteCurrentCustomData();
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Player Kill Death File found");
            if (Player.m_localPlayer.m_customData.TryGetValue(AlmanacStatsKey, out string CurrentData))
            {
                ReadCustomTrackerData(CurrentData, true);
            }
            else
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Client: Loaded player data file: " + GetCustomDataFilePath());
                AlmanacPlugin.AlmanacLogger.LogDebug("Client: Saving loaded player data to player custom data");
                string FileData = File.ReadAllText(GetCustomDataFilePath());
                Player.m_localPlayer.m_customData[AlmanacStatsKey] = FileData;
                ReadCustomTrackerData(FileData, true, false);
            }
        }
        TerminalCommands.AddAlmanacCommands();
        // Bounty.AddBountyCommands();
    }

    private static void ReadCustomTrackerData(string CurrentData, bool hasFile, bool hasCustomData = true)
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        if (hasCustomData) AlmanacPlugin.AlmanacLogger.LogDebug("Client: Player has custom data kill death values" 
                                             + (hasFile ? ", ignoring file" : ""));
        CustomData data = deserializer.Deserialize<CustomData>(CurrentData);
        LocalPlayerData = data;
        
        if (data.Player_Kill_Deaths.Count == CreatureDataCollector.TempDefeatKeys.Count) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Creature count changed, adding missing values");
        foreach (string key in CreatureDataCollector.TempDefeatKeys)
        {
            if (data.Player_Kill_Deaths.ContainsKey(key)) continue;
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Player data missing: " + key + " , adding value");
            data.Player_Kill_Deaths[key] = new KillDeaths();
        }
    }
    private static void WriteCurrentCustomData()
    {
        if (!Player.m_localPlayer) return;
        try
        {
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(LocalPlayerData);
            Player.m_localPlayer.m_customData[AlmanacEffectManager.AchievementKey] = serializer.Serialize(AlmanacEffectManager.SavedAchievementEffectNames);
            Player.m_localPlayer.m_customData[AlmanacStatsKey] = data;
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Saving almanac custom data");
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Failed to save player data");
        }
    }

    private static void UpdatePlayerDeaths(Player instance)
    {
        if (!instance) return;
        if (instance != Player.m_localPlayer) return;

        HitData? lastHit = instance.m_lastHit;
        if (lastHit == null) return;
        Character? killer = lastHit.GetAttacker();
        if (!killer) return;

        string key = killer.m_defeatSetGlobalKey;
        if (!LocalPlayerData.Player_Kill_Deaths.ContainsKey(key)) return;
        
        ++LocalPlayerData.Player_Kill_Deaths[key].deaths;
    }

    private static void UpdatePlayerKills(Character instance)
    {
        if (!instance) return;
        if (!Player.m_localPlayer) return;

        HitData? lastHit = instance.m_lastHit;
        if (lastHit == null) return;
        Character? killer = instance.m_lastHit.GetAttacker();
        if (!killer) return;
        if (killer.GetOwner() != Player.m_localPlayer.GetOwner()) return;
            
        string key = instance.m_defeatSetGlobalKey;
        if (!LocalPlayerData.Player_Kill_Deaths.ContainsKey(key)) return;
        
        ++LocalPlayerData.Player_Kill_Deaths[key].kills;
    }
    
    public static void UpdatePlayerStats()
    {
        if (!Game.instance) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Loading latest player stats");
        PlayerProfileStats = Game.instance.GetPlayerProfile().m_playerStats.m_stats;
    }

    public static int GetKnownTextCount() => !Player.m_localPlayer ? 0 : Player.m_localPlayer.m_knownTexts.Count;

    public static int GetKnownRecipeCount() => !Player.m_localPlayer ? 0 : Player.m_localPlayer.m_knownRecipes.Count;

    private static void AddRuneStoneLabels(RuneStone instance)
    {
        if (!instance) return;
        foreach (RuneStone.RandomRuneText text in instance.m_randomTexts)
        {
            if (!text.m_label.IsNullOrWhiteSpace()) continue;
            text.m_label = text.m_text + "_label";
        }
    }
    
    public static ServerPlayerData GetServerPlayerData()
    {
        if (!Player.m_localPlayer) return new ServerPlayerData();
        return new ServerPlayerData()
        {
            player_name = Player.m_localPlayer.GetHoverName(),
            data = new PlayerData()
            {
                completed_achievements = AchievementManager.AchievementList.FindAll(item => item.m_isCompleted).Count,
                total_kills = (int)GetPlayerStat(PlayerStatType.EnemyKills),
                total_deaths = (int)GetPlayerStat(PlayerStatType.Deaths)
            }
        };
    }
    
    [HarmonyPatch(typeof(RuneStone), nameof(RuneStone.Interact))]
    static class RuneStoneInteractPatch
    {
        private static void Prefix(RuneStone __instance)
        {
            if (!__instance) return;
            AddRuneStoneLabels(__instance);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class PlayerOnDeathPatch
    {
        private static void Prefix(Player __instance)
        {
            if (!__instance) return;
            UpdatePlayerDeaths(__instance);
            ISerializer serializer = new SerializerBuilder().Build();
            Player.m_localPlayer.m_customData[AlmanacEffectManager.AchievementKey] = serializer.Serialize(AlmanacEffectManager.SavedAchievementEffectNames);
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    private static class CharacterOnDeathPatch
    {
        private static void Prefix(Character __instance)
        {
            if (!__instance) return;
            UpdatePlayerKills(__instance);
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    private static class PlayerSavePatch
    {
        private static void Prefix(Player __instance)
        {
            if (!__instance) return;
            WriteCurrentCustomData();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Start))]
    private static class PlayerLoadCustomData
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            LoadPlayerData();
            Patches.CheckIfServer();
            switch (AlmanacPlugin.WorkingAsType)
            {
                case AlmanacPlugin.WorkingAs.Client:
                    Leaderboard.ClientLeaderboardCoroutine();
                    break;
                case AlmanacPlugin.WorkingAs.Both:
                    UpdatePlayerStats();
                    Leaderboard.BothLeaderboardCoroutine();
                    break;
            }
        }
    }

    // [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
    // private static class PlayerEatFoodFix
    // {
    //     private static void Postfix(Player __instance)
    //     {
    //         
    //     }
    // }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class PickableInteractPatch
    {
        private static void Postfix(Pickable __instance, Humanoid character, ref bool __result)
        {
            if (!__instance || !character || !__result) return;
            
            Player? player = character as Player;
            if (player == null) return;

            string itemName = __instance.name.Replace("(Clone)", string.Empty).Replace("Pickable_",string.Empty);
            
            if (LocalPlayerData.Player_Pickable_Data.ContainsKey(itemName))
            {
                ++LocalPlayerData.Player_Pickable_Data[itemName];
            }
            else
            {
                LocalPlayerData.Player_Pickable_Data[itemName] = 1;
            }
        }
    }

    public static bool GetPlayerPickableValue(string key, out int value)
    {
        value = 0;
        if (!LocalPlayerData.Player_Pickable_Data.TryGetValue(key, out int pickableValue)) return false;
        value = pickableValue;
        return true;
    }

    private static void LoadPlayerData()
    {
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.GetHoverName().Replace(" ", "_").IsNullOrWhiteSpace()) return;
        
        InitPlayerTracker();
    }
}

[Serializable]
public class CustomData
{
    public Dictionary<string, KillDeaths> Player_Kill_Deaths = new();
    public Dictionary<string, int> Player_Pickable_Data = new();
}

[Serializable]
public class KillDeaths
{
    public int kills;
    public int deaths;
}

[Serializable]
public class ServerPlayerData
{
    public string player_name = null!;
    public PlayerData data = null!;
}
[Serializable]
public class PlayerData
{
    public int completed_achievements;
    public int total_kills;
    public int total_deaths;
}