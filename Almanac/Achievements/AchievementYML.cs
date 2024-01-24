using System;
using System.Collections.Generic;
using System.IO;
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
        public int goal = 0;
        public int duration = 0;
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
            { Modifier.RaiseSkills, 1.1f },
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

    public static void InitDefaultAchievements()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        foreach (AchievementData achievement in GetDefaultAchievements())
        {
            string path = AlmanacPaths.AchievementFolderPath + Path.DirectorySeparatorChar + achievement.unique_name + ".yml";
            if (!File.Exists(path))
            {
                string data = serializer.Serialize(achievement);
                File.WriteAllText(path, data);
            }
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
                    { Modifier.RunStaminaDrain, 0f },
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
                    { Modifier.RunStaminaDrain, 0f },
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
                    { Modifier.RunStaminaDrain, 0f },
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
                    { Modifier.RunStaminaDrain, 0f },
                    { Modifier.DamageReduction, 0f },
                    { Modifier.FallDamage, 1f },
                    { Modifier.EitrRegen, 1f }
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
                duration = 100,
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
                duration = 100,
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
                    { Modifier.HealthRegen, 0f },
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
                duration = 100,
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
        
        return output;
    }
}