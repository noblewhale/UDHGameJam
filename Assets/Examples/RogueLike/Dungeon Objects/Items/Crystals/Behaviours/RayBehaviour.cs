namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using System;
    using System.Collections.Generic;

    public class RayBehaviour : TargetableBehaviour
    {
		public GameObject rayPrefab;
		public GameObject elementalTileEffectPrefab;
		GameObject rayObject;

		Tile currentProjectileTile;

		public float rayLength = 3;
		public float unitsPerSecond = 10;
		public float lingerTime = .4f;
		Vector2 startRayPosition;
		Vector2 endRayPosition;
		float duration;
		float attackStartTime;

		override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			Vector2 centerOfOwnerTile = owner.tilePosition + Map.instance.tileDimensions / 2;
			Vector2 directionToTarget = Map.instance.GetDifference(centerOfOwnerTile, targetTile.tilePosition + Map.instance.tileDimensions / 2);
			directionToTarget.Normalize();

			threatenedTiles = Map.instance.GetTilesInRay(centerOfOwnerTile, directionToTarget, rayLength);

			return threatenedTiles;
		}

		override public IEnumerator StartActionCoroutine()
		{
			yield return base.StartActionCoroutine();

			rayObject = Instantiate(rayPrefab);
			rayObject.transform.parent = Map.instance.transform;
			rayObject.transform.position = identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position - Vector3.forward;
		}

		override public void StartSubAction(ulong time) 
		{
			base.StartSubAction(time);

			attackStartTime = Time.time;

			startRayPosition = Map.instance.transform.InverseTransformPoint(identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position);
			endRayPosition = (Vector2)threatenedTiles.Last().localPosition + Map.instance.tileDimensions / 2;
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
			endRayPosition += ((Vector2)identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position - ((Vector2)identityCreature.baseObject.tile.position + owner.map.tileDimensions / 2));
			duration = (startRayPosition - endRayPosition).magnitude / unitsPerSecond;
		}

		override public bool ContinueSubAction(ulong time) 
		{
			float timeSinceAttackStart = Time.time - attackStartTime;

			if (timeSinceAttackStart <= duration)
			{
				//rayObject.transform.localPosition = (Vector3)Vector2.Lerp(startRayPosition, startRayPosition + (endRayPosition - startRayPosition) / 2, timeSinceAttackStart / duration) - Vector3.forward;
				rayObject.transform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, (endRayPosition - startRayPosition)));
				rayObject.GetComponent<SpriteRenderer>().size = new Vector2((endRayPosition - startRayPosition).magnitude * timeSinceAttackStart / duration, 1);
				rayObject.GetComponent<SpriteRenderer>().material.mainTextureOffset = new Vector2(-timeSinceAttackStart*4, 0);
				Vector2 circlePos = owner.map.GetWorldPositionOnMap(rayObject.transform.position);
				Tile closestTile = Tile.GetClosestTile(threatenedTiles, circlePos);

				if (closestTile != currentProjectileTile)
				{
					if (elementalTileEffectPrefab)
					{
						var fire = Instantiate(elementalTileEffectPrefab);
						closestTile.AddObject(fire.GetComponent<DungeonObject>());
					}
					DungeonObject targetObject = closestTile.objectList.FirstOrDefault(ob => ob.isCollidable);
					if (targetObject)
					{
						targetObject.TakeDamage(10);
					}
				}
				currentProjectileTile = closestTile;

				return false;
			}
			else if (timeSinceAttackStart <= duration + lingerTime)
            {
				rayObject.GetComponent<SpriteRenderer>().material.mainTextureOffset = new Vector2(-timeSinceAttackStart*4, 0);
				return false;
            }
			else
            {
				return true;
            }
		}

		override public void FinishSubAction(ulong time)
		{
			Destroy(rayObject);
		}
	}
}
