using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.Store;
using Almanac.UI;
using Almanac.Utilities;
using ServerSync;
using YamlDotNet.Serialization;

namespace Almanac.Marketplace;

public static class MarketManager
{
    private static readonly ISerializer serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static string? MarketFileName;
    private static string? RevenueFileName;
    public static readonly AlmanacDir MarketplaceDir = new (AlmanacPlugin.AlmanacDir.Path, "Marketplace");

    private static readonly CustomSyncedValue<string> SyncedMarket = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Market", "");
    private static readonly CustomSyncedValue<string> SyncedRevenue = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Revenue", "");

    private static readonly Dictionary<string, Market> marketItems = new();
    private static readonly Dictionary<string, int> revenues = new();
    public static void Setup()
    {
        AlmanacPlugin.OnZNetAwake += Initialize;
        SyncedMarket.ValueChanged += OnServerMarketChange;
        SyncedRevenue.ValueChanged += OnServerRevenueChange;
        AlmanacPlugin.OnZNetSave += Save;
    }
    private static void Save()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer() || MarketFileName == null || RevenueFileName == null) return;
        string marketData = serializer.Serialize(marketItems);
        byte[] compressedMarketData = CompressAndEncode(marketData);
        MarketplaceDir.WriteAllBytes(MarketFileName, compressedMarketData);
        string revenueData = serializer.Serialize(revenues);
        byte[] compressedRevenueData = CompressAndEncode(revenueData);
        MarketplaceDir.WriteAllBytes(RevenueFileName, compressedRevenueData);
    }
    private static void Read()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()  || MarketFileName == null || RevenueFileName == null) return;
        if (File.Exists(MarketFileName))
        {
            try
            {
                byte[] compressedData = File.ReadAllBytes(MarketFileName);
                string data = DecompressAndDecode(compressedData);
            
                Dictionary<string, Market> deserializedData = deserializer.Deserialize<Dictionary<string, Market>>(data);
                marketItems.Clear();
                marketItems.AddRange(deserializedData);
            }
            catch
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server marketplace: " + Path.GetFileName(MarketFileName));
            }
        }

        if (File.Exists(RevenueFileName))
        {
            try
            {
                byte[] compressedData = File.ReadAllBytes(RevenueFileName);
                string data = DecompressAndDecode(compressedData);
                Dictionary<string, int> deserializedData = deserializer.Deserialize<Dictionary<string, int>>(data);
                revenues.Clear();
                revenues.AddRange(deserializedData);
            }
            catch
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server revenue: " + Path.GetFileName(RevenueFileName));
            }
        }
    }
    private static byte[] CompressAndEncode(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);

        using var output = new MemoryStream();
        using var gzip = new GZipStream(output, CompressionMode.Compress);
        gzip.Write(data, 0, data.Length);
        gzip.Close();
        return output.ToArray();
    }
    private static string DecompressAndDecode(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
    private static void OnServerMarketChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedMarket.Value)) return;
        try
        {
            Dictionary<string, Market> data = deserializer.Deserialize<Dictionary<string, Market>>(SyncedMarket.Value);
            marketItems.Clear();
            marketItems.AddRange(data);
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Store].IsSelected ?? false) AlmanacPanel.instance.OnStoreTab();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server marketplace");
        }
    }
    private static void Initialize()
    {
        MarketFileName = ZNet.instance.GetWorldName() + ".Marketplace.dat";
        RevenueFileName = ZNet.instance.GetWorldName() + ".Revenue.dat";
        ZRoutedRpc.instance.Register<string>(nameof(RPC_AddMarketItem), RPC_AddMarketItem);
        ZRoutedRpc.instance.Register<string>(nameof(RPC_RemoveMarketItem), RPC_RemoveMarketItem);
        ZRoutedRpc.instance.Register<string>(nameof(RPC_RemoveRevenue), RPC_RemoveRevenue);
        Read();
        UpdateServerMarketplace();
    }
    private static void UpdateServerMarketplace()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string data = serializer.Serialize(marketItems);
        SyncedMarket.Value = data;
    }
    private static void OnServerRevenueChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedRevenue.Value)) return;
        try
        {
            Dictionary<string, int> data = deserializer.Deserialize<Dictionary<string, int>>(SyncedRevenue.Value);
            revenues.Clear();
            revenues.AddRange(data);
            if (!AlmanacPanel.IsVisible()) return;
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Store].IsSelected ?? false) AlmanacPanel.instance.OnStoreTab();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server marketplace");
        }
    }

    private static void UpdateServerRevenue()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string data = serializer.Serialize(revenues);
        SyncedRevenue.Value = data;
    }
    public static List<MarketItem> GetMarketItems()
    {
        List<MarketItem> list = new List<MarketItem>();
        foreach (Market? market in marketItems.Values)
        {
            if (market.isEquipable) list.AddRange(market.items.Values);
            else
            {
                if (market.GetCheapest() is not { } cheapest) continue;
                list.Add(cheapest);
            }
        }
        return list;
    }
    public static int GetRevenue(Player player) => revenues.TryGetValue(player.GetPlayerName(), out int revenue) ? revenue : 0;

    public static void CollectRevenue(Player player)
    {
        if (!revenues.TryGetValue(player.GetPlayerName(), out int revenue)) return;
        player.AddTokens(revenue);
        RemoveRevenue(player.GetPlayerName());
    }
    public static bool HasRevenue(Player player) => revenues.ContainsKey(player.GetPlayerName());

    private static void RPC_RemoveMarketItem(long sender, string pkg)
    {
        try
        {
            MarketItem item = deserializer.Deserialize<MarketItem>(pkg);
            RemoveMarketItem(item);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to remove market item");
        }
    }

    private static Market GetOrSet(this Dictionary<string, Market> dict, string key)
    {
        if (!dict.TryGetValue(key, out Market? market)) market = new Market(key);
        return market;
    }
    public static void RPC_AddMarketItem(long sender, string pkg)
    {
        try
        {
            MarketItem item = deserializer.Deserialize<MarketItem>(pkg);
            AddMarketItem(item);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to add market item");
        }
    }
    public static void RPC_RemoveRevenue(long sender, string playerName) => RemoveRevenue(playerName);

    private static void RemoveRevenue(string playerName)
    {
        if (ZNet.instance.IsServer())
        {
            revenues.Remove(playerName);
            UpdateServerRevenue();
            if (AlmanacPanel.IsVisible()) AlmanacPanel.instance?.OnStoreTab();
        }
        else
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC_RemoveRevenue), playerName);
        }
    }
    private static void AddRevenue(string playerName, int amount)
    {
        revenues.IncrementOrSet(playerName, amount);
        UpdateServerRevenue();
    }
    private static void AddMarketItem(MarketItem marketItem)
    {
        if (!ItemHelper.TryGetItemBySharedName(marketItem.itemData.m_shared.m_name, out ItemDrop component)) return;
        Market market = marketItems.GetOrSet(component.name);
        market.Add(marketItem);
        UpdateServerMarketplace();
        if (!AlmanacPanel.IsVisible()) return;
        if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Store].IsSelected ?? false) AlmanacPanel.instance.OnStoreTab();
    }
    private static void RemoveMarketItem(MarketItem marketItem)
    {
        marketItem.market?.Remove(marketItem);
        AddRevenue(marketItem.PostedBy, marketItem.TokenCost);
        UpdateServerMarketplace();
        UpdateServerRevenue();
        if (!AlmanacPanel.IsVisible()) return;
        if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Store].IsSelected ?? false) AlmanacPanel.instance.OnStoreTab();
    }
    public static bool AddMarketItem(ItemDrop.ItemData item, int cost, int stack, string playerName)
    {
        if (!ZNet.instance) return false;
        if (!ItemHelper.TryGetItemBySharedName(item.m_shared.m_name, out ItemDrop component)) return false;
        
        MarketItem marketItem = new MarketItem
        {
            ItemID = component.name,
            Stack = stack,
            Quality = item.m_quality,
            Variant = item.m_variant,
            Durability = item.m_durability,
            CrafterID = item.m_crafterID,
            CrafterName = item.m_crafterName,
            CustomData = item.m_customData,
            TokenCost = cost,
            DatePosted = DateTime.UtcNow,
            PostedBy = playerName,
            GUID = Guid.NewGuid().ToString(),
        };

        if (ZNet.instance.IsServer())
        {
            AddMarketItem(marketItem);
        }
        else
        {
            string pkg = serializer.Serialize(marketItem);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC_AddMarketItem), pkg);
        }
        return true;
    }
    [Serializable]
    public class Market
    {
        public string ItemID = string.Empty;
        public Dictionary<string, MarketItem> items = new();
        [YamlIgnore] public bool isEquipable => item?.m_itemData.IsEquipable() ?? false;
        [YamlIgnore] public ItemDrop? item => ObjectDB.instance?.GetItemPrefab(ItemID).GetComponent<ItemDrop>();
        public Market(string itemID)
        {
            ItemID = itemID;
            marketItems[itemID] = this; 
        }
        public Market(){}
        public MarketItem? GetCheapest()
        {
            MarketItem? cheapest = null;
            foreach (MarketItem marketItem in items.Values)
            {
                if (cheapest == null || marketItem.GetCostPerUnit() < cheapest.GetCostPerUnit())
                {
                    cheapest = marketItem;
                }
            }
            return cheapest;
        }

        public void Add(MarketItem marketItem) => items[marketItem.GUID] = marketItem;
        public void Remove(MarketItem marketItem)
        {
            items.Remove(marketItem.GUID);
            if (items.Count == 0)
            {
                marketItems.Remove(ItemID);
            }
        }
    }

    [Serializable]
    public class MarketItem
    {
        private static Entries.EntryBuilder builder = new();
        
        public string ItemID = string.Empty;
        public int Stack;
        public float Durability;
        public int Quality;
        public int Variant;
        public long CrafterID;
        public string CrafterName = string.Empty;
        public Dictionary<string, string> CustomData = new();

        public int TokenCost;
        public DateTime DatePosted;
        public string PostedBy = string.Empty;
        public string GUID = string.Empty;
        public MarketItem() {}
        [YamlIgnore] public Market? market => marketItems.TryGetValue(ItemID, out Market? Market) ? Market : null;
        [YamlIgnore] public ItemDrop.ItemData itemData => ObjectDB.instance.GetItemPrefab(ItemID).GetComponent<ItemDrop>().m_itemData;
        public bool HasRequirements(Player player) => player.GetTokens() >= TokenCost;
        public float GetCostPerUnit() => TokenCost / (float)Stack;
        public ItemDrop.ItemData GetPreview()
        {
            ItemDrop.ItemData item = ObjectDB.instance.GetItemPrefab(ItemID).GetComponent<ItemDrop>().m_itemData.Clone();
            item.m_stack = Stack;
            item.m_durability = Durability;
            item.m_quality = Quality;
            item.m_variant = Variant;
            item.m_crafterID = CrafterID;
            item.m_crafterName = CrafterName;
            item.m_customData.AddRange(CustomData);
            return item;
        }
        public List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            builder.m_showAll = true;
            builder.Add(Helpers.ReplacePositionTags(GetPreview().GetTooltip()) + "\n\n", "lore");
            if (this.HasSockets())
            {
                var jewels = this.GetSocketedGemSharedNames();
                builder.Add($"Sockets ({jewels.Count})");
                foreach (var jewel in jewels) builder.Add(Keys.Name, jewel);
                builder.Add(Keys.Data);
            }
            builder.Add(Keys.CostPerUnit, GetCostPerUnit().ToString(CultureInfo.InvariantCulture));
            builder.Add(Keys.PostedBy, PostedBy);
            builder.Add(Keys.DatePosted, DatePosted.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            return builder.ToList();
        }
        public bool Purchase(Player player)
        {
            if (!player.GetInventory().HaveEmptySlot()) return false;
            player.RemoveTokens(TokenCost);
            ItemDrop.ItemData item = player.GetInventory().AddItem(ItemID, Stack, Quality, Variant, CrafterID, CrafterName);
            item.m_durability = Durability;
            item.m_customData.AddRange(CustomData);
            if (ZNet.instance.IsServer())
            {
                RemoveMarketItem(this);
                AlmanacPanel.instance?.OnStoreTab();
            }
            else
            {
                string data = serializer.Serialize(this);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC_RemoveMarketItem), data);
            }
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{Keys.Purchased} {itemData.m_shared.m_name}");
            return true;
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element item)
        {
            panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(itemData.m_shared.m_name + $" x{Stack}");
            panel.description.SetIcon(itemData.GetIcon());
            bool hasReqs = HasRequirements(Player.m_localPlayer);
            panel.description.Interactable(hasReqs);
            panel.description.SetButtonText(Keys.Purchase);
            ToEntries().Build(panel.description.view);
            panel.description.view.Resize();
            panel.OnMainButton = () => Purchase(Player.m_localPlayer);
            panel.OnUpdate = _ => panel.description.requirements.Update();
            panel.description.requirements.SetTokens(TokenCost);
            panel.description.requirements.SetLevel(Quality);
        }
    }
}