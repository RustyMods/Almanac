using System;
using System.Collections.Generic;
using HarmonyLib;
using StatusEffectManager;
using UnityEngine;

namespace Almanac.Almanac;

public static class CustomStatusEffects
{
    private static CustomSE FirstKillSE = null!;
    private static CustomSE FirstDeathSE = null!;
    
    
    private static readonly List<CustomGuardianEffects> customGuardianPowers = new();
    
    private class CustomGuardianEffects
    {
        public powerType type;
        public string StatusEffectName = null!;
        public float initialValue;
        public float newValue;
    }

    private enum powerType
    {
        baseHP,
        baseStamina
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
        FirstKillSE.Effect.m_cooldown = 300f; // Activation cooldown
        FirstKillSE.Effect.m_cooldownIcon = false;
        FirstKillSE.Effect.m_icon = AlmanacPlugin.arrowBasicIcon;
        FirstKillSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        FirstKillSE.Effect.m_ttl = 200f; // Power effect cooldown
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            type = powerType.baseHP,
            StatusEffectName = "se_first_kill",
            initialValue = 25f,
            newValue = 35f,
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
        FirstDeathSE.Effect.m_cooldown = 15f;
        FirstDeathSE.Effect.m_icon = AlmanacPlugin.capeHoodIcon;
        FirstDeathSE.Effect.m_attributes = StatusEffect.StatusAttribute.None;
        FirstDeathSE.Effect.m_ttl = 10f;
        
        customGuardianPowers.Add(new CustomGuardianEffects()
        {
            type = powerType.baseStamina,
            StatusEffectName = "se_first_death",
            initialValue = 75f,
            newValue = 200f
        });
    }

    public static void AssignEffects()
    {
        ZNetScene scene = ZNetScene.instance;
        if (!scene) return;
        FirstKillSE.Effect.m_startEffects = CreateEffectList(
            scene,new []{"fx_DvergerMage_Mistile_die","fx_DvergerMage_Support_start"}, attach: true);
        FirstKillSE.Effect.m_stopEffects = CreateEffectList(
            scene, new[] { "fx_DvergerMage_Nova_ring" }, attach: true);

        FirstDeathSE.Effect.m_startEffects = CreateEffectList(scene, new[] { "fx_DvergerMage_MistileSpawn" });
        FirstDeathSE.Effect.m_stopEffects = CreateEffectList(scene, new[] { "fx_eikthyr_stomp" });
    }
    
    private static EffectList CreateEffectList(
        ZNetScene scene,
        string[] prefabNames,
        int variant = -1,
        bool attach = false,
        bool inheritParentRotation = false,
        bool inheritParentScale = false,
        bool scale = false,
        string childTransform = ""
    )
    {
        List<GameObject> effectGameObjects = new();
        
        foreach (string name in prefabNames)
        {
            GameObject fx = scene.GetPrefab(name);
            effectGameObjects.Add(fx);
        }

        EffectList list = new EffectList();
        EffectList.EffectData[] allEffects = new EffectList.EffectData[effectGameObjects.Count];
        for (int i = 0;  i < effectGameObjects.Count; ++i)
        {
            EffectList.EffectData effectData = new EffectList.EffectData()
            {
                m_prefab = effectGameObjects[i],
                m_enabled = true,
                m_variant = variant,
                m_attach = attach,
                m_inheritParentRotation = inheritParentRotation,
                m_inheritParentScale = inheritParentScale,
                m_scale = scale,
                m_childTransform = childTransform
            };
            allEffects[i] = effectData;
        }

        list.m_effectPrefabs = allEffects;
        return list;
    }

    

    [HarmonyPatch(typeof(Player), nameof(Player.ActivateGuardianPower))]
    static class ActivateGuardianPowerPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (!__instance.m_guardianSE) return;
            string statusEffectName = __instance.m_guardianSE.name;

            if (!customGuardianPowers.Exists(power => power.StatusEffectName == statusEffectName)) return;
            
            var data = customGuardianPowers.Find(power => power.StatusEffectName == statusEffectName);

            switch (data.type)
            {
                case powerType.baseHP:
                    __instance.m_baseHP = data.newValue;
                    break;
                case powerType.baseStamina:
                    __instance.m_baseStamina = data.newValue;
                    break;
            }
            
        }
    }

    [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Stop))]
    static class StatusEffectStopPatch
    {
        private static void Prefix(StatusEffect __instance)
        {
            if (!customGuardianPowers.Exists(power => power.StatusEffectName == __instance.name)) return;
            var data = customGuardianPowers.Find(power => power.StatusEffectName == __instance.name);
            Player player = Player.m_localPlayer;
            switch (data.type)
            {
                case powerType.baseHP:
                    player.m_baseHP = data.initialValue;
                    break;
                case powerType.baseStamina :
                    player.m_baseStamina = data.initialValue;
                    break;
            }
        }
    }
    

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveAllStatusEffects))]
    static class RemoveAllStatusEffectsPatch
    {
        private static void Prefix(SEMan __instance)
        {
            var character = __instance.m_character;
            Player localPlayer = Player.m_localPlayer;
            if (character.GetHoverName() != localPlayer.GetHoverName()) return;
            var statusEffectsList = __instance.m_statusEffects;
            foreach (var statusEffect in statusEffectsList)
            {
                var name = statusEffect.name;
                if (!customGuardianPowers.Exists(power => power.StatusEffectName == name)) return;
                var data = customGuardianPowers.Find(power => power.StatusEffectName == name);
                switch (data.type)
                {
                    case powerType.baseHP:
                        localPlayer.m_baseHP = data.initialValue;
                        break;
                    case powerType.baseStamina:
                        localPlayer.m_baseStamina = data.initialValue;
                        break;
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