using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using static Almanac.AlmanacPlugin;

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
        return value;
    }

    public static int GetKnownTextCount() => Player.m_localPlayer.m_knownTexts.Count;

    [HarmonyPatch(typeof(RuneStone), nameof(RuneStone.Interact))]
    static class RuneStoneInteractPatch
    {
        private static void Prefix(RuneStone __instance)
        {
            if (!__instance) return;
            foreach (var text in __instance.m_randomTexts)
            {
                if (!text.m_label.IsNullOrWhiteSpace()) continue;
                text.m_label = text.m_text + "_label";
            }
        }
    }
}