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
        DeathByFall
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
                tool_tip = "$almanac_increase_health_by <color=orange>6</color>",
                achievement_type = "PlainsCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 6f
            },
            new AchievementData()
            {
                unique_name = "mistlands_kill",
                display_name = "$almanac_achievement_mistlands_kill",
                description = "$almanac_achievement_mistlands_kill_desc",
                lore = "$almanac_achievement_mistlands_kill_lore",
                sprite_name = "MushroomMagecap",
                tool_tip = "$almanac_increase_health_by <color=orange>7</color>",
                achievement_type = "MistLandCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 7f
            },
            new AchievementData()
            {
                unique_name = "ranger",
                display_name = "$almanac_achievement_ranger",
                description = "$almanac_achievement_ranger_desc",
                sprite_name = "almanac_arrow",
                lore = "$almanac_ranger_lore",
                tool_tip = "$almanac_increase_projectile_damage_by <color=orange>5</color>",
                achievement_type = "Arrows",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_projectile_default",
                modifier = "RangedDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "brew_master",
                display_name = "$almanac_achievement_brew_master",
                description = "$almanac_achievement_brew_master_desc",
                sprite_name = "almanac_bottle_blue",
                lore = "$almanac_brew_master_lore",
                tool_tip = "$almanac_increase_fire_damage_by <color=orange>5</color>",
                achievement_type = "Potions",
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
                tool_tip = "$almanac_increase_projectile_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Bows",
                modifier = "RangedDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "gluttony",
                display_name = "$almanac_achievement_gluttony",
                description = "$almanac_achievement_gluttony_desc",
                sprite_name = "almanac_mushroom",
                lore = "$almanac_gluttony_lore",
                start_effects = new []{"sfx_coins_placed"},
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_resistant</color> VS <color=orange>$almanac_poison</color>",
                achievement_type = "Consumables",
                resistance_modifier = "Poison = Resistant",
            },
            new AchievementData()
            {
                unique_name = "stag_slayer",
                display_name = "$almanac_achievement_stag_slayer",
                description = "$almanac_achievement_stag_slayer_desc",
                goal = 100,
                sprite_name = "TrophyEikthyr",
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
                tool_tip = "$almanac_increase_stamina_by <color=orange>10</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Deaths",
                modifier = "BaseStamina",
                modifier_value = 10f
            },
            new AchievementData()
            {
                unique_name = "completion",
                display_name = "$almanac_achievement_completion",
                description = "$almanac_achievement_completion_desc",
                sprite_name = "almanac_crown_gold",
                lore = "$almanac_completion_lore",
                tool_tip = "$almanac_raise_skill_experience_by <color=orange>100</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalAchievements",
                modifier = "RaiseSkills",
                modifier_value = 2f
            },
            new AchievementData()
            {
                unique_name = "runner",
                display_name = "$almanac_achievement_runner",
                description = "$almanac_achievement_runner_desc",
                sprite_name = "almanac_gem_red",
                lore = "$almanac_runner_lore",
                tool_tip = "$almanac_reduce_stamina_drain_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DistanceRan",
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
                tool_tip = "$almanac_increase_attack_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalKills",
                modifier = "Attack",
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
                tool_tip = "$almanac_increase_health_regen_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TrollKills",
                modifier = "HealthRegen",
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
                tool_tip = "$almanac_increase_speed_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TarBlobKills",
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
                tool_tip = "$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "StoneGolemKills",
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
                tool_tip = "$almanac_increase_poison_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalKills",
                goal = 500,
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
                tool_tip = "$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                goal = 200,
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
                tool_tip = "$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                goal = 400,
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
                tool_tip = "$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                goal = 600,
                modifier = "LightningDMG",
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
        public string stop_message = "";
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