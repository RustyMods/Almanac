
using System;
using Almanac.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Almanac.TreasureHunt;

public class TreasureHunt : MonoBehaviour
{
    public static Data.TreasureLocation? ActiveTreasureLocation;
    
    private const float maxRadius = 9500f;
    private const float minSpawnDistance = 2f;
    private const float maxYDistance = 20f;
    private const int solidHeightMargin = 1000;
    private const float spawnOffset = 10f;

    public Data.TreasureData data = null!;
    public DropTable DropTable = null!;
    public DropOnDestroyed _DropOnDestroyed = null!;
    public ZNetView _znv = null!;

    public Minimap.PinData? pin;

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
        if (ActiveTreasureLocation != null)
        {
            if (ActiveTreasureLocation.m_data.m_name == treasureLocation.m_data.m_name)
            {
                Minimap.instance.RemovePin(ActiveTreasureLocation.m_pin);
                ActiveTreasureLocation = null;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Canceled Treasure hunt");
                UpdateAlmanac.UpdateTreasurePanel();
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Already have active treasure hunt");
            }
            return false;
        }

        if (!FindSpawnLocation(treasureLocation.m_biome, out Vector3 pos)) return false;
        treasureLocation.m_pos = pos;
        Minimap.PinData treasurePin = Minimap.instance.AddPin(pos, Minimap.PinType.Boss, treasureLocation.m_data.m_name, false, false);
        treasurePin.m_icon = UpdateAlmanac.SelectedTreasure.m_sprite;
        treasurePin.m_worldSize = 100f;
        treasurePin.m_animate = true;
        
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Search for " + treasureLocation.m_data.m_name);

        treasureLocation.m_pin = treasurePin;
        ActiveTreasureLocation = treasureLocation;
        
        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added treasure hunt: " + treasureLocation.m_data.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + pos.x + " " + pos.z);
        return true;
    }

    public static bool SpawnTreasure(GameObject prefab, Vector3 point, float maxDistance, Data.TreasureData data)
    {
        for (int index = 0; index < 100; ++index)
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
                    continue;
                }
            }

            GameObject go = UnityEngine.Object.Instantiate(prefab, vector3, Quaternion.identity);
            TreasureHunt treasure = go.AddComponent<TreasureHunt>();
            HoverText hoverText = go.AddComponent<HoverText>();
            hoverText.m_text = data.m_name;
            treasure.DropTable = data.m_drops;
            treasure.data = data;
            return true;
        }

        return false;
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
        float x = UnityEngine.Random.Range(-maxRadius, maxRadius);
        float y = UnityEngine.Random.Range(0f, 5000f);
        float z = UnityEngine.Random.Range(-maxRadius, maxRadius);
        return new Vector3(x, y, z);
    }
    
    public static void OnClickTreasure()
    {
        if (AcceptTreasureHunt(new Data.TreasureLocation()
        {
            m_data = new Data.TreasureData()
            {
                m_name = UpdateAlmanac.SelectedTreasure.m_name,
                m_drops = UpdateAlmanac.SelectedTreasure.m_dropTable
            },
            m_biome = UpdateAlmanac.SelectedTreasure.m_biome
        }))
        {
            UpdateAlmanac.UpdateTreasurePanel();
        }
    }
}