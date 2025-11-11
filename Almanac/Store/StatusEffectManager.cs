using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Almanac.Data;
using Almanac.Managers;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Store;

public static class CustomEffectManager
{
    public static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static readonly CustomSyncedValue<string> SyncedEffectData = new(AlmanacPlugin.ConfigSync, "ServerSynced_Almanac_CustomEffect_Data", "");
    public static Dictionary<string, CustomEffect.Data> effects = new();
    private static readonly Dictionary<string, CustomEffect.Data> fileEffects = new();
    private static readonly Dictionary<CustomEffect.Data, CustomEffect> registeredEffects = new();
    public static readonly AlmanacDir CustomEffectDir = new (AlmanacPlugin.AlmanacDir.Path, "CustomEffects");

    private static void Initialize()
    {
        foreach (CustomEffect.Data? data in effects.Values)
        {
            CustomEffect se = ScriptableObject.CreateInstance<CustomEffect>();
            se.name = data.UniqueID;
            se.m_nameHash = data.UniqueID.GetStableHashCode();
            se.data = data;
            se.Load();
            ObjectDB.instance.m_StatusEffects.Add(se);
            registeredEffects[data] = se;
        }
    }
    public static void Setup()
    {
        AlmanacPlugin.OnObjectDBAwake += Initialize;
        AlmanacPlugin.OnZNetAwake += UpdateServerEffects;
        LoadDefaults();
        string[] files = CustomEffectDir.GetFiles("*.yml", true);
        if (files.Length == 0)
        {
            foreach (CustomEffect.Data effectData in effects.Values)
            {
                string data = serializer.Serialize(effectData);
                string fileName = effectData.UniqueID + ".yml";
                var path = CustomEffectDir.WriteFile(fileName, data);
                fileEffects[path] = effectData;
                effectData.Path = path;
            }
        }
        else
        {
            effects.Clear();
            foreach (string file in files)
            {
                CustomEffect.Data data = deserializer.Deserialize<CustomEffect.Data>(File.ReadAllText(file));
                effects[data.UniqueID] = data;
                fileEffects[file] = data;
                data.Path = file;
            }
        }

        SyncedEffectData.ValueChanged += OnServerEffectsChanged;
        FileSystemWatcher watcher = new FileSystemWatcher(CustomEffectDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
    }

    private static void UpdateServerEffects()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedEffectData.Value = serializer.Serialize(effects);
    }
    private static void OnServerEffectsChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedEffectData.Value)) return;
        Dictionary<string, CustomEffect.Data> data = deserializer.Deserialize<Dictionary<string, CustomEffect.Data>>(SyncedEffectData.Value);
        effects = data;
    }
    private static void ReloadPage()
    {
        if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.StatusEffects].IsSelected ?? false)
        {
            AlmanacPanel.instance.OnStatusEffectTab();
        }
    }
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (!fileEffects.TryGetValue(e.FullPath, out var effect)) return;
            CustomEffect.Data data = deserializer.Deserialize<CustomEffect.Data>(File.ReadAllText(e.FullPath));
            effect.CopyFrom(data);
            if (ObjectDB.instance?.GetStatusEffect(data.UniqueID.GetStableHashCode()) is CustomEffect ce)
            {
                ce.data = data;
                ce.Load();
            }
            ReloadPage();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change custom effect: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!fileEffects.TryGetValue(e.FullPath, out CustomEffect.Data? effect)) return;
        if (!registeredEffects.TryGetValue(effect, out CustomEffect? se)) return;
        ObjectDB.instance?.m_StatusEffects.Remove(se);
        effects.Remove(effect.UniqueID);
        fileEffects.Remove(e.FullPath);
        ReloadPage();
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            CustomEffect.Data data = deserializer.Deserialize<CustomEffect.Data>(File.ReadAllText(e.FullPath));
            effects[data.UniqueID] = data;
            fileEffects[e.FullPath] = data;
            CustomEffect? se = ScriptableObject.CreateInstance<CustomEffect>();
            se.name = data.UniqueID;
            se.data = data;
            se.Load();
            se.m_nameHash = data.UniqueID.GetStableHashCode();
            ObjectDB.instance?.m_StatusEffects.Add(se);
            registeredEffects[data] = se;
            ReloadPage();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create custom effect: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void LoadDefaults()
    {
        CustomEffect.Data MinorHealth = new CustomEffect.Data();
        MinorHealth.UniqueID = "CE_MinorHealth";
        MinorHealth.Name = "Minor Health";
        MinorHealth.Modifiers[CEVars.Health] = 50f;
        MinorHealth.Duration = 1800f;
        MinorHealth.Icon = "mushroom";
        MinorHealth.Tooltip = "Increases health temporarily";
        effects[MinorHealth.UniqueID] = MinorHealth;

        CustomEffect.Data MinorStamina = new CustomEffect.Data();
        MinorStamina.UniqueID = "CE_MinorStamina";
        MinorStamina.Name = "Minor Stamina";
        MinorStamina.Modifiers[CEVars.Stamina] = 50f;
        MinorStamina.Duration = 1800f;
        MinorStamina.Icon = "bottle_blue";
        MinorStamina.Tooltip = "Increases stamina temporarily";
        effects[MinorStamina.UniqueID] = MinorStamina;
        
        CustomEffect.Data MinorEitr = new CustomEffect.Data();
        MinorEitr.UniqueID = "CE_MinorEitr";
        MinorEitr.Name = "Minor Eitr";
        MinorEitr.Modifiers[CEVars.Eitr] = 50f;
        MinorEitr.Duration = 1800f;
        MinorEitr.Icon = "necklace";
        MinorEitr.Tooltip = "Increases Eitr temporarily";
        effects[MinorEitr.UniqueID] = MinorEitr;
        
        CustomEffect.Data MinorArmor = new CustomEffect.Data();
        MinorArmor.UniqueID = "CE_MinorArmor";
        MinorArmor.Name = "Minor Armor";
        MinorArmor.Modifiers[CEVars.Armor] = 10f;
        MinorArmor.Duration = 1800f;
        MinorArmor.Icon = "shield";
        MinorArmor.Tooltip = "Increases armor temporarily";
        effects[MinorArmor.UniqueID] = MinorArmor;
        
        CustomEffect.Data LifeSteal = new CustomEffect.Data();
        LifeSteal.UniqueID = "CE_LifeSteal";
        LifeSteal.Name = "Life Steal";
        LifeSteal.Tooltip = "On hit, leeches enemy health";
        LifeSteal.Modifiers[CEVars.LifeSteal] = 1.1f; // 10%
        LifeSteal.Duration = 1800f;
        LifeSteal.Icon = "skull";
        effects[LifeSteal.UniqueID] = LifeSteal;
        
        CustomEffect.Data BluntReduction = new CustomEffect.Data();
        BluntReduction.UniqueID = "CE_BluntReduction";
        BluntReduction.Name = "Blunt Reduction";
        BluntReduction.Tooltip = "Reduces blunt damage";
        BluntReduction.Modifiers[CEVars.BluntResistance] = 0.1f;
        BluntReduction.Duration = 1800f;
        BluntReduction.Icon = "shield";
        effects[BluntReduction.UniqueID] = BluntReduction;
        
        CustomEffect.Data Mule = new CustomEffect.Data();
        Mule.UniqueID = "CE_Mule";
        Mule.Name = "Mule";
        Mule.Tooltip = "Increase carry capacity";
        Mule.Modifiers[CEVars.CarryWeight] = 100f;
        Mule.Duration = 1800f;
        Mule.Icon = "log_stack";
        effects[Mule.UniqueID] = Mule;
        
        CustomEffect.Data speed = new  CustomEffect.Data();
        speed.UniqueID = "CE_Speed";
        speed.Name = "Runner";
        speed.Tooltip = "Increase movement speed and stamina regen";
        speed.Duration = 1000f;
        speed.Icon = "bottle_blue";
        speed.Modifiers[CEVars.Speed] = 0.25f;
        speed.Modifiers[CEVars.StaminaRegenModifier] = 1.1f;
        effects[speed.UniqueID] = speed;

        CustomEffect.Data damageMod = new();
        damageMod.UniqueID = "CE_DamageMod";
        damageMod.Name = "Berserker";
        damageMod.Duration = 1800f;
        damageMod.Icon = "sword_brown";
        damageMod.Tooltip = "Increase damage output";
        damageMod.Modifiers[CEVars.DamageModifier] = 1.1f;
        effects[damageMod.UniqueID] = damageMod;

        CustomEffect.Data damageReduction = new();
        damageReduction.UniqueID = "CE_DamageReduction";
        damageReduction.Name = "Fortify";
        damageReduction.Duration = 1800f;
        damageReduction.Icon = "shield";
        damageReduction.Tooltip = "Reduce incoming damage";
        damageReduction.Modifiers[CEVars.DamageReduction] = 0.1f;
        effects[damageReduction.UniqueID] = damageReduction;

        CustomEffect.Data blunt = new();
        blunt.UniqueID = "CE_Blunt";
        blunt.Name = "Brute";
        blunt.Duration = 1800f;
        blunt.Icon = "MaceBronze";
        blunt.Tooltip = "Increases blunt damage";
        blunt.Modifiers[CEVars.BluntDamage] = 0.1f;
        effects[blunt.UniqueID] = blunt;
    }
    
    public static bool Exists(string name) => ObjectDB.instance.GetStatusEffect(name.GetStableHashCode()) != null;

    public static CustomEffect.Data? GetData(string id) => effects.TryGetValue(id, out var data) ? data : null;

    public static void ApplyModifier(this HitData hit, HitData.DamageType type, float multiplier)
    {
        switch (type)
        {
            case  HitData.DamageType.Blunt:
                hit.m_damage.m_blunt *= multiplier;
                break;
            case  HitData.DamageType.Slash:
                hit.m_damage.m_slash *= multiplier;
                break;
            case  HitData.DamageType.Pierce:
                hit.m_damage.m_pierce *= multiplier;
                break;
            case HitData.DamageType.Chop:
                hit.m_damage.m_chop *= multiplier;
                break;
            case HitData.DamageType.Pickaxe:
                hit.m_damage.m_pickaxe *= multiplier;
                break;
            case HitData.DamageType.Fire:
                hit.m_damage.m_fire *= multiplier;
                break;
            case HitData.DamageType.Frost:
                hit.m_damage.m_frost *= multiplier;
                break;
            case HitData.DamageType.Lightning:
                hit.m_damage.m_lightning *= multiplier;
                break;
            case HitData.DamageType.Poison:
                hit.m_damage.m_poison *= multiplier;
                break;
            case HitData.DamageType.Spirit:
                hit.m_damage.m_spirit *= multiplier;
                break;
        }
    }
}
public class CustomEffect : SE_Stats
{
    public Data? data;
    public float m_damageReduction;
    public float m_health;
    public float m_stamina;
    public float m_eitr;
    public float m_lifeSteal;
    public float m_armor;
    public float m_bluntResistance;
    public float m_slashResistance;
    public float m_pierceResistance;
    public float m_chopResistance;
    public float m_pickaxeResistance;
    public float m_fireResistance;
    public float m_frostResistance;
    public float m_lightningResistance;
    public float m_poisonResistance;
    public float m_spiritResistance;

    public void Load()
    {
        // data ??= CustomEffectManager.GetData(name);
        if (data == null) return;
        m_name = data.Name;
        m_tooltip = data.Tooltip;
        m_icon = data.icon;
        m_ttl = data.Duration;
        m_startEffects.Set(data.StartEffects.ToArray());
        m_stopEffects.Set(data.StopEffects.ToArray());
        m_modifyAttackSkill = Skills.SkillType.All;
        m_damageModifier = data.GetValue(CEVars.DamageModifier, 1f);
        m_noiseModifier = data.GetValue(CEVars.NoiseModifier, 0f);
        m_addMaxCarryWeight = data.GetValue(CEVars.CarryWeight, 0f);
        m_speedModifier = data.GetValue(CEVars.Speed, 0f);
        m_maxMaxFallSpeed = data.GetValue(CEVars.MaxFallSpeed, 0f);
        m_fallDamageModifier = data.GetValue(CEVars.FallDamageModifier, 0f);
        m_windMovementModifier = data.GetValue(CEVars.WindMovementModifier, 0f);
        m_windRunStaminaModifier = data.GetValue(CEVars.WindRunStaminaModifier, 0f);
        m_healthRegenMultiplier = data.GetValue(CEVars.HealthRegenModifier, 1f);
        m_staminaRegenMultiplier = data.GetValue(CEVars.StaminaRegenModifier, 1f);
        m_eitrRegenMultiplier = data.GetValue(CEVars.EitrRegenModifier, 1f);
        m_raiseSkillModifier = data.GetValue(CEVars.RaiseSkills, 0f);
        m_damageReduction = data.GetValue(CEVars.DamageReduction, 0f);
        m_health = data.GetValue(CEVars.Health, 0f);
        m_stamina = data.GetValue(CEVars.Stamina, 0f);
        m_eitr = data.GetValue(CEVars.Eitr, 0f);
        m_armor = data.GetValue(CEVars.Armor, 0f);
        m_lifeSteal = data.GetValue(CEVars.LifeSteal, 0f);
        m_percentigeDamageModifiers.m_blunt = data.GetValue(CEVars.BluntDamage, 0f);
        m_percentigeDamageModifiers.m_slash = data.GetValue(CEVars.SlashDamage, 0f);
        m_percentigeDamageModifiers.m_pierce = data.GetValue(CEVars.PierceDamage, 0f);
        m_percentigeDamageModifiers.m_chop = data.GetValue(CEVars.ChopDamage, 0f);
        m_percentigeDamageModifiers.m_pickaxe = data.GetValue(CEVars.PickaxeDamage, 0f);
        m_percentigeDamageModifiers.m_fire = data.GetValue(CEVars.FireDamage, 0f);
        m_percentigeDamageModifiers.m_frost = data.GetValue(CEVars.FrostDamage, 0f);
        m_percentigeDamageModifiers.m_lightning = data.GetValue(CEVars.LightningDamage, 0f);
        m_percentigeDamageModifiers.m_poison = data.GetValue(CEVars.PoisonDamage, 0f);
        m_percentigeDamageModifiers.m_spirit = data.GetValue(CEVars.SpiritDamage, 0f);
        m_bluntResistance = data.GetValue(CEVars.BluntResistance, 0f);
        m_slashResistance = data.GetValue(CEVars.SlashResistance, 0f);
        m_pierceResistance = data.GetValue(CEVars.PierceResistance, 0f);
        m_chopResistance = data.GetValue(CEVars.ChopResistance, 0f);
        m_pickaxeResistance = data.GetValue(CEVars.PickaxeResistance, 0f);
        m_fireResistance = data.GetValue(CEVars.FireResistance, 0f);
        m_frostResistance = data.GetValue(CEVars.FrostResistance, 0f);
        m_lightningResistance = data.GetValue(CEVars.LightningResistance, 0f);
        m_poisonResistance = data.GetValue(CEVars.PoisonResistance, 0f);
        m_spiritResistance = data.GetValue(CEVars.SpiritResistance, 0f);
        m_jumpStaminaUseModifier = data.GetValue(CEVars.JumpStaminaModifier, 0f);
        m_attackStaminaUseModifier = data.GetValue(CEVars.AttackStaminaModifier, 0f);
        m_blockStaminaUseModifier = data.GetValue(CEVars.BlockStaminaModifier, 0f);
        m_dodgeStaminaUseModifier = data.GetValue(CEVars.DodgeStaminaModifier, 0f);
        m_swimStaminaUseModifier = data.GetValue(CEVars.SwimStaminaModifier, 0f);
        m_homeItemStaminaUseModifier = data.GetValue(CEVars.HomeItemStaminaModifier, 0f);
        m_sneakStaminaUseModifier = data.GetValue(CEVars.SneakStaminaModifier, 0f);
        m_runStaminaDrainModifier = data.GetValue(CEVars.RunStaminaModifier, 0f);
    }
    public override void ModifySkillLevel(Skills.SkillType skill, ref float level)
    {
        if (data == null) return;
        if (data.Skills.TryGetValue("All", out var all)) level += all;
        if (data.Skills.TryGetValue(skill.ToString(), out var amount)) level += amount;
    }

    public override void OnDamaged(HitData hit, Character character)
    {
        hit.m_damage.m_blunt *= Mathf.Clamp01(1f - m_bluntResistance);
        hit.m_damage.m_slash *= Mathf.Clamp01(1f - m_slashResistance);
        hit.m_damage.m_pierce *= Mathf.Clamp01(1f - m_pierceResistance);
        hit.m_damage.m_chop *= Mathf.Clamp01(1f - m_chopResistance);
        hit.m_damage.m_pickaxe *= Mathf.Clamp01(1f - m_pickaxeResistance);
        hit.m_damage.m_fire *= Mathf.Clamp01(1f - m_fireResistance);
        hit.m_damage.m_frost *= Mathf.Clamp01(1f - m_frostResistance);
        hit.m_damage.m_lightning *= Mathf.Clamp01(1f - m_lightningResistance);
        hit.m_damage.m_poison *= Mathf.Clamp01(1f - m_poisonResistance);
        hit.m_damage.m_spirit *= Mathf.Clamp01(1f - m_spiritResistance);
        hit.ApplyModifier(Mathf.Clamp01(1f - m_damageReduction));
    }
    public void LifeSteal(HitData hit, Character character)
    {
        float leech = m_lifeSteal - 1f;
        if (leech > 1f) leech = 1f;
        if (leech > 0f)
        {
            float total = hit.GetTotalDamage() * leech;
            character.Heal(total);
        }
    }
    public override string GetTooltipString()
    {
        if (data == null) return base.GetTooltipString();
        StringBuilder sb = new();
        sb.Append(base.GetTooltipString());
        if (m_damageReduction != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.DamageReduction, Mathf.Clamp01(1f - m_damageReduction) * 100f - 100);
        }
        if (m_bluntResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Blunt, Mathf.Clamp01(1f - m_bluntResistance) * 100f - 100);
        }
        if (m_slashResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Slash, Mathf.Clamp01(1f - m_slashResistance) * 100f - 100);
        }
        if (m_pierceResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Pierce, Mathf.Clamp01(1f - m_pierceResistance) * 100f - 100);
        }
        if (m_chopResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Chop, Mathf.Clamp01(1f - m_chopResistance) * 100f - 100);
        }
        if (m_pickaxeResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Pickaxe, Mathf.Clamp01(1f - m_pickaxeResistance) * 100f - 100);
        }
        if (m_fireResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Fire, Mathf.Clamp01(1f - m_fireResistance) * 100f - 100);
        }
        if (m_frostResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Frost, Mathf.Clamp01(1f - m_frostResistance) * 100f - 100);
        }
        if (m_lightningResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Lightning, Mathf.Clamp01(1f - m_lightningResistance) * 100f - 100);
        }
        if (m_poisonResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Poison, Mathf.Clamp01(1f - m_poisonResistance) * 100f - 100);
        }
        if (m_spiritResistance != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Spirit, Mathf.Clamp01(1f - m_spiritResistance) * 100f - 100);
        }
        if (m_health != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", Keys.Health, m_health);
        }

        if (m_stamina != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", Keys.Stamina, m_stamina);
        }
        if (m_eitr != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", Keys.Eitr, m_eitr);
        }
        if (m_armor != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", Keys.Armor, m_armor);
        }
        
        if (m_lifeSteal != 0f)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color> {2}\n", Keys.LifeSteal, Mathf.Clamp01(m_lifeSteal - 1f) * 100f, Keys.IncomingDamage);
        }

        if (!Mathf.Approximately(m_damageModifier, 1f))
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}%</color>\n", Keys.Damage, m_damageModifier * 100f - 100f);
        }

        foreach (KeyValuePair<string, float> skill in data.Skills)
        {
            sb.AppendFormat("{0}: <color=orange>{1:+0;-0}</color>\n", "$skill_" + skill.Key.ToLower(), skill.Value);
        }
        return sb.ToString();
    }
    [Serializable]
    public class Data
    {
        public string UniqueID = string.Empty;
        public string Name = string.Empty;
        public string StartMessage = string.Empty;
        public string StopMessage = string.Empty;
        public string Tooltip = string.Empty;
        public float Duration = 0f;
        public string Icon = string.Empty;
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public List<string> StartEffects = new();
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public List<string> StopEffects = new();
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public Dictionary<string, float> Modifiers = new();
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
        public Dictionary<string, float> Skills = new();
        [YamlIgnore] public Sprite? icon => SpriteManager.GetSprite(Icon);
        [YamlIgnore] public string Path = string.Empty;
        public void CopyFrom(Data other)
        {
            UniqueID = other.UniqueID;
            Name = other.Name;
            Tooltip = other.Tooltip;
            StartMessage = other.StartMessage;
            StopMessage = other.StopMessage;
            Duration = other.Duration;
            Icon = other.Icon;
            StartEffects = other.StartEffects;
            StopEffects = other.StopEffects;
            Modifiers = other.Modifiers;
            Skills = other.Skills;
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.SetMaxHealth))]
    public static class Player_SetMaxHealth_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Player __instance, ref float health)
        {
            if (__instance != Player.m_localPlayer) return;
            foreach (StatusEffect effect in __instance.GetSEMan().GetStatusEffects())
            {
                if (effect is not CustomEffect customEffect) continue;
                health += customEffect.m_health;
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    public static class Character_RPC_Damage_Patch
    {
        [UsedImplicitly]
        private static void Postfix(HitData hit)
        {
            Character attacker = hit.GetAttacker();
            if (attacker == null) return;
            if (attacker is not Player player) return;
            foreach (StatusEffect? effect in player.GetSEMan().GetStatusEffects())
            {
                if (effect is not CustomEffect customEffect) continue;
                customEffect.LifeSteal(hit, player);
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetMaxEitr))]
    public static class Player_SetMaxEitr_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Player __instance, ref float eitr)
        {
            if (__instance != Player.m_localPlayer) return;
            foreach (var effect in __instance.GetSEMan().GetStatusEffects())
            {
                if (effect is not CustomEffect achievementEffect) continue;
                eitr += achievementEffect.m_eitr;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetMaxStamina))]
    public static class Player_SetMaxStamina_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Player __instance, ref float stamina)
        {
            if (__instance != Player.m_localPlayer) return;
            foreach (var effect in __instance.GetSEMan().GetStatusEffects())
            {
                if (effect is not CustomEffect achievementEffect) continue;
                stamina += achievementEffect.m_stamina;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetBodyArmor))]
    public static class Player_GetBodyArmor_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance, ref float __result)
        {
            float amount = 0f;
            foreach (StatusEffect? effect in __instance.GetSEMan().GetStatusEffects())
            {
                if (effect is not CustomEffect achievementEffect) continue;
                amount += achievementEffect.m_armor;
            }
            __result += amount;
        }
    }
}

public static class SEHelpers
{
    public static float GetValue(this CustomEffect.Data? data, string key, float defaultValue) => data?.Modifiers.GetValueOrDefault(key, defaultValue) ?? defaultValue;

    public static void Set(this EffectList list, params string[] effectNames)
    {
        List<EffectList.EffectData> effects = new();
        foreach (string name in effectNames)
        {
            if (ZNetScene.instance?.GetPrefab(name) is { } prefab)
            {
                effects.Add(new EffectList.EffectData() { m_prefab = prefab });
            }
        }
        list.m_effectPrefabs = effects.ToArray();
    }

    public static List<StatusEffect> statusEffects => ObjectDB.instance?.m_StatusEffects ?? new();
}
public static class CEVarsHelper
{
    private static readonly string[] allVars;

    static CEVarsHelper()
    {
        allVars = typeof(CEVars)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null))
            .ToArray();
    }

    private static readonly string[] Prefix =
    {
        "# Custom Status Effects",
        "You can create new status effects using Almanac Custom Status Effects Manager.",
        "",
        "Below are the available modifier types:",
        "```"
    };

    private static readonly string[] Postfix =
    {
        "```",
        "",
        "Each modifier uses a `float` value to define the effect strength.",
        "- Some like `CarryWeight` are additive.",
        "- Most others are multipliers.",
        "",
        "### Tips",
        "- You can create, delete, or edit files while in-game to preview tooltips live.",
        "- Almanac also includes a built-in creation tool for admins.",
        "- Only the host can add status effects, everyone else gets the data from the host",
        "- If you are admin, you can use the creation tool to define your status effect file, then upload the file to your server."
    };

    public static List<string> BuildReadMe()
    {
        List<string> lines = new List<string>();
        lines.AddRange(Prefix);
        lines.AddRange(allVars);
        lines.AddRange(Postfix);
        return lines;
    }

    public static bool IsCEVar(string value)
    {
        return allVars.Contains(value);
    }
}
public static class CEVars
{
    public static readonly string DamageModifier = nameof(DamageModifier);
    public static readonly string NoiseModifier = nameof(NoiseModifier);
    public static readonly string CarryWeight = nameof(CarryWeight);
    public static readonly string Speed = nameof(Speed);
    public static readonly string MaxFallSpeed = nameof(MaxFallSpeed);
    public static readonly string FallDamageModifier = nameof(FallDamageModifier);
    public static readonly string WindMovementModifier = nameof(WindMovementModifier);
    public static readonly string WindRunStaminaModifier = nameof(WindRunStaminaModifier);
    public static readonly string HealthRegenModifier = nameof(HealthRegenModifier);
    public static readonly string StaminaRegenModifier = nameof(StaminaRegenModifier);
    public static readonly string EitrRegenModifier = nameof(EitrRegenModifier);
    public static readonly string RaiseSkills = nameof(RaiseSkills);
    public static readonly string DamageReduction = nameof(DamageReduction);
    public static readonly string Health = nameof(Health);
    public static readonly string Stamina = nameof(Stamina);
    public static readonly string Eitr = nameof(Eitr);
    public static readonly string LifeSteal = nameof(LifeSteal);
    public static readonly string Armor = nameof(Armor);
    public static readonly string BluntDamage = nameof(BluntDamage);
    public static readonly string SlashDamage = nameof(SlashDamage);
    public static readonly string PierceDamage = nameof(PierceDamage);
    public static readonly string ChopDamage = nameof(ChopDamage);
    public static readonly string PickaxeDamage = nameof(PickaxeDamage);
    public static readonly string FireDamage = nameof(FireDamage);
    public static readonly string FrostDamage = nameof(FrostDamage);
    public static readonly string LightningDamage = nameof(LightningDamage);
    public static readonly string PoisonDamage = nameof(PoisonDamage);
    public static readonly string SpiritDamage = nameof(SpiritDamage);
    public static readonly string BluntResistance = nameof(BluntResistance);
    public static readonly string SlashResistance = nameof(SlashResistance);
    public static readonly string PierceResistance = nameof(PierceResistance);
    public static readonly string ChopResistance = nameof(ChopResistance);
    public static readonly string PickaxeResistance = nameof(PickaxeResistance);
    public static readonly string FireResistance = nameof(FireResistance);
    public static readonly string FrostResistance = nameof(FrostResistance);
    public static readonly string LightningResistance = nameof(LightningResistance);
    public static readonly string PoisonResistance = nameof(PoisonResistance);
    public static readonly string SpiritResistance = nameof(SpiritResistance);
    public static readonly string JumpStaminaModifier = nameof(JumpStaminaModifier);
    public static readonly string AttackStaminaModifier = nameof(AttackStaminaModifier);
    public static readonly string BlockStaminaModifier = nameof(BlockStaminaModifier);
    public static readonly string DodgeStaminaModifier = nameof(DodgeStaminaModifier);
    public static readonly string SwimStaminaModifier = nameof(SwimStaminaModifier);
    public static readonly string HomeItemStaminaModifier = nameof(HomeItemStaminaModifier);
    public static readonly string SneakStaminaModifier = nameof(SneakStaminaModifier);
    public static readonly string RunStaminaModifier = nameof(RunStaminaModifier);
}