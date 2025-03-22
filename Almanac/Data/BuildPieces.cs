using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.FileSystem;
using HarmonyLib;
using UnityEngine;
using static Almanac.UI.Entries;
using static Almanac.Utilities.Helpers;

namespace Almanac.Data;

public static class BuildPieces
{
    private static readonly Dictionary<Piece.PieceCategory, List<PieceData>> m_pieces = new();
    private static readonly List<PieceData> m_plants = new();
    private static readonly List<PieceData> m_defaults = new();
    private static readonly List<PieceData> m_comforts = new();
    private static readonly Dictionary<Piece.PieceCategory, List<PieceData>> m_krumpac = new();

    public static List<PieceData> GetPieces(string category)
    {
        switch (category)
        {
            case "plants":
                return AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.Off ? m_plants : m_plants.FindAll(x => !Filters.m_filter.Contains(x.m_prefab.name));
            case "defaults":
                return AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.Off ? m_defaults : m_defaults.FindAll(x => !Filters.m_filter.Contains(x.m_prefab.name));
            case "comforts":
                return AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.Off ? m_comforts : m_comforts.FindAll(x => !Filters.m_filter.Contains(x.m_prefab.name));
            default:
                return !Enum.TryParse(category, true, out Piece.PieceCategory pieceCategory) ? new() : GetPieces(pieceCategory);
        }
    }

    public static List<PieceData> GetPieces(Piece.PieceCategory category)
    {
        return m_pieces.TryGetValue(category, out List<PieceData> pieces)
            ? pieces.FindAll(x => !Filters.Ignore(x.m_prefab.name))
            : new();
    }

    public static List<PieceData> GetBuildPieces()
    {
        List<PieceData> output = new();
        output.AddRange(GetPieces(Piece.PieceCategory.BuildingWorkbench));
        output.AddRange(GetPieces(Piece.PieceCategory.BuildingStonecutter));
        return AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.Off ? output : output.FindAll(x => !Filters.m_filter.Contains(x.m_prefab.name));
    }


    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            foreach (var prefab in __instance.m_prefabs)
            {
                var _ = new PieceData(prefab);
            }
        }
    }

    public class PieceData
    {
        public readonly GameObject m_prefab;
        public readonly Piece m_piece = null!;

        public bool IsKrumpacPiece()
        {
            if (m_krumpCrafting.Contains(m_prefab.name))
            {
                if (m_krumpac.TryGetValue(Piece.PieceCategory.Crafting, out List<PieceData> list)) list.Add(this);
                else m_krumpac[Piece.PieceCategory.Crafting] = new() { this };
                return true;
            }

            if (m_krumpMisc.Contains(m_prefab.name))
            {
                if (m_krumpac.TryGetValue(Piece.PieceCategory.Misc, out List<PieceData> list)) list.Add(this);
                else m_krumpac[Piece.PieceCategory.Misc] = new() { this };
                return true;
            }

            return false;
        }

        public PieceData(GameObject prefab)
        {
            m_prefab = prefab;
            if (!prefab.TryGetComponent(out Piece component)) return;
            m_piece = component;
            if (IsKrumpacPiece()) return;
            if (prefab.GetComponent<Plant>())
            {
                m_plants.Add(this);
            }
            else
            {
                if (m_piece.m_comfort > 0) m_comforts.Add(this);
                if (m_defaultPrefabNames.Contains(prefab.name))
                {
                    m_defaults.Add(this);
                }
                else if (m_specialCases.TryGetValue(prefab.name, out Piece.PieceCategory specialCaseCategory))
                {
                    if (m_pieces.TryGetValue(specialCaseCategory, out List<PieceData> list)) list.Add(this);
                    else m_pieces[specialCaseCategory] = new() { this };
                }
                else
                {
                    if (m_pieces.TryGetValue(component.m_category, out List<PieceData> list)) list.Add(this);
                    else m_pieces[component.m_category] = new() { this };
                }
            }
        }

        public List<Entry> GetEntries()
        {
            EntryBuilder builder = new();
            if (AlmanacPlugin._ShowAllData.Value is AlmanacPlugin.Toggle.On) builder.Add("$label_prefabname", m_piece.name);
            builder.Add("$label_enabled", m_piece.enabled);
            builder.Add("$label_piececategory", m_piece.m_category);
            builder.Add("$label_isupgrade", m_piece.m_isUpgrade);
            builder.Add("$label_comfort", m_piece.m_comfort);
            builder.Add("$label_comfortgroup", m_piece.m_comfortGroup);
            builder.Add("$label_groundpiece", m_piece.m_groundPiece);
            builder.Add("$label_allowaltground", m_piece.m_allowAltGroundPlacement);
            builder.Add("$label_groundonly", m_piece.m_groundOnly);
            builder.Add("$label_cultivatedgroundonly", m_piece.m_cultivatedGroundOnly);
            builder.Add("$label_waterpiece", m_piece.m_waterPiece);
            builder.Add("$label_clipground", m_piece.m_clipGround);
            builder.Add("$label_clipeverything", m_piece.m_clipEverything);
            builder.Add("$label_noinwater", m_piece.m_noInWater);
            builder.Add("$label_noonwood", m_piece.m_notOnWood);
            builder.Add("$label_noontilt", m_piece.m_notOnTiltingSurface);
            builder.Add("$label_inceilingonly", m_piece.m_inCeilingOnly);
            builder.Add("$label_noonfloor", m_piece.m_notOnFloor);
            builder.Add("$label_noclipping", m_piece.m_noClipping);
            builder.Add("$label_onlyinteleportarea", m_piece.m_onlyInTeleportArea);
            builder.Add("$label_allowedindungeon", m_piece.m_allowedInDungeons);
            builder.Add("$label_spacerequirement", m_piece.m_spaceRequirement);
            builder.Add("$label_repairpiece", m_piece.m_repairPiece);
            builder.Add("$label_canrotate", m_piece.m_canRotate);
            builder.Add("$label_randominitbuildrotation", m_piece.m_randomInitBuildRotation);
            builder.Add("$label_allowrotateoverlap", m_piece.m_allowRotatedOverlap);
            builder.Add("$label_vegetationgroundonly", m_piece.m_vegetationGroundOnly);
            if (m_piece.m_blockingPieces.Count > 0)
            {
                builder.Add("$title_blockingpieces");
                foreach (var piece in m_piece.m_blockingPieces) builder.Add("$label_piecename", piece.m_name);
            }
            builder.Add("$label_blockradius", m_piece.m_blockRadius);
            builder.Add("$label_connectradius", m_piece.m_connectRadius);
            builder.Add("$label_mustbeaboveconnected", m_piece.m_mustBeAboveConnected);
            builder.Add("$label_onlyinbiome", m_piece.m_onlyInBiome);
            builder.Add("$label_dlc", m_piece.m_dlc);
            builder.Add("$title_craftingstation", m_piece.m_craftingStation);
            if (m_prefab.TryGetComponent(out CraftingStation craftingStation))
            {
                builder.Add("$title_craftingstation");
                builder.Add("$label_discoverrange", craftingStation.m_discoverRange);
                builder.Add("$label_rangebuild", craftingStation.m_rangeBuild, craftingStation.m_extraRangePerLevel, "/lvl");
                builder.Add("$label_requireroof", craftingStation.m_craftRequireRoof);
                builder.Add("$label_requirefire", craftingStation.m_craftRequireFire);
                builder.Add("$label_showbasicrecipes", craftingStation.m_showBasicRecipies);
                builder.Add("$label_usedistance", craftingStation.m_useDistance);
                builder.Add("$label_useanimation", craftingStation.m_useAnimation);
            }

            if (m_prefab.TryGetComponent(out StationExtension stationExtension))
            {
                builder.Add("$title_stationextension");
                builder.Add("$title_craftingstation", stationExtension.m_craftingStation);
                builder.Add("$label_maxstationdistance", stationExtension.m_maxStationDistance);
                builder.Add("$item_stack", stationExtension.m_stack);
                builder.Add("$label_continousconnection", stationExtension.m_continousConnection);
            }

            if (m_prefab.TryGetComponent(out WearNTear wearNTear))
            {
                builder.Add("$title_wearntear");
                builder.Add("$label_noroofwear", wearNTear.m_noRoofWear);
                builder.Add("$label_materialtype", wearNTear.m_materialType);
                builder.Add("$label_supports", wearNTear.m_supports);
                builder.Add("$se_health", wearNTear.m_health);
                builder.Add("$inventory_blunt", wearNTear.m_damages.m_blunt);
                builder.Add("$inventory_slash", wearNTear.m_damages.m_slash);
                builder.Add("$inventory_pierce", wearNTear.m_damages.m_pierce);
                builder.Add("$inventory_chop", wearNTear.m_damages.m_chop);
                builder.Add("$inventory_pickaxe", wearNTear.m_damages.m_pickaxe);
                builder.Add("$inventory_fire", wearNTear.m_damages.m_fire);
                builder.Add("$inventory_frost", wearNTear.m_damages.m_frost);
                builder.Add("$inventory_lightning", wearNTear.m_damages.m_lightning);
                builder.Add("$inventory_poison", wearNTear.m_damages.m_poison);
                builder.Add("$inventory_spirit", wearNTear.m_damages.m_spirit);
                builder.Add("$label_hitnoise", wearNTear.m_hitNoise);
                builder.Add("$label_destroynoise", wearNTear.m_destroyNoise);
                builder.Add("$label_triggerprivatearea", wearNTear.m_triggerPrivateArea);
            }
            if (m_prefab.TryGetComponent(out Smelter smelter))
            {
                builder.Add("$title_smelter");
                builder.Add("$label_fuelitem", smelter.m_fuelItem);
                builder.Add("$label_maxore", smelter.m_maxOre);
                builder.Add("$label_maxfuel", smelter.m_maxFuel);
                builder.Add("$label_fuelperproduct", smelter.m_fuelPerProduct);
                builder.Add("$label_secperproduct", smelter.m_secPerProduct, EntryBuilder.Option.Seconds);
                builder.Add("$label_spawnstack", smelter.m_spawnStack);
                builder.Add("$label_requireroof", smelter.m_requiresRoof);
                builder.Add("$label_addoreanimationduration", smelter.m_addOreAnimationDuration);
                if (smelter.m_conversion.Count > 0)
                {
                    builder.Add("$title_conversion");
                    foreach (var conversion in smelter.m_conversion)
                    {
                        builder.Add(conversion.m_from, conversion.m_to);
                    }
                }
            }
            if (m_prefab.TryGetComponent(out Beehive beehive))
            {
                builder.Add("$title_beehive");
                builder.Add("$label_effectonlyday", beehive.m_effectOnlyInDaylight);
                builder.Add("$label_maxcover", beehive.m_maxCover);
                builder.Add("$label_secperunit", beehive.m_secPerUnit, EntryBuilder.Option.Seconds);
                builder.Add("$label_maxstorage", beehive.m_maxHoney);
                builder.Add("$label_item", beehive.m_honeyItem);
                builder.Add("$label_biome", beehive.m_biome);
            }
            
            if (m_prefab.TryGetComponent(out Container container))
            {
                builder.Add("$title_container");
                builder.Add("$label_size", container.m_width, container.m_height, "x");
                builder.Add("$label_privacy", container.m_privacy);
                builder.Add("$label_checkguardstone", container.m_checkGuardStone);
                builder.Add("$label_autodestroyempty", container.m_autoDestroyEmpty);
                if (container.m_defaultItems.m_drops.Count > 0)
                {
                    builder.Add("$label_dropminmax", container.m_defaultItems.m_dropMin, container.m_defaultItems.m_dropMax, "-");
                    builder.Add("$label_dropchance", container.m_defaultItems.m_dropChance, EntryBuilder.Option.Percentage);
                    builder.Add("$label_oneofeach", container.m_defaultItems.m_oneOfEach);
                    float totalWeight = container.m_defaultItems.m_drops.Sum(item => item.m_weight);
                    foreach (var drop in container.m_defaultItems.m_drops)
                    {
                        if (drop.m_item == null || !drop.m_item.TryGetComponent(out ItemDrop itemDrop)) continue;
                        builder.Add(itemDrop.m_itemData.m_shared.m_name, drop.m_weight / totalWeight, EntryBuilder.Option.Percentage);
                    }
                }
            }
            
            if (m_prefab.TryGetComponent(out CookingStation cookingStation))
            {
                builder.Add("$title_cookingstation");
                builder.Add("$label_spawnforce", cookingStation.m_spawnForce);
                builder.Add("$label_overcookeditem", cookingStation.m_overCookedItem);
                builder.Add("$label_requirefire", cookingStation.m_requireFire);
                builder.Add("$label_firecheckradius", cookingStation.m_fireCheckRadius);
                builder.Add("$label_usefuel", cookingStation.m_useFuel);
                builder.Add("$label_fuelitem", cookingStation.m_fuelItem);
                builder.Add("$label_maxfuel", cookingStation.m_maxFuel);
                builder.Add("$label_secperfuel", cookingStation.m_secPerFuel);
                if (cookingStation.m_conversion.Count > 0)
                {
                    builder.Add("$title_conversion");
                    foreach (var conversion in cookingStation.m_conversion)
                    {
                        builder.Add(conversion.m_from, conversion.m_to);
                    }
                }
            }

            if (m_prefab.TryGetComponent(out TeleportWorld teleportWorld))
            {
                builder.Add("$title_portal");
                builder.Add("$label_activationrange", teleportWorld.m_activationRange);
                builder.Add("$label_exitdistance", teleportWorld.m_exitDistance);
            }

            if (m_prefab.TryGetComponent(out Ship ship))
            {
                builder.Add("$label_ship");
                builder.Add("$label_waterleveloffset", ship.m_waterLevelOffset);
                builder.Add("$label_forcedistance", ship.m_forceDistance);
                builder.Add("$label_force", ship.m_force);
                builder.Add("$label_damping", ship.m_damping);
                builder.Add("$label_dampingsideway", ship.m_dampingSideway);
                builder.Add("$label_dampingforward", ship.m_dampingForward);
                builder.Add("$label_angulardamping", ship.m_angularDamping);
                builder.Add("$label_disablelevel", ship.m_disableLevel);
                builder.Add("$label_sailforceoffset", ship.m_sailForceOffset);
                builder.Add("$label_sailforcefactor", ship.m_sailForceFactor);
                builder.Add("$label_rudderspeed", ship.m_rudderSpeed);
                builder.Add("$label_stearforceoffset", ship.m_stearForceOffset);
                builder.Add("$label_stearforce", ship.m_stearForce);
                builder.Add("$label_stearvelforcefactor", ship.m_stearVelForceFactor);
                builder.Add("$label_backwardforce", ship.m_backwardForce);
                builder.Add("$label_rudderrotationmax", ship.m_rudderRotationMax);
                builder.Add("$label_minwaterimpactforce", ship.m_minWaterImpactForce);
                builder.Add("$label_minwaterimpactinterval", ship.m_minWaterImpactInterval, EntryBuilder.Option.Seconds);
                builder.Add("$label_upsidedowndmginterval", ship.m_upsideDownDmgInterval, EntryBuilder.Option.Seconds);
                builder.Add("$label_upsidedowndmg", ship.m_upsideDownDmg);
            }
            if (m_prefab.TryGetComponent(out WispSpawner wispSpawner))
            {
                builder.Add("$title_wispspawner");
                builder.Add("$label_spawninterval", wispSpawner.m_spawnInterval, EntryBuilder.Option.Seconds);
                builder.Add("$label_spawnchance", wispSpawner.m_spawnChance, EntryBuilder.Option.Percentage);
                builder.Add("$label_maxspawned", wispSpawner.m_maxSpawned);
                builder.Add("$label_onlyspawnatnight", ConvertBoolean(wispSpawner.m_onlySpawnAtNight));
                builder.Add("$label_dontspawnincover", ConvertBoolean(wispSpawner.m_dontSpawnInCover));
                builder.Add("$label_maxcover", wispSpawner.m_maxCover, EntryBuilder.Option.Percentage);
                builder.Add("$label_prefabname", wispSpawner.m_wispPrefab);
                builder.Add("$label_nearbythreshold", wispSpawner.m_nearbyTreshold);
                builder.Add("$label_spawndistance", wispSpawner.m_spawnDistance);
                builder.Add("$label_maxspawnedarea", wispSpawner.m_maxSpawnedArea);
            }
            if (m_prefab.TryGetComponent(out Trap trap))
            {
                builder.Add("$title_trap");
                builder.Add("$label_rearmcooldown", trap.m_rearmCooldown, EntryBuilder.Option.Seconds);
                builder.Add("$label_triggeredbyenemies", ConvertBoolean(trap.m_triggeredByEnemies));
                builder.Add("$label_triggeredbyplayers", ConvertBoolean(trap.m_triggeredByPlayers));
                builder.Add("$label_forcestagger", ConvertBoolean(trap.m_forceStagger));
                builder.Add("$label_startsarmed", ConvertBoolean(trap.m_startsArmed));
            }
            if (m_prefab.TryGetComponent(out Fireplace fireplace))
            {
                builder.Add("$title_fireplace");
                builder.Add("$label_startfuel", fireplace.m_startFuel);
                builder.Add("$label_maxfuel", fireplace.m_maxFuel);
                builder.Add("$label_secperfuel", fireplace.m_secPerFuel, EntryBuilder.Option.Seconds);
                builder.Add("$label_infinitefuel", ConvertBoolean(fireplace.m_infiniteFuel));
                builder.Add("$label_fuelitem", fireplace.m_fuelItem);
                if (fireplace.m_fireworkItemList.Length > 0)
                {
                    builder.Add("$title_fireworks");
                    foreach (var firework in fireplace.m_fireworkItemList)
                    {
                        builder.Add("$label_item", firework.m_fireworkItem);
                    }
                }
            }
            
            if (m_prefab.TryGetComponent(out Door door))
            {
                builder.Add("$title_door");
                builder.Add("$label_keyitem", door.m_keyItem);
                builder.Add("$label_cannotbeclosed", door.m_canNotBeClosed);
                builder.Add("$label_checkguardstone", door.m_checkGuardStone);
            }
            if (m_prefab.TryGetComponent(out Turret turret))
            {
                builder.Add("$title_turret");
                builder.Add("$label_turnrate", turret.m_turnRate, EntryBuilder.Option.Seconds);
                builder.Add("$label_horizontalangle", turret.m_horizontalAngle, EntryBuilder.Option.Degree);
                builder.Add("$label_verticalangle", turret.m_verticalAngle, EntryBuilder.Option.Degree);
                builder.Add("$label_viewdistance", turret.m_viewDistance);
                builder.Add("$label_notargetscanrate", turret.m_noTargetScanRate, EntryBuilder.Option.Seconds);
                builder.Add("$label_lookacceleration", turret.m_lookAcceleration, EntryBuilder.Option.Seconds);
                builder.Add("$label_lookdeacceleration", turret.m_lookDeacceleration, EntryBuilder.Option.Seconds);
                builder.Add("$label_lookmindegreedelta", turret.m_lookMinDegreesDelta, EntryBuilder.Option.Degree);
                builder.Add("$label_defaultammo", turret.m_defaultAmmo);
                builder.Add("$label_attackcooldown", turret.m_attackCooldown, EntryBuilder.Option.Seconds);
                builder.Add("$label_attackwarmup", turret.m_attackWarmup, EntryBuilder.Option.Seconds);
                builder.Add("$label_hitnoise", turret.m_hitNoise);
                builder.Add("$label_shootwhenaimdiff", turret.m_shootWhenAimDiff);
                builder.Add("$label_predictionmodifier", turret.m_predictionModifier);
                builder.Add("$label_updatetargetintervalfar", turret.m_updateTargetIntervalFar, EntryBuilder.Option.Seconds);
                builder.Add("$label_updatetargetintervalnear", turret.m_updateTargetIntervalNear, EntryBuilder.Option.Seconds);
                builder.Add("$label_maxammo", turret.m_maxAmmo);
                builder.Add("$label_ammotype", turret.m_ammoType);
                builder.Add("$label_returnammoondestroy", turret.m_returnAmmoOnDestroy);
                builder.Add("$label_holdrepeatinterval", turret.m_holdRepeatInterval, EntryBuilder.Option.Seconds);
                builder.Add("$label_targetplayers", turret.m_targetPlayers);
                builder.Add("$label_targettamed", turret.m_targetTamed);
                if (turret.m_allowedAmmo.Count > 0)
                {
                    builder.Add("$title_allowedammo");
                    foreach (var ammo in turret.m_allowedAmmo)
                    {
                        builder.Add("$label_ammotype", ammo);
                    }
                }
            }

            if (m_prefab.TryGetComponent(out Fermenter ferment))
            {
                builder.Add("$title_fermenter");
                builder.Add("$label_duration", ferment.m_fermentationDuration, EntryBuilder.Option.Seconds);
                builder.Add("$label_tapdelay", ferment.m_tapDelay, EntryBuilder.Option.Seconds);
                if (ferment.m_conversion.Count > 0)
                {
                    builder.Add("$title_conversion");
                    foreach (var conversion in ferment.m_conversion)
                    {
                        builder.Add(conversion.m_to, conversion.m_from);
                    }
                }
            }

            if (m_prefab.TryGetComponent(out Plant plant))
            {
                builder.Add("$title_plant");
                builder.Add("$label_growtime", plant.m_growTime, EntryBuilder.Option.Seconds);
                builder.Add("$label_growtimemax", plant.m_growTimeMax, EntryBuilder.Option.Seconds);
                builder.Add("$label_minscale", plant.m_minScale);
                builder.Add("$label_maxscale", plant.m_maxScale);
                builder.Add("$label_growradius", plant.m_growRadius);
                builder.Add("$label_cultivatedgroundonly", plant.m_needCultivatedGround);
                builder.Add("$label_destroyifcantgrow", plant.m_destroyIfCantGrow);
                builder.Add("$label_biome", plant.m_biome);
                if (plant.m_grownPrefabs.Length > 0)
                {
                    builder.Add("$title_grownprefabs");
                    foreach (var prefab in plant.m_grownPrefabs)
                    {
                        builder.Add("$label_prefabname", prefab);
                    }
                }
            }

            return builder.ToList();
        }
    }

    public static readonly Dictionary<string, string> m_localizeOverride = new()
    {
        ["TreasureChest_blackforest"] = "$treasure_blackforest",
        ["TreasureChest_ashland_stone"] = "$treasure_ashland_stone",
        ["TreasureChest_charredfortress"] = "$treasure_charred_fortress",
        ["TreasureChest_dvergr_loose_stone"] = "$treasure_dvergr_loose_stone",
        ["TreasureChest_dvergrtower"] = "$treasure_dvergr_tower",
        ["TreasureChest_dvergrtown"] = "$treasure_dvergr_town",
        ["TreasureChest_fCrypt"] = "$treasure_forest_crypt",
        ["TreasureChest_forestcrypt"] = "$treasure_forest_crypt",
        ["TreasureChest_forestcrypt_hildir"] = "$treasure_forest_crypt_hildir",
        ["TreasureChest_heath"] = "$treasure_plains",
        ["TreasureChest_heath_hildir"] = "$treasure_plains_hildir",
        ["TreasureChest_meadows"] = "$treasure_meadows",
        ["TreasureChest_meadows_buried"] = "$treasure_meadows_buried",
        ["TreasureChest_mountaincave"] = "$treasure_mountain_cave",
        ["TreasureChest_mountaincave_hildir"] = "$treasure_mountain_cave_hildir",
        ["TreasureChest_mountains"] = "$treasure_mountains",
        ["TreasureChest_plains_stone"] = "$treasure_plains_stone",
        ["TreasureChest_plainsfortress_hildir"] = "$treasure_plains_fortress_hildir",
        ["TreasureChest_sunkencrypt"] = "$treasure_swamp_crypt",
        ["TreasureChest_swamp"] = "$treasure_swamp",
        ["TreasureChest_trollcave"] = "$treasure_troll_cave",
        ["loot_chest_stone"] = "$treasure_chest_stone",
        ["loot_chest_wood"] = "$treasure_chest_wood"
        
    };

    private static readonly Dictionary<string, Piece.PieceCategory> m_specialCases = new()
    {
        ["kg_EnchantmentScrollStation"] = Piece.PieceCategory.Crafting,
    };

    private static readonly List<string> m_defaultPrefabNames = new()
    {
        "ship_construction",
        "paved_road_v2",
        "paved_road",
        "cultivate_v2",
        "cultivate",
        "mud_road",
        "mud_road_v2",
        "path_v2",
        "path",
        "replant_v2",
        "replant",
        "raise_v2",
        "raise",
        "fire_pit_haldor",
        "fire_pit_hildir",
        "dverger_guardstone",
        "guard_stone_test",
        "ML_TreasureChestOcean"
    };

    private static readonly List<string> m_krumpMisc = new()
    {
        "Krump_Spawner_Treasure_BlackForest_Crypt",
        "Krump_Spawner_Treasure_BlackForest_Crypt",
        "Krump_Spawner_Treasure_Mountain_Cave",
        "Krump_Spawner_Treasure_Meadows_Dungeon",
        "Krump_Spawner_Treasure_Swamps",
        "Krump_Spawner_Treasure_BlackForest",
        "Krump_Spawner_Treasure_Meadows",
        "Krump_Ship_Raft",
        "Krump_Ship_Karve",
        "Krump_Ship_Knarr_Transporter",
    };

    private static readonly List<string> m_krumpCrafting = new()
    {
        "D_Alchemy_Cauldron",
        "D_Alchemy_Table",
        "D_Seed_Table",
        "D_Alchemy_Library",
        "D_Water_Catcher",
        "D_Roasting_Spit",
        "D_Preparation_Table",
        "D_Beehive",
        "D_Mortar_and_Pestle",
        "D_Stone_Griddle",
        "D_Big_Stone_Griddle",
        "D_Honey_Extractor",
        "D_Butcher_Table",
        "D_Oven",
        "D_Beverage_Station",
        "D_Butcher_Tools",
        "D_Cutting_Table",
        "D_ABronze_Caouldron",
        "D_Alchemy_Altar",
        "D_Chicken_Coop",
        "D_Book_Stand",
        "D_Bronze_Caouldron",
        "D_Bronze_HangCaouldron",
        "Krump_CS_Shipyard",
        "Krump_CS_Fishing_Trap",
        "Krump_CS_Crab_Trap",
        "Krump_CS_Fermenter_Oil",
        "Krump_CS_Shipyard_Crane",
        "Krump_Spawner_Thrall_Trader",
        "Krump_CS_Shipyard_Ship_Construction",
        "Krump_CS_Shipyard_Horn",
    };
}