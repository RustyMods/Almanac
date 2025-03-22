using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using static Almanac.UI.Entries;
using static Almanac.Utilities.Helpers;

namespace Almanac.Data;

public static class Creatures
{
    public static readonly Dictionary<string, Data> m_creatures = new();
    public static readonly Dictionary<string, Data> m_defeatKeyCreatures = new();
    public static readonly List<string> m_defeatKeys = new();

    private static readonly Dictionary<string, string> m_specialNames = new Dictionary<string, string>()
    {
        { "Leech_cave", "$enemy_leech_cave" },
        { "DvergerMageFire", "$enemy_dverger_mage_fire" },
        { "DvergerMageIce", "$enemy_dverger_mage_ice" },
        { "DvergerMageSupport", "$enemy_dverger_mage_support" },
        { "BRV_SkeletonEnemy", "$almanac_summoned $brv_summoned_skeleton" },
        { "BRV_BatEnemy", "$almanac_summoned $brv_summoned_bat" },
        { "BLV_NeutralVikings", "$almanac_neutral $blv_renegademeadows"},
        { "BLV_OverlordVikingT6Summoner", "$blv_overlordsummoner" },
        { "BLV_TraderViking", "$blv_tradername" },
        { "BLV_OverlordVikingT6", "$blv_overlordmistlands" },
        { "BLV_RenegadeVikingT6Magic", "$almanac_mistlands $almanac_renegade_sorcerer" },
        { "BLV_RenegadeVikingT6Melee", "$almanac_mistlands $blv_renegademistlandsmelee" },
        { "BLV_OverlordVikingT5", "$blv_overlordplains" },
        { "BLV_RenegadeVikingT5", "$almanac_plains $blv_renegadeplains" },
        { "BLV_OverlordVikingT4", "$blv_overlordmountains" },
        { "BLV_RenegadeVikingT4", "$almanac_mountains $blv_renegademountains" },
        { "BLV_OverlordVikingT3", "$blv_overlordswamps" },
        { "BLV_RenegadeVikingT3", "$almanac_swamps $blv_renegadeswamps" },
        { "BLV_OverlordVikingT2", "$blv_overlordblackforest" },
        { "BLV_RenegadeVikingT2", "$almanac_blackforest $blv_renegadeblackforest" },
        { "BLV_OverlordVikingT1", "$blv_overlordmeadows" },
        { "BLV_RenegadeVikingT1", "$almanac_meadows $blv_renegademeadows" },
        { "ML_Draugr_Spawn", "$almanac_spawned $enemy_draugr" },
        { "Draugr_Ranged", "$almanac_ranged $enemy_draugr" },
        { "ML_Draugr_Boat", "$enemy_draugr $almanac_boat" },
        { "ML_FrostHatchling_Spawn", "$almanac_spawned $enemy_drake" },
        { "ML_FrostHatchling", "$almanac_frost $enemy_drake" },
        { "MolluscanLand", "$almanac_land $enemy_molluscanland" },
        { "ML_DraugrBomber_Boat", "$enemy_draugr $almanac_bomber" },
        { "NightmareDragonEV", "$enemy_nightmaredragon EV" },
        { "ML_GoblinLox", "$enemy_goblin $enemy_lox" },
        { "DwarfGoblin_Spawn", "$almanac_spawned $enemy_goblindwarf" },
        { "GhostWarrior", "$enemy_ghost $almanac_warrior" },
        { "NormalGhostWarrior", "$almanac_normal $enemy_ghost $almanac_warrior" },
        { "PoisonSkeletonWarrior", "$enemy_skeletonpoison $almanac_warrior" },
        { "NormalSkeletonWarrior", "$enemy_skeleton $almanac_warrior" },
        { "MLabz1_DummyTest", "$enemy_skeleton $almanac_dummy" },
        { "ML_Surtling", "$enemy_surtling ML" },
        { "WraithWarrior", "$enemy_wraith $almanac_warrior" },
        { "ML_AshHuldraQueen2", "$enemy_ashhuldra_queen I" },
        { "ML_AshHuldraQueen3", "$enemy_ashhuldra_queen III" },
        { "BrownSpider_Spawn", "$almanac_spawned $enemy_brownspider" },
        { "DwarfGoblin_NoAttack", "$almanac_passive $enemy_goblindwarf" },
        { "DwarfGoblin_Boat", "$enemy_goblindwarf $almanac_boat $almanac_rider" },
        { "GoblinArcher", "$almanac_ranged $enemy_goblin" },
        { "DwarfGoblinShaman_Boat", "$enemy_goblindwarf $almanac_boat $almanac_shaman" },
        { "ML_BlueMistile_Aggressive", "$almanac_blue $enemy_mistile" },
        { "ML_RedMistile_Aggressive", "$almanac_red $enemy_mistile" },
        { "ML_BlueMistile_Passive", "$almanac_passive $almanac_blue $enemy_mistile" },
        { "ML_RedMistile_Passive", "$almanac_passive $almanac_red $enemy_mistile" },
        { "MLNPC_Female1", "$enemy_npc_female1" },
        { "Skeleton_NoArcher", "$enemy_skeleton $almanac_melee" },
        { "ML_Sword_Frigga_Spider_2", "$ally_brownspider_spawn 2" },
        { "ML_Sword_Frigga_Spider_3_Light", "$ally_brownspider_spawn $almanac_spirit" },
        { "ML_Sword_Frigga_Spider_3_Fire", "$ally_brownspider_spawn $almanac_fire" },
        { "ML_Sword_Frigga_Spider_3_Cold", "$ally_brownspider_spawn $almanac_frost" },
        { "TreeSpider_Spawn", "$almanac_spawned $enemy_treespider" },
        { "Skeleton_Hildir_nochest", "$enemy_skeletonfire $almanac_nochest" },
        { "Fenring_Cultist_Hildir_nochest", "$enemy_fenringcultist_hildir $almanac_nochest" },
        { "GoblinBruteBros_nochest", "$enemy_goblinbrute_hildircombined $almanac_nochest" },
        { "GoblinShaman_Hildir_nochest", "$enemy_goblin_hildir $almanac_nochest" }
    };
    
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix()
        {
            if (!ZNetScene.instance) return;
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (prefab.name.Contains("(Clone)") || prefab.name is "DvergerTest" or "TheHive" or "Hive") continue;
                var _ = new Data(prefab);
            }
        }
    }
    public class Data
    {
        public Data(GameObject prefab)
        {
            if (!prefab.TryGetComponent(out Character character)) return;
            m_prefabName = prefab.name;
            m_name = character.m_name;
            if (m_specialNames.ContainsKey(m_name)) m_name = m_specialNames[m_name];
            m_health = character.m_health;
            m_faction = character.m_faction;
            m_resistances = character.m_damageModifiers;
            m_tolerateFire = character.m_tolerateFire;
            m_weakspots = character.m_weakSpots.Select(x => x.gameObject.name).ToList();
            m_staggerWhenBlocked = character.m_staggerWhenBlocked;
            m_staggerDamageFactor = character.m_staggerDamageFactor;
            if (m_defeatKeys.Contains(character.m_defeatSetGlobalKey))
            {
                var newKey = $"defeated_{m_prefabName.ToLower()}";
                character.m_defeatSetGlobalKey = newKey;
            }
            if (character.m_defeatSetGlobalKey.IsNullOrWhiteSpace())
            {
                character.m_defeatSetGlobalKey = $"defeated_{m_prefabName.ToLower()}";
            }
            m_defeatKey = character.m_defeatSetGlobalKey;
            if (prefab.TryGetComponent(out CharacterDrop characterDrop)) m_drops = characterDrop.m_drops;
            if (prefab.TryGetComponent(out BaseAI baseAI))
            {
                m_viewRange = baseAI.m_viewRange;
                m_viewAngle = baseAI.m_viewAngle;
                m_hearRange = baseAI.m_hearRange;
                m_mistVision = baseAI.m_mistVision;
                m_avoidFire = baseAI.m_avoidLava;
                m_afraidOfFire = baseAI.m_afraidOfFire;
                m_avoidWater = baseAI.m_avoidWater;
                m_avoidLava = baseAI.m_avoidLava;
                m_aggravatable = baseAI.m_aggravatable;
                m_passiveAggressive = baseAI.m_passiveAggresive;
                m_spawnMessage = baseAI.m_spawnMessage;
                m_deathMessage = baseAI.m_deathMessage;
                m_alertedMessage = baseAI.m_alertedMessage;
                m_fleeRange = baseAI.m_fleeRange;
                m_fleeAngle = baseAI.m_fleeAngle;
                m_patrol = baseAI.m_patrol;
            }
            if (prefab.TryGetComponent(out Tameable tameable))
            {
                m_tameable = true;
                m_fedDuration = tameable.m_fedDuration;
                m_tamingTime = tameable.m_tamingTime;
                m_startsTamed = tameable.m_startsTamed;
                m_commandeable = tameable.m_commandable;
                m_unsummonDistance = tameable.m_unsummonDistance;
                if (tameable.m_saddleItem) m_saddleItem = tameable.m_saddleItem;
                m_randomStartingNames = tameable.m_randomStartingName;
            }
            if (prefab.TryGetComponent(out MonsterAI monsterAI))
            {
                m_consumeItems = monsterAI.m_consumeItems;
            }
            if (prefab.TryGetComponent(out Humanoid humanoid))
            {
                foreach (var item in humanoid.m_defaultItems)
                {
                    var attack = new Attack(item);
                    if (!attack.m_isValid || m_attacks.Any(x => x.m_name == attack.m_name)) continue;
                    m_attacks.Add(attack);
                }
            }
            foreach (var drop in m_drops)
            {
                if (!drop.m_prefab.TryGetComponent(out ItemDrop component)) continue;
                if (component.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Trophy) continue;
                m_trophy = component.m_itemData;
                break;
            }

            m_creatures[m_prefabName] = this;
            m_defeatKeys.Add(m_defeatKey);
            m_defeatKeyCreatures[m_defeatKey] = this;
        }

        public Sprite? GetIcon() => m_trophy?.GetIcon() ?? SpriteManager.AlmanacIcon;

        public List<Entry> GetEntries()
        {
            EntryBuilder builder = new();
            builder.Add("$se_health", m_health);
            builder.Add("$label_faction", m_faction);
            builder.Add("$title_resistances");
            builder.Add("$inventory_blunt", m_resistances.m_blunt);
            builder.Add("$inventory_slash", m_resistances.m_slash);
            builder.Add("$inventory_pierce", m_resistances.m_pierce);
            builder.Add("$inventory_chop", m_resistances.m_chop);
            builder.Add("$inventory_pickaxe", m_resistances.m_pickaxe);
            builder.Add("$inventory_fire", m_resistances.m_fire);
            builder.Add("$inventory_frost", m_resistances.m_fire);
            builder.Add("$inventory_lightning", m_resistances.m_lightning);
            builder.Add("$inventory_poison", m_resistances.m_poison);
            builder.Add("$inventory_spirit", m_resistances.m_spirit);
            if (PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(m_defeatKey, out KillDeaths kd))
            {
                if (kd.deaths + kd.kills != 0)
                {
                    builder.Add("$title_killdeaths");
                    builder.Add("$label_kill", kd.kills);
                    builder.Add("$label_death", kd.deaths);
                    builder.Add("$label_ratio", kd.kills / Math.Max(kd.deaths, 1));
                }
            }
            builder.Add("$title_creaturedata");
            builder.Add("$label_avoidfire", m_avoidFire);
            builder.Add("$label_afraidoffire", m_afraidOfFire);
            builder.Add("$label_avoidwater", m_avoidWater);
            foreach (var weak in m_weakspots)
            {
                builder.Add("$label_weakspot", weak);
            }
            builder.Add("$label_staggerwhenblocked", m_staggerWhenBlocked);
            builder.Add("$label_damagefactor", m_staggerDamageFactor);
            builder.Add("$label_toleratewater", m_tolerateWater);
            builder.Add("$label_toleratesmoke", m_tolerateSmoke);
            builder.Add("$label_toleratetar", m_tolerateTar);
            builder.Add("$label_defeatkey", m_defeatKey);
            foreach (var attack in m_attacks)
            {
                builder.Add("$title_attacks");
                builder.Add("$label_prefabname", attack.m_name);
                builder.Add("$inventory_damage", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_damage);
                builder.Add("$inventory_blunt", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_blunt);
                builder.Add("$inventory_slash", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_slash);
                builder.Add("$inventory_pierce", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_pierce);
                builder.Add("$inventory_chop", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_chop);
                builder.Add("$inventory_pickaxe", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_pickaxe);
                builder.Add("$inventory_fire", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_fire);
                builder.Add("$inventory_frost", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_frost);
                builder.Add("$inventory_lightning", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_lightning);
                builder.Add("$inventory_poison", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_poison);
                builder.Add("$inventory_spirit", attack.m_itemDrop.m_itemData.m_shared.m_damages.m_spirit);
                builder.Add("$item_backstab", attack.m_itemDrop.m_itemData.m_shared.m_backstabBonus);
                builder.Add("$label_dodgeable", attack.m_itemDrop.m_itemData.m_shared.m_dodgeable);
                builder.Add("$label_blockable", attack.m_itemDrop.m_itemData.m_shared.m_blockable);
                if (attack.m_itemDrop.m_itemData.m_shared.m_attackStatusEffect is { } status)
                {
                    builder.Add("$title_attackeffect");
                    builder.Add(status.GetTooltipString(), "lore");
                }
            }

            return builder.ToList();
        }

        [Header("Character")]
        public readonly string m_prefabName = "";
        public readonly string m_name = "";
        public readonly float m_health;
        public readonly Character.Faction m_faction = Character.Faction.Boss;
        public HitData.DamageModifiers m_resistances = new();
        public bool m_tolerateWater;
        public bool m_tolerateFire;
        public bool m_tolerateSmoke;
        public bool m_tolerateTar;
        public bool m_staggerWhenBlocked;
        public float m_staggerDamageFactor;
        public List<string> m_weakspots = new();
        [Header("Base AI")]
        public float m_viewRange;
        public float m_viewAngle;
        public float m_hearRange;
        public bool m_mistVision;
        public bool m_avoidFire;
        public bool m_afraidOfFire;
        public bool m_avoidWater;
        public bool m_avoidLava;
        public bool m_aggravatable;
        public bool m_passiveAggressive;
        public string m_spawnMessage = "";
        public string m_deathMessage = "";
        public string m_alertedMessage = "";
        public float m_fleeRange;
        public float m_fleeAngle;
        public bool m_patrol;
        [Header("Tameable")] 
        public bool m_tameable;
        public float m_fedDuration;
        public float m_tamingTime;
        public bool m_startsTamed;
        public bool m_commandeable;
        public float m_unsummonDistance;
        public ItemDrop? m_saddleItem;
        public List<string> m_randomStartingNames = new();
        [Header("Character Drop")]
        public readonly List<CharacterDrop.Drop> m_drops = new();
        [Header("Monster AI")]
        public readonly List<ItemDrop> m_consumeItems = new();
        
        public readonly ItemDrop.ItemData? m_trophy;
        public readonly string m_defeatKey = "";
        public readonly List<Attack> m_attacks = new();

        public class Attack
        {
            public readonly bool m_isValid;
            public readonly string m_name;
            public readonly ItemDrop m_itemDrop = null!;

            public Attack(GameObject prefab)
            {
                m_name = prefab.name;
                if (!prefab.TryGetComponent(out ItemDrop component)) return;
                m_itemDrop = component;
                m_isValid = true;
            }
        }
    }
}