using HarmonyLib;
using UnityEngine;

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
}