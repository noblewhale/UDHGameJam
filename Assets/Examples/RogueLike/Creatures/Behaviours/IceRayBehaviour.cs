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

		public float rayLength = 3;

        override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			Vector2 ownerTile = new Vector2(owner.x + .5f, owner.y + .5f);
			Vector2 dir = Map.instance.GetDifference(ownerTile, new Vector2(targetTile.x + .5f, targetTile.y + .5f));
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
					new Vector2(owner.x + .5f, owner.y + .5f),
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
			fireballObject.transform.position = identityCreature.leftHand.transform.position - Vector3.forward;
		}

		override public void StartSubAction(ulong time) 
		{
			attackStartTime = Time.time;

			// Actual target tile is rayLength units in direction of selected tile
			Vector2 ownerTile = new Vector2(owner.x + .5f, owner.y + .5f);
			Vector2 dir = Map.instance.GetDifference(ownerTile, new Vector2(targetTile.x + .5f, targetTile.y + .5f));
			Vector2 dirNorm = dir.normalized;
			Vector2 target = ownerTile + dirNorm * rayLength;
			targetTile = Map.instance.GetTile(target.x, target.y);
		}
		override public bool ContinueSubAction(ulong time) 
		{
			Vector2 startPosition = identityCreature.leftHand.transform.position;
			Vector2 endPosition = new Vector2(targetTile.transform.position.x + Map.instance.tileWidth/2, targetTile.transform.position.y + Map.instance.tileHeight/2);
			float unitsPerSecond = 10;
			float timeSinceAttackStart = Time.time - attackStartTime;
			if ((endPosition - startPosition).magnitude > Map.instance.TotalWidth / 2)
            {
				if ((startPosition.x - Map.instance.transform.position.x) > Map.instance.TotalWidth / 2)
                {
					endPosition.x += Map.instance.TotalWidth;
				}
				else
                {
					endPosition.x -= Map.instance.TotalWidth;
				}
			}

			float duration = (startPosition - endPosition).magnitude / unitsPerSecond;
			fireballObject.transform.position = (Vector3)Vector2.Lerp(startPosition, endPosition, timeSinceAttackStart / duration) - Vector3.forward;

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
			var tilesInRay = GetThreatenedTiles();

			foreach (Tile t in tilesInRay)
			{
				if (t.objectList != null)
				{
					DungeonObject targetObject = t.objectList.FirstOrDefault(ob => ob.isCollidable);
					if (targetObject)
					{
						targetObject.TakeDamage(10);
					}
				}

				var fire = Instantiate(firePrefab);
				t.AddObject(fire.GetComponent<DungeonObject>());
			}

			Destroy(fireballObject);
		}
	}
}
