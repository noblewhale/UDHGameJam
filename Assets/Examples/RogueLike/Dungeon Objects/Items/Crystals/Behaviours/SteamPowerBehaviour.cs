namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SteamPowerBehaviour : PowerBehaviour
    {
		public GameObject projectilePrefab;
		public GameObject elementalEffectPrefab;
		GameObject projectileObject;
		List<(GameObject, Tile)> secondaryProjectileObjects = new List<(GameObject, Tile)>();

		protected float attackStartTime = 0;
		protected Creature identityCreature => owner.gameObject.GetComponentInParent<Creature>(true);

		public float unitsPerSecond = 10;

		Vector2 startProjectilePosition;
		Vector2 endProjectilePosition;
		float duration;

		override public void Awake()
		{
			base.Awake();
		}

		override public void StartSubAction(ulong time)
		{
			base.StartSubAction(time);
			
			attackStartTime = Time.time;

			Vector2 startVisualRayPosition = Map.instance.transform.InverseTransformPoint(identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position);
			Vector2 endVisualRayPosition = new Vector2(shapeBehaviour.threatenedTiles[0].localPosition.x + Map.instance.tileWidth / 2, shapeBehaviour.threatenedTiles[0].localPosition.y + Map.instance.tileHeight / 2);
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
			startProjectilePosition = startVisualRayPosition;
			endProjectilePosition = endVisualRayPosition;

			GameObject projectile = Instantiate(projectilePrefab);
			projectile.transform.parent = Map.instance.transform;
			projectile.transform.position = identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position;
			projectile.transform.position = new Vector3(projectile.transform.position.x, projectile.transform.position.y, -5);
			projectileObject = projectile;

			foreach (Tile tile in shapeBehaviour.threatenedTiles)
            {
				if (tile == shapeBehaviour.threatenedTiles[0]) continue;

				GameObject secondaryProjectile = Instantiate(projectilePrefab);
				secondaryProjectile.transform.parent = Map.instance.transform;
				secondaryProjectile.transform.position = new Vector3(projectile.transform.position.x, projectile.transform.position.y, -5);
				secondaryProjectileObjects.Add((secondaryProjectile, tile));
				secondaryProjectile.SetActive(false);
			}
		}

		override public bool ContinueSubAction(ulong time)
		{
			bool allDone = true; 
			float timeSinceAttackStart = Time.time - attackStartTime;


			if (projectileObject != null)
			{
				float t = timeSinceAttackStart / duration;

				projectileObject.transform.localPosition = (Vector3)Vector2.Lerp(startProjectilePosition, endProjectilePosition, t) + Vector3.forward * projectileObject.transform.position.z;

				if (t >= 1)
				{
					attackStartTime = Time.time;
					var tileThatWasHit = Map.instance.GetTileFromWorldPosition(projectileObject.transform.position);

					// Destroy the projectile
					Destroy(projectileObject);
					projectileObject = null;

					// Add the trap
					var fire = Instantiate(elementalEffectPrefab);
					tileThatWasHit.AddObject(fire.GetComponent<DungeonObject>());

					// Do the damage
					if (tileThatWasHit != null && tileThatWasHit.objectList != null)
					{
						DungeonObject targetObject = tileThatWasHit.objectList.FirstOrDefault(ob => ob.isCollidable);
						if (targetObject)
						{
							targetObject.TakeDamage((int)Random.Range(minDamage, maxDamage));
						}
					}
					foreach ((GameObject secondaryProjectile, Tile tile) in secondaryProjectileObjects)
					{
						secondaryProjectile.SetActive(true);
					}
					allDone = false;
				}
				else
				{
					allDone = false;
				}
			}
			else
			{
				float t = timeSinceAttackStart / .15f;
				foreach ((GameObject secondaryProjectile, Tile tile) in secondaryProjectileObjects)
                {
					if (secondaryProjectile == null) continue;

					Vector2 endSecondaryProjectilePosition = new Vector2(tile.localPosition.x + Map.instance.tileWidth / 2, tile.localPosition.y + Map.instance.tileHeight / 2);
					secondaryProjectile.transform.localPosition = (Vector3)Vector2.Lerp(endProjectilePosition, endSecondaryProjectilePosition, t) + Vector3.forward * secondaryProjectile.transform.position.z;

					if (t >= 1)
					{
						var tileThatWasHit = Map.instance.GetTileFromWorldPosition(secondaryProjectile.transform.position);

						// Destroy the projectile
						Destroy(secondaryProjectile);

						// Add the trap
						var trap = Instantiate(elementalEffectPrefab);
						tileThatWasHit.AddObject(trap.GetComponent<DungeonObject>());

						// Do the damage
						if (tileThatWasHit != null && tileThatWasHit.objectList != null)
						{
							DungeonObject targetObject = tileThatWasHit.objectList.FirstOrDefault(ob => ob.isCollidable);
							if (targetObject)
							{
								targetObject.TakeDamage((int)Random.Range(minDamage, maxDamage));
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
			if (projectileObject != null) Destroy(projectileObject);
			foreach ((GameObject secondaryProjectile, _) in secondaryProjectileObjects)
			{
				if (secondaryProjectile != null) Destroy(secondaryProjectile);
			}
			secondaryProjectileObjects.Clear();
		}
	}
}