using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Almanac.NPC;

public class NPCTalk : MonoBehaviour
{
    public ZNetView m_nview = null!;
    public NPC m_npc = null!;
    public float m_randomTalkInterval = 30f;
    public float m_randomTalkChance = 0.2f;
    public float m_hideDialogueDelay = 9f;
    public float m_minTalkInterval = 3f;
    public float m_offset = 2f;
    public float m_byeRange = 15f;
    public float m_greetRange = 10f;
    public float m_maxRange = 20f;

    public List<string> m_randomTalk = new();
    public List<string> m_randomGreets = new();
    public List<string> m_randomGoodbye = new();
    
    public Player? m_targetPlayer;
    public float m_lastTargetUpdate;

    public bool m_didGreet;
    public bool m_didGoodbye;

    private readonly Queue<NPCSay> m_queuedTexts = new();
    
    public string m_randomTalkID = string.Empty;
    
    public void Awake()
    {
        m_npc = GetComponent<NPC>();
        m_nview = GetComponent<ZNetView>();
        if (!m_nview.IsValid()) return;
        m_nview.Register<string>(nameof(RPC_SetRandomTalkID),RPC_SetRandomTalkID);
        SetRandomTalkID(m_nview.GetZDO().GetString(NPCVars.RandomTalk));
    }

    public void Start()
    {
        InvokeRepeating(nameof(RandomTalk), Random.Range(m_randomTalkInterval / 5f, m_randomTalkInterval), m_randomTalkInterval);
    }

    public void SetRandomTalkID(string id)
    {
        if (m_randomTalkID == id) return;
        m_nview.InvokeRPC(nameof(RPC_SetRandomTalkID), id);
    }

    public void RPC_SetRandomTalkID(long sender, string id)
    {
        if (!m_nview.IsValid()) return;
        m_randomTalkID = id;
        m_nview.GetZDO().Set(NPCVars.RandomTalk, id);
        if (!RandomTalkManager.TryGetRandomTalk(id, out var randomTalk)) return;
        SetRandomTalk(randomTalk.Talk);
        SetRandomGreets(randomTalk.Greets);
        SetRandomGoodbye(randomTalk.GoodBye);
    }
    
    public void SetRandomTalk(List<string> talk) => m_randomTalk = talk;
    public void SetRandomGreets(List<string> talk) => m_randomGreets = talk;
    public void SetRandomGoodbye(List<string> talk) => m_randomGoodbye = talk;

    public void Update()
    {
        if (!m_nview.IsValid()) return;
        UpdateTarget();
        if (m_targetPlayer is not null)
        {
            if (m_nview.IsOwner())
            {
                float distance = Vector3.Distance(m_targetPlayer.transform.position, transform.position);
                if (!m_didGreet && distance < m_greetRange)
                {
                    m_didGreet = true;
                    QueueSay(m_randomGreets, m_nview.GetZDO().GetBool(ZDOVars.s_emoteOneshot) ? "wave" : "");
                }

                if (m_didGreet && !m_didGoodbye && distance > m_byeRange)
                {
                    m_didGoodbye = true;
                    QueueSay(m_randomGoodbye, m_nview.GetZDO().GetBool(ZDOVars.s_emoteOneshot) ? "wave" : "");
                }
            }
        }
        else
        {
            m_didGreet = false;
            m_didGoodbye = false;
        }
        
        UpdateSayQueue();
    }

    public void UpdateSayQueue()
    {
        if (m_queuedTexts.Count == 0 || Time.time - NpcTalk.m_lastTalkTime < m_minTalkInterval) return;
        NPCSay? queuedSay = m_queuedTexts.Dequeue();
        Say(queuedSay.text, queuedSay.trigger);
    }

    public void Say(string text, string trigger)
    {
        NpcTalk.m_lastTalkTime = Time.time;
        Chat.instance.SetNpcText(gameObject, Vector3.up * m_offset, 20f, m_hideDialogueDelay, "", text, false);
        if (string.IsNullOrEmpty(trigger)) return;
        m_npc.DoAnimation(trigger, false);
    }

    public void QueueSay(List<string> texts, string trigger)
    {
        if (texts.Count == 0 || m_queuedTexts.Count >= 3) return;
        m_queuedTexts.Enqueue(new NPCSay
        {
            text = texts[Random.Range(0, texts.Count)],
            trigger = trigger
        });
    }

    public void UpdateTarget()
    {
        if (Time.time - m_lastTargetUpdate <= 1.0) return;
        m_lastTargetUpdate = Time.time;
        m_targetPlayer = null;
        Player closestPlayer = Player.GetClosestPlayer(transform.position, m_maxRange);
        if (closestPlayer is null) return;
        m_targetPlayer = closestPlayer;
    }

    public void RandomTalk()
    {
        if (Time.time - NpcTalk.m_lastTalkTime < m_minTalkInterval || Random.Range(0.0f, 1f) > m_randomTalkChance) return;
        UpdateTarget();
        if (!m_targetPlayer) return;
        QueueSay(m_randomTalk, "wave");
    }

    private class NPCSay
    {
        public string text = string.Empty;
        public string trigger = string.Empty;
    }
}