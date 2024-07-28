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

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    private static class PlayerUpdatePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            CheckBountyLocation(__instance);
            CheckTreasureLocation(__instance);
        }
    }

    private static void CheckTreasureLocation(Player instance)
    {
        if (TreasureHunt.TreasureHunt.ActiveTreasureLocation == null) return;
        if (TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_spawned) return;
        if (IsWithinQuestLocation(TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_pos, instance.transform.position, 100f))
        {
            Minimap.instance.RemovePin(TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_pin);
            GameObject barrel = ZNetScene.instance.GetPrefab("barrell");
            if (!barrel) return;
            if (TreasureHunt.TreasureHunt.SpawnTreasure(barrel, TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_pos, 50f, TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_data))
            {
                TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_spawned = true;
            }
            else
            {
                instance.Message(MessageHud.MessageType.Center, "Failed to spawn treasure loot, returning cost");
                TreasureHunt.TreasureHunt.ActiveTreasureLocation = null;
                TreasureHunt.TreasureHunt.ReturnCost(true);
            }
        }
    }

    private static void CheckBountyLocation(Player instance)
    {
        if (Bounty.ActiveBountyLocation == null) return;

        if (Bounty.ActiveBountyLocation.m_spawned) return;
            
        if (IsWithinQuestLocation(Bounty.ActiveBountyLocation.m_position, instance.transform.position, 100f))
        {
            Minimap.instance.RemovePin(Bounty.ActiveBountyLocation.m_pin);
                
            if (Bounty.SpawnCreature(Bounty.ActiveBountyLocation.m_critter,
                    instance.transform.position, 10f, Bounty.ActiveBountyLocation.data))
            {
                Bounty.ActiveBountyLocation.m_spawned = true;
            }
            else
            {
                instance.Message(MessageHud.MessageType.Center, "$almanac_failed_to_spawn_bounty");
                Bounty.ActiveBountyLocation = null;
            }
        }
    }

    private static bool IsWithinQuestLocation(Vector3 a, Vector3 b, float radius)
    {
        float num1 = a.x - b.x;
        float num2 = a.z - b.z;

        return Math.Sqrt(num1 * num1 + num2 * num2) <= radius;
    }
}