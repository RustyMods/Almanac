using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static Almanac.Achievements.AlmanacEffectManager;

namespace Almanac.Achievements;

public static class AchievementManager
{
    public static List<Achievement> AchievementList = new();
    public static readonly List<AchievementYML.AchievementData> AchievementData = new();
    public class Achievement
    {
        public EffectData m_effectData = null!;
        public string m_uniqueName = null!;
        public string m_displayName = null!;
        public int m_goal;
        public AchievementTypes.AchievementType m_type;
        public string? m_desc = null!;
        public Sprite? m_sprite;
        public string? m_spriteName;
        public string m_lore = "";
        public string m_defeatKey = "";
        public bool m_isCompleted;
        public StatusEffect? m_statusEffect;
    }
    
    public static void OnAchievementConfigChanged(object sender, EventArgs e)
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Achievement configs changed, reloading achievements");
        ActiveAchievementEffects.Clear();
        if (ServerSyncedData.InitiatedServerAchievements)
        {
            ServerSyncedData.OnServerAchievementChanged();
        }
        else
        {
            InitAchievements(LoadAchievementData(AchievementData), false);
        }
    }

    public static bool AchievementsRan = false;
    public static void InitAchievements(List<Achievement> list, bool checkBool = true)
    {
        if (checkBool && AchievementsRan) return;
        if (!ObjectDB.instance) return;
        
        if (Player.m_localPlayer)
        {
            List<StatusEffect> EffectsToRemove = new();
            foreach (StatusEffect SE in Player.m_localPlayer.GetSEMan().GetStatusEffects())
            {
                if (SE is AchievementEffect)
                {
                    EffectsToRemove.Add(SE);
                }
            }

            foreach (StatusEffect SE in EffectsToRemove)
            {
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect(SE, true);
            }
        }
        
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is AchievementEffect);
        AchievementList.Clear();

        AchievementList = list;
        if (checkBool) AchievementsRan = true;
    }

    public static void ReadAchievements()
    {
        AchievementData.Clear();
        string[] filePaths = Directory.GetFiles(AlmanacPaths.AchievementFolderPath, "*.yml");

        IDeserializer deserializer = new DeserializerBuilder().Build();
        
        foreach (string path in filePaths)
        {
            try
            {
                string data = File.ReadAllText(path);
                AchievementYML.AchievementData YmlData = deserializer.Deserialize<AchievementYML.AchievementData>(data);
                AchievementData.Add(YmlData);
            }
            catch (Exception)
            {
                AlmanacPlugin.AlmanacLogger.LogWarning("Failed to load yml data: " + path);
            }
        }
    }

    public static List<Achievement> LoadAchievementData(List<AchievementYML.AchievementData> data)
    {
        List<Achievement> output = new();
        if (!ZNet.instance) return output;

        foreach (AchievementYML.AchievementData YmlData in data)
        {
            Achievement achievement = CreateAchievement(
                UniqueName: YmlData.unique_name,
                DisplayName: YmlData.display_name,
                SpriteName: YmlData.sprite_name,
                Desc: YmlData.description,
                Lore: YmlData.lore,
                StartMsg: YmlData.start_message,
                StopMsg: YmlData.stop_message,
                Tooltip: YmlData.tooltip,
                DefeatKey: YmlData.defeat_key,
                Goal: YmlData.goal,
                Type: YmlData.achievement_type,
                Duration: YmlData.duration,
                StartEffects: YmlData.start_effects.ToArray(),
                StopEffects: YmlData.stop_effects.ToArray(),
                DamageMods: YmlData.damage_modifiers.ToArray(),
                Modifiers: YmlData.modifiers
            );
            output.Add(achievement);
        }

        return output;
    }

    private static Achievement CreateAchievement(
        string UniqueName, 
        string DisplayName,
        string SpriteName = "",
        string Desc = "", 
        string Lore = "", 
        string StartMsg = "", 
        string StopMsg = "", 
        string Tooltip = "",
        string DefeatKey = "",
        int Goal = 0,
        Sprite? Sprite = null,
        AchievementTypes.AchievementType Type = AchievementTypes.AchievementType.None,
        int Duration = 0,
        string[]? StartEffects = null,
        string[]? StopEffects = null,
        HitData.DamageModPair[] DamageMods = null!,
        Dictionary<Modifier, float> Modifiers = null!)
    {
        Achievement achievement = new Achievement()
        {
            m_uniqueName = UniqueName,
            m_displayName = DisplayName,
            m_spriteName = SpriteName,
            m_desc = Desc,
            m_lore = Lore,
            m_defeatKey = DefeatKey,
            m_goal = Goal,
            m_sprite = Sprite,
            m_type = Type,
        };
        achievement.m_effectData = new()
        {
            duration = Duration,
            startEffectNames = StartEffects,
            stopEffectNames = StopEffects,
            startMsg = StartMsg,
            stopMsg = StopMsg,
            effectTooltip = Tooltip,
            damageMods = DamageMods.ToList(),
            Modifiers = Modifiers,
            effectName = achievement.m_uniqueName,
            displayName = achievement.m_displayName,
            sprite = achievement.m_sprite
        };;

        if (!achievement.m_spriteName.IsNullOrWhiteSpace())
        {
            if (SpriteManager.GetSprite(achievement.m_spriteName, out Sprite? sprite))
            {
                achievement.m_sprite = sprite;
                achievement.m_effectData.sprite = sprite;
            }
            else
            {
                GameObject? prefab = ObjectDB.instance.GetItemPrefab(achievement.m_spriteName);
                if (prefab)
                {
                    if (prefab.TryGetComponent(out ItemDrop itemDrop))
                    {
                        Sprite? icon = itemDrop.m_itemData.GetIcon();
                        achievement.m_sprite = icon;
                        achievement.m_effectData.sprite = icon;
                    };
                }
            }
        };
        
        achievement.m_statusEffect = achievement.m_effectData.Init();
        return achievement;
    }
    
}