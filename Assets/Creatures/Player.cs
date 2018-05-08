using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Creature playerPrefab;
    public Creature identity;
    public bool isControllingCamera = false;

    public float lastMovementFromKeyPressTime;
    LinkedList<Direction> commandQueue = new LinkedList<Direction>();
    public Map map;

    public Camera mainCamera;

	// Use this for initialization
	void Awake ()
    {
        map = FindObjectOfType<Map>().GetComponent<Map>();
        map.OnMapLoaded += OnMapLoaded;
        mainCamera.GetComponent<EntryAnimation>().OnDoneAnimating += OnEntryAnimationFinished;
	}
    
    public void OnEntryAnimationFinished()
    {
        isControllingCamera = true;
    }

    void OnMapLoaded()
    { 
        Tile startTile = map.floors[UnityEngine.Random.Range(0, map.floors.Count - 1)];

        identity = CreatureSpawner.instance.SpawnCreature(startTile.x, startTile.y, playerPrefab);
        map.Reveal(identity.x, identity.y, identity.viewDistance);

        mainCamera.GetComponent<PlayerCamera>().SetRotation(startTile.x, startTile.y, 1, float.MaxValue);
        mainCamera.GetComponent<EntryAnimation>().isAnimating = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || 
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            lastMovementFromKeyPressTime = 0;
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

        if (commandQueue.Count != 0 && (lastMovementFromKeyPressTime == 0 || Time.time - lastMovementFromKeyPressTime > .25f))
        {
            int newTileX = identity.x;
            int newTileY = identity.y;
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

            if (!map.tileObjects[newTileY][newTileX].IsCollidable())
            {
                identity.SetPosition(newTileX, newTileY);
                TimeManager.Tick(identity.ticksPerMove);
            }
            else
            {
                CollideWith(map.tileObjects[newTileY][newTileX]);
                map.tileObjects[newTileY][newTileX].Collide();
            }

            map.Reveal(identity.x, identity.y, identity.viewDistance);
            
            lastMovementFromKeyPressTime = Time.time;
        }
    }

    private void CollideWith(Tile tile)
    {
        if (tile.occupant != null)
        {
            identity.Attack(tile.occupant);

            TimeManager.Tick(identity.ticksPerAttack); 
        }
    }
}
