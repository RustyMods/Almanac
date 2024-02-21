using System.Collections.Generic;
using System.Text;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.FileSystem;
using static Almanac.AlmanacPlugin;

namespace Almanac.Utilities;

public static class TerminalCommands
{
    public static void AddAlmanacCommands()
    {
        AlmanacLogger.LogDebug("Client: Adding almanac commands to terminal");
        Terminal.ConsoleCommand GetCustomData = new("almanac_data", "Local player tracker data, searchable using currently registered defeat keys", (Terminal.ConsoleEventFailable)(args =>
        {
            if (args.Length < 2) return false;

            if (!PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(args[1], out KillDeaths value)) return false;
                
            AlmanacLogger.LogInfo($"Key: {args[1]} , kills: {value.kills} , deaths: {value.deaths}");
                
            return true;
        }), isSecret: true, optionsFetcher: (Terminal.ConsoleOptionsFetcher)(() => CreatureDataCollector.TempDefeatKeys));
        
        Terminal.ConsoleCommand AlmanacCommands = new("almanac", "Utility commands for the almanac",
            (Terminal.ConsoleEventFailable)(args =>
            {
                if (args.Length < 2) return false;
                switch (args[1])
                {
                    case "help":
                        Dictionary<string, string> commandsInfo = new()
                        {
                            { "keys", "Similar to listkeys, almanac keys prints all the current global keys and private keys the player current holds" },
                            { "size", "Prints the kilobyte size of almanac custom data saved in player save file" },
                            { "write_achievements", "Writes to file all the default achievements for the almanac" }
                        };
                        foreach (KeyValuePair<string, string> kvp in commandsInfo)
                        {
                            AlmanacLogger.LogInfo($"almanac {kvp.Key}: {kvp.Value}");
                        }
                        break;
                    case "keys":
                        List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
                        AlmanacLogger.LogInfo("Global keys: ");
                        foreach (string key in globalKeys)
                        {
                            AlmanacLogger.LogInfo(key);
                        }
                        AlmanacLogger.LogInfo("Private Keys: ");
                        foreach (string key in CreatureDataCollector.TempDefeatKeys)
                        {
                            if (ZoneSystem.instance.GetGlobalKey(key))
                            {
                                AlmanacLogger.LogInfo(key);
                            };
                        }
                        break;
                    case "size":
                        if (!Player.m_localPlayer) return false;
                        if (!Player.m_localPlayer.m_customData.TryGetValue(PlayerStats.AlmanacStatsKey, out string data))
                        {
                            AlmanacLogger.LogInfo("No Almanac custom player data found");
                            return false;
                        }

                        int size = Encoding.UTF8.GetByteCount(data);
                        double kilobytes = size / 1024.0;
                
                        AlmanacLogger.LogInfo("Almanac Custom Data size: " + kilobytes + " kilobytes");
                        break;
                    case "write_achievements":
                        AlmanacLogger.LogInfo("Almanac writing default achievements to file");
                        AlmanacLogger.LogInfo(AlmanacPaths.AchievementFolderPath);
                        AchievementYML.InitDefaultAchievements(true);
                        break;
                }
                return true;
            }),optionsFetcher: ()=> new () { "help", "keys", "size", "write_achievements" });
    }
}