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
    
    public static void InitBounties(bool first = true)
    {
        if (first) AlmanacPlugin.AlmanacLogger.LogDebug("Client: Initializing Bounty Manager");
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Reloading bounties");
        }
        
        RegisteredBounties.Clear();
        ValidatedBounties.Clear();
        
        AlmanacPaths.CreateFolderDirectories();

        if (ServerSyncedData.ServerBountyList.Value.IsNullOrWhiteSpace() || AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client) AddLocalBounties();
        else
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            List<Data.BountyYML> deserialized = deserializer.Deserialize<List<Data.BountyYML>>(ServerSyncedData.ServerBountyList.Value);
            foreach (Data.BountyYML bountyYML in deserialized)
            {
                if (!ValidateBounty(bountyYML, out Data.ValidatedBounty validatedBounty)) continue;
                RegisteredBounties.Add(validatedBounty);
                ValidatedBounties.Add(bountyYML);
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
        
        validatedData.m_critter = critter;
        validatedData.m_creatureName = data.bounty_name;
        validatedData.m_biome = biome;
        validatedData.m_rewardType = data.reward_type;
        validatedData.m_health = data.bounty_health;
        validatedData.m_damageMultiplier = data.damage_multiplier;
        validatedData.m_damages = data.damages;
        validatedData.level = data.level;

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
                level = 3,
                defeat_key = "defeated_boar"
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
                level = 3,
                defeat_key = "defeated_neck"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Eikthyr",
                bounty_name = "Eikthyr the Undead",
                sprite_name = "TrophyEikthyr",
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
                level = 3,
                defeat_key = "defeated_eikthyr"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Skeleton",
                bounty_name = "Skeleton the Undead",
                sprite_name = "TrophySkeleton",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Coins",
                    amount = 50,
                },
                bounty_health = 200f,
                damage_multiplier = 2f,
                level = 3,
                defeat_key = "defeated_skeleton"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Greydwarf",
                bounty_name = "Greydwarf the Burdened",
                sprite_name = "TrophyGreydwarf",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Coins",
                    amount = 50,
                },
                bounty_health = 200f,
                damage_multiplier = 2f,
                level = 3,
                defeat_key = "defeated_greydwarf"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Greydwarf_Elite",
                bounty_name = "Greydwarf the Mad",
                sprite_name = "TrophyGreydwarfBrute",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Coins",
                    amount = 50,
                },
                bounty_health = 200f,
                damage_multiplier = 2f,
                level = 3,
                defeat_key = "defeated_greydwarf_elite"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Greydwarf_Shaman",
                bounty_name = "Greydwarf the Mystic",
                sprite_name = "TrophyGreydwarfShaman",
                biome = "Swamp",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Amber",
                    amount = 20
                },
                bounty_health = 100f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { blunt = 10f, frost = 5f },
                level = 2,
                defeat_key = "defeated_greydwarf_shaman"
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
                level = 3,
                defeat_key = "KilledTroll"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Ghost",
                bounty_name = "Ghost the Haunting",
                sprite_name = "skull",
                biome = "Mistlands",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Clubs",
                    amount = 5000
                },
                bounty_health = 60f,
                damage_multiplier = 1.0f,
                damages = new Data.BountyDamages() { slash = 5f, frost = 10f },
                level = 1,
                defeat_key = "defeated_ghost"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "gd_king",
                bounty_name = "Elder the Ancient",
                sprite_name = "TrophyTheElder",
                biome = "Swamp",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Polearms",
                    amount = 15000
                },
                bounty_health = 2500f,
                damage_multiplier = 1.8f,
                damages = new Data.BountyDamages() { blunt = 50f, pierce = 20f },
                level = 3,
                defeat_key = "defeated_gdking"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Leech",
                bounty_name = "Leech the Bloodsucker",
                sprite_name = "TrophyLeech",
                biome = "Swamp",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Bloodbag",
                    amount = 50
                },
                bounty_health = 60f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { pierce = 10f, poison = 5f },
                level = 2,
                defeat_key = "defeated_leech"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Draugr",
                bounty_name = "Draugr the Undead",
                sprite_name = "TrophyDraugr",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "IronScrap",
                    amount = 10
                },
                bounty_health = 100f,
                damage_multiplier = 1.2f,
                damages = new Data.BountyDamages() { slash = 15f, frost = 5f },
                level = 2,
                defeat_key = "defeated_draugr"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Draugr_Elite",
                bounty_name = "Draugr the Warden",
                sprite_name = "TrophyDraugrElite",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "CopperScrap",
                    amount = 15
                },
                bounty_health = 200f,
                damage_multiplier = 1.5f,
                damages = new Data.BountyDamages() { slash = 20f, pierce = 10f },
                level = 3,
                defeat_key = "defeated_draugr_elite"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Draugr_Ranged",
                bounty_name = "Draugr the Archer",
                sprite_name = "TrophyDraugrFem",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "BronzeScrap",
                    amount = 20
                },
                bounty_health = 100f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { pierce = 15f, frost = 5f },
                level = 2,
                defeat_key = "defeated_draugr_ranged"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Blob",
                bounty_name = "Blob the Slime",
                sprite_name = "TrophyBlob",
                biome = "Swamp",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "FineWood",
                    amount = 30
                },
                bounty_health = 50f,
                damage_multiplier = 1.0f,
                damages = new Data.BountyDamages() { blunt = 10f, poison = 5f },
                level = 1,
                defeat_key = "defeated_blob"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "BlobElite",
                bounty_name = "Blob the Corrupted",
                sprite_name = "skull",
                biome = "Swamp",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Coins",
                    amount = 150
                },
                bounty_health = 150f,
                damage_multiplier = 1.3f,
                damages = new Data.BountyDamages() { blunt = 20f, poison = 10f },
                level = 3,
                defeat_key = "defeated_blobelite"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Wraith",
                bounty_name = "Wraith the Specter",
                sprite_name = "TrophyWraith",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Item,
                item_reward = new Data.BountyReward()
                {
                    item_name = "Crystal",
                    amount = 30
                },
                bounty_health = 100f,
                damage_multiplier = 1.2f,
                damages = new Data.BountyDamages() { slash = 15f, frost = 10f },
                level = 2,
                defeat_key = "defeated_wraith"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Skeleton_Poison",
                bounty_name = "Skeleton the Corrupted",
                sprite_name = "TrophySkeletonPoison",
                biome = "Plains",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Spears",
                    amount = 5000
                },
                bounty_health = 100f,
                damage_multiplier = 1.2f,
                damages = new Data.BountyDamages() { slash = 15f, poison = 10f },
                level = 2,
                defeat_key = "defeated_skeleton_poison"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Bonemass",
                bounty_name = "Bonemass the Decaying",
                sprite_name = "TrophyBonemass",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Clubs",
                    amount = 20000
                },
                bounty_health = 5000f,
                damage_multiplier = 2.0f,
                damages = new Data.BountyDamages() { blunt = 60f, poison = 30f },
                level = 3,
                defeat_key = "defeated_bonemass"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Wolf",
                bounty_name = "Wolf the Hunter",
                sprite_name = "TrophyWolf",
                biome = "Plains",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Clubs",
                    amount = 8000
                },
                bounty_health = 80f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { slash = 10f, pierce = 5f },
                level = 2,
                defeat_key = "defeated_wolf"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Fenring",
                bounty_name = "Fenring the Fierce",
                sprite_name = "TrophyFenring",
                biome = "Plains",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Axes",
                    amount = 10000
                },
                bounty_health = 300f,
                damage_multiplier = 1.3f,
                damages = new Data.BountyDamages() { slash = 20f, pierce = 10f },
                level = 3,
                defeat_key = "defeated_fenring"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Fenring_Cultist",
                bounty_name = "Fenring the Devoted",
                sprite_name = "TrophyCultist",
                biome = "Plains",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Blocking",
                    amount = 8000
                },
                bounty_health = 200f,
                damage_multiplier = 1.2f,
                damages = new Data.BountyDamages() { slash = 15f, pierce = 5f },
                level = 2,
                defeat_key = "defeated_fenring_cultist"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Ulv",
                bounty_name = "Ulv the Wild",
                sprite_name = "TrophyUlv",
                biome = "Mountain",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Polearms",
                    amount = 12000
                },
                bounty_health = 50f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { slash = 10f, pierce = 5f },
                level = 2,
                defeat_key = "defeated_ulv"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "StoneGolem",
                bounty_name = "Golem the Guardian",
                sprite_name = "TrophySGolem",
                biome = "Mountain",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Blocking",
                    amount = 15000
                },
                bounty_health = 800f,
                damage_multiplier = 1.5f,
                damages = new Data.BountyDamages() { blunt = 30f, pierce = 10f },
                level = 3,
                defeat_key = "defeated_stonegolem"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Bat",
                bounty_name = "Bat the Nocturnal",
                sprite_name = "skull",
                biome = "Mountain",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Bows",
                    amount = 5000
                },
                bounty_health = 10f,
                damage_multiplier = 1.0f,
                damages = new Data.BountyDamages() { pierce = 5f, poison = 2f },
                level = 3,
                defeat_key = "KilledBat"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Hatchling",
                bounty_name = "Drake the Youngling",
                sprite_name = "TrophyHatchling",
                biome = "Plains",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Knives",
                    amount = 3000
                },
                bounty_health = 100f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { slash = 15f, pierce = 5f },
                level = 2,
                defeat_key = "defeated_hatchling"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Dragon",
                bounty_name = "Dragon the Mighty",
                sprite_name = "TrophyDragonQueen",
                biome = "Mountain",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Axes",
                    amount = 25000
                },
                bounty_health = 7500f,
                damage_multiplier = 2.5f,
                damages = new Data.BountyDamages() { slash = 80f, fire = 50f },
                level = 3,
                defeat_key = "defeated_dragon"
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
                level = 3,
                defeat_key = "defeated_serpent"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Goblin",
                bounty_name = "Goblin the Menace",
                sprite_name = "TrophyGoblin",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Spears",
                    amount = 5000
                },
                bounty_health = 175f,
                damage_multiplier = 1.2f,
                damages = new Data.BountyDamages() { slash = 20f, pierce = 10f },
                level = 2,
                defeat_key = "defeated_goblin"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "GoblinBrute",
                bounty_name = "Goblin the Bulwark",
                sprite_name = "TrophyGoblinBrute",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Blocking",
                    amount = 10000
                },
                bounty_health = 800f,
                damage_multiplier = 1.5f,
                damages = new Data.BountyDamages() { blunt = 30f, pierce = 20f },
                level = 3,
                defeat_key = "defeated_goblinbrute"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "GoblinShaman",
                bounty_name = "Goblin the Hexer",
                sprite_name = "TrophyGoblinShaman",
                biome = "BlackForest",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Polearms",
                    amount = 8000
                },
                bounty_health = 100f,
                damage_multiplier = 1.1f,
                damages = new Data.BountyDamages() { slash = 10f, pierce = 5f },
                level = 2,
                defeat_key = "defeated_goblinshaman"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Lox",
                bounty_name = "Lox the Majestic",
                sprite_name = "TrophyLox",
                biome = "Plains",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Axes",
                    amount = 10000
                },
                bounty_health = 1000f,
                damage_multiplier = 1.5f,
                damages = new Data.BountyDamages() { blunt = 30f, pierce = 20f },
                level = 3,
                defeat_key = "defeated_lox"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Seeker",
                bounty_name = "Seeker the Elusive",
                sprite_name = "TrophySeeker",
                biome = "Mountain",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Spears",
                    amount = 8000
                },
                bounty_health = 200f,
                damage_multiplier = 1.2f,
                damages = new Data.BountyDamages() { pierce = 20f, spirit = 10f },
                level = 2,
                defeat_key = "defeated_seeker"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "SeekerBrute",
                bounty_name = "Seeker the Dominator",
                sprite_name = "TrophySeekerBrute",
                biome = "Mountain",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Polearms",
                    amount = 12000
                },
                bounty_health = 1500f,
                damage_multiplier = 1.8f,
                damages = new Data.BountyDamages() { pierce = 40f, spirit = 20f, blunt = 20f },
                level = 3,
                defeat_key = "defeated_seekerbrute"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "SeekerQueen",
                bounty_name = "Queen the Monarch",
                sprite_name = "TrophySeekerQueen",
                biome = "Mistlands",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Spears",
                    amount = 20000
                },
                bounty_health = 12500f,
                damage_multiplier = 2.0f,
                damages = new Data.BountyDamages() { pierce = 60f, spirit = 30f, poison = 20f },
                level = 3,
                defeat_key = "defeated_seekerqueen"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Skeleton_Hildir_nochest",
                bounty_name = "Brenna the Wicked",
                sprite_name = "TrophySkeletonHildir",
                biome = "AshLands",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Swords",
                    amount = 10000
                },
                bounty_health = 2400,
                damage_multiplier = 1.1f,
                level = 3,
                defeat_key = "BossHildir1"
            },
            new Data.BountyYML()
            {
                creature_prefab_name = "Troll",
                bounty_name = "Troll the Frozen",
                sprite_name = "TrophyFrostTroll",
                biome = "DeepNorth",
                reward_type = Data.QuestRewardType.Skill,
                skill_reward = new Data.BountySkill()
                {
                    type = "Knives",
                    amount = 20000
                },
                bounty_health = 3000f,
                damage_multiplier = 1.5f,
                damages = new(){blunt = 10f, frost = 50f},
                level = 3,
                defeat_key = "defeated_goblinking"
            },
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