using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Core.Tokens;
using Object = UnityEngine.Object;

namespace Almanac.Almanac;

public static class AchievementsUI
{
    public static readonly List<Achievement> registeredAchievements = new();

    public const int maxPowers = 5;

    public static int powerLimit = 3;

    private static readonly List<string> meadowCreatures = new()
    {
        "defeated_greyling",
        "defeated_neck",
        "defeated_boar",
        "defeated_deer",
        "defeated_eikthyr"
    };

    private static readonly List<string> blackForestCreatures = new()
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

    private static readonly List<string> plainsCreatures = new()
    {
        "defeated_deathsquito",
        "defeated_goblin",
        "defeated_goblinbrute",
        "defeated_goblinshaman",
        "defeated_lox",
        "defeated_blobtar",
        "defeated_goblinking"
    };

    private static readonly List<string> mistLandCreatures = new()
    {
        "defeated_dverger",
        "defeated_dvergermagefire",
        "defeated_dvergermagesupport",
        "defeated_dvergermageice",
        "defeated_gjall",
        "defeated_tick",
        "defeated_hare",
        "defeated_seeker",
        "defeated_seekerbrood",
        "defeated_seekerbrute",
        "defeated_queen"
    };
    
    private static readonly List<ItemDrop> allFish = ItemDataCollector.GetFishes();
    private static readonly List<ItemDrop> allMaterials = ItemDataCollector.GetMaterials();
    private static readonly List<ItemDrop> allConsumables = ItemDataCollector.GetConsumables();
    private static readonly List<ItemDrop> allWeapons = ItemDataCollector.GetWeapons();
    private static readonly List<ItemDrop> allProjectiles = ItemDataCollector.GetAmmunition();
    private static readonly List<ItemDrop> allBows = allWeapons.FindAll(weapon => weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow"));
    private static readonly List<ItemDrop> allArrows = allProjectiles.FindAll(projectile => projectile.name.Contains("Arrow"));
    private static readonly List<ItemDrop> allValuables = allMaterials.FindAll(item => item.m_itemData.m_shared.m_value > 0);
    private static readonly List<ItemDrop> allTrophies = ItemDataCollector.GetTrophies();
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
        return itemDrop ? itemDrop.m_itemData.GetIcon() : null;
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
        
        // AlmanacPlugin.AlmanacLogger.LogWarning("registering achievements to UI");
        
        foreach (AchievementManager.Achievement achievement in AchievementManager.tempAchievements)
        {
            int total = 0;

            // Conversion map to get total values to set
            switch (achievement.m_type)
            {
                case AchievementManager.AchievementType.Fish: total = allFish.Count; break;
                case AchievementManager.AchievementType.Materials: total = allMaterials.Count; break;
                case AchievementManager.AchievementType.Consumables: total = allConsumables.Count; break;
                case AchievementManager.AchievementType.Weapons: total = allWeapons.Count; break;
                case AchievementManager.AchievementType.Swords: total = allWeapons.FindAll(item => item.name.Contains("Sword")).Count; break;
                case AchievementManager.AchievementType.Axes: total = allWeapons.FindAll(item => item.name.Contains("Axe")).Count; break;
                case AchievementManager.AchievementType.PoleArms: total = allWeapons.FindAll(item => item.name.Contains("Atgeir")).Count; break;
                case AchievementManager.AchievementType.Spears: total = allWeapons.FindAll(item => item.name.Contains("Spear")).Count; break;
                case AchievementManager.AchievementType.Maces: total = allWeapons.FindAll(item => item.name.Contains("Mace")).Count; break;
                case AchievementManager.AchievementType.Knives: total = allWeapons.FindAll(item => item.name.Contains("Knife")).Count; break;
                case AchievementManager.AchievementType.Shields: total = allWeapons.FindAll(item => item.name.Contains("Shield")).Count; break;
                case AchievementManager.AchievementType.Staves: total = allWeapons.FindAll(item => item.name.Contains("Staff")).Count; break;
                case AchievementManager.AchievementType.Arrows: total = allArrows.Count; break;
                case AchievementManager.AchievementType.Bows: total = allBows.Count; break;
                case AchievementManager.AchievementType.Valuables: total = allValuables.Count; break;
                case AchievementManager.AchievementType.Potions: total = allMaterials.FindAll(item => item.name.Contains("Base")).Count; break;
                case AchievementManager.AchievementType.Trophies: total = allTrophies.Count; break;
                case AchievementManager.AchievementType.Creatures: total = CreatureDataCollector.tempCreatureData.Count; break;
                case AchievementManager.AchievementType.MeadowCreatures: total = meadowCreatures.Count; break;
                case AchievementManager.AchievementType.BlackForestCreatures: total = blackForestCreatures.Count; break;
                case AchievementManager.AchievementType.SwampCreatures: total = swampCreatures.Count; break;
                case AchievementManager.AchievementType.MountainCreatures: total = mountainCreatures.Count; break;
                case AchievementManager.AchievementType.PlainsCreatures: total = plainsCreatures.Count; break;
                case AchievementManager.AchievementType.MistLandCreatures: total = mistLandCreatures.Count; break;
                case AchievementManager.AchievementType.AshLandCreatures: break;
                case AchievementManager.AchievementType.DeepNorthCreatures: break;
                case AchievementManager.AchievementType.TotalAchievements: total = AchievementManager.tempAchievements.Count - 1; break;
                default: total = achievement.m_goal; break;
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

        int maxAchievements = registeredAchievements.Count;
        int completedAchievements = registeredAchievements.FindAll(x => x.isCompleted == true).Count;
        
        int percentage = completedAchievements / maxAchievements;

        if (percentage >= 0.5f) powerLimit = 4;
        if (percentage >= 0.9f) powerLimit = 5;
        
        Dictionary<string, int> tempMonstersKilled = TrackPlayerKills.TempMonstersKilled;
        Dictionary<string, int> currentMonstersKilled = TrackPlayerKills.GetCurrentKilledMonsters();
        Dictionary<string, int> totalMonstersKilled = CombineDict(tempMonstersKilled, currentMonstersKilled);

        Dictionary<AchievementManager.AchievementType, int> updateAchievementMap = new()
        {
            { AchievementManager.AchievementType.Fish , allFish.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Materials , allMaterials.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Consumables , allConsumables.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Weapons , allWeapons.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Swords , allWeapons.FindAll(item => item.name.Contains("Sword")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Axes , allWeapons.FindAll(item => item.name.Contains("Axe")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.PoleArms , allWeapons.FindAll(item => item.name.Contains("Atgeir")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Spears , allWeapons.FindAll(item => item.name.Contains("Spear")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Maces , allWeapons.FindAll(item => item.name.Contains("Mace")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Knives , allWeapons.FindAll(item => item.name.Contains("Knife")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Shields , allWeapons.FindAll(item => item.name.Contains("Shield")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Staves , allWeapons.FindAll(item => item.name.Contains("Staff")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { AchievementManager.AchievementType.Arrows , allArrows.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Bows , allBows.Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Valuables , allValuables.Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Potions , allMaterials.FindAll(item => item.name.Contains("Mead")).Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.Trophies , allTrophies.Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { AchievementManager.AchievementType.MeadowCreatures , GetBiomeKills(meadowCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.BlackForestCreatures , GetBiomeKills(blackForestCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.SwampCreatures , GetBiomeKills(swampCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.MountainCreatures , GetBiomeKills(mountainCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.PlainsCreatures , GetBiomeKills(plainsCreatures, totalMonstersKilled)},
            { AchievementManager.AchievementType.MistLandCreatures , GetBiomeKills(mistLandCreatures, totalMonstersKilled) },
            { AchievementManager.AchievementType.Deaths , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.Deaths) },
            { AchievementManager.AchievementType.EikthyrKills , totalMonstersKilled["defeated_eikthyr"] },
            { AchievementManager.AchievementType.ElderKills , totalMonstersKilled["defeated_gdking"] },
            { AchievementManager.AchievementType.BonemassKills , totalMonstersKilled["defeated_bonemass"]},
            { AchievementManager.AchievementType.ModerKills , totalMonstersKilled["defeated_dragon"] },
            { AchievementManager.AchievementType.YagluthKills , totalMonstersKilled["defeated_goblinking"] },
            { AchievementManager.AchievementType.QueenKills , totalMonstersKilled["defeated_queen"] },
            { AchievementManager.AchievementType.DistanceRan , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceRun)},
            { AchievementManager.AchievementType.DistanceSailed , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceSail) },
            { AchievementManager.AchievementType.TotalKills , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.EnemyKills)},
            { AchievementManager.AchievementType.TotalAchievements , registeredAchievements.FindAll(x => x.isCompleted).Count},
            { AchievementManager.AchievementType.TrollKills , totalMonstersKilled["KilledTroll"]},
            { AchievementManager.AchievementType.SerpentKills , totalMonstersKilled["defeated_serpent"]},
            { AchievementManager.AchievementType.CultistKills , totalMonstersKilled["defeated_fenring_cultist"]},
            { AchievementManager.AchievementType.StoneGolemKills , totalMonstersKilled["defeated_stonegolem"]},
            { AchievementManager.AchievementType.TarBlobKills , totalMonstersKilled["defeated_blobtar"]},
            { AchievementManager.AchievementType.DeathByFall , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByFall)},
            { AchievementManager.AchievementType.TreesChopped , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.Tree)},
            { AchievementManager.AchievementType.DeathByTree , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByTree)},
            { AchievementManager.AchievementType.DeathByEdgeOfWorld , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld)},
            { AchievementManager.AchievementType.TimeInBase , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.TimeInBase)},
            { AchievementManager.AchievementType.TimeOutOfBase , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.TimeOutOfBase)},
            { AchievementManager.AchievementType.ArrowsShot , (int)TrackPlayerStats.GetPlayerStat(PlayerStatType.ArrowsShot)},
            { AchievementManager.AchievementType.GoblinShamanKills , totalMonstersKilled["defeated_goblinshaman"]},
            { AchievementManager.AchievementType.WraithKills , totalMonstersKilled["defeated_wraith"]},
            { AchievementManager.AchievementType.DrakeKills , totalMonstersKilled.TryGetValue("defeated_hatchling", out int drakeKills) ? drakeKills : 0},
            { AchievementManager.AchievementType.GhostKills , totalMonstersKilled.TryGetValue("defeated_ghost", out int ghostKills) ? ghostKills : 0},
            { AchievementManager.AchievementType.FenringKills , totalMonstersKilled.TryGetValue("defeated_fenring", out int fenringKills) ? fenringKills : 0}
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
            int killCount = totalMonstersKilled[defeatKey];
            if (killCount > 0) totalKills += 1;
        }

        return totalKills;
    }
}