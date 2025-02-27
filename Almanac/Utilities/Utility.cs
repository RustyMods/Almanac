﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Almanac.Achievements;
using Almanac.UI;
using TMPro;
using UnityEngine;

namespace Almanac.Utilities;

public static class Utility
{
    private static readonly Color32 OrangeColor = new (255, 164, 0, 255);

    public static string FormatTreasureRewardText(TreasureHunt.Data.ValidatedTreasure data)
    {
        string output = "$almanac_reward:\n";
        foreach (var drop in data.m_dropTable.m_drops)
        {
            string name =
                Localization.instance.Localize(drop.m_item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name);
            output +=
                $"<color=orange>{drop.m_stackMin}</color> - <color=orange>{drop.m_stackMax}</color> x <color=yellow>{name}</color>\n";
        }
        return output;
    }
    public static string FormatBountyRewardText(Bounties.Data.ValidatedBounty data)
    {
        switch (data.m_rewardType)
        {
            case Bounties.Data.QuestRewardType.Item:
                if (data.m_itemReward == null) return "";
                return $"$almanac_reward:\n <color=orange>{data.m_itemAmount}</color>x {data.m_itemReward.m_itemData.m_shared.m_name}";
            case Bounties.Data.QuestRewardType.Skill:
                return $"$almanac_reward:\n <color=orange>{data.m_skillAmount}</color>$almanac_xp <color=orange>{ConvertSkills(data.m_skill)}</color>";
            default:
                return "";
        }
    }

    public static string FormatBountyDetails(Bounties.Data.ValidatedBounty data)
    {
        return $"                       $almanac_bounty: <color=yellow>{data.m_critter.name}</color>" 
               + $"\n                       $almanac_biome: <color=orange>{data.m_biome}</color>"
               + $"\n                       $almanac_health: <color=orange>{data.m_health}</color>"
               + $"\n                       $almanac_damage Multiplier: <color=orange>{data.m_damageMultiplier}</color>"
               + $"\n                       $almanac_level: <color=orange>{data.m_level}</color>"
               + "\n                       " + FormatBountyDamages(data);
    }

    private static string FormatBountyDamages(Bounties.Data.ValidatedBounty data)
    {
        string result = "";
        if (data.m_damages.blunt > 0) result += $"$almanac_blunt: <color=orange>{data.m_damages.blunt}</color> ";
        if (data.m_damages.slash > 0) result += $"$almanac_slash: <color=orange>{data.m_damages.slash}</color> ";
        if (data.m_damages.pierce > 0) result += $"$almanac_pierce: <color=orange>{data.m_damages.pierce}</color> ";
        if (data.m_damages.fire > 0) result += $"$almanac_fire: <color=orange>{data.m_damages.fire}</color> ";
        if (data.m_damages.frost > 0) result += $"$almanac_frost: <color=orange>{data.m_damages.frost}</color> ";
        if (data.m_damages.lightning > 0) result += $"$almanac_lightning: <color=orange>{data.m_damages.lightning}</color> ";
        if (data.m_damages.poison > 0) result += $"$almanac_poison: <color=orange>{data.m_damages.poison}</color> ";
        if (data.m_damages.spirit > 0) result += $"$almanac_spirit: <color=orange>{data.m_damages.spirit}</color> ";
        return result;
    }
    public static string ReplaceSpaceWithNewLine(string input) => input.Replace(' ', '\n');
    public static void MergeDictionaries(Dictionary<string, string> destination, Dictionary<string, string> source)
    {
        foreach (KeyValuePair<string, string> kvp in source)
        {
            if (destination.ContainsKey(kvp.Key))
            {
                if (destination.ContainsKey(kvp.Key + 1)) continue;
                destination.Add(kvp.Key + 1, kvp.Value);
            }
            else
            {
                destination.Add(kvp.Key, kvp.Value);
            }
        }
    }
    public static string RemoveNumbers(string input) => Regex.Replace(input, @"\d", "");
    public static string SplitCamelCase(string input)
    {
        string result = Regex.Replace(input, "([A-Z])", " $1");
            
        result = Regex.Replace(result, "([A-Z]+)([A-Z][a-z])", "$1 $2");

        return result.TrimStart();
    }
    public static string RemoveParentheses(string input) => Regex.Replace(input, @"\([^)]*\)", "");
    public static TextMeshProUGUI AddTextMeshProGUI(GameObject prefab, bool bold = false, TextWrappingModes wrap = TextWrappingModes.Normal)
    {
        TextMeshProUGUI TMP = prefab.AddComponent<TextMeshProUGUI>();
        TMP.font = bold ? CacheAssets.NorseFontBold : CacheAssets.NorseFont;
        if (bold) TMP.fontMaterial = CacheAssets.TopicTextMeshPro.fontMaterial;
        if (bold) TMP.material = CacheAssets.TopicTextMeshPro.material;
        TMP.fontSize = 14;
        TMP.fontSizeMin = 12;
        TMP.fontSizeMax = 16;
        TMP.autoSizeTextContainer = false;
        TMP.textWrappingMode = wrap;
        TMP.overflowMode = TextOverflowModes.Overflow;
        TMP.verticalAlignment = VerticalAlignmentOptions.Middle;
        TMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
        TMP.color = OrangeColor;
        TMP.richText = true;

        return TMP;
    }

    public static string ConvertDamageModifiers(HitData.DamageModifier mod)
    {
        return (mod) switch
        {
            HitData.DamageModifier.Normal => "$almanac_normal",
            HitData.DamageModifier.Ignore => "$almanac_ignore",
            HitData.DamageModifier.Immune => "$almanac_immune",
            HitData.DamageModifier.VeryResistant => "$almanac_very_resistant",
            HitData.DamageModifier.Resistant => "$almanac_resistant",
            HitData.DamageModifier.Weak => "$almanac_weak",
            HitData.DamageModifier.VeryWeak => "$almanac_very_weak",
            _ => "Unknown Damage Modifier"
        };
    }

    public static string ConvertDamageTypes(HitData.DamageType type)
    {
        return (type) switch
        {
            HitData.DamageType.Blunt => "$almanac_blunt",
            HitData.DamageType.Slash => "$almanac_slash",
            HitData.DamageType.Pierce => "$almanac_pierce",
            HitData.DamageType.Chop => "$almanac_chop",
            HitData.DamageType.Pickaxe => "$almanac_pickaxe",
            HitData.DamageType.Fire => "$almanac_fire",
            HitData.DamageType.Frost => "$almanac_frost",
            HitData.DamageType.Lightning => "$almanac_lightning",
            HitData.DamageType.Poison => "$almanac_poison",
            HitData.DamageType.Spirit => "$almanac_spirit",
            HitData.DamageType.Physical => "$almanac_physical",
            HitData.DamageType.Elemental => "$almanac_elemental",
            _ => "Unknown Damage Type"
        };
    }

    public static string ConvertSkills(Skills.SkillType type)
    {
        return (type) switch
        {
            Skills.SkillType.None => "$almanac_none",
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

    public static string ConvertAttackTypes(Attack.AttackType type)
    {
        return (type) switch
        {
            Attack.AttackType.Horizontal => "$almanac_horizontal",
            Attack.AttackType.Vertical => "$almanac_vertical",
            Attack.AttackType.Projectile => "$almanac_ammo_button",
            Attack.AttackType.None => "$almanac_none",
            Attack.AttackType.Area => "$almanac_area",
            Attack.AttackType.TriggerProjectile => "$almanac_trigger_projectile",
            _ => "Unknown Attack Type"
        };
    }

    public static string ConvertItemType(ItemDrop.ItemData.ItemType type)
    {
        return type switch
        {
            ItemDrop.ItemData.ItemType.None => "$almanac_none",
            ItemDrop.ItemData.ItemType.Material => "$almanac_material_button",
            ItemDrop.ItemData.ItemType.Consumable => "$almanac_consumable_button",
            ItemDrop.ItemData.ItemType.OneHandedWeapon => "$item_onehanded",
            ItemDrop.ItemData.ItemType.Bow => "$skill_bow",
            ItemDrop.ItemData.ItemType.Shield => "$skill_shields",
            ItemDrop.ItemData.ItemType.Helmet => "$almanac_helmet",
            ItemDrop.ItemData.ItemType.Chest => "$almanac_chest",
            ItemDrop.ItemData.ItemType.Ammo => "$almanac_ammo_button",
            ItemDrop.ItemData.ItemType.Customization => "$almanac_customization",
            ItemDrop.ItemData.ItemType.Legs => "$almanac_legs",
            ItemDrop.ItemData.ItemType.Hands => "$almanac_hands",
            ItemDrop.ItemData.ItemType.Trophy => "$almanac_trophies_button",
            ItemDrop.ItemData.ItemType.TwoHandedWeapon => "$item_twohanded",
            ItemDrop.ItemData.ItemType.Torch => "$item_torch",
            ItemDrop.ItemData.ItemType.Misc => "$almanac_miscPieces_button",
            ItemDrop.ItemData.ItemType.Shoulder => "$almanac_shoulder",
            ItemDrop.ItemData.ItemType.Utility => "$almanac_utility",
            ItemDrop.ItemData.ItemType.Tool => "$almanac_tool",
            ItemDrop.ItemData.ItemType.Attach_Atgeir => "$almanac_attach_atgeir",
            ItemDrop.ItemData.ItemType.Fish => "$almanac_fish_button",
            ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft => "$item_twohanded $almanac_left",
            ItemDrop.ItemData.ItemType.AmmoNonEquipable => "$almanac_ammo_non_equip",
            _ => "Unknown Item Type"
        };
    }

    public static string ConvertFactions(Character.Faction faction)
    {
        return (faction) switch
        {
            Character.Faction.Players => "$almanac_players",
            Character.Faction.AnimalsVeg => "$almanac_animals_veg",
            Character.Faction.ForestMonsters => "$almanac_forest_monsters",
            Character.Faction.Undead => "$almanac_undead",
            Character.Faction.Demon => "$almanac_demon",
            Character.Faction.MountainMonsters => "$almanac_mountain_monsters",
            Character.Faction.SeaMonsters => "$almanac_sea_monsters",
            Character.Faction.PlainsMonsters => "$almanac_plains_monsters",
            Character.Faction.Boss => "$almanac_boss",
            Character.Faction.Dverger => "$almanac_dverger",
            Character.Faction.MistlandsMonsters => "$almanac_mistlands_monsters",
            _ => "Unknown Faction",
        };
    }

    public static string ConvertBoolean(bool input)
    {
        return (input) switch
        {
            true => "$almanac_true",
            false => "$almanac_false",
        };
    }

    public static string ConvertEffectModifiers(AlmanacEffectManager.Modifier type)
    {
        return (type) switch
        {
            AlmanacEffectManager.Modifier.None => "$almanac_none",
            AlmanacEffectManager.Modifier.Attack => "$almanac_attack",
            AlmanacEffectManager.Modifier.HealthRegen => "$se_healthregen",
            AlmanacEffectManager.Modifier.StaminaRegen => "$se_staminaregen",
            AlmanacEffectManager.Modifier.RaiseSkills => "$almanac_raise_skill",
            AlmanacEffectManager.Modifier.Speed => "$almanac_speed",
            AlmanacEffectManager.Modifier.Noise => "$almanac_noise",
            AlmanacEffectManager.Modifier.MaxCarryWeight => "$almanac_max_carry_weight",
            AlmanacEffectManager.Modifier.Stealth => "$almanac_stealth",
            AlmanacEffectManager.Modifier.RunStaminaDrain => "$almanac_run_stamina_drain",
            AlmanacEffectManager.Modifier.DamageReduction => "$almanac_damage_reduction",
            AlmanacEffectManager.Modifier.FallDamage => "$almanac_fall_damage",
            AlmanacEffectManager.Modifier.EitrRegen => "$se_eitrregen",
            AlmanacEffectManager.Modifier.Health => "$se_health",
            AlmanacEffectManager.Modifier.Stamina => "$se_stamina",
            AlmanacEffectManager.Modifier.Eitr => "$se_eitr",
            AlmanacEffectManager.Modifier.LifeSteal => "$se_lifesteal",
            AlmanacEffectManager.Modifier.Armor => "$se_armor",
            _ => "Unknown Modifier"
        };
    }
}