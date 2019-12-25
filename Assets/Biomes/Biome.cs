using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Biome
{

    public BiomeType biomeType;
    public RectInt area;

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

    public static void SpawnRandomObject(Tile tile)
    {
        var containingBiomes = tile.map.biomes.Where(b => b.area.Contains(new Vector2Int(tile.x, tile.y)));
        float totalProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var dropRate in biome.biomeType.objects)
            {
                totalProbability += dropRate.probability;
            }
        }

        float r = Random.value;

        DropRate creatureTypeToSpawn = null;

        float currentProbability = 0;
        float previousProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var creatureType in biome.biomeType.objects)
            {
                previousProbability = currentProbability;
                currentProbability += creatureType.probability;

                if (r >= previousProbability && r < currentProbability)
                {
                    creatureTypeToSpawn = creatureType;
                }
            }
        }

        if (creatureTypeToSpawn != null)
        {
            tile.SpawnAndAddObject(creatureTypeToSpawn.item);
        }
    }
}
