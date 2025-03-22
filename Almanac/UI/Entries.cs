using System;
using System.Collections.Generic;
using Almanac.Achievements;
using BepInEx;
using UnityEngine;
using static Almanac.Data.PlayerStats;
using static Almanac.Utilities.Helpers;

namespace Almanac.UI;
public static class Entries
{
    public class EntryBuilder
    {
        private readonly List<Entry> m_list = new();
        public bool m_showAll;

        public List<Entry> ToList() => m_list;

        // Base
        public void Add(string title) => Add(title, "title");

        public void Add(string key, string value)
        {
            if (value.IsNullOrWhiteSpace() && !m_showAll) return;
            m_list.Add(new Entry(key, value));
        }
        //
        public void Add(EffectMan.Modifier mod, float value, Option option = Option.None) => Add(ConvertEffectModifiers(mod), value, option);
        public void Add(AchievementTypes.AchievementType type, string value) => Add($"$achievement_{type.ToString().ToLower()}", value);
        public void Add(PlayerStatType type) => Add($"$playerstat_{type.ToString().ToLower()}", GetPlayerStat(type));
        public void Add(HitData.DamageModPair pair)
        {
            if (pair.m_modifier is HitData.DamageModifier.Normal && !m_showAll) return;
            Add(pair.m_type, pair.m_modifier);
        }

        public void Add(HitData.DamageType type, HitData.DamageModifier modifier)
        {
            Add($"$inventory_{type.ToString().ToLower()}", $"$inventory_{modifier.ToString().ToLower()}");
        }

        public void Add(string key, Character.Faction faction) => Add(key, $"$faction_{faction.ToString().ToLower()}");
        public void Add(Skills.SkillType type, float value, Option option = Option.None) => Add($"$skill_{type.ToString().ToLower()}", value, option);
        public void Add(string key, Material? material) => Add(key, material == null ? "$almanac_none" : material.name);
        public void Add(string key, float value, float perLvl, string postfix) => Add(key, $"{value} +{perLvl}{postfix}");
        public void Add(string key, StatusEffect? statusEffect) => Add(key, statusEffect == null ? "$almanac_none" : statusEffect.m_name);
        public void Add(string key, HitData.DamageModifier modifier) => Add(key, $"$inventory_{modifier.ToString().ToLower()}");
        public void Add(string key, Enum type) => Add(key, SplitCamelCase(type.ToString()));
        public void Add(string key, CraftingStation? station) => Add(key, station == null ? "$almanac_none" : station.m_name);
        public void Add(string key, GameObject? value) => Add(key, value == null ? "$almanac_none" : value.name);
        public void Add(string key, Turret.AmmoType ammo) => Add(key, ammo.m_ammo);
        public void Add(ItemDrop from, ItemDrop to) => Add(from.m_itemData.m_shared.m_name, to.m_itemData.m_shared.m_name);
        public void Add(string key, bool value) => Add(key, value ? "$text_true" : "$text_false");
        public void Add(string key, ItemDrop? item) => Add(key, item == null ? "$almanac_none" : item.m_itemData.m_shared.m_name);
        public void Add(string key, int pre, int post, string separator) => Add(key, $"{pre}<color=orange>{separator}</color>{post}");
        public void Add(string key, Heightmap.Biome biome)
        {
            var parts = biome.ToString().Split(',');
            foreach(var part in parts) Add(key, part.Trim());
        }

        public void Add(string key, float value, string postfix) => Add(key, value.ToString("0") + postfix);
        
        public void Add(string key, float value, Option option = Option.None)
        {
            if (value == 0f && !m_showAll) return;
            switch (option)
            {
                case Option.Minutes:
                    Add(key, value.ToString("0.0") + "<color=orange>min</color>");
                    break;
                case Option.Seconds:
                    Add(key, value.ToString("0.0") + "<color=orange>s</color>");
                    break;
                case Option.Percentage:
                    Add(key, (value * 100).ToString("0.0") + "<color=orange>%</color>");
                    break;
                case Option.PerLevel:
                    Add(key, value.ToString("0.0") + "<color=orange>/lvl</color>");
                    break;
                case Option.Degree:
                    Add(key, value.ToString("0") + "<color=orange>°</color>");
                    break;
                case Option.Add:
                    Add(key, "<color=orange>+</color>" + value.ToString("0.0"));
                    break;
                case Option.Level:
                    Add(key, value.ToString("0") + "<color=orange>lvl</color>");
                    break;
                case Option.XP:
                    Add(key, value.ToString("0") + "<color=orange>XP</color>");
                    break;
                default: 
                    Add(key, value.ToString("0.0"));
                    break;
            }
        }
        public void Add(string key, int value, Option option = Option.None)
        {
            if (value == 0 && !m_showAll) return;
            switch (option)
            {
                case Option.Minutes:
                    Add(key, value + "<color=orange>min</color>");
                    break;
                case Option.Seconds:
                    Add(key, value + "<color=orange>s</color>");
                    break;
                case Option.Percentage:
                    Add(key, value * 100 + "<color=orange>%</color>");
                    break;
                case Option.PerLevel:
                    Add(key, value + "<color=orange>/lvl</color>");
                    break;
                case Option.Degree:
                    Add(key, value + "<color=orange>°</color>");
                    break;
                case Option.Add:
                    Add(key, "<color=orange>+</color>" + value);
                    break;
                case Option.Level:
                    Add(key, value + "<color=orange>lvl</color>");
                    break;
                case Option.XP:
                    Add(key, value + "<color=orange>XP</color>");
                    break;
                default:
                    Add(key, value.ToString());
                    break;
            }
        }

        public enum Option
        {
            None, Percentage, Seconds, Minutes, PerLevel, Degree, Add, Minus, Level, XP
        }
    }
    public class Entry
    {
        public string title { get; set; }
        public string value { get; set; }

        public Entry(string Title, string Value)
        {
            title = Title;
            value = Value;
        }
    }
}