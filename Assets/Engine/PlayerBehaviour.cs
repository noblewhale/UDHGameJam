namespace Noble.TileEngine
{
    using UnityEngine;
    using System.Linq;

    public class PlayerBehaviour : TickableBehaviour
    {
        protected struct BehaviourAction
        {
            public delegate bool StartAction();
            public delegate bool ContinueAction();
            public delegate void FinishAction();
            public StartAction startAction;
            public ContinueAction continueAction;
            public FinishAction finishAction;
        }

        protected BehaviourAction nextAction;

        public static PlayerBehaviour instance;

        public override void Awake()
        {
            instance = this;
            base.Awake();
        }

        public override bool StartAction(out ulong duration)
        {
            Command command = PlayerInput.instance.commandQueue.Dequeue();

            DetermineAutoAction(command, out duration);
            if (nextAction.startAction != null)
            {
                return nextAction.startAction();
            }
            return true;
        }

        public override bool ContinueAction()
        {
            if (nextAction.continueAction != null)
            {
                return nextAction.continueAction();
            }
            return true;
        }

        public override void FinishAction()
        {
            if (nextAction.finishAction != null)
            {
                nextAction.finishAction();
            }
        }

        public override float GetActionConfidence()
        {
            return 1;
        }

        virtual public void DetermineAutoAction(Command command, out ulong duration)
        {
            int newTileX = Player.instance.identity.x;
            int newTileY = Player.instance.identity.y;

            bool doSomething = true;
            switch (command.key)
            {
                case KeyCode.W: newTileY++; break;
                case KeyCode.S: newTileY--; break;
                case KeyCode.D: newTileX++; break;
                case KeyCode.A: newTileX--; break;
                default: doSomething = false; break;
            }

            if (doSomething)
            {
                newTileX = Mathf.Clamp(newTileX, 0, Map.instance.width - 1);
                newTileY = Mathf.Clamp(newTileY, 0, Map.instance.height - 1);
            }
            else
            {
                duration = 0;
                return;
            }

            var identityCreature = owner.GetComponent<Creature>();
            nextAction = new BehaviourAction();
            duration = 1;

            var tileActingOn = owner.map.tileObjects[newTileY][newTileX];

            if (!tileActingOn.IsCollidable())
            {
                duration = identityCreature.ticksPerMove;
                nextAction.finishAction = () =>
                {
                    owner.map.TryMoveObject(owner, newTileX, newTileY);
                    if (owner.tile.objectList.Any(x => x.canBePickedUp))
                    {
                        owner.PickUpAll();
                    }
                };
            }
            else
            {
                foreach (var dOb in tileActingOn.objectList)
                {
                    if (dOb.isCollidable)
                    {
                        duration = identityCreature.ticksPerAttack;
                        
                        nextAction.startAction = () =>
                        {
                            identityCreature.StartAttack(dOb);
                            return false;
                        };
                        nextAction.continueAction = () =>
                        {
                            return identityCreature.ContinueAttack(dOb);
                        };
                        nextAction.finishAction = () =>
                        {
                            owner.map.TryMoveObject(owner, newTileX, newTileY);
                            identityCreature.FinishAttack(dOb);
                        };

                        break;
                    }
                }
            }
        }
    }
}