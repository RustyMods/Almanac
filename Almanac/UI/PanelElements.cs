using System;
using System.Collections.Generic;
using System.Text;
using Almanac.Data;
using Almanac.Utilities;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Almanac.UI;

public class HoverToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text m_hoverText = null!;

    public void Start()
    {
        m_hoverText.text = Localization.instance.Localize("$label_showknown");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_hoverText.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_hoverText.color = Color.clear;
    }
}
public class ResizeTextRect : MonoBehaviour
{
    public Text m_txt = null!;
    public RectTransform m_rect = null!;
    public int m_minLength;
    public Vector2 m_size;
    public void Awake()
    {
        m_txt = GetComponentInChildren<Text>();
        m_rect = GetComponent<RectTransform>();
        m_size = m_rect.sizeDelta;
        m_minLength = CalculateMinLength();
    }

    public void Start()
    {
        var length = m_txt.text.Length;
        if (length < m_minLength) return;
        var rowCount = Mathf.CeilToInt((float)length / m_minLength);
        m_rect.sizeDelta = new Vector2(m_size.x, rowCount * m_size.y);
        m_txt.rectTransform.sizeDelta = m_rect.sizeDelta;
        m_txt.color = Color.white;
    }
    
    private int CalculateMinLength()
    {
        TextGenerator generator = new TextGenerator();
        TextGenerationSettings settings = m_txt.GetGenerationSettings(m_txt.rectTransform.rect.size);

        string testString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        float charWidth = generator.GetPreferredWidth(testString, settings) / testString.Length;
        
        return Mathf.FloorToInt(m_size.x / charWidth);
    }
}

public class PanelElement
{
    /// <summary>
    /// Holds reference to the elements instantiated in the list root root
    /// In order to filter by m_text
    /// </summary>
    public readonly GameObject m_go;
    public readonly string m_text;
    public TitleElement? m_parent;

    public PanelElement(string text, GameObject go)
    {
        m_text = text;
        m_go = go;
        SidePanel.m_elements.Add(this);
    }
}
public class GroupElement : MonoBehaviour
{
    public GameObject m_template = null!;
    public RectTransform m_rect = null!;
    public Image m_bkg = null!;
    public float m_templateHeight = 75f;
    public Vector2 m_baseSize;
    private PanelElement m_element = null!;

    public void Awake()
    {
        m_rect = GetComponent<RectTransform>();
        m_bkg = GetComponent<Image>();
        m_bkg.color = Color.clear; // Disabling darkening
        m_baseSize = m_rect.sizeDelta;
        m_template = transform.Find("Template").gameObject;
        m_template.AddComponent<Template>();
    }

    public void OnDestroy()
    {
        SidePanel.m_elements.Remove(m_element);
    }

    public PanelElement Setup(List<TemplateData> data)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var template in data)
        {
            Instantiate(m_template, m_rect).GetComponent<Template>().Setup(template);
            stringBuilder.Append(template.m_name + " ");
        }
        m_template.SetActive(false);
        m_element = new PanelElement(stringBuilder.ToString(), gameObject);
        Resize();
        return m_element;
    }

    public void Resize()
    {
        int row = Mathf.CeilToInt(m_rect.childCount / 5f);
        var newHeight = row * m_templateHeight;
        m_rect.sizeDelta = new Vector2(m_baseSize.x, newHeight);
    }

    public class TemplateData
    {
        public readonly Sprite? m_icon;
        public readonly string m_name;
        public readonly bool m_isKnown;

        public TemplateData(Piece.Requirement requirements)
        {
            m_icon = requirements.m_resItem.m_itemData.GetIcon();
            m_name = $"{requirements.m_resItem.m_itemData.m_shared.m_name}\n(<color=orange>{requirements.m_amount}</color> +<color=orange>{requirements.m_amountPerLevel}</color>/lvl)";
            m_isKnown = AlmanacPlugin._KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsKnownMaterial(requirements.m_resItem.m_itemData.m_shared.m_name) || Player.m_localPlayer.NoCostCheat();
        }
        
        public TemplateData(ItemDrop item)
        {
            m_icon = item.m_itemData.GetIcon();
            m_name = $"{item.m_itemData.m_shared.m_name}\n";
            m_isKnown = AlmanacPlugin._KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name) || Player.m_localPlayer.NoCostCheat();
        }

        public TemplateData(ItemDrop item, float dropChance):this(item)
        {
            m_name += $"<color=orange>({dropChance}%)</color>";
        }
    }

    protected internal class Template : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image m_bkg = null!;
        public Image m_icon = null!;
        public Text m_name = null!;
        public void Awake()
        {
            m_bkg = GetComponent<Image>();
            m_bkg.sprite = Assets.InputFieldBkg;
            m_icon = transform.Find("Icon").GetComponent<Image>();
            m_name = transform.Find("name").GetComponent<Text>();
            m_name.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
            m_bkg.color = Color.clear;
            m_name.gameObject.SetActive(false);
        }

        public void Setup(TemplateData data)
        {
            m_icon.sprite = data.m_icon;
            m_icon.color = data.m_isKnown ? Color.white : Color.black;
            m_name.text = data.m_isKnown ? Localization.instance.Localize(data.m_name) : "???";
            m_name.gameObject.SetActive(false);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            m_name.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_name.gameObject.SetActive(false);
        }
    }
}
/// <summary>
/// Used as an element in the main trophy list & the side panel
/// As interactable button
/// </summary>
public class AlmanacButton : MonoBehaviour
{
    public Text m_txt = null!;
    private PanelElement? m_element;
    private Action? m_onClick;
    public string m_localizedName = "";
    public AlmanacUI.Element? m_trophyElement;
    public bool m_interactable = true;
    public void Awake()
    {
        m_txt = transform.Find("Text").GetComponent<Text>();
        var button = GetComponent<Button>();
        var img = GetComponent<Image>();
        img.sprite = Assets.ButtonImage.sprite;
        img.type = Image.Type.Sliced;
        img.fillCenter = true;
        button.spriteState = Assets.ButtonComponent.spriteState;
        button.transition = Selectable.Transition.SpriteSwap;
        button.onClick.AddListener(OnClick);
        m_txt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
    }

    public void OnDestroy()
    {
        if (m_element != null) SidePanel.m_elements.Remove(m_element);
        if (m_trophyElement != null) AlmanacUI.m_elements.Remove(m_trophyElement);
    }

    public void Setup(string text, Action onClick, bool sideElement)
    {
        m_txt.text = Localization.instance.Localize(text);
        m_localizedName = m_txt.text;
        m_onClick = onClick;
        if (sideElement) m_element = new PanelElement(m_txt.text, gameObject);
    }

    public void Setup(Creatures.Data data, bool isKnown, bool sideElement = false)
    {
        Setup(isKnown ? data.m_name : "???", () => SidePanel.m_instance.Setup(new SidePanel.PanelData(data)), sideElement);
        m_localizedName = Localization.m_instance.Localize(data.m_name);
        m_interactable = isKnown;
    }

    public void OnClick()
    {
        if (!m_interactable) return;
        m_onClick?.Invoke();
    }
}
public class KeyValueElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image m_background = null!;
    public Text m_type = null!;
    public Text m_data = null!;
    private PanelElement m_element = null!;

    public void Awake()
    {
        m_background = GetComponent<Image>();
        m_type = transform.Find("Key").GetComponent<Text>();
        m_data = transform.Find("Value").GetComponent<Text>();
        m_type.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        m_data.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        m_background.color = Color.clear;
    }

    public void OnDestroy()
    {
        SidePanel.m_elements.Remove(m_element);
    }

    public PanelElement Setup(string type, string data)
    {
        m_type.text = Localization.instance.Localize(type);
        m_data.text = Localization.instance.Localize(data);
        
        m_element = new PanelElement(m_type.text + " " + m_data.text, gameObject);
        return m_element;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_background.color = new Color(1f, 1f, 1f, 0.1f);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        m_background.color = Color.clear;
    }
}

public class TitleElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text m_txt = null!;
    private PanelElement m_element = null!;
    private Image m_background = null!;
    public readonly List<PanelElement> m_elements = new();
    public bool m_isOn = true;

    public void Awake()
    {
        m_txt = transform.Find("Text").GetComponent<Text>();
        m_txt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        m_background = gameObject.AddComponent<Image>();
        m_background.sprite = Assets.InputFieldBkg;
        m_background.type = Image.Type.Sliced;
        m_background.fillCenter = true;
        m_background.color = Color.clear;
    }

    public void OnDestroy()
    {
        SidePanel.m_elements.Remove(m_element);
    }

    public TitleElement SetTitle(string text)
    {
        m_txt.text = Localization.instance.Localize(text);
        m_element = new PanelElement(m_txt.text, gameObject);
        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_isOn = !m_isOn;
        foreach (var element in m_elements)
        {
            element?.m_go.SetActive(m_isOn);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_background.color = new Color(1f, 1f, 1f, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_background.color = Color.clear;
    }
}

public class LeaderboardElement : MonoBehaviour
{
    public Image m_background = null!;
    public Text m_name = null!;
    public Text m_rank = null!;
    public Text m_achievement = null!;
    public Text m_kd = null!;
    private PanelElement m_element = null!;

    public void Awake()
    {
        m_background = GetComponent<Image>();
        m_background.sprite = Assets.ButtonImage.sprite;
        m_name = transform.Find("PlayerName/Name").GetComponent<Text>();
        transform.Find("PlayerName/LeftBraid").GetComponent<Image>().sprite = Assets.BraidLeft.sprite;
        transform.Find("PlayerName/RightBraid").GetComponent<Image>().sprite = Assets.BraidRight.sprite;
        m_rank = transform.Find("Rank/Value").GetComponent<Text>();
        m_achievement = transform.Find("Achievement/Value").GetComponent<Text>();
        m_kd = transform.Find("KD/Value").GetComponent<Text>();
        transform.Find("Rank/Key").GetComponent<Text>().text = Localization.instance.Localize("$almanac_rank_title");
        transform.Find("Achievement/Key").GetComponent<Text>().text = Localization.instance.Localize("$almanac_achievement_title");
        transform.Find("KD/Key").GetComponent<Text>().text = Localization.instance.Localize("$almanac_kd_title");
        foreach (var component in GetComponentsInChildren<Image>())
        {
            component.color = Helpers.OrangeColor;
        }
        foreach (var component in GetComponentsInChildren<Text>())
        {
            component.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        }
    }

    public void OnDestroy()
    {
        SidePanel.m_elements.Remove(m_element);
    }

    public PanelElement Setup(string playerName, int rank, PlayerData data)
    {
        m_name.text = playerName;
        m_rank.text = rank.ToString();
        m_achievement.text = data.completed_achievements.ToString();
        m_kd.text = $"{data.total_kills}/{data.total_deaths}";
        m_element = new PanelElement(playerName, gameObject);
        return m_element;
    }
}