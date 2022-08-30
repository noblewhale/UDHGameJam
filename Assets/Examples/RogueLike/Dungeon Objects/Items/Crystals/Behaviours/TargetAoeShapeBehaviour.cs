namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TargetAoeShapeBehaviour : TargetableBehaviour
    {
		override public void Awake()
		{
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			// Get all tiles in a line between the center of the owner tile and the center of the target tile stopping at the first IsCollidable tile in the line.
			Vector2 rayStart = identityCreature.tilePosition + Map.instance.tileDimensions / 2;
			Vector2 rayEnd = targetTile.position + Map.instance.tileDimensions / 2;
			threatenedTiles = Map.instance.GetTilesInRay(rayStart, rayEnd, t => t.IsCollidable(), false);

			// We only care about the last tile in the line, whether we hit something collidable, or just reached the target
			if (threatenedTiles.Count > 1)
			{
				threatenedTiles = threatenedTiles.GetRange(threatenedTiles.Count - 1, 1);
			}

			foreach (Vector2Int relativeLocation in shape)
            {
				Vector2Int pos = threatenedTiles[0].position + relativeLocation;
				Tile threatenedTile = Map.instance.GetTile(pos);
				if (threatenedTiles[0] != threatenedTile)
				{
					threatenedTiles.Add(threatenedTile);
				}
            }

			return threatenedTiles;
		}

	}
}