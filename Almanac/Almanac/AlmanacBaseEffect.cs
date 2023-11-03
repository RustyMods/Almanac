using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac.Almanac;

public static class RegisterAlmanacEffects
{
    public static readonly List<AlmanacEffectsManager.BaseEffectData> effectsData = new();

    // [UsedImplicitly]
    // private static void OnInit()
    // {
    //     AchievementManager.serverAchievements.ValueChanged += UpdateEffects;
    // }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    static class ObjectDBAwakePatch
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.VeryLow)]
        private static void Postfix()
        {
            UpdateEffects();
        }
    }
    private static void UpdateEffects()
    {
        if (!ZNetScene.instance) return;
        // AlmanacPlugin.AlmanacLogger.LogWarning($"starting object db postfix to add {AchievementManager.tempAchievements.Count} effects");
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is AlmanacEffectsManager.BaseEffect);

        try
        {
            effectsData.Clear();
            foreach (var achievement in AchievementManager.tempAchievements)
            {
                effectsData.Add(achievement.m_statusEffect);
            }
                
            foreach (var effectData in effectsData)
            {
                effectData.Init();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
}

public static class AlmanacEffectsManager
{
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
        BaseHP,
        BaseStamina,
        MeleeDMG,
        RangedDMG,
        FireDMG,
        FrostDMG,
        LightningDMG,
        PoisonDMG,
        SpiritDMG,
        EikthyrPower,
        ElderPower,
        BonemassPower,
        ModerPower,
        YagluthPower,
        QueenPower
    }
    public class BaseEffectData
    {
        public string effectName = null!;
        public string displayName = null!;
        public int duration = 0;
        public Sprite? sprite;
        public string? spriteName;
        public string[]? startEffectNames;
        public string[]? stopEffectNames;
        public string? startMsg = "";
        public string? stopMsg = "";
        public string? effectTooltip;
        public string? damageMod;
        public Modifier Modifier;
        public float m_initialValue = 0f;
        public float m_newValue = 0f;
        public Dictionary<Modifier, float> Modifiers = new()
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
            { Modifier.FallDamage , 1f }
        };

        public readonly List<HitData.DamageModPair> damageMods = new();

        private bool? isValid;
        
        public void Init()
        {
            if (isValid.HasValue) return;

            if (!damageMod.IsNullOrWhiteSpace())
            {
                damageMods.Clear();
                
                string normalizedDamageMod = damageMod.Replace(" ", "");
                string[] split = normalizedDamageMod.Split('=');
                if (split.Length != 2) return;
                string damageType = split[0];
                string damageValue = split[1];

                HitData.DamageModPair pair = new()
                {
                    m_type = HitData.DamageType.Physical,
                    m_modifier = HitData.DamageModifier.Normal
                };

                switch (damageValue)
                {
                    case "Normal": break;
                    case "Resistant": pair.m_modifier = HitData.DamageModifier.Resistant; break;
                    case "Weak": pair.m_modifier = HitData.DamageModifier.Weak; break;
                    case "Immune": pair.m_modifier = HitData.DamageModifier.Immune; break;
                    case "Ignore": pair.m_modifier = HitData.DamageModifier.Ignore; break;
                    case "VeryResistant": pair.m_modifier = HitData.DamageModifier.VeryResistant; break;
                    case "VeryWeak": pair.m_modifier = HitData.DamageModifier.VeryWeak; break;
                }

                switch (damageType)
                {
                    case "Blunt": pair.m_type = HitData.DamageType.Blunt; break;
                    case "Slash": pair.m_type = HitData.DamageType.Slash; break;
                    case "Pierce": pair.m_type = HitData.DamageType.Pierce; break;
                    case "Chop": pair.m_type = HitData.DamageType.Chop; break;
                    case "Pickaxe": pair.m_type = HitData.DamageType.Pickaxe; break;
                    case "Fire": pair.m_type = HitData.DamageType.Fire; break;
                    case "Frost": pair.m_type = HitData.DamageType.Frost; break;
                    case "Lightning": pair.m_type = HitData.DamageType.Lightning; break;
                    case "Poison": pair.m_type = HitData.DamageType.Poison; break;
                    case "Spirit": pair.m_type = HitData.DamageType.Spirit; break;
                    case "Physical": pair.m_type = HitData.DamageType.Physical; break;
                    case "Elemental": pair.m_type = HitData.DamageType.Elemental; break;
                }

                damageMods.Add(pair);
            }
            
            Sprite? icon = AlmanacPlugin.AlmanacIconButton;
            if (sprite) icon = sprite;
            if (!sprite && !spriteName.IsNullOrWhiteSpace())
            {
                GameObject item = ZNetScene.instance.GetPrefab(spriteName);
                item.TryGetComponent(out ItemDrop itemDrop);
                if (itemDrop) icon = itemDrop.m_itemData.GetIcon();
            }
            
            ObjectDB obd = ObjectDB.instance;

            // Make sure new effects have unique names
            if (obd.m_StatusEffects.Find(effect => effect.name == effectName)) return;

            BaseEffect baseEffect = ScriptableObject.CreateInstance<BaseEffect>();
            baseEffect.name = effectName;
            baseEffect.data = this;
            baseEffect.m_icon = icon;
            baseEffect.m_name = displayName;
            baseEffect.m_ttl = duration;
            baseEffect.m_tooltip = effectTooltip;
            baseEffect.m_startMessageType = MessageHud.MessageType.Center;
            baseEffect.m_stopMessageType = MessageHud.MessageType.Center;
            baseEffect.m_startMessage = startMsg;
            baseEffect.m_stopMessage = stopMsg;
            if (startEffectNames is not null)
            {
                baseEffect.m_startEffects = CreateEffectList(ZNetScene.instance, startEffectNames.ToList());
            }
            
            if (stopEffectNames is not null)
            {
                baseEffect.m_stopEffects = CreateEffectList(ZNetScene.instance, stopEffectNames.ToList());
            }
            obd.m_StatusEffects.Add(baseEffect);
            isValid = true;
        }
        
        private static EffectList CreateEffectList(
            ZNetScene scene,
            List<string> effects)
        {
            EffectList list = new();
            EffectList.EffectData[] allEffects = new EffectList.EffectData[effects.Count];

            for (int i = 0; i < effects.Count; ++i)
            {
                if (effects[i].IsNullOrWhiteSpace()) continue;
                GameObject fx = scene.GetPrefab(effects[i]);
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
    public class BaseEffect : StatusEffect
    {
        public BaseEffectData data = null!;

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            hitData.ApplyModifier(data.Modifiers[Modifier.Attack]);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            regenMultiplier *= data.Modifiers[Modifier.HealthRegen];
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            staminaRegen *= data.Modifiers[Modifier.StaminaRegen];
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            value *= data.Modifiers[Modifier.RaiseSkills];
        }

        public override void ModifySpeed(float baseSpeed, ref float speed)
        {
            speed *= data.Modifiers[Modifier.Speed];
        }

        public override void ModifyNoise(float baseNoise, ref float noise)
        {
            noise *= data.Modifiers[Modifier.Noise];
        }

        public override void ModifyStealth(float baseStealth, ref float stealth)
        {
            stealth *= data.Modifiers[Modifier.Stealth];
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            limit += data.Modifiers[Modifier.MaxCarryWeight];
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain)
        {
            drain *= data.Modifiers[Modifier.RunStaminaDrain];
        }

        public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            staminaUse *= data.Modifiers[Modifier.RunStaminaDrain];
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            float mod = Mathf.Clamp01(1f - data.Modifiers[Modifier.DamageReduction]);
            hit.ApplyModifier(mod);
        }

        public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
        {
            modifiers.Apply(data.damageMods);
        }

        public override void ModifyFallDamage(float baseDamage, ref float damage)
        {
            damage = baseDamage * data.Modifiers[Modifier.FallDamage];
            if (damage >= 0.0) return;
            damage = 0.0f;
        }
    }
}