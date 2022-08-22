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
		Vector2 startVisualRayPosition;
		Vector2 endVisualRayPosition;
		Vector2 startActualRayPosition;
		Vector2 endActualRayPosition;
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
			Vector2 target = ownerTile + dirNorm * rayLength;
			Tile realTarget = Map.instance.GetTile(target.x, target.y);

			// Ray will fire rayLength units in direction of targetTile
			int dirX = Map.instance.GetXDifference(owner.x, realTarget.x);
			int dirY = realTarget.y - owner.y;
			Vector2 direction = new Vector2(dirX, dirY);
			float distance = direction.magnitude;
			direction.Normalize();
			var area = new RectIntExclusive();
			area.SetMinMax(
				Math.Min(owner.x, owner.x + dirX),
				Math.Max(owner.x, owner.x + dirX) + 1,
				Math.Min(owner.y, owner.y + dirY),
				Math.Max(owner.y, owner.y + dirY) + 1
			);

			var tilesInRay = new List<Tile>();
			lock (Map.instance.isDirtyLock)
			{
				Map.instance.ForEachTileInArea(area, (t) => t.isDirty = false);
				tilesInRay = Map.instance.GetTilesInRay(
					new Vector2(owner.x + Map.instance.tileWidth / 2, owner.y + Map.instance.tileWidth / 2),
					direction,
					distance,
					null,
					false
				);
			}
			return tilesInRay;
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

			// Actual target tile is rayLength units in direction of selected tile
			Vector2 ownerTile = new Vector2(owner.x + Map.instance.tileWidth / 2, owner.y + Map.instance.tileWidth / 2);
			Vector2 dir = Map.instance.GetDifference(ownerTile, new Vector2(targetTile.x + Map.instance.tileWidth / 2, targetTile.y + Map.instance.tileWidth / 2));
			Vector2 dirNorm = dir.normalized;
			Vector2 target = ownerTile + dirNorm * rayLength;
			targetTile = Map.instance.GetTile(target.x, target.y);

			startVisualRayPosition = Map.instance.transform.InverseTransformPoint(identityCreature.leftHand.transform.position);
			endVisualRayPosition = new Vector2(targetTile.transform.localPosition.x + Map.instance.tileWidth / 2, targetTile.transform.localPosition.y + Map.instance.tileHeight / 2);
			if ((endVisualRayPosition - startVisualRayPosition).magnitude > Map.instance.TotalWidth / 2)
			{
				if (startVisualRayPosition.x > Map.instance.TotalWidth / 2)
				{
					endVisualRayPosition.x += Map.instance.TotalWidth;
				}
				else
				{
					endVisualRayPosition.x -= Map.instance.TotalWidth;
				}
			}
			duration = (startVisualRayPosition - endVisualRayPosition).magnitude / unitsPerSecond;

			startActualRayPosition = new Vector2(owner.x + Map.instance.tileWidth / 2, owner.y + Map.instance.tileWidth / 2);
			endActualRayPosition = new Vector2(targetTile.transform.localPosition.x + Map.instance.tileWidth / 2, targetTile.transform.localPosition.y + Map.instance.tileHeight / 2);
			endActualRayPosition = Map.instance.GetPositionOnMap(endActualRayPosition);
			if ((endActualRayPosition - startActualRayPosition).magnitude > Map.instance.TotalWidth / 2)
			{
				if (startActualRayPosition.x > Map.instance.TotalWidth / 2)
				{
					endActualRayPosition.x += Map.instance.TotalWidth;
				}
				else
				{
					endActualRayPosition.x -= Map.instance.TotalWidth;
				}
			}
		}
		override public bool ContinueSubAction(ulong time) 
		{
			float timeSinceAttackStart = Time.time - attackStartTime;
			
			fireballObject.transform.localPosition = (Vector3)Vector2.Lerp(startVisualRayPosition, endVisualRayPosition, timeSinceAttackStart / duration) - Vector3.forward;
			Vector2 actualRayPos = (Vector3)Vector2.Lerp(startActualRayPosition, endActualRayPosition, timeSinceAttackStart / duration);

			var newProjectileTile = Map.instance.GetTile(actualRayPos);
			if (newProjectileTile != currentProjectileTile && newProjectileTile != owner.tile)
            {
				var fire = Instantiate(firePrefab);
				newProjectileTile.AddObject(fire.GetComponent<DungeonObject>());
				if (newProjectileTile && newProjectileTile.objectList != null)
				{
					DungeonObject targetObject = newProjectileTile.objectList.FirstOrDefault(ob => ob.isCollidable);
					if (targetObject)
					{
						targetObject.TakeDamage(10);
					}
				}
			}
			currentProjectileTile = newProjectileTile;

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
