using System.Collections.Generic;
using System.IO;
using Almanac.FileSystem;
using Almanac.Utilities;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.TreasureHunt;

public static class TreasureManager
{
    public static readonly List<Data.Treasure> RegisteredTreasure = new();
    public static readonly List<Data.TreasureYML> ValidatedYML = new();

    public static void InitTreasureManager(bool useServerData = false)
    {
        if (!ObjectDB.instance) return;
        
        RegisteredTreasure.Clear();
        ValidatedYML.Clear();

        if (useServerData)
        {
            if (ServerSyncedData.ServerTreasureList.Value.IsNullOrWhiteSpace()) return;
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Updating treasure hunts");

            IDeserializer deserializer = new DeserializerBuilder().Build();
            List<Data.TreasureYML> list = deserializer.Deserialize<List<Data.TreasureYML>>(ServerSyncedData.ServerTreasureList.Value);
            foreach (Data.TreasureYML? treasure in list)
            {
                var _ = new Data.Treasure(treasure);
            }
        }
        else
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Initializing Treasure Manager");

            LoadLocalData();
        }
        
    }

    private static void LoadLocalData()
    {
        AlmanacPaths.CreateFolderDirectories();

        string[] paths = Directory.GetFiles(AlmanacPaths.TreasureHuntFolderPath, "*.yml");

        if (paths.Length <= 0)
        {
            ISerializer serializer = new SerializerBuilder().Build();
            
            foreach (Data.TreasureYML? treasure in GetDefaultTreasure())
            {
                var _ = new Data.Treasure(treasure);
                string path = AlmanacPaths.TreasureHuntFolderPath + Path.DirectorySeparatorChar + treasure.name.Replace(" ", "_") + ".yml";
                string data = serializer.Serialize(treasure);
                File.WriteAllText(path, data);
            }
        }
        else
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            foreach (string path in paths)
            {
                string data = File.ReadAllText(path);
                Data.TreasureYML deserialized = deserializer.Deserialize<Data.TreasureYML>(data);
                var _ = new Data.Treasure(deserialized);
            }
        }
    }

    private static List<Data.TreasureYML> GetDefaultTreasure()
    {
        return new()
        {
            new Data.TreasureYML()
            {
                name = "Meadow Treasure",
                sprite_name = "map",
                cost = 10,
                biome = Heightmap.Biome.Meadows,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 10,
                        max_stack = 20,
                    },
                    new Data.LootData()
                    {
                        item_name = "Flint",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "DeerStew",
                        min_stack = 1,
                        max_stack = 20
                    },
                    new Data.LootData()
                    {
                        item_name = "SurtlingCore",
                        min_stack = 1,
                        max_stack = 5
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Black Forest Treasure",
                sprite_name = "map",
                cost = 20,
                biome = Heightmap.Biome.BlackForest,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 10,
                        max_stack = 20,
                    },
                    new Data.LootData()
                    {
                        item_name = "CopperOre",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "TinOre",
                        min_stack = 1,
                        max_stack = 20
                    },
                    new Data.LootData()
                    {
                        item_name = "SurtlingCore",
                        min_stack = 1,
                        max_stack = 5
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Swamp Treasure",
                sprite_name = "map",
                cost = 30,
                biome = Heightmap.Biome.Swamp,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 10,
                        max_stack = 20,
                    },
                    new Data.LootData()
                    {
                        item_name = "IronScrap",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "Bloodbag",
                        min_stack = 1,
                        max_stack = 20
                    },
                    new Data.LootData()
                    {
                        item_name = "Thistle",
                        min_stack = 1,
                        max_stack = 50
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Mountain Treasure",
                sprite_name = "map",
                cost = 40,
                biome = Heightmap.Biome.Mountain,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Obsidian",
                        min_stack = 10,
                        max_stack = 20,
                    },
                    new Data.LootData()
                    {
                        item_name = "SilverOre",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "Crystal",
                        min_stack = 1,
                        max_stack = 20
                    },
                    new Data.LootData()
                    {
                        item_name = "WolfHairBundle",
                        min_stack = 1,
                        max_stack = 5
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Plains Treasure",
                sprite_name = "map",
                cost = 50,
                biome = Heightmap.Biome.Plains,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 100,
                        max_stack = 200,
                    },
                    new Data.LootData()
                    {
                        item_name = "BlackMetalScrap",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "Barley",
                        min_stack = 1,
                        max_stack = 20
                    },
                    new Data.LootData()
                    {
                        item_name = "Flax",
                        min_stack = 1,
                        max_stack = 20
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Mistlands Treasure",
                sprite_name = "map",
                cost = 100,
                biome = Heightmap.Biome.Mistlands,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 100,
                        max_stack = 200,
                    },
                    new Data.LootData()
                    {
                        item_name = "Softtissue",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "MushroomJotunPuffs",
                        min_stack = 1,
                        max_stack = 20
                    },
                    new Data.LootData()
                    {
                        item_name = "MushroomMagecap",
                        min_stack = 1,
                        max_stack = 20
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Deep North Treasure",
                sprite_name = "map",
                cost = 150,
                biome = Heightmap.Biome.DeepNorth,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 100,
                        max_stack = 200,
                    },
                    new Data.LootData()
                    {
                        item_name = "Softtissue",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "SilverOre",
                        min_stack = 1,
                        max_stack = 30
                    },
                    new Data.LootData()
                    {
                        item_name = "MushroomMagecap",
                        min_stack = 1,
                        max_stack = 20
                    }
                }
            },
            new Data.TreasureYML()
            {
                name = "Ashlands Treasure",
                sprite_name = "map",
                cost = 150,
                biome = Heightmap.Biome.AshLands,
                loot = new List<Data.LootData>()
                {
                    new Data.LootData()
                    {
                        item_name = "Coins",
                        min_stack = 100,
                        max_stack = 200,
                    },
                    new Data.LootData()
                    {
                        item_name = "FlametalOre",
                        min_stack = 2,
                        max_stack = 10,
                    },
                    new Data.LootData()
                    {
                        item_name = "IronScrap",
                        min_stack = 1,
                        max_stack = 30
                    },
                    new Data.LootData()
                    {
                        item_name = "BugMeat",
                        min_stack = 1,
                        max_stack = 20
                    }
                }
            },
        };
    }
}