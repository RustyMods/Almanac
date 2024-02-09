using System.Collections.Generic;
using System.Text;
using Almanac.Data;
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
        
        Terminal.ConsoleCommand AlmanacListKeys = new Terminal.ConsoleCommand("almanac_list_keys",
            "List of defeat keys",
            args =>
            {
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
            });

        Terminal.ConsoleCommand AlmanacDataSize = new Terminal.ConsoleCommand("almanac_size",
            "Prints out the data size of player custom data",
            args =>
            {
                if (!Player.m_localPlayer) return;
                if (!Player.m_localPlayer.m_customData.TryGetValue(PlayerStats.AlmanacStatsKey, out string data))
                {
                    AlmanacLogger.LogInfo("No Almanac custom player data found");
                    return;
                }

                int size = Encoding.UTF8.GetByteCount(data);
                double kilobytes = size / 1024.0;
                
                AlmanacLogger.LogInfo("Almanac Custom Data size: " + kilobytes + " kilobytes");
            });
    }
}