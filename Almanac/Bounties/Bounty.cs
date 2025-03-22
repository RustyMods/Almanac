using System;
using Almanac.API;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

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
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_bountyescaped");
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
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"$msg_raised {Helpers.ConvertSkills(skillType)} $msg_by {bountyData.m_skillAmount} $label_xp");
                        }
                        break;
                }

                if (bountyData.m_experience > 0) ClassesAPI.AddEXP(bountyData.m_experience);
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_bountystolenby: " + player.GetPlayerName());
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

    public static bool CancelBounty(Data.ValidatedBounty data)
    {
        if (ActiveBountyLocation == null) return false;
        Minimap.instance.RemovePin(ActiveBountyLocation.m_pin);
        ActiveBountyLocation = null;
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_canceledbounty");
        var component = Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
        component.Setup("$label_acceptbounty", () =>
        {
            if (AcceptBounty(data))
            {
                Destroy(component.gameObject);
            }
        }, true);
        return true;
    }

    public static bool AcceptBounty(Data.ValidatedBounty data)
    {
        if (m_dateTime != DateTime.MaxValue)
        {
            DateTime lastBounty = m_dateTime + TimeSpan.FromMinutes(AlmanacPlugin._BountyCooldown.Value);
            if (lastBounty > DateTime.Now)
            {
                int difference = (lastBounty - DateTime.Now).Minutes;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_bountyavailablein {difference} $msg_minutes");
                return false;
            }
        }

        if (data.m_cost > 0 && !Player.m_localPlayer.NoCostCheat())
        {
            if (!Player.m_localPlayer.GetInventory().HaveItem(data.m_currency.m_itemData.m_shared.m_name))
            {
                                            
                return false;
            }

            if (Player.m_localPlayer.GetInventory().CountItems(data.m_currency.m_itemData.m_shared.m_name) < data.m_cost)
            {

                return false;
            }

            Player.m_localPlayer.GetInventory().RemoveItem(data.m_currency.m_itemData.m_shared.m_name, data.m_cost);
        }
        var bountyLocation = new Data.BountyLocation(data);
        if (!FindSpawnLocation(bountyLocation.m_biome, out Vector3 pos)) return false;
        bountyLocation.m_position = pos;
        Minimap.PinData BountyPin = Minimap.instance.AddPin(pos, Minimap.PinType.Boss, bountyLocation.m_data.m_name, false, false);
        BountyPin.m_icon = data.m_icon;
        bountyLocation.m_pin = BountyPin;
        ActiveBountyLocation = bountyLocation;
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_hunt " + bountyLocation.m_data.m_name);
        m_dateTime = DateTime.Now;
        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added bounty: " + bountyLocation.m_data.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + pos.x + " " + pos.z);
        var component = Instantiate(AlmanacUI.m_buttonElement, SidePanel.m_instance.m_contentRoot).GetComponent<AlmanacButton>();
        component.Setup("$label_cancelbounty", () =>
        {
            if (CancelBounty(data))
            {
                Destroy(component.gameObject);
            }
        }, true);
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

            CachedEffects.m_preSpawnEffects.Create(vector3, Quaternion.identity);

            if (ActiveBountyLocation != null)
            {
                ActiveBountyLocation.m_position = vector3;
            }
            AlmanacPlugin._plugin.Invoke(nameof(AlmanacPlugin.SpawnBounty), 10f);
            return true;
        }
        return false;
    }
}