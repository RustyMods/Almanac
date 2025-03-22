using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.Achievements;
using Almanac.FileSystem;
using Almanac.UI;
using Almanac.Utilities;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;
using Patches = Almanac.FileSystem.Patches;

namespace Almanac.Data;

public static class PlayerStats
{
    public const string AlmanacStatsKey = "AlmanacStats";
    public static CustomData LocalPlayerData = new();

    public static float GetPlayerStat(PlayerStatType type)
    {
        if (!Game.instance) return 0;
        if (Game.instance.m_playerProfile == null) return 0;
        return Game.instance.m_playerProfile.m_playerStats.m_stats.TryGetValue(type, out float value) ? value : 0;
    }
    private static void InitPlayerTracker()
    {
        if (!Player.m_localPlayer) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Initializing Player Kill Death Tracker");

        if (Player.m_localPlayer.m_customData.TryGetValue(AlmanacStatsKey, out string CurrentData))
        {
            ReadCustomTrackerData(CurrentData, false);
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Generating kill death custom data");
            foreach (string key in Creatures.m_defeatKeys)
            {
                LocalPlayerData.Player_Kill_Deaths[key] = new KillDeaths();
            }
        }
        WriteCurrentCustomData();
    }
    private static void ReadCustomTrackerData(string CurrentData, bool hasFile, bool hasCustomData = true)
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        if (hasCustomData) AlmanacPlugin.AlmanacLogger.LogDebug("Client: Player has custom data kill death values" 
                                             + (hasFile ? ", ignoring file" : ""));
        CustomData data = deserializer.Deserialize<CustomData>(CurrentData);
        LocalPlayerData = data;
        
        if (data.Player_Kill_Deaths.Count == Creatures.m_defeatKeys.Count) return;
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Creature count changed, adding missing values");
        foreach (string key in Creatures.m_defeatKeys)
        {
            if (data.Player_Kill_Deaths.ContainsKey(key)) continue;
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Player data missing: " + key + " , adding value");
            data.Player_Kill_Deaths[key] = new KillDeaths();
        }
    }
    private static void WriteCurrentCustomData()
    {
        if (!Player.m_localPlayer) return;
        try
        {
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(LocalPlayerData);
            var names = EffectMan.ActiveAchievementEffects.Select(x => x.name).ToList();
            Player.m_localPlayer.m_customData[EffectMan.PlayerEffectKey] = serializer.Serialize(names);
            Player.m_localPlayer.m_customData[AlmanacStatsKey] = data;
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Saving almanac custom data");
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Failed to save player data");
        }
    }
    private static void UpdatePlayerDeaths(Player instance)
    {
        if (!instance) return;
        if (instance != Player.m_localPlayer) return;

        HitData? lastHit = instance.m_lastHit;
        if (lastHit == null) return;
        Character? killer = lastHit.GetAttacker();
        if (!killer) return;

        string key = killer.m_defeatSetGlobalKey;
        if (!LocalPlayerData.Player_Kill_Deaths.ContainsKey(key)) return;
        
        ++LocalPlayerData.Player_Kill_Deaths[key].deaths;
    }
    private static void UpdatePlayerKills(Character instance)
    {
        if (!instance || !Player.m_localPlayer) return;

        HitData? lastHit = instance.m_lastHit;
        if (lastHit == null) return;
        Character? killer = instance.m_lastHit.GetAttacker();
        if (!killer) return;
        if (killer.GetHoverName() != Player.m_localPlayer.GetHoverName()) return;
            
        string key = instance.m_defeatSetGlobalKey;
        if (!LocalPlayerData.Player_Kill_Deaths.ContainsKey(key)) return;
        
        ++LocalPlayerData.Player_Kill_Deaths[key].kills;
    }
    public static int GetKnownTextCount() => !Player.m_localPlayer ? 0 : Player.m_localPlayer.m_knownTexts.Count;
    public static int GetKnownRecipeCount() => !Player.m_localPlayer ? 0 : Player.m_localPlayer.m_knownRecipes.Count;
    private static void AddRuneStoneLabels(RuneStone instance)
    {
        try
        {
            if (!instance) return;
            foreach (RuneStone.RandomRuneText text in instance.m_randomTexts)
            {
                if (!text.m_label.IsNullOrWhiteSpace()) continue;
                if (text.m_text.IsNullOrWhiteSpace()) continue;
                text.m_label = text.m_text + "_label";
            }
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to add label to runestone");
        }
    }
    public static ServerPlayerData GetServerPlayerData()
    {
        if (!Player.m_localPlayer) return new ServerPlayerData();
        return new ServerPlayerData()
        {
            player_name = Player.m_localPlayer.GetHoverName(),
            data = new PlayerData()
            {
                completed_achievements = AchievementManager.m_achievements.Values.Count(x => x.m_isCompleted),
                total_kills = (int)GetPlayerStat(PlayerStatType.EnemyKills),
                total_deaths = (int)GetPlayerStat(PlayerStatType.Deaths)
            }
        };
    }
    
    [HarmonyPatch(typeof(RuneStone), nameof(RuneStone.Interact))]
    static class RuneStoneInteractPatch
    {
        private static void Prefix(RuneStone __instance)
        {
            if (!__instance) return;
            AddRuneStoneLabels(__instance);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class PlayerOnDeathPatch
    {
        private static void Prefix(Player __instance)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            if (__instance != Player.m_localPlayer) return;
            UpdatePlayerDeaths(__instance);
            ISerializer serializer = new SerializerBuilder().Build();
            var names = EffectMan.ActiveAchievementEffects.Select(x => x.name).ToList();
            Player.m_localPlayer.m_customData[EffectMan.PlayerEffectKey] = serializer.Serialize(names);
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    private static class CharacterOnDeathPatch
    {
        private static void Prefix(Character __instance)
        {
            if (!__instance) return;
            UpdatePlayerKills(__instance);
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    private static class PlayerSavePatch
    {
        private static void Prefix(Player __instance)
        {
            if (!__instance) return;
            WriteCurrentCustomData();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Start))]
    private static class PlayerLoadCustomData
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance || !Player.m_localPlayer) return;
            if (__instance != Player.m_localPlayer) return;
            LoadPlayerData();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
    private static class PlayerEatFoodFix
    {
        private static void Postfix(ref bool __result)
        {
            if (__result) Game.instance.IncrementPlayerStat(PlayerStatType.FoodEaten);
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class PickableInteractPatch
    {
        private static void Postfix(Pickable __instance, Humanoid character, ref bool __result)
        {
            if (!__instance || !character || !__result) return;
            
            Player? player = character as Player;
            if (player == null) return;

            string itemName = __instance.name.Replace("(Clone)", string.Empty).Replace("Pickable_",string.Empty);
            
            if (LocalPlayerData.Player_Pickable_Data.ContainsKey(itemName))
            {
                ++LocalPlayerData.Player_Pickable_Data[itemName];
            }
            else
            {
                LocalPlayerData.Player_Pickable_Data[itemName] = 1;
            }
        }
    }
    public static bool GetPlayerPickableValue(string key, out int value)
    {
        value = 0;
        if (!LocalPlayerData.Player_Pickable_Data.TryGetValue(key, out int pickableValue)) return false;
        value = pickableValue;
        return true;
    }
    private static void LoadPlayerData()
    {
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.GetHoverName().Replace(" ", "_").IsNullOrWhiteSpace()) return;
        
        InitPlayerTracker();
    }

    public static List<Entries.Entry> GetEntries()
    {
        Entries.EntryBuilder builder = new();
        builder.m_showAll = true;
        if (AlmanacPlugin._AchievementsEnabled.Value is AlmanacPlugin.Toggle.On)
        {
            Dictionary<EffectMan.Modifier, float> modifiers = new();
            HitData.DamageModifiers mods = new HitData.DamageModifiers();
            Dictionary<Skills.SkillType, float> skills = new();
            foreach (Skills.SkillType type in Enum.GetValues(typeof(Skills.SkillType))) skills[type] = 0;
            foreach (EffectMan.Modifier type in Enum.GetValues(typeof(EffectMan.Modifier))) modifiers[type] = 0;
            int count = 0;
            foreach (StatusEffect? statusEffect in Player.m_localPlayer.GetSEMan().GetStatusEffects())
            {
                if (statusEffect is not EffectMan.AchievementEffect achievementEffect) continue;
                ++count;
                foreach (KeyValuePair<EffectMan.Modifier, float> kvp in achievementEffect.m_achievement.m_data.modifiers)
                {
                    modifiers[kvp.Key] += kvp.Value;
                }
                mods.Apply(achievementEffect.m_achievement.m_data.damage_modifiers);
                foreach (var kvp in achievementEffect.m_achievement.m_skillBonus)
                {
                    skills[kvp.Key] += kvp.Value;
                }
            }

            if (count > 0)
            {
                builder.Add("$title_statuseffects");
                builder.Add("$text_total", count);
                foreach (KeyValuePair<EffectMan.Modifier, float> kvp in modifiers)
                {
                    if (kvp.Key == EffectMan.Modifier.None) continue;
                    switch (kvp.Key)
                    {
                        case EffectMan.Modifier.MaxCarryWeight 
                            or EffectMan.Modifier.Health 
                            or EffectMan.Modifier.Stamina 
                            or EffectMan.Modifier.Eitr
                            or EffectMan.Modifier.Armor:
                            builder.Add(Helpers.ConvertEffectModifiers(kvp.Key), kvp.Value, Entries.EntryBuilder.Option.Add);
                            break;
                        default:
                            builder.Add(Helpers.ConvertEffectModifiers(kvp.Key), kvp.Value, Entries.EntryBuilder.Option.Percentage);
                            break;
                    }
                }

                foreach (HitData.DamageType mod in Enum.GetValues(typeof(HitData.DamageType)))
                {
                    var modifier = mods.GetModifier(mod);
                    builder.Add(mod, modifier);
                }

                foreach (var kvp in skills)
                {
                    builder.Add(kvp.Key, kvp.Value);
                }
            }
        }
        builder.Add("$title_resistances");
        foreach (HitData.DamageType mod in Enum.GetValues(typeof(HitData.DamageType)))
        {
            var modifier = Player.m_localPlayer.m_damageModifiers.GetModifier(mod);
            builder.Add(mod, modifier);
        }
        builder.Add("$title_combat");
        builder.Add(PlayerStatType.EnemyKills);
        builder.Add(PlayerStatType.PlayerKills);
        builder.Add(PlayerStatType.HitsTakenEnemies);
        builder.Add(PlayerStatType.Deaths);
        builder.Add(PlayerStatType.EnemyHits);
        builder.Add(PlayerStatType.EnemyKillsLastHits);
        builder.Add(PlayerStatType.BossKills);
        builder.Add(PlayerStatType.BossLastHits);
        builder.Add(PlayerStatType.ArrowsShot);
        builder.Add("$title_forsaken");
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
        builder.Add("$title_mortality");
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
        builder.Add("$title_misc");
        builder.Add(PlayerStatType.Cheats);
        builder.Add(PlayerStatType.WorldLoads);
        builder.Add(PlayerStatType.TombstonesFit);
        builder.Add(PlayerStatType.TombstonesOpenedOther);
        builder.Add(PlayerStatType.TombstonesOpenedOwn);
        builder.Add("$title_husbandry");
        builder.Add(PlayerStatType.SkeletonSummons);
        builder.Add(PlayerStatType.CreatureTamed);
        builder.Add(PlayerStatType.CreatureTamed);
        builder.Add("$title_harvest");
        builder.Add(PlayerStatType.ItemsPickedUp);
        builder.Add(PlayerStatType.BeesHarvested);
        builder.Add(PlayerStatType.SapHarvested);
        builder.Add(PlayerStatType.FoodEaten);
        builder.Add("$title_build");
        builder.Add(PlayerStatType.PlaceStacks);
        builder.Add(PlayerStatType.CraftsOrUpgrades);
        builder.Add(PlayerStatType.Builds);
        builder.Add(PlayerStatType.Upgrades);
        builder.Add(PlayerStatType.Crafts);
        builder.Add("$title_defense");
        builder.Add(PlayerStatType.TurretAmmoAdded);
        builder.Add(PlayerStatType.TurretTrophySet);
        builder.Add(PlayerStatType.TrapArmed);
        builder.Add(PlayerStatType.TrapTriggered);
        builder.Add("$title_dungeon");
        builder.Add(PlayerStatType.PortalDungeonIn);
        builder.Add(PlayerStatType.PortalDungeonOut);
        builder.Add("$title_travel");
        builder.Add(PlayerStatType.DistanceTraveled);
        builder.Add(PlayerStatType.DistanceWalk);
        builder.Add(PlayerStatType.DistanceRun);
        builder.Add(PlayerStatType.DistanceSail);
        builder.Add(PlayerStatType.DistanceAir);
        builder.Add(PlayerStatType.Jumps);
        builder.Add(PlayerStatType.PortalsUsed);
        builder.Add("$title_base");
        builder.Add(PlayerStatType.DoorsOpened);
        builder.Add(PlayerStatType.DoorsClosed);
        builder.Add(PlayerStatType.TimeInBase);
        builder.Add(PlayerStatType.TimeOutOfBase);
        builder.Add(PlayerStatType.Sleep);
        builder.Add(PlayerStatType.ItemStandUses);
        builder.Add(PlayerStatType.ArmorStandUses);
        builder.Add("$title_trees");
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
        builder.Add("$title_mining");
        builder.Add(PlayerStatType.MineHits);
        builder.Add(PlayerStatType.Mines);
        builder.Add(PlayerStatType.MineTier0);
        builder.Add(PlayerStatType.MineTier1);
        builder.Add(PlayerStatType.MineTier2);
        builder.Add(PlayerStatType.MineTier3);
        builder.Add(PlayerStatType.MineTier4);
        builder.Add(PlayerStatType.MineTier5);
        builder.Add("$title_raven");
        builder.Add(PlayerStatType.RavenAppear);
        builder.Add(PlayerStatType.RavenHits);
        builder.Add(PlayerStatType.RavenTalk);
        return builder.ToList();
    }
}

[Serializable]
public class CustomData
{
    public Dictionary<string, KillDeaths> Player_Kill_Deaths = new();
    public Dictionary<string, int> Player_Pickable_Data = new();
}

[Serializable]
public class KillDeaths
{
    public int kills;
    public int deaths;
}

[Serializable]
public class ServerPlayerData
{
    public string player_name = null!;
    public PlayerData data = null!;
}
[Serializable]
public class PlayerData
{
    public int completed_achievements;
    public int total_kills;
    public int total_deaths;
}