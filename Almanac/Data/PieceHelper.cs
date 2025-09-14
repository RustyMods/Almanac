using System.Collections.Generic;
using System.Linq;
using Almanac.Managers;
using Almanac.UI;
using Almanac.Utilities;
using UnityEngine;
using static Almanac.Utilities.Entries;

namespace Almanac.Data;

public static class PieceHelper
{
    public static void Setup()
    {
        AlmanacPlugin.OnZNetScenePrefabs += OnZNetScenePrefabs;
    }
    private static void OnZNetScenePrefabs(GameObject prefab)
    {
        if (prefab == null || !prefab.TryGetComponent(out Piece component)) return;
        SpriteManager.OnPieceHelperPiece(component);
        if (!component.IsValid()) return;
        _ = new PieceInfo(prefab);
    }
    private static bool IsValid(this Piece piece)
    {
        if (!piece.m_name.StartsWith("$")) return false;
        if (Localization.instance.Localize(piece.m_name).StartsWith("[")) return false;
        return true;
    }
    private static readonly List<PieceInfo> pieces = new();
    public static List<PieceInfo> GetPieces() => pieces.FindAll(x => !Filters.Ignore(x.prefab.name));
    public static List<PieceInfo> plants => GetPieces().FindAll(p => p.plant != null);

    public readonly record struct ItemConversion(ItemDrop from, ItemDrop to)
    {
        public readonly ItemDrop from = from;
        public readonly ItemDrop to = to;
    }

    public static List<ItemConversion> ToConversions(this List<Smelter.ItemConversion> smelter)
    {
        List<ItemConversion> conversions = new();
        foreach (var itemConversion in smelter)
        {
            conversions.Add(new ItemConversion(itemConversion.m_from, itemConversion.m_to));
        }

        return conversions;
    }

    public static List<ItemConversion> ToConversions(this List<CookingStation.ItemConversion> cookingStation)
    {
        List<ItemConversion> conversions = new();
        foreach (var itemConversion in cookingStation)
        {
            conversions.Add(new ItemConversion(itemConversion.m_from, itemConversion.m_to));
        }
        return conversions;
    }
    
    public readonly record struct PieceInfo
    {
        private static readonly EntryBuilder builder = new();
        public readonly GameObject prefab;
        public readonly Piece piece;
        private readonly WearNTear? wear;
        private readonly CraftingStation? craftingStation;
        private readonly Smelter? smelter;
        private readonly Windmill? windmill;
        private readonly Ship? ship;
        private readonly StationExtension extension;
        private readonly Beehive? beehive;
        private readonly Container? container;
        private readonly CookingStation? cookingStation;
        private readonly TeleportWorld? teleportWorld;
        private readonly WispSpawner? wispSpawner;
        private readonly Trap? trap;
        private readonly Fireplace? fireplace;
        private readonly Door? door;
        private readonly Turret? turret;
        private readonly Fermenter? fermenter;
        public readonly Plant? plant;
        public readonly ArcheryTarget? archeryTarget;
        public readonly Catapult catapult;
        public readonly HashSet<ItemConversion> conversions = new();
        public bool isKnown(Player player) => piece.m_resources.All(item => player.IsKnownMaterial(item.m_resItem.m_itemData.m_shared.m_name));

        public readonly bool isFeast;
        public readonly bool isShip;
        public readonly bool isPlant;

        public PieceInfo(GameObject prefab)
        {
            this.prefab = prefab;
            piece = prefab.GetComponent<Piece>();
            wear = prefab.GetComponent<WearNTear>();
            craftingStation = prefab.GetComponent<CraftingStation>();
            smelter = prefab.GetComponent<Smelter>();
            windmill = prefab.GetComponent<Windmill>();
            ship =  prefab.GetComponent<Ship>();
            extension = prefab.GetComponent<StationExtension>();
            beehive = prefab.GetComponent<Beehive>();
            container =  prefab.GetComponent<Container>();
            cookingStation = prefab.GetComponent<CookingStation>();
            teleportWorld = prefab.GetComponent<TeleportWorld>();
            wispSpawner = prefab.GetComponent<WispSpawner>();
            trap = prefab.GetComponent<Trap>();
            fireplace = prefab.GetComponent<Fireplace>();
            door = prefab.GetComponent<Door>();
            turret = prefab.GetComponent<Turret>();
            fermenter = prefab.GetComponent<Fermenter>();
            plant = prefab.GetComponent<Plant>();
            archeryTarget = prefab.GetComponentInChildren<ArcheryTarget>();
            catapult =  prefab.GetComponent<Catapult>();
            isFeast = prefab.GetComponent<ItemDrop>();
            isShip = ship != null;
            isPlant = plant != null;
            if (cookingStation != null)
            {
                foreach (var conversion in cookingStation.m_conversion)
                {
                    conversions.Add(new ItemConversion(conversion.m_from, conversion.m_to));
                }
            }

            if (smelter != null)
            {
                foreach (var conversion in smelter.m_conversion)
                {
                    conversions.Add(new ItemConversion(conversion.m_from, conversion.m_to));
                }
            }
            pieces.Add(this);
        }

        private List<Entry> ToEntries()
        {
            builder.Clear();
            if (!string.IsNullOrEmpty(piece.m_description)) builder.Add(piece.m_description, "lore");
            if (Configs.ShowAllData)
            {
                builder.Add(Keys.InternalID, prefab.name);
                builder.Add(Keys.Enabled, piece.m_enabled);
                // if (ModHelper.TryGetAssetInfo(prefab.name, out ModHelper.AssetInfo assetInfo))
                // {
                //     builder.Add("Asset Bundle", assetInfo.bundle);
                //     if (assetInfo.info != null)
                //     {
                //         builder.Add("Plugin", assetInfo.info.Metadata.Name);
                //     }
                // }
            }
            builder.Add(Keys.PieceCategory, piece.m_category);
            if (piece.m_craftingStation != null)
            {
                builder.Add(Keys.CraftingStation, piece.m_craftingStation);
            }
            if (piece.m_comfort > 0)
            {
                builder.Add(Keys.ComfortGroup, piece.m_comfortGroup);
                builder.Add(Keys.Comfort, piece.m_comfort);
            }

            if (piece.m_onlyInBiome != Heightmap.Biome.None)
            {
                builder.Add(Keys.OnlyInBiome, piece.m_onlyInBiome);
            }

            if (craftingStation != null)
            {
                builder.Add(Keys.CraftingStation);
                builder.Add(Keys.DiscoverRange, craftingStation.m_discoverRange);
                builder.Add(Keys.BuildRange,  craftingStation.m_buildRange);
                builder.Add(Keys.RequireRoof, craftingStation.m_craftRequireRoof);
                builder.Add(Keys.RequireFire,  craftingStation.m_craftRequireFire);
                builder.Add(Keys.UseAnimationIndex, craftingStation.m_useAnimation);   
            }

            if (extension != null)
            {
                builder.Add(Keys.StationExtension);
                builder.Add(Keys.Extends, extension.m_craftingStation.m_name);
            }

            if (wear != null)
            {
                builder.Add(Keys.WearNTear);
                builder.Add(Keys.Health, wear.m_health);
                builder.Add(Keys.Supports, wear.m_supports);
                builder.Add(Keys.MaterialType, wear.m_materialType); 
                if (wear.m_damages.m_blunt != HitData.DamageModifier.Normal) builder.Add(Keys.Blunt,  wear.m_damages.m_blunt);
                if (wear.m_damages.m_slash != HitData.DamageModifier.Normal) builder.Add(Keys.Slash,  wear.m_damages.m_slash);
                if (wear.m_damages.m_pierce != HitData.DamageModifier.Normal) builder.Add(Keys.Pierce,  wear.m_damages.m_pierce);
                if (wear.m_damages.m_chop != HitData.DamageModifier.Normal) builder.Add(Keys.Chop,  wear.m_damages.m_chop);
                if (wear.m_damages.m_pickaxe != HitData.DamageModifier.Normal) builder.Add(Keys.Pickaxe,  wear.m_damages.m_pickaxe);
                if (wear.m_damages.m_fire != HitData.DamageModifier.Normal) builder.Add(Keys.Fire,  wear.m_damages.m_fire);
                if (wear.m_damages.m_frost != HitData.DamageModifier.Normal) builder.Add(Keys.Frost,  wear.m_damages.m_frost);
                if (wear.m_damages.m_lightning != HitData.DamageModifier.Normal) builder.Add(Keys.Lightning,  wear.m_damages.m_lightning);
                if (wear.m_damages.m_poison != HitData.DamageModifier.Normal) builder.Add(Keys.Poison,  wear.m_damages.m_poison);
                if (wear.m_damages.m_spirit != HitData.DamageModifier.Normal) builder.Add(Keys.Spirit,  wear.m_damages.m_spirit);
            }

            if (smelter != null)
            {
                builder.Add(Keys.Smelter);
                builder.Add(Keys.FuelItem, smelter.m_fuelItem?.m_itemData.m_shared.m_name ?? Keys.None);
                if (smelter.m_conversion.Count > 0)
                {
                    builder.Add(Keys.Conversion);
                    foreach (Smelter.ItemConversion conversion in smelter.m_conversion)
                    {
                        builder.Add(conversion.m_from.m_itemData.m_shared.m_name, conversion.m_to.m_itemData.m_shared.m_name);
                    }
                }
            }

            if (windmill != null)
            {
                // has no information that is useful to share
            }

            if (beehive != null)
            {
                builder.Add(Keys.BeeHive);
                builder.Add(Keys.OnlyInBiome, beehive.m_biome);
                builder.Add(Keys.MaxStorage, beehive.m_maxHoney);
                builder.Add(Keys.Item, beehive.m_honeyItem.m_itemData.m_shared.m_name);
            }

            if (container != null)
            {
                builder.Add(Keys.Container);
                builder.Add(Keys.Size, container.m_width, container.m_height, "x");
                if (container.m_defaultItems.m_drops.Count > 0)
                {
                    builder.Add(Keys.DefaultItems);
                    builder.Add(Keys.Amount, container.m_defaultItems.m_dropMin,  container.m_defaultItems.m_dropMax, "-");
                    builder.Add(Keys.Chance, container.m_defaultItems.m_dropChance);
                    float totalWeight = container.m_defaultItems.m_drops.Sum(item => item.m_weight);
                    foreach (var drop in container.m_defaultItems.m_drops)
                    {
                        if (drop.m_item == null || !drop.m_item.TryGetComponent(out ItemDrop itemDrop)) continue;
                        builder.Add(itemDrop.m_itemData.m_shared.m_name, drop.m_weight / totalWeight, EntryBuilder.Option.Percentage);
                    }
                }
            }

            if (cookingStation != null)
            {
                builder.Add(Keys.CookingStation);
                builder.Add(Keys.OverCookedItem, cookingStation.m_overCookedItem.m_itemData.m_shared.m_name);
                builder.Add(Keys.RequireFire,  cookingStation.m_requireFire);
                builder.Add(Keys.FuelItem,  cookingStation.m_fuelItem.m_itemData.m_shared.m_name);
                if (cookingStation.m_conversion.Count > 0)
                {
                    builder.Add(Keys.Conversion);
                    foreach (var conversion in cookingStation.m_conversion)
                    {
                        builder.Add(conversion.m_from, conversion.m_to);
                    }
                }
            }

            if (teleportWorld != null)
            {
                builder.Add(Keys.Portal);
                builder.Add(Keys.ActivationRange, teleportWorld.m_activationRange);
                builder.Add(Keys.ExitDistance, teleportWorld.m_exitDistance);
            }

            if (ship != null)
            {
                //TODO
            }

            if (wispSpawner != null)
            {
                builder.Add(Keys.WispSpawner);
                builder.Add(Keys.SpawnInterval, wispSpawner.m_spawnInterval, EntryBuilder.Option.Seconds);
                builder.Add(Keys.SpawnChance, wispSpawner.m_spawnChance, EntryBuilder.Option.Percentage);
                builder.Add(Keys.SpawnDistance, wispSpawner.m_spawnDistance);
                builder.Add(Keys.MaxSpawn, wispSpawner.m_maxSpawnedArea);
            }

            if (trap != null)
            {
                builder.Add(Keys.Trap);
                builder.Add(Keys.TargetPlayers, trap.m_triggeredByPlayers);
                builder.Add(Keys.TargetEnemies, trap.m_triggeredByEnemies);
                if (prefab.TryGetComponent(out Aoe aoe))
                {
                    builder.Add(Keys.Blunt, aoe.m_damage.m_blunt);
                    builder.Add(Keys.Slash, aoe.m_damage.m_slash);
                    builder.Add(Keys.Piece, aoe.m_damage.m_pierce);
                    builder.Add(Keys.Chop, aoe.m_damage.m_chop);
                    builder.Add(Keys.Pickaxe, aoe.m_damage.m_pickaxe);
                    builder.Add(Keys.Fire, aoe.m_damage.m_fire);
                    builder.Add(Keys.Frost, aoe.m_damage.m_frost);
                    builder.Add(Keys.Lightning, aoe.m_damage.m_lightning);
                    builder.Add(Keys.Poison, aoe.m_damage.m_poison);
                    builder.Add(Keys.Spirit, aoe.m_damage.m_spirit);
                    builder.Add(Keys.Total, aoe.m_damage.GetTotalDamage());
                }
            }

            if (fireplace != null)
            {
                builder.Add(Keys.Fireplace);
                builder.Add(Keys.FuelItem,  fireplace.m_fuelItem.m_itemData.m_shared.m_name);
            }

            if (door != null)
            {
                builder.Add(Keys.Door);
                builder.Add(Keys.KeyItem, door.m_keyItem?.m_itemData.m_shared.m_name ?? Keys.None);
            }

            if (turret != null)
            {
                builder.Add(Keys.Turret);
                builder.Add(Keys.AmmoType, turret.m_ammoType);
                builder.Add(Keys.Max, turret.m_maxAmmo);
                builder.Add(Keys.TargetPlayers, turret.m_targetPlayers);
                builder.Add(Keys.TargetTames, turret.m_targetTamed);
                builder.Add(Keys.TargetEnemies, turret.m_targetEnemies);
                builder.Add(Keys.TurnRate, turret.m_turnRate);
            }

            if (fermenter != null)
            {
                builder.Add(Keys.Fermenter);
                builder.Add(Keys.Duration, fermenter.m_fermentationDuration, EntryBuilder.Option.Seconds);
                if (fermenter.m_conversion.Count > 0)
                {
                    builder.Add(Keys.Conversion);
                    foreach (var conversion in fermenter.m_conversion)
                    {
                        builder.Add(conversion.m_to, conversion.m_from);
                    }
                }
            }

            if (plant != null)
            {
                builder.Add(Keys.Plant);
                builder.Add(Keys.GrowDuration, plant.m_growTime, EntryBuilder.Option.Seconds);
                builder.Add(Keys.Biome, plant.m_biome);
            }

            if (archeryTarget != null)
            {
                //TODO
            }

            if (catapult != null)
            {
                //TODO
            }
            
            return builder.ToList();
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element? item)
        {
            if (item != null) panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(piece.m_name);
            panel.description.SetIcon(piece.m_icon);
            ToEntries().Build(panel.description.view);
            panel.description.view.Resize();
            panel.description.Interactable(false);
            panel.description.requirements.Set(piece.m_resources);
            panel.OnUpdate = _ => panel.description.requirements.Update();
        }
    }
}