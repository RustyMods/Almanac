using System;
using System.Collections.Generic;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using static Almanac.Utilities.Utility;
using Object = UnityEngine.Object;

namespace Almanac.UI;

public static class CreateAlmanac
{
    public static GameObject AlmanacGUI = null!;
    public static Image PanelIcon = null!;
    public static TextMeshProUGUI PanelTitle = null!;
    public static Transform PanelContent = null!;
    public static GameObject CreaturePanelElement = null!;
    
    public static GameObject AchievementGUI = null!;
    public static Image AchievementPanelIcon = null!;
    public static TextMeshProUGUI AchievementPanelTitle = null!;
    public static TextMeshProUGUI AchievementPanelDesc = null!;
    public static TextMeshProUGUI AchievementPanelTooltip = null!;
    public static TextMeshProUGUI AchievementPanelLore = null!;
    public static Button AchievementButton = null!;
    public static TextMeshProUGUI PanelButton = null!;
    public static TextMeshProUGUI AchievementPanelButton = null!;

    private static Image PanelImage = null!;
    private static Image AchievementPanelImage = null!;
    public static void OnPanelTransparencyConfigChange(object sender, EventArgs e)
    {
        PanelImage.color = AlmanacPlugin._PanelImage.Value is AlmanacPlugin.Toggle.On ? Color.clear : Color.white;
        AchievementPanelImage.color = AlmanacPlugin._PanelImage.Value is AlmanacPlugin.Toggle.On ? Color.clear : Color.white;
    }

    private static void RepositionTrophyPanel(float x, float y)
    {
        RectTransform? rect = CacheAssets.TrophiesFrame as RectTransform;
        if (rect == null) return;
        
        rect.anchoredPosition = new Vector2(x, y);
    }
    public static bool IsPanelActive() => AlmanacGUI && AlmanacGUI.activeSelf;
    public static bool IsAchievementActive() => AchievementGUI && AchievementGUI.activeSelf;
    private static void CreateAlmanacPanel(InventoryGui GUI)
    {
        if (AlmanacGUI != null) return; 
        
        AlmanacGUI = Object.Instantiate(CacheAssets.Panel, GUI.transform);
        AlmanacGUI.SetActive(false);
        
        RectTransform? rect = AlmanacGUI.transform as RectTransform;
        if (rect == null) return;

        rect.anchoredPosition = new Vector2(680f, 0f);

        PanelImage = Utils.FindChild(AlmanacGUI.transform, "Panel").GetComponent<Image>();
        PanelImage.material = CacheAssets.WoodPanel.material;
        PanelImage.sprite = CacheAssets.WoodPanel.sprite;
        PanelImage.color = AlmanacPlugin._PanelImage.Value is  AlmanacPlugin.Toggle.Off ? Color.white : Color.clear;

        if (!Utils.FindChild(AlmanacGUI.transform, "$part_left_braid").TryGetComponent(out Image leftImage)) return;
        if (!Utils.FindChild(AlmanacGUI.transform, "$part_right_braid").TryGetComponent(out Image rightImage)) return;

        leftImage.sprite = CacheAssets.BraidLeft.sprite;
        rightImage.sprite = CacheAssets.BraidRight.sprite;

        PanelIcon = Utils.FindChild(AlmanacGUI.transform, "$part_icon").GetComponent<Image>();
        PanelIcon.sprite = SpriteManager.AlmanacIcon;

        PanelTitle = AddTextMeshProGUI(Utils.FindChild(AlmanacGUI.transform, "$part_name").gameObject, true);
        PanelTitle.fontSize = 40;
        PanelTitle.fontSizeMin = 20;
        PanelTitle.fontSizeMax = 50;
        PanelTitle.text = Localization.instance.Localize("$almanac_name");
        
        PanelContent = Utils.FindChild(AlmanacGUI.transform, "$part_Content");

        Transform CloseButton = Utils.FindChild(AlmanacGUI.transform, "$part_CloseButton");
        if (!CloseButton.TryGetComponent(out Image ButtonImage)) return;
        ButtonImage.sprite = CacheAssets.ButtonImage.sprite;
        ButtonImage.material = CacheAssets.ButtonImage.material;
        if (!CloseButton.TryGetComponent(out Button button)) return;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = CacheAssets.ButtonComponent.spriteState;
        button.onClick.AddListener(ToggleButtonOptions);

        ButtonSfx sfx = CloseButton.gameObject.AddComponent<ButtonSfx>();
        sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;

        PanelButton = AddTextMeshProGUI(CloseButton.Find("Text").gameObject);
        PanelButton.fontSize = 16;
        PanelButton.fontSizeMin = 5;
        PanelButton.fontSizeMax = 20;
        PanelButton.text = Localization.instance.Localize("$almanac_stats_button");
    }
    private static void ToggleButtonOptions()
    {
        if (UpdateAlmanac.isMetricsActive)
        {
            UpdateAlmanac.UpdateLeaderboardPanel();
        }
        else
        {
            UpdateAlmanac.UpdateMetricsPanel();
        }
    }

    private static void CreateAchievementPanel(InventoryGui GUI)
    {
        if (AchievementGUI != null) return;
        
        AchievementGUI = Object.Instantiate(CacheAssets.AchievementPanel, GUI.transform);
        AchievementGUI.SetActive(false);
        
        RectTransform? rect = AchievementGUI.transform as RectTransform;
        if (rect == null) return;

        rect.anchoredPosition = new Vector2(680f, 0f);

        AchievementPanelImage = Utils.FindChild(AchievementGUI.transform, "Panel").GetComponent<Image>();
        AchievementPanelImage.material = CacheAssets.WoodPanel.material;
        AchievementPanelImage.sprite = CacheAssets.WoodPanel.sprite;
        AchievementPanelImage.color = AlmanacPlugin._PanelImage.Value is  AlmanacPlugin.Toggle.Off ? Color.white : Color.clear;

        Transform iconBkg = Utils.FindChild(AchievementGUI.transform, "icon_bkg");
        if (!iconBkg.TryGetComponent(out Image bkgImage)) return;
        bkgImage.sprite = CacheAssets.TrophyImage.sprite;
        bkgImage.material = CacheAssets.TrophyImage.material;
        
        Transform icon = Utils.FindChild(AchievementGUI.transform, "$part_icon");

        if (!icon.GetComponent<ButtonSfx>())
        {
            ButtonSfx iconSfx = icon.gameObject.AddComponent<ButtonSfx>();
            iconSfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;
        }

        if (!icon.TryGetComponent(out Button iconButton)) return;
        AchievementButton = iconButton;
        
        AchievementPanelIcon = icon.GetComponent<Image>();
        AchievementPanelIcon.sprite = SpriteManager.AlmanacIcon;

        AchievementPanelTitle = AddTextMeshProGUI(Utils.FindChild(AchievementGUI.transform, "$part_name").gameObject, true);
        AchievementPanelTitle.fontSize = 40;
        AchievementPanelTitle.fontSizeMin = 20;
        AchievementPanelTitle.fontSizeMax = 50;
        AchievementPanelTitle.text = "Achievement";

        AchievementPanelDesc = AddTextMeshProGUI(Utils.FindChild(AchievementGUI.transform, "$part_desc").gameObject);
        AchievementPanelDesc.fontSize = 25;
        AchievementPanelDesc.fontSizeMin = 16;
        AchievementPanelDesc.fontSizeMax = 30;
        AchievementPanelDesc.text = "Description";
        AchievementPanelDesc.color = Color.white;
        
        AchievementPanelTooltip = AddTextMeshProGUI(Utils.FindChild(AchievementGUI.transform, "$part_tooltip").gameObject);
        AchievementPanelTooltip.fontSize = 16;
        AchievementPanelTooltip.fontSizeMin = 14;
        AchievementPanelTooltip.fontSizeMax = 20;
        AchievementPanelTooltip.text = "Tooltip";
        AchievementPanelTooltip.color = Color.white;

        AchievementPanelLore = AddTextMeshProGUI(Utils.FindChild(AchievementGUI.transform, "$part_lore").gameObject);
        AchievementPanelLore.fontSize = 18;
        AchievementPanelLore.fontSizeMin = 12;
        AchievementPanelLore.fontSizeMax = 25;
        AchievementPanelLore.text = "Lore";
        AchievementPanelLore.color = Color.white;

        Transform CloseButton = Utils.FindChild(AchievementGUI.transform, "$part_CloseButton");
        if (CloseButton.TryGetComponent(out Image ButtonImage))
        {
            ButtonImage.sprite = CacheAssets.ButtonImage.sprite;
            ButtonImage.material = CacheAssets.ButtonImage.material;
        }

        if (CloseButton.TryGetComponent(out Button button))
        {
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = CacheAssets.ButtonComponent.spriteState;
            button.onClick.AddListener(() =>
            {
                AchievementPanelIcon.sprite = SpriteManager.AlmanacIcon;
                AchievementPanelTitle.text = Localization.instance.Localize("$almanac_name");
                AchievementPanelDesc.text = "";
                AchievementPanelLore.text = "";
                AchievementPanelTooltip.text = "";
                ToggleButtonOptions();
            });
        };

        ButtonSfx sfx = CloseButton.gameObject.AddComponent<ButtonSfx>();
        sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;

        AchievementPanelButton = AddTextMeshProGUI(CloseButton.Find("Text").gameObject,true);
        AchievementPanelButton.fontSize = 16;
        AchievementPanelButton.fontSizeMin = 5;
        AchievementPanelButton.fontSizeMax = 20;
        AchievementPanelButton.text = Localization.instance.Localize("$almanac_stats_button");
        
        AchievementButton.onClick.AddListener(OnClickAchievement);
    }

    private static void OnClickAchievement()
    {
        if (UpdateAlmanac.SelectedAchievement.m_statusEffect == null) return;
        if (!Player.m_localPlayer) return;

        if (UpdateAlmanac.SelectedAchievement.m_statusEffect.m_ttl > 0)
        {
            Player.m_localPlayer.m_guardianSE = UpdateAlmanac.SelectedAchievement.m_statusEffect;
            Player.m_localPlayer.m_guardianPower = UpdateAlmanac.SelectedAchievement.m_statusEffect.name;
            Player.m_localPlayer.m_guardianPowerHash = UpdateAlmanac.SelectedAchievement.m_statusEffect.name.GetStableHashCode();
            return;
        }
        
        SEMan StatusEffectMan = Player.m_localPlayer.GetSEMan();
        ISerializer serializer = new SerializerBuilder().Build();
        if (StatusEffectMan.HaveStatusEffect(UpdateAlmanac.SelectedAchievement.m_statusEffect.name))
        {
            if (!StatusEffectMan.RemoveStatusEffect(UpdateAlmanac.SelectedAchievement.m_statusEffect, true)) return;    
            AlmanacEffectManager.ActiveAchievementEffects.Remove(UpdateAlmanac.SelectedAchievement.m_statusEffect);
            AlmanacEffectManager.SavedAchievementEffectNames.Remove(UpdateAlmanac.SelectedAchievement.m_statusEffect.name);
            Player.m_localPlayer.m_customData[AlmanacEffectManager.AchievementKey] = serializer.Serialize(AlmanacEffectManager.SavedAchievementEffectNames);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$almanac_removed_effect {UpdateAlmanac.SelectedAchievement.m_statusEffect.m_name}");
            if (InventoryGui.instance)
            {
                UpdateAlmanac.UpdateList(InventoryGui.instance);
            }
            return;
        };
        int count = StatusEffectMan.GetStatusEffects().FindAll(effect => effect is AlmanacEffectManager.AchievementEffect).Count;
        if (count >= AlmanacPlugin._AchievementThreshold.Value)
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$almanac_too_many_achievements");
            return;
        }
        StatusEffectMan.AddStatusEffect(UpdateAlmanac.SelectedAchievement.m_statusEffect);
        AlmanacEffectManager.ActiveAchievementEffects.Add(UpdateAlmanac.SelectedAchievement.m_statusEffect);
        if (AlmanacEffectManager.SavedAchievementEffectNames.Count > 3) return;
        AlmanacEffectManager.SavedAchievementEffectNames.Add(UpdateAlmanac.SelectedAchievement.m_statusEffect.name);
        Player.m_localPlayer.m_customData[AlmanacEffectManager.AchievementKey] = serializer.Serialize(AlmanacEffectManager.SavedAchievementEffectNames);
        if (InventoryGui.instance)
        {
            UpdateAlmanac.UpdateList(InventoryGui.instance);
        }
    }

    private static void EditPanelItem()
    {
        Transform type = Utils.FindChild(CacheAssets.Item.transform, "$part_infoType");
        Transform data = Utils.FindChild(CacheAssets.Item.transform, "$part_data");

        if (!type.GetComponent<TextMeshProUGUI>())
        {
            TextMeshProUGUI typeText = AddTextMeshProGUI(type.gameObject);
            typeText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            typeText.overflowMode = TextOverflowModes.Ellipsis;
        }

        if (!data.GetComponent<TextMeshProUGUI>())
        {
            TextMeshProUGUI dataText = AddTextMeshProGUI(data.gameObject);
            dataText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            dataText.color = Color.white;
            dataText.overflowMode = TextOverflowModes.Ellipsis;
        }
        
    }

    private static void EditDropItem()
    {
        Transform title = Utils.FindChild(CacheAssets.Drops.transform, "$part_title");

        if (!title.GetComponent<TextMeshProUGUI>())
        {
            TextMeshProUGUI titleText = AddTextMeshProGUI(title.gameObject, true);
            titleText.overflowMode = TextOverflowModes.Overflow;
            titleText.fontSize = 16;
            titleText.text = Localization.instance.Localize("$almanac_creature_drops");
        }
        
        for (int i = 0; i < CacheAssets.Drops.transform.Find("dropContainer").childCount; ++i)
        {
            Transform part = Utils.FindChild(CacheAssets.Drops.transform, $"$part_drop_{i}");
            Transform name = part.GetChild(0);

            if (name.GetComponent<TextMeshProUGUI>()) continue;
            
            TextMeshProUGUI nameText = AddTextMeshProGUI(name.gameObject);
            nameText.fontSize = 10;
            nameText.verticalAlignment = VerticalAlignmentOptions.Bottom;
            nameText.color = Color.white;
        }
    }

    private static void EditTitleItem()
    {
        Transform text = CacheAssets.ItemTitle.transform.Find("$part_text");

        if (text.GetComponent<TextMeshProUGUI>()) return;
        
        TextMeshProUGUI component = AddTextMeshProGUI(text.gameObject, true, TextWrappingModes.NoWrap);
        component.fontSize = 16;
    }

    private static void EditLeaderboardItem()
    {
        if (CacheAssets.LeaderboardItem.TryGetComponent(out Image leaderboardBkg))
        {
            // leaderboardBkg.sprite = CacheAssets.WoodPanel.sprite;
            leaderboardBkg.material = CacheAssets.WoodPanel.material;
        }
        if (Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_right").TryGetComponent(out Image rightImage))
        {
            rightImage.sprite = CacheAssets.BraidLeft.sprite;
            rightImage.color = new Color(1f, 0.5f, 0f, 1f);
        }
        if (Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_left").TryGetComponent(out Image leftImage))
        {
            leftImage.sprite = CacheAssets.BraidRight.sprite;
            leftImage.color = new Color(1f, 0.5f, 0f, 1f);
        }

        if (!Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_name").GetComponent<TextMeshProUGUI>())
        {
            TextMeshProUGUI name = AddTextMeshProGUI(Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_name").gameObject, true);
            name.fontSizeMin = 15;
            name.fontSize = 20;
            name.fontSizeMax = 25;
        }

        Transform rank = Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_rank");
        Transform achievement = Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_achievement");
        Transform kd = Utils.FindChild(CacheAssets.LeaderboardItem.transform, "$part_kd");

        List<Transform> items = new List<Transform>() { rank, achievement, kd };
        List<string> keys = new() { "$almanac_rank_title", "$almanac_achievement_title", "$almanac_kd_title" };
        for (int index = 0; index < items.Count; index++)
        {
            Transform element = items[index];
            string keyText = keys[index];

            if (!element.GetChild(0).GetComponent<TextMeshProUGUI>())
            {
                TextMeshProUGUI key = AddTextMeshProGUI(element.GetChild(0).gameObject);
                key.horizontalAlignment = HorizontalAlignmentOptions.Left;
                key.color = Color.white;
                key.text = Localization.instance.Localize(keyText);
            };

            if (!element.GetChild(1).GetComponent<TextMeshProUGUI>())
            {
                TextMeshProUGUI value = AddTextMeshProGUI(element.GetChild(1).gameObject);
                value.horizontalAlignment = HorizontalAlignmentOptions.Right;
                value.color = Color.white;
            }
        }
    }

    private static void EditInventoryGUI(InventoryGui GUI)
    {
        Transform info = Utils.FindChild(GUI.m_inventoryRoot.transform, "Info");
        Transform trophiesOpenButton = Utils.FindChild(info, "Trophies");
        Transform image = Utils.FindChild(trophiesOpenButton, "Image");

        trophiesOpenButton.TryGetComponent(out UITooltip openTrophiesToolTip);
        image.TryGetComponent(out Image trophiesOpenImage);
            
        if (openTrophiesToolTip) openTrophiesToolTip.m_text = "$almanac_name";
        if (trophiesOpenImage) trophiesOpenImage.sprite = SpriteManager.AlmanacIcon;
    }

    private static GameObject CreateCreaturePanelElement()
    {
        GameObject CreatureElement = new GameObject("element");
        RectTransform rect = CreatureElement.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250f, 32f);

        Image elementImage = CreatureElement.AddComponent<Image>();
        elementImage.sprite = CacheAssets.ButtonImage.sprite;
        elementImage.color = CacheAssets.ButtonImage.color;
        elementImage.raycastTarget = true;
        elementImage.maskable = true;
        elementImage.type = Image.Type.Sliced;
        elementImage.fillCenter = true;
        elementImage.pixelsPerUnitMultiplier = 1f;

        ButtonSfx sfx = CreatureElement.AddComponent<ButtonSfx>();
        sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;

        Button button = CreatureElement.AddComponent<Button>();
        button.interactable = true;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = CacheAssets.ButtonComponent.spriteState;

        GameObject text = new GameObject("text");
        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.SetParent(rect);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(250f, 32f);

        TextMeshProUGUI textComponent = text.AddComponent<TextMeshProUGUI>();
        textComponent.font = CacheAssets.NorseFont;
        textComponent.color = Color.yellow;
        textComponent.autoSizeTextContainer = false;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
        textComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textComponent.fontSize = 16;

        button.targetGraphic = elementImage;
        button.onClick = new Button.ButtonClickedEvent();

        return CreatureElement;
    }
    
    private static void AddSearchBar()
    {
        GameObject searchBar = Object.Instantiate(CacheAssets.SearchBar, CacheAssets.TrophiesFrame);
        RectTransform? rect = searchBar.transform as RectTransform;
        if (rect == null) return;

        rect.anchoredPosition = new Vector2(1050f, 760f);

        InputField SearchField = Utils.FindChild(searchBar.transform, "$part_searchbar").GetComponent<InputField>();
        SearchField.onValueChanged.AddListener(FilterList);
    }

    private static void EditTrophyElement(InventoryGui instance)
    {
        GameObject ElementOutline = new GameObject("$part_outline");
        RectTransform rect = ElementOutline.AddComponent<RectTransform>();
        rect.SetParent(instance.m_trophieElementPrefab.transform);
        rect.anchoredPosition = new Vector2(0f, 50f);
        rect.sizeDelta = new Vector2(72f, 72f);
        
        rect.SetAsFirstSibling();

        Image image = ElementOutline.AddComponent<Image>();
        image.color = AlmanacPlugin._OutlineColor.Value;
        
        ElementOutline.SetActive(false);
    }

    private static void FilterList(string value) => UpdateAlmanac.UpdateList(InventoryGui.m_instance, value);

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    private static class ModifyInventoryGUI
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            CacheAssets.GetAssets(__instance);
            Categories.CreateAssets(__instance);
            RepositionTrophyPanel(-220f, 0f);
            CreateAlmanacPanel(__instance);
            CreateAchievementPanel(__instance);
            EditPanelItem();
            EditDropItem();
            EditTitleItem();
            EditInventoryGUI(__instance);
            AddSearchBar();
            EditLeaderboardItem();
            EditTrophyElement(__instance);
            if (CreaturePanelElement == null) CreaturePanelElement = CreateCreaturePanelElement();
            CreatureDataCollector.GetSortedCreatureData();
            CreatureLists.InitCreatureLists();
            if (AlmanacPlugin.JewelCraftLoaded) ItemDataCollector.GetJewels();
        }
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
    private static class PlayerControllerOverride
    {
        private static bool Prefix() => !IsPanelActive() || !IsAchievementActive();
    }
}