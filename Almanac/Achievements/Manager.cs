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
    public static List<AchievementYML.AchievementData> AchievementData = new();

    public class Group
    {
        private readonly Dictionary<int, Achievement> m_group = new();
        public Group(Achievement achievement)
        {
            m_group[achievement.m_data.achievement_index] = achievement;
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
            m_group[achievement.m_data.achievement_index] = achievement;
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
        public readonly AchievementYML.AchievementData m_data;
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
            if (SpriteManager.GetSprite(m_data.sprite_name, out Sprite? sprite))
            {
                s_icon = sprite;
                return sprite;
            }

            if (ObjectDB.instance && ObjectDB.instance.GetItemPrefab(m_data.sprite_name) is { } item &&
                item.TryGetComponent(out ItemDrop component) && component.m_itemData.GetIcon() is { } icon)
            {
                s_icon = icon;
                return icon;
            }
            return SpriteManager.AlmanacIcon;
        }
        private Skills.SkillType GetSkillType()
        {
            if (m_skillReward is not Skills.SkillType.None) return m_skillReward;
            if (m_data.skill.IsNullOrWhiteSpace()) return Skills.SkillType.None;
            if (Enum.TryParse(m_data.skill, true, out Skills.SkillType skill))
            {
                m_skillReward = skill;
                return skill;
            }
            skill = (Skills.SkillType)Math.Abs(m_data.skill.GetStableHashCode());
            m_skillReward = Enum.IsDefined(typeof(Skills.SkillType), skill) ? skill : Skills.SkillType.None;
            return m_skillReward;
        }
        public int GetGoal() => m_data.goal;
        public string GetDefeatKey() => m_data.defeat_key;
        public AchievementType GetAchievementType() => m_data.achievement_type;
        public string GetUniqueName() => m_data.unique_name;
        public string GetCustomPickableItemName()
        {
            if (!m_customPickableName.IsNullOrWhiteSpace()) return m_customPickableName;
            if (m_data.custom_pickable_name.IsNullOrWhiteSpace()) return "";
            if (ZNetScene.instance.GetPrefab(m_data.custom_pickable_name) is not { } prefab) return "";
            if (!prefab.TryGetComponent(out Pickable component)) return "";
            if (component.m_itemPrefab == null) return "";
            if (!component.m_itemPrefab.TryGetComponent(out ItemDrop item)) return "";
            m_customPickableName = item.m_itemData.m_shared.m_name;
            return m_customPickableName;
        }
        public string GetDescription() => m_data.description;
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
            switch (m_data.reward_type)
            {
                case AchievementRewardType.Item:
                    if (m_collectedReward)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_alreadycollectedreward");
                        return false;
                    }

                    if (m_itemReward is not {} item || m_data.item_amount <= 0) return false;
                    if (!Player.m_localPlayer.GetInventory().CanAddItem(item.m_itemData, m_data.item_amount))
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inventoryfull");
                        return false;
                    }

                    var clone = item.m_itemData.Clone();
                    clone.m_dropPrefab = item.gameObject;
                    clone.m_stack = m_data.item_amount;
                    Player.m_localPlayer.GetInventory().AddItem(clone);
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_collecteditem: {clone.m_shared.m_name} x{m_data.item_amount}");
                    m_collectedReward = true;
                    SaveCollectedRewards();
                    break;
                case AchievementRewardType.Skill:
                    if (m_collectedReward)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_alreadycollectedreward");
                        return false;
                    }

                    if (GetSkillType() is Skills.SkillType.None || m_data.skill_amount <= 0)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_failedtofindskill");
                        return false;
                    }

                    Player.m_localPlayer.RaiseSkill(GetSkillType(), m_data.skill_amount);
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
                    if (m_data.class_experience == 0) return false;
                    API.ClassesAPI.AddEXP(m_data.class_experience);
                    break;
            }
            switch (m_data.reward_type)
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
            return m_data.achievement_type switch
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
                savedCollectedData.Add(m_data.unique_name);
                Player.m_localPlayer.m_customData[CollectedRewardKey] = serializer.Serialize(savedCollectedData);
            }
            else
            {
                Player.m_localPlayer.m_customData[CollectedRewardKey] = serializer.Serialize(new List<string>() { m_data.unique_name });
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
                    if (ZoneSystem.instance.GetGlobalKey(creature.m_defeatKey) ||
                        Player.m_localPlayer.HaveUniqueKey(creature.m_defeatKey)) ++count;
                }
                return $"{count} / {list.Count}";
            }

            string CustomCreatureGroupFormat()
            {
                if (!CreatureLists.CustomCreatureGroups.TryGetValue(m_data.custom_group_key,
                        out List<Creatures.Data> list)) return "";
                int progress = 0;
                foreach (var creature in list)
                {
                    if (PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(creature.m_defeatKey,
                            out KillDeaths kd))
                    {
                        if (kd.kills > 0) ++progress;
                    }
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
                AchievementType.CustomKills => $"{(PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(GetDefeatKey(), out KillDeaths data) ? data.kills : 0)} / {GetGoal()}",
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
                builder.Add("$title_groupAchievement");
                foreach (var achievement in group.GetAchievementsIndexed())
                {
                    builder.Add($"#{achievement.GetGroupIndex()} - {achievement.GetDisplayName()}", achievement.IsComplete());
                }
            }
            builder.Add(m_data.lore, "lore");
            builder.Add("$title_goal");
            if (m_data.achievement_type is AchievementType.CustomCreatureGroups)
            {
                if (CreatureLists.CustomCreatureGroups.TryGetValue(m_data.custom_group_key, out List<Creatures.Data> list))
                {
                    int progress = 0;
                    foreach (var creature in list)
                    {
                        if (PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(creature.m_defeatKey, out KillDeaths kd) && kd.kills > 0)
                        {
                            builder.Add(creature.m_name, kd.kills, "/1");
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
            else if (m_data.achievement_type is AchievementType.CustomKills)
            {
                if (Creatures.m_defeatKeyCreatures.TryGetValue(m_data.defeat_key, out Creatures.Data creature))
                {
                    builder.Add(creature.m_name, GetProgress());
                }
            }
            else if (m_data.achievement_type is AchievementType.MeadowCreatures or AchievementType.BlackForestCreatures
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
                        if (PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(creature.m_defeatKey,
                                out KillDeaths kd) && kd.kills > 0)
                        {
                            ++progress;
                            builder.Add(creature.m_name, kd.kills, "/1");
                        }
                        else
                        {
                            builder.Add(creature.m_name, "0/1");
                        }
                    }
                    builder.Add("$label_total", progress, list.Count, "/");
                }
            }
            else if (m_data.achievement_type is AchievementType.CustomPickable)
            {
                if (!m_data.custom_pickable_name.IsNullOrWhiteSpace())
                {
                    if (PlayerStats.LocalPlayerData.Player_Pickable_Data.TryGetValue(m_data.custom_pickable_name, out int pickedAmount))
                    {
                        builder.Add(GetCustomPickableItemName(), pickedAmount, GetGoal(), "/");
                    }
                }
            }
            else
            {
                builder.Add(GetAchievementType(), GetProgress());
            }
            switch (m_data.reward_type)
            {
                case AchievementRewardType.Item:
                    if (m_itemReward is not { } item) break;
                    builder.Add("$title_itemreward");
                    builder.Add(item.m_itemData.m_shared.m_name, m_data.item_amount);
                    break;
                case AchievementRewardType.Skill:
                    var type = GetSkillType();
                    if (type is Skills.SkillType.None) break;
                    builder.Add("$title_skillreward");
                    builder.Add(type, m_data.skill_amount);
                    break;
                case AchievementRewardType.StatusEffect:
                    builder.Add("$title_effectreward");
                    builder.Add("$label_duration", m_data.duration, Entries.EntryBuilder.Option.Seconds);
                    builder.Add("$title_resistances");
                    foreach (var dmg in m_data.damage_modifiers) builder.Add(dmg);
                    builder.Add("$title_modifiers");
                    foreach (var mod in m_data.modifiers)
                    {
                        if (mod.Key is EffectMan.Modifier.MaxCarryWeight)
                        {
                            if (mod.Value == 0) continue;
                            builder.Add(mod.Key, mod.Value);
                        }
                        else
                        {
                            builder.Add(mod.Key, mod.Value - 1f, Entries.EntryBuilder.Option.Percentage);
                        }
                    }
                    break;
                case AchievementRewardType.AlmanacEXP:
                    builder.Add("$title_almanac_class_exp");
                    builder.Add("$label_amount", m_data.class_experience);
                    break;
            }

            return builder.ToList();
        }
        public AchievementRewardType GetRewardType() => m_data.reward_type;
        public string GetDisplayName() => m_data.display_name;
        public string GetAchievementGroupName() => m_data.achievement_group;
        public int GetGroupIndex() => m_data.achievement_index;
        
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
                    if (PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(GetDefeatKey(), out KillDeaths value))
                    {
                        int kills = value.kills;
                        m_isCompleted = kills >= GetGoal();
                    }
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
                    CheckCompletion(CreatureLists.GetCustomCreatureGroup(m_data.custom_group_key));
                    break;
                case AchievementType.CustomPickable:
                    if (m_data.custom_pickable_name.IsNullOrWhiteSpace()) return;
                    if (PlayerStats.LocalPlayerData.Player_Pickable_Data.TryGetValue(m_data.custom_pickable_name, out int pickedAmount))
                    {
                        m_isCompleted = pickedAmount > GetGoal();
                    }
                    break;
                default:
                    m_isCompleted = false;
                    break;
            }
        }
        
        private void CheckCompletion(List<Creatures.Data> list)
        {
            if (!ZoneSystem.instance) return;
            List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
            int count = list.Count(critter => globalKeys.Contains(critter.m_defeatKey) || ZoneSystem.instance.GetGlobalKey(critter.m_defeatKey));
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
                list.Count(x => !x.m_itemData.m_shared.m_dlc.IsNullOrWhiteSpace());
        }
        
        public Achievement(AchievementYML.AchievementData data)
        {
            m_data = data;
            if (!Validate()) return;
            m_achievements[data.unique_name] = this;
            if (!GetAchievementGroupName().IsNullOrWhiteSpace())
            {
                if (m_groupAchievements.TryGetValue(GetAchievementGroupName(), out Group group)) group.Add(this);
                else m_groupAchievements[GetAchievementGroupName()] = new Group(this);
            }
            if (m_data.reward_type is not AchievementRewardType.StatusEffect) return;
            if (EffectMan.EffectData.Init(this) is { } statusEffect)
            {
                m_achievementStatusEffects[statusEffect.name] = this;
                m_statusEffect = statusEffect;
            }
        }

        private bool Validate()
        {
            if (m_data.unique_name.IsNullOrWhiteSpace())
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Achievement unique name is null or white space, skipping...");
                return false;
            }
            if (m_achievements.Keys.Contains(m_data.unique_name))
            {
                AlmanacPlugin.AlmanacLogger.LogWarning($"Duplicate achievement unique key: {m_data.unique_name}");
                return false;
            }

            if (!m_data.custom_group_key.IsNullOrWhiteSpace())
            {
                if (!CreatureLists.CustomCreatureGroups.Keys.Contains(m_data.custom_group_key))
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.unique_name}]: Failed to find custom creature group: {m_data.custom_group_key}");
                    return false;
                }
            }

            if (!m_data.custom_pickable_name.IsNullOrWhiteSpace())
            {
                if (ZNetScene.instance.GetPrefab(m_data.custom_pickable_name) is not { } prefab)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.unique_name}]: Failed to find custom pickable prefab: {m_data.custom_pickable_name}");
                    return false;
                }

                if (!prefab.GetComponent<Pickable>())
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Custom pickable prefab does not have <Pickable> component");
                    return false;
                }
            }

            if (!m_data.item.IsNullOrWhiteSpace())
            {
                if (ObjectDB.m_instance.GetItemPrefab(m_data.item) is not { } item)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.unique_name}]: Failed to find item reward: {m_data.item}");
                    return false;
                }

                if (!item.TryGetComponent(out ItemDrop itemDrop))
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.unique_name}]: Item reward does not have <ItemDrop> component");
                    return false;
                }

                m_itemReward = itemDrop;
            }

            if (!m_data.skill.IsNullOrWhiteSpace())
            {
                if (GetSkillType() is Skills.SkillType.None)
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.unique_name}]: Failed to find skill type: {m_data.skill}");
                    return false;
                }
            }

            foreach (var kvp in m_data.skill_bonus)
            {
                if (kvp.Key.IsNullOrWhiteSpace()) continue;
                if (Enum.TryParse(kvp.Key, true, out Skills.SkillType skill))
                {
                    m_skillBonus[skill] = kvp.Value;
                }
                else
                {
                    skill = (Skills.SkillType)Math.Abs(m_data.skill.GetStableHashCode());
                    if (Enum.IsDefined(typeof(Skills.SkillType), skill))
                    {
                        m_skillBonus[skill] = kvp.Value;
                    }
                    else
                    {
                        AlmanacPlugin.AlmanacLogger.LogWarning($"[{m_data.unique_name}]: Failed to find skill type for status effect: {kvp.Key}");
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

    // private static void CheckCompletion(Achievement achievement, List<Creatures.Data> list)
    // {
    //     if (!ZoneSystem.instance) return;
    //     List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
    //     int count = list.Count(critter => globalKeys.Contains(critter.m_defeatKey) || ZoneSystem.instance.GetGlobalKey(critter.m_defeatKey));
    //     achievement.m_isCompleted = count >= list.Count;
    // }
    // private static void CheckCompletion(Achievement achievement, PlayerStatType type)
    // {
    //     achievement.m_isCompleted = PlayerStats.GetPlayerStat(type) >= achievement.GetGoal();
    // }
    // private static void CheckCompletion(Achievement achievement, List<ItemDrop> list)
    // {
    //     achievement.m_isCompleted =
    //         list.FindAll(item => Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name) && !item.m_itemData.m_shared.m_dlc.IsNullOrWhiteSpace()).Count >=
    //         list.Count(x => !x.m_itemData.m_shared.m_dlc.IsNullOrWhiteSpace());
    // }
    
    public static void Read()
    {
        AchievementData.Clear();
        string[] filePaths = Directory.GetFiles(AlmanacPaths.AchievementFolderPath, "*.yml");

        IDeserializer deserializer = new DeserializerBuilder().Build();
        
        foreach (string path in filePaths)
        {
            try
            {
                string data = File.ReadAllText(path);
                AchievementYML.AchievementData YmlData = deserializer.Deserialize<AchievementYML.AchievementData>(data);
                AchievementData.Add(YmlData);
            }
            catch (Exception)
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Failed to load yml data: " + path);
            }
        }
    }

    public static void Setup()
    {
        m_achievements.Clear();
        m_groupAchievements.Clear();
        foreach (var data in AchievementData)
        {
            var _ = new Achievement(data);
        }
    }
    public static List<Achievement> GetAchievements() => m_achievements.Values.ToList();
    
}