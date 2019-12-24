using System.Collections;
using UnityEngine;

[CreateAssetMenu]
public class BiomeType : ScriptableObject
{
    public SpawnRate[] creatures;
    public BiomeDropRate[] items;

    virtual public void PreProcessMap(Map map, RectInt area)
    {
    }
}
