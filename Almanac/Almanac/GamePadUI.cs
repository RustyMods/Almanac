using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Almanac.Almanac;

public static class GamePadUI
{
    [HarmonyPatch(typeof(EventSystem), nameof(EventSystem.SetSelectedGameObject), typeof(GameObject))]
    static class SetSelectedGameObjectPatch
    {
        private static void Postfix(EventSystem __instance, GameObject selected)
        {
            if (!__instance || !selected) return;
            if (selected.name != "Closebutton" || selected.transform.parent.name != "TrophiesFrame") return;
            // Fixes the problem of close button locking user out of almanac
            EventSystem.current.SetSelectedGameObject(
                InventoryGui.instance.m_trophiesPanel.transform.Find("TrophiesButton").gameObject);
            Almanac.CreateAlmanac.SetTopic("trophies");
            InventoryGui.instance.OnOpenTrophies();
        }
    }

    [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnSelect))]
    static class SelectablePatch
    {
        private static void Postfix(Selectable __instance)
        {
            Transform? iconObj = __instance.transform.Find("iconObj");
            if (iconObj)
            {
                __instance.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                Transform? hoverElement = __instance.transform.Find("hoverTextElement");
                if (hoverElement) hoverElement.gameObject.SetActive(true);
            }
            
            switch (__instance.name)
            {
                case "TrophyElement(Clone)":
                    Transform? icon = __instance.transform.Find("icon_bkg").Find("icon");
                    icon.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    break;
                case "ImageElement (icon)":
                    __instance.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnDeselect))]
    static class DeselectPatch
    {
        private static void Postfix(Selectable __instance)
        {
            Transform? iconObj = __instance.transform.Find("iconObj");
                
            if (iconObj)
            {
                __instance.transform.localScale = Vector3.one;
                Transform? hoverElement = __instance.transform.Find("hoverTextElement");
                if (hoverElement) hoverElement.gameObject.SetActive(false);
                return;
            }

            switch (__instance.name)
            {
                case "TrophyElement(Clone)":
                    Transform? icon = __instance.transform.Find("icon_bkg").Find("icon");
                    icon.localScale = Vector3.one;
                    break;
                case "ImageElement (icon)":
                    __instance.transform.localScale = Vector3.one;
                    break;
            }
        }
    }
}