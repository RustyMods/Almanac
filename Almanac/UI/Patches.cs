using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

namespace Almanac.UI;

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
                var panelDisplayName = trophyName.gameObject.GetComponent<Text>().text;

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
                var trophyName = trophy.transform.Find("name").gameObject.GetComponent<Text>().text;

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
            var contentPanel = trophyFrame.Find("ContentPanel");
            
            var AlmanacList = contentPanel.Find("AlmanacList");
            var closeButton = trophyFrame.Find("Closebutton");

            ButtonSfx buttonSfx = closeButton.gameObject.GetComponent<ButtonSfx>();
            
            if (trophyList == null || !trophyFrame || !contentPanel || !AlmanacList) return;

            foreach (var trophy in trophyList) AddButtonComponent(trophy, AlmanacList, contentPanel, buttonSfx);
        }

        private static void AddButtonComponent(GameObject trophyPanelIconPrefab, Transform parentElement, Transform contentPanel, ButtonSfx buttonSfx)
        {
            var trophyName = trophyPanelIconPrefab.transform.Find("name").GetComponent<Text>().text;
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

            });
        }
        private static void setAlmanacData(GameObject dummyElement, string creatureName, Image trophyIcon)
        {
            var player = Player.m_localPlayer;
            var creatureData = GetAllCreatureData();
            var creature = getCreature(creatureData, creatureName);
            
            SetTextElement(dummyElement, "displayName", Localization.instance.Localize(creature.display_name));
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
                creature.weakSpot.Count != 0 ? $"{creature.weakSpot[0]}" : Localization.instance.Localize("$almanac_no_weak_spot"));

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
            var attackKeyValues = new Dictionary<string, string>()
            {
                { $"attackName ({index})", attackItem.name },
                { $"attackBlunt ({index})", attackItem.blunt.ToString() },
                { $"attackSlash ({index})", attackItem.slash.ToString() },
                { $"attackPierce ({index})", attackItem.piece.ToString() },
                { $"attackChop ({index})", attackItem.chop.ToString() },
                { $"attackPickaxe ({index})", attackItem.pickaxe.ToString() },
                { $"attackFire ({index})", attackItem.fire.ToString() },
                { $"attackFrost ({index})", attackItem.frost.ToString() },
                { $"attackLightning ({index})", attackItem.lightning.ToString() },
                { $"attackPoison ({index})", attackItem.poison.ToString() },
                { $"attackSpirit ({index})", attackItem.spirit.ToString() },
                { $"attackAttackForce ({index})", attackItem.attackForce.ToString() },
                { $"attackBackStabBonus ({index})", attackItem.backStabBonus.ToString() },
                { $"attackDodgeable ({index})", attackItem.dodgeable.ToString() },
                { $"attackBlockable ({index})", attackItem.blockable.ToString() },
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

        private static void SetActiveElement(GameObject parentElement, string type, string id, bool active)
        {
            var element = parentElement.transform.Find($"{type} ({id})");
            element.gameObject.SetActive(active);
        }

        private static void SetHoverableText(GameObject dummyElement, string id, string content)
        {
            var element = dummyElement.transform.Find($"ImageElement ({id})");
            var hoverElement = element.transform.Find("hoverTextElement");
            if (!hoverElement) return;
            var text = hoverElement.GetComponent<Text>();
            text.text = Localization.instance.Localize(content);
        }

        private static void SetTextElement(GameObject dummyElement, string id, string content = "Unknown")
        {
            var element = dummyElement.transform.Find($"TextElement ({id})");
            if (!element) return;
            
            var text = element.gameObject.GetComponent<Text>();
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
            if (!exclusionColorMap.Any(id.StartsWith)) text.color = SetColorCodes(content);
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
            
            return conversionMap.ContainsKey(originalContent) ? Localization.instance.Localize(conversionMap[originalContent]) : originalContent;
        }

        private static Color SetColorCodes(string context)
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
                _ => Color.white
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