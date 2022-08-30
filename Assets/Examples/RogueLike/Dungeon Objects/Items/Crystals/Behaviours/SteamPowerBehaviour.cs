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
		List<GameObject> projectileObjects = new List<GameObject>();

		protected float attackStartTime = 0;
		protected Creature identityCreature => owner.gameObject.GetComponentInParent<Creature>(true);

		public float unitsPerSecond = 10;

		List<Vector2> startProjectilePositions = new List<Vector2>();
		List<Vector2> endProjectilePositions = new List<Vector2>();
		List<float> durations = new List<float>();

		override public void Awake()
		{
			base.Awake();
		}

		override public void StartSubAction(ulong time)
		{
			base.StartSubAction(time);
			
			attackStartTime = Time.time;

			foreach (var tile in shapeBehaviour.threatenedTiles)
			{
				Vector2 startVisualRayPosition = Map.instance.transform.InverseTransformPoint(identityCreature.leftHand.transform.position);
				Vector2 endVisualRayPosition = new Vector2(tile.transform.localPosition.x + Map.instance.tileWidth / 2, tile.transform.localPosition.y + Map.instance.tileHeight / 2);
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

				startProjectilePositions.Add(startVisualRayPosition);
				endProjectilePositions.Add(endVisualRayPosition);
				durations.Add(duration);

				GameObject projectile = Instantiate(projectilePrefab);
				projectile.transform.parent = Map.instance.transform;
				projectile.transform.position = identityCreature.leftHand.transform.position - Vector3.forward;
				projectileObjects.Add(projectile);
			}
		}

		override public bool ContinueSubAction(ulong time)
		{
			bool allDone = true; 
			float timeSinceAttackStart = Time.time - attackStartTime;

			for (int i = 0; i < projectileObjects.Count; i++)
			{
				float t = timeSinceAttackStart / durations[i];

				if (projectileObjects[i] != null)
				{
					projectileObjects[i].transform.localPosition = (Vector3)Vector2.Lerp(startProjectilePositions[i], endProjectilePositions[i], t) - Vector3.forward;

					if (t >= 1)
					{
						var tileThatWasHit = Map.instance.GetTileFromWorldPosition(projectileObjects[i].transform.localPosition);

						// Destroy the fireball
						Destroy(projectileObjects[i]);
						projectileObjects[i] = null;

						// Add the fire
						var fire = Instantiate(elementalEffectPrefab);
						tileThatWasHit.AddObject(fire.GetComponent<DungeonObject>());

						// Do the damage
						if (tileThatWasHit && tileThatWasHit.objectList != null)
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
			foreach (var fireball in projectileObjects)
			{
				if (fireball != null) Destroy(fireball);
			}
			projectileObjects.Clear();
			startProjectilePositions.Clear();
			endProjectilePositions.Clear();
			durations.Clear();
		}
	}
}