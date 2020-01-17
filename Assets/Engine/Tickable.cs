using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(DungeonObject))]
public class Tickable : MonoBehaviour 
{
	public ulong nextActionTime = 0;

    public List<TickableBehaviour> behaviours = new List<TickableBehaviour>();
    public DungeonObject owner;
    TickableBehaviour currentBehaviour;
    public bool markedForRemoval = false;

    void Awake()
    {
        behaviours = GetComponents<TickableBehaviour>().ToList();
        owner = GetComponent<DungeonObject>();
        owner.onDeath += OnDeath;
    }

    void Start()
	{
		TimeManager.instance.tickableObjects.Add(this);
	}

	void OnDestroy()
	{
        owner.onDeath -= OnDeath;
        markedForRemoval = true;
	}

    void OnDeath()
    {
        markedForRemoval = true;
    }

    public T AddBehaviour<T>() where T : TickableBehaviour
    {
        var behaviour = gameObject.AddComponent<T>();
        behaviours.Add(behaviour);
        return behaviour;
    }

    public bool StartNewAction()
    {
        var confidences = behaviours.Where(x => x.enabled).Select(x => x.GetActionConfidence());
        float totalConfidence = confidences.Sum();
        float aRandomNumber = UnityEngine.Random.Range(0, totalConfidence);

        float currentConfidence = 0;
        float previousConfidence = 0;
        for (int i = 0; i < confidences.Count(); i++)
        {
            previousConfidence = currentConfidence;
            currentConfidence += confidences.ElementAt(i);
            if (aRandomNumber >= previousConfidence && aRandomNumber < currentConfidence)
            {
                currentBehaviour = behaviours[i];
                return behaviours[i].StartAction();
            }
        }

        return true;
    }
    
    public void FinishAction()
    {
        if (currentBehaviour) currentBehaviour.FinishAction();
    }

    virtual public bool ContinueAction()
    {
        if (currentBehaviour) return currentBehaviour.ContinueAction();
        return true;
    }
}
