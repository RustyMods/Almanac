using System;
using System.Collections.Generic;
using System.Text;
using Almanac.Managers;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.Data;

public static class PlayerExtensions
{
    private static readonly ISerializer serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    public static PlayerInfo.PlayerRecords? GetRecords(this Player player)
    {
        try
        {
            return player.m_customData.TryGetValue(PlayerInfo.ALMANAC_PLAYER_RECORDS, out string value)
                ? deserializer.Deserialize<PlayerInfo.PlayerRecords>(value)
                : new PlayerInfo.PlayerRecords();
        }
        catch
        {
            player.m_customData.Remove(PlayerInfo.ALMANAC_PLAYER_RECORDS);
            return null;
        }
    }
    public static void Save(this PlayerInfo.PlayerRecords records, Player player)
    {
        player.m_customData[PlayerInfo.ALMANAC_PLAYER_RECORDS] = serializer.Serialize(records);
    }
    public static int GetKnownRecipeCount(this Player player) => player.m_knownRecipes.Count;
    public static int GetKnownMaterialCount(this Player player) => player.m_knownMaterial.Count;
    public static int GetKnownPiecesCount(this Player player) => PieceHelper.GetPieces().FindAll(piece => piece.isKnown(player)).Count;
    public static int GetKnownWeapons(this Player player) => ItemHelper.weapons.FindAll(w => player.IsKnownMaterial(w.shared.m_name)).Count;
    public static int GetKnownSwords(this Player player) => ItemHelper.swords.FindAll(sword => player.IsKnownMaterial(sword.shared.m_name)).Count;
    public static int GetKnownAxes(this Player player) => ItemHelper.axes.FindAll(axe => player.IsKnownMaterial(axe.shared.m_name)).Count;
    public static int GetKnownPolearms(this Player player)  => ItemHelper.polearms.FindAll(polearm => player.IsKnownMaterial(polearm.shared.m_name)).Count;
    public static int GetKnownSpears(this Player player) => ItemHelper.spears.FindAll(spear => player.IsKnownMaterial(spear.shared.m_name)).Count;
    public static int GetKnownKnives(this Player player) => ItemHelper.knives.FindAll(knive => player.IsKnownMaterial(knive.shared.m_name)).Count;
    public static int GetKnownShields(this Player player) => ItemHelper.blocking.FindAll(shield => player.IsKnownMaterial(shield.shared.m_name)).Count;
    public static int GetKnownPotions(this Player player) => ItemHelper.potions.FindAll(potion => player.IsKnownMaterial(potion.shared.m_name)).Count;
    public static int GetKnownClubs(this Player player)=>ItemHelper.clubs.FindAll(c => player.IsKnownMaterial(c.shared.m_name)).Count;
    public static int GetKnownStaves(this Player player)=>ItemHelper.staves.FindAll(s => player.IsKnownMaterial(s.shared.m_name)).Count;
    public static int GetKnownBows(this Player player)=>ItemHelper.bows.FindAll(b => player.IsKnownMaterial(b.shared.m_name)).Count;
    public static int GetKnownTrophies(this Player player) => ItemHelper.trophies.FindAll(t => player.IsKnownMaterial(t.shared.m_name)).Count;
    public static int GetKnownBaits(this Player player) => ItemHelper.baits.FindAll(b => player.IsKnownMaterial(b.shared.m_name)).Count;
    public static int GetKnownFishes(this Player player) => ItemHelper.fishes.FindAll(f => player.IsKnownMaterial(f.shared.m_name)).Count;
    public static int GetKnownConsumables(this Player player) => ItemHelper.consumables.FindAll(c => player.IsKnownMaterial(c.shared.m_name)).Count;
    public static int GetKnownHelmets(this Player player) => ItemHelper.helmets.FindAll(h => player.IsKnownMaterial(h.shared.m_name)).Count;
    public static int GetKnownChests(this Player player) =>  ItemHelper.chests.FindAll(c => player.IsKnownMaterial(c.shared.m_name)).Count;
    public static int GetKnownLegs(this Player player) => ItemHelper.legs.FindAll(l => player.IsKnownMaterial(l.shared.m_name)).Count;
    public static int GetKnownValuables(this Player player) => ItemHelper.valuables.FindAll(v => player.IsKnownMaterial(v.shared.m_name)).Count;
    public static int GetKnownTrinkets(this Player player) => ItemHelper.trinkets.FindAll(t => player.IsKnownMaterial(t.shared.m_name)).Count;
    public static int GetKnownRecipes(this Player player) => player.m_knownRecipes.Count;
    public static void ClearRecords(this Player player)
    {
        player.m_customData.Remove(PlayerInfo.ALMANAC_PLAYER_RECORDS);
        PlayerInfo.ClearRecords();
    }
    public static int GetRecordByteCount(this Player player)
    {
        if (!player.m_customData.TryGetValue(PlayerInfo.ALMANAC_PLAYER_RECORDS, out string data)) return 0;
        int size = Encoding.UTF8.GetByteCount(data);
        return size;
    }
    public static int GetKills(this PlayerInfo.PlayerRecords records, string creatureName) => records.kills.TryGetValue(creatureName, out int value) ? value : 0;
    public static int GetDeaths(this PlayerInfo.PlayerRecords records, string creatureName) => records.deaths.TryGetValue(creatureName, out int value) ? value : 0;
    public static int GetItemPicked(this PlayerInfo.PlayerRecords records, string itemName) => records.itemsPicked.TryGetValue(itemName, out int value) ? value : 0;
}
public static class PlayerInfo
{
    public static readonly string ALMANAC_PLAYER_RECORDS = "AlmanacPlayerRecords";
    private static PlayerRecords? _cachedRecords;
    public static void ClearRecords() => _cachedRecords = new();
    private static PlayerRecords? Records
    {
        get
        {
            _cachedRecords ??= Player.m_localPlayer.GetRecords();
            return _cachedRecords;
        }
    }
    [Serializable]
    public class PlayerRecords
    {
        public Dictionary<string, int> kills = new();
        public Dictionary<string, int> deaths = new();
        public Dictionary<string, int> itemsPicked = new();
        public List<int> knownStatusEffects = new();
    }
    public enum RecordType
    {
        Kill,
        Death,
        Pickable,
    }
    public static J GetValueOrDefault<T, J>(this Dictionary<T, J> dict, T key, J defaultValue)
    {
        return dict.TryGetValue(key, out J value) ? value : defaultValue;
    }
    public static bool IsStatusEffectKnown(int hash) => Records?.knownStatusEffects.Contains(hash) ?? false;
    
    public static float GetPlayerStat(PlayerStatType type)
    {
        if (!Game.instance) return 0;
        if (Game.instance.m_playerProfile == null) return 0;
        return Game.instance.m_playerProfile.m_playerStats.m_stats.TryGetValue(type, out float value) ? value : 0;
    }
    public static int GetPlayerStat(RecordType type, string name)
    {
        if (!Player.m_localPlayer || string.IsNullOrEmpty(name)) return 0;
        switch (type)
        {
            case RecordType.Kill:
                return Records?.GetKills(name) ?? 0;
            case RecordType.Death:
                return Records?.GetDeaths(name) ?? 0;
            case RecordType.Pickable:
                return Records?.GetItemPicked(name) ?? 0;
            default:
                return 0;
        }
    }
    
    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    private static class CharacterOnDeathPatch
    {
        [UsedImplicitly]
        private static void Prefix(Character __instance)
        {
            if (!__instance || !Player.m_localPlayer) return;
            if (__instance.m_lastHit?.GetAttacker() is not { } attacker) return;
            if (attacker == Player.m_localPlayer)
            {
                Records?.kills.IncrementOrSet(__instance.m_name);
            }
            else if (__instance == Player.m_localPlayer)
            {
                Records?.deaths.IncrementOrSet(attacker.m_name);
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
    private static class PlayerEatFoodFix
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (__result) Game.instance.IncrementPlayerStat(PlayerStatType.FoodEaten);
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class PickableInteractPatch
    {
        [UsedImplicitly]
        private static void Postfix(Pickable __instance, Humanoid character, ref bool __result)
        {
            if (!__instance || !character || !__result) return;
            if (character is not Player player || player != Player.m_localPlayer) return;
            string itemName = __instance.name.Replace("(Clone)", string.Empty);
            Records?.itemsPicked.IncrementOrSet(itemName);
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.Internal_AddStatusEffect))]
    private static class SEManInternal_Internal_AddStatusEffect_Patch
    {
        [UsedImplicitly]
        private static void Postfix(SEMan __instance, int nameHash)
        {
            if (__instance.m_character != Player.m_localPlayer) return;
            if (Records?.knownStatusEffects.Contains(nameHash) ?? false) return;
            Records?.knownStatusEffects.Add(nameHash);
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(StatusEffect), typeof(bool), typeof(int),
        typeof(float))]
    private static class SEMan_AddStatusEffect_Patch
    {
        [UsedImplicitly]
        private static void Postfix(SEMan __instance, StatusEffect statusEffect)
        {
            if (__instance.m_character != Player.m_localPlayer) return;
            if (Records?.knownStatusEffects.Contains(statusEffect.NameHash()) ?? false) return;
            Records?.knownStatusEffects.Add(statusEffect.NameHash());
        }
    }
    public static void Setup()
    {
        AlmanacPlugin.OnPlayerProfileLoadPlayerData += player =>
        {
            _cachedRecords = player.GetRecords();
        };
        AlmanacPlugin.OnPlayerProfileSavePlayerDataPrefix += player =>
        {
            Records?.Save(player);
        };
    }
    public static List<Entries.Entry> GetEntries()
    {
        Entries.EntryBuilder builder = new();
        builder.m_showAll = true;
        builder.Add(Keys.Resistances);
        foreach (HitData.DamageType mod in Enum.GetValues(typeof(HitData.DamageType)))
        {
            HitData.DamageModifier modifier = Player.m_localPlayer.m_damageModifiers.GetModifier(mod);
            builder.Add(mod, modifier);
        }
        builder.Add(Keys.Combat);
        builder.Add(PlayerStatType.EnemyKills);
        builder.Add(PlayerStatType.PlayerKills);
        builder.Add(PlayerStatType.HitsTakenEnemies);
        builder.Add(PlayerStatType.Deaths);
        builder.Add(PlayerStatType.EnemyHits);
        builder.Add(PlayerStatType.EnemyKillsLastHits);
        builder.Add(PlayerStatType.BossKills);
        builder.Add(PlayerStatType.BossLastHits);
        builder.Add(PlayerStatType.ArrowsShot);
        builder.Add(Keys.Forsaken);
        builder.Add(PlayerStatType.SetGuardianPower);
        builder.Add(PlayerStatType.SetPowerEikthyr);
        builder.Add(PlayerStatType.SetPowerElder);
        builder.Add(PlayerStatType.SetPowerBonemass);
        builder.Add(PlayerStatType.SetPowerModer);
        builder.Add(PlayerStatType.SetPowerYagluth);
        builder.Add(PlayerStatType.SetPowerQueen);
        builder.Add(PlayerStatType.SetPowerAshlands);
        builder.Add(PlayerStatType.SetPowerDeepNorth);
        builder.Add(PlayerStatType.UsePowerEikthyr);
        builder.Add(PlayerStatType.UsePowerElder);
        builder.Add(PlayerStatType.UsePowerBonemass);
        builder.Add(PlayerStatType.UsePowerModer);
        builder.Add(PlayerStatType.UsePowerYagluth);
        builder.Add(PlayerStatType.UsePowerQueen);
        builder.Add(PlayerStatType.UsePowerAshlands);
        builder.Add(PlayerStatType.UsePowerDeepNorth);
        builder.Add(Keys.Mortality);
        builder.Add(PlayerStatType.Deaths);
        builder.Add(PlayerStatType.DeathByBoat);
        builder.Add(PlayerStatType.DeathByBurning);
        builder.Add(PlayerStatType.DeathByCart);
        builder.Add(PlayerStatType.DeathByDrowning);
        builder.Add(PlayerStatType.DeathByFall);
        builder.Add(PlayerStatType.DeathByFreezing);
        builder.Add(PlayerStatType.DeathByImpact);
        builder.Add(PlayerStatType.DeathByPoisoned);
        builder.Add(PlayerStatType.DeathBySelf);
        builder.Add(PlayerStatType.DeathBySmoke);
        builder.Add(PlayerStatType.DeathByStalagtite);
        builder.Add(PlayerStatType.DeathByStructural);
        builder.Add(PlayerStatType.DeathByTree);
        builder.Add(PlayerStatType.DeathByTurret);
        builder.Add(PlayerStatType.DeathByWater);
        builder.Add(PlayerStatType.DeathByPlayerHit);
        builder.Add(PlayerStatType.DeathByEdgeOfWorld);
        builder.Add(PlayerStatType.DeathByUndefined);
        builder.Add(Keys.Miscellaneous);
        builder.Add(PlayerStatType.Cheats);
        builder.Add(PlayerStatType.WorldLoads);
        builder.Add(PlayerStatType.TombstonesFit);
        builder.Add(PlayerStatType.TombstonesOpenedOther);
        builder.Add(PlayerStatType.TombstonesOpenedOwn);
        builder.Add(Keys.Husbandry);
        builder.Add(PlayerStatType.SkeletonSummons);
        builder.Add(PlayerStatType.CreatureTamed);
        builder.Add(PlayerStatType.CreatureTamed);
        builder.Add(Keys.Harvest);
        builder.Add(PlayerStatType.ItemsPickedUp);
        builder.Add(PlayerStatType.BeesHarvested);
        builder.Add(PlayerStatType.SapHarvested);
        builder.Add(PlayerStatType.FoodEaten);
        builder.Add(Keys.Build);
        builder.Add(PlayerStatType.PlaceStacks);
        builder.Add(PlayerStatType.CraftsOrUpgrades);
        builder.Add(PlayerStatType.Builds);
        builder.Add(PlayerStatType.Upgrades);
        builder.Add(PlayerStatType.Crafts);
        builder.Add(Keys.Defense);
        builder.Add(PlayerStatType.TurretAmmoAdded);
        builder.Add(PlayerStatType.TurretTrophySet);
        builder.Add(PlayerStatType.TrapArmed);
        builder.Add(PlayerStatType.TrapTriggered);
        builder.Add(Keys.Dungeon);
        builder.Add(PlayerStatType.PortalDungeonIn);
        builder.Add(PlayerStatType.PortalDungeonOut);
        builder.Add(Keys.Travel);
        builder.Add(PlayerStatType.DistanceTraveled);
        builder.Add(PlayerStatType.DistanceWalk);
        builder.Add(PlayerStatType.DistanceRun);
        builder.Add(PlayerStatType.DistanceSail);
        builder.Add(PlayerStatType.DistanceAir);
        builder.Add(PlayerStatType.Jumps);
        builder.Add(PlayerStatType.PortalsUsed);
        builder.Add(Keys.Base);
        builder.Add(PlayerStatType.DoorsOpened);
        builder.Add(PlayerStatType.DoorsClosed);
        builder.Add(PlayerStatType.TimeInBase);
        builder.Add(PlayerStatType.TimeOutOfBase);
        builder.Add(PlayerStatType.Sleep);
        builder.Add(PlayerStatType.ItemStandUses);
        builder.Add(PlayerStatType.ArmorStandUses);
        builder.Add(Keys.Trees);
        builder.Add(PlayerStatType.TreeChops);
        builder.Add(PlayerStatType.Tree);
        builder.Add(PlayerStatType.TreeTier0);
        builder.Add(PlayerStatType.TreeTier1);
        builder.Add(PlayerStatType.TreeTier2);
        builder.Add(PlayerStatType.TreeTier3);
        builder.Add(PlayerStatType.TreeTier4);
        builder.Add(PlayerStatType.TreeTier5);
        builder.Add(PlayerStatType.Logs);
        builder.Add(PlayerStatType.LogChops);
        builder.Add(Keys.Mining);
        builder.Add(PlayerStatType.MineHits);
        builder.Add(PlayerStatType.Mines);
        builder.Add(PlayerStatType.MineTier0);
        builder.Add(PlayerStatType.MineTier1);
        builder.Add(PlayerStatType.MineTier2);
        builder.Add(PlayerStatType.MineTier3);
        builder.Add(PlayerStatType.MineTier4);
        builder.Add(PlayerStatType.MineTier5);
        builder.Add(Keys.Raven);
        builder.Add(PlayerStatType.RavenAppear);
        builder.Add(PlayerStatType.RavenHits);
        builder.Add(PlayerStatType.RavenTalk);
        return builder.ToList();
    }
    private static List<MetricInfo> PickableMetrics()
    {
        List<MetricInfo> _metrics = new List<MetricInfo>();
        foreach (Pickable? pickable in ItemHelper.pickables)
        {
            if (!ItemHelper.pickableItems.TryGetValue(pickable.name, out ItemHelper.ItemInfo info)) continue;
            _metrics.Add(new MetricInfo(info.shared.m_name, info.GetIcon(), () => $"{Keys.Picked} x{GetPlayerStat(RecordType.Pickable, pickable.name)}"));
        }
        return _metrics;
    }
    public static List<MetricInfo> GetMetrics()
    {
        return Metrics;
        // if (!Player.m_localPlayer) return new();
        // List<MetricInfo> _metrics = new List<MetricInfo>
        // {
        //     new MetricInfo(Keys.Materials, SpriteManager.IconOption.Gem, () => $"{Player.m_localPlayer.GetKnownMaterialCount()}/{ItemHelper.materials.Count}"),
        //     new MetricInfo(Keys.Pieces, "Hammer", () => $"{Player.m_localPlayer.GetKnownPiecesCount()}/{PieceHelper.GetPieces().Count}"),
        //     new MetricInfo(Keys.Weapon, SpriteManager.IconOption.SwordBrown, () => $"{Player.m_localPlayer.GetKnownWeapons()}/{ItemHelper.weapons.Count}"),
        //     new MetricInfo(Keys.Trophies, "TrophyBoar", () => $"{Player.m_localPlayer.GetKnownTrophies()}/{ItemHelper.trophies.Count}"),
        //     new MetricInfo(Keys.Swords, SpriteManager.IconOption.SwordBlue, () => $"{Player.m_localPlayer.GetKnownSwords()}/{ItemHelper.swords.Count}"),
        //     new MetricInfo(Keys.Axes, "AxeIron", () => $"{Player.m_localPlayer.GetKnownAxes()}/{ItemHelper.axes.Count}"),
        //     new MetricInfo(Keys.Polearms, "AtgeirIron", () => $"{Player.m_localPlayer.GetKnownPolearms()}/{ItemHelper.polearms.Count}"),
        //     new MetricInfo(Keys.Spears, "SpearBronze", () => $"{Player.m_localPlayer.GetKnownSpears()}/{ItemHelper.spears.Count}"),
        //     new MetricInfo(Keys.Knives, "KnifeSkollAndHati", () => $"{Player.m_localPlayer.GetKnownKnives()}/{ItemHelper.knives.Count}"),
        //     new MetricInfo(Keys.Clubs, "MaceBronze", () => $"{Player.m_localPlayer.GetKnownClubs()}/{ItemHelper.clubs.Count}"),
        //     new MetricInfo(Keys.Shield, SpriteManager.IconOption.Shield, () => $"{Player.m_localPlayer.GetKnownShields()}/{ItemHelper.blocking.Count}"),
        //     new MetricInfo(Keys.Bows, "Bow", () => $"{Player.m_localPlayer.GetKnownBows()}/{ItemHelper.bows.Count}"),
        //     new MetricInfo(Keys.Fish, SpriteManager.IconOption.Fish, () => $"{Player.m_localPlayer.GetKnownFishes()}/{ItemHelper.fishes.Count}"),
        //     new MetricInfo(Keys.Consumables, SpriteManager.IconOption.MushroomRed, () => $"{Player.m_localPlayer.GetKnownConsumables()}/{ItemHelper.consumables.Count}"),
        //     new MetricInfo(Keys.Potions, SpriteManager.IconOption.BottleBlue, () => $"{Player.m_localPlayer.GetKnownPotions()}/{ItemHelper.potions.Count}"),
        //     new MetricInfo(Keys.Staves, "StaffFireball", () => $"{Player.m_localPlayer.GetKnownStaves()}/{ItemHelper.staves.Count}"),
        //     new MetricInfo(Keys.Baits, "FishingBaitAshlands", () => $"{Player.m_localPlayer.GetKnownBaits()}/{ItemHelper.baits.Count}"),
        //     new MetricInfo(Keys.Helmet, "HelmetIron", () => $"{Player.m_localPlayer.GetKnownHelmets()}/{ItemHelper.helmets.Count}"),
        //     new MetricInfo(Keys.ChestArmor, "ArmorIronChest", () => $"{Player.m_localPlayer.GetKnownChests()}/{ItemHelper.chests.Count}"),
        //     new MetricInfo(Keys.LegArmor, "ArmorIronLegs", () => $"{Player.m_localPlayer.GetKnownLegs()}/{ItemHelper.legs.Count}"),
        //     new MetricInfo(Keys.Valuables, "Amber", () => $"{Player.m_localPlayer.GetKnownValuables()}/{ItemHelper.valuables.Count}"),
        //     new MetricInfo(Keys.Trinkets, "TrinketBronzeStamina", () => $"{Player.m_localPlayer.GetKnownTrinkets()}/{ItemHelper.trinkets.Count}"), 
        // };
        // return _metrics;
    }

    private static readonly List<MetricInfo> _metrics = new();
    private static List<MetricInfo> Metrics
    {
        get
        {
            if (_metrics.Count != 0) return _metrics;
            _ = new MetricInfo(Keys.Materials, SpriteManager.IconOption.Gem,
                () => $"{Player.m_localPlayer.GetKnownMaterialCount()}/{ItemHelper.materials.Count}");
            _ = new MetricInfo(Keys.Pieces, "Hammer",
                () => $"{Player.m_localPlayer.GetKnownPiecesCount()}/{PieceHelper.GetPieces().Count}");
            _ = new MetricInfo(Keys.Weapon, SpriteManager.IconOption.SwordBrown,
                () => $"{Player.m_localPlayer.GetKnownWeapons()}/{ItemHelper.weapons.Count}");
            _ = new MetricInfo(Keys.Trophies, "TrophyBoar",
                () => $"{Player.m_localPlayer.GetKnownTrophies()}/{ItemHelper.trophies.Count}");
            _ = new MetricInfo(Keys.Swords, SpriteManager.IconOption.SwordBlue,
                () => $"{Player.m_localPlayer.GetKnownSwords()}/{ItemHelper.swords.Count}");
            _ = new MetricInfo(Keys.Axes, "AxeIron",
                () => $"{Player.m_localPlayer.GetKnownAxes()}/{ItemHelper.axes.Count}");
            _ = new MetricInfo(Keys.Polearms, "AtgeirIron",
                () => $"{Player.m_localPlayer.GetKnownPolearms()}/{ItemHelper.polearms.Count}");
            _ = new MetricInfo(Keys.Spears, "SpearBronze",
                () => $"{Player.m_localPlayer.GetKnownSpears()}/{ItemHelper.spears.Count}");
            _ = new MetricInfo(Keys.Knives, "KnifeSkollAndHati",
                () => $"{Player.m_localPlayer.GetKnownKnives()}/{ItemHelper.knives.Count}");
            _ = new MetricInfo(Keys.Clubs, "MaceBronze",
                () => $"{Player.m_localPlayer.GetKnownClubs()}/{ItemHelper.clubs.Count}");
            _ = new MetricInfo(Keys.Shield, SpriteManager.IconOption.Shield,
                () => $"{Player.m_localPlayer.GetKnownShields()}/{ItemHelper.blocking.Count}");
            _ = new MetricInfo(Keys.Bows, "Bow",
                () => $"{Player.m_localPlayer.GetKnownBows()}/{ItemHelper.bows.Count}");
            _ = new MetricInfo(Keys.Fish, SpriteManager.IconOption.Fish,
                () => $"{Player.m_localPlayer.GetKnownFishes()}/{ItemHelper.fishes.Count}");
            _ = new MetricInfo(Keys.Consumables, SpriteManager.IconOption.MushroomRed,
                () => $"{Player.m_localPlayer.GetKnownConsumables()}/{ItemHelper.consumables.Count}");
            _ = new MetricInfo(Keys.Potions, SpriteManager.IconOption.BottleBlue,
                () => $"{Player.m_localPlayer.GetKnownPotions()}/{ItemHelper.potions.Count}");
            _ = new MetricInfo(Keys.Staves, "StaffFireball",
                () => $"{Player.m_localPlayer.GetKnownStaves()}/{ItemHelper.staves.Count}");
            _ = new MetricInfo(Keys.Baits, "FishingBaitAshlands",
                () => $"{Player.m_localPlayer.GetKnownBaits()}/{ItemHelper.baits.Count}");
            _ = new MetricInfo(Keys.Helmet, "HelmetIron",
                () => $"{Player.m_localPlayer.GetKnownHelmets()}/{ItemHelper.helmets.Count}");
            _ = new MetricInfo(Keys.ChestArmor, "ArmorIronChest",
                () => $"{Player.m_localPlayer.GetKnownChests()}/{ItemHelper.chests.Count}");
            _ = new MetricInfo(Keys.LegArmor, "ArmorIronLegs",
                () => $"{Player.m_localPlayer.GetKnownLegs()}/{ItemHelper.legs.Count}");
            _ = new MetricInfo(Keys.Valuables, "Amber",
                () => $"{Player.m_localPlayer.GetKnownValuables()}/{ItemHelper.valuables.Count}");
            _ = new MetricInfo(Keys.Trinkets, "TrinketBronzeStamina",
                () => $"{Player.m_localPlayer.GetKnownTrinkets()}/{ItemHelper.trinkets.Count}");
            _ = new MetricInfo(Keys.Recipes, SpriteManager.IconOption.BookRed,
                () => $"{Player.m_localPlayer.GetKnownRecipeCount()}/{ItemHelper.recipes.Count}");
            return _metrics;
        }
    }
    public readonly struct MetricInfo
    {
        public readonly string Name;
        public readonly Sprite? Icon;
        private readonly Func<string> _description;
        public string Description => _description.Invoke();
        
        public MetricInfo(string name, SpriteManager.IconOption icon, Func<string> description) : this(name, SpriteManager.GetSprite(icon), description) { }
        
        public MetricInfo(string name, string icon, Func<string> description) : this(name, SpriteManager.GetSprite(icon), description){}

        public MetricInfo(string name, Sprite? icon, Func<string> description)
        {
            Name = name;
            Icon = icon;
            _description = description;
            _metrics.Add(this);
        }
    }
}