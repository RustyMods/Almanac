using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.Data;

namespace Almanac.Bounties;

public static class BountyGenerator
{
    public static readonly List<BountyManager.BountyData> TempAcceptedBounties = new();
    public static void OnAccepted(this BountyManager.BountyData data) => TempAcceptedBounties.Add(data);
    public static void OnCompleted(this BountyManager.BountyData data) => TempAcceptedBounties.Remove(data);
    
    public static List<BountyManager.BountyData> GetRandomBounties(List<BountyManager.BountyData> list, int count)
    {
        List<BountyManager.BountyData> output = new  List<BountyManager.BountyData>();
        output.AddRange(TempAcceptedBounties);
        List<CritterHelper.CritterInfo> knownCritters = CritterHelper.GetCritters().Where(x => x.isKnown()).ToList();
        knownCritters.RemoveAll(x => list.Any(y => y.Creature == x.prefab.name || y.Creature == "TentaRoot"));
        if (knownCritters.Count == 0) return output;
        List<string> availableBiomes = Player.m_localPlayer.m_knownBiome.Select(x => x.ToString()).ToList();
        availableBiomes.Remove("Ocean");
        availableBiomes.Remove("None");
        if (availableBiomes.Count == 0) return output;
        for (int index = 0; index < count; ++index)
        {
            if (knownCritters.Count == 0) return output;
            CritterHelper.CritterInfo critter = knownCritters[UnityEngine.Random.Range(0, knownCritters.Count)];
            knownCritters.Remove(critter);
            BountyManager.BountyData data = new()
            {
                UniqueID = Guid.NewGuid().ToString(),
                Creature = critter.prefab.name,
                Icon = ItemHelper.TryGetItemBySharedName(critter.trophy?.m_shared.m_name ?? "", out ItemDrop item)
                    ? item.name
                    : "skull",
                Health = UnityEngine.Random.Range(critter.character.m_health, critter.character.m_health * 10),
                AlmanacTokenReward = GetReward(critter.prefab.name),
                Biome = availableBiomes[UnityEngine.Random.Range(0, availableBiomes.Count)],
                Level = UnityEngine.Random.Range(1, 3),
                DamageMultiplier = 1f,
                generated = true
            };
            data.Cost.Add("Coins", UnityEngine.Random.Range(10, 20));
            output.Add(data);
        }
        return output;
    }

    private static int GetReward(string creatureName) => CritterHelper.GetBiome(creatureName) switch
    {
        Heightmap.Biome.Meadows => 1,
        Heightmap.Biome.BlackForest => 3,
        Heightmap.Biome.Swamp => 5,
        Heightmap.Biome.Mountain => 10,
        Heightmap.Biome.Plains => 15,
        Heightmap.Biome.Mistlands => 20,
        Heightmap.Biome.AshLands => 25,
        _ => 1
    };
}