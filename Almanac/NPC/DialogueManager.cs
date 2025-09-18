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
            NPC? component = prefab.AddComponent<NPC>();
            component.m_hitEffects = hitEffects;
            prefab.AddComponent<NPCTalk>();
            
            Piece? piece = prefab.AddComponent<Piece>();
            piece.m_icon = SpriteManager.GetSprite("blacksmith");
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
        
        Dialogue flyExample = new Dialogue();
        flyExample.UniqueID = "npc.fly_example";
        flyExample.Label = "Can you transport me?";
        flyExample.Text = "I know the ancient ways of travel. I can send you to a safe location if you need quick transport.";
        flyExample.Action = new DialogueAction
        {
            Type = Command.FlyTo,
            Label = "Transport me",
            Parameters = "100, 50, 0"  // X, Y, Z coordinates
        };
        dialogues[flyExample.UniqueID] = flyExample;

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
        
        // quest examples

        // Main quest hub dialogue
        Dialogue questHub = new Dialogue();
        questHub.UniqueID = "npc.quest_hub";
        questHub.Label = "What tasks do you have available?";
        questHub.Text = "Ah, a willing adventurer! I have several tasks that need capable hands. Each serves a different purpose in maintaining the balance of these lands.";
        questHub.Dialogues.Add("npc.harvest_dandelion_intro", "npc.collect_cores_intro", "npc.learn_sword_intro", "npc.farm_carrots_intro", "npc.hunt_boars_intro");
        questHub.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "I'll consider your offers"
        };
        dialogues[questHub.UniqueID] = questHub;

        // DANDELION HARVEST QUEST
        #region Harvest Quest
        Dialogue harvestDandelionIntro = new Dialogue();
        harvestDandelionIntro.UniqueID = "npc.harvest_dandelion_intro";
        harvestDandelionIntro.Label = "Tell me about gathering weeds";
        harvestDandelionIntro.Text = "The meadows are overrun with dandelions! While some see them as mere weeds, I know their true value. These golden flowers have medicinal properties and can be used in brewing. I need 50 of them collected.";
        harvestDandelionIntro.Dialogues.Add("npc.harvest_dandelion_accept", "npc.harvest_dandelion_info", "npc.quest_hub");
        harvestDandelionIntro.Requirements = new()
        {
            NotCompletedQuests = "001.Dandelion"
        };
        harvestDandelionIntro.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionIntro.UniqueID] = harvestDandelionIntro;

        Dialogue harvestDandelionInfo = new Dialogue();
        harvestDandelionInfo.UniqueID = "npc.harvest_dandelion_info";
        harvestDandelionInfo.Label = "Where can I find dandelions?";
        harvestDandelionInfo.Text = "Dandelions grow abundantly in the meadows, particularly in open grassy areas. Look for the bright yellow flowers - they're hard to miss once you know what to seek. Simply walk up and pick them by hand.";
        harvestDandelionInfo.Dialogues.Add("npc.harvest_dandelion_accept", "npc.harvest_dandelion_intro");
        harvestDandelionInfo.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionInfo.UniqueID] = harvestDandelionInfo;

        Dialogue harvestDandelionAccept = new Dialogue();
        harvestDandelionAccept.UniqueID = "npc.harvest_dandelion_accept";
        harvestDandelionAccept.Label = "I'll gather the dandelions";
        harvestDandelionAccept.Text = "Excellent! Remember, I need 50 dandelions total. Take your time and be thorough - quality gathering takes patience.";
        harvestDandelionAccept.CompletedText = "How goes the dandelion gathering? The meadows can be quite peaceful for such work.";
        harvestDandelionAccept.Dialogues.Add("npc.harvest_dandelion_cancel", "npc.harvest_dandelion_complete");
        harvestDandelionAccept.Action = new DialogueAction
        {
            Type = Command.StartQuest,
            Label = "Accept Task",
            Parameters = "001.Dandelion, npc.harvest_dandelion_started"
        };
        harvestDandelionAccept.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionAccept.UniqueID] = harvestDandelionAccept;

        Dialogue harvestDandelionStarted = new Dialogue();
        harvestDandelionStarted.UniqueID = "npc.harvest_dandelion_started";
        harvestDandelionStarted.Text = "May the meadows be kind to you. Return when you've gathered all 50 dandelions.";
        harvestDandelionStarted.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "I'll return soon"
        };
        dialogues[harvestDandelionStarted.UniqueID] = harvestDandelionStarted;

        Dialogue harvestDandelionCancel = new Dialogue();
        harvestDandelionCancel.UniqueID = "npc.harvest_dandelion_cancel";
        harvestDandelionCancel.Label = "I want to abandon this task";
        harvestDandelionCancel.Text = "Having second thoughts about the dandelion gathering? No shame in it - not every task suits every person. Shall I remove this from your obligations?";
        harvestDandelionCancel.Action = new DialogueAction
        {
            Type = Command.CancelQuest,
            Label = "Yes, cancel it",
            Parameters = "001.Dandelion, npc.harvest_dandelion_cancelled"
        };
        harvestDandelionCancel.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionCancel.UniqueID] = harvestDandelionCancel;

        Dialogue harvestDandelionCancelled = new Dialogue();
        harvestDandelionCancelled.UniqueID = "npc.harvest_dandelion_cancelled";
        harvestDandelionCancelled.Text = "Very well, the task is lifted from your shoulders. Should you change your mind later, you know where to find me.";
        harvestDandelionCancelled.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Until next time"
        };
        harvestDandelionCancelled.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionCancelled.UniqueID] = harvestDandelionCancelled;

        Dialogue harvestDandelionComplete = new Dialogue();
        harvestDandelionComplete.UniqueID = "npc.harvest_dandelion_complete";
        harvestDandelionComplete.Label = "I've gathered all the dandelions";
        harvestDandelionComplete.Text = "Wonderful! Let me see... yes, these are perfect specimens. Your dedication to this task shows true character.";
        harvestDandelionComplete.MissingRequirementsText = "I can see you've made progress, but you haven't quite reached 50 dandelions yet. Keep at it!";
        harvestDandelionComplete.Action = new DialogueAction
        {
            Type = Command.CompleteQuest,
            Label = "Complete Task",
            Parameters = "001.Dandelion, npc.harvest_dandelion_reward"
        };
        harvestDandelionComplete.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionComplete.UniqueID] = harvestDandelionComplete;

        Dialogue harvestDandelionReward = new Dialogue();
        harvestDandelionReward.UniqueID = "npc.harvest_dandelion_reward";
        harvestDandelionReward.Text = "Your patience and thoroughness deserve recognition. Accept this blade as payment for your service to the land.";
        harvestDandelionReward.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Accept Payment",
            Parameters = "SwordIron, 1, 2, 0"
        };
        harvestDandelionReward.DialogueChain = "HarvestDandelion";
        dialogues[harvestDandelionReward.UniqueID] = harvestDandelionReward;
        #endregion
        // SURTLING CORES COLLECTION QUEST
        #region Collect Quest
        Dialogue collectCoresIntro = new Dialogue();
        collectCoresIntro.UniqueID = "npc.collect_cores_intro";
        collectCoresIntro.Label = "What about collecting items?";
        collectCoresIntro.Text = "The burial chambers and fire creatures hold treasures of immense value - Surtling Cores. These burning stones are essential for advanced smelting operations. I need 10 cores for my experiments.";
        collectCoresIntro.Dialogues.Add("npc.collect_cores_accept", "npc.collect_cores_info", "npc.quest_hub");
        collectCoresIntro.Requirements = new()
        {
            NotCompletedQuests = "001.SurtlingCores"
        };
        collectCoresIntro.DialogueChain = "CollectCores";
        dialogues[collectCoresIntro.UniqueID] = collectCoresIntro;

        Dialogue collectCoresInfo = new Dialogue();
        collectCoresInfo.UniqueID = "npc.collect_cores_info";
        collectCoresInfo.Label = "Where do I find Surtling Cores?";
        collectCoresInfo.Text = "Surtling Cores can be found in burial chambers scattered throughout the Black Forest, or dropped by Surtlings in the Ashlands. The chambers are safer but require exploration, while Surtlings are dangerous but drop cores directly.";
        collectCoresInfo.Dialogues.Add("npc.collect_cores_accept", "npc.collect_cores_intro");
        collectCoresInfo.DialogueChain = "CollectCores";
        dialogues[collectCoresInfo.UniqueID] = collectCoresInfo;

        Dialogue collectCoresAccept = new Dialogue();
        collectCoresAccept.UniqueID = "npc.collect_cores_accept";
        collectCoresAccept.Label = "I'll gather the cores";
        collectCoresAccept.Text = "Brave soul! Ten Surtling Cores should suffice for my needs. Be cautious in the burial chambers - the dead do not appreciate visitors.";
        collectCoresAccept.CompletedText = "Any progress on those Surtling Cores? The chambers can be treacherous, so take your time.";
        collectCoresAccept.Dialogues.Add("npc.collect_cores_cancel", "npc.collect_cores_complete");
        collectCoresAccept.Action = new DialogueAction
        {
            Type = Command.StartQuest,
            Label = "Accept Task",
            Parameters = "001.SurtlingCores, npc.collect_cores_started"
        };
        collectCoresAccept.DialogueChain = "CollectCores";
        dialogues[collectCoresAccept.UniqueID] = collectCoresAccept;

        Dialogue collectCoresStarted = new Dialogue();
        collectCoresStarted.UniqueID = "npc.collect_cores_started";
        collectCoresStarted.Text = "May your torch burn bright in the darkness. Return when you have the 10 cores I need.";
        collectCoresStarted.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Into the chambers"
        };
        collectCoresStarted.DialogueChain = "CollectCores";
        dialogues[collectCoresStarted.UniqueID] = collectCoresStarted;

        Dialogue collectCoresCancel = new Dialogue();
        collectCoresCancel.UniqueID = "npc.collect_cores_cancel";
        collectCoresCancel.Label = "This task is too dangerous";
        collectCoresCancel.Text = "The burial chambers have claimed many brave souls. There's no shame in recognizing when a task exceeds your current abilities. Shall I release you from this obligation?";
        collectCoresCancel.Action = new DialogueAction
        {
            Type = Command.CancelQuest,
            Label = "Yes, it's too risky",
            Parameters = "001.SurtlingCores, npc.collect_cores_cancelled"
        };
        collectCoresCancel.DialogueChain = "CollectCores";
        dialogues[collectCoresCancel.UniqueID] = collectCoresCancel;

        Dialogue collectCoresCancelled = new Dialogue();
        collectCoresCancelled.UniqueID = "npc.collect_cores_cancelled";
        collectCoresCancelled.Text = "Wisdom knows when to retreat. Perhaps when you're better equipped and more experienced, you'll return to this task.";
        collectCoresCancelled.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Perhaps another time"
        };
        collectCoresCancelled.DialogueChain = "CollectCores";
        dialogues[collectCoresCancelled.UniqueID] = collectCoresCancelled;

        Dialogue collectCoresComplete = new Dialogue();
        collectCoresComplete.UniqueID = "npc.collect_cores_complete";
        collectCoresComplete.Label = "I have the Surtling Cores";
        collectCoresComplete.Text = "Impressive! These cores still pulse with inner fire. You've risked much to obtain them, and that courage will not go unrewarded.";
        collectCoresComplete.MissingRequirementsText = "I can see some cores in your possession, but I need the full 10 for my experiments to work properly.";
        collectCoresComplete.Action = new DialogueAction
        {
            Type = Command.CompleteQuest,
            Label = "Complete Task",
            Parameters = "001.SurtlingCores, npc.collect_cores_reward"
        };
        collectCoresComplete.DialogueChain = "CollectCores";
        dialogues[collectCoresComplete.UniqueID] = collectCoresComplete;

        Dialogue collectCoresReward = new Dialogue();
        collectCoresReward.UniqueID = "npc.collect_cores_reward";
        collectCoresReward.Text = "Your bravery in the depths has earned you this reward. Gold speaks to the value of what you've accomplished.";
        collectCoresReward.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Accept Gold",
            Parameters = "Coins, 100, 1, 0"
        };
        collectCoresReward.DialogueChain = "CollectCores";
        dialogues[collectCoresReward.UniqueID] = collectCoresReward;
        #endregion
        // LEARN ITEMS QUEST (Wooden Sword components)
        #region Learn Quest
        Dialogue learnSwordIntro = new Dialogue();
        learnSwordIntro.UniqueID = "npc.learn_sword_intro";
        learnSwordIntro.Label = "What about learning crafting?";
        learnSwordIntro.Text = "Knowledge is the foundation of all crafting. To forge a wooden sword, one must first understand the materials. I challenge you to discover and learn about Wood, Fine Wood, and Round Log - the trinity of wooden weaponcraft.";
        learnSwordIntro.Dialogues.Add("npc.learn_sword_accept", "npc.learn_sword_info", "npc.quest_hub");
        learnSwordIntro.Requirements = new()
        {
            NotCompletedQuests = "001.WoodenSword"
        };
        learnSwordIntro.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordIntro.UniqueID] = learnSwordIntro;

        Dialogue learnSwordInfo = new Dialogue();
        learnSwordInfo.UniqueID = "npc.learn_sword_info";
        learnSwordInfo.Label = "How do I learn about materials?";
        learnSwordInfo.Text = "Materials are learned by discovering them in the world. Chop trees for Wood, find birch trees for Fine Wood, and look for Round Logs in various locations. Once you've handled each material, your knowledge grows.";
        learnSwordInfo.Dialogues.Add("npc.learn_sword_accept", "npc.learn_sword_intro");
        learnSwordInfo.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordInfo.UniqueID] = learnSwordInfo;

        Dialogue learnSwordAccept = new Dialogue();
        learnSwordAccept.UniqueID = "npc.learn_sword_accept";
        learnSwordAccept.Label = "I'll learn these materials";
        learnSwordAccept.Text = "Excellent! True crafters must understand their materials intimately. Discover Wood, Fine Wood, and Round Log. Each has its own properties and uses in the art of weapon-making.";
        learnSwordAccept.CompletedText = "How goes your material studies? Understanding the fundamentals takes time and exploration.";
        learnSwordAccept.Dialogues.Add("npc.learn_sword_cancel", "npc.learn_sword_complete");
        learnSwordAccept.Action = new DialogueAction
        {
            Type = Command.StartQuest,
            Label = "Accept Studies",
            Parameters = "001.WoodenSword, npc.learn_sword_started"
        };
        learnSwordAccept.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordAccept.UniqueID] = learnSwordAccept;

        Dialogue learnSwordStarted = new Dialogue();
        learnSwordStarted.UniqueID = "npc.learn_sword_started";
        learnSwordStarted.Text = "Knowledge comes to those who seek it actively. Go forth and discover what the forests have to teach.";
        learnSwordStarted.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "To the woods!"
        };
        learnSwordStarted.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordStarted.UniqueID] = learnSwordStarted;

        Dialogue learnSwordCancel = new Dialogue();
        learnSwordCancel.UniqueID = "npc.learn_sword_cancel";
        learnSwordCancel.Label = "I'm not interested in studying";
        learnSwordCancel.Text = "Not everyone appreciates the scholarly approach to crafting. Some prefer to learn by doing rather than systematic study. Shall I end your material research?";
        learnSwordCancel.Action = new DialogueAction
        {
            Type = Command.CancelQuest,
            Label = "Yes, cancel studies",
            Parameters = "001.WoodenSword, npc.learn_sword_cancelled"
        };
        learnSwordCancel.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordCancel.UniqueID] = learnSwordCancel;

        Dialogue learnSwordCancelled = new Dialogue();
        learnSwordCancelled.UniqueID = "npc.learn_sword_cancelled";
        learnSwordCancelled.Text = "Very well. Perhaps hands-on experience will teach you what systematic study could not.";
        learnSwordCancelled.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "I'll learn my own way"
        };
        dialogues[learnSwordCancelled.UniqueID] = learnSwordCancelled;

        Dialogue learnSwordComplete = new Dialogue();
        learnSwordComplete.UniqueID = "npc.learn_sword_complete";
        learnSwordComplete.Label = "I've learned about all the materials";
        learnSwordComplete.Text = "Wonderful! I can see the understanding in your eyes. You now know the properties of Wood, Fine Wood, and Round Log. This knowledge will serve you well in future crafting endeavors.";
        learnSwordComplete.MissingRequirementsText = "You've made progress in your studies, but there are still materials you haven't fully discovered. Continue exploring!";
        learnSwordComplete.Action = new DialogueAction
        {
            Type = Command.CompleteQuest,
            Label = "Complete Studies",
            Parameters = "001.WoodenSword, npc.learn_sword_reward"
        };
        learnSwordComplete.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordComplete.UniqueID] = learnSwordComplete;

        Dialogue learnSwordReward = new Dialogue();
        learnSwordReward.UniqueID = "npc.learn_sword_reward";
        learnSwordReward.Text = "Knowledge itself is the greatest reward, but dedication deserves recognition. Accept these coins as payment for your scholarly efforts.";
        learnSwordReward.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Accept Payment",
            Parameters = "Coins, 75, 1, 0"
        };
        learnSwordReward.DialogueChain = "LearnWoodSword";
        dialogues[learnSwordReward.UniqueID] = learnSwordReward;
        #endregion
        // FARMING QUEST (Carrots)
        #region Farming Quest
        Dialogue farmCarrotsIntro = new Dialogue();
        farmCarrotsIntro.UniqueID = "npc.farm_carrots_intro";
        farmCarrotsIntro.Label = "Do you have farming work?";
        farmCarrotsIntro.Text = "The art of cultivation is often overlooked by adventurers, yet it feeds civilizations. I need someone to plant and tend 10 carrot seedlings. It's honest work that connects you to the land itself.";
        farmCarrotsIntro.Dialogues.Add("npc.farm_carrots_accept", "npc.farm_carrots_info", "npc.quest_hub");
        farmCarrotsIntro.Requirements = new()
        {
            NotCompletedQuests = "001.Farm"
        };
        farmCarrotsIntro.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsIntro.UniqueID] = farmCarrotsIntro;

        Dialogue farmCarrotsInfo = new Dialogue();
        farmCarrotsInfo.UniqueID = "npc.farm_carrots_info";
        farmCarrotsInfo.Label = "How do I farm carrots?";
        farmCarrotsInfo.Text = "Find carrot seeds in the Black Forest, then use a cultivator to prepare soil. Plant the seeds and tend them with care. Each seed will grow into a mature carrot plant that can be harvested for food.";
        farmCarrotsInfo.Dialogues.Add("npc.farm_carrots_accept", "npc.farm_carrots_intro");
        farmCarrotsInfo.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsInfo.UniqueID] = farmCarrotsInfo;

        Dialogue farmCarrotsAccept = new Dialogue();
        farmCarrotsAccept.UniqueID = "npc.farm_carrots_accept";
        farmCarrotsAccept.Label = "I'll tend the crops";
        farmCarrotsAccept.Text = "Splendid! Plant 10 carrot seedlings and watch them grow. There's something deeply satisfying about nurturing life from soil and seed. Take your time - good farming cannot be rushed.";
        farmCarrotsAccept.CompletedText = "How are your crops growing? Farming teaches patience as much as it provides sustenance.";
        farmCarrotsAccept.Dialogues.Add("npc.farm_carrots_cancel", "npc.farm_carrots_complete");
        farmCarrotsAccept.DialogueChain = "FarmCarrotSeeds";
        farmCarrotsAccept.Action = new DialogueAction
        {
            Type = Command.StartQuest,
            Label = "Accept Farming",
            Parameters = "001.Farm, npc.farm_carrots_started"
        };
        dialogues[farmCarrotsAccept.UniqueID] = farmCarrotsAccept;

        Dialogue farmCarrotsStarted = new Dialogue();
        farmCarrotsStarted.UniqueID = "npc.farm_carrots_started";
        farmCarrotsStarted.Text = "May your crops grow strong and your harvest be bountiful. The earth rewards those who work with patience and care.";
        farmCarrotsStarted.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "To the fields"
        };
        farmCarrotsStarted.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsStarted.UniqueID] = farmCarrotsStarted;

        Dialogue farmCarrotsCancel = new Dialogue();
        farmCarrotsCancel.UniqueID = "npc.farm_carrots_cancel";
        farmCarrotsCancel.Label = "Farming isn't for me";
        farmCarrotsCancel.Text = "Not everyone has the temperament for cultivation. It requires patience and dedication that some find... tedious. Shall I release you from this agricultural obligation?";
        farmCarrotsCancel.Action = new DialogueAction
        {
            Type = Command.CancelQuest,
            Label = "Yes, I prefer adventure",
            Parameters = "001.Farm, npc.farm_carrots_cancelled"
        };
        farmCarrotsCancel.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsCancel.UniqueID] = farmCarrotsCancel;

        Dialogue farmCarrotsCancelled = new Dialogue();
        farmCarrotsCancelled.UniqueID = "npc.farm_carrots_cancelled";
        farmCarrotsCancelled.Text = "The call of adventure runs strong in your blood. Perhaps the wilds suit you better than tilled soil and planted rows.";
        farmCarrotsCancelled.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Adventure calls"
        };
        farmCarrotsCancelled.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsCancelled.UniqueID] = farmCarrotsCancelled;

        Dialogue farmCarrotsComplete = new Dialogue();
        farmCarrotsComplete.UniqueID = "npc.farm_carrots_complete";
        farmCarrotsComplete.Label = "I've planted all the carrots";
        farmCarrotsComplete.Text = "Excellent work! I can see the satisfaction of honest labor in your bearing. You've learned that civilization is built not just on conquest, but on cultivation.";
        farmCarrotsComplete.MissingRequirementsText = "Your farming efforts are admirable, but you haven't quite reached 10 planted seedlings yet. Keep at it!";
        farmCarrotsComplete.Action = new DialogueAction
        {
            Type = Command.CompleteQuest,
            Label = "Complete Farming",
            Parameters = "001.Farm, npc.farm_carrots_reward"
        };
        farmCarrotsComplete.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsComplete.UniqueID] = farmCarrotsComplete;

        Dialogue farmCarrotsReward = new Dialogue();
        farmCarrotsReward.UniqueID = "npc.farm_carrots_reward";
        farmCarrotsReward.Text = "Your dedication to the agricultural arts deserves proper recognition. This bronze axe will serve you well in clearing land for future farming endeavors.";
        farmCarrotsReward.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Accept Tool",
            Parameters = "AxeBronze, 1, 2, 0"
        };
        farmCarrotsReward.DialogueChain = "FarmCarrotSeeds";
        dialogues[farmCarrotsReward.UniqueID] = farmCarrotsReward;
        #endregion
        // HUNTING QUEST (Boars)
        #region Kill Quest
        Dialogue huntBoarsIntro = new Dialogue();
        huntBoarsIntro.UniqueID = "npc.hunt_boars_intro";
        huntBoarsIntro.Label = "Any hunting contracts?";
        huntBoarsIntro.Text = "The boars have grown too numerous and bold in the meadows. Their aggressive nature threatens travelers and disrupts the natural balance. I need a hunter to cull 10 of these beasts.";
        huntBoarsIntro.Dialogues.Add("npc.hunt_boars_accept", "npc.hunt_boars_info", "npc.quest_hub");
        huntBoarsIntro.Requirements = new()
        {
            NotCompletedQuests = "001.Kill"
        };
        huntBoarsIntro.DialogueChain = "KillBoars";
        dialogues[huntBoarsIntro.UniqueID] = huntBoarsIntro;

        Dialogue huntBoarsInfo = new Dialogue();
        huntBoarsInfo.UniqueID = "npc.hunt_boars_info";
        huntBoarsInfo.Label = "Tell me about hunting boars";
        huntBoarsInfo.Text = "Boars are found throughout the meadows, often near berry bushes and trees. They're aggressive when approached but not particularly dangerous to a prepared warrior. A bow works well, but any weapon will suffice.";
        huntBoarsInfo.Dialogues.Add("npc.hunt_boars_accept", "npc.hunt_boars_intro");
        huntBoarsInfo.DialogueChain = "KillBoars";
        dialogues[huntBoarsInfo.UniqueID] = huntBoarsInfo;

        Dialogue huntBoarsAccept = new Dialogue();
        huntBoarsAccept.UniqueID = "npc.hunt_boars_accept";
        huntBoarsAccept.Label = "I'll hunt the boars";
        huntBoarsAccept.Text = "A necessary task, though not a pleasant one. Ten boars should restore balance to the meadows. Remember, this is culling for the greater good, not sport hunting.";
        huntBoarsAccept.CompletedText = "How goes the hunt? The meadows will be safer once this task is complete.";
        huntBoarsAccept.Dialogues.Add("npc.hunt_boars_cancel", "npc.hunt_boars_complete");
        huntBoarsAccept.Action = new DialogueAction
        {
            Type = Command.StartQuest,
            Label = "Accept Hunt",
            Parameters = "001.Kill, npc.hunt_boars_started"
        };
        huntBoarsAccept.DialogueChain = "KillBoars";
        dialogues[huntBoarsAccept.UniqueID] = huntBoarsAccept;

        Dialogue huntBoarsStarted = new Dialogue();
        huntBoarsStarted.UniqueID = "npc.hunt_boars_started";
        huntBoarsStarted.Text = "Hunt with purpose, not pleasure. The meadows need their balance restored, and you are the instrument of that restoration.";
        huntBoarsStarted.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "To the hunt"
        };
        huntBoarsStarted.DialogueChain = "KillBoars";
        dialogues[huntBoarsStarted.UniqueID] = huntBoarsStarted;

        Dialogue huntBoarsCancel = new Dialogue();
        huntBoarsCancel.UniqueID = "npc.hunt_boars_cancel";
        huntBoarsCancel.Label = "I cannot hunt these creatures";
        huntBoarsCancel.Text = "Some hearts are not made for culling, even when necessary. There's honor in recognizing your limitations. Shall I find another way to address the boar problem?";
        huntBoarsCancel.Action = new DialogueAction
        {
            Type = Command.CancelQuest,
            Label = "Yes, release me from this",
            Parameters = "001.Kill, npc.hunt_boars_cancelled"
        };
        huntBoarsCancel.DialogueChain = "KillBoars";
        dialogues[huntBoarsCancel.UniqueID] = huntBoarsCancel;

        Dialogue huntBoarsCancelled = new Dialogue();
        huntBoarsCancelled.UniqueID = "npc.hunt_boars_cancelled";
        huntBoarsCancelled.Text = "Your compassion is admirable, even if it complicates practical matters. Perhaps another solution will present itself in time.";
        huntBoarsCancelled.Action = new DialogueAction
        {
            Type = Command.Exit,
            Label = "Thank you for understanding"
        };
        huntBoarsCancelled.DialogueChain = "KillBoars";
        dialogues[huntBoarsCancelled.UniqueID] = huntBoarsCancelled;

        Dialogue huntBoarsComplete = new Dialogue();
        huntBoarsComplete.UniqueID = "npc.hunt_boars_complete";
        huntBoarsComplete.Label = "The boar culling is finished";
        huntBoarsComplete.Text = "You've done what needed doing, though I see it weighs on you. The meadows are safer now, and travelers can pass without fear. This was necessary work.";
        huntBoarsComplete.MissingRequirementsText = "You've made progress in the hunt, but the full cull of 10 boars hasn't been completed yet.";
        huntBoarsComplete.Action = new DialogueAction
        {
            Type = Command.CompleteQuest,
            Label = "Complete Hunt",
            Parameters = "001.Kill, npc.hunt_boars_reward"
        };
        huntBoarsComplete.DialogueChain = "KillBoars";
        dialogues[huntBoarsComplete.UniqueID] = huntBoarsComplete;

        Dialogue huntBoarsReward = new Dialogue();
        huntBoarsReward.UniqueID = "npc.hunt_boars_reward";
        huntBoarsReward.Text = "Difficult tasks deserve worthy compensation. This fine bronze sword will serve you well in future endeavors - may you need it only for protection.";
        huntBoarsReward.Action = new DialogueAction
        {
            Type = Command.Give,
            Label = "Accept Weapon",
            Parameters = "SwordBronze, 1, 2, 0"
        };
        huntBoarsReward.DialogueChain = "KillBoars";
        dialogues[huntBoarsReward.UniqueID] = huntBoarsReward;
        #endregion
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