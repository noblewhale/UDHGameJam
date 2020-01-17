using System.Collections;
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

    virtual public void PreProcessMap(Map map, RectInt area)
    {
    }
}
