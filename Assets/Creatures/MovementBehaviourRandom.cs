using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviourRandom : MovementBehaviour
{

    public override void Move()
    {
        List<Tile> adjacentAndOpen = new List<Tile>();
        
        Tile adjacent;
        if (owner.tileY < owner.map.height - 1)
        {
            adjacent = owner.map.tileObjects[owner.tileY + 1][owner.tileX];
            if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);
        }
        if (owner.tileY > 0)
        {
            adjacent = owner.map.tileObjects[owner.tileY - 1][owner.tileX];
            if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);
        }

        int wrappedX = owner.map.WrapX(owner.tileX + 1);
        adjacent = owner.map.tileObjects[owner.tileY][wrappedX];
        if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);

        wrappedX = owner.map.WrapX(owner.tileX - 1);
        adjacent = owner.map.tileObjects[owner.tileY][wrappedX];
        if (!adjacent.IsCollidable()) adjacentAndOpen.Add(adjacent);

        if (adjacentAndOpen.Count > 0)
        {
            Tile target = adjacentAndOpen[Random.Range(0, adjacentAndOpen.Count)];

            owner.SetPosition(target.x, target.y);
        }
    }

    public override float ShouldMove()
    {
        return .5f;
    }
}
