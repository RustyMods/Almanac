using System.Collections.Generic;
using HarmonyLib;

namespace Almanac.Almanac;

public static class Leaderboard
{
    public class LeaderboardData
    {
        public string playerName = null!;
        public int completedAchievements;
        public int deaths;
        public int kills;
        public int ranking;
    }

    public static List<LeaderboardData> tempLeaderboardData = new();

    // [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    // static class ZNetPatch
    // {
    //     private static void Postfix(ZNet __instance, ZNetPeer peer)
    //     {
    //         foreach (var connection in __instance.m_peers)
    //         {
    //             AlmanacPlugin.AlmanacLogger.LogWarning(connection.m_playerName);
    //         }
    //     }
    // }
}