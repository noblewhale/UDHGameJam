using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BiomeType : ScriptableObject
{
    public float nothingProbability = 0;
    public BiomeDropRate[] objects;

    public int minX, maxX;
    public int minY, maxY;
    public int minWidth, maxWidth;
    public int minHeight, maxHeight;

    virtual public IEnumerator PreProcessMap(Map map, Biome biome)
    {
        yield break;
    }

    virtual public void DrawDebug(RectIntExclusive area)
    {
        EditorUtil.DrawRect(Map.instance, area, Color.green);
    }
}
