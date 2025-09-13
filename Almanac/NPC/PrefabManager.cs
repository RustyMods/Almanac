using System.Collections.Generic;
using Almanac.NPC;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac.Managers;

[PublicAPI]
public static class PrefabManager
{
    internal static List<GameObject> PrefabsToRegister = new();
    internal static List<Clone> Clones = new();
    
    static PrefabManager()
    {
        Harmony harmony = new("org.bepinex.helpers.Almanac.PrefabManager");
        harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_FejdStartup))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ZNetScene_Awake))));
    }

    public static void RegisterPrefab(GameObject? prefab)
    {
        if (prefab == null) return;
        PrefabsToRegister.Add(prefab);
    }
    public static void RegisterPrefab(string assetBundleName, string prefabName) => RegisterPrefab(AssetBundleManager.LoadAsset<GameObject>(assetBundleName, prefabName));
    public static void RegisterPrefab(AssetBundle assetBundle, string prefabName) =>  RegisterPrefab(assetBundle.LoadAsset<GameObject>(prefabName));

    [HarmonyPriority(Priority.VeryHigh)]
    internal static void Patch_ZNetScene_Awake(ZNetScene __instance)
    {
        foreach (GameObject prefab in PrefabsToRegister)
        {
            if (!prefab.GetComponent<ZNetView>()) continue;
            __instance.m_prefabs.Add(prefab);
        }
    }

    [HarmonyPriority(Priority.VeryHigh)]
    internal static void Patch_FejdStartup(FejdStartup __instance)
    {
        Helpers._ZNetScene = __instance.m_objectDBPrefab.GetComponent<ZNetScene>();
        Helpers._ObjectDB = __instance.m_objectDBPrefab.GetComponent<ObjectDB>();
        foreach(Clone? clone in Clones) clone.Create();
        PieceManager.BuildPiece.Patch_FejdStartup(__instance);
    }
}