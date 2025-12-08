using Almanac.Managers;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.UI;

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
public static class InventoryGui_Awake_Patch
{
    [UsedImplicitly]
    private static void Postfix(InventoryGui __instance)
    {
        if (Configs.AddLogs) AlmanacPlugin.AlmanacLogger.LogDebug("Initializing Almanac UI");
        var craftingPanel = __instance.m_crafting.gameObject;
        var trophyPanel = __instance.m_trophiesPanel;
        var sfx = craftingPanel.GetComponentInChildren<ButtonSfx>().m_sfxPrefab;

        Text[]? npcDialogueText = DialoguePanel._panel.GetComponentsInChildren<Text>(true);
        foreach (var component in DialoguePanel._panel.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
        FontManager.SetFont(npcDialogueText);
        var dialoguePanel = Object.Instantiate(DialoguePanel._panel, __instance.transform.parent.Find("HUD"));
        dialoguePanel.name = "Almanac NPC Dialogue";
        dialoguePanel.AddComponent<DialoguePanel>();

        var questPanel = Object.Instantiate(DialoguePanel._panel, __instance.transform.parent.Find("HUD"));
        questPanel.name = "Almanac Quest";
        questPanel.AddComponent<QuestPanel>();

        var npcTexts = NPCCustomization._Modal.GetComponentsInChildren<Text>(true);
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
        var npc_modal = Object.Instantiate(NPCCustomization._Modal, __instance.transform.parent.Find("HUD"));
        npc_modal.name = "Almanac NPC UI";

        var modalTexts = FormPanel._Modal.GetComponentsInChildren<Text>(true);
        foreach (var component in FormPanel._Modal.GetComponentsInChildren<ButtonSfx>(true))
            component.m_sfxPrefab = sfx;
        FontManager.SetFont(modalTexts);

        FormPanel._Modal.CopySpriteAndMaterial(trophyPanel, "bkg", "TrophiesFrame/border (1)");
        FormPanel._Modal.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/Title/TabBorder",
            "TabsButtons/TabBorder");
        FormPanel._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar",
            "TrophiesFrame/Trophies/TrophyListScroll");
        FormPanel._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar/Sliding Area/Handle",
            "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        FormPanel._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");
        FormPanel._Modal.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport/Button", "TrophiesFrame/Closebutton");
        FormPanel._Modal.CopyButtonState(trophyPanel, "ListView/Viewport/Button", "TrophiesFrame/Closebutton");
        FormPanel._Modal.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/InputField/Glow", "RepairButton/Glow");

        FormPanel._Modal.CopySpriteAndMaterial(trophyPanel, "MainButton", "TrophiesFrame/Closebutton");
        FormPanel._Modal.CopyButtonState(trophyPanel, "MainButton", "TrophiesFrame/Closebutton");

        FormPanel._Modal.AddComponent<FormPanel>();

        var panel = AssetBundleManager.LoadAsset<GameObject>("almanac_ui", "AlmanacUI")!;
        var go = Object.Instantiate(panel, __instance.transform.parent.Find("HUD"));
        go.name = "Almanac";

        var panelTexts = go.GetComponentsInChildren<Text>(true);
        foreach (var component in go.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
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
        
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Readme", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Tabs/Readme/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Tabs/Readme", "TabsButtons/Craft");

        go.CopySpriteAndMaterial(craftingPanel, "TabBorder", "TabsButtons/TabBorder");

        go.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Scrollbar/Sliding Area/Handle",
            "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");

        go.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport/ListElement/Background",
            "TrophiesFrame/Trophies/TrophyList/TrophyElement/icon_bkg");
        go.CopySpriteAndMaterial(trophyPanel, "ListView/Viewport/ListElement/Background/Icon",
            "TrophiesFrame/Trophies/TrophyList/TrophyElement/icon_bkg/icon");
        go.CopySpriteAndMaterial(craftingPanel, "ListView/Viewport/ListElement/Selected", "RepairButton/Glow");

        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Scrollbar/Sliding Area/Handle",
            "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Viewport", "TrophiesFrame/Trophies/TrophyList");

        go.CopySpriteAndMaterial(trophyPanel, "GamblePanel/Scrollbar", "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "GamblePanel/Scrollbar/Sliding Area/Handle",
            "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "GamblePanel/Viewport", "TrophiesFrame/Trophies/TrophyList");
        go.CopySpriteAndMaterial(craftingPanel, "GamblePanel/Viewport/Slot/Port/Viewport/Element/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "GamblePanel/Viewport/Slot/Port/Viewport/Element/Glow",
            "RepairButton/Glow");

        go.CopySpriteAndMaterial(trophyPanel, "ButtonView/Viewport/Button", "TrophiesFrame/Closebutton");
        go.CopyButtonState(trophyPanel, "ButtonView/Viewport/Button", "TrophiesFrame/Closebutton");
        go.CopySpriteAndMaterial(craftingPanel, "ButtonView/Viewport/Button/Selected", "RepairButton/Glow");

        go.CopySpriteAndMaterial(trophyPanel, "Closebutton", "TrophiesFrame/Closebutton");
        go.CopyButtonState(trophyPanel, "Closebutton", "TrophiesFrame/Closebutton");

        go.CopySpriteAndMaterial(craftingPanel, "Description", "Decription");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Icon", "Decription/Icon");

        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Scrollbar",
            "TrophiesFrame/Trophies/TrophyListScroll");
        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Scrollbar/Sliding Area/Handle",
            "TrophiesFrame/Trophies/TrophyListScroll/Sliding Area/Handle");
        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Viewport", "TrophiesFrame/Trophies/TrophyList");

        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Title/TabBorder",
            "TabsButtons/TabBorder");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/1",
            "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/1/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/2",
            "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/2/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/3",
            "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/3/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/4",
            "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/ListView/Viewport/Icons/4/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(trophyPanel, "Description/ListView/Viewport/Button", "TrophiesFrame/Closebutton");
        go.CopyButtonState(trophyPanel, "Description/ListView/Viewport/Button", "TrophiesFrame/Closebutton");

        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/1", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/2", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/3", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/4", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/1/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/2/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/3/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/List/4/Icon",
            "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/Star", "Decription/requirements/level");
        go.CopySpriteAndMaterial(craftingPanel, "Description/Requirements/Star/Icon",
            "Decription/requirements/level/MinLevel");

        go.CopySpriteAndMaterial(craftingPanel, "Description/MainButton", "Decription/craft_button_panel/CraftButton");
        go.CopyButtonState(craftingPanel, "Description/MainButton", "Decription/craft_button_panel/CraftButton");

        go.CopySpriteAndMaterial(craftingPanel, "SearchField", "Decription/craft_button_panel/CraftButton");
        go.CopySpriteAndMaterial(craftingPanel, "SearchField (1)", "Decription/craft_button_panel/CraftButton");

        go.CopySpriteAndMaterial(craftingPanel, "Currency/Icon", "Decription/requirements/res_bkg/res_icon");

        go.AddComponent<AlmanacPanel>();
        
        AlmanacPanel.inventoryButton = new AlmanacButton(__instance);
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
        if (Configs.AddLogs) AlmanacPlugin.AlmanacLogger.LogDebug("InventoryGUI.OnOpenTrophies.Almanac.Override");
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