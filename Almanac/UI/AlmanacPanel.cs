using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.Lottery;
using Almanac.Managers;
using Almanac.Marketplace;
using Almanac.Store;
using Almanac.TreasureHunt;
using Almanac.Utilities;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Almanac.UI;

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
public static class InventoryGui_Awake_Patch
{
    [UsedImplicitly]
    private static void Postfix(InventoryGui __instance)
    {
        GameObject craftingPanel = __instance.m_crafting.gameObject;
        GameObject? trophyPanel = __instance.m_trophiesPanel;
        GameObject? sfx = craftingPanel.GetComponentInChildren<ButtonSfx>().m_sfxPrefab;

        if (DialoguePanel._panel != null)
        {
            Text[] npcDialogueText = DialoguePanel._panel.GetComponentsInChildren<Text>(true);
            foreach (var component in DialoguePanel._panel.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
            FontManager.SetFont(npcDialogueText);
            DialoguePanel._panel.AddComponent<DialoguePanel>();
            GameObject dialoguePanel = Object.Instantiate(DialoguePanel._panel, __instance.transform.parent.Find("HUD"));
            dialoguePanel.name = "Almanac NPC Dialogue";
        }

        Text[] npcTexts = NPCCustomization._Modal.GetComponentsInChildren<Text>(true);
        foreach (var component in NPCCustomization._Modal.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
        FontManager.SetFont(npcTexts);
        NPCCustomization._Modal.CopySpriteAndMaterial(trophyPanel, "bkg", "TrophiesFrame/border (1)");
        NPCCustomization._Modal.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/Element/Title/TabBorder", "TabsButtons/TabBorder");
        NPCCustomization._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        NPCCustomization._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar/Sliding Area/Handle", "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        NPCCustomization._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");
        NPCCustomization._Modal.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/Element/InputField/Glow", "RepairButton/Glow");
        NPCCustomization._Modal.CopySpriteAndMaterial(trophyPanel, "MainButton", "TrophiesFrame/Closebutton");
        NPCCustomization._Modal.CopyButtonState(trophyPanel, "MainButton", "TrophiesFrame/Closebutton");
        NPCCustomization._Modal.AddComponent<NPCCustomization>();
        GameObject npc_modal = Object.Instantiate(NPCCustomization._Modal, __instance.transform.parent.Find("HUD"));
        npc_modal.name = "Almanac NPC UI";
        
        Text[]? modalTexts = Modal._Modal.GetComponentsInChildren<Text>(true);
        foreach (ButtonSfx? component in Modal._Modal.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
        FontManager.SetFont(modalTexts);
        
        Modal._Modal.CopySpriteAndMaterial(trophyPanel, "bkg", "TrophiesFrame/border (1)");
        Modal._Modal.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/Title/TabBorder", "TabsButtons/TabBorder");
        Modal._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        Modal._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar/Sliding Area/Handle", "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        Modal._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");
        Modal._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport/Button", "TrophiesFrame/Closebutton");
        Modal._Modal.CopyButtonState(trophyPanel, "ListView/Viewport/Button", "TrophiesFrame/Closebutton");
        Modal._Modal.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/InputField/Glow", "RepairButton/Glow");

        Modal._Modal.CopySpriteAndMaterial(trophyPanel, "MainButton", "TrophiesFrame/Closebutton");
        Modal._Modal.CopyButtonState(trophyPanel, "MainButton", "TrophiesFrame/Closebutton");
    
        Modal._Modal.AddComponent<Modal>();
        
        GameObject panel = AssetBundleManager.LoadAsset<GameObject>("almanac_ui", "AlmanacUI")!;
        GameObject go = Object.Instantiate(panel, __instance.transform.parent.Find("HUD"));
        go.name = "Almanac";
        
        Text[]? panelTexts = go.GetComponentsInChildren<Text>(true);
        foreach (ButtonSfx? component in go.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
        FontManager.SetFont(panelTexts);
        
        go.CopySpriteAndMaterial(trophyPanel, "bkg", "TrophiesFrame/border (1)");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Trophies", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Trophies/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Trophies", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Pieces", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Pieces/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Pieces", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Items", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Items/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Items", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Creatures", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Creatures/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Creatures", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/StatusEffects", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/StatusEffects/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/StatusEffects", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Achievements", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Achievements/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Achievements", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Bounties", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Bounties/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Bounties", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Treasures", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Treasures/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Treasures", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Store", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Store/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Store", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Metrics", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Metrics/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Metrics", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Leaderboard", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Leaderboard/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Leaderboard", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Lottery", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Lottery/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Lottery", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "TabBorder", "TabsButtons/TabBorder");
        
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar/Sliding Area/Handle", "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");

        go.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport/ListElement/Background", "TrophiesFrame/Trophies/TrophyList/TrophyElement/icon_bkg");
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport/ListElement/Background/Icon", "TrophiesFrame/Trophies/TrophyList/TrophyElement/icon_bkg/icon");
        go.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/ListElement/Selected", "RepairButton/Glow");

        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Scrollbar/Sliding Area/Handle", "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Viewport", "TrophiesFrame/Trophies/TrophyList");
        
        go.CopySpriteAndMaterial(trophyPanel, "GamblePanel/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "GamblePanel/Scrollbar/Sliding Area/Handle", "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "GamblePanel/Viewport", "TrophiesFrame/Trophies/TrophyList");
        go.CopySpriteAndMaterial(craftingPanel, "GamblePanel/Viewport/Slot/Port/Viewport/Element/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "GamblePanel/Viewport/Slot/Port/Viewport/Element/Glow", "RepairButton/Glow");

        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Viewport/Button", "TrophiesFrame/Closebutton");
        go.CopyButtonState(trophyPanel, "ButtonView/Viewport/Button", "TrophiesFrame/Closebutton");
        go.CopySpriteAndMaterial(craftingPanel, "ButtonView/Viewport/Button/Selected", "RepairButton/Glow");

        go.CopySpriteAndMaterial(trophyPanel, "Closebutton", "TrophiesFrame/Closebutton");
        go.CopyButtonState(trophyPanel, "Closebutton", "TrophiesFrame/Closebutton");

        go.CopySpriteAndMaterial(craftingPanel, "Description", "Decription");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Icon", "Decription/Icon");
        
        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Scrollbar/Sliding Area/Handle", "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");
        
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Title/TabBorder", "TabsButtons/TabBorder");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/1", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/1/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/2", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/2/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/3", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/3/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/4", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/4/Icon", "Decription/requirements/res_bkg/res_icon");

        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/1", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/2", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/3", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/4", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/1/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/2/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/3/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/4/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/Star", "Decription/requirements/level");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/Star/Icon", "Decription/requirements/level/MinLevel");
        
        go.CopySpriteAndMaterial(craftingPanel, "Description/MainButton", "Decription/craft_button_panel/CraftButton");
        go.CopyButtonState(craftingPanel, "Description/MainButton", "Decription/craft_button_panel/CraftButton");
        
        go.CopySpriteAndMaterial(craftingPanel, "SearchField", "Decription/craft_button_panel/CraftButton");
        go.CopySpriteAndMaterial(craftingPanel, "SearchField (1)", "Decription/craft_button_panel/CraftButton");
        
        go.CopySpriteAndMaterial(craftingPanel, "Currency/Icon", "Decription/requirements/res_bkg/res_icon");
        
        go.AddComponent<AlmanacPanel>();
        
        AlmanacPanel.inventoryButton = new AlmanacPanel.AlmanacButton(__instance);
        AlmanacPanel.inventoryButton.Show(Configs.ShowAlmanac);
    }
}

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnOpenTrophies))]
public static class InventoryGui_OnOpenTrophies_Prefix
{
    [UsedImplicitly]
    private static bool Prefix(InventoryGui __instance)
    {
        if (AlmanacPanel.instance == null || !Configs.ShowAlmanac) return true;
        AlmanacPanel.instance.Show();
        __instance.Hide();
        return false;
    }
}
    
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCloseTrophies))]
public static class InventoryGui_OnCloseTrophies_Postfix
{
    [UsedImplicitly]
    private static void Postfix() => AlmanacPanel.instance?.Hide();
}

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateTrophyList))]
public static class InventoryGui_UpdateTrophyList_Prefix
{
    [UsedImplicitly]
    private static bool Prefix() => !Configs.ShowAlmanac;
}

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
public static class InventoryGui_Hide_Prefix
{
    [UsedImplicitly]
    private static bool Prefix() => !AlmanacPanel.InSearchField();
}

[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.TakeInput))]
public static class PlayerController_TakeInput_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result &= !AlmanacPanel.InSearchField();
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.TakeInput))]
public static class PlayerTakeInput_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result &= !AlmanacPanel.InSearchField();
    } 
}

[HarmonyPatch(typeof(Chat), nameof(Chat.HasFocus))]
public static class Chat_HasFocus_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result &= !AlmanacPanel.InSearchField();
    } 
}

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.IsVisible))]
public static class InventoryGui_IsVisible_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result |= AlmanacPanel.IsVisible() || NPCCustomization.IsVisible() || DialoguePanel.IsVisible();
    }
}

public class AlmanacPanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public class AlmanacButton
    {
        private readonly UITooltip tooltip;
        private readonly Image icon;

        private readonly Sprite defaultIcon;
        private readonly string defaultTooltip;

        private bool replaced;
        public AlmanacButton(InventoryGui instance)
        {
            Transform info = Utils.FindChild(instance.m_inventoryRoot.transform, "Info");
            Transform trophiesOpenButton = Utils.FindChild(info, "Trophies");
            Transform image = Utils.FindChild(trophiesOpenButton, "Image");
            
            tooltip = trophiesOpenButton.GetComponent<UITooltip>();
            icon = image.GetComponent<Image>();
            defaultIcon = icon.sprite;
            defaultTooltip = tooltip.m_text;
        }

        public void Show(bool enable)
        {
            if (enable) Replace();
            else Revert();
        }

        private void Replace()
        {
            if (replaced) return;
            SetIcon(SpriteManager.AlmanacIcon);
            SetTooltip(Keys.Almanac);
            replaced = true;
        }

        private void Revert()
        {
            if (!replaced) return;
            SetIcon(defaultIcon);
            SetTooltip(defaultTooltip);
            replaced = false;
        }

        private void SetTooltip(string text) => tooltip.m_text = text;
        private void SetIcon(Sprite sprite) => icon.sprite = sprite;
    }
    public static AlmanacButton? inventoryButton;
    
    private static readonly Tutorials.Tutorial achievements = new (Keys.Achievement, "Achievements.md");
    private static readonly Tutorials.Tutorial bounties = new (Keys.Bounties, "Bounties.md");
    private static readonly Tutorials.Tutorial intro = new (Keys.Almanac, "Intro.md");
    private static readonly Tutorials.Tutorial leaderboard = new(Keys.Leaderboard, "Leaderboard.md");
    private static readonly Tutorials.Tutorial store = new(Keys.AlmanacStore, "Store.md");
    private static readonly Tutorials.Tutorial treasures = new (Keys.Treasures, "Treasures.md");
    
    public static ConfigEntry<Vector3> panelPos = null!;
    public Background background = null!;
    public Text topic = null!;
    public readonly Dictionary<Tab.TabOption, Tab> Tabs = new();
    public ButtonView buttonView = null!;
    public ElementView elementView = null!;
    private Lottery lottery = null!;
    public RightPanel description = null!;
    private Search mainSearch = null!;
    private Search sideSearch = null!;
    private ScrollRect[]? scrollRects;
    private ButtonElement close = null!;
    public Currency currency = null!;
    private Vector3 mouseDifference = Vector3.zero;
    public Action<float>? OnUpdate;
    public Action? OnMainButton;
    
    public static AlmanacPanel? instance;
    private static Modal? modal;
    private Modal.ModalBuilder modalBuilder = null!;
    private const float Input_Cooldown = 0.1f;
    private float lastInputTime;
    public static bool isLocalAdminOrHostAndNoCost
    {
        get
        {
            if (!Player.m_localPlayer || !ZNet.instance) return false;
            bool isAdmin = ZNet.instance?.LocalPlayerIsAdminOrHost() ?? false;
            return isAdmin && Player.m_localPlayer.NoCostCheat();
        }
    }
    
    public void Awake()
    {
        instance = this;
        background = new Background(transform);
        topic = transform.Find("topic").GetComponent<Text>();
        Tabs[Tab.TabOption.Trophies] = new Tab(transform.Find("Tabs/Trophies"), OnTrophyTab);
        Tabs[Tab.TabOption.Pieces] = new Tab(transform.Find("Tabs/Pieces"), OnPieceTab);
        Tabs[Tab.TabOption.Items] = new Tab(transform.Find("Tabs/Items"), OnItemTab);
        Tabs[Tab.TabOption.Creatures] = new Tab(transform.Find("Tabs/Creatures"), OnCreatureTab);
        Tabs[Tab.TabOption.StatusEffects] = new Tab(transform.Find("Tabs/StatusEffects"), OnStatusEffectTab);
        Tabs[Tab.TabOption.Achievements] = new Tab(transform.Find("Tabs/Achievements"), OnAchievementTab);
        Tabs[Tab.TabOption.Bounties] = new Tab(transform.Find("Tabs/Bounties"), OnBountyTab);
        Tabs[Tab.TabOption.Treasures] = new Tab(transform.Find("Tabs/Treasures"), OnTreasureTab);
        Tabs[Tab.TabOption.Store] = new Tab(transform.Find("Tabs/Store"), OnStoreTab);
        Tabs[Tab.TabOption.Metrics] = new Tab(transform.Find("Tabs/Metrics"), OnMetricsTab);
        Tabs[Tab.TabOption.Leaderboard] = new Tab(transform.Find("Tabs/Leaderboard"), OnLeaderboardTab);
        Tabs[Tab.TabOption.Lottery] = new Tab(transform.Find("Tabs/Lottery"), OnLotteryTab);

        elementView = new ElementView(transform.Find("ListView"));
        buttonView = new ButtonView(transform.Find("ButtonView"));
        lottery = new Lottery(transform.Find("GamblePanel"));
        description = new RightPanel(transform.Find("Description"));
        mainSearch = new Search(transform.Find("SearchField"));
        sideSearch = new Search(transform.Find("SearchField (1)"));
        currency = new  Currency(transform.Find("Currency"));
        close = new ButtonElement(transform.Find("Closebutton"), Hide);
    }

    public void Start()
    {
        transform.position = panelPos.Value;
        mainSearch.OnValueChanged(input => GridView.activeView?.OnSearch(input));
        mainSearch.SetPlaceholder(Keys.SearchPlaceholder);
        sideSearch.OnValueChanged(input => description.view.OnSearch(input));
        sideSearch.SetPlaceholder(Keys.SearchPlaceholder);
        description.OnClick(() => OnMainButton?.Invoke());
        close.SetLabel(Keys.Close);
        topic.gameObject.SetActive(false);
        currency.SetIcon(SpriteManager.GetSprite(SpriteManager.IconOption.SilverCoins));
        background.SetBackground(Configs.Transparent ? Background.BackgroundOption.Transparent : Background.BackgroundOption.Opaque);
        elementView.SetSelectedColor(Configs.OutlineColor);
        buttonView.SetSelectedColor(Configs.OutlineColor);

        modal = Instantiate(Modal._Modal, transform).GetComponent<Modal>();
        modal.SetActive(false);
        modalBuilder = new Modal.ModalBuilder(modal, elementView);
        
        Tabs[Tab.TabOption.Trophies].SetLabel(Keys.Trophies);
        Tabs[Tab.TabOption.Pieces].SetLabel(Keys.Pieces);
        Tabs[Tab.TabOption.Items].SetLabel(Keys.Items);
        Tabs[Tab.TabOption.Creatures].SetLabel(Keys.Creature);
        Tabs[Tab.TabOption.StatusEffects].SetLabel(Keys.StatusEffect);
        Tabs[Tab.TabOption.Achievements].SetLabel(Keys.Achievement);
        Tabs[Tab.TabOption.Bounties].SetLabel(Keys.Bounties);
        Tabs[Tab.TabOption.Treasures].SetLabel(Keys.Treasures);
        Tabs[Tab.TabOption.Store].SetLabel(Keys.Store);
        Tabs[Tab.TabOption.Lottery].SetLabel(Keys.Lottery);
        Tabs[Tab.TabOption.Metrics].SetLabel(Keys.Metrics);
        Tabs[Tab.TabOption.Leaderboard].SetLabel(Keys.Leaderboard);
        
        Tabs[Tab.TabOption.Trophies].SetActive(Configs.ShowTrophies);
        Tabs[Tab.TabOption.Items].SetActive(Configs.ShowItems);
        Tabs[Tab.TabOption.Pieces].SetActive(Configs.ShowPieces);
        Tabs[Tab.TabOption.Creatures].SetActive(Configs.ShowCreatures);
        Tabs[Tab.TabOption.StatusEffects].SetActive(Configs.ShowStatusEffects);
        Tabs[Tab.TabOption.Achievements].SetActive(Configs.ShowAchievements);
        Tabs[Tab.TabOption.Bounties].SetActive(Configs.ShowBounties);
        Tabs[Tab.TabOption.Treasures].SetActive(Configs.ShowTreasures);
        Tabs[Tab.TabOption.Store].SetActive(Configs.ShowStore);
        Tabs[Tab.TabOption.Lottery].SetActive(Configs.ShowLottery);
        Tabs[Tab.TabOption.Metrics].SetActive(Configs.ShowMetrics);
        Tabs[Tab.TabOption.Leaderboard].SetActive(Configs.ShowLeaderboard);
        
        scrollRects = GetComponentsInChildren<ScrollRect>(true);
        OnScrollbarSensitivityChanged(Configs.ScrollbarSensitivity);
        SetTopic(Keys.Almanac);
        elementView.SetSelectedColor(Configs.OutlineColor);
        elementView.SetGridElementSize(Configs.ElementViewGridSize);
    }

    public static void OnScrollbarSensitivityChanged(float sensitivity)
    {
        if (instance?.scrollRects == null) return;
        foreach(ScrollRect scrollbar in instance.scrollRects) scrollbar.scrollSensitivity = sensitivity;
    }

    public void Update()
    {
        if (!IsVisible()) return;
        if (!Player.m_localPlayer || Player.m_localPlayer.IsDead())
        {
            Hide();
            return;
        }

        float dt = Time.deltaTime;
        if (Time.time - lastInputTime > Input_Cooldown && (ZInput.GetKeyDown(KeyCode.Escape) || (ZInput.GetKeyDown(KeyCode.Tab) && !InSearchField())))
        {
            lastInputTime = Time.time;
            Hide();
            return;
        }
        OnUpdate?.Invoke(dt);
        lottery.UpdateGlow(dt);
        GridView.activeView?.HandleArrowKeys();
    }
    public void OnDestroy()
    {
        instance = null;
        GridView.views.Clear();
    }

    public static bool IsVisible() => instance?.gameObject.activeInHierarchy ?? false;

    public void Show(Tab.TabOption option = Tab.TabOption.Trophies)
    {
        gameObject.SetActive(true);

        switch (option)
        {
            case Tab.TabOption.Items:
                OnItemTab();
                break;
            case Tab.TabOption.Creatures:
                OnCreatureTab();
                break;
            case Tab.TabOption.Pieces:
                OnPieceTab();
                break;
            case Tab.TabOption.Leaderboard:
                OnLeaderboardTab();
                break;
            case Tab.TabOption.Achievements:
                OnAchievementTab();
                break;
            case Tab.TabOption.Bounties:
                OnBountyTab();
                break;
            case Tab.TabOption.Treasures:
                OnTreasureTab();
                break;
            case Tab.TabOption.Store:
                OnStoreTab();
                break;
            case Tab.TabOption.Lottery:
                OnLotteryTab();
                break;
            case Tab.TabOption.Metrics:
                OnMetricsTab();
                break;
            default:
                OnTrophyTab();
                break;
        }
        
        currency.SetAmount(Player.m_localPlayer.GetTokens());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        GridView.activeView = null;
    }

    private void Reset() => Reset(null);

    private void Reset(Tutorials.Tutorial? info)
    {
        buttonView.Clear();
        elementView.Clear();
        description.Reset();
        if (info != null)
        {
            description.SetName(info.label);
            description.view.CreateTextArea().SetText(info.tooltip);
            description.view.Resize();
        }
        mainSearch.Reset();
        sideSearch.Reset();
        OnUpdate = null;
        OnMainButton = null;
    }

    public static bool InSearchField() => instance?.mainSearch.IsSearching() == true || instance?.sideSearch.IsSearching() == true || Modal.IsFocused() || NPCCustomization.IsVisible();

    public void OnTrophyTab()
    {
        Tabs[Tab.TabOption.Trophies].SetSelected(true);
        Reset(intro);
        elementView.SetActive(true);
        foreach (ItemHelper.ItemInfo data in ItemHelper.trophies)
        {
            bool isKnown = !Configs.UseKnowledgeWall || Player.m_localPlayer.IsKnownMaterial(data.shared.m_name) || Player.m_localPlayer.NoCostCheat();
            if (Configs.HideUnknownEntries && !isKnown) continue;
            ElementView.Element item = elementView.Create();
            item.isKnown = isKnown;
            item.SetName(isKnown ? data.shared.m_name : "???");
            item.SetDescription(isKnown ? data.shared.m_name + "_lore" : string.Empty);
            item.SetIcon(data.GetIcon());
            item.SetIconColor(isKnown ? Color.white : Color.black);
            item.Interactable(isKnown);
            item.OnClick(() => data.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }
    
    public void OnPieceTab()
    {
        Tabs[Tab.TabOption.Pieces].SetSelected(true);
        Reset(intro);
        elementView.SetActive(true);

        foreach (PieceHelper.PieceInfo data in PieceHelper.GetPieces())
        {
            bool isKnown = Player.m_localPlayer.IsPieceKnown(data.piece) || !Configs.UseKnowledgeWall || Player.m_localPlayer.NoCostCheat();
            if (Configs.HideUnknownEntries && !isKnown) continue;
            ElementView.Element item = elementView.Create();
            item.isKnown = isKnown;
            item.SetPieceInfo(data);
            item.SetName(isKnown ? data.piece.m_name : "???");
            item.SetDescription(isKnown ? data.piece.m_description : string.Empty);
            item.SetIcon(data.piece.m_icon);
            item.SetIconColor(isKnown ? Color.white : Color.black);
            item.Interactable(isKnown);
            item.OnClick(() => data.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnItemTab()
    {
        Tabs[Tab.TabOption.Items].SetSelected(true);
        Reset(intro);
        elementView.SetActive(true);
        foreach (ItemHelper.ItemInfo data in ItemHelper.GetItemsWhileIgnoring(ItemDrop.ItemData.ItemType.Trophy))
        {
            bool isKnown = Player.m_localPlayer.NoCostCheat() || !Configs.UseKnowledgeWall || Player.m_localPlayer.IsKnownMaterial(data.shared.m_name);
            if (Configs.HideUnknownEntries && !isKnown) continue;
            ElementView.Element item = elementView.Create();
            item.isKnown = isKnown;
            item.SetItemInfo(data);
            item.SetName(isKnown ? data.shared.m_name : "???");
            item.SetDescription(isKnown ? data.shared.m_description : string.Empty);
            item.SetIcon(data.GetIcon());
            item.SetIconColor(isKnown ? Color.white : Color.black);
            item.Interactable(isKnown);
            item.OnClick(() => data.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnCreatureTab()
    {
        Tabs[Tab.TabOption.Creatures].SetSelected(true);
        Reset(intro);
        buttonView.SetActive(true);
        foreach (CritterHelper.CritterInfo data in CritterHelper.GetCritters())
        {
            bool isKnown = Player.m_localPlayer.NoCostCheat() || !Configs.UseKnowledgeWall || data.isKnown();
            if (Configs.HideUnknownEntries && !isKnown) continue;
            ButtonView.ElementButton item = buttonView.Create();
            item.isKnown = isKnown;
            item.SetLabel(isKnown ? data.character.m_name : "???");
            item.Interactable(isKnown);
            item.OnClick(() => data.OnClick(this, item));
        }
        buttonView.Resize(buttonView.Count);
    }

    public void OnStatusEffectTab()
    {
        Tabs[Tab.TabOption.StatusEffects].SetSelected(true);
        Reset(intro);
        elementView.SetActive(true);

        if (isLocalAdminOrHostAndNoCost)
        {
            modalBuilder.Build(Modal.ModalBuilder.FormType.StatusEffect);
        }
        
        foreach (StatusEffect? se in SEHelpers.statusEffects)
        {
            bool isKnown = !Configs.UseKnowledgeWall || Player.m_localPlayer.NoCostCheat() || PlayerInfo.IsStatusEffectKnown(se.NameHash());
            if (Configs.HideUnknownEntries && !isKnown) continue;
            ElementView.Element item = elementView.Create();
            item.isKnown = isKnown;
            item.SetName(isKnown ? se.m_name : "???");
            item.SetIcon(se.m_icon);
            item.SetIconColor(isKnown ? Color.white : Color.black);
            item.SetDescription(isKnown ? se.m_tooltip : string.Empty);
            item.Interactable(isKnown);
            item.OnClick(() =>
            {
                elementView.SetSelected(item);
                description.Reset();
                if (isLocalAdminOrHostAndNoCost)
                {
                    description.Interactable(true);
                    description.SetButtonText(Keys.Give);
                    OnMainButton = () => Player.m_localPlayer.GetSEMan().AddStatusEffect(se.NameHash());
                }
                description.SetName(se.m_name);
                description.SetIcon(se.m_icon);
                description.view.CreateKeyValue().SetText(Keys.InternalID, se.name);
                description.view.CreateKeyValue().SetText(Keys.Duration, StatusEffect.GetTimeString(se.m_ttl));
                description.view.CreateTextArea().SetText(se.GetTooltipString());
                description.view.Resize();
            });
        }
        elementView.Resize(elementView.Count);
    }

    public void OnAchievementTab()
    {
        Tabs[Tab.TabOption.Achievements].SetSelected(true);
        Reset(achievements);
        elementView.SetActive(true);
        if (isLocalAdminOrHostAndNoCost)
        {
            modalBuilder.Build(Modal.ModalBuilder.FormType.Achievement);
        }
        foreach (AchievementManager.Achievement achievement in AchievementManager.achievements.Values)
        {
            ElementView.Element item = elementView.Create();
            bool isCompleted = achievement.IsCompleted(Player.m_localPlayer);
            item.SetName(achievement.Name);
            item.SetIcon(achievement.icon);
            item.SetIconColor(isCompleted ? Color.white : Color.black);
            item.SetDescription(achievement.Lore);
            item.Interactable(true);
            item.OnClick(() => achievement.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnBountyTab()
    {
        Tabs[Tab.TabOption.Bounties].SetSelected(true);
        Reset(bounties);
        elementView.SetActive(true);
        if (isLocalAdminOrHostAndNoCost) modalBuilder.Build(Modal.ModalBuilder.FormType.Bounty);
        foreach (BountyManager.BountyData? bounty in BountyManager.bounties.Values)
        {
            bool hasReqs = bounty.HasRequirements(Player.m_localPlayer);
            if (Configs.HideUnknownEntries && !hasReqs) continue;
            ElementView.Element item = elementView.Create();
            item.isKnown = hasReqs;
            item.SetName(hasReqs ? bounty.character?.m_name ?? "<color=red>Invalid</color>" : "???");
            item.SetDescription(bounty.GetNameOverride());
            item.SetIcon(bounty.icon);
            item.SetIconColor(hasReqs ? Color.white : Color.black);
            item.Interactable(hasReqs);
            item.OnClick(() => bounty.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnTreasureTab()
    {
        Tabs[Tab.TabOption.Treasures].SetSelected(true);  
        Reset(treasures);
        elementView.SetActive(true);
        if (isLocalAdminOrHostAndNoCost)
        {
            modalBuilder.Build(Modal.ModalBuilder.FormType.Treasure);
        }
        foreach (TreasureManager.TreasureData? treasure in TreasureManager.treasures.Values)
        {
            bool isKnown = Player.m_localPlayer.NoCostCheat() || Player.m_localPlayer.IsBiomeKnown(treasure.biome);
            if (Configs.HideUnknownEntries && !isKnown) continue;
            ElementView.Element item = elementView.Create();
            item.isKnown = isKnown;
            item.SetName(isKnown ? treasure.Name : "???");
            item.SetIcon(treasure.icon);
            item.SetDescription(string.Empty);
            item.SetIconColor(isKnown ? Color.white : Color.black);
            item.Interactable(isKnown);
            item.OnClick(() => treasure.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnStoreTab()
    {
        if (!Player.m_localPlayer) return;
        Tabs[Tab.TabOption.Store].SetSelected(true);
        Reset(store);
        elementView.SetActive(true);
        
        if (isLocalAdminOrHostAndNoCost)
        {
            modalBuilder.Build(Modal.ModalBuilder.FormType.StoreItem);
        }

        if (Configs.AllowConversion)
        {
            foreach (StoreManager.ConversionItem conversion in StoreManager.conversions)
            {
                ElementView.Element item = elementView.Create();
                item.SetIcon(conversion.icon);
                item.SetName(conversion.name);
                item.SetDescription(conversion.description);
                item.Interactable(true);
                item.OnClick(() => conversion.OnClick(this));
            }   
        }

        if (Configs.ShowMarketplace)
        {
            ElementView.Element sell = elementView.Create();
            sell.SetIcon(SpriteManager.GetSprite(SpriteManager.IconOption.SilverKey));
            sell.SetName(Keys.Inventory);
            sell.SetDescription(Keys.SellYourItems);
            sell.Interactable(true);
            sell.OnClick(OnMarketplace);
            ElementView.Element revenue = elementView.Create();
            revenue.SetIcon(SpriteManager.GetSprite(SpriteManager.IconOption.SilverCoins));
            revenue.SetName(Keys.Revenue);
            revenue.SetDescription(MarketManager.GetRevenue(Player.m_localPlayer).ToString());
            revenue.Interactable(MarketManager.HasRevenue(Player.m_localPlayer));
            revenue.OnClick(() => MarketManager.CollectRevenue(Player.m_localPlayer));
            foreach (MarketManager.MarketItem? playerItem in MarketManager.GetMarketItems())
            {
                ElementView.Element item = elementView.Create();
                item.SetIcon(playerItem.itemData.GetIcon());
                string formattedName = playerItem.itemData.m_shared.m_name + $" x{playerItem.Stack}";
                item.SetName(formattedName);
                item.SetDescription(playerItem.itemData.m_shared.m_description);
                item.Interactable(true);
                item.OnClick(() => playerItem.OnClick(this, item));
            }
        }
        foreach (StoreManager.StoreItem? data in StoreManager.GetStoreItems())
        {
            ElementView.Element item = elementView.Create();
            bool hasReqs = data.HasRequirements(Player.m_localPlayer);
            item.isKnown = hasReqs;
            if (Configs.HideUnknownEntries && !hasReqs) continue;
            item.SetName(hasReqs ? data.Name : "???");
            item.SetDescription(hasReqs ? data.Lore : string.Empty);
            item.SetIcon(data.sprite);
            item.SetIconColor(hasReqs ? Color.white : Color.black);
            item.Interactable(hasReqs);
            item.OnClick(() => data.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnMarketplace()
    {
        Tabs[Tab.TabOption.Store].SetSelected(true);
        Reset(store);
        elementView.SetActive(true);
        foreach (ItemDrop.ItemData itemData in Player.m_localPlayer.GetInventory().GetAllItems())
        {
            if (!itemData.m_shared.m_teleportable) continue;
            ElementView.Element item = elementView.Create();
            string formattedName = itemData.m_shared.m_name + $" x{itemData.m_stack}";
            item.SetIcon(itemData.GetIcon());
            item.SetName(formattedName);
            item.SetDescription(itemData.m_shared.m_description);
            item.Interactable(true);
            item.OnClick(() =>
            {
                elementView.SetSelected(item);
                description.Reset();
                description.SetName(formattedName);
                description.SetIcon(itemData.GetIcon());
                description.Interactable(true);
                description.SetButtonText(Keys.SetupSale);
                OnMainButton = () => modalBuilder.SetupSale(itemData);
                description.view.CreateTextArea().SetText(Helpers.ReplacePositionTags(itemData.GetTooltip()) + "\n");
                if (itemData.HasSockets())
                {
                    List<string> jewels = itemData.GetSocketedGemSharedNames();
                    description.view.CreateTitle().SetTitle($"Sockets ({jewels.Count})");
                    foreach (string? jewel in jewels)
                    {
                        description.view.CreateKeyValue().SetText("Jewel Name", jewel);
                    }
                }
                description.view.Resize();
            });
        }
        elementView.Resize(elementView.Count);
    }

    public void OnMetricsTab()
    {
        Tabs[Tab.TabOption.Metrics].SetSelected(true);
        Reset();
        elementView.SetActive(true);
        description.SetName(Player.m_localPlayer.GetPlayerName());
        PlayerInfo.GetEntries().Build(description.view);
        description.view.Resize();
        foreach (PlayerInfo.MetricInfo metric in PlayerInfo.GetMetrics())
        {
            ElementView.Element item = elementView.Create();
            item.SetName(metric.Name);
            item.SetDescription(metric.Description);
            item.SetIcon(metric.Icon);
            item.Interactable(false);
        }
        
        elementView.Resize(elementView.Count);
    }

    public void OnLeaderboardTab()
    {
        Tabs[Tab.TabOption.Leaderboard].SetSelected(true);
        Reset(leaderboard);
        elementView.SetActive(true);
        foreach (Leaderboard.LeaderboardInfo? player in Leaderboard.GetLeaderboard())
        {
            ElementView.Element item = elementView.Create();
            item.SetName(player.PlayerName);
            item.SetDescription($"{Keys.Rank} {player.GetRank()}");
            item.SetIcon(SpriteManager.GetSprite(SpriteManager.IconOption.CrownGold));
            item.Interactable(true);
            item.OnClick(() => player.OnClick(this, item));
        }
        elementView.Resize(elementView.Count);
    }

    public void OnLotteryTab()
    {
        Tabs[Tab.TabOption.Lottery].SetSelected(true);
        Reset();
        lottery.SetActive(true);
        lottery.RandomizeIcons();
        lottery.Preview();
        description.SetName(Keys.Lottery);
        description.view.CreateTextArea().SetText(Keys.LotteryLore + "\n\n");
        description.view.CreateTitle().SetTitle(Keys.Chance);
        lottery.ToEntries().Build(description.view);
        description.view.Resize();
        int tokens = Player.m_localPlayer.GetTokens();
        bool canGamble = Player.m_localPlayer.NoCostCheat() || tokens >= Lottery.COST_TO_ROLL;
        description.Interactable(canGamble);
        description.SetButtonText(Keys.Gamble);
        OnMainButton = () =>
        {
            if (lottery.isRolling) return;
            if (!Player.m_localPlayer.NoCostCheat())
            {
                Player.m_localPlayer.RemoveTokens(Lottery.COST_TO_ROLL);
                LotteryManager.SendToServer(Lottery.COST_TO_ROLL);
                description.view.Clear();
                description.view.CreateTextArea().SetText(Keys.LotteryLore + "\n\n");
                description.view.CreateTitle().SetTitle(Keys.Chance);
                lottery.ToEntries().Build(description.view);
                description.view.Resize();
                tokens = Player.m_localPlayer.GetTokens();
                canGamble = tokens >= Lottery.COST_TO_ROLL;
                description.Interactable(canGamble);
            }
            lottery.Roll();
        };
    }
    
    public void SetTopic(string text) => topic.text = Localization.instance.Localize(text);
    public void SetPanelPosition(Vector3 pos) => transform.position = pos;
    public void OnDrag(PointerEventData eventData)
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        SetPanelPosition(Input.mousePosition + mouseDifference);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 pos = eventData.position;
        mouseDifference = transform.position - new Vector3(pos.x, pos.y, 0);
    }
    public void OnEndDrag(PointerEventData eventData) => panelPos.Value = transform.position;
    public static void OnSelectedColorChange(object sender, EventArgs e)
    {
        if (sender is not ConfigEntry<Color> config) return;
        instance?.elementView.SetSelectedColor(config.Value);
        instance?.buttonView.SetSelectedColor(config.Value);
    }

    private class ButtonElement
    {
        private readonly Button button;
        private readonly Text label;

        public ButtonElement(Transform transform, UnityAction action)
        {
            button = transform.GetComponent<Button>();
            label = transform.Find("Text").GetComponent<Text>();
            OnClick(action);
        }

        private void OnClick(UnityAction action) => button.onClick.AddListener(action);
        public void SetLabel(string text) => label.text = Localization.instance.Localize(text);
    }

    private class Search
    {
        private readonly InputField field;
        private readonly Text placeholder;

        public Search(Transform transform)
        {
            field = transform.GetComponent<InputField>();
            placeholder = transform.Find("Placeholder").GetComponent<Text>();
        }
        
        public void OnValueChanged(UnityAction<string> callback) => field.onValueChanged.AddListener(callback); 
        public void SetPlaceholder(string text) => placeholder.text = Localization.instance.Localize(text);
        public bool IsSearching() => field.isFocused;
        public void Reset() => field.SetTextWithoutNotify(null);
    }

    public class RightPanel
    {
        private readonly Image icon;
        private readonly Text name;
        public readonly InfoView view;
        public readonly RequirementView requirements;
        private readonly Button button;
        private readonly Text buttonText;
        
        private readonly Sprite defaultIcon;

        public RightPanel(Transform transform)
        {
            icon = transform.Find("Icon").GetComponent<Image>();
            defaultIcon = icon.sprite;
            name = transform.Find("Name").GetComponent<Text>();
            name.horizontalOverflow = HorizontalWrapMode.Wrap;
            view = new InfoView(transform.Find("ListView"));
            requirements = new RequirementView(transform.Find("Requirements"));
            button = transform.Find("MainButton").GetComponent<Button>();
            buttonText = transform.Find("MainButton/Text").GetComponent<Text>();
        }
        
        public void SetIcon(Sprite? sprite) => icon.sprite = sprite ?? defaultIcon;
        public void SetName(string text) => name.text = Localization.instance.Localize(text);
        public void OnClick(UnityAction action) => button.onClick.AddListener(action);

        public void SetButtonText(string text)
        {
            buttonText.text = Localization.instance.Localize(text);
            button.gameObject.SetActive(true);
        }
        public void Interactable(bool enable) => button.interactable = enable;

        public void Reset()
        {
            name.text = string.Empty;
            view.Clear();
            requirements.SetActive(false);
            button.gameObject.SetActive(false);
            requirements.Clear();
            icon.sprite = defaultIcon;
            buttonText.text = string.Empty;
        }
    }
    
    public class ButtonView : GridView
    {
        private readonly ElementButton _element;
        private readonly List<ElementButton> elements = new();
        public int Count => elements.Count;
        private ElementButton? selectedElement;
        
        public ButtonView(Transform transform) : base(transform)
        {
            _element = new ElementButton(transform.Find("Viewport/Button"));
            handleArrowKeys = true;
        }

        public ElementButton Create()
        {
            ElementButton button = _element.Create(root);
            elements.Add(button);
            return button;
        }

        protected override List<GridElement> GetElements() => elements.Select(GridElement (e) => e).ToList();
        protected override GridElement? GetSelectedElement() => selectedElement;
        public void SetSelectedColor(Color color)
        {
            _element.SetSelectedColor(color);
            foreach (ElementButton button in elements) button.SetSelectedColor(color);
        }
        
        public void SetSelected(ElementButton element)
        {
            foreach (ElementButton? item in elements)
            {
                if (item == element)
                {
                    item.SetSelected(true);
                    selectedElement = element;
                }
                else
                {
                    item.SetSelected(false);
                }
            }
        }
        
        public override void OnSearch(string query)
        {
            int count = 0;
            query = query.ToLower();
            foreach (ElementButton element in elements)
            {
                bool enable = string.IsNullOrEmpty(query) || element.Name.ToLower().Contains(query);
                element.SetActive(enable);
                if (enable) ++count;
            }

            Resize(count);
        }

        public void Clear()
        {
            foreach (ElementButton element in elements) element.Destroy();
            elements.Clear();
        }
        
        public class ElementButton : GridElement
        {
            private readonly Button button;
            private readonly Image selected;
            private readonly Text label;
            public string Name => label.text;
            public bool isKnown;
            public override bool IsKnown() => isKnown;
            public ElementButton(Transform transform) : base(transform)
            {
                button = transform.GetComponent<Button>();
                selected = transform.Find("Selected").GetComponent<Image>();
                label = transform.Find("Text").GetComponent<Text>();
                SetSelected(false);
            }
            public override void Select() => button.onClick.Invoke();
            public void SetLabel(string text) => label.text = Localization.instance.Localize(text);
            public void OnClick(UnityAction action) => button.onClick.AddListener(action);
            public void SetSelectedColor(Color color) => selected.color = color;
            public void SetSelected(bool enable) => selected.gameObject.SetActive(enable);
            public void Interactable(bool enable) => button.interactable = enable;
            public ElementButton Create(Transform parent)
            {
                GameObject? go = Instantiate(prefab, parent);
                go.SetActive(true);
                ElementButton element = new ElementButton(go.transform);
                return element;
            }
        }
    }

    public class ElementView : GridView
    {
        private readonly Element _element;
        private readonly List<Element> elements = new();
        public int Count => elements.Count;
        private Element? selectedElement;
        public ElementView(Transform transform) : base(transform)
        {
            _element = new Element(transform.Find("Viewport/ListElement"));
            handleArrowKeys = true;
        }
        public Element Create()
        {
            Element temp = _element.Create(root);
            elements.Add(temp);
            return temp;
        }

        protected override List<GridElement> GetElements() => elements.Select(GridElement (e) => e).ToList();
        protected override GridElement? GetSelectedElement() => selectedElement;
        public void SetSelected(Element element)
        {
            foreach (Element? item in elements)
            {
                if (item == element)
                {
                    item.SetSelected(true);
                    selectedElement = item;
                }
                else
                {
                    item.SetSelected(false);
                }
            }
        }
        public void SetSelectedColor(Color color)
        {
            _element.SetSelectedColor(color);
            foreach (Element? element in elements) element.SetSelectedColor(color);
        }
        public override void OnSearch(string query)
        {
            int count = 0;
            query = query.ToLower();
            foreach (Element element in elements)
            {
                bool enable = string.IsNullOrEmpty(query) || element.Name.ToLower().Contains(query) || Override(query, element);
                element.SetActive(enable);
                if (enable) ++count;
            }
            Resize(count);
        }
        private static bool Override(string query, Element element)
        {
            return element.itemInfo is { } item && OverrideMatch(query, item) || element.pieceInfo is { } piece && OverrideMatch(query, piece);
        }
        private static bool OverrideMatch(string query, PieceHelper.PieceInfo info)
        {
            return query switch
            {
                "feast" => info.isFeast,
                "ship" => info.isShip,
                "plant" => info.isPlant,
                _ => false,
            };
        }
        private static bool OverrideMatch(string query, ItemHelper.ItemInfo info)
        {
            return query switch
            {
                "swords" => info.IsSword(),
                "axes" => info.IsAxe(),
                "polearms" => info.IsPolearm(),
                "spears" => info.IsSpear(),
                "potions" => info.IsPotion(),
                "staves" => info.IsStaff(),
                "clubs" => info.IsClub(),
                "knives"  => info.IsKnives(),
                "shields" => info.IsShield(),
                "fish"  => info.IsFish(),
                "materials" => info.IsMaterial(),
                "food" => info.IsConsumable(),
                "weapons"=>info.IsWeapon(),
                "helmets" => info.IsHelmet(),
                "armor" => info.IsChest(),
                "legs" => info.IsLegs(),
                "trinkets" => info.IsTrinket(),
                _ => false,
            };
        }
        public void Clear()
        {
            foreach (Element element in elements) element.Destroy();
            elements.Clear();
        }
        public class Element : GridElement
        {
            private readonly Image selected;
            private readonly Button button;
            private readonly Image icon;
            private readonly Text name;
            private readonly Text description;
            private readonly Sprite defaultSprite;
            public ItemHelper.ItemInfo? itemInfo;
            public PieceHelper.PieceInfo? pieceInfo;
            public bool isKnown;

            public override bool IsKnown() => isKnown;
            public string Name => name.text;
            public Element(Transform transform) : base(transform)
            {
                selected = transform.Find("Selected").GetComponent<Image>();
                button = transform.Find("Background").GetComponent<Button>();
                icon = transform.Find("Background/Icon").GetComponent<Image>();
                defaultSprite = icon.sprite;
                name = transform.Find("Name").GetComponent<Text>();
                description = transform.Find("Description").GetComponent<Text>();
            }
            public Element Create(Transform parent)
            {
                GameObject? go = Instantiate(prefab, parent);
                go.SetActive(true);
                Element element = new Element(go.transform);
                return element;
            }
            public override void Select() => button.onClick.Invoke();
            public void OnClick(UnityAction action) => button.onClick.AddListener(action);
            public void SetIcon(Sprite? sprite) => icon.sprite = sprite ?? defaultSprite;
            public void SetName(string text) => name.text = Localization.instance.Localize(text);
            public void SetDescription(string text) => description.text = Localization.instance.Localize(text);
            public void SetIconColor(Color color) => icon.color = color;
            public void Interactable(bool enable) => button.interactable = enable;
            public void SetSelected(bool enable) => selected.gameObject.SetActive(enable);
            public void SetSelectedColor(Color color) => selected.color = color;
            public void SetItemInfo(ItemHelper.ItemInfo info)
            {
                itemInfo = info;
                pieceInfo = null;
            }
            public void SetPieceInfo(PieceHelper.PieceInfo info)
            {
                pieceInfo = info;
                itemInfo = null;
            }
        }
    }
    public class GridElement
    {
        protected readonly GameObject prefab;
        protected GridElement(Transform transform)
        {
            prefab = transform.gameObject;
        }
        public virtual void Select()
        {
        }
        public virtual bool IsKnown() => false;
        public bool IsHidden() => !prefab.activeInHierarchy;
        public void SetActive(bool enable) => prefab.SetActive(enable);
        public void Destroy() => Object.Destroy(prefab);
    }
    public class GridView
    {
        private readonly GameObject prefab;
        protected readonly RectTransform root;
        private readonly GridLayoutGroup grid;
        private readonly Scrollbar scrollbar;
        public static readonly List<GridView> views = new List<GridView>();
        public static GridView? activeView;
        private readonly float height;
        protected bool handleArrowKeys;
        private float lastInputTime;
        private float availableWidth => root.rect.width - grid.padding.left - grid.padding.right;
        private int columns => Mathf.Max(1, Mathf.FloorToInt((availableWidth + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x)));
        protected GridView(Transform transform)
        {
            prefab = transform.gameObject;
            root = transform.Find("Viewport/ListRoot").GetComponent<RectTransform>();
            scrollbar = transform.Find("Scrollbar").GetComponent<Scrollbar>();
            height = root.sizeDelta.y;
            grid = root.GetComponent<GridLayoutGroup>();
            views.Add(this);
        }

        public void SetGridElementSize(Vector2 size) => grid.cellSize = size;
        protected virtual List<GridElement> GetElements() => new();
        protected virtual GridElement? GetSelectedElement() => null;
        private void ScrollToElement(int elementIndex)
        {
            List<GridElement> elements = GetElements();
            if (elementIndex < 0 || elementIndex >= elements.Count) return;
            int elementRow = elementIndex / columns;
            float elementY = grid.padding.top + elementRow * (grid.cellSize.y + grid.spacing.y);
            float viewportHeight = height;
            float contentHeight = root.sizeDelta.y;
            if (contentHeight <= viewportHeight)
            {
                scrollbar.value = 1f;
                return;
            }
            float elementCenterY = elementY + grid.cellSize.y / 2f;
            float targetScrollY = elementCenterY - viewportHeight / 2f;
            float maxScrollY = contentHeight - viewportHeight;
            targetScrollY = Mathf.Clamp(targetScrollY, 0f, maxScrollY);
            float scrollbarValue = 1f - (targetScrollY / maxScrollY);
            scrollbar.value = Mathf.Clamp01(scrollbarValue);
        }
        
        public void HandleArrowKeys()
        {
            if (activeView == null || !IsVisible() || !handleArrowKeys) return;
            if (Time.time - lastInputTime < Input_Cooldown) return;
            if (!ZInput.GetKeyDown(KeyCode.LeftArrow) && !ZInput.GetKeyDown(KeyCode.RightArrow) && !ZInput.GetKeyDown(KeyCode.UpArrow) && !ZInput.GetKeyDown(KeyCode.DownArrow)) return;
            lastInputTime = Time.time;
            if (activeView.GetSelectedElement() is not { } selectedElement) return;
            List<GridElement> list = activeView.GetElements().Where(element => !element.IsHidden()).ToList();
            int indexOf = list.IndexOf(selectedElement);
            if (indexOf == -1) return;
            int nextElement = indexOf;
            if (ZInput.GetKeyDown(KeyCode.LeftArrow))
            {
                nextElement = FindNextValidElement(list, indexOf, -1, 0, list.Count - 1);
            }
            else if (ZInput.GetKeyDown(KeyCode.RightArrow))
            {
                nextElement = FindNextValidElement(list, indexOf, 1, 0, list.Count - 1);
            }
            else if (ZInput.GetKeyDown(KeyCode.UpArrow))
            {
                nextElement = FindNextValidElementVertical(list, indexOf, -columns, 0, list.Count - 1);
            }
            else if (ZInput.GetKeyDown(KeyCode.DownArrow))
            {
                nextElement = FindNextValidElementVertical(list, indexOf, columns, 0, list.Count - 1);
            }

            if (nextElement == indexOf) return;
            GridElement element = list[nextElement];
            element.Select();
            ScrollToElement(nextElement);
        }

        private static int FindNextValidElement(List<GridElement> list, int currentIndex, int direction, int minIndex, int maxIndex)
        {
            int nextIndex = currentIndex;
            do
            {
                nextIndex += direction;
                if (nextIndex < minIndex || nextIndex > maxIndex)
                {
                    return currentIndex;
                }
            }
            while (!list[nextIndex].IsKnown());
            
            return nextIndex;
        }
        private static int FindNextValidElementVertical(List<GridElement> list, int currentIndex, int columnStep, int minIndex, int maxIndex)
        {
            int nextIndex = currentIndex + columnStep;
            
            while (nextIndex >= minIndex && nextIndex <= maxIndex)
            {
                if (list[nextIndex].IsKnown())
                {
                    return nextIndex;
                }
                nextIndex += columnStep;
            }
            return currentIndex;
        }
        public void SetActive(bool enable)
        {
            activeView = null;
            foreach (GridView? view in views)
            {
                view.prefab.SetActive(view == this && enable);
            }
            activeView = this;
        }
        public void Resize(int count)
        {
            int rows = Mathf.CeilToInt((float)count / columns);
            float totalHeight = grid.padding.top + grid.padding.bottom + rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y;
            root.sizeDelta = new Vector2(root.sizeDelta.x, Mathf.Max(totalHeight, height));
            scrollbar.value = 1f;
        }
        public virtual void OnSearch(string query) { }
    }
    public class InfoView
    {
        private readonly Title _title;
        private readonly TextArea _textArea;
        private readonly KeyValue _keyValue;
        private readonly Icons _icons;
        
        private readonly GameObject prefab;
        private readonly RectTransform root;
        private readonly VerticalLayoutGroup group;
        private readonly Scrollbar scrollbar;
        private readonly List<InfoElement> elements = new();
        private readonly float minHeight;
        private readonly float padding;
        private const float buffer = 2f; 
        public InfoView(Transform transform)
        {
            transform.GetComponent<Image>().enabled = false;
            prefab = transform.gameObject;
            root = transform.Find("Viewport/ListRoot").GetComponent<RectTransform>();
            group = root.GetComponent<VerticalLayoutGroup>();
            scrollbar = transform.Find("Scrollbar").GetComponent<Scrollbar>();
            padding = group.padding.top + group.padding.bottom;
            minHeight = root.sizeDelta.y;
            _title = new Title(transform.Find("Viewport/Title"));
            _textArea = new TextArea(transform.Find("Viewport/TextArea"));
            _keyValue = new KeyValue(transform.Find("Viewport/KVP"));
            _icons = new Icons(transform.Find("Viewport/Icons"));
        }
        public void Resize()
        {
            float height = elements.Sum(element => element.height);
            float spacing = Mathf.Min(0, elements.Count - 1) * group.spacing;
            float extra = buffer * elements.Count;
            root.sizeDelta = new Vector2(root.sizeDelta.x, Mathf.Max(minHeight, height + spacing + padding + extra));
            scrollbar.value = 1f;
        }
        public void OnSearch(string query)
        {
            foreach (InfoElement? element in elements)
            {
                element.SetActive(element.Contains(query));
            }
        }
        public void Clear()
        {
            foreach(InfoElement? element in elements) element.Destroy();
            elements.Clear();
        }
        public Title CreateTitle()
        {
            Title element = _title.Create(root);
            elements.Add(element);
            return element;
        }
        public TextArea CreateTextArea()
        {
            TextArea element = _textArea.Create(root);
            elements.Add(element);
            return element;
        }
        public KeyValue CreateKeyValue()
        {
            KeyValue element = _keyValue.Create(root);
            elements.Add(element);
            return element;
        }
        public Icons CreateIcons()
        {
            Icons element = _icons.Create(root);
            elements.Add(element);
            return element;
        }
        public void SetActive(bool enable) => prefab.SetActive(enable);
        public class Icons : InfoElement
        {
            private readonly List<IconElement> elements = new();
            public Icons(Transform transform) : base(transform)
            {
                elements.Add(new IconElement(transform.Find("1")));
                elements.Add(new IconElement(transform.Find("2")));
                elements.Add(new IconElement(transform.Find("3")));
                elements.Add(new IconElement(transform.Find("4")));
            }
            private class IconElement
            {
                private readonly Image bkg;
                private readonly Image icon;
                private readonly Text name;
                private readonly Text amount;
                private readonly Button button;
                public IconElement(Transform transform)
                {
                    bkg = transform.GetComponent<Image>();
                    icon = transform.Find("Icon").GetComponent<Image>();
                    name = transform.Find("Name").GetComponent<Text>();
                    amount = transform.Find("Amount").GetComponent<Text>();
                    button = transform.GetComponent<Button>();
                    Interactable(false);
                }
                public void SetIcon(Sprite? sprite) => icon.sprite = sprite;
                public void SetName(string text) => name.text = Localization.instance.Localize(text);
                public void SetAmount(string value) => amount.text = Localization.instance.Localize(value);
                public void SetAmount(int value) => amount.text = value.ToString();
                public void SetIconColor(Color color) => icon.color = color;
                public void OnClick(UnityAction action) => button.onClick.AddListener(action);
                public void Interactable(bool enable) => button.interactable = enable;
                public void Hide()
                {
                    bkg.color = Color.clear;
                    icon.color = Color.clear;
                    name.text = string.Empty;
                    amount.text = string.Empty;
                }
                public bool Contains(string query) => name.text.ToLower().Contains(query.ToLower());
            }
            public void SetTokens(int count)
            {
                int max = 999;
                foreach (IconElement? element in elements)
                {
                    if (count <= 0)
                    {
                        element.Hide();
                    }
                    else if (count < max)
                    {
                        element.SetIcon(SpriteManager.GetSprite(SpriteManager.IconOption.SilverCoins));
                        element.SetName(Keys.AlmanacToken);
                        element.SetAmount($"<color=yellow>{count}</color>");
                        count = 0;
                    }
                    else
                    {
                        element.SetIcon(SpriteManager.GetSprite(SpriteManager.IconOption.SilverCoins));
                        element.SetName(Keys.AlmanacToken);
                        element.SetAmount($"<color=yellow>{max}</color>");
                        count -= max;
                    }
                }
            }
            public void SetIcons(params Sprite[] sprites)
            {
                for (int index = 0; index < elements.Count; ++index)
                {
                    IconElement element = elements[index];
                    if (sprites.Length > index)
                    {
                        element.SetIcon(sprites[index]);
                        element.SetName(string.Empty);
                        element.SetAmount(string.Empty);
                    }
                    else element.Hide();
                }
            }
            public void SetIcons(params CritterHelper.CritterInfo[] characters)
            {
                if (instance == null) return;
                for (int index = 0; index < elements.Count; ++index)
                {
                    IconElement element = elements[index];
                    if (characters.Length > index)
                    {
                        CritterHelper.CritterInfo info = characters[index];
                        bool isKnown = Player.m_localPlayer.NoCostCheat() || !Configs.UseKnowledgeWall || info.isKnown();
                        element.SetIcon(info.trophy?.GetIcon());
                        element.SetName(info.character.m_name);
                        element.SetAmount(string.Empty);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        element.Interactable(isKnown);
                        element.OnClick(() => info.OnClick(instance, null));
                    }
                    else element.Hide();
                }
            }
            public void SetIcons(params PieceHelper.PieceInfo[] pieces)
            {
                if (instance == null) return;
                for (int index = 0; index < elements.Count; index++)
                {
                    IconElement element = elements[index];
                    if (pieces.Length > index)
                    {
                        PieceHelper.PieceInfo info = pieces[index];
                        bool isKnown = Player.m_localPlayer.NoCostCheat() || Player.m_localPlayer.IsPieceKnown(info.piece);
                        element.SetIcon(info.piece.m_icon);
                        element.SetName(isKnown ? info.piece.m_name : "???");
                        element.SetAmount(string.Empty);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        element.Interactable(isKnown);
                        element.OnClick(() => info.OnClick(instance, null));
                    }
                    else
                    {
                        element.Hide();
                    }
                }
            }
            public void SetIcons(params Recipe[] recipes)
            {
                if (instance == null) return;
                for (int index = 0; index < elements.Count; index++)
                {
                    IconElement element = elements[index];
                    if (recipes.Length > index)
                    {
                        Recipe recipe = recipes[index];
                        ItemDrop item = recipe.m_item;
                        if (item.GetInfo() is not { } info)
                        {
                            element.Hide();
                            continue;
                        }
                        bool isKnown = Player.m_localPlayer.NoCostCheat() ||
                                       Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name);
                        element.SetIcon(info.GetIcon());
                        element.SetName(isKnown ? info.itemData.m_shared.m_name ?? string.Empty : "???");
                        if (recipe.m_amount > 1) element.SetAmount(recipe.m_amount);
                        else element.SetAmount(string.Empty);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        element.Interactable(isKnown);
                        element.OnClick(() => info.OnClick(instance, null));
                    }
                    else
                    {
                        element.Hide();
                    }
                }
            }
            public void SetIcons(params DropInfo[] infos)
            {
                if (instance == null) return;
                for (int index = 0; index < elements.Count; index++)
                {
                    IconElement element = elements[index];
                    if (infos.Length > index)
                    {
                        DropInfo info = infos[index];
                        if (info.item.GetInfo() is not { } itemInfo)
                        {
                            element.Hide();
                            continue;
                        }
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(info.item.m_shared.m_name) || Player.m_localPlayer.NoCostCheat();
                        element.SetIcon(info.item.GetIcon());
                        element.SetName(isKnown ? info.item.m_shared.m_name : string.Empty);
                        element.SetAmount(isKnown ? $"{info.min}-{info.max} ({info.chance * 100}%)" : string.Empty);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        element.Interactable(isKnown);
                        element.OnClick(() => itemInfo.OnClick(instance, null));
                    }
                    else
                    {
                        element.Hide();
                    }
                }
            }

            public void SetIcons(params ItemHelper.ItemInfo[] infos)
            {
                if (instance == null) return;
                for (int index = 0; index < elements.Count; index++)
                {
                    IconElement element = elements[index];
                    if (infos.Length > index)
                    {
                        ItemHelper.ItemInfo info = infos[index];
                        bool isKnown = Player.m_localPlayer.NoCostCheat() ||
                                       Player.m_localPlayer.IsKnownMaterial(info.itemData.m_shared.m_name);
                        element.SetIcon(info.GetIcon());
                        element.SetName(isKnown ? info.itemData.m_shared.m_name ?? string.Empty : "???");
                        element.SetAmount(string.Empty);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        element.Interactable(isKnown);
                        element.OnClick(() => info.OnClick(instance, null));
                    }
                    else element.Hide();
                }
            }
            public void SetIcons(params ItemDrop[] itemDrops)
            {
                if (instance == null) return;
                for (int index = 0; index < elements.Count; index++)
                {
                    IconElement element = elements[index];
                    if (itemDrops.Length > index)
                    {
                        ItemDrop item = itemDrops[index];
                        if (item.GetInfo() is not { } info)
                        {
                            element.Hide();
                            continue;
                        }
                        bool isKnown = Player.m_localPlayer.NoCostCheat() ||
                                       Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name);
                        element.SetIcon(info.GetIcon());
                        element.SetName(isKnown ? info.itemData.m_shared.m_name ?? string.Empty : "???");
                        element.SetAmount(string.Empty);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        element.Interactable(isKnown);
                        element.OnClick(() => info.OnClick(instance, null));
                    }
                    else
                    {
                        element.Hide();
                    }
                }
            }
            public void SetIcons(params StoreManager.StoreItem.ItemInfo[] infos)
            {
                for (int index = 0; index < elements.Count; index++)
                {
                    IconElement element = elements[index];
                    if (infos.Length > index)
                    {
                        StoreManager.StoreItem.ItemInfo info = infos[index];
                        if (info.item == null) continue;
                        element.SetIcon(info.item.m_itemData.GetIcon()!);
                        string name = info.item.m_itemData.m_shared.m_name;
                        bool isKnown = Player.m_localPlayer.NoCostCheat() || Player.m_localPlayer.IsKnownMaterial(name);
                        element.SetIconColor(isKnown ? Color.white : Color.black);
                        if (info.Quality > 1) name += $" ({info.Quality}";
                        element.SetName(isKnown ? name : "???");
                        element.SetAmount($"{info.Amount}");
                    }
                    else
                    {
                        element.Hide();
                    }
                }
            }
            public Icons Create(Transform parent)
            {
                GameObject? go = Instantiate(prefab, parent);
                go.SetActive(true);
                Icons icons = new Icons(go.transform);
                return icons;
            }
            public struct DropInfo
            {
                public readonly ItemDrop.ItemData item;
                public readonly float chance;
                public readonly int min;
                public readonly int max;
                public DropInfo(ItemDrop.ItemData item, float chance, int min, int max)
                {
                    this.item = item;
                    this.chance = chance;
                    this.min = min;
                    this.max = max;
                }
            }
            public override bool Contains(string query) => elements.Any(element => element.Contains(query));
        }

        public class KeyValue : InfoElement
        {
            private readonly Text key;
            private readonly Text value;
            public KeyValue(Transform transform) : base(transform)
            {
                key = transform.Find("Key").GetComponent<Text>();
                value = transform.Find("Value").GetComponent<Text>();
                SetValueColor(Helpers._OrangeColor);
            }
            public KeyValue Create(Transform parent)
            {
                GameObject go = Instantiate(prefab, parent);
                go.SetActive(true);
                KeyValue kvp = new KeyValue(go.transform);
                return kvp;
            }
            public void SetText(string label, string text)
            {
                key.text = Localization.instance.Localize(label);
                value.text = Localization.instance.Localize(text);
            }
            public void SetKeyColor(Color color) => key.color = color;
            public void SetValueColor(Color color) => value.color = color;
            public override bool Contains(string query) => key.text.ToLower().Contains(query.ToLower()) || value.text.ToLower().Contains(query.ToLower());

        }
        public class TextArea : InfoElement
        {
            private readonly Text area;
            private readonly RectTransform rect;
            public TextArea(Transform transform) : base(transform)
            {
                area = transform.GetComponent<Text>();
                rect = transform.GetComponent<RectTransform>();
            }
            public TextArea Create(Transform parent)
            {
                GameObject? go = Instantiate(prefab, parent);
                go.SetActive(true);
                TextArea element = new TextArea(go.transform);
                return element;
            }
            public void SetText(string text)
            {
                area.text = Localization.instance.Localize(text);
                Resize();
            }
            private void Resize()
            {
                float newHeight = GetTextPreferredHeight(area, rect);
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Max(newHeight,height));
                height = newHeight;
            }
            private static float GetTextPreferredHeight(Text text, RectTransform rect)
            {
                if (string.IsNullOrEmpty(text.text)) return 0f;
                TextGenerator textGen = text.cachedTextGenerator;
                TextGenerationSettings settings = text.GetGenerationSettings(rect.rect.size);
                float preferredHeight = textGen.GetPreferredHeight(text.text, settings);
                return preferredHeight;
            }
            public override bool Contains(string query) => area.text.ToLower().Contains(query.ToLower());
        }
        public class Title : InfoElement
        {
            private readonly Text title;
            public Title(Transform transform) : base(transform)
            {
                title = transform.Find("Text").GetComponent<Text>();
            }
            public void SetTitle(string text) =>  title.text = Localization.instance.Localize(text);
            public Title Create(Transform parent)
            {
                GameObject? go = Instantiate(prefab, parent);
                go.SetActive(true);
                Title element = new Title(go.transform);
                return element;
            }
            public override bool Contains(string query) => title.text.ToLower().Contains(query.ToLower());
        }

        public class InfoElement
        {
            protected readonly GameObject prefab;
            public float height;
            protected InfoElement(Transform transform)
            {
                prefab = transform.gameObject;
                height = transform.GetComponent<RectTransform>().sizeDelta.y;
            }
            public virtual bool Contains(string query) => false;
            public void SetActive(bool active) => prefab.SetActive(active);
            public void Destroy() => Object.Destroy(prefab);
        }
    }
    public class RequirementView
    {
        private readonly GameObject prefab;
        private readonly List<RequirementItem> items = new();
        private readonly Image starBkg;
        private readonly Image starIcon;
        private readonly Text starText;
        public RequirementView(Transform transform)
        {
            prefab = transform.gameObject;
            items.Add(new  RequirementItem(transform.Find("List/1")));
            items.Add(new  RequirementItem(transform.Find("List/2")));
            items.Add(new  RequirementItem(transform.Find("List/3")));
            items.Add(new  RequirementItem(transform.Find("List/4")));
            starBkg = transform.Find("Star").GetComponent<Image>();
            starIcon = transform.Find("Star/Icon").GetComponent<Image>();
            starText = transform.Find("Star/Icon/Text").GetComponent<Text>();
        }
        public void SetTokens(int count)
        {
            int max = 999;
            foreach (RequirementItem? element in items)
            {
                if (count <= 0)
                {
                    element.Hide();
                }
                else if (count < max)
                {
                    element.Set(SpriteManager.GetSprite(SpriteManager.IconOption.SilverCoins), Keys.AlmanacToken, count);
                    count = 0;
                }
                else
                {
                    element.Set(SpriteManager.GetSprite(SpriteManager.IconOption.SilverCoins), Keys.AlmanacToken, max);
                    count -= max;
                }
            }
            SetActive(true);
        }
        public void Set(StoreManager.StoreCost costs)
        {
            for (int index = 0; index < items.Count; ++index)
            {
                RequirementItem? item = items[index];
                if (costs.Items.Count > index)
                {
                    StoreManager.StoreCost.Cost? cost = costs.Items[index];
                    if (cost.isToken) item.Set(cost.tokenIcon, Keys.AlmanacToken, cost.Amount);
                    else if (cost.item is not {} itemDrop) item.Hide();
                    else item.Set(itemDrop.m_itemData.GetIcon(), itemDrop.m_itemData.m_shared.m_name, cost.Amount);
                }
                else
                {
                    item.Hide();
                }
            }
            SetActive(true);
        }
        public void Set(Recipe recipe) => Set(recipe.m_resources);
        public void Set(Piece.Requirement[] requirements)
        {
            for (int index = 0; index < items.Count; ++index)
            {
                RequirementItem? item = items[index];
                if (requirements.Length > index)
                {
                    Piece.Requirement data = requirements[index];
                    item.Set(data.m_resItem.m_itemData.GetIcon(), data.m_resItem.m_itemData.m_shared.m_name, data.m_amount);
                }
                else
                {
                    item.Hide();
                }
            }
            SetActive(true);
        }
        public void Set(int cost, ItemDrop currency)
        {
            if (!currency.m_itemData.HasIcons()) return;
            int maxStack = currency.m_itemData.m_shared.m_maxStackSize;
            
            foreach (RequirementItem? item in items)
            {
                if (cost <= 0)
                {
                    item.Hide();
                    continue;
                }
                int amount = Mathf.Min(cost, maxStack);
                cost -= amount;
                item.Set(currency.m_itemData.GetIcon(), currency.m_itemData.m_shared.m_name, amount);
            }
            SetActive(true);
        }
        public void SetLevel(int level) => SetLevel(level.ToString());
        public void SetLevel(string level) => starText.text = Localization.instance.Localize(level);
        public void SetLevelIcon(Sprite sprite) => starIcon.sprite = sprite;
        public void Clear()
        {
            foreach (RequirementItem? item in items) item.Hide();
        }
        public void SetActive(bool enable) => prefab.SetActive(enable);
        public void Update()
        {
            foreach(RequirementItem? item in items) item.Update(Player.m_localPlayer);
        }
        private class RequirementItem
        {
            private readonly Image icon;
            private readonly Text name;
            private readonly Text amount;

            private readonly Sprite defaultIcon;
            private string? SharedName;
            private int Count;
            public RequirementItem(Transform transform)
            {
                icon = transform.Find("Icon").GetComponent<Image>();
                defaultIcon = icon.sprite;
                name = transform.Find("Name").GetComponent<Text>();
                amount = transform.Find("Amount").GetComponent<Text>();
            }
            public void Set(Sprite? sprite, string sharedName, int value)
            {
                SharedName = sharedName;
                Count = value;
                icon.sprite = sprite ?? defaultIcon;
                icon.color = Color.white;
                name.text = Localization.instance.Localize(sharedName);
                amount.text = value.ToString();
            }
            public void Hide()
            {
                SharedName = null;
                icon.color = Color.clear;
                name.text = string.Empty;
                amount.text = string.Empty;
            }
            public void Update(Player player)
            {
                if (SharedName == null) return;
                Inventory inventory = player.GetInventory();
                int count = SharedName == Keys.AlmanacToken ? player.GetTokens() : inventory.CountItems(SharedName);
                bool hasRequirement = Count <= count;
                if (!hasRequirement)
                {
                    amount.color = Mathf.Sin(Time.time * 10f) > 0.0 ? Color.red : Color.white;
                }
                else
                {
                    amount.color = Color.white;
                }            
            }
        }
    }
    public class Background
    {
        private readonly Image background;
        private readonly Image darken;
        public Background(Transform transform)
        {
            background = transform.Find("bkg").GetComponent<Image>();
            darken = transform.Find("darken").GetComponent<Image>();
        }
        public void SetBackground(BackgroundOption option)
        {
            switch (option)
            {
                case BackgroundOption.Opaque:
                    background.gameObject.SetActive(true);
                    darken.gameObject.SetActive(false);
                    break;
                case BackgroundOption.Transparent:
                    background.gameObject.SetActive(false);
                    darken.gameObject.SetActive(true);
                    break;
            }
        }
        public enum BackgroundOption {Opaque, Transparent}
    }
    public class Tab
    {
        private readonly GameObject prefab;
        private readonly Button button;
        private readonly Text label;
        private readonly GameObject selected;
        private readonly Text selectedLabel;
        public bool IsSelected => selected.activeSelf;
        public Tab(Transform transform, UnityAction action)
        {
            prefab = transform.gameObject;
            button = transform.GetComponent<Button>();
            label = transform.Find("Text").GetComponent<Text>();
            selected = transform.Find("Selected").gameObject;
            selectedLabel = transform.Find("Selected/Text").GetComponent<Text>();
            OnClick(action);
        }
        public void SetActive(bool active) => prefab.SetActive(active);
        private void OnClick(UnityAction action) => button.onClick.AddListener(action);
        public void SetLabel(string text)
        {
            label.text = Localization.instance.Localize(text);
            selectedLabel.text = Localization.instance.Localize(text);
        }
        public void SetSelected(bool enable)
        {
            if (instance == null) return;
            foreach (Tab? tab in instance.Tabs.Values)
            {
                tab.selected.SetActive(tab == this && enable);
            }
        }
        public enum TabOption {Trophies, Pieces, Items, Creatures, StatusEffects, Achievements, Bounties, Treasures, Store, Metrics, Leaderboard, Lottery}
    }
    public class Currency
    {
        private readonly GameObject prefab;
        private readonly Image icon;
        private readonly Text field;
        private readonly Sprite defaultIcon;
        public Currency(Transform transform)
        {
            prefab = transform.gameObject;
            icon = transform.Find("Icon").GetComponent<Image>();
            field = transform.Find("Text").GetComponent<Text>();
            defaultIcon = icon.sprite;
        }
        public void SetIcon(Sprite? sprite) => icon.sprite = sprite ?? defaultIcon;
        private void SetText(string text) => field.text = Localization.instance.Localize(text);
        public void SetAmount(int amount) => SetText($"{Keys.AlmanacToken} <color=yellow>x{amount}</color>");
        public void SetActive(bool enable) => prefab.SetActive(enable);
    }
    private class Lottery : GridView
    {
        private readonly Dictionary<Scrollbar, float> _scrollbars = new();
        private readonly List<Scrollbar> scrollbars;
        private readonly List<Slot> slots = new();
        private readonly List<Slot.SlotElement> finalElements = new();
        private const float fullHouseChance = 0.99f;
        private const int RequiredMatchCount = 3;
        private const float UPDATE_INTERVAL = 0.033f;
        private const int TokensPerSuccess = 10;
        public static int COST_TO_ROLL => Configs.LotteryCost;
        private float glowTimer;
        public bool isRolling;
        private bool isFullHouse;
        private int SlotCount => slots.Count;
        private static int IconCount => Slot.icons.Length;
        private double _winChance;
        private double winChance
        {
            get
            {
                if (_winChance == 0)
                {
                    _winChance = CalculateWinChance(SlotCount, IconCount, RequiredMatchCount);
                }
                return _winChance;
            }
        }
        private double _expectedWins;
        private double expectedWins
        {
            get
            {
                if (_expectedWins == 0)
                {
                    _expectedWins = CalculateExpectedWins(SlotCount, IconCount, RequiredMatchCount);
                }
                return _expectedWins;
            }
        }
        private int _possibleSequences;
        private int possibleSequences
        {
            get
            {
                if (_possibleSequences == 0)
                {
                    _possibleSequences = SlotCount - RequiredMatchCount + 1;
                }
                return _possibleSequences;
            }
        }
        private double _sequenceProb;
        private double sequenceProb
        {
            get
            {
                if (_sequenceProb == 0)
                {
                    _sequenceProb = Math.Pow(1.0 / IconCount, RequiredMatchCount - 1);
                }

                return _sequenceProb;
            }
        }
        
        public Lottery(Transform transform) : base(transform)
        {
            Slot template = new Slot(transform.Find("Viewport/Slot"));
            for(int index = 0; index < 48; ++index)
            {
                Slot slot = template.Create(root);
                slots.Add(slot);
                _scrollbars[slot.scrollbar] = 0f;
                finalElements.Add(slot.GetFinalElement());
            }
            scrollbars = new List<Scrollbar>(_scrollbars.Keys);
        }
        public void RandomizeIcons()
        {
            isFullHouse = UnityEngine.Random.value > fullHouseChance;
            SpriteManager.IconOption? fullHouseIcon = isFullHouse ? 
                (SpriteManager.IconOption)Slot.icons.GetValue(UnityEngine.Random.Range(0, Slot.icons.Length)) 
                : null;

            foreach (Slot slot in slots)
            {
                slot.RandomizeIcons(fullHouseIcon);
            }
        }
        private void UpdateRoll(float dt)
        {
            if (instance is null) return;
            
            isRolling = true;
            
            bool allFinished = true;
            
            SpriteManager.IconOption? previous = null;
            int match = 0;
            int success = 0;
            HashSet<Slot> successMatches = new();
            List<Slot> matches = new();
            float t = Time.time;
            for (int index = 0; index < slots.Count; ++index)
            {
                Slot slot = slots[index];
                Scrollbar scrollbar = scrollbars[index];
                
                float startDelay = index * 0.3f;
                bool scrollbarFinished = false;
                
                if (t >= startDelay)
                {
                    float elapsedTime = _scrollbars[scrollbar] + dt;
                    float time = Mathf.Clamp01(elapsedTime / 2f);
                    float ease = Mathf.SmoothStep(0f, 1f, time);
                    _scrollbars[scrollbar] = elapsedTime;
                    scrollbar.value = ease;
                    
                    scrollbarFinished = elapsedTime >= 2f;
                }
                
                if (!scrollbarFinished)
                {
                    allFinished = false;
                }

                if (!scrollbarFinished) continue;
                if (isFullHouse)
                {
                    slot.GetFinalElement().SetGlow(true);
                }
                else
                {
                    if (previous == null)
                    {
                        match = 1;
                        matches.Clear();
                        matches.Add(slot);
                        slot.GetFinalElement().SetGlow(true);
                    }
                    else
                    {
                        if (previous == slot.option)
                        {
                            match++;
                            matches.Add(slot);
                            slot.GetFinalElement().SetGlow(true);
                        }
                        else
                        {
                            match = 1;
                            foreach (Slot? previousMatches in matches)
                            {
                                if (successMatches.Contains(previousMatches)) continue;
                                previousMatches.GetFinalElement().SetGlow(false);
                            }
                            matches.Clear();
                            matches.Add(slot);
                            slot.GetFinalElement().SetGlow(true);
                        }
                        
                        if (match == RequiredMatchCount)
                        {
                            success++;
                            foreach(Slot successMatch in matches) successMatches.Add(successMatch);
                            List<Slot> lastTwo = matches.Skip(matches.Count - 2).ToList();
                            matches.Clear();
                            matches.AddRange(lastTwo);
                            match = 2;
                        }
                    }
                    previous = slot.option;
                }
            }
            if (!allFinished) return;
            if (isFullHouse)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.FullHouse);
                instance.description.SetButtonText(Keys.CollectReward);
                instance.OnMainButton = () =>
                {
                    Player.m_localPlayer.AddTokens(LotteryManager.LotteryTotal);
                    LotteryManager.SendToServer(0);
                    instance.description.SetButtonText(Keys.Gamble);
                    instance.OnMainButton = () => instance.lottery.Roll();
                };
            }
            else if (success > 0)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.Success);
                instance.description.SetButtonText(Keys.CollectReward + $" ({success * TokensPerSuccess})");
                instance.OnMainButton = () =>
                {
                    Player.m_localPlayer.AddTokens(success * TokensPerSuccess);
                    instance.description.SetButtonText(Keys.Gamble);
                    instance.OnMainButton = () => instance.lottery.Roll();
                };
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.TryAgainNextTime);
                foreach(Slot? previousMatch in matches) previousMatch.GetFinalElement().SetGlow(false);
            }
            isRolling = false;
            instance.OnUpdate = null;
        }
        public void UpdateGlow(float dt)
        {
            if (!instance?.Tabs[Tab.TabOption.Lottery].IsSelected ?? false) return;
            if (isRolling) return;
            glowTimer += dt;
            if (glowTimer < UPDATE_INTERVAL) return;
            glowTimer = 0.0f;
            float time = Time.time;
            for (int i = 0; i < finalElements.Count; i++)
            {
                Slot.SlotElement? element = finalElements[i];
                if (!element.isGlowing) continue;
                float staggerDelay = i * 0.2f;
                element.UpdateGlow(time, staggerDelay);
            }
        }
        public void Preview()
        {
            for (int index = 0; index < scrollbars.Count; index++)
            {
                Slot slot = slots[index];
                Scrollbar scrollbar = scrollbars[index];
                scrollbar.value = 1f;
                slot.GetFinalElement().SetGlow(true);
            }
        }
        public void Roll()
        {
            RandomizeIcons();
            if (instance == null) return;
            float accumulatedDelay = 0f;
            for (int index = 0; index < scrollbars.Count; index++)
            {
                Scrollbar scrollbar = scrollbars[index];
                Slot slot = slots[index];
                slot.GetFinalElement().ResetGlowAlpha();
                float randomDelay = UnityEngine.Random.Range(0.1f, 0.3f);
                _scrollbars[scrollbar] = -accumulatedDelay;
                accumulatedDelay += randomDelay;
                scrollbar.value = 0f;
            }
            instance.OnUpdate = UpdateRoll;
        }

        private static double CalculateWinChance(int slotCount, int iconCount, int matchLength)
        {
            if (slotCount < matchLength) return 0.0;
            double sequenceProb = Math.Pow(1.0 / iconCount, matchLength - 1);
            int possibleSequences = slotCount - matchLength + 1;
            double noWinProb = 1.0 - sequenceProb;
            double allFailProb = Math.Pow(noWinProb, possibleSequences);
            double winProb = 1.0 - allFailProb;
            return winProb * 100.0; 
        }
        private static double CalculateExpectedWins(int slotCount, int iconCount, int matchLength)
        {
            if (slotCount < matchLength) return 0.0;
            double sequenceProb = Math.Pow(1.0 / iconCount, matchLength - 1);
            int possibleSequences = slotCount - matchLength + 1;
            return possibleSequences * sequenceProb;
        }
        public List<Entries.Entry> ToEntries()
        {
            Entries.EntryBuilder builder = new();
            builder.Add(Keys.Slots, SlotCount);
            builder.Add(Keys.Icons, IconCount);
            builder.Add(Keys.MatchLength, RequiredMatchCount);
            builder.Add(Keys.PossibleSequences, possibleSequences);
            builder.Add(Keys.ChancePerSequence, $"{sequenceProb:P4} ({1.0/sequenceProb:F1} to 1)");
            builder.Add(Keys.ExpectedWins, $"{expectedWins:F3}");
            builder.Add(Keys.WinChance, $"{winChance:F2}%");
            builder.Add(Keys.FailureChance, $"{100 - winChance:F2}%");
            builder.Add(Keys.ChanceForFullHouse, $"{(1f - fullHouseChance) * 100}%");
            builder.Add(Keys.FullHouseReward, LotteryManager.LotteryTotal);
            builder.Add(Keys.Reward, $"{TokensPerSuccess} {Keys.PerMatches}");
            builder.Add(Keys.Cost);
            builder.Add(Keys.AlmanacToken, COST_TO_ROLL);
            return builder.ToList();
        }
        public class Slot
        {
            public static readonly SpriteManager.IconOption[] icons = 
                Enum.GetValues(typeof(SpriteManager.IconOption))
                    .Cast<SpriteManager.IconOption>()
                    .Where(x => x != SpriteManager.IconOption.Almanac)
                    .ToArray();            
            private readonly SlotElement template;
            private readonly RectTransform root;
            private readonly GameObject prefab;
            public readonly Scrollbar scrollbar;
            private readonly List<SlotElement> elements = new();
            public SpriteManager.IconOption? option;
            public Slot(Transform transform)
            {
                prefab = transform.gameObject;
                root = transform.Find("Port/Viewport/Listroot").GetComponent<RectTransform>();
                scrollbar = transform.Find("Port/Scrollbar").GetComponent<Scrollbar>();
                template = new SlotElement(transform.Find("Port/Viewport/Element"));
                scrollbar.interactable = false;
            }
            private void LoadElements()
            {
                for (int index = 0; index < 12; ++index)
                {
                    elements.Add(template.Create(root));
                }
            }
            public SlotElement GetFinalElement() => elements.First();
            public void RandomizeIcons(SpriteManager.IconOption? fullHouseOption = null)
            {
                foreach (SlotElement element in elements)
                {
                    SpriteManager.IconOption randomIcon = (SpriteManager.IconOption)icons.GetValue(UnityEngine.Random.Range(0, icons.Length));
                    element.option = randomIcon;
                    Sprite? sprite = SpriteManager.GetSprite(randomIcon);
                    element.SetIcon(sprite);
                }

                if (fullHouseOption != null)
                {
                    SlotElement final = GetFinalElement();
                    final.SetIcon(SpriteManager.GetSprite((SpriteManager.IconOption)fullHouseOption));
                    final.option = fullHouseOption;
                    option = fullHouseOption;
                }
                else option = GetFinalElement().option;
            }
            public Slot Create(Transform parent)
            {
                GameObject go = Instantiate(prefab, parent);
                go.layer = 5;
                go.SetActive(true);
                Slot slot = new Slot(go.transform);
                slot.LoadElements();
                return slot;
            }
            public class SlotElement
            {
                private readonly GameObject prefab;
                private readonly Image bkg;
                private readonly Image icon;
                private readonly Image glow;
                private readonly Color glowColor;
                private readonly Sprite defaultSprite;
                public bool isGlowing => glow.gameObject.activeInHierarchy;
                private const float waveSpeed = 2f;
                public SpriteManager.IconOption? option;
                public SlotElement(Transform transform)
                {
                    prefab = transform.gameObject;
                    bkg = transform.GetComponent<Image>();
                    icon = transform.Find("Icon").GetComponent<Image>();
                    glow = transform.Find("Glow").GetComponent<Image>();
                    glowColor = glow.color;
                    SetGlow(false);
                    defaultSprite = icon.sprite;
                    SetBackgroundColor(Color.clear);
                }
                public SlotElement Create(Transform parent)
                {
                    GameObject go = Instantiate(prefab, parent);
                    go.layer = 5;
                    go.SetActive(true);
                    SlotElement element = new SlotElement(go.transform);
                    return element;
                }
                private void SetBackgroundColor(Color color) => bkg.color = color;
                public void SetIcon(Sprite? sprite) => icon.sprite = sprite ?? defaultSprite;
                public void SetGlow(bool enable) => glow.gameObject.SetActive(enable);
                public void UpdateGlow(float t, float staggerOffset = 0f)
                {
                    float time = t * waveSpeed + staggerOffset;
                    float alpha = (Mathf.Cos(time) + 1f) * 0.5f;
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    Color currentGlow = glowColor;
                    currentGlow.a = alpha;
                    glow.color = currentGlow;
                }
                public void ResetGlowAlpha()
                {
                    glow.color = glowColor;
                }
            }
        }
    }
}