using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tickable : MonoBehaviour 
{
	public ulong nextActionTime = 0;

    public MovementBehaviour movementBehaviour;
    public AttackBehaviour attackBehaviour;

    void Awake()
    {
        movementBehaviour = GetComponent<MovementBehaviour>();
        attackBehaviour = GetComponent<AttackBehaviour>();
    }

    void OnEnable()
	{
		TimeManager.instance.tickableObjects.Add(this);
	}

	void OnDestroy()
	{
		TimeManager.instance.tickableObjects.Remove(this);
	}


    virtual public void StartNewAction()
    {
        float shouldMoveConfidence = 0;
        float shouldAttackConfidence = 0;
        if (movementBehaviour)
        {
            shouldMoveConfidence = movementBehaviour.ShouldMove();
        }
        if (attackBehaviour)
        {
            shouldAttackConfidence = attackBehaviour.ShouldAttack();
        }

        if (shouldMoveConfidence == 0 && shouldAttackConfidence == 0) return;

        float totalConfidence = shouldMoveConfidence + shouldAttackConfidence;
        float random = UnityEngine.Random.Range(0, totalConfidence);

        if (random < shouldMoveConfidence)
        {
            movementBehaviour.Move();
        }
        else
        {
            attackBehaviour.Attack();
        }
    }

    virtual public void ContinueAction()
    {
    }
}
