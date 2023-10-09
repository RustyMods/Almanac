    using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.MonoBehaviors;
    using BepInEx.Logging;
    using HarmonyLib;
    using TMPro;
    using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;
    using Object = UnityEngine.Object;
    using Vector2 = UnityEngine.Vector2;

    namespace Almanac.UI;

public static class Almanac
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    public static class CreateAlmanac
    {
        private static GameObject trophyElement = null!;
        private static Transform TrophiesFrame = null!;
        private static Transform TrophiesPanel = null!;
        private static Button closeButtonScript = null!;
        private static ButtonSfx closeButtonSfx = null!;
        private static TMP_FontAsset font = null!;
        private static Image closeButtonImage = null!;
        public static List<ItemDrop> materials = null!;
        public static List<ItemDrop> consummables = null!;
        public static List<ItemDrop> gear = null!;
        public static List<ItemDrop> weapons = null!;
        public static List<ItemDrop> fish = null!;
        public static List<ItemDrop> ammunitions = null!;
        public static List<CreatureDataCollector.CreatureData> creatures = null!;
        private static Image borderImage = null!;
        private static Image iconBg = null!;
        private static Image weightIcon = null!;
        private static Image armorIcon = null!;
        private static TextMeshProUGUI cloneTextMesh = null!;
        public static List<GameObject> buildables = null!;
        public static List<GameObject> cookingStations = null!;
        public static List<GameObject> fermentingStations = null!;
        public static List<GameObject> smelterStations = null!;

        private static RectTransform creatureRectTransform = null!;
        private static RectTransform materialRectTransform = null!;
        private static RectTransform consumeRectTransform = null!;
        private static RectTransform equipmentRectTransform = null!;
        private static RectTransform weaponRectTransform = null!;
        private static RectTransform projectileRectTransform = null!;
        private static RectTransform fishRectTransform = null!;

        public static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            
            trophyElement = __instance.m_trophieElementPrefab;
            TrophiesPanel = __instance.m_trophiesPanel.transform;
            TrophiesFrame = TrophiesPanel.Find("TrophiesFrame");

            var closeButton = TrophiesFrame.Find("Closebutton");
            var closeButtonText = closeButton.Find("Text");
            var TextMeshProUGui = closeButtonText.GetComponent<TextMeshProUGUI>();
            cloneTextMesh = TextMeshProUGui;
            font = TextMeshProUGui.font;
            closeButtonScript = closeButton.GetComponent<Button>();
            closeButtonSfx = closeButton.GetComponent<ButtonSfx>();
            closeButtonImage = closeButton.GetComponent<Image>();
            iconBg = trophyElement.transform.Find("icon_bkg").gameObject.GetComponent<Image>();
            materials = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, "");
            creatures = CreatureDataCollector.CollectAndSaveCreatureData();
            consummables = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, "");
            fish = fish = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Fish, "");
            
            var oneHanded = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.OneHandedWeapon, "");
            var bow = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Bow, "");
            var shield = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Shield, "");
            var helmet = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Helmet, "");
            var chest = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Chest, "");
            var ammo = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Ammo, "");
            var customization = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "");
            var legs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Legs, "");
            var hands = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Hands, "");
            var twoHanded = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.TwoHandedWeapon, "");
            var torch = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Torch, "");
            var misc = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Misc, "");
            var shoulder = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Shoulder, "");
            var utility = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Utility, "");
            var tool = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Tool, "");
            var attachAtgeir = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Attach_Atgeir, "");
            var twoHandedLeft = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft, "");
            var ammoNonEquip = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.AmmoNonEquipable, "");
            
            
            List<ItemDrop> gearList = new List<ItemDrop>();
            gearList.AddRange(helmet);
            gearList.AddRange(chest);
            gearList.AddRange(legs);
            gearList.AddRange(shoulder);
            gearList.AddRange(utility);
            gearList.AddRange(customization);
            gearList.AddRange(misc);
            
            List<ItemDrop> weaponList = new List<ItemDrop>();
            weaponList.AddRange(oneHanded);
            weaponList.AddRange(bow);
            weaponList.AddRange(shield);
            weaponList.AddRange(twoHanded);
            weaponList.AddRange(hands);
            weaponList.AddRange(torch);
            weaponList.AddRange(tool);
            weaponList.AddRange(attachAtgeir);
            weaponList.AddRange(twoHandedLeft);
            
            List<ItemDrop> ammunition = new List<ItemDrop>();
            ammunition.AddRange(ammo);
            ammunition.AddRange(ammoNonEquip);
            
            weapons = GetValidItemDropList(weaponList);
            gear = new List<ItemDrop>(GetValidItemDropList(gearList).OrderBy(name => Localization.instance.Localize(name.m_itemData.m_shared.m_name)));
            ammunitions = GetValidItemDropList(ammunition);
            borderImage = TrophiesFrame.Find("border (1)").GetComponent<Image>();
            weightIcon = __instance.m_player.Find("Weight").Find("weight_icon").GetComponent<Image>();
            armorIcon = __instance.m_player.Find("Armor").Find("armor_icon").GetComponent<Image>();

            GameObject[] AllObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> Pieces = new List<GameObject>();
            List<GameObject> cookeries = new List<GameObject>();
            List<GameObject> fermenters = new List<GameObject>();
            List<GameObject> smelters = new List<GameObject>();
            foreach (GameObject GO in AllObjects)
            {
                Piece pieceScript;
                CookingStation cookingStationScript;
                Fermenter fermenterScript;
                Smelter smelterScript;
                GO.TryGetComponent(out pieceScript);
                GO.TryGetComponent(out cookingStationScript);
                GO.TryGetComponent(out fermenterScript);
                GO.TryGetComponent(out smelterScript);
                if (pieceScript != null) Pieces.Add(GO);
                if (cookingStationScript != null) cookeries.Add(GO);
                if (fermenterScript != null) fermenters.Add(GO);
                if (smelterScript != null) smelters.Add(GO);
            }

            buildables = Pieces;
            cookingStations = cookeries;
            fermentingStations = fermenters;
            smelterStations = smelters;

            RepositionTrophyPanel(-220f, 0f);
            CreateAlmanacPanel();
            // CreateRegenerateFileButton();
            CreateCreaturesPanel();
            CreateMaterialPanel();
            CreatePanel("consummable", consummables);
            CreatePanel("gear", gear);
            CreatePanel("weapon", weapons);
            CreatePanel("ammo", ammunitions);
            CreatePanel("fish", fish);
            
            CreateTabs("fishButton", "fish", -760f, 425f);
            CreateTabs("ammoButton", "ammo", -605f, 425f);
            CreateTabs("weaponButton", "weapon", -450f, 425f);
            CreateTabs("gearButton", "gear", -295f, 425f);
            CreateTabs("ConsummableButton", "consummable", -140f, 425f);
            CreateTabs("MaterialButton", "material", 15f, 425f);
            CreateTabs("TrophiesButton", "trophies", 170f, 425f);
            CreateTabs("CreatureButton", "creature", 325f, 425f);
        }

        private static List<ItemDrop> GetValidItemDropList(List<ItemDrop> list)
        {
            List<ItemDrop> output = new List<ItemDrop>();
            for (int i = 0; i < list.Count; ++i)
            {
                try
                {
                    ItemDrop data = list[i];
                    var sprite = data.m_itemData.m_shared.m_icons[0];
                    output.Add(data);
                }
                catch (IndexOutOfRangeException)
                {
                    // AlmanacPlugin.AlmanacLogger.Log(LogLevel.Warning, $"invalid item drop data: {i}");
                }
            }
            return output;
        }
        private static void CreateRegenerateFileButton()
        {
            GameObject RegenerateButton = new GameObject("RegenerateButton");
            RectTransform buttonRectTransform = RegenerateButton.AddComponent<RectTransform>();
            buttonRectTransform.SetParent(TrophiesPanel);
            buttonRectTransform.anchoredPosition = new Vector2(660f, -425f);
            buttonRectTransform.sizeDelta = new Vector2(170f, 46f);

            Image regenerateImage = RegenerateButton.AddComponent<Image>();
            regenerateImage.sprite = closeButtonImage.sprite;
            regenerateImage.color = closeButtonImage.color;
            regenerateImage.material = closeButtonImage.material;
            regenerateImage.raycastTarget = true;
            regenerateImage.maskable = true;
            regenerateImage.type = closeButtonImage.type;
            regenerateImage.fillCenter = true;
            regenerateImage.pixelsPerUnitMultiplier = 1f;

            Button button = RegenerateButton.AddComponent<Button>();
            button.interactable = true;
            button.targetGraphic = regenerateImage;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                CreatureDataCollector.CollectAndSaveCreatureData();
            });

            GameObject text = new GameObject("RegenerateText");
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.SetParent(RegenerateButton.transform);
            textRect.anchoredPosition = new Vector2(0f, 0f);
            textRect.sizeDelta = new Vector2(150f, 40f);

            TextMeshProUGUI textContent = text.AddComponent<TextMeshProUGUI>();
            textContent.font = font;
            textContent.text = Localization.instance.Localize("$almanac_regenerate_almanac");
            textContent.fontSize = 17;
            textContent.autoSizeTextContainer = false;
            textContent.alignment = TextAlignmentOptions.MidlineJustified;
            textContent.color = new Color(0.8f, 0.5f, 0f, 1f);
            textContent.overflowMode = TextOverflowModes.ScrollRect;
            textContent.textWrappingMode = TextWrappingModes.Normal;

            var sfx = RegenerateButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }
        private static void CreateTabs(string id, string name, float anchorX, float anchorY)
        {
            GameObject TrophiesButton = new GameObject(id);
            RectTransform buttonRectTransform = TrophiesButton.AddComponent<RectTransform>();
            buttonRectTransform.SetParent(TrophiesPanel);
            buttonRectTransform.anchoredPosition = new Vector2(anchorX, anchorY);
            buttonRectTransform.sizeDelta = new Vector2(150f, 46f);

            Image backgroundImage = TrophiesButton.AddComponent<Image>();
            backgroundImage.sprite = closeButtonImage.sprite;
            backgroundImage.color = closeButtonImage.color;
            backgroundImage.material = closeButtonImage.material;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;
            backgroundImage.type = closeButtonImage.type;
            backgroundImage.fillCenter = true;
            backgroundImage.pixelsPerUnitMultiplier = 1f;

            Button button = TrophiesButton.AddComponent<Button>();
            button.interactable = true;
            button.targetGraphic = backgroundImage;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                SetTopic(name);
            });

            GameObject text = new GameObject($"{name}ButtonText");
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.SetParent(TrophiesButton.transform);
            textRect.anchoredPosition = new Vector2(0f, 0f);
            textRect.sizeDelta = new Vector2(150f, 30f);

            TextMeshProUGUI textContent = text.AddComponent<TextMeshProUGUI>();
            textContent.font = font;
            textContent.text = Localization.instance.Localize($"$almanac_{name}_button");
            textContent.autoSizeTextContainer = false;
            textContent.fontSize = 18;
            textContent.overflowMode = TextOverflowModes.Truncate;
            textContent.color = new Color(0.8f, 0.5f, 0f, 1f);
            textContent.maskable = true;
            textContent.horizontalAlignment = HorizontalAlignmentOptions.Center;
            textContent.verticalAlignment = VerticalAlignmentOptions.Middle;

            var sfx = TrophiesButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }
        private static void SetTopic(string name)
        {
            var topic = TrophiesFrame.Find("topic");
            
            var materialPanel = TrophiesFrame.Find("materialPanel");
            var trophies = TrophiesFrame.Find("Trophies");
            var creaturePanel = TrophiesFrame.Find("creaturePanel");
            var consummablePanel = TrophiesFrame.Find("consummablePanel");
            var gearPanel = TrophiesFrame.Find("gearPanel");
            var weaponPanel = TrophiesFrame.Find("weaponPanel");
            var ammoPanel = TrophiesFrame.Find("ammoPanel");
            var fishPanel = TrophiesFrame.Find("fishPanel");
            
            topic.gameObject.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize($"$almanac_{name}_button");
            
            materialPanel.gameObject.SetActive(name == "material");
            trophies.gameObject.SetActive(name == "trophies");
            creaturePanel.gameObject.SetActive(name == "creature");
            consummablePanel.gameObject.SetActive(name == "consummable");
            gearPanel.gameObject.SetActive(name == "gear");
            weaponPanel.gameObject.SetActive(name == "weapon");
            ammoPanel.gameObject.SetActive(name == "ammo");
            fishPanel.gameObject.SetActive(name == "fish");
        }
        
        private static void CreatePanel(string id, List<ItemDrop> list)
        {
            var trophies = TrophiesFrame.Find("Trophies");
            
            var panel = new GameObject($"{id}Panel") { layer = 5 };

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            panel.SetActive(false);

            var background = new GameObject($"{id}Background");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(panel.transform);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(1260f, 650f);
            
            Image backgroundImage = background.AddComponent<Image>();
            
            Image trophiesImage = trophies.gameObject.GetComponent<Image>();
            backgroundImage.color = trophiesImage.color;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;

            int pages = Mathf.CeilToInt(list.Count / 72f);
            for (int i = 0; i < list.Count; ++i)
            {
                int wrappedIndex = i % 72;
                int rowIndex = wrappedIndex / 12;
                int colIndex = wrappedIndex % 12;

                float x = -577f + colIndex * 105f;
                float y = 275f - rowIndex * 110f;

                ItemDrop data = list[i];
                Vector2 pos = new Vector2(x, y);
                try
                {
                    CreateContainer(panel.transform, data, i, pos, id);
                }
                catch (IndexOutOfRangeException)
                {
                    // AlmanacPlugin.AlmanacLogger.Log(LogLevel.Warning, $"index out of range: {i}");
                    CreateContainerWithoutSprite(panel.transform, data, i, pos, id);
                }
            }

            for (int i = 0; i < pages; ++i)
            {
                CreatePageButtons(panel.transform, i, 72, $"{id}Container", list);
            }
        }
        private static void CreatePageButtons(Transform parentElement, int index, int pageSize, string id, List<ItemDrop> list)
        {
            GameObject obj = new GameObject($"Button ({index})") { layer = 5 };
            RectTransform objRect = obj.AddComponent<RectTransform>();
            objRect.SetParent(parentElement);
            objRect.anchoredPosition = new Vector2(-615f + (index * 50f), 355f);
            objRect.sizeDelta = new Vector2(50f, 50f);

            GameObject objText = new GameObject($"ButtonText ({index})");
            RectTransform objTextRect = objText.AddComponent<RectTransform>();
            objTextRect.SetParent(obj.transform);
            objTextRect.anchoredPosition = new Vector2(0f, 0f);
            objTextRect.sizeDelta = new Vector2(50f, 50f);

            TextMeshProUGUI objTextMesh = objText.AddComponent<TextMeshProUGUI>();
            objTextMesh.font = font;
            objTextMesh.text = $"{index + 1}";
            objTextMesh.autoSizeTextContainer = false;
            objTextMesh.color = new Color(1f, 0.5f, 0f, 0.8f);
            objTextMesh.richText = true;
            objTextMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
            objTextMesh.verticalAlignment = VerticalAlignmentOptions.Middle;

            Image objImg = obj.AddComponent<Image>();
            objImg.sprite = closeButtonImage.sprite;
            objImg.color = closeButtonImage.color;
            objImg.raycastTarget = true;
            objImg.maskable = true;
            objImg.type = Image.Type.Sliced;
            objImg.fillCenter = true;
            objImg.pixelsPerUnitMultiplier = 1f;

            Button objButton = obj.AddComponent<Button>();
            ButtonSfx objSfx = obj.AddComponent<ButtonSfx>();
            objSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
            
            objButton.interactable = true;
            objButton.transition = Selectable.Transition.SpriteSwap;
            objButton.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            objButton.onClick = new Button.ButtonClickedEvent();
            objButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    try
                    {
                        var element = parentElement.Find($"{id} ({i})");
                        float min = (index * pageSize);
                        float max = min + pageSize;
                        if (i >= min && i < max)
                        {
                            element.gameObject.SetActive(true);
                        }
                        else
                        {
                            element.gameObject.SetActive(false);
                        }
                    }
                    catch (NullReferenceException)
                    {
                        // AlmanacPlugin.AlmanacLogger.Log(LogLevel.Warning, $"null reference exception: {i}");
                    }
                }
            });
        }
        
         private static void CreateContainer(Transform parentElement, ItemDrop data, int index, Vector2 position, string id)
        {
            var sharedData = data.m_itemData.m_shared;
            if (!sharedData.m_icons[0]) return;
            Sprite iconSprite = sharedData.m_icons[0];

            string name = sharedData.m_name;

            GameObject container = new GameObject($"{id}Container ({index})");
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.SetParent(parentElement);
            containerRect.anchoredPosition = position;
            containerRect.sizeDelta = new Vector2(80f, 80f);

            Image image = container.AddComponent<Image>();
            image.sprite = iconBg.sprite;
            image.fillCenter = iconBg.fillCenter;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = iconBg.color;

            GameObject iconObj = new GameObject("iconObj");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.SetParent(container.transform);
            iconRect.anchoredPosition = new Vector2(0f, 0f);
            iconRect.sizeDelta = new Vector2(65f, 65f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = new Color(1f, 1f, 1f, 1f);
            iconImage.pixelsPerUnitMultiplier = 1f;
            
            AddHoverableText(container, name, 16, anchoredY: -45f);
            
            if (index > 71) container.SetActive(false);
            
            Button containerButton = container.AddComponent<Button>();
            ButtonSfx containerButtonSfx = container.AddComponent<ButtonSfx>();
            containerButtonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;

            containerButton.interactable = true;
            containerButton.transition = Selectable.Transition.ColorTint;
            containerButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            containerButton.onClick = new Button.ButtonClickedEvent();
            containerButton.onClick.AddListener(() =>
            {
                var AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                var AlmanacList = AlmanacPanel.Find("AlmanacList");
                var element = AlmanacList.Find($"{id}Element (0)");
                SetActiveElement(id);
                Patches.OnOpenTrophiesPatch.SetItemsData(element.gameObject, data);
            });
        }
         private static void CreateContainerWithoutSprite(Transform parentElement, ItemDrop data, int index, Vector2 position, string id)
        {
            var sharedData = data.m_itemData.m_shared;

            string name = sharedData.m_name;

            GameObject container = new GameObject($"{id}Container ({index})");
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.SetParent(parentElement);
            containerRect.anchoredPosition = position;
            containerRect.sizeDelta = new Vector2(80f, 80f);

            Image image = container.AddComponent<Image>();
            image.sprite = iconBg.sprite;
            image.fillCenter = iconBg.fillCenter;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = iconBg.color;

            GameObject iconObj = new GameObject("iconObj");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.SetParent(container.transform);
            iconRect.anchoredPosition = new Vector2(0f, 0f);
            iconRect.sizeDelta = new Vector2(65f, 65f);

            TextMeshProUGUI iconTextMesh = iconObj.AddComponent<TextMeshProUGUI>();
            iconTextMesh.font = font;
            iconTextMesh.text = "?";
            iconTextMesh.fontSize = 20;
            iconTextMesh.autoSizeTextContainer = false;
            iconTextMesh.color = new Color(1f, 0.5f, 0f, 1f);
            iconTextMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
            iconTextMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
            iconTextMesh.fontSizeMin = 2f;
            iconTextMesh.fontSizeMax = 20f;
            iconTextMesh.enableAutoSizing = true;

            AddHoverableText(container, name, 16, anchoredY: -40f);
            
            if (index > 71) container.SetActive(false);
            
            Button containerButton = container.AddComponent<Button>();
            ButtonSfx containerButtonSfx = container.AddComponent<ButtonSfx>();
            containerButtonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;

            containerButton.interactable = true;
            containerButton.transition = Selectable.Transition.ColorTint;
            containerButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            containerButton.onClick = new Button.ButtonClickedEvent();
            containerButton.onClick.AddListener(() =>
            {
                var AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                var AlmanacList = AlmanacPanel.Find("AlmanacList");
                var element = AlmanacList.Find($"{id}Element (0)");
                SetActiveElement(id);
                Patches.OnOpenTrophiesPatch.SetItemsData(element.gameObject, data);
            });
        }
        private static void CreateMaterialPanel()
        {
            var trophies = TrophiesFrame.Find("Trophies");

            var materialPanel = new GameObject("materialPanel") { layer = 5 };

            RectTransform panelRect = materialPanel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            materialPanel.SetActive(false);

            var background = new GameObject("materialBackground");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(materialPanel.transform);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(1260f, 650f);
            
            Image backgroundImage = background.AddComponent<Image>();
            
            Image trophiesImage = trophies.gameObject.GetComponent<Image>();
            backgroundImage.color = trophiesImage.color;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;

            int pages = Mathf.CeilToInt(materials.Count / 72f);
            
            for (int i = 0; i < materials.Count; ++i)
            {
                int wrappedIndex = i % 72;
                int rowIndex = wrappedIndex / 12;
                int colIndex = wrappedIndex % 12;

                float x = -577f + colIndex * 105f;
                float y = 275f - rowIndex * 110f;
                
                ItemDrop data = materials[i];
                Vector2 pos = new Vector2(x, y);
                CreateMaterialContainer(materialPanel.transform, data, i, pos);
            }

            for (int i = 0; i < pages; ++i)
            {
                CreateMaterialPageButtons(materialPanel.transform, i, 72);
            }
        }
        private static void CreateMaterialPageButtons(Transform parentElement, int index, int pageSize)
        {
            GameObject obj = new GameObject($"Button ({index})") { layer = 5 };
            RectTransform objRect = obj.AddComponent<RectTransform>();
            objRect.SetParent(parentElement);
            objRect.anchoredPosition = new Vector2(-615f + (index * 50f), 355f);
            objRect.sizeDelta = new Vector2(50f, 50f);

            GameObject objText = new GameObject($"ButtonText ({index})");
            RectTransform objTextRect = objText.AddComponent<RectTransform>();
            objTextRect.SetParent(obj.transform);
            objTextRect.anchoredPosition = new Vector2(0f, 0f);
            objTextRect.sizeDelta = new Vector2(50f, 50f);

            TextMeshProUGUI objTextMesh = objText.AddComponent<TextMeshProUGUI>();
            objTextMesh.font = font;
            objTextMesh.text = $"{index + 1}";
            objTextMesh.autoSizeTextContainer = false;
            objTextMesh.color = new Color(1f, 0.5f, 0f, 0.8f);
            objTextMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
            objTextMesh.verticalAlignment = VerticalAlignmentOptions.Middle;

            Image objImg = obj.AddComponent<Image>();
            objImg.sprite = closeButtonImage.sprite;
            objImg.color = closeButtonImage.color;
            objImg.raycastTarget = true;
            objImg.maskable = true;
            objImg.type = Image.Type.Sliced;
            objImg.fillCenter = true;
            objImg.pixelsPerUnitMultiplier = 1f;

            Button objButton = obj.AddComponent<Button>();
            ButtonSfx objSfx = obj.AddComponent<ButtonSfx>();
            objSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
            
            objButton.interactable = true;
            objButton.targetGraphic = objImg;
            objButton.transition = Selectable.Transition.SpriteSwap;
            objButton.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            objButton.onClick = new Button.ButtonClickedEvent();
            objButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < materials.Count; ++i)
                {
                    var element = parentElement.Find($"materialContainer ({i})");
                    float min = (index * pageSize);
                    float max = min + pageSize;
                    if (i >= min && i < max)
                    {
                        element.gameObject.SetActive(true);
                    }
                    else
                    {
                        element.gameObject.SetActive(false);
                    }
                }
            });
        }

        private static void CreateMaterialContainer(Transform parentElement, ItemDrop data, int index, Vector2 position)
        {
            var sharedData = data.m_itemData.m_shared;
            Sprite iconSprite = sharedData.m_icons[0];
            string name = sharedData.m_name;

            GameObject container = new GameObject($"materialContainer ({index})");
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.SetParent(parentElement);
            containerRect.anchoredPosition = position;
            containerRect.sizeDelta = new Vector2(80f, 80f);

            Image image = container.AddComponent<Image>();
            image.sprite = iconBg.sprite;
            image.fillCenter = iconBg.fillCenter;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = iconBg.color;

            GameObject iconObj = new GameObject("iconObj");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.SetParent(container.transform);
            iconRect.anchoredPosition = new Vector2(0f, 0f);
            iconRect.sizeDelta = new Vector2(65f, 65f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = new Color(1f, 1f, 1f, 1f);
            iconImage.pixelsPerUnitMultiplier = 1f;
            
            AddHoverableText(container, name, 18, anchoredY: -45f);
            
            if (index > 71) container.SetActive(false);
            
            Button containerButton = container.AddComponent<Button>();
            ButtonSfx containerButtonSfx = container.AddComponent<ButtonSfx>();
            containerButtonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;

            containerButton.interactable = true;
            containerButton.transition = Selectable.Transition.ColorTint;
            containerButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            containerButton.onClick = new Button.ButtonClickedEvent();
            containerButton.onClick.AddListener(() =>
            {
                var AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                var AlmanacList = AlmanacPanel.Find("AlmanacList");
                var MaterialElement = AlmanacList.Find("materialElement (0)");
                SetActiveElement("material");
                Patches.OnOpenTrophiesPatch.SetItemsData(MaterialElement.gameObject, data);
            });
        }

        private static void SetActiveElement(string name)
        {
            var AlmanacPanel = TrophiesFrame.Find("ContentPanel");
            var AlmanacList = AlmanacPanel.Find("AlmanacList");
            var MaterialElement = AlmanacList.Find("materialElement (0)");
            var AlmanacElement = AlmanacList.Find("AlmanacElement (0)");
            var ConsummableElement = AlmanacList.Find("consummableElement (0)");
            var GearElement = AlmanacList.Find("gearElement (0)");
            var WelcomePanel = AlmanacPanel.Find("WelcomePanel (0)");
            var AmmoElement = AlmanacList.Find("ammoElement (0)");
            var weaponElement = AlmanacList.Find("weaponElement (0)");
            var fishElement = AlmanacList.Find("fishElement (0)");
                
            WelcomePanel.gameObject.SetActive(name == "welcome");
            MaterialElement.gameObject.SetActive(name == "material");
            AlmanacElement.gameObject.SetActive(name == "creatures");
            ConsummableElement.gameObject.SetActive(name == "consummable");
            GearElement.gameObject.SetActive(name == "gear");
            AmmoElement.gameObject.SetActive(name == "ammo");
            weaponElement.gameObject.SetActive(name == "weapon");
            fishElement.gameObject.SetActive(name == "fish");
        }
        private static void CreateCreaturesPanel()
        {
            var trophies = TrophiesFrame.Find("Trophies");
            
            var creaturePanel = new GameObject("creaturePanel") { layer = 5 };

            RectTransform panelRect = creaturePanel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            creaturePanel.SetActive(false);

            var background = new GameObject("creaturesBackground");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(creaturePanel.transform);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(1260f, 650f);
            
            Image backgroundImage = background.AddComponent<Image>();
            
            Image trophiesImage = trophies.gameObject.GetComponent<Image>();
            backgroundImage.color = trophiesImage.color;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;
            
            CreateCreatureContainers(creaturePanel.transform);
        }

        private static void CreateCreaturePageButtons(Transform parentElement, int index)
        {
            GameObject obj = new GameObject($"Button ({index})") { layer = 5 };
            RectTransform objRect = obj.AddComponent<RectTransform>();
            objRect.SetParent(parentElement);
            objRect.anchoredPosition = new Vector2(-615f + (index * 50f), 355f);
            objRect.sizeDelta = new Vector2(50f, 50f);

            GameObject objText = new GameObject($"ButtonText ({index})");
            RectTransform objTextRect = objText.AddComponent<RectTransform>();
            objTextRect.SetParent(obj.transform);
            objTextRect.anchoredPosition = new Vector2(0f, 0f);
            objTextRect.sizeDelta = new Vector2(50f, 50f);

            TextMeshProUGUI objTextMesh = objText.AddComponent<TextMeshProUGUI>();
            objTextMesh.font = font;
            objTextMesh.text = $"{index + 1}";
            objTextMesh.autoSizeTextContainer = false;
            objTextMesh.color = new Color(1f, 0.5f, 0f, 0.8f);
            objTextMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
            objTextMesh.verticalAlignment = VerticalAlignmentOptions.Middle;

            Image objImg = obj.AddComponent<Image>();
            objImg.sprite = closeButtonImage.sprite;
            objImg.color = closeButtonImage.color;
            objImg.raycastTarget = true;
            objImg.maskable = true;
            objImg.type = Image.Type.Sliced;
            objImg.fillCenter = true;
            objImg.pixelsPerUnitMultiplier = 1f;

            Button objButton = obj.AddComponent<Button>();
            ButtonSfx objSfx = obj.AddComponent<ButtonSfx>();
            objSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
            
            objButton.interactable = true;
            objButton.transition = Selectable.Transition.SpriteSwap;
            objButton.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            objButton.onClick = new Button.ButtonClickedEvent();
            objButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < creatures.Count; ++i)
                {
                    var element = parentElement.Find($"CreatureContainer ({i})");
                    float min = (index * 100);
                    float max = min + 100;
                    if (i >= min && i < max)
                    {
                        element.gameObject.SetActive(true);
                    }
                    else
                    {
                        element.gameObject.SetActive(false);
                    }
                }
            });
        }
        private static void CreateCreatureContainers(Transform parentElement)
        {
            var contentPanel = TrophiesFrame.Find("ContentPanel");
            var AlmanacList = contentPanel.Find("AlmanacList");
            var AlmanacElement = AlmanacList.Find("AlmanacElement (0)");
            
            int pages = Mathf.CeilToInt(creatures.Count / 100f);
            for (var i = 0; i < pages; ++i)
            {
                CreateCreaturePageButtons(parentElement, i);
            }

            float xSpacing = 250f;
            float ySpacing = 32f;
            
            for (int i = 0; i < creatures.Count; i++)
            {
                var name = creatures[i].display_name;
                float x = -500f + (i / 20) * xSpacing;
                float y = 305f - (i % 20) * ySpacing;

                if (i > 99)
                {
                    x = -500f + ((i - 100) / 20) * xSpacing;
                    y = 305f - ((i - 100) % 20) * ySpacing;
                }
                
                Vector2 position = new Vector2(x, y);
                CreateCreatureButton(AlmanacElement, parentElement, position, i, Localization.instance.Localize(name), !(i > 99f));
            }
            
        }

        private static void CreateCreatureButton(
            Transform AlmanacElement, Transform parentElement, Vector2 position, int i,
            string content, bool active)
        {
            var width = 250f;
            var height = 32f;
            
            GameObject CreatureContainer = new GameObject($"CreatureContainer ({i})") { layer = 5 };
            CreatureContainer.SetActive(active);
            RectTransform rect = CreatureContainer.AddComponent<RectTransform>();
            rect.SetParent(parentElement);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, height);

            Image ContainerBg = CreatureContainer.AddComponent<Image>();
            ContainerBg.sprite = closeButtonImage.sprite;
            ContainerBg.color = closeButtonImage.color;
            ContainerBg.raycastTarget = true;
            ContainerBg.maskable = true;
            ContainerBg.type = Image.Type.Sliced;
            ContainerBg.fillCenter = true;
            ContainerBg.pixelsPerUnitMultiplier = 1f;

            ButtonSfx buttonSfx = CreatureContainer.AddComponent<ButtonSfx>();
            buttonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;

            Button button = CreatureContainer.AddComponent<Button>();
            button.interactable = true;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };

            GameObject objText = new GameObject($"CreatureContainer Text ({i})") { layer = 5 };
            RectTransform textRect = objText.AddComponent<RectTransform>();
            textRect.SetParent(CreatureContainer.transform);
            textRect.anchoredPosition = new Vector2(0f, 0f);
            textRect.sizeDelta = new Vector2(width, height);
                
            GameObject objImage = new GameObject($"CreatureContainer Image ({i})") { layer = 5 };
            RectTransform ImageRect = objImage.AddComponent<RectTransform>();
            ImageRect.SetParent(CreatureContainer.transform);
            ImageRect.anchoredPosition = new Vector2(0f, 0f);
            ImageRect.sizeDelta = new Vector2(width, height);

            Image image = objImage.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.3f);
            image.pixelsPerUnitMultiplier = 1f;
            image.fillCenter = true;
            image.raycastTarget = true;

            TextMeshProUGUI text = objText.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = content;
            text.color = Color.yellow;
            text.autoSizeTextContainer = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.fontSize = 16;

            button.targetGraphic = ContainerBg;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                SetActiveElement("creature");
                Patches.OnOpenTrophiesPatch.setAlmanacData(AlmanacElement.gameObject, content);
            });
        }

        private static void CreateAlmanacPanel()
        {
            var position = new Vector2(880f, 0f);

            CreateBorder(position);
            GameObject AlmanacContentPanel = CreateContentPanel(position);
            GameObject AlmanacScroll = CreateScrollElement(AlmanacContentPanel.transform, 190f, 0f);
            GameObject AlmanacSlidingArea = CreateSlidingArea(AlmanacScroll.transform);
            GameObject AlmanacScrollHandle = CreateScrollHandle(AlmanacSlidingArea.transform);
            GameObject AlmanacList = CreateList(AlmanacContentPanel.transform);
            
            GameObject AlmanacElement = CreateAlmanacElement(AlmanacList.transform, iconBg);
            
            GameObject MaterialElement = CreateItemElement(AlmanacList.transform, "material");
            GameObject ConsummableElement = CreateItemElement(AlmanacList.transform, "consummable");
            GameObject GearElement = CreateItemElement(AlmanacList.transform, "gear");
            GameObject WeaponElement = CreateItemElement(AlmanacList.transform, "weapon");
            GameObject AmmoElement = CreateItemElement(AlmanacList.transform, "ammo");
            GameObject FishElement = CreateItemElement(AlmanacList.transform, "fish");
            
            MaterialElement.SetActive(false);
            ConsummableElement.SetActive(false);
            GearElement.SetActive(false);
            WeaponElement.SetActive(false);
            AmmoElement.SetActive(false);
            FishElement.SetActive(false);

            creatureRectTransform = AlmanacElement.GetComponent<RectTransform>();
            materialRectTransform = MaterialElement.GetComponent<RectTransform>();
            consumeRectTransform = ConsummableElement.GetComponent<RectTransform>();
            equipmentRectTransform = GearElement.GetComponent<RectTransform>();
            weaponRectTransform = WeaponElement.GetComponent<RectTransform>();
            projectileRectTransform = AmmoElement.GetComponent<RectTransform>();
            fishRectTransform = FishElement.GetComponent<RectTransform>();

            Scrollbar scrollbar = AddScrollbarComponent(AlmanacScroll, AlmanacScrollHandle);
            GameObject AlmanacListRoot = CreateListRoot(AlmanacList.transform);

            AddListScrollRect(AlmanacList, AlmanacListRoot, scrollbar);
            CreateWelcomePanel(AlmanacContentPanel.transform);
        }

        private static void AddListScrollRect(GameObject list, GameObject listRoot, Scrollbar scrollbar)
        {
            ScrollRect ListScrollRect = list.AddComponent<ScrollRect>();
            ListScrollRect.content = listRoot.GetComponent<RectTransform>();
            ListScrollRect.horizontal = false;
            ListScrollRect.vertical = true;
            ListScrollRect.movementType = ScrollRect.MovementType.Clamped;
            ListScrollRect.inertia = false;
            ListScrollRect.scrollSensitivity = 40f;
            ListScrollRect.verticalScrollbar = scrollbar;
            ListScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }

        private static void CreateWelcomePanel(Transform parentElement)
        {
            GameObject WelcomePanel = new GameObject("WelcomePanel (0)");
            RectTransform rectTransform = WelcomePanel.AddComponent<RectTransform>();
            rectTransform.SetParent(parentElement);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(400f, 800f);

            Image image = WelcomePanel.AddComponent<Image>();
            image.sprite = borderImage.sprite;
            image.material = borderImage.material;
            image.raycastTarget = borderImage.raycastTarget;
            image.type = borderImage.type;
            image.fillCenter = borderImage.fillCenter;
            image.pixelsPerUnitMultiplier = 1;

            CreateCustomImageElement(WelcomePanel.transform, "logo", 0f, 0f, 400f, 800f,
                "Almanac.icons.AlmanacLogo.png", true, false, 1f);
        }

        private static void CreateBorder(Vector2 pos)
        {
            GameObject AlmanacBorder = new GameObject("Border");

            RectTransform AlmanacBorderRectTransform = AlmanacBorder.AddComponent<RectTransform>();
            AlmanacBorderRectTransform.SetParent(TrophiesFrame);
            AlmanacBorderRectTransform.anchoredPosition = pos;
            AlmanacBorderRectTransform.sizeDelta = new Vector2(400f, 800f);

            CreateBorderImageClone(AlmanacBorder);
        }

        private static Image CreateBorderImageClone(GameObject GO)
        {
            Image image = GO.AddComponent<Image>();
            image.sprite = borderImage.sprite;
            image.material = borderImage.material;
            image.raycastTarget = borderImage.raycastTarget;
            image.type = borderImage.type;
            image.fillCenter = borderImage.fillCenter;
            image.pixelsPerUnitMultiplier = borderImage.pixelsPerUnitMultiplier;

            return image;
        }

        private static GameObject CreateContentPanel(Vector2 pos)
        {
            GameObject AlmanacContentPanel = new GameObject("ContentPanel");

            RectTransform AlmanacContentPanelRectTransform = AlmanacContentPanel.AddComponent<RectTransform>();
            AlmanacContentPanelRectTransform.SetParent(TrophiesFrame);
            AlmanacContentPanelRectTransform.anchoredPosition = pos;
            AlmanacContentPanelRectTransform.sizeDelta = new Vector2(380f, 780f);

            Image AlmanacContentImage = AlmanacContentPanel.AddComponent<Image>();
            AlmanacContentImage.color = new Color(0f, 0f, 0f, 0.8f);

            return AlmanacContentPanel;
        }

        private static GameObject CreateScrollElement(Transform parentElement, float x, float y)
        {
            GameObject AlmanacScroll = new GameObject("AlmanacScroll");
            RectTransform ScrollRectTransform = AlmanacScroll.AddComponent<RectTransform>();
            ScrollRectTransform.SetParent(parentElement);
            ScrollRectTransform.anchoredPosition = new Vector2(x, y);
            ScrollRectTransform.sizeDelta = new Vector2(10f, 780f);

            Image ScrollImage = AlmanacScroll.AddComponent<Image>();
            ScrollImage.color = new Color(0f, 0f, 0f, 0.2f);

            AlmanacScroll.AddComponent<RectMask2D>();

            return AlmanacScroll;
        }

        private static GameObject CreateSlidingArea(Transform parentElement)
        {
            GameObject AlmanacSlidingArea = new GameObject("AlmanacSlidingArea");
            RectTransform SlidingAreaRectTransform = AlmanacSlidingArea.AddComponent<RectTransform>();
            SlidingAreaRectTransform.SetParent(parentElement.transform);
            SlidingAreaRectTransform.sizeDelta = new Vector2(800f, 780f);
            SlidingAreaRectTransform.anchoredPosition = new Vector2(0f, 0f);

            return AlmanacSlidingArea;
        }

        private static GameObject CreateScrollHandle(Transform parentElement)
        {
            GameObject AlmanacScrollHandle = new GameObject("Handle");
            RectTransform HandleRectTransform = AlmanacScrollHandle.AddComponent<RectTransform>();
            HandleRectTransform.SetParent(parentElement);
            HandleRectTransform.sizeDelta = new Vector2(10f, 50f);
            HandleRectTransform.anchoredPosition = new Vector2(250f, 0f);

            Image HandleImage = AlmanacScrollHandle.AddComponent<Image>();
            HandleImage.sprite = borderImage.sprite;
            HandleImage.color = new Color(1f, 1f, 1f, 0f);
            HandleImage.type = borderImage.type;
            HandleImage.fillCenter = borderImage.fillCenter;
            HandleImage.raycastTarget = true;
            HandleImage.maskable = true;

            return AlmanacScrollHandle;
        }

        private static Scrollbar AddScrollbarComponent(GameObject targetGameObject, GameObject scrollHandle)
        {
            Scrollbar scrollbar = targetGameObject.AddComponent<Scrollbar>();
            scrollbar.name = "AlmanacScrollBar";
            scrollbar.interactable = true;
            scrollbar.transition = Selectable.Transition.ColorTint;
            scrollbar.targetGraphic = scrollHandle.GetComponent<Image>();
            scrollbar.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            scrollbar.handleRect = scrollHandle.GetComponent<RectTransform>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.navigation = Navigation.defaultNavigation;
            scrollbar.size = 1f;
            scrollbar.value = 1f;
            scrollbar.onValueChanged = new Scrollbar.ScrollEvent();

            scrollbar.onValueChanged.AddListener(call: (e) =>
            {
                creatureRectTransform.anchoredPosition =
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                materialRectTransform.anchoredPosition = 
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                consumeRectTransform.anchoredPosition =
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                equipmentRectTransform.anchoredPosition = 
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                weaponRectTransform.anchoredPosition = 
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                projectileRectTransform.anchoredPosition =
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                fishRectTransform.anchoredPosition = 
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
            });

            return scrollbar;
        }

        private static GameObject CreateList(Transform parentElement)
        {
            GameObject AlmanacList = new GameObject("AlmanacList");
            RectTransform ListRectTransform = AlmanacList.AddComponent<RectTransform>();
            ListRectTransform.SetParent(parentElement);
            ListRectTransform.anchoredPosition = new Vector2(0f, 0f);
            ListRectTransform.sizeDelta = new Vector2(390f, 790f);

            AlmanacList.AddComponent<RectMask2D>();

            return AlmanacList;
        }

        private static GameObject CreateListRoot(Transform parentElement)
        {
            GameObject AlmanacListRoot = new GameObject("ListRoot");
            RectTransform almanacListRootRectTransform = AlmanacListRoot.AddComponent<RectTransform>();
            almanacListRootRectTransform.SetParent(parentElement);
            almanacListRootRectTransform.sizeDelta = new Vector2(2000f, 2000f);
            almanacListRootRectTransform.anchoredPosition = new Vector2(0f, 0f);

            return AlmanacListRoot;
        }
        private static GameObject CreateItemElement(Transform parentElement, string id)
        {
            GameObject DummyPanel = new GameObject($"{id}Element (0)");
            RectTransform DummyRectTransform = DummyPanel.AddComponent<RectTransform>();
            DummyRectTransform.SetParent(parentElement);
            DummyRectTransform.anchoredPosition = new Vector2(0f, 0f);
            DummyRectTransform.sizeDelta = new Vector2(390f, 750f);

            Image dummyImage = DummyPanel.AddComponent<Image>();
            dummyImage.fillCenter = true;
            dummyImage.color = new Color(0f, 0f, 0f, 0f);
            dummyImage.raycastTarget = true;
            
            Transform DummyElement = DummyPanel.transform;

            Color gray = new Color(0.5f, 0.5f, 0.5f, 1f);
            Color orange = new Color(1f, 0.6f, 0f, 1f);

            CreateTextElement(
                DummyElement, "Name", "$almanac_no_data", 
                30f, 360f,
                225f, 25f,
                Color.yellow, 25,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                wrapMode: TextWrappingModes.NoWrap
            );
            CreateTextElement(
                DummyElement, "Description", "$almanac_no_data",
                30f, 305f,
                225f, 75f,
                Color.white, 16,
                verticalAlignment: VerticalAlignmentOptions.Top,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                overflowModes: TextOverflowModes.Ellipsis
            );
            CreateImageElement(
                DummyElement, "icon", 
                -145f, 340f, 
                120f, 120f
            );
            
            CreateTextElement(
                DummyElement, "stats", $"$almanac_general_stats", 
                0f, 250f, 
                200f, 100f, 
                Color.white, 20
            );
            
            CreateImageElement(
                DummyElement, "weight_icon",
                120f, 200f,
                50f, 50f,
                true, false, 
                shadow: true,
                sprite: weightIcon.sprite
                );
            GameObject WeightText = CreateTextElement(
                DummyElement, "weight", $"0", 
                120f, 197f, 
                50f, 100f, 
                Color.black, 18
            );
            AddHoverableText(WeightText, "$almanac_weight");
            
            GameObject prefabImage = CreateImageElement(
                DummyElement, "prefabImage",
                -50f, 200f,
                220f, 40f,
                true, false, closeButtonImage.sprite
            );
            Button prefabImageButton = prefabImage.AddComponent<Button>();
            ButtonSfx prefabImageButtonSfx = prefabImage.AddComponent<ButtonSfx>();
            prefabImageButtonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
            
            prefabImageButton.interactable = true;
            prefabImageButton.transition = closeButtonScript.transition;
            prefabImageButton.targetGraphic = prefabImage.GetComponent<Image>();
            prefabImageButton.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            
            prefabImageButton.onClick = new Button.ButtonClickedEvent();
            
            
            CreateTextElement(
                prefabImage.transform, "prefabName", "$almanac_no_data",
                0f, 0f, 
                210f, 40f,
                orange, 18
            );
            AddHoverableText(prefabImage, $"$almanac_prefab_name_label");

            float leftAlignment = -70f;

            CreateItemDataSet(
                DummyElement,
                "maxStackSizeLabel", "$almanac_stack_size_label",
                "maxStackSize", "0/0",
                leftAlignment, 130f
                );

            CreateItemDataSet(
                DummyElement,
                "valueLabel", "$almanac_value_label",
                "value", "0",
                leftAlignment, 105f
                );
            
            CreateItemDataSet(
                DummyElement,
                "qualityLabel", "$almanac_quality_label",
                "quality", "0",
                leftAlignment, 80f
                );
            
            CreateItemDataSet(
                DummyElement,
                "durabilityLabel", "$almanac_durability_label",
                "durability", "0",
                leftAlignment, 55f
                );
            
            CreateTextElement(
                DummyElement, "teleportable", "$almanac_not_teleportable",
                0f, 25f,
                225f, 25f,
                orange, 18
            );
            
            CreateTextElement(
                DummyElement, "recipeTitle", "$almanac_recipe_title",
                0f, 0f,
                150f, 100f,
                Color.white, 20
            );
            CreateTextElement(
                DummyElement, "recipeNull", "$almanac_no_recipe",
                0f, 0f,
                200f, 100f,
                orange, 18,
                active: false
            );

            GameObject craftingStation = CreateImageElement(
                DummyElement, "craftingStation",
                -120f, -50f,
                75f, 75f,
                addHoverableText: true,
                active: false
            );
            AddHoverableText(craftingStation, "$almanac_crafting_station_label", anchoredY: -40f);

            for (int i = 0; i < 5; ++i)
            {
                GameObject ResourceBackground = CreateImageElement(
                    DummyElement, $"recipe ({i})",
                    -50f + (i * 51f), -50f,
                    50f, 50f,
                    addHoverableText: true,
                    sprite: iconBg.sprite,
                    active: false
                );
                CreateImageElement(ResourceBackground.transform, "item",
                    0f, 0f,
                    40f, 40f
                    );
                CreateTextElement(ResourceBackground.transform, $"recipeAmount", "0",
                    0f, 0f,
                    50f, 50f,
                    orange, 14,
                    active: false,
                    verticalAlignment: VerticalAlignmentOptions.Bottom
                    );
            }

            CreateTextElement(
                DummyElement, "foodTitle", "$almanac_food_title",
                0f, -110f,
                150f, 100f,
                Color.white, 20
                );

            CreateItemDataSet(
                DummyElement,
                "healthBonusLabel", "$almanac_health_bonus_label",
                "healthBonus", "0",
                leftAlignment, -140f
                );

            CreateItemDataSet(
                DummyElement,
                "staminaBonusLabel", "$almanac_stamina_bonus_label",
                "staminaBonus", "0",
                leftAlignment, -165f
                );
            
            CreateItemDataSet(
                DummyElement, 
                "eitrBonusLabel", "$almanac_eitr_bonus_label", 
                "eitrBonus", "0",
                leftAlignment, -190f
                );
            
            CreateItemDataSet(
                DummyElement,
                "foodBurnLabel", "$almanac_food_burn_label",
                "foodBurn", "0",
                leftAlignment, -215f
                );
            
            CreateItemDataSet(
                DummyElement,
                "foodRegenLabel", "$almanac_food_regen_label",
                "foodRegen", "0",
                leftAlignment, -240f
                );

            CreateTextElement(
                DummyElement, "consumeEffectDescription", "$almanac_no_data",
                0f, -280f,
                350f, 50f, 
                orange, 16,
                overflowModes: TextOverflowModes.Overflow
            );
            var consumeIcons = new Dictionary<string, string>()
            {
                { "consumeEffectBlunt", "Almanac.icons.sledgeIcon.png" },
                { "consumeEffectSlash", "Almanac.icons.swordIcon.png" },
                { "consumeEffectPierce", "Almanac.icons.arrowIcon.png" },
                { "consumeEffectChop", "Almanac.icons.axeIcon.png" },
                { "consumeEffectPickaxe", "Almanac.icons.pickaxeIcon.png" },
                { "consumeEffectFire", "Almanac.icons.fireIcon.png" },
                { "consumeEffectIce", "Almanac.icons.frostIcon.png" },
                { "consumeEffectLightning", "Almanac.icons.lightningIcon.png" },
                { "consumeEffectPoison", "Almanac.icons.poisonIcon.png" },
                { "consumeEffectSpirit", "Almanac.icons.spiritIcon.png" }
            };
            List<KeyValuePair<string, string>> consumeIconList = consumeIcons.ToList();

            for (var index = 0; index < consumeIcons.Count; index++)
            {
                CreateCustomImageElement(
                    DummyElement, consumeIconList[index].Key,
                    -110f + (index * 25f), -335f,
                    25f, 25f,
                    consumeIconList[index].Value,
                    true,
                    true
                );
            } 
            
            CreateTextElement(
                DummyElement, "statModifiersTitle", "$almanac_stat_modifiers_title",
                0f, -395f,
                150f, 25f,
                Color.white, 20
            );

            CreateItemDataSet(
                DummyElement,
                "movementLabel", "$almanac_movement_modifier_label",
                "movement", "0",
                leftAlignment, -425f
                );
            CreateItemDataSet(
                DummyElement,
                "eitrRegenLabel", "$almanac_eitr_regen_label",
                "eitrRegen", "0",
                leftAlignment, -450f
                );
            CreateItemDataSet(
                DummyElement, 
                "stamModLabel", "$almanac_base_items_stamina_modifier_label",
                "stamMod", "0",
                leftAlignment, -475f
                );

            CreateTextElement(DummyElement, "setName", "$almanac_no_data",
                0f, -500f,
                350f, 50f,
                Color.white, 16
                );
            CreateTextElement(
                DummyElement, "setDescription", "$almanac_no_data",
                0f, -550f,
                350f, 50f,
                orange, 16
                );
            var setIcons = new Dictionary<string, string>()
            {
                { "setEffectBlunt", "Almanac.icons.sledgeIcon.png" },
                { "setEffectSlash", "Almanac.icons.swordIcon.png" },
                { "setEffectPierce", "Almanac.icons.arrowIcon.png" },
                { "setEffectChop", "Almanac.icons.axeIcon.png" },
                { "setEffectPickaxe", "Almanac.icons.pickaxeIcon.png" },
                { "setEffectFire", "Almanac.icons.fireIcon.png" },
                { "setEffectIce", "Almanac.icons.frostIcon.png" },
                { "setEffectLightning", "Almanac.icons.lightningIcon.png" },
                { "setEffectPoison", "Almanac.icons.poisonIcon.png" },
                { "setEffectSpirit", "Almanac.icons.spiritIcon.png" }
            };
            List<KeyValuePair<string, string>> setIconList = setIcons.ToList();

            for (var index = 0; index < setIconList.Count; index++)
            {
                CreateCustomImageElement(
                    DummyElement, setIconList[index].Key,
                    -110f + (index * 25f), -600f,
                    25f, 25f,
                    setIconList[index].Value,
                    true,
                    true
                );
            }

            CreateTextElement(
                DummyElement,
                "modifySkill", "$almanac_no_data",
                0f, -650f,
                350f, 25f,
                Color.white, 16
                );

            GameObject ArmorIconBG = CreateImageElement(
                DummyElement, "armorBg",
                -125f, -700f,
                40f, 40f,
                true,
                true,
                armorIcon.sprite,
                shadow: true
            );
            CreateTextElement(
                ArmorIconBG.transform, "armor", "0",
                0f, 0f,
                35f, 35f,
                Color.black, 16
                );
            CreateTextElement(
                DummyElement,
                "equipmentStats", "$almanac_equipment_title",
                0f, -700f,
                150f, 25f,
                Color.white, 20
                );
            CreateItemDataSet(
                DummyElement, 
                "armorPerLevelLabel", "$almanac_armor_per_level_label",
                "armorPerLevel", "0",
                leftAlignment, -730f
                );
            
            var weaponTypeIcons = new Dictionary<string, string>()
            {
                { "weaponBlunt", "Almanac.icons.sledgeIcon.png" },
                { "weaponSlash", "Almanac.icons.swordIcon.png" },
                { "weaponPierce", "Almanac.icons.arrowIcon.png" },
                { "weaponChop", "Almanac.icons.axeIcon.png" },
                { "weaponPickaxe", "Almanac.icons.pickaxeIcon.png" },
                { "weaponFire", "Almanac.icons.fireIcon.png" },
                { "weaponIce", "Almanac.icons.frostIcon.png" },
                { "weaponLightning", "Almanac.icons.lightningIcon.png" },
                { "weaponPoison", "Almanac.icons.poisonIcon.png" },
                { "weaponSpirit", "Almanac.icons.spiritIcon.png" }
            };
            List<KeyValuePair<string, string>> weaponTypeList = weaponTypeIcons.ToList();

            for (var index = 0; index < weaponTypeList.Count; index++)
            {
                CreateCustomImageElement(
                    DummyElement, weaponTypeList[index].Key,
                    leftAlignment - 30f, -755f - (index * 25f),
                    25f, 25f,
                    weaponTypeList[index].Value,
                    true,
                    true
                );
            }
            
            var weaponTypeTags = new Dictionary<string, string>()
            {
                { "bluntTag", "$almanac_blunt" },
                { "slashTag", "$almanac_slash" },
                { "pierceTag", "$almanac_pierce" },
                { "chopTag", "$almanac_chop" },
                { "pickaxeTag", "$almanac_pickaxe" },
                { "fireTag", "$almanac_fire" },
                { "frostTag", "$almanac_frost" },
                { "lightningTag", "$almanac_lightning" },
                { "poisonTag", "$almanac_poison" },
                { "spiritTag", "$almanac_spirit" },
            };
            List<KeyValuePair<string, string>> weaponTagList = weaponTypeTags.ToList();
            for (var index = 0; index < weaponTagList.Count; index++)
            {
                var localizedValue = Localization.instance.Localize(weaponTagList[index].Value);
                CreateTextElement(
                    DummyElement, weaponTagList[index].Key, localizedValue,
                    leftAlignment + 30f, -755f - (index * 25f),
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Left
                );
            }


            return DummyPanel;
        }

        private static void CreateItemDataSet(Transform parentElement, string labelId, string labelContent,
            string valueId, string valueContent, float anchoredX, float anchoredY)
        {
            CreateTextElement(
                parentElement, labelId, labelContent,
                anchoredX, anchoredY,
                180f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
            );
            CreateTextElement(
                parentElement, valueId, valueContent,
                anchoredX + 200f, anchoredY,
                40f, 40f,
                new Color(1f, 0.5f, 0f, 1f), 16
            );
        }
        private static GameObject CreateAlmanacElement(Transform parentElement, Image iconBackground)
        {
            GameObject DummyPanel = new GameObject("AlmanacElement (0)");
            RectTransform DummyRectTransform = DummyPanel.AddComponent<RectTransform>();
            DummyRectTransform.SetParent(parentElement);
            DummyRectTransform.anchoredPosition = new Vector2(0f, 0f);
            DummyRectTransform.sizeDelta = new Vector2(390f, 750f);

            Image dummyImage = DummyPanel.AddComponent<Image>();
            dummyImage.fillCenter = true;
            dummyImage.color = new Color(0f, 0f, 0f, 0f);
            dummyImage.raycastTarget = true;

            Transform DummyElement = DummyPanel.transform;

            CreateTextElement(
                DummyElement, "displayName", "$almanac_untitled",
                50f, 345f,
                250f, 50f,
                Color.yellow, 25,
                overflowModes: TextOverflowModes.ScrollRect,
                horizontalAlignment: HorizontalAlignmentOptions.Left
            );
            CreateTextElement(
                DummyElement, "faction", "$almanac_factionless", 
                0f, 310f, 
                150f, 50f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
            );
            CreateTextElement(
                DummyElement, "health", "1000 HP",
                120f, 310f,
                100f, 50f,
                Color.white, 20
            );
            CreateImageElement(
                DummyElement, "icon",
                -145f, 340f,
                120f, 120f
                );
            
            CreateCustomImageElement(
                DummyElement, "questionMark",
                -145f, 340f,
                120f, 120f,
                "Almanac.icons.QuestionMark.png");

            float anchorX = -110f;

            CreateTextElement(
                DummyElement, "creatureStats", "$almanac_creature_resistances",
                0f, 250f,
                200f, 50f,
                Color.white, 20,
                active: true,
                overflowModes: TextOverflowModes.Overflow
            );

            float iconXPos = anchorX - 50f;
            var iconElements = new Dictionary<string, string>()
            {
                { "sledgeIcon", "Almanac.icons.sledgeIcon.png" },
                { "swordIcon", "Almanac.icons.swordIcon.png" },
                { "arrowIcon", "Almanac.icons.arrowIcon.png" },
                { "axeIcon", "Almanac.icons.axeIcon.png" },
                { "pickaxeIcon", "Almanac.icons.pickaxeIcon.png" },
                { "fireIcon", "Almanac.icons.fireIcon.png" },
                { "frostIcon", "Almanac.icons.frostIcon.png" },
                { "lightningIcon", "Almanac.icons.lightningIcon.png" },
                { "poisonIcon", "Almanac.icons.poisonIcon.png" },
                { "spiritIcon", "Almanac.icons.spiritIcon.png" }
            };
            List<KeyValuePair<string, string>> iconList = iconElements.ToList();

            for (var index = 0; index < iconElements.Count; index++)
            {
                CreateCustomImageElement(
                    DummyElement, iconList[index].Key,
                    iconXPos, 230f - (index * 25f),
                    25f, 25f,
                    iconList[index].Value,
                    active: true
                    );
            }

            var creatureStatsTags = new Dictionary<string, string>()
            {
                { "bluntTag", "$almanac_blunt" },
                { "slashTag", "$almanac_slash" },
                { "pierceTag", "$almanac_pierce" },
                { "chopTag", "$almanac_chop" },
                { "pickaxeTag", "$almanac_pickaxe" },
                { "fireTag", "$almanac_fire" },
                { "frostTag", "$almanac_frost" },
                { "lightningTag", "$almanac_lightning" },
                { "poisonTag", "$almanac_poison" },
                { "spiritTag", "$almanac_spirit" },
            };
            List<KeyValuePair<string, string>> statsTagList = creatureStatsTags.ToList();
            for (var index = 0; index < statsTagList.Count; index++)
            {
                float statsTagX = anchorX + 30f;
                var localizedValue = Localization.instance.Localize(statsTagList[index].Value);
                CreateTextElement(
                    DummyElement, statsTagList[index].Key, localizedValue,
                    statsTagX, 230f - (index * 25f),
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Left
                    );
            }

            // Create values
            var statsList = new List<string>()
            {
                "blunt",
                "slash",
                "pierce",
                "chop",
                "pickaxe",
                "fire",
                "frost",
                "lightning",
                "poison",
                "spirit"
            };
            for (var index = 0; index < statsList.Count; index++)
            {
                float statListX = anchorX + 210f;
                CreateTextElement(
                    DummyElement, statsList[index], "$almanac_unknown",
                    statListX, 230f - (index * 25f),
                    150f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Left
                    );
            }

            CreateTextElement(
                DummyElement, "creatureDrops", "$almanac_creature_drops",
                0f, -55f,
                200f, 50f,
                Color.white, 20,
                overflowModes: TextOverflowModes.Overflow
            );

            for (var index = 0; index < 7; index++)
            {
                GameObject dropBackground = CreateImageElement(
                    DummyElement, $"dropIconBg ({index})",
                    -155f + (index * 52), -100f,
                    50f, 50f,
                    sprite: iconBackground.sprite
                    );

                CreateImageElement(
                    dropBackground.transform, $"creatureDrop ({index})",
                    0f, 0f,
                    45f, 45f,
                    false,
                    true,
                    shadow: true
                    );
            }

            CreateTextElement(
                DummyElement, "defaultAttacks", "$almanac_creature_attacks",
                0f, -170f,
                200f, 50f,
                Color.white, 20,
                overflowModes: TextOverflowModes.Overflow
            );

            for (var index = 0; index < 4; index++)
                CreateDefaultAttackStats(
                    DummyElement, index,
                    anchorX - 5f, -170f
                    );

            CreateTextElement(
                DummyElement, "intelligence", "$almanac_creature_intelligence",
                0f, -1015f,
                200f, 50f,
                Color.white, 20,
                overflowModes: TextOverflowModes.Overflow
            );

            CreateAIStats(
                DummyElement,
                anchorX - 5f, -1050f,
                iconBackground
                );

            return DummyPanel;
        }

        private static void CreateAIStats(Transform parentElement, float x, float y, Image iconBackground)
        {
            CreateTextElement(
                parentElement, "avoidFireTag", "$almanac_avoid_fire",
                x + 50f, y,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );
            CreateTextElement(
                parentElement, "afraidOfFireTag", "$almanac_afraid_of_fire",
                x + 50f, y - 25f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );
            CreateTextElement(
                parentElement, "avoidWaterTag", "$almanac_avoid_water",
                x + 50f, y - 50f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );

            CreateTextElement(
                parentElement, "avoidFire", "False",
                x + 240f, y,
                100f, 25f,
                 Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );
            CreateTextElement(
                parentElement, "afraidOfFire", "False",
                x + 240f, y - 25f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right);
            CreateTextElement(
                parentElement, "avoidWater", "False",
                x + 240f, y - 50f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );

            CreateTextElement(
                parentElement, "tolerateWaterTag", "$almanac_tolerate_water",
                x + 50f, y - 75f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );
            CreateTextElement(
                parentElement, "tolerateSmokeTag", "$almanac_tolerate_smoke",
                x + 50f, y - 100f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );
            CreateTextElement(
                parentElement, "tolerateTarTag", "$almanac_tolerate_tar",
                x + 50f, y - 125f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left);

            CreateTextElement(
                parentElement, "tolerateWater", "False",
                x + 240f, y - 75f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );
            CreateTextElement(
                parentElement, "tolerateSmoke", "False",
                x + 240f, y - 100f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );
            CreateTextElement(
                parentElement, "tolerateTar", "False",
                x + 240f, y - 125f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );

            CreateTextElement(
                parentElement, "staggerWhenBlockedTag", "$almanac_stagger_when_blocked",
                x + 50f, y - 150f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );
            CreateTextElement(
                parentElement, "staggerDamageFactorTag", "$almanac_stagger_damage_factor",
                x + 50f, y - 175f,
                200f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
                );

            CreateTextElement(
                parentElement, "staggerWhenBlocked", "False",
                x + 240f, y - 150f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );
            CreateTextElement(
                parentElement, "staggerDamageFactor", "0",
                x + 240f, y - 175f,
                100f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Right
                );

            CreateTextElement(
                parentElement, "weakSpot", "$almanac_no_weak_spot",
                x + 50f, y - 200f,
                200f, 25f,
                Color.yellow, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left);

            CreateImageElement(
                parentElement, $"attackOverlay (99)",
                0f, y - 100f,
                400f, 400f,
                true,
                false,
                null,
                0f
                );

            CreateTextElement(
                parentElement, "consumeItemsTag", "$almanac_taming_food_items",
                0f, y - 275f,
                200f, 50f,
                Color.white, 20,
                overflowModes: TextOverflowModes.Overflow
            );
            CreateTextElement(
                parentElement, "consumeItems (no data)", "$almanac_creature_not_tamable",
                0f, y - 310f,
                200f, 50f,
                Color.yellow, 16,
                active: false,
                overflowModes: TextOverflowModes.Overflow
            );
            for (var index = 0; index < 7; index++)
            {
                GameObject dummyBackground = CreateImageElement(
                    parentElement, $"iconBg ({index})",
                    -155f + (index * 52), y - 315f,
                    50f, 50f,
                    sprite: iconBackground.sprite,
                    active: true, alpha: 1f
                    );
                CreateImageElement(
                    dummyBackground.transform, $"consumeItem ({index})",
                    0f, 0f,
                    45f, 45f,
                    addHoverableText: true,
                    shadow: true
                    );
            }
        }

        private static void CreateDefaultAttackStats(Transform parentElement, int index, float x, float y)
        {
            float position = index * 200f;
            float spacing = 20f;
            float distanceFromTitle = 20f;

            Dictionary<string, string> attackTags = new Dictionary<string, string>()
            {
                { $"attackNameTag ({index})", "$almanac_attack_name" },
                { $"attackBluntTag ({index})", "$almanac_blunt" },
                { $"attackSlashTag ({index})", "$almanac_slash" },
                { $"attackPierceTag ({index})", "$almanac_pierce" },
                { $"attackChopTag ({index})", "$almanac_chop" },
                { $"attackPickaxeTag ({index})", "$almanac_pickaxe" },
                { $"attackAttackForceTag ({index})", "$almanac_attack_force" },
                { $"attackDodgeableTag ({index})", "$almanac_dodgeable" },
                { $"attackStatusEffectTag ({index})", "$almanac_status_effect" },
                { $"attackFireTag ({index})", "$almanac_fire" },
                { $"attackFrostTag ({index})", "$almanac_frost" },
                { $"attackLightningTag ({index})", "$almanac_lightning" },
                { $"attackPoisonTag ({index})", "$almanac_poison" },
                { $"attackSpiritTag ({index})", "$almanac_spirit" },
                { $"attackBackStabBonusTag ({index})", "$almanac_back_stab_bonus" },
                { $"attackBlockableTag ({index})", "$almanac_blockable" }
            };
            List<KeyValuePair<string, string>> attackTagList = attackTags.ToList();
            Dictionary<string, string> attackValues = new Dictionary<string, string>()
            {
                { $"attackName ({index})", "Unknown" },
                { $"attackBlunt ({index})", "0" },
                { $"attackSlash ({index})", "0" },
                { $"attackPierce ({index})", "0" },
                { $"attackChop ({index})", "0" },
                { $"attackPickaxe ({index})", "0" },
                { $"attackAttackForce ({index})", "0" },
                { $"attackDodgeable ({index})", "0" },
                { $"attackStatusEffect ({index})", "None" },

                { $"attackFire ({index})", "0" },
                { $"attackFrost ({index})", "0" },
                { $"attackLightning ({index})", "0" },
                { $"attackPoison ({index})", "0" },
                { $"attackSpirit ({index})", "0" },
                { $"attackBackStabBonus ({index})", "0" },
                { $"attackBlockable ({index})", "0" },
            };
            List<KeyValuePair<string, string>> attackValueList = attackValues.ToList();
            for (var i = 0; i < 9; ++i)
            {
                float anchorY = (y - position - (spacing * i) - distanceFromTitle);
                float valueAnchorX = x + 120f;
                if (attackTagList[i].Key.StartsWith("attackName") ||
                    attackTagList[i].Key.StartsWith("attackStatusEffect"))
                {
                    CreateTextElement(
                        parentElement, attackTagList[i].Key, attackTagList[i].Value, 
                        x, anchorY,
                        100f, 25f, 
                        Color.white, 16,
                        horizontalAlignment: HorizontalAlignmentOptions.Left
                    );
                    CreateTextElement(
                        parentElement, attackValueList[i].Key, attackValueList[i].Value,
                        valueAnchorX + 50f, anchorY, 
                        200f, 25f, 
                        Color.yellow, 16,
                        horizontalAlignment: HorizontalAlignmentOptions.Left,
                        overflowModes: TextOverflowModes.Ellipsis
                        );
                }
                else
                {
                    CreateTextElement(
                        parentElement, attackTagList[i].Key, attackTagList[i].Value, x, anchorY,
                        100f, 25f, 
                        Color.white, 16,
                        horizontalAlignment: HorizontalAlignmentOptions.Left
                        );
                    CreateTextElement(
                        parentElement, attackValueList[i].Key, attackValueList[i].Value,
                        valueAnchorX, anchorY,
                        100f, 25f,
                        Color.white, 16,
                        horizontalAlignment: HorizontalAlignmentOptions.Left
                        );
                }
            }

            for (var i = 9; i < attackTagList.Count; ++i)
            {
                float anchorY = (y - position - (spacing * (i - 9)) - distanceFromTitle - 20f);
                float anchorX = x + 180f;
                float anchorValueX = anchorX + 130f;
                CreateTextElement(
                    parentElement, attackTagList[i].Key, attackTagList[i].Value,
                    anchorX, anchorY,
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Left
                    );
                CreateTextElement(
                    parentElement, attackValueList[i].Key, attackValueList[i].Value,
                    anchorValueX, anchorY,
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Left
                    );
            }

            CreateImageElement(
                parentElement, $"attackOverlay ({index})",
                0f, y - position - 250f,
                400f, 250f,
                true,
                false,
                null,
                0f
                );
        }

        private static GameObject CreateImageElement(Transform parentElement, string id, float anchoredX,
            float anchoredY, float sizeX, float sizeY, bool active = false, bool addHoverableText = false,
            Sprite? sprite = null, float alpha = 1f, bool shadow = false)
        {
            GameObject imageElement = new GameObject($"ImageElement ({id})");
            imageElement.SetActive(active);

            RectTransform imageRectTransform = imageElement.AddComponent<RectTransform>();
            imageRectTransform.SetParent(parentElement);
            imageRectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            imageRectTransform.sizeDelta = new Vector2(sizeX, sizeY);

            Image image = imageElement.AddComponent<Image>();
            image.sprite = sprite;
            image.color = new Color(1f, 1f, 1f, alpha);
            image.material = null;
            image.fillCenter = true;
            image.type = Image.Type.Filled;

            if (addHoverableText) AddHoverableText(imageElement, "Unknown");
            if (shadow)
            {
                var shadowElement = imageElement.AddComponent<Shadow>();
                shadowElement.effectColor = new Color(0f, 0f, 0f, 0.5f);
                shadowElement.effectDistance = new Vector2(4f, -3f);
                shadowElement.useGraphicAlpha = true;
            }

            return imageElement;
        }

        private static GameObject CreateCustomImageElement(Transform parentElement, string id, float anchoredX,
            float anchoredY, float sizeX, float sizeY, string imagePath, bool active = false,
            bool addHoverableText = false, float alpha = 1f)
        {
            GameObject imageElement = new GameObject($"ImageElement ({id})");
            imageElement.SetActive(active);

            RectTransform imageRectTransform = imageElement.AddComponent<RectTransform>();
            imageRectTransform.SetParent(parentElement);
            imageRectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            imageRectTransform.sizeDelta = new Vector2(sizeX, sizeY);

            Image image = imageElement.AddComponent<Image>();

            var createSprite = imageElement.AddComponent<CreateSprite>();
            createSprite.path = imagePath;

            image.sprite = null;
            image.material = null;
            image.fillCenter = true;
            image.type = Image.Type.Filled;
            image.color = new Color(1f, 1f, 1f, alpha);

            if (addHoverableText) AddHoverableText(imageElement, "Unknown");

            return imageElement;
        }

        private static void AddHoverableText(GameObject obj, string content, int fontSize = 12, float anchoredX = 0f, float anchoredY = -35f)
        {
            obj.AddComponent<ElementHover>();

            GameObject HoverTextElement = new GameObject("hoverTextElement");
            HoverTextElement.SetActive(false);

            RectTransform rectTransform = HoverTextElement.AddComponent<RectTransform>();
            rectTransform.SetParent(obj.transform);
            rectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            rectTransform.sizeDelta = new Vector2(80f, 30f);

            TextMeshProUGUI text = HoverTextElement.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.color = Color.white;
            text.autoSizeTextContainer = false;
            text.text = content;
            text.overflowMode = TextOverflowModes.Overflow;
            text.fontSize = fontSize;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static GameObject CreateTextElement(
            Transform parentElement, 
            string id, 
            string content,
            float anchoredX, 
            float anchoredY, 
            float sizeX, 
            float sizeY,
            Color color, 
            int fontSize = 10,
            bool active = true,
            bool shadow = false,
            TextOverflowModes overflowModes = TextOverflowModes.ScrollRect,
            HorizontalAlignmentOptions horizontalAlignment = HorizontalAlignmentOptions.Center,
            VerticalAlignmentOptions verticalAlignment = VerticalAlignmentOptions.Middle,
            TextWrappingModes wrapMode = TextWrappingModes.Normal
        )
        {
            GameObject textElement = new GameObject($"TextElement ({id})");
            textElement.SetActive(active);
            RectTransform textRectTransform = textElement.AddComponent<RectTransform>();
            textRectTransform.SetParent(parentElement);
            textRectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            textRectTransform.sizeDelta = new Vector2(sizeX, sizeY);

            TextMeshProUGUI text = textElement.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = Localization.instance.Localize(content);
            text.fontSize = fontSize;
            text.overflowMode = overflowModes;
            text.color = color;
            text.isOverlay = true;
            text.enableAutoSizing = true; // makes text invisible ?
            text.autoSizeTextContainer = false;
            text.horizontalAlignment = horizontalAlignment;
            text.verticalAlignment = verticalAlignment;
            text.wordWrappingRatios = 1f;
            text.textWrappingMode = wrapMode;
            text.richText = true;
            text.fontSizeMax = fontSize;
            text.fontSizeMin = 2f;
            text.lineSpacingAdjustment = 2f;
            text.lineSpacingAdjustment = 2f;

            return textElement;
        }

        private static void RepositionTrophyPanel(float x, float y)
        {
            var FrameRectTransform = TrophiesFrame.GetComponent<RectTransform>();
            FrameRectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}