namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class Door : DungeonObject
    {

        public bool isOpen = false;
        public float openDifficulty = 1;
        public bool isLocked = false;
        public float lockPickDifficulty = 1;

        // Use this for initialization
        void Start()
        {
            SetOpen(isOpen);
            if (!isOpen)
            {
                SetLocked(isLocked);
            }
            base.onCollision += OnCollision;
        }

        // Update is called once per frame
        void SetOpen(bool isOpen)
        {
            bool wasOpen = this.isOpen;
            this.isOpen = isOpen;
            //this.isCollidable = !isOpen;

            if (isOpen && !wasOpen)
            {
                TimeManager.instance.ForceNextAction(tickable);
                tickable.nextBehaviour = GetComponent<DoorOpenBehaviour>();
            }
        }

        public void SetLocked(bool isLocked)
        {
            this.isLocked = isLocked;
            if (isLocked)
            {
                SetOpen(false);
            }
        }

        public void OnCollision(DungeonObject ob, bool isInstigator)
        {
            if (isInstigator) return;

            if (isLocked)
            {
                if (ob.GetComponent<Creature>())
                {
                    DungeonObject key;
                    bool hasKey = ob.inventory.items.TryGetValue("Key", out key);
                    if (hasKey)
                    {
                        key.quantity--;
                        if (key.quantity == 0)
                        {
                            ob.inventory.items.Remove("Key");
                        }

                        SetOpen(true);
                    }
                }
            }
            else
            {
                if (Random.value * 2 > openDifficulty)
                {
                    SetOpen(true);
                }
            }
        }
    }
}