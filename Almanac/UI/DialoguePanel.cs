using System;
using System.Collections.Generic;
using Almanac.Bounties;
using Almanac.Managers;
using Almanac.NPC;
using Almanac.Quests;
using Almanac.TreasureHunt;
using Almanac.Utilities;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Almanac.UI;

public class DialoguePanel : MonoBehaviour
{
    public static readonly GameObject _panel = AssetBundleManager.LoadAsset<GameObject>("npc_dialogue", "NPCDialogue")!;

    private RectTransform root = null!;
    private TextArea _textArea = null!;
    private DialogueButton _button = null!;
    private const float Input_Cooldown = 0.1f;
    private float lastInputTime;
    public static DialoguePanel? instance;
    private readonly List<DialogueElement> elements = new();
    private NPC.NPC? m_currentNPC;
    
    public void Awake()
    {
        if (Configs.AddLogs) AlmanacPlugin.AlmanacLogger.LogDebug("Almanac.Dialogue.Panel.Awake");
        instance = this;
        root = transform.Find("ListView/Viewport/ListRoot").GetComponent<RectTransform>();
        _textArea = new TextArea(transform.Find("ListView/Viewport/TextArea"));
        _textArea.Load();
        _button = new DialogueButton(transform.Find("ListView/Viewport/Button"));
        _button.Load();
        transform.position = Configs.DialoguePos;
        Hide();
    }
    public void OnDestroy()
    {
        instance = null;
    }
    public void Update()
    {
        if (!IsVisible()) return;
        if (!Player.m_localPlayer || Player.m_localPlayer.IsDead())
        {
            Hide();
            return;
        }

        if (Time.time - lastInputTime > Input_Cooldown && ZInput.GetKeyDown(KeyCode.Escape))
        {
            lastInputTime = Time.time;
            Hide();
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        foreach (DialogueManager.Dialogue dialogue in DialogueManager.GetDialogues()) dialogue.previous = null;
    }
    public static bool IsVisible() => instance?.gameObject.activeInHierarchy ?? false;

    public void Show(DialogueManager.Dialogue? dialogue, NPC.NPC? npc = null)
    {
        if (dialogue is not { isValid: true }) return;
        if (npc != null) m_currentNPC = npc;
        Clear();
        gameObject.SetActive(true);
        
        bool isInteractable = dialogue.IsInteractable(Player.m_localPlayer);
        CreateDescriptionText(dialogue, isInteractable);
        CreateActionButton(dialogue, isInteractable);
        CreateDialogueOptions(dialogue);
    }

    private void CreateDescriptionText(DialogueManager.Dialogue dialogue, bool isInteractable)
    {
        TextArea description = _textArea.Create(root);
        elements.Add(description);
        
        string text = GetDescriptionText(dialogue, isInteractable);
        if (m_currentNPC != null)
        {
            text = $"<color=orange>{m_currentNPC.m_name}</color>\n{text}";
        }
        description.SetText(text);
    }

    private static string GetDescriptionText(DialogueManager.Dialogue dialogue, bool isInteractable)
    {
        if (!isInteractable) return dialogue.MissingRequirementsText;
        
        if (dialogue.Action?.Type == DialogueManager.Command.StartBounty)
        {
            if (BountyManager.TryGetBountyData(dialogue.Action.Parameters, out var bountyData) && !bountyData.HasRequirements(Player.m_localPlayer))
            {
                return dialogue.MissingRequirementsText;
            }
        }
        else if (dialogue.Action?.Type == DialogueManager.Command.StartTreasure)
        {
            if (TreasureManager.TryGetTreasure(dialogue.Action.Parameters, out var treasureData) &&
                !treasureData.CanPurchase(Player.m_localPlayer))
            {
                return dialogue.MissingRequirementsText;
            }
        }
        else if (dialogue.Action?.Type == DialogueManager.Command.Give)
        {
            if (dialogue.HasReceivedItem(Player.m_localPlayer))
            {
                return dialogue.CompletedText;
            }
        }
        else if (dialogue.Action?.Type == DialogueManager.Command.Take)
        {
            if (dialogue.HasTakenItems(Player.m_localPlayer))
            {
                return dialogue.CompletedText;
            }
        }
        else if (dialogue.Action?.Type == DialogueManager.Command.StartQuest)
        {
            if (dialogue.Action == null) return dialogue.Text;
            if (dialogue.Action.TryGetQuestDialogue(out string id, out _))
            {
                if (QuestManager.IsActive(id))
                {
                    return dialogue.CompletedText;
                }
            }
        }
        else if (dialogue.Action?.Type == DialogueManager.Command.CompleteQuest)
        {
            if (dialogue.Action == null) return dialogue.Text;
            if (dialogue.Action.TryGetQuestDialogue(out string id, out _))
            {
                if (QuestManager.IsActive(id) && !QuestManager.IsQuestComplete(id))
                {
                    return dialogue.MissingRequirementsText;
                }
            }
        }
                
        return dialogue.Text;
    }
    private void CreateDialogueOptions(DialogueManager.Dialogue dialogue)
    {
        foreach (string? option in dialogue.Dialogues)
        {
            if (!DialogueManager.TryGetDialogue(option, out DialogueManager.Dialogue talk) || !talk.isValid) continue;
            if (!talk.ShouldShowDialogue()) continue;
            talk.previous = dialogue;
            DialogueButton button = _button.Create(root);
            elements.Add(button);
            button.SetText(talk.Label);
            button.OnClick(() => Show(talk));
        }

        if (dialogue.previous != null)
        {
            DialogueButton button = _button.Create(root);
            elements.Add(button);
            button.SetText(Keys.Back);
            button.OnClick(() => Show(dialogue.previous));
        }
    }

    private void CreateActionButton(DialogueManager.Dialogue dialogue, bool isInteractable)
    {
        if (!ShouldShowActionButton(dialogue, isInteractable)) return;
            
        DialogueButton actionButton = _button.Create(root);
        elements.Add(actionButton);
        
        ConfigureActionButton(actionButton, dialogue);
    }

    private static bool ShouldShowActionButton(DialogueManager.Dialogue dialogue, bool isInteractable)
    {
        if (dialogue.Action == null || dialogue.Action.Type == DialogueManager.Command.None || !isInteractable)
            return false;
            
        return dialogue.Action.Type switch
        {
            DialogueManager.Command.Take when dialogue.HasTakenItems(Player.m_localPlayer) => false,
            DialogueManager.Command.Give when dialogue.HasReceivedItem(Player.m_localPlayer) => false,
            DialogueManager.Command.StartBounty when !CanStartBounty(dialogue.Action.Parameters) => false,
            DialogueManager.Command.StartTreasure when !CanStartTreasure(dialogue.Action.Parameters) => false,
            DialogueManager.Command.StartQuest when !CanStartQuest(dialogue.Action) => false,
            DialogueManager.Command.CompleteQuest when !CanCompleteQuest(dialogue.Action) => false,
            DialogueManager.Command.CancelQuest when !CanCancelQuest(dialogue.Action) => false,
            _ => true
        };
    }

    private static bool CanCompleteQuest(DialogueManager.DialogueAction action)
    {
        if (!action.TryGetQuestDialogue(out string id, out _)) return false;
        return !QuestManager.IsQuestCollected(id);
    }

    private static bool CanStartQuest(DialogueManager.DialogueAction action)
    {
        if (!action.TryGetQuestDialogue(out string id, out _)) return false;
        return !QuestManager.IsActive(id);
    }

    private static bool CanCancelQuest(DialogueManager.DialogueAction action)
    {
        if (!action.TryGetQuestDialogue(out string id, out _)) return false;
        return QuestManager.IsActive(id) && !QuestManager.IsQuestComplete(id);
    }

    private static bool CanStartBounty(string parameters)
    {
        return BountyManager.TryGetBountyData(parameters, out var data) && data.HasRequirements(Player.m_localPlayer);
    }

    private static bool CanStartTreasure(string parameters)
    {
        return TreasureManager.TryGetTreasure(parameters, out var data) && data.CanPurchase(Player.m_localPlayer);
    }

    private void ConfigureActionButton(DialogueButton actionButton, DialogueManager.Dialogue dialogue)
    {
        (string buttonText, UnityAction buttonAction) = GetButtonConfiguration(dialogue);
        actionButton.SetText(buttonText);
        actionButton.OnClick(buttonAction);
    }

    private (string text, UnityAction action) GetButtonConfiguration(DialogueManager.Dialogue dialogue)
    {
        return dialogue.Action?.Type switch
        {
            DialogueManager.Command.StartBounty when BountyManager.ActiveBountyLocation != null =>
                (Keys.CancelBounty, BountyManager.CancelBounty),
                
            DialogueManager.Command.StartTreasure when TreasureManager.ActiveTreasureLocation != null =>
                (Keys.CancelTreasure, TreasureManager.CancelTreasure),
                
            _ => (dialogue.Action?.Label ?? "Invalid Action", () => dialogue.OnClick(this, Player.m_localPlayer))
        };
    }

    public void Clear()
    {
        foreach (var element in elements)
        {
            element.Destroy();
        }
        elements.Clear();
    }

    public static void OnPosChange(object sender, EventArgs args)
    {
        if (instance == null) return;
        if (sender is not ConfigEntry<Vector3> config) return;
        instance.transform.position = config.Value;
    }

    public class DialogueElement
    {
        protected readonly GameObject prefab;
        protected readonly RectTransform rect;
        public float height => GetHeight();
        protected DialogueElement(Transform transform)
        {
            prefab = transform.gameObject;
            rect = transform.GetComponent<RectTransform>();
        }

        protected virtual float GetHeight() => rect.sizeDelta.y;
        
        public void Destroy() => UnityEngine.Object.Destroy(prefab);
    }
    public class DialogueButton : DialogueElement
    {
        private readonly Button button;
        private readonly Text label;

        public DialogueButton(Transform transform) : base(transform)
        {
            button = prefab.GetComponent<Button>();
            label = transform.Find("Text").GetComponent<Text>();
        }

        public void Load()
        {
            prefab.AddComponent<DialogueDragHandler>();
            prefab.AddComponent<ButtonHover>();
        }

        public DialogueButton Create(Transform root)
        {
            var go = Instantiate(prefab, root);
            go.SetActive(true);
            return new DialogueButton(go.transform);
        }
        
        public void OnClick(UnityAction action) => button.onClick.AddListener(action);
        public void SetText(string text) => label.text = Localization.instance.Localize(text);
        public void Interactable(bool enable) => button.interactable = enable;
    }

    public class TextArea : DialogueElement
    {
        private readonly RectTransform textRect;
        public Image background;
        private readonly Text area;
        private const float padding = 40f;
        private float _height;
        protected override float GetHeight() => _height;

        public TextArea Create(Transform root)
        {
            var go = Instantiate(prefab, root);
            go.SetActive(true);
            return new TextArea(go.transform);
        }

        public TextArea(Transform transform) : base(transform)
        {
            background = prefab.GetComponent<Image>();
            area = transform.Find("Text").GetComponent<Text>();
            textRect = area.GetComponent<RectTransform>();
        }
        
        public void Load()
        {
            prefab.AddComponent<DialogueDragHandler>();
            area.alignment = TextAnchor.MiddleLeft;
        }

        public void SetText(string text)
        {
            area.text = text;
            Resize();
        }
        
        private void Resize()
        {
            float newHeight = GetTextPreferredHeight(area, textRect) + padding;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, newHeight);
            _height = newHeight;
        }
        private static float GetTextPreferredHeight(Text text, RectTransform rect)
        {
            if (string.IsNullOrEmpty(text.text)) return 0f;
            TextGenerator textGen = text.cachedTextGenerator;
            TextGenerationSettings settings = text.GetGenerationSettings(rect.rect.size);
            float preferredHeight = textGen.GetPreferredHeight(text.text, settings);
            return preferredHeight;
        }
    }
}

public class DialogueDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 mouseDifference = Vector3.zero;

    public void OnDrag(PointerEventData eventData)
    {
        if (DialoguePanel.instance == null) return;
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        DialoguePanel.instance.transform.position = Input.mousePosition + mouseDifference;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (DialoguePanel.instance == null) return;
        Vector2 pos = eventData.position;
        mouseDifference = DialoguePanel.instance.transform.position - new Vector3(pos.x, pos.y, 0);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (DialoguePanel.instance == null) return;
        Configs._dialoguePos.Value = DialoguePanel.instance.transform.position;
    }
}

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background = null!;
    private Text label = null!;
    public void Awake()
    {
        background = GetComponent<Image>();
        label = transform.Find("Text").GetComponent<Text>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        background.color = new Color(0.8f, 0.5f, 0.2f, 1f);
        label.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = new Color(0f, 0f, 0f, 0.5f);
        label.color = Helpers._OrangeColor;
    }
}