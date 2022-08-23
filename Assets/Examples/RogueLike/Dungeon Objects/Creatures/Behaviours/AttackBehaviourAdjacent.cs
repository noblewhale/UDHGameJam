namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AttackBehaviourAdjacent : AttackBehaviour
    {
        override public float GetActionConfidence()
        {
            List<Creature> adjacentHostileCreatures = new List<Creature>();

            Tile adjacent;
            if (owner.y < owner.map.height - 1)
            {
                adjacent = owner.map.GetTile(owner.tilePosition + Vector2Int.up);
                GetHostileOccupants(adjacent, adjacentHostileCreatures);
            }
            if (owner.y > 0)
            {
                adjacent = owner.map.GetTile(owner.tilePosition + Vector2Int.down);
                GetHostileOccupants(adjacent, adjacentHostileCreatures);
            }

            adjacent = owner.map.GetTile(owner.tilePosition + Vector2Int.right);
            GetHostileOccupants(adjacent, adjacentHostileCreatures);

            adjacent = owner.map.GetTile(owner.tilePosition + Vector2Int.left);
            GetHostileOccupants(adjacent, adjacentHostileCreatures);

            if (adjacentHostileCreatures.Count > 0)
            {
                targetTile = adjacentHostileCreatures[Random.Range(0, adjacentHostileCreatures.Count)].baseObject.tile;

                return 2f;
            }

            return 0;
        }

        void GetHostileOccupants(Tile tile, List<Creature> results)
        {
            foreach (var ob in tile.objectList)
            {
                if (ob.canTakeDamage)
                {
                    var creature = ob.GetComponent<Creature>();
                    if (creature != null)
                    {
                        // TODO: For now only considering the player hostile but could use alignments or disposition or w/e
                        // and really how hostility is determined should be up to the creature not the attack behaviour probably
                        if (creature.baseObject == Player.instance.identity)
                        {
                            results.Add(creature);
                        }
                    }
                }
            }
        }
    }
}