using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;
using static Almanac.Almanac.CreatureDataCollector;

namespace Almanac.Almanac;

public static class TrackPlayerKills
{
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";
    private static readonly string fileName = "AlmanacMonsterKilled.yml";
    private static readonly string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
        
    public static Dictionary<string, int> TempMonstersKilled = new();
    private static Dictionary<string, int> zeroMonstersKilled = new();
    public static void SetInitialData(List<CreatureData> creatures)
    {
        Dictionary<string, int> initialData = new();
        foreach (CreatureData? creature in creatures)
        {
            initialData[creature.defeatedKey] = 0;
        }

        zeroMonstersKilled = initialData;
        TempMonstersKilled = initialData;
    }

    public static void ResetTempMonstersKilled() => TempMonstersKilled = zeroMonstersKilled;

    public static Dictionary<string, int> GetCurrentKilledMonsters()
    {
        if (!File.Exists(filePath)) return TempMonstersKilled;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string deserializedData = File.ReadAllText(filePath);
        Dictionary<string, int> currentData = deserializer.Deserialize<Dictionary<string, int>>(deserializedData);

        return currentData;
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    static class CharacterOnDeathPatch
    {
        private static void Prefix(Character __instance)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            HitData? lastHit = __instance.m_lastHit;
            if (lastHit == null) return;
            if (!__instance.m_lastHit.GetAttacker()) return;
            
            Character? attacker = __instance.m_lastHit.GetAttacker();
            if (!attacker) return;
            string defeatKey = __instance.m_defeatSetGlobalKey;

            // Comparing hover names caused multiplayer issue recording kills
            if (attacker.GetOwner() != Player.m_localPlayer.GetOwner()) return;
            
            if (TempMonstersKilled.ContainsKey(defeatKey))
            {
                TempMonstersKilled[defeatKey] += 1;
            }
            else
            {
                TempMonstersKilled.Add(defeatKey, 1);
            }
        }
    }
}