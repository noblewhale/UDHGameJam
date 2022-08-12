namespace Noble.TileEngine
{
    using System.Collections;
    using UnityEngine;

	public class TickableBehaviour : MonoBehaviour
	{
		protected DungeonObject owner;

		virtual public void Awake()
		{
			owner = GetComponent<DungeonObject>();
		}

		virtual public float GetActionConfidence() { return 0; }

		virtual public bool IsActionACoroutine() { return false; }

		virtual public void StartAction()
        {
			owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
		}

		virtual public IEnumerator StartActionCoroutine() 
		{ 
			owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
			yield return null;
		}
		
		virtual public void StartSubAction(ulong time) { }
		virtual public bool ContinueSubAction(ulong time) { return true; }
		virtual public void FinishSubAction(ulong time) { }

		virtual public void FinishAction() { }
	}
}