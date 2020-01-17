using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorUtil
{
    public static void DrawRect(Map map, RectIntExclusive area, Color color)
    {
        Vector2 lowerLeft = new Vector2(area.xMin, area.yMin);
        Vector2 lowerRight = new Vector2(area.xMax + 1, area.yMin);
        Vector2 upperLeft = new Vector2(area.xMin, area.yMax + 1);
        Vector2 upperRight = new Vector2(area.xMax + 1, area.yMax + 1);

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
