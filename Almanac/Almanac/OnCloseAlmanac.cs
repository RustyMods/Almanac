using HarmonyLib;
using UnityEngine;

namespace Almanac.Almanac;

static class OnCloseAlmanac
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCloseTrophies))]
    static class OnCloseTrophiesPatch
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            if (AlmanacPlugin.WorkingAsType == AlmanacPlugin.WorkingAs.Server) return;
            
            Transform trophyFrame = __instance.m_trophiesPanel.transform.Find("TrophiesFrame");
            Transform contentPanel = trophyFrame.transform.Find("ContentPanel");
            Transform almanacList = contentPanel.transform.Find("AlmanacList");
            Transform welcomePanel = contentPanel.transform.Find("WelcomePanel (0)");
            Transform AlmanacElement = almanacList.transform.Find("AlmanacElement (0)");
            
            AlmanacElement.gameObject.SetActive(false);
            welcomePanel.gameObject.SetActive(true);
        }
    }
}