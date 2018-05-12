using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public Creature playerPrefab;
    public Creature identity;
    public bool isControllingCamera = false;

    public float lastMovementFromKeyPressTime;
    CommandQueue commandQueue = new CommandQueue();
    public Map map;

    public Camera mainCamera;

    public static Player instance;

    public bool isInputEnabled = true;
    
	void Awake ()
    {
        instance = this;
        map = FindObjectOfType<Map>().GetComponent<Map>();
        map.OnMapLoaded += OnMapLoaded;
        mainCamera.GetComponent<EntryAnimation>().OnDoneAnimating += OnEntryAnimationFinished;
	}

    void Start()
    {
        if (identity == null)
        {
            identity = CreatureSpawner.instance.SpawnCreature(0, 0, playerPrefab);
        }
    }

    public void OnEntryAnimationFinished()
    {
        isControllingCamera = true;
    }

    void OnMapLoaded()
    { 
        Tile startTile = map.floors[UnityEngine.Random.Range(0, map.floors.Count - 1)];
        identity.SetPosition(startTile.x, startTile.y, false);
        map.Reveal(identity.x, identity.y, identity.viewDistance);

        mainCamera.GetComponent<PlayerCamera>().SetRotation(startTile.x, startTile.y, float.Epsilon, float.MaxValue);
        if (map.dungeonLevel == 1)
        {
            mainCamera.GetComponent<EntryAnimation>().isAnimating = true;
        }
        else
        {
            mainCamera.GetComponent<PlayerCamera>().SetY(identity.transform.position.y, 1, float.MaxValue);
        }
    }

    public void ResetInput()
    {
        lastMovementFromKeyPressTime = 0;
        commandQueue.Clear();
        lastInputString = "";
        isInputEnabled = true;
    }

    string lastInputString = "";
	
	// Update is called once per frame
	void Update ()
    {
        if (!isInputEnabled) return;

        if (Input.inputString != String.Empty ||
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {

            if (TimeManager.isBetweenTicks)
            {
                TimeManager.Interrupt();
            }
            lastMovementFromKeyPressTime = 0;
        }
        foreach (var c in Input.inputString)
        {
            KeyCode k = (KeyCode)Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
            commandQueue.AddIfNotExists(k);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            commandQueue.AddIfNotExists(KeyCode.W);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            commandQueue.AddIfNotExists(KeyCode.S);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            commandQueue.AddIfNotExists(KeyCode.D);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            commandQueue.AddIfNotExists(KeyCode.A);
        }
        foreach (var c in lastInputString)
        {
            if (!Input.inputString.Contains(c))
            {
                KeyCode k = (KeyCode)Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
                commandQueue.RemoveIfExecuted(k);
            }
        }
        lastInputString = Input.inputString;
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            commandQueue.RemoveIfExecuted(KeyCode.W);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            commandQueue.RemoveIfExecuted(KeyCode.S);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            commandQueue.RemoveIfExecuted(KeyCode.D);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            commandQueue.RemoveIfExecuted(KeyCode.A);
        }
        if (commandQueue.Count != 0 && (lastMovementFromKeyPressTime == 0 || Time.time - lastMovementFromKeyPressTime > .25f))
        {
            int newTileX = identity.x;
            int newTileY = identity.y;

            Command command = null;
            foreach(var c in commandQueue)
            {
                if (!c.hasExecuted)
                {
                    command = c;
                    break;
                }
            }

            if (command == null)
            {
                command = commandQueue.Last.Value;
            }

            bool doSomething = true;
            switch (command.key)
            {
                case KeyCode.W: newTileY++; break;
                case KeyCode.S: newTileY--; break;
                case KeyCode.D: newTileX++; break;
                case KeyCode.A: newTileX--; break;
                default: doSomething = false; break;
            }

            command.hasExecuted = true;
            if (command.shouldRemove)
            {
                commandQueue.Remove(command);
            }

            if (!doSomething) return;

            newTileX = map.WrapX(newTileX);
            newTileY = Mathf.Clamp(newTileY, 0, map.height - 1);

            if (!map.tileObjects[newTileY][newTileX].IsCollidable())
            {
                identity.SetPosition(newTileX, newTileY);
                identity.PickUpAll();
                TimeManager.Tick(identity.ticksPerMove);
            }
            else
            {
                CollideWith(map.tileObjects[newTileY][newTileX]);
                map.tileObjects[newTileY][newTileX].Collide(identity);
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
        else 
        {
            if (tile.ContainsObjectOfType(map.objectSet[3]))
            {
                TimeManager.Tick(identity.ticksPerMove);
            }
            identity.FaceDirection(tile);
            identity.AttackAnimation(.6f, .25f);
        }
    }
}
