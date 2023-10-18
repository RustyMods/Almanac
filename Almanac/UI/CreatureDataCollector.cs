using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.UI;
public static class CreatureDataCollector
{
    public static readonly string outputFilePath = Paths.ConfigPath + Path.DirectorySeparatorChar + "AlmanacCreatureData.yml";
    
    [Serializable] [CanBeNull]
    public class CreatureData
    {
        public string name = "no data";
        public string display_name = "unknown";
        public float? health = 1000f;
        public string? faction = "unknown";
        public string blunt = "N/A";
        public string slash = "N/A";
        public string piece = "N/A";
        public string chop = "N/A";
        public string pickaxe = "N/A";
        public string fire = "N/A";
        public string frost = "N/A";
        public string lightning = "N/A";
        public string poison = "N/A";
        public string spirit = "N/A";
        public List<AttackData> defaultItems = new List<AttackData>();
        public List<string> drops = new List<string>();
        public bool avoidFire = false;
        public bool afraidOfFire = false;
        public bool avoidWater = false;
        public List<string> consumeItems = new List<string>();
        public List<string> weakSpot = new List<string>();
        public bool staggerWhenBlocked = false;
        public float staggerDamageFactor = 0.0f;
        public bool tolerateWater = false;
        public bool tolerateSmoke = false;
        public bool tolerateTar = false;
        public string trophyName = "unknown";
    }
    [Serializable] [CanBeNull]
    public class AttackData
    {
        public string name = "Unknown";
        public float damage = 0f;
        public float blunt = 0f;
        public float slash = 0f;
        public float piece = 0f;
        public float chop = 0f;
        public float pickaxe = 0f;
        public float fire = 0f;
        public float frost = 0f;
        public float lightning= 0f;
        public float poison = 0f;
        public float spirit = 0f;
        public float attackForce= 0f;
        public float backStabBonus= 0f;
        public bool dodgeable = false;
        public bool blockable = false;
        public string statusEffect = "None";
        public string statusEffectTooltip = "";
    }
    
    public static List<CreatureData> CollectAndSaveCreatureData()
    {
        List<CreatureData> creatureData = GetCreatureData();
        creatureData.RemoveAt(0);
        foreach (var data in creatureData) RenameCreatureData(data);
        return creatureData;
    }

    private static void RenameCreatureData(CreatureData creatureData)
    {
        Dictionary<string, string> conversion = new Dictionary<string, string>()
        {
            { "Leech_cave", "$enemy_leech_cave" },
            { "DvergerMageFire", "$enemy_dverger_mage_fire" },
            { "DvergerMageIce", "$enemy_dverger_mage_ice" },
            { "DvergerMageSupport", "$enemy_dverger_mage_support" }
        };
        if (conversion.ContainsKey(creatureData.name)) creatureData.display_name = conversion[creatureData.name];
    }

    private static bool inArray(CreatureData data, IEnumerable<CreatureData> array)
    {
        return array.Any(arrayData => data.name == arrayData.name);
    }
    private static bool inArray(AttackData data, IEnumerable<AttackData> array)
    {
        return array.Any(arrayData => data.name == arrayData.name);
    }

    private static List<CreatureData> GetCreatureData()
    {
        List<CreatureData> data = new List<CreatureData>();
        GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in array)
        {
            CreatureData creatureData = new CreatureData();
            obj.TryGetComponent(out Character characterScript);
            obj.TryGetComponent(out Humanoid humanoidScript);
            obj.TryGetComponent(out MonsterAI monsterAIScript);
            obj.TryGetComponent(out AnimalAI animalAI);
            if (obj.name.Contains("(Clone)") || obj.name is "DvergerTest" or "TheHive" or "Hive") continue;
            if (humanoidScript)
            {
                SaveCreatureData(obj, creatureData, humanoidScript);
                SaveDefaultItemsAttackData(creatureData, humanoidScript);
                SaveCharacterDropData(obj, creatureData);
            }
            if (characterScript)
            {
                SaveCreatureData(obj, creatureData, characterScript);
                SaveCharacterDropData(obj, creatureData);
            }
            if (monsterAIScript)
            {
                SaveMonsterAIData(monsterAIScript, creatureData);
            }

            if (animalAI)
            {
                SaveAnimalAIData(animalAI, creatureData);
            }
            if (inArray(creatureData, data) == false) data.Add(creatureData);
        }

        return data;
    }

    private static void SaveMonsterAIData(MonsterAI script, CreatureData data)
    {
        try
        {
            data.afraidOfFire = script.m_afraidOfFire;
            data.avoidWater = script.m_avoidWater;
            List<string> consumeItems = new List<string>();
            foreach (var item in script.m_consumeItems) consumeItems.Add(item.name);
            data.consumeItems = consumeItems;
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get monster data of {data.name}, continuing...");
        }
    }
    private static void SaveAnimalAIData(AnimalAI script, CreatureData data)
    {
        try
        {
            data.avoidFire = script.m_avoidFire;
            data.afraidOfFire = script.m_afraidOfFire;
            data.avoidWater = script.m_avoidWater;
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get animal data of {data.name}, continuing...");
        }
        
    }
    private static void SaveCharacterDropData(GameObject prefab, CreatureData data)
    {
        try
        {
            var characterDrop = prefab.GetComponent<CharacterDrop>();
            if (!characterDrop) return;
            List<string> dropList = new List<string>(); 
            List<CharacterDrop.Drop> drops = characterDrop.m_drops;
            foreach (CharacterDrop.Drop drop in drops)
            {
                dropList.Add(drop.m_prefab.name);
                if (drop.m_prefab.name.Contains("Trophy") && drop.m_prefab.name != "TrophyAmber_coe")
                {
                    data.trophyName = drop.m_prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                }
            }
            data.drops = dropList;
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get creature drop data of {data.name}, continuing...");
        }
    }
    private static void SaveCreatureData(GameObject prefab, CreatureData data, Humanoid script)
    {
        try
        {
            data.name = prefab.name;
            data.display_name = script.m_name;
            data.health = script.m_health;
            data.faction = script.m_faction.ToString();

            var damageModifiers = script.m_damageModifiers;
            
            data.blunt = damageModifiers.m_blunt.ToString();
            data.slash = damageModifiers.m_slash.ToString();
            data.piece = damageModifiers.m_pierce.ToString();
            data.chop = damageModifiers.m_chop.ToString();
            data.pickaxe = damageModifiers.m_pickaxe.ToString();
            data.fire = damageModifiers.m_fire.ToString();
            data.frost = damageModifiers.m_frost.ToString();
            data.lightning = damageModifiers.m_lightning.ToString();
            data.poison = damageModifiers.m_poison.ToString();
            data.spirit = damageModifiers.m_spirit.ToString();

            data.tolerateWater = script.m_tolerateWater;
            data.tolerateSmoke = script.m_tolerateSmoke;
            data.tolerateTar = script.m_tolerateTar;

            data.staggerWhenBlocked = script.m_staggerWhenBlocked;
            data.staggerDamageFactor = script.m_staggerDamageFactor;

            List<string> weakSpots = new List<string>();
            foreach (var spot in script.m_weakSpots) weakSpots.Add(spot.name);
            data.weakSpot = weakSpots;
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get humanoid data of {data.name}, continuing...");
        }
    }
    private static void SaveCreatureData(GameObject prefab, CreatureData data, Character script)
    {
        try
        {
            data.name = prefab.name;
            data.display_name = script.m_name;
            data.health = script.m_health;
            data.faction = script.m_faction.ToString();

            var damageModifiers = script.m_damageModifiers;
                        
            data.blunt = damageModifiers.m_blunt.ToString();
            data.slash = damageModifiers.m_slash.ToString();
            data.piece = damageModifiers.m_pierce.ToString();
            data.chop = damageModifiers.m_chop.ToString();
            data.pickaxe = damageModifiers.m_pickaxe.ToString();
            data.fire = damageModifiers.m_fire.ToString();
            data.frost = damageModifiers.m_frost.ToString();
            data.lightning = damageModifiers.m_lightning.ToString();
            data.poison = damageModifiers.m_poison.ToString();
            data.spirit = damageModifiers.m_spirit.ToString();

            data.tolerateWater = script.m_tolerateWater;
            data.tolerateSmoke = script.m_tolerateSmoke;
            data.tolerateTar = script.m_tolerateTar;

            data.staggerWhenBlocked = script.m_staggerWhenBlocked;
            data.staggerDamageFactor = script.m_staggerDamageFactor;
            
            List<string> weakSpots = new List<string>();
            foreach (WeakSpot weakSpot in script.m_weakSpots)
            {
                weakSpots.Add(weakSpot.name);
            }

            data.weakSpot = weakSpots;
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get character data of {data.name}, continuing...");
        }
    }

    private static void SaveDefaultItemsAttackData(CreatureData data, Humanoid script)
    {
        try
        {
            GameObject[] defaultItems = script.m_defaultItems;
            GameObject[] randomWeapons = script.m_randomWeapon;
            Humanoid.ItemSet[]? randomSets = script.m_randomSets;
                        
            List<AttackData> creatureAttackData = new List<AttackData>();
                        
            if (defaultItems != null) foreach (GameObject item in defaultItems) SaveAttackData(item, creatureAttackData);
            if (randomWeapons != null) foreach (GameObject weapon in randomWeapons) SaveAttackData(weapon, creatureAttackData);
            if (randomSets != null)
                foreach (Humanoid.ItemSet set in randomSets)
                {
                    foreach (GameObject? attackItem in set.m_items) SaveAttackData(attackItem, creatureAttackData);
                }
            data.defaultItems = creatureAttackData;
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get humanoid data of {data.name}, continuing...");
        }
    }
    private static void SaveAttackData(GameObject prefab, List<AttackData> array)
    {
        AttackData attackData = new AttackData();
        ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
        if (!itemDrop) return;
        try
        {
            ItemDrop.ItemData.SharedData? sharedData = itemDrop.m_itemData.m_shared;
            HitData.DamageTypes damages = sharedData.m_damages;
            attackData.name = prefab.name;
            attackData.damage = damages.m_damage;
            attackData.blunt = damages.m_blunt;
            attackData.slash = damages.m_slash;
            attackData.piece = damages.m_pierce;
            attackData.chop = damages.m_chop;
            attackData.pickaxe = damages.m_pickaxe;
            attackData.fire = damages.m_fire;
            attackData.frost = damages.m_frost;
            attackData.lightning = damages.m_lightning;
            attackData.poison = damages.m_poison;
            attackData.spirit = damages.m_spirit;
            attackData.attackForce = sharedData.m_attackForce;
            attackData.dodgeable = sharedData.m_dodgeable;
            attackData.blockable = sharedData.m_blockable;
            if (sharedData.m_attackStatusEffect)
            {
                attackData.statusEffect = sharedData.m_attackStatusEffect.m_name;
                attackData.statusEffectTooltip = sharedData.m_attackStatusEffect.m_tooltip;
            }
            
            if (inArray(attackData, array) == false) array.Add(attackData);
        }
        catch (Exception)
        {
            AlmanacPlugin.AlmanacLogger.Log(LogLevel.Info, $"failed to get attack data of {prefab.name}, continuing...");
        }
    }

}