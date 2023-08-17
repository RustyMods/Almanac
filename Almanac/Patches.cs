using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

namespace Almanac;

public static class Patches
{
    [HarmonyPatch(typeof(InventoryGui))]
    public static class InventoryPatches
    {
        [HarmonyPatch(nameof(InventoryGui.UpdateTrophyList)), HarmonyPostfix]
        public static void FixTrophiesList(InventoryGui __instance)
        {
            if (!__instance) return;
            var trophyList = __instance.m_trophyList;

            foreach (var trophy in trophyList)
            {
                var trophyName = trophy.transform.Find("name");
                var trophyDesc = trophy.transform.Find("description");
                var trophyPos = trophy.transform.position;

                var panelDisplayName = trophyName.gameObject.GetComponent<Text>().text;
                var localizedTrophyName = TryLocalizeString("$item_trophy_troll");
                if (panelDisplayName.ToLower().Contains("troll") || panelDisplayName == localizedTrophyName)
                {
                    if (Math.Abs(trophyPos.x - 1010f) < 5f && Math.Abs(trophyPos.y - 694f) < 5f)
                    {
                        trophyName.gameObject.SetActive(false);
                        trophyDesc.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(nameof(InventoryGui.OnCloseTrophies)), HarmonyPostfix]
        public static void closeAlmanac(InventoryGui __instance)
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
        
        [HarmonyPatch(nameof(InventoryGui.OnOpenTrophies)), HarmonyPostfix]
        public static void AddButtons(InventoryGui __instance)
        {
            if (!__instance) return;
            
            var trophyList = __instance.m_trophyList;
            var trophyFrame = __instance.m_trophiesPanel.transform.Find("TrophiesFrame");
            var contentPanel = trophyFrame.transform.Find("ContentPanel");
            var AlmanacList = contentPanel.transform.Find("AlmanacList");
            var closeButton = trophyFrame.Find("Closebutton");
            ButtonSfx buttonSfx = closeButton.gameObject.GetComponent<ButtonSfx>();
            
            if (trophyList == null || !trophyFrame || !contentPanel || !AlmanacList) return;

            foreach (var trophy in trophyList)
            {
                CreateAndAddButton(trophy, AlmanacList, contentPanel, buttonSfx);
            }
        }

        private static void CreateAndAddButton(GameObject trophyPanelIconPrefab, Transform parentElement, Transform contentPanel, ButtonSfx buttonSfx)
        {
            var trophyName = trophyPanelIconPrefab.transform.Find("name").GetComponent<Text>().text;

            var localizedName = trophyName;
            if (trophyName.StartsWith("$")) localizedName = TryLocalizeString(trophyName);

            var TrophyIcon = trophyPanelIconPrefab.transform.Find("icon_bkg").transform.Find("icon")
                .GetComponent<Image>();
            
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
                if (dummyElement) setAlmanacData(dummyElement.gameObject, localizedName, TrophyIcon);

                SetActiveElement(contentPanel.gameObject, "WelcomePanel", "0", false);
                SetActiveElement(parentElement.gameObject, "AlmanacElement", "0", true);
            });

            var sfx = trophyPanelIconPrefab.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = buttonSfx.m_sfxPrefab;
        }
        private static string TryLocalizeString(string input)
        {
            try
            {
                return Localization.instance.Localize(input);
            }
            catch (NullReferenceException e)
            {
                return input;
            }
        }
        private static void setAlmanacData(GameObject dummyElement, string creatureName, Image trophyIcon)
        {
            Player player = Player.m_localPlayer;
            var creatureData = GetAllCreatureData();
            var creature = getCreature(creatureData, creatureName);
            var displayName = creature.display_name;
            if (displayName.StartsWith("$")) displayName = TryLocalizeString(displayName);
            SetTextElement(dummyElement, "displayName", displayName);
            SetTextElement(dummyElement, "faction", creature.faction ?? "no data");
            SetTextElement(dummyElement, "health", $"{creature.health.ToString()} HP");
            SetImageElement(dummyElement, "icon", trophyIcon);

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

            for (var index = 0; index < 7; index++)
            {
                var dropBgElement = dummyElement.transform.Find($"ImageElement (dropIconBg ({index}))");
                try
                {
                    GameObject item = ObjectDB.instance.GetItemPrefab(creature.drops[index]);
                    if (!item)
                    {
                        SetActiveElement(dummyElement, "ImageElement", $"dropIconBg ({index})", false);
                    }
                    else
                    {
                        var icon = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
                        var itemName = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                        if (player.IsKnownMaterial(item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name))
                        {
                            SetImageElement(dropBgElement.gameObject, $"creatureDrop ({index})", icon, new Color(1f, 1f, 1f, 1f));
                            SetHoverableText(dropBgElement.gameObject, $"creatureDrop ({index})", itemName);
                        }
                        else
                        {
                            SetImageElement(dropBgElement.gameObject, $"creatureDrop ({index})", icon, new Color(0f, 0f, 0f, 1f));
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
                if (creature.defaultItems != new List<CreatureDataCollector.AttackData>())
                {
                    if (creature.defaultItems.Count > index)
                    {
                        try
                        {
                            SetAttackValues(dummyElement, index, creature.defaultItems[index]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Debug.Log("failed to set almanac default item data");
                        }
                    }
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
                creature.weakSpot.Count != 0 ? $"{creature.weakSpot[0]}" : "No weak spots");

            SetActiveElement(dummyElement, "TextElement", "consumeItems (no data)", creature.consumeItems.Count == 0);
            
            for (var index = 0; index < 7; index++)
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
                            SetImageElement(bgElement.gameObject, $"consumeItem ({index})", icon, new Color(1f, 1f, 1f, 1f));
                            SetHoverableText(bgElement.gameObject, $"consumeItem ({index})", itemName);
                        }
                        else
                        {
                            SetImageElement(bgElement.gameObject, $"consumeItem ({index})", icon, new Color(0f, 0f, 0f, 1f));
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
            SetTextElement(element, $"attackName ({index})", attackItem.name);
            SetTextElement(element, $"attackBlunt ({index})", attackItem.blunt.ToString());
            SetTextElement(element, $"attackSlash ({index})", attackItem.slash.ToString());
            SetTextElement(element, $"attackPierce ({index})", attackItem.piece.ToString());
            SetTextElement(element, $"attackChop ({index})", attackItem.chop.ToString());
            SetTextElement(element, $"attackPickaxe ({index})", attackItem.pickaxe.ToString());
            SetTextElement(element, $"attackFire ({index})", attackItem.fire.ToString());
            SetTextElement(element, $"attackFrost ({index})", attackItem.frost.ToString());
            SetTextElement(element, $"attackLightning ({index})", attackItem.lightning.ToString());
            SetTextElement(element, $"attackPoison ({index})", attackItem.poison.ToString());
            SetTextElement(element, $"attackSpirit ({index})", attackItem.spirit.ToString());
            SetTextElement(element, $"attackAttackForce ({index})", attackItem.attackForce.ToString());
            SetTextElement(element, $"attackBackStabBonus ({index})", attackItem.backStabBonus.ToString());
            SetTextElement(element, $"attackDodgeable ({index})", attackItem.dodgeable.ToString());
            SetTextElement(element, $"attackBlockable ({index})", attackItem.blockable.ToString());

            var localizedStatusEffect = attackItem.statusEffect;
            if (localizedStatusEffect.StartsWith("$")) localizedStatusEffect = Localization.instance.Localize(attackItem.statusEffect);
            SetTextElement(element, $"attackStatusEffect ({index})", localizedStatusEffect);
        }

        private static void SetAttackDefault(GameObject element, int index)
        {
            SetTextElement(element, $"attackName ({index})", "No data");
            SetTextElement(element, $"attackBlunt ({index})", "N/A");
            SetTextElement(element, $"attackSlash ({index})", "N/A");
            SetTextElement(element, $"attackPierce ({index})", "N/A");
            SetTextElement(element, $"attackChop ({index})", "N/A");
            SetTextElement(element, $"attackPickaxe ({index})", "N/A");
            SetTextElement(element, $"attackFire ({index})", "N/A");
            SetTextElement(element, $"attackFrost ({index})", "N/A");
            SetTextElement(element, $"attackLightning ({index})", "N/A");
            SetTextElement(element, $"attackPoison ({index})", "N/A");
            SetTextElement(element, $"attackSpirit ({index})", "N/A");
            SetTextElement(element, $"attackAttackForce ({index})", "N/A");
            SetTextElement(element, $"attackBackStabBonus ({index})", "N/A");
            SetTextElement(element, $"attackDodgeable ({index})", "N/A");
            SetTextElement(element, $"attackBlockable ({index})", "N/A");
            SetTextElement(element, $"attackStatusEffect ({index})", "N/A");
        }

        private static void SetActiveElement(GameObject parentElement, string type, string id, bool active)
        {
            var element = parentElement.transform.Find($"{type} ({id})");
            element.gameObject.SetActive(active);
        }

        private static void SetHoverableText(GameObject dummyElement, string id, string content)
        {
            var element = dummyElement.transform.Find($"ImageElement ({id})");
            var hoverElement = element.transform.Find("hoverTextElement");
            if (hoverElement)
            {
                var text = hoverElement.GetComponent<Text>();
                var localizedText = content;
                if (localizedText.StartsWith("$")) localizedText = Localization.instance.Localize(content);
                text.text = localizedText;
            }
        }

        private static void SetTextElement(GameObject dummyElement, string id, string content = "Unknown")
        {
            Color normalColorConfig = AlmanacPlugin._normalColorConfig.Value;
            Color weakColorConfig = AlmanacPlugin._weakColorConfig.Value;
            Color veryWeakColorConfig = AlmanacPlugin._veryWeakColorConfig.Value;
            Color resistantColorConfig = AlmanacPlugin._resistantColorConfig.Value;
            Color veryResistantColorConfig = AlmanacPlugin._veryResistantColorConfig.Value;
            Color immuneColorConfig = AlmanacPlugin._immuneColorConfig.Value;
            Color ignoreColorConfig = AlmanacPlugin._ignoreColorConfig.Value;
            
            var element = dummyElement.transform.Find($"TextElement ({id})");
            if (!element) return;
            var text = element.gameObject.GetComponent<Text>();
            element.gameObject.SetActive(true);

            text.text = content;

            if (content == "Weak") {text.color = weakColorConfig;}

            if (content == "VeryWeak")
            {
                text.color = veryWeakColorConfig;
                text.text = "Very Weak";
            }
            if (content == "Ignore") text.color = ignoreColorConfig;
            if (content == "Immune") text.color = immuneColorConfig;
            if (content == "Resistant") text.color = resistantColorConfig;
            if (content == "VeryResistant")
            {
                text.color = veryResistantColorConfig;
                text.text = "Very Resistant";
            }
            if (content == "N/A") text.color = Color.white;
            if (content == "Normal") text.color = normalColorConfig;
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

        private static CreatureDataCollector.CreatureData getCreature(List<CreatureDataCollector.CreatureData> data,
            string creatureName)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            var result = new CreatureDataCollector.CreatureData();
            foreach (CreatureDataCollector.CreatureData? creature in data)
            {
                var displayName = TryLocalizeString(creature.display_name);
                if (displayName == "The Queen") displayName = "Queen";
                if (displayName == "Dvergr rogue") displayName = "Dvergr";
                if (textInfo.ToTitleCase(displayName) == textInfo.ToTitleCase(creatureName)) result = creature;
                if (result.name is "no data" or "unknown")
                {
                    var trophyName = TryLocalizeString(creature.trophyName);
                    if (trophyName == creatureName && creature.name != "ML_GoblinLox") result = creature;
                }
            }

            return result;
        }

        private static List<CreatureDataCollector.CreatureData> GetAllCreatureData()
        {
            string yamlFilePath = CreatureDataCollector.outputFilePath;
            List<CreatureDataCollector.CreatureData> CreatureData;
            if (File.Exists(yamlFilePath))
            {
                string yamlContents = File.ReadAllText(yamlFilePath);
                Deserializer deserializer = new Deserializer();
                CreatureData = deserializer.Deserialize<List<CreatureDataCollector.CreatureData>>(yamlContents);
            
            }
            else
            {
                AlmanacPlugin.AlmanacLogger.Log(LogLevel.Message, "Almanac Creature Data YAML file missing, generating...");
                CreatureDataCollector.CollectAndSaveCreatureData();
                string yamlContents = File.ReadAllText(yamlFilePath);
                Deserializer deserializer = new Deserializer();
                CreatureData = deserializer.Deserialize<List<CreatureDataCollector.CreatureData>>(yamlContents);
            }
            return CreatureData;
        }
    }
}