using System;
using System.Collections.Generic;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace Almanac.TreasureHunt;

public static class Data
{
    public class TreasureLocation
    {
        public Vector3 m_pos;
        private readonly Minimap.PinData? m_pin;
        public bool m_spawned;
        public readonly Treasure m_data;

        public TreasureLocation(Treasure data, Vector3 pos)
        {
            m_data = data;
            if (TreasureHunt.ActiveTreasureLocation != null) TreasureHunt.ActiveTreasureLocation.Cancel();
            m_pos = pos;
            m_pin = Minimap.instance.AddPin(pos, Minimap.PinType.Boss, data.m_name, false, false);
            m_pin.m_icon = data.m_sprite;
            m_pin.m_worldSize = 100f;
            m_pin.m_animate = true;
            TreasureHunt.ActiveTreasureLocation = this;
        }

        public bool Spawn()
        {
            if (ZNetScene.instance.GetPrefab("barrell") is not { } barrel) return false;
            Vector3 vector3 = GetRandomVectorWithin(m_pos, 50f);
            if (WorldGenerator.instance.GetBiome(vector3) == Heightmap.Biome.Ocean)
            {
                vector3.y = ZoneSystem.instance.m_waterLevel;
            }
            else
            {
                ZoneSystem.instance.GetSolidHeight(vector3, out float height, 1000);
                if (height >= 0.0 && Mathf.Abs(height - m_pos.y) <= 20f &&
                    Vector3.Distance(vector3, m_pos) >= 2f)
                {
                    vector3.y = height + 10f;
                }
                else
                {
                    vector3.y = Player.m_localPlayer.transform.position.y + 10f;
                }
            }

            GameObject go = UnityEngine.Object.Instantiate(barrel, vector3, Quaternion.identity);
            go.AddComponent<TreasureHunt>().DropTable = m_data.m_dropTable;
            go.AddComponent<HoverText>().m_text = m_data.m_name;
            go.AddComponent<Beacon>().m_range = 50f;
            m_spawned = true;
            return true;
        }
        
        private static Vector3 GetRandomVectorWithin(Vector3 point, float margin)
        {
            Vector2 vector2 = Random.insideUnitCircle * margin;
            return point + new Vector3(vector2.x, 0.0f, vector2.y);
        }

        public void RemovePin()
        {
            if (m_pin == null) return;
            Minimap.m_instance.RemovePin(m_pin);
        }

        public void Cancel()
        {
            ReturnCost();
            Minimap.m_instance.RemovePin(m_pin);
            TreasureHunt.ActiveTreasureLocation = null;
        }

        public bool ReturnCost()
        {
            if (!Player.m_localPlayer.GetInventory().HaveEmptySlot()) return false;
            var item = m_data.m_currency.m_itemData.Clone();
            item.m_stack = m_data.m_cost;
            return Player.m_localPlayer.GetInventory().AddItem(item);
        }
        
    }

    public class Treasure
    {
        public readonly string m_name = null!;
        public readonly Sprite m_sprite = null!;
        public readonly ItemDrop m_currency = null!;
        public readonly int m_cost = 0;
        public readonly Heightmap.Biome m_biome = Heightmap.Biome.None;
        public readonly DropTable m_dropTable = new();
        public int m_experience = 0;

        public Treasure(TreasureYML data)
        {
            if (data.name.IsNullOrWhiteSpace()) return;
            if (data.biome == Heightmap.Biome.None) return;
            if (data.sprite_name.IsNullOrWhiteSpace()) return;
            if (SpriteManager.GetSprite(data.sprite_name, out Sprite? sprite))
            {
                if (sprite == null) return;
                m_sprite = sprite;
            }
            else
            {
                if (ObjectDB.instance.GetItemPrefab(data.sprite_name) is not { } item ||
                    !item.TryGetComponent(out ItemDrop component)) return;
                m_sprite = component.m_itemData.GetIcon();
            }
            
            foreach (var loot in data.loot)
            {
                if (ObjectDB.instance.GetItemPrefab(loot.item_name) is not { } item || !item.GetComponent<ItemDrop>()) continue;
                m_dropTable.m_drops.Add(new DropTable.DropData()
                {
                    m_item = item,
                    m_weight = loot.weight,
                    m_stackMax = loot.max_stack,
                    m_stackMin = loot.min_stack
                });
            }

            m_name = data.name;
            m_biome = data.biome;
            m_currency = TryGetCurrency(data);
            m_cost = data.cost;
            TreasureManager.RegisteredTreasure.Add(this);
            TreasureManager.ValidatedYML.Add(data);
        }
        
        private static ItemDrop TryGetCurrency(TreasureYML data)
        {
            GameObject currency = ObjectDB.instance.GetItemPrefab(data.currency);
            if (!currency) return GetDefaultCurrency();
            return currency.TryGetComponent(out ItemDrop currencyItem) ? currencyItem : GetDefaultCurrency();
        }
        
        private static ItemDrop GetDefaultCurrency()
        {
            GameObject coins = ObjectDB.instance.GetItemPrefab("Coins");
            return coins.GetComponent<ItemDrop>();
        }
    }

    [Serializable]
    public class TreasureYML
    {
        public string name = "";
        public string sprite_name = "";
        public string currency = "Coins";
        public int cost = 0;
        public Heightmap.Biome biome = Heightmap.Biome.None;
        public List<LootData> loot = new();
    }

    [Serializable]
    public class LootData
    {
        public string item_name = "";
        public int min_stack = 1;
        public int max_stack = 1;
        public float weight = 1f;
    }
}