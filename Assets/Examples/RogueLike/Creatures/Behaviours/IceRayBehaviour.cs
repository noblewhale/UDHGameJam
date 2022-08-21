namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using System;
    using System.Collections.Generic;

    public class IceRayBehaviour : TickableBehaviour
    {
		Tile targetTile;
		Creature identityCreature;

		public GameObject fireballObjectPrefab;
		public GameObject firePrefab;
		GameObject fireballObject;
		float attackStartTime = 0;

		public float radius = 3;

        override public void Awake()
        {
			base.Awake();
			identityCreature = owner.GetComponent<Creature>();
		}

        public override bool IsActionACoroutine()
        {
			return true;
        }

        override public IEnumerator StartActionCoroutine()
		{
			owner.tickable.nextActionTime = identityCreature.ticksPerAttack;

			fireballObject = Instantiate(fireballObjectPrefab);
			fireballObject.transform.position = identityCreature.leftHand.transform.position - Vector3.forward;

			CameraTarget.instance.owner = HighlightTile.instance;
			CameraTarget.instance.thresholdX = 6;
			CameraTarget.instance.thresholdY = 4;
			AimOverlay.instance.gameObject.SetActive(true);

			var allowedTiles = Map.instance.GetTilesInRadiusStraightLines(
				Player.instance.identity.x, Player.instance.identity.y,
				radius
			);

			Map.instance.AddOutline(allowedTiles);

			HighlightTile.instance.tile?.RemoveObject(HighlightTile.instance);
			Map.instance.tileObjects[Player.instance.identity.y][Player.instance.identity.x].AddObject(HighlightTile.instance);

			HighlightTile.instance.allowedTiles = allowedTiles;

			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].gameObject.SetActive(true);
			HighlightTile.instance.isKeyboardControlled = true;
			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.red;
			bool isDone = false;
			while (!isDone)
			{
				while (!PlayerInputHandler.instance.HasInput) yield return new WaitForEndOfFrame();
				Command nextCommand = PlayerInputHandler.instance.commandQueue.Peek();
				HighlightTile.instance.Move(nextCommand);
				if (nextCommand.key == Key.Space || nextCommand.mouseButton == Mouse.current.leftButton)
				{
					isDone = true;
				}
				PlayerInputHandler.instance.commandQueue.Dequeue();
				yield return new WaitForEndOfFrame();
			}
			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.white;
			HighlightTile.instance.isKeyboardControlled = false;

			HighlightTile.instance.allowedTiles = null;

			CameraTarget.instance.owner = Player.instance.identity;
			CameraTarget.instance.thresholdX = 0;
			CameraTarget.instance.thresholdY = 0;
			AimOverlay.instance.gameObject.SetActive(false);

			Map.instance.RemoveOutline();

			targetTile = HighlightTile.instance.tile;
		}

		override public void StartSubAction(ulong time) 
		{
			attackStartTime = Time.time;
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
			float dirX = Map.instance.GetXDifference(owner.x, targetTile.x);
			float dirY = targetTile.y - owner.y;
			Vector2 direction = new Vector2(dirX, dirY);
			float distance = direction.magnitude;
			direction.Normalize();
			var area = new RectIntExclusive();
			area.SetMinMax(Math.Min(owner.x, targetTile.x), Math.Max(owner.x, targetTile.x) + 1, Math.Min(owner.y, targetTile.y), Math.Max(owner.y, targetTile.y) + 1);

			var tilesInRay = new List<Tile>();
			lock (Map.instance.isDirtyLock)
			{
				Map.instance.ForEachTile((t) => t.isDirty = false);
				tilesInRay = Map.instance.GetTilesInRay(
					new Vector2(owner.x + .5f, owner.y + .5f),
					direction,
					distance
				);
			}

			foreach (Tile t in tilesInRay)
			{
				if (t == owner.tile) continue;

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
