using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Core.Tokens;

namespace Almanac.Almanac;

public static class Achievements
{
    public static readonly List<Achievement> registeredAchievements = new();

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
        "killed_surtling",
        "defeated_wraith",
        "defeated_leech",
        "defeated_bonemass"
    };

    private static readonly List<string> mountainCreatures = new()
    {
        "defeated_wolf",
        "defeated_fenring",
        "defeated_hatchling",
        "KilledBat",
        "defeated_fenring_cultist",
        "defeated_stonegolem",
        "defeated_ulv",
        "defeated_dragon"
    };
    
    public static readonly List<ItemDrop> allFish = ItemDataCollector.GetFishes();
    public static readonly List<ItemDrop> allMaterials = ItemDataCollector.GetMaterials();
    public static readonly List<ItemDrop> allConsumables = ItemDataCollector.GetConsumables();
    public static readonly List<ItemDrop> allWeapons = ItemDataCollector.GetWeapons();
    public static readonly List<ItemDrop> allProjectiles = ItemDataCollector.GetAmmunition();
    public static readonly List<ItemDrop> allBows = allWeapons.FindAll(weapon => weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));
    public static readonly List<ItemDrop> allArrows = allProjectiles.FindAll(projectile => projectile.name.Contains("Arrow"));
    public static readonly List<ItemDrop> allValuables = allMaterials.FindAll(item => item.m_itemData.m_shared.m_value > 0);
    public static readonly List<ItemDrop> allMeads = allMaterials.FindAll(item => vanillaMeads.Contains(item.m_itemData.m_shared.m_name));
    public static readonly List<ItemDrop> allTrophies = ItemDataCollector.GetTrophies();
    public class Achievement
    {
        public AchievementManager.AchievementType type;
        public string name = null!;
        public string? description = null!;
        public int total;
        public int value;
        public Sprite? sprite;
        public bool isCompleted;
        public string? lore = null!;
        public string power = null!;
        public string? powerToolTip = null!;
    }

    private static Sprite? GetSprite(string? prefabName)
    {
        if (prefabName.IsNullOrWhiteSpace()) return null;
        ZNetScene scene = ZNetScene.instance;
        GameObject prefab = scene.GetPrefab(prefabName);
        prefab.TryGetComponent(out ItemDrop itemDrop);
        if (itemDrop)
        {
            return itemDrop.m_itemData.GetIcon();
        }

        return null;
    }
    private static void CreateAchievement(
        string name, string? description,
        int total, Sprite? sprite,
        string? lore, string powerName,
        string? powerToolTip,
        AchievementManager.AchievementType type)
    {
        if (powerName.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace()) return;
        Achievement data = new Achievement()
        {
            name = name,
            description = description,
            total = total,
            sprite = sprite,
            value = 0,
            lore = lore,
            power = powerName,
            powerToolTip = powerToolTip,
            type = type
        };
        registeredAchievements.Add(data);
    }
    
    public static void RegisterAchievements()
    {
        registeredAchievements.Clear();
        foreach (AchievementManager.Achievement achievement in AchievementManager.tempAchievements)
        {
            int total = 0;

            switch (achievement.m_type)
            {
                case AchievementManager.AchievementType.Fish:
                    total = allFish.Count;
                    break;
                case AchievementManager.AchievementType.Materials:
                    total = allMaterials.Count;
                    break;
                case AchievementManager.AchievementType.Consumables:
                    total = allConsumables.Count;
                    break;
                case AchievementManager.AchievementType.Weapons:
                    total = allWeapons.Count;
                    break;
                case AchievementManager.AchievementType.Arrows:
                    total = allArrows.Count;
                    break;
                case AchievementManager.AchievementType.Bows:
                    total = allBows.Count;
                    break;
                case AchievementManager.AchievementType.Valuables:
                    total = allValuables.Count;
                    break;
                case AchievementManager.AchievementType.Potions:
                    total = allMaterials.FindAll(item => item.name.Contains("Mead")).Count;
                    break;
                case AchievementManager.AchievementType.Trophies:
                    total = allTrophies.Count;
                    break;
                case AchievementManager.AchievementType.Creatures:
                    total = CreatureDataCollector.tempCreatureData.Count;
                    break;
                case AchievementManager.AchievementType.MeadowCreatures:
                    total = meadowCreatures.Count;
                    break;
                case AchievementManager.AchievementType.BlackForestCreatures:
                    total = blackforestCreatures.Count;
                    break;
                case AchievementManager.AchievementType.SwampCreatures:
                    total = swampCreatures.Count;
                    break;
                case AchievementManager.AchievementType.MountainCreatures:
                    total = mountainCreatures.Count;
                    break;
                case AchievementManager.AchievementType.PlainsCreatures:
                    break;
                case AchievementManager.AchievementType.MistLandCreatures:
                    break;
                case AchievementManager.AchievementType.AshLandCreatures:
                    break;
                case AchievementManager.AchievementType.DeepNorthCreatures:
                    break;
                default:
                    total = achievement.m_goal;
                    break;
            }

            CreateAchievement(
                name: achievement.m_displayName,
                description: achievement.m_desc,
                total: total,
                sprite: achievement.m_sprite ? achievement.m_sprite : GetSprite(achievement.m_spriteName),
                lore: achievement.m_lore,
                powerName: achievement.m_statusEffect.effectName,
                powerToolTip: achievement.m_toolTip,
                type: achievement.m_type
            );
        }
    }
    public static void SetUnknownAchievements(Transform parentElement)
    {
        for (int i = 0; i < registeredAchievements.Count; ++i)
        {
            Transform container = parentElement.Find($"achievementsContainer ({i})");
            Transform icon = container.Find("iconObj");
            Transform hoverText = container.Find("hoverTextElement");
            
            icon.TryGetComponent(out Image containerImage);
            hoverText.TryGetComponent(out TextMeshProUGUI textMesh);

            if (!containerImage || !textMesh) return;

            Achievement achievement = registeredAchievements[i];

            string localizedAchievementName = Localization.instance.Localize(achievement.name);

            containerImage.color = CheckCheats.PlayerWatcher.noCost ? Color.white : achievement.isCompleted ? Color.white : Color.black;
            textMesh.text = CheckCheats.PlayerWatcher.noCost ? localizedAchievementName : achievement.isCompleted ? localizedAchievementName : "???";
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

        Dictionary<AchievementManager.AchievementType, int> updateAchievementMap = new()
        {
            { AchievementManager.AchievementType.Fish , allFish.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Materials , allMaterials.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Consumables , allConsumables.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Weapons , allWeapons.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Arrows , allArrows.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Bows , allBows.Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Valuables , allValuables.Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Potions , allMaterials.FindAll(item => item.name.Contains("Mead")).Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Trophies , allTrophies.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.MeadowCreatures , GetBiomeKills(meadowCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.BlackForestCreatures , GetBiomeKills(blackforestCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.SwampCreatures , GetBiomeKills(swampCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.MountainCreatures , GetBiomeKills(mountainCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.Deaths , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.Deaths) },
            { AchievementManager.AchievementType.EikthyrKills , GetKillCount("defeated_eikthyr", totalMonstersKilled) },
            { AchievementManager.AchievementType.ElderKills , GetKillCount("defeated_gdking", totalMonstersKilled) },
            { AchievementManager.AchievementType.BonemassKills , GetKillCount("defeated_bonemass", totalMonstersKilled) },
            { AchievementManager.AchievementType.ModerKills , GetKillCount("defeated_dragon", totalMonstersKilled) },
            { AchievementManager.AchievementType.YagluthKills , GetKillCount("defeated_goblinking", totalMonstersKilled) },
            { AchievementManager.AchievementType.QueenKills , GetKillCount("defeated_queen", totalMonstersKilled) },
        };
        
        foreach (Achievement achievement in registeredAchievements)
        {
            int value = updateAchievementMap[achievement.type];
            achievement.isCompleted = value >= achievement.total;
            achievement.value = value;
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

    private static void UpdateAchievement(AchievementManager.AchievementType type, int value)
    {

        List<Achievement> achievements = registeredAchievements.FindAll(x => x.type == type);
        if (achievements.Count == 0) return;
        
        foreach (Achievement achievement in achievements)
        {
            achievement.isCompleted = value >= achievement.total;
            achievement.value = value;
        }
    }
}