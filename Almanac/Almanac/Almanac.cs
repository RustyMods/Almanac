using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Almanac.MonoBehaviors;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;
using Vector2 = UnityEngine.Vector2;

    namespace Almanac.Almanac;

public static class Almanac
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    public static class CreateAlmanac
    {
        private static GameObject root = null!;
        private static GameObject trophyElement = null!;
        private static Transform TrophiesFrame = null!;
        private static Transform TrophiesPanel = null!;
        private static Button closeButtonScript = null!;
        private static ButtonSfx closeButtonSfx = null!;
        private static TMP_FontAsset font = null!;
        private static Image closeButtonImage = null!;
        private static Image borderImage = null!;
        private static Image iconBg = null!;
        private static Image weightIcon = null!;
        private static Image armorIcon = null!;
        private static RectTransform creatureRectTransform = null!;
        private static RectTransform materialRectTransform = null!;
        private static RectTransform consumeRectTransform = null!;
        private static RectTransform equipmentRectTransform = null!;
        private static RectTransform weaponRectTransform = null!;
        private static RectTransform projectileRectTransform = null!;
        private static RectTransform fishRectTransform = null!;
        private static RectTransform piecesRectTransform = null!;
        private static RectTransform playerStatsRectTransform = null!;

        public static List<ItemDrop> materials = null!;
        public static List<ItemDrop> consumables = null!;
        public static List<ItemDrop> gear = null!;
        public static List<ItemDrop> weapons = null!;
        public static List<ItemDrop> fish = null!;
        public static List<ItemDrop> ammunition = null!;
        public static List<CreatureDataCollector.CreatureData> creatures = null!;
        public static List<GameObject> cookingStations = null!;
        public static List<GameObject> fermentingStations = null!;
        public static List<GameObject> smelterStations = null!;
        public static List<GameObject> miscPieces = null!;
        public static List<GameObject> craftingPieces = null!;
        public static List<GameObject> buildPieces = null!;
        public static List<GameObject> furniturePieces = null!;
        public static List<GameObject> plantPieces = null!;
        public static List<GameObject> defaultPieces = null!;
        public static List<GameObject> modPieces = null!;

        public static int fishPage;
        public static int projectilePage;
        public static int weaponsPage;
        public static int equipmentPage;
        public static int consumablesPage;
        public static int materialsPage;
        public static int creaturesPage;

        public static int miscPage;
        public static int craftPage;
        public static int buildPage;
        public static int furniturePage;
        public static int plantsPage;
        public static int otherPage;
        public static int modPage;
        
        public static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            if (AlmanacPlugin.WorkingAsType == AlmanacPlugin.WorkingAs.Server) return;
            SetInitialData(__instance);
            Achievements.RegisterAchievements();
            EditInventoryGUI();
            RepositionTrophyPanel(-220f, 0f);
            CreateAllPanels();
        }
        
        private static GameObject CreateAchievementsPanel(string id, List<Achievements.Achievement> list)
        {
            Transform trophies = TrophiesFrame.Find("Trophies");
            
            GameObject panel = new GameObject($"{id}Panel") { layer = 5 };

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            panel.SetActive(false);

            GameObject background = new GameObject($"{id}Background");
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
                Achievements.Achievement data = list[i];
                int wrappedIndex = i % 72;
                int rowIndex = wrappedIndex / 12;
                int colIndex = wrappedIndex % 12;

                float x = -577f + colIndex * 105f;
                float y = 275f - rowIndex * 110f;
                
                Vector2 pos = new Vector2(x, y);

                Sprite? iconSprite = data.sprite;
                if (!iconSprite) continue;
                string name = data.name;

                GameObject container = new GameObject($"{id}Container ({i})");
                RectTransform containerRect = container.AddComponent<RectTransform>();
                containerRect.SetParent(panel.transform);
                containerRect.anchoredPosition = pos;
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
                
                AddHoverableText(container, name, 16, anchoredY: -50f);
                
                if (i > 71) container.SetActive(false);
                
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
                    Transform AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                    Transform AlmanacList = AlmanacPanel.Find("AlmanacList");
                    Transform element = AlmanacList.Find($"{id}Element (0)");
                    SetActivePanelElement(id); 
                    Patches.OnOpenTrophiesPatch.SetAchievementsData(element, data);
                });
            }

            for (int i = 0; i < pages; ++i)
            {
                CreatePageButtons(panel.transform, i, 72, $"{id}Container", list);
            }

            return panel;
        }
        
        private static void SetInitialData(InventoryGui __instance)
        {
            root = __instance.gameObject.transform.Find("root").gameObject;
            trophyElement = __instance.m_trophieElementPrefab;
            TrophiesPanel = __instance.m_trophiesPanel.transform;
            TrophiesFrame = TrophiesPanel.Find("TrophiesFrame");

            Transform closeButton = TrophiesFrame.Find("Closebutton");
            Transform closeButtonText = closeButton.Find("Text");
            Transform iconBkg = trophyElement.transform.Find("icon_bkg");
            Transform border = TrophiesFrame.Find("border (1)");
            Transform weightIconElement = __instance.m_player.Find("Weight").Find("weight_icon");
            Transform armorIconElement = __instance.m_player.Find("Armor").Find("armor_icon");
            
            closeButtonText.TryGetComponent(out TextMeshProUGUI textMesh);
            closeButton.TryGetComponent(out Button button);
            closeButton.TryGetComponent(out ButtonSfx buttonSfx);
            closeButton.TryGetComponent(out Image buttonImage);
            iconBkg.TryGetComponent(out Image iconImage);
            border.TryGetComponent(out Image borderImg);
            weightIconElement.TryGetComponent(out Image weightImg);
            armorIconElement.TryGetComponent(out Image armorImg);

            if (!textMesh || !button || !buttonSfx || !buttonImage || !iconImage || !borderImg || !weightImg || !armorImg) return;
            
            font = textMesh.font;
            closeButtonScript = button;
            closeButtonSfx = buttonSfx;
            closeButtonImage = buttonImage;
            iconBg = iconImage;
            borderImage = borderImg;
            weightIcon = weightImg;
            armorIcon = armorImg;
            
            PieceDataCollector.GetBuildPieces();

            materials = ItemDataCollector.GetMaterials();
            consumables = ItemDataCollector.GetConsumables();
            gear = ItemDataCollector.GetEquipments();
            weapons = ItemDataCollector.GetWeapons();
            fish = ItemDataCollector.GetFishes();
            ammunition = ItemDataCollector.GetAmmunition();
            creatures = CreatureDataCollector.GetSortedCreatureData();
            cookingStations = PieceDataCollector.cookingStations;
            fermentingStations = PieceDataCollector.fermentingStations;
            smelterStations = PieceDataCollector.smelterStations;
            miscPieces = PieceDataCollector.miscPieces;
            craftingPieces = PieceDataCollector.craftingPieces;
            buildPieces = PieceDataCollector.buildPieces;
            furniturePieces = PieceDataCollector.furniturePieces;
            plantPieces = PieceDataCollector.plantPieces;
            defaultPieces = PieceDataCollector.defaultPieces;
            modPieces = PieceDataCollector.modPieces;
        }

        private static void CreateAllPanels()
        {
            CreateElementPanel();
            
            CreateCreaturesPanel();
            CreateMaterialPanel();
            CreatePanel("consummable", consumables);
            CreatePanel("gear", gear);
            CreatePanel("weapon", weapons);
            CreatePanel("ammo", ammunition);
            CreatePanel("fish", fish);

            CreatePiecesPanel("miscPieces", miscPieces);
            CreatePiecesPanel("craftingPieces", craftingPieces);
            CreatePiecesPanel("buildPieces", buildPieces);
            CreatePiecesPanel("furniturePieces", furniturePieces);
            CreatePiecesPanel("other", defaultPieces);
            CreatePiecesPanel("plantPieces", plantPieces);
            CreatePiecesPanel("modPieces", modPieces);
            
            CreateTabs("fishButton", "fish", -760f, 425f);
            CreateTabs("ammoButton", "ammo", -605f, 425f);
            CreateTabs("weaponButton", "weapon", -450f, 425f);
            CreateTabs("gearButton", "gear", -295f, 425f);
            CreateTabs("ConsummableButton", "consummable", -140f, 425f);
            CreateTabs("MaterialButton", "material", 15f, 425f);
            CreateTabs("TrophiesButton", "trophies", 170f, 425f);
            CreateTabs("CreatureButton", "creature", 325f, 425f);
            
            CreateTabs("miscPiecesButton", "miscPieces", -760f, -425f);
            CreateTabs("craftingPiecesButton", "craftingPieces", -605f, -425f);
            CreateTabs("buildPiecesButton", "buildPieces", -450f, -425f);
            CreateTabs("furniturePiecesButton", "furniturePieces", -295f, -425f);
            CreateTabs("plantPiecesButton", "plantPieces", -140f, -425f);
            
            CreateTabs("defaultPiecesButton", "other", 15f, -425f);
            
            if (modPieces.Count > 0) CreateTabs("modPiecesButton", "modPieces", 170f, -425f);
            
            CreateAchievementsPanel("achievements", Achievements.registeredAchievements);
            CreatePlayerStatsPanel("stats");
            
            CreateTabs("achievementsButton", "achievements", 585f, 425f);
            CreateTabs("playerStatsButton", "stats", 740f, 425f);

        }

        private static void CreatePlayerStatsPanel(string id)
        {
            Transform trophies = TrophiesFrame.Find("Trophies");
            
            GameObject panel = new GameObject($"{id}Panel") { layer = 5 };

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            panel.SetActive(false);

            GameObject background = new GameObject($"{id}Background");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(panel.transform);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(1260f, 650f);
            
            Image backgroundImage = background.AddComponent<Image>();
            
            Image trophiesImage = trophies.gameObject.GetComponent<Image>();
            backgroundImage.color = trophiesImage.color;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;

            CreateTextElement(panel.transform, "almanacPowers", "$almanac_no_data",
            -450f, 300f, 350f, 100f, Color.yellow, 30
            );

            for (int i = 0; i < 3; ++i)
            {
                GameObject effectBackground = CreateImageElement(panel.transform, $"activeEffects ({i})",
                    -550f + (i * 78f), 225f, 75f, 75f, false, false,
                    iconBg.sprite, 1f, true
                    );
                
                AddHoverableText(effectBackground, "$almanac_no_data", 12, 0f, -45f);
                GameObject icon = CreateImageElement(effectBackground.transform, "icon",
                    0f, 0f, 65f, 65f
                    );
                ButtonSfx sfx = icon.AddComponent<ButtonSfx>();
                sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
                
                Button button = icon.AddComponent<Button>();
                button.interactable = true;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = new ColorBlock()
                {
                    normalColor = new Color(1f, 1f, 1f, 1f),
                    highlightedColor = new Color(1f, 1f, 1f, 1f),
                    pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                    selectedColor = new Color(1f, 1f, 1f, 1f),
                    disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                    colorMultiplier = 1f,
                    fadeDuration = 0.1f
                };
                button.onClick = new Button.ButtonClickedEvent();

                CreateTextElement(
                    panel.transform,
                    $"activeDesc ({i})",
                    "$almanac_no_data",
                    -475f, 125f - (i * 75f),
                    250f, 50f,
                    Color.white, 18,
                    true, TextOverflowModes.Overflow,
                    HorizontalAlignmentOptions.Left,
                    wrapMode: TextWrappingModes.NoWrap);
            }
        }
        private static void EditInventoryGUI()
        {
            Transform info = root.transform.Find("Info");
            Transform trophiesOpenButton = info.Find("Trophies");

            UITooltip openTrophiesToolTip = trophiesOpenButton.GetComponent<UITooltip>();
            Image trophiesOpenImage = trophiesOpenButton.Find("Image").GetComponent<Image>();
            
            openTrophiesToolTip.m_text = "$almanac_name";
            trophiesOpenImage.sprite = AlmanacPlugin.AlmanacIconButton;
        }

        private static void CreateTabs(string id, string name, float anchorX, float anchorY)
        {
            GameObject tabButton = new GameObject(id);
            RectTransform buttonRectTransform = tabButton.AddComponent<RectTransform>();
            buttonRectTransform.SetParent(TrophiesPanel);
            buttonRectTransform.anchoredPosition = new Vector2(anchorX, anchorY);
            buttonRectTransform.sizeDelta = new Vector2(150f, 46f);

            Image backgroundImage = tabButton.AddComponent<Image>();
            backgroundImage.sprite = closeButtonImage.sprite;
            backgroundImage.color = closeButtonImage.color;
            backgroundImage.material = closeButtonImage.material;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;
            backgroundImage.type = closeButtonImage.type;
            backgroundImage.fillCenter = true;
            backgroundImage.pixelsPerUnitMultiplier = 1f;

            Button button = tabButton.AddComponent<Button>();
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
                SetUnknown(name);
            });

            GameObject text = new GameObject($"{name}ButtonText");
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.SetParent(tabButton.transform);
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

            ButtonSfx sfx = tabButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }

        private static void SetUnknown(string name)
        {
            switch (name)
            {
                case "fish":
                    Patches.OnOpenTrophiesPatch.SetUnknownItems(name, fish);
                    break;
                case "ammo":
                    Patches.OnOpenTrophiesPatch.SetUnknownItems(name, ammunition);
                    break;
                case "weapon":
                    Patches.OnOpenTrophiesPatch.SetUnknownItems(name, weapons);
                    break;
                case "gear":
                    Patches.OnOpenTrophiesPatch.SetUnknownItems(name, gear);
                    break;
                case "material":
                    Patches.OnOpenTrophiesPatch.SetUnknownItems(name, materials);
                    break;
                case "consummable":
                    Patches.OnOpenTrophiesPatch.SetUnknownItems(name, consumables);
                    break;
                case "miscPieces":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, miscPieces);
                    break;
                case "craftingPieces":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, craftingPieces);
                    break;
                case "buildPieces":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, buildPieces);
                    break;
                case "furniturePieces":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, furniturePieces);
                    break;
                case "other":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, defaultPieces);
                    break;
                case "plantPieces":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, plantPieces);
                    break;
                case "modPieces":
                    Patches.OnOpenTrophiesPatch.SetUnknownPieces(name, modPieces);
                    break;
                case "creature":
                    Patches.OnOpenTrophiesPatch.SetUnknownCreatures();
                    break;
                case "achievements":
                    break;
                case "stats":
                    break;
            }
        }
        private static void SetTopic(string name)
        {
            Transform topic = TrophiesFrame.Find("topic");
            Transform trophies = TrophiesFrame.Find("Trophies");

            topic.TryGetComponent(out TextMeshProUGUI textMesh);
            if (!textMesh) return;
            
            List<string> topicNames = new List<string>()
            {
                "material",
                "creature",
                "consummable",
                "gear",
                "weapon",
                "ammo",
                "fish",
                "miscPieces",
                "craftingPieces",
                "buildPieces",
                "furniturePieces",
                "other",
                "plantPieces",
                "modPieces",
                "achievements",
                "stats"
            };
            // Set panels active based on topic name
            foreach (string topicName in topicNames)
            {
                Transform panel = TrophiesFrame.Find($"{topicName}Panel");
                if (!panel) continue;
                panel.gameObject.SetActive(topicName == name);
            }
            // Set trophy panel
            trophies.gameObject.SetActive(name == "trophies");
            // Set topic title
            textMesh.text = Localization.instance.Localize($"$almanac_{name}_button");
            // Set player stats element
            if (name == "stats")
            {
                SetActivePanelElement(name);
                Patches.OnOpenTrophiesPatch.SetPlayerStats();
            }

        }
        private static void CreatePiecesPanel(string id, List<GameObject> list)
        {
            Transform trophies = TrophiesFrame.Find("Trophies");
            trophies.TryGetComponent(out Image trophiesImage);
            if (!trophiesImage) return;
            
            GameObject panel = new GameObject($"{id}Panel") { layer = 5 };

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            panel.SetActive(false);

            GameObject background = new GameObject($"{id}Background");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(panel.transform);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(1260f, 650f);
            
            Image backgroundImage = background.AddComponent<Image>();
            
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

                GameObject data = list[i];
                Vector2 pos = new Vector2(x, y);
                CreatePiecesContainer(panel.transform, data, i, pos, id);
            }

            for (int i = 0; i < pages; ++i)
            {
                CreatePageButtons(panel.transform, i, 72, $"{id}Container", list);
            }
        }
        private static GameObject CreatePanel(string id, List<ItemDrop> list)
        {
            Transform trophies = TrophiesFrame.Find("Trophies");
            
            GameObject panel = new GameObject($"{id}Panel") { layer = 5 };

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            panel.SetActive(false);

            GameObject background = new GameObject($"{id}Background");
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
                    CreateContainerWithoutSprite(panel.transform, data, i, pos, id);
                }
            }

            for (int i = 0; i < pages; ++i)
            {
                CreatePageButtons(panel.transform, i, 72, $"{id}Container", list);
            }

            return panel;
        }
        
        private static void CreatePageButtons(Transform parentElement, int index, int pageSize, string id, List<Achievements.Achievement> list)
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
                SetPageNumber(id, index);
                for (int i = 0; i < list.Count; ++i)
                {
                    try
                    {
                        Transform element = parentElement.Find($"{id} ({i})");
                        // string prefab = list[i].name;
                        float min = (index * pageSize);
                        float max = min + pageSize;
                        if (i >= min && i < max)
                        {
                            // SetBlackList(element, prefab, id);
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
        
        private static void CreatePageButtons(Transform parentElement, int index, int pageSize, string id, List<GameObject> list)
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
                SetPageNumber(id, index);
                for (int i = 0; i < list.Count; ++i)
                {
                    try
                    {
                        Transform element = parentElement.Find($"{id} ({i})");
                        string prefab = list[i].name;
                        float min = (index * pageSize);
                        float max = min + pageSize;
                        if (i >= min && i < max)
                        {
                            SetBlackList(element, prefab, id);
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

        private static void SetBlackList(Transform element, string prefab, string id)
        {
            switch (id)
            {
                case "CreatureContainer": 
                    element.gameObject.SetActive(!BlackList.CreatureBlackList.Value.Contains(prefab));
                    break;
                case "materialContainer": 
                    element.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefab));
                    break;
                case "consummableContainer": 
                    element.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefab));
                    break;
                case "weaponContainer": 
                    element.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefab));
                    break;
                case "gearContainer": 
                    element.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefab));
                    break;
                case "ammoContainer": 
                    element.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefab));
                    break;
                case "fishContainer": 
                    element.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefab));
                    break;
                case "miscPiecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "craftingPiecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "buildPiecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "furniturePiecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "piecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "plantPiecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "otherContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                case "modPiecesContainer":
                    element.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefab));
                    break;
                default:
                    element.gameObject.SetActive(true);
                    break;
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
                SetPageNumber(id, index);
                for (int i = 0; i < list.Count; ++i)
                {
                    try
                    {
                        Transform element = parentElement.Find($"{id} ({i})");
                        string prefab = list[i].name;
                        float min = (index * pageSize);
                        float max = min + pageSize;
                        if (i >= min && i < max)
                        {
                            SetBlackList(element, prefab, id);
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

        private static void SetPageNumber(string id, int index)
        {
            switch (id)
            {
                case "materialContainer":
                    materialsPage = index;
                    break;
                case "CreatureContainer":
                    creaturesPage = index;
                    break;
                case "consummableContainer":
                    consumablesPage = index;
                    break;
                case "gearContainer":
                    equipmentPage = index;
                    break;
                case "weaponContainer":
                    weaponsPage = index;
                    break;
                case "ammoContainer":
                    projectilePage = index;
                    break;
                case "fishContainer":
                    fishPage = index;
                    break;
                case "miscPiecesContainer":
                    miscPage = index;
                    break;
                case "craftingPiecesContainer":
                    craftPage = index;
                    break;
                case "buildPiecesContainer":
                    buildPage = index;
                    break;
                case "furniturePiecesContainer":
                    furniturePage = index;
                    break;
                case "otherContainer":
                    otherPage = index;
                    break;
                case "plantPiecesContainer":
                    plantsPage = index;
                    break;
                case "modPiecesContainer":
                    modPage = index;
                    break;
                default:
                    break;
            }
        }
        private static void CreatePiecesContainer(Transform parentElement, GameObject data, int index, Vector2 position, string id)
        {
            data.TryGetComponent(out Piece pieceScript);
            if (!pieceScript) return;
            
            Sprite iconSprite = pieceScript.m_icon;
            string name = pieceScript.m_name;

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
            
            AddHoverableText(container, name, 16, anchoredY: -50f);
            
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
                Transform AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                Transform AlmanacList = AlmanacPanel.Find("AlmanacList");
                Transform element = AlmanacList.Find("piecesElement (0)");
                SetActivePanelElement(id);
                Patches.OnOpenTrophiesPatch.SetPiecesData(element.gameObject, data);
            });
        }
         private static void CreateContainer(Transform parentElement, ItemDrop data, int index, Vector2 position, string id)
         {
            Sprite iconSprite = data.m_itemData.GetIcon();
            if (!iconSprite) return;
            string name = data.m_itemData.m_shared.m_name;

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
            
            AddHoverableText(container, name, 16, anchoredY: -50f);
            
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
                Transform AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                Transform AlmanacList = AlmanacPanel.Find("AlmanacList");
                Transform element = AlmanacList.Find($"{id}Element (0)");
                SetActivePanelElement(id);
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
                Transform AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                Transform AlmanacList = AlmanacPanel.Find("AlmanacList");
                Transform element = AlmanacList.Find($"{id}Element (0)");
                SetActivePanelElement(id);
                Patches.OnOpenTrophiesPatch.SetItemsData(element.gameObject, data);
            });
        }
        private static void CreateMaterialPanel()
        {
            Transform trophies = TrophiesFrame.Find("Trophies");

            GameObject newMaterialPanel = new GameObject("materialPanel") { layer = 5 };

            RectTransform panelRect = newMaterialPanel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            newMaterialPanel.SetActive(false);

            GameObject background = new GameObject("materialBackground");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(newMaterialPanel.transform);
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
                CreateMaterialContainer(newMaterialPanel.transform, data, i, pos);
            }

            for (int i = 0; i < pages; ++i)
            {
                CreateMaterialPageButtons(newMaterialPanel.transform, i, 72);
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
                SetPageNumber("materialContainer", index);
                for (int i = 0; i < materials.Count; ++i)
                {
                    Transform element = parentElement.Find($"materialContainer ({i})");
                    string prefab = materials[i].name;
                    float min = (index * pageSize);
                    float max = min + pageSize;
                    if (i >= min && i < max)
                    {
                        SetBlackList(element, prefab, "materialContainer");
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
            Sprite iconSprite = data.m_itemData.GetIcon();
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
            
            AddHoverableText(container, name, 18, anchoredY: -50f);
            
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
                Transform AlmanacPanel = TrophiesFrame.Find("ContentPanel");
                Transform AlmanacList = AlmanacPanel.Find("AlmanacList");
                Transform MaterialElement = AlmanacList.Find("materialElement (0)");
                SetActivePanelElement("material");
                Patches.OnOpenTrophiesPatch.SetItemsData(MaterialElement.gameObject, data);
            });
        }

        private static void SetActivePanelElement(string name)
        {
            Transform AlmanacPanel = TrophiesFrame.Find("ContentPanel");
            Transform AlmanacList = AlmanacPanel.Find("AlmanacList");
            Transform MaterialElement = AlmanacList.Find("materialElement (0)");
            Transform AlmanacElement = AlmanacList.Find("AlmanacElement (0)");
            Transform ConsumableElement = AlmanacList.Find("consummableElement (0)");
            Transform GearElement = AlmanacList.Find("gearElement (0)");
            Transform WelcomePanel = AlmanacPanel.Find("WelcomePanel (0)");
            Transform AmmoElement = AlmanacList.Find("ammoElement (0)");
            Transform weaponElement = AlmanacList.Find("weaponElement (0)");
            Transform fishElement = AlmanacList.Find("fishElement (0)");
            Transform piecesElement = AlmanacList.Find("piecesElement (0)");
            Transform achievementsElement = AlmanacList.Find("achievementsElement (0)");
            Transform playerStatsElement = AlmanacList.Find("statsElement (0)");

            WelcomePanel.gameObject.SetActive(name == "welcome");
            MaterialElement.gameObject.SetActive(name == "material");
            AlmanacElement.gameObject.SetActive(name == "creatures");
            ConsumableElement.gameObject.SetActive(name == "consummable");
            GearElement.gameObject.SetActive(name == "gear");
            AmmoElement.gameObject.SetActive(name == "ammo");
            weaponElement.gameObject.SetActive(name == "weapon");
            fishElement.gameObject.SetActive(name == "fish");
            achievementsElement.gameObject.SetActive(name == "achievements");
            playerStatsElement.gameObject.SetActive(name == "stats");

            List<string> piecesNames = new List<string>()
            {
                "miscPieces",
                "craftingPieces",
                "buildPieces",
                "furniturePieces",
                "other",
                "plantPieces",
                "modPieces"
            };
            piecesElement.gameObject.SetActive(piecesNames.Contains(name));
        }
        private static void CreateCreaturesPanel()
        {
            Transform trophies = TrophiesFrame.Find("Trophies");
            
            GameObject creaturePanel = new GameObject("creaturePanel") { layer = 5 };

            RectTransform panelRect = creaturePanel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            creaturePanel.SetActive(false);

            GameObject background = new GameObject("creaturesBackground");
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
                SetPageNumber("CreatureContainer", index);
                for (int i = 0; i < creatures.Count; ++i)
                {
                    Transform element = parentElement.Find($"CreatureContainer ({i})");
                    string prefab = creatures[i].name;
                    float min = (index * 100);
                    float max = min + 100;
                    if (i >= min && i < max)
                    {
                        SetBlackList(element, prefab, "CreatureContainer");
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
            Transform contentPanel = TrophiesFrame.Find("ContentPanel");
            Transform AlmanacList = contentPanel.Find("AlmanacList");
            Transform AlmanacElement = AlmanacList.Find("AlmanacElement (0)");
            
            float xSpacing = 250f;
            float ySpacing = 32f;
            
            int creaturesPerPage = 100;
            int pages = Mathf.CeilToInt((float)creatures.Count / creaturesPerPage);
            
            for (var i = 0; i < pages; ++i)
            {
                CreateCreaturePageButtons(parentElement, i);
            }

            HashSet<string> uniquePrefabs = new HashSet<string>();
            
            for (int page = 0; page < pages; page++)
            {
                for (int i = 0; i < creaturesPerPage; i++)
                {
                    int index = page * creaturesPerPage + i;
                    if (index >= creatures.Count)
                    {
                        break;
                    }

                    string name = creatures[index].display_name;
                    string prefab = creatures[index].name;
                    if (!uniquePrefabs.Contains(prefab))
                    {
                        uniquePrefabs.Add(prefab);
                        float x = -500f + (i / 20) * xSpacing;
                        float y = 305f - (i % 20) * ySpacing;

                        Vector2 position = new Vector2(x, y);
                        CreateCreatureButton(
                            AlmanacElement, 
                            parentElement, 
                            position, 
                            index, 
                            Localization.instance.Localize(name), 
                            page == 0,
                            prefab
                            );
                    }
                }
            }
        }

        private static void CreateCreatureButton(
            Transform AlmanacElement, Transform parentElement, Vector2 position, int i,
            string content, bool active, string prefabName)
        {
            float width = 250f;
            float height = 32f;
            
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
                SetActivePanelElement("creature");
                Patches.OnOpenTrophiesPatch.setAlmanacData(AlmanacElement.gameObject, content, prefabName: prefabName);
            });
        }

        private static void CreateElementPanel()
        {
            Vector2 position = new Vector2(880f, 0f);

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

            GameObject piecesElement = CreatePiecesElement(AlmanacList.transform, "pieces");

            GameObject achievementsElement = CreateAchievementsElement(AlmanacList.transform, "achievements");
            GameObject statsElement = CreateStatsElement(AlmanacList.transform, "stats");
            
            MaterialElement.SetActive(false);
            ConsummableElement.SetActive(false);
            GearElement.SetActive(false);
            WeaponElement.SetActive(false);
            AmmoElement.SetActive(false);
            FishElement.SetActive(false);
            piecesElement.SetActive(false);
            achievementsElement.SetActive(false);
            statsElement.SetActive(false);

            creatureRectTransform = AlmanacElement.GetComponent<RectTransform>();
            materialRectTransform = MaterialElement.GetComponent<RectTransform>();
            consumeRectTransform = ConsummableElement.GetComponent<RectTransform>();
            equipmentRectTransform = GearElement.GetComponent<RectTransform>();
            weaponRectTransform = WeaponElement.GetComponent<RectTransform>();
            projectileRectTransform = AmmoElement.GetComponent<RectTransform>();
            fishRectTransform = FishElement.GetComponent<RectTransform>();
            piecesRectTransform = piecesElement.GetComponent<RectTransform>();
            playerStatsRectTransform = statsElement.GetComponent<RectTransform>();

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
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2150f) : new Vector2(0f, (e - 0.5f) * 1f);
                // materialRectTransform.anchoredPosition = 
                //     e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                // consumeRectTransform.anchoredPosition =
                //     e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                equipmentRectTransform.anchoredPosition = 
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -500f) : new Vector2(0f, (e - 0.5f) * 1f);
                weaponRectTransform.anchoredPosition = 
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -1700f) : new Vector2(0f, (e - 0.5f) * 1f);
                projectileRectTransform.anchoredPosition =
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -1600f) : new Vector2(0f, (e - 0.5f) * 1f);
                // fishRectTransform.anchoredPosition = 
                //     e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2100f) : new Vector2(0f, (e - 0.5f) * 1f);
                piecesRectTransform.anchoredPosition =
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -1400f) : new Vector2(0f, (e - 0.5f) * 1f);
                playerStatsRectTransform.anchoredPosition =
                    e < 0.5f ? new Vector2(0f, (e - 0.5f) * -4550f) : new Vector2(0f, (e - 0.5f) * 1f);
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

        private static GameObject CreatePiecesElement(Transform parentElement, string id)
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
            
            Color orange = new Color(1f, 0.6f, 0f, 1f);
            
            Transform DummyElement = DummyPanel.transform;

            CreateImageElement(DummyElement, "pieces backdrop", 0f, 0f, 390f, 2200f, alpha: 0f, active: true);
            CreateTextElement(
                DummyElement, "Name", "$almanac_no_data", 
                45f, 360f,
                225f, 25f,
                Color.yellow, 25,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                wrapMode: TextWrappingModes.NoWrap
            );
            CreateTextElement(
                DummyElement, "Description", "$almanac_no_data",
                45f, 305f,
                225f, 75f,
                Color.white, 16,
                verticalAlignment: VerticalAlignmentOptions.Top,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                overflowModes: TextOverflowModes.Ellipsis
            );
            CreateImageElement(
                DummyElement, "icon", 
                -130f, 330f, 
                120f, 120f
            );
            
            CreateTextElement(
                DummyElement, "pieceScript", $"$almanac_general_title", 
                0f, 250f, 
                200f, 100f, 
                Color.white, 20
            );
            
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
            AddHoverableText(prefabImage, $"$almanac_prefab_name_label", anchoredY: 35f);
            
            CreateTextElement(
                DummyElement, "health", $"0", 
                120f, 197f, 
                100f, 100f, 
                orange, 25
            );
            CreateTextElement(
                DummyElement,
                "category", "$almanac_no_data",
                0f, 155f,
                250f, 25f,
                orange, 18,
                horizontalAlignment: HorizontalAlignmentOptions.Center
            );
            float leftAlignment = -80f;
            CreateItemDataSet(
                DummyElement,
                "comfortLabel", "$almanac_comfort_label",
                "comfort", "0",
                leftAlignment, 130f
            );
            CreateItemDataSet(
                DummyElement,
                "extensionLabel", "$almanac_extension_label",
                "extension", "$almanac_false",
                leftAlignment, 105f
            );
            CreateItemDataSet(
                DummyElement,
                "groundPieceLabel", "$almanac_ground_piece_label",
                "groundPiece", "$almanac_false",
                leftAlignment, 80f
            );
            CreateItemDataSet(
                DummyElement,
                "cultivatedGroundLabel", "$almanac_cultivated_ground_label",
                "cultivated", "$almanac_false",
                leftAlignment, 55f
            );
            CreateItemDataSet(
                DummyElement,
                "allowedDungeonLabel", "$almanac_allowed_dungeon_label",
                "dungeon", "$almanac_false",
                leftAlignment, 30f
            );
            CreateItemDataSet(
                DummyElement,
                "comfortGroupLabel", "$almanac_comfort_group_label",
                "comfortGroup", "$almanac_no_data",
                leftAlignment, 5f
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
            
            CreateItemDataSet(
                DummyElement,
                "materialLabel", "$almanac_material_label",
                "material", "$almanac_no_data",
                leftAlignment, -100f
            );
            // Create wear and tear modifiers table
            float anchorX = -100f;
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
                    iconXPos, -125f - (index * 25f),
                    25f, 25f,
                    iconList[index].Value,
                    active: true
                    );
            }
            
            var modifiersTags = new Dictionary<string, string>()
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
            List<KeyValuePair<string, string>> modifiersList = modifiersTags.ToList();
            for (var index = 0; index < modifiersList.Count; index++)
            {
                float modifierPosX = anchorX + 30f;
                var localizedValue = Localization.instance.Localize(modifiersList[index].Value);
                CreateTextElement(
                    DummyElement, modifiersList[index].Key, localizedValue,
                    modifierPosX, -125f - (index * 25f),
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
                    statListX, -125f - (index * 25f),
                    150f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Center
                    );
            }
            #region Create Plant Elements
            CreateItemDataSet(
                DummyElement,
                "growTimeLabel", "$almanac_grow_time_label",
                "growTime", "0",
                leftAlignment, -400f
                );
            CreateItemDataSet(
                DummyElement,
                "growTimeMaxLabel", "$almanac_grow_time_max_label",
                "growTimeMax", "0",
                leftAlignment, -425f
            );
            #endregion
            #region Create Container Elements
            CreateItemDataSet(
                DummyElement,
                "containerSizeLabel", "$almanac_container_size_label",
                "containerSize", "0/0",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "checkGuardLabel", "$almanac_check_guard_label",
                "checkGuard", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "autoDestroyLabel", "$almanac_auto_destroy_label",
                "autoDestroy", "$almanac_na",
                leftAlignment, -450f
            );
            for (int i = 0; i < 12; ++i)
            {
                float anchoredX = -128f + (i % 6) * 51f;
                float anchoredY = -500f - (i / 6) * 75f;

                GameObject ResourceBackground = CreateImageElement(
                    DummyElement, $"containerDrops ({i})",
                    -anchoredX, anchoredY,
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
            #endregion
            #region Create Crafting Station Elements
            CreateItemDataSet(
                DummyElement,
                "discoverRangeLabel", "$almanac_discover_range_label",
                "discoverRange", "$almanac_na",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "rangeBuildLabel", "$almanac_range_build_label",
                "rangeBuild", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "extraRangePerLevelLabel", "$almanac_extra_range_label",
                "extraRange", "$almanac_na",
                leftAlignment, -450f
            );
            CreateItemDataSet(
                DummyElement,
                "requireRoofLabel", "$almanac_require_roof_label",
                "requireRoof", "$almanac_na",
                leftAlignment, -475f
            );
            CreateItemDataSet(
                DummyElement,
                "requireFireLabel", "$almanac_require_fire_label",
                "requireFire", "$almanac_na",
                leftAlignment, -500f
            );
            CreateItemDataSet(
                DummyElement,
                "showBasicRecipesLabel", "$almanac_basic_recipes_label",
                "basicRecipes", "$almanac_na",
                leftAlignment, -525f
            );
            CreateItemDataSet(
                DummyElement,
                "animationIndexLabel", "$almanac_animation_index_label",
                "animationIndex", "$almanac_na",
                leftAlignment, -550f
            );
            #endregion
            #region Create Cooking Station Elements
            CreateTextElement(
                DummyElement, 
                "cookingTooltip", "$almanac_no_data",
                0f, -410f,
                150f, 25f,
                Color.white, 20,
                horizontalAlignment: HorizontalAlignmentOptions.Center
                );
            
            for (int i = 0; i < 12; ++i)
            {
                float anchoredX = -128f + (i % 6) * 51f;
                float anchoredY = -425f - (i / 6) * 75f;

                GameObject ResourceBackground = CreateImageElement(
                    DummyElement, $"cookingConversion ({i})",
                    -anchoredX, anchoredY,
                    50f, 50f,
                    addHoverableText: true,
                    sprite: iconBg.sprite,
                    active: false
                );

                CreateImageElement(ResourceBackground.transform, "item",
                    0f, 0f,
                    40f, 40f
                );

                CreateTextElement(ResourceBackground.transform, $"cookTime", "0",
                    0f, 0f,
                    50f, 50f,
                    orange, 14,
                    active: false,
                    verticalAlignment: VerticalAlignmentOptions.Bottom
                );
            }
            
            CreateItemDataSet(
                DummyElement,
                "overCookedItemLabel", "$almanac_over_cooked_label",
                "overCooked", "$almanac_na",
                leftAlignment, -650f
            );
            CreateItemDataSet(
                DummyElement,
                "availableSlotsLabel", "$almanac_available_slots_label",
                "availableSlots", "$almanac_na",
                leftAlignment, -675f
            );
            CreateItemDataSet(
                DummyElement,
                "cookingRequireFireLabel", "$almanac_require_fire_label",
                "cookingRequireFire", "$almanac_na",
                leftAlignment, -700f
            );
            CreateItemDataSet(
                DummyElement,
                "cookingUseFuelLabel", "$almanac_use_fuel_label",
                "cookingUseFuel", "$almanac_na",
                leftAlignment, -725f
            );
            CreateItemDataSet(
                DummyElement,
                "cookingFuelItemLabel", "$almanac_fuel_item_label",
                "cookingFuelItem", "$almanac_na",
                leftAlignment, -750f
            );
            CreateItemDataSet(
                DummyElement,
                "cookingMaxFuelLabel", "$almanac_max_fuel_label",
                "cookingMaxFuel", "$almanac_na",
                leftAlignment, -775f
            );
            CreateItemDataSet(
                DummyElement,
                "cookingSecPerFuelLabel", "$almanac_sec_per_fuel_label",
                "cookingSecPerFuel", "$almanac_na",
                leftAlignment, -800f
            );
            #endregion
            #region Create Extension Station Elements
            GameObject extensionCraftingStation = CreateImageElement(
                DummyElement, "extensionCraftingStation",
                0f, -400f,
                75f, 75f,
                addHoverableText: true,
                active: false
            );
            AddHoverableText(extensionCraftingStation, "$almanac_no_data", anchoredY: -45f);
            CreateItemDataSet(
                DummyElement,
                "maxStationDistanceLabel", "$almanac_station_distance_label",
                "stationDistance", "$almanac_na",
                leftAlignment, -475f
            );
            CreateItemDataSet(
                DummyElement,
                "extensionStationStackLabel", "$almanac_extension_stack_label",
                "extensionStack", "$almanac_na",
                leftAlignment, -500f
            );
            #endregion
            #region Create Door Elements
            CreateItemDataSet(
                DummyElement,
                "doorKeyItemLabel", "$almanac_door_key_label",
                "doorKey", "$almanac_na",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "doorCanBeClosedLabel", "$almanac_can_close_label",
                "doorCanClose", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "doorCheckGuardLabel", "$almanac_door_check_guard_label",
                "doorGuard", "$almanac_na",
                leftAlignment, -450f
            );
            #endregion
            #region Create Fireplace Elements
            CreateItemDataSet(
                DummyElement,
                "fireplaceMaxFuelLabel", "$almanac_max_fuel_label",
                "fireplaceMaxFuel", "$almanac_na",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "fireplaceSecPerFuelLabel", "$almanac_sec_per_fuel_label",
                "fireplaceSecPerFuel", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "infiniteFuelLabel", "$almanac_infinite_fuel_label",
                "fireplaceInfiniteFuel", "$almanac_na",
                leftAlignment, -450f
            );
            CreateItemDataSet(
                DummyElement,
                "fireplaceFuelItemLabel", "$almanac_fuel_item_label",
                "fireplaceFuelItem", "$almanac_na",
                leftAlignment, -475f
            );
            
            for (int i = 0; i < 11; ++i)
            {
                float anchoredX = -128f + (i % 6) * 51f;
                float anchoredY = -550f - (i / 6) * 75f;

                GameObject ResourceBackground = CreateImageElement(
                    DummyElement, $"fireplaceFireworks ({i})",
                    -anchoredX, anchoredY,
                    50f, 50f,
                    addHoverableText: true,
                    sprite: iconBg.sprite,
                    active: false
                );

                CreateImageElement(ResourceBackground.transform, "item",
                    0f, 0f,
                    40f, 40f
                );

                CreateTextElement(ResourceBackground.transform, $"itemCount", "0",
                    0f, 0f,
                    50f, 50f,
                    orange, 14,
                    active: false,
                    verticalAlignment: VerticalAlignmentOptions.Bottom
                );
            }
            #endregion
            #region Create Smelter Elements
            CreateItemDataSet(
                DummyElement,
                "smelterFuelItemLabel", "$almanac_fuel_item_label",
                "smelterFuelItem", "$almanac_na",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "smelterMaxOreLabel", "$almanac_max_ore_label",
                "smelterMaxOre", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "smelterMaxFuelLabel", "$almanac_max_fuel_label",
                "smelterMaxFuel", "$almanac_na",
                leftAlignment, -450f
            );
            CreateItemDataSet(
                DummyElement,
                "smelterFuelPerProductLabel", "$almanac_fuel_per_product_label",
                "smelterFuelPerProduct", "$almanac_na",
                leftAlignment, -475f
            );
            CreateItemDataSet(
                DummyElement,
                "smelterSecPerProductLabel", "$almanac_sec_per_product_label",
                "smelterSecPerProduct", "$almanac_na",
                leftAlignment, -500f
            );
            CreateItemDataSet(
                DummyElement,
                "smelterRequiresRoofLabel", "$almanac_require_roof_label",
                "smelterRequireRoof", "$almanac_na",
                leftAlignment, -525f
            );
            
            
            for (int i = 0; i < 8; ++i)
            {
                float anchoredY = -575f - (i * 75f);

                GameObject From = CreateImageElement(
                    DummyElement, $"smelterConversion from ({i})",
                    -120f, anchoredY,
                    50f, 50f,
                    addHoverableText: true,
                    sprite: iconBg.sprite,
                    active: false
                );

                CreateImageElement(From.transform, "item",
                    0f, 0f,
                    40f, 40f
                );
                
                GameObject To = CreateImageElement(
                    DummyElement, $"smelterConversion to ({i})",
                    120f, anchoredY,
                    50f, 50f,
                    addHoverableText: true,
                    sprite: iconBg.sprite,
                    active: false
                );

                CreateImageElement(To.transform, "item",
                    0f, 0f,
                    40f, 40f
                );

                CreateTextElement(
                    DummyElement,
                    $"smelterConversionSymbol ({i})", "$almanac_conversion_symbol",
                    0f, anchoredY,
                    50f, 50f,
                    Color.white, 20
                    );
            }
            #endregion
            #region Create Wisp Elements
            CreateItemDataSet(
                DummyElement,
                "wispSpawnIntervalLabel", "$almanac_spawn_interval_label",
                "wispInterval", "$almanac_na",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "wispSpawnChanceLabel", "$almanac_spawn_chance_label",
                "wispSpawnChance", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "wispMaxSpawnedLabel", "$almanac_max_spawn_label",
                "wispMaxSpawn", "$almanac_na",
                leftAlignment, -450f
            );
            CreateItemDataSet(
                DummyElement,
                "wispSpawnAtNightLabel", "$almanac_spawn_night_label",
                "wispSpawnAtNight", "$almanac_na",
                leftAlignment, -475f
            );
            CreateItemDataSet(
                DummyElement,
                "wispSpawnInCoverLabel", "$almanac_spawn_cover_label",
                "wispSpawnCover", "$almanac_na",
                leftAlignment, -500f
            );
            CreateItemDataSet(
                DummyElement,
                "wispSpawnDistanceLabel", "$almanac_spawn_distance_label",
                "wispSpawnDistance", "$almanac_na",
                leftAlignment, -525f
            );
            CreateItemDataSet(
                DummyElement,
                "wispMaxSpawnAreaLabel", "$almanac_spawn_area_label",
                "wispSpawnArea", "$almanac_na",
                leftAlignment, -550f
            );
            CreateImageElement(
                DummyElement, "wispPrefab",
                0f, -600f,
                75f, 75f,
                addHoverableText: true,
                shadow: true
                );
            #endregion
            #region Create Turret Elements
            CreateItemDataSet(
                DummyElement,
                "turretTurnRateLabel", "$almanac_turn_rate_label",
                "turnRate", "$almanac_na",
                leftAlignment, -400f
            );
            CreateItemDataSet(
                DummyElement,
                "turretHorizontalAngleLabel", "$almanac_horizontal_angle_label",
                "horizontalAngle", "$almanac_na",
                leftAlignment, -425f
            );
            CreateItemDataSet(
                DummyElement,
                "turretVerticalAngleLabel", "$almanac_vertical_angle_label",
                "verticalAngle", "$almanac_na",
                leftAlignment, -450f
            );
            CreateItemDataSet(
                DummyElement,
                "turretViewDistanceLabel", "$almanac_view_distance_label",
                "viewDistance", "$almanac_na",
                leftAlignment, -475f
            );
            CreateItemDataSet(
                DummyElement,
                "turretNoTargetRateLabel", "$almanac_target_rate_label",
                "noTargetRate", "$almanac_na",
                leftAlignment, -500f
            );
            CreateItemDataSet(
                DummyElement,
                "turretLookAccelerationLabel", "$almanac_look_acceleration_label",
                "lookAcceleration", "$almanac_na",
                leftAlignment, -525f
            );
            CreateItemDataSet(
                DummyElement,
                "turretLookDecelerationLabel", "$almanac_look_deceleration_label",
                "lookDeceleration", "$almanac_na",
                leftAlignment, -550f
            );
            CreateItemDataSet(
                DummyElement,
                "turretLookMinDegreesLabel", "$almanac_look_degrees_label",
                "lookDegrees", "$almanac_na",
                leftAlignment, -575f
            );
            CreateItemDataSet(
                DummyElement,
                "turretMaxAmmoLabel", "$almanac_max_ammo_label",
                "turretMaxAmmo", "$almanac_na",
                leftAlignment, -600f
            );
            for (int i = 0; i < 11; ++i)
            {
                float anchoredX = -128f + (i % 6) * 51f;
                float anchoredY = -675f - (i / 6) * 75f;

                GameObject ResourceBackground = CreateImageElement(
                    DummyElement, $"turretAmmo ({i})",
                    -anchoredX, anchoredY,
                    50f, 50f,
                    addHoverableText: true,
                    sprite: iconBg.sprite,
                    active: false
                );

                CreateImageElement(ResourceBackground.transform, "item",
                    0f, 0f,
                    40f, 40f
                );
            }
            #endregion
            #region Create Fermenter Elements
            CreateItemDataSet(
                DummyElement,
                "fermentDurationLabel", "$almanac_ferment_duration_label",
                "fermentDuration", "$almanac_na",
                leftAlignment, -400f
            );
            for (int i = 0; i < 12; ++i)
            {
                float anchoredY = -450f - (i * 51f);

                GameObject From = CreateImageElement(
                    DummyElement, $"ferment from ({i})",
                    -120f, anchoredY,
                    50f, 50f,
                    addHoverableText: false,
                    sprite: iconBg.sprite,
                    active: false
                );
                AddHoverableText(From, "", 12, 50f, 20f);

                CreateImageElement(From.transform, "item",
                    0f, 0f,
                    40f, 40f
                );
                
                GameObject To = CreateImageElement(
                    DummyElement, $"ferment to ({i})",
                    120f, anchoredY,
                    50f, 50f,
                    addHoverableText: false,
                    sprite: iconBg.sprite,
                    active: false
                );
                AddHoverableText(To, "", 12, -50f, 20f);

                CreateImageElement(To.transform, "item",
                    0f, 0f,
                    40f, 40f
                );
                CreateTextElement(
                    To.transform, 
                    "produceAmount", "0",
                    0f, 0f,
                    50f, 50f,
                    orange, 16,
                    verticalAlignment: VerticalAlignmentOptions.Bottom
                    );

                CreateTextElement(
                    DummyElement,
                    $"fermentConversionSymbol ({i})", "$almanac_conversion_symbol",
                    0f, anchoredY,
                    50f, 50f,
                    Color.white, 20
                );
            }
            #endregion
            #region Create Ship Elements
            // CreateItemDataSet(
            //     DummyElement,
            //     "waterLevelOffsetLabel", "$almanac_water_offset_label",
            //     "waterOffset", "$almanac_na",
            //     leftAlignment, -400f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "forceDistanceLabel", "$almanac_force_distance_label",
            //     "forceDistance", "$almanac_na",
            //     leftAlignment, -425f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "dampingLabel", "$almanac_damping_label",
            //     "damping", "$almanac_na",
            //     leftAlignment, -450f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "dampingSidewayLabel", "$almanac_damping_sideway_label",
            //     "dampingSideway", "$almanac_na",
            //     leftAlignment, -475f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "dampingForwardLabel", "$almanac_damping_forward_label",
            //     "dampingForward", "$almanac_na",
            //     leftAlignment, -500f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "angularDampingLabel", "$almanac_damping_angular_label",
            //     "dampingAngular", "$almanac_na",
            //     leftAlignment, -525f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "disableLevelLabel", "$almanac_disable_level_label",
            //     "disableLevel", "$almanac_na",
            //     leftAlignment, -550f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "sailForceOffsetLabel", "$almanac_sail_force_offset_label",
            //     "sailForceOffset", "$almanac_na",
            //     leftAlignment, -575f
            // );
            // CreateItemDataSet(
            //     DummyElement,
            //     "waterLevelOffsetLabel", "$almanac_water_offset_label",
            //     "waterOffset", "$almanac_na",
            //     leftAlignment, -400f
            // );
            #endregion
            
            
            return DummyPanel;
        }

        private static GameObject CreateStatsElement(Transform parentElement, string id)
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

            CreateTextElement(DummyElement, "title",
                "$almanac_stats_title",
                0f, 300f,
                100f, 100f,
                Color.white, 20);

            List<string> statsList = new()
            {
                "general_title",
                "totalKills",
                "totalDeaths",
                "craftOrUpgrades",
                "builds",
                "jumps",
                "cheats",
                
                "enemy_title",
                "enemyHits",
                "enemyKills",
                "enemyKillsLastHit",
                
                "player_title",
                "playerHits",
                "playerKills",
                "hitsTakenEnemies",
                "itemPickedUp",
                "crafts",
                "upgrades",
                "portalsUsed",
                "distanceTraveled",
                "distanceWalk",
                "distanceRun",
                "distanceSail",
                "distanceAir",
                "timeInBase",
                "timeOutOfBase",
                "sleep",
                "itemStandUses",
                "armorStandUses",
                
                "misc_title",
                "worldLoads",
                "treeChops",
                "tree",
                "treeTier0",
                "treeTier1",
                "treeTier2",
                "treeTier3",
                "treeTier4",
                "treeTier5",
                "logChops",
                "logs",
                "mineHits",
                "mines",
                "mineTier0",
                "mineTier1",
                "mineTier2",
                "mineTier3",
                "mineTier4",
                "mineTier5",
                "ravenHits",
                "ravenTalk",
                "ravenAppear",
                
                "info_title",
                "creatureTamed",
                "foodEaten",
                "skeletonSummons",
                "arrowsShot",
                
                "death_title",
                "tombstonesOpenedOwn",
                "tombstonesOpenOther",
                "tombstonesFit",
                "deathByUndefined",
                "deathByEnemyHit",
                "deathByPlayerHit",
                "deathByFall",
                "deathByDrowning",
                "deathByBurning",
                "deathByFreezing",
                "deathByPoisoned",
                "deathBySmoke",
                "deathByWater",
                "deathByEdgeOfWorld",
                "deathByImpact",
                "deathByCart",
                "deathByTree",
                "deathBySelf",
                "deathByStructural",
                "deathByTurret",
                "deathByBoat",
                "deathByStalagtite",
                
                "other_title",
                "doorsOpened",
                "doorsClosed",
                "beesHarvested",
                "sapHarvested",
                "turretAmmoAdded",
                "turretTrophySet",
                "trapArmed",
                "trapTriggered",
                "placeStacks",
                "portalDungeonIn",
                "portalDungeonOut",
                "totalBossKills",
                "bossLastHits",
                
                "guardian_title",
                "setGuardianPower",
                "setPowerEikthyr",
                "setPowerElder",
                "setPowerBonemass",
                "setPowerModer",
                "setPowerYagluth",
                "setPowerQueen",
                "setPowerAshlands",
                "setPowerDeepNorth",
                "useGuardianPower",
                "usePowerEikthyr",
                "usePowerElder",
                "usePowerBonemass",
                "usePowerModer",
                "usePowerYagluth",
                "usePowerQueen",
                "usePowerAshlands",
                "usePowerDeepNorth",
                
                "count_title",
                "count"
            };

            for (int i = 0; i < statsList.Count; ++i)
            {
                CreateTextElement(
                    DummyElement,
                    statsList[i],
                    "$almanac_no_data",
                    0f,
                    225f - (i * 25f),
                    250f, 50f,
                    Color.white ,
                    20,
                    horizontalAlignment: statsList[i].Contains("title") ? HorizontalAlignmentOptions.Center : HorizontalAlignmentOptions.Left,
                    overflowModes: TextOverflowModes.Overflow,
                    wrapMode: TextWrappingModes.NoWrap
                );
            }

            CreateImageElement(DummyElement, "overlay", 0f, 0f, 390f, 2500f, true, alpha: 0f);
            
            return DummyPanel;
        }

        private static GameObject CreateAchievementsElement(Transform parentElement, string id)
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
            
            CreateTextElement(
                DummyElement,
                "achievementTitle", 
                "$almanac_no_data",
                0f, 325f,
                250f, 75f,
                Color.yellow, 30
            );

            CreateTextElement(
                DummyElement,
                "achievementDescription",
                "$almanac_no_data",
                0f, 250f,
                250f, 100f,
                Color.white, 20
                );

            GameObject achievementBg = CreateImageElement(
                DummyElement, "achievementIcon",
                0f, 125f,
                125f, 125f,
                true,
                false,
                iconBg.sprite,
                shadow: true
            );

            GameObject icon = CreateImageElement(
                achievementBg.transform, "icon",
                0f, 0f,
                100f, 100f,
                addHoverableText: false
            );

            icon.TryGetComponent(out Image iconImage);

            ButtonSfx buttonSfx = icon.AddComponent<ButtonSfx>();
            buttonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;

            Button button = icon.AddComponent<Button>();
            button.interactable = true;
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = iconImage;
            button.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            button.onClick = new Button.ButtonClickedEvent();

            CreateTextElement(
                DummyElement,
                "achievementProgress", "$almanac_no_data",
                0f, 25f,
                275f, 50f,
                Color.white, 20
                );

            CreateTextElement(
                DummyElement,
                "achievementTooltip", "$almanac_no_data",
                0f, -25f,
                275f, 50f, 
                Color.white, 18);

            CreateTextElement(
                DummyElement,
                "achievementLore", "$almanac_no_data",
                0f, -200f,
                275f, 250f,
                Color.white, 18,
                overflowModes: TextOverflowModes.Ellipsis
            );
                
            return DummyPanel;
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
                45f, 360f,
                225f, 25f,
                Color.yellow, 25,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                wrapMode: TextWrappingModes.NoWrap
            );
            CreateTextElement(
                DummyElement, "Description", "$almanac_no_data",
                45f, 305f,
                225f, 75f,
                Color.white, 16,
                verticalAlignment: VerticalAlignmentOptions.Top,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                overflowModes: TextOverflowModes.Ellipsis
            );
            CreateImageElement(
                DummyElement, "icon", 
                -130f, 340f, 
                120f, 120f
            );
            
            CreateTextElement(
                DummyElement, "stats", $"$almanac_general_title", 
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
            AddHoverableText(WeightText, "$almanac_weight", anchoredY: 35f);
            
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
            AddHoverableText(prefabImage, $"$almanac_prefab_name_label", anchoredY: 35f);

            float leftAlignment = -70f;

            CreateTextElement(
                DummyElement,
                "floating", "$almanac_item_can_float",
                0f, 155f,
                250f, 25f,
                orange, 18,
                horizontalAlignment: HorizontalAlignmentOptions.Center
                );
            
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
            
            GameObject durabilityContent = CreateItemDataSet(
                DummyElement,
                "durabilityLabel", "$almanac_durability_label",
                "durability", "0",
                leftAlignment, 55f
                );
            RectTransform durabilityRect = durabilityContent.GetComponent<RectTransform>();
            durabilityRect.sizeDelta = new Vector2(120f, 40f);
            
            CreateTextElement(
                DummyElement, "teleportable", "$almanac_no_data",
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
                active: false,
                wrapMode: TextWrappingModes.NoWrap,
                overflowModes: TextOverflowModes.Overflow
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
                0f, -110f,
                150f, 25f,
                Color.white, 20
            );

            CreateItemDataSet(
                DummyElement,
                "movementLabel", "$almanac_movement_modifier_label",
                "movement", "0",
                leftAlignment, -140f
                );
            CreateItemDataSet(
                DummyElement,
                "eitrRegenLabel", "$almanac_eitr_regen_label",
                "eitrRegen", "0",
                leftAlignment, -165f
                );
            CreateItemDataSet(
                DummyElement, 
                "stamModLabel", "$almanac_base_items_stamina_modifier_label",
                "stamMod", "0",
                leftAlignment, -190f
                );
            CreateTextElement(
                DummyElement,
                "damageMod", "$almanac_equipment_normal_damage_modifier",
                0f, -215f,
                200f, 25f,
                Color.white, 18
            );
            
            CreateTextElement(DummyElement, "setName", "$almanac_no_data",
                0f, -250f,
                350f, 50f,
                Color.white, 16
                );
            CreateTextElement(
                DummyElement, "setDescription", "$almanac_no_data",
                0f, -300f,
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
                    -110f + (index * 25f), -350f,
                    25f, 25f,
                    setIconList[index].Value,
                    true,
                    true
                );
            }

            CreateTextElement(
                DummyElement,
                "modifySkill", "$almanac_no_data",
                0f, -400f,
                350f, 25f,
                Color.white, 16
                );

            GameObject ArmorIconBG = CreateImageElement(
                DummyElement, "armorBg",
                -125f, -450f,
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
                0f, -450f,
                150f, 25f,
                Color.white, 20
                );
            CreateItemDataSet(
                DummyElement, 
                "armorPerLevelLabel", "$almanac_armor_per_level_label",
                "armorPerLevel", "0",
                leftAlignment, -500f
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
                    leftAlignment - 85f, -525f - (index * 25f),
                    25f, 25f,
                    weaponTypeList[index].Value,
                    true,
                    false
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
                    leftAlignment - 15f, -525f - (index * 25f),
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Left
                );
                
                CreateTextElement(
                    DummyElement, $"{weaponTagList[index].Key}WeaponValue", "0",
                    leftAlignment + 90f, -525f - (index * 25f),
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Right
                );
                
                CreateTextElement(
                    DummyElement, $"{weaponTagList[index].Key}WeaponPerLevel", "+0/level",
                    leftAlignment + 155f, -525f - (index * 25f),
                    100f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Right
                );
            }

            CreateItemDataSet(
                DummyElement,
                "attackStaminaLabel", "$almanac_attack_stamina_label",
                "attackStamina", "0",
                leftAlignment, -800f
                );
            CreateItemDataSet(
                DummyElement,
                "attackEitrLabel", "$almanac_attack_eitr_label",
                "attackEitr", "0",
                leftAlignment, -825f
                );
            CreateItemDataSet(
                DummyElement,
                "attackHealthLabel", "$almanac_attack_health_label",
                "attackHealth", "0",
                leftAlignment, -850f
                );
            CreateItemDataSet(
                DummyElement,
                "attackHealthPercentageLabel", "$almanac_attack_health_percentage_label",
                "attackHealthPercentage", "0",
                leftAlignment, -875f
                );
            CreateItemDataSet(
                DummyElement,
                "speedFactorLabel", "$almanac_attack_speed_factor_label",
                "speedFactor", "0",
                leftAlignment, -900f
                );
            CreateItemDataSet(
                DummyElement,
                "speedFactorRotationLabel", "$almanac_speed_factor_rotation_label",
                "speedFactorRotation", "0",
                leftAlignment, -925f
                );
            CreateItemDataSet(
                DummyElement,
                "attackStartNoiseLabel", "$almanac_attack_start_noise_label",
                "attackStartNoise", "0",
                leftAlignment, -950f
                );
            CreateItemDataSet(
                DummyElement,
                "attackHitNoiseLabel", "$almanac_attack_hit_noise_label",
                "attackHitNoise", "0",
                leftAlignment, -975f
                );
            CreateItemDataSet(
                DummyElement,
                "damageMultiplierLabel", "$almanac_damage_multiplier_label",
                "damageMultiplier", "0",
                leftAlignment, -1000f
                );
            CreateItemDataSet(
                DummyElement,
                "forceMultiplierLabel", "$almanac_force_multiplier_label",
                "forceMultiplier", "0",
                leftAlignment, -1025f
                );
            CreateItemDataSet(
                DummyElement,
                "staggerMultiplierLabel", "$almanac_stagger_multiplier_label",
                "staggerMultiplier", "0",
                leftAlignment, -1050f
                );
            CreateItemDataSet(
                DummyElement,
                "recoilPushbackLabel", "$almanac_recoil_pushback_label",
                "recoilPushback", "0",
                leftAlignment, -1075f
                );
            CreateItemDataSet(
                DummyElement, 
                "selfDamageLabel", "$almanac_self_damage_label",
                "selfDamage", "0",
                leftAlignment, -1100f
                );
            // Bows
            CreateItemDataSet(
                DummyElement,
                "drawDurationMinLabel", "$almanac_draw_duration_min_label",
                "drawDuration", "0",
                leftAlignment, -1125f
                );
            CreateItemDataSet(
                DummyElement,
                "drawStaminaDrainLabel", "$almanac_draw_stamina_drain_label",
                "drawStamina", "0",
                leftAlignment, -1150f
                );
            CreateItemDataSet(
                DummyElement,
                "reloadTimeLabel", "$almanac_reload_time_label",
                "reloadTime", "0",
                leftAlignment, -1175f
                );
            CreateItemDataSet(
                DummyElement,
                "reloadStaminaDrainLabel", "$almanac_reload_stamina_drain_label",
                "reloadStaminaDrain", "0",
                leftAlignment, -1200f
                );
            // Fish
            CreateTextElement(
                DummyElement,
                "FishBaitLabel", "$almanac_fish_bait_label",
                leftAlignment - 52f, -110f,
                180f, 25f,
                Color.white, 18
            );
            CreateImageElement(
                DummyElement,
                "baitIcon",
                120f, -115f,
                40f, 40f,
                true,
                true,
                shadow: true
            );
            CreateTextElement(
                DummyElement,
                "FishDropsTitle", "$almanac_fish_drops_title",
                0f, -150f,
                250f, 25f,
                Color.white, 20
                );
            for (int i = 0; i < 3; ++i)
            {
                GameObject fishDropBg = CreateImageElement(
                    DummyElement,
                    $"fishDrops ({i})",
                    -120f + (i * 52f), -200f,
                    50f, 50f,
                    false,
                    true,
                    iconBg.sprite
                );
                CreateImageElement(
                    fishDropBg.transform,
                    "fishDropItemIcon",
                    0f, 0f,
                    40f, 40f,
                    true
                );
            }
            
            return DummyPanel;
        }

        private static GameObject CreateItemDataSet(Transform parentElement, string labelId, string labelContent,
            string valueId, string valueContent, float anchoredX, float anchoredY)
        {
            CreateTextElement(
                parentElement, labelId, labelContent,
                anchoredX, anchoredY,
                180f, 25f,
                Color.white, 16,
                horizontalAlignment: HorizontalAlignmentOptions.Left
            );
            GameObject content = CreateTextElement(
                parentElement, valueId, valueContent,
                anchoredX + 200f, anchoredY,
                40f, 40f,
                new Color(1f, 0.5f, 0f, 1f), 16
            );
            return content;
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
                overflowModes: TextOverflowModes.Overflow,
                horizontalAlignment: HorizontalAlignmentOptions.Left,
                wrapMode: TextWrappingModes.NoWrap
            );

            GameObject clipBoard = CreateImageElement(
                DummyElement, "Clipboard",
                170f, 370f,
                25f, 25f,
                true,
                sprite: closeButtonImage.sprite
            );

            CreateImageElement(clipBoard.transform, "ClipBoard Icon",
                0f, 0f,
                20f, 20f,
                true,
                sprite: AlmanacPlugin.AlmanacIconButton,
                alpha: 0.7f
                );
            
            clipBoard.TryGetComponent(out Image clipBoardImage);
            
            AddHoverableText(clipBoard, "", anchoredY: 20f);
            Transform? clipBoardHover = clipBoard.transform.Find("hoverTextElement");
            clipBoardHover.TryGetComponent(out TextMeshProUGUI clipBoardTextMesh);
            
            ButtonSfx clipBoardButtonSfx = clipBoard.AddComponent<ButtonSfx>();
            clipBoardButtonSfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
            
            Button clipBoardButton = clipBoard.AddComponent<Button>();
            clipBoardButton.interactable = true;
            clipBoardButton.transition = Selectable.Transition.SpriteSwap;
            clipBoardButton.spriteState = new SpriteState()
            {
                highlightedSprite = closeButtonScript.spriteState.highlightedSprite,
                pressedSprite = closeButtonScript.spriteState.pressedSprite,
                selectedSprite = closeButtonScript.spriteState.selectedSprite,
                disabledSprite = closeButtonScript.spriteState.disabledSprite
            };
            clipBoardButton.targetGraphic = clipBoardImage;
            clipBoardButton.onClick = new Button.ButtonClickedEvent();
            clipBoardButton.onClick.AddListener(() =>
            {
                TextEditor textEditor = new TextEditor
                {
                    text = clipBoardTextMesh.text
                };
                textEditor.SelectAll();
                textEditor.Copy();
                
                MessageHud.instance.ShowMessage(
                    MessageHud.MessageType.Center, 
                    Localization.instance.Localize("$almanac_copy_to_clipboard"));
            });

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
                "Almanac.icons.AlmanacUnknownIcon.png",
                alpha: 0.5f);

            float anchorX = -110f;

            CreateTextElement(
                DummyElement, "creatureStats", "$almanac_creature_resistances",
                0f, 250f,
                200f, 50f,
                Color.white, 20,
                active: true,
                overflowModes: TextOverflowModes.Overflow,
                wrapMode: TextWrappingModes.NoWrap
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
                    iconXPos, 220f - (index * 25f),
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
                    statsTagX, 220f - (index * 25f),
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
                    statListX, 220f - (index * 25f),
                    150f, 25f,
                    Color.white, 16,
                    horizontalAlignment: HorizontalAlignmentOptions.Center
                    );
            }

            CreateTextElement(
                DummyElement, "creatureDrops", "$almanac_creature_drops",
                0f, -55f,
                200f, 50f,
                Color.white, 20,
                overflowModes: TextOverflowModes.Overflow,
                wrapMode: TextWrappingModes.NoWrap
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
                overflowModes: TextOverflowModes.Overflow,
                wrapMode: TextWrappingModes.NoWrap
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
                overflowModes: TextOverflowModes.Overflow,
                wrapMode: TextWrappingModes.NoWrap
            );

            CreateAIStats(
                DummyElement,
                anchorX - 5f, -1050f,
                iconBackground
                );

            CreateTextElement(DummyElement, "KilledByLabel", "$almanac_killed_by_label",
                -80f, -1425f,
                200f, 50f,
                Color.white, 16
                );
            CreateTextElement(DummyElement, "KilledBy", "0",
                150f, -1425f,
                100f, 50f,
                Color.white, 18
            );
            
            CreateTextElement(DummyElement, "KilledPlayerLabel", "$almanac_died_to_label",
                -80f, -1450f,
                200f, 50f,
                Color.white, 16
            );
            CreateTextElement(DummyElement, "KilledPlayer", "0",
                150f, -1450f,
                100f, 50f,
                Color.white, 18
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
                250f, 50f,
                Color.white, 20,
                overflowModes: TextOverflowModes.Overflow,
                wrapMode: TextWrappingModes.NoWrap
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

        private static void AddHoverableText(
            GameObject obj, string content, int fontSize = 12, float anchoredX = 0f, float anchoredY = -35f,
            float sizeX = 80f, float sizeY = 30f,
            HorizontalAlignmentOptions horizontalAlignment = HorizontalAlignmentOptions.Center,
            VerticalAlignmentOptions verticalAlignment = VerticalAlignmentOptions.Middle,
            TextWrappingModes wrapMode = TextWrappingModes.NoWrap,
            TextOverflowModes overflow = TextOverflowModes.Overflow,
            bool maskable = false
            )
        {
            obj.AddComponent<ElementHover>();

            GameObject HoverTextElement = new GameObject("hoverTextElement");
            HoverTextElement.SetActive(false);

            RectTransform rectTransform = HoverTextElement.AddComponent<RectTransform>();
            rectTransform.SetParent(obj.transform);
            rectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            rectTransform.sizeDelta = new Vector2(sizeX, sizeY);

            TextMeshProUGUI text = HoverTextElement.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.color = Color.white;
            text.autoSizeTextContainer = false;
            text.text = content;
            text.overflowMode = overflow;
            text.fontSize = fontSize;
            text.horizontalAlignment = horizontalAlignment;
            text.verticalAlignment = verticalAlignment;
            text.textWrappingMode = wrapMode;
            text.maskable = maskable;
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
            text.enableAutoSizing = true; 
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