using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Almanac.Utilities;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Achievements;

public static class EffectMan
{
    private static readonly Dictionary<string, AchievementEffect> m_allEffects = new();
    public static readonly List<StatusEffect> ActiveAchievementEffects = new();
    public const string PlayerEffectKey = "AlmanacAchievements";
    public static bool IsActive(StatusEffect statusEffect) => ActiveAchievementEffects.Contains(statusEffect);
    public static int Count() => ActiveAchievementEffects.Count;
    public static bool Add(StatusEffect statusEffect, bool save = true)
    {
        if (!Player.m_localPlayer) return false;
        if (!Player.m_localPlayer.GetSEMan().AddStatusEffect(statusEffect)) return false;
        ActiveAchievementEffects.Add(statusEffect);
        if (save) OnChange();
        return true;
    }

    public static bool Remove(StatusEffect statusEffect, bool save = true)
    {
        if (!Player.m_localPlayer) return false;
        if (!Player.m_localPlayer.GetSEMan().RemoveStatusEffect(statusEffect)) return false;
        ActiveAchievementEffects.Remove(statusEffect);
        if(save) OnChange();
        return true;
    }

    public static void Clear(bool save = true)
    {
        if (!Player.m_localPlayer) return;
        var effects = Player.m_localPlayer.GetSEMan().GetStatusEffects().Where(x => x is AchievementEffect);
        foreach (var effect in effects) Player.m_localPlayer.GetSEMan().RemoveStatusEffect(effect);
        ActiveAchievementEffects.Clear();
        if (save) OnChange();
    }

    public static void Load()
    {
        if (!Player.m_localPlayer) return;
        if (!Player.m_localPlayer.m_customData.TryGetValue(PlayerEffectKey, out string data)) return;
        var deserializer = new DeserializerBuilder().Build();
        try
        {
            var names = deserializer.Deserialize<List<string>>(data);
            Clear(false);
            foreach (var name in names)
            {
                if (!m_allEffects.TryGetValue(name, out AchievementEffect statusEffect)) continue;
                Add(statusEffect, false);
            }
            OnChange();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to get saved achievement effects from player profile");
        }
    }

    private static void OnChange()
    {
        var serializer = new SerializerBuilder().Build();
        List<string> names = ActiveAchievementEffects.Select(x => x.name).ToList();
        Player.m_localPlayer.m_customData[PlayerEffectKey] = serializer.Serialize(names);
    }

    public static bool DeleteAll()
    {
        if (!ObjectDB.m_instance) return false;
        ObjectDB.m_instance.m_StatusEffects.RemoveAll(x => x is AchievementEffect);
        return true;
    }

    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.AddActiveEffects))]
    static class CompendiumAddActiveEffectsPatch
    {
        private static void Postfix(TextsDialog __instance)
        {
            if (AlmanacPlugin._AchievementIcons.Value is AlmanacPlugin.Toggle.On) return;
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
    public static class EffectData
    {
        public static StatusEffect? Init(AchievementManager.Achievement achievement)
        {
            if (ObjectDB.instance.m_StatusEffects.Find(effect => effect.name == achievement.m_data.unique_name))
                return null;
            achievement.m_data.damage_modifiers.RemoveAll(d => d.m_modifier is HitData.DamageModifier.Normal);
            AchievementEffect Effect = ScriptableObject.CreateInstance<AchievementEffect>();
            Effect.m_achievement = achievement;
            Effect.name = achievement.m_data.unique_name.Replace(" ", "");
            Effect.m_nameHash = Effect.name.GetStableHashCode();
            Effect.m_icon = AlmanacPlugin._AchievementIcons.Value is AlmanacPlugin.Toggle.On ? achievement.GetIcon() : null;
            Effect.m_name = achievement.GetDisplayName();
            Effect.m_cooldown = achievement.m_data.duration * 1.3f; // guardian power cool down
            Effect.m_ttl = achievement.m_data.duration; // status effect cool down
            Effect.m_tooltip = AlmanacPlugin._AchievementPowers.Value is AlmanacPlugin.Toggle.On ? achievement.m_data.tooltip : "";
            Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            Effect.m_stopMessageType = MessageHud.MessageType.TopLeft;
            Effect.m_startMessage = achievement.m_data.start_message;
            Effect.m_stopMessage = achievement.m_data.stop_message;
            Effect.m_activationAnimation = "gpower";
            Effect.m_category = achievement.m_data.achievement_group;
            if (achievement.m_data.start_effects.Count > 0)
            {
                Effect.m_startEffects =
                    CreateEffectList(achievement.m_data.start_effects, achievement.m_data.unique_name);
            }

            if (achievement.m_data.stop_effects.Count > 0)
            {
                Effect.m_stopEffects =
                    CreateEffectList(achievement.m_data.stop_effects, achievement.m_data.unique_name);
            }

            // Add base effect to ObjectDB
            ObjectDB.instance.m_StatusEffects.Add(Effect);
            m_allEffects[achievement.GetUniqueName()] = Effect;
            return Effect;
        }

        private static EffectList CreateEffectList(List<string> effects, string baseEffectName)
        {
            if (!ZNetScene.instance) return new EffectList();
            
            EffectList list = new();
            List<EffectList.EffectData> allEffects = new();
            foreach (string effect in effects)
            {
                if (ZNetScene.instance.GetPrefab(effect) is not {} prefab)
                {
                    AlmanacPlugin.AlmanacLogger.LogDebug( $"[{baseEffectName}]" + " : " + "Failed to find prefab: " + effect);
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
                        if (prefab.GetComponent<TimedDestruction>() || prefab.GetComponent<ParticleSystem>()) break;
                        continue;
                    case 8 or 22:
                        break;
                    default:
                        continue;
                }
                allEffects.Add(new EffectList.EffectData()
                {
                    m_prefab = prefab, m_attach = true, m_inheritParentRotation = true, m_inheritParentScale = true
                });
            }

            list.m_effectPrefabs = allEffects.ToArray();

            return list;
        }
    }
    public class AchievementEffect : StatusEffect
    {
        public AchievementManager.Achievement m_achievement = null!;

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.Attack, out float modifier)) return;
            hitData.ApplyModifier(modifier);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.HealthRegen, out float modifier)) return;
            regenMultiplier *= modifier;
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.StaminaRegen, out float modifier)) return;
            staminaRegen *= modifier;
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.RaiseSkills, out float raiseSkill)) return;
            value *= raiseSkill;
        }
        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.Speed, out float modifier)) return;
            speed *= modifier;
        }

        public override void ModifyNoise(float baseNoise, ref float noise)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.Noise, out float modifier)) return;
            noise *= modifier;
        }

        public override void ModifyStealth(float baseStealth, ref float stealth)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.Stealth, out float modifier)) return;
            stealth *= modifier;
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.MaxCarryWeight, out float maxCarry)) return;
            limit += maxCarry;
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain, Vector3 dir)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.RunStaminaDrain, out float runStaminaDrain)) return;
            drain *= runStaminaDrain;
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.DamageReduction, out float damageReduction)) return;
            hit.ApplyModifier(Mathf.Clamp01(1f - damageReduction));
        }
        public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers) => modifiers.Apply(m_achievement.m_data.damage_modifiers);
        public override void ModifyFallDamage(float baseDamage, ref float damage)
        {
            if (m_character.GetSEMan().HaveStatusEffect("SlowFall".GetStableHashCode())) return;
            if (!m_achievement.m_data.modifiers.TryGetValue(Modifier.FallDamage, out float fallDamage)) return;
            damage = baseDamage * fallDamage;
            if (damage >= 0.0) return;
            damage = 0.0f;
        }
        public override void ModifyEitrRegen(ref float eitrRegen) => eitrRegen *= m_achievement.m_data.modifiers[Modifier.EitrRegen];
        public override void ModifySkillLevel(Skills.SkillType skill, ref float level)
        {
            if (m_achievement.m_skillBonus.TryGetValue(Skills.SkillType.All, out float allModifier))
            {
                level += allModifier;
            }

            if (m_achievement.m_skillBonus.TryGetValue(skill, out float amount))
            {
                level += amount;
            }
        }

        public override string GetTooltipString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(m_tooltip)) stringBuilder.AppendFormat("{0}\n", m_tooltip);
            foreach (var kvp in m_achievement.m_data.modifiers)
            {
                string key = Helpers.ConvertEffectModifiers(kvp.Key);
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

            foreach (KeyValuePair<Skills.SkillType, float> kvp in m_achievement.m_skillBonus)
            {
                if (kvp.Value == 0f) continue;
                stringBuilder.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", "$skill_" + kvp.Key.ToString().ToLower(), kvp.Value);
            }

            stringBuilder.AppendFormat("{0}\n", SE_Stats.GetDamageModifiersTooltipString(m_achievement.m_data.damage_modifiers));

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
                    if (achievementEffect.m_achievement.m_data.modifiers.TryGetValue(Modifier.LifeSteal, out float amount))
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
                    if (achievementEffect.m_achievement.m_data.modifiers.TryGetValue(Modifier.Eitr, out float amount))
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
                    if (achievementEffect.m_achievement.m_data.modifiers.TryGetValue(Modifier.Stamina, out float amount))
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
                    if (achievementEffect.m_achievement.m_data.modifiers.TryGetValue(Modifier.Health, out float amount))
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
                    if (achievementEffect.m_achievement.m_data.modifiers.TryGetValue(Modifier.Armor, out float value))
                    {
                        amount += value;
                    }
                }
                __result += amount;
            }
        }
    }
}
