using Almanac.Achievements;
using Almanac.Data;
using Almanac.Store;
using JetBrains.Annotations;

namespace Almanac;

[PublicAPI]
public class API
{
    public static int GetCreatureKillCount(string creatureID)
    {
        return !CritterHelper.namedCritters.TryGetValue(creatureID, out var critter) ? 0 : PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, critter.character.m_name);
    }

    public static int GetPlayerDeathCount(string creatureID)
    {
        return !CritterHelper.namedCritters.TryGetValue(creatureID, out var critter) ? 0 : PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Death, critter.character.m_name);
    }
    public static int GetPlayerCompletedAchievements(Player player) => player.GetCollectedAchievements().Count;
    public static void AddTokens(Player player, int amount, bool message) => player.AddTokens(amount, message);
    public static void RemoveTokens(Player player, int amount) => player.RemoveTokens(amount);
    public static int GetTokens(Player player) => player.GetTokens();
}