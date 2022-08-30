namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public class PowerBehaviour : TickableBehaviour
    {
        public TargetableBehaviour shapeBehaviour;
        public float minDamage;
        public float maxDamage;

        public override void Awake()
        {
            base.Awake();
        }
    }
}
