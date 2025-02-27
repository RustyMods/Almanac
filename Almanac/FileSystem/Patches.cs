﻿using System.Collections.Generic;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.UI;
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
            CheckIfServer();
            AlmanacPaths.CreateFolderDirectories();
            AchievementManager.ReadAchievements();
            AchievementManager.InitAchievements(AchievementManager.LoadAchievementData(AchievementManager.AchievementData));
            BountyManager.InitBounties();
            TreasureHunt.TreasureManager.InitTreasureManager();
            
            ServerSyncedData.InitServerAchievements();
            ServerSyncedData.InitServerIgnoreList();
            ServerSyncedData.InitServerCreatureList();
            ServerSyncedData.InitServerBountyList();
            ServerSyncedData.InitServerTreasureHunt();
        }
    }
    
    public static void CheckIfServer()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Server) return;
        if (!ZNet.instance) return;
        if (ZNet.instance.IsServer())
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client is server");
            AlmanacPlugin.WorkingAsType = AlmanacPlugin.WorkingAs.Both;
        }
        else
        {
            AlmanacPlugin.WorkingAsType = AlmanacPlugin.WorkingAs.Client;
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    private static class ZoneSystemStartPatch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            PieceDataCollector.GetBuildPieces();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    private static class PlayerOnSpawnedPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance || __instance != Player.m_localPlayer) return;
            if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Client)
            {
                PlayerStats.UpdatePlayerStats();
            }
            if (!UpdateAlmanac.CheckedCompletion) AchievementManager.CheckCompletedAchievements();
            ApplySavedAchievementEffects(__instance);
            AchievementManager.CheckCollectedRewards(__instance);
        }
    }
    private static void ApplySavedAchievementEffects(Player player)
    {
        if (!player.m_customData.TryGetValue(AlmanacEffectManager.AchievementKey, out string data)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        AlmanacEffectManager.SavedAchievementEffectNames = deserializer.Deserialize<List<string>>(data);
        if (AlmanacEffectManager.SavedAchievementEffectNames.Count <= 0) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Applying saved achievement effects");
        SEMan PlayerSEMan = player.GetSEMan();
        AlmanacEffectManager.ActiveAchievementEffects.Clear();
        foreach (string name in AlmanacEffectManager.SavedAchievementEffectNames)
        {
            
            if (PlayerSEMan.HaveStatusEffect(name.GetStableHashCode())) continue;
            AchievementManager.Achievement achievement = AchievementManager.AchievementList.Find(x => x.m_statusEffect != null && x.m_statusEffect.name == name);
            if (achievement != null && achievement.m_statusEffect != null)
            {
                if (!achievement.m_isCompleted) continue;
                PlayerSEMan.AddStatusEffect(achievement.m_statusEffect);
                AlmanacEffectManager.ActiveAchievementEffects.Add(achievement.m_statusEffect);
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
        private static bool Prefix(object o) => !o.ToString().StartsWith("Missing stat for guardian power");
    }
}