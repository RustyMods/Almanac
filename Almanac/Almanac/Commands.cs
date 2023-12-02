using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Almanac.Almanac.AchievementManager;
using static Almanac.AlmanacPlugin;

namespace Almanac.Almanac;

public static class Commands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    static class TerminalInitPatch
    {
        private static void Postfix()
        {
            Terminal.ConsoleCommand AlmanacSaveAchievementData = new("almanac_write_current_achievements", "Writes current achievement data to file",
                args =>
                {
                    AlmanacLogger.LogInfo("Writing achievement data to: \n" + achievementPath );
                    SaveAchievementData();
                }) { OnlyAdmin = true };
            
            Terminal.ConsoleCommand AlmanacResetAchievementData = new("almanac_write_default_achievements", "Writes default achievement data to file",
                args =>
                {
                    AlmanacLogger.LogInfo("Writing default achievement data to: \n" + achievementPath );
                    ResetAchievementData();
                }) { OnlyAdmin = true };

            Terminal.ConsoleCommand AlmanacResetGuardianPower = new Terminal.ConsoleCommand("almanac_reset_gp",
                "Resets the guardian power cool down",
                args =>
                {
                    if (!Player.m_localPlayer) return;
                    Player.m_localPlayer.m_guardianPowerCooldown = 0.0f;
                }) { OnlyAdmin = true };
            
            Terminal.ConsoleCommand AlmanacRebuildAchievements = new Terminal.ConsoleCommand("almanac_rebuild_achievements",
                "Rebuilds the achievements",
                args =>
                {
                    if (!Player.m_localPlayer) return;
                    AchievementManager.ReBuildAchievements();
                }) { OnlyAdmin = true };

            Terminal.ConsoleCommand AlmanacListKeys = new Terminal.ConsoleCommand("almanac_list_keys",
                "List of defeat keys",
                args =>
                {
                    List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
                    AlmanacLogger.LogInfo("Global keys: ");
                    foreach (var key in globalKeys)
                    {
                        AlmanacLogger.LogInfo(key);
                    }
                    AlmanacLogger.LogInfo("Private Keys: ");
                    foreach (var creature in Almanac.CreateAlmanac.creatures)
                    {
                        string key = creature.defeatedKey;
                        if (ZoneSystem.instance.GetGlobalKey(key))
                        {
                            AlmanacLogger.LogInfo(key);
                        };
                    }
                });
            // Terminal.ConsoleCommand AlmanacFishList = new("almanac_list_fishes", "List of fish", args =>
            // {
            //     foreach (GameObject fish in CreatureDataCollector.fishObj)
            //     {
            //         AlmanacLogger.LogWarning(fish.name);
            //     }
            // });
        }
    }
}