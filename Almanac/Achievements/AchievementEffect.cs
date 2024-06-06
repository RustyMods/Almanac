using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            
            string texts = __instance.m_texts[0].m_text;

            string addedText = "";

            foreach (StatusEffect effect in ActiveAchievementEffects)
            {
                string text = $"\n<color=orange>{effect.m_name}</color>{effect.m_tooltip}\n";
                addedText += text;
            }

            if (ActiveAchievementEffects.Count > 0)
            {
                string appendedText = addedText + "\n\n" + texts;

                __instance.m_texts[0].m_text = appendedText;
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
        EitrRegen
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
        public Dictionary<Modifier, float> Modifiers = new();

        private readonly Dictionary<Modifier, float> defaultModifiers = new()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen , 1f },
            { Modifier.StaminaRegen , 1f },
            { Modifier.RaiseSkills , 1f },
            { Modifier.Speed , 1f },
            { Modifier.Noise , 1f },
            { Modifier.MaxCarryWeight , 0f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 1f },
            { Modifier.EitrRegen , 1f }
        };
        
        public StatusEffect? Init()
        {
            ObjectDB obd = ObjectDB.instance;

            // Make sure new effects have unique names
            if (obd.m_StatusEffects.Find(effect => effect.name == effectName)) return null;

            string appendedTooltip = effectTooltip ?? "";

            List<HitData.DamageModPair> ValidatedDamageMods = new();

            foreach (HitData.DamageModPair mod in damageMods)
            {
                if (mod.m_modifier == HitData.DamageModifier.Normal) continue;

                string formattedModifier = Localization.instance.Localize(Utility.ConvertDamageModifiers(mod.m_modifier));
                string formattedModType = Localization.instance.Localize(Utility.ConvertDamageTypes(mod.m_type));
                
                string tooltip = $"\n<color=orange>{formattedModifier}</color> VS <color=orange>{formattedModType}</color>";
                appendedTooltip += tooltip;
                
                ValidatedDamageMods.Add(mod);
            }

            damageMods = ValidatedDamageMods;

            foreach (KeyValuePair<Modifier, float> mod in Modifiers)
            {
                if (Math.Abs(mod.Value - defaultModifiers[mod.Key]) < 0.009f) continue;

                string FormattedKey = Localization.instance.Localize(Utility.ConvertEffectModifiers(mod.Key));
                
                switch (mod.Key)
                {
                    case Modifier.None:
                        break;
                    case Modifier.DamageReduction:
                        appendedTooltip += $"\n{FormattedKey} -<color=orange>{Mathf.Clamp01(mod.Value) * 100}</color>%";
                        break;
                    case Modifier.MaxCarryWeight:
                        if (mod.Value < 0)
                        {
                            appendedTooltip += $"\n{FormattedKey} -<color=orange>{mod.Value.ToString(CultureInfo.CurrentCulture).Replace("-","")}</color>";
                        }
                        else
                        {
                            appendedTooltip += $"\n{FormattedKey} +<color=orange>{mod.Value}</color>";;
                        }
                        break;
                    default:
                        if (mod.Value > 1)
                        {
                            appendedTooltip += $"\n{FormattedKey} +<color=orange>{Mathf.Round((mod.Value - 1) * 100)}</color>%";
                        }
                        if (mod.Value < 1)
                        {
                            appendedTooltip += $"\n{FormattedKey} -<color=orange>{Mathf.Round((1 - mod.Value) * 100)}</color>%";
                        }
                        break;
                }
            }
            AchievementEffect Effect = ScriptableObject.CreateInstance<AchievementEffect>();
            Effect.name = effectName;
            Effect.data = this;
            Effect.m_icon = _AchievementIcons.Value is Toggle.On ? sprite : null;
            Effect.m_name = displayName;
            Effect.m_cooldown = duration * 1.3f; // guardian power cool down
            Effect.m_ttl = duration; // status effect cool down
            Effect.m_tooltip = _AchievementPowers.Value is Toggle.On ? appendedTooltip : "";
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
        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData) => hitData.ApplyModifier(data.Modifiers[Modifier.Attack]);
        public override void ModifyHealthRegen(ref float regenMultiplier) => regenMultiplier *= data.Modifiers[Modifier.HealthRegen];
        public override void ModifyStaminaRegen(ref float staminaRegen) => staminaRegen *= data.Modifiers[Modifier.StaminaRegen];
        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value) => value *= data.Modifiers[Modifier.RaiseSkills];
        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            speed *= data.Modifiers[Modifier.Speed];
        }

        public override void ModifyNoise(float baseNoise, ref float noise) => noise *= data.Modifiers[Modifier.Noise];
        public override void ModifyStealth(float baseStealth, ref float stealth) => stealth *= data.Modifiers[Modifier.Stealth];
        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit) => limit += data.Modifiers[Modifier.MaxCarryWeight];
        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain) => drain *= data.Modifiers[Modifier.RunStaminaDrain];
        public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse) => staminaUse *= data.Modifiers[Modifier.RunStaminaDrain];
        public override void OnDamaged(HitData hit, Character attacker) => hit.ApplyModifier(Mathf.Clamp01(1f - data.Modifiers[Modifier.DamageReduction]));
        public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers) => modifiers.Apply(data.damageMods);
        public override void ModifyFallDamage(float baseDamage, ref float damage)
        {
            if (m_character.GetSEMan().HaveStatusEffect("SlowFall".GetStableHashCode())) return;
            damage = baseDamage * data.Modifiers[Modifier.FallDamage];
            if (damage >= 0.0) return;
            damage = 0.0f;
        }
        public override void ModifyEitrRegen(ref float eitrRegen) => eitrRegen *= data.Modifiers[Modifier.EitrRegen];
    }
}
