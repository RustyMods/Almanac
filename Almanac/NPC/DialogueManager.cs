using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.ExternalAPIs;
using Almanac.Managers;
using Almanac.Quests;
using Almanac.Store;
using Almanac.TreasureHunt;
using Almanac.UI;
using Almanac.Utilities;
using API;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using PieceManager;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.NPC;

public static class DialogueExtensions
{
    public static string GetDialoguePlayerData(this Player player) => player.m_customData.TryGetValue(DialogueManager.DIALOGUE_PLAYER_KEY, out string? data) ? data : string.Empty;
    public static void SaveDialoguePlayerData(this Player player, string data) => player.m_customData[DialogueManager.DIALOGUE_PLAYER_KEY] = data;
    public static List<string> GetPlayerDialogueIDs(this Player player)
    {
        string data = player.GetDialoguePlayerData();
        return string.IsNullOrEmpty(data) ? new List<string>() : data.Split(';').ToList();
    }
    public static void SaveDialogueID(this Player player, string dialogueID)
    {
        List<string> list = player.GetPlayerDialogueIDs();
        list.Add(dialogueID);
        player.SaveDialoguePlayerData(string.Join(";", list));
    }

    public static bool HasDialogueID(this Player player, string dialogueID)
    {
        List<string> list = player.GetPlayerDialogueIDs();
        return list.Contains(dialogueID);
    }
    
    public static void ClearSavedDialogues(this Player player)
    {
        player.m_customData.Remove(DialogueManager.DIALOGUE_PLAYER_KEY);
    }
    
    public static int GetDialogueByteCount(this Player player)
    {
        if (!player.m_customData.TryGetValue(DialogueManager.DIALOGUE_PLAYER_KEY, out var data)) return 0;
        int size = Encoding.UTF8.GetByteCount(data);
        return size;
    }
}
public partial class DialogueManager : MonoBehaviour
{
    private static readonly Dictionary<string, Dialogue> dialogues = new ();
    private static readonly Dictionary<string, Dialogue> fileDialogues = new ();
    private static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static readonly CustomSyncedValue<string> SyncedDialogue = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Dialogues", "");
    private static readonly AlmanacDir DialogueDir = new(AlmanacPlugin.AlmanacDir.Path, "Dialogues");
    public const string DIALOGUE_PLAYER_KEY = "Almanac.DialogueManager.Data";
    
    public static bool TryGetDialogue(string uniqueID, out Dialogue dialogue) => dialogues.TryGetValue(uniqueID, out dialogue);
    public static List<Dialogue> GetDialogues() => dialogues.Values.ToList();

    private static readonly Dictionary<Minimap.PinData, float> tempPins = new();
    private float pinTimer;
    private static FlightRef? m_flightRef;
    

    public void Update()
    {
        float dt = Time.deltaTime;
        UpdateFlight(dt);
        pinTimer += dt;
        if (pinTimer < 10f) return;
        pinTimer = 0.0f;
        foreach (KeyValuePair<Minimap.PinData, float> pin in new Dictionary<Minimap.PinData, float>(tempPins))
        {
            if (pin.Value > Time.time) continue;
            Minimap.instance.RemovePin(pin.Key);
            tempPins.Remove(pin.Key);
        }
    }

    public void UpdateFlight(float dt)
    {
        if (m_flightRef == null) return;
        m_flightRef.timer += dt;
        if (m_flightRef.timer > 5f)
        {
            m_flightRef.Start();
        }
    }

    private class FlightRef
    {
        public readonly Player player;
        private readonly Vector3 pos;
        public float timer;

        public FlightRef(Player player, Vector3 pos)
        {
            this.player = player;
            this.pos = pos;
        }

        public void Start()
        {
            player.transform.position = pos;
            player.OnSpawned(true);
            m_flightRef = null;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsTeleporting))]
    private static class Player_IsTeleporting_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance, ref bool __result)
        {
            if (m_flightRef == null || m_flightRef.player != __instance) return;
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.WaitingForRespawn))]
    private static class Game_WaitingForRespawn_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (m_flightRef != null) __result = true;
        }
    }
    public static void Setup()
    {
        AlmanacPlugin.instance.gameObject.AddComponent<DialogueManager>();
        Clone npc = new Clone("Player", "AlmanacNPC");
        npc.OnCreated += prefab =>
        {
            EffectList? hitEffects = prefab.GetComponent<Player>().m_hitEffects;
            prefab.Remove<Player>();
            prefab.Remove<Rigidbody>();
            prefab.Remove<PlayerController>();
            prefab.Remove<Talker>();
            prefab.Remove<Skills>();
            prefab.Remove<FootStep>();
            prefab.GetComponent<ZNetView>().m_persistent = true;
            if (prefab.GetComponentInChildren<CharacterAnimEvent>(true) is { } characterAnimEvent) characterAnimEvent.gameObject.Remove<CharacterAnimEvent>();
            prefab.layer = 10;
            NPC? component = prefab.AddComponent<NPC>();
            component.m_hitEffects = hitEffects;
            prefab.AddComponent<NPCTalk>();
            
            Piece? piece = prefab.AddComponent<Piece>();
            piece.m_icon = SpriteManager.GetSprite("blacksmith");
            BuildPiece buildPiece = new BuildPiece(prefab);
            buildPiece.Name.English("Almanac NPC");
            buildPiece.Description.English("Placeable human NPC");
            buildPiece.Category.Set(BuildPieceCategory.Misc);
            buildPiece.RequiredItems.Add("SwordCheat", 1, false);
        };
        SyncedDialogue.ValueChanged += OnSyncedDialogueChange;
        AlmanacPlugin.OnZNetAwake += UpdateSyncedDialogues;
        Read();
        FileSystemWatcher watcher = new FileSystemWatcher(DialogueDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Changed += OnChanged;
    }

    private static void OnCreated(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string data = File.ReadAllText(args.FullPath);
            Dialogue dialogue = deserializer.Deserialize<Dialogue>(data);
            dialogues[dialogue.UniqueID] = dialogue;
            fileDialogues[args.FullPath] = dialogue;
            UpdateSyncedDialogues();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create dialogue: " + Path.GetFileName(args.FullPath));
        }
    }

    private static void OnChanged(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string data = File.ReadAllText(args.FullPath);
            Dialogue dialogue = deserializer.Deserialize<Dialogue>(data);
            dialogues[dialogue.UniqueID] = dialogue;
            fileDialogues[args.FullPath] = dialogue;
            UpdateSyncedDialogues();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change dialogue: " + Path.GetFileName(args.FullPath));
        }
    }

    private static void OnDeleted(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!fileDialogues.TryGetValue(args.FullPath, out Dialogue dialogue)) return;
        dialogues.Remove(dialogue.UniqueID);
        fileDialogues.Remove(args.FullPath);
        UpdateSyncedDialogues();
    }
    
    public static bool Exists(string id) => dialogues.ContainsKey(id);

    private static void Read()
    {
        string[] files = DialogueDir.GetFiles("*.yml", true);
        if (files.Length == 0)
        {
            LoadDefaults();

            string root = DialogueDir.CreateDir("Examples");
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);
            foreach (Dialogue dialogue in dialogues.Values)
            {
                string data = serializer.Serialize(dialogue);
                string fileName = dialogue.UniqueID + ".yml";
                string path = Path.Combine(root, fileName);
                if (!string.IsNullOrEmpty(dialogue.DialogueChain))
                {
                    var overridePath = Path.Combine(root, dialogue.DialogueChain);
                    if (!Directory.Exists(overridePath)) Directory.CreateDirectory(overridePath);
                    path = Path.Combine(overridePath, fileName);
                }
                DialogueDir.WriteFile(path, data);
                fileDialogues[path] = dialogue;
            }
        }
        else
        {
            dialogues.Clear();
            foreach (string file in files)
            {
                try
                {
                    string data = File.ReadAllText(file);
                    Dialogue dialogue = deserializer.Deserialize<Dialogue>(data);
                    dialogues[dialogue.UniqueID] = dialogue;
                    fileDialogues[file] = dialogue;
                }
                catch(Exception e)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse dialogue: " + Path.GetFileName(file));
                    AlmanacPlugin.AlmanacLogger.LogError(e.Message);
                }
            }
        }
    }

    private static void UpdateSyncedDialogues()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedDialogue.Value = serializer.Serialize(dialogues);
    }

    private static void OnSyncedDialogueChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedDialogue.Value)) return;
        try
        {
            dialogues.Clear();
            dialogues.AddRange(deserializer.Deserialize<Dictionary<string, Dialogue>>(SyncedDialogue.Value));
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server dialogues");
        }
    }

    [Serializable]
    public class Dialogue
    {
        public string UniqueID = string.Empty;
        public string Label = string.Empty;
        public string Text = string.Empty;
        public string MissingRequirementsText = string.Empty;
        public string CompletedText = string.Empty;
        
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public List<string> Dialogues = new();

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public DialogueAction? Action;

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public DialogueReqs? Requirements;

        [YamlIgnore] public string DialogueChain = string.Empty;
        
        public Dialogue(){}
        
        public bool HasReceivedItem(Player player) => player.GetPlayerDialogueIDs().Contains(UniqueID);
        public bool HasTakenItems(Player player) => player.GetPlayerDialogueIDs().Contains(UniqueID);
        public bool HasItems(Player player)
        {
            if (Action == null) return false;
            if (!Action.TryGetItemPrefab(out string name, out int amount, out int quality, out int _)) return false;
            int count = 0;
            foreach (ItemDrop.ItemData item in player.GetInventory().GetAllItems())
            {
                if (!ItemHelper.TryGetItemBySharedName(item.m_shared.m_name, out ItemDrop itemDrop)) continue;
                if (itemDrop.name != name || item.m_quality != quality) continue;
                count += item.m_stack;
            }
            return count >= amount;
        }
        public bool IsInteractable(Player player) => Action?.Type switch
        {
            Command.Give => !HasReceivedItem(player),
            Command.Take => HasItems(player),
            Command.CancelTreasure => TreasureManager.ActiveTreasureLocation != null,
            Command.CancelBounty => BountyManager.ActiveBountyLocation != null,
            _ => true
        };
        public void OnClick(DialoguePanel instance, Player player)
        {
            switch (Action?.Type)
            {
                case Command.Exit:
                    instance.Hide();
                    break;
                case Command.OpenAlmanac:
                    instance.Hide();
                    AlmanacPanel.instance?.Show();
                    break;
                case Command.OpenItems:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Items);
                    break;
                case Command.OpenPieces:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Pieces);
                    break;
                case Command.OpenCreatures:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Creatures);
                    break;
                case Command.OpenBounties:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Bounties);
                    break;
                case Command.OpenTreasures:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Treasures);
                    break;
                case Command.OpenStore:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Store);
                    break;
                case Command.OpenAchievements:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Achievements);
                    break;
                case Command.OpenMetrics:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Metrics);
                    break;
                case Command.OpenLeaderboard:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Leaderboard);
                    break;
                case Command.OpenLottery:
                    instance.Hide();
                    AlmanacPanel.instance?.Show(AlmanacPanel.Tab.TabOption.Lottery);
                    break;
                case Command.Give or Command.Take:
                    if (!Action.TryGetItemPrefab(out string itemID, out int amount, out int quality, out int variant)) return;
                    switch (Action.Type)
                    {
                        case Command.Give:
                            if (!player.GetInventory().HaveEmptySlot()) return;
                            ItemDrop.ItemData? item = player.GetInventory().AddItem(itemID, amount, quality, variant, 0L, "");
                            if (item == null) return;
                            player.ShowPickupMessage(item, amount);
                            instance.Hide();
                            break;
                        case Command.Take:
                            if (ObjectDB.instance.GetItemPrefab(itemID) is not { } itemPrefab || !itemPrefab.TryGetComponent(out ItemDrop itemDrop)) return;
                            player.GetInventory().RemoveItem(itemDrop.m_itemData.m_shared.m_name, amount, quality);
                            break;
                    }
                    player.SaveDialogueID(UniqueID);
                    instance.Hide();
                    break;
                case Command.Teleport or Command.MapPin or Command.FlyTo:
                    if (!Action.TryGetVector(out Vector3 pos, out string label, out float duration)) return;
                    switch (Action.Type)
                    {
                        case Command.Teleport:
                            player.TeleportTo(pos, Quaternion.identity, true);
                            instance.Hide();
                            break;
                        case Command.MapPin:
                            if (!Minimap.instance || !ZNet.instance || !Player.m_localPlayer || Game.m_noMap)
                            {
                                instance.Hide();
                            }
                            else
                            {
                                Minimap.PinData pin = Minimap.instance.AddPin(pos, Minimap.PinType.None, label, false, false, Player.m_localPlayer.GetPlayerID());
                                pin.m_icon = SpriteManager.GetSprite(SpriteManager.IconOption.Map);
                                pin.m_shouldDelete = true;
                                tempPins[pin] = Time.time + duration;
                                instance.Hide();
                                Minimap.instance.ShowPointOnMap(pos);
                            }
                            break;
                        case Command.FlyTo:
                            instance.Hide();
                            m_flightRef = new FlightRef(player, pos);
                            break;
                    }
                    break;
                case Command.StartBounty:
                    if (!Action.TryGetBounty(out var bounty)) return;
                    if (!BountyManager.CanPurchase(player, bounty)) return;
                    BountyManager.AcceptBounty(bounty);
                    instance.Hide();
                    break;
                case Command.CancelBounty:
                    BountyManager.CancelBounty();
                    instance.Hide();
                    break;
                case Command.CompleteBounty:
                    if (BountyManager.ActiveBountyLocation is not { } active || !active.data.completed) return;
                    player.AddTokens(active.data.AlmanacTokenReward);
                    active.data.completed = false;
                    BountyManager.ActiveBountyLocation = null;
                    instance.Hide();
                    break;
                case Command.StartQuest or Command.CancelQuest or Command.CompleteQuest:
                    if (!Action.TryGetQuestDialogue(out string questID, out string dialogueID)) return;
                    bool hasFollowup = TryGetDialogue(dialogueID, out Dialogue dialogue);
                    switch (Action.Type)
                    {
                        case Command.StartQuest:
                            QuestManager.StartQuest(questID);
                            if (hasFollowup) instance.Show(dialogue);
                            else instance.Hide();
                            break;
                        case Command.CancelQuest:
                            QuestManager.CancelQuest(questID);
                            if (hasFollowup) instance.Show(dialogue);
                            else instance.Hide();
                            break;
                        case Command.CompleteQuest:
                            if (!QuestManager.IsQuestComplete(questID)) return;
                            QuestManager.CompleteQuest(questID);
                            if (hasFollowup) instance.Show(dialogue);
                            else instance.Hide();
                            break;
                    }
                    break;
                case Command.GiveAlmanacXP:
                    if (!int.TryParse(Action.Parameters, out int almanacXP)) return;
                    ClassesAPI.AddEXP(almanacXP);
                    player.SaveDialogueID(UniqueID);
                    break;
                case Command.GiveWackyXP:
                    if (!int.TryParse(Action.Parameters, out int wackyXP)) return;
                    EpicMMOSystem_API.AddExp(wackyXP);
                    player.SaveDialogueID(UniqueID);
                    break;

            }
        }
        [YamlIgnore] public bool isValid => (Action?.IsValid() ?? true) && (Requirements?.HasRequirements() ?? true);
        [YamlIgnore] public Dialogue? previous;

        public bool ShouldShowDialogue()
        {
            if (!(Requirements?.HasRequirements() ?? true)) return false;
            if (Action?.Type is Command.CancelBounty && BountyManager.ActiveBountyLocation == null) return false;
            if (Action?.Type is Command.CancelTreasure && TreasureManager.ActiveTreasureLocation == null) return false;
            if (Action?.Type is Command.CancelQuest && (Action == null || !Action.TryGetQuestDialogue(out string id, out _) || !QuestManager.IsActive(id))) return false;
            return true;
        }
    }
    
    public enum Command
    {
        None, Exit, Give, Take, Spawn, Teleport, 
        StartQuest, CompleteQuest, CancelQuest,
        StartBounty, CancelBounty, CompleteBounty, 
        StartTreasure, CancelTreasure,
        OpenAlmanac, OpenItems, OpenPieces, OpenCreatures, OpenAchievements, OpenStore, OpenLeaderboard, OpenBounties, OpenTreasures, OpenMetrics, OpenLottery,
        MapPin,
        GiveAlmanacXP, GiveWackyXP, FlyTo
    }

    [Serializable]
    public class DialogueAction
    {
        public Command Type = Command.None;
        public string Label = string.Empty;
        public string Parameters = string.Empty;
        
        public DialogueAction(){}

        public bool IsValid()
        {
            return Type switch
            {
                Command.Give or Command.Take => TryGetItemPrefab(out _,out _,out _,out _),
                Command.Teleport or Command.MapPin or Command.FlyTo => TryGetVector(out _, out _, out _),
                Command.StartBounty => TryGetBounty(out _),
                Command.StartTreasure => TryGetTreasure(out _),
                Command.StartQuest or Command.CancelQuest or Command.CompleteQuest => TryGetQuestDialogue(out _, out _),
                Command.GiveWackyXP => EpicMMOSystem_API.IsLoaded(),
                Command.GiveAlmanacXP => ClassesAPI.IsLoaded(),
                _ => true
            };
        }

        public bool TryGetTreasure(out TreasureManager.TreasureData data)
        {
            data = new();
            return TreasureManager.TryGetTreasure(Parameters, out data);
        }

        public bool TryGetItemPrefab(out string name, out int amount, out int quality, out int variant)
        {
            name = string.Empty;
            amount = 1;
            quality = 1;
            variant = 0;
            if (string.IsNullOrEmpty(Parameters)) return false;
            string[] parts = Parameters.Split(',');
            if (parts.Length < 2) return false;
            name = parts[0].Trim();
            if (ObjectDB.instance.GetItemPrefab(name) is not { } itemPrefab || !itemPrefab.TryGetComponent(out ItemDrop itemDrop)) return false;
            if (parts.Length > 1 && !int.TryParse(parts[1].Trim(), out amount)) amount = 1;
            if (parts.Length > 2 && !int.TryParse(parts[2].Trim(), out quality)) quality = 1;
            if (parts.Length > 3 && !int.TryParse(parts[3].Trim(), out variant)) variant = 0;
            if (variant > itemDrop.m_itemData.m_shared.m_variants) variant = 0;
            return true;
        }

        public bool TryGetVector(out Vector3 vector, out string label, out float duration)
        {
            vector = Vector3.zero;
            label = string.Empty;
            duration = 60f;
            if (string.IsNullOrEmpty(Parameters)) return false;
            string[] parts = Parameters.Split(',');
            if (parts.Length < 2) return false;
            if (!float.TryParse(parts[0].Trim(), out float x)) return false;
            if (!float.TryParse(parts[1].Trim(), out float y)) return false;
            if (!float.TryParse(parts[2].Trim(), out float z)) return false;
            vector = new Vector3(x, y, z);
            if(parts.Length > 3) label = parts[3].Trim();
            if (parts.Length > 4 && !float.TryParse(parts[4].Trim(), out duration)) duration = 60f; 
            return true;
        }

        public bool TryGetBounty(out BountyManager.BountyData data)
        {
            data = new BountyManager.BountyData();
            if (BountyManager.IsOnCooldown()) return false;
            return !string.IsNullOrEmpty(Parameters) && BountyManager.TryGetBountyData(Parameters, out data);
        }

        public bool TryGetQuestDialogue(out string questID, out string dialogueID)
        {
            questID = string.Empty;
            dialogueID = string.Empty;
            if (string.IsNullOrEmpty(Parameters)) return false;
            string[] parts = Parameters.Split(',');
            questID = parts[0].Trim();
            if (parts.Length > 1) dialogueID = parts[1].Trim();
            return true;
        }
    }

    [Serializable]
    public class DialogueReqs
    {
        public string Keys = string.Empty;
        public string NotKeys = string.Empty;
        public string Killed = string.Empty;
        public string NotKilled = string.Empty;
        public string Achievements = string.Empty;
        public string NotAchievements = string.Empty;
        public string Dialogues = string.Empty;
        public string NotDialogues = string.Empty;
        public string Quests = string.Empty;
        public string NotQuests = string.Empty;
        public string CompletedQuests = string.Empty;
        public string NotCompletedQuests = string.Empty;
        public bool HasRequirements() => 
            HasRequiredKills() && HasNotRequiredKills() 
                               && HasKeys() && HasNotKeys() 
                               && HasAchievements() && HasNotAchievements() 
                               && HasDialogues() && HasNotDialogues()
                               && HasQuests() && HasNotQuests()
                               && HasCollectedQuests() && HasNotCollectedQuests();

        public bool HasCollectedQuests()
        {
            if (string.IsNullOrEmpty(CompletedQuests)) return true;
            IEnumerable<string> list = CompletedQuests.Split(',').Select(x => x.Trim());
            return list.All(QuestManager.IsQuestCollected);
        }

        public bool HasNotCollectedQuests()
        {
            if (string.IsNullOrEmpty(NotCompletedQuests)) return true;
            IEnumerable<string> list = NotCompletedQuests.Split(',').Select(x => x.Trim());
            return list.All(x => !QuestManager.IsQuestCollected(x));
        }
        public bool HasQuests()
        {
            if (string.IsNullOrEmpty(Quests)) return true;
            IEnumerable<string> list = Quests.Split(',').Select(s => s.Trim());
            return list.All(QuestManager.IsActive);
        }
        public bool HasNotQuests()
        {
            if (string.IsNullOrEmpty(NotQuests)) return true;
            IEnumerable<string> list = NotQuests.Split(',').Select(s => s.Trim());
            return list.All(quest => !QuestManager.IsActive(quest));
        }
        public bool HasDialogues()
        {
            if (string.IsNullOrEmpty(Dialogues)) return true;
            if (!Player.m_localPlayer) return false;
            string[] list = Dialogues.Split(',');
            List<string> recordedDialogues = Player.m_localPlayer.GetPlayerDialogueIDs();
            return list.All(s => recordedDialogues.Contains(s.Trim()));
        }
        public bool HasNotDialogues()
        {
            if (string.IsNullOrEmpty(NotDialogues)) return true;
            if (!Player.m_localPlayer) return false;
            string[] list = Dialogues.Split(',');
            List<string> recordedDialogues =  Player.m_localPlayer.GetPlayerDialogueIDs();
            return list.All(s => !recordedDialogues.Contains(s.Trim()));
        }
        public bool HasAchievements()
        {
            if (string.IsNullOrEmpty(Achievements)) return true;
            IEnumerable<string> list = Achievements.Split(',').Select(s => s.Trim());
            foreach (string? id in list)
            {
                if (!AchievementManager.achievements.TryGetValue(id, out AchievementManager.Achievement achievement)) return false;
                if (!Player.m_localPlayer) return false;
                if (!achievement.IsCompleted(Player.m_localPlayer)) return false;
            }
            return true;
        }
        public bool HasNotAchievements()
        {
            if (string.IsNullOrEmpty(NotAchievements)) return true;
            IEnumerable<string> list = NotAchievements.Split(',').Select(s => s.Trim());
            foreach (string? id in list)
            {
                if (!AchievementManager.achievements.TryGetValue(id, out AchievementManager.Achievement achievement)) return false;
                if (!Player.m_localPlayer) return false;
                if (achievement.IsCompleted(Player.m_localPlayer)) return false;
            }
            return true;
        }

        public bool HasKeys()
        {
            if (!ZoneSystem.instance || string.IsNullOrEmpty(Keys)) return true;
            IEnumerable<string> list = Keys.Split(',').Select(s => s.Trim());
            return list.All(s => ZoneSystem.instance.CheckKey(s) || ZoneSystem.instance.CheckKey(s, GameKeyType.Player));
        }

        public bool HasNotKeys()
        {
            if (!ZoneSystem.instance || string.IsNullOrEmpty(NotKeys)) return true;
            IEnumerable<string> list = NotKeys.Split(',').Select(s => s.Trim());
            return list.All(s => !ZoneSystem.instance.CheckKey(s) && !ZoneSystem.instance.CheckKey(s, GameKeyType.Player));
        }
        
        public bool HasRequiredKills()
        {
            if (string.IsNullOrEmpty(Killed)) return true;
            foreach (string info in Killed.Split(';'))
            {
                string[] parts = info.Trim().Split(',');
                if (parts.Length < 2) return false;
                string name = parts[0].Trim();
                if (!int.TryParse(parts[1].Trim(), out int amount)) return false;
                if (!CritterHelper.namedCritters.TryGetValue(name, out var critter)) return false;
                if (PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, critter.character.m_name) < amount) return false;
            }
            return true;
        }

        public bool HasNotRequiredKills()
        {
            if (string.IsNullOrEmpty(NotKilled)) return true;
            IEnumerable<string> list = NotKilled.Split(',').Select(s => CritterHelper.namedCritters.TryGetValue(s.Trim(), out CritterHelper.CritterInfo info) ? info.character.m_name : s.Trim());
            return list.All(s => PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, s) <= 0);
        }
    }
}