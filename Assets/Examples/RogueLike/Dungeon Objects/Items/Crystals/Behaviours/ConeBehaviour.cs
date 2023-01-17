namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using System;
    using System.Collections.Generic;

    public class ConeBehaviour : TargetableBehaviour
    {
		public GameObject fireballObjectPrefab;
		public GameObject firePrefab;
		List<GameObject> fireballObjects = new List<GameObject>();

		public float rayLength = 3;
		public float halfAngle = Mathf.PI / 7;

		public float unitsPerSecond = 10;
		List<Vector2> startVisualRayPositions = new List<Vector2>();
		List<Vector2> endVisualRayPositions = new List<Vector2>();
		List<float> durations = new List<float>();

		Vector2 lastConeStartPos;
		Vector2 lastConeDirection;
		float attackStartTime;

		override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			Vector2 centerOfOwnerTile = owner.tilePosition + Map.instance.tileDimensions / 2;
			lastConeDirection = Map.instance.GetDifference(centerOfOwnerTile, targetTile.tilePosition + Map.instance.tileDimensions / 2);
			lastConeDirection = lastConeDirection.normalized;

			float extraSpace = .02f;
			lastConeStartPos = centerOfOwnerTile + lastConeDirection * ((Mathf.Sqrt(2) / 2) * (1 + extraSpace));
            Vector2 minConePos = owner.tilePosition - Vector2.one * extraSpace / 2;
            Vector2 maxConePos = owner.tilePosition + Vector2.one * (1 + extraSpace / 2);
			lastConeStartPos.Clamp(minConePos, maxConePos);

			threatenedTiles = Map.instance.GetTilesInTruncatedArc(
				lastConeStartPos,
				rayLength,
				Mathf.Deg2Rad * Vector2.SignedAngle(lastConeDirection, Vector2.up) - halfAngle,
				Mathf.Deg2Rad * Vector2.SignedAngle(lastConeDirection, Vector2.up) + halfAngle,
				null,
				true
			);

			return threatenedTiles;
		}

        public override void StartSubAction(ulong time)
        {
            base.StartSubAction(time);

			attackStartTime = Time.time;
			foreach (var tile in threatenedTiles)
			{
				Vector2 startVisualRayPosition = Map.instance.transform.InverseTransformPoint(identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position);
				Vector2 endVisualRayPosition = new Vector2(tile.localPosition.x + Map.instance.tileWidth / 2, tile.localPosition.y + Map.instance.tileHeight / 2);
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
				float duration = (startVisualRayPosition - endVisualRayPosition).magnitude / unitsPerSecond;

				startVisualRayPositions.Add(startVisualRayPosition);
				endVisualRayPositions.Add(endVisualRayPosition);
				durations.Add(duration);

				GameObject fireballObject = Instantiate(fireballObjectPrefab);
				fireballObject.transform.parent = Map.instance.transform;
				fireballObject.transform.position = identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position - Vector3.forward;
				fireballObjects.Add(fireballObject);
			}
		}

        override public bool ContinueSubAction(ulong time) 
		{
			float timeSinceAttackStart = Time.time - attackStartTime;

			bool allDone = true;

			for (int i = 0; i < fireballObjects.Count; i++)
			{
				float t = timeSinceAttackStart / durations[i];

				if (fireballObjects[i] != null)
				{
					fireballObjects[i].transform.localPosition = (Vector3)Vector2.Lerp(startVisualRayPositions[i], endVisualRayPositions[i], t) - Vector3.forward;

					if (t >= 1)
					{
						var tileThatWasHit = Map.instance.GetTileFromWorldPosition(fireballObjects[i].transform.position);

						// Destroy the fireball
						Destroy(fireballObjects[i]);
						fireballObjects[i] = null;

						// Add the fire
						var fire = Instantiate(firePrefab);
						tileThatWasHit.AddObject(fire.GetComponent<DungeonObject>());

						// Do the damage
						if (tileThatWasHit != null && tileThatWasHit.objectList != null)
						{
							DungeonObject targetObject = tileThatWasHit.objectList.FirstOrDefault(ob => ob.isCollidable);
							if (targetObject)
							{
								targetObject.TakeDamage(10);
							}
						}
					}
					else
					{
						allDone = false;
					}
				}
			}

			if (allDone)
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
			foreach (var fireball in fireballObjects)
			{
				if (fireball != null) Destroy(fireball);
			}
			fireballObjects.Clear();
			startVisualRayPositions.Clear();
			endVisualRayPositions.Clear();
			durations.Clear();
		}
	}
}
