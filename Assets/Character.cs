using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    int tileX;
    int tileY;
    Map map;
    bool isControllingCamera = false;
    float lastMoveTime;

    LinkedList<Direction> commandQueue = new LinkedList<Direction>();

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
        map.Reveal(tileX, tileY, 4);
        Camera.main.GetComponent<EntryAnimation>().isAnimating = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || 
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            lastMoveTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            commandQueue.AddFirst(Direction.UP);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            commandQueue.AddFirst(Direction.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            commandQueue.AddFirst(Direction.RIGHT);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            commandQueue.AddFirst(Direction.LEFT);
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            commandQueue.Remove(Direction.UP);
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            commandQueue.Remove(Direction.DOWN);
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            commandQueue.Remove(Direction.RIGHT);
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            commandQueue.Remove(Direction.LEFT);
        }

        if (commandQueue.Count != 0 && (lastMoveTime == 0 || Time.time - lastMoveTime > .25f))
        {
            int newTileX = tileX;
            int newTileY = tileY;
            Direction dir = commandQueue.First.Value;
            switch (dir)
            {
                case Direction.UP: newTileY++; break;
                case Direction.DOWN: newTileY--; break;
                case Direction.RIGHT: newTileX++; break;
                case Direction.LEFT: newTileX--; break;
            }

            if (newTileX < 0) newTileX = map.width + newTileX;
            if (newTileX >= map.width) newTileX = newTileX - map.width;
            //newTileX = Mathf.Clamp(newTileX, 0, map.width - 1);
            newTileY = Mathf.Clamp(newTileY, 0, map.height - 1);

            if (map.tiles[newTileY][tileX] != 2)
            {
                tileY = newTileY;
            }
            if (map.tiles[tileY][newTileX] != 2)
            { 
                tileX = newTileX;
            }

            map.Reveal(tileX, tileY, 3);

            transform.localPosition = new Vector3(tileX * map.tileWidth, tileY * map.tileHeight, transform.localPosition.z);
            
            lastMoveTime = Time.time;
        }

        map.polarWarpMaterial.SetFloat("_Rotation", 2 * Mathf.PI * (1 - (float)(transform.localPosition.x + map.tileWidth / 2) / (map.width * map.tileWidth)) - Mathf.PI / 2);
        if (isControllingCamera)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y + cameraOffset, Camera.main.transform.position.z);
        }
    }
}
