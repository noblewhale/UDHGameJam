using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    public DungeonObject playerPrefab;
    public DungeonObject identity;
    public bool isControllingCamera = false;

    public float lastMovementFromKeyPressTime;
    CommandQueue commandQueue = new CommandQueue();
    public Map map;

    public Camera mainCamera;

    public static Player instance;
    public event Action onPlayerActed;

    public bool isInputEnabled = true;

    public PlayerBehaviour playerBehaviour;

    public bool isWaitingForPlayerInput = false;
    public bool hasReceivedInput = false;

    void Awake ()
    {
        instance = this;
        map = FindObjectOfType<Map>().GetComponent<Map>();
        map.OnMapLoaded += OnMapLoaded;
        mainCamera.GetComponent<EntryAnimation>().OnDoneAnimating += OnEntryAnimationFinished;
	}

    void Start()
    {
    }

    void OnPositionChange(int oldX, int oldY, int newX, int newY)
    {
        map.Reveal(newX, newY, identity.viewDistance);
    }

    public void OnEntryAnimationFinished()
    {
        isControllingCamera = true;
    }

    void OnMapLoaded()
    {
        if (identity == null)
        {
            identity = Instantiate(playerPrefab);
            foreach (var behaviour in identity.GetComponents<TickableBehaviour>())
            {
                behaviour.enabled = false;
            }
            playerBehaviour = identity.GetComponent<Tickable>().AddBehaviour<PlayerBehaviour>();
            identity.onSetPosition += OnPositionChange;
        }

        Tile startTile = map.GetRandomTileThatAllowsSpawn();
        startTile.AddObject(identity);
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
	
	void Update ()
    {
        if (!isInputEnabled || !identity) return;

        if (Input.inputString != String.Empty ||
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!isWaitingForPlayerInput)
            {
                TimeManager.instance.Interrupt(identity.GetComponent<Tickable>());
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
        if (commandQueue.Count != 0)
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
                return;
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

            playerBehaviour.SetNextActionTarget(newTileX, newTileY);

            lastMovementFromKeyPressTime = Time.time;

            hasReceivedInput = true;

            if (onPlayerActed != null) onPlayerActed();
        }
    }
}
