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

        override public TickableBehaviour DetermineBehaviour()
        {
            Command command = PlayerInputHandler.instance.commandQueue.Dequeue();

            if (command.key == Key.F)
            {
                return owner.GetComponent<Creature>().leftHandObject.GetComponent<TickableBehaviour>();
            }
            else
            {
                return DefaultAction(command);
            }
        }

        public TickableBehaviour DefaultAction(Command command)
        {
            if (Player.instance.identity == null || Player.instance.identity.tile == null) return null;

            Vector2Int newTilePos = Player.instance.identity.tilePosition;

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
                    Tile tile = Map.instance.GetTileFromWorldPosition(unwarpedPos);
                    Vector2Int dif = Map.instance.GetDifference(Player.instance.identity.tilePosition, tile.tilePosition);
                    if (Math.Abs(dif.x) == Math.Abs(dif.y))
                    {
                        if (dif.x > 0 && dif.y > 0) key = Key.Numpad9;
                        else if (dif.x > 0 && dif.y < 0) key = Key.Numpad3;
                        else if (dif.x < 0 && dif.y < 0) key = Key.Numpad1;
                        else if (dif.x < 0 && dif.y > 0) key = Key.Numpad7;
                    }
                    else if (Math.Abs(dif.x) > Math.Abs(dif.y))
                    {
                        if (dif.x > 0) key = Key.D;
                        else key = Key.A;
                    }
                    else // if (Math.Abs(xDif) < Math.Abs(yDif))
                    {
                        if (dif.y > 0) key = Key.W;
                        else key = Key.S;
                    }
                }
            }

            switch (key)
            {
                case Key.UpArrow:
                case Key.W:
                case Key.Numpad8:
                    newTilePos.y++;
                    break;
                case Key.DownArrow:
                case Key.S:
                case Key.Numpad2:
                    newTilePos.y--;
                    break;
                case Key.RightArrow:
                case Key.D:
                case Key.Numpad6:
                    newTilePos.x++;
                    break;
                case Key.LeftArrow:
                case Key.A:
                case Key.Numpad4:
                    newTilePos.x--;
                    break;
                case Key.Numpad9:
                    newTilePos.y++;
                    newTilePos.x++;
                    break;
                case Key.Numpad7:
                    newTilePos.y++;
                    newTilePos.x--;
                    break;
                case Key.Numpad1:
                    newTilePos.y--;
                    newTilePos.x--;
                    break;
                case Key.Numpad3:
                    newTilePos.y--;
                    newTilePos.x++;
                    break;
                default: doSomething = false; break;
            }

            if (doSomething)
            {
                newTilePos.x = Map.instance.GetXTilePositionOnMap(newTilePos.x);
                newTilePos.y = Mathf.Clamp(newTilePos.y, 0, Map.instance.height - 1);
            }
            else
            {
                owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
                return null;
            }

            var tileActingOn = owner.map.GetTile(newTilePos);

            if (!tileActingOn.IsCollidable())
            {
                moveBehaviour.targetTilePosition = newTilePos;
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
