namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class Boulder : MonoBehaviour
    {
        public bool CanMoveDiagonal = true;

        void Start()
        {

        }

        public void PushBoulder(DungeonObject ob, bool isInstigator)
        {
            if (!isInstigator)
            {
                //reference to the boulder dungeon object
                DungeonObject baseObject = GetComponent<DungeonObject>();

                //Reference to boulder position 
                var boulderPosition = baseObject.tilePosition;

                //Reference to position of the thing pushing the boulder
                var pusherPosition = ob.tilePosition;

                var pushDirection = boulderPosition - pusherPosition;

                if (!CanMoveDiagonal && pushDirection.x != 0 && pushDirection.y != 0)
                {
                    // Disallow diagonal boulder movement
                    return;
                }

                var updatedBoulderPosition = boulderPosition + pushDirection;

                var potentialNewTile = Map.instance.GetTile(updatedBoulderPosition);

                if (!potentialNewTile.IsCollidable())
                {
                    Map.instance.MoveObject(baseObject, updatedBoulderPosition);
                    Map.instance.UpdateIsVisible(Player.instance.identity.tile, ob.Creature.effectiveViewDistance, true);
                    if (potentialNewTile.ContainsObjectOfType("Water") || potentialNewTile.ContainsObjectOfType("Acid"))
                    {
                        var waterToDestroy = potentialNewTile.GetObjectOfType("Water");
                        var acidToDestroy = potentialNewTile.GetObjectOfType("Acid");

                        potentialNewTile.RemoveObject(waterToDestroy, true);
                        potentialNewTile.RemoveObject(acidToDestroy, true);

                        potentialNewTile.RemoveObject(baseObject, true);
                    }
                }
            }
        }
    }
}