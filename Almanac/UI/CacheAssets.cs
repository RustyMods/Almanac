﻿using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.UI;

public static class CacheAssets
{
    public static TMP_FontAsset NorseFont = null!;
    public static TMP_FontAsset NorseFontBold = null!;
    public static TextMeshProUGUI TopicTextMeshPro = null!;
    public static ButtonSfx ButtonSFX = null!;
    public static Image ButtonImage = null!;
    public static Button ButtonComponent = null!;
    public static Image TrophyImage = null!;
    public static Image WoodPanel = null!;
    public static Transform TrophiesFrame = null!;
    public static Image BraidLeft = null!;
    public static Image BraidRight = null!;

    public static GameObject Panel = null!;
    public static GameObject Item = null!;
    public static GameObject Drops = null!;
    public static GameObject ItemTitle = null!;
    public static GameObject AchievementPanel = null!;
    public static GameObject SearchBar = null!;
    public static GameObject LeaderboardItem = null!;

    public static GameObject TrophyElement = null!;
    
    [HarmonyWrapSafe]
    public static void GetAssets(InventoryGui GUI, bool reload = false)
    {
        if (!GUI) return;
        TrophiesFrame = Utils.FindChild(GUI.transform, "TrophiesFrame");
        Transform CloseButton = Utils.FindChild(TrophiesFrame, "Closebutton");
        Transform Border = Utils.FindChild(TrophiesFrame, "border (1)");
        Transform Topic = Utils.FindChild(TrophiesFrame, "topic");
        Transform Text = Utils.FindChild(TrophiesFrame, "Text");
        Transform IconBkg = Utils.FindChild(TrophiesFrame, "icon_bkg");

        NorseFont = Text.GetComponent<TextMeshProUGUI>().font;
        TopicTextMeshPro = Topic.GetComponent<TextMeshProUGUI>();
        NorseFontBold = TopicTextMeshPro.font;
        ButtonSFX = CloseButton.GetComponent<ButtonSfx>();
        ButtonImage = CloseButton.GetComponent<Image>();
        ButtonComponent = CloseButton.GetComponent<Button>();
        TrophyImage = IconBkg.GetComponent<Image>();
        WoodPanel = Border.GetComponent<Image>();
        BraidLeft = GUI.m_info.transform.Find("TitlePanel/BraidLineHorisontalMedium (1)").GetComponent<Image>();
        BraidRight = GUI.m_info.transform.Find("TitlePanel/BraidLineHorisontalMedium (2)").GetComponent<Image>();

        if (reload) return;
        TrophyElement = Object.Instantiate(GUI.m_trophieElementPrefab, AlmanacPlugin._root.transform, false);
        TrophyElement.name = "AlmanacElement";
        GameObject ElementOutline = new GameObject("$part_outline");
        RectTransform rect = ElementOutline.AddComponent<RectTransform>();
        rect.SetParent(TrophyElement.transform);
        rect.anchoredPosition = new Vector2(0f, 50f);
        rect.sizeDelta = new Vector2(72f, 72f);
        
        rect.SetAsFirstSibling();

        Image image = ElementOutline.AddComponent<Image>();
        image.color = AlmanacPlugin._OutlineColor.Value;
        
        ElementOutline.SetActive(false);
        UITools.AddButtonComponent(TrophyElement);
    }

    public static void ReloadAssets()
    {
        try
        {
            GetAssets(InventoryGui.instance, true);
            CreateAlmanac.PanelImage.material = WoodPanel.material;
            CreateAlmanac.PanelImage.sprite = WoodPanel.sprite;
            CreateAlmanac.PanelImage.color = WoodPanel.color;
            CreateAlmanac.AchievementPanelImage.material = WoodPanel.material;
            CreateAlmanac.AchievementPanelImage.sprite = WoodPanel.sprite;
            CreateAlmanac.AchievementPanelImage.color = WoodPanel.color;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to reload graphics");
        }
    }

    public static void LoadAssets()
    {
        Panel = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_gui");
        Item = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_item");
        Drops = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_drops");
        ItemTitle = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_item_title");
        AchievementPanel = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_achievement");
        SearchBar = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_search");
        LeaderboardItem = AlmanacPlugin._assets.LoadAsset<GameObject>("almanac_leaderboard");
    }
}