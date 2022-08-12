namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;

    public class FireAOEBehaviour : TickableBehaviour
    {
		DungeonObject targetObject;
		Creature identityCreature;

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

			targetObject = null;
			foreach (var dOb in HighlightTile.instance.tile.objectList)
			{
				if (dOb.isCollidable)
				{
					targetObject = dOb;
				}
			}
		}

		override public void StartSubAction(ulong time) 
		{
			identityCreature.StartAttack(targetObject);
		}
		override public bool ContinueSubAction(ulong time) 
		{
			return identityCreature.ContinueAttack(targetObject);
		}
		override public void FinishSubAction(ulong time) 
		{
			identityCreature.FinishAttack(targetObject);
		}
	}
}
