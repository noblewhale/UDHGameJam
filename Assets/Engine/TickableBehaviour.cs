using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickableBehaviour : MonoBehaviour 
{
	protected DungeonObject owner;

	virtual public void Awake()
	{
		owner = GetComponent<DungeonObject>();
	}

	virtual public float GetActionConfidence() { return 0; }
	virtual public bool StartAction() { return true; }
	virtual public bool ContinueAction() { return true; }
	virtual public void FinishAction() { }
}
