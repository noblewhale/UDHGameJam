using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    protected CommandQueue commandQueue = new CommandQueue();
    public Map map;

    public event Action onPlayerActed;

    public bool isInputEnabled = true;

    public bool isWaitingForPlayerInput = false;
    public bool hasReceivedInput = false;

    public virtual void Awake()
    {
        map = FindObjectOfType<Map>().GetComponent<Map>();
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

        CollectInputCommands();
        ProcessCommandQueue();
        RemoveProcessedCommands();
    }

    protected virtual void CollectInputCommands()
    {
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

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad1);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad2);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad3);
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad4);
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad5);
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad6);
        }
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad7);
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad8);
        }
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            commandQueue.AddIfNotExists(KeyCode.Keypad9);
        }
    }

    protected virtual void RemoveProcessedCommands()
    { 
        foreach (var c in Input.inputString)
        {
            if (!Input.inputString.Contains(c))
            {
                KeyCode k = (KeyCode)Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
                commandQueue.RemoveIfExecuted(k);
            }
        }
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

        if (Input.GetKeyUp(KeyCode.Keypad0))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad0);
        }
        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad1);
        }
        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad2);
        }
        if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad3);
        }
        if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad4);
        }
        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad5);
        }
        if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad6);
        }
        if (Input.GetKeyUp(KeyCode.Keypad7))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad7);
        }
        if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad8);
        }
        if (Input.GetKeyUp(KeyCode.Keypad9))
        {
            commandQueue.RemoveIfExecuted(KeyCode.Keypad9);
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

        bool doSomething = ProcessCommand(command.key);

        if (doSomething)
        {
            hasReceivedInput = true;

            if (onPlayerActed != null) onPlayerActed();
        }
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

            Player.instance.playerBehaviour.SetNextActionTarget(newTileX, newTileY);
        }

        return doSomething;
    }
}
