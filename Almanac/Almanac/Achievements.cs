using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Core.Tokens;

namespace Almanac.Almanac;

public static class Achievements
{
    public static readonly List<Achievement> allAchievements = new();

    private static readonly List<string> vanillaMeads = new()
    {
        "MeadBaseEitrMinor",
        "MeadBaseFrostResist",
        "MeadBaseHealthMajor",
        "MeadBaseHealthMedium",
        "MeadBaseHealthMinor",
        "MeadBaseStaminaLingering",
        "MeadBaseStaminaMedium",
        "MeadBaseStaminaMinor",
        "BarleyWineBase"
    };

    private static readonly List<ItemDrop> allFish = ItemDataCollector.GetFishes();
    private static readonly List<ItemDrop> allMaterials = ItemDataCollector.GetMaterials();
    private static readonly List<ItemDrop> allConsumables = ItemDataCollector.GetConsumables();
    private static readonly List<ItemDrop> allWeapons = ItemDataCollector.GetWeapons();
    private static readonly List<ItemDrop> allBows = allWeapons.FindAll(weapon => weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));

    private static readonly List<ItemDrop> allValuables =
        allMaterials.FindAll(item => item.m_itemData.m_shared.m_value > 0);

    private static readonly List<ItemDrop> allMeads =
        allMaterials.FindAll(item => vanillaMeads.Contains(item.m_itemData.m_shared.m_name));

    private static readonly List<ItemDrop> allTrophies = ItemDataCollector.GetTrophies();

    private static readonly Sprite eikthyrSprite =
        allTrophies.Find(item => item.name == "TrophyEikthyr").m_itemData.GetIcon();
    public class Achievement
    {
        public string name = null!;
        public string description = null!;
        public int total;
        public int value;
        public Sprite? sprite;
        public bool isCompleted;
        public string lore = null!;
        public string power = null!;
        public string powerToolTip = null!;
    }

    private static void CreateAchievement(
        string name, string description,
        int total, Sprite? sprite,
        string lore, string powerName,
        string powerToolTip)
    {
        Achievement data = new Achievement()
        {
            name = name,
            description = description,
            total = total,
            sprite = sprite,
            value = 0,
            lore = lore,
            power = powerName,
            powerToolTip = powerToolTip
        };
        allAchievements.Add(data);
    }
    
    public static void RegisterAchievements()
    {
        allAchievements.Clear();
        
        CreateAchievement(
            "First Kill",
            "Killed at least one creature",
            1,
            AlmanacPlugin.arrowBasicIcon,
            "$almanac_first_kill_lore",
            "se_first_kill", "Increases base health by <color=orange>10</color>"
        );
        CreateAchievement(
            "First Death", 
            "Died at least once",
            1, 
            AlmanacPlugin.capeHoodIcon,
            "$almanac_first_death_lore",
                "se_first_death", "Increases base stamina by <color=orange>25</color>"
            );
        CreateAchievement(
            "Undying", "Died at least 1000 times",
            1000, AlmanacPlugin.boneSkullIcon, "$almanac_1000_death_lore",
            "GP_Eikthyr", "");
        CreateAchievement(
            "Killtacular", "Killed 150 monsters",
            101, AlmanacPlugin.swordBasicBlueIcon, 
            "$almanac_kill_150_lore",
            "GP_Eikthyr", "" 
            );
        CreateAchievement(
            "Amateur Chemist", "Discovered at least one mead recipe",
            1,
            AlmanacPlugin.bottleStandardEmptyIcon,
            "$almanac_mead_1_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Brew Master", "Discovered all mead recipes",
            vanillaMeads.Count,
            AlmanacPlugin.bottleStandardBlueIcon, 
            "$almanac_mead_all_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Fisherman", "Discovered all the fishies",
            allFish.Count,
            AlmanacPlugin.fishGreenIcon,
            "$almanac_fish_all_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Knowledgeable", "Discovered all materials",
            allMaterials.Count,
            AlmanacPlugin.necklaceSilverRed,
            "$almanac_materials_all_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Gluttony", "Discovered all consumables",
            allConsumables.Count,
            AlmanacPlugin.mushroomBigRedIcon,
            "$almanac_consumables_all_lore",
            "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Barracks", "Discovered all weapons",
            allWeapons.Count, AlmanacPlugin.swordBasicBrownIcon,
            "$almanac_weapons_all_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Master Archer", "Discovered all bows",
            allBows.Count, AlmanacPlugin.bowWoodIcon,
            "$almanac_bows_all_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Salesman", "Discovered all valuables",
            allValuables.Count, AlmanacPlugin.goldCoinsPileIcon,
            "$almanac_valuables_all_lore", "GP_Eikthyr", ""
            );
        CreateAchievement(
            "Stag Slayer", "Killed Eikthyr over 100 times",
            100, eikthyrSprite,
            "$almanac_eikthyr_100_lore",
            "GP_Eikthyr", ""
            );
    }

    public static void SetUnknownAchievements(Transform parentElement)
    {
        for (int i = 0; i < allAchievements.Count; ++i)
        {
            Transform container = parentElement.Find($"achievementsContainer ({i})");
            Transform icon = container.Find("iconObj");
            Transform hoverText = container.Find("hoverTextElement");
            icon.TryGetComponent(out Image containerImage);
            hoverText.TryGetComponent(out TextMeshProUGUI textMesh);

            var achievement = allAchievements[i];

            containerImage.color = achievement.isCompleted ? Color.white : Color.black;
            textMesh.text = achievement.isCompleted ? achievement.name : "???";
        }
    }

    public static void UpdateAchievements()
    {
        Player player = Player.m_localPlayer;
        Dictionary<string, int> tempMonstersKilled = TrackPlayerKills.TempMonstersKilled;
        Dictionary<string, int> currentMonstersKilled = TrackPlayerKills.GetCurrentKilledMonsters();
        Dictionary<string, int> tempPlayerDeaths = TrackPlayerDeaths.TempPlayerDeaths;
        Dictionary<string, int> currentPlayerDeaths = TrackPlayerDeaths.GetCurrentPlayerDeaths();

        int totalKills = tempMonstersKilled.Values.Sum() + currentMonstersKilled.Values.Sum();
        int totalDeaths = tempPlayerDeaths.Values.Sum() + currentPlayerDeaths.Values.Sum();
        int totalKnownPotions = allMaterials.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name) && vanillaMeads.Contains(material.name));
        int totalKnownMaterials = allMaterials.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name));
        int totalKnownConsumables = allConsumables.Count(consumable => player.IsMaterialKnown(consumable.m_itemData.m_shared.m_name));
        int totalKnownFish = allFish.Count(fish => player.IsMaterialKnown(fish.m_itemData.m_shared.m_name));
        int totalKnownWeapons = allWeapons.Count(weapon => player.IsKnownMaterial(weapon.m_itemData.m_shared.m_name));
        bool isHammerKnown = player.IsKnownMaterial("Hammer");
        int totalKnownBows = allWeapons.Count(weapon => player.IsMaterialKnown(weapon.m_itemData.m_shared.m_name) && weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));
        int totalKnownValuables =
            allValuables.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name));
        int totalEikthyrKills = tempMonstersKilled["defeated_eikthyr"] + currentMonstersKilled["defeated_eikthyr"];
        Dictionary<string, int> updateAchievementMap = new()
        {
            { "First Kill", totalKills },
            { "First Death", totalDeaths },
            { "Undying", totalDeaths },
            { "Killtacular", totalKills },
            { "Amateur Chemist", totalKnownPotions },
            { "Brew Master", totalKnownPotions },
            { "Fisherman", totalKnownFish },
            { "Knowledgeable", totalKnownMaterials },
            { "Gluttony", totalKnownConsumables },
            { "Barracks", totalKnownWeapons },
            { "Master Archer", totalKnownBows },
            { "Salesman", totalKnownValuables },
            { "Stag Slayer", totalEikthyrKills }
        };

        foreach (var updateMap in updateAchievementMap)
        {
            UpdateAchievement(updateMap.Key, updateMap.Value);
        }
    }

    private static void UpdateAchievement(string name, int value)
    {
        var achievement = allAchievements.FirstOrDefault(ach => ach.name == name);
        if (achievement == null) return;
        achievement.isCompleted = value >= achievement.total;
        achievement.value = value;
    }
}