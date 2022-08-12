
namespace Noble.TileEngine
{
    public class AttackBehaviour : TickableBehaviour
    {
		public Tile targetTile;
		Creature identityCreature;
		DungeonObject targetObject;

        override public void Awake()
        {
			base.Awake();
			identityCreature = owner.GetComponent<Creature>();
		}

		override public void StartAction()
		{
			owner.tickable.nextActionTime = identityCreature.ticksPerAttack;
			foreach (var dOb in targetTile.objectList)
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
			Map.instance.TryMoveObject(owner, targetTile.x, targetTile.y);
			identityCreature.FinishAttack(targetObject);
		}
	}
}
