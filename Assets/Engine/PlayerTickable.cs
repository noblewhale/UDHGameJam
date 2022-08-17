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

            int newTileX = Player.instance.identity.x;
            int newTileY = Player.instance.identity.y;

            bool doSomething = true;
            switch (command.key)
            {
                case Key.W: newTileY++; break;
                case Key.S: newTileY--; break;
                case Key.D: newTileX++; break;
                case Key.A: newTileX--; break;
                default: doSomething = false; break;
            }

            if (doSomething)
            {
                newTileX = Mathf.Clamp(newTileX, 0, Map.instance.width - 1);
                newTileY = Mathf.Clamp(newTileY, 0, Map.instance.height - 1);
            }
            else
            {
                owner.tickable.nextActionTime = TimeManager.instance.Time + duration;
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
