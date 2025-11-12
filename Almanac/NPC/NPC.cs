using System;
using System.Collections.Generic;
using Almanac.UI;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac.NPC;

[HarmonyPatch(typeof(Piece),nameof(Piece.CanBeRemoved))]
public static class Piece_CanBeRemoved_Patch
{
    [UsedImplicitly]
    private static void Postfix(Piece __instance, ref bool __result)
    {
        if (!__result || !__instance.GetComponent<NPC>()) return;
        __result = AlmanacPanel.isLocalAdminOrHostAndNoCost || __instance.IsCreator();
    }
}

public static class NPCVars
{
    public static readonly int Animation = nameof(Animation).GetStableHashCode();
    public static readonly int Dialogue = nameof(Dialogue).GetStableHashCode();
    public static readonly int RandomTalk = nameof(RandomTalk).GetStableHashCode();
    public static readonly int Scale =  nameof(Scale).GetStableHashCode();
    public const string DefaultName = "Almanac NPC";
}
public class NPC : MonoBehaviour, Interactable, Hoverable, IDestructible
{
    public ZNetView m_nview = null!;
    public VisEquipment? m_visEquipment;
    public ZSyncAnimation m_zanim = null!;
    public NPCTalk? m_talk;
    public EffectList m_hitEffects = new();
    public Piece m_piece;

    public string m_name = NPCVars.DefaultName;
    public string m_dialogueID = string.Empty;
    public Vector3 m_scale = Vector3.one;
    private DialogueManager.Dialogue? m_dialogue => DialogueManager.TryGetDialogue(m_dialogueID, out var dialogue) ? dialogue : null;
    public string m_rightItem => m_visEquipment?.m_rightItem ?? string.Empty;
    public string m_leftItem => m_visEquipment?.m_leftItem ?? string.Empty;
    public string m_chestItem => m_visEquipment?.m_chestItem ?? string.Empty;
    public string m_legItem => m_visEquipment?.m_legItem ?? string.Empty;
    public string m_helmetItem => m_visEquipment?.m_helmetItem ?? string.Empty;
    public string m_shoulderItem => m_visEquipment?.m_shoulderItem ?? string.Empty;
    public string m_utilityItem => m_visEquipment?.m_utilityItem ?? string.Empty;
    public string m_backRight => m_visEquipment?.m_rightBackItem ?? string.Empty;
    public string m_backLeft => m_visEquipment?.m_leftBackItem ?? string.Empty;
    public string m_beardItem => m_visEquipment?.m_beardItem ?? string.Empty;
    public string m_hairItem => m_visEquipment?.m_hairItem ?? string.Empty;
    public int m_modelIndex => m_visEquipment?.m_modelIndex ?? 0;
    public Vector3 m_skinColor => m_visEquipment?.m_skinColor ?? Vector3.one;
    public Vector3 m_hairColor => m_visEquipment?.m_hairColor ?? Vector3.one;
    public string m_randomTalk => m_talk?.m_randomTalkID ?? string.Empty;
    
    public string m_animation = string.Empty;
    public float m_timeSinceLastAnim;
    public PlayerAnims m_currentAnim = PlayerAnims.None;
    private readonly Queue<AnimChain> m_animQueue = new();
    private readonly Queue<PlayerAnims> m_sequence = new();
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_visEquipment = GetComponent<VisEquipment>();
        m_zanim = GetComponent<ZSyncAnimation>();
        m_talk = GetComponent<NPCTalk>();
        m_piece = GetComponent<Piece>();
        if (!m_nview.IsValid()) return;
        m_nview.Register<HitData>(nameof(RPC_Damage), RPC_Damage);
        m_nview.Register(nameof(RPC_Stagger), RPC_Stagger);
        m_nview.Register<string>(nameof(RPC_SetName), RPC_SetName);
        m_nview.Register<string>(nameof(RPC_SetDialogue), RPC_SetDialogue);
        m_nview.Register<string>(nameof(RPC_SetAnimation),RPC_SetAnimation);
        m_nview.Register<Vector3>(nameof(RPC_SetScale), RPC_SetScale);
    }
    public void Start()
    {
        if (!m_nview.IsValid()) return;
        SetName(m_nview.GetZDO().GetString(ZDOVars.s_tamedName, m_name));
        SetAnimation(m_nview.GetZDO().GetString(NPCVars.Animation));
        SetDialogue(m_nview.GetZDO().GetString(NPCVars.Dialogue));
        SetLeftItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_leftItem)));
        SetRightItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_rightItem)));
        SetChestItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_chestItem)));
        SetLegItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_legItem)));
        SetHelmetItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_helmetItem)));
        SetShoulderItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_shoulderItem)));
        SetUtilityItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_utilityItem)));
        SetModel(m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, UnityEngine.Random.Range(0, 2)));
        SetBeardItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_beardItem)));
        SetHairItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_hairItem)));
        if (m_name == NPCVars.DefaultName)
        {
            SetName(m_modelIndex == 0 ? 
                VikingNameGenerator.GenerateMaleName() 
                : VikingNameGenerator.GenerateFemaleName());
        }
        SetSkinColor(m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one));
        SetHairColor(m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero));
        SetRandomTalk(m_randomTalk);
        DoAnimation(m_animation);
        SetScale(m_nview.GetZDO().GetVec3(NPCVars.Scale, transform.localScale));
    }
    public void Update()
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        float dt = Time.deltaTime;
        m_timeSinceLastAnim += dt;
        if (m_animQueue.Count > 0)
        {
            if (m_timeSinceLastAnim < m_currentAnim.GetAttributeOfType<AnimType>().chainInterval) return;
            AnimChain chain = m_animQueue.Dequeue();
            DoAnimation(chain.trigger, false, chain.index);
        }
        else if (m_sequence.Count > 0)
        {
            if (m_timeSinceLastAnim < m_currentAnim.GetAttributeOfType<AnimType>().chargeTime) return;
            var next = m_sequence.Dequeue();
            DoAnimation(next.ToString(), false);
        }
        else
        {
            if (m_timeSinceLastAnim < 10f) return;
            DoAnimation(m_animation, false);
        }
    }
    
    public void StopAnimation()
    {
        if (m_currentAnim is PlayerAnims.None) return;
        AnimType? type = m_currentAnim.GetAttributeOfType<AnimType>();
        if (type == null) return;
        if (type.isIndex)
        {
            m_zanim.SetInt(type.trigger, 0);
        }
        else if (type.isBool)
        {
            m_zanim.SetBool(type.trigger, false); // need to keep this, even if emoting
            if (type.isEmote) m_zanim.SetTrigger("emote_stop"); // breaks the emote loop
        }
        
        m_animQueue.Clear();
        m_sequence.Clear();
        m_currentAnim = PlayerAnims.None;
    }

    public void DoAnimation(string input, bool save = true, int chainIndex = 0)
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        if (save) m_nview.GetZDO().Set(NPCVars.Animation, input);
        if (Enum.TryParse(input, true, out PlayerAnims anim) && anim.GetAttributeOfType<AnimType>() is {} type)
        {
            if (m_currentAnim == anim && (type.isBool || type.isIndex)) return;
            StopAnimation();
            if (type.isIndex)
            {
                m_zanim.SetInt(type.trigger, type.index);
            }
            else if (type.isBool)
            {
                if (type.isSequential)
                {
                    m_sequence.Enqueue(type.nextSequence);
                    m_zanim.SetTrigger(type.trigger);
                }
                else m_zanim.SetBool(type.trigger, true);
            }
            else if (type.isChain)
            {
                if (chainIndex > type.chainMax) chainIndex = 0;
                m_animQueue.Enqueue(new AnimChain(input, chainIndex + 1));
                m_zanim.SetTrigger(type.trigger + chainIndex);
            }
            else if (type.isSequential)
            {
                m_sequence.Enqueue(type.nextSequence);
                m_zanim.SetTrigger(type.trigger);
            }
            else
            {
                m_zanim.SetTrigger(type.trigger);
            }
            m_currentAnim = anim;
            m_timeSinceLastAnim = 0f;
        }
        else
        {
            StopAnimation();
        }
    }
    public void SetName(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (m_name == text) return;
        m_name = text;
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        m_nview.GetZDO().Set(ZDOVars.s_tamedName, text);
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetName), text);
    }

    public void RPC_SetName(long sender, string text)
    {
        m_name = text;
    }

    public void SetDialogue(string id)
    {
        if (id == m_dialogueID) return;
        m_dialogueID = id;
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        m_nview.GetZDO().Set(NPCVars.Dialogue, id);
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetDialogue), id);
    }

    public void RPC_SetDialogue(long sender, string id)
    {
        m_dialogueID = id;
    }

    public void SetRandomTalk(string id)
    {
        m_talk?.SetRandomTalkID(id);
    }

    public void SetAnimation(string text)
    {
        if (text == m_animation) return;
        DoAnimation(text);
        m_animation = text;
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetAnimation), text);
    }

    public void RPC_SetAnimation(long sender, string text)
    {
        m_animation = text;
    }

    public void SetScale(Vector3 scale)
    {
        if (m_scale == scale) return;
        if (scale.AnyNegative()) return;
        
        m_scale = scale;
        transform.localScale = scale;
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        m_nview.GetZDO().Set(NPCVars.Scale, scale);
        
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetScale), scale);
    }
    public void RPC_SetScale(long sender, Vector3 scale)
    {
        m_scale = scale;
        transform.localScale = scale;
    }

    public void SetLeftItem(string item, int variant = 0)
    {
        if (m_visEquipment?.m_leftItem == item) return;
        m_visEquipment?.SetLeftItem(item, variant);
    }

    public void SetRightItem(string item)
    {
        if (m_visEquipment?.m_rightItem == item) return;
        m_visEquipment?.SetRightItem(item);
    }

    public void SetChestItem(string item)
    {
        if (m_visEquipment?.m_chestItem == item) return;
        m_visEquipment?.SetChestItem(item);
    }

    public void SetLegItem(string item)
    {
        if (m_visEquipment?.m_legItem == item) return;
        m_visEquipment?.SetLegItem(item);
    }

    public void SetHelmetItem(string item)
    {
        if (m_visEquipment?.m_helmetItem == item) return;
        m_visEquipment?.SetHelmetItem(item);
    }

    public void SetShoulderItem(string item, int variant = 0)
    {
        if (m_visEquipment?.m_shoulderItem == item) return;
        m_visEquipment?.SetShoulderItem(item, variant);
    }

    public void SetUtilityItem(string item)
    {
        if (m_visEquipment?.m_utilityItem == item) return;
        m_visEquipment?.SetUtilityItem(item);
    }

    public void SetBeardItem(string item)
    {
        if (m_visEquipment?.m_beardItem == item) return;
        m_visEquipment?.SetBeardItem(item);
    }

    public void SetHairItem(string item)
    {
        if (m_visEquipment?.m_hairItem == item) return;
        m_visEquipment?.SetHairItem(item);
    }

    public void SetRightBackItem(string item)
    {
        if (m_visEquipment?.m_rightBackItem == item) return;
        m_visEquipment?.SetRightBackItem(item);
    }

    public void SetLeftBackItem(string item, int variant = 0)
    {
        if (m_visEquipment?.m_leftBackItem == item) return;
        m_visEquipment?.SetLeftBackItem(item, variant);
    }

    public void SetSkinColor(Vector3 vec)
    {
        if (m_visEquipment?.m_skinColor == vec) return;
        m_visEquipment?.SetSkinColor(vec);
    }

    public void SetHairColor(Vector3 vec)
    {
        if (m_visEquipment?.m_hairColor == vec) return;
        m_visEquipment?.SetHairColor(vec);
    }

    public void SetModel(int index)
    {
        if (m_visEquipment?.m_modelIndex == index) return;
        m_visEquipment?.SetModel(index);
    }
    private static string GetPrefabFromHash(int hash) => !ObjectDB.instance || ObjectDB.instance.GetItemPrefab(hash) is not { } prefab ? string.Empty : prefab.name;
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (alt && (m_piece.IsCreator() || AlmanacPanel.isLocalAdminOrHostAndNoCost))
        {
            m_nview.ClaimOwnership();
            NPCCustomization.instance?.Show(this);
        }
        else
        {
            DialoguePanel.instance?.Show(m_dialogue, this);
        }
        return false;
    }
    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    public string GetHoverText()
    {
        string text = $"{m_name}";
        if (m_dialogue != null)
        {
            text += $"\n[<color=yellow><b>$KEY_Use</b></color>] {Keys.Talk}";
        }
        if (m_piece.IsCreator() || AlmanacPanel.isLocalAdminOrHostAndNoCost) text += $"\n[<color=yellow><b>L.Shift + $KEY_Use</b></color>] {Keys.Customize}";

        return Localization.instance.Localize(text);
    }
    public string GetHoverName() => m_name;
    public void Damage(HitData hit)
    {
        if (!m_nview.IsValid()) return;
        m_nview.InvokeRPC(nameof(RPC_Damage), hit);
    }
    public void RPC_Damage(long sender, HitData hit)
    {
        if (hit.m_hitType == HitData.HitType.CinderFire || hit.m_hitType == HitData.HitType.AshlandsOcean) return;
        m_hitEffects.Create(hit.m_point, Quaternion.identity, transform);
        Stagger();
    }
    
    public void Stagger()
    {
        if (m_nview.IsOwner()) RPC_Stagger(0L);
        else m_nview.InvokeRPC(nameof(RPC_Stagger));
    }

    public void RPC_Stagger(long sender) => m_zanim.SetTrigger("stagger");
    
    public DestructibleType GetDestructibleType() => DestructibleType.None;
}