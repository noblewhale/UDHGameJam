using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public Creature[] creatureTypes;
    Map map;
    public int numCreatesToSpawn = 20;

    public List<Creature> allCreatures = new List<Creature>();

    public static CreatureSpawner instance;

    void Start()
    {
        instance = this;
        map = FindObjectOfType<Map>();
        map.OnMapLoaded += () => InitialSpawn(numCreatesToSpawn);
    }

    void InitialSpawn(int numCreaturesToSpawn)
    {
        for (int i = 0; i < numCreatesToSpawn; i++)
        {
            var tile = map.floors[Random.Range(0, map.floors.Count)];
            var creatureType = creatureTypes[Random.Range(0, creatureTypes.Length)];

            SpawnCreature(tile.x, tile.y, creatureType);
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
		
	}
}
