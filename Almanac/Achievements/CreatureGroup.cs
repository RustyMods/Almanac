using System.Collections.Generic;
using System.IO;
using Almanac.Data;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using YamlDotNet.Serialization;

namespace Almanac.Achievements;
public static class CreatureGroup
{
    private static readonly ISerializer serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();
    private static readonly CustomSyncedValue<string> SyncedCreatureGroups = new(AlmanacPlugin.ConfigSync, "Almanac_Synced_Creature_Groups", "");
    private static readonly Dictionary<string, List<string>> groups = new();
    public static int GetProgress(string group, int threshold)
    {
        if (!groups.TryGetValue(group, out List<string> list)) return 0;
        int count = 0;
        foreach (string name in list)
        {
            if (!CritterHelper.namedCritters.TryGetValue(name, out var info)) continue;
            if (PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, info.character.m_name) < threshold) continue;
            ++count;
        }
        return count;
    }
    public static bool Exists(string name) => groups.ContainsKey(name);
    public static bool TryGetGroup(string name, out List<string> group) => groups.TryGetValue(name, out group);
    public static void Setup()
    {
        AlmanacPlugin.OnZNetAwake += UpdateSyncedGroups;
        LoadDefaults();
        AlmanacPaths.CreateFolderDirectories();
        string[] files = Directory.GetFiles(AlmanacPaths.CreatureFolderPath, "*.yml");
        if (files.Length == 0)
        {
            string path = AlmanacPaths.CreatureFolderPath + Path.DirectorySeparatorChar + "Creatures.yml";
            string data = serializer.Serialize(groups);
            File.WriteAllText(path, data);
        }
        else
        {
            groups.Clear();
            foreach (string file in files)
            {
                string data = File.ReadAllText(file);
                groups.AddRange(deserializer.Deserialize<Dictionary<string, List<string>>>(data));
            }
        }

        SyncedCreatureGroups.ValueChanged += OnSyncedGroupsChange;
        FileSystemWatcher watcher = new  FileSystemWatcher(AlmanacPaths.CreatureFolderPath, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
    }
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        Dictionary<string, List<string>> data = deserializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(e.FullPath));
        groups.AddRange(data);
        UpdateSyncedGroups();
    }
    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        Dictionary<string, List<string>> data = deserializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(e.FullPath));
        foreach(string key in data.Keys) groups.Remove(key);
        UpdateSyncedGroups();
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        Dictionary<string, List<string>> data = deserializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(e.FullPath));
        groups.AddRange(data);
        UpdateSyncedGroups();
    }
    private static void UpdateSyncedGroups()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedCreatureGroups.Value = serializer.Serialize(groups);
    }
    private static void OnSyncedGroupsChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedCreatureGroups.Value)) return;
        groups.Clear();
        groups.AddRange(deserializer.Deserialize<Dictionary<string, List<string>>>(SyncedCreatureGroups.Value));
    }
    private static void LoadDefaults()
    {
        groups["Meadows"] = new() { "Neck", "Greyling", "Boar", "Deer", "Eikthyr" };
        groups["BlackForest"] = new() { "Skeleton", "Greydwarf", "Bjorn", "Greydwarf_Shaman", "Greydwarf_Elite", "Ghost", "Troll", "gd_king" };
        groups["Swamp"] = new() { "Skeleton_Poison", "Leech", "Draugr", "BogWitchKvastur", "Draugr_Ranged", "Draugr_Elite", "Blob", "BlobElite", "Wraith", "Abomination", "Surtling", "Bonemass" };
        groups["Mountain"] = new() { "Wolf", "Ulv", "Wolf_cub", "Fenring", "Fenring_Cultist", "StoneGolem", "Hatchling", "Bat", "Dragon" };
        groups["Plains"] = new() { "Goblin", "GoblinShaman", "Unbjorn", "GoblinBrute", "Lox", "Lox_Calf", "Deathsquito", "BlobTar", "GoblinKing" };
        groups["Mistlands"] = new() { "Seeker", "SeekerBrood", "SeekerBrute", "Tick", "Gjall", "Dverger", "DvergerMage", "Hare", "Hen", "SeekerQueen" };
        groups["Ocean"] = new() { "Serpent", "BonemawSerpent" };
        groups["AshLands"] = new() { "Charred_Melee", "Morgen", "Charred_Archer", "BlobLava", "Charred_Mage", "Charred_Twitcher", "Volture", "Asksvin", "Asksvin_hatchling", "FallenValkyrie", "Fader" };
        groups["Bosses"] = new() { "Eikthyr", "gd_king", "Bonemass", "Dragon", "GoblinKing", "SeekerQueen", "Fader" };
        groups["MiniBosses"] = new() { "Skeleton_Hildir", "Fenring_Cultist_Hildir", "GoblinBrute_Hildir", "GoblinBruteBros", "GoblinShaman_Hildir", "Charred_Melee_Dyrnwyn"};
    }
}