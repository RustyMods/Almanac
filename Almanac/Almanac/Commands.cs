using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;
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
        }
    }
}