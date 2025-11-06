using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.Managers;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Store;

public static class StoreManager
{
    private static readonly Dictionary<string, StoreItem> items = new();
    private static readonly Dictionary<string, StoreItem> fileItems = new();
    public static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static readonly CustomSyncedValue<string> SyncedStore = new (AlmanacPlugin.ConfigSync, "ServerSynced_Almanac_Store", "");
    public const string STORE_TOKEN = "AlmanacToken";
    public static readonly AlmanacDir StoreDir = new (AlmanacPlugin.AlmanacDir.Path, "Store");

    public static bool Exists(string name) => items.ContainsKey(name);
    public static List<StoreItem> GetStoreItems() => items.Values.ToList();
    public static void Setup()
    {
        AlmanacPlugin.OnPlayerProfileSavePlayerDataPrefix += player => player.SaveTokens();
        AlmanacPlugin.OnZNetAwake += UpdateServerStore;
        LoadDefaults();
        string[] files = StoreDir.GetFiles("*.yml", true);
        if (files.Length <= 0)
        {
            foreach (StoreItem? item in items.Values)
            {
                string data = serializer.Serialize(item);
                string fileName = item.Name + ".yml";
                var path = StoreDir.WriteFile(fileName, data);
                fileItems[path] = item;
                item.Path = path;
            }
        }
        else
        {
            items.Clear();
            foreach (string file in files)
            {
                string data = File.ReadAllText(file);
                StoreItem item = deserializer.Deserialize<StoreItem>(data);
                items[item.Name] = item;
                fileItems[file] = item;
                item.Path = file;
            }
        }

        SyncedStore.ValueChanged += OnServerStoreChanged;
        FileSystemWatcher watcher = new FileSystemWatcher(StoreDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.IncludeSubdirectories = true;
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnDeleted;
    }
    private static void OnServerStoreChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedStore.Value)) return;
        try
        {
            Dictionary<string, StoreItem> data =
                deserializer.Deserialize<Dictionary<string, StoreItem>>(SyncedStore.Value);
            items.Clear();
            items.AddRange(data);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server store");
        }
    }
    private static void UpdateServerStore()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedStore.Value = serializer.Serialize(items);
    }
    private static void ReloadPage()
    {
        if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Store].IsSelected ?? false)
        {
            AlmanacPanel.instance.OnStoreTab();
        }
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            StoreItem data = deserializer.Deserialize<StoreItem>(File.ReadAllText(e.FullPath));
            items[data.Name] = data;
            UpdateServerStore();
            ReloadPage();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed create store item: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            StoreItem data = deserializer.Deserialize<StoreItem>(File.ReadAllText(e.FullPath));
            items[data.Name] = data;
            UpdateServerStore();
            ReloadPage();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change store item: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!fileItems.TryGetValue(e.FullPath, out StoreItem? item)) return;
        items.Remove(item.Name);
        fileItems.Remove(e.FullPath);
        UpdateServerStore();
        ReloadPage();
    }
    private static void LoadDefaults()
    {
        StoreItem rested = new StoreItem();
        rested.Name = "Rested";
        rested.Lore = "Adds rested bonus";
        rested.StatusEffect = new StoreItem.StatusEffectData("Rested", 1000f);
        rested.Icon = "Rested";
        rested.Cost.Set(1);
        items[rested.Name] = rested;
        
        StoreItem MinorHealth = new StoreItem();
        MinorHealth.Name = "Minor Health";
        MinorHealth.Lore = "Increases health";
        MinorHealth.StatusEffect = new StoreItem.StatusEffectData("CE_MinorHealth");
        MinorHealth.Icon = "mushroom";
        MinorHealth.Cost.Set(3);
        items[MinorHealth.Name] = MinorHealth;
        
        StoreItem MinorArmor = new StoreItem();
        MinorArmor.Name = "Minor Armor";
        MinorArmor.Lore = "Increases armor";
        MinorArmor.StatusEffect = new StoreItem.StatusEffectData("CE_MinorArmor");
        MinorArmor.Icon = "shield";
        MinorArmor.Cost.Set(3);
        items[MinorArmor.Name] = MinorArmor;
        
        StoreItem MinorStamina = new StoreItem();
        MinorStamina.Name = "Minor Stamina";
        MinorStamina.Lore = "Increases stamina";
        MinorStamina.StatusEffect = new StoreItem.StatusEffectData("CE_MinorStamina");
        MinorStamina.Icon = "bottle_blue";
        MinorStamina.Cost.Set(3);
        items[MinorStamina.Name] = MinorStamina;
        
        StoreItem MinorEitr = new  StoreItem();
        MinorEitr.Name = "Minor Eitr";
        MinorEitr.Lore = "Increases eitr";
        MinorEitr.StatusEffect = new StoreItem.StatusEffectData("CE_MinorEitr");
        MinorEitr.Icon = "necklace";
        MinorEitr.Cost.Set(3);
        items[MinorEitr.Name] = MinorEitr;
        
        StoreItem LifeSteal = new  StoreItem();
        LifeSteal.Name = "Life Steal";
        LifeSteal.Lore = "Leeches health when attacking";
        LifeSteal.StatusEffect = new StoreItem.StatusEffectData("CE_LifeSteal");
        LifeSteal.Icon = "log_stack";
        LifeSteal.Cost.Set(5);
        
        StoreItem BluntReduction = new  StoreItem();
        BluntReduction.Name = "Blunt Resister";
        BluntReduction.Lore = "Increases blunt resistance";
        BluntReduction.StatusEffect = new StoreItem.StatusEffectData("CE_BluntReduction");
        BluntReduction.Icon = "shield";
        BluntReduction.Cost.Set(1);
        items[BluntReduction.Name] = BluntReduction;

        StoreItem WoodStack = new StoreItem();
        WoodStack.Name = "Wood Stack";
        WoodStack.Lore = "Just a stack of wood";
        WoodStack.Icon = "Wood";
        WoodStack.Cost.Set(1);
        WoodStack.Items.Add("Wood", 50);
        items[WoodStack.Name] = WoodStack;

        StoreItem StoneStack = new StoreItem();
        StoneStack.Name = "Stone Stack";
        StoneStack.Lore = "Just a pile of stone";
        StoneStack.Icon = "Stone";
        StoneStack.Cost.Set(1);
        StoneStack.Items.Add("Stone", 50);
        items[StoneStack.Name] = StoneStack;

        StoreItem BronzeNails = new StoreItem();
        BronzeNails.Name = "Bronze Nails";
        BronzeNails.Lore = "Just some nails";
        BronzeNails.Icon = "BronzeNails";
        BronzeNails.Cost.Set(3);
        BronzeNails.Items.Add("BronzeNails", 20);
        BronzeNails.RequiredDefeated = "gd_king";
        items[BronzeNails.Name] = BronzeNails;
        
        StoreItem lolite = new StoreItem();
        lolite.Name = "Lolite";
        lolite.Lore = "Light is reflected sharply off this gem";
        lolite.Icon = "GemstoneBlue";
        lolite.Cost.Set(100);
        lolite.Items.Add("GemstoneBlue", 1);
        lolite.RequiredDefeated = "Charred_Melee_Dyrnwyn";
        items[lolite.Name] = lolite;
        
        StoreItem tar = new StoreItem();
        tar.Name = "Tar";
        tar.Lore = "Just some tar";
        tar.Icon = "Tar";
        tar.Cost.Set(10);
        tar.Items.Add("Tar", 50);
        tar.RequiredDefeated = "BlobTar";
        items[tar.Name] = tar;

        StoreItem silverNecklace = new StoreItem();
        silverNecklace.Name = "Silver Necklace";
        silverNecklace.Lore = "Just a necklace";
        silverNecklace.Icon = "SilverNecklace";
        silverNecklace.Cost.Set(1);
        silverNecklace.Items.Add("SilverNecklace", 1);
        silverNecklace.RequiredDefeated = "Dragon";
        items[silverNecklace.Name] = silverNecklace;
        
        StoreItem StoneGolemTrophy = new StoreItem();
        StoneGolemTrophy.Name = "Stone Golem Trophy";
        StoneGolemTrophy.Lore = "A crystalline rock formation is an impressive decoration";
        StoneGolemTrophy.Icon = "TrophySGolem";
        StoneGolemTrophy.Cost.Set(1);
        StoneGolemTrophy.Items.Add("TrophySGolem", 1);
        StoneGolemTrophy.RequiredDefeated = "StoneGolem";
        items[StoneGolemTrophy.Name] = StoneGolemTrophy;
        
        StoreItem Crystals = new StoreItem();
        Crystals.Name = "Crystals";
        Crystals.Lore = "Just some crystals";
        Crystals.Icon = "Crystal";
        Crystals.Cost.Set(10);
        Crystals.Items.Add("Crystal", 50);
        Crystals.RequiredDefeated = "StoneGolem";
        items[Crystals.Name] = Crystals;

        StoreItem Rock = new StoreItem();
        Rock.Name = "Rock";
        Rock.Lore = "Just some rock";
        Rock.Icon = "StoneRock";
        Rock.Cost.Set(100);
        Rock.Items.Add("StoneRock", 1);
        items[Rock.Name] = Rock;
        
        StoreItem QueenBee = new StoreItem();
        QueenBee.Name = "Queen Bee";
        QueenBee.Lore = "Just a queen bee";
        QueenBee.Icon = "QueenBee";
        QueenBee.Cost.Set(1);
        QueenBee.Items.Add("QueenBee", 1);
        QueenBee.RequiredDefeated = "gd_king";
        items[QueenBee.Name] = QueenBee;

        ConversionItem one = new ConversionItem(1, SpriteManager.IconOption.GoldCoins);
        ConversionItem ten = new  ConversionItem(10, SpriteManager.IconOption.SilverCoins);
        ConversionItem hundred = new ConversionItem(100, SpriteManager.IconOption.SilverBar);
    }
    public static readonly List<ConversionItem> conversions = new();
    public class ConversionItem
    {
        private static readonly Entries.EntryBuilder builder = new();
        public static ItemDrop item =>
            ObjectDB.instance.GetItemPrefab(Configs.ConversionItem)?.GetComponent<ItemDrop>() 
            ?? ObjectDB.instance.GetItemPrefab("Coins").GetComponent<ItemDrop>();
        
        public int Amount => TokenAmount * Configs.ConversionRate;
        private readonly int TokenAmount;
        private readonly SpriteManager.IconOption Icon;
        public Sprite? icon => SpriteManager.GetSprite(Icon);
        public string name => $"{Keys.AlmanacToken} x{TokenAmount}";
        public string description => $"{Keys.Convert} {item.m_itemData.m_shared.m_name} x{Amount}";

        private bool HasRequirements(Player player)
        {
            return player.GetInventory().CountItems(item.m_itemData.m_shared.m_name) >= Amount;
        }

        private void Purchase(Player player)
        {
            if (!HasRequirements(player)) return;
            player.GetInventory().RemoveItem(item.m_itemData.m_shared.m_name, Amount);
            player.AddTokens(TokenAmount);
        }
        public ConversionItem(int amount, SpriteManager.IconOption icon)
        {
            Icon = icon;
            TokenAmount = amount;
            conversions.Add(this);
        }

        private List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            builder.Add(Keys.ConversionRate, Configs.ConversionRate);
            builder.Add($"{item.m_itemData.m_shared.m_name} x{Amount}", $"{Keys.AlmanacToken} x{TokenAmount}");
            return builder.ToList();
        }

        public void OnClick(AlmanacPanel panel)
        {
            panel.description.Reset();
            panel.description.SetName(name);
            panel.description.SetIcon(icon);
            panel.description.Interactable(true);
            ToEntries().Build(panel.description.view);
            panel.description.view.Resize();
            panel.description.requirements.Set(Amount, item);
            panel.description.requirements.SetLevel(string.Empty);
            panel.description.SetButtonText(Keys.Convert);
            bool hasReqs = HasRequirements(Player.m_localPlayer);
            panel.description.Interactable(hasReqs);
            panel.OnMainButton = () => Purchase(Player.m_localPlayer);
            panel.OnUpdate =  _ =>  panel.description.requirements.Update();
        }
    }

    [Serializable]
    public class StoreItem
    {
        private static Entries.EntryBuilder builder = new();
        
        public string Name = string.Empty;
        public StoreCost Cost = new();
        public string Icon = string.Empty;
        public StatusEffectData? StatusEffect;
        public List<ItemInfo> Items = new();
        public string Lore = string.Empty;
        public string RequiredDefeated = string.Empty;
        public string RequiredAchievement = string.Empty;
        public StoreItem(){}
        [YamlIgnore] public Sprite? sprite => SpriteManager.GetSprite(Icon);
        [YamlIgnore] public string Path = string.Empty;

        public bool HasRequirements(Player player) =>
             player.NoCostCheat() 
             || (KnowsAllItems(player) && HasRequiredKey(out string _) && HasAchievement(player));

        public bool KnowsAllItems(Player player) =>
            Items.All(item => player.IsKnownMaterial(item.item?.m_itemData.m_shared.m_name));
        public bool HasAchievement(Player player)
        {
            if (string.IsNullOrEmpty(RequiredAchievement)) return true;
            if (!AchievementManager.TryGetAchievement(RequiredAchievement, out var achievement)) return true;
            return achievement.IsCompleted(player);
        }
        public bool HasRequiredKey(out string sharedName)
        {
            sharedName = string.Empty;
            if (string.IsNullOrEmpty(RequiredDefeated)) return true;
            if (!CritterHelper.namedCritters.TryGetValue(RequiredDefeated, out CritterHelper.CritterInfo critter)) return true;
            sharedName = critter.character.m_name;
            return PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, sharedName) > 0;
        }
        public List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            if (!string.IsNullOrEmpty(Lore)) builder.Add(Lore + "\n", "lore");
            if (!string.IsNullOrEmpty(RequiredDefeated))
            {
                HasRequiredKey(out string sharedName);
                builder.Add(Keys.RequiredDefeated, sharedName);
            }

            if (!string.IsNullOrEmpty(RequiredAchievement) && AchievementManager.TryGetAchievement(RequiredAchievement, out var achievement))
            {
                builder.Add(Keys.RequiredAchievement, achievement.Name);
            }
            if (StatusEffect != null)
            {
                builder.Add(Keys.Duration, global::StatusEffect.GetTimeString(StatusEffect.GetDuration(StatusEffect.effect?.m_ttl ?? StatusEffect.Duration)));
                builder.Add(StatusEffect.effect?.m_name ?? "<color=red>Invalid Status Effect</color>");
                builder.Add((StatusEffect.effect?.GetTooltipString() ?? string.Empty) + "\n", "lore");
            }
            return builder.ToList();
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element item)
        {
            panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(Name);
            panel.description.SetIcon(sprite);
            bool canPurchase = Cost.CanPurchase(Player.m_localPlayer);
            panel.description.Interactable(canPurchase);
            if (AlmanacPanel.isLocalAdminOrHostAndNoCost)
            {
                AlmanacPanel.InfoView.EditButton edit = panel.description.view.CreateEditButton();
                edit.SetLabel("Edit");
                edit.OnClick(() =>
                {
                    var form = new FormPanel.StoreForm();
                    form.SetTopic("Edit Store Item");
                    form.SetButtonText("Confirm Edit");
                    form.SetDescription("Edit Store Item");
                    form.inEditMode = true;
                    form.overridePath = Path;
                    panel.formBuilder.Setup(form);
                    form.nameField.input?.Set(Name);
                    form.loreField.input?.Set(Lore);
                    form.iconField.input?.Set(Icon);
                    if (StatusEffect != null) form.statusEffectField.input?.Set($"{StatusEffect.ID}, {StatusEffect.Duration}");
                    form.costField.input?.Set(Cost.ToString());
                    form.keyField.input?.Set(RequiredDefeated);
                    form.achievementField.input?.Set(RequiredAchievement);
                    form.itemsField.input?.Set(string.Join(":", Items.Select(itemInfo => itemInfo.ToString()).ToList()));
                    form.HasChanged = false;
                });
            }
            ToEntries().Build(panel.description.view);
            if (Items.Count > 0)
            {
                panel.description.view.CreateTitle().SetTitle(Keys.Items);
                if (Items.Count > 4)
                {
                    IEnumerable<List<ItemInfo>> batches = Items.Batch(4);
                    foreach (List<ItemInfo> batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else
                {
                    panel.description.view.CreateIcons().SetIcons(Items.ToArray());
                }
            }
            panel.description.requirements.Set(Cost);
            panel.description.SetButtonText(Keys.Purchase);
            panel.OnMainButton = () =>
            {
                Player.m_localPlayer.Purchase(this);
                canPurchase = Cost.CanPurchase(Player.m_localPlayer);
                panel.description.Interactable(canPurchase);
            };
            panel.OnUpdate = _ =>  panel.description.requirements.Update();
            panel.description.view.Resize();
            panel.description.requirements.SetLevel(string.Empty);
        }
        
        [Serializable]
        public class StatusEffectData
        {
            public string ID = string.Empty;
            public float Duration;
            [YamlIgnore] public StatusEffect? effect => ObjectDB.instance?.GetStatusEffect(ID.GetStableHashCode());
            public StatusEffectData(){}
            public StatusEffectData(string name, float duration = 0f)
            {
                ID = name;
                Duration = duration;
            }
            public float GetDuration(float defaultValue) => Duration <= 0f ? defaultValue : Duration;

            public bool Add(Player player)
            {
                if (player.GetSEMan().GetStatusEffect(ID.GetStableHashCode()) is { } status)
                {
                    status.m_ttl = GetDuration(status.m_ttl);
                    status.m_time = 0f;
                    return true;
                }
                if (player.GetSEMan().AddStatusEffect(ID.GetStableHashCode(), true) is not { } se) return false;
                se.m_ttl = GetDuration(se.m_ttl);
                return true;
            }
        }

        [Serializable]
        public class ItemInfo
        {
            public string PrefabName = string.Empty;
            public int Amount;
            public int Quality;
            public int Variant;
            [YamlIgnore] public ItemDrop? item => ObjectDB.instance?.GetItemPrefab(PrefabName)?.GetComponent<ItemDrop>();
            public ItemInfo(){}
            public ItemInfo(string name, int amount, int quality = 1, int variant = 0)
            {
                PrefabName = name;
                Amount = amount;
                Quality = quality;
                Variant = variant;
            }

            public override string ToString() => $"{PrefabName}, {Amount}, {Quality}, {Variant}";
        }
    }   

    [Serializable]
    public class StoreCost
    {
        public List<Cost> Items = new();
        public StoreCost(){}
        public void Add(Cost cost) => Items.Add(cost);
        public void Add(string name, int amount) => Add(new Cost(name, amount));
        public void Set(int amount) => Add(new Cost(STORE_TOKEN, amount));
        public bool CanPurchase(Player player)
        {
            foreach (Cost cost in Items)
            {
                if (cost.isToken)
                {
                    if (player.GetTokens() < cost.Amount) return false;
                }
                else
                {
                    var count = player.GetInventory().CountItems(cost.item?.m_itemData.m_shared.m_name);
                    if (count < cost.Amount) return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Join(":", Items.Select(item => $"{item.PrefabName}, {item.Amount}").ToList());
        }

        [Serializable]
        public class Cost
        {
            public string PrefabName = string.Empty;
            public int Amount;
            public Cost(){}
            [YamlIgnore] public bool isToken => PrefabName == STORE_TOKEN;
            [YamlIgnore] public Sprite? tokenIcon => SpriteManager.GetSprite("coins_silver");
            public Cost(string item, int amount)
            {
                PrefabName = item;
                Amount = amount;
            }
            [YamlIgnore] public ItemDrop? item => ObjectDB.instance.GetItemPrefab(PrefabName)?.GetComponent<ItemDrop>() ?? null;
        }
    }
}
public static class StoreHelpers
{
    public static void Add(this List<StoreManager.StoreItem.ItemInfo> list, string name, int amount, int quality = 1, int variant = 0) => list.Add(new StoreManager.StoreItem.ItemInfo(name, amount, quality, variant));
    public static int GetTokens(this Player player) =>
        player.m_customData.TryGetValue(StoreManager.STORE_TOKEN, out string? token)
            ? int.TryParse(token, out int amount) ? amount : 0
            : 0;

    public static void SaveTokens(this Player player) =>
        player.m_customData[StoreManager.STORE_TOKEN] = Mathf.Max(0, player.GetTokens()).ToString();

    public static void AddTokens(this Player player, int amount, bool message = true)
    {
        int newAmount = player.GetTokens() + amount;
        player.m_customData[StoreManager.STORE_TOKEN] = newAmount.ToString();
        AlmanacPanel.instance?.currency.SetAmount(newAmount);
        if (message) player.Message(MessageHud.MessageType.Center, $"{Keys.Added} {Keys.AlmanacToken} x{amount}");
    }

    public static void RemoveTokens(this Player player, int amount)
    {
        int newAmount = Mathf.Max(0, player.GetTokens() - amount);
        player.m_customData[StoreManager.STORE_TOKEN] = newAmount.ToString();
        AlmanacPanel.instance?.currency.SetAmount(newAmount);
        player.Message(MessageHud.MessageType.Center, $"{Keys.Removed} {Keys.AlmanacToken} x{amount}");
    }
    
    public static void ClearTokens(this Player player) => player.m_customData.Remove(StoreManager.STORE_TOKEN);

    public static int GetTokensByteCount(this Player player)
    {
        if (!player.m_customData.TryGetValue(StoreManager.STORE_TOKEN, out var data)) return 0;
        int size = Encoding.UTF8.GetByteCount(data);
        return size;
    }

    public static void Purchase(this Player player, StoreManager.StoreItem storeItem)
    {
        storeItem.StatusEffect?.Add(player);
        foreach (StoreManager.StoreItem.ItemInfo? item in storeItem.Items)
        {
            if (item.item is not { } itemDrop) continue;
            var variant = item.Variant > itemDrop.m_itemData.m_shared.m_variants ? 0 : item.Variant;
            player.GetInventory().AddItem(itemDrop.name, item.Amount, item.Quality, variant, 0L, string.Empty);
        }

        foreach (StoreManager.StoreCost.Cost? cost in storeItem.Cost.Items)
        {
            if (cost.isToken) player.RemoveTokens(cost.Amount);
            else player.GetInventory().RemoveItem(cost.item?.m_itemData.m_shared.m_name, cost.Amount);
        }
        player.Message(MessageHud.MessageType.Center, $"{Keys.Purchased} {storeItem.Name}");
    }
}
