namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using System.Collections.Generic;

    public class FireAOEBehaviour : TargetableBehaviour
    {
		public GameObject projectilePrefab;
		public GameObject elementalEffectPrefab;
		GameObject projectile;

		float projectileTravelSpeed = 10;
		float projectileTravelDuration;

		Vector2 projectileStartPosition;
		Vector2 projectileEndPosition;

		float attackStartTime;

		override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			// Get all tiles in a line between the center of the owner tile and the center of the target tile stopping at the first IsCollidable tile in the line.
			Vector2 rayStart = owner.tilePosition + Map.instance.tileDimensions / 2;
			Vector2 rayEnd = targetTile.tilePosition + Map.instance.tileDimensions / 2;
			threatenedTiles = Map.instance.GetTilesInRay(rayStart, rayEnd, t => t.IsCollidable(), false);

			// We only care about the last tile in the line, whether we hit something collidable, or just reached the target
			if (threatenedTiles.Count > 1)
			{
				threatenedTiles = threatenedTiles.GetRange(threatenedTiles.Count - 1, 1);
			}

			return threatenedTiles;
		}


		override public IEnumerator StartActionCoroutine()
		{
			projectile = Instantiate(projectilePrefab);
			projectile.transform.position = identityCreature.leftHand.transform.position - Vector3.forward;
			
			yield return base.StartActionCoroutine();
		}

		override public void StartSubAction(ulong time) 
		{
			base.StartSubAction(time);
			
			attackStartTime = Time.time;

			projectileStartPosition = identityCreature.leftHand.transform.position;
			projectileEndPosition = new Vector2(threatenedTiles[0].position.x + Map.instance.tileWidth / 2, threatenedTiles[0].position.y + Map.instance.tileHeight / 2);
			projectileTravelDuration = (projectileStartPosition - projectileEndPosition).magnitude / projectileTravelSpeed;
			if ((projectileEndPosition - projectileStartPosition).magnitude > Map.instance.TotalWidth / 2)
			{
				if ((projectileStartPosition.x - Map.instance.transform.position.x) > Map.instance.TotalWidth / 2)
				{
					projectileEndPosition.x += Map.instance.TotalWidth;
				}
				else
				{
					projectileEndPosition.x -= Map.instance.TotalWidth;
				}
			}
		}

		override public bool ContinueSubAction(ulong time) 
		{
			float timeSinceAttackStart = Time.time - attackStartTime;
			projectile.transform.position = (Vector3)Vector2.Lerp(projectileStartPosition, projectileEndPosition, timeSinceAttackStart / projectileTravelDuration) - Vector3.forward;

			if (timeSinceAttackStart > projectileTravelDuration)
            {
				return true;
            }
			else
            {
				return false;
            }
		}
		override public void FinishSubAction(ulong time)
		{
			if (threatenedTiles[0] != null && threatenedTiles[0].objectList != null)
			{
				DungeonObject targetObject = threatenedTiles[0].objectList.FirstOrDefault(ob => ob.isCollidable);
				if (targetObject)
				{
					targetObject.TakeDamage(10);
				}
			}
			var fire = Instantiate(elementalEffectPrefab);
			threatenedTiles[0].AddObject(fire.GetComponent<DungeonObject>());
			
			Destroy(projectile);
		}
	}
}
