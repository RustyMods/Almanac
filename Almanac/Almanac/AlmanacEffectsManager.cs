using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Almanac.Almanac;

public static class RegisterAlmanacEffects
{
    public static List<AlmanacEffectsManager.BaseEffectData> effectsData = new()
    {
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_gluttony",
            displayName = "$almanac_achievement_gluttony",
            sprite = AlmanacPlugin.mushroomBigRedIcon,
            startMsg = "$almanac_modify_poison_resistance <color=orange>$almanac_resistant</color>",
            stopMsg = "$almanac_modify_poison_default",
            effectTooltip = "$almanac_modify_poison_resistance <color=orange>$almanac_resistant</color>",
            damageMod = "Poison = Resistant",
            startEffectNames = new []{"blob_aoe", "sfx_GuckSackDestroyed"}
        }, 
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_knowledgeable",
            displayName = "Knowledge",
            sprite = AlmanacPlugin.necklaceSilverRed,
            Modifier = AlmanacEffectsManager.Modifier.MaxCarryWeight,
            Modifiers =
            {
                [AlmanacEffectsManager.Modifier.MaxCarryWeight] = 100f
            },
            startMsg = "$almanac_increase_carry_weight_by <color=orange>100</color>",
            stopMsg = "$almanac_carry_weight_default",
            effectTooltip = "$almanac_increase_carry_weight_by <color=orange>100</color>"
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_meadow_kill",
            displayName = "$almanac_achievement_meadow_kill",
            startMsg = "$almanac_increase_health_by <color=orange>5</color>",
            stopMsg = "$almanac_health_default",
            spriteName = "HardAntler",
            effectTooltip = "$almanac_increase_health_by <color=orange>5</color>",
            startEffectNames = new []{"fx_DvergerMage_Support_start"},
            stopEffectNames = new []{"fx_DvergerMage_Mistile_die"},
            Modifier = AlmanacEffectsManager.Modifier.BaseHP,
            m_initialValue = 25f,
            m_newValue = 5f
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_blackforest_kill",
            displayName = "$almanac_achievement_blackforest_kill",
            startMsg = "$almanac_increase_health_by <color=orange>10</color>",
            stopMsg = "$almanac_health_default",
            effectTooltip = "$almanac_increase_health_by <color=orange>10</color>",
            Modifier = AlmanacEffectsManager.Modifier.BaseHP,
            m_initialValue = 25f,
            m_newValue = 10f,
            sprite = AlmanacPlugin.boneWhiteIcon
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_swamp_kill",
            displayName = "$almanac_achievement_swamp_kill",
            startMsg = "$almanac_increase_health_by <color=orange>15</color>",
            stopMsg = "$almanac_health_default",
            effectTooltip = "$almanac_increase_health_by <color=orange>15</color>",
            Modifier = AlmanacEffectsManager.Modifier.BaseHP,
            m_initialValue = 25f,
            m_newValue = 15f,
            spriteName = "TrophyAbomination"
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_mountain_kill",
            displayName = "$almanac_achievement_mountain_kill",
            startMsg = "$almanac_increase_health_by <color=orange>20</color>",
            effectTooltip = "$almanac_increase_health_by <color=orange>20</color>",
            stopMsg = "$almanac_health_defeault",
            Modifier = AlmanacEffectsManager.Modifier.BaseHP,
            m_initialValue = 25f,
            m_newValue = 20f,
            spriteName = "DragonTear"
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_ranger",
            displayName = "$almanac_achievement_ranger",
            startMsg = "$almanac_increase_projectile_damage_by <color=orange>10</color>",
            stopMsg = "$almanac_projectile_default",
            effectTooltip = "$almanac_increase_projectile_damage_by <color=orange>10</color>",
            sprite = AlmanacPlugin.capeHoodIcon,
            Modifier = AlmanacEffectsManager.Modifier.RangedDMG,
            m_newValue = 10f,
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_brew_master",
            displayName = "$almanac_achievement_brew_master",
            startMsg = "$almanac_increase_fire_damage_by <color=orange>10</color>",
            effectTooltip = "$almanac_increase_fire_damage_by <color=orange>10</color>",
            stopMsg = "$almanac_damage_default",
            sprite = AlmanacPlugin.bottleStandardBlueIcon,
            Modifier = AlmanacEffectsManager.Modifier.FireDMG,
            m_newValue = 10f
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_master_archer",
            displayName = "$almanac_achievement_master_archer",
            startMsg = "$almanac_increase_projectile_damage_by <color=orange>15</color>",
            effectTooltip = "$almanac_increase_projectile_damage_by <color=orange>15</color>",
            stopMsg = "$almanac_projectile_default",
            sprite = AlmanacPlugin.arrowBasicIcon,
            Modifier = AlmanacEffectsManager.Modifier.RangedDMG,
            m_newValue = 15f
        },
        new AlmanacEffectsManager.BaseEffectData()
        {
            effectName = "se_undying",
            displayName = "$almanac_achievement_undying",
            startMsg = "$almanac_increase_stamina_by <color=orange>25</color>",
            effectTooltip = "$almanac_increase_stamina_by <color=orange>25</color>",
            stopMsg = "$almanac_stamina_default",
            sprite = AlmanacPlugin.boneSkullIcon,
            Modifier = AlmanacEffectsManager.Modifier.BaseStamina,
            m_initialValue = 50f,
            m_newValue = 25f
        }
    };

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    static class ObjectDBAwakePatch
    {
        private static void Postfix()
        {
            if (!ZNetScene.instance) return;
            ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is AlmanacEffectsManager.BaseEffect);

            try
            {
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
        SpiritDMG
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
        public string startMsg = "";
        public string stopMsg = "";
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
            { Modifier.MaxCarryWeight , 1f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 1f }
        };

        public List<HitData.DamageModPair> damageMods = new();

        private bool? isValid;
        
        public void Init()
        {
            if (isValid.HasValue) return;
            
            Sprite? icon = AlmanacPlugin.AlmanacIconButton;

            if (!damageMod.IsNullOrWhiteSpace())
            {
                damageMods.Clear();
                
                damageMod = damageMod.Replace(" ", "");
                string[] split = damageMod.Split('=');
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
                    case "Normal":
                        break;
                    case "Resistant":
                        pair.m_modifier = HitData.DamageModifier.Resistant;
                        break;
                    case "Weak":
                        pair.m_modifier = HitData.DamageModifier.Weak;
                        break;
                    case "Immune":
                        pair.m_modifier = HitData.DamageModifier.Immune;
                        break;
                    case "Ignore":
                        pair.m_modifier = HitData.DamageModifier.Ignore;
                        break;
                    case "VeryResistant":
                        pair.m_modifier = HitData.DamageModifier.VeryResistant;
                        break;
                    case "VeryWeak":
                        pair.m_modifier = HitData.DamageModifier.VeryWeak;
                        break;
                }

                switch (damageType)
                {
                    case "Blunt":
                        pair.m_type = HitData.DamageType.Blunt;
                        break;
                    case "Slash":
                        pair.m_type = HitData.DamageType.Slash;
                        break;
                    case "Pierce":
                        pair.m_type = HitData.DamageType.Pierce;
                        break;
                    case "Chop":
                        pair.m_type = HitData.DamageType.Chop;
                        break;
                    case "Pickaxe":
                        pair.m_type = HitData.DamageType.Pickaxe;
                        break;
                    case "Fire":
                        pair.m_type = HitData.DamageType.Fire;
                        break;
                    case "Frost":
                        pair.m_type = HitData.DamageType.Frost;
                        break;
                    case "Lightning":
                        pair.m_type = HitData.DamageType.Lightning;
                        break;
                    case "Poison":
                        pair.m_type = HitData.DamageType.Poison;
                        break;
                    case "Spirit":
                        pair.m_type = HitData.DamageType.Spirit;
                        break;
                    case "Physical":
                        pair.m_type = HitData.DamageType.Physical;
                        break;
                    case "Elemental":
                        pair.m_type = HitData.DamageType.Elemental;
                        break;
                }

                damageMods.Add(pair);
            }

            if (sprite) icon = sprite;
            if (!spriteName.IsNullOrWhiteSpace())
            {
                GameObject item = ZNetScene.instance.GetPrefab(spriteName);
                item.TryGetComponent(out ItemDrop itemDrop);
                if (itemDrop) icon = itemDrop.m_itemData.GetIcon();
            }

            
            ObjectDB obd = ObjectDB.instance;

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
                // GameObject startEffect = ZNetScene.instance.GetPrefab(startEffectName);
                //
                // baseEffect.m_startEffects = new EffectList()
                // {
                //     m_effectPrefabs = new[]
                //     {
                //         new EffectList.EffectData()
                //         {
                //             m_prefab = startEffect,
                //             m_attach = true,
                //             m_enabled = true,
                //             m_inheritParentRotation = true,
                //             m_inheritParentScale = true,
                //             m_randomRotation = false,
                //             m_scale = true
                //         }
                //     }
                // };
            }
            
            if (stopEffectNames is not null)
            {
                baseEffect.m_stopEffects = CreateEffectList(ZNetScene.instance, stopEffectNames.ToList());
                // GameObject stopEffect = ZNetScene.instance.GetPrefab(stopEffectName);
                // baseEffect.m_stopEffects = new EffectList()
                // {
                //     m_effectPrefabs = new[]
                //     {
                //         new EffectList.EffectData()
                //         {
                //             m_attach = true,
                //             m_enabled = true,
                //             m_inheritParentRotation = true,
                //             m_inheritParentScale = true,
                //             m_prefab = stopEffect,
                //             m_randomRotation = false,
                //             m_scale = true
                //         }
                //     }
                // };
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