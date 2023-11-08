using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using YamlDotNet.Serialization;
using static Almanac.AlmanacPlugin;

namespace Almanac.Almanac;

public static class SaveCustomData
{
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Almanac";

    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    static class PlayerSavePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (WorkingAsType is WorkingAs.Server) return;

            Dictionary<string, int> TempMonstersKilled = TrackPlayerKills.TempMonstersKilled;
            Dictionary<string, int> TempPlayerDeaths = TrackPlayerDeaths.TempPlayerDeaths;
                
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            
            Dictionary<string, Dictionary<string, int>> conversionMap = new()
            {
                { "AlmanacMonsterKilled.yml", TempMonstersKilled },
                { "AlmanacPlayerDeaths.yml", TempPlayerDeaths }
            };
            foreach (KeyValuePair<string, Dictionary<string, int>> kvp in conversionMap)
            {
                SaveAlmanac(kvp.Key, kvp.Value);
            }
            
            TrackPlayerKills.ResetTempMonstersKilled();
            TrackPlayerDeaths.ResetTempPlayerDeaths();
        }

        private static void SaveAlmanac(string fileName, Dictionary<string, int> data)
        {
            string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
            
            ISerializer serializer = new SerializerBuilder().Build();
            IDeserializer deserializer = new DeserializerBuilder().Build();
            if (!File.Exists(filePath))
            {
                string serializedData = serializer.Serialize(data);
                File.WriteAllText(filePath, serializedData);
            }
            else
            {
                string deserializedData = File.ReadAllText(filePath);
                Dictionary<string, int> currentData = deserializer.Deserialize<Dictionary<string, int>>(deserializedData);
                foreach (var kvp in data)
                {
                    if (!currentData.ContainsKey(kvp.Key))
                    {
                        currentData[kvp.Key] = 0;
                    }
                    else
                    {
                        currentData[kvp.Key] += kvp.Value;
                    }
                }
                string serializeUpdatedData = serializer.Serialize(currentData);
                File.WriteAllText(filePath, serializeUpdatedData);
            }
        }
    }
}