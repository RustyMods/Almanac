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
    public static readonly List<StatusEffect> activeAlmanacEffects = new();
    public static void AddAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Add(statusEffect);
    public static void RemoveAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Remove(statusEffect);

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    static class AddExtraDamagePatch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            
            Player localPlayer = Player.m_localPlayer;
            List<StatusEffect> activeEffects = localPlayer.m_seman.GetStatusEffects();

            if (__instance.IsPlayer())
            {
                if (localPlayer.GetHoverName() != __instance.GetHoverName()) return;
                
                // Add logic on when local player is hit

            }
            if (hit.m_attacker.IsNone()) return;
            if (hit.m_hitType is not HitData.HitType.PlayerHit) return;

            GameObject attacker = ZNetScene.instance.FindInstance(hit.m_attacker);
            attacker.TryGetComponent(out Player player);
            if (!player) return;

            if (player.GetHoverName() != localPlayer.GetHoverName()) return;

            bool ranged = hit.m_ranged;
            
            foreach (var effect in activeEffects)
            {
                AlmanacEffectsManager.BaseEffectData data =
                    RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case AlmanacEffectsManager.Modifier.MeleeDMG:
                        if (!ranged) hit.m_damage.m_damage += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.RangedDMG:
                        if (ranged) hit.m_damage.m_damage += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.FireDMG:
                        hit.m_damage.m_fire += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.FrostDMG:
                        hit.m_damage.m_frost += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.LightningDMG:
                        hit.m_damage.m_lightning += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.PoisonDMG:
                        hit.m_damage.m_poison += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.SpiritDMG:
                        hit.m_damage.m_spirit += data.m_newValue;
                        break;
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

            Dictionary<AlmanacEffectsManager.Modifier, float> totalValues = new();

            foreach (StatusEffect effect in statusEffects)
            {
                if (!RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name)) continue;
                var data = RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);
                if (totalValues.ContainsKey(data.Modifier))
                {
                    totalValues[data.Modifier] += data.m_newValue;
                }
                else
                {
                    totalValues[data.Modifier] = 25f + data.m_newValue;
                }
            }

            foreach (var kvp in totalValues)
            {
                switch (kvp.Key)
                {
                    case AlmanacEffectsManager.Modifier.BaseHP:
                        player.m_baseHP = kvp.Value;
                        break;
                    case AlmanacEffectsManager.Modifier.BaseStamina :
                        player.m_baseStamina = kvp.Value;
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), typeof(StatusEffect), typeof(bool))]
    static class SEManRemoveStatusEffectPatch
    {
        private static void Postfix(SEMan __instance)
        {
            Character character = __instance.m_character;
            Player player = Player.m_localPlayer;
            if (!character || !player) return;

            if (character.GetHoverName() != player.GetHoverName()) return;
            
            List<StatusEffect> statusEffects = __instance.m_statusEffects;

            Dictionary<AlmanacEffectsManager.Modifier, float> totalValues = new()
            {
                { AlmanacEffectsManager.Modifier.BaseHP , 25f },
                { AlmanacEffectsManager.Modifier.BaseStamina , 50f}
            };

            foreach (StatusEffect effect in statusEffects)
            {
                if (!RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name)) continue;
                var data = RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);
                if (data.Modifier is not AlmanacEffectsManager.Modifier.BaseHP
                    or AlmanacEffectsManager.Modifier.BaseStamina) continue;
                
                totalValues[data.Modifier] += data.m_newValue;
            }

            foreach (var kvp in totalValues)
            {
                switch (kvp.Key)
                {
                    case AlmanacEffectsManager.Modifier.BaseHP:
                        player.m_baseHP = kvp.Value;
                        break;
                    case AlmanacEffectsManager.Modifier.BaseStamina :
                        player.m_baseStamina = kvp.Value;
                        break;
                }
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
            List<StatusEffect> statusEffectsList = __instance.m_statusEffects;
            
            List<AlmanacEffectsManager.BaseEffectData> AlmanacEffects = RegisterAlmanacEffects.effectsData;

            foreach (var statusEffect in statusEffectsList)
            {
                string name = statusEffect.name;
                if (!AlmanacEffects.Exists(x => x.effectName == name)) continue;
                var data = AlmanacEffects.Find(x => x.effectName == name);
                switch (data.Modifier)
                {
                    case AlmanacEffectsManager.Modifier.BaseHP:
                        localPlayer.m_baseHP = data.m_initialValue;
                        break;
                    case AlmanacEffectsManager.Modifier.BaseStamina :
                        localPlayer.m_baseStamina = data.m_initialValue;
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