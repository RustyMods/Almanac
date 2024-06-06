using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Almanac.Data;
using BepInEx;
using UnityEngine;
using static Almanac.AlmanacPlugin;
using static Almanac.Data.PlayerStats;
using static Almanac.Utilities.Utility;
using Utility = Almanac.Utilities.Utility;

namespace Almanac.UI;

public static class Entries
{
    public static List<Entry> GetPieceEntries(GameObject prefab)
    {
        List<Entry> output = new();
        if (prefab.TryGetComponent(out Piece piece))
        {
            output.Add(new Entry() { title = "$almanac_enabled", value = ConvertBoolean(piece.enabled) });
            output.Add(new Entry() { title = "$almanac_piece_category", value = SplitCamelCase(piece.m_category.ToString()) });
            output.Add(new Entry() { title = "$almanac_is_upgrade", value = ConvertBoolean(piece.m_isUpgrade) });
            output.Add(new Entry() { title = "$almanac_comfort", value = piece.m_comfort.ToString() });
            output.Add(new Entry() { title = "$almanac_comfort_group", value = SplitCamelCase(piece.m_comfortGroup.ToString()) });
            output.Add(new Entry() { title = "$almanac_comfort_object", value = piece.m_comfortObject ? piece.m_comfortObject.name : "0" });
            output.Add(new Entry() { title = "$almanac_ground_piece", value = ConvertBoolean(piece.m_groundPiece) });
            output.Add(new Entry() { title = "$almanac_allow_alt_ground", value = ConvertBoolean(piece.m_allowAltGroundPlacement) });
            output.Add(new Entry() { title = "$almanac_ground_only", value = ConvertBoolean(piece.m_groundOnly) });
            output.Add(new Entry() { title = "$almanac_cultivated_only", value = ConvertBoolean(piece.m_cultivatedGroundOnly) });
            output.Add(new Entry() { title = "$almanac_water_piece", value = ConvertBoolean(piece.m_waterPiece) });
            output.Add(new Entry() { title = "$almanac_clip_ground", value = ConvertBoolean(piece.m_clipGround) });
            output.Add(new Entry() { title = "$almanac_clip_everything", value = ConvertBoolean(piece.m_clipEverything) });
            output.Add(new Entry() { title = "$almanac_no_in_water", value = ConvertBoolean(piece.m_noInWater) });
            output.Add(new Entry() { title = "$almanac_no_on_wood", value = ConvertBoolean(piece.m_notOnWood) });
            output.Add(new Entry() { title = "$almanac_no_on_tilt", value = ConvertBoolean(piece.m_notOnTiltingSurface) });
            output.Add(new Entry() { title = "$almanac_ceiling_only", value = ConvertBoolean(piece.m_inCeilingOnly) });
            output.Add(new Entry() { title = "$almanac_no_on_floor", value = ConvertBoolean(piece.m_notOnFloor) });
            output.Add(new Entry() { title = "$almanac_no_clipping", value = ConvertBoolean(piece.m_notOnFloor) });
            output.Add(new Entry() { title = "$almanac_only_in_teleport_area", value = ConvertBoolean(piece.m_onlyInTeleportArea) });
            output.Add(new Entry() { title = "$almanac_allow_dungeon", value = ConvertBoolean(piece.m_allowedInDungeons) });
            output.Add(new Entry() { title = "$almanac_space_req", value = piece.m_spaceRequirement.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_repair_piece", value = ConvertBoolean(piece.m_repairPiece) });
            output.Add(new Entry() { title = "$almanac_can_rotate", value = ConvertBoolean(piece.m_canRotate) });
            output.Add(new Entry() { title = "$almanac_random_rotate", value = ConvertBoolean(piece.m_randomInitBuildRotation) });
            output.Add(new Entry() { title = "$almanac_allow_rotate_overlap", value = ConvertBoolean(piece.m_allowRotatedOverlap) });
            output.Add(new Entry() { title = "$almanac_vegetation_ground_only", value = ConvertBoolean(piece.m_vegetationGroundOnly) });
            if (_ShowAllData.Value is Toggle.On) output.Add(new(){title = "$almanac_prefab_name", value = piece.name});
            if (piece.m_blockingPieces.Count > 0)
            {
                output.AddRange(piece.m_blockingPieces.Select(block => new Entry() { title = "$almanac_block_piece", value = block.name }));
            }
            output.Add(new Entry() { title = "$almanac_block_radius", value = piece.m_blockRadius.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_must_connect_to", value = piece.m_mustConnectTo ? piece.m_mustConnectTo.ToString() : "0" });
            output.Add(new Entry() { title = "$almanac_connect_radius", value = piece.m_connectRadius.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_must_be_above", value = ConvertBoolean(piece.m_mustBeAboveConnected) });
            output.Add(new Entry() { title = "$almanac_piece_biome", value = SplitCamelCase(piece.m_onlyInBiome.ToString()) });
            output.Add(new Entry() { title = "$almanac_dlc", value = piece.m_dlc });
            output.Add(new Entry() { title = "$almanac_craft_station", value = piece.m_craftingStation ? Localization.instance.Localize(piece.m_craftingStation.m_name) : "0" });
        }

        if (prefab.TryGetComponent(out CraftingStation craftingStation))
        {
            output.Add(new Entry() { title = "$almanac_crafting_station_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_discover_range", value = craftingStation.m_discoverRange.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_range_build", value = craftingStation.m_rangeBuild.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_extra_range_per_level", value = craftingStation.m_extraRangePerLevel.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_require_roof1", value = ConvertBoolean(craftingStation.m_craftRequireRoof) });
            output.Add(new Entry() { title = "$almanac_require_fire", value = ConvertBoolean(craftingStation.m_craftRequireFire) });
            output.Add(new Entry() { title = "$almanac_show_basic_recipes", value = ConvertBoolean(craftingStation.m_showBasicRecipies) });
            output.Add(new Entry() { title = "$almanac_use_distance", value = craftingStation.m_useDistance.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_use_animation", value = craftingStation.m_useAnimation.ToString() });
        }

        if (prefab.TryGetComponent(out StationExtension stationExtension))
        {
            output.Add(new Entry() { title = "$almanac_extension_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_piece_extends", value = stationExtension.m_craftingStation ? Localization.instance.Localize(stationExtension.m_craftingStation.m_name) : "0" });
            output.Add(new Entry() { title = "$almanac_extension_distance", value = stationExtension.m_maxStationDistance.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_extension_stack", value = stationExtension.m_stack.ToString() });
            output.Add(new Entry() { title = "$almanac_continuous_connection", value = ConvertBoolean(stationExtension.m_continousConnection) });
        }

        if (prefab.TryGetComponent(out WearNTear wearNTear))
        {
            output.Add(new Entry() { title = "$almanac_wear_tear_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_no_roof_wear", value = ConvertBoolean(wearNTear.m_noRoofWear) });
            output.Add(new Entry() { title = "$almanac_no_support_wear", value = ConvertBoolean(wearNTear.m_noSupportWear) });
            output.Add(new Entry() { title = "$almanac_material_type", value = SplitCamelCase(wearNTear.m_materialType.ToString()) });
            output.Add(new Entry() { title = "$almanac_piece_supports", value = ConvertBoolean(wearNTear.m_supports) });
            output.Add(new Entry() { title = "$almanac_support_value", value = wearNTear.m_support.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_piece_health", value = wearNTear.m_health.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_blunt", value = ConvertDamageModifiers(wearNTear.m_damages.m_blunt) });
            output.Add(new Entry() { title = "$almanac_slash", value = ConvertDamageModifiers(wearNTear.m_damages.m_slash) });
            output.Add(new Entry() { title = "$almanac_pierce", value = ConvertDamageModifiers(wearNTear.m_damages.m_pierce) });
            output.Add(new Entry() { title = "$almanac_chop", value = ConvertDamageModifiers(wearNTear.m_damages.m_chop) });
            output.Add(new Entry() { title = "$almanac_pickaxe", value = ConvertDamageModifiers(wearNTear.m_damages.m_pickaxe) });
            output.Add(new Entry() { title = "$almanac_fire", value = ConvertDamageModifiers(wearNTear.m_damages.m_fire) });
            output.Add(new Entry() { title = "$almanac_frost", value = ConvertDamageModifiers(wearNTear.m_damages.m_frost) });
            output.Add(new Entry() { title = "$almanac_lightning", value = ConvertDamageModifiers(wearNTear.m_damages.m_lightning) });
            output.Add(new Entry() { title = "$almanac_poison", value = ConvertDamageModifiers(wearNTear.m_damages.m_poison) });
            output.Add(new Entry() { title = "$almanac_spirit", value = ConvertDamageModifiers(wearNTear.m_damages.m_spirit) });
            output.Add(new Entry() { title = "$almanac_hit_noise", value = wearNTear.m_hitNoise.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_destroy_noise", value = wearNTear.m_destroyNoise.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_trigger_private_area", value = ConvertBoolean(wearNTear.m_triggerPrivateArea) });
        }

        if (prefab.TryGetComponent(out Smelter smelter))
        {
            output.Add(new Entry() { title = "$almanac_smelter_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_fuel_item", value = smelter.m_fuelItem ? Localization.instance.Localize(smelter.m_fuelItem.m_itemData.m_shared.m_name) : "0" });
            output.Add(new Entry() { title = "$almanac_max_ore", value = smelter.m_maxOre.ToString() });
            output.Add(new Entry() { title = "$almanac_max_fuel", value = smelter.m_maxFuel.ToString() });
            output.Add(new Entry() { title = "$almanac_fuel_per_product", value = smelter.m_fuelPerProduct.ToString() });
            output.Add(new Entry() { title = "$almanac_sec_per_product", value = smelter.m_secPerProduct.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_spawn_stack", value = smelter.m_spawnStack.ToString() });
            output.Add(new Entry() { title = "$almanac_require_roof2", value = ConvertBoolean(smelter.m_requiresRoof) });
            output.Add(new Entry() { title = "$almanac_add_ore_duration", value = smelter.m_addOreAnimationDuration.ToString(CultureInfo.CurrentCulture) });
            if (smelter.m_conversion.Count > 0)
            {
                output.Add(new(){title = "$almanac_conversion_title", value = "title"});
                output.AddRange(smelter.m_conversion.Select(conversion => new Entry() { title = $"<color=white>{conversion.m_from.m_itemData.m_shared.m_name}</color>", value = conversion.m_to.m_itemData.m_shared.m_name }));
            }
        }

        if (prefab.TryGetComponent(out Beehive beehive))
        {
            output.Add(new Entry() { title = "$almanac_beehive_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_effect_only_day", value = beehive.m_effectOnlyInDaylight.ToString() });
            output.Add(new Entry() { title = "$almanac_max_cover", value = beehive.m_maxCover.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_sec_per_unit", value = beehive.m_secPerUnit.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_max_honey", value = beehive.m_maxHoney.ToString() });
            output.Add(new Entry() { title = "$almanac_honey_item", value = beehive.m_honeyItem ? Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name) : "0" });
            output.AddRange(beehive.m_biome.ToString().Split(',').Select(t => new Entry() { title = "$almanac_biomes", value = SplitCamelCase(t.Replace(" ", "")) }));
        }

        if (prefab.TryGetComponent(out Container container))
        {
            output.Add(new Entry() { title = "$almanac_container_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_container_size", value = container.m_width + "<color=orange>x</color>" + container.m_height });
            output.Add(new Entry() { title = "$almanac_container_privacy", value = container.m_privacy.ToString() });
            output.Add(new Entry() { title = "$almanac_check_guard", value = ConvertBoolean(container.m_checkGuardStone) });
            output.Add(new Entry() { title = "$almanac_auto_destroy_empty", value = ConvertBoolean(container.m_autoDestroyEmpty) });
            if (container.m_defaultItems.m_drops.Count > 0)
            {
                output.Add(new(){title = "$almanac_container_drop", value = container.m_defaultItems.m_dropMin + "<color=orange>-</color>" + container.m_defaultItems.m_dropMax});
                output.Add(new(){title = "$almanac_container_drop_chance", value = (container.m_defaultItems.m_dropChance * 100).ToString("0.0") + "%"});
                output.Add(new(){title = "$almanac_container_one_of_each", value = container.m_defaultItems.m_oneOfEach.ToString()});
                
                float totalWeight = container.m_defaultItems.m_drops.Sum(item => item.m_weight);;
                foreach (var drop in container.m_defaultItems.m_drops)
                {
                    if (!drop.m_item) continue;
                    if (!drop.m_item.TryGetComponent(out ItemDrop itemDrop)) continue;
                    string name = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name);
                    string stack = $"<color=orange>(</color>{drop.m_stackMin}<color=orange> $almanac_to </color>{drop.m_stackMax}<color=orange>)</color> ";
                    float percentage = (drop.m_weight / totalWeight) * 100;
                    output.Add(new(){title = name, value = stack + percentage.ToString("0.0") + "<color=orange>%</color>"});
                }
            }
        }

        if (prefab.TryGetComponent(out CookingStation cookingStation))
        {
            output.Add(new Entry() { title = "$almanac_spawn_force", value = cookingStation.m_spawnForce.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_overcooked_item", value = cookingStation.m_overCookedItem ? Localization.instance.Localize(cookingStation.m_overCookedItem.m_itemData.m_shared.m_name) : "0" });
            output.Add(new Entry() { title = "$almanac_require_fire", value = ConvertBoolean(cookingStation.m_requireFire) });
            output.Add(new Entry() { title = "$almanac_check_radius", value = cookingStation.m_fireCheckRadius.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_use_fuel", value = ConvertBoolean(cookingStation.m_useFuel) });
            output.Add(new Entry() { title = "$almanac_fuel_item", value = cookingStation.m_fuelItem ? Localization.instance.Localize(cookingStation.m_fuelItem.m_itemData.m_shared.m_name) : "0" });
            output.Add(new Entry() { title = "$almanac_max_fuel", value = cookingStation.m_maxFuel.ToString() });
            output.Add(new Entry() { title = "$almanac_sec_per_fuel", value = cookingStation.m_secPerFuel.ToString() });
            if (cookingStation.m_conversion.Count > 0)
            {
                output.Add(new(){title = "$almanac_conversion_title", value = "title"});
                output.AddRange(cookingStation.m_conversion.Select(item => new Entry() { title = item.m_from.m_itemData.m_shared.m_name, value = $"{item.m_to.m_itemData.m_shared.m_name} <color=orange>(</color>{item.m_cookTime}<color=orange>s)</color>" }));
            }
        }

        if (prefab.TryGetComponent(out TeleportWorld teleportWorld))
        {
            output.Add(new Entry() { title = "$almanac_portal_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_activation_range", value = teleportWorld.m_activationRange.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_exit_distance", value = teleportWorld.m_exitDistance.ToString("0.0") });
        }

        if (prefab.TryGetComponent(out Ship ship))
        {
            output.Add(new Entry() { title = "$almanac_ship_title", value = "title" });
            output.Add(new Entry() { title = "$almanac_water_level_offset", value = ship.m_waterLevelOffset.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_force_distance", value = ship.m_forceDistance.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_force", value = ship.m_force.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_damping", value = ship.m_damping.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_damping_sideway", value = ship.m_dampingSideway.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_damping_forward", value = ship.m_dampingForward.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_angular_damping", value = ship.m_angularDamping.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_disable_level", value = ship.m_disableLevel.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_sail_force_offset", value = ship.m_sailForceOffset.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_sail_force_factor", value = ship.m_sailForceFactor.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_rudder_speed", value = ship.m_rudderSpeed.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_stear_force_offset", value = ship.m_stearForceOffset.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_stear_force", value = ship.m_stearForce.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_stear_velocity_force_factor", value = ship.m_stearVelForceFactor.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_backward_force", value = ship.m_backwardForce.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_rudder_rotation_max", value = ship.m_rudderRotationMax.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_min_water_impact_force", value = ship.m_minWaterImpactForce.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_min_water_impact_interval", value = ship.m_minWaterImpactInterval.ToString("0.0") + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_upside_down_damage_interval", value = ship.m_upsideDownDmgInterval.ToString("0.0") + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_upside_down_damage", value = ship.m_upsideDownDmg.ToString("0.0") });
        }

        if (prefab.TryGetComponent(out WispSpawner wispSpawner))
        {
            output.Add(new Entry() { title = "$almanac_spawn_interval", value = wispSpawner.m_spawnInterval.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_spawn_chance", value = (wispSpawner.m_spawnChance * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
            output.Add(new Entry() { title = "$almanac_max_spawned", value = wispSpawner.m_maxSpawned.ToString() });
            output.Add(new Entry() { title = "$almanac_only_spawn_night", value = ConvertBoolean(wispSpawner.m_onlySpawnAtNight) });
            output.Add(new Entry() { title = "$almanac_no_spawn_cover", value = ConvertBoolean(wispSpawner.m_dontSpawnInCover) });
            output.Add(new Entry() { title = "$almanac_max_cover", value = (wispSpawner.m_maxCover * 100).ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_wisp_prefab", value = wispSpawner.m_wispPrefab ? wispSpawner.m_wispPrefab.name : "0" });
            output.Add(new Entry() { title = "$almanac_nearby_threshold", value = wispSpawner.m_nearbyTreshold.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_spawn_distance", value = wispSpawner.m_spawnDistance.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_max_spawned_area", value = wispSpawner.m_maxSpawnedArea.ToString(CultureInfo.CurrentCulture) });
        }

        if (prefab.TryGetComponent(out Trap trap))
        {
            output.Add(new Entry() { title = "$almanac_rearm_cooldown", value = trap.m_rearmCooldown + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_triggered_by_enemies", value = ConvertBoolean(trap.m_triggeredByEnemies) });
            output.Add(new Entry() { title = "$almanac_triggered_by_players", value = ConvertBoolean(trap.m_triggeredByPlayers) });
            output.Add(new Entry() { title = "$almanac_force_stagger", value = ConvertBoolean(trap.m_forceStagger) });
            output.Add(new Entry() { title = "$almanac_starts_armed", value = ConvertBoolean(trap.m_startsArmed) });
        }

        if (prefab.TryGetComponent(out Fireplace fireplace))
        {
            output.Add(new Entry() { title = "$almanac_start_fuel", value = fireplace.m_startFuel.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_max_fuel", value = fireplace.m_maxFuel.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_sec_per_fuel", value = fireplace.m_secPerFuel.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_infinite_fuel", value = ConvertBoolean(fireplace.m_infiniteFuel) });
            output.Add(new Entry() { title = "$almanac_fuel_item", value = fireplace.m_fuelItem ? Localization.instance.Localize(fireplace.m_fuelItem.m_itemData.m_shared.m_name) : "0" });
            if (fireplace.m_fireworkItemList.Length > 0)
            {
                output.AddRange(from item in fireplace.m_fireworkItemList where item.m_fireworkItem select new Entry() { title = "$almanac_firework", value = item.m_fireworkItem.m_itemData.m_shared.m_name });
            }
        }

        if (prefab.TryGetComponent(out Door door))
        {
            output.Add(new Entry() { title = "$almanac_key_item", value = door.m_keyItem ? Localization.instance.Localize(door.m_keyItem.m_itemData.m_shared.m_name) : "0" });
            output.Add(new Entry() { title = "$almanac_can_not_be_closed", value = ConvertBoolean(door.m_canNotBeClosed) });
            output.Add(new Entry() { title = "$almanac_check_guard", value = ConvertBoolean(door.m_checkGuardStone) });

        }

        if (prefab.TryGetComponent(out Turret turret))
        {
            output.Add(new Entry() { title = "$almanac_turn_rate", value = turret.m_turnRate.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_horizontal_angle", value = turret.m_horizontalAngle.ToString(CultureInfo.CurrentCulture) + "<color=orange>°</color>" });
            output.Add(new Entry() { title = "$almanac_vertical_angle", value = turret.m_verticalAngle.ToString(CultureInfo.CurrentCulture) + "<color=orange>°</color>" });
            output.Add(new Entry() { title = "$almanac_view_distance", value = turret.m_viewDistance.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_no_target_scan_rate", value = turret.m_noTargetScanRate.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_look_acceleration", value = turret.m_lookAcceleration.ToString("0.0") + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_look_deacceleration", value = turret.m_lookDeacceleration.ToString("0.0") + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_look_min_degrees_delta", value = turret.m_lookMinDegreesDelta.ToString("0.0") + "<color=orange>°</color>" });
            output.Add(new Entry() { title = "$almanac_default_ammo", value = turret.m_defaultAmmo ? Localization.instance.Localize(turret.m_defaultAmmo.m_itemData.m_shared.m_name) : "0" });
            output.Add(new Entry() { title = "$almanac_attack_cooldown", value = turret.m_attackCooldown.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_attack_warmup", value = turret.m_attackWarmup.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_hit_noise1", value = turret.m_hitNoise.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_shoot_when_aim_diff", value = turret.m_shootWhenAimDiff.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_prediction_modifier", value = turret.m_predictionModifier.ToString("0.0") });
            output.Add(new Entry() { title = "$almanac_update_target_interval_far", value = turret.m_updateTargetIntervalFar.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_update_target_interval_near", value = turret.m_updateTargetIntervalNear.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_max_ammo", value = turret.m_maxAmmo.ToString() });
            output.Add(new Entry() { title = "$almanac_ammo_type", value = Localization.instance.Localize(turret.m_ammoType) });
            output.Add(new Entry() { title = "$almanac_return_ammo_on_destroy", value = ConvertBoolean(turret.m_returnAmmoOnDestroy) });
            output.Add(new Entry() { title = "$almanac_hold_repeat_interval", value = turret.m_holdRepeatInterval.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_target_players", value = ConvertBoolean(turret.m_targetPlayers) });
            output.Add(new Entry() { title = "$almanac_target_tamed", value = ConvertBoolean(turret.m_targetTamed) });
            if (turret.m_allowedAmmo.Count > 0)
            {
                output.AddRange(from ammo in turret.m_allowedAmmo where ammo.m_ammo select new Entry() { title = "$almanac_ammo", value = ammo.m_ammo.m_itemData.m_shared.m_name });
            }
        }

        if (prefab.TryGetComponent(out Fermenter ferment))
        {
            output.Add(new(){title = "$almanac_fermenter_title", value = "title"});
            output.Add(new Entry() { title = "$almanac_duration", value = ferment.m_fermentationDuration.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            output.Add(new Entry() { title = "$almanac_tap_delay", value = ferment.m_tapDelay.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
            if (ferment.m_conversion.Count > 0)
            {
                output.AddRange(ferment.m_conversion.Select(conversion => new Entry() { title = $"<color=white>{conversion.m_from.m_itemData.m_shared.m_name}</color>", value = $"{conversion.m_to.m_itemData.m_shared.m_name} <color=orange>(x</color> {conversion.m_producedItems}<color=orange>)</color>" }));
            }
        }
        
        return output;
    }
    
    public static List<Entry> GetItemEntries(ItemDrop.ItemData itemData, ItemDrop itemDrop)
    {
        List<Entry> output = new List<Entry>
        {
            new Entry() { title = "$almanac_stack_size_label", value = itemData.m_stack.ToString() },
            new Entry()
            {
                title = "$item_durability", value = itemData.m_durability.ToString(CultureInfo.CurrentCulture)
            },
            new Entry() { title = "$almanac_variant_label", value = itemData.m_variant.ToString() },
            new Entry() { title = "$almanac_world_level_label", value = itemData.m_worldLevel.ToString() },
            new Entry() { title = "$almanac_item_type_label", value = ConvertItemType(itemData.m_shared.m_itemType) },
            new Entry() { title = "$almanac_max_stack_size_label", value = itemData.m_shared.m_maxStackSize.ToString() },
            new Entry() { title = "$almanac_auto_stack_label", value = ConvertBoolean(itemData.m_shared.m_autoStack) },
            new Entry() { title = "$almanac_quality_label", value = itemData.m_shared.m_maxQuality.ToString() },
            new Entry() { title = "$almanac_scale_by_quality", value = (itemData.m_shared.m_scaleByQuality * 100).ToString(CultureInfo.CurrentCulture) + "%" },
            new Entry() { title = "$almanac_weight_label", value = itemData.m_shared.m_weight.ToString("0.0") },
            new Entry() { title = "$almanac_scale_by_weight", value = itemData.m_shared.m_scaleWeightByQuality.ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_value_label", value = itemData.m_shared.m_value.ToString() },
            new Entry() { title = "$almanac_teleportable", value = ConvertBoolean(itemData.m_shared.m_teleportable) },
            new Entry() { title = "$almanac_quest_item_label", value = ConvertBoolean(itemData.m_shared.m_questItem) },
            new Entry() { title = "$almanac_equip_duration", value = itemData.m_shared.m_equipDuration.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" },
            new Entry() { title = "$almanac_variant_label", value = itemData.m_shared.m_variants.ToString() }
        };

        GameObject prefab = ObjectDB.instance.GetItemPrefab(itemDrop.name);
        if (prefab != null)
        {
            output.Add(new(){title = "$almanac_floating", value = prefab.GetComponent<Floating>() ? "$almanac_true" : "$almanac_false"});
            if (_ShowAllData.Value is Toggle.On)
            {
                output.Add(new(){title = "$almanac_prefab_name", value = prefab.name});
            }
            Skills.SkillType[] allSkills = Skills.s_allSkills;
            switch (itemData.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Fish:
                    if (prefab.TryGetComponent(out Fish fish))
                    {
                        output.Add(new(){title = "$almanac_fish_title", value = "title"});
                        output.Add(new Entry() { title = "$almanac_fish_title", value = "title" });
                        output.Add(new Entry() { title = "$almanac_fish_swim_range", value = fish.m_swimRange.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_min_depth", value = fish.m_minDepth.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_speed", value = fish.m_speed.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_acceleration", value = fish.m_acceleration.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_turn_rate", value = fish.m_turnRate.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_avoid_range", value = fish.m_avoidRange.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_height", value = fish.m_height.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_hook_force", value = fish.m_hookForce.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_stamina_use", value = fish.m_staminaUse.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_escape_stamina_use", value = fish.m_escapeStaminaUse.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_escape_min", value = fish.m_escapeMin.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_escape_max", value = fish.m_escapeMax.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_base_hook_chance", value = fish.m_baseHookChance.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_escape_time", value = fish.m_escapeTime.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_jump_speed", value = fish.m_jumpSpeed.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_jump_height", value = fish.m_jumpHeight.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_jump_on_land_chance", value = fish.m_jumpOnLandChance.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_jump_on_land_decay", value = fish.m_jumpOnLandDecay.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$almanac_fish_jump_frequency", value = fish.m_jumpFrequencySeconds.ToString("0.0") + "<color=orange>s</color>" });
                        output.Add(new Entry() { title = "$almanac_fish_fast", value = ConvertBoolean(fish.m_fast) });

                        foreach (var bait in fish.m_baits)
                        {
                            output.Add(new(){title = "$almanac_fish_bait_name", value = bait.m_bait.m_itemData.m_shared.m_name});
                            output.Add(new(){title = "$almanac_fish_bait_chance", value = bait.m_chance.ToString(CultureInfo.CurrentCulture)});
                        }
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Customization:
                case ItemDrop.ItemData.ItemType.Shoulder:
                    output.Add(new Entry() { title = "$almanac_armor_title", value = "title" });
                    output.Add(new Entry() { title = "$almanac_armor_label", value = $"{itemData.m_shared.m_armor.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_armorPerLevel.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$almanac_armor_material", value = itemData.m_shared.m_armorMaterial && _ShowAllData.Value is Toggle.On ? RemoveParentheses(itemData.m_shared.m_armorMaterial.ToString()) : "0" });
                    output.Add(new Entry() { title = "$almanac_helmet_hide_hair", value = SplitCamelCase(itemData.m_shared.m_helmetHideHair.ToString()) });
                    output.Add(new Entry() { title = "$almanac_helmet_hide_beard", value = SplitCamelCase(itemData.m_shared.m_helmetHideBeard.ToString()) });
                    if (!itemData.m_shared.m_setName.IsNullOrWhiteSpace())
                    {
                        output.Add(new Entry() { title = "$almanac_set_title", value = "title" });
                        output.Add(new Entry() { title = "$almanac_set_name", value = itemData.m_shared.m_setName });
                        output.Add(new Entry() { title = "$almanac_set_size", value = itemData.m_shared.m_setSize.ToString() });
                        output.Add(new Entry() { title = "$almanac_set_status_effect", value = itemData.m_shared.m_setStatusEffect ? Localization.instance.Localize(itemData.m_shared.m_setStatusEffect.m_name) : "0" });
                        output.Add(new Entry() { title = "$almanac_set_tooltip", value = itemData.m_shared.m_setStatusEffect ? Localization.instance.Localize(itemData.m_shared.m_setStatusEffect.m_tooltip) : "0" });

                    }

                    if (itemData.m_shared.m_damageModifiers.Count > 0)
                    {
                        output.AddRange(itemData.m_shared.m_damageModifiers.Select(mod => new Entry() { title = ConvertDamageTypes(mod.m_type), value = ConvertDamageModifiers(mod.m_modifier) }));
                    }

                    if (itemData.m_shared.m_setStatusEffect)
                    {
                        foreach (Skills.SkillType skill in allSkills)
                        {
                            float amount = new float();
                            itemData.m_shared.m_setStatusEffect.ModifySkillLevel(skill, ref amount);
                            if (amount <= 0) continue;
                            output.Add(new(){title = ConvertSkills(skill), value = $"<color=orange>+</color>{amount}"});
                        }
                    }

                    if (itemData.m_shared.m_equipStatusEffect)
                    {
                        StatusEffect equipEffect = itemData.m_shared.m_equipStatusEffect;
                        output.Add(new(){title = "$almanac_equip_effects", value = "title"});
                        output.Add(new(){title = "$almanac_equip_status_effect", value = equipEffect.m_name});

                        foreach (Skills.SkillType skill in allSkills)
                        {
                            float skillLevel = new float();
                            equipEffect.ModifySkillLevel(skill, ref skillLevel);
                            float raiseLevel = new();
                            equipEffect.ModifyRaiseSkill(skill, ref raiseLevel);
                            if (skillLevel > 0)
                            {
                                output.Add(new(){title = ConvertSkills(skill), value = $"<color=orange>+</color>{skillLevel}"});
                            }

                            if (raiseLevel > 0)
                            {
                                output.Add(new(){title = ConvertSkills(skill), value = $"<color=orange>+</color>{raiseLevel}"});
                            }
                        }
                        float fallDamage = new();
                        equipEffect.ModifyFallDamage(40f, ref fallDamage);
                        float healthRegen = new float();
                        equipEffect.ModifyHealthRegen(ref healthRegen);
                        float staminaRegen = new float();
                        equipEffect.ModifyStaminaRegen(ref staminaRegen);
                        float eitrRegen = new();
                        equipEffect.ModifyEitrRegen(ref eitrRegen);
                        
                        if (fallDamage > 0) output.Add(new(){title = "$almanac_fall_damage", value = fallDamage.ToString(CultureInfo.CurrentCulture)});
                        if (healthRegen > 0) output.Add(new(){title = "$se_healthregen", value = healthRegen.ToString(CultureInfo.CurrentCulture)});
                        if (staminaRegen > 0) output.Add(new(){title = "$se_staminaregen", value = staminaRegen.ToString(CultureInfo.CurrentCulture)});
                        if (eitrRegen > 0) output.Add(new(){title = "$se_eitrregen", value = eitrRegen.ToString(CultureInfo.CurrentCulture)});
                        
                        HitData.DamageModifiers modifiers = new();
                        equipEffect.ModifyDamageMods(ref modifiers);
                        
                        output.Add(new Entry() { title = "$inventory_blunt", value = ConvertDamageModifiers(modifiers.m_blunt) });
                        output.Add(new Entry() { title = "$inventory_slash", value = ConvertDamageModifiers(modifiers.m_slash) });
                        output.Add(new Entry() { title = "$inventory_pierce", value = ConvertDamageModifiers(modifiers.m_pierce) });
                        output.Add(new Entry() { title = "$inventory_chop", value = ConvertDamageModifiers(modifiers.m_chop) });
                        output.Add(new Entry() { title = "$inventory_pickaxe", value = ConvertDamageModifiers(modifiers.m_pickaxe) });
                        output.Add(new Entry() { title = "$inventory_fire", value = ConvertDamageModifiers(modifiers.m_fire) });
                        output.Add(new Entry() { title = "$inventory_frost", value = ConvertDamageModifiers(modifiers.m_frost) });
                        output.Add(new Entry() { title = "$inventory_lightning", value = ConvertDamageModifiers(modifiers.m_lightning) });
                        output.Add(new Entry() { title = "$inventory_poison", value = ConvertDamageModifiers(modifiers.m_poison) });
                        output.Add(new Entry() { title = "$inventory_spirit", value = ConvertDamageModifiers(modifiers.m_spirit) });

                        if (itemData.m_shared.m_movementModifier > 0f 
                            || itemData.m_shared.m_eitrRegenModifier > 0f 
                            || itemData.m_shared.m_homeItemsStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$almanac_stat_modifiers_title", value = "title" });
                            output.Add(new Entry() { title = "$item_movement_modifier", value = (itemData.m_shared.m_movementModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                            output.Add(new Entry() { title = "$item_eitrregen_modifier", value = (itemData.m_shared.m_eitrRegenModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                            output.Add(new Entry() { title = "$almanac_base_items_stamina_modifier_label", value = (itemData.m_shared.m_homeItemsStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }
                        
                        if (itemData.m_shared.m_heatResistanceModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$item_heat_modifier", value = (itemData.m_shared.m_heatResistanceModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_jumpStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_jumpstamina", value = (itemData.m_shared.m_jumpStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_attackStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_attackstamina", value = (itemData.m_shared.m_attackStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_blockStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_blockstamina", value = (itemData.m_shared.m_blockStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_dodgeStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_dodgestamina", value = (itemData.m_shared.m_dodgeStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_swimStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_swimstamina", value = (itemData.m_shared.m_swimStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_sneakStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_sneakstamina", value = (itemData.m_shared.m_sneakStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }

                        if (itemData.m_shared.m_runStaminaModifier > 0f)
                        {
                            output.Add(new Entry() { title = "$se_runstamina", value = (itemData.m_shared.m_runStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                        }
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Consumable:
                    bool addedFoodTitle = false;
                    if (itemData.m_shared.m_food + itemData.m_shared.m_foodStamina + itemData.m_shared.m_foodBurnTime +
                        itemData.m_shared.m_foodRegen != 0)
                    {
                        addedFoodTitle = true;
                        output.Add(new Entry() { title = "$almanac_food_title", value = "title" });
                        output.Add(new Entry() { title = "$item_food_health", value = itemData.m_shared.m_food.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$item_food_stamina", value = itemData.m_shared.m_foodStamina.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$item_food_eitr", value = itemData.m_shared.m_foodEitr.ToString(CultureInfo.CurrentCulture) });
                        output.Add(new Entry() { title = "$item_food_duration", value = itemData.m_shared.m_foodBurnTime.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
                        output.Add(new Entry() { title = "$item_food_regen", value = itemData.m_shared.m_foodRegen.ToString(CultureInfo.CurrentCulture) + "<color=orange>/tick</color>" });
                    }

                    if (itemData.m_shared.m_consumeStatusEffect)
                    {
                        if (!addedFoodTitle) output.Add(new(){title = "$almanac_food_title", value = "title"});
                        StatusEffect ConsumeEffect = itemData.m_shared.m_consumeStatusEffect;
                        output.Add(new Entry() { title = "$almanac_consume_effect", value = Localization.instance.Localize(ConsumeEffect.m_name) });
                        output.Add(new Entry() { title = "$almanac_consume_category", value = ConsumeEffect.m_category });
                        output.Add(new Entry() { title = "$almanac_consume_tooltip", value = Localization.instance.Localize(ConsumeEffect.m_tooltip) });
                        output.Add(new Entry() { title = "$almanac_consume_attributes", value = SplitCamelCase(ConsumeEffect.m_attributes.ToString()) });
                        output.Add(new Entry() { title = "$almanac_consume_message", value = Localization.instance.Localize(ConsumeEffect.m_startMessage) });
                        output.Add(new Entry() { title = "$almanac_consume_duration", value = ConsumeEffect.m_ttl.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>" });
                        
                        foreach (Skills.SkillType skill in allSkills)
                        {
                            float skillLevel = new float();
                            ConsumeEffect.ModifySkillLevel(skill, ref skillLevel);
                            if (skillLevel > 0) output.Add(new Entry() { title = ConvertSkills(skill), value = "<color=orange>+</color>" + skillLevel.ToString(CultureInfo.CurrentCulture) });

                            float raiseLevel = new float();
                            ConsumeEffect.ModifyRaiseSkill(skill, ref raiseLevel);
                            if (raiseLevel > 0) output.Add(new Entry() { title = ConvertSkills(skill), value = "<color=orange>+</color>" + raiseLevel.ToString(CultureInfo.CurrentCulture) });
                        }
                        
                        float healthRegen = new float();
                        ConsumeEffect.ModifyHealthRegen(ref healthRegen);
                        if (healthRegen > 0) output.Add(new Entry() { title = "$almanac_consume_health_regen", value = (healthRegen * 100).ToString("0.0") + "<color=orange>%</color>" });

                        float staminaRegen = new float();
                        ConsumeEffect.ModifyStaminaRegen(ref staminaRegen);
                        if (staminaRegen > 0) output.Add(new Entry() { title = "$almanac_consume_stamina_regen", value = (staminaRegen * 100).ToString("0.0") + "<color=orange>%</color>" });

                        float eitrRegen = new();
                        ConsumeEffect.ModifyEitrRegen(ref eitrRegen);
                        if (eitrRegen > 0) output.Add(new Entry() { title = "$almanac_consume_eitr_regen", value = (eitrRegen * 100).ToString("0.0") + "<color=orange>%</color>" });

                        HitData.DamageModifiers modifiers = new();
                        ConsumeEffect.ModifyDamageMods(ref modifiers);
                        output.Add(new Entry() { title = "$inventory_blunt", value = ConvertDamageModifiers(modifiers.m_blunt) });
                        output.Add(new Entry() { title = "$inventory_slash", value = ConvertDamageModifiers(modifiers.m_slash) });
                        output.Add(new Entry() { title = "$inventory_pierce", value = ConvertDamageModifiers(modifiers.m_pierce) });
                        output.Add(new Entry() { title = "$inventory_chop", value = ConvertDamageModifiers(modifiers.m_chop) });
                        output.Add(new Entry() { title = "$inventory_pickaxe", value = ConvertDamageModifiers(modifiers.m_pickaxe) });
                        output.Add(new Entry() { title = "$inventory_fire", value = ConvertDamageModifiers(modifiers.m_fire) });
                        output.Add(new Entry() { title = "$inventory_frost", value = ConvertDamageModifiers(modifiers.m_frost) });
                        output.Add(new Entry() { title = "$inventory_lightning", value = ConvertDamageModifiers(modifiers.m_lightning) });
                        output.Add(new Entry() { title = "$inventory_poison", value = ConvertDamageModifiers(modifiers.m_poison) });
                        output.Add(new Entry() { title = "$inventory_spirit", value = ConvertDamageModifiers(modifiers.m_spirit) });
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Shield:
                    output.Add(new Entry() { title = "$almanac_shield_title", value = "title" });
                    output.Add(new Entry() { title = "$almanac_shield_block_power", value = itemData.m_shared.m_blockPower.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_shield_block_power_per_level", value = itemData.m_shared.m_blockPowerPerLevel.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_shield_deflection_force", value = itemData.m_shared.m_deflectionForce.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_shield_deflection_force_per_level", value = itemData.m_shared.m_deflectionForcePerLevel.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_timed_block_bonus", value = itemData.m_shared.m_timedBlockBonus.ToString(CultureInfo.CurrentCulture) });
                    break;
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.Bow:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch:
                case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                    output.Add(new Entry() { title = "$almanac_weapon_title", value = "title" });
                    output.Add(new Entry() { title = "$almanac_weapon_animation_state", value = SplitCamelCase(itemData.m_shared.m_animationState.ToString()) });
                    output.Add(new Entry() { title = "$almanac_weapon_skill_type", value = ConvertSkills(itemData.m_shared.m_skillType) });
                    output.Add(new Entry() { title = "$almanac_tool_tier", value = itemData.m_shared.m_toolTier.ToString() });
                    output.Add(new Entry() { title = "$inventory_damage", value = itemData.m_shared.m_damages.m_damage.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_damage.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_blunt", value = itemData.m_shared.m_damages.m_blunt.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_blunt.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_slash", value = itemData.m_shared.m_damages.m_slash.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_slash.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_pierce", value = itemData.m_shared.m_damages.m_pierce.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_pierce.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_chop", value = itemData.m_shared.m_damages.m_chop.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_chop.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_pickaxe", value = itemData.m_shared.m_damages.m_pickaxe.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_pickaxe.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_fire", value = itemData.m_shared.m_damages.m_fire.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_fire.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_frost", value = itemData.m_shared.m_damages.m_frost.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_frost.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_lightning", value = itemData.m_shared.m_damages.m_lightning.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_lightning.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_poison", value = itemData.m_shared.m_damages.m_poison.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_poison.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$inventory_spirit", value = itemData.m_shared.m_damages.m_spirit.ToString(CultureInfo.CurrentCulture) + "<color=orange> +</color>" + itemData.m_shared.m_damagesPerLevel.m_spirit.ToString(CultureInfo.CurrentCulture) + "<color=orange>/lvl</color>" });
                    output.Add(new Entry() { title = "$almanac_attack_force", value = itemData.m_shared.m_attackForce.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_back_stab_bonus", value = itemData.m_shared.m_backstabBonus.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_dodgeable", value = ConvertBoolean(itemData.m_shared.m_dodgeable) });
                    output.Add(new Entry() { title = "$almanac_blockable", value = ConvertBoolean(itemData.m_shared.m_blockable) });
                    output.Add(new Entry() { title = "$almanac_tame_only", value = ConvertBoolean(itemData.m_shared.m_tamedOnly) });
                    output.Add(new Entry() { title = "$almanac_always_rotate", value = ConvertBoolean(itemData.m_shared.m_alwaysRotate) });
                    output.Add(new Entry() { title = "$almanac_attack_effect", value = itemData.m_shared.m_attackStatusEffect ? RemoveParentheses(itemData.m_shared.m_attackStatusEffect.ToString()) : "0" });
                    output.Add(new Entry() { title = "$item_chancetoapplyse", value = (itemData.m_shared.m_attackStatusEffect ? (itemData.m_shared.m_attackStatusEffectChance * 100).ToString(CultureInfo.CurrentCulture) : "100") + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$almanac_spawn_on_hit", value = itemData.m_shared.m_spawnOnHit ? RemoveParentheses(itemData.m_shared.m_spawnOnHit.ToString()) : "0" });
                    output.Add(new Entry() { title = "$almanac_spawn_on_hit_terrain", value = itemData.m_shared.m_spawnOnHitTerrain ? RemoveParentheses(itemData.m_shared.m_spawnOnHitTerrain.ToString()) : "0" });
                    output.Add(new Entry() { title = "$almanac_projectile_tooltip", value = ConvertBoolean(itemData.m_shared.m_projectileToolTip) });
                    output.Add(new Entry() { title = "$almanac_ammo_type", value = Localization.instance.Localize(itemData.m_shared.m_ammoType) });
                    output.Add(new(){title = "$almanac_attack_label", value = "title"});                    
                    output.Add(new Entry() { title = "$almanac_attack_type", value = ConvertAttackTypes(itemData.m_shared.m_attack.m_attackType) });
                    output.Add(new Entry() { title = "$almanac_attack_animation", value = itemData.m_shared.m_attack.m_attackAnimation });
                    output.Add(new Entry() { title = "$almanac_attack_random_animations", value = itemData.m_shared.m_attack.m_attackRandomAnimations.ToString() });
                    output.Add(new Entry() { title = "$almanac_attack_chain_levels", value = itemData.m_shared.m_attack.m_attackChainLevels.ToString() });
                    output.Add(new Entry() { title = "$almanac_attack_looping", value = itemData.m_shared.m_attack.m_loopingAttack.ToString() });
                    output.Add(new Entry() { title = "$almanac_consume_item", value = itemData.m_shared.m_attack.m_consumeItem.ToString() });
                    output.Add(new Entry() { title = "$almanac_hit_terrain", value = itemData.m_shared.m_attack.m_hitTerrain.ToString() });
                    output.Add(new Entry() { title = "$almanac_is_home_item", value = itemData.m_shared.m_attack.m_isHomeItem.ToString() });
                    output.Add(new Entry() { title = "$item_staminause", value = itemData.m_shared.m_attack.m_attackStamina.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_eitruse", value = itemData.m_shared.m_attack.m_attackEitr.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_healthuse", value = itemData.m_shared.m_attack.m_attackHealth.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_healthhitreturn", value = itemData.m_shared.m_attack.m_attackHealthReturnHit.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_healthuse_percentage", value = itemData.m_shared.m_attack.m_attackHealthPercentage.ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$item_staminahold", value = itemData.m_shared.m_attack.m_drawStaminaDrain.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_speed_factor", value = (itemData.m_shared.m_attack.m_speedFactor * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$almanac_speed_factor_rotation", value = (itemData.m_shared.m_attack.m_speedFactorRotation * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$almanac_attack_start_noise", value = itemData.m_shared.m_attack.m_attackStartNoise.ToString("0.0") });
                    output.Add(new(){title = "$almanac_secondary_attack_label", value = "title"});
                    output.Add(new Entry() { title = "$almanac_attack_type", value = ConvertAttackTypes(itemData.m_shared.m_secondaryAttack.m_attackType) });
                    output.Add(new Entry() { title = "$almanac_attack_animation", value = itemData.m_shared.m_secondaryAttack.m_attackAnimation });
                    output.Add(new Entry() { title = "$almanac_attack_random_animations", value = itemData.m_shared.m_secondaryAttack.m_attackRandomAnimations.ToString() });
                    output.Add(new Entry() { title = "$almanac_attack_chain_levels", value = itemData.m_shared.m_secondaryAttack.m_attackChainLevels.ToString() });
                    output.Add(new Entry() { title = "$almanac_attack_looping", value = itemData.m_shared.m_secondaryAttack.m_loopingAttack.ToString() });
                    output.Add(new Entry() { title = "$almanac_consume_item", value = itemData.m_shared.m_secondaryAttack.m_consumeItem.ToString() });
                    output.Add(new Entry() { title = "$almanac_hit_terrain", value = itemData.m_shared.m_secondaryAttack.m_hitTerrain.ToString() });
                    output.Add(new Entry() { title = "$almanac_is_home_item", value = itemData.m_shared.m_secondaryAttack.m_isHomeItem.ToString() });
                    output.Add(new Entry() { title = "$item_staminause", value = itemData.m_shared.m_secondaryAttack.m_attackStamina.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_eitruse", value = itemData.m_shared.m_secondaryAttack.m_attackEitr.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_healthuse", value = itemData.m_shared.m_secondaryAttack.m_attackHealth.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_healthhitreturn", value = itemData.m_shared.m_secondaryAttack.m_attackHealthReturnHit.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$item_healthuse_percentage", value = itemData.m_shared.m_secondaryAttack.m_attackHealthPercentage.ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$item_staminahold", value = itemData.m_shared.m_secondaryAttack.m_drawStaminaDrain.ToString(CultureInfo.CurrentCulture) });
                    output.Add(new Entry() { title = "$almanac_speed_factor", value = (itemData.m_shared.m_secondaryAttack.m_speedFactor * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$almanac_speed_factor_rotation", value = (itemData.m_shared.m_secondaryAttack.m_speedFactorRotation * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>" });
                    output.Add(new Entry() { title = "$almanac_attack_start_noise", value = itemData.m_shared.m_secondaryAttack.m_attackStartNoise.ToString("0.0") });
                    break;
            }
        }

        return output;
    }
    
    public static List<Entry> GetCreatureEntries(CreatureDataCollector.CreatureData data)
    {
        List<Entry> output = new()
        {
            new() { title = "$se_health", value = data.health.ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_faction", value = SplitCamelCase(data.faction) },
            new Entry() { title = "$almanac_creature_resistances", value = "title" },
            new Entry() { title = "$inventory_blunt", value = SplitCamelCase(data.blunt) },
            new Entry() { title = "$inventory_slash", value = SplitCamelCase(data.slash) },
            new Entry() { title = "$inventory_pierce", value = SplitCamelCase(data.pierce) },
            new Entry() { title = "$inventory_chop", value = SplitCamelCase(data.chop) },
            new Entry() { title = "$inventory_pickaxe", value = SplitCamelCase(data.pickaxe) },
            new Entry() { title = "$inventory_fire", value = SplitCamelCase(data.fire) },
            new Entry() { title = "$inventory_frost", value = SplitCamelCase(data.frost) },
            new Entry() { title = "$inventory_lightning", value = SplitCamelCase(data.lightning) },
            new Entry() { title = "$inventory_poison", value = SplitCamelCase(data.poison) },
            new Entry() { title = "$inventory_spirit", value = SplitCamelCase(data.spirit) }
        };
        
        int kill = LocalPlayerData.Player_Kill_Deaths[data.defeatedKey].kills;
        int deaths = LocalPlayerData.Player_Kill_Deaths[data.defeatedKey].deaths;

        if (kill + deaths > 0)
        {
            output.Add(new(){title = "$almanac_kill_death_title", value = "title"});
            output.Add(new(){title = "$almanac_kill_count", value = kill.ToString()});
            output.Add(new(){title = "$almanac_death_count", value = deaths.ToString()});

            if (kill > 0 && deaths > 0)
            {
                float kd = Mathf.Floor((float)kill / (float)deaths);
                output.Add(new(){title = "$almanac_kill_death_ratio", value = kd.ToString(CultureInfo.CurrentCulture)});
            }
        }

        return output;
    }
    
    public static List<Entry> GetCreatureEntries_1(CreatureDataCollector.CreatureData data)
    {
        List<Entry> output = new()
        {
            new() { title = "$almanac_avoid_fire", value = ConvertBoolean(data.avoidFire) },
            new() { title = "$almanac_afraid_of_fire", value = ConvertBoolean(data.afraidOfFire) },
            new() { title = "$almanac_avoid_water", value = ConvertBoolean(data.avoidWater) }
        };

        foreach (var spot in data.weakSpot)
        {
            output.Add(new(){title = "$almanac_weak_spot", value = spot});
        }
        
        output.Add(new(){title = "$almanac_stagger_when_blocked", value = ConvertBoolean(data.staggerWhenBlocked)});
        output.Add(new(){title = "$almanac_stagger_damage_factor", value = data.staggerDamageFactor.ToString("0.0")});
        output.Add(new(){ title = "$almanac_tolerate_water", value = ConvertBoolean(data.tolerateWater) });
        output.Add(new() { title = "$almanac_tolerate_smoke", value = ConvertBoolean(data.tolerateSmoke) });
        output.Add(new() { title = "$almanac_tolerate_tar", value = ConvertBoolean(data.tolerateTar) });
        output.Add(new() { title = "$almanac_defeat_key", value = data.defeatedKey});

        foreach (var attack in data.defaultItems)
        {
            if (attack.name == "Unknown") continue;
            output.Add(new Entry() { title = "$almanac_creature_attacks", value = "title" });
            output.Add(new Entry() { title = "$almanac_attack_name", value = attack.name });
            output.Add(new Entry() { title = "$almanac_damage", value = attack.damage.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_blunt", value = attack.blunt.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_slash", value = attack.slash.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_pierce", value = attack.pierce.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_chop", value = attack.chop.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_pickaxe", value = attack.pickaxe.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_fire", value = attack.fire.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_frost", value = attack.frost.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_lightning", value = attack.lightning.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_poison", value = attack.poison.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$inventory_spirit", value = attack.spirit.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$item_knockback", value = attack.attackForce.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$item_backstab", value = attack.backStabBonus.ToString(CultureInfo.CurrentCulture) });
            output.Add(new Entry() { title = "$almanac_dodgeable", value = ConvertBoolean(attack.dodgeable) });
            output.Add(new Entry() { title = "$almanac_blockable", value = ConvertBoolean(attack.blockable) });
            output.Add(new Entry() { title = "$almanac_status_effect", value = attack.statusEffect });
            output.Add(new Entry() { title = "$almanac_status_effect_tooltip", value = attack.statusEffectTooltip });
        }
        
        return output;
    }
    public static List<Entry> GetMetricEntries()
    {
        return new()
        {
            new Entry() { title = "$almanac_kill_title", value = "title" },
            new Entry() { title = "$almanac_enemy_kills", value = GetPlayerStat(PlayerStatType.EnemyKills).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_player_kills", value = GetPlayerStat(PlayerStatType.PlayerKills).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_hits_taken_enemies", value = GetPlayerStat(PlayerStatType.HitsTakenEnemies).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_total_deaths", value = GetPlayerStat(PlayerStatType.Deaths).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_enemy_hits", value = GetPlayerStat(PlayerStatType.EnemyHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_enemy_last_hit", value = GetPlayerStat(PlayerStatType.EnemyKillsLastHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_player_kills", value = GetPlayerStat(PlayerStatType.PlayerKills).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_craft_or_upgrade_count", value = GetPlayerStat(PlayerStatType.CraftsOrUpgrades).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_builds", value = GetPlayerStat(PlayerStatType.Builds).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_crafts", value = GetPlayerStat(PlayerStatType.Crafts).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_upgrades", value = GetPlayerStat(PlayerStatType.Upgrades).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_misc_title", value = "title" },
            new Entry() { title = "$almanac_skeleton_summoned", value = GetPlayerStat(PlayerStatType.SkeletonSummons).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_item_picked_up", value = GetPlayerStat(PlayerStatType.ItemsPickedUp).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_jumps", value = GetPlayerStat(PlayerStatType.Jumps).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_portal_used", value = GetPlayerStat(PlayerStatType.PortalsUsed).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_cheats", value = GetPlayerStat(PlayerStatType.Cheats).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_world_loads", value = GetPlayerStat(PlayerStatType.WorldLoads).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_creature_tame", value = GetPlayerStat(PlayerStatType.CreatureTamed).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_arrows_shot", value = GetPlayerStat(PlayerStatType.ArrowsShot).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_doors_opened", value = GetPlayerStat(PlayerStatType.DoorsOpened).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_doors_closed", value = GetPlayerStat(PlayerStatType.DoorsClosed).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_bees_harvested", value = GetPlayerStat(PlayerStatType.BeesHarvested).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_sap_harvested", value = GetPlayerStat(PlayerStatType.SapHarvested).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_turret_ammo_added", value = GetPlayerStat(PlayerStatType.TurretAmmoAdded).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_turret_trophy_set", value = GetPlayerStat(PlayerStatType.TurretTrophySet).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_trap_armed", value = GetPlayerStat(PlayerStatType.TrapArmed).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_trap_triggered", value = GetPlayerStat(PlayerStatType.TrapTriggered).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_place_stacks", value = GetPlayerStat(PlayerStatType.PlaceStacks).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_portal_dungeon_in", value = GetPlayerStat(PlayerStatType.PortalDungeonIn).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_portal_dungeon_out", value = GetPlayerStat(PlayerStatType.PortalDungeonOut).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_total_boss_kills", value = GetPlayerStat(PlayerStatType.BossKills).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_boss_last_hits", value = GetPlayerStat(PlayerStatType.BossLastHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_distance_title", value = "title" },
            new Entry() { title = "$almanac_distanced_traveled", value = GetPlayerStat(PlayerStatType.DistanceTraveled).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_distance_walked", value = GetPlayerStat(PlayerStatType.DistanceWalk).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_distance_ran", value = GetPlayerStat(PlayerStatType.DistanceRun).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_distance_sailed", value = GetPlayerStat(PlayerStatType.DistanceSail).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_distance_air", value = GetPlayerStat(PlayerStatType.DistanceAir).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_base_title", value = "title" },
            new Entry() { title = "$almanac_time_in_base", value = (GetPlayerStat(PlayerStatType.TimeInBase) / 60).ToString(CultureInfo.CurrentCulture) + "<color=orange>min</color>" },
            new Entry() { title = "$almanac_time_out_base", value = (GetPlayerStat(PlayerStatType.TimeOutOfBase) / 60).ToString(CultureInfo.CurrentCulture) + "<color=orange>min</color>" },
            new Entry() { title = "$almanac_sleep", value = GetPlayerStat(PlayerStatType.Sleep).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_stands_title", value = "title" },
            new Entry() { title = "$almanac_item_stand_used", value = GetPlayerStat(PlayerStatType.ItemStandUses).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_armor_stand_used", value = GetPlayerStat(PlayerStatType.ArmorStandUses).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_trees_title", value = "title" },
            new Entry() { title = "$almanac_tree_chopped", value = GetPlayerStat(PlayerStatType.TreeChops).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree", value = GetPlayerStat(PlayerStatType.Tree).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_0", value = GetPlayerStat(PlayerStatType.TreeTier0).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_1", value = GetPlayerStat(PlayerStatType.TreeTier1).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_2", value = GetPlayerStat(PlayerStatType.TreeTier2).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_3", value = GetPlayerStat(PlayerStatType.TreeTier3).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_4", value = GetPlayerStat(PlayerStatType.TreeTier4).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_5", value = GetPlayerStat(PlayerStatType.TreeTier5).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_log_chopped", value = GetPlayerStat(PlayerStatType.LogChops).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_logs", value = GetPlayerStat(PlayerStatType.Logs).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_ore_title", value = "title" },
            new Entry() { title = "$almanac_mine_hits", value = GetPlayerStat(PlayerStatType.MineHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mines", value = GetPlayerStat(PlayerStatType.Mines).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_0", value = GetPlayerStat(PlayerStatType.MineTier0).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_1", value = GetPlayerStat(PlayerStatType.MineTier1).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_2", value = GetPlayerStat(PlayerStatType.MineTier2).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_3", value = GetPlayerStat(PlayerStatType.MineTier3).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_4", value = GetPlayerStat(PlayerStatType.MineTier4).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_5", value = GetPlayerStat(PlayerStatType.MineTier5).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_raven_title", value = "title" },
            new Entry() { title = "$almanac_raven_hits", value = GetPlayerStat(PlayerStatType.RavenHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_raven_talk", value = GetPlayerStat(PlayerStatType.RavenTalk).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_raven_appear", value = GetPlayerStat(PlayerStatType.RavenAppear).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_food_eaten", value = GetPlayerStat(PlayerStatType.FoodEaten).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tombstone_title", value = "title" },
            new Entry() { title = "$almanac_tombstones_open_own", value = GetPlayerStat(PlayerStatType.TombstonesOpenedOwn).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tombstone_open_other", value = GetPlayerStat(PlayerStatType.TombstonesOpenedOther).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tombstone_fit", value = GetPlayerStat(PlayerStatType.TombstonesFit).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_title", value = "title" },
            new Entry() { title = "$almanac_death_by_undefined", value = GetPlayerStat(PlayerStatType.DeathByUndefined).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_enemy_hit", value = GetPlayerStat(PlayerStatType.DeathByEnemyHit).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_player_hit", value = GetPlayerStat(PlayerStatType.DeathByPlayerHit).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_fall", value = GetPlayerStat(PlayerStatType.DeathByFall).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_drowning", value = GetPlayerStat(PlayerStatType.DeathByDrowning).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_burning", value = GetPlayerStat(PlayerStatType.DeathByBurning).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_freezing", value = GetPlayerStat(PlayerStatType.DeathByFreezing).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_poisoned", value = GetPlayerStat(PlayerStatType.DeathByPoisoned).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_smoke", value = GetPlayerStat(PlayerStatType.DeathBySmoke).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_water", value = GetPlayerStat(PlayerStatType.DeathByWater).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_edge_of_world", value = GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_impact", value = GetPlayerStat(PlayerStatType.DeathByImpact).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_cart", value = GetPlayerStat(PlayerStatType.DeathByCart).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_tree", value = GetPlayerStat(PlayerStatType.DeathByTree).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_self", value = GetPlayerStat(PlayerStatType.DeathBySelf).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_structural", value = GetPlayerStat(PlayerStatType.DeathByStructural).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_boat", value = GetPlayerStat(PlayerStatType.DeathByBoat).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_turret", value = GetPlayerStat(PlayerStatType.DeathByTurret).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_stalagtite", value = GetPlayerStat(PlayerStatType.DeathByStalagtite).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_guardian_power_title", value = "title" },
            new Entry() { title = "$almanac_set_guardian_power", value = GetPlayerStat(PlayerStatType.SetGuardianPower).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_eikthyr", value = GetPlayerStat(PlayerStatType.SetPowerEikthyr).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_elder", value = GetPlayerStat(PlayerStatType.SetPowerElder).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_bonemass", value = GetPlayerStat(PlayerStatType.SetPowerBonemass).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_moder", value = GetPlayerStat(PlayerStatType.SetPowerModer).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_yagluth", value = GetPlayerStat(PlayerStatType.SetPowerYagluth).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_queen", value = GetPlayerStat(PlayerStatType.SetPowerQueen).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_ashlands", value = GetPlayerStat(PlayerStatType.SetPowerAshlands).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_deepNorth", value = GetPlayerStat(PlayerStatType.SetPowerDeepNorth).ToString(CultureInfo.CurrentCulture)},
            new Entry() { title = "$almanac_stands_title", value = "title" },
            new Entry() { title = "$almanac_item_stand_used", value = GetPlayerStat(PlayerStatType.ItemStandUses).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_armor_stand_used", value = GetPlayerStat(PlayerStatType.ArmorStandUses).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_trees_title", value = "title" },
            new Entry() { title = "$almanac_tree_chopped", value = GetPlayerStat(PlayerStatType.TreeChops).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree", value = GetPlayerStat(PlayerStatType.Tree).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_0", value = GetPlayerStat(PlayerStatType.TreeTier0).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_1", value = GetPlayerStat(PlayerStatType.TreeTier1).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_2", value = GetPlayerStat(PlayerStatType.TreeTier2).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_3", value = GetPlayerStat(PlayerStatType.TreeTier3).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_4", value = GetPlayerStat(PlayerStatType.TreeTier4).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tree_tier_5", value = GetPlayerStat(PlayerStatType.TreeTier5).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_log_chopped", value = GetPlayerStat(PlayerStatType.LogChops).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_logs", value = GetPlayerStat(PlayerStatType.Logs).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_ore_title", value = "title" },
            new Entry() { title = "$almanac_mine_hits", value = GetPlayerStat(PlayerStatType.MineHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mines", value = GetPlayerStat(PlayerStatType.Mines).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_0", value = GetPlayerStat(PlayerStatType.MineTier0).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_1", value = GetPlayerStat(PlayerStatType.MineTier1).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_2", value = GetPlayerStat(PlayerStatType.MineTier2).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_3", value = GetPlayerStat(PlayerStatType.MineTier3).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_4", value = GetPlayerStat(PlayerStatType.MineTier4).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_mine_tier_5", value = GetPlayerStat(PlayerStatType.MineTier5).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_raven_title", value = "title" },
            new Entry() { title = "$almanac_raven_hits", value = GetPlayerStat(PlayerStatType.RavenHits).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_raven_talk", value = GetPlayerStat(PlayerStatType.RavenTalk).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_raven_appear", value = GetPlayerStat(PlayerStatType.RavenAppear).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_food_eaten", value = GetPlayerStat(PlayerStatType.FoodEaten).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tombstone_title", value = "title" },
            new Entry() { title = "$almanac_tombstones_open_own", value = GetPlayerStat(PlayerStatType.TombstonesOpenedOwn).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tombstone_open_other", value = GetPlayerStat(PlayerStatType.TombstonesOpenedOther).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_tombstone_fit", value = GetPlayerStat(PlayerStatType.TombstonesFit).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_title", value = "title" },
            new Entry() { title = "$almanac_death_by_undefined", value = GetPlayerStat(PlayerStatType.DeathByUndefined).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_enemy_hit", value = GetPlayerStat(PlayerStatType.DeathByEnemyHit).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_player_hit", value = GetPlayerStat(PlayerStatType.DeathByPlayerHit).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_fall", value = GetPlayerStat(PlayerStatType.DeathByFall).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_drowning", value = GetPlayerStat(PlayerStatType.DeathByDrowning).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_burning", value = GetPlayerStat(PlayerStatType.DeathByBurning).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_freezing", value = GetPlayerStat(PlayerStatType.DeathByFreezing).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_poisoned", value = GetPlayerStat(PlayerStatType.DeathByPoisoned).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_smoke", value = GetPlayerStat(PlayerStatType.DeathBySmoke).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_water", value = GetPlayerStat(PlayerStatType.DeathByWater).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_edge_of_world", value = GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_impact", value = GetPlayerStat(PlayerStatType.DeathByImpact).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_cart", value = GetPlayerStat(PlayerStatType.DeathByCart).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_tree", value = GetPlayerStat(PlayerStatType.DeathByTree).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_self", value = GetPlayerStat(PlayerStatType.DeathBySelf).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_structural", value = GetPlayerStat(PlayerStatType.DeathByStructural).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_boat", value = GetPlayerStat(PlayerStatType.DeathByBoat).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_turret", value = GetPlayerStat(PlayerStatType.DeathByTurret).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_death_by_stalagtite", value = GetPlayerStat(PlayerStatType.DeathByStalagtite).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_guardian_power_title", value = "title" },
            new Entry() { title = "$almanac_set_guardian_power", value = GetPlayerStat(PlayerStatType.SetGuardianPower).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_eikthyr", value = GetPlayerStat(PlayerStatType.SetPowerEikthyr).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_elder", value = GetPlayerStat(PlayerStatType.SetPowerElder).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_bonemass", value = GetPlayerStat(PlayerStatType.SetPowerBonemass).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_moder", value = GetPlayerStat(PlayerStatType.SetPowerModer).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_yagluth", value = GetPlayerStat(PlayerStatType.SetPowerYagluth).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_queen", value = GetPlayerStat(PlayerStatType.SetPowerQueen).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_ashlands", value = GetPlayerStat(PlayerStatType.SetPowerAshlands).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_set_power_deepNorth", value = GetPlayerStat(PlayerStatType.SetPowerDeepNorth).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_guardian_power", value = GetPlayerStat(PlayerStatType.UseGuardianPower).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_eikthyr", value = GetPlayerStat(PlayerStatType.UsePowerEikthyr).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_elder", value = GetPlayerStat(PlayerStatType.UsePowerElder).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_bonemass", value = GetPlayerStat(PlayerStatType.UsePowerBonemass).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_moder", value = GetPlayerStat(PlayerStatType.UsePowerModer).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_yagluth", value = GetPlayerStat(PlayerStatType.UsePowerYagluth).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_queen", value = GetPlayerStat(PlayerStatType.UsePowerQueen).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_ashlands", value = GetPlayerStat(PlayerStatType.UsePowerAshlands).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_use_power_deepNorth", value = GetPlayerStat(PlayerStatType.UsePowerDeepNorth).ToString(CultureInfo.CurrentCulture) },
            new Entry() { title = "$almanac_count", value = GetPlayerStat(PlayerStatType.Count).ToString(CultureInfo.CurrentCulture) }
        };
    }

    public class Entry
    {
        public string title = "";
        public string value = "";
    }
}