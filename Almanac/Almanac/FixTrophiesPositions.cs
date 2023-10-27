using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Almanac.Almanac;

static class FixTrophiesPositions
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateTrophyList))]
    static class UpdateTrophyListPatch
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
        
            List<GameObject> trophyList = __instance.m_trophyList;
            List<string> bossNames = new List<string>()
            {
                Localization.instance.Localize("$enemy_eikthyr"),
                Localization.instance.Localize("$enemy_gdking"),
                Localization.instance.Localize("$enemy_bonemass"),
                Localization.instance.Localize("$enemy_dragon"),
                Localization.instance.Localize("$enemy_goblinking"),
                Localization.instance.Localize("$enemy_seekerqueen")
            };
            HashSet<Vector3> uniqueVectorSet = new HashSet<Vector3>();
            
            foreach (GameObject trophy in trophyList)
            {
                Transform? trophyName = trophy.transform.Find("name");
                Vector3 trophyPos = trophy.transform.position;
                trophyName.TryGetComponent(out TextMeshProUGUI textMesh);
                if (!textMesh) continue;
                string panelDisplayName = textMesh.text;
                
                if (Localization.instance.Localize(panelDisplayName).ToLower().Contains("troll"))
                {
                    if (!(Math.Abs(trophyPos.x - 1010f) < 5f) || !(Math.Abs(trophyPos.y - 694f) < 5f)) continue;
                    trophy.transform.position = new Vector3(830f, 874f, 0.0f);
                };
                if (bossNames.Contains(Localization.instance.Localize(panelDisplayName))) uniqueVectorSet.Add(trophyPos);
                
            }
            
            foreach (GameObject trophy in trophyList)
            {
                Vector3 trophyPos = trophy.transform.position;
                Transform nameElement = trophy.transform.Find("name");
                nameElement.TryGetComponent(out TextMeshProUGUI textMesh);
                if (!textMesh) continue;
                string trophyName = textMesh.text;
                // Check if trophy positions are within the expected ranges
                if ((trophyPos.x - 110f) % 180f != 0f || (trophyPos.y - 154f) % 180f != 0f)
                {
                    // If false, then set it to forest troll position to then be moved
                    trophy.transform.position = new Vector3(830f, 874f, 0.0f);
                }
                // Check if position is unique
                if (uniqueVectorSet.Contains(trophyPos) 
                    && Localization.instance.Localize(trophyName).ToLower() != Localization.instance.Localize("draugr") 
                    && !bossNames.Contains(trophyName)
                    )
                {
                    // If false, then try to move trophy to empty slot
                    trophyPos = TryMoveTrophy(trophyPos, uniqueVectorSet);
                }
                // Add position to hash set
                uniqueVectorSet.Add(trophyPos);
                // Set trophy position
                trophy.transform.position = trophyPos;
            }
        }
        private static Vector2 TryMoveTrophy(Vector3 position, HashSet<Vector3> uniqueVectors)
        {
            float increment = 180f;
    
            float currentX = position.x;
            float currentY = position.y;
    
            // Increment position of trophy to next slot
            position = currentX + increment > 1190f 
                ? new Vector3(110f, currentY - increment, 0.0f) 
                : new Vector3(position.x + increment, currentY, 0.0f);
            // Check if slot is taken
            if (uniqueVectors.Contains(position)) position = TryMoveTrophy(position, uniqueVectors);
            // If slot is available, return position
            return position;
        }
    }
}