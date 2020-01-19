using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Biome
{

    public BiomeType biomeType;
    public RectIntExclusive area;

    Dictionary<DungeonObject, int> stacksSpawned = new Dictionary<DungeonObject, int>();

    public static DungeonObject SelectRandomObject(BiomeDropRate[] rates)
    {
        // Get the total of all probabilities
        float totalProbability = rates.Sum(pob => pob.probability);
        // Generate a random number within the probability range
        float aRandomNumber = UnityEngine.Random.Range(0, totalProbability);

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
    struct BiomeRate
    {
        public Biome biome;
        public BiomeDropRate dropRate;

        public BiomeRate(Biome biome, BiomeDropRate rate)
        {
            this.biome = biome;
            this.dropRate = rate;
        }
    }

    public static void SpawnRandomObject(Tile tile)
    {
        int numStacksAlreadyPlaced = 0;
        var containingBiomes = tile.map.biomes.Where(b => b.area.Contains(new Vector2Int(tile.x, tile.y)));
        float totalProbability = 0;
    
        List<BiomeRate> viableDrops = new List<BiomeRate>();
        foreach (var biome in containingBiomes)
        {
            totalProbability += biome.biomeType.nothingProbability;
            if (biome.biomeType.nothingProbability != 0)
            {
                var biomeRate = new BiomeRate(biome, null);
                viableDrops.Add(biomeRate);
            }
            foreach (var dropRate in biome.biomeType.objects)
            {
                if (dropRate.requireSpawnable && !tile.AllowsSpawn()) continue;
                if (dropRate.onlySpawnOn != null && dropRate.onlySpawnOn.Length != 0)
                {
                    if (!tile.ContainsObjectOfType(dropRate.onlySpawnOn))
                    {
                        continue;
                    }
                }
                if (dropRate.dontSpawnOn != null && dropRate.dontSpawnOn.Length != 0)
                {
                    if (tile.ContainsObjectOfType(dropRate.dontSpawnOn))
                    {
                        continue;
                    }
                }
                numStacksAlreadyPlaced = 0;
                biome.stacksSpawned.TryGetValue(dropRate.item, out numStacksAlreadyPlaced);
                if (numStacksAlreadyPlaced < dropRate.maxQuantityPerBiome || dropRate.maxQuantityPerBiome == -1)
                {
                    var biomeRate = new BiomeRate(biome, dropRate);
                    viableDrops.Add(biomeRate);
                    totalProbability += dropRate.probability;
                }
            }
        }

        float r = UnityEngine.Random.Range(0, totalProbability);

        DropRate typeOfObjectToSpawn = null;

        float currentProbability = 0;
        float previousProbability = 0;
        foreach (var biomeRate in viableDrops)
        {
            Biome biome = biomeRate.biome;
            BiomeDropRate dropRate = biomeRate.dropRate;
            if (dropRate == null)
            {
                previousProbability = currentProbability;
                currentProbability += biome.biomeType.nothingProbability;
            }
            else
            {
                previousProbability = currentProbability;
                currentProbability += dropRate.probability;
            }
            if (r >= previousProbability && r < currentProbability)
            {
                if (dropRate == null)
                {
                    // Spawn nothing
                    break;
                }
                else
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
            }
        }

        if (typeOfObjectToSpawn != null)
        {
            int quantity = UnityEngine.Random.Range(typeOfObjectToSpawn.minQuantity, typeOfObjectToSpawn.maxQuantity + 1);
            tile.SpawnAndAddObject(typeOfObjectToSpawn.item, quantity);
        }
    }

    internal void DrawDebug()
    {
        biomeType.DrawDebug(area);
    }
}
