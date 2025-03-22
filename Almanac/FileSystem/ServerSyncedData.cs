using System.Collections.Generic;
using System.Linq;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.TreasureHunt;
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
        if (!ZNet.instance || !ZNet.instance.IsServer())
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
        string data = serializer.Serialize(AchievementYML.m_data);
        if (data.IsNullOrWhiteSpace()) return;

        ServerAchievements.Value = data;
    }

    public static void OnServerAchievementChanged()
    {
        if (ServerAchievements.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received new achievements");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            AchievementYML.m_data = deserializer.Deserialize<List<AchievementYML.Data>>(ServerAchievements.Value);
            InitiatedServerAchievements = true;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to deserialize server achievements");
        }
    }
    
    #endregion
    
    #region Ignore List
    
    private static readonly CustomSyncedValue<string> ServerIgnoreList =
        new(AlmanacPlugin.ConfigSync, "ServerIgnoreList", "");

    public static void InitServerIgnoreList()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer())
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
        string data = serializer.Serialize(Filters.m_filter);
        if (data.IsNullOrWhiteSpace()) return;

        ServerIgnoreList.Value = data;
    }

    private static void OnServerIgnoreListChanged()
    {
        if (ServerIgnoreList.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received server ignore list");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<string> data = deserializer.Deserialize<List<string>>(ServerIgnoreList.Value);

        Filters.m_filter = data;
    }
    
    #endregion

    #region Creature List

    private static readonly CustomSyncedValue<string> ServerCreatureList = new(AlmanacPlugin.ConfigSync, "CreatureList", "");

    private static readonly CustomSyncedValue<string> ServerCustomCreatureList = new(AlmanacPlugin.ConfigSync, "CustomCreatureList", "");
    public static void InitServerCreatureList()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer())
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
            kvp => kvp.Value.Select(creature => creature.m_prefabName).ToList()
        );
    }

    private static Dictionary<string, List<string>> FormatCustomCreatureListData()
    {
        return CreatureLists.CustomCreatureGroups.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(creature => creature.m_prefabName).ToList()
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
        try
        {
            Dictionary<Heightmap.Biome, List<string>> data =
                deserializer.Deserialize<Dictionary<Heightmap.Biome, List<string>>>(ServerCreatureList.Value);

            CreatureLists.BiomeCreatureMap = data.ToDictionary(
                kvp => kvp.Key,
                kvp => CreatureLists.ValidatedPrefabs(kvp.Key.ToString(), kvp.Value));
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to deserialize server biome creatures");
        }
    }

    private static void OnServerCustomCreatureListChanged()
    {
        if (ServerCustomCreatureList.Value.IsNullOrWhiteSpace()) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Server Custom Creature List changed, reloading");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            Dictionary<string, List<string>> data =
                deserializer.Deserialize<Dictionary<string, List<string>>>(ServerCustomCreatureList.Value);
            CreatureLists.CustomCreatureGroups = data.ToDictionary(
                kvp => kvp.Key,
                kvp => CreatureLists.ValidatedPrefabs(kvp.Key, kvp.Value));
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to deserialize server custom creatures");
        }
    }

    #endregion
    
    #region Bounties

    public static readonly CustomSyncedValue<string>
        ServerBountyList = new(AlmanacPlugin.ConfigSync, "BountyList", "");

    public static void InitServerBountyList()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer())
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Awaiting server bounties");
            ServerBountyList.ValueChanged += OnServerBountyListChange;
        }
        else
        {
            if (BountyManager.ValidatedBounties.Count <= 0) return;
            UpdateServerBountyList();
        }
    }

    public static void UpdateServerBountyList()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Initializing server bounties");
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(BountyManager.ValidatedBounties);
        ServerBountyList.Value = data;
    }

    private static void OnServerBountyListChange()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received server bounties");
        BountyManager.InitBounties(false);
        
    }

    #endregion
    
    #region Treasure Hunt

    public static readonly CustomSyncedValue<string> ServerTreasureList = new(AlmanacPlugin.ConfigSync, "ServerTreasure", "");

    public static void InitServerTreasureHunt()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer())
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Awaiting server treasure hunts");
            ServerTreasureList.ValueChanged += OnServerTreasureChange;
        }
        else
        {
            UpdateServerTreasureList();
        }
    }

    public static void UpdateServerTreasureList()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Updating server treasure");
        TreasureManager.InitTreasureManager();
        var serializer = new SerializerBuilder().Build();
        var data = serializer.Serialize(TreasureManager.ValidatedYML);
        ServerTreasureList.Value = data;
    }

    private static void OnServerTreasureChange()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received server treasures");
        TreasureManager.InitTreasureManager(true);
    }
    
    #endregion

}