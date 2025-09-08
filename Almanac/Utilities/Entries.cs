using System;
using System.Collections.Generic;
using Almanac.Achievements;
using Almanac.UI;
using UnityEngine;
using static Almanac.Data.PlayerInfo;
using static Almanac.Utilities.Helpers;

namespace Almanac.Utilities;
public static class Entries
{
    public class EntryBuilder
    {
        private readonly List<Entry> m_list = new();
        public bool m_showAll;
        public void Clear() => m_list.Clear();
        public List<Entry> ToList() => m_list;

        // Base
        public void Add(string title) => Add(title, "title");
        public void AddRange(List<Entry> entries) => m_list.AddRange(entries);

        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(value) && !m_showAll) return;
            m_list.Add(new Entry(key, value));
        }

        public void Add(string key, string value, string defaultValue)
        {
            m_list.Add(new Entry(key, string.IsNullOrEmpty(value) ? defaultValue : value));
        }
        public void Add(AchievementType type, string value) => Add($"$label_{type.ToString().ToLower()}", value);
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
        public void Add(string key, Material? material) => Add(key, material == null ? Keys.None : material.name);
        public void Add(string key, float value, float perLvl, string postfix) => Add(key, $"{value} +{perLvl}{postfix}");
        public void Add(string key, StatusEffect? statusEffect) => Add(key, statusEffect == null ? Keys.None : statusEffect.m_name);

        public void Add(string key, HitData.DamageModifier modifier)
        {
            if (modifier == HitData.DamageModifier.Normal) return;
            Add(key, $"$inventory_{modifier.ToString().ToLower()}");
        }
        public void Add(string key, Enum type) => Add(key, SplitCamelCase(type.ToString()));
        public void Add(string key, CraftingStation? station) => Add(key, station == null ? Keys.None : station.m_name);
        public void Add(string key, GameObject? value) => Add(key, value == null ? Keys.None : value.name);
        public void Add(string key, Turret.AmmoType ammo) => Add(key, ammo.m_ammo);
        public void Add(ItemDrop from, ItemDrop to) => Add(from.m_itemData.m_shared.m_name, to.m_itemData.m_shared.m_name);
        public void Add(string key, bool value) => Add(key, value ? Keys.True : Keys.False);
        public void Add(string key, ItemDrop? item) => Add(key, item == null ? Keys.None : item.m_itemData.m_shared.m_name);
        public void Add(string key, int pre, int post, string separator) => Add(key, $"{pre}<color=orange>{separator}</color>{post}");
        public void Add(string key, Heightmap.Biome biome)
        {
            string[] parts = biome.ToString().Split(',');
            foreach (string part in parts)
            {
                if (!Enum.TryParse(part.Trim(), true, out Heightmap.Biome land)) continue;
                Add(key, $"$biome_{land.ToString().ToLower()}");
            }
        }

        public void Add(string key, float value, string postfix) => Add(key, value.ToString("0") + postfix);
        
        public void Add(string key, float value, Option option = Option.None)
        {
            if (value == 0f && !m_showAll) return;
            switch (option)
            {
                case Option.Minutes:
                    Add(key, value.ToString("0.0") + $"<color=orange>{Keys.Min}</color>");
                    break;
                case Option.Seconds:
                    Add(key, value.ToString("0.0") + "<color=orange>s</color>");
                    break;
                case Option.Percentage:
                    Add(key, (value * 100).ToString("0.0") + "<color=orange>%</color>");
                    break;
                case Option.PerLevel:
                    Add(key, value.ToString("0.0") + $"<color=orange>/{Keys.Lvl}</color>");
                    break;
                case Option.Degree:
                    Add(key, value.ToString("0") + "<color=orange>°</color>");
                    break;
                case Option.Add:
                    Add(key, "<color=orange>+</color>" + value.ToString("0.0"));
                    break;
                case Option.Level:
                    Add(key, value.ToString("0") + $"<color=orange>{Keys.Lvl}</color>");
                    break;
                case Option.XP:
                    Add(key, value.ToString("0") + $"<color=orange>{Keys.XP}</color>");
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
                    Add(key, value + $"<color=orange>{Keys.Min}</color>");
                    break;
                case Option.Seconds:
                    Add(key, value + "<color=orange>s</color>");
                    break;
                case Option.Percentage:
                    Add(key, value * 100 + "<color=orange>%</color>");
                    break;
                case Option.PerLevel:
                    Add(key, value + $"<color=orange>/{Keys.Lvl}</color>");
                    break;
                case Option.Degree:
                    Add(key, value + "<color=orange>°</color>");
                    break;
                case Option.Add:
                    Add(key, "<color=orange>+</color>" + value);
                    break;
                case Option.Level:
                    Add(key, value + $"<color=orange>{Keys.Lvl}</color>");
                    break;
                case Option.XP:
                    Add(key, value + $"<color=orange>{Keys.XP}</color>");
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
        public readonly string title;
        public readonly string value;
        public readonly EntryType type;
        public enum EntryType {Title, Area, KeyValue}

        public Entry(string Title, string Value)
        {
            title = Title;
            value = Value;
            if (Value == "title") type = EntryType.Title;
            else if (Value == "lore") type = EntryType.Area;
            else type = EntryType.KeyValue;
        }
    }

    public static void Build(this List<Entry> entries, AlmanacPanel.InfoView view)
    {
        foreach (Entry? entry in entries)
        {
            switch (entry.type)
            {
                case Entry.EntryType.Title:
                    view.CreateTitle().SetTitle(entry.title);
                    break;
                case Entry.EntryType.Area:
                    view.CreateTextArea().SetText(entry.title);
                    break;
                case Entry.EntryType.KeyValue:
                    view.CreateKeyValue().SetText(entry.title, entry.value);
                    break;
            }
        }
    }
}