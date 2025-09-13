using System;
using System.Collections.Generic;
using Almanac.Managers;
using Almanac.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Almanac.NPC;

public class Clone
{
    public static GameObject _root = null!;
    internal static readonly Dictionary<string, GameObject> registeredPrefabs = new();
    private GameObject? Prefab;
    private readonly string PrefabName;
    private readonly string NewName;
    private bool Loaded;
    public event Action<GameObject>? OnCreated;

    public Clone(string prefabName, string newName)
    {
        PrefabName = prefabName;
        NewName = newName;
        PrefabManager.Clones.Add(this);
    }

    internal void Create()
    {
        if (Loaded) return;
        if (Helpers.GetPrefab(PrefabName) is not { } prefab) return;
        Prefab = Object.Instantiate(prefab, _root.transform, false);
        Prefab.name = NewName;
        PrefabManager.RegisterPrefab(Prefab);
        OnCreated?.Invoke(Prefab);
        registeredPrefabs[Prefab.name] = Prefab;
        Loaded = true;
    }
}