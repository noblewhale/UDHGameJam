namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class WallLighting : MonoBehaviour
    {
        DungeonObject owner;
        // Start is called before the first frame update
        void Start()
        {
            owner = GetComponent<DungeonObject>();
        }

        // Update is called once per frame
        void Update()
        {
            //var map = Map.instance;

            //if (!map.isDoneGeneratingMap) return;

            //bool shouldLight = false;
            //bool shouldStayLit = false;

            //for (int x = owner.x - 1; x <= owner.x + 1; x++)
            //{
            //    for (int y = owner.y - 1; y <= owner.y + 1; y++)
            //    {
            //        if (x == owner.x && y == owner.y) continue;
            //        if (y == 0 || y >= map.height - 1) continue;

            //        int wrappedX = map.WrapX(x);
            //        var neighbor = map.tileObjects[y][wrappedX];
            //        bool hasFloor = neighbor.ContainsObjectOfType("Floor");
            //        bool hasWall = neighbor.ContainsObjectOfType("Wall");
            //        if (neighbor.isInView && hasFloor && !hasWall && neighbor.isLit)
            //        {
            //            if (neighbor.isAlwaysLit) shouldStayLit = true;
            //            shouldLight = true;
            //            break;
            //        }
            //    }
            //}

            //if (shouldLight)
            //{
            //    owner.tile.SetInView(true);
            //    if (shouldStayLit) owner.isAlwaysLit = true;
            //    owner.tile.SetLit(true);
            //}
        }
    }
}