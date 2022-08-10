using System;
using UnityEngine;

public class RoguePlayerInput : PlayerInput
{
    public Camera mapCamera;
    public Transform mapRenderer;

    protected override void CollectInputCommands()
    {
        base.CollectInputCommands();

        bool hasPress = Input.GetMouseButtonDown(0);

        if (hasPress)
        {
            Vector2 relativeWorldPos = PolarMapUtil.GetPositionRelativeToMap(Input.mousePosition);
            Vector2 unwarpedPos;
            bool success = PolarMapUtil.UnwarpPosition(relativeWorldPos, out unwarpedPos);

            if (success)
            {
                bool isInsideMap = PolarMapUtil.PositionToTile(unwarpedPos, out int tileX, out int tileY);
                if (isInsideMap)
                {
                    int xDif = tileX - Player.instance.identity.x;
                    int yDif = tileY - Player.instance.identity.y;
                    if (Math.Abs(xDif) == Math.Abs(yDif))
                    {
                        if (xDif > 0 && yDif > 0) commandQueue.AddIfNotExists(KeyCode.Keypad9);
                        else if (xDif > 0 && yDif < 0) commandQueue.AddIfNotExists(KeyCode.Keypad3);
                        else if (xDif < 0 && yDif < 0) commandQueue.AddIfNotExists(KeyCode.Keypad1);
                        else if (xDif < 0 && yDif > 0) commandQueue.AddIfNotExists(KeyCode.Keypad7);
                    }
                    else if (Math.Abs(xDif) > Math.Abs(yDif))
                    {
                        if (xDif > 0) commandQueue.AddIfNotExists(KeyCode.D, true);
                        else commandQueue.AddIfNotExists(KeyCode.A, true);
                    }
                    else // if (Math.Abs(xDif) < Math.Abs(yDif))
                    {
                        if (yDif > 0) commandQueue.AddIfNotExists(KeyCode.W, true);
                        else commandQueue.AddIfNotExists(KeyCode.S, true);
                    }
                }
            }
        }
    }

    protected override bool ProcessCommand(KeyCode key)
    {
        int newTileX = Player.instance.identity.x;
        int newTileY = Player.instance.identity.y;

        bool doSomething = true;
        switch (key)
        {
            case KeyCode.UpArrow:
            case KeyCode.W:
            case KeyCode.Keypad8:
                newTileY++; 
                break;
            case KeyCode.DownArrow:
            case KeyCode.S:
            case KeyCode.Keypad2:
                newTileY--; 
                break;
            case KeyCode.RightArrow:
            case KeyCode.D:
            case KeyCode.Keypad6:
                newTileX++; 
                break;
            case KeyCode.LeftArrow:
            case KeyCode.A:
            case KeyCode.Keypad4:
                newTileX--; 
                break;
            case KeyCode.Keypad9:
                newTileY++;
                newTileX++;
                break;
            case KeyCode.Keypad7:
                newTileY++;
                newTileX--;
                break;
            case KeyCode.Keypad1:
                newTileY--;
                newTileX--;
                break;
            case KeyCode.Keypad3:
                newTileY--;
                newTileX++;
                break;
            default: doSomething = false; break;
        }

        if (doSomething)
        {
            newTileX = map.WrapX(newTileX);
            newTileY = Mathf.Clamp(newTileY, 0, map.height - 1);

            PlayerBehaviour.instance.SetNextActionTarget(newTileX, newTileY);
        }

        return doSomething;
    }
}
