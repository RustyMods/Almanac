﻿using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.FileSystem;
using BepInEx;
using UnityEngine;

namespace Almanac.Data;

public static class ItemDataCollector
{
    private static List<ItemDrop> CachedScrolls = new();
    private static List<ItemDrop> CachedJewels = new();
    private static List<ItemDrop> CachedMaterials = new();
    private static List<ItemDrop> CachedAmmo = new();
    private static List<ItemDrop> CachedWeapons = new();
    private static List<ItemDrop> CachedEquipment = new();
    private static List<ItemDrop> CachedConsumables = new();
    private static List<ItemDrop> CachedFishes = new();
    private static List<ItemDrop> CachedTrophies = new();
    public static void ClearCachedItemDrops()
    {
        CachedScrolls.Clear();
        CachedJewels.Clear();
        CachedMaterials.Clear();
        CachedAmmo.Clear();
        CachedWeapons.Clear();
        CachedEquipment.Clear();
        CachedConsumables.Clear();
        CachedFishes.Clear();
        CachedTrophies.Clear();
    }
    public static List<ItemDrop> GetTrophies(string filter = "")
    {
        if (CachedTrophies.Count > 0)
            return filter.IsNullOrWhiteSpace()
                ? CachedTrophies
                : CachedTrophies.FindAll(item =>
                    Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
        
        CachedTrophies = GetValidItemDropList(ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Trophy, ""));

        return filter.IsNullOrWhiteSpace() ? CachedTrophies : CachedTrophies.FindAll(item =>
            Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    } 
    public static List<ItemDrop> GetFishes(string filter = "")
    {
        if (CachedFishes.Count <= 0)
        {
            List<ItemDrop> fishes = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Fish, "");
            
            if (AlmanacPlugin.KrumpacLoaded)
            {
                List<ItemDrop> KrumpFishes = GetConsumables(true).FindAll(x =>
                    x.name.StartsWith("Krump_Mat_") 
                    && !x.name.EndsWith("Dried") 
                    && !x.name.Contains("Krump_Mat_Oil") 
                    && !x.name.EndsWith("_Meat"));
                fishes.AddRange(KrumpFishes);
            }
            
            CachedFishes = fishes.FindAll(fish => !Filters.FilterList.Contains(fish.name) && fish.enabled);
        }

        return filter.IsNullOrWhiteSpace() ? CachedFishes : CachedFishes.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetScrolls(string filter = "")
    {
        if (CachedEquipment.Count <= 0) GetEquipments();
        var scrolls = CachedScrolls.Count > 0 ? CachedScrolls : CachedEquipment.FindAll(x => x.name.StartsWith("kg"));
        return filter.IsNullOrWhiteSpace() ? scrolls : scrolls.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetJewels(string filter = "")
    {
        if (CachedJewels.Count <= 0)
        {
            List<ItemDrop> jewels = new();
            if (CachedMaterials.Count <= 0) GetMaterials();
            FilterJewels(CachedMaterials, jewels);

            if (CachedEquipment.Count <= 0) GetEquipments();
            FilterJewels(CachedEquipment, jewels);
            CachedJewels = GetValidItemDropList(jewels);
        }
        return filter.IsNullOrWhiteSpace() ? CachedJewels : CachedJewels.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    private static void FilterJewels(List<ItemDrop> source, List<ItemDrop> destination)
    {
        foreach (ItemDrop item in source)
        {
            string localizedName = Localization.instance.Localize(item.m_itemData.m_shared.m_name);
            string name = item.name;
            Recipe? resources = ObjectDB.instance.GetRecipe(item.m_itemData);
            if (resources)
            {
                CraftingStation station = resources.m_craftingStation;
                if (station && station.name == "op_transmution_table")
                {
                    destination.Add(item);
                    continue;
                }
            }
            if (name.Contains("Gem")
                || localizedName.Contains("Gem")
                || name.EndsWith("Gem")
                || name.StartsWith("JC_")
                || name == "Soulcatcher_CursedDoll"
                || name.Contains("_Crystal")
                || name.Contains("_Socket"))
            {
                destination.Add(item);
            }
        }
    }
    public static List<ItemDrop> GetConsumables(bool forFishes = false, string filter = "")
    {
        if (forFishes) return ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, "");
        if (CachedConsumables.Count > 0) return filter.IsNullOrWhiteSpace() ? CachedConsumables : CachedConsumables.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
        List<ItemDrop> consumables = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, "");
        if (AlmanacPlugin.KrumpacLoaded) consumables.RemoveAll(x => x.name.EndsWith("_Raw"));

        CachedConsumables = GetValidItemDropList(consumables);

        return filter.IsNullOrWhiteSpace() ? CachedConsumables : CachedConsumables.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetAmmunition(string filter = "")
    {
        if (CachedAmmo.Count <= 0)
        {
            List<ItemDrop> ammo = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Ammo, "");
            List<ItemDrop> ammoNonEquip = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.AmmoNonEquipable, "");
            
            List<ItemDrop> ammunition = new List<ItemDrop>();
            ammunition.AddRange(ammo);
            ammunition.AddRange(ammoNonEquip);

            CachedAmmo = GetValidItemDropList(ammunition);
        }
        return filter.IsNullOrWhiteSpace() ? CachedAmmo : CachedAmmo.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetMaterials(string filter = "")
    {
        if (AlmanacPlugin.JewelCraftLoaded && CachedMaterials.Count != 0)
        {
            if (CachedJewels.Count <= 0) GetJewels();
            CachedMaterials.RemoveAll(x => CachedJewels.Contains(x));
        }
        
        if (CachedMaterials.Count > 0) return filter.IsNullOrWhiteSpace() ? CachedMaterials : CachedMaterials.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
        
        List<ItemDrop> materials = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, "");
        
        materials.AddRange(GetFilteredMisc(filterOption.toMaterials));
        
        CachedMaterials = GetValidItemDropList(materials);
        return filter.IsNullOrWhiteSpace() ? CachedMaterials : CachedMaterials.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetWeapons(string filter = "")
    {
        if (CachedWeapons.Count > 0)
            return filter.IsNullOrWhiteSpace()
                ? CachedWeapons
                : CachedWeapons.FindAll(item =>
                    Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
        
        List<ItemDrop> oneHanded = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.OneHandedWeapon, "");
        List<ItemDrop> bow = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Bow, "");
        List<ItemDrop> shield = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Shield, "");
        List<ItemDrop> hands = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Hands, "");
        List<ItemDrop> twoHanded = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.TwoHandedWeapon, "");
        List<ItemDrop> torch = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Torch, "");
        List<ItemDrop> tool = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Tool, "");
        List<ItemDrop> attachAtgeir = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Attach_Atgeir, "");
        List<ItemDrop> twoHandedLeft = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft, "");
        
        List<ItemDrop> weaponList = new List<ItemDrop>();
        weaponList.AddRange(oneHanded);
        weaponList.AddRange(bow);
        weaponList.AddRange(shield);
        weaponList.AddRange(twoHanded);
        weaponList.AddRange(hands);
        weaponList.AddRange(torch);
        weaponList.AddRange(tool);
        weaponList.AddRange(attachAtgeir);
        weaponList.AddRange(twoHandedLeft);

        CachedWeapons = GetValidItemDropList(weaponList);

        return filter.IsNullOrWhiteSpace() ? CachedWeapons :CachedWeapons.FindAll(item =>
            Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetEquipments(string filter = "")
    {
        if (AlmanacPlugin.JewelCraftLoaded && CachedEquipment.Count != 0)
        {
            CachedEquipment.RemoveAll(x => CachedJewels.Contains(x));
        }

        if (AlmanacPlugin.KGEnchantmentLoaded && CachedEquipment.Count != 0)
        {
            CachedEquipment.RemoveAll(x => CachedScrolls.Contains(x));
        }

        if (CachedEquipment.Count > 0) return filter.IsNullOrWhiteSpace() ? CachedEquipment : CachedEquipment.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
        
        List<ItemDrop> helmet = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Helmet, "");
        List<ItemDrop> chest = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Chest, "");
        List<ItemDrop> legs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Legs, "");
        List<ItemDrop> shoulder = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Shoulder, "");
        List<ItemDrop> utility = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Utility, "");
        List<ItemDrop> customization = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "");
        
        List<ItemDrop> gearList = new List<ItemDrop>();
        gearList.AddRange(helmet);
        gearList.AddRange(chest);
        gearList.AddRange(legs);
        gearList.AddRange(shoulder);
        gearList.AddRange(utility);
        gearList.AddRange(customization);
        gearList.AddRange(GetFilteredMisc(filterOption.toMisc));
        
        CachedEquipment = new List<ItemDrop>(GetValidItemDropList(gearList).OrderBy(name => Localization.instance.Localize(name.m_itemData.m_shared.m_name)));

        return filter.IsNullOrWhiteSpace() ? CachedEquipment : CachedEquipment.FindAll(item => Localization.instance.Localize(item.m_itemData.m_shared.m_name).ToLower().Contains(filter));
    }
    public static List<ItemDrop> GetSwords() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("sword"));
    public static List<ItemDrop> GetAxes() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("axe"));
    public static List<ItemDrop> GetPoleArms() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("atgeir"));
    public static List<ItemDrop> GetSpears() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("spear"));
    public static List<ItemDrop> GetMaces() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("mace"));
    public static List<ItemDrop> GetKnives() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("knife"));
    public static List<ItemDrop> GetShields() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("shield"));
    public static List<ItemDrop> GetStaves() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("staff"));
    public static List<ItemDrop> GetBows() => (CachedWeapons.Count > 0 ? CachedWeapons : GetWeapons()).FindAll(item => item.name.ToLower().Contains("bow"));
    public static List<ItemDrop> GetValuables() => (CachedMaterials.Count > 0 ? CachedMaterials : GetMaterials()).FindAll(item => item.m_itemData.m_shared.m_value > 0);
    public static List<ItemDrop> GetPotions() => GetConsumables().FindAll(item => item.m_itemData.m_shared.m_consumeStatusEffect);
    enum filterOption { toMisc, toMaterials }
    private static List<ItemDrop> GetFilteredMisc(filterOption option)
    {
        List<ItemDrop> misc = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Misc, "");
        
        List<string> toMaterialsMap = new List<string>()
        {
            "Turnip",
            "GoblinTotem",
        };
        
        List<ItemDrop> filteredMisc = new List<ItemDrop>();
        List<ItemDrop> MiscToMaterials = new List<ItemDrop>();
        
        foreach (ItemDrop drop in misc)
        {
            if (drop.name.Contains("Egg")) MiscToMaterials.Add(drop);
            else if (drop.name.Contains("chest")) MiscToMaterials.Add(drop);
            else if (drop.name.Contains("Chest")) MiscToMaterials.Add(drop);
            else if (drop.name.Contains("Key")) MiscToMaterials.Add(drop);
            else if (toMaterialsMap.Contains(drop.name)) MiscToMaterials.Add(drop);
            else filteredMisc.Add(drop);
        }
        
        return option == filterOption.toMisc ? filteredMisc : MiscToMaterials;
    }
    private static List<ItemDrop> GetValidItemDropList(List<ItemDrop> list)
    {
        List<ItemDrop> output = new List<ItemDrop>();
        foreach (ItemDrop? itemDrop in list)
        {
            try
            {
                ItemDrop data = itemDrop;
                Sprite sprite = data.m_itemData.GetIcon();
                
                if (!sprite) continue;

                if (Filters.FilterList.Contains(itemDrop.name) && AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.On) continue;
                if (!itemDrop.enabled) continue;
                output.Add(data);
            }
            catch 
            {
                // ignored
            }
        }
        return output;
    }
}