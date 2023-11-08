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
using static Almanac.Almanac.AchievementManager;
using static Almanac.Almanac.AchievementManager.AchievementType;
using static Almanac.Almanac.Commands;
using static Almanac.Almanac.CreatureDataCollector;
using static Almanac.Almanac.ItemDataCollector;
using static Almanac.Almanac.PieceDataCollector;
using static Almanac.Almanac.TrackPlayerStats;
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
    public class Achievement
    {
        public AchievementType type;
        public string name = null!;
        public string? description = null!;
        public int total;
        public int value;
        public Sprite? sprite;
        public bool isCompleted;
        public string lore = null!;
        public string power = null!;
        public string powerToolTip = null!;
    }

    private static Sprite? GetSprite(string? prefabName)
    {
        if (prefabName.IsNullOrWhiteSpace()) return null;
        ZNetScene scene = ZNetScene.instance;
        GameObject prefab = scene.GetPrefab(prefabName);
        if (!prefab) return null;
        prefab.TryGetComponent(out ItemDrop itemDrop);
        return itemDrop ? itemDrop.m_itemData.GetIcon() : null;
    }
    private static void CreateAchievement(
        string name, string? description,
        int goal, Sprite? sprite,
        string lore, string powerName,
        string powerToolTip,
        AchievementType type)
    {
        if (powerName.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace()) return;
        if (!sprite) sprite = AlmanacPlugin.AlmanacIconButton;
        
        Achievement data = new Achievement()
        {
            name = name,
            description = description,
            total = goal,
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
        // Make sure temp achievements has data or else do not clear the currently registered achievements
        // We do this because when the player logs out, it does not re-initialize the reading of the achievements,
        // Therefore temp achievements ends up becoming empty as they have already been registered.
        if (tempAchievements.Count == 0) return;
        registeredAchievements.Clear();
        
        // Register achievements to the UI
        
        foreach (AchievementManager.Achievement achievement in tempAchievements)
        {
            int m_goal = 0;

            // Conversion map to get total values to set
            switch (achievement.m_type)
            {
                case AchievementType.Fish: m_goal = GetFishes().Count; break;
                case Materials: m_goal = GetMaterials().Count; break;
                case Consumables: m_goal = GetConsumables().Count; break;
                case Weapons: m_goal = GetWeapons().Count; break;
                case Swords: m_goal = GetWeapons().FindAll(item => item.name.Contains("Sword")).Count; break;
                case Axes: m_goal = GetWeapons().FindAll(item => item.name.Contains("Axe")).Count; break;
                case PoleArms: m_goal = GetWeapons().FindAll(item => item.name.Contains("Atgeir")).Count; break;
                case Spears: m_goal = GetWeapons().FindAll(item => item.name.Contains("Spear")).Count; break;
                case Maces: m_goal = GetWeapons().FindAll(item => item.name.Contains("Mace")).Count; break;
                case Knives: m_goal = GetWeapons().FindAll(item => item.name.Contains("Knife")).Count; break;
                case Shields: m_goal = GetWeapons().FindAll(item => item.name.Contains("Shield")).Count; break;
                case Staves: m_goal = GetWeapons().FindAll(item => item.name.Contains("Staff")).Count; break;
                case Arrows: m_goal = GetAmmunition().FindAll(projectile => projectile.name.Contains("Arrow")).Count; break;
                case Bows: m_goal = GetWeapons().FindAll(weapon => weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow")).Count; break;
                case Valuables: m_goal = GetMaterials().FindAll(item => item.m_itemData.m_shared.m_value > 0).Count; break;
                case Potions: m_goal = GetMaterials().FindAll(item => item.name.Contains("Base")).Count; break;
                case Trophies: m_goal = GetTrophies().Count; break;
                case Creatures: m_goal = tempCreatureData.Count; break;
                case MeadowCreatures: m_goal = meadowCreatures.Count; break;
                case BlackForestCreatures: m_goal = blackForestCreatures.Count; break;
                case SwampCreatures: m_goal = swampCreatures.Count; break;
                case MountainCreatures: m_goal = mountainCreatures.Count; break;
                case PlainsCreatures: m_goal = plainsCreatures.Count; break;
                case MistLandCreatures: m_goal = mistLandCreatures.Count; break;
                case TotalAchievements: m_goal = tempAchievements.Count - 1; break;
                case ComfortPieces: m_goal = comfortPieces.Count; break;
                case AshLandCreatures: break;
                case DeepNorthCreatures: break;
                default: m_goal = achievement.m_goal; break; // The rest are threshold achievement
            }

            CreateAchievement(
                name: achievement.m_displayName,
                description: achievement.m_desc,
                goal: m_goal,
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
            if (!container) continue;
            Transform icon = container.Find("iconObj");
            Transform hoverText = container.Find("hoverTextElement");
            Transform glowContainer = container.Find($"glow ({i})");
            
            icon.TryGetComponent(out Image containerImage);
            hoverText.TryGetComponent(out TextMeshProUGUI textMesh);

            if (!containerImage || !textMesh) return;

            Achievement achievement = registeredAchievements[i];

            string localizedAchievementName = Localization.instance.Localize(achievement.name);

            containerImage.color = Player.m_localPlayer.NoCostCheat() ? Color.white : achievement.isCompleted ? Color.white : Color.black;
            textMesh.text = Player.m_localPlayer.NoCostCheat() ? localizedAchievementName : achievement.isCompleted ? localizedAchievementName : "???";

            glowContainer.gameObject.SetActive(CustomStatusEffects.activeAlmanacEffects.Find(x => x.name == achievement.power));
        }
    }

    public static void SetAchievementGlow()
    {
        Transform? panel = Patches.OnOpenTrophiesPatch.trophyFrame.Find("achievementsPanel");

        for (int i = 0; i < registeredAchievements.Count; ++i)
        {
            Transform container = panel.Find($"achievementsContainer ({i})");
            Transform glowContainer = container.Find($"glow ({i})");
            Achievement achievement = registeredAchievements[i];

            glowContainer.gameObject.SetActive(CustomStatusEffects.activeAlmanacEffects.Find(x => x.name == achievement.power));
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

        Dictionary<AchievementType, int> updateAchievementMap = new()
        {
            { AchievementType.Fish , GetFishes().Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { Materials , GetMaterials().Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { Consumables , GetConsumables().Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { Weapons , GetWeapons().Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { Swords , GetWeapons().FindAll(item => item.name.Contains("Sword")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Axes , GetWeapons().FindAll(item => item.name.Contains("Axe")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { PoleArms , GetWeapons().FindAll(item => item.name.Contains("Atgeir")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Spears , GetWeapons().FindAll(item => item.name.Contains("Spear")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Maces , GetWeapons().FindAll(item => item.name.Contains("Mace")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Knives , GetWeapons().FindAll(item => item.name.Contains("Knife")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Shields , GetWeapons().FindAll(item => item.name.Contains("Shield")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Staves , GetWeapons().FindAll(item => item.name.Contains("Staff")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name))},
            { Arrows , GetAmmunition().FindAll(projectile => projectile.name.Contains("Arrow")).Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { Bows , GetWeapons().FindAll(weapon => weapon.name.Contains("Bow") || weapon.name.Contains("Crossbow")).Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { Valuables , GetMaterials().FindAll(item => item.m_itemData.m_shared.m_value > 0).Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { Potions , GetMaterials().FindAll(item => item.name.Contains("Mead")).Count(item => player.IsKnownMaterial(item.m_itemData.m_shared.m_name)) },
            { Trophies , GetTrophies().Count(item => player.IsMaterialKnown(item.m_itemData.m_shared.m_name)) },
            { MeadowCreatures , GetBiomeKills(meadowCreatures, totalMonstersKilled) },
            { BlackForestCreatures , GetBiomeKills(blackForestCreatures, totalMonstersKilled) },
            { SwampCreatures , GetBiomeKills(swampCreatures, totalMonstersKilled) },
            { MountainCreatures , GetBiomeKills(mountainCreatures, totalMonstersKilled) },
            { PlainsCreatures , GetBiomeKills(plainsCreatures, totalMonstersKilled)},
            { MistLandCreatures , GetBiomeKills(mistLandCreatures, totalMonstersKilled) },
            { Deaths , (int)GetPlayerStat(PlayerStatType.Deaths) },
            { EikthyrKills , totalMonstersKilled["defeated_eikthyr"] },
            { ElderKills , totalMonstersKilled["defeated_gdking"] },
            { BonemassKills , totalMonstersKilled["defeated_bonemass"]},
            { ModerKills , totalMonstersKilled["defeated_dragon"] },
            { YagluthKills , totalMonstersKilled["defeated_goblinking"] },
            { QueenKills , totalMonstersKilled["defeated_queen"] },
            { DistanceRan , (int)GetPlayerStat(PlayerStatType.DistanceRun)},
            { DistanceSailed , (int)GetPlayerStat(PlayerStatType.DistanceSail) },
            { TotalKills , (int)GetPlayerStat(PlayerStatType.EnemyKills)},
            { TotalAchievements , registeredAchievements.FindAll(x => x.isCompleted).Count},
            { TrollKills , totalMonstersKilled["KilledTroll"]},
            { SerpentKills , totalMonstersKilled["defeated_serpent"]},
            { CultistKills , totalMonstersKilled["defeated_fenring_cultist"]},
            { StoneGolemKills , totalMonstersKilled["defeated_stonegolem"]},
            { TarBlobKills , totalMonstersKilled["defeated_blobtar"]},
            { DeathByFall , (int)GetPlayerStat(PlayerStatType.DeathByFall)},
            { TreesChopped , (int)GetPlayerStat(PlayerStatType.Tree)},
            { DeathByTree , (int)GetPlayerStat(PlayerStatType.DeathByTree)},
            { DeathByEdgeOfWorld , (int)GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld)},
            { TimeInBase , (int)GetPlayerStat(PlayerStatType.TimeInBase)},
            { TimeOutOfBase , (int)GetPlayerStat(PlayerStatType.TimeOutOfBase)},
            { ArrowsShot , (int)GetPlayerStat(PlayerStatType.ArrowsShot)},
            { GoblinShamanKills , totalMonstersKilled["defeated_goblinshaman"]},
            { WraithKills , totalMonstersKilled["defeated_wraith"]},
            { DrakeKills , totalMonstersKilled.TryGetValue("defeated_hatchling", out int drakeKills) ? drakeKills : 0},
            { GhostKills , totalMonstersKilled.TryGetValue("defeated_ghost", out int ghostKills) ? ghostKills : 0},
            { FenringKills , totalMonstersKilled.TryGetValue("defeated_fenring", out int fenringKills) ? fenringKills : 0},
            { ComfortPieces , comfortPieces.Count(x => player.IsMaterialKnown(x.name))},
            { GreydwarfShamanKills , totalMonstersKilled.TryGetValue("defeated_greydwarf_shaman", out int greydwarfShamanKills) ? greydwarfShamanKills : 0},
            { DvergerKills , totalMonstersKilled.TryGetValue("defeated_dverger", out int dvergerKills) ? dvergerKills : 0},
            { DvergerFireKills , totalMonstersKilled.TryGetValue("defeated_dvergermagefire", out int dvergerFireKills) ? dvergerFireKills : 0},
            { DvergerFrostKills , totalMonstersKilled.TryGetValue("defeated_dvergermageice" ,out int dvergerFrostKills) ? dvergerFrostKills : 0},
            { DvergerSupportKills , totalMonstersKilled.TryGetValue("defeated_dvergermagesupport", out int dvergerSupportKills) ? dvergerSupportKills : 0},
            { TotalJumps , (int)GetPlayerStat(PlayerStatType.Jumps)},
            { TotalCraftsOrUpgrades , (int)GetPlayerStat(PlayerStatType.CraftsOrUpgrades)},
            { TotalBuilds , (int)GetPlayerStat(PlayerStatType.Builds)},
            { EnemyHits , (int)GetPlayerStat(PlayerStatType.EnemyHits)},
            { PlayerKills , (int)GetPlayerStat(PlayerStatType.PlayerKills)},
            { HitsTaken , (int)GetPlayerStat(PlayerStatType.HitsTakenEnemies) + (int)GetPlayerStat(PlayerStatType.HitsTakenPlayers)},
            { ItemsPicked , (int)GetPlayerStat(PlayerStatType.ItemsPickedUp)},
            { DistanceWalked , (int)GetPlayerStat(PlayerStatType.DistanceWalk)},
            { DistanceInAir , (int)GetPlayerStat(PlayerStatType.DistanceAir)},
            { MineHits , (int)GetPlayerStat(PlayerStatType.MineHits)},
            { TotalMined , (int)GetPlayerStat(PlayerStatType.Mines)},
            { CreatureTamed , (int)GetPlayerStat(PlayerStatType.CreatureTamed)},
            { FoodEaten , (int)GetPlayerStat(PlayerStatType.FoodEaten)},
            { SkeletonSummoned , (int)GetPlayerStat(PlayerStatType.SkeletonSummons)},
            { DeathByDrowning , (int)GetPlayerStat(PlayerStatType.DeathByDrowning)},
            { DeathByBurning , (int)GetPlayerStat(PlayerStatType.DeathByBurning)},
            { DeathByFreezing , (int)GetPlayerStat(PlayerStatType.DeathByFreezing)},
            { DeathByPoisoned , (int)GetPlayerStat(PlayerStatType.DeathByPoisoned)},
            { DeathBySmoke, (int)GetPlayerStat(PlayerStatType.DeathBySmoke)},
            { DeathByCart , (int)GetPlayerStat(PlayerStatType.DeathByCart)},
            { DeathBySelf , (int)GetPlayerStat(PlayerStatType.DeathBySelf)},
            { DeathByStalagtite , (int)GetPlayerStat(PlayerStatType.DeathByStalagtite)},
            { BeesHarvested , (int)GetPlayerStat(PlayerStatType.BeesHarvested)},
            { SapHarvested , (int)GetPlayerStat(PlayerStatType.SapHarvested)},
            { TrapsArmed , (int)GetPlayerStat(PlayerStatType.TrapArmed)},
            { StacksPlaced , (int)GetPlayerStat(PlayerStatType.PlaceStacks)},
            { BossKills , (int)GetPlayerStat(PlayerStatType.BossKills)}

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