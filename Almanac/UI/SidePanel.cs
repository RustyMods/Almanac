using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.FileSystem;
using Almanac.Utilities;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Almanac.UI;

public class SidePanel : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public static SidePanel m_instance = null!;
    [Header("Instantiating Elements")]
    public static GameObject m_keyValueElement = null!;
    public static GameObject m_titleElement = null!;
    public static GameObject m_leaderboardElement = null!;
    public static GameObject m_groupElement = null!;
    [Description("List of elements instantiated, used to filter")]
    public static readonly List<PanelElement> m_elements = new();
    [Header("Panel components")]
    public RectTransform m_rect = null!;
    public Image m_background = null!;
    public Image m_icon = null!;
    public Text m_title = null!;
    public Text m_buttonTxt = null!;
    public InputField m_searchField = null!;
    public Button m_button = null!;
    public Image m_contentFrame = null!;
    public Image m_buttonImg = null!;
    public Button m_settingButton = null!;
    public Image m_settingButtonImg = null!;
    public RectTransform m_contentRoot = null!;
    [Header("Variables")]
    public float m_contentHeight = 566f;
    public float m_spacing = 5f;
    public Vector3 m_mouseDifference;
    public bool m_metricsOpen;

    public void ReloadAssets()
    {
        m_background.material = Assets.WoodPanel.material;
        m_background.sprite = Assets.WoodPanel.sprite;
        m_button.spriteState = Assets.ButtonComponent.spriteState;
        m_buttonImg.sprite = Assets.ButtonImage.sprite;
    }
    
    public void Init()
    {
        m_rect = GetComponent<RectTransform>();
        m_rect.anchoredPosition = new Vector2(680f, 0f);
        m_background = GetComponent<Image>();
        m_background.material = Assets.LitHud;
        m_background.sprite = Assets.WoodPanel.sprite;
        m_background.color = Color.white;
        m_icon = transform.Find("Icon").GetComponent<Image>();
        m_title = transform.Find("Title").GetComponent<Text>();
        transform.Find("BraidGroup/LeftBraid").GetComponent<Image>().sprite =Assets.BraidLeft.sprite;
        transform.Find("BraidGroup/RightBraid").GetComponent<Image>().sprite =Assets.BraidRight.sprite;
        m_contentFrame = transform.Find("ContentFrame").GetComponent<Image>();
        m_contentFrame.sprite = Assets.InputFieldBkg;
        m_contentFrame.type = Image.Type.Sliced;
        m_contentFrame.fillCenter = true;
        m_contentFrame.color = new Color32(0, 0, 0, 100);
        m_contentRoot = transform.Find("ContentFrame/ScrollRect/Viewport/ListRoot").GetComponent<RectTransform>();
        m_button = transform.Find("Button").GetComponent<Button>();
        m_button.gameObject.AddComponent<ButtonSfx>().m_sfxPrefab = Assets.ButtonSFX.m_sfxPrefab;
        m_buttonImg = m_button.GetComponent<Image>();
        m_buttonImg.sprite = Assets.ButtonImage.sprite;
        m_buttonImg.color = Color.white;
        m_button.transition = Selectable.Transition.SpriteSwap;
        m_button.spriteState = Assets.ButtonComponent.spriteState;
        m_buttonTxt = m_button.transform.Find("Text").GetComponent<Text>();
        m_buttonTxt.text = Localization.instance.Localize("$almanac_stats_button");
        m_buttonTxt.color = Helpers.OrangeColor;
        m_searchField = transform.Find("SearchField").GetComponent<InputField>();
        m_searchField.onValueChanged.AddListener(OnSearch);
        m_keyValueElement = AlmanacPlugin._UIAssets.LoadAsset<GameObject>("KeyValueElement");
        m_keyValueElement.AddComponent<KeyValueElement>();
        m_titleElement = AlmanacPlugin._UIAssets.LoadAsset<GameObject>("TitleElement");
        m_titleElement.AddComponent<TitleElement>();
        m_leaderboardElement = AlmanacPlugin._UIAssets.LoadAsset<GameObject>("LeaderboardElement");
        m_leaderboardElement.AddComponent<LeaderboardElement>();
        m_groupElement = AlmanacPlugin._UIAssets.LoadAsset<GameObject>("GroupElement");
        m_groupElement.AddComponent<GroupElement>();
        m_settingButton = transform.Find("SettingButton").GetComponent<Button>();
        m_settingButtonImg = m_settingButton.GetComponent<Image>();
        m_settingButtonImg.sprite = Assets.ButtonImage.sprite;
        m_settingButton.transition = Selectable.Transition.SpriteSwap;
        m_settingButton.spriteState = Assets.ButtonComponent.spriteState;
        m_settingButton.onClick.AddListener(OnSettings);
        m_settingButton.gameObject.SetActive(false);
        m_button.onClick.AddListener(OnClick);
        FontManager.SetFont(GetComponentsInChildren<Text>());
        OnClose();
        SetIcon(null);
        m_instance = this;
    }

    public void OnSettings()
    {
        
    }

    public void OnOpen()
    {
        gameObject.SetActive(true);
        ShowMetrics();
    }

    public void OnSearch(string input)
    {
        foreach (PanelElement? element in m_elements)
        {
            element.m_go.SetActive(element.m_text.ToLower().Contains(input));
            if (element.m_parent != null) element.m_parent.m_isOn = true;
        }
    }

    public void ShowLeaderboard()
    {
        if (!Player.m_localPlayer) return;
        Clear();
        List<KeyValuePair<string, PlayerData>> ranked = Leaderboard.LeaderboardData.OrderByDescending(kv => kv.Value.completed_achievements).ToDictionary(kv => kv.Key, kv => kv.Value).ToList();
        for (int index = 0; index < ranked.Count; ++index)
        {
            var kvp = ranked[index];
            var element = Instantiate(m_leaderboardElement, m_contentRoot).GetComponent<LeaderboardElement>();
            element.Setup(kvp.Key, index + 1, kvp.Value);
        }

        m_metricsOpen = false;
        SetButtonText("$label_metrics");
    }

    public void ShowMetrics()
    {
        if (!Player.m_localPlayer) return;
        Clear();
        SetTitle(Player.m_localPlayer.GetPlayerName());
        SetIcon(SpriteManager.crownGoldIcon);
        TitleElement? component = null;
        foreach (var entry in PlayerStats.GetEntries())
        {
            if (entry.value == "title")
            {
                component = Instantiate(m_titleElement, m_contentRoot).GetComponent<TitleElement>().SetTitle(entry.title);
            }
            else
            { 
                var element = Instantiate(m_keyValueElement, m_contentRoot).GetComponent<KeyValueElement>().Setup(entry.title, entry.value);
                if (component != null)
                {
                    component.m_elements.Add(element);
                    element.m_parent = component;
                }
            }
        }
        m_metricsOpen = true;
        SetButtonText("$label_leaderboard");
    }

    public void SetButtonText(string text) => m_buttonTxt.text = Localization.instance.Localize(text);

    public void OnClose()
    {
        Clear();
        gameObject.SetActive(false);
    }

    public void SetIcon(Sprite? icon)
    {
        m_icon.sprite = icon == null ? SpriteManager.AlmanacIcon : icon;
    }

    public void SetTitle(string text) => m_title.text = Localization.instance.Localize(text);
    
    public void OnClick()
    {
        if (m_metricsOpen) ShowLeaderboard();
        else ShowMetrics();
    }

    public void Clear()
    {
        foreach (Transform child in m_contentRoot) Destroy(child.gameObject);
    }

    public void Setup(PanelData data)
    {
        Clear();
        SetIcon(data.m_icon);
        SetTitle(data.m_name);
        data.m_content?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_mouseDifference = m_rect.position - new Vector3(eventData.position.x, eventData.position.y, 0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        m_rect.position = Input.mousePosition + m_mouseDifference;
    }

    public class PanelData
    {
        public readonly string m_name;
        public readonly Sprite? m_icon;
        public readonly Action? m_content;

        public PanelData(Creatures.Data data)
        {
            m_name = data.m_name;
            m_icon = data.GetIcon();
            m_content = () =>
            {
                Build(data.GetEntries());
                if (data.m_drops.Count > 0)
                {
                    var title = Title("$title_creaturedrops");
                    List<GroupElement.TemplateData> templateData = new();
                    foreach (var drop in data.m_drops)
                    {
                        if (drop.m_prefab.TryGetComponent(out ItemDrop component))
                        {
                            templateData.Add(new GroupElement.TemplateData(component, drop.m_chance));
                        }
                    }
                    var element = Group(templateData);
                    if (title != null)
                    {
                        title.m_elements.Add(element);
                        element.m_parent = title;
                    }
                }

                if (data.m_consumeItems.Count > 0)
                {
                    var title = Title("$title_tamingitems");
                    List<GroupElement.TemplateData> templateData = new();
                    foreach (var item in data.m_consumeItems)
                    {
                        templateData.Add(new GroupElement.TemplateData(item));
                    }
                    var element = Group(templateData);
                    if (title != null)
                    {
                        title.m_elements.Add(element);
                        element.m_parent = title;
                    }
                }
            };
        }

        public PanelData(AlmanacUI.ElementData data)
        {
            m_name = data.m_name;
            m_icon = data.m_icon;
            m_content = GetContent(data);
        }
        
        private static void Build(List<Entries.Entry> list)
        {
            TitleElement? component = null;
            foreach (var entry in list)
            {
                if (entry.value == "title")
                {
                    component = Title(entry.title);
                }
                else if (entry.value == "lore")
                {
                    var go = Instantiate(m_titleElement, m_instance.m_contentRoot);
                    go.GetComponent<TitleElement>().SetTitle(entry.title);
                    go.AddComponent<ResizeTextRect>();
                }
                else
                {
                    var element = KeyValue(entry.title, entry.value);
                    if (component != null)
                    {
                        component.m_elements.Add(element);
                        element.m_parent = component;
                    }
                }
            }
        }

        private static Action GetContent(AlmanacUI.ElementData data)
        {
            switch (data.m_type)
            {
                case AlmanacUI.ElementData.ReferenceType.Item:
                    return () =>
                    {
                        if (data.m_item == null) return;
                        Build(data.m_item.GetEntries());
                        if (data.m_item.m_set.Count > 0)
                        {
                            Group(data.m_item.m_set.Select(x => new GroupElement.TemplateData(x.m_item)).ToList());
                        }
                        if (data.m_item.m_recipe is {} recipe)
                        {
                            Title("$title_recipe");
                            Group(recipe.m_resources.Select(x => new GroupElement.TemplateData(x)).ToList());
                        }
                    };
                case AlmanacUI.ElementData.ReferenceType.Piece:
                    return () =>
                    {
                        if (data.m_pieceData == null || data.m_pieceData.m_prefab == null) return;
                        if (data.m_pieceData.m_piece.m_resources.Length > 0)
                        {
                            Title("$title_recipe");
                            Group(data.m_pieceData.m_piece.m_resources.Select(x => new GroupElement.TemplateData(x)).ToList());
                        }
                        Build(data.m_pieceData.GetEntries());
                    };
                case AlmanacUI.ElementData.ReferenceType.Bounty:
                    return () =>
                    {
                        if (data.m_bounty == null) return;
                        Entries.EntryBuilder builder = new();
                        builder.Add("$title_reward");
                        switch (data.m_bounty.m_rewardType)
                        {
                            case Bounties.Data.QuestRewardType.Item:
                                if (data.m_bounty.m_itemReward == null) break;
                                builder.Add("$label_item", data.m_bounty.m_itemReward);
                                builder.Add("$label_amount", data.m_bounty.m_itemAmount);
                                break;
                            case Bounties.Data.QuestRewardType.Skill:
                                if (data.m_bounty.m_skill == Skills.SkillType.None) break;
                                builder.Add("$label_skilltype", data.m_bounty.m_skill);
                                builder.Add("$label_amount", data.m_bounty.m_skillAmount, Entries.EntryBuilder.Option.XP);
                                break;
                        }

                        if (data.m_bounty.m_experience > 0)
                        {
                            builder.Add("$label_classexperience", data.m_bounty.m_experience, Entries.EntryBuilder.Option.XP);
                        }
                        builder.Add("$title_bountyinfo");
                        builder.Add("$label_prefabname", data.m_bounty.m_creatureName);
                        builder.Add("$label_level", data.m_bounty.m_level);
                        builder.Add("$se_health", data.m_bounty.m_health);
                        builder.Add("$label_damagemultiplier", data.m_bounty.m_damageMultiplier);
                        builder.Add("$label_purchasecurrency", data.m_bounty.m_currency);
                        builder.Add("$label_purchasecost", data.m_bounty.m_cost);
                        builder.Add("$title_damages");
                        builder.Add("$inventory_blunt", data.m_bounty.m_damages.blunt);
                        builder.Add("$inventory_slash", data.m_bounty.m_damages.slash);
                        builder.Add("$inventory_pierce", data.m_bounty.m_damages.pierce);
                        builder.Add("$inventory_fire", data.m_bounty.m_damages.fire);
                        builder.Add("$inventory_frost", data.m_bounty.m_damages.frost);
                        builder.Add("$inventory_lightning", data.m_bounty.m_damages.lightning);
                        builder.Add("$inventory_spirit", data.m_bounty.m_damages.spirit);
                        Build(builder.ToList());
                        if (Bounty.ActiveBountyLocation != null)
                        {
                            var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                            component.Setup("$label_cancelbounty", () =>
                            {
                                if (Bounty.CancelBounty(data.m_bounty))
                                {
                                    Destroy(component.gameObject);
                                }
                            }, true);
                        }
                        else
                        {
                            var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                            component.Setup("$label_acceptbounty",
                            () =>
                            {
                                if (Bounty.AcceptBounty(data.m_bounty))
                                {
                                    Destroy(component.gameObject);
                                }
                            }, true);
                        }
                    };
                case AlmanacUI.ElementData.ReferenceType.Treasure:
                    return () =>
                    {
                        if (data.m_treasure == null) return;
                        Entries.EntryBuilder builder = new Entries.EntryBuilder();
                        builder.Add("$title_reward");
                        foreach (var drop in data.m_treasure.m_dropTable.m_drops)
                        {
                            if (!drop.m_item.TryGetComponent(out ItemDrop component)) continue;
                            builder.Add(component.m_itemData.m_shared.m_name, drop.m_stackMin, drop.m_stackMax, "-");
                        }

                        builder.Add("$label_dropchance", data.m_treasure.m_dropTable.m_dropChance, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$label_oneofeach", data.m_treasure.m_dropTable.m_oneOfEach);
                        builder.Add("$label_minmax", data.m_treasure.m_dropTable.m_dropMin, data.m_treasure.m_dropTable.m_dropMax, "-");
                        builder.Add("$title_treasureinfo");
                        builder.Add("$label_biome", data.m_treasure.m_biome);
                        builder.Add("$label_purchasecurrency", data.m_treasure.m_currency);
                        builder.Add("$label_purchasecost", data.m_treasure.m_cost);
                        Build(builder.ToList());
                        if (TreasureHunt.TreasureHunt.ActiveTreasureLocation != null)
                        {
                            var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                            component.Setup("$label_canceltreasure", () =>
                            {
                                if (TreasureHunt.TreasureHunt.AcceptTreasure(data.m_treasure))
                                {
                                    Destroy(component.gameObject);
                                }
                            }, true);
                        }
                        else
                        {
                            var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                            component.Setup("$label_accepttreasure",
                                () =>
                                {
                                    if (TreasureHunt.TreasureHunt.CancelTreasure(data.m_treasure))
                                    {
                                        Destroy(component.gameObject);
                                    }
                                }, true);
                        }
                    };
                case AlmanacUI.ElementData.ReferenceType.Achievement:
                    return () =>
                    {
                        if (data.m_achievement == null) return;
                        Build(data.m_achievement.GetEntries());
                        switch (data.m_achievement.GetRewardType())
                        {
                            case AchievementTypes.AchievementRewardType.StatusEffect:
                                if (data.m_achievement.m_statusEffect == null) return;
                                if (EffectMan.ActiveAchievementEffects.Contains(data.m_achievement.m_statusEffect))
                                {
                                    var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                                    component.Setup("$label_removeeffect", () =>
                                    {
                                        if (data.m_achievement.Collect()) Destroy(component.gameObject);
                                    }, true);
                                }
                                else
                                {
                                    var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                                    component.Setup("$label_addeffect",
                                        () =>
                                        {
                                            if (data.m_achievement.Collect()) Destroy(component.gameObject);
                                        }, true);
                                }
                                break;
                            default:
                                if (data.m_achievement.m_collectedReward)
                                {
                                    var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                                    component.Setup("$label_alreadycollected", () =>
                                    {
                                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_alreadycollectedreward");
                                    }, true);
                                }
                                else
                                {
                                    var component = Instantiate(AlmanacUI.m_buttonElement, m_instance.m_contentRoot).GetComponent<AlmanacButton>();
                                    component.Setup("$label_collectreward",
                                        () =>
                                        {
                                            if (data.m_achievement.Collect()) Destroy(component.gameObject);
                                        }, true);
                                }
                                break;
                        }
                    };
                default: return () => { };
            }
        }

        private static TitleElement Title(string title) => Instantiate(m_titleElement, m_instance.m_contentRoot).GetComponent<TitleElement>().SetTitle(title);
        private static PanelElement KeyValue(string key, string value) => Instantiate(m_keyValueElement, m_instance.m_contentRoot).GetComponent<KeyValueElement>().Setup(key, value);
        private static PanelElement Group(List<GroupElement.TemplateData> data) => Instantiate(m_groupElement, m_instance.m_contentRoot).GetComponent<GroupElement>().Setup(data);
    }
}