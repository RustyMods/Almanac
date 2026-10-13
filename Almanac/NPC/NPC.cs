using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.UI;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

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
    public static readonly int Model = "Almanac.NPC.Model".GetStableHashCode();
    public const string DefaultName = "Almanac NPC";
}
public class NPC : MonoBehaviour, Interactable, Hoverable, IDestructible
{
    public ZNetView m_nview = null!;
    public VisEquipment m_visEquipment = null!;
    public ZSyncAnimation m_zanim = null!;
    public NPCTalk? m_talk;
    public EffectList m_hitEffects = new();
    public Piece m_piece = null!;
    public CapsuleCollider m_collider = null!;
    public GameObject m_visual = null!;

    public string m_name = NPCVars.DefaultName;
    public string m_dialogueID = string.Empty;
    public Vector3 m_scale = Vector3.one;
    private DialogueManager.Dialogue? m_dialogue => DialogueManager.TryGetDialogue(m_dialogueID, out var dialogue) ? dialogue : null;
    public int m_rightItem => m_visEquipment?.m_rightItem ?? 0;
    public int m_leftItem => m_visEquipment?.m_leftItem ?? 0;
    public int m_chestItem => m_visEquipment?.m_chestItem ?? 0;
    public int m_legItem => m_visEquipment?.m_legItem ?? 0;
    public int m_helmetItem => m_visEquipment?.m_helmetItem ?? 0;
    public int m_shoulderItem => m_visEquipment?.m_shoulderItem ?? 0;
    public int m_utilityItem => m_visEquipment?.m_utilityItem ?? 0;
    public int m_backRight => m_visEquipment?.m_rightBackItem ?? 0;
    public int m_backLeft => m_visEquipment?.m_leftBackItem ?? 0;
    public int m_beardItem => m_visEquipment?.m_beardItem ?? 0;
    public int m_hairItem => m_visEquipment?.m_hairItem ?? 0;
    public int m_modelIndex => m_visEquipment?.m_modelIndex ?? 0;
    public Vector3 m_skinColor => m_visEquipment?.m_skinColor ?? Vector3.one;
    public Vector3 m_hairColor => m_visEquipment?.m_hairColor ?? Vector3.one;
    public string m_randomTalk => m_talk?.m_randomTalkID ?? string.Empty;
    
    public string m_animation = string.Empty;
    public float m_timeSinceLastAnim;
    public PlayerAnims m_currentAnim = PlayerAnims.None;
    private readonly Queue<AnimChain> m_animQueue = new();
    private readonly Queue<PlayerAnims> m_sequence = new();
    public string m_selectedModel = "Player";
    private GameObject? m_currentModel;
    private VisEquipment? m_modelVis;
    private Animator m_animator = null!;    
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_visEquipment = GetComponent<VisEquipment>();
        m_zanim = GetComponent<ZSyncAnimation>();
        m_talk = GetComponent<NPCTalk>();
        m_piece = GetComponent<Piece>();
        m_collider = GetComponent<CapsuleCollider>();
        m_visual = transform.Find("Visual").gameObject;
        m_animator = m_visual.GetComponent<Animator>();
        if (!m_nview.IsValid()) return;
        m_nview.Register<HitData>(nameof(RPC_Damage), RPC_Damage);
        m_nview.Register(nameof(RPC_Stagger), RPC_Stagger);
        m_nview.Register<string>(nameof(RPC_SetName), RPC_SetName);
        m_nview.Register<string>(nameof(RPC_SetDialogue), RPC_SetDialogue);
        m_nview.Register<string>(nameof(RPC_SetAnimation),RPC_SetAnimation);
        m_nview.Register<Vector3>(nameof(RPC_SetScale), RPC_SetScale);
        m_nview.Register<string>(nameof(RPC_SetModel), RPC_SetModel);
    }
    public void Start()
    {
        if (!m_nview.IsValid()) return;
        SetModel(m_nview.GetZDO().GetString(NPCVars.Model));
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

    public void SetModel(string model)
    {
        if (m_selectedModel == model) return;
        m_selectedModel = model;
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        m_nview.GetZDO().Set(NPCVars.Model, model);
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetModel), model);
    }

    public void RPC_SetModel(long sender, string model)
    {
        m_selectedModel = model;
        if (m_currentModel != null)
        {
            Destroy(m_currentModel);
            m_currentModel = null;
        }

        m_modelVis = null;
        m_zanim.m_animator = m_animator;
        m_visEquipment.enabled = true;
        m_visual.SetActive(true);
        m_collider.enabled = true;
        

        if (string.IsNullOrEmpty(model))
        {
            m_selectedModel = "Player";
            return;
        }
        if (Helpers.GetPrefab(model) is not { } prefab)
        {
            m_selectedModel = "Player";
            m_nview.GetZDO().Set(NPCVars.Model, "");
        }
        else
        {
            m_visual.SetActive(false);
            m_collider.enabled = false;
            m_visEquipment.enabled = false;
            ZNetView.m_forceDisableInit = true;
            m_currentModel = Instantiate(prefab, transform);
            m_currentModel.SetActive(false);
            m_currentModel.layer = gameObject.layer;
            RemoveAllComponents(m_currentModel);
            ZNetView.m_forceDisableInit = false;
            if (m_currentModel.TryGetComponent(out VisEquipment modelVis))
            {
                modelVis.m_nViewOverride = m_nview;
                modelVis.m_nview = m_nview;
                m_modelVis = modelVis;
            }

            m_zanim.m_animator = m_currentModel.GetComponentInChildren<Animator>(true) ?? m_animator;
            m_currentModel.transform.localPosition = Vector3.zero;
            m_currentModel.SetActive(true);
            modelVis?.UpdateEquipmentVisuals();
        }
    }
    
    public void RemoveAllComponents(GameObject go)
    {
        if (go.TryGetComponent(out Rigidbody rb)) DestroyImmediate(rb);
        var components = go.GetComponentsInChildren<MonoBehaviour>(true);
        var queue = new Queue<MonoBehaviour>(components);
        
        int stalled = 0;

        while (queue.Count > 0)
        {
            var component = queue.Dequeue();
            var componentType = component.GetType();
            if (componentType == typeof(VisEquipment) || componentType == typeof(Animator) )
            {
                continue;
            }
            bool isStillDependedOn = queue.Any(other =>
            {
                var otherRequirements = other.GetType()
                    .GetCustomAttributes(typeof(RequireComponent), true)
                    .Cast<RequireComponent>()
                    .SelectMany(r => new[] { r.m_Type0, r.m_Type1, r.m_Type2 })
                    .Where(t => t != null);

                return otherRequirements.Any(r => r.IsAssignableFrom(componentType));
            });

            if (!isStillDependedOn)
            {
                DestroyImmediate(component);
                stalled = 0;
            }
            else
            {
                queue.Enqueue(component);
                ++stalled;

                if (stalled > queue.Count)
                {
                    break;
                }
            }
        }

        if (stalled > 0)
        {
            for (int i = 0; i < queue.Count; ++i)
            {
                var leftover = queue.Dequeue();
                DestroyImmediate(leftover);
            }
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
    public void RPC_SetName(long sender, string text) => m_name = text;

    public void SetDialogue(string id)
    {
        if (id == m_dialogueID) return;
        m_dialogueID = id;
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        m_nview.GetZDO().Set(NPCVars.Dialogue, id);
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetDialogue), id);
    }

    public void RPC_SetDialogue(long sender, string id) => m_dialogueID = id;

    public void SetRandomTalk(string id) => m_talk?.SetRandomTalkID(id);

    public void SetAnimation(string text)
    {
        if (text == m_animation) return;
        DoAnimation(text);
        m_animation = text;
        m_nview.InvokeRPC(ZNetView.Everybody, nameof(RPC_SetAnimation), text);
    }

    public void RPC_SetAnimation(long sender, string text) => m_animation = text;

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
        if (m_visEquipment.m_leftItem == item.GetStableHashCode()) return;
        m_visEquipment.SetLeftItem(item.GetStableHashCode(), variant, 1);
    }

    public void SetRightItem(string item)
    {
        if (m_visEquipment.m_rightItem == item.GetStableHashCode()) return;
        m_visEquipment.SetRightItem(item.GetStableHashCode(), 1);
    }

    public void SetChestItem(string item)
    {
        if (m_visEquipment.m_chestItem == item.GetStableHashCode()) return;
        m_visEquipment.SetChestItem(item.GetStableHashCode());
    }

    public void SetLegItem(string item)
    {
        if (m_visEquipment.m_legItem == item.GetStableHashCode()) return;
        m_visEquipment.SetLegItem(item.GetStableHashCode());
    }

    public void SetHelmetItem(string item)
    {
        if (m_visEquipment.m_helmetItem == item.GetStableHashCode()) return;
        m_visEquipment.SetHelmetItem(item.GetStableHashCode());
    }

    public void SetShoulderItem(string item, int variant = 0)
    {
        if (m_visEquipment.m_shoulderItem == item.GetStableHashCode()) return;
        m_visEquipment.SetShoulderItem(item.GetStableHashCode(), variant, 1);
    }

    public void SetUtilityItem(string item)
    {
        if (m_visEquipment.m_utilityItem == item.GetStableHashCode()) return;
        m_visEquipment.SetUtilityItem(item.GetStableHashCode());
    }

    public void SetBeardItem(string item)
    {
        if (m_visEquipment?.m_beardItem == item.GetStableHashCode()) return;
        m_visEquipment?.SetBeardItem(item.GetStableHashCode());
    }

    public void SetHairItem(string item)
    {
        if (m_visEquipment.m_hairItem == item.GetStableHashCode()) return;
        m_visEquipment.SetHairItem(item.GetStableHashCode());
    }

    public void SetRightBackItem(string item)
    {
        if (m_visEquipment.m_rightBackItem == item.GetStableHashCode()) return;
        m_visEquipment.SetRightBackItem(item.GetStableHashCode(), 1);
    }

    public void SetLeftBackItem(string item, int variant = 0)
    {
        if (m_visEquipment.m_leftBackItem == item.GetStableHashCode()) return;
        m_visEquipment.SetLeftBackItem(item.GetStableHashCode(), variant, 1);
    }

    public void SetSkinColor(Vector3 vec)
    {
        if (m_visEquipment.m_skinColor == vec) return;
        m_visEquipment.SetSkinColor(vec);
    }

    public void SetHairColor(Vector3 vec)
    {
        if (m_visEquipment.m_hairColor == vec) return;
        m_visEquipment.SetHairColor(vec);
    }

    public void SetModel(int index)
    {
        if (m_visEquipment.m_modelIndex == index) return;
        m_visEquipment.SetModel(index);
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
    public float GetHoverOffset() => 0f;

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