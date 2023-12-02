using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Almanac.Almanac;

public static class PieceDataCollector
{
    // private static readonly List<string> exclusionMap = AlmanacPlugin._IgnoredPrefabs.Value.Split(',').ToList();
    private static readonly List<string> exclusionMap = IgnoreList.serverIgnoreList;

    public static readonly List<GameObject> plantPieces = new();
    public static readonly List<GameObject> furniturePieces = new();
    public static readonly List<GameObject> modPieces = new();
    public static readonly List<GameObject> miscPieces = new();
    public static readonly List<GameObject> buildPieces = new();
    public static readonly List<GameObject> craftingPieces = new();
    public static readonly List<GameObject> defaultPieces = new();

    public static readonly List<GameObject> comfortPieces = new();
    
    public static readonly List<GameObject> allPieces = new ();
    public static readonly List<GameObject> cookingStations = new ();
    public static readonly List<GameObject> fermentingStations = new ();
    public static readonly List<GameObject> smelterStations = new ();

    public static void GetBuildPieces()
    {
        GetPieces();
        
        plantPieces.Clear();
        furniturePieces.Clear();
        modPieces.Clear();
        miscPieces.Clear();
        buildPieces.Clear();
        craftingPieces.Clear();
        defaultPieces.Clear();
        
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
        
        foreach (GameObject piece in allPieces)
        {
            piece.TryGetComponent(out Piece pieceScript);
            piece.TryGetComponent(out WearNTear wearNTear);
            piece.TryGetComponent(out Plant plant);
            
            if (!pieceScript) continue;

            // Piece.PieceCategory category = pieceScript.m_category;
            // AlmanacPlugin.AlmanacLogger.LogWarning(category.ToString());
            
            if (exclusionMap.Contains(piece.name)) continue;
            
            string name = pieceScript.name;
            string hoverName = pieceScript.m_name;

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
        cookingStations.Clear();
        fermentingStations.Clear();
        smelterStations.Clear();

        GameObject[] AllObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject GO in AllObjects)
        {
            GO.TryGetComponent(out Piece pieceScript);
            GO.TryGetComponent(out CookingStation cookingStationScript);
            GO.TryGetComponent(out Fermenter fermentScript);
            GO.TryGetComponent(out Smelter smelterScript);
            
            if (pieceScript != null) allPieces.Add(GO);
            if (cookingStationScript != null) cookingStations.Add(GO);
            if (fermentScript != null) fermentingStations.Add(GO);
            if (smelterScript != null) smelterStations.Add(GO);
        }
    }
}