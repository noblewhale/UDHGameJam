namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;

    [Serializable]
    public class AdditiveIntModifier : PropertyModifier<int>
    {
        public int valueToAdd = 10;
        public override int Modify(int input)
        {
            return input + valueToAdd;
        }
    }
}
