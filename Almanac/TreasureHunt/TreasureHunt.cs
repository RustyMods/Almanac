using System;
using Almanac.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Almanac.TreasureHunt;

public class TreasureHunt : MonoBehaviour
{
    public static Data.TreasureLocation? ActiveTreasureLocation;
    private static DateTime m_dateTime = DateTime.MaxValue;
    private const float maxRadius = 9500f;

    public DropTable DropTable = null!;
    public DropOnDestroyed _DropOnDestroyed = null!;
    public ZNetView _znv = null!;

    private Minimap.PinData? pin;

    public void Awake()
    {
        _znv = GetComponent<ZNetView>();
        _DropOnDestroyed = GetComponent<DropOnDestroyed>();

        AddPin();
    }

    public void Start()
    {
        _DropOnDestroyed.m_dropWhenDestroyed = DropTable;
        if (!_znv.IsValid()) return;
        _znv.GetZDO().Persistent = false;
    }

    public void OnDestroy()
    {
        if (pin != null) DestroyPin();
        ActiveTreasureLocation = null;
    }

    public void AddPin()
    {
        pin = Minimap.instance.AddPin(gameObject.transform.position, Minimap.PinType.EventArea, "", false, false);
        pin.m_worldSize = 100f;
        pin.m_animate = true;
    }

    public void DestroyPin() => Minimap.instance.RemovePin(pin);

    public static bool CancelTreasure(Data.Treasure data)
    {
        if (ActiveTreasureLocation == null) return false;
        ActiveTreasureLocation.Cancel();
        var component = Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
        component.Setup("Accept", () => { if (AcceptTreasure(data)) { Destroy(component.gameObject); }}, true);
        return true;
    }

    public static bool AcceptTreasure(Data.Treasure data)
    {
        if (m_dateTime != DateTime.MaxValue)
        {
            DateTime lastTreasure = m_dateTime + TimeSpan.FromMinutes(AlmanacPlugin._TreasureCooldown.Value);
            if (lastTreasure > DateTime.Now)
            {
                int difference = (lastTreasure - DateTime.Now).Minutes;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Treasure Available in: " + difference + "min");
                return false;
            }
        }
        if (data.m_cost > 0 && !Player.m_localPlayer.NoCostCheat())
        {
            if (!Player.m_localPlayer.GetInventory().HaveItem(data.m_currency.m_itemData.m_shared.m_name))
            {

                return false;
            }

            if (Player.m_localPlayer.GetInventory().CountItems(data.m_currency.m_itemData.m_shared.m_name) <
                data.m_cost)
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Not enough currency to buy treasure");
                return false;
            }

            Player.m_localPlayer.GetInventory().RemoveItem(data.m_currency.m_itemData.m_shared.m_name, data.m_cost);
        }

        if (!FindSpawnLocation(data.m_biome, out Vector3 pos)) return false;
        var treasureLocation = new Data.TreasureLocation(data, pos);
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Search for " + treasureLocation.m_data.m_name);
        m_dateTime = DateTime.Now;
        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added treasure hunt: " + treasureLocation.m_data.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + pos.x + " " + pos.z);
        var component = Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
        component.Setup("Cancel", () => { if (CancelTreasure(data)) Destroy(component.gameObject); }, true);
        return true;
    }
    
    private static bool FindSpawnLocation(Heightmap.Biome biome, out Vector3 pos)
    {
        pos = Vector3.zero;
        
        // Try get location within margin
        for (int index = 0; index < 1000; ++index)
        {
            Vector3 vector3 = GetRandomVectorWithin(Player.m_localPlayer.transform.position, 3000f);

            if (WorldGenerator.instance.GetBiome(vector3) != biome) continue;
            
            pos = vector3;
            return true;
        }
        // Else try get location entire world
        for (int index = 0; index < 1000; ++index)
        {
            Vector3 vector3 = GetRandomVector();

            if (WorldGenerator.instance.GetBiome(vector3) != biome) continue;
            
            pos = vector3;
            return true;
        }
        return false;
    }
    private static Vector3 GetRandomVectorWithin(Vector3 point, float margin)
    {
        Vector2 vector2 = Random.insideUnitCircle * margin;
        return point + new Vector3(vector2.x, 0.0f, vector2.y);
    }

    private static Vector3 GetRandomVector()
    {
        // var max = WorldGenerator.worldSize / 2;
        float x = Random.Range(-maxRadius, maxRadius);
        float y = Random.Range(0f, 5000f);
        float z = Random.Range(-maxRadius, maxRadius);
        return new Vector3(x, y, z);
    }
}