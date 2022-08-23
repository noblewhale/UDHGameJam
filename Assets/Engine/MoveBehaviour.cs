namespace Noble.TileEngine
{
	using System.Linq;
    using UnityEngine;

    public class MoveBehaviour : TickableBehaviour
    {
		public Vector2Int targetTilePosition;

		Creature identityCreature;

        override public void Awake()
        {
			base.Awake();
			identityCreature = owner.GetComponent<Creature>();
		}

        override public void StartAction()
		{
			owner.tickable.nextActionTime = TimeManager.instance.Time + identityCreature.ticksPerMove;
		}

		override public void FinishSubAction(ulong time) 
		{
			owner.map.TryMoveObject(owner, targetTilePosition);
			if (owner.tile.objectList.Any(x => x.canBePickedUp))
			{
				owner.PickUpAll();
			}
		}
	}
}
