namespace Noble.TileEngine
{
    using System.Collections;
    using UnityEngine;

	public class TickableBehaviour : MonoBehaviour
	{
		public DungeonObject owner;
		public bool executeEveryTick = false;

		virtual public void Awake()
		{
			owner = GetComponentInParent<DungeonObject>();
		}

		virtual public float GetActionConfidence() { return 0; }

		virtual public bool IsActionACoroutine() { return false; }

		virtual public void StartAction()
        {
			if (owner.tickable)
			{
				owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
			}
		}

		virtual public IEnumerator StartActionCoroutine() 
		{
			if (owner.tickable)
			{
				owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
			}
			yield return null;
		}
		
		virtual public void StartSubAction(ulong time) { }
		virtual public bool ContinueSubAction(ulong time) { return true; }
		virtual public void FinishSubAction(ulong time) { }

		virtual public void FinishAction() { }
	}
}