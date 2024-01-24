using System.Collections.Generic;
using Almanac.Data;
using static Almanac.AlmanacPlugin;

namespace Almanac.Utilities;

public static class TerminalCommands
{
    public static void AddAlmanacCommands()
    {
        Terminal.ConsoleCommand GetCustomData = new("almanac_data", "", (Terminal.ConsoleEventFailable)(args =>
        {
            if (args.Length < 2) return false;

            if (!PlayerStats.TempCustomData.Player_Kill_Deaths.TryGetValue(args[1], out KillDeaths value)) return false;
                
            AlmanacLogger.LogInfo($"Key: {args[1]} , kills: {value.kills} , deaths: {value.deaths}");
                
            return true;
        }), isSecret: true, optionsFetcher: (Terminal.ConsoleOptionsFetcher)(() => CreatureDataCollector.TempDefeatKeys));
        
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
                foreach (var key in CreatureDataCollector.TempDefeatKeys)
                {
                    if (ZoneSystem.instance.GetGlobalKey(key))
                    {
                        AlmanacLogger.LogInfo(key);
                    };
                }
            });
    }
}