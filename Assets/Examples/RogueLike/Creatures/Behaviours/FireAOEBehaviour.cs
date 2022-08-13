namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;

    public class FireAOEBehaviour : TickableBehaviour
    {
		Tile targetTile;
		Creature identityCreature;

		public GameObject fireballObjectPrefab;
		public GameObject firePrefab;
		GameObject fireballObject;
		float animationStartTime = 0;

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

			HighlightTile.instance.enableKeyboardControl = true;
			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.red;
			bool isDone = false;
			while (!isDone)
			{
				while (!PlayerInput.instance.HasInput) yield return new WaitForEndOfFrame();
				Command nextCommand = PlayerInput.instance.commandQueue.Peek();
				if (nextCommand.key == KeyCode.Space || nextCommand.key == KeyCode.Mouse0)
				{
					PlayerInput.instance.commandQueue.Dequeue();
					isDone = true;
				}
				yield return new WaitForEndOfFrame();
			}
			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.white;
			HighlightTile.instance.enableKeyboardControl = true;

			targetTile = HighlightTile.instance.tile;
		}

		override public void StartSubAction(ulong time) 
		{
			fireballObject = Instantiate(fireballObjectPrefab);
			fireballObject.transform.position = identityCreature.transform.position - Vector3.forward;
			animationStartTime = Time.time;
		}
		override public bool ContinueSubAction(ulong time) 
		{
			float duration = ((Vector2)identityCreature.transform.position - (Vector2)targetTile.transform.position).magnitude / 5;
			fireballObject.transform.position = (Vector3)Vector2.Lerp(identityCreature.transform.position, targetTile.transform.position, (Time.time - animationStartTime) / duration) - Vector3.forward;
			return Time.time - animationStartTime > duration;
		}
		override public void FinishSubAction(ulong time)
		{
			if (targetTile && targetTile.objectList != null)
			{
				DungeonObject targetObject = null;
				foreach (var dOb in targetTile.objectList)
				{
					if (dOb.isCollidable)
					{
						targetObject = dOb;
					}
				}
				if (targetObject)
				{
					targetObject.TakeDamage(10);
				}
			}
			var fire = Instantiate(firePrefab);
			targetTile.AddObject(fire.GetComponent<DungeonObject>());
			Destroy(fireballObject);
		}
	}
}
