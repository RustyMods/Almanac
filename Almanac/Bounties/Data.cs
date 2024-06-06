using System;
using UnityEngine;

namespace Almanac.Bounties;

public static class Data
{
    [Serializable]
    public class BountyData
    {
        public long m_hunter = 0L;
        public string m_name = "Bounty";
        public QuestRewardType m_rewardType = QuestRewardType.Item;
        public string m_skillType = "None";
        public int m_skillAmount = 0;
        public int m_rewardAmount = 0;
        public string m_rewardItem = "Coins";
        public float m_health = 100;
        public float m_damageMultiplier = 2f;
        public BountyDamages m_damages = new();
        public int m_level = 1;
        public int m_experience = 0;
    }

    public enum QuestRewardType
    {
        Item,
        Skill
    }

    public class BountyLocation
    {
        public BountyData data = null!;
        public GameObject m_critter = null!;
        public Heightmap.Biome m_biome = Heightmap.Biome.Meadows;
        public Vector3 m_position = Vector3.zero;
        public bool m_spawned = false;
        public Minimap.PinData m_pin = null!;
    }

    [Serializable]
    public class BountyYML
    {
        public string creature_prefab_name = null!;
        public string bounty_name = null!;
        public string sprite_name = null!;
        public string biome = "None";
        public QuestRewardType reward_type = QuestRewardType.Item;
        public BountyReward item_reward = new();
        public BountySkill skill_reward = new();
        public int experience_reward = 0;
        public float bounty_health = 1000f;
        public float damage_multiplier = 1f;
        public BountyDamages damages = new();
        public int level = 1;
        public string defeat_key = "";
        public int cost = 0;
        public string currency = "Coins";
    }

    public class ValidatedBounty
    {
        public GameObject m_critter = null!;
        public string m_creatureName = "";
        public Sprite m_icon = null!;
        public Heightmap.Biome m_biome = Heightmap.Biome.None;
        public QuestRewardType m_rewardType = QuestRewardType.Item;
        public ItemDrop? m_itemReward;
        public Skills.SkillType m_skill = Skills.SkillType.None;
        public int m_itemAmount = 0;
        public int m_skillAmount = 0;
        public float m_health = 1000f;
        public float m_damageMultiplier = 1f;
        public BountyDamages m_damages = new();
        public int m_level = 1;
        public string m_defeatKey = "";
        public int m_cost = 0;
        public ItemDrop m_currency = null!;
        public int m_experience = 0;
    }

    [Serializable]
    public class BountyReward
    {
        public string item_name = "";
        public int amount = 0;
    }

    [Serializable]
    public class BountySkill
    {
        public string type = "None";
        public int amount = 0;
    }

    [Serializable]
    public class BountyDamages
    {
        public float blunt = 0f;
        public float slash = 0f;
        public float pierce = 0f;
        public float fire = 0f;
        public float frost = 0f;
        public float lightning = 0f;
        public float poison = 0f;
        public float spirit = 0f;
    }

    [Serializable]
    public class BountyInfo
    {
        public string prefab = null!;
        public float health;
        public string key = null!;
        public string sprite = "";
    }
}