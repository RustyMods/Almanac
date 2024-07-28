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
    private const float minSpawnDistance = 2f;
    private const float maxYDistance = 20f;
    private const int solidHeightMargin = 1000;
    private const float spawnOffset = 10f;

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

    private static bool AcceptTreasureHunt(Data.TreasureLocation treasureLocation)
    {
        if (!FindSpawnLocation(treasureLocation.m_biome, out Vector3 pos)) return false;
        treasureLocation.m_pos = pos;
        Minimap.PinData treasurePin = Minimap.instance.AddPin(pos, Minimap.PinType.Boss, treasureLocation.m_data.m_name, false, false);
        treasurePin.m_icon = UpdateAlmanac.SelectedTreasure.m_sprite;
        treasurePin.m_worldSize = 100f;
        treasurePin.m_animate = true;
        
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Search for " + treasureLocation.m_data.m_name);

        treasureLocation.m_pin = treasurePin;
        ActiveTreasureLocation = treasureLocation;
        m_dateTime = DateTime.Now;
        
        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added treasure hunt: " + treasureLocation.m_data.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + pos.x + " " + pos.z);
        return true;
    }

    public static bool SpawnTreasure(GameObject prefab, Vector3 point, float maxDistance, Data.TreasureData data)
    {
        Vector3 vector3 = GetRandomVectorWithin(point, maxDistance);
        if (WorldGenerator.instance.GetBiome(vector3) == Heightmap.Biome.Ocean)
        {
            vector3.y = ZoneSystem.instance.m_waterLevel;
        }
        else
        {
            ZoneSystem.instance.GetSolidHeight(vector3, out float height, solidHeightMargin);
            if (height >= 0.0 && Mathf.Abs(height - point.y) <= maxYDistance &&
                Vector3.Distance(vector3, point) >= minSpawnDistance)
            {
                vector3.y = height + spawnOffset;
            }
            else
            {
                vector3.y = Player.m_localPlayer.transform.position.y + spawnOffset;
            }
        }

        GameObject go = Instantiate(prefab, vector3, Quaternion.identity);
        if (!go) return false;
        go.AddComponent<TreasureHunt>().DropTable = data.m_drops;
        go.AddComponent<HoverText>().m_text = data.m_name;
        go.AddComponent<Beacon>().m_range = 50f;
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
        var max = WorldGenerator.worldSize / 2;
        float x = Random.Range(-maxRadius, maxRadius);
        float y = Random.Range(0f, 5000f);
        float z = Random.Range(-maxRadius, maxRadius);
        return new Vector3(x, y, z);
    }
    
    public static void OnClickTreasure()
    {
        if (!CheckForActiveTreasureHunt()) return;
        if (!CheckForTreasureCost()) return;
        
        if (AcceptTreasureHunt(new Data.TreasureLocation()
        {
            m_data = new Data.TreasureData()
            {
                m_name = UpdateAlmanac.SelectedTreasure.m_name,
                m_drops = UpdateAlmanac.SelectedTreasure.m_dropTable,
            },
            m_biome = UpdateAlmanac.SelectedTreasure.m_biome
        }))
        {
            UpdateAlmanac.UpdateTreasurePanel();
        }
        else
        {
            // Return cost of treasure hunt
            if (UpdateAlmanac.SelectedTreasure.m_cost > 0)
            {
                ReturnCost();
            }
        }
    }

    private static bool CheckForActiveTreasureHunt()
    {
        if (m_dateTime != DateTime.MaxValue)
        {
            DateTime lastAccept = m_dateTime + TimeSpan.FromMinutes(AlmanacPlugin._TreasureCooldown.Value);
            if (lastAccept > DateTime.Now)
            {
                int difference = (lastAccept - DateTime.Now).Minutes;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_treasure_available {difference} $info_minutes");
                return false;
            }
        }
        if (ActiveTreasureLocation == null) return true;
        if (ActiveTreasureLocation.m_data.m_name == UpdateAlmanac.SelectedTreasure.m_name)
        {
            Minimap.instance.RemovePin(ActiveTreasureLocation.m_pin);
            ActiveTreasureLocation = null;
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_canceled_treasure");
            UpdateAlmanac.UpdateTreasurePanel();
            if (UpdateAlmanac.SelectedTreasure.m_cost > 0)
            {
                ReturnCost();
            }
        }
        else
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_active_treasure");
        }

        return false;
    }

    public static void ReturnCost(bool enabled = false)
    {
        if (!enabled) return;
        Inventory? inventory = Player.m_localPlayer.GetInventory();
        if (!inventory.CanAddItem(UpdateAlmanac.SelectedTreasure.m_currency.m_itemData, UpdateAlmanac.SelectedTreasure.m_cost)) return;
        ItemDrop.ItemData? item = UpdateAlmanac.SelectedTreasure.m_currency.m_itemData.Clone();
        item.m_stack = UpdateAlmanac.SelectedTreasure.m_cost;
        inventory.AddItem(item);
    }

    private static bool CheckForTreasureCost()
    {
        if (UpdateAlmanac.SelectedTreasure.m_cost <= 0) return true;
        Inventory? inventory = Player.m_localPlayer.GetInventory();
        if (!inventory.HaveItem(UpdateAlmanac.SelectedTreasure.m_currency.m_itemData.m_shared.m_name)) return false;
        ItemDrop.ItemData? item = inventory.GetItem(UpdateAlmanac.SelectedTreasure.m_currency.m_itemData.m_shared.m_name);
        if (item == null)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to get currency item from inventory");
            return false;
        }

        if (item.m_stack > UpdateAlmanac.SelectedTreasure.m_cost)
        {
            item.m_stack -= UpdateAlmanac.SelectedTreasure.m_cost;
            return true;
        }
        if (item.m_stack == UpdateAlmanac.SelectedTreasure.m_cost)
        {
            inventory.RemoveItem(item);
            return true;
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$info_not_enough " + UpdateAlmanac.SelectedTreasure.m_currency.m_itemData.m_shared.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Not enough currency to buy treasure");
        return false;
    }
}