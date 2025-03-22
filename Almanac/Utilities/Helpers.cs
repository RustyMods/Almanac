using System.Text.RegularExpressions;
using Almanac.Achievements;
using UnityEngine;

namespace Almanac.Utilities;

public static class Helpers
{
    public static readonly Color32 OrangeColor = new (255, 164, 0, 255);
    public static string ConvertSkills(Skills.SkillType type)
    {
        return (type) switch
        {
            Skills.SkillType.None => "$text_none",
            Skills.SkillType.Swords => "$skill_swords",
            Skills.SkillType.Knives => "$skill_knives",
            Skills.SkillType.Clubs => "$skill_clubs",
            Skills.SkillType.Polearms => "$skill_polearms",
            Skills.SkillType.Spears => "$skill_spears",
            Skills.SkillType.Blocking => "$skill_shields",
            Skills.SkillType.Axes => "$skill_axes",
            Skills.SkillType.Bows => "$skill_bows",
            Skills.SkillType.ElementalMagic => "$skill_elementalmagic",
            Skills.SkillType.BloodMagic => "$skill_bloodmagic",
            Skills.SkillType.Unarmed => "$skill_unarmed",
            Skills.SkillType.WoodCutting => "$skill_woodcutting",
            Skills.SkillType.Crossbows => "$skill_crossbows",
            Skills.SkillType.Jump => "$skill_jump",
            Skills.SkillType.Sneak => "$skill_sneak",
            Skills.SkillType.Run => "$skill_run",
            Skills.SkillType.Swim => "$skill_swim",
            Skills.SkillType.Fishing => "$skill_fishing",
            Skills.SkillType.Ride => "$skill_ride",
            Skills.SkillType.All => "$skill_all",
             _ => "Unknown Skill"
        };
    }
    public static string ConvertBoolean(bool input)
    {
        return input switch
        {
            true => "$text_true",
            false => "$text_false",
        };
    }

    public static string ConvertEffectModifiers(EffectMan.Modifier type)
    {
        return type switch
        {
            EffectMan.Modifier.None => "$text_none",
            EffectMan.Modifier.Attack => "$se_attack",
            EffectMan.Modifier.HealthRegen => "$se_healthregen",
            EffectMan.Modifier.StaminaRegen => "$se_staminaregen",
            EffectMan.Modifier.RaiseSkills => "$se_raiseskill",
            EffectMan.Modifier.Speed => "$se_speed",
            EffectMan.Modifier.Noise => "$se_noisemod",
            EffectMan.Modifier.MaxCarryWeight => "$se_max_carryweight",
            EffectMan.Modifier.Stealth => "$se_stealth",
            EffectMan.Modifier.RunStaminaDrain => "$se_runstamina",
            EffectMan.Modifier.DamageReduction => "$se_damagereduction",
            EffectMan.Modifier.FallDamage => "$item_falldamage",
            EffectMan.Modifier.EitrRegen => "$se_eitrregen",
            EffectMan.Modifier.Health => "$se_health",
            EffectMan.Modifier.Stamina => "$se_stamina",
            EffectMan.Modifier.Eitr => "$se_eitr",
            EffectMan.Modifier.LifeSteal => "$se_lifesteal",
            EffectMan.Modifier.Armor => "$item_armor",
            _ => "$text_none"
        };
    }

    public static string SplitCamelCase(string input)
    {
        string result = Regex.Replace(input, "([A-Z])", " $1");
            
        result = Regex.Replace(result, "([A-Z]+)([A-Z][a-z])", "$1 $2");

        return result.TrimStart();
    }
}