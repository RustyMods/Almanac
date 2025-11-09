using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.TreasureHunt;

public class TreasureHunt : MonoBehaviour
{
    public DropOnDestroyed m_destructible = null!;
    public ZNetView m_nview = null!;
    private Minimap.PinData? pin;
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_destructible = GetComponent<DropOnDestroyed>();
    }
    public void Start()
    {
        AddPin();
        if (!m_nview.IsValid()) return;
        string? data = m_nview.GetZDO().GetString(ZDOVars.s_drops);
        if (!string.IsNullOrEmpty(data))
        {
            m_destructible.m_dropWhenDestroyed.m_drops.Clear();
            ZPackage pkg = new ZPackage(data);
            int count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string? item = pkg.ReadString();
                int min = pkg.ReadInt();
                int max = pkg.ReadInt();
                double weight = pkg.ReadDouble();

                if (ZNetScene.instance.GetPrefab(item) is not { } prefab) continue;
                m_destructible.m_dropWhenDestroyed.m_drops.Clear();
                m_destructible.m_dropWhenDestroyed.m_drops.Add(new DropTable.DropData()
                {
                    m_item = prefab,
                    m_stackMin = min,
                    m_stackMax = max,
                    m_weight = Mathf.Max((float)weight, 0.1f),
                });
            }

            m_destructible.m_dropWhenDestroyed.m_dropChance = 1f;
            m_destructible.m_dropWhenDestroyed.m_dropMin = count;
            m_destructible.m_dropWhenDestroyed.m_dropMax = count;
        }
    }
    public void OnDestroy()
    {
        if (pin != null) Minimap.instance.RemovePin(pin);
        TreasureManager.ActiveTreasureLocation = null;
    }
    public void AddPin()
    {
        pin = Minimap.instance.AddPin(gameObject.transform.position, Minimap.PinType.EventArea, "", false, false);
        pin.m_worldSize = 100f;
        pin.m_animate = true;
    }
}