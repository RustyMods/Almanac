using System.Collections.Generic;
using System.Linq;
using Almanac.FileSystem;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Almanac.Data;

public static class Items
{
    public static readonly List<Data> m_allItems = new();
    public static readonly Dictionary<string, List<Data>> m_setItems = new();
    public static readonly Dictionary<ItemDrop.ItemData.ItemType, List<Data>> m_typeToItem = new();
    public static readonly Dictionary<Skills.SkillType, List<Data>> m_skillToItem = new();
    public static readonly List<Data> m_jewels = new();
    public static readonly List<Data> m_valuables = new();
    public static readonly List<Data> m_potions = new();
    public static readonly List<Data> m_equipment = new();
    public static readonly List<Data> m_ammo = new();
    public static readonly List<Data> m_weapons = new();
    public static readonly List<Data> m_miscMaterials = new();
    public static readonly List<Data> m_creatureItems = new();
    public static readonly List<Data> m_dlc = new();
    public static readonly List<Data> m_tools = new();

    public static List<Data> GetTrophies()
    {
        var output = m_typeToItem.TryGetValue(ItemDrop.ItemData.ItemType.Trophy, out List<Data> list) ? list : new();
        return output.Where(x => !Filters.Ignore(x.m_item.name)).ToList();
    }

    public static List<Data> GetFishes()
    {
        if (!m_typeToItem.TryGetValue(ItemDrop.ItemData.ItemType.Fish, out var list))
            list = new List<Data>();
        list = list.Where(x => !Filters.Ignore(x.m_item.name)).ToList();
        if (!AlmanacPlugin.KrumpacLoaded)
            return list;

        var consumables = m_typeToItem.TryGetValue(ItemDrop.ItemData.ItemType.Consumable, out var consumes)
            ? consumes
            : new();

        var filteredConsumables = consumables.Where(x => IsKrumpacFishMaterial(x.m_item.name)).ToList();

        return list.Concat(filteredConsumables).ToList();
    }

    private static bool IsKrumpacFishMaterial(string name)
    {
        return name.StartsWith("Krump_Mat_") &&
               !name.EndsWith("Dried") &&
               !name.Contains("Krump_Mat_Oil") &&
               !name.EndsWith("_Meat");
    }


    public static List<Data> GetScrolls()
    {
        return m_equipment.Where(x => x.m_item.name.StartsWith("kg") && !Filters.Ignore(x.m_item.name)).ToList();
    }

    public static List<Data> GetJewels() => m_jewels.Where(x => !Filters.Ignore(x.m_item.name)).ToList();

    public static List<Data> GetConsumables()
    {
        return !m_typeToItem.TryGetValue(ItemDrop.ItemData.ItemType.Consumable, out var list) ? new List<Data>() : list.Where(x=>!Filters.Ignore(x.m_item.name)).ToList();
    }

    public static List<Data> GetAmmunition() => m_ammo.Where(x=>!Filters.Ignore(x.m_item.name)).ToList();

    public static List<Data> GetMaterials()
    {
        if (!m_typeToItem.TryGetValue(ItemDrop.ItemData.ItemType.Material, out var list))
            return new List<Data>();
        list = list.Where(x => !Filters.Ignore(x.m_item.name)).ToList();
        if (!AlmanacPlugin.JewelCraftLoaded)
            return list.Concat(m_miscMaterials).ToList();

        return list.Where(x => !m_jewels.Contains(x)).ToList();
    }

    public static List<Data> GetWeapons() => m_weapons.Where(x => x.GetIcon() != SpriteManager.AlmanacIcon).Where(x=>!Filters.Ignore(x.m_item.name)).ToList();

    public static List<Data> GetEquipment()
    {
        if (!AlmanacPlugin.JewelCraftLoaded) return m_equipment.Where(x => x.GetIcon() != SpriteManager.AlmanacIcon).Where(x=>!Filters.Ignore(x.m_item.name)).ToList();
        return m_equipment.Where(x => x.GetIcon() != SpriteManager.AlmanacIcon || m_jewels.Contains(x) || x.m_item.name.StartsWith("kg") || !Filters.Ignore(x.m_item.name)).ToList();
    }

    public static List<Data> GetWeaponBySkill(Skills.SkillType type)
    {
        var output = m_skillToItem.TryGetValue(type, out List<Data> list) ? list : new();
        return output.Where(x => !Filters.Ignore(x.m_item.name) && !IsTool(x.m_item)).ToList();
    }

    public static List<Data> GetStaves()
    {
        var blood = m_skillToItem.TryGetValue(Skills.SkillType.BloodMagic, out var bloodList) ? bloodList : new();
        var element = m_skillToItem.TryGetValue(Skills.SkillType.ElementalMagic, out var elementList) ? elementList : new();

        var list = blood.Concat(element).ToList();
        return list.Where(x => !Filters.Ignore(x.m_item.name)).ToList();
    }

    public static List<Data> GetValuables() => m_valuables.Where(x => !Filters.Ignore(x.m_item.name)).ToList();
    public static List<Data> GetPotions() => m_potions.Where(x => !Filters.Ignore(x.m_item.name)).ToList();

    
    public class Data
    {
        public List<Data> m_set = new();
        public readonly ItemDrop m_item;
        public readonly ItemDrop.ItemData.SharedData m_shared;
        public readonly Recipe? m_recipe;
        public readonly Sprite? m_icon;
        public readonly bool m_floating;

        public Data(ItemDrop item)
        {
            m_item = item;
            m_shared = item.m_itemData.m_shared;
            m_icon = GetIcon();
            if (m_icon == SpriteManager.AlmanacIcon)
            {
                m_creatureItems.Add(this);
                return;
            }

            m_allItems.Add(this);
            if (!m_shared.m_setName.IsNullOrWhiteSpace())
            {
                if (m_setItems.TryGetValue(m_shared.m_setName, out List<Data> setItems)) setItems.Add(this);
                else m_setItems[m_shared.m_setName] = new() { this };
            }
            if (!item.m_itemData.m_shared.m_dlc.IsNullOrWhiteSpace()) m_dlc.Add(this);
            if (m_typeToItem.TryGetValue(item.m_itemData.m_shared.m_itemType, out List<Data> list)) list.Add(this);
            else m_typeToItem[item.m_itemData.m_shared.m_itemType] = new() { this };
            if (IsJewel(item)) m_jewels.Add(this);
            if (IsAmmo(item.m_itemData.m_shared.m_itemType)) m_ammo.Add(this);
            if (item.m_itemData.m_shared.m_value > 0) m_valuables.Add(this);
            if (IsEquipment(item.m_itemData.m_shared.m_itemType)) m_equipment.Add(this);
            if (item.m_itemData.m_shared.m_consumeStatusEffect != null) m_potions.Add(this);
            if (IsWeapon(item.m_itemData.m_shared.m_itemType))
            {
                m_weapons.Add(this);
                if (m_skillToItem.TryGetValue(item.m_itemData.m_shared.m_skillType, out List<Data> weapons)) weapons.Add(this);
                else m_skillToItem[item.m_itemData.m_shared.m_skillType] = new() { this };
            }
            if (IsMiscMaterial(item)) m_miscMaterials.Add(this);
            m_recipe = ObjectDB.m_instance.GetRecipe(m_item.m_itemData);
            m_floating = item.gameObject.GetComponent<Floating>();
        }

        public Sprite? GetIcon()
        {
            try
            {
                return m_item.m_itemData.GetIcon();
            }
            catch
            {
                return SpriteManager.AlmanacIcon;
            }
        }

        private bool HasStatModifiers()
        {
            return m_shared.m_movementModifier 
                   + m_shared.m_eitrRegenModifier 
                   + m_shared.m_homeItemsStaminaModifier 
                   + m_shared.m_heatResistanceModifier
                   + m_shared.m_jumpStaminaModifier
                   + m_shared.m_attackStaminaModifier 
                   + m_shared.m_blockStaminaModifier 
                   + m_shared.m_dodgeStaminaModifier 
                   + m_shared.m_swimStaminaModifier
                   + m_shared.m_sneakStaminaModifier
                   + m_shared.m_runStaminaModifier
                   > 0f;
        }

        public List<Entries.Entry> GetEntries()
        {
            Entries.EntryBuilder builder = new();
            builder.Add(m_shared.m_description, "lore");
            builder.Add("$label_prefabname", m_item.name);
            builder.Add("$label_teleportable", m_shared.m_teleportable);
            builder.Add("$item_value", m_shared.m_value);
            builder.Add("$item_weight", m_shared.m_weight);
            builder.Add("$item_quality", m_shared.m_maxQuality);
            builder.Add("$item_durability", m_shared.m_maxDurability, m_shared.m_durabilityPerLevel, "/lvl");
            builder.Add("$label_canberepaired", m_shared.m_canBeReparied);
            if (m_recipe is { } recipe)
            {
                builder.Add("$item_repairlevel", recipe.m_minStationLevel);
            }
            builder.Add("$label_maxstacksize", m_shared.m_maxStackSize);
            builder.Add("$label_variant", m_shared.m_icons.Length);
            builder.Add("$label_itemtype", m_shared.m_itemType);
            builder.Add("$label_questitem", m_shared.m_questItem);
            builder.Add("$label_equipduration", m_shared.m_equipDuration, Entries.EntryBuilder.Option.Seconds);
            builder.Add("$label_floating", m_floating);
            Skills.SkillType[] allSkills = Skills.s_allSkills;
            switch (m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Fish:
                    if (m_item.TryGetComponent(out Fish fish))
                    {
                        builder.Add("$title_fish");
                        builder.Add("$label_swimrange", fish.m_swimRange);
                        builder.Add("$label_mindepth", fish.m_minDepth);
                        builder.Add("$label_speed", fish.m_speed);
                        builder.Add("$label_acceleration", fish.m_acceleration);
                        builder.Add("$label_turnrate", fish.m_turnRate);
                        builder.Add("$label_avoidrange", fish.m_avoidRange);
                        builder.Add("$label_height", fish.m_height);
                        builder.Add("$label_hookforce", fish.m_hookForce);
                        builder.Add("$label_staminause", fish.m_staminaUse);
                        builder.Add("$label_escapestaminause", fish.m_escapeStaminaUse);
                        builder.Add("$label_escape", fish.m_escapeMin, fish.m_escapeMax, "-");
                        builder.Add("$label_basehookchance", fish.m_baseHookChance);
                        builder.Add("$label_jumpspeed", fish.m_jumpSpeed);
                        builder.Add("$label_jumpheight", fish.m_jumpHeight);
                        builder.Add("$label_jumponlandchance", fish.m_jumpOnLandChance);
                        builder.Add("$label_jumpfrequency", fish.m_jumpFrequencySeconds, Entries.EntryBuilder.Option.Seconds);
                        builder.Add("$label_fishfast", fish.m_fast);
                        builder.Add("$title_baits");
                        foreach (var bait in fish.m_baits)
                        {
                            builder.Add(bait.m_bait.m_itemData.m_shared.m_name, bait.m_chance, Entries.EntryBuilder.Option.Percentage);
                        }
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Helmet or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Chest or ItemDrop.ItemData.ItemType.Customization or ItemDrop.ItemData.ItemType.Shoulder:
                    builder.Add("$item_armor");
                    builder.Add("$item_armor", m_shared.m_armor, m_shared.m_armorPerLevel, "/lvl");
                    builder.Add("$label_material", m_shared.m_armorMaterial);
                    builder.Add("$label_hidehelmet", m_shared.m_helmetHideHair);
                    builder.Add("$label_hidebeard", m_shared.m_helmetHideBeard);
                    if (m_shared.m_damageModifiers.Count > 0)
                    {
                        builder.Add("$title_resistances");
                        foreach (var resistance in m_shared.m_damageModifiers) builder.Add(resistance);
                    }

                    if (m_shared.m_equipStatusEffect is { } equipStatus)
                    {
                        builder.Add("$item_equipeeffect");
                        builder.Add("$item_equipeeffect", equipStatus.m_name);
                        foreach (var skill in allSkills)
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
                        builder.Add("$se_falldamage", fallDamage);
                        builder.Add("$se_healthregen", healthRegen);
                        builder.Add("$se_staminaregen", staminaRegen);
                        builder.Add("$se_eitrregen", eitrRegen);
                        HitData.DamageModifiers modifiers = new();
                        equipStatus.ModifyDamageMods(ref modifiers);
                        builder.Add("$inventory_blunt", modifiers.m_blunt);
                        builder.Add("$inventory_slash", modifiers.m_slash);
                        builder.Add("$inventory_pierce", modifiers.m_pierce);
                        builder.Add("$inventory_chop", modifiers.m_chop);
                        builder.Add("$inventory_pickaxe", modifiers.m_pickaxe);
                        builder.Add("$inventory_fire", modifiers.m_fire);
                        builder.Add("$inventory_frost", modifiers.m_frost);
                        builder.Add("$inventory_lightning", modifiers.m_lightning);
                        builder.Add("$inventory_poison", modifiers.m_poison);
                        builder.Add("$inventory_spirit", modifiers.m_spirit);
                    }

                    if (HasStatModifiers())
                    {
                        builder.Add("$title_statmodifier");
                        builder.Add("$item_movement_modifier", m_shared.m_movementModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$item_eitrregen_modifier", m_shared.m_eitrRegenModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$item_homeitem_modifier", m_shared.m_homeItemsStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$item_heat_modifier", m_shared.m_heatResistanceModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_jumpstamina", m_shared.m_jumpStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_attackstamina", m_shared.m_attackStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_blockstamina", m_shared.m_blockStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_dodgestamina", m_shared.m_dodgeStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_swimstamina", m_shared.m_swimStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_sneakstamina", m_shared.m_sneakStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                        builder.Add("$se_runstamina", m_shared.m_runStaminaModifier, Entries.EntryBuilder.Option.Percentage);
                    }
                    if (!m_shared.m_setName.IsNullOrWhiteSpace())
                    {
                        builder.Add("$title_set");
                        builder.Add("$text_name", m_shared.m_setName);
                        builder.Add("$item_parts", m_shared.m_setSize);
                        builder.Add("$item_seteffect", m_shared.m_setStatusEffect);
                        if (m_shared.m_setStatusEffect is {} setStatus)
                        {
                            builder.Add(setStatus.GetTooltipString(), "lore");
                            foreach (var skill in allSkills)
                            {
                                float amount = 0f;
                                setStatus.ModifySkillLevel(skill, ref amount);
                                builder.Add(skill, amount);
                            }
                        }
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Consumable:
                    builder.Add("$title_food");
                    builder.Add("$item_food_health", m_shared.m_food);
                    builder.Add("$item_food_stamina", m_shared.m_foodStamina);
                    builder.Add("$item_food_eitr", m_shared.m_foodEitr);
                    builder.Add("$item_food_duration", m_shared.m_foodBurnTime);
                    builder.Add("$item_food_regen", m_shared.m_foodRegen);
                    if (m_shared.m_consumeStatusEffect is { } consumeStatus)
                    {
                        builder.Add("$se_consumeeffect", consumeStatus.m_name);
                        builder.Add("$se_category", consumeStatus.m_category);
                        builder.Add("$item_food_duration", consumeStatus.m_ttl, Entries.EntryBuilder.Option.Seconds);
                        builder.Add(consumeStatus.GetTooltipString(), "lore");
                        foreach (var skill in allSkills)
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
                        builder.Add("$item_falldamage", fallDamage);
                        builder.Add("$se_healthregen", healthRegen);
                        builder.Add("$se_staminaregen", staminaRegen);
                        builder.Add("$se_eitrregen", eitrRegen);
                        HitData.DamageModifiers modifiers = new();
                        consumeStatus.ModifyDamageMods(ref modifiers);
                        builder.Add("$inventory_blunt", modifiers.m_blunt);
                        builder.Add("$inventory_slash", modifiers.m_slash);
                        builder.Add("$inventory_pierce", modifiers.m_pierce);
                        builder.Add("$inventory_chop", modifiers.m_chop);
                        builder.Add("$inventory_pickaxe", modifiers.m_pickaxe);
                        builder.Add("$inventory_fire", modifiers.m_fire);
                        builder.Add("$inventory_frost", modifiers.m_frost);
                        builder.Add("$inventory_lightning", modifiers.m_lightning);
                        builder.Add("$inventory_poison", modifiers.m_poison);
                        builder.Add("$inventory_spirit", modifiers.m_spirit);
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Shield:
                    builder.Add("$title_shield");
                    builder.Add("$item_blockarmor", m_shared.m_blockPower, m_shared.m_blockPowerPerLevel, "/lvl");
                    builder.Add("$item_blockforce", m_shared.m_deflectionForce, m_shared.m_deflectionForcePerLevel, "/lvl");
                    builder.Add("$item_parrybonus", m_shared.m_timedBlockBonus);
                    if (!m_shared.m_setName.IsNullOrWhiteSpace())
                    {
                        builder.Add("$title_set");
                        builder.Add("$label_setname", m_shared.m_setName);
                        builder.Add("$item_parts", m_shared.m_setSize);
                        builder.Add("$item_seteffect", m_shared.m_setStatusEffect);
                        if (m_shared.m_setStatusEffect is {} setStatus)
                        {
                            builder.Add(setStatus.GetTooltipString(), "lore");
                            foreach (var skill in allSkills)
                            {
                                float amount = 0f;
                                setStatus.ModifySkillLevel(skill, ref amount);
                                builder.Add(skill, amount);
                            }
                        }
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Ammo or ItemDrop.ItemData.ItemType.Bow or ItemDrop.ItemData.ItemType.Hands or ItemDrop.ItemData.ItemType.Tool or ItemDrop.ItemData.ItemType.Torch or ItemDrop.ItemData.ItemType.Attach_Atgeir or ItemDrop.ItemData.ItemType.AmmoNonEquipable or ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                    builder.Add("$title_weapon");
                    builder.Add("$label_animationstate", m_shared.m_animationState);
                    builder.Add("$label_skilltype", m_shared.m_skillType);
                    builder.Add("$label_tooltier", m_shared.m_toolTier);
                    builder.Add("$inventory_damage", m_shared.m_damages.m_damage);
                    builder.Add("$inventory_blunt", m_shared.m_damages.m_blunt);
                    builder.Add("$inventory_slash", m_shared.m_damages.m_slash);
                    builder.Add("$inventory_pierce", m_shared.m_damages.m_pierce);
                    builder.Add("$inventory_chop", m_shared.m_damages.m_chop);
                    builder.Add("$inventory_pickaxe", m_shared.m_damages.m_pickaxe);
                    builder.Add("$inventory_fire", m_shared.m_damages.m_fire);
                    builder.Add("$inventory_frost", m_shared.m_damages.m_frost);
                    builder.Add("$inventory_lightning", m_shared.m_damages.m_lightning);
                    builder.Add("$inventory_poison", m_shared.m_damages.m_poison);
                    builder.Add("$inventory_spirit", m_shared.m_damages.m_spirit);
                    builder.Add("$label_attackforce", m_shared.m_attackForce);
                    builder.Add("$label_dodgeable", m_shared.m_dodgeable);
                    builder.Add("$label_blockable", m_shared.m_blockable);
                    builder.Add("$label_tameonly", m_shared.m_tamedOnly);
                    builder.Add("$label_alwaysrotate", m_shared.m_alwaysRotate);
                    builder.Add("$label_attackeffect", m_shared.m_attackStatusEffect);
                    builder.Add("$item_chancetoapplyse", m_shared.m_attackStatusEffectChance);
                    builder.Add("$label_spawnonhit", m_shared.m_spawnOnHit);
                    builder.Add("$label_spawnonhitterrain", m_shared.m_spawnOnHitTerrain);
                    builder.Add("$label_projectiletooltip", m_shared.m_projectileToolTip);
                    builder.Add("$label_ammotype", m_shared.m_ammoType);
                    // Attack
                    builder.Add("$title_attack");
                    builder.Add("$label_attacktype", m_shared.m_attack.m_attackType);
                    builder.Add("$label_attackanimation", m_shared.m_attack.m_attackAnimation);
                    builder.Add("$label_attackrandomanimations", m_shared.m_attack.m_attackRandomAnimations);
                    builder.Add("$label_attackchainlevels", m_shared.m_attack.m_attackChainLevels);
                    builder.Add("$label_loopingattack", m_shared.m_attack.m_loopingAttack);
                    builder.Add("$label_consumeitem", m_shared.m_attack.m_consumeItem);
                    builder.Add("$label_hitterrain", m_shared.m_attack.m_hitTerrain);
                    builder.Add("$label_ishomeitem", m_shared.m_attack.m_isHomeItem);
                    builder.Add("$label_attackstamina", m_shared.m_attack.m_attackStamina);
                    builder.Add("$item_eitruse", m_shared.m_attack.m_attackEitr);
                    builder.Add("$item_healthuse", m_shared.m_attack.m_attackHealth);
                    builder.Add("$item_healthuse_percentage", m_shared.m_attack.m_attackHealthPercentage, Entries.EntryBuilder.Option.Percentage);
                    builder.Add("$item_staminahold", m_shared.m_attack.m_drawStaminaDrain);
                    builder.Add("$label_speedfactor", m_shared.m_attack.m_speedFactor);
                    builder.Add("$label_speedfactorrotation", m_shared.m_attack.m_speedFactorRotation);
                    builder.Add("$label_attackstartnoise", m_shared.m_attack.m_attackStartNoise);
                    // Secondary Attack
                    builder.Add("$title_attacksecondary");
                    builder.Add("$label_attacktype", m_shared.m_secondaryAttack.m_attackType);
                    builder.Add("$label_attackanimation", m_shared.m_secondaryAttack.m_attackAnimation);
                    builder.Add("$label_attackrandomanimations", m_shared.m_secondaryAttack.m_attackRandomAnimations);
                    builder.Add("$label_attackchainlevels", m_shared.m_secondaryAttack.m_attackChainLevels);
                    builder.Add("$label_loopingattack", m_shared.m_secondaryAttack.m_loopingAttack);
                    builder.Add("$label_consumeitem", m_shared.m_secondaryAttack.m_consumeItem);
                    builder.Add("$label_hitterrain", m_shared.m_secondaryAttack.m_hitTerrain);
                    builder.Add("$label_ishomeitem", m_shared.m_secondaryAttack.m_isHomeItem);
                    builder.Add("$item_staminause", m_shared.m_secondaryAttack.m_attackStamina);
                    builder.Add("$item_eitruse", m_shared.m_secondaryAttack.m_attackEitr);
                    builder.Add("$item_healthuse", m_shared.m_secondaryAttack.m_attackHealth);
                    builder.Add("$item_healthuse_percentage", m_shared.m_secondaryAttack.m_attackHealthPercentage, Entries.EntryBuilder.Option.Percentage);
                    builder.Add("$item_staminahold", m_shared.m_secondaryAttack.m_drawStaminaDrain);
                    builder.Add("$label_speedfactor", m_shared.m_secondaryAttack.m_speedFactor);
                    builder.Add("$label_speedfactorrotation", m_shared.m_secondaryAttack.m_speedFactorRotation);
                    builder.Add("$label_attackstartnoise", m_shared.m_secondaryAttack.m_attackStartNoise);
                    if (!m_shared.m_setName.IsNullOrWhiteSpace())
                    {
                        builder.Add("$title_set");
                        builder.Add("$label_setname", m_shared.m_setName);
                        builder.Add("$item_parts", m_shared.m_setSize);
                        builder.Add("$item_seteffect", m_shared.m_setStatusEffect);
                        if (m_shared.m_setStatusEffect is {} setStatus)
                        {
                            builder.Add(setStatus.GetTooltipString(), "lore");
                            foreach (var skill in allSkills)
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
    }

    public static bool IsBait(ItemDrop item)
    {
        return item.name.ToLower().Contains("bait");
    }

    public static bool IsTool(ItemDrop item)
    {
        List<string> toolNames = new()
        {
            "Cultivator", "Hammer", "Hoe", "Tankard", "Tankard_dvergr", "Lantern", "TankardAnniversary"
        };
        if (item.m_itemData.m_shared.m_buildPieces) return true;
        if (item.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Torch) return true;
        if (toolNames.Contains(item.name)) return true;
        return false;
    }

    private static bool IsEquipment(ItemDrop.ItemData.ItemType type)
    {
        return type is ItemDrop.ItemData.ItemType.Helmet or ItemDrop.ItemData.ItemType.Chest
            or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Shoulder
            or ItemDrop.ItemData.ItemType.Utility or ItemDrop.ItemData.ItemType.Customization;
    }

    private static bool IsWeapon(ItemDrop.ItemData.ItemType type) => type is ItemDrop.ItemData.ItemType.OneHandedWeapon
        or ItemDrop.ItemData.ItemType.Bow or ItemDrop.ItemData.ItemType.Shield or ItemDrop.ItemData.ItemType.Hands
        or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.Torch
        or ItemDrop.ItemData.ItemType.Tool or ItemDrop.ItemData.ItemType.Attach_Atgeir
        or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft;

    private static bool IsAmmo(ItemDrop.ItemData.ItemType type) =>
        type is ItemDrop.ItemData.ItemType.Ammo or ItemDrop.ItemData.ItemType.AmmoNonEquipable;

    private static bool IsJewel(ItemDrop item)
    {
        if (!AlmanacPlugin.JewelCraftLoaded) return false;
        List<string> tables = new()
        {
            "Odins_Stone_Transmuter",
            "op_transmution_table",
            "Odins_Jewelry_Box",
            "JC_CrystalBall_Ext",
            "JC_Gemstone_Furnace"
        };
        if (ObjectDB.instance.GetRecipe(item.m_itemData) is { } recipe && recipe.m_craftingStation is {} craftingStation)
        {
            if (tables.Contains(craftingStation.name)) return true;
        }
        string localizedName = Localization.instance.Localize(item.m_itemData.m_shared.m_name);
        string name = item.name;
        if (name.Contains("Gem") || localizedName.Contains("Gem") || name.StartsWith("JC_") ||
            name == "Soulcatcher_CursedDoll" || name.Contains("_Crystal") || name.Contains("_Socket")) return true;
        return false;
    }

    private static bool IsMiscMaterial(ItemDrop item)
    {
        var name = item.name;
        return name.Contains("Egg") || name.Contains("chest") || name.Contains("Key") || name is "Turnip" or "GoblinTotem";
    }

    public static void Init()
    {
        if (!ObjectDB.instance || !ZNetScene.instance) return;
        m_typeToItem.Clear();
        m_skillToItem.Clear();
        m_jewels.Clear();
        m_valuables.Clear();
        m_potions.Clear();
        foreach (var prefab in ObjectDB.instance.m_items)
        {
            if (!prefab.TryGetComponent(out ItemDrop component)) continue;
            var _ = new Data(component);
        }

        foreach (var data in m_allItems)
        {
            if (data.m_shared.m_setName.IsNullOrWhiteSpace()) continue;
            if (m_setItems.TryGetValue(data.m_shared.m_setName, out List<Data> items)) data.m_set = items;
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix() => Init();
    }
}