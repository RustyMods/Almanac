using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using HarmonyLib;
using StatusEffectManager;
using UnityEngine;

namespace Almanac.Almanac;

public static class CustomStatusEffects
{
    private static CustomSE MeadowKillSE = null!;
    private static CustomSE BlackForestKillSE = null!;
    private static CustomSE RangerSE = null!;
    private static CustomSE KnowledgeableSE = null!;
    private static CustomSE BrewMasterSE = null!;
    private static CustomSE MasterArcherSE = null!;
    
    public static readonly List<CustomGuardianEffects> customGuardianPowers = new();
    public static readonly List<StatusEffect> activeAlmanacEffects = new();
    public class CustomGuardianEffects
    {
        public Modifier m_modifier;
        public string m_name = null!;
        public float m_initialValue;
        public float m_newValue;
    }
    
    private class EffectData
    {
        public bool enabled = true;
        public int variant = -1;
        public bool attach = false;
        public bool inheritParentRotation = false;
        public bool inheritParentScale = false;
        public bool scale = false;
        public string childTransform = "";
    }
    public enum Modifier
    {
        baseHP,
        baseStamina,
        maxCarryWeight,
        meleeDmg,
        projectileDmg,
        addFireDmg,
        addFrostDmg,
        addPoisonDmg,
        addLightningDmg,
        addSpiritDmg,
    }

    private enum Value
    {
        initialValue,
        newValue
    }

    public static void AddAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Add(statusEffect);
    public static void RemoveAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Remove(statusEffect);

    private static void SetAlmanacPlayerEffects(Player player, CustomGuardianEffects data, Value valueType)
    {
        float value = 0f;
        switch (valueType)
        {
            case Value.initialValue:
                value = data.m_initialValue;
                break;
            case Value.newValue:
                value = data.m_newValue;
                break;
        }
        switch (data.m_modifier)
        {
            case Modifier.baseHP:
                player.m_baseHP = value;
                break;
            case Modifier.baseStamina :
                player.m_baseStamina = value;
                break;
            case Modifier.maxCarryWeight:
                player.m_maxCarryWeight = value;
                break;
        }
    }

    public static void RegisterCustomStatusEffects()
    {   
        customGuardianPowers.Clear();
        
        MeadowKillSE = new CustomSE("se_meadow_kill");
        MeadowKillSE.Name.English("Meadow Hunter");
        MeadowKillSE.Type = EffectType.Consume;
        MeadowKillSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        MeadowKillSE.Effect.m_startMessage = "$almanac_meadow_kill_start_msg";
        MeadowKillSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        MeadowKillSE.Effect.m_stopMessage = "$almanac_meadow_kill_stop_msg";
        MeadowKillSE.Effect.m_tooltip = "<color=white>$almanac_increase_health_by </color><color=orange>10</color>";
        MeadowKillSE.Effect.m_cooldownIcon = false;
        MeadowKillSE.Effect.m_icon = AlmanacPlugin.arrowBasicIcon;
        MeadowKillSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.baseHP,
            m_name = "se_meadow_kill",
            m_initialValue = 25f,
            m_newValue = 30f,
        });
        
        BlackForestKillSE = new CustomSE("se_blackforest_kill");
        BlackForestKillSE.Name.English("BlackForest Hunter");
        BlackForestKillSE.Type = EffectType.Consume;
        BlackForestKillSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        BlackForestKillSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        BlackForestKillSE.Effect.m_startMessage = "$almanac_blackforest_killer_start_msg";
        BlackForestKillSE.Effect.m_stopMessage = "$almanac_blackforest_killer_stop_msg";
        BlackForestKillSE.Effect.m_tooltip = "<color=white>$almanac_increase_health_by</color><color=orange> 10</color>";
        BlackForestKillSE.Effect.m_cooldownIcon = false;
        BlackForestKillSE.Effect.m_icon = AlmanacPlugin.boneWhiteIcon;
        BlackForestKillSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.baseHP,
            m_name = "se_blackforest_kill",
            m_initialValue = 25f,
            m_newValue = 35f
        });
        
        RangerSE = new CustomSE("se_ranger");
        RangerSE.Name.English("Ranger");
        RangerSE.Type = EffectType.Consume;
        RangerSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        RangerSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        RangerSE.Effect.m_startMessage = "$almanac_ranger_start_msg";
        RangerSE.Effect.m_stopMessage = "$almanac_ranger_stop_msg";
        RangerSE.Effect.m_tooltip = "<color=white>$almanac_increase_projectile_damage_by</color><color=orange> 10</color>";
        RangerSE.Effect.m_cooldownIcon = false;
        RangerSE.Effect.m_icon = AlmanacPlugin.capeHoodIcon;
        RangerSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.projectileDmg,
            m_name = "se_ranger",
            m_initialValue = 0f,
            m_newValue = 10f
        });
        
        KnowledgeableSE = new CustomSE("se_knowledgeable");
        KnowledgeableSE.Name.English("Knowledgeable");
        KnowledgeableSE.Type = EffectType.Consume;
        KnowledgeableSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        KnowledgeableSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        KnowledgeableSE.Effect.m_startMessage = "$almanac_knowledgeable_start_msg";
        KnowledgeableSE.Effect.m_stopMessage = "$almanac_knowledgeable_stop_msg";
        KnowledgeableSE.Effect.m_tooltip = "<color=white>$almanac_increase_carry_weight_by</color><color=orange> 100</color>";
        KnowledgeableSE.Effect.m_cooldownIcon = false;
        KnowledgeableSE.Effect.m_icon = AlmanacPlugin.necklaceSilverRed;
        KnowledgeableSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.maxCarryWeight,
            m_name = "se_knowledgeable",  
            m_initialValue = 300f,
            m_newValue = 400f
        });
        
        BrewMasterSE = new CustomSE("se_brew_master");
        BrewMasterSE.Name.English("Brew Master");
        BrewMasterSE.Type = EffectType.Consume;
        BrewMasterSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        BrewMasterSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        BrewMasterSE.Effect.m_startMessage = "$almanac_brew_master_start_msg";
        BrewMasterSE.Effect.m_stopMessage = "$almanac_brew_master_stop_msg";
        BrewMasterSE.Effect.m_tooltip = "<color=white>$almanac_increase_fire_damage_by</color><color=orange> 10</color>";
        BrewMasterSE.Effect.m_cooldownIcon = false;
        BrewMasterSE.Effect.m_icon = AlmanacPlugin.bottleStandardBlueIcon;
        BrewMasterSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.addFireDmg,
            m_name = "se_brew_master",  
            m_initialValue = 0f,
            m_newValue = 10f
        });
        
        MasterArcherSE = new CustomSE("se_master_archer");
        MasterArcherSE.Name.English("Master Archer");
        MasterArcherSE.Type = EffectType.Consume;
        MasterArcherSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        MasterArcherSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        MasterArcherSE.Effect.m_startMessage = "$almanac_master_archer_start_msg";
        MasterArcherSE.Effect.m_stopMessage = "$almanac_master_archer_stop_msg";
        MasterArcherSE.Effect.m_tooltip = "<color=white>$almanac_increase_projectile_damage_by</color><color=orange> 15</color>";
        MasterArcherSE.Effect.m_cooldownIcon = false;
        MasterArcherSE.Effect.m_icon = AlmanacPlugin.capeHoodIcon;
        MasterArcherSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.projectileDmg,
            m_name = "se_master_archer",
            m_initialValue = 0f,
            m_newValue = 15f
        });
    }

    public static void AssignSpecialEffects()
    {
        ZNetScene scene = ZNetScene.instance;
        if (!scene) return;
        
        AssignEffect(scene, MeadowKillSE.Effect,
            new List<KeyValuePair<string, EffectData>>()
            {
                new ("fx_DvergerMage_Mistile_die", new EffectData(){attach = true}),
                new ("fx_DvergerMage_Support_start", new EffectData(){attach = true})
            },
            new List<KeyValuePair<string, EffectData>>()
        {
            new ("fx_DvergerMage_Nova_ring", new EffectData(){attach = true})
        });
        AssignEffect(scene, BlackForestKillSE.Effect,
            new List<KeyValuePair<string, EffectData>>()
            {
                new("fx_DvergerMage_MistileSpawn", new EffectData()),
            },
            new List<KeyValuePair<string, EffectData>>()
            {
                new ("fx_eikthyr_stomp", new EffectData())
            });
    }

    private static void AssignEffect(
        ZNetScene scene,
        StatusEffect effect,
        List<KeyValuePair<string, EffectData>> startEffectsMap,
        List<KeyValuePair<string, EffectData>> stopEffectsMap)
    {
        effect.m_startEffects = CreateEffectList(scene, startEffectsMap);
        effect.m_stopEffects = CreateEffectList(scene, stopEffectsMap);
    }
    
    private static EffectList CreateEffectList(
        ZNetScene scene,
        List<KeyValuePair<string, EffectData>> effectsMap)
    {
        EffectList list = new();
        EffectList.EffectData[] allEffects = new EffectList.EffectData[effectsMap.Count];

        for (int i = 0; i < effectsMap.Count; ++i)
        {
            KeyValuePair<string, EffectData> effect = effectsMap[i];
            GameObject fx = scene.GetPrefab(effect.Key);
            EffectList.EffectData effectData = new EffectList.EffectData()
            {
                m_prefab = fx,
                m_enabled = effect.Value.enabled,
                m_variant = effect.Value.variant,
                m_attach = effect.Value.attach,
                m_inheritParentRotation = effect.Value.inheritParentRotation,
                m_inheritParentScale = effect.Value.inheritParentScale,
                m_scale = effect.Value.scale,
                m_childTransform = effect.Value.childTransform
            };

            allEffects[i] = effectData;

        }

        list.m_effectPrefabs = allEffects;

        return list;
    }

    
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    static class AddExtraDamagePatch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!__instance) return;
            if (__instance.IsPlayer()) return;
            if (hit.m_attacker.IsNone()) return;
            if (hit.m_hitType is not HitData.HitType.PlayerHit) return;
            
            GameObject attacker = ZNetScene.instance.FindInstance(hit.m_attacker);
            attacker.TryGetComponent(out Player player);
            if (!player) return;
            
            Player localPlayer = Player.m_localPlayer;
            if (player.GetHoverName() != localPlayer.GetHoverName()) return;
            
            bool ranged = hit.m_ranged;

            List<StatusEffect> activeEffects = localPlayer.m_seman.GetStatusEffects();

            foreach (var effect in activeEffects)
            {
                if (customGuardianPowers.Exists(custom => custom.m_name == effect.name))
                {
                    var data = customGuardianPowers.Find(power => power.m_name == effect.name);
                    switch (data.m_modifier)
                    {
                        case Modifier.meleeDmg:
                            if (!ranged) hit.m_damage.m_damage += data.m_newValue;
                            break;
                        case Modifier.projectileDmg:
                            if (ranged) hit.m_damage.m_damage += data.m_newValue;
                            break;
                        case Modifier.addFireDmg:
                            hit.m_damage.m_fire += data.m_newValue;
                            break;
                        case Modifier.addFrostDmg:
                            hit.m_damage.m_frost += data.m_newValue;
                            break;
                        case Modifier.addLightningDmg:
                            hit.m_damage.m_lightning += data.m_newValue;
                            break;
                        case Modifier.addPoisonDmg:
                            hit.m_damage.m_poison += data.m_newValue;
                            break;
                        case Modifier.addSpiritDmg:
                            hit.m_damage.m_spirit += data.m_newValue;
                            break;
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(SEMan),nameof(SEMan.AddStatusEffect), typeof(StatusEffect), typeof(bool), typeof(int), typeof(float))]
    static class AddStatusEffectPatch
    {
        private static void Postfix(SEMan __instance)
        {
            Character character = __instance.m_character;
            if (!character) return;
            if (!Player.m_localPlayer) return;
            Player player = Player.m_localPlayer;
            if (player.GetHoverName() != character.GetHoverName()) return;
            
            List<StatusEffect> statusEffects = __instance.m_statusEffects;
            foreach (StatusEffect effect in statusEffects)
            {
                if (!customGuardianPowers.Exists(power => power.m_name == effect.name)) continue;
                List<CustomGuardianEffects> dataList = customGuardianPowers.FindAll(power => power.m_name == effect.name);
                foreach (CustomGuardianEffects data in dataList)
                {
                    SetAlmanacPlayerEffects(player, data, Value.newValue);
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Stop))]
    static class StatusEffectStopPatch
    {
        private static void Prefix(StatusEffect __instance)
        {
            if (!customGuardianPowers.Exists(power => power.m_name == __instance.name)) return;
            Player player = Player.m_localPlayer;
            List<CustomGuardianEffects> dataList = customGuardianPowers.FindAll(power => power.m_name == __instance.name);
            foreach (CustomGuardianEffects data in dataList)
            {
                SetAlmanacPlayerEffects(player, data, Value.initialValue);
            }
        }
    }
    

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveAllStatusEffects))]
    static class RemoveAllStatusEffectsPatch
    {
        private static void Prefix(SEMan __instance)
        {
            Character character = __instance.m_character;
            Player localPlayer = Player.m_localPlayer;
            if (character.GetHoverName() != localPlayer.GetHoverName()) return;
            var statusEffectsList = __instance.m_statusEffects;
            foreach (var statusEffect in statusEffectsList)
            {
                string name = statusEffect.name;
                if (!customGuardianPowers.Exists(power => power.m_name == name)) return;
                List<CustomGuardianEffects> dataList = customGuardianPowers.FindAll(power => power.m_name == name);
                foreach (CustomGuardianEffects data in dataList)
                {
                    SetAlmanacPlayerEffects(localPlayer, data, Value.initialValue);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ZLog), nameof(ZLog.LogWarning))]
    static class MuteGuardianPowerStats
    {
        private static bool Prefix(object o)
        {
            return !o.ToString().StartsWith("Missing stat for guardian power");
        }
    }
}