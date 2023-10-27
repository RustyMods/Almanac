using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Almanac;

public static class TrackPlayerKills
{
    public static class PlayerTracker
    {
        private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";
        private static readonly string fileName = "AlmanacMonsterKilled.yml";
        private static readonly string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
        
        private static Dictionary<string, int> TempMonstersKilled = new();

        public static void SetInitialData(List<CreatureDataCollector.CreatureData> creatures)
        {
            Dictionary<string, int> initialData = new();
            foreach (var creature in creatures)
            {
                initialData[creature.defeatedKey] = 0;
            }

            TempMonstersKilled = initialData;
        }

        public static Dictionary<string, int> GetCurrentKilledMonsters()
        {
            if (!File.Exists(filePath)) return TempMonstersKilled;
            var deserializer = new DeserializerBuilder().Build();
            string deserializedData = File.ReadAllText(filePath);
            Dictionary<string, int> currentData = deserializer.Deserialize<Dictionary<string, int>>(deserializedData);

            return currentData;
        }

        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        static class CharacterCustomFixedUpdatePatch
        {
            private static void Prefix(Character __instance)
            {
                if (!__instance) return;

                string localPlayer = Player.m_localPlayer.GetHoverName();
                Character attacker = __instance.m_lastHit.GetAttacker();
                string hoverName = attacker.GetHoverName();
                string defeatKey = __instance.m_defeatSetGlobalKey;

                if (localPlayer == hoverName)
                {
                    TempMonstersKilled[defeatKey] += 1;
                }
            }
        }
        
        [HarmonyPatch(typeof(Player), nameof(Player.Save))]
        static class PlayerAwakePatch
        {
            private static void Postfix(Player __instance)
            {
                if (!__instance) return;
                if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Server) return;
                
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var serializer = new SerializerBuilder().Build();
                var deserializer = new DeserializerBuilder().Build();

                if (!File.Exists(filePath))
                {
                    
                    string serializedData = serializer.Serialize(TempMonstersKilled);
                    File.WriteAllText(filePath, serializedData);
                }
                else
                {
                    string deserializedData = File.ReadAllText(filePath);
                    Dictionary<string, int> currentData = deserializer.Deserialize<Dictionary<string, int>>(deserializedData);
                    foreach (var monsterKilled in TempMonstersKilled)
                    {
                        if (!currentData.ContainsKey(monsterKilled.Key))
                        {
                            currentData[monsterKilled.Key] = 0;
                        }
                        else
                        {
                            currentData[monsterKilled.Key] += monsterKilled.Value;
                        }
                    }
                    string serializeUpdatedData = serializer.Serialize(currentData);
                    File.WriteAllText(filePath, serializeUpdatedData);
                }
            }
        }
    }
}