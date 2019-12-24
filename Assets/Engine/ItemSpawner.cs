using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    Map map;

    public static ItemSpawner instance;
    Dictionary<DungeonObject, int> stacksSpawned = new Dictionary<DungeonObject, int>();

    void Start()
    {
        instance = this;
        map = FindObjectOfType<Map>();
        map.OnMapLoaded += InitialSpawn;
    }

    void OnDestroy()
    {
        map.OnMapLoaded -= InitialSpawn;
    }

    void InitialSpawn()
    {
        stacksSpawned.Clear();
        map.ForEachFloorTile(SpawnItemsOnFloorTiles);
    }

    void SpawnItemsOnFloorTiles(Tile tile)
    { 
        var containingBiomes = map.biomes.Where(b => b.area.Contains(new Vector2Int(tile.x, tile.y)));
        float totalProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var itemType in biome.biomeType.items)
            {
                int numStacksAlreadyPlaced = 0;
                stacksSpawned.TryGetValue(itemType.item, out numStacksAlreadyPlaced);
                if (numStacksAlreadyPlaced < itemType.maxQuantityPerBiome)
                {
                    totalProbability += itemType.probability;
                }
            }
        }

        float r = Random.value;

        BiomeDropRate itemTypeToSpawn = null;

        float currentProbability = 0;
        float previousProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var itemType in biome.biomeType.items)
            {
                int numStacksAlreadyPlaced = 0;
                stacksSpawned.TryGetValue(itemType.item, out numStacksAlreadyPlaced);
                if (numStacksAlreadyPlaced < itemType.maxQuantityPerBiome)
                {
                    previousProbability = currentProbability;
                    currentProbability += itemType.probability;

                    if (r >= previousProbability && r < currentProbability)
                    {
                        itemTypeToSpawn = itemType;
                    }
                }
            }
        }

        if (itemTypeToSpawn != null)
        {
            Debug.Log("Spawning " + itemTypeToSpawn.item.objectName);
            var item = Instantiate(itemTypeToSpawn.item.gameObject).GetComponent<DungeonObject>();

            int quantity = Random.Range(itemTypeToSpawn.minQuantity, itemTypeToSpawn.maxQuantity + 1);
            item.quantity = quantity;

            int numStacksAlreadyPlaced = 0;
            bool anyStacksPlaced = stacksSpawned.TryGetValue(itemTypeToSpawn.item, out numStacksAlreadyPlaced);
            if (anyStacksPlaced)
            {
                stacksSpawned[itemTypeToSpawn.item]++;
            }
            else
            {
                stacksSpawned[itemTypeToSpawn.item] = 1;
            }

            tile.AddObject(item);
        }
	}
}
