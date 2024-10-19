using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Almanac.Achievements;
using Almanac.Data;
using BepInEx;
using UnityEngine;
using static Almanac.AlmanacPlugin;
using static Almanac.Data.PlayerStats;
using static Almanac.Utilities.Utility;

namespace Almanac.UI;

public static class Entries
{
    public static List<Entry> GetPieceEntries(GameObject prefab)
    {
        List<Entry> output = new();
        if (prefab.TryGetComponent(out Piece piece))
        {
            output.Add(new Entry("$almanac_enabled", ConvertBoolean(piece.enabled)));
            output.Add(new Entry("$almanac_piece_category", SplitCamelCase(piece.m_category.ToString())));
            output.Add(new Entry("$almanac_is_upgrade", ConvertBoolean(piece.m_isUpgrade)));
            output.Add(new Entry("$almanac_comfort", piece.m_comfort.ToString()));
            output.Add(new Entry("$almanac_comfort_group", SplitCamelCase(piece.m_comfortGroup.ToString())));
            output.Add(new Entry("$almanac_comfort_object", piece.m_comfortObject ? piece.m_comfortObject.name : "0"));
            output.Add(new Entry("$almanac_ground_piece", ConvertBoolean(piece.m_groundPiece)));
            output.Add(new Entry("$almanac_allow_alt_ground", ConvertBoolean(piece.m_allowAltGroundPlacement)));
            output.Add(new Entry("$almanac_ground_only", ConvertBoolean(piece.m_groundOnly)));
            output.Add(new Entry("$almanac_cultivated_only", ConvertBoolean(piece.m_cultivatedGroundOnly)));
            output.Add(new Entry("$almanac_water_piece", ConvertBoolean(piece.m_waterPiece)));
            output.Add(new Entry("$almanac_clip_ground", ConvertBoolean(piece.m_clipGround)));
            output.Add(new Entry("$almanac_clip_everything", ConvertBoolean(piece.m_clipEverything)));
            output.Add(new Entry("$almanac_no_in_water", ConvertBoolean(piece.m_noInWater)));
            output.Add(new Entry("$almanac_no_on_wood", ConvertBoolean(piece.m_notOnWood)));
            output.Add(new Entry("$almanac_no_on_tilt", ConvertBoolean(piece.m_notOnTiltingSurface)));
            output.Add(new Entry("$almanac_ceiling_only", ConvertBoolean(piece.m_inCeilingOnly)));
            output.Add(new Entry("$almanac_no_on_floor", ConvertBoolean(piece.m_notOnFloor)));
            output.Add(new Entry("$almanac_no_clipping", ConvertBoolean(piece.m_notOnFloor))); // This looks like a duplicate from the previous line
            output.Add(new Entry("$almanac_only_in_teleport_area", ConvertBoolean(piece.m_onlyInTeleportArea)));
            output.Add(new Entry("$almanac_allow_dungeon", ConvertBoolean(piece.m_allowedInDungeons)));
            output.Add(new Entry("$almanac_space_req", piece.m_spaceRequirement.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_repair_piece", ConvertBoolean(piece.m_repairPiece)));
            output.Add(new Entry("$almanac_can_rotate", ConvertBoolean(piece.m_canRotate)));
            output.Add(new Entry("$almanac_random_rotate", ConvertBoolean(piece.m_randomInitBuildRotation)));
            output.Add(new Entry("$almanac_allow_rotate_overlap", ConvertBoolean(piece.m_allowRotatedOverlap)));
            output.Add(new Entry("$almanac_vegetation_ground_only", ConvertBoolean(piece.m_vegetationGroundOnly)));
            if (_ShowAllData.Value is Toggle.On) output.Add(new Entry("$almanac_prefab_name", piece.name));
            if (piece.m_blockingPieces.Count > 0)
                output.AddRange(piece.m_blockingPieces.Select(block => new Entry("$almanac_block_piece", block.name)));
            output.Add(new Entry("$almanac_block_radius", piece.m_blockRadius.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_must_connect_to", piece.m_mustConnectTo ? piece.m_mustConnectTo.ToString() : "0"));
            output.Add(new Entry("$almanac_connect_radius", piece.m_connectRadius.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_must_be_above", ConvertBoolean(piece.m_mustBeAboveConnected)));
            output.Add(new Entry("$almanac_piece_biome", SplitCamelCase(piece.m_onlyInBiome.ToString())));
            output.Add(new Entry("$almanac_dlc", piece.m_dlc));
            output.Add(new Entry("$almanac_craft_station", piece.m_craftingStation ? Localization.instance.Localize(piece.m_craftingStation.m_name) : "0"));
        }
        
        if (prefab.TryGetComponent(out CraftingStation craftingStation))
        {
            output.Add(new Entry("$almanac_crafting_station_title", "title"));
            output.Add(new Entry("$almanac_discover_range", craftingStation.m_discoverRange.ToString("0.0")));
            output.Add(new Entry("$almanac_range_build", craftingStation.m_rangeBuild.ToString("0.0")));
            output.Add(new Entry("$almanac_extra_range_per_level", craftingStation.m_extraRangePerLevel.ToString("0.0")));
            output.Add(new Entry("$almanac_require_roof1", ConvertBoolean(craftingStation.m_craftRequireRoof)));
            output.Add(new Entry("$almanac_require_fire", ConvertBoolean(craftingStation.m_craftRequireFire)));
            output.Add(new Entry("$almanac_show_basic_recipes", ConvertBoolean(craftingStation.m_showBasicRecipies)));
            output.Add(new Entry("$almanac_use_distance", craftingStation.m_useDistance.ToString("0.0")));
            output.Add(new Entry("$almanac_use_animation", craftingStation.m_useAnimation.ToString()));
        }
        if (prefab.TryGetComponent(out StationExtension stationExtension))
        {
            output.Add(new Entry("$almanac_extension_title", "title"));
            output.Add(new Entry("$almanac_piece_extends", stationExtension.m_craftingStation ? Localization.instance.Localize(stationExtension.m_craftingStation.m_name) : "0"));
            output.Add(new Entry("$almanac_extension_distance", stationExtension.m_maxStationDistance.ToString("0.0")));
            output.Add(new Entry("$almanac_extension_stack", stationExtension.m_stack.ToString()));
            output.Add(new Entry("$almanac_continuous_connection", ConvertBoolean(stationExtension.m_continousConnection)));
        }

        if (prefab.TryGetComponent(out WearNTear wearNTear))
        {
            output.Add(new Entry("$almanac_wear_tear_title", "title"));
            output.Add(new Entry("$almanac_no_roof_wear", ConvertBoolean(wearNTear.m_noRoofWear)));
            output.Add(new Entry("$almanac_no_support_wear", ConvertBoolean(wearNTear.m_noSupportWear)));
            output.Add(new Entry("$almanac_material_type", SplitCamelCase(wearNTear.m_materialType.ToString())));
            output.Add(new Entry("$almanac_piece_supports", ConvertBoolean(wearNTear.m_supports)));
            output.Add(new Entry("$almanac_support_value", wearNTear.m_support.ToString("0.0")));
            output.Add(new Entry("$almanac_piece_health", wearNTear.m_health.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_blunt", ConvertDamageModifiers(wearNTear.m_damages.m_blunt)));
            output.Add(new Entry("$almanac_slash", ConvertDamageModifiers(wearNTear.m_damages.m_slash)));
            output.Add(new Entry("$almanac_pierce", ConvertDamageModifiers(wearNTear.m_damages.m_pierce)));
            output.Add(new Entry("$almanac_chop", ConvertDamageModifiers(wearNTear.m_damages.m_chop)));
            output.Add(new Entry("$almanac_pickaxe", ConvertDamageModifiers(wearNTear.m_damages.m_pickaxe)));
            output.Add(new Entry("$almanac_fire", ConvertDamageModifiers(wearNTear.m_damages.m_fire)));
            output.Add(new Entry("$almanac_frost", ConvertDamageModifiers(wearNTear.m_damages.m_frost)));
            output.Add(new Entry("$almanac_lightning", ConvertDamageModifiers(wearNTear.m_damages.m_lightning)));
            output.Add(new Entry("$almanac_poison", ConvertDamageModifiers(wearNTear.m_damages.m_poison)));
            output.Add(new Entry("$almanac_spirit", ConvertDamageModifiers(wearNTear.m_damages.m_spirit)));
            output.Add(new Entry("$almanac_hit_noise", wearNTear.m_hitNoise.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_destroy_noise", wearNTear.m_destroyNoise.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_trigger_private_area", ConvertBoolean(wearNTear.m_triggerPrivateArea)));
        }
        
        if (prefab.TryGetComponent(out Smelter smelter))
        {
            output.Add(new Entry("$almanac_smelter_title", "title"));
            output.Add(new Entry("$almanac_fuel_item", smelter.m_fuelItem ? Localization.instance.Localize(smelter.m_fuelItem.m_itemData.m_shared.m_name) : "0"));
            output.Add(new Entry("$almanac_max_ore", smelter.m_maxOre.ToString()));
            output.Add(new Entry("$almanac_max_fuel", smelter.m_maxFuel.ToString()));
            output.Add(new Entry("$almanac_fuel_per_product", smelter.m_fuelPerProduct.ToString()));
            output.Add(new Entry("$almanac_sec_per_product", smelter.m_secPerProduct.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_spawn_stack", smelter.m_spawnStack.ToString()));
            output.Add(new Entry("$almanac_require_roof2", ConvertBoolean(smelter.m_requiresRoof)));
            output.Add(new Entry("$almanac_add_ore_duration", smelter.m_addOreAnimationDuration.ToString(CultureInfo.CurrentCulture)));

            if (smelter.m_conversion.Count > 0)
            {
                output.Add(new Entry("$almanac_conversion_title", "title"));
                output.AddRange(smelter.m_conversion.Select(conversion => 
                    new Entry($"<color=white>{conversion.m_from.m_itemData.m_shared.m_name}</color>", conversion.m_to.m_itemData.m_shared.m_name)
                ));
            }
        }
        if (prefab.TryGetComponent(out Beehive beehive))
        {
            output.Add(new Entry("$almanac_beehive_title", "title"));
            output.Add(new Entry("$almanac_effect_only_day", beehive.m_effectOnlyInDaylight.ToString()));
            output.Add(new Entry("$almanac_max_cover", beehive.m_maxCover.ToString("0.0")));
            output.Add(new Entry("$almanac_sec_per_unit", beehive.m_secPerUnit.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_max_honey", beehive.m_maxHoney.ToString()));
            output.Add(new Entry("$almanac_honey_item", beehive.m_honeyItem ? Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name) : "0"));
            output.AddRange(beehive.m_biome.ToString().Split(',').Select(t => 
                new Entry("$almanac_biomes", SplitCamelCase(t.Replace(" ", "")))
            ));
        }

        if (prefab.TryGetComponent(out Container container))
        {
            output.Add(new Entry("$almanac_container_title", "title"));
            output.Add(new Entry("$almanac_container_size", container.m_width + "<color=orange>x</color>" + container.m_height));
            output.Add(new Entry("$almanac_container_privacy", container.m_privacy.ToString()));
            output.Add(new Entry("$almanac_check_guard", ConvertBoolean(container.m_checkGuardStone)));
            output.Add(new Entry("$almanac_auto_destroy_empty", ConvertBoolean(container.m_autoDestroyEmpty)));
    
            if (container.m_defaultItems.m_drops.Count > 0)
            {
                output.Add(new Entry("$almanac_container_drop", container.m_defaultItems.m_dropMin + "<color=orange>-</color>" + container.m_defaultItems.m_dropMax));
                output.Add(new Entry("$almanac_container_drop_chance", (container.m_defaultItems.m_dropChance * 100).ToString("0.0") + "%"));
                output.Add(new Entry("$almanac_container_one_of_each", container.m_defaultItems.m_oneOfEach.ToString()));
        
                float totalWeight = container.m_defaultItems.m_drops.Sum(item => item.m_weight);
                foreach (var drop in container.m_defaultItems.m_drops)
                {
                    if (!drop.m_item) continue;
                    if (!drop.m_item.TryGetComponent(out ItemDrop itemDrop)) continue;
            
                    string name = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name);
                    string stack = $"<color=orange>(</color>{drop.m_stackMin}<color=orange> $almanac_to </color>{drop.m_stackMax}<color=orange>)</color> ";
                    float percentage = (drop.m_weight / totalWeight) * 100;
                    output.Add(new Entry(name, stack + percentage.ToString("0.0") + "<color=orange>%</color>"));
                }
            }
        }
        if (prefab.TryGetComponent(out CookingStation cookingStation))
        {
            output.Add(new Entry("$almanac_spawn_force", cookingStation.m_spawnForce.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_overcooked_item", cookingStation.m_overCookedItem ? Localization.instance.Localize(cookingStation.m_overCookedItem.m_itemData.m_shared.m_name) : "0"));
            output.Add(new Entry("$almanac_require_fire", ConvertBoolean(cookingStation.m_requireFire)));
            output.Add(new Entry("$almanac_check_radius", cookingStation.m_fireCheckRadius.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_use_fuel", ConvertBoolean(cookingStation.m_useFuel)));
            output.Add(new Entry("$almanac_fuel_item", cookingStation.m_fuelItem ? Localization.instance.Localize(cookingStation.m_fuelItem.m_itemData.m_shared.m_name) : "0"));
            output.Add(new Entry("$almanac_max_fuel", cookingStation.m_maxFuel.ToString()));
            output.Add(new Entry("$almanac_sec_per_fuel", cookingStation.m_secPerFuel.ToString()));

            if (cookingStation.m_conversion.Count > 0)
            {
                output.Add(new Entry("$almanac_conversion_title", "title"));
                output.AddRange(cookingStation.m_conversion.Select(item => 
                    new Entry(item.m_from.m_itemData.m_shared.m_name, $"{item.m_to.m_itemData.m_shared.m_name} <color=orange>(</color>{item.m_cookTime}<color=orange>s)</color>")));
            }
        }

        if (prefab.TryGetComponent(out TeleportWorld teleportWorld))
        {
            output.Add(new Entry("$almanac_portal_title", "title"));
            output.Add(new Entry("$almanac_activation_range", teleportWorld.m_activationRange.ToString("0.0")));
            output.Add(new Entry("$almanac_exit_distance", teleportWorld.m_exitDistance.ToString("0.0")));
        }

        if (prefab.TryGetComponent(out Ship ship))
        {
            output.Add(new Entry("$almanac_ship_title", "title"));
            output.Add(new Entry("$almanac_water_level_offset", ship.m_waterLevelOffset.ToString("0.0")));
            output.Add(new Entry("$almanac_force_distance", ship.m_forceDistance.ToString("0.0")));
            output.Add(new Entry("$almanac_force", ship.m_force.ToString("0.0")));
            output.Add(new Entry("$almanac_damping", ship.m_damping.ToString("0.0")));
            output.Add(new Entry("$almanac_damping_sideway", ship.m_dampingSideway.ToString("0.0")));
            output.Add(new Entry("$almanac_damping_forward", ship.m_dampingForward.ToString("0.0")));
            output.Add(new Entry("$almanac_angular_damping", ship.m_angularDamping.ToString("0.0")));
            output.Add(new Entry("$almanac_disable_level", ship.m_disableLevel.ToString("0.0")));
            output.Add(new Entry("$almanac_sail_force_offset", ship.m_sailForceOffset.ToString("0.0")));
            output.Add(new Entry("$almanac_sail_force_factor", ship.m_sailForceFactor.ToString("0.0")));
            output.Add(new Entry("$almanac_rudder_speed", ship.m_rudderSpeed.ToString("0.0")));
            output.Add(new Entry("$almanac_stear_force_offset", ship.m_stearForceOffset.ToString("0.0")));
            output.Add(new Entry("$almanac_stear_force", ship.m_stearForce.ToString("0.0")));
            output.Add(new Entry("$almanac_stear_velocity_force_factor", ship.m_stearVelForceFactor.ToString("0.0")));
            output.Add(new Entry("$almanac_backward_force", ship.m_backwardForce.ToString("0.0")));
            output.Add(new Entry("$almanac_rudder_rotation_max", ship.m_rudderRotationMax.ToString("0.0")));
            output.Add(new Entry("$almanac_min_water_impact_force", ship.m_minWaterImpactForce.ToString("0.0")));
            output.Add(new Entry("$almanac_min_water_impact_interval", ship.m_minWaterImpactInterval.ToString("0.0") + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_upside_down_damage_interval", ship.m_upsideDownDmgInterval.ToString("0.0") + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_upside_down_damage", ship.m_upsideDownDmg.ToString("0.0")));
        }

        if (prefab.TryGetComponent(out WispSpawner wispSpawner))
        {
            output.Add(new Entry("$almanac_spawn_interval", wispSpawner.m_spawnInterval.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_spawn_chance", (wispSpawner.m_spawnChance * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
            output.Add(new Entry("$almanac_max_spawned", wispSpawner.m_maxSpawned.ToString()));
            output.Add(new Entry("$almanac_only_spawn_night", ConvertBoolean(wispSpawner.m_onlySpawnAtNight)));
            output.Add(new Entry("$almanac_no_spawn_cover", ConvertBoolean(wispSpawner.m_dontSpawnInCover)));
            output.Add(new Entry("$almanac_max_cover", (wispSpawner.m_maxCover * 100).ToString("0.0")));
            output.Add(new Entry("$almanac_wisp_prefab", wispSpawner.m_wispPrefab ? wispSpawner.m_wispPrefab.name : "0"));
            output.Add(new Entry("$almanac_nearby_threshold", wispSpawner.m_nearbyTreshold.ToString("0.0")));
            output.Add(new Entry("$almanac_spawn_distance", wispSpawner.m_spawnDistance.ToString("0.0")));
            output.Add(new Entry("$almanac_max_spawned_area", wispSpawner.m_maxSpawnedArea.ToString(CultureInfo.CurrentCulture)));
        }

        if (prefab.TryGetComponent(out Trap trap))
        {
            output.Add(new Entry("$almanac_rearm_cooldown", trap.m_rearmCooldown + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_triggered_by_enemies", ConvertBoolean(trap.m_triggeredByEnemies)));
            output.Add(new Entry("$almanac_triggered_by_players", ConvertBoolean(trap.m_triggeredByPlayers)));
            output.Add(new Entry("$almanac_force_stagger", ConvertBoolean(trap.m_forceStagger)));
            output.Add(new Entry("$almanac_starts_armed", ConvertBoolean(trap.m_startsArmed)));
        }

        if (prefab.TryGetComponent(out Fireplace fireplace))
        {
            output.Add(new Entry("$almanac_start_fuel", fireplace.m_startFuel.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_max_fuel", fireplace.m_maxFuel.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_sec_per_fuel", fireplace.m_secPerFuel.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_infinite_fuel", ConvertBoolean(fireplace.m_infiniteFuel)));
            output.Add(new Entry("$almanac_fuel_item", fireplace.m_fuelItem ? Localization.instance.Localize(fireplace.m_fuelItem.m_itemData.m_shared.m_name) : "0"));
    
            if (fireplace.m_fireworkItemList.Length > 0)
            {
                output.AddRange(fireplace.m_fireworkItemList
                    .Where(item => item.m_fireworkItem)
                    .Select(item => new Entry("$almanac_firework", item.m_fireworkItem.m_itemData.m_shared.m_name)));
            }
        }

        if (prefab.TryGetComponent(out Door door))
        {
            output.Add(new Entry("$almanac_key_item", door.m_keyItem ? Localization.instance.Localize(door.m_keyItem.m_itemData.m_shared.m_name) : "0"));
            output.Add(new Entry("$almanac_can_not_be_closed", ConvertBoolean(door.m_canNotBeClosed)));
            output.Add(new Entry("$almanac_check_guard", ConvertBoolean(door.m_checkGuardStone)));
        }

        if (prefab.TryGetComponent(out Turret turret))
        {
            output.Add(new Entry("$almanac_turn_rate", turret.m_turnRate.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_horizontal_angle", turret.m_horizontalAngle.ToString(CultureInfo.CurrentCulture) + "<color=orange>°</color>"));
            output.Add(new Entry("$almanac_vertical_angle", turret.m_verticalAngle.ToString(CultureInfo.CurrentCulture) + "<color=orange>°</color>"));
            output.Add(new Entry("$almanac_view_distance", turret.m_viewDistance.ToString("0.0")));
            output.Add(new Entry("$almanac_no_target_scan_rate", turret.m_noTargetScanRate.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_look_acceleration", turret.m_lookAcceleration.ToString("0.0") + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_look_deacceleration", turret.m_lookDeacceleration.ToString("0.0") + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_look_min_degrees_delta", turret.m_lookMinDegreesDelta.ToString("0.0") + "<color=orange>°</color>"));
            output.Add(new Entry("$almanac_default_ammo", turret.m_defaultAmmo ? Localization.instance.Localize(turret.m_defaultAmmo.m_itemData.m_shared.m_name) : "0"));
            output.Add(new Entry("$almanac_attack_cooldown", turret.m_attackCooldown.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_attack_warmup", turret.m_attackWarmup.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_hit_noise1", turret.m_hitNoise.ToString("0.0")));
            output.Add(new Entry("$almanac_shoot_when_aim_diff", turret.m_shootWhenAimDiff.ToString("0.0")));
            output.Add(new Entry("$almanac_prediction_modifier", turret.m_predictionModifier.ToString("0.0")));
            output.Add(new Entry("$almanac_update_target_interval_far", turret.m_updateTargetIntervalFar.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_update_target_interval_near", turret.m_updateTargetIntervalNear.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_max_ammo", turret.m_maxAmmo.ToString()));
            output.Add(new Entry("$almanac_ammo_type", Localization.instance.Localize(turret.m_ammoType)));
            output.Add(new Entry("$almanac_return_ammo_on_destroy", ConvertBoolean(turret.m_returnAmmoOnDestroy)));
            output.Add(new Entry("$almanac_hold_repeat_interval", turret.m_holdRepeatInterval.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_target_players", ConvertBoolean(turret.m_targetPlayers)));
            output.Add(new Entry("$almanac_target_tamed", ConvertBoolean(turret.m_targetTamed)));

            if (turret.m_allowedAmmo.Count > 0)
            {
                output.AddRange(turret.m_allowedAmmo
                    .Where(ammo => ammo.m_ammo)
                    .Select(ammo => new Entry("$almanac_ammo", ammo.m_ammo.m_itemData.m_shared.m_name)));
            }
        }

        if (prefab.TryGetComponent(out Fermenter ferment))
        {
            output.Add(new Entry("$almanac_fermenter_title", "title"));
            output.Add(new Entry("$almanac_duration", ferment.m_fermentationDuration.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
            output.Add(new Entry("$almanac_tap_delay", ferment.m_tapDelay.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));

            if (ferment.m_conversion.Count > 0)
            {
                output.AddRange(ferment.m_conversion.Select(conversion => new Entry(
                    $"<color=white>{conversion.m_from.m_itemData.m_shared.m_name}</color>", 
                    $"{conversion.m_to.m_itemData.m_shared.m_name} <color=orange>(x</color> {conversion.m_producedItems}<color=orange>)</color>"
                )));
            }
        }

        return output;
    }
    
    public static List<Entry> GetItemEntries(ItemDrop.ItemData itemData, ItemDrop itemDrop)
    {
        List<Entry> output = new List<Entry>
        {
            new Entry("$almanac_stack_size_label", itemData.m_stack.ToString()),
            new Entry("$item_durability", itemData.m_durability.ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_variant_label", itemData.m_variant.ToString()),
            new Entry("$almanac_world_level_label", itemData.m_worldLevel.ToString()),
            new Entry("$almanac_item_type_label", ConvertItemType(itemData.m_shared.m_itemType)),
            new Entry("$almanac_max_stack_size_label", itemData.m_shared.m_maxStackSize.ToString()),
            new Entry("$almanac_auto_stack_label", ConvertBoolean(itemData.m_shared.m_autoStack)),
            new Entry("$almanac_quality_label", itemData.m_shared.m_maxQuality.ToString()),
            new Entry("$almanac_scale_by_quality", (itemData.m_shared.m_scaleByQuality * 100).ToString(CultureInfo.CurrentCulture) + "%"),
            new Entry("$almanac_weight_label", itemData.m_shared.m_weight.ToString("0.0")),
            new Entry("$almanac_scale_by_weight", itemData.m_shared.m_scaleWeightByQuality.ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_value_label", itemData.m_shared.m_value.ToString()),
            new Entry("$almanac_teleportable", ConvertBoolean(itemData.m_shared.m_teleportable)),
            new Entry("$almanac_quest_item_label", ConvertBoolean(itemData.m_shared.m_questItem)),
            new Entry("$almanac_equip_duration", itemData.m_shared.m_equipDuration.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"),
            new Entry("$almanac_variant_label", itemData.m_shared.m_variants.ToString())
        };
        GameObject prefab = ObjectDB.instance.GetItemPrefab(itemDrop.name);
        if (!prefab) return output;
        output.Add(prefab.TryGetComponent(out Floating floating)
            ? new Entry("$almanac_floating", "$almanac_true")
            : new Entry("$almanac_floating", "$almanac_false"));

        if (_ShowAllData.Value is Toggle.On)
        {
            output.Add(new Entry("$almanac_prefab_name", prefab.name));
        }

        Skills.SkillType[] allSkills = Skills.s_allSkills;
        switch (itemData.m_shared.m_itemType)
        {
            case ItemDrop.ItemData.ItemType.Fish:
                if (prefab.TryGetComponent(out Fish fish))
                {
                    output.Add(new Entry("$almanac_fish_title", "title")); // You can remove this line if it's a duplicate
                    output.Add(new Entry("$almanac_fish_swim_range", fish.m_swimRange.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_min_depth", fish.m_minDepth.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_speed", fish.m_speed.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_acceleration", fish.m_acceleration.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_turn_rate", fish.m_turnRate.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_avoid_range", fish.m_avoidRange.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_height", fish.m_height.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_hook_force", fish.m_hookForce.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_stamina_use", fish.m_staminaUse.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_escape_stamina_use", fish.m_escapeStaminaUse.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_escape_min", fish.m_escapeMin.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_escape_max", fish.m_escapeMax.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_base_hook_chance", fish.m_baseHookChance.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_escape_time", fish.m_escapeTime.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_jump_speed", fish.m_jumpSpeed.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_jump_height", fish.m_jumpHeight.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_jump_on_land_chance", fish.m_jumpOnLandChance.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_jump_on_land_decay", fish.m_jumpOnLandDecay.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$almanac_fish_jump_frequency", fish.m_jumpFrequencySeconds.ToString("0.0") + "<color=orange>s</color>"));
                    output.Add(new Entry("$almanac_fish_fast", ConvertBoolean(fish.m_fast)));

                    foreach (var bait in fish.m_baits)
                    {
                        output.Add(new Entry("$almanac_fish_bait_name", bait.m_bait.m_itemData.m_shared.m_name));
                        output.Add(new Entry("$almanac_fish_bait_chance", bait.m_chance.ToString(CultureInfo.CurrentCulture)));
                    }
                }

                break;
            case ItemDrop.ItemData.ItemType.Helmet:
            case ItemDrop.ItemData.ItemType.Legs:
            case ItemDrop.ItemData.ItemType.Chest:
            case ItemDrop.ItemData.ItemType.Customization:
            case ItemDrop.ItemData.ItemType.Shoulder:
                output.Add(new Entry("$almanac_armor_title", "title"));
                output.Add(new Entry("$almanac_armor_label", $"{itemData.m_shared.m_armor.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_armorPerLevel.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$almanac_armor_material", itemData.m_shared.m_armorMaterial && _ShowAllData.Value is Toggle.On ? RemoveParentheses(itemData.m_shared.m_armorMaterial.ToString()) : "0"));
                output.Add(new Entry("$almanac_helmet_hide_hair", SplitCamelCase(itemData.m_shared.m_helmetHideHair.ToString())));
                output.Add(new Entry("$almanac_helmet_hide_beard", SplitCamelCase(itemData.m_shared.m_helmetHideBeard.ToString())));

                if (!itemData.m_shared.m_setName.IsNullOrWhiteSpace())
                {
                    output.Add(new Entry("$almanac_set_title", "title"));
                    output.Add(new Entry("$almanac_set_name", itemData.m_shared.m_setName));
                    output.Add(new Entry("$almanac_set_size", itemData.m_shared.m_setSize.ToString()));
                    output.Add(new Entry("$almanac_set_status_effect", itemData.m_shared.m_setStatusEffect ? Localization.instance.Localize(itemData.m_shared.m_setStatusEffect.m_name) : "0"));
                    output.Add(new Entry("$almanac_set_tooltip", itemData.m_shared.m_setStatusEffect ? Localization.instance.Localize(itemData.m_shared.m_setStatusEffect.m_tooltip) : "0"));
                }

                if (itemData.m_shared.m_damageModifiers.Count > 0)
                {
                    output.AddRange(itemData.m_shared.m_damageModifiers.Select(mod => 
                        new Entry(ConvertDamageTypes(mod.m_type), ConvertDamageModifiers(mod.m_modifier))));
                }

                if (itemData.m_shared.m_setStatusEffect)
                {
                    foreach (Skills.SkillType skill in allSkills)
                    {
                        float amount = 0f; 
                        itemData.m_shared.m_setStatusEffect.ModifySkillLevel(skill, ref amount);
                        if (amount <= 0) continue;
                        output.Add(new Entry(ConvertSkills(skill), $"<color=orange>+</color>{amount}"));
                    }
                }
                
                if (itemData.m_shared.m_equipStatusEffect)
                {
                    StatusEffect equipEffect = itemData.m_shared.m_equipStatusEffect;
                    output.Add(new Entry("$almanac_equip_effects", "title"));
                    output.Add(new Entry("$almanac_equip_status_effect", equipEffect.m_name));

                    foreach (Skills.SkillType skill in allSkills)
                    {
                        float skillLevel = 0f;
                        equipEffect.ModifySkillLevel(skill, ref skillLevel);
                        float raiseLevel = 0f;
                        equipEffect.ModifyRaiseSkill(skill, ref raiseLevel);

                        if (skillLevel > 0)
                        {
                            output.Add(new Entry(ConvertSkills(skill), $"<color=orange>+</color>{skillLevel}"));
                        }

                        if (raiseLevel > 0)
                        {
                            output.Add(new Entry(ConvertSkills(skill), $"<color=orange>+</color>{raiseLevel}"));
                        }
                    }

                    float fallDamage = 0f;
                    equipEffect.ModifyFallDamage(40f, ref fallDamage);
                    float healthRegen = 0f;
                    equipEffect.ModifyHealthRegen(ref healthRegen);
                    float staminaRegen = 0f;
                    equipEffect.ModifyStaminaRegen(ref staminaRegen);
                    float eitrRegen = 0f;
                    equipEffect.ModifyEitrRegen(ref eitrRegen);

                    if (fallDamage > 0) 
                        output.Add(new Entry("$almanac_fall_damage", fallDamage.ToString(CultureInfo.CurrentCulture)));
                    if (healthRegen > 0) 
                        output.Add(new Entry("$se_healthregen", healthRegen.ToString(CultureInfo.CurrentCulture)));
                    if (staminaRegen > 0) 
                        output.Add(new Entry("$se_staminaregen", staminaRegen.ToString(CultureInfo.CurrentCulture)));
                    if (eitrRegen > 0) 
                        output.Add(new Entry("$se_eitrregen", eitrRegen.ToString(CultureInfo.CurrentCulture)));
                    
                    HitData.DamageModifiers modifiers = new();
                    equipEffect.ModifyDamageMods(ref modifiers);

                    output.Add(new Entry("$inventory_blunt", ConvertDamageModifiers(modifiers.m_blunt)));
                    output.Add(new Entry("$inventory_slash", ConvertDamageModifiers(modifiers.m_slash)));
                    output.Add(new Entry("$inventory_pierce", ConvertDamageModifiers(modifiers.m_pierce)));
                    output.Add(new Entry("$inventory_chop", ConvertDamageModifiers(modifiers.m_chop)));
                    output.Add(new Entry("$inventory_pickaxe", ConvertDamageModifiers(modifiers.m_pickaxe)));
                    output.Add(new Entry("$inventory_fire", ConvertDamageModifiers(modifiers.m_fire)));
                    output.Add(new Entry("$inventory_frost", ConvertDamageModifiers(modifiers.m_frost)));
                    output.Add(new Entry("$inventory_lightning", ConvertDamageModifiers(modifiers.m_lightning)));
                    output.Add(new Entry("$inventory_poison", ConvertDamageModifiers(modifiers.m_poison)));
                    output.Add(new Entry("$inventory_spirit", ConvertDamageModifiers(modifiers.m_spirit)));

                    if (itemData.m_shared.m_movementModifier > 0f 
                        || itemData.m_shared.m_eitrRegenModifier > 0f 
                        || itemData.m_shared.m_homeItemsStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$almanac_stat_modifiers_title", "title"));
                        output.Add(new Entry("$item_movement_modifier", (itemData.m_shared.m_movementModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                        output.Add(new Entry("$item_eitrregen_modifier", (itemData.m_shared.m_eitrRegenModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                        output.Add(new Entry("$almanac_base_items_stamina_modifier_label", (itemData.m_shared.m_homeItemsStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_heatResistanceModifier > 0f)
                    {
                        output.Add(new Entry("$item_heat_modifier", (itemData.m_shared.m_heatResistanceModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_jumpStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_jumpstamina", (itemData.m_shared.m_jumpStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_attackStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_attackstamina", (itemData.m_shared.m_attackStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_blockStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_blockstamina", (itemData.m_shared.m_blockStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_dodgeStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_dodgestamina", (itemData.m_shared.m_dodgeStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_swimStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_swimstamina", (itemData.m_shared.m_swimStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_sneakStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_sneakstamina", (itemData.m_shared.m_sneakStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                    if (itemData.m_shared.m_runStaminaModifier > 0f)
                    {
                        output.Add(new Entry("$se_runstamina", (itemData.m_shared.m_runStaminaModifier * 100).ToString(CultureInfo.CurrentCulture) + "<color=orange>%</color>"));
                    }

                }
                break;
            case ItemDrop.ItemData.ItemType.Consumable:
                bool addedFoodTitle = false;
                if (itemData.m_shared.m_food + itemData.m_shared.m_foodStamina + itemData.m_shared.m_foodBurnTime +
                    itemData.m_shared.m_foodRegen != 0)
                {
                    addedFoodTitle = true;
                    output.Add(new Entry("$almanac_food_title", "title"));
                    output.Add(new Entry("$item_food_health", itemData.m_shared.m_food.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$item_food_stamina", itemData.m_shared.m_foodStamina.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$item_food_eitr", itemData.m_shared.m_foodEitr.ToString(CultureInfo.CurrentCulture)));
                    output.Add(new Entry("$item_food_duration", itemData.m_shared.m_foodBurnTime.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));
                    output.Add(new Entry("$item_food_regen", itemData.m_shared.m_foodRegen.ToString(CultureInfo.CurrentCulture) + "<color=orange>/tick</color>"));
                }

                if (itemData.m_shared.m_consumeStatusEffect)
                {
                    if (!addedFoodTitle) 
                    {
                        output.Add(new Entry("$almanac_food_title", "title"));
                    }

                    StatusEffect consumeEffect = itemData.m_shared.m_consumeStatusEffect;
                    output.Add(new Entry("$almanac_consume_effect", Localization.instance.Localize(consumeEffect.m_name)));
                    output.Add(new Entry("$almanac_consume_category", consumeEffect.m_category));
                    output.Add(new Entry("$almanac_consume_tooltip", Localization.instance.Localize(consumeEffect.m_tooltip)));
                    output.Add(new Entry("$almanac_consume_attributes", SplitCamelCase(consumeEffect.m_attributes.ToString())));
                    output.Add(new Entry("$almanac_consume_message", Localization.instance.Localize(consumeEffect.m_startMessage)));
                    output.Add(new Entry("$almanac_consume_duration", consumeEffect.m_ttl.ToString(CultureInfo.CurrentCulture) + "<color=orange>s</color>"));

                    foreach (Skills.SkillType skill in allSkills)
                    {
                        float skillLevel = 0f;
                        consumeEffect.ModifySkillLevel(skill, ref skillLevel);
                        if (skillLevel > 0) 
                        {
                            output.Add(new Entry(ConvertSkills(skill), "<color=orange>+</color>" + skillLevel.ToString(CultureInfo.CurrentCulture)));
                        }

                        float raiseLevel = 0f;
                        consumeEffect.ModifyRaiseSkill(skill, ref raiseLevel);
                        if (raiseLevel > 0) 
                        {
                            output.Add(new Entry(ConvertSkills(skill), "<color=orange>+</color>" + raiseLevel.ToString(CultureInfo.CurrentCulture)));
                        }
                    }

                    float healthRegen = 0f;
                    consumeEffect.ModifyHealthRegen(ref healthRegen);
                    if (healthRegen > 0) 
                    {
                        output.Add(new Entry("$almanac_consume_health_regen", (healthRegen * 100).ToString("0.0") + "<color=orange>%</color>"));
                    }

                    float staminaRegen = 0f;  // Initialize to 0
                    consumeEffect.ModifyStaminaRegen(ref staminaRegen);
                    if (staminaRegen > 0) 
                    {
                        output.Add(new Entry("$almanac_consume_stamina_regen", (staminaRegen * 100).ToString("0.0") + "<color=orange>%</color>"));
                    }

                    float eitrRegen = 0f;
                    consumeEffect.ModifyEitrRegen(ref eitrRegen);
                    if (eitrRegen > 0) 
                    {
                        output.Add(new Entry("$almanac_consume_eitr_regen", (eitrRegen * 100).ToString("0.0") + "<color=orange>%</color>"));
                    }


                    HitData.DamageModifiers modifiers = new();
                    consumeEffect.ModifyDamageMods(ref modifiers);

                    output.Add(new Entry("$inventory_blunt", ConvertDamageModifiers(modifiers.m_blunt)));
                    output.Add(new Entry("$inventory_slash", ConvertDamageModifiers(modifiers.m_slash)));
                    output.Add(new Entry("$inventory_pierce", ConvertDamageModifiers(modifiers.m_pierce)));
                    output.Add(new Entry("$inventory_chop", ConvertDamageModifiers(modifiers.m_chop)));
                    output.Add(new Entry("$inventory_pickaxe", ConvertDamageModifiers(modifiers.m_pickaxe)));
                    output.Add(new Entry("$inventory_fire", ConvertDamageModifiers(modifiers.m_fire)));
                    output.Add(new Entry("$inventory_frost", ConvertDamageModifiers(modifiers.m_frost)));
                    output.Add(new Entry("$inventory_lightning", ConvertDamageModifiers(modifiers.m_lightning)));
                    output.Add(new Entry("$inventory_poison", ConvertDamageModifiers(modifiers.m_poison)));
                    output.Add(new Entry("$inventory_spirit", ConvertDamageModifiers(modifiers.m_spirit)));

                }
                break;
            case ItemDrop.ItemData.ItemType.Shield:
                output.Add(new Entry("$almanac_shield_title", "title"));
                output.Add(new Entry("$almanac_shield_block_power", itemData.m_shared.m_blockPower.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_shield_block_power_per_level", itemData.m_shared.m_blockPowerPerLevel.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_shield_deflection_force", itemData.m_shared.m_deflectionForce.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_shield_deflection_force_per_level", itemData.m_shared.m_deflectionForcePerLevel.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_timed_block_bonus", itemData.m_shared.m_timedBlockBonus.ToString(CultureInfo.CurrentCulture)));
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
                output.Add(new Entry("$almanac_weapon_title", "title"));
                output.Add(new Entry("$almanac_weapon_animation_state", SplitCamelCase(itemData.m_shared.m_animationState.ToString())));
                output.Add(new Entry("$almanac_weapon_skill_type", ConvertSkills(itemData.m_shared.m_skillType)));
                output.Add(new Entry("$almanac_tool_tier", itemData.m_shared.m_toolTier.ToString()));

                output.Add(new Entry("$inventory_damage", $"{itemData.m_shared.m_damages.m_damage.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_damage.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_blunt", $"{itemData.m_shared.m_damages.m_blunt.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_blunt.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_slash", $"{itemData.m_shared.m_damages.m_slash.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_slash.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_pierce", $"{itemData.m_shared.m_damages.m_pierce.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_pierce.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_chop", $"{itemData.m_shared.m_damages.m_chop.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_chop.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_pickaxe", $"{itemData.m_shared.m_damages.m_pickaxe.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_pickaxe.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_fire", $"{itemData.m_shared.m_damages.m_fire.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_fire.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_frost", $"{itemData.m_shared.m_damages.m_frost.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_frost.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_lightning", $"{itemData.m_shared.m_damages.m_lightning.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_lightning.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_poison", $"{itemData.m_shared.m_damages.m_poison.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_poison.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));
                output.Add(new Entry("$inventory_spirit", $"{itemData.m_shared.m_damages.m_spirit.ToString(CultureInfo.CurrentCulture)} <color=orange>+</color> {itemData.m_shared.m_damagesPerLevel.m_spirit.ToString(CultureInfo.CurrentCulture)} <color=orange>/lvl</color>"));

                output.Add(new Entry("$almanac_attack_force", itemData.m_shared.m_attackForce.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_back_stab_bonus", itemData.m_shared.m_backstabBonus.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_dodgeable", ConvertBoolean(itemData.m_shared.m_dodgeable)));
                output.Add(new Entry("$almanac_blockable", ConvertBoolean(itemData.m_shared.m_blockable)));
                output.Add(new Entry("$almanac_tame_only", ConvertBoolean(itemData.m_shared.m_tamedOnly)));
                output.Add(new Entry("$almanac_always_rotate", ConvertBoolean(itemData.m_shared.m_alwaysRotate)));
                output.Add(new Entry("$almanac_attack_effect", itemData.m_shared.m_attackStatusEffect ? RemoveParentheses(itemData.m_shared.m_attackStatusEffect.ToString()) : "0"));
                output.Add(new Entry("$item_chancetoapplyse", itemData.m_shared.m_attackStatusEffect ? $"{(itemData.m_shared.m_attackStatusEffectChance * 100).ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>" : "100"));
                output.Add(new Entry("$almanac_spawn_on_hit", itemData.m_shared.m_spawnOnHit ? RemoveParentheses(itemData.m_shared.m_spawnOnHit.ToString()) : "0"));
                output.Add(new Entry("$almanac_spawn_on_hit_terrain", itemData.m_shared.m_spawnOnHitTerrain ? RemoveParentheses(itemData.m_shared.m_spawnOnHitTerrain.ToString()) : "0"));
                output.Add(new Entry("$almanac_projectile_tooltip", ConvertBoolean(itemData.m_shared.m_projectileToolTip)));
                output.Add(new Entry("$almanac_ammo_type", Localization.instance.Localize(itemData.m_shared.m_ammoType)));

                output.Add(new Entry("$almanac_attack_label", "title"));
                output.Add(new Entry("$almanac_attack_type", ConvertAttackTypes(itemData.m_shared.m_attack.m_attackType)));
                output.Add(new Entry("$almanac_attack_animation", itemData.m_shared.m_attack.m_attackAnimation));
                output.Add(new Entry("$almanac_attack_random_animations", itemData.m_shared.m_attack.m_attackRandomAnimations.ToString()));
                output.Add(new Entry("$almanac_attack_chain_levels", itemData.m_shared.m_attack.m_attackChainLevels.ToString()));
                output.Add(new Entry("$almanac_attack_looping", itemData.m_shared.m_attack.m_loopingAttack.ToString()));
                output.Add(new Entry("$almanac_consume_item", itemData.m_shared.m_attack.m_consumeItem.ToString()));
                output.Add(new Entry("$almanac_hit_terrain", itemData.m_shared.m_attack.m_hitTerrain.ToString()));
                output.Add(new Entry("$almanac_is_home_item", itemData.m_shared.m_attack.m_isHomeItem.ToString()));
                output.Add(new Entry("$item_staminause", itemData.m_shared.m_attack.m_attackStamina.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_eitruse", itemData.m_shared.m_attack.m_attackEitr.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_healthuse", itemData.m_shared.m_attack.m_attackHealth.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_healthhitreturn", itemData.m_shared.m_attack.m_attackHealthReturnHit.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_healthuse_percentage", $"{itemData.m_shared.m_attack.m_attackHealthPercentage.ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>"));
                output.Add(new Entry("$item_staminahold", itemData.m_shared.m_attack.m_drawStaminaDrain.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_speed_factor", $"{(itemData.m_shared.m_attack.m_speedFactor * 100).ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>"));
                output.Add(new Entry("$almanac_speed_factor_rotation", $"{(itemData.m_shared.m_attack.m_speedFactorRotation * 100).ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>"));
                output.Add(new Entry("$almanac_attack_start_noise", itemData.m_shared.m_attack.m_attackStartNoise.ToString("0.0")));
                
                output.Add(new Entry("$almanac_secondary_attack_label", "title"));
                output.Add(new Entry("$almanac_attack_type", ConvertAttackTypes(itemData.m_shared.m_secondaryAttack.m_attackType)));
                output.Add(new Entry("$almanac_attack_animation", itemData.m_shared.m_secondaryAttack.m_attackAnimation));
                output.Add(new Entry("$almanac_attack_random_animations", itemData.m_shared.m_secondaryAttack.m_attackRandomAnimations.ToString()));
                output.Add(new Entry("$almanac_attack_chain_levels", itemData.m_shared.m_secondaryAttack.m_attackChainLevels.ToString()));
                output.Add(new Entry("$almanac_attack_looping", itemData.m_shared.m_secondaryAttack.m_loopingAttack.ToString()));
                output.Add(new Entry("$almanac_consume_item", itemData.m_shared.m_secondaryAttack.m_consumeItem.ToString()));
                output.Add(new Entry("$almanac_hit_terrain", itemData.m_shared.m_secondaryAttack.m_hitTerrain.ToString()));
                output.Add(new Entry("$almanac_is_home_item", itemData.m_shared.m_secondaryAttack.m_isHomeItem.ToString()));
                output.Add(new Entry("$item_staminause", itemData.m_shared.m_secondaryAttack.m_attackStamina.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_eitruse", itemData.m_shared.m_secondaryAttack.m_attackEitr.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_healthuse", itemData.m_shared.m_secondaryAttack.m_attackHealth.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_healthhitreturn", itemData.m_shared.m_secondaryAttack.m_attackHealthReturnHit.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$item_healthuse_percentage", $"{itemData.m_shared.m_secondaryAttack.m_attackHealthPercentage.ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>"));
                output.Add(new Entry("$item_staminahold", itemData.m_shared.m_secondaryAttack.m_drawStaminaDrain.ToString(CultureInfo.CurrentCulture)));
                output.Add(new Entry("$almanac_speed_factor", $"{(itemData.m_shared.m_secondaryAttack.m_speedFactor * 100).ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>"));
                output.Add(new Entry("$almanac_speed_factor_rotation", $"{(itemData.m_shared.m_secondaryAttack.m_speedFactorRotation * 100).ToString(CultureInfo.CurrentCulture)}<color=orange>%</color>"));
                output.Add(new Entry("$almanac_attack_start_noise", itemData.m_shared.m_secondaryAttack.m_attackStartNoise.ToString("0.0")));

                break;
        }

        return output;
    }
    
    public static List<Entry> GetCreatureEntries(CreatureDataCollector.CreatureData data)
    {
        List<Entry> output = new()
        {
            new Entry("$se_health", data.health.ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_faction", SplitCamelCase(data.faction)),
            new Entry("$almanac_creature_resistances", "title"),
            new Entry("$inventory_blunt", SplitCamelCase(data.blunt)),
            new Entry("$inventory_slash", SplitCamelCase(data.slash)),
            new Entry("$inventory_pierce", SplitCamelCase(data.pierce)),
            new Entry("$inventory_chop", SplitCamelCase(data.chop)),
            new Entry("$inventory_pickaxe", SplitCamelCase(data.pickaxe)),
            new Entry("$inventory_fire", SplitCamelCase(data.fire)),
            new Entry("$inventory_frost", SplitCamelCase(data.frost)),
            new Entry("$inventory_lightning", SplitCamelCase(data.lightning)),
            new Entry("$inventory_poison", SplitCamelCase(data.poison)),
            new Entry("$inventory_spirit", SplitCamelCase(data.spirit))
        };

        
        int kill = LocalPlayerData.Player_Kill_Deaths[data.defeatedKey].kills;
        int deaths = LocalPlayerData.Player_Kill_Deaths[data.defeatedKey].deaths;

        if (kill + deaths > 0)
        {
            output.Add(new Entry("$almanac_kill_death_title", "title"));
            output.Add(new Entry("$almanac_kill_count", kill.ToString()));
            output.Add(new Entry("$almanac_death_count", deaths.ToString()));

            if (kill > 0 && deaths > 0)
            {
                float kd = Mathf.Floor((float)kill / deaths);
                output.Add(new Entry("$almanac_kill_death_ratio", kd.ToString(CultureInfo.CurrentCulture)));
            }
        }

        return output;
    }
    
    public static List<Entry> GetCreatureEntries_1(CreatureDataCollector.CreatureData data)
    {
        List<Entry> output = new()
        {
            new Entry("$almanac_avoid_fire", ConvertBoolean(data.avoidFire)),
            new Entry("$almanac_afraid_of_fire", ConvertBoolean(data.afraidOfFire)),
            new Entry("$almanac_avoid_water", ConvertBoolean(data.avoidWater))
        };

        foreach (var spot in data.weakSpot)
        {
            output.Add(new Entry("$almanac_weak_spot", spot));
        }

        output.Add(new Entry("$almanac_stagger_when_blocked", ConvertBoolean(data.staggerWhenBlocked)));
        output.Add(new Entry("$almanac_stagger_damage_factor", data.staggerDamageFactor.ToString("0.0")));
        output.Add(new Entry("$almanac_tolerate_water", ConvertBoolean(data.tolerateWater)));
        output.Add(new Entry("$almanac_tolerate_smoke", ConvertBoolean(data.tolerateSmoke)));
        output.Add(new Entry("$almanac_tolerate_tar", ConvertBoolean(data.tolerateTar)));
        output.Add(new Entry("$almanac_defeat_key", data.defeatedKey));

        foreach (var attack in data.defaultItems)
        {
            if (attack.name == "Unknown") continue;

            output.Add(new Entry("$almanac_creature_attacks", "title"));
            output.Add(new Entry("$almanac_attack_name", attack.name));
            output.Add(new Entry("$almanac_damage", attack.damage.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_blunt", attack.blunt.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_slash", attack.slash.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_pierce", attack.pierce.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_chop", attack.chop.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_pickaxe", attack.pickaxe.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_fire", attack.fire.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_frost", attack.frost.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_lightning", attack.lightning.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_poison", attack.poison.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$inventory_spirit", attack.spirit.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$item_knockback", attack.attackForce.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$item_backstab", attack.backStabBonus.ToString(CultureInfo.CurrentCulture)));
            output.Add(new Entry("$almanac_dodgeable", ConvertBoolean(attack.dodgeable)));
            output.Add(new Entry("$almanac_blockable", ConvertBoolean(attack.blockable)));
            output.Add(new Entry("$almanac_status_effect", attack.statusEffect));
            output.Add(new Entry("$almanac_status_effect_tooltip", attack.statusEffectTooltip));
        }

        return output;
    }

    public static List<Entry> GetPlayerResistances()
    {
        if (!Player.m_localPlayer) return new List<Entry>();

        HitData.DamageModifiers modifiers = Player.m_localPlayer.GetDamageModifiers(null);
        return new List<Entry>
        {
            new Entry("$almanac_player_resistances", "title"),
            new Entry(ConvertDamageTypes(HitData.DamageType.Blunt), ConvertDamageModifiers(modifiers.m_blunt)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Slash), ConvertDamageModifiers(modifiers.m_slash)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Pierce), ConvertDamageModifiers(modifiers.m_pierce)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Chop), ConvertDamageModifiers(modifiers.m_chop)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Pickaxe), ConvertDamageModifiers(modifiers.m_pickaxe)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Fire), ConvertDamageModifiers(modifiers.m_fire)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Frost), ConvertDamageModifiers(modifiers.m_frost)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Lightning), ConvertDamageModifiers(modifiers.m_lightning)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Poison), ConvertDamageModifiers(modifiers.m_poison)),
            new Entry(ConvertDamageTypes(HitData.DamageType.Spirit), ConvertDamageModifiers(modifiers.m_spirit))
        };

    }

    public static List<Entry> GetTotalAchievementEffects()
    {
        if (!Player.m_localPlayer) return new();
        List<Entry> result = new();
        Dictionary<AlmanacEffectManager.Modifier, float> modifiers = new();
        HitData.DamageModifiers mods = new HitData.DamageModifiers();
        Dictionary<Skills.SkillType, float> skills = new();
        foreach (Skills.SkillType type in Enum.GetValues(typeof(Skills.SkillType))) skills[type] = 0;
        foreach (AlmanacEffectManager.Modifier type in Enum.GetValues(typeof(AlmanacEffectManager.Modifier))) modifiers[type] = 0;
        int count = 0;
        foreach (StatusEffect? statusEffect in Player.m_localPlayer.GetSEMan().GetStatusEffects())
        {
            if (statusEffect is not AlmanacEffectManager.AchievementEffect achievementEffect) continue;
            ++count;
            foreach (KeyValuePair<AlmanacEffectManager.Modifier, float> kvp in achievementEffect.data.m_modifiers)
            {
                switch (kvp.Key)
                {
                    case AlmanacEffectManager.Modifier.MaxCarryWeight
                        or AlmanacEffectManager.Modifier.Health
                        or AlmanacEffectManager.Modifier.Eitr
                        or AlmanacEffectManager.Modifier.Stamina
                        or AlmanacEffectManager.Modifier.Armor:
                        modifiers[kvp.Key] += kvp.Value;
                        break;
                    case AlmanacEffectManager.Modifier.DamageReduction:
                        modifiers[kvp.Key] += Mathf.Clamp01(1f - kvp.Value) * 100f - 100;
                        break;
                    default:
                        modifiers[kvp.Key] += kvp.Value * 100f - 100;
                        break;  
                }
            }
            mods.Apply(achievementEffect.data.damageMods);
            foreach (var kvp in achievementEffect.data.m_skills)
            {
                skills[kvp.Key] += kvp.Value;
            }
        }
        result.Add(new Entry("$almanac_statuseffects", "title"));
        result.Add(new Entry("$almanac_total_effects", count.ToString()));

        foreach (KeyValuePair<AlmanacEffectManager.Modifier, float> kvp in modifiers)
        {
            if (kvp.Key is AlmanacEffectManager.Modifier.None) continue;
            if (kvp.Value == 0) continue;
            switch (kvp.Key)
            {
                case AlmanacEffectManager.Modifier.MaxCarryWeight 
                    or AlmanacEffectManager.Modifier.Health 
                    or AlmanacEffectManager.Modifier.Stamina 
                    or AlmanacEffectManager.Modifier.Eitr
                    or AlmanacEffectManager.Modifier.Armor:
                    result.Add(new Entry(ConvertEffectModifiers(kvp.Key), $"{kvp.Value:+0;-0}\n"));
                    break;
                default:
                    result.Add(new Entry(ConvertEffectModifiers(kvp.Key), $"{kvp.Value:+0;-0}%\n"));
                    break;
            }
        }
        if (mods.m_blunt is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Blunt), ConvertDamageModifiers(mods.m_blunt)));

        if (mods.m_slash is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Slash), ConvertDamageModifiers(mods.m_slash)));

        if (mods.m_pierce is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Pierce), ConvertDamageModifiers(mods.m_pierce)));

        if (mods.m_chop is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Chop), ConvertDamageModifiers(mods.m_chop)));

        if (mods.m_pickaxe is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Pickaxe), ConvertDamageModifiers(mods.m_pickaxe)));

        if (mods.m_fire is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Fire), ConvertDamageModifiers(mods.m_fire)));

        if (mods.m_frost is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Frost), ConvertDamageModifiers(mods.m_frost)));

        if (mods.m_lightning is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Lightning), ConvertDamageModifiers(mods.m_lightning)));

        if (mods.m_poison is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Poison), ConvertDamageModifiers(mods.m_poison)));

        if (mods.m_spirit is not HitData.DamageModifier.Normal)
            result.Add(new Entry(ConvertDamageTypes(HitData.DamageType.Spirit), ConvertDamageModifiers(mods.m_spirit)));
        
        foreach (KeyValuePair<Skills.SkillType, float> kvp in skills)
        {
            if (kvp.Key is Skills.SkillType.None || kvp.Value == 0) continue;
            result.Add(new Entry("$skill_" + kvp.Key.ToString().ToLower(), kvp.Value.ToString("0")));
        }

        return result;
    }
    public static List<Entry> GetMetricEntries()
    {
        return new()
        {
            new Entry("$almanac_kill_title", "title"),
            new Entry("$almanac_enemy_kills", GetPlayerStat(PlayerStatType.EnemyKills).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_player_kills", GetPlayerStat(PlayerStatType.PlayerKills).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_hits_taken_enemies", GetPlayerStat(PlayerStatType.HitsTakenEnemies).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_total_deaths", GetPlayerStat(PlayerStatType.Deaths).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_enemy_hits", GetPlayerStat(PlayerStatType.EnemyHits).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_enemy_last_hit", GetPlayerStat(PlayerStatType.EnemyKillsLastHits).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_player_kills", GetPlayerStat(PlayerStatType.PlayerKills).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_craft_or_upgrade_count", GetPlayerStat(PlayerStatType.CraftsOrUpgrades).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_builds", GetPlayerStat(PlayerStatType.Builds).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_crafts", GetPlayerStat(PlayerStatType.Crafts).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_upgrades", GetPlayerStat(PlayerStatType.Upgrades).ToString(CultureInfo.CurrentCulture)),

            new Entry("$almanac_misc_title", "title"),
            new Entry("$almanac_skeleton_summoned", GetPlayerStat(PlayerStatType.SkeletonSummons).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_item_picked_up", GetPlayerStat(PlayerStatType.ItemsPickedUp).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_jumps", GetPlayerStat(PlayerStatType.Jumps).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_portal_used", GetPlayerStat(PlayerStatType.PortalsUsed).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_cheats", GetPlayerStat(PlayerStatType.Cheats).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_world_loads", GetPlayerStat(PlayerStatType.WorldLoads).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_creature_tame", GetPlayerStat(PlayerStatType.CreatureTamed).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_arrows_shot", GetPlayerStat(PlayerStatType.ArrowsShot).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_doors_opened", GetPlayerStat(PlayerStatType.DoorsOpened).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_doors_closed", GetPlayerStat(PlayerStatType.DoorsClosed).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_bees_harvested", GetPlayerStat(PlayerStatType.BeesHarvested).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_sap_harvested", GetPlayerStat(PlayerStatType.SapHarvested).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_turret_ammo_added", GetPlayerStat(PlayerStatType.TurretAmmoAdded).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_turret_trophy_set", GetPlayerStat(PlayerStatType.TurretTrophySet).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_trap_armed", GetPlayerStat(PlayerStatType.TrapArmed).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_trap_triggered", GetPlayerStat(PlayerStatType.TrapTriggered).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_place_stacks", GetPlayerStat(PlayerStatType.PlaceStacks).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_portal_dungeon_in", GetPlayerStat(PlayerStatType.PortalDungeonIn).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_portal_dungeon_out", GetPlayerStat(PlayerStatType.PortalDungeonOut).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_total_boss_kills", GetPlayerStat(PlayerStatType.BossKills).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_boss_last_hits", GetPlayerStat(PlayerStatType.BossLastHits).ToString(CultureInfo.CurrentCulture)),
            
            new Entry("$almanac_distance_title", "title"),
            new Entry("$almanac_distanced_traveled", GetPlayerStat(PlayerStatType.DistanceTraveled).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_distance_walked", GetPlayerStat(PlayerStatType.DistanceWalk).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_distance_ran", GetPlayerStat(PlayerStatType.DistanceRun).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_distance_sailed", GetPlayerStat(PlayerStatType.DistanceSail).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_distance_air", GetPlayerStat(PlayerStatType.DistanceAir).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_base_title", "title"),
            new Entry("$almanac_time_in_base", (GetPlayerStat(PlayerStatType.TimeInBase) / 60).ToString(CultureInfo.CurrentCulture) + "<color=orange>min</color>"),
            new Entry("$almanac_time_out_base", (GetPlayerStat(PlayerStatType.TimeOutOfBase) / 60).ToString(CultureInfo.CurrentCulture) + "<color=orange>min</color>"),
            new Entry("$almanac_sleep", GetPlayerStat(PlayerStatType.Sleep).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_stands_title", "title"),
            new Entry("$almanac_item_stand_used", GetPlayerStat(PlayerStatType.ItemStandUses).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_armor_stand_used", GetPlayerStat(PlayerStatType.ArmorStandUses).ToString(CultureInfo.CurrentCulture)),

            new Entry("$almanac_trees_title", "title"),
            new Entry("$almanac_tree_chopped", GetPlayerStat(PlayerStatType.TreeChops).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree", GetPlayerStat(PlayerStatType.Tree).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_0", GetPlayerStat(PlayerStatType.TreeTier0).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_1", GetPlayerStat(PlayerStatType.TreeTier1).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_2", GetPlayerStat(PlayerStatType.TreeTier2).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_3", GetPlayerStat(PlayerStatType.TreeTier3).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_4", GetPlayerStat(PlayerStatType.TreeTier4).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_5", GetPlayerStat(PlayerStatType.TreeTier5).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_log_chopped", GetPlayerStat(PlayerStatType.LogChops).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_logs", GetPlayerStat(PlayerStatType.Logs).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_ore_title", "title"),
            new Entry("$almanac_mine_hits", GetPlayerStat(PlayerStatType.MineHits).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mines", GetPlayerStat(PlayerStatType.Mines).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_0", GetPlayerStat(PlayerStatType.MineTier0).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_1", GetPlayerStat(PlayerStatType.MineTier1).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_2", GetPlayerStat(PlayerStatType.MineTier2).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_3", GetPlayerStat(PlayerStatType.MineTier3).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_4", GetPlayerStat(PlayerStatType.MineTier4).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_5", GetPlayerStat(PlayerStatType.MineTier5).ToString(CultureInfo.CurrentCulture)),

            new Entry("$almanac_raven_title", "title"),
            new Entry("$almanac_raven_hits", GetPlayerStat(PlayerStatType.RavenHits).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_raven_talk", GetPlayerStat(PlayerStatType.RavenTalk).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_raven_appear", GetPlayerStat(PlayerStatType.RavenAppear).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_food_eaten", GetPlayerStat(PlayerStatType.FoodEaten).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tombstone_title", "title"),
            new Entry("$almanac_tombstones_open_own", GetPlayerStat(PlayerStatType.TombstonesOpenedOwn).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tombstone_open_other", GetPlayerStat(PlayerStatType.TombstonesOpenedOther).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tombstone_fit", GetPlayerStat(PlayerStatType.TombstonesFit).ToString(CultureInfo.CurrentCulture)),

            new Entry("$almanac_death_title", "title"),
            new Entry("$almanac_death_by_undefined", GetPlayerStat(PlayerStatType.DeathByUndefined).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_enemy_hit", GetPlayerStat(PlayerStatType.DeathByEnemyHit).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_player_hit", GetPlayerStat(PlayerStatType.DeathByPlayerHit).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_fall", GetPlayerStat(PlayerStatType.DeathByFall).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_drowning", GetPlayerStat(PlayerStatType.DeathByDrowning).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_burning", GetPlayerStat(PlayerStatType.DeathByBurning).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_freezing", GetPlayerStat(PlayerStatType.DeathByFreezing).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_poisoned", GetPlayerStat(PlayerStatType.DeathByPoisoned).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_smoke", GetPlayerStat(PlayerStatType.DeathBySmoke).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_water", GetPlayerStat(PlayerStatType.DeathByWater).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_edge_of_world", GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_impact", GetPlayerStat(PlayerStatType.DeathByImpact).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_cart", GetPlayerStat(PlayerStatType.DeathByCart).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_tree", GetPlayerStat(PlayerStatType.DeathByTree).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_self", GetPlayerStat(PlayerStatType.DeathBySelf).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_structural", GetPlayerStat(PlayerStatType.DeathByStructural).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_boat", GetPlayerStat(PlayerStatType.DeathByBoat).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_turret", GetPlayerStat(PlayerStatType.DeathByTurret).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_stalagtite", GetPlayerStat(PlayerStatType.DeathByStalagtite).ToString(CultureInfo.CurrentCulture)),

            new Entry("$almanac_guardian_power_title", "title"),
            new Entry("$almanac_set_guardian_power", GetPlayerStat(PlayerStatType.SetGuardianPower).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_eikthyr", GetPlayerStat(PlayerStatType.SetPowerEikthyr).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_elder", GetPlayerStat(PlayerStatType.SetPowerElder).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_bonemass", GetPlayerStat(PlayerStatType.SetPowerBonemass).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_moder", GetPlayerStat(PlayerStatType.SetPowerModer).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_yagluth", GetPlayerStat(PlayerStatType.SetPowerYagluth).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_queen", GetPlayerStat(PlayerStatType.SetPowerQueen).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_ashlands", GetPlayerStat(PlayerStatType.SetPowerAshlands).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_deepNorth", GetPlayerStat(PlayerStatType.SetPowerDeepNorth).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_stands_title", "title"),
            new Entry("$almanac_item_stand_used", GetPlayerStat(PlayerStatType.ItemStandUses).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_armor_stand_used", GetPlayerStat(PlayerStatType.ArmorStandUses).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_trees_title", "title"),
            new Entry("$almanac_tree_chopped", GetPlayerStat(PlayerStatType.TreeChops).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree", GetPlayerStat(PlayerStatType.Tree).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_0", GetPlayerStat(PlayerStatType.TreeTier0).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_1", GetPlayerStat(PlayerStatType.TreeTier1).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_2", GetPlayerStat(PlayerStatType.TreeTier2).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_3", GetPlayerStat(PlayerStatType.TreeTier3).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_4", GetPlayerStat(PlayerStatType.TreeTier4).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tree_tier_5", GetPlayerStat(PlayerStatType.TreeTier5).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_log_chopped", GetPlayerStat(PlayerStatType.LogChops).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_logs", GetPlayerStat(PlayerStatType.Logs).ToString(CultureInfo.CurrentCulture)),

            new Entry("$almanac_ore_title", "title"),
            new Entry("$almanac_mine_hits", GetPlayerStat(PlayerStatType.MineHits).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mines", GetPlayerStat(PlayerStatType.Mines).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_0", GetPlayerStat(PlayerStatType.MineTier0).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_1", GetPlayerStat(PlayerStatType.MineTier1).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_2", GetPlayerStat(PlayerStatType.MineTier2).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_3", GetPlayerStat(PlayerStatType.MineTier3).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_4", GetPlayerStat(PlayerStatType.MineTier4).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_mine_tier_5", GetPlayerStat(PlayerStatType.MineTier5).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_raven_title", "title"),
            new Entry("$almanac_raven_hits", GetPlayerStat(PlayerStatType.RavenHits).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_raven_talk", GetPlayerStat(PlayerStatType.RavenTalk).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_raven_appear", GetPlayerStat(PlayerStatType.RavenAppear).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_food_eaten", GetPlayerStat(PlayerStatType.FoodEaten).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tombstone_title", "title"),
            new Entry("$almanac_tombstones_open_own", GetPlayerStat(PlayerStatType.TombstonesOpenedOwn).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tombstone_open_other", GetPlayerStat(PlayerStatType.TombstonesOpenedOther).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_tombstone_fit", GetPlayerStat(PlayerStatType.TombstonesFit).ToString(CultureInfo.CurrentCulture)),
            
            new Entry("$almanac_death_title", "title"),
            new Entry("$almanac_death_by_undefined", GetPlayerStat(PlayerStatType.DeathByUndefined).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_enemy_hit", GetPlayerStat(PlayerStatType.DeathByEnemyHit).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_player_hit", GetPlayerStat(PlayerStatType.DeathByPlayerHit).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_fall", GetPlayerStat(PlayerStatType.DeathByFall).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_drowning", GetPlayerStat(PlayerStatType.DeathByDrowning).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_burning", GetPlayerStat(PlayerStatType.DeathByBurning).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_freezing", GetPlayerStat(PlayerStatType.DeathByFreezing).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_poisoned", GetPlayerStat(PlayerStatType.DeathByPoisoned).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_smoke", GetPlayerStat(PlayerStatType.DeathBySmoke).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_water", GetPlayerStat(PlayerStatType.DeathByWater).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_edge_of_world", GetPlayerStat(PlayerStatType.DeathByEdgeOfWorld).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_impact", GetPlayerStat(PlayerStatType.DeathByImpact).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_cart", GetPlayerStat(PlayerStatType.DeathByCart).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_tree", GetPlayerStat(PlayerStatType.DeathByTree).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_self", GetPlayerStat(PlayerStatType.DeathBySelf).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_structural", GetPlayerStat(PlayerStatType.DeathByStructural).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_boat", GetPlayerStat(PlayerStatType.DeathByBoat).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_turret", GetPlayerStat(PlayerStatType.DeathByTurret).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_death_by_stalagtite", GetPlayerStat(PlayerStatType.DeathByStalagtite).ToString(CultureInfo.CurrentCulture)),
            
            new Entry("$almanac_guardian_power_title", "title"),
            new Entry("$almanac_set_guardian_power", GetPlayerStat(PlayerStatType.SetGuardianPower).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_eikthyr", GetPlayerStat(PlayerStatType.SetPowerEikthyr).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_elder", GetPlayerStat(PlayerStatType.SetPowerElder).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_bonemass", GetPlayerStat(PlayerStatType.SetPowerBonemass).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_moder", GetPlayerStat(PlayerStatType.SetPowerModer).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_yagluth", GetPlayerStat(PlayerStatType.SetPowerYagluth).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_queen", GetPlayerStat(PlayerStatType.SetPowerQueen).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_ashlands", GetPlayerStat(PlayerStatType.SetPowerAshlands).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_set_power_deepNorth", GetPlayerStat(PlayerStatType.SetPowerDeepNorth).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_guardian_power", GetPlayerStat(PlayerStatType.UseGuardianPower).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_eikthyr", GetPlayerStat(PlayerStatType.UsePowerEikthyr).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_elder", GetPlayerStat(PlayerStatType.UsePowerElder).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_bonemass", GetPlayerStat(PlayerStatType.UsePowerBonemass).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_moder", GetPlayerStat(PlayerStatType.UsePowerModer).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_yagluth", GetPlayerStat(PlayerStatType.UsePowerYagluth).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_queen", GetPlayerStat(PlayerStatType.UsePowerQueen).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_ashlands", GetPlayerStat(PlayerStatType.UsePowerAshlands).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_use_power_deepNorth", GetPlayerStat(PlayerStatType.UsePowerDeepNorth).ToString(CultureInfo.CurrentCulture)),
            new Entry("$almanac_count", GetPlayerStat(PlayerStatType.Count).ToString(CultureInfo.CurrentCulture)),

        };
    }

    public class Entry
    {
        public string title { get; set; }
        public string value { get; set; }

        public Entry(string Title, string Value)
        {
            title = Title;
            value = Value;
        }
    }
}