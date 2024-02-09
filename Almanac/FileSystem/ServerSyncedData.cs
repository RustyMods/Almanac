using System.Collections.Generic;
using System.Linq;
using Almanac.Achievements;
using Almanac.Data;
using BepInEx;
using ServerSync;
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
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received new achievements");
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
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received server ignore list");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<string> data = deserializer.Deserialize<List<string>>(ServerIgnoreList.Value);

        Filters.FilterList = data;
        ItemDataCollector.ClearCachedItemDrops();
    }
    
    #endregion

    #region Creature List

    private static readonly CustomSyncedValue<string> ServerCreatureList = new(AlmanacPlugin.ConfigSync, "CreatureList", "");

    private static readonly CustomSyncedValue<string> ServerCustomCreatureList = new(AlmanacPlugin.ConfigSync, "CustomCreatureList", "");
    public static void InitServerCreatureList()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Awaiting server creature list");

            ServerCreatureList.ValueChanged += OnServerCreatureListChanged;
            ServerCustomCreatureList.ValueChanged += OnServerCustomCreatureListChanged;
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

    private static Dictionary<string, List<string>> FormatCustomCreatureListData()
    {
        return CreatureLists.CustomCreatureGroups.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(creature => creature.name).ToList()
            );
    }

    public static void UpdateServerCreatureList()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(FormatCreatureListData());
        if (!data.IsNullOrWhiteSpace()) ServerCreatureList.Value = data;

        string customData = serializer.Serialize(FormatCustomCreatureListData());
        if (!customData.IsNullOrWhiteSpace()) ServerCustomCreatureList.Value = customData;
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

    private static void OnServerCustomCreatureListChanged()
    {
        if (ServerCustomCreatureList.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Server Custom Creature List changed, reloading");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, List<string>> data =
            deserializer.Deserialize<Dictionary<string, List<string>>>(ServerCustomCreatureList.Value);
        CreatureLists.CustomCreatureGroups = data.ToDictionary(
            kvp => kvp.Key,
            kvp => CreatureLists.ValidatedPrefabs(kvp.Value));
    }

    #endregion

}