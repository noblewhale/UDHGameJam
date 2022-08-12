﻿namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    internal class RoguePlayerTickable : Tickable
    {
        public AttackBehaviour attackBehaviour;
        public MoveBehaviour moveBehaviour;
        public FireAOEBehaviour fireAOEBehaviour;

        override public TickableBehaviour DetermineBehaviour()
        {
            Command command = PlayerInput.instance.commandQueue.Dequeue();

            if (command.key == KeyCode.F)
            {
                return fireAOEBehaviour;
            }
            else
            {
                return DefaultAction(command);
            }
        }

        public TickableBehaviour DefaultAction(Command command)
        {
            int newTileX = Player.instance.identity.x;
            int newTileY = Player.instance.identity.y;

            bool doSomething = true;

            KeyCode key = command.key;

            if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1)
            {
                Vector2 relativeWorldPos = PolarMapUtil.GetPositionRelativeToMap(command.target);
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
                            if (xDif > 0 && yDif > 0) key = KeyCode.Keypad9;
                            else if (xDif > 0 && yDif < 0) key = KeyCode.Keypad3;
                            else if (xDif < 0 && yDif < 0) key = KeyCode.Keypad1;
                            else if (xDif < 0 && yDif > 0) key = KeyCode.Keypad7;
                        }
                        else if (Math.Abs(xDif) > Math.Abs(yDif))
                        {
                            if (xDif > 0) key = KeyCode.D;
                            else key = KeyCode.A;
                        }
                        else // if (Math.Abs(xDif) < Math.Abs(yDif))
                        {
                            if (yDif > 0) key = KeyCode.W;
                            else key = KeyCode.S;
                        }
                    }
                }
            }

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
                newTileX = Map.instance.WrapX(newTileX);
                newTileY = Mathf.Clamp(newTileY, 0, Map.instance.height - 1);
            }
            else
            {
                owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
                return null;
            }

            var tileActingOn = owner.map.tileObjects[newTileY][newTileX];

            if (!tileActingOn.IsCollidable())
            {
                moveBehaviour.targetX = newTileX;
                moveBehaviour.targetY = newTileY;
                return moveBehaviour;
            }
            else
            {
                attackBehaviour.targetTile = tileActingOn;
                return attackBehaviour;
            }
        }
    }
}