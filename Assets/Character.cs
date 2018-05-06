using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    int tileX;
    int tileY;
    Map map;

	// Use this for initialization
	void Awake ()
    {
        map = FindObjectOfType<Map>();
        map.OnMapLoaded += OnMapLoaded;
        transform.parent = map.transform;
	}

    void OnMapLoaded()
    {
        Tile startTile = map.floors[Random.Range(0, map.floors.Count - 1)];
        tileX = startTile.x;
        tileY = startTile.y;
    }
	
	// Update is called once per frame
	void Update ()
    {
        int newTileX = tileX;
        int newTileY = tileY;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            newTileY++;
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            newTileY--;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            newTileX++;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            newTileX--;
        }

        newTileX = Mathf.Clamp(newTileX, 0, map.width - 1);
        newTileY = Mathf.Clamp(newTileY, 0, map.height - 1);

        if (map.tiles[newTileY][newTileX] != 2)
        {
            tileX = newTileX;
            tileY = newTileY;
        }

        transform.localPosition = new Vector3(tileX * map.tileWidth, tileY * map.tileHeight, transform.localPosition.z);
    }
}
