using System;
using System.Collections.Generic;
using Almanac.Quests;
using Almanac.Utilities;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Almanac.UI;

public class QuestPanel : MonoBehaviour
{
    private RectTransform root = null!;
    private TextArea _textArea = null!;
    private QuestButton _button = null!;
    public const float Input_Cooldown = 0.1f;
    public float lastInputTime;
    public static QuestPanel? instance;
    private readonly List<QuestElement> elements = new();
    private static bool ShouldShow => Player.m_localPlayer && !Player.m_localPlayer.IsDead() && !Player.m_localPlayer.IsTeleporting() && !Player.m_localPlayer.InCutscene();
    private readonly Vector3 offScreenPos = new Vector3(5000f, 5000f, 0f);
    public void Awake()
    {
        if (Configs.AddLogs) AlmanacPlugin.AlmanacLogger.LogDebug("Almanac.Quest.Panel.Awake");
        instance = this;
        root = transform.Find("ListView/Viewport/ListRoot").GetComponent<RectTransform>();
        _textArea = new TextArea(transform.Find("ListView/Viewport/TextArea"));
        _textArea.Load();
        _button = new QuestButton(transform.Find("ListView/Viewport/Button"));
        _button.Load();
        transform.position = Configs.QuestPanelPos;
        Hide();
    }

    public void Start() => LoadActiveQuests();
    public void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        bool shouldShow = ShouldShow;
        bool isOffScreen = transform.position == offScreenPos;
        if (shouldShow && isOffScreen)
        {
            transform.position = Configs.QuestPanelPos;
        }
        else if (!shouldShow && !isOffScreen)
        {
            transform.position = offScreenPos;
        }
    }

    public void OnDestroy()
    {
        instance = null;
    }

    public void Toggle()
    {
        if (gameObject.activeInHierarchy)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
    public void Show()
    {
        if (gameObject.activeInHierarchy) return;
        if (elements.Count == 0) return;
        if (!ShouldShow) return;
        gameObject.SetActive(true);
        transform.position = Configs.QuestPanelPos;
    }
    public void LoadActiveQuests()
    {
        Clear();
    
        foreach (QuestManager.Quest quest in QuestManager.GetActiveQuests())
        {
            if (quest.isCompleted) continue;
            TextArea element = _textArea.Create(root);
            quest.referenceUI = element;
            element.referenceQuest = quest;
            element.SetText(quest.GetTooltip());
            elements.Add(element);
        }
    
        if (elements.Count > 0)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;
        gameObject.SetActive(false);
    }
    
    public static bool IsVisible() => instance?.gameObject.activeInHierarchy ?? false;
    
    public void Clear()
    {
        foreach(QuestElement element in elements) element.Destroy();
        elements.Clear();
    }
    
    public static void OnPosChange(object sender, EventArgs args)
    {
        if (instance == null) return;
        if (sender is not ConfigEntry<Vector3> config) return;
        instance.transform.position = config.Value;
    }
    public class QuestButton : QuestElement
    {
        private readonly Button button;
        private readonly Text label;

        public QuestButton(Transform transform) : base(transform)
        {
            button = prefab.GetComponent<Button>();
            label = transform.Find("Text").GetComponent<Text>();
        }

        public void Load()
        {
            prefab.AddComponent<QuestDragHandler>();
            prefab.AddComponent<ButtonHover>();
        }

        public QuestButton Create(Transform root)
        {
            var go = Instantiate(prefab, root);
            go.SetActive(true);
            return new QuestButton(go.transform);
        }
        
        public void OnClick(UnityAction action) => button.onClick.AddListener(action);
        public void SetText(string text) => label.text = text;
        public void Interactable(bool enable) => button.interactable = enable;
    }
    public class TextArea : QuestElement
    {
        private readonly RectTransform textRect;
        public Image background;
        private readonly Text area;
        private const float padding = 20f;
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
            prefab.AddComponent<QuestDragHandler>();
            area.alignment = TextAnchor.MiddleLeft;
        }

        public void SetText(string text)
        {
            area.text = Localization.instance.Localize(text);
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

    public class QuestElement
    {
        protected readonly GameObject prefab;
        protected readonly RectTransform rect;
        public float height => GetHeight();
        public QuestManager.Quest? referenceQuest;

        protected QuestElement(Transform transform)
        {
            prefab = transform.gameObject;
            rect = transform.GetComponent<RectTransform>();
        }

        protected virtual float GetHeight() => rect.sizeDelta.y;

        public void Destroy()
        {
            if (referenceQuest != null) referenceQuest.referenceUI = null;
            UnityEngine.Object.Destroy(prefab);
        }
    }
}

public class QuestDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 mouseDifference = Vector3.zero;

    public void OnDrag(PointerEventData eventData)
    {
        if (QuestPanel.instance == null) return;
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        QuestPanel.instance.transform.position = Input.mousePosition + mouseDifference;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (QuestPanel.instance == null) return;
        Vector2 pos = eventData.position;
        mouseDifference = QuestPanel.instance.transform.position - new Vector3(pos.x, pos.y, 0);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (QuestPanel.instance == null) return;
        Configs._questPanelPos.Value = QuestPanel.instance.transform.position;
    }
}