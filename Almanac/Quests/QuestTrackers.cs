using System;
using Almanac.UI;
using HarmonyLib;
using JetBrains.Annotations;

namespace Almanac.Quests;

public static class QuestTrackers
{
    [HarmonyPatch(typeof(Player),nameof(Player.OnInventoryChanged))]
    public static class Player_OnInventoryChanged_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance)
        {
            if (__instance != Player.m_localPlayer || __instance.m_isLoading) return;
            try
            {
                if (DialoguePanel.IsVisible()) return; // problem updating UI when dialogue modifies quest UI
                
                foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Collect))
                {
                    int count = __instance.GetInventory().CountItems(quest.GetSharedName());
                    quest.SetProgress(count);
                }

                foreach (QuestManager.Quest quest in QuestManager.GetQuestsByType(QuestType.LearnItems))
                {
                    int itemCount = 0;
                    foreach (string sharedName in quest.GetSharedNames())
                    {
                        if (__instance.IsKnownMaterial(sharedName)) ++itemCount;
                    }
                    quest.SetProgress(itemCount);
                }
            }
            catch (Exception ex)
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Almanac.QuestTracker.Player.OnInventoryChanged: Failed to update quests");
                AlmanacPlugin.AlmanacLogger.LogError(ex.Message);
            }
        }
    }
    
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Destroy))]
    public class Destructible_Destroy_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Destructible __instance, HitData? hit)
        {
            if (DialoguePanel.IsVisible()) return;
            if (hit?.GetAttacker() is not { } attacker || attacker != Player.m_localPlayer) return;
            string normalizedName = __instance.name.Replace("Clone)",string.Empty);
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Mine))
            {
                if (quest.prefabName != normalizedName) continue;
                quest.Increment();
            }
        }
    }

    [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.Damage))]
    public static class MineRock5_Damage_Patch
    {
        [UsedImplicitly]
        private static void Postfix(MineRock5 __instance, HitData? hit)
        {
            if (DialoguePanel.IsVisible()) return;
            if (hit?.GetAttacker() is not { } attacker || attacker != Player.m_localPlayer) return;
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Mine))
            {
                if (quest.GetSharedName() != __instance.m_name) continue;
                quest.Increment();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    public static class PlayerPlacePiece_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance, Piece piece)
        {
            if (DialoguePanel.IsVisible()) return;
            if (__instance != Player.m_localPlayer) return;
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Farm))
            {
                if (quest.prefabName != piece.name) continue;
                quest.Increment();
            }
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    public static class Pickable_Interact_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Pickable __instance, Humanoid character)
        {
            if (DialoguePanel.IsVisible()) return;
            if (character != Player.m_localPlayer) return;
            if (!__instance.m_nview.IsValid() || __instance.m_enabled == 0) return;
            if (__instance.m_tarPreventsPicking && (__instance.m_floating == null || __instance.m_floating.IsInTar())) return;
            string normalizedName = __instance.name.Replace("(Clone)",string.Empty);
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Harvest))
            {
                if (quest.prefabName != normalizedName) continue;
                quest.Increment();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnJump))]
    public static class Player_OnJump_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance)
        {
            if (DialoguePanel.IsVisible()) return;
            if (__instance != Player.m_localPlayer) return;
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Jump))
            {
                quest.Increment();
            }
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Tame))]
    public static class Tameable_Tame_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Tameable __instance)
        {
            if (DialoguePanel.IsVisible()) return;
            Player? closestPlayer = Player.GetClosestPlayer(__instance.transform.position, 10f);
            if (closestPlayer == null) return;
            if (closestPlayer != Player.m_localPlayer) return;
            var normalizedName = __instance.name.Replace("(Clone)",string.Empty);
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Tame))
            {
                if (quest.prefabName != normalizedName) continue;
                quest.Increment();
            }
        }
    }
    
    [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Destroy), typeof(HitData))]
    public static class TreeLog_Destroy_Patch
    {
        [UsedImplicitly]
        private static void Prefix(TreeLog __instance, HitData? hitData)
        {
            if (DialoguePanel.IsVisible()) return;
            if (hitData?.GetAttacker() is not { } attacker || attacker != Player.m_localPlayer) return;
            // beech_log
            string normalizedName = __instance.name.Replace("(Clone)",string.Empty);
            foreach (QuestManager.Quest? quest in QuestManager.GetQuestsByType(QuestType.Chop))
            {
                if (quest.prefabName != normalizedName) continue;
                quest.Increment();
            }
        }
    }
}