using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Almanac.Data;
using Almanac.Managers;
using Almanac.Store;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Achievements;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
public static class ZNet_RegisterAchievements
{
    [UsedImplicitly]
    private static void Postfix() => AchievementManager.UpdateServerAchievements();
}

public static class AchievementManager
{
    public const string ACHIEVEMENT_KEY = "Almanac_Collected_Achievements";
    public static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();
    public static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    private static readonly CustomSyncedValue<string> SyncedServerAchievements = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Achievements", "");
    public static readonly Dictionary<string, Achievement> achievements = new();
    private static readonly Dictionary<string, Achievement> fileAchievements = new();
    public static readonly AlmanacDir AchievementDir = new(AlmanacPlugin.AlmanacDir.Path, "Achievements");
    public static void Setup()
    {
        LoadDefaults();
        string[] files = AchievementDir.GetFiles("*.yml", true);
        if (files.Length == 0)
        {
            foreach (Achievement achievement in achievements.Values)
            {
                string data = serializer.Serialize(achievement);
                string fileName = achievement.Name + ".yml";
                string path = AchievementDir.WriteFile(fileName, data);
                File.WriteAllText(path, data);
                fileAchievements[path] = achievement;
                achievement.Path = path;
            }
        }
        else
        {
            achievements.Clear();
            foreach (string file in files)
            {
                try
                {
                    string data = File.ReadAllText(file);
                    Achievement achievement = deserializer.Deserialize<Achievement>(data);
                    achievements[achievement.UniqueID] = achievement;
                    fileAchievements[file] = achievement;
                    achievement.Path = file;
                }
                catch
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse achievement: " + Path.GetFileName(file));
                }
            }            
        }
        SyncedServerAchievements.ValueChanged += OnServerAchievementsChanged;
        FileSystemWatcher watcher = new FileSystemWatcher(AchievementDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
    }
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            Achievement data = deserializer.Deserialize<Achievement>(File.ReadAllText(e.FullPath));
            if (!achievements.TryGetValue(data.UniqueID, out var achievement)) return;
            achievement.CopyFrom(data);
            UpdateServerAchievements();
            
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Achievements].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnAchievementTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change achievement: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            Achievement data = deserializer.Deserialize<Achievement>(File.ReadAllText(e.FullPath));
            achievements[data.UniqueID] = data;
            fileAchievements[e.FullPath] = data;
            UpdateServerAchievements();
            
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Achievements].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnAchievementTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create achievement: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            if (!fileAchievements.TryGetValue(e.FullPath, out Achievement achievement)) return;
            achievements.Remove(achievement.UniqueID);
            UpdateServerAchievements();
            
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Achievements].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnAchievementTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to delete achievement: " + Path.GetFileName(e.FullPath));
        }
    }

    public static void UpdateServerAchievements()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedServerAchievements.Value = serializer.Serialize(achievements);
    }
    private static void OnServerAchievementsChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedServerAchievements.Value)) return;
        try
        {
            Dictionary<string, Achievement> data = deserializer.Deserialize<Dictionary<string, Achievement>>(SyncedServerAchievements.Value);
            achievements.Clear();
            achievements.AddRange(data);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server achievements");
        }
    }
    public static bool Exists(string id) => achievements.ContainsKey(id);
    public static bool TryGetAchievement(string id, out Achievement achievement) => achievements.TryGetValue(id, out achievement);
    private static void LoadDefaults()
    {
        Achievement harvester = new Achievement();
        harvester.UniqueID = "Harvest.001";
        harvester.Name = "Harvester";
        harvester.Lore = "A true survivor knows the benefits of foraging";
        harvester.Icon = "Dandelion";
        harvester.TokenReward = 1;
        harvester.Requirement.Type = AchievementType.ItemsPicked;
        harvester.Requirement.Threshold = 50;
        achievements[harvester.UniqueID] = harvester;
        
        Achievement allFish = new Achievement();
        allFish.UniqueID = "Fisherman.001";
        allFish.Name = "Fisherman";
        allFish.Lore = "To live as a Viking is to honor the sea, for it feeds the clan and tests the soul";
        allFish.Icon = "fish";
        allFish.TokenReward = 100;
        allFish.Requirement.Type = AchievementType.Fish;
        achievements[allFish.UniqueID] = allFish;
        
        Achievement allMaterials = new  Achievement();
        allMaterials.UniqueID = "Materials.001";
        allMaterials.Name = "Collector";
        allMaterials.Lore = "To know every stone, root, and ore is to know the very bones of the world.";
        allMaterials.Icon = "gem";
        allMaterials.TokenReward = 100;
        allMaterials.Requirement.Type = AchievementType.Materials;
        achievements[allMaterials.UniqueID] = allMaterials;
        
        Achievement allConsumables = new   Achievement();
        allConsumables.UniqueID = "Consumables.001";
        allConsumables.Name = "Glutton";
        allConsumables.Lore = "A Viking’s might is forged as much at the table as on the battlefield";
        allConsumables.Icon = "CookedMeat";
        allConsumables.TokenReward = 100;
        allConsumables.Requirement.Type = AchievementType.Consumables;
        achievements[allConsumables.UniqueID] = allConsumables;

        Achievement allWeapons = new Achievement();
        allWeapons.UniqueID = "Weapons.001";
        allWeapons.Name = "Warlord";
        allWeapons.Lore = "From axe to bow, from spear to sword — none escape your mastery.";
        allWeapons.Icon = "sword_blue";
        allWeapons.TokenReward = 100;
        allWeapons.Requirement.Type = AchievementType.Weapons;
        achievements[allWeapons.UniqueID] = allWeapons;
        
        Achievement swords = new  Achievement();
        swords.UniqueID = "Swords.001";
        swords.Name = "Swordsmith";
        swords.Lore = "To forge and wield the sword is to shape the fate of warriors.";
        swords.Icon = "SwordIron";
        swords.TokenReward = 50;
        swords.Requirement.Type = AchievementType.Swords;
        achievements[swords.UniqueID] = swords;

        Achievement axes = new Achievement();
        axes.UniqueID = "Axes.001";
        axes.Name = "Axesmith";
        axes.Lore = "Wood or bone, timber or shield — all yield to your swing.";
        axes.Icon = "AxeIron";
        axes.TokenReward = 50;
        axes.Requirement.Type = AchievementType.Axes;
        achievements[axes.UniqueID] = axes;
        
        Achievement polearms = new Achievement();
        polearms.UniqueID = "PoleArms.001";
        polearms.Name = "Polearm Master";
        polearms.Lore = "Long reach, longer legacy — no foe escapes your strike.";
        polearms.Icon = "AtgeirIron";
        polearms.TokenReward = 50;
        polearms.Requirement.Type = AchievementType.PoleArms;
        achievements[polearms.UniqueID] = polearms;

        Achievement spears = new Achievement();
        spears.UniqueID = "Spears.001";
        spears.Name = "Spearsmith";
        spears.Lore = "The spear is the weapon of gods and mortals alike — swift, certain, deadly.";
        spears.Icon = "SpearBronze";
        spears.TokenReward = 50;
        spears.Requirement.Type = AchievementType.Spears;
        achievements[spears.UniqueID] = spears;
        
        Achievement maces = new Achievement();
        maces.UniqueID = "Maces.001";
        maces.Name = "Boneshatter";
        maces.Lore = "Crushing steel and shattering bone, you wield ruin with every blow.";
        maces.Icon = "MaceIron";
        maces.TokenReward = 50;
        maces.Requirement.Type = AchievementType.Maces;
        achievements[maces.UniqueID] = maces;
        
        Achievement knives = new Achievement();
        knives.UniqueID = "Knives.001";
        knives.Name = "Blade in the Dark";
        knives.Lore = "Swift, silent, and certain — your knives strike before the foe can breathe.";
        knives.Icon = "KnifeSkollAndHati";
        knives.TokenReward = 50;
        knives.Requirement.Type = AchievementType.Knives;
        achievements[knives.UniqueID] = knives;
        
        Achievement shields = new Achievement();
        shields.UniqueID = "Shields.001";
        shields.Name = "Wall of Iron";
        shields.Lore = "The shield is not just defense — it is the strength of the clan made steel.";
        shields.Icon = "ShieldIronBuckler";
        shields.TokenReward = 50;
        shields.Requirement.Type = AchievementType.Shields;
        achievements[shields.UniqueID] = shields;
        
        Achievement staves = new Achievement();
        staves.UniqueID = "Staves.001";
        staves.Name = "Runestaff Adept";
        staves.Lore = "With staff in hand, you channel the wild power of gods and storms.";
        staves.Icon = "StaffFireball";
        staves.TokenReward = 50;
        staves.Requirement.Type = AchievementType.Staves;
        achievements[staves.UniqueID] = staves;
        
        Achievement arrows = new Achievement();
        arrows.UniqueID = "Arrows.001";
        arrows.Name = "Quiver Keeper";
        arrows.Lore = "A true marksman never runs dry; every arrow counts in the hunt.";
        arrows.Icon = "ArrowWood";
        arrows.TokenReward = 50;
        arrows.Requirement.Type = AchievementType.Arrows;
        achievements[arrows.UniqueID] = arrows;

        Achievement bows = new Achievement();
        bows.UniqueID = "Bows.001";
        bows.Name = "Bowmaster";
        bows.Lore = "From distance you command the battlefield; every pull of the string is precision incarnate.";
        bows.Icon = "Bow";
        bows.TokenReward = 50;
        bows.Requirement.Type = AchievementType.Bows;
        achievements[bows.UniqueID] = bows;

        Achievement valuables = new Achievement();
        valuables.UniqueID = "Valuables.001";
        valuables.Name = "Gold Digger";
        valuables.Lore = "Gold, gems, relics — you know the worth of the world and claim it all.";
        valuables.Icon = "Amber";
        valuables.TokenReward = 50;
        valuables.Requirement.Type = AchievementType.Valuables;
        achievements[valuables.UniqueID] = valuables;

        Achievement potions = new Achievement();
        potions.UniqueID = "Potions.001";
        potions.Name = "Alchemist";
        potions.Lore = "Mix, brew, and imbibe — your potions turn fate in your favor.";
        potions.Icon = "bottle_empty";
        potions.TokenReward = 50;
        potions.Requirement.Type = AchievementType.Potions;
        achievements[potions.UniqueID] = potions;

        Achievement trophies = new Achievement();
        trophies.UniqueID = "Trophies.001";
        trophies.Name = "Glorious";
        trophies.Lore = "Every conquest leaves a mark; your trophies tell the tale of victory.";
        trophies.Icon = "TrophyEikthyr";
        trophies.TokenReward = 50;
        trophies.Requirement.Type = AchievementType.Trophies;
        achievements[trophies.UniqueID] = trophies;

        Achievement kills = new Achievement();
        kills.UniqueID = "Kills.001";
        kills.Name = "Huntsman";
        kills.Lore = "A true Viking leaves no foe standing; your name is feared across the land.";
        kills.Icon = "skull";
        kills.TokenReward = 10;
        kills.Requirement.Type = AchievementType.EnemyKills;
        kills.Requirement.Threshold = 50;
        achievements[kills.UniqueID] = kills;
        
        Achievement treesChopped = new Achievement();
        treesChopped.UniqueID = "TreesChopped.001";
        treesChopped.Name = "Woodcutter";
        treesChopped.Lore = "Axes sing and trees fall — your labor shapes the land itself.";
        treesChopped.Icon = "log";
        treesChopped.TokenReward = 10;
        treesChopped.Requirement.Type = AchievementType.TreesChopped;
        treesChopped.Requirement.Threshold = 50;
        achievements[treesChopped.UniqueID] = treesChopped;

        Achievement timeInBase = new Achievement();
        timeInBase.UniqueID = "TimeInBase.001";
        timeInBase.Name = "Hearthbound";
        timeInBase.Lore = "A wise Viking knows when to rest, plan, and fortify within the safety of home.";
        timeInBase.Icon = "book";
        timeInBase.TokenReward = 5;
        timeInBase.Requirement.Type = AchievementType.TimeInBase;
        timeInBase.Requirement.Threshold = 3600; // Example: 1 hour
        achievements[timeInBase.UniqueID] = timeInBase;

        Achievement timeOutOfBase = new Achievement();
        timeOutOfBase.UniqueID = "TimeOutOfBase.001";
        timeOutOfBase.Name = "Wayfarer";
        timeOutOfBase.Lore = "The world is vast, and your journey takes you to its farthest corners.";
        timeOutOfBase.Icon = "hood";
        timeOutOfBase.TokenReward = 10;
        timeOutOfBase.Requirement.Type = AchievementType.TimeOutOfBase;
        timeOutOfBase.Requirement.Threshold = 3600; // Example: 1 hour
        achievements[timeOutOfBase.UniqueID] = timeOutOfBase;

        Achievement arrowsShot = new Achievement();
        arrowsShot.UniqueID = "ArrowsShot.001";
        arrowsShot.Name = "Sharpshooter";
        arrowsShot.Lore = "Each arrow released is a testament to your skill and precision.";
        arrowsShot.Icon = "arrow";
        arrowsShot.TokenReward = 5;
        arrowsShot.Requirement.Type = AchievementType.ArrowsShot;
        arrowsShot.Requirement.Threshold = 100;
        achievements[arrowsShot.UniqueID] = arrowsShot;
        
        Achievement totalJumps = new Achievement();
        totalJumps.UniqueID = "TotalJumps.001";
        totalJumps.Name = "Leaper";
        totalJumps.Lore = "Every leap shows your daring spirit and agility.";
        totalJumps.Icon = "mushroom";
        totalJumps.TokenReward = 10;
        totalJumps.Requirement.Type = AchievementType.TotalJumps;
        totalJumps.Requirement.Threshold = 100;
        achievements[totalJumps.UniqueID] = totalJumps;

        Achievement playerKills = new Achievement();
        playerKills.UniqueID = "PlayerKills.001";
        playerKills.Name = "Duelist";
        playerKills.Lore = "Your skill is unmatched in battle — fellow Vikings fall before you.";
        playerKills.Icon = "skull";
        playerKills.TokenReward = 50;
        playerKills.Requirement.Type = AchievementType.PlayerKills;
        playerKills.Requirement.Threshold = 10;
        achievements[playerKills.UniqueID] = playerKills;

        Achievement distanceWalked = new Achievement();
        distanceWalked.UniqueID = "DistanceWalked.001";
        distanceWalked.Name = "Wayfarer";
        distanceWalked.Lore = "Miles tread and paths explored — the world is yours to roam.";
        distanceWalked.Icon = "map";
        distanceWalked.TokenReward = 10;
        distanceWalked.Requirement.Type = AchievementType.DistanceWalked;
        distanceWalked.Requirement.Threshold = 10000; // Example: 10 km
        achievements[distanceWalked.UniqueID] = distanceWalked;

        Achievement distanceRan = new Achievement();
        distanceRan.UniqueID = "DistanceRan.001";
        distanceRan.Name = "Fleetfoot";
        distanceRan.Lore = "Swift of foot, you outrun foes and chase your destiny across the land.";
        distanceRan.Icon = "map";
        distanceRan.TokenReward = 10;
        distanceRan.Requirement.Type = AchievementType.DistanceRan;
        distanceRan.Requirement.Threshold = 5000; // Example: 5 km
        achievements[distanceRan.UniqueID] = distanceRan;

        Achievement distanceSailed = new Achievement();
        distanceSailed.UniqueID = "DistanceSailed.001";
        distanceSailed.Name = "Seafarer";
        distanceSailed.Lore = "From fjord to ocean, your longships have charted every wave.";
        distanceSailed.Icon = "VikingShip";
        distanceSailed.TokenReward = 15;
        distanceSailed.Requirement.Type = AchievementType.DistanceSailed;
        distanceSailed.Requirement.Threshold = 10000; // Example: 10 km
        achievements[distanceSailed.UniqueID] = distanceSailed;

        Achievement distanceInAir = new Achievement();
        distanceInAir.UniqueID = "DistanceInAir.001";
        distanceInAir.Name = "Skywalker";
        distanceInAir.Lore = "Whether leaping from cliffs or gliding with the wind, you master the skies.";
        distanceInAir.Icon = "TrophyBlob";
        distanceInAir.TokenReward = 15;
        distanceInAir.Requirement.Type = AchievementType.DistanceInAir;
        distanceInAir.Requirement.Threshold = 2000; // Example: 2 km
        achievements[distanceInAir.UniqueID] = distanceInAir;

        Achievement mineHits = new Achievement();
        mineHits.UniqueID = "MineHits.001";
        mineHits.Name = "Miner";
        mineHits.Lore = "Every strike against stone and ore shapes your mastery over the earth.";
        mineHits.Icon = "PickaxeIron";
        mineHits.TokenReward = 10;
        mineHits.Requirement.Type = AchievementType.MineHits;
        mineHits.Requirement.Threshold = 1000;
        achievements[mineHits.UniqueID] = mineHits;

        Achievement totalMined = new Achievement();
        totalMined.UniqueID = "TotalMined.001";
        totalMined.Name = "Earthshaper";
        totalMined.Lore = "From stone to ore, you claim the treasures hidden beneath the land.";
        totalMined.Icon = "CopperOre";
        totalMined.TokenReward = 15;
        totalMined.Requirement.Type = AchievementType.TotalMined;
        totalMined.Requirement.Threshold = 500;
        achievements[totalMined.UniqueID] = totalMined;

        Achievement creatureTamed = new Achievement();
        creatureTamed.UniqueID = "CreatureTamed.001";
        creatureTamed.Name = "Beast Whisperer";
        creatureTamed.Lore = "Wild creatures bow to your will, for few can tame what you command.";
        creatureTamed.Icon = "TrophyBoar";
        creatureTamed.TokenReward = 20;
        creatureTamed.Requirement.Type = AchievementType.CreatureTamed;
        creatureTamed.Requirement.Threshold = 5;
        achievements[creatureTamed.UniqueID] = creatureTamed;

        Achievement foodEaten = new Achievement();
        foodEaten.UniqueID = "FoodEaten.001";
        foodEaten.Name = "Feaster";
        foodEaten.Lore = "A Viking fueled by feasts is unstoppable — strength comes from the belly.";
        foodEaten.Icon = "CookedMeat";
        foodEaten.TokenReward = 10;
        foodEaten.Requirement.Type = AchievementType.FoodEaten;
        foodEaten.Requirement.Threshold = 50;
        achievements[foodEaten.UniqueID] = foodEaten;
        
        Achievement meadows = new Achievement();
        meadows.UniqueID = "Meadows.001";
        meadows.Name = "Meadows I";
        meadows.Lore = "Prove your might by defeating the creatures of the Meadows. Only then will the land respect your strength.";
        meadows.Icon = "TrophyEikthyr";
        meadows.TokenReward = 10;
        meadows.Requirement.Type = AchievementType.CreatureGroup;
        meadows.Requirement.Threshold = 5;
        meadows.Requirement.Group = "Meadows";
        achievements[meadows.UniqueID] = meadows;

        Achievement blackforest = new Achievement();
        blackforest.UniqueID = "Blackforest.001";
        blackforest.Name = "Blackforest I";
        blackforest.Lore = "Venture into the shadows of the Black Forest and defeat its fearsome creatures to earn your renown.";
        blackforest.Icon = "TrophyTheElder";
        blackforest.TokenReward = 20;
        blackforest.Requirement.Type = AchievementType.CreatureGroup;
        blackforest.Requirement.Threshold = 5;
        blackforest.Requirement.Group = "BlackForest";
        achievements[blackforest.UniqueID] = blackforest;

        Achievement swamp = new Achievement();
        swamp.UniqueID = "Swamp.001";
        swamp.Name = "Swamp I";
        swamp.Lore = "Brave the murky depths of the Swamp and defeat the lurking horrors to prove your might.";
        swamp.Icon = "TrophyBonemass";
        swamp.TokenReward = 30;
        swamp.Requirement.Type = AchievementType.CreatureGroup;
        swamp.Requirement.Threshold = 5;
        swamp.Requirement.Group = "Swamp";
        achievements[swamp.UniqueID] = swamp;
        
        Achievement mountains = new Achievement();
        mountains.UniqueID = "Mountains.001";
        mountains.Name = "Mountains I";
        mountains.Lore = "Venture into the high peaks and face the creatures that dwell among the Mountains.";
        mountains.Icon = "TrophyDragonQueen";
        mountains.TokenReward = 40;
        mountains.Requirement.Type = AchievementType.CreatureGroup;
        mountains.Requirement.Threshold = 5;
        mountains.Requirement.Group = "Mountain";
        achievements[mountains.UniqueID] = mountains;
        
        Achievement plains = new Achievement();
        plains.UniqueID = "Plains.001";
        plains.Name = "Plains I";
        plains.Lore = "Brave the open fields and confront the creatures roaming the Plains.";
        plains.Icon = "TrophyGoblinKing";
        plains.TokenReward = 50;
        plains.Requirement.Type = AchievementType.CreatureGroup;
        plains.Requirement.Threshold = 5;
        plains.Requirement.Group = "Plains";
        achievements[plains.UniqueID] = plains;
        
        Achievement mistlands = new Achievement();
        mistlands.UniqueID = "Mistlands.001";
        mistlands.Name = "Mistlands I";
        mistlands.Lore = "Venture into the fog-shrouded Mistlands and face the mysterious creatures within.";
        mistlands.Icon = "TrophySeekerQueen";
        mistlands.TokenReward = 60;
        mistlands.Requirement.Type = AchievementType.CreatureGroup;
        mistlands.Requirement.Threshold = 5;
        mistlands.Requirement.Group = "Mistlands";
        achievements[mistlands.UniqueID] = mistlands;
        
        Achievement ashlands = new Achievement();
        ashlands.UniqueID = "Ashlands.001";
        ashlands.Name = "Ashlands I";
        ashlands.Lore = "The infernal legions rise again and again, yet you’ve cut them down fivefold. The Ashlands themselves bear witness to your relentless fury.";
        ashlands.Icon = "TrophyFader";
        ashlands.TokenReward = 100;
        ashlands.Requirement.Type = AchievementType.CreatureGroup;
        ashlands.Requirement.Threshold = 5;
        ashlands.Requirement.Group = "AshLands";
        achievements[ashlands.UniqueID] = ashlands;
        
        Achievement boars = new Achievement();
        boars.UniqueID = "Boars.001";
        boars.Name = "Boars I";
        boars.Lore = "Through field and forest, you’ve culled the wild herds—fifty boars felled by your hand.";
        boars.Icon = "TrophyBoar";
        boars.TokenReward = 10;
        boars.Requirement.Type = AchievementType.Kill;
        boars.Requirement.Threshold = 200;
        boars.Requirement.PrefabName = "Boar";
        achievements[boars.UniqueID] = boars;
        
        Achievement eikthyr = new Achievement();
        eikthyr.UniqueID = "Eikthyr.001";
        eikthyr.Name = "Eikthyr I";
        eikthyr.Lore = "The great stag’s thunder once shook the skies, yet you have brought it low—twenty-five times over.";
        eikthyr.Icon = "TrophyEikthyr";
        eikthyr.TokenReward = 10;
        eikthyr.Requirement.Type = AchievementType.Kill;
        eikthyr.Requirement.Threshold = 25;
        eikthyr.Requirement.PrefabName = "Eikthyr";
        achievements[eikthyr.UniqueID] = eikthyr;
        
        Achievement trinkets = new Achievement();
        trinkets.UniqueID = "Trinkets.001";
        trinkets.Name = "Trinkets";
        trinkets.Lore = "The powers of the ancients flow through you like a thunderous cloud.";
        trinkets.Icon = "TrinketBronzeStamina";
        trinkets.TokenReward = 100;
        trinkets.Requirement.Type = AchievementType.Trinkets;
        achievements[trinkets.UniqueID] = trinkets;
    }
    
    public static bool IsValidType(string input, out AchievementType type) => Enum.TryParse(input, true, out type);

    [Serializable]
    public class Achievement
    {
        public string UniqueID = string.Empty;
        public string Name = string.Empty;
        public string Lore = string.Empty;
        public string Icon = string.Empty;
        public int TokenReward;
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public AchievementRequirement Requirement = new();
        [YamlIgnore] public Sprite? icon => SpriteManager.GetSprite(Icon);
        [YamlIgnore] public string Path = string.Empty;
        public void CopyFrom(Achievement other)
        {
            UniqueID = other.UniqueID;
            Name = other.Name;
            Lore = other.Lore;
            Icon = other.Icon;
            TokenReward = other.TokenReward;
            Requirement.Type = other.Requirement.Type;
            Requirement.Threshold = other.Requirement.Threshold;
            Requirement.PrefabName = other.Requirement.PrefabName;
            Requirement._sharedName = null;
        }
        public List<Entries.Entry> ToEntries()
        {
            Entries.EntryBuilder builder = new();
            builder.Add(Lore + "\n", "lore");
            builder.Add(Keys.AchievementType, Requirement.Type);
            if (!string.IsNullOrEmpty(Requirement.PrefabName)) builder.Add(Keys.Target, Requirement.GetSharedName());
            builder.Add(Keys.Threshold, $"{Requirement.GetProgress(Player.m_localPlayer)}/{Requirement.GetThreshold()}");
            switch (Requirement.Type)
            {
                case AchievementType.Fish:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.fishes)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Materials:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.materials)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Consumables:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.consumables)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Weapons:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.weapons)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Swords:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.swords)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Axes:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.axes)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.PoleArms:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.polearms)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Spears:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.spears)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Maces:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.clubs)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Knives:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo d in ItemHelper.knives)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(d.shared.m_name);
                        builder.Add(isKnown ? d.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Shields:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.blocking)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Staves:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.staves)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Arrows:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.ammo)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Bows:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.bows)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Valuables:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.valuables)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Potions:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.potions)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Trophies:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.trophies)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.Trinkets:
                    builder.Add(Keys.Require);
                    foreach (ItemHelper.ItemInfo x in ItemHelper.trinkets)
                    {
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(x.shared.m_name);
                        builder.Add(isKnown ? x.shared.m_name : "???", isKnown);
                    }
                    break;
                case AchievementType.CreatureGroup:
                    if (!CreatureGroup.TryGetGroup(Requirement.Group, out List<string> group)) break;
                    builder.Add(Keys.Require);
                    foreach (string prefabName in group)
                    {
                        string? sharedName = CritterHelper.namedCritters.TryGetValue(prefabName, out CritterHelper.CritterInfo info) ? info.character.m_name : prefabName;
                        int progress = PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, sharedName);
                        string formatted = $"{progress}/{Requirement.Threshold}";
                        builder.Add(sharedName, formatted);
                    }
                    break;
            }
            return builder.ToList();
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element item)
        {
            bool isCompleted = this.IsCompleted(Player.m_localPlayer);
            bool isCollected = this.IsCollected(Player.m_localPlayer);
            panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(Name);
            panel.description.SetIcon(icon);
            if (AlmanacPanel.isLocalAdminOrHostAndNoCost)
            {
                AlmanacPanel.InfoView.EditButton edit = panel.description.view.CreateEditButton();
                edit.SetLabel("Edit");
                edit.OnClick(() =>
                {
                    var form = new FormPanel.AchievementForm();
                    form.SetTopic("Edit Achievement");
                    form.SetButtonText("Confirm Edit");
                    form.SetDescription("Edit achievement");
                    form.inEditMode = true;
                    form.overridePath = Path;
                    panel.formBuilder.Setup(form);
                    form.idField.input?.Set(UniqueID);
                    form.nameField.input?.Set(Name);
                    form.loreField.input?.Set(Lore);
                    form.iconField.input?.Set(Icon);
                    form.rewardField.input?.Set(TokenReward.ToString());
                    form.typeField.input?.Set(Requirement.Type.ToString());
                    form.prefabField.input?.Set(Requirement.PrefabName);
                    form.groupField.input?.Set(Requirement.Group);
                    form.thresholdField.input?.Set(Requirement.Threshold.ToString());
                    form.HasChanged = false;
                });
            }
            ToEntries().Build(panel.description.view);
            panel.description.requirements.SetTokens(TokenReward);
            panel.description.view.Resize();
            panel.description.SetButtonText(isCompleted ? isCollected ? Keys.Collected : Keys.CollectReward : Keys.InProgress);
            panel.description.Interactable(isCompleted && !isCollected);
            if (!isCompleted) return;
            panel.OnMainButton = () =>
            {
                Player.m_localPlayer.AddTokens(TokenReward);
                Player.m_localPlayer.SetAchievementCollected(this);
                panel.description.Interactable(false);
                panel.description.SetButtonText(Keys.Collected);
            };
        }
        
        [Serializable]
        public class AchievementRequirement
        {
            public AchievementType Type;
            public string Group = string.Empty;
            public string PrefabName = string.Empty;
            public int Threshold;
            [NonSerialized, YamlIgnore] public string? _sharedName;
            [YamlIgnore] public GameObject? prefab => ZNetScene.instance.GetPrefab(PrefabName);
            public int GetThreshold()
            {
                return Type switch
                {
                    AchievementType.Fish => Threshold > 0 ? Threshold : ItemHelper.fishes.Count,
                    AchievementType.Materials => Threshold > 0 ? Threshold : ItemHelper.materials.Count,
                    AchievementType.Consumables => Threshold > 0 ? Threshold : ItemHelper.consumables.Count,
                    AchievementType.Weapons => Threshold > 0 ? Threshold : ItemHelper.weapons.Count,
                    AchievementType.Swords => Threshold > 0 ? Threshold : ItemHelper.swords.Count,
                    AchievementType.Axes => Threshold > 0 ? Threshold : ItemHelper.axes.Count,
                    AchievementType.PoleArms => Threshold > 0 ? Threshold : ItemHelper.polearms.Count,
                    AchievementType.Spears => Threshold > 0 ? Threshold : ItemHelper.spears.Count,
                    AchievementType.Maces => Threshold > 0 ? Threshold : ItemHelper.clubs.Count,
                    AchievementType.Knives => Threshold > 0 ? Threshold : ItemHelper.knives.Count,
                    AchievementType.Shields => Threshold > 0 ? Threshold : ItemHelper.blocking.Count,
                    AchievementType.Staves => Threshold > 0 ? Threshold : ItemHelper.staves.Count,
                    AchievementType.Arrows => Threshold > 0 ? Threshold : ItemHelper.ammo.Count,
                    AchievementType.Bows => Threshold > 0 ? Threshold : ItemHelper.bows.Count,
                    AchievementType.Valuables => Threshold > 0 ? Threshold : ItemHelper.valuables.Count,
                    AchievementType.Potions => Threshold > 0 ? Threshold : ItemHelper.potions.Count,
                    AchievementType.Trophies => Threshold > 0 ? Threshold : ItemHelper.trophies.Count,
                    AchievementType.Trinkets => Threshold > 0 ? Threshold : ItemHelper.trinkets.Count,
                    AchievementType.Recipes => Threshold > 0 ? Threshold : ItemHelper.recipes.Count,
                    AchievementType.CreatureGroup => CreatureGroup.TryGetGroup(Group, out List<string> group) ? group.Count : 0,
                    _ => Threshold,
                };
            }
            public int GetProgress(Player player)
            {
                return Type switch
                {
                    AchievementType.Deaths => PlayerInfo.GetPlayerStat(PlayerStatType.Deaths).Floor(),
                    AchievementType.Fish => ItemHelper.fishes.Count(f => player.IsKnownMaterial(f.shared.m_name)),
                    AchievementType.Materials => ItemHelper.materials.Count(m => player.IsKnownMaterial(m.shared.m_name)),
                    AchievementType.Consumables => ItemHelper.consumables.Count(c => player.IsKnownMaterial(c.shared.m_name)),
                    AchievementType.Weapons => ItemHelper.weapons.Count(w => player.IsKnownMaterial(w.shared.m_name)),
                    AchievementType.Swords => ItemHelper.swords.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Axes => ItemHelper.axes.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.PoleArms => ItemHelper.polearms.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Spears => ItemHelper.spears.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Maces => ItemHelper.clubs.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Knives => ItemHelper.knives.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Shields => ItemHelper.blocking.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Staves => ItemHelper.staves.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Arrows => ItemHelper.ammo.Count(a => player.IsKnownMaterial(a.shared.m_name)),
                    AchievementType.Bows => ItemHelper.bows.Count(s => player.IsKnownMaterial(s.shared.m_name)),
                    AchievementType.Valuables => ItemHelper.valuables.Count(v => player.IsKnownMaterial(v.shared.m_name)),
                    AchievementType.Potions => ItemHelper.potions.Count(p => player.IsKnownMaterial(p.shared.m_name)),
                    AchievementType.Trophies => ItemHelper.trophies.Count(t => player.IsKnownMaterial(t.shared.m_name)),
                    AchievementType.Trinkets => ItemHelper.trinkets.Count(t => player.IsKnownMaterial(t.shared.m_name)),
                    AchievementType.EnemyKills => PlayerInfo.GetPlayerStat(PlayerStatType.EnemyKills).Floor(),
                    AchievementType.TreesChopped => PlayerInfo.GetPlayerStat(PlayerStatType.Tree).Floor(),
                    AchievementType.TimeInBase => PlayerInfo.GetPlayerStat(PlayerStatType.TimeInBase).Floor(),
                    AchievementType.TimeOutOfBase => PlayerInfo.GetPlayerStat(PlayerStatType.TimeOutOfBase).Floor(),
                    AchievementType.PlayerKills => PlayerInfo.GetPlayerStat(PlayerStatType.PlayerKills).Floor(),
                    AchievementType.DistanceWalked => PlayerInfo.GetPlayerStat(PlayerStatType.DistanceWalk).Floor(),
                    AchievementType.DistanceRan => PlayerInfo.GetPlayerStat(PlayerStatType.DistanceRun).Floor(),
                    AchievementType.DistanceSailed => PlayerInfo.GetPlayerStat(PlayerStatType.DistanceSail).Floor(),
                    AchievementType.DistanceInAir => PlayerInfo.GetPlayerStat(PlayerStatType.DistanceAir).Floor(),
                    AchievementType.MineHits => PlayerInfo.GetPlayerStat(PlayerStatType.MineHits).Floor(),
                    AchievementType.FoodEaten => PlayerInfo.GetPlayerStat(PlayerStatType.FoodEaten).Floor(),
                    AchievementType.Recipes => Player.m_localPlayer.GetKnownRecipeCount(),
                    AchievementType.ItemsPicked => PlayerInfo.GetPlayerStat(PlayerStatType.ItemsPickedUp).Floor(),
                    AchievementType.ArrowsShot => PlayerInfo.GetPlayerStat(PlayerStatType.ArrowsShot).Floor(),
                    AchievementType.TotalJumps => PlayerInfo.GetPlayerStat(PlayerStatType.Jumps).Floor(),
                    AchievementType.TotalMined => PlayerInfo.GetPlayerStat(PlayerStatType.Mines).Floor(),
                    AchievementType.CreatureTamed => PlayerInfo.GetPlayerStat(PlayerStatType.CreatureTamed).Floor(),
                    AchievementType.Pickable => PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Pickable, PrefabName),
                    AchievementType.Kill => PlayerInfo.GetEnemyKill(PrefabName, false),
                    AchievementType.CreatureGroup => CreatureGroup.GetProgress(Group, Threshold),
                    _ => 0,
                };
            }
        }
    }
}

public static class AchievementHelpers
{
    public static bool IsCompleted(this AchievementManager.Achievement achievement, Player player) => achievement.Requirement.GetProgress(player) >= achievement.Requirement.GetThreshold();
    public static bool IsCollected(this AchievementManager.Achievement achievement, Player player)
    {
        return player.GetCollectedAchievements().Contains(achievement.Name.GetStableHashCode());
    }
    public static void ResetAchievementCollected(this Player player, AchievementManager.Achievement achievement)
    {
        List<int> list = player.GetCollectedAchievements();
        list.Remove(achievement.Name.GetStableHashCode());
        string data = AchievementManager.serializer.Serialize(list);
        player.m_customData[AchievementManager.ACHIEVEMENT_KEY] = data;
    }
    public static void SetAchievementCollected(this Player player, AchievementManager.Achievement achievement)
    {
        List<int> list = player.GetCollectedAchievements();
        list.Add(achievement.Name.GetStableHashCode());
        string data = AchievementManager.serializer.Serialize(list);
        player.m_customData[AchievementManager.ACHIEVEMENT_KEY] = data;
    }
    public static List<int> GetCollectedAchievements(this Player player)
    {
        return !player.m_customData.TryGetValue(AchievementManager.ACHIEVEMENT_KEY, out var achievements) 
            ? new() 
            : AchievementManager.deserializer.Deserialize<List<int>>(achievements);
    }
    public static string GetSharedName(this AchievementManager.Achievement.AchievementRequirement requirement)
    {
        if (!string.IsNullOrEmpty(requirement._sharedName)) return requirement._sharedName!;
        if (requirement.prefab == null) return string.Empty;
        requirement._sharedName = requirement.Type switch
        {
            AchievementType.Fish => requirement.prefab.TryGetComponent(out Fish fish) ? fish.m_name : "<color=red>Invalid</color>",
            AchievementType.Materials or AchievementType.Consumables or AchievementType.Weapons or AchievementType.Swords or 
                AchievementType.Axes or AchievementType.PoleArms or AchievementType.Spears or AchievementType.Maces or 
                AchievementType.Knives or AchievementType.Shields or AchievementType.Staves or AchievementType.Arrows or
                AchievementType.Bows or AchievementType.Valuables or AchievementType.Potions or AchievementType.Trophies or AchievementType.Trinkets
                => requirement.prefab.TryGetComponent(out ItemDrop itemDrop) ? itemDrop.m_itemData.m_shared.m_name : "<color=red>Invalid</color>",
            AchievementType.ItemsPicked => requirement.prefab.TryGetComponent(out Pickable pickable) 
                ? pickable.m_itemPrefab.TryGetComponent(out ItemDrop pickedItem) 
                    ? pickedItem.m_itemData.m_shared.m_name 
                    : "<color=red>Invalid</color>" : "<color=red>Invalid</color>",
            AchievementType.Kill => CritterHelper.namedCritters.TryGetValue(requirement.PrefabName, out var info) ? info.character.m_name : "<color=red>Invalid</color>",
            AchievementType.Pickable => ItemHelper.pickableItems.TryGetValue(requirement.PrefabName, out var info) ? info.shared.m_name : "<color=red>Invalid</color>",
            _ => "<color=red>Invalid</color>"
        };
        return requirement._sharedName;
    }
    public static int Floor(this float value) => Mathf.FloorToInt(value);
}

public static class AchievementReadMeBuilder
{
    private static readonly string[] Prefix = new[]
    {
        "# Achievements",
        "Almanac lets you define custom achievements using `.yml` files in the Achievements folder.",
        "These achievements sync between server and client, and are dynamically reloaded when edited.",
        "",
        "Below are the available **Achievement Types** you can use:",
        "```"
    };

    private static readonly string[] Postfix = new[]
    {
        "```",
        "### Achievement File Structure",
        "Each achievement is defined as a YAML file with properties like:",
        "- `UniqueID`: A unique identifier string (e.g., `Weapons.001`).",
        "- `Name`: Display name for the achievement.",
        "- `Lore`: A short description or flavor text.",
        "- `Icon`: The icon name from the game's assets.",
        "- `TokenReward`: Reward tokens for completing the achievement.",
        "- `Requirement`: The type, threshold, and optional group or prefab name.",
        "",
        "### Examples",
        "- Defeating creatures from a biome: `AchievementType.CreatureGroup` with `Group = Meadows`.",
        "- Reaching a milestone (kills, distance, etc.): set `Requirement.Type` to the relevant stat.",
        "- Collecting all of an item type (fish, weapons, trophies, etc.): use collection-based types.",
        "",
        "### Tips",
        "- Files can be added, changed, or deleted while the server is running.",
        "- Server automatically syncs achievements to clients.",
        "- Thresholds can be left at `0` for auto-detection (e.g., total number of fish).",
        "- PrefabName is required for `Kill` and `Pickable` types."
    };

    public static void Write()
    {
        if (AlmanacPlugin.AlmanacDir.FileExists("Achievements_README.md")) return;
        var achievementTypes = Enum.GetNames(typeof(AchievementType));
        List<string> lines = new();
        lines.AddRange(Prefix);
        lines.AddRange(achievementTypes);
        lines.AddRange(Postfix);
        AlmanacPlugin.AlmanacDir.WriteAllLines("Achievements_README.md", lines);
    }
}