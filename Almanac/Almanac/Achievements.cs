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

    private static readonly List<string> meadowCreatures = new()
    {
        "defeated_greyling",
        "defeated_neck",
        "defeated_boar",
        "defeated_deer",
        "defeated_eikthyr"
    };

    private static readonly List<string> blackforestCreatures = new()
    {
        "defeated_skeleton",
        "KilledTroll",
        "defeated_ghost",
        "defeated_greydwarf",
        "defeated_greydwarf_elite",
        "defeated_greydwarf_shaman",
        "defeated_gdking"
    };

    private static readonly List<string> swampCreatures = new()
    {
        "defeated_blob",
        "defeated_blobelite",
        "defeated_draugr",
        "defeated_draugr_elite",
        "defeated_skeleton_poison",
        "defeated_surtling",
        "defeated_bonemass"
    };
    
    private static readonly List<ItemDrop> allFish = ItemDataCollector.GetFishes();
    private static readonly List<ItemDrop> allMaterials = ItemDataCollector.GetMaterials();
    private static readonly List<ItemDrop> allConsumables = ItemDataCollector.GetConsumables();
    private static readonly List<ItemDrop> allWeapons = ItemDataCollector.GetWeapons();
    private static readonly List<ItemDrop> allProjectiles = ItemDataCollector.GetAmmunition();
    private static readonly List<ItemDrop> allBows = 
        allWeapons.FindAll(weapon => weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));

    private static readonly List<ItemDrop> allArrows =
        allProjectiles.FindAll(projectile => projectile.name.Contains("Arrow"));
    
    private static readonly List<ItemDrop> allValuables =
        allMaterials.FindAll(item => item.m_itemData.m_shared.m_value > 0);

    private static readonly List<ItemDrop> allMeads =
        allMaterials.FindAll(item => vanillaMeads.Contains(item.m_itemData.m_shared.m_name));

    private static readonly List<ItemDrop> allTrophies = ItemDataCollector.GetTrophies();
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

    private static Sprite GetTrophySprite(string prefabName) => allTrophies.Find(item => item.name == prefabName).m_itemData.GetIcon();

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
        
        CreateKillAchievements();
        // CreateDeathAchievements();
        CreateDiscoverAchievements();
        // CreateBossAchievements();
    }

    private static void CreateDiscoverAchievements()
    {
        CreateAchievement(
            "$almanac_achievement_ranger",
            "$almanac_achievement_ranger_desc",
            allArrows.Count,
            AlmanacPlugin.arrowBasicIcon,
            "$almanac_ranger_lore",
            "se_ranger",
            "<color=white>$almanac_increase_projectile_damage_by </color><color=orange>10</color>"
            );
        CreateAchievement(
            "$almanac_achievement_brew_master",
            "$almanac_achievement_brew_master_desc",
            vanillaMeads.Count,
            AlmanacPlugin.bottleStandardBlueIcon, 
            "$almanac_brew_master_lore",
            "se_brew_master",
            "<color=white>$almanac_increase_fire_damage_by</color><color=orange>10</color>"
            );
        CreateAchievement(
            "$almanac_achievement_fisher",
            "$almanac_achievement_fisher_desc",
            allFish.Count,
            AlmanacPlugin.fishGreenIcon,
            "$almanac_fisher_lore",
            "GP_Moder",
            "<color=white>$almanac_allows_moder_power</color>"
            );
        CreateAchievement(
            "$almanac_achievement_knowledgeable",
            "$almanac_achievement_knowledgeable_desc",
            allMaterials.Count,
            AlmanacPlugin.necklaceSilverRed,
            "$almanac_knowledgeable_lore",
            "se_knowledgeable",
            "<color=white>$almanac_increase_carry_weight_by </color><color=orange>100</color>"
            );
        // CreateAchievement(
        //     "Gluttony",
        //      "Discovered all consumables",
        //     allConsumables.Count,
        //     AlmanacPlugin.mushroomBigRedIcon,
        //     "$almanac_consumables_all_lore",
        //     "",
        //     ""
        //     );
        // CreateAchievement(
        //     "Barracks",
        //     "Discovered all weapons",
        //     allWeapons.Count,
        //      AlmanacPlugin.swordBasicBrownIcon,
        //     "$almanac_weapons_all_lore",
        //     "",
        //     ""
        //     );
        CreateAchievement(
            "$almanac_achievement_master_archer",
            "$almanac_achievement_master_archer_desc",
            allBows.Count,
            AlmanacPlugin.bowWoodIcon,
            "$almanac_master_archer_lore",
            "se_master_archer",
            "<color=white>$almanac_increase_projectile_damage_by</color><color=orange>15</color>"
            );
        // CreateAchievement(
        //     "Salesman",
        //     "Discovered all valuables",
        //     allValuables.Count,
        //     AlmanacPlugin.goldCoinsPileIcon,
        //     "$almanac_valuables_all_lore",
        //     "se_salesman",
        //     "Increases carry weight by <color=orange>25</color>"
        //     );
    }
    private static void CreateBossAchievements()
    {
        CreateAchievement(
            "Stag Slayer",
            "Killed Eikthyr over 100 times",
            100,
            GetTrophySprite("TrophyEikthyr"),
            "$almanac_eikthyr_100_lore",
            "GP_Eikthyr",
            "Enables access to <color=orange>Eikthyr Power</color> from Almanac"
        );
    }

    private static void CreateKillAchievements()
    {
        CreateAchievement(
            "$almanac_achievement_meadow_kill",
            "$almanac_achievement_meadow_kill_desc",
            meadowCreatures.Count,
            GetTrophySprite("TrophyEikthyr"),
            "$almanac_achievement_meadow_kill_lore",
            "se_meadow_kill",
            "$almanac_increase_health_by <color=orange>5</color>"
        );
        CreateAchievement(
            "$almanac_achievement_blackforest_kill",
            "$almanac_achievement_blackforest_kill_desc",
            blackforestCreatures.Count,
            GetTrophySprite("TrophyTheElder"), 
            "$almanac_achievement_blackforest_kill_lore",
            "se_blackforest_kill",
            "$almanac_increase_health_by <color=orange>10</color>" 
        );
    }

    private static void CreateDeathAchievements()
    {
        CreateAchievement(
            "First Death", 
            "Died at least once",
            1, 
            AlmanacPlugin.capeHoodIcon,
            "$almanac_first_death_lore",
            "se_first_death",
            "Increases base stamina by <color=orange>10</color>"
        );
        CreateAchievement(
            "Undying", "Died at least 1000 times",
            1000,
            AlmanacPlugin.boneSkullIcon,
            "$almanac_1000_death_lore",
            "",
            ""
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

    private static Dictionary<string,int> CombineDict(Dictionary<string, int> dict1, Dictionary<string, int> dict2)
    {
        Dictionary<string, int> combinedDict = new();

        foreach (var kvp in dict1)
        {
            if (combinedDict.ContainsKey(kvp.Key)) combinedDict[kvp.Key] += kvp.Value;
            else combinedDict[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in dict2)
        {
            if (combinedDict.ContainsKey(kvp.Key)) combinedDict[kvp.Key] += kvp.Value;
            else combinedDict[kvp.Key] = kvp.Value;
        }

        return combinedDict;
    }
    public static void UpdateAchievements()
    {
        Player player = Player.m_localPlayer;
        Dictionary<string, int> tempMonstersKilled = TrackPlayerKills.TempMonstersKilled;
        Dictionary<string, int> currentMonstersKilled = TrackPlayerKills.GetCurrentKilledMonsters();

        Dictionary<string, int> totalMonstersKilled = CombineDict(tempMonstersKilled, currentMonstersKilled);
        // Dictionary<string, int> tempPlayerDeaths = TrackPlayerDeaths.TempPlayerDeaths;
        // Dictionary<string, int> currentPlayerDeaths = TrackPlayerDeaths.GetCurrentPlayerDeaths();

        int totalKills = tempMonstersKilled.Values.Sum() + currentMonstersKilled.Values.Sum();
        int totalDeaths = Convert.ToInt32(TrackPlayerStats.GetPlayerStat(PlayerStatType.Deaths));
        int totalKnownPotions = allMaterials.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name) && vanillaMeads.Contains(material.name));
        int totalKnownMaterials = allMaterials.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name));
        // int totalKnownConsumables = allConsumables.Count(consumable => player.IsMaterialKnown(consumable.m_itemData.m_shared.m_name));
        int totalKnownFish = allFish.Count(fish => player.IsMaterialKnown(fish.m_itemData.m_shared.m_name));
        // int totalKnownWeapons = allWeapons.Count(weapon => player.IsKnownMaterial(weapon.m_itemData.m_shared.m_name));
        // bool isHammerKnown = player.IsKnownMaterial("Hammer");
        int totalKnownBows = allWeapons.Count(weapon => player.IsMaterialKnown(weapon.m_itemData.m_shared.m_name) && weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));
        int totalKnownProjectiles = allProjectiles.Count(projectile =>
            player.IsMaterialKnown(projectile.m_itemData.m_shared.m_name) && projectile.name.Contains("Arrow"));
        // int totalKnownValuables =
        //     allValuables.Count(material => player.IsMaterialKnown(material.m_itemData.m_shared.m_name));
        // int totalEikthyrKills = tempMonstersKilled["defeated_eikthyr"] + currentMonstersKilled["defeated_eikthyr"];

        int totalMeadowKills = GetBiomeKills(meadowCreatures, totalMonstersKilled);
        int totalBlackforestKills = GetBiomeKills(blackforestCreatures, totalMonstersKilled);

        Dictionary<string, int> updateAchievementMap = new()
        {
            { "$almanac_achievement_meadow_kill", totalMeadowKills },
            { "$almanac_achievement_blackforest_kill", totalBlackforestKills },
            { "$almanac_achievement_ranger", totalKnownProjectiles },
            { "$almanac_achievement_brew_master", totalKnownPotions },
            { "$almanac_achievement_fisher", totalKnownFish },
            { "$almanac_achievement_master_archer", totalKnownBows },
            { "$almanac_achievement_knowledgeable", totalKnownMaterials },

        };

        foreach (var updateMap in updateAchievementMap)
        {
            UpdateAchievement(updateMap.Key, updateMap.Value);
        }
    }

    private static int GetBiomeKills(List<string> creatureList, Dictionary<string, int> totalMonstersKilled)
    {
        int totalKills = 0;
        foreach (var defeatKey in creatureList)
        {
            int killCount = GetKillCount(defeatKey, totalMonstersKilled);
            if (killCount > 0) totalKills += 1;
        }

        return totalKills;
    }

    private static int GetKillCount(string defeatKey, Dictionary<string, int> totalMonstersKilled)
    {
        return totalMonstersKilled[defeatKey];
    }

    private static void UpdateAchievement(string name, int value)
    {
        var achievement = allAchievements.FirstOrDefault(ach => ach.name == name);
        if (achievement == null) return;
        achievement.isCompleted = value >= achievement.total;
        achievement.value = value;
    }
}