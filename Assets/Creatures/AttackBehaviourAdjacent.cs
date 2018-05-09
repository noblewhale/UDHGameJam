using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviourAdjacent : AttackBehaviour
{
    public Tile nextAttackTarget;

    override public void Attack()
    {
        owner.Attack(nextAttackTarget.occupant);
    }

    override public float ShouldAttack()
    {
        List<Tile> occupiedByHostile = new List<Tile>();

        Tile adjacent;
        if (owner.y < owner.map.height - 1)
        {
            adjacent = owner.map.tileObjects[owner.y + 1][owner.x];
            if (adjacent.occupant && adjacent.occupant.race != owner.race) occupiedByHostile.Add(adjacent);
        }
        if (owner.y > 0)
        {
            adjacent = owner.map.tileObjects[owner.y - 1][owner.x];
            if (adjacent.occupant && adjacent.occupant.race != owner.race) occupiedByHostile.Add(adjacent);
        }

        int wrappedX = owner.map.WrapX(owner.x + 1);
        adjacent = owner.map.tileObjects[owner.y][wrappedX];
        if (adjacent.occupant && adjacent.occupant.race != owner.race) occupiedByHostile.Add(adjacent);

        wrappedX = owner.map.WrapX(owner.x - 1);
        adjacent = owner.map.tileObjects[owner.y][wrappedX];
        if (adjacent.occupant && adjacent.occupant.race != owner.race) occupiedByHostile.Add(adjacent);

        if (occupiedByHostile.Count > 0)
        {
            nextAttackTarget = occupiedByHostile[Random.Range(0, occupiedByHostile.Count)];

            return 2f;
        }

        return 0;
    }
}
