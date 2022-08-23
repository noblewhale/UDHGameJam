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

			float dirX = Map.instance.GetXDifference(owner.x, targetTile.x);
			float dirY = targetTile.y - owner.y;
			Vector2 direction = new Vector2(dirX, dirY);
			float distance = direction.magnitude;
			direction.Normalize();

			float stepSize = Mathf.Min(Map.instance.tileWidth, Map.instance.tileHeight) * .9f;
			Vector2 center = new Vector2(owner.x + .5f, owner.y + .5f);
			for (int d = 1; d < distance / stepSize; d++)
			{
				Vector2 relative = center + direction * d * stepSize;

				int y = (int)relative.y;
				if (y < 0 || y >= Map.instance.height) break;

				int wrappedX = (int)Map.instance.GetXPositionOnMap(relative.x);

				var tileInRay = Map.instance.tileObjects[y][wrappedX];

				if (tileInRay.IsCollidable() && (y != owner.y || wrappedX != owner.x))
				{
					threatenedTiles.Add(tileInRay);
					break;
				}
			}

			if (threatenedTiles.Count == 0)
            {
				threatenedTiles.Add(targetTile);
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
