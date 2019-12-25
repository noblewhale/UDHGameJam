using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    /*Map map;

    public List<Creature> allCreatures = new List<Creature>();

    public static CreatureSpawner instance;

    void Awake()
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
        map.ForEachTileThatAllowsSpawn(SpawnCreaturesOnFloorTiles);
    }

    void SpawnCreaturesOnFloorTiles(Tile tile)
    { 
        var containingBiomes = map.biomes.Where(b => b.area.Contains(new Vector2Int(tile.x, tile.y)));
        float totalProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var creatureType in biome.biomeType.creatures)
            {
                totalProbability += creatureType.probability;
            }
        }

        float r = Random.value;

        SpawnRate creatureTypeToSpawn = null;

        float currentProbability = 0;
        float previousProbability = 0;
        foreach (var biome in containingBiomes)
        {
            foreach (var creatureType in biome.biomeType.creatures)
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
            SpawnCreature(tile.x, tile.y, creatureTypeToSpawn.creature);
        }
	}

    public void KillAll()
    {
        foreach (var c in allCreatures)
        {
            if (c == Player.instance.identity) continue;
            c.map.tileObjects[c.y][c.x].SetOccupant(null);
            c.inventory.DestroyAll();
            Destroy(c.gameObject);
        }

        allCreatures.Clear();
    }

    public Creature SpawnCreature(int x, int y, Creature creatureType)
    {
        Creature c = Instantiate(creatureType.gameObject).GetComponent<Creature>();
        c.transform.parent = map.transform;
        c.SetPosition(x, y, false);

        allCreatures.Add(c);

        return c;
    }

    public void RemoveCreature(Creature c)
    {
        allCreatures.Remove(c);
    }
	
	// Update is called once per frame
	void Update () {
		
	}*/
}
