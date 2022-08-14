namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;

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

			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.red;
			bool isDone = false;
			while (!isDone)
			{
				while (!PlayerInput.instance.HasInput) yield return new WaitForEndOfFrame();
				Command nextCommand = PlayerInput.instance.commandQueue.Peek();
				HighlightTile.instance.Move(nextCommand);
				if (nextCommand.key == KeyCode.Space || nextCommand.key == KeyCode.Mouse0)
				{
					isDone = true;
				}
				PlayerInput.instance.commandQueue.Dequeue();
				yield return new WaitForEndOfFrame();
			}
			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.white;

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
			float duration = (startPosition - endPosition).magnitude / unitsPerSecond;
			float timeSinceAttackStart = Time.time - attackStartTime;
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
