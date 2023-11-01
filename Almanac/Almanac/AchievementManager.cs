using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.Serialization;
using YamlDotNet.Serialization;

namespace Almanac.Almanac;

public static class AchievementManager
{
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
        QueenKills

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
            case AlmanacEffectsManager.Modifier.EikthyrPower:
                statusEffectName = "GP_Eikthyr";
                break;
            case AlmanacEffectsManager.Modifier.ElderPower:
                statusEffectName = "GP_TheElder";
                break;
            case AlmanacEffectsManager.Modifier.BonemassPower:
                statusEffectName = "GP_Bonemass";
                break;
            case AlmanacEffectsManager.Modifier.ModerPower:
                statusEffectName = "GP_Moder";
                break;
            case AlmanacEffectsManager.Modifier.YagluthPower:
                statusEffectName = "GP_Yagluth";
                break;
            case AlmanacEffectsManager.Modifier.QueenPower:
                statusEffectName = "GP_Queen";
                break;
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

    private static readonly string folderName = "Almanac";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    private static readonly string achievementPath = folderPath + Path.DirectorySeparatorChar + "AchievementData";

    public static void InitAchievements()
    {
        if (!Directory.Exists(achievementPath))
        {
            Directory.CreateDirectory(achievementPath);
        }

        var deserializer = new DeserializerBuilder().Build();
        List<AchievementData> deserializedData;

        if (!File.Exists(achievementPath + Path.DirectorySeparatorChar + "AlmanacAchievements.yml"))
        {
            string serializedData = CreateAlmanacAchievements();
            File.WriteAllText(
                (achievementPath + Path.DirectorySeparatorChar + "AlmanacAchievements.yml"),
                serializedData);

            deserializedData = deserializer.Deserialize <List<AchievementData>>(serializedData);
        }
        else
        {
            string serializedData = File.ReadAllText(
                achievementPath + Path.DirectorySeparatorChar + "AlmanacAchievements.yml");
            deserializedData = deserializer.Deserialize<List<AchievementData>>(serializedData);
        }
        
        foreach (AchievementData achievement in deserializedData)
        {
            AchievementType type = AchievementType.None;
            AlmanacEffectsManager.Modifier mod = AlmanacEffectsManager.Modifier.None;
            Dictionary<AlmanacEffectsManager.Modifier, float> modifiers = new ()
            {
                { AlmanacEffectsManager.Modifier.Attack, 1f },
                { AlmanacEffectsManager.Modifier.HealthRegen , 1f },
                { AlmanacEffectsManager.Modifier.StaminaRegen , 1f },
                { AlmanacEffectsManager.Modifier.RaiseSkills , 1f },
                { AlmanacEffectsManager.Modifier.Speed , 1f },
                { AlmanacEffectsManager.Modifier.Noise , 1f },
                { AlmanacEffectsManager.Modifier.MaxCarryWeight , 0f },
                { AlmanacEffectsManager.Modifier.Stealth , 1f },
                { AlmanacEffectsManager.Modifier.RunStaminaDrain , 1f },
                { AlmanacEffectsManager.Modifier.DamageReduction , 0f },
                { AlmanacEffectsManager.Modifier.FallDamage , 1f }
            };

            Sprite? customSprite = null;

            switch (achievement.achievement_type)
            {
                    case "Fish":
                        type = AchievementType.Fish;
                        break;
                    case "Materials":
                        type = AchievementType.Materials;
                        break;
                    case "Consumables":
                        type = AchievementType.Consumables;
                        break;
                    case "Weapons":
                        type = AchievementType.Weapons;
                        break;
                    case "Arrows":
                        type = AchievementType.Arrows;
                        break;
                    case "Bows":
                        type = AchievementType.Bows;
                        break;
                    case "Valuables":
                        type = AchievementType.Valuables;
                        break;
                    case "Potions":
                        type = AchievementType.Potions;
                        break;
                    case "Trophies":
                        type = AchievementType.Trophies;
                        break;
                    case "Creatures":
                        type = AchievementType.Creatures;
                        break;
                    case "MeadowCreatures":
                        type = AchievementType.MeadowCreatures;
                        break;
                    case "BlackForestCreatures":
                        type = AchievementType.BlackForestCreatures;
                        break;
                    case "SwampCreatures":
                        type = AchievementType.SwampCreatures;
                        break;
                    case "MountainCreatures":
                        type = AchievementType.MountainCreatures;
                        break;
                    case "PlainsCreatures":
                        type = AchievementType.PlainsCreatures;
                        break;
                    case "MistLandCreatures":
                        type = AchievementType.MistLandCreatures;
                        break;
                    case "AshLandCreatures":
                        type = AchievementType.AshLandCreatures;
                        break;
                    case "DeepNorthCreatures":
                        type = AchievementType.DeepNorthCreatures;
                        break;
                    case "EikthyrKills":
                        type = AchievementType.EikthyrKills;
                        break;
                    case "ElderKills":
                        type = AchievementType.ElderKills;
                        break;
                    case "BonemassKills":
                        type = AchievementType.BonemassKills;
                        break;
                    case "ModerKills":
                        type = AchievementType.ModerKills;
                        break;
                    case "YagluthKills":
                        type = AchievementType.YagluthKills;
                        break;
                    case "QueenKills":
                        type = AchievementType.QueenKills;
                        break;
                    case "Deaths":
                        type = AchievementType.Deaths;
                        break;
            }

            switch (achievement.modifier)
            {
                case "Attack":
                    mod = AlmanacEffectsManager.Modifier.Attack;
                    modifiers[AlmanacEffectsManager.Modifier.Attack] = achievement.modifier_value;
                    break;
                case "HealthRegen":
                    mod = AlmanacEffectsManager.Modifier.HealthRegen;
                    modifiers[AlmanacEffectsManager.Modifier.HealthRegen] = achievement.modifier_value;
                    break;
                case "StaminaRegen":
                    mod = AlmanacEffectsManager.Modifier.StaminaRegen;
                    modifiers[AlmanacEffectsManager.Modifier.StaminaRegen] = achievement.modifier_value;
                    break;
                case "RaiseSkills":
                    mod = AlmanacEffectsManager.Modifier.RaiseSkills;
                    modifiers[AlmanacEffectsManager.Modifier.RaiseSkills] = achievement.modifier_value;
                    break;
                case "Speed":
                    mod = AlmanacEffectsManager.Modifier.Speed;
                    modifiers[AlmanacEffectsManager.Modifier.Speed] = achievement.modifier_value;
                    break;
                case "Noise":
                    mod = AlmanacEffectsManager.Modifier.Noise;
                    modifiers[AlmanacEffectsManager.Modifier.Noise] = achievement.modifier_value;
                    break;
                case "MaxCarryWeight":
                    mod = AlmanacEffectsManager.Modifier.MaxCarryWeight;
                    modifiers[AlmanacEffectsManager.Modifier.MaxCarryWeight] = achievement.modifier_value;
                    break;
                case "Stealth":
                    mod = AlmanacEffectsManager.Modifier.Stealth;
                    modifiers[AlmanacEffectsManager.Modifier.Stealth] = achievement.modifier_value;
                    break;
                case "RunStaminaDrain":
                    mod = AlmanacEffectsManager.Modifier.RunStaminaDrain;
                    modifiers[AlmanacEffectsManager.Modifier.RunStaminaDrain] = achievement.modifier_value;
                    break;
                case "DamageReduction":
                    mod = AlmanacEffectsManager.Modifier.DamageReduction;
                    modifiers[AlmanacEffectsManager.Modifier.DamageReduction] = achievement.modifier_value;
                    break;
                case "FallDamage":
                    mod = AlmanacEffectsManager.Modifier.FallDamage;
                    modifiers[AlmanacEffectsManager.Modifier.FallDamage] = achievement.modifier_value;
                    break;
                case "BaseHP":
                    mod = AlmanacEffectsManager.Modifier.BaseHP;
                    // modifiers[AlmanacEffectsManager.Modifier.BaseHP] = achievement.modifier_value;
                    break;
                case "BaseStamina":
                    mod = AlmanacEffectsManager.Modifier.BaseStamina;
                    // modifiers[AlmanacEffectsManager.Modifier.BaseStamina] = achievement.modifier_value;
                    break;
                case "MeleeDMG":
                    mod = AlmanacEffectsManager.Modifier.MeleeDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.MeleeDMG] = achievement.modifier_value;
                    break;
                case "RangedDMG":
                    mod = AlmanacEffectsManager.Modifier.RangedDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.RangedDMG] = achievement.modifier_value;
                    break;
                case "FireDMG":
                    mod = AlmanacEffectsManager.Modifier.FireDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.FireDMG] = achievement.modifier_value;
                    break;
                case "FrostDMG":
                    mod = AlmanacEffectsManager.Modifier.FrostDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.FrostDMG] = achievement.modifier_value;
                    break;
                case "LightningDMG":
                    mod = AlmanacEffectsManager.Modifier.LightningDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.LightningDMG] = achievement.modifier_value;
                    break;
                case "PoisonDMG":
                    mod = AlmanacEffectsManager.Modifier.PoisonDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.PoisonDMG] = achievement.modifier_value;
                    break;
                case "SpiritDMG":
                    mod = AlmanacEffectsManager.Modifier.SpiritDMG;
                    // modifiers[AlmanacEffectsManager.Modifier.SpiritDMG] = achievement.modifier_value;
                    break;
                case "EikthyrPower":
                    mod = AlmanacEffectsManager.Modifier.EikthyrPower;
                    break;
                case "ElderPower":
                    mod = AlmanacEffectsManager.Modifier.ElderPower;
                    break;
                case "BonemassPower":
                    mod = AlmanacEffectsManager.Modifier.BonemassPower;
                    break;
                case "ModerPower":
                    mod = AlmanacEffectsManager.Modifier.ModerPower;
                    break;
                case "YagluthPower":
                    mod = AlmanacEffectsManager.Modifier.YagluthPower;
                    break;
                case "QueenPower":
                    mod = AlmanacEffectsManager.Modifier.QueenPower;
                    break;
            }

            switch (achievement.sprite_name)
            {
                case "almanac_bone_skull":
                    customSprite = AlmanacPlugin.boneSkullIcon;
                    break;
                case "almanac_sword_blue":
                    customSprite = AlmanacPlugin.swordBasicBlueIcon;
                    break;
                case "almanac_sword_brown":
                    customSprite = AlmanacPlugin.swordBasicBrownIcon;
                    break;
                case "almanac_arrow":
                    customSprite = AlmanacPlugin.arrowBasicIcon;
                    break;
                case "almanac_cape_hood":
                    customSprite = AlmanacPlugin.capeHoodIcon;
                    break;
                case "almanac_bottle_empty":
                    customSprite = AlmanacPlugin.bottleStandardEmptyIcon;
                    break;
                case "almanac_bottle_blue":
                    customSprite = AlmanacPlugin.bottleStandardBlueIcon;
                    break;
                case "almanac_fish_green":
                    customSprite = AlmanacPlugin.fishGreenIcon;
                    break;
                case "almanac_bow":
                    customSprite = AlmanacPlugin.bowWoodIcon;
                    break;
                case "almanac_necklace":
                    customSprite = AlmanacPlugin.necklaceSilverRed;
                    break;
                case "almanac_mushroom":
                    customSprite = AlmanacPlugin.mushroomBigRedIcon;
                    break;
                case "almanac_gold_coins":
                    customSprite = AlmanacPlugin.goldCoinsPileIcon;
                    break;
                case "almanac_key_silver":
                    customSprite = AlmanacPlugin.keySilverIcon;
                    break;
                case "almanac_bone_white":
                    customSprite = AlmanacPlugin.boneWhiteIcon;
                    break;
                case "almanac_book_red":
                    customSprite = AlmanacPlugin.bookClosedRedIcon;
                    break;
                case "almanac_bottle_green":
                    customSprite = AlmanacPlugin.bottleStandardGreenIcon;
                    break;
                case "almanac_crown_gold":
                    customSprite = AlmanacPlugin.crownGoldIcon;
                    break;
                case "almanac_gem_red":
                    customSprite = AlmanacPlugin.gemDiamondRedIcon;
                    break;
                case "almanac_gold_bars":
                    customSprite = AlmanacPlugin.goldBarsIcon;
                    break;
                case "almanac_scroll_map":
                    customSprite = AlmanacPlugin.scrollMapIcon;
                    break;
                case "almanac_shield":
                    customSprite = AlmanacPlugin.shieldBasicIcon;
                    break;
                case "almanac_silver_bars":
                    customSprite = AlmanacPlugin.silverBarsIcon;
                    break;
                case "almanac_silver_coins":
                    customSprite = AlmanacPlugin.silverCoinsIcon;
                    break;
                case "almanac_wood_log":
                    customSprite = AlmanacPlugin.woodLogIcon;
                    break;
                case "almanac_wood_stack":
                    customSprite = AlmanacPlugin.woodLogsIcon;
                    break;
            }
            
            CreateAchievement(
                uniqueName: achievement.unique_name,
                name: achievement.display_name,
                type: type,
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

    private static string CreateAlmanacAchievements()
    {
        List<AchievementData> almanacAchievements = new()
        {
            new AchievementData()
            {
                unique_name = "meadow_kill",
                display_name = "$almanac_achievement_meadow_kill",
                goal = 0,
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
                tool_tip = "$almanac_increase_health_by <color=orange>10</color>",
                achievement_type = "BlackForestCreatures",
                stop_message = "$almanac_health_default",
                modifier = "BaseHP",
                modifier_value = 10f
            },
            new AchievementData()
            {
                unique_name = "swamp_kill",
                display_name = "$almanac_achievement_swamp_kill",
                description = "$almanac_achievement_swamp_kill_desc",
                sprite_name = "TrophyAbomination",
                lore = "$almanac_achievement_swamp_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>15</color>",
                achievement_type = "SwampCreatures",
                stop_message = "$almanac_health_default",
                modifier = "BaseHP",
                modifier_value = 15f
            },
            new AchievementData()
            {
                unique_name = "mountain_kill",
                display_name = "$almanac_achievement_mountain_kill",
                description = "$almanac_achievement_mountain_kill_desc",
                sprite_name = "DragonTear",
                lore = "$almanac_achievement_mountain_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>20</color>",
                achievement_type = "MountainCreatures",
                stop_message = "$almanac_health_defeault",
                modifier = "BaseHP",
                modifier_value = 20f
            },
            new AchievementData()
            {
                unique_name = "ranger",
                display_name = "$almanac_achievement_ranger",
                description = "$almanac_achievement_ranger_desc",
                sprite_name = "almanac_arrow",
                lore = "$almanac_ranger_lore",
                tool_tip = "$almanac_increase_projectile_damage_by <color=orange>10</color>",
                achievement_type = "Arrows",
                stop_message = "$almanac_projectile_default",
                modifier = "RangedDMG",
                modifier_value = 10f
            },
            new AchievementData()
            {
                unique_name = "brew_master",
                display_name = "$almanac_achievement_brew_master",
                description = "$almanac_achievement_brew_master_desc",
                sprite_name = "almanac_bottle_blue",
                lore = "$almanac_brew_master_lore",
                tool_tip = "$almanac_increase_fire_damage_by <color=orange>10</color>",
                achievement_type = "Potions",
                stop_message = "$almanac_damage_default",
                modifier = "FireDMG",
                modifier_value = 10f
            },
            new AchievementData()
            {
                unique_name = "fisher",
                display_name = "$almanac_achievement_fisher",
                description = "$almanac_achievement_fisher_desc",
                sprite_name = "almanac_fish_green",
                lore = "$almanac_fisher_lore",
                tool_tip = "$almanac_allows_moder_power",
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
                tool_tip = "$almanac_increase_projectile_damage_by <color=orange>15</color>",
                achievement_type = "Bows",
                modifier = "RangedDMG",
                modifier_value = 15f
            },
            new AchievementData()
            {
                unique_name = "gluttony",
                display_name = "$almanac_achievement_gluttony",
                description = "$almanac_achievement_gluttony_desc",
                sprite_name = "almanac_mushroom",
                lore = "$almanac_gluttony_lore",
                tool_tip = "$almanac_modify_poison_resistance <color=orange>$almanac_resistant</color>",
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
                achievement_type = "EikthyrKills",
                modifier = "EikthyrPower"
            },
            new AchievementData()
            {
                unique_name = "undying",
                display_name = "$almanac_achievement_undying",
                description = "$almanac_achievement_undying_lore",
                goal = 1000,
                sprite_name = "almanac_bone_skull",
                lore = "$almanac_undying_lore",
                tool_tip = "$almanac_increase_stamina_by <color=orange>25</color>",
                achievement_type = "Deaths",
                modifier = "BaseStamina",
                modifier_value = 25f
            }
        };

        var serializer = new SerializerBuilder().Build();
        string serializedData = serializer.Serialize(almanacAchievements);

        return serializedData;
    }

    [Serializable]
    public class AchievementData
    {
        public string unique_name = null!;
        public string display_name = null!;
        public string sprite_name = null!;
        public string description = null!;
        public string lore = null!;
        public string tool_tip = null!;
        public string stop_message = null!;
        public string[] start_effects = null!;
        public string[] stop_effects = null!;
        public string achievement_type = null!;
        public int goal = 0;
        public string resistance_modifier = null!;
        public string modifier = null!;
        public float modifier_value = 0f;
    }

    private static AchievementData CreateAchievementData(
        string uniqueName,
        string displayName,
        int goal,
        string description,
        string spriteName,
        string lore,
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
        AchievementData data = new AchievementData()
        {
            unique_name = uniqueName,
            display_name = displayName,
            goal = goal,
            description = description,
            sprite_name = spriteName,
            lore = lore,
            tool_tip = toolTip,
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