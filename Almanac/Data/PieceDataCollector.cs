using System.Collections.Generic;
using System.Text.RegularExpressions;
using Almanac.FileSystem;
using UnityEngine;

namespace Almanac.Data;

public static class PieceDataCollector
{
    private static readonly List<GameObject> allPieces = new ();
    public static readonly List<GameObject> plantPieces = new();
    public static readonly List<GameObject> furniturePieces = new();
    public static readonly List<GameObject> modPieces = new();
    public static readonly List<GameObject> miscPieces = new();
    public static readonly List<GameObject> buildPieces = new();
    public static readonly List<GameObject> craftingPieces = new();
    public static readonly List<GameObject> defaultPieces = new();
    public static readonly List<GameObject> comfortPieces = new();
    public static List<GameObject> GetFilteredPieces(List<GameObject> list) => AlmanacPlugin._UseIgnoreList.Value is AlmanacPlugin.Toggle.Off ? list : list.FindAll(piece => !Filters.FilterList.Contains(piece.name));
    private static void ClearCachedPieces()
    {
        plantPieces.Clear();
        furniturePieces.Clear();
        modPieces.Clear();
        miscPieces.Clear();
        buildPieces.Clear();
        craftingPieces.Clear();
        defaultPieces.Clear();
    }
    public static void GetBuildPieces()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Saving pieces to almanac");
        GetPieces();
        ClearCachedPieces();
        HashSet<string> plantNames = new HashSet<string>();
        HashSet<string> furnitureNames = new HashSet<string>();
        HashSet<string> modNames = new HashSet<string>();
        HashSet<string> miscNames = new HashSet<string>();
        HashSet<string> buildNames = new HashSet<string>();
        HashSet<string> craftNames = new HashSet<string>();
        HashSet<string> defaultNames = new HashSet<string>();

        List<string> toDefaultMap = new List<string>()
        {
            "ship_construction",
            "paved_road_v2",
            "paved_road",
            "cultivate_v2",
            "cultivate",
            "mud_road",
            "mud_road_v2",
            "path_v2",
            "path",
            "replant_v2",
            "replant",
            "raise_v2",
            "raise",
            "fire_pit_haldor",
            "fire_pit_hildir",
            "dverger_guardstone",
            "guard_stone_test",
            "ML_TreasureChestOcean"
        };

        List<string> krumpToCraftMap = new()
        {
            "D_Alchemy_Cauldron",
            "D_Alchemy_Table",
            "D_Seed_Table",
            "D_Alchemy_Library",
            "D_Water_Catcher",
            "D_Roasting_Spit",
            "D_Preparation_Table",
            "D_Beehive",
            "D_Mortar_and_Pestle",
            "D_Stone_Griddle",
            "D_Big_Stone_Griddle",
            "D_Honey_Extractor",
            "D_Butcher_Table",
            "D_Oven",
            "D_Beverage_Station",
            "D_Butcher_Tools",
            "D_Cutting_Table",
            "D_ABronze_Caouldron",
            "D_Alchemy_Altar",
            "D_Chicken_Coop",
            "D_Book_Stand",
            "D_Bronze_Caouldron",
            "D_Bronze_HangCaouldron",
            "Krump_CS_Shipyard",
            "Krump_CS_Fishing_Trap",
            "Krump_CS_Crab_Trap",
            "Krump_CS_Fermenter_Oil",
            "Krump_CS_Shipyard_Crane",
            "Krump_Spawner_Thrall_Trader",
            "Krump_CS_Shipyard_Ship_Construction",
            "Krump_CS_Shipyard_Horn",
            
        };
        List<string> krumpToOtherMap = new()
        {
            "Krump_Spawner_Treasure_BlackForest_Crypt",
            "Krump_Spawner_Treasure_BlackForest_Crypt",
            "Krump_Spawner_Treasure_Mountain_Cave",
            "Krump_Spawner_Treasure_Meadows_Dungeon",
            "Krump_Spawner_Treasure_Swamps",
            "Krump_Spawner_Treasure_BlackForest",
            "Krump_Spawner_Treasure_Meadows",
            
        };
        List<string> krumpToMiscMap = new()
        {
            "Krump_Ship_Raft",
            "Krump_Ship_Karve",
            "Krump_Ship_Knarr_Transporter",
        };
        
        foreach (GameObject piece in allPieces)
        {
            piece.TryGetComponent(out Piece pieceScript);
            piece.TryGetComponent(out WearNTear wearNTear);
            piece.TryGetComponent(out Plant plant);
            
            if (!pieceScript) continue;
            if (!pieceScript.enabled && AlmanacPlugin._ShowAllData.Value is AlmanacPlugin.Toggle.Off) continue;
            
            string name = pieceScript.name;
            string hoverName = pieceScript.m_name;

            if (krumpToCraftMap.Contains(name))
            {
                craftingPieces.Add(piece);
                continue;
            }

            if (krumpToMiscMap.Contains(name))
            {
                miscPieces.Add(piece);
                continue;
            }

            if (krumpToOtherMap.Contains(name))
            {
                defaultPieces.Add(piece);
                continue;
            }
            if (name == "kg_EnchantmentScrollStation")
            {
                craftingPieces.Add(piece);
                continue;
            }

            if (plant)
            {
                if (plantNames.Contains(hoverName)) continue;
                plantPieces.Add(piece);
                plantNames.Add(hoverName);
            }
            else
            {
                if (!wearNTear) continue;
                if (
                    toDefaultMap.Contains(name) 
                    || name.StartsWith("TreasureChest")
                    || name.StartsWith("loot_chest")
                    || name.StartsWith("Jewelcrafting"))
                {
                    if (defaultNames.Contains(name)) continue;
                    defaultPieces.Add(piece);
                    defaultNames.Add(name);
                }
                else if (pieceScript.m_comfort > 0)
                {
                    if (furnitureNames.Contains(hoverName)) continue;
                    furniturePieces.Add((piece));
                    furnitureNames.Add(hoverName);
                    comfortPieces.Add(piece);
                }
                else if (Regex.IsMatch(pieceScript.m_category.ToString(), @"^[-]?\d+$"))
                {
                    if (modNames.Contains(hoverName)) continue;
                    modPieces.Add(piece);
                    modNames.Add(hoverName);
                }
                else if (name == "portal") defaultPieces.Add(piece);
                else
                {
                    switch (pieceScript.m_category)
                    {
                        case Piece.PieceCategory.Misc:
                            if (miscNames.Contains(hoverName)) break;
                            miscPieces.Add(piece);
                            miscNames.Add(hoverName);
                            break;

                        case Piece.PieceCategory.Building:
                            if (buildNames.Contains(hoverName)) break;
                            buildPieces.Add(piece);
                            buildNames.Add(hoverName);
                            break;
                        
                        case Piece.PieceCategory.Crafting:
                            if (craftNames.Contains(hoverName)) break;
                            craftingPieces.Add(piece);
                            craftNames.Add(hoverName);
                            break;
                        
                        case Piece.PieceCategory.Furniture:
                            if (furnitureNames.Contains(hoverName)) break;
                            furniturePieces.Add(piece);
                            furnitureNames.Add(hoverName);
                            break;
                        
                        default:
                            if (defaultNames.Contains(hoverName)) break;
                            defaultPieces.Add(piece);
                            defaultNames.Add(hoverName);
                            break;
                    }
                }
            }
        }
    }
    private static void GetPieces()
    {
        allPieces.Clear();
        GameObject[] AllObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject GO in AllObjects)
        {
            if (!GO.GetComponent<Piece>()) continue;
            allPieces.Add(GO);
        }
    }
}