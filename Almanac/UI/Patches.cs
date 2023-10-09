using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Almanac.MonoBehaviors;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace Almanac.UI;

public static class Patches
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateTrophyList))]
    static class UpdateTrophyListPatch
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
        
            var trophyList = __instance.m_trophyList;
            var bossNames = new List<string>()
            {
                Localization.instance.Localize("$enemy_eikthyr"),
                Localization.instance.Localize("$enemy_gdking"),
                Localization.instance.Localize("$enemy_bonemass"),
                Localization.instance.Localize("$enemy_dragon"),
                Localization.instance.Localize("$enemy_goblinking"),
                Localization.instance.Localize("$enemy_seekerqueen")
            };
            var uniqueVectorSet = new HashSet<Vector3>();
            
            foreach (var trophy in trophyList)
            {
                var trophyName = trophy.transform.Find("name");
                var trophyPos = trophy.transform.position;
                // var panelDisplayName = trophyName.gameObject.GetComponent<Text>().text;
                var panelDisplayName = trophyName.gameObject.GetComponent<TextMeshProUGUI>().text;
                
                if (Localization.instance.Localize(panelDisplayName).ToLower().Contains("troll"))
                {
                    if (!(Math.Abs(trophyPos.x - 1010f) < 5f) || !(Math.Abs(trophyPos.y - 694f) < 5f)) continue;
                    trophy.transform.position = new Vector3(830f, 874f, 0.0f);
                };
                if (bossNames.Contains(Localization.instance.Localize(panelDisplayName))) uniqueVectorSet.Add(trophyPos);
                
            }
            
            foreach (var trophy in trophyList)
            {
                var trophyPos = trophy.transform.position;
                // var trophyName = trophy.transform.Find("name").gameObject.GetComponent<Text>().text;
                var trophyName = trophy.transform.Find("name").gameObject.GetComponent<TextMeshProUGUI>().text;
        
                if (uniqueVectorSet.Contains(trophyPos) && Localization.instance.Localize(trophyName).ToLower() != Localization.instance.Localize("draugr") && !bossNames.Contains(trophyName))
                {
                    trophyPos = TryMoveTrophy(trophyPos, uniqueVectorSet);
                }
                uniqueVectorSet.Add(trophyPos);
                trophy.transform.position = trophyPos;
            }
        }
        private static Vector2 TryMoveTrophy(Vector3 position, HashSet<Vector3> uniqueVectors)
        {
            float increment = 180f;
    
            var currentX = position.x;
            var currentY = position.y;
    
            position = currentX + increment >= 1190f ? new Vector3(110f, currentY - increment, 0.0f) : new Vector3(position.x + increment, currentY, 0.0f);
            if (uniqueVectors.Contains(position)) position = TryMoveTrophy(position, uniqueVectors);
            
            return position;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCloseTrophies))]
    static class OnCloseTrophiesPatch
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            var trophyFrame = __instance.m_trophiesPanel.transform.Find("TrophiesFrame");
            var contentPanel = trophyFrame.transform.Find("ContentPanel");
            var welcomePanel = contentPanel.transform.Find("WelcomePanel (0)");
            var almanacList = contentPanel.transform.Find("AlmanacList");
            var dummyElement = almanacList.transform.Find("AlmanacElement (0)");
            
            dummyElement.gameObject.SetActive(false);
            welcomePanel.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnOpenTrophies))]
    public static class OnOpenTrophiesPatch
    {
        private static void Postfix(InventoryGui __instance)
        {
            SetUnknownCreatures(
                __instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off
            );
            SetUnknownMaterials(__instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off);
            SetUnknownItems(__instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off,
                "consummable", Almanac.CreateAlmanac.consummables);
            SetUnknownItems(__instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off,
                "weapon", Almanac.CreateAlmanac.weapons);
            SetUnknownItems(__instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off,
                "gear", Almanac.CreateAlmanac.gear);
            SetUnknownItems(__instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off,
                "ammo", Almanac.CreateAlmanac.ammunitions);
            SetUnknownItems(__instance,
                AlmanacPlugin._CreatureKnowledgeLock.Value == AlmanacPlugin.Toggle.On
                    ? AlmanacPlugin.Toggle.On
                    : AlmanacPlugin.Toggle.Off,
                "fish", Almanac.CreateAlmanac.fish);
        
            var trophyList = __instance.m_trophyList;
            var trophyFrame = __instance.m_trophiesPanel.transform.Find("TrophiesFrame");
            var contentPanel = trophyFrame.Find("ContentPanel");
        
            var AlmanacList = contentPanel.Find("AlmanacList");
            var closeButton = trophyFrame.Find("Closebutton");
        
            ButtonSfx buttonSfx = closeButton.gameObject.GetComponent<ButtonSfx>();
        
            if (trophyList == null || !trophyFrame || !contentPanel || !AlmanacList) return;
        
            foreach (var trophy in trophyList) AddButtonComponent(trophy, AlmanacList, contentPanel, buttonSfx);
        }

        private static void SetUnknownCreatures(InventoryGui __instance, AlmanacPlugin.Toggle toggle)
        {
            var creatures = Almanac.CreateAlmanac.creatures;
            var trophyPanel = __instance.m_trophiesPanel;
            var trophyFrame = trophyPanel.transform.Find("TrophiesFrame");
            var creaturePanel = trophyFrame.transform.Find("creaturePanel");
            for (int i = 0; i < creatures.Count; ++i)
            {
                var container = creaturePanel.Find($"CreatureContainer ({i})");
                var faction = creatures[i].faction;
                bool isWise = true;
                if (toggle == AlmanacPlugin.Toggle.On)
                {
                    var globalKeys = Player.m_localPlayer.m_uniques;
                    Dictionary<string, string> requiredKeys = new Dictionary<string, string>
                    {
                        { "ForestMonsters", "defeated_gdking" },
                        { "Undead", "defeated_bonemass" },
                        { "MountainMonsters", "defeated_dragon" },
                        { "PlainsMonsters", "defeated_goblinking" },
                        { "MistlandsMonsters", "defeated_goblinking" },
                        { "Dverger", "defeated_goblinking" },
                        { "AnimalsVeg", "defeated_goblinking" },
                        { "Boss", "defeated_goblinking" },
                        { "SeaMonsters", "defeated_dragon" },
                        { "Demon", "defeated_goblinking" }
                    };
                    if (faction != null && requiredKeys.TryGetValue(faction, out string requiredKey))
                    {
                        if (!globalKeys.Contains(requiredKey)) isWise = false;
                    }
                }

                Button button = container.gameObject.GetComponent<Button>();
                TextMeshProUGUI text = container.Find($"CreatureContainer Text ({i})").gameObject.GetComponent<TextMeshProUGUI>();

                button.interactable = isWise;
                text.text = isWise
                    ? Localization.instance.Localize(creatures[i].display_name)
                    : Localization.instance.Localize("$almanac_locked");
                text.color = isWise
                    ? Color.yellow
                    : Color.gray;
            }
        }

        private static void SetUnknownMaterials(InventoryGui __instance, AlmanacPlugin.Toggle toggle)
        {
            var materials = Almanac.CreateAlmanac.materials;
            var trophyPanel = __instance.m_trophiesPanel;
            var trophyFrame = trophyPanel.transform.Find("TrophiesFrame");
            var materialPanel = trophyFrame.transform.Find("materialPanel");
            for (int i = 0; i < materials.Count; ++i)
            {
                var container = materialPanel.Find($"materialContainer ({i})");
                var icon = container.Find("iconObj");
                Image iconImage = icon.gameObject.GetComponent<Image>();
                var hoverText = container.Find("hoverTextElement");
                // Text text = hoverText.gameObject.GetComponent<Text>();
                TextMeshProUGUI text = hoverText.gameObject.GetComponent<TextMeshProUGUI>();

                Button button = container.GetComponent<Button>();
                if (!Player.m_localPlayer.IsMaterialKnown(materials[i].m_itemData.m_shared.m_name) &&
                    toggle == AlmanacPlugin.Toggle.On)
                {
                    iconImage.color = new Color(0f, 0f, 0f, 1f);
                    text.text = "???";
                    button.interactable = false;
                }
                else
                {
                    iconImage.color = new Color(1f, 1f, 1f, 1f);
                    text.text = Localization.instance.Localize(materials[i].m_itemData.m_shared.m_name);
                    button.interactable = true;
                }
            }
        }

        private static void SetUnknownItems(InventoryGui __instance, AlmanacPlugin.Toggle toggle, string id,
            List<ItemDrop> list)
        {
            var trophyPanel = __instance.m_trophiesPanel;
            var trophyFrame = trophyPanel.transform.Find("TrophiesFrame");
            var materialPanel = trophyFrame.transform.Find($"{id}Panel");
            for (int i = 0; i < list.Count; ++i)
            {
                try
                {
                    var container = materialPanel.Find($"{id}Container ({i})");
                    var icon = container.Find("iconObj");
                    Image iconImage = icon.gameObject.GetComponent<Image>();
                    var hoverText = container.Find("hoverTextElement");
                    TextMeshProUGUI text = hoverText.gameObject.GetComponent<TextMeshProUGUI>();
                    Button button = container.GetComponent<Button>();
                    if (!Player.m_localPlayer.IsMaterialKnown(list[i].m_itemData.m_shared.m_name) &&
                        toggle == AlmanacPlugin.Toggle.On)
                    {
                        iconImage.color = new Color(0f, 0f, 0f, 1f);
                        text.text = "???";
                        button.interactable = false;
                    }
                    else
                    {
                        iconImage.color = new Color(1f, 1f, 1f, 1f);
                        text.text = Localization.instance.Localize(list[i].m_itemData.m_shared.m_name);
                        button.interactable = true;
                    }
                }
                catch (NullReferenceException)
                {
                    // AlmanacPlugin.AlmanacLogger.Log(LogLevel.Warning, $"null reference: {id}");
                }
            }
        }

        private static void AddButtonComponent(GameObject trophyPanelIconPrefab, Transform parentElement,
            Transform contentPanel, ButtonSfx buttonSfx)
        {
            var trophyName = trophyPanelIconPrefab.transform.Find("name").GetComponent<TextMeshProUGUI>().text;

            var localizedName = Localization.instance.Localize(trophyName);
            var TrophyIcon = trophyPanelIconPrefab.transform.Find("icon_bkg").transform.Find("icon")
                .GetComponent<Image>();

            var sfx = trophyPanelIconPrefab.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = buttonSfx.m_sfxPrefab;

            var button = trophyPanelIconPrefab.AddComponent<Button>();
            button.interactable = true;
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = TrophyIcon;
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
            button.onClick.AddListener(() =>
            {
                var dummyElement = parentElement.transform.Find("AlmanacElement (0)");
                if (!dummyElement) return;
                setAlmanacData(dummyElement.gameObject, localizedName, TrophyIcon);
                SetActiveElement(contentPanel.gameObject, "WelcomePanel", "0", false);
                SetActiveElement(parentElement.gameObject, "AlmanacElement", "0", true);
                SetActiveElement(parentElement.gameObject, "materialElement", "0", false);
                SetActiveElement(parentElement.gameObject, "consummableElement", "0", false);
                SetActiveElement(parentElement.gameObject, "gearElement", "0", false);
                SetActiveElement(parentElement.gameObject, "weaponElement", "0", false);
                SetActiveElement(parentElement.gameObject, "ammoElement", "0", false);
                SetActiveElement(parentElement.gameObject, "fishElement", "0", false);
            });
        }

        private static void SetActiveElement(GameObject parentElement, string type, string id, bool active)
        {
            var element = parentElement.transform.Find($"{type} ({id})");
            element.gameObject.SetActive(active);
        }

        public static void SetItemsData(GameObject Element, ItemDrop data)
        {
            var sharedData = data.m_itemData.m_shared;
            string name = Localization.instance.Localize(sharedData.m_name);
            string description = Localization.instance.Localize(sharedData.m_description);
            
            float maxStackSize = sharedData.m_maxStackSize;
            float weight = sharedData.m_weight;
            int value = sharedData.m_value;
            int maxQuality = sharedData.m_maxQuality;
            float maxDurability = sharedData.m_maxDurability;
            float foodHealth = sharedData.m_food;
            float foodStamina = sharedData.m_foodStamina;
            float foodEitr = sharedData.m_foodEitr;
            float foodBurn = sharedData.m_foodBurnTime;
            float foodRegen = sharedData.m_foodRegen;
            float movementMod = sharedData.m_movementModifier * 100f;
            float eitrRegenMod = sharedData.m_eitrRegenModifier * 100f;
            float staminaMod = sharedData.m_baseItemsStaminaModifier * 100f;
            

            bool teleportable = sharedData.m_teleportable;

            GameObject item = data.gameObject;
            Fish fishScript;
            item.TryGetComponent(out fishScript);
            Floating floatingScript;
            item.TryGetComponent(out floatingScript);
            
            Recipe recipe = ObjectDB.instance.GetRecipe(data.m_itemData);
            
            // Set all data to default
            SetActiveElement(Element, "TextElement", "recipeTitle", true);
            SetActiveElement(Element, "TextElement", "recipeNull", false);
            
            for (int i = 0; i < 5; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"recipe ({i})", false);
            }
            
            SetActiveElement(Element, "ImageElement", "craftingStation", false);
            
            try
            {
                // If data has a crafting station and resources
                var resources = recipe.m_resources;
                CraftingStation craftingStation = recipe.m_craftingStation;
                Sprite stationIcon = craftingStation.m_icon;

                SetImageElement(Element, "craftingStation", stationIcon, Color.white);
                SetHoverableText(Element, "craftingStation", $"{craftingStation.m_name}");
                SetActiveElement(Element, "ImageElement", "craftingStation", true);

                for (int i = 0; i < 5; ++i)
                {
                    try
                    {
                        string resourceName = resources[i].m_resItem.m_itemData.m_shared.m_name;
                        var resourceAmount = resources[i].m_amount;
                        Sprite resourceIcon = resources[i].m_resItem.m_itemData.m_shared.m_icons[0];
                        GameObject ResourceBackground =
                            Element.transform.Find($"ImageElement (recipe ({i}))").gameObject;

                        SetActiveElement(Element, "ImageElement", $"recipe ({i})", true);
                        SetHoverableText(Element, $"recipe ({i})", Localization.instance.Localize(resourceName));
                        SetImageElement(ResourceBackground, "item", resourceIcon, Color.white);
                        SetTextElement(ResourceBackground, $"recipeAmount", $"{resourceAmount}");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        SetActiveElement(Element, "ImageElement", $"recipe ({i})", false);
                    }
                }
            }
            catch (NullReferenceException)
            {
                // if crafting station is a cooking station
                foreach (var station in Almanac.CreateAlmanac.cookingStations)
                {
                    Piece piece = station.GetComponent<Piece>();
                    CookingStation script = station.GetComponent<CookingStation>();
                    foreach (CookingStation.ItemConversion conversion in script.m_conversion)
                    {
                        if (conversion.m_to == data)
                        {
                            SetImageElement(Element, "craftingStation", piece.m_icon, Color.white);
                            SetHoverableText(Element, "craftingStation", $"{piece.m_name}");
                            SetActiveElement(Element, "ImageElement", "craftingStation", true);
                            GameObject ResourceBackground =
                                Element.transform.Find($"ImageElement (recipe (0))").gameObject;
                            SetImageElement(ResourceBackground, "item", conversion.m_from.m_itemData.m_shared.m_icons[0], Color.white);
                            SetHoverableText(Element, $"recipe (0)", Localization.instance.Localize(conversion.m_from.m_itemData.m_shared.m_name));
                            SetActiveElement(Element, "ImageElement", $"recipe (0)", true);
                            SetTextElement(ResourceBackground, $"recipeAmount", "1");
                            break;
                        }
                    }
                }

                var targetElement = Element.transform.Find("ImageElement (craftingStation)");
                
                if (!targetElement.gameObject.activeInHierarchy)
                {
                    // if crafting station is a fermenter
                    foreach (var station in Almanac.CreateAlmanac.fermentingStations)
                    {
                        Piece piece = station.GetComponent<Piece>();
                        Fermenter script = station.GetComponent<Fermenter>();
                        foreach (Fermenter.ItemConversion conversion in script.m_conversion)
                        {
                            if (conversion.m_to == data)
                            {
                                SetImageElement(Element, "craftingStation", piece.m_icon, Color.white);
                                SetHoverableText(Element, "craftingStation", $"{piece.m_name}");
                                SetActiveElement(Element, "ImageElement", "craftingStation", true);
                                GameObject ResourceBackground =
                                    Element.transform.Find($"ImageElement (recipe (0))").gameObject;
                                SetImageElement(ResourceBackground, "item",
                                    conversion.m_from.m_itemData.m_shared.m_icons[0], Color.white);
                                SetHoverableText(Element, $"recipe (0)",
                                    Localization.instance.Localize(conversion.m_from.m_itemData.m_shared.m_name));
                                SetActiveElement(Element, "ImageElement", $"recipe (0)", true);
                                SetTextElement(ResourceBackground, $"recipeAmount", "1");
                                break;
                            }
                        }
                    }
                }
                
                if (!targetElement.gameObject.activeInHierarchy)
                {
                    // if crafting station is a smelter
                    foreach (var station in Almanac.CreateAlmanac.smelterStations)
                    {
                        Piece piece = station.GetComponent<Piece>();
                        Smelter script = station.GetComponent<Smelter>();
                        foreach (Smelter.ItemConversion conversion in script.m_conversion)
                        {
                            if (conversion.m_to == data)
                            {
                                SetImageElement(Element, "craftingStation", piece.m_icon, Color.white);
                                SetHoverableText(Element, "craftingStation", $"{piece.m_name}");
                                SetActiveElement(Element, "ImageElement", "craftingStation", true);
                                GameObject ore = Element.transform.Find($"ImageElement (recipe (0))").gameObject;
                                SetImageElement(ore, "item",
                                    conversion.m_from.m_itemData.m_shared.m_icons[0], Color.white);
                                SetHoverableText(Element, $"recipe (0)",
                                    Localization.instance.Localize(conversion.m_from.m_itemData.m_shared.m_name));
                                SetActiveElement(Element, "ImageElement", $"recipe (0)", true);
                                SetTextElement(ore, $"recipeAmount", "1");

                                if (script.m_fuelItem != null)
                                {
                                    GameObject fuel = Element.transform.Find($"ImageElement (recipe (1))").gameObject;
                                    SetImageElement(fuel, "item",
                                        script.m_fuelItem.m_itemData.m_shared.m_icons[0], Color.white);
                                    SetHoverableText(Element, $"recipe (1)",
                                        Localization.instance.Localize(script.m_fuelItem.m_itemData.m_shared.m_name));
                                    SetActiveElement(Element, "ImageElement", $"recipe (1)", true);
                                    SetTextElement(fuel, $"recipeAmount", $"{script.m_fuelPerProduct}");
                                }
                                break;
                            }
                        }
                    }
                }
                
                if (!targetElement.gameObject.activeInHierarchy)
                {
                    try
                    {
                        // if craft is made on player
                        var resources = recipe.m_resources;
                        for (int i = 0; i < 5; ++i)
                        {
                            try
                            {
                                string resourceName = resources[i].m_resItem.m_itemData.m_shared.m_name;
                                var resourceAmount = resources[i].m_amount;
                                Sprite resourceIcon = resources[i].m_resItem.m_itemData.m_shared.m_icons[0];
                                GameObject ResourceBackground =
                                    Element.transform.Find($"ImageElement (recipe ({i}))").gameObject;

                                SetActiveElement(Element, "ImageElement", $"recipe ({i})", true);
                                SetHoverableText(Element, $"recipe ({i})",
                                    Localization.instance.Localize(resourceName));
                                SetImageElement(ResourceBackground, "item", resourceIcon, Color.white);
                                SetTextElement(ResourceBackground, $"recipeAmount", $"{resourceAmount}");
                            }
                            catch (IndexOutOfRangeException)
                            {
                                SetActiveElement(Element, "ImageElement", $"recipe ({i})", false);
                            }
                        }
                    }
                    catch (NullReferenceException)
                    {
                        SetActiveElement(Element, "TextElement", "recipeTitle", false);
                        SetActiveElement(Element, "TextElement", "recipeNull", true);
                    }
                }
            }

            Sprite sprite = sharedData.m_icons[0];
            
            Button prefabImageButton = Element.transform.Find("ImageElement (prefabImage)").GetComponent<Button>();
            prefabImageButton.onClick.AddListener(() =>
            {
                TextEditor textEditor = new TextEditor
                {
                    text = data.name
                };
                textEditor.SelectAll();
                textEditor.Copy();
                
                MessageHud.instance.ShowMessage(
                    MessageHud.MessageType.Center, 
                    Localization.instance.Localize("$almanac_copy_to_clipboard"));
            });

            Dictionary<string, string> staticTextData = new Dictionary<string, string>
            {
                { "Name", name },
                { "Description", description },
                { "weight", $"{weight}" },
                { "maxStackSize", $"1/{maxStackSize}" },
                { "value", $"{value}" },
                { "quality", $"{maxQuality}" },
                { "durability", $"{maxDurability}" },
                { "healthBonus", $"{foodHealth}" },
                { "staminaBonus", $"{foodStamina}" },
                { "eitrBonus", $"{foodEitr}" },
                { "foodBurn", $"{foodBurn}" },
                { "foodRegen", $"{foodRegen}" },
                { "movement", $"{movementMod}%" },
                { "eitrRegen", $"{eitrRegenMod}%" },
                { "stamMod", $"{staminaMod}%" }
            };
            foreach (KeyValuePair<string, string> textData in staticTextData)
            {
                SetTextElement(Element, textData.Key, textData.Value);
            }

            var prefabImage = Element.transform.Find("ImageElement (prefabImage)").gameObject;
            SetTextElement(prefabImage, "prefabName", $"{data.name}");
            SetImageElement(Element, "icon", sprite, Color.white);
            SetActiveElement(Element, "TextElement", "teleportable", !teleportable);
            
            SetTextElement(Element, "consumeEffectDescription", "$almanac_no_data");
            if (sharedData.m_consumeStatusEffect)
            {
                StatusEffect consumeEffect = sharedData.m_consumeStatusEffect;
                string consumeEffectDescription = Localization.instance.Localize(consumeEffect.GetTooltipString());
                SetTextElement(Element, "consumeEffectDescription", consumeEffectDescription);

                HitData.DamageModifiers modifiers = new HitData.DamageModifiers();
                consumeEffect.ModifyDamageMods(ref modifiers);

                Dictionary<string, Color> consumeEffectDict = new Dictionary<string, Color>()
                {
                    { "consumeEffectBlunt", GetModifierColor(modifiers.m_blunt) },
                    { "consumeEffectSlash", GetModifierColor(modifiers.m_slash) },
                    { "consumeEffectPierce", GetModifierColor(modifiers.m_pierce) },
                    { "consumeEffectChop", GetModifierColor(modifiers.m_chop) },
                    { "consumeEffectPickaxe", GetModifierColor(modifiers.m_pickaxe) },
                    { "consumeEffectFire", GetModifierColor(modifiers.m_fire) },
                    { "consumeEffectIce", GetModifierColor(modifiers.m_frost) },
                    { "consumeEffectLightning", GetModifierColor(modifiers.m_lightning) },
                    { "consumeEffectPoison", GetModifierColor(modifiers.m_poison) },
                    { "consumeEffectSpirit", GetModifierColor(modifiers.m_spirit) }
                };
                List<KeyValuePair<string, Color>> consumeEffectList = consumeEffectDict.ToList();
                foreach (var consumeEffectData in consumeEffectList)
                {
                    ColorizeImageElement(Element, consumeEffectData.Key, consumeEffectData.Value);
                    string tag = Localization.instance.Localize(GetTagFromID(consumeEffectData.Key));
                    SetHoverableText(Element, $"{consumeEffectData.Key}", $"{tag}");
                }
            }
            
            SetTextElement(Element, "setName", "$almanac_no_data");
            SetTextElement(Element, "setDescription", "$almanac_no_data");
            if (sharedData.m_setStatusEffect)
            {
                StatusEffect setEffect = sharedData.m_setStatusEffect;
                int setAmount = sharedData.m_setSize;
                string setName = Localization.instance.Localize(setEffect.m_name);
                string setDescription = Localization.instance.Localize(setEffect.m_tooltip);
                
                SetTextElement(Element, "setName", $"<color=yellow>{setName}</color> (<color=orange>{setAmount}</color> parts)");
                SetTextElement(Element, "setDescription", $"{setDescription}");
                
                HitData.DamageModifiers modifiers = new HitData.DamageModifiers();
                setEffect.ModifyDamageMods(ref modifiers);
                
                Dictionary<string, Color> setEffectDict = new Dictionary<string, Color>()
                {
                    { "setEffectBlunt", GetModifierColor(modifiers.m_blunt) },
                    { "setEffectSlash", GetModifierColor(modifiers.m_slash) },
                    { "setEffectPierce", GetModifierColor(modifiers.m_pierce) },
                    { "setEffectChop", GetModifierColor(modifiers.m_chop) },
                    { "setEffectPickaxe", GetModifierColor(modifiers.m_pickaxe) },
                    { "setEffectFire", GetModifierColor(modifiers.m_fire) },
                    { "setEffectIce", GetModifierColor(modifiers.m_frost) },
                    { "setEffectLightning", GetModifierColor(modifiers.m_lightning) },
                    { "setEffectPoison", GetModifierColor(modifiers.m_poison) },
                    { "setEffectSpirit", GetModifierColor(modifiers.m_spirit) }
                };
                List<KeyValuePair<string, Color>> setEffectList = setEffectDict.ToList();
                foreach (var setEffectData in setEffectList)
                {
                    ColorizeImageElement(Element, setEffectData.Key, setEffectData.Value);
                    string tag = Localization.instance.Localize(GetTagFromID(setEffectData.Key));
                    SetHoverableText(Element, $"{setEffectData.Key}", $"{tag}");
                }

                Skills.SkillType[] allSkills = Skills.s_allSkills;
                foreach (Skills.SkillType skill in allSkills)
                {
                    float skillLevelModifier = new float();
                    setEffect.ModifySkillLevel(skill, ref skillLevelModifier);
                    if (!(skillLevelModifier > 0)) continue;
                    SetTextElement(Element, "modifySkill", $"{skill} <color=orange>+{skillLevelModifier}</color>");
                    break;
                }
            }
            SetHoverableText(Element, "armorBg", "$almanac_armor_label");
            
        }

        private static string GetTagFromID(string id)
        {
            switch (id)
            {
                case { } when id.Contains("Blunt"):
                    return "$almanac_blunt";
                case { } when id.Contains("Slash"):
                    return "$almanac_slash";
                case { } when id.Contains("Pierce"):
                    return "$almanac_pierce";
                case { } when id.Contains("Chop"):
                    return "$almanac_chop";
                case { } when id.Contains("Pickaxe"):
                    return "$almanac_pickaxe";
                case { } when id.Contains("Fire"):
                    return "$almanac_fire";
                case { } when id.Contains("Ice"):
                    return "$almanac_frost";
                case { } when id.Contains("Lightning"):
                    return "$almanac_lightning";
                case { } when id.Contains("Poison"):
                    return "$almanac_poison";
                case { } when id.Contains("Spirit"):
                    return "$almanac_spirit";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Color GetModifierColor(HitData.DamageModifier modifier)
        {
            switch (modifier)
            {
                case HitData.DamageModifier.Normal:
                    return Color.white;
                case HitData.DamageModifier.Resistant:
                    return new Color(0f, 0.5f, 0f, 1f);
                case HitData.DamageModifier.Weak:
                    return new Color(1f, 0.5f, 0f, 1f);
                case HitData.DamageModifier.Ignore:
                    return AlmanacPlugin._ignoreColorConfig.Value;
                case HitData.DamageModifier.Immune:
                    return AlmanacPlugin._immuneColorConfig.Value;
                case HitData.DamageModifier.VeryResistant:
                    return new Color(0f, 1f, 0f, 1f);
                case HitData.DamageModifier.VeryWeak:
                    return new Color(1f, 0f, 0f, 1f);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static void ColorizeImageElement(GameObject parentElement, string id, Color color)
        {
            GameObject imageElement = parentElement.transform.Find($"ImageElement ({id})").gameObject;
            Image image = imageElement.GetComponent<Image>();
            image.color = color;
        }

        public static void setAlmanacData(GameObject dummyElement, string creatureName, Image trophyIcon = null!)
        {
            var player = Player.m_localPlayer;
            var creatureData = GetAllCreatureData();
            var creature = getCreature(creatureData, creatureName);
            
            SetActiveElement(dummyElement, "ImageElement", "questionMark", false);
            SetTextElement(dummyElement, "displayName", Localization.instance.Localize(creature.display_name));
            SetTextElement(dummyElement, "faction", creature.faction ?? "no data");
            SetTextElement(dummyElement, "health", $"{creature.health.ToString()} HP");
            if (trophyIcon != null)
            {
                SetImageElement(dummyElement, "icon", trophyIcon);
            }
            else
            {
                var trophy = creature.trophyName;
                foreach (var item in creature.drops)
                {
                    if (item.Contains("Trophy")) trophy = item;
                }

                var trophyObj = ObjectDB.instance.GetItemPrefab(trophy);
                if (trophyObj)
                {
                    var icon = trophyObj.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
                    SetImageElement(dummyElement, "icon", icon, new Color(1f, 1f, 1f, 1f));
                }
                else
                {
                    SetImageDefault(dummyElement, "icon");
                    SetActiveElement(dummyElement, "ImageElement", "questionMark", true);
                }
            }

            SetTextElement(dummyElement, "blunt", $"{creature.blunt}");
            SetTextElement(dummyElement, "slash", $"{creature.slash}");
            SetTextElement(dummyElement, "pierce", $"{creature.piece}");
            SetTextElement(dummyElement, "chop", $"{creature.chop}");
            SetTextElement(dummyElement, "pickaxe", $"{creature.pickaxe}");

            SetTextElement(dummyElement, "fire", $"{creature.fire}");
            SetTextElement(dummyElement, "frost", $"{creature.frost}");
            SetTextElement(dummyElement, "lightning", $"{creature.lightning}");
            SetTextElement(dummyElement, "poison", $"{creature.poison}");
            SetTextElement(dummyElement, "spirit", $"{creature.spirit}");

            for (var index = 0; index < 7; ++index)
            {
                var dropBgElement = dummyElement.transform.Find($"ImageElement (dropIconBg ({index}))");
                try
                {
                    var item = ObjectDB.instance.GetItemPrefab(creature.drops[index]);
                    if (!item)
                    {
                        SetActiveElement(dummyElement, "ImageElement", $"dropIconBg ({index})", false);
                    }
                    else
                    {
                        var icon = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
                        var itemName = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                        if (player.IsKnownMaterial(itemName))
                        {
                            SetImageElement(dropBgElement.gameObject, $"creatureDrop ({index})", icon,
                                new Color(1f, 1f, 1f, 1f));
                            SetHoverableText(dropBgElement.gameObject, $"creatureDrop ({index})", itemName);
                        }
                        else
                        {
                            SetImageElement(dropBgElement.gameObject, $"creatureDrop ({index})", icon,
                                new Color(0f, 0f, 0f, 1f));
                            SetHoverableText(dropBgElement.gameObject, $"creatureDrop ({index})", "???");
                        }

                        SetActiveElement(dummyElement, "ImageElement", $"dropIconBg ({index})", true);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    SetActiveElement(dummyElement, "ImageElement", $"dropIconBg ({index})", false);
                }
            }

            for (var index = 0; index < 4; index++)
            {
                SetAttackDefault(dummyElement, index);
                if (creature.defaultItems == new List<CreatureDataCollector.AttackData>()) continue;
                if (creature.defaultItems.Count <= index) continue;
                try
                {
                    SetAttackValues(dummyElement, index, creature.defaultItems[index]);
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.Log("failed to set almanac default item data");
                }
            }

            SetTextElement(dummyElement, "avoidFire", $"{creature.avoidFire}");
            SetTextElement(dummyElement, "afraidOfFire", $"{creature.afraidOfFire}");
            SetTextElement(dummyElement, "avoidWater", $"{creature.avoidWater}");
            SetTextElement(dummyElement, "tolerateWater", $"{creature.tolerateWater}");
            SetTextElement(dummyElement, "tolerateSmoke", $"{creature.tolerateSmoke}");
            SetTextElement(dummyElement, "tolerateTar", $"{creature.tolerateTar}");

            SetTextElement(dummyElement, "staggerWhenBlocked", $"{creature.staggerWhenBlocked}");
            SetTextElement(dummyElement, "staggerDamageFactor", $"{creature.staggerDamageFactor}");

            SetTextElement(dummyElement, "weakSpot",
                creature.weakSpot.Count != 0
                    ? $"{creature.weakSpot[0]}"
                    : Localization.instance.Localize("$almanac_no_weak_spot"));

            SetActiveElement(dummyElement, "TextElement", "consumeItems (no data)", creature.consumeItems.Count == 0);

            for (var index = 0; index < 7; ++index)
            {
                var bgElement = dummyElement.transform.Find($"ImageElement (iconBg ({index}))");
                try
                {
                    GameObject item = ObjectDB.instance.GetItemPrefab(creature.consumeItems[index]);
                    if (!item)
                    {
                        SetActiveElement(dummyElement, "ImageElement", $"iconBg ({index})", false);
                    }
                    else
                    {
                        var icon = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
                        var itemName = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                        if (player.IsKnownMaterial(item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name))
                        {
                            SetImageElement(bgElement.gameObject, $"consumeItem ({index})", icon,
                                new Color(1f, 1f, 1f, 1f));
                            SetHoverableText(bgElement.gameObject, $"consumeItem ({index})", itemName);
                        }
                        else
                        {
                            SetImageElement(bgElement.gameObject, $"consumeItem ({index})", icon,
                                new Color(0f, 0f, 0f, 1f));
                            SetHoverableText(bgElement.gameObject, $"consumeItem ({index})", "???");
                        }

                        SetActiveElement(dummyElement, "ImageElement", $"iconBg ({index})", true);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    SetActiveElement(dummyElement, "ImageElement", $"iconBg ({index})", false);
                }
            }

            dummyElement.SetActive(true);
        }

        private static void SetAttackValues(GameObject element, int index, CreatureDataCollector.AttackData attackItem)
        {
            var attackKeyValues = new Dictionary<string, string>()
            {
                { $"attackName ({index})", attackItem.name },
                { $"attackBlunt ({index})", $"{attackItem.blunt}" },
                { $"attackSlash ({index})", $"{attackItem.slash}" },
                { $"attackPierce ({index})", $"{attackItem.piece}" },
                { $"attackChop ({index})", $"{attackItem.chop}" },
                { $"attackPickaxe ({index})", $"{attackItem.pickaxe}" },
                { $"attackFire ({index})", $"{attackItem.fire}" },
                { $"attackFrost ({index})", $"{attackItem.frost}" },
                { $"attackLightning ({index})", $"{attackItem.lightning}" },
                { $"attackPoison ({index})", $"{attackItem.poison}" },
                { $"attackSpirit ({index})", $"{attackItem.spirit}" },
                { $"attackAttackForce ({index})", $"{attackItem.attackForce}" },
                { $"attackBackStabBonus ({index})", $"{attackItem.backStabBonus}" },
                { $"attackDodgeable ({index})", $"{attackItem.dodgeable}" },
                { $"attackBlockable ({index})", $"{attackItem.blockable}" },
                { $"attackStatusEffect ({index})", Localization.instance.Localize(attackItem.statusEffect) },
            };
            foreach (var data in attackKeyValues) SetTextElement(element, data.Key, data.Value);
        }

        private static void SetAttackDefault(GameObject element, int index)
        {
            var defaultDataMap = new Dictionary<string, string>()
            {
                { $"attackName ({index})", "No data" },
                { $"attackBlunt ({index})", "N/A" },
                { $"attackSlash ({index})", "N/A" },
                { $"attackPierce ({index})", "N/A" },
                { $"attackPickaxe ({index})", "N/A" },
                { $"attackFire ({index})", "N/A" },
                { $"attackFrost ({index})", "N/A" },
                { $"attackLightning ({index})", "N/A" },
                { $"attackPoison ({index})", "N/A" },
                { $"attackSpirit ({index})", "N/A" },
                { $"attackAttackForce ({index})", "N/A" },
                { $"attackBackStabBonus ({index})", "N/A" },
                { $"attackDodgeable ({index})", "N/A" },
                { $"attackBlockable ({index})", "N/A" },
                { $"attackStatusEffect ({index})", "N/A" },
            };
            foreach (var data in defaultDataMap) SetTextElement(element, data.Key, data.Value);
        }

        private static void SetHoverableText(GameObject dummyElement, string id, string content)
        {
            var element = dummyElement.transform.Find($"ImageElement ({id})");
            var hoverElement = element.transform.Find("hoverTextElement");
            if (!hoverElement) return;
            var text = hoverElement.GetComponent<TextMeshProUGUI>();

            text.text = Localization.instance.Localize(content);
        }

        private static void SetTextElement(GameObject dummyElement, string id, string content = "Unknown")
        {
            var element = dummyElement.transform.Find($"TextElement ({id})");
            if (!element) return;

            var text = element.gameObject.GetComponent<TextMeshProUGUI>();
            element.gameObject.SetActive(true);

            text.text = SetLocalizedText(content);

            var exclusionColorMap = new List<string>()
            {
                "weakSpot",
                "consumeItems (no data)",
                "displayName",
                "attackName",
                "attackStatusEffect"
            };
            if (!exclusionColorMap.Any(id.StartsWith)) text.color = SetColorCodes(content, text.color);
        }

        private static string SetLocalizedText(string originalContent)
        {
            var conversionMap = new Dictionary<string, string>()
            {
                { "Weak", "$almanac_weak" },
                { "VeryWeak", "$almanac_very_weak" },
                { "Ignore", "$almanac_ignore" },
                { "Immune", "$almanac_immune" },
                { "Resistant", "$almanac_resistant" },
                { "N/A", "$almanac_na" },
                { "Normal", "$almanac_normal" },
                { "False", "$almanac_false" },
                { "True", "$almanac_true" },
                { "Unknown", "$almanac_unknown" },
                { "No data", "$almanac_no_data" },
                { "ForestMonsters", "$almanac_forest_monsters" },
                { "Undead", "$almanac_undead" },
                { "MountainMonsters", "$almanac_mountain_monsters" },
                { "PlainsMonsters", "$almanac_plains_monsters" },
                { "MistlandsMonsters", "$almanac_mistlands_monsters" },
                { "AnimalsVeg", "$almanac_animals_veg" },
                { "SeaMonsters", "$almanac_sea_monsters" },
                { "Boss", "$almanac_boss" },
                { "Dverger", "$almanac_dverger" }
            };

            return conversionMap.ContainsKey(originalContent)
                ? Localization.instance.Localize(conversionMap[originalContent])
                : originalContent;
        }

        private static Color SetColorCodes(string context, Color defaultColor)
        {
            return context switch
            {
                "Normal" => AlmanacPlugin._normalColorConfig.Value,
                "Weak" => AlmanacPlugin._weakColorConfig.Value,
                "VeryWeak" => AlmanacPlugin._veryWeakColorConfig.Value,
                "Resistant" => AlmanacPlugin._resistantColorConfig.Value,
                "VeryResistant" => AlmanacPlugin._veryResistantColorConfig.Value,
                "Ignore" => AlmanacPlugin._ignoreColorConfig.Value,
                "Immune" => AlmanacPlugin._immuneColorConfig.Value,
                "N/A" => Color.white,
                _ => defaultColor
            };
        }

        private static void SetImageElement(GameObject dummyElement, string id, Image inputImage)
        {
            var element = dummyElement.transform.Find($"ImageElement ({id})");
            element.gameObject.SetActive(true);
            Image image = element.gameObject.GetComponent<Image>();
            image.sprite = inputImage.sprite;
            image.material = inputImage.material;
            image.fillCenter = inputImage.fillCenter;
            image.type = inputImage.type;
        }

        private static void SetImageElement(GameObject dummyElement, string id, Sprite sprite, Color color)
        {
            var element = dummyElement.transform.Find($"ImageElement ({id})");
            if (!element) return;
            element.gameObject.SetActive(true);
            Image image = element.gameObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
        }

        private static void SetImageDefault(GameObject dummyElement, string id)
        {
            var element = dummyElement.transform.Find($"ImageElement ({id})");
            if (!element) return;
            element.gameObject.SetActive(false);
        }

        private static CreatureDataCollector.CreatureData getCreature(List<CreatureDataCollector.CreatureData> data,
            string creatureName)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            var result = new CreatureDataCollector.CreatureData();
            for (var i = data.Count - 1; i >= 0; --i)
            {
                var creature = data[i];
                var displayName = Localization.instance.Localize(creature.display_name);
                if (displayName == "The Queen") displayName = "Queen";
                if (displayName == "Dvergr rogue") displayName = "Dvergr";

                var trophyName = Localization.instance.Localize(creature.trophyName);

                if (textInfo.ToTitleCase(displayName) == textInfo.ToTitleCase(creatureName)) return creature;

                if (trophyName == creatureName)
                {
                    if (creatureName.ToLower().Contains(displayName.ToLower())) return creature;
                    if (trophyName.ToLower().Contains(displayName.ToLower())) return creature;
                    if (SubstringExistsInAnyOrder(trophyName, creatureName)) return creature;
                }

                if (trophyName.Contains(creatureName)) return creature;
            }

            return result;
        }

        private static bool SubstringExistsInAnyOrder(string mainString, string subString)
        {
            mainString = mainString.ToLower();
            subString = subString.ToLower();

            var mainFreq = mainString.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            var subFreq = subString.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in subFreq)
            {
                if (!mainFreq.ContainsKey(kvp.Key) || mainFreq[kvp.Key] < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static List<CreatureDataCollector.CreatureData> GetAllCreatureData()
        {
            // string yamlFilePath = CreatureDataCollector.outputFilePath;
            // List<CreatureDataCollector.CreatureData> CreatureData;
            // if (File.Exists(yamlFilePath))
            // {
            //     string yamlContents = File.ReadAllText(yamlFilePath);
            //     Deserializer deserializer = new Deserializer();
            //     CreatureData = deserializer.Deserialize<List<CreatureDataCollector.CreatureData>>(yamlContents);
            // }
            // else
            // {
            //     AlmanacPlugin.AlmanacLogger.Log(LogLevel.Message,
            //         "Almanac Creature Data YAML file missing, generating...");
            //     CreatureDataCollector.CollectAndSaveCreatureData();
            //     string yamlContents = File.ReadAllText(yamlFilePath);
            //     Deserializer deserializer = new Deserializer();
            //     CreatureData = deserializer.Deserialize<List<CreatureDataCollector.CreatureData>>(yamlContents);
            // }
            //
            // return CreatureData;
            return Almanac.CreateAlmanac.creatures;
        }
    }
}