using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Almanac.API;
using Almanac.Data;
using Almanac.FileSystem;
using Almanac.UI;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;
using Utility = Almanac.Utilities.Utility;

namespace Almanac.Bounties;

public class Bounty : MonoBehaviour
{
    public static Data.BountyLocation? ActiveBountyLocation;
    public static DateTime m_dateTime = DateTime.MaxValue;
    
    public static readonly int bountyHash = "QuestBounty".GetStableHashCode();
    private const float maxRadius = 9500f;
    private const float minSpawnDistance = 2f;
    private const float maxYDistance = 10f;
    private const int solidHeightMargin = 1000;
    private const float spawnOffset = 5f;
    public Character _character = null!;
    public ZNetView _znv = null!;
    public bool m_isDead;

    private Minimap.PinData pin = null!;
    public void Awake()
    {
        _znv = GetComponent<ZNetView>();
        _character = GetComponent<Character>();
        _character.m_onDeath += OnDeath;
        AddPin();
        _znv.GetZDO().Persistent = false;
    }
    public void Update() => UpdatePin();

    public void OnDestroy()
    {
        DestroyPin();
        if (!m_isDead)
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$almanac_bounty_escaped");
            ActiveBountyLocation = null;
        }
    }
    
    public void Start() => ApplyBountyData();

    public void AddPin()
    {
        pin = Minimap.instance.AddPin(gameObject.transform.position, Minimap.PinType.EventArea, "", false, false);
        pin.m_worldSize = 100f;
        pin.m_animate = true;
    }
    
    public void UpdatePin() => pin.m_pos = gameObject.transform.position;
    public void DestroyPin() => Minimap.instance.RemovePin(pin);

    public void ApplyBountyData()
    {
        if (GetData() is not { } data) return;
        ApplyCharacterData(data);
            
        if (_character.GetComponentInChildren<LevelEffects>() is {} levelEffects)
        {
            levelEffects.SetupLevelVisualization(data.m_level);
        }
            
        _znv.GetZDO().Set(ZDOVars.s_maxHealth, data.m_health);
        _znv.GetZDO().Set(ZDOVars.s_health, data.m_health);
        _znv.GetZDO().Set(ZDOVars.s_level, data.m_level);
    }

    private void ApplyCharacterData(Data.BountyData data)
    {
        _character.m_faction = Character.Faction.Boss;
        _character.m_boss = true;

        _character.m_name = data.m_name;
        _character.m_level = data.m_level;
    }

    public void OnDeath()
    {
        if (!_character) return;
        // if (_character.m_lastHit == null)
        // {
        //     ActiveBountyLocation = null;
        //     return;
        // }

        if (_character.m_lastHit?.GetAttacker() is Player player && GetData() is {} bountyData)
        {
            var killerID = player.GetPlayerID();
            if (Groups.API.FindGroupMemberByPlayerId(killerID) is { } group || killerID == bountyData.m_hunter)
            {
                switch (bountyData.m_rewardType)
                {
                    case Data.QuestRewardType.Item:
                        if (ZNetScene.instance.GetPrefab(bountyData.m_rewardItem) is { } item)
                        {
                            GameObject drop = Instantiate(item, _character.transform.position, Quaternion.identity);
                            if (!drop.TryGetComponent(out ItemDrop component)) break;
                            component.m_itemData.m_stack = bountyData.m_rewardAmount;
                        }
                        break;
                    case Data.QuestRewardType.Skill:
                        if (Enum.TryParse(bountyData.m_skillType, out Skills.SkillType skillType))
                        {
                            Player.m_localPlayer.RaiseSkill(skillType, bountyData.m_skillAmount);
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"$almanac_raised {Utility.ConvertSkills(skillType)} $almanac_by {bountyData.m_skillAmount} $almanac_xp");
                        }
                        break;
                }

                if (bountyData.m_experience > 0) ClassesAPI.AddEXP(bountyData.m_experience);
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Bounty stolen by: " + player.GetPlayerName());
            }
            
        }
        m_isDead = true;
        ActiveBountyLocation = null;
    }

    public static void ApplyBountyModifiers(HitData hit)
    {
        if (hit.GetAttacker() is not { } attacker || attacker.IsPlayer() || !attacker.m_nview.IsValid()) return;

        string bountyData = attacker.m_nview.GetZDO().GetString(bountyHash);
        if (bountyData.IsNullOrWhiteSpace()) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Data.BountyData data = deserializer.Deserialize<Data.BountyData>(bountyData);
            
        hit.ApplyModifier(data.m_damageMultiplier);
        hit.m_damage.m_blunt += data.m_damages.blunt;
        hit.m_damage.m_slash += data.m_damages.slash;
        hit.m_damage.m_pierce += data.m_damages.pierce;
        hit.m_damage.m_fire += data.m_damages.fire;
        hit.m_damage.m_frost += data.m_damages.frost;
        hit.m_damage.m_lightning += data.m_damages.lightning;
        hit.m_damage.m_poison += data.m_damages.poison;
        hit.m_damage.m_spirit += data.m_damages.spirit;
    }

    private Data.BountyData? GetData()
    {
        if (!_znv.IsValid()) return null;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string value = _znv.GetZDO().GetString(bountyHash);
        return value.IsNullOrWhiteSpace() ? null : deserializer.Deserialize<Data.BountyData>(value);
    }

    private static bool AcceptBounty(Data.BountyLocation bountyLocation)
    {
        if (ActiveBountyLocation != null)
        {
            if (ActiveBountyLocation.m_critter == bountyLocation.m_critter)
            {
                Minimap.instance.RemovePin(ActiveBountyLocation.m_pin);
                ActiveBountyLocation = null;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_canceled_bounty");
                UpdateAlmanac.UpdateBountyPanel();
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$almanac_already_have_active_bounty");
            }
            return false;
        }
        if (!FindSpawnLocation(bountyLocation.m_biome, out Vector3 pos)) return false;
        
        bountyLocation.m_position = pos;

        Minimap.PinData BountyPin = Minimap.instance.AddPin(pos, Minimap.PinType.Boss, bountyLocation.data.m_name, false, false);
        BountyPin.m_icon = UpdateAlmanac.SelectedBounty.m_icon;

        bountyLocation.m_pin = BountyPin;

        ActiveBountyLocation = bountyLocation;
        
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$almanac_hunt " + bountyLocation.data.m_name);
        
        m_dateTime = DateTime.Now;

        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added bounty: " + bountyLocation.data.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + pos.x + " " + pos.z);
        
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
            if (WorldGenerator.instance.GetBiomeArea(vector3) is not Heightmap.BiomeArea.Median) continue;
            pos = vector3;
            return true;
        }
        // Else try get location entire world
        for (int index = 0; index < 1000; ++index)
        {
            Vector3 vector3 = GetRandomVector();

            if (WorldGenerator.instance.GetBiome(vector3) != biome) continue;
            if (WorldGenerator.instance.GetBiomeArea(vector3) is not Heightmap.BiomeArea.Median) continue;
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

    public static bool SpawnCreature(GameObject critter, Vector3 point, float maxDistance, Data.BountyData data)
    {
        if (!critter || !critter.GetComponent<MonsterAI>()) return false;

        for (int index = 0; index < 100; index++)
        {
            Vector3 vector3 = GetRandomVectorWithin(point, maxDistance);

            if (WorldGenerator.instance.GetBiome(vector3) == Heightmap.Biome.Ocean)
            {
                vector3.y = ZoneSystem.instance.m_waterLevel - 0.3f;
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
            CachedEffects.PreSpawnEffectList.Create(vector3, Quaternion.identity);

            if (ActiveBountyLocation != null)
            {
                ActiveBountyLocation.m_position = vector3;
            }
            AlmanacPlugin._plugin.Invoke(nameof(AlmanacPlugin.SpawnBounty), 10f);
            return true;
        }
        return false;
    }

    public static void OnClickBounty()
    {
        if (!HasRequirements()) return;
        Data.BountyLocation bountyLocation = new Data.BountyLocation()
        {
            data = new Data.BountyData()
            {
                m_name = UpdateAlmanac.SelectedBounty.m_creatureName,
                m_damageMultiplier = UpdateAlmanac.SelectedBounty.m_damageMultiplier,
                m_health = UpdateAlmanac.SelectedBounty.m_health,
                m_hunter = Player.m_localPlayer.GetPlayerID(),
                m_rewardType = UpdateAlmanac.SelectedBounty.m_rewardType,
                m_rewardAmount = UpdateAlmanac.SelectedBounty.m_itemAmount,
                m_rewardItem = "",
                m_damages = UpdateAlmanac.SelectedBounty.m_damages,
                m_level = UpdateAlmanac.SelectedBounty.m_level,
                m_skillType = UpdateAlmanac.SelectedBounty.m_skill.ToString(),
                m_skillAmount = UpdateAlmanac.SelectedBounty.m_skillAmount,
                m_experience = UpdateAlmanac.SelectedBounty.m_experience
            },
            m_biome = UpdateAlmanac.SelectedBounty.m_biome,
            m_critter = UpdateAlmanac.SelectedBounty.m_critter,
        };
        if (UpdateAlmanac.SelectedBounty.m_itemReward is { } itemDrop)
        {
            bountyLocation.data.m_rewardItem = itemDrop.name;
        }
        if (AcceptBounty(bountyLocation)) UpdateAlmanac.UpdateBountyPanel();
    }

    private static bool HasRequirements()
    {
        if (m_dateTime != DateTime.MaxValue)
        {
            DateTime lastBounty = m_dateTime + TimeSpan.FromMinutes(AlmanacPlugin._BountyCooldown.Value);
            if (lastBounty > DateTime.Now)
            {
                int difference = (lastBounty - DateTime.Now).Minutes;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_bounty_available {difference} $info_minutes");
                return false;
            }
        }
        if (UpdateAlmanac.SelectedBounty.m_cost <= 0) return true;
        Inventory? inventory = Player.m_localPlayer.GetInventory();
        if (!inventory.HaveItem(UpdateAlmanac.SelectedBounty.m_currency.m_itemData.m_shared.m_name)) return false;
        if (inventory.GetItem(UpdateAlmanac.SelectedBounty.m_currency.m_itemData.m_shared.m_name) is not {} item)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to get currency item from inventory");
            return false;
        }
        if (item.m_stack > UpdateAlmanac.SelectedBounty.m_cost)
        {
            item.m_stack -= UpdateAlmanac.SelectedBounty.m_cost;
            return true;
        }
        if (item.m_stack == UpdateAlmanac.SelectedBounty.m_cost)
        {
            inventory.RemoveItem(item);
            return true;
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$info_not_enough " + UpdateAlmanac.SelectedBounty.m_currency.m_itemData.m_shared.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Not enough currency to buy bounty");
        return false;
    }
}