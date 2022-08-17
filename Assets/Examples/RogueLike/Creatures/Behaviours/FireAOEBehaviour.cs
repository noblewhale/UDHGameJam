namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    public class FireAOEBehaviour : TickableBehaviour
    {
		Tile targetTile;
		Creature identityCreature;

		public GameObject fireballObjectPrefab;
		public GameObject firePrefab;
		GameObject fireballObject;
		float attackStartTime = 0;

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

			Map.instance.AddOutline(Player.instance.identity.x, Player.instance.identity.y, 2);

			if (!Cursor.visible)
			{
				Vector3 warpedPos = PolarMapUtil.WarpPosition(Player.instance.identity.tile.transform.position + Vector3.one * .25f);
				warpedPos *= (Vector2)MapRenderer.instance.transform.lossyScale;
				warpedPos += MapRenderer.instance.transform.position;
				Debug.Log(warpedPos);
				Vector2 screenPos = Camera.main.WorldToScreenPoint(warpedPos);

				Mouse.current.WarpCursorPosition(screenPos);

				Cursor.visible = true;
			}

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
			float unitsPerSecond = 5;
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
			if (targetTile && targetTile.objectList != null)
			{
				DungeonObject targetObject = targetTile.objectList.SingleOrDefault(ob => ob.isCollidable);
				if (targetObject)
				{
					targetObject.TakeDamage(10);
				}
			}
			var fire = Instantiate(firePrefab);
			targetTile.AddObject(fire.GetComponent<DungeonObject>());
			Map.instance.UpdateLighting();
			Map.instance.Reveal(Player.instance.identity.x, Player.instance.identity.y, Player.instance.identity.viewDistance);
			
			Destroy(fireballObject);
		}
	}
}
