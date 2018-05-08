using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public Creature[] creatureTypes;
    Map map;
    public int numCreatesToSpawn = 20;

    void Start()
    {
        map = FindObjectOfType<Map>();
        map.OnMapLoaded += () => InitialSpawn(numCreatesToSpawn);
    }

    void InitialSpawn(int numCreaturesToSpawn)
    {
        for (int i = 0; i < numCreatesToSpawn; i++)
        {
            var tile = map.floors[Random.Range(0, map.floors.Count - 1)];
            var creatureType = creatureTypes[Random.Range(0, creatureTypes.Length - 1)];
            Creature c = Instantiate(creatureType.gameObject).GetComponent<Creature>();
            c.transform.parent = map.transform;
            c.SetPosition(tile.x, tile.y);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
