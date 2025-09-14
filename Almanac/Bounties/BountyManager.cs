﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Data;
using Almanac.Managers;
using Almanac.Store;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Bounties;

public class BountyManager : MonoBehaviour
{
    private static readonly List<Effect> Effects = new();
    private static readonly Effect PreSpawnEffects = new("vfx_prespawn", "sfx_prespawn");
    private static readonly Effect SpawnEffects = new("vfx_spawn", "sfx_spawn");
    public static readonly ISerializer serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static readonly CustomSyncedValue<string> SyncedBounties = new(AlmanacPlugin.ConfigSync, "ServerSynced_Almanac_Bounties", "");
    public static readonly AlmanacDir BountyDir = new (AlmanacPlugin.AlmanacDir.Path, "Bounties");
    public static readonly Dictionary<string, BountyData> bounties = new();
    private static Dictionary<string, BountyData> fileBounties = new();
    public static BountyLocation? ActiveBountyLocation;

    private static BountyManager? instance;
    private const float maxRadius = 9500f;
    private const float minSpawnDistance = 2f;
    private const float maxYDistance = 10f;
    private const int solidHeightMargin = 1000;
    private const float spawnOffset = 5f;
    private static DateTime datetime = DateTime.MaxValue;
    
    public static bool TryGetBountyData(string id, out BountyData data) => bounties.TryGetValue(id, out data); 
    public void Awake()
    {
        instance = this;
    }
    public void Initialize()
    {
        if (IsInvoking(nameof(CheckBountyLocation))) return;
        InvokeRepeating(nameof(CheckBountyLocation), 10f, 10f);
    }
    public void OnDestroy()
    {
        instance = null;
    }
    private void CheckBountyLocation()
    {
        if (!Player.m_localPlayer || !ZNetScene.instance || !Minimap.instance) return;
        if (ActiveBountyLocation == null) return;
        if (ActiveBountyLocation.isSpawned) return;
        if (!ActiveBountyLocation.IsWithin()) return;
        ActiveBountyLocation.RemovePin();
        if (ActiveBountyLocation.Spawn()) return;
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.FailedToSpawnBounty);
    }
    public void SpawnBounty()
    {
        if (ActiveBountyLocation == null) return;
        var hash = ActiveBountyLocation.data.Creature.GetStableHashCode();
        ZDO? zdo = ZDOMan.instance.CreateNewZDO(ActiveBountyLocation.position, hash);
        zdo.Persistent = false;
        zdo.Type = ZDO.ObjectType.Default;
        zdo.Distant = false;
        zdo.SetPrefab(hash);
        zdo.SetOwner(ZDOMan.GetSessionID());
        if (ActiveBountyLocation.data.Level > 1) zdo.Set(ZDOVars.s_level, ActiveBountyLocation.data.Level);
        zdo.Set(ZDOVars.s_tamedName, ActiveBountyLocation.data.GetNameOverride());
        if (ActiveBountyLocation.data.Health > 0)
        {
            zdo.Set(BountyVars.BountyHealth, ActiveBountyLocation.data.Health);
            zdo.Set(ZDOVars.s_maxHealth, ActiveBountyLocation.data.Health);
            zdo.Set(ZDOVars.s_health, ActiveBountyLocation.data.Health);
        }
        zdo.Set(BountyVars.BountyID, ActiveBountyLocation.data.UniqueID);
        zdo.Set(BountyVars.DamageModifier, ActiveBountyLocation.data.DamageMultiplier);
        zdo.Set(ZDOVars.s_creator, Player.m_localPlayer.GetPlayerID());
        GameObject go = ZNetScene.instance.CreateObject(zdo);
        go.AddComponent<Bounty>();
        SpawnEffects.Create(go.transform.position, Quaternion.identity);
    }
    public static bool IsOnCooldown()
    {
        int cooldownDuration = Configs.BountyCooldown;
        if (cooldownDuration <= 0f) return false;
        if (datetime == DateTime.MaxValue) return false;
        DateTime lastBounty = datetime + TimeSpan.FromMinutes(cooldownDuration);
        if (lastBounty <= DateTime.Now) return false;
        int difference = (lastBounty - DateTime.Now).Minutes;
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"{Keys.BountyAvailableIn} {difference} {Keys.Minutes}");
        return true;
    }
    public static bool AcceptBounty(BountyData data)
    {
        if (IsOnCooldown()) return false;
        BountyLocation bountyLocation = new BountyLocation(data);
        if (!bountyLocation.FindSpawnLocation()) return false;
        Purchase(Player.m_localPlayer, data);
        bountyLocation.AddPin();
        ActiveBountyLocation = bountyLocation;
        if (string.IsNullOrEmpty(bountyLocation.data.Name))
        {
            bountyLocation.data.Name = NameGenerator.GenerateName(bountyLocation.data.character?.m_name ?? bountyLocation.data.Creature);
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.Hunt + " " + bountyLocation.data.Name);
        datetime = DateTime.Now;
        return true;
    }
    public static bool CanPurchase(Player player, BountyData data)
    {
        if (player.NoCostCheat()) return true;
        foreach (StoreManager.StoreCost.Cost? item in data.Cost.Items)
        {
            if (item.isToken)
            {
                if (player.GetTokens() >= item.Amount) continue;
                return false;
            }
            if (player.GetInventory().CountItems(item.item?.m_itemData.m_shared.m_name ?? "$item_coins") >= item.Amount) continue;
            return false;
        }

        return true;
    }
    public static void Purchase(Player player, BountyData data)
    {
        if (player.NoCostCheat()) return;
        foreach (StoreManager.StoreCost.Cost? item in data.Cost.Items)
        {
            if (item.isToken)
            {
                player.RemoveTokens(item.Amount);
            }
            else
            {
                player.GetInventory().RemoveItem(item.item?.m_itemData.m_shared.m_name, item.Amount);
            }
        }
    }
    private static void ReturnCost(Player player, BountyData data)
    {
        if (player.NoCostCheat()) return;
        foreach (StoreManager.StoreCost.Cost? item in data.Cost.Items)
        {
            if (item.isToken)
            {
                player.AddTokens(item.Amount);
            }
            else
            {
                player.GetInventory().AddItem(item.item?.m_itemData.m_shared.m_name ?? "Coins", item.Amount, 1, 0, 0L, string.Empty);
            }
        }
    }
    public static void CancelBounty()
    {
        if (ActiveBountyLocation == null) return;
        ActiveBountyLocation.data.completed = false;
        if (ActiveBountyLocation.pin != null) Minimap.instance.RemovePin(ActiveBountyLocation.pin);
        if (Configs.ReturnBountyCostWhenCancel)
        {
            ReturnCost(Player.m_localPlayer, ActiveBountyLocation.data);
        }
        ActiveBountyLocation = null;
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.BountyCanceled);
    }
    public static void Setup()
    {
        AlmanacPlugin.instance.gameObject.AddComponent<BountyManager>();
        AlmanacPlugin.OnZNetAwake += UpdateServerBounties;
        AlmanacPlugin.OnZNetSceneAwake += OnZNetSceneAwake;
        LoadDefaults();
        string[] files = BountyDir.GetFiles("*.yml", true);
        if (files.Length <= 0)
        {
            foreach (BountyData? bounty in bounties.Values)
            {
                string data = serializer.Serialize(bounty);
                string fileName = bounty.UniqueID + ".yml";
                string path = BountyDir.WriteFile(fileName, data);
                fileBounties[path] = bounty;
            }
        }
        else
        {
            bounties.Clear();
            foreach (string file in files)
            {
                string data = File.ReadAllText(file);
                BountyData bounty = deserializer.Deserialize<BountyData>(data);
                bounties[bounty.UniqueID] = bounty;
                fileBounties[file] = bounty;
            }
        }

        SyncedBounties.ValueChanged += OnServerBountyChanged;
        FileSystemWatcher watcher = new FileSystemWatcher(BountyDir.Path, "*.yml");
        watcher.EnableRaisingEvents = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.IncludeSubdirectories = true;
        watcher.Created += OnCreated;
        watcher.Changed += OnChange;
        watcher.Deleted += OnDeleted;
    }
    private static void OnServerBountyChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedBounties.Value)) return;
        try
        {
            Dictionary<string, BountyData> data =
                deserializer.Deserialize<Dictionary<string, BountyData>>(SyncedBounties.Value);
            ActiveBountyLocation = null;
            fileBounties = data;
            bounties.Clear();
            foreach (BountyData bounty in fileBounties.Values)
            {
                bounties[bounty.UniqueID] = bounty;
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server bounties");
        }
    }
    private static void OnChange(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            BountyData data = deserializer.Deserialize<BountyData>(File.ReadAllText(e.FullPath));
            if (!fileBounties.TryGetValue(e.FullPath, out BountyData? bounty)) return;
            bounty.CopyFrom(data);
            UpdateServerBounties();
            if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Bounties].IsSelected ?? false)
            {
                AlmanacPanel.instance.OnBountyTab();
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change bounty: " + Path.GetFileName(e.FullPath));
        }
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        BountyData data = deserializer.Deserialize<BountyData>(File.ReadAllText(e.FullPath));
        bounties[data.UniqueID] = data;
        fileBounties[e.FullPath] = data;
        UpdateServerBounties();
        if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Bounties].IsSelected ?? false)
        {
            AlmanacPanel.instance.OnBountyTab();
        }
    }
    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!fileBounties.TryGetValue(e.FullPath, out BountyData? bounty)) return;
        bounties.Remove(bounty.UniqueID);
        fileBounties.Remove(e.FullPath);
        UpdateServerBounties();
        if (AlmanacPanel.instance?.Tabs[AlmanacPanel.Tab.TabOption.Bounties].IsSelected ?? false)
        {
            AlmanacPanel.instance.OnBountyTab();
        }
    }
    private static void OnZNetSceneAwake()
    {
        foreach (Effect? effect in Effects) effect.Init();
        instance?.Initialize();
    }
    private static void UpdateServerBounties()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string data = serializer.Serialize(fileBounties);
        SyncedBounties.Value = data;
    }
    public static bool Exists(string name) => bounties.ContainsKey(name);
    private static void LoadDefaults()
    {
        BountyData boar = new BountyData();
        boar.UniqueID = "Boar.001";
        boar.Creature = "Boar";
        boar.Lore = "The meadows have been wicked to the boars, hunted endlessly by ruthless vikings - it breeds true beasts";
        boar.Icon = "TrophyBoar";
        boar.Biome = "Meadows";
        boar.Health = 1000f;
        boar.DamageMultiplier = 1.5f;
        boar.AlmanacTokenReward = 1;
        boar.Level = 3;
        boar.Cost.Add("Coins", 10);
        bounties[boar.UniqueID] = boar;
        
        BountyData neck = new BountyData();
        neck.UniqueID = "Neck.001";
        neck.Creature = "Neck";
        neck.Lore = "The meadows may seem calm, but when the rains fall, the Neck emerges to claim the unwary.";
        neck.Icon = "TrophyNeck";
        neck.Biome = "Meadows";
        neck.Health = 1000f;
        neck.DamageMultiplier = 1.5f;
        neck.AlmanacTokenReward = 1;
        neck.Level = 3;
        neck.Cost.Add("Coins", 10);
        bounties[neck.UniqueID] = neck;

        BountyData troll = new BountyData();
        troll.UniqueID = "Troll.001";
        troll.Creature = "Troll";
        troll.Lore = "Lumbering through the Black Forest, the troll’s steps shake the earth as it smashes all in its path.";
        troll.Icon = "TrophyFrostTroll";
        troll.Biome = "BlackForest";
        troll.Health = 1200f;
        troll.DamageMultiplier = 1.5f;
        troll.AlmanacTokenReward = 5;
        troll.Level = 3;
        troll.Cost.Add("Coins", 10);
        bounties[troll.UniqueID] = troll;

        BountyData serpent = new BountyData();
        serpent.UniqueID = "Serpent.001";
        serpent.Creature = "Serpent";
        serpent.Lore = "Sailors whisper of a serpent that drags ships beneath the waves, leaving only foam and silence.";
        serpent.Icon = "TrophySerpent";
        serpent.Biome = "Ocean";
        serpent.Health = 1000f;
        serpent.DamageMultiplier = 1.5f;
        serpent.AlmanacTokenReward = 5;
        serpent.Level = 3;
        serpent.Cost.Add("Coins", 10);
        bounties[serpent.UniqueID] = serpent;

        BountyData abomination = new BountyData();
        abomination.UniqueID = "Abomination.001";
        abomination.Creature = "Abomination";
        abomination.Lore = "From the mire it rises, a tangle of roots and hate, the swamp itself given monstrous form.";
        abomination.Icon = "TrophyAbomination";
        abomination.Biome = "Swamp";
        abomination.Health = 1600f;
        abomination.DamageMultiplier = 1.5f;
        abomination.AlmanacTokenReward = 5;
        abomination.Level = 3;
        abomination.Cost.Add("Coins", 10);
        bounties[abomination.UniqueID] = abomination;
        
        BountyData wraith = new BountyData();
        wraith.UniqueID = "Wraith.001";
        wraith.Creature = "Wraith";
        wraith.Lore = "When the mists thicken and the air grows cold, the Wraith glides forth to claim the living.";
        wraith.Icon = "TrophyWraith";
        wraith.Biome = "Swamp";
        wraith.Health = 1000f;
        wraith.DamageMultiplier = 1.5f;
        wraith.AlmanacTokenReward = 5;
        wraith.Level = 3;
        wraith.Cost.Add("Coins", 10);
        bounties[wraith.UniqueID] = wraith;
        
        BountyData lox = new BountyData();
        lox.UniqueID = "Lox.001";
        lox.Creature = "Lox";
        lox.Lore = "The ground trembles beneath its hooves, for the Lox knows no predator but death itself.";
        lox.Icon = "TrophyLox";
        lox.Biome = "Plains";
        lox.Health = 2000f;
        lox.DamageMultiplier = 1.5f;
        lox.AlmanacTokenReward = 5;
        lox.Level = 3;
        lox.Cost.Add("Coins", 10);
        bounties[lox.UniqueID] = lox;
        
        BountyData seekerSoldier = new BountyData();
        seekerSoldier.UniqueID = "SeekerSoldier.001";
        seekerSoldier.Creature = "SeekerBrute";
        seekerSoldier.Lore = "Forged in the mists, this armored brute marches with the strength of many men and the hunger of a swarm.";
        seekerSoldier.Icon = "TrophySeekerBrute";
        seekerSoldier.Biome = "Mistlands";
        seekerSoldier.Health = 3000f;
        seekerSoldier.DamageMultiplier = 1.5f;
        seekerSoldier.AlmanacTokenReward = 10;
        seekerSoldier.Level = 3;
        seekerSoldier.Cost.Add("Coins", 10);
        bounties[seekerSoldier.UniqueID] = seekerSoldier;
        
        BountyData fallenValkyrie = new BountyData();
        fallenValkyrie.UniqueID = "FallenValkyrie.001";
        fallenValkyrie.Creature = "FallenValkyrie";
        fallenValkyrie.Lore = "Once a chooser of the slain, now cursed in flame, the Fallen Valkyrie haunts the Ashlands with broken wings.";
        fallenValkyrie.Icon = "TrophyFallenValkyrie";
        fallenValkyrie.Biome = "AshLands";
        fallenValkyrie.Health = 3000f;
        fallenValkyrie.DamageMultiplier = 1.5f;
        fallenValkyrie.AlmanacTokenReward = 20;
        fallenValkyrie.Level = 3;
        fallenValkyrie.Cost.Add("Coins", 10);
        bounties[fallenValkyrie.UniqueID] = fallenValkyrie;
    }
    public class BountyLocation
    {
        public readonly BountyData data;
        public Vector3 position;
        public bool isSpawned;
        public Minimap.PinData? pin;
        public BountyLocation(BountyData data)
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

            PreSpawnEffects.Create(vector3, Quaternion.identity);

            position = vector3;
            instance?.Invoke(nameof(SpawnBounty), 10f);
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
    public class BountyData
    {
        public string UniqueID = string.Empty;
        public string Creature = string.Empty;
        public string Name = string.Empty;
        public string Icon = string.Empty;
        public string Biome = string.Empty;
        public float Health;
        public int Level = 1;
        public float DamageMultiplier;
        public int AlmanacTokenReward;
        public string Lore = string.Empty;
        public StoreManager.StoreCost Cost = new();
        public BountyData(){}
        public void CopyFrom(BountyData data)
        {
            UniqueID = data.UniqueID;
            Creature = data.Creature;
            Name = data.Name;
            Icon = data.Icon;
            Health = data.Health;
            DamageMultiplier = data.DamageMultiplier;
            AlmanacTokenReward = data.AlmanacTokenReward;
            Cost = data.Cost;
            _prefab = null;
        }
        private static Entries.EntryBuilder builder = new();
        public List<Entries.Entry> ToEntries()
        {
            builder.Clear();
            builder.Add(Lore + "\n", "lore");
            builder.Add(Keys.Biome, biome);
            builder.Add(Keys.Health, Health);
            builder.Add(Keys.Level, Level);
            builder.Add(Keys.DamageModifier, DamageModString());
            builder.Add(Keys.Reward);
            builder.Add(Keys.AlmanacToken, AlmanacTokenReward);
            if (monsterAI is null)
            {
                builder.Add("<color=red>Invalid, missing MonsterAI component!</color>", "lore");
            }
            return builder.ToList();
        }
        public bool HasRequirements(Player player) => player.NoCostCheat() || PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, character?.m_name ?? string.Empty) > 0;
        [NonSerialized, YamlIgnore] public bool completed;
        [YamlIgnore] public Heightmap.Biome biome => Enum.TryParse(Biome, true, out Heightmap.Biome land) ? land : Heightmap.Biome.None;
        [YamlIgnore] private GameObject? _prefab;
        [YamlIgnore] public GameObject? Prefab
        {
            get
            {
                if (_prefab != null) return _prefab;
                _prefab = ZNetScene.instance.GetPrefab(Creature);
                return _prefab;
            }
        }
        [YamlIgnore] public Character? character => Prefab?.GetComponent<Character>();
        [YamlIgnore] public MonsterAI? monsterAI => Prefab?.GetComponent<MonsterAI>();
        [YamlIgnore] public Sprite? icon => SpriteManager.GetSprite(Icon);
        public string DamageModString()
        {
            return DamageMultiplier < 1f
                ? $"-{(1f - DamageMultiplier) * 100f:0.0}%"
                : $"+{(DamageMultiplier - 1f) * 100f:0.0}%";
        }
        public string GetNameOverride()
        {
            if (!ZNetScene.instance) return string.Empty;
            if (!string.IsNullOrEmpty(Name)) return Name;
            Name = NameGenerator.GenerateName(character?.m_name ?? Creature);
            return Name;
        }

        public void ReturnCost(Player player)
        {
            foreach (StoreManager.StoreCost.Cost? cost in Cost.Items)
            {
                if (cost.isToken) player.AddTokens(cost.Amount);
                else player.GetInventory().AddItem(cost.PrefabName, cost.Amount, 1, 0, 0L, string.Empty);
            }
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ElementView.Element item)
        {
            panel.elementView.SetSelected(item);
            panel.description.Reset();
            panel.description.SetName(character?.m_name ?? "<color=red>Invalid</color>");
            panel.description.SetIcon(icon);
            ToEntries().Build(panel.description.view);
            panel.description.view.Resize();
            bool isActive = ActiveBountyLocation != null;
            bool isCompleted = ActiveBountyLocation?.data.completed ?? false;
            bool canPurchase = CanPurchase(Player.m_localPlayer, this);
            panel.description.Interactable(canPurchase || isActive);
            panel.description.SetButtonText(isActive ? isCompleted ? Keys.Collect : Keys.CancelBounty : Keys.StartBounty);
            panel.OnMainButton = () =>
            {
                if (ActiveBountyLocation is {} activeBountyLocation)
                {
                    if (isCompleted)
                    {
                        Player.m_localPlayer.AddTokens(activeBountyLocation.data.AlmanacTokenReward);
                        activeBountyLocation.data.completed = false;
                        ActiveBountyLocation = null;
                    }
                    else
                    {
                        CancelBounty();
                    }
                    panel.description.SetButtonText(Keys.StartBounty);
                }
                else
                {
                    if (AcceptBounty(this))
                    {
                        panel.description.SetButtonText(Keys.CancelBounty);
                    }
                }
            };
            panel.description.requirements.Set(Cost);
            panel.description.requirements.SetLevel(Level);
            panel.OnUpdate = _ => panel.description.requirements.Update();
        }
    }

    private class Effect
    {
        private readonly List<string> m_effectNames;
        private readonly EffectList m_effectList = new();
        public Effect(params string[] effectNames)
        {
            m_effectNames = effectNames.ToList();
            Effects.Add(this);
        }
        public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform? baseParent = null, float scale = 1f, int variant = -1)
            => m_effectList.Create(basePos, baseRot, baseParent, scale, variant);
        public void Init()
        {
            if (!ZNetScene.instance) return;
            List<EffectList.EffectData> data = new();
            foreach (string? effectName in m_effectNames)
            {
                if (ZNetScene.instance.GetPrefab(effectName) is not { } prefab) continue;
                data.Add(new EffectList.EffectData(){m_prefab = prefab});
            }

            m_effectList.m_effectPrefabs = data.ToArray();
        }
    }
}

public static class BountyReadMeBuilder
{
    private static readonly string[] Prefix =
    {
        "# Almanac Bounties",
        "The Almanac Bounty system lets players purchase bounty contracts to hunt special creatures.",
        "",
        "Bounties are defined in `.yml` files inside the **Bounties** folder.",
        "Admins can add, remove, or edit bounty entries at runtime — changes will sync to all clients.",
        "",
        "Each bounty entry can define:",
        "- **UniqueID**: Unique identifier for the bounty (e.g., `Troll.001`).",
        "- **Creature**: The prefab name of the target creature (e.g., `Troll`, `Serpent`).",
        "- **Name**: (Optional) Custom name override; if empty, a generated name will be used.",
        "- **Icon**: Trophy sprite or icon for the bounty.",
        "- **Biome**: The biome where the bounty will spawn (`Meadows`, `Swamp`, `AshLands`, etc.).",
        "- **Health**: Override maximum health value of the bounty.",
        "- **Level**: Creature level (scales difficulty).",
        "- **DamageMultiplier**: Multiplier applied to the bounty’s attacks (e.g., `1.5`).",
        "- **AlmanacTokenReward**: Tokens given upon completing the bounty.",
        "- **Lore**: A short description displayed in the Almanac panel.",
        "- **Cost**: Item or token requirements to purchase the bounty.",
        "### Notes",
        "- Costs can be `AlmanacToken` or regular items (e.g., `Coins`).",
        "- Bounties require players to kill the target directly — indirect deaths won’t count.",
        "- If the bounty despawns, escapes, or is killed by another player, the cost is returned.",
        "- Bounties are subject to a configurable cooldown (default in minutes).",
        "",
        "### Tips",
        "- You can reload or edit `.yml` bounty files while the server is running; changes sync automatically.",
        "- Each bounty spawns a pin on the map when accepted.",
        "- Default bounties include **Boar, Neck, Troll, Serpent, Abomination, Wraith, Lox, Seeker Brute, Fallen Valkyrie**.",
        "- Use `Lore` to tell a short story or flavor text for each hunt."
    };
    
    public static void Write()
    {
        if (AlmanacPlugin.AlmanacDir.FileExists("Bounty_README.md")) return;
        List<string> lines = new List<string>();
        lines.AddRange(Prefix);

        lines.Add("# Example Entry");
        lines.Add("```yml");
        lines.Add("UniqueID: Troll.001");
        lines.Add("Creature: Troll");
        lines.Add("Name: Forest Stalker");
        lines.Add("Icon: TrophyFrostTroll");
        lines.Add("Biome: BlackForest");
        lines.Add("Health: 1200");
        lines.Add("Level: 3");
        lines.Add("DamageMultiplier: 1.5");
        lines.Add("AlmanacTokenReward: 5");
        lines.Add("Lore: \"Lumbering through the Black Forest, the troll’s steps shake the earth as it smashes all in its path.\"");
        lines.Add("Cost:");
        lines.Add("  Coins: 10");
        lines.Add("```");
        lines.Add("");
        AlmanacPlugin.AlmanacDir.WriteAllLines("Bounty_README.md", lines);
    }
}
