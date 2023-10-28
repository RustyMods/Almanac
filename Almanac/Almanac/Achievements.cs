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

    private static List<string> vanillaMeads = new()
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

    public class Achievement
    {
        public string name = null!;
        public string description = null!;
        public int total;
        public int value;
        public Sprite? sprite;
        public bool isCompleted;
    }

    private static void CreateAchievement(string name, string description, int total, Sprite sprite)
    {
        Achievement data = new Achievement()
        {
            name = name,
            description = description,
            total = total,
            sprite = sprite,
            value = 0
        };
        allAchievements.Add(data);
    }

    public static void RegisterAchievements()
    {
        allAchievements.Clear();
        CreateAchievement("First Kill", "Killed at least one creature", 1, AlmanacPlugin.arrowBasicIcon);
        CreateAchievement("First Death", "Died at least once", 1, AlmanacPlugin.capeHoodIcon);
        CreateAchievement("Undying", "Died at least 1000 times", 1000, AlmanacPlugin.boneSkullIcon);
        CreateAchievement("Killtacular", "Killed 150 monsters", 101, AlmanacPlugin.swordBasicBlueIcon);
        CreateAchievement("Amateur Chemist", "Discovered at least one mead recipe", 1,
            AlmanacPlugin.bottleStandardEmptyIcon);
        CreateAchievement("Brew Master", "Discovered all mead recipes", vanillaMeads.Count,
            AlmanacPlugin.bottleStandardBlueIcon);
        CreateAchievement("Fisherman", "Discovered all the fishies", allFish.Count, AlmanacPlugin.fishGreenIcon);
        CreateAchievement("Knowledgeable", "Discovered all materials", allMaterials.Count,
            AlmanacPlugin.necklaceSilverRed);
        CreateAchievement("Gluttony", "Discovered all consumables", allConsumables.Count,
            AlmanacPlugin.mushroomBigRedIcon);
        CreateAchievement("Barracks", "Discovered all weapons", allWeapons.Count, AlmanacPlugin.swordBasicBrownIcon);
        CreateAchievement("Master Archer", "Discovered all bows", allBows.Count, AlmanacPlugin.bowWoodIcon);
        CreateAchievement("Salesman", "Discovered all valuables", allValuables.Count, AlmanacPlugin.goldCoinsPileIcon);
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
        int totalKills = TrackPlayerKills.TempMonstersKilled.Values.Sum() +
                         TrackPlayerKills.GetCurrentKilledMonsters().Values.Sum();
        int totalDeaths = TrackPlayerDeaths.TempPlayerDeaths.Values.Sum() +
                          TrackPlayerDeaths.GetCurrentPlayerDeaths().Values.Sum();
        int totalKnownPotions = allMaterials.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name) && vanillaMeads.Contains(material.name));
        int totalKnownMaterials = allMaterials.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name));
        int totalKnownConsumables = allConsumables.Count(consumable => player.IsMaterialKnown(consumable.m_itemData.m_shared.m_name));
        int totalKnownFish = allFish.Count(fish => player.IsMaterialKnown(fish.m_itemData.m_shared.m_name));
        int totalKnownWeapons = allWeapons.Count(weapon => player.IsKnownMaterial(weapon.m_itemData.m_shared.m_name));
        bool isHammerKnown = player.IsKnownMaterial("Hammer");
        int totalKnownBows = allWeapons.Count(weapon => player.IsMaterialKnown(weapon.m_itemData.m_shared.m_name) && weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));
        int totalKnownValuables =
            allValuables.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name));
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
            { "Salesman", totalKnownValuables }
        };

        foreach (var updateMap in updateAchievementMap)
        {
            UpdateAchievement(updateMap.Key, updateMap.Value);
        }
    }

    private static void UpdateAchievement(string name, int value)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.name == name)
            {
                achievement.isCompleted = value >= achievement.total;
                achievement.value = value;
            }
        }
    }
}