using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviourRandom : MovementBehaviour
{
    
    Tile nextMoveTarget;

    public override void Move()
    {
        owner.SetPosition(nextMoveTarget.x, nextMoveTarget.y);
    }

    public override float ShouldMove()
    {
        List<Tile> adjacentAndOpen = new List<Tile>();

        Tile adjacent;
        if (owner.y < owner.map.height - 1)
        {
            adjacent = owner.map.tileObjects[owner.y + 1][owner.x];
            if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);
        }
        if (owner.y > 0)
        {
            adjacent = owner.map.tileObjects[owner.y - 1][owner.x];
            if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);
        }

        int wrappedX = owner.map.WrapX(owner.x + 1);
        adjacent = owner.map.tileObjects[owner.y][wrappedX];
        if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);

        wrappedX = owner.map.WrapX(owner.x - 1);
        adjacent = owner.map.tileObjects[owner.y][wrappedX];
        if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);

        if (adjacentAndOpen.Count > 0)
        {
            nextMoveTarget = adjacentAndOpen[Random.Range(0, adjacentAndOpen.Count)];

            return .5f;
        }

        return 0;
    }
}
