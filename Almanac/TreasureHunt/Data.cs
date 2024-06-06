using System;
using System.Collections.Generic;
using UnityEngine;

namespace Almanac.TreasureHunt;

public static class Data
{
    [Serializable]
    public class TreasureData
    {
        public string m_name = "";
        public DropTable m_drops = new();
    }

    public class TreasureLocation
    {
        public Heightmap.Biome m_biome;
        public TreasureData m_data = null!;
        public Vector3 m_pos;
        public Minimap.PinData m_pin = null!;
        public bool m_spawned;
    }

    public class ValidatedTreasure
    {
        public string m_name = null!;
        public Sprite m_sprite = null!;
        public ItemDrop m_currency = null!;
        public int m_cost = 0;
        public Heightmap.Biome m_biome = Heightmap.Biome.None;
        public DropTable m_dropTable = new();
        public int m_experience = 0;
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