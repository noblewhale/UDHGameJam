namespace Noble.TileEngine
{
	using System.Linq;

    public class MoveBehaviour : TickableBehaviour
    {
		public int targetX;
		public int targetY;

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
			owner.map.TryMoveObject(owner, targetX, targetY);
			if (owner.tile.objectList.Any(x => x.canBePickedUp))
			{
				owner.PickUpAll();
			}
		}
	}
}
