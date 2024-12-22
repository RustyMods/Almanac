using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Almanac.Utilities;
using HarmonyLib;
using UnityEngine;
using static Almanac.AlmanacPlugin;

namespace Almanac.Achievements;

public static class AlmanacEffectManager
{
    public static readonly List<StatusEffect> ActiveAchievementEffects = new();
    public static List<string> SavedAchievementEffectNames = new();
    public static readonly string AchievementKey = "AlmanacAchievements";

    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.AddActiveEffects))]
    static class CompendiumAddActiveEffectsPatch
    {
        private static void Postfix(TextsDialog __instance)
        {
            if (_AchievementIcons.Value is Toggle.On) return;
            if (!Player.m_localPlayer) return;
            if (ActiveAchievementEffects.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(__instance.m_texts[0].m_text);
                stringBuilder.Append("\n\n");
                foreach (StatusEffect effect in ActiveAchievementEffects)
                {
                    stringBuilder.AppendFormat("<color=orange>{0}</color>", effect.m_name);
                    stringBuilder.AppendFormat("\n{0}", effect.GetTooltipString());
                }
                
                __instance.m_texts[0].m_text = stringBuilder.ToString();
            }
        }
    }
    public enum Modifier
    {
        None,
        Attack,
        HealthRegen,
        StaminaRegen,
        RaiseSkills,
        Speed,
        Noise,
        MaxCarryWeight,
        Stealth,
        RunStaminaDrain,
        DamageReduction,
        FallDamage,
        EitrRegen,
        Health,
        Stamina,
        Eitr,
        LifeSteal,
        Armor
    }
    public class EffectData
    {
        public string effectName = null!;
        public string displayName = "";
        public int duration = 0;
        public Sprite? sprite;
        public string[]? startEffectNames;
        public string[]? stopEffectNames;
        public string? startMsg = "";
        public string? stopMsg = "";
        public string? effectTooltip = "";
        public List<HitData.DamageModPair> damageMods = new();
        public Dictionary<Modifier, float> m_modifiers = new();
        public readonly Dictionary<Skills.SkillType, float> m_skills = new();
        public StatusEffect? Init()
        {
            ObjectDB obd = ObjectDB.instance;

            // Make sure new effects have unique names
            if (obd.m_StatusEffects.Find(effect => effect.name == effectName)) return null;
            AchievementEffect Effect = ScriptableObject.CreateInstance<AchievementEffect>();
            Effect.name = effectName;
            Effect.m_nameHash = effectName.GetStableHashCode();
            Effect.data = this;
            Effect.m_icon = _AchievementIcons.Value is Toggle.On ? sprite : null;
            Effect.m_name = displayName;
            Effect.m_cooldown = duration * 1.3f; // guardian power cool down
            Effect.m_ttl = duration; // status effect cool down
            Effect.m_tooltip = _AchievementPowers.Value is Toggle.On ? effectTooltip : "";
            Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            Effect.m_stopMessageType = MessageHud.MessageType.TopLeft;
            Effect.m_startMessage = startMsg;
            Effect.m_stopMessage = stopMsg;
            Effect.m_activationAnimation = "gpower";
            if (startEffectNames is not null)
            {
                Effect.m_startEffects = CreateEffectList(startEffectNames.ToList(), effectName);
            }
            if (stopEffectNames is not null)
            {
                Effect.m_stopEffects = CreateEffectList(stopEffectNames.ToList(), effectName);
            }

            // Add base effect to ObjectDB
            obd.m_StatusEffects.Add(Effect);
            return Effect;
        }
        
        private static EffectList CreateEffectList(List<string> effects, string baseEffectName)
        {
            if (!ZNetScene.instance) return new EffectList();
            
            EffectList list = new();
            List<GameObject> validatedPrefabs = new();
            
            foreach (string effect in effects)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(effect);
                if (!prefab)
                {
                    AlmanacLogger.LogDebug( $"[{baseEffectName}]" + " : " + "Failed to find prefab: " + effect);
                    continue;
                }
                // 0 = Default
                // 9 = Character
                // 12 = Item
                // 15 = Static Solid
                // 22 = Weapon
                switch (prefab.layer)
                {
                    case 0:
                        prefab.TryGetComponent(out TimedDestruction timedDestruction);
                        prefab.TryGetComponent(out ParticleSystem particleSystem);
                        if (timedDestruction || particleSystem)
                        {
                            validatedPrefabs.Add(prefab);
                        }
                        break;
                    case 8 or 22:
                        validatedPrefabs.Add(prefab);
                        break;
                }
            }
            
            EffectList.EffectData[] allEffects = new EffectList.EffectData[validatedPrefabs.Count];

            for (int i = 0; i < validatedPrefabs.Count; ++i)
            {
                GameObject fx = validatedPrefabs[i];

                EffectList.EffectData effectData = new EffectList.EffectData()
                {
                    m_prefab = fx,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = true,
                    m_inheritParentRotation = true,
                    m_inheritParentScale = true,
                    m_scale = true,
                    m_childTransform = ""
                };
                allEffects[i] = effectData;
            }

            list.m_effectPrefabs = allEffects;

            return list;
        }
    }
    public class AchievementEffect : StatusEffect
    {
        public EffectData data = null!;

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.Attack, out float modifier)) return;
            hitData.ApplyModifier(modifier);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.HealthRegen, out float modifier)) return;
            regenMultiplier *= modifier;
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.StaminaRegen, out float modifier)) return;
            staminaRegen *= modifier;
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.RaiseSkills, out float raiseSkill)) return;
            value *= raiseSkill;
        }
        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.Speed, out float modifier)) return;
            speed *= modifier;
        }

        public override void ModifyNoise(float baseNoise, ref float noise)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.Noise, out float modifier)) return;
            noise *= modifier;
        }

        public override void ModifyStealth(float baseStealth, ref float stealth)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.Stealth, out float modifier)) return;
            stealth *= modifier;
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.MaxCarryWeight, out float maxCarry)) return;
            limit += maxCarry;
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.RunStaminaDrain, out float runStaminaDrain)) return;
            drain *= runStaminaDrain;
        }

        // public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
        // {
        //     if (!data.m_modifiers.TryGetValue(Modifier.RunStaminaDrain, out float runStaminaDrain)) return;
        //     staminaUse *= runStaminaDrain;
        // }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            if (!data.m_modifiers.TryGetValue(Modifier.DamageReduction, out float damageReduction)) return;
            hit.ApplyModifier(Mathf.Clamp01(1f - damageReduction));
        }
        public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers) => modifiers.Apply(data.damageMods);
        public override void ModifyFallDamage(float baseDamage, ref float damage)
        {
            if (m_character.GetSEMan().HaveStatusEffect("SlowFall".GetStableHashCode())) return;
            if (!data.m_modifiers.TryGetValue(Modifier.FallDamage, out float fallDamage)) return;
            damage = baseDamage * fallDamage;
            if (damage >= 0.0) return;
            damage = 0.0f;
        }
        public override void ModifyEitrRegen(ref float eitrRegen) => eitrRegen *= data.m_modifiers[Modifier.EitrRegen];
        public override void ModifySkillLevel(Skills.SkillType skill, ref float level)
        {
            if (data.m_skills.TryGetValue(Skills.SkillType.All, out float allModifier))
            {
                level += allModifier;
            }

            if (data.m_skills.TryGetValue(skill, out float amount))
            {
                level += amount;
            }
        }

        public override string GetTooltipString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(m_tooltip)) stringBuilder.AppendFormat("{0}\n", m_tooltip);
            foreach (var kvp in data.m_modifiers)
            {
                string key = Utility.ConvertEffectModifiers(kvp.Key);
                switch (kvp.Key)
                {
                    case Modifier.MaxCarryWeight or Modifier.Health or Modifier.Stamina or Modifier.Eitr or Modifier.Armor:
                        if (kvp.Value == 0f) continue;
                        stringBuilder.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", key, kvp.Value);
                        break;
                    case Modifier.DamageReduction:
                        if (kvp.Value == 0f) continue;
                        stringBuilder.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", key, Mathf.Clamp01(1f - kvp.Value) * 100f - 100);
                        break;
                    default:
                        if (Math.Abs(kvp.Value - 1f) < 0.01f) continue;
                        stringBuilder.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", key, kvp.Value * 100f - 100);
                        break;
                }
            }

            foreach (KeyValuePair<Skills.SkillType, float> kvp in data.m_skills)
            {
                if (kvp.Value == 0f) continue;
                stringBuilder.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", "$skill_" + kvp.Key.ToString().ToLower(), kvp.Value);
            }

            stringBuilder.AppendFormat("{0}\n", SE_Stats.GetDamageModifiersTooltipString(data.damageMods));

            return Localization.instance.Localize(stringBuilder.ToString());
        }

        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        private static class Character_RPC_Damage_Patch
        {
            private static void Postfix(HitData hit)
            {
                Character attacker = hit.GetAttacker();
                if (attacker == null) return;
                if (attacker is not Player player) return;
                float leech = 0f;
                foreach (StatusEffect? effect in player.GetSEMan().GetStatusEffects())
                {
                    if (effect is not AchievementEffect achievementEffect) continue;
                    if (achievementEffect.data.m_modifiers.TryGetValue(Modifier.LifeSteal, out float amount))
                    {
                        leech += amount - 1f;
                    }
                }
                if (leech > 1f) leech = 1f;
                if (leech > 0f)
                {
                    float total = hit.GetTotalDamage() * leech;
                    player.Heal(total);
                }
            }
        }
        [HarmonyPatch(typeof(Player), nameof(Player.SetMaxEitr))]
        private static class Player_SetMaxEitr_Patch
        {
            private static void Prefix(Player __instance, ref float eitr)
            {
                if (__instance != Player.m_localPlayer) return;
                foreach (var effect in __instance.GetSEMan().GetStatusEffects())
                {
                    if (effect is not AchievementEffect achievementEffect) continue;
                    if (achievementEffect.data.m_modifiers.TryGetValue(Modifier.Eitr, out float amount))
                    {
                        eitr += amount;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetMaxStamina))]
        private static class Player_SetMaxStamina_Patch
        {
            private static void Prefix(Player __instance, ref float stamina)
            {
                if (__instance != Player.m_localPlayer) return;
                foreach (var effect in __instance.GetSEMan().GetStatusEffects())
                {
                    if (effect is not AchievementEffect achievementEffect) continue;
                    if (achievementEffect.data.m_modifiers.TryGetValue(Modifier.Stamina, out float amount))
                    {
                        stamina += amount;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetMaxHealth))]
        private static class Player_SetMaxHealth_Patch
        {
            private static void Prefix(Player __instance, ref float health)
            {
                if (__instance != Player.m_localPlayer) return;
                foreach (var effect in __instance.GetSEMan().GetStatusEffects())
                {
                    if (effect is not AchievementEffect achievementEffect) continue;
                    if (achievementEffect.data.m_modifiers.TryGetValue(Modifier.Health, out float amount))
                    {
                        health += amount;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetBodyArmor))]
        private static class Player_GetBodyArmor_Patch
        {
            private static void Postfix(Player __instance, ref float __result)
            {
                float amount = 0f;
                foreach (StatusEffect? effect in __instance.GetSEMan().GetStatusEffects())
                {
                    if (effect is not AchievementEffect achievementEffect) continue;
                    if (achievementEffect.data.m_modifiers.TryGetValue(Modifier.Armor, out float value))
                    {
                        amount += value;
                    }
                }
                __result += amount;
            }
        }
    }
}
