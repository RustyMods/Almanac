using System;
using System.Collections.Generic;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;

namespace Almanac.NPC;

public static class NPCVars
{
    public static readonly int Animation = nameof(Animation).GetStableHashCode();
    public static readonly int Dialogue = nameof(Dialogue).GetStableHashCode();
    public static readonly int RandomTalk = nameof(RandomTalk).GetStableHashCode();
    public static readonly string DefaultName = "Almanac NPC";
}
public class NPC : MonoBehaviour, Interactable, Hoverable, IDestructible
{
    public ZNetView m_nview = null!;
    public VisEquipment? m_visEquipment;
    public ZSyncAnimation m_zanim = null!;
    public Animator m_animator = null!;
    public NPCTalk? m_talk;
    public EffectList m_hitEffects = new();

    public string m_name = NPCVars.DefaultName;
    public string m_dialogueID = string.Empty;
    private DialogueManager.Dialogue? m_dialogue => DialogueManager.TryGetDialogue(m_dialogueID, out var dialogue) ? dialogue : null;
    public string m_animation = string.Empty;
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
    
    private static readonly Dictionary<string, int> craftingIndices = new()
    {
        ["work"] = 1,
        ["forge"] = 2,
        ["stir"] = 3
    };
    
    public string m_emoteState = "";
    public int m_emoteID;
    private bool m_oneshot;
    
    private Emotes? m_activeEmote;
    public float m_timeSinceLastEmote;
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_visEquipment = GetComponent<VisEquipment>();
        m_zanim = GetComponent<ZSyncAnimation>();
        m_animator = GetComponentInChildren<Animator>();
        m_talk = GetComponent<NPCTalk>();
        if (!m_nview.IsValid()) return;
        m_nview.Register<HitData>(nameof(RPC_Damage), RPC_Damage);
        m_nview.Register(nameof(RPC_Stagger), RPC_Stagger);
        m_nview.Register<string>(nameof(RPC_SetName), RPC_SetName);
        m_nview.Register<string>(nameof(RPC_SetDialogue), RPC_SetDialogue);
        m_nview.Register<string>(nameof(RPC_SetAnimation),RPC_SetAnimation);
        m_name = m_nview.GetZDO().GetString(ZDOVars.s_tamedName, m_name);
        m_animation = m_nview.GetZDO().GetString(NPCVars.Animation);
        m_dialogueID = m_nview.GetZDO().GetString(NPCVars.Dialogue);
    }
    public void Start()
    {
        if (m_visEquipment == null || m_nview.GetZDO() == null || !m_nview.IsOwner()) return;
        SetLeftItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_leftItem)));
        SetRightItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_rightItem)));
        SetChestItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_chestItem)));
        SetLegItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_legItem)));
        SetHelmetItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_helmetItem)));
        SetShoulderItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_shoulderItem)));
        SetUtilityItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_utilityItem)));
        SetBeardItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_beardItem)));
        SetHairItem(GetPrefabFromHash(m_nview.GetZDO().GetInt(ZDOVars.s_hairItem)));
        SetModel(m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, UnityEngine.Random.Range(0, 2)));
        if (m_name == NPCVars.DefaultName)
        {
            SetName(m_visEquipment.m_modelIndex == 0 ? VikingNameGenerator.GenerateMaleName() : VikingNameGenerator.GenerateFemaleName());
        }
        SetSkinColor(m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one));
        SetHairColor(m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.one));
        SetRandomTalk(m_randomTalk);
        DoAnimation(m_animation);
    }
    public void Update()
    {
        if (m_activeEmote == null || !m_oneshot) return;
        float dt = Time.deltaTime;
        m_timeSinceLastEmote += dt;
        if (m_timeSinceLastEmote < 10f) return;
        m_timeSinceLastEmote = 0f;
        DoEmote(m_activeEmote);
    }
    public void LateUpdate()
    {
        if (!m_nview.IsValid()) return;
        m_oneshot = IsOneShot(m_activeEmote);
        int emoteID = m_nview.GetZDO().GetInt(ZDOVars.s_emoteID);
        string emote = m_nview.GetZDO().GetString(ZDOVars.s_emote);
        if (emoteID == m_emoteID) return;
        m_emoteID = emoteID;
        if (!string.IsNullOrEmpty(m_emoteState))
        {
            m_animator.SetBool("emote_" + m_emoteState, false);
        }
        m_emoteState = "";
        m_animator.SetTrigger("emote_stop");
        if (string.IsNullOrEmpty(emote)) return;
        m_animator.ResetTrigger("emote_stop");
        if (m_oneshot)
        {
            m_animator.SetTrigger("emote_" + emote);
        }
        else
        {
            m_emoteState = emote;
            m_animator.SetBool("emote_" + emote, true);
        }
    }

    private bool InEmote() => !string.IsNullOrWhiteSpace(m_emoteState);

    public void SetName(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (m_name == text) return;
        m_nview.InvokeRPC(nameof(RPC_SetName), text);
    }

    public void RPC_SetName(long sender, string text)
    {
        m_name = text;
        if (!m_nview.IsValid()) return;
        m_nview.GetZDO().Set(ZDOVars.s_tamedName, text);
    }

    public void SetDialogue(string id)
    {
        if (id == m_dialogueID) return;
        m_nview.InvokeRPC(nameof(RPC_SetDialogue), id);
    }

    public void RPC_SetDialogue(long sender, string id)
    {
        if (!m_nview.IsValid()) return;
        m_dialogueID = id;
        m_nview.GetZDO().Set(NPCVars.Dialogue, id);
    }

    public void SetRandomTalk(string id)
    {
        m_talk?.SetRandomTalkID(id);
    }

    public void SetAnimation(string text)
    {
        if (text == m_animation) return;
        m_nview.InvokeRPC(nameof(RPC_SetAnimation), text);
    }

    public void RPC_SetAnimation(long sender, string text)
    {
        DoAnimation(text);
        m_animation = text;
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
    
    public static bool IsValidEmote(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && (craftingIndices.ContainsKey(input) || Enum.TryParse(input, true, out Emotes _));
    }

    public void DoAnimation(string input, bool save = true)
    {
        if (!m_nview.IsValid()) return;
        if (save) m_nview.GetZDO().Set(NPCVars.Animation, input);
        if (string.IsNullOrEmpty(input))
        {
            StopEmote();
            return;
        }
        if (craftingIndices.TryGetValue(input.ToLower(), out int craftIndex))
        {
            StopEmote();
            StartCraft(input, craftIndex);
        }
        else if (!Enum.TryParse(input, true, out Emotes emote))
        {
            StopEmote();
        }
        else DoEmote(emote, save);
    }

    public void StartCraft(string input, int index) => m_zanim.SetInt("crafting", index);

    private static bool IsOneShot(Emotes? emote) => emote switch
    {
        Emotes.Sit => false,
        Emotes.Kneel => false,
        Emotes.Dance => false,
        Emotes.Headbang => false,
        Emotes.Relax => false,
        Emotes.Rest => false,
        _ => true
    };
    public void DoEmote(Emotes? emote, bool save = true)
    {
        bool oneshot = IsOneShot(emote);
        StartEmote(emote.ToString().ToLower(), oneshot);
        if (save) m_activeEmote = emote;
        m_oneshot = oneshot;
        m_timeSinceLastEmote = 0f;
    }
    public void StartEmote(string emote, bool oneshot = true)
    {
        var num = m_nview.GetZDO().GetInt(ZDOVars.s_emoteID);
        m_nview.GetZDO().Set(ZDOVars.s_emoteID, num + 1);
        m_nview.GetZDO().Set(ZDOVars.s_emote, emote);
        m_nview.GetZDO().Set(ZDOVars.s_emoteOneshot, oneshot);
    }
    public void StopEmote()
    {
        m_zanim.SetInt("crafting", 0);
        if (m_nview.GetZDO().GetString(ZDOVars.s_emote) == string.Empty) return;
        var num = m_nview.GetZDO().GetInt(ZDOVars.s_emoteID);
        m_nview.GetZDO().Set(ZDOVars.s_emoteID, num + 1);
        m_nview.GetZDO().Set(ZDOVars.s_emote, string.Empty);
    }
    private static string GetPrefabFromHash(int hash) => !ObjectDB.instance || ObjectDB.instance.GetItemPrefab(hash) is not { } prefab ? string.Empty : prefab.name;
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (alt)
        {
            if (AlmanacPanel.isLocalAdminOrHostAndNoCost) NPCCustomization.instance?.Show(this);
        }
        else
        {
            DialoguePanel.instance?.Show(m_dialogue);
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
        if (AlmanacPanel.isLocalAdminOrHostAndNoCost)
        {
            text += $"\n[<color=yellow><b>L.Shift + $KEY_Use</b></color>] {Keys.Customize}";
        }
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