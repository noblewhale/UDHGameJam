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

		float attackStartTime = 0;

		float projectileTravelSpeed = 10;
		float projectileTravelDuration;

		Vector2 projectileStartPosition;
		Vector2 projectileEndPosition;

		override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			threatenedTiles.Clear();

			Vector2 direction = Map.instance.GetDifference(owner.tilePosition, targetTile.position);
			float distance = direction.magnitude;
			direction.Normalize();

			var area = new RectIntExclusive();
			area.SetToSquare(owner.tilePosition, distance);

			lock (Map.instance.isDirtyLock)
			{
				Map.instance.ForEachTileInArea(area, (t) => t.isDirty = false);
				threatenedTiles = Map.instance.GetTilesInRay(
					owner.tilePosition + Map.instance.tileDimensions / 2,
					direction,
					distance,
					t => t.IsCollidable(),
					false
				);
			}

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
			attackStartTime = Time.time; 
			projectileStartPosition = identityCreature.leftHand.transform.position;
			projectileEndPosition = new Vector2(threatenedTiles[0].transform.position.x + Map.instance.tileWidth / 2, threatenedTiles[0].transform.position.y + Map.instance.tileHeight / 2);
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
			if (threatenedTiles[0] && threatenedTiles[0].objectList != null)
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
