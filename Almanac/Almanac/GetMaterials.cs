// using System.Collections.Generic;
// using System.Linq;
// using HarmonyLib;
// using JetBrains.Annotations;
// using UnityEngine;
//
// namespace Almanac.Almanac;
//
// public static class GetMaterials
// {
//     public static List<Material> allMats = null!;
//
//     [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
//     static class GetAllMats
//     {
//         private static void Postfix(ZNetScene __instance)
//         {
//             allMats = Resources.FindObjectsOfTypeAll<Material>().ToList();
//
//             foreach (var mat in allMats)
//             {
//                 AlmanacPlugin.AlmanacLogger.LogWarning(mat.name);
//             }
//         }
//     }
// }