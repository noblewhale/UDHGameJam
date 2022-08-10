using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    protected CommandQueue commandQueue = new CommandQueue();
    public Map map;

    public bool isInputEnabled = true;

    public bool isWaitingForPlayerInput = false;
    public bool hasReceivedInput = false;
    KeyCode[] allKeys;

    public virtual void Awake()
    {
        map = FindObjectOfType<Map>().GetComponent<Map>();
        allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
    }

    public virtual void ResetInput()
    {
        commandQueue.Clear();
        isInputEnabled = true;
    }

    public virtual void Update()
    {
        if (!isInputEnabled) return;
        if (!Player.instance.identity) return;
        if (!isWaitingForPlayerInput) return;

        RemoveProcessedCommands();
        CollectInputCommands();
        ProcessCommandQueue();
    }

    protected virtual void CollectInputCommands()
    {
        foreach (KeyCode key in allKeys)
        {
            if (Input.GetKeyDown(key))
            {
                commandQueue.AddIfNotExists(key);
            }
        }
    }

    protected virtual void RemoveProcessedCommands()
    {
        foreach (KeyCode key in allKeys)
        {
            if (Input.GetKeyUp(key))
            {
                commandQueue.RemoveIfExecuted(key);
            }
        }
    }

    protected virtual void ProcessCommandQueue()
    {
        if (commandQueue.Count == 0) return;

        Command command = null;
        foreach (var c in commandQueue)
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

        command.hasExecuted = true;
        if (command.shouldRemove)
        {
            commandQueue.Remove(command);
        }

        hasReceivedInput = ProcessCommand(command.key);
    }

    protected virtual bool ProcessCommand(KeyCode key)
    {
        int newTileX = Player.instance.identity.x;
        int newTileY = Player.instance.identity.y;

        bool doSomething = true;
        switch (key)
        {
            case KeyCode.W: newTileY++; break;
            case KeyCode.S: newTileY--; break;
            case KeyCode.D: newTileX++; break;
            case KeyCode.A: newTileX--; break;
            default: doSomething = false; break;
        }

        if (doSomething)
        {
            newTileX = Mathf.Clamp(newTileX, 0, map.width - 1);
            newTileY = Mathf.Clamp(newTileY, 0, map.height - 1);

            PlayerBehaviour.instance.SetNextActionTarget(newTileX, newTileY);
        }

        return doSomething;
    }
}
