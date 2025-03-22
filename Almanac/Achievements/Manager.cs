using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Data;
using Almanac.FileSystem;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static Almanac.Achievements.AchievementTypes;
using Object = UnityEngine.Object;

namespace Almanac.Achievements;

public static class AchievementManager
{
    public const string CollectedRewardKey = "AlmanacCollectedRewards";
    public static readonly Dictionary<string, Achievement> m_achievements = new();
    public static readonly Dictionary<string, Group> m_groupAchievements = new();
    public static readonly Dictionary<string, Achievement> m_achievementStatusEffects = new();
    
    public static void AddOrSet<T>(this Dictionary<T, Group> dict, T key, Achievement value)
    {
        if (dict.TryGetValue(key, out Group group)) group.Add(value);
        else dict[key] = new Group(value);
    }

    public class Group
    {
        private readonly Dictionary<int, Achievement> m_group = new();
        public Group(Achievement achievement)
        {
            m_group[achievement.m_data.Group.Index] = achievement;
            achievement.m_isGrouped = true;
        }

        public bool HasActiveEffect()
        {
            foreach (var achievement in m_group.Values)
            {
                if (achievement.GetRewardType() is not AchievementRewardType.StatusEffect) continue;
                if (achievement.m_statusEffect == null) continue;
                if (EffectMan.IsActive(achievement.m_statusEffect)) return true;
            }

            return false;
        }

        public List<Achievement> GetAchievementsIndexed() => m_group.Values.OrderBy(x => x.GetGroupIndex()).ToList();

        public void Add(Achievement achievement)
        {
            m_group[achievement.m_data.Group.Index] = achievement;
            achievement.m_isGrouped = true;
        }

        public bool IsPreviousAchievementComplete(Achievement achievement)
        {
            var list = GetAchievementsIndexed();
            var indexOf = list.IndexOf(achievement);
            if (indexOf == -1) return achievement.IsComplete();
            if (indexOf == 0) return true;
            return list[indexOf - 1].IsComplete();
        }
    }
    public class Achievement
    {
        public readonly AchievementYML.Data m_data;
        public bool m_isCompleted;
        public readonly StatusEffect? m_statusEffect;
        public bool m_collectedReward;
        private Sprite? s_icon;
        public bool m_isGrouped;
        private string m_customPickableName = "";
        private ItemDrop? m_itemReward;
        private Skills.SkillType m_skillReward = Skills.SkillType.None;
        public readonly Dictionary<Skills.SkillType, float> m_skillBonus = new();

        public bool IsComplete() => m_isCompleted || Player.m_localPlayer.NoCostCheat();
        public Sprite? GetIcon()
        {
            if (s_icon) return s_icon;
            if (SpriteManager.GetSprite(m_data.SpriteName, out Sprite? sprite))
            {
                s_icon = sprite;
                return sprite;
            }

            if (ObjectDB.instance && ObjectDB.instance.GetItemPrefab(m_data.SpriteName) is { } item &&
                item.TryGetComponent(out ItemDrop component) && component.m_itemData.GetIcon() is { } icon)
            {
                s_icon = icon;
                return icon;
            }
            return SpriteManager.AlmanacIcon;
        }
        private Skills.SkillType GetSkillRewardType()
        {
            if (m_skillReward is not Skills.SkillType.None) return m_skillReward;
            if (m_data.Reward.SkillReward.SkillType.IsNullOrWhiteSpace()) return Skills.SkillType.None;
            if (Enum.TryParse(m_data.Reward.SkillReward.SkillType, true, out Skills.SkillType skill))
            {
                m_skillReward = skill;
                return skill;
            }
            skill = (Skills.SkillType)Math.Abs(m_data.Reward.SkillReward.SkillType.GetStableHashCode());
            m_skillReward = Enum.IsDefined(typeof(Skills.SkillType), skill) ? skill : Skills.SkillType.None;
            return m_skillReward;
        }
        public int GetGoal() => m_data.Goal.Threshold;
        public string GetCreatureName() => m_data.Goal.CreatureName;
        public AchievementType GetAchievementType() => m_data.Goal.Type;
        public string GetUniqueName() => m_data.ID;
        public string GetCustomPickableItemName()
        {
            if (!m_customPickableName.IsNullOrWhiteSpace()) return m_customPickableName;
            if (m_data.Goal.PickablePrefab.IsNullOrWhiteSpace()) return "";
            if (ZNetScene.instance.GetPrefab(m_data.Goal.PickablePrefab) is not { } prefab) return "";
            if (!prefab.TryGetComponent(out Pickable component)) return "";
            if (component.m_itemPrefab == null) return "";
            if (!component.m_itemPrefab.TryGetComponent(out ItemDrop item)) return "";
            m_customPickableName = item.m_itemData.m_shared.m_name;
            return m_customPickableName;
        }
        public string GetDescription() => m_data.Description;
        public bool Collect()
        {
            if (!m_isCompleted)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_incomplete");
                return false;
            }
            if (m_isGrouped)
            {
                if (!m_groupAchievements.TryGetValue(GetAchievementGroupName(), out Group group))
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Failed to find achievement group");
                    return false;
                }

                if (!group.IsPreviousAchievementComplete(this))
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_mustcompleteprevious");
                    return false;
                }

                if (group.HasActiveEffect())
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_groupeffectactive");
                    return false;
                }
            }
            switch (m_data.Reward.RewardType)
            {
                case AchievementRewardType.Item:
                    if (m_collectedReward)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_alreadycollectedreward");
                        return false;
                    }

                    if (m_itemReward is not {} item || m_data.Reward.ItemReward.Amount <= 0) return false;
                    if (!Player.m_localPlayer.GetInventory().CanAddItem(item.m_itemData, m_data.Reward.ItemReward.Amount))
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inventoryfull");
                        return false;
                    }

                    var clone = item.m_itemData.Clone();
                    clone.m_dropPrefab = item.gameObject;
                    clone.m_stack = m_data.Reward.ItemReward.Amount;
                    Player.m_localPlayer.GetInventory().AddItem(clone);
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_collecteditem: {clone.m_shared.m_name} x{m_data.Reward.ItemReward.Amount}");
                    m_collectedReward = true;
                    SaveCollectedRewards();
                    break;
                case AchievementRewardType.Skill:
                    if (m_collectedReward)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_alreadycollectedreward");
                        return false;
                    }

                    if (GetSkillRewardType() is Skills.SkillType.None || m_data.Reward.SkillReward.Amount <= 0)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_failedtofindskill");
                        return false;
                    }

                    Player.m_localPlayer.RaiseSkill(GetSkillRewardType(), m_data.Reward.SkillReward.Amount);
                    m_collectedReward = true;
                    SaveCollectedRewards();
                    break;
                case AchievementRewardType.StatusEffect:
                    if (m_statusEffect == null) return false;
                    if (m_statusEffect.m_ttl > 0)
                    {
                        Player.m_localPlayer.m_guardianSE = m_statusEffect;
                        Player.m_localPlayer.m_guardianPower = m_statusEffect.name;
                        Player.m_localPlayer.m_guardianPowerHash = m_statusEffect.NameHash();
                    }
                    else
                    {
                        if (EffectMan.IsActive(m_statusEffect))
                        {
                            RemoveEffect();
                        }
                        else
                        {
                            if (EffectMan.Count() >= AlmanacPlugin._AchievementThreshold.Value)
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_toomanyachievements");
                                return false;
                            }
                            AddEffect();
                        }
                    }
                    break;
                case AchievementRewardType.AlmanacEXP:
                    if (m_data.Reward.AlmanacClassEXP == 0) return false;
                    API.ClassesAPI.AddEXP(m_data.Reward.AlmanacClassEXP);
                    break;
            }
            switch (m_data.Reward.RewardType)
            {
                case AchievementRewardType.StatusEffect:
                    if (m_statusEffect == null) return false;
                    if (EffectMan.IsActive(m_statusEffect))
                    {
                        var component = Object.Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                        component.Setup("$label_removeeffect", () =>
                        {
                            if (Collect()) Object.Destroy(component.gameObject);
                        }, true);
                    }
                    else
                    {
                        var component = Object.Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                        component.Setup("$label_addeffect",
                            () =>
                            {
                                if (Collect()) Object.Destroy(component.gameObject);
                            }, true);
                    }
                    break;
                default:
                    if (m_collectedReward)
                    {
                        var component = Object.Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                        component.Setup("$msg_alreadycollectedreward", () =>
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_alreadycollectedreward");
                        }, true);
                    }
                    else
                    {
                        var component = Object.Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                        component.Setup("$label_collectreward",
                            () =>
                            {
                                if (Collect()) Object.Destroy(component.gameObject);
                            }, true);
                    }
                    break;
            }

            return true;
        }
        private void AddEffect()
        {
            if (m_statusEffect == null) return;
            if (!EffectMan.Add(m_statusEffect)) return;
            AlmanacElement.UpdateBackgrounds();
        }
        private void RemoveEffect()
        {
            if (m_statusEffect == null) return;
            if (!EffectMan.Remove(m_statusEffect)) return;
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_removedeffect: {m_statusEffect.m_name}");
            AlmanacElement.UpdateBackgrounds();
        }
        private Heightmap.Biome GetCreatureBiomeGroup()
        {
            return m_data.Goal.Type switch
            {
                AchievementType.MeadowCreatures => Heightmap.Biome.Meadows,
                AchievementType.BlackForestCreatures => Heightmap.Biome.BlackForest,
                AchievementType.SwampCreatures => Heightmap.Biome.Swamp,
                AchievementType.MountainCreatures => Heightmap.Biome.Mountain,
                AchievementType.PlainsCreatures => Heightmap.Biome.Plains,
                AchievementType.MistLandCreatures => Heightmap.Biome.Mistlands,
                AchievementType.AshLandCreatures => Heightmap.Biome.AshLands,
                AchievementType.DeepNorthCreatures => Heightmap.Biome.DeepNorth,
                AchievementType.OceanCreatures => Heightmap.Biome.Ocean,
                _ => Heightmap.Biome.None
            };
        }
        private void SaveCollectedRewards()
        {
            var deserializer = new DeserializerBuilder().Build();
            var serializer = new SerializerBuilder().Build();
            if (Player.m_localPlayer.m_customData.TryGetValue(CollectedRewardKey, out string data))
            {
                List<string> savedCollectedData = deserializer.Deserialize<List<string>>(data);
                savedCollectedData.Add(m_data.ID);
                Player.m_localPlayer.m_customData[CollectedRewardKey] = serializer.Serialize(savedCollectedData);
            }
            else
            {
                Player.m_localPlayer.m_customData[CollectedRewardKey] = serializer.Serialize(new List<string>() { m_data.ID });
            }
        }
        private string GetProgress()
        {
            string ItemFormat(List<ItemDrop> list)
            {
                return $"{list.Count(x => Player.m_localPlayer.IsKnownMaterial(x.m_itemData.m_shared.m_name))} / {list.Count}";
            }

            string StatFormat(PlayerStatType type)
            {
                return $"{PlayerStats.GetPlayerStat(type)} / {GetGoal()}";
            }

            string BiomeCreatureFormat(Heightmap.Biome biome)
            {
                int count = 0;
                var list = CreatureLists.GetBiomeCreatures(biome);
                foreach (var creature in list)
                {
                    var amount = PlayerStats.m_records.m_kills.GetValueOrDefault(creature.m_prefabName, 0);
                    if (amount > 0) ++count;
                }
                return $"{count} / {list.Count}";
            }

            string CustomCreatureGroupFormat()
            {
                if (!CreatureLists.CustomCreatureGroups.TryGetValue(m_data.Goal.CreatureGroup,
                        out List<Creatures.Data> list)) return "";
                int progress = 0;
                foreach (var creature in list)
                {
                    if (PlayerStats.m_records.m_kills.GetValueOrDefault(creature.m_name, 0) > 0) ++progress;
                }

                return $"{progress} / {list.Count}";
            }
            return GetAchievementType() switch
            {
                AchievementType.Deaths => StatFormat(PlayerStatType.Deaths),
                AchievementType.Fish => ItemFormat(Items.GetFishes().Select(x => x.m_item).ToList()),
                AchievementType.Materials => ItemFormat(Items.GetMaterials().Select(x=>x.m_item).ToList()),
                AchievementType.Consumables => ItemFormat(Items.GetConsumables().Select(x=>x.m_item).ToList()),
                AchievementType.Weapons => ItemFormat(Items.GetWeapons().Select(x=>x.m_item).ToList()),
                AchievementType.Swords => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Swords).Select(x=>x.m_item).ToList()),
                AchievementType.Axes => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Axes).Select(x=>x.m_item).ToList()),
                AchievementType.PoleArms => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Polearms).Select(x=>x.m_item).ToList()),
                AchievementType.Spears => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Spears).Select(x=>x.m_item).ToList()),
                AchievementType.Maces => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Clubs).Select(x=>x.m_item).ToList()),
                AchievementType.Knives => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Knives).Select(x=>x.m_item).ToList()),
                AchievementType.Shields => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Blocking).Select(x=>x.m_item).ToList()),
                AchievementType.Staves => ItemFormat(Items.GetStaves().Select(x=>x.m_item).ToList()),
                AchievementType.Arrows => ItemFormat(Items.GetAmmunition().Select(x=>x.m_item).ToList()),
                AchievementType.Bows => ItemFormat(Items.GetWeaponBySkill(Skills.SkillType.Bows).Select(x=>x.m_item).ToList()),
                AchievementType.Valuables => ItemFormat(Items.GetValuables().Select(x=>x.m_item).ToList()),
                AchievementType.Potions => ItemFormat(Items.GetPotions().Select(x=>x.m_item).ToList()),
                AchievementType.Trophies => ItemFormat(Items.GetTrophies().Select(x=>x.m_item).ToList()),
                AchievementType.EnemyKills => StatFormat(PlayerStatType.EnemyKills),
                AchievementType.MeadowCreatures => BiomeCreatureFormat(Heightmap.Biome.Meadows),
                AchievementType.BlackForestCreatures => BiomeCreatureFormat(Heightmap.Biome.BlackForest),
                AchievementType.SwampCreatures => BiomeCreatureFormat(Heightmap.Biome.Swamp),
                AchievementType.MountainCreatures => BiomeCreatureFormat(Heightmap.Biome.Mountain),
                AchievementType.PlainsCreatures => BiomeCreatureFormat(Heightmap.Biome.Plains),
                AchievementType.MistLandCreatures => BiomeCreatureFormat(Heightmap.Biome.Mistlands),
                AchievementType.AshLandCreatures => BiomeCreatureFormat(Heightmap.Biome.AshLands),
                AchievementType.DeepNorthCreatures => BiomeCreatureFormat(Heightmap.Biome.DeepNorth),
                AchievementType.OceanCreatures => BiomeCreatureFormat(Heightmap.Biome.Ocean),
                AchievementType.TreesChopped => StatFormat(PlayerStatType.Tree),
                AchievementType.DeathByTree => StatFormat(PlayerStatType.DeathByTree),
                AchievementType.DeathByEdgeOfWorld => StatFormat(PlayerStatType.DeathByEdgeOfWorld),
                AchievementType.TimeInBase => StatFormat(PlayerStatType.TimeInBase),
                AchievementType.TimeOutOfBase => StatFormat(PlayerStatType.TimeOutOfBase),
                AchievementType.ArrowsShot => StatFormat(PlayerStatType.ArrowsShot),
                AchievementType.TotalJumps => StatFormat(PlayerStatType.Jumps),
                AchievementType.TotalBuilds => StatFormat(PlayerStatType.Builds),
                AchievementType.EnemyHits => StatFormat(PlayerStatType.EnemyHits),
                AchievementType.PlayerKills => StatFormat(PlayerStatType.PlayerKills),
                AchievementType.HitsTaken => StatFormat(PlayerStatType.HitsTakenEnemies),
                AchievementType.ItemsPicked => StatFormat(PlayerStatType.ItemsPickedUp),
                AchievementType.DistanceWalked => StatFormat(PlayerStatType.DistanceWalk),
                AchievementType.DistanceRan => StatFormat(PlayerStatType.DistanceRun),
                AchievementType.DistanceSailed => StatFormat(PlayerStatType.DistanceSail),
                AchievementType.DistanceInAir => StatFormat(PlayerStatType.DistanceAir),
                AchievementType.MineHits => StatFormat(PlayerStatType.MineHits),
                AchievementType.TotalMined => StatFormat(PlayerStatType.Mines),
                AchievementType.CreatureTamed => StatFormat(PlayerStatType.CreatureTamed),
                AchievementType.FoodEaten => StatFormat(PlayerStatType.FoodEaten),
                AchievementType.SkeletonSummoned => StatFormat(PlayerStatType.SkeletonSummons),
                AchievementType.DeathByDrowning => StatFormat(PlayerStatType.DeathByDrowning),
                AchievementType.DeathByBurning => StatFormat(PlayerStatType.DeathByBurning),
                AchievementType.DeathByFreezing => StatFormat(PlayerStatType.DeathByFreezing),
                AchievementType.DeathByPoisoned => StatFormat(PlayerStatType.DeathByPoisoned),
                AchievementType.DeathBySmoke => StatFormat(PlayerStatType.DeathBySmoke),
                AchievementType.DeathByStalagtite => StatFormat(PlayerStatType.DeathByStalagtite),
                AchievementType.BeesHarvested => StatFormat(PlayerStatType.BeesHarvested),
                AchievementType.SapHarvested => StatFormat(PlayerStatType.SapHarvested),
                AchievementType.TrapsArmed => StatFormat(PlayerStatType.TrapArmed),
                AchievementType.CustomKills => $"{PlayerStats.m_records.m_kills.GetValueOrDefault(GetCreatureName(), 0)} / {GetGoal()}",
                AchievementType.RuneStones => $"{PlayerStats.GetKnownTextCount()} / {GetGoal()}",
                AchievementType.Recipes => $"{PlayerStats.GetKnownRecipeCount()} / {GetGoal()}",
                AchievementType.CustomCreatureGroups => CustomCreatureGroupFormat(),
                _ => ""
            };
        }
        public List<Entries.Entry> GetEntries()
        {
            Entries.EntryBuilder builder = new();
            if (m_isGrouped && m_groupAchievements.TryGetValue(GetAchievementGroupName(), out Group group))
            {
                builder.Add("$title_groupachievement");
                foreach (var achievement in group.GetAchievementsIndexed())
                {
                    builder.Add($"#{achievement.GetGroupIndex()} - {achievement.GetDisplayName()}", achievement.IsComplete());
                }
            }
            builder.Add(m_data.Lore, "lore");
            builder.Add("$title_goal");
            if (m_data.Goal.Type is AchievementType.CustomCreatureGroups)
            {
                if (CreatureLists.CustomCreatureGroups.TryGetValue(m_data.Goal.CreatureGroup, out List<Creatures.Data> list))
                {
                    int progress = 0;
                    foreach (var creature in list)
                    {
                        var kills = PlayerStats.m_records.m_kills.GetValueOrDefault(creature.m_prefabName, 0);
                        if (kills > 0)
                        {
                            builder.Add(creature.m_name, kills, "/1");
                            ++progress;
                        }
                        else
                        {
                            builder.Add(creature.m_name, "0/1");
                        }
                    }
                    builder.Add("$label_total", progress, list.Count, "/");
                }
            }
            else if (m_data.Goal.Type is AchievementType.CustomKills)
            {
                if (Creatures.m_creatures.TryGetValue(m_data.Goal.CreatureName.ToLower(), out Creatures.Data creature))
                {
                    builder.Add(creature.m_name, GetProgress());
                }
            }
            else if (m_data.Goal.Type is AchievementType.MeadowCreatures or AchievementType.BlackForestCreatures
                     or AchievementType.SwampCreatures or AchievementType.MountainCreatures
                     or AchievementType.PlainsCreatures or AchievementType.MistLandCreatures
                     or AchievementType.AshLandCreatures or AchievementType.DeepNorthCreatures
                     or AchievementType.OceanCreatures)
            {
                if (CreatureLists.BiomeCreatureMap.TryGetValue(GetCreatureBiomeGroup(), out List<Creatures.Data> list))
                {
                    int progress = 0;
                    foreach (var creature in list)
                    {
                        int kills = PlayerStats.m_records.m_kills.GetValueOrDefault(creature.m_prefabName, 0);
                        if (kills > 0)
                        {
                            builder.Add(creature.m_name, kills, "/1");
                            ++progress;
                        }
                        else
                        {
                            builder.Add(creature.m_name, "0/1");
                        }
                    }
                    builder.Add("$label_total", progress, list.Count, "/");
                }
            }
            else if (m_data.Goal.Type is AchievementType.CustomPickable)
            {
                if (!m_data.Goal.PickablePrefab.IsNullOrWhiteSpace())
                {
                    int count = PlayerStats.m_records.m_picked.GetValueOrDefault(m_data.Goal.PickablePrefab, 0);
                    builder.Add(GetCustomPickableItemName(), count, GetGoal(), "/");
                }
            }
            else
            {
                builder.Add(GetAchievementType(), GetProgress());
            }
            switch (m_data.Reward.RewardType)
            {
                case AchievementRewardType.Item:
                    if (m_itemReward is not { } item) break;
                    builder.Add("$title_itemreward");
                    builder.Add(item.m_itemData.m_shared.m_name, m_data.Reward.ItemReward.Amount);
                    break;
                case AchievementRewardType.Skill:
                    var type = GetSkillRewardType();
                    if (type is Skills.SkillType.None) break;
                    builder.Add("$title_skillreward");
                    builder.Add(type, m_data.Reward.SkillReward.Amount);
                    break;
                case AchievementRewardType.StatusEffect:
                    builder.Add("$title_effectreward");
                    builder.Add("$label_duration", m_data.Reward.StatusEffect.Duration, Entries.EntryBuilder.Option.Seconds);
                    if (m_data.Reward.StatusEffect.Resistances.Any(x => x.m_modifier != HitData.DamageModifier.Normal))
                    {
                        builder.Add("$title_resistances");
                        foreach (var dmg in m_data.Reward.StatusEffect.Resistances) builder.Add(dmg);
                    }
                    builder.Add("$title_modifiers");
                    foreach (var mod in m_data.Reward.StatusEffect.Modifiers)
                    {
                        if (mod.Key is EffectMan.Modifier.MaxCarryWeight or EffectMan.Modifier.Health or EffectMan.Modifier.Stamina or EffectMan.Modifier.Eitr or EffectMan.Modifier.Armor)
                        {
                            if (mod.Value == 0f) continue;
                            builder.Add(mod.Key, mod.Value, Entries.EntryBuilder.Option.Add);
                        }
                        else if (mod.Key is EffectMan.Modifier.DamageReduction)
                        {
                            if (mod.Value == 0f) continue;
                            builder.Add(mod.Key, Mathf.Clamp01(1f - mod.Value), Entries.EntryBuilder.Option.Percentage);
                        }
                        else
                        {
                            if (Math.Abs(mod.Value - 1f) < 0.01f) continue;
                            builder.Add(mod.Key, mod.Value - 1f, Entries.EntryBuilder.Option.Percentage);
                        }
                    }
                    break;
                case AchievementRewardType.AlmanacEXP:
                    builder.Add("$title_almanac_class_exp");
                    builder.Add("$label_amount", m_data.Reward.AlmanacClassEXP);
                    break;
            }

            return builder.ToList();
        }
        public AchievementRewardType GetRewardType() => m_data.Reward.RewardType;
        public string GetDisplayName() => m_data.Name;
        public string GetAchievementGroupName() => m_data.Group.GroupID;
        public int GetGroupIndex() => m_data.Group.Index;
        public void Check()
        {
            if (m_isGrouped)
            {
                if (!m_groupAchievements.TryGetValue(GetAchievementGroupName(), out Group group))
                {
                    m_isCompleted = false;
                    return;
                }

                if (!group.IsPreviousAchievementComplete(this))
                {
                    m_isCompleted = false;
                    return;
                }
            }
            switch (GetAchievementType())
            {
                case AchievementType.Deaths:
                    CheckCompletion(PlayerStatType.Deaths);
                    break;
                case AchievementType.Fish:
                    CheckCompletion(Items.GetFishes().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Materials:
                    CheckCompletion(Items.GetMaterials().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Consumables:
                    CheckCompletion(Items.GetConsumables().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Weapons:
                    CheckCompletion(Items.GetWeapons().Where(x => !Items.IsTool(x.m_item)).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Swords:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Swords).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Axes:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Axes).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.PoleArms:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Polearms).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Spears:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Spears).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Maces:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Clubs).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Knives:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Knives).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Shields:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Blocking).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Staves:
                    CheckCompletion(Items.GetStaves().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Arrows:
                    CheckCompletion(Items.GetAmmunition().Where(x => !Items.IsBait(x.m_item)).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Bows:
                    CheckCompletion(Items.GetWeaponBySkill(Skills.SkillType.Bows).Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Valuables:
                    CheckCompletion(Items.GetValuables().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Potions:
                    CheckCompletion(Items.GetPotions().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.Trophies:
                    CheckCompletion(Items.GetTrophies().Select(x=>x.m_item).ToList());
                    break;
                case AchievementType.EnemyKills:
                    CheckCompletion(PlayerStatType.EnemyKills);
                    break;
                case AchievementType.DeathByFall:
                    CheckCompletion(PlayerStatType.DeathByFall);
                    break;
                case AchievementType.TreesChopped:
                    CheckCompletion(PlayerStatType.Tree);
                    break;
                case AchievementType.DeathByTree:
                    CheckCompletion(PlayerStatType.DeathByTree);
                    break;
                case AchievementType.DeathByEdgeOfWorld:
                    CheckCompletion(PlayerStatType.DeathByEdgeOfWorld);
                    break;
                case AchievementType.TimeInBase:
                    CheckCompletion(PlayerStatType.TimeInBase);
                    break;
                case AchievementType.TimeOutOfBase:
                    CheckCompletion(PlayerStatType.TimeOutOfBase);
                    break;
                case AchievementType.ArrowsShot:
                    CheckCompletion(PlayerStatType.ArrowsShot);
                    break;
                case AchievementType.TotalJumps:
                    CheckCompletion(PlayerStatType.Jumps);
                    break;
                case AchievementType.TotalBuilds:
                    CheckCompletion(PlayerStatType.Builds);
                    break;
                case AchievementType.EnemyHits:
                    CheckCompletion(PlayerStatType.EnemyHits);
                    break;
                case AchievementType.PlayerKills:
                    CheckCompletion(PlayerStatType.PlayerKills);
                    break;
                case AchievementType.HitsTaken:
                    CheckCompletion(PlayerStatType.HitsTakenEnemies);
                    break;
                case AchievementType.ItemsPicked:
                    CheckCompletion(PlayerStatType.ItemsPickedUp);
                    break;
                case AchievementType.DistanceWalked:
                    CheckCompletion(PlayerStatType.DistanceWalk);
                    break;
                case AchievementType.DistanceInAir:
                    CheckCompletion(PlayerStatType.DistanceAir);
                    break;
                case AchievementType.DistanceRan:
                    CheckCompletion(PlayerStatType.DistanceRun);
                    break;
                case AchievementType.DistanceSailed:
                    CheckCompletion(PlayerStatType.DistanceSail);
                    break;
                case AchievementType.MineHits:
                    CheckCompletion(PlayerStatType.MineHits);
                    break;
                case AchievementType.TotalMined:
                    CheckCompletion(PlayerStatType.Mines);
                    break;
                case AchievementType.CreatureTamed:
                    CheckCompletion(PlayerStatType.CreatureTamed);
                    break;
                case AchievementType.FoodEaten:
                    CheckCompletion(PlayerStatType.FoodEaten);
                    break;
                case AchievementType.SkeletonSummoned:
                    CheckCompletion(PlayerStatType.SkeletonSummons);
                    break;
                case AchievementType.DeathByDrowning:
                    CheckCompletion(PlayerStatType.DeathByDrowning);
                    break;
                case AchievementType.DeathByBurning:
                    CheckCompletion(PlayerStatType.DeathByBurning);
                    break;
                case AchievementType.DeathByFreezing:
                    CheckCompletion(PlayerStatType.DeathByFreezing);
                    break;
                case AchievementType.DeathByPoisoned:
                    CheckCompletion(PlayerStatType.DeathByPoisoned);
                    break;
                case AchievementType.DeathBySmoke:
                    CheckCompletion(PlayerStatType.DeathBySmoke);
                    break;
                case AchievementType.DeathByStalagtite:
                    CheckCompletion(PlayerStatType.DeathByStalagtite);
                    break;
                case AchievementType.BeesHarvested:
                    CheckCompletion(PlayerStatType.BeesHarvested);
                    break;
                case AchievementType.SapHarvested:
                    CheckCompletion(PlayerStatType.SapHarvested);
                    break;
                case AchievementType.TrapsArmed:
                    CheckCompletion(PlayerStatType.TrapArmed);
                    break;
                case AchievementType.RuneStones:
                    int loreCount = PlayerStats.GetKnownTextCount();
                    m_isCompleted = loreCount >= GetGoal();
                    break;
                case AchievementType.Recipes:
                    int recipeCount = PlayerStats.GetKnownRecipeCount();
                    m_isCompleted = recipeCount >= GetGoal();
                    break;
                case AchievementType.CustomKills:
                    int kills = PlayerStats.m_records.m_kills.GetValueOrDefault(GetCreatureName(), 0);
                    m_isCompleted = kills >= GetGoal();
                    break;
                case AchievementType.MeadowCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Meadows));
                    break;
                case AchievementType.BlackForestCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.BlackForest));
                    break;
                case AchievementType.SwampCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Swamp));
                    break;
                case AchievementType.MountainCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Mountain));
                    break;
                case AchievementType.PlainsCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Plains));
                    break;
                case AchievementType.MistLandCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Mistlands));
                    break;
                case AchievementType.DeepNorthCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.DeepNorth));
                    break;
                case AchievementType.AshLandCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.AshLands));
                    break;
                case AchievementType.OceanCreatures:
                    CheckCompletion(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Ocean));
                    break;
                case AchievementType.CustomCreatureGroups:
                    CheckCompletion(CreatureLists.GetCustomCreatureGroup(m_data.Goal.CreatureGroup));
                    break;
                case AchievementType.CustomPickable:
                    if (m_data.Goal.PickablePrefab.IsNullOrWhiteSpace()) return;
                    int count = PlayerStats.m_records.m_picked.GetValueOrDefault(m_data.Goal.PickablePrefab, 0);
                    m_isCompleted = count > GetGoal();
                    break;
                default:
                    m_isCompleted = false;
                    break;
            }
        }
        
        private void CheckCompletion(List<Creatures.Data> list)
        {
            int count = 0;
            foreach (var creature in list)
            {
                int kills = PlayerStats.m_records.m_kills.GetValueOrDefault(creature.m_prefabName, 0);
                if (kills > 0) ++count;
            }
            m_isCompleted = count >= list.Count;
        }
        private void CheckCompletion(PlayerStatType type)
        {
            m_isCompleted = PlayerStats.GetPlayerStat(type) >= GetGoal();
        }
        private void CheckCompletion(List<ItemDrop> list)
        {
            m_isCompleted =
                list.FindAll(item => Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name) && !item.m_itemData.m_shared.m_dlc.IsNullOrWhiteSpace()).Count >=
                list.Count(x => x.m_itemData.m_shared.m_dlc.IsNullOrWhiteSpace());
        }

        public Achievement(AchievementYML.Data data)
        {
            m_data = data;
            if (!Validate()) return;
            if (!m_data.Group.GroupID.IsNullOrWhiteSpace())
            {
                m_groupAchievements.AddOrSet(m_data.Group.GroupID, this);
            }

            if (m_data.Reward.RewardType is AchievementRewardType.StatusEffect)
            {
                if (EffectMan.EffectData.Init(this) is { } statusEffect)
                {
                    m_achievementStatusEffects[statusEffect.name] = this;
                    m_statusEffect = statusEffect;
                }
            }
            m_achievements[m_data.ID] = this;
        }

        private bool Validate()
        {
            if (m_data.ID.IsNullOrWhiteSpace())
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Achievement unique name is null or white space, skipping...");
                return false;
            }
            if (m_achievements.Keys.Contains(m_data.ID))
            {
                AlmanacPlugin.AlmanacLogger.LogWarning($"Duplicate achievement unique key: {m_data.ID}");
                return false;
            }
            
            if (!m_data.Goal.CreatureName.IsNullOrWhiteSpace())
            {
                bool updateFile = false;
                if (Creatures.m_defeatKeyCreatures.TryGetValue(m_data.Goal.CreatureName, out Creatures.Data creatureData))
                {
                    m_data.Goal.CreatureName = creatureData.m_prefabName;
                    updateFile = true;
                }
                else
                {
                    if (m_data.Goal.CreatureName.StartsWith("defeated_"))
                    {
                        var name = m_data.Goal.CreatureName.Replace("defeated_", string.Empty).ToLower();
                        if (Creatures.m_creatures.TryGetValue(name, out creatureData))
                        {
                            m_data.Goal.CreatureName = creatureData.m_prefabName;
                            updateFile = true;
                        }
                    }
                }

                if (updateFile)
                {
                    var filePath = AlmanacPaths.AchievementFolderPath + Path.DirectorySeparatorChar + m_data.ID + ".yml";
                    var serializer = new SerializerBuilder().Build();
                    File.WriteAllText(filePath, serializer.Serialize(m_data));
                }
            }

            if (!m_data.Goal.CreatureGroup.IsNullOrWhiteSpace())
            {
                if (!CreatureLists.CustomCreatureGroups.Keys.Contains(m_data.Goal.CreatureGroup))
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.ID}]: Failed to find custom creature group: {m_data.Goal.CreatureGroup}");
                    return false;
                }
            }

            if (!m_data.Goal.PickablePrefab.IsNullOrWhiteSpace())
            {
                if (ZNetScene.instance.GetPrefab(m_data.Goal.PickablePrefab) is not { } prefab)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.ID}]: Failed to find custom pickable prefab: {m_data.Goal.PickablePrefab}");
                    return false;
                }

                if (!prefab.GetComponent<Pickable>())
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Custom pickable prefab does not have <Pickable> component");
                    return false;
                }
            }

            if (!m_data.Reward.ItemReward.PrefabName.IsNullOrWhiteSpace())
            {
                if (ObjectDB.m_instance.GetItemPrefab(m_data.Reward.ItemReward.PrefabName) is not { } item)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.ID}]: Failed to find item reward: {m_data.Reward.ItemReward.PrefabName}");
                    return false;
                }

                if (!item.TryGetComponent(out ItemDrop itemDrop))
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.ID}]: Item reward does not have <ItemDrop> component");
                    return false;
                }

                m_itemReward = itemDrop;
            }

            if (!m_data.Reward.SkillReward.SkillType.IsNullOrWhiteSpace())
            {
                if (GetSkillRewardType() is Skills.SkillType.None)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.ID}]: Failed to find skill type: {m_data.Reward.SkillReward.SkillType}");
                    return false;
                }
            }

            foreach (var kvp in m_data.Reward.StatusEffect.Skills)
            {
                if (kvp.Key.IsNullOrWhiteSpace()) continue;
                if (Enum.TryParse(kvp.Key, true, out Skills.SkillType skill))
                {
                    m_skillBonus[skill] = kvp.Value;
                }
                else
                {
                    skill = (Skills.SkillType)Math.Abs(kvp.Key.GetStableHashCode());
                    if (Enum.IsDefined(typeof(Skills.SkillType), skill))
                    {
                        m_skillBonus[skill] = kvp.Value;
                    }
                    else
                    {
                        AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.ID}]: Failed to find skill type for status effect: {kvp.Key}");
                    }
                }
            }
            return true;
        }
    }
    
    public static void OnAchievementConfigChanged(object sender, EventArgs e)
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Achievement configs changed, reloading achievements");
        EffectMan.Clear();
        if (ServerSyncedData.InitiatedServerAchievements)
        {
            ServerSyncedData.OnServerAchievementChanged();
        }
        else
        {
            InitAchievements();
        }
    }
    public static void InitAchievements()
    {
        if (!ObjectDB.instance) return;
        EffectMan.Clear();
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is EffectMan.AchievementEffect);
        Setup();
        if (Player.m_localPlayer) CheckCollectedRewards(Player.m_localPlayer);
    }
    public static void CheckCollectedRewards(Player player)
    {
        if (!player.m_customData.TryGetValue(CollectedRewardKey, out string data)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<string> SavedCollectedData = deserializer.Deserialize<List<string>>(data);
        foreach (string name in SavedCollectedData)
        {
            if (!m_achievements.TryGetValue(name, out Achievement achievement)) continue;
            achievement.m_collectedReward = true;
        }
    }

    public static void Read()
    {
        AchievementYML.m_data.Clear();
        AchievementYML.Init();
    }

    public static void Setup()
    {
        m_achievements.Clear();
        m_groupAchievements.Clear();
        foreach (var data in AchievementYML.m_data)
        {
            var _ = new Achievement(data);
        }
    }
    public static List<Achievement> GetAchievements() => m_achievements.Values.ToList();
    
}