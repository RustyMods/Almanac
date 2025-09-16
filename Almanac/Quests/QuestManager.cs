using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Almanac.Data;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using ServerSync;
using YamlDotNet.Serialization;

namespace Almanac.Quests;

public static class QuestManager
{
    private static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static readonly CustomSyncedValue<string> SyncedQuests = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Quests", "");
    private static readonly Dictionary<string, QuestData> quests = new();
    private static readonly Dictionary<string, QuestData> fileQuests = new();
    private static readonly Dictionary<QuestType, Dictionary<string, Quest>> activeQuests = new();
    private const string PlayerQuestKey = "Almanac.Quests.Player.Key";
    private static readonly AlmanacDir QuestDir = new AlmanacDir(AlmanacPlugin.AlmanacDir.Path, "Quests");
    
    public static bool Exists(string id) => quests.ContainsKey(id);
    public static bool TryGetQuestData(string id, out QuestData quest) => quests.TryGetValue(id, out quest);
    public static List<Quest> GetActiveQuests() => activeQuests.Values.SelectMany(kvp => kvp.Values).ToList();
    public static List<Quest> GetQuestsByType(QuestType type) => activeQuests.TryGetValue(type, out Dictionary<string, Quest> list) ? list.Values.ToList() : new();

    public static bool IsActive(string id)
    {
        return quests.TryGetValue(id, out var quest) && activeQuests.TryGetValue(quest.Type, out var questDict) && questDict.ContainsKey(id);
    }

    private static readonly Entries.EntryBuilder builder = new();

    public static List<Entries.Entry> ToEntries()
    {
        builder.Clear();
        foreach (Quest quest in GetActiveQuests())
        {
            builder.Add(quest.name);
            if (quest.isCompleted) builder.Add(Keys.Progress, "Completed");
            else builder.Add(quest.GetTooltip(false), "lore");
        }
        return builder.ToList();
    }

    public static void StartQuest(string id)
    {
        if (IsActive(id)) return;
        if (!TryGetQuestData(id, out var quest)) return;
        activeQuests.AddOrSet(quest.Type, quest.UniqueID, new Quest(){id = quest.UniqueID});
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Started quest `{quest.Name}`");
        QuestPanel.instance?.LoadActiveQuests();
    }

    public static void CancelQuest(string id)
    {
        if (!IsActive(id)) return;
        if (!TryGetQuestData(id, out var quest)) return;
        if (!activeQuests.TryGetValue(quest.Type, out var questDict)) return;
        questDict.Remove(id);
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Canceled quest `{quest.Name}`");
        QuestPanel.instance?.LoadActiveQuests();
    }

    public static bool IsQuestComplete(string id)
    {
        if (!IsActive(id)) return false;
        if (!TryGetQuestData(id, out QuestData quest)) return false;
        if (!activeQuests.TryGetValue(quest.Type, out var questDict)) return false;
        return questDict.TryGetValue(id, out Quest activeQuest) && activeQuest.IsComplete();
    }

    public static bool IsQuestCollected(string id)
    {
        if (!IsActive(id)) return false;
        if (!TryGetQuestData(id, out var quest)) return false;
        if (!activeQuests.TryGetValue(quest.Type, out var questDict)) return false;
        return questDict.TryGetValue(id, out Quest activeQuest) && activeQuest.isCompleted;
    }

    public static void CompleteQuest(string id)
    {
        if (!IsActive(id)) return;
        if (!TryGetQuestData(id, out var quest)) return;
        if (!activeQuests.TryGetValue(quest.Type, out var questDict)) return;
        if (!questDict.TryGetValue(id, out var activeQuest)) return;
        activeQuest.isCompleted = true;
        QuestPanel.instance?.LoadActiveQuests();
    }

    public static void Setup()
    {
        AlmanacPlugin.OnPlayerProfileLoadPlayerDataPostfix += player => player.LoadActiveQuests();
        AlmanacPlugin.OnPlayerProfileSavePlayerDataPrefix += player => player.SaveActiveQuests();
        PlayerInfo.OnCharacterDeathByLocal += character =>
        {
            foreach (var quest in GetQuestsByType(QuestType.Kill))
            {
                if (quest.GetSharedName() != character.m_name) continue;
                quest.Increment();
            }
        };
        SyncedQuests.ValueChanged += OnSyncedQuestChange;
        AlmanacPlugin.OnZNetAwake += UpdateSyncedQuests;
        Read();
        SetupWatcher();
    }

    private static void SetupWatcher()
    {
        FileSystemWatcher watcher = new  FileSystemWatcher(QuestDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
    }

    private static void Read()
    {
        string[] files = QuestDir.GetFiles("*.yml", true);
        if (files.Length == 0)
        {
            LoadDefaults();
            foreach (QuestData? quest in quests.Values)
            {
                string data = serializer.Serialize(quest);
                string fileName = quest.UniqueID + ".yml";
                string path = QuestDir.WriteFile(fileName, data);
                fileQuests[path] = quest;
            }
        }
        else
        {
            quests.Clear();
            foreach (string file in files)
            {
                try
                {
                    string data = File.ReadAllText(file);
                    QuestData quest = deserializer.Deserialize<QuestData>(data);
                    quests[quest.UniqueID] = quest;
                    fileQuests[file] = quest;
                }
                catch
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse quest: " + Path.GetFileName(file));
                }
            }
        }
    }

    private static void OnSyncedQuestChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedQuests.Value)) return;
        try
        {
            Dictionary<string, QuestData> data = deserializer.Deserialize<Dictionary<string, QuestData>>(SyncedQuests.Value);
            quests.Clear();
            quests.AddRange(data);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server quests");
        }
    }

    private static void UpdateSyncedQuests()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string data = serializer.Serialize(quests);
        SyncedQuests.Value = data;
    }

    private static void OnChanged(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string data = File.ReadAllText(args.FullPath);
            QuestData quest = deserializer.Deserialize<QuestData>(data);
            quests[quest.UniqueID] = quest;
            fileQuests[args.FullPath] = quest;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create quest: " + Path.GetFileName(args.FullPath));
        }
    }

    private static void OnCreated(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            var data = File.ReadAllText(args.FullPath);
            var quest = deserializer.Deserialize<QuestData>(data);
            quests[quest.UniqueID] = quest;
            fileQuests[args.FullPath] = quest;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create quest: " + Path.GetFileName(args.FullPath));
        }
    }

    private static void OnDeleted(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!fileQuests.TryGetValue(args.FullPath, out var quest)) return;
        fileQuests.Remove(args.FullPath);
        quests.Remove(quest.UniqueID);
    }

    private static void LoadDefaults()
    {
        QuestData harvest = new  QuestData();
        harvest.Type = QuestType.Harvest;
        harvest.UniqueID = "001.Dandelion";
        harvest.Name = "Weeds";
        harvest.PrefabName = "Pickable_Dandelion";
        harvest.Threshold = 50;
        quests[harvest.UniqueID] = harvest;
        
        QuestData collect = new QuestData();
        collect.Type = QuestType.Collect;
        collect.UniqueID = "001.SurtlingCores";
        collect.Name = "Cores";
        collect.PrefabName = "SurtlingCore";
        collect.Threshold = 10;
        quests[collect.UniqueID] = collect;

        QuestData collectItems = new QuestData();
        collectItems.Type = QuestType.LearnItems;
        collectItems.UniqueID = "001.WoodenSword";
        collectItems.Name = "Wooden Sword";
        collectItems.PrefabNames.Add("Wood", "FineWood", "RoundLog");
        quests[collectItems.UniqueID] = collectItems;

        QuestData farm = new QuestData();
        farm.Type = QuestType.Farm;
        farm.UniqueID = "001.Farm";
        farm.Name = "Farm";
        farm.PrefabName = "sapling_seedcarrot";
        farm.Threshold = 10;
        quests[farm.UniqueID] = farm;

        QuestData kill = new QuestData();
        kill.Type = QuestType.Kill;
        kill.UniqueID = "001.Kill";
        kill.Name = "Hunt Boars";
        kill.PrefabName = "Boar";
        kill.Threshold = 10;
        quests[kill.UniqueID] = kill;

    }

    private static void LoadActiveQuests(this Player player)
    {
        activeQuests.Clear();
        if (!player.m_customData.TryGetValue(PlayerQuestKey, out string serialized)) return;
        try
        {
            List<Quest> data = deserializer.Deserialize<List<Quest>>(serialized);
            foreach (Quest? quest in data)
            {
                if (!quest.isValid || quest.data == null) continue;
                activeQuests.AddOrSet(quest.type, quest.data.UniqueID, quest);
            }
            QuestPanel.instance?.LoadActiveQuests();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to load active quests");
            player.m_customData.Remove(PlayerQuestKey);
        }
    }

    private static void SaveActiveQuests(this Player player)
    {
        List<Quest> questToSave = new List<Quest>();
        foreach (var questsDict in activeQuests.Values)
        {
            questToSave.AddRange(questsDict.Values);
        }

        if (questToSave.Count == 0)
        {
            player.m_customData.Remove(PlayerQuestKey);
        }
        else
        {
            string data = serializer.Serialize(questToSave);
            player.m_customData[PlayerQuestKey] = data;
        }
    }

    public static void ClearSavedQuests(this Player player)
    {
        player.m_customData.Remove(PlayerQuestKey);
        activeQuests.Clear();
        QuestPanel.instance?.Clear();
    }

    [Serializable]
    public class Quest
    {
        public string id = string.Empty;
        public int progress;
        public bool isCompleted;
        
        [YamlIgnore] public QuestData? data => TryGetQuestData(id, out QuestData quest) ? quest : null;
        [YamlIgnore] public bool isValid => data != null;
        [YamlIgnore] public QuestType type => data?.Type ?? QuestType.None;
        [YamlIgnore] public string name => data?.Name ?? string.Empty;
        [YamlIgnore] public string prefabName => data?.PrefabName ?? string.Empty;
        [YamlIgnore] public List<string> prefabNames => data?.PrefabNames ?? new List<string>();
        [YamlIgnore] public QuestPanel.TextArea? ui;

        public void Increment(int amount = 1)
        {
            progress += amount;
            ui?.SetText(GetTooltip());
        }

        public void SetProgress(int amount)
        {
            progress = amount;
            ui?.SetText(GetTooltip());
        }
        public bool IsComplete() => progress >= data?.Threshold || isCompleted;

        public string GetSharedName() => type switch
        {
            QuestType.Collect => ObjectDB.instance.GetItemPrefab(prefabName)?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_name ?? prefabName,
            QuestType.Mine => ZNetScene.instance.GetPrefab(prefabName)?.GetComponent<HoverText>()?.m_text ?? prefabName,
            QuestType.Kill => CritterHelper.namedCritters.TryGetValue(prefabName, out var info) ? info.character.m_name : prefabName,
            QuestType.Harvest => ZNetScene.instance.GetPrefab(prefabName)?.GetComponent<Pickable>()?.m_itemPrefab?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_name ?? prefabName,
            QuestType.Farm => ZNetScene.instance.GetPrefab(prefabName)?.GetComponent<Plant>()?.m_name ?? prefabName,
            _ => prefabName
        };

        public HashSet<string> GetSharedNames() => type switch
        {
            QuestType.LearnItems => prefabNames.Select(n => ObjectDB.instance.GetItemPrefab(n)?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_name ?? n).ToHashSet(),
            _ => new HashSet<string>()
        };

        private static StringBuilder sb = new();
        public string GetTooltip(bool includeName = true)
        {
            if (!Player.m_localPlayer) return "";
            sb.Clear();
            if (includeName) sb.Append($"<color=orange><b>{name}</b></color>");
            switch (type)
            {
                default:
                    sb.Append($"\n{GetSharedName()}: {progress}/{data?.Threshold}");
                    break;
                case QuestType.LearnItems:
                    foreach (string sharedName in GetSharedNames())
                    {
                        bool isKnown = Player.m_localPlayer.IsKnownMaterial(sharedName);
                        sb.Append($"\n- {(isKnown ? sharedName : "???")}");
                    }
                    sb.Append($"\n{Keys.Progress}:  {progress}/{prefabNames.Count}");
                    break;
            }
            return sb.ToString();
        }
    }
    
    [Serializable]
    public class QuestData
    {
        public string UniqueID = string.Empty;
        public string Name = string.Empty;
        public QuestType Type = QuestType.None;
        public string PrefabName = string.Empty;
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public List<string> PrefabNames = new();
        public int Threshold;
    }
}