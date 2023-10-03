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
        private static Font font = null!;
        private static Image closeButtonImage = null!;
        private static List<ItemDrop> materials = null!;
        private static List<ItemDrop> consummables = null!;
        private static List<ItemDrop> gear = null!;
        private static List<ItemDrop> weapons = null!;
        private static List<ItemDrop> fish = null!;
        private static List<ItemDrop> ammunitions = null!;
        private static List<CreatureDataCollector.CreatureData> creatures = null!;
        // private static Image handleImage = null!;
        private static Image borderImage = null!;
        private static Image iconBg = null!;
        
        public static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            
            trophyElement = __instance.m_trophieElementPrefab;
            TrophiesPanel = __instance.m_trophiesPanel.transform;
            TrophiesFrame = TrophiesPanel.Find("TrophiesFrame");

            var closeButton = TrophiesFrame.Find("Closebutton");
            font = closeButton.Find("Text").GetComponent<Text>().font;
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
            // handleImage = TrophiesFrame.Find("Trophies").Find("TrophyListScroll")
            //     .Find("Sliding Area").Find("Handle").GetComponent<Image>();

            borderImage = TrophiesFrame.Find("border (1)").GetComponent<Image>();

            RepositionTrophyPanel(-220f, 0f);
            CreateAlmanacPanel();
            CreateRegenerateFileButton();
            CreateCreaturesPanel();
            CreateMaterialPanel();
            CreatePanel("consummable", consummables);
            CreatePanel("gear", gear);
            CreatePanel("weapon", weapons);
            CreatePanel("ammo", ammunitions);
            
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

            Text textContent = text.AddComponent<Text>();
            textContent.text = Localization.instance.Localize($"$almanac_{name}_button");
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

            topic.gameObject.GetComponent<Text>().text = Localization.instance.Localize($"$almanac_{name}_button");
            
            materialPanel.gameObject.SetActive(name == "material");
            trophies.gameObject.SetActive(name == "trophies");
            creaturePanel.gameObject.SetActive(name == "creature");
            consummablePanel.gameObject.SetActive(name == "consummable");
            gearPanel.gameObject.SetActive(name == "gear");
            weaponPanel.gameObject.SetActive(name == "weapon");
            ammoPanel.gameObject.SetActive(name == "ammo");
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

            Text objTextComponent = objText.AddComponent<Text>();
            objTextComponent.text = $"{index + 1}";
            objTextComponent.alignment = TextAnchor.MiddleCenter;
            objTextComponent.font = font;
            objTextComponent.supportRichText = true;
            objTextComponent.resizeTextForBestFit = true;
            objTextComponent.color = new Color(1f, 0.5f, 0f, 0.8f);

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
            objButton.transition = Selectable.Transition.ColorTint;
            objButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
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
            
            AddHoverableText(container, name, 16);
            
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
                Patches.InventoryPatches.SetItemsData(element.gameObject, data);
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

            Text iconText = iconObj.AddComponent<Text>();
            iconText.text = "?";
            iconText.font = font;
            iconText.fontSize = 20;
            iconText.color = new Color(1f, 0.5f, 0f, 1f);
            iconText.resizeTextForBestFit = true;
            iconText.alignment = TextAnchor.MiddleCenter;

            // Image iconImage = iconObj.AddComponent<Image>();
            // iconImage.sprite = iconSprite;
            // iconImage.color = new Color(1f, 1f, 1f, 1f);
            // iconImage.pixelsPerUnitMultiplier = 1f;
            
            AddHoverableText(container, name, 16);
            
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
                Patches.InventoryPatches.SetItemsData(element.gameObject, data);
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

            Text objTextComponent = objText.AddComponent<Text>();
            objTextComponent.text = $"{index + 1}";
            objTextComponent.alignment = TextAnchor.MiddleCenter;
            objTextComponent.font = font;
            objTextComponent.supportRichText = true;
            objTextComponent.resizeTextForBestFit = true;
            objTextComponent.color = new Color(1f, 0.5f, 0f, 0.8f);

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
            objButton.transition = Selectable.Transition.ColorTint;
            objButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
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
            
            AddHoverableText(container, name, 18);
            
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
                var MaterialElement = AlmanacList.Find("MaterialElement (0)");
                SetActiveElement("material");
                Patches.InventoryPatches.SetMaterialData(MaterialElement.gameObject, data);
            });
        }

        private static void SetActiveElement(string name)
        {
            var AlmanacPanel = TrophiesFrame.Find("ContentPanel");
            var AlmanacList = AlmanacPanel.Find("AlmanacList");
            var MaterialElement = AlmanacList.Find("MaterialElement (0)");
            var AlmanacElement = AlmanacList.Find("AlmanacElement (0)");
            var ConsummableElement = AlmanacList.Find("consummableElement (0)");
            var GearElement = AlmanacList.Find("gearElement (0)");
            var WelcomePanel = AlmanacPanel.Find("WelcomePanel (0)");
                
            WelcomePanel.gameObject.SetActive(name == "welcome");
            MaterialElement.gameObject.SetActive(name == "material");
            AlmanacElement.gameObject.SetActive(name == "creatures");
            ConsummableElement.gameObject.SetActive(name == "consummable");
            GearElement.gameObject.SetActive(name == "gear");
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

            Text objTextComponent = objText.AddComponent<Text>();
            objTextComponent.text = $"{index + 1}";
            objTextComponent.alignment = TextAnchor.MiddleCenter;
            objTextComponent.font = font;
            objTextComponent.supportRichText = true;
            objTextComponent.resizeTextForBestFit = true;
            objTextComponent.color = new Color(1f, 0.5f, 0f, 0.8f);

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
            objButton.transition = Selectable.Transition.ColorTint;
            objButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
                selectedColor = new Color(1f, 1f, 1f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
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
            
                if (i > 99f)
                {
                    x = -500f + ((i - 100) / 20) * xSpacing;
                    y = 305f - ((i - 100) % 20) * ySpacing;
                    
                    Vector2 position = new Vector2(x, y);
                    CreateCreatureButton(AlmanacElement, parentElement, position, i, Localization.instance.Localize(name), false);
                }
                else
                {
                    Vector2 position = new Vector2(x, y);
                    CreateCreatureButton(AlmanacElement ,parentElement, position, i, Localization.instance.Localize(name), true);
                }
            }
            
        }

        private static void CreateCreatureButton(Transform AlmanacElement, Transform parentElement, Vector2 position, int i,
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
            
            Text text = objText.AddComponent<Text>();
            text.font = font;
            text.color = Color.yellow;
            text.text = content;
            text.alignment = TextAnchor.MiddleCenter;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            
            button.targetGraphic = ContainerBg;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                SetActiveElement("creature");
                Patches.InventoryPatches.setAlmanacData(AlmanacElement.gameObject, content);
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
            GameObject MaterialElement = CreateMaterialElement(AlmanacList.transform);
            GameObject ConsummableElement = CreateItemElement(AlmanacList.transform, "consummable");
            GameObject GearElement = CreateItemElement(AlmanacList.transform, "gear");
            GameObject WeaponElement = CreateItemElement(AlmanacList.transform, "weapon");
            GameObject AmmoElement = CreateItemElement(AlmanacList.transform, "ammo");
            
            MaterialElement.SetActive(false);
            ConsummableElement.SetActive(false);
            GearElement.SetActive(false);
            WeaponElement.SetActive(false);
            AmmoElement.SetActive(false);

            Scrollbar scrollbar = AddScrollbarComponent(AlmanacScroll, AlmanacScrollHandle, AlmanacElement);
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

            CreateTextElement(DummyElement, "Name", "no data", 50f, 360f, 250f, 150f,
                Color.yellow, 25, TextAnchor.MiddleLeft, shadow: true);
            CreateTextElement(DummyElement, "Description", "no data", 0f, 320f, 150f, 50f, Color.white,
                16, TextAnchor.MiddleLeft);
            CreateTextElement(DummyElement, "maxStackSize", $"Stack size: no data", 110f, 320f, 100f, 50f, Color.white, 20,
                TextAnchor.MiddleRight, true);
            CreateImageElement(DummyElement, "icon", -145f, 340f, 120f, 120f, false);
            // panel left alignment
            float anchorX = -110f;

            CreateTextElement(DummyElement, "stats", $"$almanac_{id}_stats", anchorX, 300f,
                100f, 100f, Color.white, 20, active: true, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);
            
            
            return DummyPanel;
        }
        private static GameObject CreateMaterialElement(Transform parentElement)
        {
            GameObject DummyPanel = new GameObject("MaterialElement (0)");
            RectTransform DummyRectTransform = DummyPanel.AddComponent<RectTransform>();
            DummyRectTransform.SetParent(parentElement);
            DummyRectTransform.anchoredPosition = new Vector2(0f, 0f);
            DummyRectTransform.sizeDelta = new Vector2(390f, 750f);

            Image dummyImage = DummyPanel.AddComponent<Image>();
            dummyImage.fillCenter = true;
            dummyImage.color = new Color(0f, 0f, 0f, 0f);
            dummyImage.raycastTarget = true;
            
            Transform DummyElement = DummyPanel.transform;

            CreateTextElement(DummyElement, "materialName", "no data", 50f, 360f, 250f, 150f,
                Color.yellow, 25, TextAnchor.MiddleLeft, shadow: true);
            CreateTextElement(DummyElement, "materialDescription", "no data", 0f, 320f, 150f, 50f, Color.white,
                16, TextAnchor.MiddleLeft);
            CreateTextElement(DummyElement, "maxStackSize", $"Stack size: no data", 110f, 320f, 100f, 50f, Color.white, 20,
                TextAnchor.MiddleRight, true);
            CreateImageElement(DummyElement, "icon", -145f, 340f, 120f, 120f, false);
            // panel left alignment
            float anchorX = -110f;

            CreateTextElement(DummyElement, "materialStats", "$almanac_material_stats", anchorX, 300f,
                100f, 100f, Color.white, 20, active: true, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);
            
            
            return DummyPanel;
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

            CreateTextElement(DummyElement, "displayName", "$almanac_untitled", 50f, 360f, 250f, 150f,
                Color.yellow, 25, TextAnchor.MiddleLeft, shadow: true);
            CreateTextElement(DummyElement, "faction", "$almanac_factionless", 0f, 320f, 150f, 50f, Color.white,
                16, TextAnchor.MiddleLeft);
            CreateTextElement(DummyElement, "health", "1000 HP", 110f, 320f, 100f, 50f, Color.white, 20,
                TextAnchor.MiddleRight, true);
            CreateImageElement(DummyElement, "icon", -145f, 340f, 120f, 120f, false);
            // panel left alignment
            float anchorX = -110f;

            CreateTextElement(DummyElement, "creatureStats", "$almanac_creature_resistances", anchorX, 300f,
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
                CreateCustomImageElement(DummyElement, iconList[index].Key, iconXPos, 230f - (index * 25f), 25f,
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
                CreateTextElement(DummyElement, statsTagList[index].Key, localizedValue, statsTagX,
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
                CreateTextElement(DummyElement, statsList[index], "$almanac_unknown", statListX,
                    270f - (index * 25f), 150f, 100f, Color.white, 16);
            }

            CreateTextElement(DummyElement, "creatureDrops", "$almanac_creature_drops", anchorX, 0f,
                100f, 100f, Color.white, 20, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

            for (var index = 0; index < 7; index++)
            {
                GameObject dropBackground = CreateImageElement(DummyElement, $"dropIconBg ({index})",
                    -155f + (index * 52), -100f, 50f, 50f, false, sprite: iconBackground.sprite);

                CreateImageElement(dropBackground.transform, $"creatureDrop ({index})", 0f, 0f, 45f, 45f, false,
                    true, shadow: true);
            }

            CreateTextElement(DummyElement, "defaultAttacks", "$almanac_creature_attacks", anchorX, -125f,
                100f, 100f, Color.white, 20, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

            for (var index = 0; index < 4; index++)
                CreateDefaultAttackStats(DummyElement, index, anchorX - 5f, -170f);

            CreateTextElement(DummyElement, "intelligence", "$almanac_creature_intelligence", anchorX, -975f,
                100f, 100f, Color.white, 20, shadow: true,
                horizontalWrapMode: HorizontalWrapMode.Overflow, verticalWrapMode: VerticalWrapMode.Overflow);

            CreateAIStats(DummyElement, anchorX - 5f, -1050f, iconBackground);

            return DummyPanel;
        }

        private static void CreateAIStats(Transform parentElement, float x, float y, Image iconBackground)
        {
            CreateTextElement(parentElement, "avoidFireTag", "$almanac_avoid_fire", x + 50f, y, 200f, 25f,
                Color.white, 16);
            CreateTextElement(parentElement, "afraidOfFireTag", "$almanac_afraid_of_fire", x + 50f, y - 25f, 200f,
                25f, Color.white, 16);
            CreateTextElement(parentElement, "avoidWaterTag", "$almanac_avoid_water", x + 50f, y - 50f, 200f, 25f,
                Color.white, 16);

            CreateTextElement(parentElement, "avoidFire", "False", x + 250f, y, 100f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "afraidOfFire", "False", x + 250f, y - 25f, 100f, 25f, Color.white,
                16);
            CreateTextElement(parentElement, "avoidWater", "False", x + 250f, y - 50f, 100f, 25f, Color.white,
                16);

            CreateTextElement(parentElement, "tolerateWaterTag", "$almanac_tolerate_water", x + 50f, y - 75f,
                200f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateSmokeTag", "$almanac_tolerate_smoke", x + 50f, y - 100f,
                200f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "tolerateTarTag", "$almanac_tolerate_tar", x + 50f, y - 125f, 200f,
                25f, Color.white, 16);

            CreateTextElement(parentElement, "tolerateWater", "False", x + 250f, y - 75f, 100f, 25f, Color.white,
                16);
            CreateTextElement(parentElement, "tolerateSmoke", "False", x + 250f, y - 100f, 100f, 25f, Color.white,
                16);
            CreateTextElement(parentElement, "tolerateTar", "False", x + 250f, y - 125f, 100f, 25f, Color.white,
                16);

            CreateTextElement(parentElement, "staggerWhenBlockedTag", "$almanac_stagger_when_blocked", x + 50f,
                y - 150f, 200f, 25f, Color.white, 16);
            CreateTextElement(parentElement, "staggerDamageFactorTag", "$almanac_stagger_damage_factor", x + 50f,
                y - 175f, 200f, 25f, Color.white, 16);

            CreateTextElement(parentElement, "staggerWhenBlocked", "False", x + 250f, y - 150f, 100f, 25f,
                Color.white, 16);
            CreateTextElement(parentElement, "staggerDamageFactor", "0", x + 250f, y - 175f, 100f, 25f,
                Color.white, 16);

            CreateTextElement(parentElement, "weakSpot", "$almanac_no_weak_spot", x + 50f, y - 200f, 200f, 25f,
                Color.yellow, 16);

            CreateImageElement(parentElement, $"attackOverlay (99)", 0f, y - 100f, 400f, 400f, true,
                false, null, 0f);

            CreateTextElement(parentElement, "consumeItemsTag", "$almanac_taming_food_items", x + 5f, y - 215f,
                100f, 100f, Color.white, 20, shadow: true, horizontalWrapMode: HorizontalWrapMode.Overflow);
            CreateTextElement(parentElement, "consumeItems (no data)", "$almanac_creature_not_tamable", x,
                y - 250f, 100f, 100f, Color.yellow, 16, active: false, horizontalWrapMode: HorizontalWrapMode.Overflow);
            for (var index = 0; index < 7; index++)
            {
                GameObject dummyBackground = CreateImageElement(parentElement, $"iconBg ({index})",
                    -155f + (index * 52), y - 315f, 50f, 50f, sprite: iconBackground.sprite, active: true, alpha: 1f);
                CreateImageElement(dummyBackground.transform, $"consumeItem ({index})", 0f, 0f, 45f, 45f,
                    addHoverableText: true, shadow: true);
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
                    CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, x, anchorY,
                        100f, 25f, Color.white, 16);
                    CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value,
                        valueAnchorX + 50f, anchorY, 200f, 25f, Color.yellow, 16);
                }
                else
                {
                    CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, x, anchorY,
                        100f, 25f, Color.white, 16);
                    CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value,
                        valueAnchorX, anchorY, 100f, 25f, Color.white, 16);
                }
            }

            for (var i = 9; i < attackTagList.Count; ++i)
            {
                float anchorY = (y - position - (spacing * (i - 9)) - distanceFromTitle - 20f);
                float anchorX = x + 180f;
                float anchorValueX = anchorX + 130f;
                CreateTextElement(parentElement, attackTagList[i].Key, attackTagList[i].Value, anchorX, anchorY,
                    100f, 25f, Color.white, 16);
                CreateTextElement(parentElement, attackValueList[i].Key, attackValueList[i].Value, anchorValueX,
                    anchorY, 100f, 25f, Color.white, 16);
            }

            CreateImageElement(parentElement, $"attackOverlay ({index})", 0f, y - position - 250f, 400f, 250f,
                true,
                false, null, 0f);
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

        private static void CreateCustomImageElement(Transform parentElement, string id, float anchoredX,
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
        }

        private static void AddHoverableText(GameObject obj, string content, int fontSize = 12)
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
            text.fontSize = fontSize;
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
            Transform parentElement, string id, string content,
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

        private static void RepositionTrophyPanel(float x, float y)
        {
            var FrameRectTransform = TrophiesFrame.GetComponent<RectTransform>();
            FrameRectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}