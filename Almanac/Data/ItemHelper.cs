using System.Collections.Generic;
using System.Linq;
using Almanac.Achievements;
using Almanac.Managers;
using Almanac.UI;
using Almanac.Utilities;
using UnityEngine;
using static ItemDrop.ItemData;

namespace Almanac.Data;
public static class ItemHelper
{
    public static bool HasIcons(this ItemDrop.ItemData item) => item.m_shared.m_icons.Length > 0;
    public static bool HasIcons(this ItemDrop itemDrop) => itemDrop.m_itemData.HasIcons();
    public static bool HasVariants(this ItemDrop.ItemData item) => item.m_shared.m_icons.Length > 1;
    public static bool HasVariants(this ItemDrop itemDrop) => itemDrop.m_itemData.HasVariants();
    public static HashSet<ItemDrop> GetSet(this ItemDrop.ItemData item)
    {
        HashSet<ItemDrop> set = new HashSet<ItemDrop>();
        if (string.IsNullOrEmpty(item.m_shared.m_setName)) return set;
        foreach (ItemDrop itemDrop in ObjectDB.instance.m_items.Select(i => i.GetComponent<ItemDrop>()))
        {
            if (itemDrop.m_itemData.m_shared.m_setName == item.m_shared.m_setName) set.Add(itemDrop);
        }
        return set;
    }
    private static HashSet<CritterHelper.CritterInfo> DroppedBy(this ItemDrop.ItemData item)
    {
        HashSet<CritterHelper.CritterInfo> characters = new();
        foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
        {
            if (!prefab.TryGetComponent(out Character character) || !prefab.TryGetComponent(out CharacterDrop component)) continue;
            if (!component.m_drops.Contains(item)) continue;
            if (character.GetInfo() is not { } info) continue;
            characters.Add(info);
        }
        return characters;
    }
    private static bool Contains(this List<CharacterDrop.Drop> list, ItemDrop.ItemData item)
    {
        foreach (CharacterDrop.Drop? drop in list)
        {
            if (!drop.m_prefab.TryGetComponent(out ItemDrop itemDrop)) continue;
            if (itemDrop.m_itemData.m_shared.m_name == item.m_shared.m_name)
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsPartOfSet(this ItemDrop.ItemData item) => !string.IsNullOrEmpty(item.m_shared.m_setName);
    private static bool HasStatModifiers(this ItemDrop.ItemData item) => item.m_shared.HasStatModifiers();
    private static bool HasStatModifiers(this SharedData shared)
    {
        return shared.m_movementModifier 
               + shared.m_eitrRegenModifier 
               + shared.m_homeItemsStaminaModifier 
               + shared.m_heatResistanceModifier
               + shared.m_jumpStaminaModifier
               + shared.m_attackStaminaModifier 
               + shared.m_blockStaminaModifier 
               + shared.m_dodgeStaminaModifier 
               + shared.m_swimStaminaModifier
               + shared.m_sneakStaminaModifier
               + shared.m_runStaminaModifier
               > 0f;
    }
    private static ItemInfo? GetInfo(string sharedName) => ItemInfos.TryGetValue(sharedName, out var info) ? info : null;
    public static ItemInfo? GetInfo(this ItemDrop.ItemData item) => GetInfo(item.m_shared.m_name);
    public static List<ItemInfo> GetItemsWhileIgnoring(params ItemType[] ignoreTypes)
    {
        HashSet<ItemType> typeSet = new HashSet<ItemType>(ignoreTypes);
        return GetItems().Where(item => !typeSet.Contains(item.shared.m_itemType)).ToList();
    }
    private static List<ItemInfo> GetItemByType(params ItemType[] types)
    {
        HashSet<ItemType> typeSet = new HashSet<ItemType>(types);
        return GetItems().Where(item => typeSet.Contains(item.shared.m_itemType)).ToList();
    }
    private static List<ItemInfo> GetItemsBySkill(params Skills.SkillType[] types)
    {
        HashSet<Skills.SkillType> typeSet = new HashSet<Skills.SkillType>(types);
        return weapons.Where(item => typeSet.Contains(item.shared.m_skillType)).ToList();
    }
    public static List<ItemInfo> scrolls => GetItems().Where(item => item.prefab.name.StartsWith("kg")).ToList();
    public static List<ItemInfo> jewels => GetItems().Where(item => item.IsJewel()).ToList();
    private static bool IsJewel(this ItemInfo info)
    {
         List<string> tables = new() { "Odins_Stone_Transmuter", "op_transmution_table", "Odins_Jewelry_Box", "JC_CrystalBall_Ext", "JC_Gemstone_Furnace" };
         var station = info.recipe?.m_craftingStation?.name ?? string.Empty;
         if (!string.IsNullOrEmpty(station)) return tables.Contains(station);
         string localizedName = Localization.instance.Localize(info.shared.m_name);
         if (info.prefab.name.Contains("Gem") || localizedName.Contains("Gem") || info.prefab.name.StartsWith("JC_") ||
             info.prefab.name == "Soulcatcher_CursedDoll" || info.prefab.name.Contains("_Crystal") || info.prefab.name.Contains("_Socket")) return true;
         return false;
    }
    public static bool IsTool(this ItemDrop item)
    {
        List<string> toolNames = new()
            { "Cultivator", "Hammer", "Hoe", "Tankard", "Tankard_dvergr", "Lantern", "TankardAnniversary" };
        if (item.m_itemData.m_shared.m_buildPieces) return true;
        if (item.m_itemData.m_shared.m_itemType is ItemType.Torch) return true;
        return toolNames.Contains(item.name);
    }
    private static Dictionary<string, ItemInfo>? _pickableItems;
    public static Dictionary<string, ItemInfo> pickableItems
    {
        get
        {
            if (_pickableItems != null) return _pickableItems;
            var result  = new Dictionary<string, ItemInfo>();
            foreach (Pickable? pickable in pickables)
            {
                if (pickable.m_itemPrefab == null) continue;
                if (!pickable.m_itemPrefab.TryGetComponent(out ItemDrop component)) continue;
                if (component.m_itemData.GetInfo() is not { } info) continue;
                result[pickable.name] = info;
            }
            _pickableItems = result;
            return _pickableItems;
        }
    }
    private static List<Pickable>? _pickables;
    public static List<Pickable> pickables
    {
        get
        {
            if (_pickables != null) return _pickables;
            var result = new List<Pickable>();
            foreach (GameObject? prefab in ZNetScene.instance.m_prefabs)
            {
                var pickable = prefab.GetComponent<Pickable>();
                if (pickable != null)
                    result.Add(pickable);
            }
            _pickables = result;
            return result;
        }
    }
    private static bool IsBait(this ItemDrop itemDrop) => itemDrop.name.ToLower().Contains("bait");
    public static List<Recipe> recipes => ObjectDB.instance.m_recipes;
    public static List<ItemInfo> baits => GetItems().Where(item => item.itemDrop.IsBait()).ToList();
    public static List<ItemInfo> swords => GetItemsBySkill(Skills.SkillType.Swords);
    public static List<ItemInfo> axes => GetItemsBySkill(Skills.SkillType.Axes);
    public static List<ItemInfo> polearms => GetItemsBySkill(Skills.SkillType.Polearms);
    public static List<ItemInfo> spears => GetItemsBySkill(Skills.SkillType.Spears);
    public static List<ItemInfo> clubs => GetItemsBySkill(Skills.SkillType.Clubs);
    public static List<ItemInfo> knives =>  GetItemsBySkill(Skills.SkillType.Knives);
    public static List<ItemInfo> blocking => GetItemsBySkill(Skills.SkillType.Blocking);
    public static List<ItemInfo> capes => GetItemByType(ItemType.Shoulder);
    public static List<ItemInfo> staves => GetItemsBySkill(Skills.SkillType.ElementalMagic, Skills.SkillType.BloodMagic);
    public static List<ItemInfo> potions => consumables.Where(item => item.shared.m_consumeStatusEffect != null).ToList();
    public static List<ItemInfo> ammo => GetItemByType(ItemType.Ammo);
    public static List<ItemInfo> bows => GetItemsBySkill(Skills.SkillType.Bows);
    public static List<ItemInfo> unarmed => GetItemsBySkill(Skills.SkillType.Unarmed);
    public static List<ItemInfo> crossbows => GetItemsBySkill(Skills.SkillType.Crossbows);
    public static List<ItemInfo> valuables => GetItems().Where(item => item.shared.m_value > 0).ToList();
    public static List<ItemInfo> trophies => GetItemByType(ItemType.Trophy);
    public static List<ItemInfo> trinkets => GetItemByType(ItemType.Trinket);
    public static List<ItemInfo> fishes => GetItemByType(ItemType.Fish);
    public static List<ItemInfo> materials => GetItemByType(ItemType.Material);
    public static List<ItemInfo> consumables =>  GetItemByType(ItemType.Consumable);
    public static List<ItemInfo> helmets => GetItemByType(ItemType.Helmet);
    public static List<ItemInfo> chests => GetItemByType(ItemType.Chest);
    public static List<ItemInfo> legs => GetItemByType(ItemType.Legs);
    public static List<ItemInfo> weapons => GetItemsWhileIgnoring(ItemType.None, ItemType.Material, ItemType.Consumable, ItemType.Customization, ItemType.Legs, ItemType.Trophy, ItemType.Torch, ItemType.Misc, ItemType.Shoulder, ItemType.Utility, ItemType.Tool, ItemType.Attach_Atgeir, ItemType.Fish, ItemType.Ammo, ItemType.AmmoNonEquipable, ItemType.Helmet, ItemType.Chest);
    private static List<ItemInfo> items => ItemInfos.Values.ToList();
    private static List<ItemInfo> GetItems() => items.FindAll(x => !Filters.Ignore(x.prefab.name));
    public static bool IsSword(this ItemInfo info) => swords.Contains(info);
    public static bool IsAxe(this ItemInfo info) => axes.Contains(info);
    public static bool IsPolearm(this ItemInfo info) => polearms.Contains(info);
    public static bool IsSpear(this ItemInfo info) => spears.Contains(info);
    public static bool IsClub(this ItemInfo info) => clubs.Contains(info);
    public static bool IsKnives(this ItemInfo info) => knives.Contains(info);
    public static bool IsShield(this ItemInfo info) => blocking.Contains(info);
    public static bool IsUnarmed(this ItemInfo info) => unarmed.Contains(info);
    public static bool IsTrophy(this ItemInfo info) => trophies.Contains(info);
    public static bool IsFish(this ItemInfo info) => fishes.Contains(info);
    public static bool IsMaterial(this ItemInfo info) => materials.Contains(info);
    public static bool IsConsumable(this ItemInfo info) => consumables.Contains(info);
    public static bool IsWeapon(this ItemInfo info) => weapons.Contains(info);
    public static bool IsValuable(this ItemInfo info) => valuables.Contains(info);
    public static bool IsAmmo(this ItemInfo info) => ammo.Contains(info);
    public static bool IsPotion(this ItemInfo info) => potions.Contains(info);
    public static bool IsStaff(this ItemInfo info) => staves.Contains(info);
    public static bool IsHelmet(this ItemInfo info) => helmets.Contains(info);
    public static bool IsChest(this ItemInfo info) => chests.Contains(info);
    public static bool IsLegs(this ItemInfo info) => legs.Contains(info);
    public static bool IsTrinket(this ItemInfo info) => trinkets.Contains(info);
    public static ItemInfo? GetInfo(this ItemDrop item) => item.m_itemData.GetInfo();
    private static readonly Dictionary<string, ItemInfo> ItemInfos = new();
    public readonly record struct ItemInfo
    {
        private static readonly Entries.EntryBuilder builder = new();

        public readonly GameObject prefab;
        public readonly ItemDrop itemDrop;
        public readonly ItemDrop.ItemData itemData = null!;
        public readonly SharedData shared = null!;
        public readonly Recipe? recipe;
        private readonly List<Sprite> Icons = new();
        private readonly HashSet<CritterHelper.CritterInfo> droppedBy = new();
        private readonly HashSet<ItemDrop> setItems = new();
        private readonly HashSet<Recipe> usedIn = new();
        private readonly HashSet<PieceHelper.PieceInfo> usedInPieces = new();
        private readonly Fish? fish;
        private readonly HashSet<ItemInfo> _itemBaits = new();
        private HashSet<ItemInfo> itemBaits
        {
            get
            {
                if (_itemBaits.Count != 0 || fish == null) return _itemBaits;
                foreach (Fish.BaitSetting? bait in fish.m_baits)
                {
                    if (bait.m_bait != null && bait.m_bait.GetInfo() is { } info)
                    {
                        _itemBaits.Add(info);
                    }
                }
                return _itemBaits;
            }
        }
        private readonly bool isFloating;
        private bool IsUsedInOtherRecipes => usedIn.Count != 0;
        private bool IsUsedInPieces => usedInPieces.Count != 0;
        public Sprite? GetIcon() => itemData.GetIcon();
        public ItemInfo(GameObject prefab)
        {
            this.prefab = prefab;
            itemDrop = prefab.GetComponent<ItemDrop>();
            isFloating = prefab.GetComponent<Floating>();
            itemData = itemDrop.m_itemData;
            recipe = ObjectDB.instance?.GetRecipe(itemData);
            shared = itemData.m_shared;
            droppedBy = itemData.DroppedBy();
            setItems = itemData.GetSet();
            fish = prefab.GetComponent<Fish>();
            if (!itemData.HasIcons()) return;
            Icons.AddRange(shared.m_icons);
            usedIn = recipes.Where(r => r.m_item != null && r.m_resources.Any(resource => resource.m_resItem.name == prefab.name)).ToHashSet();
            usedInPieces = PieceHelper.GetPieces().Where(p => p.piece.m_resources.Any(resource => resource.m_resItem.name == prefab.name)).ToHashSet();
            ItemInfos[shared.m_name] = this;
        }

        private List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            if (Configs.ShowAllData)
            {
                builder.Add(Keys.InternalID, prefab.name);
                // if (ModHelper.TryGetAssetInfo(prefab.name, out ModHelper.AssetInfo assetInfo))
                // {
                //     builder.Add("Asset Bundle", assetInfo.bundle);
                //     if (assetInfo.info != null)
                //     {
                //         builder.Add("Plugin", assetInfo.info.Metadata.Name);
                //     }
                // }
            }
            builder.Add(Keys.Teleportable, shared.m_teleportable);
            builder.Add(Keys.Value, shared.m_value);
            builder.Add(Keys.Weight, shared.m_weight);
            builder.Add(Keys.Quality, shared.m_maxQuality);
            builder.Add(Keys.Durability, shared.m_maxDurability, shared.m_durabilityPerLevel, $"/{Keys.Lvl}");
            builder.Add(Keys.CanBeRepaired, shared.m_canBeReparied);
            if (recipe != null) builder.Add(Keys.RepairLevel, recipe.m_minStationLevel);
            builder.Add(Keys.MaxStackSize, shared.m_maxStackSize);
            builder.Add(Keys.Variant, shared.m_icons.Length);
            builder.Add(Keys.ItemType, shared.m_itemType);
            builder.Add(Keys.QuestItem, shared.m_questItem);
            builder.Add(Keys.EquipDuration, shared.m_equipDuration, Entries.EntryBuilder.Option.Seconds);
            builder.Add(Keys.Floating, isFloating);
            switch (shared.m_itemType)
            {
                case ItemType.Fish:
                    if (fish != null)
                    {
                        builder.Add(Keys.Fish);
                        builder.Add(Keys.SwimRange, fish.m_swimRange);
                        builder.Add(Keys.MinDepth, fish.m_minDepth);
                        builder.Add(Keys.Speed, fish.m_speed);
                        builder.Add(Keys.Acceleration, fish.m_acceleration);
                        builder.Add(Keys.TurnRate, fish.m_turnRate);
                        builder.Add(Keys.AvoidRange, fish.m_avoidRange);
                        builder.Add(Keys.Height, fish.m_height);
                        builder.Add(Keys.HookForce, fish.m_hookForce);
                        builder.Add(Keys.StaminaUse, fish.m_staminaUse);
                        builder.Add(Keys.EscapeStaminaUse, fish.m_escapeStaminaUse);
                        builder.Add(Keys.Escape, (int)fish.m_escapeMin, (int)fish.m_escapeMax, "-");
                        builder.Add(Keys.BaseHookChance, fish.m_baseHookChance);
                        builder.Add(Keys.JumpSpeed, fish.m_jumpSpeed);
                        builder.Add(Keys.JumpHeight, fish.m_jumpHeight);
                        builder.Add(Keys.JumpOnLandChance, fish.m_jumpOnLandChance);
                        builder.Add(Keys.JumpFrequency, fish.m_jumpFrequencySeconds, Entries.EntryBuilder.Option.Seconds);
                        builder.Add(Keys.FishFast, fish.m_fast);
                    }
                    break;
                case ItemType.Helmet or ItemType.Legs or ItemType.Chest or ItemType.Customization or ItemType.Shoulder:
                    builder.Add(Keys.Armor);
                    builder.Add(Keys.Armor, shared.m_armor, shared.m_armorPerLevel, $"/{Keys.Lvl}");
                    builder.Add(Keys.Material, shared.m_armorMaterial);
                    builder.Add(Keys.HideHelmet, shared.m_helmetHideHair);
                    builder.Add(Keys.HideBeard, shared.m_helmetHideBeard);
                    if (shared.m_damageModifiers.Count > 0)
                    {
                        builder.Add(Keys.Resistances);
                        foreach (HitData.DamageModPair resistance in shared.m_damageModifiers) builder.Add(resistance);
                    }

                    if (shared.m_equipStatusEffect is { } equipStatus)
                    {
                        builder.Add(Keys.EquipEffect);
                        builder.Add(Keys.EquipEffect, equipStatus.m_name);
                        foreach (Skills.SkillType skill in Skills.s_allSkills)
                        {
                            float skillLevel = 0f;
                            equipStatus.ModifySkillLevel(skill, ref skillLevel);
                            float raiseLevel = 0f;
                            equipStatus.ModifyRaiseSkill(skill, ref raiseLevel);
                            builder.Add(skill, skillLevel, Entries.EntryBuilder.Option.Level);
                            builder.Add(skill, raiseLevel, Entries.EntryBuilder.Option.XP);
                        }

                        float fallDamage = 0f;
                        equipStatus.ModifyFallDamage(40f, ref fallDamage);
                        float healthRegen = 0f;
                        equipStatus.ModifyHealthRegen(ref healthRegen);
                        float staminaRegen = 0f;
                        equipStatus.ModifyStaminaRegen(ref staminaRegen);
                        float eitrRegen = 0f;
                        equipStatus.ModifyEitrRegen(ref eitrRegen);
                        builder.Add(Keys.FallDamage, fallDamage);
                        builder.Add(Keys.HealthRegen, healthRegen);
                        builder.Add(Keys.StaminaRegen, staminaRegen);
                        builder.Add(Keys.EitRegen, eitrRegen);
                        HitData.DamageModifiers modifiers = new();
                        equipStatus.ModifyDamageMods(ref modifiers);
                        if (modifiers.m_blunt != HitData.DamageModifier.Normal) builder.Add(Keys.Blunt, modifiers.m_blunt);
                        if (modifiers.m_slash != HitData.DamageModifier.Normal) builder.Add(Keys.Slash, modifiers.m_slash);
                        if (modifiers.m_pierce != HitData.DamageModifier.Normal) builder.Add(Keys.Pierce, modifiers.m_pierce);
                        if (modifiers.m_chop != HitData.DamageModifier.Normal) builder.Add(Keys.Chop, modifiers.m_chop);
                        if (modifiers.m_pickaxe != HitData.DamageModifier.Normal) builder.Add(Keys.Pickaxe, modifiers.m_pickaxe);
                        if (modifiers.m_fire != HitData.DamageModifier.Normal) builder.Add(Keys.Fire, modifiers.m_fire);
                        if (modifiers.m_frost != HitData.DamageModifier.Normal) builder.Add(Keys.Frost, modifiers.m_frost);
                        if (modifiers.m_lightning != HitData.DamageModifier.Normal) builder.Add(Keys.Lightning, modifiers.m_lightning);
                        if (modifiers.m_poison != HitData.DamageModifier.Normal) builder.Add(Keys.Poison, modifiers.m_poison);
                        if (modifiers.m_spirit != HitData.DamageModifier.Normal) builder.Add(Keys.Spirit, modifiers.m_spirit);
                    }

                    if (itemData.HasStatModifiers())
                    {
                        builder.Add(Keys.StatModifier);
                        builder.Add(Keys.MovementModifier, shared.m_movementModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.EitrRegenModifier, shared.m_eitrRegenModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.HomeItemModifier, shared.m_homeItemsStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.HeatModifier, shared.m_heatResistanceModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.JumpStamina, shared.m_jumpStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.AttackStamina, shared.m_attackStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.BlockStamina, shared.m_blockStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.DodgeStamina, shared.m_dodgeStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.SwimStamina, shared.m_swimStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.SneakStamina, shared.m_sneakStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.RunStamina, shared.m_runStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                    }
                    if (itemData.IsPartOfSet())
                    {
                        builder.Add(Keys.Set);
                        builder.Add(Keys.SetName, shared.m_setName);
                        builder.Add(Keys.ItemParts, shared.m_setSize);
                        builder.Add(Keys.SetEffect, shared.m_setStatusEffect);
                        if (shared.m_setStatusEffect is {} setStatus)
                        {
                            builder.Add(setStatus.GetTooltipString(), "lore");
                            foreach (Skills.SkillType skill in Skills.s_allSkills)
                            {
                                float amount = 0f;
                                setStatus.ModifySkillLevel(skill, ref amount);
                                builder.Add(skill, amount);
                            }
                        }
                    }
                    break;
                case ItemType.Trinket:
                    builder.Add(Keys.Trinkets);
                    if (shared.m_fullAdrenalineSE is { } adrenaline)
                    {
                        builder.Add(Keys.EquipEffect, adrenaline.m_name);
                        builder.Add(Keys.BlockAdrenaline, shared.m_blockAdrenaline);
                        builder.Add(Keys.PerfectBlockAdrenaline, shared.m_perfectBlockAdrenaline);
                        builder.Add(adrenaline.GetTooltipString(), "lore");
                    }
                    break;
                case ItemType.Consumable:
                    builder.Add(Keys.Food);
                    builder.Add(Keys.Health, shared.m_food);
                    builder.Add(Keys.Stamina, shared.m_foodStamina);
                    builder.Add(Keys.Eitr, shared.m_foodEitr);
                    builder.Add(Keys.Duration, shared.m_foodBurnTime);
                    builder.Add(Keys.Healing, shared.m_foodRegen);
                    if (shared.m_consumeStatusEffect is { } consumeStatus)
                    {
                        builder.Add(Keys.ConsumeEffect, consumeStatus.m_name);
                        builder.Add(Keys.Category, consumeStatus.m_category);
                        builder.Add(Keys.Duration, consumeStatus.m_ttl, Entries.EntryBuilder.Option.Seconds);
                        builder.Add(consumeStatus.GetTooltipString(), "lore");
                        foreach (Skills.SkillType skill in Skills.s_allSkills)
                        {
                            float skillLevel = 0f;
                            consumeStatus.ModifySkillLevel(skill, ref skillLevel);
                            float raiseLevel = 0f;
                            consumeStatus.ModifyRaiseSkill(skill, ref raiseLevel);
                            builder.Add(skill, skillLevel, Entries.EntryBuilder.Option.Level);
                            builder.Add(skill, raiseLevel, Entries.EntryBuilder.Option.XP);
                        }

                        float fallDamage = 0f;
                        consumeStatus.ModifyFallDamage(40f, ref fallDamage);
                        float healthRegen = 0f;
                        consumeStatus.ModifyHealthRegen(ref healthRegen);
                        float staminaRegen = 0f;
                        consumeStatus.ModifyStaminaRegen(ref staminaRegen);
                        float eitrRegen = 0f;
                        consumeStatus.ModifyEitrRegen(ref eitrRegen);
                        builder.Add(Keys.FallDamage, fallDamage);
                        builder.Add(Keys.HealthRegen, healthRegen);
                        builder.Add(Keys.StaminaRegen, staminaRegen);
                        builder.Add(Keys.EitRegen, eitrRegen);
                        HitData.DamageModifiers modifiers = new();
                        consumeStatus.ModifyDamageMods(ref modifiers);
                        if (modifiers.m_blunt != HitData.DamageModifier.Normal) builder.Add(Keys.Blunt, modifiers.m_blunt);
                        if (modifiers.m_slash != HitData.DamageModifier.Normal) builder.Add(Keys.Slash, modifiers.m_slash);
                        if (modifiers.m_pierce != HitData.DamageModifier.Normal) builder.Add(Keys.Pierce, modifiers.m_pierce);
                        if (modifiers.m_chop != HitData.DamageModifier.Normal) builder.Add(Keys.Chop, modifiers.m_chop);
                        if (modifiers.m_pickaxe != HitData.DamageModifier.Normal) builder.Add(Keys.Pickaxe, modifiers.m_pickaxe);
                        if (modifiers.m_fire != HitData.DamageModifier.Normal) builder.Add(Keys.Fire, modifiers.m_fire);
                        if (modifiers.m_frost != HitData.DamageModifier.Normal) builder.Add(Keys.Frost, modifiers.m_frost);
                        if (modifiers.m_lightning != HitData.DamageModifier.Normal) builder.Add(Keys.Lightning, modifiers.m_lightning);
                        if (modifiers.m_poison != HitData.DamageModifier.Normal) builder.Add(Keys.Poison, modifiers.m_poison);
                        if (modifiers.m_spirit != HitData.DamageModifier.Normal) builder.Add(Keys.Spirit, modifiers.m_spirit);
                    }
                    break;
                case ItemType.Shield:
                    builder.Add(Keys.Shield);
                    builder.Add(Keys.Armor, shared.m_blockPower, shared.m_blockPowerPerLevel, $"/{Keys.Lvl}");
                    builder.Add(Keys.BlockForce, shared.m_deflectionForce, shared.m_deflectionForcePerLevel, $"/{Keys.Lvl}");
                    builder.Add(Keys.ParryBonus, shared.m_timedBlockBonus);
                    if (itemData.IsPartOfSet())
                    {
                        builder.Add(Keys.Set);
                        builder.Add(Keys.SetName, shared.m_setName);
                        builder.Add(Keys.ItemParts, shared.m_setSize);
                        builder.Add(Keys.SetEffect, shared.m_setStatusEffect);
                        if (shared.m_setStatusEffect is {} setStatus)
                        {
                            builder.Add(setStatus.GetTooltipString(), "lore");
                            foreach (Skills.SkillType skill in Skills.s_allSkills)
                            {
                                float amount = 0f;
                                setStatus.ModifySkillLevel(skill, ref amount);
                                builder.Add(skill, amount);
                            }
                        }
                    }
                    break;
                case ItemType.Ammo or ItemType.Bow or ItemType.Hands or ItemType.Tool or ItemType.Torch or ItemType.Attach_Atgeir or ItemType.AmmoNonEquipable or ItemType.OneHandedWeapon or ItemType.TwoHandedWeapon or ItemType.TwoHandedWeaponLeft:
                    builder.Add(Keys.Weapon);
                    if (Configs.ShowAllData) builder.Add(Keys.AnimationState, shared.m_animationState);
                    builder.Add(Keys.SkillType, shared.m_skillType);
                    builder.Add(Keys.ToolTier, shared.m_toolTier);
                    builder.Add(Keys.Damage, shared.m_damages.m_damage);
                    builder.Add(Keys.Blunt, shared.m_damages.m_blunt);
                    builder.Add(Keys.Slash, shared.m_damages.m_slash);
                    builder.Add(Keys.Pierce, shared.m_damages.m_pierce);
                    builder.Add(Keys.Chop, shared.m_damages.m_chop);
                    builder.Add(Keys.Pickaxe, shared.m_damages.m_pickaxe);
                    builder.Add(Keys.Fire, shared.m_damages.m_fire);
                    builder.Add(Keys.Frost, shared.m_damages.m_frost);
                    builder.Add(Keys.Lightning, shared.m_damages.m_lightning);
                    builder.Add(Keys.Poison, shared.m_damages.m_poison);
                    builder.Add(Keys.Spirit, shared.m_damages.m_spirit);
                    builder.Add(Keys.AttackForce, shared.m_attackForce);
                    builder.Add(Keys.Dodgeable, shared.m_dodgeable);
                    builder.Add(Keys.Blockable, shared.m_blockable);
                    builder.Add(Keys.TameOnly, shared.m_tamedOnly);
                    builder.Add(Keys.AlwaysRotate, shared.m_alwaysRotate);
                    builder.Add(Keys.AttackEffect, shared.m_attackStatusEffect);
                    builder.Add(Keys.ChanceToApplySE,shared.m_attackStatusEffectChance, Entries.EntryBuilder.Option.Percentage);
                    builder.Add(Keys.SpawnOnHit, shared.m_spawnOnHit);
                    builder.Add(Keys.SpawnOnHitTerrain, shared.m_spawnOnHitTerrain);
                    builder.Add(Keys.ProjectileTooltip, shared.m_projectileToolTip);
                    builder.Add(Keys.AmmoType, shared.m_ammoType);
                    // Attack
                    if (Configs.ShowAllData)
                    {
                        builder.Add(Keys.Attack);
                        builder.Add(Keys.AttackType, shared.m_attack.m_attackType);
                        builder.Add(Keys.AttackAnimation, shared.m_attack.m_attackAnimation);
                        builder.Add(Keys.AttackRandomAnimation, shared.m_attack.m_attackRandomAnimations);
                        builder.Add(Keys.AttackChainLevel, shared.m_attack.m_attackChainLevels);
                        builder.Add(Keys.LoopingAttack, shared.m_attack.m_loopingAttack);
                        builder.Add(Keys.ConsumeItem, shared.m_attack.m_consumeItem);
                        builder.Add(Keys.HitTerrain, shared.m_attack.m_hitTerrain);
                        builder.Add(Keys.IsHomeItem, shared.m_attack.m_isHomeItem);
                        builder.Add(Keys.AttackStamina, shared.m_attack.m_attackStamina);
                        builder.Add(Keys.EitrUse, shared.m_attack.m_attackEitr);
                        builder.Add(Keys.HealthUse, shared.m_attack.m_attackHealth);
                        builder.Add(Keys.HealthUsePercentage, shared.m_attack.m_attackHealthPercentage, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.StaminaHold, shared.m_attack.m_drawStaminaDrain);
                        builder.Add(Keys.SpeedFactor, shared.m_attack.m_speedFactor);
                        builder.Add(Keys.SpeedFactorRotation, shared.m_attack.m_speedFactorRotation);
                        builder.Add(Keys.AttackStartNoise, shared.m_attack.m_attackStartNoise);
                        // Secondary Attack
                        builder.Add(Keys.Secondary);
                        builder.Add(Keys.AttackType, shared.m_secondaryAttack.m_attackType);
                        builder.Add(Keys.AttackAnimation, shared.m_secondaryAttack.m_attackAnimation);
                        builder.Add(Keys.AttackRandomAnimation, shared.m_secondaryAttack.m_attackRandomAnimations);
                        builder.Add(Keys.AttackChainLevel, shared.m_secondaryAttack.m_attackChainLevels);
                        builder.Add(Keys.LoopingAttack, shared.m_secondaryAttack.m_loopingAttack);
                        builder.Add(Keys.ConsumeItem, shared.m_secondaryAttack.m_consumeItem);
                        builder.Add(Keys.HitTerrain, shared.m_secondaryAttack.m_hitTerrain);
                        builder.Add(Keys.IsHomeItem, shared.m_secondaryAttack.m_isHomeItem);
                        builder.Add(Keys.StaminaUse, shared.m_secondaryAttack.m_attackStamina);
                        builder.Add(Keys.EitrUse, shared.m_secondaryAttack.m_attackEitr);
                        builder.Add(Keys.HealthUse, shared.m_secondaryAttack.m_attackHealth);
                        builder.Add(Keys.HealthUsePercentage, shared.m_secondaryAttack.m_attackHealthPercentage, Entries.EntryBuilder.Option.Percentage);
                        builder.Add(Keys.StaminaHold, shared.m_secondaryAttack.m_drawStaminaDrain);
                        builder.Add(Keys.SpeedFactor, shared.m_secondaryAttack.m_speedFactor);
                        builder.Add(Keys.SpeedFactorRotation, shared.m_secondaryAttack.m_speedFactorRotation);
                        builder.Add(Keys.AttackStartNoise, shared.m_secondaryAttack.m_attackStartNoise);
                    }
                    if (itemData.IsPartOfSet())
                    {
                        builder.Add(Keys.Set);
                        builder.Add(Keys.SetName, shared.m_setName);
                        builder.Add(Keys.ItemParts, shared.m_setSize);
                        builder.Add(Keys.SetEffect, shared.m_setStatusEffect);
                        if (shared.m_setStatusEffect is {} setStatus)
                        {
                            builder.Add(setStatus.GetTooltipString(), "lore");
                            foreach (Skills.SkillType skill in Skills.s_allSkills)
                            {
                                float amount = 0f;
                                setStatus.ModifySkillLevel(skill, ref amount);
                                builder.Add(skill, amount);
                            }
                        }
                    }
                    break;
            }
            return builder.ToList();
        }
        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element? element)
        {
            if (element != null) panel.elementView.SetSelected(element);
            panel.description.Reset();
            panel.description.SetName(shared.m_name);
            panel.description.SetIcon(GetIcon());
            if (AlmanacPanel.isLocalAdminOrHostAndNoCost)
            {
                panel.description.Interactable(true);
                panel.description.SetButtonText(Keys.Spawn);
                GameObject? gameObject = prefab;
                panel.OnMainButton = () =>
                {
                    GameObject go = Object.Instantiate(gameObject, Player.m_localPlayer.transform.position, Quaternion.identity);
                    go.GetComponent<ItemDrop>().m_itemData.m_worldLevel = Game.m_worldLevel;
                };
            }
            panel.description.view.CreateTextArea().SetText(shared.m_description + "\n\n");
            if (itemData.IsPartOfSet())
            {
                panel.description.view.CreateTitle().SetTitle($"{shared.m_setName} ({shared.m_setSize})");
                if (setItems.Count > 4)
                {
                    IEnumerable<List<ItemDrop>> batches = setItems.ToList().Batch(4);
                    foreach (List<ItemDrop>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else panel.description.view.CreateIcons().SetIcons(setItems.ToArray());
            }
            ToEntries().Build(panel.description.view);
            if (itemData.HasVariants())
            {
                panel.description.view.CreateTitle().SetTitle(Keys.Variant);
                List<Sprite> icons = Icons.Skip(1).ToList();
                if (icons.Count > 4)
                {
                    IEnumerable<List<Sprite>> batches = icons.Batch(4);
                    foreach (List<Sprite>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else
                {
                    panel.description.view.CreateIcons().SetIcons(icons.ToArray());
                }
            }
            if (droppedBy.Count > 0)
            {
                panel.description.view.CreateTitle().SetTitle(Keys.DroppedBy);
                if (droppedBy.Count > 4)
                {
                    IEnumerable<List<CritterHelper.CritterInfo>> batches = droppedBy.ToList().Batch(4);
                    foreach (List<CritterHelper.CritterInfo>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else panel.description.view.CreateIcons().SetIcons(droppedBy.ToArray());
            }
            if (IsUsedInOtherRecipes || IsUsedInPieces)
            {
                panel.description.view.CreateTitle().SetTitle(Keys.UsedIn);
                if (IsUsedInOtherRecipes)
                {
                    if (usedIn.Count > 4)
                    {
                        IEnumerable<List<Recipe>> batches = usedIn.ToList().Batch(4);
                        foreach (List<Recipe>? batch in batches)
                        {
                            panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                        }
                    }
                    else
                    {
                        panel.description.view.CreateIcons().SetIcons(usedIn.ToArray());
                    }
                }

                if (IsUsedInPieces)
                {
                    if (usedInPieces.Count > 4)
                    {
                        IEnumerable<List<PieceHelper.PieceInfo>> batches = usedInPieces.ToList().Batch(4);
                        foreach (List<PieceHelper.PieceInfo>? batch in batches)
                        {
                            panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                        }
                    }
                    else
                    {
                        panel.description.view.CreateIcons().SetIcons(usedInPieces.ToArray());
                    }
                }
            }
            if (fish != null)
            {
                panel.description.view.CreateTitle().SetTitle(Keys.Baits);
                if (itemBaits.Count > 4)
                {
                    IEnumerable<List<ItemInfo>> batches = itemBaits.ToList().Batch(4);
                    foreach (List<ItemInfo>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else panel.description.view.CreateIcons().SetIcons(itemBaits.ToArray());
                
            }
            if (recipe is not null)
            {
                panel.description.requirements.Set(recipe);
                panel.OnUpdate = _ => panel.description.requirements.Update();
            }
            panel.description.view.Resize();
        }
    }
    private static readonly Dictionary<string, ItemDrop> m_itemBySharedName = new();
    public static bool TryGetItemBySharedName(string sharedName, out ItemDrop itemDrop) => m_itemBySharedName.TryGetValue(sharedName, out itemDrop);
    public static void Setup()
    {
        AlmanacPlugin.OnObjectDBPrefabs += OnObjectDBPrefabs;
    }
    private static void OnObjectDBPrefabs(GameObject prefab)
    {
        if (prefab == null || !prefab.TryGetComponent(out ItemDrop component)) return;
        SpriteManager.OnItemHelperItem(component);
        m_itemBySharedName[component.m_itemData.m_shared.m_name] = component;
        _ = new ItemInfo(prefab);
    }
    
}