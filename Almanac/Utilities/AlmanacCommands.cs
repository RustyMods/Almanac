using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.FileSystem;
using Almanac.UI;
using BepInEx;
using HarmonyLib;

namespace Almanac.Utilities;

public static class CommandsManager
{
    private static readonly Dictionary<string, AlmanacCommand> m_commands = new();

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
    private static class Terminal_Awake_Patch
    {
        private static void Postfix()
        {
            Terminal.ConsoleCommand _ = new Terminal.ConsoleCommand("almanac", "use help to list out commands",
                (Terminal.ConsoleEventFailable)(
                    args =>
                    {
                        if (args.Length < 2) return false;
                        if (!m_commands.TryGetValue(args[1], out AlmanacCommand command)) return false;
                        return command.Run(args);
                    }), optionsFetcher: () => m_commands.Keys.ToList());

            var keys = new AlmanacCommand("keys", "Similar to listkeys, almanac keys prints all the current global keys and private keys the player current holds", _ =>
            {
                List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
                AlmanacPlugin.AlmanacLogger.LogInfo("Global keys: ");
                foreach (string key in globalKeys) AlmanacPlugin.AlmanacLogger.LogInfo(key);
                AlmanacPlugin.AlmanacLogger.LogInfo("Private Keys: ");
                foreach (string key in Creatures.m_defeatKeys)
                {
                    if (ZoneSystem.instance.GetGlobalKey(key)) AlmanacPlugin.AlmanacLogger.LogInfo(key);
                }
                return true;
            });

            var size = new AlmanacCommand("size", "Prints the kilobyte size of almanac custom data saved in player save file", _ =>
            {
                if (!Player.m_localPlayer) return false;
                if (!Player.m_localPlayer.m_customData.TryGetValue(PlayerStats.AlmanacStatsKey, out string data))
                {
                    AlmanacPlugin.AlmanacLogger.LogInfo("No Almanac custom player data found");
                    return false;
                }
                int size = Encoding.UTF8.GetByteCount(data);
                double kilobytes = size / 1024.0;
                AlmanacPlugin.AlmanacLogger.LogInfo("Almanac Custom Data size: " + kilobytes + " kilobytes");
                return true;
            });

            var write = new AlmanacCommand("write", "Writes to file all the default achievements for the almanac", args =>
            {
                if (args.Length < 3) return false;
                if (args[2] == "achievements")
                {
                    AlmanacPlugin.AlmanacLogger.LogInfo("Almanac writing default achievements to file");
                    AlmanacPlugin.AlmanacLogger.LogInfo(AlmanacPaths.AchievementFolderPath);
                    AchievementYML.InitDefaultAchievements(true);
                    return true;
                }

                return false;
            }, optionsFetcher: () => new List<string>(){"achievements"});

            var pickable = new AlmanacCommand("pickable", "[prefabName] - Prints total amount of picked item, you can use 'all' to print a list of entire pickable data", args =>
            {
                if (args.Length < 3)
                {
                    AlmanacPlugin.AlmanacLogger.LogInfo("Invalid information: ex: almanac pickable [pickable prefab name]");
                    return false;
                }

                if (args[2] == "all")
                {
                    foreach (KeyValuePair<string, int> kvp in PlayerStats.LocalPlayerData.Player_Pickable_Data)
                    {
                        AlmanacPlugin.AlmanacLogger.LogInfo(kvp.Key + " : " + kvp.Value);
                    }
                    return true;
                }

                if (!PlayerStats.GetPlayerPickableValue(args[2], out int pickableValue))
                {
                    AlmanacPlugin.AlmanacLogger.LogInfo("Failed to get pickable value");
                    return false;
                };
                AlmanacPlugin.AlmanacLogger.LogInfo(Player.m_localPlayer.GetHoverName() + " has picked " + pickableValue + " " + args[2]);
                return true;
            }, optionsFetcher: () =>
            {
                List<string> output = new();
                output.AddRange(PlayerStats.LocalPlayerData.Player_Pickable_Data.Keys);
                output.Add("all");
                output.Sort();
                return output;
            });

            var reload = new AlmanacCommand("reload", "Reloads graphic assets", _ =>
            {
                if (!AlmanacUI.m_instance) return false;
                AlmanacUI.m_instance.ReloadAssets();
                return true;
            });

            var kd = new AlmanacCommand("kd", "[prefabName] - local kill / death tracked almanac data", args =>
            {
                if (args.Length < 3) return false;
                if (!Creatures.m_creatures.TryGetValue(args[2], out Creatures.Data data)) return false;
                if (!PlayerStats.LocalPlayerData.Player_Kill_Deaths.TryGetValue(data.m_defeatKey, out KillDeaths value)) return false;
                AlmanacPlugin.AlmanacLogger.LogInfo($"Key: {args[1]} , kills: {value.kills} , deaths: {value.deaths}");
                return true;
            }, optionsFetcher: () => Creatures.m_creatures.Keys.ToList());

            var achievement = new AlmanacCommand("achievement",
                "[type] [key?]- list of prefabs included in the completion count, only list achievements",
                args =>
                {
                    if (args.Length < 3) return false;
                    if (!Enum.TryParse(args[2], true, out AchievementTypes.AchievementType type)) return false;
                    switch (type)
                    {
                        case AchievementTypes.AchievementType.MeadowCreatures or AchievementTypes.AchievementType.BlackForestCreatures or AchievementTypes.AchievementType.SwampCreatures or AchievementTypes.AchievementType.MountainCreatures or AchievementTypes.AchievementType.PlainsCreatures or AchievementTypes.AchievementType.AshLandCreatures or AchievementTypes.AchievementType.MistLandCreatures or AchievementTypes.AchievementType.DeepNorthCreatures or AchievementTypes.AchievementType.OceanCreatures:
                            Heightmap.Biome biome = type switch
                            {
                                AchievementTypes.AchievementType.MeadowCreatures => Heightmap.Biome.Meadows,
                                AchievementTypes.AchievementType.BlackForestCreatures => Heightmap.Biome.BlackForest,
                                AchievementTypes.AchievementType.SwampCreatures => Heightmap.Biome.Swamp,
                                AchievementTypes.AchievementType.MountainCreatures => Heightmap.Biome.Mountain,
                                AchievementTypes.AchievementType.PlainsCreatures => Heightmap.Biome.Plains,
                                AchievementTypes.AchievementType.MistLandCreatures => Heightmap.Biome.Mistlands,
                                AchievementTypes.AchievementType.AshLandCreatures => Heightmap.Biome.AshLands,
                                AchievementTypes.AchievementType.DeepNorthCreatures => Heightmap.Biome.DeepNorth,
                                AchievementTypes.AchievementType.OceanCreatures => Heightmap.Biome.Ocean,
                                // ReSharper disable once UnreachableSwitchArmDueToIntegerAnalysis
                                _ => Heightmap.Biome.None
                            };
                            var biomeCreatures = CreatureLists.GetBiomeCreatures(biome);
                            foreach (var creature in biomeCreatures)
                            {
                                AlmanacPlugin.AlmanacLogger.LogInfo(creature.m_prefabName);
                            }
                            AlmanacPlugin.AlmanacLogger.LogInfo("Total: " + biomeCreatures.Count);
                            break;
                        case AchievementTypes.AchievementType.CustomCreatureGroups:
                            if (args.Length < 4)
                            {
                                AlmanacPlugin.AlmanacLogger.LogWarning("Missing key specifier, example: almanac achievement customcreaturegroup custom_brute_group");
                                return true;
                            }

                            var customCreatures = CreatureLists.GetCustomCreatureGroup(args[3]);
                            foreach (var creature in customCreatures)
                            {
                                AlmanacPlugin.AlmanacLogger.LogInfo(creature.m_prefabName);
                            }
                            AlmanacPlugin.AlmanacLogger.LogInfo("Total: " + customCreatures.Count);
                            break;
                        case AchievementTypes.AchievementType.Fish or AchievementTypes.AchievementType.Materials or AchievementTypes.AchievementType.Consumables or AchievementTypes.AchievementType.Weapons or AchievementTypes.AchievementType.Swords or AchievementTypes.AchievementType.Axes or AchievementTypes.AchievementType.PoleArms or AchievementTypes.AchievementType.Spears or AchievementTypes.AchievementType.Maces or AchievementTypes.AchievementType.Knives or AchievementTypes.AchievementType.Shields or AchievementTypes.AchievementType.Staves or AchievementTypes.AchievementType.Arrows or AchievementTypes.AchievementType.Bows or AchievementTypes.AchievementType.Valuables or AchievementTypes.AchievementType.Potions or AchievementTypes.AchievementType.Trophies:
                            List<Items.Data> list = type switch
                            {
                                AchievementTypes.AchievementType.Fish => Items.GetFishes(),
                                AchievementTypes.AchievementType.Materials => Items.GetMaterials(),
                                AchievementTypes.AchievementType.Consumables => Items.GetConsumables(),
                                AchievementTypes.AchievementType.Weapons => Items.GetWeapons(),
                                AchievementTypes.AchievementType.Swords => Items.GetWeaponBySkill(Skills.SkillType.Swords),
                                AchievementTypes.AchievementType.Axes => Items.GetWeaponBySkill(Skills.SkillType.Axes),
                                AchievementTypes.AchievementType.PoleArms => Items.GetWeaponBySkill(Skills.SkillType.Polearms),
                                AchievementTypes.AchievementType.Spears => Items.GetWeaponBySkill(Skills.SkillType.Spears),
                                AchievementTypes.AchievementType.Maces => Items.GetWeaponBySkill(Skills.SkillType.Clubs),
                                AchievementTypes.AchievementType.Knives => Items.GetWeaponBySkill(Skills.SkillType.Knives),
                                AchievementTypes.AchievementType.Shields => Items.GetWeaponBySkill(Skills.SkillType.Blocking),
                                AchievementTypes.AchievementType.Staves => Items.GetStaves(),
                                AchievementTypes.AchievementType.Arrows => Items.GetAmmunition(),
                                AchievementTypes.AchievementType.Bows => Items.GetWeaponBySkill(Skills.SkillType.Bows),
                                AchievementTypes.AchievementType.Valuables => Items.GetValuables(),
                                AchievementTypes.AchievementType.Potions => Items.GetPotions(),
                                AchievementTypes.AchievementType.Trophies => Items.GetTrophies(),
                                // ReSharper disable once UnreachableSwitchArmDueToIntegerAnalysis
                                _ => new List<Items.Data>(),
                            };
                            list = list.Where(x => !Items.IsTool(x.m_item) || !Items.IsBait(x.m_item)).ToList();
                            foreach (var data in list)
                            {
                                AlmanacPlugin.AlmanacLogger.LogInfo(data.m_item.name);
                            }
                            AlmanacPlugin.AlmanacLogger.LogInfo("Total: " + list.Count);
                            break;
                    }
                    return true;
                }, optionsFetcher: () =>
                {
                    List<AchievementTypes.AchievementType> availableTypes = new()
                    {
                        AchievementTypes.AchievementType.MeadowCreatures, AchievementTypes.AchievementType.BlackForestCreatures, AchievementTypes.AchievementType.SwampCreatures, AchievementTypes.AchievementType.MountainCreatures, AchievementTypes.AchievementType.PlainsCreatures, AchievementTypes.AchievementType.MistLandCreatures, AchievementTypes.AchievementType.AshLandCreatures, AchievementTypes.AchievementType.DeepNorthCreatures, AchievementTypes.AchievementType.OceanCreatures,
                        AchievementTypes.AchievementType.CustomCreatureGroups,
                        AchievementTypes.AchievementType.Fish, AchievementTypes.AchievementType.Materials, AchievementTypes.AchievementType.Consumables, AchievementTypes.AchievementType.Weapons, AchievementTypes.AchievementType.Swords, AchievementTypes.AchievementType.Axes, AchievementTypes.AchievementType.PoleArms, AchievementTypes.AchievementType.Spears, AchievementTypes.AchievementType.Maces, AchievementTypes.AchievementType.Knives, AchievementTypes.AchievementType.Shields, AchievementTypes.AchievementType.Staves, AchievementTypes.AchievementType.Arrows, AchievementTypes.AchievementType.Bows, AchievementTypes.AchievementType.Valuables, AchievementTypes.AchievementType.Potions, AchievementTypes.AchievementType.Trophies
                    };
                    return availableTypes.Select(x => x.ToString()).ToList();
                });

            var clear = new AlmanacCommand("reset", "clears almanac data from player save file", _ =>
            {
                if (!Player.m_localPlayer) return false;
                Player.m_localPlayer.m_customData.Remove(PlayerStats.AlmanacStatsKey);
                Player.m_localPlayer.m_customData.Remove(AchievementManager.CollectedRewardKey);
                Player.m_localPlayer.m_customData.Remove(EffectMan.PlayerEffectKey);
                EffectMan.Clear();
                AlmanacPlugin.AlmanacLogger.LogInfo("Cleared Almanac Records from Player Save");
                return true;
            });

            var help = new AlmanacCommand("help", "list of almanac commands", _ =>
            {
                foreach (var command in m_commands)
                {
                    AlmanacPlugin.AlmanacLogger.LogInfo($"{command.Key} - {command.Value.m_description}");
                }
                return true;
            });
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.updateSearch))]
    private static class Terminal_UpdateSearch_Patch
    {
        private static bool Prefix(Terminal __instance, string word)
        {
            if (__instance.m_search == null) return true;
            string[] strArray = __instance.m_input.text.Split(' ');
            if (strArray.Length < 3) return true;
            if (strArray[0] != "almanac") return true;
            return HandleSearch(__instance, word, strArray);
        }
        
        private static bool HandleSearch(Terminal __instance, string word, string[] strArray)   
        {
            if (!m_commands.TryGetValue(strArray[1], out AlmanacCommand command)) return true;
            if (command.HasOptions() && strArray.Length == 3)
            {
                List<string> list = command.FetchOptions();
                List<string> filteredList;
                string currentSearch = strArray[2];
                if (!currentSearch.IsNullOrWhiteSpace())
                {
                    int indexOf = list.IndexOf(currentSearch);
                    filteredList = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                    filteredList = filteredList.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
                }
                else filteredList = list;
                if (filteredList.Count <= 0) __instance.m_search.text = command.m_description;
                else
                {
                    __instance.m_lastSearch.Clear();
                    __instance.m_lastSearch.AddRange(filteredList);
                    __instance.m_lastSearch.Remove(word);
                    __instance.m_search.text = "";
                    int maxShown = 10;
                    int count = Math.Min(__instance.m_lastSearch.Count, maxShown);
                    for (int index = 0; index < count; ++index)
                    {
                        string text = __instance.m_lastSearch[index];
                        __instance.m_search.text += text + " ";
                    }

                    if (__instance.m_lastSearch.Count <= maxShown) return false;
                    int remainder = __instance.m_lastSearch.Count - maxShown;
                    __instance.m_search.text += $"... {remainder} more.";
                }
            }
            else __instance.m_search.text = command.m_description;
                
            return false;
        }
    }

    private class AlmanacCommand
    {
        public readonly string m_input;
        public readonly string m_description;
        private readonly bool m_isSecret;
        private readonly bool m_adminOnly;
        private readonly Func<Terminal.ConsoleEventArgs, bool> m_command;
        private readonly Func<List<string>>? m_optionFetcher;
        public bool Run(Terminal.ConsoleEventArgs args) => !IsAdmin() || m_command(args);
        private bool IsAdmin()
        {
            if (!m_adminOnly || ZNet.m_instance.LocalPlayerIsAdminOrHost()) return true;
            AlmanacPlugin.AlmanacLogger.LogWarning("Admin only command");
            return false;
        }
        public bool IsSecret() => m_isSecret;
        public List<string> FetchOptions() => m_optionFetcher == null ? new() :  m_optionFetcher();
        public bool HasOptions() => m_optionFetcher != null;
        
        [Description("Register a custom command with the prefix almanac")]
        public AlmanacCommand(string input, string description, Func<Terminal.ConsoleEventArgs, bool> command, Func<List<string>>? optionsFetcher = null, bool isSecret = false, bool adminOnly = false)
        {
            m_input = input;
            m_description = description;
            m_command = command;
            m_isSecret = isSecret;
            m_commands[m_input] = this;
            m_optionFetcher = optionsFetcher;
            m_adminOnly = adminOnly;
        }
    }
}