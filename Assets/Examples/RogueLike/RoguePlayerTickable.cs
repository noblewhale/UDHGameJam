namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    internal class RoguePlayerTickable : Tickable
    {
        public AttackBehaviour attackBehaviour;
        public MoveBehaviour moveBehaviour;
        public FireAOEBehaviour fireAOEBehaviour;
        public IceRayBehaviour iceRayBehaviour;

        override public TickableBehaviour DetermineBehaviour()
        {
            Command command = PlayerInputHandler.instance.commandQueue.Dequeue();

            if (command.key == Key.F)
            {
                return fireAOEBehaviour;
            }
            if (command.key == Key.G)
            {
                return iceRayBehaviour;
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

            Key key = command.key;
            var mouseButton = command.mouseButton;

            if (mouseButton == Mouse.current.leftButton || mouseButton == Mouse.current.rightButton)
            {
                Vector2 relativeWorldPos = PolarMapUtil.GetPositionRelativeToMap(command.target);
                Vector2 unwarpedPos;
                bool success = PolarMapUtil.UnwarpPosition(relativeWorldPos, out unwarpedPos);

                if (success)
                {
                    bool isInsideMap = PolarMapUtil.PositionToTile(unwarpedPos, out int tileX, out int tileY);
                    if (isInsideMap)
                    {
                        int xDif = Map.instance.GetXDifference(Player.instance.identity.x, tileX);
                        int yDif = tileY - Player.instance.identity.y;
                        if (Math.Abs(xDif) == Math.Abs(yDif))
                        {
                            if (xDif > 0 && yDif > 0) key = Key.Numpad9;
                            else if (xDif > 0 && yDif < 0) key = Key.Numpad3;
                            else if (xDif < 0 && yDif < 0) key = Key.Numpad1;
                            else if (xDif < 0 && yDif > 0) key = Key.Numpad7;
                        }
                        else if (Math.Abs(xDif) > Math.Abs(yDif))
                        {
                            if (xDif > 0) key = Key.D;
                            else key = Key.A;
                        }
                        else // if (Math.Abs(xDif) < Math.Abs(yDif))
                        {
                            if (yDif > 0) key = Key.W;
                            else key = Key.S;
                        }
                    }
                }
            }

            switch (key)
            {
                case Key.UpArrow:
                case Key.W:
                case Key.Numpad8:
                    newTileY++;
                    break;
                case Key.DownArrow:
                case Key.S:
                case Key.Numpad2:
                    newTileY--;
                    break;
                case Key.RightArrow:
                case Key.D:
                case Key.Numpad6:
                    newTileX++;
                    break;
                case Key.LeftArrow:
                case Key.A:
                case Key.Numpad4:
                    newTileX--;
                    break;
                case Key.Numpad9:
                    newTileY++;
                    newTileX++;
                    break;
                case Key.Numpad7:
                    newTileY++;
                    newTileX--;
                    break;
                case Key.Numpad1:
                    newTileY--;
                    newTileX--;
                    break;
                case Key.Numpad3:
                    newTileY--;
                    newTileX++;
                    break;
                default: doSomething = false; break;
            }

            if (doSomething)
            {
                newTileX = Map.instance.GetXPositionOnMap(newTileX);
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
