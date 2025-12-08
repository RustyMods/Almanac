using UnityEngine;

namespace Almanac.Utilities;

public static class RandomLocationFinder
{
    private const float maxRadius = 9500f;
    private static Vector3 GetRandomVectorWithin(Vector3 point, float margin)
    {
        Vector2 vector2 = UnityEngine.Random.insideUnitCircle * margin;
        return point + new Vector3(vector2.x, 0.0f, vector2.y);
    }
    public static bool FindSpawnLocation(Heightmap.Biome biome, float range, float increment, out Vector3 position)
    {
        position = Vector3.zero;
        // First try near player
        for (int index = 0; index < 1000; ++index)
        {
            Vector3 candidatePos = GetRandomVectorWithin(Player.m_localPlayer.transform.position, range);
            
            if (IsValidSpawnLocation(biome, candidatePos))
            {
                position = candidatePos;
                return true;
            }

            range += increment; // increment range for each failed random position
        }
        // Then try entire world
        for (int index = 0; index < 1000; ++index)
        {
            Vector3 candidatePos = GetRandomVector();
            
            if (IsValidSpawnLocation(biome, candidatePos))
            {
                position = candidatePos;
                return true;
            }
        }
        return false;
    }

    private static bool IsValidSpawnLocation(Heightmap.Biome biome, Vector3 candidatePos)
    {
        Heightmap.Biome candidateBiome = WorldGenerator.instance.GetBiome(candidatePos);
        if (!biome.HasFlag(candidateBiome)) return false;
            
        if (WorldGenerator.instance.GetBiomeArea(candidatePos) is not Heightmap.BiomeArea.Median)
            return false;
        
        if (biome != Heightmap.Biome.Ocean && IsInWater(candidatePos))
            return false;

        if (biome is Heightmap.Biome.AshLands && ZoneSystem.instance.IsLava(candidatePos)) return false;
        
        return true;
    }

    private static bool IsInWater(Vector3 position)
    {
        float groundHeight = WorldGenerator.instance.GetHeight(position.x, position.z);
        float waterLevel = ZoneSystem.instance.m_waterLevel;
        if (groundHeight < waterLevel) return true;
        if (IsMarkedAsWater(position)) return true;
        return false;
    }
    private static bool IsMarkedAsWater(Vector3 position)
    {
        return IsSurroundedByWater(position);
    }
    private static bool IsSurroundedByWater(Vector3 position, int samples = 8, float radius = 50f)
    {
        int waterSamples = 0;
        
        for (int i = 0; i < samples; i++)
        {
            float angle = (360f / samples) * i * Mathf.Deg2Rad;
            Vector3 samplePos = position + new Vector3(
                Mathf.Cos(angle) * radius, 
                0, 
                Mathf.Sin(angle) * radius
            );
            
            float groundHeight = WorldGenerator.instance.GetHeight(samplePos.x, samplePos.z);
            float waterLevel = ZoneSystem.instance.m_waterLevel;
            
            if (groundHeight < waterLevel)
                waterSamples++;
        }
        return waterSamples > (samples / 2);
    }
    private static Vector3 GetRandomVector()
    {
        float x = UnityEngine.Random.Range(-maxRadius, maxRadius);
        float z = UnityEngine.Random.Range(-maxRadius, maxRadius);
        float y = WorldGenerator.instance.GetHeight(x, z);
        return new Vector3(x, y + 1f, z);
    }
}