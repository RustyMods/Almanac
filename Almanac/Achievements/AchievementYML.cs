using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.FileSystem;
using YamlDotNet.Serialization;
using static Almanac.Achievements.AlmanacEffectManager;

namespace Almanac.Achievements;

public static class AchievementYML
{
    [Serializable]
    public class AchievementData
    {
        public string unique_name = null!;
        public string display_name = "";
        public string sprite_name = "";
        public string description = "";
        public string lore = "";
        public string start_message = "";
        public string stop_message = "";
        public string tooltip = "";
        public string defeat_key = "";
        public AchievementTypes.AchievementType achievement_type;
        public string custom_group_key = "";
        public string custom_pickable_name = "";
        public int goal = 0;
        public int duration = 0;
        public AchievementTypes.AchievementRewardType reward_type = AchievementTypes.AchievementRewardType.StatusEffect;
        public string achievement_group = "";
        public int achievement_index = 0;
        public string item = "";
        public int item_amount = 0;
        public string skill = "";
        public int skill_amount = 0;
        public int class_experience = 0;
        public List<string> start_effects = new();
        public List<string> stop_effects = new();
        public List<HitData.DamageModPair> damage_modifiers = new()
        {
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
        };
        public Dictionary<Modifier, float> modifiers = new()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen, 1f },
            { Modifier.StaminaRegen, 1f },
            { Modifier.RaiseSkills, 1f },
            { Modifier.Speed, 1f },
            { Modifier.Noise, 1f },
            { Modifier.MaxCarryWeight, 0f },
            { Modifier.Stealth, 1f },
            { Modifier.RunStaminaDrain, 1f },
            { Modifier.DamageReduction, 0f },
            { Modifier.FallDamage, 1f },
            { Modifier.EitrRegen, 1f }
        };
    }

    public static void InitDefaultAchievements(bool overwrite = false)
    {
        ISerializer serializer = new SerializerBuilder().Build();
        AlmanacPaths.CreateFolderDirectories();
        List<string> paths = Directory.GetFiles(AlmanacPaths.AchievementFolderPath, "*yml").ToList();
        if (paths.Count > 0 && !overwrite && AlmanacPlugin._LoadDefaultAchievements.Value is AlmanacPlugin.Toggle.Off) return;
        foreach (AchievementData achievement in GetDefaultAchievements())
        {
            string path = AlmanacPaths.AchievementFolderPath + Path.DirectorySeparatorChar + achievement.unique_name + ".yml";
            if (File.Exists(path) && !overwrite) continue;
            string data = serializer.Serialize(achievement);
            File.WriteAllText(path, data);
        }
    }

    private static List<AchievementData> GetDefaultAchievements()
    {
        List<AchievementData> output = new();
        #region Kill Achievements
        List<AchievementData> KillAchievements = new()
        {
            new AchievementData()
            {
                unique_name = "neck_kill",
                display_name = "Neck Jab",
                sprite_name = "TrophyNeck",
                description = "Kill over <color=orange>100</color> Necks",
                lore =
                    "The necks are treacherous creatures who are known to drown their prey on the seemingly peaceful shores of the meadows.",
                defeat_key = "defeated_neck",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.VeryWeak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.05f },
                    { Modifier.RaiseSkills, 1.1f },
                    { Modifier.Speed, 1.05f },
                    { Modifier.Noise, 1.05f },
                    { Modifier.MaxCarryWeight, -50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "boar_kill",
                display_name = "Boar Hunt",
                sprite_name = "TrophyBoar",
                description = "Kill over <color=orange>100</color> Boars",
                lore =
                    "Do not mess with the boars, for they are known to linger and reap your land till the end of time.",
                defeat_key = "defeated_boar",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.05f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 0.9f },
                    { Modifier.Noise, 1.05f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1.05f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "deer_kill",
                display_name = "Deer Season",
                sprite_name = "TrophyDeer",
                description = "Kill over <color=orange>100</color> Deers",
                lore =
                    "The ancients revered the deer god, Eikthyr, for they knew, with a single blow from it's hard antlers, they would lose all of their will to be.",
                defeat_key = "defeated_deer",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 0.95f },
                    { Modifier.HealthRegen, 0.95f },
                    { Modifier.StaminaRegen, 1.05f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1.05f },
                    { Modifier.Noise, 0.95f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 0.95f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "greyling_kill",
                display_name = "Greyling Massacre",
                sprite_name = "Resin",
                description = "Kill over <color=orange>100</color> Greylings",
                lore =
                    "The mystical forces of the black forest reach beyond it's borders and harass all newcomers with their pesky greylings",
                defeat_key = "defeated_greyling",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.05f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 0.95f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "eikthyr_kill",
                display_name = "Stagbreaker",
                sprite_name = "TrophyEikthyr",
                description = "Kill Eikthyr over <color=orange>10</color> times",
                lore =
                    "The nightmare that is Eikthyr, may finally be laid to rest",
                defeat_key = "defeated_eikthyr",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.05f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 0.95f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "greydwarf_kill",
                display_name = "Entman",
                sprite_name = "TrophyGreydwarf",
                description = "Kill over <color=orange>100</color> Greydwarfs",
                lore =
                    "The pesky greydwarfs will never cease to haunt our dreams.",
                defeat_key = "defeated_greydwarf",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 0.5f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0.05f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "greydwarf_shaman_kill",
                display_name = "Shaman",
                sprite_name = "TrophyGreydwarfShaman",
                description = "Kill over <color=orange>100</color> Greydwarf Shamans",
                lore =
                    "The pesky greydwarf shamans will never cease to haunt our dreams.",
                defeat_key = "defeated_greydwarf_shaman",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.05f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 0.5f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "greydwarf_elite_kill",
                display_name = "Brute",
                sprite_name = "TrophyGreydwarfBrute",
                description = "Kill over <color=orange>100</color> Greydwarf Brutes",
                lore =
                    "The pesky greydwarf brutes will never cease to haunt our dreams.",
                defeat_key = "defeated_greydwarf_elite",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.05f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1.5f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "troll_kill",
                display_name = "Troller",
                sprite_name = "TrophyForestTroll",
                description = "Kill over <color=orange>100</color> Trolls",
                lore =
                    "Fear the might of the trolls, for their swings and punches lay waste to anything it touches.",
                defeat_key = "KilledTroll",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.0f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1.5f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 0.5f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "skeleton_kill",
                display_name = "Break A Leg",
                sprite_name = "bone",
                description = "Kill over <color=orange>100</color> Skeletons",
                lore =
                    "Break a leg so many times that the gods can't be unfavorable towards you.",
                defeat_key = "defeated_skeleton",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.0f },
                    { Modifier.HealthRegen, 1.0f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1.05f },
                    { Modifier.Noise, 1.5f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 0f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.95f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "ghost_kill",
                display_name = "Ghastly",
                sprite_name = "YagluthDrop",
                description = "Kill over <color=orange>100</color> Ghosts",
                lore =
                    "Practically impossible to find any damn ghosts.",
                defeat_key = "defeated_ghost",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Resistant, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.0f },
                    { Modifier.HealthRegen, 1.0f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1.05f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 0f },
                    { Modifier.RunStaminaDrain, 0.5f },
                    { Modifier.DamageReduction, 0.85f },
                    { Modifier.FallDamage, 0.5f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "gdking_1",
                display_name = "Slenderman",
                sprite_name = "TrophyTheElder",
                description = "Kill the Elder over <color=orange>10</color> times",
                lore =
                    "The Ents will forever strike your lineage with all their might.",
                defeat_key = "defeated_gdking",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.0f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1.2f },
                    { Modifier.Speed, 1.0f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "blob_1",
                display_name = "Yuck",
                sprite_name = "TrophyBlob",
                description = "Kill over <color=orange>100</color> Blobs",
                lore =
                    "Don't let their slime fool you. It is actually gross.",
                defeat_key = "defeated_blob",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1.0f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "oozer_1",
                display_name = "Ooze",
                sprite_name = "bottle_green",
                description = "Kill over <color=orange>100</color> Oozers",
                lore =
                    "The stench emitting from these creatures is unbearable.",
                defeat_key = "defeated_blobelite",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1.0f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "draugr_1",
                display_name = "Walkers",
                sprite_name = "TrophyDraugr",
                description = "Kill over <color=orange>100</color> Draugrs",
                lore =
                    "Even the flies around these creatures harm you.",
                defeat_key = "defeated_draugr",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 0.95f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1.1f },
                    { Modifier.DamageReduction, 0.05f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "draugr_2",
                display_name = "Walkers II",
                sprite_name = "TrophyDraugr",
                description = "Kill over <color=orange>100</color> Female Draugrs",
                lore =
                    "Even the flies around these creatures harm you.",
                defeat_key = "defeated_draugr_ranged",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 0.95f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1.1f },
                    { Modifier.DamageReduction, 0.05f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "draugr_3",
                display_name = "Walkers III",
                sprite_name = "TrophyDraugrElite",
                description = "Kill over <color=orange>100</color> Elite Draugrs",
                lore =
                    "Even the flies around these creatures harm you.",
                defeat_key = "defeated_draugr_elite",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 0.95f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1.1f },
                    { Modifier.DamageReduction, 0.05f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "wraith_1",
                display_name = "Wrath",
                sprite_name = "TrophyWraith",
                description = "Kill over <color=orange>100</color> Wraiths",
                lore =
                    "These apparitions can only for tale unresolved issues.",
                defeat_key = "defeated_wraith",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1.05f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 0.95f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.95f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "abomination_1",
                display_name = "A-Bomb",
                sprite_name = "TrophyAbomination",
                description = "Kill over <color=orange>100</color> Abominations",
                lore =
                    "The swamps are full of un-goldy entities. ",
                defeat_key = "defeated_abomination",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 0.95f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "leech_1",
                display_name = "Blood Sucker",
                sprite_name = "TrophyLeech",
                description = "Kill over <color=orange>100</color> Leeches",
                lore =
                    "The blood sucking critters of the swamps is by far the worst of the bunch.",
                defeat_key = "defeated_leech",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "bonemass_1",
                display_name = "Mass",
                sprite_name = "TrophyBonemass",
                description = "Kill Bonemass over <color=orange>10</color> times",
                lore =
                    "It is apt that the swamp king is so bloated and full of himself.",
                defeat_key = "defeated_bonemass",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.15f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "wolf_1",
                display_name = "Husky",
                sprite_name = "TrophyWolf",
                description = "Kill over <color=orange>100</color> Wolves",
                lore =
                    "These creatures are actually quite adorable, once they stop biting.",
                defeat_key = "defeated_wolf",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1.05f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "fenring_1",
                display_name = "Darkly",
                sprite_name = "TrophyFenring",
                description = "Kill over <color=orange>100</color> Fenrings",
                lore =
                    "These wolves look awe-fully different in the dark. Almost human.",
                defeat_key = "defeated_fenring",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "cultist_1",
                display_name = "Cult",
                sprite_name = "TrophyCultist",
                description = "Kill over <color=orange>100</color> Fenring Cultists",
                lore =
                    "To be honest, their cult seems like a good time.",
                defeat_key = "defeated_fenring_cultist",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.9f },
                    { Modifier.EitrRegen, 1.1f }
                }
            },
            new AchievementData()
            {
                unique_name = "golem_1",
                display_name = "Golem",
                sprite_name = "TrophySGolem",
                description = "Kill over <color=orange>100</color> Stone Golems",
                lore =
                    "The absolute worst thing about the mountains. Yet is what makes my blood pump and keeps me warm.",
                defeat_key = "defeated_stonegolem",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "hatchling_1",
                display_name = "Glands",
                sprite_name = "TrophyHatchling",
                description = "Kill over <color=orange>100</color> Drakes",
                lore =
                    "Their is beauty in the joy of shooting down a drake from a high mountain top.",
                defeat_key = "defeated_leech",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.9f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "dragon_1",
                display_name = "Dragon",
                sprite_name = "TrophyDragonQueen",
                description = "Kill Moder over <color=orange>10</color> times",
                lore =
                    "The cries of the dragon queen brings relief to all who lived under her reign.",
                defeat_key = "defeated_dragon",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "goblin_1",
                display_name = "Gubbers",
                sprite_name = "TrophyGoblin",
                description = "Kill over <color=orange>100</color> Fulings",
                lore =
                    "Their laughs haunt the dreams of their unfortunate neighbors.",
                defeat_key = "defeated_goblin",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "goblin_2",
                display_name = "Shamanistic",
                sprite_name = "TrophyGoblinShaman",
                description = "Kill over <color=orange>100</color> Fuling Shamans",
                lore =
                    "The witches of the plains who harness the powers of the gods are a true nightmare.",
                defeat_key = "defeated_goblinshaman",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.1f }
                }
            },
            new AchievementData()
            {
                unique_name = "goblin_3",
                display_name = "Brutish",
                sprite_name = "TrophyGoblinBrute",
                description = "Kill over <color=orange>100</color> Fuling Brutes",
                lore =
                    "With great determination and skill, it is possible to overcome the beasts of the plains.",
                defeat_key = "defeated_goblinbrute",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0.05f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "lox_1",
                display_name = "Loxen",
                sprite_name = "TrophyLox",
                description = "Kill over <color=orange>100</color> Loxens",
                lore =
                    "After consideration, these mighty beasts are a delight to tame and ride around.",
                defeat_key = "defeated_lox",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 150f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "deathsquito_1",
                display_name = "Pistol",
                sprite_name = "TrophyDeathsquito",
                description = "Kill over <color=orange>100</color> Deathsquitoes",
                lore =
                    "The mere sound of their buzz makes any viking shiver in their wet boots.",
                defeat_key = "defeated_deathsquito",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "blobtar_1",
                display_name = "Growth",
                sprite_name = "TrophyGrowth",
                description = "Kill over <color=orange>100</color> Tar Blobs",
                lore =
                    "The ages have been unkind to these creatures, and for that, they are unkind to anything that meets their gaze.",
                defeat_key = "defeated_blobtar",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1.25f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "yagluth_1",
                display_name = "The King",
                sprite_name = "TrophyGoblinKing",
                description = "Kill Yagluth over <color=orange>10</color> times",
                lore =
                    "The gates of hell are protected by the dragging corpse that we call Yagluth.",
                defeat_key = "defeated_goblinking",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.15f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.15f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.1f }
                }
            },
            new AchievementData()
            {
                unique_name = "serpent_1",
                display_name = "Growl",
                sprite_name = "TrophySerpent",
                description = "Kill over <color=orange>100</color> Serpents",
                lore =
                    "The hunt of the serpent is one of the greatest past-times of a viking.",
                defeat_key = "defeated_serpent",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.25f },
                    { Modifier.RaiseSkills, 1.1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "seeker_1",
                display_name = "Bug Off",
                sprite_name = "TrophySeeker",
                description = "Kill over <color=orange>100</color> Seekers",
                lore =
                    "Through the mist, one peers at true horror when faced with a seeker.",
                defeat_key = "defeated_seeker",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.95f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "seekerbrood_1",
                display_name = "Broods",
                sprite_name = "CookedBugMeat",
                description = "Kill over <color=orange>100</color> Seekers Broods",
                lore =
                    "Somehow, they get worse when their smaller.",
                defeat_key = "defeated_seekerbrood",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0.1f },
                    { Modifier.FallDamage, 0.95f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "seeker_soldier_1",
                display_name = "Soldier On",
                sprite_name = "TrophySeekerBrute",
                description = "Kill over <color=orange>100</color> Seeker Soldiers",
                lore =
                    "Through the mist, one peers at true horror when faced with a seeker soldier.",
                defeat_key = "defeated_seekerbrute",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.9f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "tick_1",
                display_name = "Blood Suckers",
                sprite_name = "TrophyTick",
                description = "Kill over <color=orange>100</color> Seekers",
                lore =
                    "The terror of having a creature follow you through a portal.",
                defeat_key = "defeated_seeker",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.95f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "gjall_1",
                display_name = "Y'all",
                sprite_name = "TrophyGjall",
                description = "Kill over <color=orange>100</color> Gjalls",
                lore =
                    "The sound of a gjall shakes the seas and the skies",
                defeat_key = "defeated_gjall",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.9f },
                    { Modifier.EitrRegen, 1.1f }
                }
            },
            new AchievementData()
            {
                unique_name = "dverger_fire_1",
                display_name = "Fireball",
                sprite_name = "FlametalOre",
                description = "Kill over <color=orange>100</color> Fire Mage Dverger",
                lore =
                    "They may look innocent and simple, but they are full of might and magic.",
                defeat_key = "defeated_dvergermagefire",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.2f }
                }
            },
            new AchievementData()
            {
                unique_name = "dverger_ice_1",
                display_name = "Freeze Shards",
                sprite_name = "FreezeGland",
                description = "Kill over <color=orange>100</color> Ice Mage Dverger",
                lore =
                    "They may look innocent and simple, but they are full of might and magic.",
                defeat_key = "defeated_dvergermageice",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.2f }
                }
            },
            new AchievementData()
            {
                unique_name = "dverger_support_1",
                display_name = "Supporter",
                sprite_name = "Lantern",
                description = "Kill over <color=orange>100</color> Support Mage Dverger",
                lore =
                    "They may look innocent and simple, but they are full of might and magic.",
                defeat_key = "defeated_dvergermagesupport",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Resistant, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.2f }
                }
            },
            new AchievementData()
            {
                unique_name = "dverger_fire_1",
                display_name = "Rogue",
                sprite_name = "TrophyDvergr",
                description = "Kill over <color=orange>100</color> Dverger",
                lore =
                    "They may look innocent and simple, but they are full of might and magic.",
                defeat_key = "defeated_dverger",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.2f }
                }
            },
            new AchievementData()
            {
                unique_name = "seeker_queen_1",
                display_name = "Queen",
                sprite_name = "TrophySeekerQueen",
                description = "Kill the Seeker Queen over <color=orange>10</color> times",
                lore =
                    "In the depths of the chambers of the mistland realms, the seeker queen lies and waits for her next prey.",
                defeat_key = "defeated_queen",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 2f }
                }
            },
        };
        output.AddRange(KillAchievements);
        #endregion
        #region Knowledge Achievements
        List<AchievementData> knowledge = new()
        {
            new AchievementData()
            {
                unique_name = "trophies_1",
                display_name = "Decorator",
                sprite_name = "gem",
                description = "Uncover <color=orange>all</color> trophies",
                lore =
                    "The beauty of a home brings joy to all who visit.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Trophies,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 2f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 0f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0.1f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "potions_1",
                display_name = "Addict",
                sprite_name = "bottle_empty",
                description = "Uncover <color=orange>all</color> potions",
                lore =
                    "Life is always better with a potion in hand.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Potions,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 0f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.1f }
                }
            },
            new AchievementData()
            {
                unique_name = "valuables_1",
                display_name = "Wasp",
                sprite_name = "coins_gold",
                description = "Uncover <color=orange>all</color> valuables",
                lore =
                    "Money, money, money.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Valuables,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 2f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 0f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "bows_1",
                display_name = "Ranger",
                sprite_name = "bow",
                description = "Uncover <color=orange>all</color> bows",
                lore =
                    "The relics of time is no match for the eye of an archer.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Bows,
                start_effects = new List<string>() { "fx_DvergerMage_Support_start" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 0f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.5f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "staves_1",
                display_name = "Blaster",
                sprite_name = "StaffSkeleton",
                description = "Uncover <color=orange>all</color> staves",
                lore =
                    "The remains of a scarred battlefield left behind by magic leaves deep wounds to the earth itself.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Staves,
                start_effects = new List<string>() { "fx_DvergerMage_Support_start" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 0f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 2f }
                }
            },
            new AchievementData()
            {
                unique_name = "shields_1",
                display_name = "Defender",
                sprite_name = "shield",
                description = "Uncover <color=orange>all</color> shields",
                lore =
                    "Defence is sometimes the greatest offense.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Shields,
                start_effects = new List<string>() { "vfx_HealthUpgrade" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 0.5f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0.5f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "knives_1",
                display_name = "Backstabber",
                sprite_name = "KnifeSkollAndHati",
                description = "Uncover <color=orange>all</color> knives",
                lore =
                    "The deadly silence of death by knives brings terror to all.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Knives,
                start_effects = new List<string>() { "vfx_Potion_health_medium" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "maces_1",
                display_name = "Bludgeon",
                sprite_name = "MaceSilver",
                description = "Uncover <color=orange>all</color> maces",
                lore =
                    "The bloody remains of a bludgeoned warrior brings fear through generations.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Maces,
                start_effects = new List<string>() { "vfx_Potion_health_medium" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "spears_1",
                display_name = "Poke",
                sprite_name = "SpearBronze",
                description = "Uncover <color=orange>all</color> spears",
                lore =
                    "A spear can do no harm, it is useful at range just as it is in the melee.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Spears,
                start_effects = new List<string>() { "vfx_Potion_health_medium" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "polearms_1",
                display_name = "Dancer",
                sprite_name = "AtgeirIron",
                description = "Uncover <color=orange>all</color> pole-arms",
                lore =
                    "Truly a spectacle for the ages, when a master dances with their pole-arm.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.PoleArms,
                start_effects = new List<string>() { "vfx_Potion_health_medium" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "axes_1",
                display_name = "Chopper",
                sprite_name = "AxeIron",
                description = "Uncover <color=orange>all</color> axes",
                lore =
                    "Be wary of people with axes, for they do not only fell trees.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Axes,
                start_effects = new List<string>() { "vfx_Potion_health_medium" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "swords_1",
                display_name = "Slasher",
                sprite_name = "sword_brown",
                description = "Uncover <color=orange>all</color> swords",
                lore =
                    "The art of swordsmanship does not come easily, it is full of cuts and bruises.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Swords,
                start_effects = new List<string>() { "fx_DvergerMage_Support_start" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "weapon_1",
                display_name = "Armory",
                sprite_name = "sword_blue",
                description = "Uncover <color=orange>all</color> weapons",
                lore =
                    "Only those who test all the different options available, can accurately decide what is best for any situation.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Weapons,
                start_effects = new List<string>() { "fx_DvergerMage_Support_start" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 2f },
                    { Modifier.HealthRegen, 2f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "consumable_1",
                display_name = "Glutton",
                sprite_name = "mushroom",
                description = "Uncover <color=orange>all</color> materials",
                lore =
                    "Now that is what we call a viking's appetite.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Consumables,
                start_effects = new List<string>() { "fx_DvergerMage_Support_start" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "material_1",
                display_name = "Hoarder",
                sprite_name = "log_stack",
                description = "Uncover <color=orange>all</color> materials",
                lore =
                    "To be sure to succeed, one must know all the materials that one can use to defeat one's foes.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Materials,
                start_effects = new List<string>() { "fx_DvergerMage_Support_start" },
                stop_effects = new List<string>() { "fx_DvergerMage_Mistile_die" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 2f },
                    { Modifier.StaminaRegen, 2f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "fish_1",
                display_name = "Fisherman",
                sprite_name = "fish",
                description = "Uncover <color=orange>all</color> fishes",
                lore =
                    "A true conqueror has successfully dominated all ocean dwellers as well as terrestrial beings.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Fish,
                start_effects = new List<string>() { "vfx_lox_love" },
                stop_effects = new List<string>() { "vfx_GodExplosion" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 2f },
                    { Modifier.RaiseSkills, 2f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "arrow_1",
                display_name = "Whittler",
                sprite_name = "arrow",
                description = "Uncover <color=orange>all</color> projectiles",
                lore =
                    "A true survivalist knows well that range is your greatest ally.",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.Arrows,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.5f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 2f },
                    { Modifier.RunStaminaDrain, 0.5f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
        };
        output.AddRange(knowledge);
        #endregion
        #region Player Stats

        List<AchievementData> playerStats = new()
        {
            new AchievementData()
            {
                unique_name = "enemy_kills_1",
                display_name = "Enemy #1",
                sprite_name = "MaceNeedle",
                description = "Kill over <color=orange>1000</color> creatures",
                lore = "Creatures tremble by the sight of a vikings deeds",
                defeat_key = "",
                goal = 1000,
                achievement_type = AchievementTypes.AchievementType.EnemyKills,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.2f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "base_time_1",
                display_name = "Homely",
                sprite_name = "crown",
                description = "Stay in base for over <color=orange>100000</color> seconds",
                lore = "To build a home and live in it is the true luxury of life.",
                defeat_key = "",
                goal = 100000,
                achievement_type = AchievementTypes.AchievementType.TimeInBase,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1.1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "out_time_1",
                display_name = "Adventurer",
                sprite_name = "map",
                description = "Stay out of base for over <color=orange>100000</color> seconds",
                lore = "To thrive in the wild is the greatest skill.",
                defeat_key = "",
                goal = 100000,
                achievement_type = AchievementTypes.AchievementType.TimeOutOfBase,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1.1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 100f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "shooter_1",
                display_name = "Sharpshooter",
                sprite_name = "ArrowFire",
                description = "Shoot over <color=orange>1000</color> arrows",
                lore = "Practice makes perfect, at least when it comes to aiming.",
                defeat_key = "",
                goal = 1000,
                achievement_type = AchievementTypes.AchievementType.ArrowsShot,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Weak, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "runner_1",
                display_name = "Runner",
                sprite_name = "hood",
                description = "Run for over <color=orange>10000</color> units",
                lore = "To build a home and live in it is the true luxury of life.",
                defeat_key = "",
                goal = 10000,
                achievement_type = AchievementTypes.AchievementType.DistanceRan,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1.2f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 50f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "miner_1",
                display_name = "Quarry",
                sprite_name = "PickaxeIron",
                description = "Mine over <color=orange>100</color> ores",
                lore = "True wealth comes to those who work for it.",
                defeat_key = "",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.TotalMined,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 150f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "lumber_1",
                display_name = "Lumberman",
                sprite_name = "log",
                description = "Chop down over <color=orange>1000</color> trees",
                lore = "A great viking knows the value of wood.",
                defeat_key = "",
                goal = 1000,
                achievement_type = AchievementTypes.AchievementType.TreesChopped,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 150f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "lore_1",
                display_name = "Runic",
                sprite_name = "necklace",
                description = "Discover over <color=orange>10</color> runestone lores",
                lore = "The wealth of knowledge is priceless.",
                defeat_key = "",
                goal = 10,
                achievement_type = AchievementTypes.AchievementType.RuneStones,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1.05f },
                    { Modifier.StaminaRegen, 1.05f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1.05f }
                }
            },
        };
        output.AddRange(playerStats);
        #endregion
        #region Per Biome
        List<AchievementData> biomes = new()
        {
            new AchievementData()
            {
                unique_name = "meadows_1",
                display_name = "Meadows",
                sprite_name = "Flint",
                description = "Kill <color=orange>all</color> meadow creatures",
                lore = "The meadows may be the beginning but it is nevertheless, ruthless",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.MeadowCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "blackforest_1",
                display_name = "Black Forest",
                sprite_name = "SurtlingCore",
                description = "Kill <color=orange>all</color> black forest creatures",
                lore = "The black forest is treacherous and dangerous",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.BlackForestCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "swamps_1",
                display_name = "Swamps",
                sprite_name = "IronScrap",
                description = "Kill <color=orange>all</color> swamp creatures",
                lore = "Slush and dredge through the swamps for the glory of all vikings",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.SwampCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "mountains_1",
                display_name = "Mountains",
                sprite_name = "Obsidian",
                description = "Kill <color=orange>all</color> mountains creatures",
                lore = "The high mountain tops are where legends are born",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.MountainCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "plains_1",
                display_name = "Plains",
                sprite_name = "Flax",
                description = "Kill <color=orange>all</color> plains creatures",
                lore = "The great plains, where lox roam and goblins thrive are full of unknown bounties",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.PlainsCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
            new AchievementData()
            {
                unique_name = "mistlands_1",
                display_name = "Mistlands",
                sprite_name = "BlackCore",
                description = "Kill <color=orange>all</color> mistlands creatures",
                lore = "To peer through the mist, you find treasures and ancient relics",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.MistLandCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 0.8f },
                    { Modifier.EitrRegen, 1.05f }
                }
            },
            new AchievementData()
            {
                unique_name = "ocean_1",
                display_name = "Ocean",
                sprite_name = "SerpentScale",
                description = "Kill <color=orange>all</color> ocean creatures",
                lore = "The high seas are where the real treasures lie",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.OceanCreatures,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1.2f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            },
        };
        output.AddRange(biomes);
        #endregion
        #region Custom Creature Groups
        List<AchievementData> CustomCreatureGroups = new()
        {
            new AchievementData()
            {
                unique_name = "a_custom_brutes_1",
                display_name = "Brutish",
                sprite_name = "Obsidian",
                description = "Kill <color=orange>all</color> brute creatures",
                lore = "A true viking never shies away from brutes",
                defeat_key = "",
                goal = 0,
                achievement_type = AchievementTypes.AchievementType.CustomCreatureGroups,
                custom_group_key = "Custom_Brutes",
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1.05f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1.05f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            }
        };
        output.AddRange(CustomCreatureGroups);
        #endregion
        #region Item Rewards
        
        List<AchievementData> ItemRewards = new()
        {
            new AchievementData()
            {
                unique_name = "b_item_reward_1",
                display_name = "Riches",
                sprite_name = "Coins",
                description = "Kill over <color=orange>200</color> creatures",
                lore = "The gods shall reward you for pruning midgard of the overpopulating creatures",
                defeat_key = "",
                goal = 200,
                achievement_type = AchievementTypes.AchievementType.EnemyKills,
                custom_group_key = "",
                reward_type = AchievementTypes.AchievementRewardType.Item,
                item = "Coins",
                item_amount = 999,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            }
        };
        output.AddRange(ItemRewards);
        #endregion
        #region Skill Rewards
        
        List<AchievementData> SkillRewards = new()
        {
            new AchievementData()
            {
                unique_name = "c_skill_reward_1",
                display_name = "Lumberman",
                sprite_name = "log_stack",
                description = "Chop down over <color=orange>100</color> trees",
                lore = "Cutting trees is only natural for a viking",
                defeat_key = "",
                goal = 100,
                achievement_type = AchievementTypes.AchievementType.TreesChopped,
                reward_type = AchievementTypes.AchievementRewardType.Skill,
                custom_group_key = "",
                skill = "WoodCutting",
                skill_amount = 100,
                start_effects = new List<string>() { "sfx_coins_placed" },
                damage_modifiers = new List<HitData.DamageModPair>()
                {
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Pickaxe, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal, },
                    new HitData.DamageModPair()
                        { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal, },
                },
                modifiers = new Dictionary<Modifier, float>()
                {
                    { Modifier.Attack, 1f },
                    { Modifier.HealthRegen, 1f },
                    { Modifier.StaminaRegen, 1f },
                    { Modifier.RaiseSkills, 1f },
                    { Modifier.Speed, 1f },
                    { Modifier.Noise, 1f },
                    { Modifier.MaxCarryWeight, 0f },
                    { Modifier.Stealth, 1f },
                    { Modifier.RunStaminaDrain, 1f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
                }
            }
        };
        output.AddRange(SkillRewards);
        #endregion
        #region grouped achievements

        List<AchievementData> GroupedAchievements = new()
        {
            new AchievementData()
            {
                unique_name = "neck_group_1",
                display_name = "Neck Hunter",
                sprite_name = "TrophyNeck",
                description = "Kill over <color=orange>30</color> necks\n<color=orange>(1/3)</color>",
                lore = "A viking's journey begins by the waters, facing the necks",
                defeat_key = "defeated_neck",
                achievement_group = "neck_group",
                achievement_index = 1,
                goal = 30,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                reward_type = AchievementTypes.AchievementRewardType.Item,
                start_effects = new List<string>() { "sfx_coins_placed" },
                item = "Flint",
                item_amount = 50,
            },
            new AchievementData()
            {
                unique_name = "neck_group_2",
                display_name = "Neck Murderer",
                sprite_name = "TrophyNeck",
                description = "Kill over <color=orange>60</color> necks\n<color=orange>(2/3)</color>",
                lore = "A viking's journey begins by the waters, facing the necks",
                defeat_key = "defeated_neck",
                achievement_group = "neck_group",
                achievement_index = 2,
                goal = 60,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                reward_type = AchievementTypes.AchievementRewardType.Item,
                start_effects = new List<string>() { "sfx_coins_placed" },
                item = "Flint",
                item_amount = 50,
            },
            new AchievementData()
            {
                unique_name = "neck_group_3",
                display_name = "Neck Craze",
                sprite_name = "TrophyNeck",
                description = "Kill over <color=orange>90</color> necks\n<color=orange>(3/3)</color>",
                lore = "A viking's journey begins by the waters, facing the necks",
                defeat_key = "defeated_neck",
                achievement_group = "neck_group",
                achievement_index = 3,
                goal = 90,
                achievement_type = AchievementTypes.AchievementType.CustomKills,
                reward_type = AchievementTypes.AchievementRewardType.Item,
                start_effects = new List<string>() { "sfx_coins_placed" },
                item = "Flint",
                item_amount = 50,
            },
        };
        
        output.AddRange(GroupedAchievements);
        #endregion
        return output;
    }
}