using System;
using System.Collections.Generic;
using Fishlabs;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Almanac.Almanac.Almanac.CreateAlmanac;

namespace Almanac.Almanac;

public static class GamePadUI
{
    public static bool AlmanacActive;
    private static GameObject selectedObj = null!;

    [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnSelect))]
    static class SelectablePatch
    {
        private static void Postfix(Selectable __instance)
        {
            if (!__instance) return;
            // Save currently selected game object to trigger appropriate response
            selectedObj = __instance.gameObject;
            // If element has iconObj, set hover element visibility
            Transform? iconObj = __instance.transform.Find("iconObj");
            if (iconObj)
            {
                __instance.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                Transform? hoverElement = __instance.transform.Find("hoverTextElement");
                if (hoverElement) hoverElement.gameObject.SetActive(true);
            }
            // Special cases
            switch (__instance.name)
            {
                case "TrophyElement(Clone)": // Trophy Panel
                    Transform? icon = __instance.transform.Find("icon_bkg").Find("icon");
                    icon.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    break;
                case "ImageElement (icon)": // Achievement panel
                    __instance.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    break;
                case "AlmanacScrollBar": // Almanac Element
                    ElementSelectFrame.SetActive(true);
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnDeselect))]
    static class DeselectPatch
    {
        private static void Postfix(Selectable __instance)
        {
            if (!__instance) return;
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
                case "AlmanacScrollBar":
                    ElementSelectFrame.SetActive(false);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(ZInput), nameof(ZInput.OnInput))]
    static class ZInputPatch
    {
        private static DateTime lastInput;
        private static KeyCode lastKey;
        private static void Postfix(ZInput __instance, ZInput.InputSource inputSource)
        {
            if (!ZNetScene.instance) return;
            if (inputSource is not ZInput.InputSource.Gamepad) return;
            if (__instance.GetPressedKey() is KeyCode.None) return;
            
            if (!AlmanacActive) return;
            KeyCode keyCode = __instance.GetPressedKey();
            // Since key input triggers many times, throttle the response
            if (lastInput + new TimeSpan(0, 0, 0, 0, 500) > __instance.m_inputTimerGamepad && lastKey == keyCode) return;

            lastInput = __instance.m_inputTimerGamepad;
            lastKey = keyCode;
            
            switch (keyCode)
            {
                case KeyCode.JoystickButton4: // Left tab
                    switch (selectedObj.name)
                    {
                        case "jewelcraftingButton": EventSystem.current.SetSelectedGameObject(fishTab); break; 
                        case "fishButton": EventSystem.current.SetSelectedGameObject(ammoTab); break; 
                        case "ammoButton": EventSystem.current.SetSelectedGameObject(weaponTab); break; 
                        case "weaponButton": EventSystem.current.SetSelectedGameObject(gearTab); break; 
                        case "gearButton": EventSystem.current.SetSelectedGameObject(consumeTab); break; 
                        case "ConsummableButton": EventSystem.current.SetSelectedGameObject(materialTab); break; 
                        case "MaterialButton": EventSystem.current.SetSelectedGameObject(trophyTab); break; 
                        case "TrophiesButton": EventSystem.current.SetSelectedGameObject(creatureTab); break; 
                        case "CreatureButton": EventSystem.current.SetSelectedGameObject(achievementTab); break; 
                        case "achievementsButton": EventSystem.current.SetSelectedGameObject(metricTab); break; 
                        case "playerStatsButton": EventSystem.current.SetSelectedGameObject(miscTab); break; 
                        case "miscPiecesButton": EventSystem.current.SetSelectedGameObject(craftTab); break; 
                        case "craftingPiecesButton": EventSystem.current.SetSelectedGameObject(buildTab); break; 
                        case "buildPiecesButton": EventSystem.current.SetSelectedGameObject(furnitureTab); break; 
                        case "furniturePiecesButton": EventSystem.current.SetSelectedGameObject(comfortTab); break; 
                        case "comfortPiecesButton": EventSystem.current.SetSelectedGameObject(plantTab); break; 
                        case "plantPiecesButton": EventSystem.current.SetSelectedGameObject(otherTab); break;
                        case "defaultPiecesButton": EventSystem.current.SetSelectedGameObject(modTab ? modTab : jewelTab ? jewelTab : fishTab); break; 
                        case "modPiecesButton": EventSystem.current.SetSelectedGameObject(jewelTab ? jewelTab : fishTab); break;
                        default: EventSystem.current.SetSelectedGameObject(fishTab); break;
                    }
                    break;
                case KeyCode.JoystickButton5: // Right tab
                    EventSystem.current.SetSelectedGameObject(AlmanacScrollBar);
                    break;
                case KeyCode.JoystickButton2: // X button
                    EventSystem.current.SetSelectedGameObject(TrophyListScroll);
                    break;
            }
        }
    }
}