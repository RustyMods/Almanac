using System;
using System.Collections.Generic;

namespace Almanac.Marketplace;

public static class JewelCraft
{
    private const string JEWELCRAFTING_ITEMDATA_CUSTOMDATA_SOCKET_KEY = "org.bepinex.plugins.jewelcrafting#Jewelcrafting.Sockets";

    public static bool HasSockets(this ItemDrop.ItemData itemData) => itemData.m_customData.ContainsKey(JEWELCRAFTING_ITEMDATA_CUSTOMDATA_SOCKET_KEY);

    public static string[] GetSocketGems(this ItemDrop.ItemData itemData)
    {
        if (!itemData.m_customData.TryGetValue(JEWELCRAFTING_ITEMDATA_CUSTOMDATA_SOCKET_KEY, out string sockets)) return Array.Empty<string>();
        return sockets.Split(',');
    }

    private static List<ItemDrop> GetSockets(this ItemDrop.ItemData itemData)
    {
        if (!itemData.m_customData.TryGetValue(JEWELCRAFTING_ITEMDATA_CUSTOMDATA_SOCKET_KEY, out string data)) return new();
        string[] itemIDs = data.Split(',');
        List<ItemDrop> itemDrops = new();
        foreach (string itemName in itemIDs)
        {
            if (ObjectDB.instance.GetItemPrefab(itemName.Trim()) is not { } gem || !gem.TryGetComponent(out ItemDrop component)) continue;
            itemDrops.Add(component);
        }
        return itemDrops;
    }

    public static List<string> GetSocketedGemSharedNames(this ItemDrop.ItemData itemData)
    {
        List<string> sharedNames = new();
        foreach(var item in itemData.GetSockets()) sharedNames.Add(item.m_itemData.m_shared.m_name);
        return sharedNames;
    }

    public static bool HasSockets(this MarketManager.MarketItem marketItem) =>
        marketItem.CustomData.ContainsKey(JEWELCRAFTING_ITEMDATA_CUSTOMDATA_SOCKET_KEY);

    private static List<ItemDrop> GetSockets(this MarketManager.MarketItem marketItem)
    {
        if (!marketItem.CustomData.TryGetValue(JEWELCRAFTING_ITEMDATA_CUSTOMDATA_SOCKET_KEY, out string data)) return new();
        string[] itemIDs = data.Split(',');
        List<ItemDrop> itemDrops = new();
        foreach (string itemName in itemIDs)
        {
            if (ObjectDB.instance.GetItemPrefab(itemName.Trim()) is not { } gem || !gem.TryGetComponent(out ItemDrop component)) continue;
            itemDrops.Add(component);
        }
        return itemDrops;
    }

    public static List<string> GetSocketedGemSharedNames(this MarketManager.MarketItem marketItem)
    {
        List<string> sharedNames = new();
        foreach(ItemDrop? item in marketItem.GetSockets()) sharedNames.Add(item.m_itemData.m_shared.m_name);
        return sharedNames;
    }
}