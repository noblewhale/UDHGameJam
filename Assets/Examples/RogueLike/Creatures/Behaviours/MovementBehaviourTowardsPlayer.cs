using System.Collections;
using System.Collections.Generic;
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

    public override void FinishAction()
    {
        owner.map.MoveObject(owner, nextMoveTarget.x, nextMoveTarget.y);
    }

    public override float GetActionConfidence()
    {
        Vector2 playerPos = new Vector2(Player.instance.identity.x, Player.instance.identity.y);
        Vector2 myPos = new Vector2(owner.x, owner.y);

        float radius = radiusIfNotUsingViewDistance;
        if (useViewDistance) radius = owningCreature.baseObject.viewDistance;
        float distanceToPlayer = Vector2.Distance(playerPos, myPos);
        if (distanceToPlayer < radius && distanceToPlayer > 1f)
        {
            int xDif = (int)(playerPos.x - owner.x);
            int yDif = (int)(playerPos.y - owner.y);
            float r = Random.value;

            bool moveHorizontal = false;

            if (Mathf.Abs(xDif) > Mathf.Abs(yDif))
            {
                moveHorizontal = true;
            }
            else if (Mathf.Abs(yDif) > Mathf.Abs(xDif))
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
            int nextX = (int)(myPos.x + Mathf.Sign(xDif));
            if (owner.map.tileObjects[owner.y][nextX].IsCollidable())
            {
                horizontalBlocked = true;
            }

            if (moveHorizontal && horizontalBlocked) moveHorizontal = false;

            bool verticalBlocked = false;
            int nextY = (int)(myPos.y + Mathf.Sign(yDif));
            if (owner.map.tileObjects[nextY][owner.x].IsCollidable())
            {
                verticalBlocked = true;
            }

            if (!moveHorizontal && verticalBlocked) moveHorizontal = true;

            if (!(horizontalBlocked && verticalBlocked))
            {
                if (moveHorizontal)
                {
                    nextMoveTarget = owner.map.tileObjects[owner.y][nextX];
                }
                else
                {
                    nextMoveTarget = owner.map.tileObjects[nextY][owner.x];
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
