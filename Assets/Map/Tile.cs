using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int value;
    Map map;
    public TextMesh glyph;
    public bool isFloodFilled;
    bool isVisible;

    public void Init(Map map, int value, int x, int y)
    {
        transform.parent = map.transform;
        glyph = GetComponentInChildren<TextMesh>();
        this.value = value;
        this.map = map;
        this.x = x;
        this.y = y;

        SetVisible(false);
    }

    public void SetVisible(bool isVisible)
    {
        this.isVisible = isVisible;
        glyph.gameObject.SetActive(isVisible);
    }

    public void Update()
    {
        transform.localPosition = new Vector3(x * map.tileWidth, y * map.tileHeight, 0);
    }
}
