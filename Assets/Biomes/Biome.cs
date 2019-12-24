using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Biome
{

    public BiomeType biomeType;
    public RectInt area;

    public static List<BiomeDropRate> GetSpawnRatesForBaseType(int x, int y, List<Biome> biomes, Map.TileType baseType)
    {
        var possibleObs = new List<BiomeDropRate>();
        foreach (var biome in biomes)
        {
            BiomeDropRate[] tileTypes = biome.biomeType.GetSpawnRatesForBaseType(baseType);

            // If no drop rates for this tile type then move on to the next biome
            if (tileTypes == null || tileTypes.Length == 0) continue;
            // If this biome does not effect this location then move on to the next one
            if (!biome.area.Contains(new Vector2Int(x, y))) continue;

            // Add to our list of possible tiles
            possibleObs.AddRange(tileTypes);
        }

        return possibleObs;
    }

    public static DungeonObject GetRandomBaseTile(int x, int y, List<Biome> biomes, Map.TileType baseType)
    {
        // Gather all possible floor tiles from each biome at this location
        var possibleObs = GetSpawnRatesForBaseType(x, y, biomes, baseType);
        return SelectRandomObject(possibleObs);
    }

    public static DungeonObject SelectRandomObject(List<BiomeDropRate> rates)
    {
        // Get the total of all probabilities
        float totalProbability = rates.Sum(pob => pob.probability);
        // Generate a random number within the probability range
        float aRandomNumber = Random.Range(0, totalProbability);

        // Randomly select an object such that objects with a higher probability are more likely to be selected
        float currentProbability = 0;
        float previousProbability = 0;
        DungeonObject selectedObject = null;
        foreach (var probability in rates)
        {
            previousProbability = currentProbability;
            currentProbability += probability.probability;
            if (aRandomNumber >= previousProbability && aRandomNumber < currentProbability)
            {
                selectedObject = probability.item;
                break;
            }
        }

        // TODO: Respect min and max and shit
        return selectedObject;
    }
}
