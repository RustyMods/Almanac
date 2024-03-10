using UnityEngine;

namespace Almanac.TreasureHunt;

public static class CacheLootBox
{
    public static GameObject? GetBarrelPrefab()
    {
        GameObject barrel = ZNetScene.instance.GetPrefab("barrell");
        if (!barrel)
        {
            Debug.LogWarning("failed to get barrel");
            return null;
        }
        return barrel;
    }
}