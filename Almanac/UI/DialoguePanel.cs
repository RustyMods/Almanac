using System;
using System.Collections.Generic;
using Almanac.Bounties;
using Almanac.Managers;
using Almanac.NPC;
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
    
    public void Awake()
    {
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

    public void Show(DialogueManager.Dialogue? dialogue)
    {
        if (dialogue is not { isValid: true }) return;
        
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
        
        return dialogue.Text;
    }

    private static bool ShouldShowDialogueOption(DialogueManager.Dialogue dialogue)
    {
        if (dialogue.Action?.Type is DialogueManager.Command.CancelBounty)
        {
            if (BountyManager.ActiveBountyLocation == null) return false;
        }

        if (dialogue.Action?.Type == DialogueManager.Command.CancelTreasure)
        {
            if (TreasureManager.ActiveTreasureLocation == null) return false;
        }

        return true;
    }
    private void CreateDialogueOptions(DialogueManager.Dialogue dialogue)
    {
        foreach (string? option in dialogue.Dialogues)
        {
            if (!DialogueManager.TryGetDialogue(option, out DialogueManager.Dialogue talk) || !talk.isValid) continue;
            if (!ShouldShowDialogueOption(talk)) continue;
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
            button.SetText("Back");
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
            
        // Hide button for commands that have already been completed
        return dialogue.Action.Type switch
        {
            DialogueManager.Command.Take when dialogue.HasTakenItems(Player.m_localPlayer) => false,
            DialogueManager.Command.Give when dialogue.HasReceivedItem(Player.m_localPlayer) => false,
            DialogueManager.Command.StartBounty when !CanStartBounty(dialogue.Action.Parameters) => false,
            DialogueManager.Command.StartTreasure when !CanStartTreasure(dialogue.Action.Parameters) => false,
            _ => true
        };
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
        foreach(var element in elements) element.Destroy();
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
            prefab.AddComponent<ElementDragHandler>();
            prefab.AddComponent<DialogueHover>();
        }

        public DialogueButton Create(Transform root)
        {
            var go = Instantiate(prefab, root);
            go.SetActive(true);
            return new DialogueButton(go.transform);
        }
        
        public void OnClick(UnityAction action) => button.onClick.AddListener(action);
        public void SetText(string text) => label.text = text;
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
            prefab.AddComponent<ElementDragHandler>();
            area.alignment = TextAnchor.MiddleLeft;
        }

        public void SetText(string text)
        {
            area.text = text;
            Resize();
        }
        
        private void Resize()
        {
            float newHeight = GetTextPreferredHeight(area, textRect);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight + padding);
            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, newHeight);
            _height = newHeight + padding;
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

public class ElementDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
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

public class DialogueHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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