using UnityEngine;

public static class PolarMapUtil
{
    public static Vector2 GetPositionRelativeToMap(Vector3 screenPos)
    {
        Vector2 mousePosRelativeToMapRenderer = ((Vector2)Camera.main.ScreenToWorldPoint(screenPos) - (Vector2)Camera.main.transform.position) - (Vector2)MapRenderer.instance.transform.localPosition;
        mousePosRelativeToMapRenderer /= MapRenderer.instance.transform.localScale;
        Vector2 rotated = new Vector2();
        float rotation = MapRenderer.instance.material.GetFloat("_Rotation");
        rotated.x = mousePosRelativeToMapRenderer.x * Mathf.Sin(rotation) - mousePosRelativeToMapRenderer.y * Mathf.Cos(rotation);
        rotated.y = mousePosRelativeToMapRenderer.x * Mathf.Cos(rotation) + mousePosRelativeToMapRenderer.y * Mathf.Sin(rotation);
        return rotated;
    }

    public static bool UnwarpPosition(Vector2 warpedPos, out Vector2 unwarpedPos)
    {
        unwarpedPos = new Vector2();

        float d = warpedPos.magnitude / .5f;
        float _SeaLevel = MapRenderer.instance.material.GetFloat("_SeaLevel");
        if (d < .1f)
        {
            return false;
        }
        else
        {
            d = (d - .1f) / .9f;
            d = Mathf.Log(1 + d / _SeaLevel) / Mathf.Log(1 + 1 / _SeaLevel);
            unwarpedPos.y = 1 - d;
            Vector2 normalized = warpedPos.normalized;
            float angle = Mathf.Acos(Vector2.Dot(normalized, Vector2.up));
            Vector3 check = Vector3.Cross(normalized, Vector3.up);
            if (check.z < 0) angle = 2 * Mathf.PI - angle;
            unwarpedPos.x = angle / (2 * Mathf.PI);
            unwarpedPos = unwarpedPos - Vector2.one * .5f;
            unwarpedPos.x *= -1;

            unwarpedPos = unwarpedPos * new Vector2(PlayerCamera.instance.camera.orthographicSize * 2 * PlayerCamera.instance.camera.aspect, PlayerCamera.instance.camera.orthographicSize * 2);
            unwarpedPos += (Vector2)PlayerCamera.instance.camera.transform.position;

            return true;
        }
    }

    public static bool PositionToTile(Vector2 pos, out int x, out int y)
    {
        pos.x += Map.instance.TotalWidth / 2;
        pos.y += Map.instance.TotalHeight / 2;
        if (pos.x > 0 && pos.x < Map.instance.TotalWidth && pos.y > 0 && pos.y < Map.instance.TotalHeight)
        {
            x = (int)(pos.x / Map.instance.tileWidth);
            y = (int)(pos.y / Map.instance.tileHeight);
            return true;
        }
        else
        {
            x = 0;
            y = 0;
            return false;
        }
    }
}
