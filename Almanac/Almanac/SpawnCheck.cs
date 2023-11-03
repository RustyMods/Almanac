// using System.Collections.Generic;
// using BepInEx;
// using HarmonyLib;
//
// namespace Almanac.Almanac;
//
// public static class SpawnCheck
// {
//     private class SpawnInfo
//     {
//         public string m_name = null!;
//         public float m_spawn_chance;
//     }
//
//     private static readonly Dictionary<Heightmap.Biome, List<SpawnInfo>> spawnCheckData = new()
//     {
//         { Heightmap.Biome.Meadows , new List<SpawnInfo>() },
//         { Heightmap.Biome.BlackForest , new List<SpawnInfo>() },
//         { Heightmap.Biome.Swamp , new List<SpawnInfo>() },
//         { Heightmap.Biome.Mountain , new List<SpawnInfo>() },
//         { Heightmap.Biome.Plains , new List<SpawnInfo>() },
//         { Heightmap.Biome.Mistlands , new List<SpawnInfo>() },
//     };
//     
//     [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
//     static class SpawnSystemPatch
//     {
//         private static void Postfix(SpawnSystem __instance)
//         {
//             var spawnList = __instance.m_spawnLists;
//             foreach (var spawn in spawnList)
//             {
//                 List<SpawnSystem.SpawnData> spawners = spawn.m_spawners;
//                 foreach (SpawnSystem.SpawnData? spawner in spawners)
//                 {
//                     if (spawner.m_name.IsNullOrWhiteSpace()) continue;
//                     if (spawner.m_name.Contains("Fish")) continue;
//
//                     Heightmap.Biome biomes = spawner.m_biome;
//                     float spawnChance = spawner.m_spawnChance;
//
//                     SpawnInfo creatureInfo = new SpawnInfo()
//                     {
//                         m_name = spawner.m_name,
//                         m_spawn_chance = spawnChance
//                     };
//
//                     switch (biomes)
//                     {
//                         case Heightmap.Biome.Meadows:
//                             if (spawnCheckData[Heightmap.Biome.Meadows].Exists(x => x.m_name == creatureInfo.m_name)) continue;
//                             spawnCheckData[Heightmap.Biome.Meadows].Add(creatureInfo);
//                             continue;
//                         case Heightmap.Biome.BlackForest:
//                             if (spawnCheckData[Heightmap.Biome.BlackForest].Exists(x => x.m_name == creatureInfo.m_name)) continue;
//                             spawnCheckData[Heightmap.Biome.BlackForest].Add(creatureInfo);
//                             continue;
//                         case Heightmap.Biome.Swamp:
//                             if (spawnCheckData[Heightmap.Biome.Swamp].Exists(x => x.m_name == creatureInfo.m_name)) continue;
//                             spawnCheckData[Heightmap.Biome.Swamp].Add(creatureInfo);
//                             continue;
//                         case Heightmap.Biome.Mountain:
//                             if (spawnCheckData[Heightmap.Biome.Mountain].Exists(x => x.m_name == creatureInfo.m_name)) continue;
//                             spawnCheckData[Heightmap.Biome.Mountain].Add(creatureInfo);
//                             continue;
//                         case Heightmap.Biome.Plains:
//                             if (spawnCheckData[Heightmap.Biome.Plains].Exists(x => x.m_name == creatureInfo.m_name)) continue;
//                             spawnCheckData[Heightmap.Biome.Plains].Add(creatureInfo);
//                             continue;
//                         case Heightmap.Biome.Mistlands:
//                             if (spawnCheckData[Heightmap.Biome.Mistlands].Exists(x => x.m_name == creatureInfo.m_name)) continue;
//                             spawnCheckData[Heightmap.Biome.Mistlands].Add(creatureInfo);
//                             continue;
//                     }
//                 }
//             }
//             
//             AlmanacPlugin.AlmanacLogger.LogWarning("Meadows" + ":" + spawnCheckData[Heightmap.Biome.Meadows].Count);
//             AlmanacPlugin.AlmanacLogger.LogWarning("BlackForest" + ":" + spawnCheckData[Heightmap.Biome.BlackForest].Count);
//             AlmanacPlugin.AlmanacLogger.LogWarning("Swamp" + ":" + spawnCheckData[Heightmap.Biome.Swamp].Count);
//             AlmanacPlugin.AlmanacLogger.LogWarning("Mountains" + ":" + spawnCheckData[Heightmap.Biome.Mountain].Count);
//             AlmanacPlugin.AlmanacLogger.LogWarning("Plains" + ":" + spawnCheckData[Heightmap.Biome.Plains].Count);
//             AlmanacPlugin.AlmanacLogger.LogWarning("MistLands" + ":" + spawnCheckData[Heightmap.Biome.Mistlands].Count);
//             
//         }
//     }
//     
// }