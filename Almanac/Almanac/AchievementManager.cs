using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using UnityEngine.Serialization;
using YamlDotNet.Serialization;
using static Almanac.Almanac.AchievementManager.AchievementType;
using static Almanac.Almanac.AlmanacEffectsManager;
using static Almanac.Almanac.AlmanacEffectsManager.Modifier;
using static Almanac.AlmanacPlugin;
using Object = UnityEngine.Object;

namespace Almanac.Almanac;

public static class AchievementManager
{
    public static readonly CustomSyncedValue<List<string>> serverAchievementData =
        new(AlmanacPlugin.ConfigSync, "ServerAchievements", new()); // To be deserialized
    
    public static readonly List<Achievement> tempAchievements = new(); // To be used to register to achievements UI, ObjectDB effects and Custom Status Effects

    private static List<AchievementData> tempAchievementData = new(); // to be serialized / deserialized

    public static List<string> currentServerData = new();
    public class Achievement
    {
        public BaseEffectData m_statusEffect = null!;
        public string m_uniqueName = null!;
        public string m_displayName = null!;
        public int m_goal;
        public AchievementType m_type;
        public string? m_desc = null!;
        public Sprite? m_sprite;
        public string? m_spriteName;
        public string m_lore = "";
        public string m_toolTip = "";
        public bool isCompleted;
    }

    public enum AchievementType
    {
        None,
        Deaths,
        Fish,
        Materials,
        Consumables,
        Weapons,
        Swords,
        Axes,
        PoleArms,
        Spears,
        Maces,
        Knives,
        Shields,
        Staves,
        Arrows,
        Bows,
        Valuables,
        Potions,
        Trophies,
        Creatures,
        MeadowCreatures,
        BlackForestCreatures,
        SwampCreatures,
        MountainCreatures,
        PlainsCreatures,
        MistLandCreatures,
        AshLandCreatures,
        DeepNorthCreatures,
        EikthyrKills,
        ElderKills,
        BonemassKills,
        ModerKills,
        YagluthKills,
        QueenKills,
        DistanceRan,
        DistanceSailed,
        TotalKills,
        TotalAchievements,
        TrollKills,
        SerpentKills,
        CultistKills,
        StoneGolemKills,
        TarBlobKills,
        DeathByFall,
        TreesChopped,
        DeathByTree,
        DeathByEdgeOfWorld,
        TimeInBase,
        TimeOutOfBase,
        ArrowsShot,
        GoblinShamanKills,
        WraithKills,
        DrakeKills,
        GhostKills,
        FenringKills,
        ComfortPieces,
        GreydwarfShamanKills,
        DvergerSupportKills,
        DvergerFireKills,
        DvergerFrostKills,
        DvergerKills,
        TotalJumps,
        TotalCraftsOrUpgrades,
        TotalBuilds,
        EnemyHits,
        PlayerKills,
        HitsTaken,
        ItemsPicked,
        DistanceWalked,
        DistanceInAir,
        MineHits,
        TotalMined,
        CreatureTamed,
        FoodEaten,
        SkeletonSummoned,
        DeathByDrowning,
        DeathByBurning,
        DeathByFreezing,
        DeathByPoisoned,
        DeathBySmoke,
        DeathByWater,
        DeathByCart,
        DeathBySelf,
        DeathByStalagtite,
        BeesHarvested,
        SapHarvested,
        TrapsArmed,
        StacksPlaced,
        BossKills,
        
    }

    private static void CreateAchievement(
        string uniqueName,
        string name, 
        AchievementType type,
        Modifier modifier,
        Dictionary<Modifier, float> modifiers,
        string[]? startEffects,
        string[]? stopEffects,
        int goal = 0,
        Sprite? sprite = null,
        string damageModifier = "",
        string desc = "",
        string spriteName = "",
        string lore = "",
        string toolTip = "",
        string statusEffectStopMsg = "",
        string activationAnimation = "gpower",
        float newValue = 0f,
        int statusEffectDuration = 0
    )
    {
        if (type is AchievementType.None)
        {
            AlmanacLogger.LogWarning($"[{uniqueName}] : Failed to recognize achievement type");
            return;
        }
        if (tempAchievements.Exists(x => x.m_uniqueName == uniqueName)) return;

        string statusEffectName = "se_" + uniqueName;
        switch (modifier)
        {
            case EikthyrPower: statusEffectName = "GP_Eikthyr"; break;
            case ElderPower: statusEffectName = "GP_TheElder"; break;
            case BonemassPower: statusEffectName = "GP_Bonemass"; break;
            case ModerPower: statusEffectName = "GP_Moder"; break;
            case YagluthPower: statusEffectName = "GP_Yagluth"; break;
            case QueenPower: statusEffectName = "GP_Queen"; break;
        }

        if (statusEffectDuration > 0) statusEffectName = "GP_" + uniqueName;
        
        Achievement data = new Achievement()
        {
            m_statusEffect = new BaseEffectData()
            {
                effectName = statusEffectName,
                displayName = name,
                duration = statusEffectDuration,
                sprite = sprite,
                spriteName = spriteName,
                startEffectNames = startEffects,
                stopEffectNames = stopEffects,
                startMsg = toolTip,
                stopMsg = statusEffectStopMsg,
                effectTooltip = toolTip,
                damageMod = damageModifier,
                Modifier = modifier,
                Modifiers = modifiers,
                m_newValue = newValue,
                activationAnimation = activationAnimation
            },
            m_uniqueName = uniqueName,
            m_displayName = name,
            m_desc = desc,
            m_goal = goal,
            m_sprite = sprite,
            m_spriteName = spriteName,
            m_lore = lore,
            m_toolTip = toolTip,
            m_type = type
        };
        
        tempAchievements.Add(data);
    }

    private static readonly string folderName = "Almanac";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    public static readonly string achievementPath = folderPath + Path.DirectorySeparatorChar + "AchievementData";

    private static readonly string achievementTutorialPath = folderPath + Path.DirectorySeparatorChar + "Achievements_README.md";


    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    static class GameLogoutPatch
    {
        private static void Postfix()
        {
            serverAchievementData.Value.Clear();
            currentServerData.Clear();
        }
    }

    private static void WriteTutorial()
    {
        if (File.Exists(achievementTutorialPath)) return;
        List<string> tutorial = new List<string>()
        {
            "# Almanac Achievements",
            "Users can customize and create their own achievements by manipulating the files within this folder.",
            "`If almanac recognizes a folder named: 'AchievementData' in config/Almanac folder, then it will use the files within",
            "as data for achievements.`",
            "## Features",
            "- Server shares achievements data to peers",
            "- Create custom passive or active effects",
            "- Create achievements for players to reach",
            "- Use in-game assets to customize your achievements",
            "## Unique Name",
            "Make sure each achievement has a unique name or else it will be ignored - no white spaces, use underscore",
            "## Active vs Passive",
            "If cooldown is set to anything higher than 0, power is set as guardian power. Effects only applied while active, and countdown is running.",
            "## Modifiers",
            "```","Modifier key : description","```",
            "```yml",
            "Attack : Damage multiplier",
            "HealthRegen : Health regeneration multiplier",
            "StaminaRegen : Stamina regeneration multiplier",
            "RaiseSkills : Experience multiplier",
            "Speed : Speed multiplier",
            "Noise : Noise multiplier",
            "MaxCarryWeight : Additive carry weight",
            "Stealth : Stealth multiplier",
            "RunStaminaDrain : Run stamina drain multiplier",
            "DamageReduction : Damage reduction multiplier",
            "FallDamage : Fall damage multiplier",
            "BaseHP : Additive base health",
            "BaseStamina : Addive base stamina",
            "MeleeDMG : Additive melee damage",
            "RangedDMG : Additive ranged damage",
            "FireDMG : Additive fire damage",
            "FrostDMG : Additive frost damage",
            "LightningDMG : Additive lightning damage",
            "PoisonDMG : Additive poison damage",
            "SpiritDMG : Additive spirit damage",
            "ChopDMG : Additive chop damage",
            "PickaxedDMG : Additive pickaxe damage",
            "BluntDMG : Additive blunt damage",
            "PierceDMG : Additive pierce damage",
            "SlashDMG : Additive slash damage",
            "EikthyrPower : On click, switches forsaken power to eikthyr",
            "ElderPower : On click, switches forsaken power to elder",
            "BonemassPower : On click, switches forsaken power to bonemass",
            "ModerPower : On click, switches forsaken power to moder",
            "YagluthPower : On click, switches forsaken power to yagluth",
            "QueenPower : On click, switches forsaken power to seeker queen",
            "```",
            "## Sprite Name",
            "You can use any loaded prefabs (ex:HardAntler) with icons or you can choose from almanac's custom icons:",
            "```","Icon key : description","```",
            "```yml",
            "almanac_bone_skull : white bone skull",
            "almanac_sword_blue : basic sword with a blue hilt",
            "almanac_sword_brown : basic sword with a brown hilt",
            "almanac_arrow : basic arrow",
            "almanac_cape_hood : hooded cape",
            "almanac_bottle_empty : empty bottle",
            "almanac_bottle_blue : bottle with blue fluid",
            "almanac_fish_green : green fish",
            "almanac_bow : basic bow",
            "almanac_necklace : silver necklace with red gem",
            "almanac_mushroom : red mushroom",
            "almanac_gold_coins : stack of gold coins",
            "almanac_key_silver : silver key",
            "almanac_bone_white : white femur bone",
            "almanac_book_red : red book",
            "almanac_bottle_green : bottle with green fluid",
            "almanac_crown_gold : golden crown",
            "almanac_gem_red : red gem",
            "almanac_gold_bars : stack of gold bars",
            "almanac_silver_coins : stack of silver coins",
            "almanac_wood_log : one wood log",
            "almanac_wood_stack : stack of wood logs",
            "```",
            "## Available Trackers - Achievement Type",
            "List of currently available types of trackers to use:",
            "```","achievement type key : description","```",
            "```yml",
            "Fish : tracks total currently known fish - goal irrelevant",
            "Materials : tracks total currently known materials - goal irrelevant",
            "Consumables : tracks total currently known consumables - goal irrelevant",
            "Weapons : tracks total currently known weapons - goal irrelevant",
            "Swords : tracks total currently known swords - goal irrelevant",
            "Axes : tracks total currently known axes - goal irrelevant",
            "Polearms : tracks total currently known atgeirs - goal irrelevant",
            "Spears : tracks total currently known spears - goal irrelevant",
            "Maces : tracks total currently known maces - goal irrelevant",
            "Knives : tracks total currently known knives - goal irrelevant",
            "Shields : tracks total currently known shields - goal irrelevant",
            "Staves : tracks total currently known staves - goal irrelevant",
            "Arrows : tracks total currently known arrows - goal irrelevant",
            "Bows : tracks total currently known bows - goal irrelevant",
            "Valuables : tracks total currently known sellable items - goal irrelevant",
            "Potions : tracks total currently known mead bases - goal irrelevant",
            "Trophies : tracks total currently known trophies - goal irrelevant",
            "Creatures : tracks total currently known creatures - goal irrelevant",
            "MeadowCreatures : tracks total currently known meadow creatures - goal irrelevant",
            "BlackForestCreatures : tracks total currently known black forest creatures - goal irrelevant",
            "SwampCreatures : tracks total currently known swamp creatures - goal irrelevant",
            "MountainCreatures : tracks total currently known mountain creatures - goal irrelevant",
            "PlainsCreatures : tracks total currently known plains creatures - goal irrelevant",
            "MistLandCreatures : tracks total currently known mistlands creatures - goal irrelevant",
            "EikthyrKills : tracks total eikthyr kills - use goal to set up threshold",
            "ElderKills : tracks total elder kills - use goal to set up threshold",
            "BonemassKills : tracks total bonemass kills - use goal to set up threshold",
            "ModerKills : tracks total moder kills - use goal to set up threshold",
            "YagluthKills : tracks total yagluth kills - use goal to set up threshold",
            "QueenKills : tracks total seeker queen kills - use goal to set up threshold",
            "Deaths : tracks total player deaths - use goal to set up threshold",
            "DistanceRan : tracks total distance ran - use goal to set up threshold",
            "DistanceSailed : tracks total distance sailed - use goal to set up threshold",
            "TotalKills : tracks total enemy kills - use goal to set up threshold",
            "TotalAchievements : tracks total achievements - use goal to set up threshold",
            "TrollKills : tracks total troll kills - use goal to set up threshold",
            "SerpentKills : tracks total serpent kills - use goal to set up threshold",
            "CultistKills : tracks total fenring cultists kills - use goal to set up threshold",
            "StoneGolemKills : tracks total stone golem kills - use goal to set up threshold",
            "TarBlobKills : tracks total tar blob kills - use goal to set up threshold",
            "DeathByFall : tracks total deaths by falling - use goal to set up threshold",
            "Trees : tracks total trees chopped - use goal to set up threshold",
            "DeathByTree : tracks total death by trees - use goal to set up threshold",
            "DeathByEdge : tracks total death by falling off edge of world - use goal to set up threshold",
            "TimeInBase : tracks total time in base - use goal to set up threshold",
            "TimeOutOfBase : tracks total time out of base - use goal to set up threshold",
            "ArrowsShot : tracks total arrows shot - use goal to set up threshold",
            "GoblinShamanKills : tracks total goblin shaman kills - use goal to set up threshold",
            "DrakeKills : tracks total drake kills - use goal to set up threshold",
            "WraithKills : tracks total wraith kills - use goal to set up threshold",
            "ComfortPieces : tracks total known comfort pieces - goal irrelevant",
            "GreydwarfShamanKills : tracks total greydwarf shaman kills - use goal to set up threshold",
            "DvergerKills : tracks total dverger kills - use goal to set up threshold",
            "DvergerFireKills : tracks total dverger fire mage kills - use goal to set up threshold",
            "DvergerFrostKills : tracks total dverger frost mage kills - use goal to set up threshold",
            "DvergerSupportKills : tracks total dverger support mage kills - use goal to set up threshold",
            "TotalJumps : tracks total jumps - use goal to set up threshold",
            "TotalCraftsOrUpgrades : tracks total crafts and upgrades of items - use goal to set up threshold",
            "TotalBuilds : tracks total built pieces - use goal to set up threshold",
            "EnemyHits : tracks total hits against creatures - use goal to set up threshold",
            "PlayerKills : tracks total player kills - pvp - - use goal to set up threshold",
            "HitsTaken : tracks total hits taken - use goal to set up threshold",
            "ItemsPicked : tracks total items picked up - use goal to set up threshold",
            "DistanceWalked : tracks total distance walked - use goal to set up threshold",
            "DistanceInAir : tracks total air time - use goal to set up threshold",
            "MineHits : tracks total pickaxe mine hits - use goal to set up threshold",
            "CreatureTamed : tracks total successful tame - use goal to set up threshold",
            "FoodEaten : tracks total food eaten - use goal to set up threshold",
            "SkeletonSummoned : tracks total f riendly skeleton summoned - use goal to set up threshold",
            "DeathByDrowning : tracks total death by drowning - use goal to set up threshold",
            "DeathByBurning : tracks total death by drowning - use goal to set up threshold",
            "DeathByFreezing : tracks total death by freezing - use goal to set up threshold",
            "DeathByPoisoned : tracks total death by poisoned - use goal to set up threshold",
            "DeathBySmoke : tracks total death by smoke - use goal to set up threshold",
            "DeathByWater : tracks total death by water - use goal to set up threshold",
            "DeathByCart : tracks total death by cart - use goal to set up threshold",
            "DeathBySelf : tracks total suicides - use goal to set up threshold",
            "DeathByStalagtite : tracks total death by stalagtite - use goal to set up threshold",
            "BeesHarvested : tracks total bees harvested - use goal to set up threshold",
            "SapHarvested : tracks total sap harvested - use goal to set up threshold",
            "TrapsArmed : tracks total traps armed - use goal to set up threshold",
            "StacksPlaced : tracks total stacks placed - use goal to set up threshold",
            "BossKills : tracks total boss kills - use goal to set up threshold",
            "```",
            "## Resistance Modifier",
            "This can be used to apply damage modifiers `on the player`,",
            "It can be read as list if seperated by commas.",
            "(ex: Fire = Weak, Frost = Resistant)",
            "This affects any damages from status effects such as `burning` but not direct damages.",
            "### Acceptable resistances:",
            "```yml",
            "Physical",
            "Elemental",
            "Fire",
            "Frost",
            "Lightning",
            "Poison",
            "Spirit",
            "```",
            "### Acceptable modifier:",
            "```yml",
            "VeryWeak",
            "Weak",
            "Normal",
            "Resistant",
            "VeryResistant",
            "```",
            "## Activation animation [NOT WORKING]",
            "Only applies to active powers that use the 'F' key to engage forsaken power (aka guardian power)",
            "Work in progress, need to figure out how to make custom animations invoke the guardian power",
            "Currently all set to default of 'gpower'",
            "## Extra",
            "Please come find me on OdinPlus discord if you have any ideas on how to improve the system",
            "Best, Rusty"
        };
            
        File.WriteAllLines(achievementTutorialPath, tutorial);
    }
    public static void SaveAchievementData()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        
        if (!Directory.Exists(achievementPath)) Directory.CreateDirectory(achievementPath);

        WriteTutorial();
        
        foreach (var data in tempAchievementData)
        {
            string serializedData = serializer.Serialize(data);
            
            File.WriteAllText(
                (achievementPath + Path.DirectorySeparatorChar + $"{data.unique_name}.yml"),
                serializedData
            );
        }
    }

    public static void ResetAchievementData()
    {
        ISerializer serializer = new SerializerBuilder().Build();

        if (!Directory.Exists(achievementPath)) Directory.CreateDirectory(achievementPath);

        List<AchievementData> defaultData = CreateDefaultAchievements();

        foreach (AchievementData? data in defaultData)
        {
            string serializedData = serializer.Serialize(data);
            
            File.WriteAllText(
                (achievementPath + Path.DirectorySeparatorChar + $"{data.unique_name}.yml"),
                serializedData
            );
        }
    }

    public static void ReBuildAchievements()
    {
        if (!Player.m_localPlayer) return;
        // Remove all currently active Base Effects
        foreach (var effect in CustomStatusEffects.activeAlmanacEffects)
        {
            Player.m_localPlayer.GetSEMan().RemoveStatusEffect(effect);
        }
        CustomStatusEffects.activeAlmanacEffects.Clear();
        
        AlmanacLogger.LogInfo("Rebuilding achievements");
        if (serverAchievementData.Value.Count == 0)
        {
            AlmanacLogger.LogInfo($"Found no server data, loading local data");
            InitAchievements(); // Read files
            RegisterAlmanacEffects.AddStatusEffectsToObjectDB(); // Remove BaseEffects from ObjectDB and Instantiate new status effects
            AchievementsUI.RegisterAchievements(); // Setup UI
        }
        else
        {
            AlmanacLogger.LogInfo($"Loading {serverAchievementData.Value.Count} achievements");
            IDeserializer deserializer = new DeserializerBuilder().Build();
            List<AchievementData> serverData = new();
            foreach (string? serializedData in serverAchievementData.Value)
            {
                AchievementData data = deserializer.Deserialize<AchievementData>(serializedData);
                serverData.Add(data);
            }
            AddAchievementToRegistrar(serverData); // Build server achievements
            RegisterAlmanacEffects.AddStatusEffectsToObjectDB();
            AchievementsUI.RegisterAchievements();
            currentServerData = new List<string>(serverAchievementData.Value);
        }
        // If new or deleted achievement data
        Object.DestroyImmediate(Almanac.CreateAlmanac.achievementsPanel, true); // Destroy old achievements panel
        // Re-build achievement panel
        GameObject panel = Almanac.CreateAlmanac.CreateAchievementsPanel("achievements", AchievementsUI.registeredAchievements);
        Almanac.CreateAlmanac.achievementsPanel = panel;
    }

    public static void InitSyncedAchievementData()
    {
        if (WorkingAsType is WorkingAs.Server)
        {
            if (!Directory.Exists(achievementPath))
            {
                // If no achievement data, write defaults
                AlmanacLogger.LogInfo($"Writing default achievements to file");
                ResetAchievementData();
            } 
            WriteTutorial(); // If no tutorial, write tutorial

            AlmanacLogger.LogInfo("Initializing achievements");
            InitAchievements();
        }
        else
        {
            InitAchievements();
        }
    }

    public static void InitAchievements()
    {
        tempAchievementData.Clear();
        serverAchievementData.Value.Clear();
        
        if (Directory.Exists(achievementPath))
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            string[] filePaths = Directory.GetFiles(achievementPath, "*.yml");

            List<AchievementData> newData = new();
            List<string> newServerData = new();
            foreach (string filePath in filePaths)
            {
                try
                {
                    string serializedData = File.ReadAllText(filePath);
                    AchievementData data = deserializer.Deserialize<AchievementData>(serializedData);
                    if (newData.Exists(x => x.unique_name == data.unique_name)) continue;
                    newData.Add(data);
                    newServerData.Add(serializedData);
                }
                catch (Exception ex) { AlmanacLogger.LogWarning($"Error reading file '{filePath}': {ex.Message}"); }
            }
            
            if (ZNet.instance.IsServer()) AlmanacLogger.LogInfo($"Loading {newData.Count} local achievements" );
            serverAchievementData.Value = newServerData; 
            tempAchievementData = newData;
        }
        else { tempAchievementData = CreateDefaultAchievements(); } // If achievement data folder does not exist, use default data
        
        AddAchievementToRegistrar(tempAchievementData); // Add read files or data to registry to be added to temp achievements
    }

    private static void AddAchievementToRegistrar(List<AchievementData> deserializedData)
    {
        tempAchievements.Clear();
        
        foreach (AchievementData achievement in deserializedData)
        {
            Modifier mod = Modifier.None;
            Dictionary<Modifier, float> modifiers = new()
            {
                { Modifier.Attack, 1f },
                { HealthRegen, 1f },
                { StaminaRegen, 1f },
                { RaiseSkills, 1f },
                { Speed, 1f },
                { Noise, 1f },
                { MaxCarryWeight, 0f },
                { Stealth, 1f },
                { RunStaminaDrain, 1f },
                { DamageReduction, 0f },
                { FallDamage, 1f }
            };
            Sprite? customSprite = null;
            switch (achievement.modifier)
            {
                case "Attack": mod = Modifier.Attack; modifiers[Modifier.Attack] = achievement.modifier_value; break;
                case "HealthRegen": mod = HealthRegen; modifiers[HealthRegen] = achievement.modifier_value; break;
                case "StaminaRegen": mod = StaminaRegen; modifiers[StaminaRegen] = achievement.modifier_value; break;
                case "RaiseSkills": mod = RaiseSkills; modifiers[RaiseSkills] = achievement.modifier_value; break;
                case "Speed": mod = Speed; modifiers[Speed] = achievement.modifier_value; break;
                case "Noise": mod = Noise; modifiers[Noise] = achievement.modifier_value; break;
                case "MaxCarryWeight": mod = MaxCarryWeight; modifiers[MaxCarryWeight] = achievement.modifier_value; break;
                case "Stealth": mod = Stealth; modifiers[Stealth] = achievement.modifier_value; break;
                case "RunStaminaDrain": mod = RunStaminaDrain; modifiers[RunStaminaDrain] = achievement.modifier_value; break;
                case "DamageReduction": mod = DamageReduction; modifiers[DamageReduction] = achievement.modifier_value; break;
                case "FallDamage": mod = FallDamage; modifiers[FallDamage] = achievement.modifier_value; break;
                case "BaseHP": mod = BaseHP; break;
                case "BaseStamina": mod = BaseStamina; break;
                case "BaseEitr": mod = BaseEitr; break;
                case "MeleeDMG": mod = MeleeDMG; break;
                case "RangedDMG": mod = RangedDMG; break;
                case "FireDMG": mod = FireDMG; break;
                case "FrostDMG": mod = FrostDMG; break;
                case "LightningDMG": mod = LightningDMG; break;
                case "PoisonDMG": mod = PoisonDMG; break;
                case "SpiritDMG": mod = SpiritDMG; break;
                case "ChopDMG" : mod = ChopDMG; break;
                case "PickaxeDMG" : mod = PickaxeDMG; break;
                case "BluntDMG": mod = BluntDMG; break;
                case "PierceDMG": mod = PierceDMG; break;
                case "SlashDMG": mod = SlashDMG; break;
                case "EikthyrPower": mod = EikthyrPower; break;
                case "ElderPower": mod = ElderPower; break;
                case "BonemassPower": mod = BonemassPower; break;
                case "ModerPower": mod = ModerPower; break;
                case "YagluthPower": mod = YagluthPower; break;
                case "QueenPower": mod = QueenPower; break;
            }

            switch (achievement.sprite_name)
            {
                case "almanac_bone_skull": customSprite = boneSkullIcon; break;
                case "almanac_sword_blue": customSprite = swordBasicBlueIcon; break;
                case "almanac_sword_brown": customSprite = swordBasicBrownIcon; break;
                case "almanac_arrow": customSprite = arrowBasicIcon; break;
                case "almanac_cape_hood": customSprite = capeHoodIcon; break;
                case "almanac_bottle_empty": customSprite = bottleStandardEmptyIcon; break;
                case "almanac_bottle_blue": customSprite = bottleStandardBlueIcon; break;
                case "almanac_fish_green": customSprite = fishGreenIcon; break;
                case "almanac_bow": customSprite = bowWoodIcon; break;
                case "almanac_necklace": customSprite = necklaceSilverRed; break;
                case "almanac_mushroom": customSprite = mushroomBigRedIcon; break;
                case "almanac_gold_coins": customSprite = goldCoinsPileIcon; break;
                case "almanac_key_silver": customSprite = keySilverIcon; break;
                case "almanac_bone_white": customSprite = boneWhiteIcon; break;
                case "almanac_book_red": customSprite = bookClosedRedIcon; break;
                case "almanac_bottle_green": customSprite = bottleStandardGreenIcon; break;
                case "almanac_crown_gold": customSprite = crownGoldIcon; break;
                case "almanac_gem_red": customSprite = gemDiamondRedIcon; break;
                case "almanac_gold_bars": customSprite = goldBarsIcon; break;
                case "almanac_scroll_map": customSprite = scrollMapIcon; break;
                case "almanac_shield": customSprite = shieldBasicIcon; break;
                case "almanac_silver_bars": customSprite = silverBarsIcon; break;
                case "almanac_silver_coins": customSprite = silverCoinsIcon; break;
                case "almanac_wood_log": customSprite = woodLogIcon; break;
                case "almanac_wood_stack": customSprite = woodLogsIcon; break;
            }

            CreateAchievement(
                uniqueName: achievement.unique_name,
                name: achievement.display_name,
                type: GetAchievementType(achievement.achievement_type),
                modifier: mod,
                modifiers: modifiers,
                startEffects: achievement.start_effects,
                stopEffects: achievement.stop_effects,
                goal: achievement.goal,
                sprite: customSprite,
                spriteName: achievement.sprite_name,
                lore: achievement.lore,
                toolTip: achievement.tool_tip,
                desc: achievement.description,
                damageModifier: achievement.resistance_modifier,
                statusEffectStopMsg: achievement.stop_message,
                newValue: achievement.modifier_value,
                activationAnimation: achievement.activation_animation,
                statusEffectDuration: achievement.cooldown
            );
        }
    }

    private static AchievementType GetAchievementType(string modifier)
    {
        switch (modifier)
        {
            case "Fish": return AchievementType.Fish;
            case "Materials": return Materials;
            case "Consumables": return Consumables;
            case "Weapons": return Weapons;
            case "Swords": return Swords;
            case "Axes": return Axes;
            case "Polearms": return PoleArms;
            case "Spears": return Spears;
            case "Maces": return Maces;
            case "Knives": return Knives;
            case "Shields": return Shields;
            case "Staves": return Staves;
            case "Arrows": return Arrows;
            case "Bows": return Bows;
            case "Valuables": return Valuables;
            case "Potions": return Potions;
            case "Trophies": return Trophies;
            case "Creatures": return Creatures;
            case "MeadowCreatures": return MeadowCreatures;
            case "BlackForestCreatures": return BlackForestCreatures;
            case "SwampCreatures": return SwampCreatures;
            case "MountainCreatures": return MountainCreatures;
            case "PlainsCreatures": return PlainsCreatures;
            case "MistLandCreatures": return MistLandCreatures;
            case "AshLandCreatures": return AshLandCreatures;
            case "DeepNorthCreatures": return DeepNorthCreatures;
            case "EikthyrKills": return EikthyrKills;
            case "ElderKills": return ElderKills;
            case "BonemassKills": return BonemassKills;
            case "ModerKills": return ModerKills;
            case "YagluthKills": return YagluthKills;
            case "QueenKills": return QueenKills;
            case "Deaths": return Deaths;
            case "DistanceRan": return DistanceRan;
            case "DistanceSailed": return DistanceSailed;
            case "TotalKills": return TotalKills;
            case "TotalAchievements": return TotalAchievements;
            case "TrollKills": return TrollKills;
            case "SerpentKills": return SerpentKills;
            case "CultistKills": return CultistKills;
            case "StoneGolemKills": return StoneGolemKills;
            case "TarBlobKills": return TarBlobKills;
            case "DeathByFall": return DeathByFall;
            case "Trees": return TreesChopped;
            case "DeathByTree": return DeathByTree;
            case "DeathByEdge": return DeathByEdgeOfWorld;
            case "TimeInBase": return TimeInBase;
            case "TimeOutOfBase": return TimeOutOfBase;
            case "ArrowsShot": return ArrowsShot;
            case "GoblinShamanKills": return GoblinShamanKills;
            case "DrakeKills": return DrakeKills;
            case "GhostKills": return GhostKills;
            case "FenringKills": return FenringKills;
            case "WraithKills": return WraithKills;
            case "ComfortPieces": return ComfortPieces;
            case "GreydwarfShamanKills": return GreydwarfShamanKills;
            case "DvergerKills": return DvergerKills;
            case "DvergerSupportKills": return DvergerSupportKills;
            case "DvergerFireKills": return DvergerFireKills;
            case "DvergerFrostKills": return DvergerFrostKills;
            case "TotalJumps": return TotalJumps;
            case "TotalCraftsOrUpgrades": return TotalCraftsOrUpgrades;
            case "TotalBuilds": return TotalBuilds;
            case "EnemyHits": return EnemyHits;
            case "PlayerKills": return PlayerKills;
            case "HitsTaken": return HitsTaken;
            case "ItemsPicked": return ItemsPicked;
            case "DistanceWalked": return DistanceWalked;
            case "DistanceInAir": return DistanceInAir;
            case "MineHits": return MineHits;
            case "TotalMined": return TotalMined;
            case "CreatureTamed": return CreatureTamed;
            case "FoodEaten": return FoodEaten;
            case "SkeletonSummoned": return SkeletonSummoned;
            case "DeathByDrowning": return DeathByDrowning;
            case "DeathByBurning": return DeathByBurning;
            case "DeathByFreezing": return DeathByFreezing;
            case "DeathByPoisoned": return DeathByPoisoned;
            case "DeathBySmoke": return DeathBySmoke;
            case "DeathByWater": return DeathByWater;
            case "DeathByCart": return DeathByCart;
            case "DeathBySelf": return DeathBySelf;
            case "DeathByStalagtite": return DeathByStalagtite;
            case "BeesHarvested": return BeesHarvested;
            case "SapHarvested": return SapHarvested;
            case "TrapsArmed": return TrapsArmed;
            case "StacksPlaced": return StacksPlaced;
            case "BossKills": return BossKills;
            default: return AchievementType.None;
        }
    }

    private static List<AchievementData> CreateDefaultAchievements()
    {
        List<AchievementData> almanacAchievements = new()
        {
            new AchievementData()
            {
                unique_name = "meadow_kill",
                display_name = "$almanac_achievement_meadow_kill",
                description = "$almanac_achievement_meadow_kill_desc",
                sprite_name = "HardAntler",
                lore = "$almanac_achievement_meadow_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "MeadowCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                resistance_modifier = "",
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "blackforest_kill",
                display_name = "$almanac_achievement_blackforest_kill",
                goal = 0,
                description = "$almanac_achievement_blackforest_kill_desc",
                sprite_name = "almanac_bone_white",
                lore = "$almanac_achievement_blackforest_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "BlackForestCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "swamp_kill",
                display_name = "$almanac_achievement_swamp_kill",
                description = "$almanac_achievement_swamp_kill_desc",
                sprite_name = "TrophyAbomination",
                lore = "$almanac_achievement_swamp_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "SwampCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "mountain_kill",
                display_name = "$almanac_achievement_mountain_kill",
                description = "$almanac_achievement_mountain_kill_desc",
                sprite_name = "DragonTear",
                lore = "$almanac_achievement_mountain_kill_lore",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "MountainCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "plains_kill",
                display_name = "$almanac_achievement_plains_kill",
                description = "$almanac_achievement_plains_kill_desc",
                lore = "$almanac_achievement_plains_kill_lore",
                sprite_name = "Barley",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "PlainsCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "mistlands_kill",
                display_name = "$almanac_achievement_mistlands_kill",
                description = "$almanac_achievement_mistlands_kill_desc",
                lore = "$almanac_achievement_mistlands_kill_lore",
                sprite_name = "MushroomMagecap",
                tool_tip = "$almanac_increase_health_by <color=orange>5</color>",
                achievement_type = "MistLandCreatures",
                stop_message = "$almanac_health_default",
                start_effects = new []{"fx_DvergerMage_Support_start"},
                stop_effects = new []{"fx_DvergerMage_Mistile_die"},
                modifier = "BaseHP",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "ranger",
                display_name = "$almanac_achievement_ranger",
                description = "$almanac_achievement_ranger_desc",
                sprite_name = "almanac_arrow",
                lore = "$almanac_ranger_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_projectile_damage_by <color=orange>5</color>",
                achievement_type = "Arrows",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_projectile_default",
                modifier = "RangedDMG",
                resistance_modifier = "Fire = Weak",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "brew_master",
                display_name = "$almanac_achievement_brew_master",
                description = "$almanac_achievement_brew_master_desc",
                sprite_name = "almanac_bottle_blue",
                lore = "$almanac_brew_master_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_frost</color>\n$almanac_increase_fire_damage_by <color=orange>5</color>",
                achievement_type = "Potions",
                resistance_modifier = "Frost = Weak",
                stop_message = "$almanac_damage_default",
                start_effects = new []{"sfx_coins_placed"},
                modifier = "FireDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "fisher",
                display_name = "$almanac_achievement_fisher",
                description = "$almanac_achievement_fisher_desc",
                sprite_name = "almanac_fish_green",
                lore = "$almanac_fisher_lore",
                tool_tip = "$almanac_allows_moder_power",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Fish",
                modifier = "ModerPower"
            },
            new AchievementData()
            {
                unique_name = "knowledge",
                display_name = "$almanac_achievement_knowledgeable",
                description = "$almanac_achievement_knowledgeable_desc",
                sprite_name = "almanac_necklace",
                lore = "$almanac_knowledgeable_lore",
                tool_tip = "$almanac_increase_carry_weight_by <color=orange>100</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "Materials",
                modifier = "MaxCarryWeight",
                modifier_value = 100f
            },
            new AchievementData()
            {
                unique_name = "master_archer",
                display_name = "$almanac_achievement_master_archer",
                description = "$almanac_achievement_master_archer_desc",
                sprite_name = "almanac_bow",
                lore = "$almanac_master_archer_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_projectile_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Bows",
                resistance_modifier = "Poison = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "RangedDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "gluttony",
                display_name = "$almanac_achievement_gluttony",
                description = "$almanac_achievement_gluttony_desc",
                sprite_name = "almanac_mushroom",
                stop_message = "$almanac_removed_achievement_power",
                lore = "$almanac_gluttony_lore",
                start_effects = new []{"sfx_coins_placed"},
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_very_resistant</color> VS <color=orange>$almanac_poison</color>\n$almanac_reduce_health_by <color=orange>5</color>",
                achievement_type = "Consumables",
                resistance_modifier = "Poison = VeryResistant",
                modifier = "BaseHP",
                modifier_value = -5f
            },
            new AchievementData()
            {
                unique_name = "stag_slayer",
                display_name = "$almanac_achievement_stag_slayer",
                description = "$almanac_achievement_stag_slayer_desc",
                goal = 100,
                sprite_name = "TrophyEikthyr",
                stop_message = "$almanac_removed_achievement_power",
                lore = "$almanac_stag_slayer_lore",
                tool_tip = "$almanac_allows_eikthyr_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                modifier = "EikthyrPower"
            },
            new AchievementData()
            {
                unique_name = "undying",
                display_name = "$almanac_achievement_undying",
                description = "$almanac_achievement_undying_desc",
                goal = 200,
                sprite_name = "almanac_bone_skull",
                lore = "$almanac_undying_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_stamina_by <color=orange>10</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "Deaths",
                modifier = "BaseStamina",
                resistance_modifier = "Spirit = Weak",
                modifier_value = 10f
            },
            new AchievementData()
            {
                unique_name = "completion",
                display_name = "$almanac_achievement_completion",
                description = "$almanac_achievement_completion_desc",
                sprite_name = "almanac_crown_gold",
                lore = "$almanac_completion_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_resistant</color> VS <color=orange>$almanac_frost</color>\n$almanac_raise_skill_experience_by <color=orange>100</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalAchievements",
                resistance_modifier = "Frost = Resistant",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "RaiseSkills",
                modifier_value = 2f
            },
            new AchievementData()
            {
                unique_name = "runner",
                display_name = "$almanac_achievement_runner",
                description = "$almanac_achievement_runner_desc",
                sprite_name = "almanac_scroll_map",
                lore = "$almanac_runner_lore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_pierce</color>\n$almanac_reduce_stamina_drain_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DistanceRan",
                resistance_modifier = "Pierce = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "RunStaminaDrain",
                modifier_value = 0.5f,
                goal = 999999
            },
            new AchievementData()
            {
                unique_name = "killer",
                display_name = "$almanac_achievement_killer",
                description = "$almanac_achievement_killer_desc",
                lore = "$almanac_killer_lore",
                sprite_name = "Acorn",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_attack_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalKills",
                modifier = "Attack",
                resistance_modifier = "Fire = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier_value = 1.5f,
                goal = 999999
            },
            new AchievementData()
            {
                unique_name = "sailor",
                display_name = "$almanac_achievement_sailor",
                description = "$almanac_achievement_sailor_desc",
                lore = "$almanac_sailor_lore",
                sprite_name = "SpearChitin",
                tool_tip = "$almanac_allows_moder_power",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "DistanceSailed",
                modifier = "ModerPower",
                goal = 999999
            },
            new AchievementData()
            {
                unique_name = "break_a_leg",
                display_name = "$almanac_achievement_break_a_leg",
                description = "$almanac_achievement_break_a_leg_desc",
                lore = "$almanac_break_a_leg_lore",
                sprite_name = "BoneFragments",
                tool_tip = "$almanac_reduce_fall_damage_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DeathByFall",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "FallDamage",
                modifier_value = 0.9f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "troll",
                display_name = "$almanac_achievement_troll",
                description = "$almanac_achievement_troll_desc",
                lore = "$almanac_troll_lore",
                sprite_name = "TrophyForestTroll",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_blunt</color>\n$almanac_increase_health_regen_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TrollKills",
                modifier = "HealthRegen",
                resistance_modifier = "Blunt = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier_value = 1.1f,
                goal = 200,
            },
            new AchievementData()
            {
                unique_name = "tarred",
                display_name = "$almanac_achievement_tarred",
                description = "$almanac_achievement_tarred_desc",
                lore = "$almanac_tarred_lore",
                sprite_name = "Tar",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_speed_by <color=orange>10</color>%",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TarBlobKills",
                resistance_modifier = "Poison = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "Speed",
                modifier_value = 1.1f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "golem_hunter",
                display_name = "$almanac_achievement_golem_hunter",
                description = "$almanac_achievement_golem_hunter_desc",
                lore = "$almanac_golem_hunter_lore",
                sprite_name = "TrophySGolem",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_blunt</color>\n$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "StoneGolemKills",
                resistance_modifier = "Blunt = Weak",
                stop_message = "$almanac_removed_achievement_power",
                modifier = "FrostDMG",
                modifier_value = 5f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "gaseous",
                display_name = "$almanac_achievement_gaseous",
                description = "$almanac_achievement_gaseous_desc",
                lore = "$almanac_gaseous_lore",
                sprite_name = "BombOoze",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_poison_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "TotalKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 500,
                resistance_modifier = "Poison = Weak",
                modifier = "PoisonDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "spark",
                display_name = "$almanac_achievement_spark I",
                description = "$almanac_achievement_spark_desc",
                lore = "$almanac_spark_lore",
                sprite_name = "Thunderstone",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_lightning</color>\n$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 200,
                resistance_modifier = "Lightning = Weak",
                modifier = "LightningDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "spark_1",
                display_name = "$almanac_achievement_spark II",
                description = "$almanac_achievement_spark_desc",
                lore = "$almanac_spark_lore",
                sprite_name = "Thunderstone",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_lightning</color>\n$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 400,
                resistance_modifier = "Lightning = Weak",
                modifier = "LightningDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "spark_2",
                display_name = "$almanac_achievement_spark III",
                description = "$almanac_achievement_spark_desc",
                lore = "$almanac_spark_lore",
                sprite_name = "Thunderstone",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_lightning</color>\n$almanac_increase_lightning_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "EikthyrKills",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Lightning = Weak",
                goal = 600,
                modifier = "LightningDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "sword_master",
                display_name = "$almanac_achievement_sword_master",
                description = "$almanac_achievement_sword_master_desc",
                lore = "$almanac_sword_master_lore",
                sprite_name = "almanac_sword_blue",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_pierce</color>\n$almanac_increase_melee_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Swords",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Pierce = Weak",
                modifier = "MeleeDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "axe_master",
                display_name = "$almanac_achievement_axe_master",
                description = "$almanac_achievement_axe_master_desc",
                lore = "$almanac_axe_master_lore",
                sprite_name = "AxeJotunBane",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_blunt</color>\n$almanac_increase_melee_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Axes",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Blunt = Weak",
                modifier = "MeleeDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "lumberjack",
                display_name = "$almanac_achievement_lumberjack",
                description = "$almanac_achievement_lumberjack_desc",
                lore = "$almanac_lumberjack_lore",
                sprite_name = "almanac_wood_stack",
                tool_tip = "$almanac_increase_chop_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "Trees",
                modifier = "ChopDMG",
                modifier_value = 5f,
                goal = 1000
            },
            new AchievementData()
            {
                unique_name = "lumberer",
                display_name = "$almanac_achievement_lumberer",
                description = "$almanac_achievement_lumberer_desc",
                lore = "$almanac_lumberer_lore",
                sprite_name = "almanac_wood_log",
                tool_tip = "$almanac_increase_chop_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                stop_message = "$almanac_removed_achievement_power",
                achievement_type = "DeathByTree",
                modifier = "ChopDMG",
                modifier_value = 5f,
                goal = 100
            },
            new AchievementData()
            {
                unique_name = "daredevil",
                display_name = "$almanac_achievement_daredevil",
                description = "$almanac_achievement_daredevil_desc",
                lore = "$almanac_daredevil_lore",
                sprite_name = "YagluthDrop",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_stamina_by <color=orange>15</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DeathByEdge",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Spirit = Weak",
                modifier = "BaseStamina",
                modifier_value = 15f,
                goal = 1
            },
            new AchievementData()
            {
                unique_name = "turret",
                display_name = "$almanac_achievement_turret",
                description = "$almanac_achievement_turret_desc",
                lore = "$almanac_turret_lore",
                sprite_name = "ArrowCarapace",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_projectile_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "ArrowsShot",
                stop_message = "$almanac_removed_achievement_power",
                resistance_modifier = "Fire = Weak",
                modifier = "RangedDMG",
                modifier_value = 5f,
                goal = 5000
            },
            new AchievementData()
            {
                unique_name = "adventurer",
                display_name = "$almanac_achievement_adventurer",
                description = "$almanac_achievement_adventurer_desc",
                lore = "$almanac_adventurer_lore",
                sprite_name = "BlackCore",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_poison</color>\n$almanac_increase_stamina_regen_by <color=orange>50</color>%",
                start_effects = new []{"sfx_coins_placed","vfx_HealthUpgrade"},
                achievement_type = "TimeOutOfBase",
                stop_message = "$almanac_removed_achievement_power",
                goal = 50000,
                resistance_modifier = "Poison = Weak",
                modifier = "StaminaRegen",
                modifier_value = 1.5f
            },
            new AchievementData()
            {
                unique_name = "cultist_hunter",
                display_name = "$almanac_achievement_cultist_hunter",
                description = "$almanac_achievement_cultist_hunter_desc",
                lore = "$almanac_cultist_hunter_lore",
                sprite_name = "almanac_book_red",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_frost</color>\n$almanac_increase_fire_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "CultistKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Frost = Weak",
                modifier = "FireDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "goblin_shaman_hunter",
                display_name = "$almanac_achievement_goblin_shaman_hunter",
                description = "$almanac_achievement_goblin_shaman_hunter_desc",
                lore = "$almanac_goblin_shaman_hunter_lore",
                sprite_name = "GoblinTotem",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_frost</color>\n$almanac_increase_fire_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "GoblinShamanKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Frost = Weak",
                modifier = "FireDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "drake_hunter",
                display_name = "$almanac_achievement_drake_hunter",
                description = "$almanac_achievement_drake_hunter_desc",
                lore = "$almanac_drake_hunter_lore",
                sprite_name = "FreezeGland",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "DrakeKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Fire = Weak",
                modifier = "FrostDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "dragon_killer",
                display_name = "$almanac_achievement_dragon_killer",
                description = "$almanac_achievement_dragon_killer_desc",
                lore = "$almanac_dragon_killer_lore",
                sprite_name = "TrophyDragonQueen",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_fire</color>\n$almanac_increase_frost_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "ModerKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Fire = Weak",
                modifier = "FrostDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "bonemass_killer",
                display_name = "$almanac_achievement_bonemass_killer",
                description = "$almanac_achievement_bonemass_killer_desc",
                lore = "$almanac_bonemass_killer_lore",
                sprite_name = "TrophyBonemass",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_poison_damage_by <color=orange>5</color>",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "BonemassKills",
                stop_message = "$almanac_removed_achievement_power",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "PoisonDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "stench",
                display_name = "$almanac_achievement_stench",
                description = "$almanac_achievement_stench_desc",
                lore = "$almanac_stench_lore",
                sprite_name = "Ooze",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_poison_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Deaths",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "PoisonDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "vengeful",
                display_name = "$almanac_achievement_vengeful",
                description = "$almanac_achievement_vengeful_desc",
                lore = "$almanac_vengeful_lore",
                sprite_name = "almanac_gem_red",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_spirit_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "WraithKills",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "SpiritDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "ghastly",
                display_name = "$almanac_achievement_ghastly",
                description = "$almanac_achievement_ghastly_desc",
                lore = "$almanac_ghastly_lore",
                sprite_name = "almanac_bottle_empty",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_spirit_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "GhostKills",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "SpiritDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "fenring",
                display_name = "$almanac_achievement_fenring",
                description = "$almanac_achievement_fenring_desc",
                lore = "$almanac_fenring_lore",
                sprite_name = "TrophyUlv",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_spirit_damage_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "FenringKills",
                goal = 100,
                resistance_modifier = "Spirit = Weak",
                modifier = "SpiritDMG",
                modifier_value = 5f
            },
            new AchievementData()
            {
                unique_name = "healer",
                display_name = "$almanac_achievement_healer",
                description = "$almanac_achievement_healer_desc",
                lore = "$almanac_healer_lore",
                sprite_name = "TrophyGreydwarfShaman",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_slash</color>\n$almanac_greydwarf_shaman_heal\n$almanac_reduce_health_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"shaman_heal_aoe","sfx_greydwarf_shaman_heal"},
                stop_effects = new []{"shaman_heal_aoe","sfx_greydwarf_shaman_heal"},
                achievement_type = "GreydwarfShamanKills",
                goal = 100,
                resistance_modifier = "Slash = Weak",
                modifier = "BaseHP",
                activation_animation = "staff_summon",
                modifier_value = -5f,
                cooldown = 30
            },
            new AchievementData()
            {
                unique_name = "dverger_healer",
                display_name = "$almanac_achievement_dverger_healer",
                description = "$almanac_achievement_dverger_healer_desc",
                lore = "$almanac_dverger_healer_lore",
                sprite_name = "Lantern",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_weak</color> VS <color=orange>$almanac_elemental</color>\n$almanac_dverger_heal\n$almanac_reduce_health_by <color=orange>5</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"DvergerStaffHeal_aoe","sfx_dverger_heal_start"},
                stop_effects = new []{"DvergerStaffHeal_aoe","sfx_dverger_heal_finish"},
                achievement_type = "GreydwarfShamanKills",
                goal = 100,
                resistance_modifier = "Elemental = Weak",
                modifier = "BaseHP",
                activation_animation = "staff_summon",
                modifier_value = -5f,
                cooldown = 30
            },
            new AchievementData()
            {
                unique_name = "trophy_hunter",
                display_name = "$almanac_achievement_trophy_hunter",
                description = "$almanac_achievement_trophy_hunter_desc",
                lore = "$almanac_trophy_hunter_lore",
                sprite_name = "almanac_silver_coins",
                tool_tip = "$almanac_modify_damage_by <color=orange>$almanac_very_resistant</color> VS <color=orange>$almanac_spirit</color>\n$almanac_increase_eitr_by <color=orange>50</color>",
                stop_message = "$almanac_removed_achievement_power",
                start_effects = new []{"sfx_coins_placed"},
                achievement_type = "Trophies",
                resistance_modifier = "Spirit = VeryResistant",
                modifier = "BaseEitr",
                modifier_value = 50f
            },
        };
        
        return almanacAchievements;
    }

    [Serializable]
    public class AchievementData : ISerializableParameter
    {
        public string unique_name = null!;
        public string display_name = "";
        public string sprite_name = "";
        public string description = "";
        public string lore = "";
        public string tool_tip = "";
        public string stop_message = "$almanac_removed_achievement_power";
        public string[] start_effects = null!;
        public string[] stop_effects = null!;
        public string achievement_type = "";
        public int goal = 0;
        public string resistance_modifier = "";
        public string modifier = "";
        public float modifier_value = 0f;
        public string activation_animation = "gpower";
        [FormerlySerializedAs("statusEffectDuration")] public int cooldown = 0;
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(unique_name);
            pkg.Write(display_name);
            pkg.Write(sprite_name ?? "");
            pkg.Write(description ?? "");
            pkg.Write(lore ?? "");
            pkg.Write(tool_tip ?? "");
            pkg.Write(stop_message ?? "");
            pkg.Write(start_effects.Length); // Write the number of start_effects
            foreach(string effect in start_effects) pkg.Write(effect);
            pkg.Write(stop_effects.Length);
            foreach (string effect in stop_effects) pkg.Write(effect);
            pkg.Write(achievement_type);
            pkg.Write(goal);
            pkg.Write(resistance_modifier);
            pkg.Write(modifier);
            pkg.Write(modifier_value);
            pkg.Write(activation_animation);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            unique_name = pkg.ReadString();
            display_name = pkg.ReadString();
            sprite_name = pkg.ReadString();
            description = pkg.ReadString();
            lore = pkg.ReadString();
            tool_tip = pkg.ReadString();
            stop_message = pkg.ReadString();
            int startEffectsLength = pkg.ReadInt();
            start_effects = new string[startEffectsLength];
            for (int i = 0; i < startEffectsLength; ++i) start_effects[i] = pkg.ReadString();
            int stopEffectsLength = pkg.ReadInt();
            stop_effects = new string[stopEffectsLength];
            for (int i = 0; i < stopEffectsLength; ++i) stop_effects[i] = pkg.ReadString();
            achievement_type = pkg.ReadString();
            goal = pkg.ReadInt();
            resistance_modifier = pkg.ReadString();
            modifier = pkg.ReadString();
            modifier_value = pkg.ReadSingle();
            activation_animation = pkg.ReadString();

        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            AchievementData other = (AchievementData)obj;

            return unique_name == other.unique_name &&
                   display_name == other.display_name &&
                   description == other.description &&
                   sprite_name == other.sprite_name &&
                   lore == other.lore &&
                   tool_tip == other.tool_tip &&
                   stop_message == other.stop_message &&
                   achievement_type == other.achievement_type &&
                   goal == other.goal &&
                   resistance_modifier == other.resistance_modifier &&
                   modifier == other.modifier &&
                   activation_animation == other.activation_animation &&
                   cooldown == other.cooldown;
        }

        public override int GetHashCode()
        {
            int hashCode = (unique_name != null ? unique_name.GetHashCode() : 0);

            return hashCode;
        }

    }
}