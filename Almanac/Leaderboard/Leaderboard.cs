using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.UI;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac;

public static class Leaderboard
{
    private static string? LeaderboardFileName;
    private static readonly CustomSyncedValue<string> SyncedLeaderboard = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Leaderboard", "");
    public static readonly Dictionary<string, LeaderboardInfo> players = new();
    public static readonly AlmanacDir LeaderboardDir = new (AlmanacPlugin.AlmanacDir.Path, "Leaderboards");
    private static readonly ISerializer serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();
    public static void Setup()
    {
        AlmanacPlugin.OnZNetAwake += Initialize;
        SyncedLeaderboard.ValueChanged += OnSyncedLeaderboardChange;
        AlmanacPlugin.OnZNetSave += Save;
        AlmanacPlugin.OnPlayerProfileLoadPlayerDataPostfix += _ => SendLocalPlayerInfo();
        AlmanacPlugin.OnPlayerProfileSavePlayerDataPostfix += SendLocalPlayerInfo;
    }
    private static void Initialize()
    {
        LeaderboardFileName = ZNet.instance.GetWorldName() + ".Leaderboard.dat";

        ZRoutedRpc.instance.Register<ZPackage>(nameof(RPC_Leaderboard), RPC_Leaderboard);
        Read();
        UpdateServerLeaderboard();
    }
    private static void UpdateServerLeaderboard()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string data = serializer.Serialize(players);
        SyncedLeaderboard.Value = data;
    }
    private static void OnSyncedLeaderboardChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedLeaderboard.Value)) return;
        try
        {
            Dictionary<string, LeaderboardInfo> data = deserializer.Deserialize<Dictionary<string, LeaderboardInfo>>(SyncedLeaderboard.Value);
            players.Clear();
            players.AddRange(data);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server leaderboard");
        }
    }
    public static List<LeaderboardInfo> GetLeaderboard()
    {
        return players.Values.ToList().OrderBy(player => player.GetRank()).ToList();
    }
    private static void SendLocalPlayerInfo()
    {
        if (!ZNet.instance || !Player.m_localPlayer || !Player.m_localPlayer.m_nview.IsValid()) return;
        LeaderboardInfo info = new LeaderboardInfo(Player.m_localPlayer.GetPlayerName(),
            Player.m_localPlayer.GetCollectedAchievements().Count,
            PlayerInfo.GetPlayerStat(PlayerStatType.EnemyKills).Floor(),
            PlayerInfo.GetPlayerStat(PlayerStatType.Deaths).Floor());
        if (ZNet.instance.IsServer())
        {
            players[info.PlayerName] = info;
            UpdateServerLeaderboard();
        }
        else
        {
            ZPackage pkg = new ZPackage();
            pkg.Write(info.PlayerName);
            pkg.Write(info.CollectedAchievements);
            pkg.Write(info.Kills);
            pkg.Write(info.Deaths);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC_Leaderboard), pkg);
        }
    }
    public static void RPC_Leaderboard(long sender, ZPackage pkg)
    {
        string playerName = pkg.ReadString();
        int collectedAchievement = pkg.ReadInt();
        int kills = pkg.ReadInt();
        int deaths = pkg.ReadInt();
        players[playerName] = new (playerName, collectedAchievement, kills, deaths);
        UpdateServerLeaderboard();
    }
    private static void Save()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer() || LeaderboardFileName == null) return;
        string data = serializer.Serialize(players);
        byte[] compressedData = CompressAndEncode(data);
        LeaderboardDir.WriteAllBytes(LeaderboardFileName, compressedData);
    }
    private static void Read()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()  || LeaderboardFileName == null) return;
        if (!LeaderboardDir.FileExists(LeaderboardFileName)) return;
    
        try
        {
            byte[] compressedData = LeaderboardDir.ReadAllBytes(LeaderboardFileName);
            string data = DecompressAndDecode(compressedData);
        
            Dictionary<string, LeaderboardInfo> deserializedData = deserializer.Deserialize<Dictionary<string, LeaderboardInfo>>(data);
            players.Clear();
            players.AddRange(deserializedData);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server leaderboard: " + Path.GetFileName(LeaderboardFileName));
        }
    }
    private static byte[] CompressAndEncode(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);

        using var output = new MemoryStream();
        using var gzip = new GZipStream(output, CompressionMode.Compress);
        gzip.Write(data, 0, data.Length);
        gzip.Close();
        return output.ToArray();
    }
    private static string DecompressAndDecode(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
    [Serializable]
    public class LeaderboardInfo
    {
        private static Entries.EntryBuilder builder = new();
        public string PlayerName = string.Empty;
        public int CollectedAchievements;
        public int Kills;
        public int Deaths;
        public DateTime LastUpdated;

        public LeaderboardInfo(string playerName, int collectedAchievements, int kills, int deaths)
        {
            PlayerName = playerName;
            CollectedAchievements = collectedAchievements;
            Kills = kills;
            Deaths = deaths;
            LastUpdated = DateTime.UtcNow;
        }
        
        public LeaderboardInfo(){}

        public int GetRank()
        {
            return CollectedAchievements + Mathf.Max(Kills / Math.Max(Deaths, 1), 0);
        }

        public List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            builder.Add(Keys.CollectedAchievement, CollectedAchievements);
            builder.Add(Keys.TotalKills, Kills);
            builder.Add(Keys.TotalDeaths, Deaths);
            builder.Add(Keys.Ratio, Deaths == 0 ? Kills : (float)Kills / Deaths);
            builder.Add(Keys.LastUpdated, LastUpdated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            return builder.ToList();
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element item)
        {
            panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(PlayerName);
            ToEntries().Build(panel.description.view);
            panel.description.view.Resize();
        }
    }
}