using System.Collections.Generic;
using UnityEngine;

namespace Almanac.Almanac;

public static class TrackPlayerStats
{
    private static Dictionary<PlayerStatType, float> PlayerStats = new();
    public static void UpdatePlayerStats()
    {
        PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
        PlayerStats = playerProfile.m_playerStats.m_stats;
    }
    public static float GetPlayerStat(PlayerStatType type)
    {
        PlayerStats.TryGetValue(type, out float value);
        // Debug.LogWarning($"{type.ToString()} : {value}");
        return value;
    }
}