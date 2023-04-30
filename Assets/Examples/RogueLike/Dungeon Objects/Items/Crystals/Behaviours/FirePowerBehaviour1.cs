namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class FirePowerBehaviour : PowerBehaviour
    {
		public GameObject projectilePrefab;
		public GameObject elementalEffectPrefab;
		GameObject projectileObject;
		List<(GameObject, Tile)> secondaryProjectileObjects = new List<(GameObject, Tile)>();

		protected float attackStartTime = 0;
		protected Creature identityCreature => owner.gameObject.GetComponentInParent<Creature>(true);

		public float unitsPerSecond = 10;

		public float knockbackForce = 150;

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

			Vector2 startVisualRayPosition = identityCreature.GetEquipmentSlot(Equipment.Slot.LEFT_HAND_WEAPON).position;
			Vector2 endVisualRayPosition = new Vector2(shapeBehaviour.threatenedTiles[0].position.x + Map.instance.tileWidth / 2, shapeBehaviour.threatenedTiles[0].position.y + Map.instance.tileHeight / 2);
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

				projectileObject.transform.position = (Vector3)Vector2.Lerp(startProjectilePosition, endProjectilePosition, t) + Vector3.forward * projectileObject.transform.position.z;

				if (t >= 1)
				{
					attackStartTime = Time.time;
					var tileThatWasHit = Map.instance.GetTileFromWorldPosition(projectileObject.transform.position);

					// Destroy the projectile
					Destroy(projectileObject);
					projectileObject = null;

					DoTheThing(tileThatWasHit);
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

					Vector2 endSecondaryProjectilePosition = new Vector2(tile.position.x + Map.instance.tileWidth / 2, tile.position.y + Map.instance.tileHeight / 2);
					secondaryProjectile.transform.position = (Vector3)Vector2.Lerp(endProjectilePosition, endSecondaryProjectilePosition, t) + Vector3.forward * secondaryProjectile.transform.position.z;

					if (t >= 1)
					{
						var tileThatWasHit = Map.instance.GetTileFromWorldPosition(secondaryProjectile.transform.position);

						// Destroy the projectile
						Destroy(secondaryProjectile);

						DoTheThing(tileThatWasHit);
					}
					else
					{
						allDone = false;
					}
				}
            }

			return allDone;
		}

		void DoTheThing(Tile tileThatWasHit)
		{
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

				var movableObjects = tileThatWasHit.objectList.Where(x => x.weight < knockbackForce).ToList();
                foreach (var ob in movableObjects)
                {
                    if (!ob.isCollidable) continue;

                    var dir = tileThatWasHit.tilePosition - owner.GetComponent<Equipable>().EquippedBy.tilePosition;
                    dir.x = Mathf.Clamp(dir.x, -1, 1);
                    dir.y = Mathf.Clamp(dir.y, -1, 1);
                    var newTilePosition = tileThatWasHit.tilePosition + dir;
					var newTile = Map.instance.GetTile(newTilePosition);
                    Debug.Log(tileThatWasHit.tilePosition + " => " + newTilePosition);
                    if (newTile.ContainsCollidableObject())
                    {
						// Can't move, do additional damage
						targetObject.TakeDamage((int)Random.Range(minDamage, maxDamage));
                        DungeonObject collidedObject = newTile.objectList.FirstOrDefault(ob => ob.isCollidable);
						collidedObject.TakeDamage((int)Random.Range(minDamage, maxDamage));
                    }
                    else
                    {
                        // Ok, move
                        Map.instance.MoveObject(ob, newTilePosition);
                    }
                }
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