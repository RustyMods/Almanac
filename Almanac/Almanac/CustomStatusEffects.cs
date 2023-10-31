using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StatusEffectManager;
using UnityEngine;

namespace Almanac.Almanac;

public static class CustomStatusEffects
{
    private static CustomSE FirstKillSE = null!;
    private static CustomSE FirstDeathSE = null!;
    private static CustomSE PackMuleSE = null!;
    private static CustomSE ApprenticeSE = null!;
    
    public static readonly List<CustomGuardianEffects> customGuardianPowers = new();
    public static List<StatusEffect> activeAlmanacEffects = new();
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
        maxCarryWeight
    }

    private enum Value
    {
        initialValue,
        newValue
    }

    public static void AddAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Add(statusEffect);
    public static void RemoveAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Remove(statusEffect);

    private static void SetCustomGuardianEffects(Player player, CustomGuardianEffects data, Value valueType)
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
        
        FirstKillSE = new CustomSE("se_first_kill");
        FirstKillSE.Name.English("First Kill");
        FirstKillSE.Type = EffectType.Consume;
        FirstKillSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        FirstKillSE.Effect.m_startMessage = "$almanac_first_kill_start_msg";
        FirstKillSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        FirstKillSE.Effect.m_stopMessage = "$almanac_first_kill_stop_msg";
        FirstKillSE.Effect.m_tooltip = "<color=white>Increases base health by </color><color=orange>10</color>";
        // FirstKillSE.Effect.m_cooldown = 300f; // Activation cooldown
        FirstKillSE.Effect.m_cooldownIcon = false;
        FirstKillSE.Effect.m_icon = AlmanacPlugin.arrowBasicIcon;
        FirstKillSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        // FirstKillSE.Effect.m_ttl = 200f; // Power effect cooldown
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.baseHP,
            m_name = "se_first_kill",
            m_initialValue = 25f,
            m_newValue = 30f,
        });

        FirstDeathSE = new CustomSE("se_first_death");
        FirstDeathSE.Name.English("First Death");
        FirstDeathSE.Type = EffectType.Consume;
        FirstDeathSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        FirstDeathSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        FirstDeathSE.Effect.m_startMessage = "$almanac_first_death_start_msg";
        FirstDeathSE.Effect.m_stopMessage = "$almanac_first_death_stop_msg";
        FirstDeathSE.Effect.m_tooltip = "<color=white>Increase base stamina by</color><color=orange> 25</color>";
        FirstDeathSE.Effect.m_cooldownIcon = false;
        // FirstDeathSE.Effect.m_cooldown = 15f;
        FirstDeathSE.Effect.m_icon = AlmanacPlugin.capeHoodIcon;
        FirstDeathSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        // FirstDeathSE.Effect.m_ttl = 10f;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.baseStamina,
            m_name = "se_first_death",
            m_initialValue = 75f,
            m_newValue = 100f
        });

        PackMuleSE = new CustomSE("se_pack_mule");
        PackMuleSE.Name.English("Pack Mule");
        PackMuleSE.Type = EffectType.Consume;
        PackMuleSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        PackMuleSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        PackMuleSE.Effect.m_startMessage = "$almanac_pack_mule_start_msg";
        PackMuleSE.Effect.m_stopMessage = "$almanac_pack_mule_stop_msg";
        PackMuleSE.Effect.m_tooltip = "<color=white>Increase base carry weight by</color><color=orange> 100</color>";
        PackMuleSE.Effect.m_cooldownIcon = false;
        // PackMuleSE.Effect.m_cooldown = 15f;
        PackMuleSE.Effect.m_icon = AlmanacPlugin.woodLogsIcon;
        PackMuleSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        // PackMuleSE.Effect.m_ttl = 10f;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.maxCarryWeight,
            m_name = "se_pack_mule",  
            m_initialValue = 300f,
            m_newValue = 325f
        });
        
        ApprenticeSE = new CustomSE("se_apprentice");
        ApprenticeSE.Name.English("Apprentice");
        ApprenticeSE.Type = EffectType.Consume;
        ApprenticeSE.Effect.m_startMessageType = MessageHud.MessageType.Center;
        ApprenticeSE.Effect.m_stopMessageType = MessageHud.MessageType.Center;
        ApprenticeSE.Effect.m_startMessage = "$almanac_apprentice_start_msg";
        ApprenticeSE.Effect.m_stopMessage = "$almanac_apprentice_stop_msg";
        ApprenticeSE.Effect.m_tooltip = "<color=white>Increase base eitr by</color><color=orange> 100</color>";
        ApprenticeSE.Effect.m_cooldownIcon = false;
        // ApprenticeSE.Effect.m_cooldown = 15f;
        ApprenticeSE.Effect.m_icon = AlmanacPlugin.boneWhiteIcon;
        ApprenticeSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        // ApprenticeSE.Effect.m_ttl = 10f;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            m_modifier = Modifier.baseHP,
            m_name = "se_apprentice",
            m_initialValue = 25f,
            m_newValue = 40f
        });
        
    }

    public static void AssignSpecialEffects()
    {
        ZNetScene scene = ZNetScene.instance;
        if (!scene) return;
        
        AssignEffect(scene, FirstKillSE.Effect,
            new List<KeyValuePair<string, EffectData>>()
            {
                new ("fx_DvergerMage_Mistile_die", new EffectData(){attach = true}),
                new ("fx_DvergerMage_Support_start", new EffectData(){attach = true})
            },
            new List<KeyValuePair<string, EffectData>>()
        {
            new ("fx_DvergerMage_Nova_ring", new EffectData(){attach = true})
        });
        AssignEffect(scene, FirstDeathSE.Effect,
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
                    SetCustomGuardianEffects(player, data, Value.newValue);
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
                SetCustomGuardianEffects(player, data, Value.initialValue);
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
                    SetCustomGuardianEffects(localPlayer, data, Value.initialValue);
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