using System;
using System.Collections;
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
    
    public Data.BountyData _data = null!;
    private static readonly int bountyHash = "QuestBounty".GetStableHashCode();
    private const float maxRadius = 10000f;
    private const float minSpawnDistance = 2f;
    private const float maxYDistance = 10f;
    private const int solidHeightMargin = 1000;
    private const float spawnOffset = 5f;
    public Character _character = null!;
    public ZNetView _znv = null!;

    private Minimap.PinData pin = null!;
    public void Awake()
    {
        _znv = GetComponent<ZNetView>();
        _character = GetComponent<Character>();
        
        _znv.Register<string>(nameof(RPC_SetBountyData),RPC_SetBountyData);
        
        AddPin();
        _znv.GetZDO().Persistent = false;
    }
    public void Update() => UpdatePin();

    public void OnDestroy()
    {
        DestroyPin();
        if (_character.GetHealth() > 0)
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
    
    public void RPC_SetBountyData(long sender, string value)
    {
        if (!_znv.IsValid()) return;
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(value);
        _znv.GetZDO().Set(bountyHash, data);
    }

    public void ApplyBountyData()
    {
        if (!TryGetBountyData(_znv, out Data.BountyData? data)) return;
        ApplyCharacterData(data);
        
        LevelEffects levelEffects = _character.GetComponentInChildren<LevelEffects>();
        if (levelEffects)
        {
            levelEffects.SetupLevelVisualization(data.m_level);
        }
        
        _znv.GetZDO().Set(ZDOVars.s_maxHealth, data.m_health);
        _znv.GetZDO().Set(ZDOVars.s_health, data.m_health);
    }

    private void ApplyCharacterData(Data.BountyData data)
    {
        _character.m_faction = Character.Faction.Boss;
        _character.m_boss = true;

        _character.m_name = data.m_name;
        _character.m_level = data.m_level;
    }

    // Static

    public static void OnDeath(Character instance)
    {
        if (!instance) return;
        if (instance.m_lastHit == null) return;
        Character attacker = instance.m_lastHit.GetAttacker();
        if (attacker == null) return;
        if (!attacker.IsPlayer()) return;
        Player? player = attacker as Player;
        if (player == null) return;

        if (!instance.m_nview.IsValid()) return;
            
        if (!TryGetBountyData(instance.m_nview, out Data.BountyData data)) return;

        if (player.GetPlayerID() != data.m_hunter)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Bounty: Invalid Player ID");
            return;
        }
        switch (data.m_rewardType)
        {
            case Data.QuestRewardType.Item:
                GameObject item = ZNetScene.instance.GetPrefab(data.m_rewardItem);
                if (!item) return;
                GameObject drop = Instantiate(item, instance.transform.position, Quaternion.identity);
                if (!drop.TryGetComponent(out ItemDrop component)) return;
                component.m_itemData.m_stack = data.m_rewardAmount;
                break;
            case Data.QuestRewardType.Skill:
                if (!Enum.TryParse(data.m_skillType, out Skills.SkillType skillType)) return;
                Player.m_localPlayer.RaiseSkill(skillType, data.m_skillAmount);
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"$almanac_raised {Utility.ConvertSkills(skillType)} $almanac_by {data.m_skillAmount} $almanac_xp");
                break;
        }
    }

    public static void ApplyBountyModifiers(HitData hit)
    {
        Character attacker = hit.GetAttacker();
        if (!attacker) return;
        if (attacker.IsPlayer()) return;
        if (!attacker.m_nview.IsValid()) return;
            
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
    
    private static bool TryGetBountyData(ZNetView znv, out Data.BountyData data)
    {
        data = new Data.BountyData();
        if (!znv.IsValid()) return false;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string value = znv.GetZDO().GetString(bountyHash);
        if (value.IsNullOrWhiteSpace()) return false;
        data = deserializer.Deserialize<Data.BountyData>(value);
        return true;
    }
    
    public static bool AcceptBounty(Data.BountyLocation bountyLocation)
    {
        if (ActiveBountyLocation != null)
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$almanac_already_have_active_bounty");
            return false;
        }
        if (!FindSpawnLocation(bountyLocation.m_biome, out Vector3 pos)) return false;
        
        bountyLocation.m_position = pos;

        Minimap.PinData BountyPin = Minimap.instance.AddPin(pos, Minimap.PinType.Boss, bountyLocation.data.m_name, false, false);
        BountyPin.m_icon = UpdateAlmanac.SelectedBounty.m_icon;

        bountyLocation.m_pin = BountyPin;

        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$almanac_hunt " + bountyLocation.data.m_name);
        
        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added bounty: " + bountyLocation.data.m_name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + pos.x + " " + pos.z);
        
        ActiveBountyLocation = bountyLocation;
        
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
        float x = UnityEngine.Random.Range(-maxRadius, maxRadius);
        float y = UnityEngine.Random.Range(0f, 5000f);
        float z = UnityEngine.Random.Range(-maxRadius, maxRadius);
        return new Vector3(x, y, z);
    }

    public static bool SpawnCreature(GameObject critter, Vector3 point, float maxDistance, Data.BountyData data)
    {
        if (!critter) return false;
        if (!critter.GetComponent<MonsterAI>()) return false;

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

            AlmanacPlugin._plugin.StartCoroutine(DelayedSpawn(critter, vector3, data));
            return true;
        }

        return false;
    }
    private static IEnumerator DelayedSpawn(GameObject prefab, Vector3 pos, Data.BountyData bountyData)
    {
        yield return new WaitForSeconds(10f);
        GameObject go = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);

        Bounty bounty = go.AddComponent<Bounty>();
        bounty._data = bountyData;
        
        if (go.TryGetComponent(out ZNetView znv))
        {
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(bountyData);
            znv.GetZDO().Set(bountyHash, data);
        }

        CachedEffects.DoneSpawnEffectList.Create(go.transform.position, Quaternion.identity);
    }
    
    public static void AddBountyCommands()
    {
        Terminal.ConsoleCommand SpawnBounty = new("spawn_bounty", "", (Terminal.ConsoleEventFailable)(args =>
        {
            if (args.Length < 2) return false;

            GameObject? prefab = ZNetScene.instance.GetPrefab(args[1]);
            
            if (!prefab) return false;
            if (!prefab.GetComponent<MonsterAI>()) return false;

            Transform transform = Player.m_localPlayer.transform;
            Vector3 position = transform.position;

            return Bounty.SpawnCreature(prefab, position, 10f, new Data.BountyData()
            {
                m_name = prefab.name,
                m_rewardType = Data.QuestRewardType.Item,
                m_rewardAmount = 1,
                m_rewardItem = "Coins",
                m_health = 100,
                m_damageMultiplier = 2f,
                m_hunter = Player.m_localPlayer.GetPlayerID(),
                m_level = 3
            });
        }), isSecret: true);

        Terminal.ConsoleCommand GetBounty = new("get_bounty", "[monster] [biome]", (Terminal.ConsoleEventFailable)(args =>
        {
            if (args.Length < 3) return false;

            GameObject critter = ZNetScene.instance.GetPrefab(args[1]);
            if (!critter) return false;
            if (!critter.GetComponent<MonsterAI>()) return false;

            if (!Enum.TryParse(args[2], out Heightmap.Biome biome)) return false;

            return Bounty.AcceptBounty(new Data.BountyLocation()
            {
                data = new Data.BountyData()
                {
                    m_hunter = Player.m_localPlayer.GetPlayerID(),
                    m_name = critter.name,
                    m_rewardType = Data.QuestRewardType.Item,
                    m_rewardAmount = 100,
                    m_rewardItem = "Coins",
                    m_health = 1,
                    m_damageMultiplier = 10,
                    m_level = 3
                },
                m_biome = biome,
                m_critter = critter,
            });
        }),isSecret: true);
    }
}