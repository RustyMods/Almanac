using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.Managers;
using Almanac.Store;
using Almanac.TreasureHunt;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
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
}
public class DialogueManager : MonoBehaviour
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

    public void Update()
    {
        float dt = Time.deltaTime;
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
    
    public static void Setup()
    {
        AlmanacPlugin.instance.gameObject.AddComponent<DialogueManager>();
        Clone npc = new Clone("Player", "AlmanacNPC");
        npc.OnCreated += prefab =>
        {
            var hitEffects = prefab.GetComponent<Player>().m_hitEffects;
            prefab.Remove<Player>();
            prefab.Remove<Rigidbody>();
            prefab.Remove<PlayerController>();
            prefab.Remove<Talker>();
            prefab.Remove<Skills>();
            prefab.Remove<FootStep>();
            prefab.GetComponent<ZNetView>().m_persistent = true;
            if (prefab.GetComponentInChildren<CharacterAnimEvent>(true) is { } characterAnimEvent) characterAnimEvent.gameObject.Remove<CharacterAnimEvent>();
            prefab.layer = 10;
            var component = prefab.AddComponent<NPC>();
            component.m_hitEffects = hitEffects;
            prefab.AddComponent<NPCTalk>();
            
            Piece? piece = prefab.AddComponent<Piece>();
            piece.m_icon = SpriteManager.GetSprite(SpriteManager.IconOption.Almanac);
            BuildPiece buildPiece = new BuildPiece(prefab);
            buildPiece.Name.English("Almanac NPC");
            buildPiece.Description.English("Placeable human NPC [admin only]");
            buildPiece.Category.Set("Almanac");
            buildPiece.SpecialProperties.NoConfig = true;
            buildPiece.SpecialProperties.AdminOnly = true;
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
                string path = Path.Combine(root, dialogue.UniqueID + ".yml");
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

    private static void LoadDefaults()
    {
        // Main intro dialogue with multiple conversation branches
        Dialogue intro = new Dialogue();
        intro.UniqueID = "npc.intro";
        intro.Label = "Greetings, traveler";
        intro.Text = "Welcome to the world of Almanac! I'm here to help you navigate this realm and its many mysteries.";
        intro.Dialogues.Add("npc.about_almanac", "npc.about_bounties", "npc.about_treasures", "npc.get_weapon", "npc.teleport_example");
        intro.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Farewell"
        };
        dialogues[intro.UniqueID] = intro;

        // Almanac information with action to open the panel
        Dialogue aboutAlmanac = new Dialogue();
        aboutAlmanac.UniqueID = "npc.about_almanac";
        aboutAlmanac.Label = "Tell me about the Almanac";
        aboutAlmanac.Text = "The Almanac is your comprehensive guide to this world. Press Tab to open your inventory, then click the Almanac icon in the top right corner.";
        aboutAlmanac.Dialogues.Add("npc.open_items", "npc.open_creatures", "npc.open_pieces");
        aboutAlmanac.Action = new DialogueAction
        {
            Type = Command.OpenAlmanac,
            Label = "Open Almanac"
        };
        dialogues[aboutAlmanac.UniqueID] = aboutAlmanac;

        // Items section example
        Dialogue openItems = new Dialogue();
        openItems.UniqueID = "npc.open_items";
        openItems.Label = "Show me the item catalog";
        openItems.Text = "The Items section contains every item in the realm. Unknown items appear blacked out until you discover them.";
        openItems.Dialogues.Add("npc.about_almanac", "npc.open_creatures", "npc.open_pieces");
        openItems.Action = new DialogueAction
        {
            Type = Command.OpenItems,
            Label = "Browse Items"
        };
        dialogues[openItems.UniqueID] = openItems;

        // Creatures section example
        Dialogue openCreatures = new Dialogue();
        openCreatures.UniqueID = "npc.open_creatures";
        openCreatures.Label = "Show me the creature catalog";
        openCreatures.Text = "Here you can study all the creatures of this world. Undiscovered creatures remain hidden until encountered.";
        openCreatures.Dialogues.Add("npc.about_almanac", "npc.open_items", "npc.open_pieces");
        openCreatures.Action = new DialogueAction
        {
            Type = Command.OpenCreatures,
            Label = "Browse Creatures"
        };
        dialogues[openCreatures.UniqueID] = openCreatures;

        // Pieces section example
        Dialogue openPieces = new Dialogue();
        openPieces.UniqueID = "npc.open_pieces";
        openPieces.Label = "Show me building pieces";
        openPieces.Text = "The Pieces section catalogs all buildable structures. Unknown pieces appear blacked out until you learn their recipes.";
        openPieces.Dialogues.Add("npc.about_almanac", "npc.open_items", "npc.open_creatures");
        openPieces.Action = new DialogueAction
        {
            Type = Command.OpenPieces,
            Label = "Browse Pieces"
        };
        dialogues[openPieces.UniqueID] = openPieces;

        // Bounty system explanation and access
        Dialogue aboutBounties = new Dialogue();
        aboutBounties.UniqueID = "npc.about_bounties";
        aboutBounties.Label = "What are bounties?";
        aboutBounties.Text = "Bounties are hunting contracts for dangerous creatures. You must have killed the target at least once to see available bounties for that creature type.";
        aboutBounties.Dialogues.Add("npc.open_bounties", "npc.start_bounty_example");
        aboutBounties.Action = new DialogueAction
        {
            Type = Command.OpenBounties,
            Label = "View Bounty Board"
        };
        dialogues[aboutBounties.UniqueID] = aboutBounties;

        // Direct bounty board access
        Dialogue openBounties = new Dialogue();
        openBounties.UniqueID = "npc.open_bounties";
        openBounties.Label = "Open the bounty ledger";
        openBounties.Text = "Here are all available hunting contracts. Choose wisely, as each bounty requires payment upfront for information.";
        openBounties.Dialogues.Add("npc.about_bounties", "npc.start_bounty_example");
        openBounties.Action = new DialogueAction
        {
            Type = Command.OpenBounties,
            Label = "Browse Bounties"
        };
        dialogues[openBounties.UniqueID] = openBounties;

        // Example bounty start with cost requirement
        Dialogue startBounty = new Dialogue();
        startBounty.UniqueID = "npc.start_bounty_example";
        startBounty.Label = "Accept boar hunting contract";
        startBounty.Text = "I have information about aggressive boars terrorizing the meadows. For 10 coins, I'll mark their location on your map.";
        startBounty.MissingRequirementsText = "You need at least 10 coins to purchase this bounty information.";
        startBounty.Dialogues.Add("npc.about_bounties", "npc.cancel_bounty_example");
        startBounty.Action = new DialogueAction
        {
            Type = Command.StartBounty,
            Label = "Accept Contract (10 coins)",
            Parameters = "Boar.001"
        };
        dialogues[startBounty.UniqueID] = startBounty;

        // Bounty cancellation example
        Dialogue cancelBounty = new Dialogue();
        cancelBounty.UniqueID = "npc.cancel_bounty_example";
        cancelBounty.Label = "Cancel current bounty";
        cancelBounty.Text = "Having second thoughts about your current contract? I can cancel it, but you won't get your payment back.";
        cancelBounty.Action = new DialogueAction
        {
            Type = Command.CancelBounty,
            Label = "Cancel Contract"
        };
        dialogues[cancelBounty.UniqueID] = cancelBounty;

        // Treasure hunting explanation
        Dialogue aboutTreasures = new Dialogue();
        aboutTreasures.UniqueID = "npc.about_treasures";
        aboutTreasures.Label = "Tell me about treasure hunting";
        aboutTreasures.Text = "Treasure hunts lead you to hidden stashes of valuable loot. You must discover a biome before treasure hunts in that area become available.";
        aboutTreasures.Dialogues.Add("npc.open_treasures", "npc.cancel_treasure_example");
        aboutTreasures.Action = new DialogueAction
        {
            Type = Command.OpenTreasures,
            Label = "View Treasure Maps"
        };
        dialogues[aboutTreasures.UniqueID] = aboutTreasures;

        // Direct treasure access
        Dialogue openTreasures = new Dialogue();
        openTreasures.UniqueID = "npc.open_treasures";
        openTreasures.Label = "Show me available treasures";
        openTreasures.Text = "These are all known treasure locations in discovered biomes. Each map comes with clues to help you find the stash.";
        openTreasures.Action = new DialogueAction
        {
            Type = Command.OpenTreasures,
            Label = "Browse Treasures"
        };
        dialogues[openTreasures.UniqueID] = openTreasures;

        // Treasure cancellation example
        Dialogue cancelTreasure = new Dialogue();
        cancelTreasure.UniqueID = "npc.cancel_treasure_example";
        cancelTreasure.Label = "Abandon current treasure hunt";
        cancelTreasure.Text = "Given up on finding that treasure? I can remove the markers from your map if you wish.";
        cancelTreasure.Action = new DialogueAction
        {
            Type = Command.CancelTreasure,
            Label = "Abandon Hunt"
        };
        dialogues[cancelTreasure.UniqueID] = cancelTreasure;

        // Weapon giving example with requirements and alt text
        Dialogue getWeapon = new Dialogue();
        getWeapon.UniqueID = "npc.get_weapon";
        getWeapon.Label = "I need a weapon";
        getWeapon.Text = "Ah, a warrior in need! Here, take this iron sword. It should serve you well in battle.";
        getWeapon.MissingRequirementsText = "I've already given you a sword, friend. One should be enough to start your journey!";
        getWeapon.Dialogues.Add("npc.weapon_info");
        getWeapon.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Take Sword",
            Parameters = "SwordIron, 1, 2, 0"
        };
        getWeapon.Requirements = new DialogueReqs
        {
            Killed = "Eikthyr, 1", 
            NotDialogues = "npc.get_weapon"
        };
        dialogues[getWeapon.UniqueID] = getWeapon;

        // Follow-up weapon info
        Dialogue weaponInfo = new Dialogue();
        weaponInfo.UniqueID = "npc.weapon_info";
        weaponInfo.Label = "Tell me about this sword";
        weaponInfo.Text = "This iron sword was forged in the old ways. Its quality should help you face the dangers ahead. May it serve you well!";
        weaponInfo.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Thank you"
        };
        dialogues[weaponInfo.UniqueID] = weaponInfo;

        // Item taking example (requiring player to have specific items)
        Dialogue takeItems = new Dialogue();
        takeItems.UniqueID = "npc.take_items";
        takeItems.Label = "I can trade materials";
        takeItems.Text = "I see you have some fine wood there. I could take some off your hands for my crafting projects.";
        takeItems.MissingRequirementsText = "You don't seem to have any fine wood to trade right now.";
        takeItems.CompletedText = "This wood is quite spectacular, thank you.";
        takeItems.Dialogues.Add("npc.wood_reward");
        takeItems.Action = new DialogueAction
        {
            Type = Command.Take,
            Label = "Trade Wood",
            Parameters = "FineWood, 5, 1, 0"
        };
        takeItems.Requirements = new DialogueReqs()
        {
            NotDialogues = "npc.take_items"
        };
        dialogues[takeItems.UniqueID] = takeItems;
        
        // Follow-up dialogue that appears after trading wood
        Dialogue woodReward = new Dialogue();
        woodReward.UniqueID = "npc.wood_reward";
        woodReward.Label = "About my crafting project";
        woodReward.Text = "Excellent wood quality! As promised, here's a special reward for helping with my crafting project.";
        woodReward.MissingRequirementsText = "I'm still working on something special for you. Bring me that fine wood first!";
        woodReward.CompletedText = "A beautiful day to craft, isn't it ?";
        woodReward.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Accept Reward",
            Parameters = "Coins, 50, 1, 0"
        };
        woodReward.Requirements = new DialogueReqs
        {
            Dialogues = "npc.take_items",  // Only available after completing the wood trade
            NotDialogues = "npc.wood_reward"
        };
        dialogues[woodReward.UniqueID] = woodReward;

        // Teleportation example
        Dialogue teleportExample = new Dialogue();
        teleportExample.UniqueID = "npc.teleport_example";
        teleportExample.Label = "Can you transport me?";
        teleportExample.Text = "I know the ancient ways of travel. I can send you to a safe location if you need quick transport.";
        teleportExample.Action = new DialogueAction
        {
            Type = Command.Teleport,
            Label = "Transport me",
            Parameters = "0, 50, 0"  // X, Y, Z coordinates
        };
        dialogues[teleportExample.UniqueID] = teleportExample;

        // Other panel access examples
        Dialogue achievements = new Dialogue();
        achievements.UniqueID = "npc.achievements";
        achievements.Label = "Show my accomplishments";
        achievements.Text = "Let me show you a record of all your great deeds and achievements in this realm.";
        achievements.Action = new DialogueAction
        {
            Type = Command.OpenAchievements,
            Label = "View Achievements"
        };
        dialogues[achievements.UniqueID] = achievements;

        Dialogue store = new Dialogue();
        store.UniqueID = "npc.store";
        store.Label = "Browse the store";
        store.Text = "I have various goods available for Almanac tokens. Take a look at what's available.";
        store.Action = new DialogueAction
        {
            Type = Command.OpenStore,
            Label = "Browse Store"
        };
        dialogues[store.UniqueID] = store;

        Dialogue metrics = new Dialogue();
        metrics.UniqueID = "npc.metrics";
        metrics.Label = "Show my statistics";
        metrics.Text = "Curious about your progress? Here are detailed metrics of your adventures.";
        metrics.Action = new DialogueAction
        {
            Type = Command.OpenMetrics,
            Label = "View Statistics"
        };
        dialogues[metrics.UniqueID] = metrics;

        Dialogue leaderboard = new Dialogue();
        leaderboard.UniqueID = "npc.leaderboard";
        leaderboard.Label = "Show server rankings";
        leaderboard.Text = "See how you compare to other adventurers on this server.";
        leaderboard.Action = new DialogueAction
        {
            Type = Command.OpenLeaderboard,
            Label = "View Rankings"
        };
        dialogues[leaderboard.UniqueID] = leaderboard;

        Dialogue lottery = new Dialogue();
        lottery.UniqueID = "npc.lottery";
        lottery.Label = "Feeling lucky?";
        lottery.Text = "Try your luck with the Almanac lottery! You might win valuable prizes.";
        lottery.Action = new DialogueAction
        {
            Type = Command.OpenLottery,
            Label = "Try Lottery"
        };
        dialogues[lottery.UniqueID] = lottery;

        // Example with complex requirements
        Dialogue advancedDialogue = new Dialogue();
        advancedDialogue.UniqueID = "npc.advanced_example";
        advancedDialogue.Label = "Veteran's conversation";
        advancedDialogue.Text = "I see you're an experienced adventurer. You've proven yourself worthy of this knowledge.";
        advancedDialogue.Requirements = new DialogueReqs
        {
            Killed = "Eikthyr, 1; gd_king, 1",  // Must have killed Eikthyr once and The Elder once
            Keys = "defeated_bonemass",          // Must have defeated Bonemass
            Achievements = "TreesChopped.001"          // Must have specific achievement
        };
        dialogues[advancedDialogue.UniqueID] = advancedDialogue;

        // Example with negative requirements (things player must NOT have done)
        Dialogue beginnerDialogue = new Dialogue();
        beginnerDialogue.UniqueID = "npc.beginner_only";
        beginnerDialogue.Label = "New adventurer guidance";
        beginnerDialogue.Text = "I can see you're new to these lands. Let me give you some starting advice.";
        beginnerDialogue.Requirements = new DialogueReqs
        {
            NotKilled = "Eikthyr",              // Must NOT have killed Eikthyr
            NotKeys = "defeated_eikthyr",       // Must NOT have Eikthyr key
            NotAchievements = "TimeInBase.001"     // Must NOT have experienced achievement
        };
        dialogues[beginnerDialogue.UniqueID] = beginnerDialogue;
        
        // Map pin example for location information
        Dialogue mapPinExample = new Dialogue();
        mapPinExample.UniqueID = "npc.map_pin_example";
        mapPinExample.Label = "Where can I find rare mushrooms?";
        mapPinExample.Text = "Ah, seeking the blue mushrooms of the Black Forest? I know a grove where they grow abundantly. Let me mark it on your map.";
        mapPinExample.Action = new DialogueAction
        {
            Type = Command.MapPin,
            Label = "Mark Location",
            Parameters = "150, 25, -200, Mushroom Grove, 60"  // X, Y, Z coordinates, Label, Duration
        };
        dialogues[mapPinExample.UniqueID] = mapPinExample;
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
                case Command.Teleport or Command.MapPin:
                    if (!Action.TryGetVector(out Vector3 pos, out string label, out float duration)) return;
                    switch (Action.Type)
                    {
                        case Command.Teleport:
                            player.TeleportTo(pos, Quaternion.identity, true);
                            instance.Hide();
                            break;
                        case Command.MapPin:
                            if (!Minimap.instance || !ZNet.instance || !Player.m_localPlayer)
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
            }
        }

        [YamlIgnore] public bool isValid => (Action?.IsValid() ?? true) && (Requirements?.HasRequirements() ?? true);
        [YamlIgnore] public Dialogue? previous;
    }
    
    public enum Command
    {
        None, Exit, Give, Take, Spawn, Teleport, 
        StartQuest, CompleteQuest, CancelQuest,
        StartBounty, CancelBounty, CompleteBounty, 
        StartTreasure, CancelTreasure,
        OpenAlmanac, OpenItems, OpenPieces, OpenCreatures, OpenAchievements, OpenStore, OpenLeaderboard, OpenBounties, OpenTreasures, OpenMetrics, OpenLottery,
        MapPin
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
                Command.Teleport or Command.MapPin => TryGetVector(out _, out _, out _),
                Command.StartBounty => TryGetBounty(out _),
                Command.StartTreasure => TryGetTreasure(out _),
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
            if (!ObjectDB.instance.m_itemByHash.ContainsKey(name.GetStableHashCode())) return false;
            if (parts.Length > 1 && !int.TryParse(parts[1].Trim(), out amount)) amount = 1;
            if (parts.Length > 2 && !int.TryParse(parts[2].Trim(), out quality)) quality = 1;
            if (parts.Length > 3 && !int.TryParse(parts[3].Trim(), out variant)) variant = 0;
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

        public bool HasRequirements() => 
            HasRequiredKills() && HasNotRequiredKills() 
                               && HasKeys() && HasNotKeys() 
                               && HasAchievements() && HasNotAchievements() 
                               && HasDialogues() && HasNotDialogues();

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