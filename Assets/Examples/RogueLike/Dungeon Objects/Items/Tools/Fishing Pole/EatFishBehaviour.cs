namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;

    public class EatFishBehaviour : TickableBehaviour
    {
        public List<AdditiveIntModifier> buffs = new();

        public override void StartAction()
        {
            base.StartAction();

            // Apply modifier to whatever object has the fish meat in its inventory
            var fishMeat = owner;
            var eater = fishMeat.container;
            if (eater != null)
            {
                foreach (var buff in buffs)
                {
                    var prop = eater.GetProperty<int>(buff.propertyName);
                    prop.AddModifier(buff);
                }
            }
        }
    }
}