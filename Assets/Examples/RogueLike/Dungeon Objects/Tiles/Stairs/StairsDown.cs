namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class StairsDown : MonoBehaviour
    {
        public void OnSteppedOn(Creature creature)
        {
            if (Player.Identity == creature)
            {
                Map.instance.StartCoroutine(Map.instance.RegenerateMap());
            }
        }
    }
}
