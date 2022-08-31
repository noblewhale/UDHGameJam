namespace Noble.TileEngine
{
    using System;

    public class Modifier : TickableBehaviour
    {
        public ulong duration = 0;
        public ulong timeAdded = 0;

        public override void Awake()
        {
            base.Awake();
            executeEveryTick = true;
        }

        public void Start()
        {
            timeAdded = TimeManager.instance.Time;
        }

        public override void StartSubAction(ulong time)
        {
            base.StartSubAction(time);
            if (TimeManager.instance.Time - timeAdded >= duration)
            {
                owner.GetComponent<Creature>().RemoveModifier(this);
            }
        }
    }
}
