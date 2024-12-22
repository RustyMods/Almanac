using System;
using System.Collections.Generic;
using Almanac.Data;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Almanac.UI.CacheAssets;
using Object = UnityEngine.Object;

namespace Almanac.UI;

public static class Categories
{
    private static GameObject ItemTabs = null!;
    private static GameObject PieceTabs = null!;
    private static GameObject AlmanacTabs = null!;
    private static GameObject SpecialTabs = null!;

    public static GameObject BaseTab = null!;

    public static string SelectedTab = "$almanac_trophies_button";

    public static readonly Dictionary<string, Action<InventoryGui, string>> m_tabs = new();

    // If you change options here, make sure to match it on other conditions
    private static readonly List<string> ItemOptions = new()
    {
        "$almanac_trophies_button",
        "$almanac_fish_button",
        "$almanac_ammo_button",
        "$almanac_consumable_button",
        "$almanac_material_button",
        "$almanac_weapon_button",
        "$almanac_gear_button",
        "$almanac_creature_button"
    };

    private static readonly List<string> PieceOptions = new()
    {
        "$almanac_miscPieces_button",
        "$almanac_craftingPieces_button",
        "$almanac_buildPieces_button",
        "$almanac_furniturePieces_button",
        "$almanac_plantPieces_button",
        "$almanac_other_button",
        "$almanac_comfortPieces_button"
    };

    private static readonly List<string> AlmanacOptions = new()
    {
        "$almanac_achievements_button", "$almanac_quests_button", "$almanac_treasure_hunt_button"
    };

    private static readonly List<string> SpecialOptions = new();

    public static void Setup()
    {
        m_tabs["$almanac_scroll_button"] = (GUI, filter) => UpdateAlmanac.UpdateItemList(GUI, ItemDataCollector.GetScrolls(filter));
        
    }

    public static void CreateTabs()
    {
        if (!ItemTabs || !PieceTabs || !AlmanacTabs || !BaseTab) return;
        SpecialOptions.Clear();
        if (AlmanacPlugin.JewelCraftLoaded) SpecialOptions.Add("$almanac_jewel_button");
        if (AlmanacPlugin.KGEnchantmentLoaded) SpecialOptions.Add("$almanac_scroll_button");
        if (PieceDataCollector.modPieces.Count > 0 && !PieceOptions.Contains("$almanac_modPieces_button")) PieceOptions.Add("$almanac_modPieces_button");
        AlmanacOptions.Clear();
        if (AlmanacPlugin._AchievementsEnabled.Value is AlmanacPlugin.Toggle.On) AlmanacOptions.Add("$almanac_achievements_button");
        if (AlmanacPlugin._BountyEnabled.Value is AlmanacPlugin.Toggle.On) AlmanacOptions.Add("$almanac_quests_button");
        if (AlmanacPlugin._TreasureEnabled.Value is AlmanacPlugin.Toggle.On) AlmanacOptions.Add("$almanac_treasure_hunt_button");
        
        CreateBaseTabs(ItemTabs, ItemOptions, -750f, 425f);
        CreateBaseTabs(PieceTabs, PieceOptions, -750f, -425f);
        CreateBaseTabs(AlmanacTabs, AlmanacOptions, 530f + 75f * (3 - AlmanacOptions.Count), 425f);
        CreateBaseTabs(SpecialTabs, SpecialOptions, -750f, 473);
    }

    private static void CreateBaseTabs(GameObject parent, List<string> options, float x, float y)
    {
        parent.SetActive(true);
        
        foreach(Transform tab in parent.transform) Object.Destroy(tab.gameObject);

        for (int index = 0; index < options.Count; ++index)
        {
            string selection = options[index];
            
            GameObject tab = Object.Instantiate(BaseTab, parent.transform);
            if (!tab.TryGetComponent(out RectTransform rect)) continue;
            rect.anchoredPosition = new Vector2(x + (index * 152f), y);
            Transform text = Utils.FindChild(tab.transform, "text");
            if (!text.TryGetComponent(out TextMeshProUGUI textMesh)) continue;
            textMesh.text = Localization.instance.Localize(selection);
            if (!tab.TryGetComponent(out Button button)) continue;
            button.onClick.AddListener(() =>
            {
                SelectedTab = selection;
                if (!InventoryGui.instance) return;
                InventoryGui.instance.UpdateTrophyList();
            });
        }
    }
    public static void DestroyTabs()
    {
        if (!BaseTab || !ItemTabs || !PieceTabs || !AlmanacTabs || !SpecialTabs) return;

        ItemTabs.SetActive(false);
        PieceTabs.SetActive(false);
        AlmanacTabs.SetActive(false);
        SpecialTabs.SetActive(false);

        foreach(Transform tab in PieceTabs.transform) Object.Destroy(tab.gameObject);
        foreach (Transform tab in ItemTabs.transform) Object.Destroy(tab.gameObject);
        foreach(Transform tab in AlmanacTabs.transform) Object.Destroy(tab.gameObject);
        foreach(Transform tab in SpecialTabs.transform) Object.Destroy(tab.gameObject);
    }
    public static bool AreTabsVisible() => ItemTabs && ItemTabs.activeSelf && PieceTabs && PieceTabs.activeSelf && AlmanacTabs && AlmanacTabs.activeSelf;
    
    public static void CreateAssets(InventoryGui GUI)
    {
        if (!InventoryGui.instance) return;
        
        if (ItemTabs == null) ItemTabs = CreateTabContainer(GUI);
        if (PieceTabs == null) PieceTabs = CreateTabContainer(GUI);
        if (AlmanacTabs == null) AlmanacTabs = CreateTabContainer(GUI);
        if (SpecialTabs == null) SpecialTabs = CreateTabContainer(GUI);
        if (BaseTab == null) BaseTab = CreateBaseTab();
    }
    
    private static GameObject CreateBaseTab()
    {
        GameObject Tab = new GameObject("tab");
        RectTransform rect = Tab.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150f, 46f);

        Image background = Tab.AddComponent<Image>();
        background.sprite = ButtonImage.sprite;
        background.material = ButtonImage.material;
        background.raycastPadding = ButtonImage.raycastPadding;
        background.raycastTarget = ButtonImage.raycastTarget;
        background.maskable = ButtonImage.maskable;
        background.type = ButtonImage.type;
        background.fillCenter = ButtonImage.fillCenter;
        background.pixelsPerUnitMultiplier = ButtonImage.pixelsPerUnitMultiplier;

        GameObject text = new GameObject("text");
        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150f, 30f);
        textRect.SetParent(rect);
        
        TextMeshProUGUI textMesh = text.AddComponent<TextMeshProUGUI>();
        textMesh.font = NorseFont;
        textMesh.fontSize = 18;
        textMesh.overflowMode = TextOverflowModes.Truncate;
        textMesh.color = new Color(0.8f, 0.5f, 0f, 1f);
        textMesh.maskable = true;
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        Button button = Tab.AddComponent<Button>();
        button.interactable = true;
        button.targetGraphic = background;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = ButtonComponent.spriteState;
        button.onClick = new Button.ButtonClickedEvent();
        
        ButtonSfx sfx = Tab.AddComponent<ButtonSfx>();
        sfx.m_sfxPrefab = ButtonSFX.m_sfxPrefab;

        return Tab;
    }

    private static GameObject CreateTabContainer(InventoryGui GUI)
    {
        GameObject container = new GameObject("container");
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.SetParent(GUI.transform);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = new Vector2();

        return container;
    }
}