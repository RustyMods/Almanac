using System;
using System.Collections.Generic;
using HarmonyLib;
using ServerSync;
using YamlDotNet.Serialization;

namespace Almanac.Almanac;

public static class Leaderboard
{
    [Serializable]
    public class LeaderboardData
    {
        public string playerName = null!;
        public int completedAchievements = 0;
        public int deaths = 0;
        public int kills = 0;
        public int ranking = 0;
    }
    
    public static readonly CustomSyncedValue<List<string>> SyncedLeaderboard =
        new(AlmanacPlugin.ConfigSync, "LeaderboardData", new());

    private static DateTime lastSent;
    
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPlayerList))]
    static class SendPlayerListPatch
    {
        private static void Postfix(ZNet __instance)
        {
            if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Server) return;
            if (lastSent + TimeSpan.FromMinutes(AlmanacPlugin._LeaderboardRefreshRate.Value) > DateTime.Now) return;
            // Update leaderboard
            SyncedLeaderboard.Value.Clear();
            lastSent = DateTime.Now;
            List<string> updatedData = new();
            ISerializer serializer = new SerializerBuilder().Build();
            foreach (ZNet.PlayerInfo player in __instance.m_players)
            {
                // Need to find a way to get data from players to populate here
                
                LeaderboardData data = new LeaderboardData()
                {
                    playerName = player.m_name,
                };
                string serializedData = serializer.Serialize(data);
                updatedData.Add(serializedData);
            }
            SyncedLeaderboard.Value = updatedData;
        }
    }
}