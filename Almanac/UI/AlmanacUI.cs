using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.Utilities;
using HarmonyLib;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.UI;

public class AlmanacUI : MonoBehaviour
{
    public static readonly List<Element> m_elements = new();
    public static AlmanacUI m_instance = null!;
    public static GameObject m_element = null!;
    public static GameObject m_buttonElement = null!;
    
    public Dictionary<string, Func<List<ElementData>>> m_options = new();
    public RectTransform m_listRoot = null!;
    public Scrollbar m_scrollbar = null!;
    public GridLayoutGroup m_gridLayoutGroup = null!;
    public Dropdown m_dropdown = null!;
    public InputField m_searchField = null!;
    public RectTransform m_dropdownTemplate = null!;
    public Image m_background = null!;

    public Vector2 m_panelPos = new (-220f, 0f);
    public Vector2 m_spacing = new (30f, 30f);
    public Vector2 m_cellSize = new (180f, 180f);
    public float m_listHeight = 650f;
    public float m_elementHeight = 210f;
    public float m_creatureHeight = 45f;
    public Vector2 m_creatureSpacing = new (5f, 5f);
    public Vector2 m_creatureCellSize = new (310f, 40f);
    public string m_selectedTab = "";
    public Toggle m_toggle = null!;
    public Image m_toggleImage = null!;
    public Text m_toggleText = null!;
    public Image m_dropdownImg = null!;
    public Image m_templateImg = null!;
    public Image m_searchFieldImg = null!;
    public GameObject m_craftingPanel = null!;
    public GameObject m_inventoryPanel = null!;
    public GameObject m_containerPanel = null!;
    public GameObject m_infoPanel = null!;
    
    public void Update()
    {
        if (!Player.m_localPlayer || !m_instance) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Player.m_localPlayer.IsDead())
        {
            OnClose();
        }
    }

    public void Toggle()
    {
        if (m_instance.gameObject.activeInHierarchy) InventoryGui.m_instance.OnOpenTrophies();
        else InventoryGui.m_instance.Hide();
    }

    public void Init()
    {
        m_craftingPanel = transform.Find("root/Crafting").gameObject;
        m_inventoryPanel = transform.Find("root/Player").gameObject;
        m_containerPanel = transform.Find("root/Container").gameObject;
        m_infoPanel = transform.Find("root/Info").gameObject;
        
        InventoryGui.instance.m_trophiesPanel.SetActive(true);
        transform.Find("root/Trophies/TrophiesFrame").GetComponent<RectTransform>().anchoredPosition = m_panelPos;
        CreateDropdown();
        CreateToggle();
        m_dropdown.AddOptions(SetupCategories());
        m_dropdown.onValueChanged.AddListener(OnTabChange);
        m_element = Instantiate(InventoryGui.instance.m_trophieElementPrefab, AlmanacPlugin._root.transform, false);
        m_element.name = "AlmanacElement";
        m_element.gameObject.SetActive(true);
        m_element.AddComponent<AlmanacElement>();
        m_listRoot = InventoryGui.m_instance.m_trophieListRoot;
        m_scrollbar = InventoryGui.m_instance.m_trophyListScroll;
        m_gridLayoutGroup = InventoryGui.m_instance.m_trophieListRoot.gameObject.AddComponent<GridLayoutGroup>();
        m_gridLayoutGroup.cellSize = m_cellSize;
        m_gridLayoutGroup.spacing = m_spacing;
        m_background = transform.Find("root/Trophies/TrophiesFrame/border (1)").GetComponent<Image>();
        CreateSearchBar();
        Instantiate(AlmanacPlugin._UIAssets.LoadAsset<GameObject>("SidePanel"), transform).AddComponent<SidePanel>().Init();
        m_buttonElement = AlmanacPlugin._UIAssets.LoadAsset<GameObject>("CreatureElement");
        m_buttonElement.AddComponent<AlmanacButton>();
        m_buttonElement.AddComponent<ButtonSfx>().m_sfxPrefab = Assets.ButtonSFX.m_sfxPrefab;
        m_instance = this;
        InventoryGui.instance.m_trophiesPanel.SetActive(false);
        CreatureLists.Init();
        SetTransparent(AlmanacPlugin._Transparent.Value is AlmanacPlugin.Toggle.On);
        AlmanacPlugin._Transparent.SettingChanged += (_, _) =>
        {
            SetTransparent(AlmanacPlugin._Transparent.Value is AlmanacPlugin.Toggle.On);
        };
    }

    public void SetTransparent(bool enable)
    {
        m_background.color = enable ? Color.clear : Color.white;
        SidePanel.m_instance.m_background.color = enable ? Color.clear : Color.white;
    }

    public void ReloadAssets()
    {
        m_dropdownImg.sprite = Assets.ButtonImage.sprite;
        m_dropdownImg.material = Assets.WoodPanel.material;
        m_templateImg.sprite = Assets.InputFieldBkg;
        m_searchFieldImg.sprite = Assets.InputFieldBkg;
    }

    private List<string> SetupCategories()
    {
        m_options = new Dictionary<string, Func<List<ElementData>>>()
        {
            ["$title_trophies"] = () => Items.GetTrophies().Select(x => new ElementData(x)).ToList(),
            ["$title_projectiles"] = () => Items.GetAmmunition().Select(x => new ElementData(x)).ToList(),
            ["$title_fish"] = () => Items.GetFishes().Select(x => new ElementData(x)).ToList(),
            ["$title_materials"] = () => Items.GetMaterials().Select(x => new ElementData(x)).ToList(),
            ["$title_weapons"] = () => Items.GetWeapons().Select(x => new ElementData(x)).ToList(),
            ["$title_equipment"] = () => Items.GetEquipment().Select(x => new ElementData(x)).ToList(),
            ["$title_consumables"] = () => Items.GetConsumables().Select(x => new ElementData(x)).ToList(),
            ["$title_creatures"] = () => Creatures.m_creatures.Select(x => new ElementData(x.Value)).ToList(),
            ["$title_miscpieces"] = () => BuildPieces.GetPieces(Piece.PieceCategory.Misc).Select(x => new ElementData(x)).ToList(),
            ["$title_plant"] = () => BuildPieces.GetPieces("plants").Select(x => new ElementData(x)).ToList(),
            ["$title_buildpieces"] = () => BuildPieces.GetBuildPieces().Select(x => new ElementData(x)).ToList(),
            ["$title_craftingpieces"] = () => BuildPieces.GetPieces(Piece.PieceCategory.Crafting).Select(x => new ElementData(x)).ToList(),
            ["$title_furniturepieces"] = () => BuildPieces.GetPieces(Piece.PieceCategory.Furniture).Select(x => new ElementData(x)).ToList(),
            ["$title_otherpieces"] = () => BuildPieces.GetPieces("defaults").Select(x => new ElementData(x)).ToList(),
            ["$title_comfortpieces"] = () => BuildPieces.GetPieces("comforts").Select(x => new ElementData(x)).ToList(),
            ["$title_achievements"] = () => AchievementManager.GetAchievements().Select(x => new ElementData(x)).ToList(),
        };
        if (AlmanacPlugin.JewelCraftLoaded) m_options["$title_jewel"] = () => Items.GetJewels().Select(x => new ElementData(x)).ToList();
        if (AlmanacPlugin.KGEnchantmentLoaded) m_options["$title_scroll"] = () => Items.GetScrolls().Select(x => new ElementData(x)).ToList();
        if (AlmanacPlugin._BountyEnabled.Value is AlmanacPlugin.Toggle.On) m_options["$title_bounty"] = () => BountyManager.RegisteredBounties.Select(x => new ElementData(x)).ToList();
        AlmanacPlugin._BountyEnabled.SettingChanged += (_, _) =>
        {
            if (AlmanacPlugin._BountyEnabled.Value is AlmanacPlugin.Toggle.On)
            {
                if (m_options.ContainsKey("$title_bounty")) return;
                m_options["$title_bounty"] = () => BountyManager.RegisteredBounties.Select(x => new ElementData(x)).ToList();
                m_dropdown.ClearOptions();
                m_dropdown.AddOptions(m_options.Keys.Select(x=>Localization.instance.Localize(x)).ToList());
            }
            else
            {
                if (!m_options.ContainsKey("$title_bounty")) return;
                m_options.Remove("$title_bounty");
                m_dropdown.ClearOptions();
                m_dropdown.AddOptions(m_options.Keys.Select(x=>Localization.instance.Localize(x)).ToList());
            }
        };
        if (AlmanacPlugin._TreasureEnabled.Value is AlmanacPlugin.Toggle.On) m_options["$title_treasure"] = () => TreasureHunt.TreasureManager.RegisteredTreasure.Select(x => new ElementData(x)).ToList();
        AlmanacPlugin._TreasureEnabled.SettingChanged += (_, _) =>
        {
            if (AlmanacPlugin._TreasureEnabled.Value is AlmanacPlugin.Toggle.On)
            {
                if (m_options.ContainsKey("$title_treasure")) return;
                m_options["$title_treasure"] = () => TreasureHunt.TreasureManager.RegisteredTreasure.Select(x => new ElementData(x)).ToList();
                m_dropdown.ClearOptions();
                m_dropdown.AddOptions(m_options.Keys.Select(x=>Localization.instance.Localize(x)).ToList());
            }
            else
            {
                if (!m_options.ContainsKey("$title_treasure")) return;
                m_options.Remove("$title_teasure");
                m_dropdown.ClearOptions();
                m_dropdown.AddOptions(m_options.Keys.Select(x=>Localization.instance.Localize(x)).ToList());
            }
        };
        m_dropdownTemplate.localPosition = new Vector3(0f, -(m_options.Count * 19f));
        m_selectedTab = m_options.Keys.ToList()[0];
        return m_options.Keys.Select(x => Localization.instance.Localize(x)).ToList();
    }

    public void CreateToggle()
    {
        var go = new GameObject("AchievementToggle");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(transform.Find("root/Trophies"));
        rect.SetAsLastSibling();
        rect.sizeDelta = new Vector2(40f, 40f);
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = new Vector3(400f, 365f);
        var icon = new GameObject("Icon");
        var iconRect = icon.AddComponent<RectTransform>();
        iconRect.SetParent(rect);
        iconRect.sizeDelta = new Vector2(40f, 40f);
        iconRect.anchoredPosition= Vector2.zero;
        var iconImg = icon.AddComponent<Image>();
        iconImg.sprite = SpriteManager.ToggleOff;
        iconImg.color = new Color(0.5f, 0.1f, 0f, 1f);
        var hover = new GameObject("Text");
        var hoverRect = hover.AddComponent<RectTransform>();
        hoverRect.SetParent(rect);
        hoverRect.sizeDelta = new Vector2(100f, 40f);
        hoverRect.anchoredPosition=Vector2.zero;
        hoverRect.localPosition = new Vector3(0f, 20f);
        var hoverTxt = hover.AddComponent<Text>();
        hoverTxt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        hoverTxt.alignment = TextAnchor.MiddleCenter;
        hoverTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        hoverTxt.verticalOverflow = VerticalWrapMode.Overflow;
        hoverTxt.raycastTarget = false;
        hoverTxt.color = Color.clear;
        var toggle = go.AddComponent<Toggle>();
        toggle.targetGraphic = iconImg;
        var component = go.AddComponent<HoverToggle>();
        component.m_hoverText = hoverTxt;
        toggle.onValueChanged.AddListener(OnToggle);
        m_toggle = toggle;
        m_toggleImage = iconImg;
        m_toggleText = hoverTxt;
    }

    public void OnToggle(bool enable)
    {
        m_toggleText.text = Localization.instance.Localize(m_selectedTab == "$title_achievements"
            ? enable ? "$label_showall" : "$label_showcompleted"
            : enable ? "$label_showall" : "$label_showknown");
        m_toggleImage.sprite = enable ? SpriteManager.ToggleOn : SpriteManager.ToggleOff;
        m_toggleImage.color = enable ? new Color(0.2f, 0.8f, 0f, 1f) : new Color(0.5f, 0.1f, 0f, 1f);
        OnFilter(m_searchField.text);
    }

    public void SetToggleWithoutNotify(bool enable)
    {
        m_toggleText.text = Localization.instance.Localize(m_selectedTab == "$title_achievements"
            ? enable ? "$label_showall" : "$label_showcompleted"
            : enable ? "$label_showall" : "$label_showknown");
        m_toggleImage.sprite = enable ? SpriteManager.ToggleOn : SpriteManager.ToggleOff;
        m_toggleImage.color = enable ? new Color(0.2f, 0.8f, 0f, 1f) : new Color(0.5f, 0.1f, 0f, 1f);
        m_toggle.SetIsOnWithoutNotify(enable);
    }

    public void CreateDropdown()
    {
        var go = new GameObject("Dropdown");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(transform.Find("root/Trophies"));
        rect.sizeDelta = new Vector2(400f, 40f);
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = new Vector3(-180f, 365f);
        rect.SetAsLastSibling();
        m_dropdownImg = go.AddComponent<Image>();
        m_dropdownImg.sprite = Assets.ButtonImage.sprite;
        m_dropdownImg.material = Assets.LitHud;
        m_dropdownImg.type = Image.Type.Sliced;
        m_dropdownImg.fillCenter = true;
        m_dropdownImg.color = Color.white;
        // Disable trophies topic text
        var topic = transform.Find("root/Trophies/TrophiesFrame/topic").gameObject;
        topic.SetActive(false);
        var topicTxt = topic.GetComponent<TextMeshProUGUI>();
        var label = new GameObject("Label");
        var labelRect = label.AddComponent<RectTransform>();
        labelRect.SetParent(rect);
        labelRect.sizeDelta = rect.sizeDelta;
        labelRect.anchoredPosition = Vector2.zero;
        var labelTxt = label.AddComponent<Text>();
        labelTxt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        labelTxt.fontSize = 32;
        labelTxt.text = "TEST";
        labelTxt.color = topicTxt.color;
        labelTxt.alignment = TextAnchor.MiddleCenter;
        label.AddComponent<Outline>();
        var arrow = new GameObject("Arrow");
        var arrowRect = arrow.AddComponent<RectTransform>();
        arrowRect.SetParent(rect);
        arrowRect.sizeDelta = new Vector2(40f, 40f);
        arrowRect.anchoredPosition = Vector2.zero;
        arrowRect.localPosition = new Vector3(-180f, 0f);
        var arrowImg = arrow.AddComponent<Image>();
        arrowImg.material = Assets.LitHud;
        arrowImg.sprite = SpriteManager.DropdownArrow;
        arrowImg.preserveAspect = true;
        arrowImg.raycastTarget = false;
        var template = new GameObject("Template");
        var templateRect = template.AddComponent<RectTransform>();
        templateRect.SetParent(rect);
        templateRect.sizeDelta = rect.sizeDelta;
        templateRect.SetAsLastSibling();
        templateRect.anchoredPosition = Vector2.zero;
        m_dropdownTemplate = templateRect;
        m_templateImg = template.AddComponent<Image>();
        m_templateImg.sprite = Assets.InputFieldBkg;
        m_templateImg.color = new Color32(0, 0, 0, 190);
        m_templateImg.type = Image.Type.Sliced;
        m_templateImg.fillCenter = true;
        var item = new GameObject("Item");
        var itemRect = item.AddComponent<RectTransform>();
        itemRect.SetParent(templateRect);
        itemRect.sizeDelta = rect.sizeDelta;
        itemRect.anchoredPosition = Vector2.zero;
        var toggle = item.AddComponent<Toggle>();
        var itemBkg = new GameObject("Item Background");
        var itemBkgRect = itemBkg.AddComponent<RectTransform>();
        itemBkgRect.SetParent(itemRect);
        itemBkgRect.sizeDelta = rect.sizeDelta;
        itemBkgRect.anchoredPosition = Vector2.zero;
        var itemBkgImg = itemBkg.AddComponent<Image>();
        itemBkgImg.sprite = Assets.InputFieldBkg;
        itemBkgImg.color = new Color(1f, 1f, 1f, 0.1f);
        itemBkgImg.type = Image.Type.Sliced;
        itemBkgImg.fillCenter = true;
        var itemLabel = new GameObject("Item Label");
        var itemLabelRect = itemLabel.AddComponent<RectTransform>();
        itemLabelRect.SetParent(itemRect);
        itemLabelRect.sizeDelta = rect.sizeDelta;
        itemLabelRect.anchoredPosition = Vector2.zero;
        var itemLabelTxt = itemLabel.AddComponent<Text>();
        itemLabelTxt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        itemLabelTxt.fontSize = 32;
        itemLabelTxt.text = "ITEM LABEL";
        itemLabelTxt.alignment = TextAnchor.MiddleCenter;
        itemLabelTxt.raycastTarget = false;
        itemLabel.AddComponent<Outline>();
        toggle.graphic = itemBkgImg;
        var drop = go.AddComponent<Dropdown>();
        drop.targetGraphic = m_dropdownImg;
        drop.template = templateRect;
        drop.captionText = labelTxt;
        drop.itemText = itemLabelTxt;
        drop.transition = Selectable.Transition.SpriteSwap;
        drop.spriteState = Assets.ButtonComponent.spriteState;
        template.SetActive(false);
        m_dropdown = drop;
    }

    public void CreateSearchBar()
    {
        GameObject go = new GameObject("SearchField");
        var rect = go.AddComponent<RectTransform>();
        rect.SetAsLastSibling();
        rect.sizeDelta = new Vector2(460f, 40f);
        rect.SetParent(transform.Find("root/Trophies/TrophiesFrame"));
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = new Vector3(-400f, 365f);
        m_searchFieldImg = go.AddComponent<Image>();
        m_searchFieldImg.sprite = Assets.InputFieldBkg;
        m_searchFieldImg.type = Image.Type.Sliced;
        m_searchFieldImg.fillCenter = true;
        m_searchFieldImg.color = new Color32(0, 0, 0, 190);
        var placeHolder = new GameObject("Placeholder");
        var placeHolderRect = placeHolder.AddComponent<RectTransform>();
        placeHolderRect.SetParent(rect);
        placeHolderRect.anchoredPosition = Vector2.zero;
        placeHolderRect.sizeDelta = new Vector2(440f, 20f);
        var placeHolderTxt = placeHolder.AddComponent<Text>();
        placeHolderTxt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        placeHolderTxt.text = "Search...";
        placeHolderTxt.color = new Color32(255, 255, 255, 150);
        placeHolderTxt.alignment = TextAnchor.MiddleLeft;
        var text = new GameObject("Text");
        var textRect = text.AddComponent<RectTransform>();
        textRect.SetParent(rect);
        textRect.sizeDelta = placeHolderRect.sizeDelta;
        textRect.anchoredPosition = Vector2.zero;
        var textTxt = text.AddComponent<Text>();
        textTxt.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
        textTxt.fontSize = 14;
        textTxt.alignment = TextAnchor.MiddleLeft;
        textTxt.color = Color.white;
        var searchField = go.AddComponent<InputField>();
        searchField.textComponent = textTxt;
        searchField.placeholder = placeHolderTxt;
        searchField.onValueChanged.AddListener(OnFilter);
        m_searchField = searchField;
    }

    public void OnTabChange(int option)
    {
        m_selectedTab = m_options.Keys.ToList()[option];
        UpdateList();
    }

    public void Clear()
    {
        foreach (Transform child in m_listRoot) Destroy(child.gameObject);
    }

    public void UpdateList()
    {
        // Prefix bool to InventoryGui.instance.UpdateTrophyList()
        if (Player.m_localPlayer == null || !m_options.TryGetValue(m_selectedTab, out Func<List<ElementData>> func)) return;
        Clear();
        SetToggleWithoutNotify(false);
        int count = 0;
        bool creatures = m_selectedTab == "$title_creatures";
        m_gridLayoutGroup.spacing = creatures ? m_creatureSpacing : m_spacing;
        m_gridLayoutGroup.cellSize = creatures ? m_creatureCellSize : m_cellSize;
        foreach (ElementData item in func.Invoke())
        {
            if (creatures && item.m_critterData != null)
            {
                var go = Instantiate(m_buttonElement, m_listRoot);
                var component = go.GetComponent<AlmanacButton>();
                component.Setup(item.m_critterData, item.m_isKnown);
                component.m_trophyElement = new Element(go, component.m_localizedName, item);
            }
            else
            {
                var go = Instantiate(m_element, m_listRoot);
                var component = go.GetComponent<AlmanacElement>();
                component.Setup(item);
                component.m_element = new Element(go, component.m_localizedName, item);
            }

            ++count;
        }

        Resize(count, creatures);
    }

    public void OnFilter(string input)
    {
        int count = 0;
        bool creatures = m_selectedTab == "$title_creatures";
        foreach (var element in m_elements)
        {
            if (m_toggle.isOn)
            {
                if (!element.m_data.m_isKnown)
                {
                    element.m_go.SetActive(false);
                    continue;
                }
            }
            if (element.m_text.ToLower().Contains(input))
            {
                element.m_go.SetActive(true);
                ++count;
            }
            else
            {
                element.m_go.SetActive(false);
            }
        }
        Resize(count, creatures);
    }

    public void Resize(int count, bool creatures)
    {
        m_scrollbar.value = 1f;
        float newHeight;
        if (creatures)
        {
            newHeight = Mathf.CeilToInt(m_creatureHeight * Mathf.CeilToInt(count / 4f));
        }
        else
        {
            newHeight = Mathf.CeilToInt(m_elementHeight * Mathf.CeilToInt(count / 6f));
        }
        m_listRoot.offsetMin = newHeight > m_listHeight ? new Vector2(0f, -(newHeight - m_listHeight)) : Vector2.zero;
    }

    public void OnOpen()
    {
        // Postfix to InventoryGui.instance.Show()
        SidePanel.m_instance.OnOpen();
        m_infoPanel.SetActive(false);
        m_craftingPanel.SetActive(false);
        m_inventoryPanel.SetActive(false);
    }
    public void OnClose()
    {
        // Postfix to InventoryGui.instance.Hide()
        SidePanel.m_instance.OnClose();
        m_searchField.text = "";
        m_infoPanel.SetActive(true);
        m_craftingPanel.SetActive(true);
        m_inventoryPanel.SetActive(true);
    }
    public class Element
    {
        public readonly GameObject m_go;
        public readonly string m_text;
        public readonly ElementData m_data;

        public Element(GameObject go, string text, ElementData data)
        {
            m_go = go;
            m_text = text;
            m_data = data;
            m_elements.Add(this);
        }
    }
    public class ElementData
    {
        public readonly string m_name;
        public readonly string m_desc;
        public readonly Sprite? m_icon;
        public readonly bool m_isKnown;
        public readonly ReferenceType m_type;

        public readonly Items.Data? m_item;
        public readonly BuildPieces.PieceData? m_pieceData;
        public readonly Creatures.Data? m_critterData;
        public readonly TreasureHunt.Data.Treasure? m_treasure;
        public readonly Bounties.Data.ValidatedBounty? m_bounty;
        public readonly AchievementManager.Achievement? m_achievement;

        public ElementData(Bounties.Data.ValidatedBounty data)
        {
            m_bounty = data;
            m_name = data.m_creatureName;
            m_desc = data.m_biome.ToString();
            m_icon = data.m_icon;
            m_type = ReferenceType.Bounty;
        }

        public ElementData(TreasureHunt.Data.Treasure data)
        {
            m_treasure = data;
            m_name = data.m_name;
            m_desc = data.m_biome.ToString();
            m_icon = data.m_sprite;
            m_type = ReferenceType.Treasure;
        }

        public ElementData(AchievementManager.Achievement data)
        {
            data.Check();
            m_achievement = data;
            m_name = data.GetDisplayName();
            m_desc = data.GetDescription();
            m_icon = data.GetIcon();
            m_isKnown = data.IsComplete();
            m_type = ReferenceType.Achievement;
        }

        public ElementData(Creatures.Data data)
        {
            m_type = ReferenceType.Creature;
            m_critterData = data;
            m_name = data.m_name;
            m_desc = data.m_defeatKey;
            m_icon = data.GetIcon();
            m_isKnown = ZoneSystem.instance.GetGlobalKeys().Contains(data.m_defeatKey) || ZoneSystem.instance.GetGlobalKey(data.m_defeatKey) || AlmanacPlugin._KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.NoCostCheat();
        }

        public ElementData(Items.Data data)
        {
            m_type = ReferenceType.Item;
            m_item = data;
            m_name = data.m_item.m_itemData.m_shared.m_name;
            m_desc = data.m_item.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Trophy
                ? data.m_item.m_itemData.m_shared.m_name + "_lore"
                : data.m_item.m_itemData.m_shared.m_description;
            m_icon = data.GetIcon();
            m_isKnown = AlmanacPlugin._KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsKnownMaterial(data.m_item.m_itemData.m_shared.m_name) || Player.m_localPlayer.NoCostCheat();

        }

        public ElementData(BuildPieces.PieceData data)
        {
            m_type = ReferenceType.Piece;
            m_pieceData = data;
            m_name = data.m_piece.m_name;
            if (BuildPieces.m_localizeOverride.TryGetValue(data.m_piece.name, out string localizeOverride)) m_name = localizeOverride;
            m_desc = data.m_piece.m_description;
            m_icon = data.m_piece.m_icon;
            m_isKnown = AlmanacPlugin._KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsRecipeKnown(data.m_piece.m_name) || Player.m_localPlayer.NoCostCheat();
        }
        public enum ReferenceType
        {
            Item,
            Creature,
            Piece,
            Treasure,
            Bounty,
            Achievement
        }
    }
    
    #region Patches
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnOpenTrophies))]
    private static class UpdateAlmanacAssets
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            m_instance.OnOpen();
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCloseTrophies))]
    private static class DestroyAlmanacAssets
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            m_instance.OnClose();
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateTrophyList))]
    private static class UpdateTrophyListPatch
    {
        private static bool Prefix()
        {
            // Override
            m_instance.UpdateList();
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    private static class InventoryGUI_Awake_Patch
    {
        private static void Postfix(InventoryGui __instance)
        {
            Assets.Cache();
            __instance.gameObject.AddComponent<AlmanacUI>().Init();
            Transform info = Utils.FindChild(__instance.m_inventoryRoot.transform, "Info");
            Transform trophiesOpenButton = Utils.FindChild(info, "Trophies");
            Transform image = Utils.FindChild(trophiesOpenButton, "Image");

            if (trophiesOpenButton.TryGetComponent(out UITooltip openTrophiesToolTip))
            {
                openTrophiesToolTip.m_text = "$title_almanac";
            }
            if (image.TryGetComponent(out Image trophiesOpenImage))
            {
                trophiesOpenImage.sprite = SpriteManager.AlmanacIcon;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
    private static class PlayerControllerOverride
    {
        private static bool Prefix() => !IsTrophyPanelActive();
    }
    
    // Pressing interact key would close trophy panel
    // This patch prevents that
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetButtonDown))]
    private static class ZInputPatch
    {
        private static bool Prefix() => !IsTrophyPanelActive();
    }
    
    private static bool IsTrophyPanelActive()
    {
        if (!InventoryGui.instance) return true;
        return InventoryGui.instance.m_trophiesPanel && InventoryGui.instance.m_trophiesPanel.activeInHierarchy;
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    private static class AlmanacHidePatch
    {
        private static void Postfix()
        {
            if (!m_instance) return;
            m_instance.OnClose();
        }
    }
    #endregion
}