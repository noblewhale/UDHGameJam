namespace Noble.TileEngine
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Linq;

    [RequireComponent(typeof(DungeonObject))]
    public class Tickable : MonoBehaviour
    {
        public ulong nextActionTime = 0;
        public ulong lastActionTime = 0;
        public LinkedListNode<Tickable> listNode;
        public List<TickableBehaviour> behaviours = new List<TickableBehaviour>();
        public DungeonObject owner;
        public List<TickableBehaviour> currentBehaviours = new List<TickableBehaviour>();
        public bool markedForRemoval = false;

        public TickableBehaviour nextBehaviour;

        virtual public void Awake()
        {
            behaviours = GetComponents<TickableBehaviour>().ToList();
            owner = GetComponent<DungeonObject>();
            owner.onDeath += OnDeath;
        }

        virtual public void Start()
        {
            listNode = TimeManager.instance.tickableObjects.AddLast(this);
        }

        virtual public void OnDestroy()
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

        public List<TickableBehaviour> GetBehavioursToExecute()
        {
            currentBehaviours.Clear();
            nextActionTime = TimeManager.instance.Time + 1;

            if (nextBehaviour == null)
            {
                currentBehaviours.Add(DetermineBehaviour());
            }
            else
            {
                currentBehaviours.Add(nextBehaviour);
                nextBehaviour = null;
            }

            currentBehaviours.AddRange(behaviours.Where(x => x.enabled && x.executeEveryTick));

            lastActionTime = TimeManager.instance.Time;
            return currentBehaviours;
        }

        virtual public TickableBehaviour DetermineBehaviour()
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
                    return behaviours[i];
                }
            }

            return null;
        }
    }
}