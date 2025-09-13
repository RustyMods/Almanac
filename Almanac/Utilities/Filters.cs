using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Managers;
using BepInEx;
using ServerSync;
using YamlDotNet.Serialization;

namespace Almanac.Utilities;

public static class Filters
{
    private static readonly CustomSyncedValue<string> ServerFilters = new(AlmanacPlugin.ConfigSync, "Almanac_Synced_Filters", "");
    private static readonly CustomSyncedValue<string> ServerSpecialFilters = new(AlmanacPlugin.ConfigSync, "Almanac_Synced_Special_Filters", "");
    private static readonly ISerializer serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();
    public static readonly AlmanacDir FilterDir = new (AlmanacPlugin.AlmanacDir.Path, "Filters");

    public static bool Ignore(string name)
    {
        if (!Configs.UseIgnoreList) return false;
        return filters.Contains(name) || specialFilters.Any(name.EndsWith);
    }

    private static readonly List<string> filters = new();
    private static readonly List<string> m_default = new()
    {
        "#List out prefabs to ignore:",
        "StaminaUpgrade_Greydwarf",
        "StaminaUpgrade_Troll",
        "StaminaUpgrade_Wraith",
        "IronOre",
        "DvergerArbalest_shoot",
        "DvergerArbalest",
        "CapeTest",
        "SledgeCheat",
        "SwordCheat",
        "HealthUpgrade_Bonemass",
        "HealthUpgrade_GDKing",
        "guard_stone_test",
        "Trailership",
        "Player",
        "TorchMist",
        "NPC_HelmetIron_Worn0",
        "NPC_HelmetBronze_Worn0",
        "NPC_ArmorIronChest_Worn",
        "NPC_ArmorIronLegs_Worn",
        "TrainingDummy",
        "VegvisirShard_Bonemass",
        "CapeOdin",
        "HelmetOdin",
        "TankardOdin",
        "ShieldIronSquare",
        "SwordIronFire",
        "goblin_bed",
        "DvergerTest",
        "Hive",
        "Pot_Shard_Red",
        "Goblin_Gem",
        "Leech_cave",
        "staff_greenroots_tentaroot",
        "Morgen_NonSleeping",
        "Skeleton_NoArcher",
        "TheHive",
        "portal",
        "raise",
        "mud_road",
        "cultivate",
        "path",
        "paved_road",
        "fire_pit_haldor",
        "fire_pit_hildir",
        "ShieldKnight",
        "Larva",
        "TurretBoltBone",
        "FlametalOre",
        "Flametal",
        "*_nochest",
    };

    private static readonly List<string> specialFilters = new();

    private static void UpdateServerFilters()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        ServerFilters.Value = serializer.Serialize(filters);
        ServerSpecialFilters.Value = serializer.Serialize(specialFilters);
    }

    private static void OnServerSpecialFiltersChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(ServerSpecialFilters.Value)) return;
        try
        {
            specialFilters.Clear();
            specialFilters.AddRange(deserializer.Deserialize<List<string>>(ServerSpecialFilters.Value));
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server filters");
        }
    }

    private static void OnServerFiltersChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(ServerFilters.Value)) return;
        try
        {
            filters.Clear();
            filters.AddRange(deserializer.Deserialize<List<string>>(ServerFilters.Value));
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server filters");
        }
    }
    public static void Setup()
    {
        string[] files = FilterDir.GetFiles("*.yml");
        if (files.Length == 0)
        {
            FilterDir.WriteAllLines("IgnoreList.yml", m_default);
        }
        
        filters.Clear();
        foreach (string file in FilterDir.GetFiles("*.yml"))
        {
            foreach (string line in File.ReadAllLines(file))
            {
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("*")) specialFilters.Add(line.Replace("*", string.Empty));
                else filters.Add(line);
            }
        }

        AlmanacPlugin.OnZNetAwake += UpdateServerFilters;
        ServerFilters.ValueChanged += OnServerFiltersChanged;
        ServerSpecialFilters.ValueChanged += OnServerSpecialFiltersChanged;

        FileSystemWatcher watcher = new(FilterDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnDeleted;
    }

    private static void OnChanged(object source, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        filters.Clear();
        foreach (string line in File.ReadAllLines(e.FullPath))
        {
            if (line.StartsWith("#")) continue;
            if (line.StartsWith("*")) specialFilters.Add(line.Replace("*", string.Empty));
            else filters.Add(line);
        }
        UpdateServerFilters();
    }

    private static void OnCreated(object source, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        foreach (string line in File.ReadAllLines(e.FullPath))
        {
            if (line.StartsWith("#")) continue;
            if (line.StartsWith("*")) specialFilters.Add(line.Replace("*", string.Empty));
            else filters.Add(line);
        }
        UpdateServerFilters();
    }
    
    private static void OnDeleted(object source, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        filters.Clear();
        foreach (string file in FilterDir.GetFiles("*.yml"))
        {
            foreach (string line in File.ReadAllLines(file))
            {
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("*")) specialFilters.Add(line.Replace("*", string.Empty));
                else filters.Add(line);
            }
        }
        UpdateServerFilters();
    }
}