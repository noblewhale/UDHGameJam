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

    public Transform mapRenderer;
    public Camera mapCamera;

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
        map.UpdateLighting();
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
        map.UpdateLighting();
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

        bool hasPress = Input.GetMouseButtonDown(0);

        if (Input.inputString != String.Empty ||
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            hasPress)
        {
            if (!isWaitingForPlayerInput)
            {
                //TimeManager.instance.Interrupt(identity.GetComponent<Tickable>());
            }
            else
            {
                lastMovementFromKeyPressTime = 0;
            }
        }
        foreach (var c in Input.inputString)
        {
            KeyCode k = (KeyCode)Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
            commandQueue.AddIfNotExists(k);
        }

        bool pressUp = false;
        bool pressDown = false;
        bool pressLeft = false;
        bool pressRight = false;
        
        if (hasPress)
        {
            Vector2 pressPosRelativeToMapRenderer = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)Camera.main.transform.position) - (Vector2)mapRenderer.localPosition;
            pressPosRelativeToMapRenderer /= mapRenderer.localScale;
            Vector2 rotated = new Vector2();
            float rotation = mapRenderer.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Rotation");
            rotated.x = pressPosRelativeToMapRenderer.x * Mathf.Sin(rotation) - pressPosRelativeToMapRenderer.y * Mathf.Cos(rotation);
            rotated.y = pressPosRelativeToMapRenderer.x * Mathf.Cos(rotation) + pressPosRelativeToMapRenderer.y * Mathf.Sin(rotation);
            pressPosRelativeToMapRenderer = rotated;
            Vector2 unwarpedPos = new Vector2();
            float d = pressPosRelativeToMapRenderer.magnitude / .5f;
            float _SeaLevel = .27f;
            if (d < .1f)
            {
                pressUp = true;
            }
            else
            {
                d = (d - .1f) / .9f;
                d = Mathf.Log(1 + d / _SeaLevel) / Mathf.Log(1 + 1 / _SeaLevel);
                unwarpedPos.y = 1 - d;
                Vector2 normalized = pressPosRelativeToMapRenderer.normalized;
                float angle = Mathf.Acos(Vector2.Dot(normalized, Vector2.up));
                Vector3 check = Vector3.Cross(normalized, Vector3.up);
                if (check.z < 0) angle = 2 * Mathf.PI - angle;
                unwarpedPos.x = angle / (2 * Mathf.PI);
                unwarpedPos = unwarpedPos - Vector2.one * .5f;
                unwarpedPos.x *= -1;
                Vector2 unwarpedMapSpace = unwarpedPos * new Vector2(mapCamera.orthographicSize * 2 * mapCamera.aspect, mapCamera.orthographicSize * 2);
                unwarpedMapSpace += (Vector2)mapCamera.transform.position;

                var relativeToPlayer = unwarpedMapSpace - (Vector2)identity.transform.position;
                if (relativeToPlayer.x > Map.instance.TotalWidth / 2) relativeToPlayer.x = -(Map.instance.TotalWidth - unwarpedMapSpace.x) - identity.transform.position.x;
                if (relativeToPlayer.x < -Map.instance.TotalWidth / 2) relativeToPlayer.x = (Map.instance.TotalWidth + unwarpedMapSpace.x) - identity.transform.position.x;
                if (Mathf.Abs(relativeToPlayer.x) > Mathf.Abs(relativeToPlayer.y))
                {
                    if (relativeToPlayer.x > 0) pressRight = true;
                    else pressLeft = true;
                }
                else
                {
                    if (relativeToPlayer.y > 0) pressUp = true;
                    else pressDown = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || pressUp)
        {
            commandQueue.AddIfNotExists(KeyCode.W);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || pressDown)
        {
            commandQueue.AddIfNotExists(KeyCode.S);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || pressRight)
        {
            commandQueue.AddIfNotExists(KeyCode.D);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || pressLeft)
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
