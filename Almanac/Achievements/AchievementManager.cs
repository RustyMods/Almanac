using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Data;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static Almanac.Achievements.AlmanacEffectManager;
using static Almanac.Data.ItemDataCollector;

namespace Almanac.Achievements;

public static class AchievementManager
{
    public static List<Achievement> AchievementList = new();
    public static readonly List<AchievementYML.AchievementData> AchievementData = new();
    public class Achievement
    {
        public EffectData m_effectData = null!;
        public string m_uniqueName = null!;
        public string m_displayName = null!;
        public int m_goal;
        public AchievementTypes.AchievementType m_type;
        public string? m_desc;
        public Sprite? m_sprite;
        public string? m_spriteName;
        public string m_lore = "";
        public string m_defeatKey = "";
        public bool m_isCompleted;
        public StatusEffect? m_statusEffect;
    }
    public static void OnAchievementConfigChanged(object sender, EventArgs e)
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Achievement configs changed, reloading achievements");
        ActiveAchievementEffects.Clear();
        if (ServerSyncedData.InitiatedServerAchievements)
        {
            ServerSyncedData.OnServerAchievementChanged();
        }
        else
        {
            InitAchievements(LoadAchievementData(AchievementData), false);
        }
    }

    public static bool AchievementsRan = false;
    public static void InitAchievements(List<Achievement> list, bool checkBool = true)
    {
        if (checkBool && AchievementsRan) return;
        if (!ObjectDB.instance) return;
        
        if (Player.m_localPlayer)
        {
            List<StatusEffect> EffectsToRemove = new();
            foreach (StatusEffect SE in Player.m_localPlayer.GetSEMan().GetStatusEffects())
            {
                if (SE is AchievementEffect)
                {
                    EffectsToRemove.Add(SE);
                }
            }

            foreach (StatusEffect SE in EffectsToRemove)
            {
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect(SE, true);
            }
        }
        
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is AchievementEffect);
        AchievementList.Clear();

        AchievementList = list;
        
        if (checkBool) AchievementsRan = true;
    }

    public static void CheckCompletedAchievements()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Setting achievement completions");
        foreach (Achievement achievement in AchievementList)
        {
            SetCompletedAchievement(achievement);
        }
    }

    private static void SetCompletedAchievement(Achievement data)
    {
        switch (data.m_type)
        {
            case AchievementTypes.AchievementType.Deaths:
                SetCompleted(data, PlayerStatType.Deaths);
                break;
            case AchievementTypes.AchievementType.Fish:
                SetCompleted(data, GetFishes());
                break;
            case AchievementTypes.AchievementType.Materials:
                SetCompleted(data, GetMaterials());
                break;
            case AchievementTypes.AchievementType.Consumables:
                SetCompleted(data, GetConsumables());
                break;
            case AchievementTypes.AchievementType.Weapons:
                SetCompleted(data, GetWeapons());
                break;
            case AchievementTypes.AchievementType.Swords:
                SetCompleted(data, GetSwords());
                break;
            case AchievementTypes.AchievementType.Axes:
                SetCompleted(data, GetAxes());
                break;
            case AchievementTypes.AchievementType.PoleArms:
                SetCompleted(data, GetPoleArms());
                break;
            case AchievementTypes.AchievementType.Spears:
                SetCompleted(data, GetSpears());
                break;
            case AchievementTypes.AchievementType.Maces:
                SetCompleted(data, GetMaces());
                break;
            case AchievementTypes.AchievementType.Knives:
                SetCompleted(data,GetKnives());
                break;
            case AchievementTypes.AchievementType.Shields:
                SetCompleted(data,GetShields());
                break;
            case AchievementTypes.AchievementType.Staves:
                SetCompleted(data,GetStaves());
                break;
            case AchievementTypes.AchievementType.Arrows:
                SetCompleted(data, GetAmmunition());
                break;
            case AchievementTypes.AchievementType.Bows:
                SetCompleted(data, GetBows());
                break;
            case AchievementTypes.AchievementType.Valuables:
                SetCompleted(data, GetValuables());
                break;
            case AchievementTypes.AchievementType.Potions:
                SetCompleted(data, GetPotions());
                break;
            case AchievementTypes.AchievementType.Trophies:
                SetCompleted(data, GetTrophies());
                break;
            case AchievementTypes.AchievementType.EnemyKills:
                SetCompleted(data, PlayerStatType.EnemyKills);
                break;
            case AchievementTypes.AchievementType.DeathByFall:
                SetCompleted(data, PlayerStatType.DeathByFall);
                break;
            case AchievementTypes.AchievementType.TreesChopped:
                SetCompleted(data, PlayerStatType.Tree);
                break;
            case AchievementTypes.AchievementType.DeathByTree:
                SetCompleted(data, PlayerStatType.DeathByTree);
                break;
            case AchievementTypes.AchievementType.DeathByEdgeOfWorld:
                SetCompleted(data, PlayerStatType.DeathByEdgeOfWorld);
                break;
            case AchievementTypes.AchievementType.TimeInBase:
                SetCompleted(data, PlayerStatType.TimeInBase);
                break;
            case AchievementTypes.AchievementType.TimeOutOfBase:
                SetCompleted(data, PlayerStatType.TimeOutOfBase);
                break;
            case AchievementTypes.AchievementType.ArrowsShot:
                SetCompleted(data, PlayerStatType.ArrowsShot);
                break;
            case AchievementTypes.AchievementType.TotalJumps:
                SetCompleted(data, PlayerStatType.Jumps);
                break;
            case AchievementTypes.AchievementType.TotalBuilds:
                SetCompleted(data, PlayerStatType.Builds);
                break;
            case AchievementTypes.AchievementType.EnemyHits:
                SetCompleted(data, PlayerStatType.EnemyHits);
                break;
            case AchievementTypes.AchievementType.PlayerKills:
                SetCompleted(data, PlayerStatType.PlayerKills);
                break;
            case AchievementTypes.AchievementType.HitsTaken:
                SetCompleted(data, PlayerStatType.HitsTakenEnemies);
                break;
            case AchievementTypes.AchievementType.ItemsPicked:
                SetCompleted(data, PlayerStatType.ItemsPickedUp);
                break;
            case AchievementTypes.AchievementType.DistanceWalked:
                SetCompleted(data, PlayerStatType.DistanceWalk);
                break;
            case AchievementTypes.AchievementType.DistanceInAir:
                SetCompleted(data, PlayerStatType.DistanceAir);
                break;
            case AchievementTypes.AchievementType.DistanceRan:
                SetCompleted(data, PlayerStatType.DistanceRun);
                break;
            case AchievementTypes.AchievementType.DistanceSailed:
                SetCompleted(data, PlayerStatType.DistanceSail);
                break;
            case AchievementTypes.AchievementType.MineHits:
                SetCompleted(data, PlayerStatType.MineHits);
                break;
            case AchievementTypes.AchievementType.TotalMined:
                SetCompleted(data, PlayerStatType.Mines);
                break;
            case AchievementTypes.AchievementType.CreatureTamed:
                SetCompleted(data, PlayerStatType.CreatureTamed);
                break;
            case AchievementTypes.AchievementType.FoodEaten:
                SetCompleted(data, PlayerStatType.FoodEaten);
                break;
            case AchievementTypes.AchievementType.SkeletonSummoned:
                SetCompleted(data, PlayerStatType.SkeletonSummons);
                break;
            case AchievementTypes.AchievementType.DeathByDrowning:
                SetCompleted(data, PlayerStatType.DeathByDrowning);
                break;
            case AchievementTypes.AchievementType.DeathByBurning:
                SetCompleted(data, PlayerStatType.DeathByBurning);
                break;
            case AchievementTypes.AchievementType.DeathByFreezing:
                SetCompleted(data, PlayerStatType.DeathByFreezing);
                break;
            case AchievementTypes.AchievementType.DeathByPoisoned:
                SetCompleted(data, PlayerStatType.DeathByPoisoned);
                break;
            case AchievementTypes.AchievementType.DeathBySmoke:
                SetCompleted(data, PlayerStatType.DeathBySmoke);
                break;
            case AchievementTypes.AchievementType.DeathByStalagtite:
                SetCompleted(data, PlayerStatType.DeathByStalagtite);
                break;
            case AchievementTypes.AchievementType.BeesHarvested:
                SetCompleted(data, PlayerStatType.BeesHarvested);
                break;
            case AchievementTypes.AchievementType.SapHarvested:
                SetCompleted(data, PlayerStatType.SapHarvested);
                break;
            case AchievementTypes.AchievementType.TrapsArmed:
                SetCompleted(data, PlayerStatType.TrapArmed);
                break;
            case AchievementTypes.AchievementType.RuneStones:
                int loreCount = PlayerStats.GetKnownTextCount();
                data.m_isCompleted = loreCount >= data.m_goal;
                break;
            case AchievementTypes.AchievementType.Recipes:
                int recipeCount = PlayerStats.GetKnownRecipeCount();
                data.m_isCompleted = recipeCount >= data.m_goal;
                break;
            case AchievementTypes.AchievementType.CustomKills:
                if (PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(data.m_defeatKey, out KillDeaths value))
                {
                    int kills = value.kills;
                    data.m_isCompleted = kills >= data.m_goal;
                }
                break;
            case AchievementTypes.AchievementType.MeadowCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.Meadows));
                break;
            case AchievementTypes.AchievementType.BlackForestCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.BlackForest));
                break;
            case AchievementTypes.AchievementType.SwampCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.Swamp));
                break;
            case AchievementTypes.AchievementType.MountainCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.Mountain));
                break;
            case AchievementTypes.AchievementType.PlainsCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.Plains));
                break;
            case AchievementTypes.AchievementType.MistLandCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.Mistlands));
                break;
            case AchievementTypes.AchievementType.DeepNorthCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.DeepNorth));
                break;
            case AchievementTypes.AchievementType.AshLandCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.AshLands));
                break;
            case AchievementTypes.AchievementType.OceanCreatures:
                SetCompleted(data, CreatureLists.GetBiomeCreatures(Heightmap.Biome.Ocean));
                break;
            default:
                data.m_isCompleted = false;
                break;
        }
    }

    private static void SetCompleted(Achievement achievement, List<CreatureDataCollector.CreatureData> list)
    {
        ZoneSystem? Zone = ZoneSystem.instance;
        if (!Zone) return;
        List<string> globalKeys = Zone.GetGlobalKeys();
        int count = list.Count(critter => globalKeys.Contains(critter.defeatedKey) || Zone.GetGlobalKey(critter.defeatedKey));
        achievement.m_isCompleted = count >= list.Count;
    }
    private static void SetCompleted(Achievement achievement, PlayerStatType type)
    {
        achievement.m_isCompleted = PlayerStats.GetPlayerStat(type) > achievement.m_goal;
    }

    private static void SetCompleted(Achievement achievement, List<ItemDrop> list)
    {
        achievement.m_isCompleted =
            list.FindAll(item => Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name)).Count >=
            list.Count;
    }

    public static void ReadAchievements()
    {
        AchievementData.Clear();
        string[] filePaths = Directory.GetFiles(AlmanacPaths.AchievementFolderPath, "*.yml");

        IDeserializer deserializer = new DeserializerBuilder().Build();
        
        foreach (string path in filePaths)
        {
            try
            {
                string data = File.ReadAllText(path);
                AchievementYML.AchievementData YmlData = deserializer.Deserialize<AchievementYML.AchievementData>(data);
                AchievementData.Add(YmlData);
            }
            catch (Exception)
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Failed to load yml data: " + path);
            }
        }
    }

    public static List<Achievement> LoadAchievementData(List<AchievementYML.AchievementData> data)
    {
        List<Achievement> output = new();
        if (!ZNet.instance) return output;

        foreach (AchievementYML.AchievementData YmlData in data)
        {
            Achievement achievement = CreateAchievement(
                UniqueName: YmlData.unique_name,
                DisplayName: YmlData.display_name,
                SpriteName: YmlData.sprite_name,
                Desc: YmlData.description,
                Lore: YmlData.lore,
                StartMsg: YmlData.start_message,
                StopMsg: YmlData.stop_message,
                Tooltip: YmlData.tooltip,
                DefeatKey: YmlData.defeat_key,
                Goal: YmlData.goal,
                Type: YmlData.achievement_type,
                Duration: YmlData.duration,
                StartEffects: YmlData.start_effects.ToArray(),
                StopEffects: YmlData.stop_effects.ToArray(),
                DamageMods: YmlData.damage_modifiers.ToArray(),
                Modifiers: YmlData.modifiers
            );
            output.Add(achievement);
        }

        return output;
    }

    private static Achievement CreateAchievement(
        string UniqueName, 
        string DisplayName,
        string SpriteName = "",
        string Desc = "", 
        string Lore = "", 
        string StartMsg = "", 
        string StopMsg = "", 
        string Tooltip = "",
        string DefeatKey = "",
        int Goal = 0,
        Sprite? Sprite = null,
        AchievementTypes.AchievementType Type = AchievementTypes.AchievementType.None,
        int Duration = 0,
        string[]? StartEffects = null,
        string[]? StopEffects = null,
        HitData.DamageModPair[] DamageMods = null!,
        Dictionary<Modifier, float> Modifiers = null!)
    {
        Achievement achievement = new Achievement()
        {
            m_uniqueName = UniqueName,
            m_displayName = DisplayName,
            m_spriteName = SpriteName,
            m_desc = Desc,
            m_lore = Lore,
            m_defeatKey = DefeatKey,
            m_goal = Goal,
            m_sprite = Sprite,
            m_type = Type,
        };
        achievement.m_effectData = new()
        {
            duration = Duration,
            startEffectNames = StartEffects,
            stopEffectNames = StopEffects,
            startMsg = StartMsg,
            stopMsg = StopMsg,
            effectTooltip = Tooltip,
            damageMods = DamageMods.ToList(),
            Modifiers = Modifiers,
            effectName = achievement.m_uniqueName,
            displayName = achievement.m_displayName,
            sprite = achievement.m_sprite
        };;

        if (!achievement.m_spriteName.IsNullOrWhiteSpace())
        {
            if (SpriteManager.GetSprite(achievement.m_spriteName, out Sprite? sprite))
            {
                achievement.m_sprite = sprite;
                achievement.m_effectData.sprite = sprite;
            }
            else
            {
                GameObject? prefab = ObjectDB.instance.GetItemPrefab(achievement.m_spriteName);
                if (prefab)
                {
                    if (prefab.TryGetComponent(out ItemDrop itemDrop))
                    {
                        Sprite? icon = itemDrop.m_itemData.GetIcon();
                        achievement.m_sprite = icon;
                        achievement.m_effectData.sprite = icon;
                    };
                }
            }
        };
        
        achievement.m_statusEffect = achievement.m_effectData.Init();
        return achievement;
    }
    
}