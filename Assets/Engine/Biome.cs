using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Biome
{

    public BiomeType biomeType;
    public RectInt area;

    Dictionary<DungeonObject, int> stacksSpawned = new Dictionary<DungeonObject, int>();

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
        int numStacksAlreadyPlaced = 0;
        var containingBiomes = tile.map.biomes.Where(b => b.area.Contains(new Vector2Int(tile.x, tile.y)));
        float totalProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var dropRate in biome.biomeType.objects)
            {
                if (dropRate.requireSpawnable && !tile.AllowsSpawn()) continue;
                if (dropRate.onlySpawnOn != null && dropRate.onlySpawnOn.Length != 0)
                {
                    bool foundRequiredObject = false;
                    foreach (var ob in dropRate.onlySpawnOn)
                    {
                        if (tile.ContainsObjectOfType(ob))
                        {
                            foundRequiredObject = true;
                            break;
                        }
                    }
                    if (!foundRequiredObject) continue;
                }
                numStacksAlreadyPlaced = 0;
                biome.stacksSpawned.TryGetValue(dropRate.item, out numStacksAlreadyPlaced);
                if (numStacksAlreadyPlaced < dropRate.maxQuantityPerBiome || dropRate.maxQuantityPerBiome == -1)
                {
                    totalProbability += dropRate.probability;
                }
            }
        }

        float r = Random.value;

        DropRate typeOfObjectToSpawn = null;

        float currentProbability = 0;
        float previousProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var dropRate in biome.biomeType.objects)
            {
                if (dropRate.requireSpawnable && !tile.AllowsSpawn()) continue;
                if (dropRate.onlySpawnOn != null && dropRate.onlySpawnOn.Length != 0)
                {
                    bool foundRequiredObject = false;
                    foreach (var ob in dropRate.onlySpawnOn)
                    {
                        if (tile.ContainsObjectOfType(ob))
                        {
                            foundRequiredObject = true;
                            break;
                        }
                    }
                    if (!foundRequiredObject) continue;
                }
                numStacksAlreadyPlaced = 0;
                biome.stacksSpawned.TryGetValue(dropRate.item, out numStacksAlreadyPlaced);
                if (numStacksAlreadyPlaced >= dropRate.maxQuantityPerBiome && dropRate.maxQuantityPerBiome != -1)
                {
                    continue;
                }

                previousProbability = currentProbability;
                currentProbability += dropRate.probability;

                if (r >= previousProbability && r < currentProbability)
                {
                    bool anyStacksPlaced = biome.stacksSpawned.TryGetValue(dropRate.item, out numStacksAlreadyPlaced);
                    if (anyStacksPlaced)
                    {
                        biome.stacksSpawned[dropRate.item]++;
                    }
                    else
                    {
                        biome.stacksSpawned[dropRate.item] = 1;
                    }

                    typeOfObjectToSpawn = dropRate;
                    break;
                }

                if (typeOfObjectToSpawn != null) break;
            }
        }

        if (typeOfObjectToSpawn != null)
        {
            int quantity = Random.Range(typeOfObjectToSpawn.minQuantity, typeOfObjectToSpawn.maxQuantity + 1);
            tile.SpawnAndAddObject(typeOfObjectToSpawn.item, quantity);
        }
    }
}
