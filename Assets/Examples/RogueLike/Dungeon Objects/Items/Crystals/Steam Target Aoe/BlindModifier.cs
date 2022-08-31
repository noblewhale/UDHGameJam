namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;

    public class BlindModifier : Modifier, ViewDistanceModifier
    {
        public void ModifyViewDistance(ref float viewDistance)
        {
            viewDistance = 0;
        }
    }

}