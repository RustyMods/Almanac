using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Almanac.Bounties;

public static class CachedEffects
{
    private static readonly List<Effect> m_effects = new();
    public static readonly Effect m_preSpawnEffects = new("vfx_prespawn", "sfx_prespawn");
    public static readonly Effect m_spawnEffects = new("vfx_spawn", "sfx_spawn");

    public class Effect
    {
        private readonly List<string> m_effectNames;
        private readonly EffectList m_effectList = new();
        public Effect(params string[] effectNames)
        {
            m_effectNames = effectNames.ToList();
            m_effects.Add(this);
        }

        public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform? baseParent = null, float scale = 1f, int variant = -1)
            => m_effectList.Create(basePos, baseRot, baseParent, scale, variant);

        public void Init()
        {
            if (!ZNetScene.instance) return;
            List<EffectList.EffectData> data = new();
            foreach (var effectName in m_effectNames)
            {
                if (ZNetScene.instance.GetPrefab(effectName) is not { } prefab) continue;
                data.Add(new EffectList.EffectData(){m_prefab = prefab});
            }

            m_effectList.m_effectPrefabs = data.ToArray();
        }
        
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_GetBountyAssets
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;
            foreach(var effect in m_effects) effect.Init();
        }
    }
}