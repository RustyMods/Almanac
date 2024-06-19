using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Almanac.Achievements;
using Almanac.Bounties;
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
    
    private static readonly List<GameObject> PanelElements = new();
    private static readonly float TrophySpacing = 180f;
    private static readonly string UnknownText = Localization.instance.Localize("$almanac_locked");
    
    private static ItemDrop SelectedItemDrop = null!;
    private static CreatureData SelectedCreature = null!;
    private static GameObject SelectedPiece = null!;
    public static Achievement SelectedAchievement = null!;
    public static Bounties.Data.ValidatedBounty SelectedBounty = null!;
    public static TreasureHunt.Data.ValidatedTreasure SelectedTreasure = null!;
    
    public static bool isMetricsActive;
    public static bool CheckedCompletion;

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
        CreateAlmanac.SetCheckmarkVisible();
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
                        ? GetAchievements() 
                        : GetAchievements().FindAll(item => item.m_displayName.ToLower().Contains(filter)));
                    break;
                case "$almanac_stats_button":
                    UpdateMetricsPanel();
                    break;
                case "$almanac_leaderboard_button":
                    UpdateLeaderboardPanel();
                    break;
                case "$almanac_quests_button":
                    UpdateBountyList(GUI, BountyManager.RegisteredBounties);
                    break;
                case "$almanac_treasure_hunt_button":
                    UpdateTreasureList(GUI, TreasureHunt.TreasureManager.RegisteredTreasure);
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
    public static void DestroyTrophies(InventoryGui instance)
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
            string LocalizedDesc = Localization.instance.Localize(component.m_itemData.m_shared.m_description);
            string LocalizedLore = Localization.instance.Localize(component.m_itemData.m_shared.m_name + "_lore");
            Sprite? ItemIcon = UITools.TryGetIcon(component);
            bool isKnown = _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsKnownMaterial(component.m_itemData.m_shared.m_name) || Player.m_localPlayer.NoCostCheat();
            
            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);

            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            UITools.PlaceElement(transform, index, TrophySpacing);

            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);
            
            UITools.SetElementText(transform, isKnown, LocalizedName, isTrophies ? LocalizedLore : LocalizedDesc, UnknownText);
            
            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = ItemIcon;
            iconImage.color = isKnown ? Color.white : Color.black;

            Button button = UITools.AddButtonComponent(icon.gameObject, iconImage, isKnown);

            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedItemDrop = component;
                UpdateItemPanel();
            });

            instance.m_trophyList.Add(gameObject);
        }
        UITools.ResizePanel(instance, -a1);
    }
    private static void UpdatePieceList(InventoryGui instance, List<GameObject> prefabs)
    {
        float a1 = 0.0f;
        for (int index = 0; index < prefabs.Count; ++index)
        {
            GameObject prefab = prefabs[index];
            if (!prefab.TryGetComponent(out Piece piece)) continue;
            string LocalizedName = Localization.instance.Localize(piece.m_name);
            string LocalizedDesc = Localization.instance.Localize(piece.m_description);

            bool isKnown = _KnowledgeWall.Value is AlmanacPlugin.Toggle.Off || Player.m_localPlayer.IsRecipeKnown(piece.m_name) || Player.m_localPlayer.NoCostCheat();
            
            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);
            
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            UITools.PlaceElement(transform, index, TrophySpacing);

            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);
            
            UITools.SetElementText(transform, isKnown, LocalizedName, LocalizedDesc, UnknownText);
            
            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = piece.m_icon;
            iconImage.color = isKnown ? Color.white : Color.black;

            Button button = UITools.AddButtonComponent(icon.gameObject, iconImage, isKnown);
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedPiece = prefab;
                UpdatePiecePanel();
            });

            instance.m_trophyList.Add(gameObject);
        }
        UITools.ResizePanel(instance, -a1);
    }
    private static void UpdateCreatureList(InventoryGui instance, List<CreatureData> creatures)
    {
        float totalHeight = (Mathf.CeilToInt(creatures.Count / 5f) * 34f);
        float half = (totalHeight / 2f) - 17f;
        if (totalHeight <= instance.m_trophieListBaseSize) half = 306f;
        
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
        UITools.ResizePanel(instance, totalHeight);
    }
    private static void UpdateTreasureList(InventoryGui instance, List<TreasureHunt.Data.ValidatedTreasure> treasures)
    {
        float a1 = 0.0f;
        for (int index = 0; index < treasures.Count; ++index)
        {
            TreasureHunt.Data.ValidatedTreasure treasure = treasures[index];
            bool isKnown = Player.m_localPlayer.IsBiomeKnown(treasure.m_biome) || Player.m_localPlayer.NoCostCheat();

            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);
            
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            UITools.PlaceElement(transform, index, TrophySpacing);
            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);
            
            UITools.SetElementText(transform, isKnown, treasure.m_name, treasure.m_biome.ToString(), UnknownText);

            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = treasure.m_sprite ? treasure.m_sprite : SpriteManager.AlmanacIcon;
            iconImage.color = isKnown ? Color.white : Color.black;
            
            
            Button button = UITools.AddButtonComponent(icon.gameObject, iconImage, isKnown);
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementPanelButton.text = Localization.instance.Localize(isMetricsActive ? "$almanac_leaderboard_button" : "$almanac_stats_button");
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedTreasure = treasure;
                UpdateTreasurePanel();
            });

            instance.m_trophyList.Add(gameObject);
        }
        UITools.ResizePanel(instance, -a1);
    }
    private static void UpdateBountyList(InventoryGui instance, List<Bounties.Data.ValidatedBounty> bounties)
    {
        float a1 = 0.0f;
        for (int index = 0; index < bounties.Count; ++index)
        {
            Bounties.Data.ValidatedBounty bounty = bounties[index];
            bool isKnown = ZoneSystem.instance.GetGlobalKeys().Contains(bounty.m_defeatKey) || ZoneSystem.instance.GetGlobalKey(bounty.m_defeatKey) || Player.m_localPlayer.NoCostCheat();

            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);
            
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            UITools.PlaceElement(transform, index, TrophySpacing);
            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);
            
            UITools.SetElementText(transform, isKnown, bounty.m_creatureName, bounty.m_biome.ToString(), UnknownText);

            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = bounty.m_icon ? bounty.m_icon : SpriteManager.AlmanacIcon;
            iconImage.color = isKnown ? Color.white : Color.black;

            Button button = UITools.AddButtonComponent(icon.gameObject, iconImage, isKnown);
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementPanelButton.text = Localization.instance.Localize(isMetricsActive ? "$almanac_leaderboard_button" : "$almanac_stats_button");
                CreateAlmanac.AchievementGUI.SetActive(false);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                SelectedBounty = bounty;
                UpdateBountyPanel();
            });
            instance.m_trophyList.Add(gameObject);
        }
        UITools.ResizePanel(instance, -a1);
    }
    public static void UpdateAchievementList(InventoryGui instance, List<Achievement> achievements, bool completed = false)
    {
        float a1 = 0.0f;
        if (completed) achievements = achievements.OrderBy(x => !x.m_isCompleted).ToList();
        for (int index = 0; index < achievements.Count; ++index)
        {
            Achievement achievement = achievements[index];
            
            bool isCompleted = achievement.m_isCompleted || Player.m_localPlayer.NoCostCheat();
            
            GameObject gameObject = Object.Instantiate(instance.m_trophieElementPrefab, instance.m_trophieListRoot);
            
            gameObject.SetActive(true);
            RectTransform? transform = gameObject.transform as RectTransform;
            if (transform == null) continue;

            UITools.PlaceElement(transform, index, TrophySpacing);
            a1 = Mathf.Min(a1, transform.anchoredPosition.y - instance.m_trophieListSpace);

            Transform icon = transform.Find("icon_bkg/icon");
            if (!icon.TryGetComponent(out Image iconImage)) continue;
            iconImage.sprite = achievement.m_sprite ? achievement.m_sprite : SpriteManager.AlmanacIcon;
            
            transform.Find("name").GetComponent<TMP_Text>().text = isCompleted ? achievement.m_displayName : UnknownText;
            transform.Find("description").GetComponent<TMP_Text>().text = achievement.m_desc;
            if (achievement.m_statusEffect != null)
            {
                transform.Find("$part_outline").gameObject.SetActive(Player.m_localPlayer.GetSEMan().HaveStatusEffect(achievement.m_statusEffect.name.GetStableHashCode()));
            }

            Button button = gameObject.AddComponent<Button>();
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
                normalColor = isCompleted ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.black,
                selectedColor = Color.white
            };
            button.onClick = new Button.ButtonClickedEvent();
            ButtonSfx sfx = gameObject.gameObject.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;
            
            button.onClick.AddListener(() =>
            {
                CreateAlmanac.AchievementPanelButton.text = Localization.instance.Localize(isMetricsActive ? "$almanac_leaderboard_button" : "$almanac_stats_button");
                CreateAlmanac.AchievementGUI.SetActive(true);
                CreateAlmanac.AlmanacGUI.SetActive(false);
                SelectedAchievement = achievement;
                UpdateAchievementPanel();
            });
            instance.m_trophyList.Add(gameObject);
        }
        UITools.ResizePanel(instance, -a1);
    }
    public static void UpdateBountyPanel()
    {
        DestroyPanelElements();

        CreateAlmanac.PanelIcon.sprite = SelectedBounty.m_icon;
        CreateAlmanac.PanelTitle.text = SelectedBounty.m_creatureName;

        Dictionary<string, string> output = new()
        {
            { "$almanac_reward", "title" },
        };

        switch (SelectedBounty.m_rewardType)
        {
            case Bounties.Data.QuestRewardType.Item:
                if (SelectedBounty.m_itemReward == null) break;
                output.Add("$almanac_item", SelectedBounty.m_itemReward.m_itemData.m_shared.m_name);
                output.Add("$almanac_amount", SelectedBounty.m_itemAmount.ToString(CultureInfo.CurrentCulture));
                break;
            case Bounties.Data.QuestRewardType.Skill:
                output.Add("$almanac_skill_type", ConvertSkills(SelectedBounty.m_skill));
                output.Add("$almanac_amount", SelectedBounty.m_skillAmount.ToString(CultureInfo.CurrentCulture) + "<color=orange>$almanac_xp</color>");
                break;
        }

        if (SelectedBounty.m_experience > 0)
        {
            output.Add("Class Experience", $"{SelectedBounty.m_experience}<color=orange>XP</color>");
        }

        Dictionary<string, string> BountyData = new Dictionary<string, string>()
        {
            { "$almanac_bounty_info", "title" },
            { "$almanac_creature_prefab", SelectedBounty.m_critter.name },
            { "$almanac_level", SelectedBounty.m_level.ToString() },
            { "$almanac_health", SelectedBounty.m_health.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_damage_multiplier", SelectedBounty.m_damageMultiplier.ToString(CultureInfo.CurrentCulture) },
            { "Purchase Currency", SelectedBounty.m_currency.m_itemData.m_shared.m_name},
            { "Purchase Cost", SelectedBounty.m_cost == 0 ? "Free" : SelectedBounty.m_cost.ToString()}
        };

        Dictionary<string, string> BountyDamages = new()
        {
            { "$almanac_damages", "title" },
            { "$almanac_blunt", SelectedBounty.m_damages.blunt.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_slash", SelectedBounty.m_damages.slash.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_pierce", SelectedBounty.m_damages.pierce.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_fire", SelectedBounty.m_damages.fire.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_frost", SelectedBounty.m_damages.frost.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_lightning", SelectedBounty.m_damages.lightning.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_poison", SelectedBounty.m_damages.poison.ToString(CultureInfo.CurrentCulture) },
            { "$almanac_spirit", SelectedBounty.m_damages.spirit.ToString(CultureInfo.CurrentCulture) },
        };
        
        MergeDictionaries(output, BountyData);
        if (HasBountyDamages()) MergeDictionaries(output, BountyDamages);
        
        foreach (KeyValuePair<string, string> kvp in output)
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
        
        GameObject tab = Object.Instantiate(BaseTab, CreateAlmanac.PanelContent);
        Transform text = Utils.FindChild(tab.transform, "text");
        if (text.TryGetComponent(out TextMeshProUGUI textMesh))
        {
            if (Bounty.ActiveBountyLocation != null)
            {
                textMesh.text = Localization.instance.Localize(Bounty.ActiveBountyLocation.m_critter == SelectedBounty.m_critter 
                    ? "$almanac_cancel_bounty" : "$almanac_accept_bounty");
            }
            else
            {
                textMesh.text = Localization.instance.Localize("$almanac_accept_bounty");
            }
            
        }
        if (tab.TryGetComponent(out Button button))
        {
            button.onClick.AddListener(Bounty.OnClickBounty);
        }
        
        PanelElements.Add(tab);
    }
    private static bool HasBountyDamages()
    {
        return SelectedBounty.m_damages.blunt > 0 ||
               SelectedBounty.m_damages.slash > 0 ||
               SelectedBounty.m_damages.pierce > 0 ||
               SelectedBounty.m_damages.fire > 0 ||
               SelectedBounty.m_damages.frost > 0 ||
               SelectedBounty.m_damages.lightning > 0 ||
               SelectedBounty.m_damages.poison > 0 ||
               SelectedBounty.m_damages.spirit > 0;
    }
    public static void UpdateTreasurePanel()
    {
        DestroyPanelElements();
        CreateAlmanac.PanelIcon.sprite = SelectedTreasure.m_sprite;
        CreateAlmanac.PanelTitle.text = SelectedTreasure.m_name;

        Dictionary<string, string> output = new()
        {
            { "$almanac_reward", "title" },
        };

        for (int index = 0; index < SelectedTreasure.m_dropTable.m_drops.Count; index++)
        {
            var drop = SelectedTreasure.m_dropTable.m_drops[index];
            string name = drop.m_item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
            string amount = $"{drop.m_stackMin} - {drop.m_stackMax}";
            
            output.Add(name + " " + index, amount);
        }

        Dictionary<string, string> BountyData = new Dictionary<string, string>()
        {
            { "$almanac_treasure_info", "title" },
            { "$almanac_biome", SelectedTreasure.m_biome.ToString() },
            { "Wishbone Pings", ConvertBoolean(true) },
            { "Purchase Item", SelectedTreasure.m_currency.m_itemData.m_shared.m_name },
            { "Purchase Cost", SelectedTreasure.m_cost == 0 ? "Free" : SelectedTreasure.m_cost.ToString() }
        };
        
        MergeDictionaries(output, BountyData);

        foreach (KeyValuePair<string, string> kvp in output)
        {
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
        
        
        GameObject tab = Object.Instantiate(BaseTab, CreateAlmanac.PanelContent);
        Transform text = Utils.FindChild(tab.transform, "text");
        if (text.TryGetComponent(out TextMeshProUGUI textMesh))
        {
            textMesh.overflowMode = TextOverflowModes.Overflow;
            textMesh.textWrappingMode = TextWrappingModes.NoWrap;
            if (TreasureHunt.TreasureHunt.ActiveTreasureLocation != null)
            {
                textMesh.text = Localization.instance.Localize(TreasureHunt.TreasureHunt.ActiveTreasureLocation.m_data.m_name == SelectedTreasure.m_name
                    ? "$almanac_cancel_treasure" : "$almanac_accept_treasure");
            }
            else
            {
                textMesh.text = Localization.instance.Localize("$almanac_accept_treasure");
            }
            
        }
        if (tab.TryGetComponent(out Button button))
        {
            button.onClick.AddListener(TreasureHunt.TreasureHunt.OnClickTreasure);
        }
        
        PanelElements.Add(tab);
        
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
        CreateAlmanac.AchievementPanelTooltip.alignment = TextAlignmentOptions.Center;
        switch (SelectedAchievement.m_rewardType)
        {
            case AchievementTypes.AchievementRewardType.Item:
                if (SelectedAchievement.m_collectedReward)
                {
                    CreateAlmanac.AchievementPanelTooltip.text = Localization.instance.Localize("$almanac_collected_reward_already");
                    break;
                }
                CreateAlmanac.AchievementPanelTooltip.text = SelectedAchievement.m_item != null 
                    ? $"{Localization.instance.Localize(SelectedAchievement.m_item.m_shared.m_name)} x<color=orange>{SelectedAchievement.m_item_amount}</color>" 
                    : "<color=red>Failed to find item</color>";
                break;
            case AchievementTypes.AchievementRewardType.Skill:
                if (SelectedAchievement.m_collectedReward)
                {
                    CreateAlmanac.AchievementPanelTooltip.text = Localization.instance.Localize("$almanac_collected_reward_already");
                    break;
                }
                if (SelectedAchievement.m_skill is not Skills.SkillType.None)
                {
                    CreateAlmanac.AchievementPanelTooltip.text =
                        $"+<color=orange>{SelectedAchievement.m_skillAmount}</color>XP {SplitCamelCase(SelectedAchievement.m_skill.ToString())}";
                }
                break;
            case AchievementTypes.AchievementRewardType.StatusEffect:
                if (SelectedAchievement.m_statusEffect != null) CreateAlmanac.AchievementPanelTooltip.text = SelectedAchievement.m_statusEffect.m_tooltip;
                break;
        }

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
                SelectedAchievement.m_isCompleted = loreCount >= SelectedAchievement.m_goal;
                UpdateAchievementList(loreCount, SelectedAchievement.m_goal);
                break;
            case AchievementTypes.AchievementType.Recipes:
                int recipeCount = GetKnownRecipeCount();
                CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(recipeCount, SelectedAchievement.m_goal);
                SelectedAchievement.m_isCompleted = recipeCount >= SelectedAchievement.m_goal;
                UpdateAchievementList(recipeCount, SelectedAchievement.m_goal);
                break;
            case AchievementTypes.AchievementType.CustomKills:
                if (!LocalPlayerData.Player_Kill_Deaths.TryGetValue(SelectedAchievement.m_defeatKey, out KillDeaths value))
                {
                    CreateAlmanac.AchievementPanelDesc.text = "<color=red>Failed to find defeat key</color>";
                    break;
                }
                int kills = value.kills;
                CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(kills, SelectedAchievement.m_goal);
                SelectedAchievement.m_isCompleted = kills >= SelectedAchievement.m_goal;
                UpdateAchievementList(kills, SelectedAchievement.m_goal);
                break;
            case AchievementTypes.AchievementType.CustomPickable:
                if (!LocalPlayerData.Player_Pickable_Data.TryGetValue(SelectedAchievement.m_customPickable, out int pickableValue))
                {
                    CreateAlmanac.AchievementPanelDesc.text = "<color=red>Failed to find pickable value</color>";
                    break;
                }
                CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(pickableValue, SelectedAchievement.m_goal);
                SelectedAchievement.m_isCompleted = pickableValue >= SelectedAchievement.m_goal;
                UpdateAchievementList(pickableValue, SelectedAchievement.m_goal);
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
            case AchievementTypes.AchievementType.CustomCreatureGroups:
                SetAchievementPanel(CreatureLists.GetCustomCreatureGroup(SelectedAchievement.m_CustomGroupKey));
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
        SelectedAchievement.m_isCompleted = count >= list.Count;
        UpdateAchievementList(count, list.Count);
    }
    private static void SetAchievementPanel(List<ItemDrop> list)
    {
        int count = list.FindAll(x => Player.m_localPlayer.IsKnownMaterial(x.m_itemData.m_shared.m_name)).Count;
        int total = list.Count;
        CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(count, total);
        SelectedAchievement.m_isCompleted = count >= total;
        UpdateAchievementList(count, total);
    }
    private static void SetAchievementPanel(PlayerStatType type)
    {
        int count = (int)GetPlayerStat(type);
        CreateAlmanac.AchievementPanelDesc.text = FormatProgressText(count, SelectedAchievement.m_goal);
        SelectedAchievement.m_isCompleted = count >= SelectedAchievement.m_goal;
        UpdateAchievementList(count, SelectedAchievement.m_goal);
    }
    private static string FormatProgressText(float value, float goal) => $"<color=orange>{value}</color> / <color=orange>{goal}</color> (<color=orange>{((value / goal) * 100):0.0}</color>%)";
    private static Achievement? FindAchievement()
    {
        Achievement achievement = AchievementList.Find(item => item.m_uniqueName == SelectedAchievement.m_uniqueName);
        if (achievement == null)
        {
            Achievement groupAchievement = GroupedAchievements.Find(item => item.m_uniqueName == SelectedAchievement.m_uniqueName);
            if (groupAchievement != null) return groupAchievement;
        }

        return achievement;
    }
    private static void UpdateAchievementList(int count, int total)
    {
        Achievement? achievement = FindAchievement();
        if (achievement == null) return;
        achievement.m_isCompleted = count >= total;
    }
    private static void UpdateItemPanel()
    {
        DestroyPanelElements();
        if (!SelectedItemDrop) return;
        
        ItemDrop.ItemData itemData = SelectedItemDrop.m_itemData;
        Sprite? icon;
        try { icon = itemData.GetIcon(); }
        catch {icon = SpriteManager.AlmanacIcon;}
        CreateAlmanac.PanelIcon.sprite = icon;
        CreateAlmanac.PanelTitle.text = Localization.instance.Localize(itemData.m_shared.m_name);
        
        AddItemRecipe(itemData);

        foreach (var entry in Entries.GetItemEntries(itemData, SelectedItemDrop))
        {
            if (IsValueToBeIgnored(entry.value)) continue;
            if (entry.value == "title")
            {
                GameObject data = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!data.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) continue;
                component.text = Localization.instance.Localize(entry.title);
                PanelElements.Add(data);
            }
            else
            {
                GameObject data = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                if (!Utils.FindChild(data.transform, "$part_infoType").TryGetComponent(out TextMeshProUGUI TypeComponent)) continue;
                if (!Utils.FindChild(data.transform, "$part_data").GetComponent<TextMeshProUGUI>().TryGetComponent(out TextMeshProUGUI DataComponent)) continue;
                
                TypeComponent.text = Localization.instance.Localize(entry.title);
                DataComponent.text = Localization.instance.Localize(entry.value);
    
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

        foreach (var entry in Entries.GetPieceEntries(SelectedPiece))
        {
            if (IsValueToBeIgnored(entry.value)) continue;
            if (entry.value == "title")
            {
                GameObject data = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!data.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) return;

                component.text = Localization.instance.Localize(RemoveNumbers(entry.title));
                
                PanelElements.Add(data);
            }
            else
            {
                GameObject data = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                if (!Utils.FindChild(data.transform, "$part_infoType").TryGetComponent(out TextMeshProUGUI TypeComponent)) continue;
                if (!Utils.FindChild(data.transform, "$part_data").GetComponent<TextMeshProUGUI>().TryGetComponent(out TextMeshProUGUI DataComponent)) continue;
                
                TypeComponent.text = Localization.instance.Localize(RemoveNumbers(entry.title));
                DataComponent.text = Localization.instance.Localize(entry.value);

                PanelElements.Add(data);
            }
        }
        
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

        foreach (var entry in Entries.GetCreatureEntries(SelectedCreature))
        {
            if (IsValueToBeIgnored(entry.value)) continue;
            if (entry.value == "title")
            {
                GameObject item = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!item.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) return;
                component.text = Localization.instance.Localize(RemoveNumbers(entry.title));
                PanelElements.Add(item);
            }
            else
            {
                GameObject item = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                Utils.FindChild(item.transform, "$part_infoType").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(entry.title);
                Utils.FindChild(item.transform, "$part_data").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(entry.value);
                
                PanelElements.Add(item);
            }
        }

        AddCreatureDrops();

        foreach (var entry in Entries.GetCreatureEntries_1(SelectedCreature))
        {
            if (IsValueToBeIgnored(entry.value)) continue;
            if (entry.value == "title")
            {
                GameObject item = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!item.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) return;
                component.text = Localization.instance.Localize(entry.title);
                PanelElements.Add(item);
            }
            else
            {
                GameObject item = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                Utils.FindChild(item.transform, "$part_infoType").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(entry.title);
                Utils.FindChild(item.transform, "$part_data").GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(entry.value);
                
                PanelElements.Add(item);
            }
        }

        AddCreatureConsumeItems();
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

        foreach (Entries.Entry entry in Entries.GetMetricEntries())
        {
            if (entry.value == "title")
            {
                GameObject data = Object.Instantiate(CacheAssets.ItemTitle, CreateAlmanac.PanelContent);
                if (!data.transform.Find("$part_text").TryGetComponent(out TextMeshProUGUI component)) continue;
                component.text = Localization.instance.Localize(entry.title);
                PanelElements.Add(data);
            }
            else
            {
                GameObject data = Object.Instantiate(CacheAssets.Item, CreateAlmanac.PanelContent);
                if (!Utils.FindChild(data.transform, "$part_infoType").TryGetComponent(out TextMeshProUGUI TypeComponent)) continue;
                if (!Utils.FindChild(data.transform, "$part_data").GetComponent<TextMeshProUGUI>().TryGetComponent(out TextMeshProUGUI DataComponent)) continue;

                TypeComponent.text = Localization.instance.Localize(entry.title);
                DataComponent.text = Localization.instance.Localize(entry.value);
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
    private static bool IsValueToBeIgnored(string input) => ValuesToIgnore.Contains(RemoveArrows(Localization.instance.Localize(input)).ToLower().Replace(" ", "")) || input.IsNullOrWhiteSpace();
    private static string RemoveArrows(string input) => Regex.Replace(input, "<.*?>", "");

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
            HideAlmanac();
        }
    }

    private static void HideAlmanac()
    {
        CreateAlmanac.AlmanacGUI.SetActive(false);
        CreateAlmanac.AchievementGUI.SetActive(false);
    }
    public static void UpdateGUI()
    {
        if (!Player.m_localPlayer) return;
        // Check if certain keys are hit to close Almanac GUI
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Player.m_localPlayer.IsDead())
        {
            if (AreTabsVisible() || CreateAlmanac.IsPanelActive() || CreateAlmanac.IsAchievementActive())
            {
                DestroyTabs();
                HideAlmanac();
                InventoryGui.instance.Hide();
            }
        }
        // Hotkey to open almanac
        if (Input.GetKeyDown(_AlmanacHotKey.Value))
        {
            if (!InventoryGui.instance) return;
            if (CreateAlmanac.IsPanelActive())
            {
                HideAlmanac();
                InventoryGui.instance.Hide();
                Categories.DestroyTabs();
            }
            else
            {
                InventoryGui.instance.Show(null);
                CreateAlmanac.AlmanacGUI.SetActive(true);
                InventoryGui.instance.m_trophiesPanel.SetActive(true);
                Categories.CreateTabs();
                PlayerStats.UpdatePlayerStats();
                UpdateAlmanac.UpdateMetricsPanel();
                
                UpdateAlmanac.UpdateTopic();
                UpdateAlmanac.UpdateList(InventoryGui.instance);
            
                if (UpdateAlmanac.CheckedCompletion) return;
                
                AchievementManager.CheckCompletedAchievements();
                UpdateAlmanac.CheckedCompletion = true;
            }
        }
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

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    private static class AlmanacHidePatch
    {
        private static bool Prefix() => !CreateAlmanac.IsAchievementActive() || !CreateAlmanac.IsPanelActive();
    }
}