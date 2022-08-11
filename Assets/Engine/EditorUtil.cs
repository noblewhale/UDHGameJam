namespace Noble.TileEngine
{
    using UnityEngine;

    public static class EditorUtil
    {
        public static void DrawRect(Map map, RectIntExclusive area, Color color)
        {
            DrawRect(map, area, color, Vector2.zero);
        }

        public static void DrawRect(Map map, RectIntExclusive area, Color color, Vector2 offset)
        {
            Vector2 lowerLeft = new Vector2(area.xMin + offset.x, area.yMin + offset.y);
            Vector2 lowerRight = new Vector2(area.xMax + 1 - offset.x, area.yMin + offset.y);
            Vector2 upperLeft = new Vector2(area.xMin + offset.x, area.yMax + 1 - offset.y);
            Vector2 upperRight = new Vector2(area.xMax + 1 - offset.x, area.yMax + 1 - offset.y);

            lowerLeft.x *= map.tileWidth;
            lowerLeft.y *= map.tileHeight;
            lowerRight.x *= map.tileWidth;
            lowerRight.y *= map.tileHeight;
            upperLeft.x *= map.tileWidth;
            upperLeft.y *= map.tileHeight;
            upperRight.x *= map.tileWidth;
            upperRight.y *= map.tileHeight;

            lowerLeft += (Vector2)map.transform.position;
            lowerRight += (Vector2)map.transform.position;
            upperLeft += (Vector2)map.transform.position;
            upperRight += (Vector2)map.transform.position;

            Debug.DrawLine(lowerLeft, lowerRight, color);
            Debug.DrawLine(lowerRight, upperRight, color);
            Debug.DrawLine(upperRight, upperLeft, color);
            Debug.DrawLine(upperLeft, lowerLeft, color);
        }
    }
}