using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Achievements;
using Almanac.Data;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.FileSystem;

public static class ServerSyncedData
{
    public static bool InitiatedServerAchievements = false;
    
    #region Achievements
    private static readonly CustomSyncedValue<string> ServerAchievements =
        new(AlmanacPlugin.ConfigSync, "ServerAchievements", "");
    
    public static void InitServerAchievements()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Initializing server achievements");
            ServerAchievements.ValueChanged += OnServerAchievementChanged;
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: Initializing server achievements");
            UpdateServerAchievements();
        }
    }

    public static void UpdateServerAchievements()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(AchievementManager.AchievementData);
        if (data.IsNullOrWhiteSpace()) return;

        ServerAchievements.Value = data;
    }

    public static void OnServerAchievementChanged()
    {
        if (ServerAchievements.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received new server data, reloading achievements");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<AchievementYML.AchievementData> data = deserializer.Deserialize<List<AchievementYML.AchievementData>>(ServerAchievements.Value);
        
        AchievementManager.InitAchievements(AchievementManager.LoadAchievementData(data), false);
        InitiatedServerAchievements = true;
    }
    
    #endregion
    
    #region Ignore List
    
    private static readonly CustomSyncedValue<string> ServerIgnoreList =
        new(AlmanacPlugin.ConfigSync, "ServerIgnoreList", "");

    public static void InitServerIgnoreList()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Awaiting server ignore list");

            ServerIgnoreList.ValueChanged += OnServerIgnoreListChanged;
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: Initializing server ignore list");
            UpdateServerIgnoreList();
        }
    }

    public static void UpdateServerIgnoreList()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(Filters.FilterList);
        if (data.IsNullOrWhiteSpace()) return;

        ServerIgnoreList.Value = data;
    }

    private static void OnServerIgnoreListChanged()
    {
        if (ServerIgnoreList.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received new server ignore list, updating");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<string> data = deserializer.Deserialize<List<string>>(ServerIgnoreList.Value);

        Filters.FilterList = data;
        ItemDataCollector.ClearCachedItemDrops();
    }
    
    #endregion
    
    #region Player Data
    
    private static readonly CustomSyncedValue<string> ServerPlayerData =
        new(AlmanacPlugin.ConfigSync, "ServerPlayerData", "");

    private static readonly CustomSyncedValue<string> ServerPlayerDataListed =
        new(AlmanacPlugin.ConfigSync, "ListedServerPlayerData", "");

    public static Dictionary<string, PlayerData> ServerPlayerDataList = new();

    public static void InitServerPlayerData()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Starting coroutine to send player data to server");
            AlmanacPlugin._plugin.StartCoroutine(UpdateSendPlayerDataToServer());
            ServerPlayerDataListed.ValueChanged += OnListedPlayerDataChange;
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: Initialize receive Player Data");
            InitServerPlayerListData();
            ServerPlayerData.ValueChanged += OnReceivePlayerData;
            AlmanacPlugin._plugin.StartCoroutine(UpdateSendListedPlayerData());
        }
    }
    
    private static IEnumerator UpdateSendListedPlayerData()
    {
        while (ZNet.instance.m_peers.Count > 0)
        {
            SendListedPlayerDataToClients();
            yield return new WaitForSeconds(30f * 60f);
        }
    }

    public static void SendListedPlayerDataToClients()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Sending updated listed player data to clients");
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(ServerPlayerDataList);
        ServerPlayerDataListed.Value = data;
    }

    private static void OnListedPlayerDataChange()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: received new listed player data, updating");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, PlayerData> data = deserializer.Deserialize<Dictionary<string, PlayerData>>(ServerPlayerDataListed.Value);
        ServerPlayerDataList = data;
    }
    
    private static void SendPlayerDataToServer()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(PlayerStats.GetServerPlayerData());
        ServerPlayerData.Value = data;
    }

    private static IEnumerator UpdateSendPlayerDataToServer()
    {
        while (Player.m_localPlayer)
        {
            SendPlayerDataToServer();
            yield return new WaitForSeconds(30f * 60f);
        }
    }

    private static void InitServerPlayerListData()
    {
        if (!File.Exists(AlmanacPaths.ServerPlayerDataFilePath))
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: No server player file found, generating");
            SaveLatestServerPlayerData();
        }
        
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, PlayerData> ServerData = deserializer.Deserialize<Dictionary<string, PlayerData>>(File.ReadAllText(AlmanacPaths.ServerPlayerDataFilePath));
        ServerPlayerDataList = ServerData;
    }
    
    private static void OnReceivePlayerData()
    {
        if (!File.Exists(AlmanacPaths.ServerPlayerDataFilePath))
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: No server player file found, generating");
            SaveLatestServerPlayerData();
        }

        if (ServerPlayerData.Value.IsNullOrWhiteSpace()) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        ServerPlayerData receivedData = deserializer.Deserialize<ServerPlayerData>(ServerPlayerData.Value);
        AlmanacPlugin.AlmanacLogger.LogDebug($"Server: Received new data from {receivedData.player_name}, saving to disk");
        if (ServerPlayerDataList.TryGetValue(receivedData.player_name, out PlayerData localData))
        {
            if (localData.completed_achievements >= receivedData.data.completed_achievements)
            {
                // To make sure completed achievements is added rather than overwritten
                ServerPlayerDataList[receivedData.player_name].total_deaths = receivedData.data.total_deaths;
                ServerPlayerDataList[receivedData.player_name].total_kills = receivedData.data.total_kills;
            }
            else
            {
                ServerPlayerDataList[receivedData.player_name].completed_achievements += receivedData.data.completed_achievements;
                ServerPlayerDataList[receivedData.player_name].total_deaths = receivedData.data.total_deaths;
                ServerPlayerDataList[receivedData.player_name].total_kills = receivedData.data.total_kills;
            }
        }
        else
        {
            ServerPlayerDataList[receivedData.player_name] = receivedData.data;
        }
        SaveLatestServerPlayerData();
        SendListedPlayerDataToClients();
    }

    private static void SaveLatestServerPlayerData()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: saving latest server player data to file");
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(ServerPlayerDataList);
        File.WriteAllText(AlmanacPaths.ServerPlayerDataFilePath, data);
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    private static class ZNetPlayerDisconnectPatch
    {
        private static void Prefix(ZNet __instance)
        {
            if (!__instance) return;
            if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client) return;
            AlmanacPlugin.AlmanacLogger.LogDebug("Player logged out, saving latest server player data");
            SaveLatestServerPlayerData();
        }
    }

    #endregion
    
    #region Creature List

    private static readonly CustomSyncedValue<string> ServerCreatureList = new(AlmanacPlugin.ConfigSync, "CreatureList", "");
    
    public static void InitServerCreatureList()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Awaiting server creature list");

            ServerCreatureList.ValueChanged += OnServerCreatureListChanged;
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: Initializing server creature list");

            UpdateServerCreatureList();
        }
    }
    
    private static Dictionary<Heightmap.Biome, List<string>> FormatCreatureListData()
    {
        return CreatureLists.BiomeCreatureMap.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(creature => creature.name).ToList()
        );
    }

    public static void UpdateServerCreatureList()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(FormatCreatureListData());
        if (data.IsNullOrWhiteSpace()) return;
        ServerCreatureList.Value = data;
    }

    private static void OnServerCreatureListChanged()
    {
        if (ServerCreatureList.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Server Creature List changed, reloading");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<Heightmap.Biome, List<string>> data = deserializer.Deserialize<
            Dictionary<Heightmap.Biome, List<string>>>(ServerCreatureList.Value);

        CreatureLists.BiomeCreatureMap = data.ToDictionary(
            kvp => kvp.Key,
            kvp => CreatureLists.ValidatedPrefabs(kvp.Value));
    }

    #endregion

}