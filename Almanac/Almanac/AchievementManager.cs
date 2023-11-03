using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using UnityEngine.Serialization;
using YamlDotNet.Serialization;

namespace Almanac.Almanac;

public static class AchievementManager
{
    // public static readonly CustomSyncedValue<List<AchievementData>> serverAchievements =
    //     new(AlmanacPlugin.ConfigSync, "ServerAchievements", new());

    public static readonly List<Achievement> tempAchievements = new();
    public class Achievement
    {
        public AlmanacEffectsManager.BaseEffectData m_statusEffect = null!;
        public string m_uniqueName = null!;
        public string m_displayName = null!;
        public int m_goal;
        public AchievementType m_type;
        public string? m_desc = null!;
        public Sprite? m_sprite;
        public string? m_spriteName;
        public string? m_lore;
        public string? m_toolTip;
        public bool isCompleted;
    }

    public enum AchievementType
    {
        None,
        Deaths,
        Fish,
        Materials,
        Consumables,
        Weapons,
        Swords,
        Axes,
        PoleArms,
        Spears,
        Maces,
        Knives,
        Shields,
        Staves,
        Arrows,
        Bows,
        Valuables,
        Potions,
        Trophies,
        Creatures,
        MeadowCreatures,
        BlackForestCreatures,
        SwampCreatures,
        MountainCreatures,
        PlainsCreatures,
        MistLandCreatures,
        AshLandCreatures,
        DeepNorthCreatures,
        EikthyrKills,
        ElderKills,
        BonemassKills,
        ModerKills,
        YagluthKills,
        QueenKills,
        DistanceRan,
        DistanceSailed,
        TotalKills,
        TotalAchievements,
        TrollKills,
        SerpentKills,
        CultistKills,
        StoneGolemKills,
        TarBlobKills,
        DeathByFall,
        TreesChopped,
        DeathByTree,
        DeathByEdgeOfWorld,
        TimeInBase,
        TimeOutOfBase,
        ArrowsShot,
        GoblinShamanKills,
        WraithKills,
        DrakeKills,
        GhostKills,
        FenringKills
    }

    private static void CreateAchievement(
        string uniqueName,
        string name, 
        AchievementType type,
        AlmanacEffectsManager.Modifier modifier,
        Dictionary<AlmanacEffectsManager.Modifier, float> modifiers,
        string[]? startEffects,
        string[]? stopEffects,
        int goal = 0,
        Sprite? sprite = null,
        string? damageModifier = "",
        string? desc = "",
        string? spriteName = "",
        string? lore = "",
        string? toolTip = "",
        string? statusEffectStopMsg = "",
        float newValue = 0f,
        int statusEffectDuration = 0
    )
    {
        string statusEffectName = "se_" + uniqueName;
        switch (modifier)
        {
            case AlmanacEffectsManager.Modifier.EikthyrPower: statusEffectName = "GP_Eikthyr"; break;
            case AlmanacEffectsManager.Modifier.ElderPower: statusEffectName = "GP_TheElder"; break;
            case AlmanacEffectsManager.Modifier.BonemassPower: statusEffectName = "GP_Bonemass"; break;
            case AlmanacEffectsManager.Modifier.ModerPower: statusEffectName = "GP_Moder"; break;
            case AlmanacEffectsManager.Modifier.YagluthPower: statusEffectName = "GP_Yagluth"; break;
            case AlmanacEffectsManager.Modifier.QueenPower: statusEffectName = "GP_Queen"; break;
        }
        
        Achievement data = new Achievement()
        {
            m_statusEffect = new AlmanacEffectsManager.BaseEffectData()
            {
                effectName = statusEffectName,
                displayName = name,
                duration = statusEffectDuration,
                sprite = sprite,
                spriteName = spriteName,
                startEffectNames = startEffects,
                stopEffectNames = stopEffects,
                startMsg = toolTip,
                stopMsg = statusEffectStopMsg,
                effectTooltip = toolTip,
                damageMod = damageModifier,
                Modifier = modifier,
                Modifiers = modifiers,
                m_newValue = newValue
            },
            m_uniqueName = uniqueName,
            m_displayName = name,
            m_desc = desc,
            m_goal = goal,
            m_sprite = sprite,
            m_spriteName = spriteName,
            m_lore = lore,
            m_toolTip = toolTip,
            m_type = type
        };
        
        tempAchievements.Add(data);
    }

    // private static readonly string folderName = "Almanac";
    // private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    // private static readonly string achievementPath = folderPath + Path.DirectorySeparatorChar + "AchievementData";

    public static void InitAchievements()
    {
        // AlmanacPlugin.AlmanacLogger.LogWarning($"starting achievements initialization");
        
        // IDeserializer deserializer = new DeserializerBuilder().Build();
        
        // if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Server)
        // {
        //     if (!Directory.Exists(achievementPath))
        //     {
        //         Directory.CreateDirectory(achievementPath);
        //         
        //         string serializedData = CreateAlmanacAchievements();
        //         File.WriteAllText(
        //             (achievementPath + Path.DirectorySeparatorChar + "AlmanacAchievements.yml"),
        //             serializedData
        //         );
        //     }
        //
        //     string[] filePaths = Directory.GetFiles(achievementPath, "*.yml");
        //
        //     List<AchievementData> newData = new List<AchievementData>();
        //
        //     foreach (string filePath in filePaths)
        //     {
        //         try
        //         {
        //             string serializedData = File.ReadAllText(filePath);
        //             List<AchievementData> data = deserializer.Deserialize<List<AchievementData>>(serializedData);
        //
        //             foreach (var achievement in data)
        //             {
        //                 if (newData.Exists(x => x.unique_name == achievement.unique_name)) continue;
        //                 newData.Add(achievement);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             // Handle exceptions here (e.g., log the error)
        //             Debug.LogWarning($"Error reading file '{filePath}': {ex.Message}");
        //         }
        //     }
        //     
        //     serverAchievements.Value = newData;
        //
        //     FileSystemWatcher achievementWatcher = new FileSystemWatcher(achievementPath)
        //     {
        //         Filter = "*.yml",
        //         EnableRaisingEvents = true,
        //         IncludeSubdirectories = true,
        //         SynchronizingObject = ThreadingHelper.SynchronizingObject,
        //         NotifyFilter = NotifyFilters.LastWrite
        //     };
        //     achievementWatcher.Created += AchievementChanged;
        //     achievementWatcher.Changed += AchievementChanged;
        //     
        //     GetSetAchievementData(serverAchievements.Value);
        //
        // }
        // else
        // {
        //
        //     if (serverAchievements.Value.Count != 0)
        //     {
        //         GetSetAchievementData(serverAchievements.Value);
        //     }
        //     else
            // {
                // string serializedData = CreateAlmanacAchievements();
                // List<AchievementData> deserializedData = deserializer.Deserialize<List<AchievementData>>(serializedData);
                GetSetAchievementData(CreateAlmanacAchievements());
            // }
        // }
    }

    // private static void UpdateAchievementsData() => GetSetAchievementData(serverAchievements.Value);

    // private static void AchievementChanged(object sender, FileSystemEventArgs e)
    // {
    //     if (e.ChangeType is not (WatcherChangeTypes.Changed or WatcherChangeTypes.Deleted or WatcherChangeTypes.Created)) return;
    //
    //     string fName = Path.GetFileName(e.Name);
    //     string serializedData = File.ReadAllText(Path.Combine(achievementPath, fName));
    //     
    //     IDeserializer deserializer = new DeserializerBuilder().Build();
    //     List<AchievementData> deserializedData = deserializer.Deserialize<List<AchievementData>>(serializedData);
    //
    //     foreach (var achievement in deserializedData)
    //     {
    //         if (serverAchievements.Value.Exists(x => x.unique_name == achievement.unique_name))
    //         {
    //             var match = serverAchievements.Value.Find(x => x.unique_name == achievement.unique_name);
    //             serverAchievements.Value.Remove(match);
    //         }
    //         serverAchievements.Value.Add(achievement);
    //         AlmanacPlugin.AlmanacLogger.LogWarning($"adding {achievement.unique_name} to server achievements");
    //     }
    // }

    private static void GetSetAchievementData(List<AchievementData> deserializedData)
    {
        tempAchievements.Clear();
        // AlmanacPlugin.AlmanacLogger.LogWarning($"Reading {deserializedData.Count} achievements..." );
        foreach (AchievementData achievement in deserializedData)
        {
            AlmanacEffectsManager.Modifier mod = AlmanacEffectsManager.Modifier.None;
            Dictionary<AlmanacEffectsManager.Modifier, float> modifiers = new()
            {
                { AlmanacEffectsManager.Modifier.Attack, 1f },
                { AlmanacEffectsManager.Modifier.HealthRegen, 1f },
                { AlmanacEffectsManager.Modifier.StaminaRegen, 1f },
                { AlmanacEffectsManager.Modifier.RaiseSkills, 1f },
                { AlmanacEffectsManager.Modifier.Speed, 1f },
                { AlmanacEffectsManager.Modifier.Noise, 1f },
                { AlmanacEffectsManager.Modifier.MaxCarryWeight, 0f },
                { AlmanacEffectsManager.Modifier.Stealth, 1f },
                { AlmanacEffectsManager.Modifier.RunStaminaDrain, 1f },
                { AlmanacEffectsManager.Modifier.DamageReduction, 0f },
                { AlmanacEffectsManager.Modifier.FallDamage, 1f }
            };
            Sprite? customSprite = null;
            switch (achievement.modifier)
            {
                case "Attack": mod = AlmanacEffectsManager.Modifier.Attack; modifiers[AlmanacEffectsManager.Modifier.Attack] = achievement.modifier_value; break;
                case "HealthRegen": mod = AlmanacEffectsManager.Modifier.HealthRegen; modifiers[AlmanacEffectsManager.Modifier.HealthRegen] = achievement.modifier_value; break;
                case "StaminaRegen": mod = AlmanacEffectsManager.Modifier.StaminaRegen; modifiers[AlmanacEffectsManager.Modifier.StaminaRegen] = achievement.modifier_value; break;
                case "RaiseSkills": mod = AlmanacEffectsManager.Modifier.RaiseSkills; modifiers[AlmanacEffectsManager.Modifier.RaiseSkills] = achievement.modifier_value; break;
                case "Speed": mod = AlmanacEffectsManager.Modifier.Speed; modifiers[AlmanacEffectsManager.Modifier.Speed] = achievement.modifier_value; break;
                case "Noise": mod = AlmanacEffectsManager.Modifier.Noise; modifiers[AlmanacEffectsManager.Modifier.Noise] = achievement.modifier_value; break;
                case "MaxCarryWeight": mod = AlmanacEffectsManager.Modifier.MaxCarryWeight; modifiers[AlmanacEffectsManager.Modifier.MaxCarryWeight] = achievement.modifier_value; break;
                case "Stealth": mod = AlmanacEffectsManager.Modifier.Stealth; modifiers[AlmanacEffectsManager.Modifier.Stealth] = achievement.modifier_value; break;
                case "RunStaminaDrain": mod = AlmanacEffectsManager.Modifier.RunStaminaDrain; modifiers[AlmanacEffectsManager.Modifier.RunStaminaDrain] = achievement.modifier_value; break;
                case "DamageReduction": mod = AlmanacEffectsManager.Modifier.DamageReduction; modifiers[AlmanacEffectsManager.Modifier.DamageReduction] = achievement.modifier_value; break;
                case "FallDamage": mod = AlmanacEffectsManager.Modifier.FallDamage; modifiers[AlmanacEffectsManager.Modifier.FallDamage] = achievement.modifier_value; break;
                case "BaseHP": mod = AlmanacEffectsManager.Modifier.BaseHP; break;
                case "BaseStamina": mod = AlmanacEffectsManager.Modifier.BaseStamina; break;
                case "MeleeDMG": mod = AlmanacEffectsManager.Modifier.MeleeDMG; break;
                case "RangedDMG": mod = AlmanacEffectsManager.Modifier.RangedDMG; break;
                case "FireDMG": mod = AlmanacEffectsManager.Modifier.FireDMG; break;
                case "FrostDMG": mod = AlmanacEffectsManager.Modifier.FrostDMG; break;
                case "LightningDMG": mod = AlmanacEffectsManager.Modifier.LightningDMG; break;
                case "PoisonDMG": mod = AlmanacEffectsManager.Modifier.PoisonDMG; break;
                case "SpiritDMG": mod = AlmanacEffectsManager.Modifier.SpiritDMG; break;
                case "ChopDMG" : mod = AlmanacEffectsManager.Modifier.ChopDMG; break;
                case "PickaxeDMG" : mod = AlmanacEffectsManager.Modifier.PickaxeDMG; break;
                case "BluntDMG": mod = AlmanacEffectsManager.Modifier.BluntDMG; break;
                case "PierceDMG": mod = AlmanacEffectsManager.Modifier.PierceDMG; break;
                case "SlashDMG": mod = AlmanacEffectsManager.Modifier.SlashDMG; break;
                case "EikthyrPower": mod = AlmanacEffectsManager.Modifier.EikthyrPower; break;
                case "ElderPower": mod = AlmanacEffectsManager.Modifier.ElderPower; break;
                case "BonemassPower": mod = AlmanacEffectsManager.Modifier.BonemassPower; break;
                case "ModerPower": mod = AlmanacEffectsManager.Modifier.ModerPower; break;
                case "YagluthPower": mod = AlmanacEffectsManager.Modifier.YagluthPower; break;
                case "QueenPower": mod = AlmanacEffectsManager.Modifier.QueenPower; break;
            }

            switch (achievement.sprite_name)
            {
                case "almanac_bone_skull": customSprite = AlmanacPlugin.boneSkullIcon; break;
                case "almanac_sword_blue": customSprite = AlmanacPlugin.swordBasicBlueIcon; break;
                case "almanac_sword_brown": customSprite = AlmanacPlugin.swordBasicBrownIcon; break;
                case "almanac_arrow": customSprite = AlmanacPlugin.arrowBasicIcon; break;
                case "almanac_cape_hood": customSprite = AlmanacPlugin.capeHoodIcon; break;
                case "almanac_bottle_empty": customSprite = AlmanacPlugin.bottleStandardEmptyIcon; break;
                case "almanac_bottle_blue": customSprite = AlmanacPlugin.bottleStandardBlueIcon; break;
                case "almanac_fish_green": customSprite = AlmanacPlugin.fishGreenIcon; break;
                case "almanac_bow": customSprite = AlmanacPlugin.bowWoodIcon; break;
                case "almanac_necklace": customSprite = AlmanacPlugin.necklaceSilverRed; break;
                case "almanac_mushroom": customSprite = AlmanacPlugin.mushroomBigRedIcon; break;
                case "almanac_gold_coins": customSprite = AlmanacPlugin.goldCoinsPileIcon; break;
                case "almanac_key_silver": customSprite = AlmanacPlugin.keySilverIcon; break;
                case "almanac_bone_white": customSprite = AlmanacPlugin.boneWhiteIcon; break;
                case "almanac_book_red": customSprite = AlmanacPlugin.bookClosedRedIcon; break;
                case "almanac_bottle_green": customSprite = AlmanacPlugin.bottleStandardGreenIcon; break;
                case "almanac_crown_gold": customSprite = AlmanacPlugin.crownGoldIcon; break;
                case "almanac_gem_red": customSprite = AlmanacPlugin.gemDiamondRedIcon; break;
                case "almanac_gold_bars": customSprite = AlmanacPlugin.goldBarsIcon; break;
                case "almanac_scroll_map": customSprite = AlmanacPlugin.scrollMapIcon; break;
                case "almanac_shield": customSprite = AlmanacPlugin.shieldBasicIcon; break;
                case "almanac_silver_bars": customSprite = AlmanacPlugin.silverBarsIcon; break;
                case "almanac_silver_coins": customSprite = AlmanacPlugin.silverCoinsIcon; break;
                case "almanac_wood_log": customSprite = AlmanacPlugin.woodLogIcon; break;
                case "almanac_wood_stack": customSprite = AlmanacPlugin.woodLogsIcon; break;
            }

            CreateAchievement(
                uniqueName: achievement.unique_name,
                name: achievement.display_name,
                type: GetAchievementType(achievement.achievement_type),
                modifier: mod,
                modifiers: modifiers,
                startEffects: achievement.start_effects,
                stopEffects: achievement.stop_effects,
                goal: achievement.goal,
                sprite: customSprite,
                spriteName: achievement.sprite_name,
                lore: achievement.lore,
                toolTip: achievement.tool_tip,
                desc: achievement.description,
                damageModifier: achievement.resistance_modifier,
                statusEffectStopMsg: achievement.stop_message,
                newValue: achievement.modifier_value
            );
        }
    }

    private static AchievementType GetAchievementType(string modifier)
    {
        switch (modifier)
        {
            case "Fish": return AchievementType.Fish;
            case "Materials": return AchievementType.Materials;
            case "Consumables": return AchievementType.Consumables;
            case "Weapons": return AchievementType.Weapons;
            case "Swords": return AchievementType.Swords;
            case "Axes": return AchievementType.Axes;
            case "Polearms": return AchievementType.PoleArms;
            case "Spears": return AchievementType.Spears;
            case "Maces": return AchievementType.Maces;
            case "Knives": return AchievementType.Knives;
            case "Shields": return AchievementType.Shields;
            case "Staves": return AchievementType.Staves;
            case "Arrows": return AchievementType.Arrows;
            case "Bows": return AchievementType.Bows;
            case "Valuables": return AchievementType.Valuables;
            case "Potions": return AchievementType.Potions;
            case "Trophies": return AchievementType.Trophies;
            case "Creatures": return AchievementType.Creatures;
            case "MeadowCreatures": return AchievementType.MeadowCreatures;
            case "BlackForestCreatures": return AchievementType.BlackForestCreatures;
            case "SwampCreatures": return AchievementType.SwampCreatures;
            case "MountainCreatures": return AchievementType.MountainCreatures;
            case "PlainsCreatures": return AchievementType.PlainsCreatures;
            case "MistLandCreatures": return AchievementType.MistLandCreatures;
            case "AshLandCreatures": return AchievementType.AshLandCreatures;
            case "DeepNorthCreatures": return AchievementType.DeepNorthCreatures;
            case "EikthyrKills": return AchievementType.EikthyrKills;
            case "ElderKills": return AchievementType.ElderKills;
            case "BonemassKills": return AchievementType.BonemassKills;
            case "ModerKills": return AchievementType.ModerKills;
            case "YagluthKills": return AchievementType.YagluthKills;
            case "QueenKills": return AchievementType.QueenKills;
            case "Deaths": return AchievementType.Deaths;
            case "DistanceRan": return AchievementType.DistanceRan;
            case "DistanceSailed": return AchievementType.DistanceSailed;
            case "TotalKills": return AchievementType.TotalKills;
            case "TotalAchievements": return AchievementType.TotalAchievements;
            case "TrollKills": return AchievementType.TrollKills;
            case "SerpentKills": return AchievementType.SerpentKills;
            case "CultistKills": return AchievementType.CultistKills;
            case "StoneGolemKills": return AchievementType.StoneGolemKills;
            case "TarBlobKills": return AchievementType.TarBlobKills;
            case "DeathByFall": return AchievementType.DeathByFall;
            case "Trees": return AchievementType.TreesChopped;
            case "DeathByTree": return AchievementType.DeathByTree;
            case "DeathByEdge": return AchievementType.DeathByEdgeOfWorld;
            case "TimeInBase": return AchievementType.TimeInBase;
            case "TimeOutOfBase": return AchievementType.TimeOutOfBase;
            case "ArrowsShot": return AchievementType.ArrowsShot;
            case "GoblinShamanKills": return AchievementType.GoblinShamanKills;
            case "DrakeKills": return AchievementType.DrakeKills;
            case "GhostKills": return AchievementType.GhostKills;
            case "FenringKills": return AchievementType.FenringKills;
            case "WraithKills": return AchievementType.WraithKills;
            default: return AchievementType.None;
        }
    }

    private static List<AchievementData> CreateAlmanacAchievements()
    {
        List<AchievementData> almanacAchievements = new()
        {
            new AchievementData()
            {
                unique_name = "meadow_kill",
                display_name = "$almanac_achievement_meadow_kill",
                description = "$almanac_achievement_meadow_kill_desc",
                sprite_name = "HardAntler",
                lore = "$almanac_achievement_meadow_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "MeadowCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                resistance_modifier = "",
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "blackforest_kill",
                display_name = "$almanac_achievement_blackforest_kill",
                goal = 0,
                description = "$almanac_achievement_blackforest_kill_desc",
                sprite_name = "almanac_bone_white",
                lore = "$almanac_achievement_blackforest_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "BlackForestCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "swamp_kill",
                display_name = "$almanac_achievement_swamp_kill",
                description = "$almanac_achievement_swamp_kill_desc",
                sprite_name = "TrophyAbomination",
                lore = "$almanac_achievement_swamp_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "SwampCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "mountain_kill",
                display_name = "$almanac_achievement_mountain_kill",
                description = "$almanac_achievement_mountain_kill_desc",
                sprite_name = "DragonTear",
                lore = "$almanac_achievement_mountain_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "MountainCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "plains_kill",
                display_name = "$almanac_achievement_plains_kill",
                description = "$almanac_achievement_plains_kill_desc",
                lore = "$almanac_achievement_plains_kill_lore",
                sprite_name = "Barley",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "PlainsCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "mistlands_kill",
                display_name = "$almanac_achievement_mistlands_kill",
                description = "$almanac_achievement_mistlands_kill_desc",
                lore = "$almanac_achievement_mistlands_kill_lore",
                sprite_name = "MushroomMagecap",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "MistLandCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "ranger",
                display_name = "$almanac_achievement_ranger",
                description = "$almanac_achievement_ranger_desc",
                sprite_name = "almanac_arrow",
                lore = "$almanac_ranger_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_increase_projectile_damage_by <color=orange>5</color>",
                achievement_type = "Arrows",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_projectile_default",
                modifier = "RangedDMG",
                resistance_modifier = "Physical = Weak",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "brew_master",
                display_name = "$almanac_achievement_brew_master",
                description = "$almanac_achievement_brew_master_desc",
                sprite_name = "almanac_bottle_blue",
                lore = "$almanac_brew_master_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_frost</color>\n$almanac_increase_fire_damage_by <color=orange>5</color>",
                achievement_type = "Potions",
                resistance_modifier = "Frost = Weak",
                stop_message = "$almanac_damage_default",
                start_effects = new []{"sfx_coins_placed"},
                modifier = "FireDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "fisher",
                display_name = "$almanac_achievement_fisher",
                description = "$almanac_achievement_fisher_desc",
                sprite_name = "almanac_fish_green",
                lore = "$almanac_fisher_lore",
                tool_tip = "$almanac_allows_moder_power",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Fish",
                modifier = "ModerPower"
            },
            new AchievementData()
            {
                unique_name = "knowledge",
                display_name = "$almanac_achievement_knowledgeable",
                description = "$almanac_achievement_knowledgeable_desc",
                sprite_name = "almanac_necklace",
                lore = "$almanac_knowledgeable_lore",
                tool_tip = "$almanac_increase_carry_weight_by <color=orange>100</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "Materials",
                modifier = "MaxCarryWeight",
                modifier_value = 100f
            },
            new AchievementData()
            {
                unique_name = "master_archer",
                display_name = "$almanac_achievement_master_archer",
                description = "$almanac_achievement_master_archer_desc",
                sprite_name = "almanac_bow",
                lore = "$almanac_master_archer_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_increase_projectile_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Bows",
                resistance_modifier = "Physical = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "RangedDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "gluttony",
                display_name = "$almanac_achievement_gluttony",
                description = "$almanac_achievement_gluttony_desc",
                sprite_name = "almanac_mushroom",
                stop_message = "$almanac_removed_achievement_power",
                lore = "$almanac_gluttony_lore",
                start_effects = new []{"sfx_coins_placed"},
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_very_resistant</color> VS <color=orange>$almanac_poison</color>\n$almanac_reduce_health_by <color=orange>5</color>",
                achievement_type = "Consumables",
                resistance_modifier = "Poison = VeryResistant",
                modifier = "BaseHP",
                modifier_value = -5f
            },
            new AchievementData()
            {
                unique_name = "stag_slayer",
                display_name = "$almanac_achievement_stag_slayer",
                description = "$almanac_achievement_stag_slayer_desc",
                goal = 100,
                sprite_name = "TrophyEikthyr",
                stop_message = "$almanac_removed_achievement_power",
                lore = "$almanac_stag_slayer_lore",
                tool_tip = "$almanac_allows_eikthyr_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                modifier = "EikthyrPower"
            },
            new AchievementData()
            {
                unique_name = "undying",
                display_name = "$almanac_achievement_undying",
                description = "$almanac_achievement_undying_desc",
                goal = 200,
                sprite_name = "almanac_bone_skull",
                lore = "$almanac_undying_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_stamina_by <color=orange>10</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "Deaths",
                modifier = "BaseStamina",
                resistance_modifier = "Spirit = Weak",
                modifier_value = 10f
            },
            new AchievementData()
            {
                unique_name = "completion",
                display_name = "$almanac_achievement_completion",
                description = "$almanac_achievement_completion_desc",
                sprite_name = "almanac_crown_gold",
                lore = "$almanac_completion_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_raise_skill_experience_by <color=orange>100</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalAchievements",
                resistance_modifier = "Physical = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "RaiseSkills",
                modifier_value = 2f
            },
            new AchievementData()
            {
                unique_name = "runner",
                display_name = "$almanac_achievement_runner",
                description = "$almanac_achievement_runner_desc",
                sprite_name = "almanac_scroll_map",
                lore = "$almanac_runner_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_pierce</color>\n$almanac_reduce_stamina_drain_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DistanceRan",
                resistance_modifier = "Pierce = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "RunStaminaDrain",
                modifier_value = 0.5f,
                goal = 999999
            },
            new AchievementData()
            {
                unique_name = "killer",
                display_name = "$almanac_achievement_killer",
                description = "$almanac_achievement_killer_desc",
                lore = "$almanac_killer_lore",
                sprite_name = "Acorn",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_increase_attack_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalKills",
                modifier = "Attack",
                resistance_modifier = "Physical = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier_value = 1.5f,
                goal = 999999
            },
            new AchievementData()
            {
                unique_name = "sailor",
                display_name = "$almanac_achievement_sailor",
                description = "$almanac_achievement_sailor_desc",
                lore = "$almanac_sailor_lore",
                sprite_name = "SpearChitin",
                tool_tip = "$almanac_allows_moder_power",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "DistanceSailed",
                modifier = "ModerPower",
                goal = 999999
            },
            new AchievementData()
            {
                unique_name = "break_a_leg",
                display_name = "$almanac_achievement_break_a_leg",
                description = "$almanac_achievement_break_a_leg_desc",
                lore = "$almanac_break_a_leg_lore",
                sprite_name = "BoneFragments",
                tool_tip = "$almanac_reduce_fall_damage_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DeathByFall",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "FallDamage",
                modifier_value = 0.9f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "troll",
                display_name = "$almanac_achievement_troll",
                description = "$almanac_achievement_troll_desc",
                lore = "$almanac_troll_lore",
                sprite_name = "TrophyForestTroll",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_blunt</color>\n$almanac_increase_health_regen_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TrollKills",
                modifier = "HealthRegen",
                resistance_modifier = "Blunt = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier_value = 1.1f,
                goal = 200,
            },
            new AchievementData()
            {
                unique_name = "tarred",
                display_name = "$almanac_achievement_tarred",
                description = "$almanac_achievement_tarred_desc",
                lore = "$almanac_tarred_lore",
                sprite_name = "Tar",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_speed_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TarBlobKills",
                resistance_modifier = "Poison = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "Speed",
                modifier_value = 1.1f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "golem_hunter",
                display_name = "$almanac_achievement_golem_hunter",
                description = "$almanac_achievement_golem_hunter_desc",
                lore = "$almanac_golem_hunter_lore",
                sprite_name = "TrophySGolem",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_blunt</color>\n$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "StoneGolemKills",
                resistance_modifier = "Blunt = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "FrostDMG",
                modifier_value = 5f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "gaseous",
                display_name = "$almanac_achievement_gaseous",
                description = "$almanac_achievement_gaseous_desc",
                lore = "$almanac_gaseous_lore",
                sprite_name = "BombOoze",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_poison_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 500,
                resistance_modifier = "Poison = Weak",
                modifier = "PoisonDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "spark",
                display_name = "$almanac_achievement_spark II",
                description = "$almanac_achievement_spark_desc",
                lore = "$almanac_spark_lore",
                sprite_name = "Thunderstone",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_lightning</color>\n$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 200,
                resistance_modifier = "Lightning = Weak",
                modifier = "LightningDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "spark_1",
                display_name = "$almanac_achievement_spark II",
                description = "$almanac_achievement_spark_desc",
                lore = "$almanac_spark_lore",
                sprite_name = "Thunderstone",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_lightning</color>\n$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 400,
                resistance_modifier = "Lightning = Weak",
                modifier = "LightningDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "spark_2",
                display_name = "$almanac_achievement_spark III",
                description = "$almanac_achievement_spark_desc",
                lore = "$almanac_spark_lore",
                sprite_name = "Thunderstone",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_lightning</color>\n$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Lightning = Weak",
                goal = 600,
                modifier = "LightningDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "sword_master",
                display_name = "$almanac_achievement_sword_master",
                description = "$almanac_achievement_sword_master_desc",
                lore = "$almanac_sword_master_lore",
                sprite_name = "almanac_sword_blue",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_pierce</color>\n$almanac_increase_melee_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Swords",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Pierce = Weak",
                modifier = "MeleeDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "axe_master",
                display_name = "$almanac_achievement_axe_master",
                description = "$almanac_achievement_axe_master_desc",
                lore = "$almanac_axe_master_lore",
                sprite_name = "AxeJotunBane",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_blunt</color>\n$almanac_increase_melee_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Axes",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Blunt = Weak",
                modifier = "MeleeDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "lumberjack",
                display_name = "$almanac_achievement_lumberjack",
                description = "$almanac_achievement_lumberjack_desc",
                lore = "$almanac_lumberjack_lore",
                sprite_name = "almanac_wood_stack",
                tool_tip = "$almanac_increase_chop_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "Trees",
                modifier = "ChopDMG",
                modifier_value = 5f,
                goal = 1000
            },
            new AchievementData()
            {
                unique_name = "lumberer",
                display_name = "$almanac_achievement_lumberer",
                description = "$almanac_achievement_lumberer_desc",
                lore = "$almanac_lumberer_lore",
                sprite_name = "almanac_wood_log",
                tool_tip = "$almanac_increase_chop_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "DeathByTree",
                modifier = "ChopDMG",
                modifier_value = 5f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "daredevil",
                display_name = "$almanac_achievement_daredevil",
                description = "$almanac_achievement_daredevil_desc",
                lore = "$almanac_daredevil_lore",
                sprite_name = "YagluthDrop",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_stamina_by <color=orange>15</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DeathByEdge",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Spirit = Weak",
                modifier = "BaseStamina",
                modifier_value = 15f,
                goal = 1
            },
            new AchievementData()
            {
                unique_name = "turret",
                display_name = "$almanac_achievement_turret",
                description = "$almanac_achievement_turret_desc",
                lore = "$almanac_turret_lore",
                sprite_name = "ArrowCarapace",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_projectile_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "ArrowsShot",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Fire = Weak",
                modifier = "RangedDMG",
                modifier_value = 5f,
                goal = 5000
            },
            new AchievementData()
            {
                unique_name = "adventurer",
                display_name = "$almanac_achievement_adventurer",
                description = "$almanac_achievement_adventurer_desc",
                lore = "$almanac_adventurer_lore",
                sprite_name = "BlackCore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_stamina_regen_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TimeOutOfBase",
                stop_message = "$almanac_removed_achievement_power",
                goal = 50000,
                resistance_modifier = "Poison = Weak",
                modifier = "StaminaRegen",
                modifier_value = 1.5f
            },
            new AchievementData()
            {
                unique_name = "cultist_hunter",
                display_name = "$almanac_achievement_cultist_hunter",
                description = "$almanac_achievement_cultist_hunter_desc",
                lore = "$almanac_cultist_hunter_lore",
                sprite_name = "almanac_book_red",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_frost</color>\n$almanac_increase_fire_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "CultistKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Frost = Weak",
                modifier = "FireDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
            unique_name = "goblin_shaman_hunter",
            display_name = "$almanac_achievement_goblin_shaman_hunter",
            description = "$almanac_achievement_goblin_shaman_hunter_desc",
            lore = "$almanac_goblin_shaman_hunter_lore",
            sprite_name = "GoblinTotem",
            tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_frost</color>\n$almanac_increase_fire_damage_by <color=orange>5</color>",
            start_effects = new []{"sfx_coins_placed"},
            achievement_type = "GoblinShamanKills",
            stop_message = "$almanac_removed_achievement_power",
            goal = 100,
            resistance_modifier = "Frost = Weak",
            modifier = "FireDMG",
            modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "drake_hunter",
                display_name = "$almanac_achievement_drake_hunter",
                description = "$almanac_achievement_drake_hunter_desc",
                lore = "$almanac_drake_hunter_lore",
                sprite_name = "FreezeGland",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DrakeKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Fire = Weak",
                modifier = "FrostDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "dragon_killer",
                display_name = "$almanac_achievement_dragon_killer",
                description = "$almanac_achievement_dragon_killer_desc",
                lore = "$almanac_dragon_killer_lore",
                sprite_name = "TrophyDragonQueen",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "ModerKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Fire = Weak",
                modifier = "FrostDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "bonemass_killer",
                display_name = "$almanac_achievement_bonemass_killer",
                description = "$almanac_achievement_bonemass_killer_desc",
                lore = "$almanac_bonemass_killer_lore",
                sprite_name = "TrophyBonemass",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_poison_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "BonemassKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "PoisonDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "stench",
                display_name = "$almanac_achievement_stench",
                description = "$almanac_achievement_stench_desc",
                lore = "$almanac_stench_lore",
                sprite_name = "Ooze",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_poison_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Deaths",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "PoisonDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "vengeful",
                display_name = "$almanac_achievement_vengeful",
                description = "$almanac_achievement_vengeful_desc",
                lore = "$almanac_vengeful_lore",
                sprite_name = "almanac_gem_red",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_increase_spirit_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "WraithKills",
                goal = 100,
                resistance_modifier = "Physical = Weak",
                modifier = "SpiritDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "ghastly",
                display_name = "$almanac_achievement_ghastly",
                description = "$almanac_achievement_ghastly_desc",
                lore = "$almanac_ghastly_lore",
                sprite_name = "almanac_bottle_empty",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_increase_spirit_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "GhostKills",
                goal = 100,
                resistance_modifier = "Physical = Weak",
                modifier = "SpiritDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "fenring",
                display_name = "$almanac_achievement_fenring",
                description = "$almanac_achievement_fenring_desc",
                lore = "$almanac_fenring_lore",
                sprite_name = "TrophyUlv",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_physical</color>\n$almanac_increase_spirit_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "FenringKills",
                goal = 100,
                resistance_modifier = "Physical = Weak",
                modifier = "SpiritDMG",
                modifier_value = 5f
            },
        };

        return almanacAchievements;
        // var serializer = new SerializerBuilder().Build();
        // string serializedData = serializer.Serialize(almanacAchievements);
        //
        // return serializedData;
    }

    [Serializable]
    public class AchievementData : ISerializableParameter
    {
        public string unique_name = null!;
        public string display_name = "";
        public string sprite_name = "";
        public string description = "";
        public string lore = "";
        public string tool_tip = "";
        public string stop_message = "$almanac_removed_achievement_power";
        public string[] start_effects;
        public string[] stop_effects;
        public string achievement_type = "";
        public int goal = 0;
        public string resistance_modifier = "";
        public string modifier = "";
        public float modifier_value = 0f;
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(unique_name);
            pkg.Write(display_name);
            pkg.Write(sprite_name ?? "");
            pkg.Write(description ?? "");
            pkg.Write(lore ?? "");
            pkg.Write(tool_tip ?? "");
            pkg.Write(stop_message ?? "");
            pkg.Write(start_effects.Length); // Write the number of start_effects
            foreach(string effect in start_effects) pkg.Write(effect);
            pkg.Write(stop_effects.Length);
            foreach (string effect in stop_effects) pkg.Write(effect);
            pkg.Write(achievement_type);
            pkg.Write(goal);
            pkg.Write(resistance_modifier);
            pkg.Write(modifier);
            pkg.Write(modifier_value);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            unique_name = pkg.ReadString();
            display_name = pkg.ReadString();
            sprite_name = pkg.ReadString();
            description = pkg.ReadString();
            lore = pkg.ReadString();
            tool_tip = pkg.ReadString();
            stop_message = pkg.ReadString();
            int startEffectsLength = pkg.ReadInt();
            start_effects = new string[startEffectsLength];
            for (int i = 0; i < startEffectsLength; ++i) start_effects[i] = pkg.ReadString();
            int stopEffectsLength = pkg.ReadInt();
            stop_effects = new string[stopEffectsLength];
            for (int i = 0; i < stopEffectsLength; ++i) stop_effects[i] = pkg.ReadString();
            achievement_type = pkg.ReadString();
            goal = pkg.ReadInt();
            resistance_modifier = pkg.ReadString();
            modifier = pkg.ReadString();
            modifier_value = pkg.ReadSingle();

        }
    }

    private static AchievementData CreateAchievementData(
        string uniqueName,
        int goal,
        string spriteName,
        string toolTip,
        string achievementType,
        string stopMsg,
        string[] startEffects,
        string[] stopEffects,
        string resistanceMod,
        string modifier,
        float value
    )
    {
        string displayName = $"$almanac_achievement_{uniqueName}";
        string description = $"$almanac_achievement_{uniqueName}_desc";
        string lore = $"$almanac_{uniqueName}_lore";
        string toolTipValued = $"{toolTip} <color=orange>{value}</color>";

        AchievementData data = new AchievementData()
        {
            unique_name = uniqueName,
            display_name = displayName,
            goal = goal,
            description = description,
            sprite_name = spriteName,
            lore = lore,
            tool_tip = toolTipValued,
            achievement_type = achievementType,
            stop_message = stopMsg,
            start_effects = startEffects,
            stop_effects = stopEffects,
            resistance_modifier = resistanceMod,
            modifier = modifier,
            modifier_value = value
        };
        return data;
    }
}