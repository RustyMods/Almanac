using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using static Almanac.Almanac.AchievementManager;
using static Almanac.Almanac.AlmanacEffectsManager;
using static Almanac.AlmanacPlugin;

namespace Almanac.Almanac;

public static class RegisterAlmanacEffects
{
    public static readonly List<BaseEffectData> effectsData = new();
    
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    static class ObjectDBAwakePatch
    { 
        private static void Postfix() => AddStatusEffectsToObjectDB();
    }
    public static void AddStatusEffectsToObjectDB()
    {
        if (!ZNetScene.instance) return;
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is BaseEffect);
        
        if (tempAchievements.Count == 0) return;
        
        effectsData.Clear();
        foreach (Achievement achievement in tempAchievements) effectsData.Add(achievement.m_statusEffect);
        foreach (BaseEffectData effectData in effectsData)
        {
            if (effectData.effectName 
                is "GP_Eikthyr" 
                or "GP_TheElder" 
                or "GP_Bonemass" 
                or "GP_Moder" 
                or "GP_Yagluth"
                ) continue;
            
            effectData.Init();
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
        BaseEitr,
        MeleeDMG,
        RangedDMG,
        FireDMG,
        FrostDMG,
        LightningDMG,
        PoisonDMG,
        SpiritDMG,
        ChopDMG,
        PickaxeDMG,
        BluntDMG,
        PierceDMG,
        SlashDMG,
        EikthyrPower,
        ElderPower,
        BonemassPower,
        ModerPower,
        YagluthPower,
        QueenPower,
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
        public float m_newValue = 0f;
        public string? activationAnimation = "gpower";
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
        
        public void Init()
        {
            ObjectDB obd = ObjectDB.instance;

            // Make sure new effects have unique names
            if (obd.m_StatusEffects.Find(effect => effect.name == effectName)) return;

            if (!damageMod.IsNullOrWhiteSpace())
            {
                string normalizedDamageMod = damageMod!.Replace(" ", "");
                string[] resistanceMods = normalizedDamageMod.Split(',');
                foreach (string resistance in resistanceMods)
                {
                    string[] split = resistance.Split('=');
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

                    this.damageMods.Add(pair);
                }
            }
            
            Sprite? icon = AlmanacIconButton;
            if (sprite) icon = sprite;
            if (!sprite && !spriteName.IsNullOrWhiteSpace())
            {
                GameObject item = ZNetScene.instance.GetPrefab(spriteName);
                if (!item)
                {
                    AlmanacLogger.LogWarning($"[{effectName}] : Failed to get prefab: {spriteName}");
                }
                else
                {
                    item.TryGetComponent(out ItemDrop itemDrop);
                    if (itemDrop) icon = itemDrop.m_itemData.GetIcon();
                }
            }

            BaseEffect baseEffect = ScriptableObject.CreateInstance<BaseEffect>();
            baseEffect.name = effectName;
            baseEffect.data = this;
            baseEffect.m_icon = icon;
            baseEffect.m_name = displayName;
            baseEffect.m_cooldown = duration + 60; // guardian power cool down
            baseEffect.m_ttl = duration; // status effect cool down
            baseEffect.m_tooltip = effectTooltip;
            baseEffect.m_startMessageType = MessageHud.MessageType.Center;
            baseEffect.m_stopMessageType = MessageHud.MessageType.Center;
            baseEffect.m_startMessage = startMsg;
            baseEffect.m_stopMessage = stopMsg;
            baseEffect.m_activationAnimation = activationAnimation;
            if (startEffectNames is not null)
            {
                baseEffect.m_startEffects = CreateEffectList(ZNetScene.instance, startEffectNames.ToList(), effectName);
            }
            
            if (stopEffectNames is not null)
            {
                baseEffect.m_stopEffects = CreateEffectList(ZNetScene.instance, stopEffectNames.ToList(), effectName);
            }
            
            // Add base effect to ObjectDB
            obd.m_StatusEffects.Add(baseEffect);
        }
        
        private static EffectList CreateEffectList(
            ZNetScene scene,
            List<string> effects,
            string baseEffectName)
        {
            EffectList list = new();
            List<GameObject> validatedPrefabs = new();
            
            foreach (string effect in effects)
            {
                GameObject prefab = scene.GetPrefab(effect);
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
                            continue;
                        }
                        
                        AlmanacLogger.LogInfo($"[{baseEffectName}] Failed to create effect: " + prefab.name + " is invalid layer type " + prefab.layer);
                        break;
                    case 8 or 22:
                        validatedPrefabs.Add(prefab);
                        break;
                    default:
                        AlmanacLogger.LogInfo($"[{baseEffectName}] Failed to create effect: " + prefab.name + " is invalid layer type " + prefab.layer);
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