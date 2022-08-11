namespace Noble.TileEngine
{
	using UnityEngine;

	public class TickableBehaviour : MonoBehaviour
	{
		protected DungeonObject owner;

		virtual public void Awake()
		{
			owner = GetComponent<DungeonObject>();
		}

		virtual public float GetActionConfidence() { return 0; }

		virtual public bool StartAction(out ulong duration) { duration = 1; return true; }
		
		virtual public void StartSubAction(ulong time) { }
		virtual public bool ContinueSubAction(ulong time) { return true; }
		virtual public void FinishSubAction(ulong time) { }

		virtual public void FinishAction() { }
	}
}