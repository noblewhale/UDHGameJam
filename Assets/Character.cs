using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    int tileX;
    int tileY;
    Map map;
    bool isControllingCamera = false;

    public int cameraOffset = 3;

	// Use this for initialization
	void Awake ()
    {
        map = FindObjectOfType<Map>();
        map.OnMapLoaded += OnMapLoaded;
        transform.parent = map.transform;
        Camera.main.GetComponent<EntryAnimation>().OnDoneAnimating += OnEntryAnimationFinished;
	}
    
    public void OnEntryAnimationFinished()
    {
        isControllingCamera = true;
    }

    void OnMapLoaded()
    { 
        Tile startTile = map.floors[Random.Range(0, map.floors.Count - 1)];
        tileX = startTile.x;
        tileY = startTile.y;
        transform.localPosition = new Vector3(tileX * map.tileWidth, tileY * map.tileHeight, transform.localPosition.z);
        Camera.main.GetComponent<EntryAnimation>().isAnimating = true;
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
        map.polarWarpMaterial.SetFloat("_Rotation", 2 * Mathf.PI * (1 - (float)(transform.localPosition.x + map.tileWidth / 2) / (map.width*map.tileWidth)) - Mathf.PI / 2);
        if (isControllingCamera)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y + cameraOffset, Camera.main.transform.position.z);
        }
    }
}
