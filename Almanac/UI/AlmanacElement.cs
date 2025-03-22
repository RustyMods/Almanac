using System.Collections.Generic;
using Almanac.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Almanac.UI;

public class AlmanacElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static readonly List<AlmanacElement> m_instances = new();
    public Image m_bkg = null!;
    public Image m_icon = null!;
    public Image m_iconBgk = null!;
    public TextMeshProUGUI m_name = null!;
    public TextMeshProUGUI m_desc = null!;
    public Color m_nameBaseColor;
    public AlmanacUI.ElementData m_data = null!;
    public string m_localizedName = "";
    public AlmanacUI.Element m_element = null!;
    public bool m_interactable = true;
    

    public void Awake()
    {
        m_bkg = transform.Find("bkg").GetComponent<Image>();
        m_bkg.gameObject.SetActive(true);
        m_iconBgk = transform.Find("icon_bkg").GetComponent<Image>();
        m_icon = m_iconBgk.transform.Find("icon").GetComponent<Image>();
        m_name = transform.Find("name").GetComponent<TextMeshProUGUI>();
        m_desc = transform.Find("description").GetComponent<TextMeshProUGUI>();
        m_nameBaseColor = m_name.color;
        m_instances.Add(this);
        SetBackground(false);
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
        AlmanacUI.m_elements.Remove(m_element);
    }
    
    public void Setup(AlmanacUI.ElementData data)
    {
        m_data = data;
        SetIcon(data.m_icon, data.m_isKnown);
        SetName(data.m_name, data.m_isKnown || data.m_type is AlmanacUI.ElementData.ReferenceType.Achievement);
        SetDescription(data.m_desc, data.m_isKnown || data.m_type is AlmanacUI.ElementData.ReferenceType.Achievement);
        UpdateBackground();
        m_interactable = data.m_isKnown;
    }

    public void UpdateBackground()
    {
        if (m_data.m_achievement == null || m_data.m_achievement.m_statusEffect == null) return;
        SetBackground(EffectMan.ActiveAchievementEffects.Contains(m_data.m_achievement.m_statusEffect));
    }

    public void SetIcon(Sprite? icon, bool known)
    {
        m_icon.sprite = icon;
        m_icon.color = known ? Color.white : Color.black;
    }

    public void SetName(string text, bool known)
    {
        m_localizedName = Localization.instance.Localize(text);
        m_name.text = known ? m_localizedName : "???";
        
    }
    public void SetDescription(string text, bool known) => m_desc.text = known ? Localization.instance.Localize(text) : "???";
    public void SetBackground(bool enable)
    {
        m_bkg.color = enable ? AlmanacPlugin._OutlineColor.Value : Color.clear;
        m_name.color = enable ? Color.white : m_nameBaseColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_iconBgk.transform.localScale *= 1.15f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_iconBgk.transform.localScale /= 1.15f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_data.m_type is AlmanacUI.ElementData.ReferenceType.Item or AlmanacUI.ElementData.ReferenceType.Piece && !m_interactable)
        {
            SidePanel.m_instance.ShowMetrics();
            return;
        }

        if (m_data.m_type is AlmanacUI.ElementData.ReferenceType.Achievement && eventData.button is PointerEventData.InputButton.Right)
        {
            if (m_data.m_achievement != null && m_data.m_achievement.IsComplete())
            {
                m_data.m_achievement.Collect();
            }
        }
        SidePanel.m_instance.Setup(new SidePanel.PanelData(m_data));
    }

    public static void UpdateBackgrounds()
    {
        foreach(var instance in m_instances) instance.UpdateBackground();
    }
    
}