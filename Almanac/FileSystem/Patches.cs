using Almanac.Achievements;
using Almanac.Data;
using HarmonyLib;

namespace Almanac.FileSystem;

public static class Patches
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    private static class ZNetStartPatch
    {
        private static void Postfix(ZNet __instance)
        {
            if (!__instance) return;
            CheckIfServer();
            AlmanacPaths.CreateFolderDirectories();
            AchievementManager.ReadAchievements();
            AchievementManager.InitAchievements(AchievementManager.LoadAchievementData(AchievementManager.AchievementData));
            ServerSyncedData.InitServerAchievements();
            ServerSyncedData.InitServerIgnoreList();
            ServerSyncedData.InitServerCreatureList();
        }
    }
    
    private static void CheckIfServer()
    {
        if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client) return;
        if (!ZNet.instance) return;
        if (ZNet.instance.IsServer())
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client is server");
            AlmanacPlugin.WorkingAsType = AlmanacPlugin.WorkingAs.Both;
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    private static class ZoneSystemStartPatch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            PieceDataCollector.GetBuildPieces();
            if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
            {
                ServerSyncedData.InitServerPlayerData();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    private static class PlayerOnSpawnedPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
            {
                PlayerStats.UpdatePlayerStats();
                ServerSyncedData.InitServerPlayerData();
            }
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.StartGuardianPower))]
    private static class StartGuardianPowerPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (__instance.m_guardianSE is AlmanacEffectManager.AchievementEffect)
            {
                if (__instance.GetSEMan().HaveStatusEffect(__instance.m_guardianPowerHash)) return;
                __instance.GetSEMan().AddStatusEffect(__instance.m_guardianSE);
            }
        }
    }
    
    [HarmonyPatch(typeof(ZLog), nameof(ZLog.LogWarning))]
    static class MuteGuardianPowerStats
    {
        private static bool Prefix(object o)
        {
            return !o.ToString().StartsWith("Missing stat for guardian power");
        }
    }
}