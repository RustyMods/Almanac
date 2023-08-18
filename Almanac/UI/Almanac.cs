using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LocalizationManager;
using UnityEngine;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;

namespace Almanac;

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
        }

        private static void CreateRegenerateFileButton(Transform parentElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var Closebutton = TrophiesFrame.Find("Closebutton");
            var closeText = Closebutton.Find("Text");
            var closeButtonScript = Closebutton.gameObject.GetComponent<Button>();
            var closeButtonSfx = Closebutton.gameObject.GetComponent<ButtonSfx>();
            
            Font font = closeText.gameObject.GetComponent<Text>().font;
            Image closeImage = Closebutton.GetComponent<Image>();

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

            var sfx = RegenerateButton.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = closeButtonSfx.m_sfxPrefab;
        }
        
        private static void CreateAlmanacPanel(Transform parentElement, GameObject trophyElement)
        {
            var TrophiesFrame = parentElement.Find("TrophiesFrame");
            var border = TrophiesFrame.Find("border (1)");
            if (!TrophiesFrame || !border) return;
            // Get panel background image
            var borderBgImage = border.GetComponent<Image>();
            // Get the blue background image from trophy list
            var iconBg = trophyElement.transform.Find("icon_bkg").gameObject.GetComponent<Image>();
            if (!iconBg) return;
            
            Image cloneHandle = TrophiesFrame.transform.Find("Trophies").transform.Find("TrophyListScroll").transform
                .Find("Sliding Area").transform.Find("Handle").GetComponent<Image>();
            if (!cloneHandle) return;
            
            Vector2 position = new Vector2(880f, 0f);
            
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

            CreateCustomImageElement(WelcomePanel.transform, "logo", font, 0f, 0f, 400f, 800f, "Almanac.icons.AlmanacLogo.png", true, false, 1f);
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

        private static Scrollbar AddScrollbarComponent(GameObject targetGameObject, GameObject scrollHandle, GameObject element)
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
            scrollbar.onValueChanged.AddListener(call: (e) => { rectTransform.anchoredPosition = e < 0.5f ? new Vector2(0f, (e - 0.5f) * -2400f) : new Vector2(0f, (e - 0.5f) * 1f); });
            
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
            
            CreateTextElement(DummyElement, "displayName", "$almanac_untitled", font, -30f, 360f, 100f, 150f, Color.yellow, 25, TextAnchor.MiddleLeft, shadow: true);
            CreateTextElement(DummyElement, "faction", "$almanac_factionless", font, -30f, 320f, 100f, 100f, Color.white, 16, TextAnchor.MiddleLeft);
            CreateTextElement(DummyElement, "health", "1000 HP", font, 100f, 320f, 100f, 100f, Color.white, 20, TextAnchor.MiddleRight, true);
            CreateImageElement(DummyElement,"icon", font, -145f, 340f, 120f, 120f, false);
            // panel left alignment
            float anchorX = -110f;
            
            CreateTextElement(DummyElement, "creatureStats", "$almanac_creature_resistances", font, anchorX, 300f, 100f, 100f, Color.white, 20, active: true, shadow: true);
            
            // Create Icons
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
                CreateCustomImageElement(DummyElement, iconList[index].Key, font, iconXPos, 230f - (index * 25f), 25f, 25f, iconList[index].Value, active: true);
            }
            // Create Tags
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
                CreateTextElement(DummyElement, statsTagList[index].Key, localizedValue, font, statsTagX, 270f - (index * 25f),  100f, 100f, Color.white, 16);
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
                CreateTextElement(DummyElement, statsList[index], "$almanac_unknown", font, statListX, 270f - (index * 25f), 100f, 100f, Color.white, 16);
            }
            CreateTextElement(DummyElement, "creatureDrops", "$almanac_creature_drops", font, anchorX, 0f, 100f, 100f, Color.white, 20, shadow: true);
            for (var index = 0; index < 7; index++)
            {
                GameObject dropBackground = CreateImageElement(DummyElement, $"dropIconBg ({index})", font,
                    -155f + (index * 52), -100f, 50f, 50f, false, sprite: iconBackground.sprite);
                
                CreateImageElement(dropBackground.transform, $"creatureDrop ({index})", font, 0f, 0f, 45f, 45f, false, true, shadow: true);
            }

            CreateTextElement(DummyElement, "defaultAttacks", "$almanac_creature_attacks", font, anchorX, -125f, 100f, 100f, Color.white, 20, shadow: true);
            for (var index = 0; index < 4; index++) CreateDefaultAttackStats(DummyElement, font, index, anchorX - 5f, -140f);
            
            CreateTextElement(DummyElement, "intelligence", "$almanac_creature_intelligence", font, anchorX, -1085f, 100f, 100f, Color.white, 20, shadow: true);
            
            CreateAIStats(DummyElement, font, anchorX - 5f, -1125f, iconBackground);

            return DummyPanel;
        }

        private static void CreateAIStats(Transform parentElement, Font font, float x, float y, Image iconBackground)
        {
            CreateTextElement(parentElement, "avoidFireTag", "$almanac_avoid_fire", font, x, y, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "afraidOfFireTag", "$almanac_afraid_of_fire", font, x, y - 25f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "avoidWaterTag", "$almanac_avoid_water", font, x, y - 50f, 100f, 100f, Color.white, 16);
            
            CreateTextElement(parentElement, "avoidFire", "False", font, x + 250f, y, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "afraidOfFire", "False", font, x + 250f, y - 25f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "avoidWater", "False", font, x + 250f, y - 50f, 100f, 100f, Color.white, 16);

            CreateTextElement(parentElement, "tolerateWaterTag", "$almanac_tolerate_water", font, x, y - 75f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateSmokeTag", "$almanac_tolerate_smoke", font, x, y - 100f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateTarTag", "$almanac_tolerate_tar", font, x, y - 125f, 100f, 100f, Color.white, 16);
            
            CreateTextElement(parentElement, "tolerateWater", "False", font, x + 250f, y - 75f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateSmoke", "False", font, x + 250f, y - 100f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateTar", "False", font, x + 250f, y - 125f, 100f, 100f, Color.white, 16);
            
            CreateTextElement(parentElement, "staggerWhenBlockedTag", "$almanac_stagger_when_blocked", font, x, y - 150f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "staggerDamageFactorTag", "$almanac_stagger_damage_factor", font, x, y - 175f, 100f, 100f, Color.white, 16);
            
            CreateTextElement(parentElement, "staggerWhenBlocked", "False", font, x + 250f, y - 150f, 100f, 100f, Color.white, 16);
            CreateTextElement(parentElement, "staggerDamageFactor", "0", font, x + 250f, y - 175f, 100f, 100f, Color.white, 16);
            
            CreateTextElement(parentElement, "weakSpot", "$almanac_no_weak_spot", font, x, y - 200f, 100f, 100f, Color.yellow, 16);
            
            CreateImageElement(parentElement, $"attackOverlay (99)", font, 0f, y - 100f, 400f, 200f, true,
                false, null, 0f);

            CreateTextElement(parentElement, "consumeItemsTag", "$almanac_taming_food_items", font, x, y - 250f, 100f, 100f, Color.white, 20, shadow: true);
            CreateTextElement(parentElement, "consumeItems (no data)", "$almanac_creature_not_tamable", font, x, y - 275f, 100f, 100f, Color.yellow, 16, active: false);
            for (var index = 0; index < 7; index++)
            {
                GameObject dummyBackground = CreateImageElement(parentElement, $"iconBg ({index})", font,
                    -155f + (index * 52), y - 350f, 50f, 50f, sprite: iconBackground.sprite, active: true, alpha: 1f);
                CreateImageElement(dummyBackground.transform, $"consumeItem ({index})", font, 0f, 0f, 45f, 45f, addHoverableText: true, shadow: true);
            }
        }
        private static void CreateDefaultAttackStats(Transform parentElement, Font font, int index, float x, float y)
        {
            float position = index * 225f;
            float spacing = 25f;
            float distanceFromTitle = 25f;

            Dictionary<string, string> attackTags = new Dictionary<string, string>()
            {
                { $"attackNameTag ({index})", "$almanac_attack_name" },
                { $"attackBluntTag ({index})", "$almanac_blunt"},
                { $"attackSlashTag ({index})", "$almanac_slash"},
                { $"attackPierceTag ({index})", "$almanac_pierce"},
                { $"attackChopTag ({index})", "$almanac_chop"},
                { $"attackPickaxeTag ({index})", "$almanac_pickaxe"},
                { $"attackAttackForceTag ({index})", "$almanac_attack_force"},
                { $"attackDodgeableTag ({index})", "$almanac_dodgeable"},
                { $"attackStatusEffectTag ({index})", "$almanac_status_effect"},
                { $"attackFireTag ({index})", "$almanac_fire"},
                { $"attackFrostTag ({index})", "$almanac_frost"},
                { $"attackLightningTag ({index})", "$almanac_lightning"},
                { $"attackPoisonTag ({index})", "$almanac_poison"},
                { $"attackSpiritTag ({index})", "$almanac_spirit"},
                { $"attackBackStabBonusTag ({index})", "$almanac_back_stab_bonus"},
                { $"attackBlockableTag ({index})", "$almanac_blockable"}
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
                { $"attackStatusEffect ({index})", "None"},
                
                { $"attackFire ({index})", "0" },
                { $"attackFrost ({index})", "0" },
                { $"attackLightning ({index})", "0" },
                { $"attackPoison ({index})", "0" },
                { $"attackSpirit ({index})", "0" },
                { $"attackBackStabBonus ({index})", "0" },
                { $"attackBlockable ({index})", "0" },
            };
            List<KeyValuePair<string, string>> attackValueList = attackValues.ToList();
            for (var i = 0; i < 9; i++)
            {
                float anchorY = (y - position - (spacing * i) - distanceFromTitle);
                float valueAnchorX = x + 120f;
                CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, font, x, anchorY, 100f, 100f, Color.white, 16);
                CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value, font, valueAnchorX, anchorY, 100f, 100f, (i == 0) ? Color.yellow : Color.white, 16);
            }
            for (var i = 9; i < attackTagList.Count; i++)
            {
                float anchorY = (y - position - (spacing * (i - 9)) - distanceFromTitle - 25f);
                float anchorX = x + 180f;
                float anchorValueX = anchorX + 130f;
                CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, font, anchorX, anchorY, 100f, 100f, Color.white, 16);
                CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value, font, anchorValueX, anchorY, 100f, 100f, Color.white, 16);
            }
            CreateImageElement(parentElement, $"attackOverlay ({index})", font, 0f, y - position - 250f, 400f, 250f, true,
                false, null, 0f);
        }
        private static GameObject CreateImageElement(Transform parentElement, string id,Font font, float anchoredX,
            float anchoredY, float sizeX, float sizeY, bool active = false, bool addHoverableText = false, Sprite? sprite = null, float alpha = 1f, bool shadow = false)
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
        
        private static void CreateCustomImageElement(Transform parentElement, string id,Font font, float anchoredX,
            float anchoredY, float sizeX, float sizeY, string imagePath, bool active = false, bool addHoverableText = false, float alpha = 1f)
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
            Color color , int fontSize = 10, TextAnchor alignment = TextAnchor.LowerLeft, bool active = true,
            bool shadow = false)
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
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 1;
            text.resizeTextMaxSize = 32;
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
    }
}