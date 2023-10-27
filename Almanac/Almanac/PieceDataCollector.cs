using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Almanac.Almanac;

public static class PieceDataCollector
{
    public static class IndexedPieces
    {
        public enum categories
        {
            plants,
            furniture,
            mod,
            misc,
            build,
            craft,
            defaults
        }
        public static List<GameObject> GetBuildPieces(categories option)
        {
            List<GameObject> allPieces = GetPieces(pieceOptions.allPieces);
            
            List<GameObject> plantPieces = new();
            List<GameObject> furniturePieces = new();
            List<GameObject> modPieces = new();
            List<GameObject> miscPieces = new();
            List<GameObject> buildPieces = new();
            List<GameObject> craftingPieces = new();
            List<GameObject> defaultPieces = new();

            HashSet<string> plantNames = new HashSet<string>();
            HashSet<string> furnitureNames = new HashSet<string>();
            HashSet<string> modNames = new HashSet<string>();
            HashSet<string> miscNames = new HashSet<string>();
            HashSet<string> buildNames = new HashSet<string>();
            HashSet<string> craftNames = new HashSet<string>();
            HashSet<string> defaultNames = new HashSet<string>();

            List<string> exclusionMap = new List<string>()
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

                string name = pieceScript.name;
                string hoverName = pieceScript.m_name;
                
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
                        exclusionMap.Contains(name) 
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

            switch (option)
            {
                case categories.build:
                    return buildPieces;
                case categories.craft:
                    return craftingPieces;
                case categories.defaults:
                    return defaultPieces;
                case categories.furniture:
                    return furniturePieces;
                case categories.misc:
                    return miscPieces;
                case categories.mod:
                    return modPieces;
                case categories.plants:
                    return plantPieces;
                default:
                    return allPieces;
            }
        }

        public enum pieceOptions
        {
            allPieces,
            cookStations,
            fermentStations,
            smeltStations
        }
        public static List<GameObject> GetPieces(pieceOptions option)
        {
            GameObject[] AllObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            
            List<GameObject> allPieces = new List<GameObject>();
            List<GameObject> cookingStations = new List<GameObject>();
            List<GameObject> fermentingStations = new List<GameObject>();
            List<GameObject> smelterStations = new List<GameObject>();
            
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

            switch (option)
            {
                case pieceOptions.allPieces:
                    return allPieces;
                case pieceOptions.cookStations:
                    return cookingStations;
                case pieceOptions.fermentStations:
                    return fermentingStations;
                case pieceOptions.smeltStations:
                    return smelterStations;
                default:
                    return allPieces;
            }
        }
    }
}