namespace Noble.TileEngine
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerTickable : Tickable
    {

        public AttackBehaviour attackBehaviour;
        public MoveBehaviour moveBehaviour;

        override public TickableBehaviour DetermineBehaviour()
        {
            Command command = PlayerInputHandler.instance.commandQueue.Dequeue();

            ulong duration = 1;

            Vector2Int newTilePosition = Player.instance.identity.tilePosition;

            bool doSomething = true;
            switch (command.key)
            {
                case Key.W: newTilePosition.y++; break;
                case Key.S: newTilePosition.y--; break;
                case Key.D: newTilePosition.x++; break;
                case Key.A: newTilePosition.x--; break;
                default: doSomething = false; break;
            }

            if (doSomething)
            {
                newTilePosition.x = Mathf.Clamp(newTilePosition.x, 0, Map.instance.width - 1);
                newTilePosition.y = Mathf.Clamp(newTilePosition.y, 0, Map.instance.height - 1);
            }
            else
            {
                owner.tickable.nextActionTime = TimeManager.instance.Time + duration;
                return null;
            }

            var tileActingOn = owner.map.tiles[newTilePosition.y][newTilePosition.x];

            if (!tileActingOn.IsCollidable())
            {
                moveBehaviour.targetTilePosition = newTilePosition;
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
