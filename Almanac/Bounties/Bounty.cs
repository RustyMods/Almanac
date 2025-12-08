using Almanac.Data;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac.Bounties;

[HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
public static class CharacterDamagePatch
{
    [UsedImplicitly]
    private static void Prefix(HitData hit)
    {
        if (hit.GetAttacker()?.TryGetComponent(out Bounty bounty) ?? false)
        {
            hit.ApplyModifier(bounty.m_damageMultiplier);
        }
    }
}

[HarmonyPatch(typeof(Character), nameof(Character.GetHoverName))]
public static class CharacterHoverPatch
{
    private static void Postfix(Character __instance, ref string __result)
    {
        if (!__instance.TryGetComponent(out Bounty component)) return;
        __result = component.m_overrideName;
    }
}

public static class BountyVars
{
    public static readonly int LocationGUID = "Almanac.BountyLocation.ID".GetStableHashCode();
    public static readonly int DamageModifier = "Almanac.DamageModifier".GetStableHashCode();
    public static readonly int BountyID = "Almanac.BountyID".GetStableHashCode();
    public static readonly int BountyHealth = "Almanac.BountyHealth".GetStableHashCode();
}

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(ZNetView))]
[RequireComponent(typeof(MonsterAI))]
public class Bounty : MonoBehaviour
{
    public Character m_character = null!;
    public ZNetView m_nview = null!;
    public MonsterAI m_monsterAI = null!;
    public bool m_isDead;
    private long m_hunter;
    private string m_bountyID = null!;
    public float m_damageMultiplier = 1f;
    public string m_overrideName = "";

    private Minimap.PinData pin = null!;
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_character = GetComponent<Character>();
        m_monsterAI = GetComponent<MonsterAI>();
        m_character.m_onDeath += OnDeath;
        m_hunter = m_nview.GetZDO().GetLong(ZDOVars.s_creator);
        m_bountyID = m_nview.GetZDO().GetString(BountyVars.BountyID);
        m_damageMultiplier = m_nview.GetZDO().GetFloat(BountyVars.DamageModifier, 1f);
        m_overrideName = m_nview.GetZDO().GetString(ZDOVars.s_tamedName, m_character.m_name);
        // m_character.m_name = m_nview.GetZDO().GetString(ZDOVars.s_tamedName, m_character.m_name);
        m_character.m_boss = true;
        m_nview.Register<string, string>(nameof(RPC_OnDeath), RPC_OnDeath);
        AddPin();
    }

    public void Start()
    {
        m_monsterAI.SetAlerted(true);
        m_monsterAI.SetHuntPlayer(true);
        m_monsterAI.SetTarget(Player.GetPlayer(m_hunter));
        
        var healthOverride = m_nview.GetZDO().GetFloat(BountyVars.BountyHealth, m_character.m_health);
        var health = healthOverride * m_character.m_level;
        m_character.SetMaxHealth(health);
        m_character.SetHealth(health);
    }

    public void Update() => UpdatePin();

    public void OnDestroy()
    {
        DestroyPin();
        if (!m_isDead)
        {
            if (m_hunter != Player.m_localPlayer.GetPlayerID()) return;
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.BountyEscaped);
            BountyManager.ActiveBountyLocation?.data.ReturnCost(Player.m_localPlayer);
            BountyManager.ActiveBountyLocation = null;
        }
    }
    public void AddPin()
    {
        pin = Minimap.instance.AddPin(gameObject.transform.position, Minimap.PinType.EventArea, "", false, false);
        pin.m_worldSize = 100f;
        pin.m_animate = true;
    }
    
    public void UpdatePin() => pin.m_pos = gameObject.transform.position;
    public void DestroyPin() => Minimap.instance.RemovePin(pin);
    public void OnDeath()
    {
        if (m_character == null || Player.GetPlayer(m_hunter) is not {} owner) return;
        
        m_nview.InvokeRPC(owner.GetZDOID().UserID, nameof(RPC_OnDeath), m_character.m_name, m_bountyID);
        
        m_isDead = true;
    }

    public void RPC_OnDeath(long sender, string characterName, string bountyID)
    {
        if (BountyManager.bounties.TryGetValue(bountyID, out BountyManager.BountyData bounty))
        {
            bounty.completed = true;
            MessageHud.instance.ShowBiomeFoundMsg($"{characterName} {Keys.Vanquished}", true);
            Player.m_localPlayer.GetRecords().bounties++;
        }
    }
}