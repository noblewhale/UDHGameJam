namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using System;
    using System.Collections.Generic;

    public class IceRayBehaviour : TargetableBehaviour
    {
		public GameObject fireballObjectPrefab;
		public GameObject firePrefab;
		GameObject fireballObject;
		float attackStartTime = 0;

		Tile currentProjectileTile;

		public float rayLength = 3;

		public float unitsPerSecond = 10;
		Vector2 startRayPosition;
		Vector2 endRayPosition;
		float duration;

		override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			Vector2 ownerTile = new Vector2(owner.x + Map.instance.tileWidth / 2, owner.y + Map.instance.tileWidth / 2);
			Vector2 dir = Map.instance.GetDifference(ownerTile, new Vector2(targetTile.x + Map.instance.tileWidth / 2, targetTile.y + Map.instance.tileWidth / 2));
			Vector2 dirNorm = dir.normalized;
			var area = new RectIntExclusive();
			area.SetMinMax(
				(int)(owner.x - rayLength),
				(int)(owner.x + rayLength),
				(int)(owner.y - rayLength),
				(int)(owner.y + rayLength)
			);

			lock (Map.instance.isDirtyLock)
			{
				Map.instance.ForEachTileInArea(area, (t) => t.isDirty = false);
				threatenedTiles = Map.instance.GetTilesInRay(
					new Vector2(owner.x + Map.instance.tileWidth / 2, owner.y + Map.instance.tileWidth / 2),
					dirNorm,
					rayLength,
					null,
					false,
					.4f,
					true
				);
			}
			return threatenedTiles;
		}

		override public IEnumerator StartActionCoroutine()
		{
			yield return base.StartActionCoroutine();
			fireballObject = Instantiate(fireballObjectPrefab);
			fireballObject.transform.parent = Map.instance.transform;
			fireballObject.transform.position = identityCreature.leftHand.transform.position - Vector3.forward;
		}

		override public void StartSubAction(ulong time) 
		{
			attackStartTime = Time.time;

			startRayPosition = Map.instance.transform.InverseTransformPoint(identityCreature.leftHand.transform.position);
			endRayPosition = new Vector2(threatenedTiles.Last().transform.localPosition.x + Map.instance.tileWidth / 2, threatenedTiles.Last().transform.localPosition.y + Map.instance.tileHeight / 2);
			if ((endRayPosition - startRayPosition).magnitude > Map.instance.TotalWidth / 2)
			{
				if (startRayPosition.x > Map.instance.TotalWidth / 2)
				{
					endRayPosition.x += Map.instance.TotalWidth;
				}
				else
				{
					endRayPosition.x -= Map.instance.TotalWidth;
				}
			}
			duration = (startRayPosition - endRayPosition).magnitude / unitsPerSecond;
		}
		override public bool ContinueSubAction(ulong time) 
		{
			float timeSinceAttackStart = Time.time - attackStartTime;
			
			fireballObject.transform.localPosition = (Vector3)Vector2.Lerp(startRayPosition, endRayPosition, timeSinceAttackStart / duration) - Vector3.forward;

			Tile closestTile = Tile.GetClosestTile(threatenedTiles, fireballObject.transform.position);

			if (closestTile != currentProjectileTile)
            {
				var fire = Instantiate(firePrefab);
				closestTile.AddObject(fire.GetComponent<DungeonObject>());
                DungeonObject targetObject = closestTile.objectList.FirstOrDefault(ob => ob.isCollidable);
                if (targetObject)
                {
                    targetObject.TakeDamage(10);
                }
            }
			currentProjectileTile = closestTile;

			if (timeSinceAttackStart > duration)
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
			Destroy(fireballObject);
		}
	}
}
