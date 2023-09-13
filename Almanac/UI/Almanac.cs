    using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.MonoBehaviors;
    using BepInEx.Logging;
    using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;
    using Object = UnityEngine.Object;

    namespace Almanac.UI;

public static class Almanac
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    public static class CreateAlmanac
    {
        public static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            RepositionTrophyPanel(__instance.m_trophiesPanel, -220f, 0f);
            CreateAlmanacPanel(__instance.m_trophiesPanel.transform, __instance.m_trophieElementPrefab);
            CreateRegenerateFileButton(__instance.m_trophiesPanel.transform);
            CreateCreaturesPanel(__instance.m_trophiesPanel.transform);
            CreateCreatureButton(__instance.m_trophiesPanel.transform);
            CreateTrophiesButton(__instance.m_trophiesPanel.transform);
        }

        private static void CreateRegenerateFileButton(Transform parentElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var closeButton = TrophiesFrame.Find("Closebutton");
            var closeText = closeButton.Find("Text");
            var closeButtonScript = closeButton.gameObject.GetComponent<Button>();
            var closeButtonSfx = closeButton.gameObject.GetComponent<ButtonSfx>();

            Font font = closeText.gameObject.GetComponent<Text>().font;
            Image closeImage = closeButton.GetComponent<Image>();

            if (!closeImage || !font || !closeButtonScript) return;

            GameObject RegenerateButton = new GameObject("RegenerateButton");
            RectTransform buttonRectTransform = RegenerateButton.AddComponent<RectTransform>();
            buttonRectTransform.SetParent(parentElement);
            buttonRectTransform.anchoredPosition = new Vector2(660f, -425f);
            buttonRectTransform.sizeDelta = new Vector2(170f, 46f);

            Image regenerateImage = RegenerateButton.AddComponent<Image>();
            regenerateImage.sprite = closeImage.sprite;
            regenerateImage.color = closeImage.color;
            regenerateImage.material = closeImage.material;
            regenerateImage.raycastTarget = true;
            regenerateImage.maskable = true;
            regenerateImage.type = closeImage.type;
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
            button.onClick.AddListener(CreatureDataCollector.CollectAndSaveCreatureData);

            GameObject text = new GameObject("RegenerateText");
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.SetParent(RegenerateButton.transform);
            textRect.anchoredPosition = new Vector2(0f, 0f);
            textRect.sizeDelta = new Vector2(150f, 40f);

            Text textContent = text.AddComponent<Text>();
            textContent.text = Localization.instance.Localize("$almanac_regenerate_almanac");
            textContent.font = font;
            textContent.alignment = TextAnchor.MiddleCenter;
            textContent.fontSize = 18;
            textContent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textContent.verticalOverflow = VerticalWrapMode.Truncate;
            textContent.color = new Color(0.8f, 0.5f, 0f, 1f);
            textContent.supportRichText = true;
            textContent.resizeTextMinSize = 1;
            textContent.resizeTextMaxSize = 25;
            textContent.resizeTextForBestFit = true;

            Outline outline = text.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 1f);
            outline.effectDistance = new Vector2(1f, -1f);

            var sfx = RegenerateButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }

        private static void CreateCreatureButton(Transform parentElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var closeButton = TrophiesFrame.Find("Closebutton");
            var closeText = closeButton.Find("Text");
            var closeButtonScript = closeButton.gameObject.GetComponent<Button>();
            var closeButtonSfx = closeButton.gameObject.GetComponent<ButtonSfx>();

            Font font = closeText.gameObject.GetComponent<Text>().font;
            Image closeImage = closeButton.GetComponent<Image>();

            if (!closeImage || !font || !closeButtonScript) return;

            GameObject CreatureButton = new GameObject("CreatureButton");
            RectTransform buttonRectTransform = CreatureButton.AddComponent<RectTransform>();
            buttonRectTransform.SetParent(parentElement);
            buttonRectTransform.anchoredPosition = new Vector2(325f, 365f);
            buttonRectTransform.sizeDelta = new Vector2(150f, 46f);

            Image backgroundImage = CreatureButton.AddComponent<Image>();
            backgroundImage.sprite = closeImage.sprite;
            backgroundImage.color = closeImage.color;
            backgroundImage.material = closeImage.material;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;
            backgroundImage.type = closeImage.type;
            backgroundImage.fillCenter = true;
            backgroundImage.pixelsPerUnitMultiplier = 1f;

            Button button = CreatureButton.AddComponent<Button>();
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
            button.onClick.AddListener(SetTopicCreatures);

            GameObject text = new GameObject("CreatureButtonText");
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.SetParent(CreatureButton.transform);
            textRect.anchoredPosition = new Vector2(0f, 0f);
            textRect.sizeDelta = new Vector2(150f, 30f);

            Text textContent = text.AddComponent<Text>();
            textContent.text = Localization.instance.Localize("$almanac_creature_button");
            textContent.font = font;
            textContent.alignment = TextAnchor.MiddleCenter;
            textContent.fontSize = 18;
            textContent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textContent.verticalOverflow = VerticalWrapMode.Truncate;
            textContent.color = new Color(0.8f, 0.5f, 0f, 1f);
            textContent.supportRichText = true;
            textContent.resizeTextMinSize = 1;
            textContent.resizeTextMaxSize = 25;
            textContent.resizeTextForBestFit = true;

            Outline outline = text.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 1f);
            outline.effectDistance = new Vector2(1f, -1f);

            var sfx = CreatureButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }

        private static void CreateTrophiesButton(Transform parentElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var closeButton = TrophiesFrame.Find("Closebutton");
            var closeText = closeButton.Find("Text");
            var closeButtonScript = closeButton.gameObject.GetComponent<Button>();
            var closeButtonSfx = closeButton.gameObject.GetComponent<ButtonSfx>();

            Font font = closeText.gameObject.GetComponent<Text>().font;
            Image closeImage = closeButton.GetComponent<Image>();

            if (!closeImage || !font || !closeButtonScript) return;

            GameObject TrophiesButton = new GameObject("TrophiesButton");
            RectTransform buttonRectTransform = TrophiesButton.AddComponent<RectTransform>();
            buttonRectTransform.SetParent(parentElement);
            buttonRectTransform.anchoredPosition = new Vector2(170f, 365f);
            buttonRectTransform.sizeDelta = new Vector2(150f, 46f);

            Image backgroundImage = TrophiesButton.AddComponent<Image>();
            backgroundImage.sprite = closeImage.sprite;
            backgroundImage.color = closeImage.color;
            backgroundImage.material = closeImage.material;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;
            backgroundImage.type = closeImage.type;
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
            button.onClick.AddListener(SetTopicTrophies);

            GameObject text = new GameObject("TrophiesButtonText");
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.SetParent(TrophiesButton.transform);
            textRect.anchoredPosition = new Vector2(0f, 0f);
            textRect.sizeDelta = new Vector2(150f, 30f);

            Text textContent = text.AddComponent<Text>();
            textContent.text = Localization.instance.Localize("$almanac_trophies_button");
            textContent.font = font;
            textContent.alignment = TextAnchor.MiddleCenter;
            textContent.fontSize = 18;
            textContent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textContent.verticalOverflow = VerticalWrapMode.Truncate;
            textContent.color = new Color(0.8f, 0.5f, 0f, 1f);
            textContent.supportRichText = true;
            textContent.resizeTextMinSize = 1;
            textContent.resizeTextMaxSize = 25;
            textContent.resizeTextForBestFit = true;

            Outline outline = text.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 1f);
            outline.effectDistance = new Vector2(1f, -1f);

            var sfx = TrophiesButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }

        private static void SetTopicCreatures()
        {
            var parentElement = InventoryGui.instance.m_trophiesPanel.transform;
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var topic = TrophiesFrame.Find("topic");
            var trophies = TrophiesFrame.Find("Trophies");

            var creaturePanel = TrophiesFrame.Find("creaturePanel");

            topic.gameObject.GetComponent<Text>().text = Localization.instance.Localize("$almanac_creature_button");
            trophies.gameObject.SetActive(false);
            creaturePanel.gameObject.SetActive(true);
        }

        private static void SetTopicTrophies()
        {
            var parentElement = InventoryGui.instance.m_trophiesPanel.transform;
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var topic = TrophiesFrame.Find("topic");
            var trophies = TrophiesFrame.Find("Trophies");

            var creaturePanel = TrophiesFrame.Find("creaturePanel");

            topic.gameObject.GetComponent<Text>().text = Localization.instance.Localize("$almanac_trophies_button");
            trophies.gameObject.SetActive(true);
            creaturePanel.gameObject.SetActive(false);
        }

        private static void CreateCreaturesPanel(Transform parentElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var trophies = TrophiesFrame.Find("Trophies");

            if (!TrophiesFrame) return;

            var creaturePanel = new GameObject("creaturePanel");
            RectTransform panelRect = creaturePanel.AddComponent<RectTransform>();
            panelRect.SetParent(TrophiesFrame);
            panelRect.anchoredPosition = new Vector2(0f, 0f);
            panelRect.sizeDelta = new Vector2(1310f, 800f);

            creaturePanel.SetActive(false);

            var background = new GameObject("creaturesBackground");
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.SetParent(creaturePanel.transform);
            rectTransform.anchoredPosition = new Vector2(0f, 10f);
            rectTransform.sizeDelta = new Vector2(1260f, 650f);

            Image backgroundImage = background.AddComponent<Image>();

            Image trophiesImage = trophies.gameObject.GetComponent<Image>();
            backgroundImage.color = trophiesImage.color;
            backgroundImage.raycastTarget = true;
            backgroundImage.maskable = true;
        }
        
        
        private static void CreateAlmanacPanel(Transform parentElement, GameObject trophyElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var border = TrophiesFrame.Find("border (1)");
            if (!TrophiesFrame || !border) return;

            var borderBgImage = border.GetComponent<Image>();
            var iconBg = trophyElement.transform.Find("icon_bkg").gameObject.GetComponent<Image>();
            if (!iconBg) return;

            Image cloneHandle = TrophiesFrame.transform.Find("Trophies").transform.Find("TrophyListScroll").transform
                .Find("Sliding Area").transform.Find("Handle").GetComponent<Image>();
            if (!cloneHandle) return;

            var position = new Vector2(880f, 0f);

            CreateBorder(TrophiesFrame, borderBgImage, position);
            GameObject AlmanacContentPanel = CreateContentPanel(TrophiesFrame, position);
            GameObject AlmanacScroll = CreateScrollElement(AlmanacContentPanel.transform, 190f, 0f);
            GameObject AlmanacSlidingArea = CreateSlidingArea(AlmanacScroll.transform);
            GameObject AlmanacScrollHandle = CreateScrollHandle(AlmanacSlidingArea.transform, borderBgImage);
            GameObject AlmanacList = CreateList(AlmanacContentPanel.transform);

            var TrophyText = trophyElement.transform.Find("name").GetComponent<Text>();
            if (!TrophyText) return;
            GameObject DummyElement = CreateDummyElement(AlmanacList.transform, TrophyText.font, iconBg);

            Scrollbar scrollbar = AddScrollbarComponent(AlmanacScroll, AlmanacScrollHandle, DummyElement);
            GameObject AlmanacListRoot = CreateListRoot(AlmanacList.transform);

            AddListScrollRect(AlmanacList, AlmanacListRoot, scrollbar);
            CreateWelcomePanel(AlmanacContentPanel.transform, borderBgImage, TrophyText.font);
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

        private static void CreateWelcomePanel(Transform parentElement, Image cloneImage, Font font)
        {
            GameObject WelcomePanel = new GameObject("WelcomePanel (0)");
            RectTransform rectTransform = WelcomePanel.AddComponent<RectTransform>();
            rectTransform.SetParent(parentElement);
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(400f, 800f);

            Image image = WelcomePanel.AddComponent<Image>();
            image.sprite = cloneImage.sprite;
            image.material = cloneImage.material;
            image.raycastTarget = cloneImage.raycastTarget;
            image.type = cloneImage.type;
            image.fillCenter = cloneImage.fillCenter;
            image.pixelsPerUnitMultiplier = 1;

            CreateCustomImageElement(WelcomePanel.transform, "logo", font, 0f, 0f, 400f, 800f,
                "Almanac.icons.AlmanacLogo.png", true, false, 1f);
        }

        private static void CreateBorder(Transform parentElement, Image cloneImage, Vector2 pos)
        {
            GameObject AlmanacBorder = new GameObject("Border");

            RectTransform AlmanacBorderRectTransform = AlmanacBorder.AddComponent<RectTransform>();
            AlmanacBorderRectTransform.SetParent(parentElement);
            AlmanacBorderRectTransform.anchoredPosition = pos;
            AlmanacBorderRectTransform.sizeDelta = new Vector2(400f, 800f);

            Image AlmanacBorderImage = AlmanacBorder.AddComponent<Image>();
            AlmanacBorderImage.sprite = cloneImage.sprite;
            AlmanacBorderImage.material = cloneImage.material;
            AlmanacBorderImage.raycastTarget = true;
            AlmanacBorderImage.type = Image.Type.Sliced;
            AlmanacBorderImage.fillCenter = true;
            AlmanacBorderImage.pixelsPerUnitMultiplier = 1;
        }


        private static GameObject CreateContentPanel(Transform parentElement, Vector2 pos)
        {
            GameObject AlmanacContentPanel = new GameObject("ContentPanel");

            RectTransform AlmanacContentPanelRectTransform = AlmanacContentPanel.AddComponent<RectTransform>();
            AlmanacContentPanelRectTransform.SetParent(parentElement);
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

        private static GameObject CreateScrollHandle(Transform parentElement, Image cloneImage)
        {
            GameObject AlmanacScrollHandle = new GameObject("Handle");
            RectTransform HandleRectTransform = AlmanacScrollHandle.AddComponent<RectTransform>();
            HandleRectTransform.SetParent(parentElement);
            HandleRectTransform.sizeDelta = new Vector2(10f, 50f);
            HandleRectTransform.anchoredPosition = new Vector2(250f, 0f);

            Image HandleImage = AlmanacScrollHandle.AddComponent<Image>();
            HandleImage.sprite = cloneImage.sprite;
            HandleImage.color = new Color(1f, 1f, 1f, 0f);
            HandleImage.type = cloneImage.type;
            HandleImage.fillCenter = cloneImage.fillCenter;
            HandleImage.raycastTarget = true;
            HandleImage.maskable = true;

            return AlmanacScrollHandle;
        }

        private static Scrollbar AddScrollbarComponent(GameObject targetGameObject, GameObject scrollHandle,
            GameObject element)
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

            RectTransform rectTransform = element.GetComponent<RectTransform>();
            scrollbar.onValueChanged.AddListener(call: (e) =>
            {
                rectTransform.anchoredPosition =
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

        private static GameObject CreateDummyElement(Transform parentElement, Font font, Image iconBackground)
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

            CreateTextElement(DummyElement, "displayName", "$almanac_untitled", font, 50f, 360f, 250f, 150f,
                Color.yellow, 25, TextAnchor.MiddleLeft, shadow: true);
            CreateTextElement(DummyElement, "faction", "$almanac_factionless", font, 0f, 320f, 150f, 50f, Color.white,
                16, TextAnchor.MiddleLeft);
            CreateTextElement(DummyElement, "health", "1000 HP", font, 110f, 320f, 100f, 50f, Color.white, 20,
                TextAnchor.MiddleRight, true);
            CreateImageElement(DummyElement, "icon", font, -145f, 340f, 120f, 120f, false);
            // panel left alignment
            float anchorX = -110f;

            CreateTextElement(DummyElement, "creatureStats", "$almanac_creature_resistances", font, anchorX, 300f,
                100f, 100f, Color.white, 20, active: true, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

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
                CreateCustomImageElement(DummyElement, iconList[index].Key, font, iconXPos, 230f - (index * 25f), 25f,
                    25f, iconList[index].Value, active: true);
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
                CreateTextElement(DummyElement, statsTagList[index].Key, localizedValue, font, statsTagX,
                    270f - (index * 25f), 100f, 100f, Color.white, 16);
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
                CreateTextElement(DummyElement, statsList[index], "$almanac_unknown", font, statListX,
                    270f - (index * 25f), 150f, 100f, Color.white, 16);
            }

            CreateTextElement(DummyElement, "creatureDrops", "$almanac_creature_drops", font, anchorX, 0f,
                100f, 100f, Color.white, 20, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

            for (var index = 0; index < 7; index++)
            {
                GameObject dropBackground = CreateImageElement(DummyElement, $"dropIconBg ({index})", font,
                    -155f + (index * 52), -100f, 50f, 50f, false, sprite: iconBackground.sprite);

                CreateImageElement(dropBackground.transform, $"creatureDrop ({index})", font, 0f, 0f, 45f, 45f, false,
                    true, shadow: true);
            }

            CreateTextElement(DummyElement, "defaultAttacks", "$almanac_creature_attacks", font, anchorX, -125f,
                100f, 100f, Color.white, 20, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

            for (var index = 0; index < 4; index++)
                CreateDefaultAttackStats(DummyElement, font, index, anchorX - 5f, -170f);

            CreateTextElement(DummyElement, "intelligence", "$almanac_creature_intelligence", font, anchorX, -975f,
                100f, 100f, Color.white, 20, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

            CreateAIStats(DummyElement, font, anchorX - 5f, -1050f, iconBackground);

            return DummyPanel;
        }

        private static void CreateAIStats(Transform parentElement, Font font, float x, float y, Image iconBackground)
        {
            CreateTextElement(parentElement, "avoidFireTag", "$almanac_avoid_fire", font, x + 50f, y, 200f, 25f,
                Color.white, 16);
            CreateTextElement(parentElement, "afraidOfFireTag", "$almanac_afraid_of_fire", font, x + 50f, y - 25f, 200f,
                25f, Color.white, 16);
            CreateTextElement(parentElement, "avoidWaterTag", "$almanac_avoid_water", font, x + 50f, y - 50f, 200f, 25f,
                Color.white, 16);

            CreateTextElement(parentElement, "avoidFire", "False", font, x + 250f, y, 100f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "afraidOfFire", "False", font, x + 250f, y - 25f, 100f, 25f, Color.white,
                16);
            CreateTextElement(parentElement, "avoidWater", "False", font, x + 250f, y - 50f, 100f, 25f, Color.white,
                16);

            CreateTextElement(parentElement, "tolerateWaterTag", "$almanac_tolerate_water", font, x + 50f, y - 75f,
                200f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateSmokeTag", "$almanac_tolerate_smoke", font, x + 50f, y - 100f,
                200f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateTarTag", "$almanac_tolerate_tar", font, x + 50f, y - 125f, 200f,
                25f, Color.white, 16);

            CreateTextElement(parentElement, "tolerateWater", "False", font, x + 250f, y - 75f, 100f, 25f, Color.white,
                16);
            CreateTextElement(parentElement, "tolerateSmoke", "False", font, x + 250f, y - 100f, 100f, 25f, Color.white,
                16);
            CreateTextElement(parentElement, "tolerateTar", "False", font, x + 250f, y - 125f, 100f, 25f, Color.white,
                16);

            CreateTextElement(parentElement, "staggerWhenBlockedTag", "$almanac_stagger_when_blocked", font, x + 50f,
                y - 150f, 200f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "staggerDamageFactorTag", "$almanac_stagger_damage_factor", font, x + 50f,
                y - 175f, 200f, 25f, Color.white, 16);

            CreateTextElement(parentElement, "staggerWhenBlocked", "False", font, x + 250f, y - 150f, 100f, 25f,
                Color.white, 16);
            CreateTextElement(parentElement, "staggerDamageFactor", "0", font, x + 250f, y - 175f, 100f, 25f,
                Color.white, 16);

            CreateTextElement(parentElement, "weakSpot", "$almanac_no_weak_spot", font, x + 50f, y - 200f, 200f, 25f,
                Color.yellow, 16);

            CreateImageElement(parentElement, $"attackOverlay (99)", font, 0f, y - 100f, 400f, 400f, true,
                false, null, 0f);

            CreateTextElement(parentElement, "consumeItemsTag", "$almanac_taming_food_items", font, x + 5f, y - 215f,
                100f, 100f, Color.white, 20, shadow: true, horizontalWrapMode: HorizontalWrapMode.Overflow);
            CreateTextElement(parentElement, "consumeItems (no data)", "$almanac_creature_not_tamable", font, x,
                y - 250f, 100f, 100f, Color.yellow, 16, active: false, horizontalWrapMode: HorizontalWrapMode.Overflow);
            for (var index = 0; index < 7; index++)
            {
                GameObject dummyBackground = CreateImageElement(parentElement, $"iconBg ({index})", font,
                    -155f + (index * 52), y - 315f, 50f, 50f, sprite: iconBackground.sprite, active: true, alpha: 1f);
                CreateImageElement(dummyBackground.transform, $"consumeItem ({index})", font, 0f, 0f, 45f, 45f,
                    addHoverableText: true, shadow: true);
            }
        }

        private static void CreateDefaultAttackStats(Transform parentElement, Font font, int index, float x, float y)
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
                    CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, font, x, anchorY,
                        100f, 25f, Color.white, 16);
                    CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value, font,
                        valueAnchorX + 50f, anchorY, 200f, 25f, Color.yellow, 16);
                }
                else
                {
                    CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, font, x, anchorY,
                        100f, 25f, Color.white, 16);
                    CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value, font,
                        valueAnchorX, anchorY, 100f, 25f, Color.white, 16);
                }
            }

            for (var i = 9; i < attackTagList.Count; ++i)
            {
                float anchorY = (y - position - (spacing * (i - 9)) - distanceFromTitle - 20f);
                float anchorX = x + 180f;
                float anchorValueX = anchorX + 130f;
                CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, font, anchorX, anchorY,
                    100f, 25f, Color.white, 16);
                CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value, font, anchorValueX,
                    anchorY, 100f, 25f, Color.white, 16);
            }

            CreateImageElement(parentElement, $"attackOverlay ({index})", font, 0f, y - position - 250f, 400f, 250f,
                true,
                false, null, 0f);
        }

        private static GameObject CreateImageElement(Transform parentElement, string id, Font font, float anchoredX,
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

            if (addHoverableText) AddHoverableText(imageElement, "Unknown", font);
            if (shadow)
            {
                var shadowElement = imageElement.AddComponent<Shadow>();
                shadowElement.effectColor = new Color(0f, 0f, 0f, 0.5f);
                shadowElement.effectDistance = new Vector2(4f, -3f);
                shadowElement.useGraphicAlpha = true;
            }

            return imageElement;
        }

        private static void CreateCustomImageElement(Transform parentElement, string id, Font font, float anchoredX,
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

            if (addHoverableText) AddHoverableText(imageElement, "Unknown", font);
        }

        private static void AddHoverableText(GameObject obj, string content, Font font)
        {
            obj.AddComponent<ElementHover>();

            GameObject HoverTextElement = new GameObject("hoverTextElement");
            HoverTextElement.SetActive(false);

            RectTransform rectTransform = HoverTextElement.AddComponent<RectTransform>();
            rectTransform.SetParent(obj.transform);
            rectTransform.anchoredPosition = new Vector2(0f, -35f);
            rectTransform.sizeDelta = new Vector2(80f, 30f);

            Text text = HoverTextElement.AddComponent<Text>();

            text.text = content;
            text.font = font;
            text.fontSize = 12;
            text.lineSpacing = 1;
            text.supportRichText = true;
            text.alignment = TextAnchor.MiddleCenter;
            text.alignByGeometry = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = false;
            text.resizeTextMinSize = 1;
            text.resizeTextMaxSize = 16;
            text.color = Color.white;
            text.raycastTarget = false;
        }

        private static void CreateTextElement(
            Transform parentElement, string id, string content, Font font,
            float anchoredX, float anchoredY, float sizeX, float sizeY,
            Color color, int fontSize = 10, TextAnchor alignment = TextAnchor.LowerLeft, bool active = true,
            bool shadow = false, HorizontalWrapMode horizontalWrapMode = HorizontalWrapMode.Wrap,
            VerticalWrapMode verticalWrapMode = VerticalWrapMode.Truncate)
        {
            GameObject textElement = new GameObject($"TextElement ({id})");
            textElement.SetActive(active);
            RectTransform textRectTransform = textElement.AddComponent<RectTransform>();
            textRectTransform.SetParent(parentElement);
            textRectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
            textRectTransform.sizeDelta = new Vector2(sizeX, sizeY);

            Text text = textElement.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.lineSpacing = 1;
            text.supportRichText = true;
            text.alignment = alignment;
            text.alignByGeometry = false;
            text.horizontalOverflow = horizontalWrapMode;
            text.verticalOverflow = verticalWrapMode;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 6;
            text.resizeTextMaxSize = fontSize;
            text.color = color;
            text.raycastTarget = false;

            text.text = Localization.instance.Localize(content);

            if (shadow)
            {
                var shadowElement = textElement.AddComponent<Shadow>();
                shadowElement.effectColor = new Color(0f, 0f, 0f, 0.5f);
                shadowElement.effectDistance = new Vector2(4f, -3f);
                shadowElement.useGraphicAlpha = true;
            }
        }

        private static void RepositionTrophyPanel(GameObject panel, float x, float y)
        {
            var trophiesFrame = panel.transform.Find("TrophiesFrame");
            var FrameRectTransform = trophiesFrame.GetComponent<RectTransform>();
            FrameRectTransform.anchoredPosition = new Vector2(x, y);
        }
        
        private static void SnapshotPiece(GameObject prefab, float lightIntensity = 1.3f, Quaternion? cameraRotation = null)
        {
        const int layer = 3;
        if (prefab == null) return;
        if (!prefab.GetComponentsInChildren<Renderer>().Any() && !prefab.GetComponentsInChildren<MeshFilter>().Any())
        {
            return;
        }

        Camera camera = new GameObject("CameraIcon", typeof(Camera)).GetComponent<Camera>();
        camera.backgroundColor = Color.clear;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(10000f, 10000f, 10000f);
        camera.transform.rotation = cameraRotation ?? Quaternion.Euler(0f, 180f, 0f);
        camera.fieldOfView = 0.5f;
        camera.farClipPlane = 100000;
        camera.cullingMask = 1 << layer;

        Light sideLight = new GameObject("LightIcon", typeof(Light)).GetComponent<Light>();
        sideLight.transform.position = new Vector3(10000f, 10000f, 10000f);
        sideLight.transform.rotation = Quaternion.Euler(5f, 180f, 5f);
        sideLight.type = LightType.Directional;
        sideLight.cullingMask = 1 << layer;
        sideLight.intensity = lightIntensity;

        GameObject visual = Object.Instantiate(prefab);
        foreach (Transform child in visual.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = layer;
        }

        visual.transform.position = Vector3.zero;
        visual.transform.rotation = Quaternion.Euler(23, 51, 25.8f);
        visual.name = prefab.name;

        MeshRenderer[] renderers = visual.GetComponentsInChildren<MeshRenderer>();
        Vector3 min = renderers.Aggregate(Vector3.positiveInfinity,
            (cur, renderer) => Vector3.Min(cur, renderer.bounds.min));
        Vector3 max = renderers.Aggregate(Vector3.negativeInfinity,
            (cur, renderer) => Vector3.Max(cur, renderer.bounds.max));
        // center the prefab
        visual.transform.position = (new Vector3(10000f, 10000f, 10000f)) - (min + max) / 2f;
        Vector3 size = max - min;

        // just in case it doesn't gets deleted properly later
        TimedDestruction timedDestruction = visual.AddComponent<TimedDestruction>();
        timedDestruction.Trigger(1f);
        Rect rect = new(0, 0, 128, 128);
        camera.targetTexture = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);

        camera.fieldOfView = 20f;
        // calculate the Z position of the prefab as it needs to be far away from the camera
        float maxMeshSize = Mathf.Max(size.x, size.y) + 0.1f;
        float distance = maxMeshSize / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad) * 1.1f;

        camera.transform.position = (new Vector3(10000f, 10000f, 10000f)) + new Vector3(0, 0, distance);

        camera.Render();

        RenderTexture currentRenderTexture = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        Texture2D previewImage = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        previewImage.ReadPixels(new Rect(0, 0, (int)rect.width, (int)rect.height), 0, 0);
        previewImage.Apply();

        RenderTexture.active = currentRenderTexture;

        prefab.GetComponent<Piece>().m_icon = Sprite.Create(previewImage,
            new Rect(0, 0, (int)rect.width, (int)rect.height), Vector2.one / 2f);
        sideLight.gameObject.SetActive(false);
        camera.targetTexture.Release();
        camera.gameObject.SetActive(false);
        visual.SetActive(false);
        Object.DestroyImmediate(visual);

        Object.Destroy(camera);
        Object.Destroy(sideLight);
        Object.Destroy(camera.gameObject);
        Object.Destroy(sideLight.gameObject);
    }
    }
}