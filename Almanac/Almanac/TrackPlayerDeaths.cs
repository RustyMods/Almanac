using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using YamlDotNet.Serialization;
using static Almanac.Almanac.CreatureDataCollector;

namespace Almanac.Almanac;

public static class TrackPlayerDeaths
{
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";
    private static readonly string fileName = "AlmanacPlayerDeaths.yml";
    private static readonly string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
        
    public static Dictionary<string, int> TempPlayerDeaths = new();
    private static Dictionary<string, int> zeroPlayerDeaths = new();

    public static void SetInitialData(List<CreatureData> creatures)
    {
        Dictionary<string, int> initialData = new();
        foreach (var creature in creatures)
        {
            initialData[creature.defeatedKey] = 0;
        }

        zeroPlayerDeaths = initialData;
        TempPlayerDeaths = initialData;
    }

    public static void ResetTempPlayerDeaths()
    {
        TempPlayerDeaths = zeroPlayerDeaths;
    }

    public static Dictionary<string, int> GetCurrentPlayerDeaths()
    {
        if (!File.Exists(filePath)) return TempPlayerDeaths;
        var deserializer = new DeserializerBuilder().Build();
        string deserializedData = File.ReadAllText(filePath);
        Dictionary<string, int> currentData = deserializer.Deserialize <Dictionary<string, int>>(deserializedData);

        return currentData;
    }

    [HarmonyPatch(typeof(Character),nameof(Character.ApplyDamage))]
    private static class Character_ApplyDamage_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Character __instance, ref HitData hit)
        {
            if(__instance != Player.m_localPlayer) return;
            
            if (Player.m_localPlayer.GetHealth() <= 0)
            {
                if (hit.GetAttacker() is { } killer)
                {
                    killer.TryGetComponent(out Character character);
                    string defeatKey = character.m_defeatSetGlobalKey;
                    TempPlayerDeaths[defeatKey] += 1;
                }
            }
        }
    }
}