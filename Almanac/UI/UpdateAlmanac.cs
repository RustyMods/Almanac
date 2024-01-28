using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Almanac.Achievements.AchievementManager;
using static Almanac.AlmanacPlugin;
using static Almanac.Data.CreatureDataCollector;
using static Almanac.Data.ItemDataCollector;
using static Almanac.Data.PieceDataCollector;
using static Almanac.Data.PlayerStats;
using static Almanac.UI.Categories;
using static Almanac.Utilities.Utility;
using Object = UnityEngine.Object;

namespace Almanac.UI;

public static class UpdateAlmanac
{
    private static readonly List<GameObject> PanelElements = new();
    
    private static readonly float TrophySpacing = 180f;
    private static readonly string UnknownText = Localization.instance.Localize("$almanac_locked");
    
    private static readonly List<string> ValuesToIgnore = new()
    {
        "0",
        "0+0/lvl",
        "none",
        "normal",
        "0%",
        "0s",
        "0/tick"
    };
    private static bool IsValueToBeIgnored(string input) => ValuesToIgnore.Contains(RemoveArrows(Localization.instance.Localize(input)).ToLower().Replace(" ", "")) || input.IsNullOrWhiteSpace();
    private static string RemoveArrows(string input) => Regex.Replace(input, "<.*?>", "");
    private static ItemDrop SelectedItemDrop = null!;
    private static CreatureData SelectedCreature = null!;
    private static GameObject SelectedPiece = null!;
    public static Achievement SelectedAchievement = null!;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateTrophyList))]
    private static class UpdateTrophyListPatch
    {
        private static bool Prefix(InventoryGui __instance)
        {
            if (!__instance) return false;
            if (!Player.m_localPlayer) return false;
            
            UpdateTopic();
            UpdateList(__instance);

            return false;
        }
    }
    public static void UpdateList(InventoryGui GUI, string filter = "")
    {
        DestroyTrophies(GUI);
        switch (SelectedTab)
            {
                case "$almanac_scroll_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetScrolls() 
                        : GetScrolls().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_jewel_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetJewels() 
                        : GetJewels().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_ammo_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetAmmunition() 
                        : GetAmmunition().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_fish_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFishes() 
                        : GetFishes().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_material_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetMaterials()
                        : GetMaterials().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_weapon_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetWeapons()
                        : GetWeapons().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_gear_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetEquipments()
                        : GetEquipments().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_consumable_button":
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetConsumables()
                        : GetConsumables().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_creature_button":
                    UpdateCreatureList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetSavedCreatureData()
                        : GetSavedCreatureData().FindAll(item => Localization.instance.Localize(item.display_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_miscPieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(miscPieces)
                        : GetFilteredPieces(miscPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_plantPieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(plantPieces) 
                        : GetFilteredPieces(plantPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_buildPieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(buildPieces) 
                        : GetFilteredPieces(buildPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_craftingPieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(craftingPieces) 
                        : GetFilteredPieces(craftingPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_furniturePieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(furniturePieces) 
                        : GetFilteredPieces(furniturePieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_other_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(defaultPieces) 
                        : GetFilteredPieces(defaultPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_modPieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(modPieces) 
                        : GetFilteredPieces(modPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_comfortPieces_button":
                    UpdatePieceList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetFilteredPieces(comfortPieces) 
                        : GetFilteredPieces(comfortPieces).FindAll(item => Localization.instance.Localize(item.GetComponent<Piece>().m_name).ToLower().Contains(filter)));
                    break;
                case "$almanac_achievements_button":
                    UpdateAchievementList(GUI, filter.IsNullOrWhiteSpace() 
                        ? AchievementList 
                        : AchievementList.FindAll(item => item.m_displayName.ToLower().Contains(filter)));
                    break;
                case "$almanac_stats_button":
                    UpdateMetricsPanel();
                    break;
                case "$almanac_leaderboard_button":
                    UpdateLeaderboardPanel();
                    break;
                default:
                    UpdateItemList(GUI, filter.IsNullOrWhiteSpace() 
                        ? GetTrophies() 
                        : GetTrophies().FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter)), true);
                    break;
            }
    }
    private static void UpdateTopic()
    {
         Transform topic = CacheAssets.TrophiesFrame.Find("topic");

         if (!topic.TryGetComponent(out TextMeshProUGUI textMesh)) return;

         textMesh.text = Localization.instance.Localize(SelectedTab);
    }
    private static void DestroyTrophies(InventoryGui instance)
    {
        foreach (GameObject trophy in instance.m_trophyList) Object.Destroy(trophy);
        
        instance.m_trophyList.Clear();
    }
    private static void UpdateItemList(InventoryGui instance, List<ItemDrop> items, bool isTrophies = false)
    {
        float a1 = 0.0f;
        for (int index = 0; index < items.Count; ++index)
        {
            ItemDrop component = items[index];
            string LocalizedName = Localization.instance.Localize(component.m_itemData.m_shared.m_name);
            bool isKnown = _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsKnownMaterial(component.m_itemData.m_shared.m_name) || Player.m_localPlayer.NoCostCheat();
            
            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);

            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            float x = (index % 7) * TrophySpacing;
            float y = Mathf.FloorToInt(index / 7f) * -TrophySpacing;
            
            transform.anchoredPosition = new Vector2(x, y);
            
            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);
            
            string LocalizedDesc = Localization.instance.Localize(component.m_itemData.m_shared.m_description);
            string LocalizedLore = Localization.instance.Localize(component.m_itemData.m_shared.m_name + "_lore");

            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = component.m_itemData.GetIcon();
            
            transform.Find("name").GetComponent<TMP_Text>().text = isKnown ? LocalizedName : UnknownText;
            transform.Find("description").GetComponent<TMP_Text>().text = isKnown ? isTrophies ? LocalizedLore : LocalizedDesc : "";
            iconImage.color = isKnown ? Color.white : Color.black;

            Button button = icon.gameObject.AddComponent<Button>();
            button.interactable = isKnown;
            button.targetGraphic = iconImage;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock()
            {
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                disabledColor = new Color(0f, 0f, 0f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = Color.white
            };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedItemDrop = component;
                UpdateItemPanel();
            });
            ButtonSfx sfx = icon.gameObject.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;
            
            instance.m_trophyList.Add(gameObject);
        }
        instance.m_trophieListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(instance.m_trophieListBaseSize, -a1));
        instance.m_trophyListScroll.value = 1f;
    }
    private static void UpdatePieceList(InventoryGui instance, List<GameObject> prefabs)
    {
        float a1 = 0.0f;
        for (int index = 0; index < prefabs.Count; ++index)
        {
            GameObject prefab = prefabs[index];
            if (!prefab.TryGetComponent(out Piece piece)) continue;
            string LocalizedName = Localization.instance.Localize(piece.m_name);
            bool isKnown = _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsRecipeKnown(piece.m_name) || Player.m_localPlayer.NoCostCheat();
            
            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);
            
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            float x = (index % 7) * TrophySpacing;
            float y = Mathf.FloorToInt(index / 7f) * -TrophySpacing;
            
            transform.anchoredPosition = new Vector2(x, y);
            
            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);
            
            string LocalizedDesc = Localization.instance.Localize(piece.m_description);

            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = piece.m_icon;
            
            transform.Find("name").GetComponent<TMP_Text>().text = isKnown ? LocalizedName : UnknownText;
            transform.Find("description").GetComponent<TMP_Text>().text = isKnown ? LocalizedDesc : "";
            iconImage.color = isKnown ? Color.white : Color.black;

            Button button = icon.gameObject.AddComponent<Button>();
            button.interactable = isKnown;
            button.targetGraphic = iconImage;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock()
            {
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                disabledColor = new Color(0f, 0f, 0f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = Color.white
            };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedPiece = prefab;
                UpdatePiecePanel();
            });
            ButtonSfx sfx = icon.gameObject.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;
            
            instance.m_trophyList.Add(gameObject);
        }
        instance.m_trophieListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(instance.m_trophieListBaseSize, -a1));
        instance.m_trophyListScroll.value = 1f;
    }
    private static void UpdateCreatureList(InventoryGui instance, List<CreatureData> creatures)
    {
        float totalHeight = (Mathf.CeilToInt(creatures.Count / 5f) * 34f);
        float half = (totalHeight / 2f) - 17f;
        if (totalHeight <= instance.m_trophieListBaseSize)
        {
            half = 306f;
        }
        for (int index = 0; index < creatures.Count; ++index)
        {
            CreatureData data = creatures[index];
            string LocalizedName = Localization.instance.Localize(data.display_name);
            bool isKnown = ZoneSystem.instance.GetGlobalKeys().Contains(data.defeatedKey) || ZoneSystem.instance.GetGlobalKey(data.defeatedKey) || _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.NoCostCheat();
            GameObject gameObject = Object.Instantiate(CreateAlmanac.CreaturePanelElement, instance.m_trophieListRoot);
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            float x = -501f + ((index % 5) * 251f); 
            float y = half - (Mathf.FloorToInt(index / 5f) * 34f);

            transform.anchoredPosition = new Vector2(x, y);
            
            if (!transform.Find("text").TryGetComponent(out TextMeshProUGUI text)) continue;
            text.text = isKnown ? LocalizedName : UnknownText;
            
            if (!gameObject.TryGetComponent(out Button button)) continue;
            button.interactable = isKnown;
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedCreature = data;
                UpdateCreaturePanel();
            });
            
            instance.m_trophyList.Add(gameObject);
        }
        instance.m_trophieListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(instance.m_trophieListBaseSize, totalHeight));
        instance.m_trophyListScroll.value = 1f;
    }

    public static bool CheckedCompletion;
    private static void UpdateAchievementList(InventoryGui instance, List<Achievement> achievements)
    {
        float a1 = 0.0f;
        for (int index = 0; index < achievements.Count; ++index)
        {
            Achievement achievement = achievements[index];
            
            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);
            
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            float x = (index % 7) * TrophySpacing;
            float y = Mathf.FloorToInt(index / 7f) * -TrophySpacing;
            
            transform.anchoredPosition = new Vector2(x, y);
            
            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);

            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = achievement.m_sprite ? achievement.m_sprite : SpriteManager.AlmanacIcon;

            bool isCompleted = achievement.m_isCompleted || Player.m_localPlayer.NoCostCheat();
            
            transform.Find("name").GetComponent<TMP_Text>().text = isCompleted ? achievement.m_displayName : UnknownText;
            transform.Find("description").GetComponent<TMP_Text>().text = achievement.m_desc;

            Button button = icon.gameObject.AddComponent<Button>();
            button.interactable = true;
            button.targetGraphic = iconImage;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock()
            {
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                disabledColor = new Color(0f, 0f, 0f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = achievement.m_isCompleted ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.black,
                selectedColor = Color.white
            };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementPanelButton.text = Localization.instance.Localize(isMetricsActive ? "$almanac_leaderboard_button" : "$almanac_stats_button");
                CreateAlmanac.AchievementGUI.SetActive(true);
                CreateAlmanac.AlmanacGUI.SetActive(false);
                SelectedAchievement = achievement;
                UpdateAchievementPanel();
            });
            ButtonSfx sfx = icon.gameObject.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;
            
            instance.m_trophyList.Add(gameObject);
        }
        instance.m_trophieListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(instance.m_trophieListBaseSize, -a1));
        instance.m_trophyListScroll.value = 1f;
    }
    private static void UpdateAchievementPanel()
    {
        DestroyPanelElements();
        SetAchievementDesc();
        
        bool isCompleted = SelectedAchievement.m_isCompleted || Player.m_localPlayer.NoCostCheat();
        
        CreateAlmanac.AchievementPanelIcon.sprite = SelectedAchievement.m_sprite;
        CreateAlmanac.AchievementPanelIcon.color = isCompleted ? Color.white : Color.black;
        CreateAlmanac.AchievementButton.interactable = isCompleted && _AchievementPowers.Value is AlmanacPlugin.Toggle.On;
        CreateAlmanac.AchievementPanelTitle.text = isCompleted ? SelectedAchievement.m_displayName : UnknownText;
        
        if (SelectedAchievement.m_statusEffect != null) CreateAlmanac.AchievementPanelTooltip.text = SelectedAchievement.m_statusEffect.m_tooltip;
        CreateAlmanac.AchievementPanelLore.text = isCompleted ? SelectedAchievement.m_lore : UnknownText;
    }
    private static void SetAchievementDesc()
    {
        switch (SelectedAchievement.m_type)
        {
            case AchievementTypes.AchievementType.Deaths:
                SetAchievementPanel(PlayerStatType.Deaths);
                break;
            case AchievementTypes.AchievementType.Fish:
                SetAchievementPanel(GetFishes());
                break;
            case AchievementTypes.AchievementType.Materials:
                SetAchievementPanel(GetMaterials());
                break;
            case AchievementTypes.AchievementType.Consumables:
                SetAchievementPanel(GetConsumables());
                break;
            case AchievementTypes.AchievementType.Weapons:
                SetAchievementPanel(GetWeapons());
                break;
            case AchievementTypes.AchievementType.Swords:
                SetAchievementPanel(GetSwords());
                break;
            case AchievementTypes.AchievementType.Axes:
                SetAchievementPanel(GetAxes());
                break;
            case AchievementTypes.AchievementType.PoleArms:
                SetAchievementPanel(GetPoleArms());
                break;
            case AchievementTypes.AchievementType.Spears:
                SetAchievementPanel(GetSpears());
                break;
            case AchievementTypes.AchievementType.Maces:
                SetAchievementPanel(GetMaces());
                break;
            case AchievementTypes.AchievementType.Knives:
                SetAchievementPanel(GetKnives());
                break;
            case AchievementTypes.AchievementType.Shields:
                SetAchievementPanel(GetShields());
                break;
            case AchievementTypes.AchievementType.Staves:
                SetAchievementPanel(GetStaves());
                break;
            case AchievementTypes.AchievementType.Arrows:
                SetAchievementPanel(GetAmmunition());
                break;
            case AchievementTypes.AchievementType.Bows:
                SetAchievementPanel(GetBows());
                break;
            case AchievementTypes.AchievementType.Valuables:
                SetAchievementPanel(GetValuables());
                break;
            case AchievementTypes.AchievementType.Potions:
                SetAchievementPanel(GetPotions());
                break;
            case AchievementTypes.AchievementType.Trophies:
                SetAchievementPanel(GetTrophies());
                break;
            case AchievementTypes.AchievementType.EnemyKills:
                SetAchievementPanel(PlayerStatType.EnemyKills);
                break;
            case AchievementTypes.AchievementType.DeathByFall:
                SetAchievementPanel(PlayerStatType.DeathByFall);
                break;
            case AchievementTypes.AchievementType.TreesChopped:
                SetAchievementPanel(PlayerStatType.Tree);
                break;
            case AchievementTypes.AchievementType.DeathByTree:
                SetAchievementPanel(PlayerStatType.DeathByTree);
                break;
            case AchievementTypes.AchievementType.DeathByEdgeOfWorld:
                SetAchievementPanel(PlayerStatType.DeathByEdgeOfWorld);
                break;
            case AchievementTypes.AchievementType.TimeInBase:
                SetAchievementPanel(PlayerStatType.TimeInBase);
                break;
            case AchievementTypes.AchievementType.TimeOutOfBase:
                SetAchievementPanel(PlayerStatType.TimeOutOfBase);
                break;
            case AchievementTypes.AchievementType.ArrowsShot:
                SetAchievementPanel(PlayerStatType.ArrowsShot);
                break;
            case AchievementTypes.AchievementType.TotalJumps:
                SetAchievementPanel(PlayerStatType.Jumps);
                break;
            case AchievementTypes.AchievementType.TotalBuilds:
                SetAchievementPanel(PlayerStatType.Builds);
                break;
            case AchievementTypes.AchievementType.EnemyHits:
                SetAchievementPanel(PlayerStatType.EnemyHits);
                break;
            case AchievementTypes.AchievementType.PlayerKills:
                SetAchievementPanel(PlayerStatType.PlayerKills);
                break;
            case AchievementTypes.AchievementType.HitsTaken:
                SetAchievementPanel(PlayerStatType.HitsTakenEnemies);
                break;
            case AchievementTypes.AchievementType.ItemsPicked:
                SetAchievementPanel(PlayerStatType.ItemsPickedUp);
                break;
            case AchievementTypes.AchievementType.DistanceWalked:
                SetAchievementPanel(PlayerStatType.DistanceWalk);
                break;
            case AchievementTypes.AchievementType.DistanceInAir:
                SetAchievementPanel(PlayerStatType.DistanceAir);
                break;
            case AchievementTypes.AchievementType.DistanceRan:
                SetAchievementPanel(PlayerStatType.DistanceRun);
                break;
            case AchievementTypes.AchievementType.DistanceSailed:
                SetAchievementPanel(PlayerStatType.DistanceSail);
                break;
            case AchievementTypes.AchievementType.MineHits:
                SetAchievementPanel(PlayerStatType.MineHits);
                break;
            case AchievementTypes.AchievementType.TotalMined:
                SetAchievementPanel(PlayerStatType.Mines);
                break;
            case AchievementTypes.AchievementType.CreatureTamed:
                SetAchievementPanel(PlayerStatType.CreatureTamed);
                break;
            case AchievementTypes.AchievementType.FoodEaten:
                SetAchievementPanel(PlayerStatType.FoodEaten);
                break;
            case AchievementTypes.AchievementType.SkeletonSummoned:
                SetAchievementPanel(PlayerStatType.SkeletonSummons);
                break;
            case AchievementTypes.AchievementType.DeathByDrowning:
                SetAchievementPanel(PlayerStatType.DeathByDrowning);
                break;
            case AchievementTypes.AchievementType.DeathByBurning:
                SetAchievementPanel(PlayerStatType.DeathByBurning);
                break;
            case AchievementTypes.AchievementType.DeathByFreezing:
                SetAchievementPanel(PlayerStatType.DeathByFreezing);
                break;
            case AchievementTypes.AchievementType.DeathByPoisoned:
                SetAchievementPanel(PlayerStatType.DeathByPoisoned);
                break;
            case AchievementTypes.AchievementType.DeathBySmoke:
                SetAchievementPanel(PlayerStatType.DeathBySmoke);
                break;
            case AchievementTypes.AchievementType.DeathByStalagtite:
                SetAchievementPanel(PlayerStatType.DeathByStalagtite);
                break;
            case AchievementTypes.AchievementType.BeesHarvested:
                SetAchievementPanel(PlayerStatType.BeesHarvested);
                break;
            case AchievementTypes.AchievementType.SapHarvested:
                SetAchievementPanel(PlayerStatType.SapHarvested);
                break;
            case AchievementTypes.AchievementType.TrapsArmed:
                SetAchievementPanel(PlayerStatType.TrapArmed);
                break;
            case AchievementTypes.AchievementType.RuneStones:
                int loreCount = GetKnownTextCount();
                CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(loreCount, SelectedAchievement.m_goal);
                FindAchievement().m_isCompleted = loreCount >= SelectedAchievement.m_goal;
                SelectedAchievement.m_isCompleted = loreCount >= SelectedAchievement.m_goal;
                break;
            case AchievementTypes.AchievementType.Recipes:
                int recipeCount = GetKnownRecipeCount();
                CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(recipeCount, SelectedAchievement.m_goal);
                FindAchievement().m_isCompleted = recipeCount >= SelectedAchievement.m_goal;
                SelectedAchievement.m_isCompleted = recipeCount >= SelectedAchievement.m_goal;
                break;
            case AchievementTypes.AchievementType.CustomKills:
                if (!LocalPlayerData.Player_Kill_Deaths.TryGetValue(SelectedAchievement.m_defeatKey, out KillDeaths value))
                {
                    CreateAlmanac.AchievementPanelDesc.text = "<color=red>Failed to find defeat key</color>";
                    break;
                }
                int kills = value.kills;
                CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(kills, SelectedAchievement.m_goal);
                FindAchievement().m_isCompleted = kills >= SelectedAchievement.m_goal;
                SelectedAchievement.m_isCompleted = kills >= SelectedAchievement.m_goal;
                break;
            case AchievementTypes.AchievementType.MeadowCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Meadows));
                break;
            case AchievementTypes.AchievementType.BlackForestCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.BlackForest));
                break;
            case AchievementTypes.AchievementType.SwampCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Swamp));
                break;
            case AchievementTypes.AchievementType.MountainCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Mountain));
                break;
            case AchievementTypes.AchievementType.PlainsCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Plains));
                break;
            case AchievementTypes.AchievementType.MistLandCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Mistlands));
                break;
            case AchievementTypes.AchievementType.DeepNorthCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.DeepNorth));
                break;
            case AchievementTypes.AchievementType.AshLandCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.AshLands));
                break;
            case AchievementTypes.AchievementType.OceanCreatures:
                SetAchievementPanel(CreatureLists.GetBiomeCreatures(Heightmap.Biome.Ocean));
                break;
            default:
                CreateAlmanac.AchievementPanelDesc.text = SelectedAchievement.m_desc;
                break;
        }
    }
    private static void SetAchievementPanel(List<CreatureData> list)
    {
        ZoneSystem? Zone = ZoneSystem.instance;
        if (!Zone) return;
        List<string> globalKeys = Zone.GetGlobalKeys();
        int count = list.Count(critter => globalKeys.Contains(critter.defeatedKey) || Zone.GetGlobalKey(critter.defeatedKey));
        CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(count, list.Count);
        FindAchievement().m_isCompleted = count >= list.Count;
        SelectedAchievement.m_isCompleted = count >= list.Count;
    }
    private static void SetAchievementPanel(List<ItemDrop> list)
    {
        int count = list.FindAll(food => Player.m_localPlayer.IsKnownMaterial(food.m_itemData.m_shared.m_name)).Count;
        int total = list.Count;
        CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(count, total);
        FindAchievement().m_isCompleted = count >= total;
        SelectedAchievement.m_isCompleted = count >= total;
    }
    private static void SetAchievementPanel(PlayerStatType type)
    {
        int count = (int)GetPlayerStat(type);
        CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(count, SelectedAchievement.m_goal);
        FindAchievement().m_isCompleted = count >= SelectedAchievement.m_goal;
        SelectedAchievement.m_isCompleted = count >= SelectedAchievement.m_goal;
    }
    private static string FormatProgressText(int value, int goal) => $"<color=orange>{value}</color> / <color=orange>{goal}</color> (<color=orange>{((value / goal) * 100):0.0)}</color>%)";
    private static Achievement FindAchievement() => AchievementList.Find(item => item.m_uniqueName == SelectedAchievement.m_uniqueName);
    private static void UpdateItemPanel()
    {
        DestroyPanelElements();
        if (!SelectedItemDrop) return;
        
        ItemDrop.ItemData itemData = SelectedItemDrop.m_itemData;
        CreateAlmanac.PanelIcon.sprite = itemData.GetIcon();
        CreateAlmanac.PanelTitle.text = Localization.instance.Localize(itemData.m_shared.m_name);

        Dictionary<string, string> PanelData = GetItemData(itemData);

        foreach (KeyValuePair<string, string> kvp in PanelData)
        {
            if (IsValueToBeIgnored(kvp.Value)) continue;

            if (kvp.Value == "title")
            {
                GameObject data = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!data.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) continue;

                component.text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                
                PanelElements.Add(data);
            }
            else
            {
                GameObject data = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                if (!Utils.FindChild(data.transform, "$part_infoType").TryGetComponent(out TextMeshProUGUI TypeComponent)) continue;
                if (!Utils.FindChild(data.transform, "$part_data").GetComponent<TextMeshProUGUI>().TryGetComponent(out TextMeshProUGUI DataComponent)) continue;
                
                TypeComponent.text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                DataComponent.text = Localization.instance.Localize(kvp.Value);
    
                PanelElements.Add(data);
            }
        }
    }
    private static void UpdatePiecePanel()
    {
        DestroyPanelElements();
        if (!SelectedPiece) return;
        if (!SelectedPiece.TryGetComponent(out Piece piece)) return;
        CreateAlmanac.PanelIcon.sprite = piece.m_icon;
        CreateAlmanac.PanelTitle.text = Localization.instance.Localize(piece.m_name);
        
        AddPiecesResources();

        Dictionary<string, string> PanelData = GetPieceData();

        foreach (KeyValuePair<string, string> kvp in PanelData)
        {
            if (IsValueToBeIgnored(kvp.Value)) continue;

            if (kvp.Value == "title")
            {
                GameObject data = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!data.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) return;

                component.text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                
                PanelElements.Add(data);
            }
            else
            {
                GameObject data = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                if (!Utils.FindChild(data.transform, "$part_infoType").TryGetComponent(out TextMeshProUGUI TypeComponent)) continue;
                if (!Utils.FindChild(data.transform, "$part_data").GetComponent<TextMeshProUGUI>().TryGetComponent(out TextMeshProUGUI DataComponent)) continue;
                
                TypeComponent.text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                DataComponent.text = Localization.instance.Localize(kvp.Value);

                PanelElements.Add(data);
            }
        }
    }
    private static Dictionary<string, string> GetPieceData()
    {
        Dictionary<string, string> defaultData = new();
        
        if (SelectedPiece.TryGetComponent(out Piece piece))
        {
            Dictionary<string, string> pieceData = new()
            {
                {"$almanac_enabled", ConvertBoolean(piece.enabled)},
                {"$almanac_piece_category", SplitCamelCase(piece.m_category.ToString())},
                {"$almanac_is_upgrade", ConvertBoolean(piece.m_isUpgrade)},
                {"$almanac_comfort", piece.m_comfort.ToString()},
                {"$almanac_comfort_group", SplitCamelCase(piece.m_comfortGroup.ToString())},
                {"$almanac_comfort_object", piece.m_comfortObject ? piece.m_comfortObject.name : "0"},
                {"$almanac_ground_piece", ConvertBoolean(piece.m_groundPiece)},
                {"$almanac_allow_alt_ground", ConvertBoolean(piece.m_allowAltGroundPlacement)},
                {"$almanac_ground_only", ConvertBoolean(piece.m_groundOnly)},
                {"$almanac_cultivated_only", ConvertBoolean(piece.m_cultivatedGroundOnly)},
                {"$almanac_water_piece", ConvertBoolean(piece.m_waterPiece)},
                {"$almanac_clip_ground", ConvertBoolean(piece.m_clipGround)},
                {"$almanac_clip_everything", ConvertBoolean(piece.m_clipEverything)},
                {"$almanac_no_in_water", ConvertBoolean(piece.m_noInWater)},
                {"$almanac_no_on_wood", ConvertBoolean(piece.m_notOnWood)},
                {"$almanac_no_on_tilt", ConvertBoolean(piece.m_notOnTiltingSurface)},
                {"$almanac_ceiling_only", ConvertBoolean(piece.m_inCeilingOnly)},
                {"$almanac_no_on_floor", ConvertBoolean(piece.m_notOnFloor)},
                {"$almanac_no_clipping", ConvertBoolean(piece.m_notOnFloor)},
                {"$almanac_only_in_teleport_area", ConvertBoolean(piece.m_onlyInTeleportArea)},
                {"$almanac_allow_dungeon", ConvertBoolean(piece.m_allowedInDungeons)},
                {"$almanac_space_req", piece.m_spaceRequirement.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_repair_piece", ConvertBoolean(piece.m_repairPiece)},
                {"$almanac_can_rotate", ConvertBoolean(piece.m_canRotate)},
                {"$almanac_random_rotate", ConvertBoolean(piece.m_randomInitBuildRotation)},
                {"$almanac_allow_rotate_overlap", ConvertBoolean(piece.m_allowRotatedOverlap)},
                {"$almanac_vegetation_ground_only", ConvertBoolean(piece.m_vegetationGroundOnly)},
            };
            
            if (_ShowAllData.Value is AlmanacPlugin.Toggle.On) pieceData.Add("$almanac_prefab_name", piece.name);

            if (piece.m_blockingPieces.Count > 0)
            {
                for (int index = 0; index < piece.m_blockingPieces.Count; index++)
                {
                    Piece block = piece.m_blockingPieces[index];
                    pieceData.Add("$almanac_block_piece" + index, block.m_name);
                }
            }

            Dictionary<string, string> pieceData1 = new()
            {
                {"$almanac_block_radius", piece.m_blockRadius.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_must_connect_to", piece.m_mustConnectTo ? piece.m_mustConnectTo.ToString() : "0"},
                {"$almanac_connect_radius", piece.m_connectRadius.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_must_be_above", ConvertBoolean(piece.m_mustBeAboveConnected)},
                {"$almanac_piece_biome", SplitCamelCase(piece.m_onlyInBiome.ToString())},
                {"$almanac_dlc", piece.m_dlc},
                {"$almanac_craft_station", piece.m_craftingStation ? Localization.instance.Localize(piece.m_craftingStation.m_name) : "0"}
            };
            
            MergeDictionaries(pieceData, pieceData1);
            MergeDictionaries(defaultData,pieceData);
        }

        if (SelectedPiece.TryGetComponent(out CraftingStation craftingStation))
        {
            Dictionary<string, string> craftingData = new()
            {
                {"$almanac_crafting_station_title", "title"},
                {"$almanac_discover_range", craftingStation.m_discoverRange.ToString("0.0")},
                {"$almanac_range_build", craftingStation.m_rangeBuild.ToString("0.0")},
                {"$almanac_extra_range_per_level", craftingStation.m_extraRangePerLevel.ToString("0.0")},
                {"$almanac_require_roof1", ConvertBoolean(craftingStation.m_craftRequireRoof)},
                {"$almanac_require_fire", ConvertBoolean(craftingStation.m_craftRequireFire)},
                {"$almanac_show_basic_recipes", ConvertBoolean(craftingStation.m_showBasicRecipies)},
                {"$almanac_use_distance", craftingStation.m_useDistance.ToString("0.0")},
                {"$almanac_use_animation", craftingStation.m_useAnimation.ToString()},
            };
            
            MergeDictionaries(defaultData, craftingData);
        }

        if (SelectedPiece.TryGetComponent(out StationExtension stationExtension))
        {
            Dictionary<string, string> extensionData = new()
            {
                {"$almanac_extension_title", "title"},
                {"$almanac_piece_extends", stationExtension.m_craftingStation ? Localization.instance.Localize(stationExtension.m_craftingStation.m_name) : "0"},
                {"$almanac_extension_distance", stationExtension.m_maxStationDistance.ToString("0.0")},
                {"$almanac_extension_stack", stationExtension.m_stack.ToString()},
                {"$almanac_continuous_connection", ConvertBoolean(stationExtension.m_continousConnection)}
            };
            MergeDictionaries(defaultData, extensionData);
        }

        if (SelectedPiece.TryGetComponent(out WearNTear wearNTear))
        {
            Dictionary<string, string> wearData = new()
            {
                {"$almanac_wear_tear_title", "title"},
                {"$almanac_no_roof_wear", ConvertBoolean(wearNTear.m_noRoofWear).ToString()},
                {"$almanac_no_support_wear", ConvertBoolean(wearNTear.m_noSupportWear).ToString()},
                {"$almanac_material_type", SplitCamelCase(wearNTear.m_materialType.ToString())},
                {"$almanac_piece_supports", ConvertBoolean(wearNTear.m_supports).ToString()},
                {"$almanac_support_value", wearNTear.m_support.ToString("0.0")},
                {"$almanac_piece_health", wearNTear.m_health.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_blunt", ConvertDamageModifiers(wearNTear.m_damages.m_blunt)},
                {"$almanac_slash", ConvertDamageModifiers(wearNTear.m_damages.m_slash)},
                {"$almanac_pierce", ConvertDamageModifiers(wearNTear.m_damages.m_pierce)},
                {"$almanac_chop", ConvertDamageModifiers(wearNTear.m_damages.m_chop)},
                {"$almanac_pickaxe", ConvertDamageModifiers(wearNTear.m_damages.m_pickaxe)},
                {"$almanac_fire",ConvertDamageModifiers(wearNTear.m_damages.m_fire)},
                {"$almanac_frost",ConvertDamageModifiers(wearNTear.m_damages.m_frost)},
                {"$almanac_lightning", ConvertDamageModifiers(wearNTear.m_damages.m_lightning)},
                {"$almanac_poison", ConvertDamageModifiers(wearNTear.m_damages.m_poison)},
                {"$almanac_spirit", ConvertDamageModifiers(wearNTear.m_damages.m_spirit)},
                {"$almanac_hit_noise", wearNTear.m_hitNoise.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_destroy_noise", wearNTear.m_destroyNoise.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_trigger_private_area", ConvertBoolean(wearNTear.m_triggerPrivateArea)},
            };
            
            MergeDictionaries(defaultData, wearData);
        }

        if (SelectedPiece.TryGetComponent(out Smelter smelter))
        {
            Dictionary<string, string> smelterData = new()
            {
                {"$almanac_smelter_title", "title"},
                {"$almanac_fuel_item", smelter.m_fuelItem ? Localization.instance.Localize(smelter.m_fuelItem.m_itemData.m_shared.m_name) : "0"},
                {"$almanac_max_ore", smelter.m_maxOre.ToString()},
                {"$almanac_max_fuel", smelter.m_maxFuel.ToString()},
                {"$almanac_fuel_per_product", smelter.m_fuelPerProduct.ToString()},
                {"$almanac_sec_per_product", smelter.m_secPerProduct.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_spawn_stack", smelter.m_spawnStack.ToString()},
                {"$almanac_require_roof2", ConvertBoolean(smelter.m_requiresRoof)},
                {"$almanac_add_ore_duration", smelter.m_addOreAnimationDuration.ToString(CultureInfo.CurrentCulture)},
            };

            if (smelter.m_conversion.Count > 0)
            {
                Dictionary<string, string> conversionData = new()
                {
                    { "$almanac_conversion_title", "title" }
                };
                foreach (Smelter.ItemConversion conversion in smelter.m_conversion)
                {
                    string from = Localization.instance.Localize(conversion.m_from.m_itemData.m_shared.m_name);
                    string to = Localization.instance.Localize(conversion.m_to.m_itemData.m_shared.m_name);
                    conversionData.Add($"<color=white>{from}</color>", to);
                }
                
                MergeDictionaries(smelterData, conversionData);
            }
            
            MergeDictionaries(defaultData, smelterData);
        }

        if (SelectedPiece.TryGetComponent(out Beehive beehive))
        {
            Dictionary<string, string> beeHiveData = new()
            {
                {"$almanac_beehive_title", "title"},
                {"$almanac_effect_only_day", beehive.m_effectOnlyInDaylight.ToString()},
                {"$almanac_max_cover", beehive.m_maxCover.ToString("0.0")},
                {"$almanac_sec_per_unit", beehive.m_secPerUnit.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_max_honey", beehive.m_maxHoney.ToString()},
                {"$almanac_honey_item", beehive.m_honeyItem ? Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name) : "0"},
            };

            string[] biomes = beehive.m_biome.ToString().Split(',');
            for (int index = 0; index < biomes.Length; index++)
            {
                string biome = biomes[index].Replace(" ", "");
                beeHiveData.Add("$almanac_biomes" + index, SplitCamelCase(biome));
            }

            MergeDictionaries(defaultData, beeHiveData);
        }

        if (SelectedPiece.TryGetComponent(out Container container))
        {
            Dictionary<string, string> containerData = new()
            {
                {"$almanac_container_title", "title"},
                {"$almanac_container_size", container.m_width + "<color=orange>x</color>" + container.m_height},
                {"$almanac_container_privacy", container.m_privacy.ToString()},
                {"$almanac_check_guard", ConvertBoolean(container.m_checkGuardStone)},
                {"$almanac_auto_destroy_empty", ConvertBoolean(container.m_autoDestroyEmpty)},
            };

            if (container.m_defaultItems.m_drops.Count > 0)
            {
                containerData.Add("$almanac_container_drop", container.m_defaultItems.m_dropMin + "<color=orange>-</color>" + container.m_defaultItems.m_dropMax);
                containerData.Add("$almanac_container_drop_chance", (container.m_defaultItems.m_dropChance * 100).ToString("0.0") + "%");
                containerData.Add("$almanac_container_one_of_each", container.m_defaultItems.m_oneOfEach.ToString());
                
                float totalWeight = container.m_defaultItems.m_drops.Sum(item => item.m_weight);;
                for (int index = 0; index < container.m_defaultItems.m_drops.Count; index++)
                {
                    DropTable.DropData drop = container.m_defaultItems.m_drops[index];
                    if (!drop.m_item) continue;
                    if (!drop.m_item.TryGetComponent(out ItemDrop itemDrop)) continue;
                    string name = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name);
                    string stack = $"<color=orange>(</color>{drop.m_stackMin}<color=orange> $almanac_to </color>{drop.m_stackMax}<color=orange>)</color> ";
                    float percentage = (drop.m_weight / totalWeight) * 100;
                    containerData.Add(name + index, stack + percentage.ToString("0.0") + "<color=orange>%</color>");
                }
            }
            MergeDictionaries(defaultData, containerData);
        }

        if (SelectedPiece.TryGetComponent(out CookingStation cookingStation))
        {
            Dictionary<string, string> cookingData = new()
            {
                {"$almanac_spawn_force", cookingStation.m_spawnForce.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_overcooked_item", cookingStation.m_overCookedItem ? Localization.instance.Localize(cookingStation.m_overCookedItem.m_itemData.m_shared.m_name) : "0"},
                {"$almanac_require_fire", ConvertBoolean(cookingStation.m_requireFire)},
                {"$almanac_check_radius", cookingStation.m_fireCheckRadius.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_use_fuel", ConvertBoolean(cookingStation.m_useFuel)},
                {"$almanac_fuel_item", cookingStation.m_fuelItem ? Localization.instance.Localize(cookingStation.m_fuelItem.m_itemData.m_shared.m_name) : "0"},
                {"$almanac_max_fuel", cookingStation.m_maxFuel.ToString()},
                {"$almanac_sec_per_fuel", cookingStation.m_secPerFuel.ToString()},
            };

            if (cookingStation.m_conversion.Count > 0)
            {
                cookingData.Add("$almanac_conversion_title", "title");
                foreach (var item in cookingStation.m_conversion)
                {
                    string from = Localization.instance.Localize(item.m_from.m_itemData.m_shared.m_name);
                    string to = Localization.instance.Localize(item.m_to.m_itemData.m_shared.m_name);
                    string cookTime = " <color=orange>(</color>" + item.m_cookTime + "<color=orange>s)</color>";
                    
                    cookingData.Add(from, to + cookTime);
                }
            }
            
            MergeDictionaries(defaultData, cookingData);
        }

        if (SelectedPiece.TryGetComponent(out TeleportWorld teleportWorld))
        {
            Dictionary<string, string> teleportData = new()
            {
                {"$almanac_portal_title", "title"},
                {"$almanac_activation_range", teleportWorld.m_activationRange.ToString("0.0")},
                {"$almanac_exit_distance", teleportWorld.m_exitDistance.ToString("0.0")}
            };
            
            MergeDictionaries(defaultData, teleportData);
        }

        if (SelectedPiece.TryGetComponent(out Ship ship))
        {
            Dictionary<string, string> shipData = new()
            {
                {"$almanac_ship_title", "title"},
                {"$almanac_water_level_offset", ship.m_waterLevelOffset.ToString("0.0")},
                {"$almanac_force_distance", ship.m_forceDistance.ToString("0.0")},
                {"$almanac_force", ship.m_force.ToString("0.0")},
                {"$almanac_damping", ship.m_damping.ToString("0.0")},
                {"$almanac_damping_sideway", ship.m_dampingSideway.ToString("0.0")},
                {"$almanac_damping_forward", ship.m_dampingForward.ToString("0.0")},
                {"$almanac_angular_damping", ship.m_angularDamping.ToString("0.0")},
                {"$almanac_disable_level", ship.m_disableLevel.ToString("0.0")},
                {"$almanac_sail_force_offset", ship.m_sailForceOffset.ToString("0.0")},
                {"$almanac_sail_force_factor", ship.m_sailForceFactor.ToString("0.0")},
                {"$almanac_rudder_speed", ship.m_rudderSpeed.ToString("0.0")},
                {"$almanac_stear_force_offset", ship.m_stearForceOffset.ToString("0.0")},
                {"$almanac_stear_force", ship.m_stearForce.ToString("0.0")},
                {"$almanac_stear_velocity_force_factor", ship.m_stearVelForceFactor.ToString("0.0")},
                {"$almanac_backward_force", ship.m_backwardForce.ToString("0.0")},
                {"$almanac_rudder_rotation_max", ship.m_rudderRotationMax.ToString("0.0")},
                {"$almanac_min_water_impact_force", ship.m_minWaterImpactForce.ToString("0.0")},
                {"$almanac_min_water_impact_interval", ship.m_minWaterImpactInterval.ToString("0.0") + "<color=orange>s</color>"},
                {"$almanac_upside_down_damage_interval", ship.m_upsideDownDmgInterval.ToString("0.0") + "<color=orange>s</color>"},
                {"$almanac_upside_down_damage", ship.m_upsideDownDmg.ToString("0.0")}
            };
            
            MergeDictionaries(defaultData, shipData);
        }

        if (SelectedPiece.TryGetComponent(out WispSpawner wispSpawner))
        {
            Dictionary<string, string> wispData = new()
            {
                {"$almanac_wisp_spawner_title", "title"},
                {"$almanac_spawn_interval", wispSpawner.m_spawnInterval.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_spawn_chance", (wispSpawner.m_spawnChance * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"},
                {"$almanac_max_spawned", wispSpawner.m_maxSpawned.ToString()},
                {"$almanac_only_spawn_night", ConvertBoolean(wispSpawner.m_onlySpawnAtNight)},
                {"$almanac_no_spawn_cover", ConvertBoolean(wispSpawner.m_dontSpawnInCover)},
                {"$almanac_max_cover", (wispSpawner.m_maxCover * 100).ToString("0.0")},
                {"$almanac_wisp_prefab", wispSpawner.m_wispPrefab ? wispSpawner.m_wispPrefab.name : "0"},
                {"$almanac_nearby_threshold", wispSpawner.m_nearbyTreshold.ToString("0.0")},
                {"$almanac_spawn_distance", wispSpawner.m_spawnDistance.ToString("0.0")},
                {"$almanac_max_spawned_area", wispSpawner.m_maxSpawnedArea.ToString(CultureInfo.CurrentCulture)}
            };
            
            MergeDictionaries(defaultData, wispData);
        }

        if (SelectedPiece.TryGetComponent(out Trap trap))
        {
            Dictionary<string, string> trapData = new()
            {
                {"$almanac_trap_title", "title"},
                {"$almanac_rearm_cooldown", trap.m_rearmCooldown + "<color=orange>s</color>"},
                {"$almanac_triggered_by_enemies", ConvertBoolean(trap.m_triggeredByEnemies)},
                {"$almanac_triggered_by_players", ConvertBoolean(trap.m_triggeredByPlayers)},
                {"$almanac_force_stagger", ConvertBoolean(trap.m_forceStagger)},
                {"$almanac_starts_armed", ConvertBoolean(trap.m_startsArmed)}
            };
            
            MergeDictionaries(defaultData,trapData);
        }

        if (SelectedPiece.TryGetComponent(out Fireplace fireplace))
        {
            Dictionary<string, string> fireplaceData = new()
            {
                {"$almanac_fireplace_title", "title"},
                {"$almanac_start_fuel", fireplace.m_startFuel.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_max_fuel", fireplace.m_maxFuel.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_sec_per_fuel", fireplace.m_secPerFuel.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_infinite_fuel", ConvertBoolean(fireplace.m_infiniteFuel)},
                {"$almanac_fuel_item", fireplace.m_fuelItem ? Localization.instance.Localize(fireplace.m_fuelItem.m_itemData.m_shared.m_name) : "0"},
            };

            if (fireplace.m_fireworkItemList.Length > 0)
            {
                for (int index = 0; index < fireplace.m_fireworkItemList.Length; index++)
                {
                    Fireplace.FireworkItem item = fireplace.m_fireworkItemList[index];
                    if (item.m_fireworkItem) fireplaceData.Add("$almanac_firework" + index, Localization.instance.Localize(item.m_fireworkItem.m_itemData.m_shared.m_name));
                }
            }
            
            MergeDictionaries(defaultData,fireplaceData);
        }

        if (SelectedPiece.TryGetComponent(out Door door))
        {
            Dictionary<string, string> doorData = new()
            {
                {"$almanac_door_title", "title"},
                {"$almanac_key_item", door.m_keyItem ? Localization.instance.Localize(door.m_keyItem.m_itemData.m_shared.m_name) : "0"},
                {"$almanac_can_not_be_closed", ConvertBoolean(door.m_canNotBeClosed)},
                {"$almanac_check_guard", ConvertBoolean(door.m_checkGuardStone)}
            };
            
            MergeDictionaries(defaultData, doorData);
        }

        if (SelectedPiece.TryGetComponent(out Turret turret))
        {
            Dictionary<string, string> turretData = new()
            {
                {"$almanac_turret_title", "title"},
                {"$almanac_turn_rate", turret.m_turnRate.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_horizontal_angle", turret.m_horizontalAngle.ToString(CultureInfo.CurrentCulture) + "<color=orange>°</color>"},
                {"$almanac_vertical_angle", turret.m_verticalAngle.ToString(CultureInfo.CurrentCulture) + "<color=orange>°</color>"},
                {"$almanac_view_distance", turret.m_viewDistance.ToString("0.0")},
                {"$almanac_no_target_scan_rate", turret.m_noTargetScanRate.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_look_acceleration", turret.m_lookAcceleration.ToString("0.0") + "<color=orange>s</color>"},
                {"$almanac_look_deacceleration", turret.m_lookDeacceleration.ToString("0.0") + "<color=orange>s</color>"},
                {"$almanac_look_min_degrees_delta", turret.m_lookMinDegreesDelta.ToString("0.0") + "<color=orange>°</color>"},
                {"$almanac_default_ammo", turret.m_defaultAmmo ? Localization.instance.Localize(turret.m_defaultAmmo.m_itemData.m_shared.m_name) : "0"},
                {"$almanac_attack_cooldown", turret.m_attackCooldown.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_attack_warmup", turret.m_attackWarmup.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_hit_noise1", turret.m_hitNoise.ToString("0.0")},
                {"$almanac_shoot_when_aim_diff", turret.m_shootWhenAimDiff.ToString("0.0")},
                {"$almanac_prediction_modifier", turret.m_predictionModifier.ToString("0.0")},
                {"$almanac_update_target_interval_far", turret.m_updateTargetIntervalFar.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_update_target_interval_near", turret.m_updateTargetIntervalNear.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_max_ammo", turret.m_maxAmmo.ToString()},
                {"$almanac_ammo_type", Localization.instance.Localize(turret.m_ammoType)},
                {"$almanac_return_ammo_on_destroy", ConvertBoolean(turret.m_returnAmmoOnDestroy)},
                {"$almanac_hold_repeat_interval", turret.m_holdRepeatInterval.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_target_players", ConvertBoolean(turret.m_targetPlayers)},
                {"$almanac_target_tamed", ConvertBoolean(turret.m_targetTamed)}
            };

            if (turret.m_allowedAmmo.Count > 0)
            {
                for (int index = 0; index < turret.m_allowedAmmo.Count; index++)
                {
                    Turret.AmmoType ammo = turret.m_allowedAmmo[index];
                    if (ammo.m_ammo) turretData.Add("$almanac_ammo" + index, Localization.instance.Localize(ammo.m_ammo.m_itemData.m_shared.m_name));
                }
            }
            
            MergeDictionaries(defaultData, turretData);
        }

        if (SelectedPiece.TryGetComponent(out Fermenter ferment))
        {
            Dictionary<string, string> fermentData = new()
            {
                {"$almanac_fermenter_title", "title"},
                {"$almanac_duration", ferment.m_fermentationDuration.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                {"$almanac_tap_delay", ferment.m_tapDelay.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
            };

            if (ferment.m_conversion.Count > 0)
            {
                for (int index = 0; index < ferment.m_conversion.Count; ++index)
                {
                    Fermenter.ItemConversion conversion = ferment.m_conversion[index];
                    string from = Localization.instance.Localize(conversion.m_from.m_itemData.m_shared.m_name);
                    string to = Localization.instance.Localize(conversion.m_to.m_itemData.m_shared.m_name);
                    fermentData.Add($"<color=white>{from}</color>", to + " <color=orange>(x</color>" + conversion.m_producedItems + "<color=orange>)</color>");
                }
            }
            
            MergeDictionaries(defaultData,fermentData);
        }

        return defaultData;
    }
    private static void DestroyPanelElements()
    {
        foreach (GameObject element in PanelElements) Object.Destroy(element);
        PanelElements.Clear();
    }
    private static void UpdateCreaturePanel()
    {
        DestroyPanelElements();

        CreateAlmanac.PanelIcon.sprite = SelectedCreature.trophyImage ? SelectedCreature.trophyImage : SpriteManager.AlmanacIcon;
        CreateAlmanac.PanelTitle.text = Localization.instance.Localize(SelectedCreature.display_name);

        Dictionary<string, string> CreatureData = GetCreatureData();

        foreach (KeyValuePair<string, string> kvp in CreatureData)
        {
            if (IsValueToBeIgnored(kvp.Value)) continue;

            if (kvp.Value == "title")
            {
                GameObject item = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!item.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) return;

                component.text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                
                PanelElements.Add(item);
            }
            else
            {
                GameObject item = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                Utils.FindChild(item.transform, "$part_infoType").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                Utils.FindChild(item.transform, "$part_data").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(kvp.Value);
                
                PanelElements.Add(item);
            }
        }
        
        AddCreatureDrops();

        Dictionary<string, string> CreatureData1 = GetCreatureData1();
        foreach (KeyValuePair<string, string> kvp in CreatureData1)
        {
            if (IsValueToBeIgnored(kvp.Value)) continue;
            if (kvp.Value == "title")
            {
                GameObject item = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!item.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) return;

                component.text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                
                PanelElements.Add(item);
            }
            else
            {
                GameObject item = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                Utils.FindChild(item.transform, "$part_infoType").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(RemoveNumbers(kvp.Key));
                Utils.FindChild(item.transform, "$part_data").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(kvp.Value);
                
                PanelElements.Add(item);
            }
        }
        
        AddCreatureConsumeItems();
    }
    private static Dictionary<string, string> GetCreatureData()
    {
        Dictionary<string, string> defaultData = new()
        {
            {"$almanac_health", SelectedCreature.health.ToString(CultureInfo.CurrentCulture)},
            {"$almanac_faction", SplitCamelCase(SelectedCreature.faction)},
            {"$almanac_creature_resistances", "title"},
            {"$almanac_blunt", SplitCamelCase(SelectedCreature.blunt)},
            {"$almanac_slash", SplitCamelCase(SelectedCreature.slash)},
            {"$almanac_pierce", SplitCamelCase(SelectedCreature.pierce)},
            {"$almanac_chop", SplitCamelCase(SelectedCreature.chop)},
            {"$almanac_pickaxe", SplitCamelCase(SelectedCreature.pickaxe)},
            {"$almanac_fire", SplitCamelCase(SelectedCreature.fire)},
            {"$almanac_frost", SplitCamelCase(SelectedCreature.frost)},
            {"$almanac_lightning", SplitCamelCase(SelectedCreature.lightning)},
            {"$almanac_poison", SplitCamelCase(SelectedCreature.poison)},
            {"$almanac_spirit", SplitCamelCase(SelectedCreature.spirit)},
        };

        Dictionary<string, string> TrackedData = new()
        {
            {"$almanac_kill_death_title", "title"},
            {"$almanac_kill_count", LocalPlayerData.Player_Kill_Deaths[SelectedCreature.defeatedKey].kills.ToString()},
            {"$almanac_death_count", LocalPlayerData.Player_Kill_Deaths[SelectedCreature.defeatedKey].deaths.ToString()},
        };
        
        int kills = LocalPlayerData.Player_Kill_Deaths[SelectedCreature.defeatedKey].kills;
        int deaths = LocalPlayerData.Player_Kill_Deaths[SelectedCreature.defeatedKey].deaths;

        if (kills != 0 && deaths != 0)
        {
            // ReSharper disable once PossibleLossOfFraction
            float KD = Mathf.Floor(kills / deaths);

            TrackedData.Add("$almanac_kill_death_ratio", KD.ToString(CultureInfo.CurrentCulture));
        }

        if (kills + deaths > 0)
        {
            MergeDictionaries(defaultData, TrackedData);
        }

        return defaultData;
    }
    private static void AddPiecesResources()
    {
        if (!SelectedPiece.TryGetComponent(out Piece piece)) return;
        int resourcesRow =  Mathf.FloorToInt(piece.m_resources.Length / 8f) + 1;
        if (piece.m_resources.Length != 0)
        {
            for (int index = 0; index < resourcesRow; ++index)
            {
                GameObject container = Object.Instantiate(CacheAssets.Drops, CreateAlmanac.PanelContent);
                Transform title = container.transform.Find("$part_title");
                if (!title.TryGetComponent(out TextMeshProUGUI component)) continue;
                component.text = Localization.instance.Localize("$almanac_resources");
                PanelElements.Add(container);
                int j = 0;
                for (int i = index * 7; i < (index + 1) * 7; ++i)
                {
                    if (i >= piece.m_resources.Length) break;
                    Transform part = Utils.FindChild(container.transform, $"$part_drop_{j}");
                    Transform name = part.GetChild(0);
                
                    if (!part.TryGetComponent(out Image image)) continue;
                    if (!name.TryGetComponent(out TextMeshProUGUI text)) continue;
                    
                    Piece.Requirement resource = piece.m_resources[i];

                    bool isKnown = Player.m_localPlayer.IsKnownMaterial(resource.m_resItem.m_itemData.m_shared.m_name) || _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.NoCostCheat();

                    image.sprite = resource.m_resItem.m_itemData.GetIcon();
                    image.color = isKnown ? Color.white : Color.black;
                    text.text = isKnown ? ReplaceSpaceWithNewLine(Localization.instance.Localize(resource.m_resItem.m_itemData.m_shared.m_name))
                                + $"\n<color=orange>{resource.m_amount}</color>" : UnknownText;
                
                    part.gameObject.SetActive(true);
                    ++j;
                }
            }
        }
    }
    private static void AddCreatureDrops()
    {
        int dropContainerCount =  Mathf.FloorToInt(SelectedCreature.drops.Count / 8f) + 1;
        if (SelectedCreature.drops.Count != 0)
        {
            for (int index = 0; index < dropContainerCount; ++index)
            {
                GameObject dropContainer = Object.Instantiate(CacheAssets.Drops, CreateAlmanac.PanelContent);
                PanelElements.Add(dropContainer);
                int j = 0;
                for (int i = index * 7; i < (index + 1) * 7; ++i)
                {
                    if (i >= SelectedCreature.drops.Count) break;
                    Transform part = Utils.FindChild(dropContainer.transform, $"$part_drop_{j}");
                    Transform name = part.GetChild(0);
                
                    if (!part.TryGetComponent(out Image image)) continue;
                    if (!name.TryGetComponent(out TextMeshProUGUI text)) continue;
                    
                    string drop = SelectedCreature.drops[i];
                    float dropChance = SelectedCreature.dropChance[SelectedCreature.drops[i]];
                    
                    GameObject dropPrefab = ObjectDB.instance.GetItemPrefab(drop);
                    if (!dropPrefab) continue;
                    if (!dropPrefab.TryGetComponent(out ItemDrop itemDrop)) continue;

                    bool isKnown = Player.m_localPlayer.IsKnownMaterial(itemDrop.m_itemData.m_shared.m_name) || _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.NoCostCheat();

                    image.sprite = itemDrop.m_itemData.GetIcon();
                    image.color = isKnown ? Color.white : Color.black;
                    text.text = isKnown ? ReplaceSpaceWithNewLine(Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name))
                                + $"\n(<color=orange>{dropChance}%</color>)" : UnknownText;
                
                    part.gameObject.SetActive(true);
                    ++j;
                }
            }
        }
    }
    private static void AddCreatureConsumeItems()
    {
        int consumeItemCount =  Mathf.FloorToInt(SelectedCreature.consumeItems.Count / 8f) + 1;
        if (SelectedCreature.consumeItems.Count != 0)
        {
            for (int index = 0; index < consumeItemCount; ++index)
            {
                GameObject dropContainer = Object.Instantiate(CacheAssets.Drops, CreateAlmanac.PanelContent);
                Transform title = dropContainer.transform.Find("$part_title");
                if (!title.TryGetComponent(out TextMeshProUGUI component)) continue;
                component.text = Localization.instance.Localize("$almanac_taming_items");
                if (index > 0) title.gameObject.SetActive(false);
                PanelElements.Add(dropContainer);
            
                for (int i = index * 7; i < (index + 1) * 7; ++i)
                {
                    if (i >= SelectedCreature.consumeItems.Count) break;
                    Transform part = Utils.FindChild(dropContainer.transform, $"$part_drop_{i}");
                    Transform name = part.GetChild(0);
                
                    if (!part.TryGetComponent(out Image image)) continue;
                    if (!name.TryGetComponent(out TextMeshProUGUI text)) continue;
                    
                    string item = SelectedCreature.consumeItems[i];
                    
                    GameObject dropPrefab = ObjectDB.instance.GetItemPrefab(item);
                    if (!dropPrefab) continue;
                    if (!dropPrefab.TryGetComponent(out ItemDrop itemDrop)) continue;

                    bool isKnown = Player.m_localPlayer.IsKnownMaterial(itemDrop.m_itemData.m_shared.m_name) || _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.NoCostCheat();

                    image.sprite = itemDrop.m_itemData.GetIcon();
                    image.color = isKnown ? Color.white : Color.black;
                    text.text = isKnown ? ReplaceSpaceWithNewLine(Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name)) : UnknownText;

                    part.gameObject.SetActive(true);
                }
            }
        }
    }
    private static Dictionary<string, string> GetCreatureData1()
    {
        Dictionary<string, string> defaultData = new()
        {
            {"$almanac_avoid_fire", ConvertBoolean(SelectedCreature.avoidFire)},
            {"$almanac_afraid_of_fire", ConvertBoolean(SelectedCreature.afraidOfFire)},
            {"$almanac_avoid_water", ConvertBoolean(SelectedCreature.avoidWater)},
        };

        for (int index = 0; index < SelectedCreature.weakSpot.Count; index++)
        {
            string spot = SelectedCreature.weakSpot[index];
            defaultData.Add("$almanac_weak_spot" + index, spot);
        }

        Dictionary<string, string> data = new()
        {
            {"$almanac_stagger_when_blocked", ConvertBoolean(SelectedCreature.staggerWhenBlocked)},
            {"$almanac_stagger_damage_factor", SelectedCreature.staggerDamageFactor.ToString("0.0")},
            {"$almanac_tolerate_water", ConvertBoolean(SelectedCreature.tolerateWater)},
            {"$almanac_tolerate_smoke", ConvertBoolean(SelectedCreature.tolerateSmoke)},
            {"$almanac_tolerate_tar", ConvertBoolean(SelectedCreature.tolerateTar)},
            {"$almanac_defeat_key", SelectedCreature.defeatedKey},
        };

        MergeDictionaries(defaultData, data);

        for (int index = 0; index < SelectedCreature.defaultItems.Count; index++)
        {
            AttackData AttackData = SelectedCreature.defaultItems[index];
            if (AttackData.name == "Unknown") continue;
            defaultData.Add("$almanac_creature_attacks" + index, "title");
            defaultData.Add("$almanac_attack_name" + index, AttackData.name);
            defaultData.Add("$almanac_damage" + index, AttackData.damage.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_blunt" + index, AttackData.blunt.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_slash" + index, AttackData.slash.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_pierce" + index, AttackData.pierce.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_chop" + index, AttackData.chop.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_pickaxe" + index, AttackData.pickaxe.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_fire" + index, AttackData.fire.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_frost" + index, AttackData.frost.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_lightning" + index, AttackData.lightning.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_poison" + index, AttackData.poison.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_spirit" + index, AttackData.spirit.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_force" + index, AttackData.attackForce.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_back_stab_bonus" + index, AttackData.backStabBonus.ToString(CultureInfo.CurrentCulture));
            defaultData.Add("$almanac_dodgeable" + index, ConvertBoolean(AttackData.dodgeable));
            defaultData.Add("$almanac_blockable" + index, ConvertBoolean(AttackData.blockable));
            defaultData.Add("$almanac_status_effect" + index, AttackData.statusEffect);
            defaultData.Add("$almanac_status_effect_tooltip" + index, AttackData.statusEffectTooltip);
        }

        return defaultData;
    }
    private static Dictionary<string, string> GetItemData(ItemDrop.ItemData itemData)
    {
        Dictionary<string, string> DefaultData = new()
        {
            {"$almanac_stack_size_label", itemData.m_stack.ToString()},
            {"$almanac_durability_label", itemData.m_durability.ToString(CultureInfo.CurrentCulture)},
            {"$almanac_variant_label", itemData.m_variant.ToString()},
            {"$almanac_world_level_label", itemData.m_worldLevel.ToString()},
            {"$almanac_item_type_label", ConvertItemType(itemData.m_shared.m_itemType)},
            {"$almanac_max_stack_size_label", itemData.m_shared.m_maxStackSize.ToString()},
            {"$almanac_auto_stack_label", ConvertBoolean(itemData.m_shared.m_autoStack)},
            {"$almanac_quality_label", itemData.m_shared.m_maxQuality.ToString()},
            {"$almanac_scale_by_quality", (itemData.m_shared.m_scaleByQuality * 100).ToString(CultureInfo.CurrentCulture) + "%"},
            {"$almanac_weight_label", itemData.m_shared.m_weight.ToString("0.0")},
            {"$almanac_scale_by_weight", itemData.m_shared.m_scaleWeightByQuality.ToString(CultureInfo.CurrentCulture)},
            {"$almanac_value_label", itemData.m_shared.m_value.ToString()},
            {"$almanac_teleportable", ConvertBoolean(itemData.m_shared.m_teleportable)},
            {"$almanac_quest_item_label", ConvertBoolean(itemData.m_shared.m_questItem)},
            {"$almanac_equip_duration", itemData.m_shared.m_equipDuration.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
            {"$almanac_variant_label1", itemData.m_shared.m_variants.ToString()},
        };

        AddItemRecipe(itemData);

        GameObject prefab = ObjectDB.instance.GetItemPrefab(SelectedItemDrop.name);
        if (prefab != null)
        {
            DefaultData.Add("$almanac_floating", prefab.GetComponent<Floating>() ? "$almanac_true" : "$almanac_false");
            if (_ShowAllData.Value is AlmanacPlugin.Toggle.On)
            {
                DefaultData.Add("$almanac_prefab_name", prefab.name);
            }
            
            if (itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Fish)
            {
                if (prefab.TryGetComponent(out Fish fish))
                {
                    Dictionary<string, string> fishData = new()
                    {
                        {"$almanac_fish_title", "title"},
                        {"$almanac_fish_swim_range", fish.m_swimRange.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_min_depth", fish.m_minDepth.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_speed", fish.m_speed.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_acceleration", fish.m_acceleration.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_turn_rate", fish.m_turnRate.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_avoid_range", fish.m_avoidRange.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_height", fish.m_height.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_hook_force", fish.m_hookForce.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_stamina_use", fish.m_staminaUse.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_escape_stamina_use", fish.m_escapeStaminaUse.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_escape_min", fish.m_escapeMin.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_escape_max", fish.m_escapeMax.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_base_hook_chance", fish.m_baseHookChance.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_escape_time", fish.m_escapeTime.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_jump_speed", fish.m_jumpSpeed.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_jump_height", fish.m_jumpHeight.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_jump_on_land_chance", fish.m_jumpOnLandChance.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_jump_on_land_decay", fish.m_jumpOnLandDecay.ToString(CultureInfo.CurrentCulture)},
                        {"$almanac_fish_jump_frequency", fish.m_jumpFrequencySeconds.ToString("0.0") + "<color=orange>s</color>"},
                        {"$almanac_fish_fast", ConvertBoolean(fish.m_fast)},
                    };

                    for (int index = 0; index < fish.m_baits.Count; index++)
                    {
                        Fish.BaitSetting bait = fish.m_baits[index];
                        fishData.Add("$almanac_fish_bait_name" + index, Localization.instance.Localize(bait.m_bait.m_itemData.m_shared.m_name));
                        fishData.Add("$almanac_fish_bait_chance" + index, bait.m_chance.ToString(CultureInfo.CurrentCulture));
                    }
                    
                    MergeDictionaries(DefaultData, fishData);
                };
            }
        }
        
        if (itemData.m_shared.m_itemType
            is ItemDrop.ItemData.ItemType.Helmet
            or ItemDrop.ItemData.ItemType.Legs
            or ItemDrop.ItemData.ItemType.Chest
            or ItemDrop.ItemData.ItemType.Customization
            or ItemDrop.ItemData.ItemType.Shoulder
           )
        {
            Dictionary<string, string> ArmorSettings = new()
            {
                {"$almanac_armor_title", "title"},
                {"$almanac_armor_label", itemData.m_shared.m_armor.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_armorPerLevel.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_armor_material", itemData.m_shared.m_armorMaterial && _ShowAllData.Value is AlmanacPlugin.Toggle.On ? RemoveParentheses(itemData.m_shared.m_armorMaterial.ToString()) : "0"},
                {"$almanac_helmet_hide_hair", SplitCamelCase(itemData.m_shared.m_helmetHideHair.ToString())},
                {"$almanac_helmet_hide_beard", SplitCamelCase(itemData.m_shared.m_helmetHideBeard.ToString())},
            };

            MergeDictionaries(DefaultData, ArmorSettings);
        }

        if (!itemData.m_shared.m_setName.IsNullOrWhiteSpace())
        {
            Dictionary<string, string> SetData = new()
            {
                {"$almanac_set_title", "title"},
                {"$almanac_set_name", itemData.m_shared.m_setName},
                {"$almanac_set_size", itemData.m_shared.m_setSize.ToString()},
                {"$almanac_set_status_effect",itemData.m_shared.m_setStatusEffect ? Localization.instance.Localize(itemData.m_shared.m_setStatusEffect.m_name) : "0"},
                {"$almanac_set_tooltip", itemData.m_shared.m_setStatusEffect ? Localization.instance.Localize(itemData.m_shared.m_setStatusEffect.m_tooltip) : "0"},
            };

            MergeDictionaries(DefaultData, SetData);
        }

        if (itemData.m_shared.m_damageModifiers.Count > 0)
        {
            Dictionary<string, string> SetDamageMods = new();

            foreach (HitData.DamageModPair mod in itemData.m_shared.m_damageModifiers)
            {
                SetDamageMods.Add(ConvertDamageTypes(mod.m_type),ConvertDamageModifiers(mod.m_modifier));
            }

            MergeDictionaries(DefaultData, SetDamageMods);
        }

        if (itemData.m_shared.m_setStatusEffect)
        {
            Dictionary<string, string> SetSkillMods = new();

            Skills.SkillType[] allSkills = Skills.s_allSkills;
            foreach (Skills.SkillType skill in allSkills)
            {
                float amount = new float();
                itemData.m_shared.m_setStatusEffect.ModifySkillLevel(skill, ref amount);
                if (!(amount > 0)) continue;
                SetSkillMods.Add(ConvertSkills(skill), "<color=orange>+</color>" + amount.ToString(CultureInfo.CurrentCulture));
            }
            
            MergeDictionaries(DefaultData, SetSkillMods);
        }

        if (itemData.m_shared.m_equipStatusEffect)
        {
            StatusEffect equipEffect = itemData.m_shared.m_equipStatusEffect;
            Dictionary<string, string> EquipData = new()
            {
                {"$almanac_equip_effects", "title"},
                {"$almanac_equip_status_effect", Localization.instance.Localize(equipEffect.m_name)},
            };
            
            Skills.SkillType[] allSkills = Skills.s_allSkills;
            foreach (Skills.SkillType skill in allSkills)
            {
                float skillLevel = new float();
                equipEffect.ModifySkillLevel(skill, ref skillLevel);
                if (skillLevel > 0) EquipData.Add(ConvertSkills(skill), "<color=orange>+</color>" + skillLevel.ToString(CultureInfo.CurrentCulture));

                float raiseLevel = new();
                equipEffect.ModifyRaiseSkill(skill, ref raiseLevel);
                if (raiseLevel > 0) EquipData.Add(ConvertSkills(skill), "<color=orange>+</color>" + raiseLevel.ToString(CultureInfo.CurrentCulture));
            }

            float fallDamage = new();
            equipEffect.ModifyFallDamage(40f, ref fallDamage);
            if (fallDamage > 0) EquipData.Add("$almanac_fall_damage", fallDamage.ToString(CultureInfo.CurrentCulture));

            float healthRegen = new float();
            equipEffect.ModifyHealthRegen(ref healthRegen);
            if (healthRegen > 0) EquipData.Add("$almanac_health_regen", healthRegen.ToString(CultureInfo.CurrentCulture));

            float staminaRegen = new float();
            equipEffect.ModifyStaminaRegen(ref staminaRegen);
            if (staminaRegen > 0) EquipData.Add("$almanac_stamina_regen", (staminaRegen * 100).ToString(CultureInfo.CurrentCulture) + "%");

            float eitrRegen = new();
            equipEffect.ModifyEitrRegen(ref eitrRegen);
            if (eitrRegen > 0) EquipData.Add("$almanac_eitr_regen", eitrRegen.ToString(CultureInfo.CurrentCulture));

            HitData.DamageModifiers modifiers = new();
            equipEffect.ModifyDamageMods(ref modifiers);
            EquipData.Add("$almanac_blunt1", ConvertDamageModifiers(modifiers.m_blunt));
            EquipData.Add("$almanac_slash1", ConvertDamageModifiers(modifiers.m_slash));
            EquipData.Add("$almanac_pierce1", ConvertDamageModifiers(modifiers.m_pierce));
            EquipData.Add("$almanac_chop1", ConvertDamageModifiers(modifiers.m_chop));
            EquipData.Add("$almanac_pickaxe1", ConvertDamageModifiers(modifiers.m_pickaxe));
            EquipData.Add("$almanac_fire1", ConvertDamageModifiers(modifiers.m_fire));
            EquipData.Add("$almanac_frost1", ConvertDamageModifiers(modifiers.m_frost));
            EquipData.Add("$almanac_lightning1", ConvertDamageModifiers(modifiers.m_lightning));
            EquipData.Add("$almanac_poison1", ConvertDamageModifiers(modifiers.m_poison));
            EquipData.Add("$almanac_spirit1", ConvertDamageModifiers(modifiers.m_spirit));

            MergeDictionaries(DefaultData, EquipData);
        }

        if (itemData.m_shared.m_movementModifier != 0f || itemData.m_shared.m_eitrRegenModifier != 0f ||
            itemData.m_shared.m_baseItemsStaminaModifier != 0)
        {
            Dictionary<string, string> StatModifiers = new()
            {
                {"$almanac_stat_modifiers_title", "title"},
                {"$almanac_movement_modifier_label", (itemData.m_shared.m_movementModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"},
                {"$almanac_eitr_regen_label", (itemData.m_shared.m_eitrRegenModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"},
                {"$almanac_base_items_stamina_modifier_label", (itemData.m_shared.m_baseItemsStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"},
            };

            MergeDictionaries(DefaultData, StatModifiers);
        }

        if (itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable)
        {
            bool addedFoodTitle = false;
            if (itemData.m_shared.m_food + itemData.m_shared.m_foodStamina + itemData.m_shared.m_foodBurnTime + itemData.m_shared.m_foodRegen != 0)
            {
                Dictionary<string, string> FoodSettings = new()
                {
                    {"$almanac_food_title", "title"},
                    {"$almanac_food_health", itemData.m_shared.m_food.ToString(CultureInfo.CurrentCulture)},
                    {"$almanac_food_stamina", itemData.m_shared.m_foodStamina.ToString(CultureInfo.CurrentCulture)},
                    {"$almanac_food_eitr", itemData.m_shared.m_foodEitr.ToString(CultureInfo.CurrentCulture)},
                    {"$almanac_food_burn_time", itemData.m_shared.m_foodBurnTime.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                    {"$almanac_food_health_regen", itemData.m_shared.m_foodRegen.ToString(CultureInfo.CurrentCulture) + "<color=orange>/tick</color>"},
                };
                addedFoodTitle = true;
                MergeDictionaries(DefaultData, FoodSettings);
            }

            if (itemData.m_shared.m_consumeStatusEffect)
            {
                if (!addedFoodTitle) DefaultData.Add("$almanac_food_title", "title");
                StatusEffect ConsumeEffect = itemData.m_shared.m_consumeStatusEffect;
                
                Dictionary<string, string> ConsumeData = new()
                {
                    {"$almanac_consume_effect", Localization.instance.Localize(ConsumeEffect.m_name)},
                    {"$almanac_consume_category", ConsumeEffect.m_category},
                    {"$almanac_consume_tooltip", Localization.instance.Localize(ConsumeEffect.m_tooltip)},
                    {"$almanac_consume_attributes", SplitCamelCase(ConsumeEffect.m_attributes.ToString())},
                    {"$almanac_consume_message", Localization.instance.Localize(ConsumeEffect.m_startMessage)},
                    {"$almanac_consume_duration", ConsumeEffect.m_ttl.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"},
                };
                
                Skills.SkillType[] allSkills = Skills.s_allSkills;
                foreach (Skills.SkillType skill in allSkills)
                {
                    float skillLevel = new float();
                    ConsumeEffect.ModifySkillLevel(skill, ref skillLevel);
                    if (skillLevel > 0) ConsumeData.Add(ConvertSkills(skill), "<color=orange>+</color>" + skillLevel.ToString(CultureInfo.CurrentCulture));

                    float raiseLevel = new();
                    ConsumeEffect.ModifyRaiseSkill(skill, ref raiseLevel);
                    if (raiseLevel > 0) ConsumeData.Add(ConvertSkills(skill), "<color=orange>+</color>" + raiseLevel.ToString(CultureInfo.CurrentCulture));
                }

                float healthRegen = new float();
                ConsumeEffect.ModifyHealthRegen(ref healthRegen);
                if (healthRegen > 0) ConsumeData.Add("$almanac_consume_health_regen", healthRegen.ToString(CultureInfo.CurrentCulture));

                float staminaRegen = new float();
                ConsumeEffect.ModifyStaminaRegen(ref staminaRegen);
                if (staminaRegen > 0) ConsumeData.Add("$almanac_consume_stamina_regen", (staminaRegen * 100).ToString(CultureInfo.CurrentCulture) + "%");

                float eitrRegen = new();
                ConsumeEffect.ModifyEitrRegen(ref eitrRegen);
                if (eitrRegen > 0) ConsumeData.Add("$almanac_consume_eitr_regen", eitrRegen.ToString(CultureInfo.CurrentCulture));

                HitData.DamageModifiers modifiers = new();
                ConsumeEffect.ModifyDamageMods(ref modifiers);
                ConsumeData.Add("$almanac_blunt2", ConvertDamageModifiers(modifiers.m_blunt));
                ConsumeData.Add("$almanac_slash2", ConvertDamageModifiers(modifiers.m_slash));
                ConsumeData.Add("$almanac_pierce2", ConvertDamageModifiers(modifiers.m_pierce));
                ConsumeData.Add("$almanac_chop2", ConvertDamageModifiers(modifiers.m_chop));
                ConsumeData.Add("$almanac_pickaxe2", ConvertDamageModifiers(modifiers.m_pickaxe));
                ConsumeData.Add("$almanac_fire2", ConvertDamageModifiers(modifiers.m_fire));
                ConsumeData.Add("$almanac_frost2", ConvertDamageModifiers(modifiers.m_frost));
                ConsumeData.Add("$almanac_lightning2", ConvertDamageModifiers(modifiers.m_lightning));
                ConsumeData.Add("$almanac_poison2", ConvertDamageModifiers(modifiers.m_poison));
                ConsumeData.Add("$almanac_spirit2", ConvertDamageModifiers(modifiers.m_spirit));

                MergeDictionaries(DefaultData, ConsumeData);
            }
        }

        if (itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Shield)
        {
            Dictionary<string, string> ShieldSettings = new()
            {
                {"$almanac_shield_title", "title"},
                {"$almanac_shield_block_power", itemData.m_shared.m_blockPower.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_shield_block_power_per_level", itemData.m_shared.m_blockPowerPerLevel.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_shield_deflection_force", itemData.m_shared.m_deflectionForce.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_shield_deflection_force_per_level", itemData.m_shared.m_deflectionForcePerLevel.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_timed_block_bonus", itemData.m_shared.m_timedBlockBonus.ToString(CultureInfo.CurrentCulture)}
            };

            MergeDictionaries(DefaultData, ShieldSettings);
        }

        if (itemData.m_shared.m_itemType
            is ItemDrop.ItemData.ItemType.Ammo
            or ItemDrop.ItemData.ItemType.Bow
            or ItemDrop.ItemData.ItemType.Hands
            or ItemDrop.ItemData.ItemType.Tool
            or ItemDrop.ItemData.ItemType.Torch
            or ItemDrop.ItemData.ItemType.Attach_Atgeir
            or ItemDrop.ItemData.ItemType.AmmoNonEquipable
            or ItemDrop.ItemData.ItemType.OneHandedWeapon
            or ItemDrop.ItemData.ItemType.TwoHandedWeapon
            or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft
           )
        {
            Dictionary<string, string> Weapon = new()
            {
                {"$almanac_weapon_title", "title"},
                {"$almanac_weapon_animation_state", SplitCamelCase(itemData.m_shared.m_animationState.ToString())},
                {"$almanac_weapon_skill_type", ConvertSkills(itemData.m_shared.m_skillType)},
                {"$almanac_tool_tier", itemData.m_shared.m_toolTier.ToString()},
                {"$almanac_damage3", itemData.m_shared.m_damages.m_damage.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_damage.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_blunt3", itemData.m_shared.m_damages.m_blunt.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_blunt.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_slash3", itemData.m_shared.m_damages.m_slash.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_slash.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_pierce3", itemData.m_shared.m_damages.m_pierce.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_pierce.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_chop3", itemData.m_shared.m_damages.m_chop.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_chop.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_pickaxe3", itemData.m_shared.m_damages.m_pickaxe.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_pickaxe.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_fire3", itemData.m_shared.m_damages.m_fire.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_fire.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_frost3", itemData.m_shared.m_damages.m_frost.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_frost.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_lighting3", itemData.m_shared.m_damages.m_lightning.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_lightning.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_poison3", itemData.m_shared.m_damages.m_poison.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_poison.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_spirit3", itemData.m_shared.m_damages.m_spirit.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_spirit.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>"},
                {"$almanac_attack_force3", itemData.m_shared.m_attackForce.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_back_stab_bonus3", itemData.m_shared.m_backstabBonus.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_dodgeable3", ConvertBoolean(itemData.m_shared.m_dodgeable)},
                {"$almanac_blockable3", ConvertBoolean(itemData.m_shared.m_blockable)},
                {"$almanac_tame_only", ConvertBoolean(itemData.m_shared.m_tamedOnly)},
                {"$almanac_always_rotate", ConvertBoolean(itemData.m_shared.m_alwaysRotate)},
                {"$almanac_attack_effect", itemData.m_shared.m_attackStatusEffect ? RemoveParentheses(itemData.m_shared.m_attackStatusEffect.ToString()) : "0"},
                {"$almanac_spawn_on_hit", itemData.m_shared.m_spawnOnHit ? RemoveParentheses(itemData.m_shared.m_spawnOnHit.ToString()) : "0"},
                {"$almanac_spawn_on_hit_terrain", itemData.m_shared.m_spawnOnHitTerrain ? RemoveParentheses(itemData.m_shared.m_spawnOnHitTerrain.ToString()) : "0"},
                {"$almanac_projectile_tooltip", ConvertBoolean(itemData.m_shared.m_projectileToolTip)},
                {"$almanac_ammo_type", Localization.instance.Localize(itemData.m_shared.m_ammoType)},
            };

            MergeDictionaries(DefaultData, Weapon);

            Dictionary<string, string> Attacks = new()
            {
                {"$almanac_attack_type", ConvertAttackTypes(itemData.m_shared.m_attack.m_attackType)},
                {"$almanac_attack_animation", itemData.m_shared.m_attack.m_attackAnimation},
                {"$almanac_attack_random_animations", itemData.m_shared.m_attack.m_attackRandomAnimations.ToString()},
                {"$almanac_attack_chain_levels", itemData.m_shared.m_attack.m_attackChainLevels.ToString()},
                {"$almanac_attack_looping", itemData.m_shared.m_attack.m_loopingAttack.ToString()},
                {"$almanac_consume_item", itemData.m_shared.m_attack.m_consumeItem.ToString()},
                {"$almanac_hit_terrain", itemData.m_shared.m_attack.m_hitTerrain.ToString()},
                {"$almanac_is_home_item", itemData.m_shared.m_attack.m_isHomeItem.ToString()},
                {"$almanac_attack_stamina", itemData.m_shared.m_attack.m_attackStamina.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_attack_eitr", itemData.m_shared.m_attack.m_attackEitr.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_attack_health", itemData.m_shared.m_attack.m_attackHealth.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_attack_health_percentage", itemData.m_shared.m_attack.m_attackHealthPercentage.ToString(CultureInfo.CurrentCulture)},
                {"$almanac_speed_factor", (itemData.m_shared.m_attack.m_speedFactor * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"},
                {"$almanac_speed_factor_rotation", (itemData.m_shared.m_attack.m_speedFactorRotation * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"},
                {"$almanac_attack_start_noise", itemData.m_shared.m_attack.m_attackStartNoise.ToString("0.0")}
            };

            MergeDictionaries(DefaultData, Attacks);
        }
        return DefaultData;
    }
    private static void AddItemRecipe(ItemDrop.ItemData itemData)
    {
        Recipe recipe = ObjectDB.instance.GetRecipe(itemData);
        if (recipe == null) return;
        
        int resourceCount =  Mathf.FloorToInt(recipe.m_resources.Length / 8f) + 1;
        if (recipe.m_resources.Length != 0)
        {
            for (int index = 0; index < resourceCount; ++index)
            {
                GameObject recipeContainer = Object.Instantiate(CacheAssets.Drops, CreateAlmanac.PanelContent);
                Utils.FindChild(recipeContainer.transform, "$part_title").GetComponent<TextMeshProUGUI>().text = index > 0 ? "" :
                    Localization.instance.Localize("$almanac_recipe_title");
                PanelElements.Add(recipeContainer);
                int j = 0;
                for (int i = index * 7; i < (index + 1) * 7; ++i)
                {
                    if (i >= recipe.m_resources.Length) break;
                    Transform part = Utils.FindChild(recipeContainer.transform, $"$part_drop_{j}");
                    Transform name = part.GetChild(0);
                
                    if (!part.TryGetComponent(out Image image)) continue;
                    if (!name.TryGetComponent(out TextMeshProUGUI text)) continue;
                    
                    Piece.Requirement req = recipe.m_resources[i];
                    ItemDrop item = req.m_resItem;

                    bool isKnown = Player.m_localPlayer.IsKnownMaterial(item.m_itemData.m_shared.m_name) || _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.NoCostCheat();

                    image.sprite = item.m_itemData.GetIcon();
                    image.color = isKnown ? Color.white : Color.black;
                    text.text = isKnown ? ReplaceSpaceWithNewLine(Localization.instance.Localize(item.m_itemData.m_shared.m_name))
                                + $"\n(<color=orange>{req.m_amount}</color> +<color=orange>{req.m_amountPerLevel}</color>/lvl)" : UnknownText;
                
                    part.gameObject.SetActive(true);
                    ++j;
                }
            }
        }
    }

    public static bool isMetricsActive = false;
    public static void UpdateMetricsPanel()
    {
        if (!Player.m_localPlayer) return;
        CreateAlmanac.AlmanacGUI.SetActive(true);
        CreateAlmanac.AchievementGUI.SetActive(false);
        DestroyPanelElements();
        isMetricsActive = true;
        CreateAlmanac.PanelIcon.sprite = SpriteManager.bookClosedRedIcon;
        CreateAlmanac.PanelTitle.text = Player.m_localPlayer.GetHoverName();
        CreateAlmanac.PanelButton.text = Localization.instance.Localize("$almanac_leaderboard_button");
        
        foreach (KeyValuePair<string, string> kvp in GetMetricData())
        {
            if (kvp.Value == "title")
            {
                GameObject data = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!data.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) continue;

                component.text = Localization.instance.Localize(kvp.Key);
                
                PanelElements.Add(data);
            }
            else
            {
                GameObject data = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                if (!Utils.FindChild(data.transform, "$part_infoType").TryGetComponent(out TextMeshProUGUI TypeComponent)) continue;
                if (!Utils.FindChild(data.transform, "$part_data").GetComponent<TextMeshProUGUI>().TryGetComponent(out TextMeshProUGUI DataComponent)) continue;
                
                TypeComponent.text = Localization.instance.Localize(kvp.Key);
                DataComponent.text = Localization.instance.Localize(kvp.Value);
    
                PanelElements.Add(data);
            }
        }
    }
    public static void UpdateLeaderboardPanel()
    {
        if (!Player.m_localPlayer) return;
        CreateAlmanac.AlmanacGUI.SetActive(true);
        CreateAlmanac.AchievementGUI.SetActive(false);
        DestroyPanelElements();
        isMetricsActive = false;
        CreateAlmanac.PanelIcon.sprite = SpriteManager.crownGoldIcon;
        CreateAlmanac.PanelTitle.text = Localization.instance.Localize("$almanac_leaderboard_title");
        CreateAlmanac.PanelButton.text = Localization.instance.Localize("$almanac_stats_button");
        if (Leaderboard.LeaderboardData.Count == 0) return;
        Dictionary<string, PlayerData> ranked = Leaderboard.LeaderboardData.OrderByDescending(kv => kv.Value.completed_achievements).ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (KeyValuePair<string, PlayerData> kvp in ranked)
        {
            GameObject item = Object.Instantiate(CacheAssets.LeaderboardItem, CreateAlmanac.PanelContent);
            if (Utils.FindChild(item.transform, "$part_name").TryGetComponent(out TextMeshProUGUI name))
            {
                name.text = kvp.Key;
            }

            if (Utils.FindChild(item.transform, "$part_rank").GetChild(1).TryGetComponent(out TextMeshProUGUI rank))
            {
                int ranking = ranked.Keys.ToList().IndexOf(kvp.Key) + 1;
                rank.text = $"<color=orange>{ranking}</color>";
            }

            if (Utils.FindChild(item.transform, "$part_achievement").GetChild(1).TryGetComponent(out TextMeshProUGUI achievement))
            {
                achievement.text = kvp.Value.completed_achievements.ToString();
            }

            if (Utils.FindChild(item.transform, "$part_kd").GetChild(1).TryGetComponent(out TextMeshProUGUI kd))
            {
                double ratio = kvp.Value.total_deaths == 0
                    ? double.PositiveInfinity
                    : (double)kvp.Value.total_kills / kvp.Value.total_deaths;

                string formattedRatio = ratio.ToString("0.0");

                kd.text = Localization.instance.Localize(
                    $"<color=orange>{kvp.Value.total_kills}</color> $almanac_kills / <color=orange>{kvp.Value.total_deaths}</color> $almanac_deaths ({formattedRatio})");
            }
            
            PanelElements.Add(item);
        }
    }
    private static Dictionary<string, string> GetMetricData()
    {
        Dictionary<string, string> metrics = new()
        {
            {"$almanac_kill_title", "title"},
            {"$almanac_total_kills", GetPlayerStat(PlayerStatType.EnemyKills).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_enemy_kills", GetPlayerStat(PlayerStatType.EnemyKills).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_player_kills", GetPlayerStat(PlayerStatType.PlayerKills).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_hits_taken_enemies",GetPlayerStat(PlayerStatType.HitsTakenEnemies).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_total_deaths", GetPlayerStat(PlayerStatType.Deaths).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_enemy_hits",GetPlayerStat(PlayerStatType.EnemyHits).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_enemy_last_hit", GetPlayerStat(PlayerStatType.EnemyKillsLastHits).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_player_hits",GetPlayerStat(PlayerStatType.PlayerHits).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_craft_or_upgrades", "title"},
            {"$almanac_craft_or_upgrade_count", GetPlayerStat(PlayerStatType.CraftsOrUpgrades).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_builds", GetPlayerStat(PlayerStatType.Builds).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_crafts", GetPlayerStat(PlayerStatType.Crafts).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_upgrades",GetPlayerStat(PlayerStatType.Upgrades).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_misc_title", "title"},
            {"$almanac_skeleton_summoned",GetPlayerStat(PlayerStatType.SkeletonSummons).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_item_picked_up",GetPlayerStat(PlayerStatType.ItemsPickedUp).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_jumps", GetPlayerStat(PlayerStatType.Jumps).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_portal_used", GetPlayerStat(PlayerStatType.PortalsUsed).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_cheats",GetPlayerStat(PlayerStatType.Cheats).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_world_loads",GetPlayerStat(PlayerStatType.WorldLoads).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_creature_tame",GetPlayerStat(PlayerStatType.CreatureTamed).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_arrows_shot",GetPlayerStat(PlayerStatType.ArrowsShot).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_doors_opened", GetPlayerStat(PlayerStatType.DoorsOpened).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_doors_closed", GetPlayerStat(PlayerStatType.DoorsClosed).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_bees_harvested", GetPlayerStat(PlayerStatType.BeesHarvested).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_sap_harvested", GetPlayerStat(PlayerStatType.SapHarvested).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_turret_ammo_added", GetPlayerStat(PlayerStatType.TurretAmmoAdded).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_turret_trophy_set", GetPlayerStat(PlayerStatType.TurretTrophySet).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_trap_armed", GetPlayerStat(PlayerStatType.TrapArmed).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_trap_triggered", GetPlayerStat(PlayerStatType.TrapTriggered).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_place_stacks", GetPlayerStat(PlayerStatType.PlaceStacks).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_portal_dungeon_in", GetPlayerStat(PlayerStatType.PortalDungeonIn).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_portal_dungeon_out", GetPlayerStat(PlayerStatType.PortalDungeonOut).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_total_boss_kills", GetPlayerStat(PlayerStatType.BossKills).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_boss_last_hits", GetPlayerStat(PlayerStatType.BossLastHits).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_distance_title", "title"},
            {"$almanac_distanced_traveled",GetPlayerStat(PlayerStatType.DistanceTraveled).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_distance_walked",GetPlayerStat(PlayerStatType.DistanceWalk).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_distance_ran",GetPlayerStat(PlayerStatType.DistanceRun).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_distance_sailed",GetPlayerStat(PlayerStatType.DistanceSail).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_distance_air",GetPlayerStat(PlayerStatType.DistanceAir).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_base_title", "title"},
            {"$almanac_time_in_base", (GetPlayerStat(PlayerStatType.TimeInBase) / 60).ToString(CultureInfo.CurrentCulture) + "<color=orange>min</color>"},
            {"$almanac_time_out_base", (GetPlayerStat(PlayerStatType.TimeOutOfBase) / 60).ToString(CultureInfo.CurrentCulture) + "<color=orange>min</color>"},
            {"$almanac_sleep",GetPlayerStat(PlayerStatType.Sleep).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_stands_title", "title"},
            {"$almanac_item_stand_used",GetPlayerStat(PlayerStatType.ItemStandUses).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_armor_stand_used",GetPlayerStat(PlayerStatType.ArmorStandUses).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_trees_title", "title"},
            {"$almanac_tree_chopped",GetPlayerStat(PlayerStatType.TreeChops).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree",GetPlayerStat(PlayerStatType.Tree).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree_tier_0",GetPlayerStat(PlayerStatType.TreeTier0).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree_tier_1",GetPlayerStat(PlayerStatType.TreeTier1).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree_tier_2",GetPlayerStat(PlayerStatType.TreeTier2).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree_tier_3",GetPlayerStat(PlayerStatType.TreeTier3).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree_tier_4",GetPlayerStat(PlayerStatType.TreeTier4).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tree_tier_5",GetPlayerStat(PlayerStatType.TreeTier5).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_log_chopped",GetPlayerStat(PlayerStatType.LogChops).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_logs",GetPlayerStat(PlayerStatType.Logs).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_ore_title", "title"},
            {"$almanac_mine_hits",GetPlayerStat(PlayerStatType.MineHits).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mines",GetPlayerStat(PlayerStatType.Mines).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mine_tier_0",GetPlayerStat(PlayerStatType.MineTier0).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mine_tier_1",GetPlayerStat(PlayerStatType.MineTier1).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mine_tier_2",GetPlayerStat(PlayerStatType.MineTier2).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mine_tier_3",GetPlayerStat(PlayerStatType.MineTier3).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mine_tier_4",GetPlayerStat(PlayerStatType.MineTier4).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_mine_tier_5",GetPlayerStat(PlayerStatType.MineTier5).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_raven_title","title"},
            {"$almanac_raven_hits",GetPlayerStat(PlayerStatType.RavenHits).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_raven_talk",GetPlayerStat(PlayerStatType.RavenTalk).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_raven_appear",GetPlayerStat(PlayerStatType.RavenAppear).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_food_eaten",GetPlayerStat(PlayerStatType.FoodEaten).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tombstone_title","title"},
            {"$almanac_tombstones_open_own",GetPlayerStat(PlayerStatType.TombstonesOpenedOwn).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tombstone_open_other",GetPlayerStat(PlayerStatType.TombstonesOpenedOther).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_tombstone_fit",GetPlayerStat(PlayerStatType.TombstonesFit).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_death_title","title"},
            {"$almanac_death_by_undefined",GetPlayerStat(PlayerStatType.DeathByUndefined).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_death_by_enemy_hit",GetPlayerStat(PlayerStatType.DeathByEnemyHit).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_death_by_player_hit",GetPlayerStat(PlayerStatType.DeathByPlayerHit).ToString(CultureInfo.CurrentCulture)},
            {"$almanac_death_by_fall", GetPlayerStat(PlayerStatType.DeathByFall).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_drowning", GetPlayerStat(PlayerStatType.DeathByDrowning).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_burning", GetPlayerStat(PlayerStatType.DeathByBurning).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_freezing", GetPlayerStat(PlayerStatType.DeathByFreezing).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_poisoned", GetPlayerStat(PlayerStatType.DeathByPoisoned).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_smoke", GetPlayerStat(PlayerStatType.DeathBySmoke).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_water", GetPlayerStat(PlayerStatType.DeathByWater).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_edge_of_world", GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_impact", GetPlayerStat(PlayerStatType.DeathByImpact).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_cart", GetPlayerStat(PlayerStatType.DeathByCart).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_tree", GetPlayerStat(PlayerStatType.DeathByTree).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_self", GetPlayerStat(PlayerStatType.DeathBySelf).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_structural", GetPlayerStat(PlayerStatType.DeathByStructural).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_boat", GetPlayerStat(PlayerStatType.DeathByBoat).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_turret", GetPlayerStat(PlayerStatType.DeathByTurret).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_death_by_stalagtite", GetPlayerStat(PlayerStatType.DeathByStalagtite).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_guardian_power_title","title"},
            {"$almanac_set_guardian_power", GetPlayerStat(PlayerStatType.SetGuardianPower).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_eikthyr", GetPlayerStat(PlayerStatType.SetPowerEikthyr).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_elder", GetPlayerStat(PlayerStatType.SetPowerElder).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_bonemass", GetPlayerStat(PlayerStatType.SetPowerBonemass).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_moder", GetPlayerStat(PlayerStatType.SetPowerModer).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_yagluth", GetPlayerStat(PlayerStatType.SetPowerYagluth).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_queen", GetPlayerStat(PlayerStatType.SetPowerQueen).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_ashlands", GetPlayerStat(PlayerStatType.SetPowerAshlands).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_set_power_deepNorth", GetPlayerStat(PlayerStatType.SetPowerDeepNorth).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_guardian_power", GetPlayerStat(PlayerStatType.UseGuardianPower).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_eikthyr", GetPlayerStat(PlayerStatType.UsePowerEikthyr).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_elder", GetPlayerStat(PlayerStatType.UsePowerElder).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_bonemass", GetPlayerStat(PlayerStatType.UsePowerBonemass).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_moder", GetPlayerStat(PlayerStatType.UsePowerModer).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_yagluth", GetPlayerStat(PlayerStatType.UsePowerYagluth).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_queen", GetPlayerStat(PlayerStatType.UsePowerQueen).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_ashlands", GetPlayerStat(PlayerStatType.UsePowerAshlands).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_use_power_deepNorth", GetPlayerStat(PlayerStatType.UsePowerDeepNorth).ToString(CultureInfo.CurrentCulture) },
            {"$almanac_count", GetPlayerStat(PlayerStatType.Count).ToString(CultureInfo.CurrentCulture) },
        };

        return metrics;
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnOpenTrophies))]
    private static class UpdateAlmanacAssets
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            CreateTabs();
            CreateAlmanac.AlmanacGUI.SetActive(true);
            UpdatePlayerStats();
            UpdateMetricsPanel();
            
            UpdateTopic();

            if (CheckedCompletion) return;
            
            CheckCompletedAchievements();
            CheckedCompletion = true;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCloseTrophies))]
    private static class DestroyAlmanacAssets
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            DestroyTabs();
            CreateAlmanac.AlmanacGUI.SetActive(false);
            CreateAlmanac.AchievementGUI.SetActive(false);
        }
    }
    
    // Check if certain keys are hit to close Almanac GUI
    public static void UpdateGUI()
    {
        if (!Player.m_localPlayer) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Player.m_localPlayer.IsDead())
        {
            if (AreTabsVisible() || CreateAlmanac.IsPanelActive() || CreateAlmanac.IsAchievementActive())
            {
                DestroyTabs();
                CreateAlmanac.AlmanacGUI.SetActive(false);
                CreateAlmanac.AchievementGUI.SetActive(false);
            }
        };
    }

    // Pressing interact key would close trophy panel
    // This patch prevents that
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetButtonDown))]
    private static class ZInputPatch
    {
        private static bool Prefix() => !IsTrophyPanelActive();

        private static bool IsTrophyPanelActive()
        {
            if (!InventoryGui.instance) return true;
            
            return InventoryGui.instance.m_trophiesPanel && InventoryGui.instance.m_trophiesPanel.activeInHierarchy;
        }
    }
}