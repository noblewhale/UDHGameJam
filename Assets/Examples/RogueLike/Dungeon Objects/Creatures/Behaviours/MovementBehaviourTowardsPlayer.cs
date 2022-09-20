namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using UnityEngine;

    public class MovementBehaviourTowardsPlayer : TickableBehaviour
    {
        public bool useViewDistance;
        public float radiusIfNotUsingViewDistance;
        Tile nextMoveTarget;
        Creature owningCreature;

        override public void Awake()
        {
            base.Awake();
            owningCreature = owner.GetComponent<Creature>();
        }

        public override void StartAction()
        {
            owner.tickable.nextActionTime = TimeManager.instance.Time + owningCreature.ticksPerMove;
        }

        public override bool ContinueSubAction(ulong time)
        {
            return true;
        }

        public override void FinishSubAction(ulong time)
        {
            owner.map.MoveObject(owner, nextMoveTarget.tilePosition);
        }

        public override float GetActionConfidence()
        {
            Vector2Int playerPos = Player.instance.identity.tilePosition;
            Vector2Int myPos = owner.tilePosition;

            float radius = radiusIfNotUsingViewDistance;
            if (useViewDistance) radius = owningCreature.effectiveViewDistance;
            float distanceToPlayer = Vector2.Distance(playerPos, myPos);
            Vector2 wrappedMyPos = new Vector2(myPos.x + owner.map.width, myPos.y);
            float wrappedDistanceToPlayer = Vector2.Distance(playerPos, wrappedMyPos);
            wrappedMyPos = new Vector2(myPos.x - owner.map.width, myPos.y);
            float otherWrappedDistanceToPlayer = Vector2.Distance(playerPos, wrappedMyPos);
            distanceToPlayer = Mathf.Min(distanceToPlayer, wrappedDistanceToPlayer, otherWrappedDistanceToPlayer);
            if (distanceToPlayer < radius && distanceToPlayer > 1f)
            {
                Vector2Int dif = owner.map.GetDifference(myPos, playerPos);

                float r = Random.value;
                bool moveHorizontal;
                if (Mathf.Abs(dif.x) > Mathf.Abs(dif.y))
                {
                    moveHorizontal = true;
                }
                else if (Mathf.Abs(dif.y) > Mathf.Abs(dif.x))
                {
                    moveHorizontal = false;
                }
                else if (r > .5f)
                {
                    moveHorizontal = true;
                }
                else
                {
                    moveHorizontal = false;
                }


                bool horizontalBlocked = false;
                Vector2Int nextHorizontalPos = myPos;
                nextHorizontalPos.x += (int)Mathf.Sign(dif.x);
                Tile horizontalMoveTarget = owner.map.GetTile(nextHorizontalPos);
                if (horizontalMoveTarget.IsCollidable() || horizontalMoveTarget.ContainsObjectWithComponent<Trap>() || horizontalMoveTarget.GetPathingWeight() > 5)
                {
                    horizontalBlocked = true;
                }

                if (moveHorizontal && horizontalBlocked) moveHorizontal = false;

                bool verticalBlocked = false;
                Vector2Int nextVerticalPos = myPos;
                nextVerticalPos.y += (int)Mathf.Sign(dif.y);
                Tile verticalMoveTarget = owner.map.GetTile(nextVerticalPos);
                if (verticalMoveTarget.IsCollidable() || verticalMoveTarget.ContainsObjectWithComponent<Trap>() || verticalMoveTarget.GetPathingWeight() > 5)
                {
                    verticalBlocked = true;
                }

                if (!moveHorizontal && verticalBlocked) moveHorizontal = true;

                if (!(horizontalBlocked && verticalBlocked))
                {
                    if (moveHorizontal)
                    {
                        nextMoveTarget = horizontalMoveTarget;
                    }
                    else
                    {
                        nextMoveTarget = verticalMoveTarget;
                    }

                    return .5f;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }
    }
}