using System.Collections.Generic;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using HarmonyLib;
using YamlDotNet.Serialization;

namespace Almanac.FileSystem;

public static class Patches
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    private static class ZNetStartPatch
    {
        private static void Postfix(ZNet __instance)
        {
            if (!__instance) return;
            AlmanacPaths.CreateFolderDirectories();
            AchievementManager.Read();
            AchievementManager.Setup();
            BountyManager.InitBounties();
            TreasureHunt.TreasureManager.InitTreasureManager();
            
            ServerSyncedData.InitServerAchievements();
            ServerSyncedData.InitServerIgnoreList();
            ServerSyncedData.InitServerCreatureList();
            ServerSyncedData.InitServerBountyList();
            ServerSyncedData.InitServerTreasureHunt();
        }
    }

    // [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    // private static class ZoneSystemStartPatch
    // {
    //     private static void Postfix(ZoneSystem __instance)
    //     {
    //         if (!__instance) return;
    //         PieceDataCollector.GetBuildPieces();
    //     }
    // }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    private static class PlayerOnSpawnedPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance || __instance != Player.m_localPlayer) return;
            EffectMan.Load();
            AchievementManager.CheckCollectedRewards(__instance);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.StartGuardianPower))]
    private static class StartGuardianPowerPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (__instance.m_guardianSE is EffectMan.AchievementEffect)
            {
                if (__instance.GetSEMan().HaveStatusEffect(__instance.m_guardianPowerHash)) return;
                __instance.GetSEMan().AddStatusEffect(__instance.m_guardianSE);
            }
        }
    }
    
    [HarmonyPatch(typeof(ZLog), nameof(ZLog.LogWarning))]
    static class MuteGuardianPowerStats
    {
        private static bool Prefix(object o) => !o.ToString().StartsWith("Missing stat for guardian power");
    }
}