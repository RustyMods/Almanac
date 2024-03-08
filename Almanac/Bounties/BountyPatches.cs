using System;
using HarmonyLib;
using UnityEngine;

namespace Almanac.Bounties;

public static class BountyPatches
{
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    private static class CharacterDamagePatch
    {
        private static void Prefix(HitData hit) => Bounty.ApplyBountyModifiers(hit);
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    private static class CharacterOnDeathPatch
    {
        private static void Prefix(Character __instance)
        {
            if (!__instance) return;
            Bounty.OnDeath(__instance);
            Bounty.ActiveBountyLocation = null;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    private static class PlayerUpdatePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;

            if (Bounty.ActiveBountyLocation == null) return;

            if (Bounty.ActiveBountyLocation.m_spawned) return;
            
            if (IsWithinBountyLocation(Bounty.ActiveBountyLocation.m_position, __instance.transform.position, 100f))
            {
                Minimap.instance.RemovePin(Bounty.ActiveBountyLocation.m_pin);
                
                if (Bounty.SpawnCreature(Bounty.ActiveBountyLocation.m_critter,
                        __instance.transform.position, 10f, Bounty.ActiveBountyLocation.data))
                {
                    Bounty.ActiveBountyLocation.m_spawned = true;
                }
                else
                {
                    __instance.Message(MessageHud.MessageType.Center, "$almanac_failed_to_spawn_bounty");
                    Bounty.ActiveBountyLocation = null;
                }
            }
        }
    }

    private static bool IsWithinBountyLocation(Vector3 a, Vector3 b, float radius)
    {
        float num1 = a.x - b.x;
        float num2 = a.z - b.z;

        return Math.Sqrt(num1 * num1 + num2 * num2) <= radius;
    }
}