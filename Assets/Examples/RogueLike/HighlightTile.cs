namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class HighlightTile : MonoBehaviour
    {
        public GameObject highlightPrefab;
        GameObject highLightObject;

        void Start()
        {
            highLightObject = Instantiate(highlightPrefab);
        }

        void Update()
        {
            Vector2 relativeWorldPos = PolarMapUtil.GetPositionRelativeToMap(Input.mousePosition);
            Vector2 unwarpedPos;
            bool success = PolarMapUtil.UnwarpPosition(relativeWorldPos, out unwarpedPos);
            if (success)
            {
                bool isInsideMap = PolarMapUtil.PositionToTile(unwarpedPos, out int tileX, out int tileY);
                if (isInsideMap)
                {
                    highLightObject.transform.position = Map.instance.tileObjects[tileY][tileX].transform.position;
                }
            }
        }
    }
}