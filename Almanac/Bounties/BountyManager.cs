using System;
using System.Collections.Generic;
using System.IO;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Bounties;

public static class BountyManager
{
    public static List<Data.ValidatedBounty> RegisteredBounties = new();
    public static List<Data.BountyYML> ValidatedBounties = new();
    
    public static void InitBounties()
    {
        if (AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Server) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Initializing Bounty Manager");
        
        RegisteredBounties.Clear();
        ValidatedBounties.Clear();
        
        AlmanacPaths.CreateFolderDirectories();

        if (ServerSyncedData.ServerBountyList.Value.IsNullOrWhiteSpace() || AlmanacPlugin.WorkingAsType is AlmanacPlugin.WorkingAs.Both) AddLocalBounties();
        else
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            List<Data.BountyYML> deserialized = deserializer.Deserialize<List<Data.BountyYML>>(ServerSyncedData.ServerBountyList.Value);
            foreach (Data.BountyYML bountyYML in deserialized)
            {
                if (!ValidateBounty(bountyYML, out Data.ValidatedBounty validatedBounty)) continue;
                RegisteredBounties.Add(validatedBounty);
            }
        }
    }

    private static void AddLocalBounties()
    {
        string[] paths = Directory.GetFiles(AlmanacPaths.BountyFolderPath, "*.yml");

        if (paths.Length <= 0)
        {
            RegisteredBounties = GetDefaultBounties();
        }
        else
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            foreach (string path in paths)
            {
                string data = File.ReadAllText(path);
                Data.BountyYML bountyData = deserializer.Deserialize<Data.BountyYML>(data);
                if (!ValidateBounty(bountyData, out Data.ValidatedBounty validatedBounty)) continue;
                RegisteredBounties.Add(validatedBounty);
                ValidatedBounties.Add(bountyData);
            }
        }
    }

    private static bool ValidateBounty(Data.BountyYML data, out Data.ValidatedBounty validatedData)
    {
        validatedData = new();
        if (data.bounty_name.IsNullOrWhiteSpace()) return false;
        validatedData.m_creatureName = data.bounty_name;
        
        GameObject critter = ZNetScene.instance.GetPrefab(data.creature_prefab_name);
        if (!critter)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to find bounty prefab: " + data.creature_prefab_name);
            return false;
        }

        if (!critter.GetComponent<MonsterAI>())
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Invalid bounty prefab: " + data.creature_prefab_name);
            return false;
        }
        
        validatedData.m_critter = critter;

        if (data.sprite_name.IsNullOrWhiteSpace()) return false;
        if (!SpriteManager.GetSprite(data.sprite_name, out Sprite? sprite))
        {
            GameObject? prefab = ObjectDB.instance.GetItemPrefab(data.sprite_name);
            if (!prefab) return false;
            if (!prefab.TryGetComponent(out ItemDrop itemDrop)) return false;
            validatedData.m_icon = itemDrop.m_itemData.GetIcon();
        }
        else
        {
            if (sprite == null) return false;
            validatedData.m_icon = sprite;
        }

        if (!Enum.TryParse(data.biome, out Heightmap.Biome biome)) return false;
        validatedData.m_biome = biome;
        validatedData.m_rewardType = data.reward_type;

        if (data.reward_type == Data.QuestRewardType.Item)
        {
            GameObject item = ZNetScene.instance.GetPrefab(data.item_reward.item_name);
            if (!item)
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Failed to find item: " + data.item_reward.item_name);
                return false;
            }

            if (!item.TryGetComponent(out ItemDrop itemDrop)) return false;
            validatedData.m_itemReward = itemDrop;
            validatedData.m_itemAmount = data.item_reward.amount;
        }

        if (data.reward_type == Data.QuestRewardType.Skill)
        {
            if (!Enum.TryParse(data.skill_reward.type, out Skills.SkillType skillType)) return false;
            validatedData.m_skill = skillType;
            validatedData.m_skillAmount = data.skill_reward.amount;
        }

        validatedData.m_health = data.bounty_health;
        validatedData.m_damageMultiplier = data.damage_multiplier;
        validatedData.m_damages = data.damages;

        return true;
    }

    private static List<Data.ValidatedBounty> GetDefaultBounties()
    {
        List<Data.BountyYML> defaultYmls = new()
        {
            new Data.BountyYML()
            {
                creature_prefab_name = "Boar",
                bounty_name = "Boar the Wretched",
                sprite_name = "TrophyBoar",
                biome = "Meadows",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Coins",
                    amount = 50,
                },
                bounty_health = 500f,
                damage_multiplier = 2f,
                damages = new Data.BountyDamages()
                {
                    blunt = 10f,
                    fire = 5f
                },
                level = 3
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Neck",
                bounty_name = "Neck the Wretched",
                sprite_name = "TrophyNeck",
                biome = "Meadows",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Coins",
                    amount = 50,
                },
                bounty_health = 500f,
                damage_multiplier = 2f,
                damages = new Data.BountyDamages()
                {
                    slash = 10f,
                    frost = 5f
                },
                level = 3
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Serpent",
                bounty_name = "Serpent the Wretched",
                sprite_name = "TrophySerpent",
                biome = "Ocean",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new()
                {
                    item_name = "SerpentMeat",
                    amount = 50
                },
                bounty_health = 1000f,
                damage_multiplier = 2f,
                damages = new(){blunt = 10f, lightning = 5f},
                level = 3
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Troll",
                bounty_name = "Troll the Wretched",
                sprite_name = "TrophyFrostTroll",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new()
                {
                    item_name = "SurtlingCore",
                    amount = 10
                },
                bounty_health = 1000f,
                damage_multiplier = 1.05f,
                damages = new(){blunt = 10f, frost = 5f},
                level = 3
            }
        };
        List<Data.ValidatedBounty> validated = new();
        foreach (Data.BountyYML data in defaultYmls)
        {
            if (!ValidateBounty(data, out Data.ValidatedBounty validate)) continue;
            validated.Add(validate);
            ISerializer serializer = new SerializerBuilder().Build();
            string serialized = serializer.Serialize(data);
            File.WriteAllText(AlmanacPaths.BountyFolderPath + Path.DirectorySeparatorChar + data.creature_prefab_name + "_bounty.yml", serialized);
        }

        return validated;
    }
}