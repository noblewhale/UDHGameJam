using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviourTowardsPlayer : MovementBehaviour
{
    public bool useViewDistance;
    public float radiusIfNotUsingViewDistance;
    Tile nextMoveTarget;

    public override void Move()
    {
        owner.SetPosition(nextMoveTarget.x, nextMoveTarget.y);
    }

    public override float ShouldMove()
    {
        Vector2 playerPos = new Vector2(Player.instance.identity.x, Player.instance.identity.y);
        Vector2 myPos = new Vector2(owner.x, owner.y);

        float radius = radiusIfNotUsingViewDistance;
        if (useViewDistance) radius = owner.viewDistance;
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

            bool bothBlocked = true;
            if (moveHorizontal)
            {
                int nextX = (int)(myPos.x + Mathf.Sign(xDif));
                if (owner.map.tileObjects[owner.y][nextX].IsCollidable())
                {
                    moveHorizontal = false;
                }
            }
            else
            {
                bothBlocked = false;
            }

            if (!moveHorizontal)
            {
                int nextY = (int)(myPos.y + Mathf.Sign(yDif));
                if (owner.map.tileObjects[nextY][owner.x].IsCollidable())
                {
                    moveHorizontal = true;
                }
            }
            else
            {
                bothBlocked = false;
            }

            if (!bothBlocked)
            {
                if (moveHorizontal)
                {
                    int nextX = (int)(myPos.x + Mathf.Sign(xDif));
                    nextMoveTarget = owner.map.tileObjects[owner.y][nextX];
                }
                else
                {
                    int nextY = (int)(myPos.y + Mathf.Sign(yDif));
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
