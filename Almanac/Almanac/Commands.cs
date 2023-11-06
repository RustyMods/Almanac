using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using static Almanac.Almanac.AchievementManager;
using static Almanac.AlmanacPlugin;

namespace Almanac.Almanac;

public static class CheckCheats
{
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public static class PlayerWatcher
    {
        public static bool noCost;
        
        public static void Postfix(Player __instance)
        {
            noCost = __instance.NoCostCheat();
        }    
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    static class TerminalInitPatch
    {
        private static void Postfix()
        {
            Terminal.ConsoleCommand AlmanacSaveAchievementData = new("almanac_print_achievements", "Writes current achievement data to file",
                args =>
                {
                    AlmanacLogger.LogInfo("Writing achievement data to: \n" + achievementPath );
                    SaveAchievementData();
                }) { OnlyAdmin = true };
            
            Terminal.ConsoleCommand AlmanacResetAchievementData = new("almanac_reset_achievements", "Writes default achievement data to file",
                args =>
                {
                    AlmanacLogger.LogInfo("Writing default achievement data to: \n" + achievementPath );
                    ResetAchievementData();
                }) { OnlyAdmin = true };

            Terminal.ConsoleCommand AlmanacResetEffects = new Terminal.ConsoleCommand("almanac_reset_effects", "Attempts to re-create status effects", args =>
            {
                AlmanacLogger.LogInfo("Attempting to re-create almanac custom status effects");

                RegisterAlmanacEffects.AddStatusEffectsToObjectDB();
            }) { OnlyAdmin = true};

            Terminal.ConsoleCommand AlmanacResetGuardianPower = new Terminal.ConsoleCommand("almanac_reset_gp",
                "Resets the guardian power cool down",
                args =>
                {
                    if (!Player.m_localPlayer) return;
                    Player.m_localPlayer.m_guardianPowerCooldown = 0.0f;
                });

            Terminal.ConsoleCommand AlmanacUpdateAchievements = new Terminal.ConsoleCommand(
                "almanac_update_achievements", "Rebuilds achievements data",
                args => { UpdateAchievementData(); });
        }
        
        private static void UpdateAchievementData()
        {
            string[] filePaths = Directory.GetFiles(achievementPath, "*.yml");
            if (filePaths.Length != tempAchievements.Count)
            {
                // Do something if new or deleted achievement data
            }
            InitAchievements(); // Read files
            RegisterAlmanacEffects.AddStatusEffectsToObjectDB(); // Create status effects
            AchievementsUI.RegisterAchievements(); // Setup UI
            
            Object.DestroyImmediate(Almanac.CreateAlmanac.achievementsPanel, true); // Destroy old achievements panel
            
            // For some reason, I can remove but I cannot add...
            Almanac.CreateAlmanac.CreateAchievementsPanel("achievements", AchievementsUI.registeredAchievements);
        }
    }
}