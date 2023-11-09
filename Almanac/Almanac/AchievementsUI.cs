using System.Collections.Generic;
using System.Linq;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Almanac.Almanac.AchievementManager;
using static Almanac.Almanac.AchievementManager.AchievementType;
using static Almanac.Almanac.CreatureDataCollector;
using static Almanac.Almanac.ItemDataCollector;
using static Almanac.Almanac.PieceDataCollector;
using static Almanac.Almanac.TrackPlayerStats;
using static Almanac.AlmanacPlugin;

namespace Almanac.Almanac;

public static class AchievementsUI
{
    public static readonly List<Achievement> registeredAchievements = new();

    public const int maxPowers = 5;

    public static int powerLimit = 3;
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
        public string defeatKey = null!;
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
        AchievementType type,
        string defeatKey = "")
    {
        if (powerName.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace()) return;
        if (!sprite) sprite = AlmanacIconButton;
        
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
            type = type,
            defeatKey = defeatKey
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
                case MeadowCreatures: m_goal = StringToListDefeatKeys(_MeadowCreatures.Value).Count; break;
                case BlackForestCreatures: m_goal = StringToListDefeatKeys(_BlackForestCreatures.Value).Count; break;
                case SwampCreatures: m_goal = StringToListDefeatKeys(_SwampCreatures.Value).Count; break;
                case MountainCreatures: m_goal = StringToListDefeatKeys(_MountainCreatures.Value).Count; break;
                case PlainsCreatures: m_goal = StringToListDefeatKeys(_PlainsCreatures.Value).Count; break;
                case MistLandCreatures: m_goal = StringToListDefeatKeys(_MistLandCreatures.Value).Count; break;
                case TotalAchievements: m_goal = tempAchievements.Count - 1; break;
                case ComfortPieces: m_goal = comfortPieces.Count; break;
                case AshLandCreatures: m_goal = StringToListDefeatKeys(_AshLandsCreatures.Value).Count; break;
                case DeepNorthCreatures: m_goal = StringToListDefeatKeys(_DeepNorthCreatures.Value).Count ;break;
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
                type: achievement.m_type,
                defeatKey: achievement.m_defeatKey
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

        foreach (KeyValuePair<string, int> kvp in dict1)
        {
            if (combinedDict.ContainsKey(kvp.Key)) combinedDict[kvp.Key] += kvp.Value;
            else combinedDict[kvp.Key] = kvp.Value;
        }

        foreach (KeyValuePair<string, int> kvp in dict2)
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
            { MeadowCreatures , GetBiomeKills(StringToListDefeatKeys(_MeadowCreatures.Value), totalMonstersKilled) },
            { BlackForestCreatures , GetBiomeKills(StringToListDefeatKeys(_BlackForestCreatures.Value), totalMonstersKilled) },
            { SwampCreatures , GetBiomeKills(StringToListDefeatKeys(_SwampCreatures.Value), totalMonstersKilled) },
            { MountainCreatures , GetBiomeKills(StringToListDefeatKeys(_MountainCreatures.Value), totalMonstersKilled) },
            { PlainsCreatures , GetBiomeKills(StringToListDefeatKeys(_PlainsCreatures.Value), totalMonstersKilled)},
            { MistLandCreatures , GetBiomeKills(StringToListDefeatKeys(_MistLandCreatures.Value), totalMonstersKilled) },
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
            { BossKills , (int)GetPlayerStat(PlayerStatType.BossKills)},
            { CustomKills, 0 }
        };
        
        foreach (Achievement achievement in registeredAchievements)
        {
            int value = updateAchievementMap[achievement.type];
            switch (achievement.type)
            {
                case CustomKills:
                    if (!totalMonstersKilled.ContainsKey(achievement.defeatKey))
                    {
                        AlmanacLogger.LogInfo($"Failed to find key: {achievement.defeatKey}");
                        break;
                    }
                    totalMonstersKilled.TryGetValue(achievement.defeatKey, out int customValue);
                    value = customValue;
                    break;
            }
            achievement.isCompleted = value >= achievement.total;
            achievement.value = value;
        }
        
    }

    private static int GetBiomeKills(List<string> creatureList, Dictionary<string, int> totalMonstersKilled)
    {
        int totalKills = 0;
        foreach (string? defeatKey in creatureList)
        {
            int killCount = totalMonstersKilled[defeatKey];
            if (killCount > 0) totalKills += 1;
        }

        return totalKills;
    }
}