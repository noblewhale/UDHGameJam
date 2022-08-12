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
        TickableBehaviour currentBehaviour;
        public bool markedForRemoval = false;

        public TickableBehaviour nextBehaviour;

        void Awake()
        {
            behaviours = GetComponents<TickableBehaviour>().ToList();
            owner = GetComponent<DungeonObject>();
            owner.onDeath += OnDeath;
        }

        void Start()
        {
            listNode = TimeManager.instance.tickableObjects.AddLast(this);
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

        public TickableBehaviour GetBehaviourToExecute()
        {
            currentBehaviour = null;
            nextActionTime = TimeManager.instance.Time + 1;

            if (nextBehaviour == null)
            {
                currentBehaviour = DetermineBehaviour();
            }
            else
            {
                currentBehaviour = nextBehaviour;
                nextBehaviour = null;
            }

            lastActionTime = TimeManager.instance.Time;
            return currentBehaviour;
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

        virtual public void StartAction()
        {
            if (currentBehaviour) currentBehaviour.StartAction();
        }

        virtual public IEnumerator StartActionCoroutine()
        {
            if (currentBehaviour) yield return currentBehaviour.StartActionCoroutine();
        }

        virtual public void StartSubAction()
        {
            if (currentBehaviour) currentBehaviour.StartSubAction(TimeManager.instance.Time - lastActionTime);
        }

        virtual public bool ContinueSubAction()
        {
            if (currentBehaviour) return currentBehaviour.ContinueSubAction(TimeManager.instance.Time - lastActionTime);
            return true;
        }
        virtual public void FinishSubAction()
        {
            if (currentBehaviour) currentBehaviour.FinishSubAction(TimeManager.instance.Time - lastActionTime);
        }

        virtual public void FinishAction()
        {
            if (currentBehaviour) currentBehaviour.FinishAction();
        }
    }
}