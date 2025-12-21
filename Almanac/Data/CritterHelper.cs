using System.Collections.Generic;
using System.Linq;
using Almanac.Managers;
using Almanac.UI;
using Almanac.Utilities;
using UnityEngine;
using static Almanac.Utilities.Entries;

namespace Almanac.Data;
public static class CritterHelper
{
    private static ItemDrop.ItemData? FindTrophy(this List<CharacterDrop.Drop> list)
    {
        foreach (CharacterDrop.Drop? drop in list)
        {
            if (drop.m_prefab == null) continue;
            if (!drop.m_prefab.TryGetComponent(out ItemDrop itemDrop)) continue;
            if (itemDrop.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Trophy) return itemDrop.m_itemData;
        }

        return null;
    }
    public static AlmanacPanel.InfoView.Icons.DropInfo[] ToDropInfos(this List<CharacterDrop.Drop> drops)
    {
        List<AlmanacPanel.InfoView.Icons.DropInfo> infos = new();
        foreach (CharacterDrop.Drop? drop in drops)
        {
            if (!drop.m_prefab.TryGetComponent(out ItemDrop component)) continue;
            infos.Add(new AlmanacPanel.InfoView.Icons.DropInfo(component.m_itemData, drop.m_chance, drop.m_amountMin, drop.m_amountMax));
        }

        return infos.ToArray();
    }
    public static void Setup()
    {
        AlmanacPlugin.OnZNetScenePrefabs += OnZNetScenePrefabs;
    }
    private static void OnZNetScenePrefabs(GameObject prefab)
    {
        if (prefab == null || !prefab.TryGetComponent(out Character character)) return;
        if (character is Player) return;
        _ = new CritterInfo(prefab);
    }
    private static readonly Dictionary<string, Heightmap.Biome> creatureBiomes = new()
    {
        ["Boar"] = Heightmap.Biome.Meadows,
        ["Boar_piggy"] = Heightmap.Biome.Meadows,
        ["Neck"] = Heightmap.Biome.Meadows,
        ["Deer"] =  Heightmap.Biome.Meadows,
        ["Greyling"] = Heightmap.Biome.Meadows,
        ["Eikthyr"] = Heightmap.Biome.Meadows,
        ["Greydwarf"] = Heightmap.Biome.BlackForest,
        ["Greydwarf_Shaman"] = Heightmap.Biome.BlackForest,
        ["Greydwarf_Elite"] = Heightmap.Biome.BlackForest,
        ["Troll"] = Heightmap.Biome.BlackForest,
        ["gd_king"] = Heightmap.Biome.BlackForest,
        ["Skeleton"] =  Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp,
        ["Leech"] = Heightmap.Biome.Swamp,
        ["Abomination"] = Heightmap.Biome.Swamp,
        ["Draugr"] =  Heightmap.Biome.Swamp,
        ["Draugr_Ranged"] = Heightmap.Biome.Swamp,
        ["Draugr_Elite"] = Heightmap.Biome.Swamp,
        ["Blob"] =   Heightmap.Biome.Swamp,
        ["BlobElite"] = Heightmap.Biome.Swamp,
        ["Surtling"] = Heightmap.Biome.Swamp,
        ["Wraith"] =  Heightmap.Biome.Swamp,
        ["Skeleton_Poison"] = Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp,
        ["Wolf"] = Heightmap.Biome.Mountain,
        ["Ulv"] = Heightmap.Biome.Mountain,
        ["Bat"] = Heightmap.Biome.Mountain,
        ["Fenring"] = Heightmap.Biome.Mountain,
        ["Hatchling"] =   Heightmap.Biome.Mountain,
        ["StoneGolem"] = Heightmap.Biome.Mountain,
        ["Fenring_Cultist"] =  Heightmap.Biome.Mountain,
        ["Goblin"] = Heightmap.Biome.Plains,
        ["GoblinArcher"] = Heightmap.Biome.Plains,
        ["GoblinBrute"] =  Heightmap.Biome.Plains,
        ["Lox"] =  Heightmap.Biome.Plains,
        ["Lox_Calf"] =  Heightmap.Biome.Plains,
        ["Deathsquito"] =   Heightmap.Biome.Plains,
        ["BlobTar"] =   Heightmap.Biome.Plains,
        ["GoblinShaman"] =   Heightmap.Biome.Plains,
        ["GoblinKing"] = Heightmap.Biome.Plains,
        ["Seeker"] = Heightmap.Biome.Mistlands,
        ["SeekerBrute"] = Heightmap.Biome.Mistlands,
        ["Hare"] = Heightmap.Biome.Mistlands,
        ["SeekerBrood"] = Heightmap.Biome.Mistlands,
        ["Gjall"] = Heightmap.Biome.Mistlands,
        ["Tick"] = Heightmap.Biome.Mistlands,
        ["Dverger"] = Heightmap.Biome.Mistlands,
        ["DvergerMage"] = Heightmap.Biome.Mistlands,
        ["DvergerMageFire"] = Heightmap.Biome.Mistlands,
        ["DvergerMageIce"] =  Heightmap.Biome.Mistlands,
        ["DvergerMageSupport"] = Heightmap.Biome.Mistlands,
        ["SeekerQueen"] = Heightmap.Biome.Mistlands,
        ["Mistile"] = Heightmap.Biome.Mistlands,
        ["Asksvin"] = Heightmap.Biome.AshLands,
        ["Asksvin_hatchling"] = Heightmap.Biome.AshLands,
        ["Morgen"] = Heightmap.Biome.AshLands,
        ["Morgen_NonSleeping"] = Heightmap.Biome.AshLands,
        ["Volture"] = Heightmap.Biome.AshLands,
        ["Charred_Melee"] = Heightmap.Biome.AshLands,
        ["BlobLava"] = Heightmap.Biome.AshLands,
        ["Charred_Archer"] = Heightmap.Biome.AshLands,
        ["Charred_Mage"] = Heightmap.Biome.AshLands,
        ["FallenValkyrie"] = Heightmap.Biome.AshLands,
        ["Fader"] = Heightmap.Biome.AshLands,
        ["Skeleton_Hildir"] = Heightmap.Biome.BlackForest,
        ["GoblinShaman_Hildir"] = Heightmap.Biome.Plains,
        ["GoblinBruteBros"] = Heightmap.Biome.Plains,
        ["GoblinBrute_Hildir"] = Heightmap.Biome.Plains,
        ["Fenring_Cultist_Hildir"] = Heightmap.Biome.Plains,
        ["Charred_Melee_Dyrnwyn"] = Heightmap.Biome.AshLands,
        ["DvergerAshlands"] = Heightmap.Biome.AshLands,
        ["BogWitchKvastur"] = Heightmap.Biome.Swamp,
        ["Hen"] = Heightmap.Biome.Mistlands,
        ["Chicken"] = Heightmap.Biome.Mistlands,
        ["Charred_Twicher"] = Heightmap.Biome.AshLands,
        ["Dragon"] = Heightmap.Biome.Mountain,
        ["Bjorn"] = Heightmap.Biome.BlackForest,
        ["Bonemass"] = Heightmap.Biome.Swamp,
        ["BlobFrost"] = Heightmap.Biome.Mountain,
        ["BonemawSerpent"] = Heightmap.Biome.AshLands,
        ["Charred_Archer_Fader"] = Heightmap.Biome.AshLands,
        ["Charred_Melee_Fader"] = Heightmap.Biome.AshLands,
        ["Charred_Twitcher"] = Heightmap.Biome.AshLands,
        ["Charred_Twitcher_Summoned"] = Heightmap.Biome.AshLands,
        ["Ghost"] = Heightmap.Biome.BlackForest,
        ["Serpent"] = Heightmap.Biome.Ocean,
        ["TentaRoot"] = Heightmap.Biome.BlackForest,
        ["Unbjorn"] = Heightmap.Biome.Plains,
        ["Wolf_cub"] = Heightmap.Biome.Mountain,
    };
    public static Heightmap.Biome GetBiome(string name) => creatureBiomes.TryGetValue(name, out var biome) ? biome : Heightmap.Biome.None;
    public static List<CritterInfo> GetCritters() => critters.FindAll(c => !Filters.Ignore(c.prefab.name));
    public static CritterInfo? GetInfo(this Character character) => namedCritters.TryGetValue(character.name, out var info) ? info : null;
    private static readonly List<CritterInfo> critters = new();
    public static readonly Dictionary<string, CritterInfo> namedCritters = new();
    private static readonly Dictionary<string, CritterInfo> sharedCritters = new();
    public static bool Exists(string name) => namedCritters.ContainsKey(name) || sharedCritters.ContainsKey(name);
    public readonly record struct CritterInfo
    {
        private static readonly EntryBuilder builder = new();
        public readonly GameObject prefab;
        public readonly Character character;
        private readonly BaseAI ai = null!;
        private readonly HashSet<ItemDrop> items = new();
        private readonly HashSet<Attack> attacks = new();
        public readonly HashSet<ItemDrop> consumeItems = new();
        public readonly CharacterDrop? drops;
        private readonly Tameable? tameable;
        private readonly Growup? growUp;
        public readonly ItemDrop.ItemData? trophy = null;
        public bool isTameable => tameable != null;
        public bool isKnown() => PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, character.m_name) > 0;
        public CritterInfo(GameObject prefab)
        {
            this.prefab = prefab;
            character = prefab.GetComponent<Character>();
            ai = prefab.GetComponent<BaseAI>();
            if (ai != null && ai is MonsterAI { m_onConsumedItem: not null } monsterAI)
            {
                foreach (ItemDrop? item in monsterAI.m_consumeItems)
                {
                    if (item == null) continue;
                    consumeItems.Add(item);
                }
            }
            prefab.TryGetComponent(out drops);
            trophy = drops?.m_drops.FindTrophy() ?? null;
            prefab.TryGetComponent(out tameable);
            growUp = prefab.GetComponent<Growup>();
            
            if (character is Humanoid humanoid)
            {
                if (humanoid.m_defaultItems != null)
                {
                    foreach (GameObject? item in humanoid.m_defaultItems)
                    {
                        if (item == null || !item.TryGetComponent(out ItemDrop itemDrop)) continue;
                        items.Add(itemDrop);
                    }
                }

                if (humanoid.m_randomWeapon != null)
                {
                    foreach (GameObject? item in humanoid.m_randomWeapon)
                    {
                        if (item == null || !item.TryGetComponent(out ItemDrop itemDrop)) continue;
                        items.Add(itemDrop);
                    }
                }

                if (humanoid.m_randomShield != null)
                {
                    foreach (GameObject? item in humanoid.m_randomShield)
                    {
                        if (item == null || !item.TryGetComponent(out ItemDrop itemDrop)) continue;
                        items.Add(itemDrop);
                    }
                }

                if (humanoid.m_randomItems != null)
                {
                    foreach (Humanoid.RandomItem? randomItem in humanoid.m_randomItems)
                    {
                        if (randomItem.m_prefab == null || !randomItem.m_prefab.TryGetComponent(out ItemDrop itemDrop)) continue;
                        items.Add(itemDrop);
                    }
                }

                if (humanoid.m_randomSets != null)
                {
                    foreach (Humanoid.ItemSet? set in humanoid.m_randomSets)
                    {
                        foreach (GameObject? item in set.m_items)
                        {
                            if (item == null || !item.TryGetComponent(out ItemDrop itemDrop)) continue;
                            items.Add(itemDrop);
                        }
                    }
                }

                foreach (ItemDrop? item in items)
                {
                    attacks.Add(item.m_itemData.m_shared.m_attack);
                    attacks.Add(item.m_itemData.m_shared.m_secondaryAttack);
                }
            }

            critters.Add(this);
            namedCritters[prefab.name] = this;
            sharedCritters[character.m_name] = this;
        }

        private List<Entry> ToEntries()
        {
            builder.Clear();    
            if (Configs.ShowAllData)
            {
                builder.Add(Keys.InternalID, prefab.name);
            }
            int killCount = PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Kill, character.m_name);
            int deathCount = PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Death, character.m_name);
            builder.Add(Keys.Killed, killCount);
            builder.Add(Keys.Died, deathCount);
            builder.Add(Keys.Character);
            builder.Add(Keys.Health, character.m_health);
            builder.Add(Keys.Faction, character.m_faction);
            builder.Add(Keys.Biome, GetBiome(prefab.name));
            builder.Add(Keys.Group, character.m_group);
            builder.Add(Keys.Resistances);
            builder.Add(Keys.Blunt, character.m_damageModifiers.m_blunt);
            builder.Add(Keys.Slash, character.m_damageModifiers.m_slash);
            builder.Add(Keys.Pierce, character.m_damageModifiers.m_pierce);
            builder.Add(Keys.Chop, character.m_damageModifiers.m_chop);
            builder.Add(Keys.Pickaxe, character.m_damageModifiers.m_pickaxe);
            builder.Add(Keys.Fire, character.m_damageModifiers.m_fire);
            builder.Add(Keys.Frost, character.m_damageModifiers.m_frost);
            builder.Add(Keys.Lightning, character.m_damageModifiers.m_lightning);
            builder.Add(Keys.Poison, character.m_damageModifiers.m_poison);
            builder.Add(Keys.Spirit, character.m_damageModifiers.m_spirit);
            builder.Add(Keys.CreatureData);
            builder.Add(Keys.AvoidFire, ai.m_avoidFire);
            builder.Add(Keys.AfraidOfFire, ai.m_afraidOfFire);
            builder.Add(Keys.AvoidWater, ai.m_avoidWater);
            foreach(WeakSpot weakspot in character.m_weakSpots) builder.Add(Keys.Weakspot, weakspot.name);
            builder.Add(Keys.StaggerWhenBlocked, character.m_staggerWhenBlocked);
            builder.Add(Keys.StaggerDamageFactor, character.m_staggerDamageFactor);
            builder.Add(Keys.TolerateWater, character.m_tolerateWater);
            builder.Add(Keys.TolerateSmoke, character.m_tolerateSmoke);
            builder.Add(Keys.TolerateTar, character.m_tolerateTar);
            builder.Add(Keys.DefeatKey, character.m_defeatSetGlobalKey, Keys.None);
            if (growUp != null)
            {
                builder.Add(Keys.GrowDuration, growUp.m_growTime);
            }
            if (tameable != null)
            {
                builder.Add(Keys.Husbandry);
                builder.Add(Keys.FedDuration, tameable.m_fedDuration);
                builder.Add(Keys.TamingDuration, tameable.m_tamingTime);
                builder.Add(Keys.Commandable, tameable.m_commandable);
            }
            return builder.ToList();
        }

        public void OnClick(AlmanacPanel panel, AlmanacPanel.ButtonView.ElementButton? element)
        {
            if (element != null) panel.buttonView.SetSelected(element);
            panel.description.Reset();
            panel.description.SetName(character.m_name);
            if (AlmanacPanel.isLocalAdminOrHostAndNoCost)
            {
                panel.description.Interactable(true);
                panel.description.SetButtonText(Keys.Spawn);
                var go = prefab;
                panel.OnMainButton = () =>
                {
                    Object.Instantiate(go, Player.m_localPlayer.transform.position, Quaternion.identity);
                };
            }
            panel.description.SetIcon(trophy?.HasIcons() ?? false ? trophy.GetIcon() : null);
            ToEntries().Build(panel.description.view);
            if (this is { isTameable: true, consumeItems.Count: > 0 })
            {
                panel.description.view.CreateTitle().SetTitle(Keys.ConsumeItem);
                if (consumeItems.Count > 4)
                {
                    IEnumerable<List<ItemDrop>> batches = consumeItems.ToList().Batch(4);
                    foreach (List<ItemDrop>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToArray());
                    }
                }
                else panel.description.view.CreateIcons().SetIcons(consumeItems.ToArray());
            }

            if (drops != null)
            {
                panel.description.view.CreateTitle().SetTitle(Keys.CharacterDrops);
                if (drops.m_drops.Count > 4)
                {
                    IEnumerable<List<CharacterDrop.Drop>> batches = drops.m_drops.Batch(4);
                    foreach (List<CharacterDrop.Drop>? batch in batches)
                    {
                        panel.description.view.CreateIcons().SetIcons(batch.ToDropInfos());
                    }
                }
                else panel.description.view.CreateIcons().SetIcons(drops.m_drops.ToDropInfos());
            }
            panel.description.view.Resize();
        }
    }
}