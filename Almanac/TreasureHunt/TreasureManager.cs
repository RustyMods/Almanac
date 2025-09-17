using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Almanac.Managers;
using Almanac.Store;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.TreasureHunt;
public class TreasureManager : MonoBehaviour
{
    private const float maxRadius = 9500f;
    private const float minSpawnDistance = 2f;
    private const float maxYDistance = 10f;
    private const int solidHeightMargin = 1000;
    private const float spawnOffset = 5f;
    private static DateTime datetime = DateTime.MaxValue;
    public static Dictionary<string, TreasureData> treasures = new();
    private static readonly Dictionary<string, TreasureData> fileTreasures = new();
    public static TreasureLocation? ActiveTreasureLocation;
    public static readonly AlmanacDir TreasureDir = new (AlmanacPlugin.AlmanacDir.Path, "Treasures");

    public static bool IsActive => ActiveTreasureLocation != null;
    private static readonly CustomSyncedValue<string> SyncedTreasures = new(AlmanacPlugin.ConfigSync, "ServerSynced_Almanac_Treasures", "");
    public static readonly ISerializer serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    public static bool TryGetTreasure(string id, out TreasureData treasure) => treasures.TryGetValue(id, out treasure);
    public static TreasureManager? instance;
    public void Awake()
    {
        instance = this;
        AlmanacPlugin.OnZNetSceneAwake += Initialize;
    }
    public void Initialize()
    {
        InvokeRepeating(nameof(CheckTreasureLocation), 10f, 10f);
    }
    public void OnDestroy()
    {
        instance = null;
    }
    public static bool Exists(string name) => treasures.ContainsKey(name);
    private void CheckTreasureLocation()
    {
        if (!Player.m_localPlayer || !ZNetScene.instance || !Minimap.instance) return;
        if (ActiveTreasureLocation == null) return;
        if (ActiveTreasureLocation.isSpawned) return;
        if (!ActiveTreasureLocation.IsWithin()) return;
        ActiveTreasureLocation.RemovePin();
        if (!ActiveTreasureLocation.Spawn())
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Failed to spawn treasure loot, returning cost");
            return;
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Spawned Treasure loot!");
    }
    public static void Setup()
    {
        AlmanacPlugin.instance.gameObject.AddComponent<TreasureManager>();
        LoadDefaults();
        string[] files = TreasureDir.GetFiles("*.yml", true);
        if (files.Length == 0)
        {
            foreach (TreasureData? treasure in treasures.Values)
            {
                string fileName = treasure.Name + ".yml";
                string data = serializer.Serialize(treasure);
                var path = TreasureDir.WriteFile(fileName, data);
                fileTreasures[path] = treasure;
                treasure.Path = path;
            }
        }
        else
        {
            treasures.Clear();
            foreach (string file in files)
            {
                try
                {
                    TreasureData data = deserializer.Deserialize<TreasureData>(File.ReadAllText(file));
                    treasures[data.Name] = data;
                    fileTreasures[file] = data;
                    data.Path = file;
                }
                catch
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse treasure: " + Path.GetFileName(file));
                }
            }
        }

        SyncedTreasures.ValueChanged += OnServerTreasureChange;
        FileSystemWatcher watcher = new FileSystemWatcher(TreasureDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.IncludeSubdirectories = true;
        watcher.Created += OnCreated;
        watcher.Changed += OnChange;
        watcher.Deleted += OnDeleted;
    }

    private static void OnServerTreasureChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedTreasures.Value)) return;
        try
        {
            Dictionary<string, TreasureData> data = deserializer.Deserialize<Dictionary<string, TreasureData>>(SyncedTreasures.Value);
            ActiveTreasureLocation = null;
            treasures.Clear();
            treasures.AddRange(data);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server treasures");
        }
    }
    private static void OnCreated(object? sender, FileSystemEventArgs e)
    {
        try
        {
            if (!ZNet.instance || !ZNet.instance.IsServer()) return;
            TreasureData data = deserializer.Deserialize<TreasureData>(File.ReadAllText(e.FullPath));
            treasures[data.Name] = data;
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Treasures].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnTreasureTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create treasure: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnChange(object? sender, FileSystemEventArgs e)
    {
        try
        {
            if (!ZNet.instance || !ZNet.instance.IsServer()) return;
            TreasureData data = deserializer.Deserialize<TreasureData>(File.ReadAllText(e.FullPath));
            if (!treasures.TryGetValue(data.Name, out var treasure)) return;
            treasure.CopyFrom(data);
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Treasures].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnTreasureTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change treasure: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnDeleted(object? sender, FileSystemEventArgs e)
    {
        try
        {
            if (!ZNet.instance || !ZNet.instance.IsServer()) return;
            if (!fileTreasures.TryGetValue(e.FullPath, out var treasure)) return;
            treasures.Remove(treasure.Name);
            fileTreasures.Remove(e.FullPath);
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Treasures].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnTreasureTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to delete treasure: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void LoadDefaults()
    {
        TreasureData meadow = new TreasureData();
        meadow.Name = "Meadow Stash";
        meadow.Cost.Add(StoreManager.STORE_TOKEN, 1);
        meadow.Biome = "Meadows";
        meadow.Icon = "map";
        meadow.Lore = "Hidden among the rolling meadows, this stash awaits a clever hunter.”";
        meadow.Loot.Add("Flint", 1, 10);
        meadow.Loot.Add("Wood", 10, 20);
        meadow.Loot.Add("Coins", 5, 20);
        treasures[meadow.Name] = meadow;

        TreasureData blackforest = new TreasureData();
        blackforest.Name = "Black Forest Stash";
        blackforest.Cost.Add(StoreManager.STORE_TOKEN, 1);
        blackforest.Biome = "BlackForest";
        blackforest.Icon = "map";
        blackforest.Lore = "Among twisted trees and darkened paths, the forest guards its treasure well.";
        blackforest.Loot.Add("TinOre", 5,  10);
        blackforest.Loot.Add("CopperOre", 5, 10);
        blackforest.Loot.Add("Coins", 50, 100);
        treasures[blackforest.Name] = blackforest;

        TreasureData swamps = new TreasureData();
        swamps.Name = "Swamps Stash";
        swamps.Cost.Add(StoreManager.STORE_TOKEN, 1);
        swamps.Biome = "Swamp";
        swamps.Icon = "map";
        swamps.Lore = "Where the waters are dark and the ground treacherous, a hidden stash awaits.";
        swamps.Loot.Add("IronScrap", 10, 20);
        swamps.Loot.Add("Ooze", 1, 50);
        swamps.Loot.Add("Bloodbag", 1, 50);
        swamps.Loot.Add("Coins", 50, 100);
        treasures[swamps.Name] = swamps;

        TreasureData mountains = new TreasureData();
        mountains.Name = "Mountains Stash";
        mountains.Cost.Add(StoreManager.STORE_TOKEN, 1);
        mountains.Biome = "Mountain";
        mountains.Icon = "map";
        mountains.Lore = "Only those who brave the jagged cliffs will uncover what the mountains hide.";
        mountains.Loot.Add("SilverOre", 10, 20);
        mountains.Loot.Add("WolfHairBundle", 1, 50);
        mountains.Loot.Add("WolfClaw", 1, 20);
        mountains.Loot.Add("TrophyCultist", 1, 1);
        treasures[mountains.Name] = mountains;
        
        TreasureData plains = new  TreasureData();
        plains.Name = "Plains Stash";
        plains.Cost.Add(StoreManager.STORE_TOKEN, 1);
        plains.Biome = "Plains";
        plains.Icon = "map";
        plains.Lore = "Across the open plains, hidden bounty waits for those who wander with purpose";
        plains.Loot.Add("Barley", 1, 100);
        plains.Loot.Add("Flax", 1, 100);
        plains.Loot.Add("BarleyWine", 1, 10);
        plains.Loot.Add("Coins", 100, 200);
        treasures[plains.Name] = plains;
        
        TreasureData mistlands = new  TreasureData();
        mistlands.Name = "Mistlands Stash";
        mistlands.Cost.Add(StoreManager.STORE_TOKEN, 1);
        mistlands.Biome = "Mistlands";
        mistlands.Icon = "map";
        mistlands.Lore = "Through the swirling mists, hidden treasures wait for those bold enough to seek them.";
        mistlands.Loot.Add("Softtissue", 1, 20);
        mistlands.Loot.Add("ChickenEgg", 1, 1);
        mistlands.Loot.Add("GiantBloodSack", 1, 50);
        mistlands.Loot.Add("YggdrasilWood", 1, 50);
        mistlands.Loot.Add("Sap", 1, 50);
        treasures[mistlands.Name] = mistlands;

        TreasureData ashlands = new TreasureData();
        ashlands.Name = "Ashlands Stash";
        ashlands.Cost.Add(StoreManager.STORE_TOKEN, 10);
        ashlands.Biome = "AshLands";
        ashlands.Icon = "map";
        ashlands.Lore = "The Ashlands burn with secrets; only the fearless can claim their bounty";
        ashlands.Loot.Add("GemstoneRed", 1, 1);
        ashlands.Loot.Add("GemstoneGreen", 1, 1);
        ashlands.Loot.Add("GemstoneBlue", 1, 1);
        ashlands.Loot.Add("FlametalOreNew", 10, 30);
        treasures[ashlands.Name] = ashlands;
    }

    private static void ReturnCost(Player player, TreasureData data)
    {
        if (player.NoCostCheat()) return;
        foreach (var item in data.Cost.Items)
        {
            if (item.isToken)
            {
                player.AddTokens(item.Amount);
            }
            else
            {
                player.GetInventory()
                    .AddItem(item.item?.m_itemData?.m_shared.m_name, item.Amount, 1, 0, 0L, "");
            }
        }
    }
    public static void CancelTreasure()
    {
        if (ActiveTreasureLocation == null) return;
        if (ActiveTreasureLocation.spawnedObject != null)
        {
            ZNetView? znv = ActiveTreasureLocation.spawnedObject.GetComponent<ZNetView>();
            znv.ClaimOwnership();
            znv.Destroy();
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.BountyCanceled);
        if (Configs.ReturnTreasureCostWhenCancel)
        {
            ReturnCost(Player.m_localPlayer, ActiveTreasureLocation.data);
        }
        ActiveTreasureLocation.RemovePin();
        ActiveTreasureLocation = null;
    }
    public static bool AcceptTreasure(TreasureData data)
    {
        if (datetime != DateTime.MaxValue)
        {
            DateTime lastTreasure = datetime + TimeSpan.FromMinutes(Configs.TreasureCooldown);
            if (lastTreasure > DateTime.Now)
            {
                int difference = (lastTreasure - DateTime.Now).Minutes;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{Keys.TreasureAvailableIn}: {difference} min");
                return false;
            }
        }
        
        TreasureLocation treasureLocation = new TreasureLocation(data);
        if (!treasureLocation.FindSpawnLocation())
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center,"Failed to find treasure spawn location");
            return false;
        }
        data.Purchase(Player.m_localPlayer);
        treasureLocation.AddPin();
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{Keys.SearchFor} {treasureLocation.data.Name}");
        datetime = DateTime.Now;
        ActiveTreasureLocation = treasureLocation;
        AlmanacPlugin.AlmanacLogger.LogDebug("Successfully added treasure hunt: " + treasureLocation.data.Name);
        AlmanacPlugin.AlmanacLogger.LogDebug("Location: " + treasureLocation.position.x + " " + treasureLocation.position.z);
        return true;
    }
    public class TreasureLocation
    {
        public readonly TreasureData data;
        public Vector3 position;
        public bool isSpawned;
        private Minimap.PinData? pin;
        public GameObject? spawnedObject;
        public TreasureLocation(TreasureData data)
        {
            this.data = data;
        }
        public bool IsWithin()
        {
            if (!Player.m_localPlayer) return false;
            float num1 = position.x - Player.m_localPlayer.transform.position.x;
            float num2 = position.z - Player.m_localPlayer.transform.position.z;

            return Math.Sqrt(num1 * num1 + num2 * num2) <= 100f;
        }
        public void AddPin()
        {
            RemovePin();
            pin = Minimap.instance.AddPin(position, Minimap.PinType.Boss, data.Name, false, false);
            pin.m_icon = data.icon ?? SpriteManager.GetSprite(SpriteManager.IconOption.Map);
        }
        public void RemovePin()
        {
            if (pin == null || !Minimap.instance) return;
            Minimap.instance.RemovePin(pin);
        }
        public bool Spawn()
        {
            Vector3 vector3 = GetRandomVectorWithin(position, 10f);
            if (data.biome is Heightmap.Biome.AshLands)
            {
                if (ZoneSystem.instance.IsLava(vector3))
                {
                    for (int index = 0; index < 1000; ++index)
                    {
                        Vector3 possible = GetRandomVectorWithin(vector3, 10f * index + 1);
                        if (ZoneSystem.instance.IsLava(possible))
                        {
                            continue;
                        }
                        vector3 = position;
                        break;
                    }
                }
            }
            
            if (WorldGenerator.instance.GetBiome(vector3) == Heightmap.Biome.Ocean)
            {
                vector3.y = ZoneSystem.instance.m_waterLevel - 0.3f;
            }
            else
            {
                ZoneSystem.instance.GetSolidHeight(vector3, out float height);
                if (height >= 0.0 && Mathf.Abs(height - position.y) <= maxYDistance &&
                    Vector3.Distance(vector3, position) >= minSpawnDistance)
                {
                    vector3.y = height + spawnOffset;
                }
                else
                {
                    vector3.y = Player.m_localPlayer.transform.position.y + spawnOffset;
                }
            }
            position = vector3;

            ZPackage pkg = new ZPackage();
            pkg.Write(data.Loot.Count);
            foreach (LootInfo? loot in data.Loot)
            {
                pkg.Write(loot.Item);
                pkg.Write(loot.Min);
                pkg.Write(loot.Max);
                pkg.Write((double)loot.Weight);
            }
            ZDO? zdo = ZDOMan.instance.CreateNewZDO(position, "barrell".GetStableHashCode());
            zdo.Persistent = false;
            zdo.Type = ZDO.ObjectType.Default;
            zdo.Distant = false;
            zdo.SetPrefab("barrell".GetStableHashCode());
            zdo.SetOwner(ZDOMan.GetSessionID());
            zdo.Set(ZDOVars.s_drops, pkg.GetBase64());
            zdo.Set(ZDOVars.s_creator, Player.m_localPlayer.GetPlayerID());
            GameObject go = ZNetScene.instance.CreateObject(zdo);
            go.AddComponent<TreasureHunt>();
            go.AddComponent<HoverText>().m_text = data.Name;;
            go.AddComponent<Beacon>().m_range = 50f;
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
                rb.velocity = Vector3.down * 0.1f;
            }            
            spawnedObject = go;
            isSpawned = true;
            return true;
        }
        private static Vector3 GetRandomVectorWithin(Vector3 point, float margin)
        {
            Vector2 vector2 = UnityEngine.Random.insideUnitCircle * margin;
            return point + new Vector3(vector2.x, 0.0f, vector2.y);
        }
        public bool FindSpawnLocation()
        {
            return RandomLocationFinder.FindSpawnLocation(data.biome, out position);
        }
    }
    [Serializable]
    public class TreasureData
    {
        public string Name = string.Empty;
        public string Lore = string.Empty;
        public string Icon = string.Empty;
        public string Biome = string.Empty;
        public StoreManager.StoreCost Cost = new();
        public List<LootInfo> Loot = new();
        public TreasureData(){}
        public void CopyFrom(TreasureData data)
        {
            Name = data.Name;
            Lore = data.Lore;
            Icon = data.Icon;
            Biome = data.Biome;
            Cost = data.Cost;
            Loot = data.Loot;
        }

        [YamlIgnore] public string Path = string.Empty;
        [YamlIgnore] public Heightmap.Biome biome => Enum.TryParse(Biome, true, out Heightmap.Biome land) ? land : Heightmap.Biome.None;
        [YamlIgnore] public Sprite? icon => SpriteManager.GetSprite(Icon);
        public List<AlmanacPanel.InfoView.Icons.DropInfo> ToDropInfo()
        {
            List<AlmanacPanel.InfoView.Icons.DropInfo> info = new();
            foreach (LootInfo loot in Loot)
            {
                if (loot.ToDropInfo() is not { } dropInfo) continue;
                info.Add(dropInfo);
            }
            return info;
        }

        private static Entries.EntryBuilder builder = new();
        public List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            builder.Add(Lore + "\n", "lore");
            builder.Add(Keys.Biome, biome);
            builder.Add(Keys.Loot);
            return builder.ToList();
        }

        public bool CanPurchase(Player player)
        {
            if (player.NoCostCheat()) return true;
            foreach (var item in Cost.Items)
            {
                if (item.isToken)
                {
                    if (player.GetTokens() < item.Amount) return false;
                }
                else
                {
                    if (player.GetInventory().CountItems(item.item?.m_itemData.m_shared.m_name ?? "$item_coins") < item.Amount) return false;
                }
            }

            return true;
        }

        public void Purchase(Player player)
        {
            if (player.NoCostCheat()) return;
            foreach (var item in Cost.Items)
            {
                if (item.isToken)
                {
                    player.RemoveTokens(item.Amount);
                }
                else
                {
                    player.GetInventory().RemoveItem(item.item?.m_itemData.m_shared.m_name ?? "$item_coins", item.Amount);
                }
            }
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element item)
        {
            panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(Name);
            panel.description.SetIcon(icon);
            panel.description.Interactable(true);
            if (AlmanacPanel.isLocalAdminOrHostAndNoCost)
            {
                AlmanacPanel.InfoView.EditButton edit = panel.description.view.CreateEditButton();
                edit.SetLabel("Edit");
                edit.OnClick(() =>
                {
                    var form = new FormPanel.TreasureForm();
                    form.SetTopic("Edit Treasure");
                    form.SetButtonText("Confirm Edit");
                    form.SetDescription("Edit Treasure Ledger");
                    form.inEditMode = true;
                    form.overridePath = Path;
                    panel.formBuilder.Setup(form);
                    form.nameField.input?.Set(Name);
                    form.loreField.input?.Set(Lore);
                    form.iconField.input?.Set(Icon);
                    form.biomeField.input?.Set(Biome);
                    form.costField.input?.Set(Cost.ToString());
                    form.lootField.input?.Set(string.Join(":", Loot.Select(l => l.ToString())));
                    form.HasChanged = false;
                });
            }
            ToEntries().Build(panel.description.view);
            List<AlmanacPanel.InfoView.Icons.DropInfo> drops = ToDropInfo();
            if (drops.Count > 0)
            {
                if (drops.Count > 4)
                {
                    IEnumerable<List<AlmanacPanel.InfoView.Icons.DropInfo>> batches = ToDropInfo().Batch(4);
                    foreach (List<AlmanacPanel.InfoView.Icons.DropInfo>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else
                {
                    panel.description.view.CreateIcons().SetIcons(drops.ToArray());
                }
            }
            panel.description.view.Resize();
            bool isActive = IsActive;
            bool canPurchase = CanPurchase(Player.m_localPlayer);
            panel.description.SetButtonText(isActive ? Keys.CancelHunt : Keys.StartTreasureHunt);
            panel.description.Interactable(canPurchase || isActive);
            panel.OnMainButton = () =>
            {
                if (isActive)
                {
                    CancelTreasure();
                    panel.description.SetButtonText(Keys.StartTreasureHunt);
                }
                else
                {
                    if (AcceptTreasure(this))
                    {
                        panel.description.SetButtonText(Keys.CancelHunt);
                    }
                }
                isActive = IsActive;
                canPurchase = CanPurchase(Player.m_localPlayer);
            };
            panel.description.requirements.Set(Cost);
            panel.description.requirements.SetLevel(string.Empty);
            panel.OnUpdate = _ =>  panel.description.requirements.Update();
        }
    }

    [Serializable]
    public class LootInfo
    {
        public string Item = string.Empty;
        public int Min;
        public int Max;
        public float Weight;
        public LootInfo(){}
        public LootInfo(string item, int min, int max, float weight)
        {
            Item = item;
            Min = min;
            Max = max;
            Weight = weight;
        }

        public override string ToString() => $"{Item}, {Min}, {Max}, {Weight}";
        [YamlIgnore] public GameObject? prefab => ObjectDB.instance.GetItemPrefab(Item);
        [YamlIgnore] public bool isValid => prefab is not null;
        public DropTable.DropData ToDropTableData()
        {
            DropTable.DropData data = new()
            {
                m_item = prefab,
                m_stackMin = Min,
                m_stackMax = Max,
                m_weight = Weight
            };
            return data;
        }
        public AlmanacPanel.InfoView.Icons.DropInfo? ToDropInfo()
        {
            if (prefab?.GetComponent<ItemDrop>().m_itemData is not { } itemData) return null;
            return new AlmanacPanel.InfoView.Icons.DropInfo(itemData, Weight, Min, Max);;
        }
    }
}

public static class TreasureHelpers
{
    public static void Add(this List<TreasureManager.LootInfo> list, string item, int min, int max, float weight = 1f)
    {
        list.Add(new TreasureManager.LootInfo(item, min, max, weight));
    }
}

public static class TreasureReadMeBuilder
{
    // private static readonly string FilePath = AlmanacPaths.FolderPath + Path.DirectorySeparatorChar + "Treasure_README.md";
    private static readonly string[] Prefix = new[]
    {
        "# Treasure Hunts",
        "Almanac lets you define custom treasure hunts using `.yml` files in the TreasureHunt folder.",
        "These treasures sync between server and client, and are dynamically reloaded when edited.",
        "",
        "Treasure hunts create interactive map pins that spawn loot containers when you reach their location.",
        "Players can purchase treasure hunts using tokens from the Almanac store system.",
        "",
        "## Available Biomes",
        "```"
    };

    private static readonly string[] MiddleSection = new[]
    {
        "```",
        "",
        "## Treasure File Structure",
        "Each treasure hunt is defined as a YAML file with the following properties:",
        "",
        "### Core Properties",
        "- `Name`: Display name for the treasure hunt (e.g., \"Meadow Stash\").",
        "- `Lore`: Descriptive flavor text shown to players.",
        "- `Icon`: The icon name from game assets (defaults to \"map\" if not specified).",
        "- `Biome`: Target biome where the treasure will spawn (see list above).",
        "",
        "### Cost System",
        "- `Cost`: Dictionary of required tokens/items to purchase this treasure hunt.",
        "  - Use `StoreManager.STORE_TOKEN` as the key for Almanac tokens.",
        "  - Example: `Cost: { \"almanac_token\": 10 }` for 10 tokens.",
        "",
        "### Loot Configuration",
        "- `Loot`: List of items that can be found in the treasure container.",
        "  - `Item`: Prefab name of the item (e.g., \"Coins\", \"SilverOre\").",
        "  - `Min`: Minimum stack size.",
        "  - `Max`: Maximum stack size.",
        "  - `Weight`: Drop chance weight (higher = more likely).",
        "",
        "## Example Treasure File",
        "```yaml",
        "Name: \"Mountain Stash\"",
        "Lore: \"Only those who brave the jagged cliffs will uncover what the mountains hide.\"",
        "Icon: \"map\"",
        "Biome: \"Mountain\"",
        "Cost:",
        "  almanac_token: 1",
        "Loot:",
        "  - Item: \"SilverOre\"",
        "    Min: 10",
        "    Max: 20",
        "    Weight: 1.0",
        "  - Item: \"WolfClaw\"",
        "    Min: 1",
        "    Max: 20",
        "    Weight: 1.0",
        "  - Item: \"TrophyCultist\"",
        "    Min: 1",
        "    Max: 1",
        "    Weight: 1.0",
        "```"
    };

    private static readonly string[] Postfix = new[]
    {
        "",
        "## Gameplay Mechanics",
        "",
        "### Purchase Process",
        "1. Players purchase treasure hunts from the Almanac store using tokens.",
        "2. System finds a random spawn location within the specified biome.",
        "3. A map pin is added showing the approximate treasure location.",
        "",
        "### Treasure Spawning",
        "- When players get within 100 meters of the pin, a barrel spawns nearby.",
        "- The barrel contains randomized loot based on the configured drop table.",
        "- Each loot item has weighted chances and stack size ranges.",
        "",
        "### Cooldown System",
        "- Configurable cooldown between treasure hunts (default settings apply).",
        "- Prevents spam purchasing and maintains game balance.",
        "",
        "### File Management",
        "- Hot-reloading: Edit files while server is running.",
        "- Server sync: Changes automatically distribute to all clients.",
        "- Default generation: Creates example files on first run.",
        "- Error handling: Invalid files are logged and skipped.",
        "",
        "## Tips for Content Creators",
        "",
        "### Balancing",
        "- Higher biome difficulty = higher token cost and better rewards.",
        "- Use weight system to create rare vs common drops.",
        "- Consider travel time and danger when setting costs.",
        "",
        "### Lore Integration",
        "- Write immersive lore that fits the biome atmosphere.",
        "- Reference environmental hazards and local creatures.",
        "- Create narrative connections between different treasure hunts.",
        "",
        "### Item Selection",
        "- Include biome-appropriate materials and resources.",
        "- Mix common materials with rare trophies or special items.",
        "- Consider what players need at different progression stages.",
        "",
        "### Testing",
        "- Verify all item prefab names exist in the game.",
        "- Test spawn locations in each biome for accessibility.",
        "- Confirm treasure costs align with server economy."
    };

    public static void Write()
    {
        if (AlmanacPlugin.AlmanacDir.FileExists("Treasure_README.md")) return;
        IOrderedEnumerable<string> biomes = Enum.GetNames(typeof(Heightmap.Biome))
            .Where(biome => biome != "None" && biome != "All")
            .OrderBy(biome => biome);
        
        List<string> lines = new();
        lines.AddRange(Prefix);
        lines.AddRange(biomes);
        lines.AddRange(MiddleSection);
        lines.AddRange(Postfix);
        AlmanacPlugin.AlmanacDir.WriteAllLines("Treasure_README.md", lines);

    }
}