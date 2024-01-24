using System;
using System.Collections.Generic;
using System.IO;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using YamlDotNet.Serialization;

namespace Almanac.Data;

public static class PlayerStats
{
    private static Dictionary<PlayerStatType, float> PlayerProfileStats = new();

    public static float GetPlayerStat(PlayerStatType type)
    {
        if (!PlayerProfileStats.TryGetValue(type, out float value)) return 0;

        return value;
    }
    
    public static CustomData TempCustomData = new();

    private static string GetCustomDataFilePath()
    {
        string PrefixPath = AlmanacPaths.PlayerDataFolderPath + Path.DirectorySeparatorChar;
        string FormattedPlayerName = Player.m_localPlayer.GetHoverName().Replace(" ", "_");
        return PrefixPath + FormattedPlayerName + AlmanacPaths.CustomDataFileName;
    }

    private static void InitPlayerTracker(bool overwrite = false)
    {
        if (!Player.m_localPlayer) return;
        
        if (!File.Exists(GetCustomDataFilePath()) || overwrite)
        {
            switch (overwrite)
            {
                case true:
                    AlmanacPlugin.AlmanacLogger.LogDebug("Overwriting player data, generating new file");
                    break;
                case false:
                    AlmanacPlugin.AlmanacLogger.LogDebug("Did not find player data, generating file");
                    break;
            }
            foreach (string key in CreatureDataCollector.TempDefeatKeys)
            {
                TempCustomData.Player_Kill_Deaths[key] = new KillDeaths();
            }
            
            WriteCurrentCustomData();
        }
        else
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            CustomData data = deserializer.Deserialize<CustomData>(File.ReadAllText(GetCustomDataFilePath()));

            if (data.Player_Kill_Deaths.Count != CreatureDataCollector.TempDefeatKeys.Count)
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Creature count changed, adding missing values");
                foreach (string key in CreatureDataCollector.TempDefeatKeys)
                {
                    if (!data.Player_Kill_Deaths.TryGetValue(key, out KillDeaths value))
                    {
                        AlmanacPlugin.AlmanacLogger.LogDebug("player data missing: " + key + " , adding value");
                        data.Player_Kill_Deaths[key] = new KillDeaths();
                    }
                }
            }
            
            TempCustomData = data;
            
            if (TempCustomData.Player_Kill_Deaths.Count == 0 && !overwrite)
            {
                InitPlayerTracker(true);
                return;
            }
        }
        AlmanacPlugin.AlmanacLogger.LogDebug("Loaded player data file: " + GetCustomDataFilePath());
        TerminalCommands.AddAlmanacCommands();
    }
    private static void WriteCurrentCustomData()
    {
        if (!Player.m_localPlayer) return;
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(TempCustomData);
        File.WriteAllText(GetCustomDataFilePath(), data);
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

        ++TempCustomData.Player_Kill_Deaths[key].deaths;
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

        ++TempCustomData.Player_Kill_Deaths[key].kills;
    }
    
    public static void UpdatePlayerStats()
    {
        if (!Game.instance) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Loading initial player profile");
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
        return new ServerPlayerData()
        {
            player_name = Player.m_localPlayer.GetHoverName(),
            data = new PlayerData()
            {
                completed_achievements = Achievements.AchievementManager.AchievementList.FindAll(item => item.m_isCompleted).Count,
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
        private static void Postfix(Player __instance)
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
            LoadPlayerData();
        }
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