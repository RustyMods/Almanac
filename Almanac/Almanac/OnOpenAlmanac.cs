using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Almanac.MonoBehaviors;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.Almanac;

public static class Patches
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnOpenTrophies))]
    public static class OnOpenTrophiesPatch
    {
        private static AlmanacPlugin.Toggle knowledgeLockToggle;
        private static List<GameObject> trophyList = null!;
        private static Transform trophyFrame = null!;
        private static Transform contentPanel = null!;
        private static Transform AlmanacList = null!;
        private static Transform closeButton = null!;
        private static Transform creaturePanel = null!;

        private static Transform achievementsElement = null!;

        private static Transform playerStatsElement = null!;
        // private static Transform materialPanel = null!;
        private static Transform AlmanacElement = null!;
        private static ButtonSfx buttonSfx = null!;
        private static readonly List<CreatureDataCollector.CreatureData> creatures = new(Almanac.CreateAlmanac.creatures);
        private static readonly List<string> modifiersTags = new() { "blunt", "slash", "pierce", "chop", "pickaxe", "fire", "frost", "lightning", "poison", "spirit" };
        
        private static void Postfix(InventoryGui __instance)
        {
            if (!__instance) return;
            if (AlmanacPlugin.WorkingAsType == AlmanacPlugin.WorkingAs.Server) return;
            
            SetInitialData(__instance);
            
            foreach (GameObject trophy in trophyList) AddButtonComponent(trophy);
            
            SetUnknownCreatures();
            
            SetUnknownItems("material", Almanac.CreateAlmanac.materials);
            SetUnknownItems("consummable", Almanac.CreateAlmanac.consumables);
            SetUnknownItems("weapon", Almanac.CreateAlmanac.weapons);
            SetUnknownItems("gear", Almanac.CreateAlmanac.gear);
            SetUnknownItems("ammo", Almanac.CreateAlmanac.ammunition);
            SetUnknownItems("fish", Almanac.CreateAlmanac.fish);
            
            SetUnknownPieces("miscPieces", Almanac.CreateAlmanac.miscPieces);
            SetUnknownPieces("craftingPieces", Almanac.CreateAlmanac.craftingPieces);
            SetUnknownPieces("buildPieces", Almanac.CreateAlmanac.buildPieces);
            SetUnknownPieces("furniturePieces", Almanac.CreateAlmanac.furniturePieces);
            SetUnknownPieces("other", Almanac.CreateAlmanac.defaultPieces);
            SetUnknownPieces("plantPieces", Almanac.CreateAlmanac.plantPieces);
            SetUnknownPieces("modPieces", Almanac.CreateAlmanac.modPieces);
            
            TrackPlayerStats.UpdatePlayerStats();
            SetPlayerStats();
            
            AchievementsUI.UpdateAchievements();
            AchievementsUI.SetUnknownAchievements(trophyFrame.Find("achievementsPanel"));
            foreach (var achievement in AchievementsUI.registeredAchievements)
            {
                SetAchievementsData(achievementsElement, achievement);
            }
        }

        private static void SetPlayerElementData(Transform parentElement)
        {
            // Dictionary<string, int> savedKills = TrackPlayerKills.GetCurrentKilledMonsters();
            // Dictionary<string, int> tempKills = TrackPlayerKills.TempMonstersKilled;
            // int totalKills = savedKills.Values.Sum() + tempKills.Values.Sum();

            // Dictionary<string, int> savedDeaths = TrackPlayerDeaths.GetCurrentPlayerDeaths();
            // Dictionary<string, int> tempDeaths = TrackPlayerDeaths.TempPlayerDeaths;

            // int totalDeaths = savedDeaths.Values.Sum() + tempDeaths.Values.Sum(); // Almanac Recorded Deaths

            Dictionary<string, string> conversionMap = new()
            {
                { "general_title", "$almanac_general_title" },
                { "enemy_title", "$almanac_enemy_title" },
                { "player_title", "$almanac_player_title" },
                { "misc_title", "$almanac_misc_title" },
                { "info_title", "$almanac_info_title" },
                { "death_title", "$almanac_death_title" },
                { "other_title", "$almanac_other_title" },
                { "guardian_title", "$almanac_guardian_title" },
                { "count_title", "$almanac_count_title" },
                { "totalKills", TrackPlayerStats.GetPlayerStat(PlayerStatType.EnemyKills).ToString(CultureInfo.CurrentCulture) },
                { "totalDeaths",TrackPlayerStats.GetPlayerStat(PlayerStatType.Deaths).ToString(CultureInfo.CurrentCulture) },
                { "craftOrUpgrades", TrackPlayerStats.GetPlayerStat(PlayerStatType.CraftsOrUpgrades).ToString(CultureInfo.CurrentCulture) },
                { "builds", TrackPlayerStats.GetPlayerStat(PlayerStatType.Builds).ToString(CultureInfo.CurrentCulture) },
                { "jumps", TrackPlayerStats.GetPlayerStat(PlayerStatType.Jumps).ToString(CultureInfo.CurrentCulture) },
                { "cheats", TrackPlayerStats.GetPlayerStat(PlayerStatType.Cheats).ToString(CultureInfo.InvariantCulture) },
                { "enemyHits", TrackPlayerStats.GetPlayerStat(PlayerStatType.EnemyHits).ToString(CultureInfo.CurrentCulture) },
                { "enemyKills", TrackPlayerStats.GetPlayerStat(PlayerStatType.EnemyKills).ToString(CultureInfo.InvariantCulture) },
                { "enemyKillsLastHit", TrackPlayerStats.GetPlayerStat(PlayerStatType.EnemyKillsLastHits).ToString(CultureInfo.CurrentCulture) },
                { "playerHits", TrackPlayerStats.GetPlayerStat(PlayerStatType.PlayerHits).ToString(CultureInfo.CurrentCulture) },
                { "playerKills", TrackPlayerStats.GetPlayerStat(PlayerStatType.PlayerKills).ToString(CultureInfo.CurrentCulture) },
                { "hitsTakenEnemies", TrackPlayerStats.GetPlayerStat(PlayerStatType.HitsTakenEnemies).ToString(CultureInfo.CurrentCulture) },
                { "itemPickedUp", TrackPlayerStats.GetPlayerStat(PlayerStatType.ItemsPickedUp).ToString(CultureInfo.CurrentCulture) },
                { "crafts", TrackPlayerStats.GetPlayerStat(PlayerStatType.Crafts).ToString(CultureInfo.CurrentCulture) },
                { "upgrades", TrackPlayerStats.GetPlayerStat(PlayerStatType.Upgrades).ToString(CultureInfo.CurrentCulture) },
                { "portalsUsed", TrackPlayerStats.GetPlayerStat(PlayerStatType.PortalsUsed).ToString(CultureInfo.CurrentCulture) },
                { "distanceTraveled", TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceTraveled).ToString(CultureInfo.CurrentCulture) },
                { "distanceWalk", TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceWalk).ToString(CultureInfo.CurrentCulture) },
                { "distanceRun", TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceRun).ToString(CultureInfo.CurrentCulture) },
                { "distanceSail", TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceSail).ToString(CultureInfo.CurrentCulture) },
                { "distanceAir", TrackPlayerStats.GetPlayerStat(PlayerStatType.DistanceAir).ToString(CultureInfo.CurrentCulture) },
                { "timeInBase", TrackPlayerStats.GetPlayerStat(PlayerStatType.TimeInBase).ToString(CultureInfo.CurrentCulture) },
                { "timeOutOfBase", TrackPlayerStats.GetPlayerStat(PlayerStatType.TimeOutOfBase).ToString(CultureInfo.CurrentCulture) },
                { "sleep", TrackPlayerStats.GetPlayerStat(PlayerStatType.Sleep).ToString(CultureInfo.CurrentCulture) },
                { "itemStandUses", TrackPlayerStats.GetPlayerStat(PlayerStatType.ItemStandUses).ToString(CultureInfo.CurrentCulture) },
                { "armorStandUses", TrackPlayerStats.GetPlayerStat(PlayerStatType.ArmorStandUses).ToString(CultureInfo.CurrentCulture) },
                { "worldLoads", TrackPlayerStats.GetPlayerStat(PlayerStatType.WorldLoads).ToString(CultureInfo.CurrentCulture) },
                { "treeChops", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeChops).ToString(CultureInfo.CurrentCulture) },
                { "tree", TrackPlayerStats.GetPlayerStat(PlayerStatType.Tree).ToString(CultureInfo.CurrentCulture) },
                { "treeTier0", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeTier0).ToString(CultureInfo.CurrentCulture) },
                { "treeTier1", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeTier1).ToString(CultureInfo.CurrentCulture) },
                { "treeTier2", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeTier2).ToString(CultureInfo.CurrentCulture) },
                { "treeTier3", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeTier3).ToString(CultureInfo.CurrentCulture) },
                { "treeTier4", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeTier4).ToString(CultureInfo.CurrentCulture) },
                { "treeTier5", TrackPlayerStats.GetPlayerStat(PlayerStatType.TreeTier5).ToString(CultureInfo.CurrentCulture) },
                { "logChops", TrackPlayerStats.GetPlayerStat(PlayerStatType.LogChops).ToString(CultureInfo.CurrentCulture) },
                { "logs", TrackPlayerStats.GetPlayerStat(PlayerStatType.Logs).ToString(CultureInfo.CurrentCulture) },
                { "mineHits", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineHits).ToString(CultureInfo.CurrentCulture) },
                { "mines", TrackPlayerStats.GetPlayerStat(PlayerStatType.Mines).ToString(CultureInfo.CurrentCulture) },
                { "mineTier0", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineTier0).ToString(CultureInfo.CurrentCulture) },
                { "mineTier1", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineTier1).ToString(CultureInfo.CurrentCulture) },
                { "mineTier2", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineTier2).ToString(CultureInfo.CurrentCulture) },
                { "mineTier3", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineTier3).ToString(CultureInfo.CurrentCulture) },
                { "mineTier4", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineTier4).ToString(CultureInfo.CurrentCulture) },
                { "mineTier5", TrackPlayerStats.GetPlayerStat(PlayerStatType.MineTier5).ToString(CultureInfo.CurrentCulture) },
                { "ravenHits", TrackPlayerStats.GetPlayerStat(PlayerStatType.RavenHits).ToString(CultureInfo.CurrentCulture) },
                { "ravenTalk", TrackPlayerStats.GetPlayerStat(PlayerStatType.RavenTalk).ToString(CultureInfo.CurrentCulture) },
                { "ravenAppear", TrackPlayerStats.GetPlayerStat(PlayerStatType.RavenAppear).ToString(CultureInfo.CurrentCulture) },
                { "creatureTamed", TrackPlayerStats.GetPlayerStat(PlayerStatType.CreatureTamed).ToString(CultureInfo.CurrentCulture) },
                { "foodEaten", TrackPlayerStats.GetPlayerStat(PlayerStatType.FoodEaten).ToString(CultureInfo.CurrentCulture) },
                { "skeletonSummons", TrackPlayerStats.GetPlayerStat(PlayerStatType.SkeletonSummons).ToString(CultureInfo.CurrentCulture) },
                { "arrowsShot", TrackPlayerStats.GetPlayerStat(PlayerStatType.ArrowsShot).ToString(CultureInfo.CurrentCulture) },
                { "tombstonesOpenedOwn", TrackPlayerStats.GetPlayerStat(PlayerStatType.TombstonesOpenedOwn).ToString(CultureInfo.CurrentCulture) },
                { "tombstonesOpenOther", TrackPlayerStats.GetPlayerStat(PlayerStatType.TombstonesOpenedOther).ToString(CultureInfo.CurrentCulture) },
                { "tombstonesFit", TrackPlayerStats.GetPlayerStat(PlayerStatType.TombstonesFit).ToString(CultureInfo.CurrentCulture) },
                { "deathByUndefined", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByUndefined).ToString(CultureInfo.CurrentCulture) },
                { "deathByEnemyHit", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByEnemyHit).ToString(CultureInfo.CurrentCulture) },
                { "deathByPlayerHit", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByPlayerHit).ToString(CultureInfo.CurrentCulture) },
                { "deathByFall", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByFall).ToString(CultureInfo.CurrentCulture) },
                { "deathByDrowning", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByDrowning).ToString(CultureInfo.CurrentCulture) },
                { "deathByBurning", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByBurning).ToString(CultureInfo.CurrentCulture) },
                { "deathByFreezing", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByFreezing).ToString(CultureInfo.CurrentCulture) },
                { "deathByPoisoned", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByPoisoned).ToString(CultureInfo.CurrentCulture) },
                { "deathBySmoke", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathBySmoke).ToString(CultureInfo.CurrentCulture) },
                { "deathByWater", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByWater).ToString(CultureInfo.CurrentCulture) },
                { "deathByEdgeOfWorld", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld).ToString(CultureInfo.CurrentCulture) },
                { "deathByImpact", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByImpact).ToString(CultureInfo.CurrentCulture) },
                { "deathByCart", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByCart).ToString(CultureInfo.CurrentCulture) },
                { "deathByTree", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByTree).ToString(CultureInfo.CurrentCulture) },
                { "deathBySelf", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathBySelf).ToString(CultureInfo.CurrentCulture) },
                { "deathByStructural", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByStructural).ToString(CultureInfo.CurrentCulture) },
                { "deathByBoat", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByBoat).ToString(CultureInfo.CurrentCulture) },
                { "deathByTurret", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByTurret).ToString(CultureInfo.CurrentCulture) },
                { "deathByStalagtite", TrackPlayerStats.GetPlayerStat(PlayerStatType.DeathByStalagtite).ToString(CultureInfo.CurrentCulture) },
                { "doorsOpened", TrackPlayerStats.GetPlayerStat(PlayerStatType.DoorsOpened).ToString(CultureInfo.CurrentCulture) },
                { "doorsClosed", TrackPlayerStats.GetPlayerStat(PlayerStatType.DoorsClosed).ToString(CultureInfo.CurrentCulture) },
                { "beesHarvested", TrackPlayerStats.GetPlayerStat(PlayerStatType.BeesHarvested).ToString(CultureInfo.CurrentCulture) },
                { "sapHarvested", TrackPlayerStats.GetPlayerStat(PlayerStatType.SapHarvested).ToString(CultureInfo.CurrentCulture) },
                { "turretAmmoAdded", TrackPlayerStats.GetPlayerStat(PlayerStatType.TurretAmmoAdded).ToString(CultureInfo.CurrentCulture) },
                { "turretTrophySet", TrackPlayerStats.GetPlayerStat(PlayerStatType.TurretTrophySet).ToString(CultureInfo.CurrentCulture) },
                { "trapArmed", TrackPlayerStats.GetPlayerStat(PlayerStatType.TrapArmed).ToString(CultureInfo.CurrentCulture) },
                { "trapTriggered", TrackPlayerStats.GetPlayerStat(PlayerStatType.TrapTriggered).ToString(CultureInfo.CurrentCulture) },
                { "placeStacks", TrackPlayerStats.GetPlayerStat(PlayerStatType.PlaceStacks).ToString(CultureInfo.CurrentCulture) },
                { "portalDungeonIn", TrackPlayerStats.GetPlayerStat(PlayerStatType.PortalDungeonIn).ToString(CultureInfo.CurrentCulture) },
                { "portalDungeonOut", TrackPlayerStats.GetPlayerStat(PlayerStatType.PortalDungeonOut).ToString(CultureInfo.CurrentCulture) },
                { "totalBossKills", TrackPlayerStats.GetPlayerStat(PlayerStatType.BossKills).ToString(CultureInfo.CurrentCulture) },
                { "bossLastHits", TrackPlayerStats.GetPlayerStat(PlayerStatType.BossLastHits).ToString(CultureInfo.CurrentCulture) },
                { "setGuardianPower", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetGuardianPower).ToString(CultureInfo.CurrentCulture) },
                { "setPowerEikthyr", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerEikthyr).ToString(CultureInfo.CurrentCulture) },
                { "setPowerElder", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerElder).ToString(CultureInfo.CurrentCulture) },
                { "setPowerBonemass", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerBonemass).ToString(CultureInfo.CurrentCulture) },
                { "setPowerModer", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerModer).ToString(CultureInfo.CurrentCulture) },
                { "setPowerYagluth", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerYagluth).ToString(CultureInfo.CurrentCulture) },
                { "setPowerQueen", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerQueen).ToString(CultureInfo.CurrentCulture) },
                { "setPowerAshlands", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerAshlands).ToString(CultureInfo.CurrentCulture) },
                { "setPowerDeepNorth", TrackPlayerStats.GetPlayerStat(PlayerStatType.SetPowerDeepNorth).ToString(CultureInfo.CurrentCulture) },
                { "useGuardianPower", TrackPlayerStats.GetPlayerStat(PlayerStatType.UseGuardianPower).ToString(CultureInfo.CurrentCulture) },
                { "usePowerEikthyr", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerEikthyr).ToString(CultureInfo.CurrentCulture) },
                { "usePowerElder", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerElder).ToString(CultureInfo.CurrentCulture) },
                { "usePowerBonemass", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerBonemass).ToString(CultureInfo.CurrentCulture) },
                { "usePowerModer", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerModer).ToString(CultureInfo.CurrentCulture) },
                { "usePowerYagluth", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerYagluth).ToString(CultureInfo.CurrentCulture) },
                { "usePowerQueen", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerQueen).ToString(CultureInfo.CurrentCulture) },
                { "usePowerAshlands", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerAshlands).ToString(CultureInfo.CurrentCulture) },
                { "usePowerDeepNorth", TrackPlayerStats.GetPlayerStat(PlayerStatType.UsePowerDeepNorth).ToString(CultureInfo.CurrentCulture) },
                { "count", TrackPlayerStats.GetPlayerStat(PlayerStatType.Count).ToString(CultureInfo.CurrentCulture) },
            };

            SetTextElement(parentElement.gameObject, "title", Player.m_localPlayer.GetHoverName() + " " + "$almanac_stats_button");

            // Calculate the largest string length
            int largestString = 0;
            foreach (KeyValuePair<string, string> kvp in conversionMap)
            {
                // if (kvp.Key.Contains("title")) continue;
                string content = $"$almanac_{kvp.Key}_label : <color=orange>{kvp.Value}</color>";
                string localizedValue = Localization.instance.Localize(content);
                int length = localizedValue.Length;
                if (length > largestString) largestString = length;
            }

            StringBuilder sb = new StringBuilder();

            // Set text elements with padding
            foreach (KeyValuePair<string, string> kvp in conversionMap)
            {
                string content = $"$almanac_{kvp.Key}_label : <color=orange>{kvp.Value}</color>";
                string localizedValue = Localization.instance.Localize(content);

                sb.Clear();
                sb.Append(content);

                if (localizedValue.Length < largestString)
                {
                    int difference = largestString - localizedValue.Length;

                    sb.Insert(sb.ToString().IndexOf(':') + 1, new string(' ', difference));
                }

                SetTextElement(parentElement.gameObject, kvp.Key, kvp.Key.Contains("title") ? kvp.Value : sb.ToString());
            }
        }

        public static void SetAchievementsData(Transform parentElement, AchievementsUI.Achievement data)
        {
            Transform achievementIconBg = parentElement.Find("ImageElement (achievementIcon)");
            Transform achievementIcon = achievementIconBg.Find("ImageElement (icon)");

            achievementIcon.TryGetComponent(out Button button);
            float progress = data.value * 100f / data.total;
            progress = Mathf.Clamp(progress, 0f, 100f);

            if (!button) return;
            button.interactable = CheckCheats.PlayerWatcher.noCost || data.isCompleted;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SetAlmanacPowers(data);
            });

            SetTextElement(parentElement.gameObject, "achievementTooltip", CheckCheats.PlayerWatcher.noCost ? data.powerToolTip : data.isCompleted ? data.powerToolTip : "");
            SetTextElement(parentElement.gameObject, "achievementTitle", CheckCheats.PlayerWatcher.noCost ? data.name : data.isCompleted ? data.name : "???");
            SetTextElement(parentElement.gameObject, "achievementDescription", data.description);
            SetImageElement(achievementIconBg.gameObject, "icon", data.sprite, CheckCheats.PlayerWatcher.noCost ? Color.white : data.isCompleted ? Color.white : Color.black);
            SetTextElement(parentElement.gameObject, "achievementProgress", $"<color=orange>{data.value}</color> / {data.total} (<color=orange>{progress}</color>%)");
            SetTextElement(parentElement.gameObject, "achievementLore", CheckCheats.PlayerWatcher.noCost ? data.lore : data.isCompleted ? data.lore : "");

        }

        private static void SetAlmanacPowers(AchievementsUI.Achievement data)
        {
            if (data.power.IsNullOrWhiteSpace())
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    "$almanac_no_power_set", icon: AlmanacPlugin.AlmanacIconButton);
                return;
            }
            Player player = Player.m_localPlayer;
            if (data.power.StartsWith("GP"))
            {
                player.SetGuardianPower(data.power);
                player.GetGuardianPowerHUD(out StatusEffect GP, out float coolDown);
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    $"$almanac_set_guardian_power {GP.m_name} $almanac_power");
            }
            else
            {
                List<StatusEffect> activeAlmanacEffects = CustomStatusEffects.activeAlmanacEffects;
                
                int guardianPowerHashCode = string.IsNullOrEmpty(data.power) ? 0 : data.power.GetStableHashCode();
                StatusEffect customPower = ObjectDB.instance.GetStatusEffect(guardianPowerHashCode);

                if (activeAlmanacEffects.Exists(effect => effect.name == data.power))
                {
                    // Debug.LogWarning($"Removing {data.power}");
                    CustomStatusEffects.RemoveAlmanacEffect(customPower);
                    player.m_seman.RemoveStatusEffect(customPower);
                }
                else
                {
                    if (activeAlmanacEffects.Count >= AchievementsUI.maxPowers)
                    {
                        MessageHud.instance.ShowMessage(
                            MessageHud.MessageType.Center, "$almanac_max_powers");
                        return;
                    };

                    // Debug.LogWarning($"Adding {data.power}");
                    CustomStatusEffects.AddAlmanacEffect(customPower);
                    player.m_seman.AddStatusEffect(customPower);
                }
            }
        }

        public static void SetPlayerStats()
        {
            SetPlayerElementData(playerStatsElement);
            SetPlayerPanel();
        }

        private static void SetPlayerPanel()
        {
            Transform panel = trophyFrame.Find("statsPanel");
            
            SetTextElement(panel.gameObject, "almanacPowers", "$almanac_custom_powers_label");
            
            SetActivePowers();
        }

        private static void SetActivePowers()
        {
            Player player = Player.m_localPlayer;
            Transform panel = trophyFrame.Find("statsPanel");

            List<StatusEffect> activePowers = CustomStatusEffects.activeAlmanacEffects;

            for (int i = 0; i < AchievementsUI.maxPowers; ++i)
            {
                SetActiveElement(panel.gameObject, "ImageElement", $"activeEffects ({i})", false);
                SetActiveElement(panel.gameObject, "TextElement", $"activeDesc ({i})", false);
            }
            for (int i = 0; i < activePowers.Count; ++i)
            {
                StatusEffect power = activePowers[i];
                SetActiveElement(panel.gameObject, "ImageElement", $"activeEffects ({i})", true);
                SetHoverableText(panel.gameObject, $"activeEffects ({i})", activePowers[i].m_name);
                Transform iconBackground = panel.Find($"ImageElement (activeEffects ({i}))");
                SetImageElement(iconBackground.gameObject, "icon", activePowers[i].m_icon, Color.white);

                Transform achievementIcon = iconBackground.Find("ImageElement (icon)");
                achievementIcon.TryGetComponent(out Button button);

                if (!button) return; ;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    // Debug.LogWarning($"Removing {power.name}");
                    CustomStatusEffects.RemoveAlmanacEffect(power);
                    player.m_seman.RemoveStatusEffect(power);
                    SetActivePowers();
                });

                string desc = $"<color=orange>{power.m_name}</color>\n{power.m_tooltip}";
                SetTextElement(panel.gameObject, $"activeDesc ({i})", desc);
            }
        }

        private static void SetInitialData(InventoryGui __instance)
        {
            trophyList = __instance.m_trophyList;
            trophyFrame = __instance.m_trophiesPanel.transform.Find("TrophiesFrame");
            closeButton = trophyFrame.Find("Closebutton");
            contentPanel = trophyFrame.Find("ContentPanel");
            AlmanacList = contentPanel.Find("AlmanacList");
            creaturePanel = trophyFrame.transform.Find("creaturePanel");
            // materialPanel = trophyFrame.transform.Find("materialPanel");
            AlmanacElement = AlmanacList.transform.Find("AlmanacElement (0)");
            knowledgeLockToggle = AlmanacPlugin._KnowledgeLock.Value;
            achievementsElement = AlmanacList.Find("achievementsElement (0)");
            playerStatsElement = AlmanacList.Find("statsElement (0)");

            closeButton.TryGetComponent(out ButtonSfx buttonSfxScript);
            if (!buttonSfxScript) return;
            buttonSfx = buttonSfxScript;
        }

        public static void SetUnknownPieces(string id, List<GameObject> list)
        {
            Transform panel = trophyFrame.Find($"{id}Panel");
            for (int i = 0; i < list.Count; ++i)
            {
                Transform container = panel.Find($"{id}Container ({i})");
                Transform icon = container.Find("iconObj");
                Transform hoverText = container.Find("hoverTextElement");

                icon.TryGetComponent(out Image iconImage);
                hoverText.TryGetComponent(out TextMeshProUGUI textMesh);
                container.TryGetComponent(out Button button);

                if (!iconImage || !textMesh || !button) continue;

                list[i].TryGetComponent(out Piece piece);
                if (!piece) continue;

                string prefab = piece.name;
                string name = piece.m_name;
                bool isRecipeKnown = Player.m_localPlayer.IsRecipeKnown(name) || CheckCheats.PlayerWatcher.noCost;
                string localizedName = Localization.instance.Localize(name);

                button.interactable = knowledgeLockToggle != AlmanacPlugin.Toggle.On || isRecipeKnown;
                textMesh.text = knowledgeLockToggle == AlmanacPlugin.Toggle.On
                    ? isRecipeKnown ? localizedName : "???"
                    : localizedName;
                iconImage.color = knowledgeLockToggle == AlmanacPlugin.Toggle.On
                    ? isRecipeKnown ? Color.white : Color.black
                    : Color.white;

                if (BlackList.PieceBlackList.Value.Count == 0) continue;
                SetBlackListByPage(container, id, prefab, i, BlackListTypes.pieces);
            }
        }

        private enum BlackListTypes{
            items,
            creatures,
            pieces
        }
        private static void SetBlackListByPage(Transform container, string id, string prefabName, int index, BlackListTypes type)
        {
            int page = 0;
            switch (type)
            {
                case BlackListTypes.pieces:
                    page = Mathf.FloorToInt(index / 72f);
                    break;
                case BlackListTypes.creatures:
                    page = Mathf.FloorToInt(index / 100f);
                    break;
                case BlackListTypes.items:
                    page = Mathf.FloorToInt(index / 72f);
                    break;
            }
            int currentPage = Int32.MaxValue;
            switch (id)
            {
                case "miscPieces":
                    currentPage = Almanac.CreateAlmanac.miscPage;
                    break;
                case "craftingPieces":
                    currentPage = Almanac.CreateAlmanac.craftPage;
                    break;
                case "buildPieces":
                    currentPage = Almanac.CreateAlmanac.buildPage;
                    break;
                case "furniturePieces":
                    currentPage = Almanac.CreateAlmanac.furniturePage;
                    break;
                case "other":
                    currentPage = Almanac.CreateAlmanac.otherPage;
                    break;
                case "plantPieces":
                    currentPage = Almanac.CreateAlmanac.plantsPage;
                    break;
                case "modPieces":
                    currentPage = Almanac.CreateAlmanac.modPage;
                    break;
                case "Creature":
                    currentPage = Almanac.CreateAlmanac.creaturesPage;
                    break;
                case "material":
                    currentPage = Almanac.CreateAlmanac.materialsPage;
                    break;
                case "consummable":
                    currentPage = Almanac.CreateAlmanac.consumablesPage;
                    break;
                case "weapon":
                    currentPage = Almanac.CreateAlmanac.weaponsPage;
                    break;
                case "gear":
                    currentPage = Almanac.CreateAlmanac.equipmentPage;
                    break;
                case "ammo":
                    currentPage = Almanac.CreateAlmanac.projectilePage;
                    break;
                case "fish":
                    currentPage = Almanac.CreateAlmanac.fishPage;
                    break;
            }

            if (currentPage != page) return;
            switch (id)
            {
                case "miscPieces":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "craftingPieces":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "buildPieces":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "furniturePieces":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "other":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "plantPieces":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "modPieces":
                    container.gameObject.SetActive(!BlackList.PieceBlackList.Value.Contains(prefabName));
                    break;
                case "Creature":
                    container.gameObject.SetActive(!BlackList.CreatureBlackList.Value.Contains(prefabName));
                    break;
                case "material":
                    container.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefabName));
                    break;
                case "consummable":
                    container.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefabName));
                    break;
                case "weapon":
                    container.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefabName));
                    break;
                case "gear":
                    container.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefabName));
                    break;
                case "ammo":
                    container.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefabName));
                    break;
                case "fish":
                    container.gameObject.SetActive(!BlackList.ItemBlackList.Value.Contains(prefabName));
                    break;
            }
        }
        public static void SetUnknownCreatures()
        {
            List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
            
            for (int i = 0; i < creatures.Count; ++i)
            {
                Transform container = creaturePanel.Find($"CreatureContainer ({i})");
                Transform textMesh = container.Find($"CreatureContainer Text ({i})");
                
                container.TryGetComponent(out Button button);
                textMesh.TryGetComponent(out TextMeshProUGUI text);

                if (!button || !text) continue;
                
                string prefab = creatures[i].name;
                string defeatedKey = creatures[i].defeatedKey;
                bool isWise = true;
                
                if (knowledgeLockToggle == AlmanacPlugin.Toggle.On)
                {
                    isWise = globalKeys.Contains(defeatedKey);
                }

                if (CheckCheats.PlayerWatcher.noCost) isWise = true;
                
                // Set values
                button.interactable = isWise;
                text.text = isWise
                    ? Localization.instance.Localize(creatures[i].display_name)
                    : Localization.instance.Localize("$almanac_locked");
                text.color = isWise
                    ? Color.yellow
                    : Color.gray;
                
                // Check against blacklist
                if (BlackList.CreatureBlackList.Value.Count == 0) continue;
                SetBlackListByPage(container, "Creature", prefab, i, BlackListTypes.creatures);
            }
        }

        public static void SetUnknownItems(string id, List<ItemDrop> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                try
                {
                    Transform panel = trophyFrame.Find($"{id}Panel");
                    Transform container = panel.Find($"{id}Container ({i})");
                    Transform icon = container.Find("iconObj");
                    Transform hoverText = container.Find("hoverTextElement");

                    icon.TryGetComponent(out Image iconImage);
                    hoverText.TryGetComponent(out TextMeshProUGUI text);
                    container.TryGetComponent(out Button button);

                    ItemDrop? data = list[i];
                    if (!iconImage || !text || !button || !data) continue;

                    string prefab = list[i].name;
                    string name = list[i].m_itemData.m_shared.m_name;
                    string localizedName = Localization.instance.Localize(name);
                    bool isKnown = Player.m_localPlayer.IsMaterialKnown(name) || CheckCheats.PlayerWatcher.noCost;

                    iconImage.color = knowledgeLockToggle == AlmanacPlugin.Toggle.On
                        ? (isKnown ? Color.white : Color.black)
                        : Color.white;
                    text.text = knowledgeLockToggle == AlmanacPlugin.Toggle.On
                        ? (isKnown ? localizedName : "???")
                        : localizedName;
                    button.interactable = knowledgeLockToggle != AlmanacPlugin.Toggle.On || isKnown;
                    // Check against blacklist
                    if (BlackList.ItemBlackList.Value.Count == 0) continue;
                    SetBlackListByPage(container, id, prefab, i, BlackListTypes.items);
                }
                catch (NullReferenceException)
                {
                    // Debug.Log(list[i].name);
                }
            }
        }

        private static void AddButtonComponent(GameObject trophyPanelIconPrefab)
        {
            Transform nameElement = trophyPanelIconPrefab.transform.Find("name");
            Transform iconBkg = trophyPanelIconPrefab.transform.Find("icon_bkg");
            Transform iconElement = iconBkg.Find("icon");

            iconElement.TryGetComponent(out Image TrophyIcon);
            nameElement.TryGetComponent(out TextMeshProUGUI textMesh);
            if (!textMesh || !TrophyIcon) return;
            
            string trophyName = textMesh.text;
            string localizedName = Localization.instance.Localize(trophyName);

            trophyPanelIconPrefab.TryGetComponent(out Button possibleButton);
            trophyPanelIconPrefab.TryGetComponent(out ButtonSfx possibleButtonSfx);
            if (possibleButton && possibleButtonSfx) return;
            
            ButtonSfx sfx = trophyPanelIconPrefab.AddComponent<ButtonSfx>();
            sfx.m_sfxPrefab = buttonSfx.m_sfxPrefab;

            Button button = trophyPanelIconPrefab.AddComponent<Button>();
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
                setAlmanacData(AlmanacElement.gameObject, localizedName, TrophyIcon);
                SetActiveElement(AlmanacList.gameObject, "AlmanacElement", "0", true);
                
                SetActiveElement(contentPanel.gameObject, "WelcomePanel", "0", false);
                SetActiveElement(AlmanacList.gameObject, "materialElement", "0", false);
                SetActiveElement(AlmanacList.gameObject, "consummableElement", "0", false);
                SetActiveElement(AlmanacList.gameObject, "gearElement", "0", false);
                SetActiveElement(AlmanacList.gameObject, "weaponElement", "0", false);
                SetActiveElement(AlmanacList.gameObject, "ammoElement", "0", false);
                SetActiveElement(AlmanacList.gameObject, "fishElement", "0", false);
                
                SetActiveElement(AlmanacList.gameObject, "piecesElement", "0", false);
                
                SetActiveElement(AlmanacList.gameObject, "achievementsElement", "0", false);
                SetActiveElement(AlmanacList.gameObject, "statsElement", "0", false);
            });
        }

        private static void SetActiveElement(GameObject parentElement, string type, string id, bool active)
        {
            Transform element = parentElement.transform.Find($"{type} ({id})");
            element.gameObject.SetActive(active);
        }

        public static void SetPiecesData(GameObject Element, GameObject data)
        {
            // Set unknowns back to default
            SetActiveElement(Element, "ImageElement", "craftingStation", false);
            for (int i = 0; i < 5; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"recipe ({i})", false);
            }
            SetImageElement(Element, "craftingStation", AlmanacPlugin.questionMarkIcon, Color.white);
            SetHoverableText(Element, "craftingStation", "$almanac_no_data");
            SetActiveElement(Element, "ImageElement", "craftingStation", false);
            // Plants
            List<string> plantIds = new List<string>()
            {
                "growTimeLabel",
                "growTime",
                "growTimeMaxLabel",
                "growTimeMax"
            };
            foreach (string plantId in plantIds)
            {
                SetActiveElement(Element, "TextElement", plantId, false);
            }
            // WearNTear
            SetTextElement(Element, "health", "$almanac_na");
            SetTextElement(Element, "material", "$almanac_na");
            for (int index = 0; index < modifiersTags.Count; ++index)
            {
                SetTextElement(Element, $"{modifiersTags[index]}", "$almanac_no_data");
            }
            // Container
            List<string> containerIds = new List<string>()
            {
                "containerSizeLabel",
                "containerSize",
                "checkGuardLabel",
                "checkGuard",
                "autoDestroyLabel",
                "autoDestroy"
            };
            foreach (string containerId in containerIds)
            {
                SetActiveElement(Element, "TextElement", containerId, false);
            }

            for (int i = 0; i < 12; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"containerDrops ({i})", false);
            }
            // Crafting station
            List<string> craftingStationIds = new List<string>()
            {
                "discoverRangeLabel",
                "discoverRange",
                "rangeBuildLabel",
                "rangeBuild",
                "extraRangePerLevelLabel",
                "extraRange",
                "requireRoofLabel",
                "requireRoof",
                "requireFireLabel",
                "requireFire",
                "showBasicRecipesLabel",
                "basicRecipes",
                "animationIndexLabel",
                "animationIndex"
            };
            foreach (string craftingStationId in craftingStationIds)
            {
                SetActiveElement(Element, "TextElement", craftingStationId, false);
            }
            // Cooking Station
            List<string> cookingStationIds = new List<string>()
            {
                "cookingTooltip",
                "overCookedItemLabel",
                "overCooked",
                "availableSlotsLabel",
                "availableSlots",
                "cookingRequireFireLabel",
                "cookingRequireFire",
                "cookingUseFuelLabel",
                "cookingUseFuel",
                "cookingFuelItemLabel",
                "cookingFuelItem",
                "cookingMaxFuelLabel",
                "cookingMaxFuel",
                "cookingSecPerFuelLabel",
                "cookingSecPerFuel"
            };
            foreach (string cookingStationId in cookingStationIds)
            {
                SetActiveElement(Element, "TextElement", cookingStationId, false);
            }

            for (int i = 0; i < 12; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"cookingConversion ({i})", false);
            }
            // Station Extension
            SetActiveElement(Element, "ImageElement", "extensionCraftingStation", false);
            List<string> extensionStationIds = new List<string>()
            {
                "maxStationDistanceLabel",
                "stationDistance",
                "extensionStationStackLabel",
                "extensionStack"
                
            };
            foreach (string extensionStationId in extensionStationIds)
            {
                SetActiveElement(Element, "TextElement", extensionStationId, false);
            }
            // Door
            List<string> doorLabelIds = new List<string>()
            {
                "doorKeyItemLabel",
                "doorCanBeClosedLabel",
                "doorCheckGuardLabel"
            };
            List<string> doorValueIds = new List<string>()
            {
                "doorKey",
                "doorCanClose",
                "doorGuard"
            };
            List<string> allDoorIds = new List<string>();
            allDoorIds.AddRange(doorLabelIds);
            allDoorIds.AddRange(doorValueIds);
            foreach (string doorId in allDoorIds)
            {
                SetActiveElement(Element, "TextElement", doorId, false);
            }
            // Fireplace
            List<string> fireplaceLabelIds = new List<string>()
            {
                "fireplaceMaxFuelLabel",
                "fireplaceSecPerFuelLabel",
                "infiniteFuelLabel",
                "fireplaceFuelItemLabel",
            };
            List<string> fireplaceValueIds = new List<string>()
            {
                "fireplaceMaxFuel",
                "fireplaceSecPerFuel",
                "fireplaceInfiniteFuel",
                "fireplaceFuelItem"
            };
            List<string> allFireplaceIds = new List<string>();
            allFireplaceIds.AddRange(fireplaceLabelIds);
            allFireplaceIds.AddRange(fireplaceValueIds);
            foreach (string fireplaceId in allFireplaceIds)
            {
                SetActiveElement(Element, "TextElement", fireplaceId, false);
            }

            for (int i = 0; i < 11; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"fireplaceFireworks ({i})", false);
            }
            // Smelter
            List<string> smelterLabelIds = new List<string>()
            {
                "smelterFuelItemLabel",
                "smelterMaxOreLabel",
                "smelterMaxFuelLabel",
                "smelterFuelPerProductLabel",
                "smelterSecPerProductLabel",
                "smelterRequiresRoofLabel"
            };
            List<string> smelterValueIds = new List<string>()
            {
                "smelterFuelItem",
                "smelterMaxOre",
                "smelterMaxFuel",
                "smelterFuelPerProduct",
                "smelterSecPerProduct",
                "smelterRequireRoof"
            };
            List<string> allSmelterIds = new List<string>();
            allSmelterIds.AddRange(smelterLabelIds);
            allSmelterIds.AddRange(smelterValueIds);
            foreach (string smelterId in allSmelterIds)
            {
                SetActiveElement(Element, "TextElement", smelterId, false);
            }

            for (int i = 0; i < 8; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"smelterConversion from ({i})", false);
                SetActiveElement(Element, "ImageElement", $"smelterConversion to ({i})", false);
                SetActiveElement(Element, "TextElement", $"smelterConversionSymbol ({i})", false);
            }
            // Wisp Spawner
            List<string> wispLabelIds = new List<string>()
            {
                "wispSpawnIntervalLabel",
                "wispSpawnChanceLabel",
                "wispMaxSpawnedLabel",
                "wispSpawnAtNightLabel",
                "wispSpawnInCoverLabel",
                "wispSpawnDistanceLabel",
                "wispMaxSpawnAreaLabel",
            };
            List<string> wispValueIds = new List<string>()
            {
                "wispInterval",
                "wispSpawnChance",
                "wispMaxSpawn",
                "wispSpawnAtNight",
                "wispSpawnCover",
                "wispSpawnDistance",
                "wispSpawnArea"
            };
            List<string> allWispIds = new List<string>();
            allWispIds.AddRange(wispLabelIds);
            allWispIds.AddRange(wispValueIds);
            foreach (string wispId in allWispIds)
            {
                SetActiveElement(Element, "TextElement", wispId, false);
            }
            SetActiveElement(Element, "ImageElement", "wispPrefab", false);
            // Turret
            List<string> turretLabelIds = new List<string>()
            {
                "turretTurnRateLabel",
                "turretHorizontalAngleLabel",
                "turretVerticalAngleLabel",
                "turretViewDistanceLabel",
                "turretNoTargetRateLabel",
                "turretLookAccelerationLabel",
                "turretLookDecelerationLabel",
                "turretLookMinDegreesLabel",
                "turretMaxAmmoLabel", 
            };
            List<string> turretValueIds = new List<string>()
            {
                "turnRate",
                "horizontalAngle",
                "verticalAngle",
                "viewDistance",
                "noTargetRate",
                "lookAcceleration",
                "lookDeceleration",
                "lookDegrees",
                "turretMaxAmmo"
                
            };
            List<string> allTurretIds = new List<string>();
            allTurretIds.AddRange(turretLabelIds);
            allTurretIds.AddRange(turretValueIds);
            foreach (string turretId in allTurretIds)
            {
                SetActiveElement(Element, "TextElement", turretId, false);
            }

            for (int i = 0; i < 11; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"turretAmmo ({i})", false);
            }
            // Ferment
            List<string> fermentLabelIds = new List<string>()
            {
                "fermentDurationLabel",
            };
            List<string> fermentValueIds = new List<string>()
            {
                "fermentDuration"
            };
            List<string> allFermentIds = new List<string>();
            allFermentIds.AddRange(fermentLabelIds);
            allFermentIds.AddRange(fermentValueIds);
            foreach (string fermentId in allFermentIds)
            {
                SetActiveElement(Element, "TextElement", fermentId, false);
            }

            for (int i = 0; i < 12; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"ferment from ({i})", false);
                SetActiveElement(Element, "ImageElement", $"ferment to ({i})", false);
                SetActiveElement(Element, "TextElement", $"fermentConversionSymbol ({i})", false);
            }
            // Get data from Game Object
            data.TryGetComponent(out Piece piece);
            data.TryGetComponent(out CraftingStation craftingStation);
            data.TryGetComponent(out WearNTear wearNTear);
            data.TryGetComponent(out Door door);
            data.TryGetComponent(out Container container);
            data.TryGetComponent(out Fireplace fireplace);
            data.TryGetComponent(out StationExtension stationExtension);
            data.TryGetComponent(out Smelter smelter);
            data.TryGetComponent(out WispSpawner wispSpawner);
            data.TryGetComponent(out Turret turret);
            data.TryGetComponent(out Fermenter fermenter);
            data.TryGetComponent(out ItemStand itemStand);
            data.TryGetComponent(out Ship ship);
            data.TryGetComponent(out Plant plant);
            data.TryGetComponent(out Destructible destructible);
            data.TryGetComponent(out CookingStation cookingStation);

            if (!piece) return;
            Sprite icon = piece.m_icon;
            string name = Localization.instance.Localize($"{piece.m_name}");
            string prefabName = piece.name;
            string description = Localization.instance.Localize($"{piece.m_description}");
            bool isExtension = piece.m_isUpgrade;
            var category = piece.m_category;

            int comfort = piece.m_comfort;
            var comfortGroup = piece.m_comfortGroup;
            bool groundPiece = piece.m_groundPiece;
            bool cultivatedGroundOnly = piece.m_cultivatedGroundOnly;
            bool allowedInDungeons = piece.m_allowedInDungeons;
            Heightmap.Biome onlyInBiome = piece.m_onlyInBiome;

            CraftingStation? pieceCraftingStation = piece.m_craftingStation;
            Piece.Requirement[] resources = piece.m_resources;

            GameObject prefabImage = Element.transform.Find("ImageElement (prefabImage)").gameObject;
            Button prefabImageButton = prefabImage.GetComponent<Button>();
            prefabImageButton.onClick.AddListener(() =>
            {
                TextEditor textEditor = new TextEditor
                {
                    text = prefabName
                };
                textEditor.SelectAll();
                textEditor.Copy();
                
                MessageHud.instance.ShowMessage(
                    MessageHud.MessageType.Center, 
                    Localization.instance.Localize("$almanac_copy_to_clipboard"));
            });

            #region set general data
            SetTextElement(Element, "Name", $"{name}");
            SetTextElement(Element, "Description", $"{description}");
            SetImageElement(Element, "icon", icon, Color.white);
            
            SetTextElement(prefabImage, "prefabName", $"{prefabName}");
            SetTextElement(Element, "category", $"{category.ToString()}");
            SetTextElement(Element, "comfort", $"{comfort}");
            SetTextElement(Element, "extension", isExtension ? "$almanac_true" : "$almanac_false");
            SetTextElement(Element, "groundPiece", groundPiece ? "$almanac_true" : "$almanac_false");
            SetTextElement(Element, "cultivated", cultivatedGroundOnly ? "$almanac_true" : "$almanac_false");
            SetTextElement(Element, "dungeon", allowedInDungeons ? "$almanac_true" : "$almanac_false");
            SetTextElement(Element, "comfortGroup", $"{comfortGroup.ToString()}");
            
            for (int i = 0; i < resources.Length; ++i)
            {
                if (i >= 5) continue;
                string resourceName = resources[i].m_resItem.m_itemData.m_shared.m_name;
                bool isKnown = Player.m_localPlayer.IsMaterialKnown(resourceName);
                if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                int resourceAmount = resources[i].m_amount;
                Sprite resourceIcon = resources[i].m_resItem.m_itemData.m_shared.m_icons[0];
                GameObject ResourceBackground =
                    Element.transform.Find($"ImageElement (recipe ({i}))").gameObject;
                string localizedName = Localization.instance.Localize(resourceName);
                SetActiveElement(Element, "ImageElement", $"recipe ({i})", true);
                SetHoverableText(
                    Element,
                    $"recipe ({i})",
                    isKnown 
                        ? localizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedName 
                );
                SetImageElement(
                    ResourceBackground,
                    "item", 
                    resourceIcon, 
                    isKnown 
                        ? Color.white 
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ?Color.black : Color.white
                );
                SetTextElement(ResourceBackground, $"recipeAmount", $"{resourceAmount}");
            }
            #endregion
            
            if (wearNTear)
            {
                float health = wearNTear.m_health;
                var materialType = wearNTear.m_materialType;
                var modifiers = wearNTear.m_damages;
                var modifierConversionMap = new Dictionary<string, HitData.DamageModifier>()
                {
                    { "blunt", modifiers.m_blunt},
                    { "slash", modifiers.m_slash},
                    { "pierce", modifiers.m_pierce},
                    { "chop", modifiers.m_chop},
                    { "pickaxe", modifiers.m_pickaxe},
                    { "fire", modifiers.m_fire},
                    { "frost", modifiers.m_frost},
                    { "lightning", modifiers.m_lightning},
                    { "poison", modifiers.m_poison},
                    { "spirit", modifiers.m_spirit}
                };
                List<KeyValuePair<string, HitData.DamageModifier>> modifierMap =
                    new List<KeyValuePair<string, HitData.DamageModifier>>(modifierConversionMap); 
                foreach (KeyValuePair<string, HitData.DamageModifier> wearNTearMod in modifierMap)
                {
                    SetTextElement(Element, $"{wearNTearMod.Key}", $"{wearNTearMod.Value}");
                }
                
                SetTextElement(Element, "health", $"{health}<color=white>HP</color>");
                SetTextElement(Element, "material", $"{materialType.ToString()}");
            }
            
            if (destructible)
            {
                float health = destructible.m_health;
                var modifiers = destructible.m_damages;
                var modifierConversionMap = new Dictionary<string, HitData.DamageModifier>()
                {
                    { "blunt", modifiers.m_blunt},
                    { "slash", modifiers.m_slash},
                    { "pierce", modifiers.m_pierce},
                    { "chop", modifiers.m_chop},
                    { "pickaxe", modifiers.m_pickaxe},
                    { "fire", modifiers.m_fire},
                    { "frost", modifiers.m_frost},
                    { "lightning", modifiers.m_lightning},
                    { "poison", modifiers.m_poison},
                    { "spirit", modifiers.m_spirit}
                };
                List<KeyValuePair<string, HitData.DamageModifier>> modifierMap =
                    new List<KeyValuePair<string, HitData.DamageModifier>>(modifierConversionMap); 
                for (int index = 0; index < modifierMap.Count; ++index)
                {
                    SetTextElement(Element, $"{modifierMap[index].Key}", $"{modifierMap[index].Value}");
                }
                
                SetTextElement(Element, "health", $"{health}<color=white>HP</color>");
            }
            
            if (plant)
            {
                SetTextElement(Element, "growTime", $"{plant.m_growTime}");
                SetTextElement(Element, "growTimeMax", $"{plant.m_growTimeMax}");
                SetActiveElement(Element, "TextElement", "growTimeLabel", true);
                SetActiveElement(Element, "TextElement", "growTimeMaxLabel", true);
            }
            
            if (pieceCraftingStation)
            {
                SetImageElement(Element, "craftingStation", pieceCraftingStation.m_icon, Color.white);
                SetHoverableText(Element, "craftingStation", $"{pieceCraftingStation.m_name}");
                SetActiveElement(Element, "ImageElement", "craftingStation", true);
            }
            
            if (craftingStation)
            {
                Dictionary<string, string> stationInfoMap = new Dictionary<string, string>
                {
                    { "discoverRange", craftingStation.m_discoverRange.ToString(CultureInfo.CurrentCulture) },
                    { "rangeBuild", craftingStation.m_rangeBuild.ToString(CultureInfo.CurrentCulture) },
                    { "extraRange", craftingStation.m_extraRangePerLevel.ToString(CultureInfo.CurrentCulture) },
                    { "requireRoof", craftingStation.m_craftRequireRoof.ToString() },
                    { "requireFire", craftingStation.m_craftRequireFire.ToString() },
                    { "basicRecipes", craftingStation.m_showBasicRecipies.ToString() },
                    { "animationIndex", craftingStation.m_useAnimation.ToString() }
                };

                foreach (KeyValuePair<string, string> stationInfo in stationInfoMap)
                {
                    SetTextElement(Element, stationInfo.Key, stationInfo.Value);
                }

                List<string> craftingStationLabels = new List<string>()
                {
                    "discoverRangeLabel",
                    "rangeBuildLabel",
                    "extraRangePerLevelLabel",
                    "requireRoofLabel",
                    "requireFireLabel",
                    "showBasicRecipesLabel",
                    "animationIndexLabel"
                };
                foreach (string craftingStationLabel in craftingStationLabels)
                {
                    SetActiveElement(Element, "TextElement", craftingStationLabel, true);
                }
            }
            
            if (container && !craftingStation)
            {
                SetTextElement(Element, "containerSize", $"{container.m_width}<color=white>x</color>{container.m_height}");
                SetTextElement(Element, "checkGuard", $"{container.m_checkGuardStone}");
                SetTextElement(Element, "autoDestroy", $"{container.m_autoDestroyEmpty}");
                
                List<DropTable.DropData>? defaultItems = container.m_defaultItems.m_drops;
                if (defaultItems != null)
                {
                    float totalWeight = defaultItems.Sum(item => item.m_weight);
                    for (int i = 0; i < defaultItems.Count; i++)
                    {
                        if (i >= 12) continue;
                        GameObject item = defaultItems[i].m_item;
                        item.TryGetComponent(out ItemDrop itemDropData);
                        
                        string dropName = itemDropData.m_itemData.m_shared.m_name;
                        Sprite dropIcon = itemDropData.m_itemData.GetIcon();
                        float weight = defaultItems[i].m_weight;
                        float dropChance = Mathf.Round((weight / totalWeight) * 100f);
                        int dropMin = defaultItems[i].m_stackMin;
                        int dropMax = defaultItems[i].m_stackMax;
    
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(dropName);
                        if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                        string elementName = $"containerDrops ({i})";
                        GameObject ResourceBackground = Element.transform.Find($"ImageElement ({elementName})").gameObject;
                        string localizedContent =
                            Localization.instance.Localize($"{dropName} (<color=orange>{dropChance}%</color>)");
                        SetActiveElement(Element, "ImageElement", elementName, true);
                        SetHoverableText(Element, elementName, isKnown 
                            ? localizedContent
                            : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedContent);
                        SetImageElement(ResourceBackground, "item", dropIcon, isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                        SetTextElement(ResourceBackground, $"recipeAmount", $"{dropMin}<color=white>-</color>{dropMax}");
                    }
                };
                SetActiveElement(Element, "TextElement", "containerSizeLabel", true);
                SetActiveElement(Element, "TextElement", "checkGuardLabel", true);
                SetActiveElement(Element, "TextElement", "autoDestroyLabel", true);
            }

            if (cookingStation)
            {
                ItemDrop? overCookedItem = cookingStation.m_overCookedItem;
                ItemDrop? fuelItem = cookingStation.m_fuelItem;
                
                Dictionary<string, string> cookingConversionMap = new Dictionary<string, string>()
                {
                    { "cookingTooltip", $"{cookingStation.m_addItemTooltip}" },
                    { "overCooked", overCookedItem ? $"{overCookedItem.m_itemData.m_shared.m_name}" : "$almanac_na" },
                    { "availableSlots", $"{cookingStation.m_slots.Length}" },
                    { "cookingRequireFire", $"{cookingStation.m_requireFire}" },
                    { "cookingUseFuel", $"{cookingStation.m_useFuel}" },
                    { "cookingFuelItem", fuelItem ? $"{fuelItem.m_itemData.m_shared.m_name}" : "$almanac_na" },
                    { "cookingMaxFuel", $"{cookingStation.m_maxFuel}" },
                    { "cookingSecPerFuel", $"{cookingStation.m_secPerFuel}" },
                    
                };
                foreach (KeyValuePair<string, string> cookingConversion in cookingConversionMap)
                {
                    SetTextElement(Element, cookingConversion.Key, cookingConversion.Value);
                }

                List<string> cookingLabelIds = new List<string>()
                {
                    "overCookedItemLabel",
                    "availableSlotsLabel",
                    "cookingRequireFireLabel",
                    "cookingUseFuelLabel",
                    "cookingFuelItemLabel",
                    "cookingMaxFuelLabel",
                    "cookingSecPerFuelLabel"
                };
                foreach (string cookingLabelId in cookingLabelIds)
                {
                    SetActiveElement(Element, "TextElement", cookingLabelId, true);
                }

                List<CookingStation.ItemConversion> cookingConversions = cookingStation.m_conversion;
                for (int i = 0; i < cookingConversions.Count; ++i)
                {
                    if (i >= 12) continue;
                    CookingStation.ItemConversion cookingConversion = cookingConversions[i];
                    string cookingItemName = cookingConversion.m_from.m_itemData.m_shared.m_name;
                    Sprite cookingItemIcon = cookingConversion.m_from.m_itemData.GetIcon();
                    float cookTime = cookingConversion.m_cookTime;
        
                    bool isKnown = Player.m_localPlayer.IsMaterialKnown(cookingItemName);
                    if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                    string elementName = $"cookingConversion ({i})";
                    GameObject ResourceBackground = Element.transform.Find($"ImageElement ({elementName})").gameObject;
                    string localizedName = Localization.instance.Localize($"{cookingItemName}");
                    SetActiveElement(Element, "ImageElement", elementName, true);
                    SetHoverableText(Element, elementName, isKnown 
                        ? localizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedName);
                    SetImageElement(ResourceBackground, "item", cookingItemIcon, isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    SetTextElement(ResourceBackground, $"cookTime", $"{cookTime}<color=white>s</color>");
                }
            }

            if (door)
            {
                ItemDrop? key = door.m_keyItem;
                Dictionary<string, string> doorConversionMap = new Dictionary<string, string>()
                {
                    { "doorKey", key ? $"{door.m_keyItem.m_itemData.m_shared.m_name}" : "$almanac_na" },
                    { "doorCanClose", $"{!door.m_canNotBeClosed}" },
                    { "doorGuard", $"{door.m_checkGuardStone}" }
                };
                foreach (string doorLabelId in doorLabelIds)
                {
                    SetActiveElement(Element, "TextElement", doorLabelId, true);
                }

                foreach (KeyValuePair<string, string> doorConversion in doorConversionMap)
                {
                    SetTextElement(Element, doorConversion.Key, doorConversion.Value);
                }
            }

            if (fireplace)
            {
                foreach (string fireplaceLabelId in fireplaceLabelIds)
                {
                    SetActiveElement(Element, "TextElement", fireplaceLabelId, true);
                }

                ItemDrop? fuelItem = fireplace.m_fuelItem;
                
                Dictionary<string, string> fireplaceConversionMap = new Dictionary<string, string>()
                {
                    { "fireplaceMaxFuel", $"{fireplace.m_maxFuel}" },
                    { "fireplaceSecPerFuel", $"{fireplace.m_secPerFuel}" },
                    { "fireplaceInfiniteFuel", $"{fireplace.m_infiniteFuel}" },
                    { "fireplaceFuelItem", fuelItem ? $"{fuelItem.m_itemData.m_shared.m_name}" : "$almanac_na" },
                };
                foreach (KeyValuePair<string, string> fireplaceConversion in fireplaceConversionMap)
                {
                    SetTextElement(Element, fireplaceConversion.Key, fireplaceConversion.Value);
                }

                for (int i = 0; i < fireplace.m_fireworkItemList.Length; ++i)
                {
                    if (i >= 11) continue;
                    Fireplace.FireworkItem firework = fireplace.m_fireworkItemList[i];
                    string fireworkName = firework.m_fireworkItem.m_itemData.m_shared.m_name;
                    Sprite? fireworkIcon = firework.m_fireworkItem.m_itemData.GetIcon();
                    int fireworkCount = firework.m_fireworkItemCount;
                    
                    string elementName = $"fireplaceFireworks ({i})";
                    bool isKnown = Player.m_localPlayer.IsMaterialKnown(fireworkName);
                    if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                    GameObject ResourceBackground = Element.transform.Find($"ImageElement ({elementName})").gameObject;
                    string localizedName = Localization.instance.Localize($"{fireworkName}");
                    SetActiveElement(Element, "ImageElement", elementName, true);
                    SetHoverableText(Element, elementName, isKnown 
                        ? localizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedName);
                    SetImageElement(ResourceBackground, "item", fireworkIcon, isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    SetTextElement(ResourceBackground, $"itemCount", $"{fireworkCount}");
                    
                }
            }

            if (smelter && !craftingStation)
            {
                foreach (string smelterLabelId in smelterLabelIds) SetActiveElement(Element, "TextElement", smelterLabelId, true);
                ItemDrop? fuelItem = smelter.m_fuelItem;
                Dictionary<string, string> smelterConversionMap = new Dictionary<string, string>()
                {
                    { "smelterFuelItem", fuelItem ? $"{fuelItem.m_itemData.m_shared.m_name}" : "$almanac_na" },
                    { "smelterMaxOre", $"{smelter.m_maxOre}" },
                    { "smelterMaxFuel", $"{smelter.m_maxFuel}" },
                    { "smelterFuelPerProduct", $"{smelter.m_fuelPerProduct}" },
                    { "smelterSecPerProduct", $"{smelter.m_secPerProduct}" },
                    { "smelterRequireRoof", $"{smelter.m_requiresRoof}" },
                };
                foreach (KeyValuePair<string, string> smelterConversion in smelterConversionMap)
                {
                    SetTextElement(Element, smelterConversion.Key, smelterConversion.Value);
                }

                for (int i = 0; i < smelter.m_conversion.Count; ++i)
                {
                    if (i >= 7) continue;
                    Smelter.ItemConversion smelterItem = smelter.m_conversion[i];

                    string fromItemName = smelterItem.m_from.m_itemData.m_shared.m_name;
                    Sprite? FromItemIcon = smelterItem.m_from.m_itemData.GetIcon();
                    
                    string FromElementId = $"smelterConversion from ({i})";
                    bool fromIsKnown = Player.m_localPlayer.IsMaterialKnown(fromItemName);
                    if (CheckCheats.PlayerWatcher.noCost) fromIsKnown = true;
                    GameObject smelterFrom = Element.transform.Find($"ImageElement ({FromElementId})").gameObject;
                    string fromLocalizedName = Localization.instance.Localize($"{fromItemName}");
                    SetActiveElement(Element, "ImageElement", FromElementId, true);
                    SetHoverableText(Element, FromElementId, fromIsKnown 
                        ? fromLocalizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : fromLocalizedName);
                    SetImageElement(smelterFrom, "item", FromItemIcon, fromIsKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    
                    string toItemName = smelterItem.m_to.m_itemData.m_shared.m_name;
                    Sprite? toItemIcon = smelterItem.m_to.m_itemData.GetIcon();
                    
                    string toElementId = $"smelterConversion to ({i})";
                    bool toIsKnown = Player.m_localPlayer.IsMaterialKnown(toItemName);
                    if (CheckCheats.PlayerWatcher.noCost) toIsKnown = true;
                    GameObject smelterTo = Element.transform.Find($"ImageElement ({toElementId})").gameObject;
                    string toLocalizedName = Localization.instance.Localize($"{toItemName}");
                    SetActiveElement(Element, "ImageElement", toElementId, true);
                    SetHoverableText(Element, toElementId, toIsKnown 
                        ? toLocalizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : toLocalizedName);
                    SetImageElement(smelterTo, "item", toItemIcon, toIsKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    
                    SetActiveElement(Element, "TextElement", $"smelterConversionSymbol ({i})", true);
                }
            }

            if (wispSpawner)
            {
                GameObject? wispPrefab = wispSpawner.m_wispPrefab;
                foreach (string wispLabelId in wispLabelIds)
                {
                    SetActiveElement(Element, "TextElement", wispLabelId, true);
                }

                Dictionary<string, string> wispConversionMap = new Dictionary<string, string>()
                {
                    { "wispInterval", $"{wispSpawner.m_spawnInterval}" },
                    { "wispSpawnChance", $"{wispSpawner.m_spawnChance * 100}<color=white>%</color>" },
                    { "wispMaxSpawn", $"{wispSpawner.m_maxSpawned}" },
                    { "wispSpawnAtNight", $"{wispSpawner.m_onlySpawnAtNight}" },
                    { "wispSpawnCover", $"{wispSpawner.m_dontSpawnInCover}" },
                    { "wispSpawnDistance", $"{wispSpawner.m_spawnDistance}" },
                };
                foreach (KeyValuePair<string, string> wispConversion in wispConversionMap)
                {
                    SetTextElement(Element, wispConversion.Key, wispConversion.Value);
                }

                if (wispPrefab)
                {
                    Pickable pickable = wispPrefab.GetComponent<Pickable>();
                    ItemDrop pickableGO = pickable.m_itemPrefab.GetComponent<ItemDrop>();
                    Sprite wispPrefabIcon = pickableGO.m_itemData.GetIcon();
                    SetImageElement(Element, "wispPrefab", wispPrefabIcon, Color.white);
                    SetHoverableText(Element, "wispPrefab", $"{pickableGO.m_itemData.m_shared.m_name}");
                }
            }

            if (turret)
            {
                foreach(string turretLabelId in turretLabelIds)
                {
                    SetActiveElement(Element, "TextElement", turretLabelId, true);
                }

                Dictionary<string, string> turretConversionMap = new Dictionary<string, string>()
                {
                    { "turnRate", $"{turret.m_turnRate}" },
                    { "horizontalAngle", $"{turret.m_horizontalAngle}<color=white>°</color>" },
                    { "verticalAngle", $"{turret.m_verticalAngle}<color=white>°</color>" },
                    { "viewDistance", $"{turret.m_viewDistance}" },
                    { "noTargetRate", $"{turret.m_noTargetScanRate}" },
                    { "lookAcceleration", $"{turret.m_lookAcceleration * 100}<color=white>%</color>" },
                    { "lookDeceleration", $"{turret.m_lookDeacceleration * 100}<color=white>%</color>" },
                    { "lookDegrees", $"{turret.m_lookMinDegreesDelta * 100}<color=white>%</color>" },
                    { "turretMaxAmmo", $"{turret.m_maxAmmo}" }
                };
                foreach (KeyValuePair<string, string> turretConversion in turretConversionMap)
                {
                    SetTextElement(Element, turretConversion.Key, turretConversion.Value);
                }

                List<Turret.AmmoType>? allowedAmmo = turret.m_allowedAmmo;
                if (allowedAmmo != null)
                {
                    for (int i = 0; i < allowedAmmo.Count; ++i)
                    {
                        if (i >= 11) return;
                        Turret.AmmoType ammoType = allowedAmmo[i];
                        ItemDrop ammoItemDrop = ammoType.m_ammo;
                        string ammoName = ammoItemDrop.m_itemData.m_shared.m_name;
                        Sprite? ammoIcon = ammoItemDrop.m_itemData.GetIcon();
                        
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(ammoName);
                        if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                        string elementId = $"turretAmmo ({i})";
                        GameObject resourceBackground = Element.transform.Find($"ImageElement ({elementId})").gameObject;
                        string localizedName = Localization.instance.Localize($"{ammoName}");
                        SetActiveElement(Element, "ImageElement", elementId, true);
                        SetHoverableText(Element, elementId, isKnown 
                            ? localizedName
                            : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedName );
                        SetImageElement(resourceBackground, "item", ammoIcon, isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    }
                }
            }

            if (fermenter)
            {
                foreach (string fermentLabelId in fermentLabelIds)
                {
                    SetActiveElement(Element, "TextElement", fermentLabelId, true);
                }

                Dictionary<string, string> fermentConversionMap = new Dictionary<string, string>()
                {
                    { "fermentDuration", $"{fermenter.m_fermentationDuration}" }
                };
                foreach (KeyValuePair<string, string> fermentConversion in fermentConversionMap)
                {
                    SetTextElement(Element, fermentConversion.Key, fermentConversion.Value);
                }

                for (int i = 0; i < fermenter.m_conversion.Count; ++i)
                {
                    if (i >= 12) continue;
                    Fermenter.ItemConversion itemConversion = fermenter.m_conversion[i];
                    ItemDrop fromItemDrop = itemConversion.m_from;
                    ItemDrop toItemDrop = itemConversion.m_to;
                    string fromName = fromItemDrop.m_itemData.m_shared.m_name;
                    string toName = toItemDrop.m_itemData.m_shared.m_name;
                    int producedItems = itemConversion.m_producedItems;

                    Sprite? fromIcon = fromItemDrop.m_itemData.GetIcon();
                    Sprite? toIcon = toItemDrop.m_itemData.GetIcon();

                    bool isFromKnown = Player.m_localPlayer.IsMaterialKnown(fromName);
                    bool isToKnown = Player.m_localPlayer.IsMaterialKnown(toName);

                    string fromElementId = $"ferment from ({i})";
                    string toElementId = $"ferment to ({i})";

                    GameObject fromBackground = Element.transform.Find($"ImageElement ({fromElementId})").gameObject;
                    GameObject toBackground = Element.transform.Find($"ImageElement ({toElementId})").gameObject;
                    string fromLocalizedName = Localization.instance.Localize($"{fromName}");
                    string toLocalizedName = Localization.instance.Localize($"{toName}");
                    SetActiveElement(Element, "ImageElement", fromElementId, true);
                    SetHoverableText(Element, fromElementId, isFromKnown 
                        ? fromLocalizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : fromLocalizedName);
                    SetImageElement(fromBackground, "item", fromIcon, isFromKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    
                    SetActiveElement(Element, "TextElement", $"fermentConversionSymbol ({i})", true);
                    
                    SetActiveElement(Element, "ImageElement", toElementId, true);
                    SetHoverableText(Element, toElementId, isToKnown 
                        ? toLocalizedName
                        : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : toLocalizedName);
                    SetImageElement(toBackground, "item", toIcon, isToKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white);
                    
                    SetTextElement(toBackground, "produceAmount", $"{producedItems}");
                }
            }
            
            if (stationExtension && !fermenter)
            {
                CraftingStation? extensionCraftingStation = stationExtension.m_craftingStation;
                Sprite extCraftIcon = extensionCraftingStation.m_icon;
                SetImageElement(Element, "extensionCraftingStation", extCraftIcon, Color.white);
                SetHoverableText(Element, "extensionCraftingStation", $"{extensionCraftingStation.m_name}");

                Dictionary<string, string> stationExtensionConversionMap = new Dictionary<string, string>()
                {
                    { "stationDistance", $"{stationExtension.m_maxStationDistance}" },
                    { "extensionStack", $"{stationExtension.m_stack}"}
                };
                foreach (KeyValuePair<string, string> extensionConversion in stationExtensionConversionMap)
                {
                    SetTextElement(Element, extensionConversion.Key, extensionConversion.Value);
                }
                List<string> extensionLabels = new List<string>()
                {
                    "maxStationDistanceLabel",
                    "extensionStationStackLabel"
                };
                foreach (string extensionLabel in extensionLabels)
                {
                    SetActiveElement(Element, "TextElement", extensionLabel, true);
                }
            }
            
            
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
            float durabilityPerLevel = sharedData.m_durabilityPerLevel;
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
            item.TryGetComponent(out Floating floatingScript);

            bool itemFloats = floatingScript != null;

            Recipe recipe = ObjectDB.instance.GetRecipe(data.m_itemData);
            
            // Set all data to default
            SetActiveElement(Element, "TextElement", "recipeTitle", true);
            SetActiveElement(Element, "TextElement", "recipeNull", false);
            
            for (int i = 0; i < 5; ++i)
            {
                SetActiveElement(Element, "ImageElement", $"recipe ({i})", false);
            }
            
            SetActiveElement(Element, "ImageElement", "craftingStation", false);
            
            SetTextElement(Element, "setDescription", "$almanac_not_part_of_set");
            SetTextElement(Element, "setName", "");
            SetTextElement(Element, "modifySkill", "");
            SetTextElement(Element, "damageMod", "");
            SetTextElement(Element, "floating", "$almanac_item_can_float");
            
            if (!itemFloats) SetTextElement(Element, "floating", "$almanac_item_can_not_float");

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
                        bool isKnown = Player.m_localPlayer.IsMaterialKnown(resourceName);
                        if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                        int resourceAmount = resources[i].m_amount;
                        Sprite resourceIcon = resources[i].m_resItem.m_itemData.m_shared.m_icons[0];
                        GameObject ResourceBackground =
                            Element.transform.Find($"ImageElement (recipe ({i}))").gameObject;
                        string localizedName = Localization.instance.Localize(resourceName);
                        SetActiveElement(Element, "ImageElement", $"recipe ({i})", true);
                        SetHoverableText(
                            Element,
                            $"recipe ({i})",
                             isKnown 
                                 ? localizedName
                                 : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedName
                            );
                        SetImageElement(
                            ResourceBackground,
                            "item", 
                            resourceIcon, 
                            isKnown 
                                ? Color.white 
                                : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white
                            );
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

                Transform targetElement = Element.transform.Find("ImageElement (craftingStation)");
                
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
                    foreach (GameObject station in Almanac.CreateAlmanac.smelterStations)
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
                { "durability", $"{maxDurability} <color=white>+</color>{durabilityPerLevel}<color=white>/level</color>" },
                { "movement", $"{movementMod}%" },
                { "eitrRegen", $"{eitrRegenMod}%" },
                { "stamMod", $"{staminaMod}%" }
            };
            foreach (KeyValuePair<string, string> textData in staticTextData)
            {
                SetTextElement(Element, textData.Key, textData.Value);
            }

            GameObject prefabImage = Element.transform.Find("ImageElement (prefabImage)").gameObject;
            SetTextElement(prefabImage, "prefabName", $"{data.name}");
            SetImageElement(Element, "icon", sprite, Color.white);
            SetTextElement(Element, "teleportable", teleportable ? "$almanac_teleportable" : "$almanac_not_teleportable");

            Dictionary<string, string> foodDatas = new Dictionary<string, string>()
            {
                { "healthBonus", $"{foodHealth}" },
                { "staminaBonus", $"{foodStamina}" },
                { "eitrBonus", $"{foodEitr}" },
                { "foodBurn", $"{foodBurn}" },
                { "foodRegen", $"{foodRegen}" },
            };
            foreach (KeyValuePair<string, string> foodData in foodDatas)
            {
                SetTextElement(Element, foodData.Key, foodData.Value);
            }
            SetTextElement(Element, "consumeEffectDescription", "$almanac_no_consume_bonus");
            Dictionary<string, Color> consumeEffectDefault = new Dictionary<string, Color>()
            {
                { "consumeEffectBlunt", Color.white },
                { "consumeEffectSlash", Color.white },
                { "consumeEffectPierce", Color.white },
                { "consumeEffectChop", Color.white },
                { "consumeEffectPickaxe", Color.white },
                { "consumeEffectFire", Color.white },
                { "consumeEffectIce", Color.white },
                { "consumeEffectLightning", Color.white },
                { "consumeEffectPoison", Color.white },
                { "consumeEffectSpirit", Color.white }
            };
            List<KeyValuePair<string, Color>> consumeEffectDefaultList = consumeEffectDefault.ToList();
            foreach (var consumeEffectData in consumeEffectDefaultList)
            {
                ColorizeImageElement(Element, consumeEffectData.Key, consumeEffectData.Value);
                string tag = Localization.instance.Localize(GetTagFromID(consumeEffectData.Key));
                SetHoverableText(Element, $"{consumeEffectData.Key}", $"{tag}");
            }
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

            if (sharedData.m_damageModifiers.Count > 0)
            {
                HitData.DamageModPair gearDamageModifier = sharedData.m_damageModifiers[0];
                SetTextElement(Element, "damageMod", $"<color=orange>{gearDamageModifier.m_modifier}</color> VS <color=orange>{gearDamageModifier.m_type}</color>");
            }
            Dictionary<string, Color> defaultEffects = new Dictionary<string, Color>()
            {
                { "setEffectBlunt", Color.white },
                { "setEffectSlash", Color.white },
                { "setEffectPierce", Color.white },
                { "setEffectChop", Color.white },
                { "setEffectPickaxe", Color.white },
                { "setEffectFire", Color.white },
                { "setEffectIce", Color.white },
                { "setEffectLightning", Color.white },
                { "setEffectPoison", Color.white },
                { "setEffectSpirit", Color.white }
            };
            List<KeyValuePair<string, Color>> defaultEffectList = defaultEffects.ToList();
            foreach (var setEffectData in defaultEffectList)
            {
                ColorizeImageElement(Element, setEffectData.Key, setEffectData.Value);
                string tag = Localization.instance.Localize(GetTagFromID(setEffectData.Key));
                SetHoverableText(Element, $"{setEffectData.Key}", $"{tag}");
            }
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
            GameObject armorBg = Element.transform.Find("ImageElement (armorBg)").gameObject;
            
            SetTextElement(armorBg, "armor", "0");
            SetTextElement(Element, "armorPerLevel", "0");
            
            SetTextElement(armorBg, "armor", $"{sharedData.m_armor}");
            SetTextElement(Element, "armorPerLevel", $"{sharedData.m_armorPerLevel}");
            Dictionary<string, string> weaponTypeTagValues = new Dictionary<string, string>()
            {
                { "bluntTagWeaponValue", $"{sharedData.m_damages.m_blunt}" },
                { "slashTagWeaponValue", $"{sharedData.m_damages.m_slash}" },
                { "pierceTagWeaponValue", $"{sharedData.m_damages.m_pierce}" },
                { "chopTagWeaponValue", $"{sharedData.m_damages.m_chop}" },
                { "pickaxeTagWeaponValue", $"{sharedData.m_damages.m_pickaxe}" },
                { "fireTagWeaponValue", $"{sharedData.m_damages.m_fire}" },
                { "frostTagWeaponValue", $"{sharedData.m_damages.m_frost}" },
                { "lightningTagWeaponValue", $"{sharedData.m_damages.m_lightning}" },
                { "poisonTagWeaponValue", $"{sharedData.m_damages.m_poison}" },
                { "spiritTagWeaponValue", $"{sharedData.m_damages.m_spirit}" },
            };
            foreach (KeyValuePair<string, string> weaponType in weaponTypeTagValues)
            {
                SetTextElement(Element, weaponType.Key, $"<color=yellow>{weaponType.Value}</color>");
            }
            Dictionary<string, string> weaponTypeTagPerLevel = new Dictionary<string, string>()
            {
                { "bluntTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_blunt}" },
                { "slashTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_slash}" },
                { "pierceTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_pierce}" },
                { "chopTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_chop}" },
                { "pickaxeTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_pickaxe}" },
                { "fireTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_fire}" },
                { "frostTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_frost}" },
                { "lightningTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_lightning}" },
                { "poisonTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_poison}" },
                { "spiritTagWeaponPerLevel", $"{sharedData.m_damagesPerLevel.m_spirit}" },
            };
            foreach (KeyValuePair<string, string> weaponPerLevel in weaponTypeTagPerLevel)
            {
                SetTextElement(Element, weaponPerLevel.Key, $"+<color=orange>{weaponPerLevel.Value}</color>/level");
            }

            Dictionary<string, string> weaponAttackData = new Dictionary<string, string>()
            {
                { "attackStamina", $"{sharedData.m_attack.m_attackStamina}" },
                { "attackEitr", $"{sharedData.m_attack.m_attackEitr}" },
                { "attackHealth", $"{sharedData.m_attack.m_attackHealth}" },
                { "attackHealthPercentage", $"{sharedData.m_attack.m_attackHealthPercentage}%" },
                { "speedFactor", $"{sharedData.m_attack.m_speedFactor * 100}%" },
                { "speedFactorRotation", $"{sharedData.m_attack.m_speedFactorRotation * 100}%" },
                { "attackStartNoise", $"{sharedData.m_attack.m_attackStartNoise}" },
                { "attackHitNoise", $"{sharedData.m_attack.m_attackHitNoise}" },
                { "damageMultiplier", $"{sharedData.m_attack.m_damageMultiplier}" },
                { "forceMultiplier", $"{sharedData.m_attack.m_forceMultiplier}" },
                { "staggerMultiplier", $"{sharedData.m_attack.m_staggerMultiplier}" },
                { "recoilPushback", $"{sharedData.m_attack.m_recoilPushback}" },
                { "selfDamage", $"{sharedData.m_attack.m_selfDamage}" },
                
                { "drawDuration", $"{sharedData.m_attack.m_drawDurationMin}" },
                { "drawStamina", $"{sharedData.m_attack.m_drawStaminaDrain}" },
                { "reloadTime", $"{sharedData.m_attack.m_reloadTime}" },
                { "reloadStaminaDrain", $"{sharedData.m_attack.m_reloadStaminaDrain}" },

            };
            foreach (KeyValuePair<string, string> weaponAttackInfo in weaponAttackData)
            {
                SetTextElement(Element, weaponAttackInfo.Key, weaponAttackInfo.Value);
            }
            // Fish
            item.TryGetComponent(out Fish fishScript);
            if (fishScript != null)
            {
                SetImageElement(
                    Element, "baitIcon", 
                    fishScript.m_baits[0].m_bait.m_itemData.m_shared.m_icons[0],
                    Color.white
                    );
                SetHoverableText(
                    Element, "baitIcon",
                    $"{fishScript.m_baits[0].m_bait.m_itemData.m_shared.m_name}"
                    );
                for (int i =0; i < 3; ++i)
                {
                    SetActiveElement(Element, "ImageElement", $"fishDrops ({i})", false);
                }
                for (int i = 0; i < fishScript.m_extraDrops.m_drops.Count; ++i)
                {
                    GameObject drop = fishScript.m_extraDrops.m_drops[i].m_item;
                    ItemDrop itemDrop = drop.GetComponent<ItemDrop>();
                    string dropItemName = itemDrop.m_itemData.m_shared.m_name;
                    bool isKnown = Player.m_localPlayer.IsMaterialKnown(dropItemName);
                    if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                    GameObject fishDropBg = Element.transform.Find($"ImageElement (fishDrops ({i}))").gameObject;
                    string localizedName = Localization.instance.Localize(dropItemName);
                    SetImageElement(
                        fishDropBg,
                        "fishDropItemIcon",
                        itemDrop.m_itemData.m_shared.m_icons[0],
                        isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white
                        );
                    SetHoverableText(
                        Element,
                        $"fishDrops ({i})",
                        isKnown ? localizedName : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : localizedName);
                    SetActiveElement(Element, "ImageElement", $"fishDrops ({i})", true);
                }
            }
            

            // Reorganize panel based on topic
            Dictionary<string, string> foodElements = new Dictionary<string, string>()
            {
                { "foodTitle", "TextElement" },
                
                { "healthBonusLabel", "TextElement" },
                { "healthBonus", "TextElement" },
                
                { "staminaBonusLabel", "TextElement" },
                { "staminaBonus", "TextElement" },
                
                { "eitrBonusLabel", "TextElement" },
                { "eitrBonus", "TextElement" },

                { "foodBurnLabel", "TextElement" },
                { "foodBurn", "TextElement" },
                
                { "foodRegenLabel", "TextElement" },
                { "foodRegen", "TextElement" },
                
                { "consumeEffectDescription", "TextElement" },

                { "consumeEffectBlunt", "ImageElement" },
                { "consumeEffectSlash", "ImageElement" },
                { "consumeEffectPierce", "ImageElement" },
                { "consumeEffectChop", "ImageElement" },
                { "consumeEffectPickaxe", "ImageElement" },
                { "consumeEffectFire", "ImageElement" },
                { "consumeEffectIce", "ImageElement" },
                { "consumeEffectLightning", "ImageElement" },
                { "consumeEffectPoison", "ImageElement" },
                { "consumeEffectSpirit", "ImageElement" },
            };

            Dictionary<string, string> modifierElements = new Dictionary<string, string>()
            {
                { "statModifiersTitle", "TextElement" },
                
                { "movementLabel", "TextElement" },
                { "movement", "TextElement" },
                
                { "eitrRegenLabel", "TextElement" },
                { "eitrRegen", "TextElement" },
                
                { "stamModLabel", "TextElement" },
                { "stamMod", "TextElement" },
                
                { "damageMod", "TextElement" },
                
                { "setName", "TextElement" },
                { "setDescription", "TextElement" },
                
                { "setEffectBlunt", "ImageElement" },
                { "setEffectSlash", "ImageElement" },
                { "setEffectPierce", "ImageElement" },
                { "setEffectChop", "ImageElement" },
                { "setEffectPickaxe", "ImageElement" },
                { "setEffectFire", "ImageElement" },
                { "setEffectIce", "ImageElement" },
                { "setEffectLightning", "ImageElement" },
                { "setEffectPoison", "ImageElement" },
                { "setEffectSpirit", "ImageElement" },
                
                { "modifySkill", "TextElement" },
            };

            Dictionary<string, string> equipmentElements = new Dictionary<string, string>()
            {
                { "armorBg", "ImageElement" },
                
                { "equipmentStats", "TextElement" },
                
                { "armorPerLevelLabel", "TextElement" },
                { "armorPerLevel", "TextElement" },
            };

            Dictionary<string, string> weaponElements = new Dictionary<string, string>()
            {
                { "weaponBlunt", "ImageElement" },
                { "weaponSlash", "ImageElement" },
                { "weaponPierce", "ImageElement" },
                { "weaponChop", "ImageElement" },
                { "weaponPickaxe", "ImageElement" },
                { "weaponFire", "ImageElement" },
                { "weaponIce", "ImageElement" },
                { "weaponLightning", "ImageElement" },
                { "weaponPoison", "ImageElement" },
                { "weaponSpirit", "ImageElement" },
                
                { "bluntTag", "TextElement" },
                { "slashTag", "TextElement" },
                { "pierceTag", "TextElement" },
                { "chopTag", "TextElement" },
                { "pickaxeTag", "TextElement" },
                { "fireTag", "TextElement" },
                { "frostTag", "TextElement" },
                { "lightningTag", "TextElement" },
                { "poisonTag", "TextElement" },
                { "spiritTag", "TextElement" },
                
                { "bluntTagWeaponValue", "TextElement" },
                { "slashTagWeaponValue", "TextElement" },
                { "pierceTagWeaponValue", "TextElement" },
                { "chopTagWeaponValue", "TextElement" },
                { "pickaxeTagWeaponValue", "TextElement" },
                { "fireTagWeaponValue", "TextElement" },
                { "frostTagWeaponValue", "TextElement" },
                { "lightningTagWeaponValue", "TextElement" },
                { "poisonTagWeaponValue", "TextElement" },
                { "spiritTagWeaponValue", "TextElement" },
                
                { "bluntTagWeaponPerLevel", "TextElement" },
                { "slashTagWeaponPerLevel", "TextElement" },
                { "pierceTagWeaponPerLevel", "TextElement" },
                { "chopTagWeaponPerLevel", "TextElement" },
                { "pickaxeTagWeaponPerLevel", "TextElement" },
                { "fireTagWeaponPerLevel", "TextElement" },
                { "frostTagWeaponPerLevel", "TextElement" },
                { "lightningTagWeaponPerLevel", "TextElement" },
                { "poisonTagWeaponPerLevel", "TextElement" },
                { "spiritTagWeaponPerLevel", "TextElement" },
                
                { "attackStaminaLabel", "TextElement" },
                { "attackStamina", "TextElement" },
                
                { "attackEitrLabel", "TextElement" },
                { "attackEitr", "TextElement" },
                
                { "attackHealthLabel", "TextElement" },
                { "attackHealth", "TextElement" },
                
                { "attackHealthPercentageLabel", "TextElement" },
                { "attackHealthPercentage", "TextElement" },
                
                { "speedFactorLabel", "TextElement" },
                { "speedFactor", "TextElement" },
                
                { "speedFactorRotationLabel", "TextElement" },
                { "speedFactorRotation", "TextElement" },
                
                { "attackStartNoiseLabel", "TextElement" },
                { "attackStartNoise", "TextElement" },
                
                { "attackHitNoiseLabel", "TextElement" },
                { "attackHitNoise", "TextElement" },
                
                { "damageMultiplierLabel", "TextElement" },
                { "damageMultiplier", "TextElement" },
                
                { "forceMultiplierLabel", "TextElement" },
                { "forceMultiplier", "TextElement" },
                
                { "staggerMultiplierLabel", "TextElement" },
                { "staggerMultiplier", "TextElement" },
                
                { "recoilPushbackLabel", "TextElement" },
                { "recoilPushback", "TextElement" },
                
                { "selfDamageLabel", "TextElement" },
                { "selfDamage", "TextElement" },
            };

            Dictionary<string, string> bowElements = new Dictionary<string, string>()
            {
                { "drawDurationMinLabel", "TextElement" },
                { "drawStaminaDrainLabel", "TextElement" },
                { "reloadTimeLabel", "TextElement" },
                { "reloadStaminaDrainLabel", "TextElement" },
                
                { "drawDuration", "TextElement" },
                { "drawStamina", "TextElement" },
                { "reloadTime", "TextElement" },
                { "reloadStaminaDrain", "TextElement" },
            };

            Dictionary<string, string> generalElements = new Dictionary<string, string>()
            {
                { "floating",  "TextElement"},
                { "teleportable", "TextElement" }
            };
            Dictionary<string, string> fishElements = new Dictionary<string, string>()
            {
                { "FishBaitLabel", "TextElement" },
                { "baitIcon", "ImageElement" },
                { "FishDropsTitle", "TextElement" },
            };

            var ItemType = sharedData.m_itemType;
            
            foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, true);
            foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, true);
            foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, true);
            foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, true);
            foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, true);
            foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
            foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, true);
            
            if (ItemType is ItemDrop.ItemData.ItemType.Material)
            {
                foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, false);
                foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, false);
                foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, false);
                foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, false);
                foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, false);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, false);
            }
            
            if (ItemType is ItemDrop.ItemData.ItemType.Consumable)
            {
                // foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, true);
                foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, false);
                foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, false);
                foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, false);
                foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, false);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, false);
            }
            
            if (ItemType 
                is ItemDrop.ItemData.ItemType.Chest 
                or ItemDrop.ItemData.ItemType.Helmet 
                or ItemDrop.ItemData.ItemType.Legs
                or ItemDrop.ItemData.ItemType.Shoulder
                or ItemDrop.ItemData.ItemType.Misc
                or ItemDrop.ItemData.ItemType.Customization
                or ItemDrop.ItemData.ItemType.Utility
                )
            {
                foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, false);
                // foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, true);
                // foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, true);
                foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, false);
                foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, false);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, false);
            }
            
            if (ItemType
                is ItemDrop.ItemData.ItemType.Hands
                or ItemDrop.ItemData.ItemType.Shield
                or ItemDrop.ItemData.ItemType.Tool
                or ItemDrop.ItemData.ItemType.Torch
                or ItemDrop.ItemData.ItemType.Attach_Atgeir
                or ItemDrop.ItemData.ItemType.OneHandedWeapon
                or ItemDrop.ItemData.ItemType.TwoHandedWeapon
                or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft
               )
            {
                foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, false);
                // foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, true);
                // foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, true);
                // foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, true);
                foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, false);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, false);
            }

            if (ItemType is ItemDrop.ItemData.ItemType.Bow)
            {
                foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, false);
                // foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, true);
                // foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, true);
                // foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, true);
                // foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, true);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, false);
            }
            
            if (ItemType is ItemDrop.ItemData.ItemType.Fish)
            {
                foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, false);
                foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, false);
                foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, false);
                foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, false);
                foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, false);
                foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, false);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, true);
            }

            if (ItemType 
                is ItemDrop.ItemData.ItemType.Ammo 
                or ItemDrop.ItemData.ItemType.AmmoNonEquipable
                )
            {
                foreach (KeyValuePair<string, string> foodElement in foodElements) SetActiveElement(Element, foodElement.Value, foodElement.Key, false);
                // foreach (KeyValuePair<string, string> modiferElement in modifierElements) SetActiveElement(Element, modiferElement.Value, modiferElement.Key, true);
                // foreach (KeyValuePair<string, string> equipmentElement in equipmentElements) SetActiveElement(Element, equipmentElement.Value, equipmentElement.Key, true);
                // foreach (KeyValuePair<string, string> weaponElement in weaponElements) SetActiveElement(Element, weaponElement.Value, weaponElement.Key, true);
                foreach (KeyValuePair<string, string> bowElement in bowElements) SetActiveElement(Element, bowElement.Value, bowElement.Key, false);
                // foreach (KeyValuePair<string, string> generalElement in generalElements) SetActiveElement(Element, generalElement.Value, generalElement.Key, true);
                foreach (KeyValuePair<string, string> fishElement in fishElements) SetActiveElement(Element, fishElement.Value, fishElement.Key, false);
            }
            
            SetActiveElement(Element, "TextElement", "teleportable", !teleportable);
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

        public static void setAlmanacData(GameObject dummyElement, string creatureName, Image trophyIcon = null!, string prefabName = "")
        {
            Player player = Player.m_localPlayer;
            List<CreatureDataCollector.CreatureData> creatureData = Almanac.CreateAlmanac.creatures;
            CreatureDataCollector.CreatureData creature = getCreature(creatureData, creatureName, prefabName);

            Transform? clipBoard = dummyElement.transform.Find("ImageElement (Clipboard)");
            Transform? clipBoardHover = clipBoard.Find("hoverTextElement");
            clipBoardHover.TryGetComponent(out TextMeshProUGUI clipBoardTextMesh);
            clipBoardTextMesh.text = creature.name;
            
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
                string trophy = creature.trophyName;
                foreach (string item in creature.drops)
                {
                    if (item.Contains("Trophy")) trophy = item;
                }

                GameObject trophyObj = ObjectDB.instance.GetItemPrefab(trophy);
                if (trophyObj)
                {
                    trophyObj.TryGetComponent(out ItemDrop itemDrop);
                    if (!itemDrop) return;

                    Sprite icon = itemDrop.m_itemData.GetIcon();
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
                Transform dropBgElement = dummyElement.transform.Find($"ImageElement (dropIconBg ({index}))");
                try
                {
                    GameObject? item = ObjectDB.instance.GetItemPrefab(creature.drops[index]);
                    if (!item)
                    {
                        SetActiveElement(dummyElement, "ImageElement", $"dropIconBg ({index})", false);
                    }
                    else
                    {
                        item.TryGetComponent(out ItemDrop itemDrop);
                        if (!itemDrop) continue;
                        float dropChance = creature.dropChance[creature.drops[index]];
                        Sprite icon = itemDrop.m_itemData.GetIcon();
                        string itemName = itemDrop.m_itemData.m_shared.m_name;
                        bool isKnown = player.IsMaterialKnown(itemName);
                        if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                        string content = $"{itemName} (<color=orange>{dropChance}%</color>)";
                        SetImageElement(
                            dropBgElement.gameObject,
                            $"creatureDrop ({index})", icon,
                            isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white
                        );
                        SetHoverableText(
                            dropBgElement.gameObject,
                            $"creatureDrop ({index})",
                            isKnown ? content : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : content
                            );

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
                Transform bgElement = dummyElement.transform.Find($"ImageElement (iconBg ({index}))");
                try
                {
                    GameObject item = ObjectDB.instance.GetItemPrefab(creature.consumeItems[index]);
                    if (!item)
                    {
                        SetActiveElement(dummyElement, "ImageElement", $"iconBg ({index})", false);
                    }
                    else
                    {
                        item.TryGetComponent(out ItemDrop itemDrop);
                        if (!itemDrop) return;
                        Sprite icon = itemDrop.m_itemData.GetIcon();
                        string itemName = itemDrop.m_itemData.m_shared.m_name;
                        bool isKnown = player.IsKnownMaterial(itemName);
                        if (CheckCheats.PlayerWatcher.noCost) isKnown = true;
                        
                        SetImageElement(
                            bgElement.gameObject, $"consumeItem ({index})", icon,
                            isKnown ? Color.white : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? Color.black : Color.white
                            );
                        SetHoverableText(
                            bgElement.gameObject,
                            $"consumeItem ({index})",
                            isKnown ? itemName : AlmanacPlugin._KnowledgeLock.Value == AlmanacPlugin.Toggle.On ? "???" : itemName
                            );

                        SetActiveElement(dummyElement, "ImageElement", $"iconBg ({index})", true);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    SetActiveElement(dummyElement, "ImageElement", $"iconBg ({index})", false);
                }
            }
            
            // Set killed value
            int tempKillCount = TrackPlayerKills.TempMonstersKilled[creature.defeatedKey];
            int tempDeathCount = TrackPlayerDeaths.TempPlayerDeaths[creature.defeatedKey];
            SetTextElement(dummyElement, "KilledBy", $"<color=orange>{creature.PlayerKills + tempKillCount}</color>");
            SetTextElement(dummyElement, "KilledPlayer", $"<color=orange>{creature.PlayerDeaths + tempDeathCount}</color>");

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
            Transform element = dummyElement.transform.Find($"ImageElement ({id})");
            Transform hoverElement = element.transform.Find("hoverTextElement");
            if (!hoverElement) return;
            hoverElement.TryGetComponent(out TextMeshProUGUI text);
            if (!text) return;
            
            text.text = Localization.instance.Localize(content);
        }

        private static void SetTextElement(GameObject dummyElement, string id, string content = "Unknown")
        {
            Transform element = dummyElement.transform.Find($"TextElement ({id})");
            if (!element) return;

            element.TryGetComponent(out TextMeshProUGUI text);
            if (!text) return;
            
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
                : Localization.instance.Localize(originalContent);
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
            Transform element = dummyElement.transform.Find($"ImageElement ({id})");
            element.gameObject.SetActive(true);
            Image image = element.gameObject.GetComponent<Image>();
            image.sprite = inputImage.sprite;
            image.material = inputImage.material;
            image.fillCenter = inputImage.fillCenter;
            image.type = inputImage.type;
        }

        private static void SetImageElement(GameObject dummyElement, string id, Sprite? sprite, Color color)
        {
            Transform element = dummyElement.transform.Find($"ImageElement ({id})");
            if (!element) return;
            element.gameObject.SetActive(true);
            element.TryGetComponent(out Image image);
            if (!image) return;
            image.sprite = sprite;
            image.color = color;
        }

        private static void SetImageDefault(GameObject dummyElement, string id)
        {
            Transform element = dummyElement.transform.Find($"ImageElement ({id})");
            if (!element) return;
            element.gameObject.SetActive(false);
        }

        private static CreatureDataCollector.CreatureData getCreature(List<CreatureDataCollector.CreatureData> data,
            string creatureName, string prefabName)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            var result = new CreatureDataCollector.CreatureData();

            if (prefabName != "")
            {
                foreach (var creature in data)
                {
                    if (creature.name == prefabName)
                    {
                        return creature;
                    }
                }
            }
            for (int i = data.Count - 1; i >= 0; --i)
            {
                var creature = data[i];
                string prefab = creature.name;

                string displayName = Localization.instance.Localize(creature.display_name);
                if (displayName == "The Queen") displayName = "Queen";
                if (displayName == "Dvergr rogue") displayName = "Dvergr";
                if (creatureName == "Draugr" && prefab is "TrainingDummy" or "Draugr_Elite" or "ML_Draugr_Spawn" or "Draugr_Ranged") continue;
                if (creatureName == "MolluscanLand" && prefab is "Molluscan") continue;
                if (textInfo.ToTitleCase(displayName) != textInfo.ToTitleCase(creatureName)) continue;
                return creature;
            }
            
            foreach (var creature in data)
            {
                string prefab = creature.name;
                string trophyName = Localization.instance.Localize(creature.trophyName);
                string displayName = Localization.instance.Localize(creature.display_name);
                if (displayName == "The Queen") displayName = "Queen";
                if (displayName == "Dvergr rogue") displayName = "Dvergr";
                
                // Get exact match for draugr
                if (creatureName == "Draugr" && prefab is "TrainingDummy" or "Draugr_Elite" or "ML_Draugr_Spawn" or "Draugr_Ranged") continue;
                if (creatureName == "Molluscan" && prefab is "Molluscan") continue;

                if (trophyName == creatureName)
                {
                    if (creatureName.ToLower().Contains(displayName.ToLower()))
                    {
                        result = creature;
                        break;
                    }

                    if (trophyName.ToLower().Contains(displayName.ToLower()))
                    {
                        result = creature;
                        break;
                    }

                    if (SubstringExistsInAnyOrder(trophyName, creatureName))
                    {
                        result = creature;
                        break;
                    }
                }

                if (trophyName.Contains(creatureName))
                {
                    result = creature;
                    break;
                }
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
            return Almanac.CreateAlmanac.creatures;
        }
    }
}