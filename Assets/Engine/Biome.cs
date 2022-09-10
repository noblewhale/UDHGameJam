using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Noble.TileEngine
{
    [CreateAssetMenu]
    public class Biome : ScriptableObject
    {
        public float nothingProbability = 0;
        public BiomeDropRate[] objects;

        public int minX, maxX;
        public int minY, maxY;
        public int minWidth, maxWidth;
        public int minHeight, maxHeight;

        public List<Biome> subBiomes = new List<Biome>();

        Dictionary<DungeonObject, int> stacksSpawned = new Dictionary<DungeonObject, int>();

        virtual public IEnumerator PreProcessMap(Map map, BiomeObject biomeObject)
        {
            //foreach (var subBiome in subBiomes)
            //{
            //    yield return map.StartCoroutine(subBiome.PreProcessMap(map, area));
            //}
            yield return null;
        }

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

        public static void GatherBiomes(Vector2Int pos, ref List<BiomeObject> biomes, BiomeObject parentBiome)
        {
            var subBiomesContainingXY = parentBiome.subBiomes.Where(b => b.area.Contains(pos));
            foreach (var subBiome in subBiomesContainingXY)
            {
                biomes.Add(subBiome);
                GatherBiomes(pos, ref biomes, subBiome);
            }
        }

        public static void SpawnRandomObject(Tile tile)
        {
            int numStacksAlreadyPlaced = 0;
            var containingBiomes = tile.map.biomes.Where(b => b.Contains(tile.transform.position)).ToList();
            var subBiomes = new List<BiomeObject>();
            foreach (var biome in containingBiomes)
            {
                GatherBiomes(tile.position, ref subBiomes, biome);
            }
            containingBiomes.AddRange(subBiomes);
            float totalProbability = 0;

            List<BiomeRate> viableDrops = new List<BiomeRate>();
            foreach (var biome in containingBiomes)
            {
                if (biome.biome == null) continue;

                totalProbability += biome.biome.nothingProbability;
                if (biome.biome.nothingProbability != 0)
                {
                    var biomeRate = new BiomeRate(biome.biome, null);
                    viableDrops.Add(biomeRate);
                }
                foreach (var dropRate in biome.biome.objects)
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
                    biome.biome.stacksSpawned.TryGetValue(dropRate.item, out numStacksAlreadyPlaced);
                    if (numStacksAlreadyPlaced < dropRate.maxQuantityPerBiome || dropRate.maxQuantityPerBiome == -1)
                    {
                        var biomeRate = new BiomeRate(biome.biome, dropRate);
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
                    currentProbability += biome.nothingProbability;
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
                tile.SpawnAndAddObject(typeOfObjectToSpawn.item, quantity, 2);
            }
        }

        virtual public void DrawDebug(RectIntExclusive area)
        {
            EditorUtil.DrawRect(Map.instance, area, Color.green);
        }
    }
}